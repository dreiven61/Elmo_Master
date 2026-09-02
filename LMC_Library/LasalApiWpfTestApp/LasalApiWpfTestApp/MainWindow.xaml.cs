using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow : Window
    {
        private const int MinimumGroupMotionMonitorMilliseconds = 15000;
        private const int MaximumGroupMotionMonitorMilliseconds = 600000;
        private const int PersistentInitFailureFreshSessionRetryDelayMilliseconds =
            100;
        private const int PreResponseTransportFreshSessionRetryDelayMilliseconds =
            1000;
        private const string TopologyUiFeatureMarker =
            "GENERIC_SDO_WRITE_DIRECT_MANUAL_V1";
        private const string ReconnectPolicyMarker =
            "RPC_INIT_FRESH_TCP_ONCE_V2";
        private static readonly PlcUnitOption[] PlcUnitOptions =
        {
            new PlcUnitOption("None / raw DINT (no conversion)", "raw", 1, true),
            new PlcUnitOption("mm (x10000)", "mm", LMC_Units.MM, false),
            new PlcUnitOption("m (x10000000)", "m", LMC_Units.M, false),
            new PlcUnitOption("deg (x10000)", "deg", LMC_Units.DEG, false)
        };

        // The API async methods schedule transport work. Keep live and safety
        // command ordering explicit at the example-application boundary.
        private readonly SemaphoreSlim commandSendGate = new SemaphoreSlim(1, 1);
        private readonly LMCSendPriorityCoordinator sendPriorityCoordinator =
            new LMCSendPriorityCoordinator();
        private LMCConnection connection;
        private LMCCallbackV2StatisticsChangedEventArgs
            lastCallbackV2Statistics;
        private int rpcConnectionAttemptSerial;
        private string lastRpcInitializationEvidence =
            "No RPC initialization attempt";
        private bool lastRpcInitializationRetired = true;
        private string lastCallbackListenerError;
        private LMCConnection recoveryIdentityReadOnlyConnection;
        private string recoveryIdentityReadOnlyReason;
        private LMCSingleAxis axis;
        private LMCGroupAxis group;
        private LMCAxisStopWaitContinuation
            pendingAxisStopWaitContinuation;
        private LMCAxisResetWaitContinuation
            pendingAxisResetWaitContinuation;
        private bool axisResetWaitInterferenceConfirmed;
        private bool axisPowerOffWaitInterferenceConfirmed;
        internal Func<LMCSingleAxis, CancellationToken,
            Task<LMCAxisPowerOffWaitContinuation>>
            AxisPowerOffBeginAsyncOverride { get; set; }
        internal Func<int, Task> FreshSessionRetryDelayAsyncOverride
        {
            get;
            set;
        }
        private LMCGroupStopWaitContinuation
            pendingGroupStopWaitContinuation;
        private bool operationRunning;
        private bool connectionTransitionRunning;
        private bool safetyCommandRunning;
        private int safetyMonitorCount;
        // Stop/PowerOff reserves a new generation before waiting for
        // commandSendGate. App checks and the SDK pre-write boundary share
        // this single source of truth.
        private long safetyRequestGeneration
        {
            get { return sendPriorityCoordinator.CurrentGeneration; }
        }
        private bool motionMayBeActive;
        private string motionAxisName;
        private string motionOperation;
        private bool motionWasObserved;
        private int motionTrackingGeneration;
        private bool groupPowerVerificationPending;
        private bool groupPowerOffVerificationPending;
        private bool groupStatusRefreshRequired;
        private bool groupActiveVerified;
        private bool groupIdentityHomeCheckComplete;
        private bool groupIdentityHomeCheckPassed;
        private bool groupIdentityConfigured;
        private LMCGroupEnableWaitContinuation
            pendingGroupEnableWaitContinuation;
        private LMCGroupDisableWaitContinuation
            pendingGroupDisableWaitContinuation;
        private bool groupProfileLockVerificationPending;
        private bool groupProfileUnlockVerificationPending;
        private bool groupProfileLockRecoveryRequired;
        private string groupProfileLockRecoveryGroupName;
        private bool groupProfileLocked;
        private bool shutdownInProgress;
        private bool allowWindowClose;
        private bool uiInitializationComplete;
        private readonly string uiLanguagePreferenceFilePath;
        private UiLanguage currentUiLanguage;
        private bool uiLanguageSelectionUpdating;

        public MainWindow()
            : this(null, null, null, null)
        {
        }

        internal MainWindow(string diagnosticsMutationJournalDirectoryPath)
            : this(
                diagnosticsMutationJournalDirectoryPath,
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "RecorderDoubleRecovery"),
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "GroupProfileLockRecovery"),
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "MotionUncertaintyRecovery"))
        {
        }

        internal MainWindow(
            string diagnosticsMutationJournalDirectoryPath,
            string recorderDoubleRecoveryJournalDirectoryPath)
            : this(
                diagnosticsMutationJournalDirectoryPath,
                recorderDoubleRecoveryJournalDirectoryPath,
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "GroupProfileLockRecovery"),
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "MotionUncertaintyRecovery"))
        {
        }

        internal MainWindow(
            string diagnosticsMutationJournalDirectoryPath,
            string recorderDoubleRecoveryJournalDirectoryPath,
            string groupProfileLockRecoveryJournalDirectoryPath)
            : this(
                diagnosticsMutationJournalDirectoryPath,
                recorderDoubleRecoveryJournalDirectoryPath,
                groupProfileLockRecoveryJournalDirectoryPath,
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "MotionUncertaintyRecovery"))
        {
        }

        internal MainWindow(
            string diagnosticsMutationJournalDirectoryPath,
            string recorderDoubleRecoveryJournalDirectoryPath,
            string groupProfileLockRecoveryJournalDirectoryPath,
            string motionUncertaintyJournalDirectoryPath)
        {
            this.diagnosticsMutationJournalDirectoryPath =
                diagnosticsMutationJournalDirectoryPath;
            this.recorderDoubleRecoveryJournalDirectoryPath =
                recorderDoubleRecoveryJournalDirectoryPath;
            this.groupProfileLockRecoveryJournalDirectoryPath =
                groupProfileLockRecoveryJournalDirectoryPath;
            this.motionUncertaintyJournalDirectoryPath =
                motionUncertaintyJournalDirectoryPath;
            this.axisPowerOnRecoveryJournalDirectoryPath =
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "AxisPowerOnRecovery");
            this.axisCommandRecoveryJournalDirectoryPath =
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "AxisCommandRecovery");
            this.axisQualificationRecoveryJournalDirectoryPath =
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "AxisQualificationRecovery");
            this.groupPowerRecoveryJournalDirectoryPath =
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "GroupPowerRecovery");
            this.groupResetRecoveryJournalDirectoryPath =
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "GroupResetRecovery");
            this.recoveryRecordRetirementLedgerDirectoryPath =
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "RecoveryRecordRetirementLedger");
            this.maintenanceActionRecoveryJournalDirectoryPath =
                diagnosticsMutationJournalDirectoryPath == null
                    ? null
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "MaintenanceActionRecovery");
            this.uiLanguagePreferenceFilePath =
                diagnosticsMutationJournalDirectoryPath == null
                    ? UiLanguagePreferenceStore.GetDefaultFilePath()
                    : System.IO.Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "UiLanguage",
                        "ui-language.txt");
            InitializeComponent();
            InitializeUiLocalization();

            ComboAxisUnit.ItemsSource = PlcUnitOptions;
            ComboAxisUnit.SelectedIndex = 1;
            ComboGroupUnit.ItemsSource = PlcUnitOptions;
            ComboGroupUnit.SelectedIndex = 1;

            ComboDirection.Items.Add(LMC_DIRECTION.Positive);
            ComboDirection.Items.Add(LMC_DIRECTION.Negative);
            ComboDirection.SelectedItem = LMC_DIRECTION.Positive;

            ComboGroupCoordinate.Items.Add(LMC_COORD_SYSTEM.None);
            ComboGroupCoordinate.Items.Add(LMC_COORD_SYSTEM.Acs);
            ComboGroupCoordinate.SelectedItem = LMC_COORD_SYSTEM.None;

            ComboGroupTransition.Items.Add(
                LMC_GROUP_TRANSITION_MODE.ExactStop);
            ComboGroupTransition.Items.Add(
                LMC_GROUP_TRANSITION_MODE.ContinuousDirect);
            ComboGroupTransition.SelectedItem =
                LMC_GROUP_TRANSITION_MODE.ExactStop;

            ComboGroupBuffer.Items.Add(LMC_BUFFER_MODE.Aborting);
            ComboGroupBuffer.Items.Add(LMC_BUFFER_MODE.Buffered);
            ComboGroupBuffer.SelectedItem = LMC_BUFFER_MODE.Aborting;

            InitializeDiagnosticsUi();
            // Read-only API initialization opens SetOperationMode recovery and
            // must see already committed operator-retirement decisions.
            InitializeRecoveryRecordRetirementLedger();
            InitializeReadOnlyApiUi();
            InitializeQualificationUi();
            InitializeDiagnosticsMutationJournal();
            InitializeRecorderDoubleRecoveryJournal();
            InitializeGroupProfileLockRecoveryJournal();
            InitializeMotionUncertaintyJournal();
            InitializeAxisPowerOnRecoveryJournal();
            InitializeAxisCommandRecoveryJournal();
            InitializeAxisQualificationRecoveryJournal();
            InitializeGroupPowerRecoveryJournal();
            InitializeGroupResetRecoveryJournal();
            InitializeMaintenanceActionUi();
            uiInitializationComplete = true;

            ConfigureExecutableIdentity();
            WriteLog(
                "Example ready. Connect, load _LMCAxis1, and start with Read Status. "
                + "Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.");
            UpdateUiState();
        }

        private void InitializeUiLocalization()
        {
            currentUiLanguage = UiLanguagePreferenceStore.Load(
                uiLanguagePreferenceFilePath);
            uiLanguageSelectionUpdating = true;
            try
            {
                ComboUiLanguage.ItemsSource =
                    UiLanguageOption.CreateDefaultOptions();
                ComboUiLanguage.SelectedIndex =
                    currentUiLanguage == UiLanguage.Korean ? 1 : 0;
            }
            finally
            {
                uiLanguageSelectionUpdating = false;
            }

            ApplyUiLanguage();
        }

        private void ComboUiLanguage_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (uiLanguageSelectionUpdating)
            {
                return;
            }

            var selected = ComboUiLanguage.SelectedItem as UiLanguageOption;
            if (selected == null)
            {
                return;
            }

            currentUiLanguage = selected.Language;
            try
            {
                UiLanguagePreferenceStore.Save(
                    uiLanguagePreferenceFilePath,
                    currentUiLanguage);
            }
            catch (Exception exception)
            {
                WriteLog(
                    "UI language preference could not be saved: "
                    + exception.Message);
            }

            ApplyUiLanguage();
        }

        private void ApplyUiLanguage()
        {
            UiLocalizationService.Apply(this, currentUiLanguage);
        }

        private string TranslateUiText(string english)
        {
            return UiLocalizationCatalog.Translate(
                english,
                currentUiLanguage);
        }

        private void ConfigureExecutableIdentity()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var executablePath = assembly.Location;
            var version = assembly.GetName().Version == null
                ? "unknown"
                : assembly.GetName().Version.ToString();
            var buildTimeUtc = System.IO.File.GetLastWriteTimeUtc(
                executablePath);
            var sdkPath = typeof(LMCConnection).Assembly.Location;
            var sdkBuildTimeUtc = System.IO.File.GetLastWriteTimeUtc(
                sdkPath);

            Title = "LASAL Motion Control API Example v"
                + version
                + " [Generic SDO Write / LIVE Diagnostics]";
            WriteLog(
                "Executable identity: Path="
                + executablePath
                + ", Version="
                + version
                + ", BuildUtc="
                + buildTimeUtc.ToString(
                    "yyyy-MM-dd HH:mm:ss 'UTC'",
                    CultureInfo.InvariantCulture)
                + ", Feature="
                + TopologyUiFeatureMarker
                + ", ReconnectPolicy="
                + ReconnectPolicyMarker
                + ", SdkPath="
                + sdkPath
                + ", SdkBuildUtc="
                + sdkBuildTimeUtc.ToString(
                    "yyyy-MM-dd HH:mm:ss 'UTC'",
                    CultureInfo.InvariantCulture)
                + ".");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(
                    () =>
                    {
                        ButtonConnect.Focus();
                        ScrollSelectedMotionTabToTop();
                    }));
        }

        private void MotionTabs_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(e.Source, sender))
            {
                return;
            }

            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(ScrollSelectedMotionTabToTop));
        }

        private void ComboGroupCoordinate_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            UpdateUiState();
        }

        private void ScrollSelectedMotionTabToTop()
        {
            if (TabsMotion == null)
            {
                return;
            }

            if (TabsMotion.SelectedIndex == 0)
            {
                ScrollSingleAxis?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 1)
            {
                ScrollGroupMotion?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 2)
            {
                ScrollDiagnostics?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 3)
            {
                ScrollBulkSnapshot?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 4)
            {
                ScrollRecorder?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 5)
            {
                ScrollDiagnosticsOperations?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 6)
            {
                ScrollReadOnlyApi?.ScrollToTop();
            }
        }

        private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Connect",
                async () =>
                {
                    if (recoveryRecordRetirementRestartRequired)
                    {
                        throw new InvalidOperationException(
                            "Connect is blocked because stale recovery records "
                            + "were retired in this process. Restart the "
                            + "application before opening another session.");
                    }

                    if (HasActiveAxisCommandRecoveryRecord
                        && connection != null
                        && connection.IsConnected)
                    {
                        throw new InvalidOperationException(
                            "Reconnect is blocked while Axis Stop/Reset recovery is active on the current session.");
                    }

                    if (HasActiveAxisQualificationRecoveryRecord
                        && connection != null
                        && connection.IsConnected)
                    {
                        throw new InvalidOperationException(
                            "Reconnect is blocked while Single Axis qualification recovery is active on the current session. Complete explicit Stop/Power Off recovery first.");
                    }

                    if (HasActiveGroupPowerRecoveryRecord
                        && connection != null
                        && connection.IsConnected)
                    {
                        throw new InvalidOperationException(
                            "Reconnect is blocked while Group Power recovery is "
                            + "active on the current connection. Continue exact-"
                            + "identity recovery without replacing the TCP session.");
                    }

                    var pendingProfileLock =
                        HasPendingGroupProfileLockContinuation();
                    var activeProfileLockJournal =
                        HasActiveGroupProfileLockRecoveryJournalRecord;
                    if (pendingProfileLock || activeProfileLockJournal)
                    {
                        if (connection != null && connection.IsConnected)
                        {
                            throw new InvalidOperationException(
                                "Reconnect is blocked while Group Enable is pending "
                                + "or a durable profile-lock recovery record is active. "
                                + "Resume status-only verification, run Disable, or "
                                + "complete stable Power Off verification first.");
                        }

                        PromotePendingGroupProfileLockToRecovery(
                            "Reconnect after connection loss");
                    }

                    if (groupProfileLockRecoveryRequired
                        && connection != null
                        && connection.IsConnected)
                    {
                        throw new InvalidOperationException(
                            "Reconnect is blocked while the group profile-lock result "
                            + "is uncertain. Run Disable or complete stable Power Off "
                            + "verification first.");
                    }

                    if (GroupResetReconnectBlockedOnCurrentSession())
                    {
                        throw new InvalidOperationException(
                            "Reconnect is blocked while Group Reset is pending "
                            + "or its submission outcome is uncertain. "
                            + GetGroupResetRecoveryGuidance());
                    }

                    var remoteIp = RequiredText(
                        TextRemoteIp.Text,
                        "PLC IP");
                    var remotePort = ParsePort(
                        TextRemotePort.Text,
                        "TCP port",
                        false);
                    var localIp = RequiredText(
                        TextLocalIp.Text,
                        "PC local IPv4");
                    var callbackPort = ParsePort(
                        TextCallbackPort.Text,
                        "Callback UDP port",
                        true);
                    EnsureAxisPowerOnRecoveryEndpoint(
                        remoteIp,
                        remotePort);
                    EnsureAxisCommandRecoveryEndpoint(remoteIp, remotePort);
                    EnsureAxisQualificationRecoveryEndpoint(
                        remoteIp,
                        remotePort);
                    EnsureGroupProfileLockRecoveryEndpoint(
                        remoteIp,
                        remotePort);
                    EnsureGroupPowerRecoveryEndpoint(
                        remoteIp,
                        remotePort);
                    EnsureGroupResetRecoveryEndpoint(
                        remoteIp,
                        remotePort,
                        localIp,
                        callbackPort);
                    EnsureMotionRecoveryEndpoint(remoteIp, remotePort);
                    EnsureMaintenanceRecoveryEndpoint(remoteIp, remotePort);

                    var admission = EvaluateDiagnosticsAdmission(
                        DiagnosticsAdmissionOperation.ConnectOrReconnect);
                    if (!admission.IsAllowed)
                    {
                        throw CreateDiagnosticsAdmissionException(
                            "Reconnect",
                            admission);
                    }

                    if (motionMayBeActive
                        && connection != null
                        && connection.IsConnected)
                    {
                        throw new InvalidOperationException(
                            "A second reconnect is blocked because motion may still be active on "
                            + motionAxisName
                            + ". Use the already connected exact recovery target to send Stop or PowerOff.");
                    }

                    if (connection != null)
                    {
                        await CloseCurrentConnectionAsync(false);
                    }

                    var connectionAttempt = ++rpcConnectionAttemptSerial;
                    var freshSessionRetryUsed = false;
                    string freshSessionRetryFirstFailureEvidence = null;
                    string freshSessionRetryReason = null;
                    var freshSessionRetryDelayMilliseconds = 0;
                    var candidateOrdinal = 0;
                    LMCConnection newConnection;
                    while (true)
                    {
                        candidateOrdinal++;
                        lastRpcInitializationRetired = false;
                        lastRpcInitializationEvidence =
                            AppendFreshSessionRetryEvidence(
                                FormatRpcInitializationEvidence(
                                    connectionAttempt,
                                    candidateOrdinal,
                                    "Connecting",
                                    remoteIp,
                                    remotePort,
                                    localIp,
                                    callbackPort,
                                    null,
                                    null),
                                freshSessionRetryUsed,
                                freshSessionRetryReason,
                                freshSessionRetryDelayMilliseconds,
                                candidateOrdinal,
                                freshSessionRetryFirstFailureEvidence);
                        newConnection = CreateCoordinatedConnection();
                        AttachConnection(newConnection);
                        connection = newConnection;
                        ClearLoadedObjects();
                        UpdateUiState();

                        try
                        {
                            await newConnection.RpcInitConnectionAsync(
                                remoteIp,
                                remotePort,
                                localIp,
                                callbackPort,
                                1u,
                                CancellationToken.None);
                            lastRpcInitializationEvidence =
                                AppendFreshSessionRetryEvidence(
                                    FormatRpcInitializationEvidence(
                                        connectionAttempt,
                                        candidateOrdinal,
                                        "Connected",
                                        remoteIp,
                                        remotePort,
                                        localIp,
                                        callbackPort,
                                        newConnection,
                                        null),
                                    freshSessionRetryUsed,
                                    freshSessionRetryReason,
                                    freshSessionRetryDelayMilliseconds,
                                    candidateOrdinal,
                                    freshSessionRetryFirstFailureEvidence);
                            lastRpcInitializationRetired = false;
                            RememberConnectedRemoteEndpoint(
                                remoteIp,
                                remotePort);
                            break;
                        }
                        catch (Exception error)
                        {
                            var failedInitializationEvidence =
                                FormatRpcInitializationEvidence(
                                    connectionAttempt,
                                    candidateOrdinal,
                                    "Failed",
                                    remoteIp,
                                    remotePort,
                                    localIp,
                                    callbackPort,
                                    newConnection,
                                    error);
                            var persistentInitFailureRetry =
                                !freshSessionRetryUsed
                                && IsExactPersistentSessionInitMinusOneFailure(
                                    newConnection);
                            var preResponseTransportRetry =
                                !freshSessionRetryUsed
                                && !persistentInitFailureRetry
                                && IsEligiblePreResponseTransportFailure(
                                    newConnection,
                                    error);
                            var useFreshSessionRetry =
                                persistentInitFailureRetry
                                || preResponseTransportRetry;

                            if (ReferenceEquals(connection, newConnection))
                            {
                                connection = null;
                            }

                            DetachConnection(newConnection);
                            newConnection.Dispose();
                            ClearLoadedObjects();

                            if (useFreshSessionRetry)
                            {
                                freshSessionRetryUsed = true;
                                freshSessionRetryReason =
                                    persistentInitFailureRetry
                                        ? "PersistentSessionInitMinusOne"
                                        : "PreResponseTransportFailure";
                                freshSessionRetryDelayMilliseconds =
                                    persistentInitFailureRetry
                                        ? PersistentInitFailureFreshSessionRetryDelayMilliseconds
                                        : PreResponseTransportFreshSessionRetryDelayMilliseconds;
                                freshSessionRetryFirstFailureEvidence =
                                    failedInitializationEvidence;
                                lastRpcInitializationEvidence =
                                    AppendFreshSessionRetryScheduledEvidence(
                                        failedInitializationEvidence,
                                        freshSessionRetryReason,
                                        freshSessionRetryDelayMilliseconds,
                                        candidateOrdinal,
                                        freshSessionRetryFirstFailureEvidence);
                                lastRpcInitializationRetired = true;
                                WriteLog(
                                    "RPC init fresh-session retry scheduled. Reason="
                                    + freshSessionRetryReason
                                    + ", CandidateOrdinal="
                                    + candidateOrdinal.ToString(
                                        CultureInfo.InvariantCulture)
                                    + ", NextCandidateOrdinal="
                                    + (candidateOrdinal + 1).ToString(
                                        CultureInfo.InvariantCulture)
                                    + ". "
                                    + "The failed TCP session was retired; one fresh TCP "
                                    + "session retry will start after "
                                    + freshSessionRetryDelayMilliseconds
                                        .ToString(CultureInfo.InvariantCulture)
                                    + " ms. FreshSessionFirstFailure={"
                                    + failedInitializationEvidence
                                    + "}");
                                UpdateUiState();
                                await DelayBeforeFreshSessionRetryAsync(
                                    freshSessionRetryDelayMilliseconds);
                                continue;
                            }

                            lastRpcInitializationEvidence =
                                AppendFreshSessionRetryEvidence(
                                failedInitializationEvidence,
                                freshSessionRetryUsed,
                                freshSessionRetryReason,
                                freshSessionRetryDelayMilliseconds,
                                candidateOrdinal,
                                freshSessionRetryFirstFailureEvidence);
                            lastRpcInitializationRetired = true;
                            UpdateUiState();
                            throw;
                        }
                    }

                    WriteLog(
                        "RPC initialized. Callback endpoint="
                        + newConnection.CallbackLocalEndPoint
                        + ", EventMask=0x"
                        + newConnection.EventMask.ToString("X8"));

                    await TryAutoLoadEtherCATTopologyAfterConnectAsync(
                        newConnection);
                    try
                    {
                        await EnsureAxisPowerOnRecoveryConnectionIdentityAsync(
                            "Reconnect Axis Power On recovery identity");
                        await EnsureAxisCommandRecoveryConnectionIdentityAsync(
                            "Reconnect Axis Stop/Reset recovery identity");
                        await EnsureAxisQualificationRecoveryConnectionIdentityAsync(
                            "Reconnect Single Axis qualification recovery identity");
                        await EnsureMotionRecoveryConnectionIdentityAsync(
                            "Reconnect motion recovery identity");
                        await EnsureGroupProfileLockRecoveryConnectionIdentityAsync(
                            "Reconnect recovery identity");
                        await EnsureGroupPowerRecoveryConnectionIdentityAsync(
                            "Reconnect Group Power recovery identity");
                        await EnsureGroupResetRecoveryConnectionIdentityAsync(
                            "Reconnect Group Reset recovery identity");
                        await EnsureMaintenanceRecoveryConnectionIdentityAsync(
                            "Reconnect Home/Test recovery identity");
                        await EnsureAxisSetOperationModeRecoveryConnectionIdentityAsync(
                            "Reconnect SetOperationMode recovery identity");
                        await EnsureDiagnosticsMutationRecoveryConnectionIdentityAsync(
                            "Reconnect diagnostics mutation recovery identity");
                        ClearRecoveryIdentityReadOnlyQuarantine();
                    }
                    catch (RecoveryConnectionIdentityMismatchException error)
                    {
                        EnterRecoveryIdentityReadOnlyQuarantine(
                            newConnection,
                            error);
                    }
                    catch
                    {
                        if (motionMayBeActive
                            || HasActiveAxisPowerOnRecoveryRecord
                            || HasActiveAxisCommandRecoveryRecord
                            || HasActiveAxisQualificationRecoveryRecord
                            || HasActiveGroupPowerRecoveryRecord
                            || HasActiveGroupResetRecoveryRecord
                            || HasUnresolvedMaintenanceAction)
                        {
                            await CloseRejectedMotionRecoveryConnectionAsync(
                                newConnection);
                        }
                        else
                        {
                            await CloseCurrentConnectionAsync(false);
                        }
                        throw;
                    }
                },
                true);
        }

        private async void ButtonCloseConnection_Click(
            object sender,
            RoutedEventArgs e)
        {
            var recoveryIdentityReadOnlyClose =
                IsRecoveryIdentityReadOnlyConnection(connection);
            if (!recoveryIdentityReadOnlyClose
                && HasUnresolvedAxisPowerState())
            {
                WriteLog(
                    "Close Connection is blocked while Axis Power recovery is unresolved. "
                    + GetAxisPowerOnRecoveryGuidance());
                return;
            }

            if (!recoveryIdentityReadOnlyClose
                && HasUnresolvedAxisCommandState())
            {
                WriteLog(
                    "Close Connection is blocked while Axis Stop/Reset recovery is unresolved. Complete exact status-only proof or the explicit recovery action first.");
                return;
            }

            if (!recoveryIdentityReadOnlyClose
                && HasUnresolvedAxisQualificationState())
            {
                WriteLog(
                    "Close Connection is blocked while Single Axis qualification recovery is unresolved. "
                    + GetAxisQualificationRecoveryGuidance());
                return;
            }

            if (!recoveryIdentityReadOnlyClose
                && HasUnresolvedGroupResetState())
            {
                WriteLog(
                    "Close Connection is blocked while an accepted Group Reset "
                    + "is awaiting stable group/member error-clearance proof. "
                    + GetGroupResetRecoveryGuidance());
                return;
            }

            if (!recoveryIdentityReadOnlyClose
                && HasUnresolvedGroupProfileLockState())
            {
                WriteLog(
                    "Close Connection is blocked while Group Enable is pending or "
                    + "the profile-lock result is uncertain. Resume verification, "
                    + "run Disable, or complete stable Power Off verification first.");
                return;
            }

            if (!recoveryIdentityReadOnlyClose
                && HasUnresolvedGroupPowerState())
            {
                WriteLog(
                    "Close Connection is blocked while Group Power recovery "
                    + "is unresolved. "
                    + GetGroupPowerRecoveryGuidance());
                return;
            }

            await RunOperationAsync(
                "Close Connection",
                () =>
                {
                    var admission = EvaluateDiagnosticsAdmission(
                        DiagnosticsAdmissionOperation.CloseConnection);
                    if (!admission.IsAllowed)
                    {
                        throw CreateDiagnosticsAdmissionException(
                            "Close Connection",
                            admission);
                    }

                    return CloseCurrentConnectionAsync(true);
                },
                true);
        }

        private async void ButtonLookupAxis_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRecoveryIdentityReadOnlyConnection(connection)
                && HasPendingAxisHandleBoundContinuation())
            {
                WriteLog(
                    "Load Axis is blocked while an accepted Axis Reset, Stop, "
                    + "or Power Off continuation is pending. Resume status-only "
                    + "verification or close the connection before replacing "
                    + "the Axis handle.");
                return;
            }

            await RunOperationAsync(
                "Load Axis",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    var objectName = RequiredText(
                        TextAxisName.Text,
                        "Axis object name");
                    var inspectionOnly =
                        IsRecoveryIdentityReadOnlyConnection(currentConnection);
                    if (inspectionOnly)
                    {
                        var inspectedAxis = await LMCSingleAxis.CreateAsync(
                            currentConnection,
                            objectName,
                            CancellationToken.None);
                        TextAxisReference.Text = inspectedAxis.AxisReference.ToString(
                            CultureInfo.InvariantCulture);
                        TextAxisResult.Text =
                            "READ-ONLY INSPECTION"
                            + Environment.NewLine
                            + "Loaded "
                            + inspectedAxis.AxisName
                            + Environment.NewLine
                            + "Reference="
                            + inspectedAxis.AxisReference
                            + Environment.NewLine
                            + FormatResponse(inspectedAxis.AxisInfoResponse)
                            + Environment.NewLine
                            + "The recovery identity mismatch remains unresolved; "
                            + "no application control handle was retained. Status "
                            + "and position reads use transient inspection handles; "
                            + "no durable recovery record was changed.";
                        WriteLog(
                            "READ-ONLY INSPECTION: Axis information loaded without "
                            + "retaining an application control handle. Name="
                            + inspectedAxis.AxisName
                            + ", Ref="
                            + inspectedAxis.AxisReference);
                        return;
                    }

                    if (GetActiveAxisPowerRecoveryRecord() != null
                        && axis != null)
                    {
                        throw new InvalidOperationException(
                            "Axis reload is blocked while Axis Power recovery is "
                            + "active on the loaded recovery handle. Complete the "
                            + "recovery before loading the axis again; no lookup RPC "
                            + "was sent.");
                    }

                    EnsureAxisPowerOnRecoveryLookupAllowed(objectName);
                    EnsureAxisCommandRecoveryLookupAllowed(objectName);
                    EnsureAxisQualificationRecoveryLookupAllowed(objectName);
                    EnsureMotionRecoveryLookupAllowed(
                        MotionUncertaintyTargetKind.Axis,
                        objectName);
                    var loadedAxis = await LMCSingleAxis.CreateAsync(
                        currentConnection,
                        objectName,
                        CancellationToken.None);
                    EnsureLoadedAxisMatchesMotionRecovery(loadedAxis);
                    EnsureLoadedAxisMatchesPowerOnRecovery(loadedAxis);
                    EnsureLoadedAxisMatchesAxisCommandRecovery(loadedAxis);
                    EnsureLoadedAxisMatchesAxisQualificationRecovery(
                        loadedAxis);
                    RememberMotionLookupIdentity(
                        MotionUncertaintyTargetKind.Axis,
                        loadedAxis.AxisName,
                        loadedAxis.AxisReference);

                    axis = loadedAxis;
                    InvalidateAxisQualificationConfirmations();
                    TextAxisReference.Text = loadedAxis.AxisReference.ToString(
                        CultureInfo.InvariantCulture);
                    TextAxisResult.Text =
                        "Loaded "
                        + loadedAxis.AxisName
                        + Environment.NewLine
                        + "Reference="
                        + loadedAxis.AxisReference
                        + Environment.NewLine
                        + FormatResponse(loadedAxis.AxisInfoResponse);
                    WriteLog(
                        "Axis loaded. Name="
                        + loadedAxis.AxisName
                        + ", Ref="
                        + loadedAxis.AxisReference);
                });
        }

        private async void ButtonLookupGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRecoveryIdentityReadOnlyConnection(connection)
                && GroupResetLookupBlockedOnCurrentSession())
            {
                WriteLog(
                    "Load Group is blocked while an accepted Group Reset is "
                    + "pending on the loaded session-bound group handle. "
                    + GetGroupResetRecoveryGuidance());
                return;
            }

            await RunOperationAsync(
                "Load Group",
                async () =>
                {
                    var objectName = RequiredText(
                        TextGroupName.Text,
                        "Group object name");
                    var currentConnection = RequireConnection();
                    var inspectionOnly =
                        IsRecoveryIdentityReadOnlyConnection(currentConnection);
                    if (inspectionOnly)
                    {
                        var inspectedGroup = await LMCGroupAxis.CreateAsync(
                            currentConnection,
                            objectName,
                            CancellationToken.None);
                        var members = await inspectedGroup
                            .GetGroupMembersInfoResultAsync(CancellationToken.None);
                        EnsureGroupMembersSuccess(
                            "Inspect Group Members",
                            members);

                        TextGroupReference.Text = inspectedGroup.GroupReference.ToString(
                            CultureInfo.InvariantCulture);
                        TextGroupResult.Text =
                            "READ-ONLY INSPECTION"
                            + Environment.NewLine
                            + "Loaded "
                            + inspectedGroup.GroupName
                            + Environment.NewLine
                            + "Reference="
                            + inspectedGroup.GroupReference
                            + Environment.NewLine
                            + FormatGroupMembers(members)
                            + Environment.NewLine
                            + "The recovery identity mismatch remains unresolved; "
                            + "no application control handle was retained. Member, "
                            + "status, and position reads use transient inspection "
                            + "handles; no durable recovery record was changed.";
                        WriteLog(
                            "READ-ONLY INSPECTION: Group information and members loaded "
                            + "without retaining an application control handle. Name="
                            + inspectedGroup.GroupName
                            + ", Ref="
                            + inspectedGroup.GroupReference
                            + ", AxisCount="
                            + members.AxisCount);
                        return;
                    }

                    EnsureGroupResetRecoveryLookupAllowed(objectName);
                    EnsureGroupPowerRecoveryLookupAllowed(objectName);
                    EnsureMotionRecoveryLookupAllowed(
                        MotionUncertaintyTargetKind.Group,
                        objectName);
                    if (HasPendingGroupProfileLockContinuation()
                        && !HasAcceptedGroupProfileLockRecoveryRecord
                        && !HasAcceptedGroupProfileUnlockRecoveryRecord)
                    {
                        throw new InvalidOperationException(
                            "Group reload is blocked while an accepted Group Enable "
                            + "is pending. Resume status-only verification, run Disable, "
                            + "or complete stable Power Off verification first.");
                    }

                    if (!groupProfileLockRecoveryRequired
                        && HasActiveGroupProfileLockRecoveryJournalRecord)
                    {
                        PromoteGroupProfileLockRecoveryJournal(
                            "Group reload found an active durable recovery record");
                    }

                    if (HasActiveGroupProfileLockRecoveryJournalRecord
                        && !string.Equals(
                            groupProfileLockRecoveryGroupName,
                            objectName,
                            StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "A different group cannot be loaded while a durable "
                            + "Group Enable recovery record is active. Reload only "
                            + groupProfileLockRecoveryGroupName
                            + "; no lookup RPC was sent.");
                    }

                    if (groupProfileLockRecoveryRequired)
                    {
                        if (!string.Equals(
                            groupProfileLockRecoveryGroupName,
                            objectName,
                            StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException(
                                "A different group cannot be loaded while the "
                                + "profile-lock result is uncertain. Reload only "
                                + groupProfileLockRecoveryGroupName
                                + " after reconnect, then run Disable or complete "
                                + "stable Power Off verification.");
                        }

                        if (group != null)
                        {
                            throw new InvalidOperationException(
                                "The recovery group is already loaded. Run Disable "
                                + "or complete stable Power Off verification before "
                                + "loading it again.");
                        }
                    }

                    var loadedGroup = await LMCGroupAxis.CreateAsync(
                        currentConnection,
                        objectName,
                        CancellationToken.None);
                    EnsureLoadedGroupMatchesResetRecovery(loadedGroup);
                    EnsureLoadedGroupMatchesProfileLockRecovery(
                        loadedGroup);
                    EnsureLoadedGroupMatchesPowerRecovery(loadedGroup);
                    EnsureLoadedGroupMatchesMotionRecovery(loadedGroup);
                    await AttachGroupResetRecoveryAsync(loadedGroup);
                    RememberMotionLookupIdentity(
                        MotionUncertaintyTargetKind.Group,
                        loadedGroup.GroupName,
                        loadedGroup.GroupReference);

                    group = loadedGroup;
                    ResetGroupPreparationState();
                    ReapplyCurrentGroupResetRecoveryState();
                    TextGroupReference.Text = loadedGroup.GroupReference.ToString(
                        CultureInfo.InvariantCulture);
                    TextGroupResult.Text =
                        "Loaded "
                        + loadedGroup.GroupName
                        + Environment.NewLine
                        + "Reference="
                        + loadedGroup.GroupReference;
                    if (HasActiveGroupResetRecoveryRecord)
                    {
                        TextGroupResult.Text += Environment.NewLine
                            + "Durable Group Reset recovery attached after one "
                            + "fresh exact 0x20D2 member snapshot. Resume Reset "
                            + "Verification sends 0x2045/0x2028 only; 0x2049 "
                            + "will not be replayed.";
                    }
                    else if (groupProfileLockRecoveryRequired)
                    {
                        TextGroupResult.Text += Environment.NewLine
                            + "Uncertain profile-lock recovery state retained; "
                            + "Disable or stable Power Off verification is required.";
                    }
                    else if (HasAcceptedGroupProfileLockRecoveryRecord)
                    {
                        TextGroupResult.Text += Environment.NewLine
                            + "Accepted Group Enable recovery is ready for exact-"
                            + "identity status-only verification; 0x2047 will not "
                            + "be replayed. Set Identity/Home Check remains "
                            + "process-local and must be re-established before motion.";
                    }
                    else if (HasAcceptedGroupProfileUnlockRecoveryRecord)
                    {
                        TextGroupResult.Text += Environment.NewLine
                            + "Accepted Group Disable recovery is ready for exact-"
                            + "identity status-only verification; 0x2048 will not "
                            + "be replayed.";
                    }
                    WriteLog(
                        "Group loaded. Name="
                        + loadedGroup.GroupName
                        + ", Ref="
                        + loadedGroup.GroupReference);
                });
        }

        private async void ButtonReadStatus_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Axis Status",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    var inspectionOnly =
                        IsRecoveryIdentityReadOnlyConnection(currentConnection);
                    var currentAxis = inspectionOnly
                        ? await CreateReadOnlyInspectionAxisAsync(
                            currentConnection,
                            "Read Axis Status")
                        : RequireAxis();
                    var result = await currentAxis.ReadStatusResultAsync(
                        CancellationToken.None);

                    if (inspectionOnly)
                    {
                        EnsureAxisStatusReadSuccess("Read Status", result);
                        DisplayAxisStatus(result);
                        WriteLog(
                            "READ-ONLY INSPECTION: Axis status read without "
                            + "changing application or durable recovery state. Name="
                            + currentAxis.AxisName
                            + ", Ref="
                            + currentAxis.AxisReference);
                        return;
                    }

                    EnsureAxisStatusSuccess("Read Status", result);
                    DisplayAxisStatus(result);

                    if (!IsTrackedMotionAxis(currentAxis.AxisName))
                    {
                        return;
                    }

                    if (!result.IsStandstill)
                    {
                        RecordMotionObserved(currentAxis.AxisName);
                        return;
                    }

                    if (!result.IsPowerOn)
                    {
                        var verified =
                            await WaitForStablePowerOffAndStandstillAsync(
                                currentAxis,
                                750);
                        DisplayAxisStatus(verified);
                        await ClearMotionWarningAfterVerifiedStateAsync(
                            "Read Status verified three stable PowerOn=false and Standstill samples");
                        return;
                    }

                    if (motionWasObserved)
                    {
                        var verified = await WaitForStandstillAsync(
                            currentAxis,
                            750,
                            0);
                        DisplayAxisStatus(verified);
                        await ClearMotionWarningAfterVerifiedStateAsync(
                            "Read Status verified three stable safe samples");
                        return;
                    }

                    WriteLog(
                        "SAFETY: Standstill was reported, but motion has not yet "
                        + "been observed. The motion warning remains active; use "
                        + "Stop or PowerOff to establish a known safe state.");
                });
        }

        private async void ButtonReadPosition_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Actual Position",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    var inspectionOnly =
                        IsRecoveryIdentityReadOnlyConnection(currentConnection);
                    var currentAxis = inspectionOnly
                        ? await CreateReadOnlyInspectionAxisAsync(
                            currentConnection,
                            "Read Actual Position")
                        : RequireAxis();
                    var unit = ReadAxisUnitSelection();
                    var result = await currentAxis.GetActualPositionResultAsync(
                        CancellationToken.None);
                    EnsureAxisPositionSuccess("Read Actual Position", result);

                    TextAxisResult.Text =
                        "Actual position"
                        + Environment.NewLine
                        + "Raw DINT="
                        + result.PositionRaw
                        + Environment.NewLine
                        + FormatEngineeringPosition(result.PositionRaw, unit)
                        + Environment.NewLine
                        + "FunctionStatus=0x"
                        + result.FunctionStatus.ToString("X4")
                        + ", ErrorId="
                        + result.ErrorId;
                    if (inspectionOnly)
                    {
                        WriteLog(
                            "READ-ONLY INSPECTION: Axis position read without "
                            + "changing application or durable recovery state. Name="
                            + currentAxis.AxisName
                            + ", Ref="
                            + currentAxis.AxisReference);
                    }
                });
        }

        private async Task<LMCSingleAxis> CreateReadOnlyInspectionAxisAsync(
            LMCConnection currentConnection,
            string operation)
        {
            if (!IsRecoveryIdentityReadOnlyConnection(currentConnection))
            {
                throw new InvalidOperationException(
                    operation
                    + " requested a transient inspection Axis outside the "
                    + "recovery-identity read-only quarantine.");
            }

            var objectName = RequiredText(
                TextAxisName.Text,
                "Axis object name");
            var inspectedAxis = await LMCSingleAxis.CreateAsync(
                currentConnection,
                objectName,
                CancellationToken.None);
            TextAxisReference.Text = inspectedAxis.AxisReference.ToString(
                CultureInfo.InvariantCulture);
            return inspectedAxis;
        }

        private async void ButtonPowerOn_Click(object sender, RoutedEventArgs e)
        {
            var loadedAxis = axis;
            var recoveryRecord = GetActiveAxisPowerRecoveryRecord();
            var resumeContinuation = loadedAxis == null
                ? null
                : loadedAxis.PendingPowerOnWaitContinuation;
            var resumeAcceptedPowerOn = recoveryRecord != null
                && recoveryRecord.ExpectedPowerOn
                && !axisPowerOnRecoveryRequired
                && ((resumeContinuation != null
                        && resumeContinuation.IsPending)
                    || axisPowerOnAcceptedRestartRecovery);
            if (!resumeAcceptedPowerOn
                && !CanStartLiveCommand("Power On"))
            {
                return;
            }

            if (resumeAcceptedPowerOn && motionMayBeActive)
            {
                WriteLog(
                    "Power On verification resume is blocked while motion may "
                    + "still be active.");
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Power On",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var continuation = currentAxis
                        .PendingPowerOnWaitContinuation;
                    var resume = continuation != null
                        && continuation.IsPending;
                    var restartAccepted = !resume
                        && axisPowerOnAcceptedRestartRecovery
                        && recoveryRecord != null
                        && recoveryRecord.ExpectedPowerOn;
                    AxisPowerOnRecoveryRecord verificationRecord =
                        resume || restartAccepted ? recoveryRecord : null;
                    var powerCommandDispatchStarted = false;
                    if (resume || restartAccepted)
                    {
                        if (axisPowerOnRecoveryRequired)
                        {
                            throw new InvalidOperationException(
                                "Restart recovery cannot resume Power On proof. "
                                + "Power On will not be replayed; send Power Off "
                                + "explicitly and verify the safe state.");
                        }

                        await EnsureAxisPowerRecoveryIdentityAsync(
                            currentAxis,
                            verificationRecord,
                            "Resume Axis Power On verification");
                    }
                    else
                    {
                        EnsureNoUnresolvedDiagnosticMutation("Power On");
                        verificationRecord =
                            await ArmAxisPowerOnRecoveryBeforeDispatchAsync(
                                currentAxis);
                    }

                    try
                    {
                        EnsureNoNewSafetyRequest(
                            safetyGeneration,
                            "Power On");
                        LMCAxisPowerStateWaitResult result;
                        using (sendPriorityCoordinator.BeginPreemptibleScope(
                            safetyGeneration,
                            resume
                                ? "Resume Axis Power On verification"
                                : (restartAccepted
                                    ? "Restart Axis Power On status-only verification"
                                    : "Axis Power On accepted-once")))
                        {
                            if (!resume && !restartAccepted)
                            {
                                powerCommandDispatchStarted = true;
                            }
                            result = resume
                                ? await currentAxis
                                    .ResumePowerOnWaitForStableStateAsync(
                                        continuation,
                                        CancellationToken.None)
                                : (restartAccepted
                                    ? await currentAxis.WaitForPowerStateAsync(
                                        true,
                                        CancellationToken.None)
                                    : await currentAxis
                                        .PowerOnAndWaitForStableStateAsync(
                                            new LMCAxisPowerStateWaitOptions(),
                                            accepted =>
                                                PersistAxisPowerOnAcceptedForRecord(
                                                    accepted,
                                                    verificationRecord,
                                                    "Axis Power On ACK accepted"),
                                            CancellationToken.None));
                        }

                        EnsureNoNewSafetyRequestBeforeResultApplication(
                            safetyGeneration,
                            "Power On verification");
                        await CompleteAxisPowerRecoveryAfterStableProofAsync(
                            currentAxis,
                            true,
                            result.FinalStatus,
                            result.StableSampleCount,
                            result.RequiredStableSampleCount,
                            verificationRecord,
                            resume
                                ? "Resumed Axis Power On verification"
                                : (restartAccepted
                                    ? "Restart Axis Power On status-only proof"
                                    : "Axis Power On stable proof"));
                        DisplayAxisStatus(result.FinalStatus);
                        TextAxisResult.Text += Environment.NewLine
                            + "Power On ACK accepted exactly once; 0x2023 was not "
                            + "replayed. Status polls="
                            + result.PollCount
                            + ", Stable="
                            + result.StableSampleCount
                            + "/"
                            + result.RequiredStableSampleCount
                            + ".";
                    }
                    catch (Exception error)
                    {
                        var powerOnEvidence =
                            GetAxisPowerOnWaitEvidence(error);
                        if (powerOnEvidence != null
                            && powerOnEvidence.LastObservedStatus != null)
                        {
                            DisplayAxisStatus(
                                powerOnEvidence.LastObservedStatus);
                        }

                        AppendAxisPowerOnWaitEvidence(
                            powerOnEvidence,
                            "Axis Power On stable-state completion was not proven.");
                        PreserveAxisPowerOnWaitFailure(
                            currentAxis,
                            error,
                            verificationRecord,
                            powerCommandDispatchStarted,
                            "Axis Power On");
                        throw;
                    }
                });
        }

        private async void ButtonPowerOff_Click(object sender, RoutedEventArgs e)
        {
            var recoveryRecord = GetActiveAxisPowerRecoveryRecord();
            var statusOnlyPowerOff = recoveryRecord != null
                && !recoveryRecord.ExpectedPowerOn
                && !axisPowerOffReplacementAllowed;
            if (statusOnlyPowerOff)
            {
                var statusOnlyAcceptedAcknowledgement =
                    recoveryRecord.State
                        == AxisPowerOnRecoveryState.AcceptedAwaitingProof;
                var currentAxis = axis;
                var currentConnection = connection;
                if (currentAxis == null
                    || currentConnection == null
                    || !currentConnection.IsConnected)
                {
                    WriteLog(
                        "Axis Power Off status-only verification is blocked until the exact recovery axis is loaded on a live connection. No 0x2023 or status RPC was sent.");
                    return;
                }

                AxisCommandRecoveryRecord stopResolvedByPowerOff = null;

                await RunSafetyMonitorAsync(
                    "Power Off",
                    currentAxis,
                    async () =>
                    {
                        var continuation = pendingAxisPowerOffWaitContinuation;
                        if (continuation != null
                            && continuation.IsPending
                            && ReferenceEquals(
                                currentAxis.PendingPowerOffWaitContinuation,
                                continuation))
                        {
                            EnsureCurrentAxisMatchesPowerRecovery(
                                currentAxis,
                                recoveryRecord,
                                "Resume Axis Power Off verification");
                            try
                            {
                                var resumed = await currentAxis
                                    .ResumePowerOffWaitForStableStateAsync(
                                        continuation,
                                        CancellationToken.None);
                                await CompleteAxisPowerRecoveryAfterStableProofAsync(
                                    currentAxis,
                                    false,
                                    resumed.FinalStatus,
                                    resumed.StablePowerOffStandstillSampleCount,
                                    resumed.RequiredStableSampleCount,
                                    recoveryRecord,
                                    "Resumed Axis Power Off proof");
                                stopResolvedByPowerOff = await
                                    PrepareAxisCommandStopAfterStablePowerOffAsync(
                                        currentAxis,
                                        continuation,
                                        resumed.FinalStatus,
                                        resumed.StablePowerOffStandstillSampleCount,
                                        resumed.RequiredStableSampleCount,
                                        "Resumed Axis Power Off proof");
                                return resumed.FinalStatus;
                            }
                            catch (Exception error)
                            {
                                PreserveAxisPowerOffWaitFailure(
                                    currentAxis,
                                    error,
                                    recoveryRecord,
                                    false,
                                    false,
                                    false,
                                    continuation,
                                    "Resume Axis Power Off verification");
                                throw;
                            }
                        }

                        try
                        {
                            await EnsureAxisPowerRecoveryIdentityAsync(
                                currentAxis,
                                recoveryRecord,
                                "Restart Axis Power Off status-only verification");
                            var observed = await currentAxis.WaitForPowerStateAsync(
                                false,
                                CancellationToken.None);
                            ObserveAxisPowerRecoveryStatus(
                                currentAxis,
                                recoveryRecord,
                                observed.FinalStatus,
                                "Axis Power Off status-only recovery");
                            await CompleteAxisPowerRecoveryAfterStableProofAsync(
                                currentAxis,
                                false,
                                observed.FinalStatus,
                                observed.StableSampleCount,
                                observed.RequiredStableSampleCount,
                                recoveryRecord,
                                "Axis Power Off status-only proof");
                            stopResolvedByPowerOff = await
                                PrepareAxisCommandStopAfterStablePowerOffAsync(
                                    currentAxis,
                                    null,
                                    observed.FinalStatus,
                                    observed.StableSampleCount,
                                    observed.RequiredStableSampleCount,
                                    "Axis Power Off status-only proof");
                            return observed.FinalStatus;
                        }
                        catch (Exception error)
                        {
                            var evidence = GetAxisPowerOnWaitEvidence(error);
                            if (evidence != null
                                && evidence.LastObservedStatus != null)
                            {
                                DisplayAxisStatus(
                                    evidence.LastObservedStatus);
                                ObserveAxisPowerRecoveryStatus(
                                    currentAxis,
                                    recoveryRecord,
                                    evidence.LastObservedStatus,
                                    "Axis Power Off status-only recovery");
                            }
                            ReapplyCurrentAxisPowerRecoveryState(currentAxis);
                            throw;
                        }
                    },
                    safetyRequestGeneration,
                    false,
                    () =>
                    {
                        if (stopResolvedByPowerOff != null)
                        {
                            ResolveAxisCommandAfterStableProof(
                                stopResolvedByPowerOff,
                                AxisCommandRecoveryOperation.Stop,
                                "Axis Power Off retired Axis Stop");
                        }
                    });
                if (!HasActiveAxisPowerRecoveryRecord
                    && string.Equals(
                        TextOperationState.Text,
                        "Power Off verified",
                        StringComparison.Ordinal))
                {
                    TextAxisResult.Text += Environment.NewLine
                        + (statusOnlyAcceptedAcknowledgement
                            ? "Accepted Axis Power Off completed by status-only resume without replaying 0x2023."
                            : "Axis Power Off safe state was proved by status-only recovery without replaying 0x2023; no accepted ACK is claimed.");
                }
                return;
            }

            var powerOnToPowerOffTakeover = recoveryRecord != null
                && recoveryRecord.ExpectedPowerOn;
            var confirmedPowerOffReplacement = recoveryRecord != null
                && !recoveryRecord.ExpectedPowerOn
                && axisPowerOffReplacementAllowed;
            LMCSingleAxis sendingAxis = null;
            LMCAxisPowerOffWaitContinuation currentPowerOff = null;
            LMCAxisPowerOffWaitContinuation priorPowerOff = null;
            LMCAxisPowerOffWaitResult powerOffResult = null;
            AxisPowerOnRecoveryRecord verificationRecord = recoveryRecord;
            var powerCommandDispatchStarted = false;
            var acceptedBoundaryRecovery = false;
            AxisCommandRecoveryRecord stopResolvedByFreshPowerOff = null;
            var safetySend = await RunSafetyCommandAsync(
                "Power Off Send",
                async reservedGeneration =>
                {
                    sendingAxis = RequireAxis();
                    priorPowerOff = sendingAxis
                        .PendingPowerOffWaitContinuation;
                    try
                    {
                        var motionIdentityAlreadyRefreshed = motionMayBeActive;
                        await EnsureMotionRecoverySafetyDispatchIdentityAsync(
                            reservedGeneration,
                            MotionUncertaintyTargetKind.Axis,
                            sendingAxis.AxisName,
                            sendingAxis.AxisReference,
                            "Axis Power Off recovery");
                        verificationRecord = await
                            PrepareAxisPowerOffBeforeDispatchAsync(
                                sendingAxis,
                                recoveryRecord,
                                confirmedPowerOffReplacement,
                                motionIdentityAlreadyRefreshed,
                                "Axis Power Off");
                        powerCommandDispatchStarted = true;
                        var beginOverride = AxisPowerOffBeginAsyncOverride;
                        if (beginOverride == null)
                        {
                            currentPowerOff = await sendingAxis
                                .BeginPowerOffWaitForStableStateAsync(
                                    new LMCAxisPowerStateWaitOptions(),
                                    accepted =>
                                    {
                                        PersistAxisPowerOffAcceptedForRecord(
                                            accepted,
                                            verificationRecord,
                                            "Axis Power Off accepted observer");
                                        SupersedePendingGroupResetByMemberAxisMutation(
                                            sendingAxis,
                                            "Accepted captured-member Axis Power Off");
                                    },
                                    CancellationToken.None);
                        }
                        else
                        {
                            currentPowerOff = await beginOverride(
                                sendingAxis,
                                CancellationToken.None);
                        }

                        MarkAxisPowerOffAcceptedForRecord(
                            sendingAxis,
                            currentPowerOff,
                            verificationRecord,
                            "Axis Power Off accepted");
                        SupersedePendingGroupResetByMemberAxisMutation(
                            sendingAxis,
                            "Accepted captured-member Axis Power Off");
                        RecordMotionRecoverySafetyCommandAccepted(
                            reservedGeneration,
                            MotionUncertaintyTargetKind.Axis,
                            sendingAxis.AxisName,
                            sendingAxis.AxisReference,
                            "Axis Power Off");
                        TextAxisResult.Text = FormatResponse(
                            currentPowerOff.Acknowledgement);
                    }
                    catch (Exception error)
                    {
                        var evidence = GetAxisPowerOffWaitEvidence(error);
                        if (evidence != null
                            && evidence.LastObservedStatus != null)
                        {
                            DisplayAxisStatus(evidence.LastObservedStatus);
                        }
                        AppendAxisPowerOffWaitEvidence(
                            evidence,
                            "Axis Power Off send or accepted-boundary completion was not proven.");
                        if (evidence != null
                            && (evidence.SubmissionOutcome
                                    == LMCAxisPowerOffSubmissionOutcome.Accepted
                                || evidence.SubmissionOutcome
                                    == LMCAxisPowerOffSubmissionOutcome
                                        .OutcomeUncertain))
                        {
                            SupersedePendingGroupResetByMemberAxisMutation(
                                sendingAxis,
                                "Captured-member Axis Power Off accepted or outcome-uncertain dispatch");
                        }
                        PreserveAxisPowerOffWaitFailure(
                            sendingAxis,
                            error,
                            verificationRecord,
                            powerOnToPowerOffTakeover,
                            confirmedPowerOffReplacement,
                            powerCommandDispatchStarted,
                            priorPowerOff,
                            "Axis Power Off");
                        var boundaryContinuation =
                            GetAxisPowerOffWaitContinuation(error);
                        var acceptedAfterBoundaryFailure =
                            boundaryContinuation;
                        if (acceptedAfterBoundaryFailure == null
                            && sendingAxis != null)
                        {
                            acceptedAfterBoundaryFailure =
                                sendingAxis.PendingPowerOffWaitContinuation;
                        }
                        if (acceptedAfterBoundaryFailure != null
                            && acceptedAfterBoundaryFailure.IsPending
                            && (boundaryContinuation != null
                                || !ReferenceEquals(
                                    acceptedAfterBoundaryFailure,
                                    priorPowerOff))
                            && IsCurrentAxisPowerOperationRecord(
                                verificationRecord))
                        {
                            currentPowerOff = acceptedAfterBoundaryFailure;
                            pendingAxisPowerOffWaitContinuation =
                                acceptedAfterBoundaryFailure;
                            acceptedBoundaryRecovery = true;
                            RecordMotionRecoverySafetyCommandAccepted(
                                reservedGeneration,
                                MotionUncertaintyTargetKind.Axis,
                                sendingAxis.AxisName,
                                sendingAxis.AxisReference,
                                "Axis Power Off");
                            TextAxisResult.Text += Environment.NewLine
                                + "Power Off was accepted at the Begin boundary; continuing with status-only verification without replaying 0x2023.";
                            return;
                        }
                        throw;
                    }
                },
                () => CancelQualificationForExternalSafety(
                    "Axis Power Off",
                    false),
                true);

            if (!safetySend.Sent
                || sendingAxis == null
                || currentPowerOff == null)
            {
                ReleaseUnusedSafetyMonitorReservation(
                    safetySend,
                    "Power Off");
                return;
            }

            await RunSafetyMonitorAsync(
                "Power Off",
                sendingAxis,
                async () =>
                {
                    try
                    {
                        powerOffResult = await sendingAxis
                            .ResumePowerOffWaitForStableStateAsync(
                                currentPowerOff,
                                CancellationToken.None);
                    }
                    catch (Exception error)
                    {
                        var evidence = GetAxisPowerOffWaitEvidence(error);
                        if (evidence != null
                            && evidence.LastObservedStatus != null)
                        {
                            DisplayAxisStatus(evidence.LastObservedStatus);
                        }
                        AppendAxisPowerOffWaitEvidence(
                            evidence,
                            error is LMCAxisPowerOffInterferenceException
                                ? "Axis Power Off interference was confirmed. One later explicit Power Off Again is required."
                                : "Axis Power Off stable safe-state completion was not proven; 0x2023 is not replayed automatically.");
                        PreserveAxisPowerOffWaitFailure(
                            sendingAxis,
                            error,
                            verificationRecord,
                            powerOnToPowerOffTakeover,
                            confirmedPowerOffReplacement,
                            true,
                            priorPowerOff,
                            "Axis Power Off verification");
                        throw;
                    }

                    await CompleteAxisPowerRecoveryAfterStableProofAsync(
                        sendingAxis,
                        false,
                        powerOffResult.FinalStatus,
                        powerOffResult.StablePowerOffStandstillSampleCount,
                        powerOffResult.RequiredStableSampleCount,
                        verificationRecord,
                        "Axis Power Off stable proof");
                    stopResolvedByFreshPowerOff = await
                        PrepareAxisCommandStopAfterStablePowerOffAsync(
                            sendingAxis,
                            currentPowerOff,
                            powerOffResult.FinalStatus,
                            powerOffResult
                                .StablePowerOffStandstillSampleCount,
                            powerOffResult.RequiredStableSampleCount,
                            "Axis Power Off stable proof");
                    if (ReferenceEquals(
                        pendingAxisPowerOffWaitContinuation,
                        currentPowerOff))
                    {
                        pendingAxisPowerOffWaitContinuation = null;
                    }
                    return powerOffResult.FinalStatus;
                },
                safetySend.Generation,
                safetySend.MonitorReserved,
                () =>
                {
                    if (stopResolvedByFreshPowerOff != null)
                    {
                        ResolveAxisCommandAfterStableProof(
                            stopResolvedByFreshPowerOff,
                            AxisCommandRecoveryOperation.Stop,
                            "Axis Power Off retired Axis Stop");
                    }
                });

            if (powerOffResult != null
                && safetySend.Generation == safetyRequestGeneration)
            {
                AppendAxisPowerOffWaitEvidence(
                    powerOffResult.Evidence,
                    "Axis Power Off stable safe-state evidence was accepted without replaying 0x2023.");
                if (acceptedBoundaryRecovery)
                {
                    TextAxisResult.Text += Environment.NewLine
                        + "Begin deadline/cancellation boundary retained the accepted continuation; verification completed without replaying 0x2023.";
                }
            }
        }

        private async void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            var activeRecord = GetActiveAxisCommandRecoveryRecord();
            var acceptedRecovery = activeRecord != null
                && activeRecord.Operation == AxisCommandRecoveryOperation.Reset
                && activeRecord.State
                    == AxisCommandRecoveryState.AcceptedAwaitingProof;
            var explicitRetry = activeRecord != null
                && activeRecord.Operation == AxisCommandRecoveryOperation.Reset
                && activeRecord.State
                    == AxisCommandRecoveryState.RecoveryRequired;
            if (!acceptedRecovery
                && !explicitRetry
                && !CanStartLiveCommand("Reset"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Reset",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var options = new LMCAxisResetWaitOptions();
                    var verificationRecord =
                        GetActiveAxisCommandRecoveryRecord();
                    var currentReset = pendingAxisResetWaitContinuation;
                    var resumeSameSession = verificationRecord != null
                        && verificationRecord.Operation
                            == AxisCommandRecoveryOperation.Reset
                        && (verificationRecord.State
                                == AxisCommandRecoveryState.AcceptedAwaitingProof
                            || verificationRecord.State
                                == AxisCommandRecoveryState.RecoveryRequired)
                        && currentReset != null
                        && currentReset.IsPending
                        && !axisResetWaitInterferenceConfirmed;
                    var resumeAfterRestart = verificationRecord != null
                        && verificationRecord.Operation
                            == AxisCommandRecoveryOperation.Reset
                        && verificationRecord.State
                            == AxisCommandRecoveryState.AcceptedAwaitingProof
                        && !resumeSameSession;

                    try
                    {
                        if (!resumeSameSession && !resumeAfterRestart)
                        {
                            await SendLiveCommandAsync(
                                safetyGeneration,
                                "Reset Send",
                                async () =>
                                {
                                    verificationRecord = await
                                        PrepareAxisResetBeforeDispatchAsync(
                                            currentAxis,
                                            options);
                                    try
                                    {
                                        currentReset = await currentAxis
                                            .BeginResetWaitForStableErrorClearanceAsync(
                                                options,
                                                accepted =>
                                                    PersistAxisResetAccepted(
                                                        accepted,
                                                        verificationRecord),
                                                CancellationToken.None);
                                    }
                                    catch (Exception error)
                                    {
                                        var accepted =
                                            GetAxisResetWaitContinuation(error);
                                        if (accepted == null
                                            && !axisResetWaitInterferenceConfirmed)
                                        {
                                            accepted =
                                                GetExactPendingAxisResetContinuation(
                                                    currentAxis,
                                                    verificationRecord);
                                        }
                                        if (accepted != null
                                            && accepted.IsPending)
                                        {
                                            currentReset = accepted;
                                            pendingAxisResetWaitContinuation =
                                                accepted;
                                        }
                                        await PreserveAxisCommandDispatchFailureAsync(
                                            error,
                                            null,
                                            verificationRecord,
                                            currentAxis);
                                        throw;
                                    }
                                    return currentReset;
                                });
                            TextAxisResult.Text = FormatResponse(
                                    currentReset.Acknowledgement)
                                + Environment.NewLine
                                + "Reset ACK accepted exactly once; status-only 0x2028 verification is pending.";
                        }
                        else
                        {
                            await EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
                                currentAxis,
                                verificationRecord,
                                "Axis Reset status-only recovery",
                                resumeAfterRestart);
                            TextAxisResult.Text =
                                "Resuming accepted Axis Reset with status-only 0x2028 reads; 0x2024 will not be replayed.";
                        }

                        LMCReadStatusResult finalStatus;
                        int stableCount;
                        int requiredStableCount;
                        LMCAxisResetWaitEvidence evidence = null;
                        if (resumeAfterRestart)
                        {
                            var restarted = await currentAxis
                                .WaitForStableErrorClearanceAsync(
                                    options,
                                    CancellationToken.None);
                            finalStatus = restarted.FinalStatus;
                            stableCount = restarted.StableErrorClearSampleCount;
                            requiredStableCount =
                                restarted.RequiredStableSampleCount;
                            WriteLog(
                                "Axis Reset restart status-only polls="
                                + restarted.StatusPollCount
                                + ", Stable="
                                + restarted.StableErrorClearSampleCount
                                + "/"
                                + restarted.RequiredStableSampleCount
                                + ".");
                        }
                        else
                        {
                            var resetStopwatch =
                                System.Diagnostics.Stopwatch.StartNew();
                            var resumed = await currentAxis
                                .ResumeResetWaitForStableErrorClearanceAsync(
                                    currentReset,
                                    new LMCAxisResetWaitOptions
                                    {
                                        StableSampleCount = currentReset
                                            .RequiredStableSampleCount
                                    },
                                    CancellationToken.None,
                                    () => resetStopwatch.ElapsedMilliseconds,
                                    (delayMilliseconds, cancellationToken) =>
                                        Task.Delay(
                                            delayMilliseconds,
                                            cancellationToken),
                                    null,
                                    () =>
                                    {
                                        var hook =
                                            AxisResetAfterStatusPublicationTestHook;
                                        if (hook != null)
                                        {
                                            hook(currentReset);
                                        }
                                    });
                            finalStatus = resumed.FinalStatus;
                            stableCount = resumed.StableErrorClearSampleCount;
                            requiredStableCount =
                                resumed.RequiredStableSampleCount;
                            evidence = resumed.Evidence;
                        }
                        EnsureNoNewSafetyRequestBeforeResultApplication(
                            safetyGeneration,
                            "Reset status-only verification completion");
                        await EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
                            currentAxis,
                            verificationRecord,
                            "Axis Reset final proof");
                        if (finalStatus == null
                            || !finalStatus.IsSuccess
                            || finalStatus.ErrorId != 0
                            || finalStatus.AxisErrorId != 0
                            || stableCount < requiredStableCount)
                        {
                            throw new InvalidOperationException(
                                "Axis Reset stable error-clear proof is incomplete.");
                        }
                        ResolveAxisCommandAfterStableProof(
                            verificationRecord,
                            AxisCommandRecoveryOperation.Reset,
                            "Axis Reset stable error-clear proof");
                        DisplayAxisStatus(finalStatus);
                        AppendAxisResetWaitEvidence(
                            evidence,
                            "Axis Reset stable error-clear proof completed without replay.");
                    }
                    catch (Exception error)
                    {
                        PromoteAxisCommandAfterProofInterference(
                            verificationRecord,
                            error);
                        var evidence = GetAxisResetWaitEvidence(error);
                        if (evidence != null
                            && evidence.LastObservedStatus != null)
                        {
                            DisplayAxisStatus(evidence.LastObservedStatus);
                        }
                        AppendAxisResetWaitEvidence(
                            evidence,
                            error is LMCAxisResetInterferenceException
                                || error is LMCAxisStableErrorClearanceInterferenceException
                                ? "Axis Reset proof was invalidated by same-axis mutation. The durable record remains RecoveryRequired; no automatic 0x2024 replay is allowed."
                                : "Axis Reset completion was not proven; the durable accepted/recovery record remains active.");
                        throw;
                    }
                });
        }

        private async void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            var activeRecord = GetActiveAxisCommandRecoveryRecord();
            var exactPendingStop = GetExactPendingAxisStopContinuation(
                axis,
                activeRecord);
            if (activeRecord != null
                && activeRecord.Operation
                    == AxisCommandRecoveryOperation.Stop
                && (activeRecord.State
                        == AxisCommandRecoveryState.AcceptedAwaitingProof
                    || (activeRecord.State
                            == AxisCommandRecoveryState.RecoveryRequired
                        && exactPendingStop != null)))
            {
                var statusOnlyAxis = axis;
                var statusOnlyConnection = connection;
                if (statusOnlyAxis == null
                    || statusOnlyConnection == null
                    || !statusOnlyConnection.IsConnected)
                {
                    WriteLog(
                        "Axis Stop status-only verification requires the exact loaded recovery axis. No 0x2022 or 0x2028 was sent.");
                    return;
                }

                if (activeRecord.State
                        == AxisCommandRecoveryState.RecoveryRequired
                    && exactPendingStop != null)
                {
                    RecordMotionRecoverySafetyCommandAccepted(
                        safetyRequestGeneration,
                        MotionUncertaintyTargetKind.Axis,
                        statusOnlyAxis.AxisName,
                        statusOnlyAxis.AxisReference,
                        "Axis Stop same-session accepted recovery");
                }

                LMCAxisStopWaitResult resumedResult = null;
                LMCAxisStableStandstillWaitResult restartedResult = null;
                await RunSafetyMonitorAsync(
                    "Stop",
                    statusOnlyAxis,
                    async () =>
                    {
                        try
                        {
                            var continuation = exactPendingStop;
                            await EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
                                statusOnlyAxis,
                                activeRecord,
                                "Axis Stop status-only recovery",
                                continuation == null
                                    || !continuation.IsPending);
                            LMCReadStatusResult finalStatus;
                            int stableCount;
                            int requiredStableCount;
                            if (continuation != null
                                && continuation.IsPending
                                && ReferenceEquals(
                                    statusOnlyAxis.PendingStopWaitContinuation,
                                    continuation))
                            {
                                resumedResult = await statusOnlyAxis
                                    .ResumeStopWaitForStableStandstillAsync(
                                        continuation,
                                        CancellationToken.None);
                                finalStatus = resumedResult.FinalStatus;
                                stableCount = resumedResult
                                    .StableStandstillSampleCount;
                                requiredStableCount = resumedResult
                                    .RequiredStableSampleCount;
                            }
                            else
                            {
                                var options = new LMCAxisStopWaitOptions
                                {
                                    StableSampleCount = activeRecord
                                        .RequiredStableSampleCount
                                };
                                restartedResult = await statusOnlyAxis
                                    .WaitForStableStandstillAsync(
                                        options,
                                        CancellationToken.None);
                                finalStatus = restartedResult.FinalStatus;
                                stableCount = restartedResult
                                    .StableStandstillSampleCount;
                                requiredStableCount = restartedResult
                                    .RequiredStableSampleCount;
                            }
                            await EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
                                statusOnlyAxis,
                                activeRecord,
                                "Axis Stop final stable proof");
                            if (finalStatus == null
                                || !finalStatus.IsSuccess
                                || !finalStatus.IsStandstill
                                || stableCount < requiredStableCount)
                            {
                                throw new InvalidOperationException(
                                    "Axis Stop stable standstill proof is incomplete.");
                            }
                            return finalStatus;
                        }
                        catch (Exception error)
                        {
                            PromoteAxisCommandAfterProofInterference(
                                activeRecord,
                                error);
                            throw;
                        }
                    },
                    safetyRequestGeneration,
                    false,
                    () => ResolveAxisCommandAfterStableProof(
                        activeRecord,
                        AxisCommandRecoveryOperation.Stop,
                        "Axis Stop stable standstill proof"));
                if (resumedResult != null)
                {
                    AppendAxisStopWaitEvidence(
                        resumedResult.Evidence,
                        "Axis Stop stable standstill proof completed without replaying 0x2022.");
                }
                else if (restartedResult != null)
                {
                    WriteLog(
                        "Axis Stop restart status-only polls="
                        + restartedResult.StatusPollCount
                        + ", Stable="
                        + restartedResult.StableStandstillSampleCount
                        + "/"
                        + restartedResult.RequiredStableSampleCount
                        + "; 0x2022 replay count=0.");
                }
                return;
            }

            LMCSingleAxis currentAxis = null;
            LMCAxisStopWaitContinuation currentStop = null;
            LMCAxisStopWaitResult stopResult = null;
            LMCAxisStopWaitEvidence acceptedBeginBoundaryEvidence = null;
            AxisStopDispatchPreparation stopPreparation = null;
            var safetySend = await RunSafetyCommandAsync(
                "Stop Send",
                async reservedGeneration =>
                {
                    currentAxis = RequireAxis();
                    var currentConnection = RequireConnection();
                    var activeBeforePrepare =
                        GetActiveAxisCommandRecoveryRecord();
                    int deceleration;
                    int jerk;
                    int stableSampleCount;
                    if (activeBeforePrepare != null
                        && activeBeforePrepare.Operation
                            == AxisCommandRecoveryOperation.Stop
                        && activeBeforePrepare.State
                            == AxisCommandRecoveryState.RecoveryRequired)
                    {
                        deceleration = activeBeforePrepare.StopDeceleration;
                        jerk = activeBeforePrepare.StopJerk;
                        stableSampleCount = activeBeforePrepare
                            .RequiredStableSampleCount;
                    }
                    else
                    {
                        var input = ReadStopInput();
                        deceleration = input.DecelerationRaw;
                        jerk = input.JerkRaw;
                        stableSampleCount =
                            LMCAxisStopWaitOptions.DefaultStableSampleCount;
                    }
                    var options = new LMCAxisStopWaitOptions
                    {
                        StableSampleCount = stableSampleCount
                    };
                    if (activeBeforePrepare != null
                        && activeBeforePrepare.Operation
                            == AxisCommandRecoveryOperation.Reset)
                    {
                        stopPreparation =
                            PrepareAxisStopTakeoverBeforeSafetyAbort(
                                currentAxis,
                                deceleration,
                                jerk,
                                options);
                        try
                        {
                            currentAxis = await
                                AbortReconnectAndReloadAxisForStopTakeoverAsync(
                                    currentConnection,
                                    stopPreparation,
                                    reservedGeneration);
                        }
                        catch
                        {
                            await HandleAxisStopDefinitelyNotAttemptedAsync(
                                stopPreparation,
                                null);
                            EndAxisCommandSafetyReconnectOrchestration();
                            throw;
                        }
                    }
                    else
                    {
                        await EnsureMotionRecoverySafetyDispatchIdentityAsync(
                            reservedGeneration,
                            MotionUncertaintyTargetKind.Axis,
                            currentAxis.AxisName,
                            currentAxis.AxisReference,
                            "Axis Stop recovery");
                        stopPreparation = await
                            PrepareAxisStopBeforeDispatchAsync(
                                currentAxis,
                                deceleration,
                                jerk,
                                options);
                    }
                    try
                    {
                        var beforeBegin =
                            AxisStopBeforeBeginDispatchTestHook;
                        if (beforeBegin != null)
                        {
                            beforeBegin(
                                RequireConnection(),
                                stopPreparation.Record);
                        }
                        currentStop = await currentAxis
                            .BeginStopWaitForStableStandstillAsync(
                                deceleration,
                                jerk,
                                options,
                                accepted =>
                                {
                                    PersistAxisStopAccepted(
                                        accepted,
                                        stopPreparation.Record);
                                    SupersedePendingGroupResetByMemberAxisMutation(
                                        currentAxis,
                                        "Accepted captured-member Axis Stop");
                                },
                                CancellationToken.None);
                    }
                    catch (Exception error)
                    {
                        acceptedBeginBoundaryEvidence =
                            GetAxisStopWaitEvidence(error);
                        if (acceptedBeginBoundaryEvidence != null
                            && (acceptedBeginBoundaryEvidence.SubmissionOutcome
                                    == LMCAxisStopSubmissionOutcome.Accepted
                                || acceptedBeginBoundaryEvidence
                                    .SubmissionOutcome
                                    == LMCAxisStopSubmissionOutcome
                                        .OutcomeUncertain))
                        {
                            SupersedePendingGroupResetByMemberAxisMutation(
                                currentAxis,
                                "Captured-member Axis Stop accepted or outcome-uncertain dispatch");
                        }
                        currentStop = GetAxisStopWaitContinuation(error);
                        if (currentStop == null)
                        {
                            currentStop = GetExactPendingAxisStopContinuation(
                                currentAxis,
                                stopPreparation.Record);
                        }
                        CompleteAxisStopSafetyReplacementConnectionSetup(
                            stopPreparation);
                        await PreserveAxisCommandDispatchFailureAsync(
                            error,
                            stopPreparation,
                            null,
                            currentAxis);
                        EndAxisCommandSafetyReconnectOrchestration();
                        if (currentStop == null || !currentStop.IsPending)
                        {
                            AppendAxisStopWaitEvidence(
                                acceptedBeginBoundaryEvidence,
                                "Axis Stop acknowledgement was not accepted.");
                            throw;
                        }
                    }

                    CompleteAxisStopSafetyReplacementConnectionSetup(
                        stopPreparation);
                    pendingAxisStopWaitContinuation = currentStop;
                    SupersedePendingGroupResetByMemberAxisMutation(
                        currentAxis,
                        "Accepted captured-member Axis Stop");
                    RecordMotionRecoverySafetyCommandAccepted(
                        reservedGeneration,
                        MotionUncertaintyTargetKind.Axis,
                        currentAxis.AxisName,
                        currentAxis.AxisReference,
                        "Axis Stop");
                    TextAxisResult.Text = FormatResponse(
                        currentStop.Acknowledgement)
                        + Environment.NewLine
                        + "Stop ACK accepted exactly once; status-only 0x2028 "
                        + "verification is pending.";
                    if (acceptedBeginBoundaryEvidence != null)
                    {
                        AppendAxisStopWaitEvidence(
                            acceptedBeginBoundaryEvidence,
                            "Axis Stop ACK was accepted at the Begin deadline/cancellation boundary; continuing with status-only verification without replaying 0x2022.");
                    }
                },
                () => CancelQualificationForExternalSafety(
                    "Axis Stop",
                    false),
                true);

            if (!safetySend.Sent
                || currentAxis == null
                || currentStop == null)
            {
                ReleaseUnusedSafetyMonitorReservation(
                    safetySend,
                    "Stop");
                return;
            }

            await RunSafetyMonitorAsync(
                "Stop",
                currentAxis,
                async () =>
                {
                    try
                    {
                        stopResult = await currentAxis
                            .ResumeStopWaitForStableStandstillAsync(
                                currentStop,
                                CancellationToken.None);
                        await EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
                            currentAxis,
                            stopPreparation.Record,
                            "Axis Stop final stable proof");
                    }
                    catch (Exception error)
                    {
                        PromoteAxisCommandAfterProofInterference(
                            stopPreparation.Record,
                            error);
                        var evidence = GetAxisStopWaitEvidence(error);
                        if (evidence != null
                            && evidence.LastObservedStatus != null)
                        {
                            DisplayAxisStatus(evidence.LastObservedStatus);
                        }

                        AppendAxisStopWaitEvidence(
                            evidence,
                            "Axis Stop stable-standstill completion was not proven.");
                        throw;
                    }

                    WriteLog(
                        "Stop ACK accepted exactly once; status-only "
                        + "verification polls="
                        + stopResult.StatusPollCount
                        + ", Stable="
                        + stopResult.StableStandstillSampleCount
                        + "/"
                        + stopResult.RequiredStableSampleCount
                        + ".");
                    return stopResult.FinalStatus;
                },
                safetySend.Generation,
                safetySend.MonitorReserved,
                () => ResolveAxisCommandAfterStableProof(
                    stopPreparation.Record,
                    AxisCommandRecoveryOperation.Stop,
                    "Axis Stop stable standstill proof"));

            if (stopResult != null
                && safetySend.Generation == safetyRequestGeneration)
            {
                AppendAxisStopWaitEvidence(
                    stopResult.Evidence,
                    "Axis Stop stable-standstill evidence was accepted.");
            }
        }

        private async void ButtonMoveAbsolute_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartMotionCommand("Move Absolute"))
            {
                return;
            }

            await RunFiniteMotionAsync(
                "Move Absolute",
                true,
                safetyRequestGeneration,
                async (currentAxis, input) =>
                    await currentAxis.MoveAbsoluteExAsync(
                        input.PositionRaw,
                        input.VelocityRaw,
                        input.AccelerationRaw,
                        input.DecelerationRaw,
                        input.JerkRaw,
                        LMC_DIRECTION.Shortest,
                        CancellationToken.None));
        }

        private async void ButtonMoveRelative_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartMotionCommand("Move Relative"))
            {
                return;
            }

            await RunFiniteMotionAsync(
                "Move Relative",
                false,
                safetyRequestGeneration,
                async (currentAxis, input) =>
                    await currentAxis.MoveRelativeExAsync(
                        input.PositionRaw,
                        input.VelocityRaw,
                        input.AccelerationRaw,
                        input.DecelerationRaw,
                        input.JerkRaw,
                        LMC_DIRECTION.Shortest,
                        CancellationToken.None));
        }

        private async void ButtonMoveVelocity_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartMotionCommand("Move Velocity"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Move Velocity",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var input = ReadVelocityMotionInput();
                    await EnsureAxisPoweredOnAsync(currentAxis);

                    var response = await DispatchTrackedMotionAsync(
                        safetyGeneration,
                        MotionUncertaintyTargetKind.Axis,
                        currentAxis.AxisName,
                        currentAxis.AxisReference,
                        "Move Velocity",
                        null,
                        async () => await currentAxis.MoveVelocityExAsync(
                                input.VelocityRaw,
                                input.AccelerationRaw,
                                0,
                                input.JerkRaw,
                                input.Direction,
                                CancellationToken.None));

                    ClearMotionOnConfirmedRejection(
                        currentAxis.AxisName,
                        "Move Velocity",
                        response);
                    EnsureResponseSuccess("Move Velocity", response);
                    RequireExplicitMotionRecoverySafety(
                        "Move Velocity requires an explicit Stop or PowerOff");

                    TextAxisResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Motion remains active until Stop or PowerOff is verified.";
                    WriteLog(
                        "SAFETY: Move Velocity accepted. Use Stop or PowerOff; "
                        + "Close is blocked until standstill is verified.");
                });
        }

        private async Task RunFiniteMotionAsync(
            string operation,
            bool absolute,
            long safetyGeneration,
            Func<LMCSingleAxis, MotionInput, Task<LMC_Response>> send)
        {
            LMCSingleAxis monitoredAxis = null;
            var trackingGeneration = 0;
            var noMovementExpected = false;

            await RunOperationAsync(
                operation + " Send",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var input = ReadFiniteMotionInput();
                    await EnsureAxisPoweredOnAsync(currentAxis);

                    var startPosition = await currentAxis
                        .GetActualPositionResultAsync(CancellationToken.None);
                    EnsureAxisPositionSuccess(
                        operation + " start position",
                        startPosition);
                    noMovementExpected = absolute
                        ? startPosition.PositionRaw == input.PositionRaw
                        : input.PositionRaw == 0;
                    monitoredAxis = currentAxis;

                    var response = await DispatchTrackedMotionAsync(
                        safetyGeneration,
                        MotionUncertaintyTargetKind.Axis,
                        currentAxis.AxisName,
                        currentAxis.AxisReference,
                        operation,
                        generation => trackingGeneration = generation,
                        async () => await send(currentAxis, input));
                    ClearMotionOnConfirmedRejection(
                        currentAxis.AxisName,
                        operation,
                        response);
                    EnsureResponseSuccess(operation, response);

                    TextAxisResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Command accepted; monitoring for movement and stable standstill.";
                });

            if (monitoredAxis == null
                || trackingGeneration == 0
                || !IsTrackedMotion(
                    monitoredAxis.AxisName,
                    trackingGeneration))
            {
                return;
            }

            await MonitorFiniteMotionAsync(
                operation,
                monitoredAxis,
                trackingGeneration,
                noMovementExpected);
        }

        private async Task MonitorFiniteMotionAsync(
            string operation,
            LMCSingleAxis monitoredAxis,
            int trackingGeneration,
            bool noMovementExpected)
        {
            WriteLog(
                operation
                + " monitor started. Stop and PowerOff remain available.");
            TextOperationState.Text = operation + " monitoring";
            UpdateUiState();

            try
            {
                var status = await WaitForFiniteMotionCompletionAsync(
                    monitoredAxis,
                    trackingGeneration,
                    noMovementExpected,
                    15000);
                if (status == null)
                {
                    WriteLog(
                        operation
                        + " monitor ended because another safety action cleared tracking.");
                    return;
                }

                DisplayAxisStatus(status);
                await ClearMotionWarningAfterVerifiedStateAsync(
                    operation + " completed at stable standstill",
                    trackingGeneration);
                WriteLog(operation + " completion PASS.");
                TextOperationState.Text = operation + " completed";
            }
            catch (Exception error)
            {
                if (IsTrackedMotion(
                    monitoredAxis.AxisName,
                    trackingGeneration))
                {
                    RequireExplicitMotionRecoverySafety(
                        operation + " automatic completion monitor failed");
                    WriteLog(
                        operation
                        + " monitor FAILED: "
                        + error.Message
                        + " Stop or PowerOff is still required.");
                    TextOperationState.Text = operation + " monitor failed";
                }
                else
                {
                    WriteLog(
                        operation
                        + " monitor ended after the tracked motion was cleared: "
                        + error.Message);
                }
            }
            finally
            {
                UpdateUiState();
            }
        }

        private async Task MonitorGroupFiniteMotionAsync(
            string operation,
            LMCGroupAxis monitoredGroup,
            int trackingGeneration,
            bool noMovementExpected,
            int timeoutMilliseconds)
        {
            WriteLog(
                operation
                + " group monitor started with timeout "
                + timeoutMilliseconds
                + " ms. Group Stop remains available.");
            TextOperationState.Text =
                operation
                + " monitoring (limit "
                + (timeoutMilliseconds / 1000.0).ToString(
                    "0.###",
                    CultureInfo.InvariantCulture)
                + " s)";
            UpdateUiState();

            try
            {
                var status = await WaitForGroupMotionCompletionAsync(
                    monitoredGroup,
                    trackingGeneration,
                    noMovementExpected,
                    timeoutMilliseconds);
                if (status == null)
                {
                    WriteLog(
                        operation
                        + " monitor ended because a safety action cleared tracking.");
                    return;
                }

                DisplayGroupStatus(status);
                await ClearMotionWarningAfterVerifiedStateAsync(
                    operation + " completed at stable Group InPosition",
                    trackingGeneration);
                WriteLog(operation + " completion PASS.");
                TextOperationState.Text = operation + " completed";
            }
            catch (Exception error)
            {
                if (IsTrackedMotion(
                    monitoredGroup.GroupName,
                    trackingGeneration))
                {
                    RequireExplicitMotionRecoverySafety(
                        operation + " automatic completion monitor failed");
                    WriteLog(
                        operation
                        + " monitor FAILED: "
                        + error.Message
                        + " Group Stop is still required.");
                    TextOperationState.Text = operation + " monitor failed";
                }
                else
                {
                    WriteLog(
                        operation
                        + " monitor ended after tracked motion was cleared: "
                        + error.Message);
                }
            }
            finally
            {
                UpdateUiState();
            }
        }

        private async void ButtonGetMembers_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Get Group Members",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    var inspectionOnly =
                        IsRecoveryIdentityReadOnlyConnection(currentConnection);
                    var currentGroup = inspectionOnly
                        ? await CreateReadOnlyInspectionGroupAsync(
                            currentConnection,
                            "Get Group Members")
                        : RequireGroup();
                    var result = await currentGroup
                        .GetGroupMembersInfoResultAsync(CancellationToken.None);
                    EnsureGroupMembersSuccess("Get Group Members", result);
                    TextGroupResult.Text = FormatGroupMembers(result);
                    if (inspectionOnly)
                    {
                        WriteLog(
                            "READ-ONLY INSPECTION: Group members read without "
                            + "changing application or durable recovery state. Name="
                            + currentGroup.GroupName
                            + ", Ref="
                            + currentGroup.GroupReference);
                    }
                });
        }

        private static string FormatGroupMembers(
            LMCGroupMembersInfoResult result)
        {
            var memberLines = result.Members.Select(
                member =>
                    "["
                    + member.Index
                    + "] Name="
                    + member.AxisName
                    + ", Ref="
                    + member.AxisReference
                    + ", DeviceId="
                    + member.DeviceId);
            return "AxisCount="
                + result.AxisCount
                + Environment.NewLine
                + string.Join(Environment.NewLine, memberLines);
        }

        private async void ButtonGroupReadStatus_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Group Status",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    var inspectionOnly =
                        IsRecoveryIdentityReadOnlyConnection(currentConnection);
                    var currentGroup = inspectionOnly
                        ? await CreateReadOnlyInspectionGroupAsync(
                            currentConnection,
                            "Read Group Status")
                        : RequireGroup();
                    var statusSafetyGeneration = safetyRequestGeneration;
                    var result = await currentGroup.GroupReadStatusResultAsync(
                        CancellationToken.None);
                    if (inspectionOnly)
                    {
                        EnsureGroupStatusReadSuccess("Read Group Status", result);
                        DisplayGroupStatus(result);
                        WriteLog(
                            "READ-ONLY INSPECTION: Group status read without "
                            + "changing application or durable recovery state. Name="
                            + currentGroup.GroupName
                            + ", Ref="
                            + currentGroup.GroupReference);
                        return;
                    }

                    EnsureNoNewSafetyRequestBeforeResultApplication(
                        statusSafetyGeneration,
                        "Read Group Status result application");
                    if (result == null || !result.IsSuccess)
                    {
                        InvalidateGroupPreparationAfterStatusFailure();
                    }
                    EnsureGroupStatusSuccess("Read Group Status", result);
                    groupStatusRefreshRequired = false;
                    DisplayGroupStatus(result);
                    if (groupResetObservedLockedStandby
                        && !(result.IsPowerOn && result.IsStandby))
                    {
                        groupResetObservedLockedStandby = false;
                        WriteLog(
                            "The post-Reset LockedStandby observation is no "
                            + "longer current; its fresh safe Disable path was "
                            + "cleared.");
                    }
                    ObserveGroupPowerRecoveryStatus(
                        currentGroup,
                        result,
                        "Read Group Status");

                    if (result.IsPowerOn)
                    {
                        if (groupPowerOffVerificationPending)
                        {
                            WriteLog(
                                "Group Power Off was accepted/start only, but Power "
                                + "Ready is still reported. Resume Power Off Verification "
                                + "to require three consecutive PowerOn=False samples.");
                        }
                        else if (groupPowerVerificationPending)
                        {
                            WriteLog(
                                "One PowerOn=True sample was observed, but Power Ready is "
                                + "not verified. Resume Power On Verification to require "
                                + "three consecutive samples without replaying 0x204A.");
                        }
                        else
                        {
                            if (!groupActiveVerified)
                            {
                                WriteLog(
                                    "One PowerOn=True sample was observed outside a pending "
                                    + "Power On command. It is not promoted to ACTIVE; the "
                                    + "three-sample power-state verification is required.");
                            }
                        }
                    }
                    else
                    {
                        var powerOffWasPending =
                            groupPowerOffVerificationPending;
                        if (groupActiveVerified
                            || groupIdentityHomeCheckComplete
                            || groupIdentityConfigured
                            || groupProfileLockVerificationPending
                            || groupProfileLocked)
                        {
                            WriteLog(
                                "Group status no longer reports PowerOn. The local "
                                + "Home, identity, and profile-lock state was cleared.");
                        }

                        groupActiveVerified = false;
                        groupIdentityConfigured = false;
                        ResetIdentityHomeCheckState();
                        groupProfileLocked = false;
                        if (powerOffWasPending)
                        {
                            WriteLog(
                                "One PowerOn=False sample was observed, but Group Power Off "
                                + "is still pending. Three consecutive samples are required; "
                                + "resume verification without replaying 0x204B.");
                        }
                        if (groupPowerVerificationPending)
                        {
                            WriteLog(
                                "Group Power On was accepted/start only, but Power "
                                + "Ready is not verified yet. Resume Power On Verification "
                                + "without replaying 0x204A.");
                        }
                    }

                    if (result.IsPowerOn && result.IsStandby)
                    {
                        var continuation =
                            GetPendingGroupEnableWaitContinuation(currentGroup);
                        if (continuation != null && continuation.IsPending)
                        {
                            if (continuation.StableSampleCount
                                >= continuation.RequiredStableSampleCount)
                            {
                                try
                                {
                                    var completed = await currentGroup
                                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                                            continuation,
                                            CancellationToken.None);
                                    if (statusSafetyGeneration
                                        != safetyRequestGeneration)
                                    {
                                        MarkGroupProfileLockResultDiscarded(
                                            "Read Group Status lock completion");
                                    }
                                    EnsureNoNewSafetyRequestBeforeResultApplication(
                                        statusSafetyGeneration,
                                        "Read Group Status lock completion");
                                    await EnsureGroupProfileLockRecoveryIdentityAsync(
                                        currentGroup,
                                        "Read Group Status lock completion",
                                        true);
                                    if (statusSafetyGeneration
                                        != safetyRequestGeneration)
                                    {
                                        MarkGroupProfileLockResultDiscarded(
                                            "Read Group Status post-identity completion");
                                    }
                                    EnsureNoNewSafetyRequestBeforeResultApplication(
                                        statusSafetyGeneration,
                                        "Read Group Status post-identity completion");
                                    CompleteGroupEnableWaitUi(completed);
                                }
                                catch
                                {
                                    if (HasActiveGroupProfileLockRecoveryJournalRecord
                                        && currentGroup
                                            .PendingGroupEnableWaitContinuation == null
                                        && !groupProfileLockRecoveryRequired)
                                    {
                                        MarkGroupProfileLockCompletionOutcomeUncertain(
                                            "Read Group Status lock completion");
                                    }

                                    throw;
                                }
                            }
                            else
                            {
                                groupProfileLockVerificationPending = true;
                                groupProfileLocked = false;
                                WriteLog(
                                    "Group Lock Ready sample observed ("
                                    + continuation.StableSampleCount
                                    + "/"
                                    + continuation.RequiredStableSampleCount
                                    + "). Three consecutive stable samples are required.");
                            }
                        }
                        else
                        {
                            pendingGroupEnableWaitContinuation = null;
                            groupProfileLockVerificationPending =
                                groupProfileLockAcceptedRestartRecovery;
                            if (!groupProfileLocked)
                            {
                                WriteLog(
                                    "One Enabled/Locked Standby sample was observed "
                                    + (groupProfileLockAcceptedRestartRecovery
                                        ? "for a durable accepted Group Enable. It is "
                                            + "not final proof; use Resume Lock "
                                            + "Verification for three consecutive "
                                            + "status-only samples."
                                        : "without a pending Group Enable. It is not "
                                            + "promoted to a verified profile lock; "
                                            + "three consecutive samples are required."));
                            }
                        }
                        if (!groupIdentityConfigured)
                        {
                            WriteLog(
                                "Group status reports Enabled/Locked Standby, but "
                                + "this session did not configure the identity. "
                                + "Unlock, configure identity, then lock again.");
                        }
                    }
                    else if (result.IsDisabled)
                    {
                        var externalUnlockObserved = groupProfileLocked
                            && !groupProfileUnlockVerificationPending;
                        if (!groupProfileUnlockVerificationPending)
                        {
                            groupProfileLocked = false;
                        }
                        if (externalUnlockObserved)
                        {
                            WriteLog(
                                "Group status reports Disabled/Unlocked; the local "
                                + "profile-lock state was cleared.");
                        }
                        else if (groupProfileUnlockVerificationPending)
                        {
                            WriteLog(
                                "One Disabled/Unlocked sample was observed for a "
                                + "pending Group Disable. Durable resolve and the "
                                + "volatile unlock state still require three "
                                + "consecutive stable samples.");
                        }
                        await ReconcilePendingGroupEnableSafeStateProofAsync(
                            currentGroup,
                            statusSafetyGeneration,
                            "Disabled/Unlocked");
                    }
                    else
                    {
                        await ReconcilePendingGroupEnableSafeStateProofAsync(
                            currentGroup,
                            statusSafetyGeneration,
                            result.IsPowerOn
                                ? "transitional powered state"
                                : "PowerOn=False");
                    }

                    if (groupProfileLockVerificationPending
                        && !(result.IsPowerOn && result.IsStandby))
                    {
                        WriteLog(
                            "Group Enable was accepted, but Lock Ready is not "
                            + "verified yet. Resume verification or run Read Status until "
                            + "Enabled/Locked Standby=True.");
                    }

                    UpdateUiState();

                    if (!IsTrackedMotionAxis(currentGroup.GroupName))
                    {
                        return;
                    }

                    if (!result.IsPowerOn)
                    {
                        await RunGroupPowerOffSafetyMonitorAsync(
                            "Read Group Status Power Off recovery",
                            currentGroup,
                            safetyRequestGeneration,
                            false,
                            HasActiveGroupPowerRecoveryRecord
                                ? groupPowerRecoveryJournal.CurrentRecord
                                : null);
                        return;
                    }

                    if (!IsGroupInPosition(result))
                    {
                        RecordMotionObserved(currentGroup.GroupName);
                        return;
                    }

                    if (motionWasObserved)
                    {
                        var verified = await WaitForGroupInPositionAsync(
                            currentGroup,
                            750,
                            0);
                        DisplayGroupStatus(verified);
                        await ClearMotionWarningAfterVerifiedStateAsync(
                            "Read Group Status verified three stable in-position samples");
                        return;
                    }

                    WriteLog(
                        "SAFETY: Group InPosition was reported, but motion has not "
                        + "yet been observed. The motion warning remains active; "
                        + "use Group Stop to establish a known stopped state.");
                });
        }

        private async void ButtonGroupReadPosition_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Group Position",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    var inspectionOnly =
                        IsRecoveryIdentityReadOnlyConnection(currentConnection);
                    var currentGroup = inspectionOnly
                        ? await CreateReadOnlyInspectionGroupAsync(
                            currentConnection,
                            "Read Group Position")
                        : RequireGroup();
                    var coordinateSystem = ReadGroupPositionCoordinateSystem();
                    var unit = ReadGroupUnitSelection();
                    var result = await currentGroup
                        .GroupReadActualPositionAsync(
                            coordinateSystem,
                            CancellationToken.None);
                    EnsureGroupPositionSuccess(
                        "Read Group Position",
                        result);
                    DisplayGroupPosition(result, unit);
                    if (inspectionOnly)
                    {
                        WriteLog(
                            "READ-ONLY INSPECTION: Group position read without "
                            + "changing application or durable recovery state. Name="
                            + currentGroup.GroupName
                            + ", Ref="
                            + currentGroup.GroupReference);
                    }
                });
        }

        private async Task<LMCGroupAxis> CreateReadOnlyInspectionGroupAsync(
            LMCConnection currentConnection,
            string operation)
        {
            if (!IsRecoveryIdentityReadOnlyConnection(currentConnection))
            {
                throw new InvalidOperationException(
                    operation
                    + " requested a transient inspection Group outside the "
                    + "recovery-identity read-only quarantine.");
            }

            var objectName = RequiredText(
                TextGroupName.Text,
                "Group object name");
            var inspectedGroup = await LMCGroupAxis.CreateAsync(
                currentConnection,
                objectName,
                CancellationToken.None);
            TextGroupReference.Text = inspectedGroup.GroupReference.ToString(
                CultureInfo.InvariantCulture);
            return inspectedGroup;
        }

        private async void ButtonGroupPowerOn_Click(
            object sender,
            RoutedEventArgs e)
        {
            var recoveryRecord = HasActiveGroupPowerRecoveryRecord
                ? groupPowerRecoveryJournal.CurrentRecord
                : null;
            var exactPendingPowerOn =
                pendingGroupPowerStateWaitContinuation != null
                && pendingGroupPowerStateWaitContinuation.IsPending
                && pendingGroupPowerStateWaitContinuation.ExpectedPowerOn
                && group != null
                && ReferenceEquals(
                    group.PendingGroupPowerStateWaitContinuation,
                    pendingGroupPowerStateWaitContinuation);
            var resumeAcceptedPowerOn = recoveryRecord != null
                && recoveryRecord.ExpectedPowerOn
                && (recoveryRecord.State
                        == GroupPowerRecoveryState.AcceptedAwaitingProof
                    || exactPendingPowerOn)
                && !groupPowerRecoveryRequired;
            var operation = resumeAcceptedPowerOn
                ? "Resume Group Power On Verification"
                : "Group Power On";
            if (recoveryRecord != null
                && recoveryRecord.ExpectedPowerOn
                && recoveryRecord.State
                    == GroupPowerRecoveryState.RecoveryRequired)
            {
                WriteLog(
                    "Group Power On replay is blocked because its dispatch "
                    + "outcome is uncertain. Send Group Power Off explicitly "
                    + "and verify stable PowerOn=false instead.");
                return;
            }

            if (recoveryRecord != null && !recoveryRecord.ExpectedPowerOn)
            {
                WriteLog(
                    "Group Power On is blocked while Group Power Off recovery "
                    + "is unresolved. Complete stable PowerOn=false proof first.");
                return;
            }

            if (!resumeAcceptedPowerOn
                && (groupProfileLockRecoveryRequired
                    || HasPendingGroupProfileLockContinuation()
                    || HasActiveGroupProfileLockRecoveryJournalRecord))
            {
                WriteLog(
                    "Group Power On is blocked while Group Enable is pending or "
                    + "the profile-lock result is uncertain. Resume lock verification, "
                    + "run Disable, or complete stable Power Off verification before "
                    + "a new 0x204A is allowed.");
                return;
            }

            if (!resumeAcceptedPowerOn
                && !CanStartLiveCommand(operation))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                operation,
                async () =>
                {
                    var currentGroup = RequireGroup();
                    GroupPowerRecoveryRecord verificationRecord =
                        resumeAcceptedPowerOn ? recoveryRecord : null;
                    var powerCommandDispatchStarted = false;
                    try
                    {
                        if (!resumeAcceptedPowerOn)
                        {
                            ResetGroupPreparationState();
                            var continuation = await SendLiveCommandAsync(
                                safetyGeneration,
                                "Group Power On",
                                async () =>
                                {
                                    verificationRecord = await
                                        ArmGroupPowerRecoveryBeforeDispatchAsync(
                                            currentGroup,
                                            true);
                                    powerCommandDispatchStarted = true;
                                    return await currentGroup
                                        .BeginGroupPowerOnWaitForStableStateAsync(
                                        new LMCGroupPowerStateWaitOptions(),
                                        accepted => MarkGroupPowerAccepted(
                                            currentGroup,
                                            accepted,
                                            "Group Power On accepted observer"),
                                        CancellationToken.None);
                                });
                            pendingGroupPowerStateWaitContinuation =
                                continuation;
                            TextGroupResult.Text =
                                FormatResponse(continuation.Acknowledgement)
                                + Environment.NewLine
                                + "Power On accepted once; automatic stable-state "
                                + "verification is running.";
                            WriteLog(
                                "Group Power On accepted/start only. Verifying "
                                + "three consecutive PowerOn=True samples before "
                                + "ACTIVE is trusted.");
                        }
                        else
                        {
                            WriteLog(
                                "Resuming Group Power On verification with 0x2045 "
                                + "status reads only; no 0x204A replay is allowed.");
                        }

                        var result = await ResumeOrObserveGroupPowerStateAsync(
                            currentGroup,
                            true,
                            operation);
                        EnsureNoNewSafetyRequestBeforeResultApplication(
                            safetyGeneration,
                            operation + " completion");
                        CompleteGroupPowerRecoveryAfterStableProof(
                            currentGroup,
                            true,
                            result,
                            verificationRecord,
                            operation + " stable Power On proof");
                        CompleteGroupPowerOnWaitUi(
                            result,
                            resumeAcceptedPowerOn);
                    }
                    catch (Exception error)
                    {
                        PreserveGroupPowerWaitFailure(
                            currentGroup,
                            error,
                            verificationRecord,
                            false,
                            false,
                            powerCommandDispatchStarted,
                            null,
                            operation);
                        throw;
                    }
                });
        }

        private async void ButtonGroupPowerOff_Click(
            object sender,
            RoutedEventArgs e)
        {
            var recoveryRecord = HasActiveGroupPowerRecoveryRecord
                ? groupPowerRecoveryJournal.CurrentRecord
                : null;
            var statusOnlyPowerOff = recoveryRecord != null
                && !recoveryRecord.ExpectedPowerOn
                && !groupPowerOffReplacementAllowed;
            if (statusOnlyPowerOff)
            {
                var pendingGroup = group;
                var currentConnection = connection;
                if (pendingGroup == null
                    || currentConnection == null
                    || !currentConnection.IsConnected)
                {
                    WriteLog(
                        "Group Power Off status-only verification is blocked "
                        + "until the exact recovery group is loaded on a live "
                        + "connection. No 0x204B or status RPC was sent.");
                    TextOperationState.Text =
                        "Resume Group Power Off Verification blocked";
                    UpdateUiState();
                    return;
                }

                WriteLog(
                    "Resuming Group Power Off verification with 0x2045 status "
                    + "reads only; no 0x204B replay is allowed.");
                await RunGroupPowerOffSafetyMonitorAsync(
                    "Resume Group Power Off Verification",
                    pendingGroup,
                    safetyRequestGeneration,
                    false,
                    recoveryRecord);
                return;
            }

            var powerOnToPowerOffTakeover = recoveryRecord != null
                && recoveryRecord.ExpectedPowerOn;
            var confirmedPowerOffReplacement = recoveryRecord != null
                && !recoveryRecord.ExpectedPowerOn
                && groupPowerOffReplacementAllowed;

            LMCGroupAxis currentGroup = null;
            GroupPowerRecoveryRecord verificationRecord = recoveryRecord;
            var powerCommandDispatchStarted = false;
            LMCGroupPowerStateWaitContinuation priorPowerContinuation = null;
            var safetySend = await RunSafetyCommandAsync(
                "Group Power Off Send",
                async reservedGeneration =>
                {
                    currentGroup = RequireGroup();
                    priorPowerContinuation = currentGroup
                        .PendingGroupPowerStateWaitContinuation;
                    try
                    {
                        if (groupProfileLockRecoveryRequired
                            || HasActiveGroupProfileLockRecoveryJournalRecord)
                        {
                            await EnsureGroupProfileLockRecoveryIdentityAsync(
                                currentGroup,
                                "Group Power Off recovery",
                                RequireActiveGroupProfileLockRecoveryRecord(
                                    "Group Power Off recovery")
                                    .ExpectedProfileLocked);
                        }
                        await EnsureMotionRecoverySafetyDispatchIdentityAsync(
                            reservedGeneration,
                            MotionUncertaintyTargetKind.Group,
                            currentGroup.GroupName,
                            currentGroup.GroupReference,
                            "Group Power Off motion recovery");

                        if (powerOnToPowerOffTakeover)
                        {
                            verificationRecord = await
                                ReplaceUncertainGroupPowerOnWithPowerOffBeforeDispatchAsync(
                                    currentGroup,
                                    "Group Power Off safety takeover");
                        }
                        else if (confirmedPowerOffReplacement)
                        {
                            verificationRecord = await
                                PrepareConfirmedGroupPowerOffReplacementAsync(
                                    currentGroup,
                                    "Group Power Off confirmed replacement");
                        }
                        else
                        {
                            verificationRecord = await
                                ArmGroupPowerRecoveryBeforeDispatchAsync(
                                    currentGroup,
                                    false);
                        }

                        powerCommandDispatchStarted = true;
                        var continuation = await currentGroup
                            .BeginGroupPowerOffWaitForStableStateAsync(
                                new LMCGroupPowerStateWaitOptions(),
                                accepted =>
                                {
                                    MarkGroupPowerAccepted(
                                        currentGroup,
                                        accepted,
                                        "Group Power Off accepted observer");
                                    SupersedePendingGroupResetByLaterMutation(
                                        "Accepted Group Power Off");
                                },
                                CancellationToken.None);
                        pendingGroupPowerStateWaitContinuation = continuation;
                        RecordMotionRecoverySafetyCommandAccepted(
                            reservedGeneration,
                            MotionUncertaintyTargetKind.Group,
                            currentGroup.GroupName,
                            currentGroup.GroupReference,
                            "Group Power Off");
                        pendingGroupEnableWaitContinuation =
                            currentGroup.PendingGroupEnableWaitContinuation;
                        groupProfileLockVerificationPending =
                            groupProfileLockAcceptedRestartRecovery
                            || pendingGroupEnableWaitContinuation != null;
                        TextGroupResult.Text =
                            FormatResponse(continuation.Acknowledgement)
                            + Environment.NewLine
                            + "Power Off accepted once; automatic stable-state "
                            + "verification is running.";
                        WriteLog(
                            "Group Power Off accepted/start only. Verifying "
                            + "three consecutive PowerOn=False samples before "
                            + "safe state is trusted.");
                    }
                    catch (Exception error)
                    {
                        var powerEvidence = GetGroupPowerWaitEvidence(error);
                        PreserveGroupPowerWaitFailure(
                            currentGroup,
                            error,
                            verificationRecord,
                            powerOnToPowerOffTakeover,
                            confirmedPowerOffReplacement,
                            powerCommandDispatchStarted,
                            priorPowerContinuation,
                            "Group Power Off");
                        if (powerEvidence != null
                            && (powerEvidence.SubmissionOutcome
                                    == LMCGroupPowerSubmissionOutcome.Accepted
                                || powerEvidence.SubmissionOutcome
                                    == LMCGroupPowerSubmissionOutcome
                                        .OutcomeUncertain))
                        {
                            SupersedePendingGroupResetByLaterMutation(
                                "Group Power Off dispatch outcome");
                        }

                        throw;
                    }
                },
                () => CancelQualificationForExternalSafety(
                    "Group Power Off",
                    true),
                true);

            if (safetySend.Sent && currentGroup != null)
            {
                await RunGroupPowerOffSafetyMonitorAsync(
                    "Group Power Off",
                    currentGroup,
                    safetySend.Generation,
                    safetySend.MonitorReserved,
                    verificationRecord);
            }
            else
            {
                ReleaseUnusedSafetyMonitorReservation(
                    safetySend,
                    "Group Power Off");
            }
        }

        private async void ButtonGroupEnable_Click(object sender, RoutedEventArgs e)
        {
            var acceptedRestartRecovery =
                HasAcceptedGroupProfileLockRecoveryRecord;
            if (groupProfileLockRecoveryRequired
                || (HasActiveGroupProfileLockRecoveryJournalRecord
                    && !HasPendingGroupProfileLockContinuation()
                    && !acceptedRestartRecovery))
            {
                WriteLog(
                    "Group Enable is blocked because a completed lock result was "
                    + "discarded after a newer safety request. Run Disable or "
                    + "complete stable Power Off verification before a new "
                    + "0x2047 is allowed.");
                return;
            }

            var resumeAcceptedEnable =
                pendingGroupEnableWaitContinuation != null
                    && pendingGroupEnableWaitContinuation.IsPending
                || acceptedRestartRecovery;
            var operation = resumeAcceptedEnable
                ? "Resume Group Lock Verification"
                : "Group Enable (Lock Profile)";
            if (!CanStartLiveCommand(operation))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                operation,
                async () =>
                {
                    var currentGroup = RequireGroup();
                    var continuation =
                        GetPendingGroupEnableWaitContinuation(currentGroup);
                    var statusOnlyRestartRecovery = continuation == null
                        && HasAcceptedGroupProfileLockRecoveryRecord;
                    if (!statusOnlyRestartRecovery)
                    {
                        EnsureGroupActiveVerified();
                        if (!groupIdentityConfigured)
                        {
                            throw new InvalidOperationException(
                                "Set Identity (Configure) before Enable "
                                + "(Lock Profile).");
                        }
                    }

                    var freshEnableAttempt = continuation == null
                        && !statusOnlyRestartRecovery;
                    if (freshEnableAttempt)
                    {
                        await ArmGroupProfileLockRecoveryBeforeEnableAsync(
                            currentGroup);
                    }

                    try
                    {
                        if (statusOnlyRestartRecovery)
                        {
                            await EnsureGroupProfileLockRecoveryIdentityAsync(
                                currentGroup,
                                operation + " pre-status identity",
                                true);
                            EnsureNoNewSafetyRequestBeforeResultApplication(
                                safetyGeneration,
                                operation + " pre-status identity");
                            var statusOnlyResult = await SendLiveCommandAsync(
                                safetyGeneration,
                                operation,
                                () => currentGroup.WaitForLockedStandbyAsync(
                                    new LMCGroupEnableWaitOptions(),
                                    CancellationToken.None));
                            if (safetyGeneration != safetyRequestGeneration)
                            {
                                MarkGroupProfileLockResultDiscarded(
                                    operation + " completion");
                            }
                            EnsureNoNewSafetyRequestBeforeResultApplication(
                                safetyGeneration,
                                operation + " completion");
                            await EnsureGroupProfileLockRecoveryIdentityAsync(
                                currentGroup,
                                operation + " post-status identity",
                                true);
                            if (safetyGeneration != safetyRequestGeneration)
                            {
                                MarkGroupProfileLockResultDiscarded(
                                    operation + " post-identity completion");
                            }
                            EnsureNoNewSafetyRequestBeforeResultApplication(
                                safetyGeneration,
                                operation + " post-identity completion");
                            CompleteGroupEnableStatusOnlyRecoveryUi(
                                statusOnlyResult);
                            return;
                        }

                        var result = await SendLiveCommandAsync(
                            safetyGeneration,
                            operation,
                            () => continuation == null
                                ? currentGroup
                                    .GroupEnableAndWaitForLockedStandbyAsync(
                                        new LMCGroupEnableWaitOptions(),
                                        accepted =>
                                            MarkGroupProfileLockAccepted(
                                                currentGroup,
                                                accepted,
                                                operation),
                                        CancellationToken.None)
                                : currentGroup
                                    .ResumeGroupEnableWaitForLockedStandbyAsync(
                                        continuation,
                                        CancellationToken.None));
                        if (safetyGeneration != safetyRequestGeneration)
                        {
                            MarkGroupProfileLockResultDiscarded(
                                operation + " completion");
                        }
                        EnsureNoNewSafetyRequestBeforeResultApplication(
                            safetyGeneration,
                            operation + " completion");
                        await EnsureGroupProfileLockRecoveryIdentityAsync(
                            currentGroup,
                            operation + " completion",
                            true);
                        if (safetyGeneration != safetyRequestGeneration)
                        {
                            MarkGroupProfileLockResultDiscarded(
                                operation + " post-identity completion");
                        }
                        EnsureNoNewSafetyRequestBeforeResultApplication(
                            safetyGeneration,
                            operation + " post-identity completion");
                        CompleteGroupEnableWaitUi(result);
                    }
                    catch (Exception error)
                    {
                        var acceptedContinuation = currentGroup
                            .PendingGroupEnableWaitContinuation;
                        if (acceptedContinuation == null
                            && HasActiveGroupProfileLockRecoveryJournalRecord)
                        {
                            if (!(freshEnableAttempt
                                && TryResolveGroupProfileLockRecoveryForKnownNoDispatch(
                                    error,
                                    operation)))
                            {
                                MarkGroupProfileLockCompletionOutcomeUncertain(
                                    operation);
                            }
                        }

                        if (groupProfileLockRecoveryRequired)
                        {
                            // MarkGroupProfileLockResultDiscarded already set the
                            // only safe recovery path. Do not replace it with a
                            // new 0x2047-capable state.
                        }
                        else
                        {
                            if (acceptedContinuation != null)
                            {
                                PreservePendingGroupEnableWaitUi(
                                    currentGroup,
                                    acceptedContinuation,
                                    error.Message);
                            }
                            else
                            {
                                pendingGroupEnableWaitContinuation = null;
                                groupProfileLockVerificationPending =
                                    groupProfileLockAcceptedRestartRecovery;
                                groupProfileLocked = false;
                            }
                        }

                        throw;
                    }
                });
        }

        private async void ButtonGroupDisable_Click(
            object sender,
            RoutedEventArgs e)
        {
            var acceptedRestartRecovery =
                HasAcceptedGroupProfileUnlockRecoveryRecord;
            var operation = acceptedRestartRecovery
                ? "Resume Group Unlock Verification"
                : "Group Disable (Unlock Profile)";
            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                operation,
                async () =>
                {
                    if (HasUnresolvedGroupPowerState())
                    {
                        throw new InvalidOperationException(
                            "Group Disable is blocked while Group Power recovery "
                            + "is unresolved. Use Group Stop or Group Power Off "
                            + "for safety recovery; no 0x2048 was sent.");
                    }

                    EnsureNoUnresolvedDiagnosticMutationIgnoringPendingGroupReset(
                        "Group Disable (Unlock Profile)");
                    var currentGroup = RequireGroup();
                    var resetRecoveryWasUnresolved =
                        HasUnresolvedGroupResetState()
                        || groupResetObservedLockedStandby;
                    var continuation =
                        GetPendingGroupDisableWaitContinuation(currentGroup);
                    var statusOnlyRestartRecovery = continuation == null
                        && HasAcceptedGroupProfileUnlockRecoveryRecord;
                    var recordBeforeDisable =
                        groupProfileLockRecoveryJournal == null
                            ? null
                            : groupProfileLockRecoveryJournal.CurrentRecord;
                    var freshVerifiedLockedDisableAttempt =
                        continuation == null
                        && !statusOnlyRestartRecovery
                        && (recordBeforeDisable == null
                            || !recordBeforeDisable.IsActive)
                        && groupProfileLocked;
                    if (!statusOnlyRestartRecovery && continuation == null)
                    {
                        var record = recordBeforeDisable;
                        if (record != null
                            && record.IsActive
                            && !record.ExpectedProfileLocked)
                        {
                            if (record.State
                                != GroupProfileLockRecoveryState
                                    .RecoveryRequired)
                            {
                                throw new InvalidOperationException(
                                    "An armed or accepted Group Disable cannot be "
                                    + "replayed. Resume accepted status-only proof, "
                                    + "or explicitly retry only a RecoveryRequired "
                                    + "unlock record.");
                            }

                            EnsureGroupProfileLockRecoveryJournalAvailableForMutation(
                                "Explicit Group Disable recovery retry");
                            await EnsureGroupProfileLockRecoveryIdentityAsync(
                                currentGroup,
                                "Explicit Group Disable recovery retry",
                                false);
                        }
                        else
                        {
                            await ArmGroupProfileLockRecoveryBeforeDisableAsync(
                                currentGroup);
                        }
                    }

                    var disableRecoveryRecord =
                        RequireActiveGroupProfileLockRecoveryRecord(
                            operation + " durable operation identity");
                    if (disableRecoveryRecord.ExpectedProfileLocked)
                    {
                        throw new InvalidOperationException(
                            operation
                            + " did not publish a durable unlock identity before dispatch.");
                    }
                    var disableRecoveryIdentity =
                        disableRecoveryRecord.Identity;

                    try
                    {
                        if (statusOnlyRestartRecovery)
                        {
                            await EnsureGroupProfileLockRecoveryIdentityAsync(
                                currentGroup,
                                operation + " pre-status identity",
                                false);
                            var statusOnlyResult =
                                await SendLiveCommandAsync(
                                    safetyGeneration,
                                    operation,
                                    () => currentGroup
                                        .WaitForStableDisabledAsync(
                                            new LMCGroupDisableWaitOptions(),
                                            CancellationToken.None));
                            EnsureNoNewSafetyRequestBeforeResultApplication(
                                safetyGeneration,
                                operation + " status-only completion");
                            await EnsureGroupProfileLockRecoveryIdentityAsync(
                                currentGroup,
                                operation + " post-status identity",
                                false);
                            EnsureNoNewSafetyRequestBeforeResultApplication(
                                safetyGeneration,
                                operation + " post-status identity");
                            CompleteGroupDisableStatusOnlyRecoveryUi(
                                statusOnlyResult);
                            return;
                        }

                        var result = await SendLiveCommandAsync(
                            safetyGeneration,
                            operation,
                            () => continuation == null
                                ? currentGroup
                                    .GroupDisableAndWaitForStableDisabledAsync(
                                        new LMCGroupDisableWaitOptions(),
                                        accepted =>
                                        {
                                            MarkGroupProfileUnlockAccepted(
                                                currentGroup,
                                                accepted,
                                                operation);
                                            SupersedePendingGroupResetByLaterMutation(
                                                "Accepted Group Disable");
                                        },
                                        CancellationToken.None)
                                : currentGroup
                                    .ResumeGroupDisableWaitForStableDisabledAsync(
                                        continuation,
                                        CancellationToken.None),
                            true);
                        EnsureNoNewSafetyRequestBeforeResultApplication(
                            safetyGeneration,
                            operation + " completion");
                        await EnsureGroupProfileLockRecoveryIdentityAsync(
                            currentGroup,
                            operation + " completion",
                            false);
                        EnsureNoNewSafetyRequestBeforeResultApplication(
                            safetyGeneration,
                            operation + " post-identity completion");
                        CompleteGroupDisableWaitUi(result);
                        if (resetRecoveryWasUnresolved)
                        {
                            InvalidateGroupPreparationAfterAcceptedReset();
                            WriteLog(
                                "Group Disable resolved the Reset safety path, "
                                + "but Reset-invalidated Power/Identity/Home/"
                                + "Profile preparation remains fail-closed.");
                        }
                    }
                    catch (Exception error)
                    {
                        var disableEvidence =
                            GetGroupDisableWaitEvidence(error);
                        if (freshVerifiedLockedDisableAttempt
                            && TryRestoreFreshVerifiedLockAfterKnownNoEffectGroupDisable(
                                error,
                                disableRecoveryIdentity,
                                safetyGeneration,
                                operation))
                        {
                            throw;
                        }

                        if (TryDiscardGroupDisableOutcomeSupersededBySafety(
                            disableRecoveryIdentity,
                            safetyGeneration,
                            operation))
                        {
                            throw;
                        }

                        var acceptedContinuation = currentGroup
                            .PendingGroupDisableWaitContinuation;
                        var record = groupProfileLockRecoveryJournal == null
                            ? null
                            : groupProfileLockRecoveryJournal.CurrentRecord;
                        if (acceptedContinuation != null
                            && record != null
                            && record.IsActive
                            && !record.ExpectedProfileLocked
                            && record.State
                                == GroupProfileLockRecoveryState
                                    .AcceptedAwaitingProof)
                        {
                            PreservePendingGroupDisableWaitUi(
                                currentGroup,
                                acceptedContinuation,
                                error.Message);
                        }
                        else if (HasAcceptedGroupProfileUnlockRecoveryRecord)
                        {
                            pendingGroupDisableWaitContinuation = null;
                            groupProfileUnlockVerificationPending = true;
                        }
                        else
                        {
                            PromoteGroupProfileLockRecoveryJournal(
                                operation + " completion outcome uncertainty");
                            pendingGroupDisableWaitContinuation = null;
                            groupProfileUnlockVerificationPending = false;
                        }

                        if ((acceptedContinuation != null
                                && acceptedContinuation.IsPending)
                            || (disableEvidence != null
                                && (disableEvidence.SubmissionOutcome
                                        == LMCGroupDisableSubmissionOutcome
                                            .Accepted
                                    || disableEvidence.SubmissionOutcome
                                        == LMCGroupDisableSubmissionOutcome
                                            .OutcomeUncertain)))
                        {
                            SupersedePendingGroupResetByLaterMutation(
                                "Group Disable accepted or outcome-uncertain dispatch");
                        }

                        throw;
                    }
                });
        }

        private async void ButtonGroupReset_Click(
            object sender,
            RoutedEventArgs e)
        {
            var currentGroup = group;
            var continuation = currentGroup == null
                ? null
                : GetPendingGroupResetWaitContinuation(currentGroup);
            var resumeAcceptedReset = continuation != null
                && continuation.IsPending;
            var operation = resumeAcceptedReset
                ? "Resume Group Reset Verification"
                : "Group Reset";
            if (!CanStartLiveCommand(
                    operation,
                    resumeAcceptedReset))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            GroupResetRecoveryRecord verificationRecord = null;
            await RunOperationAsync(
                operation,
                async () =>
                {
                    if (!resumeAcceptedReset)
                    {
                        EnsureNoUnresolvedGroupProfileLockMutation(
                            operation);
                    }
                    currentGroup = RequireGroup();
                    continuation =
                        GetPendingGroupResetWaitContinuation(currentGroup);
                    var freshResetAttempt = continuation == null;
                    var options = new LMCGroupResetWaitOptions();
                    try
                    {
                        GroupResetDispatchIdentityContext dispatchIdentity = null;
                        if (freshResetAttempt)
                        {
                            EnsureGroupResetRecoveryJournalCanArm();
                            await RefreshDiagnosticsCapabilitiesAsync(
                                RequireConnection());
                            dispatchIdentity =
                                CaptureGroupResetDispatchIdentity(operation);
                        }
                        else
                        {
                            verificationRecord =
                                RequireActiveGroupResetRecoveryRecord(operation);
                        }

                        var result = await SendLiveCommandAsync(
                            safetyGeneration,
                            operation,
                            async () =>
                            {
                                if (freshResetAttempt)
                                {
                                    continuation = await currentGroup
                                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                                            options,
                                            prepared =>
                                            {
                                                verificationRecord =
                                                    ArmGroupResetRecoveryBeforeDispatch(
                                                        dispatchIdentity,
                                                        prepared,
                                                        operation);
                                            },
                                            accepted => MarkGroupResetAccepted(
                                                currentGroup,
                                                accepted,
                                                verificationRecord,
                                                operation),
                                            CancellationToken.None);
                                }

                                return await currentGroup
                                    .ResumeGroupResetWaitForStableErrorClearanceAsync(
                                        continuation,
                                        new LMCGroupResetWaitOptions
                                        {
                                            StableSampleCount = continuation
                                                .RequiredStableSampleCount
                                        },
                                        CancellationToken.None);
                            },
                            resumeAcceptedReset);
                        EnsureNoNewSafetyRequestBeforeResultApplication(
                            safetyGeneration,
                            operation + " completion");
                        CompleteGroupResetWaitUi(
                            result,
                            verificationRecord);
                    }
                    catch (Exception error)
                    {
                        HandleGroupResetWaitFailure(
                            currentGroup,
                            verificationRecord,
                            error);
                        throw;
                    }
                });
        }

        private async void ButtonGroupStop_Click(
            object sender,
            RoutedEventArgs e)
        {
            LMCGroupAxis currentGroup = null;
            LMCGroupStopWaitContinuation currentStop = null;
            LMCGroupStopWaitEvidence acceptedBeginBoundaryEvidence = null;
            var safetySend = await RunSafetyCommandAsync(
                "Group Stop Send",
                async reservedGeneration =>
                {
                    currentGroup = RequireGroup();
                    await EnsureMotionRecoverySafetyDispatchIdentityAsync(
                        reservedGeneration,
                        MotionUncertaintyTargetKind.Group,
                        currentGroup.GroupName,
                        currentGroup.GroupReference,
                        "Group Stop recovery");
                    var input = ReadGroupStopInput();
                    try
                    {
                        currentStop = await currentGroup
                            .BeginGroupStopWaitForStableStandbyAsync(
                                input.DecelerationRaw,
                                input.JerkRaw,
                                CancellationToken.None);
                    }
                    catch (Exception error)
                    {
                        acceptedBeginBoundaryEvidence =
                            GetGroupStopWaitEvidence(error);
                        currentStop =
                            GetGroupStopWaitContinuation(error);
                        if (currentStop == null || !currentStop.IsPending)
                        {
                            if (acceptedBeginBoundaryEvidence != null
                                && (acceptedBeginBoundaryEvidence
                                            .SubmissionOutcome
                                        == LMCGroupStopSubmissionOutcome
                                            .Accepted
                                    || acceptedBeginBoundaryEvidence
                                            .SubmissionOutcome
                                        == LMCGroupStopSubmissionOutcome
                                            .OutcomeUncertain))
                            {
                                SupersedePendingGroupResetByLaterMutation(
                                    "Group Stop outcome-uncertain dispatch");
                            }

                            AppendGroupStopWaitEvidence(
                                "Group Stop acknowledgement was not accepted.",
                                acceptedBeginBoundaryEvidence,
                                currentStop);
                            throw;
                        }
                    }

                    pendingGroupStopWaitContinuation = currentStop;
                    SupersedePendingGroupResetByLaterMutation(
                        "Accepted Group Stop");
                    RecordMotionRecoverySafetyCommandAccepted(
                        reservedGeneration,
                        MotionUncertaintyTargetKind.Group,
                        currentGroup.GroupName,
                        currentGroup.GroupReference,
                        "Group Stop");
                    TextGroupResult.Text = FormatResponse(
                        currentStop.Acknowledgement)
                        + Environment.NewLine
                        + "Group Stop ACK accepted exactly once; status-only "
                        + "0x2045 verification is pending.";
                    if (acceptedBeginBoundaryEvidence != null)
                    {
                        AppendGroupStopWaitEvidence(
                            "Group Stop ACK was accepted at the Begin deadline/cancellation boundary; continuing with status-only verification without replaying 0x2085.",
                            acceptedBeginBoundaryEvidence,
                            currentStop);
                    }
                },
                () => CancelQualificationForExternalSafety(
                    "Group Stop",
                    true),
                true);

            if (!safetySend.Sent
                || currentGroup == null
                || currentStop == null)
            {
                ReleaseUnusedSafetyMonitorReservation(
                    safetySend,
                    "Group Stop");
                return;
            }

            await RunGroupSafetyMonitorAsync(
                "Group Stop",
                currentGroup,
                currentStop,
                safetySend.Generation,
                safetySend.MonitorReserved);
        }

        private async void ButtonGroupMoveLinear_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!CanStartMotionCommand("Move Linear Absolute"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            LMCGroupAxis monitoredGroup = null;
            var trackingGeneration = 0;
            var noMovementExpected = false;
            var monitorTimeoutMilliseconds =
                MinimumGroupMotionMonitorMilliseconds;

            await RunOperationAsync(
                "Move Linear Absolute Send",
                async () =>
                {
                    var currentGroup = RequireGroup();
                    EnsureGroupReadyForMotion();
                    var input = ReadGroupMotionInput();
                    var startPosition = await currentGroup
                        .GroupReadActualPositionAsync(
                            input.Options.CoordinateSystem,
                            CancellationToken.None);
                    EnsureGroupPositionSuccess(
                        "Move Linear Absolute start position",
                        startPosition);

                    var currentPositions = startPosition.PositionsRaw;
                    var monitorDistances = input.PositionsRaw
                        .Select(
                            (target, index) =>
                                (long)target - currentPositions[index])
                        .ToArray();
                    noMovementExpected = monitorDistances
                        .Take(4)
                        .All(distance => distance == 0);
                    monitorTimeoutMilliseconds =
                        CalculateGroupMotionMonitorTimeoutMilliseconds(
                            monitorDistances,
                            input.VelocityRaw,
                            input.AccelerationRaw,
                            input.DecelerationRaw);
                    WriteLog(
                        "Move Linear Absolute input: StartRaw="
                        + FormatGroupPositionsRaw(currentPositions)
                        + ", TargetRaw="
                        + FormatGroupPositionsRaw(input.PositionsRaw)
                        + ", VelocityRaw="
                        + input.VelocityRaw
                        + ", AccelerationRaw="
                        + input.AccelerationRaw
                        + ", DecelerationRaw="
                        + input.DecelerationRaw
                        + ", JerkRaw="
                        + input.JerkRaw
                        + ", Transition="
                        + input.Options.TransitionMode
                        + ", Buffer="
                        + input.Options.BufferMode
                        + ", MonitorTimeoutMs="
                        + monitorTimeoutMilliseconds
                        + ".");
                    monitoredGroup = currentGroup;

                    var response = await DispatchTrackedMotionAsync(
                        safetyGeneration,
                        MotionUncertaintyTargetKind.Group,
                        currentGroup.GroupName,
                        currentGroup.GroupReference,
                        "Move Linear Absolute",
                        generation => trackingGeneration = generation,
                        async () => await currentGroup
                                .MoveLinearAbsoluteExAsync(
                                    input.PositionsRaw,
                                    input.VelocityRaw,
                                    input.AccelerationRaw,
                                    input.DecelerationRaw,
                                    input.JerkRaw,
                                    input.Options,
                                    CancellationToken.None));

                    ClearMotionOnConfirmedRejection(
                        currentGroup.GroupName,
                        "Move Linear Absolute",
                        response);
                    if (response != null
                        && response.IsFrameValid
                        && response.ErrorId == 7)
                    {
                        var diagnostic =
                            "LASAL rejected the endpoint with "
                            + "_LMCPROF_SWE_ERROR (7): a runtime software end "
                            + "position limit was violated. StartRaw="
                            + FormatGroupPositionsRaw(currentPositions)
                            + ", TargetRaw="
                            + FormatGroupPositionsRaw(input.PositionsRaw)
                            + ". Compare the target with each axis "
                            + "AxReadSWEndPos maximum/minimum and "
                            + "ReadProfileError().SubErrorNo in LASAL.";
                        TextGroupResult.Text =
                            FormatResponse(response)
                            + Environment.NewLine
                            + diagnostic;
                        throw new InvalidOperationException(diagnostic);
                    }
                    EnsureResponseSuccess("Move Linear Absolute", response);
                    TextGroupResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Command accepted; monitoring Group InPosition "
                        + "with timeout "
                        + monitorTimeoutMilliseconds
                        + " ms.";
                });

            if (monitoredGroup == null
                || trackingGeneration == 0
                || !IsTrackedMotion(
                    monitoredGroup.GroupName,
                    trackingGeneration))
            {
                return;
            }

            await MonitorGroupFiniteMotionAsync(
                "Move Linear Absolute",
                monitoredGroup,
                trackingGeneration,
                noMovementExpected,
                monitorTimeoutMilliseconds);
        }

        private async void ButtonGroupMoveLinearRelative_Click(
            object sender,
            RoutedEventArgs e)
        {
            const string operation = "Move Linear Relative";
            if (!CanStartMotionCommand(operation))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            LMCGroupAxis monitoredGroup = null;
            var trackingGeneration = 0;
            var noMovementExpected = false;
            var monitorTimeoutMilliseconds =
                MinimumGroupMotionMonitorMilliseconds;

            await RunOperationAsync(
                operation + " Send",
                async () =>
                {
                    EnsureNoUnresolvedGroupProfileLockMutation(
                        operation);
                    var currentConnection = RequireConnection();
                    var currentGroup = RequireGroup();
                    EnsureGroupReadyForMotion();
                    var input = ReadGroupMotionInput();
                    var startPosition = await currentGroup
                        .GroupReadActualPositionAsync(
                            input.Options.CoordinateSystem,
                            CancellationToken.None);
                    EnsureGroupPositionSuccess(
                        operation + " start position",
                        startPosition);

                    var currentPositions = startPosition.PositionsRaw;
                    var monitorDistances = input.PositionsRaw
                        .Select(distance => (long)distance)
                        .ToArray();
                    noMovementExpected = monitorDistances
                        .Take(4)
                        .All(distance => distance == 0);
                    monitorTimeoutMilliseconds =
                        CalculateGroupMotionMonitorTimeoutMilliseconds(
                            monitorDistances,
                            input.VelocityRaw,
                            input.AccelerationRaw,
                            input.DecelerationRaw);
                    WriteLog(
                        operation
                        + " input: StartRaw="
                        + FormatGroupPositionsRaw(currentPositions)
                        + ", DeltaRaw="
                        + FormatGroupPositionsRaw(input.PositionsRaw)
                        + ", VelocityRaw="
                        + input.VelocityRaw
                        + ", AccelerationRaw="
                        + input.AccelerationRaw
                        + ", DecelerationRaw="
                        + input.DecelerationRaw
                        + ", JerkRaw="
                        + input.JerkRaw
                        + ", Transition="
                        + input.Options.TransitionMode
                        + ", Buffer="
                        + input.Options.BufferMode
                        + ", MonitorTimeoutMs="
                        + monitorTimeoutMilliseconds
                        + ".");
                    monitoredGroup = currentGroup;
                    var verifiedCapabilities = await currentConnection.Admin
                        .GetCapabilitiesAsync(CancellationToken.None);
                    if (!verifiedCapabilities.Supports(
                            LMCAdminFeature.GroupLinearRelative)
                        || verifiedCapabilities.GroupReference
                            != currentGroup.GroupReference)
                    {
                        throw new NotSupportedException(
                            "The connected PLC does not advertise the group "
                            + "linear-relative motion facade for the loaded "
                            + "group.");
                    }

                    try
                    {
                        var response = await DispatchTrackedMotionAsync(
                            safetyGeneration,
                            MotionUncertaintyTargetKind.Group,
                            currentGroup.GroupName,
                            currentGroup.GroupReference,
                            operation,
                            generation => trackingGeneration = generation,
                            async () => await currentGroup
                                    .MoveLinearRelativeExAsync(
                                        input.PositionsRaw,
                                        input.VelocityRaw,
                                        input.AccelerationRaw,
                                        input.DecelerationRaw,
                                        input.JerkRaw,
                                        input.Options,
                                        verifiedCapabilities,
                                        CancellationToken.None));

                        ClearMotionOnConfirmedRejection(
                            currentGroup.GroupName,
                            operation,
                            response);
                        EnsureAdminResponseSuccess(operation, response);
                        TextGroupResult.Text =
                            FormatAdminResponse(response)
                            + Environment.NewLine
                            + "Command accepted; monitoring Group InPosition "
                            + "with timeout "
                            + monitorTimeoutMilliseconds
                            + " ms.";
                    }
                    catch (LMCAdminCommandException error)
                    {
                        ClearMotionOnConfirmedRejection(
                            currentGroup.GroupName,
                            operation,
                            error.Response);
                        TextGroupResult.Text = FormatAdminResponse(
                            error.Response);
                        throw;
                    }
                });

            if (monitoredGroup == null
                || trackingGeneration == 0
                || !IsTrackedMotion(
                    monitoredGroup.GroupName,
                    trackingGeneration))
            {
                return;
            }

            await MonitorGroupFiniteMotionAsync(
                operation,
                monitoredGroup,
                trackingGeneration,
                noMovementExpected,
                monitorTimeoutMilliseconds);
        }

        private async void ButtonCheckKinHome_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Identity Home Check",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    RequireGroup();
                    var homeCheck = await CheckIdentityAxesHomeAsync(
                        currentConnection,
                        CancellationToken.None);
                    if (!homeCheck.AllReferenced)
                    {
                        throw new InvalidOperationException(
                            "Home Check failed. Reference the following identity "
                            + "axes before Set Identity: "
                            + homeCheck.UnreferencedAxisSummary
                            + ".");
                    }
                });
        }

        private async void ButtonSetKinTransform_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Set Identity Kinematics"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Set Identity Kinematics",
                async () =>
                {
                    EnsureNoUnresolvedGroupProfileLockMutation(
                        "Set Identity Kinematics");
                    var currentConnection = RequireConnection();
                    var currentGroup = RequireGroup();
                    EnsureGroupActiveVerified();
                    if (groupProfileLocked
                        || groupProfileLockVerificationPending)
                    {
                        throw new InvalidOperationException(
                            "Finish or cancel pending Lock Verification, or Disable "
                            + "(Unlock Profile) before changing the identity "
                            + "configuration.");
                    }

                    groupIdentityConfigured = false;
                    var homeCheck = await CheckIdentityAxesHomeAsync(
                        currentConnection,
                        CancellationToken.None);
                    if (!homeCheck.AllReferenced)
                    {
                        groupIdentityConfigured = false;
                        throw new InvalidOperationException(
                            "Set Identity blocked because these identity axes "
                            + "are not referenced: "
                            + homeCheck.UnreferencedAxisSummary
                            + ". Run Home, then retry Set Identity.");
                    }

                    var axisX = homeCheck.AxisX.Axis;
                    var axisY = homeCheck.AxisY.Axis;
                    var axisZ = homeCheck.AxisZ.Axis;
                    var axisU = homeCheck.AxisU.Axis;

                    var response = await SendLiveCommandAsync(
                        safetyGeneration,
                        "Set Identity Kinematics",
                        () => currentGroup
                            .SetKinTransformCartesian4AxisAsync(
                                axisX,
                                axisY,
                                axisZ,
                                axisU,
                                CancellationToken.None));
                    EnsureResponseSuccess(
                        "Set Identity Kinematics",
                        response);
                    groupIdentityConfigured = true;
                    TextGroupResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "X="
                        + axisX.AxisName
                        + " ("
                        + axisX.AxisReference
                        + "), Y="
                        + axisY.AxisName
                        + " ("
                        + axisY.AxisReference
                        + "), Z="
                        + axisZ.AxisName
                        + " ("
                        + axisZ.AxisReference
                        + "), U="
                        + axisU.AxisName
                        + " ("
                        + axisU.AxisReference
                        + ")"
                        + Environment.NewLine
                        + "Identity configured; Enable (Lock Profile) is now "
                        + "available.";
                });
        }

        private async Task<IdentityHomeCheckResult> CheckIdentityAxesHomeAsync(
            LMCConnection currentConnection,
            CancellationToken cancellationToken)
        {
            groupIdentityHomeCheckComplete = false;
            groupIdentityHomeCheckPassed = false;

            try
            {
                var result = new IdentityHomeCheckResult(
                    await ReadIdentityAxisHomeAsync(
                        currentConnection,
                        "X",
                        RequiredText(TextKinAxisX.Text, "X axis object"),
                        cancellationToken),
                    await ReadIdentityAxisHomeAsync(
                        currentConnection,
                        "Y",
                        RequiredText(TextKinAxisY.Text, "Y axis object"),
                        cancellationToken),
                    await ReadIdentityAxisHomeAsync(
                        currentConnection,
                        "Z",
                        RequiredText(TextKinAxisZ.Text, "Z axis object"),
                        cancellationToken),
                    await ReadIdentityAxisHomeAsync(
                        currentConnection,
                        "U",
                        RequiredText(TextKinAxisU.Text, "U axis object"),
                        cancellationToken));

                groupIdentityHomeCheckComplete = true;
                groupIdentityHomeCheckPassed = result.AllReferenced;
                if (!result.AllReferenced)
                {
                    groupIdentityConfigured = false;
                }

                DisplayIdentityHomeCheck(result);
                WriteLog(
                    "Identity Home Check "
                    + (result.AllReferenced ? "PASS" : "FAIL")
                    + ": "
                    + result.ReferencedCount
                    + "/4 axes referenced.");
                UpdateUiState();
                return result;
            }
            catch (Exception error)
            {
                groupIdentityConfigured = false;
                if (TextKinHomeStatus != null)
                {
                    TextKinHomeStatus.Text =
                        "Home Check ERROR"
                        + Environment.NewLine
                        + error.Message;
                }

                UpdateUiState();
                throw;
            }
        }

        private static async Task<IdentityAxisHomeStatus>
            ReadIdentityAxisHomeAsync(
                LMCConnection currentConnection,
                string coordinateName,
                string axisName,
                CancellationToken cancellationToken)
        {
            var selectedAxis = await LMCSingleAxis.CreateAsync(
                currentConnection,
                axisName,
                cancellationToken);
            var status = await selectedAxis.ReadStatusResultAsync(
                cancellationToken);
            EnsureAxisStatusSuccess(
                coordinateName + " axis Home Check",
                status);
            return new IdentityAxisHomeStatus(
                coordinateName,
                selectedAxis,
                status);
        }

        private void DisplayIdentityHomeCheck(IdentityHomeCheckResult result)
        {
            var axisLines = result.Axes.Select(
                item =>
                    item.CoordinateName
                    + " "
                    + item.Axis.AxisName
                    + " Ref="
                    + item.Axis.AxisReference
                    + " Home/Referenced="
                    + item.Status.IsReferenced
                    + " PowerOn="
                    + item.Status.IsPowerOn
                    + " Standstill="
                    + item.Status.IsStandstill
                    + " State=0x"
                    + item.Status.State.ToString("X8"));

            TextKinHomeStatus.Text =
                "Home Check "
                + (result.AllReferenced ? "PASS" : "FAIL")
                + " ("
                + result.ReferencedCount
                + "/4 referenced) at "
                + DateTime.Now.ToString(
                    "HH:mm:ss.fff",
                    CultureInfo.InvariantCulture)
                + Environment.NewLine
                + string.Join(Environment.NewLine, axisLines);
        }

        private void TextKinAxis_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            groupIdentityConfigured = false;
            ResetIdentityHomeCheckState();
            if (TextKinHomeStatus == null)
            {
                return;
            }

            UpdateUiState();
        }

        private void TextAxisName_TextChanged(object sender, TextChangedEventArgs e)
        {
            InvalidateAxisQualificationConfirmations();
            if (axis == null || TextAxisName == null)
            {
                return;
            }

            if (!string.Equals(
                axis.AxisName,
                TextAxisName.Text.Trim(),
                StringComparison.Ordinal))
            {
                axis = null;
                ClearMotionLookupIdentity(
                    MotionUncertaintyTargetKind.Axis);
                if (TextAxisReference != null)
                {
                    TextAxisReference.Text = "not loaded";
                }

                UpdateUiState();
            }
        }

        private void TextGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextGroupName == null)
            {
                return;
            }

            if (IsRecoveryIdentityReadOnlyConnection(connection))
            {
                // This text is only a local inspection draft in quarantine.
                // Do not rewrite it from a durable record that belongs to a
                // different PLC identity.
                return;
            }

            if (HasActiveGroupPowerRecoveryRecord)
            {
                var recoveryGroupName = groupPowerRecoveryJournal
                    .CurrentRecord.GroupName;
                if (!string.Equals(
                    recoveryGroupName,
                    TextGroupName.Text.Trim(),
                    StringComparison.Ordinal))
                {
                    TextGroupName.Text = recoveryGroupName;
                    TextGroupName.CaretIndex = TextGroupName.Text.Length;
                    WriteLog(
                        "Group name change was rejected while Group Power "
                        + "recovery is active.");
                }

                return;
            }

            if (HasUnresolvedGroupResetState())
            {
                var pendingGroupName = group == null
                    ? null
                    : group.GroupName;
                if (!string.IsNullOrWhiteSpace(pendingGroupName)
                    && !string.Equals(
                        pendingGroupName,
                        TextGroupName.Text.Trim(),
                        StringComparison.Ordinal))
                {
                    TextGroupName.Text = pendingGroupName;
                    TextGroupName.CaretIndex = TextGroupName.Text.Length;
                    WriteLog(
                        "Group name change was rejected while Group Reset "
                        + "verification is pending in this live session.");
                }

                return;
            }

            if (HasUnresolvedGroupProfileLockState())
            {
                var unresolvedGroupName =
                    GetUnresolvedGroupProfileLockName();
                if (!string.IsNullOrWhiteSpace(
                        unresolvedGroupName)
                    && !string.Equals(
                        unresolvedGroupName,
                        TextGroupName.Text.Trim(),
                        StringComparison.Ordinal))
                {
                    TextGroupName.Text = unresolvedGroupName;
                    TextGroupName.CaretIndex = TextGroupName.Text.Length;
                    WriteLog(
                        "Group name change was rejected while Group Enable is pending "
                        + "or the profile-lock result is uncertain.");
                }

                return;
            }

            if (group == null || TextGroupName == null)
            {
                return;
            }

            if (!string.Equals(
                group.GroupName,
                TextGroupName.Text.Trim(),
                StringComparison.Ordinal))
            {
                group = null;
                ClearMotionLookupIdentity(
                    MotionUncertaintyTargetKind.Group);
                ResetGroupPreparationState();
                if (TextGroupReference != null)
                {
                    TextGroupReference.Text = "not loaded";
                }

                UpdateUiState();
            }
        }

        private void ButtonCopyLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TextExecutionLog.Text ?? string.Empty);
                TextOperationState.Text = "Log copied";
            }
            catch (Exception error)
            {
                WriteLog("Copy Log failed: " + error.Message);
            }
        }

        private void ButtonClearLog_Click(object sender, RoutedEventArgs e)
        {
            TextExecutionLog.Clear();
            TextOperationState.Text = "Log cleared";
        }

        private async Task RunOperationAsync(
            string operation,
            Func<Task> action,
            bool blockSafetyCommands = false)
        {
            if (operationRunning
                || safetyCommandRunning
                || safetyMonitorCount > 0
                || qualificationRunning)
            {
                WriteLog(
                    "Another operation, safety verification, or qualification is already running.");
                return;
            }

            var operationSafetyGeneration = safetyRequestGeneration;
            operationRunning = true;
            connectionTransitionRunning = blockSafetyCommands;
            TextOperationState.Text = operation + " running";
            UpdateUiState();

            try
            {
                WriteLog(operation + " started.");
                using (sendPriorityCoordinator.BeginPreemptibleScope(
                    operationSafetyGeneration,
                    operation))
                {
                    await action();
                }
                WriteLog(operation + " PASS.");
                TextOperationState.Text = operation + " completed";
            }
            catch (Exception error)
            {
                WriteLog(operation + " FAILED: " + error.Message);
                TextOperationState.Text = operation + " failed";
            }
            finally
            {
                operationRunning = false;
                connectionTransitionRunning = false;
                UpdateUiState();
            }
        }

        private sealed class SafetyCommandResult
        {
            internal static readonly SafetyCommandResult NotSent =
                new SafetyCommandResult(false, 0, false);

            internal SafetyCommandResult(
                bool sent,
                long generation,
                bool monitorReserved)
            {
                Sent = sent;
                Generation = generation;
                MonitorReserved = monitorReserved;
            }

            internal bool Sent { get; private set; }
            internal long Generation { get; private set; }
            internal bool MonitorReserved { get; private set; }
        }

        private void ReleaseUnusedSafetyMonitorReservation(
            SafetyCommandResult result,
            string operation)
        {
            if (result == null || !result.MonitorReserved)
            {
                return;
            }

            safetyMonitorCount--;
            WriteLog(
                operation
                + " safety monitor reservation was released because no monitor owner was available.");
            UpdateUiState();
        }

        private async Task<SafetyCommandResult> RunSafetyCommandAsync(
            string operation,
            Func<long, Task> action,
            Action safetyReserved = null,
            bool reserveSafetyMonitor = false)
        {
            if (IsRecoveryIdentityReadOnlyConnection(connection))
            {
                WriteLog(
                    operation
                    + " is blocked before wire send because the connection is in "
                    + "recovery-identity read-only quarantine. "
                    + GetRecoveryIdentityReadOnlyGuidance());
                return SafetyCommandResult.NotSent;
            }

            if (safetyCommandRunning)
            {
                WriteLog("Another Stop or Power Off send is already running.");
                return SafetyCommandResult.NotSent;
            }

            var reservedGeneration =
                sendPriorityCoordinator.ReservePrioritySend();
            var currentGroup = group;
            if (currentGroup != null)
            {
                currentGroup.InvalidatePendingGroupEnableWaitStatusProof();
            }
            var monitorReserved = false;
            safetyCommandRunning = true;
            TextOperationState.Text = operation + " running";
            UpdateUiState();

            try
            {
                if (safetyReserved != null)
                {
                    safetyReserved();
                }

                WriteLog(operation + " queued with safety priority.");
                await commandSendGate.WaitAsync();
                try
                {
                    WriteLog(operation + " transmitting.");
                    using (sendPriorityCoordinator.BeginPriorityScope(
                        reservedGeneration,
                        operation))
                    {
                        await action(reservedGeneration);
                    }

                    if (reserveSafetyMonitor)
                    {
                        // Keep admission closed continuously from accepted
                        // safety send through the caller's stable-state
                        // verification. The monitor consumes this reservation.
                        safetyMonitorCount++;
                        monitorReserved = true;
                    }
                }
                finally
                {
                    commandSendGate.Release();
                }

                WriteLog(operation + " PASS.");
                TextOperationState.Text = operation + " accepted";
                return new SafetyCommandResult(
                    true,
                    reservedGeneration,
                    monitorReserved);
            }
            catch (Exception error)
            {
                if (monitorReserved)
                {
                    safetyMonitorCount--;
                    monitorReserved = false;
                }

                WriteLog(operation + " FAILED: " + error.Message);
                TextOperationState.Text = operation + " failed";
                PublishDeferredGroupResetSubmissionUncertainIfAny();
                return new SafetyCommandResult(
                    false,
                    reservedGeneration,
                    false);
            }
            finally
            {
                safetyCommandRunning = false;
                UpdateUiState();
            }
        }

        private async Task<T> SendLiveCommandAsync<T>(
            long expectedSafetyGeneration,
            string operation,
            Func<Task<T>> send,
            bool allowPendingGroupReset = false)
        {
            await commandSendGate.WaitAsync();
            try
            {
                if (allowPendingGroupReset)
                {
                    EnsureNoUnresolvedDiagnosticMutationIgnoringPendingGroupReset(
                        operation);
                }
                else
                {
                    EnsureNoUnresolvedDiagnosticMutation(operation);
                }
                EnsureNoNewSafetyRequest(
                    expectedSafetyGeneration,
                    operation);
                using (sendPriorityCoordinator.BeginPreemptibleScope(
                    expectedSafetyGeneration,
                    operation))
                {
                    var result = await send();
                    EnsureNoNewSafetyRequestBeforeResultApplication(
                        expectedSafetyGeneration,
                        operation + " result application");
                    return result;
                }
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private async Task<T> SendSerializedCommandAsync<T>(Func<Task<T>> send)
        {
            var expectedSafetyGeneration = safetyRequestGeneration;
            await commandSendGate.WaitAsync();
            try
            {
                EnsureNoNewSafetyRequest(
                    expectedSafetyGeneration,
                    "Serialized command");
                using (sendPriorityCoordinator.BeginPreemptibleScope(
                    expectedSafetyGeneration,
                    "Serialized command"))
                {
                    var result = await send();
                    EnsureNoNewSafetyRequestBeforeResultApplication(
                        expectedSafetyGeneration,
                        "Serialized command result application");
                    return result;
                }
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private void EnsureNoNewSafetyRequest(
            long expectedGeneration,
            string operation)
        {
            if (expectedGeneration == safetyRequestGeneration)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " was cancelled before transmission because Stop or Power Off "
                + "was requested.");
        }

        private void EnsureNoNewSafetyRequestBeforeResultApplication(
            long expectedGeneration,
            string operation)
        {
            if (expectedGeneration == safetyRequestGeneration)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " discarded a completed response because a newer Stop or "
                + "Power Off request was reserved.");
        }

        private async Task RunSafetyMonitorAsync(
            string operation,
            LMCSingleAxis currentAxis,
            Func<Task<LMCReadStatusResult>> verifySafeState,
            long monitorSafetyGeneration,
            bool monitorReservationAlreadyHeld,
            Action afterMotionJournalResolvedBeforeVolatileClear = null)
        {
            if (!monitorReservationAlreadyHeld)
            {
                safetyMonitorCount++;
            }
            TextOperationState.Text = operation + " verifying standstill";
            WriteLog(
                operation
                + " accepted. Verifying three stable Standstill samples; "
                + "explicit newer safety commands allowed by the current "
                + "recovery policy remain available.");
            UpdateUiState();

            try
            {
                LMCReadStatusResult status;
                using (sendPriorityCoordinator.BeginPreemptibleScope(
                    monitorSafetyGeneration,
                    operation + " safety verification"))
                {
                    status = await verifySafeState();
                }
                EnsureNoNewSafetyRequestBeforeResultApplication(
                    monitorSafetyGeneration,
                    operation + " safety verification completion");
                DisplayAxisStatus(status);
                await ClearMotionWarningAfterVerifiedStateAsync(
                    operation + " and stable standstill were verified",
                    null,
                    afterMotionJournalResolvedBeforeVolatileClear);
                WriteLog(operation + " safety verification PASS.");
                TextOperationState.Text = operation + " verified";
            }
            catch (Exception error)
            {
                WriteLog(
                    operation
                    + " safety verification FAILED: "
                    + error.Message
                    + (error.InnerException == null
                        ? string.Empty
                        : " Inner: " + error.InnerException.Message)
                    + " Do not assume the axis is stopped.");
                TextOperationState.Text = operation + " verification failed";
            }
            finally
            {
                safetyMonitorCount--;
                UpdateUiState();
            }
        }

        private async Task RunGroupSafetyMonitorAsync(
            string operation,
            LMCGroupAxis currentGroup,
            LMCGroupStopWaitContinuation currentStop,
            long monitorSafetyGeneration,
            bool monitorReservationAlreadyHeld)
        {
            if (!monitorReservationAlreadyHeld)
            {
                safetyMonitorCount++;
            }
            TextOperationState.Text = operation + " verifying stable standby";
            WriteLog(
                operation
                + " ACK was accepted exactly once. Verifying three stable "
                + "Group Standby samples with status-only 0x2045 reads; "
                + "Group Stop and Power Off remain available.");
            UpdateUiState();

            try
            {
                LMCGroupStopWaitResult result;
                using (sendPriorityCoordinator.BeginPreemptibleScope(
                    monitorSafetyGeneration,
                    operation + " safety verification"))
                {
                    result = await currentGroup
                        .ResumeGroupStopWaitForStableStandbyAsync(
                            currentStop,
                            CancellationToken.None);
                }
                EnsureNoNewSafetyRequestBeforeResultApplication(
                    monitorSafetyGeneration,
                    operation + " safety verification completion");
                DisplayGroupStatus(result.FinalStatus);
                if (ReferenceEquals(
                    pendingGroupStopWaitContinuation,
                    currentStop))
                {
                    pendingGroupStopWaitContinuation = null;
                }

                AppendGroupStopWaitEvidence(
                    "Group Stop stable-standby evidence was accepted.",
                    result.Evidence,
                    result.Continuation);
                await ClearMotionWarningAfterVerifiedStateAsync(
                    operation + " and stable Group Standby were verified");
                WriteLog(
                    "Group Stop ACK accepted exactly once; status-only "
                    + "verification polls="
                    + result.StatusPollCount
                    + ", Stable="
                    + result.StableStandbySampleCount
                    + "/"
                    + result.RequiredStableSampleCount
                    + ".");
                WriteLog(operation + " safety verification PASS.");
                TextOperationState.Text = operation + " verified";
            }
            catch (Exception error)
            {
                var continuation =
                    GetGroupStopWaitContinuation(error);
                if (continuation != null && continuation.IsPending)
                {
                    pendingGroupStopWaitContinuation = continuation;
                }

                var evidence = GetGroupStopWaitEvidence(error);
                if (evidence != null
                    && evidence.LastObservedStatus != null)
                {
                    DisplayGroupStatus(evidence.LastObservedStatus);
                }

                AppendGroupStopWaitEvidence(
                    "Group Stop stable-standby completion was not proven; the accepted continuation remains available without replaying 0x2085.",
                    evidence,
                    continuation ?? currentStop);
                WriteLog(
                    operation
                    + " safety verification FAILED: "
                    + error.Message
                    + (error.InnerException == null
                        ? string.Empty
                        : " Inner: " + error.InnerException.Message)
                    + " Do not assume the group is stopped.");
                TextOperationState.Text = operation + " verification failed";
            }
            finally
            {
                safetyMonitorCount--;
                UpdateUiState();
            }
        }

        private async Task RunGroupPowerOffSafetyMonitorAsync(
            string operation,
            LMCGroupAxis currentGroup,
            long monitorSafetyGeneration,
            bool monitorReservationAlreadyHeld,
            GroupPowerRecoveryRecord verificationRecord)
        {
            if (!monitorReservationAlreadyHeld)
            {
                safetyMonitorCount++;
            }
            TextOperationState.Text = operation + " verifying PowerOn=False";
            WriteLog(
                operation
                + " is verifying three stable PowerOn=False samples; "
                + "Group Stop remains available.");
            UpdateUiState();

            try
            {
                using (sendPriorityCoordinator.BeginPreemptibleScope(
                    monitorSafetyGeneration,
                    operation + " safety verification"))
                {
                    var result = await ResumeOrObserveGroupPowerStateAsync(
                        currentGroup,
                        false,
                        operation);
                    EnsureNoNewSafetyRequestBeforeResultApplication(
                        monitorSafetyGeneration,
                        operation + " safety verification completion");
                    CompleteGroupPowerRecoveryAfterStableProof(
                        currentGroup,
                        false,
                        result,
                        verificationRecord,
                        operation + " stable Power Off proof");
                    var pendingDisableBeforeRetire = currentGroup
                        .PendingGroupDisableWaitContinuation;
                    if (pendingDisableBeforeRetire != null
                        && (groupProfileLockRecoveryRequired
                            || HasActiveGroupProfileLockRecoveryJournalRecord))
                    {
                        await EnsureGroupProfileLockRecoveryIdentityAsync(
                            currentGroup,
                            operation + " recovery completion",
                            RequireActiveGroupProfileLockRecoveryRecord(
                                operation + " recovery completion")
                                .ExpectedProfileLocked);
                    }
                    await RetirePendingGroupDisableAfterStablePowerOffAsync(
                        currentGroup,
                        result,
                        operation);
                    await ReconcilePendingGroupEnableSafeStateProofAsync(
                        currentGroup,
                        monitorSafetyGeneration,
                        "three stable PowerOn=False samples");
                    if (pendingDisableBeforeRetire == null
                        && (groupProfileLockRecoveryRequired
                            || HasActiveGroupProfileLockRecoveryJournalRecord))
                    {
                        await EnsureGroupProfileLockRecoveryIdentityAsync(
                            currentGroup,
                            operation + " recovery completion",
                            RequireActiveGroupProfileLockRecoveryRecord(
                                operation + " recovery completion")
                                .ExpectedProfileLocked);
                    }
                    ResolveGroupProfileLockRecoveryJournal(
                        operation + " stable Power Off proof");
                    groupPowerVerificationPending = false;
                    groupPowerOffVerificationPending = false;
                    groupStatusRefreshRequired = false;
                    groupActiveVerified = false;
                    groupIdentityConfigured = false;
                    ResetIdentityHomeCheckState();
                    pendingGroupDisableWaitContinuation = null;
                    groupProfileLockVerificationPending = false;
                    groupProfileUnlockVerificationPending = false;
                    ClearGroupProfileLockRecovery();
                    groupProfileLocked = false;
                    DisplayGroupStatus(result.FinalStatus);
                    TextGroupResult.Text += Environment.NewLine
                        + "Group Power Off verified; Status polls="
                        + result.StatusPollCount
                        + ", Stable="
                        + result.StableSampleCount
                        + "/"
                        + result.RequiredStableSampleCount
                        + ", 0x204B was not replayed by verification.";
                    EnsureNoNewSafetyRequestBeforeResultApplication(
                        monitorSafetyGeneration,
                        operation + " motion recovery resolution");
                    await ClearMotionWarningAfterVerifiedStateAsync(
                        operation
                        + " and three stable PowerOn=False samples were verified");
                    WriteLog(
                        operation
                        + " safety verification PASS with "
                        + result.StableSampleCount
                        + " stable samples.");
                    TextOperationState.Text = operation + " verified";
                    return;
                }
            }
            catch (Exception error)
            {
                PreserveGroupPowerWaitFailure(
                    currentGroup,
                    error,
                    verificationRecord,
                    false,
                    false,
                    false,
                    null,
                    operation);
                ReapplyActiveGroupProfileLockRecoveryAfterPowerOffFailure(
                    operation);
                WriteLog(
                    operation
                    + " safety verification FAILED: "
                    + error.Message
                    + " Do not assume the group is powered off or stopped. "
                    + "Resume verification; do not replay 0x204B.");
                TextOperationState.Text = operation + " verification failed";
                ButtonGroupPowerOff.Focus();
            }
            finally
            {
                safetyMonitorCount--;
                UpdateUiState();
            }
        }

        private async Task CloseCurrentConnectionAsync(bool reportCloseError)
        {
            var currentConnection = connection;
            var recoveryIdentityReadOnlyClose =
                IsRecoveryIdentityReadOnlyConnection(currentConnection);
            if (!recoveryIdentityReadOnlyClose
                && HasUnresolvedAxisPowerState()
                && currentConnection != null
                && currentConnection.IsConnected)
            {
                throw new InvalidOperationException(
                    "Close is blocked while Axis Power recovery is unresolved. "
                    + GetAxisPowerOnRecoveryGuidance());
            }

            if (!recoveryIdentityReadOnlyClose
                && HasUnresolvedAxisCommandState()
                && currentConnection != null
                && currentConnection.IsConnected)
            {
                throw new InvalidOperationException(
                    "Close is blocked while Axis Stop/Reset recovery is unresolved.");
            }

            if (!recoveryIdentityReadOnlyClose
                && HasUnresolvedAxisQualificationState()
                && currentConnection != null
                && currentConnection.IsConnected)
            {
                throw new InvalidOperationException(
                    "Close is blocked while Single Axis qualification recovery is unresolved. "
                    + GetAxisQualificationRecoveryGuidance());
            }

            if (!recoveryIdentityReadOnlyClose
                && HasUnresolvedGroupPowerState()
                && currentConnection != null
                && currentConnection.IsConnected)
            {
                throw new InvalidOperationException(
                    "Close is blocked while Group Power recovery is unresolved. "
                    + GetGroupPowerRecoveryGuidance());
            }

            if (!recoveryIdentityReadOnlyClose
                && motionMayBeActive
                && currentConnection != null
                && currentConnection.IsConnected)
            {
                throw new InvalidOperationException(
                    "Close is blocked because "
                    + motionOperation
                    + " may still be active on "
                    + motionAxisName
                    + ". Send Stop or PowerOff and verify standstill first.");
            }

            if (currentConnection == null)
            {
                return;
            }

            Exception closeError = null;
            if (reportCloseError)
            {
                try
                {
                    await currentConnection.CloseConnectionAsync(
                        CancellationToken.None);
                }
                catch (Exception error)
                {
                    closeError = error;
                }
            }
            else
            {
                await EnsureCompleteLocalConnectionCleanupAsync(
                    currentConnection,
                    "Connection replacement cleanup");
                closeError = currentConnection.LastCloseException;
            }

            if (!HasCompleteLocalConnectionCleanup(currentConnection))
            {
                await EnsureCompleteLocalConnectionCleanupAsync(
                    currentConnection,
                    "Explicit connection cleanup fallback");
            }

            if (ReferenceEquals(connection, currentConnection))
            {
                connection = null;
            }

            DetachConnection(currentConnection);
            ClearLoadedObjects();
            UpdateUiState();

            if (closeError == null)
            {
                return;
            }

            if (reportCloseError)
            {
                throw closeError;
            }

            WriteLog(
                "Connection cleanup warning retained after local cleanup. "
                + "Response={"
                + FormatRpcSessionInitResponse(
                    currentConnection.RpcCloseResponse)
                + "}, Failure="
                + closeError.GetType().Name
                + ": "
                + closeError.Message);
        }

        private RecoveryConnectionIdentityMismatchException
            CreateRecoveryConnectionIdentityMismatch(
                string operation,
                string recoveryOwner,
                uint storedDiagnosticsBootId,
                uint storedMapRevision,
                uint currentDiagnosticsBootId,
                uint currentMapRevision)
        {
            return new RecoveryConnectionIdentityMismatchException(
                operation
                + " is blocked because DiagnosticsBootId or MapRevision does not "
                + "match the durable "
                + recoveryOwner
                + " recovery record. Stored BootId=0x"
                + storedDiagnosticsBootId.ToString("X8")
                + ", current BootId=0x"
                + currentDiagnosticsBootId.ToString("X8")
                + ", stored MapRevision=0x"
                + storedMapRevision.ToString("X8")
                + ", current MapRevision=0x"
                + currentMapRevision.ToString("X8")
                + ".");
        }

        private void EnterRecoveryIdentityReadOnlyQuarantine(
            LMCConnection quarantinedConnection,
            RecoveryConnectionIdentityMismatchException error)
        {
            if (quarantinedConnection == null)
            {
                throw new ArgumentNullException(nameof(quarantinedConnection));
            }

            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            recoveryIdentityReadOnlyConnection = quarantinedConnection;
            recoveryIdentityReadOnlyReason = error.Message;
            WriteLog(
                "RECOVERY IDENTITY READ-ONLY QUARANTINE: "
                + error.Message
                + " The TCP/RPC connection remains open for ordinary non-D5 "
                + "read-only inspection and local draft editing. No recovery "
                + "record was resolved or replayed; all control, mutation, D5, cleanup, and "
                + "qualification operations are blocked. Close and Exit remain available.");
            UpdateUiState();
        }

        private void ClearRecoveryIdentityReadOnlyQuarantine()
        {
            recoveryIdentityReadOnlyConnection = null;
            recoveryIdentityReadOnlyReason = null;
        }

        private bool IsRecoveryIdentityReadOnlyConnection(
            LMCConnection candidate)
        {
            return candidate != null
                && ReferenceEquals(
                    candidate,
                    recoveryIdentityReadOnlyConnection);
        }

        private bool IsRecoveryIdentityReadOnlyExitPermitted()
        {
            var quarantinedConnection = recoveryIdentityReadOnlyConnection;
            if (quarantinedConnection == null)
            {
                return false;
            }

            var currentConnection = connection;
            return currentConnection == null
                || ReferenceEquals(currentConnection, quarantinedConnection);
        }

        private string GetRecoveryIdentityReadOnlyGuidance()
        {
            return "The stored recovery identity does not match the current PLC. "
                + "Only ordinary non-D5 read-only inspection, local draft editing, "
                + "and Close/Exit are allowed. Do not infer the old command result from the current "
                + "PLC state; the durable recovery record remains unchanged. "
                + (string.IsNullOrEmpty(recoveryIdentityReadOnlyReason)
                    ? string.Empty
                    : recoveryIdentityReadOnlyReason);
        }

        private sealed class RecoveryConnectionIdentityMismatchException
            : InvalidOperationException
        {
            internal RecoveryConnectionIdentityMismatchException(string message)
                : base(message)
            {
            }
        }

        private LMCConnection CreateCoordinatedConnection()
        {
            return new LMCConnection(
                new LMCConnectionOptions
                {
                    SendPriorityCoordinator = sendPriorityCoordinator,
                    CallbackRegistrationMode =
                        LMCCallbackRegistrationMode.Version2WakeHint,
                    CallbackRequestedMaxDatagramBytes = 52
                });
        }

        private void AttachConnection(LMCConnection newConnection)
        {
            lastCallbackV2Statistics = null;
            lastCallbackListenerError = null;
            newConnection.ConnectionStateChanged += Connection_StateChanged;
            newConnection.CallbackWakeHintReceived +=
                Connection_CallbackWakeHintReceived;
            newConnection.CallbackV2StatisticsChanged +=
                Connection_CallbackV2StatisticsChanged;
            newConnection.CallbackListenerError +=
                Connection_CallbackListenerError;
        }

        private void DetachConnection(LMCConnection oldConnection)
        {
            oldConnection.ConnectionStateChanged -= Connection_StateChanged;
            oldConnection.CallbackWakeHintReceived -=
                Connection_CallbackWakeHintReceived;
            oldConnection.CallbackV2StatisticsChanged -=
                Connection_CallbackV2StatisticsChanged;
            oldConnection.CallbackListenerError -=
                Connection_CallbackListenerError;
        }

        private void Connection_StateChanged(
            object sender,
            LMCConnectionStateChangedEventArgs e)
        {
            RunOnUi(
                () =>
                {
                    if (!ReferenceEquals(sender, connection))
                    {
                        return;
                    }

                    var eventConnection = sender as LMCConnection;
                    if (e.CurrentState != LMCConnectionState.Connected
                        && eventConnection != null
                        && eventConnection.IsConnected)
                    {
                        WriteLog(
                            "Ignored stale connection-state event "
                            + e.CurrentState
                            + " from an older transport session.");
                        return;
                    }

                    if (e.CurrentState == LMCConnectionState.Closing
                        || e.CurrentState == LMCConnectionState.Disconnected
                        || e.CurrentState == LMCConnectionState.Faulted)
                    {
                        lastRpcInitializationRetired = true;
                    }

                    WriteLog(
                        "Connection state "
                        + e.PreviousState
                        + " -> "
                        + e.CurrentState
                        + (e.Exception == null
                            ? string.Empty
                            : ": " + e.Exception.Message));
                    if (e.CurrentState != LMCConnectionState.Connected)
                    {
                        RetireSdoWriteActivationQualificationProof();
                        try
                        {
                            DiscardPendingGroupResetAfterConnectionLoss(
                                "connection state changed to "
                                + e.CurrentState);
                        }
                        catch (Exception groupResetJournalError)
                        {
                            SetGroupResetRecoveryJournalRuntimeError(
                                "connection-loss transition",
                                groupResetJournalError);
                            WriteLog(
                                "Group Reset connection-loss journal transition failed; "
                                + "the active record remains fail-closed: "
                                + groupResetJournalError.Message);
                            UpdateUiState();
                        }
                        var recoveryIdentityReadOnlyDisconnect =
                            IsRecoveryIdentityReadOnlyConnection(eventConnection);
                        if (!recoveryIdentityReadOnlyDisconnect)
                        {
                            try
                            {
                                PreserveAxisCommandRecoveryAfterConnectionLoss(
                                    sender as LMCConnection,
                                    e.Exception,
                                    "Connection state changed to "
                                        + e.CurrentState);
                            }
                            catch (Exception axisCommandJournalError)
                            {
                                SetAxisCommandRecoveryJournalRuntimeError(
                                    "connection-loss transition",
                                    axisCommandJournalError);
                            }
                            try
                            {
                                if (HasActiveAxisPowerRecoveryRecord)
                                {
                                    PreserveAxisPowerRecoveryAfterConnectionLoss(
                                        "Connection state changed to "
                                        + e.CurrentState);
                                }
                            }
                            catch (Exception axisJournalError)
                            {
                                if (string.IsNullOrEmpty(
                                    axisPowerOnRecoveryJournalRuntimeError))
                                {
                                    SetAxisPowerOnRecoveryJournalRuntimeError(
                                        "connection-loss transition",
                                        axisJournalError);
                                }
                                else
                                {
                                    WriteLog(
                                        "Axis Power On connection-loss journal transition failed; the active record remains fail-closed: "
                                        + axisJournalError.Message);
                                }
                            }
                            if (motionMayBeActive)
                            {
                                PromoteMotionUncertaintyJournal(
                                    "Connection state changed to " + e.CurrentState,
                                    true);
                            }
                            PromotePendingGroupProfileLockToRecovery(
                                "Connection state changed to " + e.CurrentState);
                            try
                            {
                                PreserveGroupPowerRecoveryAfterConnectionLoss(
                                    "Connection state changed to "
                                        + e.CurrentState);
                            }
                            catch (Exception groupPowerJournalError)
                            {
                                if (string.IsNullOrEmpty(
                                    groupPowerRecoveryJournalRuntimeError))
                                {
                                    SetGroupPowerRecoveryJournalRuntimeError(
                                        "connection-loss transition",
                                        groupPowerJournalError);
                                }
                                else
                                {
                                    WriteLog(
                                        "Group Power connection-loss journal transition failed; the active record remains fail-closed: "
                                        + groupPowerJournalError.Message);
                                }
                            }
                        }
                        else
                        {
                            WriteLog(
                                "Recovery-identity read-only connection closed; "
                                + "all durable recovery records were preserved without "
                                + "promotion, resolution, replacement, or replay.");
                        }
                        ClearTopologyIoState();
                        if (!recoveryIdentityReadOnlyDisconnect)
                        {
                            try
                            {
                                MarkDigitalOutputWriteConnectionLost();
                                MarkActiveDiagnosticsMutationConnectionLost();
                            }
                            catch (Exception journalError)
                            {
                                WriteLog(
                                    "Durable mutation connection-loss update failed; the active record remains fail-closed: "
                                    + journalError.Message);
                                RefreshDiagnosticsMutationJournalUi();
                            }
                        }
                    }
                    UpdateUiState();
                });
        }

        private void Connection_CallbackWakeHintReceived(
            object sender,
            LMCCallbackWakeHintEventArgs e)
        {
            RunOnUi(
                () => HandleCallbackWakeHintOnUi(sender, e));
        }

        private void Connection_CallbackV2StatisticsChanged(
            object sender,
            LMCCallbackV2StatisticsChangedEventArgs e)
        {
            RunOnUi(
                () =>
                {
                    var currentConnection = connection;
                    if (!ReferenceEquals(sender, currentConnection))
                    {
                        return;
                    }
                    if (!e.BelongsToCurrentSession(currentConnection))
                    {
                        return;
                    }

                    lastCallbackV2Statistics = e;
                    UpdateCallbackDiagnosticsUiState(currentConnection);
                });
        }

        private void HandleCallbackWakeHintOnUi(
            object sender,
            LMCCallbackWakeHintEventArgs e)
        {
            var currentConnection = connection;
            var wakeHint = e.WakeHint;
            if (!ReferenceEquals(sender, currentConnection))
            {
                WriteLog(
                    "D5 terminal wake ignored: stale connection owner, EventId=0x"
                    + wakeHint.EventId.ToString("X8"));
                return;
            }

            var ticket = diagnosticOperationTicket;
            if (ticket == null
                || !e.MatchesD5OperationTerminalTicket(
                    currentConnection,
                    ticket))
            {
                WriteLog(
                    "D5 terminal wake ignored: no exact current retained ticket, EventId=0x"
                    + wakeHint.EventId.ToString("X8")
                    + ", BootId=0x"
                    + wakeHint.BootId.ToString("X8"));
                return;
            }

            if (operationRunning
                || safetyCommandRunning
                || safetyMonitorCount > 0
                || qualificationRunning
                || callbackDiagnosticRefreshTicket != null)
            {
                WriteLog(
                    "D5 terminal wake skipped while busy; manual/poll refresh remains available. TicketId=0x"
                    + ticket.TicketId.ToString("X8"));
                return;
            }

            var operationSafetyGeneration = safetyRequestGeneration;
            callbackDiagnosticRefreshTicket = ticket;
            operationRunning = true;
            TextOperationState.Text = "Callback D5 status refresh running";
            UpdateUiState();
            _ = RefreshDiagnosticOperationFromWakeAsync(
                currentConnection,
                ticket,
                operationSafetyGeneration);
        }

        private async Task RefreshDiagnosticOperationFromWakeAsync(
            LMCConnection currentConnection,
            LMCOperationTicket ticket,
            long operationSafetyGeneration)
        {
            try
            {
                WriteLog(
                    "D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x"
                    + ticket.TicketId.ToString("X8"));
                bool applied;
                using (sendPriorityCoordinator.BeginPreemptibleScope(
                    operationSafetyGeneration,
                    "Callback D5 status refresh"))
                {
                    applied = await RefreshDiagnosticOperationCoreAsync(
                        currentConnection,
                        ticket,
                        CancellationToken.None,
                        "callback-d5-terminal-wake");
                }

                if (!ReferenceEquals(connection, currentConnection)
                    || !ReferenceEquals(
                        callbackDiagnosticRefreshTicket,
                        ticket))
                {
                    WriteLog(
                        "Ignored stale callback D5 status continuation after the connection or retained ticket changed. TicketId=0x"
                        + ticket.TicketId.ToString("X8"));
                    return;
                }

                if (applied)
                {
                    WriteLog(
                        "Callback D5 authoritative TCP status processed. TicketId=0x"
                        + ticket.TicketId.ToString("X8"));
                    TextOperationState.Text =
                        "Callback D5 status refresh completed";
                }
                else
                {
                    WriteLog(
                        "Callback D5 status ignored after TCP query because the retained ticket/session changed. TicketId=0x"
                        + ticket.TicketId.ToString("X8"));
                }
            }
            catch (Exception error)
            {
                if (ReferenceEquals(connection, currentConnection)
                    && ReferenceEquals(
                        callbackDiagnosticRefreshTicket,
                        ticket))
                {
                    WriteLog(
                        "Callback D5 authoritative TCP status query failed: "
                        + error.Message);
                    TextOperationState.Text =
                        "Callback D5 status refresh failed; use manual refresh";
                }
                else
                {
                    WriteLog(
                        "Ignored stale callback D5 status failure after the connection or retained ticket changed. TicketId=0x"
                        + ticket.TicketId.ToString("X8")
                        + ", Error="
                        + error.Message);
                }
            }
            finally
            {
                if (ReferenceEquals(connection, currentConnection)
                    && ReferenceEquals(
                        callbackDiagnosticRefreshTicket,
                        ticket))
                {
                    callbackDiagnosticRefreshTicket = null;
                    operationRunning = false;
                    UpdateUiState();
                }
            }
        }

        private void Connection_CallbackListenerError(
            object sender,
            LMCCallbackErrorEventArgs e)
        {
            RunOnUi(
                () =>
                {
                    if (!ReferenceEquals(sender, connection))
                    {
                        return;
                    }

                    WriteLog(
                        "Callback listener error: "
                        + (e.Exception == null
                            ? "unknown error"
                            : e.Exception.Message));
                    lastCallbackListenerError = e.Exception == null
                        ? "unknown error"
                        : e.Exception.GetType().Name
                            + ": "
                            + e.Exception.Message;
                    UpdateUiState();
                });
        }

        private void RunOnUi(Action action)
        {
            if (shutdownInProgress || Dispatcher.HasShutdownStarted)
            {
                return;
            }

            if (Dispatcher.CheckAccess())
            {
                action();
                return;
            }

            Dispatcher.BeginInvoke(action);
        }

        private async Task EnsureAxisPoweredOnAsync(LMCSingleAxis currentAxis)
        {
            var status = await currentAxis.ReadStatusResultAsync(
                CancellationToken.None);
            EnsureAxisStatusSuccess("Motion power check", status);
            if (!status.IsPowerOn)
            {
                throw new InvalidOperationException(
                    "Motion is blocked because Read Status reports PowerOn=false.");
            }
        }

        private async Task<LMCReadStatusResult> WaitForPowerStateAsync(
            LMCSingleAxis currentAxis,
            bool expectedPowerOn,
            int timeoutMilliseconds)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTime.UtcNow < deadline)
            {
                var status = await currentAxis.ReadStatusResultAsync(
                    CancellationToken.None);
                EnsureAxisStatusSuccess("Power state verification", status);
                if (status.IsPowerOn == expectedPowerOn)
                {
                    return status;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentAxis.AxisName
                + " did not reach PowerOn="
                + expectedPowerOn
                + " within "
                + timeoutMilliseconds
                + " ms.");
        }

        private async Task<LMCReadStatusResult> WaitForStandstillAsync(
            LMCSingleAxis currentAxis,
            int timeoutMilliseconds,
            int minimumDelayMilliseconds)
        {
            if (minimumDelayMilliseconds > 0)
            {
                await Task.Delay(minimumDelayMilliseconds);
            }

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            var stableSamples = 0;
            LMCReadStatusResult latest = null;
            while (DateTime.UtcNow < deadline)
            {
                latest = await currentAxis.ReadStatusResultAsync(
                    CancellationToken.None);
                EnsureAxisStatusSuccess("Standstill verification", latest);
                if (latest.IsStandstill)
                {
                    stableSamples++;
                    if (stableSamples >= 3)
                    {
                        return latest;
                    }
                }
                else
                {
                    stableSamples = 0;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentAxis.AxisName
                + " did not report stable standstill within "
                + timeoutMilliseconds
                + " ms.");
        }

        private async Task<LMCReadStatusResult>
            WaitForStablePowerOffAndStandstillAsync(
                LMCSingleAxis currentAxis,
                int timeoutMilliseconds)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(
                timeoutMilliseconds);
            var stableSamples = 0;
            LMCReadStatusResult latest = null;
            while (DateTime.UtcNow < deadline)
            {
                latest = await currentAxis.ReadStatusResultAsync(
                    CancellationToken.None);
                EnsureAxisStatusSuccess(
                    "Power Off and standstill verification",
                    latest);
                if (!latest.IsPowerOn && latest.IsStandstill)
                {
                    stableSamples++;
                    if (stableSamples >= 3)
                    {
                        return latest;
                    }
                }
                else
                {
                    stableSamples = 0;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentAxis.AxisName
                + " did not report three consecutive PowerOn=false and "
                + "Standstill samples within "
                + timeoutMilliseconds
                + " ms.");
        }

        private async Task<LMCReadStatusResult>
            WaitForFiniteMotionCompletionAsync(
                LMCSingleAxis currentAxis,
                int trackingGeneration,
                bool noMovementExpected,
                int timeoutMilliseconds)
        {
            await Task.Delay(250);

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            var stableSamples = 0;
            while (DateTime.UtcNow < deadline)
            {
                if (!IsTrackedMotion(
                    currentAxis.AxisName,
                    trackingGeneration))
                {
                    return null;
                }

                if (operationRunning
                    || safetyCommandRunning
                    || safetyMonitorCount > 0)
                {
                    await Task.Delay(25);
                    continue;
                }

                var monitorSafetyGeneration = safetyRequestGeneration;
                LMCReadStatusResult status;
                using (sendPriorityCoordinator.BeginPreemptibleScope(
                    monitorSafetyGeneration,
                    "Finite motion completion monitor"))
                {
                    status = await currentAxis.ReadStatusResultAsync(
                        CancellationToken.None);
                }
                if (!IsTrackedMotion(
                    currentAxis.AxisName,
                    trackingGeneration))
                {
                    return null;
                }

                EnsureAxisStatusSuccess(
                    "Finite motion standstill verification",
                    status);
                if (!status.IsStandstill)
                {
                    RecordMotionObserved(currentAxis.AxisName);
                    stableSamples = 0;
                }
                else if (!status.IsPowerOn
                    || noMovementExpected
                    || motionWasObserved)
                {
                    stableSamples++;
                    if (stableSamples >= 3)
                    {
                        return status;
                    }
                }
                else
                {
                    stableSamples = 0;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentAxis.AxisName
                + " did not show movement followed by three stable safe samples "
                + "within "
                + timeoutMilliseconds
                + " ms.");
        }

        private async Task<LMCGroupReadStatusResult>
            WaitForGroupInPositionAsync(
                LMCGroupAxis currentGroup,
                int timeoutMilliseconds,
                int minimumDelayMilliseconds)
        {
            if (minimumDelayMilliseconds > 0)
            {
                await Task.Delay(minimumDelayMilliseconds);
            }

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            var stableSamples = 0;
            LMCGroupReadStatusResult latest = null;
            while (DateTime.UtcNow < deadline)
            {
                latest = await currentGroup.GroupReadStatusResultAsync(
                    CancellationToken.None);
                EnsureGroupStatusSuccess(
                    "Group InPosition verification",
                    latest);
                if (IsGroupInPosition(latest))
                {
                    stableSamples++;
                    if (stableSamples >= 3)
                    {
                        return latest;
                    }
                }
                else
                {
                    stableSamples = 0;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentGroup.GroupName
                + " did not report stable Group InPosition within "
                + timeoutMilliseconds
                + " ms.");
        }

        private async Task<LMCGroupReadStatusResult>
            WaitForGroupMotionCompletionAsync(
                LMCGroupAxis currentGroup,
                int trackingGeneration,
                bool noMovementExpected,
                int timeoutMilliseconds)
        {
            await Task.Delay(250);

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            var stableSamples = 0;
            while (DateTime.UtcNow < deadline)
            {
                if (!IsTrackedMotion(
                    currentGroup.GroupName,
                    trackingGeneration))
                {
                    return null;
                }

                if (operationRunning
                    || safetyCommandRunning
                    || safetyMonitorCount > 0)
                {
                    await Task.Delay(25);
                    continue;
                }

                var monitorSafetyGeneration = safetyRequestGeneration;
                LMCGroupReadStatusResult status;
                using (sendPriorityCoordinator.BeginPreemptibleScope(
                    monitorSafetyGeneration,
                    "Group motion completion monitor"))
                {
                    status = await currentGroup.GroupReadStatusResultAsync(
                        CancellationToken.None);
                }
                if (!IsTrackedMotion(
                    currentGroup.GroupName,
                    trackingGeneration))
                {
                    return null;
                }

                EnsureGroupStatusSuccess(
                    "Group motion completion verification",
                    status);
                if (!IsGroupInPosition(status))
                {
                    RecordMotionObserved(currentGroup.GroupName);
                    stableSamples = 0;
                }
                else if (noMovementExpected || motionWasObserved)
                {
                    stableSamples++;
                    if (stableSamples >= 3)
                    {
                        return status;
                    }
                }
                else
                {
                    stableSamples = 0;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentGroup.GroupName
                + " did not show motion followed by three stable Group "
                + "InPosition samples within "
                + timeoutMilliseconds
                + " ms.");
        }

        private bool CanStartLiveCommand(
            string operation,
            bool allowPendingGroupReset = false)
        {
            var admission = allowPendingGroupReset
                ? EvaluateDiagnosticsAdmissionIgnoringPendingGroupReset(
                    DiagnosticsAdmissionOperation.NewLiveOrMutation)
                : EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation.NewLiveOrMutation);
            if (!admission.IsAllowed)
            {
                WriteLog(
                    CreateDiagnosticsAdmissionException(
                        operation,
                        admission).Message);
                return false;
            }

            if (HasUnresolvedAxisCommandState())
            {
                WriteLog(
                    operation
                    + " blocked by unresolved Axis Stop/Reset recovery. Use the matching status-only verification or safety takeover action.");
                return false;
            }

            if (HasUnresolvedAxisQualificationState())
            {
                WriteLog(
                    operation
                    + " blocked by unresolved Single Axis qualification recovery. "
                    + GetAxisQualificationRecoveryGuidance());
                return false;
            }

            if (AxisQualificationRecoveryJournalUnavailable)
            {
                WriteLog(
                    operation
                    + " blocked because the Single Axis qualification recovery journal is unavailable. "
                    + GetAxisQualificationRecoveryGuidance());
                return false;
            }

            if (HasUnresolvedAxisPowerState())
            {
                WriteLog(
                    operation
                    + " blocked by unresolved Axis Power state. "
                    + GetAxisPowerOnRecoveryGuidance());
                return false;
            }

            if (!allowPendingGroupReset
                && HasUnresolvedGroupResetState())
            {
                WriteLog(
                    operation
                    + " blocked by pending Group Reset verification. "
                    + GetGroupResetRecoveryGuidance());
                return false;
            }

            if (motionMayBeActive)
            {
                WriteLog(
                    operation
                    + " blocked because "
                    + motionOperation
                    + " may still be active on "
                    + motionAxisName
                    + ".");
                return false;
            }

            return true;
        }

        private bool CanStartMotionCommand(string operation)
        {
            if (!CanStartLiveCommand(operation))
            {
                return false;
            }

            if (MotionUncertaintyJournalCanArm)
            {
                return true;
            }

            WriteLog(
                operation
                + " blocked because the durable motion journal cannot arm. "
                + GetMotionUncertaintyJournalGuidance());
            return false;
        }

        private int MarkMotionUncertain(
            MotionUncertaintyTargetKind targetKind,
            string currentAxisName,
            ushort targetReference,
            string operation)
        {
            ArmMotionUncertaintyBeforeDispatch(
                targetKind,
                currentAxisName,
                targetReference,
                operation);
            motionTrackingGeneration++;
            motionMayBeActive = true;
            motionAxisName = currentAxisName;
            motionOperation = operation;
            motionWasObserved = false;
            motionTargetKind = targetKind;
            motionTargetReference = targetReference;
            motionRecoveryRequiresExplicitSafetyCommand = false;
            motionRecoverySafetyTrackingGeneration = 0;
            motionRecoverySafetyGeneration = 0;
            WriteLog(
                "SAFETY: "
                + operation
                + " send may start for "
                + currentAxisName
                + ". Motion state is uncertain until rejection or verified standstill.");
            UpdateUiState();
            return motionTrackingGeneration;
        }

        private void ClearMotionOnConfirmedRejection(
            string currentAxisName,
            string operation,
            LMC_Response response)
        {
            if (response != null
                && response.IsFrameValid
                && !response.IsSuccess
                && IsTrackedMotionAxis(currentAxisName))
            {
                ClearMotionWarningAfterConfirmedNoMotion(
                    operation + " was rejected by a valid response");
            }
        }

        private void ClearMotionOnConfirmedRejection(
            string currentAxisName,
            string operation,
            LMCAdminResponse response)
        {
            if (response != null
                && response.TransportResponse != null
                && response.TransportResponse.IsFrameValid
                && !response.IsSuccess
                && IsTrackedMotionAxis(currentAxisName))
            {
                ClearMotionWarningAfterConfirmedNoMotion(
                    operation + " was rejected by a valid response");
            }
        }

        private void ClearMotionWarningAfterConfirmedNoMotion(
            string reason,
            int? expectedTrackingGeneration = null)
        {
            ClearMotionWarningCore(reason, expectedTrackingGeneration);
        }

        private void ClearMotionWarningCore(
            string reason,
            int? expectedTrackingGeneration = null,
            Action afterMotionJournalResolvedBeforeVolatileClear = null)
        {
            if (!motionMayBeActive
                || (expectedTrackingGeneration.HasValue
                    && expectedTrackingGeneration.Value
                        != motionTrackingGeneration))
            {
                return;
            }

            EnsureExplicitMotionRecoverySafetyWasAccepted(reason);
            ResolveMotionUncertaintyJournal(reason);
            if (afterMotionJournalResolvedBeforeVolatileClear != null)
            {
                afterMotionJournalResolvedBeforeVolatileClear();
            }
            motionTrackingGeneration++;
            motionMayBeActive = false;
            motionAxisName = null;
            motionOperation = null;
            motionWasObserved = false;
            motionTargetKind = default(MotionUncertaintyTargetKind);
            motionTargetReference = 0;
            motionRecoveryRequiresExplicitSafetyCommand = false;
            motionRecoverySafetyTrackingGeneration = 0;
            motionRecoverySafetyGeneration = 0;
            WriteLog("Motion warning cleared: " + reason + ".");
            UpdateUiState();
        }

        private void RecordMotionObserved(string currentAxisName)
        {
            if (!IsTrackedMotionAxis(currentAxisName) || motionWasObserved)
            {
                return;
            }

            motionWasObserved = true;
            WriteLog(
                "SAFETY: Non-standstill motion was observed for "
                + currentAxisName
                + ".");
        }

        private bool IsTrackedMotionAxis(string currentAxisName)
        {
            return motionMayBeActive
                && string.Equals(
                    motionAxisName,
                    currentAxisName,
                    StringComparison.Ordinal);
        }

        private bool IsTrackedMotion(
            string currentAxisName,
            int trackingGeneration)
        {
            return IsTrackedMotionAxis(currentAxisName)
                && motionTrackingGeneration == trackingGeneration;
        }

        private MotionInput ReadFiniteMotionInput()
        {
            var unit = ReadAxisUnitSelection();
            return new MotionInput
            {
                PositionRaw = ToLasalDint(TextPosition.Text, unit, "Position"),
                VelocityRaw = ToPositiveLasalDint(
                    TextVelocity.Text,
                    unit,
                    "Velocity"),
                AccelerationRaw = ToPositiveLasalDint(
                    TextAcceleration.Text,
                    unit,
                    "Acceleration"),
                DecelerationRaw = ToPositiveLasalDint(
                    TextDeceleration.Text,
                    unit,
                    "Deceleration"),
                JerkRaw = ToNonNegativeLasalDint(
                    TextJerk.Text,
                    unit,
                    "Jerk")
            };
        }

        private MotionInput ReadVelocityMotionInput()
        {
            var unit = ReadAxisUnitSelection();
            if (!(ComboDirection.SelectedItem is LMC_DIRECTION direction)
                || (direction != LMC_DIRECTION.Positive
                    && direction != LMC_DIRECTION.Negative))
            {
                throw new InvalidOperationException(
                    "Velocity direction must be Positive or Negative.");
            }

            return new MotionInput
            {
                VelocityRaw = ToPositiveLasalDint(
                    TextVelocity.Text,
                    unit,
                    "Velocity"),
                AccelerationRaw = ToPositiveLasalDint(
                    TextAcceleration.Text,
                    unit,
                    "Acceleration"),
                JerkRaw = ToNonNegativeLasalDint(
                    TextJerk.Text,
                    unit,
                    "Jerk"),
                Direction = direction
            };
        }

        private MotionInput ReadStopInput()
        {
            var unit = ReadAxisUnitSelection();
            return new MotionInput
            {
                DecelerationRaw = ToPositiveLasalDint(
                    TextDeceleration.Text,
                    unit,
                    "Stop deceleration"),
                JerkRaw = ToNonNegativeLasalDint(
                    TextJerk.Text,
                    unit,
                    "Stop jerk")
            };
        }

        private GroupMotionInput ReadGroupMotionInput()
        {
            var unit = ReadGroupUnitSelection();
            if (!(ComboGroupTransition.SelectedItem
                is LMC_GROUP_TRANSITION_MODE transitionMode)
                || (transitionMode != LMC_GROUP_TRANSITION_MODE.ExactStop
                    && transitionMode
                        != LMC_GROUP_TRANSITION_MODE.ContinuousDirect))
            {
                throw new InvalidOperationException(
                    "Group transition must be ExactStop or ContinuousDirect.");
            }

            if (!(ComboGroupBuffer.SelectedItem is LMC_BUFFER_MODE bufferMode)
                || (bufferMode != LMC_BUFFER_MODE.Aborting
                    && bufferMode != LMC_BUFFER_MODE.Buffered))
            {
                throw new InvalidOperationException(
                    "Group buffer mode must be Aborting or Buffered.");
            }

            return new GroupMotionInput
            {
                PositionsRaw = new[]
                {
                    ToLasalDint(
                        TextGroupPositionX.Text,
                        unit,
                        "Group X target"),
                    ToLasalDint(
                        TextGroupPositionY.Text,
                        unit,
                        "Group Y target"),
                    ToLasalDint(
                        TextGroupPositionZ.Text,
                        unit,
                        "Group Z target"),
                    ToLasalDint(
                        TextGroupPositionU.Text,
                        unit,
                        "Group U target")
                },
                VelocityRaw = ToPositiveLasalDint(
                    TextGroupVelocity.Text,
                    unit,
                    "Group velocity"),
                AccelerationRaw = ToPositiveLasalDint(
                    TextGroupAcceleration.Text,
                    unit,
                    "Group acceleration"),
                DecelerationRaw = ToPositiveLasalDint(
                    TextGroupDeceleration.Text,
                    unit,
                    "Group deceleration"),
                JerkRaw = ToNonNegativeLasalDint(
                    TextGroupJerk.Text,
                    unit,
                    "Group jerk"),
                Options = new LMCGroupMotionOptions
                {
                    CoordinateSystem = ReadGroupMotionCoordinateSystem(),
                    TransitionMode = transitionMode,
                    BufferMode = bufferMode,
                    Execute = true
                }
            };
        }

        private GroupMotionInput ReadGroupStopInput()
        {
            var unit = ReadGroupUnitSelection();
            return new GroupMotionInput
            {
                DecelerationRaw = ToPositiveLasalDint(
                    TextGroupDeceleration.Text,
                    unit,
                    "Group stop deceleration"),
                JerkRaw = ToNonNegativeLasalDint(
                    TextGroupJerk.Text,
                    unit,
                    "Group stop jerk")
            };
        }

        private LMC_COORD_SYSTEM ReadGroupPositionCoordinateSystem()
        {
            if (!(ComboGroupCoordinate.SelectedItem
                is LMC_COORD_SYSTEM coordinateSystem)
                || (coordinateSystem != LMC_COORD_SYSTEM.None
                    && coordinateSystem != LMC_COORD_SYSTEM.Acs))
            {
                throw new InvalidOperationException(
                    "Group Read Position supports Coordinate=None or ACS only.");
            }

            return coordinateSystem;
        }

        private LMC_COORD_SYSTEM ReadGroupMotionCoordinateSystem()
        {
            if (!(ComboGroupCoordinate.SelectedItem
                is LMC_COORD_SYSTEM coordinateSystem)
                || coordinateSystem != LMC_COORD_SYSTEM.None)
            {
                throw new InvalidOperationException(
                    "Group motion currently supports Coordinate=None only. "
                    + "Select None before Move Linear.");
            }

            return coordinateSystem;
        }

        private static int CalculateGroupMotionMonitorTimeoutMilliseconds(
            long[] distancesRaw,
            int velocityRaw,
            int accelerationRaw,
            int decelerationRaw)
        {
            if (distancesRaw == null || distancesRaw.Length < 4)
            {
                throw new ArgumentException(
                    "Group monitor distance requires four XYZU values.",
                    nameof(distancesRaw));
            }

            if (velocityRaw <= 0
                || accelerationRaw <= 0
                || decelerationRaw <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(velocityRaw),
                    "Group monitor dynamics must be positive.");
            }

            var conservativePathRaw = distancesRaw
                .Take(4)
                .Sum(distance => Math.Abs((double)distance));
            var nominalSeconds = conservativePathRaw / velocityRaw;
            var accelerationSeconds = velocityRaw / (double)accelerationRaw;
            var decelerationSeconds = velocityRaw / (double)decelerationRaw;
            var estimatedMilliseconds = Math.Ceiling(
                ((nominalSeconds
                    + accelerationSeconds
                    + decelerationSeconds)
                    * 1.25
                    + 5.0)
                * 1000.0);

            return (int)Math.Max(
                MinimumGroupMotionMonitorMilliseconds,
                Math.Min(
                    MaximumGroupMotionMonitorMilliseconds,
                    estimatedMilliseconds));
        }

        private PlcUnitOption ReadGroupUnitSelection()
        {
            var unit = ComboGroupUnit.SelectedItem as PlcUnitOption;
            if (unit == null)
            {
                throw new InvalidOperationException(
                    "Select a group PLC application UNIT.");
            }

            return unit;
        }

        private PlcUnitOption ReadAxisUnitSelection()
        {
            var unit = ComboAxisUnit.SelectedItem as PlcUnitOption;
            if (unit == null)
            {
                throw new InvalidOperationException(
                    "Select an axis PLC application UNIT.");
            }

            return unit;
        }

        private static int ToLasalDint(
            string value,
            PlcUnitOption unit,
            string fieldName)
        {
            if (unit.IsRaw)
            {
                return ParseRawDint(value, fieldName);
            }

            var engineeringValue = ParseDouble(value, fieldName);
            return ScaleToLasalDint(
                engineeringValue,
                unit.Multiplier,
                fieldName);
        }

        private static int ToPositiveLasalDint(
            string value,
            PlcUnitOption unit,
            string fieldName)
        {
            if (unit.IsRaw)
            {
                var rawValue = ParseRawDint(value, fieldName);
                if (rawValue <= 0)
                {
                    throw new InvalidOperationException(
                        fieldName + " raw DINT must be greater than zero.");
                }

                return rawValue;
            }

            var engineeringValue = ParseDouble(value, fieldName);
            if (engineeringValue <= 0)
            {
                throw new InvalidOperationException(
                    fieldName + " must be greater than zero.");
            }

            var raw = ScaleToLasalDint(
                engineeringValue,
                unit.Multiplier,
                fieldName);
            if (raw <= 0)
            {
                throw new InvalidOperationException(
                    fieldName + " multiplied by UNIT must be at least 1 DINT count.");
            }

            return raw;
        }

        private static int ToNonNegativeLasalDint(
            string value,
            PlcUnitOption unit,
            string fieldName)
        {
            if (unit.IsRaw)
            {
                var rawValue = ParseRawDint(value, fieldName);
                if (rawValue < 0)
                {
                    throw new InvalidOperationException(
                        fieldName + " raw DINT must be zero or greater.");
                }

                return rawValue;
            }

            var engineeringValue = ParseDouble(value, fieldName);
            if (engineeringValue < 0)
            {
                throw new InvalidOperationException(
                    fieldName + " must be zero or greater.");
            }

            var raw = ScaleToLasalDint(
                engineeringValue,
                unit.Multiplier,
                fieldName);
            if (engineeringValue > 0 && raw <= 0)
            {
                throw new InvalidOperationException(
                    fieldName + " multiplied by UNIT must be at least 1 DINT count.");
            }

            return raw;
        }

        private static int ScaleToLasalDint(
            double engineeringValue,
            double unitMultiplier,
            string fieldName)
        {
            var scaled = engineeringValue * unitMultiplier;
            if (double.IsNaN(scaled) || double.IsInfinity(scaled))
            {
                throw new OverflowException(
                    fieldName + " multiplied by UNIT is not finite.");
            }

            var rounded = Math.Round(
                scaled,
                0,
                MidpointRounding.AwayFromZero);
            if (rounded < int.MinValue || rounded > int.MaxValue)
            {
                throw new OverflowException(
                    fieldName + " multiplied by UNIT is outside DINT range.");
            }

            return checked((int)rounded);
        }

        private static int ParseRawDint(string value, string fieldName)
        {
            int parsed;
            if (!int.TryParse(
                (value ?? string.Empty).Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out parsed))
            {
                throw new InvalidOperationException(
                    fieldName
                    + " must be an integer in the DINT range when UNIT is None / raw DINT.");
            }

            return parsed;
        }

        private static double ParseDouble(string value, string fieldName)
        {
            double parsed;
            if (!double.TryParse(
                (value ?? string.Empty).Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out parsed)
                || double.IsNaN(parsed)
                || double.IsInfinity(parsed))
            {
                throw new InvalidOperationException(
                    fieldName + " must be a finite number using '.' as decimal separator.");
            }

            return parsed;
        }

        private static int ParsePort(
            string value,
            string fieldName,
            bool allowZero)
        {
            int parsed;
            if (!int.TryParse(
                (value ?? string.Empty).Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out parsed)
                || parsed < (allowZero ? 0 : 1)
                || parsed > 65535)
            {
                throw new InvalidOperationException(
                    fieldName
                    + (allowZero
                        ? " must be between 0 and 65535."
                        : " must be between 1 and 65535."));
            }

            return parsed;
        }

        private static string RequiredText(string value, string fieldName)
        {
            var result = (value ?? string.Empty).Trim();
            if (result.Length == 0)
            {
                throw new InvalidOperationException(fieldName + " is required.");
            }

            return result;
        }

        private LMCConnection RequireConnection()
        {
            if (connection == null || !connection.IsConnected)
            {
                throw new InvalidOperationException("Connect to the PLC first.");
            }

            return connection;
        }

        private LMCSingleAxis RequireAxis()
        {
            RequireConnection();
            if (axis == null)
            {
                throw new InvalidOperationException("Load an axis object first.");
            }

            return axis;
        }

        private LMCGroupAxis RequireGroup()
        {
            RequireConnection();
            if (group == null)
            {
                throw new InvalidOperationException("Load a group object first.");
            }

            return group;
        }

        private void EnsureGroupActiveVerified()
        {
            if (groupActiveVerified)
            {
                return;
            }

            throw new InvalidOperationException(
                "Run Group Power On and complete its three-sample PowerOn=True "
                + "verification. If verification is pending, resume it without "
                + "replaying 0x204A.");
        }

        private void EnsureGroupReadyForMotion()
        {
            if (HasUnresolvedGroupResetState())
            {
                throw new InvalidOperationException(
                    "Move Linear is blocked while Group Reset verification is "
                    + "pending. "
                    + GetGroupResetRecoveryGuidance());
            }

            EnsureGroupActiveVerified();
            if (!groupIdentityConfigured)
            {
                throw new InvalidOperationException(
                    "Set Identity (Configure) before Move Linear.");
            }

            if (HasUnresolvedGroupProfileLockState())
            {
                throw new InvalidOperationException(
                    "Move Linear is blocked while Group Enable/Disable proof or "
                    + "durable profile-lock recovery is unresolved.");
            }

            if (!groupProfileLocked)
            {
                throw new InvalidOperationException(
                    groupProfileLockRecoveryRequired
                        ? "A Group Enable completion was discarded after a newer "
                            + "safety request. Run Disable or complete stable Power "
                            + "Off verification before Move Linear."
                        : groupProfileLockVerificationPending
                        ? "Resume Lock Verification until three consecutive "
                            + "powered Locked Standby samples are verified "
                            + "before Move Linear."
                        : "Enable (Lock Profile) and wait for three consecutive "
                            + "powered Locked Standby samples before Move Linear.");
            }
        }

        private LMCGroupEnableWaitContinuation
            GetPendingGroupEnableWaitContinuation(
                LMCGroupAxis currentGroup)
        {
            if (currentGroup == null)
            {
                pendingGroupEnableWaitContinuation = null;
                groupProfileLockVerificationPending =
                    groupProfileLockAcceptedRestartRecovery;
                return null;
            }

            pendingGroupEnableWaitContinuation =
                currentGroup.PendingGroupEnableWaitContinuation;
            groupProfileLockVerificationPending =
                groupProfileLockAcceptedRestartRecovery
                || pendingGroupEnableWaitContinuation != null;
            return pendingGroupEnableWaitContinuation;
        }

        private void PreservePendingGroupEnableWaitUi(
            LMCGroupAxis currentGroup,
            LMCGroupEnableWaitContinuation continuation,
            string reason)
        {
            var durableRecord = groupProfileLockRecoveryJournal == null
                ? null
                : groupProfileLockRecoveryJournal.CurrentRecord;
            var durableAccepted = durableRecord != null
                && durableRecord.IsActive
                && durableRecord.State
                    == GroupProfileLockRecoveryState.AcceptedAwaitingProof;
            var continuationReusable = durableAccepted
                && currentGroup != null
                && connection != null
                && connection.IsConnected
                && ReferenceEquals(group, currentGroup)
                && ReferenceEquals(
                    currentGroup.PendingGroupEnableWaitContinuation,
                    continuation)
                && continuation != null
                && continuation.IsPending;

            if (durableAccepted)
            {
                pendingGroupEnableWaitContinuation = continuationReusable
                    ? continuation
                    : null;
                groupProfileLockAcceptedRestartRecovery =
                    !continuationReusable;
                groupProfileLockVerificationPending = true;
                groupProfileLockRecoveryRequired = false;
                groupProfileLockRecoveryGroupName = durableRecord.GroupName;
                groupProfileLockRecoveryGroupReference =
                    durableRecord.GroupReference;
                groupProfileLockRecoveryEndpointIp =
                    durableRecord.EndpointIp;
                groupProfileLockRecoveryEndpointPort =
                    durableRecord.EndpointPort;
                groupProfileLockRecoveryDiagnosticsBootId =
                    durableRecord.DiagnosticsBootId;
                groupProfileLockRecoveryMapRevision =
                    durableRecord.MapRevision;
            }
            else
            {
                pendingGroupEnableWaitContinuation = continuation;
                groupProfileLockVerificationPending = continuation != null
                    && continuation.IsPending;
                ClearGroupProfileLockRecovery();
            }
            groupProfileLocked = false;

            if (continuation == null)
            {
                return;
            }

            if (continuation.LastObservedStatus != null)
            {
                DisplayGroupStatus(continuation.LastObservedStatus);
            }
            else
            {
                TextGroupResult.Text =
                    FormatResponse(continuation.Acknowledgement);
            }

            TextGroupResult.Text += Environment.NewLine
                + "GroupEnable ACK is preserved; no 0x2047 replay is allowed. "
                + "Polls="
                + continuation.PollCount
                + ", Stable="
                + continuation.StableSampleCount
                + "/"
                + continuation.RequiredStableSampleCount
                + (continuationReusable
                    ? ". Resume Lock Verification in this session to send status "
                        + "reads only."
                    : ". The session continuation was discarded; reconnect to "
                        + "the exact durable identity and resume status-only "
                        + "verification.")
                + Environment.NewLine
                + "Pending reason: "
                + reason;
            WriteLog(
                "Group Enable ACK preserved. Resume sends 0x2045 only; 0x2047 replay is blocked. "
                + "Polls="
                + continuation.PollCount
                + ", Stable="
                + continuation.StableSampleCount
                + "/"
                + continuation.RequiredStableSampleCount
                + ", SessionContinuationReusable="
                + continuationReusable
                + ".");
        }

        private void CompleteGroupEnableWaitUi(
            LMCGroupEnableWaitResult result)
        {
            if (result == null || result.FinalStatus == null)
            {
                throw new InvalidOperationException(
                    "Group Enable verification returned no final status.");
            }

            ResolveGroupProfileLockRecoveryJournal(
                "Group Enable verified Locked Standby");
            pendingGroupEnableWaitContinuation = null;
            groupProfileLockVerificationPending = false;
            ClearGroupProfileLockRecovery();
            groupProfileLocked = true;
            groupStatusRefreshRequired = false;
            DisplayGroupStatus(result.FinalStatus);
            TextGroupResult.Text += Environment.NewLine
                + "GroupEnable ACK accepted once; 0x2047 requests=1, Status polls="
                + result.PollCount
                + ", Stable="
                + result.StableSampleCount
                + "/"
                + result.Continuation.RequiredStableSampleCount
                + ", ReusedACK="
                + result.ReusedAcceptedAcknowledgement
                + ".";
            WriteLog(
                "Group Lock Ready verified with three consecutive powered Locked Standby samples. "
                + "GroupEnable was sent once; Move Linear is now available.");
        }

        private void CompleteGroupEnableStatusOnlyRecoveryUi(
            LMCGroupLockedStandbyWaitResult result)
        {
            var record = RequireActiveGroupProfileLockRecoveryRecord(
                "Accepted Group Enable status-only recovery");
            if (result == null
                || result.FinalStatus == null
                || !result.FinalStatus.IsSuccess
                || !result.FinalStatus.IsPowerOn
                || !result.FinalStatus.IsStandby
                || result.StableSampleCount
                    < result.RequiredStableSampleCount)
            {
                throw new InvalidOperationException(
                    "Accepted Group Enable recovery returned no stable powered "
                    + "Locked Standby proof.");
            }

            if (record.State
                != GroupProfileLockRecoveryState.AcceptedAwaitingProof)
            {
                throw new InvalidOperationException(
                    "Accepted Group Enable status proof cannot resolve a journal "
                    + "that is no longer AcceptedAwaitingProof.");
            }

            ResolveGroupProfileLockRecoveryJournal(
                "Accepted Group Enable status-only recovery");
            pendingGroupEnableWaitContinuation = null;
            groupProfileLockVerificationPending = false;
            ClearGroupProfileLockRecovery();
            groupProfileLocked = true;
            groupStatusRefreshRequired = false;
            groupActiveVerified = true;
            DisplayGroupStatus(result.FinalStatus);
            TextGroupResult.Text += Environment.NewLine
                + "Accepted GroupEnable recovered with status-only proof; "
                + "0x2047 requests=0, Status polls="
                + result.StatusPollCount
                + ", Stable="
                + result.StableSampleCount
                + "/"
                + result.RequiredStableSampleCount
                + ". Set Identity/Home Check state was not restored; Disable, "
                + "then re-establish preparation before motion.";
            WriteLog(
                "Accepted Group Enable recovery verified three consecutive "
                + "powered Locked Standby samples without replaying 0x2047. "
                + "Process-local Set Identity/Home Check remains fail-closed; "
                + "Disable and re-establish preparation before motion.");
        }

        private void MarkGroupProfileLockResultDiscarded(string operation)
        {
            PromoteGroupProfileLockRecoveryJournal(operation, true);
            pendingGroupEnableWaitContinuation = null;
            groupProfileLockVerificationPending = false;
            groupProfileLockRecoveryRequired = true;
            groupProfileLockRecoveryGroupName = group == null
                ? TextGroupName.Text.Trim()
                : group.GroupName;
            groupProfileLocked = false;
            TextGroupResult.Text += Environment.NewLine
                + "Lock completion was discarded after a newer safety request. "
                + "Do not replay 0x2047; run Disable or complete stable Power Off "
                + "verification first.";
            WriteLog(
                operation
                + " returned after a newer safety request. The stale lock result "
                + "was not applied. Disable or stable Power Off verification is "
                    + "required before another Enable.");
        }

        private void MarkGroupProfileLockCompletionOutcomeUncertain(
            string operation)
        {
            PromoteGroupProfileLockRecoveryJournal(
                operation + " completion outcome uncertainty");
            pendingGroupEnableWaitContinuation = null;
            groupProfileLockVerificationPending =
                groupProfileLockAcceptedRestartRecovery;
            if (string.IsNullOrWhiteSpace(
                    groupProfileLockRecoveryGroupName))
            {
                groupProfileLockRecoveryGroupName = group == null
                    ? TextGroupName.Text.Trim()
                    : group.GroupName;
            }

            groupProfileLocked = false;
            TextGroupResult.Text += Environment.NewLine
                + (groupProfileLockAcceptedRestartRecovery
                    ? "Group Enable ACK remains durably accepted. Reconnect to "
                        + "the exact endpoint/group identity and resume status-only "
                        + "verification; do not replay 0x2047."
                    : "Group Enable completion could not be applied safely. Do not "
                        + "replay 0x2047; reconnect to the durable endpoint/group "
                        + "identity and run Disable or complete stable Power Off "
                        + "verification.");
        }

        private void CompleteGroupPowerOnWaitUi(
            LMCGroupPowerStateWaitResult result,
            bool resumedAcceptedCommand)
        {
            if (result == null || result.FinalStatus == null)
            {
                throw new InvalidOperationException(
                    "Group Power On verification returned no final status.");
            }

            groupPowerVerificationPending = false;
            groupStatusRefreshRequired = false;
            groupActiveVerified = true;
            DisplayGroupStatus(result.FinalStatus);
            TextGroupResult.Text += Environment.NewLine
                + "Group Power On verified; Status polls="
                + result.StatusPollCount
                + ", Stable="
                + result.StableSampleCount
                + "/"
                + result.RequiredStableSampleCount
                + ", ResumedWithout0x204AReplay="
                + resumedAcceptedCommand
                + ".";
            WriteLog(
                "Group Power Ready/ACTIVE verified with three consecutive "
                + "PowerOn=True samples. Set Identity is now available; 0x204A "
                + "was not replayed by verification.");
        }

        private async Task ReconcilePendingGroupEnableSafeStateProofAsync(
            LMCGroupAxis currentGroup,
            long proofSafetyGeneration,
            string observedState)
        {
            var continuation =
                GetPendingGroupEnableWaitContinuation(currentGroup);
            if (continuation == null)
            {
                return;
            }

            if (currentGroup.TryReleasePendingGroupEnableForRetry(
                    continuation))
            {
                try
                {
                    await EnsureGroupProfileLockRecoveryIdentityAsync(
                        currentGroup,
                        "Pending Group Enable safe-state proof",
                        true);
                    if (proofSafetyGeneration != safetyRequestGeneration)
                    {
                        MarkGroupProfileLockResultDiscarded(
                            "Pending Group Enable safe-state proof post-identity completion");
                    }
                    EnsureNoNewSafetyRequestBeforeResultApplication(
                        proofSafetyGeneration,
                        "Pending Group Enable safe-state proof post-identity completion");
                    ResolveGroupProfileLockRecoveryJournal(
                        "Pending Group Enable safe-state proof");
                }
                catch
                {
                    PromoteGroupProfileLockRecoveryJournal(
                        "Pending Group Enable safe-state proof identity failure",
                        true);
                    throw;
                }

                pendingGroupEnableWaitContinuation = null;
                groupProfileLockVerificationPending = false;
                ClearGroupProfileLockRecovery();
                groupProfileLocked = false;
                WriteLog(
                    "Pending Group Enable was released after stable safe-state proof ("
                    + observedState
                    + "). A new Enable may now be sent explicitly.");
                return;
            }

            groupProfileLockVerificationPending = true;
            WriteLog(
                "Group Enable remains pending after "
                + observedState
                + ". Disabled proof="
                + continuation.DisabledUnlockedSampleCount
                + "/3, PowerOff proof="
                + continuation.PoweredOffSampleCount
                + "/3.");
        }

        private void ResetIdentityHomeCheckState()
        {
            groupIdentityHomeCheckComplete = false;
            groupIdentityHomeCheckPassed = false;
            if (TextKinHomeStatus != null)
            {
                TextKinHomeStatus.Text = "Home Check: not checked.";
            }
        }

        private void ResetGroupPreparationState()
        {
            ClearGroupPowerSessionContinuation();
            groupStatusRefreshRequired = false;
            groupActiveVerified = false;
            groupIdentityConfigured = false;
            ResetIdentityHomeCheckState();
            pendingGroupEnableWaitContinuation = null;
            pendingGroupDisableWaitContinuation = null;
            groupProfileLockVerificationPending =
                groupProfileLockAcceptedRestartRecovery;
            groupProfileUnlockVerificationPending =
                groupProfileUnlockAcceptedRestartRecovery;
            groupProfileLocked = false;
            groupResetObservedLockedStandby = false;
        }

        private void ClearGroupProfileLockRecovery()
        {
            groupProfileLockAcceptedRestartRecovery = false;
            groupProfileUnlockAcceptedRestartRecovery = false;
            groupProfileLockRecoveryRequired = false;
            groupProfileLockRecoveryGroupName = null;
            ClearGroupProfileLockRecoveryIdentity();
        }

        private bool HasPendingAxisHandleBoundContinuation()
        {
            return (pendingAxisResetWaitContinuation != null
                    && pendingAxisResetWaitContinuation.IsPending)
                || (pendingAxisStopWaitContinuation != null
                    && pendingAxisStopWaitContinuation.IsPending)
                || (pendingAxisPowerOffWaitContinuation != null
                    && pendingAxisPowerOffWaitContinuation.IsPending);
        }

        private bool HasPendingGroupProfileLockContinuation()
        {
            if (groupProfileLockVerificationPending
                || groupProfileUnlockVerificationPending
                || pendingGroupEnableWaitContinuation != null
                || pendingGroupDisableWaitContinuation != null)
            {
                return true;
            }

            var currentGroup = group;
            return currentGroup != null
                && (currentGroup.PendingGroupEnableWaitContinuation != null
                    || currentGroup.PendingGroupDisableWaitContinuation != null);
        }

        private bool HasUnresolvedGroupProfileLockState()
        {
            return groupProfileLockRecoveryRequired
                || HasPendingGroupProfileLockContinuation()
                || HasActiveGroupProfileLockRecoveryJournalRecord;
        }

        private void EnsureNoUnresolvedGroupProfileLockMutation(
            string operation)
        {
            if (!HasUnresolvedGroupProfileLockState())
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " is blocked while Group Enable verification or durable "
                + "profile-lock recovery is unresolved. Run Disable or complete "
                + "stable Power Off verification first.");
        }

        private string GetUnresolvedGroupProfileLockName()
        {
            if (!string.IsNullOrWhiteSpace(groupProfileLockRecoveryGroupName))
            {
                return groupProfileLockRecoveryGroupName;
            }

            var currentGroup = group;
            if (currentGroup != null
                && !string.IsNullOrWhiteSpace(currentGroup.GroupName))
            {
                return currentGroup.GroupName;
            }

            return TextGroupName == null
                ? string.Empty
                : TextGroupName.Text.Trim();
        }

        private void PromotePendingGroupProfileLockToRecovery(string reason)
        {
            if (!HasPendingGroupProfileLockContinuation()
                && !HasActiveGroupProfileLockRecoveryJournalRecord)
            {
                return;
            }

            groupProfileLockRecoveryGroupName =
                GetUnresolvedGroupProfileLockName();
            pendingGroupEnableWaitContinuation = null;
            groupProfileLocked = false;
            PromoteGroupProfileLockRecoveryJournal(reason);
            groupProfileLockVerificationPending =
                groupProfileLockAcceptedRestartRecovery;
            WriteLog(
                reason
                + " invalidated the session-bound Group Enable continuation. "
                + (groupProfileLockAcceptedRestartRecovery
                    ? "The ACK remains durably accepted; reconnect to the same "
                        + "group and resume status-only verification without "
                        + "replaying 0x2047."
                    : "Do not replay 0x2047; reconnect to the same group and run "
                        + "Disable or complete stable Power Off verification."));
        }

        private void InvalidateGroupPreparationAfterStatusFailure()
        {
            groupStatusRefreshRequired = true;
            groupActiveVerified = false;
            groupProfileLocked = false;
            UpdateUiState();
        }

        private void ClearLoadedObjects()
        {
            InvalidateAxisQualificationConfirmations();
            axis = null;
            group = null;
            pendingAxisStopWaitContinuation = null;
            pendingAxisResetWaitContinuation = null;
            axisResetWaitInterferenceConfirmed = false;
            InvalidateAxisCommandSessionContinuations();
            pendingGroupStopWaitContinuation = null;
            ClearGroupResetSessionState();
            ClearAxisPowerSessionContinuation();
            ClearMotionLookupIdentities();
            ResetGroupPreparationState();
            ClearDiagnosticsState();
            ClearReadOnlyApiState();
            if (TextAxisReference != null)
            {
                TextAxisReference.Text = "not loaded";
            }

            if (TextGroupReference != null)
            {
                TextGroupReference.Text = "not loaded";
            }
        }

        private void DisplayAxisStatus(LMCReadStatusResult result)
        {
            TextAxisResult.Text =
                "State=0x"
                + result.State.ToString("X8")
                + Environment.NewLine
                + "PowerOn="
                + result.IsPowerOn
                + ", Home/Referenced="
                + result.IsReferenced
                + ", Standstill="
                + result.IsStandstill
                + Environment.NewLine
                + "FunctionStatus=0x"
                + result.FunctionStatus.ToString("X4")
                + ", ErrorId="
                + result.ErrorId
                + Environment.NewLine
                + "AxisErrorId="
                + result.AxisErrorId
                + ", StatusWord=0x"
                + result.StatusWord.ToString("X4");
        }

        private void AppendAxisResetWaitEvidence(
            LMCAxisResetWaitEvidence evidence,
            string summary)
        {
            if (evidence == null)
            {
                return;
            }

            TextAxisResult.Text += Environment.NewLine
                + summary
                + Environment.NewLine
                + "Reset submission="
                + evidence.SubmissionOutcome
                + ", CommandMayHaveBeenSent="
                + evidence.CommandMayHaveBeenSent
                + Environment.NewLine
                + "Status polls="
                + evidence.StatusPollCount
                + ", Stable AxisErrorId=0="
                + evidence.StableErrorClearSampleCount
                + "/"
                + evidence.RequiredStableSampleCount
                + Environment.NewLine
                + "ResetMutationGeneration="
                + evidence.ResetMutationGeneration
                + ", ObservedMutationGeneration="
                + evidence.ObservedMutationGeneration
                + ", InterveningMutationDetected="
                + evidence.InterveningMutationDetected
                + Environment.NewLine
                + "Boundary: this proves the LASAL AxisErrorId observation only; "
                + "DS402 Fault and drive error-register clearance are not proven.";
        }

        private static LMCAxisResetWaitEvidence GetAxisResetWaitEvidence(
            Exception error)
        {
            var rejected = error as LMCAxisResetRejectedException;
            if (rejected != null)
            {
                return rejected.Evidence;
            }

            var submission = error as LMCAxisResetSubmissionException;
            if (submission != null)
            {
                return submission.Evidence;
            }

            var timeout = error as LMCAxisResetWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Evidence;
            }

            var canceled = error as LMCAxisResetWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Evidence;
            }

            var interference = error as LMCAxisResetInterferenceException;
            if (interference != null)
            {
                return interference.Evidence;
            }

            var status = error as LMCAxisResetStatusException;
            return status == null ? null : status.Evidence;
        }

        private static LMCAxisResetWaitContinuation
            GetAxisResetWaitContinuation(Exception error)
        {
            var timeout = error as LMCAxisResetWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Continuation;
            }

            var canceled = error as LMCAxisResetWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Continuation;
            }

            var interference = error as LMCAxisResetInterferenceException;
            if (interference != null)
            {
                return interference.Continuation;
            }

            var status = error as LMCAxisResetStatusException;
            return status == null ? null : status.Continuation;
        }

        private void AppendAxisPowerOnWaitEvidence(
            LMCAxisPowerOnWaitEvidence evidence,
            string summary)
        {
            if (evidence == null)
            {
                return;
            }

            TextAxisResult.Text += Environment.NewLine
                + summary
                + Environment.NewLine
                + "Power On submission="
                + evidence.SubmissionOutcome
                + ", CommandMayHaveBeenSent="
                + evidence.CommandMayHaveBeenSent
                + Environment.NewLine
                + "PowerOnAccepted="
                + evidence.PowerOnAccepted
                + ", AckPresent="
                + (evidence.PowerOnAcknowledgement != null)
                + Environment.NewLine;

            var acknowledgement = evidence.PowerOnAcknowledgement;
            if (acknowledgement != null)
            {
                TextAxisResult.Text += "PowerOn ACK: HeaderStatus="
                    + acknowledgement.HeaderStatus
                    + ", CommandStatus="
                    + acknowledgement.CommandStatus
                    + ", ErrorId="
                    + acknowledgement.ErrorId
                    + ", PayloadLength="
                    + acknowledgement.PayloadLength
                    + ", FrameValid="
                    + acknowledgement.IsFrameValid
                    + Environment.NewLine;

                LMCErrorDescription errorDescription;
                if (acknowledgement.ErrorId != 0
                    && LMCErrorCatalog.TryDescribe(
                        LMCErrorDomain.AdapterCommand,
                        acknowledgement.ErrorId,
                        out errorDescription))
                {
                    TextAxisResult.Text += "PowerOn ACK meaning: "
                        + errorDescription.Symbol
                        + " - "
                        + errorDescription.Description
                        + Environment.NewLine
                        + "PowerOn ACK action: "
                        + errorDescription.Resolution
                        + Environment.NewLine;
                }
            }

            TextAxisResult.Text += "Status polls="
                + evidence.StatusPollCount
                + ", Stable PowerOn="
                + evidence.StablePowerOnSampleCount
                + "/"
                + evidence.RequiredStableSampleCount
                + Environment.NewLine
                + "TransportInvalidatedAtDeadline="
                + evidence.TransportInvalidatedAtDeadline
                + Environment.NewLine
                + "Boundary: ACK proves Power On acceptance only; stable "
                + "PowerOn status and physical drive readiness require separate proof.";
        }

        private static LMCAxisPowerOnWaitEvidence
            GetAxisPowerOnWaitEvidence(Exception error)
        {
            var rejected = error as LMCAxisPowerOnRejectedException;
            if (rejected != null)
            {
                return rejected.Evidence;
            }

            var submission = error as LMCAxisPowerOnSubmissionException;
            if (submission != null)
            {
                return submission.Evidence;
            }

            var timeout = error as LMCAxisPowerStateWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Evidence;
            }

            var canceled = error as LMCAxisPowerStateWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Evidence;
            }

            var status = error as LMCAxisPowerStateStatusException;
            return status == null ? null : status.Evidence;
        }

        private void AppendAxisStopWaitEvidence(
            LMCAxisStopWaitEvidence evidence,
            string summary)
        {
            if (evidence == null)
            {
                return;
            }

            TextAxisResult.Text += Environment.NewLine
                + summary
                + Environment.NewLine
                + "Stop submission="
                + evidence.SubmissionOutcome
                + ", CommandMayHaveBeenSent="
                + evidence.CommandMayHaveBeenSent
                + Environment.NewLine
                + "StopAccepted="
                + evidence.StopAccepted
                + ", AckPresent="
                + (evidence.StopAcknowledgement != null)
                + Environment.NewLine
                + "Status polls="
                + evidence.StatusPollCount
                + ", Stable Standstill="
                + evidence.StableStandstillSampleCount
                + "/"
                + evidence.RequiredStableSampleCount
                + Environment.NewLine
                + "StopMutationGeneration="
                + evidence.StopMutationGeneration
                + ", ObservedMutationGeneration="
                + evidence.ObservedMutationGeneration
                + ", InterveningMutationDetected="
                + evidence.InterveningMutationDetected
                + Environment.NewLine
                + "TransportInvalidatedAtDeadline="
                + evidence.TransportInvalidatedAtDeadline
                + Environment.NewLine
                + "Boundary: this proves successful LASAL 0x2028 Standstill "
                + "samples only; independent DS402 or physical-stop proof is "
                + "not claimed.";
        }

        private void AppendAxisPowerOffWaitEvidence(
            LMCAxisPowerOffWaitEvidence evidence,
            string summary)
        {
            if (evidence == null)
            {
                return;
            }

            TextAxisResult.Text += Environment.NewLine
                + summary
                + Environment.NewLine
                + "PowerOff submission="
                + evidence.SubmissionOutcome
                + ", CommandMayHaveBeenSent="
                + evidence.CommandMayHaveBeenSent
                + Environment.NewLine
                + "PowerOffAccepted="
                + evidence.PowerOffAccepted
                + ", AckPresent="
                + (evidence.PowerOffAcknowledgement != null)
                + Environment.NewLine
                + "Status polls="
                + evidence.StatusPollCount
                + ", Stable PowerOff+Standstill="
                + evidence.StablePowerOffStandstillSampleCount
                + "/"
                + evidence.RequiredStableSampleCount
                + Environment.NewLine
                + "PowerOffMutationGeneration="
                + evidence.PowerOffMutationGeneration
                + ", ObservedMutationGeneration="
                + evidence.ObservedMutationGeneration
                + ", InterveningMutationDetected="
                + evidence.InterveningMutationDetected
                + Environment.NewLine
                + "TransportInvalidatedAtDeadline="
                + evidence.TransportInvalidatedAtDeadline
                + Environment.NewLine
                + "Boundary: this proves successful LASAL 0x2028 PowerOff and "
                + "Standstill samples only; independent DS402 or physical-stop "
                + "proof is not claimed.";
        }

        private static LMCAxisPowerOffWaitEvidence
            GetAxisPowerOffWaitEvidence(Exception error)
        {
            var rejected = error as LMCAxisPowerOffRejectedException;
            if (rejected != null)
            {
                return rejected.Evidence;
            }

            var submission = error as LMCAxisPowerOffSubmissionException;
            if (submission != null)
            {
                return submission.Evidence;
            }

            var timeout = error as LMCAxisPowerOffWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Evidence;
            }

            var canceled = error as LMCAxisPowerOffWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Evidence;
            }

            var interference = error as LMCAxisPowerOffInterferenceException;
            if (interference != null)
            {
                return interference.Evidence;
            }

            var status = error as LMCAxisPowerOffStatusException;
            return status == null ? null : status.Evidence;
        }

        private static LMCAxisPowerOffWaitContinuation
            GetAxisPowerOffWaitContinuation(Exception error)
        {
            var timeout = error as LMCAxisPowerOffWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Continuation;
            }

            var canceled = error as LMCAxisPowerOffWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Continuation;
            }

            var interference = error as LMCAxisPowerOffInterferenceException;
            if (interference != null)
            {
                return interference.Continuation;
            }

            var status = error as LMCAxisPowerOffStatusException;
            return status == null ? null : status.Continuation;
        }

        private static LMCAxisStopWaitEvidence GetAxisStopWaitEvidence(
            Exception error)
        {
            var rejected = error as LMCAxisStopRejectedException;
            if (rejected != null)
            {
                return rejected.Evidence;
            }

            var submission = error as LMCAxisStopSubmissionException;
            if (submission != null)
            {
                return submission.Evidence;
            }

            var timeout = error as LMCAxisStopWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Evidence;
            }

            var canceled = error as LMCAxisStopWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Evidence;
            }

            var interference = error as LMCAxisStopInterferenceException;
            if (interference != null)
            {
                return interference.Evidence;
            }

            var status = error as LMCAxisStopStatusException;
            return status == null ? null : status.Evidence;
        }

        private static LMCAxisStopWaitContinuation
            GetAxisStopWaitContinuation(Exception error)
        {
            var timeout = error as LMCAxisStopWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Continuation;
            }

            var canceled = error as LMCAxisStopWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Continuation;
            }

            var interference = error as LMCAxisStopInterferenceException;
            if (interference != null)
            {
                return interference.Continuation;
            }

            var status = error as LMCAxisStopStatusException;
            return status == null ? null : status.Continuation;
        }

        private void AppendGroupStopWaitEvidence(
            string summary,
            LMCGroupStopWaitEvidence evidence,
            LMCGroupStopWaitContinuation continuation)
        {
            if (evidence == null)
            {
                return;
            }

            TextGroupResult.Text += Environment.NewLine
                + summary
                + Environment.NewLine
                + "Group Stop submission="
                + evidence.SubmissionOutcome
                + ", CommandMayHaveBeenSent="
                + evidence.CommandMayHaveBeenSent
                + Environment.NewLine
                + "StopAccepted="
                + evidence.StopAccepted
                + ", AckPresent="
                + (evidence.StopAcknowledgement != null)
                + Environment.NewLine
                + "Status polls="
                + evidence.StatusPollCount
                + ", Stable Standby="
                + evidence.StableStandbySampleCount
                + "/"
                + evidence.RequiredStableSampleCount
                + Environment.NewLine
                + "StopMutationGeneration="
                + evidence.StopMutationGeneration
                + ", ObservedMutationGeneration="
                + evidence.ObservedMutationGeneration
                + ", InterveningMutation="
                + evidence.InterveningMutationDetected
                + Environment.NewLine
                + "TransportInvalidatedAtDeadline="
                + evidence.TransportInvalidatedAtDeadline
                + ", ContinuationPending="
                + (continuation != null && continuation.IsPending)
                + ", ContinuationSuperseded="
                + (continuation != null && continuation.IsSuperseded)
                + Environment.NewLine
                + "Boundary: this proves successful LASAL 0x2045 Standby "
                + "samples only; independent DS402 or physical-stop proof is "
                + "not claimed.";
        }

        private static LMCGroupStopWaitEvidence GetGroupStopWaitEvidence(
            Exception error)
        {
            var rejected = error as LMCGroupStopRejectedException;
            if (rejected != null)
            {
                return rejected.Evidence;
            }

            var submission = error as LMCGroupStopSubmissionException;
            if (submission != null)
            {
                return submission.Evidence;
            }

            var timeout = error as LMCGroupStopWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Evidence;
            }

            var canceled = error as LMCGroupStopWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Evidence;
            }

            var status = error as LMCGroupStopStatusException;
            if (status != null)
            {
                return status.Evidence;
            }

            var interference = error as LMCGroupStopInterferenceException;
            return interference == null ? null : interference.Evidence;
        }

        private static LMCGroupStopWaitContinuation
            GetGroupStopWaitContinuation(Exception error)
        {
            var timeout = error as LMCGroupStopWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Continuation;
            }

            var canceled = error as LMCGroupStopWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Continuation;
            }

            var status = error as LMCGroupStopStatusException;
            if (status != null)
            {
                return status.Continuation;
            }

            var interference = error as LMCGroupStopInterferenceException;
            return interference == null
                ? null
                : interference.Continuation;
        }

        private void DisplayGroupStatus(LMCGroupReadStatusResult result)
        {
            TextGroupResult.Text =
                "State=0x"
                + result.State.ToString("X8")
                + Environment.NewLine
                + "PowerOn="
                + result.IsPowerOn
                + Environment.NewLine
                + "Disabled/Unlocked="
                + result.IsDisabled
                + ", Enabled/LockedStandby="
                + result.IsStandby
                + Environment.NewLine
                + "FunctionStatus=0x"
                + result.FunctionStatus.ToString("X4")
                + ", ErrorId="
                + result.ErrorId
                + Environment.NewLine
                + "GroupErrorId="
                + result.GroupErrorId;
        }

        private void DisplayGroupPosition(
            LMCGroupReadActualPositionResult result,
            PlcUnitOption unit)
        {
            var positions = result.PositionsRaw;
            var raw = positions
                .Select(
                    (position, index) =>
                        "["
                        + index
                        + "]="
                        + position)
                .ToArray();
            var engineering = unit.IsRaw
                ? new[] { "conversion disabled (None / raw DINT)" }
                : positions
                    .Take(4)
                    .Select(
                        (position, index) =>
                            "XYZU"[index]
                            + "="
                            + (position / (double)unit.Multiplier).ToString(
                                "0.########",
                                CultureInfo.InvariantCulture)
                            + " "
                            + unit.Symbol)
                    .ToArray();

            TextGroupResult.Text =
                "Coordinate="
                + result.CoordinateSystem
                + Environment.NewLine
                + "Engineering: "
                + string.Join(", ", engineering)
                + Environment.NewLine
                + "Raw DINT: "
                + string.Join(", ", raw)
                + Environment.NewLine
                + "FunctionStatus=0x"
                + result.FunctionStatus.ToString("X4")
                + ", ErrorId="
                + result.ErrorId;
        }

        private static string FormatEngineeringPosition(
            int positionRaw,
            PlcUnitOption unit)
        {
            if (unit.IsRaw)
            {
                return "PLC UNIT=None / raw DINT; engineering conversion disabled";
            }

            return "Engineering="
                + (positionRaw / (double)unit.Multiplier).ToString(
                    "0.########",
                    CultureInfo.InvariantCulture)
                + " "
                + unit.Symbol;
        }

        private static string FormatGroupPositionsRaw(int[] positions)
        {
            if (positions == null)
            {
                return "<null>";
            }

            const string labels = "XYZU";
            return string.Join(
                ", ",
                positions
                    .Take(4)
                    .Select(
                        (position, index) =>
                            labels[index]
                            + "="
                            + position));
        }

        private static bool IsGroupInPosition(
            LMCGroupReadStatusResult result)
        {
            return result != null && result.IsStandby;
        }

        private static void EnsureResponseSuccess(
            string operation,
            LMC_Response response)
        {
            if (response != null && response.IsFrameValid && response.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. "
                + (response == null
                    ? "No response."
                    : FormatResponse(response)));
        }

        private static void EnsureAdminResponseSuccess(
            string operation,
            LMCAdminResponse response)
        {
            if (response != null && response.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. "
                + FormatAdminResponse(response));
        }

        private static void EnsureAxisStatusSuccess(
            string operation,
            LMCReadStatusResult result)
        {
            if (result != null && result.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ", AxisErrorId="
                + (result == null ? 0 : result.AxisErrorId)
                + ".");
        }

        private static void EnsureAxisStatusReadSuccess(
            string operation,
            LMCReadStatusResult result)
        {
            if (result != null && result.IsReadSuccessful)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " read failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ", FunctionStatus=0x"
                + (result == null ? 0 : result.FunctionStatus).ToString(
                    "X4",
                    CultureInfo.InvariantCulture)
                + ", "
                + FormatResponse(result == null ? null : result.Response)
                + ".");
        }

        private static void EnsureAxisPositionSuccess(
            string operation,
            LMCReadActualPositionResult result)
        {
            if (result != null && result.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ".");
        }

        private static void EnsureGroupStatusSuccess(
            string operation,
            LMCGroupReadStatusResult result)
        {
            if (result != null && result.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ", GroupErrorId="
                + (result == null ? 0 : result.GroupErrorId)
                + ".");
        }

        private static void EnsureGroupStatusReadSuccess(
            string operation,
            LMCGroupReadStatusResult result)
        {
            if (result != null && result.IsReadSuccessful)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " read failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ", FunctionStatus=0x"
                + (result == null ? 0 : result.FunctionStatus).ToString(
                    "X4",
                    CultureInfo.InvariantCulture)
                + ", "
                + FormatResponse(result == null ? null : result.Response)
                + ".");
        }

        private static void EnsureGroupPositionSuccess(
            string operation,
            LMCGroupReadActualPositionResult result)
        {
            if (result != null && result.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ".");
        }

        private static void EnsureGroupMembersSuccess(
            string operation,
            LMCGroupMembersInfoResult result)
        {
            if (result != null && result.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ".");
        }

        private static string FormatResponse(LMC_Response response)
        {
            if (response == null)
            {
                return "Response=<null>";
            }

            return
                "FrameValid="
                + response.IsFrameValid
                + ", Success="
                + response.IsSuccess
                + ", Status="
                + response.Status
                + ", ErrorId="
                + response.ErrorId
                + ", Bytes="
                + (response.Raw == null ? 0 : response.Raw.Length);
        }

        private static string FormatAdminResponse(LMCAdminResponse response)
        {
            if (response == null)
            {
                return "AdminResponse=<null>";
            }

            return
                "Schema="
                + response.SchemaVersion
                + ", CommandStatus="
                + response.CommandStatus
                + ", ErrorId="
                + response.ErrorId
                + ", RequestId="
                + response.RequestId
                + ", Detail="
                + response.DetailCode
                + " ("
                + response.DetailCodeValue
                + "), Transport="
                + FormatResponse(response.TransportResponse);
        }

        private void UpdateUiState()
        {
            if (!uiInitializationComplete
                || shutdownInProgress
                || ButtonConnect == null)
            {
                return;
            }

            var currentConnection = connection;
            var connected = currentConnection != null
                && currentConnection.IsConnected;
            var recoveryIdentityReadOnly =
                IsRecoveryIdentityReadOnlyConnection(currentConnection);
            var axisReady = connected && axis != null;
            var groupReady = connected && group != null;
            pendingAxisPowerOnWaitContinuation = axisReady
                ? axis.PendingPowerOnWaitContinuation
                : null;
            pendingGroupEnableWaitContinuation = groupReady
                ? group.PendingGroupEnableWaitContinuation
                : null;
            pendingGroupDisableWaitContinuation = groupReady
                ? group.PendingGroupDisableWaitContinuation
                : null;
            if (groupReady
                && !groupResetSubmissionUncertain
                && !groupResetSupersededByLaterMutation
                && !groupResetSessionContinuationDiscarded)
            {
                var sdkGroupResetContinuation =
                    group.PendingGroupResetWaitContinuation;
                if (sdkGroupResetContinuation != null
                    && sdkGroupResetContinuation.IsPending)
                {
                    pendingGroupResetWaitContinuation =
                        sdkGroupResetContinuation;
                    groupResetVerificationPending = true;
                }
            }
            groupProfileLockVerificationPending =
                groupProfileLockAcceptedRestartRecovery
                || pendingGroupEnableWaitContinuation != null;
            groupProfileUnlockVerificationPending =
                groupProfileUnlockAcceptedRestartRecovery
                || pendingGroupDisableWaitContinuation != null;
            var idle = !operationRunning
                && !safetyCommandRunning
                && safetyMonitorCount == 0
                && !qualificationRunning;
            var safetySendAvailable = !safetyCommandRunning
                && !connectionTransitionRunning
                && !recoveryIdentityReadOnly;
            var diagnosticMutationUnresolved =
                HasUnresolvedDiagnosticMutation;
            var diagnosticMutationCommandInterlocked =
                HasDiagnosticsMutationCommandInterlock;
            var connectAdmission = EvaluateDiagnosticsAdmission(
                DiagnosticsAdmissionOperation.ConnectOrReconnect);
            var closeConnectionAdmission = EvaluateDiagnosticsAdmission(
                DiagnosticsAdmissionOperation.CloseConnection);
            var groupResetTransitionPending =
                HasUnresolvedGroupResetState();
            var liveCommandAllowedIgnoringGroupReset = idle
                && !recoveryIdentityReadOnly
                && !motionMayBeActive
                && !diagnosticMutationCommandInterlocked
                && !AxisPowerOnRecoveryJournalUnavailable
                && !AxisCommandRecoveryJournalUnavailable
                && !AxisQualificationRecoveryJournalUnavailable
                && !GroupPowerRecoveryJournalUnavailable
                && !GroupResetRecoveryJournalUnavailable
                && !MaintenanceActionRecoveryJournalUnavailable
                && !HasUnresolvedAxisPowerState()
                && !HasUnresolvedAxisCommandState()
                && !HasUnresolvedAxisQualificationState()
                && !HasUnresolvedGroupPowerState();
            if (HasUnresolvedMaintenanceAction)
            {
                liveCommandAllowedIgnoringGroupReset = false;
            }
            var liveCommandAllowed =
                liveCommandAllowedIgnoringGroupReset
                && !groupResetTransitionPending;
            var axisPowerRecoveryRecord =
                GetActiveAxisPowerRecoveryRecord();
            var axisCommandRecoveryRecord =
                GetActiveAxisCommandRecoveryRecord();
            var exactPendingAxisReset =
                GetExactPendingAxisResetContinuation(
                    axis,
                    axisCommandRecoveryRecord);
            var exactPendingAxisStop =
                GetExactPendingAxisStopContinuation(
                    axis,
                    axisCommandRecoveryRecord);
            var groupPowerRecoveryRecord = HasActiveGroupPowerRecoveryRecord
                ? groupPowerRecoveryJournal.CurrentRecord
                : null;
            var groupProfileRecoveryRecord =
                HasActiveGroupProfileLockRecoveryJournalRecord
                    ? groupProfileLockRecoveryJournal.CurrentRecord
                    : null;
            var groupPowerTransitionPending = HasUnresolvedGroupPowerState();
            var groupMotionCoordinateReady =
                ComboGroupCoordinate.SelectedItem is LMC_COORD_SYSTEM
                    groupCoordinate
                && groupCoordinate == LMC_COORD_SYSTEM.None;

            ButtonConnect.IsEnabled = idle
                && !recoveryRecordRetirementRestartRequired
                && (!HasActiveAxisQualificationRecoveryRecord || !connected)
                && (!motionMayBeActive
                    || (MotionRecoveryReconnectAvailable && !connected))
                && connectAdmission.IsAllowed
                && (!HasUnresolvedGroupProfileLockState() || !connected)
                && (!HasUnresolvedGroupPowerState() || !connected)
                && (!groupResetTransitionPending
                    || GroupResetRecoveryReconnectAvailable);
            ButtonCloseConnection.IsEnabled =
                idle
                && currentConnection != null
                && closeConnectionAdmission.IsAllowed
                && (recoveryIdentityReadOnly
                    || (!motionMayBeActive
                        && !groupPowerTransitionPending
                        && !HasUnresolvedAxisPowerState()
                        && !HasUnresolvedAxisQualificationState()
                        && !HasUnresolvedGroupProfileLockState()
                        && !HasUnresolvedGroupPowerState()
                        && !groupResetTransitionPending
                        && !HasUnresolvedAxisCommandState()));
            TextRemoteIp.IsEnabled = idle
                && currentConnection == null
                && !motionMayBeActive
                && !HasUnresolvedGroupProfileLockState()
                && !HasActiveGroupPowerRecoveryRecord
                && !HasActiveGroupResetRecoveryRecord
                && axisCommandRecoveryRecord == null
                && axisPowerRecoveryRecord == null
                && !HasActiveAxisQualificationRecoveryRecord
                && !HasUnresolvedMaintenanceAction;
            TextRemotePort.IsEnabled = idle
                && currentConnection == null
                && !motionMayBeActive
                && !HasUnresolvedGroupProfileLockState()
                && !HasActiveGroupPowerRecoveryRecord
                && !HasActiveGroupResetRecoveryRecord
                && axisCommandRecoveryRecord == null
                && axisPowerRecoveryRecord == null
                && !HasActiveAxisQualificationRecoveryRecord
                && !HasUnresolvedMaintenanceAction;
            TextLocalIp.IsEnabled = idle
                && currentConnection == null
                && !HasActiveGroupResetRecoveryRecord;
            TextCallbackPort.IsEnabled = idle
                && currentConnection == null
                && !HasActiveGroupResetRecoveryRecord;

            TextAxisName.IsEnabled = idle
                && (recoveryIdentityReadOnly
                    || (!motionMayBeActive
                        && axisPowerRecoveryRecord == null
                            && axisCommandRecoveryRecord == null
                            && !HasActiveAxisQualificationRecoveryRecord
                            && !HasUnresolvedMaintenanceAction
                            && !HasPendingAxisHandleBoundContinuation()));
            ButtonLookupAxis.IsEnabled = connected
                && idle
                && (recoveryIdentityReadOnly
                    || ((!motionMayBeActive
                            || (IsMotionRecoveryTargetKind(
                                    MotionUncertaintyTargetKind.Axis)
                                && axis == null))
                        && (axisPowerRecoveryRecord == null || axis == null)
                        && (axisCommandRecoveryRecord == null || axis == null)
                        && (!HasActiveAxisQualificationRecoveryRecord
                            || axis == null)
                        && !HasPendingAxisHandleBoundContinuation()));
            ButtonReadStatus.IsEnabled = idle
                && (axisReady || (connected && recoveryIdentityReadOnly));
            ButtonReadPosition.IsEnabled = idle
                && (axisReady || (connected && recoveryIdentityReadOnly));
            var axisPowerOnResumeAvailable = axisReady
                && idle
                && axisPowerRecoveryRecord != null
                && axisPowerRecoveryRecord.ExpectedPowerOn
                && ((pendingAxisPowerOnWaitContinuation != null
                        && pendingAxisPowerOnWaitContinuation.IsPending)
                    || axisPowerOnAcceptedRestartRecovery)
                && !axisPowerOnRecoveryRequired;
            var axisPowerOnSendAvailable = axisReady
                && liveCommandAllowed
                && AxisPowerOnRecoveryJournalCanArm;
            ButtonPowerOn.IsEnabled = axisPowerOnResumeAvailable
                || axisPowerOnSendAvailable;
            ButtonPowerOn.Content = axisPowerRecoveryRecord != null
                    && (!axisPowerRecoveryRecord.ExpectedPowerOn
                        || axisPowerOnRecoveryRequired)
                ? "Power On Replay Blocked - Send Power Off"
                : (axisPowerOnResumeAvailable
                    ? "Resume Power On Verification (No 0x2023 Replay)"
                    : "Power On");
            var axisPowerOffStatusOnlyAvailable = axisReady
                && idle
                && axisPowerRecoveryRecord != null
                && !axisPowerRecoveryRecord.ExpectedPowerOn
                && !axisPowerOffReplacementAllowed;
            var axisPowerOffSendAvailable = axisReady
                && safetySendAvailable
                && (axisPowerRecoveryRecord == null
                    || axisPowerRecoveryRecord.ExpectedPowerOn
                    || axisPowerOffReplacementAllowed)
                && (!motionMayBeActive
                    || IsTrackedMotionTarget(axis));
            ButtonPowerOff.IsEnabled = axisPowerOffStatusOnlyAvailable
                || axisPowerOffSendAvailable;
            ButtonPowerOff.Content = axisPowerOffReplacementAllowed
                ? "Power Off Again (Confirmed Interference)"
                : (axisPowerRecoveryRecord != null
                        && !axisPowerRecoveryRecord.ExpectedPowerOn
                    ? "Resume Power Off Verification (No 0x2023 Replay)"
                    : (axisPowerRecoveryRecord != null
                            && axisPowerRecoveryRecord.ExpectedPowerOn
                        ? "Power Off Safety Takeover"
                        : (AxisPowerOnRecoveryJournalUnavailable
                            ? "Power Off (Durability Degraded)"
                            : "Power Off")));
            var axisResetResumeAvailable = axisReady
                && idle
                && axisCommandRecoveryRecord != null
                && axisCommandRecoveryRecord.Operation
                    == AxisCommandRecoveryOperation.Reset
                && ((axisCommandRecoveryRecord.State
                            == AxisCommandRecoveryState.AcceptedAwaitingProof
                        && (exactPendingAxisReset != null
                            || axisResetAcceptedRestartRecovery))
                    || (axisCommandRecoveryRecord.State
                            == AxisCommandRecoveryState.RecoveryRequired
                        && exactPendingAxisReset != null))
                && !axisResetWaitInterferenceConfirmed;
            var axisResetRetryAvailable = axisReady
                && idle
                && axisCommandRecoveryRecord != null
                && axisCommandRecoveryRecord.Operation
                    == AxisCommandRecoveryOperation.Reset
                && axisCommandRecoveryRecord.State
                    == AxisCommandRecoveryState.RecoveryRequired
                && (exactPendingAxisReset == null
                    || axisResetWaitInterferenceConfirmed)
                && !AxisCommandRecoveryJournalUnavailable;
            var axisResetFreshAvailable = axisReady
                && liveCommandAllowed
                && AxisCommandRecoveryJournalCanArm;
            ButtonReset.IsEnabled = axisResetResumeAvailable
                || axisResetRetryAvailable
                || axisResetFreshAvailable;
            ButtonReset.Content = axisResetResumeAvailable
                ? "Resume Reset Verification (No 0x2024 Replay)"
                : (axisResetRetryAvailable
                    ? (axisResetWaitInterferenceConfirmed
                        ? "Reset Again (Confirmed Interference)"
                        : "Retry Reset (Outcome Uncertain)")
                    : (axisCommandRecoveryRecord != null
                            && axisCommandRecoveryRecord.Operation
                                == AxisCommandRecoveryOperation.Stop
                        ? "Reset Blocked by Stop Recovery"
                        : "Reset"));
            var axisStopStatusOnlyAvailable = axisReady
                && idle
                && axisCommandRecoveryRecord != null
                && axisCommandRecoveryRecord.Operation
                    == AxisCommandRecoveryOperation.Stop
                && ((axisCommandRecoveryRecord.State
                            == AxisCommandRecoveryState.AcceptedAwaitingProof
                        && (exactPendingAxisStop != null
                            || axisStopAcceptedRestartRecovery))
                    || (axisCommandRecoveryRecord.State
                            == AxisCommandRecoveryState.RecoveryRequired
                        && exactPendingAxisStop != null));
            var axisStopRetryAvailable = axisReady
                && safetySendAvailable
                && axisCommandRecoveryRecord != null
                && axisCommandRecoveryRecord.Operation
                    == AxisCommandRecoveryOperation.Stop
                && axisCommandRecoveryRecord.State
                    == AxisCommandRecoveryState.RecoveryRequired
                && exactPendingAxisStop == null
                && !AxisCommandRecoveryJournalUnavailable;
            var axisStopTakeoverAvailable = axisReady
                && safetySendAvailable
                && axisCommandRecoveryRecord != null
                && axisCommandRecoveryRecord.Operation
                    == AxisCommandRecoveryOperation.Reset
                && !AxisCommandRecoveryJournalUnavailable;
            var axisStopFreshAvailable = axisReady
                && safetySendAvailable
                && axisCommandRecoveryRecord == null
                && AxisCommandRecoveryJournalCanArm;
            ButtonStop.IsEnabled = (axisStopStatusOnlyAvailable
                    || axisStopRetryAvailable
                    || axisStopTakeoverAvailable
                    || axisStopFreshAvailable)
                && (!motionMayBeActive || IsTrackedMotionTarget(axis));
            ButtonStop.Content = axisStopStatusOnlyAvailable
                ? "Resume Stop Verification (No 0x2022 Replay)"
                : (axisStopRetryAvailable
                    ? "Retry Stop (Outcome Uncertain)"
                    : (axisStopTakeoverAvailable
                        ? "Stop Safety Takeover"
                        : "Stop"));
            ButtonMoveAbsolute.IsEnabled = axisReady
                && liveCommandAllowed
                && MotionUncertaintyJournalCanArm;
            ButtonMoveRelative.IsEnabled = axisReady
                && liveCommandAllowed
                && MotionUncertaintyJournalCanArm;
            ButtonMoveVelocity.IsEnabled = axisReady
                && liveCommandAllowed
                && MotionUncertaintyJournalCanArm;

            var localEngineeringDraftAllowed = idle
                && (recoveryIdentityReadOnly || !motionMayBeActive);
            ComboAxisUnit.IsEnabled = localEngineeringDraftAllowed;
            TextPosition.IsEnabled = localEngineeringDraftAllowed;
            TextVelocity.IsEnabled = localEngineeringDraftAllowed;
            TextAcceleration.IsEnabled = localEngineeringDraftAllowed;
            TextDeceleration.IsEnabled = !operationRunning
                && !safetyCommandRunning
                && (axisCommandRecoveryRecord == null
                    || axisCommandRecoveryRecord.Operation
                        != AxisCommandRecoveryOperation.Stop);
            TextJerk.IsEnabled = !operationRunning
                && !safetyCommandRunning;
            ComboDirection.IsEnabled = localEngineeringDraftAllowed;

            TextGroupName.IsEnabled = idle
                && (recoveryIdentityReadOnly
                    || (!motionMayBeActive
                        && !groupPowerTransitionPending
                        && !groupResetTransitionPending
                        && !HasUnresolvedGroupProfileLockState()
                        && !HasActiveGroupPowerRecoveryRecord
                        && !HasActiveGroupResetRecoveryRecord));
            ButtonLookupGroup.IsEnabled = connected
                && idle
                && (recoveryIdentityReadOnly
                    || ((!motionMayBeActive
                            || (IsMotionRecoveryTargetKind(
                                    MotionUncertaintyTargetKind.Group)
                                && group == null))
                        && (!groupPowerTransitionPending || group == null)
                        && (!groupResetTransitionPending || group == null)
                        && (!HasPendingGroupProfileLockContinuation()
                            || HasAcceptedGroupProfileLockRecoveryRecord
                            || HasAcceptedGroupProfileUnlockRecoveryRecord)
                        && (!groupProfileLockRecoveryRequired || group == null)
                        && (!HasActiveGroupPowerRecoveryRecord || group == null)));
            ButtonGetMembers.IsEnabled = idle
                && (recoveryIdentityReadOnly
                    ? connected
                    : groupReady
                        && !motionMayBeActive
                        && !groupPowerOffVerificationPending);
            ButtonGroupReadStatus.IsEnabled = idle
                && (groupReady || (connected && recoveryIdentityReadOnly));
            ButtonGroupReadStatus.Content = recoveryIdentityReadOnly
                ? "Read Status (Inspection Only)"
                : (groupResetTransitionPending
                    ? "Observe Pending Reset (Single Group Status)"
                    : (groupPowerOffVerificationPending
                    ? "Observe Pending Power Off (Single Status)"
                    : (groupPowerVerificationPending
                        ? "Observe Pending Power On (Single Status)"
                        : (groupProfileLockRecoveryRequired
                            ? "Observe Lock State (Safe Recovery Required)"
                            : (groupProfileLockVerificationPending
                                ? "Verify Pending Lock State (Read Status)"
                                : "2 / 5 Read Status (Power Ready / Lock Ready)")))));
            ButtonGroupReadPosition.IsEnabled = idle
                && (recoveryIdentityReadOnly
                    ? connected
                    : groupReady && !groupPowerOffVerificationPending);
            var groupPowerOnResumeAvailable = groupReady
                && idle
                && groupPowerRecoveryRecord != null
                && groupPowerRecoveryRecord.ExpectedPowerOn
                && (groupPowerRecoveryRecord.State
                        == GroupPowerRecoveryState.AcceptedAwaitingProof
                    || (pendingGroupPowerStateWaitContinuation != null
                        && pendingGroupPowerStateWaitContinuation.IsPending
                        && pendingGroupPowerStateWaitContinuation.ExpectedPowerOn
                        && ReferenceEquals(
                            group.PendingGroupPowerStateWaitContinuation,
                            pendingGroupPowerStateWaitContinuation)))
                && !groupPowerRecoveryRequired;
            var groupPowerOnSendAvailable = groupReady
                && liveCommandAllowed
                && !groupActiveVerified
                && groupPowerRecoveryRecord == null
                && GroupPowerRecoveryJournalCanArm
                && !groupStatusRefreshRequired
                && !groupProfileLockVerificationPending
                && !groupProfileLockRecoveryRequired
                && !HasActiveGroupProfileLockRecoveryJournalRecord
                && !groupProfileLocked;
            ButtonGroupPowerOn.IsEnabled = groupPowerOnResumeAvailable
                || groupPowerOnSendAvailable;
            ButtonGroupPowerOn.Content = groupPowerRecoveryRecord != null
                    && groupPowerRecoveryRecord.ExpectedPowerOn
                    && groupPowerRecoveryRequired
                ? "Power On Replay Blocked - Send Power Off"
                : (groupPowerOnResumeAvailable
                    ? "Resume Power On Verification (No 0x204A Replay)"
                    : "1 Power On");
            var groupPowerOffStatusOnlyAvailable = groupReady
                && idle
                && groupPowerRecoveryRecord != null
                && !groupPowerRecoveryRecord.ExpectedPowerOn
                && !groupPowerOffReplacementAllowed;
            var groupPowerOffTakeoverAvailable = groupReady
                && safetySendAvailable
                && groupPowerRecoveryRecord != null
                && groupPowerRecoveryRecord.ExpectedPowerOn;
            var groupPowerOffReplacementAvailable = groupReady
                && safetySendAvailable
                && groupPowerRecoveryRecord != null
                && !groupPowerRecoveryRecord.ExpectedPowerOn
                && groupPowerOffReplacementAllowed;
            var groupPowerOffSendAvailable = groupReady
                && safetySendAvailable
                && groupPowerRecoveryRecord == null
                && GroupPowerRecoveryJournalCanArm
                && (!motionMayBeActive
                    || IsTrackedMotionTarget(group));
            ButtonGroupPowerOff.IsEnabled = groupPowerOffStatusOnlyAvailable
                || groupPowerOffTakeoverAvailable
                || groupPowerOffReplacementAvailable
                || groupPowerOffSendAvailable;
            ButtonGroupPowerOff.Content = groupPowerOffReplacementAvailable
                ? "Power Off Again (Confirmed Interference)"
                : (groupPowerOffTakeoverAvailable
                    ? "Send Power Off Safety Takeover"
                    : (groupPowerOffStatusOnlyAvailable
                        ? "Resume Power Off Verification (No 0x204B Replay)"
                        : "7 Power Off"));
            var groupProfileLockResumeAvailable = groupReady
                && liveCommandAllowed
                && !groupPowerOffVerificationPending
                && !groupProfileLockRecoveryRequired
                && !GroupProfileLockRecoveryJournalUnavailable
                && (groupProfileRecoveryRecord == null
                    || groupProfileRecoveryRecord.ExpectedProfileLocked)
                && ((pendingGroupEnableWaitContinuation != null
                        && pendingGroupEnableWaitContinuation.IsPending)
                    || HasAcceptedGroupProfileLockRecoveryRecord);
            var groupProfileLockSendAvailable = groupReady
                && liveCommandAllowed
                && !groupPowerOffVerificationPending
                && groupActiveVerified
                && groupIdentityConfigured
                && !groupProfileLockRecoveryRequired
                && !groupProfileLocked
                && GroupProfileLockRecoveryJournalCanArm;
            ButtonGroupEnable.IsEnabled = groupProfileLockResumeAvailable
                || groupProfileLockSendAvailable;
            ButtonGroupEnable.Content = groupProfileLockRecoveryRequired
                || (HasActiveGroupProfileLockRecoveryJournalRecord
                    && !groupProfileLockVerificationPending)
                ? "Lock State Uncertain - Safe Recovery Required"
                : (groupProfileLockVerificationPending
                    ? "Resume Lock Verification (No 0x2047 Replay)"
                    : "4 Enable (Lock Profile)");
            var groupProfileUnlockStatusOnlyAvailable =
                groupProfileRecoveryRecord != null
                && !groupProfileRecoveryRecord.ExpectedProfileLocked
                && groupProfileRecoveryRecord.State
                    == GroupProfileLockRecoveryState.AcceptedAwaitingProof
                && ((pendingGroupDisableWaitContinuation != null
                        && pendingGroupDisableWaitContinuation.IsPending)
                    || HasAcceptedGroupProfileUnlockRecoveryRecord);
            var groupProfileUnlockExplicitRetryAvailable =
                groupProfileRecoveryRecord != null
                && !groupProfileRecoveryRecord.ExpectedProfileLocked
                && groupProfileRecoveryRecord.State
                    == GroupProfileLockRecoveryState.RecoveryRequired
                && !GroupProfileLockRecoveryJournalUnavailable;
            var groupProfileUnlockTakeoverAvailable =
                groupProfileRecoveryRecord != null
                && groupProfileRecoveryRecord.ExpectedProfileLocked
                && !GroupProfileLockRecoveryJournalUnavailable;
            var groupProfileUnlockFreshSendAvailable =
                groupProfileRecoveryRecord == null
                && GroupProfileLockRecoveryJournalCanArm
                && (groupProfileLocked
                    || groupResetTransitionPending
                    || groupResetObservedLockedStandby);
            var groupProfileUnlockCommandAvailable = groupReady
                && idle
                && !motionMayBeActive
                && !diagnosticMutationCommandInterlocked
                && !groupPowerTransitionPending
                && !groupPowerOffVerificationPending
                && (groupProfileUnlockStatusOnlyAvailable
                    || groupProfileUnlockExplicitRetryAvailable
                    || groupProfileUnlockTakeoverAvailable
                    || groupProfileUnlockFreshSendAvailable);
            ButtonGroupDisable.IsEnabled =
                groupProfileUnlockCommandAvailable;
            ButtonGroupDisable.Content =
                groupProfileUnlockStatusOnlyAvailable
                ? "Resume Unlock Verification (No 0x2048 Replay)"
                : (groupProfileUnlockExplicitRetryAvailable
                    ? "Retry Disable Explicitly (0x2048)"
                    : (groupProfileUnlockTakeoverAvailable
                        ? "Disable (Lock-to-Unlock Takeover)"
                        : (groupProfileRecoveryRecord != null
                                && !groupProfileRecoveryRecord
                                    .ExpectedProfileLocked
                            ? "Disable Replay Blocked"
                            : (groupResetTransitionPending
                                ? "Disable (Reset Safety Recovery)"
                                : (groupResetObservedLockedStandby
                                    ? "Disable (Observed Reset LockedStandby)"
                                    : "Disable (Unlock Profile)")))));
            var groupResetResumeAvailable = groupReady
                && liveCommandAllowedIgnoringGroupReset
                && !groupPowerTransitionPending
                && pendingGroupResetWaitContinuation != null
                && pendingGroupResetWaitContinuation.IsPending
                && !groupResetSupersededByLaterMutation;
            var groupResetFreshAvailable = groupReady
                && liveCommandAllowed
                && !groupPowerTransitionPending
                && !HasUnresolvedGroupProfileLockState()
                && GroupResetRecoveryJournalCanArm;
            ButtonGroupReset.IsEnabled = groupResetResumeAvailable
                || groupResetFreshAvailable;
            ButtonGroupReset.Content = groupResetResumeAvailable
                ? "Resume Reset Verification (No 0x2049 Replay)"
                : (groupResetSubmissionUncertain
                    ? "Reset Replay Blocked - Safety Recovery Required"
                    : "Group Reset");
            ButtonGroupStop.IsEnabled = groupReady
                && safetySendAvailable
                && (!motionMayBeActive
                    || IsTrackedMotionTarget(group));
            ButtonGroupMoveLinear.IsEnabled = groupReady
                && liveCommandAllowed
                && MotionUncertaintyJournalCanArm
                && !groupPowerOffVerificationPending
                && groupActiveVerified
                && groupIdentityConfigured
                && !HasUnresolvedGroupProfileLockState()
                && groupProfileLocked
                && groupMotionCoordinateReady;
            ButtonGroupMoveLinearRelative.IsEnabled = groupReady
                && liveCommandAllowed
                && MotionUncertaintyJournalCanArm
                && !groupPowerOffVerificationPending
                && groupActiveVerified
                && groupIdentityConfigured
                && !HasUnresolvedGroupProfileLockState()
                && groupProfileLocked
                && groupMotionCoordinateReady;
            ButtonCheckKinHome.IsEnabled = groupReady
                && idle
                && !groupPowerOffVerificationPending;
            ButtonSetKinTransform.IsEnabled = groupReady
                && liveCommandAllowed
                && !groupPowerOffVerificationPending
                && groupActiveVerified
                && !groupProfileLockVerificationPending
                && !groupProfileLockRecoveryRequired
                && !HasActiveGroupProfileLockRecoveryJournalRecord
                && !groupProfileLocked;

            ComboGroupUnit.IsEnabled = localEngineeringDraftAllowed;
            TextGroupPositionX.IsEnabled = localEngineeringDraftAllowed;
            TextGroupPositionY.IsEnabled = localEngineeringDraftAllowed;
            TextGroupPositionZ.IsEnabled = localEngineeringDraftAllowed;
            TextGroupPositionU.IsEnabled = localEngineeringDraftAllowed;
            TextGroupVelocity.IsEnabled = localEngineeringDraftAllowed;
            TextGroupAcceleration.IsEnabled = localEngineeringDraftAllowed;
            TextGroupDeceleration.IsEnabled = !operationRunning
                && !safetyCommandRunning;
            TextGroupJerk.IsEnabled = !operationRunning
                && !safetyCommandRunning;
            ComboGroupCoordinate.IsEnabled = localEngineeringDraftAllowed;
            ComboGroupTransition.IsEnabled = localEngineeringDraftAllowed;
            ComboGroupBuffer.IsEnabled = localEngineeringDraftAllowed;
            var identityInputAllowed = recoveryIdentityReadOnly
                ? idle
                : groupReady
                    && liveCommandAllowed
                    && !groupPowerOffVerificationPending
                    && groupActiveVerified
                    && !groupProfileLockVerificationPending
                    && !groupProfileLocked;
            TextKinAxisX.IsEnabled = identityInputAllowed;
            TextKinAxisY.IsEnabled = identityInputAllowed;
            TextKinAxisZ.IsEnabled = identityInputAllowed;
            TextKinAxisU.IsEnabled = identityInputAllowed;

            TextGroupPreparationState.Text = GetGroupPreparationStateText(
                groupReady);

            TextConnectionState.Text = currentConnection == null
                ? LMCConnectionState.Disconnected.ToString()
                : currentConnection.State.ToString();
            UpdateCallbackListenerSummaryUiState(currentConnection);
            UpdateCallbackDiagnosticsUiState(currentConnection);

            UpdateDiagnosticsUiState(currentConnection, connected, idle);
            UpdateReadOnlyApiUiState(connected, idle);
            UpdateQualificationUiState(connected, idle);
            UpdateMaintenanceActionUiState(
                connected,
                idle,
                axisReady,
                liveCommandAllowed);

            var trackedGroup = motionMayBeActive
                && motionTargetKind == MotionUncertaintyTargetKind.Group;
            TextMotionWarning.Text = recoveryIdentityReadOnly
                ? "SAFETY: RECOVERY IDENTITY READ-ONLY QUARANTINE. "
                    + GetRecoveryIdentityReadOnlyGuidance()
                : MaintenanceActionRecoveryJournalUnavailable
                    || HasUnresolvedMaintenanceAction
                ? GetMaintenanceActionGlobalWarning()
                : motionMayBeActive
                ? "SAFETY: "
                    + motionOperation
                    + " may still be active on "
                    + motionAxisName
                    + (trackedGroup
                        ? ". Use Group Stop and verify InPosition."
                        : ". Use Stop or PowerOff and verify standstill.")
                : HasUnresolvedAxisPowerState()
                    ? "SAFETY: " + GetAxisPowerOnRecoveryGuidance()
                : AxisPowerOnRecoveryJournalUnavailable
                    ? "SAFETY: " + GetAxisPowerOnRecoveryGuidance()
                : HasUnresolvedAxisQualificationState()
                    ? "SAFETY: " + GetAxisQualificationRecoveryGuidance()
                : AxisQualificationRecoveryJournalUnavailable
                    ? "SAFETY: " + GetAxisQualificationRecoveryGuidance()
                : HasUnresolvedGroupResetState()
                    ? "SAFETY: Group Reset is unresolved. "
                        + GetGroupResetRecoveryGuidance()
                : HasUnresolvedGroupPowerState()
                    ? "SAFETY: " + GetGroupPowerRecoveryGuidance()
                : GroupPowerRecoveryJournalUnavailable
                    ? "SAFETY: " + GetGroupPowerRecoveryGuidance()
                : GroupResetRecoveryJournalUnavailable
                    ? "SAFETY: " + GetGroupResetRecoveryGuidance()
                : MotionUncertaintyJournalUnavailable
                    ? "SAFETY: " + GetMotionUncertaintyJournalGuidance()
                    : diagnosticMutationUnresolved
                    ? "SAFETY: diagnostics mutation or durable recovery evidence is unresolved. New motion/diagnostic mutation and Close are blocked. "
                        + GetUnresolvedDiagnosticMutationGuidance()
                    : AnyDiagnosticsMutationJournalUnavailable
                        ? "SAFETY: "
                            + GetAnyDiagnosticsMutationJournalUnavailableGuidance()
                        : "Stop, PowerOff, and Group Stop remain available while connected. Closing the connection does not stop motion.";
            RefreshRecoveryIdentityRetirementUi();
            if (currentUiLanguage == UiLanguage.Korean)
            {
                ApplyUiLanguage();
            }
        }

        private void UpdateCallbackDiagnosticsUiState(
            LMCConnection currentConnection)
        {
            var statistics = lastCallbackV2Statistics;
            UpdateCallbackListenerSummaryUiState(
                currentConnection,
                statistics == null
                    ? (long?)null
                    : statistics.RejectedCount);
            TextRpcInitialization.Text = lastRpcInitializationEvidence
                + (string.Equals(
                        lastRpcInitializationEvidence,
                        "No RPC initialization attempt",
                        StringComparison.Ordinal)
                    ? string.Empty
                    : ", Current="
                        + (lastRpcInitializationRetired
                            ? "Retired"
                            : "Active"));

            if (currentConnection == null)
            {
                TextCallbackRegistration.Text = "Not registered";
                TextCallbackCounters.Text =
                    "Accepted=0, Rejected=0, Duplicate=0, OutOfOrder=0";
                TextCallbackLastDecision.Text = "Last decision=None";
                return;
            }

            var registration =
                currentConnection.RpcCallbackRegistrationV2Response;
            var fence = registration == null
                ? null
                : registration.SessionFence;
            TextCallbackRegistration.Text = registration == null
                ? "Not registered"
                : "Status="
                    + registration.Status.ToString(CultureInfo.InvariantCulture)
                    + ", ErrorId="
                    + registration.ErrorId.ToString(CultureInfo.InvariantCulture)
                    + ", Version="
                    + registration.AcceptedVersion.ToString(
                        CultureInfo.InvariantCulture)
                    + ", MaxDatagram="
                    + registration.AcceptedMaxDatagram.ToString(
                        CultureInfo.InvariantCulture)
                    + ", BootId=0x"
                    + registration.DiagnosticsBootId.ToString("X8")
                    + ", SessionEpoch="
                    + registration.SessionEpoch.ToString(
                        CultureInfo.InvariantCulture)
                    + ", Flags=0x"
                    + registration.AcceptedFlags.ToString("X8")
                    + (fence == null
                        ? ", Fence=missing"
                        : ", Cookie=0x"
                            + fence.Cookie.ToString("X16")
                            + ", ListenerGeneration="
                            + fence.ListenerGeneration.ToString(
                                CultureInfo.InvariantCulture)
                            + ", Source="
                            + new System.Net.IPAddress(
                                fence.ExpectedSourceIPv4).ToString()
                            + ", EventMask=0x"
                            + fence.RegisteredEventMask.ToString("X8")
                            + ", LocalSessionGeneration="
                            + currentConnection.CurrentSessionGeneration.ToString(
                                CultureInfo.InvariantCulture));

            var acceptedCount = statistics == null
                ? currentConnection.AcceptedCallbackWakeHintCount
                : statistics.AcceptedWakeHintCount;
            var rejectedCount = statistics == null
                ? currentConnection.RejectedCallbackCount
                : statistics.RejectedCount;
            var duplicateCount = statistics == null
                ? currentConnection.DuplicateCallbackWakeHintCount
                : statistics.DuplicateWakeHintCount;
            var outOfOrderCount = statistics == null
                ? currentConnection.OutOfOrderCallbackWakeHintCount
                : statistics.OutOfOrderWakeHintCount;
            TextCallbackCounters.Text = "Accepted="
                + acceptedCount.ToString(
                    CultureInfo.InvariantCulture)
                + ", Rejected="
                + rejectedCount.ToString(
                    CultureInfo.InvariantCulture)
                + ", Duplicate="
                + duplicateCount.ToString(
                    CultureInfo.InvariantCulture)
                + ", OutOfOrder="
                + outOfOrderCount.ToString(
                    CultureInfo.InvariantCulture);

            TextCallbackLastDecision.Text = statistics == null
                ? "Last decision=None"
                : "Last decision="
                    + statistics.DecisionKind
                    + ", ProtocolError="
                    + statistics.ProtocolError;
            if (!string.IsNullOrEmpty(lastCallbackListenerError))
            {
                TextCallbackLastDecision.Text += ", ListenerError="
                    + lastCallbackListenerError;
            }
        }

        private void UpdateCallbackListenerSummaryUiState(
            LMCConnection currentConnection,
            long? rejectedCount = null)
        {
            var currentRejectedCount = rejectedCount
                ?? (currentConnection == null
                    ? 0
                    : currentConnection.RejectedCallbackCount);
            TextCallbackState.Text = currentConnection == null
                ? "Stopped"
                : (currentConnection.IsCallbackListenerRunning
                    ? "Listening "
                        + currentConnection.CallbackLocalEndPoint
                        + ", rejected="
                        + currentRejectedCount
                    : "Stopped, rejected="
                        + currentRejectedCount);
        }

        private static string FormatRpcInitializationEvidence(
            int connectionAttempt,
            int candidateOrdinal,
            string outcome,
            string remoteIp,
            int remotePort,
            string localIp,
            int callbackPort,
            LMCConnection observedConnection,
            Exception failure)
        {
            var evidence = "Attempt="
                + connectionAttempt.ToString(CultureInfo.InvariantCulture)
                + ", Outcome="
                + outcome
                + ", CandidateOrdinal="
                + candidateOrdinal.ToString(CultureInfo.InvariantCulture)
                + ", Remote="
                + remoteIp
                + ":"
                + remotePort.ToString(CultureInfo.InvariantCulture)
                + ", Local="
                + localIp
                + ", RequestedCallback="
                + localIp
                + ":"
                + callbackPort.ToString(CultureInfo.InvariantCulture)
                + ", Mode=Version2WakeHint";

            if (observedConnection != null)
            {
                var boundCallback = observedConnection.CallbackLocalEndPoint;
                evidence += ", BoundCallback="
                    + (boundCallback == null
                        ? "not-bound"
                        : boundCallback.ToString());
                var initialization = observedConnection
                    .LastRpcSessionInitializationEvidence;
                if (initialization == null)
                {
                    evidence += ", LocalSessionGeneration="
                        + observedConnection.CurrentSessionGeneration.ToString(
                            CultureInfo.InvariantCulture)
                        + ", RPCInit=pending";
                }
                else
                {
                    evidence += ", LocalSessionGeneration="
                        + initialization.SessionGeneration.ToString(
                            CultureInfo.InvariantCulture)
                        + ", 0x8080Attempts="
                        + initialization.AttemptCount.ToString(
                            CultureInfo.InvariantCulture)
                        + ", Retry="
                        + initialization.CanonicalRetryUsed
                        + ", InitOutcome="
                        + initialization.Outcome
                        + ", StartedUtc="
                        + initialization.StartedAtUtc.ToString(
                            "O",
                            CultureInfo.InvariantCulture)
                        + ", CompletedUtc="
                        + initialization.CompletedAtUtc.ToString(
                            "O",
                            CultureInfo.InvariantCulture)
                        + ", LastACK={"
                        + FormatRpcSessionInitResponse(
                            initialization.LastReceivedResponse)
                        + "}";

                    if (initialization.FirstFailureResponse != null)
                    {
                        evidence += ", FirstFailure={"
                            + FormatRpcSessionInitResponse(
                                initialization.FirstFailureResponse)
                            + "}";
                    }

                    if (!string.IsNullOrEmpty(initialization.FailureType))
                    {
                        evidence += ", InitFailure="
                            + initialization.FailureType
                            + ": "
                            + initialization.FailureMessage;
                    }
                }
            }

            if (failure != null)
            {
                evidence += ", Failure="
                    + failure.GetType().Name
                    + ": "
                    + failure.Message;
            }

            return evidence;
        }

        private static bool IsExactPersistentSessionInitMinusOneFailure(
            LMCConnection observedConnection)
        {
            if (observedConnection == null
                || observedConnection.IsRpcInitialized
                || observedConnection.IsCallbackListenerRunning
                || observedConnection.CallbackLocalEndPoint != null
                || observedConnection.RpcCallbackRegistrationResponse != null
                || observedConnection.RpcCallbackRegistrationV2Response != null)
            {
                return false;
            }

            var initialization = observedConnection
                .LastRpcSessionInitializationEvidence;
            return initialization != null
                && initialization.Outcome
                    == LMCRpcSessionInitializationOutcome.Failed
                && initialization.AttemptCount == 2
                && initialization.CanonicalRetryUsed
                && IsExactSessionInitMinusOneFailure(
                    initialization.FirstFailureResponse)
                && IsExactSessionInitMinusOneFailure(
                    initialization.LastReceivedResponse);
        }

        private static bool IsEligiblePreResponseTransportFailure(
            LMCConnection observedConnection,
            Exception failure)
        {
            if (observedConnection == null
                || failure == null
                || failure is OperationCanceledException
                || failure is ObjectDisposedException
                || observedConnection.IsRpcInitialized
                || observedConnection.IsCallbackListenerRunning
                || observedConnection.CallbackLocalEndPoint != null
                || observedConnection.RpcSessionInitResponse != null
                || observedConnection.RpcCallbackRegistrationResponse != null
                || observedConnection.RpcCallbackRegistrationV2Response != null)
            {
                return false;
            }

            var initialization = observedConnection
                .LastRpcSessionInitializationEvidence;
            return initialization != null
                && initialization.Outcome
                    == LMCRpcSessionInitializationOutcome.Failed
                && initialization.AttemptCount == 1
                && !initialization.CanonicalRetryUsed
                && initialization.FirstFailureResponse == null
                && initialization.LastReceivedResponse == null
                && IsEligiblePreResponseTransportException(failure);
        }

        private Task DelayBeforeFreshSessionRetryAsync(
            int delayMilliseconds)
        {
            var delayOverride = FreshSessionRetryDelayAsyncOverride;
            return delayOverride == null
                ? Task.Delay(delayMilliseconds)
                : delayOverride(delayMilliseconds);
        }

        private static bool IsEligiblePreResponseTransportException(
            Exception failure)
        {
            if (failure is InvalidDataException)
            {
                return false;
            }

            if (failure is EndOfStreamException
                || failure is SocketException
                || failure is TimeoutException)
            {
                return true;
            }

            var ioFailure = failure as IOException;
            return ioFailure != null
                && ioFailure.InnerException != null
                && IsEligiblePreResponseTransportException(
                    ioFailure.InnerException);
        }

        private static bool HasCompleteLocalConnectionCleanup(
            LMCConnection observedConnection)
        {
            return observedConnection != null
                && observedConnection.State
                    == LMCConnectionState.Disconnected
                && !observedConnection.IsConnected
                && !observedConnection.IsRpcInitialized
                && !observedConnection.IsCallbackListenerRunning
                && observedConnection.CallbackLocalEndPoint == null;
        }

        private async Task EnsureCompleteLocalConnectionCleanupAsync(
            LMCConnection observedConnection,
            string operation)
        {
            Exception lastUnexpectedError = null;
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    await Task.Run(() => observedConnection.Dispose());
                }
                catch (Exception error)
                {
                    lastUnexpectedError = error;
                    WriteLog(
                        operation
                        + " Dispose attempt "
                        + attempt.ToString(CultureInfo.InvariantCulture)
                        + " warning: "
                        + error.Message);
                }

                if (HasCompleteLocalConnectionCleanup(observedConnection))
                {
                    return;
                }
            }

            throw new InvalidOperationException(
                operation
                + " did not reach the complete disconnected postcondition "
                + "after two bounded Dispose attempts.",
                lastUnexpectedError);
        }

        private static bool IsExactSessionInitMinusOneFailure(
            LMC_Response response)
        {
            return response != null
                && response.IsFrameValid
                && response.HeaderStatus == 1
                && response.HeaderReserved == 0
                && response.PayloadLength == 4
                && response.HasCommandResult
                && response.CommandStatus == 1
                && response.ErrorId == -1;
        }

        private static string AppendFreshSessionRetryEvidence(
            string evidence,
            bool freshSessionRetryUsed,
            string retryReason,
            int retryDelayMilliseconds,
            int currentCandidateOrdinal,
            string firstFailureEvidence)
        {
            if (!freshSessionRetryUsed)
            {
                return evidence;
            }

            return evidence
                + ", FreshSessionRetry=Used"
                + ", FreshSessionRetryReason="
                + retryReason
                + ", FreshSessionRetryDelayMs="
                + retryDelayMilliseconds.ToString(
                    CultureInfo.InvariantCulture)
                + ", FreshSessionRetryFromCandidate="
                + (currentCandidateOrdinal - 1).ToString(
                    CultureInfo.InvariantCulture)
                + ", FreshSessionRetryNextCandidate="
                + currentCandidateOrdinal.ToString(
                    CultureInfo.InvariantCulture)
                + ", FreshSessionFirstFailure={"
                + firstFailureEvidence
                + "}";
        }

        private static string AppendFreshSessionRetryScheduledEvidence(
            string evidence,
            string retryReason,
            int retryDelayMilliseconds,
            int candidateOrdinal,
            string firstFailureEvidence)
        {
            return evidence
                + ", FreshSessionRetry=Scheduled"
                + ", FreshSessionRetryReason="
                + retryReason
                + ", FreshSessionRetryDelayMs="
                + retryDelayMilliseconds.ToString(
                    CultureInfo.InvariantCulture)
                + ", FreshSessionRetryFromCandidate="
                + candidateOrdinal.ToString(CultureInfo.InvariantCulture)
                + ", FreshSessionRetryNextCandidate="
                + (candidateOrdinal + 1).ToString(
                    CultureInfo.InvariantCulture)
                + ", FreshSessionFirstFailure={"
                + firstFailureEvidence
                + "}";
        }

        private static string FormatRpcSessionInitResponse(
            LMC_Response response)
        {
            if (response == null)
            {
                return "none";
            }

            return "FrameValid="
                + response.IsFrameValid
                + ", HeaderStatus="
                + response.HeaderStatus.ToString(CultureInfo.InvariantCulture)
                + ", HeaderReserved="
                + response.HeaderReserved.ToString(CultureInfo.InvariantCulture)
                + ", PayloadLength="
                + response.PayloadLength.ToString(CultureInfo.InvariantCulture)
                + ", HasCommandResult="
                + response.HasCommandResult
                + ", CommandStatus="
                + response.CommandStatus.ToString(CultureInfo.InvariantCulture)
                + ", ErrorId="
                + response.ErrorId.ToString(CultureInfo.InvariantCulture);
        }

        private string GetGroupPreparationStateText(bool groupReady)
        {
            if (!groupReady)
            {
                return "Preparation: load the group first.";
            }

            if (HasUnresolvedGroupResetState())
            {
                var continuation = pendingGroupResetWaitContinuation;
                if (groupResetSubmissionUncertain)
                {
                    return "Preparation: Group Reset may have been sent, but "
                        + "there is no accepted status-only continuation. "
                        + "Fresh 0x2049, reconnect, mutation, and Close are "
                        + "blocked. Use Group Stop, Power Off, safe Disable, "
                        + "or disconnect. Readiness is invalid.";
                }

                if (IsAttachedOutcomeUncertainGroupResetRecovery)
                {
                    return "Preparation: outcome-uncertain Group Reset recovery "
                        + "is attached; current group/member stable error-clearance "
                        + "proof is pending. The prior 0x2049 outcome remains "
                        + "unknown and will not be replayed. Power, identity/Home, "
                        + "and profile-lock readiness are invalid.";
                }

                return "Preparation: Group Reset ACK accepted; stable group/member "
                    + "error-clearance proof is pending"
                    + (continuation == null
                        ? string.Empty
                        : " (rounds="
                            + continuation.StatusRoundCount
                            + ", stable="
                            + continuation.StableSampleCount
                            + "/"
                            + continuation.RequiredStableSampleCount
                            + ")")
                    + ". Power, identity/Home, and profile-lock readiness are "
                    + "invalid. Next: Resume Reset Verification (status reads "
                    + "only; no 0x2049 replay), or use Stop, Power Off, or safe "
                    + "Disable.";
            }

            var recoveryRecord = HasActiveGroupPowerRecoveryRecord
                ? groupPowerRecoveryJournal.CurrentRecord
                : null;
            string powerState;
            if (recoveryRecord != null
                && recoveryRecord.ExpectedPowerOn
                && groupPowerRecoveryRequired)
            {
                powerState = "Power On outcome uncertain, replay blocked";
            }
            else if (recoveryRecord != null
                && !recoveryRecord.ExpectedPowerOn
                && groupPowerOffReplacementAllowed)
            {
                powerState =
                    "Power Off proof interfered, explicit replacement allowed";
            }
            else if (recoveryRecord != null
                && !recoveryRecord.ExpectedPowerOn
                && groupPowerRecoveryRequired)
            {
                powerState =
                    "Power Off outcome uncertain, status-only proof required";
            }
            else if (groupStatusRefreshRequired)
            {
                powerState = "Read Status failed, Power/Lock state unknown";
            }
            else if (groupPowerOffVerificationPending)
            {
                powerState =
                    "Power Off accepted/start only, Power Off pending";
            }
            else if (groupActiveVerified)
            {
                powerState = "Power Ready/ACTIVE verified";
            }
            else if (groupPowerVerificationPending)
            {
                powerState =
                    "Power On accepted/start only, Power Ready pending";
            }
            else
            {
                powerState = "Power On required";
            }
            var identityState = groupIdentityConfigured
                ? "identity configured"
                : "identity not configured";
            var homeState = groupIdentityHomeCheckComplete
                ? (groupIdentityHomeCheckPassed
                    ? "identity axes referenced"
                    : "identity axis Home required")
                : "identity Home not checked";
            var profileRecoveryRecord =
                HasActiveGroupProfileLockRecoveryJournalRecord
                    ? groupProfileLockRecoveryJournal.CurrentRecord
                    : null;
            var unlockProofPending =
                groupProfileUnlockVerificationPending
                || (profileRecoveryRecord != null
                    && !profileRecoveryRecord.ExpectedProfileLocked
                    && profileRecoveryRecord.State
                        == GroupProfileLockRecoveryState
                            .AcceptedAwaitingProof);
            var profileState = unlockProofPending
                ? "profile unlock accepted, Disabled proof pending"
                : (groupProfileLocked
                    ? "profile locked/standby verified"
                    : (groupProfileLockRecoveryRequired
                    ? "profile lock result stale, Disable or stable Power Off required"
                    : (groupProfileLockVerificationPending
                        ? "profile lock accepted, Lock Ready pending"
                        : "profile unlocked")));

            string nextStep;
            if (recoveryRecord != null
                && recoveryRecord.ExpectedPowerOn
                && groupPowerRecoveryRequired)
            {
                nextStep =
                    "Next: Send Power Off Safety Takeover; do not replay 0x204A.";
            }
            else if (groupPowerOffReplacementAllowed)
            {
                nextStep =
                    "Next: Power Off Again is allowed after confirmed interference.";
            }
            else if (groupPowerOffVerificationPending)
            {
                nextStep =
                    "Next: Resume Power Off Verification (status reads only; no 0x204B replay).";
            }
            else if (groupStatusRefreshRequired)
            {
                nextStep = "Next: Read Status to refresh the group state.";
            }
            else if (unlockProofPending)
            {
                nextStep =
                    "Next: Resume Unlock Verification (status reads only; no 0x2048 replay).";
            }
            else if (!groupActiveVerified)
            {
                nextStep = groupPowerVerificationPending
                    ? "Next: Resume Power On Verification (status reads only; no 0x204A replay)."
                    : "Next: Power On.";
            }
            else if (!groupIdentityConfigured)
            {
                nextStep = groupIdentityHomeCheckComplete
                    && !groupIdentityHomeCheckPassed
                    ? "Next: Home the failed axes, then Set Identity."
                    : "Next: Set Identity (automatic Home Check).";
            }
            else if (groupProfileLockRecoveryRequired)
            {
                nextStep =
                    "Next: Disable or complete stable Power Off verification; "
                    + "do not replay Enable.";
            }
            else if (groupProfileLockVerificationPending)
            {
                nextStep =
                    "Next: Resume Lock Verification (status reads only; no Enable replay).";
            }
            else if (!groupProfileLocked)
            {
                nextStep = "Next: Enable (Lock Profile).";
            }
            else
            {
                nextStep = "Ready: Move Linear or Disable (Unlock Profile).";
            }

            return "Preparation: "
                + powerState
                + " | "
                + homeState
                + " | "
                + identityState
                + " | "
                + profileState
                + ". "
                + nextStep;
        }

        private void WriteLog(string message)
        {
            if (TextExecutionLog == null)
            {
                return;
            }

            TextExecutionLog.AppendText(
                "["
                + DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)
                + "] "
                + message
                + Environment.NewLine);
            TextExecutionLog.ScrollToEnd();
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (allowWindowClose)
            {
                base.OnClosing(e);
                return;
            }

            e.Cancel = true;
            base.OnClosing(e);

            if (shutdownInProgress)
            {
                return;
            }

            var recoveryIdentityReadOnlyExit =
                IsRecoveryIdentityReadOnlyExitPermitted();
            if (!recoveryIdentityReadOnlyExit
                && HasUnresolvedAxisPowerState())
            {
                WriteLog(
                    "Window close is blocked while Axis Power recovery is unresolved. "
                    + GetAxisPowerOnRecoveryGuidance());
                return;
            }

            if (!recoveryIdentityReadOnlyExit
                && HasUnresolvedAxisCommandState())
            {
                WriteLog(
                    "Window close is blocked while Axis Stop/Reset recovery is unresolved. Complete exact status-only proof or the explicit recovery action first.");
                return;
            }

            if (!recoveryIdentityReadOnlyExit
                && HasUnresolvedAxisQualificationState())
            {
                WriteLog(
                    "Window close is blocked while Single Axis qualification recovery is unresolved. "
                    + GetAxisQualificationRecoveryGuidance());
                return;
            }

            if (!recoveryIdentityReadOnlyExit
                && HasUnresolvedGroupResetState())
            {
                WriteLog(
                    "Window close is blocked while an accepted Group Reset is "
                    + "awaiting stable group/member error-clearance proof. "
                    + GetGroupResetRecoveryGuidance());
                return;
            }

            if (!recoveryIdentityReadOnlyExit
                && HasUnresolvedGroupProfileLockState())
            {
                WriteLog(
                    "Window close is blocked while Group Enable is pending or the "
                    + "profile-lock result is uncertain. Resume verification, run "
                    + "Disable, or complete stable Power Off verification first.");
                return;
            }

            if (!recoveryIdentityReadOnlyExit
                && HasUnresolvedGroupPowerState())
            {
                WriteLog(
                    "Window close is blocked while Group Power recovery is "
                    + "unresolved. "
                    + GetGroupPowerRecoveryGuidance());
                return;
            }

            if (operationRunning
                || safetyCommandRunning
                || safetyMonitorCount > 0
                || qualificationRunning)
            {
                if (qualificationRunning)
                {
                    CancelQualification(
                        "Window close requested",
                        false);
                }

                WriteLog(
                    "Window close is blocked while an API operation, safety "
                    + "verification, or qualification cleanup is running. "
                    + "Wait for its timeout or completion, then close again.");
                return;
            }

            if (!recoveryIdentityReadOnlyExit && motionMayBeActive)
            {
                WriteLog(
                    "Window close is blocked while "
                    + motionOperation
                    + " may still be active on "
                    + motionAxisName
                    + ". No Stop command is sent automatically. Reconnect to the "
                    + "exact durable recovery identity, then use Stop or PowerOff "
                    + "and verify the stable safe state first.");
                return;
            }

            var closeAdmission = EvaluateDiagnosticsAdmission(
                DiagnosticsAdmissionOperation.CloseWindow);
            if (!closeAdmission.IsAllowed)
            {
                WriteLog(
                    CreateDiagnosticsAdmissionException(
                        "Window close",
                        closeAdmission).Message);
                return;
            }

            shutdownInProgress = true;
            var currentConnection = connection;

            if (currentConnection != null)
            {
                try
                {
                    await EnsureCompleteLocalConnectionCleanupAsync(
                        currentConnection,
                        "Window shutdown cleanup");
                }
                catch (Exception cleanupError)
                {
                    shutdownInProgress = false;
                    UpdateUiState();
                    WriteLog(
                        "Window close cancelled: "
                        + cleanupError.Message);
                    return;
                }

                if (currentConnection.LastCloseException != null)
                {
                    WriteLog(
                        "Shutdown RPC close warning retained after local cleanup. "
                        + "Response={"
                        + FormatRpcSessionInitResponse(
                            currentConnection.RpcCloseResponse)
                        + "}, Failure="
                        + currentConnection.LastCloseException
                            .GetType().Name
                        + ": "
                        + currentConnection.LastCloseException.Message);
                }

                if (ReferenceEquals(connection, currentConnection))
                {
                    connection = null;
                }

                DetachConnection(currentConnection);
            }

            ClearLoadedObjects();
            shutdownInProgress = false;
            UpdateUiState();
            shutdownInProgress = true;
            allowWindowClose = true;
            _ = Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(Close));
        }

        private sealed class IdentityAxisHomeStatus
        {
            public IdentityAxisHomeStatus(
                string coordinateName,
                LMCSingleAxis selectedAxis,
                LMCReadStatusResult status)
            {
                CoordinateName = coordinateName;
                Axis = selectedAxis;
                Status = status;
            }

            public string CoordinateName { get; }
            public LMCSingleAxis Axis { get; }
            public LMCReadStatusResult Status { get; }
        }

        private sealed class IdentityHomeCheckResult
        {
            public IdentityHomeCheckResult(
                IdentityAxisHomeStatus axisX,
                IdentityAxisHomeStatus axisY,
                IdentityAxisHomeStatus axisZ,
                IdentityAxisHomeStatus axisU)
            {
                AxisX = axisX;
                AxisY = axisY;
                AxisZ = axisZ;
                AxisU = axisU;
                Axes = new[] { AxisX, AxisY, AxisZ, AxisU };
            }

            public IdentityAxisHomeStatus AxisX { get; }
            public IdentityAxisHomeStatus AxisY { get; }
            public IdentityAxisHomeStatus AxisZ { get; }
            public IdentityAxisHomeStatus AxisU { get; }
            public IdentityAxisHomeStatus[] Axes { get; }

            public int ReferencedCount
            {
                get { return Axes.Count(item => item.Status.IsReferenced); }
            }

            public bool AllReferenced
            {
                get { return ReferencedCount == Axes.Length; }
            }

            public string UnreferencedAxisSummary
            {
                get
                {
                    return string.Join(
                        ", ",
                        Axes
                            .Where(item => !item.Status.IsReferenced)
                            .Select(
                                item =>
                                    item.CoordinateName
                                    + "="
                                    + item.Axis.AxisName));
                }
            }
        }

        private sealed class MotionInput
        {
            public int PositionRaw { get; set; }
            public int VelocityRaw { get; set; }
            public int AccelerationRaw { get; set; }
            public int DecelerationRaw { get; set; }
            public int JerkRaw { get; set; }
            public LMC_DIRECTION Direction { get; set; }
        }

        private sealed class GroupMotionInput
        {
            public int[] PositionsRaw { get; set; }
            public int VelocityRaw { get; set; }
            public int AccelerationRaw { get; set; }
            public int DecelerationRaw { get; set; }
            public int JerkRaw { get; set; }
            public LMCGroupMotionOptions Options { get; set; }
        }

        private sealed class PlcUnitOption
        {
            public PlcUnitOption(
                string displayName,
                string symbol,
                int multiplier,
                bool isRaw)
            {
                DisplayName = displayName;
                Symbol = symbol;
                Multiplier = multiplier;
                IsRaw = isRaw;
            }

            public string DisplayName { get; }
            public string Symbol { get; }
            public int Multiplier { get; }
            public bool IsRaw { get; }

            public override string ToString()
            {
                return DisplayName;
            }
        }
    }
}

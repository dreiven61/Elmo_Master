using System;
using System.Windows;
using System.Windows.Threading;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private static readonly TimeSpan HomeRepeatRecoveryPollInterval =
            TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan HomeRepeatRecoveryRetryInterval =
            TimeSpan.FromSeconds(1);

        private DispatcherTimer homeRepeatRecoveryTimer;
        private bool homeRepeatRecoveryPollRunning;
        private DateTime homeRepeatRecoveryNextPollUtc;

        static MainWindow()
        {
            EventManager.RegisterClassHandler(
                typeof(MainWindow),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(HomeRepeatWindow_Loaded),
                true);
            EventManager.RegisterClassHandler(
                typeof(MainWindow),
                FrameworkElement.UnloadedEvent,
                new RoutedEventHandler(HomeRepeatWindow_Unloaded),
                true);
        }

        private static void HomeRepeatWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            var window = sender as MainWindow;
            if (window == null || !ReferenceEquals(e.OriginalSource, window))
            {
                return;
            }

            window.EnsureHomeRepeatRecoveryMonitorStarted();
        }

        private static void HomeRepeatWindow_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            var window = sender as MainWindow;
            if (window == null || !ReferenceEquals(e.OriginalSource, window))
            {
                return;
            }

            window.StopHomeRepeatRecoveryMonitor();
        }

        private void EnsureHomeRepeatRecoveryMonitorStarted()
        {
            if (homeRepeatRecoveryTimer != null)
            {
                return;
            }

            homeRepeatRecoveryTimer = new DispatcherTimer
            {
                Interval = HomeRepeatRecoveryPollInterval
            };
            homeRepeatRecoveryTimer.Tick += HomeRepeatRecoveryTimer_Tick;
            homeRepeatRecoveryTimer.Start();
            WriteLog(
                "HOME_REPEAT MONITOR_READY: automatic exact Home outcome/retire recovery is active.");
        }

        private void StopHomeRepeatRecoveryMonitor()
        {
            var timer = homeRepeatRecoveryTimer;
            homeRepeatRecoveryTimer = null;
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= HomeRepeatRecoveryTimer_Tick;
            }

            homeRepeatRecoveryPollRunning = false;
        }

        private async void HomeRepeatRecoveryTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (homeRepeatRecoveryPollRunning
                || operationRunning
                || connectionTransitionRunning
                || safetyCommandRunning
                || safetyMonitorCount != 0
                || qualificationRunning
                || shutdownInProgress
                || DateTime.UtcNow < homeRepeatRecoveryNextPollUtc)
            {
                return;
            }

            if (!IsLoaded
                || connection == null
                || !connection.IsConnected
                || axis == null
                || maintenanceActionRecoveryJournal == null
                || !HasUnresolvedMaintenanceAction)
            {
                return;
            }

            var recovery = maintenanceActionRecoveryJournal.CurrentRecord;
            if (recovery == null
                || (recovery.Action != MaintenanceActionKind.LmcHome
                    && recovery.Action != MaintenanceActionKind.Ds402Home))
            {
                return;
            }

            if (!string.Equals(
                    axis.AxisName,
                    recovery.AxisName,
                    StringComparison.Ordinal)
                || axis.AxisReference != recovery.AxisReference)
            {
                return;
            }

            homeRepeatRecoveryPollRunning = true;
            homeRepeatRecoveryNextPollUtc =
                DateTime.UtcNow.Add(HomeRepeatRecoveryRetryInterval);
            var action = recovery.Action;
            var correlationId = recovery.TransportCorrelationId;
            var lmcHome = action == MaintenanceActionKind.LmcHome;
            var operation = lmcHome
                ? "LMC Home Automatic Outcome"
                : "DS402 Home Automatic Outcome";
            var logPrefix = lmcHome
                ? "LMC_HOME_REPEAT"
                : "DS402_HOME_REPEAT";

            WriteLog(
                logPrefix
                + " OUTCOME_POLL: AxisRef="
                + recovery.AxisReference
                + "; Correlation="
                + correlationId
                + ".");

            try
            {
                await RunOperationAsync(
                    operation,
                    async () =>
                    {
                        if (lmcHome)
                        {
                            await ReadExactLmcHomeOutcomeAsync(axis, recovery);
                        }
                        else
                        {
                            await ReadExactDs402HomeOutcomeAsync(axis, recovery);
                        }
                    });

                if (!HasUnresolvedMaintenanceAction)
                {
                    if (lmcHome)
                    {
                        latestLmcHomeRecoveryKey = null;
                    }
                    else
                    {
                        latestDs402HomeRecoveryKey = null;
                    }

                    WriteLog(
                        logPrefix
                        + " JOURNAL_RESOLVED: AxisRef="
                        + recovery.AxisReference
                        + "; Correlation="
                        + correlationId
                        + "; Retire=Confirmed; ReadyForNextStart=True.");
                    TextHomeResult.Text += Environment.NewLine
                        + TranslateUiText(
                            "Home terminal outcome was retired and the durable recovery record was resolved. Ready for the next Home Start.");
                }
            }
            finally
            {
                homeRepeatRecoveryPollRunning = false;
            }
        }
    }
}

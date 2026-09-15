using System;
using System.Threading.Tasks;
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
        }

        private static void HomeRepeatWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            var window = sender as MainWindow;
            if (window == null)
            {
                return;
            }

            window.EnsureHomeRepeatRecoveryMonitorStarted();
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
            var operation = action == MaintenanceActionKind.LmcHome
                ? "LMC Home Automatic Outcome"
                : "DS402 Home Automatic Outcome";

            WriteLog(
                "HOME_REPEAT OUTCOME_POLL: Action="
                + action
                + "; AxisRef="
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
                        if (action == MaintenanceActionKind.LmcHome)
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
                    if (action == MaintenanceActionKind.LmcHome)
                    {
                        latestLmcHomeRecoveryKey = null;
                    }
                    else
                    {
                        latestDs402HomeRecoveryKey = null;
                    }

                    WriteLog(
                        "HOME_REPEAT READY_FOR_NEXT_START: Action="
                        + action
                        + "; AxisRef="
                        + recovery.AxisReference
                        + "; Correlation="
                        + correlationId
                        + "; Retire=Confirmed; Journal=Resolved.");
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

using System;
using System.Windows;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private static readonly bool axisSetPositionLoadedHandlerRegistered =
            RegisterAxisSetPositionLoadedHandler();
        private bool axisSetPositionClosedHandlerHooked;

        private static bool RegisterAxisSetPositionLoadedHandler()
        {
            EventManager.RegisterClassHandler(
                typeof(MainWindow),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(MainWindowAxisSetPositionLoaded),
                true);
            return true;
        }

        private static void MainWindowAxisSetPositionLoaded(
            object sender,
            RoutedEventArgs e)
        {
            var window = sender as MainWindow;
            if (window != null)
            {
                window.EnsureAxisSetPositionClosedHandler();
                window.EnsureAxisSetPositionRecoveryInitialized();
                window.ApplyAxisSetPositionPassiveControlState();
            }
        }

        private void EnsureAxisSetPositionRecoveryInitialized()
        {
            if (axisSetPositionRecoveryJournal != null)
            {
                return;
            }

            InitializeAxisSetPositionRecoveryUi();
        }

        private void EnsureAxisSetPositionClosedHandler()
        {
            if (axisSetPositionClosedHandlerHooked)
            {
                return;
            }

            Closed += MainWindowAxisSetPositionClosed;
            axisSetPositionClosedHandlerHooked = true;
        }

        private static void MainWindowAxisSetPositionClosed(
            object sender,
            EventArgs e)
        {
            var window = sender as MainWindow;
            if (window == null)
            {
                return;
            }

            window.Closed -= MainWindowAxisSetPositionClosed;
            window.axisSetPositionClosedHandlerHooked = false;
            window.DisposeAxisSetPositionRecoveryJournal();
        }

        internal void InitializeAxisSetPositionRecoveryForTests()
        {
            EnsureAxisSetPositionClosedHandler();
            EnsureAxisSetPositionRecoveryInitialized();
            ApplyAxisSetPositionPassiveControlState();
        }

        private void ApplyAxisSetPositionPassiveControlState()
        {
            if (buttonRecoverAxisSetPosition != null)
            {
                // Recovery is safe to expose while disconnected: clicking it
                // still passes through RequireConnection before any wire send.
                // Keeping the action visible avoids stranding a recovered
                // startup journal behind a stale button state.
                buttonRecoverAxisSetPosition.IsEnabled =
                    HasActiveAxisSetPositionRecoveryRecord
                    && !AxisSetPositionRecoveryJournalUnavailable;
            }

            if (buttonStartAxisSetPosition != null
                && !HasAxisSetPositionCapabilityTriad())
            {
                // Current production dev intentionally advertises bits 3/5/7
                // OFF. Never infer activation merely because the UI exists.
                buttonStartAxisSetPosition.IsEnabled = false;
            }
        }
    }
}

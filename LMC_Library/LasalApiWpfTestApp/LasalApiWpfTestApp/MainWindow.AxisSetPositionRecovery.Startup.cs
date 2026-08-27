using System.Windows;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private static readonly bool axisSetPositionLoadedHandlerRegistered =
            RegisterAxisSetPositionLoadedHandler();

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
                window.InitializeAxisSetPositionRecoveryUi();
                window.ApplyAxisSetPositionPassiveControlState();
            }
        }

        internal void InitializeAxisSetPositionRecoveryForTests()
        {
            InitializeAxisSetPositionRecoveryUi();
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

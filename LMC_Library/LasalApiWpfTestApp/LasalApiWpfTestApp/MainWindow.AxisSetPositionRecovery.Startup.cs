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
            }
        }

        internal void InitializeAxisSetPositionRecoveryForTests()
        {
            InitializeAxisSetPositionRecoveryUi();
        }
    }
}

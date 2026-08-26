using System;
using System.Windows;

namespace LasalMotionControlApiExample
{
    public partial class App : Application
    {
        private static ApplicationInstanceLease applicationInstanceLease;

        protected override void OnStartup(StartupEventArgs e)
        {
            var probeRequested = e.Args != null
                && e.Args.Length != 0
                && string.Equals(
                    e.Args[0],
                    ExecutableRelaunchProbeArgument,
                    StringComparison.Ordinal);
            if (probeRequested)
            {
                try
                {
                    executableRelaunchProbe =
                        ExecutableRelaunchProbeOptions.Parse(e.Args);
                }
                catch
                {
                    Shutdown(64);
                    return;
                }
            }

            var startupLanguage = UiLanguage.English;
            if (executableRelaunchProbe == null)
            {
                try
                {
                    startupLanguage = UiLanguagePreferenceStore.Load(
                        UiLanguagePreferenceStore.GetDefaultFilePath());
                }
                catch
                {
                    // Startup safety dialogs remain available in English even if
                    // the local preference location cannot be resolved.
                }
            }

            try
            {
                if (!ApplicationInstanceLease.TryAcquireDefault(
                        out applicationInstanceLease))
                {
                    if (executableRelaunchProbe != null)
                    {
                        WriteExecutableRelaunchProbeReport(
                            executableRelaunchProbe,
                            "MUTEX_BUSY",
                            null,
                            "The default application-instance mutex is already owned.");
                        Shutdown(2);
                        return;
                    }

                    MessageBox.Show(
                        UiLocalizationCatalog.Translate(
                            "LASAL Motion Control API Example is already running "
                                + "in this Windows session.\n\nClose the existing "
                                + "instance before starting another. This second "
                                + "instance will exit before opening recovery journals "
                                + "or network ports.",
                            startupLanguage),
                        UiLocalizationCatalog.Translate(
                            "LASAL Motion Control API Example - Already Running",
                            startupLanguage),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    Shutdown(2);
                    return;
                }
            }
            catch (Exception error)
            {
                if (executableRelaunchProbe != null)
                {
                    WriteExecutableRelaunchProbeReport(
                        executableRelaunchProbe,
                        "STARTUP_FAILED",
                        null,
                        error.GetType().Name + ": " + error.Message);
                    Shutdown(3);
                    return;
                }

                MessageBox.Show(
                    UiLocalizationCatalog.Translate(
                        "Startup was blocked because the single-instance guard "
                            + "could not be acquired. No recovery journal or network "
                            + "port was opened.\n\n",
                        startupLanguage)
                    + error.GetType().Name
                    + ": "
                    + error.Message,
                    UiLocalizationCatalog.Translate(
                        "LASAL Motion Control API Example - Startup Blocked",
                        startupLanguage),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(3);
                return;
            }

            base.OnStartup(e);
            MainWindow window;
            try
            {
                window = executableRelaunchProbe == null
                    ? new MainWindow()
                    : new MainWindow(executableRelaunchProbe.JournalRootPath);
            }
            catch (Exception error)
            {
                if (executableRelaunchProbe == null)
                {
                    throw;
                }

                WriteExecutableRelaunchProbeReport(
                    executableRelaunchProbe,
                    "STARTUP_FAILED",
                    null,
                    error.GetType().Name + ": " + error.Message);
                Shutdown(3);
                return;
            }

            MainWindow = window;
            if (executableRelaunchProbe != null)
            {
                window.Closed += ExecutableRelaunchProbeWindow_Closed;
            }
            window.Show();

            if (executableRelaunchProbe != null)
            {
                Dispatcher.BeginInvoke(
                    new Action(
                        () => BeginExecutableRelaunchProbeConnect(window)));
            }

            // Intentionally retain the lease until OS process teardown. Releasing
            // it from a startup-failure or WPF OnExit path could admit another
            // process while this process still owns partially opened journals.
        }
    }
}

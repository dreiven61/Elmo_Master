using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class App
    {
        internal const string ExecutableRelaunchProbeArgument =
            "--wpf-executable-relaunch-probe-v1";
        private const string ExecutableRelaunchProbeDirectoryPrefix =
            "Elmo.WpfExecutableRelaunch.";
        private const int ExecutableRelaunchProbeConnectTimeoutMilliseconds =
            15000;
        private ExecutableRelaunchProbeOptions executableRelaunchProbe;
        private bool executableRelaunchProbeReady;
        private bool executableRelaunchProbeFailed;

        private async void BeginExecutableRelaunchProbeConnect(
            MainWindow window)
        {
            try
            {
                window.TextRemoteIp.Text = "127.0.0.1";
                window.TextRemotePort.Text =
                    executableRelaunchProbe.RpcPort.ToString(
                        CultureInfo.InvariantCulture);
                window.TextLocalIp.Text = "127.0.0.1";
                window.TextCallbackPort.Text = "0";

                window.ButtonConnect.RaiseEvent(
                    new RoutedEventArgs(
                        Button.ClickEvent,
                        window.ButtonConnect));

                var deadline = DateTime.UtcNow.AddMilliseconds(
                    ExecutableRelaunchProbeConnectTimeoutMilliseconds);
                var operationStarted = false;
                while (DateTime.UtcNow < deadline)
                {
                    var operationState =
                        window.TextOperationState.Text ?? string.Empty;
                    if (operationState.IndexOf(
                            "Connect running",
                            StringComparison.Ordinal) >= 0)
                    {
                        operationStarted = true;
                    }

                    if (operationStarted
                        && string.Equals(
                            operationState,
                            "Connect completed",
                            StringComparison.Ordinal)
                        && string.Equals(
                            window.TextConnectionState.Text,
                            LMCConnectionState.Connected.ToString(),
                            StringComparison.Ordinal)
                        && window.ButtonCloseConnection.IsEnabled
                        && window.GridEtherCATTopology.Items.Count > 0)
                    {
                        executableRelaunchProbeReady = true;
                        WriteExecutableRelaunchProbeReport(
                            executableRelaunchProbe,
                            "READY",
                            window,
                            null);
                        return;
                    }

                    if (operationStarted
                        && string.Equals(
                            operationState,
                            "Connect failed",
                            StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "The normal MainWindow Connect operation failed. "
                            + window.TextExecutionLog.Text);
                    }

                    await Task.Delay(25);
                }

                throw new TimeoutException(
                    "The normal MainWindow Connect operation did not reach "
                    + "Connected with completed automatic topology loading.");
            }
            catch (Exception error)
            {
                executableRelaunchProbeFailed = true;
                WriteExecutableRelaunchProbeReport(
                    executableRelaunchProbe,
                    "CONNECT_FAILED",
                    window,
                    error.GetType().Name + ": " + error.Message);
                Environment.ExitCode = 70;
                window.Close();
            }
        }

        private void ExecutableRelaunchProbeWindow_Closed(
            object sender,
            EventArgs e)
        {
            var window = sender as MainWindow;
            if (executableRelaunchProbeReady
                && !executableRelaunchProbeFailed)
            {
                try
                {
                    if (window == null)
                    {
                        throw new InvalidOperationException(
                            "The X-close path did not publish its Closed event source.");
                    }

                    WriteExecutableRelaunchProbeReport(
                        executableRelaunchProbe,
                        "PASS",
                        window,
                        null);
                    Environment.ExitCode = 0;
                }
                catch (Exception error)
                {
                    executableRelaunchProbeFailed = true;
                    Environment.ExitCode = 71;
                    try
                    {
                        WriteExecutableRelaunchProbeReport(
                            executableRelaunchProbe,
                            "CLOSE_FAILED",
                            window,
                            error.GetType().Name + ": " + error.Message);
                    }
                    catch
                    {
                    }
                }
            }

            Shutdown(Environment.ExitCode);
        }

        private static void WriteExecutableRelaunchProbeReport(
            ExecutableRelaunchProbeOptions options,
            string status,
            MainWindow window,
            string error)
        {
            var process = Process.GetCurrentProcess();
            var executablePath = Path.GetFullPath(
                Assembly.GetEntryAssembly().Location);
            var sdkPath = Path.GetFullPath(
                typeof(LMCConnection).Assembly.Location);
            var handle = window == null
                ? IntPtr.Zero
                : new WindowInteropHelper(window).Handle;
            var report = new StringBuilder();
            AppendExecutableRelaunchProbeField(
                report,
                "Schema",
                "WPF_EXECUTABLE_RELAUNCH_PROBE_V1");
            AppendExecutableRelaunchProbeField(report, "Status", status);
            AppendExecutableRelaunchProbeField(
                report,
                "Phase",
                options.Phase);
            AppendExecutableRelaunchProbeField(
                report,
                "OwnershipToken",
                options.OwnershipToken);
            AppendExecutableRelaunchProbeField(
                report,
                "ProcessId",
                process.Id.ToString(CultureInfo.InvariantCulture));
            AppendExecutableRelaunchProbeField(
                report,
                "WindowHandle",
                handle.ToInt64().ToString("X", CultureInfo.InvariantCulture));
            AppendExecutableRelaunchProbeField(
                report,
                "RpcEndpoint",
                "127.0.0.1:"
                    + options.RpcPort.ToString(CultureInfo.InvariantCulture));
            AppendExecutableRelaunchProbeField(
                report,
                "CallbackPort",
                "0");
            AppendExecutableRelaunchProbeField(
                report,
                "JournalRoot",
                options.JournalRootPath);
            AppendExecutableRelaunchProbeField(
                report,
                "ExecutablePath",
                executablePath);
            AppendExecutableRelaunchProbeField(
                report,
                "SdkPath",
                sdkPath);
            AppendExecutableRelaunchProbeField(
                report,
                "ConnectionState",
                window == null
                    ? string.Empty
                    : window.TextConnectionState.Text);
            AppendExecutableRelaunchProbeField(
                report,
                "CloseCompletion",
                string.Equals(status, "PASS", StringComparison.Ordinal)
                    ? "PASS_BY_ONCLOSING_COMPLETION"
                    : string.Empty);
            AppendExecutableRelaunchProbeField(
                report,
                "TopologyRows",
                window == null
                    ? "0"
                    : window.GridEtherCATTopology.Items.Count.ToString(
                        CultureInfo.InvariantCulture));
            AppendExecutableRelaunchProbeField(
                report,
                "RpcInitializationBase64",
                EncodeExecutableRelaunchProbeText(
                    window == null
                        ? string.Empty
                        : window.TextRpcInitialization.Text));
            AppendExecutableRelaunchProbeField(
                report,
                "ExecutionLogBase64",
                EncodeExecutableRelaunchProbeText(
                    window == null
                        ? string.Empty
                        : window.TextExecutionLog.Text));
            AppendExecutableRelaunchProbeField(
                report,
                "ErrorBase64",
                EncodeExecutableRelaunchProbeText(error));

            var temporaryPath = options.ReportPath
                + ".tmp."
                + process.Id.ToString(CultureInfo.InvariantCulture);
            File.WriteAllText(
                temporaryPath,
                report.ToString(),
                new UTF8Encoding(false));
            if (File.Exists(options.ReportPath))
            {
                File.Replace(temporaryPath, options.ReportPath, null);
            }
            else
            {
                File.Move(temporaryPath, options.ReportPath);
            }
        }

        private static void AppendExecutableRelaunchProbeField(
            StringBuilder report,
            string name,
            string value)
        {
            report.Append(name);
            report.Append('=');
            report.Append(value ?? string.Empty);
            report.Append(Environment.NewLine);
        }

        private static string EncodeExecutableRelaunchProbeText(string value)
        {
            return Convert.ToBase64String(
                Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private sealed class ExecutableRelaunchProbeOptions
        {
            private ExecutableRelaunchProbeOptions(
                string phase,
                int rpcPort,
                string ownershipToken,
                string basePath,
                string journalRootPath,
                string reportPath)
            {
                Phase = phase;
                RpcPort = rpcPort;
                OwnershipToken = ownershipToken;
                BasePath = basePath;
                JournalRootPath = journalRootPath;
                ReportPath = reportPath;
            }

            internal string Phase { get; private set; }
            internal int RpcPort { get; private set; }
            internal string OwnershipToken { get; private set; }
            internal string BasePath { get; private set; }
            internal string JournalRootPath { get; private set; }
            internal string ReportPath { get; private set; }

            internal static ExecutableRelaunchProbeOptions Parse(
                string[] args)
            {
                if (args == null
                    || args.Length != 7
                    || !string.Equals(
                        args[0],
                        ExecutableRelaunchProbeArgument,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        args[1],
                        "--phase",
                        StringComparison.Ordinal)
                    || !string.Equals(
                        args[3],
                        "--rpc-port",
                        StringComparison.Ordinal)
                    || !string.Equals(
                        args[5],
                        "--ownership-token",
                        StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        "The executable relaunch probe arguments are malformed.");
                }

                var phase = args[2];
                if (!string.Equals(phase, "first", StringComparison.Ordinal)
                    && !string.Equals(
                        phase,
                        "second",
                        StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        "The executable relaunch probe phase is invalid.");
                }

                int rpcPort;
                if (!int.TryParse(
                        args[4],
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out rpcPort)
                    || rpcPort < 1
                    || rpcPort > 65535)
                {
                    throw new ArgumentException(
                        "The executable relaunch probe RPC port is invalid.");
                }

                Guid ownershipToken;
                if (!Guid.TryParseExact(args[6], "N", out ownershipToken))
                {
                    throw new ArgumentException(
                        "The executable relaunch probe ownership token is invalid.");
                }

                var normalizedToken = ownershipToken.ToString("N");
                var basePath = Path.GetFullPath(
                    Path.Combine(
                        Path.GetTempPath(),
                        ExecutableRelaunchProbeDirectoryPrefix
                            + normalizedToken));
                var journalRootPath = Path.Combine(basePath, "journals");
                var reportPath = Path.Combine(
                    basePath,
                    "phase-" + phase + ".report");
                ValidateOwnedTempRoot(
                    phase,
                    basePath,
                    journalRootPath,
                    reportPath);

                return new ExecutableRelaunchProbeOptions(
                    phase,
                    rpcPort,
                    normalizedToken,
                    basePath,
                    journalRootPath,
                    reportPath);
            }

            private static void ValidateOwnedTempRoot(
                string phase,
                string basePath,
                string journalRootPath,
                string reportPath)
            {
                if (!Directory.Exists(basePath)
                    || IsReparsePoint(basePath)
                    || File.Exists(reportPath))
                {
                    throw new InvalidOperationException(
                        "The executable relaunch probe temp root is not a fresh owned directory.");
                }

                EnsureOwnedTempTreeHasNoReparsePoints(basePath);
                var entries = Directory.GetFileSystemEntries(basePath);
                if (string.Equals(phase, "first", StringComparison.Ordinal))
                {
                    if (entries.Length != 0
                        || Directory.Exists(journalRootPath))
                    {
                        throw new InvalidOperationException(
                            "The first executable relaunch probe requires an empty temp root.");
                    }

                    return;
                }

                var firstReportPath = Path.Combine(
                    basePath,
                    "phase-first.report");
                if (!Directory.Exists(journalRootPath)
                    || IsReparsePoint(journalRootPath)
                    || !File.Exists(firstReportPath)
                    || IsReparsePoint(firstReportPath)
                    || entries.Length != 2
                    || !entries.Any(path => string.Equals(
                        Path.GetFullPath(path),
                        Path.GetFullPath(journalRootPath),
                        StringComparison.OrdinalIgnoreCase))
                    || !entries.Any(path => string.Equals(
                        Path.GetFullPath(path),
                        Path.GetFullPath(firstReportPath),
                        StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException(
                        "The second executable relaunch probe does not own the exact first-phase temp state.");
                }
            }

            private static bool IsReparsePoint(string path)
            {
                return (File.GetAttributes(path)
                    & FileAttributes.ReparsePoint) != 0;
            }

            private static void EnsureOwnedTempTreeHasNoReparsePoints(
                string basePath)
            {
                var normalizedRoot = Path.GetFullPath(basePath).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
                var requiredPrefix = normalizedRoot
                    + Path.DirectorySeparatorChar;
                var pendingDirectories = new System.Collections.Generic.Stack<string>();
                pendingDirectories.Push(normalizedRoot);
                while (pendingDirectories.Count != 0)
                {
                    var directory = pendingDirectories.Pop();
                    foreach (var entry in Directory.GetFileSystemEntries(directory))
                    {
                        var fullPath = Path.GetFullPath(entry);
                        if (!fullPath.StartsWith(
                                requiredPrefix,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException(
                                "The executable relaunch probe temp entry escaped its owned root.");
                        }

                        var attributes = File.GetAttributes(fullPath);
                        if ((attributes & FileAttributes.ReparsePoint) != 0)
                        {
                            throw new InvalidOperationException(
                                "The executable relaunch probe temp tree contains a reparse point.");
                        }

                        if ((attributes & FileAttributes.Directory) != 0)
                        {
                            pendingDirectories.Push(fullPath);
                        }
                    }
                }
            }
        }
    }
}

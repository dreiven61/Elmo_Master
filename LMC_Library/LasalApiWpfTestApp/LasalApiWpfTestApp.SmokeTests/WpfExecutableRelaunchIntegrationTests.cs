using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static class WpfExecutableRelaunchIntegrationTests
    {
        private const string InvocationArgument =
            "--wpf-executable-relaunch";
        private const string ProbeDirectoryPrefix =
            "Elmo.WpfExecutableRelaunch.";
        private const int ProcessTimeoutMilliseconds = 30000;
        private const int ReportTimeoutMilliseconds = 25000;
        private const uint WmSysCommand = 0x0112;
        private const uint ScClose = 0xF060;
        private const uint SmtoAbortIfHung = 0x0002;
        private const uint SendMessageTimeoutMilliseconds = 5000;
        private const uint TopologyRevision = 0x15867EECu;
        private const uint DiagnosticMapRevision = 0xE245539Au;
        private const uint DiagnosticsBootId = 0x10203040u;
        private const ushort TopologyNodeCount = 7;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr windowHandle,
            uint message,
            IntPtr wParam,
            IntPtr lParam,
            uint flags,
            uint timeoutMilliseconds,
            out IntPtr result);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr windowHandle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(
            IntPtr windowHandle,
            out uint processId);

        internal static bool IsInvocation(string[] args)
        {
            return args != null
                && args.Length != 0
                && string.Equals(
                    args[0],
                    InvocationArgument,
                    StringComparison.Ordinal);
        }

        internal static int Run(string[] args)
        {
            try
            {
                if (args == null
                    || args.Length != 2
                    || !string.Equals(
                        args[0],
                        InvocationArgument,
                        StringComparison.Ordinal)
                    || string.IsNullOrWhiteSpace(args[1])
                    || !Path.IsPathRooted(args[1]))
                {
                    Console.Error.WriteLine(
                        "ERROR use --wpf-executable-relaunch <absolute-exe-path>.");
                    return 64;
                }

                RunCore(Path.GetFullPath(args[1]));
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(
                    "FAIL Wpf.ExecutableRelaunch.XCloseProcessExitNamedMutexFreshTcp");
                Console.Error.WriteLine(error.ToString());
                return 1;
            }
        }

        private static void RunCore(string executablePath)
        {
            if (!File.Exists(executablePath)
                || !string.Equals(
                    Path.GetFileName(executablePath),
                    "LasalMotionControlApiExample.exe",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new FileNotFoundException(
                    "The exact LASAL API example executable was not found.",
                    executablePath);
            }

            var executableDirectory = Path.GetDirectoryName(executablePath);
            var sdkPath = Path.Combine(
                executableDirectory,
                "LasalMotionControlLib.dll");
            var configPath = executablePath + ".config";
            if (!File.Exists(sdkPath))
            {
                throw new FileNotFoundException(
                    "The example runtime SDK DLL was not found.",
                    sdkPath);
            }

            var beforeExecutable = ArtifactIdentity.Capture(
                executablePath,
                true);
            var beforeSdk = ArtifactIdentity.Capture(sdkPath, true);
            var beforeConfig = ArtifactIdentity.Capture(configPath, false);
            var malformedToken = Guid.NewGuid().ToString("N");
            var malformedRoot = CreateOwnedProbeRoot(malformedToken);
            var probeToken = Guid.NewGuid().ToString("N");
            var probeRoot = CreateOwnedProbeRoot(probeToken);
            var contenderToken = Guid.NewGuid().ToString("N");
            var contenderRoot = CreateOwnedProbeRoot(contenderToken);

            try
            {
                using (var releaseOwnerConnectBarrier =
                    new ManualResetEvent(false))
                using (var ownerConnectBarrierReached =
                    new ManualResetEvent(false))
                using (var server = new FakeRpcServer(
                    CreateRpcSteps(
                        ownerConnectBarrierReached,
                        releaseOwnerConnectBarrier).ToArray()))
                {
                    VerifyMalformedProbeBeforeMutexAndNetwork(
                        executablePath,
                        server,
                        malformedToken,
                        malformedRoot);

                    ProbeReport firstReady;
                    var first = StartProbeProcess(
                        executablePath,
                        "first",
                        server.Port,
                        probeToken);
                    try
                    {
                        firstReady = WaitForProbeReport(
                            first,
                            GetProbeReportPath(probeToken, "first"),
                            "READY");
                        VerifyReadyReport(
                            firstReady,
                            first,
                            executablePath,
                            sdkPath,
                            probeToken,
                            "first",
                            server.Port,
                            false);
                        AssertEx.True(
                            ownerConnectBarrierReached.WaitOne(5000),
                            "Owner READY was published before the final topology response barrier.");
                        AssertEx.Equal(1, server.AcceptedClientCount);

                        try
                        {
                            VerifyLiveContenderIsRejectedBeforeNetwork(
                                executablePath,
                                server,
                                contenderToken,
                                contenderRoot);
                        }
                        finally
                        {
                            releaseOwnerConnectBarrier.Set();
                        }

                        SendTitleBarClose(first, firstReady.WindowHandle);
                        WaitForExactExit(first, 0, "first probe");
                    }
                    finally
                    {
                        releaseOwnerConnectBarrier.Set();
                        TerminateProbeProcess(first);
                    }

                    var firstPass = ReadExactProbeReport(
                        GetProbeReportPath(probeToken, "first"),
                        "PASS");
                    VerifyPassReport(firstPass, "first", true);

                    ProbeReport secondReady;
                    var second = StartProbeProcess(
                        executablePath,
                        "second",
                        server.Port,
                        probeToken);
                    try
                    {
                        secondReady = WaitForProbeReport(
                            second,
                            GetProbeReportPath(probeToken, "second"),
                            "READY");
                        VerifyReadyReport(
                            secondReady,
                            second,
                            executablePath,
                            sdkPath,
                            probeToken,
                            "second",
                            server.Port,
                            true);
                        SendTitleBarClose(second, secondReady.WindowHandle);
                        WaitForExactExit(second, 0, "second probe");
                    }
                    finally
                    {
                        TerminateProbeProcess(second);
                    }

                    var secondPass = ReadExactProbeReport(
                        GetProbeReportPath(probeToken, "second"),
                        "PASS");
                    VerifyPassReport(secondPass, "second", false);

                    server.Verify();
                    VerifyExactWireContract(server);
                }

                var afterExecutable = ArtifactIdentity.Capture(
                    executablePath,
                    true);
                var afterSdk = ArtifactIdentity.Capture(sdkPath, true);
                var afterConfig = ArtifactIdentity.Capture(configPath, false);
                beforeExecutable.AssertSame(afterExecutable, "EXE");
                beforeSdk.AssertSame(afterSdk, "SDK DLL");
                beforeConfig.AssertSame(afterConfig, "optional config");

                Console.WriteLine(
                    "PASS Wpf.ExecutableRelaunch.XCloseProcessExitNamedMutexFreshTcp");
                Console.WriteLine(
                    "Executable relaunch gate: PASS");
                Console.WriteLine(
                    "Fake RPC sessions/requests: 3/28 (13,2,13)");
                Console.WriteLine(
                    "Malformed probe: exit64, temp writes 0, TCP sessions 0; named-mutex contender: exit2, TCP sessions 0");
                Console.WriteLine(
                    "Tested EXE SHA256: " + beforeExecutable.Sha256);
                Console.WriteLine(
                    "Tested SDK SHA256: " + beforeSdk.Sha256);
                Console.WriteLine(
                    "Optional config identity: "
                    + (beforeConfig.Exists
                        ? beforeConfig.Sha256
                        : "ABSENT_TO_ABSENT_PASS"));
                Console.WriteLine(
                    "Evidence boundary: PC loopback fake-server only; not PLC cleanup, disarm, or readiness proof.");
            }
            finally
            {
                DeleteOwnedProbeRoot(malformedToken, malformedRoot);
                DeleteOwnedProbeRoot(probeToken, probeRoot);
                DeleteOwnedProbeRoot(contenderToken, contenderRoot);
            }
        }

        private static void VerifyMalformedProbeBeforeMutexAndNetwork(
            string executablePath,
            FakeRpcServer server,
            string token,
            string root)
        {
            ApplicationInstanceLease lease;
            AssertEx.True(
                ApplicationInstanceLease.TryAcquireDefault(out lease),
                "The malformed-probe precondition could not own the default named mutex.");
            using (lease)
            {
                var process = StartProbeProcess(
                executablePath,
                "invalid",
                server.Port,
                token);
                try
                {
                    WaitForExactExit(process, 64, "malformed probe");
                }
                finally
                {
                    TerminateProbeProcess(process);
                }
            }

            AssertEx.Equal(
                0,
                Directory.GetFileSystemEntries(root).Length,
                "Malformed probe arguments touched the owned temp root.");
            AssertEx.Equal(
                0,
                server.AcceptedClientCount,
                "Malformed probe arguments reached the fake RPC listener.");
        }

        private static void VerifyLiveContenderIsRejectedBeforeNetwork(
            string executablePath,
            FakeRpcServer server,
            string token,
            string root)
        {
            var contender = StartProbeProcess(
                executablePath,
                "first",
                server.Port,
                token);
            try
            {
                WaitForExactExit(contender, 2, "live named-mutex contender");
            }
            finally
            {
                TerminateProbeProcess(contender);
            }

            var report = ReadExactProbeReport(
                GetProbeReportPath(token, "first"),
                "MUTEX_BUSY");
            AssertEx.Equal("first", report.Phase);
            AssertEx.Equal(token, report.OwnershipToken);
            AssertEx.Equal(
                1,
                Directory.GetFileSystemEntries(root).Length,
                "The rejected contender opened journals or other temp state.");
            AssertEx.Equal(
                1,
                server.AcceptedClientCount,
                "The rejected contender opened a TCP session.");
        }

        private static Process StartProbeProcess(
            string executablePath,
            string phase,
            int port,
            string ownershipToken)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = App.ExecutableRelaunchProbeArgument
                        + " --phase "
                        + phase
                        + " --rpc-port "
                        + port.ToString(CultureInfo.InvariantCulture)
                        + " --ownership-token "
                        + ownershipToken,
                    WorkingDirectory = Path.GetDirectoryName(executablePath),
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };
            if (!process.Start())
            {
                process.Dispose();
                throw new InvalidOperationException(
                    "The executable relaunch probe process did not start.");
            }

            return process;
        }

        private static ProbeReport WaitForProbeReport(
            Process process,
            string reportPath,
            string expectedStatus)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(
                ReportTimeoutMilliseconds);
            while (DateTime.UtcNow < deadline)
            {
                if (File.Exists(reportPath))
                {
                    ProbeReport report;
                    if (ProbeReport.TryRead(reportPath, out report))
                    {
                        if (string.Equals(
                                report.Status,
                                expectedStatus,
                                StringComparison.Ordinal))
                        {
                            return report;
                        }

                        if (!string.Equals(
                                report.Status,
                                "READY",
                                StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException(
                                "Probe reported "
                                + report.Status
                                + ": "
                                + report.ErrorText);
                        }
                    }
                }

                if (process.HasExited)
                {
                    throw new InvalidOperationException(
                        "Probe exited before "
                        + expectedStatus
                        + ", ExitCode="
                        + process.ExitCode.ToString(
                            CultureInfo.InvariantCulture));
                }

                Thread.Sleep(25);
            }

            throw new TimeoutException(
                "Probe report did not reach " + expectedStatus + ".");
        }

        private static ProbeReport ReadExactProbeReport(
            string reportPath,
            string expectedStatus)
        {
            ProbeReport report;
            if (!ProbeReport.TryRead(reportPath, out report)
                || !string.Equals(
                    report.Status,
                    expectedStatus,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The final probe report was not " + expectedStatus + ".");
            }

            return report;
        }

        private static void VerifyReadyReport(
            ProbeReport report,
            Process process,
            string executablePath,
            string sdkPath,
            string token,
            string phase,
            int port,
            bool freshSessionRetryExpected)
        {
            AssertEx.Equal(
                "WPF_EXECUTABLE_RELAUNCH_PROBE_V1",
                report.Schema);
            AssertEx.Equal("READY", report.Status);
            AssertEx.Equal(phase, report.Phase);
            AssertEx.Equal(token, report.OwnershipToken);
            AssertEx.Equal(process.Id, report.ProcessId);
            AssertEx.Equal(
                "127.0.0.1:"
                    + port.ToString(CultureInfo.InvariantCulture),
                report.RpcEndpoint);
            AssertEx.Equal("0", report.CallbackPort);
            AssertExactPath(executablePath, report.ExecutablePath, "EXE");
            AssertExactPath(sdkPath, report.SdkPath, "SDK DLL");
            AssertEx.Equal("Connected", report.ConnectionState);
            AssertEx.Equal((int)TopologyNodeCount, report.TopologyRows);
            AssertEx.True(
                report.WindowHandle != IntPtr.Zero,
                "Probe did not publish its actual MainWindow handle.");

            uint ownerProcessId;
            AssertEx.True(
                IsWindow(report.WindowHandle),
                "Published MainWindow handle is not a live window.");
            AssertEx.True(
                GetWindowThreadProcessId(
                    report.WindowHandle,
                    out ownerProcessId) != 0,
                "Published MainWindow handle has no owning thread.");
            AssertEx.Equal(
                process.Id,
                checked((int)ownerProcessId),
                "Published MainWindow belongs to another process.");

            AssertEx.Contains(
                "ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2",
                report.ExecutionLogText);
            AssertEx.Contains("SdkPath=", report.ExecutionLogText);
            if (freshSessionRetryExpected)
            {
                AssertEx.Contains(
                    "FreshSessionRetry=Used",
                    report.RpcInitializationText);
            }
        }

        private static void VerifyPassReport(
            ProbeReport report,
            string phase,
            bool closeMinusOneExpected)
        {
            AssertEx.Equal("PASS", report.Status);
            AssertEx.Equal(phase, report.Phase);
            AssertEx.Equal(
                "PASS_BY_ONCLOSING_COMPLETION",
                report.CloseCompletion);
            AssertEx.Equal("Disconnected", report.ConnectionState);
            if (closeMinusOneExpected)
            {
                AssertEx.Contains(
                    "Shutdown RPC close warning retained after local cleanup.",
                    report.ExecutionLogText);
                AssertEx.Contains("ErrorId=-1", report.ExecutionLogText);
            }
        }

        private static void SendTitleBarClose(
            Process process,
            IntPtr windowHandle)
        {
            uint ownerProcessId;
            if (!IsWindow(windowHandle)
                || GetWindowThreadProcessId(
                    windowHandle,
                    out ownerProcessId) == 0
                || ownerProcessId != (uint)process.Id)
            {
                throw new InvalidOperationException(
                    "Refusing to send SC_CLOSE to an unowned window handle.");
            }

            IntPtr messageResult;
            var sendResult = SendMessageTimeout(
                windowHandle,
                WmSysCommand,
                new IntPtr(ScClose),
                IntPtr.Zero,
                SmtoAbortIfHung,
                SendMessageTimeoutMilliseconds,
                out messageResult);
            if (sendResult == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    "SC_CLOSE failed or timed out. Win32Error="
                    + Marshal.GetLastWin32Error().ToString(
                        CultureInfo.InvariantCulture));
            }
        }

        private static void WaitForExactExit(
            Process process,
            int expectedExitCode,
            string description)
        {
            if (!process.WaitForExit(ProcessTimeoutMilliseconds))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(ProcessTimeoutMilliseconds);
                }
                catch
                {
                }

                throw new TimeoutException(
                    description + " did not exit within the bounded timeout.");
            }

            AssertEx.Equal(
                expectedExitCode,
                process.ExitCode,
                description + " returned an unexpected exit code.");
        }

        private static void TerminateProbeProcess(Process process)
        {
            if (process == null)
            {
                return;
            }

            var processId = process.Id;
            var exited = false;
            Exception terminationError = null;
            try
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (Exception error)
                {
                    terminationError = error;
                }

                try
                {
                    if (!process.HasExited)
                    {
                        process.WaitForExit(5000);
                    }
                    exited = process.HasExited;
                }
                catch (Exception error)
                {
                    terminationError = terminationError == null
                        ? error
                        : new AggregateException(
                            terminationError,
                            error);
                }

            }
            finally
            {
                process.Dispose();
            }

            if (!exited)
            {
                throw new InvalidOperationException(
                    "Probe cleanup left PID "
                    + processId.ToString(CultureInfo.InvariantCulture)
                    + " running.",
                    terminationError);
            }
        }

        private static void VerifyExactWireContract(FakeRpcServer server)
        {
            AssertEx.Equal(3, server.AcceptedClientCount);
            AssertEx.Equal(28, server.ReceivedRequests.Count);
            AssertEx.Equal(13, CountRequestsInSession(server, 1));
            AssertEx.Equal(2, CountRequestsInSession(server, 2));
            AssertEx.Equal(13, CountRequestsInSession(server, 3));
            AssertEx.Equal(1, CountCommandInSession(server, 1, 0x8080));
            AssertEx.Equal(1, CountCommandInSession(server, 1, 0x405C));
            AssertEx.Equal(1, CountCommandInSession(server, 1, 0x405D));
            AssertEx.Equal(2, CountCommandInSession(server, 2, 0x8080));
            AssertEx.Equal(0, CountCommandInSession(server, 2, 0x405C));
            AssertEx.Equal(0, CountCommandInSession(server, 2, 0x405D));
            AssertEx.Equal(1, CountCommandInSession(server, 3, 0x8080));
            AssertEx.Equal(1, CountCommandInSession(server, 3, 0x405C));
            AssertEx.Equal(1, CountCommandInSession(server, 3, 0x405D));
        }

        private static int CountRequestsInSession(
            FakeRpcServer server,
            int sessionOrdinal)
        {
            return server.ReceivedRequestSessionOrdinals.Count(
                ordinal => ordinal == sessionOrdinal);
        }

        private static int CountCommandInSession(
            FakeRpcServer server,
            int sessionOrdinal,
            ushort command)
        {
            var count = 0;
            var requests = server.ReceivedRequests;
            var ordinals = server.ReceivedRequestSessionOrdinals;
            for (var index = 0; index < requests.Count; index++)
            {
                if (ordinals[index] == sessionOrdinal
                    && TestFrame.ReadUInt16(requests[index], 0) == command)
                {
                    count++;
                }
            }

            return count;
        }

        private static List<FakeRpcStep> CreateRpcSteps(
            EventWaitHandle ownerConnectBarrierReached,
            WaitHandle releaseOwnerConnectBarrier)
        {
            var steps = CreateSuccessfulConnectSteps();
            steps[steps.Count - 1].AfterResponse = request =>
            {
                ownerConnectBarrierReached.Set();
                if (!releaseOwnerConnectBarrier.WaitOne(
                    ProcessTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The owner-connect contender barrier was not released.");
                }
            };
            steps.Add(CloseShortFailureStep(-1));
            steps.Add(ClientDisconnectBoundaryStep());
            steps.Add(SessionInitShortFailureStep(-1));
            steps.Add(SessionInitShortFailureStep(-1));
            steps.Add(ClientDisconnectBoundaryStep());
            steps.AddRange(CreateSuccessfulConnectSteps());
            steps.Add(CloseStep());
            return steps;
        }

        private static List<FakeRpcStep> CreateSuccessfulConnectSteps()
        {
            var canonical = CreateTopologyCanonicalBytes();
            var steps = new List<FakeRpcStep>
            {
                InitStep(),
                CallbackStep(),
                CapabilitiesStep(1),
                CapabilitiesStep(2),
                new FakeRpcStep(
                    0x7E11,
                    TestFrame.Response(0, TopologyInfoPayload(3)))
            };
            for (ushort startIndex = 0;
                startIndex < TopologyNodeCount;
                startIndex++)
            {
                steps.Add(new FakeRpcStep(
                    0x7E12,
                    TestFrame.Response(
                        0,
                        TopologyChunkPayload(
                            checked((uint)(4 + startIndex)),
                            startIndex,
                            canonical))));
            }

            return steps;
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(0x8080, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep SessionInitShortFailureStep(short errorId)
        {
            var payload = new byte[4];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteInt16(payload, 2, errorId);
            return new FakeRpcStep(
                0x8080,
                TestFrame.Response(1, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(0x405C, null)
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    CallbackResponsePayload(request))
            };
        }

        private static byte[] CallbackResponsePayload(byte[] request)
        {
            AssertEx.Equal(40, request.Length);
            AssertEx.Equal((ushort)32, TestFrame.ReadUInt16(request, 4));
            AssertEx.Equal(1u, TestFrame.ReadUInt32(request, 8));
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 20));
            AssertEx.Equal((ushort)52, TestFrame.ReadUInt16(request, 22));
            AssertEx.True(
                TestFrame.ReadUInt32(request, 24) != 0
                    || TestFrame.ReadUInt32(request, 28) != 0,
                "Version-2 callback registration cookie was zero.");
            var payload = new byte[20];
            TestFrame.WriteUInt16(payload, 4, 2);
            TestFrame.WriteUInt16(payload, 6, 52);
            TestFrame.WriteUInt32(payload, 8, DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 12, 1);
            return payload;
        }

        private static FakeRpcStep CapabilitiesStep(uint requestId)
        {
            var payload = CommonPayload(68, requestId);
            TestFrame.WriteUInt32(payload, 16, 1);
            TestFrame.WriteUInt32(
                payload,
                20,
                (uint)LMCDiagnosticCapability.EtherCATTopology);
            TestFrame.WriteUInt32(payload, 24, DiagnosticMapRevision);
            TestFrame.WriteUInt16(payload, 32, 4);
            TestFrame.WriteUInt32(payload, 36, 1000);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt16(payload, 50, 80);
            TestFrame.WriteUInt16(payload, 52, 16);
            TestFrame.WriteUInt32(payload, 56, 16000);
            TestFrame.WriteUInt16(payload, 60, 4);
            TestFrame.WriteUInt32(payload, 64, DiagnosticsBootId);
            return new FakeRpcStep(
                0x7E00,
                TestFrame.Response(0, payload));
        }

        private static byte[] TopologyInfoPayload(uint requestId)
        {
            var payload = CommonPayload(44, requestId);
            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt16(payload, 20, TopologyNodeCount);
            TestFrame.WriteUInt16(payload, 22, 96);
            TestFrame.WriteUInt16(payload, 24, 1);
            TestFrame.WriteUInt16(payload, 26, 5);
            TestFrame.WriteUInt16(payload, 28, 2);
            TestFrame.WriteUInt16(payload, 30, 4);
            TestFrame.WriteUInt32(payload, 32, 0x0000000Fu);
            TestFrame.WriteUInt32(payload, 36, 1);
            return payload;
        }

        private static byte[] TopologyChunkPayload(
            uint requestId,
            ushort startIndex,
            byte[] canonical)
        {
            var payload = CommonPayload(124, requestId);
            if (startIndex == TopologyNodeCount - 1)
            {
                TestFrame.WriteUInt16(payload, 2, 2);
            }

            TestFrame.WriteUInt32(payload, 16, TopologyRevision);
            TestFrame.WriteUInt16(payload, 20, startIndex);
            TestFrame.WriteUInt16(payload, 22, 1);
            TestFrame.WriteUInt16(payload, 24, TopologyNodeCount);
            TestFrame.WriteUInt16(payload, 26, 96);
            Buffer.BlockCopy(canonical, startIndex * 96, payload, 28, 96);
            return payload;
        }

        private static byte[] CommonPayload(int length, uint requestId)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep CloseShortFailureStep(short errorId)
        {
            var payload = new byte[4];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteInt16(payload, 2, errorId);
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(1, payload));
        }

        private static FakeRpcStep ClientDisconnectBoundaryStep()
        {
            return new FakeRpcStep(0, null)
            {
                RequireClientDisconnectBeforeRequest = true,
                ContinueWithNextClientAfterDisconnect = true
            };
        }

        private static byte[] CreateTopologyCanonicalBytes()
        {
            var canonical = new byte[TopologyNodeCount * 96];
            WriteTopologyEntry(
                canonical,
                0,
                0xEC000001u,
                0,
                0,
                0,
                1,
                65,
                0,
                0,
                ushort.MaxValue,
                669,
                1196200070,
                65536,
                0,
                0,
                0,
                "GL_9086_11",
                0);
            var driveNames = new[]
            {
                "Elmo_11",
                "Elmo_21",
                "Elmo_31",
                "Elmo_41"
            };
            for (ushort axis = 1; axis <= driveNames.Length; axis++)
            {
                WriteTopologyEntry(
                    canonical,
                    axis,
                    checked(0xEC000100u + axis),
                    0,
                    axis,
                    axis,
                    1,
                    39,
                    axis,
                    axis,
                    ushort.MaxValue,
                    154,
                    198948,
                    66592,
                    0,
                    0,
                    0,
                    driveNames[axis - 1],
                    0);
            }

            WriteTopologyEntry(
                canonical,
                5,
                0xEC010001u,
                0xEC000001u,
                5,
                ushort.MaxValue,
                2,
                136,
                0,
                0,
                0,
                669,
                1196692218,
                0,
                0,
                4,
                0,
                "GL_9086_1_Slot001",
                0x00010001u);
            WriteTopologyEntry(
                canonical,
                6,
                0xEC010002u,
                0xEC000001u,
                6,
                ushort.MaxValue,
                2,
                144,
                0,
                0,
                1,
                669,
                1196696250,
                0,
                0,
                0,
                4,
                "GL_9086_1_Slot011",
                0x00010002u);
            return canonical;
        }

        private static void WriteTopologyEntry(
            byte[] canonical,
            int entryIndex,
            uint nodeId,
            uint parentNodeId,
            ushort topologyIndex,
            ushort masterSlaveIndex,
            byte nodeKind,
            ushort nodeFlags,
            ushort sdoSlaveReference,
            ushort physicalAxisReference,
            ushort slotIndex,
            uint vendorId,
            uint productCode,
            uint revisionNumber,
            uint serialNumber,
            ushort inputBytes,
            ushort outputBytes,
            string name,
            uint ioReference)
        {
            var offset = entryIndex * 96;
            TestFrame.WriteUInt32(canonical, offset, nodeId);
            TestFrame.WriteUInt32(canonical, offset + 4, parentNodeId);
            TestFrame.WriteUInt16(canonical, offset + 8, topologyIndex);
            TestFrame.WriteUInt16(canonical, offset + 10, masterSlaveIndex);
            canonical[offset + 12] = nodeKind;
            TestFrame.WriteUInt16(canonical, offset + 14, nodeFlags);
            TestFrame.WriteUInt16(canonical, offset + 16, sdoSlaveReference);
            TestFrame.WriteUInt16(canonical, offset + 18, physicalAxisReference);
            TestFrame.WriteUInt16(canonical, offset + 20, slotIndex);
            TestFrame.WriteUInt32(canonical, offset + 24, vendorId);
            TestFrame.WriteUInt32(canonical, offset + 28, productCode);
            TestFrame.WriteUInt32(canonical, offset + 32, revisionNumber);
            TestFrame.WriteUInt32(canonical, offset + 36, serialNumber);
            TestFrame.WriteUInt16(canonical, offset + 40, inputBytes);
            TestFrame.WriteUInt16(canonical, offset + 42, outputBytes);
            var nameBytes = Encoding.ASCII.GetBytes(name);
            Buffer.BlockCopy(
                nameBytes,
                0,
                canonical,
                offset + 44,
                nameBytes.Length);
            TestFrame.WriteUInt32(canonical, offset + 92, ioReference);
        }

        private static string CreateOwnedProbeRoot(string token)
        {
            var root = GetProbeBasePath(token);
            if (Directory.Exists(root) || File.Exists(root))
            {
                throw new InvalidOperationException(
                    "Generated probe root already exists.");
            }

            Directory.CreateDirectory(root);
            if ((File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidOperationException(
                    "Generated probe root is a reparse point.");
            }

            return root;
        }

        private static string GetProbeBasePath(string token)
        {
            return Path.GetFullPath(
                Path.Combine(
                    Path.GetTempPath(),
                    ProbeDirectoryPrefix + token));
        }

        private static string GetProbeReportPath(
            string token,
            string phase)
        {
            return Path.Combine(
                GetProbeBasePath(token),
                "phase-" + phase + ".report");
        }

        private static void DeleteOwnedProbeRoot(
            string token,
            string root)
        {
            var expected = GetProbeBasePath(token);
            if (!string.Equals(
                    Path.GetFullPath(root),
                    expected,
                    StringComparison.OrdinalIgnoreCase)
                || !Path.GetFileName(expected).StartsWith(
                    ProbeDirectoryPrefix,
                    StringComparison.Ordinal)
                || !Directory.Exists(expected))
            {
                return;
            }

            if ((File.GetAttributes(expected) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidOperationException(
                    "Refusing to delete a reparse-point probe root.");
            }

            ValidateOwnedProbeTreeForDeletion(expected);
            Directory.Delete(expected, true);
        }

        private static void ValidateOwnedProbeTreeForDeletion(string root)
        {
            var normalizedRoot = Path.GetFullPath(root).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            var requiredPrefix = normalizedRoot
                + Path.DirectorySeparatorChar;
            var pendingDirectories = new Stack<string>();
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
                            "Probe cleanup entry escaped its owned root.");
                    }

                    var attributes = File.GetAttributes(fullPath);
                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        throw new InvalidOperationException(
                            "Refusing to delete a probe tree containing a reparse point.");
                    }

                    if ((attributes & FileAttributes.Directory) != 0)
                    {
                        pendingDirectories.Push(fullPath);
                    }
                }
            }
        }

        private static void AssertExactPath(
            string expected,
            string actual,
            string description)
        {
            AssertEx.True(
                string.Equals(
                    Path.GetFullPath(expected),
                    Path.GetFullPath(actual),
                    StringComparison.OrdinalIgnoreCase),
                description + " path provenance mismatch.");
        }

        private sealed class ProbeReport
        {
            private static readonly string[] ExpectedFieldNames =
            {
                "Schema",
                "Status",
                "Phase",
                "OwnershipToken",
                "ProcessId",
                "WindowHandle",
                "RpcEndpoint",
                "CallbackPort",
                "JournalRoot",
                "ExecutablePath",
                "SdkPath",
                "ConnectionState",
                "CloseCompletion",
                "TopologyRows",
                "RpcInitializationBase64",
                "ExecutionLogBase64",
                "ErrorBase64"
            };

            private ProbeReport(IDictionary<string, string> fields)
            {
                Schema = Required(fields, "Schema");
                Status = Required(fields, "Status");
                Phase = Required(fields, "Phase");
                OwnershipToken = Required(fields, "OwnershipToken");
                ProcessId = int.Parse(
                    Required(fields, "ProcessId"),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture);
                WindowHandle = new IntPtr(long.Parse(
                    Required(fields, "WindowHandle"),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture));
                RpcEndpoint = Required(fields, "RpcEndpoint");
                CallbackPort = Required(fields, "CallbackPort");
                JournalRoot = Required(fields, "JournalRoot");
                ExecutablePath = Required(fields, "ExecutablePath");
                SdkPath = Required(fields, "SdkPath");
                ConnectionState = Required(fields, "ConnectionState");
                CloseCompletion = Required(fields, "CloseCompletion");
                TopologyRows = int.Parse(
                    Required(fields, "TopologyRows"),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture);
                RpcInitializationText = Decode(
                    Required(fields, "RpcInitializationBase64"));
                ExecutionLogText = Decode(
                    Required(fields, "ExecutionLogBase64"));
                ErrorText = Decode(Required(fields, "ErrorBase64"));
            }

            internal string Schema { get; private set; }
            internal string Status { get; private set; }
            internal string Phase { get; private set; }
            internal string OwnershipToken { get; private set; }
            internal int ProcessId { get; private set; }
            internal IntPtr WindowHandle { get; private set; }
            internal string RpcEndpoint { get; private set; }
            internal string CallbackPort { get; private set; }
            internal string JournalRoot { get; private set; }
            internal string ExecutablePath { get; private set; }
            internal string SdkPath { get; private set; }
            internal string ConnectionState { get; private set; }
            internal string CloseCompletion { get; private set; }
            internal int TopologyRows { get; private set; }
            internal string RpcInitializationText { get; private set; }
            internal string ExecutionLogText { get; private set; }
            internal string ErrorText { get; private set; }

            internal static bool TryRead(
                string path,
                out ProbeReport report)
            {
                report = null;
                try
                {
                    var fields = new Dictionary<string, string>(
                        StringComparer.Ordinal);
                    foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
                    {
                        var separator = line.IndexOf('=');
                        if (separator <= 0)
                        {
                            return false;
                        }

                        var name = line.Substring(0, separator);
                        var value = line.Substring(separator + 1);
                        if (!ExpectedFieldNames.Contains(name)
                            || fields.ContainsKey(name))
                        {
                            return false;
                        }

                        fields.Add(name, value);
                    }

                    if (fields.Count != ExpectedFieldNames.Length)
                    {
                        return false;
                    }

                    report = new ProbeReport(fields);
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
                catch (FormatException)
                {
                    return false;
                }
                catch (OverflowException)
                {
                    return false;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
            }

            private static string Required(
                IDictionary<string, string> fields,
                string name)
            {
                string value;
                if (!fields.TryGetValue(name, out value))
                {
                    throw new KeyNotFoundException(name);
                }

                return value;
            }

            private static string Decode(string value)
            {
                return Encoding.UTF8.GetString(
                    Convert.FromBase64String(value));
            }
        }

        private sealed class ArtifactIdentity
        {
            private ArtifactIdentity(
                string path,
                bool exists,
                long length,
                string sha256)
            {
                Path = path;
                Exists = exists;
                Length = length;
                Sha256 = sha256;
            }

            internal string Path { get; private set; }
            internal bool Exists { get; private set; }
            internal long Length { get; private set; }
            internal string Sha256 { get; private set; }

            internal static ArtifactIdentity Capture(
                string path,
                bool required)
            {
                var fullPath = System.IO.Path.GetFullPath(path);
                if (!File.Exists(fullPath))
                {
                    if (required)
                    {
                        throw new FileNotFoundException(
                            "Required identity artifact is missing.",
                            fullPath);
                    }

                    return new ArtifactIdentity(
                        fullPath,
                        false,
                        0,
                        string.Empty);
                }

                var file = new FileInfo(fullPath);
                using (var stream = new FileStream(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                using (var sha = SHA256.Create())
                {
                    return new ArtifactIdentity(
                        fullPath,
                        true,
                        file.Length,
                        BitConverter.ToString(sha.ComputeHash(stream))
                            .Replace("-", string.Empty));
                }
            }

            internal void AssertSame(
                ArtifactIdentity actual,
                string description)
            {
                AssertExactPath(Path, actual.Path, description);
                AssertEx.Equal(
                    Exists,
                    actual.Exists,
                    description + " existence changed during the gate.");
                AssertEx.Equal(
                    Length,
                    actual.Length,
                    description + " length changed during the gate.");
                AssertEx.Equal(
                    Sha256,
                    actual.Sha256,
                    description + " SHA-256 changed during the gate.");
            }
        }
    }
}

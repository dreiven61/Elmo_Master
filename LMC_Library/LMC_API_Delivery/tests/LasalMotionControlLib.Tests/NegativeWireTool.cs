using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlLib.Tests
{
    internal enum NegativeWireScenario
    {
        All = 0,
        MalformedPayload = 1,
        StaleMapRevision = 2,
        StaleBootId = 3,
        StaleConfigRevision = 4,
        DuplicateBulkRelease = 5
    }

    internal sealed class NegativeWireOptions
    {
        internal const string LiveConfirmation = "PLC-RAW-NEGATIVE";

        internal NegativeWireOptions()
        {
            Scenario = NegativeWireScenario.All;
            RemotePort = 4000;
            TimeoutMilliseconds = 3000;
        }

        internal NegativeWireScenario Scenario { get; private set; }
        internal bool ExecuteLive { get; private set; }
        internal bool ShowHelp { get; private set; }
        internal string RemoteAddress { get; private set; }
        internal int RemotePort { get; private set; }
        internal string LocalAddress { get; private set; }
        internal int TimeoutMilliseconds { get; private set; }
        internal string OutputPath { get; private set; }
        internal string Confirmation { get; private set; }

        internal static NegativeWireOptions Parse(string[] args)
        {
            if (args == null
                || args.Length == 0
                || !string.Equals(
                    args[0],
                    "negative-wire",
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The first argument must be the exact token 'negative-wire'.");
            }

            var options = new NegativeWireOptions();
            var sawDryRun = false;

            for (var index = 1; index < args.Length; index++)
            {
                var argument = args[index];
                if (string.Equals(argument, "--help", StringComparison.Ordinal))
                {
                    options.ShowHelp = true;
                }
                else if (string.Equals(argument, "--dry-run", StringComparison.Ordinal))
                {
                    sawDryRun = true;
                }
                else if (string.Equals(argument, "--execute-live", StringComparison.Ordinal))
                {
                    options.ExecuteLive = true;
                }
                else if (string.Equals(argument, "--scenario", StringComparison.Ordinal))
                {
                    options.Scenario = ParseScenario(
                        ReadValue(args, ref index, argument));
                }
                else if (string.Equals(argument, "--host", StringComparison.Ordinal))
                {
                    options.RemoteAddress = ReadValue(args, ref index, argument);
                }
                else if (string.Equals(argument, "--port", StringComparison.Ordinal))
                {
                    options.RemotePort = ParseBoundedInt(
                        ReadValue(args, ref index, argument),
                        argument,
                        1,
                        65535);
                }
                else if (string.Equals(argument, "--local", StringComparison.Ordinal))
                {
                    options.LocalAddress = ReadValue(args, ref index, argument);
                }
                else if (string.Equals(argument, "--timeout-ms", StringComparison.Ordinal))
                {
                    options.TimeoutMilliseconds = ParseBoundedInt(
                        ReadValue(args, ref index, argument),
                        argument,
                        250,
                        10000);
                }
                else if (string.Equals(argument, "--output", StringComparison.Ordinal))
                {
                    options.OutputPath = ReadValue(args, ref index, argument);
                }
                else if (string.Equals(argument, "--confirm", StringComparison.Ordinal))
                {
                    options.Confirmation = ReadValue(args, ref index, argument);
                }
                else
                {
                    throw new ArgumentException(
                        "Unknown negative-wire argument '" + argument + "'.");
                }
            }

            if (sawDryRun && options.ExecuteLive)
            {
                throw new ArgumentException(
                    "--dry-run and --execute-live cannot be used together.");
            }

            if (options.ShowHelp)
            {
                return options;
            }

            if (options.ExecuteLive)
            {
                RequireLiveOptions(options);
            }

            return options;
        }

        internal static string GetScenarioToken(NegativeWireScenario scenario)
        {
            switch (scenario)
            {
                case NegativeWireScenario.All:
                    return "all";
                case NegativeWireScenario.MalformedPayload:
                    return "malformed-payload";
                case NegativeWireScenario.StaleMapRevision:
                    return "stale-map";
                case NegativeWireScenario.StaleBootId:
                    return "stale-boot";
                case NegativeWireScenario.StaleConfigRevision:
                    return "stale-config";
                case NegativeWireScenario.DuplicateBulkRelease:
                    return "duplicate-bulk-release";
                default:
                    throw new ArgumentOutOfRangeException("scenario");
            }
        }

        private static void RequireLiveOptions(NegativeWireOptions options)
        {
            if (!string.Equals(
                options.Confirmation,
                LiveConfirmation,
                StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Live execution requires --confirm "
                    + LiveConfirmation
                    + ".");
            }

            RequireIpv4(options.RemoteAddress, "--host");
            RequireIpv4(options.LocalAddress, "--local");

            if (string.IsNullOrWhiteSpace(options.OutputPath))
            {
                throw new ArgumentException(
                    "Live execution requires an explicit --output report path.");
            }

            options.OutputPath = Path.GetFullPath(options.OutputPath);
        }

        private static void RequireIpv4(string value, string option)
        {
            IPAddress address;
            if (!IPAddress.TryParse(value, out address)
                || address.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException(
                    option + " requires an explicit IPv4 address.");
            }
        }

        private static string ReadValue(
            string[] args,
            ref int index,
            string option)
        {
            if (index + 1 >= args.Length)
            {
                throw new ArgumentException(option + " requires a value.");
            }

            index++;
            var value = args[index];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    option + " requires a non-empty value.");
            }

            return value;
        }

        private static int ParseBoundedInt(
            string value,
            string option,
            int minimum,
            int maximum)
        {
            int parsed;
            if (!int.TryParse(
                    value,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out parsed)
                || parsed < minimum
                || parsed > maximum)
            {
                throw new ArgumentException(
                    option
                    + " must be between "
                    + minimum
                    + " and "
                    + maximum
                    + ".");
            }

            return parsed;
        }

        private static NegativeWireScenario ParseScenario(string value)
        {
            switch (value)
            {
                case "all":
                    return NegativeWireScenario.All;
                case "malformed-payload":
                    return NegativeWireScenario.MalformedPayload;
                case "stale-map":
                    return NegativeWireScenario.StaleMapRevision;
                case "stale-boot":
                    return NegativeWireScenario.StaleBootId;
                case "stale-config":
                    return NegativeWireScenario.StaleConfigRevision;
                case "duplicate-bulk-release":
                    return NegativeWireScenario.DuplicateBulkRelease;
                default:
                    throw new ArgumentException(
                        "Unknown negative-wire scenario '" + value + "'.");
            }
        }
    }

    internal sealed class NegativeWireReport
    {
        private readonly StringBuilder text = new StringBuilder();

        internal NegativeWireReport(NegativeWireOptions options)
        {
            Add("FORMAT", "LMC_NEGATIVE_WIRE_V1");
            Add("START_UTC", DateTime.UtcNow.ToString("O"));
            Add("MODE", options.ExecuteLive ? "LIVE" : "DRY_RUN");
            Add("SCENARIO", NegativeWireOptions.GetScenarioToken(
                options.Scenario));
            Add("PCAP_EVIDENCE", "NOT_CAPTURED_BY_TOOL");

            if (options.ExecuteLive)
            {
                Add("REMOTE_ENDPOINT", options.RemoteAddress
                    + ":"
                    + options.RemotePort);
                Add("LOCAL_IPV4", options.LocalAddress);
                Add("TIMEOUT_MS", options.TimeoutMilliseconds);
            }
        }

        internal void Add(string key, object value)
        {
            var rendered = value == null
                ? string.Empty
                : Convert.ToString(value, CultureInfo.InvariantCulture);
            rendered = rendered
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
            text.Append(key).Append('=').Append(rendered).AppendLine();
        }

        internal void AddFrame(string prefix, byte[] frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }

            Add(prefix + "_BYTES", frame.Length);
            Add(prefix + "_SHA256", ComputeSha256(frame));
            Add(prefix + "_HEX", ToHex(frame));
        }

        internal void Save(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var fullPath = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                fullPath,
                text.ToString(),
                new UTF8Encoding(false));
        }

        internal void WriteTo(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanWrite || !stream.CanSeek)
            {
                throw new ArgumentException(
                    "The report stream must be writable and seekable.",
                    "stream");
            }

            var bytes = new UTF8Encoding(false).GetBytes(text.ToString());
            stream.Position = 0;
            stream.SetLength(0);
            stream.Write(bytes, 0, bytes.Length);
            var fileStream = stream as FileStream;
            if (fileStream != null)
            {
                fileStream.Flush(true);
            }
            else
            {
                stream.Flush();
            }
        }

        public override string ToString()
        {
            return text.ToString();
        }

        internal static string ToHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            for (var index = 0; index < bytes.Length; index++)
            {
                builder.Append(bytes[index].ToString("X2"));
            }

            return builder.ToString();
        }

        internal static string ComputeSha256(byte[] bytes)
        {
            using (var hash = SHA256.Create())
            {
                return ToHex(hash.ComputeHash(bytes));
            }
        }
    }

    internal static class NegativeWireTool
    {
        internal const int SuccessExitCode = 0;
        internal const int UsageExitCode = 2;
        internal const int VerificationFailureExitCode = 3;
        internal const int ReportFailureExitCode = 4;

        private static readonly ushort[] AllowedRawCommands =
        {
            LMC_CommandId.GetSignalCatalogInfo,
            LMC_CommandId.GetSignalCatalogChunk,
            LMC_CommandId.GetOperationStatus,
            LMC_CommandId.ReadBulkStatus,
            LMC_CommandId.ReleaseBulk
        };

        private static int rawRequestSequence;

        internal static bool IsInvocation(string[] args)
        {
            return args != null
                && args.Length > 0
                && string.Equals(
                    args[0],
                    "negative-wire",
                    StringComparison.Ordinal);
        }

        internal static int Run(string[] args)
        {
            NegativeWireOptions options;
            try
            {
                options = NegativeWireOptions.Parse(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR " + ex.Message);
                WriteUsage(Console.Error);
                return UsageExitCode;
            }

            if (options.ShowHelp)
            {
                WriteUsage(Console.Out);
                return SuccessExitCode;
            }

            var report = new NegativeWireReport(options);
            if (!options.ExecuteLive)
            {
                AppendDryRunPlan(report, options.Scenario);
                Console.Write(report.ToString());
                if (!string.IsNullOrWhiteSpace(options.OutputPath))
                {
                    try
                    {
                        report.Save(options.OutputPath);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(
                            "ERROR report write failed: " + ex.Message);
                        return ReportFailureExitCode;
                    }
                }

                return SuccessExitCode;
            }

            string inProgressReportPath = null;
            FileStream inProgressReport = null;
            try
            {
                inProgressReport = ReserveLiveReport(
                    options.OutputPath,
                    out inProgressReportPath);
                report.Add("REPORT_FINAL_PATH", options.OutputPath);
                report.Add("REPORT_IN_PROGRESS_PATH", inProgressReportPath);
                report.WriteTo(inProgressReport);
                Console.WriteLine("REPORT_IN_PROGRESS " + inProgressReportPath);
            }
            catch (Exception ex)
            {
                if (inProgressReport != null)
                {
                    inProgressReport.Dispose();
                }

                Console.Error.WriteLine(
                    "ERROR live report preflight failed before network access: "
                    + ex.Message);
                return ReportFailureExitCode;
            }

            var result = SuccessExitCode;
            try
            {
                RunLive(options, report);
                report.Add("OVERALL_RESULT", "PASS");
            }
            catch (Exception ex)
            {
                result = VerificationFailureExitCode;
                report.Add("OVERALL_RESULT", "FAIL");
                report.Add("EXCEPTION_TYPE", ex.GetType().FullName);
                report.Add("EXCEPTION_MESSAGE", ex.Message);
                Console.Error.WriteLine(ex);
            }

            try
            {
                report.Add("END_UTC", DateTime.UtcNow.ToString("O"));
                report.WriteTo(inProgressReport);
                inProgressReport.Dispose();
                inProgressReport = null;
                File.Move(inProgressReportPath, options.OutputPath);
                Console.WriteLine("REPORT " + options.OutputPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    "ERROR report finalization failed: "
                    + ex.Message
                    + ". In-progress evidence path: "
                    + inProgressReportPath);
                result = ReportFailureExitCode;
            }
            finally
            {
                if (inProgressReport != null)
                {
                    inProgressReport.Dispose();
                }
            }

            return result;
        }

        internal static byte[] CreateMalformedCatalogInfoRequest(
            uint requestId)
        {
            if (requestId == 0)
            {
                throw new ArgumentOutOfRangeException("requestId");
            }

            var request = LMC_Frame.CreateRequest(
                LMC_CommandId.GetSignalCatalogInfo,
                0,
                9);
            LMC_Frame.WriteUInt16(
                request,
                LMC_Frame.HeaderSize,
                LMC_DiagnosticsFrame.SchemaVersion);
            LMC_Frame.WriteUInt16(
                request,
                LMC_Frame.HeaderSize + 2,
                0);
            LMC_Frame.WriteUInt32(
                request,
                LMC_Frame.HeaderSize + 4,
                requestId);
            request[LMC_Frame.HeaderSize + 8] = 0;
            EnsureAllowedRawRequest(request);
            return request;
        }

        internal static byte[] CreateStaleMapRequest(
            uint requestId,
            uint currentMapRevision)
        {
            var request = LMC_DiagnosticsFrame.GetSignalCatalogChunk(
                requestId,
                MakeDifferentNonZero(currentMapRevision),
                0,
                1);
            EnsureAllowedRawRequest(request);
            return request;
        }

        internal static byte[] CreateStaleBootRequest(
            uint requestId,
            uint currentBootId)
        {
            var request = LMC_DiagnosticsFrame.GetOperationStatus(
                requestId,
                1,
                MakeDifferentNonZero(currentBootId));
            EnsureAllowedRawRequest(request);
            return request;
        }

        internal static byte[] CreateStaleConfigRequest(
            uint requestId,
            LMCBulkConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var request = LMC_DiagnosticsFrame.ReadBulkStatus(
                requestId,
                configuration.BulkId,
                MakeDifferentNonZero(configuration.ConfigRevision),
                configuration.MapRevision);
            EnsureAllowedRawRequest(request);
            return request;
        }

        internal static byte[] CreateDuplicateBulkReleaseRequest(
            uint requestId,
            uint bulkId,
            uint configRevision,
            uint mapRevision)
        {
            var request = LMC_DiagnosticsFrame.ReleaseBulk(
                requestId,
                bulkId,
                configRevision,
                mapRevision);
            EnsureAllowedRawRequest(request);
            return request;
        }

        internal static uint MakeDifferentNonZero(uint current)
        {
            if (current == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "current",
                    "A stale identity requires a current non-zero value.");
            }

            return current == uint.MaxValue ? 1u : current + 1u;
        }

        internal static void EnsureAllowedRawRequest(byte[] request)
        {
            if (request == null || request.Length < LMC_Frame.HeaderSize)
            {
                throw new InvalidDataException(
                    "Negative-wire raw request is missing its RPC header.");
            }

            var command = LMC_Frame.GetRequestCommand(request);
            var isAllowed = false;
            for (var index = 0; index < AllowedRawCommands.Length; index++)
            {
                isAllowed |= command == AllowedRawCommands[index];
            }

            if (!isAllowed)
            {
                throw new InvalidOperationException(
                    "Raw command 0x"
                    + command.ToString("X4")
                    + " is not in the negative-wire read/resource allowlist.");
            }

            var payloadLength = LMC_Frame.ReadUInt16(request, 4);
            if (request.Length != LMC_Frame.HeaderSize + payloadLength)
            {
                throw new InvalidDataException(
                    "Raw request byte length does not match its RPC header.");
            }

            var expectedPayloadLength = GetAllowedPayloadLength(command);
            if (payloadLength != expectedPayloadLength)
            {
                throw new InvalidOperationException(
                    "Raw command 0x"
                    + command.ToString("X4")
                    + " is allowed only with fixed payload length "
                    + expectedPayloadLength
                    + ".");
            }

            ValidateAllowedRequestShape(request, command);
        }

        internal static void EnsureScenarioRequest(
            byte[] request,
            NegativeWireScenario scenario,
            bool duplicateReleaseConfirmed)
        {
            EnsureAllowedRawRequest(request);

            ushort expectedCommand;
            switch (scenario)
            {
                case NegativeWireScenario.MalformedPayload:
                    expectedCommand = LMC_CommandId.GetSignalCatalogInfo;
                    break;
                case NegativeWireScenario.StaleMapRevision:
                    expectedCommand = LMC_CommandId.GetSignalCatalogChunk;
                    break;
                case NegativeWireScenario.StaleBootId:
                    expectedCommand = LMC_CommandId.GetOperationStatus;
                    break;
                case NegativeWireScenario.StaleConfigRevision:
                    expectedCommand = LMC_CommandId.ReadBulkStatus;
                    break;
                case NegativeWireScenario.DuplicateBulkRelease:
                    expectedCommand = LMC_CommandId.ReleaseBulk;
                    if (!duplicateReleaseConfirmed)
                    {
                        throw new InvalidOperationException(
                            "Duplicate Bulk release raw transport requires proof that public ReleaseBulk already completed.");
                    }

                    break;
                default:
                    throw new InvalidOperationException(
                        "A concrete negative-wire scenario is required.");
            }

            if (LMC_Frame.GetRequestCommand(request) != expectedCommand)
            {
                throw new InvalidOperationException(
                    "The raw request command does not match the selected negative-wire scenario.");
            }
        }

        internal static LMCDiagnosticsResponse ValidateNegativeResponse(
            byte[] raw,
            uint expectedRequestId,
            LMCDiagnosticsDetailCode expectedDetail)
        {
            var transport = LMCConnection.Parse(raw);
            if (!transport.IsFrameValid
                || transport.HeaderStatus != 0
                || transport.HeaderReserved != 0
                || transport.PayloadLength
                    != LMC_DiagnosticsParser.CommonResponsePayloadLength
                || transport.Payload.Length
                    != LMC_DiagnosticsParser.CommonResponsePayloadLength)
            {
                throw new InvalidDataException(
                    "Negative-wire response is not an exact 16-byte diagnostics error envelope.");
            }

            var response = LMC_DiagnosticsParser.ParseCommonResponse(
                transport,
                expectedRequestId);
            if (response.SchemaVersion != LMC_DiagnosticsFrame.SchemaVersion
                || response.ResponseFlags != LMCDiagnosticsResponseFlags.None
                || response.CommandStatus != 1
                || response.ErrorId != LMC_DiagnosticsParser.DiagnosticsErrorId
                || response.RequestId != expectedRequestId
                || response.Detail != expectedDetail
                || response.IsSuccess)
            {
                throw new InvalidDataException(
                    "Negative-wire diagnostics error does not match expected detail "
                    + (uint)expectedDetail
                    + ". Actual detail="
                    + response.DetailCode
                    + ".");
            }

            return response;
        }

        internal static void WriteUsage(TextWriter writer)
        {
            writer.WriteLine("Usage:");
            writer.WriteLine(
                "  LasalMotionControlLib.Tests.exe negative-wire --dry-run [--scenario NAME]");
            writer.WriteLine(
                "  LasalMotionControlLib.Tests.exe negative-wire --execute-live --confirm "
                + NegativeWireOptions.LiveConfirmation
                + " --host IPv4 --local IPv4 [--port 4000] [--timeout-ms 3000] --output FILE [--scenario NAME]");
            writer.WriteLine(
                "Scenarios: all, malformed-payload, stale-map, stale-boot, stale-config, duplicate-bulk-release");
            writer.WriteLine(
                "No arbitrary command or payload input is supported. Motion, Admin, PI Write, SDO Submit, and Recorder commands are unavailable.");
        }

        private static void AppendDryRunPlan(
            NegativeWireReport report,
            NegativeWireScenario scenario)
        {
            report.Add("NETWORK_CONNECTED", "FALSE");
            report.Add("LIVE_CONFIRMATION_REQUIRED",
                NegativeWireOptions.LiveConfirmation);
            foreach (var selected in ExpandScenario(scenario))
            {
                report.Add("PLANNED_SCENARIO",
                    NegativeWireOptions.GetScenarioToken(selected));
            }

            report.Add("OVERALL_RESULT", "DRY_RUN_ONLY");
        }

        private static void RunLive(
            NegativeWireOptions options,
            NegativeWireReport report)
        {
            var selected = new HashSet<NegativeWireScenario>(
                ExpandScenario(options.Scenario));
            if (selected.Contains(NegativeWireScenario.MalformedPayload))
            {
                WithConnection(
                    options,
                    report,
                    connection => RunMalformedPayload(connection, report));
            }

            selected.Remove(NegativeWireScenario.MalformedPayload);
            if (selected.Count != 0)
            {
                WithConnection(
                    options,
                    report,
                    connection => RunStateAwareScenarios(
                        connection,
                        report,
                        selected));
            }
        }

        private static void RunMalformedPayload(
            LMCConnection connection,
            NegativeWireReport report)
        {
            var requestId = NextRawRequestId();
            ExecuteExpectedFailure(
                connection,
                report,
                NegativeWireScenario.MalformedPayload,
                CreateMalformedCatalogInfoRequest(requestId),
                requestId,
                LMCDiagnosticsDetailCode.BoundsInvalid);
        }

        private static void RunStateAwareScenarios(
            LMCConnection connection,
            NegativeWireReport report,
            ISet<NegativeWireScenario> selected)
        {
            var capabilities = connection.Diagnostics.GetCapabilities();
            report.Add("CAPABILITY_BITS",
                "0x" + capabilities.CapabilityBits.ToString("X8"));
            report.Add("MAP_REVISION",
                "0x" + capabilities.MapRevision.ToString("X8"));
            report.Add("DIAGNOSTICS_BOOT_ID",
                "0x" + capabilities.DiagnosticsBootId.ToString("X8"));

            if (selected.Contains(NegativeWireScenario.StaleMapRevision))
            {
                RequireCapability(capabilities,
                    LMCDiagnosticCapability.SignalCatalog,
                    "stale-map",
                    false);
                var requestId = NextRawRequestId();
                ExecuteExpectedFailure(
                    connection,
                    report,
                    NegativeWireScenario.StaleMapRevision,
                    CreateStaleMapRequest(requestId, capabilities.MapRevision),
                    requestId,
                    LMCDiagnosticsDetailCode.MapRevisionMismatch);
            }

            if (selected.Contains(NegativeWireScenario.StaleBootId))
            {
                RequireCapability(capabilities,
                    LMCDiagnosticCapability.SDORead,
                    "stale-boot",
                    true);
                var requestId = NextRawRequestId();
                ExecuteExpectedFailure(
                    connection,
                    report,
                    NegativeWireScenario.StaleBootId,
                    CreateStaleBootRequest(
                        requestId,
                        capabilities.DiagnosticsBootId),
                    requestId,
                    LMCDiagnosticsDetailCode.BootIdMismatch);
            }

            if (selected.Contains(NegativeWireScenario.StaleConfigRevision)
                || selected.Contains(NegativeWireScenario.DuplicateBulkRelease))
            {
                RequireCapability(capabilities,
                    LMCDiagnosticCapability.BulkSnapshot,
                    "Bulk negative-wire",
                    true);
                var catalog = connection.Diagnostics.GetSignalCatalog();
                var signalId = FindBulkReadableSignal(catalog);

                if (selected.Contains(NegativeWireScenario.StaleConfigRevision))
                {
                    RunStaleConfig(connection, report, signalId);
                }

                if (selected.Contains(NegativeWireScenario.DuplicateBulkRelease))
                {
                    RunDuplicateBulkRelease(connection, report, signalId);
                }
            }
        }

        private static void RunStaleConfig(
            LMCConnection connection,
            NegativeWireReport report,
            uint signalId)
        {
            LMCBulkConfiguration configuration = null;
            Exception primary = null;
            Exception cleanup = null;
            try
            {
                configuration = connection.Diagnostics.ConfigureBulk(
                    new[] { signalId });
                AppendBulkIdentity(report, "STALE_CONFIG", configuration);
                var requestId = NextRawRequestId();
                ExecuteExpectedFailure(
                    connection,
                    report,
                    NegativeWireScenario.StaleConfigRevision,
                    CreateStaleConfigRequest(requestId, configuration),
                    requestId,
                    LMCDiagnosticsDetailCode.HandleOrGenerationStale);
            }
            catch (Exception ex)
            {
                primary = ex;
            }
            finally
            {
                if (configuration != null && !configuration.IsReleased)
                {
                    try
                    {
                        connection.Diagnostics.ReleaseBulk(configuration);
                        report.Add("STALE_CONFIG_CLEANUP", "PASS");
                    }
                    catch (Exception ex)
                    {
                        cleanup = ex;
                        report.Add("STALE_CONFIG_CLEANUP", "FAIL");
                        report.Add("STALE_CONFIG_CLEANUP_ERROR", ex.Message);
                    }
                }
            }

            ThrowPrimaryAndCleanup(primary, cleanup);
        }

        private static void RunDuplicateBulkRelease(
            LMCConnection connection,
            NegativeWireReport report,
            uint signalId)
        {
            LMCBulkConfiguration configuration = null;
            Exception primary = null;
            Exception cleanup = null;
            try
            {
                configuration = connection.Diagnostics.ConfigureBulk(
                    new[] { signalId });
                AppendBulkIdentity(report, "DUPLICATE_RELEASE", configuration);

                var bulkId = configuration.BulkId;
                var configRevision = configuration.ConfigRevision;
                var mapRevision = configuration.MapRevision;
                connection.Diagnostics.ReleaseBulk(configuration);
                if (!configuration.IsReleased)
                {
                    throw new InvalidOperationException(
                        "Public Bulk release did not mark the local handle released.");
                }

                report.Add("DUPLICATE_RELEASE_PUBLIC_RELEASE", "PASS");
                var requestId = NextRawRequestId();
                ExecuteExpectedFailure(
                    connection,
                    report,
                    NegativeWireScenario.DuplicateBulkRelease,
                    CreateDuplicateBulkReleaseRequest(
                        requestId,
                        bulkId,
                        configRevision,
                        mapRevision),
                    requestId,
                    LMCDiagnosticsDetailCode.HandleOrGenerationStale,
                    true);
            }
            catch (Exception ex)
            {
                primary = ex;
            }
            finally
            {
                if (configuration != null && !configuration.IsReleased)
                {
                    try
                    {
                        connection.Diagnostics.ReleaseBulk(configuration);
                        report.Add("DUPLICATE_RELEASE_CLEANUP_RETRY", "PASS");
                    }
                    catch (Exception ex)
                    {
                        cleanup = ex;
                        report.Add("DUPLICATE_RELEASE_CLEANUP_RETRY", "FAIL");
                        report.Add(
                            "DUPLICATE_RELEASE_CLEANUP_ERROR",
                            ex.Message);
                    }
                }
            }

            ThrowPrimaryAndCleanup(primary, cleanup);
        }

        private static void ExecuteExpectedFailure(
            LMCConnection connection,
            NegativeWireReport report,
            NegativeWireScenario scenario,
            byte[] request,
            uint requestId,
            LMCDiagnosticsDetailCode expectedDetail,
            bool duplicateReleaseConfirmed = false)
        {
            EnsureScenarioRequest(
                request,
                scenario,
                duplicateReleaseConfirmed);
            var token = NegativeWireOptions.GetScenarioToken(scenario);
            report.Add("SCENARIO_BEGIN", token);
            report.Add("COMMAND", "0x"
                + LMC_Frame.GetRequestCommand(request).ToString("X4"));
            report.Add("REQUEST_ID", "0x" + requestId.ToString("X8"));
            report.Add("EXPECTED_DETAIL", (uint)expectedDetail);
            report.AddFrame("REQUEST", request);
            try
            {
                var raw = connection.Exchange(request);
                report.AddFrame("RESPONSE", raw);
                var response = ValidateNegativeResponse(
                    raw,
                    requestId,
                    expectedDetail);
                report.Add("ACTUAL_DETAIL", response.DetailCode);
                report.Add("SCENARIO_RESULT", "PASS");
            }
            catch (Exception ex)
            {
                report.Add("SCENARIO_RESULT", "FAIL");
                report.Add("SCENARIO_ERROR", ex.Message);
                throw;
            }
            finally
            {
                report.Add("SCENARIO_END", token);
            }
        }

        private static void WithConnection(
            NegativeWireOptions options,
            NegativeWireReport report,
            Action<LMCConnection> body)
        {
            var connection = new LMCConnection(
                new LMCConnectionOptions
                {
                    ConnectTimeoutMilliseconds = options.TimeoutMilliseconds,
                    ReceiveTimeoutMilliseconds = options.TimeoutMilliseconds,
                    SendTimeoutMilliseconds = options.TimeoutMilliseconds,
                    CallbackThreadJoinTimeoutMilliseconds = 500,
                    ValidateCallbackSourceAddress = true
                });

            Exception primary = null;
            Exception cleanup = null;
            var openCompleted = false;
            try
            {
                connection.RpcInitConnection(
                    options.RemoteAddress,
                    options.RemotePort,
                    options.LocalAddress,
                    0,
                    LMCConnection.DefaultEventMask);
                openCompleted = true;
                report.Add("CONNECTION_OPEN", "PASS");
                report.Add("CALLBACK_PORT", connection.CallbackPort);
                body(connection);
            }
            catch (Exception ex)
            {
                primary = ex;
                if (!openCompleted)
                {
                    report.Add("CONNECTION_OPEN", "FAIL");
                    report.Add("CONNECTION_OPEN_ERROR", ex.Message);
                }
            }
            finally
            {
                try
                {
                    if (connection.State != LMCConnectionState.Disconnected)
                    {
                        connection.CloseConnection();
                        report.Add("CONNECTION_CLOSE", "PASS");
                    }
                    else
                    {
                        report.Add("CONNECTION_CLOSE", "NOT_REQUIRED");
                    }
                }
                catch (Exception ex)
                {
                    cleanup = ex;
                    report.Add("CONNECTION_CLOSE", "FAIL");
                    report.Add("CONNECTION_CLOSE_ERROR", ex.Message);
                }
                finally
                {
                    connection.Dispose();
                }
            }

            ThrowPrimaryAndCleanup(primary, cleanup);
        }

        private static void ThrowPrimaryAndCleanup(
            Exception primary,
            Exception cleanup)
        {
            if (primary != null && cleanup != null)
            {
                throw new AggregateException(primary, cleanup);
            }

            if (primary != null)
            {
                throw primary;
            }

            if (cleanup != null)
            {
                throw cleanup;
            }
        }

        private static void AppendBulkIdentity(
            NegativeWireReport report,
            string prefix,
            LMCBulkConfiguration configuration)
        {
            report.Add(prefix + "_BULK_ID", configuration.BulkId);
            report.Add(prefix + "_CONFIG_REVISION",
                configuration.ConfigRevision);
            report.Add(prefix + "_MAP_REVISION",
                "0x" + configuration.MapRevision.ToString("X8"));
            report.Add(prefix + "_BOOT_ID",
                "0x" + configuration.DiagnosticsBootId.ToString("X8"));
        }

        private static uint FindBulkReadableSignal(LMCSignalCatalog catalog)
        {
            foreach (var entry in catalog.Entries)
            {
                if ((entry.AccessFlags & LMCSignalAccessFlags.BulkReadable) != 0)
                {
                    return entry.SignalId;
                }
            }

            throw new InvalidOperationException(
                "The negotiated Catalog contains no BulkReadable signal.");
        }

        private static void RequireCapability(
            LMCDiagnosticCapabilities capabilities,
            LMCDiagnosticCapability capability,
            string scenario,
            bool requireBootId)
        {
            if (!capabilities.Supports(capability))
            {
                throw new NotSupportedException(
                    scenario + " requires capability " + capability + ".");
            }

            if (capabilities.MapRevision == 0
                || (requireBootId
                    && capabilities.DiagnosticsBootId == 0))
            {
                throw new InvalidDataException(
                    scenario
                    + (requireBootId
                        ? " requires stable non-zero MapRevision and DiagnosticsBootId."
                        : " requires a stable non-zero MapRevision."));
            }
        }

        private static IEnumerable<NegativeWireScenario> ExpandScenario(
            NegativeWireScenario scenario)
        {
            if (scenario != NegativeWireScenario.All)
            {
                yield return scenario;
                yield break;
            }

            yield return NegativeWireScenario.MalformedPayload;
            yield return NegativeWireScenario.StaleMapRevision;
            yield return NegativeWireScenario.StaleBootId;
            yield return NegativeWireScenario.StaleConfigRevision;
            yield return NegativeWireScenario.DuplicateBulkRelease;
        }

        private static uint NextRawRequestId()
        {
            var next = unchecked(++rawRequestSequence);
            var low = unchecked((uint)next) & 0x0000FFFFu;
            return 0x4E570000u | (low == 0 ? 1u : low);
        }

        private static ushort GetAllowedPayloadLength(ushort command)
        {
            switch (command)
            {
                case LMC_CommandId.GetSignalCatalogInfo:
                    return 9;
                case LMC_CommandId.GetSignalCatalogChunk:
                case LMC_CommandId.GetOperationStatus:
                    return 16;
                case LMC_CommandId.ReadBulkStatus:
                case LMC_CommandId.ReleaseBulk:
                    return 20;
                default:
                    throw new InvalidOperationException(
                        "Command is not in the fixed negative-wire allowlist.");
            }
        }

        private static void ValidateAllowedRequestShape(
            byte[] request,
            ushort command)
        {
            var payloadOffset = LMC_Frame.HeaderSize;
            if (LMC_Frame.ReadUInt16(request, 2) != 0
                || LMC_Frame.ReadUInt16(request, 6) != 0
                || LMC_Frame.ReadUInt16(request, payloadOffset)
                    != LMC_DiagnosticsFrame.SchemaVersion
                || LMC_Frame.ReadUInt16(request, payloadOffset + 2) != 0
                || LMC_Frame.ReadUInt32(request, payloadOffset + 4) == 0)
            {
                throw new InvalidOperationException(
                    "Negative-wire requests require zero header reserved/reference fields, schema version 1, zero flags, and a non-zero request ID.");
            }

            switch (command)
            {
                case LMC_CommandId.GetSignalCatalogInfo:
                    if (request[payloadOffset + 8] != 0)
                    {
                        throw new InvalidOperationException(
                            "Malformed CatalogInfo permits only one trailing zero byte beyond the normal common request.");
                    }

                    break;
                case LMC_CommandId.GetSignalCatalogChunk:
                    if (LMC_Frame.ReadUInt32(request, payloadOffset + 8) == 0
                        || LMC_Frame.ReadUInt16(request, payloadOffset + 12) != 0
                        || LMC_Frame.ReadUInt16(request, payloadOffset + 14) != 1)
                    {
                        throw new InvalidOperationException(
                            "Stale MapRevision permits only a non-zero revision and Catalog range 0..1.");
                    }

                    break;
                case LMC_CommandId.GetOperationStatus:
                    if (LMC_Frame.ReadUInt32(request, payloadOffset + 8) != 1
                        || LMC_Frame.ReadUInt32(request, payloadOffset + 12) == 0)
                    {
                        throw new InvalidOperationException(
                            "Stale BootId permits only TicketId 1 and a non-zero stale DiagnosticsBootId.");
                    }

                    break;
                case LMC_CommandId.ReadBulkStatus:
                case LMC_CommandId.ReleaseBulk:
                    if (LMC_Frame.ReadUInt32(request, payloadOffset + 8) == 0
                        || LMC_Frame.ReadUInt32(request, payloadOffset + 12) == 0
                        || LMC_Frame.ReadUInt32(request, payloadOffset + 16) == 0)
                    {
                        throw new InvalidOperationException(
                            "Bulk negative-wire requests require non-zero BulkId, ConfigRevision, and MapRevision.");
                    }

                    break;
            }
        }

        internal static FileStream ReserveLiveReport(
            string outputPath,
            out string inProgressPath)
        {
            var fullPath = Path.GetFullPath(outputPath);
            if (File.Exists(fullPath) || Directory.Exists(fullPath))
            {
                throw new IOException(
                    "The live report target already exists and will not be overwritten: "
                    + fullPath);
            }

            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            inProgressPath = fullPath
                + ".inprogress-"
                + Guid.NewGuid().ToString("N")
                + ".tmp";
            return new FileStream(
                inProgressPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read);
        }
    }
}

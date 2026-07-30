using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LasalMotionControlLib.Tests
{
    internal enum TopologyIoQualificationScope
    {
        IntegratedReadOwnerDormant = 0,
        TopologyInventory = 1
    }

    internal sealed class TopologyIoQualificationOptions
    {
        internal const string LiveConfirmation =
            "PLC-RAW-TOPOLOGY-IO-READ";
        internal const string TopologyInventoryLiveConfirmation =
            "PLC-RAW-TOPOLOGY-INVENTORY-READ";
        internal const string IntegratedReadOwnerDormantScope =
            "integrated-read-owner-dormant";
        internal const string TopologyInventoryScope =
            "topology-inventory";

        internal TopologyIoQualificationOptions()
        {
            RemotePort = 4000;
            TimeoutMilliseconds = 3000;
            Scope = TopologyIoQualificationScope
                .IntegratedReadOwnerDormant;
        }

        internal bool ExecuteLive { get; private set; }
        internal bool ShowHelp { get; private set; }
        internal TopologyIoQualificationScope Scope { get; private set; }
        internal bool ScopeWasExplicit { get; private set; }
        internal string RemoteAddress { get; private set; }
        internal int RemotePort { get; private set; }
        internal string LocalAddress { get; private set; }
        internal int TimeoutMilliseconds { get; private set; }
        internal string OutputPath { get; private set; }
        internal string Confirmation { get; private set; }

        internal static TopologyIoQualificationOptions Parse(string[] args)
        {
            if (args == null
                || args.Length == 0
                || !string.Equals(
                    args[0],
                    "topology-io-qualify",
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The first argument must be the exact token 'topology-io-qualify'.");
            }

            var options = new TopologyIoQualificationOptions();
            var sawDryRun = false;
            for (var index = 1; index < args.Length; index++)
            {
                var argument = args[index];
                if (string.Equals(argument, "--help", StringComparison.Ordinal))
                {
                    options.ShowHelp = true;
                }
                else if (string.Equals(
                    argument,
                    "--dry-run",
                    StringComparison.Ordinal))
                {
                    sawDryRun = true;
                }
                else if (string.Equals(
                    argument,
                    "--execute-live",
                    StringComparison.Ordinal))
                {
                    options.ExecuteLive = true;
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
                else if (string.Equals(
                    argument,
                    "--timeout-ms",
                    StringComparison.Ordinal))
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
                else if (string.Equals(argument, "--scope", StringComparison.Ordinal))
                {
                    if (options.ScopeWasExplicit)
                    {
                        throw new ArgumentException(
                            "--scope may be specified exactly once.");
                    }

                    options.Scope = ParseScope(
                        ReadValue(args, ref index, argument));
                    options.ScopeWasExplicit = true;
                }
                else
                {
                    throw new ArgumentException(
                        "Unknown topology-io-qualify argument '"
                        + argument
                        + "'.");
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

        private static void RequireLiveOptions(
            TopologyIoQualificationOptions options)
        {
            var expectedConfirmation = options.Scope
                    == TopologyIoQualificationScope.TopologyInventory
                ? TopologyInventoryLiveConfirmation
                : LiveConfirmation;
            if (!string.Equals(
                options.Confirmation,
                expectedConfirmation,
                StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Live execution requires --confirm "
                    + expectedConfirmation
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

        internal static string GetScopeToken(
            TopologyIoQualificationScope scope)
        {
            switch (scope)
            {
                case TopologyIoQualificationScope.TopologyInventory:
                    return TopologyInventoryScope;
                case TopologyIoQualificationScope.IntegratedReadOwnerDormant:
                    return IntegratedReadOwnerDormantScope;
                default:
                    throw new ArgumentOutOfRangeException("scope");
            }
        }

        private static TopologyIoQualificationScope ParseScope(string value)
        {
            if (string.Equals(
                value,
                TopologyInventoryScope,
                StringComparison.Ordinal))
            {
                return TopologyIoQualificationScope.TopologyInventory;
            }

            if (string.Equals(
                value,
                IntegratedReadOwnerDormantScope,
                StringComparison.Ordinal))
            {
                return TopologyIoQualificationScope
                    .IntegratedReadOwnerDormant;
            }

            throw new ArgumentException(
                "--scope must be exactly '"
                + TopologyInventoryScope
                + "' or '"
                + IntegratedReadOwnerDormantScope
                + "'.");
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
            if (string.IsNullOrWhiteSpace(args[index]))
            {
                throw new ArgumentException(
                    option + " requires a non-empty value.");
            }

            return args[index];
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
    }

    internal sealed class TopologyIoQualificationReportException : IOException
    {
        internal TopologyIoQualificationReportException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }

    internal sealed class TopologyIoQualificationReport
    {
        private readonly StringBuilder text = new StringBuilder();
        private Stream checkpointStream;

        internal TopologyIoQualificationReport(
            TopologyIoQualificationOptions options)
        {
            Add("FORMAT", "LMC_TOPOLOGY_IO_QUALIFICATION_V1");
            Add("START_UTC", DateTime.UtcNow.ToString("O"));
            Add("MODE", options.ExecuteLive ? "LIVE" : "DRY_RUN");
            Add(
                "QUALIFICATION_SCOPE",
                options.Scope == TopologyIoQualificationScope
                        .TopologyInventory
                    ? "TOPOLOGY_INVENTORY"
                    : "INTEGRATED_READ_OWNER_DORMANT");
            Add(
                "CLI_SCOPE",
                TopologyIoQualificationOptions.GetScopeToken(
                    options.Scope));
            Add(
                "CLI_SCOPE_EXPLICIT",
                options.ScopeWasExplicit ? "TRUE" : "FALSE");
            Add("CAPABILITY_PROMOTION", "NOT_PERFORMED_BY_TOOL");
            Add("PHYSICAL_STATE_CORRELATION", "REQUIRED_OUTSIDE_TOOL");
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
            var line = key + "=" + rendered + Environment.NewLine;
            text.Append(line);
            AppendCheckpoint(line);
        }

        internal void AddFrame(string prefix, byte[] frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }

            Add(prefix + "_BYTES", frame.Length);
            Add(prefix + "_SHA256", NegativeWireReport.ComputeSha256(frame));
            Add(prefix + "_HEX", NegativeWireReport.ToHex(frame));
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

        internal bool CheckpointFailed { get; private set; }

        internal void AttachCheckpointStream(Stream stream)
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

            if (checkpointStream != null)
            {
                throw new InvalidOperationException(
                    "A live report checkpoint stream is already attached.");
            }

            if (stream.Position != 0 || stream.Length != 0)
            {
                throw new ArgumentException(
                    "The live report checkpoint stream must be a new empty file.",
                    "stream");
            }

            try
            {
                WriteAndFlush(stream, text.ToString());
                checkpointStream = stream;
            }
            catch (Exception ex)
            {
                CheckpointFailed = true;
                throw new TopologyIoQualificationReportException(
                    "Initial live report checkpoint failed.",
                    ex);
            }
        }

        internal void DetachCheckpointStream()
        {
            checkpointStream = null;
        }

        public override string ToString()
        {
            return text.ToString();
        }

        private void AppendCheckpoint(string line)
        {
            if (checkpointStream == null)
            {
                return;
            }

            try
            {
                WriteAndFlush(checkpointStream, line);
            }
            catch (Exception ex)
            {
                CheckpointFailed = true;
                checkpointStream = null;
                throw new TopologyIoQualificationReportException(
                    "Live report checkpoint append failed.",
                    ex);
            }
        }

        private static void WriteAndFlush(Stream stream, string value)
        {
            var bytes = new UTF8Encoding(false).GetBytes(value);
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
    }

    internal sealed class TopologyIoQualificationResult
    {
        internal TopologyIoQualificationResult(
            LMCEtherCATTopology topology,
            IList<LMCEtherCATNodeHealth> health,
            IList<LMCDigitalIOValue> digitalIo)
        {
            Topology = topology;
            Health = new List<LMCEtherCATNodeHealth>(health).AsReadOnly();
            DigitalIo = new List<LMCDigitalIOValue>(digitalIo).AsReadOnly();
        }

        internal LMCEtherCATTopology Topology { get; private set; }
        internal IReadOnlyList<LMCEtherCATNodeHealth> Health
        {
            get;
            private set;
        }

        internal IReadOnlyList<LMCDigitalIOValue> DigitalIo
        {
            get;
            private set;
        }
    }

    internal static class TopologyIoQualificationTool
    {
        internal const int SuccessExitCode = 0;
        internal const int UsageExitCode = 2;
        internal const int VerificationFailureExitCode = 3;
        internal const int ReportFailureExitCode = 4;
        internal const uint ExpectedTopologyRevision = 0x15867EECu;

        private static readonly ushort[] TopologyInventoryRawCommands =
        {
            LMC_CommandId.GetEtherCATTopologyInfo,
            LMC_CommandId.GetEtherCATTopologyChunk
        };

        private static readonly ushort[] IntegratedRawCommands =
        {
            LMC_CommandId.GetEtherCATTopologyInfo,
            LMC_CommandId.GetEtherCATTopologyChunk,
            LMC_CommandId.ReadEtherCATNodeHealth,
            LMC_CommandId.ReadDigitalIO
        };

        private static int rawRequestSequence;

        internal static bool IsInvocation(string[] args)
        {
            return args != null
                && args.Length > 0
                && string.Equals(
                    args[0],
                    "topology-io-qualify",
                    StringComparison.Ordinal);
        }

        internal static int Run(string[] args)
        {
            TopologyIoQualificationOptions options;
            try
            {
                options = TopologyIoQualificationOptions.Parse(args);
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

            var report = new TopologyIoQualificationReport(options);
            if (!options.ExecuteLive)
            {
                AppendDryRunPlan(options, report);
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

            string inProgressPath = null;
            FileStream inProgressReport = null;
            try
            {
                inProgressReport = NegativeWireTool.ReserveLiveReport(
                    options.OutputPath,
                    out inProgressPath);
                report.Add("REPORT_FINAL_PATH", options.OutputPath);
                report.Add("REPORT_IN_PROGRESS_PATH", inProgressPath);
                report.AttachCheckpointStream(inProgressReport);
                Console.WriteLine("REPORT_IN_PROGRESS " + inProgressPath);
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
                result = report.CheckpointFailed
                    ? ReportFailureExitCode
                    : VerificationFailureExitCode;
                if (!report.CheckpointFailed)
                {
                    try
                    {
                        report.Add("OVERALL_RESULT", "FAIL");
                        report.Add("EXCEPTION_TYPE", ex.GetType().FullName);
                        report.Add("EXCEPTION_MESSAGE", ex.Message);
                    }
                    catch (TopologyIoQualificationReportException reportEx)
                    {
                        result = ReportFailureExitCode;
                        Console.Error.WriteLine(reportEx);
                    }
                }
                Console.Error.WriteLine(ex);
            }

            if (report.CheckpointFailed)
            {
                inProgressReport.Dispose();
                inProgressReport = null;
                Console.Error.WriteLine(
                    "ERROR live report checkpoint failed. Partial evidence remains at: "
                    + inProgressPath);
                return ReportFailureExitCode;
            }

            try
            {
                report.Add("END_UTC", DateTime.UtcNow.ToString("O"));
                report.DetachCheckpointStream();
                inProgressReport.Dispose();
                inProgressReport = null;
                File.Move(inProgressPath, options.OutputPath);
                Console.WriteLine("REPORT " + options.OutputPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    "ERROR report finalization failed: "
                    + ex.Message
                    + ". In-progress evidence path: "
                    + inProgressPath);
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

        internal static void EnsureAllowedReadOnlyRequest(byte[] request)
        {
            EnsureAllowedReadOnlyRequest(
                request,
                TopologyIoQualificationScope.IntegratedReadOwnerDormant);
        }

        internal static void EnsureAllowedReadOnlyRequest(
            byte[] request,
            TopologyIoQualificationScope scope)
        {
            var scopeToken =
                TopologyIoQualificationOptions.GetScopeToken(scope);
            if (request == null || request.Length < LMC_Frame.HeaderSize)
            {
                throw new InvalidDataException(
                    "Topology/I/O raw request is missing its RPC header.");
            }

            var command = LMC_Frame.GetRequestCommand(request);
            var allowedCommands = scope
                    == TopologyIoQualificationScope.TopologyInventory
                ? TopologyInventoryRawCommands
                : IntegratedRawCommands;
            var allowed = false;
            for (var index = 0; index < allowedCommands.Length; index++)
            {
                allowed |= command == allowedCommands[index];
            }

            if (!allowed)
            {
                throw new InvalidOperationException(
                    "Raw command 0x"
                    + command.ToString("X4")
                    + " is not in the "
                    + scopeToken
                    + " read-only allowlist.");
            }

            var payloadLength = LMC_Frame.ReadUInt16(request, 4);
            if (request.Length != LMC_Frame.HeaderSize + payloadLength)
            {
                throw new InvalidDataException(
                    "Raw request byte length does not match its RPC header.");
            }

            if (payloadLength != GetAllowedPayloadLength(command))
            {
                throw new InvalidOperationException(
                    "Raw command 0x"
                    + command.ToString("X4")
                    + " has a non-canonical payload length.");
            }

            ValidateAllowedRequestShape(request, command);
        }

        internal static void ValidateTopologyInventoryCapabilities(
            LMCDiagnosticCapabilities capabilities)
        {
            ValidateTopologyCapabilityBase(
                capabilities,
                LMC_DiagnosticsFrame.TopologyChunkRequestPayloadLength,
                "Topology-inventory qualification");
        }

        internal static void ValidateDormantCapabilities(
            LMCDiagnosticCapabilities capabilities)
        {
            ValidateTopologyCapabilityBase(
                capabilities,
                LMC_DiagnosticsFrame.DigitalIOReadRequestPayloadLength,
                "Dormant qualification");

            var forbidden = LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead
                | LMCDiagnosticCapability.DigitalIOWrite;
            if ((capabilities.Capabilities & forbidden) != 0)
            {
                throw new InvalidOperationException(
                    "Dormant qualification requires capability bits 15, 16, and 17 to remain off.");
            }

        }

        private static void ValidateTopologyCapabilityBase(
            LMCDiagnosticCapabilities capabilities,
            ushort minimumRequestPayloadBytes,
            string operation)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            if (!capabilities.Supports(
                LMCDiagnosticCapability.EtherCATTopology))
            {
                throw new NotSupportedException(
                    operation + " requires EtherCATTopology bit 14.");
            }

            if (!capabilities.HasStableDiagnosticsBootId)
            {
                throw new InvalidDataException(
                    operation
                    + " requires a non-zero DiagnosticsBootId for evidence identity.");
            }

            if (capabilities.MaxRequestPayloadBytes
                    < minimumRequestPayloadBytes
                || capabilities.MaxResponsePayloadBytes
                    < LMC_DiagnosticsParser.TopologyChunkHeaderPayloadLength
                        + LMC_DiagnosticsParser.TopologyEntryStride)
            {
                throw new InvalidDataException(
                    "Negotiated diagnostics payload limits cannot carry the "
                    + operation
                    + " frames.");
            }
        }

        private static void RequireQualificationDelegates(
            Func<LMCDiagnosticCapabilities> readCapabilities,
            Func<byte[], byte[]> exchange,
            TopologyIoQualificationReport report)
        {
            if (readCapabilities == null)
            {
                throw new ArgumentNullException("readCapabilities");
            }

            if (exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }

            if (report == null)
            {
                throw new ArgumentNullException("report");
            }
        }

        private static void AddCapabilityIdentityBefore(
            TopologyIoQualificationReport report,
            LMCDiagnosticCapabilities capabilities)
        {
            report.Add("CAPABILITY_BITS", Hex(capabilities.CapabilityBits));
            report.Add(
                "DIAGNOSTICS_BOOT_ID",
                Hex(capabilities.DiagnosticsBootId));
            report.Add(
                "CAPABILITY_BITS_BEFORE",
                Hex(capabilities.CapabilityBits));
            report.Add(
                "DIAGNOSTICS_BOOT_ID_BEFORE",
                Hex(capabilities.DiagnosticsBootId));
        }

        private static void AddCapabilityIdentityAfter(
            TopologyIoQualificationReport report,
            LMCDiagnosticCapabilities capabilities)
        {
            report.Add(
                "CAPABILITY_BITS_AFTER",
                Hex(capabilities.CapabilityBits));
            report.Add(
                "DIAGNOSTICS_BOOT_ID_AFTER",
                Hex(capabilities.DiagnosticsBootId));
        }

        private static void RequireCapabilityIdentityUnchanged(
            LMCDiagnosticCapabilities before,
            LMCDiagnosticCapabilities after,
            string operation)
        {
            if (after.DiagnosticsBootId != before.DiagnosticsBootId
                || after.DiagnosticsBuild != before.DiagnosticsBuild
                || after.CapabilityBits != before.CapabilityBits
                || after.MapRevision != before.MapRevision
                || after.MaxRequestPayloadBytes
                    != before.MaxRequestPayloadBytes
                || after.MaxResponsePayloadBytes
                    != before.MaxResponsePayloadBytes)
            {
                throw new InvalidDataException(
                    "Diagnostics capability identity changed during the "
                    + operation
                    + ".");
            }
        }

        internal static TopologyIoQualificationResult
            RunTopologyInventoryRawWithCapabilityIdentity(
                Func<LMCDiagnosticCapabilities> readCapabilities,
                Func<byte[], byte[]> exchange,
                TopologyIoQualificationReport report)
        {
            RequireQualificationDelegates(
                readCapabilities,
                exchange,
                report);

            var before = readCapabilities();
            ValidateTopologyInventoryCapabilities(before);
            AddCapabilityIdentityBefore(report, before);
            report.Add("TOPOLOGY_CAPABILITY_PRECONDITION", "PASS");

            var result = RunTopologyInventoryRaw(exchange, report);

            var after = readCapabilities();
            ValidateTopologyInventoryCapabilities(after);
            AddCapabilityIdentityAfter(report, after);
            RequireCapabilityIdentityUnchanged(
                before,
                after,
                "raw topology inventory");

            report.Add("TOPOLOGY_CAPABILITY_POSTCONDITION", "PASS");
            report.Add("CAPABILITY_IDENTITY_RESULT", "PASS");
            return result;
        }

        internal static TopologyIoQualificationResult
            RunReadOnlyRawWithCapabilityIdentity(
                Func<LMCDiagnosticCapabilities> readCapabilities,
                Func<byte[], byte[]> exchange,
                TopologyIoQualificationReport report)
        {
            RequireQualificationDelegates(
                readCapabilities,
                exchange,
                report);

            var before = readCapabilities();
            ValidateDormantCapabilities(before);
            AddCapabilityIdentityBefore(report, before);
            report.Add("DORMANT_CAPABILITY_PRECONDITION", "PASS");

            var result = RunReadOnlyRaw(exchange, report);

            var after = readCapabilities();
            ValidateDormantCapabilities(after);
            AddCapabilityIdentityAfter(report, after);
            RequireCapabilityIdentityUnchanged(
                before,
                after,
                "raw topology/I/O snapshot");

            report.Add("DORMANT_CAPABILITY_POSTCONDITION", "PASS");
            report.Add("CAPABILITY_IDENTITY_RESULT", "PASS");
            return result;
        }

        internal static TopologyIoQualificationResult RunReadOnlyRaw(
            Func<byte[], byte[]> exchange,
            TopologyIoQualificationReport report)
        {
            if (exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }

            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            var topology = ReadExpectedTopology(
                exchange,
                report,
                TopologyIoQualificationScope.IntegratedReadOwnerDormant);
            var info = topology.Info;
            var entries = topology.Entries;

            var healthValues = new List<LMCEtherCATNodeHealth>(entries.Count);
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                var requestId = NextRawRequestId();
                var prefix = "NODE_HEALTH_"
                    + index.ToString("D2", CultureInfo.InvariantCulture);
                var raw = ExchangeReadOnly(
                    exchange,
                    report,
                    prefix,
                    LMC_DiagnosticsFrame.ReadEtherCATNodeHealth(
                        requestId,
                        info.TopologyRevision,
                        entry.NodeId),
                    TopologyIoQualificationScope
                        .IntegratedReadOwnerDormant);
                var health = LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                    raw,
                    requestId,
                    info.TopologyRevision,
                    entry.NodeId);
                topology.ValidateNodeHealth(health);
                healthValues.Add(health);
                AddHealthToReport(report, prefix, health);
            }

            var ioValues = new List<LMCDigitalIOValue>();
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry.InputBytes != 0)
                {
                    ReadDigitalIo(
                        exchange,
                        report,
                        topology,
                        entry,
                        LMCDigitalIODirection.Input,
                        entry.InputBytes,
                        ioValues);
                }

                if (entry.OutputBytes != 0)
                {
                    ReadDigitalIo(
                        exchange,
                        report,
                        topology,
                        entry,
                        LMCDigitalIODirection.Output,
                        entry.OutputBytes,
                        ioValues);
                }
            }

            if (ioValues.Count != 2)
            {
                throw new InvalidDataException(
                    "The current CREVIS topology must produce exactly one input and one output read.");
            }

            report.Add("RAW_NODE_HEALTH_COUNT", healthValues.Count);
            report.Add("RAW_DIGITAL_IO_COUNT", ioValues.Count);
            report.Add("RAW_SCHEMA_RESULT", "PASS");
            report.Add("LIVE_GATE_RESULT", "REQUIRES_PHYSICAL_CORRELATION");
            return new TopologyIoQualificationResult(
                topology,
                healthValues,
                ioValues);
        }

        internal static TopologyIoQualificationResult
            RunTopologyInventoryRaw(
                Func<byte[], byte[]> exchange,
                TopologyIoQualificationReport report)
        {
            if (exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }

            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            var topology = ReadExpectedTopology(
                exchange,
                report,
                TopologyIoQualificationScope.TopologyInventory);
            report.Add("RAW_NODE_HEALTH_COUNT", 0);
            report.Add("RAW_DIGITAL_IO_COUNT", 0);
            report.Add("RAW_TOPOLOGY_REQUEST_COUNT", 8);
            report.Add("RAW_SCHEMA_RESULT", "PASS");
            report.Add("LIVE_GATE_RESULT", "STATIC_TOPOLOGY_ONLY");
            return new TopologyIoQualificationResult(
                topology,
                new LMCEtherCATNodeHealth[0],
                new LMCDigitalIOValue[0]);
        }

        internal static void WriteUsage(TextWriter writer)
        {
            writer.WriteLine("Usage:");
            writer.WriteLine(
                "  LasalMotionControlLib.Tests.exe topology-io-qualify --scope "
                + TopologyIoQualificationOptions.TopologyInventoryScope
                + " --dry-run [--output FILE]");
            writer.WriteLine(
                "  LasalMotionControlLib.Tests.exe topology-io-qualify --scope "
                + TopologyIoQualificationOptions.TopologyInventoryScope
                + " --execute-live --confirm "
                + TopologyIoQualificationOptions
                    .TopologyInventoryLiveConfirmation
                + " --host IPv4 --local IPv4 [--port 4000] [--timeout-ms 3000] --output FILE");
            writer.WriteLine(
                "  LasalMotionControlLib.Tests.exe topology-io-qualify --scope "
                + TopologyIoQualificationOptions
                    .IntegratedReadOwnerDormantScope
                + " --execute-live --confirm "
                + TopologyIoQualificationOptions.LiveConfirmation
                + " --host IPv4 --local IPv4 [--port 4000] [--timeout-ms 3000] --output FILE");
            writer.WriteLine(
                "Topology-inventory allowlist: 0x7E11, 0x7E12 only. Integrated dormant allowlist: 0x7E11, 0x7E12, 0x7E13, 0x7E22. 0x7E23 and all mutation commands are forbidden.");
        }

        private static void RunLive(
            TopologyIoQualificationOptions options,
            TopologyIoQualificationReport report)
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
            var opened = false;
            try
            {
                connection.RpcInitConnection(
                    options.RemoteAddress,
                    options.RemotePort,
                    options.LocalAddress,
                    0,
                    LMCConnection.DefaultEventMask);
                opened = true;
                report.Add("CONNECTION_OPEN", "PASS");
                report.Add("CALLBACK_PORT", connection.CallbackPort);

                var generation = connection.SessionGeneration;
                if (options.Scope
                    == TopologyIoQualificationScope.TopologyInventory)
                {
                    RunTopologyInventoryRawWithCapabilityIdentity(
                        () => connection.Diagnostics.GetCapabilities(),
                        request => connection.Exchange(request, generation),
                        report);
                }
                else
                {
                    RunReadOnlyRawWithCapabilityIdentity(
                        () => connection.Diagnostics.GetCapabilities(),
                        request => connection.Exchange(request, generation),
                        report);
                }
                connection.EnsureSessionGeneration(generation);
            }
            catch (Exception ex)
            {
                primary = ex;
                if (!opened)
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

        private static void AppendDryRunPlan(
            TopologyIoQualificationOptions options,
            TopologyIoQualificationReport report)
        {
            report.Add("NETWORK_CONNECTED", "FALSE");
            report.Add(
                "RAW_ALLOWLIST",
                options.Scope == TopologyIoQualificationScope
                        .TopologyInventory
                    ? "0x7E11,0x7E12"
                    : "0x7E11,0x7E12,0x7E13,0x7E22");
            report.Add("RAW_READ_0x7E13",
                options.Scope == TopologyIoQualificationScope
                        .TopologyInventory
                    ? "FORBIDDEN"
                    : "ALLOWED");
            report.Add("RAW_READ_0x7E22",
                options.Scope == TopologyIoQualificationScope
                        .TopologyInventory
                    ? "FORBIDDEN"
                    : "ALLOWED");
            report.Add("RAW_WRITE_0x7E23", "FORBIDDEN");
            report.Add("DRY_RUN_RESULT", "NO_NETWORK_IO");

            var sampleRequests = options.Scope
                    == TopologyIoQualificationScope.TopologyInventory
                ? new[]
                {
                    LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(
                        0x54490001u),
                    LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                        0x54490002u,
                        ExpectedTopologyRevision,
                        0,
                        1)
                }
                : new[]
            {
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(0x54490001u),
                LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    0x54490002u,
                    ExpectedTopologyRevision,
                    0,
                    1),
                LMC_DiagnosticsFrame.ReadEtherCATNodeHealth(
                    0x54490003u,
                    ExpectedTopologyRevision,
                    0xEC000001u),
                LMC_DiagnosticsFrame.ReadDigitalIO(
                    0x54490004u,
                    new LMCDigitalIOReadRequest(
                        ExpectedTopologyRevision,
                        0x00010001u,
                        LMCDigitalIODirection.Input,
                        32)),
                LMC_DiagnosticsFrame.ReadDigitalIO(
                    0x54490005u,
                    new LMCDigitalIOReadRequest(
                        ExpectedTopologyRevision,
                        0x00010002u,
                        LMCDigitalIODirection.Output,
                        32))
            };
            for (var index = 0; index < sampleRequests.Length; index++)
            {
                EnsureAllowedReadOnlyRequest(
                    sampleRequests[index],
                    options.Scope);
                report.AddFrame(
                    "SAMPLE_REQUEST_"
                        + index.ToString("D2", CultureInfo.InvariantCulture),
                    sampleRequests[index]);
            }
        }

        private static byte[] ExchangeReadOnly(
            Func<byte[], byte[]> exchange,
            TopologyIoQualificationReport report,
            string prefix,
            byte[] request,
            TopologyIoQualificationScope scope)
        {
            EnsureAllowedReadOnlyRequest(request, scope);
            report.AddFrame(prefix + "_REQUEST", request);
            var response = exchange(request);
            if (response == null)
            {
                throw new InvalidDataException(
                    prefix + " returned a null response.");
            }

            report.AddFrame(prefix + "_RESPONSE", response);
            return response;
        }

        private static LMCEtherCATTopology ReadExpectedTopology(
            Func<byte[], byte[]> exchange,
            TopologyIoQualificationReport report,
            TopologyIoQualificationScope scope)
        {
            var infoRequestId = NextRawRequestId();
            var infoRaw = ExchangeReadOnly(
                exchange,
                report,
                "TOPOLOGY_INFO",
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(infoRequestId),
                scope);
            var info = LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                infoRaw,
                infoRequestId);
            ValidateExpectedCurrentTopologyInfo(info);
            report.Add("TOPOLOGY_REVISION", Hex(info.TopologyRevision));
            report.Add("TOPOLOGY_NODE_COUNT", info.TotalNodeCount);
            report.Add(
                "TOPOLOGY_MAX_ENTRIES_PER_CHUNK",
                info.MaxEntriesPerChunk);

            var entries = new List<LMCEtherCATTopologyEntry>(
                info.TotalNodeCount);
            while (entries.Count < info.TotalNodeCount)
            {
                var startIndex = checked((ushort)entries.Count);
                var requestId = NextRawRequestId();
                var prefix = "TOPOLOGY_CHUNK_"
                    + startIndex.ToString("D2", CultureInfo.InvariantCulture);
                var raw = ExchangeReadOnly(
                    exchange,
                    report,
                    prefix,
                    LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                        requestId,
                        info.TopologyRevision,
                        startIndex,
                        info.MaxEntriesPerChunk),
                    scope);
                var chunk = LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                    raw,
                    requestId,
                    info.TopologyRevision,
                    startIndex,
                    info.MaxEntriesPerChunk);
                if (chunk.TotalNodeCount != info.TotalNodeCount
                    || chunk.EntryStride != info.EntryStride
                    || chunk.ReturnedCount == 0)
                {
                    throw new InvalidDataException(
                        "Topology chunk identity changed or made no progress during the raw snapshot.");
                }

                foreach (var entry in chunk.Entries)
                {
                    entries.Add(entry);
                }
            }

            LMCDiagnostics.ValidateCompleteTopology(info, entries);
            var topology = new LMCEtherCATTopology(info, entries);
            ValidateExpectedCurrentTopology(topology);
            AddTopologyToReport(report, topology);
            return topology;
        }

        private static void ReadDigitalIo(
            Func<byte[], byte[]> exchange,
            TopologyIoQualificationReport report,
            LMCEtherCATTopology topology,
            LMCEtherCATTopologyEntry entry,
            LMCDigitalIODirection direction,
            ushort byteWidth,
            IList<LMCDigitalIOValue> values)
        {
            if (entry.IOReference == 0 || byteWidth > 8)
            {
                throw new InvalidDataException(
                    "Digital I/O topology entry has an invalid reference or width.");
            }

            var bitWidth = checked((byte)(byteWidth * 8));
            var request = new LMCDigitalIOReadRequest(
                ExpectedTopologyRevision,
                entry.IOReference,
                direction,
                bitWidth);
            topology.ValidateDigitalIOReadRequest(request);
            var requestId = NextRawRequestId();
            var prefix = direction == LMCDigitalIODirection.Input
                ? "DIGITAL_INPUT"
                : "DIGITAL_OUTPUT_SHADOW";
            var raw = ExchangeReadOnly(
                exchange,
                report,
                prefix,
                LMC_DiagnosticsFrame.ReadDigitalIO(requestId, request),
                TopologyIoQualificationScope
                    .IntegratedReadOwnerDormant);
            var value = LMC_DiagnosticsParser.ParseDigitalIO(
                raw,
                requestId,
                request);
            topology.ValidateDigitalIOValue(value);

            values.Add(value);
            report.Add(prefix + "_IO_REFERENCE", Hex(value.IOReference));
            report.Add(prefix + "_NODE_ID", Hex(value.NodeId));
            report.Add(prefix + "_STATUS_FLAGS", Hex((uint)value.StatusFlags));
            report.Add(prefix + "_VALUE", Hex(value.Value));
            report.Add(prefix + "_VALID_MASK", Hex(value.ValidMask));
            report.Add(prefix + "_CYCLE", value.CycleCounter);
            report.Add(prefix + "_OUTPUT_REVISION",
                Hex(value.OutputRevision));
        }

        private static void ValidateExpectedCurrentTopology(
            LMCEtherCATTopology topology)
        {
            if (topology == null)
            {
                throw new ArgumentNullException("topology");
            }

            ValidateExpectedCurrentTopologyInfo(topology.Info);
            var expected = ExpectedCurrentTopologyEntries();
            if (topology.Entries.Count != expected.Length)
            {
                throw new InvalidDataException(
                    "Downloaded topology does not match the current 5-slave/7-entry CREVIS contract.");
            }

            for (var index = 0; index < expected.Length; index++)
            {
                RequireEntryMatch(expected[index], topology.Entries[index]);
            }
        }

        private static void ValidateExpectedCurrentTopologyInfo(
            LMCEtherCATTopologyInfo info)
        {
            if (info == null
                || info.TopologyRevision != ExpectedTopologyRevision
                || info.TotalNodeCount != 7
                || info.EntryStride
                    != LMC_DiagnosticsParser.TopologyEntryStride
                || info.MaxEntriesPerChunk != 1
                || info.ConfiguredSlaveCount != 5
                || info.SlotModuleCount != 2
                || info.PhysicalAxisCount != 4)
            {
                throw new InvalidDataException(
                    "Topology info does not match the current bounded 5-slave/7-entry CREVIS contract.");
            }
        }

        internal static LMCEtherCATTopologyEntry[]
            ExpectedCurrentTopologyEntries()
        {
            return new[]
            {
                new LMCEtherCATTopologyEntry(
                    0xEC000001u, 0, 0, 0,
                    LMCEtherCATTopologyNodeKind.EtherCATSlave,
                    LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex
                        | LMCEtherCATTopologyNodeFlags.IoCoupler,
                    0, 0, ushort.MaxValue,
                    669, 1196200070, 65536, 0, 0, 0,
                    "GL_9086_11", 0),
                CreateExpectedDrive(1, "Elmo_11"),
                CreateExpectedDrive(2, "Elmo_21"),
                CreateExpectedDrive(3, "Elmo_31"),
                CreateExpectedDrive(4, "Elmo_41"),
                new LMCEtherCATTopologyEntry(
                    0xEC010001u, 0xEC000001u, 5, ushort.MaxValue,
                    LMCEtherCATTopologyNodeKind.SlotModule,
                    LMCEtherCATTopologyNodeFlags.HasInputs
                        | LMCEtherCATTopologyNodeFlags.HasDigitalIO,
                    0, 0, 0,
                    669, 1196692218, 0, 0, 4, 0,
                    "GL_9086_1_Slot001", 0x00010001u),
                new LMCEtherCATTopologyEntry(
                    0xEC010002u, 0xEC000001u, 6, ushort.MaxValue,
                    LMCEtherCATTopologyNodeKind.SlotModule,
                    LMCEtherCATTopologyNodeFlags.HasOutputs
                        | LMCEtherCATTopologyNodeFlags.HasDigitalIO,
                    0, 0, 1,
                    669, 1196696250, 0, 0, 0, 4,
                    "GL_9086_1_Slot011", 0x00010002u)
            };
        }

        private static LMCEtherCATTopologyEntry CreateExpectedDrive(
            ushort axis,
            string name)
        {
            return new LMCEtherCATTopologyEntry(
                checked(0xEC000100u + axis),
                0,
                axis,
                axis,
                LMCEtherCATTopologyNodeKind.EtherCATSlave,
                LMCEtherCATTopologyNodeFlags.HasMasterSlaveIndex
                    | LMCEtherCATTopologyNodeFlags.SupportsSdo
                    | LMCEtherCATTopologyNodeFlags.PhysicalAxis
                    | LMCEtherCATTopologyNodeFlags.Ds402Drive,
                axis,
                axis,
                ushort.MaxValue,
                154,
                198948,
                66592,
                0,
                0,
                0,
                name,
                0);
        }

        private static void RequireEntryMatch(
            LMCEtherCATTopologyEntry expected,
            LMCEtherCATTopologyEntry actual)
        {
            if (actual == null
                || actual.NodeId != expected.NodeId
                || actual.ParentNodeId != expected.ParentNodeId
                || actual.TopologyIndex != expected.TopologyIndex
                || actual.MasterSlaveIndex != expected.MasterSlaveIndex
                || actual.NodeKind != expected.NodeKind
                || actual.NodeFlags != expected.NodeFlags
                || actual.SdoSlaveReference != expected.SdoSlaveReference
                || actual.PhysicalAxisReference
                    != expected.PhysicalAxisReference
                || actual.SlotIndex != expected.SlotIndex
                || actual.VendorId != expected.VendorId
                || actual.ProductCode != expected.ProductCode
                || actual.RevisionNumber != expected.RevisionNumber
                || actual.SerialNumber != expected.SerialNumber
                || actual.InputBytes != expected.InputBytes
                || actual.OutputBytes != expected.OutputBytes
                || actual.IOReference != expected.IOReference
                || !string.Equals(
                    actual.Name,
                    expected.Name,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Topology entry "
                    + expected.TopologyIndex
                    + " does not match the current configured identity contract.");
            }
        }

        private static void AddTopologyToReport(
            TopologyIoQualificationReport report,
            LMCEtherCATTopology topology)
        {
            for (var index = 0; index < topology.Entries.Count; index++)
            {
                var entry = topology.Entries[index];
                var prefix = "TOPOLOGY_ENTRY_"
                    + index.ToString("D2", CultureInfo.InvariantCulture);
                report.Add(prefix + "_NAME", entry.Name);
                report.Add(prefix + "_NODE_ID", Hex(entry.NodeId));
                report.Add(prefix + "_PARENT_NODE_ID",
                    Hex(entry.ParentNodeId));
                report.Add(prefix + "_KIND", entry.NodeKind);
                report.Add(prefix + "_FLAGS", Hex((uint)entry.NodeFlags));
                report.Add(prefix + "_MASTER_SLAVE_INDEX",
                    entry.MasterSlaveIndex);
                report.Add(prefix + "_SLOT_INDEX", entry.SlotIndex);
                report.Add(prefix + "_INPUT_BYTES", entry.InputBytes);
                report.Add(prefix + "_OUTPUT_BYTES", entry.OutputBytes);
                report.Add(prefix + "_IO_REFERENCE", Hex(entry.IOReference));
                report.Add(prefix + "_VENDOR_ID", Hex(entry.VendorId));
                report.Add(prefix + "_PRODUCT_CODE", Hex(entry.ProductCode));
            }
        }

        private static void AddHealthToReport(
            TopologyIoQualificationReport report,
            string prefix,
            LMCEtherCATNodeHealth health)
        {
            report.Add(prefix + "_NODE_ID", Hex(health.NodeId));
            report.Add(prefix + "_FLAGS", Hex((uint)health.HealthFlags));
            report.Add(prefix + "_CYCLE", health.CycleCounter);
            report.Add(prefix + "_TIMESTAMP_US", health.TimestampMicroseconds);
            report.Add(prefix + "_SEQUENCE", health.SnapshotSequence);
            report.Add(prefix + "_ONLINE", health.Online);
            report.Add(prefix + "_ECAT_STATE", health.EtherCATState);
            report.Add(prefix + "_AL_STATUS", Hex(health.ALStatusCode));
            report.Add(prefix + "_SLAVE_STATE", Hex(health.SlaveState));
            report.Add(prefix + "_CLASS_STATE", Hex(health.ClassState));
            report.Add(prefix + "_DS402_STATUS", Hex(health.DS402StatusWord));
            report.Add(prefix + "_AXIS_ERROR", Hex(health.AxisError));
            report.Add(prefix + "_LAST_VALID_CYCLE", health.LastValidCycle);
            report.Add(prefix + "_LAST_CHANGE_CYCLE",
                health.LastStateChangeCycle);
        }

        private static uint NextRawRequestId()
        {
            var next = unchecked(Interlocked.Increment(ref rawRequestSequence));
            var low = unchecked((uint)next) & 0x0000FFFFu;
            return 0x54490000u | (low == 0 ? 1u : low);
        }

        private static ushort GetAllowedPayloadLength(ushort command)
        {
            switch (command)
            {
                case LMC_CommandId.GetEtherCATTopologyInfo:
                    return LMC_DiagnosticsFrame.CommonRequestPayloadLength;
                case LMC_CommandId.GetEtherCATTopologyChunk:
                    return LMC_DiagnosticsFrame.TopologyChunkRequestPayloadLength;
                case LMC_CommandId.ReadEtherCATNodeHealth:
                    return LMC_DiagnosticsFrame.NodeHealthRequestPayloadLength;
                case LMC_CommandId.ReadDigitalIO:
                    return LMC_DiagnosticsFrame.DigitalIOReadRequestPayloadLength;
                default:
                    throw new InvalidOperationException(
                        "Command is not in the topology/I/O read-only allowlist.");
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
                    "Topology/I/O raw reads require zero header reserved/reference fields, schema version 1, zero flags, and a non-zero request ID.");
            }

            if (command == LMC_CommandId.GetEtherCATTopologyInfo)
            {
                return;
            }

            var topologyRevision = LMC_Frame.ReadUInt32(
                request,
                payloadOffset + 8);
            if (topologyRevision == 0)
            {
                throw new InvalidOperationException(
                    "Topology/I/O raw reads require a non-zero TopologyRevision.");
            }

            if (command == LMC_CommandId.GetEtherCATTopologyChunk)
            {
                var maxEntries = LMC_Frame.ReadUInt16(
                    request,
                    payloadOffset + 14);
                if (maxEntries == 0
                    || maxEntries
                        > LMC_DiagnosticsFrame.MaxTopologyEntriesPerChunk)
                {
                    throw new InvalidOperationException(
                        "Topology chunk range is outside the fixed contract.");
                }

                return;
            }

            if (command == LMC_CommandId.ReadEtherCATNodeHealth)
            {
                if (LMC_Frame.ReadUInt32(request, payloadOffset + 12) == 0)
                {
                    throw new InvalidOperationException(
                        "Node-health raw reads require a non-zero NodeId.");
                }

                return;
            }

            var direction = request[payloadOffset + 16];
            var width = request[payloadOffset + 17];
            if (LMC_Frame.ReadUInt32(request, payloadOffset + 12) == 0
                || (direction != (byte)LMCDigitalIODirection.Input
                    && direction != (byte)LMCDigitalIODirection.Output)
                || width == 0
                || width > 64
                || LMC_Frame.ReadUInt16(request, payloadOffset + 18) != 0)
            {
                throw new InvalidOperationException(
                    "Digital I/O raw reads require a non-zero reference, exact direction/width, and zero reserved field.");
            }
        }

        private static string Hex(uint value)
        {
            return "0x" + value.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static string Hex(ushort value)
        {
            return "0x" + value.ToString("X4", CultureInfo.InvariantCulture);
        }

        private static string Hex(ulong value)
        {
            return "0x" + value.ToString("X16", CultureInfo.InvariantCulture);
        }
    }
}

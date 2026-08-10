using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlLib.Tests
{
    internal enum CallbackOwnershipWireScenario
    {
        All = 0,
        GdN10A = 1,
        GdN13Candidate = 2,
        GdN14Candidate = 3
    }

    internal sealed class CallbackOwnershipWireOptions
    {
        internal const string LiveConfirmation = "PLC-CALLBACK-OWNERSHIP";

        internal CallbackOwnershipWireOptions()
        {
            Scenario = CallbackOwnershipWireScenario.All;
            RemotePort = 4000;
            OwnerCallbackPort = 0;
            CandidateCallbackPort = 0;
            TimeoutMilliseconds = 3000;
        }

        internal CallbackOwnershipWireScenario Scenario { get; private set; }
        internal bool ExecuteLive { get; private set; }
        internal bool ShowHelp { get; private set; }
        internal string RemoteAddress { get; private set; }
        internal int RemotePort { get; private set; }
        internal string OwnerLocalAddress { get; private set; }
        internal string CandidateLocalAddress { get; private set; }
        internal int OwnerCallbackPort { get; private set; }
        internal int CandidateCallbackPort { get; private set; }
        internal int TimeoutMilliseconds { get; private set; }
        internal string OutputPath { get; private set; }
        internal string Confirmation { get; private set; }
        internal string SourceFingerprint { get; private set; }

        internal static CallbackOwnershipWireOptions Parse(string[] args)
        {
            if (args == null
                || args.Length == 0
                || !string.Equals(
                    args[0],
                    "callback-ownership-wire",
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The first argument must be the exact token 'callback-ownership-wire'.");
            }

            var options = new CallbackOwnershipWireOptions();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var sawDryRun = false;

            for (var index = 1; index < args.Length; index++)
            {
                var argument = args[index];
                if (!seen.Add(argument))
                {
                    throw new ArgumentException(
                        "Duplicate callback-ownership-wire argument '"
                        + argument
                        + "'.");
                }

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
                else if (string.Equals(argument, "--owner-local", StringComparison.Ordinal))
                {
                    options.OwnerLocalAddress = ReadValue(
                        args,
                        ref index,
                        argument);
                }
                else if (string.Equals(argument, "--candidate-local", StringComparison.Ordinal))
                {
                    options.CandidateLocalAddress = ReadValue(
                        args,
                        ref index,
                        argument);
                }
                else if (string.Equals(argument, "--owner-callback-port", StringComparison.Ordinal))
                {
                    options.OwnerCallbackPort = ParseBoundedInt(
                        ReadValue(args, ref index, argument),
                        argument,
                        0,
                        65535);
                }
                else if (string.Equals(argument, "--candidate-callback-port", StringComparison.Ordinal))
                {
                    options.CandidateCallbackPort = ParseBoundedInt(
                        ReadValue(args, ref index, argument),
                        argument,
                        0,
                        65535);
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
                else if (string.Equals(
                    argument,
                    "--source-fingerprint",
                    StringComparison.Ordinal))
                {
                    options.SourceFingerprint = ReadValue(
                        args,
                        ref index,
                        argument);
                }
                else
                {
                    throw new ArgumentException(
                        "Unknown callback-ownership-wire argument '"
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
                if (args.Length != 2)
                {
                    throw new ArgumentException(
                        "--help cannot be combined with other arguments.");
                }

                return options;
            }

            if (options.ExecuteLive)
            {
                RequireLiveOptions(options);
            }
            else if (!string.IsNullOrWhiteSpace(options.OutputPath)
                || !string.IsNullOrWhiteSpace(options.Confirmation)
                || !string.IsNullOrWhiteSpace(options.RemoteAddress)
                || !string.IsNullOrWhiteSpace(options.OwnerLocalAddress)
                || !string.IsNullOrWhiteSpace(options.CandidateLocalAddress)
                || !string.IsNullOrWhiteSpace(options.SourceFingerprint)
                || seen.Contains("--port")
                || seen.Contains("--owner-callback-port")
                || seen.Contains("--candidate-callback-port")
                || seen.Contains("--timeout-ms"))
            {
                throw new ArgumentException(
                    "Network endpoints, confirmation, and output are accepted only with --execute-live.");
            }

            return options;
        }

        internal static string GetScenarioToken(
            CallbackOwnershipWireScenario scenario)
        {
            switch (scenario)
            {
                case CallbackOwnershipWireScenario.All:
                    return "all";
                case CallbackOwnershipWireScenario.GdN10A:
                    return "gd-n10a";
                case CallbackOwnershipWireScenario.GdN13Candidate:
                    return "gd-n13-candidate";
                case CallbackOwnershipWireScenario.GdN14Candidate:
                    return "gd-n14-candidate";
                default:
                    throw new ArgumentOutOfRangeException("scenario");
            }
        }

        private static void RequireLiveOptions(
            CallbackOwnershipWireOptions options)
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

            if (options.Scenario == CallbackOwnershipWireScenario.All)
            {
                throw new ArgumentException(
                    "Live execution requires one concrete scenario; 'all' is prohibited.");
            }

            var remote = RequireIpv4(options.RemoteAddress, "--host");
            var owner = RequireIpv4(
                options.OwnerLocalAddress,
                "--owner-local");
            var candidate = RequireIpv4(
                options.CandidateLocalAddress,
                "--candidate-local");
            RequireSourceFingerprint(options.SourceFingerprint);

            if (remote.Equals(IPAddress.Any)
                || remote.Equals(IPAddress.None)
                || owner.Equals(IPAddress.Any)
                || owner.Equals(IPAddress.None)
                || candidate.Equals(IPAddress.Any)
                || candidate.Equals(IPAddress.None))
            {
                throw new ArgumentException(
                    "Unspecified and broadcast IPv4 addresses are prohibited.");
            }

            var sameLocal = owner.Equals(candidate);
            if (options.Scenario == CallbackOwnershipWireScenario.GdN13Candidate
                && !sameLocal)
            {
                throw new ArgumentException(
                    "gd-n13-candidate requires identical owner and candidate source IPv4 addresses.");
            }

            if ((options.Scenario == CallbackOwnershipWireScenario.GdN10A
                    || options.Scenario
                        == CallbackOwnershipWireScenario.GdN14Candidate)
                && sameLocal)
            {
                throw new ArgumentException(
                    CallbackOwnershipWireOptions.GetScenarioToken(
                        options.Scenario)
                    + " requires different owner and candidate IPv4 addresses.");
            }

            if (options.Scenario == CallbackOwnershipWireScenario.GdN10A
                && options.CandidateCallbackPort != 0)
            {
                throw new ArgumentException(
                    "gd-n10a requires --candidate-callback-port 0 because mismatch B reuses the actual owner callback port.");
            }

            if (string.IsNullOrWhiteSpace(options.OutputPath))
            {
                throw new ArgumentException(
                    "Live execution requires an explicit --output report path.");
            }

            options.OutputPath = Path.GetFullPath(options.OutputPath);
        }

        private static void RequireSourceFingerprint(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Live execution requires --source-fingerprint HEAD/TRACKED/UNTRACKED.");
            }

            var parts = value.Split('/');
            if (parts.Length != 3)
            {
                throw new ArgumentException(
                    "--source-fingerprint must be HEAD/TRACKED/UNTRACKED using Git object hashes.");
            }

            for (var partIndex = 0; partIndex < parts.Length; partIndex++)
            {
                var part = parts[partIndex];
                if ((part.Length != 40 && part.Length != 64)
                    || !IsHex(part))
                {
                    throw new ArgumentException(
                        "--source-fingerprint components must be 40- or 64-character hexadecimal Git object hashes.");
                }
            }
        }

        private static bool IsHex(string value)
        {
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if (!((character >= '0' && character <= '9')
                    || (character >= 'a' && character <= 'f')
                    || (character >= 'A' && character <= 'F')))
                {
                    return false;
                }
            }

            return true;
        }

        private static IPAddress RequireIpv4(string value, string option)
        {
            IPAddress address;
            if (!IPAddress.TryParse(value, out address)
                || address.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException(
                    option + " requires an explicit IPv4 address.");
            }

            return address;
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
            if (string.IsNullOrWhiteSpace(value)
                || value.StartsWith("--", StringComparison.Ordinal))
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

        private static CallbackOwnershipWireScenario ParseScenario(
            string value)
        {
            switch (value)
            {
                case "all":
                    return CallbackOwnershipWireScenario.All;
                case "gd-n10a":
                    return CallbackOwnershipWireScenario.GdN10A;
                case "gd-n13-candidate":
                    return CallbackOwnershipWireScenario.GdN13Candidate;
                case "gd-n14-candidate":
                    return CallbackOwnershipWireScenario.GdN14Candidate;
                default:
                    throw new ArgumentException(
                        "Unknown callback-ownership-wire scenario '"
                        + value
                        + "'.");
            }
        }
    }

    internal sealed class CallbackOwnershipWireReport
    {
        private readonly StringBuilder text = new StringBuilder();

        internal CallbackOwnershipWireReport(
            CallbackOwnershipWireOptions options)
        {
            Add("FORMAT", "LMC_CALLBACK_OWNERSHIP_WIRE_V1");
            Add("START_UTC", DateTime.UtcNow.ToString("O"));
            Add("MODE", options.ExecuteLive ? "LIVE" : "DRY_RUN");
            Add("SCENARIO", CallbackOwnershipWireOptions.GetScenarioToken(
                options.Scenario));
            Add("EVIDENCE_CLASS", "PC_RAW_WIRE_HARNESS");
            Add("PEER_IDENTITY", "UNVERIFIED");
            Add("PCAP_EVIDENCE", "NOT_CAPTURED_BY_TOOL");
            Add("PLC_WATCH_EVIDENCE", "NOT_CAPTURED_BY_TOOL");
            Add("QUALIFICATION_COMPLETE", "FALSE");
            Add(
                "QUALIFICATION_RESULT",
                "INCOMPLETE_WITHOUT_PCAP_AND_PLC_WATCH");
            Add(
                "QUALIFICATION_LIMITATION",
                "Requires correlated packet capture and PLC Watch counters; this report alone is not PLC qualification.");
            Add("RETRY_COUNT", 0);
            Add("COMMAND_ALLOWLIST", "0x8080,0x405C,0x405D");

            if (options.ExecuteLive)
            {
                Add("REMOTE_ENDPOINT", options.RemoteAddress
                    + ":"
                    + options.RemotePort);
                Add("OWNER_LOCAL_IPV4", options.OwnerLocalAddress);
                Add("CANDIDATE_LOCAL_IPV4", options.CandidateLocalAddress);
                Add("OWNER_CALLBACK_PORT_REQUEST",
                    options.OwnerCallbackPort);
                Add("CANDIDATE_CALLBACK_PORT_REQUEST",
                    options.CandidateCallbackPort);
                Add("TIMEOUT_MS", options.TimeoutMilliseconds);
                Add("SOURCE_FINGERPRINT_DECLARED",
                    options.SourceFingerprint);
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

        internal void WriteTo(Stream stream)
        {
            if (stream == null
                || !stream.CanWrite
                || !stream.CanSeek)
            {
                throw new ArgumentException(
                    "The report stream must be writable and seekable.",
                    "stream");
            }

            var bytes = new UTF8Encoding(false).GetBytes(text.ToString());
            stream.Position = 0;
            stream.SetLength(0);
            stream.Write(bytes, 0, bytes.Length);
            var file = stream as FileStream;
            if (file != null)
            {
                file.Flush(true);
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

        internal static string ComputeSha256(Stream stream)
        {
            using (var hash = SHA256.Create())
            {
                return ToHex(hash.ComputeHash(stream));
            }
        }
    }

    internal sealed class CallbackOwnershipInconclusiveException
        : Exception
    {
        internal CallbackOwnershipInconclusiveException(string message)
            : base(message)
        {
        }
    }

    internal sealed class CallbackOwnershipBoundUdp : IDisposable
    {
        internal CallbackOwnershipBoundUdp(
            string localAddress,
            int requestedPort,
            string label,
            CallbackOwnershipWireReport report,
            FileStream reportStream)
        {
            Client = new UdpClient(
                new IPEndPoint(
                    IPAddress.Parse(localAddress),
                    requestedPort));
            EndPoint = (IPEndPoint)Client.Client.LocalEndPoint;
            report.Add(label + "_UDP_BOUND_ENDPOINT", EndPoint);
            report.Add(label + "_CALLBACK_ENDPOINT_ACTUAL", EndPoint);
            report.WriteTo(reportStream);
        }

        internal UdpClient Client { get; private set; }
        internal IPEndPoint EndPoint { get; private set; }

        public void Dispose()
        {
            if (Client != null)
            {
                Client.Close();
                Client = null;
            }
        }
    }

    internal sealed class CallbackOwnershipRegistrationResult
    {
        internal CallbackOwnershipRegistrationResult(
            byte[] request,
            byte[] response,
            LMCCallbackRegistrationV2Request registration,
            LMCCallbackRegistrationV2Response accepted)
        {
            Request = (byte[])request.Clone();
            Response = (byte[])response.Clone();
            Registration = registration;
            Accepted = accepted;
        }

        internal byte[] Request { get; private set; }
        internal byte[] Response { get; private set; }
        internal LMCCallbackRegistrationV2Request Registration
        {
            get;
            private set;
        }
        internal LMCCallbackRegistrationV2Response Accepted
        {
            get;
            private set;
        }
    }

    internal sealed class CallbackOwnershipWireSession : IDisposable
    {
        private readonly CallbackOwnershipWireOptions options;
        private readonly CallbackOwnershipWireReport report;
        private readonly FileStream reportStream;
        private readonly string label;
        private readonly IPAddress localAddress;
        private readonly IPAddress remoteAddress;
        private TcpClient client;
        private NetworkStream stream;
        private bool disposed;

        internal CallbackOwnershipWireSession(
            CallbackOwnershipWireOptions options,
            CallbackOwnershipWireReport report,
            FileStream reportStream,
            string label,
            string localAddress)
        {
            this.options = options;
            this.report = report;
            this.reportStream = reportStream;
            this.label = label;
            this.localAddress = IPAddress.Parse(localAddress);
            remoteAddress = IPAddress.Parse(options.RemoteAddress);
        }

        internal bool IsAuthoritative { get; set; }

        internal void Open()
        {
            if (client != null)
            {
                throw new InvalidOperationException(
                    label + " transport is already open.");
            }

            var opening = new TcpClient(
                new IPEndPoint(localAddress, 0));
            opening.NoDelay = true;
            opening.ReceiveTimeout = options.TimeoutMilliseconds;
            opening.SendTimeout = options.TimeoutMilliseconds;
            try
            {
                var connect = opening.BeginConnect(
                    remoteAddress,
                    options.RemotePort,
                    null,
                    null);
                if (!connect.AsyncWaitHandle.WaitOne(
                    options.TimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        label + " TCP connect timed out.");
                }

                opening.EndConnect(connect);
                client = opening;
                stream = client.GetStream();
                report.Add(label + "_TCP_LOCAL_ENDPOINT",
                    client.Client.LocalEndPoint);
                report.Add(label + "_TCP_REMOTE_ENDPOINT",
                    client.Client.RemoteEndPoint);
                Checkpoint();
            }
            catch
            {
                opening.Close();
                throw;
            }
        }

        internal void Initialize()
        {
            var response = Exchange(
                label + "_INIT",
                LMC_Frame.RpcSessionInit());
            var parsed = LMCConnection.Parse(response);
            if (!parsed.IsFrameValid
                || parsed.HeaderStatus != 0
                || parsed.HeaderReserved != 0
                || parsed.PayloadLength != 24
                || parsed.Payload.Length != 24)
            {
                throw new InvalidDataException(
                    label
                    + " RPC session init did not return the exact success envelope.");
            }

            report.Add(label + "_INIT_RESULT", "PASS");
            Checkpoint();
        }

        internal void ExpectPeerCloseOnInitialization()
        {
            var request = LMC_Frame.RpcSessionInit();
            CallbackOwnershipWireTool.EnsureAllowedRequest(request);
            report.Add(label + "_REJECTION_PROBE_COMMAND", "0x8080");
            report.AddFrame(label + "_REJECTION_PROBE_REQUEST", request);
            Checkpoint();

            try
            {
                stream.Write(request, 0, request.Length);
                stream.Flush();
                var firstByte = stream.ReadByte();
                if (firstByte < 0)
                {
                    report.Add(label + "_REJECTION_RESULT",
                        "EXPECTED_PEER_CLOSE_PASS");
                    Checkpoint();
                    return;
                }

                byte[] unexpected;
                try
                {
                    unexpected = ReadUnexpectedInitResponse(
                        stream,
                        (byte)firstByte);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException(
                        label
                        + " received a partial 0x8080 response instead of a clean peer close.",
                        ex);
                }

                report.AddFrame(
                    label + "_REJECTION_UNEXPECTED_RESPONSE",
                    unexpected);
                Checkpoint();
                throw new InvalidDataException(
                    label
                    + " received a successful or malformed 0x8080 response instead of the required different-IP peer close.");
            }
            catch (IOException ex)
            {
                if (CallbackOwnershipWireTool
                    .IsInconclusivePeerTermination(ex))
                {
                    report.Add(label + "_REJECTION_RESULT",
                        "INCONCLUSIVE_TRANSPORT_TERMINATION");
                    Checkpoint();
                    throw new CallbackOwnershipInconclusiveException(
                        label
                        + " different-IP rejection ended with timeout, ConnectionAborted, or Shutdown; clean EOF/ConnectionReset was not observed.");
                }

                if (CallbackOwnershipWireTool.IsPeerClose(ex))
                {
                    report.Add(label + "_REJECTION_RESULT",
                        "EXPECTED_PEER_CLOSE_PASS");
                    Checkpoint();
                    return;
                }

                throw;
            }
            catch (SocketException ex)
            {
                if (CallbackOwnershipWireTool
                    .IsInconclusivePeerTermination(ex))
                {
                    report.Add(label + "_REJECTION_RESULT",
                        "INCONCLUSIVE_TRANSPORT_TERMINATION");
                    Checkpoint();
                    throw new CallbackOwnershipInconclusiveException(
                        label
                        + " different-IP rejection ended with timeout, ConnectionAborted, or Shutdown; clean EOF/ConnectionReset was not observed.");
                }

                if (CallbackOwnershipWireTool.IsPeerClose(ex))
                {
                    report.Add(label + "_REJECTION_RESULT",
                        "EXPECTED_PEER_CLOSE_PASS");
                    Checkpoint();
                    return;
                }

                throw;
            }
        }

        internal void ObserveOldOwnerPeerCloseAfterTakeover()
        {
            report.Add(label + "_TAKEOVER_RETIRE_WAIT", "BEGIN");
            Checkpoint();
            try
            {
                var firstByte = stream.ReadByte();
                if (firstByte < 0)
                {
                    report.Add(label + "_TAKEOVER_RETIRE_RESULT",
                        "CLEAN_EOF_PASS");
                    Checkpoint();
                    return;
                }

                report.Add(label + "_TAKEOVER_RETIRE_UNEXPECTED_BYTE",
                    "0x" + firstByte.ToString("X2"));
                report.Add(label + "_TAKEOVER_RETIRE_RESULT",
                    "INCONCLUSIVE_UNSOLICITED_DATA");
                Checkpoint();
                throw new CallbackOwnershipInconclusiveException(
                    label
                    + " old-owner stream produced unsolicited data instead of peer retirement.");
            }
            catch (IOException ex)
            {
                HandleTakeoverRetireException(ex);
            }
            catch (SocketException ex)
            {
                HandleTakeoverRetireException(ex);
            }
        }

        private void HandleTakeoverRetireException(Exception exception)
        {
            if (CallbackOwnershipWireTool.IsPeerClose(exception))
            {
                report.Add(label + "_TAKEOVER_RETIRE_RESULT",
                    "CONNECTION_RESET_PASS");
                Checkpoint();
                return;
            }

            report.Add(label + "_TAKEOVER_RETIRE_RESULT",
                "INCONCLUSIVE_TRANSPORT_TERMINATION");
            report.Add(label + "_TAKEOVER_RETIRE_EXCEPTION",
                exception.GetType().FullName + ": " + exception.Message);
            Checkpoint();
            throw new CallbackOwnershipInconclusiveException(
                label
                + " old-owner peer retirement was not clean EOF or ConnectionReset.");
        }

        private static byte[] ReadUnexpectedInitResponse(
            NetworkStream stream,
            byte firstByte)
        {
            var header = new byte[LMC_Frame.HeaderSize];
            header[0] = firstByte;
            var remainder = ReadExact(stream, LMC_Frame.HeaderSize - 1);
            Buffer.BlockCopy(
                remainder,
                0,
                header,
                1,
                remainder.Length);
            var payloadLength = LMC_Frame.GetResponsePayloadLength(header);
            if (payloadLength
                > LMC_ResponsePayloadLimits.GetMaximumPayloadLength(
                    LMC_CommandId.RpcSessionInit))
            {
                throw new InvalidDataException(
                    "Unexpected 0x8080 response exceeded its payload limit.");
            }

            var payload = payloadLength == 0
                ? new byte[0]
                : ReadExact(stream, payloadLength);
            var response = new byte[header.Length + payload.Length];
            Buffer.BlockCopy(header, 0, response, 0, header.Length);
            if (payload.Length != 0)
            {
                Buffer.BlockCopy(
                    payload,
                    0,
                    response,
                    header.Length,
                    payload.Length);
            }

            return response;
        }

        internal CallbackOwnershipRegistrationResult Register(
            string operation,
            LMCCallbackRegistrationV2Request registration,
            bool expectSuccess)
        {
            var request = LMCCallbackProtocol.CreateRegistrationV2Request(
                registration,
                LMCCallbackProtocolPolicy.InitialV2WakeHint);
            return RegisterRaw(
                operation,
                request,
                registration,
                expectSuccess);
        }

        internal CallbackOwnershipRegistrationResult RegisterRaw(
            string operation,
            byte[] request,
            LMCCallbackRegistrationV2Request registration,
            bool expectSuccess)
        {
            var response = Exchange(operation, request);
            var envelope = LMCConnection
                .ParseCallbackRegistrationV2Envelope(response);

            if (!expectSuccess)
            {
                if (envelope.CommandStatus != 1
                    || envelope.ErrorId != -1
                    || envelope.IsSuccess)
                {
                    throw new InvalidDataException(
                        operation
                        + " did not return the canonical version-2 failure envelope.");
                }

                report.Add(operation + "_RESULT", "EXPECTED_FAILURE_PASS");
                Checkpoint();
                return new CallbackOwnershipRegistrationResult(
                    request,
                    response,
                    registration,
                    null);
            }

            if (!envelope.IsSuccess)
            {
                throw new InvalidOperationException(
                    operation
                    + " failed. Status="
                    + envelope.CommandStatus
                    + ", ErrorId="
                    + envelope.ErrorId
                    + ".");
            }

            var parsed = LMCCallbackProtocol.ParseRegistrationV2Response(
                envelope.Payload,
                registration,
                remoteAddress.GetAddressBytes(),
                1,
                LMCCallbackProtocolPolicy.InitialV2WakeHint);
            if (!parsed.IsAccepted)
            {
                throw new InvalidDataException(
                    operation
                    + " success fence failed validation: "
                    + parsed.Error
                    + ".");
            }

            report.Add(operation + "_RESULT", "PASS");
            report.Add(operation + "_COOKIE",
                "0x" + registration.ClientCookie.ToString("X16"));
            report.Add(operation + "_BOOT_ID",
                "0x" + parsed.Value.DiagnosticsBootId.ToString("X8"));
            report.Add(operation + "_SESSION_EPOCH",
                "0x" + parsed.Value.SessionEpoch.ToString("X8"));
            report.Add(operation + "_ACCEPTED_MAX",
                parsed.Value.AcceptedMaxDatagram);
            Checkpoint();
            return new CallbackOwnershipRegistrationResult(
                request,
                response,
                registration,
                parsed.Value);
        }

        internal void CloseAuthoritative()
        {
            if (!IsAuthoritative)
            {
                throw new InvalidOperationException(
                    label
                    + " cannot send 0x405D because it is not the authoritative owner.");
            }

            var response = Exchange(
                label + "_CLOSE",
                LMC_Frame.CloseConnection());
            var parsed = LMCConnection.ParseShortAcknowledgement(
                response,
                label + " close");
            if (!parsed.IsSuccess)
            {
                throw new InvalidOperationException(
                    label
                    + " close failed. Status="
                    + parsed.CommandStatus
                    + ", ErrorId="
                    + parsed.ErrorId
                    + ".");
            }

            IsAuthoritative = false;
            report.Add(label + "_CLOSE_RESULT", "PASS");
            Checkpoint();
            DisposeTransport();
        }

        internal void DisconnectWithoutClose(string reason)
        {
            report.Add(label + "_TRANSPORT_DISCONNECT_WITHOUT_405D", reason);
            Checkpoint();
            DisposeTransport();
        }

        private byte[] Exchange(string operation, byte[] request)
        {
            CallbackOwnershipWireTool.EnsureAllowedRequest(request);
            if (stream == null)
            {
                throw new InvalidOperationException(
                    label + " transport is not open.");
            }

            report.Add(operation + "_COMMAND", "0x"
                + LMC_Frame.GetRequestCommand(request).ToString("X4"));
            report.AddFrame(operation + "_REQUEST", request);
            Checkpoint();

            stream.Write(request, 0, request.Length);
            stream.Flush();
            var response = ReadResponse(
                stream,
                LMC_Frame.GetRequestCommand(request));
            report.AddFrame(operation + "_RESPONSE", response);
            Checkpoint();
            return response;
        }

        private static byte[] ReadResponse(
            NetworkStream stream,
            ushort command)
        {
            var header = ReadExact(stream, LMC_Frame.HeaderSize);
            var payloadLength = LMC_Frame.GetResponsePayloadLength(header);
            if (payloadLength
                > LMC_ResponsePayloadLimits.GetMaximumPayloadLength(command))
            {
                throw new InvalidDataException(
                    "RPC response payload exceeds the SDK maximum.");
            }

            var payload = payloadLength == 0
                ? new byte[0]
                : ReadExact(stream, payloadLength);
            var response = new byte[header.Length + payload.Length];
            Buffer.BlockCopy(header, 0, response, 0, header.Length);
            if (payload.Length != 0)
            {
                Buffer.BlockCopy(
                    payload,
                    0,
                    response,
                    header.Length,
                    payload.Length);
            }

            return response;
        }

        private static byte[] ReadExact(NetworkStream stream, int count)
        {
            var buffer = new byte[count];
            var offset = 0;
            while (offset < count)
            {
                var read = stream.Read(buffer, offset, count - offset);
                if (read <= 0)
                {
                    throw new EndOfStreamException(
                        "RPC peer closed before the complete response arrived.");
                }

                offset += read;
            }

            return buffer;
        }

        private void Checkpoint()
        {
            report.WriteTo(reportStream);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            DisposeTransport();
        }

        private void DisposeTransport()
        {
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }

            if (client != null)
            {
                client.Close();
                client = null;
            }
        }
    }

    internal static class CallbackOwnershipWireTool
    {
        internal const int SuccessExitCode = 0;
        internal const int UsageExitCode = 2;
        internal const int VerificationFailureExitCode = 3;
        internal const int ReportFailureExitCode = 4;

        internal static bool IsInvocation(string[] args)
        {
            return args != null
                && args.Length > 0
                && string.Equals(
                    args[0],
                    "callback-ownership-wire",
                    StringComparison.Ordinal);
        }

        internal static int Run(string[] args)
        {
            CallbackOwnershipWireOptions options;
            try
            {
                options = CallbackOwnershipWireOptions.Parse(args);
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

            var report = new CallbackOwnershipWireReport(options);
            AppendExecutionIdentity(report);
            if (!options.ExecuteLive)
            {
                AppendDryRunPlan(report, options.Scenario);
                Console.Write(report.ToString());
                return SuccessExitCode;
            }

            string inProgressPath = null;
            FileStream inProgressReport = null;
            try
            {
                inProgressReport = ReserveLiveReport(
                    options.OutputPath,
                    out inProgressPath);
                report.Add("REPORT_FINAL_PATH", options.OutputPath);
                report.Add("REPORT_IN_PROGRESS_PATH", inProgressPath);
                report.WriteTo(inProgressReport);
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
                RunLive(options, report, inProgressReport);
                report.Add("OVERALL_RESULT", "PASS");
                report.Add("TOOL_RESULT", "PASS");
            }
            catch (CallbackOwnershipInconclusiveException ex)
            {
                result = VerificationFailureExitCode;
                report.Add("OVERALL_RESULT", "INCONCLUSIVE");
                report.Add("TOOL_RESULT", "INCONCLUSIVE");
                report.Add("EXCEPTION_TYPE", ex.GetType().FullName);
                report.Add("EXCEPTION_MESSAGE", ex.Message);
                Console.Error.WriteLine(ex);
            }
            catch (Exception ex)
            {
                result = VerificationFailureExitCode;
                report.Add("OVERALL_RESULT", "FAIL");
                report.Add("TOOL_RESULT", "FAIL");
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

        internal static byte[] CreateRegistrationRequest(
            string callbackAddress,
            int callbackPort,
            ulong cookie)
        {
            if (cookie == 0)
            {
                throw new ArgumentOutOfRangeException("cookie");
            }

            var registration = CreateRegistration(
                callbackAddress,
                callbackPort,
                cookie);
            var request = LMCCallbackProtocol.CreateRegistrationV2Request(
                registration,
                LMCCallbackProtocolPolicy.InitialV2WakeHint);
            EnsureAllowedRequest(request);
            return request;
        }

        internal static void EnsureAllowedRequest(byte[] request)
        {
            if (request == null || request.Length < LMC_Frame.HeaderSize)
            {
                throw new InvalidDataException(
                    "Callback ownership request is missing its RPC header.");
            }

            var command = LMC_Frame.GetRequestCommand(request);
            if (command == LMC_CommandId.RpcSessionInit)
            {
                EnsureByteIdentical(
                    LMC_Frame.RpcSessionInit(),
                    request,
                    "0x8080 request");
                return;
            }

            if (command == LMC_CommandId.CloseConnection)
            {
                EnsureByteIdentical(
                    LMC_Frame.CloseConnection(),
                    request,
                    "0x405D request");
                return;
            }

            if (command != LMC_CommandId.RpcCallbackRegistration)
            {
                throw new InvalidOperationException(
                    "Raw command 0x"
                    + command.ToString("X4")
                    + " is outside the callback ownership allowlist.");
            }

            if (request.Length != LMC_Frame.HeaderSize
                    + LMCCallbackProtocol.RegistrationV2RequestPayloadBytes
                || LMC_Frame.ReadUInt16(request, 2) != 0
                || LMC_Frame.ReadUInt16(request, 4)
                    != LMCCallbackProtocol.RegistrationV2RequestPayloadBytes
                || LMC_Frame.ReadUInt16(request, 6) != 0
                || LMC_Frame.ReadUInt32(request, 8) != 1
                || LMC_Frame.ReadInt32(request, 12) < 1
                || LMC_Frame.ReadInt32(request, 12) > 65535
                || LMC_Frame.ReadUInt16(request, 20)
                    != LMCCallbackProtocol.ProtocolVersion2
                || LMC_Frame.ReadUInt16(request, 22)
                    != LMCCallbackProtocol.DatagramHeaderBytes
                || (LMC_Frame.ReadUInt32(request, 24) == 0
                    && LMC_Frame.ReadUInt32(request, 28) == 0)
                || LMC_Frame.ReadUInt32(request, 32) != 0
                || LMC_Frame.ReadUInt32(request, 36) != 0)
            {
                throw new InvalidOperationException(
                    "0x405C is allowed only as the fixed version-2 registration shape: mask 1, max 52, non-zero cookie, flags/reserved zero.");
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

        internal static bool IsPeerClose(Exception exception)
        {
            var socket = exception as SocketException;
            if (socket != null)
            {
                return socket.SocketErrorCode == SocketError.ConnectionReset;
            }

            var io = exception as IOException;
            return io != null
                && io.InnerException != null
                && IsPeerClose(io.InnerException);
        }

        internal static bool IsInconclusivePeerTermination(
            Exception exception)
        {
            var socket = exception as SocketException;
            if (socket != null)
            {
                return socket.SocketErrorCode == SocketError.TimedOut
                    || socket.SocketErrorCode == SocketError.ConnectionAborted
                    || socket.SocketErrorCode == SocketError.Shutdown;
            }

            var io = exception as IOException;
            return io != null
                && io.InnerException != null
                && IsInconclusivePeerTermination(io.InnerException);
        }

        internal static void WriteUsage(TextWriter writer)
        {
            writer.WriteLine("Usage:");
            writer.WriteLine(
                "  LasalMotionControlLib.Tests.exe callback-ownership-wire --dry-run [--scenario NAME]");
            writer.WriteLine(
                "  LasalMotionControlLib.Tests.exe callback-ownership-wire --execute-live --confirm "
                + CallbackOwnershipWireOptions.LiveConfirmation
                + " --scenario NAME --host IPv4 --owner-local IPv4 --candidate-local IPv4 --source-fingerprint HEAD/TRACKED/UNTRACKED [--port 4000] [--owner-callback-port 0] [--candidate-callback-port 0] [--timeout-ms 3000] --output NEW_FILE");
            writer.WriteLine(
                "Scenarios: gd-n10a, gd-n13-candidate, gd-n14-candidate (all is dry-run only)");
            writer.WriteLine(
                "Fixed commands only: 0x8080, version-2 0x405C, and 0x405D only for the current owner. No arbitrary command, payload, retry, downgrade, write, motion, reset, or download input exists.");
        }

        private static void AppendDryRunPlan(
            CallbackOwnershipWireReport report,
            CallbackOwnershipWireScenario scenario)
        {
            report.Add("NETWORK_CONNECTED", "FALSE");
            report.Add("LIVE_CONFIRMATION_REQUIRED",
                CallbackOwnershipWireOptions.LiveConfirmation);
            if (scenario == CallbackOwnershipWireScenario.All)
            {
                report.Add("PLANNED_SCENARIO", "gd-n10a");
                report.Add("PLANNED_SCENARIO", "gd-n13-candidate");
                report.Add("PLANNED_SCENARIO", "gd-n14-candidate");
            }
            else
            {
                report.Add("PLANNED_SCENARIO",
                    CallbackOwnershipWireOptions.GetScenarioToken(scenario));
            }

            report.Add("OVERALL_RESULT", "DRY_RUN_ONLY");
            report.Add("TOOL_RESULT", "DRY_RUN_ONLY");
        }

        private static void AppendExecutionIdentity(
            CallbackOwnershipWireReport report)
        {
            try
            {
                var executable = Assembly.GetExecutingAssembly().Location;
                report.Add("EXECUTABLE_PATH", Path.GetFullPath(executable));
                using (var stream = new FileStream(
                    executable,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete))
                {
                    report.Add("EXECUTABLE_SHA256",
                        CallbackOwnershipWireReport.ComputeSha256(stream));
                }
            }
            catch (Exception ex)
            {
                report.Add("EXECUTABLE_IDENTITY", "UNAVAILABLE");
                report.Add("EXECUTABLE_IDENTITY_ERROR", ex.Message);
            }

            string root;
            string head;
            if (TryReadGitHead(out root, out head))
            {
                report.Add("GIT_ROOT", root);
                report.Add("GIT_HEAD", head);
                report.Add("CHECKPOINT_IDENTITY", "GIT_HEAD:" + head);
                report.Add("GIT_WORKTREE_STATE", "NOT_CAPTURED_BY_TOOL");
            }
            else
            {
                report.Add("GIT_HEAD", "UNAVAILABLE");
                report.Add("CHECKPOINT_IDENTITY", "UNAVAILABLE");
            }
        }

        private static bool TryReadGitHead(
            out string root,
            out string head)
        {
            root = null;
            head = null;
            var starts = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Directory.GetCurrentDirectory()
            };

            foreach (var start in starts)
            {
                var directory = new DirectoryInfo(start);
                while (directory != null)
                {
                    var git = Path.Combine(directory.FullName, ".git");
                    var headPath = Path.Combine(git, "HEAD");
                    if (Directory.Exists(git) && File.Exists(headPath))
                    {
                        var headText = File.ReadAllText(headPath).Trim();
                        if (headText.StartsWith("ref: ", StringComparison.Ordinal))
                        {
                            var reference = headText.Substring(5);
                            var loose = Path.Combine(
                                git,
                                reference.Replace('/',
                                    Path.DirectorySeparatorChar));
                            if (File.Exists(loose))
                            {
                                headText = File.ReadAllText(loose).Trim();
                            }
                            else
                            {
                                headText = ReadPackedReference(
                                    Path.Combine(git, "packed-refs"),
                                    reference);
                            }
                        }

                        if (IsGitObjectId(headText))
                        {
                            root = directory.FullName;
                            head = headText;
                            return true;
                        }
                    }

                    directory = directory.Parent;
                }
            }

            return false;
        }

        private static string ReadPackedReference(
            string packedRefsPath,
            string reference)
        {
            if (!File.Exists(packedRefsPath))
            {
                return null;
            }

            foreach (var line in File.ReadAllLines(packedRefsPath))
            {
                if (line.Length > 41
                    && line[40] == ' '
                    && string.Equals(
                        line.Substring(41),
                        reference,
                        StringComparison.Ordinal))
                {
                    return line.Substring(0, 40);
                }
            }

            return null;
        }

        private static bool IsGitObjectId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 40)
            {
                return false;
            }

            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if (!((character >= '0' && character <= '9')
                    || (character >= 'a' && character <= 'f')
                    || (character >= 'A' && character <= 'F')))
                {
                    return false;
                }
            }

            return true;
        }

        private static void RunLive(
            CallbackOwnershipWireOptions options,
            CallbackOwnershipWireReport report,
            FileStream reportStream)
        {
            switch (options.Scenario)
            {
                case CallbackOwnershipWireScenario.GdN10A:
                    RunGdN10A(options, report, reportStream);
                    return;
                case CallbackOwnershipWireScenario.GdN13Candidate:
                    RunGdN13(options, report, reportStream);
                    return;
                case CallbackOwnershipWireScenario.GdN14Candidate:
                    RunGdN14(options, report, reportStream);
                    return;
                default:
                    throw new InvalidOperationException(
                        "A concrete live scenario is required.");
            }
        }

        private static void RunGdN10A(
            CallbackOwnershipWireOptions options,
            CallbackOwnershipWireReport report,
            FileStream reportStream)
        {
            var cookie = CreateCookie();
            using (var ownerUdp = new CallbackOwnershipBoundUdp(
                options.OwnerLocalAddress,
                options.OwnerCallbackPort,
                "OWNER",
                report,
                reportStream))
            using (var owner = new CallbackOwnershipWireSession(
                options,
                report,
                reportStream,
                "OWNER",
                options.OwnerLocalAddress))
            {
                var actualPort = ownerUdp.EndPoint.Port;
                var registrationA = CreateRegistration(
                    options.OwnerLocalAddress,
                    actualPort,
                    cookie);
                var registrationB = CreateRegistration(
                    options.CandidateLocalAddress,
                    actualPort,
                    cookie);
                report.Add(
                    "GD_N10A_MISMATCH_B_CALLBACK_PORT_SOURCE",
                    "OWNER_CALLBACK_ENDPOINT_ACTUAL");
                report.Add(
                    "GD_N10A_MISMATCH_B_CALLBACK_PORT",
                    actualPort);
                report.WriteTo(reportStream);
                owner.Open();
                owner.Initialize();
                var accepted = owner.Register(
                    "GD_N10A_REGISTER_A",
                    registrationA,
                    true);
                owner.IsAuthoritative = true;

                var mismatchRequest = (byte[])accepted.Request.Clone();
                var mismatchBytes = registrationB.CallbackIPv4;
                Buffer.BlockCopy(mismatchBytes, 0, mismatchRequest, 16, 4);
                EnsureOnlyIpv4Differs(accepted.Request, mismatchRequest);
                owner.RegisterRaw(
                    "GD_N10A_REGISTER_B_MISMATCH",
                    mismatchRequest,
                    registrationB,
                    false);

                var duplicate = owner.RegisterRaw(
                    "GD_N10A_REGISTER_A_DUPLICATE",
                    (byte[])accepted.Request.Clone(),
                    registrationA,
                    true);
                EnsureSameFence(
                    accepted.Accepted,
                    duplicate.Accepted,
                    "gd-n10a duplicate");
                owner.CloseAuthoritative();
            }
        }

        private static void RunGdN13(
            CallbackOwnershipWireOptions options,
            CallbackOwnershipWireReport report,
            FileStream reportStream)
        {
            using (var ownerUdp = new CallbackOwnershipBoundUdp(
                options.OwnerLocalAddress,
                options.OwnerCallbackPort,
                "OWNER",
                report,
                reportStream))
            using (var candidateUdp = new CallbackOwnershipBoundUdp(
                options.CandidateLocalAddress,
                options.CandidateCallbackPort,
                "CANDIDATE",
                report,
                reportStream))
            using (var owner = new CallbackOwnershipWireSession(
                options,
                report,
                reportStream,
                "OWNER",
                options.OwnerLocalAddress))
            using (var candidate = new CallbackOwnershipWireSession(
                options,
                report,
                reportStream,
                "CANDIDATE",
                options.CandidateLocalAddress))
            {
                var ownerRegistration = CreateRegistration(
                    options.OwnerLocalAddress,
                    ownerUdp.EndPoint.Port,
                    CreateCookie());
                var candidateRegistration = CreateRegistration(
                    options.CandidateLocalAddress,
                    candidateUdp.EndPoint.Port,
                    CreateCookie());
                owner.Open();
                owner.Initialize();
                var oldFence = owner.Register(
                    "GD_N13_OWNER_REGISTER",
                    ownerRegistration,
                    true);
                owner.IsAuthoritative = true;

                candidate.Open();
                candidate.Initialize();
                var takeover = candidate.Register(
                    "GD_N13_CANDIDATE_REGISTER",
                    candidateRegistration,
                    true);
                EnsureEpochAdvanced(oldFence.Accepted, takeover.Accepted);
                owner.IsAuthoritative = false;
                candidate.IsAuthoritative = true;
                owner.ObserveOldOwnerPeerCloseAfterTakeover();
                owner.DisconnectWithoutClose(
                    "PEER_RETIREMENT_OBSERVED_AFTER_TAKEOVER");

                var duplicate = candidate.Register(
                    "GD_N13_CANDIDATE_DUPLICATE",
                    candidateRegistration,
                    true);
                EnsureSameFence(
                    takeover.Accepted,
                    duplicate.Accepted,
                    "gd-n13 replacement owner duplicate");
                candidate.CloseAuthoritative();
            }
        }

        private static void RunGdN14(
            CallbackOwnershipWireOptions options,
            CallbackOwnershipWireReport report,
            FileStream reportStream)
        {
            using (var ownerUdp = new CallbackOwnershipBoundUdp(
                options.OwnerLocalAddress,
                options.OwnerCallbackPort,
                "OWNER",
                report,
                reportStream))
            using (var candidateUdp = new CallbackOwnershipBoundUdp(
                options.CandidateLocalAddress,
                options.CandidateCallbackPort,
                "CANDIDATE",
                report,
                reportStream))
            using (var owner = new CallbackOwnershipWireSession(
                options,
                report,
                reportStream,
                "OWNER",
                options.OwnerLocalAddress))
            using (var candidate = new CallbackOwnershipWireSession(
                options,
                report,
                reportStream,
                "CANDIDATE",
                options.CandidateLocalAddress))
            {
                var ownerRegistration = CreateRegistration(
                    options.OwnerLocalAddress,
                    ownerUdp.EndPoint.Port,
                    CreateCookie());
                owner.Open();
                owner.Initialize();
                var original = owner.Register(
                    "GD_N14_OWNER_REGISTER",
                    ownerRegistration,
                    true);
                owner.IsAuthoritative = true;

                candidate.Open();
                candidate.ExpectPeerCloseOnInitialization();
                candidate.DisconnectWithoutClose(
                    "REJECTED_CANDIDATE_MUST_NOT_SEND_0x405D");

                var duplicate = owner.Register(
                    "GD_N14_OWNER_DUPLICATE",
                    ownerRegistration,
                    true);
                EnsureSameFence(
                    original.Accepted,
                    duplicate.Accepted,
                    "gd-n14 retained owner duplicate");
                owner.CloseAuthoritative();
            }
        }

        private static LMCCallbackRegistrationV2Request CreateRegistration(
            string callbackAddress,
            int callbackPort,
            ulong cookie)
        {
            var address = IPAddress.Parse(callbackAddress);
            if (address.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException(
                    "Callback registration requires IPv4.",
                    "callbackAddress");
            }

            return new LMCCallbackRegistrationV2Request(
                1,
                callbackPort,
                address.GetAddressBytes(),
                LMCCallbackProtocol.DatagramHeaderBytes,
                (uint)(cookie & uint.MaxValue),
                (uint)(cookie >> 32));
        }

        private static ulong CreateCookie()
        {
            var bytes = new byte[8];
            using (var random = RandomNumberGenerator.Create())
            {
                while (true)
                {
                    random.GetBytes(bytes);
                    var cookie = TestFrame.ReadUInt64(bytes, 0);
                    if (cookie != 0)
                    {
                        return cookie;
                    }
                }
            }
        }

        private static void EnsureOnlyIpv4Differs(
            byte[] original,
            byte[] mismatch)
        {
            if (original == null
                || mismatch == null
                || original.Length != mismatch.Length)
            {
                throw new InvalidDataException(
                    "GD-N10A registrations must have equal frame lengths.");
            }

            var differenceCount = 0;
            for (var index = 0; index < original.Length; index++)
            {
                if (original[index] == mismatch[index])
                {
                    continue;
                }

                differenceCount++;
                if (index < 16 || index > 19)
                {
                    throw new InvalidDataException(
                        "GD-N10A mismatch changed a field outside callback IPv4.");
                }
            }

            if (differenceCount == 0)
            {
                throw new InvalidDataException(
                    "GD-N10A mismatch did not change callback IPv4.");
            }
        }

        private static void EnsureSameFence(
            LMCCallbackRegistrationV2Response expected,
            LMCCallbackRegistrationV2Response actual,
            string operation)
        {
            if (expected == null
                || actual == null
                || expected.DiagnosticsBootId != actual.DiagnosticsBootId
                || expected.SessionEpoch != actual.SessionEpoch
                || expected.AcceptedMaxDatagram
                    != actual.AcceptedMaxDatagram)
            {
                throw new InvalidDataException(
                    operation
                    + " changed BootId, SessionEpoch, or accepted maximum.");
            }
        }

        private static void EnsureEpochAdvanced(
            LMCCallbackRegistrationV2Response previous,
            LMCCallbackRegistrationV2Response replacement)
        {
            if (previous == null
                || replacement == null
                || previous.DiagnosticsBootId
                    != replacement.DiagnosticsBootId
                || previous.SessionEpoch == replacement.SessionEpoch
                || replacement.AcceptedMaxDatagram
                    != LMCCallbackProtocol.DatagramHeaderBytes)
            {
                throw new InvalidDataException(
                    "GD-N13 takeover did not preserve BootId and advance SessionEpoch with max 52.");
            }
        }

        private static void EnsureByteIdentical(
            byte[] expected,
            byte[] actual,
            string operation)
        {
            if (expected.Length != actual.Length)
            {
                throw new InvalidDataException(
                    operation + " length is not canonical.");
            }

            for (var index = 0; index < expected.Length; index++)
            {
                if (expected[index] != actual[index])
                {
                    throw new InvalidDataException(
                        operation + " bytes are not canonical.");
                }
            }
        }
    }
}

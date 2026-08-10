using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace LasalMotionControlLib.Tests
{
    internal static class CallbackOwnershipWireToolTests
    {
        private const ulong GoldenCookie = 0x11223344AABBCCDDUL;
        private static readonly string ValidSourceFingerprint =
            new string('a', 40)
            + "/"
            + new string('b', 40)
            + "/"
            + new string('c', 40);

        private sealed class ToolRunResult
        {
            internal int ExitCode;
            internal string StandardOutput;
            internal string StandardError;
        }

        private sealed class TemporaryReportDirectory : IDisposable
        {
            internal TemporaryReportDirectory()
            {
                Path = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    "LmcCallbackOwnership-"
                    + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(Path);
            }

            internal string Path { get; private set; }

            internal string NewReportPath(string name)
            {
                return System.IO.Path.Combine(Path, name + ".txt");
            }

            public void Dispose()
            {
                if (!Directory.Exists(Path))
                {
                    return;
                }

                foreach (var file in Directory.GetFiles(Path))
                {
                    File.Delete(file);
                }

                Directory.Delete(Path);
            }
        }

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "CallbackOwnershipWire.InvocationAndDryRunAreExact",
                InvocationAndDryRunAreExact);
            tests.Add(
                "CallbackOwnershipWire.LiveGuardAndScenarioRelationsAreExact",
                LiveGuardAndScenarioRelationsAreExact);
            tests.Add(
                "CallbackOwnershipWire.DangerousAndArbitraryInputsAreAbsent",
                DangerousAndArbitraryInputsAreAbsent);
            tests.Add(
                "CallbackOwnershipWire.OutputPreflightPrecedesNetwork",
                OutputPreflightPrecedesNetwork);
            tests.Add(
                "CallbackOwnershipWire.SourceFingerprintPreflightPrecedesNetwork",
                SourceFingerprintPreflightPrecedesNetwork);
            tests.Add(
                "CallbackOwnershipWire.AtomicReportReservationDoesNotOverwrite",
                AtomicReportReservationDoesNotOverwrite);
            tests.Add(
                "CallbackOwnershipWire.FixedRequestGoldenAndAllowlist",
                FixedRequestGoldenAndAllowlist);
            tests.Add(
                "CallbackOwnershipWire.RegistrationEnvelopesAreCanonical",
                RegistrationEnvelopesAreCanonical);
            tests.Add(
                "CallbackOwnershipWire.PeerCloseClassificationIsStrict",
                PeerCloseClassificationIsStrict);
            tests.Add(
                "CallbackOwnershipWire.GdN10ASameSocketMismatchAndDuplicate",
                GdN10ASameSocketMismatchAndDuplicate);
            tests.Add(
                "CallbackOwnershipWire.GdN13SameIpTakeoverLateDisconnect",
                GdN13SameIpTakeoverLateDisconnect);
            tests.Add(
                "CallbackOwnershipWire.GdN13MissingRetireBarrierIsInconclusive",
                GdN13MissingRetireBarrierIsInconclusive);
            tests.Add(
                "CallbackOwnershipWire.GdN14DifferentIpPeerCloseRetainsOwner",
                GdN14DifferentIpPeerCloseRetainsOwner);
            tests.Add(
                "CallbackOwnershipWire.GdN14ImmediateCleanEofRetainsOwner",
                GdN14ImmediateCleanEofRetainsOwner);
            tests.Add(
                "CallbackOwnershipWire.GdN14TimeoutIsInconclusive",
                GdN14TimeoutIsInconclusive);
            tests.Add(
                "CallbackOwnershipWire.VerificationFailureReportIsPreserved",
                VerificationFailureReportIsPreserved);
        }

        private static void InvocationAndDryRunAreExact()
        {
            AssertEx.True(CallbackOwnershipWireTool.IsInvocation(
                new[] { "callback-ownership-wire" }));
            AssertEx.False(CallbackOwnershipWireTool.IsInvocation(
                new[] { "Callback-Ownership-Wire" }));

            var options = CallbackOwnershipWireOptions.Parse(
                new[] { "callback-ownership-wire" });
            AssertEx.False(options.ExecuteLive);
            AssertEx.Equal(CallbackOwnershipWireScenario.All, options.Scenario);
            AssertEx.Equal(0, options.OwnerCallbackPort);
            AssertEx.Equal(0, options.CandidateCallbackPort);

            using (var server = new FakeCallbackOwnershipServer())
            {
                var result = CaptureRun(
                    new[] { "callback-ownership-wire", "--dry-run" });
                AssertEx.Equal(CallbackOwnershipWireTool.SuccessExitCode,
                    result.ExitCode);
                AssertEx.Contains("NETWORK_CONNECTED=FALSE", result.StandardOutput);
                AssertEx.Contains(
                    "QUALIFICATION_RESULT=INCOMPLETE_WITHOUT_PCAP_AND_PLC_WATCH",
                    result.StandardOutput);
                Thread.Sleep(50);
                AssertEx.Equal(0, server.AcceptedClientCount);
            }
        }

        private static void LiveGuardAndScenarioRelationsAreExact()
        {
            using (var reports = new TemporaryReportDirectory())
            {
                var output = reports.NewReportPath("unused");
                AssertParseFails(new[]
                {
                    "callback-ownership-wire", "--execute-live",
                    "--scenario", "gd-n10a", "--host", "127.0.0.1",
                    "--owner-local", "127.0.0.1",
                    "--candidate-local", "127.0.0.2", "--output", output
                });
                AssertParseFails(new[]
                {
                    "callback-ownership-wire", "--execute-live",
                    "--confirm", "plc-callback-ownership",
                    "--scenario", "gd-n10a", "--host", "127.0.0.1",
                    "--owner-local", "127.0.0.1",
                    "--candidate-local", "127.0.0.2", "--output", output
                });
                AssertParseFails(new[]
                {
                    "callback-ownership-wire", "--execute-live",
                    "--confirm", CallbackOwnershipWireOptions.LiveConfirmation,
                    "--scenario", "all", "--host", "127.0.0.1",
                    "--owner-local", "127.0.0.1",
                    "--candidate-local", "127.0.0.2", "--output", output
                });
                AssertParseFails(LiveArgs(
                    "gd-n13-candidate", 4000, "127.0.0.1",
                    "127.0.0.2", output));
                AssertParseFails(LiveArgs(
                    "gd-n14-candidate", 4000, "127.0.0.1",
                    "127.0.0.1", output));
                AssertParseFails(LiveArgs(
                    "gd-n10a", 4000, "127.0.0.1",
                    "127.0.0.1", output));

                var exact = CallbackOwnershipWireOptions.Parse(LiveArgs(
                    "gd-n10a", 4000, "127.0.0.1",
                    "127.0.0.2", output));
                AssertEx.True(exact.ExecuteLive);
                AssertEx.Equal(3000, exact.TimeoutMilliseconds);
                AssertEx.Equal(System.IO.Path.GetFullPath(output),
                    exact.OutputPath);
                AssertEx.Equal(ValidSourceFingerprint,
                    exact.SourceFingerprint);

                AssertParseFails(LiveArgs(
                    "gd-n10a", 0, "127.0.0.1",
                    "127.0.0.2", output));

                var nonzeroUnusedCandidatePort = new List<string>(LiveArgs(
                    "gd-n10a", 4000, "127.0.0.1",
                    "127.0.0.2", output));
                ReplaceOptionValue(
                    nonzeroUnusedCandidatePort,
                    "--candidate-callback-port",
                    "5004");
                AssertParseFails(nonzeroUnusedCandidatePort.ToArray());

                var tooShort = new List<string>(LiveArgs(
                    "gd-n10a", 4000, "127.0.0.1",
                    "127.0.0.2", output));
                tooShort.Add("--timeout-ms");
                tooShort.Add("249");
                AssertParseFails(tooShort.ToArray());
                var tooLong = new List<string>(LiveArgs(
                    "gd-n10a", 4000, "127.0.0.1",
                    "127.0.0.2", output));
                tooLong.Add("--timeout-ms");
                tooLong.Add("10001");
                AssertParseFails(tooLong.ToArray());
            }
        }

        private static void DangerousAndArbitraryInputsAreAbsent()
        {
            foreach (var prohibited in new[]
            {
                "--command", "--payload", "--retry", "--downgrade",
                "--write", "--motion", "--reset", "--download"
            })
            {
                AssertParseFails(new[]
                {
                    "callback-ownership-wire", "--dry-run", prohibited, "1"
                });
            }

            AssertParseFails(new[]
            {
                "callback-ownership-wire", "--dry-run", "--output", "x.txt"
            });
            AssertParseFails(new[]
            {
                "callback-ownership-wire", "--dry-run", "--port", "4000"
            });
            AssertParseFails(new[]
            {
                "callback-ownership-wire", "--dry-run", "--dry-run"
            });
            AssertParseFails(new[]
            {
                "Callback-Ownership-Wire", "--dry-run"
            });
        }

        private static void OutputPreflightPrecedesNetwork()
        {
            using (var reports = new TemporaryReportDirectory())
            using (var server = new FakeCallbackOwnershipServer())
            {
                var output = reports.NewReportPath("existing");
                File.WriteAllText(output, "sentinel");
                var result = CaptureRun(LiveArgs(
                    "gd-n10a", server.Port, "127.0.0.1",
                    "127.0.0.2", output));
                AssertEx.Equal(
                    CallbackOwnershipWireTool.ReportFailureExitCode,
                    result.ExitCode);
                Thread.Sleep(50);
                AssertEx.Equal(0, server.AcceptedClientCount);
                AssertEx.Equal("sentinel", File.ReadAllText(output));
                AssertEx.Contains(
                    "before network access",
                    result.StandardError);
            }
        }

        private static void AtomicReportReservationDoesNotOverwrite()
        {
            using (var reports = new TemporaryReportDirectory())
            {
                var output = reports.NewReportPath("reserved");
                string inProgress;
                using (var stream = CallbackOwnershipWireTool
                    .ReserveLiveReport(output, out inProgress))
                {
                    AssertEx.False(File.Exists(output));
                    AssertEx.True(File.Exists(inProgress));
                    AssertEx.Equal(0L, stream.Length);
                }

                File.Delete(inProgress);
                File.WriteAllText(output, "sentinel");
                AssertEx.Throws<IOException>(() =>
                {
                    string ignored;
                    using (CallbackOwnershipWireTool.ReserveLiveReport(
                        output,
                        out ignored))
                    {
                    }
                });
                AssertEx.Equal("sentinel", File.ReadAllText(output));
            }
        }

        private static void SourceFingerprintPreflightPrecedesNetwork()
        {
            using (var reports = new TemporaryReportDirectory())
            using (var server = new FakeCallbackOwnershipServer())
            {
                var missingOutput = reports.NewReportPath(
                    "missing-fingerprint");
                var missing = new List<string>(LiveArgs(
                    "gd-n10a", server.Port, "127.0.0.1",
                    "127.0.0.2", missingOutput));
                RemoveOptionPair(missing, "--source-fingerprint");
                var missingResult = CaptureRun(missing.ToArray());
                AssertEx.Equal(
                    CallbackOwnershipWireTool.UsageExitCode,
                    missingResult.ExitCode);
                AssertEx.False(File.Exists(missingOutput));

                var malformedOutput = reports.NewReportPath(
                    "malformed-fingerprint");
                var malformed = new List<string>(LiveArgs(
                    "gd-n10a", server.Port, "127.0.0.1",
                    "127.0.0.2", malformedOutput));
                ReplaceOptionValue(
                    malformed,
                    "--source-fingerprint",
                    new string('a', 39)
                        + "/"
                        + new string('b', 40)
                        + "/"
                        + new string('g', 40));
                var malformedResult = CaptureRun(malformed.ToArray());
                AssertEx.Equal(
                    CallbackOwnershipWireTool.UsageExitCode,
                    malformedResult.ExitCode);
                AssertEx.False(File.Exists(malformedOutput));

                Thread.Sleep(50);
                AssertEx.Equal(0, server.AcceptedClientCount);
            }
        }

        private static void FixedRequestGoldenAndAllowlist()
        {
            var request = CallbackOwnershipWireTool.CreateRegistrationRequest(
                "192.0.2.20",
                12345,
                GoldenCookie);
            AssertEx.Equal(40, request.Length);
            AssertEx.Equal((ushort)0x405C, TestFrame.ReadUInt16(request, 0));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 2));
            AssertEx.Equal((ushort)32, TestFrame.ReadUInt16(request, 4));
            AssertEx.Equal((ushort)0, TestFrame.ReadUInt16(request, 6));
            AssertEx.Equal((uint)1, TestFrame.ReadUInt32(request, 8));
            AssertEx.Equal(12345, TestFrame.ReadInt32(request, 12));
            AssertEx.SequenceEqual(
                new byte[] { 192, 0, 2, 20 },
                request.Skip(16).Take(4).ToArray());
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 20));
            AssertEx.Equal((ushort)52, TestFrame.ReadUInt16(request, 22));
            AssertEx.Equal(GoldenCookie, TestFrame.ReadUInt64(request, 24));
            AssertEx.Equal((uint)0, TestFrame.ReadUInt32(request, 32));
            AssertEx.Equal((uint)0, TestFrame.ReadUInt32(request, 36));
            CallbackOwnershipWireTool.EnsureAllowedRequest(request);
            CallbackOwnershipWireTool.EnsureAllowedRequest(
                LMC_Frame.RpcSessionInit());
            CallbackOwnershipWireTool.EnsureAllowedRequest(
                LMC_Frame.CloseConnection());
            AssertEx.SequenceEqual(
                TestFrame.Hex("80 80 00 00 01 00 00 00 00"),
                LMC_Frame.RpcSessionInit(),
                "0x8080 golden request changed.");
            AssertEx.SequenceEqual(
                TestFrame.Hex("5D 40 00 00 01 00 00 00 00"),
                LMC_Frame.CloseConnection(),
                "0x405D golden request changed.");

            var wrongMaximum = (byte[])request.Clone();
            TestFrame.WriteUInt16(wrongMaximum, 22, 53);
            AssertEx.Throws<InvalidOperationException>(() =>
                CallbackOwnershipWireTool.EnsureAllowedRequest(wrongMaximum));
            var wrongFlags = (byte[])request.Clone();
            TestFrame.WriteUInt32(wrongFlags, 32, 1);
            AssertEx.Throws<InvalidOperationException>(() =>
                CallbackOwnershipWireTool.EnsureAllowedRequest(wrongFlags));
            AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                CallbackOwnershipWireTool.CreateRegistrationRequest(
                    "192.0.2.20", 12345, 0));
            AssertEx.Throws<InvalidOperationException>(() =>
                CallbackOwnershipWireTool.EnsureAllowedRequest(
                    LMC_Frame.CreateRequest(LMC_CommandId.GroupStop, 0, 0)));
        }

        private static void RegistrationEnvelopesAreCanonical()
        {
            var request = CallbackOwnershipWireTool.CreateRegistrationRequest(
                "192.0.2.20",
                12345,
                GoldenCookie);
            var successPayload = new byte[20];
            TestFrame.WriteUInt16(successPayload, 4, 2);
            TestFrame.WriteUInt16(successPayload, 6, 52);
            TestFrame.WriteUInt32(successPayload, 8, 0x10203040u);
            TestFrame.WriteUInt32(successPayload, 12, 0x50607080u);
            var success = LMCConnection.ParseCallbackRegistrationV2Envelope(
                TestFrame.Response(0, successPayload));
            AssertEx.True(success.IsSuccess);

            var registrationPayload = request.Skip(8).ToArray();
            var registration = LMCCallbackProtocol.ParseRegistrationV2Payload(
                registrationPayload,
                new byte[] { 192, 0, 2, 20 },
                LMCCallbackProtocolPolicy.InitialV2WakeHint);
            AssertEx.True(registration.IsAccepted);
            var parsed = LMCCallbackProtocol.ParseRegistrationV2Response(
                success.Payload,
                registration.Value,
                new byte[] { 192, 0, 2, 10 },
                1,
                LMCCallbackProtocolPolicy.InitialV2WakeHint);
            AssertEx.True(parsed.IsAccepted);
            AssertEx.Equal((uint)0x10203040, parsed.Value.DiagnosticsBootId);
            AssertEx.Equal((uint)0x50607080, parsed.Value.SessionEpoch);

            var failurePayload = new byte[20];
            TestFrame.WriteUInt16(failurePayload, 0, 1);
            TestFrame.WriteInt16(failurePayload, 2, -1);
            var failure = LMCConnection.ParseCallbackRegistrationV2Envelope(
                TestFrame.Response(0, failurePayload));
            AssertEx.False(failure.IsSuccess);
            AssertEx.Equal((ushort)1, failure.CommandStatus);
            AssertEx.Equal((short)-1, failure.ErrorId);
            failurePayload[4] = 1;
            AssertEx.Throws<InvalidDataException>(() =>
                LMCConnection.ParseCallbackRegistrationV2Envelope(
                    TestFrame.Response(0, failurePayload)));
            AssertEx.Throws<InvalidDataException>(() =>
                LMCConnection.ParseCallbackRegistrationV2Envelope(
                    TestFrame.Response(1, new byte[20])));
            AssertEx.Throws<InvalidDataException>(() =>
                LMCConnection.ParseCallbackRegistrationV2Envelope(
                    TestFrame.Response(0, new byte[19])));
        }

        private static void PeerCloseClassificationIsStrict()
        {
            var reset = new SocketException(
                (int)SocketError.ConnectionReset);
            var aborted = new SocketException(
                (int)SocketError.ConnectionAborted);
            var shutdown = new SocketException(
                (int)SocketError.Shutdown);
            var timeout = new SocketException(
                (int)SocketError.TimedOut);

            AssertEx.True(CallbackOwnershipWireTool.IsPeerClose(reset));
            AssertEx.True(CallbackOwnershipWireTool.IsPeerClose(
                new IOException("wrapped", reset)));
            AssertEx.False(CallbackOwnershipWireTool.IsPeerClose(aborted));
            AssertEx.False(CallbackOwnershipWireTool.IsPeerClose(shutdown));
            AssertEx.False(CallbackOwnershipWireTool.IsPeerClose(timeout));
            AssertEx.True(CallbackOwnershipWireTool
                .IsInconclusivePeerTermination(aborted));
            AssertEx.True(CallbackOwnershipWireTool
                .IsInconclusivePeerTermination(shutdown));
            AssertEx.True(CallbackOwnershipWireTool
                .IsInconclusivePeerTermination(timeout));
            AssertEx.False(CallbackOwnershipWireTool
                .IsInconclusivePeerTermination(reset));
        }

        private static void GdN10ASameSocketMismatchAndDuplicate()
        {
            using (var reports = new TemporaryReportDirectory())
            using (var server = new FakeCallbackOwnershipServer())
            {
                var output = reports.NewReportPath("gd-n10a");
                var result = CaptureRun(LiveArgs(
                    "gd-n10a", server.Port, "127.0.0.1",
                    "127.0.0.2", output));
                AssertEx.Equal(CallbackOwnershipWireTool.SuccessExitCode,
                    result.ExitCode);
                server.Verify(5);
                var requests = server.Requests;
                AssertCommands(requests,
                    0x8080, 0x405C, 0x405C, 0x405C, 0x405D);
                AssertEx.True(requests.All(
                    request => request.SessionOrdinal == 1));
                AssertEx.SequenceEqual(
                    requests[1].Frame,
                    requests[3].Frame,
                    "GD-N10A duplicate must be byte-identical.");
                AssertOnlyIpv4Differs(
                    requests[1].Frame,
                    requests[2].Frame);
                AssertEx.Equal(0, server.TakeoverCount);
                AssertEx.Equal(0, server.RejectCount);
                AssertEx.Equal(1, server.OwnerCloseCount);

                var report = File.ReadAllText(output);
                AssertCommonReport(report, "gd-n10a");
                AssertEx.Contains("OWNER_UDP_BOUND_ENDPOINT=127.0.0.1:", report);
                AssertEx.Contains(
                    "GD_N10A_REGISTER_B_MISMATCH_RESULT=EXPECTED_FAILURE_PASS",
                    report);
                AssertEx.Contains(
                    "GD_N10A_MISMATCH_B_CALLBACK_PORT_SOURCE=OWNER_CALLBACK_ENDPOINT_ACTUAL",
                    report);
                AssertEx.Contains("OVERALL_RESULT=PASS", report);
            }
        }

        private static void GdN13SameIpTakeoverLateDisconnect()
        {
            using (var reports = new TemporaryReportDirectory())
            using (var server = new FakeCallbackOwnershipServer())
            {
                var output = reports.NewReportPath("gd-n13");
                var result = CaptureRun(LiveArgs(
                    "gd-n13-candidate", server.Port, "127.0.0.1",
                    "127.0.0.1", output));
                AssertEx.Equal(CallbackOwnershipWireTool.SuccessExitCode,
                    result.ExitCode);
                server.Verify(6);
                var requests = server.Requests;
                AssertCommands(requests,
                    0x8080, 0x405C, 0x8080, 0x405C, 0x405C, 0x405D);
                AssertEx.Equal(1, requests[0].SessionOrdinal);
                AssertEx.Equal(1, requests[1].SessionOrdinal);
                for (var index = 2; index < requests.Count; index++)
                {
                    AssertEx.Equal(2, requests[index].SessionOrdinal);
                }
                AssertEx.Equal(1, server.TakeoverCount);
                AssertEx.Equal(0, server.RejectCount);
                AssertEx.True(server.LateNonOwnerDisconnectCount >= 1);
                AssertEx.Equal(1, server.OwnerCloseCount);
                AssertEx.Equal(1, server.OldOwnerRetireBarrierCount);

                var events = server.Events.ToList();
                var takeoverRequest = events.IndexOf(
                    "REQUEST:2:0x405C");
                var oldDisconnect = events.IndexOf("DISCONNECT:1");
                var barrier = events.IndexOf("TAKEOVER_BARRIER:1");
                var takeoverResponse = events.IndexOf(
                    "RESPONSE:2:0x405C");
                var duplicateRequest = IndexOfAfter(
                    events,
                    "REQUEST:2:0x405C",
                    takeoverRequest + 1);
                AssertEx.True(takeoverRequest >= 0);
                AssertEx.True(oldDisconnect > takeoverRequest);
                AssertEx.True(barrier > oldDisconnect);
                AssertEx.True(takeoverResponse > barrier);
                AssertEx.True(duplicateRequest > takeoverResponse);

                var report = File.ReadAllText(output);
                AssertCommonReport(report, "gd-n13-candidate");
                AssertEx.Contains(
                    "OWNER_TRANSPORT_DISCONNECT_WITHOUT_405D=PEER_RETIREMENT_OBSERVED_AFTER_TAKEOVER",
                    report);
                AssertEx.Contains(
                    "OWNER_TAKEOVER_RETIRE_RESULT=",
                    report);
                AssertEx.Contains(
                    "GD_N13_CANDIDATE_DUPLICATE_RESULT=PASS",
                    report);
                AssertEx.Contains("OVERALL_RESULT=PASS", report);
            }
        }

        private static void GdN14DifferentIpPeerCloseRetainsOwner()
        {
            using (var reports = new TemporaryReportDirectory())
            using (var server = new FakeCallbackOwnershipServer())
            {
                var output = reports.NewReportPath("gd-n14");
                var result = CaptureRun(LiveArgs(
                    "gd-n14-candidate", server.Port, "127.0.0.1",
                    "127.0.0.2", output));
                AssertEx.Equal(CallbackOwnershipWireTool.SuccessExitCode,
                    result.ExitCode);
                server.Verify(5);
                var requests = server.Requests;
                AssertCommands(requests,
                    0x8080, 0x405C, 0x8080, 0x405C, 0x405D);
                AssertEx.Equal(2, requests[2].SessionOrdinal);
                AssertEx.Equal((ushort)0x8080, requests[2].Command);
                AssertEx.False(requests.Any(request =>
                    request.SessionOrdinal == 2
                    && (request.Command == 0x405C
                        || request.Command == 0x405D)));
                AssertEx.Equal(0, server.TakeoverCount);
                AssertEx.Equal(1, server.RejectCount);
                AssertEx.Equal(1, server.OwnerCloseCount);

                var report = File.ReadAllText(output);
                AssertCommonReport(report, "gd-n14-candidate");
                AssertEx.Contains(
                    "CANDIDATE_REJECTION_RESULT=EXPECTED_PEER_CLOSE_PASS",
                    report);
                AssertEx.Contains(
                    "GD_N14_OWNER_DUPLICATE_RESULT=PASS",
                    report);
                AssertEx.Contains("OVERALL_RESULT=PASS", report);
            }
        }

        private static void GdN13MissingRetireBarrierIsInconclusive()
        {
            using (var reports = new TemporaryReportDirectory())
            using (var server = new FakeCallbackOwnershipServer(
                false,
                false,
                true))
            {
                var output = reports.NewReportPath("gd-n13-no-barrier");
                var arguments = new List<string>(LiveArgs(
                    "gd-n13-candidate", server.Port, "127.0.0.1",
                    "127.0.0.1", output));
                arguments.Add("--timeout-ms");
                arguments.Add("250");
                var result = CaptureRun(arguments.ToArray());
                AssertEx.Equal(
                    CallbackOwnershipWireTool.VerificationFailureExitCode,
                    result.ExitCode);
                server.Verify(4);
                AssertCommands(server.Requests,
                    0x8080, 0x405C, 0x8080, 0x405C);
                AssertEx.Equal(0, server.OldOwnerRetireBarrierCount);
                var report = File.ReadAllText(output);
                AssertEx.Contains(
                    "OWNER_TAKEOVER_RETIRE_RESULT=INCONCLUSIVE_TRANSPORT_TERMINATION",
                    report);
                AssertEx.Contains("OVERALL_RESULT=INCONCLUSIVE", report);
                AssertEx.Contains("TOOL_RESULT=INCONCLUSIVE", report);
                AssertEx.False(report.Contains(
                    "GD_N13_CANDIDATE_DUPLICATE_REQUEST_BYTES="));
            }
        }

        private static void GdN14TimeoutIsInconclusive()
        {
            using (var reports = new TemporaryReportDirectory())
            using (var server = new FakeCallbackOwnershipServer(true))
            {
                var output = reports.NewReportPath("gd-n14-timeout");
                var arguments = new List<string>(LiveArgs(
                    "gd-n14-candidate", server.Port, "127.0.0.1",
                    "127.0.0.2", output));
                arguments.Add("--timeout-ms");
                arguments.Add("250");
                var result = CaptureRun(arguments.ToArray());
                AssertEx.Equal(
                    CallbackOwnershipWireTool.VerificationFailureExitCode,
                    result.ExitCode);
                server.Verify(3);
                var requests = server.Requests;
                AssertCommands(requests, 0x8080, 0x405C, 0x8080);
                AssertEx.False(requests.Any(request =>
                    request.SessionOrdinal == 2
                    && (request.Command == 0x405C
                        || request.Command == 0x405D)));
                AssertEx.Equal(0, server.RejectCount);
                AssertEx.True(File.Exists(output));
                var report = File.ReadAllText(output);
                AssertEx.Contains(
                    "CANDIDATE_REJECTION_PROBE_REQUEST_SHA256=",
                    report);
                AssertEx.Contains(
                    "CANDIDATE_REJECTION_RESULT=INCONCLUSIVE_TRANSPORT_TERMINATION",
                    report);
                AssertEx.Contains("OVERALL_RESULT=INCONCLUSIVE", report);
                AssertEx.Contains("TOOL_RESULT=INCONCLUSIVE", report);
                AssertEx.Contains(
                    typeof(CallbackOwnershipInconclusiveException).FullName,
                    report);
            }
        }

        private static void GdN14ImmediateCleanEofRetainsOwner()
        {
            using (var reports = new TemporaryReportDirectory())
            using (var server = new FakeCallbackOwnershipServer(false, true))
            {
                var output = reports.NewReportPath("gd-n14-accept-close");
                var result = CaptureRun(LiveArgs(
                    "gd-n14-candidate", server.Port, "127.0.0.1",
                    "127.0.0.2", output));
                AssertEx.Equal(CallbackOwnershipWireTool.SuccessExitCode,
                    result.ExitCode);
                server.Verify(5);
                AssertEx.Equal(2, server.AcceptedClientCount);
                var candidateRequests = server.Requests
                    .Where(request => request.SessionOrdinal == 2)
                    .ToArray();
                AssertEx.Equal(1, candidateRequests.Length);
                AssertEx.Equal((ushort)0x8080,
                    candidateRequests[0].Command);
                var ownerCommands = server.Requests
                    .Where(request => request.SessionOrdinal == 1)
                    .Select(request => request.Command)
                    .ToArray();
                AssertEx.Equal(4, ownerCommands.Length);
                AssertEx.Equal((ushort)0x8080, ownerCommands[0]);
                AssertEx.Equal((ushort)0x405C, ownerCommands[1]);
                AssertEx.Equal((ushort)0x405C, ownerCommands[2]);
                AssertEx.Equal((ushort)0x405D, ownerCommands[3]);
                AssertEx.Equal(1, server.RejectCount);
                AssertEx.Equal(1, server.OwnerCloseCount);
                var report = File.ReadAllText(output);
                AssertEx.Contains(
                    "CANDIDATE_REJECTION_RESULT=EXPECTED_PEER_CLOSE_PASS",
                    report);
                AssertEx.Contains(
                    "GD_N14_OWNER_DUPLICATE_RESULT=PASS",
                    report);
                AssertEx.Contains("TOOL_RESULT=PASS", report);
            }
        }

        private static void VerificationFailureReportIsPreserved()
        {
            using (var reports = new TemporaryReportDirectory())
            using (var server = new FakeRpcServer(
                new FakeRpcStep(
                    0x8080,
                    TestFrame.Response(0, new byte[4]))))
            {
                var output = reports.NewReportPath("failure");
                var result = CaptureRun(LiveArgs(
                    "gd-n10a", server.Port, "127.0.0.1",
                    "127.0.0.2", output));
                AssertEx.Equal(
                    CallbackOwnershipWireTool.VerificationFailureExitCode,
                    result.ExitCode);
                server.Verify();
                AssertEx.True(File.Exists(output));
                var report = File.ReadAllText(output);
                AssertEx.Contains("OWNER_INIT_REQUEST_BYTES=9", report);
                AssertEx.Contains("OWNER_INIT_RESPONSE_BYTES=12", report);
                AssertEx.Contains("OVERALL_RESULT=FAIL", report);
                AssertEx.Contains("EXCEPTION_TYPE=", report);
                AssertEx.Equal(0, Directory.GetFiles(
                    reports.Path,
                    "*.inprogress-*.tmp").Length);
            }
        }

        private static string[] LiveArgs(
            string scenario,
            int port,
            string ownerLocal,
            string candidateLocal,
            string output)
        {
            return new[]
            {
                "callback-ownership-wire",
                "--execute-live",
                "--confirm",
                CallbackOwnershipWireOptions.LiveConfirmation,
                "--scenario",
                scenario,
                "--host",
                "127.0.0.1",
                "--port",
                port.ToString(),
                "--owner-local",
                ownerLocal,
                "--candidate-local",
                candidateLocal,
                "--owner-callback-port",
                "0",
                "--candidate-callback-port",
                "0",
                "--source-fingerprint",
                ValidSourceFingerprint,
                "--output",
                output
            };
        }

        private static void AssertParseFails(string[] args)
        {
            AssertEx.Throws<ArgumentException>(() =>
                CallbackOwnershipWireOptions.Parse(args));
        }

        private static void ReplaceOptionValue(
            IList<string> arguments,
            string option,
            string value)
        {
            var index = arguments.IndexOf(option);
            AssertEx.True(index >= 0 && index + 1 < arguments.Count);
            arguments[index + 1] = value;
        }

        private static void RemoveOptionPair(
            IList<string> arguments,
            string option)
        {
            var index = arguments.IndexOf(option);
            AssertEx.True(index >= 0 && index + 1 < arguments.Count);
            arguments.RemoveAt(index + 1);
            arguments.RemoveAt(index);
        }

        private static int IndexOfAfter(
            IList<string> values,
            string value,
            int startIndex)
        {
            for (var index = startIndex; index < values.Count; index++)
            {
                if (string.Equals(
                    values[index],
                    value,
                    StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static ToolRunResult CaptureRun(string[] args)
        {
            var originalOut = Console.Out;
            var originalError = Console.Error;
            var standardOut = new StringWriter();
            var standardError = new StringWriter();
            try
            {
                Console.SetOut(standardOut);
                Console.SetError(standardError);
                return new ToolRunResult
                {
                    ExitCode = CallbackOwnershipWireTool.Run(args),
                    StandardOutput = standardOut.ToString(),
                    StandardError = standardError.ToString()
                };
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }

        private static void AssertCommands(
            IList<FakeCallbackOwnershipRequest> requests,
            params ushort[] expected)
        {
            AssertEx.Equal(expected.Length, requests.Count);
            for (var index = 0; index < expected.Length; index++)
            {
                AssertEx.Equal(expected[index], requests[index].Command);
            }
        }

        private static void AssertOnlyIpv4Differs(
            byte[] original,
            byte[] mismatch)
        {
            AssertEx.Equal(original.Length, mismatch.Length);
            var differences = 0;
            for (var index = 0; index < original.Length; index++)
            {
                if (original[index] == mismatch[index])
                {
                    continue;
                }

                differences++;
                AssertEx.True(index >= 16 && index <= 19,
                    "Only callback IPv4 may differ.");
            }

            AssertEx.True(differences > 0);
        }

        private static void AssertCommonReport(
            string report,
            string scenario)
        {
            AssertEx.Contains("FORMAT=LMC_CALLBACK_OWNERSHIP_WIRE_V1", report);
            AssertEx.Contains("SCENARIO=" + scenario, report);
            AssertEx.Contains("EVIDENCE_CLASS=PC_RAW_WIRE_HARNESS", report);
            AssertEx.Contains("PCAP_EVIDENCE=NOT_CAPTURED_BY_TOOL", report);
            AssertEx.Contains(
                "QUALIFICATION_RESULT=INCOMPLETE_WITHOUT_PCAP_AND_PLC_WATCH",
                report);
            AssertEx.Contains("RETRY_COUNT=0", report);
            AssertEx.Contains("COMMAND_ALLOWLIST=0x8080,0x405C,0x405D", report);
            AssertEx.Contains("EXECUTABLE_SHA256=", report);
            AssertEx.Contains("CHECKPOINT_IDENTITY=", report);
            AssertEx.Contains(
                "SOURCE_FINGERPRINT_DECLARED=" + ValidSourceFingerprint,
                report);
            AssertEx.Contains("TOOL_RESULT=PASS", report);
            AssertEx.Contains("_REQUEST_SHA256=", report);
            AssertEx.Contains("_RESPONSE_SHA256=", report);
        }
    }
}

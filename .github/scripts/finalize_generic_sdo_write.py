from pathlib import Path


def replace_once(path, old, new, label):
    text = path.read_text(encoding="utf-8-sig")
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{label}: expected 1 match, got {count}")
    path.write_text(text.replace(old, new), encoding="utf-8")


proof = Path("LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/SdoWriteActivationQualificationProof.cs")
replace_once(
    proof,
    """        internal bool MatchesCurrent(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target)
        {
            return MatchesTargetTuple(target)
                && MatchesCurrent(connection, capabilities);
        }
""",
    """        internal bool MatchesCurrent(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target)
        {
            // Compatibility overload only. The qualification target is
            // capture provenance for the known canary round trip; once
            // captured, manual SDO Write admission is scoped to the
            // current connection/session and diagnostics transport
            // identity, not to the canary ObjectIndex tuple.
            return MatchesCurrent(connection, capabilities);
        }
""",
    "transport-scoped proof compatibility overload",
)

smoke = Path("LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/SdoWriteActivationQualificationProofTests.cs")
text = smoke.read_text(encoding="utf-8-sig")
replacements = [
    (
        '"Wpf.SdoWriteActivationProof.TargetTupleMismatch"',
        '"Wpf.SdoWriteActivationProof.QualificationTargetDoesNotScopeManualWrite"',
        "smoke registration",
    ),
    (
        "TargetTupleMismatchFailsClosed",
        "QualificationTargetDoesNotScopeManualWrite",
        "smoke method name",
    ),
    (
        """                    AssertEx.False(
                        proof.MatchesCurrent(
                            connection,
                            capabilities,
                            mismatch));
""",
        """                    AssertEx.True(
                        proof.MatchesCurrent(
                            connection,
                            capabilities,
                            mismatch));
""",
        "target mismatch expectation",
    ),
    (
        """                AssertEx.False(
                    proof.MatchesCurrent(
                        connection,
                        capabilities,
                        dataLengthMismatch));
""",
        """                AssertEx.True(
                    proof.MatchesCurrent(
                        connection,
                        capabilities,
                        dataLengthMismatch));
""",
        "data length mismatch expectation",
    ),
]
for old, new, label in replacements:
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{label}: expected 1 match, got {count}")
    text = text.replace(old, new)
smoke.write_text(text, encoding="utf-8")

main = Path("LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs")
replace_once(
    main,
    """                : isSdoWrite
                    ? sdoWriteConfirmationState.IsArmed
                            ? \"Confirm & Submit SDO Write\"
                            : \"Arm SDO Write\"
                    : \"Submit SDO Read\";
            ButtonSubmitSdo.ToolTip = isSdoWrite
                && !HasPendingD5SdoWriteReadback
                    ? \"Write Once uses an exact-request two-click confirmation, safe-axis preflight, durable no-replay journal, and mandatory exact readback. Known targets are optional presets.\"
                    : \"Read mode submits one tracked SDO Read.\";
""",
    """                : isSdoWrite
                    ? !hasCurrentSdoWriteTransportProof
                        ? \"Run Same-Value Qualification First\"
                        : sdoWriteConfirmationState.IsArmed
                            ? \"Confirm & Submit SDO Write\"
                            : \"Arm SDO Write\"
                    : \"Submit SDO Read\";
            ButtonSubmitSdo.ToolTip = isSdoWrite
                && !HasPendingD5SdoWriteReadback
                    ? !hasCurrentSdoWriteTransportProof
                        ? \"Run the approved UI24 same-value qualification once for this connection/session. After PASS, the proof is transport-scoped and manual ObjectIndex/SubIndex values do not need to match UI24.\"
                        : \"Write Once accepts any valid generic 1/2/4-byte SDO Write request. Known targets are optional presets; two-click confirmation, safe-axis preflight, durable no-replay journal, and mandatory exact readback remain enforced.\"
                    : \"Read mode submits one tracked SDO Read.\";
""",
    "manual SDO Write readiness UI",
)

api_test = Path("LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs")
text = api_test.read_text(encoding="utf-8-sig")
call_old = "            RunIdentityPinnedSdoWriteSuccess();\n"
call_new = "            RunIdentityPinnedSdoWriteSuccess();\n            RunIdentityPinnedGenericSdoWriteSuccess();\n"
if text.count(call_old) != 1:
    raise RuntimeError("generic identity-pinned test call insertion point mismatch")
text = text.replace(call_old, call_new)

marker = "        private static void PIWriteValidation()\n"
if text.count(marker) != 1:
    raise RuntimeError("generic identity-pinned method insertion point mismatch")
method = """        private static void RunIdentityPinnedGenericSdoWriteSuccess()
        {
            const uint writeTicketId = 0x72727272u;
            var requiredCapabilitiesBits =
                LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.SDOReadGeneralInline;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            1,
                            MapRevision,
                            DiagnosticsBootId,
                            5,
                            requiredCapabilitiesBits))),
                new FakeRpcStep(
                    0x7E00,
                    TestFrame.Response(
                        0,
                        CapabilitiesPayload(
                            2,
                            MapRevision,
                            DiagnosticsBootId,
                            5,
                            requiredCapabilitiesBits))),
                new FakeRpcStep(
                    0x7E50,
                    TestFrame.Response(
                        0,
                        SubmitPayload(
                            3,
                            writeTicketId,
                            LMCOperationKind.SDOWrite,
                            DiagnosticsBootId)))
                {
                    InspectRequest = frame =>
                    {
                        AssertEx.Equal(
                            MapRevision,
                            TestFrame.ReadUInt32(frame, 16));
                        AssertEx.Equal(
                            DiagnosticsBootId,
                            TestFrame.ReadUInt32(frame, 36));
                    }
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var requiredCapabilities =
                    connection.Diagnostics.GetCapabilities();
                var request = LMCSdoRequest.CreateWrite(
                    1,
                    0x6060,
                    0,
                    LMCSignalValueType.Int8,
                    TestFrame.Hex(\"01\"),
                    100);

                var ticket = connection.Diagnostics
                    .SubmitSdoWriteIdentityPinnedAsync(
                        request,
                        requiredCapabilities,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(writeTicketId, ticket.TicketId);
                AssertEx.Equal(DiagnosticsBootId, ticket.DiagnosticsBootId);
                AssertEx.Equal(MapRevision, ticket.SubmissionMapRevision);
                AssertEx.Equal(
                    (ushort)0x6060,
                    ticket.SubmittedSdoRequest.ObjectIndex);

                connection.CloseConnection();
                server.Verify();
            }
        }

"""
text = text.replace(marker, method + marker)
api_test.write_text(text, encoding="utf-8")

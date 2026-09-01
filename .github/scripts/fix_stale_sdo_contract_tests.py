from pathlib import Path


def replace_exact(path, old, new, expected, label):
    text = path.read_text(encoding="utf-8-sig")
    count = text.count(old)
    if count != expected:
        raise RuntimeError(f"{label}: expected {expected} match(es), got {count}")
    path.write_text(text.replace(old, new), encoding="utf-8")


def replace_in_method(text, method_name, old, new, expected, label):
    marker = "        private static void\n            " + method_name + "()"
    start = text.find(marker)
    if start < 0:
        raise RuntimeError(f"{label}: method marker not found")
    end = text.find("        private static void", start + len(marker))
    if end < 0:
        end = len(text)
    section = text[start:end]
    count = section.count(old)
    if count != expected:
        raise RuntimeError(
            f"{label}: expected {expected} match(es) in {method_name}, got {count}")
    return text[:start] + section.replace(old, new) + text[end:]


journal = Path("LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsMutationJournalTests.cs")
text = journal.read_text(encoding="utf-8-sig")
replacements = [
    ("TypedSdoV3RoundTripIsImmutable", "TypedSdoV4RoundTripIsImmutable", 3, "typed SDO journal test name"),
    ("NonCanonicalV3MetadataMarkerFailsClosed", "NonCanonicalV4MetadataMarkerFailsClosed", 3, "metadata marker test name"),
    ("FindV3MetadataMarkerOffset", "FindV4MetadataMarkerOffset", 2, "metadata marker helper name"),
    ("New durable SDO records must use journal format v3.", "New durable SDO records must use journal format v4.", 1, "journal format message"),
    ("""                        AssertEx.Equal(
                            3,
                            BitConverter.ToInt32(encoded, 8),
                            \"New durable SDO records must use journal format v4.\");
""", """                        AssertEx.Equal(
                            4,
                            BitConverter.ToInt32(encoded, 8),
                            \"New durable SDO records must use journal format v4.\");
""", 1, "journal format version expectation"),
    ("""                AssertEx.Equal(3, reader.ReadInt32());
""", """                AssertEx.Equal(4, reader.ReadInt32());
""", 1, "metadata marker format expectation"),
]
for old, new, expected, label in replacements:
    count = text.count(old)
    if count != expected:
        raise RuntimeError(f"{label}: expected {expected} match(es), got {count}")
    text = text.replace(old, new)
journal.write_text(text, encoding="utf-8")

contract = Path("LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs")
replace_exact(
    contract,
    """                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Diagnostics.SubmitSdo(unsafeWrite));
""",
    """                AssertEx.Throws<NotSupportedException>(
                    () => connection.Diagnostics.SubmitSdo(unsafeWrite));
""",
    1,
    "formerly blocked noncanonical write expectation",
)

completion = Path("LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs")
replace_exact(
    completion,
    """                var writeContext =
                    RequireSubmissionFailureContext(writeError);
                AssertEx.Equal(
                    LMCSdoSubmissionPhase.RequestValidation,
                    writeContext.Phase);
""",
    """                var writeContext =
                    RequireSubmissionFailureContext(writeError);
                AssertEx.Equal(
                    LMCSdoSubmissionPhase.SessionPreflight,
                    writeContext.Phase);
""",
    1,
    "valid generic write disconnected preflight phase",
)

localization = Path("LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/UiLocalizationTests.cs")
replace_exact(
    localization,
    """                    AssertEx.Equal(
                        \"SDO Write 준비\",
                        (string)window.ButtonSubmitSdo.Content,
                        \"Korean SDO Write mode did not localize its action.\");
                    window.TextSdoWriteData.Text = \"1\";
""",
    """                    AssertEx.Equal(
                        \"먼저 동일 값 Qualification 실행\",
                        (string)window.ButtonSubmitSdo.Content,
                        \"Korean SDO Write mode did not localize its action.\");
                    window.TextSdoWriteData.Text = \"1\";
""",
    1,
    "Korean SDO Write qualification-first initial caption",
)

recovery = Path("LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/RecoveryRecordRetirementTests.cs")
replace_exact(
    recovery,
    """                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => new DiagnosticsSdoWriteMutationMetadata(
                        1,
                        0x3204,
                        0,
                        LMCSignalValueType.UInt16,
                        2,
                        100,
                        new byte[] { 0, 0 }));
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => new DiagnosticsSdoWriteMutationMetadata(
                        1,
                        0x20FC,
                        0,
                        LMCSignalValueType.UInt32,
                        4,
                        100,
                        new byte[] { 0, 0, 0, 0 }));
""",
    """                var tw20Metadata =
                    new DiagnosticsSdoWriteMutationMetadata(
                        1,
                        0x3204,
                        0,
                        LMCSignalValueType.UInt16,
                        2,
                        100,
                        new byte[] { 0, 0 });
                AssertEx.Equal((ushort)0x3204, tw20Metadata.ObjectIndex);
                var tw19Metadata =
                    new DiagnosticsSdoWriteMutationMetadata(
                        1,
                        0x20FC,
                        0,
                        LMCSignalValueType.UInt32,
                        4,
                        100,
                        new byte[] { 0, 0, 0, 0 });
                AssertEx.Equal((ushort)0x20FC, tw19Metadata.ObjectIndex);
""",
    1,
    "WPF durable recovery allows formerly reserved SDO objects",
)

wpf = Path("LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs")
text = wpf.read_text(encoding="utf-8-sig")

text = replace_in_method(
    text,
    "WriteConfirmationRequiresExactSecondClickWithoutModal",
    """                    AssertEx.Equal(
                        \"Confirm & Submit SDO Write\",
                        Convert.ToString(
                            window.ButtonSubmitSdo.Content,
                            CultureInfo.InvariantCulture));
""",
    """                    AssertEx.Equal(
                        \"Run Same-Value Qualification First\",
                        Convert.ToString(
                            window.ButtonSubmitSdo.Content,
                            CultureInfo.InvariantCulture));
""",
    1,
    "confirmation-state smoke has no transport proof",
)

text = replace_in_method(
    text,
    "WriteSameValueAxis1OnlyRequiresConfirmations",
    """                    AssertEx.True(
                        window.ButtonSubmitSdo.IsEnabled,
                        \"Generic SDO Write did not open with current capabilities and a healthy durable journal.\");
                    AssertEx.Equal(
                        \"Arm SDO Write\",
                        Convert.ToString(
                            window.ButtonSubmitSdo.Content,
                            CultureInfo.InvariantCulture));
""",
    """                    AssertEx.False(
                        window.ButtonSubmitSdo.IsEnabled,
                        \"Manual SDO Write must remain closed until the current-session same-value transport qualification passes.\");
                    AssertEx.Equal(
                        \"Run Same-Value Qualification First\",
                        Convert.ToString(
                            window.ButtonSubmitSdo.Content,
                            CultureInfo.InvariantCulture));
""",
    1,
    "qualification proof is mandatory before manual Write",
)
text = replace_in_method(
    text,
    "WriteSameValueAxis1OnlyRequiresConfirmations",
    """                    AssertEx.True(
                        window.ButtonSubmitSdo.IsEnabled,
                        \"Generic Write must not depend on the optional known-preset same-value qualification proof.\");
""",
    """                    AssertEx.False(
                        window.ButtonSubmitSdo.IsEnabled,
                        \"Removing the current-session transport proof must close manual SDO Write.\");
                    AssertEx.Equal(
                        \"Run Same-Value Qualification First\",
                        Convert.ToString(
                            window.ButtonSubmitSdo.Content,
                            CultureInfo.InvariantCulture));
""",
    1,
    "manual Write closes when transport proof is removed",
)
text = replace_in_method(
    text,
    "WriteSameValueAxis1OnlyRequiresConfirmations",
    """                    window.TextSdoIndex.Text = \"0x6060\";
                    var reservedRequestArguments = new object[] { null, null };
                    AssertEx.False(
                        (bool)InvokePrivate(
                            window,
                            \"TryCreateSdoRequest\",
                            reservedRequestArguments),
                        \"Generic SDO Write accepted the semantic SetOperationMode object.\");
                    AssertEx.Contains(
                        \"semantic or dedicated-owner objects\",
                        Convert.ToString(
                            reservedRequestArguments[1],
                            CultureInfo.InvariantCulture));
                    InvokePrivate(window, \"UpdateSdoRequestPreview\");
                    AssertEx.Equal(
                        System.Windows.Visibility.Visible,
                        window.TextSdoSemanticWarning.Visibility);
                    AssertEx.Contains(
                        \"BLOCKED RESERVED SDO WRITE\",
                        window.TextSdoSemanticWarning.Text);
                    AssertEx.Contains(
                        \"NOT SUBMITTED\",
                        window.TextSdoSemanticWarning.Text);
                    AssertEx.Contains(
                        \"semantic or dedicated-owner objects\",
                        window.TextSdoSemanticWarning.Text);
""",
    """                    window.TextSdoIndex.Text = \"0x6060\";
                    var formerlyReservedRequestArguments =
                        new object[] { null, null };
                    AssertEx.True(
                        (bool)InvokePrivate(
                            window,
                            \"TryCreateSdoRequest\",
                            formerlyReservedRequestArguments),
                        Convert.ToString(
                            formerlyReservedRequestArguments[1],
                            CultureInfo.InvariantCulture));
                    var formerlyReservedRequest =
                        formerlyReservedRequestArguments[0] as LMCSdoRequest;
                    AssertEx.NotNull(formerlyReservedRequest);
                    AssertEx.Equal(
                        (ushort)0x6060,
                        formerlyReservedRequest.ObjectIndex);
                    InvokePrivate(window, \"UpdateSdoRequestPreview\");
                    AssertEx.Equal(
                        System.Windows.Visibility.Collapsed,
                        window.TextSdoSemanticWarning.Visibility);
""",
    1,
    "0x6060 generic Write is no longer ObjectIndex-reserved",
)
text = replace_in_method(
    text,
    "PendingReadbackPreservesDraftAndExplicitLoadRestoresExactRequest",
    """                    InvokePrivate(
                        window,
                        \"ArmSdoWriteMutationJournal\",
                        writeRequest,
                        currentConnection,
                        DiagnosticsBootId,
                        DiagnosticMapRevision);
""",
    """                    InvokePrivate(
                        window,
                        \"ArmSdoWriteMutationJournal\",
                        writeRequest,
                        currentConnection,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        new byte[] { 0x2A, 0, 0, 0 },
                        new byte[] { 0x2A, 0, 0, 0 });
""",
    1,
    "durable SDO arm reflection signature",
)

wpf.write_text(text, encoding="utf-8")

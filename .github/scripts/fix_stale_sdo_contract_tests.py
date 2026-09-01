from pathlib import Path


def replace_exact(path, old, new, expected, label):
    text = path.read_text(encoding="utf-8-sig")
    count = text.count(old)
    if count != expected:
        raise RuntimeError(f"{label}: expected {expected} match(es), got {count}")
    path.write_text(text.replace(old, new), encoding="utf-8")


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

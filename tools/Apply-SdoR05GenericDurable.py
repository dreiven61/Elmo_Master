from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
JOURNAL = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/DiagnosticsMutationJournal.cs'
TESTS = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsMutationJournalTests.cs'
DESIGN = ROOT / 'docs/api/design/SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'
VERIFY = ROOT / 'tools/Verify-SdoR05GenericDurable.ps1'


def replace_once(path, old, new, label):
    text = path.read_text(encoding='utf-8')
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f'{label}: expected exactly one anchor, found {count}')
    path.write_text(text.replace(old, new, 1), encoding='utf-8')


replace_once(
    JOURNAL,
    '''            if (valueType != LMCSignalValueType.Int32
                && valueType != LMCSignalValueType.UInt32)
            {
                throw new NotSupportedException(
                    "Durable SDO recovery supports only approved 32-bit integer targets.");
            }

            if (dataLength != 4)
            {
                throw new ArgumentOutOfRangeException(
                    "dataLength",
                    "Durable SDO recovery requires exactly four data bytes.");
            }''',
    '''            var expectedDataLength = GetCanonicalScalarDataLength(
                valueType);
            if (dataLength != expectedDataLength)
            {
                throw new ArgumentOutOfRangeException(
                    "dataLength",
                    "Durable SDO recovery requires canonical scalar length: 8-bit=1, 16-bit=2, 32-bit=4.");
            }''',
    'generic scalar metadata validation')

replace_once(
    JOURNAL,
    '''        private static bool IsPermanentlyUnsafeObject(ushort objectIndex)
        {
            return objectIndex == 0x6040
                || objectIndex == 0x607A
                || objectIndex == 0x60FF
                || objectIndex == 0x6071;
        }''',
    '''        private static ushort GetCanonicalScalarDataLength(
            LMCSignalValueType valueType)
        {
            switch (valueType)
            {
                case LMCSignalValueType.Int8:
                case LMCSignalValueType.UInt8:
                    return 1;

                case LMCSignalValueType.Int16:
                case LMCSignalValueType.UInt16:
                    return 2;

                case LMCSignalValueType.Int32:
                case LMCSignalValueType.UInt32:
                    return 4;

                default:
                    throw new NotSupportedException(
                        "Durable SDO recovery supports canonical 1/2/4-byte integer scalar types only.");
            }
        }

        private static bool IsPermanentlyUnsafeObject(ushort objectIndex)
        {
            return objectIndex == 0x6040
                || objectIndex == 0x6060
                || objectIndex == 0x607A
                || objectIndex == 0x60FF
                || objectIndex == 0x6071;
        }''',
    'canonical scalar helper and semantic deny')

replace_once(
    JOURNAL,
    '''        TargetNotApproved = 1,''',
    '''        RequestNotRecoverable = 1,''',
    'restart disposition naming')

replace_once(
    JOURNAL,
    '''                Func<DiagnosticsSdoWriteMutationMetadata, bool>
                    exactTargetApproved,''',
    '''                Func<DiagnosticsSdoWriteMutationMetadata, bool>
                    exactRequestRecoverable,''',
    'restart predicate parameter')

replace_once(
    JOURNAL,
    '''            if (exactTargetApproved == null)
            {
                throw new ArgumentNullException("exactTargetApproved");
            }''',
    '''            if (exactRequestRecoverable == null)
            {
                throw new ArgumentNullException("exactRequestRecoverable");
            }''',
    'restart predicate null guard')

replace_once(
    JOURNAL,
    '''            // This local allowlist decision deliberately precedes every
            // capability or SDO delegate. Legacy v1 records and disabled
            // compile-time targets therefore remain zero-wire.
            if (!exactTargetApproved(metadata))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition
                        .TargetNotApproved);
            }''',
    '''            // Exact-request recoverability is a generic semantic-policy
            // decision, not a target allowlist. It deliberately precedes every
            // capability or SDO delegate so legacy/semantic-reserved requests
            // remain zero-wire.
            if (!exactRequestRecoverable(metadata))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition
                        .RequestNotRecoverable);
            }''',
    'remove approved-target recovery wording')

replace_once(
    TESTS,
    '''            tests.Add(
                "Qualification.MutationJournal.TypedSdoV2RoundTripIsImmutable",
                TypedSdoV2RoundTripIsImmutable);''',
    '''            tests.Add(
                "Qualification.MutationJournal.TypedSdoV2RoundTripIsImmutable",
                TypedSdoV2RoundTripIsImmutable);
            tests.Add(
                "Qualification.MutationJournal.GenericScalarMetadataSupportsOneTwoFourBytes",
                GenericScalarMetadataSupportsOneTwoFourBytes);
            tests.Add(
                "Qualification.MutationJournal.SemanticModeObjectIsRejectedForDurableRecovery",
                SemanticModeObjectIsRejectedForDurableRecovery);''',
    'register generic durable tests')

replace_once(
    TESTS,
    '''            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryUnapprovedIsZeroWire",
                RestartRecoveryUnapprovedIsZeroWire);''',
    '''            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryUnrecoverableIsZeroWire",
                RestartRecoveryUnrecoverableIsZeroWire);''',
    'rename recovery test registration')

replace_once(
    TESTS,
    '''        private static void NonCanonicalV2MetadataMarkerFailsClosed()
        {''',
    '''        private static void GenericScalarMetadataSupportsOneTwoFourBytes()
        {
            AssertScalarMetadata(
                LMCSignalValueType.Int8,
                1,
                new byte[] { 0xFE });
            AssertScalarMetadata(
                LMCSignalValueType.UInt8,
                1,
                new byte[] { 0x7F });
            AssertScalarMetadata(
                LMCSignalValueType.Int16,
                2,
                new byte[] { 0x34, 0x12 });
            AssertScalarMetadata(
                LMCSignalValueType.UInt16,
                2,
                new byte[] { 0x78, 0x56 });
            AssertScalarMetadata(
                LMCSignalValueType.Int32,
                4,
                new byte[] { 1, 2, 3, 4 });
            AssertScalarMetadata(
                LMCSignalValueType.UInt32,
                4,
                new byte[] { 5, 6, 7, 8 });

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new DiagnosticsSdoWriteMutationMetadata(
                    2,
                    0x2000,
                    3,
                    LMCSignalValueType.UInt16,
                    1,
                    1000,
                    new byte[] { 0x12 }),
                "A non-canonical type/length pair must fail before durable arm.");
        }

        private static void SemanticModeObjectIsRejectedForDurableRecovery()
        {
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new DiagnosticsSdoWriteMutationMetadata(
                    1,
                    0x6060,
                    0,
                    LMCSignalValueType.Int8,
                    1,
                    1000,
                    new byte[] { 8 }),
                "Generic durable recovery must never make 0x6060 replayable/recoverable as raw SDO Write.");
        }

        private static void NonCanonicalV2MetadataMarkerFailsClosed()
        {''',
    'insert generic durable tests')

replace_once(
    TESTS,
    '''        private static void RestartRecoveryUnapprovedIsZeroWire()''',
    '''        private static void RestartRecoveryUnrecoverableIsZeroWire()''',
    'rename recovery test method')

replace_once(
    TESTS,
    '''                            DiagnosticsSdoRestartRecoveryDisposition
                                .TargetNotApproved,''',
    '''                            DiagnosticsSdoRestartRecoveryDisposition
                                .RequestNotRecoverable,''',
    'generic recovery disposition assertion')

replace_once(
    TESTS,
    '''        private static DiagnosticsSdoWriteMutationMetadata CreateSdoMetadata(
            byte[] expectedWriteData)
        {''',
    '''        private static void AssertScalarMetadata(
            LMCSignalValueType valueType,
            ushort dataLength,
            byte[] expectedWriteData)
        {
            var metadata = new DiagnosticsSdoWriteMutationMetadata(
                2,
                0x2000,
                3,
                valueType,
                dataLength,
                1000,
                expectedWriteData);
            AssertEx.Equal((ushort)2, metadata.SlaveReference);
            AssertEx.Equal((ushort)0x2000, metadata.ObjectIndex);
            AssertEx.Equal((byte)3, metadata.SubIndex);
            AssertEx.Equal(valueType, metadata.ValueType);
            AssertEx.Equal(dataLength, metadata.DataLength);
            AssertEx.SequenceEqual(
                expectedWriteData,
                metadata.ExpectedWriteData);
        }

        private static DiagnosticsSdoWriteMutationMetadata CreateSdoMetadata(
            byte[] expectedWriteData)
        {''',
    'insert scalar metadata helper')

# Keep the design truthful: R05-A is source-complete, while endpoint/build v3 identity remains open.
replace_once(
    DESIGN,
    '''- [ ] tamper/corrupt journal fail-closed

---''',
    '''- [ ] tamper/corrupt journal fail-closed

2026-08-28 current-dev R05-A: durable SDO metadata를 `Int8/UInt8/Int16/UInt16/Int32/UInt32` canonical 1/2/4-byte scalar로 일반화했다. restart recovery의 legacy `approved target` 용어/전제는 generic exact-request recoverability policy로 교체했고, `0x6060`은 durable metadata 단계에서도 semantic-reserved zero-wire deny로 고정했다. 기존 BootId/MapRevision exact-read no-replay 경계는 유지한다. 다음 R05-B에서 journal v3에 Endpoint IP/port + DiagnosticsBuild를 추가하고 v1/v2 legacy record를 full-identity recovery에서 fail-closed 처리한다.

---''',
    'R05-A design status note')

VERIFY.write_text(r'''param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$journalPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\DiagnosticsMutationJournal.cs'
$testPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\DiagnosticsMutationJournalTests.cs'
$designPath = Join-Path $root 'docs\api\design\SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'
$journal = Get-Content -LiteralPath $journalPath -Raw
$tests = Get-Content -LiteralPath $testPath -Raw
$design = Get-Content -LiteralPath $designPath -Raw
function Require-Text([string]$Text, [string]$Needle, [string]$Label) {
    if ($Text.IndexOf($Needle, [StringComparison]::Ordinal) -lt 0) {
        throw "Missing ${Label}: $Needle"
    }
}
Require-Text $journal 'case LMCSignalValueType.Int8:' 'Int8 durable metadata'
Require-Text $journal 'case LMCSignalValueType.UInt16:' 'UInt16 durable metadata'
Require-Text $journal 'case LMCSignalValueType.UInt32:' 'UInt32 durable metadata'
Require-Text $journal 'objectIndex == 0x6060' '0x6060 durable deny'
Require-Text $journal 'exactRequestRecoverable' 'generic restart predicate'
Require-Text $journal 'RequestNotRecoverable' 'generic restart disposition'
if ($journal.IndexOf('TargetNotApproved', [StringComparison]::Ordinal) -ge 0) {
    throw 'Legacy approved-target restart disposition remains.'
}
Require-Text $tests 'GenericScalarMetadataSupportsOneTwoFourBytes' '1/2/4-byte durable test'
Require-Text $tests 'SemanticModeObjectIsRejectedForDurableRecovery' '0x6060 durable test'
Require-Text $design 'current-dev R05-A' 'R05-A design sync'
Write-Host 'PASS SDO-R05-A generic durable metadata source contract.'
''', encoding='utf-8')

print('SDO-R05-A generic durable patch applied.')

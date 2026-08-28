from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
POLICY = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsSdoWritePolicyEvaluationTests.cs'
D45 = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs'


def replace_exception_in_method(path, method_name, expected_count):
    text = path.read_text(encoding='utf-8')
    pattern = (
        r'(        private static void ' + re.escape(method_name)
        + r'\(\)\n        \{.*?\n        \}\n)(?=\n        private static)'
    )
    match = re.search(pattern, text, flags=re.S)
    if match is None:
        raise RuntimeError(f'{path.name}: method {method_name} not found')
    block = match.group(1)
    actual = block.count('AssertEx.Throws<NotSupportedException>')
    if actual != expected_count:
        raise RuntimeError(
            f'{path.name}: {method_name} expected {expected_count} '
            f'NotSupportedException assertions, found {actual}')
    block = block.replace(
        'AssertEx.Throws<NotSupportedException>',
        'AssertEx.Throws<InvalidOperationException>')
    path.write_text(
        text[:match.start(1)] + block + text[match.end(1):],
        encoding='utf-8')


# LMCDiagnostics.ValidateSdoWritePolicy intentionally owns the public
# fail-closed contract for permanently unsafe semantic/dedicated-owner
# objects, and that established contract is InvalidOperationException.
replace_exception_in_method(
    POLICY,
    'SemanticOwnerSubmitSyncAndAsyncIsZeroWire',
    2)
replace_exception_in_method(
    POLICY,
    'EncoderMaintenanceObjectsSyncAndAsyncAreZeroWire',
    2)
replace_exception_in_method(
    D45,
    'D5SubmitSdoLocalPreflightContext',
    1)

print('SDO-R03 fail-closed exception expectations aligned.')

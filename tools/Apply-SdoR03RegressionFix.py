from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
SDK = ROOT / 'LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs'
PLC = ROOT / 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
D5 = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs'
POLICY = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsSdoWritePolicyEvaluationTests.cs'
VERIFY = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsSdoWriteVerificationTests.cs'
D45 = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs'


def replace_once(text, old, new, label):
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f'{label}: expected exactly one match, found {count}')
    return text.replace(old, new, 1)


def regex_once(text, pattern, replacement, label):
    updated, count = re.subn(pattern, replacement, text, count=1, flags=re.S)
    if count != 1:
        raise RuntimeError(f'{label}: expected exactly one regex match, found {count}')
    return updated

# Dedicated semantic owners stay inaccessible through raw Generic SDO Write.
sdk = SDK.read_text(encoding='utf-8')
old_switch = '''                case 0x6040:\n                case 0x6060:\n                case 0x607A:\n                case 0x60FF:\n                case 0x6071:\n'''
new_switch = '''                case 0x6040:\n                case 0x6060:\n                case 0x607A:\n                case 0x60FF:\n                case 0x6071:\n                case 0x3204:\n                case 0x20FC:\n'''
count = sdk.count(old_switch)
if count < 1:
    raise RuntimeError('post-R03 SDK semantic-owner switch was not found')
sdk = sdk.replace(old_switch, new_switch)
SDK.write_text(sdk, encoding='utf-8')

plc = PLC.read_text(encoding='utf-8')
plc = regex_once(
    plc,
    r'(?P<indent>\s*)if \(ObjectIndex = 0x6040\) \| \(ObjectIndex = 0x6060\) \|\s*'
    r'\(ObjectIndex = 0x607A\) \| \(ObjectIndex = 0x60FF\) \|\s*'
    r'\(ObjectIndex = 0x6071\) then',
    lambda match: (
        match.group('indent')
        + 'if (ObjectIndex = 0x6040) | (ObjectIndex = 0x6060) |\n'
        + '\t\t\t(ObjectIndex = 0x607A) | (ObjectIndex = 0x60FF) |\n'
        + '\t\t\t(ObjectIndex = 0x6071) | (ObjectIndex = 0x3204) |\n'
        + '\t\t\t(ObjectIndex = 0x20FC) then'),
    'PLC semantic owner block')
PLC.write_text(plc, encoding='utf-8')

# LMCSdoRequest uses a four-byte minimum wire container even for narrow scalar
# writes; DataLength carries the canonical 1/2-byte width.
d5 = D5.read_text(encoding='utf-8')
d5 = replace_once(
    d5,
    '''                    TestFrame.Hex("5A"), 100),\n''',
    '''                    TestFrame.Hex("5A 00 00 00"), 100),\n''',
    'UInt8 generic write wire container')
d5 = replace_once(
    d5,
    '''                    TestFrame.Hex("34 12"), 100),\n''',
    '''                    TestFrame.Hex("34 12 00 00"), 100),\n''',
    'UInt16 generic write wire container')
D5.write_text(d5, encoding='utf-8')

# A formerly non-allowlisted axis is now a valid generic target. Preserve a
# strict zero-wire test by testing a semantic-owner object instead.
policy = POLICY.read_text(encoding='utf-8')
policy = replace_once(
    policy,
    '''                "Rpc.DiagnosticsD5.SdoWriteNonAllowlistedAxisSubmitIsZeroWire",\n                NonAllowlistedAxisSubmitSyncAndAsyncIsZeroWire);\n''',
    '''                "Rpc.DiagnosticsD5.SdoWriteSemanticOwnerSubmitIsZeroWire",\n                SemanticOwnerSubmitSyncAndAsyncIsZeroWire);\n''',
    'policy registration rename')
new_semantic_method = '''        private static void SemanticOwnerSubmitSyncAndAsyncIsZeroWire()\n        {\n            using (var server = new FakeRpcServer(\n                InitStep(),\n                CallbackStep(),\n                CloseStep()))\n            using (var connection = new LMCConnection())\n            {\n                Connect(connection, server.Port);\n                var request = LMCSdoRequest.CreateWrite(\n                    2,\n                    0x6060,\n                    0,\n                    LMCSignalValueType.Int8,\n                    new byte[] { 8, 0, 0, 0 },\n                    1000);\n                var requestCountBeforeSubmissions =\n                    server.ReceivedRequests.Count;\n\n                var syncError = AssertEx.Throws<NotSupportedException>(\n                    () => connection.Diagnostics.SubmitSdo(request));\n                AssertRequestValidationNotAttempted(syncError);\n                AssertEx.Equal(\n                    requestCountBeforeSubmissions,\n                    server.ReceivedRequests.Count,\n                    "Synchronous semantic-owner SDO Write sent an RPC request.");\n\n                var asyncError = AssertEx.Throws<NotSupportedException>(\n                    () => connection.Diagnostics.SubmitSdoAsync(\n                            request,\n                            CancellationToken.None)\n                        .GetAwaiter()\n                        .GetResult());\n                AssertRequestValidationNotAttempted(asyncError);\n                AssertEx.Equal(\n                    requestCountBeforeSubmissions,\n                    server.ReceivedRequests.Count,\n                    "Asynchronous semantic-owner SDO Write sent an RPC request.");\n\n                connection.CloseConnection();\n                server.Verify();\n            }\n        }\n\n'''
policy = regex_once(
    policy,
    r'        private static void NonAllowlistedAxisSubmitSyncAndAsyncIsZeroWire\(\)\n        \{.*?\n        \}\n\n(?=        private static void EncoderMaintenanceObjectsSyncAndAsyncAreZeroWire)',
    new_semantic_method,
    'non-allowlisted policy test replacement')
POLICY.write_text(policy, encoding='utf-8')

# The public verification overload now accepts a generic request through the
# same policy as submission; explicit false predicates remain fail-closed.
verify = VERIFY.read_text(encoding='utf-8')
old_default_reject = '''                AssertEx.Throws<NotSupportedException>(\n                    () => connection.Diagnostics\n                        .CreateSdoWriteVerificationContext(\n                            request,\n                            ticket,\n                            terminalStatus));\n'''
new_default_accept = '''                AssertEx.NotNull(\n                    connection.Diagnostics\n                        .CreateSdoWriteVerificationContext(\n                            request,\n                            ticket,\n                            terminalStatus));\n'''
verify = replace_once(
    verify,
    old_default_reject,
    new_default_accept,
    'default generic verification context')
VERIFY.write_text(verify, encoding='utf-8')

# Replace old allowlist language with capability-gate semantics. Two distinct
# writes each perform a capability observation and neither reaches a mutation.
d45 = D45.read_text(encoding='utf-8')
d45 = replace_once(
    d45,
    '''                "Policy.DiagnosticsD5.WriteAllowlistFailClosed",\n                D5WriteAllowlistFailClosed);\n''',
    '''                "Policy.DiagnosticsD5.WriteCapabilityFailClosed",\n                D5WriteCapabilityFailClosed);\n''',
    'D45 registration rename')
d45 = d45.replace(
    'private static void D5WriteAllowlistFailClosed()',
    'private static void D5WriteCapabilityFailClosed()', 1)
d45 = d45.replace(
    'RunD5WriteAllowlistFailClosed(false);',
    'RunD5WriteCapabilityFailClosed(false);', 1)
d45 = d45.replace(
    'RunD5WriteAllowlistFailClosed(true);',
    'RunD5WriteCapabilityFailClosed(true);', 1)
d45 = d45.replace(
    'private static void RunD5WriteAllowlistFailClosed(bool useAsync)',
    'private static void RunD5WriteCapabilityFailClosed(bool useAsync)', 1)
cap_step = '''                new FakeRpcStep(\n                    0x7E00,\n                    TestFrame.Response(\n                        0,\n                        CapabilitiesPayload(\n                            1,\n                            LMCDiagnosticCapability.SignalCatalog\n                                | LMCDiagnosticCapability.PIWrite,\n                            0,\n                            0))),\n'''
d45 = replace_once(
    d45,
    cap_step,
    cap_step + cap_step.replace('CapabilitiesPayload(\n                            1,', 'CapabilitiesPayload(\n                            2,'),
    'second capability observation for generic SDO write')

# The same formerly-arbitrary 0x2000 write appears in two local preflight
# contexts. Both must become a dedicated-owner target so they remain zero-wire
# after arbitrary generic addresses become valid.
old_local = '''                    0x2000,\n                    0,\n                    LMCSignalValueType.UInt32,\n                    TestFrame.Hex("78 56 34 12"),\n'''
new_local = '''                    0x6060,\n                    0,\n                    LMCSignalValueType.Int8,\n                    TestFrame.Hex("08 00 00 00"),\n'''
local_count = d45.count(old_local)
if local_count != 2:
    raise RuntimeError(
        f'D45 local semantic-owner preflight requests: expected exactly 2 matches, found {local_count}')
d45 = d45.replace(old_local, new_local)
D45.write_text(d45, encoding='utf-8')

print('SDO-R03 regression contracts updated successfully.')

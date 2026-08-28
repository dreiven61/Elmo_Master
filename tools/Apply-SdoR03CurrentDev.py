from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
SDK = ROOT / 'LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs'
VERIFY_SRC = ROOT / 'LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsSdoWriteVerification.cs'
D5_TEST = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs'
POLICY_TEST = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsSdoWritePolicyEvaluationTests.cs'
VERIFY_TEST = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsSdoWriteVerificationTests.cs'
D45_TEST = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs'
PLC = ROOT / 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'


def once(text, old, new, label):
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f'{label}: expected 1 match, found {count}')
    return text.replace(old, new, 1)


def rex(text, pattern, replacement, label, count=1):
    updated, n = re.subn(pattern, replacement, text, count=count, flags=re.S)
    if n != count:
        raise RuntimeError(f'{label}: expected {count} matches, found {n}')
    return updated


# ---------------------------------------------------------------------------
# SDK model / policy
# ---------------------------------------------------------------------------
sdk = SDK.read_text(encoding='utf-8')

sdk = once(
    sdk,
    '''            if (writeData.Length != 4\n                && writeData.Length != 8\n                && writeData.Length != 12)\n            {\n                throw new ArgumentOutOfRangeException(\n                    "writeData",\n                    "D5 SDO WriteData must contain exactly 4, 8, or 12 bytes.");\n            }\n''',
    '''            if (writeData.Length != 1\n                && writeData.Length != 2\n                && writeData.Length != 4\n                && writeData.Length != 8\n                && writeData.Length != 12)\n            {\n                throw new ArgumentOutOfRangeException(\n                    "writeData",\n                    "D5 SDO WriteData must contain exactly 1, 2, 4, 8, or 12 bytes.");\n            }\n''',
    'CreateWrite scalar lengths')

sdk = once(
    sdk,
    '''            var isWriteInlineLength = dataLength == 4\n                || dataLength == 8\n                || dataLength == 12;\n''',
    '''            var isWriteInlineLength = dataLength == 1\n                || dataLength == 2\n                || dataLength == 4\n                || dataLength == 8\n                || dataLength == 12;\n''',
    'ValidateIdentity write lengths')

sdk = once(
    sdk,
    '''                    "SDO Write DataLength must be exactly 4, 8, or 12 bytes.");\n''',
    '''                    "SDO Write DataLength must be exactly 1, 2, 4, 8, or 12 bytes.");\n''',
    'ValidateIdentity write length message')

unsafe_old = '''                case 0x6040:\n                case 0x607A:\n                case 0x60FF:\n                case 0x6071:\n'''
unsafe_new = '''                case 0x6040:\n                case 0x6060:\n                case 0x607A:\n                case 0x60FF:\n                case 0x6071:\n                case 0x3204:\n                case 0x20FC:\n'''
unsafe_count = sdk.count(unsafe_old)
if unsafe_count < 1:
    raise RuntimeError('unsafe object switch not found')
sdk = sdk.replace(unsafe_old, unsafe_new)

sdk = sdk.replace(
    '''    /// Immutable compile-time SDO Write target intended to be mirrored by the\n    /// PLC policy. Applications must not treat an arbitrary SDO address as a\n    /// writable target, and submission still verifies both policies.\n''',
    '''    /// Immutable known SDO Write preset. Presets provide engineering metadata\n    /// and conservative value ranges, but are not the generic D5 address policy.\n    /// Submission still enforces semantic-owner, scalar-shape, capability,\n    /// journal/no-replay and exact readback contracts.\n''')

sdk = sdk.replace(
    '''        // Axis 1 UI[24] is source-approved for an explicitly supervised live\n        // qualification. Drive-program ownership and hardware behavior still\n        // require PLC/runtime proof, so exact capability, axis-state, target,\n        // confirmation, journal, and readback interlocks remain mandatory.\n''',
    '''        // UI[24] remains a known qualification preset. Generic D5 scalar Write\n        // is admitted by request validity and semantic ownership rather than an\n        // exact address allowlist. Confirmation, journal/no-replay and exact\n        // readback interlocks remain mandatory at the higher-level workflow.\n''')

sdk = once(
    sdk,
    '''            if (approvedTargets.Count == 0)\n            {\n                blockers |= LMCSdoWritePolicyBlockers.NoApprovedTarget;\n            }\n''',
    '''            if (!SdoWriteEnabled)\n            {\n                blockers |= LMCSdoWritePolicyBlockers.NoApprovedTarget;\n            }\n''',
    'policy evaluation no-preset gate')

new_require = '''        internal static void RequireSdoWriteAllowed(LMCSdoRequest request)\n        {\n            if (request == null)\n            {\n                throw new ArgumentNullException("request");\n            }\n\n            if (!request.IsWrite)\n            {\n                return;\n            }\n\n            if (!SdoWriteEnabled)\n            {\n                throw new NotSupportedException(\n                    "SDO Write is disabled by the SDK policy gate.");\n            }\n\n            if (request.SlaveReference < 1 || request.SlaveReference > 4)\n            {\n                throw new NotSupportedException(\n                    "Generic SDO Write supports SlaveReference 1 through 4 only.");\n            }\n\n            if (request.ObjectIndex == 0\n                || LMCSdoRequest.IsPermanentlyUnsafeObject(request.ObjectIndex))\n            {\n                throw new NotSupportedException(\n                    "Generic SDO Write cannot target semantic or dedicated-owner objects.");\n            }\n\n            var expectedLength =\n                LMCDiagnosticsSdoPolicy.ExpectedReadLength(request.ValueType);\n            if (request.DataLength != expectedLength\n                || request.DataLength > 4)\n            {\n                throw new NotSupportedException(\n                    "Generic SDO Write requires canonical 1/2/4-byte scalar type lengths.");\n            }\n\n            var data = request.WriteDataUnsafe;\n            if (data == null || data.Length != request.DataLength)\n            {\n                throw new NotSupportedException(\n                    "Generic SDO Write requires an exact canonical payload length.");\n            }\n\n            if (request.ValueType == LMCSignalValueType.Bool && data[0] > 1)\n            {\n                throw new NotSupportedException(\n                    "Generic Bool SDO Write accepts only 0 or 1.");\n            }\n\n            // Keep the conservative qualification range for the known UI[24]\n            // preset without turning the preset into an address allowlist.\n            if (request.ObjectIndex == 0x2F00\n                && request.SubIndex == 24\n                && request.ValueType == LMCSignalValueType.Int32\n                && request.DataLength == 4)\n            {\n                var value = unchecked((int)(\n                    (uint)data[0]\n                    | ((uint)data[1] << 8)\n                    | ((uint)data[2] << 16)\n                    | ((uint)data[3] << 24)));\n                if (value < -1073741823 || value > 1073741823)\n                {\n                    throw new NotSupportedException(\n                        "The UI[24] qualification preset value is outside its conservative range.");\n                }\n            }\n        }\n\n'''
sdk = rex(
    sdk,
    r'        internal static void RequireSdoWriteAllowed\(LMCSdoRequest request\)\n        \{.*?\n        \}\n\n(?=        internal static void RequireSdoWriteVerificationCapabilities)',
    new_require,
    'RequireSdoWriteAllowed')

if 'target is not in the SDK compile-time allowlist' in sdk:
    raise RuntimeError('stale SDO address allowlist rejection remains')
SDK.write_text(sdk, encoding='utf-8')


# ---------------------------------------------------------------------------
# Exact readback verification must follow the scalar's actual wire width.
# ---------------------------------------------------------------------------
verify_src = VERIFY_SRC.read_text(encoding='utf-8')
verify_src = once(
    verify_src,
    '''            ValidateSdoWritePolicy(approvedWriteRequest);\n            if (!approvedWriteRequest.IsWrite\n                || approvedWriteRequest.DataLength != 4\n                || approvedWriteRequest.WriteDataUnsafe.Length != 4)\n            {\n                throw new ArgumentException(\n                    "SDO Write verification requires an exact four-byte Write request.",\n                    "approvedWriteRequest");\n            }\n''',
    '''            ValidateSdoWritePolicy(approvedWriteRequest);\n            var expectedWriteLength = LMCDiagnosticsSdoPolicy\n                .ExpectedReadLength(approvedWriteRequest.ValueType);\n            if (!approvedWriteRequest.IsWrite\n                || approvedWriteRequest.DataLength != expectedWriteLength\n                || approvedWriteRequest.WriteDataUnsafe.Length\n                    != expectedWriteLength)\n            {\n                throw new ArgumentException(\n                    "SDO Write verification requires an exact canonical 1/2/4-byte scalar Write request.",\n                    "approvedWriteRequest");\n            }\n''',
    'verification scalar width')
VERIFY_SRC.write_text(verify_src, encoding='utf-8')


# ---------------------------------------------------------------------------
# Focused D5 contract: preset retained, generic 1/2/4 scalar writes admitted.
# ---------------------------------------------------------------------------
tests = D5_TEST.read_text(encoding='utf-8')
new_policy_test = '''        private static void SdoWriteTargetPolicy()\n        {\n            IReadOnlyList<LMCSdoWriteTarget> approved;\n            using (var connection = new LMCConnection())\n            {\n                approved = connection.Diagnostics.GetApprovedSdoWriteTargets();\n                AssertEx.Equal(1, approved.Count);\n            }\n\n            AssertEx.Equal("Reserved diagnostic UI[24]", approved[0].DisplayName);\n            AssertEx.Equal((ushort)1, approved[0].SlaveReference);\n            AssertEx.Equal((ushort)0x2F00, approved[0].ObjectIndex);\n            AssertEx.Equal((byte)24, approved[0].SubIndex);\n            AssertEx.Equal(LMCSignalValueType.Int32, approved[0].ValueType);\n            AssertEx.Equal((ushort)4, approved[0].DataLength);\n            AssertEx.Equal(-1073741823L, approved[0].MinimumIntegerValue);\n            AssertEx.Equal(1073741823L, approved[0].MaximumIntegerValue);\n            LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(\n                approved[0].CreateRequest(0, 100));\n\n            var genericWrites = new[]\n            {\n                LMCSdoRequest.CreateWrite(1, 0x2000, 1,\n                    LMCSignalValueType.UInt8, TestFrame.Hex("5A"), 100),\n                LMCSdoRequest.CreateWrite(2, 0x2001, 2,\n                    LMCSignalValueType.UInt16, TestFrame.Hex("34 12"), 100),\n                LMCSdoRequest.CreateWrite(3, 0x3000, 7,\n                    LMCSignalValueType.Int32, TestFrame.Hex("FE FF FF FF"), 100),\n                LMCSdoRequest.CreateWrite(4, 0x3001, 255,\n                    LMCSignalValueType.Real32, TestFrame.Hex("00 00 80 3F"), 100)\n            };\n            foreach (var generic in genericWrites)\n            {\n                LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(generic);\n            }\n\n            foreach (var blockedObject in new ushort[]\n            {\n                0x6040, 0x6060, 0x607A, 0x60FF, 0x6071, 0x3204, 0x20FC\n            })\n            {\n                var blocked = LMCSdoRequest.CreateWrite(\n                    1, blockedObject, 0, LMCSignalValueType.UInt16,\n                    TestFrame.Hex("00 00"), 100);\n                AssertEx.Throws<NotSupportedException>(\n                    () => LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(blocked));\n            }\n\n            var outOfRangeUi24 = LMCSdoRequest.CreateWrite(\n                4, 0x2F00, 24, LMCSignalValueType.Int32,\n                TestFrame.Hex("FF FF FF 7F"), 100);\n            AssertEx.Throws<NotSupportedException>(\n                () => LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(\n                    outOfRangeUi24));\n\n            AssertEx.Throws<ArgumentNullException>(\n                () => LMCDiagnosticsWritePolicy\n                    .RequireSdoWriteVerificationCapabilities(null));\n            AssertEx.Throws<NotSupportedException>(\n                () => LMCDiagnosticsWritePolicy\n                    .RequireSdoWriteVerificationCapabilities(\n                        SdoCapabilities(LMCDiagnosticCapability.SDOWrite\n                            | LMCDiagnosticCapability.SDORead)));\n            AssertEx.Throws<NotSupportedException>(\n                () => LMCDiagnosticsWritePolicy\n                    .RequireSdoWriteVerificationCapabilities(\n                        SdoCapabilities(LMCDiagnosticCapability.SDOWrite\n                            | LMCDiagnosticCapability.SDOReadGeneralInline)));\n            LMCDiagnosticsWritePolicy.RequireSdoWriteVerificationCapabilities(\n                SdoCapabilities(LMCDiagnosticCapability.SDOWrite\n                    | LMCDiagnosticCapability.SDORead\n                    | LMCDiagnosticCapability.SDOReadGeneralInline));\n        }\n\n'''
tests = rex(
    tests,
    r'        private static void SdoWriteTargetPolicy\(\)\n        \{.*?\n        \}\n\n(?=        private static void SdoRequestValidation)',
    new_policy_test,
    'SdoWriteTargetPolicy')
D5_TEST.write_text(tests, encoding='utf-8')


# ---------------------------------------------------------------------------
# Existing zero-wire regression now tests a semantic owner, not a random axis.
# ---------------------------------------------------------------------------
policy_test = POLICY_TEST.read_text(encoding='utf-8')
policy_test = once(
    policy_test,
    '''                "Rpc.DiagnosticsD5.SdoWriteNonAllowlistedAxisSubmitIsZeroWire",\n                NonAllowlistedAxisSubmitSyncAndAsyncIsZeroWire);\n''',
    '''                "Rpc.DiagnosticsD5.SdoWriteSemanticOwnerSubmitIsZeroWire",\n                SemanticOwnerSubmitSyncAndAsyncIsZeroWire);\n''',
    'policy registration')
new_zero_wire = '''        private static void SemanticOwnerSubmitSyncAndAsyncIsZeroWire()\n        {\n            using (var server = new FakeRpcServer(\n                InitStep(),\n                CallbackStep(),\n                CloseStep()))\n            using (var connection = new LMCConnection())\n            {\n                Connect(connection, server.Port);\n                var request = LMCSdoRequest.CreateWrite(\n                    2,\n                    0x6060,\n                    0,\n                    LMCSignalValueType.Int8,\n                    new byte[] { 8 },\n                    1000);\n                var requestCountBeforeSubmissions =\n                    server.ReceivedRequests.Count;\n\n                var syncError = AssertEx.Throws<NotSupportedException>(\n                    () => connection.Diagnostics.SubmitSdo(request));\n                AssertRequestValidationNotAttempted(syncError);\n                AssertEx.Equal(\n                    requestCountBeforeSubmissions,\n                    server.ReceivedRequests.Count,\n                    "Synchronous semantic-owner SDO Write sent an RPC request.");\n\n                var asyncError = AssertEx.Throws<NotSupportedException>(\n                    () => connection.Diagnostics.SubmitSdoAsync(\n                            request, CancellationToken.None)\n                        .GetAwaiter().GetResult());\n                AssertRequestValidationNotAttempted(asyncError);\n                AssertEx.Equal(\n                    requestCountBeforeSubmissions,\n                    server.ReceivedRequests.Count,\n                    "Asynchronous semantic-owner SDO Write sent an RPC request.");\n\n                connection.CloseConnection();\n                server.Verify();\n            }\n        }\n\n'''
policy_test = rex(
    policy_test,
    r'        private static void NonAllowlistedAxisSubmitSyncAndAsyncIsZeroWire\(\)\n        \{.*?\n        \}\n\n(?=        private static void EncoderMaintenanceObjectsSyncAndAsyncAreZeroWire)',
    new_zero_wire,
    'semantic zero-wire test')
POLICY_TEST.write_text(policy_test, encoding='utf-8')


# ---------------------------------------------------------------------------
# Default verification is valid for any policy-approved exact scalar request.
# ---------------------------------------------------------------------------
verify_test = VERIFY_TEST.read_text(encoding='utf-8')
verify_test = once(
    verify_test,
    '''                AssertEx.Throws<NotSupportedException>(\n                    () => connection.Diagnostics\n                        .CreateSdoWriteVerificationContext(\n                            request,\n                            ticket,\n                            terminalStatus));\n''',
    '''                AssertEx.NotNull(connection.Diagnostics\n                    .CreateSdoWriteVerificationContext(\n                        request,\n                        ticket,\n                        terminalStatus));\n''',
    'default verification context')
VERIFY_TEST.write_text(verify_test, encoding='utf-8')


# ---------------------------------------------------------------------------
# Completion/preflight regressions: random generic address is no longer a
# local policy reject; missing SDOWrite capability and semantic-owner blocks are.
# ---------------------------------------------------------------------------
d45 = D45_TEST.read_text(encoding='utf-8')
d45 = once(
    d45,
    '''                "Policy.DiagnosticsD5.WriteAllowlistFailClosed",\n                D5WriteAllowlistFailClosed);\n''',
    '''                "Policy.DiagnosticsD5.WriteCapabilityFailClosed",\n                D5WriteCapabilityFailClosed);\n''',
    'D45 registration')
d45 = d45.replace('D5WriteAllowlistFailClosed', 'D5WriteCapabilityFailClosed')

# The PI and SDO submissions each refresh capabilities in this combined test.
cap_step = '''                new FakeRpcStep(\n                    0x7E00,\n                    TestFrame.Response(\n                        0,\n                        CapabilitiesPayload(\n                            1,\n                            LMCDiagnosticCapability.SignalCatalog\n                                | LMCDiagnosticCapability.PIWrite,\n                            0,\n                            0))),\n                CloseStep()))\n'''
cap_step_new = '''                new FakeRpcStep(\n                    0x7E00,\n                    TestFrame.Response(\n                        0,\n                        CapabilitiesPayload(\n                            1,\n                            LMCDiagnosticCapability.SignalCatalog\n                                | LMCDiagnosticCapability.PIWrite,\n                            0,\n                            0))),\n                new FakeRpcStep(\n                    0x7E00,\n                    TestFrame.Response(\n                        0,\n                        CapabilitiesPayload(\n                            2,\n                            LMCDiagnosticCapability.SignalCatalog\n                                | LMCDiagnosticCapability.PIWrite,\n                            0,\n                            0))),\n                CloseStep()))\n'''
d45 = once(d45, cap_step, cap_step_new, 'D45 capability refresh')

old_local_write = '''                var writeRequest = LMCSdoRequest.CreateWrite(\n                    1,\n                    0x2000,\n                    0,\n                    LMCSignalValueType.UInt32,\n                    TestFrame.Hex("78 56 34 12"),\n                    100);\n'''
new_local_write = '''                var writeRequest = LMCSdoRequest.CreateWrite(\n                    1,\n                    0x6060,\n                    0,\n                    LMCSignalValueType.Int8,\n                    TestFrame.Hex("08"),\n                    100);\n'''
local_count = d45.count(old_local_write)
if local_count < 1:
    raise RuntimeError('D45 local generic preflight request not found')
d45 = d45.replace(old_local_write, new_local_write)
D45_TEST.write_text(d45, encoding='utf-8')


# ---------------------------------------------------------------------------
# PLC: generic scalar shape gate, semantic-owner deny and preset-only range.
# ---------------------------------------------------------------------------
plc = PLC.read_text(encoding='utf-8')

new_gate = '''\t\t// Generic scalar D5 Write is admitted by request shape and semantic\n\t\t// ownership, not by a compile-time address list. Dedicated encoder\n\t\t// maintenance remains available only through 0x7E53.\n\t\tif LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = FALSE then\n\t\t\tDetailCode := 7;\n\t\t\tRETURN;\n\t\tend_if;\n\t\tif (SlaveReference < 1) | (SlaveReference > 4) |\n\t\t\t(ObjectIndex = 0) then\n\t\t\tDetailCode := 7;\n\t\t\tRETURN;\n\t\tend_if;\n\n\t\tcase ValueType of\n\t\t\t1, 9, 10, 11:\n\t\t\t\tif DataLength <> 1 then\n\t\t\t\t\tDetailCode := 7;\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n\t\t\t2, 3, 7:\n\t\t\t\tif DataLength <> 2 then\n\t\t\t\t\tDetailCode := 7;\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n\t\t\t4, 5, 6, 8:\n\t\t\t\tif DataLength <> 4 then\n\t\t\t\t\tDetailCode := 7;\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n\t\telse\n\t\t\tDetailCode := 7;\n\t\t\tRETURN;\n\t\tend_case;\n\n\t\tif (ValueType = 1) & (WriteData <> 0) & (WriteData <> 1) then\n\t\t\tDetailCode := 12;\n\t\t\tRETURN;\n\t\tend_if;\n'''
plc = rex(
    plc,
    r'\t\t// Generic UI\[24\] policy is the only approved 0x7E50 write target\.\n\t\t// Encoder maintenance objects are available only through 0x7E53\.\n\t\tif LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = FALSE then.*?\t\tend_case;\n',
    new_gate,
    'PLC address allowlist gate')

plc = once(
    plc,
    '''\t\t// Conservative local policy range; this is not the vendor UI range.\n\t\twriteValue := WriteData$DINT;\n\t\tif (writeValue < -1073741823) | (writeValue > 1073741823) then\n\t\t\tDetailCode := 12;\n\t\t\tRETURN;\n\t\tend_if;\n''',
    '''\t\t// UI[24] is a known preset with a conservative qualification range.\n\t\t// The range does not make UI[24] the generic address admission gate.\n\t\tif (ObjectIndex = 0x2F00) & (SubIndex = 24) &\n\t\t\t(ValueType = 4) & (DataLength = 4) then\n\t\t\twriteValue := WriteData$DINT;\n\t\t\tif (writeValue < -1073741823) | (writeValue > 1073741823) then\n\t\t\t\tDetailCode := 12;\n\t\t\t\tRETURN;\n\t\t\tend_if;\n\t\tend_if;\n''',
    'PLC UI24 range')

# Extend the existing semantic raw-write deny list, preserving 0x6060.
plc = rex(
    plc,
    r'\(ObjectIndex = 0x6040\) \| \(ObjectIndex = 0x6060\) \|\n\t\t\t\(ObjectIndex = 0x607A\) \| \(ObjectIndex = 0x60FF\) \|\n\t\t\t\(ObjectIndex = 0x6071\)',
    '''(ObjectIndex = 0x6040) | (ObjectIndex = 0x6060) |\n\t\t\t(ObjectIndex = 0x607A) | (ObjectIndex = 0x60FF) |\n\t\t\t(ObjectIndex = 0x6071) | (ObjectIndex = 0x3204) |\n\t\t\t(ObjectIndex = 0x20FC)''',
    'PLC semantic block list')

plc = rex(
    plc,
    r'\t\tif \(LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = TRUE\) &\n\t\t\t\(\(LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED = TRUE\) \|\n\t\t\t \(LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED = TRUE\) \|\n\t\t\t \(LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED = TRUE\) \|\n\t\t\t \(LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED = TRUE\)\) then',
    '''\t\tif LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = TRUE then''',
    'PLC capability advertisement')

PLC.write_text(plc, encoding='utf-8')
print('SDO-R03 current-dev source/test patches applied.')

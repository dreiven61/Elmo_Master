from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
SDK = ROOT / 'LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs'
TESTS = ROOT / 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs'
PLC = ROOT / 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'


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

sdk = SDK.read_text(encoding='utf-8')

# Raw generic SDO must never bypass semantic motion/control owners.
unsafe_old = '''                case 0x6040:\n                case 0x607A:\n                case 0x60FF:\n                case 0x6071:\n'''
unsafe_new = '''                case 0x6040:\n                case 0x6060:\n                case 0x607A:\n                case 0x60FF:\n                case 0x6071:\n'''
count = sdk.count(unsafe_old)
if count < 1:
    raise RuntimeError('SDK unsafe-object switch was not found')
sdk = sdk.replace(unsafe_old, unsafe_new)

sdk = sdk.replace(
    '''    /// Immutable compile-time SDO Write target intended to be mirrored by the\n    /// PLC policy. Applications must not treat an arbitrary SDO address as a\n    /// writable target, and submission still verifies both policies.\n''',
    '''    /// Immutable known SDO Write preset. Presets provide engineering metadata\n    /// and conservative value ranges, but they are not the generic D5 address\n    /// admission policy. Submission still enforces semantic-owner, type/length,\n    /// capability, axis-state, journal, and readback contracts.\n''')

sdk = sdk.replace(
    '''        // Axis 1 UI[24] is source-approved for an explicitly supervised live\n        // qualification. Drive-program ownership and hardware behavior still\n        // require PLC/runtime proof, so exact capability, axis-state, target,\n        // confirmation, journal, and readback interlocks remain mandatory.\n''',
    '''        // UI[24] remains a known qualification preset. Generic D5 scalar Write\n        // is no longer admitted by an address allowlist; semantic motion/control\n        // objects remain blocked and all capability, axis-state, confirmation,\n        // journal, no-replay, and readback interlocks remain mandatory.\n''')

sdk = replace_once(
    sdk,
    '''            if (approvedTargets.Count == 0)\n            {\n                blockers |= LMCSdoWritePolicyBlockers.NoApprovedTarget;\n            }\n''',
    '''            if (!SdoWriteEnabled)\n            {\n                blockers |= LMCSdoWritePolicyBlockers.NoApprovedTarget;\n            }\n''',
    'SDK generic policy evaluation')

new_require = '''        internal static void RequireSdoWriteAllowed(LMCSdoRequest request)\n        {\n            if (request == null)\n            {\n                throw new ArgumentNullException("request");\n            }\n\n            if (!request.IsWrite)\n            {\n                return;\n            }\n\n            if (!SdoWriteEnabled)\n            {\n                throw new NotSupportedException(\n                    "SDO Write is disabled by the SDK policy gate.");\n            }\n\n            if (request.SlaveReference < 1 || request.SlaveReference > 4)\n            {\n                throw new NotSupportedException(\n                    "Generic SDO Write supports SlaveReference 1 through 4 only.");\n            }\n\n            if (request.ObjectIndex == 0\n                || LMCSdoRequest.IsPermanentlyUnsafeObject(request.ObjectIndex))\n            {\n                throw new NotSupportedException(\n                    "Generic SDO Write cannot target semantic motion/control-owner objects.");\n            }\n\n            var expectedLength =\n                LMCDiagnosticsSdoPolicy.ExpectedReadLength(request.ValueType);\n            if (request.DataLength != expectedLength\n                || request.DataLength > 4)\n            {\n                throw new NotSupportedException(\n                    "Generic SDO Write requires canonical 1/2/4-byte scalar type lengths.");\n            }\n\n            var data = request.WriteDataUnsafe;\n            if (data == null || data.Length != request.DataLength)\n            {\n                throw new NotSupportedException(\n                    "Generic SDO Write requires an exact canonical payload length.");\n            }\n\n            if (request.ValueType == LMCSignalValueType.Bool\n                && data[0] > 1)\n            {\n                throw new NotSupportedException(\n                    "Generic Bool SDO Write accepts only 0 or 1.");\n            }\n\n            // Preserve the conservative vendor-specific qualification range for\n            // the known UI[24] preset without making that preset an address gate.\n            if (request.ObjectIndex == 0x2F00\n                && request.SubIndex == 24\n                && request.ValueType == LMCSignalValueType.Int32\n                && request.DataLength == 4)\n            {\n                var value = unchecked((int)(\n                    (uint)data[0]\n                    | ((uint)data[1] << 8)\n                    | ((uint)data[2] << 16)\n                    | ((uint)data[3] << 24)));\n                if (value < -1073741823 || value > 1073741823)\n                {\n                    throw new NotSupportedException(\n                        "The UI[24] qualification preset value is outside its conservative range.");\n                }\n            }\n        }\n\n'''
sdk = regex_once(
    sdk,
    r'        internal static void RequireSdoWriteAllowed\(LMCSdoRequest request\)\n        \{.*?\n        \}\n\n(?=        internal static void RequireSdoWriteVerificationCapabilities)',
    new_require,
    'SDK RequireSdoWriteAllowed')

if 'target is not in the SDK compile-time allowlist' in sdk:
    raise RuntimeError('stale SDK SDO address-allowlist rejection remains')
SDK.write_text(sdk, encoding='utf-8')

# Replace only the policy-focused contract test. Known UI[24] preset tests remain,
# but generic addresses/slaves must now be accepted and semantic objects rejected.
tests = TESTS.read_text(encoding='utf-8')
new_test = '''        private static void SdoWriteTargetPolicy()\n        {\n            IReadOnlyList<LMCSdoWriteTarget> approved;\n            using (var connection = new LMCConnection())\n            {\n                approved = connection.Diagnostics.GetApprovedSdoWriteTargets();\n                AssertEx.Equal(1, approved.Count);\n            }\n\n            // UI[24] remains a known engineering preset, not an address gate.\n            AssertEx.Equal("Reserved diagnostic UI[24]", approved[0].DisplayName);\n            AssertEx.Equal((ushort)1, approved[0].SlaveReference);\n            AssertEx.Equal((ushort)0x2F00, approved[0].ObjectIndex);\n            AssertEx.Equal((byte)24, approved[0].SubIndex);\n            AssertEx.Equal(LMCSignalValueType.Int32, approved[0].ValueType);\n            AssertEx.Equal((ushort)4, approved[0].DataLength);\n            AssertEx.Equal(-1073741823L, approved[0].MinimumIntegerValue);\n            AssertEx.Equal(1073741823L, approved[0].MaximumIntegerValue);\n            LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(\n                approved[0].CreateRequest(0, 100));\n\n            var genericWrites = new[]\n            {\n                LMCSdoRequest.CreateWrite(\n                    1, 0x2000, 1, LMCSignalValueType.UInt8,\n                    TestFrame.Hex("5A"), 100),\n                LMCSdoRequest.CreateWrite(\n                    2, 0x2001, 2, LMCSignalValueType.UInt16,\n                    TestFrame.Hex("34 12"), 100),\n                LMCSdoRequest.CreateWrite(\n                    3, 0x3000, 7, LMCSignalValueType.Int32,\n                    TestFrame.Hex("FE FF FF FF"), 100),\n                LMCSdoRequest.CreateWrite(\n                    4, 0x3001, 255, LMCSignalValueType.Real32,\n                    TestFrame.Hex("00 00 80 3F"), 100)\n            };\n            foreach (var generic in genericWrites)\n            {\n                LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(generic);\n            }\n\n            // Semantic owners cannot be bypassed by the raw generic route.\n            foreach (var blockedObject in new ushort[]\n            {\n                0x6040, 0x6060, 0x607A, 0x60FF, 0x6071\n            })\n            {\n                var blocked = LMCSdoRequest.CreateWrite(\n                    1, blockedObject, 0, LMCSignalValueType.UInt16,\n                    TestFrame.Hex("00 00"), 100);\n                AssertEx.Throws<NotSupportedException>(\n                    () => LMCDiagnosticsWritePolicy\n                        .RequireSdoWriteAllowed(blocked));\n            }\n\n            // The UI[24] preset retains its conservative value guard on every\n            // slave even though the address itself is no longer allow-listed.\n            var outOfRangeUi24 = LMCSdoRequest.CreateWrite(\n                4, 0x2F00, 24, LMCSignalValueType.Int32,\n                TestFrame.Hex("FF FF FF 7F"), 100);\n            AssertEx.Throws<NotSupportedException>(\n                () => LMCDiagnosticsWritePolicy\n                    .RequireSdoWriteAllowed(outOfRangeUi24));\n\n            AssertEx.Throws<ArgumentNullException>(\n                () => LMCDiagnosticsWritePolicy\n                    .RequireSdoWriteVerificationCapabilities(null));\n            AssertEx.Throws<NotSupportedException>(\n                () => LMCDiagnosticsWritePolicy\n                    .RequireSdoWriteVerificationCapabilities(\n                        SdoCapabilities(\n                            LMCDiagnosticCapability.SDOWrite\n                                | LMCDiagnosticCapability.SDORead)));\n            AssertEx.Throws<NotSupportedException>(\n                () => LMCDiagnosticsWritePolicy\n                    .RequireSdoWriteVerificationCapabilities(\n                        SdoCapabilities(\n                            LMCDiagnosticCapability.SDOWrite\n                                | LMCDiagnosticCapability\n                                    .SDOReadGeneralInline)));\n            LMCDiagnosticsWritePolicy\n                .RequireSdoWriteVerificationCapabilities(\n                    SdoCapabilities(\n                        LMCDiagnosticCapability.SDOWrite\n                            | LMCDiagnosticCapability.SDORead\n                            | LMCDiagnosticCapability\n                                .SDOReadGeneralInline));\n        }\n\n'''
tests = regex_once(
    tests,
    r'        private static void SdoWriteTargetPolicy\(\)\n        \{.*?\n        \}\n\n(?=        private static void SdoRequestValidation)',
    new_test,
    'DiagnosticsD5ContractTests.SdoWriteTargetPolicy')
TESTS.write_text(tests, encoding='utf-8')

plc = PLC.read_text(encoding='utf-8')
old_gate = '''\t\t// Generic UI[24] policy is the only approved 0x7E50 write target.\n\t\t// Encoder maintenance objects are available only through 0x7E53.\n\t\tif LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = FALSE then\n\t\t\tDetailCode := 7;\n\t\t\tRETURN;\n\t\tend_if;\n\t\tif (SlaveReference < 1) | (SlaveReference > 4) |\n\t\t\t(ObjectIndex <> 0x2F00) | (SubIndex <> 24) |\n\t\t\t(ValueType <> 4) | (DataLength <> 4) then\n\t\t\tDetailCode := 7;\n\t\t\tRETURN;\n\t\tend_if;\n\t\tcase SlaveReference of\n\t\t\t1:\n\t\t\t\tif LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED = FALSE then\n\t\t\t\t\tDetailCode := 7;\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n\t\t\t2:\n\t\t\t\tif LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED = FALSE then\n\t\t\t\t\tDetailCode := 7;\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n\t\t\t3:\n\t\t\t\tif LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED = FALSE then\n\t\t\t\t\tDetailCode := 7;\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n\t\t\t4:\n\t\t\t\tif LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED = FALSE then\n\t\t\t\t\tDetailCode := 7;\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n\t\tend_case;\n'''
new_gate = '''\t\t// Generic scalar D5 Write is admitted by shape and semantic ownership,\n\t\t// not by a compile-time object-address list. Encoder maintenance remains\n\t\t// available only through its dedicated 0x7E53 owner.\n\t\tif LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = FALSE then\n\t\t\tDetailCode := 7;\n\t\t\tRETURN;\n\t\tend_if;\n\t\tif (SlaveReference < 1) | (SlaveReference > 4) |\n\t\t\t(ObjectIndex = 0) then\n\t\t\tDetailCode := 7;\n\t\t\tRETURN;\n\t\tend_if;\n\n\t\tcase ValueType of\n\t\t\t1, 9, 10, 11:\n\t\t\t\tif DataLength <> 1 then\n\t\t\t\t\tDetailCode := 7;\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n\t\t\t2, 3, 7:\n\t\t\t\tif DataLength <> 2 then\n\t\t\t\t\tDetailCode := 7;\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n\t\t\t4, 5, 6, 8:\n\t\t\t\tif DataLength <> 4 then\n\t\t\t\t\tDetailCode := 7;\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n\t\telse\n\t\t\tDetailCode := 7;\n\t\t\tRETURN;\n\t\tend_case;\n\n\t\tif (ValueType = 1) & (WriteData <> 0) & (WriteData <> 1) then\n\t\t\tDetailCode := 12;\n\t\t\tRETURN;\n\t\tend_if;\n'''
plc = replace_once(plc, old_gate, new_gate, 'PLC generic address gate')

old_range = '''\t\t// Conservative local policy range; this is not the vendor UI range.\n\t\twriteValue := WriteData$DINT;\n\t\tif (writeValue < -1073741823) | (writeValue > 1073741823) then\n\t\t\tDetailCode := 12;\n\t\t\tRETURN;\n\t\tend_if;\n'''
new_range = '''\t\t// Preserve the existing conservative UI[24] qualification range only\n\t\t// for that known preset. It is no longer the generic address policy.\n\t\tif (ObjectIndex = 0x2F00) & (SubIndex = 24) &\n\t\t\t(ValueType = 4) & (DataLength = 4) then\n\t\t\twriteValue := WriteData$DINT;\n\t\t\tif (writeValue < -1073741823) | (writeValue > 1073741823) then\n\t\t\t\tDetailCode := 12;\n\t\t\t\tRETURN;\n\t\t\tend_if;\n\t\tend_if;\n'''
plc = replace_once(plc, old_range, new_range, 'PLC UI24 preset range')

old_cap = '''\t\tif (LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = TRUE) &\n\t\t\t((LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED = TRUE) |\n\t\t\t (LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED = TRUE) |\n\t\t\t (LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED = TRUE) |\n\t\t\t (LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED = TRUE)) then\n\t\t\t(pResponse + 20)^$UDINT :=\n\t\t\t\t(pResponse + 20)^$UDINT OR 0x00000200;\n\t\tend_if;\n'''
new_cap = '''\t\tif LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = TRUE then\n\t\t\t(pResponse + 20)^$UDINT :=\n\t\t\t\t(pResponse + 20)^$UDINT OR 0x00000200;\n\t\tend_if;\n'''
plc = replace_once(plc, old_cap, new_cap, 'PLC SDO Write capability advertisement')

if '(ObjectIndex <> 0x2F00) | (SubIndex <> 24)' in plc:
    raise RuntimeError('stale PLC UI24 address gate remains')
PLC.write_text(plc, encoding='utf-8')

print('SDO-R03 generic scalar Write policy patch applied successfully.')

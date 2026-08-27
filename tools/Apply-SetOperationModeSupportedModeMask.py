from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def read(rel):
    return (ROOT / rel).read_text(encoding="utf-8")


def write(rel, text):
    (ROOT / rel).write_text(text, encoding="utf-8", newline="")


def replace_once(rel, old, new):
    text = read(rel)
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{rel}: expected exactly one match, got {count}: {old[:120]!r}")
    write(rel, text.replace(old, new, 1))


# SDK model: preserve the 40-byte schema and expose the former reserved UInt16
# as a fail-closed SetOperationMode support mask.
models = "LMC_Library/LMC_API_Delivery/src/LmcAdminModels.cs"
replace_once(
    models,
    "            ushort groupReference,\n            ushort maxGroupParameterCount,\n            ushort errorCatalogVersion)\n",
    "            ushort groupReference,\n            ushort maxGroupParameterCount,\n            ushort errorCatalogVersion,\n            ushort setOperationModeSupportedMask)\n")
replace_once(
    models,
    "            ErrorCatalogVersion = errorCatalogVersion;\n",
    "            ErrorCatalogVersion = errorCatalogVersion;\n            SetOperationModeSupportedMask = setOperationModeSupportedMask;\n")
replace_once(
    models,
    "        public ushort ErrorCatalogVersion { get; private set; }\n\n        internal long ConnectionSessionGeneration",
    "        public ushort ErrorCatalogVersion { get; private set; }\n        public ushort SetOperationModeSupportedMask { get; private set; }\n\n        internal long ConnectionSessionGeneration")
replace_once(
    models,
    "        public bool Supports(LMCAxisParameterKey key)\n",
    "        public bool SupportsSetOperationMode(LMCDriveOperationMode mode)\n        {\n            var raw = (int)(sbyte)mode;\n            if (raw < 0 || raw > 15)\n            {\n                return false;\n            }\n\n            return (SetOperationModeSupportedMask & (1 << raw)) != 0;\n        }\n\n        public bool Supports(LMCAxisParameterKey key)\n")

# SDK parser: byte-compatible schema. Offset 38 is no longer reserved; it is a
# UInt16 mask where bit N means positive DS402 mode N is advertised.
protocol = "LMC_Library/LMC_API_Delivery/src/LmcAdminProtocol.cs"
replace_once(
    protocol,
    "            var errorCatalogVersion = LMC_Frame.ReadUInt16(payload, 36);\n            var reserved = LMC_Frame.ReadUInt16(payload, 38);\n",
    "            var errorCatalogVersion = LMC_Frame.ReadUInt16(payload, 36);\n            var setOperationModeSupportedMask =\n                LMC_Frame.ReadUInt16(payload, 38);\n")
replace_once(
    protocol,
    "            const uint knownAxisMask = 0x0000003Fu;\n\n            if ((features & ~knownFeatures) != 0",
    "            const uint knownAxisMask = 0x0000003Fu;\n            const ushort knownSetOperationModeSupportedMask = 0x018A;\n\n            if ((features & ~knownFeatures) != 0")
replace_once(
    protocol,
    "                || (groupSelection & ~LMCGroupParameterSelection.All) != 0\n                || reserved != 0)\n",
    "                || (groupSelection & ~LMCGroupParameterSelection.All) != 0\n                || (setOperationModeSupportedMask\n                    & ~knownSetOperationModeSupportedMask) != 0)\n")
replace_once(
    protocol,
    "            return new LMCAdminCapabilities(\n",
    "            var setOperationModeFeatures = features\n                & (LMCAdminFeature.AxisSetOperationModeStart\n                    | LMCAdminFeature.AxisSetOperationModeOutcomeRead\n                    | LMCAdminFeature.AxisSetOperationModeOutcomeRetire);\n            var fullSetOperationModeTriad =\n                LMCAdminFeature.AxisSetOperationModeStart\n                | LMCAdminFeature.AxisSetOperationModeOutcomeRead\n                | LMCAdminFeature.AxisSetOperationModeOutcomeRetire;\n            if ((setOperationModeFeatures == LMCAdminFeature.None\n                    && setOperationModeSupportedMask != 0)\n                || (setOperationModeFeatures == fullSetOperationModeTriad\n                    && setOperationModeSupportedMask == 0))\n            {\n                throw new InvalidDataException(\n                    \"GetAdminCapabilities SetOperationMode triad and supported-mode mask are inconsistent.\");\n            }\n\n            return new LMCAdminCapabilities(\n")
replace_once(
    protocol,
    "                groupReference,\n                maxGroupParameterCount,\n                errorCatalogVersion);\n",
    "                groupReference,\n                maxGroupParameterCount,\n                errorCatalogVersion,\n                setOperationModeSupportedMask);\n")

# PLC AdminCapabilities: keep payload length 40. The supported-mode mask is
# advertised only when the exact Start/Outcome/Retire triad is enabled.
control = "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st"
replace_once(
    control,
    "\t\t\t(pResponseFrame + 44)^$UINT := 6;\n\t\t\t(pResponseFrame + 46)^$UINT := 0;\n\t\t\tResponseSize := 48;",
    "\t\t\t(pResponseFrame + 44)^$UINT := 6;\n\t\t\tif (((pResponseFrame + 24)^$UDINT and 0x00000700) = 0x00000700) then\n\t\t\t\t// bits: PP(1), PV(3), IP(7), CSP(8)\n\t\t\t\t(pResponseFrame + 46)^$UINT := 0x018A;\n\t\t\telse\n\t\t\t\t(pResponseFrame + 46)^$UINT := 0;\n\t\t\tend_if;\n\t\t\tResponseSize := 48;")

# LASAL SetOperationMode dormant software path from the prior qualification
# branch. Production activation remains FALSE.
diag = "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st"
replace_once(
    diag,
    "#define LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE\n#define LMC_DIAG_DS402_PREFLIGHT_READY -2",
    "#define LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE\n#define LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE\n#define LMC_DIAG_DS402_PREFLIGHT_READY -2")
replace_once(
    diag,
    "\telsif requestedMode <> 8 then\n\t\tdetailCode := LMC_DIAG_MODE_DETAIL_UNSUPPORTED;",
    "\telsif (requestedMode <> 8) &\n\t      ((LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES = FALSE) |\n\t       ((requestedMode <> 1) & (requestedMode <> 3) & (requestedMode <> 7))) then\n\t\tdetailCode := LMC_DIAG_MODE_DETAIL_UNSUPPORTED;")
replace_once(
    diag,
    "\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_WRITE_DATA] := 8;",
    "\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_WRITE_DATA] := TO_DINT(requestedMode);")
replace_once(
    diag,
    "\telsif requestedMode <> 8 then\n\t\tdetailCode := LMC_DIAG_MODE_DETAIL_KEY_MISMATCH;",
    "\telsif (requestedMode <> 8) &\n\t      ((LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES = FALSE) |\n\t       ((requestedMode <> 1) & (requestedMode <> 3) & (requestedMode <> 7))) then\n\t\tdetailCode := LMC_DIAG_MODE_DETAIL_KEY_MISMATCH;")
replace_once(
    diag,
    "\t\t\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_WRITE_DATA] := 8;",
    "\t\t\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_WRITE_DATA] :=\n\t\t\t\t\tAxisOperationModeState[recoveryScanBase + 10];")
replace_once(
    diag,
    "\t\t\t\tif observedMode = 8 then",
    "\t\t\t\tif observedMode = AxisOperationModeState[recordBase + 10]$SINT then")

# Existing contract test name/body must follow the promoted multi-mode SDK path.
tests = "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminSetOperationModeContractTests.cs"
replace_once(
    tests,
    "                \"Contract.Admin.SetOperationMode.CspOnlyImmediate\",\n                CspOnlyImmediate);",
    "                \"Contract.Admin.SetOperationMode.SoftwareAllowListImmediate\",\n                SoftwareAllowListImmediate);\n            tests.Add(\n                \"Response.Admin.SetOperationMode.SupportedModeMaskStrict\",\n                SupportedModeMaskStrict);")
old_csp = '''        private static void CspOnlyImmediate()\n        {\n            AssertEx.Throws<NotSupportedException>(\n                () => new LMCAxisSetOperationModeRecoveryKey(\n                    1,\n                    OriginalRequestId,\n                    DiagnosticsBuild,\n                    DiagnosticsBootId,\n                    MapRevision,\n                    Intent0,\n                    Intent1,\n                    Intent2,\n                    Intent3,\n                    2,\n                    LMCDriveOperationMode.Homing,\n                    TimeoutMilliseconds));\n            AssertEx.Throws<ArgumentOutOfRangeException>(\n                () => new LMCAxisSetOperationModeRecoveryKey(\n                    1,\n                    OriginalRequestId,\n                    DiagnosticsBuild,\n                    DiagnosticsBootId,\n                    MapRevision,\n                    Intent0,\n                    Intent1,\n                    Intent2,\n                    Intent3,\n                    2,\n                    LMCDriveOperationMode.CyclicSynchronousPosition,\n                    0));\n            AssertEx.Throws<ArgumentException>(\n                () => new LMCAxisSetOperationModeClientIntentId(\n                    0,\n                    0,\n                    0,\n                    0));\n        }\n'''
new_multi = '''        private static void SoftwareAllowListImmediate()\n        {\n            foreach (var allowed in new[]\n            {\n                LMCDriveOperationMode.ProfilePosition,\n                LMCDriveOperationMode.ProfileVelocity,\n                LMCDriveOperationMode.InterpolatedPosition,\n                LMCDriveOperationMode.CyclicSynchronousPosition\n            })\n            {\n                var key = new LMCAxisSetOperationModeRecoveryKey(\n                    1, OriginalRequestId, DiagnosticsBuild, DiagnosticsBootId,\n                    MapRevision, Intent0, Intent1, Intent2, Intent3, 2,\n                    allowed, TimeoutMilliseconds);\n                AssertEx.Equal(allowed, key.RequestedMode);\n            }\n\n            foreach (var blocked in new[]\n            {\n                LMCDriveOperationMode.NoModeAssigned,\n                LMCDriveOperationMode.Velocity,\n                LMCDriveOperationMode.ProfileTorque,\n                LMCDriveOperationMode.Homing,\n                LMCDriveOperationMode.CyclicSynchronousVelocity,\n                LMCDriveOperationMode.CyclicSynchronousTorque\n            })\n            {\n                AssertEx.Throws<NotSupportedException>(\n                    () => new LMCAxisSetOperationModeRecoveryKey(\n                        1, OriginalRequestId, DiagnosticsBuild, DiagnosticsBootId,\n                        MapRevision, Intent0, Intent1, Intent2, Intent3, 2,\n                        blocked, TimeoutMilliseconds));\n            }\n\n            AssertEx.Throws<ArgumentOutOfRangeException>(\n                () => new LMCAxisSetOperationModeRecoveryKey(\n                    1, OriginalRequestId, DiagnosticsBuild, DiagnosticsBootId,\n                    MapRevision, Intent0, Intent1, Intent2, Intent3, 2,\n                    LMCDriveOperationMode.CyclicSynchronousPosition, 0));\n            AssertEx.Throws<ArgumentException>(\n                () => new LMCAxisSetOperationModeClientIntentId(0, 0, 0, 0));\n        }\n\n        private static void SupportedModeMaskStrict()\n        {\n            const ushort mask = 0x018A;\n            var capabilities = LMC_AdminParser.ParseCapabilities(\n                TestFrame.Response(\n                    0,\n                    CapabilitiesPayload(\n                        OriginalRequestId, CapabilityTriad, 6, mask)),\n                OriginalRequestId,\n                1);\n            AssertEx.Equal(mask, capabilities.SetOperationModeSupportedMask);\n            AssertEx.True(capabilities.SupportsSetOperationMode(\n                LMCDriveOperationMode.ProfilePosition));\n            AssertEx.True(capabilities.SupportsSetOperationMode(\n                LMCDriveOperationMode.ProfileVelocity));\n            AssertEx.True(capabilities.SupportsSetOperationMode(\n                LMCDriveOperationMode.InterpolatedPosition));\n            AssertEx.True(capabilities.SupportsSetOperationMode(\n                LMCDriveOperationMode.CyclicSynchronousPosition));\n            AssertEx.False(capabilities.SupportsSetOperationMode(\n                LMCDriveOperationMode.Homing));\n\n            AssertEx.Throws<InvalidDataException>(\n                () => LMC_AdminParser.ParseCapabilities(\n                    TestFrame.Response(\n                        0,\n                        CapabilitiesPayload(\n                            OriginalRequestId, CapabilityTriad, 6, 0)),\n                    OriginalRequestId,\n                    1));\n            AssertEx.Throws<InvalidDataException>(\n                () => LMC_AdminParser.ParseCapabilities(\n                    TestFrame.Response(\n                        0,\n                        CapabilitiesPayload(\n                            OriginalRequestId, LMCAdminFeature.None, 1, mask)),\n                    OriginalRequestId,\n                    1));\n            AssertEx.Throws<InvalidDataException>(\n                () => LMC_AdminParser.ParseCapabilities(\n                    TestFrame.Response(\n                        0,\n                        CapabilitiesPayload(\n                            OriginalRequestId, CapabilityTriad, 6, 0x0200)),\n                    OriginalRequestId,\n                    1));\n        }\n'''
replace_once(tests, old_csp, new_multi)
replace_once(
    tests,
    "        private static byte[] CapabilitiesPayload(\n            uint requestId,\n            LMCAdminFeature features,\n            ushort errorCatalogVersion)\n        {\n            var payload = CommonPayload(requestId, 40);\n            TestFrame.WriteUInt32(payload, 16, (uint)features);\n            TestFrame.WriteUInt16(payload, 28, 4);\n            TestFrame.WriteUInt16(payload, 36, errorCatalogVersion);\n            return payload;\n        }",
    "        private static byte[] CapabilitiesPayload(\n            uint requestId,\n            LMCAdminFeature features,\n            ushort errorCatalogVersion,\n            ushort setOperationModeSupportedMask = 0)\n        {\n            var payload = CommonPayload(requestId, 40);\n            TestFrame.WriteUInt32(payload, 16, (uint)features);\n            TestFrame.WriteUInt16(payload, 28, 4);\n            TestFrame.WriteUInt16(payload, 36, errorCatalogVersion);\n            TestFrame.WriteUInt16(\n                payload, 38, setOperationModeSupportedMask);\n            return payload;\n        }")
replace_once(
    tests,
    "                    CapabilitiesPayload(\n                        requestId,\n                        features,\n                        (ushort)((features & CapabilityTriad)\n                                == CapabilityTriad\n                            ? 6\n                            : 1))));",
    "                    CapabilitiesPayload(\n                        requestId,\n                        features,\n                        (ushort)((features & CapabilityTriad)\n                                == CapabilityTriad\n                            ? 6\n                            : 1),\n                        (ushort)((features & CapabilityTriad)\n                                == CapabilityTriad\n                            ? 0x018A\n                            : 0))));")

print("SetOperationMode backend SupportedModeMask promotion applied.")

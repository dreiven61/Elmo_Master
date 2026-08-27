from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st"
VERIFY = ROOT / "tools/Verify-SetOperationModeStatic.ps1"

source = SOURCE.read_text(encoding="utf-8")
verify = VERIFY.read_text(encoding="utf-8")

old_candidate = """\t\t\t   (AxisOperationModeState[recoveryScanBase + 9] = recoveryScanAxis) &\n\t\t\t   (AxisOperationModeState[recoveryScanBase + 10]$SINT = 8) &\n\t\t\t   (AxisOperationModeState[recoveryScanBase + 27]$UDINT <> 0) &\n\t\t\t   (AxisOperationModeState[recoveryScanBase + 28]$UDINT <> 0) then\n"""
new_candidate = """\t\t\t   (AxisOperationModeState[recoveryScanBase + 9] = recoveryScanAxis) &\n\t\t\t   ((AxisOperationModeState[recoveryScanBase + 10]$SINT = 8) |\n\t\t\t    ((LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES <> FALSE) &\n\t\t\t     ((AxisOperationModeState[recoveryScanBase + 10]$SINT = 1) |\n\t\t\t      (AxisOperationModeState[recoveryScanBase + 10]$SINT = 3) |\n\t\t\t      (AxisOperationModeState[recoveryScanBase + 10]$SINT = 7)))) &\n\t\t\t   (AxisOperationModeState[recoveryScanBase + 22]$UDINT <> 0) &\n\t\t\t   (AxisOperationModeState[recoveryScanBase + 27]$UDINT <> 0) &\n\t\t\t   (AxisOperationModeState[recoveryScanBase + 28]$UDINT <> 0) &\n\t\t\t   (AxisOperationModeState[recoveryScanBase + 29]$UDINT <> 0) &\n\t\t\t   (AxisOperationModeState[recoveryScanBase + 30]$UDINT <> 0) then\n"""

if source.count(old_candidate) != 1:
    raise SystemExit(f"expected exactly one CSP-only warm-start candidate block, found {source.count(old_candidate)}")
source = source.replace(old_candidate, new_candidate, 1)

old_duplicate = """\t\t\t\tif recoveryCandidateFound then\n\t\t\t\t\t// More than one candidate violates the singleton SDO owner model.\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n"""
new_duplicate = """\t\t\t\tif recoveryCandidateFound then\n\t\t\t\t\t// More than one candidate violates the singleton SDO owner model.\n\t\t\t\t\t// Clear the first staged candidate before returning so a later cycle\n\t\t\t\t\t// cannot continue recovery from an ambiguous retained set.\n\t\t\t\t\t_memset(dest:=#AxisOperationModeState[LMC_DIAG_MODE_RUNTIME_BASE],\n\t\t\t\t\t\tusByte:=0, cntr:=32 * 4);\n\t\t\t\t\tRETURN;\n\t\t\t\tend_if;\n"""
if source.count(old_duplicate) != 1:
    raise SystemExit(f"expected exactly one duplicate-candidate fail-closed block, found {source.count(old_duplicate)}")
source = source.replace(old_duplicate, new_duplicate, 1)

anchor = """    Assert-Regex $processMode 'TryStartWrite\\([^;\\r\\n]*ObjectIndex:=0x6060' 'main processor owns no 0x6060 write site after split' -ExpectedCount 0\n}\n"""
insert = """    Assert-Regex $processMode 'TryStartWrite\\([^;\\r\\n]*ObjectIndex:=0x6060' 'main processor owns no 0x6060 write site after split' -ExpectedCount 0\n    Assert-Regex $processMode 'recoveryScanBase \\+ 10\\]\\$SINT = 8[\\s\\S]{0,260}recoveryScanBase \\+ 10\\]\\$SINT = 1[\\s\\S]{0,180}recoveryScanBase \\+ 10\\]\\$SINT = 3[\\s\\S]{0,180}recoveryScanBase \\+ 10\\]\\$SINT = 7' 'MODE-11E warm-start accepts exact PP/PV/IP/CSP candidate set' -ExpectedCount 1\n    Assert-Regex $processMode 'LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES[\\t ]*<>[\\t ]*FALSE' 'MODE-11E non-CSP recovery follows loaded-image software-mode gate' -ExpectedCount 1\n    Assert-Regex $processMode 'recoveryScanBase \\+ 22\\]\\$UDINT <> 0[\\s\\S]{0,220}recoveryScanBase \\+ 27\\]\\$UDINT <> 0[\\s\\S]{0,160}recoveryScanBase \\+ 28\\]\\$UDINT <> 0[\\s\\S]{0,160}recoveryScanBase \\+ 29\\]\\$UDINT <> 0[\\s\\S]{0,160}recoveryScanBase \\+ 30\\]\\$UDINT <> 0' 'MODE-11E warm-start requires record generation and complete owner/session identity' -ExpectedCount 1\n    Assert-Regex $processMode 'if recoveryCandidateFound then[\\s\\S]{0,360}_memset\\(dest:=#AxisOperationModeState\\[LMC_DIAG_MODE_RUNTIME_BASE\\][\\s\\S]{0,120}RETURN;' 'MODE-11E multiple retained candidates clear staged runtime and fail closed' -ExpectedCount 1\n}\n"""
if verify.count(anchor) != 1:
    raise SystemExit(f"expected static-verifier main-processor anchor once, found {verify.count(anchor)}")
verify = verify.replace(anchor, insert, 1)

# Defensive semantic checks before writing anything.
for mode in (1, 3, 7, 8):
    needle = f"AxisOperationModeState[recoveryScanBase + 10]$SINT = {mode}"
    if source.count(needle) < 1:
        raise SystemExit(f"missing warm-start mode candidate {mode}")
if "AxisOperationModeState[recoveryScanBase + 22]$UDINT <> 0" not in source:
    raise SystemExit("missing retained record generation fence")
if "Clear the first staged candidate" not in source:
    raise SystemExit("missing duplicate-candidate runtime clear")

SOURCE.write_text(source, encoding="utf-8", newline="\n")
VERIFY.write_text(verify, encoding="utf-8", newline="\n")
print("MODE-11E promotion patch applied")

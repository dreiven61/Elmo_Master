from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
WPF_DIAG = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs'
WPF_SDO = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs'
WPF_OPMODE = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisSetOperationModeRecovery.cs'
PLC = ROOT / 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
DESIGN = ROOT / 'docs/api/design/SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'


def replace_exact(path, old, new, expected, label):
    text = path.read_text(encoding='utf-8')
    count = text.count(old)
    if count != expected:
        raise RuntimeError(f'{label}: expected {expected}, found {count}')
    path.write_text(text.replace(old, new), encoding='utf-8')


# Ordinary generic SDO Write must not reuse the stricter same-value qualification
# requirement that the axis be fully powered off. Keep that requirement as the
# default for every qualification caller; only the ordinary manual Write calls
# opt into the runtime-safe relaxed path.
replace_exact(
    WPF_SDO,
    '''        private async Task VerifyD5SdoQualificationSafeAxisAsync(\n            LMCConnection currentConnection,\n            ushort slaveReference,\n            string axisObjectName,\n            CancellationToken cancellationToken)\n''',
    '''        private async Task VerifyD5SdoQualificationSafeAxisAsync(\n            LMCConnection currentConnection,\n            ushort slaveReference,\n            string axisObjectName,\n            CancellationToken cancellationToken,\n            bool requirePowerOff = true)\n''',
    1,
    'SDO safety helper signature')

replace_exact(
    WPF_SDO,
    '''                if (latestStatus.IsPowerOn || !latestStatus.IsStandstill)\n                {\n                    throw new InvalidOperationException(\n                        "D5 SDO qualification requires "\n                        + axisObjectName\n                        + " PowerOn=False and Standstill=True. Actual PowerOn="\n                        + latestStatus.IsPowerOn\n                        + ", Standstill="\n                        + latestStatus.IsStandstill\n                        + ".");\n                }\n''',
    '''                var ds402BaseState =\n                    (uint)latestStatus.StatusWord & 0x0000006Fu;\n                var ds402Fault =\n                    ((uint)latestStatus.StatusWord & 0x00000008u) != 0;\n                var ds402OperationEnabled =\n                    ((uint)latestStatus.StatusWord & 0x00000004u) != 0;\n\n                if ((requirePowerOff && latestStatus.IsPowerOn)\n                    || !latestStatus.IsStandstill\n                    || ds402Fault\n                    || ds402OperationEnabled)\n                {\n                    var requiredState = requirePowerOff\n                        ? "PowerOn=False, Standstill=True, DS402 Fault=False, and OperationEnabled=False"\n                        : "Standstill=True, DS402 Fault=False, and OperationEnabled=False";\n                    throw new InvalidOperationException(\n                        "D5 SDO safety preflight requires "\n                        + axisObjectName\n                        + " "\n                        + requiredState\n                        + ". Actual PowerOn="\n                        + latestStatus.IsPowerOn\n                        + ", Standstill="\n                        + latestStatus.IsStandstill\n                        + ", StatusWord=0x"\n                        + latestStatus.StatusWord.ToString("X4", CultureInfo.InvariantCulture)\n                        + ", DS402BaseState=0x"\n                        + ds402BaseState.ToString("X2", CultureInfo.InvariantCulture)\n                        + ".");\n                }\n''',
    1,
    'SDO strict qualification safety block')

ordinary_call = '''                            await VerifyD5SdoQualificationSafeAxisAsync(\n                                currentConnection,\n                                request.SlaveReference,\n                                "_LMCAxis"\n                                    + request.SlaveReference.ToString(\n                                        CultureInfo.InvariantCulture),\n                                CancellationToken.None);\n'''
ordinary_call_relaxed = '''                            await VerifyD5SdoQualificationSafeAxisAsync(\n                                currentConnection,\n                                request.SlaveReference,\n                                "_LMCAxis"\n                                    + request.SlaveReference.ToString(\n                                        CultureInfo.InvariantCulture),\n                                CancellationToken.None,\n                                false);\n'''
replace_exact(
    WPF_DIAG,
    ordinary_call,
    ordinary_call_relaxed,
    2,
    'ordinary SDO Write safety calls')

# PLC generic Write admission: semantic/dedicated-owner objects are already
# blocked earlier. For the remaining generic scalar objects, allow safe
# non-enabled DS402 states rather than requiring only Switch On Disabled.
replace_exact(
    PLC,
    '''\telsif ((pSnapshot + axisHealthOffset + 24)^$DINT <> 0) |\n\t\t((statusWord and 0x0000006F) <> 0x00000040) then\n\t\tDetailCode := 19;\n''',
    '''\telsif ((pSnapshot + axisHealthOffset + 24)^$DINT <> 0) |\n\t\t(((statusWord and 0x0000006F) <> 0x00000040) &\n\t\t ((statusWord and 0x0000006F) <> 0x00000021) &\n\t\t ((statusWord and 0x0000006F) <> 0x00000023)) then\n\t\tDetailCode := 19;\n''',
    1,
    'PLC generic SDO safe DS402 state gate')

# Make the SetOperationMode distinction explicit on the PC before arming the
# durable one-shot Start. Same-target (for example CSP->CSP) is allowed as a
# no-write candidate. A real cross-mode transition is sent only after fresh
# status/0x6041/0x6061 evidence proves standstill, fault clear, and Operation
# Enabled clear, exactly matching the PLC preflight safety contract.
opmode_helper = '''        private async Task VerifyAxisSetOperationModeTransitionPreflightAsync(\n            LMCSingleAxis currentAxis,\n            LMCDriveOperationMode requestedMode)\n        {\n            if (currentAxis == null)\n            {\n                throw new ArgumentNullException("currentAxis");\n            }\n\n            var driveStatus = await currentAxis.ReadDriveStatusAsync(\n                CancellationToken.None);\n            var axisStatus = driveStatus.AxisStatus;\n            if (axisStatus == null\n                || !axisStatus.IsReadSuccessful\n                || axisStatus.HasAxisError)\n            {\n                throw new InvalidOperationException(\n                    "SetOperationMode preflight could not prove a clean LASAL axis state. No Start was sent.");\n            }\n\n            if (driveStatus.OperationMode == requestedMode)\n            {\n                WriteLog(\n                    "SetOperationMode preflight: current 0x6061 already equals requested mode "\n                    + ((sbyte)requestedMode).ToString(CultureInfo.InvariantCulture)\n                    + ". Start may complete as SucceededNoWrite; this does not prove a 0x6060 cross-mode Write.");\n                return;\n            }\n\n            var statusWord = driveStatus.Ds402StatusWord;\n            var ds402Fault = (statusWord & 0x0008) != 0;\n            var ds402OperationEnabled = (statusWord & 0x0004) != 0;\n            if (!axisStatus.IsStandstill\n                || ds402Fault\n                || ds402OperationEnabled)\n            {\n                throw new InvalidOperationException(\n                    "SetOperationMode cross-mode preflight failed. No Start was sent. "\n                    + "A real 0x6060 transition requires Standstill=True, DS402 Fault=False, and OperationEnabled=False. "\n                    + "Power Off / disable the servo before changing PP/PV/IP/CSP. Actual Standstill="\n                    + axisStatus.IsStandstill\n                    + ", StatusWord=0x"\n                    + statusWord.ToString("X4", CultureInfo.InvariantCulture)\n                    + ", currentMode="\n                    + driveStatus.OperationModeRaw.ToString(CultureInfo.InvariantCulture)\n                    + ", requestedMode="\n                    + ((sbyte)requestedMode).ToString(CultureInfo.InvariantCulture)\n                    + ".");\n            }\n\n            WriteLog(\n                "SetOperationMode cross-mode preflight passed: axis="\n                + currentAxis.AxisReference.ToString(CultureInfo.InvariantCulture)\n                + ", currentMode="\n                + driveStatus.OperationModeRaw.ToString(CultureInfo.InvariantCulture)\n                + ", requestedMode="\n                + ((sbyte)requestedMode).ToString(CultureInfo.InvariantCulture)\n                + ", StatusWord=0x"\n                + statusWord.ToString("X4", CultureInfo.InvariantCulture)\n                + ".");\n        }\n\n'''
replace_exact(
    WPF_OPMODE,
    '''        private async Task StartAxisSetOperationModeOnceAsync()\n''',
    opmode_helper + '''        private async Task StartAxisSetOperationModeOnceAsync()\n''',
    1,
    'SetOperationMode WPF preflight helper insertion')

replace_exact(
    WPF_OPMODE,
    '''            var currentAxis = await GetPhysicalAxisAsync(axisReference);\n            var prepared = currentAxis.PrepareSetOperationMode(\n''',
    '''            var currentAxis = await GetPhysicalAxisAsync(axisReference);\n            await VerifyAxisSetOperationModeTransitionPreflightAsync(\n                currentAxis,\n                requestedMode);\n            var prepared = currentAxis.PrepareSetOperationMode(\n''',
    1,
    'SetOperationMode WPF preflight call')

appendix = '''\n\n## 2026-08-28 실기 피드백 corrective tranche\n\n실기에서 `SetOperationMode`는 CSP에서만 성공처럼 보이고 generic `SDO Write`는 동작하지 않는 현상이 확인됐다. corrective source는 다음 두 원인을 분리한다.\n\n- SetOperationMode: CSP->CSP는 `0x6061` already-target 경로의 `SucceededNoWrite`가 될 수 있으므로 실제 `0x6060` cross-mode write 증거가 아니다. PC preflight가 fresh LASAL status + `0x6041` + `0x6061`을 읽고, 다른 mode로 바꿀 때만 `Standstill=True`, Fault clear, OperationEnabled clear를 확인한 뒤 one-shot Start를 허용한다. 안전조건은 완화하지 않는다.\n- Generic SDO Write: ordinary editor가 same-value qualification용 `PowerOn=False` 조건을 재사용해 safe non-semantic write까지 wire 전에 차단하던 경로를 분리한다. qualification은 기존 PowerOff 조건을 유지하고, ordinary generic Write는 `Standstill=True + Fault=False + OperationEnabled=False`를 요구한다. PLC도 semantic/dedicated-owner blocklist를 유지하면서 DS402 `Switch On Disabled(0x40)`, `Ready To Switch On(0x21)`, `Switched On(0x23)`만 허용한다. `Operation Enabled(0x27)` 및 기타 상태는 계속 fail-closed다.\n\n이 tranche는 hardware finding corrective source이며 실기 PASS 자체를 주장하지 않는다. PP/PV/IP 실제 전환과 arbitrary safe-object SDO Write는 재검증 후 qualification evidence를 갱신한다.\n'''
text = DESIGN.read_text(encoding='utf-8')
if '## 2026-08-28 실기 피드백 corrective tranche' not in text:
    DESIGN.write_text(text.rstrip() + appendix + '\n', encoding='utf-8')

print('Applied Operation Mode / Generic SDO hardware-finding corrective patch.')

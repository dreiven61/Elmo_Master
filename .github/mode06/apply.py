from pathlib import Path

SOURCE = Path('Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st')
FRAGMENT = Path('.github/mode06/AxisOperationModeTail.stfrag')

raw = SOURCE.read_bytes()
text = raw.decode('utf-8')
nl = '\r\n' if b'\r\n' in raw else '\n'


def norm(value):
    return value.replace('\r\n', '\n').replace('\n', nl)


if text.count('AxisOperationModeState : ARRAY [0..191] OF DINT;') != 1:
    raise SystemExit('AxisOperationModeState declaration is missing or duplicated')

implementation_marker = '//{{LSL_IMPLEMENTATION'
tail_marker = 'FUNCTION LMCDiagnosticsService::HandleAxisSetOperationModeStart'
if text.count(implementation_marker) != 1 or text.count(tail_marker) != 1:
    raise SystemExit('unexpected LMCDiagnosticsService implementation/tail shape')
if text.index(tail_marker) < text.index(implementation_marker):
    raise SystemExit('refusing to edit generated declaration region')

pump_old = norm('''\tProcessEncoderMaintenance();
\tProcessAxisDs402Home();
\tProcessAxisOwnershipStartup();''')
pump_new = norm('''\tProcessEncoderMaintenance();
\tProcessAxisDs402Home();
\tProcessAxisSetOperationMode();
\tProcessAxisOwnershipStartup();''')
if text.count(pump_old) != 1:
    raise SystemExit('ProcessOperations pump anchor changed')
text = text.replace(pump_old, pump_new, 1)

fragment = FRAGMENT.read_text(encoding='utf-8')

# Fail closed before any array dereference when runtime metadata is corrupt.
unsafe_guard = '''\tif (axisReference < 1) | (axisReference > 4) |
\t   (recordBase <> TO_DINT(axisReference - 1) * LMC_DIAG_MODE_RECORD_STRIDE) |
\t   (AxisOperationModeState[recordBase + 31]$UDINT <>
\t    LMC_DIAG_MODE_RECORD_MAGIC) |
\t   (AxisOperationModeState[recordBase] = LMC_DIAG_MODE_RECORD_EMPTY) then
\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_QUARANTINE_REASON] :=
\t\t\tLMC_DIAG_MODE_QUARANTINE_CALLBACK;
\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_STAGE] :=
\t\t\tLMC_DIAG_MODE_STAGE_QUARANTINE;
\t\tRETURN;
\tend_if;'''
safe_guard = '''\tif (axisReference < 1) | (axisReference > 4) |
\t   (recordBase < 0) | (recordBase > 96) |
\t   (recordBase <> TO_DINT(axisReference - 1) * LMC_DIAG_MODE_RECORD_STRIDE) then
\t\t// Runtime metadata is corrupt and no trustworthy owner identity can be
\t\t// reconstructed here. Keep the operation inert for startup recovery.
\t\tRETURN;
\tend_if;
\tif (AxisOperationModeState[recordBase + 31]$UDINT <>
\t    LMC_DIAG_MODE_RECORD_MAGIC) |
\t   (AxisOperationModeState[recordBase] = LMC_DIAG_MODE_RECORD_EMPTY) then
\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_QUARANTINE_REASON] :=
\t\t\tLMC_DIAG_MODE_QUARANTINE_CALLBACK;
\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_STAGE] :=
\t\t\tLMC_DIAG_MODE_STAGE_QUARANTINE;
\t\tRETURN;
\tend_if;'''
if fragment.count(unsafe_guard) != 1:
    raise SystemExit('runtime guard fragment changed')
fragment = fragment.replace(unsafe_guard, safe_guard, 1)

# CopyCompletion consumes RESULT_READY and makes the executor reusable. The
# same-cycle safety decision must observe that transition instead of the
# pre-CopyCompletion IsReusable sample.
preflight_completion = '''\t\t\tif completionResult = 0 then
\t\t\t\tif (completion.ValidationCode <> 0) | (completion.OsResult <> 0) |'''
preflight_completion_fixed = '''\t\t\tif completionResult = 0 then
\t\t\t\texecutorReusable := TRUE;
\t\t\t\tif (completion.ValidationCode <> 0) | (completion.OsResult <> 0) |'''
if fragment.count(preflight_completion) != 1:
    raise SystemExit('preflight completion fragment changed')
fragment = fragment.replace(preflight_completion, preflight_completion_fixed, 1)

# Match the existing LASAL source convention: use named arguments for the
# executor API instead of relying on positional parameter ordering.
for axis in range(1, 5):
    old = 'SdoAxis%d.TryStartRead(currentToken, 0x6061, 0, 1, remainingMs)' % axis
    new = '''SdoAxis%d.TryStartRead(
\t\t\t\t\tOperationToken:=currentToken,
\t\t\t\t\tObjectIndex:=0x6061,
\t\t\t\t\tSubIndex:=0,
\t\t\t\t\tReadLength:=1,
\t\t\t\t\tTimeoutMs:=remainingMs)''' % axis
    count = fragment.count(old)
    if count != 2:
        raise SystemExit('axis %d read call count changed: %d' % (axis, count))
    fragment = fragment.replace(old, new)

# 0x6061 is an Int8 object. Explicitly isolate the low byte before casting.
mode_cast_old = 'observedMode := completion.Data$SINT;'
mode_cast_new = 'observedMode := (completion.Data and 0x000000FF)$SINT;'
if fragment.count(mode_cast_old) != 2:
    raise SystemExit('observed-mode cast count changed')
fragment = fragment.replace(mode_cast_old, mode_cast_new)

fragment = norm(fragment)
idx = text.index(tail_marker)
text = text[:idx] + fragment

# Source-only safety assertions. Four axis-specific call sites are mutually
# exclusive; only the selected axis can dispatch one 0x6060 write in stage 3.
tail = text[text.index('#define LMC_DIAG_MODE_RECORD_STRIDE'):]
checks = {
    'mode processor pump': text.count('ProcessAxisSetOperationMode();') == 1,
    'generated declaration preserved': text.count('AxisOperationModeState : ARRAY [0..191] OF DINT;') == 1,
    'single write stage': tail.count('LMC_DIAG_MODE_STAGE_WRITE_START:') == 1,
    'four axis write call sites': tail.count('ObjectIndex:=0x6060') == 4,
    'no second write stage': tail.count('TryStartWrite(') == 4,
    'eight named 6061 read call sites': tail.count('ObjectIndex:=0x6061') == 8,
    '112 byte outcome': tail.count('ResponseSize := 112;') >= 2,
    'no positional 6061 executor calls': 'TryStartRead(currentToken, 0x6061' not in tail,
    'capability untouched by patch shape': text.index('#define LMC_DIAG_MODE_RECORD_STRIDE') > text.index(implementation_marker),
}
failed = [name for name, ok in checks.items() if not ok]
if failed:
    raise SystemExit('static MODE-06 assertion(s) failed: ' + ', '.join(failed))

SOURCE.write_bytes(text.encode('utf-8'))
print('MODE-06 exact patch applied')

from pathlib import Path
import sys

SOURCE = Path(
    "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/"
    "LMCDiagnosticsService/LMCDiagnosticsService.st"
)


def main() -> int:
    raw = SOURCE.read_bytes()
    text = raw.decode("utf-8")
    nl = "\r\n" if b"\r\n" in raw else "\n"

    def n(value: str) -> str:
        return value.replace("\r\n", "\n").replace("\n", nl)

    if "FUNCTION LMCDiagnosticsService::ProcessAxisSetOperationModeMutationStages" in text:
        print("ALREADY_SPLIT")
        return 0

    declaration_old = n("""\tFUNCTION ProcessAxisSetOperationMode;
  //Tables:""")
    declaration_new = n("""\tFUNCTION ProcessAxisSetOperationMode;
\tFUNCTION ProcessAxisSetOperationModeMutationStages;
\tFUNCTION ProcessAxisSetOperationModeRecoveryStages;
  //Tables:""")
    require_count(text, declaration_old, 1, "declaration anchor")
    text = text.replace(declaration_old, declaration_new, 1)

    function_marker = n("FUNCTION LMCDiagnosticsService::ProcessAxisSetOperationMode\n")
    function_start = text.find(function_marker)
    require(function_start >= 0, "ProcessAxisSetOperationMode implementation not found")

    function_end_marker = n("\nEND_FUNCTION")
    function_end = text.find(function_end_marker, function_start)
    require(function_end >= 0, "ProcessAxisSetOperationMode END_FUNCTION not found")
    function_end += len(function_end_marker)
    original_function = text[function_start:function_end]

    common_marker = n("""\tserviceNow := ops.tAbsolute;
\tstartMs := AxisOperationModeState[LMC_DIAG_MODE_RUNTIME_START_MS]$UDINT;""")
    common_start = original_function.find(common_marker)
    require(common_start >= 0, "common stage setup anchor not found")

    case_marker = n("""\tcase stage of
\t\tLMC_DIAG_MODE_STAGE_PREFLIGHT_START:""")
    case_start = original_function.find(case_marker, common_start)
    require(case_start >= 0, "stage case anchor not found")

    common_setup = original_function[common_start:case_start]
    case_header = n("\tcase stage of\n")
    case_body_start = case_start + len(case_header)
    case_end = original_function.rfind(n("\n\tend_case;"))
    require(case_end >= case_body_start, "outer stage case end not found")
    case_body = original_function[case_body_start:case_end]

    recovery_marker = n("\t\tLMC_DIAG_MODE_STAGE_RECOVERY_START:")
    recovery_start = case_body.find(recovery_marker)
    require(recovery_start >= 0, "recovery stage split anchor not found")
    mutation_cases = case_body[:recovery_start].rstrip()
    recovery_cases = case_body[recovery_start:].rstrip()

    helper_vars = n("""\tVAR
\t\tcompletion : LMCSdoExecutor::LMCSdoExecutorResult;
\t\tstartupSnapshot : ARRAY [0..11] OF UDINT;
\t\tstage, recordBase, finalRecordState : DINT;
\t\taxisReference : UINT;
\t\taxisMask, currentToken, evidenceFlags, serviceNow : UDINT;
\t\tstartMs, timeoutMs, elapsedMs, remainingMs : UDINT;
\t\tcurrentCycle, selectedAxisStatus, selectedStatusWord : UDINT;
\t\tcontextCheck, failureDetail, quarantineReason, nextToken : UDINT;
\t\tcopyResult, completionResult, orphanResult, ownerResult : DINT;
\t\tstartResult : iprStates;
\t\tobservedMode : SINT;
\t\texecutorConnected, executorReusable, safetyReady, expired : BOOL;
\tEND_VAR
""")

    helper_context = n("""
\tstage := AxisOperationModeState[LMC_DIAG_MODE_RUNTIME_STAGE];
\taxisReference := AxisOperationModeState[LMC_DIAG_MODE_RUNTIME_AXIS]$UINT;
\trecordBase := AxisOperationModeState[LMC_DIAG_MODE_RUNTIME_RECORD_BASE];
\tif (axisReference < 1) | (axisReference > 4) |
\t   (recordBase < 0) | (recordBase > 96) |
\t   (recordBase <> TO_DINT(axisReference - 1) * LMC_DIAG_MODE_RECORD_STRIDE) then
\t\tRETURN;
\tend_if;
\taxisMask := TO_UDINT(1) shl TO_UDINT(axisReference - 1);
\tcurrentToken := AxisOperationModeState[LMC_DIAG_MODE_RUNTIME_SDO_TOKEN]$UDINT;
\tevidenceFlags := AxisOperationModeState[recordBase + 18]$UDINT;
""")

    mutation_helper = (
        n("\n\nFUNCTION LMCDiagnosticsService::ProcessAxisSetOperationModeMutationStages\n")
        + helper_vars
        + helper_context
        + common_setup
        + n("\tcase stage of\n")
        + mutation_cases
        + n("""
\telse
\tend_case;

END_FUNCTION""")
    )

    recovery_helper = (
        n("\n\nFUNCTION LMCDiagnosticsService::ProcessAxisSetOperationModeRecoveryStages\n")
        + helper_vars
        + helper_context
        + common_setup
        + n("\tcase stage of\n")
        + recovery_cases
        + n("""
\tend_case;

END_FUNCTION""")
    )

    dispatch = n("""\tcase stage of
\t\tLMC_DIAG_MODE_STAGE_PREFLIGHT_START,
\t\tLMC_DIAG_MODE_STAGE_PREFLIGHT_WAIT,
\t\tLMC_DIAG_MODE_STAGE_WRITE_START,
\t\tLMC_DIAG_MODE_STAGE_WRITE_WAIT,
\t\tLMC_DIAG_MODE_STAGE_VERIFY_START,
\t\tLMC_DIAG_MODE_STAGE_VERIFY_WAIT:
\t\t\tProcessAxisSetOperationModeMutationStages();
\t\tLMC_DIAG_MODE_STAGE_RECOVERY_START,
\t\tLMC_DIAG_MODE_STAGE_RECOVERY_WAIT,
\t\tLMC_DIAG_MODE_STAGE_TERMINAL_SUCCESS,
\t\tLMC_DIAG_MODE_STAGE_TERMINAL_FAILURE,
\t\tLMC_DIAG_MODE_STAGE_QUARANTINE,
\t\tLMC_DIAG_MODE_STAGE_QUARANTINE_HOLD:
\t\t\tProcessAxisSetOperationModeRecoveryStages();
\telse
\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_QUARANTINE_REASON] :=
\t\t\tLMC_DIAG_MODE_QUARANTINE_CALLBACK;
\t\tAxisOperationModeState[LMC_DIAG_MODE_RUNTIME_STAGE] :=
\t\t\tLMC_DIAG_MODE_STAGE_QUARANTINE;
\tend_case;
""")

    new_main = original_function[:common_start] + dispatch + n("\nEND_FUNCTION")
    replacement = new_main + mutation_helper + recovery_helper
    text = text[:function_start] + replacement + text[function_end:]

    for name in (
        "FUNCTION LMCDiagnosticsService::ProcessAxisSetOperationMode\n",
        "FUNCTION LMCDiagnosticsService::ProcessAxisSetOperationModeMutationStages\n",
        "FUNCTION LMCDiagnosticsService::ProcessAxisSetOperationModeRecoveryStages\n",
    ):
        require_count(text, n(name), 1, name.strip())

    SOURCE.write_bytes(text.encode("utf-8"))
    print("SPLIT_APPLIED")
    return 0


def require(condition: bool, message: str) -> None:
    if not condition:
        raise RuntimeError(message)


def require_count(text: str, needle: str, expected: int, label: str) -> None:
    actual = text.count(needle)
    if actual != expected:
        raise RuntimeError(f"{label}: count={actual}, expected={expected}")


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"SPLIT_FAILED: {exc}", file=sys.stderr)
        raise

from pathlib import Path
import re
import sys

SOURCE = Path(
    "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/"
    "LMCDiagnosticsService/LMCDiagnosticsService.st"
)
METHOD_LIMIT_BYTES = 32768


def main() -> int:
    raw = SOURCE.read_bytes()
    text = raw.decode("utf-8")
    nl = "\r\n" if b"\r\n" in raw else "\n"

    def n(value: str) -> str:
        return value.replace("\r\n", "\n").replace("\n", nl)

    # The LASAL IDE owns the generated declaration section. The user has
    # already created the two private methods there, so this transformer must
    # never add or rewrite declarations.
    for declaration in (
        "\tFUNCTION ProcessAxisSetOperationMode;",
        "\tFUNCTION ProcessAxisSetOperationModeMutationStages;",
        "\tFUNCTION ProcessAxisSetOperationModeRecoveryStages;",
    ):
        require_count(text, n(declaration), 1, f"generated declaration {declaration.strip()}")

    main_name = "LMCDiagnosticsService::ProcessAxisSetOperationMode"
    mutation_name = "LMCDiagnosticsService::ProcessAxisSetOperationModeMutationStages"
    recovery_name = "LMCDiagnosticsService::ProcessAxisSetOperationModeRecoveryStages"

    main_start, main_end, original_main = get_function(text, main_name, n)
    mutation_start, mutation_end, original_mutation = get_function(text, mutation_name, n)
    recovery_start, recovery_end, original_recovery = get_function(text, recovery_name, n)

    if is_split_main(original_main) and not is_empty_function(original_mutation, mutation_name, n) and not is_empty_function(original_recovery, recovery_name, n):
        validate_split(text, n)
        print("ALREADY_SPLIT")
        return 0

    require(
        is_empty_function(original_mutation, mutation_name, n),
        "MutationStages exists but is not empty; refusing to overwrite user/LASAL code",
    )
    require(
        is_empty_function(original_recovery, recovery_name, n),
        "RecoveryStages exists but is not empty; refusing to overwrite user/LASAL code",
    )
    require(
        not is_split_main(original_main),
        "main processor already dispatches helpers while helper bodies are empty",
    )

    common_marker = n(
        """\tserviceNow := ops.tAbsolute;
\tstartMs := AxisOperationModeState[LMC_DIAG_MODE_RUNTIME_START_MS]$UDINT;"""
    )
    common_start = original_main.find(common_marker)
    require(common_start >= 0, "common stage setup anchor not found")

    case_marker = n(
        """\tcase stage of
\t\tLMC_DIAG_MODE_STAGE_PREFLIGHT_START:"""
    )
    case_start = original_main.find(case_marker, common_start)
    require(case_start >= 0, "stage case anchor not found")

    common_setup = original_main[common_start:case_start]
    case_header = n("\tcase stage of\n")
    case_body_start = case_start + len(case_header)
    case_end = original_main.rfind(n("\n\tend_case;"))
    require(case_end >= case_body_start, "outer stage case end not found")
    case_body = original_main[case_body_start:case_end]

    recovery_marker = n("\t\tLMC_DIAG_MODE_STAGE_RECOVERY_START:")
    recovery_case_start = case_body.find(recovery_marker)
    require(recovery_case_start >= 0, "recovery stage split anchor not found")
    mutation_cases = case_body[:recovery_case_start].rstrip()
    recovery_cases = case_body[recovery_case_start:].rstrip()

    # These are exactly the locals referenced by the stage case bodies and the
    # shared stage setup copied out of the oversized processor. MODE-08
    # preemption locals intentionally remain in the main processor.
    helper_vars = n(
        """\tVAR
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
"""
    )

    helper_context = n(
        """
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
"""
    )

    mutation_helper = (
        n(f"FUNCTION {mutation_name}\n")
        + helper_vars
        + helper_context
        + common_setup
        + n("\tcase stage of\n")
        + mutation_cases
        + n(
            """
\telse
\tend_case;

END_FUNCTION"""
        )
    )

    recovery_helper = (
        n(f"FUNCTION {recovery_name}\n")
        + helper_vars
        + helper_context
        + common_setup
        + n("\tcase stage of\n")
        + recovery_cases
        + n(
            """
\tend_case;

END_FUNCTION"""
        )
    )

    dispatch = n(
        """\tcase stage of
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
"""
    )

    # Keep all warm-start, identity, MODE-08 safety-preemption, activation-OFF,
    # timeout and MODE-07 no-replay normalization code in the main processor.
    # Only the normal stage machine beginning at the second serviceNow/startMs
    # setup is delegated.
    new_main = original_main[:common_start] + dispatch + n("\nEND_FUNCTION")

    replacements = [
        (main_start, main_end, new_main),
        (mutation_start, mutation_end, mutation_helper),
        (recovery_start, recovery_end, recovery_helper),
    ]
    for start, end, replacement in sorted(replacements, reverse=True):
        text = text[:start] + replacement + text[end:]

    validate_split(text, n)
    SOURCE.write_bytes(text.encode("utf-8"))
    print("SPLIT_APPLIED")
    return 0


def get_function(text: str, qualified_name: str, normalize):
    marker = normalize(f"FUNCTION {qualified_name}\n")
    require_count(text, marker, 1, f"implementation {qualified_name}")
    start = text.find(marker)
    end_marker = normalize("\nEND_FUNCTION")
    end = text.find(end_marker, start)
    require(end >= 0, f"END_FUNCTION not found for {qualified_name}")
    end += len(end_marker)
    return start, end, text[start:end]


def is_empty_function(function_text: str, qualified_name: str, normalize) -> bool:
    header = normalize(f"FUNCTION {qualified_name}\n")
    end_marker = normalize("\nEND_FUNCTION")
    require(function_text.startswith(header), f"unexpected header for {qualified_name}")
    require(function_text.endswith(end_marker), f"unexpected end for {qualified_name}")
    return function_text[len(header):-len(end_marker)].strip() == ""


def is_split_main(function_text: str) -> bool:
    return (
        "ProcessAxisSetOperationModeMutationStages();" in function_text
        and "ProcessAxisSetOperationModeRecoveryStages();" in function_text
    )


def validate_split(text: str, normalize) -> None:
    main_name = "LMCDiagnosticsService::ProcessAxisSetOperationMode"
    mutation_name = "LMCDiagnosticsService::ProcessAxisSetOperationModeMutationStages"
    recovery_name = "LMCDiagnosticsService::ProcessAxisSetOperationModeRecoveryStages"

    _, _, main_body = get_function(text, main_name, normalize)
    _, _, mutation_body = get_function(text, mutation_name, normalize)
    _, _, recovery_body = get_function(text, recovery_name, normalize)

    require(is_split_main(main_body), "main processor does not dispatch both helpers")
    require("CopyAxisOwnershipPreemption" in main_body, "MODE-08 preemption read left main processor")
    require("PublishAxisOwnershipPreemptionCleanup" in main_body, "MODE-08 cleanup publication left main processor")
    require("LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED" in main_body, "MODE-07 write-dispatch no-replay guard left main processor")
    require("LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE" in main_body, "activation-OFF gate left main processor")

    for stage in (
        "LMC_DIAG_MODE_STAGE_PREFLIGHT_START",
        "LMC_DIAG_MODE_STAGE_PREFLIGHT_WAIT",
        "LMC_DIAG_MODE_STAGE_WRITE_START",
        "LMC_DIAG_MODE_STAGE_WRITE_WAIT",
        "LMC_DIAG_MODE_STAGE_VERIFY_START",
        "LMC_DIAG_MODE_STAGE_VERIFY_WAIT",
    ):
        require(stage in mutation_body, f"mutation helper missing {stage}")

    for stage in (
        "LMC_DIAG_MODE_STAGE_RECOVERY_START",
        "LMC_DIAG_MODE_STAGE_RECOVERY_WAIT",
        "LMC_DIAG_MODE_STAGE_TERMINAL_SUCCESS",
        "LMC_DIAG_MODE_STAGE_TERMINAL_FAILURE",
        "LMC_DIAG_MODE_STAGE_QUARANTINE",
        "LMC_DIAG_MODE_STAGE_QUARANTINE_HOLD",
    ):
        require(stage in recovery_body, f"recovery helper missing {stage}")

    write_pattern = re.compile(r"TryStartWrite\([^;\r\n]*ObjectIndex:=0x6060")
    mutation_writes = len(write_pattern.findall(mutation_body))
    recovery_writes = len(write_pattern.findall(recovery_body))
    main_writes = len(write_pattern.findall(main_body))
    require(mutation_writes == 4, f"mutation helper 0x6060 write count={mutation_writes}, expected=4")
    require(recovery_writes == 0, f"recovery helper 0x6060 write count={recovery_writes}, expected=0")
    require(main_writes == 0, f"main processor 0x6060 write count={main_writes}, expected=0")

    require(
        "LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED" in mutation_body,
        "mutation helper does not persist irreversible write-dispatch evidence",
    )
    require(
        "never fall back to WRITE_START" in recovery_body,
        "read-only recovery invariant comment/anchor missing",
    )
    require(
        normalize("\n\telse\n\tend_case;") not in recovery_body,
        "recovery helper contains a duplicate empty ELSE arm",
    )

    for qualified_name, body in (
        (main_name, main_body),
        (mutation_name, mutation_body),
        (recovery_name, recovery_body),
    ):
        method_bytes = len(body.encode("utf-8"))
        require(
            method_bytes < METHOD_LIMIT_BYTES,
            f"{qualified_name} method budget {method_bytes} >= {METHOD_LIMIT_BYTES}",
        )
        print(f"METHOD_BYTES {qualified_name}={method_bytes}")

    require_count(
        text,
        normalize("#define LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE"),
        1,
        "compile-time activation gate",
    )


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

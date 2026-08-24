# SetOperationMode MODE-10 method split 수정 설계

- 대상: `LMCDiagnosticsService::ProcessAxisSetOperationMode`
- 기준 branch/commit: `dev@ba775c652ffb810c9fa65262b5a733117f369d7d`
- 기준일: 2026-08-24
- 목적: LASAL custom method 32 KiB 제한을 만족하도록 SetOperationMode cyclic processor를 **동작 변경 없이** 3개 method로 분할한다.
- 성격: MODE-10 static/source qualification을 위한 refactor 전용 문서다. 기존 `SET_OPERATION_MODE_DESIGN.md`의 wire, owner, no-replay, recovery, capability 계약을 변경하지 않는다.

## 1. 현재 상태

사용자가 LASAL IDE에서 다음 private method 2개를 이미 생성했다.

```text
ProcessAxisSetOperationModeMutationStages
ProcessAxisSetOperationModeRecoveryStages
```

현재 `LMCDiagnosticsService.st`에는 declaration과 빈 implementation이 각각 정확히 1개 존재한다.
따라서 Codex는 **generated declaration 영역을 추가/삭제/재생성하지 않는다.**
수정 대상은 기존 3개 implementation body다.

```text
ProcessAxisSetOperationMode
ProcessAxisSetOperationModeMutationStages
ProcessAxisSetOperationModeRecoveryStages
```

이 작업의 목적은 현재 `ProcessAxisSetOperationMode`에 이미 존재하는 MODE-06/07/08 runtime logic을
재설계하는 것이 아니라 method-size 문제를 해소하도록 stage case를 분리하는 것이다.

## 2. 절대 변경 금지 범위

이번 작업에서는 다음을 변경하지 않는다.

- `0x7D23 / 0x7D24 / 0x7D25` wire layout, payload size, field offset
- SetOperationMode request/ack/outcome/retire C# contract
- `AxisOperationMode` owner kind/resource numeric ABI
- timeout, RequestId, ClientIntentId, build/BootId/map identity 규칙
- `0x6060` exact one-byte write 의미
- `0x6061` preflight/verify 의미
- write-dispatch 이후 no-replay 규칙
- recovery의 read-only 규칙
- terminal/quarantine/outcome 의미
- Home/SetPosition/motion/generic SDO preemption 의미
- generic D5 `0x6060` permanent deny 정책
- compile-time activation gate
- capability bits 8/9/10
- WPF recovery/journal

특히 다음 gate는 계속 OFF여야 한다.

```text
#define LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE
```

MODE-10을 통과시키기 위해 capability를 켜거나 artifact ratchet을 임의 갱신해서는 안 된다.

## 3. 분할 구조

최종 구조는 아래로 고정한다.

```text
ProcessAxisSetOperationMode
  ├─ warm-start / identity validation
  ├─ MODE-08 ownership safety-preemption 처리
  ├─ activation-OFF 처리
  ├─ timeout / irreversible-write evidence 기반 MODE-07 no-replay normalization
  └─ current stage에 따라 helper 1회 dispatch

ProcessAxisSetOperationModeMutationStages
  ├─ PREFLIGHT_START
  ├─ PREFLIGHT_WAIT
  ├─ WRITE_START
  ├─ WRITE_WAIT
  ├─ VERIFY_START
  └─ VERIFY_WAIT

ProcessAxisSetOperationModeRecoveryStages
  ├─ RECOVERY_START
  ├─ RECOVERY_WAIT
  ├─ TERMINAL_SUCCESS
  ├─ TERMINAL_FAILURE
  ├─ QUARANTINE
  └─ QUARANTINE_HOLD
```

### 3.1 main processor에 반드시 남길 것

`ProcessAxisSetOperationMode`에는 다음 orchestration이 남아 있어야 한다.

- warm-start / runtime identity validation
- `CopyAxisOwnershipPreemption`
- `PublishAxisOwnershipPreemptionCleanup`
- `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE` 경로
- `LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED`를 이용한 irreversible-dispatch 판단
- uncertainty를 `LMC_DIAG_MODE_STAGE_RECOVERY_START`로 정규화하는 no-replay 처리
- helper dispatch
- 잘못된/알 수 없는 stage를 quarantine으로 보내는 fail-closed default

main processor에는 분할 완료 후 `0x6060` write site가 0개여야 한다.

### 3.2 mutation helper로 이동할 stage

다음 stage case는 `ProcessAxisSetOperationModeMutationStages`로 이동한다.

```text
LMC_DIAG_MODE_STAGE_PREFLIGHT_START
LMC_DIAG_MODE_STAGE_PREFLIGHT_WAIT
LMC_DIAG_MODE_STAGE_WRITE_START
LMC_DIAG_MODE_STAGE_WRITE_WAIT
LMC_DIAG_MODE_STAGE_VERIFY_START
LMC_DIAG_MODE_STAGE_VERIFY_WAIT
```

규칙:

- 기존 stage body를 의미 변경 없이 이동한다.
- `0x6060` write는 이 helper에만 존재한다.
- physical axis 1..4 fan-out 때문에 source 상 `TryStartWrite(... ObjectIndex:=0x6060 ...)`는 정확히 4개다.
- 이는 4번 재시도하라는 뜻이 아니라 기존 one-logical-write/four-axis branch 구조를 그대로 보존하라는 뜻이다.
- `LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED` 기록을 반드시 유지한다.
- verify는 기존대로 `0x6061` exact-match를 사용한다.

### 3.3 recovery helper로 이동할 stage

다음 stage case는 `ProcessAxisSetOperationModeRecoveryStages`로 이동한다.

```text
LMC_DIAG_MODE_STAGE_RECOVERY_START
LMC_DIAG_MODE_STAGE_RECOVERY_WAIT
LMC_DIAG_MODE_STAGE_TERMINAL_SUCCESS
LMC_DIAG_MODE_STAGE_TERMINAL_FAILURE
LMC_DIAG_MODE_STAGE_QUARANTINE
LMC_DIAG_MODE_STAGE_QUARANTINE_HOLD
```

규칙:

- recovery helper의 `0x6060` write site는 반드시 0개다.
- write-dispatch 이후 recovery는 `0x6061` read/verify와 executor drain/evidence 처리만 수행한다.
- recovery에서 `WRITE_START`로 되돌아가는 fallback을 만들지 않는다.
- terminal/quarantine commit, owner cleanup, outcome evidence 순서를 기존 코드 그대로 보존한다.

## 4. helper 공통 context

helper는 stage body가 현재 참조하는 runtime context를 스스로 다시 읽도록 한다.
main의 local variable을 helper로 전달하기 위해 ABI를 새로 만들지 않는다.

최소 공통 context는 현재 runtime state 기준으로 다음을 다시 구성한다.

```text
stage
axisReference
recordBase
axisMask
currentToken
evidenceFlags
serviceNow
startMs / timeoutMs / elapsedMs / remainingMs
currentCycle
selectedAxisStatus / selectedStatusWord
contextCheck / failureDetail / quarantineReason / nextToken
completion / startupSnapshot
executorConnected / executorReusable / safetyReady / expired
```

`axisReference`는 1..4, `recordBase`는 해당 축 record stride와 일치하는지 helper 시작부에서 fail-closed 확인한다.
기존 state array, record offset, token/evidence 필드의 의미와 타입을 변경하지 않는다.

## 5. main dispatch 규칙

main processor의 normal stage dispatch는 다음 의미를 가져야 한다.

```text
PREFLIGHT_START/WAIT,
WRITE_START/WAIT,
VERIFY_START/WAIT
    -> ProcessAxisSetOperationModeMutationStages()

RECOVERY_START/WAIT,
TERMINAL_SUCCESS/FAILURE,
QUARANTINE/QUARANTINE_HOLD
    -> ProcessAxisSetOperationModeRecoveryStages()

else
    -> callback/invalid-stage quarantine
```

각 cyclic 호출에서 현재 stage에 대응하는 helper는 최대 1개만 호출한다.
helper 내부에서 다른 helper를 호출하지 않는다.

## 6. method-size acceptance

다음 3개 method는 각각 UTF-8 기준 **32,768 bytes 미만**이어야 한다.

```text
LMCDiagnosticsService::ProcessAxisSetOperationMode
LMCDiagnosticsService::ProcessAxisSetOperationModeMutationStages
LMCDiagnosticsService::ProcessAxisSetOperationModeRecoveryStages
```

`32,768`과 같아도 실패다. 각 method는 `< 32768`이어야 한다.

분할을 위해 의미 없는 주석 삭제, 안전 검사 제거, state merge, write/recovery 로직 축약을 하지 않는다.
필요하면 공통 setup을 helper 각각에 중복시키는 편을 택한다. 이번 목표는 semantic deduplication이 아니라
LASAL method budget 만족과 기존 동작 보존이다.

## 7. transformer 사용 시 주의

현재 `dev`에는 사용자가 두 helper declaration/빈 implementation을 이미 추가했다.
따라서 과거 transformer처럼 function 이름 존재만 보고 `ALREADY_SPLIT`으로 종료하면 안 된다.

transformer를 사용할 경우 다음 조건을 만족해야 한다.

1. generated declaration 3개가 각각 정확히 1개인지 확인만 하고 declaration은 수정하지 않는다.
2. helper body가 비어 있는 경우에만 자동 채운다.
3. helper body가 이미 비어 있지 않으면 사용자/LASAL 코드를 덮어쓰지 않고 즉시 중단한다.
4. main이 이미 두 helper를 dispatch하고 helper도 비어 있지 않을 때만 `ALREADY_SPLIT`으로 판정한다.
5. 변환 후 아래 static invariant를 자체 검사한다.

참고 구현은 branch `codex/mode-10-method-split`의
`tools/Apply-SetOperationModeMethodSplit.py`와 `tools/Verify-SetOperationModeStatic.ps1`에 있다.
Codex는 해당 branch를 wholesale merge하지 말고, **latest `dev`를 기준으로 필요한 로직만 참고**한다.

## 8. 필수 static invariant

분할 후 최소한 아래 조건이 모두 참이어야 한다.

| 항목 | 기대값 |
|---|---:|
| main -> mutation helper call | 1 |
| main -> recovery helper call | 1 |
| main `0x6060` write site | 0 |
| mutation helper `0x6060` write site | 4 |
| recovery helper `0x6060` write site | 0 |
| generated declaration 3개 | 각각 1 |
| compile-time activation gate | FALSE |
| main에 `CopyAxisOwnershipPreemption` | 존재 |
| main에 `PublishAxisOwnershipPreemptionCleanup` | 존재 |
| main에 irreversible write-dispatch no-replay guard | 존재 |
| recovery의 read-only/no-WRITE_START invariant | 존재 |
| 3개 processor method size | 각각 < 32768 bytes |

`0x6060` write count 4는 physical axis 1..4 source fan-out의 syntactic count다.
실행 의미는 기존 exactly-once/no-replay 계약 그대로다.

## 9. Codex 실행 순서

Codex는 다음 순서로 작업한다.

1. 반드시 latest `dev`에서 시작하고 HEAD가 최소 `ba775c652ffb810c9fa65262b5a733117f369d7d`를 포함하는지 확인한다.
2. `LMCDiagnosticsService.st`의 세 processor implementation을 읽고 사용자 생성 helper가 빈 body인지 재확인한다.
3. generated declaration 영역은 건드리지 않는다.
4. main의 preemption/activation/no-replay orchestration을 유지한 채 normal stage case만 두 helper로 이동한다.
5. method-size와 `0x6060` write-site invariant를 확인한다.
6. static verifier를 현재 3-method 구조에 맞춘다. 이미 맞는 verifier가 있으면 불필요하게 재작성하지 않는다.
7. `git diff --check`와 LASAL ASCII gate를 실행한다.
8. MODE-10 static verifier를 실행한다.
9. SourceOnly contract를 실행한다.
10. 결과를 PASS / FAIL / STOP으로 구분해 기록한다. artifact identity ratchet 문제를 source failure로 위장하지 않는다.
11. C78/PLC build나 hardware proof를 실제로 수행하지 않았다면 수행한 것으로 기록하지 않는다.
12. 이번 commit에서 capability를 활성화하지 않는다.

## 10. 권장 검증 명령

repository root 기준:

```powershell
git diff --check

powershell.exe -ExecutionPolicy Bypass `
  -File .\tools\Verify-CodexLasalAscii.ps1 -Staged

powershell.exe -ExecutionPolicy Bypass `
  -File .\tools\Verify-SetOperationModeStatic.ps1 `
  -ExpectedSdoWriteAxis 1

powershell.exe -ExecutionPolicy Bypass `
  -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 `
  -RepositoryRoot (Get-Location).Path `
  -SourceOnly `
  -ExpectedSdoWriteAxis 1
```

실제 repository의 verifier 인자나 경로가 변경돼 있으면 현재 script help/source를 우선한다.
검증을 통과시키기 위해 contract를 약화시키지 않는다.

## 11. 현재 알려진 qualification 주의사항

이전 checkpoint에서 C78/ARM Rebuild/Link와 PLC load는 성공했지만 `Classes.lcb` artifact identity가
기존 ratchet과 달라 UDP VerifyCurrent/full SourceOnly가 STOP 상태였다.

따라서 이번 refactor 후에도 같은 ratchet mismatch만 남는다면:

- method split/static/source semantics 자체 PASS 여부와
- artifact identity ratchet STOP

을 분리해서 보고한다.

ratchet을 자동 갱신하거나 capability를 켜서 MODE-10을 억지로 완료 처리하지 않는다.
새 artifact를 정식 ratchet으로 승인하는 것은 별도 결정이다.

## 12. 완료 조건

이 수정은 아래가 모두 확인됐을 때만 method-split 완료로 간주한다.

- 사용자 생성 두 helper가 실제 stage body를 가진다.
- main processor가 두 helper를 정확히 dispatch한다.
- main/mutation/recovery 모두 32 KiB 미만이다.
- `0x6060` write site가 mutation helper에만 존재한다.
- no-replay/read-only recovery invariant가 유지된다.
- generated declaration을 수동 재생성/중복 추가하지 않았다.
- activation/capability는 OFF다.
- static verifier가 PASS한다.
- SourceOnly 결과가 기록돼 있다.

단, C78/PLC/hardware proof와 artifact ratchet 승인이 없으면 `MODE-10` 전체 체크박스를 완료로 올리지 않는다.
이번 문서는 **Codex가 source refactor를 수행하기 위한 handoff**이며, production activation 승인 문서가 아니다.

## 13. 구현 결과 (2026-08-24)

### 13.1 적용 결과

- 작업 branch는 `dev`, 작업 시작 HEAD는 `ef77cc208a76e1ed3e27387cdcf68e5274044a91`이다.
- HEAD가 기준 commit `ba775c652ffb810c9fa65262b5a733117f369d7d`를 포함함을 확인했다.
- IDE가 생성한 세 private method declaration은 수정하지 않았다.
- main processor에는 warm-start, identity, MODE-08 preemption, activation-OFF, timeout/no-replay 정규화를 유지했다.
- mutation helper에는 PREFLIGHT/WRITE/VERIFY stage만 이동했다.
- recovery helper에는 RECOVERY/TERMINAL/QUARANTINE stage와 기존 fail-closed `else`를 이동했다.
- 변환기가 recovery helper에 빈 `else`를 중복 생성하지 않도록 자체 검증을 추가했다.
- `LMC_DIAG_SET_OPERATION_MODE_ENABLED`는 `FALSE` 그대로다.

### 13.2 source/static 결과

| 검증 항목 | 결과 |
|---|---|
| generated declaration 변경 | PASS, 변경 없음 |
| main -> mutation/recovery dispatch | PASS, 각각 1개 |
| `0x6060` write site | PASS, main 0 / mutation 4 / recovery 0 |
| main LF/CRLF method size | PASS, 19,895 / 20,309 bytes |
| mutation LF/CRLF method size | PASS, 19,731 / 20,150 bytes |
| recovery LF/CRLF method size | PASS, 14,251 / 14,559 bytes |
| MODE-10 static verifier | PASS, 57 checks |
| focused diagnostics method-split source contract | PASS |
| LASAL source 7-bit ASCII scan | PASS, 비ASCII 문자 0개 |
| `git diff --check` | PASS |

설계서에 기록된 `tools/Verify-CodexLasalAscii.ps1`은 현재 repository에 존재하지 않는다.
따라서 이번 검증은 변경한 LASAL source 전체를 대상으로 동일한 7-bit ASCII 조건을 직접 검사했다.

### 13.3 full SourceOnly 결과

full SourceOnly는 이번 method split과 무관한 기존 ownership preemption self-test baseline에서 STOP됐다.

```text
replacement helper persistent read inventory is 44/12, expected 41/12.
```

현재 `dev`와 기준 commit의 `LMCControlCommandService::ValidateAxisOwnershipPreemptionReplacement`에는
SetOperationMode owner 상태 검증용 `OwnershipState` read 3개가 이미 포함돼 있지만 self-test 기대값은
과거 41에 머물러 있다. 이번 변경 파일에는 `LMCControlCommandService.st`가 포함되지 않으며,
해당 전역 ratchet을 부분 갱신하지 않았다.

따라서 현재 판정은 다음과 같다.

- MODE-10 method split source/static semantics: PASS
- focused diagnostics source contract: PASS
- full SourceOnly: STOP, pre-existing ownership self-test baseline drift
- C78/IDE Rebuild/Link: 미수행
- PLC download/runtime/hardware: 미수행
- production activation/capability: OFF

C78/PLC/hardware proof와 full SourceOnly blocker 해소 전까지 상위 `SET_OPERATION_MODE_DESIGN.md`의
`MODE-10` 체크박스는 완료로 올리지 않는다.

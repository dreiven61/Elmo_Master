# SetOperationMode 구현 설계 보완 — 2026-08-28

> 상태: current implementation addendum
>
> 기준 source: `dev@45fc528c48723cbe3ba20fb11c3a2d7ec0e7ef0b`
>
> 현재 qualification branch: `codex/setopmode-mode11-bench-activation@eae31dd0365c4ae39f4d56874b8a1b82ab477146` — **stale / 다음 실기 전 재생성 필요**
>
> 추적: issue #46, qualification PR #18, MODE-11E PR #52, MODE-11F PR #53
>
> 이 문서는 기존 `SET_OPERATION_MODE_DESIGN.md`의 historical evidence를 삭제하지 않고 current implementation과 release boundary를 보완한다. 충돌하는 경우 이 문서의 current-state 판정을 우선한다.

## 1. 현재 판정

SetOperationMode의 **multi-mode software path와 no-replay recovery, operator diagnostics는 구현 단계가 상당 부분 닫혔다.** 다만 PP/PV/IP의 실제 mode-change packet/hardware qualification과 current-image C78/PLC evidence가 아직 남아 있으므로 release 완료로 보지 않는다.

- source/PC 구현 진행도: **약 80%**
- release-oriented qualification 진행도: **약 65%**
- production activation: **OFF 유지**
- `dev` runtime gate: `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE`
- `dev` Admin bits 8/9/10: OFF
- `dev` advertised `SetOperationModeSupportedMask`: `0x0000`
- production 의미: 신규 SetOperationMode mutation은 dormant

진행도는 checklist 단순 비율이 아니라 구현/검증 grade를 분리한 추정치다. PP/PV/IP physical PASS 전에는 `multi-mode complete`로 기록하지 않는다.

### 1.1 기능별 판정

| 기능 | 현재 판정 | 근거 / 남은 것 |
|---|---|---|
| immutable Start/Outcome/Retire contract | 완료 | `0x7D23/0x7D24/0x7D25`, durable identity, exact-generation retire |
| PP(1)/PV(3)/IP(7)/CSP(8) request parsing | 완료 | SDK/LASAL requested-mode path |
| same-mode no-write | source 완료 / physical evidence 대기 | `0x6061 == requestedMode`이면 `WriteRequested=0`, `WriteDispatched=0` |
| cross-mode `0x6060` mutation | source 완료 / physical 미완료 | requested mode exact 1-byte write 후 `0x6061` exact verify |
| non-CSP preflight | source 완료 | PhysicalValid + Standstill + Fault clear + OperationEnabled clear + conflict clear |
| write-dispatch 이후 no-replay | 완료 | recovery는 `0x6061` read-only, original Start/`0x6060` replay 금지 |
| PP/PV/IP/CSP warm-start recovery | **완료 — MODE-11E** | CSP-only candidate 제거, multi-mode candidate + identity fence + multi-candidate fail-closed |
| PLC `SupportedModeMask` wire/SDK/WPF 연동 | source 완료 | candidate mask PP/PV/IP/CSP = `0x018A`; production은 physical PASS mode만 광고해야 함 |
| WPF selector | 완료 | PP/PV/IP/CSP 선택 가능, 실제 Start는 live PLC triad/mask로 fail-closed |
| WPF rejection/preflight diagnostics | **완료 — MODE-11F** | requested/preflight/observed, DetailCode, DS402/evidence/write state 표시 |
| Axis1 CSP/PP/PV/IP actual mode change | **미완료** | exact current C78/PLC image에서 packet/readback evidence 필요 |
| timeout/disconnect/mismatch/quarantine matrix | 미완료 | Axis1부터 evidence 필요 |
| Axis2..4 matrix | 미완료 | Axis1 closure 이후 확대 |
| production activation | 미완료 | physical qualification + release mask + paired activation review 필요 |

## 2. 현재 정상 mutation 경로

정상 mutation path는 CSP=8로 하드코딩하지 않는다.

1. `0x7D23 Start`에서 `requestedMode`를 durable key/record에 저장한다.
2. `0x6061:0`을 Int8/1 byte로 preflight read한다.
3. `observedMode == requestedMode`이면 write 없이 terminal success한다.
4. mode가 다르면 cross-mode safety preflight를 수행한다.
5. 통과하면 `requestedMode`를 `0x6060:0`에 **exact 1 byte, 1회** write한다.
6. write completion 후 `0x6061:0`을 다시 read한다.
7. `observedMode == requestedMode`일 때만 success를 publish한다.
8. write dispatch 이후 outcome이 불확실하면 새 `0x6060` write 없이 read-only recovery로 이동한다.

따라서 PP/PV/IP를 선택했을 때 내부에서 CSP=8을 쓰는 정상 mutation 경로는 current source에 없다.

## 3. CSP만 동작하는 것처럼 보일 수 있는 이유

현재 mode가 CSP(8)인 상태에서 CSP를 다시 선택하면 same-mode가 즉시 성립하므로 실제 `0x6060` write 없이 성공할 수 있다.

반면 CSP에서 PP/PV/IP로 전환하려면 다음 조건이 모두 필요하다.

- physical context valid
- target axis Standstill
- DS402 Fault bit clear
- **DS402 OperationEnabled bit clear**
- HomeDS402/HomeDS402Ex inactive
- Encoder maintenance inactive
- same-axis generic SDO conflict 없음
- `LMCSdoExecutor` connected/reusable
- common ownership/admission tuple valid

하나라도 실패하면 `SetOperationModeUnsafeState(44)` 또는 실행 계열 detail로 safe failure하고 `0x6060` write를 수행하지 않는다.

따라서 실기 판정은 반드시 `requested/preflight/observed mode`, DetailCode, `WriteRequested`, `WriteDispatched`를 함께 본다.

## 4. MODE-11E — multi-mode warm-start recovery 완료

PR #52에서 기존 CSP-only warm-start reconstruction을 PP/PV/IP/CSP로 일반화했고 `dev`에 merge했다.

### 4.1 구현 내용

- write-dispatched Running record의 requested mode를 PP(1)/PV(3)/IP(7)/CSP(8) 범위로 일반화
- non-CSP candidate는 loaded-image software-mode gate를 통과해야 함
- record generation과 admission/owner/session/sequence identity를 nonzero/exact fence로 강화
- exact magic / axis / reference / record identity를 검증
- candidate가 둘 이상이면 fail-closed
- 다중 candidate 발견 시 이미 staging한 첫 runtime 영역도 즉시 clear
- recovery helper는 `0x6060` write를 생성하지 않음
- recovery는 exact `0x6061` read-only 확인으로만 terminal 판단

### 4.2 검증 결과

- SetOperationMode static qualification: PASS
- SetOperationMode C78 evidence tool: PASS
- HomeDS402 H37 source regression: PASS
- HomeDS402Ex LASAL/static/ownership/retained-store regressions: PASS
- MODE-11E merge commit: `dev@10e7ba11e99770d1d62988007df6ed444604b33f`

이 PASS는 source/static/PC regression 증거다. fresh current-image C78/PLC/hardware PASS를 대신하지 않는다.

## 5. MODE-11F — WPF rejection/preflight diagnostics 완료

PR #53에서 실패 원인과 실제 wire mutation 여부를 operator가 구분할 수 있도록 WPF diagnostics를 확장했고 `dev`에 merge했다.

### 5.1 표시 항목

Start definitive rejection 또는 terminal outcome에서 다음을 표시한다.

- Requested mode
- Preflight/Previous mode
- Observed mode
- Axis / RequestId
- CommandStatus
- ErrorId
- symbolic + numeric DetailCode
- DiagnosticsBuild / BootId / MapRevision
- DS402 StatusWord
- Fault
- OperationEnabled
- PhysicalValid
- EvidenceFlags
- WriteRequested
- WriteDispatched
- VerifyReadDispatched
- VerifyReadCompleted
- OwnerReleased
- ExecutorReusable
- ContextCheck
- QuarantineReason
- RecordGeneration

현재 outcome wire에는 원본 Standstill field가 없으므로 **`Standstill=not-exported`**로 표시한다. 다른 bit로 추정해서 PASS/FAIL을 만들지 않는다.

### 5.2 검증 결과

PR #53 fixed head `875dbe588c08509193660eeb8675dc8b734b312b` 기준:

- SetOperationMode WPF recovery Debug/Release: PASS
- HomeDS402Ex WPF recovery Debug/Release: PASS
- HomeDS402 H37 source qualification: PASS
- Korean/English localization round-trip: PASS
- MODE-11F merge commit: `dev@45fc528c48723cbe3ba20fb11c3a2d7ec0e7ef0b`

## 6. SupportedModeMask 계약

AdminCapabilities의 `SetOperationModeSupportedMask`에서 bit N은 DS402 mode N을 의미한다.

- PP(1): `0x0002`
- PV(3): `0x0008`
- IP(7): `0x0080`
- CSP(8): `0x0100`
- PP/PV/IP/CSP qualification candidate mask: `0x018A`

### 6.1 image별 의미

| image | Admin triad | mask | 의미 |
|---|---:|---:|---|
| production `dev` dormant | OFF | `0x0000` | 신규 SetOperationMode mutation 금지 |
| qualification candidate | ON | 시험 대상 mask | bench test enable; physical PASS 자체를 뜻하지 않음 |
| future production | ON | physical PASS mode만 | mode별 evidence와 paired activation 후 허용 |

`0x018A`를 광고했다는 사실만으로 PP/PV/IP/CSP가 production-qualified라고 판정하지 않는다.

## 7. WPF/SDK admission 계약

### selector

- PP/PV/IP/CSP 4개 software-known target은 항상 표시한다.
- PLC mask가 0이어도 operator가 선택/로그 확인은 가능하다.
- Homing(6)은 표시하지 않는다.

### Start

Start는 다음을 모두 만족해야만 enabled/dispatch 가능하다.

- connected + idle
- no unresolved SetOperationMode recovery
- durable journal arm 가능
- Admin capability bits 8/9/10 full triad
- stable DiagnosticsBuild/BootId/MapRevision
- selected mode가 live `SetOperationModeSupportedMask`에 포함
- axis/timeout valid
- explicit one-shot confirmation
- common diagnostics admission allowed

Start 직전 capability를 다시 읽고 selected mode가 사라졌으면 zero-wire로 거부한다.

## 8. revised state machine

```text
Accepted(requestedMode)
  -> OwnerReserve
  -> Preflight6061Read
     -> observed == requested
          -> SucceededNoWrite
     -> observed != requested
          -> ValidateCrossModeSafeState
               - PhysicalValid
               - Standstill
               - FaultClear
               - OperationEnabledClear
               - NoConflictingOwner
               - ExecutorReusable
          -> Write6060Requested(requestedMode)
          -> Write6060Dispatched(exact 1 byte)
          -> Verify6061Read
               -> exact requestedMode -> TerminalCandidate
               -> definitive mismatch -> Quarantine
               -> timeout/lost callback -> ReadOnlyRecovery
                    -> exact requestedMode -> TerminalCandidate
                    -> otherwise -> IndeterminateQuarantined
  -> TerminalPayloadStage/Readback
  -> OwnerRelease
  -> RecordState terminal
  -> exact-generation Retire
```

Warm-start는 `WriteDispatched`가 증명된 exact Running record만 reconstruct한다. requestedMode는 PP/PV/IP/CSP candidate 범위이며 **warm-start/reconnect recovery는 어떤 경우에도 새 `0x6060` write를 만들지 않는다.**

## 9. qualification branch 상태

현재 branch `codex/setopmode-mode11-bench-activation@eae31dd...`는 merge-base가 `dev@b3be6bf...`이고, current `dev@45fc528c...`와 **diverged** 상태다.

current qualification branch에는 activation-only commit이 존재하지만 다음 current-dev 변경이 빠져 있다.

- MODE-11E multi-mode warm-start recovery
- MODE-11F WPF diagnostics/localization
- current implementation addendum update

따라서 **다음 실장비 test에는 현재 branch head를 그대로 사용하지 않는다.**

### 9.1 다음 qualification branch 재생성 규칙

current `dev@45fc528c...`에서 새 qualification head를 만든 뒤 production source와 차이는 최소한 다음 activation delta만 허용한다.

1. `LMC_DIAG_SET_OPERATION_MODE_ENABLED = TRUE`
2. Admin capability bits 8/9/10 ON
3. qualification `SetOperationModeSupportedMask`에 시험 대상 mode bit만 명시

재생성 후 반드시 `dev...qualification` diff를 확인해 stale implementation/generated artifact가 섞이지 않았는지 증명한다.

## 10. 구현 / qualification 체크리스트

- [x] `MODE-01..09` protocol/owner/runtime/no-replay/safety/generic-0x6060-deny source 구현
- [ ] `MODE-10` **current exact `dev@45fc528c...`** C78/ARM Rebuild+Link + generated artifact identity + PLC load
- [ ] `MODE-11A` Axis1 CSP same-mode zero-write packet evidence
- [ ] `MODE-11B` Axis1 safe-state `CSP -> PP -> CSP` exact-one-write/readback
- [ ] `MODE-11C` Axis1 safe-state `CSP -> PV -> CSP` exact-one-write/readback
- [ ] `MODE-11D` Axis1 safe-state `CSP -> IP -> CSP` exact-one-write/readback
- [x] `MODE-11E` PP/PV/IP/CSP warm-start recovery generalization + no-write recovery regression
- [x] `MODE-11F` WPF preflight/rejection/write-evidence diagnostics + localization regression
- [ ] `MODE-11G` mode별 qualification evidence와 qualification/release mask coupling
- [ ] `MODE-12A` Axis1 timeout/disconnect/mismatch/quarantine/retire matrix
- [ ] `MODE-12B` Axis2..4 확대
- [x] `MODE-13` WPF durable journal / startup no-replay recovery
- [ ] `MODE-14` production capability bits 8/9/10 + production-qualified mode mask paired activation

## 11. 다음 작업 순서

### Phase A — test baseline 재정렬

1. current `dev@45fc528c...`에서 qualification branch 재생성
2. activation delta만 남았는지 diff 검증
3. LASAL generated source / `Classes.lcb`를 IDE에서 current source로 재생성
4. C78/ARM Rebuild + Link
5. exact image identity 기록
6. PLC Download/Load
7. WPF도 동일 source 기준으로 build

### Phase B — Axis1 mode matrix

8. CSP same-mode zero-write 확인
9. safe-state `CSP -> PP -> CSP`
10. safe-state `CSP -> PV -> CSP`
11. safe-state `CSP -> IP -> CSP`

cross-mode PASS 조건:

- preflight `0x6061` = source mode
- `0x6060:0 = requestedMode` exact 1-byte write **1회**
- duplicate `0x6060` write 0회
- verify `0x6061:0 = requestedMode`
- terminal success
- owner released / executor reusable
- exact-generation retire 후 slot clear

same-mode PASS 조건:

- `WriteRequested=False`
- `WriteDispatched=False`
- `ObservedMode == RequestedMode`
- owner released / executor reusable

### Phase C — failure/recovery matrix

12. unsafe preflight: OperationEnabled=true / Fault / Standstill false 등에서 zero-write rejection 확인
13. timeout before write / after possible write 구분
14. disconnect / lost callback
15. verify mismatch
16. quarantine 유지
17. reconnect/warm-start read-only recovery
18. exact-generation retire

### Phase D — evidence-driven mode mask

19. MODE-11G에서 mode별 evidence ledger를 작성한다.
20. qualification mask와 future production mask를 분리한다.
21. physical PASS 없는 mode bit는 production mask에 넣지 않는다.
22. Axis1 closure 후 Axis2..4로 동일 matrix를 확대한다.

### Phase E — release

23. current exact source/generated/C78/distribution/docs sync
24. MODE-14 paired activation review
25. runtime gate + Admin triad + production-qualified mask를 하나의 승인된 image에서 함께 변경

## 12. 실기 로그 판정 규칙

MODE-11F 이후에는 다음 식으로 원인을 구분한다.

### preflight reject

예: `Detail=SetOperationModeUnsafeState(44)` + `WriteRequested=False` + `WriteDispatched=False`

→ drive가 requested mode를 거부한 증거가 아니다. PLC safety preflight에서 mutation 전에 차단된 것이다.

### write dispatched + verify mismatch

`WriteDispatched=True`인데 `Observed != Requested`

→ 실제 `0x6060` dispatch 이후 drive/readback 문제로 분류한다. 자동 Start/Write replay 금지.

### same-mode success

`Requested == PreflightMode == Observed` + `WriteDispatched=False`

→ same-mode no-write success다. cross-mode mutation PASS 증거로 사용하지 않는다.

## 13. 현재 known blockers

현재 software blocker 중 MODE-11E/11F는 닫혔다. 남은 blocker는 다음이다.

- qualification branch가 current `dev` 기준이 아님
- current exact `dev@45fc528c...` C78/generated-artifact/PLC-load evidence 미갱신
- Axis1 CSP same-mode formal packet evidence 미완료
- Axis1 PP/PV/IP actual `0x6060` packet + `0x6061` readback physical proof 없음
- non-CSP safe-state preflight가 실제 장비에서 충족되는지 evidence 필요
- Standstill 원본 field가 current SetOperationMode outcome wire에 export되지 않음
- timeout/disconnect/mismatch/quarantine/retire matrix 미완료
- Axis2..4 physical matrix 미완료
- production mode mask를 physical evidence와 결합하는 MODE-11G/14 release gate 미완료

이 blocker가 닫히기 전 production `dev`의 runtime gate, Admin bits 8/9/10, production supported-mode mask는 계속 OFF/OFF/`0x0000`을 유지한다.

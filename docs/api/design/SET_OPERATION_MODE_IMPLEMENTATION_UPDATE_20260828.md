# SetOperationMode 구현 설계 보완 — 2026-08-28

> 상태: current implementation addendum
>
> 기준 source: `dev@b3be6bf19bd9effb2240a4713424b288643d2a3e`
>
> qualification branch: `codex/setopmode-mode11-bench-activation@eae31dd0365c4ae39f4d56874b8a1b82ab477146`
>
> 추적: issue #46, qualification PR #18
>
> 이 문서는 기존 `SET_OPERATION_MODE_DESIGN.md`의 historical evidence를 삭제하지 않고, 현재 구현과 달라진 2/4/6/8/9절의 해석을 보완한다. 충돌하는 경우 이 문서의 2026-08-28 current-state 판정을 우선한다.

## 1. 현재 판정

현재 SetOperationMode는 **multi-mode software implementation은 진행됐지만 PP/PV/IP가 CSP와 동일 수준으로 qualification된 상태는 아니다.**

- source/PC 구현 진행도: **약 70%**
- release-oriented qualification 진행도: **약 60%**
- production activation: **OFF 유지**
- `dev` runtime gate: `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE`
- `dev` Admin bits 8/9/10: OFF
- `dev` advertised `SetOperationModeSupportedMask`: 0
- qualification branch: runtime gate + Admin triad만 test-active로 올리는 `DO NOT MERGE` branch

### 1.1 기능별 판정

| 기능 | 현재 판정 | 근거 / 남은 것 |
|---|---|---|
| immutable Start/Outcome/Retire contract | 완료 | `0x7D23/0x7D24/0x7D25`, durable identity, exact-generation retire |
| PP(1)/PV(3)/IP(7)/CSP(8) request parsing | 완료 | SDK/LASAL requested-mode allow path 존재 |
| same-mode no-write | source 완료 | `0x6061 == requestedMode`이면 `SucceededNoWrite`; formal packet evidence는 CSP부터 확보 필요 |
| cross-mode `0x6060` mutation | source 완료 / physical 미완료 | requested mode를 exact 1-byte write하고 `0x6061` exact verify |
| non-CSP preflight | source 완료 | Standstill + Fault clear + OperationEnabled clear + owner/SDO conflict clear 요구 |
| write-dispatch 이후 no-replay | 완료 | recovery는 `0x6061` read-only; original Start/`0x6060` replay 금지 |
| PP/PV/IP warm-start recovery | **미완료** | warm-start candidate filter에 `requestedMode == 8` CSP-only 조건 잔존 |
| PLC `SupportedModeMask` wire/SDK/WPF 연동 | source 완료 | software mask PP/PV/IP/CSP = `0x018A`; release 의미는 아래 정책으로 제한 |
| WPF selector | 완료 | PP/PV/IP/CSP 항상 표시/선택 가능; Start는 live PLC triad/mask로 fail-closed |
| Axis1 PP/PV/IP 실제 mode change | **미완료** | exact C78/PLC image에서 packet/readback evidence 필요 |
| Axis2..4 matrix | 미완료 | Axis1 closure 이후 확대 |
| production activation | 미완료 | physical qualification 완료 후 별도 review |

## 2. 현재 정상 mutation 경로

정상 mutation path는 더 이상 CSP=8 write로 하드코딩하지 않는다.

1. `0x7D23 Start`에서 `requestedMode`를 exact durable record에 저장한다.
2. `0x6061:0`을 Int8/1 byte로 preflight read한다.
3. `observedMode == requestedMode`이면 `WriteRequested=0`, `WriteDispatched=0`으로 terminal success한다.
4. mode가 다르면 cross-mode safety preflight를 수행한다.
5. 통과하면 `AxisOperationModeState[LMC_DIAG_MODE_RUNTIME_WRITE_DATA] = requestedMode`를 사용해 `0x6060:0`에 **exact 1 byte**를 한 번 write한다.
6. write completion 후 `0x6061:0`을 다시 read한다.
7. `observedMode == requestedMode`일 때만 terminal success를 publish한다.
8. write outcome이 불확실하면 새 `0x6060` write를 만들지 않고 read-only recovery로 이동한다.

즉 PP/PV/IP가 선택됐는데 내부에서 CSP=8을 쓰는 구조는 현재 source에 없다.

## 3. CSP만 동작하는 것처럼 보이는 이유

현재 축이 CSP(8)일 때 CSP를 다시 선택하면 preflight read에서 즉시 same-mode가 성립한다. 이 경로는 실제 `0x6060` mutation이 필요 없으므로 가장 쉽게 성공한다.

반면 CSP에서 PP/PV/IP로 전환하려면 아래를 모두 만족해야 한다.

- physical context valid
- target axis Standstill
- DS402 Fault bit clear
- **DS402 OperationEnabled bit clear**
- HomeDS402/HomeDS402Ex inactive
- Encoder maintenance inactive
- same-axis generic SDO conflict 없음
- `LMCSdoExecutor` connected/reusable
- common ownership/admission tuple valid

하나라도 실패하면 `LMC_DIAG_MODE_DETAIL_UNSAFE(44)` 또는 실행 계열 detail로 terminal safe failure하고 `0x6060` write를 수행하지 않는다.

따라서 `CSP -> CSP PASS`, `CSP -> PP/PV/IP FAIL`만 관찰됐다는 사실만으로 requested-mode write가 CSP로 고정됐다고 판정하면 안 된다. 먼저 preflight detail과 actual SDO dispatch evidence를 구분해야 한다.

## 4. 확인된 구현 결함 — warm-start recovery CSP-only

`ProcessAxisSetOperationMode`의 warm-start reconstruction에는 아직 다음 의미의 조건이 남아 있다.

```text
Running record
+ valid magic / owner identity
+ WriteDispatched evidence
+ requestedMode == 8
```

이 때문에 PP/PV/IP write가 이미 dispatch된 뒤 PLC restart/reconnect가 발생한 경우 CSP와 동일한 recovery candidate로 복원되지 않는다.

### 4.1 수정 원칙

warm-start candidate는 mode 값 자체가 아니라 다음 조건으로 판정해야 한다.

- `requestedMode`가 **그 record가 만들어질 당시 loaded image에서 허용된 qualification mode**인지
- exact record magic / generation / owner tuple이 유효한지
- `WriteDispatched` evidence가 존재하는지
- `OwnerReleased`가 아직 없는 Running record인지
- axis/reference/recordBase가 exact match인지
- candidate가 singleton인지

복원 후 `LMC_DIAG_MODE_RUNTIME_WRITE_DATA`는 record의 `requestedMode`를 복사하지만, recovery stage에서는 이를 write source로 사용하지 않는다. recovery는 계속 `0x6061` read-only여야 한다.

### 4.2 acceptance criteria

- PP/PV/IP/CSP 각각 write-dispatched Running fixture가 warm-start recovery candidate가 됨
- recovery helper 내부 `0x6060` write site 0개 유지
- exact `0x6061` match만 terminal success
- mismatch/timeout/callback uncertainty는 quarantine
- 두 개 이상의 candidate가 있으면 fail-closed
- unsupported/corrupt requested mode record는 reconstruct하지 않음

## 5. SupportedModeMask 의미 재정의

현재 wire는 AdminCapabilities의 기존 reserved `UInt16` 슬롯을 `SetOperationModeSupportedMask`로 사용한다.

bit N은 DS402 mode N을 의미한다.

- PP(1): `0x0002`
- PV(3): `0x0008`
- IP(7): `0x0080`
- CSP(8): `0x0100`
- PP/PV/IP/CSP 전체 candidate mask: `0x018A`

### 5.1 중요한 구분

`SupportedModeMask`는 **현재 loaded image가 Start를 받아들일 수 있도록 enable한 mode 집합**을 뜻한다. 이것만으로 physical qualification PASS를 의미하지 않는다.

따라서 image 종류별 의미를 분리한다.

| image | Admin triad | mask | 의미 |
|---|---:|---:|---|
| production `dev` dormant | OFF | `0x0000` | 신규 SetOperationMode mutation 금지 |
| qualification candidate | ON | 시험 대상 mask | bench에서 실제 mode를 검증하기 위한 test enable |
| future production | ON | physical PASS mode만 | release evidence와 paired activation 후에만 허용 |

qualification branch에서 `0x018A`를 광고하는 것은 PP/PV/IP/CSP가 이미 release-qualified라는 뜻이 아니다. WPF에는 qualification banner와 live mask를 명확히 표시해야 한다.

### 5.2 production 정책

production activation review에서는 mode별 evidence matrix를 기준으로 mask를 구성한다. 한 mode라도 physical evidence가 없으면 그 bit를 production mask에 넣지 않는다.

## 6. WPF/SDK 동작 계약

PR #50 이후 selector UX와 wire admission을 분리한다.

### selector

- PP/PV/IP/CSP 4개 software-known target은 항상 표시한다.
- PLC mask가 0이어도 operator가 mode를 선택해 UI/로그를 확인할 수 있다.
- Homing(6)은 표시하지 않는다.

### Start

Start는 다음 조건을 모두 만족해야만 enabled/dispatch 가능하다.

- connected + idle
- no unresolved SetOperationMode recovery
- durable recovery journal arm 가능
- Admin capability bits 8/9/10 full triad
- stable DiagnosticsBuild/BootId/MapRevision
- selected mode가 live `SetOperationModeSupportedMask`에 포함
- axis/timeout valid
- explicit one-shot confirmation
- common diagnostics admission allowed

Start 직전 capabilities를 다시 읽고 selected mode가 사라졌으면 zero-wire로 거부한다.

### 진단 보완 필요

현재 다음 operator-visible 정보가 부족하다.

- Start definitive rejection의 symbolic/numeric DetailCode
- requested mode / preflight observed mode
- DS402 StatusWord snapshot
- Standstill / OperationEnabled / Fault 판단 결과
- owner/resource conflict 종류
- SDO executor connected/reusable 여부
- actual `WriteRequested/WriteDispatched` 여부

다음 WPF tranche에서 이 정보를 로그와 evidence export에 포함한다.

## 7. revised state machine

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

Warm-start는 `WriteDispatched`가 증명된 Running record만 reconstruct하며, requestedMode는 PP/PV/IP/CSP candidate 범위로 일반화한다. **warm-start/reconnect recovery는 어떤 경우에도 새 `0x6060` write를 만들지 않는다.**

## 8. 구현 체크리스트 개정

기존 MODE-01..14 historical numbering은 유지하고, current multi-mode work를 다음 sub-gate로 추가한다.

- [x] `MODE-01..09` protocol/owner/runtime/no-replay/safety/generic-0x6060-deny source 구현
- [ ] `MODE-10` current exact `dev` C78/ARM Rebuild+Link + generated artifact identity + PLC load 재검증
- [ ] `MODE-11A` Axis1 CSP same-mode zero-write packet evidence
- [ ] `MODE-11B` Axis1 safe-state cross-mode `CSP -> PP -> CSP` exact-one-write/readback
- [ ] `MODE-11C` Axis1 safe-state cross-mode `CSP -> PV -> CSP` exact-one-write/readback
- [ ] `MODE-11D` Axis1 safe-state cross-mode `CSP -> IP -> CSP` exact-one-write/readback
- [ ] `MODE-11E` **PP/PV/IP/CSP warm-start recovery generalization** + no-write recovery regression
- [ ] `MODE-11F` WPF preflight/rejection diagnostics 및 evidence export
- [ ] `MODE-11G` mode별 qualification evidence와 qualification/release mask coupling
- [ ] `MODE-12` Axis1 timeout/disconnect/mismatch/quarantine/retire + Axis2..4 확대
- [x] `MODE-13` WPF durable journal / startup no-replay recovery
- [ ] `MODE-14` production capability bits 8/9/10 + production-qualified mode mask paired activation

## 9. 실기 테스트 순서

### 9.1 먼저 확인할 공통 상태

1. exact qualification branch / exact C78 image identity
2. `AdminTriad=True`
3. qualification `SupportedModeMask` 확인
4. `0x6061` current mode read
5. DS402 StatusWord read
6. Standstill=true
7. Fault=false
8. OperationEnabled=false — cross-mode test 필수
9. conflicting owner/SDO 없음

### 9.2 same-mode

각 mode가 실제로 current mode가 된 뒤 동일 mode Start를 실행한다.

PASS 조건:

- `WriteRequested=0`
- `WriteDispatched=0`
- terminal `ObservedMode == RequestedMode`
- owner released / executor reusable

### 9.3 cross-mode

PP/PV/IP 각각에 대해 Axis1에서 먼저 수행한다.

PASS 조건:

- preflight `0x6061`은 source mode
- `0x6060:0 = requestedMode` exact 1-byte write **1회**
- duplicate write 0회
- verify `0x6061:0 = requestedMode`
- terminal success evidence
- retire 후 slot clear
- CSP 복귀도 동일한 exact-one-write/readback 규칙을 만족

현재 ordinary motion은 non-CSP mode에서 별도 activation하지 않는다. mode change qualification과 PP/PV/IP motion qualification을 혼동하지 않는다.

## 10. 다음 구현 순서

1. **MODE-11E warm-start recovery multi-mode 일반화**
2. source/static regression에서 recovery helper `0x6060` write 0개 재확인
3. **MODE-11F WPF failure/preflight evidence 상세화**
4. qualification branch를 current `dev`에서 재생성하고 candidate mask를 명시
5. fresh C78/ARM Rebuild + Link + generated artifact capture
6. Axis1 CSP same-mode / PP / PV / IP / CSP-return physical matrix
7. mode별 PASS 결과에 따라 qualification/release mask 정책 갱신
8. timeout/disconnect/mismatch/quarantine/retire matrix
9. Axis2..4 확대
10. MODE-14 production paired activation review

## 11. 진행도 산정 규칙

향후 SetOperationMode 진행도는 source가 존재한다는 이유만으로 올리지 않는다.

- **source/PC implementation 약 70%**: multi-mode normal path, supported-mask, WPF selector까지 구현됨
- **release-oriented 약 60%**: non-CSP recovery와 actual hardware matrix가 미완료
- PP/PV/IP physical PASS 전에는 `multi-mode complete`로 기록하지 않는다.
- qualification `0x018A` 광고만으로 production support 완료로 기록하지 않는다.
- C78/generated artifact/PLC load는 같은 exact source tree 증거여야 한다.
- branch-only 또는 stale artifact evidence를 current `dev` 완료 근거로 사용하지 않는다.

## 12. 현재 known blockers

- warm-start recovery CSP-only candidate condition
- non-CSP cross-mode preflight가 실제 장비 상태에서 만족되는지 evidence 부족
- PP/PV/IP 실제 `0x6060` packet + `0x6061` readback physical proof 없음
- operator-visible rejection/preflight detail 부족
- current exact `dev` C78/generated-artifact evidence 갱신 필요
- production mode mask를 physical evidence와 결합하는 release gate 미완료

이 blocker가 닫히기 전 production `dev`의 runtime gate, Admin bits 8/9/10, production supported-mode mask는 계속 OFF/0을 유지한다.

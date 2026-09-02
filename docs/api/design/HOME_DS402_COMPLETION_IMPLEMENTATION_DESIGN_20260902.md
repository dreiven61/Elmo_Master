# HomeDS402 완료 구현 설계 — 2026-09-02

- 대상: No.19 `MMC_HomeDS402Cmd`
- 기준 branch: `dev`
- source baseline: `dev@90a86a795773d5f8eca211368aac3f0d64944a32` (`dev : SDO Write Func Complete`)
- current 상태: `Dormant`, core lifecycle/source/WPF recovery 구현 완료, activation OFF
- tracker: issue #32
- command: `0x7D15 Start`, `0x7D16 ReadOutcome`, `0x7D17 Retire`
- 의미: DS402 method 37, 현재 위치를 0으로 확정하는 non-search Home
- production posture: **NO-GO until artifact/hardware/activation gates close**

이 문서는 기존 `HOME_DS402_DESIGN.md`의 wire/state-machine 계약을 변경하지 않고,
**현재 dev에서 실제 남은 작업만 실행 순서대로 고정하는 completion handoff**다.

가장 중요한 결론은 다음과 같다.

> HomeDS402는 신규 state machine을 구현하는 작업이 아니다. 현재 source의 method37 lifecycle을
> 다시 작성하지 말고, current source/artifact를 닫고 실제 PLC/hardware에서 검증한 뒤 5개 gate를
> 원자적으로 활성화하는 작업이다.

---

## 1. current source truth

이미 구현되어 있으므로 중복 구현하지 않는다.

### SDK / WPF

- `LmcAxisDs402Home.cs`: prepare/start/query sync/async
- `LmcAxisDs402HomeOutcomeRetirement.cs`: generation-bound retire
- `LmcAdminDs402Home*.cs`: immutable request/recovery key/outcome parser
- WPF explicit confirmation + durable recovery journal
- startup unresolved record -> exact Query/Retire recovery
- reconnect/restart 뒤 original `0x7D15` Start automatic replay 없음

### LASAL

- `TCPMotionInterface.st`: `0x7D15` route + ordinary axis owner admission
- `LMCDiagnosticsService.st`: Start/Outcome/Retire + `ProcessAxisDs402Home`
- `LMCEcatInputLatch.st`: RT controlword/start-bit/setpoint alignment/safety drain
- `LMCControlCommandService.st`: shared axis ownership/preemption
- axis 1..4 SDO executor / InputLatch / owner wiring 존재

### current qualification tooling

- `tools/Verify-HomeDs402H37Activation.ps1`
- `tools/Verify-HomeDs402H37Ownership.ps1`
- `tools/Verify-HomeDs402H37MethodSize.ps1`
- `tools/Verify-HomeDs402H37WpfRecovery.ps1`
- `tools/Capture-HomeDs402H37C78Evidence.ps1`
- `AdminDs402HomeH37QualificationTests.cs`

PR #40 기준 hardware-independent qualification은 이미 통합돼 있다.

- activation contract: 43 checks PASS
- ownership/preemption: 21 checks PASS
- method-size: 10 checks PASS
- WPF recovery/no-replay: 36 checks PASS
- API Debug/Release full suite PASS

이 증거는 PLC/hardware activation 증거가 아니다.

---

## 2. frozen runtime contract

다음 순서를 변경하지 않는다.

```text
Admission / OwnerReserve
-> Read 0x6061 baseline
-> Write 0x607C = 0
-> Write 0x6098 = 37
-> Acquire RT control owner
-> Write 0x6060 = 6
-> Verify 0x6061 = 6
-> Raise controlword bit4
-> Observe Home attained / Target reached / no error / ActualPosition=0
-> Lower bit4
-> Align LASAL setpoint
-> Write 0x6060 = 8
-> Verify 0x6061 = 8
-> Release RT owner
-> Fresh post-release ActualPosition=0
-> Commit terminal outcome
```

성공 조건은 모두 AND다.

- Homing attained bit12 fresh 3 samples
- Target reached bit10 fresh 3 samples
- Homing error bit13 clear
- Fault bit3 clear
- ActualPosition = 0
- controlword start bit low
- mode 8 restored
- LASAL setpoint aligned
- RT owner released
- pending SDO/orphan/uncertainty 없음
- terminal record commit/query 가능

어느 cleanup 단계든 불확실하면 `Succeeded`로 축소하지 않는다.

---

## 3. activation 원자성

아래 5개는 반드시 한 changeset에서 `OFF -> ON` 한다.

| 소유 위치 | gate |
|---|---|
| `TCPMotionInterface.st` | `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED` |
| `LMCControlCommandService.st` | `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED` |
| `LMCDiagnosticsService.st` | `LMC_DIAG_DS402_HOME_ENABLED` |
| `LMCEcatInputLatch.st` | `LMC_DS402_HOME_STARTUP_SWEEP_ENABLED` |
| Admin capability | HomeDS402 feature bit 6 |

허용 상태:

```text
0/0/0/0/0
1/1/1/1/1
```

모든 mixed state는 verifier에서 실패해야 한다.

Hardware gate가 닫히기 전에는 source가 준비됐다는 이유로 위 값을 켜지 않는다.

---

# 4. 구현 tranche

## H37-C0 — current-dev regression rebaseline

SDO Write 완료 commit 이후 shared SDO executor/diagnostics code가 바뀌었으므로 먼저 HomeDS402의
기존 source contract가 깨지지 않았는지 확인한다.

실행:

```powershell
./tools/Verify-HomeDs402H37Activation.ps1
./tools/Verify-HomeDs402H37Ownership.ps1
./tools/Verify-HomeDs402H37MethodSize.ps1
./tools/Verify-HomeDs402H37WpfRecovery.ps1
```

그리고 API/WPF targeted tests를 current `dev`에서 다시 실행한다.

PASS 조건:

- 기존 H37 lifecycle/packet shape 불변
- shared SDO changes로 HomeDS402 write/read sequence 변형 없음
- Stop/PowerOff/Reset/Group/maintenance preemption regression 없음
- duplicate Start/replay 0
- method-size budget 유지

이 단계에서 runtime behavior를 수정하지 않는다. 실패한 assertion이 실제 regression일 때만 최소 수정한다.

## H37-C1 — exact current-tree C78/generated artifact closure

issue #32의 blocker를 닫는다.

1. exact current `dev` source SHA 기록
2. rebuild 시작 UTC 기록
3. LASAL C78/ARM Rebuild + Link
4. `0 errors`, Link success, 새 `CInvalidArgException=0`
5. build-start 이후 생성된 다음 artifact 확인
   - `Class/Classes.lcb`
   - `Elmo_EtherCAT_Test_4Axis.lcb`
   - `Network/Networks.lcb`
6. direct-open 확인
   - `HandleAxisDs402HomeStart`
   - `HandleAxisDs402HomeOutcome`
   - `HandleAxisDs402HomeRetire`
   - `ProcessAxisDs402Home`
7. Network smoke
   - `TCPMotionInterface`
   - `LMCControlCommandService`
   - `LMCDiagnosticsService`
   - `LMCEcatInputLatch`
8. `Capture-HomeDs402H37C78Evidence.ps1`로 receipt 생성
9. artifact identity를 수동 review
10. justified identity만 physical ratchet에 반영
11. full SourceOnly rerun

기존 tracked hash를 verifier에 그대로 복사해서 맞추는 방식은 금지한다.

### H37-C1 완료조건

- evidence `CAPTURED_FOR_REVIEW`
- fresh generated artifacts
- reviewed identity
- full SourceOnly PASS
- activation values 여전히 all-OFF

## H37-C2 — activation candidate source preparation

C1 완료 후에도 즉시 gate를 켜지 않는다. 먼저 **activation candidate patch를 OFF 상태로 준비**한다.

확인 항목:

- Admin bit6 계산 위치가 single source of truth인지 확인
- WPF가 bit6 OFF에서 Start UI를 열지 않는지 확인
- bit6 ON fixture에서만 HomeDS402 UI/action이 활성화되는지 smoke 작성/갱신
- 5-value mixed-state negative test 유지
- activation revert가 한 commit으로 가능한지 확인

이 단계에서 제품 source gate는 OFF 유지한다.

## H37-C3 — same-image PLC/hardware Axis1 qualification

C1에서 검증한 exact source/artifact를 PLC에 download한다.

먼저 Axis1만 수행한다.

### Normal

```text
fresh Connect
-> exact Build/BootId/MapRevision capture
-> PowerOff / Standstill / position-stable preflight
-> Start once
-> Running query
-> terminal query
-> exact generation Retire
-> physical/online status verification
```

필수 evidence:

- `0x7D15` exactly once
- mode 6 entry / mode 8 restore
- controlword bit4 high -> low
- attained/target/error/fault bits
- ActualPosition 0
- setpoint alignment
- owner release
- pending SDO/orphan 0

### Failure matrix

각 case는 별도 run/evidence로 남긴다.

- SDO abort before homing start
- timeout before completion
- Stop preemption
- PowerOff preemption
- TCP response loss
- disconnect during Running
- query after response loss
- exact retire retry
- BootId change with unresolved record

mutation이 시작된 뒤에는 original Start replay 0회다.

### H37-C3 완료조건

Axis1 normal + failure matrix가 같은 source/artifact/image identity에서 PASS.

## H37-C4 — Axis2..4 expansion

Axis1 결과를 복사해 승인하지 않는다.

각 axis에서 최소:

- normal
- timeout
- Stop/PowerOff preemption
- disconnect/response-loss recovery
- final mode8/ActualPosition0/owner-release

를 반복한다.

## H37-C5 — atomic activation

C0~C4가 모두 PASS한 뒤에만 5개 gate를 한 commit에서 ON한다.

Activation commit acceptance:

- 5/5 ON
- bit6 advertised
- WPF feature available
- H37 source/static suites PASS
- API/WPF Debug/Release PASS
- C78 Rebuild/Link PASS
- same-image PLC normal smoke PASS
- rollback commit path 확인

---

# 5. 개발 파일별 작업 규칙

| 파일/영역 | 현재 작업 원칙 |
|---|---|
| `LMCDiagnosticsService.st` | lifecycle 재작성 금지. hardware evidence에서 발견된 결함만 최소 수정 |
| `LMCEcatInputLatch.st` | RT start-bit/observer/alignment 계약 유지 |
| `LMCControlCommandService.st` | shared owner/preemption 계약 약화 금지 |
| `TCPMotionInterface.st` | packet ABI와 ordinary owner gate 유지 |
| C# SDK | `0x7D15/16/17` wire/parser 변경 금지 |
| WPF | no-replay recovery와 bit6 gating 회귀만 보강 |
| verifier | artifact hash 우회/완화 금지 |

---

# 6. 자동 회귀 필수 목록

최소 다음을 current `dev`에 고정한다.

- exact Start payload 72 bytes
- exact Outcome/Retire key + generation
- duplicate Start -> no new native/SDO mutation
- response loss -> Query/Retire only
- mixed activation state rejection
- Stop/PowerOff/Reset/Group preemption
- WPF unresolved startup -> RecoveryRequired
- WPF recovery -> Start replay 0
- method size < 32 KiB
- source/artifact identity mismatch -> activation blocked

---

# 7. 완료 판정

HomeDS402를 `Active`로 올리려면 다음이 모두 필요하다.

- [x] H37-02 activation contract
- [x] H37-03 exact lifecycle PC contract
- [x] H37-04 ownership/preemption
- [x] H37-10 WPF durable no-replay recovery
- [ ] H37-C0 current-dev regression rebaseline
- [ ] H37-C1 fresh C78/generated artifact + SourceOnly closure
- [ ] H37-C2 activation candidate OFF-state qualification
- [ ] H37-C3 Axis1 normal/failure hardware matrix
- [ ] H37-C4 Axis2..4 expansion
- [ ] H37-C5 5-value atomic activation

`HomeDS402Ex`의 switch/index search evidence를 이 완료조건으로 대체하지 않는다.

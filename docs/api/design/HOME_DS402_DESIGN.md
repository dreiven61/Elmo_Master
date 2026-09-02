# HomeDS402 최우선 개발 설계

- 대상: No.19 `MMC_HomeDS402Cmd`
- 현재 진행도: 50%
- current 상태: source implemented, deployment `Dormant`
- current baseline: `dev@1f741bfd08e9d75a52f7edd03862ef26ac562edd`
- current qualification: H37-02/03/04/10 software/source qualification이 PR #40으로 `dev` 통합 완료
- existing command: `0x7D15 Start`, `0x7D16 ReadOutcome`, `0x7D17 Retire`
- v1 의미: DS402 method 37, 현재 위치를 0으로 설정하는 non-search Home

## 1. 목표와 비범위

목표는 이미 구현된 method 37 lifecycle을 source/build/PLC/hardware까지 적격화하고 atomic
activation하는 것이다. 이 기능은 Home/limit/index switch를 찾아 이동하는 일반 homing이
아니다. Position, velocity, acceleration, distance와 torque는 모두 0이며 현재 위치를 DS402
Home으로 확정한다.

다음은 이 문서의 범위가 아니다.

- switch/index/limit search와 nonzero offset
- arbitrary DS402 homing method
- `HomeDS402Ex`
- start ACK만으로 완료 판정
- connection loss 뒤 original Start 자동 재전송

## 2. current 구현

### C#과 WPF

- `LmcAxisDs402Home.cs`: prepare/start/query sync/async
- `LmcAxisDs402HomeOutcomeRetirement.cs`: generation-bound retire
- `LmcAdminDs402Home*.cs`: immutable request, recovery key, parser와 outcome model
- `MainWindow.MaintenanceActions.cs`: explicit confirmation과 recovery journal 선등록
- unresolved durable DS402 Home journal은 WPF startup에서 exact recovery key로 재구성
- recovery path는 original `0x7D15` Start를 재전송하지 않고 Query/Retire만 사용

### LASAL

- `TCPMotionInterface.st`: `0x7D15` route와 ordinary owner admission
- `LMCDiagnosticsService.st`: Start/ReadOutcome/Retire와 `ProcessAxisDs402Home`
- `LMCEcatInputLatch.st`: RT controlword, setpoint alignment와 safety drain
- `LMCControlCommandService.st`: unified axis ownership/preemption
- Network: 축 1~4 SDO executor와 InputLatch/ownership 연결

### current qualification tooling

- `tools/Verify-HomeDs402H37Activation.ps1`
- `tools/Verify-HomeDs402H37Ownership.ps1`
- `tools/Verify-HomeDs402H37MethodSize.ps1`
- `tools/Verify-HomeDs402H37WpfRecovery.ps1`
- `AdminDs402HomeH37QualificationTests.cs`
- `.github/workflows/home-ds402-h37-source-qualification.yml`

## 3. frozen wire

wire 정본은 `DINT_PACKET_MAP.txt`의 current `0x7D15/16/17` 항목이다. 활성화 작업에서
offset을 변경하지 않는다.

### Start `0x7D15`, 72 bytes

- P8/P12/P16: expected Diagnostics build/BootId/MapRevision
- P20..P32: nonzero 128-bit ClientIntentId
- P36: HomingMethod = 37
- P40/P44/P48/P52/P56: Position/Velocity/Acceleration/Distance/Torque = 0
- P60: BufferMode = Aborting, P62 Reserved = 0
- P64: nonzero overall timeout
- P68: ExecuteToken `0x32303448` (`H402`)

24-byte ACK는 echoed method와 NativeCommandState만 반환하며 terminal proof가 아니다.

### Outcome/Retire

- `0x7D16` request 44 bytes, success response 92 bytes
- State: Running, Succeeded, Failed, Aborted
- key: build/BootId/map/original RequestId/ClientIntentId/axis/method
- terminal: status/error/detail, Statusword, ActualPosition, cycles, native state, generation
- `0x7D17` request 48 bytes, exact key + nonzero generation
- exact retire retry는 idempotent이며 Start를 replay하지 않는다.

## 4. activation 원자성

다음 5개 값은 하나의 activation changeset에서 모두 OFF 또는 모두 ON이어야 한다.

| 파일 | 값 |
|---|---|
| `TCPMotionInterface.st` | `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED` |
| `LMCControlCommandService.st` | `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED` |
| `LMCDiagnosticsService.st` | `LMC_DIAG_DS402_HOME_ENABLED` |
| `LMCEcatInputLatch.st` | `LMC_DS402_HOME_STARTUP_SWEEP_ENABLED` |
| Admin response | HomeDS402 feature bit 6 |

PR #40 current-dev qualification에서 all-OFF/all-ON 및 mixed-state negative contract를
고정했다. 2026-09-02 corrected verifier는 **46 checks PASS**다. HomeDS402 capability는
Admin mask `0x00000757`의 bit 6이며 current source에서 ON이다. Diagnostics operational mask
`0x0000613F`의 bit 6은 RecorderDoubleBank이므로 Home capability가 아니다. 이 verifier PASS는
source/static 원자성 증거이며 PLC image나 hardware activation 증거가 아니다.

ordinary ownership은 Home 전용이 아니므로 Stop, PowerOff, Reset, SetPosition, encoder maintenance와
Group preemption 회귀도 같은 gate에 포함한다.

## 5. runtime 상태와 성공 조건

```text
Admission/OwnerReserve
  -> Read 0x6061 baseline
  -> Write 0x607C=0 and 0x6098=37
  -> Acquire RT control owner
  -> Write 0x6060=6 and verify 0x6061=6
  -> Raise controlword bit 4
  -> Observe attained/target/no-error/ActualPosition=0 on 3 fresh cycles
  -> Lower bit 4
  -> Align LASAL setpoint
  -> Write 0x6060=8 and verify 0x6061=8
  -> Release RT owner
  -> Fresh post-release ActualPosition=0 proof
  -> Commit terminal outcome
```

Success는 다음을 모두 만족해야 한다.

- Homing attained bit 12와 target reached bit 10을 homing mode에서 fresh 3회 확인
- Homing error bit 13과 Fault bit 3 clear
- ActualPosition = 0
- start bit low, CSP 8 복원, setpoint alignment와 RT owner release 완료
- pending SDO, callback/orphan drain, uncertainty flag 없음
- terminal record commit/readback 완료

cleanup 중 하나라도 불확실하면 success 또는 safe retry로 축소하지 않는다. outcome은
Indeterminate/Quarantined로 보존하고 original Start를 재전송하지 않는다.

## 6. 시험 순서

### Source/static

1. 5개 activation value의 all-OFF/all-ON만 허용하고 모든 mixed mutation을 거부한다.
2. exact method37 Start/Running/terminal/retire packet contract를 검증한다.
3. write-dispatch 이후 duplicate Start와 owner release-before-terminal을 거부한다.
4. ordinary ownership/preemption 공통 회귀를 검증한다.
5. custom method-size를 32 KiB 미만으로 유지한다.
6. WPF startup/reconnect recovery가 Start replay 없이 exact recovery key만 복원하는지 검증한다.
7. full SourceOnly의 source gate와 generated-artifact ratchet을 분리 판정한다.

### IDE/PLC

1. 같은 current source tree에서 fresh C78/ARM Rebuild/Link를 수행한다.
2. generated method/network 연결과 artifact identity를 직접 확인한다.
3. C78 0 error, linker 성공, 새 `CInvalidArgException` 0을 확인한다.
4. 동일 artifact를 PLC에 다운로드하고 build/BootId/map tuple을 기록한다.

### Hardware/packet

1. 축 1에서 PowerOff/Standstill/position-stable preflight를 기록한다.
2. 정상 Start 1회, Running query, terminal query, exact retire를 캡처한다.
3. ActualPosition=0과 6061=8 복귀를 online/packet으로 확인한다.
4. timeout, SDO abort, Stop/PowerOff preemption, disconnect와 response-loss를 검증한다.
5. 축 2~4에 같은 matrix를 반복한다.

## 7. 작업 체크리스트

- [x] `H37-01` packet map의 receipt/drain 미완료 문구와 current source 차이 해소
- [x] `H37-02` all-OFF/all-ON activation과 mixed-state negative verifier 고정
- [x] `H37-03` method37 exact request/terminal/retire PC runner와 packet assertion 작성
- [x] `H37-04` ordinary ownership 공통 회귀 Stop/Power/Reset/Group/maintenance 추가
- [ ] `H37-05` activation candidate SourceOnly/method-size PASS — method-size/source gate PASS, fresh generated artifact ratchet closure 미완료
- [ ] `H37-06` C78 Rebuild/Link, method direct-open와 Network smoke PASS
- [ ] `H37-07` Axis1 normal/timeout/fault/disconnect/response-loss matrix PASS
- [ ] `H37-08` Axis2 matrix + Axis3/4 deterministic nonphysical rejection PASS
- [ ] `H37-09` 5개 gate와 Admin capability bit 6 paired runtime qualification — source candidate는 atomic ON
- [x] `H37-10` WPF recovery journal/startup no-replay recovery qualification

## 8. 2026-08-27 current-dev qualification checkpoint

PR #40 `test(h37): qualify HomeDS402 source and recovery on current dev`를 current `dev`에서
qualification한 뒤 squash merge했다.

- qualified head: `f39fe0e9b56b0994619aed3f68b22c33a86d3b24`
- workflow run: `33026506170`
- successful rerun job: `98369296568`
- merge commit: `1f741bfd08e9d75a52f7edd03862ef26ac562edd`

결과:

- H37-02 activation contract: **43 checks PASS**
- H37-03 exact `0x7D15/16/17` PC lifecycle: PASS
- H37-04 ownership/preemption: **21 checks PASS**
- method-size: **10 checks PASS**
  - `HandleAxisDs402HomeStart`: 22,041 bytes
  - `HandleAxisDs402HomeOutcome`: 7,255 bytes
  - `HandleAxisDs402HomeRetire`: 4,221 bytes
  - `ProcessAxisDs402Home`: 29,497 bytes (< 32,768)
- H37-10 WPF durable no-replay source contract: **36 checks PASS**
- API Debug/Release full suites: PASS
- WPF Debug/Release `MaintenanceJournal` + `Ds402Home` smoke: PASS
- diff hygiene: PASS

첫 workflow attempt는 위 네 verifier까지 PASS한 뒤 hosted Windows runner의 MSBuild 탐색 문제로
중단됐다. source workaround 없이 동일 head의 failed job만 재실행했고 전체 qualification이 green으로
완료됐다.

full SourceOnly은 source/static gate를 통과한 뒤 다음 exact generated-artifact boundary에 도달한다.

`LASAL.UdpCallbackContract blocker: SetPosition-augmented Classes.lcb physical identity drifted.`

이 경계는 **H37-05/06 완료가 아니다**. fresh C78/ARM rebuild와 generated artifact identity review 후
정당한 ratchet update 여부를 별도로 승인해야 한다. source/static PASS를 C78/artifact PASS로 승격하지
않는다.

## 9. 다음 개발 tranche

1. H37 fresh C78 evidence collector를 current `dev`에 통합한다.
2. 같은 source tree에서 실제 C78/ARM Rebuild/Link evidence를 수집한다.
3. generated `Classes.lcb`/project/network artifact identity를 review한다.
4. H37-05 SourceOnly artifact ratchet을 정당한 evidence로 닫는다.
5. H37-06 direct-open/Network smoke 후 H37-07 Axis1 hardware matrix로 이동한다.
6. H37-07/08이 끝나기 전 source ON을 runtime/production Active로 판정하지 않는다.

## 10. release 경계

축 1 성공만으로 축 2를 승인하지 않으며 Axis3/4는 nonphysical rejection으로 검증한다. method37 성공을 switch-search Home 또는
HomeDS402Ex 증거로 사용하지 않는다. PLC warm state/outcome이 cold-power durable하다고
주장하지 않으며, BootId가 바뀐 unresolved record는 자동 replay 없이 operator recovery로
남긴다.

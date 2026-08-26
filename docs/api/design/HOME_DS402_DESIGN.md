# HomeDS402 최우선 개발 설계

- 대상: No.19 `MMC_HomeDS402Cmd`
- 현재 진행도: 50%
- current 상태: source implemented, deployment `Dormant`
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

### LASAL

- `TCPMotionInterface.st`: `0x7D15` route와 ordinary owner admission
- `LMCDiagnosticsService.st`: Start/ReadOutcome/Retire와 `ProcessAxisDs402Home`
- `LMCEcatInputLatch.st`: RT controlword, setpoint alignment와 safety drain
- `LMCControlCommandService.st`: unified axis ownership/preemption
- Network: 축 1~4 SDO executor와 InputLatch/ownership 연결

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
| Admin response | operational feature mask `0x0000613F -> 0x0000617F` (bit 6 only) |

현재 `dev`의 non-Home operational feature mask는 `0x0000613F`이며 HomeDS402 activation은 다른 feature bit를 바꾸지 않고 bit 6(`0x40`)만 추가한 `0x0000617F`이어야 한다. 이 값은 과거 `0x17/0x57` 예시를 current source에 맞춰 교정한 것이다.

activation verifier는 혼합 상태를 거부해야 한다. ordinary ownership은 Home 전용이 아니므로
Stop, PowerOff, Reset, SetPosition, encoder maintenance와 Group preemption 회귀도 같은 gate에
포함한다.

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

1. current receipt/safety-drain source와 packet map의 남은-item 설명을 대조한다.
2. 5개 activation value의 all-OFF/all-ON만 허용하고 모든 mixed mutation을 거부한다.
3. write-dispatch 이후 duplicate Start와 owner release-before-terminal을 거부한다.
4. method 37 이외, nonzero motion parameter와 malformed key를 zero-native로 거부한다.
5. custom method-size와 full SourceOnly를 같은 tree에서 통과한다.

### IDE/PLC

1. generated method/network 연결을 직접 확인한다.
2. C78 Rebuild/Link 0 error와 새 `CInvalidArgException` 0을 확인한다.
3. 동일 artifact를 PLC에 다운로드하고 build/BootId/map tuple을 기록한다.

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
- [ ] `H37-05` activation candidate SourceOnly/method-size PASS
- [ ] `H37-06` C78 Rebuild/Link, method direct-open와 Network smoke PASS
- [ ] `H37-07` Axis1 normal/timeout/fault/disconnect/response-loss matrix PASS
- [ ] `H37-08` Axis2~4 동일 matrix PASS
- [ ] `H37-09` 5개 gate와 global capability bit 6 paired activation
- [ ] `H37-10` WPF recovery journal과 API manual/progress 갱신

`H37-01` source/static 대조는 2026-08-20에 완료했다. current source의 staged
`PublishAxisOwnershipDs402Receipt` owner-release/rollback receipt와
`RequestDs402HomeSafetyDrain` bit-4 low/readback, exact dispatch barrier, late-command
tombstone을 기존 focused verifier와 대조했고 packet map과 delivery README의 과거
미구현 표현을 정정했다. 이 완료는 C78 build, PLC download 또는 실축 검증을
의미하지 않는다.

### H37-02/03/04 source/PC qualification — 2026-08-26

- H37-02 atomic activation verifier: workflow `32924160296`, job `98043523551`
  - current activation mask `0x0000613F`, 5개 tracked gate 모두 OFF 확인
  - all-OFF/all-ON 2개만 허용하고 30개 mixed vector 전부 reject
  - verifier `43 checks PASS`
- H37-03 method37 PC runner: 동일 workflow에서 exact `0x7D15/16/17` Start/Running/terminal/retire와 no-replay assertion PASS
- H37-04 ownership regression: workflow `32924573097`, job `98044740310`
  - HomeDS402 OwnerKind 4 / ResourceKind 3 / active state 6 / lifecycle command `0x7D15` 고정
  - Stop/Power safety preemption, Axis/Group Reset, Group/non-group collision, encoder maintenance `0x7E53` regression 검증
  - ownership verifier `21 checks PASS`
- H37-02/03 workflow Debug/Release full API suite: 각각 `1194/1194 PASS`, 0 warnings / 0 errors, diff hygiene PASS
- H37-04 workflow Debug/Release full API suite: 각각 `1194/1194 PASS`, 0 warnings / 0 errors, diff hygiene PASS

이 증거는 Source/static 및 PC contract qualification이다. 5개 activation gate는 계속 OFF이며
H37-05 full SourceOnly/method-size, H37-06 C78/Network, PLC load/runtime 또는 hardware activation을
의미하지 않는다.

## 8. release 경계

축 1 성공만으로 축 2~4를 승인하지 않는다. method37 성공을 switch-search Home 또는
HomeDS402Ex 증거로 사용하지 않는다. PLC warm state/outcome이 cold-power durable하다고
주장하지 않으며, BootId가 바뀐 unresolved record는 자동 replay 없이 operator recovery로
남긴다.
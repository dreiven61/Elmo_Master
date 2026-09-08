# Current implementation handoff — 2026-09-07

- branch / source: `dev@4821797d9279770ba4e3ff396eae4dbb73d421d1`
- functional code baseline: `1852bd2e4f9fe13588420537fe7ed2ad6b33c1ef` (`fix : servo on error fix`)
- latest EtherCAT testbed update: `c6bda1e31a0b355037a0c4e75131e9cc5a26752a`
- latest history-only commit: `4821797d9279770ba4e3ff396eae4dbb73d421d1`
- current uncommitted implementation: Diagnostics static topology aligned to the two-drive testbed
- production posture: **NO-GO**

이 문서는 2026-09-02 설계 이후 실제 소스 변경과 2026-09-07 current HEAD 검증을 반영한
재개 정본이다. 기존 문서의 frozen wire/state-machine 계약은 유지하지만, current 상태와 다음 순서는
이 문서를 우선한다.

## 1. 결론 — 어디서부터 진행할 것인가

`c6bda1e` testbed EtherCAT 변경이 반영된 PLC를 읽기 전용으로 확인한 결과, Admin은
`PhysicalAxisCount=2`를 반환하지만 Diagnostics static topology는 이전 7-node/CREVIS inventory를
반환했다. 따라서 P0-0에서 이 stale inventory를 먼저 수정했다. 새 기능을 추가한 것이 아니라
current EtherCAT configuration과 공개 inventory를 일치시킨 것이다.

진행 순서는 아래로 고정한다.

```text
P0-0 current image identity / EtherCAT baseline closure
  -> P0-1 Axis Power On/Off lifecycle physical qualification
  -> P0-2 HomeDS402 Method 37 physical completion
  -> P0-3 Generic SDO physical completion evidence
  -> P1-0 SetPosition SP-C1 external prerequisite capture
  -> P1-1 SetPosition SP-C2 durable A/B backend implementation
  -> P2 HomeDS402Ex physical runtime
```

즉, **현재 시작점은 P0-0 source fix의 새 C78 build/download와 P0-1~P0-2 재검증**이다. 이 단계가 통과하면
**다음 신규 코드 구현 시작점은 SetPosition SP-C2**다. 단, SP-C2는 SP-C1의 vendor CRC golden
fixture와 LASAL IDE-generated `_FileSys` ABI가 확보된 뒤에만 시작한다.

## 2. 2026-09-02 이후 반영된 변경

### 2.1 Servo Power lifecycle 수정

`1852bd2`에 다음 수정이 반영됐다.

- direct ordinary Axis Power On을 retained rebase bit만으로 거절하지 않음
- 2026-09-08 후속 수정: direct ordinary Axis MoveAbsolute/MoveRelative/MoveVelocity도 retained
  rebase bit로 선차단하지 않고 native LMC 결과를 반환함
- 2026-09-08 최종 운영 수정: Group Enable/PowerOn/motion/SetKin/Group Home Current도 동일하게
  native handler까지 전달함. 실제 axis/Group 오류는 사용자 Reset 또는 복구 대상으로 반환하며
  자동 Reset/Home/replay는 하지 않음
- Group Enable과 Group motion의 adapter-side power/lock/kinematic readiness 선차단도 제거하고
  연결된 native Group API 반환값을 그대로 성공/오류 판정에 사용함
- Power Off가 `PowerOff + Standstill`에 도달하면 alarm-only 상태를 terminal failure로 오분류하지 않음
- 새 Power On 예약을 Power Off safety-repeat로 오분류해 `ErrorId=-9`와 `RESERVED` 잔류를 만들던
  `HandleAxisOwnershipSafetyRepeat` 경로 수정
- 자동 재전송, 강제 owner record 삭제, identity 완화는 추가하지 않음

2026-09-07 current source 검증:

- Servo/Group native-dispatch source predicate: `83/83 PASS`
- rebase negative fixtures: `25/25 PASS`
- safety-repeat negative fixtures: `31/31 PASS`

PC Release tests are `1201/1201 PASS`. The focused WPF localization and legacy `-15`
display tests also return exit code 0. Full LASAL `-SourceOnly` currently stops at the pre-existing
compact identity/preemption self-test baseline header inventory/order drift, so it is not claimed as
a full static PASS.

이 결과는 source/static 결과다. 추가 helper 수정이 포함된 current image의 LASAL build/download와
실제 Servo On 성공을 증명하지 않는다. 과거 BootId 137 시험은 수정 전 실패 원인 확인 증거이며,
current success evidence로 재사용하지 않는다.

상세 원인과 변경 경계는
`../../architecture/LASAL_SERVO_POWER_LIFECYCLE_FIX_2026-09-03.md`를 따른다.

### 2.2 EtherCAT testbed hardware update

`c6bda1e`가 다음 generated/project artifact를 변경했다.

- `Elmo_11.SlaveIndex`: `1 -> 0`
- `Elmo_21.SlaveIndex`: `2 -> 1`
- `GL_9086_11.SlaveIndex`: `0 -> DEACTIVATED_LSL`
- `Eni.xml`, EtherCAT Network, `Classes.lcb`, project `.lcb` 갱신

current physical application contract는 계속 Axis1/Axis2, mask `0x00000003`이다. 그러나 기존
`TOPO-C0` verifier는 physical mask와 Motion/SimulationSetup 연결을 검사할 뿐 위 EtherCAT slave index와
deactivated GL_9086 상태의 PLC runtime 효과까지 증명하지 않는다.

따라서 다음을 별도 확인해야 한다.

- Elmo Drive1/2가 실제 slave index 0/1과 일치하는지
- GL_9086가 없어도 `LMCEcatInputLatch`의 optional Coupler/InputSlot/OutputSlot 상태가 Drive1/2 startup
  readiness를 잘못 막지 않는지
- 새 Eni/Network/project artifact가 같은 C78 build와 같은 PLC download에 포함됐는지

커밋에 `.lcb`가 포함된 사실만으로 compile/link/download 성공을 판정하지 않는다.

### 2.3 P0-0 stale Diagnostics topology 수정

2026-09-07 read-only live probe에서 다음 불일치를 확인했다.

- PLC identity: `Build=0x00000001`, `BootId=0x00000005`, `MapRevision=0x957F101E`
- Admin: `PhysicalAxisCount=2`, feature mask `0x00000757`
- Diagnostics topology: revision `0x15867EEC`, 7 entries, slave 5, slot 2, physical axis 4
- generated/current EtherCAT: Elmo 2 slaves, index 0/1, GL/Elmo3/Elmo4 deactivated

current working tree에서 다음을 수정했다.

- `LMCDiagnosticsService` topology revision을 canonical two-entry CRC `0x96FC461C`로 변경
- static inventory를 `Elmo_11`, `Elmo_21` 두 entry로 제한
- master slave index를 0/1, SDO/physical axis reference를 1/2로 유지
- removed CREVIS node/slot, Axis3/4 health lookup, CREVIS digital-I/O reference를 노출하지 않음
- `Verify-CurrentPhysicalTopology.ps1`을 ENI/network/generated-table/serializer까지 확장
- C# topology golden/download fixture와 LASAL source verifier를 two-drive contract로 갱신

현재 검증 결과:

- TOPO-C0 source/static: `184/184 PASS`
- C# library/tests Release build: PASS
- PC protocol/fake-RPC regression: `1201/1201 PASS`
- repository-wide LASAL SourceOnly: topology 도달 전 기존 compact identity/preemption self-test
  baseline drift에서 중단됨. 이번 topology 변경 실패로 분류하지 않는다.

이 수정 이후 LASAL Rebuild/Link/Download는 아직 수행하지 않았다. 따라서 위 BootId 5 live response와
10:10 build / 10:44 download log는 **수정 전 image evidence**이며 새 source의 PLC runtime 증거가 아니다.

## 3. current source 상태

| 영역 | current source 판정 | 2026-09-07 확인 | 아직 증명하지 않은 것 |
|---|---|---|---|
| Axis Power `0x2023` | 수정 반영 / Active candidate | 133 + 39 + 31 static fixtures PASS | current image Servo On/Off, fault/restart matrix |
| Topology / SimulationSetup | two-drive static inventory 수정 / Axis1/2 physical, Axis3..9 simulation | TOPO-C0 static `184 PASS`; PC `1201/1201 PASS` | 수정 후 C78, download, cold boot, live topology `0x96FC461C`, actual slave 0/1 |
| HomeDS402 `0x7D15/16/17` | Method 37 source/UI gates ON | current-dev top-level `18 PASS`; activation `46`, ownership `21`, size `10`, WPF recovery `36` | current image PLC terminal, actual-position zero, Axis1/2 hardware matrix |
| LMC Home `0x7D13/18/19` | Active / no-motion current-position-zero | 기존 source contract 유지 | Servo On을 수행하는 기능이 아님 |
| Generic SDO Write | software implementation complete / Limited | 기존 1/2/4-byte, journal, no-replay contract 유지 | current testbed physical write/readback matrix |
| SetOperationMode | implementation complete / Active | 기존 frozen implementation 유지 | 전체 hardware/fault/release matrix |
| SetPosition `0x7D12/14/1A` | Dormant | SP-C0 current inventory `39 PASS` | durable backend, native exactly-once execution, activation |
| HomeDS402Ex | Dormant | scaffold/profile preparation 존재 | moving homing runtime과 physical qualification |

HomeDS402는 더 이상 `five gates OFF` 상태가 아니다. current source의 아래 값은 모두 ON이다.

- TCP `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED=TRUE`
- Control `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED=TRUE`
- Diagnostics `LMC_DIAG_DS402_HOME_ENABLED=TRUE`
- InputLatch `LMC_DS402_HOME_STARTUP_SWEEP_ENABLED=TRUE`
- Admin capability mask `0x00000757`의 HomeDS402 bit 6 ON

SetPosition은 shared ordinary ownership이 ON이어도 활성 상태가 아니다. 아래 독립 gate가 계속 닫혀 있다.

- `LMC_ADMIN_SET_POSITION_STORE_CONFIGURED=FALSE`
- Axis1..4 `SetPositionMaxJump=0`
- Admin capability의 SetPosition bits 3/5/7 OFF
- authorized native `.SetPosition()` 실행 경로 없음

## 4. P0-0 — current image baseline closure

### 4.1 PC/source 재현 기준

아래 결과를 current HEAD에서 유지한다.

```powershell
tools/Verify-CurrentPhysicalTopology.ps1
tools/Verify-HomeDs402H37CurrentDevRegression.ps1
tools/Verify-SetPositionCurrentSourceInventory.ps1
LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalServoPowerLifecycle.ps1 `
  -IncludeRebaseContractSelfTest -IncludeSafetyRepeatContractSelfTest
```

2026-09-07 current working tree 결과는 각각 `184 PASS`, top-level `18 PASS`, `39 PASS`,
`133 + 39 + 31 PASS`다.

### 4.2 LASAL IDE/artifact

current exact HEAD를 열어 다음을 확인한다.

1. C78 Rebuild/Link 0 errors
2. Object Network Server/Client 연결과 일반 method source를 AGENTS.md 규칙대로 각각 확인
3. smoke 시작 이후 `%TEMP%\Lasal2.log`에 새 `CInvalidArgException` 없음
4. generated EtherCAT table에서 Elmo slave index `0/1`, GL_9086 deactivated 확인
5. source와 `Classes.lcb`가 같은 작업 결과인지 기록
6. download 전 source SHA / artifact SHA / build time 기록

2026-09-07 pre-topology-fix repository snapshot:

- `Classes.lcb`: `C93C7E3FF9FA789393C82C190D83E5EE8761547AE261EA1B9AAC1C13B6EEFE3E`
- project `.lcb`: `5890D44409866ED0F821D20AF04F2024826C7D8431D615AE350408338737CD61`
- EtherCAT generated table: `77286438EACB34FD9B953455720D1C078704B35164C1445D961C186D5152DA4F`
- `LMCControlCommandService.st`: `09FDA146AF727727CCF3412E45AB679F1631267CAB8A778172258DD6F0CDA65D`

이 hash는 수정 전 repository snapshot 식별용이다. 현재 `LMCDiagnosticsService.st`가 변경됐으므로
새 Rebuild에서 생성되는 artifact hash와 반드시 다시 묶어야 한다.

### 4.3 PLC load/runtime

1. current working tree C78 Rebuild/Link 0 errors
2. exact image download 및 PLC restart
3. 새 `DiagnosticsBuild / BootId / MapRevision` 기록
4. static topology revision `0x96FC461C`, total/slave/slot/physical=`2/2/0/2` 확인
5. EtherCAT master operational 및 Elmo slave 0/1 operational 확인
6. Axis1/2 physical startup proof 유효 확인
7. Axis3/4 absence가 startup과 ownership을 막지 않는지 확인
8. global quarantine `OwnershipState[24]=0` 확인
9. Axis1/2 owner가 명령 전 `IDLE`인지 확인

이 단계 전에는 이전 BootId, 이전 Watch, 이전 WPF terminal record를 current proof로 사용하지 않는다.

## 5. P0-1 — Servo Power physical qualification

축을 안전하게 정지시킨 뒤 Axis1부터 수행한다.

1. Power On 최초 요청
2. positive ACK와 stable `PowerOn` 3회 확인
3. owner state가 `RESERVED`에 남지 않고 정상 terminal/release되는지 확인
4. Power Off 최초 요청
5. `PowerOff + Standstill` stable 확인
6. 동일 요청 재전송 없이 read-only status로 결과 확인
7. Axis2 동일 시험

실패 시 최소 evidence:

- request/response ID, HeaderStatus, CommandStatus, ErrorId
- `OwnershipState[3]`, `[24]`, Axis별 owner record, observer record
- current Build/Boot/Map
- LASAL log의 command 시각 전후 구간

합격 기준은 UI의 ACK만이 아니라 PLC stable state와 실제 drive enable/disable이 모두 일치하는 것이다.

## 6. P0-2 — HomeDS402 Method 37 completion

Servo Power가 먼저 통과한 동일 image에서 수행한다.

1. Capability refresh 후 `AxisDs402Home` 광고 확인
2. Axis1 정확한 정지, actual position/raw unit 기록
3. Method 37 Start를 한 번만 전송
4. ReadOutcome으로 terminal 확인; Start 자동 재전송 금지
5. actual position이 0으로 정의된 결과 확인
6. Retire 후 owner와 durable recovery record 정리 확인
7. Axis2 반복
8. Axis3/4는 physical-unavailable/invalid-reference로 deterministic reject되는지 확인
9. timeout/disconnect/response-loss 시 원 Home 명령 no-replay 확인

Method 37은 현재 위치를 0으로 만드는 no-motion homing이다. Servo On을 대신하지 않으며 home switch를
찾아 이동하지 않는다. 이동형 homing은 HomeDS402Ex 범위다.

P0-2가 실패하면 관측된 실패를 수정한다. P0-2가 통과하면 HomeDS402 state machine을 다시 작성하지 않는다.

## 7. P0-3 — Generic SDO physical completion

동일 image/session에서 Axis1/2 safe target의 1/2/4-byte baseline read, pre-write guard, one-shot write,
exact readback과 disconnect/timeout no-replay를 닫는다. 이 작업은 신규 runtime 구현보다 qualification이다.
실패가 재현될 때만 해당 producer/sender/recovery 경로를 수정한다.

## 8. P1 — SetPosition 다음 구현 시작점

### P1-0 / SP-C1 external prerequisite capture

다음 두 자료를 먼저 확보한다.

1. vendor `CheckSum.CRC32`의 입력 bytes, seed/final 처리, expected CRC가 포함된 golden fixture
2. LASAL IDE가 생성한 `_FileSys` class/client/channel 선언과 최소 open/read/write/flush/replace 동작 ABI

확보 전 금지:

- CRC 알고리즘 추측 구현
- `_FileSys` ABI hand-authoring
- Store configured gate ON
- SetPosition capability/max-jump activation
- native position 변경 실행 추가

### P1-1 / SP-C2 first code tranche

SP-C1이 닫힌 뒤 첫 신규 코드 tranche는 `LMCSetPositionStore`의 fixed dual-file A/B durable backend다.

최소 범위:

- frozen 336-UDINT / 1344-byte canonical ledger 유지
- inactive bank write -> flush -> readback/CRC verify -> active selection
- torn write와 한쪽 bank corruption 복구
- identity mismatch fail-closed
- 아직 native `.SetPosition()` 호출 및 capability activation은 하지 않음

이후 순서는 기존 `SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`의
SP-C3 Store adapter -> SP-C4 RT claim-before-native exactly-once -> SP-C5 terminal-before-release ->
SP-C6 WPF recovery -> SP-C7 C78 -> SP-C8 hardware -> SP-C9 paired activation을 따른다.

## 9. 보류 항목

- HomeDS402Ex physical runtime은 P0와 SetPosition P1보다 뒤에 둔다.
- Digital Output Write와 기타 dormant diagnostics surface는 현재 축 mutation completion보다 뒤에 둔다.
- testbed 변경으로 physical drive 수가 다시 바뀌면 feature 개발 전에 TOPO-C0부터 재수행한다.

## 10. 완료 판정 경계

- 현재 확정: source/static regression PASS
- 현재 미확정: latest C78 build/link, exact PLC load, Servo Power physical success, HomeDS402 physical success
- HomeDS402 implementation 판정: P0-0~P0-2 동일 image evidence 완료 후
- SetPosition 구현 착수: SP-C1 external evidence 확보 후
- 전체 production release: 별도 정상/fault/timeout/disconnect/restart 및 distribution gate 완료 후

따라서 현재 전체 판정은 **SOURCE/STATIC PASS / CURRENT PLC-HARDWARE INCONCLUSIVE / PRODUCTION NO-GO**다.

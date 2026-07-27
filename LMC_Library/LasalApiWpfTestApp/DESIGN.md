# LASAL Motion Control API Example 설계

## 1. 목적

기존 WPF 화면의 레이아웃 장점은 유지하되, legacy transport와 simulation 동작을
제거하고 현재 `LasalMotionControlLib`가 PLC와 실제로 교환하는 기능만 제공한다.
처음 사용하는 개발자가 연결, 축 조회, 저속 단축 시험과 그룹 상태 확인 순서를
화면만 보고 이해할 수 있어야 한다.

## 2. 화면 범위

- 상단 Connection: PLC IP/port, PC local IPv4, callback UDP port, Connect/Close
- Single Axis: object lookup, Power/Reset/Stop, status/position, 3가지 motion
- Group Motion: object/member 조회, Power On/Off, profile Lock/Unlock,
  Reset/Stop, status/position, static 4축 absolute/relative Move Linear와 identity configuration
- EtherCAT / PI: capability, master/slave health, active signal Catalog와 typed PI value
- Bulk Snapshot: 선택 signal의 same-cycle snapshot, entry status와 raw value
- Recorder: capability-gated Single/Ring/Double 및 Manual/Edge/Window/Mask configuration,
  start/stop/status/header, reconnect adoption, chunk download, CSV와 dependency-free
  downsample plot
- SDO/Write Policy: general-inline SDO Read ticket submit/status/queued cancel,
  nonzero ObjectIndex, 임의 U8 SubIndex와 typed 1/2/4-byte inline 결과 표시/save,
  capability 및 write allowlist로
  차단되는 PI/SDO Write와 extended result 확인
- Read-only API: Admin capability를 선행 확인한 뒤 physical axis 1~4의 semantic
  parameter, fixed group `0x0100` parameter, typed drive operation mode와 non-atomic
  drive status를 읽는 Phase 1 실기 검증 화면
- Qualification: Group Enable accepted-then-locked, true Buffered A/B, deterministic
  Stop-first, 24-entry Bulk snapshot/lifecycle soak, Recorder Single/Ring/trigger
  lifecycle과 reconnect exact/0/0 discovery, read-only D5 SDO abort -> recovery를
  공통 상태·progress·구조화 로그로 실행하는 반복 시험 화면
- Execution Log: connection state, response 결과와 raw callback diagnostic

Motion command는 현재 PLC 활성 경로만 노출한다. Diagnostics command는 SDK surface를
노출하되 `GetCapabilities` 결과로 기능별 버튼을 fail-closed한다. 따라서 PC build에
화면이 포함돼도 PLC가 bit를 광고하지 않으면 실행할 수 없다.

## 3. 실제 API 연결

WPF 프로젝트는 `../LMC_API_Delivery/src/LasalMotionControlLib.csproj`를
`ProjectReference`로 직접 참조한다. command ID, frame offset과 response parser의
기준은 공용 API 소스 하나다.

축과 그룹 object는 이름 lookup으로 얻은 reference를 보관한다. 연결을 닫거나
재연결하면 기존 object를 즉시 폐기하고 다시 Load해야 한다.

Diagnostics Catalog와 Bulk/Recorder handle도 connection session에 귀속된다. Close나
reconnect 시 UI의 live handle은 폐기한다. 단, Recorder Start에서 받은
`DiagnosticsBootId + RecordId + BufferId` 텍스트는 reconnect 뒤 adoption 시험을 위해
보존한다. 같은 PLC boot에서 Capabilities를 다시 읽고 `AdoptRecorderAsync`로 새
connection 소유 identity를 만든 뒤 Status 또는 Header를 읽어 configuration metadata를
복구한다. 활성 connection 안에서는 Catalog reload 전에 Bulk/Recorder resource를 먼저
release해야 한다.

Recorder plot은 외부 chart package를 사용하지 않는다. downloaded immutable raw
buffer에서 화면 폭에 맞게 sample을 downsample하고 WPF `Canvas/Polyline`으로 그린다.
CSV export와 plot은 PLC live object를 다시 읽지 않는다. `Cancel Download`는 PC-side
chunk download token만 취소하며 recorder stop/release를 대신하지 않는다.

Recorder Trigger는 Configure payload의 signal/operator/value/mask 조건을 RT recorder가
판정한다. `Trigger Now`는 `TriggerRecorderAsync(0x7E42)`를 호출해 locally configured
non-Manual D4 recorder를 명시적으로 trigger한다. Adopt identity에는 configuration
shape가 없으므로 사용하지 않는다. Ring은 trigger capture에만 사용하고 Double은 PLC가
해당 capability를 광고할 때만 선택할 수 있다.

Window trigger의 기존 wire 필드는 `TriggerValue=lower bound`,
`TriggerMask=upper bound`로 해석한다. Window signal은 Int16/UInt16/Int32/UInt32로
제한하고 signed type은 signed ordering으로 `lower <= upper`를 검사한다. Edge는
TriggerMask를 항상 0으로 보내고 Mask는 BitField16/32와 non-zero TriggerMask를
요구해 세 경로의 의미를 섞지 않는다.

현재 SDO UI는 slave 1~4, nonzero ObjectIndex, 임의 U8 SubIndex와 ValueType에 정확히
맞는 1/2/4-byte Read를 제출한다. 활성화에는 bit 8 `SDORead`와 bit 13
`SDOReadGeneralInline`이 모두 필요하다.
`GetOperationStatusAsync`의 terminal `ResultData`를 raw bytes로 표시하고 저장한다.
SDK와 WPF에 extended result parser/download scaffold가 있더라도 current inline policy와
PLC capability가 8/12-byte 및 `0x7E51` 경로를 차단하므로 현재 화면 계약에 포함하지
않는다. SDO Write와 PI Write도 non-empty SDK/PLC allowlist가 승인되기 전까지
fail-closed한다. Phase 1 PI Write는 SDK compile-time allowlist가 empty인 것에 더해
`Phase1AllowsPiWrite=false`가 input/button을 비활성화하고 click handler도 다시 거부한다.

Read-only API 탭의 Admin 흐름은 `GetAdminCapabilitiesAsync(0x7D00)` 성공 결과를
connection-local UI cache로 보관한 뒤에만 axis/group 버튼을 연다. capability refresh를
시작하면 기존 cache를 먼저 폐기하고, 실패한 응답 뒤 stale capability로 read를 계속하지
않는다. axis parameter는 physical reference 1~4와 6개 semantic key만, group parameter는
reference `0x0100`과 3개 key의 선택 mask만 허용한다. Close/reconnect에서는 capability
cache와 표시 결과를 모두 지운다.

Drive read는 선택한 physical reference와 현재 loaded axis가 일치하면 그 handle을
재사용하고, 아니면 `_LMCAxisN`을 새 session에서 lookup한 뒤 반환 reference를 다시
검증한다. `ReadDriveStatusAsync`는 LASAL axis status, DS402 `0x6041:0`, operation mode
`0x6061:0`을 순차 실행하므로 atomic same-cycle snapshot으로 표시하거나 해석하지 않는다.
이 탭에는 motion/write control을 추가하지 않는다. Admin `0x7D00/10/20`은 LASAL IDE
build/download가 일치해야 한다. 화면은 2026-07-23 happy-path PASS와 아직 남은
invalid/stale/fault 검증 경계를 함께 명시한다.

## 4. UNIT 규칙

API 입력은 LASAL internal DINT다. Axis와 Group 화면은 숫자 배율을 직접
입력하지 않고 application UNIT 콤보에서 변환 방식을 선택한다. 기본 선택은
현재 PLC 축 설정과 같은 `mm (x10000)`이다.

```csharp
var raw = checked((int)Math.Round(
    engineeringValue * unitMultiplier,
    MidpointRounding.AwayFromZero));
```

- 선택 가능한 application UNIT은 `mm`, `m`, `deg`다. 이 화면은 하나의 축
  application UNIT을 모든 motion 인자에 공통 적용하므로 `RPM`, force, time,
  memory UNIT은 노출하지 않는다.
- `None / raw DINT`는 배율 1의 engineering unit이 아니다. 이미 변환된 정수
  DINT를 그대로 송수신하는 모드이며 소수 입력은 거부한다.
- NaN, Infinity와 DINT 범위 초과는 송신 전에 거부한다. 선택 UNIT이 있으면
  actual position은 `raw / UNIT`, Raw 모드이면 raw DINT만 표시한다.
- `mm (x10000)`은 PC application UNIT이다. 현재 Git 추적 축 transmission은
  `ExUnits=8388608`, `IntUnits=1 mm(10000)`이며 두
  설정을 같은 값으로 취급하지 않는다.
- `117440512 DINT`는 `mm` 선택에서 `11744.0512`, Raw 선택에서
  `117440512`로 입력한다. 이 변환 가능 범위와 PLC/장비 motion limit는 별개다.
- Absolute/Relative는 `Shortest`, Relative 방향은 distance 부호를 사용한다.
- Velocity는 Positive/Negative만 사용하고 deceleration 인자는 0으로 보낸다.
  제어 감속은 Stop 입력으로 전달한다.
- velocity, acceleration, deceleration은 0보다 커야 한다. UNIT 변환 후
  1 DINT count 미만이 되는 양수도 송신 전에 거부한다.
- Jerk 화면값은 `_LMCAxis` 입력 단위인 `axis application unit/s^3/1000`이며
  `Jerk DINT = 화면값 x UNIT`으로 변환한다. 물리 jerk를 직접 알고 있으면 먼저
  `1000`으로 나눈 값을 화면에 입력한다. `0`은 허용하고 음수는 거부한다.
- 현재 저장된 축 설정은 `_JERK_PROFILE`, `JMax=75000 mm`다. 실제 시험에서는
  다운로드된 PLC 설정과 장비 제한을 별도로 확인한다.
- Group position/dynamics도 별도 Group UNIT 콤보로 같은 DINT 변환을 수행한다.
  Read Position은 static member-slot alias인 `None/ACS`를 허용한다. Move Linear
  Absolute는 X/Y/Z/U를 target으로, Relative는 같은 입력을 delta로 해석하며 두 motion
  경로는 coordinate `None`만 허용한다. ACS 선택 중에는 Move 버튼을 비활성화한다.
- Relative는 Admin `0x7D22` capability가 없는 PLC에서 API가 송신 전에 거부한다.
  PLC가 `MoveRelativeCoord`를 직접 호출하므로 UI는 현재 위치를 합산해 absolute target을
  만들지 않는다. Admin ACK는 queue 수락이며 완료는 기존 Group InPosition monitor다.
- Group finite-motion monitor timeout은 첫 네 축의 absolute distance 합과
  velocity/acceleration/deceleration으로 보수적으로 계산하고 25% 및 5초 여유를 더한 뒤
  15~600초로 제한한다. timeout 뒤에도 motion-uncertain 상태와 Group Stop 경로는 유지한다.
- group Jerk 입력도 `group application unit/s^3/1000` 값으로 보고 UNIT을 곱한다.
  canonical `_LMCRobotBase1`은 `_JERK_PROFILE`, `JMax=50000 mm`다.

## 5. 안전 상태

- Power On, Reset, motion과 Group Power/Configure/Lock 명령은 arm 체크박스와
  확인창 없이 버튼 클릭 시 입력 및 상태 검사를 통과하면 즉시 송신한다.
- Group 준비 순서는 `1 Power On -> 2 Read Status의 Power Ready/ACTIVE 확인 ->
  3 Set Identity -> 4 Enable(Lock Profile) -> 5 Read Status의
  Enabled/Locked Standby 확인 -> 6 Move`다. 종료는
  `Disable(Unlock Profile) -> 7 Power Off -> Verify Power Off (Read Status)에서
  PowerOn=False 확인`
  순서다.
- Group Power On/Off 응답은 mode-change 요청 수락만 뜻한다. 화면은 Read
  Status에서 프로젝트 로컬 확장 `0x00040000` Power Ready를 확인한 뒤에만
  identity configuration 버튼을 활성화하고, Power Off 뒤에는 같은 비트가
  해제될 때까지 다른 group 준비 명령과 Read Position을 막고 Read Status 버튼을
  Verify Power Off로 표시/focus한다. `0x00010000`은
  Disabled/Unlocked, `0x00020000`은 Enabled/Locked Standby로 표시한다.
- Group Enable/Disable은 power가 아니라 configured profile Lock/Unlock이다.
  Enable ACK만으로 lock 완료를 판정하지 않는다. `Read Status`에서
  `0x00020000` Enabled/Locked Standby를 확인한 뒤에만 Move를 활성화한다.
  현재 추적 PLC `0x2047` handler는 `LockProfile`의 `_LMCPROF_NoError`를 request
  acceptance로 ACK하고 같은 cycle의 `LockState`를 완료 판정에 사용하지 않는다.
  최종 완료는 PC가 `0x2045`를 poll해 판단하는 accepted-then-poll 계약이다. 이 변경은
  source/static contract에는 반영됐지만 새 LASAL build/download와 실물 capture로 아직
  재검증하지 않았다.
  Status가 Disabled/Unlocked를 3회 연속 보고하면 lock 대기를 해제해 Enable
  재시도를 허용한다. Status 조회가 실패하면 local Power Ready와 lock 판정을
  무효화하되 진행 중인 lock 확인은 보존하고, 다음 성공한 Status 조회 전에는
  Power On과 Move를 막는다. `PowerOn=False`가 확인되면 identity도 지운다.
- Group Disable은 motion stop 명령이 아니다. UI는 local motion-uncertain 상태에서
  버튼을 막고, PLC handler가 실제 `ProfileInPosition`을 확인한 뒤에만 unlock한다.
- Stop과 Power Off는 확인창 없이 실행하며 유한 motion 및 standstill 감시 중에도
  사용할 수 있다. 다른 safety 송신 또는 연결 전환이 진행 중인 짧은 구간에는
  중복 송신을 막는다.
- Group Stop도 확인창 없이 실행하고 group motion 감시 중 사용할 수 있다. ACK는
  정지 완료가 아니므로 stable Group InPosition을 다시 확인한다. PLC의
  `StopMove(Mode:=3)`은 기존 profile buffer를 폐기하며, 정지 뒤 새 Move를 금지하는
  명령이 아니다.
- Relative move도 absolute와 같은 motion-uncertain tracking을 사용한다. valid Admin
  rejection만 local tracking을 해제하고 timeout, malformed response 또는 연결 손실은
  상태가 불확실하므로 Stop/PowerOff recovery 경로를 유지한다.
- Move Linear 응답 `ErrorId=7`은 `_LMCPROF_SWE_ERROR`다. 예제는 송신 직전의
  X/Y/Z/U `StartRaw`, `TargetRaw`와 dynamics를 로그에 남기고, runtime software
  end position 위반임을 명시한다. 어느 축이 위반했는지는 현재 wire 응답에
  `SubErrorNo`가 없으므로 LASAL의 `AxReadSWEndPos`와 `ReadProfileError()`로 확인한다.
- Group Reset은 axis/hardware error reset이다. robot profile error 전체 reset으로
  간주하지 않고 Group Read Status의 state/error를 확인한다.
- Stop은 position, velocity, acceleration 입력을 읽지 않고 Stop deceleration과
  Jerk만 변환한다. 다른 motion 입력의 오타가 Stop을 막지 않아야 한다.
- Stop은 Standstill/InPosition 3회, Power Off는 PowerOn=false 3회를 확인해야 안전
  확인을 통과한다. 한 번의 상태 sample만으로 정지 또는 전원 차단 완료를 판단하지 않는다.
- motion 전에 Read Status로 PowerOn을 확인한다.
- 유한 motion은 ACK 뒤 non-standstill을 관측한 후 stable standstill 3회를 확인한다.
  대기 중에도 Stop과 Power Off는 실행할 수 있다.
- motion 송신 직전부터 결과를 모르는 상태로 추적한다. 정상 거부 또는
  Stop/PowerOff/Read Status에서 Standstill 3회 확인 전에는 Close/Reconnect를 막는다.
- Stop/Power Off 요청이 motion 선행 상태 조회 중 들어오면 safety generation을
  변경한다. live command와 Stop/Power Off는 같은 app-level send gate를 사용하고,
  live command는 gate 안에서 generation을 다시 확인한다. 따라서 아직 송신되지
  않은 motion은 취소되고 이미 송신된 motion 뒤에는 Stop/Power Off가 전송된다.
- motion 가능성이 남아 있는 동안 UNIT, 위치, 속도, 가속도와 방향은 잠그고
  Stop deceleration과 Jerk만 수정할 수 있게 한다.
- 일반 motion Cancel 버튼은 제공하지 않는다. Qualification의 `Cancel Test`는 다음
  RPC 전 취소와 scenario cleanup을 요청할 뿐이며, in-flight RPC를 끊거나 Stop을
  대신하지 않는다.
- 창을 닫을 때 motion 가능성이 남아 있어도 확인창이나 자동 Stop 없이 종료한다.
  종료 직전 경고 로그만 남기며 실제 정지는 사용자와 외부 장치의 책임이다.

## 6. Qualification runner

### 6.1 공통 실행 계약

- runner는 한 번에 하나만 실행한다. ordinary operation, safety verification 또는 다른
  qualification이 실행 중이면 시작하지 않으며, 기존 `motionMayBeActive`가 남아 있어도
  차단한다.
- 공통 상태는 `BEGIN -> PASS/FAIL/SKIP/ABORTED`이며 progress와 최근 구조화 로그를
  Group/Bulk/Recorder 세 탭에 같이 표시한다. 로그 한 줄은 UTC, elapsed ms, run GUID,
  scenario, step과 assertion/cleanup field를 포함하고 사용자가 파일로 저장할 수 있다.
- `NotSupportedException`으로 판정한 capability 부재는 `SKIP`, 고정 Catalog/identity/
  limit 계약 불일치는 `FAIL`이다. Recorder의 명시적 capability/bank limit precheck는
  `SKIP`으로 분류한다. UI에 버튼이 있다는 사실만으로 PLC가 해당 기능을 지원한다고
  간주하지 않는다.
- 각 qualification wire dispatch는 공용 send gate를 얻은 뒤 runner 시작 시점의
  safety generation과 scenario token을 다시 확인한다. 송신을 시작한 단일 RPC에는
  `CancellationToken.None`을 넘겨 응답/timeout을 확정한다. Recorder download는 SDK의
  compound helper를 한 번에 호출하지 않고 header/chunk별 gate를 다시 얻어 chunk 사이
  취소와 safety 선점을 허용한다. Bulk Catalog public helper는 cancellation이 connection을
  강제 종료할 수 있으므로 `CancellationToken.None`으로 하나의 bounded compound
  operation을 완료한다. cleanup은 cancellation과 독립적이며 진행 중인 safety
  send/monitor를 먼저 통과시킨 뒤 같은 gate로 직렬화한다.
- 실행 중 기존 Axis/Group Stop 또는 Power Off는 계속 사용할 수 있다. 이 safety
  요청은 qualification token을 취소하며, Group motion scenario는 외부 Group Stop의
  stable InPosition 또는 Group Power Off의 PowerOn=False 3회를 확인한다. 확인에
  실패하면 자체 Group Stop cleanup으로 fallback한다.

### 6.2 Group qualification

- Enable scenario는 PowerOn + Disabled/Unlocked 3회 안정 preflight,
  `0x2047 ErrorId=0`, 이어지는 `0x2045` PowerOn + Locked Standby 3회를 요구한다.
  성공 상태는 profile locked이며 자동 Unlock/PowerOff하지 않는다. 취소도 lock
  transition을 되돌리는 명령이 아니므로 이후 Read Status와 명시적 Disable이 필요하다.
- Buffered scenario는 Admin capability와 fixed group reference, 4-member mapping,
  선택 축 software min/max, group velocity/acceleration limit, initial stable InPosition을
  확인한다. 첫 live slice는 동일 부호의 A/B, 각 delta 절대값 최대 1,000,000 raw,
  `Jerk=0`, `Coordinate=None`, `ExactStop`, `BufferMode=Buffered`로 제한한다.
  A의 non-InPosition을 관측한 뒤 B를 보내고 B ACK 직후에도 non-InPosition인지 확인한
  다음 `start + A + B` endpoint/tolerance와 stable InPosition을 검사한다. 성공 시
  Aborting relative command로 captured start에 복귀한다. motion 중 오류/취소는
  Group Stop + stable InPosition cleanup 대상이며 cleanup failure는 원 오류와 합쳐
  안전 상태 미확정 FAIL로 보고한다.
- Stop-first scenario는 실제 이동을 만들지 않는다. shared send gate를 보유한 채
  zero-delta Move task를 먼저 대기시키고 Stop task가 safety generation을 변경한 뒤
  gate를 연다. Move delegate invocation 0, local pre-transmission cancellation,
  Group Stop ACK와 stable Standby 3회를 요구한다. Stop/검증 실패 시 gate를 먼저 반환하고
  non-cancelable fallback Stop + stable Standby를 실행하며, fallback도 실패하면 primary와
  cleanup 오류를 aggregate한다. 이 assertion은 app ordering을
  증명하지만 실제 wire의 `0x7D22=0`, `0x2085=1`은 packet capture로 별도 확인한다.

### 6.3 Bulk qualification

- 시작 시 capability와 Catalog를 다시 읽고 stable nonzero DiagnosticsBootId,
  동일 MapRevision, CatalogEntryCount 24, BulkReadable 24개, MaxBulkSignals >= 24와
  전 entry InputMapped phase를 요구한다. manual Bulk/Recorder resource가 남아 있으면
  시작하지 않는다.
- Snapshot Soak는 revision-bound public builder에 24개를 Catalog 순서 그대로 넣고
  Active까지 최대 5초 bounded poll한다. 기본 100회, 10 ms 간격으로 읽으며 각 응답의
  Boot/config/map identity, entry count/stride/order/type, SameCycle + InputMapped flags,
  even SnapshotSequence, Partial=false, 모든 entry Valid/detail 0을 검사한다. cycle,
  timestamp와 sequence는 unsigned wrap-aware nondecreasing으로 검사하고 RPC latency
  min/avg/max와 cycle delta를 기록한다.
- Lifecycle Soak는 지정 횟수마다 새 builder로
  `Configure -> Active -> Snapshot -> Release`를 수행한다. 끝난 뒤 새 Configure/Active/
  Release가 다시 성공해야 하며, released reader의 두 번째 Release는 local
  `InvalidOperationException`으로 막혀 wire request가 없어야 한다.
- 모든 생성 reader는 `finally` Release 대상이다. cleanup 실패는 PASS로 숨기지 않는다.
- one-slave offline partial은 Group PowerOff/Disabled와 4축 actual-position 3회 동일값을
  checkpoint 직전에 확인한 뒤 프로그램이 fault를 만들지 않는 두 operator checkpoint로
  구현한다. baseline 24 Valid, 한 SourceIndex의 6개 `SlaveOffline` bit/Detail 18과
  나머지 18 exact Valid, 같은 slave 복구 뒤 24 Valid를 요구한다. offline 축의 status는
  PLC가 OR하는 추가 invalid bit를 허용하되 Valid bit는 금지하며, 첫 Partial에서 다른 축도
  invalid이면 즉시 실패한다.
  reconnect stale handle과 raw old revision/BootId rejection은 별도 내부 시험이다.

### 6.4 Recorder qualification

- fresh capability와 이미 load한 동일 MapRevision Catalog를 대조하고 Catalog 순서의
  첫 4개 Recordable signal을 고정 channel order로 사용한다. Single은
  RecorderSingleBank, Ring/soak는 RecorderTrigger도 요구한다. Double capability는
  표시만 하며 public qualification에서 사용하지 않는다.
- Single Manual은 SamplePeriod 1 cycle, capacity 1000이다. 자연
  `SampleCountComplete`와 1000 samples, zero dropped/overflow를 기다린 뒤 Header,
  Download A/B의 identity/revision/channel order, 16-byte stride, 16,000-byte data와
  raw SHA-256 일치를 검사한다. cleanup 후 buffer/configuration 각각의 두 번째 Release가
  local guard에서 막히는지도 검사한다.
- Ring forced trigger는 capacity 1000, pre 100/post 899, Edge와 자동 trigger가
  도달하지 않는 threshold를 사용한다. Recording에서 pre-history 100개 이상을 확인한
  뒤 `TriggerRecorderAsync`를 보내 `TriggerComplete`, TriggerIndex 100, 1000 samples,
  Header/data identity와 qualification per-RPC gated exact chunk coverage를 검사한다.
- Trigger Lifecycle Soak는 capacity 32, pre 16/post 15로 같은 forced-trigger lifecycle을
  기본 100회(입력 상한 1000회) 반복한다. 매 회 buffer를 먼저 terminal/frozen 상태로
  만든 뒤 buffer, configuration 순서로 Release하며 completed count, ResourceBusy,
  dropped/overflow를 집계한다. WPF가 기록하는 `rtEvidence=NOT_MEASURED_BY_WPF`처럼 이
  결과만으로 PLC RT jitter나 sample 무손실을 독립 증명하지 않는다.
- Reconnect qualification은 Start ACK 직후 BootId/RecordId/BufferId, old OwnerSessionEpoch,
  config/map revision과 signal order를 snapshot으로 보존하고, Ring Recorder가
  Recording/pre-history 상태임을 확인한 뒤 앱 RPC connection을 실제 close/reopen한다. fresh capability의
  same BootId/MapRevision을 확인한 뒤 exact와 single-bank 0/0 discovery를 별도 run으로
  실행한다. 반환 identity가 snapshot과 일치하고 OwnerSessionEpoch가 새 값인지 확인한 뒤
  Status -> 필요 시 Stop -> Header -> exact-coverage Download 순서로 검증한다. discovery가
  다른 active resource를 반환하면 Adopt 응답 identity 검증에서 즉시 실패하며 해당
  resource에 Status/Stop/Release를 보내지 않는다.
- cancellation/실패 cleanup은 active recorder에 Stop을 시도하고 final Status를 확인한다.
  buffer와 configuration handle의 자동 Release는 `Ready` 또는 이미 frozen download가
  시작된 `Uploading`에서만 수행한다. `Fault`는 releasable frozen state로 취급하지 않고
  identity/resource를 보존하며 recovery-required QTEST failure를 남긴다. reconnect
  close 전 transport fault를 포함한 cancellation/실패 cleanup은 실제 original connection
  상태와 보존 expectation을 기준으로 route를 선택하고, 필요하면 exact identity로
  connection/adoption을 복구한 뒤 adopted identity로 buffer와 configuration을 해제한다. 이후 명시적
  Status/error 진단과 수동 복구가 필요하다. 보존 ownership은 manual UI에서 quarantine하고
  Status 확인 전 mutation을 막는다. 확인 상태가 Armed/Recording이면 명시적 Release가
  Stop -> Ready/Uploading poll -> buffer/configuration Release를 수행하며 Fault/Empty는
  계속 보존한다. buffer/configuration 중 configuration만 남은 tail은 Status 없이 retry한다.
  external fault, BufferOverwritten와
  Double bank는 구현된 qualification scenario 밖이다.

### 6.5 D5 SDO abort -> recovery qualification

- runner는 SDO Read만 생성하며 write와 EtherCAT fault injection은 하지 않는다.
- 선택 slave와 `_LMCAxis1..4`를 일치시키고 `PowerOn=False`, `Standstill=True`, actual
  position 3회 동일값을 확인한 뒤 D5 ticket을 제출한다.
- 먼저 `0x6061:0 Int8/1` baseline을 읽는다. 사용자가 제조사 기준으로 선택한 존재하지 않는
  read-only object/subindex가 실제 abort를 반환한 뒤 같은 BootId/MapRevision의 새
  `0x6061:0` ticket이 baseline과 같은 값을 반환해야 한다.
- abort PASS 계약은 status RPC 성공, terminal `Failed/Failed`,
  `OperationErrorId=-32000`, `OperationDetail`의 nonzero raw EtherCAT SDO abort code와
  result 없음이다. local transport error, timeout과 cancel은 abort 증거가 아니다.
- cancel 요청 시 실제 `Queued` ticket만 public Cancel을 보낸다. 이미 `Running`이면 PLC
  Stop이나 transport close 없이 원래 terminal deadline의 남은 시간+1초를 반영한다.
  cleanup wait는 최소 15초, 최대 120초이며, 끝나지 않으면 ticket identity를 지우지 않은
  채 cleanup timeout으로 실패한다.
- Submit wire 호출 전에 ticket ID 0의 outcome evidence를 먼저 quarantine ledger에 넣는다.
  명시적 PLC command rejection이면 제거하고, 응답 유실/transport 예외면 unknown-ticket
  evidence로 보존한다. ticket 응답을 받았으면 active ticket/owner connection/deadline을
  먼저 저장한 뒤 outcome evidence 제거 성공을 확인한다.
- 모든 pending-ticket cleanup은 status/cancel 전에 같은 `LMCConnection`의 current capability
  BootId/MapRevision을 old ticket의 `DiagnosticsBootId`/`SubmissionMapRevision`과 먼저
  비교한다. 둘 중 하나가 변경됐거나 status가 exact `BootIdMismatch`면 old terminal을 추정하지 않고
  known ticket을 stale-session quarantine으로 이동한다. local ticket의 session generation이
  stale인 예외도 quarantine한다. 같은 Boot/session의 exact `TicketNotFound`는 terminal-slot
  교체 계약상 이전 ticket terminal만 증명하므로 `TERMINAL_INFERRED`, outcome `UNKNOWN`으로
  해당 ticket을 해제한다.
- quarantine은 known ticket과 submit-outcome unknown evidence를 여러 개 보존할 수 있다.
  모두 같은 slave여야 자동 recovery proof가 가능하다. stable BootId/MapRevision 아래 서로
  다른 두 ticket을 사용하되 GeneralInline capability면 `0x6061:0 Int8/1`, legacy
  SDORead-only면 `0x1000:0 UInt32/4`를 선택한다. 두 결과의 exact type/length/bytes가 같고 proof
  동안 evidence 목록이 불변일 때만 quarantine 전체를 해제한다. UI 독립
  `D5SdoRecoveryScopePolicy`는 owner reference+BootId+MapRevision 조합만으로 scope를 순수
  판정하며 MainWindow는 proof 시작 로그와 PASS 로그에 같은 decision을 사용한다.
  owner+BootId+MapRevision이
  동질인 경우에만 scope는 `same_owner_connection_recovery`,
  `new_diagnostics_identity_session`, `new_connection_session` 중 하나다. current owner와
  current identity를 모두 공유하면 첫 scope, current owner와 한 previous identity를 공유하면
  둘째 scope, 모두 current owner와 다르면서 한 previous owner+identity를 공유하면 셋째
  scope다. owner 또는 submission identity가 섞이면 `mixed_evidence_sessions`이며 same/new
  session 증거로 세지 않는다. mixed도 two-ticket application recovery proof와 성공 시
  quarantine clear는 허용한다. 첫 scope와 둘째 scope는 orphan PASS가 아니다. 한 previous
  owner+identity로 동질인 셋째 scope만 decision의 `NewConnectionRecovery=true`이고
  `newConnectionRecovery=true`로 기록한다. WPF는 항상
  `orphanQualified=false`를 기록한다. 이 scope는 새 RPC connection에서 application recovery가
  성립했다는 뜻일 뿐 PLC 내부 orphan cleanup이나 late callback을 증명하지 않는다. 실제
  orphan PASS에는 known Running old ticket, 실제 owner loss와 별도 PLC hook/capture가 필요하다.
  QTEST는 `evidenceBootIds`/`evidenceMapRevisions`,
  `recoveryBootId`/`recoveryMapRevision`, `proofScope`, `mapChangedEvidence`,
  `sameIdentityEvidence`, `mixedEvidenceSessions`, `newConnectionRecovery`,
  `orphanQualified=false`를 따로 기록한다.
- unresolved가 하나라도 있으면 Configure Bulk, Recorder Configure/Adopt/Start/Trigger,
  Group Disable, motion/PowerOn/Reset, manual SDO/PI, Close와 모든 다른 qualification 같은
  새 mutation을 차단한다. 기존 resource cleanup인 Bulk Release, Recorder Stop/Release와
  queued diagnostic Cancel, motion Stop/PowerOff 및 read-only는 허용한다. reconnect는 외부
  connection loss 뒤에만 허용한다. Resolve는 reconnect 없이 same-session/new-Boot proof에도
  사용하고, 외부 loss 후에는 새 connection proof에 사용한다.
- `D5SdoPendingCleanup` Resolve는 기존 `qualificationLogLines`를 clear하지 않고 append하며
  `D5_LOG_CONTINUATION`을 기록한다. 원래 `FAIL`/`OUTCOME_UNCERTAIN`과 resolution proof를
  같은 저장 QTEST log에 보존한다.
- manual SDO와 Drive read의 external tracker event는 마지막 qualification context에 붙이지
  않고 별도 `D5ExternalTracking:<stage>` run ID/step/elapsed context를 사용한다. unresolved
  상태에서는 이 원본 context를 유지하고 Resolve가 끝난 뒤에만 close한다.
- Phase 1 drive-read facade는 원래 exception type/stack을 그대로 다시 던지고
  `LMCDriveReadFailureContext.TryGet`으로 typed all-failure context를 제공한다. phase는
  `FacadePreflight`, `AxisStatusRead`, `CapabilityPreflight`, `Submission`, `StatusPolling`,
  `ResultMaterialization`의 6개이고, 각 SDO attempt의 `GenericSubmissionOutcome`은 공용
  `LMCSdoSubmissionOutcome`의 `NotAttempted`, `Rejected`, `OutcomeUncertain`, `Accepted`이다.
  기존 `SubmissionOutcome`/`LMCSdoReadSubmissionOutcome`은 호환용으로 같은 값을 유지한다.
  snapshot은 실제 capability의
  `DiagnosticsBootId`/`MapRevision`, ticket, 마지막 status를 불변 snapshot으로 보존한다.
  이전 attempt가 terminal이 아니면 다음 attempt를 만들 수 없다. WPF orchestrator는
  no-submit/rejected/accepted-terminal context의 guard를 해제하고, uncertain은 실제 Submit
  identity로 unknown evidence를 보정해 quarantine하며, accepted nonterminal은 exact ticket을
  보존하고 guard를 해제한다. context 누락, 둘 이상의 nonterminal ticket 또는 불일치 상태는
  fail-closed한다.
- 수동 `Submit SDO Read`가 직접 호출하는 `LMCDiagnostics.SubmitSdo[Async]`는 원래 exception에
  `LMCSdoSubmissionFailureContext`를 연결하며 `TryGet`으로 조회한다. phase는
  `RequestValidation`, `SessionPreflight`, `CapabilityPreflight`, `Submission`,
  `PostSubmissionValidation`의 5개이고 같은 `LMCSdoSubmissionOutcome`을 사용한다. dispatch된
  attempt는 실제 capability `DiagnosticsBootId`/`MapRevision`을 보존하고 accepted failure는
  같은 `DiagnosticsBootId`/`SubmissionMapRevision`을 가진 exact ticket을 보존한다. manual router는 no-submit/rejected를 disarm하고 uncertain identity를
  reconcile해 quarantine한다. accepted ticket은 이전 manual status/result/cancel flag를
  초기화하고 manual operation state와 D5 tracker 양쪽에 보존한 뒤 disarm하며, context
  누락/불일치는 fail-closed한다.
- D5 quarantine은 UI field의 mutable list가 아니라 `D5SdoQuarantineLedger`가 소유한다.
  owner-bound opaque handle, immutable evidence snapshot, entry/global revision과 exact-once
  disarm을 사용한다. accepted ticket은 `LMCOperationTicket.BelongsTo`로 owner connection을
  검증하고 ticket의 `DiagnosticsBootId`/`SubmissionMapRevision`을 actual BootId/MapRevision과
  exact match한 뒤 전이한다. recovery는 proof 자체의 두 임시 accepted
  guard는 허용하지만 persistent evidence 변경이나 candidate 이후 ABA를 거부하며, PASS log
  callback 성공과 clear를 같은 ledger lock에서 commit한다.

### 6.6 검증 경계

Qualification UI와 assertion/cleanup 코드는 구현돼 있고 C# build와 정적 계약으로
검사할 수 있다. 현행 Debug visual/startup smoke에서는 Group/Bulk/Recorder panel 렌더와
prerequisite 미충족 초기 실행 버튼 disabled를 확인했다. 이는 WPF 렌더와 fail-closed
gate 확인일 뿐이다. API Debug/Release는 각각 256/256 PASS다. 직전 249개에 UI 독립 D5
recovery scope policy 계약 시험 7개가 추가됐다. Group queue chaining/Stop-first wire
order, 수정된 `0x2047`,
Bulk 100회와 one-slave-offline partial/recovery, Recorder Single/Ring/soak/reconnect-adopt,
D5 abort/recovery는 해당 PLC build를 다운로드한 실물 장비에서
아직 실행·packet capture하지 않았다. 따라서 runner의 `PASS`와 지정 capture의 wire
조건을 모두 얻기 전에는 production qualification 완료로 표시하지 않는다.

## 7. callback 범위

Connect가 callback listener와 endpoint 등록까지 처리한다. 수신 payload는 시각,
remote endpoint, 길이와 최대 48-byte hex preview로 기록한다. PLC event sender와
typed callback payload가 정의되기 전에는 motion complete 신호로 해석하지 않는다.

## 8. 검증 기준

- Debug/Release solution rebuild
- `LasalMotionControlLib` project reference 출력 DLL 확인
- legacy transport와 제거 화면 class 참조가 신규 프로젝트에 남지 않았는지 정적 검색
- Jerk 입력 활성화, DINT 범위 검사와 Stop/Move API 전달 확인
- LASAL static contract에서 `_JERK_PROFILE`, nonzero JMax, Jerk 수신 offset과
  `_LMCAxis` 및 `_LMCRobotBase1` 전달 경로 확인
- Group Power On/Off, profile Lock/Unlock, Reset/Stop/Read Position,
  Move Linear Absolute/Relative 및 Set Identity Kinematics의 UI-to-API handler와 group InPosition
  monitor 확인
- 실제 실행 창과 모든 탭의 layout/accessibility smoke test
- diagnostics capability fail-closed 상태, Catalog selection, Bulk resource lifecycle,
  Recorder mode/trigger capability gate, Ready/Header gate, reconnect adoption,
  download progress/cancel, metadata CSV와 plot smoke test
- general-inline SDO Read ticket submit/status/queued cancel, terminal typed 1/2/4-byte
  inline result/save와 PI/SDO Write 및 extended result gate. live packet은 1/2/4-byte와
  동일 BootId TypeMismatch recovery까지 PASS했다. read-only abort/recovery runner와
  analyzer는 code/build/test 완료지만 PLC live/pcap과 나머지 fault matrix는 별도다.
- Read-only API의 Admin capability fail-closed, axis/group semantic allowlist,
  physical axis lookup/reference 검증과 drive status non-atomic 표기
- 실제 PLC 시험은 Read Status/Position부터 시작하고 motion은 마지막에 수행
- `MoveCircle`은 공개 API와 승인된 DINT wire 계약이 생기기 전까지 UI에 추가하지 않음

구현된 runtime qualification UI의 원 설계와 단계별 packet 합격 기준은
`../../docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md`를
따른다.

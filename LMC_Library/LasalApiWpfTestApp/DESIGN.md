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
  Reset/Stop, status/position, static 4축 Move Linear와 identity configuration
- Execution Log: connection state, response 결과와 raw callback diagnostic

현재 PLC 활성 경로가 아닌 기능은 UI, source와 mapping에 포함하지 않는다.
별도 시험 도구가 필요하면 이 예제에 다시 섞지 않고 독립 프로그램으로 만든다.

## 3. 실제 API 연결

WPF 프로젝트는 `../LMC_API_Delivery/src/LasalMotionControlLib.csproj`를
`ProjectReference`로 직접 참조한다. command ID, frame offset과 response parser의
기준은 공용 API 소스 하나다.

축과 그룹 object는 이름 lookup으로 얻은 reference를 보관한다. 연결을 닫거나
재연결하면 기존 object를 즉시 폐기하고 다시 Load해야 한다.

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
- `mm (x10000)`은 PC application UNIT이다. 현재 저장된 축 transmission은
  실제 `10 mm/rev` 기준 `ExUnits=8388608`, `IntUnits=10 mm(100000)`이며 두
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
  현재 Move Linear는 X/Y/Z/U 4개 값만 사용하고 coordinate는 `None`만 허용한다.
- group Jerk 입력도 `group application unit/s^3/1000` 값으로 보고 UNIT을 곱한다.
  canonical `_LMCRobotBase1`은 `_JERK_PROFILE`, `JMax=50000 mm`다.

## 5. 안전 상태

- Power On, Reset, motion과 Group Power/Configure/Lock 명령은 arm 체크박스와
  확인창 없이 버튼 클릭 시 입력 및 상태 검사를 통과하면 즉시 송신한다.
- Group 준비 순서는 `1 Power On -> 2 Read Status의 Power Ready/ACTIVE 확인 ->
  3 Set Identity -> 4 Enable(Lock Profile) -> 5 Read Status의
  Enabled/Locked Standby 확인 -> 6 Move`다. 종료는
  `Disable(Unlock Profile) -> Power Off -> Read Status에서 PowerOn=False 확인`
  순서다.
- Group Power On/Off 응답은 mode-change 요청 수락만 뜻한다. 화면은 Read
  Status에서 프로젝트 로컬 확장 `0x00040000` Power Ready를 확인한 뒤에만
  identity configuration 버튼을 활성화하고, Power Off 뒤에는 같은 비트가
  해제될 때까지 다른 group 준비 명령을 막는다. `0x00010000`은
  Disabled/Unlocked, `0x00020000`은 Enabled/Locked Standby로 표시한다.
- Group Enable/Disable은 power가 아니라 configured profile Lock/Unlock이다.
  Enable ACK만으로 lock 완료를 판정하지 않는다. `Read Status`에서
  `0x00020000` Enabled/Locked Standby를 확인한 뒤에만 Move를 활성화한다.
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
- Move Linear 응답 `ErrorId=7`은 `_LMCPROF_SWE_ERROR`다. 예제는 송신 직전의
  X/Y/Z/U `StartRaw`, `TargetRaw`와 dynamics를 로그에 남기고, runtime software
  end position 위반임을 명시한다. 어느 축이 위반했는지는 현재 wire 응답에
  `SubErrorNo`가 없으므로 LASAL의 `AxReadSWEndPos`와 `ReadProfileError()`로 확인한다.
- Group Reset은 axis/hardware error reset이다. robot profile error 전체 reset으로
  간주하지 않고 Group Read Status의 state/error를 확인한다.
- Stop은 position, velocity, acceleration 입력을 읽지 않고 Stop deceleration과
  Jerk만 변환한다. 다른 motion 입력의 오타가 Stop을 막지 않아야 한다.
- Stop은 Standstill 3회, Power Off는 PowerOn=false 뒤 Standstill 3회까지 확인해야
  안전 확인을 통과한다. PowerOn=false 하나만으로 정지 완료로 판단하지 않는다.
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
- Cancel 버튼은 제공하지 않는다. in-flight 취소는 Stop이 아니며 transport와
  PLC command 결과를 불명확하게 만들 수 있다.
- 창을 닫을 때 motion 가능성이 남아 있어도 확인창이나 자동 Stop 없이 종료한다.
  종료 직전 경고 로그만 남기며 실제 정지는 사용자와 외부 장치의 책임이다.

## 6. callback 범위

Connect가 callback listener와 endpoint 등록까지 처리한다. 수신 payload는 시각,
remote endpoint, 길이와 최대 48-byte hex preview로 기록한다. PLC event sender와
typed callback payload가 정의되기 전에는 motion complete 신호로 해석하지 않는다.

## 7. 검증 기준

- Debug/Release solution rebuild
- `LasalMotionControlLib` project reference 출력 DLL 확인
- legacy transport와 제거 화면 class 참조가 신규 프로젝트에 남지 않았는지 정적 검색
- Jerk 입력 활성화, DINT 범위 검사와 Stop/Move API 전달 확인
- LASAL static contract에서 `_JERK_PROFILE`, nonzero JMax, Jerk 수신 offset과
  `_LMCAxis` 및 `_LMCRobotBase1` 전달 경로 확인
- Group Power On/Off, profile Lock/Unlock, Reset/Stop/Read Position,
  Move Linear/Set Identity Kinematics의 UI-to-API handler와 group InPosition
  monitor 확인
- 실제 실행 창과 두 탭의 layout/accessibility smoke test
- 실제 PLC 시험은 Read Status/Position부터 시작하고 motion은 마지막에 수행
- `MoveCircle`은 공개 API와 승인된 DINT wire 계약이 생기기 전까지 UI에 추가하지 않음

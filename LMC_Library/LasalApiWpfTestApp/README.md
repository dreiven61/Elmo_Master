# LASAL Motion Control API Example

루트 `Codex_LASAL_WPF`의 기능을 현재 `LasalMotionControlLib` 기반의 간단한
실제 PLC 예제로 다시 구성한 프로젝트다. 기존 `LasalMotionControlLibTestApp`은
이 예제로 대체되어 제거했다.

## 빌드

Visual Studio 2019에서 `LasalApiWpfTestApp.sln`을 열고 `Debug|Any CPU` 또는
`Release|Any CPU`로 빌드한다. solution 표시는 Any CPU지만 실행 프로젝트의
`PlatformTarget`은 x64다. 출력 파일 이름은 `LasalMotionControlApiExample.exe`다.

프로젝트는 아래 공용 API 소스를 직접 참조한다.

```text
../LMC_API_Delivery/src/LasalMotionControlLib.csproj
```

## 권장 시험 순서

1. 장비의 physical E-stop, software limit와 이동 가능 범위를 확인한다.
2. PLC IP, PC local IPv4와 callback UDP port를 입력하고 Connect한다.
3. `_LMCAxis1` 같은 실제 LASAL object name을 입력하고 Load Axis를 누른다.
4. Read Status와 Read Position을 먼저 실행한다.
5. 대상 축을 다시 확인하고 Power On을 실행한다. 버튼 클릭 시 확인창 없이 즉시 송신된다.
6. 다시 Read Status로 PowerOn 상태를 확인한다.
7. UNIT과 작은 motion 값을 확인하고 Move Absolute/Relative를 시험한다.
8. Move Velocity는 마지막에 시험하고 반드시 Stop 또는 Power Off로 끝낸다.
9. Group 기능은 실제 `_LMCRobotBase1`이 PLC network에 연결된 경우에만 사용한다.
10. Load Group 뒤 Get Members와 Read Position으로 대상 구성을 먼저 확인한다.
11. `1 Power On`을 누른다. PASS는 mode-change 요청이 수락됐다는 뜻이며
    `_ROBOT_ACTIVE` 완료를 뜻하지 않는다.
12. `2 / 5 Read Status (Power Ready / Lock Ready)`를 반복해 `PowerOn=True`를 확인한다.
    프로젝트 로컬 확장 state `0x00040000`만 Power On 완료를 뜻한다.
    확인 전에는 Set Identity, profile lock과 Move 버튼이 활성화되지 않는다.
13. 네 축 이름을 확인하고 `Home Check (X/Y/Z/U)`로 각 축의
    `Home/Referenced=True`를 확인한다. 이 버튼은 진단용이며 생략해도 된다.
14. `3 Set Identity (Auto Home Check + Configure)`를 실행한다. Set Identity는
    같은 Home Check를 다시 수행하며, 한 축이라도 reference되지 않았으면 PLC에
    kinematics 설정 명령을 보내지 않는다.
15. `4 Enable (Lock Profile)`을 실행한다. PASS는 Lock API 성공이며 최종
    Locked/Standby 확인은 아니다.
16. `2 / 5 Read Status (Power Ready / Lock Ready)`를 다시 실행해
    `Enabled/LockedStandby=True`를 확인한다. 이 확인 뒤에만 Move가 활성화된다.
17. 작은 X/Y/Z/U 목표로 `6 Move Linear Absolute`를 시험한다.
18. 종료 순서는 Group Stop 및 InPosition 확인, `Disable (Unlock Profile)`,
    `Power Off`, `Read Status`에서 `PowerOn=False` 확인이다.

## Load Axis 실패 진단

`_LMCAxis1`이 network의 실제 object name인데도 Load Axis가 실패하면 Execution
Log의 lookup 응답을 확인한다.

- `HeaderStatus=1`, `CommandStatus=1`, `ErrorId=-2`: 해당 LASAL client/name
  registry entry가 아직 준비되지 않았거나 입력 이름이 실제 object name과 다르다.
- `FrameValid=False`: PLC 배포본과 PC API의 response framing이 다르거나 TCP
  응답이 잘렸다.
- `HeaderStatus=0`, `PayloadLength=6`, `Reference=0`: 현재 LASAL dispatcher가
  허용하지 않는 구형/잘못된 descriptor 응답이다.

LASAL Online Debugger에서는 Load Axis 요청 직후
`TCPMotionInterface1.AxisObjectName1`, `GroupObjectName`과 각 client 연결
상태를 확인한다. `ObjectRegistryReady`는 Get Group Members 요청 때만 9축과
group entry가 모두 유효한지를 나타낸다. 축 5~9 이름도 CodeGenerator에 등록된
`AxisObjectName1`을 순차 scratch buffer로 사용한다. PC API와 LASAL 소스를 수정한 뒤에는 둘 다
rebuild하고 PLC에 최신 프로그램을 다시 download해야 한다.

LASAL runtime 생성 테이블은 object symbol을 대문자로 저장한다. 현재
dispatcher는 대소문자를 구분하지 않으므로 `_LMCAxis1`과 `_LMCAXIS1`,
`_LMCRobotBase1`과 `_LMCROBOTBASE1`을 각각 같은 이름으로 처리한다. 이 수정이
반영되지 않은 이전 PLC 배포본에서는 임시 확인용으로 전체 대문자 이름을 입력한다.

## 중요한 규칙

- DLL은 UNIT을 곱하거나 나누지 않는다. 이 예제의 Axis/Group UNIT 콤보가
  송신 전에 호출자 측 변환 방식을 선택한다.
- 기본 선택 `mm (x10000)`은 현재 저장된 `_LMCAxis1..4`의 `1 mm` macro와
  일치한다. `8,388,608`은 encoder 측 `ExUnits`이며 PC API UNIT이 아니다.
- `None / raw DINT`를 선택하면 정수 입력을 변환 없이 보낸다. 예를 들어
  `117440512`를 입력하면 같은 DINT가 전송된다. `mm` 선택에서 같은 값을
  전송하려면 `11744.0512`를 입력한다. Raw 모드는 소수 입력을 거부한다.
- UNIT 콤보는 PC 변환만 바꾼다. PLC의 software limit, MaxModulo, DS402 범위나
  실제 장비의 허용 이동 범위를 변경하지 않는다.
- 현재 Git 추적 PLC transmission은 `ExUnits=8388608`,
  `IntUnits=1 mm(10000)`다. offset 0 기준 external signed-DINT 좌표 상한은
  약 `255.9999 mm`이며, 기존 `+0x40000000` BinOffset이 남아 있으면 양의
  headroom은 약 `128 mm`다. 스케일 변경 후
  절대엔코더를 재참조하고 MaxModulo/BinOffset을 읽기 전에는 큰 이동을 시험하지
  않는다.
- 단축 continuous/endless motion은 비활성 SW limit 상태에서 MaxModulo overflow
  뒤 남은 거리를 계속 이동할 수 있다. Group `_LMCProfile`은 기본적으로
  명시적 SW limit가 없어도 `±MaxModulo`를 final endpoint로 검사하므로 별도다.
- Jerk 입력 단위는 `_LMCAxis`가 정의한 `axis application unit/s^3/1000`이다.
  UI는 입력값에 UNIT을 곱해 DINT로 보내며 기본값 `0`도 허용한다. 예를 들어
  물리 jerk가 `1000 mm/s^3`이면 Jerk 칸에 `1`을 입력하고 UNIT `10000`을 사용한다.
- 현재 저장된 `_LMCAxis1..4`는 `_JERK_PROFILE`, `JMax=75000 mm`다. nonzero
  Jerk 시험 전 다운로드된 PLC의 MoveType/JMax와 장비 허용 범위를 다시 확인한다.
- Group UNIT 콤보도 PC UI가 적용한다. 현재 static group은 X/Y/Z/U 4축,
  `Coordinate=None`, `ExactStop`/`ContinuousDirect`, `Aborting`/`Buffered`만
  지원한다. `_LMCRobotBase1`은 `_JERK_PROFILE`, `JMax=50000 mm`로 저장돼 있다.
- Group Power On/Off는 각각 `0x204A`/`0x204B`의 별도 API다. 두 ACK 모두
  mode-change 시작 접수일 뿐 최종 상태가 아니다. Group Read Status에서 각각
  `PowerOn=True`/`PowerOn=False`를 확인해야 한다.
- Group Read Status의 `0x00040000`은 Power Ready, `0x00010000`은
  Disabled/Unlocked, `0x00020000`은 Enabled/Locked Standby로 표시한다.
- Group Enable/Disable은 robot power 명령이 아니라 configured profile의
  Lock/Unlock 명령이다. Enable ACK 뒤 `Read Status`의
  `Enabled/LockedStandby=True`를 확인해야 Move가 활성화된다. Disable은 Stop이
  아니며, PLC는 group in-position이 확인되지 않으면 unlock을 `-6`으로 거부한다.
- Enable 뒤 성공한 Read Status가 `Disabled/Unlocked`를 3회 연속 보고하면 lock
  확인 대기를 해제하고 Enable 재시도를 허용한다. Read Status 자체가 실패하면
  화면의 Power Ready와 lock 판정을 무효화하고, 진행 중이던 lock 확인은 보존한다.
  성공한 Read Status로 상태를 새로 읽기 전에는 Power On과 Move를 허용하지 않는다.
  이후 `PowerOn=False`가 확인되면 identity와 lock 준비 상태도 지운다.
- Group Reset은 axis/hardware error reset이며 profile error 전체 reset이 아니다.
  Group Stop ACK도 정지 완료가 아니므로 두 명령 뒤 Group Read Status를 확인한다.
- Set Identity Kinematics는 generic kinematic transform 생성이 아니라 현재 4축의
  identity configuration을 준비하는 제한 구현이다. 실제 profile lock은 그 다음
  Group Enable에서 수행한다. Group 화면의 Home Check와 Set Identity 자동 검사는
  선택한 X/Y/Z/U 네 축의 `_LMCAXIS_STATUS.IsReferenced`(`0x00000002`)를 읽는다.
  상태 조회 실패와 `Home/Referenced=False`를 구분해 표시하며, 후자의 경우에도
  Set Identity 전송을 차단한다. 가상축 5~9는 Cartesian identity 대상이 아니므로
  이 검사에 자동 포함하지 않는다.
- Single Axis 탭은 object name 자유 입력 방식이므로 `_LMCAxis1`부터
  `_LMCAxis9`까지 한 축씩 Load해 동일한 Power/Read/Move/Stop/Reset API를 시험한다.
  이 지원 범위는 9축 동시 Cartesian group motion을 뜻하지 않는다.
- `MoveCircle`은 공개 API와 승인된 DINT wire 계약이 없어 이 예제에 없다.
- 속도, 가속도와 감속도는 양수를 입력한다. Stop은 Stop deceleration과 현재
  Jerk 입력만 사용하므로 다른 motion 입력값과 독립적으로 실행된다.
- 유한 이동 완료는 non-standstill 관측 후 Standstill 3회로 판단하며, 감시 중에도
  Stop과 Power Off를 사용할 수 있다.
- Group Stop은 LASAL `StopMove(Mode:=3)`으로 감속 정지하고 기존 profile buffer를
  폐기한다. 정지 뒤 새 Move는 허용된다. Move 응답 `ErrorId=7`은 재시작 금지가
  아니라 `_LMCPROF_SWE_ERROR`이며, 해당 시점의 목표가 런타임 software end
  position 검사에 걸렸다는 뜻이다. 예제 로그의 `StartRaw`/`TargetRaw`와 LASAL의
  `AxReadSWEndPos`, `ReadProfileError().SubErrorNo`를 대조한다.
- Close와 창 닫기는 Stop이 아니다.
- Power On, Reset, motion과 Group Power/Configure/Lock 명령은 체크박스나
  확인창 없이 버튼 클릭 시 즉시 송신된다.
- motion 가능성이 남은 상태에서 창을 닫아도 확인창이나 자동 Stop 없이 연결을
  종료한다. 실제 축 정지는 사용자가 Stop, Power Off 또는 외부 장치로 확인한다.
- 실행 중 Cancel 기능은 제공하지 않는다. API timeout은 기본 3초다.
- callback log는 raw UDP diagnostic data이며 motion 완료 판정이 아니다.

활성 command mapping은 `API_MAPPING.md`, 구현 판단과 안전 설계는 `DESIGN.md`를
참조한다.

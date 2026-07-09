# LASAL Motion Control Lib API List

기준 구현은 `LMC_Library/LMC_API_Delivery`의 `LasalMotionControlLib`이다.

이 문서는 현재 DLL의 public API와 구현 대상 기준을 정리한다. 기존
Elmo/PMAS 스타일의 `LMC_*Cmd` 메소드명은 현재 DLL에서 제거했다. 같은
패킷을 보내는 중복 alias를 만들지 않고, 한 동작에는 하나의 public API만
둔다.

## 구현 원칙

- Wireshark 캡처로 확인된 패킷 중 LASAL 쪽에서 실제로 받을 기능만 함수화한다.
- API 내부에서 단위 변환을 하지 않는다.
- 호출자는 LASAL PLC가 받을 internal DINT 값을 직접 넘긴다.
- `LMCSingleAxis`/`LMCGroupAxis` 객체가 name lookup으로 reference를 받아
  보관하고, 이후 호출에서는 저장된 reference로 패킷을 만든다.
- `PowerMembers`처럼 여러 축에 같은 명령을 반복하는 동작은 라이브러리
  protocol API가 아니라 사용자 프로그램/test app helper에서 처리한다.

## 연결 API

클래스: `LMCConnection`

- `RpcInitConnection(string remoteAddress, int remotePort, string localAddress)`
- `RpcInitConnection(string remoteAddress, int remotePort, string localAddress, int callbackPort, uint eventMask)`
- `CloseConnection()`
- `Dispose()`

연결 API는 TCP socket을 열고 캡처 기반 RPC handshake를 수행한다.
`RpcInitConnection`은 session init(`0x8080`) 후 callback registration
(`0x405C`)을 보낸다. `CloseConnection`/`Dispose`는 close frame(`0x405D`)을
보낸다.

## 단축 Axis API

클래스: `LMCSingleAxis`

짧은 호환 class alias: `LMCAxis`

생성자:

- `LMCSingleAxis(LMCConnection connection, string axisName)`
- `LMCAxis(LMCConnection connection, string axisName)`

생성자는 axis name lookup(`0x103C`) 후 axis info(`0x202B`)를 보낸다.

동작 API:

- `PowerOn()`
- `PowerOff()`
- `Reset()`
- `Stop(int deceleration, int jerk)`
- `ReadStatus()`
- `ReadStatus(out LMC_Response response)`
- `GetActualPosition()`
- `GetActualPosition(out LMC_Response response)`
- `MoveAbsoluteEx(int position, int velocity, int acceleration, int deceleration, int jerk, LMC_DIRECTION direction = LMC_DIRECTION.Shortest)`
- `MoveRelativeEx(int distance, int velocity, int acceleration, int deceleration, int jerk, LMC_DIRECTION direction = LMC_DIRECTION.Shortest)`
- `MoveVelocityEx(int velocity, int acceleration, int deceleration, int jerk, LMC_DIRECTION direction)`

## Group API

클래스: `LMCGroupAxis`

짧은 호환 class alias: `LMCGroup`

생성자:

- `LMCGroupAxis(LMCConnection connection, string groupName)`
- `LMCGroup(LMCConnection connection, string groupName)`

생성자는 group name lookup(`0x1042`)을 보내고 group reference를 보관한다.

동작 API:

- `GetGroupMembersInfo()`
- `GroupEnable()`
- `GroupDisable()`
- `GroupReset()`
- `GroupStop(int deceleration, int jerk)`
- `GroupReadStatus()`
- `GroupReadStatus(out LMC_Response response)`
- `MoveLinearAbsoluteEx(int[] position, int velocity, int acceleration, int deceleration, int jerk)`

## 현재 비제공 API

아래 항목은 기존 문서 또는 PMAS API 이름에는 있었지만 현재
`LMC_API_Delivery` public API로 제공하지 않는다.

| 항목 | 현재 판단 |
|---|---|
| `LMC_*Cmd` 메소드 alias | 제거. 중복 public API를 만들지 않는다. |
| `LMC_PowerMembers` | 라이브러리 API 아님. 사용자 프로그램에서 축 목록을 순회하며 `PowerOn`/`PowerOff` 호출. |
| `LMC_SetKinTransformCartesian4Axis` | 캡처 패킷은 있으나 현재 DLL public API 미구현. LASAL group 운용에서 필요 확정 시 구현. |
| `LMC_GroupReadActualPosition` | command `0x2051` 상수만 있고 public API/frame/parser 미구현. |
| 자동 unit converter | 제거. `LMC_Units`는 상수 선언 용도이며 packet builder가 참조하지 않는다. |

## 권장 호출 순서

Group 운전:

`RpcInitConnection → LMCGroup 생성 → LMCAxis 멤버 생성 → 각 축 PowerOn → GroupEnable → MoveLinearAbsoluteEx`

Group 정지/복귀:

`GroupStop → GroupDisable → 각 축 PowerOff 또는 단축 Axis API로 개별 운전`

단축 Axis 운전:

`RpcInitConnection → LMCAxis 생성 → PowerOn → MoveAbsoluteEx/MoveRelativeEx/MoveVelocityEx → Stop → PowerOff`

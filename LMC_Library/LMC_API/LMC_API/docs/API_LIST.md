# LASAL Motion Control Lib API List

기준 구현은 `LMC_Library/LMC_API_Delivery`의 `LasalMotionControlLib`이다.
검토 기준일은 2026-07-10이다.

이 문서는 현재 DLL의 public API와 구현 대상 기준을 정리한다. 기존
Elmo/PMAS 스타일의 `LMC_*Cmd` 메소드명은 현재 DLL에서 제거했다. 같은
패킷을 보내는 중복 alias를 만들지 않고, 한 동작에는 하나의 public API만
둔다.

상세 개발 순서와 완료 조건은
[`API_DEVELOPMENT_BACKLOG_2026-07-10.md`](../../../LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)를
기준으로 한다.

## 현재 판정

API 개발은 완료 상태가 아니다.

- 캡처된 고유 command ID: 23개
- C# request builder/public 호출 경로: 21개
- C# 미구현: `0x2051`, `0x20E7`
- tracked LASAL server와 end-to-end 완료: 0개
- 자동화 test project: 없음

tracked `TCPMotionInterface`에는 2026-07-10에 `0x8080`, `0x405C`,
`0x405D` 단일-session handler 코드가 반영됐다. 다만 LASAL IDE compile과
실제 PLC handshake 재캡처가 아직 없으므로 end-to-end 완료로 표시하지
않는다. 아래의 'C# 구현'도 PLC 통합 완료를 뜻하지 않는다.

상태 정의:

- C# 구현: public/internal path와 request builder가 존재
- 부분 구현: response/error/result 또는 test가 부족
- E2E 차단: LASAL handler/contract가 없어 실제 PLC 왕복 불가
- 미구현: public API 또는 frame builder 없음

## 구현 원칙

- Wireshark 캡처로 확인된 패킷 중 LASAL 쪽에서 실제로 받을 기능만 함수화한다.
- API 내부에서 단위 변환을 하지 않는다.
- 호출자는 물리값에 축/인자에 맞는 `LMC_Units`를 곱하고 DINT 범위를
  검사한 `int`를 넘긴다.
- read 결과는 internal DINT 그대로 반환하며 호출자가 같은 UNIT으로 나눈다.
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

상태/응답 property:

- `IsRpcInitialized`
- `IsCallbackListenerRunning`
- `CallbackPort`, `EventMask`, `CallbackLocalEndPoint`
- `RpcSessionInitResponse`, `RpcCallbackRegistrationResponse`, `RpcCloseResponse`

event:

- `CallbackReceived`
- `CallbackListenerError`

연결 API는 TCP socket을 열고 캡처 기반 RPC handshake를 수행한다.
`RpcInitConnection`은 session init(`0x8080`) 후 callback registration
(`0x405C`)을 보낸다. `CloseConnection`/`Dispose`는 close frame(`0x405D`)을
보낸다.

현재 판정: RPC phase-1 코드 반영 / LASAL IDE·PLC E2E 검증 대기.

- tracked LASAL은 한 개의 활성 RPC session만 허용
- C#은 callback/close 계열 4-byte ACK를 status/error로 해석
- callback transport는 Maestro manual 기준 UDP로 확정
- callback port `0`은 실제 ephemeral listener port를 등록
- remote/local address는 `0.0.0.0`, broadcast, IPv6가 아닌 구체적인 IPv4만 허용
- 실제 callback datagram이 없어 event payload와 typed parser는 미확정

UNIT 배포 규칙은
[`UNIT_CONVERSION_MANUAL_2026-07-10.md`](../../../LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md),
RPC 구현 범위는
[`RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md`](../../../LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md)를
따른다.

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

| API | C# 상태 | E2E 상태 / 남은 작업 |
|---|---|---|
| Axis 생성자 | 부분 구현 | `0x103C` tracked handler 없음, `0x202B` response 폐기, 4축 dispatch 없음 |
| `PowerOn/Off`, `Reset`, `Stop` | request 구현 | tracked command ID 불일치, `_Edit` response/인자 처리 미완료 |
| `ReadStatus` | 부분 구현 | LASAL response와 error tail 확정 필요 |
| `GetActualPosition` | 부분 구현 | PMAS LREAL과 LASAL DINT response 구분, live PLC 캡처 필요 |
| 세 motion API | request 구현 | C# DINT와 LASAL LREAL parser 불일치, no-op/오동작 위험 |

추가 결함:

- value parser 실패도 숫자 `0`을 반환해 정상값 0과 구분되지 않음
- name encoding/길이와 direction enum validation 부족
- test app이 모든 field에 `8388608` scale을 적용해 unit 정책과 충돌

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

| API | C# 상태 | E2E 상태 / 남은 작업 |
|---|---|---|
| Group 생성자 | request 구현 | `0x1042` LASAL handler가 없어 생성 불가 |
| `GetGroupMembersInfo` | 잘못된 부분 구현 | 1350-byte structured response를 ACK로 오판; typed parser 필요 |
| enable/disable/reset/stop | request 구현 | tracked handler 없음, `_Edit` 실행 주석/no response |
| `GroupReadStatus` | 부분 구현 | C# payload 첫 DINT `0`과 캡처 group handle `0x0100` 불일치 |
| `MoveLinearAbsoluteEx` | 부분 구현 | C# DINT 104B와 LASAL LREAL 312B 불일치; mode가 고정됨 |
| `GroupReadActualPosition` | 미구현 | `0x2051` builder/public API/vector parser/LASAL handler 필요 |
| `SetKinTransformEx/Cartesian` | 미구현 | `0x20E7` 1320-byte serializer/public API/LASAL apply path 필요 |

`0x20E7` 구조는 2026-07-10 보완 분석에서
`MMC_SETKINTRANSFORMEX_IN`/Cartesian wrapper로 확인했다. 구조 미확정 상태는
아니지만 unique payload sample이 1개뿐이므로 값 변화 캡처가 더 필요하다.

## Public support types

- `LMC_Response`: common envelope, raw payload, status/error compatibility facade
- `LMCCallbackEventArgs`, `LMCCallbackErrorEventArgs`
- `LMC_DIRECTION`
- `LMC_Units`: unit 상수 선언만 제공하며 packet builder에서는 사용하지 않음

typed lookup/status/position/group-members result와 callback message parser는
아직 제공하지 않는다.

## 의도적으로 제공하지 않는 API

| 항목 | 판단 |
|---|---|
| `LMC_*Cmd` method alias | 제거 유지. 중복 public API를 복구하지 않음 |
| `LMC_PowerMembers` | protocol API가 아님. application/test helper에서 반복 호출 |
| 자동 unit converter | 제거 유지. caller가 internal DINT를 명시적으로 전달 |

## 개발 우선순위

P0는 canonical LASAL, RPC/DINT contract, target dispatch, response parser,
test app 안전성, 자동화 test다. P1은 `0x20D2` typed result, `0x2051`,
`0x20E7`, group mode/session/callback이다.

세부 task ID와 Definition of Done은
[`API_DEVELOPMENT_BACKLOG_2026-07-10.md`](../../../LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)를
따른다.

## 권장 호출 순서

아래 순서는 목표 API 흐름이며 현재 tracked LASAL source에서는 실행되지
않는다. P0 contract와 handler가 완료된 뒤 사용한다.

Group 운전:

`RpcInitConnection → LMCGroup 생성 → LMCAxis 멤버 생성 → 각 축 PowerOn → 필요 시 SetKinTransformEx/Cartesian → GroupEnable → MoveLinearAbsoluteEx`

Group 정지/복귀:

`GroupStop → GroupDisable → 각 축 PowerOff 또는 단축 Axis API로 개별 운전`

단축 Axis 운전:

`RpcInitConnection → LMCAxis 생성 → PowerOn → MoveAbsoluteEx/MoveRelativeEx/MoveVelocityEx → Stop → PowerOff`

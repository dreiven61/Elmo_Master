# LASAL Motion Control Lib API List

기준 구현은 `LMC_Library/LMC_API_Delivery`의 `LasalMotionControlLib`이며,
검토 기준일은 2026-07-13이다.

## 현재 판정

- Wireshark 기준 대상 command: 23개
- PC request builder/public 호출 경로: 23/23
- PC 자동 테스트 runner: 42/42 PASS
- LASAL static source contract: PASS
- LASAL IDE canonical project Rebuild: PASS (`0 error`, `0 warning`)
- tracked LASAL active source path: 18/23
- tracked LASAL deterministic unsupported `-5`: 5/23
  (`0x2049`, `0x2085`, `0x2051`, `0x20A4`, `0x20E7`)
- 실제 PLC E2E 및 Wireshark 재캡처: 0/23

따라서 "PC API 구현 완료"는 23개 packet을 생성·파싱하는 C# source 범위를
뜻한다. LASAL은 RT Task 없이 CyWork에서 실행되며 위 5개 group 명령은 안전한
LASAL 의미가 승인되지 않아 의도적으로 `-5`를 반환한다. PLC download와 실제
재캡처가 끝났다는 의미는 아니다.

상세 개발 상태는
[`API_DEVELOPMENT_BACKLOG_2026-07-10.md`](../../../LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)를
기준으로 한다.

## 공통 계약

- wire numeric motion field는 little-endian signed DINT다.
- DLL은 단위를 변환하지 않는다. 호출자가 물리값에 PLC 설정과 일치하는
  `LMC_Units`를 곱하고 DINT 범위를 검사한 `int`를 전달한다.
- read 결과는 raw DINT이며 호출자가 같은 UNIT으로 나눠 표시한다.
- axis/group 이름은 LASAL의 실제 object name과 대조하고, 반환된 `UINT16`
  reference는 포인터가 아닌 opaque descriptor로 취급한다.
- public API는 한 기능당 하나를 원칙으로 하며 제거된 `LMC_*Cmd` alias는
  복구하지 않는다.
- legacy `LmcMotionApi.dll`과 binary/source 호환되지 않는다. 기존 consumer는
  [`MIGRATION_FROM_LMCMOTIONAPI.md`](MIGRATION_FROM_LMCMOTIONAPI.md)에 따라
  namespace, 객체 API와 UNIT/DINT 변환을 이관하고 재컴파일해야 한다.
- callback은 source address를 검증한 UDP raw payload event까지만 제공한다.
  실제 callback datagram 구조가 캡처되기 전에는 typed parser를 만들지 않는다.
- 다중 PC 읽기 공유와 motion/control ownership은 LASAL session/dispatcher가
  강제해야 한다. PC DLL 인스턴스만으로 server-side 소유권은 보장되지 않는다.

UNIT 규칙은
[`UNIT_CONVERSION_MANUAL_2026-07-10.md`](../../../LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md),
RPC/callback 범위는
[`RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md`](../../../LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md)를
따른다.

## 연결 API

클래스: `LMCConnection`

생성자:

- `LMCConnection()`
- `LMCConnection(LMCConnectionOptions options)`

동기 API:

- `RpcInitConnection(string remoteAddress, int remotePort, string localAddress)`
- `RpcInitConnection(string remoteAddress, int remotePort, string localAddress, int callbackPort, uint eventMask)`
- `CloseConnection()`
- `Dispose()`

비동기 API:

- `RpcInitConnectionAsync(..., CancellationToken cancellationToken)`
- `CloseConnectionAsync(CancellationToken cancellationToken)`

품질/상태 API:

- `State`, `IsConnected`, `IsRpcInitialized`, `IsCallbackListenerRunning`
- `Options`, `LastInitializationException`, `LastTransportException`,
  `LastCloseException`
- `CallbackPort`, `EventMask`, `CallbackLocalEndPoint`, `RejectedCallbackCount`
- `ConnectionStateChanged`, `CallbackReceived`, `CallbackListenerError`

`LMCConnectionOptions`는 connect/read/send timeout, callback thread join timeout,
callback source-address 검증을 설정한다. callback port `0`은 실제 ephemeral
UDP port를 연 뒤 그 값을 `0x405C`에 등록한다. callback payload getter는
방어 복사본을 반환한다.

timeout/전송 오류와 이미 시작된 RPC의 취소는 해당 transport를 즉시
폐기하고 state를 `Faulted`로 바꿔 늦게 도착한 response가 다음 command에
섞이지 않게 한다. lock/queue에서 대기 중인 작업의 취소는 현재 active
request transport를 닫지 않는다. 새 연결 인자가 유효하지 않으면 기존
session을 유지하고, reconnect가 성공하면 이전 session에서 만든 axis/group
object는 session generation mismatch로 거부한다. close nonzero ACK는 local
cleanup 뒤 예외로 전달하고 response와 `LastCloseException`을 보존한다.

## 단축 Axis API

클래스: `LMCSingleAxis`, 호환 짧은 이름 `LMCAxis`

생성자:

- `LMCSingleAxis(LMCConnection connection, string axisName)`
- `LMCAxis(LMCConnection connection, string axisName)`
- `LMCSingleAxis.CreateAsync(connection, axisName, cancellationToken)`

생성자는 `0x103C` lookup 뒤 `0x202B` AxisInfo를 검증하고 reference를
보관한다. WPF처럼 lookup 중 취소가 필요한 호출자는 synchronous 생성자를
`Task.Run`으로 감싸지 말고 `CreateAsync`를 사용한다.

| Public API | Command | 입력/결과 | PC 상태 | PLC 상태 |
|---|---:|---|---|---|
| `PowerOn`, `PowerOff` | `0x2023` | `LMC_Response` | 구현 | handler 있음, PLC 미검증 |
| `Reset` | `0x2024` | `LMC_Response` | 구현 | handler 있음, PLC 미검증 |
| `Stop` | `0x2022` | deceleration/jerk DINT | 구현 | handler 있음, PLC 미검증 |
| `ReadStatusResult` | `0x2028` | `LMCReadStatusResult` | typed parser 구현 | PLC 재캡처 필요 |
| `GetActualPositionResult` | `0x202E` | `LMCReadActualPositionResult` | DINT parser 구현 | PLC 재캡처 필요 |
| `MoveAbsoluteEx` | `0x209F` | 5개 motion DINT, direction | 구현 | handler 있음, PLC 미검증 |
| `MoveRelativeEx` | `0x20A0` | signed distance 포함 DINT | 구현 | handler 있음, PLC 미검증 |
| `MoveVelocityEx` | `0x20A2` | velocity/dynamics/direction | 구현 | handler 있음, PLC 미검증 |

각 네트워크 동작은 같은 parser를 쓰는 `*Async(..., CancellationToken)` 버전을
제공한다. `ReadStatus()`/`GetActualPosition()` 호환 facade도 남아 있지만,
오류 context를 보존하는 typed result 사용을 권장한다.

## Group API

클래스: `LMCGroupAxis`, 호환 짧은 이름 `LMCGroup`

생성자:

- `LMCGroupAxis(LMCConnection connection, string groupName)`
- `LMCGroup(LMCConnection connection, string groupName)`
- `LMCGroupAxis.CreateAsync(connection, groupName, cancellationToken)`

| Public API | Command | 입력/결과 | PC 상태 | PLC 상태 |
|---|---:|---|---|---|
| `GetGroupMembersInfoResult` | `0x20D2` | 16축 reference/device/name/count typed result | 구현 | PLC 재캡처 필요 |
| `GroupEnable` | `0x2047` | `LMC_Response` | 구현 | handler 있음, PLC 미검증 |
| `GroupDisable` | `0x2048` | `LMC_Response` | 구현 | handler 있음, PLC 미검증 |
| `GroupReset` | `0x2049` | `LMC_Response` | 구현 | unsupported `-5` |
| `GroupStop` | `0x2085` | deceleration/jerk DINT | 구현 | unsupported `-5` |
| `GroupReadStatusResult` | `0x2045` | `LMCGroupReadStatusResult` | 구현 | PLC 재캡처 필요 |
| `MoveLinearAbsoluteEx` | `0x20A4` | DINT position[1..16], dynamics, options | 구현 | unsupported `-5` |
| `GroupReadActualPosition` | `0x2051` | coordinate enum, `LMCGroupReadActualPositionResult` | 구현 | unsupported `-5` |
| `SetKinTransformCartesian4Axis` | `0x20E7` | X/Y/Z/U axis objects | 구현 | unsupported `-5` |

`MoveLinearAbsoluteEx`는 `LMCGroupMotionOptions`로 coordinate system,
transition mode, buffer mode, execute를 명시할 수 있다. position 배열은
null 없이 1..16개여야 하며 남는 wire slot은 0으로 채운다.

`GroupReadActualPosition`은 LASAL-DINT v1에서 success response payload를
정확히 68 bytes로 정의한다. 4-byte command-error envelope는 typed error
result로 보존한다.

| Payload offset | Size | 의미 |
|---:|---:|---|
| 0 | 64 | `DINT position[16]` |
| 64 | 2 | function status `UINT16` |
| 66 | 2 | error ID `INT16` |

PMAS capture의 136-byte `LREAL[16] + status/error + ABI padding`은 legacy
응답이며 typed parser가 명시적으로 거부한다.

`SetKinTransformCartesian4Axis`는 캡처와 동일한 1,320-byte payload를 만든다.
공개 지원 범위는 다음 profile 하나다.

- Cartesian kinematic type `0`
- 같은 `LMCConnection`에 속한 고유 axis reference 4개
- node 순서 X/Y/Z/U
- 각 node는 identity ratio `1.0/1.0`, shift `0.0`, transform `Shift(1)`
- buffer mode `Buffered(2)`, execute `1`

노드 수·계수·축 타입·buffer가 다른 generic kinematics는 캡처가 없어 공개
지원으로 판정하지 않는다.

## Public support types

- 응답: `LMC_Response`, `LMCReadStatusResult`,
  `LMCReadActualPositionResult`, `LMCGroupReadStatusResult`,
  `LMCGroupReadActualPositionResult`, `LMCGroupMembersInfoResult`
- 연결: `LMCConnectionOptions`, `LMCConnectionState`,
  `LMCConnectionStateChangedEventArgs`
- callback: `LMCCallbackEventArgs`, `LMCCallbackErrorEventArgs`
- group: `LMCGroupMotionOptions`, `LMC_COORD_SYSTEM`, `LMC_BUFFER_MODE`,
  `LMC_GROUP_TRANSITION_MODE`
- kinematics public surface: 캡처 profile을 강제하는
  `SetKinTransformCartesian4Axis(axisX, axisY, axisZ, axisU)`만 제공
- motion/unit: `LMC_DIRECTION`, `LMC_Units`

## 권장 호출 순서

아래 순서는 PLC E2E와 machine safety 승인 전 테스트 기준이다.

현재 Group 확인:

`RpcInitConnection → group/axis lookup → GetGroupMembersInfoResult → 각 축 ReadStatusResult → GroupReadStatusResult`

단축 운전:

`RpcInitConnection → axis lookup → PowerOn → typed status 확인 → axis motion → Stop → PowerOff`

현재 PLC에서 `0x2049`, `0x2085`, `0x2051`, `0x20A4`, `0x20E7`은 `-5`를
반환한다. PC 함수가 존재하더라도 group reset/stop/position/motion/kinematics
기능으로 사용하면 안 된다. 이 5개는 negative protocol test에만 사용한다.

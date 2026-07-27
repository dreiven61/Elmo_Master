# TCPMotionInterface 성능 우선 OOP 분리 설계

- 작성일: 2026-07-23
- 대상: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- 상태: Phase 5 transport-only 외부 text cleanup 적용. `TCPMotionInterface` generated
  server/client/data count는 `4/3/0`, 구현 함수는 8개이고 Diagnostics route는
  `MsgPaser`에 inline됐다. TCP direct axis/robot 연결 10개를 `.lcn` text에서 제거하고
  `ONE_Comm_Network_Table.st` external connection text를 26개에서 16개로 줄였다. tracked
  `Classes.lcb`/`Networks.lcb`도 transport-only registration과 network tuple 계약을 만족해
  switch 없는 `Phase5TransportClean` SourceOnly/full static이 PASS했다. 이후 SDO Write
  checkpoint의 현재 worktree PC Debug/Release 각 277/277 tests와 개발 WPF Debug/Release
  별도 output build도 PASS했다. 2026-07-24
  14:40~14:46 LASAL log에서 현재 Phase 5 main project의 Compiler/Linker 완료,
  ERROR/FATAL 0건과 `CInvalidArgException` 0건을 확인했다. `Find in Implementation` smoke와
  PLC runtime은 아직 검증하지 않았다
- 우선순위: PLC 주기 성능 > wire 호환성 > 유지보수성 > 구현 편의

## 1. 목적

`TCPMotionInterface`에 누적된 TCP lifecycle, request queue, RPC session, Admin,
object lookup, single-axis, group, diagnostics routing과 response 송신 책임을 분리한다.
분리는 객체 수 자체를 늘리는 것이 목적이 아니다. 다음 네 가지를 동시에 만족해야 한다.

1. 기존 `LASAL-DINT v1` request/response byte 계약을 변경하지 않는다.
2. 별도 task, mailbox, 주기 지연과 frame copy를 추가하지 않는다.
3. `TCPMotionInterface`를 transport와 static routing 책임으로 제한한다.
4. 명령 family 구현을 작고 탐색 가능한 method/class로 분리한다.

이 문서는 최종 구조와 단계별 이행 계약을 고정한다. 현재 기능 범위와 runtime 검증
상태는 [현재 아키텍처 및 릴리스 상태](ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)를
같이 본다.

## 2. 확인된 기준선

2026-07-23 Phase 1 직전 source 기준은 다음과 같다.

| 항목 | 기준선 |
|---|---:|
| `TCPMotionInterface.st` 전체 | 3,665 lines, 124,284 bytes |
| `MsgPaser` | 1,937 lines, 67,081 bytes |
| Group family | 11 command IDs, 약 24 KB |
| request queue | depth 8 |
| 실행량 | `CyWork`에서 scan당 최대 1 request |
| 기존 request copy | queue → `ActiveRequest` → `RequestBuf` |
| TCP 송신 소유자 | `TCPMotionInterface.SendData` |
| diagnostics domain | `LMCDiagnosticsService`로 이미 동기 위임 |

Phase 1 적용 후 확인값은 다음과 같다.

| 함수 | 크기 | 상태 |
|---|---:|---|
| `MsgPaser` | 44,784 bytes | Group aggregate route만 보유 |
| `HandleGroupCommands` | 23,926 bytes | Group 11개 본문 보유 |

Phase 1b 적용 당시 UTF-8 source block 확인값은 다음과 같다. LASAL 저장으로 line
ending이 CRLF로 정규화됐으므로 Phase 1 수치와 단순 증감 비교하지 않는다.

| 함수 | 당시 크기 | 상태 |
|---|---:|---|
| `MsgPaser` | 5,392 bytes | session gate, lifecycle 3개, family aggregate route만 보유 |
| `HandleAdminCommands` | 15,049 bytes | Admin 4개 본문 보유 |
| `HandleDiagnosticsCommands` | 4,745 bytes | diagnostics 24개 route/capability 본문 보유 |
| `HandleRegistryCommands` | 8,072 bytes | registry/info 3개 본문 보유 |
| `HandleAxisCommands` | 11,219 bytes | axis 8개 본문 보유 |
| `HandleGroupCommands` | 24,581 bytes | Group 11개 본문 보유 |

Phase 5 cleanup 후의 현재 source inventory는 다음과 같다. tracked `Classes.lcb`/
`Networks.lcb` registration도 이 구조와 정적으로 일치하지만 LASAL IDE Rebuild/Link와
PLC download를 수행한 runtime 증거는 아니다.

| 항목 | 외부 text 상태 |
|---|---:|
| `TCPMotionInterface` generated server/client/data count | `4/3/0` |
| `TCPMotionInterface` 구현 함수 | 8개 |
| TCP local domain/family/helper 함수 | 0개 |
| TCP direct axis/robot client 및 `.lcn` 연결 | 0개 |
| Comm Network generated external connection text | 16개, cleanup 전 26개 |

이 크기 제한은 LASAL compiler의 공식 hard limit가 아니다. 구현이 다시 비대해지는 것을
조기에 막기 위한 이 저장소의 정적 계약이다.

## 3. 결정

### 3.1 선택 패턴

최종 패턴은 **Static Router + synchronous no-task Domain Service**다.

- `TCPMotionInterface`: transport/session/FIFO/static family router/유일한 `SendData`
- `LMCControlCommandService`: Admin, registry, axis, group 명령의 검증과 실행
- `LMCDiagnosticsService`: 기존 diagnostics D0~D5 처리 유지
- family 내부 분기: private method의 `case`, 직접 호출

`LMCControlCommandService`는 task를 갖지 않는다. `TCPMotionInterface.CyWork`가 service
method를 동기 호출하므로 request는 기존과 같은 scan에서 처리된다.

### 3.2 제외한 패턴

| 대안 | 제외 이유 |
|---|---|
| 명령별 객체/Command pattern | 객체와 VMT 간접 호출이 command 수만큼 늘고 LASAL network가 과도하게 커짐 |
| 이벤트 버스/Observer | 실행 순서와 response ownership이 불명확해지고 queue가 하나 더 필요함 |
| 별도 control task + mailbox | 최소 1 scan 지연, 동기화와 copy가 추가됨 |
| reflection/문자열 기반 dispatch | 주기 경로의 문자열 탐색과 실패 모드가 증가함 |
| 상속 계층 확대 | transport와 motion domain은 is-a 관계가 아니며 base 변경 영향이 커짐 |
| service별 request/response array | request와 최대 2 KB response copy가 추가됨 |

상속은 vendor class contract를 확장할 때만 사용한다. 이 분리는 조합과 required client
연결이 맞다.

## 4. 목표 구조

```mermaid
flowchart LR
    APP["C# API / WPF"] --> TCP["TCPMotionInterface\ntransport + queue + static router"]
    TCP -->|"direct synchronous call"| CTRL["LMCControlCommandService\nno task"]
    TCP -->|"direct synchronous call"| DIAG["LMCDiagnosticsService\nexisting"]
    CTRL --> AX["_LMCAxis1..9"]
    CTRL --> ROBOT["_LMCRobotBase1"]
    TCP -->|"one owner"| SEND["SendData"]
```

현재 source와 tracked metadata 기준으로 `TCPMotionInterface`는 axis/robot client를 직접
소유하지 않고 한 command ID의 실행 소유자도 하나뿐이다. 다만 이 구조를 최종 production
network라고 부르려면 LASAL IDE에서 Reload/저장 후 generated table, Rebuild/Link와 PLC
download를 다시 확인해야 한다.

## 5. command ownership

dispatcher/wire contract 53개를 다음처럼 고정한다.

| 소유자 | family | command IDs | 수량 |
|---|---|---|---:|
| Transport | lifecycle | `0x8080`, `0x405C`, `0x405D` | 3 |
| Control | Admin general | `0x7D00`, `0x7D10` | 2 |
| Control | Group-domain Admin | `0x7D20`, `0x7D22` | 2 |
| Control | registry/info | `0x103C`, `0x1042`, `0x202B` | 3 |
| Control | axis | `0x2022`, `0x2023`, `0x2024`, `0x2028`, `0x202E`, `0x209F`, `0x20A0`, `0x20A2` | 8 |
| Control | group | `0x20D2`, `0x2047`, `0x2048`, `0x2049`, `0x204A`, `0x204B`, `0x2085`, `0x20A4`, `0x2045`, `0x2051`, `0x20E7` | 11 |
| Diagnostics | D0~D5 reserved family | `0x7E00`~`0x7E51`의 정의된 24개 ID | 24 |
| 합계 |  |  | 53 |

`0x7E00` capability frame은 최종적으로 diagnostics owner에 포함한다. 이행 중 현재
transport에 남아 있는 capability 조립도 별도 phase에서 service로 이동하며 wire
payload는 바꾸지 않는다.

## 6. 호출 계약

### 6.1 외부 service method

`LMCControlCommandService.ClassSvr`에 다음 global method를 둔다.

```text
HandleRequest(
  CommandId       : UINT,
  Reference       : UINT,
  pRequestFrame   : ^USINT,
  RequestFrameSize: UDINT,
  pResponseFrame  : ^USINT,
  ResponseCapacity: UDINT
) -> ResponseSize : DINT
```

- request pointer는 `TCPMotionInterface.RequestBuf[0]`을 직접 가리킨다.
- response pointer는 `TCPMotionInterface.Sendbuf[0]`을 직접 가리킨다.
- size는 8-byte outer header를 포함한 전체 frame 크기다.
- service가 기존 offset 그대로 response frame을 작성한다.
- `ResponseSize > 0`이면 transport가 `SendData`를 정확히 한 번 호출한다.
- `ResponseSize <= 0` 또는 capacity 위반이면 transport가 공통 fail-closed error를 만든다.
- service는 socket, queue, session close와 `SendData`에 접근하지 않는다.

이 계약은 추가 frame copy 없이 기존 body를 단계적으로 옮길 수 있게 한다. private
family handler는 아래의 고정 ABI로 같은 pointer/size를 직접 전달받는다. class variable에
caller pointer를 보존하거나 별도 request/response frame을 만들지 않는다.

```text
HandleAdminCommands(
  CommandId, Reference, pRequestFrame, RequestFrameSize,
  pResponseFrame, ResponseCapacity
) -> ResponseSize

HandleRegistryCommands(...same ABI...) -> ResponseSize
HandleAxisCommands(...same ABI...) -> ResponseSize
HandleGroupCommands(...same ABI...) -> ResponseSize

MoveLinearAbsEx(
  Reference, pResponseFrame, ResponseCapacity,
  pRequestFrame, RequestFrameSize
) -> ResponseSize

GroupReadStatus(
  pResponseFrame, ResponseCapacity
) -> ResponseSize
```

타입은 `HandleRequest`와 동일하게 `CommandId/Reference : UINT`, frame pointer는
`^USINT`, size/capacity는 `UDINT`, `ResponseSize : DINT`다. 이 순서와 타입은 LASAL
declaration과 정적 계약에서 함께 고정한다.

### 6.2 router

router는 command range 추론이 아니라 명시적 ID 목록을 사용한다. reserved gap이나
향후 extension이 잘못된 service로 들어가는 것을 막기 위해서다.

```text
case CommandID of
  lifecycle IDs:
    HandleTransportCommand();

  26 control IDs:
    responseSize := ControlCommands.HandleRequest(...);

  24 diagnostics IDs:
    responseSize := Diagnostics.HandleRequest(...);

  else
    BuildUnsupportedCommandResponse();
end_case;
```

service의 `HandleRequest`도 Admin/registry/axis/group의 네 묶음만 고정 분기한다. family
handler 호출은 private direct method로 유지하고 command별 객체 호출은 만들지 않는다.

## 7. 상태와 의존성 소유권

| 상태/의존성 | 최종 소유자 |
|---|---|
| socket, connected client, RPC registration | `TCPMotionInterface` |
| session epoch와 close ordering | `TCPMotionInterface` |
| ingress parser, depth-8 queue, active request | `TCPMotionInterface` |
| `RequestBuf`, `Sendbuf`, 유일한 `SendData` | `TCPMotionInterface` |
| axis/group command scratch와 last status | `LMCControlCommandService` |
| object-name buffers와 registry readiness | `LMCControlCommandService` |
| `LMCAxis1..9`, `LMCRobot` clients | `LMCControlCommandService` |
| Bulk/Recorder/SDO ticket와 BootId | `LMCDiagnosticsService` 및 기존 하위 service |

외부에서 관측되는 상태를 옮길 때 초기값과 reset 시점도 함께 옮긴다. TCP session close가
control state를 무효화해야 하는 항목이 확인되면 `NotifySessionClosed`를 명시적으로
추가한다. 근거 없이 모든 motion state를 disconnect 때 초기화하지 않는다.

## 8. 성능 불변조건

다음은 설계 권고가 아니라 구현 gate다.

1. 새 realtime/cyclic/background task를 만들지 않는다.
2. 기존 depth-8 queue와 scan당 최대 1 request 정책을 유지한다.
3. 기존 queue copy 외 request/response array copy를 추가하지 않는다.
4. control request당 domain service global call은 최대 1회다.
5. family 내부는 private direct method와 정적 `case`만 사용한다.
6. heap allocation, 문자열 dispatch, 주기별 object-name discovery를 금지한다.
7. TCP 송신은 `TCPMotionInterface.SendData`만 수행한다.
8. accepted command와 후속 status poll 순서를 바꾸지 않는다.
9. diagnostics `ProcessOperations`는 기존 request 처리 뒤 순서를 유지한다.

정적 size gate는 service의 custom implementation method와 final `MsgPaser` 각각에
`32,768 bytes` 기준을 유지한다. Phase 1b의 다섯 local family handler는 Phase 5 source에서
제거됐다. switch 없는 `Phase5TransportClean` default checkpoint가 최종 size와 tracked
method registration을 확인해 PASS했다. LASAL IDE compiler의 실제 수용 여부는 Rebuild/Link로
별도 확인한다.

## 9. 프로토콜 및 동작 불변조건

- `LmcProtocol.cs`와 `DINT_PACKET_MAP.txt`의 command ID, endian, byte offset을 유지한다.
- response outer header와 command별 payload 길이를 유지한다.
- stale session request는 실행하지 않는다.
- close ACK를 보낸 뒤 session epoch를 증가시키는 순서를 유지한다.
- ingress fault response는 fault 이전에 accept된 request 뒤에 보낸다.
- D5 submit 처리 뒤 `Diagnostics.ProcessOperations` 순서를 유지한다.
- `0x2047`은 native acceptance를 반환하고 완료는 `0x2045` poll로 확인한다.
- `0x7D22`와 group motion의 configured/powered/locked gate를 유지한다.
- unknown command는 현재 공통 `-4` 응답을 유지한다.

## 10. 단계별 구현

### Phase 0 — 계약 동결

- current source/full static 계약과 PC tests를 baseline으로 보존한다.
- command 53개 ownership 표와 wire 문서를 고정한다.
- 완료 상태: 완료.

### Phase 1 — 동일 class의 Group method 분리

- LASAL IDE에서 `TCPMotionInterface.HandleGroupCommands` private method를 생성한다.
- Group 11개 case body를 byte 동등하게 이동한다.
- `MsgPaser`에는 한 개 aggregate route만 둔다.
- method size와 단일 caller 계약을 자동 검사한다.
- 완료 상태: 2026-07-23 구현 완료, SourceOnly/full static PASS.

이 phase는 즉시 `Find in Implementation` 탐색 단위를 줄이고 다음 class migration의
diff를 작게 만든다. wire와 network는 바뀌지 않는다.

### Phase 1b — 동일 class의 나머지 family method 분리

- LASAL IDE에서 `HandleAdminCommands`, `HandleDiagnosticsCommands`,
  `HandleRegistryCommands`, `HandleAxisCommands` private method를 생성한다.
- 기존 case body를 byte 동등하게 이동하고 `MsgPaser`에는 aggregate route만 둔다.
- lifecycle `0x8080`, `0x405C`, `0x405D`와 session gate는 transport에 남긴다.
- 완료 상태: 2026-07-23 구현 완료, SourceOnly/full static PASS.

이 단계도 class/network/task/frame copy를 추가하지 않는다. 다음 service 이관 시 family별
diff와 LASAL 탐색 범위를 줄이기 위한 안전한 중간 구조다.

### Phase 2 — no-task service 골격과 network

- `LMCControlCommandService` class를 IDE에서 생성한다.
- task/automatic 속성을 모두 끈다.
- `ClassSvr`, `LMCAxis1..9`, `LMCRobot` channel을 만든다.
- `TCPMotionInterface`에 required client `ControlCommands`를 추가한다.
- `Comm_Network`에 service object와 연결을 추가한다.
- 초기에는 command route를 바꾸지 않고 generated metadata/full static부터 통과시킨다.

완료 상태(2026-07-24): class 속성, `ClassSvr`, required axis/robot client 10개,
global/private method ABI, `TCPMotionInterface.ControlCommands`와 generated class/header
metadata까지 저장했다. 이어 `GroupMovePos : _LMCPROF_POS`,
`GroupKinematicReady : BOOL`, 그리고 `MoveLinearAbsEx`의
`pRequestFrame : ^USINT`/`RequestFrameSize : UDINT` 입력 선언도 LASAL IDE에서 저장했다.
Phase 2 구조 저장 시점에는 service method가 모두 `ResponseSize := -1`인 fail-closed
골격이었다. 이후 Phase 3A에서 Group-domain body를 준비했지만 그 checkpoint에서는
`HandleRequest`, registry, axis가 계속 fail-closed이고 `ControlCommands.HandleRequest`
호출도 0개라 기존 command route가 유지됐다.

`Comm_Network`에는 task 없는 `LMCControlCommandService1` 객체 한 개와 incoming 1개,
axis/robot outgoing 10개를 합한 관련 연결 11개가 저장됐다. 성공 Rebuild가 삭제됐던
`ONE_Comm_Network_Table.st`를 현재 network 기준으로 재생성했고, Link, PLC Download와
project load까지 성공했다. 따라서 선언 저장 직후의 미연결 `ControlCommands` 오류와
cascade 한 건은 해소됐다. 이 Download는 dormant service의 compile/topology 증거이며
service runtime route 증거는 아니다.

Phase 3A 최종 checkpoint에서 SourceOnly/full `Phase3GroupDormant`, PC Debug/Release 각 148개,
개발 WPF Debug/Release build가 모두 PASS했다. IDE 종료 전 `TCPMotionInterface`의
`ControlCommands`, `LMCAxis3` implementation search도 성공했고 전체
`%TEMP%\Lasal2.log`의 `CInvalidArgException`은 0건이다.

### Phase 3 — Group domain 원자 이동

- Group 11개와 Group 상태를 공유하는 `0x7D20`, `0x7D22`를 같은 checkpoint에서
  service로 이동한다. 둘만 transport에 남기면 Group state owner가 둘로 갈라진다.
- `HandleGroupCommands`, `HandleAdminCommands`의 두 Group-domain case와
  `MoveLinearAbsEx`, `GroupReadStatus` helper/state를 service로 이동한다.
- 모든 직접 `SendData`를 `ResponseSize` 반환으로 바꾼다.
- `TCPMotionInterface`의 13-ID aggregate route를 service call로 교체한다.
- 13개 ID 각각 local/service 중 실행 소유자가 정확히 하나인지 검증한다.
- 호출되지 않는 `ClampLRealToDint`는 이 phase의 이동 대상이 아니다.

#### Phase 3A — dormant body 준비

network route를 활성화하기 전에는 service의 Group-domain body를 외부 편집기로
작성할 수 있다. 단, `TCPMotionInterface`의 기존 13-ID route를 그대로 두고 service의
`HandleRequest`도 fail-closed로 유지한다. 이렇게 하면 새 body는 PLC 주기 경로에서 도달할
수 없으므로 legacy owner와 이중 실행되지 않는다. 이 단계의 목적은 큰 body 이동 diff와
network route 변경 diff를 분리하는 것이다. 구현 완료를 의미하지 않으며 wire/runtime
승인도 아니다.

이동 대상의 outer header 포함 frame 크기는 다음과 같이 고정한다. 응답 크기는 정상
contract의 최대 total size이며, legacy error path의 더 짧은 응답과 status 위치도 기존
body 그대로 유지한다.

| ID | 명령 | request total bytes | response max total bytes |
|---|---|---:|---:|
| `0x20D2` | GetGroupMembersInfo | 9 | 1,358 |
| `0x2047` | GroupEnable/ProfileLock | 9 | 16 |
| `0x2048` | GroupDisable/ProfileUnlock | 9 | 16 |
| `0x2049` | GroupReset | 9 | 16 |
| `0x204A` | GroupPowerOn | 9 | 16 |
| `0x204B` | GroupPowerOff | 9 | 16 |
| `0x2085` | GroupStop | 24 | 16 |
| `0x20A4` | MoveLinearAbsoluteEx | 104 | 16 |
| `0x2045` | GroupReadStatus | 16 | 20 |
| `0x2051` | GroupReadActualPosition | 16 | 76 |
| `0x20E7` | SetKinTransformCartesian4Axis | 1,328 | 12 |
| `0x7D20` | ReadGroupParameters | 20 | 40 |
| `0x7D22` | GroupMoveLinearRelative | 112 | 24 |

Phase 3A에서 허용하는 persistent Group state는 `GroupKinematicReady`와 motion call에
필요한 `GroupMovePos`뿐이다. 나머지 parser/status scratch는 method local로 둔다. service는
`SendData`, socket, queue, `RequestBuf`, `Sendbuf`, `CurrentSock`를 참조하지 않고 전달받은
pointer에 직접 읽고 쓴다.

완료 상태(2026-07-24): 위 13개 command body와 두 helper를 service에 구현했다. legacy
transport의 13-ID route와 service `HandleRequest` fail-closed를 유지해 실행 owner는 여전히
legacy 하나뿐이다. service pointer ABI는 각 dereference 전에 total frame size를 먼저
확인하고, response capacity가 부족하면 native side effect 없이 `ResponseSize = -1`로
반환한다. command별 request/response offset, outer status, native dispatch와 helper state를
service body 자체에서 검사하는 `Phase3GroupDormant` 의미 검증도 추가했다. 외부 편집한
implementation은 이후 LASAL Rebuild/Link/Download를 통과했다. 다만 public route가
fail-closed이므로 신규 body의 PLC runtime 승인을 뜻하지 않는다.

#### Phase 3B — network 확인 후 원자 route 전환

service object와 11개 연결, generated network table, dormant full static gate는 2026-07-24
완료됐다. 그 상태에서 위 13개 ID의 transport local route를
`ControlCommands.HandleRequest` 한 번으로 바꾸고, legacy/service owner가 ID별 정확히 하나인지
검사한다. 일부 ID만 전환하는 상태는 허용하지 않는다.

이 route는 `MsgPaser` method-local `controlResponseSize : DINT` 하나만 사용한다. request와
response를 복사하지 않고 `RequestBuf[0]`, `Sendbuf[0]` pointer를 그대로 전달하며
`RequestFrameSize := Payload + 8`, `ResponseCapacity := sizeof(Sendbuf)`로 호출한다. 반환값이
`1..sizeof(Sendbuf)` 범위면 service가 만든 frame을 유지한다. 연결 실패나 범위 밖 반환이면
transport가 공통 12-byte `status=1/error=-1` frame으로 덮어쓰고
`controlResponseSize := 12`로 바꾼다. 두 경로는 분기 뒤의 공통 `SendData` 한 번으로만
전송한다. 이 규칙으로 channel call, frame copy와 send call을 각각 최소화한다.

source 완료 상태(2026-07-24): service `HandleRequest`가 Group 11개와 Admin 2개만 명시적으로
분기하고, `MsgPaser`는 해당 13개 ID를 하나의 zero-copy service route로 전달한다.
`0x7D00`, `0x7D10`은 기존 Admin handler에 남고 Registry/Axis service route는 계속
fail-closed다. verifier 기본 checkpoint와 MSBuild target은 `Phase3GroupRouted`로 바꿨으며
SourceOnly/full network 계약은 PASS했다. PC/WPF, LASAL Rebuild/Link/Download와 PLC
packet/performance 검증은 사용자의 구현 우선 결정에 따라 보류했다.

온라인 hot-switch에서는 기존 `TCPMotionInterface.GroupKinematicReady` 값이 별도 service
state로 승계되지 않는다. runtime 검증은 cold download/restart 후 새 session에서 `0x20E7`을
다시 수행하는 조건으로 시작한다. route 전 legacy 성능 baseline은 source 전환 전에 측정하지
않았으므로, 비교 시험 때 pre-route revision `65f8000`을 별도로 배포해 같은 조건으로 측정한다.

### Phase 4 — Axis, registry, remaining Admin 이동

- axis 8개와 helper/state를 이동한다.
- registry/info 3개를 이동한다.
- remaining Admin `0x7D00`, `0x7D10`을 마지막으로 이동한다.
- family마다 source/full static, PC tests와 capture regression을 통과시킨다.

source 완료 상태(2026-07-24): service `HandleRequest`가 Control 26개를 Registry 3개,
Axis 8개, Group 11개, Admin 4개의 정확한 family set으로 분기한다. `MsgPaser`는 26개를
하나의 zero-copy `ControlCommands.HandleRequest` call과 공통 `SendData`로 전달하며 네 local
family handler caller는 0개다. Phase 4 checkpoint에서는 rollback과 Phase 5 선언 정리를
위해 기존 TCP body/client/state를 남겨 뒀다. `Phase4AllControlRouted` SourceOnly/full static과
임시 Phase 4 snapshot의 PC Debug/Release 각 148 tests, 개발 WPF Debug/Release build는
통과했다. 이 결과는 현재 Phase 5 결과로 대체됐으며 IDE/PLC 증거가 아니다.

### Phase 4D — Diagnostics 24-ID 단일 service route

Phase 4D source 완료 상태(2026-07-24): `0x7E00` capability payload 생성을
`LMCDiagnosticsService.HandleRequest`로 이동했다. Diagnostics 24개 모두 기존 payload-only
zero-copy ABI를 사용했다. 이 checkpoint에서 TCP의 `HandleDiagnosticsCommands`는 outer
8-byte header, 16..2040-byte response bound, 12-byte transport fallback과 공통
`SendData` 한 번만 소유했다.
service response는 68 bytes이고 TCP total frame은 76 bytes다. service method는 32,768-byte
gate 미만이며 `Phase4DiagnosticsRouted` SourceOnly/full static을 통과했다.

required Diagnostics client가 끊긴 비정상 topology에서는 기존 local degraded capability
76-byte 응답 대신 12-byte transport `-1`을 반환한다. 정상 연결 경로는 기존 byte layout과
동등하며 이 fault-path 변경은 service 단일 owner를 유지하기 위한 승인된 정책이다.

### Phase 5 — transport 정리와 성능 승인

- 외부 text cleanup에서 `TCPMotionInterface`의 axis/robot clients, domain server/state와
  local family/helper implementation을 제거했다. generated channel count는 `4/3/0`, 최종
  구현 함수는 8개다.
- `HandleDiagnosticsCommands`를 제거하고 Diagnostics 24-ID route를 `MsgPaser`에 inline했다.
  transport에는 outer header, response bound, fallback과 최종 `SendData`만 남겼다.
- `Comm_Network.lcn`의 TCP direct axis/robot 연결 10개를 제거하고 control service의
  axis/robot 연결 10개와 TCP의 `ControlCommands`/`Diagnostics` service 연결을 유지했다.
  `ONE_Comm_Network_Table.st` external connection text는 26개에서 16개로 정리했다.
- tracked `Classes.lcb`/`Networks.lcb`의 scoped class/network record도 위 transport-only
  구조와 일치한다. TCP object의 제거 대상 member와 direct axis/robot tuple은 0개이고,
  control service의 axis/robot tuple 10개는 유지된다.
- verifier/csproj에 `Phase5TransportClean`을 구현했다. transport-only checkpoint 당시 switch
  없는 SourceOnly/full static이 PASS했고 `-AllowStaleLasalBinaryMetadata` 없이 binary
  registration gate까지 통과했다. 현재 SDO Write source는 `Classes.lcb` 동기화 전이므로 이
  과거 full 결과를 재사용하지 않는다.
- 현재 Phase 5 worktree에서 PC Debug/Release 각 277/277 tests가 PASS했다. 개발 WPF
  build도 PASS해 임시 Phase 4 snapshot 결과를 대체한다.
- PC response reader는 53개 command 각각의 정상 최대 payload를 body read 전에 검사한다.
  가장 큰 정상 payload는 Recorder chunk의 1,972 bytes이고, 초과 선언은 stream desync를
  막기 위해 transport를 즉시 `Faulted`로 전환한다. 미등록 command는 wire 송신 전에
  fail-closed한다.
- `AxisInfo(0x202B)` 성공 응답은 payload descriptor와 요청 AxisReference를 sync/async
  모두 대조한다. PMAS 38개와 SIGMATEK 32개 capture sample의 canonical field를 기준으로
  mismatch를 fail-closed하며 기존 short command error 의미는 유지한다.
- 개발 WPF의 read-only `0x2045` runner는 기본 warm-up 100회와 측정 10,000회를 순차
  실행한다. 시작 전과 실행 중 매 응답에서 Group InPosition, exact 20-byte frame과 측정
  구간 byte stability를 요구하고 raw hash/percentile/부분 실패를 CSV로 보존한다. 표시 수치는
  command gate 획득 후부터 API 응답 완료까지의 `PC_API_RPC_ELAPSED`이며 UI dispatch/gate
  wait를 제외하지만 PLC 내부 dispatch, task jitter와 overrun은 측정하지 않는다.
- runner의 count/percentile/throughput/hash/PASS/partial CSV 판정은 UI 독립 helper로
  분리하고 WPF와 PC test project가 같은 source를 compile한다. 최소/최대 count,
  nearest-rank, 안정/불안정 raw, 10,000-sample PASS evidence와 zero-sample FAIL/ABORTED
  CSV를 자동 검증한다. callback handler 예외와 callback-thread close/dispose 재진입
  loopback도 포함하며 이 검증은 PLC 내부 성능 증거가 아니다.
- `MsgPaser`를 transport/session/static router 수준으로 축소하고 올바른 이름으로
  바꾸는 것은 별도 호환 commit에서 수행한다.
- 동일 PLC/build에서 전후 성능과 packet regression을 비교한다.

## 11. LASAL IDE 배치 가이드

사용자가 Phase 2 객체 배치를 수행할 때 다음 이름을 그대로 사용한다.

소유권 경계는 명확하다. service object 생성과 Object Network 연결은 사용자가 LASAL IDE에서
수행한다. 외부 편집 단계에서는 `.lcn`을 직접 합성하거나 연결을 추정하지 않는다. 사용자가
배치·저장하고 LASAL을 완전히 종료한 뒤에만 source/정적 계약 작업을 재개한다.

1. class: `LMCControlCommandService`
2. class properties:
   - `RealtimeTask=false`
   - `CyclicTask=false`
   - `BackgroundTask=false`
   - `Automatic=false`
   - `SharedCommandTable=true`
3. server: 기본 `ClassSvr`
4. required clients:
   - `LMCAxis1` ... `LMCAxis9`
   - `LMCRobot`
   - 정확히 10개이며 `_StdLib` client는 만들지 않는다. 이동 본문의 `MemCpy`는 direct
     `_memcpy`로 치환한다.
5. global method: `HandleRequest`
6. private methods:
   - `HandleAdminCommands`
   - `HandleRegistryCommands`
   - `HandleAxisCommands`
   - `HandleGroupCommands`
   - `MoveLinearAbsEx`
   - `GroupReadStatus`
7. `TCPMotionInterface` required client: `ControlCommands`
8. `Comm_Network` object: `LMCControlCommandService1`
9. connections:
   - `TCPMotionInterface1.ControlCommands -> LMCControlCommandService1.ClassSvr`
   - `LMCControlCommandService1.LMCAxis1..9 -> _LMCAxis1..9.Control`
   - `LMCControlCommandService1.LMCRobot -> _LMCRobotBase1.Control`

Phase 2에서는 rollback을 위해 기존 TCP axis/robot 연결을 유지했다. Phase 5 외부
`Comm_Network.lcn` text에서는 TCP direct 연결 10개를 제거하고 위 service 관련 연결 11개를
유지했다. tracked `Classes.lcb`/`Networks.lcb`도 이 정적 topology와 일치하지만 최종 LASAL
IDE 저장/Rebuild/Link 완료 증거는 아니다.

Phase 5를 완료하려면 IDE를 연 뒤 변경 class를 Reload Class/선언 동기화하고, Object Network를
IDE에서 저장·재생성해야 한다. implementation source를 외부 편집하는 순서는 유지하되 `.lcn`
text만 보고 network 완료를 선언하지 않는다. 저장 후 TCP direct 연결 10개가 없고 service
관련 연결 11개가 유지되는지 generated table과 project metadata에서 함께 확인한다.

## 12. 검증과 승인 기준

### Phase 5 자동 검증

`Phase5TransportClean` checkpoint는 구현됐다. 현재 SDO Write external source의 default
SourceOnly는 PASS한다. 그러나 tracked `Classes.lcb`에 신규 Write declaration이 아직 없어
switch 없는 full은 의도적으로 FAIL한다. 아래 full 명령은 LASAL IDE에서 Reload Class/저장/Rebuild
후 binary registration gate를 우회하지 않는 최종 정적 검증으로 다시 실행해야 한다.

IDE 적용 전 external source/XML/`ONE_*` table만 중간 점검할 때는 verifier의
`-AllowStaleLasalBinaryMetadata`를 사용할 수 있다. 이 switch는 binary registration gate를
명시적으로 우회하므로 final static 결과에 사용하지 않는다.

2026-07-24 transport-only IDE 재검증과 2026-07-27 SDO Write source/PC 재검증 결과는
다음과 같다.

- default SourceOnly: PASS
- switch 없는 full: 의도적 FAIL(`Classes.lcb` SDO Write declaration 미동기화)
- `-AllowStaleLasalBinaryMetadata` full: PASS, final/build 증거로 사용 금지
- PC Debug/Release: 각 277/277 PASS
- 개발 WPF Debug/Release build: 경고 0, 오류 0
- `git diff --check`: PASS
- 2026-07-24 transport-only LASAL main project Compiler/Linker, ERROR/FATAL 0,
  `CInvalidArgException` 0: PASS. 현재 SDO Write 변경은 IDE build 전
- 위 결과는 `Find in Implementation` smoke 또는 PLC runtime 증거가 아니다

```powershell
powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass `
  -File "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" `
  -RepositoryRoot "." -SourceOnly `
  -ControlServiceCheckpoint Phase5TransportClean

powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass `
  -File "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" `
  -RepositoryRoot "." `
  -ControlServiceCheckpoint Phase5TransportClean
```

- Phase 5 SDO Write checkpoint source 기준 전체 C# request/parser tests Debug/Release 각
  277/277 PASS. SDK Write target policy, Read/Write-aware quarantine/cleanup과 성공 Write 뒤
  원 owner/session/BootId/MapRevision에 묶인 exact manual readback interlock 계약을 포함한다.
  pending cleanup orchestrator는 owner/current connection, ticket owner와 저장 MapRevision을
  dispatch 전에 fail-closed하고 capability BootId를 우선 판정한 뒤 Map mismatch를
  status/cancel 없이 quarantine한다. cached terminal status/cancel 무송신/cached pending refresh,
  Queued-only cancel과 `InvalidState` race, Running wait, exact `Cancelled/Cancelled`, fresh status와
  command exception 보존, 최소 15초/남은 deadline+1초/최대 120초 및 `<=` 경계를 검사한다.
  production WPF adapter는 같은 source를 호출하지만 wire/LASAL 변경이나 PLC live/pcap 증거는
  아니다. 직전 ledger concurrency 4개는 각 등록 test를 50회 반복해 candidate snapshot 뒤 clear 전 mutation,
  atomic clear 뒤 Arm 보존, callback 예외 뒤 waiter/ledger 재사용과 concurrent Disarm
  exact-once를 bounded wait로 검증하며 `Thread.Sleep`을 사용하지 않는다.
- D5 WPF runner는 transport/domain 분리를 유지한 public API 경로로 Submit 전 outcome
  guard/unknown-ticket quarantine, same-connection BootId·MapRevision/exact `BootIdMismatch` quarantine,
  stale local session quarantine과 capability별 two-ticket recovery proof를 구현했다.
  GeneralInline은 `0x6061:0 Int8/1`, legacy SDORead-only는 `0x1000:0 UInt32/4`의 서로 다른
  두 ticket에서 exact type/length/bytes를 확인한다. 같은 Boot/session의 exact
  `TicketNotFound`는 terminal-slot 교체 계약상 이전 ticket terminal만 증명하고 outcome
  `UNKNOWN`으로 해제한다. unresolved mutation gate와 원 deadline을 반영한 15~120초 cleanup을
  구현했다. UI 독립 `D5SdoRecoveryScopePolicy`는 owner reference+BootId+MapRevision 조합으로
  `same_owner_connection_recovery`, `new_diagnostics_identity_session`,
  `new_connection_session`, `mixed_evidence_sessions`를 순수 판정한다. MainWindow는 proof 시작
  로그와 PASS 로그에 같은 decision을 사용한다. mixed도 two-ticket application recovery
  proof와 성공 시 quarantine clear는 허용하지만 same/new session 증거로 세지 않는다. 한
  previous owner+identity로 동질인 `new_connection_session`만 decision의
  `NewConnectionRecovery=true`이고 로그의 `newConnectionRecovery=true`가 된다.
  `same_owner_connection_recovery`는 disconnect/orphan PASS가 아니다. WPF는 항상
  `orphanQualified=false`를 기록한다. 실제 orphan PASS에는 known Running old ticket, 실제
  owner loss와 별도 PLC hook/capture가 필요하다. Group Disable 포함 새 mutation은 막되 기존
  resource cleanup, Stop/PowerOff와 read-only는 허용한다. `D5SdoPendingCleanup` Resolve는
  `D5_LOG_CONTINUATION`으로 원래 qualification log에 이어 쓴다. drive-read facade는 원래
  exception type/stack을 보존하면서 `LMCDriveReadFailureContext.TryGet`으로
  `FacadePreflight`/`AxisStatusRead`/`CapabilityPreflight`/`Submission`/`StatusPolling`/
  `ResultMaterialization`의 6개 phase와 `GenericSubmissionOutcome`의 공용
  `LMCSdoSubmissionOutcome` 값을 제공한다. 기존 `SubmissionOutcome`/
  `LMCSdoReadSubmissionOutcome`은 호환용으로 같은 값을 유지한다. 각 attempt에는
  실제 capability `DiagnosticsBootId`/`MapRevision`, ticket과
  마지막 status가 보존된다. WPF는 no-submit/rejected/terminal guard를 해제하고 uncertain은
  실제 Submit identity로 quarantine하며 accepted nonterminal exact ticket을 보존한다. context
  누락/불일치는 fail-closed한다. 수동 raw `SubmitSdo[Async]`도 원래 exception에 연결된
  `LMCSdoSubmissionFailureContext.TryGet` context를 제공한다. 5개 phase는
  `RequestValidation`/`SessionPreflight`/`CapabilityPreflight`/`Submission`/
  `PostSubmissionValidation`이고 같은 `LMCSdoSubmissionOutcome`을 사용한다. 실제
  `DiagnosticsBootId`/`MapRevision`을 기록하며, WPF manual router는 no-submit/rejected를
  disarm하고 uncertain identity를 reconcile해 quarantine한다. accepted exact ticket은 manual
  operation state와 D5 tracker에 보존한 뒤 disarm하며 context 누락/불일치는 fail-closed한다.
  quarantine evidence는 operation kind를 보존하고 Read recovery proof로 Write uncertainty를
  해제하지 않는다. `0x7E50` exact Int32/4-byte Write executor/API/WPF 경로는 구현됐지만
  `UI[24] 0x2F00:24` 예약과 적용 축 확정 전에는 SDK/PLC global+per-axis gate가 off이므로
  capability bit 9와 GUI submit은 fail-closed한다.
  Phase 1 PI Write는 SDK empty allowlist와
  WPF button/handler로 이중 차단한다. PLC live/pcap 증거는 아직 없다.
- 개발 WPF Debug/Release build 경고 0/오류 0 PASS
- `git diff --check` PASS
- command ID별 owner 정확히 1개
- `Response`/`CyWork`에서 domain helper 직접 호출 금지
- control/diagnostics service에서 `SendData`, socket, queue 접근 금지
- transport에서 axis/robot client와 local domain/helper 접근 금지

### LASAL IDE 검증

1. IDE를 닫은 상태에서 변경 전 Git 상태와 external text inventory를 기록한다.
2. IDE를 열고 변경 class를 Reload Class한 뒤 declaration을 동기화한다.
3. Object Network에서 TCP direct axis/robot 10개가 없고 service 관련 연결 11개가 유지되는지
   확인한 뒤 저장·재생성한다. 외부에서 `.lcn`을 합성하지 않는다.
4. 저장 후 `.st` implementation이 이전 내용으로 덮어써지지 않았는지 확인하고 generated
   server/client/data `4/3/0`, 함수 8개, network external connection 16개를 다시 센다.
5. Rebuild/Link error 0건을 확인한다.
6. 변경 class 각각 앞/중간/뒤 implementation symbol을 `Find in Implementation`하고 smoke
   시작 이후 `%TEMP%\Lasal2.log` 신규 `CInvalidArgException` 0건을 확인한다.
7. 그 뒤에만 PLC download/cold restart를 수행하고 새 session에서 `0x20E7`부터 packet
   regression을 시작한다.

### 성능 승인

동일 controller, task cycle, compiler와 build 옵션에서 전후를 비교한다.

- 10,000회 이상 control request dispatch 측정
- task overrun 0회
- dispatch P95가 기준선 대비 5% 이상 악화되지 않을 것
- command throughput이 기준선의 98% 미만으로 떨어지지 않을 것
- response frame과 command status가 byte-for-byte 동일할 것

수치는 목표 gate이며 아직 PLC에서 측정된 결과가 아니다. WPF의 `0x2045` runner는
PC API RPC elapsed를 별도 수집할 수 있지만 network/API 처리 시간이 섞인 보조 지표다.
PLC dispatch 구간, task jitter와 overrun은 PLC 내부 측정으로 분리한다.

## 13. 남은 작업과 병행 테스트 계획

병행은 작업 흐름 기준이다. `_TCPIPServer1.MaxConnections=1`이고 같은 PLC motion owner를
공유하므로 실제 PLC 송신 시험 두 개를 동시에 실행하지 않는다. PC/static 검증과 문서·capture
분석은 병행할 수 있지만 PLC write/motion 시험은 한 세션씩 직렬화한다.

2026-07-24 이후 LASAL 변경·시험 순서는 다음으로 고정한다. 개발 source는 main 저장소의
`Lasal_PRG/Elmo_EtherCAT_Test_4Axis`에서만 수정한다. 변경 준비 후 사용자가 main project를
빌드해 오류를 확인하고, 통과한 `Elmo_EtherCAT_Test_4Axis` 폴더만
`C:\work\Elmo\Elmo_Master_test`로 복사한 뒤 그 복사본에서 장비 시험한다. 개발 작업은
test 폴더를 수정하거나 자동 동기화하지 않는다.

| 흐름 | 다음 작업 | 같이 수행할 검증 | 완료 조건 |
|---|---|---|---|
| A. Phase 5 재검증 | 임시 Phase 4 snapshot 결과를 현재 Phase 5 source 결과로 대체 | `Phase5TransportClean` SourceOnly와 PC/WPF Debug/Release는 PASS. current SDO Write declaration의 full/Compiler/Linker, implementation smoke와 장비시험 진행 | final static, IDE-generated state와 장비 결과가 모두 보존됨 |
| B. legacy/신규 성능 비교 | pre-route `65f8000`을 배포해 legacy baseline을 얻고 같은 PLC/build 조건에서 routed source 재측정 | 1 ms cycle jitter/overrun, dispatch P95, throughput, RAM, 10,000회 이상 soak | 12절 성능 gate 충족 및 원시 로그 보존 |
| C. packet 회귀 | read-only/identity를 먼저 확인한 뒤 저속 Group command를 안전 순서로 실행 | 정상·잘못된 size/reference/mode, Power/Enable/Stop, disconnect/reconnect, response byte 비교 | 기존 golden과 byte/status 동일, 이중 실행·stale session 0 |
| D. Phase 5 IDE 확인 | source와 tracked metadata의 `4/3/0`, 함수 8개, TCP direct 연결 0개, network external 16개 정적 계약은 PASS. 2026-07-24 Compiler/Linker와 오류 로그 확인도 PASS | `Find in Implementation` smoke, generated count 최종 확인 | IDE가 외부 구현을 보존하고 smoke까지 최종 구조를 수용함 |
| E. 9축 network | 새 `PosController5..9`와 `_LMCAxis5..9.LMCController` 연결을 축별 점검 | generated table, simulated axis position/status, axis-order readback | 1..9 매핑과 `0x2028`/`0x202E` 값 일치 |
| F. diagnostics qualification | Group route 변경과 독립된 runner backlog 수행 | Bulk/Recorder와 read-only D5 abort/recovery code/build 완료. exact allowlist SDO Write 경로는 gate-off checkpoint 완료. D5 outcome/BootId/operation-kind quarantine, two-ticket Read recovery proof, unresolved mutation gate와 deadline-aware cleanup 포함; negative-wire PC test/dry-run 완료. PLC live/pcap 및 SDO Read fault matrix와 승인 target의 same-value Write/readback/restore 수행 | happy path가 아닌 fault/soak와 Write 복구 원시 결과까지 보존 |

Phase 4 source 구현을 baseline보다 먼저 진행했으므로 B는 `65f8000`과 routed revision을
각각 cold download해 같은 조건으로 비교한다. 현재 Phase 5 `Phase5TransportClean`
SourceOnly는 PASS했지만 SDO Write declaration의 full static과 Compiler/Linker,
implementation smoke 및 PLC runtime은 대기 상태다. A/C/F의 PLC 실행은 서로 병렬
실행하지 않고, 장비 정지·저속·무부하 조건과 motion owner를 먼저 확인한다. static PASS만으로
production 승인하지 않는다.

## 14. rollback

- Phase 1/1b: 해당 aggregate route를 원래 same-class case body로 되돌린다.
- Phase 3/4 checkpoint rollback은 승인된 pre-cleanup revision을 사용한다. Phase 5 source에는
  local family handler와 TCP direct client가 없으므로 일부 route만 임의로 되살리지 않는다.
- Phase 5 rollback은 `TCPMotionInterface.st`, service source, class declaration,
  `Comm_Network`와 generated metadata를 같은 pre-cleanup checkpoint로 함께 복원한다.
- wire/API change가 없으므로 C# DLL rollback은 필요하지 않아야 하지만 request/parser
  regression은 rollback revision에서도 다시 확인한다.
- project metadata를 되돌릴 때 `.st`만 수정하지 말고 IDE 등록과 network를 같은
  checkpoint로 복원한다.

## 15. 관련 기준

- [현재 아키텍처 및 릴리스 상태](ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)
- [LASAL 코딩 규칙](SIGMATEK_LASAL_coding_rules.md)
- [LASAL 프로그래밍 방법 연구](SIGMATEK_LASAL_programming_method_study.md)
- [LASAL 오류 예방 가이드](SIGMATEK_LASAL_programming_error_prevention_guide.md)
- [CyWork-only TCP 실행 설계](../../LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md)
- [DINT packet map](../../LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt)
- [정적 계약 검사](../../LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1)

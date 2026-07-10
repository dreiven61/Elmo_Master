# LASAL Motion Control Lib API Development Backlog

Date: 2026-07-10

Analysis baseline: `996686d`

Current checkpoint: `d91da48`

Status: Open

## 결론

현재 API 개발은 완료 상태가 아니다.

Wireshark 자료에는 고유 command ID 23개가 있다. 현재 C# DLL에는 23개
모두의 request builder와 public 호출 경로가 있고 tracked LASAL에는 그중
21개 command의 `case` handler가 있다. 다만 `GroupReset(0x2049)`과
`GroupStop(0x2085)` handler는 실제 기능 대신 deterministic unsupported
error `-5`를 반환하므로 정상 기능 후보 source path는 19개다.

다만 source-first RT safety migration의 현재 runtime gate는 `0x202E
ReadActualPosition` 하나만 client call로 허용한다. 나머지 기존
axis/group client-call handler body는 보존하되 typed mailbox로 옮기기 전까지
error `-5`를 반환한다. handler body 존재와 현재 실행 허용을 구분해야 한다.

C#에는 command별 typed parser, strict ACK, `0x2051` LASAL-DINT vector,
`0x20E7` exact Cartesian4 serializer, group mode 옵션, timeout/state/async와
callback source 검증을 반영했다. tracked LASAL에는 RPC lifecycle, 실제
object-name lookup, opaque descriptor와 4축 DINT dispatcher를 반영했다.

LASAL IDE compile, PLC download와 실제 packet 재캡처는 아직 없다. 따라서
source 구현과 자동 테스트가 완료된 항목도 PLC end-to-end 검증 완료로
계산하지 않으며, 검증 완료 API 수는 여전히 0개다.

PC packet API의 남은 핵심 blocker는 신규 frame 추가가 아니다. 먼저 아래
LASAL/PLC P0를 끝내야 한다.

1. canonical LASAL source 확정
2. RPC/header/DINT/response 계약 통일
3. 4축 및 group target dispatch
4. false-success와 response parser 오류 제거
5. 테스트 앱 단위·성공 판정 수정
6. byte-level 자동화 테스트와 실제 PLC 캡처 확보

## 현재 구현 완료도 판정

| 구분 | 상태 | 완료 판정 |
|---|---:|---|
| Wireshark 기준 대상 command | 23개 | 전체 범위 |
| C# request builder 또는 public 호출 경로 | 23/23 | source 구현이며 PLC 완료가 아님 |
| 대응 LASAL `case` handler | 21/23 | `0x2049`, `0x2085`는 `-5` 전용 handler |
| 정상 기능 후보 source path | 19/23 | RT safety와 실제 동작은 미검증 |
| 현재 RT client-call 허용 | 1/23 | `0x202E` source path만 허용, IDE/network/PLC 검증 전 |
| C# 자동 테스트 | 42/42 PASS | fake/synthetic/loopback/source contract 검증 |
| 실제 PLC E2E 및 Wireshark 재캡처 | 0/23 | 완료된 command 없음 |

`0x2051 GroupReadActualPosition`은 PC에서 coordinate request와 exact 68-byte
LASAL-DINT response(`DINT[16] + status/error`) typed parser까지 구현했다.
captured PMAS 136-byte LREAL response는 거부한다. `0x20E7`도 PC에서 exact
1320-byte serializer와 동일 connection의 X/Y/Z/U axis object를 받는
`SetKinTransformCartesian4Axis` public API까지 구현했다. 두 command 모두
LASAL handler는 아직 없다.

따라서 완료 범위는 다음처럼 구분한다.

- **single-PC P0 MVP:** 현재 정상 기능 후보 19개는 PC core와 LASAL handler
  body가 준비됐지만, runtime client-call은 `0x202E` 하나만 먼저 연다. 나머지
  command의 queue/RtWork 이관, IDE model/build, PLC smoke test와 재캡처가
  주된 잔여 작업이다.
- **전체 23-command API:** PC packet API는 23개 source path를 갖췄다.
  LASAL의 `0x2051`/`0x20E7`, large-command staging, 실제 callback sender,
  session/ownership과 IDE/PLC 검증이 남아 있다. PC preview assembly와
  build manifest는 `0.9.0-pc-api`로 생성했다.

## 2026-07-10 진행 내용

- canonical 변경 대상은 Git 추적 중인
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`로 정했다.
- caller가 물리값에 `LMC_Units`를 곱해 DINT로 전달하는 배포 매뉴얼을
  추가했다. DLL과 PLC는 재변환하지 않는다.
- C# callback port `0` 처리와 4-byte ACK parser를 교정했다.
- tracked LASAL에 receive bound/header 검증과 `0x8080`, `0x405C`,
  `0x405D` 단일-session handler를 추가했다.
- callback 등록 전 command와 non-owner socket command를 차단했다.
- Maestro manual로 callback transport가 UDP임을 확인했다.
- 실제 callback event payload와 LASAL UDP sender는 캡처/명세가 없어
  의도적으로 구현하지 않았다.
- `_LMCAxis1..4`와 `_LMCRobotBase1`의 실제 LASAL object name을 startup 때
  읽어 opaque descriptor를 발급하고, 4개 typed client channel로 dispatch한다.
- Axis Power/Reset/Stop/Read/Move DINT handler와 Group lookup/members/
  enable/disable/status/linear handler를 tracked project에 반영했다.
- GroupReset/GroupStop은 안전한 LASAL 대응 method 확정 전까지 deterministic
  `-5` error를 반환한다.
- command별 typed parser, WPF response 안전 판정과 23-bit dummy profile
  표기, NuGet 없는 .NET Framework 4.8 자동 테스트와 LASAL static contract
  suite를 추가했다.
- PC에 `GroupReadActualPosition(0x2051)` coordinate request와 LASAL-DINT v1
  68-byte typed vector result를 추가하고 legacy 136-byte LREAL response를
  명시적으로 거부했다.
- PC에 `SetKinTransformCartesian4Axis(0x20E7)` exact 1320-byte serializer를
  추가했다. 공개 범위는 캡처된 Cartesian X/Y/Z/U identity-shift,
  `Buffered(2)` profile로 제한한다.
- group position 배열/enum validation과 coordinate/transition/buffer/execute
  옵션을 public API에 반영했다.
- connection timeout/state/error, 취소 가능한 async API, callback remote
  source 검증과 payload 방어 복사를 반영했다.
- timeout/전송 오류와 in-flight 취소는 해당 transport를 폐기해 `Faulted`로
  전환하고, queue 대기 중 취소는 active request를 건드리지 않게 했다.
- invalid reconnect input은 기존 session을 유지하고, reconnect 뒤 이전
  axis/group object는 session generation mismatch로 거부한다.
- initialization/transport/close 오류를 분리 보존하고 close nonzero ACK는
  local cleanup 뒤 호출자에게 예외로 전달한다.
- callback typed parser는 실제 datagram payload 근거가 없어 의도적으로
  추가하지 않았다. raw payload event가 현재 PC 완료 범위다.

## 분석 기준

이 문서는 다음을 교차 대조했다.

- C# source: `LMC_Library/LMC_API_Delivery/src/**`
- WPF test app: `LMC_Library/LasalMotionControlLibTestApp/**`
- tracked LASAL server:
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/**`
- untracked candidate:
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit/**`
- dummy/reference implementation:
  `Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs`
- packet evidence:
  `LMC_Library/LMC_API/Elmo_API_Packet2/WireShark/*.pcapng`
- packet analysis: `LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md`
- Maestro manual extraction: `output/pdf/maestro_api_md/**`
- current API docs: `API_LIST.md`, `LMC_PACKET_MAP.md`, delivery design docs
- history handoff: `docs/history/260710/99_analysis_summary.md`

## 상태와 우선순위 정의

- C# 구현: public/internal call path와 request builder가 존재한다.
- 부분 구현: parser, error semantics, public result 또는 test가 부족하다.
- E2E 차단: LASAL handler/response가 없어 실제 PLC 왕복이 불가능하다.
- P0: 다음 motion test 전에 해결해야 하는 blocker 또는 오동작 위험.
- P1: P0 contract 위에서 완료할 기능.
- P2: 품질, 사용성, 배포 정리.

C# 구현은 PLC 통합 완료를 의미하지 않는다.

## 현재 데이터 흐름

```mermaid
flowchart LR
    A["WPF 또는 사용자 프로그램"] --> B["LasalMotionControlLib C#"]
    B -->|"8-byte header와 DINT payload"| C["TCPMotionInterface"]
    C --> D["actual name registry와 opaque descriptor"]
    D --> F["_LMCAxis1..4 및 _LMCRobotBase1"]
    E["PMAS Wireshark capture"] -. "LREAL/REAL 기준 근거" .-> B
    E -. "legacy wire 기준" .-> C
    C -. "LASAL IDE/PLC 검증 대기" .-> B
```

tracked canonical project와 delivery C#은 현재 구현 범위에서 LASAL-DINT
header/payload를 맞췄다. `Codex_LASAL_WPF` dummy와 untracked `_Edit`는
legacy/hybrid 참고 자료이며 canonical source가 아니다.

## Command 구현 매트릭스

### Connection과 lookup

| ID | 기능 | C# DLL | canonical tracked LASAL | 판정 |
|---:|---|---|---|---|
| `0x8080` | Session Init | frame와 응답 대기 구현 | tracked 단일-session response 코드 반영 | P0: LASAL IDE/PLC 검증 대기 |
| `0x405C` | Callback Register | UDP listener, 실제 bound port, 4B ACK parser | tracked endpoint 저장/ACK 코드 반영 | P0: event 송신 제외, PLC 검증 대기 |
| `0x405D` | Close | 재연결/종료 frame 구현 | tracked ACK 후 state clear 코드 반영 | P0: PLC 검증 대기 |
| `0x103C` | Axis lookup | reference parser 구현 | 실제 object-name registry, descriptor 1..4 | source 반영, IDE/PLC 검증 대기 |
| `0x1042` | Group lookup | reference parser 구현 | 실제 `_LMCRobotBase1` name, descriptor `0x0100` | source 반영, IDE/PLC 검증 대기 |
| `0x202B` | AxisInfo | 8B ACK 보존·검증 | descriptor 검증 후 8B ACK | source/자동 테스트 완료, PLC 검증 대기 |

### Single Axis

| ID | 기능 | C# DLL | canonical tracked LASAL | 판정 |
|---:|---|---|---|---|
| `0x2023` | Power | DINT 16-byte request | handler body 보존 | runtime `-5`, typed RT migration 대기 |
| `0x2024` | Reset | 9-byte request | handler body 보존 | runtime `-5`, typed RT migration 대기 |
| `0x2022` | Stop | DINT 24-byte request | handler body 보존 | runtime `-5`, typed RT migration 대기 |
| `0x2028` | ReadStatus | 12B typed result | handler body 보존 | runtime `-5`, typed RT migration 대기 |
| `0x202E` | ReadPosition | 8B DINT typed result | depth-8 queue와 typed RtWork first path | source 반영, IDE/network/PLC golden 대기 |
| `0x209F` | MoveAbsoluteEx | DINT 40-byte request | handler body 보존 | runtime `-5`, typed RT migration 대기 |
| `0x20A0` | MoveRelativeEx | DINT 40-byte request | handler body 보존 | runtime `-5`, typed RT migration 대기 |
| `0x20A2` | MoveVelocityEx | DINT 32-byte request | handler body 보존 | runtime `-5`, typed RT migration 대기 |

source 첫 client는 `LMCAxis1`로 바뀌지만 현재 network의 첫 link는 아직
`TCPMotionInterface1.LMCAxis -> _LMCAxis1.Control`이다. LASAL IDE 적용에서
`TCPMotionInterface1.LMCAxis1 -> _LMCAxis1.Control`로 맞추기 전에는 axis 1
client 연결 완료로 판정하지 않는다. `LMCAxis2..4` link는 유지한다.

### Group

| ID | 기능 | C# DLL | canonical tracked LASAL | 판정 |
|---:|---|---|---|---|
| `0x20D2` | GetGroupMembersInfo | exact 1350B typed parser | descriptor/name 4축 1350B response | source/자동 테스트 완료, PLC 검증 대기 |
| `0x2047` | GroupEnable | request/ACK parser | handler body 보존 | runtime `-5`, typed RT migration 대기 |
| `0x2048` | GroupDisable | request/ACK parser | handler body 보존 | runtime `-5`, typed RT migration 대기 |
| `0x2049` | GroupReset | request/ACK parser | deterministic unsupported `-5` | P0: 승인된 LASAL reset semantics 필요 |
| `0x2045` | GroupReadStatus | 12B typed result, payload에도 descriptor | handler body 보존 | runtime `-5`, typed RT migration 대기 |
| `0x2085` | GroupStop | DINT 24-byte request | deterministic unsupported `-5` | P0: 승인된 LASAL stop semantics 필요 |
| `0x20A4` | MoveLinearAbsoluteEx | DINT 104-byte request, public mode options | handler body 보존 | runtime `-5`, group profile/typed RT migration 대기 |
| `0x2051` | GroupReadActualPosition | coordinate request + exact 68B DINT[16] typed result | handler 없음 | PC 완료 / LASAL mapping·handler·PLC 검증 필요 |
| `0x20E7` | SetKinTransformCartesian4Axis | exact 1320B captured-profile serializer | handler/large staging 없음 | PC 완료 / LASAL handler·PLC 검증 필요 |

## 현재 코드의 주요 결함

### Response parser

1. 4-byte ACK와 8-byte ACK 구분은 2026-07-10에 교정했다.
   - 4-byte ACK: status/error가 payload offset `0`/`2`
   - 8-byte ACK: handle/reserved 뒤 payload offset `4`/`6`
   - command별 구조가 아닌 payload 길이 기반 분기이므로 structured/value
     response를 generic ACK parser에 넣으면 안 된다.
2. `GetGroupMembersInfo`, AxisInfo와 typed read parser는 교정했고 malformed
   payload는 `InvalidDataException`으로 구분한다.
3. RPC init 24-byte payload의 첫 DWORD `64` 의미는 아직 확정되지 않았고,
   실제 UDP callback payload도 캡처되지 않았다. PC close는 ACK 오류를
   호출자에게 전달하면서 transport cleanup을 수행하도록 교정했다.
4. PC connection은 timeout, 상태 전이, transport/close 오류, 취소 가능한
   async path와 callback source-address 검증을 제공한다. 이 기능은 LASAL의
   session ownership 정책을 대신하지 않는다.
5. timeout 또는 in-flight cancellation 이후 transport는 폐기돼 재사용되지
   않는다. queued cancellation은 active RPC를 중단하지 않고, reconnect 뒤
   stale axis/group descriptor도 거부한다.

### WPF test app

1. `8388608` 배율은 caller-side 23-bit encoder dummy profile로 명시했다.
   DLL 자동 변환이 아니며 production caller는 PLC 설정에 맞는 UNIT/scale로
   교체해야 한다.
2. `Result()`와 polling read는 `IsFrameValid`/`IsSuccess`를 검사하며 실패값
   `0`을 상태 판정에 사용하지 않는다. Power/Standstill 판정도 PMAS mask가
   아니라 LASAL `_LMCAXIS_STATUS.PowerOn` bit 0과 `Standstill` bit 25를 쓴다.
3. callback state/error와 raw payload/endpoint/time을 UI thread로 marshal해
   표시한다. 실제 payload 명세가 없으므로 typed event로 해석하지 않는다.
4. 모든 네트워크/polling path를 async/cancellation으로 바꾸고 Cancel 버튼과
   비동기 delay를 적용했다. axis/group lookup도 취소 가능한 async factory를
   사용하고 window 종료 시 RPC close/dispose 완료를 기다린다.
5. `GroupReadActualPosition`, `SetKinTransformCartesian4Axis`, group coordinate/
   transition/buffer option을 UI에 노출했다.

WPF Debug/Release build는 성공했지만 LASAL handler와 PLC 검증은 아직 없다.
따라서 UI 기능이 존재한다는 이유로 실제 motion을 production-safe로
판정하면 안 된다.

### Input과 public API

- object name은 null/빈 값, printable ASCII 이외 문자와 79 bytes 초과를
  frame 생성 전에 명시적으로 거부한다.
- group position 배열은 null 없이 1..16개로 검증하고 남는 wire slot만
  0 padding한다. 16개 초과와 잘못된 enum은 전송 전에 거부한다.
- `MoveLinearAbsoluteEx`의 coordinate system, transition mode, buffer mode와
  execute는 `LMCGroupMotionOptions`로 공개했다.
- callback port `0`은 UDP listener의 실제 ephemeral port를 registration
  frame에 쓰도록 교정했다.
- typed result, request/response golden과 LASAL static contract suite가 추가됐다.

## 캡처에서 추가로 확정한 구조

### `0x20D2 GetGroupMembersInfo`

Response payload 1350 bytes:

| Offset | 구조 |
|---:|---|
| 0 | AxisReference `UINT16[16]` |
| 32 | DeviceId `UINT16[16]` |
| 64 | Status `UINT16` |
| 66 | ErrorId `INT16` |
| 68 | AxisName `CHAR[16][80]` |
| 1348 | NumAxes `BYTE` |
| 1349 | padding |

### `0x2051 GroupReadActualPosition`

Request payload는 coordinate-system DINT, enable BYTE, padding 3 bytes다.
Response payload는 `double[16]`, status, error ID, padding 4 bytes다.
padding을 17번째 position으로 해석하면 안 된다.

LASAL-DINT v1 local contract는 response를 exact 68 bytes,
`DINT[16] + UINT16 status + INT16 error`로 확정했다. PC typed parser는 이
형태만 받고 captured legacy 136-byte LREAL response를 거부한다. 남은 작업은
LASAL에서 PMAS coordinate enum(None/ACS/MCS/PCS)을 실제 robot coordinate
index로 명시적으로 mapping하고 같은 68-byte response를 만드는 것이다.

### `0x20E7 SetKinTransformEx/Cartesian`

1320-byte payload는 legacy `MMC_SETKINTRANSFORM_IN`이 아니라
`MMC_SETKINTRANSFORMEX_IN`/Cartesian wrapper와 맞는다.

| Payload offset | 구조 |
|---:|---|
| 0..639 | `MC_KIN_NODE_DEF[16]`, 각 40 bytes |
| 640 | `iNumAxes = 4` |
| 644..1303 | `MC_KIN_REF` union remainder |
| 1304 | `eKinType = 0` Cartesian |
| 1308 | buffer mode = `2` |
| 1312 | execute BYTE = `1` |
| 1313..1319 | ABI padding |

두 pcap의 application frame은 byte-identical해서 unique sample은 1개다.
구조는 captured Cartesian4 serializer 개발에 충분하다. PC 공개 API는
고유 X/Y/Z/U axis reference, identity shift, `Buffered(2)`로 제한해 구현했다.
축 수, 계수, node type, buffer mode를 바꾼 generic 기능을 열려면 추가
캡처가 필요하다. LASAL에는 1320-byte command용 large-command staging과
apply handler가 아직 없다.

## P0 개발 목록

| ID | 작업 | 완료 조건 |
|---|---|---|
| P0-01 | canonical LASAL project 확정 | tracked `Elmo_EtherCAT_Test_4Axis`에 승인된 변경만 반영하고 untracked `_Edit` 의존 제거 |
| P0-02 | LASAL-DINT protocol v1 명세 고정 | 모든 23 command의 header, request, response, type, error schema 문서화 |
| P0-03 | RPC lifecycle 구현 | LASAL에 `0x8080`, `0x405C`, `0x405D` handler/response 추가, 요청 `dSock`으로 응답 |
| P0-04 | target dispatcher 구현 | LASAL actual object name lookup 후 opaque descriptor가 `_LMCAxis1~4`/robot으로 정확히 route |
| P0-05 | 기존 C# command의 LASAL handler 완성 | Power/Reset/Stop/read/move/group command가 DINT contract로 실제 실행되고 미구현 command는 deterministic error 반환 |
| P0-06 | response parser 교정 | 4B/8B ACK, lookup, value, AxisInfo, 0x20D2를 command별 parser로 처리 |
| P0-07 | WPF test app 안전 수정 | 23-bit dummy가 caller profile임을 명시, `IsFrameValid`/`IsSuccess` 확인, 실패값 0과 PMAS state mask 사용 금지 |
| P0-08 | 자동화 test 기반 | request golden bytes, captured response parser, malformed frame, fake TCP server integration test 추가 |
| P0-09 | LASAL receive/실행 context 안전성 | TCP stack은 non-RT task 하나가 소유하고 callback은 frame 복사/queue까지만 수행하며 실제 `_LMCAxis` 명령은 동일 core `RtWork`에서 실행; bound/partial/combined frame 검증 |

진행 상태:

- P0-01: 이번 변경부터 tracked project를 canonical 변경 대상으로 사용
- P0-02: PC 23-command request/response schema와 UNIT 책임 문서화 완료.
  LASAL `0x2051` coordinate mapping, `0x20E7` apply/ACK와 callback event
  payload는 server 구현/캡처 뒤 확정 필요
- P0-03: PC/LASAL phase-1 코드 반영, LASAL IDE와 PLC E2E 검증 대기
- P0-04: actual-name registry, descriptor 1..4와 4축 client wiring source 반영,
  LASAL IDE/PLC 검증 대기
- P0-05: single-axis와 일부 group DINT handler body는 반영했다. 실제
  client-call runtime은 source-first 단계에서 `0x202E` 하나만 허용하고 나머지는
  `-5`로 차단했다. GroupReset/GroupStop semantics와 `0x2051`/`0x20E7`
  handler는 남음
- P0-06: exact 4B/8B ACK, typed read, AxisInfo, `0x20D2`, `0x2051` parser와
  legacy/truncated shape tests 완료
- P0-07: WPF dummy profile 표기, response 실패 판정, LASAL PowerOn/Standstill
  mask, async/cancel, raw callback/state 표시와 신규 group UI 반영 완료.
  실제 PLC 장시간 polling/motion 검증은 남음
- P0-08: request golden, captured/synthetic parser, malformed frame와 fake RPC/
  UDP callback/lifecycle 통합 test 42/42 PASS. source-first generated table/offset와
  기존 axis 2~4 link는 `RunLasalContract` PASS. strict
  `RunLasalNetworkContract`는 `LMCAxis1` link IDE 적용 전까지 의도적으로 pending
- P0-09: source-first로 `Response -> depth-8 queue -> CyWork -> typed RtWork ->
  CyWork response` 경로와 `0x202E` first command를 반영한다. 다른 client-call은
  migration 전 `-5`로 차단한다. LASAL IDE class model, `LMCAxis1` network link,
  CyclicTime/RealTime 1 ms, same-core와 PLC 검증은 남았다. 상세 적용 경계는
  `LASAL_SOURCE_QUEUE_AND_NETWORK_APPLY_PLAN_2026-07-10.md`에 기록했다.

tracked `.st`는 CodeGenerator export이므로 새 session/accumulator 변수를
LASAL IDE class model에 등록하고 재생성해야 P0-03 변경이 영구 보존된다.

P0가 끝나기 전에는 현재 WPF test app으로 실제 motion을 수행하지 않는 것이
맞다.

## P1 개발 목록

| ID | 작업 | 완료 조건 |
|---|---|---|
| P1-01 | `GetGroupMembersInfo` typed API (source/test 완료) | `LMCGroupMembersInfoResult`가 16축 reference/device/name/count/status/error를 반환 |
| P1-02 | `GroupReadActualPosition(0x2051)` (PC 완료) | PC exact 68B DINT result와 WPF 경로 완료; LASAL coordinate mapping/handler, PLC 재캡처 필요 |
| P1-03 | `SetKinTransformCartesian4Axis(0x20E7)` (PC 완료) | PC exact 1320B captured profile 완료; LASAL large staging/apply/ACK와 PLC 재캡처 필요 |
| P1-04 | group motion mode API (PC 완료) | PC coordinate/transition/buffer/execute validation 완료; LASAL semantics/PLC 검증 필요 |
| P1-05 | typed read 결과 (완료), lookup 결과 (부분) | 정상값 0과 실패를 구분하고 response/error context 보존 |
| P1-06 | session/ownership | LASAL dSock session table, axis/group ownership, busy error, disconnect/timeout cleanup |
| P1-07 | callback 검증 | PC raw listener 완료; 실제 payload 캡처 후에만 LASAL sender/typed parser 추가 |
| P1-08 | 실제 PLC 재캡처 | handshake, lookup, 4축 routing, 성공/실패 ACK, read/motion/group packet 저장 및 문서 갱신 |

`0x2051`과 `0x20E7`의 PC 코드는 구현됐지만 실행할 LASAL handler가 없다.
실제 motion 경로는 LASAL P0와 두 handler가 끝나기 전까지 사용할 수 없다.

## P2 개발 목록

- configurable timeout, cancellation/async, state/error 분리와 session
  generation/stale-handle 차단: PC 반영
- group array length/enum validation: PC 반영; application별 motion range는 caller 정책
- typed callback: 실제 payload capture 전에는 구현 금지, raw event 유지
- test app UI-thread blocking 제거/취소/raw callback/group UI 반영 완료;
  반복 object lookup 최소화와 실제 PLC 검증은 남음
- assembly version `0.9.0.0`과 PC preview release manifest 반영 완료
- package DLL/EXE를 current source에서 Release rebuild하고 SHA-256 기록 완료;
  release artifact commit에서 ignored `bin` DLL도 명시적으로 추적
- sample/function TXT/API list/packet map 문서는 current PC API로 교체 완료
- `LMC_API_Delivery`, package, packet analysis의 링크와 용어 최종 통일

## 배포 패키지 상태

`LMC_Library/LMC_API/LMC_API`에는 현재 PC preview 산출물을 조립했다.

- README, API list, packet map, sample과 function/Command ID TXT를 현재
  23-command PC public API와 LASAL 미구현 범위에 맞췄다.
- `0.9.0.0` DLL/EXE를 current Release source에서 재빌드했고
  `RELEASE_MANIFEST.md`에 size와 SHA-256을 기록했다.
- old tracked binary를 제거하고 새 이름의 package/Delivery DLL과 test-app
  DLL/EXE를 release artifact commit에서 명시적으로 추적했다.

현재 산출물은 PC API source/test용 preview다. LASAL/PLC 검증 뒤 실제 release를
만들 때는 확정 source commit으로 다시 빌드하고 manifest의 source와 SHA-256을
갱신해야 한다.

## 설계 결정 및 미결정 항목

1. canonical LASAL source
   - 결정 완료: Git tracked `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`만 개발
     대상으로 사용하고 `_Edit` 복제본은 무시한다.
2. protocol identity
   - 권장: PMAS wire-compatible라고 부르지 말고 `LASAL-DINT v1`로 명시한다.
3. `0x2051` response type
   - 결정 완료: LASAL-DINT v1 exact 68B `DINT[16]+status/error`.
   - LASAL handler와 coordinate enum/index mapping은 남음.
4. `0x20E7` 적용 방식
   - 결정 완료: PC는 exact 1320-byte captured Cartesian4 serializer를 사용.
   - LASAL large-command staging/apply 방식은 함께 설계·구현해야 함.
5. callback event protocol
   - transport는 Maestro manual 기준 UDP로 확정했다.
   - event mask bit, datagram payload, 재전송/유실 정책은 실제 callback
     capture 또는 승인된 LASAL-local 명세 후 확정한다.
6. multi-PC ownership
   - 읽기는 공유하고 motion/control은 axis/group owner만 허용하는 LASAL
     server 정책을 기본안으로 한다. PC DLL만으로 완료 처리하지 않는다.

## 권장 실행 순서

1. source-first `LMCAxis1`, depth-8 queue, CyWork와 typed `0x202E` mailbox 정적 검증
2. LASAL IDE class model에 새 client/변수를 등록하고 CodeGenerator 재생성
3. network에 `LMCAxis1` link, CyclicTime/RealTime 1 ms, same-core,
   `Config=0`, `MaxConnections=1` 적용
4. LASAL IDE compile, PLC download, RPC/lookup/descriptor 1..4 재캡처
5. ReadActualPosition -> read/admin -> Power/Stop -> Move 순서로 typed RT migration/E2E
6. 나머지 single-axis command E2E
7. GroupReset/GroupStop의 승인된 LASAL semantics 구현
8. group lookup/members/enable/status/linear/stop E2E
9. `0x2051` coordinate mapping/68B response와 `0x20E7` large staging/apply 구현
10. 실제 UDP callback payload 캡처 뒤 LASAL sender/typed parser 구현
11. multi-PC ownership, 실제 pcap, release package 완료

## Definition of Done

각 command는 아래를 모두 만족해야 완료로 표시한다.

1. C# public API 또는 명시된 internal operation이 존재한다.
2. request golden bytes가 protocol 문서와 일치한다.
3. LASAL parser가 같은 header/type/offset으로 수신한다.
4. 올바른 axis/group object로 dispatch하고 실제 동작 또는 값을 만든다.
5. success와 error response schema가 문서화되고 parser test를 통과한다.
6. fake server integration test와 실제 PLC smoke test를 통과한다.
7. Wireshark 재캡처가 request/response 문서와 일치한다.
8. API list, packet map, test app, package 문서를 같은 변경에서 갱신한다.
9. C# build와 `git diff --check`를 통과한다.

## 범위에서 제외

- 캡처와 실제 LASAL 요구가 없는 Maestro 전체 API 복제
- 제거된 `LMC_*Cmd` method alias 복구
- DLL 내부 자동 unit conversion
- `PowerMembers` 같은 반복 helper를 protocol command로 추가

## 관련 문서

- [Current API list](../../LMC_API/LMC_API/docs/API_LIST.md)
- [Current C# packet map](../../LMC_API/LMC_API/docs/LMC_PACKET_MAP.md)
- [PMAS packet analysis](../../LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md)
- [API structure decision](API_STRUCTURE_DECISION_2026-07-09.md)
- [Response model](RESPONSE_MODEL_DESIGN_2026-07-09.md)
- [RPC packet decision](RPC_CONNECTION_PACKET_DECISION_2026-07-09.md)
- [RPC and UDP callback implementation](RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md)
- [UNIT conversion manual](UNIT_CONVERSION_MANUAL_2026-07-10.md)
- [LASAL command queue / RtWork design](LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md)
- [Session management design](SESSION_MANAGEMENT_DESIGN_2026-07-09.md)
- [History handoff](../../../docs/history/260710/99_analysis_summary.md)

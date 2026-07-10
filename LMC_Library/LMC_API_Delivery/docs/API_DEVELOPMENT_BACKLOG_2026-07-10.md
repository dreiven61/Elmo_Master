# LASAL Motion Control Lib API Development Backlog

Date: 2026-07-10

Baseline: `c65c56a`

Status: Open

## 결론

현재 API 개발은 완료 상태가 아니다.

Wireshark 자료에는 고유 command ID 23개가 있고 C# DLL에는 그중 21개의
request builder 또는 public 호출 경로가 있다. 2026-07-10에 Git 추적 중인
LASAL `TCPMotionInterface`에 RPC lifecycle phase-1 코드를 반영했지만 LASAL
IDE compile과 실제 PLC 재캡처가 아직 없다. motion handler와 target
dispatcher도 현재 DINT 계약과 맞지 않으므로 end-to-end 완료 API는 여전히
0개다.

남은 작업은 `0x2051`과 `0x20E7` 두 함수 추가만이 아니다. 먼저 아래 P0를
끝내야 한다.

1. canonical LASAL source 확정
2. RPC/header/DINT/response 계약 통일
3. 4축 및 group target dispatch
4. false-success와 response parser 오류 제거
5. 테스트 앱 단위·성공 판정 수정
6. byte-level 자동화 테스트와 실제 PLC 캡처 확보

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
    C --> D["_LMCAxis1..4 및 _LMCRobot"]
    E["PMAS Wireshark capture"] -. "LREAL/REAL 기준 근거" .-> B
    E -. "legacy wire 기준" .-> C
    C -. "현재 header, type, command 불일치" .-> B
```

현재 C#은 LASAL DINT 전용 protocol을 의도하지만 tracked LASAL과 dummy는
legacy header/LREAL 계열이고 `_Edit`는 두 계약이 섞인 hybrid다.

## Command 구현 매트릭스

### Connection과 lookup

| ID | 기능 | C# DLL | tracked / `_Edit` LASAL | 판정 |
|---:|---|---|---|---|
| `0x8080` | Session Init | frame와 응답 대기 구현 | tracked 단일-session response 코드 반영 | P0: LASAL IDE/PLC 검증 대기 |
| `0x405C` | Callback Register | UDP listener, 실제 bound port, 4B ACK parser | tracked endpoint 저장/ACK 코드 반영 | P0: event 송신 제외, PLC 검증 대기 |
| `0x405D` | Close | 재연결/종료 frame 구현 | tracked ACK 후 state clear 코드 반영 | P0: PLC 검증 대기 |
| `0x103C` | Axis lookup | reference parser 구현 | tracked 없음, `_Edit`는 a01~a04 고정 매핑 | P0: canonical mapping 필요 |
| `0x1042` | Group lookup | reference parser 구현 | 모두 없음 | P0: 모든 Group 생성 차단 |
| `0x202B` | AxisInfo | request 구현, response 폐기 | tracked 없음, `_Edit` 고정 ACK | P1: response 검증 필요 |

### Single Axis

| ID | 기능 | C# DLL | tracked / `_Edit` LASAL | 판정 |
|---:|---|---|---|---|
| `0x2023` | Power | DINT 16-byte request | tracked는 `0x2081/82`, `_Edit`는 실행 | P0: ID/response/axis dispatch 통일 |
| `0x2024` | Reset | 9-byte request | tracked는 `0x2083`, `_Edit`는 응답 없음 | P0: timeout과 execute semantics 해결 |
| `0x2022` | Stop | DINT 24-byte request | tracked는 `0x2084`, `_Edit`는 인자 무시·응답 없음 | P0: 실제 인자와 ACK 구현 |
| `0x2028` | ReadStatus | value parser 구현 | tracked 없음, `_Edit` 실행 주석 | P0: value/error response 구현 |
| `0x202E` | ReadPosition | DINT parser 구현 | 모두 구형 response header | P0: stream desync 방지와 DINT response 확정 |
| `0x209F` | MoveAbsoluteEx | DINT 40-byte request | 모두 LREAL offset 사용 | P0: 오동작/false-success 위험 |
| `0x20A0` | MoveRelativeEx | DINT 40-byte request | tracked 없음, `_Edit` 실행 주석 | P0: 실제 motion/ACK 구현 |
| `0x20A2` | MoveVelocityEx | DINT 32-byte request | tracked 없음, `_Edit` 실행 주석 | P0: 실제 motion/ACK 구현 |

두 LASAL 프로젝트 모두 network상 `TCPMotionInterface1.LMCAxis`가
`_LMCAxis1.Control` 하나에만 연결된다. a02~a04 reference를 lookup해도
축 2~4로 dispatch되지 않는다.

### Group

| ID | 기능 | C# DLL | tracked / `_Edit` LASAL | 판정 |
|---:|---|---|---|---|
| `0x20D2` | GetGroupMembersInfo | request 구현, 잘못된 ACK parser | 모두 없음 | P0/P1: server와 typed parser 필요 |
| `0x2047` | GroupEnable | request 구현 | tracked 없음, `_Edit` 실행 주석 | P0: no-op 제거 |
| `0x2048` | GroupDisable | request 구현 | tracked 없음, `_Edit` 실행 주석 | P0: no-op 제거 |
| `0x2049` | GroupReset | request 구현 | tracked 없음, `_Edit` 실행 주석 | P0: 4-byte ACK error 처리 |
| `0x2045` | GroupReadStatus | value parser, request payload 첫 DINT=`0` | 고립 handler 있음 | P0: 캡처의 group handle `0x0100`과 계약 결정 |
| `0x2085` | GroupStop | DINT 24-byte request | tracked 없음, `_Edit` 실행 주석 | P0: 실제 stop/ACK 구현 |
| `0x20A4` | MoveLinearAbsoluteEx | DINT 104-byte request, mode 고정 | 모두 312-byte LREAL 기대 | P0: no-op false-success 위험 |
| `0x2051` | GroupReadActualPosition | command 상수만 있음 | 모두 없음 | P1: API/builder/vector parser/server 구현 |
| `0x20E7` | SetKinTransformEx/Cartesian | 상수와 API 없음 | 모두 없음 | P1: 1320-byte serializer/server 구현 |

## 현재 코드의 주요 결함

### Response parser

1. 4-byte ACK와 8-byte ACK 구분은 2026-07-10에 교정했다.
   - 4-byte ACK: status/error가 payload offset `0`/`2`
   - 8-byte ACK: handle/reserved 뒤 payload offset `4`/`6`
   - command별 구조가 아닌 payload 길이 기반 분기이므로 structured/value
     response를 generic ACK parser에 넣으면 안 된다.
2. `GetGroupMembersInfo()`가 1350-byte structured response를 ACK로
   파싱한다.
   - captured reference `2`/`3`을 CommandStatus/ErrorId로 오인한다.
   - axis references, device IDs, names와 member count를 반환하지 않는다.
3. `AxisInfo` response는 생성자에서 완전히 버린다.
4. `ReadStatus`, `GetActualPosition`, `GroupReadStatus`는 value만 반환하고
   command/error tail을 충분히 해석하지 않는다.
5. value parser 실패도 숫자 `0`을 반환해 정상값 0과 구분되지 않는다.
6. RPC init 24-byte payload의 첫 DWORD `64` 의미는 아직 확정되지 않았고,
   실제 UDP callback payload도 캡처되지 않았다. close ACK는 파싱·보관하지만
   연결 종료 중 발생한 오류는 현재 호출자에게 throw하지 않는다.

### WPF test app

1. 모든 motion 값에 기본 `8388608`을 곱한다.
   - delivery 정책은 caller가 LASAL internal DINT를 넘기는 것이다.
   - 현재 a01~a04 Motion Network는 position/speed/accel/decel에 `DEG=10000`
     profile을 사용하며 nonzero jerk 물리 변환은 별도 검증이 필요하다.
   - group/robot은 kinematic 축별 UNIT profile을 따로 확정해야 한다.
2. `Result()`는 `response.Status`만 검사하고 `IsFrameValid`, `IsSuccess`,
   `ErrorId`를 무시한다.
3. status read 실패값 `0`을 Power ON 성공으로 판단할 수 있다.
4. callback event/error를 표시하지 않는다.
5. 모든 네트워크 호출과 polling이 UI thread에서 실행된다.

현재 상태로 실제 motion test를 수행하면 잘못된 단위값 전송과 false-success
판정 위험이 있다.

### Input과 public API

- name이 null/비 ASCII/79 bytes 초과여도 명확한 validation 없이
  빈 문자열·치환·truncate가 일어난다.
- group position 배열이 null/짧으면 0 padding, 16개 초과면 조용히
  truncate된다.
- `MoveLinearAbsoluteEx`의 coordinate system, transition mode, buffer mode는
  public 인자가 아니며 현재 `0, 0, 1, 1`로 고정된다.
- callback port `0`은 UDP listener의 실제 ephemeral port를 registration
  frame에 쓰도록 교정했다.
- typed result와 자동화 test project가 없다.

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

LASAL DINT API로 구현할 때는 response도 DINT[16]으로 새로 정의할지
captured LREAL[16]을 유지할지 PC/PLC 양쪽에서 먼저 결정해야 한다.

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
구조는 serializer 개발에 충분하지만 축 수, 계수, node type, buffer mode를
바꾼 추가 캡처가 필요하다.

## P0 개발 목록

| ID | 작업 | 완료 조건 |
|---|---|---|
| P0-01 | canonical LASAL project 확정 | tracked `Elmo_EtherCAT_Test_4Axis`에 승인된 변경만 반영하고 untracked `_Edit` 의존 제거 |
| P0-02 | LASAL-DINT protocol v1 명세 고정 | 모든 23 command의 header, request, response, type, error schema 문서화 |
| P0-03 | RPC lifecycle 구현 | LASAL에 `0x8080`, `0x405C`, `0x405D` handler/response 추가, 요청 `dSock`으로 응답 |
| P0-04 | target dispatcher 구현 | a01~a04가 `_LMCAxis1~4`, v01이 group/robot 객체로 정확히 route |
| P0-05 | 기존 C# command의 LASAL handler 완성 | Power/Reset/Stop/read/move/group command가 DINT contract로 실제 실행되고 미구현 command는 deterministic error 반환 |
| P0-06 | response parser 교정 | 4B/8B ACK, lookup, value, AxisInfo, 0x20D2를 command별 parser로 처리 |
| P0-07 | WPF test app 안전 수정 | field별 unit profile, `IsFrameValid`/`IsSuccess` 확인, 실패값 0 사용 금지 |
| P0-08 | 자동화 test 기반 | request golden bytes, captured response parser, malformed frame, fake TCP server integration test 추가 |
| P0-09 | LASAL receive 안전성 | `udSize`/payload length/buffer bound 검증, partial/combined frame 처리, no-op success 제거 |

진행 상태:

- P0-01: 이번 변경부터 tracked project를 canonical 변경 대상으로 사용
- P0-02: UNIT 책임과 RPC packet 문서화 완료, 나머지 command schema 미완료
- P0-03: PC/LASAL phase-1 코드 반영, LASAL IDE와 PLC E2E 검증 대기
- P0-06: 4B/8B ACK 분기 완료, `0x20D2`와 typed/value parser 미완료
- P0-09: receive buffer/header와 단일-socket TCP stream accumulator 반영,
  socket별 accumulator와 no-op command 제거 미완료

tracked `.st`는 CodeGenerator export이므로 새 session/accumulator 변수를
LASAL IDE class model에 등록하고 재생성해야 P0-03 변경이 영구 보존된다.

P0가 끝나기 전에는 현재 WPF test app으로 실제 motion을 수행하지 않는 것이
맞다.

## P1 개발 목록

| ID | 작업 | 완료 조건 |
|---|---|---|
| P1-01 | `GetGroupMembersInfo` typed API | `LMCGroupMembersInfoResult`가 16축 reference/device/name/count/status/error를 반환 |
| P1-02 | `GroupReadActualPosition(0x2051)` | coordinate enum request, vector result, error parser, LASAL handler, tests, WPF 노출 |
| P1-03 | `SetKinTransformEx/Cartesian(0x20E7)` | explicit 1320-byte serializer, 4축 Cartesian builder, LASAL apply path, ACK parser, tests |
| P1-04 | group motion mode API | coordinate/transition/buffer/superimposed 정책을 public API와 LASAL semantics에 연결 |
| P1-05 | typed read/lookup 결과 | 정상값 0과 실패를 구분하고 response/error context 보존 |
| P1-06 | session/ownership | dSock session table, axis/group ownership, busy error, disconnect/timeout cleanup |
| P1-07 | callback 검증 | 실제 transport/payload 캡처 후 typed callback parser와 test app 표시 |
| P1-08 | 실제 PLC 재캡처 | handshake, lookup, 4축 routing, 성공/실패 ACK, read/motion/group packet 저장 및 문서 갱신 |

`0x2051`과 `0x20E7`은 P0 protocol과 LASAL routing이 끝난 뒤 구현한다.
그 전에 C# 함수만 추가하면 실행할 server가 없고 false-success가 늘어난다.

## P2 개발 목록

- configurable timeout, cancellation/async API와 connection state 개선
- input validation: name encoding/length, direction, array length, enum range
- Phase 2 typed result와 Phase 3 callback model 완료
- test app의 UI-thread blocking 제거와 반복 object lookup 최소화
- assembly version과 release manifest 추가
- package DLL/EXE를 current source에서 재빌드하고 SHA-256/source commit 기록
- stale sample과 legacy function TXT를 current public API로 교체
- `LMC_API_Delivery`, package, packet analysis의 링크와 용어 통일

## 배포 패키지 상태

`LMC_Library/LMC_API/LMC_API`는 현재 전달 가능한 상태가 아니다.

- README 일부는 새 DLL을 설명하지만 sample은 제거된 `LMC_*Cmd`,
  `PowerMembers`, `SetKinTransformCartesian4Axis`를 호출한다.
- function/Command ID TXT는 미구현 API를 최종 제공 API처럼 기록한다.
- package의 untracked DLL/EXE는 current rebuild 산출물과 일치하지 않는다.
- old tracked binary는 삭제 상태이고 새 binary는 ignore되어 fresh clone에는
  전달 DLL이 없을 수 있다.

기존 package 변경은 사용자 작업이므로 이 문서 작업에서는 수정하지 않는다.
기능과 E2E 검증이 끝난 뒤 별도 release/package commit으로 재생성한다.

## 미결정 설계 항목

1. canonical LASAL source
   - 권장: tracked 프로젝트를 기준으로 `_Edit` 변경을 검토·이식한다.
2. protocol identity
   - 권장: PMAS wire-compatible라고 부르지 말고 `LASAL-DINT v1`로 명시한다.
3. `0x2051` response type
   - captured LREAL[16] 유지 또는 DINT[16] local contract 중 하나를 양쪽에서
     동시에 결정한다.
4. `0x20E7` 적용 방식
   - exact 1320-byte captured serializer와 compact LASAL-local command를
     혼용하지 않는다. 어떤 방식을 쓸지 먼저 결정한다.
5. callback event protocol
   - transport는 Maestro manual 기준 UDP로 확정했다.
   - event mask bit, datagram payload, 재전송/유실 정책은 실제 callback
     capture 또는 승인된 LASAL-local 명세 후 확정한다.
6. multi-PC ownership
   - 읽기는 공유하고 motion/control은 axis/group owner만 허용하는 설계를
     기본안으로 한다.

## 권장 실행 순서

1. WPF motion test 중지 및 P0-07 수정
2. tracked LASAL을 canonical source로 확정
3. protocol v1 byte map과 golden tests 작성
4. RPC + lookup + axis dispatch 구현
5. Power -> ReadStatus -> MoveAbsolute -> Stop 최소 축 경로 E2E
6. 나머지 single-axis command E2E
7. group lookup/members/enable/status/linear/stop E2E
8. `0x2051` 구현
9. `0x20E7` 및 Prepare Group MCS 구현
10. multi-PC/callback/실제 pcap/release package 완료

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
- [Session management design](SESSION_MANAGEMENT_DESIGN_2026-07-09.md)
- [History handoff](../../../docs/history/260710/99_analysis_summary.md)

# Axis Reference LASAL-native dormant 계약

작성일: 2026-07-31

## 1. 결론

새 기능 이름은 `ReferenceAxis`이며 LASAL-local Admin command `0x7D13`
`StartAxisReference`로 정의한다. 이 기능의 backend 후보는 `_LMCAxis.MoveReference()`다.
Maestro/MMCLib의 `HomeDS402` 또는 `HomeDS402Ex`와 같은 기능이라고 부르지 않는다.

현재 단계에서는 PC SDK의 typed request/response, one-shot 전송 계약과 PLC의 dormant
parser까지만 구현한다. 현재 Motion Network에는 `HWMin`, `HWMax`, `RefSwitch`,
`ZImpulse`, `LatchPos`의 external physical source 연결이 한 건도 없다. 따라서 Admin
capability bit 4는 `0`이고 `_LMCAxis.MoveReference()` 호출은 항상 `0`회여야 하며 WPF
example에는 실행 버튼을 노출하지 않는다.

`StartAxisReference`의 성공 응답은 시작 명령의 수락 ACK일 뿐 reference 완료가 아니다.
완료는 command provenance가 있는 상태에서 `IsReferenced`, `Standstill`, axis error와
position을 여러 cycle에 걸쳐 별도로 확인해야 한다.

## 2. Maestro DS402 Homing과의 차이

Maestro `HomeDS402`는 drive DS402 homing method, position, velocity, acceleration,
distance/torque/time limit를 받는다. `HomeDS402Ex`는 high/low velocity와 detection
velocity/time limit까지 추가한다. 두 기능은 Standstill에서 DS402 homing state machine을
실행하고 Done/Aborted/Error를 반환한다.

현재 LASAL `_LMCAxis.MoveReference()`의 입력은 다음뿐이다.

- `_LMCAXIS_REFMODE` bitfield
- reference position
- search velocity `VRef1`
- backoff velocity `VRef2`
- acceleration
- Z-pulse position window
- jerk

현재 Elmo 1..4는 startup SDO에서 `0x6060=8`, 즉 CSP로 고정된다. runtime
`0x6060/0x6061` PDO는 disabled이고 LMC position controller가 cyclic target position을
계속 소유한다. 따라서 `ReferenceAxis`는 LASAL software reference sequence이며 DS402
Elmo Gold DS402 method `1..14`, `17..30`, `33..35`, torque/detection limit 또는 DS402 mode 전환을 제공하지 않는다.

## 3. v1 semantic 범위

wire에는 raw `_LMCAXIS_REFMODE`를 싣지 않는다. v1 recipe는 다음 두 개만 예약한다.

| Recipe | 값 | 의미 |
|---|---:|---|
| `NegativeReferenceSwitchThenBackoff` | `1` | negative 방향 reference switch 탐색 후 backoff |
| `PositiveReferenceSwitchThenBackoff` | `2` | positive 방향 reference switch 탐색 후 backoff |

이 값은 stable public semantic ID이지 native LASAL bitmask가 아니다. native mode의 정확한
bit 조합은 physical IO와 PLC bench 시험을 통과한 뒤 PLC 내부 mapping으로만 추가한다.

v1에서 지원하지 않는 범위는 다음과 같다.

- raw native mode 조합
- Z-pulse recipe와 `RefLatchPos`
- safety reference bits
- DS402 homing method 번호
- torque 또는 detection-velocity limit
- simulation axis 5..9
- group reference/homing

## 4. Wire 계약

모든 정수는 little-endian이다. outer envelope는 기존 8-byte RPC header를 사용한다.

### 4.1 Request

Command ID는 `0x7D13`, outer `Reference`는 physical axis `1..4`, payload length는
정확히 48 byte이고 전체 frame은 56 byte다.

| Payload offset | Type | Field | 규칙 |
|---:|---|---|---|
| `P+0` | U16 | SchemaVersion | `1` |
| `P+2` | U16 | Flags | `0` |
| `P+4` | U32 | RequestId | nonzero |
| `P+8` | U16 | Recipe | `1` 또는 `2` |
| `P+10` | U16 | Reserved | `0` |
| `P+12` | I32 | ReferencePosition | application units |
| `P+16` | I32 | SearchVelocity | `>0`, application units/s |
| `P+20` | I32 | BackoffVelocity | `>0`, application units/s |
| `P+24` | I32 | Acceleration | `>0`, application units/s^2 |
| `P+28` | I32 | PositionWindow | `>=0`, application units |
| `P+32` | I32 | Jerk | `>=0`, application units/s^3/1000 |
| `P+36` | I32 | MaxTravel | `>0`, PLC-side travel watchdog bound |
| `P+40` | U32 | TimeoutMs | `>0`, PLC-side elapsed-time watchdog bound |
| `P+44` | U32 | ExecuteToken | `0x52464552`, little-endian ASCII `REFR` |

`MaxTravel`과 `TimeoutMs`는 PC wait timeout이 아니다. 연결이 끊겨도 PLC cyclic owner가
reference motion을 독립적으로 정지시키기 위한 mandatory safety input이다. dormant
단계에서는 값의 형식만 검증하고 어떤 motion에도 사용하지 않는다.

### 4.2 Response

payload length는 정확히 24 byte이고 전체 frame은 32 byte다.

| Payload offset | Type | Field | 규칙 |
|---:|---|---|---|
| `P+0` | U16 | SchemaVersion | `1` |
| `P+2` | U16 | ResponseFlags | `0` |
| `P+4` | U16 | CommandStatus | `0=accepted`, `1=failure` |
| `P+6` | I16 | ErrorId | pre-native failure `-31000`, native reject `-6` |
| `P+8` | U32 | RequestId | request echo |
| `P+12` | U32 | DetailCode | Admin detail |
| `P+16` | U16 | Recipe | request recipe echo |
| `P+18` | U16 | Reserved | `0` |
| `P+20` | U32 | NativeCommandState | full `_LMCAXIS_CMDERROR` bitfield |

조합 규칙은 다음과 같다.

- accepted: echoed recipe, `NativeCommandState=0`
- native 호출 전 rejection: echoed recipe, `NativeCommandState=0`
- `NativeCommandRejected(11)`: `ErrorId=-6`, echoed recipe,
  `NativeCommandState<>0`
- 모순되는 correlated response: confirmed rejection으로 처리하지 않고 post-write
  outcome uncertain으로 처리한다.

현재 PLC는 exact valid request도 `InvalidState(10)`, `ErrorId=-31000`, native state `0`으로
결정적으로 거부한다.

## 5. SDK 실행 계약

호출자는 current connection/session에서 받은 Admin capability와 명시적 execute token으로
prepared command를 만든다. execute token은 preparation 시 한 번 소비되고 prepared command는
첫 write boundary에서 한 번 소비된다.

prepared command에는 다음 값이 고정된다.

- connection owner와 session generation
- axis handle/reference
- capability snapshot
- request id
- recipe와 모든 reference parameter
- `MaxTravel`, `TimeoutMs`

write boundary 전 validation/cancel은 zero-wire다. write boundary 뒤 timeout, EOF, socket
error, malformed response 또는 result publication failure는 outcome uncertain이다. 이 경우
prepared command를 재전송하지 않고 exact session을 fault 처리한 뒤 reconnect해야 한다.
older session invalidation이 newer reconnect를 종료해서는 안 된다.

current PLC는 capability bit 4를 광고하지 않으므로 public facade는 command frame을 보내기
전에 `NotSupportedException`으로 끝나야 한다.

## 6. PLC activation gate

아래 조건을 모두 만족하기 전 capability bit 4와 native call은 활성화하지 않는다.

1. axis 1..4 각각 `HWMin`, `HWMax`, `RefSwitch`, `ZImpulse`, `LatchPos`의 실제 source,
   input bit, active level, debounce와 단선 동작을 문서화한다.
2. 선택 recipe에 필요 없는 input까지 무조건 요구하지 않고 recipe별 required input을
   명시한다. Z-pulse recipe를 추가하려면 `ZImpulse` 배선을 별도로 입증한다.
3. `NoRefMeth=0`, physical/non-simulation axis, valid ActPosition/controller/reference client를
   PLC runtime에서 확인한다.
4. PowerOn, Standstill, axis error 없음, Emergency 없음 상태를 확인한다.
5. `MasterLock=0`이고 group/profile 및 다른 axis mutation owner와 충돌하지 않는다.
6. handler가 axis realtime thread와 같은 core이고 equal-or-lower priority임을 IDE task map과
   runtime에서 확인한다.
7. application-approved position/velocity/acceleration/window/jerk 범위를 적용한다.
8. PLC cyclic owner가 `MaxTravel`과 `TimeoutMs`를 독립 감시하고 초과 시 controlled Stop을
   실행한다. PC timeout만으로 대체하지 않는다.
9. absolute encoder retain, reference origin 변경과 cold restart 정책을 승인한다.
10. native mode mapping, switch transition, backoff 및 stop 동작을 축별 bench에서 검증한다.

현재 generic axis mutation admission만으로는 axis/group ownership을 완전히 증명하지 못한다.
activation 전에 unified ownership coordinator 또는 검증된 고정 gate 획득 순서가 필요하다.

## 7. 완료·취소·검증 기준

ACK를 reference 완료로 해석하지 않는다. 활성화 이후 완료 판정에는 최소 다음이 필요하다.

- 해당 request가 accepted/running 상태를 실제로 거쳤다는 provenance
- `IsReferenced=1`
- `Standstill=1`
- axis error `0`
- 여러 연속 cycle의 안정 상태
- recipe에 맞는 switch release 상태
- reference/actual position readback

이미 `IsReferenced=1`인 축의 상태만 읽어 새 command 성공을 역추론하면 안 된다. activation
전에 operation generation/ticket 또는 동등한 accepted/running provenance를 확정한다.

취소는 새로운 reference 명령을 보내는 방식이 아니라 기존 controlled `Stop` 경로를 사용한다.
Stop ACK 뒤에도 stable Standstill과 reference 결과를 별도로 판정한다.

현재 dormant slice의 검증은 다음으로 제한한다.

- exact 56-byte request golden bytes와 24-byte response parser
- capability-off zero-wire
- one-shot/concurrent dispatch/no-replay
- native reject full bitfield 보존
- response loss/malformed/publication failure의 exact-session fault
- LASAL exact offset/length/token/parameter validation
- capability `0x00000007` 유지
- `_LMCAxis.MoveReference` 및 모든 axis native/read call 0회
- SourceOnly/full static contract

LASAL IDE Rebuild/Link, implementation smoke, current PLC download, raw packet과 physical axis 시험은
별도 gate이며 이 문서의 source/static 완료로 대체하지 않는다.

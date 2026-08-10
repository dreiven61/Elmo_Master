# Axis Set Position 제한 좌표 보정 설계

작성일: 2026-07-31

## 1. 결론

`SetAxisPosition`은 LASAL-local Admin command `0x7D12`로 정의한다. public API는
native `_LMCAXIS_SETPOSITION` enum을 받지 않고, application-unit actual position과
destination을 함께 맞추는 semantic mode 한 개만 제공한다.

현재 단계에서는 PC SDK의 wire/parser/one-shot 계약, durable intent identity,
read-only outcome query 계약과 PLC의 dormant 요청 파싱 골격까지만 구현한다.
실제 `_LMCAxis.SetPosition()` 호출과 capability bit 3 광고는 아래 activation gate가
모두 확인될 때까지 금지한다. outcome query용 capability bit 5도 PLC ledger와
`0x7D14` route가 IDE에서 구성되기 전에는 광고하지 않는다.

가장 중요한 제한은 다음 두 가지다.

- `LMCAXIS_PAR_RD_SWLIMWINDOW`는 software end-position의 standstill tolerance
  window다. 좌표 변경 최대 점프값이 아니므로 `SetPositionMaxJump` 대신 사용할 수 없다.
- `_LMCAxis.SetPosition()`은 해당 축 realtime thread와 같은 core에서, 그 realtime
  thread와 같거나 낮은 priority로만 호출할 수 있다. 생성된 task table의 TaskId 값만으로
  실제 CPU core와 OS priority 관계를 최종 증명할 수 없다.

따라서 source에 handler가 있어도 현재 PLC 응답은 fail-closed여야 하고 native mutation은
0회여야 한다.

## 2. 근거와 범위

### 2.1 확인된 vendor 계약

- `_LMCAxis.SetPosition(Mode, Position)`은 `_LMCAXIS_CMDERROR`를 반환한다.
- 선택한 native mode는 `LMCAXIS_SET_ACTPOS_APPUNIT_DEST`다.
- 이 mode의 native enum 값은 PLC 내부에서만 사용한다. wire로 enum 값 `9`를 전달하지 않는다.
- `_LMCAxis.SetPosition()` 선언은 same-core 및 equal-or-lower-priority 호출 조건을 명시한다.
- `LMCAXIS_PAR_RD_SWLIMWINDOW`의 의미는 end-position tolerance window다.

### 2.2 v1 범위

- physical axis reference `1..4`
- application-unit signed DINT target
- actual position과 destination을 함께 설정하는 semantic mode `1`
- 명시적 execute token과 nonzero request id
- stale read를 막는 expected-actual-position compare-and-set
- process/session을 넘어 중복되지 않는 128-bit client intent id
- 같은 session에서 읽은 DiagnosticsBuild/BootId/MapRevision 고정
- 한 prepared command당 최대 한 번의 TCP write 시도
- 응답 상실 시 자동 재전송 금지
- exact terminal retained outcome의 read-only 조회

### 2.3 v1 비범위

- raw native mode passthrough
- modulo axis
- simulation axis
- powered/moving/coupled/profile-locked axis
- homing/reference 대체
- SDO `0x6064` 또는 `0x607A` 직접 write
- response ACK만으로 좌표 적용 또는 물리 무동작을 증명하는 것

## 3. Wire 계약

모든 정수는 little-endian이다. outer envelope는 기존 8-byte RPC header를 그대로 쓴다.

### 3.1 Request

Command ID는 `0x7D12`, outer `Reference`는 physical axis `1..4`, payload length는
정확히 48 byte다.

| Payload offset | Type | Field | 규칙 |
|---:|---|---|---|
| `P+0` | U16 | SchemaVersion | `1` |
| `P+2` | U16 | Flags | `0` |
| `P+4` | U32 | OriginalRequestId | nonzero |
| `P+8` | U32 | ExpectedDiagnosticsBuild | nonzero, fresh `0x7E00` snapshot |
| `P+12` | U32 | ExpectedDiagnosticsBootId | nonzero, fresh `0x7E00` snapshot |
| `P+16` | U32 | ExpectedMapRevision | nonzero, fresh `0x7E00` snapshot |
| `P+20` | U32 | ClientIntentId0 | 128-bit raw intent word 0 |
| `P+24` | U32 | ClientIntentId1 | 128-bit raw intent word 1 |
| `P+28` | U32 | ClientIntentId2 | 128-bit raw intent word 2 |
| `P+32` | U32 | ClientIntentId3 | 128-bit raw intent word 3 |
| `P+36` | I32 | TargetPosition | application units |
| `P+40` | I32 | ExpectedActualPosition | PLC readback와 정확히 같아야 함 |
| `P+44` | U32 | ExecuteToken | `0x50544553`, little-endian bytes ASCII `SETP` |

`OriginalRequestId`는 `LMCAdmin` instance마다 다시 시작하므로 단독으로 authoritative
duplicate-suppression key가 될 수 없다. `ClientIntentId0..3`은 preparation 전에
CSPRNG로 생성한 네 개의 raw U32이고 all-zero를 금지한다. `.NET Guid`의 혼합 endian
byte 배열을 wire 의미로 노출하지 않는다. `ExecuteToken`은 인증값이 아니라 의도하지
않은 호출을 줄이는 protocol guard다.

### 3.2 Response

payload length는 정확히 28 byte다.

| Payload offset | Type | Field | 규칙 |
|---:|---|---|---|
| `P+0` | U16 | SchemaVersion | `1` |
| `P+2` | U16 | ResponseFlags | `0` |
| `P+4` | U16 | CommandStatus | `0=success`, `1=failure` |
| `P+6` | I16 | ErrorId | pre-native failure는 `-31000`, native reject는 adapter sentinel `-6` |
| `P+8` | U32 | RequestId | request echo |
| `P+12` | U32 | DetailCode | Admin detail |
| `P+16` | I32 | AppliedPosition | success에서 target echo |
| `P+20` | U16 | SemanticMode | `1=ActualAndDestinationApplicationUnits` |
| `P+22` | U16 | Reserved | `0` |
| `P+24` | U32 | NativeCommandState | full `_LMCAXIS_CMDERROR` bitfield |

extension field 조합은 다음처럼 제한한다.

- success: `AppliedPosition=TargetPosition`, `NativeCommandState=0`
- native 호출 전 validation failure: `AppliedPosition=0`, `NativeCommandState=0`
- `NativeCommandRejected(11)`: `ErrorId=-6`, `AppliedPosition=0`,
  `NativeCommandState<>0`
- 위 조합과 모순되는 correlated response는 confirmed rejection으로 처리하지 않고 malformed
  post-boundary response로 보아 outcome uncertain 및 exact-session connection fault로 처리한다.

Admin detail은 기존 `0..11`에 아래 값을 예약한다.

| Detail | 의미 |
|---:|---|
| `12` | actual 또는 set velocity가 0이 아님 |
| `13` | active axis error |
| `14` | `InvalidSetPositionSafetyConfiguration`: software limit 또는 local safety configuration이 유효하지 않음 |
| `15` | `CoordinatePreconditionFailed`: expected actual mismatch 또는 허용 coordinate jump 초과 |

activation 전에는 유효한 요청도 `InvalidState(10)`로 실패시키고 native call을 하지 않는다.

### 3.3 ReadAxisSetPositionOutcome request

Command ID는 `0x7D14`, outer `Reference`는 원래 요청의 physical axis `1..4`, payload
length는 정확히 48 byte다. 이 명령은 read-only이고 SetPosition을 재실행하지 않는다.

| Payload offset | Type | Field | 규칙 |
|---:|---|---|---|
| `P+0` | U16 | SchemaVersion | `1` |
| `P+2` | U16 | Flags | `0` |
| `P+4` | U32 | QueryRequestId | nonzero, query 상관관계 전용 |
| `P+8` | U32 | ExpectedDiagnosticsBuild | 원래 intent의 nonzero 값 |
| `P+12` | U32 | ExpectedDiagnosticsBootId | 원래 intent의 nonzero 값 |
| `P+16` | U32 | ExpectedMapRevision | 원래 intent의 nonzero 값 |
| `P+20` | U32 | OriginalRequestId | 원래 `0x7D12` request id |
| `P+24..36` | 4 x U32 | ClientIntentId | 원래 128-bit raw intent |
| `P+40` | I32 | TargetPosition | 원래 target |
| `P+44` | I32 | ExpectedActualPosition | 원래 CAS 값 |

exact terminal record가 있을 때 success response payload는 정확히 84 byte다. 앞의
16 byte는 query 자체의 Admin common success response이고 뒤의 68 byte는 아래 record다.

| Payload offset | Type | Field | 규칙 |
|---:|---|---|---|
| `P+16` | U16 | RecordState | `2=Succeeded`, `3=Rejected` |
| `P+18` | U16 | SemanticMode | `1` |
| `P+20` | U32 | DiagnosticsBuild | exact echo |
| `P+24` | U32 | DiagnosticsBootId | exact echo |
| `P+28` | U32 | MapRevision | exact echo |
| `P+32` | U32 | OriginalRequestId | exact echo |
| `P+36..48` | 4 x U32 | ClientIntentId | exact echo |
| `P+52` | U16 | AxisReference | exact echo |
| `P+54` | U16 | Reserved | `0` |
| `P+56` | I32 | TargetPosition | exact echo |
| `P+60` | I32 | ExpectedActualPosition | exact echo |
| `P+64` | I32 | AppliedPosition | terminal result |
| `P+68` | U16 | OriginalCommandStatus | original `0x7D12` status |
| `P+70` | I16 | OriginalErrorId | original `0x7D12` error |
| `P+72` | U32 | OriginalDetailCode | original `0x7D12` detail |
| `P+76` | U32 | NativeCommandState | original native result |
| `P+80` | U32 | RecordGeneration | nonzero, no wrap |

`Succeeded`는 original status/error/detail/native state가 모두 0이고
`AppliedPosition=TargetPosition`이어야 한다. `Rejected`는 original status `1`이어야
하며 pre-native reject와 native reject의 값 조합은 3.2와 같아야 한다. NotFound,
identity mismatch, Armed/Indeterminate, corrupt 또는 unavailable storage는 16-byte common
failure만 반환한다. 어느 failure도 "실행되지 않음"을 증명하지 않으므로 PC journal을
resolve하지 않는다.

outcome ledger용 Admin detail `16..24`는 각각 diagnostics build mismatch, BootId
mismatch, map revision mismatch, not found, indeterminate, store corrupt, exact-key
mismatch, occupied slot, storage unavailable로 예약한다. 이 detail을 광고하는 paired
PLC/SDK rollout에서만 `ErrorCatalogVersion=2`를 사용한다.

## 4. SDK 실행 계약

일반 `SetAxisPosition(axis, position)` 형태의 재사용 가능한 mutation API를 만들지 않는다.
호출자는 같은 connection/session에서 확인한 capabilities와 expected actual position으로
prepared command를 한 번 생성해야 한다.

prepared command는 다음 값을 변경 불가능하게 고정한다.

- connection owner
- session generation
- axis reference
- target position
- expected actual position
- semantic mode
- capability snapshot
- request id
- diagnostics build, boot id, map revision snapshot
- 128-bit client intent id

execute confirmation object는 preparation 시점에 atomic하게 소비하고 prepared
command에 재사용 가능한 형태로 보관하지 않는다. wire의 `ExecuteToken`은
caller가 입력한 raw 값이 아니라 SDK가 기록하는 고정 상수다.

prepared command는 첫 write 가능 지점 전에 atomic하게 consumed 상태로 전환한다. 동시 두 번
호출해도 한 호출만 write boundary에 도달해야 하며, 두 번째 호출은 zero-wire로 실패한다.

- write boundary 전 cancellation/validation 실패: 결과 확정, zero-wire
- write가 시작될 수 있는 지점 이후 timeout, EOF, socket error, cancellation: outcome uncertain
- write가 시작될 수 있는 지점 이후 malformed response 또는 result publication 실패: outcome uncertain
- outcome uncertain: exact session connection을 fault/close하고 reconnect
- reconnect 뒤 old prepared command replay 금지
- sync/async 모두 같은 one-shot 의미 유지

현재 위치가 target과 같다는 사실만으로 과거 요청 성공을 추론하지 않는다.

## 5. PLC activation gate

아래를 모두 충족하기 전 capability bit 3은 `0`이고 `_LMCAxis.SetPosition()` 호출 수는
항상 `0`이어야 한다.

1. 축별 application-approved `SetPositionMaxJump`가 별도 설정으로 존재하고 `>0`이다.
2. LASAL IDE task map과 PLC runtime에서 handler가 대상 axis realtime thread와 같은 core이고
   equal-or-lower priority임을 확인한다.
3. current project가 `SimulateMode=0`, `Modulo=0`임을 runtime에서 확인한다.
4. `IsReferenced=1`이다. 미참조 축의 좌표 설정은 v1에서 허용하지 않고,
   필요하면 별도 유지보수 승인 절차와 명령으로 분리한다.
5. `PowerOn=0`, `Standstill=1`이다. `SetFlg`는 이전 SetPosition/Reference 이력을
   나타내므로 vendor safety gate로 해석하지 않고 별도 관찰값으로만 기록한다.
6. `MasterLock=0`, `DelayedMasterLock=0`, `NCMotion=0`, `BrakeForPowerOff=0`이다.
7. `NoActPosMeth=0`, `NoControlMeth=0`, `NoActPosChk=0`이며 position feedback가 valid다.
8. actual velocity와 set velocity가 모두 정확히 `0`이다.
9. `ReadAxisError()`가 `0`이고 Emergency/MasterError/SlaveError/Overflow 상태가 없다.
10. group/profile lock이 해제되어 있고 다른 command owner가 없다.
11. PLC actual position이 request의 `ExpectedActualPosition`과 정확히 같다.
12. software min/max가 유효하고 current/target이 모두 범위 안이다.
13. overflow 없이 `abs(target-current) <= SetPositionMaxJump`를 만족한다.
14. 모든 값을 먼저 읽고 검증한 뒤 native method를 정확히 한 번만 호출한다.

검사 중 하나라도 불명확하면 허용하는 쪽으로 추정하지 않고 실패시킨다.

현재 SDK의 generic `EnsureAxisMutationAdmission()`은 위 9번의 최종 activation
gate로 충분하지 않다. native 호출을 추가하기 전에 SetPosition 전용 final-prewire
admission으로 PowerOn/PowerOff/Stop/Reset의 모든 pending continuation과 acceptance
observer, 기타 axis mutation owner 및 해당 축을 포함한 group/profile owner를 하나도
남김없이 차단해야 한다. axis/group coordinator를 별도로 확인한 뒤 순서 간
race를 허용하면 안 되므로, unified ownership coordinator 또는 고정된 다중 gate
획득 순서가 activation 조건이다.

### 5.1 activation을 막는 authoritative recovery 결정

전용 durable journal만 추가해서는 activation gate가 닫히지 않는다. write boundary 뒤 응답이
사라진 `OutcomeUncertain` record를 같은 PLC identity에서 안전하게 해소할 authoritative 절차가
현재 source에 없기 때문이다. fresh actual position이 target과 같아도 요청 전부터 같은 값이었을
수 있으므로 과거 SetPosition 성공으로 판정하거나 journal을 자동 resolve하면 안 된다.

선택한 기본 경로는 PLC retained request/outcome ledger와 read-only `0x7D14` exact query다.
축마다 한 intent slot과 두 개의 complete bank를 두고 generation, 전체 record CRC,
commit marker를 마지막에 기록해야 한다. 여러 retentive scalar를 순서대로 쓰는 것은
atomic record가 아니다.

필수 순서는 `syntax/identity/storage 검증 -> Armed bank durable commit/readback -> safety
검증 -> native call 최대 1회 -> terminal bank durable commit/readback -> RPC response`다.
terminal commit이 실패하면 이전 Armed를 남기고 socket을 fault/close한다. 재기동 시 최고
valid generation이 Armed면 Indeterminate로만 취급한다. 동일 exact intent의 terminal
재요청은 stored response만 반환하고 native call은 반복하지 않는다. 다른 intent가 축 slot을
점유하면 새 요청은 zero-native-call로 거부한다.

일반 terminal record retirement는 별도 CAS 명령으로 분리해야 한다. Armed 또는
Indeterminate record는 일반 retirement로 지우면 안 된다. 이 retirement와 store lifecycle이
구현되지 않은 동안 bit 3은 계속 OFF다. 현재 source에는 store 선언과 `0x7D14` route가 없고,
이를 추가하려면 LASAL IDE에서 class/channel/network 생성 구조를 맞춰야 한다.

PC journal core는 독립적으로 구현할 수 있지만 retained query 없이 WPF 초기화/dispatch/interlock에
연결하면 안 된다. 연결 단계에서는 wire 전 durable Arm, response-loss 뒤 RecoveryRequired,
exact terminal query 후 durable Resolve, 자동 replay 0회를 강제한다. old PLC identity의 record는
현재 위치가 target이라는 이유나 generic operator 확인만으로 retire하지 않는다.

## 6. 검증 기준

### 6.1 PC 자동 시험

- exact 56-byte request frame golden bytes
- diagnostics identity와 4 x U32 intent id golden bytes
- exact 28-byte response payload parser와 full native command-state echo
- malformed/truncated/trailing/unknown semantic mode 거부
- capability owner/session/axis pinning
- expected actual position pinning
- pre-cancel zero-wire
- 동시 double-dispatch에서 write 최대 1회
- response loss가 typed outcome-uncertain으로 변환됨
- reconnect/session 변경 뒤 prepared command replay 0회
- 새 process에서 RequestId가 충돌해도 다른 intent id와 일치하지 않음
- `0x7D14` exact 56-byte query와 84-byte terminal response parser
- NotFound/Indeterminate/corrupt/identity mismatch에서 journal resolve 0회
- feature bit 5 독립 광고와 `bit 3 => bit 5` strict dependency
- Debug/Release 전체 회귀

### 6.2 LASAL source/static

- `0x7D12` route와 exact 56-byte frame offset/length
- capability bit 3과 bit 5 OFF
- activation 전 native `SetPosition` call 0회
- invalid length/schema/flags/request/reference/token이 mutation 0회
- dormant 단계에서는 expected-actual 값에 관계없이 mutation 0회이며, activation 시
  PLC current actual과의 exact CAS negative matrix를 추가
- `SWLIMWINDOW`를 jump cap으로 사용하지 않음
- `_Edit` 프로젝트와 generated declaration/network 미수정
- `0x7D14`와 retained store는 LASAL IDE 구조 작업 전 source-active로 간주하지 않음

### 6.3 IDE/PLC 활성화 전후

활성화 전에는 SourceOnly/static 검증만 완료로 기록한다. 활성화는 다음 증거가 모두 있어야 한다.

- LASAL Reload/Rebuild/Link 성공
- Object Network Server/Client `Find in Implementation` smoke
- 변경 function/method는 `Edit Method` 또는 `Enter`로 exact Implementation header 직접 open
- smoke 시작 이후 `%TEMP%\Lasal2.log` 신규 `CInvalidArgException` 없음
- task/core/priority 확인 화면 또는 runtime trace
- axis 1..4 각각 invalid-state, stale-CAS, jump-limit, software-limit negative capture
- zero/small bounded correction success capture와 three-sample stable readback
- response-loss/reconnect에서 old command replay 0회

ACK는 dispatch 결과일 뿐이다. 실제 좌표 상태는 별도 readback으로 확인하며, readback만으로
과거 명령의 실행 여부를 역추론하지 않는다.

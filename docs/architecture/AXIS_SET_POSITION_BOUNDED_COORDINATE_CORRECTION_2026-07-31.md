# Axis Set Position 제한 좌표 보정 설계

작성일: 2026-07-31

## 1. 결론

`SetAxisPosition`은 LASAL-local Admin command `0x7D12`로 정의한다. public API는
native `_LMCAXIS_SETPOSITION` enum을 받지 않고, application-unit actual position과
destination을 함께 맞추는 semantic mode 한 개만 제공한다.

2026-08-19 현재 PC SDK의 wire/parser/one-shot, durable intent, query/retirement
계약뿐 아니라 PLC source의 fail-closed tranche도 구현됐다. tracked project에는 IDE-created 1344-byte
`VAR_GLOBAL RETAIN` ledger가 있고, Store의 Begin/Commit/Read/Retire/Scan/Select/
CommitRecord 구현과 Control의 `0x7D12/0x7D14/0x7D1A` dormant wiring이 존재한다. `0x7D12`는
private `HandleAdminSetPosition`으로, top-level route는 private `DispatchRequestCommand`로 분리됐다.
축별 coordinate max-jump gate와 new-Armed 전용 direct-axis ownership admission/rollback source도
들어갔다. 2026-08-19 후속 P0에서 `LMCEcatInputLatch`의 frozen 16-DINT mailbox와
32-DINT result, atomic Submit/Copy/RT Process preflight도 source/static/IDE build까지
완료됐다. `READY`는 coherent pre-native snapshot일 뿐 SetPosition 성공이 아니다. 상세
async exactly-once 경계와 crash/recovery 순서는
[Axis SetPosition async RT executor 및 복구 설계](AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md)를
따른다. 이는 source/build 완료이지 runtime activation이 아니다.
`LMC_ADMIN_SET_POSITION_STORE_CONFIGURED=FALSE`, advertised capability `0x00000017`의
bit 3/5/7은 OFF다. `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED=FALSE`이고 축 1..4의
`LMC_ADMIN_SET_POSITION_MAX_JUMP_AXIS*`도 모두 `0`이다. 신규 Admin SetPosition 경로의 native
`_LMCAxis.SetPosition()` 호출은 0회다.
실제 PLC target `Autoexec.lsl`과 전체 `SET SRAMRETAIN` allocation도 확인되지 않았다.
따라서 아래 activation gate가 모두 확인될 때까지 유효 요청은 detail 24로 닫고
retained/native mutation을 모두 0회로 유지한다.

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
- exact terminal retained outcome의 read-only 조회. recovery request는 저장된 original
  BootId와 fresh current BootId를 별도 필드로 보내므로 PLC 재기동 뒤에도 old-boot
  record key를 보존한 채 현재 PLC instance를 검증할 수 있다.

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

retained terminal record의 Rejected snapshot은 durable Armed commit 뒤에 발생 가능한 detail
`10..15`만 허용한다. syntax/request/identity/store admission 단계의 detail `1..9`, `16..18`,
`23..24`는 Armed commit 전에 끝나므로 terminal store에 존재할 수 없다. `0x7D14`/`0x7D1A`
parser와 PC journal은 이런 impossible snapshot을 malformed로 거부하고 journal을 resolve하지 않는다.

Admin detail은 기존 `0..11`에 아래 값을 예약한다.

| Detail | 의미 |
|---:|---|
| `12` | actual 또는 set velocity가 0이 아님 |
| `13` | active axis error |
| `14` | `InvalidSetPositionSafetyConfiguration`: software limit 또는 local safety configuration이 유효하지 않음 |
| `15` | `CoordinatePreconditionFailed`: expected actual mismatch 또는 허용 coordinate jump 초과 |

retained store activation 전에는 유효한 요청도
`SetPositionOutcomeStorageUnavailable(24)`로 실패시키고 native call을 하지 않는다.

### 3.2.1 terminal commit 실패 시 no-response transport fence

durable Armed commit 뒤 terminal outcome을 retained store에 commit하고 readback하는 데
실패하면 성공 또는 실패 response를 만들면 안 된다. 이는 safety/pre-native rejection처럼
native call이 0회인 경로와 native SetPosition 경계를 지난 경로를 모두 포함한다. 즉 이 fence의
조건은 `post-native`가 아니라 `post-Armed terminal commit uncertainty`이고 `NativeCount`는 0 또는
1일 수 있다. 이 경우 service는 내부 전용 sentinel
`LMC_ADMIN_SET_POSITION_CLOSE_WITHOUT_RESPONSE=-12`를 반환하고, TCP transport는 exact
`CommandId=0x7D12`에서만 이를 소비한다. transport는 response buffer와 `SendData`를 사용하지
않고 active socket을 보존한 뒤 첫 closed-session epoch를 기록하고 callback endpoint를 disarm하며
session epoch를 증가시킨다. 이어 ingress를 차단하고 RPC initialized/socket 상태를 지운 뒤 해당
socket을 close하고 즉시 return한다. 이미 pending closed-session epoch가 있으면 이를 덮어쓰거나
callback/session epoch를 다시 변경하지 않는다.

이 sentinel은 wire status/error가 아니다. current source에는 service/TCP 상수, exact TCP
consumer와 terminal commit/readback 실패 경로의 service producer 1곳이 있다. 다만
`LMC_ADMIN_SET_POSITION_STORE_CONFIGURED=FALSE`가 Store 진입 전에 지배하고 native
SetPosition call도 없으므로 producer는 runtime-unreachable이다. 현재 `0x7D12`는 detail 24
normal failure response를 계속 반환하며 no-response branch에 들어가지 않는다. 향후 macro를
활성화하더라도 exact failure injection과 PLC runtime 검증 전에는 이 source producer를
도달 가능한 production 경로로 판정하지 않는다.

### 3.3 ReadAxisSetPositionOutcome request

Command ID는 `0x7D14`, outer `Reference`는 원래 요청의 physical axis `1..4`, payload
length는 정확히 52 byte다. 이 명령은 read-only이고 SetPosition을 재실행하지 않는다.

| Payload offset | Type | Field | 규칙 |
|---:|---|---|---|
| `P+0` | U16 | SchemaVersion | `1` |
| `P+2` | U16 | Flags | `0` |
| `P+4` | U32 | QueryRequestId | nonzero, query 상관관계 전용 |
| `P+8` | U32 | ExpectedDiagnosticsBuild | 원래 intent의 nonzero 값 |
| `P+12` | U32 | OriginalDiagnosticsBootId | 원래 intent의 nonzero 값. retained key 일부이며 current BootId로 치환하지 않음 |
| `P+16` | U32 | ExpectedMapRevision | 원래 intent의 nonzero 값 |
| `P+20` | U32 | CurrentDiagnosticsBootId | fresh current `0x7E00`의 nonzero BootId. current PLC instance 검증 전용 |
| `P+24` | U32 | OriginalRequestId | 원래 `0x7D12` request id |
| `P+28..40` | 4 x U32 | ClientIntentId | 원래 128-bit raw intent |
| `P+44` | I32 | TargetPosition | 원래 target |
| `P+48` | I32 | ExpectedActualPosition | 원래 CAS 값 |

SDK는 recovery key의 Build/MapRevision이 fresh diagnostics와 같아야 한다고 검증하지만,
original BootId가 current BootId와 같다고 요구하지 않는다. PLC는
`CurrentDiagnosticsBootId`를 현재 authoritative BootId와 먼저 비교하고, 별도로
`OriginalDiagnosticsBootId`를 retained record exact key와 비교한다. 이 분리를 제거하면
PLC 재기동 뒤 old-boot terminal/Armed record가 조회도 retirement도 불가능해진다.

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

### 3.4 payload와 total frame 길이

이 문서에서 `payload length`는 8-byte TCP motion transport header를 제외한 길이다.
PLC `RequestFrameSize`/`ResponseSize`와 검증 항목의 `total frame length`는 그 header를
포함한다. 두 용어를 섞지 않는다.

| Command | Request payload | Request total frame | Success payload | Success total frame | Failure payload | Failure total frame |
|---|---:|---:|---:|---:|---:|---:|
| `0x7D12 SetAxisPosition` | 48 | 56 | 28 | 36 | 28 | 36 |
| `0x7D14 ReadAxisSetPositionOutcome` | 52 | 60 | 84 | 92 | 16 | 24 |
| `0x7D1A RetireAxisSetPositionOutcome` | 56 | 64 | 84 | 92 | 16 | 24 |

`0x7D14`와 `0x7D1A`의 84-byte success payload는 16-byte Admin common response와
68-byte terminal snapshot의 합이다. retained 84-byte record 자체와 wire success
payload가 우연히 같은 길이이지만 두 layout은 같지 않다. wire snapshot은 retained
record bytes `8..75`만 payload `16..83`으로 옮긴다.

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
physical axis `1..4`마다 84-byte durable intent/Armed slot 한 개와 84-byte complete
terminal/tombstone bank A/B/C 세 개를 둔다. 축당 4 records, 336 bytes이며 총 예약
크기는 `4 records * 84 bytes * 4 axes = 1344 bytes`다. intent slot을 A/B/C
terminal bank에 포함해 계산하면 안 된다. 별도 intent slot과 세 terminal bank가 있어야
이전 protected tombstone과 현재 terminal을 모두 보존한 채 새 tombstone을 세 번째 bank에
commit하여 `다음 distinct retirement durable commit 전까지` 보장을 지킬 수 있다.

물리 배치는 axis-major로 고정한다. `axisBase = (AxisReference - 1) * 336`이며 각 축 내부
offset은 Intent `+0`, bank A `+84`, bank B `+168`, bank C `+252`다.

| Axis | Intent | Terminal A | Terminal B | Terminal C | Axis byte range |
|---:|---:|---:|---:|---:|---:|
| 1 | `0` | `84` | `168` | `252` | `0..335` |
| 2 | `336` | `420` | `504` | `588` | `336..671` |
| 3 | `672` | `756` | `840` | `924` | `672..1007` |
| 4 | `1008` | `1092` | `1176` | `1260` | `1008..1343` |

LASAL 공식 도움말의 SRAM retain 계약에 따라 SetPosition store는 `VAR_GLOBAL RETAIN`으로
선언하고 PLC target의 `Autoexec.lsl`에는 전체 retained allocation 이상인
`SET SRAMRETAIN`을 별도로 설정해야 한다. 둘 중 하나만 적용해서는 retained store가 아니다.
SetPosition 전용 사용량은 1344 bytes다. 다른 retained consumer와 SRAM을 공유하면 그
사용량에 1344 bytes를 더한 전체 allocation을 `SET SRAMRETAIN`이 수용해야 한다. 설정이
없거나 실제 예약 영역이 전체 allocation보다 작거나 겹치면 store는 unavailable이며,
store-aware handler는
`SetPositionOutcomeStorageUnavailable/detail 24`를 반환하고 retained write와 native call을
모두 0회로 유지해야 한다. 2026-08-18 현재 tracked project에는 LASAL EasyCase import로
`g_LMCSetPositionStoreWords : ARRAY [0..335] OF UDINT`인 1344-byte
`VAR_GLOBAL RETAIN` declaration과 `LMCSetPositionStore` class가 생성되어 있으며,
`LMCSetPositionStore1` object와
`LMCControlCommandService1.SetPositionStore -> LMCSetPositionStore1.ClassSvr` connection도
tracked Comm_Network에 저장돼 있다. external `_CheckSum` object
`LMCSetPositionCheckSum1`과 external client
`LMCSetPositionStore.CheckSum(Required=true, Internal=false)`도 존재하며,
`LMCSetPositionStore1.CheckSum -> LMCSetPositionCheckSum1.ClassSvr` connection이 저장돼 있다.

Store source에는 public `BeginSetPosition`, `CommitSetPositionTerminal`,
`ReadSetPositionOutcome`, `RetireSetPositionOutcome`와 private `ScanAxisStore`,
`SelectTerminalSlot`, `CommitRecord`의 실제 ledger 구현이 모두 들어 있다. Control source도
`0x7D12` Begin/Commit/replay, `0x7D14` Read, `0x7D1A` Retire 호출과 wire response 검증까지
dormant wiring을 완료했다. `0x7D12` new-Armed branch는 exact 48-byte identity로 direct-axis
ordinary ownership을 reserve하고, terminal full-proof 뒤 acquired ownership만 rollback하도록
구성됐다. replay와 non-admitted result는 reserve하지 않는다. coordinate gate는 승인된
max-jump가 `>0`일 때만 axis client/actual position/software limits를 읽고 exact CAS와 signed
DINT 전체 범위의 jump를 검사한다. 현재 ordinary ownership은 `FALSE`, 축별 max-jump는 모두
`0`이므로 이 source slice도 fail-closed다. 따라서 현재 상태는 선언 또는 parser 골격 단계를
지났지만 native mutation을 활성화하지 않는다.

2026-08-19 current verified working-tree checkpoint의 RT preflight source SHA-256은
`F7DC9857DB528D73481831D3D1F9DA3A63420DF653A2146C6E30397337855FA1`, semantic
SHA-256은 `A5BDF88EFA2C1942B1CFF7AA7BAF512A2B5ECF3BFE852BFC33F68449258DB508`이다.
focused RT preflight verifier는 `95/95 PASS`이고 negative fixture를 모두 거부했다.
C78/ARM Rebuild All은 `Linker Done`, `0 errors / 79 compiler warnings`, rebuild 시작 이후
새 `CInvalidArgException=0`으로 끝났다. 생성된 `Class/Classes.lcb`는 `8,600,084` bytes,
SHA-256 `CC5B7FD831616551117DB8260257362069DB51880C53250DBF3CEC35458A48E4`다.
이 증거는 source/IDE compile/link까지만 증명한다. 실제 PLC target의 `Autoexec.lsl`과 다른
retained consumer를 포함한 전체 `SET SRAMRETAIN` allocation은 아직 확인되지 않았고,
`LMC_ADMIN_SET_POSITION_STORE_CONFIGURED`는 `FALSE`다. 따라서 Store method가 runtime에
호출되지 않고 capability bit 3/5/7은 모두 OFF이며, 유효한 `0x7D12/0x7D14/0x7D1A`도
detail 24로 끝난다. 이 Admin SetPosition 경로의 native `_LMCAxis.SetPosition()` source
call은 0회다. PLC download,
runtime 및 hardware E2E 검증도 수행하지 않았다.

#### 5.1.1 `LMCSetPositionStore` public boundary

wire parser, live diagnostics identity validation, axis safety/native calls and TCP response
formatting belong to `LMCControlCommandService`. `LMCSetPositionStore` owns only the retained
ledger; it has no axis client and never calls `_LMCAxis.SetPosition()`. The normalized key passed
from the Control service is an explicit 48-byte buffer that does not depend on compiler packing.

| Offset | Type | Field |
|---:|---|---|
| `0` | U16 | SchemaVersion=`1` |
| `2` | U16 | SemanticMode=`1` |
| `4` | U32 | DiagnosticsBuild |
| `8` | U32 | OriginalDiagnosticsBootId |
| `12` | U32 | MapRevision |
| `16` | U32 | OriginalRequestId |
| `20..32` | 4 x U32 | ClientIntentId0..3 |
| `36` | U16 | AxisReference |
| `38` | U16 | Reserved=`0` |
| `40` | I32 | TargetPosition |
| `44` | I32 | ExpectedActualPosition |

The store public boundary is split into four stages. `pKey` points to the 48 bytes above and
`pSnapshot` points to the 68-byte terminal snapshot that is identical to retained record bytes
`8..75`. `pKey` is non-NIL and `KeySize` is exactly 48; `pSnapshot` is non-NIL and
`SnapshotCapacity` is at least 68. Every output pointer is non-NIL. An invalid boundary performs
no retained mutation.

- `BeginSetPosition` performs exact terminal/tombstone replay, Armed/occupied/corrupt admission,
  generation allocation and the Intent durable commit. It distinguishes a newly committed Armed
  record from a stored terminal replay.
- `CommitSetPositionTerminal` binds the safety/native result supplied by the Control service to
  the exact key and `RecordGeneration`, then durably commits a terminal bank. The store neither
  decides whether a native call occurred nor closes the transport.
- `ReadSetPositionOutcome` performs only the read-only scan and query precedence.
- `RetireSetPositionOutcome` performs only exact terminal generation CAS and the tombstone
  durable commit.

The IDE-created public declarations use these exact inputs and outputs:

```text
BeginSetPosition(
  pKey:^void, KeySize:UDINT, pSnapshot:^void, SnapshotCapacity:UDINT,
  pRecordGeneration:^UDINT, pDetailCode:^UDINT) -> Result:DINT

CommitSetPositionTerminal(
  pKey:^void, KeySize:UDINT, RecordGeneration:UDINT, RecordState:UINT,
  AppliedPosition:DINT, OriginalCommandStatus:UINT, OriginalErrorId:INT,
  OriginalDetailCode:UDINT, NativeCommandState:UDINT,
  pSnapshot:^void, SnapshotCapacity:UDINT, pDetailCode:^UDINT) -> Result:DINT

ReadSetPositionOutcome(
  pKey:^void, KeySize:UDINT, pSnapshot:^void, SnapshotCapacity:UDINT,
  pDetailCode:^UDINT) -> Result:DINT

RetireSetPositionOutcome(
  pKey:^void, KeySize:UDINT, ExpectedRecordGeneration:UDINT,
  pSnapshot:^void, SnapshotCapacity:UDINT, pDetailCode:^UDINT) -> Result:DINT
```

`BeginSetPosition` returns `1` for a newly durable Armed record, `2` for an exact stored terminal
replay and `0` for a domain failure. The other three methods return `1` only when their exact
operation is proven and `0` for a domain failure. Negative values are internal-only and never
constitute durable outcome evidence. `CommitSetPositionTerminal` may return only the exact `-12`
sentinel when a durable Armed record exists but terminal commit/readback cannot be proven.
`RetireSetPositionOutcome` may return a negative internal failure only before its first retained
write. After tombstone marker-clear it must rescan and return either `1`, or `0` with detail 21 or
24; it must never return a negative value or `-12`. Every output buffer/pointer is validated and
zero-initialized before a scan, allocation or retained write.

The Control service owns the complete wire response buffer. It validates total response capacity
of at least 92 bytes before calling `ReadSetPositionOutcome` on a success-capable path and, more
critically, before calling a mutating `RetireSetPositionOutcome`. The store independently validates
that `SnapshotCapacity` is at least 68 bytes. Both gates must dominate the first tombstone
marker-clear; the 68-byte store boundary must not be mistaken for the 92-byte wire gate.

`BeginSetPosition` and `CommitSetPositionTerminal` are one synchronous logical transaction. Before
the Intent commit, Begin reserves both required `StoreGeneration` values, the new
`RecordGeneration`, and the terminal target bank, and rejects any no-wrap failure without a write.
Only after the Armed full readback succeeds does it set volatile transaction state containing the
exact 12 key words, axis, Intent and Record generations, terminal target and terminal store
generation, duplicate reservation guards for the target/generation, and the exact 84 bytes read
from the reserved target bank before the Armed commit.
Commit requires an exact match to that volatile state. Query/retirement on the transaction axis is
fail-closed until Commit finishes. Every Commit result consumes the volatile transaction state;
failure leaves the durable Armed record intact and the Control service applies the exact `0x7D12`
no-response policy. A restart loses only volatile transaction state, so an Armed record without a
terminal remains Indeterminate and cannot be replayed.

The reference model and static verifier must exercise the four public stages separately, not only
through one combined Start helper. Required negative coverage includes Commit without Begin,
key/generation mismatch, reserved terminal slot/generation drift, double Commit, transaction
consumption on every Commit result, and same-axis Query/Retire while a transaction is active.
Syntax, live identity, store/client availability, response capacity and every pre-Armed detail
`1..9,16..18,23..24` must be resolved before Begin can commit. After Begin, a stored Rejected
terminal may contain only detail `10..15`.

While `LMC_ADMIN_SET_POSITION_STORE_CONFIGURED` is `FALSE`, the Control service does not call
these methods and returns detail 24. Method implementation and object/network wiring alone do not
enable this macro or capability bits 3/5/7.

### 5.2 retained record ABI freeze

네 record는 모두 같은 84-byte little-endian layout을 사용한다. 구현은 compiler-dependent
implicit packing에 의존하지 않고 아래 offset으로 byte serialization해야 하며, build/IDE
검증에서 각 record size가 정확히 84임을 확인한다.

| Record offset | Type | Field | 규칙 |
|---:|---|---|---|
| `0` | U16 | StoreSchema | `1` |
| `2` | U16 | StoreFlags | bit 0=`RetiredTombstone`, 나머지 `0` |
| `4` | U32 | StoreGeneration | physical durable commit 순서, nonzero, no wrap |
| `8` | U16 | RecordState | `1=Armed`, `2=Succeeded`, `3=Rejected` |
| `10` | U16 | SemanticMode | `1` |
| `12` | U32 | DiagnosticsBuild | original exact identity |
| `16` | U32 | DiagnosticsBootId | original exact identity |
| `20` | U32 | MapRevision | original exact identity |
| `24` | U32 | OriginalRequestId | original `0x7D12` request id |
| `28..40` | 4 x U32 | ClientIntentId0..3 | original 128-bit intent |
| `44` | U16 | AxisReference | physical axis `1..4` |
| `46` | U16 | Reserved | `0` |
| `48` | I32 | TargetPosition | original target |
| `52` | I32 | ExpectedActualPosition | original CAS value |
| `56` | I32 | AppliedPosition | Armed/pre-native reject는 `0` |
| `60` | U16 | OriginalCommandStatus | original `0x7D12` status |
| `62` | I16 | OriginalErrorId | original `0x7D12` error |
| `64` | U32 | OriginalDetailCode | original `0x7D12` detail |
| `68` | U32 | NativeCommandState | full native result, pre-native는 `0` |
| `72` | U32 | RecordGeneration | intent에서 할당하고 terminal/tombstone까지 불변, nonzero, no wrap |
| `76` | U32 | RecordCrc32 | record bytes `0..75`, `CrcStart=0` |
| `80` | U32 | CommitMarker | `0x7D12C0DE`, 반드시 마지막에 기록 |

`RecordCrc32`는 tracked `_CheckSum.CRC32(pRecord, 76, 0)`와 같은
`LDR_CRC32_BufferEx` 결과를 사용한다. activation evidence에는 canonical 76-byte record 한 개와
그 expected CRC32를 golden vector로 고정한다.

#### 5.2.1 blank, incomplete, valid 및 corrupt 분류

각 physical slot은 아래 순서로 분류한다.

1. 84 bytes가 모두 `0`이면 `Blank`다. fresh/provisioned store의 유일한 empty encoding이다.
2. `CommitMarker=0`이고 나머지 bytes 중 하나라도 nonzero면 `Incomplete`다. commit-last 중단
   흔적이며 committed record나 generation allocator 입력으로 사용하지 않는다.
3. `CommitMarker=0x7D12C0DE`이고 schema/flags/slot role/state/semantic/identity/Reserved,
   nonzero generations 및 CRC가 모두 맞으면 `ValidCommitted`다.
4. `CommitMarker`가 `0`도 `0x7D12C0DE`도 아닌 nonzero 값이거나, exact marker인데 위
   validation 하나라도 실패하면 `Corrupt`다.

`Blank`와 `Incomplete`는 valid winner가 있으면 이를 오염시키지 않고 overwrite candidate가
될 수 있다. `Corrupt`가 하나라도 있거나 아래 cross-record invariant가 깨지면 해당 physical
axis store 전체를 `SetPositionOutcomeStoreCorrupt/detail 21`로 닫는다. boot/open 시 corrupt,
Incomplete 또는 Armed를 자동 clear하거나 format하지 않는다. explicit clear/reformat 명령은
이번 v1 범위에 없다.

Intent slot은 `StoreFlags=0`, `RecordState=Armed`만 valid하다. A/B/C는
`RecordState=Succeeded|Rejected`만 valid하며 `StoreFlags`는 `0` 또는 bit 0만 허용한다.
terminal/tombstone snapshot의 status/error/detail/native 조합은 3.2와 같아야 한다.

#### 5.2.2 StoreGeneration과 RecordGeneration allocator

`StoreGeneration`은 per-axis Intent+A+B+C 전체에서 성공한 physical durable commit마다
증가한다. 다음 값은 모든 `ValidCommitted` record의 최대 `StoreGeneration + 1`이다. valid
record가 없으면 `1`부터 시작한다. duplicate nonzero `StoreGeneration`은 bytes가 같더라도
physical commit 순서가 모순되므로 detail 21이다. max가 `0xFFFFFFFF`이면 wrap하지 않고
detail 24, retained write 0회, native call 0회로 닫는다. `Blank`와 marker-zero
`Incomplete` body의 generation 값은 committed allocator 입력에서 무시한다. 한 번이라도
`ValidCommitted`가 된 `StoreGeneration`은 record가 나중에 교체되더라도 재사용하지 않는다.

`RecordGeneration`은 per-axis logical intent generation이다. 새 exact intent를 durable Arm하기
전에 모든 `ValidCommitted` record의 최대 `RecordGeneration + 1`로 할당하며 valid record가
없으면 `1`부터 시작한다. 같은 intent의 Armed, terminal 및 tombstone에서는 절대 바꾸지 않는다.
max가 `0xFFFFFFFF`이면 wrap하지 않고 detail 24로 닫는다. 같은 nonzero
`RecordGeneration`을 여러 record가 가질 수 있는 경우는 동일 exact key의 Armed/terminal/
tombstone뿐이다. 같은 `RecordGeneration`의 different key는 detail 21이다.
한 번 durable Arm으로 commit된 `RecordGeneration`도 이후 다른 exact key에 재사용하지 않는다.

`StoreGeneration`과 wire/CAS의 `RecordGeneration`은 독립 namespace다. 숫자가 우연히 같을
수 있지만 같음 또는 대소 관계에 의미를 부여하지 않는다.

#### 5.2.3 immutable outcome과 cross-record consistency

Intent slot은 `RecordState=Armed`, `StoreFlags=0`이고 identity/target/CAS를 채우며
Applied/status/error/detail/native 필드는 모두 0이다. terminal bank는 `Succeeded` 또는
`Rejected`만 저장한다. tombstone은 terminal state와 68-byte outcome snapshot 및
`RecordGeneration`을 그대로 보존하고 `StoreFlags.bit0`만 설정한다.

exact key는 DiagnosticsBuild/BootId/MapRevision, OriginalRequestId, 128-bit ClientIntentId,
AxisReference, SemanticMode, TargetPosition 및 ExpectedActualPosition 전체다. 같은 exact key와
`RecordGeneration`의 terminal/tombstone이 둘 이상이면 retained bytes `8..75`의 terminal
snapshot이 정확히 같아야 한다. `StoreGeneration`이 더 큰 record라도 state/applied/status/
error/detail/native 중 하나가 다르면 winner로 선택하지 않고 detail 21이다. matching tombstone은
동일 snapshot이고 terminal보다 큰 `StoreGeneration`일 때만 그 terminal을 retired 상태로
supersede한다. matching terminal이 이미 교체되어 tombstone만 남은 상태는 valid하다.

matching terminal/tombstone은 matching Armed보다 우선하지만, terminal/tombstone의
`StoreGeneration`은 matching Armed보다 커야 한다. 이 순서가 뒤집히거나 동일하면 detail 21이다.
서로 다른 key의 older tombstone과 current Armed/terminal 공존은 정상이며 global newest record
하나만으로 exact query winner를 정하지 않는다.

matching tombstone으로 supersede되지 않은 non-tombstone terminal은 동일 exact key와
`RecordGeneration`을 가진 matching Armed가 정확히 한 개 있어야 하고, 그 Armed의
`StoreGeneration`이 더 작아야 한다. active terminal만 있고 matching Armed가 없거나 둘 이상이면
정상 lifecycle에서 만들 수 없는 상태이므로 detail 21이다. newer matching tombstone이 있는 retired
terminal과, Intent가 다음 distinct key로 교체된 뒤 남은 tombstone-only history는 계속 valid하다.

per-axis 허용 state graph에는 unsuperseded terminal이 최대 하나만 존재한다. current Armed와
different-key non-tombstone terminal이 함께 있으면 그 older terminal을 supersede하는 matching
newer tombstone이 반드시 있어야 한다. 서로 다른 key의 unsuperseded terminal이 둘 이상이거나,
different-key active terminal을 retire하지 않은 상태에서 새 Armed가 committed된 조합은 detail
21이다. `replaceable valid record`는 ProtectedTombstone, current exact terminal source 및 이
state graph가 요구하는 record를 제외한 older tombstone 또는 superseded terminal만 뜻한다.

각 record write는 `CommitMarker=0` write/readback, bytes `0..79` write/readback 및 CRC 확인,
마지막 `CommitMarker=0x7D12C0DE` write/readback, 최종 84-byte reread와 full validation 순서다.
각 readback은 실제 retained SRAM bytes와 staged bytes를 비교한다. 최종 validation이 끝난
시점만 durable commit이다. 여러 retentive scalar를 순서대로 쓰고 marker/readback을 생략한
것은 atomic record가 아니다.

per-axis store는 single-writer다. `0x7D12`, `0x7D14`, `0x7D1A`와 boot scan은 같은 serialized
owner에서 실행하며, scan, generation allocation, slot selection, marker-clear, body/CRC write,
final marker와 full readback 사이에 다른 store operation을 끼워 넣지 않는다. `0x7D12`는
Armed commit부터 safety/native boundary와 terminal commit까지 같은 logical transaction
owner를 유지한다. 이 규칙 때문에 두 `ValidCommitted` slot의 duplicate
`StoreGeneration`은 bytes가 같아도 정상 실행에서 생길 수 없다.

### 5.3 Armed, terminal 및 tombstone lifecycle

필수 순서는 `syntax/identity/storage 검증 -> intent/Armed slot durable commit/readback -> safety
검증 -> native call 최대 1회 -> terminal bank durable commit/readback -> RPC response`다.
terminal commit이 실패하면 Armed를 남기고 socket을 fault/close한다. 재기동 시 exact
RecordGeneration의 valid terminal bank가 matching Armed보다 우선한다. matching terminal이
없고 Armed만 남았으면 Indeterminate로만 취급한다. 동일 exact intent의 terminal
재요청은 stored response만 반환하고 native call은 반복하지 않는다. 다른 intent가 축 slot을
점유하면 새 요청은 zero-native-call로 거부한다.

#### 5.3.1 protected tombstone과 A/B/C slot 선택

`ProtectedTombstone`은 A/B/C의 valid tombstone 중 가장 큰 `StoreGeneration`을 가진 record다.
이는 직전 distinct retirement의 lost-response retry proof이며 다음 distinct retirement가
durable commit될 때까지 overwrite할 수 없다. 더 오래된 tombstone은 opportunistic history로
조회할 수 있지만 이 bounded 보장의 보호 대상은 아니다.

새 terminal을 쓸 때는 ProtectedTombstone을 제외한 A/B/C에서 `Blank`, `Incomplete`, 가장 작은
`StoreGeneration`의 replaceable valid record 순으로 선택한다. retirement는 exact active
terminal source와 different-key ProtectedTombstone을 모두 제외하고 남은 세 번째 bank에 동일
terminal snapshot, 더 큰 `StoreGeneration`, `RetiredTombstone=1`을 durable commit한다.
동일 우선순위의 empty candidate는 A, B, C 순으로 고른다. 정상 state graph인데 이 제외 규칙을
지킬 candidate가 없으면 overwrite하지 않고 detail 21로 닫는다.

원래 exact terminal과 이전 ProtectedTombstone은 새 tombstone의 final marker와 full readback이
완료될 때까지 모두 valid해야 한다. 그 뒤 새 tombstone이 ProtectedTombstone이 되고 이전
tombstone의 보장 기간이 끝난다. 따라서 A/B/C는
`다음 distinct retirement durable commit 전까지` lost-retirement-response retry 보장을
marker-clear crash point에서도 유지한다.

retirement 성공 뒤 matching Intent/Armed slot은 지우지 않는다. matching terminal 또는
tombstone이 winner이므로 남은 Armed는 inactive shadow다. 다음 distinct `0x7D12` admission이
허용될 때 Intent slot을 새 record의 marker-clear, body/CRC, marker-last, full readback 순서로
덮어쓴다. 84-byte Intent를 한 번의 memset/array clear로 지우거나 body부터 부분적으로 0으로
만드는 것은 금지한다. 이 규칙은 power loss 중 old exact marker와 partially-cleared body가
남아 store 전체를 corrupt로 만드는 경로를 제거한다.

#### 5.3.2 exact query, retirement 및 admission precedence

모든 명령은 request length/reference/schema/flags/request id와 live identity를 먼저 검증한다.
`0x7D12`의 original Build/BootId/MapRevision은 live identity와 같아야 한다. cross-boot
recovery를 허용하는 `0x7D14/0x7D1A`는 original BootId를 retained exact key로 유지하고,
별도 `CurrentDiagnosticsBootId`만 live BootId와 비교한다. original Build/MapRevision은 live
Build/MapRevision과 계속 같아야 한다. 그 다음 store allocation/configuration을 검증하여
unavailable이면 detail 24, 그 다음 네 slot을 scan하여 corrupt이면 detail 21을 반환한다.
exact winner는 아래 command-specific 순서를 바꾸지 않는다.

`0x7D14 ReadAxisSetPositionOutcome`:

1. exact terminal 또는 matching tombstone이 있으면 84-byte success payload를 반환한다. 둘 다
   있으면 consistency 검증을 통과한 newer tombstone이 retired winner지만 wire snapshot은 같다.
2. exact Armed만 있으면 `SetPositionOutcomeIndeterminate/detail 20`이다.
3. valid committed record는 있지만 exact key가 없으면 `SetPositionOutcomeKeyMismatch/detail 22`다.
4. valid committed record가 하나도 없으면 `SetPositionOutcomeNotFound/detail 19`다.

`0x7D1A RetireAxisSetPositionOutcome`:

1. exact key+generation tombstone이 있으면 success snapshot을 반환하고 retained mutation은 0회다.
2. exact active terminal과 generation CAS가 맞으면 A/B/C 선택 규칙으로 tombstone을 commit한 뒤
   success snapshot을 반환한다. Control service는 Store method 호출 전에 total response capacity가
   92 bytes 이상인지 확인하고, Store는 별도로 snapshot capacity 68 bytes 이상을 확인해야 한다.
   하나라도 부족하면 retained mutation 0회로 internal failure를 반환하며 wire success를 만들지
   않는다.
3. exact Armed만 있으면 detail 20이며 retirement할 수 없다.
4. exact key의 generation mismatch 또는 different valid key만 있으면 detail 22다.
5. valid committed record가 없으면 detail 19다.

`0x7D14`와 `0x7D1A`의 모든 wire success path는 Control service의 total response capacity
92-byte gate를 요구한다. 특히 `0x7D1A` mutating path는 Store call 전에 이 gate를 통과해야 하며,
Store의 68-byte snapshot gate도 tombstone marker-clear 전에 통과해야 한다.

`0x7D1A`에는 `-12` no-response transport sentinel을 사용하지 않는다. retirement는 native
side effect가 없는 idempotent CAS다. tombstone commit/readback 단계가 실패하면 같은 serialized
owner에서 네 slot을 다시 scan한다. exact tombstone이 durable하면 success를 반환하고, scan이
corrupt면 detail 21, original exact terminal이 보존됐지만 durable tombstone을 증명하지 못하면
detail 24를 반환한다. 어느 failure도 PC journal을 resolve하지 않으며 같은 exact
key+generation retirement를 다시 시도할 수 있다. `-12`는 durable Armed 뒤 terminal commit을
증명하지 못한 exact `0x7D12`에만 한정하며 native call 여부는 조건이 아니다.

`0x7D12 SetAxisPosition` admission:

1. exact terminal 또는 tombstone이면 stored original 28-byte response payload만 반환하고 retained
   mutation과 native call은 0회다.
2. exact Armed만 있으면 detail 20이며 replay하지 않는다.
3. different-key Armed 또는 matching tombstone으로 supersede되지 않은 terminal이 있으면
   `SetPositionOutcomeSlotOccupied/detail 23`이다.
4. no record 또는 retired tombstone history만 있으면 새 `RecordGeneration`을 할당하고 Intent를
   durable Arm한 뒤에만 safety/native 단계로 진행한다.

Armed-only/Indeterminate는 boot, query, retirement 또는 다른 intent admission으로 clear하지
않는다. exact query는 계속 detail 20, different intent는 계속 detail 23으로 영구 fail-closed다.
이번 범위에는 operator clear, force retire 또는 store reformat 명령이 없다.

`0x7D14` exact query와 동일 `0x7D1A` retry는 A/B/C 모두를 검색한다. tombstone exact match도
원래 terminal과 같은 84-byte success payload를 반환하며 native call과 추가 retained mutation은
0회다. 보장 경계 뒤 이전 key는 NotFound 또는 exact-key mismatch이고 PC journal을 resolve하지
않는다. 이 경계보다 긴 보존이 필요하면 bit 7 활성화 전에 별도 retained tombstone ring/log ABI를
승인해야 한다.

일반 terminal record retirement는 별도 CAS 명령으로 분리한다. Armed 또는
Indeterminate record는 일반 retirement로 지우면 안 된다. 이 retirement와 store lifecycle은
current Store source에 구현됐지만 runtime gate는 계속 닫혀 있다. current source에는
IDE-generated 1344-byte `VAR_GLOBAL RETAIN` ledger, Store class/object/client/network,
`0x7D12/0x7D14/0x7D1A` Control wiring과 full ledger implementation이 있다. 실제 retained
store로 활성화하려면 PLC target의 실제 `Autoexec.lsl`을 먼저 확보하여 다른 retained
소비량을 포함한 전체 `SET SRAMRETAIN` 값을 계산·배포하고, 남은 safety/native/ownership
gate를 검증한 뒤 macro를 명시적으로 전환해야 한다. `.lcp` XML에 임의 속성을 추가하거나
source implementation만 완료하는 방식은 이 runtime 설정을 대신하지 않는다.

2026-08-12에 retirement의 PC wire/API 계약을 `0x7D1A
RetireAxisSetPositionOutcome`으로 고정했다. 요청 payload는 `0x7D14`의 exact recovery key
52 byte에 nonzero `RecordGeneration` U32를 붙인 56 byte이고, 성공 응답은 `0x7D14`와
동일한 exact 84-byte terminal snapshot이다. exact terminal key+generation만 허용하며,
paired PLC는 응답 상실 뒤 동일 요청을 재시도해도 mutation을 반복하지 않는 tombstone을
보존해야 한다. `0x7D1A`는 SetPosition retirement 전용으로 예약하며 HomeDS402Ex가
재사용하지 않는다.

기존 bit 5 `AxisSetPositionOutcomeRead`의 read-only 의미는 확장하지 않는다. 새 bit 7
`AxisSetPositionOutcomeRetirement`를 사용하고 `bit 7 => bit 5`, `bit 3 => bit 5 + bit 7`을
강제한다. 이렇게 해야 과거 query-only bit 5 PLC를 새 SDK가 retire-capable로 오판하지
않는다. current PLC source는 store/tombstone lifecycle을 구현하고 `0x7D1A`를 exact-parse하지만
configuration macro가 `FALSE`라 bit 3/5/7 모두 OFF이고 detail 24만 반환한다. 따라서 source
구현 완료는 runtime 활성화나 journal resolve 권한이 아니다.

PC journal core는 독립적으로 구현할 수 있지만 retained query 없이 WPF 초기화/dispatch/interlock에
연결하면 안 된다. 연결 단계에서는 wire 전 durable Arm, response-loss 뒤 RecoveryRequired,
exact terminal query로 generation을 고정한 뒤 `0x7D1A` retirement 성공을 확인해야만 durable
Resolve하며, 자동 replay 0회를 강제한다. old PLC identity의 record는 현재 위치가 target이라는
이유나 generic operator 확인만으로 retire하지 않는다.

2026-08-18 current journal core는 같은 `v1` directory와 `ELMOASP1` magic을 유지하면서 storage
format 1/2를 읽고 새 record만 format 2로 쓴다. format 2는 terminal query의 request id,
terminal state/applied/status/error/detail/native state/nonzero generation과 retirement request id를
영속화한다. `ArmedBeforeDispatch|RecoveryRequired -> TerminalOutcomeObserved -> Resolved`만 허용하고,
마지막 전이는 exact key/generation/terminal snapshot의 성공한 typed `0x7D1A` 결과가 필수다.
evidence-free `Resolve` API는 제거됐다. 이 사실은 journal core의 안전 계약일 뿐 MainWindow 실행
노출이나 PLC capability 활성화가 아니다.

## 6. 검증 기준

### 6.1 PC 자동 시험

- `0x7D12` exact 56-byte request frame golden bytes
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
- `0x7D14` exact 52-byte request payload/60-byte total frame과 84-byte success payload/92-byte
  total response parser
- `0x7D1A` exact 56-byte request payload/64-byte total frame, 84-byte success payload/92-byte
  total response, nonzero
  generation CAS와 bit 7 dependency
- original BootId와 fresh current BootId가 다른 cross-boot query/retirement golden bytes,
  current BootId mismatch zero-wire/PLC reject, stored original identity exact-match matrix
- NotFound/Indeterminate/corrupt/identity mismatch에서 journal resolve 0회
- feature bit 5 독립 query 광고, `bit 7 => bit 5`와
  `bit 3 => bit 5 + bit 7` strict dependency
- `Verify-LasalSetPositionRetainedStoreReference.ps1 -SelfTestOnly`의 Intent shadow,
  A/B/C selection, 11-stage write/readback fault, retirement rescan, generation invariant,
  raw 48-byte key/68-byte snapshot boundary, staged Begin/Commit transaction 및 same-axis
  interleave fence matrix `41/41 fixtures, 1253 assertions` PASS. 이는 reference model 자체
  검증이며 LASAL
  production source나 vendor CRC equivalence 증거가 아니다.
- 2026-08-18 SDK Debug와 isolated Release 각각 `1153/1153` PASS
- current WPF smoke `356/356` PASS. PC 증거이며 PLC/runtime 증거가 아니다.

### 6.2 LASAL source/static

- `0x7D12` route와 exact 56-byte frame offset/length
- capability bit 3, bit 5와 bit 7 OFF
- Admin `0x7D12` activation 전 해당 경로의 native `SetPosition` call 0회
- internal no-response sentinel은 service/TCP에 각각 `-12`로 exact 정의한다. service producer는
  source상 terminal commit/readback failure 1곳이며 macro `FALSE` 상태의 runtime 도달은 0회다.
- TCP consumer는 exact `0x7D12`+sentinel에서만 first-wins closed-session capture, callback disarm,
  session epoch roll, ingress/RPC fence, socket close를 수행하고 response write/`SendData`는 0회
- invalid length/schema/flags/request/reference/token이 mutation 0회
- retained runtime activation 시 existing `VAR_GLOBAL RETAIN` declaration과 PLC `Autoexec.lsl`의 전체
  allocation 기준 `SET SRAMRETAIN >= other retained consumers + 1344`, SetPosition 전용
  1344-byte 영역 확인
- `SET SRAMRETAIN` unset/부족/overlap에서 detail 24, retained write/native call 0회
- axis-major `axisBase=(AxisReference-1)*336`, axis별 84-byte Intent + terminal/tombstone A/B/C
  exact offset과 총 1344-byte 경계
- all-zero Blank, marker-zero Incomplete, committed Valid, Corrupt 분류와 boot 자동 clear 0회
- CRC bytes `0..75`, seed 0, commit marker `0x7D12C0DE` last-write/readback과 각 crash point
- per-axis no-wrap `StoreGeneration`/`RecordGeneration` allocator와 namespace 분리
- same key+RecordGeneration outcome divergence, different-key generation reuse 및 duplicate
  StoreGeneration이 detail 21
- exact query/retire/admission precedence와 Armed-only 영구 fail-closed
- `0x7D1A` 92-byte total response capacity pre-mutation 및 zero-mutation negative
- tombstone exact retry와 다음 distinct retirement commit 경계 전후 matrix
- dormant 단계에서는 expected-actual 값에 관계없이 mutation 0회이며, activation 시
  PLC current actual과의 exact CAS negative matrix를 추가
- `SWLIMWINDOW`를 jump cap으로 사용하지 않음
- `_Edit` 프로젝트와 generated declaration/network 미수정
- `0x7D14`/`0x7D1A` route와 exact parser는 source-active이다. retained store, tombstone,
  success response path도 source 구현됐지만 configuration macro와 SRAM runtime 증거가
  준비될 때까지 inactive로 유지
- 2026-08-18 service `44/44`는 historical focused checkpoint다. Current final focused
  suites는 AxisZeroHome `34/34`, close fence `38/38`, AdminStore `110/110` + edge
  `15/15`, StoreScan `292/292` + generated wiring `41/41`, RT preflight `95/95` PASS다.
- RT preflight focused verifier `95/95` PASS, `LMCEcatInputLatch.st`
  `F7DC9857DB528D73481831D3D1F9DA3A63420DF653A2146C6E30397337855FA1`, semantic
  `A5BDF88EFA2C1942B1CFF7AA7BAF512A2B5ECF3BFE852BFC33F68449258DB508`
- SetPosition verifier integration은 정확히 4개 `usingLtd` pragma, 12개 generated client와
  Store/CheckSum/network 계약을 full SourceOnly에 반영했다. Store read-only `292/292`와
  dedicated generated-wiring `41/41` verifier는 모두 PASS했다.
- 2026-08-19 UDP callback Gate D verifier SHA-256
  `FBC6E185C81E744A59D70A0EBDDA8D3BD2E8871F3F9BE6FB354CBD718A785ADA`는 PowerShell 5.1
  parser와 self-test `336/336`을 PASS했다. Current
  `TerminalWakeBrokerCandidate` VerifyCurrent도 `ProductionApproved=True`,
  `NeedsRebaseline=False`, `IDEClosed=True`로 PASS했고 current `Classes.lcb` pin은
  `8600084/CC5B7FD8...`다.
  이는 exact current registry/snapshot delta만 승인한 PC/static Gate D 결과이며
  generic/hash-only rebaseline이 아니다.
- historical pre-split full SourceOnly는 UDP Gate D 뒤
  `LMCControlCommandService::HandleAdminCommands` UTF-8
  `raw/LF/CRLF=34437/34437/35442`에서 exit `1`로 fail-closed했다. 이 blocker 기록은
  당시 증거로 보존한다.
- current source는 private `HandleAdminSetPosition`과 `DispatchRequestCommand`로 분리됐다.
  final `raw/LF/CRLF`는 `HandleAdminCommands=24140/24140/24879`,
  `HandleAdminSetPosition=17166/17166/17659`, `HandleRequest=30802/30802/31626`,
  `DispatchRequestCommand=1909/1909/1966`이다. Current method-size gate는 6 classes /
  106 methods, under-limit 103 / baseline debt 3, threshold `<32768`로 PASS했다.
- main verifier SHA-256
  `878DFB46691271F5ADA982A6585AA6A4FF5065AA357D07CBFA7488F845A688BD`의
  `-SourceOnly -ExpectedSdoWriteAxis 1` terminal run은
  `PASS LASAL.StaticContract.SourceOnly`, `Phase5TransportClean`,
  `TopologyIoCheckpoint=IntegratedReadOwnerDormant`, exit `0`으로 PASS했다. 이는
  source/static 증거이며 clean release input, PLC SRAM allocation, PLC download/runtime 또는
  hardware proof를 추가하지 않는다.

### 6.3 IDE/PLC 활성화 전후

2026-08-19 current verified Rebuild All은 RT preflight source를 포함해 `Linker Done`,
`0 errors / 79 warnings`, ERROR-level 0, 신규 `CInvalidArgException` 0으로 완료됐다. 이 결과는 activation
build/download가 아니며 PLC runtime도 증명하지 않는다. 활성화는 다음 증거가 모두 있어야 한다.

- macro/config 전환 뒤 LASAL Reload/Rebuild/Link 성공
- SetPosition store가 `VAR_GLOBAL RETAIN`으로 선언되고 PLC target `Autoexec.lsl`의
  `SET SRAMRETAIN` allocation이 다른 retained consumer를 포함해 SetPosition 전용
  1344-byte 영역을 실제로 수용한다는 memory map 증거
- Object Network Server/Client `Find in Implementation` smoke
- 변경 function/method는 `Edit Method` 또는 `Enter`로 exact Implementation header 직접 open
- smoke 시작 이후 `%TEMP%\Lasal2.log` 신규 `CInvalidArgException` 없음
- task/core/priority 확인 화면 또는 runtime trace
- axis 1..4 각각 invalid-state, stale-CAS, jump-limit, software-limit negative capture
- zero/small bounded correction success capture와 three-sample stable readback
- response-loss/reconnect에서 old command replay 0회

ACK는 dispatch 결과일 뿐이다. 실제 좌표 상태는 별도 readback으로 확인하며, readback만으로
과거 명령의 실행 여부를 역추론하지 않는다.

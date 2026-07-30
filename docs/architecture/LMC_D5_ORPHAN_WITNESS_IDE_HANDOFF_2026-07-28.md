# D5 SDO orphan witness LASAL IDE 구조 작업 인계

- 날짜: 2026-07-28
- 대상: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- 목적: D5 SDO owner transport loss 뒤 PLC 내부 `MarkOrphan`과 late callback drain을
  새 RPC session에서 판정할 수 있도록 durable read-only evidence 구조를 만든다.
- 상태: 설계/IDE 인계만 작성했다. 아래 declaration, handler, capability와 RPC는 아직 구현하거나
  광고하지 않았다.

## 1. 현재 증명 범위

현재 PLC source에는 다음 동작이 있다.

- `TCPMotionInterface.PendingClosedSessionEpoch`가 다음 cycle에
  `LMCDiagnosticsService.NotifySessionClosed(SessionEpoch)`를 호출한다.
- graceful RPC Close `0x405D`, peer socket disconnect, partial/failed send가 모두 같은
  `PendingClosedSessionEpoch`만 남기므로 close 원인을 구분할 수 없다.
- owner ticket이 `Running`이면 `NotifySessionClosed`가 해당 축 executor의
  `MarkOrphan(ExpectedToken)`을 호출한다.
- 이후 public ticket/session identity를 지우고 orphan drain state를 유지한다.
- late callback은 executor/service에서 소비하고 slot을 재사용할 수 있게 한다.

하지만 `MarkOrphan` 반환값과 drain 완료가 durable하게 남지 않는다. 기존 owner 전용
`0x7E03` status도 새 session에서 old ticket을 조회하는 증거가 될 수 없다. 따라서 현재 WPF
qualification은 local zero-linger close/no-`0x405D`와 distinct new connection의 two-ticket
recovery까지만 `ApplicationRecoveryOnly`로 판정하며 `orphanQualified=false`를 고정한다.

## 2. IDE에서 추가할 declaration

기존 class의 declaration은 LASAL IDE에서 생성한다. `.st` declaration 영역이나 generated
`.lcb`를 외부 편집기로 직접 보정하지 않는다. 이름은 7-bit ASCII를 사용한다.

### TCPMotionInterface

`PendingClosedSessionEpoch` 옆에 아래 상태를 추가한다.

| Name | Type | 의미 |
|---|---|---|
| `PendingClosedSessionReason` | `UDINT` | 아래 close reason code |

close reason code 초안:

| 값 | 이름 | 의미 |
|---:|---|---|
| 0 | `NONE` | 유효한 pending close 없음 |
| 1 | `GRACEFUL_RPC_405D` | 정상 RPC Close 요청 |
| 2 | `SOCKET_PEER_DISCONNECT` | receive 경로에서 peer 단절 확인 |
| 3 | `TRANSPORT_SEND_FAILURE` | partial/failed send 뒤 session quarantine |

`Diagnostics.NotifySessionClosed` 호출에는 `CloseReason : UDINT` input을 추가한다. pending epoch와
reason은 같은 조건에서 함께 publish/clear해야 하며, graceful `0x405D`를 abrupt orphan proof로
승격하지 않는다.

### LMCDiagnosticsService

`NotifySessionClosed`에 `CloseReason : UDINT` input을 추가하고, 아래 durable evidence state를
class variable로 생성한다. 실제 type 이름은 IDE에서 project naming rule에 맞춰 구조체 하나로
묶어도 되지만 필드 의미와 폭은 유지한다.

| 필드 | Type | 요구사항 |
|---|---|---|
| `D5OrphanEvidenceGeneration` | `UDINT` | publish마다 증가, 0은 사용하지 않음 |
| `D5OrphanEventId` | `UDINT` | 한 close/orphan event 동안 고정, 0은 없음 |
| `D5OrphanCloseReason` | `UDINT` | 위 close reason |
| `D5OrphanSessionEpoch` | `UDINT` | 닫힌 owner session |
| `D5OrphanTicketId` | `UDINT` | clear 전 old public ticket |
| `D5OrphanBootId` | `UDINT` | clear 전 ticket BootId |
| `D5OrphanMapRevision` | `UDINT` | clear 전 ticket MapRevision |
| `D5OrphanOperationToken` | `UDINT` | executor에 전달한 exact token |
| `D5OrphanOperationKind` | `UINT` | SDO Read/Write 구분 |
| `D5OrphanPreCloseState` | `UINT` | `NotifySessionClosed` 진입 시 state |
| `D5OrphanSlaveReference` | `UINT` | old request metadata |
| `D5OrphanObjectIndex` | `UINT` | old request metadata |
| `D5OrphanSubIndex` | `USINT` | old request metadata |
| `D5OrphanValueType` | `USINT` | old request metadata |
| `D5OrphanDataLength` | `UINT` | old request metadata |
| `D5OrphanTimeoutCycles` | `UDINT` | old request metadata |
| `D5OrphanMarkResult` | `DINT` | exact `MarkOrphan` 반환값 |
| `D5OrphanMarkCycle` | `UDINT` | Mark 호출 cycle |
| `D5OrphanDrainPending` | `BOOL` | orphan late callback 대기 |
| `D5OrphanDrainCompleted` | `BOOL` | 같은 event의 drain 완료 |
| `D5OrphanLateCallbackObserved` | `BOOL` | executor callback을 실제 소비함 |
| `D5OrphanDrainCompletionCycle` | `UDINT` | drain 완료 cycle |

`NotifySessionClosed`는 public ticket identity를 지우기 전에 모든 old identity/request field와
pre-close state를 복사해야 한다. `MarkOrphan` 반환값을 무시하지 않고 같은 event에 기록한다.
evidence는 PLC boot 전까지 또는 더 최신 event가 publish될 때까지 유지한다. generation은 모든
field를 쓴 뒤 마지막에 publish하여 reader가 coherent snapshot을 확인할 수 있게 한다.

`ProcessOperations`의 orphan drain 완료 지점에서는 같은 `D5OrphanEventId`와 operation token을
검증한 뒤 `DrainCompleted`, `LateCallbackObserved`, completion cycle을 갱신한다. 다른 ticket이나
새 event의 evidence를 덮어쓰면 안 된다.

### LMCSdoExecutor

현재 `MarkOrphan`의 반환 계약은 유지한다.

- `0`: exact active token이 `Running -> Orphaned`로 전환됨
- `-1`: token이 0이거나 active token 불일치
- `-2`: token은 일치하지만 state가 `Running`이 아님

service가 현재 callback/drain path에서 exact event 완료를 확정할 수 없으면, executor에 마지막
callback token/state/result를 읽는 read-only query method를 IDE에서 추가한다. 별도 Network object는
기본안이 아니다. 기존 `LMCDiagnosticsService`가 evidence owner와 RPC handler를 맡는다.

## 3. provisional read-only RPC

후보 command는 `0x7E52 ReadD5OrphanEvidence`다. `0x7E51`은 extended SDO result chunk용으로 이미
예약돼 있으므로 재사용하지 않는다. `0x7E52`와 capability bit 18은 아직 provisional이다.

최종 packet layout은 아래 필드를 모두 전달해야 한다.

- request: schema/version, nonzero request id, optional minimum evidence generation
- response identity: evidence generation/event id, DiagnosticsBootId, MapRevision
- close identity: reason, closed session epoch
- old operation: ticket, token, kind/state, slave/index/subindex/type/length/timeout
- transition: exact `MarkOrphan` result/cycle
- drain: pending/completed, late callback observed, completion cycle

LASAL handler, `TCPMotionInterface` route, `DINT_PACKET_MAP.txt`, C# request/parser/model, deterministic
parser tests와 WPF proof adapter가 모두 준비되기 전에는 command range나 capability bit 18을
광고하지 않는다. unsupported 상태에서 raw `0x7E52`는 기존 fail-closed error envelope를 유지한다.

## 4. 구현 후 판정 조건

`orphanQualified=true`는 아래 조건을 한 run에서 모두 만족할 때만 허용한다.

1. old owner의 exact ticket이 실제 `Running`이었다.
2. pcap에서 old session에 `0x405D`가 없고 실제 transport loss 순서가 확인된다.
3. 새 RPC connection에서 읽은 evidence의 session/ticket/Boot/Map/token/request가 old ticket과 exact다.
4. close reason이 graceful close가 아니며 `MarkOrphanResult=0`이다.
5. 같은 event/token에서 drain pending 뒤 late callback observed와 drain completed가 확인된다.
6. evidence generation이 fresh하고 coherent하며 중간 PLC reboot/Map drift가 없다.
7. 새 connection의 exact `0x6061:0 Int8/1` recovery ticket 두 개가 서로 다른 identity로 성공한다.
8. final capability/evidence reread가 같은 Boot/Map/contract를 유지한다.

하나라도 없으면 current `ApplicationRecoveryOnly`, `orphanQualified=false`를 유지하고 증거를
quarantine한다.

## 5. IDE 저장/빌드/다운로드 확인

1. `TCPMotionInterface`, `LMCDiagnosticsService`와 필요 시 `LMCSdoExecutor` declaration/method를
   LASAL IDE에서 생성한다.
2. class/include/generated metadata를 저장하고 Rebuild/Link error 0을 확인한다.
3. 변경 class마다 `Find in Implementation` smoke test를 수행한다.
4. smoke 시작 시점 이후 `%TEMP%\Lasal2.log`에 새 `CInvalidArgException`이 없는지 확인한다.
5. 외부 편집 단계에서 implementation, TCP route, C# parser/tests를 완성한다.
6. capability bit 18은 source/static/PC tests가 모두 통과한 뒤에도 dormant로 둔다.
7. target download 뒤 live PLC/pcap matrix가 위 판정 조건을 만족할 때만 bit를 활성화한다.

사용자에게 요청할 다음 작업은 위 declaration 생성과 IDE Rebuild/Link 결과 전달이다. 그 전에는
기존 LASAL implementation을 외부에서 변경하지 않는다.

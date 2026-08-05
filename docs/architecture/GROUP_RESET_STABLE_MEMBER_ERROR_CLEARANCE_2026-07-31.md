# Group Reset stable member error-clearance contract

- 작성일: 2026-07-31
- 상태: **SDK/WPF/PC 자동 시험 구현 완료, PLC/runtime 재검증 대기**
- 적용 wire: `0x20D2`, `0x2049`, `0x2045`, `0x2028`
- 증거 범위: source review와 PC/static contract만 해당한다. PLC/runtime 완료 증거가 아니다.

## 결론

현재 `GroupReset[Async]`의 `0x2049` 성공 응답은
`LMCRobot.AxQuitError(AxisNo:=0)` 호출이 접수됐다는 ACK다. 모든 group member의 LASAL
axis error가 사라졌다는 완료 증거가 아니다. `_LMCAxis.QuitError()` 처리는 realtime task로
지연될 수 있고, `0x2045 GroupReadStatus`는 각 member의 `ReadAxisError()`를 반환하지 않는다.

안정 완료 계약은 아래 순서로 고정한다.

1. 유효한 `0x20D2` exact member snapshot을 확보하고 고정한다.
2. `0x2049 GroupReset`을 정확히 한 번만 보낸다.
3. Resume의 각 round에서 `0x2045` 한 번과 snapshot에 고정된 모든 member의 `0x2028`을
   snapshot 순서대로 각각 한 번 보낸다.
4. group error와 모든 member axis error가 모두 0인 full-clear round를 기본 3회 연속 확인한다.
5. 동일 connection session의 timeout, cancel 또는 status failure 뒤에는 `0x2049`를 replay하지
   않고 exact continuation으로 status-only Resume만 허용한다.
6. disconnect/process restart 뒤에는 old continuation을 재사용하지 않는다. command-before durable
   record와 current endpoint/PLC/group/member identity가 전부 exact-match한 경우에만 새 session에
   recovery continuation을 attach하고 status-only Resume한다. 이때도 `0x2049`는 0회다.

## 현재 source 사실

- LASAL `0x2049` handler는 group reference `0x0100`, payload 길이와 `Execute=1`을 확인한 뒤
  `LMCRobot.AxQuitError(AxisNo:=0)`을 호출하고 즉시 ACK를 만든다.
- `AxisNo=0`은 robot에 연결된 모든 axis를 대상으로 한다. 현재 추적 LASAL source의
  `0x20D2` 응답은 reference `1..9`의 9개 member를 만든다. SDK의 generic 계약은 정상 응답의
  `1..16`개 member를 허용하며, 관찰된 snapshot만 고정한다. 따라서 이 snapshot은 expected
  topology, 현재 PLC build 또는 항상 9개라는 사실을 증명하지 않는다.
- `0x2045`는 robot/profile state와 `GroupErrorId`, power/standby/disabled mask를 반환하지만
  member별 `ReadAxisError()` 값은 반환하지 않는다.
- `0x2028`은 선택한 axis의 native `AxisErrorId`를 반환한다.
- raw SDK `GroupReset[Async]`는 ACK-only 호환 API로 남아 있다. WPF Group Reset 버튼은 아래
  stable API와 durable journal을 사용해 accepted-once Reset, same-session Resume, exact-match
  reconnect/process-restart status-only recovery를 수행한다.

## 구현된 public API

- `PendingGroupResetWaitContinuation`
- `LMCGroupResetPreparedEvidence`
- `LMCGroupResetDurableMemberIdentity`
- `LMCGroupResetDurableRecoveryRecord`
- `BeginGroupResetWaitForStableErrorClearanceAsync`
- `AttachGroupResetDurableRecoveryAsync`
- `ResumeGroupResetWaitForStableErrorClearanceAsync`
- `GroupResetAndWaitForStableErrorClearanceAsync`
- `SupersedePendingGroupResetAfterCapturedMemberSafetyMutation`

raw `GroupReset[Async]`는 호환성을 위해 ACK-only API로 남긴다. WPF 일반 Group Reset 경로는
위 split/compound API를 사용한다. 현재 qualification runner에는 Group Reset scenario가 없으므로
qualification이 이 API를 사용한다고 주장하지 않는다.

## Begin 계약

1. total timeout, poll interval, required stable count를 validation한다.
2. current session에서 group을 lookup하고 `0x20D2`를 읽는다.
3. member count, order, name, reference, device ID를 포함한 observed snapshot을 defensive copy로
   고정한다. empty, zero/duplicate/misindexed reference 또는 허용 범위 `1..16` 밖 count는
   zero-reset failure다. 이 validation은 expected topology attestation이 아니다.
4. exact snapshot 뒤 `0x2049` write가 시작되기 직전에 synchronous prepared observer를 실행한다.
   observer는 operation ID, group/session, ordered member identity, required stable count의 immutable
   evidence를 받는다. observer가 throw하거나 reentrant mutation을 시도하면 `0x2049`는 0회이며
   connection은 read-only RPC에 재사용할 수 있다.
5. `0x2049`를 정확히 한 번 전송한다. Begin은 `0x2045`/`0x2028`을 보내지 않는다.
6. success ACK 뒤 connection session, group와 member mutation generation, exact snapshot을 담은
   `PendingGroupResetWaitContinuation`을 첫 status round 전에 게시한다.

ACK rejection은 `Rejected`, write 전 실패는 `NotAttempted`다. write가 시작됐으나 ACK를 확정할
수 없으면 `OutcomeUncertain`이며 transport를 폐기하고 reset을 재전송하지 않는다.

## Resume와 stable 판정

Resume은 `0x2049`를 절대 보내지 않는다. 각 round는 다음 exact wire order를 사용한다.

1. pinned group reference에 `0x2045` 한 번
2. pinned member snapshot의 첫 항목부터 마지막 항목까지 각 reference에 `0x2028` 한 번씩

한 round가 full clear인 조건은 전부 참이어야 한다.

- group status가 정상 parse되고 `IsReadSuccessful=true`
- `GroupErrorId == 0`
- 모든 pinned member status가 정상 parse되고 `IsReadSuccessful=true`
- 모든 pinned member의 `AxisErrorId == 0`

기본 완료 기준은 full-clear round 3회 연속이다. group 또는 member error가 하나라도 nonzero면
stable count를 0으로 reset하고 다음 round를 계속한다. malformed/function/transport failure는
typed failure로 종료하고 accepted continuation을 pending으로 보존한다. partial round는 sample로
세지 않으며 서로 다른 Resume 호출 epoch의 stable count도 합산하지 않는다.

compound 호출의 total deadline에는 Begin과 Resume의 local gate, `0x20D2`, `0x2049` ACK,
모든 `0x2045`/`0x2028`, poll delay가 포함된다. split Begin은 그 호출의 gate/snapshot/Reset
deadline을 사용하며, 나중의 수동 Resume은 새 status-only timeout epoch를 시작한다. stable
count도 Resume epoch 시작 시 0으로 초기화한다. wire write 뒤 no-response는
transport-invalidated evidence를 남긴다.

## session, identity, mutation attribution

동일 connection에서 continuation은 다음 값에 고정한다.

- connection session generation
- group name/reference
- exact member snapshot
- group mutation generation
- snapshot의 모든 member별 axis mutation generation

foreign, stale, superseded, completed continuation과 concurrent second Resume은 pre-wire로
거부한다. reset 이후 group mutation 또는 pinned member 중 하나의 axis mutation이 actual write
boundary에 도달하면 원 reset completion attribution을 무효화한다.

disconnect, reconnect 또는 process restart 뒤에는 기존 continuation을 stale로 거부하고 SDK가
`0x2049`를 자동 replay하지 않는다. WPF는 prepared observer에서 다음 값을
`ArmedBeforeDispatch`로 원자 저장하고, success ACK observer에서 exact CAS로
`AcceptedAwaitingProof`로 바꾼다.

- operation ID, record revision, state, prior outcome, UTC timestamps와 SHA-256 checksum
- PLC IPv4/TCP port, 실제 callback binding의 PC local IPv4/UDP port
- `DiagnosticsBuild`, nonzero `DiagnosticsBootId`, nonzero `MapRevision`
- group name/reference, old command-owner session generation
- exact ordered `1..16` member name/reference/device와 required stable count

old command-owner session generation은 positive provenance/evidence일 뿐이다. durable attach는 새
connection의 current-session continuation을 새로 만드므로 old generation을 새 session
generation과 비교하거나 같아야 한다고 판정하지 않는다.

process startup 또는 connection loss에서 `ArmedBeforeDispatch`는 `OutcomeUncertain`, accepted record는
`Accepted`를 보존한 채 `RecoveryRequired`로 승격한다. 새 connection에서는 endpoint와
Diagnostics identity를 먼저 exact-match하고, exact group handle을 Load한 뒤 SDK
`AttachGroupResetDurableRecoveryAsync`가 fresh `0x20D2`를 정확히 한 번 읽어 count/order/name/reference/
device를 다시 비교한다. 일치할 때만 current group/member mutation generation을 baseline으로 잡고
recovery continuation을 게시한다. mismatch, corrupt/unsupported record, zero identity, duplicate 또는
concurrent attach는 fail-closed이며 `0x2049`/`0x2045`/`0x2028`은 0회다. snapshot mismatch에는
`0x20D2`만 1회다.

recovery Resume은 기존 status round를 그대로 사용한다. stable success에서 durable Resolve를 UI
interlock 해제보다 먼저 수행한다. prior outcome이 `OutcomeUncertain`이면 현재 group/member error가
안정적으로 clear됐다는 사실만 기록하며 이전 Reset 성공이나 ACK를 추론하지 않는다. evidence의
`RecoveredFromDurableRecord=true`, `CommandDispatchedInOwnerSession=false`가 이 경계를 명시한다.

## stale identity retirement

현재 endpoint는 같지만 active Group Reset record의 `DiagnosticsBuild`, `DiagnosticsBootId`
또는 `MapRevision`이 current PLC와 다르면 exact status-only recovery를 시작하지
않고 connection-scoped read-only quarantine을 유지한다. 운영자가 장비와 drive의
물리 상태를 독립 확인한 경우에만 stale record retirement을 명시적으로 시작할
수 있다.

- retirement owner는 `GroupReset(6)`으로 분리한다.
- immutable ledger는 original journal bytes, SHA-256, record identity/revision, prior outcome,
  endpoint/local callback, old owner session, stable count와 ordered member hash를 보존한다.
- confirmation 전후에 current Build/BootId/MapRevision과 full active evidence vector를 다시
  읽고, commit된 exact-source decision에 대해서만 원 journal을 full-byte CAS `Resolved`로
  바꾼다.
- same-endpoint exact-current record는 retire하지 않고 다음 process의 exact recovery에 남겨
  둔다. 다른 endpoint record는 현재 PLC identity로 판정하지 않고 fail-closed로
  보존한다.
- old Reset outcome은 계속 `UNKNOWN`이며 retirement는 Motion, Power, SDO Write,
  `0x2049`, cleanup을 전송하지 않는다. 성공하면 quarantine connection을 닫고 app
  restart를 강제한다.

새 process에서 reconnect한 뒤에도 남아 있는 exact-current recovery가 있으면 그
status-only proof를 먼저 완료해야 Motion/Power/approved SDO Write admission이 다시
열린다. retirement 자체는 제어 허용 증거가 아니다.

## power, profile lock, motion과의 분리

Group Reset은 power, kinematic identity, profile lock 또는 motion-ready를 설정하는 명령이 아니다.
따라서 이 값들은 error-clear 완료 predicate가 아니며 Reset proof가 Group Power/Enable/Profile
Lock journal을 resolve하거나 승격해서는 안 된다. 마지막 `0x2045`의 power/lock 값은 관측
evidence로만 보존한다.

관측 상태가 cached preparation과 모순되면 해당 preparation을 무효화하고 별도 exact
Power/Lock verification 전 motion을 fail-closed한다. 현재 WPF는 accepted Reset 즉시
Power/Identity/Home/Profile readiness를 무효화하고 stable proof 성공 뒤에도 자동 복원하지
않는다. Power On, Set Identity, Enable을 별도로 다시 검증하기 전 motion은 차단한다.

## safety takeover

Group Stop, Group Power Off와 safe Group Disable은 pending Reset monitor 중에도 허용하며
현재 full status round 뒤 monitor를 선점할 수 있다.
safety reservation 뒤에는 Reset Resume의 후속 status RPC가 pre-wire로 중단되어야 한다.

- safety 명령이 명확한 zero-wire/사전 실패 또는 structurally valid NACK이면 exact Reset
  continuation을 보존하고 status-only Resume을 다시 허용한다.
- safety 명령이 accepted 또는 outcome-uncertain이면 safety recovery가 우선한다.
- Reset record는 predecessor evidence로 보존하지만 original Reset 완료를 나중에 주장하지 않는다.
- safety 뒤에도 Reset을 replay하지 않는다. 재시도는 기존 attribution과 안전 상태를 별도 확인한
  뒤 운영자가 명시적으로 새 operation으로 시작해야 한다.

그룹 또는 pinned member mutation이 accepted/outcome-uncertain boundary에 도달하면 continuation은
terminal superseded다. timeout, cancel, group/member status failure는 pending/resumable이다. SDK는
same-group unsafe command를 pending gate에서 pre-wire 차단하고, captured member-axis mutation은
process-local generation으로 검출해 Resume에서 terminalize한다. SDK direct axis consumer를 group
pending에 역등록해 선제 차단하는 기능은 이번 범위가 아니다. WPF 중앙 admission은 exact live-session
pending 또는 outcome-uncertain 상태에서 axis/group의 새 Reset, Power On, Enable, SetKin, Move,
mutation qualification, reconnect, connection/window Close를 모두 막는다. read-only inspection과
  exact status-only Resume은 허용한다. 외부 연결 손실은 session continuation을 폐기하고 durable
  record를 `RecoveryRequired`로 승격한다. exact reconnect/status-only recovery 전에는 preparation을
  fail-closed로 유지하며 Reset을 자동 replay하지 않는다.

WPF가 captured-member Axis Stop/PowerOff의 accepted 또는 outcome-uncertain 결과를 얻으면
`SupersedePendingGroupResetAfterCapturedMemberSafetyMutation`으로 exact continuation, same-session
axis와 실제 member generation mismatch를 다시 확인하고 SDK pending도 즉시 terminalize한다. valid
NACK에서 generation이 rollback됐으면 이 API는 false를 반환하고 continuation을 보존한다.

WPF는 safety command를 하나씩 직렬화한다. public SDK를 직접 사용하는 caller가 provisional
same-group safety와 captured-member safety를 동시에 실행하는 경우는 자동 직렬화하지 않는다.
그 경우 member generation mismatch는 남아 다음 Resume에서 terminal interference로 검출되므로
false completion은 없지만, caller는 safety command를 직렬화하고 group safety 결과 확정 뒤 exact
reconciliation 또는 Resume을 수행해야 한다.

## 결과 evidence 최소 필드

- submission outcome: `NotAttempted`, `Rejected`, `OutcomeUncertain`, `Accepted`
- `CommandMayHaveBeenSent`, ACK와 native status/error
- operation ID, old command-owner session generation,
  `RecoveredFromDurableRecord`, `CommandDispatchedInOwnerSession`
- exact group/member snapshot
- last group status와 pinned member별 last axis status
- 완료된 `StatusRoundCount`, current/required consecutive full-clear count
- expected/observed group 및 member mutation generations와 interference 원인
- elapsed milliseconds와 transport invalidation 여부
- 마지막 power/profile-lock observation; 완료 predicate가 아니라는 표기

성공에서만 pending을 완료 처리한다. timeout, cancel, status failure는 exact continuation과 durable
record를 보존한다. interference/safety supersede는 terminal evidence로 남기며 원 Reset을 Resume할
수 없다. identity mismatch는 fail-closed로 거부하고 record를 유지한다.

## 필요한 PC/static 시험

- `0x20D2` valid/empty/duplicate/malformed snapshot과 defensive copy
- Begin의 `0x20D2 -> 0x2049` exact order, `0x2049` 1회, status 0회
- Resume round의 `0x2045 -> each 0x2028` exact order와 `0x2049` 0회
- group/member mismatch가 stable count를 reset하고 full clear 3회에서만 완료
- partial round와 Resume epoch 사이 sample 비누적
- reject, timeout, cancel, malformed, no-response, transport fault의 typed evidence/no replay
- stale/foreign/superseded/completed/concurrent continuation zero-wire
- group 또는 pinned member mutation interference와 final publication race
- disconnect/reconnect 뒤 stale continuation zero-wire와 Reset automatic replay 0회
- prepared observer exact boundary, throw/reentrant zero Reset wire와 connection reuse
- durable record state/outcome exact CAS, checksum/version/corruption/single-writer/defensive copy
- restart exact endpoint/build/BootId/Map/group/member attach에서 fresh `0x20D2` 1회와 `0x2049` 0회
- endpoint/build/BootId/Map/group/member 각 mismatch에서 status/reset 0회와 record 보존
- process kill at Armed, ACK-before-status, first status round 뒤 restart의 zero Reset replay
- recovery mutation baseline 0과 group/각 member `0 -> 1` interference/final-publication race
- Stop/PowerOff/Disable takeover, accepted/outcome-uncertain terminal supersede, valid NACK restore,
  safety priority와 Reset no-replay
- captured-member safety reconciliation은 generation mismatch에서만 terminalize하고, valid NACK
  rollback은 false/보존, 이미 terminal 상태의 repeat는 false임을 검증
- power/profile-lock pending/proof 비승격

위 PC/static 계약 시험은 SDK와 WPF smoke suite에 포함했다. current Debug/Release
forced Rebuild는 SDK 각각 1006/1006, WPF 각각 278/278 PASS다.

## PLC/runtime 완료 조건

PC/static 증거만으로 realtime task가 모든 axis error를 지웠다고 할 수 없다. current PLC cold
download와 physical safety 승인 뒤 다음을 별도 입증한다.

- pcap에 valid `0x20D2` snapshot, `0x2049` 정확히 1회
- 이후 각 round마다 `0x2045` 1회와 pinned member 전원의 `0x2028` 1회, exact order
- group/member error 모두 0인 full-clear 3회 연속과 UI evidence 일치
- physical axis 1..4와 simulated axis 5..9의 적용 범위 및 persistent fault matrix
- timeout/disconnect/safety takeover에서 `0x2049` 자동 replay 0회

이 계약은 LASAL application error-clear 관측이다. DS402 `0x6041` Fault bit, `0x603F` drive
error register, servo-ready, profile lock, reference/home 또는 실제 motion 안전을 증명하지 않는다.

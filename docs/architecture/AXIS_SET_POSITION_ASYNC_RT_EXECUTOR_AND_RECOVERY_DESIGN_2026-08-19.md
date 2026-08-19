# Axis SetPosition 비동기 RT executor 및 복구 설계

작성일: 2026-08-19

상태: preflight-only tranche source/static/IDE 검증 완료, runtime activation OFF

## 1. 결론

`0x7D12 SetAxisPosition`의 다음 구현 단계는 TCP/Cyclic owner가 축 명령을 직접
실행하는 구조가 아니다. non-RT `LMCControlCommandService`가 retained Store와
ownership lifecycle을 소유하고, `_LMCAxis1.LMCPreRtWorkTrigger`로 호출되는
`LMCEcatInputLatch.RtWork()`가 RT preflight와 향후 native
`_LMCAxis.SetPosition()` 경계를 소유하는 비동기 구조로 분리한다.

이번 tranche의 범위는 **RT preflight mailbox/result 경계만 만드는 것**이다.
`ready` 결과는 "이 RT sample에서 preflight 입력과 관찰값이 일관됐다"는 뜻일 뿐,
SetPosition 성공, native 접수, 좌표 적용 또는 durable terminal outcome이 아니다.
이번 tranche에서는 다음 값이 모두 고정이다.

- `LMC_ADMIN_SET_POSITION_STORE_CONFIGURED=FALSE`
- `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED=FALSE`
- axis 1..4 `LMC_ADMIN_SET_POSITION_MAX_JUMP_AXIS*=0`
- Admin capability `0x00000017`; bit 3/5/7 OFF
- Admin SetPosition executor의 `claim=0`, `nativeCount=0`, `nativeState=0`
- Admin SetPosition 경로의 native `_LMCAxis.SetPosition()` call site 0개
- PLC download, PLC runtime 및 hardware mutation 0회

향후 activation은 한 번에 열지 않는다. retained Store 외부 설정, async Control
lifecycle, RT claim-before-native, 3-sample terminal proof, WPF durable journal과
capability gate를 모두 구현하고 검증한 뒤 별도 변경에서 macro와 capability를
전환한다.

기존 wire/retained 계약은
[`AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md`](AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md)를
그대로 따른다. 이 문서는 그 계약을 RT 실행 경계와 crash/recovery 순서로 확장한다.

## 2. 확인된 현재 상태와 증거 경계

### 2.1 현재 source에서 확인한 사실

- [`LMCControlCommandService.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st)는
  `0x7D12`에서 syntax/identity를 검증하고 Store가 비활성이면 detail 24로 닫는다.
- 같은 source의 capability response는 `0x00000017`이다. 이는 axis/group read,
  group relative move 및 Axis Home만 광고하며 SetPosition bit 3, outcome read bit 5,
  retirement bit 7은 광고하지 않는다.
- [`LMCSetPositionStore.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSetPositionStore/LMCSetPositionStore.st)와
  [`global_LMCSetPositionStore.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSetPositionStore/global_LMCSetPositionStore.st)에는
  1344-byte `VAR_GLOBAL RETAIN` ledger와 Begin/Commit/Read/Retire source가 있다.
  그러나 실제 PLC target의 `Autoexec.lsl`과 전체 `SET SRAMRETAIN` allocation은
  저장소에서 확인되지 않았다.
- [`LMCEcatInputLatch.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st)는
  realtime class이고, tracked Motion Network에서
  `_LMCAxis1.LMCPreRtWorkTrigger -> LMCEcatInputLatch1.ClassSvr`로 연결된다.
  `LMCEcatInputLatch1.LMCAxis1..4`도 각 `_LMCAxis1..4.Control`에 연결된다.
- `_LMCAxisBase.LMCPreRtWorkTrigger` vendor comment는 연결된 user class의
  `RtWork`가 같은 task time에서 axis `RtWork`의 첫 task로 호출된다고 설명한다.
  이 정적 연결은 실행 위치의 설계 근거지만 실제 target의 CPU core와 OS priority
  측정값을 대신하지 않는다.
- 이번 tranche의 IDE-generated declaration에는 16-DINT request mailbox,
  32-DINT result, request/applied sequence와 Submit/Copy/private Process method가 있다.
  [`LMCEcatInputLatch.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st)의
  Submit/Copy/Process body와 `RtWork`의 Process call까지 구현됐다. `RtWork`는
  `cycleCounter` 계산 직후, 기존 `PublishSequence` odd writer-open보다 먼저 Process를
  호출한다. 이 구현은 observation-only이며 Control/Store/capability 연결이나 Admin native
  SetPosition 호출을 추가하지 않는다.

### 2.2 current verified working-tree checkpoint

아래 tuple은 이번 RT preflight implementation을 포함한 2026-08-19 current working-tree
checkpoint다. 아직 clean commit/release 또는 PLC image provenance가 아니다.

- `LMCEcatInputLatch.st` SHA-256:
  `F7DC9857DB528D73481831D3D1F9DA3A63420DF653A2146C6E30397337855FA1`
- `LMCEcatInputLatch.st`: `137,891` bytes, 17 methods
- preflight semantic SHA-256:
  `A5BDF88EFA2C1942B1CFF7AA7BAF512A2B5ECF3BFE852BFC33F68449258DB508`
- focused verifier
  `Verify-LasalContract.ps1 -AxisSetPositionPreflightRtVerifierSelfTestOnly`:
  `95/95 PASS`; negative fixture 전부 reject
- method-size budget: 6 classes / 106 methods / under-limit 103 / baseline debt 3 PASS
- main terminal: `-SourceOnly -ExpectedSdoWriteAxis 1`,
  `PASS LASAL.StaticContract.SourceOnly`, `Phase5TransportClean`,
  `TopologyIoCheckpoint=IntegratedReadOwnerDormant`, exit `0`
- main verifier SHA-256:
  `878DFB46691271F5ADA982A6585AA6A4FF5065AA357D07CBFA7488F845A688BD`
- existing focused suites: AxisZeroHome `34/34`, close `38/38`, AdminStore `110/110`
  + edge `15/15`, StoreScan `292/292` + generated wiring `41/41` PASS
- UDP Gate D verifier SHA-256
  `FBC6E185C81E744A59D70A0EBDDA8D3BD2E8871F3F9BE6FB354CBD718A785ADA`:
  PowerShell 5.1 parser PASS, self-test `336/336`, current
  `TerminalWakeBrokerCandidate` VerifyCurrent
  `ProductionApproved=True`, `NeedsRebaseline=False`, `IDEClosed=True`
- C78 output `Class/Classes.lcb`: `8,600,084` bytes, SHA-256
  `CC5B7FD831616551117DB8260257362069DB51880C53250DBF3CEC35458A48E4`
- C78/ARM Rebuild All: `0 errors / 79 compiler warnings`, `Linker Done`, rebuild
  시작 이후 새 `CInvalidArgException=0`
- PC SDK tests: Debug/isolated Release 각각 `1153/1153 PASS`
- WPF smoke tests: `356/356 PASS`

이 checkpoint에도 새 native executor, Control async lifecycle, Store-to-mailbox 연결,
capability bit 3/5/7 활성화는 없다. PLC download/runtime/hardware 시험도 수행하지 않았다.

### 2.3 증거 수준

| 수준 | 이 문서에서 인정하는 증거 | 현재 판정 |
|---|---|---|
| PC | SDK build/test, WPF journal test, golden frame/parser | SDK `1153/1153`, WPF `356/356` PASS; MainWindow SetPosition dispatch 연결은 없음 |
| source/static | `.st`, generated declaration, Network/table, verifier | preflight focused `95/95`, main SourceOnly exit 0, method budget `6/106/103/3` PASS; source/semantic/verifier SHA-256 고정 |
| IDE | declaration/source compile, Rebuild/Link, 새 IDE log | `Classes.lcb` `8,600,084` bytes, C78 0 errors/79 warnings, Linker Done, 새 CInvalidArg 0 |
| PLC | download된 build/BootId, task/core/priority, SRAM map, runtime trace | 없음 |
| hardware | 실제 axis 1..4 negative/success, power-loss 및 packet capture | 없음 |

PC/static/IDE PASS는 PLC 또는 hardware PASS로 승격하지 않는다.

## 3. 불변 안전 계약

향후 구현은 다음 순서를 바꾸면 안 된다.

1. TCP syntax, live diagnostics identity와 response capacity를 검증한다.
2. Store `BeginSetPosition`을 먼저 호출하여 exact replay/admission을 결정한다.
3. **새로 durable commit된 Armed**에서만 internal axis ownership을 reserve한다.
4. RT preflight를 비동기로 submit하고 coherent result만 읽는다.
5. `ready` 뒤에도 ownership과 live tuple을 다시 검증한다.
6. RT executor가 claim을 먼저 확정한 뒤 native method를 최대 한 번 호출한다.
7. native return만으로 성공시키지 않고 3개의 연속 stable sample을 확인한다.
8. retained terminal commit과 full readback을 먼저 완료한다.
9. 그 뒤에만 ownership을 release하고 TCP response를 허용한다.
10. 어느 단계에서든 증거가 모호하면 replay하지 않고 socket fence와 quarantine을
    적용하며 retained Armed를 보존한다.

"현재 위치가 target과 같다"는 사실은 과거 SetPosition 실행 증거가 아니다. exact
retained terminal snapshot만 authoritative outcome이다.

## 4. 이번 preflight-only tranche

### 4.1 목적과 비범위

목적은 Cyclic owner와 RT owner 사이에 bounded SPSC request/result protocol을 두고,
axis 1..4의 같은 RT observation에서 safety 입력을 읽을 수 있음을 source/static으로
검증하는 것이다.

이번 tranche에서 하지 않는 작업은 다음과 같다.

- `LMCControlCommandService.HandleAdminSetPosition`과 mailbox 연결
- Store Begin/Commit transaction과 RT mailbox 연결
- ordinary ownership enable 또는 lifecycle 변경
- mailbox claim/cancel 활성화
- native `_LMCAxis.SetPosition()` 추가
- capability bit 3/5/7 변경
- WPF MainWindow 버튼, dispatch 또는 recovery interlock 연결
- PLC download, runtime 또는 hardware 시험

따라서 현재 request를 수동으로 submit하여 `state=ready`를 얻더라도 public
`0x7D12` 성공으로 해석할 수 없다.

### 4.2 frozen request mailbox ABI

`AxisSetPositionPreflightMailbox : ARRAY [0..15] OF DINT`는 정확히 64 byte다.
모든 slot은 DINT storage이고 unsigned field는 `$UDINT` overlay로 읽고 쓴다.

| Slot | Byte | Field | 이번 tranche 규칙 |
|---:|---:|---|---|
| 0 | 0 | `OperationToken` | nonzero |
| 1 | 4 | `OwnerGeneration` | nonzero |
| 2 | 8 | `StoreRecordGeneration` | nonzero |
| 3 | 12 | `CallerSessionEpoch` | nonzero |
| 4 | 16 | caller `RequestSequence` / request identity | caller가 준 immutable nonzero identity |
| 5 | 20 | `AxisReference` | signed DINT, `1..4` |
| 6 | 24 | `TargetPosition` | application-unit signed DINT |
| 7 | 28 | `ExpectedActualPosition` | application-unit signed DINT |
| 8 | 32 | `MaxJump` | UDINT; `0`도 coherent publish 후 configuration reject |
| 9 | 36 | `ExpectedAxisMask` | exactly `1 << (axis-1)` |
| 10 | 40 | internal `RequestSequenceEcho` | Submit이 idle atomic sequence를 `+1`하고 `0`을 건너뛰어 만든 publication echo |
| 11 | 44 | `Claim` | 이번 tranche는 반드시 `0` |
| 12 | 48 | `Cancel` | 이번 tranche는 반드시 `0` |
| 13 | 52 | `Reserved0` | `0` |
| 14 | 56 | `Reserved1` | `0` |
| 15 | 60 | `Reserved2` | `0` |

slot 4는 caller가 준 immutable request identity이고 slot 10은 Submit이 만든 internal
mailbox atomic publication sequence echo다. 둘을 같은 counter로 간주하거나 한 field를
생략하지 않는다.

### 4.3 frozen result ABI

`AxisSetPositionPreflightResult : ARRAY [0..31] OF DINT`는 정확히 128 byte다.

| Slot | Byte | Field | 의미 |
|---:|---:|---|---|
| 0 | 0 | `OperationToken` | request echo |
| 1 | 4 | `OwnerGeneration` | request echo |
| 2 | 8 | `StoreRecordGeneration` | request echo |
| 3 | 12 | `CallerSessionEpoch` | request echo |
| 4 | 16 | caller `RequestSequence` / request identity | request slot 4 echo |
| 5 | 20 | `AxisReference` | request echo |
| 6 | 24 | `TargetPosition` | request echo |
| 7 | 28 | `ExpectedActualPosition` | request echo |
| 8 | 32 | `MaxJump` | request echo |
| 9 | 36 | `ExpectedAxisMask` | request echo |
| 10 | 40 | internal `RequestSequenceEcho` | processed publication sequence |
| 11 | 44 | `State` | `0=empty`, `1=ready`, `2=rejected`, `3=corrupt` |
| 12 | 48 | `Failure` | internal preflight failure code |
| 13 | 52 | `Detail` | Admin detail candidate; 아직 wire response 아님 |
| 14 | 56 | `ObservationCycle` | RT observation cycle |
| 15 | 60 | `AxisStatus` | full observed status word |
| 16 | 64 | `AxisError` | full observed axis error |
| 17 | 68 | `ActualPosition` | application-unit actual |
| 18 | 72 | `SetPosition` | application-unit set position |
| 19 | 76 | `ActualVelocity` | observed actual velocity |
| 20 | 80 | `SetVelocity` | observed set velocity |
| 21 | 84 | `SoftwareMinPosition` | application-unit SW min |
| 22 | 88 | `SoftwareMaxPosition` | application-unit SW max |
| 23 | 92 | `SimulateMode` | observed configuration |
| 24 | 96 | `Modulo` | observed configuration |
| 25 | 100 | `MasterLock` | observed status/config |
| 26 | 104 | `DelayedMasterLock` | observed status/config |
| 27 | 108 | `BiasedDistance` | overflow-safe unsigned coordinate distance |
| 28 | 112 | `Evidence` | capture/pass evidence bitmap |
| 29 | 116 | `Claim` | 이번 tranche는 반드시 `0` |
| 30 | 120 | `NativeCount` | 이번 tranche는 반드시 `0` |
| 31 | 124 | `NativeState` | 이번 tranche는 반드시 `0` |

`State=ready`의 필요조건은 coherent identity, connected axis client, supported physical
axis, valid nonzero max-jump/exact axis mask, safe status/error/configuration, exact expected-actual
CAS, valid SW limits와 overflow-safe distance gate다. 이는 한 sample의 pre-native
판정이다. `ready`는 다음을 증명하지 않는다.

- ownership Active commit
- execution claim
- native method call
- terminal state
- retained commit/readback
- TCP response

frozen internal failure 값은 다음과 같다. 이 값은 wire ErrorId가 아니다.

| `Failure` | 이름 | 의미 |
|---:|---|---|
| `0` | `None` | snapshot-ready 또는 별도 failure 없음 |
| `-1` | `Invalid` | method boundary/identity 입력 invalid |
| `-2` | `Busy` | 다른 outstanding tuple |
| `-3` | `Corrupt` | stable snapshot의 mailbox/result integrity 불충족 |
| `-4` | `Client` | axis client 연결 없음 |
| `-5` | `UnsafeState` | axis state/lock 조건 불충족 |
| `-6` | `Configuration` | simulate/modulo/SW limit/max-jump 구성 invalid |
| `-7` | `Velocity` | actual/set velocity nonzero |
| `-8` | `AxisError` | active axis error |
| `-9` | `Coordinate` | CAS 또는 bounded distance 불충족 |

`Detail`은 `0,10,12,13,14,15`만 사용한다.

| RT result | `Failure/Detail` | 의미 |
|---|---:|---|
| `ready` | `0/0` | coherent preflight snapshot; SetPosition 성공 아님 |
| `rejected` | `-4/10` | axis client 미연결 |
| `rejected` | `-5/10` | required/unsafe status 불충족 |
| `rejected` | `-6/14` | simulate/modulo/move type/SW limit 구성 invalid 또는 `MaxJump=0` |
| `rejected` | `-7/12` | actual/set velocity nonzero |
| `rejected` | `-8/13` | axis error 또는 error status active |
| `rejected` | `-9/15` | expected-actual CAS 또는 max-jump 불충족 |
| `corrupt` | `-3/0` | stable snapshot의 mailbox/result integrity 불충족 |

따라서 현재 축별 설정값 `MaxJump=0`은 torn/corrupt가 아니라 coherent
`state=2`, `failure=-6`, `detail=14` configuration rejection이다.

`Evidence` bit는 다음과 같이 고정한다.

| Bit | 의미 |
|---:|---|
| 0 | request identity captured and valid |
| 1 | exact physical axis mask captured and valid |
| 2 | axis client connected |
| 3 | axis status/error captured |
| 4 | simulate/modulo/move-type parameters captured |
| 5 | actual/set velocity captured |
| 6 | software limits captured |
| 7 | expected-actual CAS gate |
| 8 | overflow-safe max-jump gate |
| 9 | `READY` snapshot gate |
| 31 | activation-off proof; claim/native fields are zero |

`RtWork` 호출 위치는 `cycleCounter` 계산 직후이며 기존 `PublishSequence` odd
publication보다 앞이다. Process가 이 위치에서 bounded time 안에 끝나지 않으면 안 된다.

### 4.4 atomic publication

request producer는 다음 순서를 지킨다.

1. 현재 atomic request/applied sequence를 읽는다.
2. outstanding request가 있으면 다른 tuple을 덮어쓰지 않는다.
3. mailbox 16 words를 모두 쓴다.
4. slot 10에 publish할 next sequence를 쓴다.
5. 마지막에 `sigclib_atomic_setU32(AxisSetPositionPreflightRequestSequence)`를
   실행한다.

RT consumer는 request sequence를 atomic read하고 16 words를 local snapshot으로
복사한 뒤 request sequence를 다시 읽는다. snapshot 중 atomic sequence가 바뀌면 즉시
return하며 axis client method를 하나도 호출하지 않고 result도 publish하지 않는다.
즉 `AppliedSequence`는 그대로다. 두 atomic read가 같지만 slot 10/identity/reserved field
또는 이전 result integrity가 잘못된 stable snapshot만 `state=3`, `failure=-3`, `detail=0`
corrupt result로 publish한다.

result producer는 32 words를 모두 작성하고 마지막에
`AxisSetPositionPreflightAppliedSequence`를 atomic publish한다. result reader는 applied
sequence를 읽고 128 bytes를 복사한 뒤 sequence를 다시 읽는다. before/after가 다르거나
request/result identity가 다르면 결과를 반환하지 않는다.

`CopyAxisSetPositionPreflightResult`의 `DestSize`는 정확히 `128`이어야 한다. 더 작거나 큰
buffer를 허용하지 않는다.

sequence `0`은 empty sentinel이다. 증가 결과가 `0`이면 `1`로 건너뛴다. payload보다
sequence를 먼저 publish하거나 AppliedSequence 전에 일부 result만 외부에 노출하면 안 된다.

## 5. 향후 Control 비동기 lifecycle

### 5.1 Store Begin precedence

Control은 ownership을 먼저 잡고 Store를 조회하면 안 된다. 정확한 순서는 다음이다.

| `BeginSetPosition` 결과 | 의미 | Control 동작 |
|---:|---|---|
| `2` | exact terminal/tombstone replay | stored 28-byte original response만 반환, reserve/RT/native 0회 |
| `1` | 새 Armed durable commit/readback | 이 경우에만 internal ownership reserve로 진행 |
| `0`, detail 20 | exact Armed/Indeterminate | reserve/RT/native 0회, recovery 필요 |
| `0`, detail 21/23/24 | corrupt/occupied/unavailable | reserve/RT/native 0회, fail closed |
| negative | internal boundary failure | wire success 금지, mutation 진행 금지 |

Store의 volatile Begin/Commit transaction state는 fresh Armed부터 terminal Commit까지
유지한다. 같은 axis의 Query/Retire와 다른 Begin을 사이에 끼우지 않는다. PLC restart로
volatile transaction state가 사라지면 retained Armed만 남으므로 자동 replay하지 않는다.

### 5.2 lifecycle state

Control의 future volatile context는 적어도 exact Store key, `RecordGeneration`, session,
request sequence, token/generation/axis mask, mailbox sequence, response socket과 deadline을
고정한다. 상태 전이는 아래 한 방향만 허용한다.

| State | 진입 조건 | 허용 동작 |
|---|---|---|
| `Idle` | active context 없음 | 새 syntax/identity 검증 |
| `BeginPending` | Store call 직전 | Begin 한 번 |
| `FreshArmed` | Begin result `1` exact readback | ownership reserve 한 번 |
| `OwnershipReserved` | exact reserved tuple | preflight submit 한 번 |
| `PreflightPending` | coherent request published | Copy result polling만 수행 |
| `PreflightReady` | exact `ready`, claim/native 모두 0 | ownership 재검증 및 Active commit |
| `OwnershipActive` | `CommitAxisOwnership` exact success | execution-claim request publish |
| `ExecutionPending` | future claim request published | RT terminal/result polling만 수행 |
| `TerminalCommitPending` | pre-native reject 또는 proven RT terminal | Store terminal Commit 한 번 |
| `TerminalProven` | exact terminal commit/full readback | exact ownership release와 response 허용 |
| `Quarantined` | claim/native/terminal 증거 모호 | response/replay 금지, socket close, Armed 보존 |

stored replay는 이 lifecycle에 진입하지 않는다. preflight reject는 native 0을 증명한
coherent RT result일 때만 retained Rejected terminal 후보가 된다. `PreflightReady` 뒤
stale context, session close, timeout 또는 inconsistent copy를 ordinary reject로 바꾸지
않고 quarantine한다.

### 5.3 TCP pending sentinel 분리

async lifecycle은 한 `CyWork` 호출 안에서 끝나지 않는다. 따라서 TCP와 Control 사이에
wire에 노출되지 않는 별도 pending sentinel이 필요하다.

- 설계 예약: `LMC_ADMIN_SET_POSITION_PENDING = -13`
- `-13`은 response size, error id 또는 Admin detail이 아니다.
- TCP는 active request buffer/socket/session을 유지하고 queue head를 advance하지 않는다.
- response buffer를 전송하거나 `SendData`를 호출하지 않는다.
- callback endpoint disarm, session epoch 증가 또는 socket close를 수행하지 않는다.
- 다음 CyWork에서 같은 frozen context를 poll하며 request parser/Begin을 다시 실행하지 않는다.

기존 `LMC_ADMIN_SET_POSITION_CLOSE_WITHOUT_RESPONSE=-12`와 절대 합치지 않는다.
`-12`는 durable Armed 뒤 terminal commit/readback을 증명할 수 없는 exact `0x7D12`의
close fence다. `-13`은 정상적인 in-progress 상태다.

claim/native/stable proof 자체가 모호한 경우는 terminal commit 실패와 원인이 다르다.
future implementation은 이를 별도 quarantine-close internal result로 고정해야 하며
`-12` 의미를 조용히 확장하지 않는다. 숫자와 exact TCP consumer는 구현 tranche에서
static verifier와 함께 freeze한다.

## 6. 향후 RT exactly-once native bridge

### 6.1 exactly-once의 정확한 범위

이 설계에서 exactly-once는 **한 fresh durable intent의 live executor가 native call
site를 최대 한 번 통과**한다는 뜻이다. PLC power loss 뒤 실행 여부를 마법처럼
복원한다는 뜻이 아니다. crash 뒤 exact terminal이 없고 Armed만 있으면 결과는 계속
Indeterminate이며 재실행하지 않는다.

필수 구현 제약은 다음과 같다.

- native call site는 Admin SetPosition executor 전체에서 축 선택별 한 논리 지점만 둔다.
- replay, exact Armed, preflight reject, cancellation과 corrupt result는 native 0회다.
- native call 전 exact tuple과 ownership Active를 다시 확인한다.
- claim state를 먼저 확정하고, 같은 tuple의 `NativeCount=0`일 때만 call한다.
- call 직전에 `NativeCount`를 1로 전환하며 이후 어떤 path에서도 0으로 되돌리지 않는다.
- 같은 request/claim을 다시 보아도 stored executor state/result만 반환한다.
- PLC restart 뒤 volatile claim이 사라져도 retained Armed admission이 replay를 차단한다.

### 6.2 two-phase preflight와 claim

frozen preflight ABI는 claim slot을 0으로 남긴다. future native tranche는 별도 IDE-created
claim method 또는 versioned executor method를 추가해 아래 순서를 구현한다.

1. Control이 claim 0 preflight를 submit한다.
2. RT가 coherent `ready`, claim 0, native count 0을 publish한다.
3. Control이 reserved ownership tuple을 재검증하고 Active로 commit한다.
4. Control이 exact same key/generation에 대한 claim request를 publish한다.
5. RT가 safety/status/CAS/limit를 **다시 읽는다**. preflight ready snapshot을 재사용하지
   않는다.
6. RT가 exact tuple의 internal claim을 먼저 publish한다.
7. claim이 exact하고 `NativeCount=0`일 때만
   `SetPosition(LMCAXIS_SET_ACTPOS_APPUNIT_DEST, TargetPosition)`을 호출한다.
8. full `_LMCAXIS_CMDERROR`와 `NativeCount=1`을 보존한다.

claim request가 추가되기 전까지 mailbox slot 11과 result slot 29는 0이어야 한다.
이번 tranche의 existing Submit method에 숨은 claim 동작을 넣지 않는다.

### 6.3 3 stable samples

native return `0`은 final success가 아니다. RT executor는 native call 뒤 연속 세 번의
coherent RT scan에서 아래 값이 모두 같고 유효해야 success candidate를 publish한다.

- 같은 operation token, owner generation, Store record generation과 axis
- supported physical axis, `SimulateMode=0`, `Modulo=0`
- `MasterLock=0`, `DelayedMasterLock=0`
- active axis error 없음
- Standstill이며 actual/set velocity가 정확히 `0`
- actual application position과 set application position이 모두 target과 같음
- SW min < SW max이고 target이 범위 안
- executor claim exact, `NativeCount=1`, native command state `0`

한 값이라도 바뀌거나 sample이 incoherent하면 stable counter를 0으로 되돌린다. 세 sample은
서로 다른 RT observation cycle이어야 한다. evidence flags에 stable-3 proof가 없으면
Control은 Succeeded terminal을 commit할 수 없다.

native call 뒤 timeout, connection/state drift 또는 stable-3 미달은 "Rejected, native
0회"로 바꾸면 안 된다. terminal outcome을 확정하지 않고 retained Armed를 남기며
quarantine/close한다. operator가 현재 좌표를 보아 성공 또는 실패를 추정하지 않는다.

### 6.4 terminal mapping

- pre-native coherent rejection: `RecordState=Rejected`, applied 0, native state 0,
  기존 허용 detail `10,12,13,14,15` 중 exact 원인만 사용한다.
- native command reject: `RecordState=Rejected`, detail 11, `ErrorId=-6`, applied 0,
  native state nonzero.
- native state 0 + stable-3 complete: `RecordState=Succeeded`, applied=target,
  status/error/detail/native state 모두 0.
- post-claim ambiguity 또는 stable proof failure: terminal record를 만들지 않는다.

## 7. terminal durability, response와 release

Control은 RT result를 wire response로 직접 복사하지 않는다. exact Store key와
`RecordGeneration`으로 `CommitSetPositionTerminal`을 호출하고 반환된 68-byte snapshot을
원 request/result와 전부 대조한다.

필수 barrier는 다음과 같다.

`RT proven result -> retained marker-clear/body+CRC/marker-last/full readback -> exact snapshot
validation -> ownership release -> response formatting/send`

terminal commit/readback 전에는 ownership을 release하지 않는다. response도 보내지 않는다.
terminal proof 뒤 exact release가 실패하면 durable outcome은 보존하되 ownership table을
quarantine하고 정상 response를 보내지 않는다. reconnect 뒤 exact query/retirement로
outcome을 복구할 수 있지만 새 mutation은 ownership reconciliation 전까지 차단한다.

terminal commit/readback을 증명할 수 없으면 `-12`를 반환한다. TCP는 response 0회,
first-wins closed-session capture, callback disarm, session epoch roll, ingress/RPC fence와
socket close를 수행한다. Store scan에서 terminal이 실제 durable하면 이후 exact query가
이를 우선하고, terminal이 없으면 Armed/Indeterminate가 남는다.

## 8. crash 및 disconnect matrix

| Crash/fault 지점 | native 가능성 | durable 상태 | restart/reconnect 판정 |
|---|---:|---|---|
| syntax/identity 검증 전 | 0 | 없음 | 같은 caller가 새 request 준비 가능 |
| Begin의 첫 retained write 전 | 0 | 없음 | 새 admission 가능 |
| Armed marker-clear/body/marker write 중 | 0 | Blank/Incomplete/Corrupt 가능 | 자동 clear 금지; Store scan 결과로 detail 21/24 |
| Armed full readback 뒤, ownership reserve 전 | 0 | Armed | detail 20, 자동 replay 금지, quarantine |
| ownership reserved 뒤, RT submit 전 | 0 | Armed | detail 20; volatile owner reconcile 뒤에도 intent 재실행 금지 |
| preflight request/result publish 중 | 0 | Armed | incoherent result 폐기, detail 20 |
| preflight rejected 뒤 terminal commit 전 | 0 | Armed | 실제로 native 0이어도 retained evidence는 Indeterminate |
| preflight ready 뒤 ownership Active 전 | 0 | Armed | detail 20, replay 금지 |
| ownership Active 뒤 RT claim 전 | 0 | Armed | detail 20, owner quarantine/reconcile |
| claim publish 뒤 native call 전 | 0 | Armed | claim은 volatile이므로 외부에서 0회를 단정하지 않음 |
| native call 진입/return 중 | 0 또는 1 | Armed | outcome Indeterminate, retry 금지 |
| native return 뒤 stable sample 1~2 | 1 | Armed | outcome Indeterminate, 현재 좌표로 추론 금지 |
| stable-3 뒤 terminal marker 전 | 1 | Armed | outcome Indeterminate |
| terminal commit/readback 중 | 0 또는 1 | Armed/terminal/Corrupt 가능 | `-12`, no response, rescan/query precedence 적용 |
| terminal full readback 뒤 release 전 | 0 또는 1 | exact terminal | exact query/replay로 outcome 복구; native 재실행 0 |
| terminal 뒤 ownership release 실패 | 0 또는 1 | exact terminal | outcome은 query 가능, mutation은 quarantine |
| response send 중/직후 socket loss | 0 또는 1 | exact terminal | exact retry는 stored response, native 0회 |
| retirement tombstone commit 중 | 0 | original terminal 보존 | rescan 후 terminal/tombstone; exact retirement retry 가능 |

pending 중 session close/cancel은 claim 0을 coherent하게 증명해도 Armed record를 지우지
않는다. 기존 retained ABI에 일반 cancel terminal이 없으므로 임의 detail을 만들어
Resolved 처리하지 않는다. claim 여부 또는 executor 상태가 모호하면 socket을 close하고
ownership을 quarantine한다.

## 9. retained Armed와 quarantine

Armed-only는 operator clear, position equality, warm reboot 또는 WPF 확인 버튼으로 해제하지
않는다. exact query는 detail 20, different intent는 detail 23을 유지한다. 이 문서 범위에
force-retire/reformat 명령은 없다.

quarantine은 다음 중 하나에서 설정한다.

- mailbox/result identity 또는 sequence corrupt
- preflight ready 뒤 ownership tuple drift
- claim/native count 불일치
- native call 뒤 stable terminal proof 부재
- terminal commit/readback 불확실
- terminal 뒤 exact ownership release 실패
- crash/restart에서 Armed가 있고 exact terminal이 없음

quarantine에서는 capability를 광고하지 않고 new SetPosition뿐 아니라 같은 axis와 충돌하는
ordinary mutation을 차단한다. read-only diagnostics와 exact outcome query는 허용하되,
recovery wire는 bit 5/7과 exact identity gate를 별도로 통과해야 한다.

## 10. task/core/priority activation proof

정적 Network에는 `_LMCAxis1.LMCPreRtWorkTrigger -> LMCEcatInputLatch1.ClassSvr`가 있고
axis 1..4 Control client가 연결돼 있다. 이 구조는 RT owner 후보를 정하지만 activation
proof는 아래를 모두 요구한다.

1. saved `Motion_Network.lcn`과 `ONE_Motion_Network_Table.st`에서 exact trigger와 axis
   1..4 client 연결을 대조한다.
2. LASAL IDE object/task view에서 `_LMCAxis1`과 `LMCEcatInputLatch1`이 실제로 같은 RT
   task chain에 있음을 캡처한다.
3. target runtime에서 task id, CPU core와 OS priority를 각각 읽어 기록한다.
4. native caller가 대상 axis RT thread와 같은 core이며 equal-or-lower priority라는 vendor
   조건을 runtime trace로 확인한다.
5. axis 2..4 호출도 axis 1 pre-trigger context에서 허용되는지 vendor contract와 runtime
   측정으로 증명한다. 증명되지 않으면 axis별 executor/trigger로 분리한다.
6. worst-case preflight, native call과 3-sample observer의 execution time/jitter를 측정하고
   RT deadline miss가 0인지 확인한다.

generated table의 task index `1`이나 class `RealtimeTask=true`만으로 2~6을 PASS 처리하지
않는다.

## 11. `SRAMRETAIN` 외부 검증

source의 `g_LMCSetPositionStoreWords : ARRAY [0..335] OF UDINT`는 1344 byte지만 이것만으로
retention은 성립하지 않는다. activation 전에 실제 PLC target에서 다음 외부 증거를 확보한다.

1. deployed `Autoexec.lsl`의 `SET SRAMRETAIN` 값을 원본 그대로 보존한다.
2. SetPosition 외 모든 retained consumer의 size, alignment와 address range를 inventory한다.
3. 전체 allocation이 `other retained consumers + 1344 bytes` 이상이며 영역이 겹치지
   않음을 memory map으로 확인한다.
4. 실제 target에서 cold power-off/on 뒤 exact Armed, Succeeded, Rejected와 tombstone record의
   CRC, marker, StoreGeneration/RecordGeneration이 유지되는지 확인한다.
5. marker-clear/body/CRC/marker-last 각 fault injection에서 Blank/Incomplete/Valid/Corrupt
   분류와 no-auto-clear를 확인한다.
6. allocation unset/부족/overlap build에서는 detail 24, retained write 0회, native 0회를
   runtime trace로 확인한다.

현재 이 증거는 없다. 따라서 Store macro를 `TRUE`로 바꾸거나 bit 3/5/7을 광고하면 안 된다.

## 12. WPF journal 및 capability gate

현재 [`AxisSetPositionRecoveryJournal.cs`](../../LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/AxisSetPositionRecoveryJournal.cs)는
`ArmedBeforeDispatch -> RecoveryRequired -> TerminalOutcomeObserved -> Resolved` evidence
lifecycle과 format 1/2 read, format 2 write를 구현한다. evidence-free `Resolve`는 없다.
그러나 MainWindow dispatch/interlock에는 연결되지 않았다.

향후 WPF 연결은 아래 gate를 분리한다.

### 12.1 새 mutation gate

- exact endpoint와 current `LMCConnection` owner/session
- fresh Admin capability의 Build/BootId/MapRevision
- bit 3, bit 5, bit 7 모두 ON
- SDK dependency `bit 3 => bit 5 + bit 7`
- physical axis count `1..4`, selected axis가 advertised mask에 포함
- `ErrorCatalogVersion >= 2`
- active SetPosition journal 없음
- same axis의 common ownership/ordinary mutation recovery interlock 없음
- prepared request의 target, expected actual, request id와 128-bit intent 고정
- journal `ArmedBeforeDispatch` durable write 완료 뒤에만 one-shot TCP write

### 12.2 recovery gate

- startup `ArmedBeforeDispatch`는 wire 없이 `RecoveryRequired`로 승격
- automatic SetPosition replay 0회
- outcome query는 bit 5만 요구하며 bit 3 OFF여도 old journal 복구를 허용
- retirement는 bit 7과 bit 5를 요구하며 bit 3은 요구하지 않음
- original Build/MapRevision과 endpoint는 exact match
- original BootId는 retained key로 보존하고 fresh current BootId는 별도 field로 전달
- query의 exact terminal snapshot과 nonzero generation을 journal에 먼저 영속화
- 같은 snapshot/generation의 typed `0x7D1A` success 뒤에만 `Resolved`
- capability/identity mismatch, NotFound, Indeterminate, corrupt, key mismatch 또는 RPC loss는
  journal bytes를 유지하고 new mutation을 zero-wire로 차단

normal `0x7D12` response를 받았더라도 terminal query와 retirement를 생략해 journal을
삭제하지 않는다. direct response loss 뒤에도 같은 recovery 절차 하나만 사용한다.

## 13. 검증 계획

### 13.1 이번 preflight-only tranche

- declaration의 exact method input order/type와 16/32 array size
- mailbox/result slot offset static verifier
- request payload-before-sequence, result payload-before-applied sequence
- exact retry, busy, sequence wrap, torn request/result와 identity mismatch matrix
- axis 1..4 exact mask와 invalid axis/mask/max-jump negative
- client disconnected, status/error/velocity/lock/simulate/modulo/SW limit/CAS/jump negative
- every ready/rejected result에서 claim/native count/native state 모두 0
- Admin capability/store/ownership macro와 native call count가 기존 inactive 값 유지
- changed custom source added lines의 7-bit ASCII 검사
- method-size threshold와 `git diff --check`
- IDE method direct-open, Rebuild/Link 및 새 `CInvalidArgException` 0

이번 tranche PASS는 PLC execution proof가 아니다.

### 13.2 future executor static/PC

- Store Begin replay가 ownership reserve보다 항상 앞서는 call-order verifier
- Begin result 2/0에서 reserve, mailbox, native 모두 0
- fresh Armed result 1에서만 reserve 한 번
- pending `-13`과 close `-12` exact TCP consumer 분리
- claim publication이 single native call site보다 앞서는 source-order verifier
- duplicate poll/retry/concurrent request에서 native count 최대 1
- three distinct observation-cycle stable samples 전 success 0회
- terminal full readback 전 response/release 0회
- crash matrix reference model과 11-stage retained fault injection
- WPF capability-off/identity mismatch/startup recovery zero-wire matrix

### 13.3 PLC/hardware activation

- activation build의 fresh BootId/MapRevision과 bit dependency
- task/core/priority runtime trace
- actual `SET SRAMRETAIN` map과 cold power-cycle retention
- axis 1..4 invalid state/CAS/jump/limit/lock/error negative capture
- axis 1..4 bounded zero/small correction, native count 1과 stable sample 3 capture
- claim 전/후, native 전/후, terminal commit 각 crash/fault injection
- `-12` response 0회와 exact reconnect query/retirement
- response-loss stored replay에서 native 추가 호출 0회
- actual request/response packet capture와 retained snapshot correlation

## 14. rollout 순서

1. **P0 preflight-only**: frozen ABI, atomic SPSC, observation-only, activation OFF.
2. **P1 Control async**: Store Begin precedence, fresh-Armed-only reserve, pending sentinel,
   no native.
3. **P2 RT claim/native**: two-phase claim, exactly-one live call, three stable samples,
   capability 계속 OFF.
4. **P3 durability/fault**: terminal-before-release/response, crash matrix, external SRAM과
   task/core/priority proof.
5. **P4 WPF recovery**: journal/interlock/capability gate, query/retirement E2E, replay 0.
6. **P5 activation**: approved max-jump, Store/ordinary macro, bit 3/5/7을 한 paired
   PLC/SDK release에서 전환하고 hardware regression을 수행.

각 phase는 이전 phase의 hash, build 및 negative matrix를 고정한 뒤 진행한다. P0 source가
존재한다는 이유로 P5 설정을 미리 바꾸지 않는다.

## 15. 완료 조건과 남은 사항

P0 preflight-only tranche는 frozen 16/32 ABI, exact numeric mapping, source/semantic hash,
focused `95/95`와 C78 Rebuild/Link checkpoint까지 닫혔다. 다음 항목은 후속 tranche다.

- Control async context와 `-13` TCP pending consumer 구현
- versioned claim method와 RT native executor 구현
- stable-3 terminal observer와 post-claim uncertainty quarantine 구현
- 실제 task/core/priority proof
- actual target `Autoexec.lsl` 및 전체 `SET SRAMRETAIN` proof
- WPF MainWindow journal/interlock 연결
- PLC download/runtime/hardware crash matrix

위 항목 전에는 current verified checkpoint를 다음처럼 기록한다.

> RT preflight ABI/source/static/IDE tranche PASS; `READY` is a coherent snapshot only.
> Admin SetPosition activation OFF, capability `0x00000017`, axis max-jump `0`, ordinary
> ownership OFF, Admin native SetPosition call `0`; no PLC download/runtime/hardware proof.

## 16. 관련 기준

- [`AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md`](AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md)
- [`SIGMATEK_LASAL_coding_rules.md`](SIGMATEK_LASAL_coding_rules.md)
- [`SIGMATEK_LASAL_programming_method_study.md`](SIGMATEK_LASAL_programming_method_study.md)
- [`SIGMATEK_LASAL_programming_error_prevention_guide.md`](SIGMATEK_LASAL_programming_error_prevention_guide.md)
- [`LMCControlCommandService.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st)
- [`LMCEcatInputLatch.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st)
- [`Motion_Network.lcn`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn)
- [`ONE_Motion_Network_Table.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/ONE_Motion_Network_Table.st)
- [`_LMCAxisBase.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_LMCAxisBase/_LMCAxisBase.st)
- [`LMCSetPositionStore.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSetPositionStore/LMCSetPositionStore.st)
- [`AxisSetPositionRecoveryJournal.cs`](../../LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/AxisSetPositionRecoveryJournal.cs)
- [`LmcAdminModels.cs`](../../LMC_Library/LMC_API_Delivery/src/LmcAdminModels.cs)

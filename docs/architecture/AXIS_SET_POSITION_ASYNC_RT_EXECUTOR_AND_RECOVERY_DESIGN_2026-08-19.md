# Axis SetPosition 비동기 RT executor 및 복구 설계

작성일: 2026-08-19

상태: P0 preflight source/static/IDE checkpoint 검증 완료; P1 Control/TCP
source/static/C78 artifact checkpoint 검증 완료; runtime activation OFF

> **2026-08-20 13:06 artifact override:** 마지막 IDE build의 `Classes.lcb`는
> `8,610,206` bytes / SHA-256 `568FE55148D734BE4DB0BB5ED9AF4D7800DB33672A5FCE21ECCFE15EE3CAC5A7`다.
> 아래 `33C1...49A8` ratchet과 main SourceOnly PASS는 11:34 historical checkpoint이며 current
> UDP VerifyCurrent/full SourceOnly는 physical identity drift에서 STOP한다. current release 판정은
> [API 개발 진척도](../api/API_DEVELOPMENT_PROGRESS.md)를 따른다. C78/PLC load 성공은 P1
> artifact 승인이나 SetPosition activation을 뜻하지 않는다.

## 1. 결론

`0x7D12 SetAxisPosition`의 다음 구현 단계는 TCP/Cyclic owner가 축 명령을 직접
실행하는 구조가 아니다. non-RT `LMCControlCommandService`가 retained Store와
ownership lifecycle을 소유하고, `_LMCAxis1.LMCPreRtWorkTrigger`로 호출되는
`LMCEcatInputLatch.RtWork()`가 RT preflight와 향후 native
`_LMCAxis.SetPosition()` 경계를 소유하는 비동기 구조로 분리한다.

마지막으로 닫힌 P0 tranche의 범위는 **RT preflight mailbox/result 경계만 만드는 것**이다.
P1 Control/TCP source/static/C78의 11:34 historical artifact checkpoint는 fresh C78 Rebuild/Link,
artifact rebaseline과 main SourceOnly까지 통과했다. 2026-08-20 13:06 current volatile image는
PLC link/download와 project load까지 성공했지만 `Classes.lcb` identity가 다시 바뀌어 current
artifact gate는 STOP 상태다. 이는 dirty working-tree checkpoint이며 clean release가 아니다.
BootId/MapRevision, task/core/priority와 hardware motion 증거도 아직 없다.
`ready` 결과는 "이 RT sample에서 preflight 입력과 관찰값이 일관됐다"는 뜻일 뿐,
SetPosition 성공, native 접수, 좌표 적용 또는 durable terminal outcome이 아니다.

2026-08-20 current source에서는 출하 PLC마다 `SET SRAMRETAIN`을 별도 설정해야 하는
배포 의존성을 제거하기 위해 1344-byte backing을 `VAR_GLOBAL RETAIN`에서 ordinary
`VAR_GLOBAL`로 전환한다. `ARRAY [0..335] OF UDINT` layout과 Store source는 유지하지만 이
backing은 **volatile**이다. 따라서 현재 build에는 power-off/restart durability, restart 뒤
stored replay, durable query 또는 retirement 보장이 없다. 아래 retained record/crash 계약은
향후 durable Store를 다시 설계할 때의 target contract이며 current runtime claim이 아니다.

이번 tranche에서는 다음 값이 모두 고정이다.

- `LMC_ADMIN_SET_POSITION_STORE_CONFIGURED=FALSE`
- `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED=FALSE`
- axis 1..4 `LMC_ADMIN_SET_POSITION_MAX_JUMP_AXIS*=0`
- Admin capability `0x00000017`; bit 3/5/7 OFF
- Admin SetPosition executor의 `claim=0`, `nativeCount=0`, `nativeState=0`
- Admin SetPosition 경로의 native `_LMCAxis.SetPosition()` call site 0개
- current volatile artifact tuple의 PLC download/project load 1회; hardware mutation 0회

향후 activation은 한 번에 열지 않는다. project-deployed durable Store 재설계, async
Control lifecycle, RT claim-before-native, 3-sample terminal proof, WPF durable journal과
capability gate를 모두 구현하고 검증한 뒤 별도 변경에서 macro와 capability를
전환한다.

기존 wire/retained 계약은 향후 durable target design으로
[`AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md`](AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md)를
따른다. current volatile backing은 그 durability를 충족하지 않는다. 이 문서는 그 계약을
RT 실행 경계와 crash/recovery 순서로 확장한다.

## 2. 확인된 현재 상태와 증거 경계

### 2.1 현재 source에서 확인한 사실

- [`LMCControlCommandService.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st)는
  `0x7D12`에서 syntax/identity를 검증하고 Store가 비활성이면 detail 24로 닫는다.
- 같은 source의 capability response는 `0x00000017`이다. 이는 axis/group read,
  group relative move 및 Axis Home만 광고하며 SetPosition bit 3, outcome read bit 5,
  retirement bit 7은 광고하지 않는다.
- [`LMCSetPositionStore.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSetPositionStore/LMCSetPositionStore.st)와
  [`global_LMCSetPositionStore.st`](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSetPositionStore/global_LMCSetPositionStore.st)에는
  1344-byte ordinary `VAR_GLOBAL` ledger와 Begin/Commit/Read/Retire source가 있다. 이
  ledger는 한 application run 안의 byte layout만 제공하며 전원 차단 또는 restart 뒤 값을
  보존하지 않는다. Store macro가 `FALSE`이므로 public execute/query/retire 경로는 detail 24로
  닫히고 이 volatile ledger도 runtime에서 사용되지 않는다.
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

### 2.2 current P1 편집 전 마지막 검증된 P0 checkpoint

아래 tuple은 RT preflight implementation까지 포함해 마지막으로 닫힌 2026-08-19 P0
checkpoint다. 현재 dirty P1 working tree의 tuple이 아니며, clean release 또는 PLC image
provenance도 아니다.

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

현재 P1 working tree에서는 Control/TCP source, verifier와 `Classes.lcb`가 위 P0 tuple에서
변경됐다. `HandleAdminSetPosition`은 private `ProcessAdminSetPositionAsync`에 한 번만
위임하며, 새 processor는 raw/LF `31,645`, CRLF `32,482` bytes다. current method-size gate는
6 classes / 108 methods / under-limit 105 / baseline debt 3 PASS다. P1 verifier harness는
async `84/84`, close-without-response `40/40` PASS하고 actual P1 focused gate도 PASS다. generated
private TCP helper ABI는 `Phase : UINT`, `Result : DINT`로 저장됐다.

첫 P1 C78 Rebuild는 `4 errors / 79 warnings`로 실패했다. 세 E0166은 `_memcmp()`의 `UDINT`
return을 두 `DINT` local에 대입한 문제였고, 한 E0041은 RESERVED
`ValidateAxisOwnershipIdentity()` 호출에서 `ExpectedAxisMask`보다 `RequiredPhase`를 먼저 쓴
ABI 순서 문제였다. 두 compare local을 `UDINT`로 바꾸고 호출을 generated declaration 순서로
수정했으며, verifier에도 exact local type/call order와 targeted mutation을 추가했다.
수정 뒤 fresh C78 Rebuild/Link는 `0 errors / 79 compiler warnings`, `Linker Done`이며 rebuild
시작 이후 새 `CInvalidArgException=0`이다. C78 output `Class/Classes.lcb`는 `8,610,206`
bytes, SHA-256
`33C1C2A68B97E852AD6646317CAE032A110D1F50C9615FA5B7EEF00410B649A8`이고 project
`.lcb`는 `634,865` bytes, SHA-256
`FE640A0683466FC1C68537A1CF5E9B96EEFBBBC5EE4885A78F25AF2557193A0E`다. 이 tuple은
ordinary `VAR_GLOBAL` 전환 뒤 2026-08-20 fresh Rebuild 산출물이다. UDP Gate D
VerifyCurrent와 self-test `336/336`가 PASS했고, actual P1 focused `84/84`, close `40/40`,
main SourceOnly도 `PASS LASAL.StaticContract.SourceOnly`, exit `0`으로 닫혔다.

현재 dirty P1 working tree에서도 PC-only 회귀는 SDK Debug/Release 각각 `1153/1153`, WPF
full smoke Debug/Release 각각 `356/356`, WPF `AxisSetPosition` filter 각각 `11/11` PASS다.
tracked C# diff는 없다. 이 결과는 C#/fake-RPC 계약만 증명하며 `.st` compile, PLC 또는
runtime 증거가 아니다.

### 2.3 증거 수준

| 수준 | 이 문서에서 인정하는 증거 | 현재 P1 판정 |
|---|---|---|
| PC | SDK build/test, WPF journal test, golden frame/parser | SDK `1153/1153`, WPF `356/356` PASS; MainWindow SetPosition dispatch 연결은 없음 |
| source/static | `.st`, generated declaration, Network/table, verifier | P1 actual focused `84/84`, close `40/40`, main SourceOnly exit 0, method budget `6/108/105/3` PASS |
| IDE/artifact | declaration/source compile, Rebuild/Link, 새 IDE log | volatile backing 반영 `Classes.lcb` `8,610,206` bytes, SHA-256 `33C1C2A6...0B649A8`; project `.lcb` `634,865` bytes, SHA-256 `FE640A06...193A0E`; C78 0 errors/79 warnings, Linker Done, 새 CInvalidArg 0 |
| PLC | download된 build/BootId, task/core/priority, SRAM map, runtime trace | 2026-08-20 13:06 `Linking at the PLC successful`, `Download Ok`, `SystemInit: OK`, `Project successfully loaded`; IDE 종료에 따른 정상 `go offline` 확인. BootId/MapRevision, fresh Salamander export, task/core/priority는 없음 |
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

## 5. P1 Control 비동기 lifecycle frozen contract

### 5.1 Store Begin precedence

Control은 ownership을 먼저 잡고 Store를 조회하면 안 된다. 정확한 순서는 다음이다.

| `BeginSetPosition` 결과 | 의미 | Control 동작 |
|---:|---|---|
| `2` | exact terminal/tombstone replay | stored 28-byte payload를 담은 36-byte complete frame만 반환, reserve/RT/native 0회 |
| `1` | 새 Armed durable commit/readback | 이 경우에만 internal ownership reserve로 진행 |
| `0`, detail 20 | exact Armed/Indeterminate | reserve/RT/native 0회, recovery 필요 |
| `0`, detail 21/23/24 | corrupt/occupied/unavailable | reserve/RT/native 0회, fail closed |
| negative | internal boundary failure | wire success 금지, mutation 진행 금지 |

Store의 volatile Begin/Commit transaction state는 fresh Armed부터 terminal Commit까지
유지한다. 같은 axis의 Query/Retire와 다른 Begin을 사이에 끼우지 않는다. PLC restart로
volatile transaction state가 사라지면 retained Armed만 남으므로 자동 replay하지 않는다.

### 5.2 lifecycle state와 Control context ABI

Control의 volatile context는 exact Store key, raw ownership identity, `RecordGeneration`,
session, request sequence, token/generation/axis mask, mailbox sequence, response socket과
deadline을 고정한다. 상태 숫자는 구현 편의에 따라 다시 배정하지 않고 아래 값으로
freeze한다.

| Value | Constant suffix | State | 진입 조건 | 허용 동작 |
|---:|---|---|---|---|
| `0` | `IDLE` | `Idle` | active context 없음 | 새 syntax/identity 검증 |
| `1` | `BEGIN_PENDING` | `BeginPending` | Store call 직전 | Begin 한 번 |
| `2` | `FRESH_ARMED` | `FreshArmed` | Begin result `1` exact readback | ownership reserve 한 번 |
| `3` | `OWNERSHIP_RESERVED` | `OwnershipReserved` | exact reserved tuple | preflight Submit 한 번 |
| `4` | `PREFLIGHT_PENDING` | `PreflightPending` | coherent request published 또는 exact completed tuple 확인 | Copy result polling만 수행 |
| `5` | `PREFLIGHT_READY` | `PreflightReady` | exact `ready`, claim/native 모두 0 | RESERVED ownership 재검증 및 Active commit |
| `6` | `OWNERSHIP_ACTIVE` | `OwnershipActive` | `CommitAxisOwnership` exact success | P1에서는 `-13` 유지, P2에서만 claim publish |
| `7` | `EXECUTION_PENDING` | `ExecutionPending` | P2 claim request published | RT terminal/result polling만 수행 |
| `8` | `TERMINAL_COMMIT_PENDING` | `TerminalCommitPending` | coherent pre-native reject 또는 proven RT terminal | Store terminal Commit 한 번 |
| `9` | `TERMINAL_PROVEN` | `TerminalProven` | exact terminal commit/full readback | exact ownership release와 response 허용 |
| `10` | `QUARANTINED` | `Quarantined` | context/claim/native/terminal 증거 모호 | response/replay 금지, socket close, Armed 보존 |

구현 constant의 full prefix는 `LMC_ADMIN_SET_POSITION_ASYNC_STATE_`다. `ExecutionPending=7`은
P2 전용이고 P1 source에서는 진입하지 않는다. stored replay는 이 lifecycle에 진입하지
않는다.

#### 5.2.1 `AxisSetPositionAsyncState` exact slot freeze

`AxisSetPositionAsyncState : ARRAY [0..127] OF DINT`는 정확히 512 byte다. 모든 slot은
DINT storage이며 unsigned 값은 `$UDINT` overlay로 읽고 쓴다. context가 없을 때는 128
slot 모두 0이고 `Lifecycle=Idle`이다. active context magic은
`LMC_ADMIN_SET_POSITION_ASYNC_MAGIC=0x53504131`이다.

| Slot | Field | 규칙 |
|---:|---|---|
| `0` | `Magic` | active context에서 exact `0x53504131`; 초기화/clear 때 `0` |
| `1` | `Lifecycle` | 위 numeric state `0..10` |
| `2` | `StoreRecordGeneration` | fresh Armed readback의 nonzero generation |
| `3` | `CallerSessionEpoch` | nonzero frozen caller session |
| `4` | `RequestSequence` | nonzero frozen TCP request sequence |
| `5` | `OperationToken` | ownership reserve가 반환한 nonzero `AdmissionToken`과 정확히 동일 |
| `6` | `OwnerGeneration` | ownership reserve가 반환한 nonzero generation |
| `7` | `ExpectedAxisMask` | exactly `1 << (Reference-1)` |
| `8` | `ResponseSocket` | initial active request의 nonzero DINT socket identity |
| `9` | `StartTimeMs` | fresh Armed exact readback 직후의 `ops.tAbsolute` UDINT |
| `10` | `DeadlineMs` | duration constant `1000` |
| `11` | `Reference` | signed DINT `1..4` |
| `12` | `TargetPosition` | application-unit signed DINT |
| `13` | `ExpectedActualPosition` | application-unit signed DINT |
| `14` | `MaxJump` | frozen UDINT; activation OFF에서는 `0` |
| `15` | `MailboxSequence` | pending 중 `0`; first coherent Copy에서 result slot 10 nonzero 값을 한 번 고정 |
| `16` | `DetailCode` | terminal/detail candidate |
| `17` | `CommandStatus` | wire candidate `0` 또는 `1` |
| `18` | `ErrorId` | sign-extended wire I16 candidate |
| `19` | `AppliedPosition` | terminal applied position |
| `20` | `NativeCommandState` | P1에서는 반드시 `0` |
| `21` | `StoreResult` | last Begin/Commit result |
| `22` | `StoreDetail` | last Store detail |
| `23` | `EffectiveAxisMask` | reserve output; slot 7과 exact match |
| `24` | `OwnerReserveResult` | last reserve result |
| `25` | `OwnerValidateResult` | last exact identity validation result |
| `26` | `OwnerCommitResult` | last Active commit result |
| `27` | `OwnerRollbackResult` | terminal release 또는 quarantine result |
| `28` | `PreflightSubmitResult` | Submit result |
| `29` | `PreflightCopyResult` | last Copy result |
| `30` | `ContextCheck` | 아래 immutable metadata XOR check |
| `31` | `QuarantineReason` | 정상 path `0`; quarantine에서 exact internal reason |
| `32..43` | `OwnershipIdentity[0..11]` | `pRequestFrame+8`의 raw 48-byte request payload exact copy |
| `44..55` | `StoreKey[0..11]` | section 5.1의 normalized exact 48-byte Store key |
| `56..72` | `TerminalSnapshot[0..16]` | Store가 반환한 exact 68-byte snapshot |
| `73..104` | `PreflightResult[0..31]` | Copy가 반환한 exact 128-byte RT result |
| `105..113` | `WireResponse[0..8]` | terminal proof 뒤 만든 exact 36-byte response |
| `114` | `CurrentSessionEpoch` | current invocation scratch |
| `115` | `CurrentRequestSequence` | current invocation scratch |
| `116` | `CurrentResponseSocket` | current invocation scratch |
| `117` | `CurrentCommandId` | current invocation scratch |
| `118` | `CurrentReference` | current invocation scratch |
| `119` | `CurrentOuterAdmissionToken` | current invocation scratch; `0x7D12`에서는 `0` |
| `120` | `CurrentOuterOwnerGeneration` | current invocation scratch; `0x7D12`에서는 `0` |
| `121` | `InvocationCheck` | 아래 current invocation XOR check |
| `122..127` | `Reserved[0..5]` | 항상 `0`; nonzero면 quarantine |

`ContextCheck`는 다음 XOR의 exact UDINT 결과다.

```text
0x53504131 xor StoreRecordGeneration xor CallerSessionEpoch xor RequestSequence xor
OperationToken xor OwnerGeneration xor ExpectedAxisMask xor ResponseSocket$UDINT xor
StartTimeMs xor DeadlineMs xor Reference$UDINT xor TargetPosition$UDINT xor
ExpectedActualPosition$UDINT xor MaxJump
```

이 check는 `OwnershipReserved` 진입 때 nonzero token/generation을 받은 뒤 계산하고 이후
immutable하다. raw identity, Store key, result와 snapshot은 check로 축약하지 않고 사용
직전에 전체 byte를 대조한다. slot 15는 Submit ABI가 publication sequence를 output으로
반환하지 않으므로 pending 동안 0이다. first coherent Copy가 반환한 result slot 10이
nonzero이고 slots 0..9가 frozen tuple과 일치할 때만 slot 15에 한 번 기록한다. 이후 다른
sequence는 quarantine이다.

`QuarantineReason`은 wire/Store detail이 아닌 volatile internal evidence다.

| Value | 의미 |
|---:|---|
| `0` | quarantine 없음 |
| `1` | async context magic/check/reserved corruption |
| `2` | current invocation 또는 session/socket/request tuple drift |
| `3` | P1 1000 ms timeout |
| `4` | Submit/Copy/result integrity 또는 mapping failure |
| `5` | ownership reserve/validate/Active commit failure |
| `6` | durable terminal 뒤 ownership rollback/release failure |
| `7` | Store Begin/replay/terminal의 계약 밖 non-`-12` anomaly |

slots 114..121은 stable async context가 아니라 existing private handler에 global
`HandleRequest` caller identity를 전달하는 transient scratch다. `InvocationCheck`는
`CurrentSessionEpoch xor CurrentRequestSequence xor CurrentResponseSocket$UDINT xor
CurrentCommandId$UDINT xor CurrentReference$UDINT xor CurrentOuterAdmissionToken xor
CurrentOuterOwnerGeneration`이다. 별도 magic/salt는 XOR하지 않으며
`LMC_SET_POSITION_ASYNC_INVOKE_MAGIC` 같은 legacy alias를 두지 않는다. global
`HandleRequest`는 Dispatch 직전에 여덟 slot을 쓰고
Dispatch가 반환한 즉시 성공/pending/error와 관계없이 모두 0으로 지운다. private handler는
사용 전에 check와 current command/reference를 검증한다. `0x7D12`는 outer ownership
adapter가 reserve하지 않으므로 slots 119와 120이 둘 다 0이어야 하며, P1 internal reserve가
반환한 token/generation은 stable slots 5와 6에만 기록한다.

#### 5.2.2 `ResponseSocket` call ABI

`LMCControlCommandService.HandleRequest`의 마지막 input은 아래 exact ABI를 사용한다.

```text
ResponseSocket : DINT
```

TCP는 매 호출에서 `ActiveRequest.Socket`을 그대로 전달한다. `0x7D12` fresh request와
continuation에서 exact 조건은 `ResponseSocket <> 0`이고 context slot 8 및 TCP pending marker와 정확히 같아야
한다. mismatch는 ordinary validation response가 아니라 `-14` quarantine-close다.
`ResponseSocket`은 wire/Store key/ownership raw identity의 일부가 아니며 Control이 socket
API를 직접 호출할 권한도 주지 않는다. 다른 command handler는 이 값을 의미에 사용하지
않는다.

#### 5.2.3 P1 exact transitions와 deadline

P1은 `BeginPending -> FreshArmed -> OwnershipReserved -> PreflightPending`을 한 번만
통과한다. `BeginSetPosition=2/0/negative`는 persistent async context를 만들지 않고 기존
Begin precedence대로 끝난다. fresh Armed 뒤 `StartTimeMs=ops.tAbsolute`,
`DeadlineMs=1000`을 고정한다. 매 continuation 시작에서 다음 UDINT subtraction을 사용한다.

```text
(ops.tAbsolute - StartTimeMs)$UDINT >= DeadlineMs
```

이 비교는 UDINT wrap에도 동일하다. deadline과 result가 같은 CyWork에서 관찰되면 deadline
검사를 먼저 적용한다. timeout은 reject terminal이 아니다. Store의 durable Armed와 volatile
Begin/Commit transaction을 소비하거나 지우지 않고 exact RESERVED 또는 ACTIVE owner를
quarantine하며 state 10과 `-14`로 끝낸다.

fresh Armed 뒤 ownership reserve 결과는 `0`만 success다. conflict `-2`를 포함한 모든
nonzero result는 synthetic detail 10 Rejected로 commit하지 않는다. Armed/transaction을
그대로 보존하고 `QuarantineReason=5`, state 10과 `-14`로 끝낸다.

coherent `ready`에서는 result slots 0..10, failure/detail, evidence와 claim/native 0을 모두
검증한 뒤 `PreflightReady=5`로 간다. Control은 raw 48-byte ownership identity까지 포함한
exact RESERVED tuple을 재검증하고 `CommitAxisOwnership`을 한 번 호출한다. exact success면
`OwnershipActive=6`으로 commit하고 P1에서는 `-13`을 계속 반환한다. `READY`에 멈추거나
claim request를 publish하지 않는다. P2 전까지 `PreflightResult[29..31]`, 즉
`AxisSetPositionAsyncState[102..104]`의 claim/native/result 값은 계속 0이다.

coherent pre-native `rejected`는 P1에서 terminal barrier까지 수행한다. 허용 mapping
`-4/10`, `-5/10`, `-6/14`, `-7/12`, `-8/13`, `-9/15`와 native 0을 전부 검증하고
`TerminalCommitPending=8`로 전이한다. exact Store key/generation으로 retained Rejected를
한 번 Commit하고 68-byte full readback을 전부 대조한 뒤에만 `TerminalProven=9`가 된다.
그 다음 exact `RollbackAxisOwnership(Reason=0)`이 success일 때만 stored fields로 36-byte
response를 slots 105..113에 만들고 caller buffer로 복사한다. release 실패는 durable
terminal을 보존하고 ownership을 quarantine한 뒤 `-14`; normal response는 0회다. Commit 또는
full readback을 증명하지 못하면 기존 `-12`를 반환하며 response/release는 0회다. 정상
response를 반환한 뒤 volatile context를 전부 0으로 지우며, response loss recovery는 Store
replay가 담당한다.

Store가 documented Begin/Commit result set 밖의 값을 반환하면 이를 success 또는 `-12`로
정규화하지 않는다. `QuarantineReason=7`과 `-14`로 close하며 mutation/release/response를 더
진행하지 않는다.

session close는 RESERVED와 ACTIVE 모두 ordinary rollback하지 않는다. exact owner를
quarantine하고 durable Armed, Store volatile transaction과 async context를 보존하며
`Quarantined=10`으로 전이한다. reconnect request를 자동 replay하거나 terminal reject로
바꾸지 않는다.

#### 5.2.4 Submit/Copy return handling

P1 consumer는 current frozen method return을 아래처럼 해석한다.

| Method result | P1 처리 |
|---:|---|
| Submit `1` | new publish 또는 exact tuple already pending; 재호출 없이 `PreflightPending` |
| Submit `0` | exact tuple result가 이미 complete; 재publish 없이 `PreflightPending`에서 Copy |
| Submit `-1/-2/-3` 또는 그 외 값 | Armed/transaction 보존, owner quarantine, `-14` |
| Copy `1` | exact pending 또는 bounded copy retry exhaustion; 다음 CyWork에서 Copy만 재시도하고 `-13` |
| Copy `0` | 128 bytes 전체와 frozen context를 검증한 뒤 READY/REJECTED 분기 |
| Copy `-1/-3` 또는 그 외 값 | Armed/transaction 보존, owner quarantine, `-14` |

Copy 자체가 확인하는 token/generation triple만으로 결과를 승인하지 않는다. slots 0..10,
state/failure/detail/evidence와 slots 29..31을 Control이 다시 exact 검증한다. `state=3`, torn,
reserved nonzero, identity mismatch 또는 impossible mapping은 모두 `-14`이고 Rejected terminal로
commit하지 않는다.

### 5.3 TCP pending sentinel 분리

async lifecycle은 한 `CyWork` 호출 안에서 끝나지 않는다. 따라서 TCP와 Control 사이에
wire에 노출되지 않는 별도 pending sentinel이 필요하다.

- 설계 예약: `LMC_ADMIN_SET_POSITION_PENDING = -13`
- `-13`은 response size, error id 또는 Admin detail이 아니다.
- TCP는 active request buffer/socket/session을 유지하고 queue head를 advance하지 않는다.
- response buffer를 전송하거나 `SendData`를 호출하지 않는다.
- callback endpoint disarm, session epoch 증가 또는 socket close를 수행하지 않는다.
- 다음 CyWork에서 같은 frozen context를 poll하며 request parser/Begin을 다시 실행하지 않는다.

TCP의 canonical helper는 private `HandleAdminSetPositionPending`이며 exact ABI는
`VAR_INPUT Phase : UINT`, `VAR_OUTPUT Result : DINT`다. magic은
`LMC_ADMIN_SET_POSITION_PENDING_MAGIC=0x5350504E`다. phase는 `PREPARE=1`, `RETAIN=2`,
`TERMINAL=3`, `QUARANTINE=4`로 고정하며 full names는
`LMC_ADMIN_SET_POSITION_PENDING_PHASE_PREPARE/RETAIN/TERMINAL/QUARANTINE`이다. TCP 소비
구간은 `LMC_ADMIN_SET_POSITION_PENDING_BEGIN/END`와
`LMC_ADMIN_SET_POSITION_QUARANTINE_BEGIN/END` marker 쌍을 사용한다. Control은
`LMC_ADMIN_SET_POSITION_ASYNC_MAGIC`, `LMC_ADMIN_SET_POSITION_ASYNC_STATE_*`,
`LMC_ADMIN_SET_POSITION_PENDING`만 사용하며 `LMC_SET_POSITION_ASYNC_*` 또는
`LMC_ADMIN_SET_POSITION_ASYNC_PENDING` legacy alias를 남기지 않는다. Control의 private
`ProcessAdminSetPositionAsync`는 inputs `Reference : UINT`, `pRequestFrame : ^USINT`,
`RequestFrameSize : UDINT`, `pResponseFrame : ^USINT`, `ResponseCapacity : UDINT`와 output
`ResponseSize : DINT`를 사용한다. implementation의 lifecycle CASE는
`LMC_ADMIN_SET_POSITION_ASYNC_BEGIN/END` marker 한 쌍으로 감싼다. 기존
`HandleAdminSetPosition`은 이 private method를 정확히 한 번 위임 호출하고 lifecycle/Store/
ownership/mailbox call을 직접 소유하지 않는다.

기존 `LMC_ADMIN_SET_POSITION_CLOSE_WITHOUT_RESPONSE=-12`와 절대 합치지 않는다.
`-12`는 durable Armed 뒤 terminal commit/readback을 증명할 수 없는 exact `0x7D12`의
close fence다. `-13`은 정상적인 in-progress 상태다.

claim/native/context proof 자체가 모호한 경우는 terminal commit 실패와 원인이 다르다.
별도 internal result를 `LMC_ADMIN_SET_POSITION_QUARANTINE_CLOSE=-14`로 freeze한다. `-14`도
wire status/error/detail이 아니며 exact `0x7D12` TCP consumer만 처리한다. TCP는 response와
`SendData` 0회, first-wins closed-session capture, callback disarm, session epoch roll,
ingress/RPC fence 및 socket close를 수행한다. `-12`는 terminal commit/readback uncertainty,
`-14`는 context/ownership/preflight/timeout/session ambiguity이므로 서로 바꾸거나 합치지 않는다.

#### 5.3.1 logical active request marker

`ActiveRequest.Reserved`는 기존 safety pending/close state와 공유하지 않고 `0`을 유지한다.
SetPosition pending은 `ActiveRequest.PayloadData`의 payload 밖 unused tail에 아래 primary와
duplicate marker를 사용한다. safety pending tail은 byte offset 1252부터이므로 겹치지 않는다.

| Byte offset | Field | Byte offset | Duplicate field |
|---:|---|---:|---|
| `1152` | `Magic=0x5350504E` | `1180` | `MagicCopy=0x5350504E` |
| `1156` | `SessionEpoch` U32 | `1184` | `SessionEpochCopy` U32 |
| `1160` | `RequestSequence` U32 | `1188` | `RequestSequenceCopy` U32 |
| `1164` | `ResponseSocket` DINT | `1192` | `ResponseSocketCopy` DINT |
| `1168` | `CommandId` U16=`0x7D12` | `1196` | `CommandIdCopy` U16=`0x7D12` |
| `1170` | `Reference` U16=`1..4` | `1198` | `ReferenceCopy` U16=`1..4` |
| `1172` | `PayloadLength` U32=`48` | `1200` | `PayloadLengthCopy` U32=`48` |
| `1176` | `Check` U32 | `1204` | `CheckCopy` U32 |

각 check는 `0x5350504E xor SessionEpoch xor RequestSequence xor ResponseSocket$UDINT xor
PackedCommandReference xor PayloadLength`다. `PackedCommandReference`는 byte 1168 또는 1196의
4 bytes를 little-endian U32로 읽은 값이다. producer는 두 block의 magic을 0으로 만든 뒤
body/check, duplicate magic, primary magic 순서로 쓰며 primary magic을 마지막 publication으로
취급한다. consumer는 primary/duplicate 전 field, 두 check와 frozen `ActiveRequest` tuple을
모두 대조한다. mismatch는 `-14`다.

initial request를 `ActiveRequest`로 복사할 때 physical `QueueReadIndex`는 이미 정확히 한 번
advance한다. `-13`의 "queue head를 advance하지 않는다"는 이후 physical cursor를 다시
움직이지 않고 `ActiveRequestValid=TRUE`인 logical active request를 보존한다는 뜻이다.
다음 queued request는 이 logical request가 terminal, `-12` 또는 `-14`가 될 때까지 dispatch하지
않는다. terminal response, `-12` 또는 `-14` 소비 시 두 marker를 clear한다. marker가 valid한
continuation은 같은 request/context poll로만 진입하며 syntax parser, Store Begin, reserve와
Submit을 반복하지 않는다.

`ConnSocketInfo`의 same-peer takeover 또는 owner 교체는 이 logical active request를 즉시
`_memset`하지 않는다. primary/duplicate marker 중 하나가 publish되었거나 해당 tail 영역에
partial nonzero data가 있으면 `ActiveRequest`를 old session close 처리까지 보존한다. 다음
`CyWork`가 first-wins `PendingClosedSessionEpoch`으로 Control의 session-close quarantine/notify를
완료한 뒤에만 request와 marker를 지운다. 새 socket은 old `0x7D12`를 이어받거나 poll하지 않으며,
이 보존 규칙이 기존 safety-drain continuation 보존 조건과 함께 적용되어야 한다.

이 보존 조건은 exact `CommandId=0x7D12`, `PayloadLength=48`에서 primary/copy magic 또는
offsets `1152..1204`의 임의 nonzero DINT를 `ActiveRequest` clear 전에 검사한다. 결과를 기존
safety continuation 조건과 OR하고, takeover에서는 `PendingClosedSessionEpoch=0`일 때만 old
epoch를 first-wins로 기록한다. pending closed epoch가 남아 있는 동안 새 queue를
dequeue/dispatch하지 않는다. 다음 `CyWork` 순서는
`Diagnostics.NotifySessionClosed(oldEpoch)` ->
`ControlCommands.NotifyAxisOwnershipSessionClosed(oldEpoch)` -> exact old request epoch일 때만
`ActiveRequest` clear -> 마지막 `PendingClosedSessionEpoch:=0`이다. partial tail은 continuation
승인이 아니라 old-session cleanup evidence이며 새 socket이 poll하지 않는다.

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
quarantine하고 `-14`로 close하며 정상 response를 보내지 않는다. reconnect 뒤 exact query/retirement로
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
않고 Store transaction과 async context도 소비하지 않는다. 기존 retained ABI에 일반 cancel
terminal이 없으므로 임의 detail을 만들어 Resolved 처리하지 않는다. RESERVED 또는 ACTIVE
owner를 quarantine하고 `-14`로 socket을 close한다.

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

## 11. durable Store 재설계와 외부 검증

LASAL CLASS 2 도움말의 `Ram Image` topic은 `VAR_GLOBAL RETAIN`이 별도 SRAM 영역을 사용하며
반드시 `SET SRAMRETAIN <size>`로 구성해야 한다고 명시한다. 그러므로 이 선언을 유지하면서
PLC별 설정만 없애는 방법은 없다. current source는 이를 ordinary `VAR_GLOBAL`로 바꿔 배포
의존성을 제거했으며, 그 대가로 retention도 제거했다.

2026-08-20 후속 감사에서 file-backed `RamEx UseFile=1`은 production durability barrier로
채택하지 않기로 했다. `SetDataAt` 성공은 enqueue 성공이고, `SRamFileAsyncInfo`는 request별
write result를 주지 않으며, `GetDataAt`은 physical file reopen readback이 아니기 때문이다.
현재 선택한 다음 설계는 `_FileSys` rev1.20 위의 fixed 2 x 2,048-byte A/B file backend다.
inactive file committed full write, request별 completion, close/reopen/full readback, CRC와
generation/marker complement 검증을 모두 통과한 뒤에만 성공을 반환한다. exact layout,
host-side factory deployment/readback 및 runtime commit 계약은
`docs/api/design/SET_POSITION_DESIGN.md`가 정본이다.

다음 두 경로는 이 고빈도 multi-record journal의 대안으로 채택하지 않는다.

- `RamFile`: tracked vendor class revision 1.9 자체가 `_old`로 이동했으며 `RamEx`와
  `UseFile=1` 사용을 지시한다.
- `Retentive=File` server: 공식 도움말상 server 하나는 4-byte 값만 보존하고 file write는
  비동기이며 flash 수명 제약이 있다. 336개 scalar 사이의 transaction ordering/atomicity를
  보장하는 계약도 확인되지 않았다.

`_FileSys` A/B durable Store를 구현하기 전 activation 증거는 다음과 같다.

1. current `VAR_GLOBAL` backing에서 restart durability/replay/query/retire를 주장하지 않는다.
2. Store macro `FALSE`, capability bits 3/5/7 OFF, native call 0을 유지한다.
3. future `_FileSys` client/backend object와 file path가 project download로 배포되는지 확인한다.
4. cold power-off/on 뒤 exact Armed, Succeeded, Rejected와 tombstone의 CRC, marker와 generation이
   유지되는지 확인한다.
5. marker-clear/body/CRC/marker-last fault injection에서 Blank/Incomplete/Valid/Corrupt 분류와
   no-auto-clear를 확인한다.
6. file operation incomplete, storage full/corrupt 및 restart 각 경우 detail 24, durable mutation
   0회와 native 0회를 runtime trace로 확인한다.

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

### 13.2 P1/P2 static/PC verification plan

- Store Begin replay가 ownership reserve보다 항상 앞서는 call-order verifier
- Begin result 2/0에서 reserve, mailbox, native 모두 0
- fresh Armed result 1에서만 reserve 한 번
- pending `-13`, terminal uncertainty `-12`, quarantine-close `-14` exact TCP consumer 분리
- `Reserved=0` logical active marker의 primary/duplicate tuple, no-send/no-close와 cursor 불변
- 1000 ms wrap-safe timeout과 session/socket/request drift의 Armed/transaction 보존
- READY 뒤 exact Active commit, claim/native/response/release 0과 `-13` 유지
- coherent pre-native REJECTED의 terminal full readback 전 response/release 0
- claim publication이 single native call site보다 앞서는 source-order verifier
- duplicate poll/retry/concurrent request에서 native count 최대 1
- three distinct observation-cycle stable samples 전 success 0회
- terminal full readback 전 response/release 0회
- crash matrix reference model과 11-stage retained fault injection
- WPF capability-off/identity mismatch/startup recovery zero-wire matrix

### 13.3 PLC/hardware activation

- activation build의 fresh BootId/MapRevision과 bit dependency
- task/core/priority runtime trace
- project-deployed `_FileSys` A/B store와 cold power-cycle retention
- axis 1..4 invalid state/CAS/jump/limit/lock/error negative capture
- axis 1..4 bounded zero/small correction, native count 1과 stable sample 3 capture
- claim 전/후, native 전/후, terminal commit 각 crash/fault injection
- `-12` response 0회와 exact reconnect query/retirement
- response-loss stored replay에서 native 추가 호출 0회
- actual request/response packet capture와 retained snapshot correlation

## 14. rollout 순서

1. **P0 preflight-only**: frozen ABI, atomic SPSC, observation-only, activation OFF.
2. **P1 Control async**: Store Begin precedence, fresh-Armed-only reserve, pending/quarantine
   sentinel, READY-to-Active와 coherent pre-native Rejected의 terminal/release/response barrier;
   no native.
3. **P2 RT claim/native**: two-phase claim, exactly-one live call, three stable samples,
   capability 계속 OFF.
4. **P3 post-claim/native durability/fault**: native/claim 뒤 terminal-before-release/response,
   crash matrix, `_FileSys` A/B storage와 task/core/priority proof. P1 pre-native Rejected barrier를
   다시 구현하거나 완화하지 않는다.
5. **P4 WPF recovery**: journal/interlock/capability gate, query/retirement E2E, replay 0.
6. **P5 activation**: approved max-jump, Store/ordinary macro, bit 3/5/7을 한 paired
   PLC/SDK release에서 전환하고 hardware regression을 수행.

각 phase는 이전 phase의 hash, build 및 negative matrix를 고정한 뒤 진행한다. P0 source가
존재한다는 이유로 P5 설정을 미리 바꾸지 않는다.

## 15. 완료 조건과 남은 사항

P0 preflight-only tranche는 frozen 16/32 ABI, exact numeric mapping, source/semantic hash,
focused `95/95`와 C78 Rebuild/Link checkpoint까지 닫혔다. 다음 항목은 후속 tranche다.

P1 Control async context, `-13` TCP pending consumer와 pre-native terminal barrier의 현재
source/static/C78 artifact checkpoint도 닫혔다. P1 actual focused `84/84`, close `40/40`,
method-size `6/108/105/3`, fresh C78 Rebuild/Link와 main SourceOnly가 모두 PASS했다.
이 판정은 PLC/runtime/native 실행 PASS가 아니다.

- versioned claim method와 RT native executor 구현
- stable-3 terminal observer와 post-claim uncertainty quarantine 구현
- 실제 task/core/priority proof
- `_FileSys` A/B object/network, request별 completion, reopen/readback 및 cold power-cycle proof
- WPF MainWindow journal/interlock 연결
- PLC download/runtime/hardware crash matrix

현재 verified checkpoint는 다음처럼 기록한다.

> P1 Control/TCP source/static/C78 artifact checkpoint PASS; `READY` is a coherent pre-native
> snapshot only.
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

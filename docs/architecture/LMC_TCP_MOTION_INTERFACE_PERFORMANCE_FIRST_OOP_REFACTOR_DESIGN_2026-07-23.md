# TCPMotionInterface 성능 우선 OOP 분리 설계

- 작성일: 2026-07-23
- 대상: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- 상태: Phase 5 transport-only 외부 text cleanup 적용. `TCPMotionInterface` generated
  server/client/data count는 `4/3/0`, 구현 함수는 8개이고 Diagnostics route는
  `MsgPaser`에 inline됐다. TCP direct axis/robot 연결 10개를 `.lcn` text에서 제거하고
  `ONE_Comm_Network_Table.st` external connection text를 26개에서 16개로 줄였다. tracked
  `Classes.lcb`/`Networks.lcb`도 transport-only registration과 network tuple 계약을 만족해
  switch 없는 `Phase5TransportClean` SourceOnly/full static이 PASS했다. 2026-07-30 current
  worktree는 `Phase5TransportClean / IntegratedReadOwnerDormant`, `ExpectedSdoWriteAxis=1`
  SourceOnly/full static, PC Debug/Release 1006/1006와 개발 WPF
  Debug/Release 278/278을 통과했다. fresh LASAL IDE Rebuild/Link는 `0 errors / 20 warnings`,
  Linker Done이고 변경 implementation 직접 open smoke도 성공했다. 현재 IDE PID의
  `CInvalidArgException`은 0건이다. 이 checkpoint는 `0x7E11/12/13/22` route, coherent
  464-byte snapshot과 CREVIS read-owner wiring을 포함하지만 bits 15~17은 OFF다. `0x7E23`
  PLC route도 없고 PLC download/runtime은 아직 검증하지 않았다
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

current C# protocol inventory 62개와 LASAL dispatcher route 61개를 다음처럼 구분한다.
capability-advertised active route는 53개이고, dormant read-owner 2개와
reserved/dormant 6개가 더해진다. `0x7E23`은 C# contract에만 있고 LASAL route가 없다.

| 소유자 | family | command IDs | 수량 |
|---|---|---|---:|
| Transport | lifecycle | `0x8080`, `0x405C`, `0x405D` | 3 |
| Control | Admin general | `0x7D00`, `0x7D10` | 2 |
| Control | Group-domain Admin | `0x7D20`, `0x7D22` | 2 |
| Control | registry/info | `0x103C`, `0x1042`, `0x202B` | 3 |
| Control | axis | `0x2022`, `0x2023`, `0x2024`, `0x2028`, `0x202E`, `0x209F`, `0x20A0`, `0x20A2` | 8 |
| Control | group | `0x20D2`, `0x2047`, `0x2048`, `0x2049`, `0x204A`, `0x204B`, `0x2085`, `0x20A4`, `0x2045`, `0x2051`, `0x20E7` | 11 |
| Diagnostics | capability-advertised active family | `0x7E00`~`0x7E51`의 active 24개 ID | 24 |
| Diagnostics | dormant read owner | `0x7E13`, `0x7E22` | 2 |
| Diagnostics | reserved/dormant | `0x7E21`, `0x7E51`, `0x7E4A`~`0x7E4D` | 6 |
| LASAL route 합계 |  |  | 61 |
| C#-only | LASAL route 없음 | digital output write `0x7E23` | 1 |
| C# protocol ID 합계 |  |  | 62 |

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

### 8.1 2026-08-05 custom method-size debt ratchet

현재 6개 custom service class의 qualified implementation `93`개를 raw/LF/all-CRLF UTF-8로
전수 계산한다. vendor/framework class는 이 debt ledger에 섞지 않는다. 새 method는 세 크기 중
하나라도 `32768` 이상이면 실패한다. 기존 초과 method는 아래 7개만 baseline debt로 인정하되,
어느 크기 차원도 현재 baseline보다 증가할 수 없다. 분할로 줄어들거나 사라지는 것은 허용한다.

| Class | Method | raw | LF | all-CRLF |
|---|---|---:|---:|---:|
| `LMCControlCommandService` | `ReserveAxisOwnership` | 79880 | 77732 | 79881 |
| `LMCRecorderStore` | `HandleRequest` | 75829 | 75249 | 77210 |
| `LMCEcatInputLatch` | `RtWork` | 73392 | 71906 | 73766 |
| `LMCControlCommandService` | `PublishAxisOwnership` | 65118 | 63444 | 65119 |
| `LMCControlCommandService` | `RollbackAxisOwnership` | 50103 | 48798 | 50104 |
| `LMCControlCommandService` | `PublishAxisOwnershipDs402Receipt` | 47506 | 46336 | 47507 |
| `LMCControlCommandService` | `PublishAxisOwnershipPreemptionCleanup` | 37128 | 36143 | 37129 |

`LMCControlCommandService.HandleAxisCommands`는 all-CRLF `32700` bytes로 여유가 `68` bytes뿐이다.
`HandleRequest`도 `32575` bytes다. verifier는 30000 bytes 이상 method를 내림차순으로 출력해
초과 전의 근접 위험도 보이게 한다.

검증기는
`LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1`이다.
direct self-test는 baseline shrink/removal 허용과 exact-threshold 신규 debt/baseline growth 거부를
포함해 `5/5`를 통과한다. current tree는 six classes, `93` methods, under-limit `86`, baseline debt
`7`을 확인한다. 전체 `Verify-LasalContract.ps1`도 이 ratchet을 호출하므로 별도 실행 누락으로
size gate를 우회할 수 없다.

2026-08-05 `ReserveAxisOwnership`의 미선언 `preemptRecordBase` 5곳을 같은 function에 이미 선언된
`probeRecordBase`로 교정한 뒤 current `LMCControlCommandService.st` SHA-256은
`C976CD364010EEFDFDDA8D7BC6D7655293DAD221FBEC908D50E5805CE4AFF072`다. 이 교정은 public ABI,
local 수, 호출과 write 순서를 바꾸지 않고 debt baseline만 세 차원에서 각각 10 bytes 줄였다.
아래 8.2~8.5의 whole-source planned SHA와 reverse-inline target
`ACCDD97A171A5D054F1115A7CDFA0B0C83FCF165FF59ED75E5C180D448C64AD3`은 이 P0 교정 전 계산
snapshot이다. 해당 split을 실제 적용할 때 current baseline으로 다시 계산해야 하며, current 승인값으로
재사용하지 않는다. 8.6의 Reserve 계획은 교정 후 source를 기준으로 다시 계산했다.

현재 pending Section 17 IDE handoff는 확장하지 않는다. 먼저 hidden channel 1개와 private helper
8개의 generated declaration, default SourceOnly, C78 baseline을 고정한다. 다음 별도 분할은 초과 폭이
가장 작은 `PublishAxisOwnershipPreemptionCleanup`부터 reverse-inline proof와 semantic negative
fixture를 갖춰 진행한다. 그 뒤 DS402 receipt, rollback, general publication, reservation 순서로
Control debt를 줄이고, RT `RtWork`와 Recorder `HandleRequest`는 독립 단계로 다룬다.

### 8.2 post-C78 preemption-cleanup split plan

이 절은 **미적용 계획**이다. current LASAL source, generated declaration과 Section 17 handoff에는
아래 helper가 아직 없다. Section 17 Save/inspection/default SourceOnly와 C78 baseline을 먼저 닫은 뒤
별도 IDE batch로 진행한다.

`PublishAxisOwnershipPreemptionCleanup`의 replacement tuple read-only validator만 아래 private helper로
분리한다. `GLOBAL` 또는 `VIRTUAL GLOBAL`로 만들지 않는다.

```text
ValidateAxisOwnershipPreemptionReplacement
  PreemptedAdmissionToken : UDINT
  PreemptedOwnerGeneration : UDINT
  OldCommand : DINT
  OldOwnerKind : DINT
  OldResourceKind : DINT
  OldIdentitySize : UDINT
  Result : BOOL
```

추출 범위는 current source의 `singletonToken := 0;`부터
`tupleValid := replacementValid;`까지다. adapter는 원 public input과 이미 한 번 검증·표본한 old tuple
값만 by-value로 넘기고 `tupleValid`에 helper 결과를 대입한다. helper는 `OwnershipState`와
`OwnershipIdentityState`를 기존 순서로 읽기만 하며 persistent write, clock/latch/client/SDO call과
live tuple 재표본을 하지 않는다.

current CRLF source에서 계산한 예상 크기는 다음과 같다.

- existing GLOBAL adapter: raw/LF/all-CRLF `29300/28500/29301`
- new private helper: raw/LF/all-CRLF `8486/8278/8487`

adapter의 normal commit은 계속 preemption-root magic clear로 시작하고 singleton/overlay/evidence를 쓴 뒤
root magic을 마지막에 복원한다. public Result `-3/-2/1/0` mapping도 adapter에 남긴다. in-memory
reverse-inline은 whole source를 byte-exact 복원했고 reconstructed SHA-256은 current source와 같은
`ACCDD97A171A5D054F1115A7CDFA0B0C83FCF165FF59ED75E5C180D448C64AD3`였다.

실제 적용 전에는 validation-prefix mutation 금지, safety/old/live tuple의 exact token-generation,
replay zero-mutation, incomplete bank retention, quarantine observer publication, commit-last와 Result domain을
focused negative fixture로 먼저 고정한다. 적용 후에는 size debt ledger에서 기존 GLOBAL debt를 제거하고
adapter/helper 각각을 일반 `<32768` hard gate로 승격한다.

2026-08-05 pre-split semantic fence는 적용 완료했다. verifier는 current public cleanup block의
`Ownership*State` mutation 26개와 publication 순서를 exact inventory로 고정하고, `:=`뿐 아니라
compound write도 검사한다. replacement old-token absence의 두 번째 9-axis loop와 quarantine observer
publication loop는 각각 독립 scope의 init/body/increment를 고정하며, cleanup 내부의 bank copy/clear와
clock/client 재표본도 거부한다.

- focused fixture `24/24` reject
- ownership aggregate `271/271` reject
- independent mutation review에서 처음 확인된 lease/startup state write, compound write, Axis 9 skip와
  quarantine-loop early exit 우회를 보강한 뒤 동일 변이 재검토 PASS
- custom method-size debt ratchet self-test `5/5` PASS
- five-waiver full `-SourceOnly -ExpectedSdoWriteAxis 1` PASS; six classes / `93` methods /
  under-limit `86` / unchanged baseline debt `7`
- 이 단계는 LASAL source, generated declaration, Network와 Section 17 handoff를 변경하지 않았으며
  위 private helper split도 여전히 미적용이다

### 8.3 post-C78 DS402 owner-receipt split plan

이 절도 **미적용 계획**이다. `PublishAxisOwnershipDs402Receipt`의 현재 public ABI와 mutation
순서를 먼저 semantic fence로 고정했으며, LASAL source, generated declaration, Network와 Section 17
handoff는 변경하지 않았다. Section 17 Save/inspection/default SourceOnly와 C78 baseline을 닫기 전에는
아래 helper를 IDE에 선언하거나 source에 적용하지 않는다.

현재 public ABI는 다음 순서로 고정한다.

```text
PublishAxisOwnershipDs402Receipt
  AxisMask : UDINT
  AdmissionToken : UDINT
  OwnerGeneration : UDINT
  ReportKind : UINT
  ReportValue0 : UDINT
  ReportValue1 : UDINT
  ObservationCycle : UDINT
  pDs402State : ^void
  Ds402StateSize : UDINT
  Result : DINT
```

pre-split verifier는 comment/string을 제거한 provider에서 외부 `pState` typed write,
`Ownership*State` write, persistent destination `_memset/_memcpy`를 실행 순서대로 수집한다. current
inventory는 정확히 `77`개, whitespace 제거/invariant lowercase/`|` join 길이는 `3547`, UTF-8
SHA-256은 `95A9EAF512D0F4DCB5B406F2FB8B1B433A420A8C729C722AF3BC7C41B93388BA`다.
Result assignment와 `RETURN;` token inventory는 `62`개, joined 길이 `621`, SHA-256
`DD95D7B15C57C4E768882CDE310B69FF581EB54390DEB9D87B982E09DAD3AA59`로 별도 고정한다.
모든 focused assertion 뒤에는 comment/string/whitespace/case를 제외한 provider 전체 semantic token
길이 `35988`, SHA-256
`3744AF0E5470B753EB12EAA3301FD7BB94F38350ED523B732F81827D8184D4E4` ratchet을 둔다. 이는 알려진
의미 assertion을 대체하지 않고 아직 모델링하지 않은 branch/call/control-flow drift를 마지막에 막는다.
단순 hash에만 의존하지 않고 아래 의미를 별도로 검사한다.

- `pState : ^USINT` 단일 local pointer/단일 초기화, exact input-guard prefix, 허용 call histogram,
  입력/identity validation 전 persistent publication 금지와 client/clock 재표본 금지
- receipt magic, PREPARED/CLEAR/COMPLETE/ROLLBACK phase와 Stage-87 magic/stage의 exact external ABI 값
- retained state, identity, observer, common record, singleton의 exact token/generation pair
- first/partial receipt의 `Validate -> magic -> PREPARED -> kind -> generation commit -> adoption clear`
- PREPARED와 COMPLETE replay의 단일 immediate-return branch
- identity, observer, singleton, record 순서의 retained phase publication과 root invalidation/body clear
- idle record magic 복원 뒤 COMPLETE를 마지막 persistent mutation으로 commit
- public Result exact sequence/domain `-3/-2/-1/0/1/2`, validator result 전달 한 곳과
  Result/RETURN control-flow inventory

기존 provider 음성 fixture `17`개에 early retained/direct-pointer write, token/generation 교차,
validation 전 magic과 validator-result 우회, pointer alias/reassignment, early return/input-guard bypass,
unexpected ownership call, trailing ABI input, replay mutation, clear/commit 역전, write-after-COMPLETE,
함수명과 external receipt ABI constant drift 등 `38`개를 추가했다.

- focused provider fixture `55/55` reject
- ownership aggregate `271/271` reject
- custom method-size debt ratchet self-test `5/5` PASS
- five-waiver full `-SourceOnly -ExpectedSdoWriteAxis 1` PASS; six classes / `93` methods /
  under-limit `86` / unchanged baseline debt `7`

post-C78에는 current Stage-87 tokenless recovery의 닫힌 always-return 분기 전체만 아래 private helper로
분리한다. `GLOBAL` 또는 `VIRTUAL GLOBAL`로 만들지 않는다.

```text
HandleAxisOwnershipDs402ReceiptStage87Recovery
  pState : ^USINT
  activeIndex : DINT
  AxisMask : UDINT
  ReportKind : UINT
  ReportValue0 : UDINT
  ReportValue1 : UDINT
  ObservationCycle : UDINT
  Result : DINT
```

helper는 `activeIndex`에서 `axisIndex`, `recordBase`, `recordByteBase`만 다시 계산하고, 이미 검증·표본한
`pState`와 by-value input만 사용한다. public `AdmissionToken`과 `OwnerGeneration`은 tokenless 분기에서
항상 0이므로 helper ABI에 넣지 않는다. 분기 안의 15개 explicit return, 16개 Result assignment와
Stage-87 `pState` mutation 순서는 그대로 helper가 소유하며 common ownership surface는 읽기만 한다.
normal durable receipt path는 adapter에 byte-unchanged로 남긴다.

current source를 대상으로 한 in-memory 계획 크기는 다음과 같다.

- public adapter raw/LF/all-CRLF `22783/22196/22784`
- private helper raw/LF/all-CRLF `26174/25523/26175`
- adapter replacement call block all-CRLF `299`

두 method 모두 `32768` 미만이다. helper를 public adapter 바로 앞에 삽입한 계획 source SHA-256은
`B587606ABFFF236C118C7FC9A999B8C804EEFB2B396CC84A8064D85CBB8ADA93`이며, helper를 제거하고 call을
원 Stage-87 branch로 reverse-inline하면 current Control source SHA-256
`ACCDD97A171A5D054F1115A7CDFA0B0C83FCF165FF59ED75E5C180D448C64AD3`을 byte-exact 복원한다.
실제 split 적용 시에는 parent와 helper의 transitive persistent-mutation inventory 및 call dominance로
baseline을 다시 승인해야 하며, parent inventory 감소만으로 PASS 처리하지 않는다.

### 8.4 post-C78 ownership rollback split plan

이 절도 **미적용 계획**이다. current `RollbackAxisOwnership` source와 public ABI를 먼저 전용
semantic fence로 고정했다. LASAL source, generated declaration, Network와 Section 17 handoff는
변경하지 않았다. Section 17 external inspection, default SourceOnly와 C78 baseline이 닫히기 전에는
아래 private helper를 IDE에 선언하거나 source에 적용하지 않는다.

current public ABI는 다음 exact order다.

```text
RollbackAxisOwnership
  AdmissionToken : UDINT
  OwnerGeneration : UDINT
  CallerSessionEpoch : UDINT
  RequestSequence : UDINT
  Reason : DINT
  Result : DINT
```

pre-split fence는 public declaration/implementation ABI, local pointer 부재, client/clock/custom helper
call 부재와 current call histogram `_memcmp=4`, `_memcpy=7`, `_memset=17`, `sizeof=6`,
`TO_DINT=4`, `TO_UDINT=32`를 고정한다. method가 사용하는 `47`개 `LMC_OWNER_*` define은 각각
단일 canonical value만 허용한다. `#include`, `#undef`, conditional directive, 동일 이름 재정의와
executable identifier macro alias도 금지해 source hash가 외부 또는 local preprocessor 주입으로
우회되지 않게 했다.

comment/string을 제거한 persistent destination write inventory는 정확히 `79`개다. whitespace 제거,
invariant lowercase, `|` join 길이는 `6251`, SHA-256은
`FFA826951AFAD84F64A21788ED0590330D5FA6A92C22B89A0363E03F9CF3BB08`이다. Result assignment와
`RETURN;` token inventory는 `29`개, joined 길이 `290`, SHA-256
`E03138AF05891034DAF1DFE79BAD9B3FB68B33E6D6730950068F622476E32A51`로 별도 고정한다. known
assertion 뒤의 whole-method semantic token ratchet은 길이 `36717`, SHA-256
`B997DB4BE547EF3EE07B4A2D2C8CAFC0588A1BACE65FF3A59D78C1F5E9AE2142`다.

- focused rollback fixture `38/38` reject, comment-only positive fixture accept
- ownership aggregate `271/271` reject
- five-waiver full `-SourceOnly -ExpectedSdoWriteAxis 1` PASS; six classes / `93` methods /
  under-limit `86` / unchanged baseline debt `7`
- current method raw/LF/all-CRLF `50103/48798/50104`, raw block SHA-256
  `2A88838417913B76449739447AAA8175157EAF8A370CC53F7FF916A3F25FF745`
- current Control source SHA-256
  `ACCDD97A171A5D054F1115A7CDFA0B0C83FCF165FF59ED75E5C180D448C64AD3`

post-C78에는 full preemption-bank read-only validation만 아래 private helper로 분리한다. `GLOBAL` 또는
`VIRTUAL GLOBAL`로 만들지 않는다.

```text
ValidateAxisOwnershipRollbackPreemptBank
  ExpectedAxisMask : UDINT
  pRestoreContext : ^void
  RestoreContextSize : UDINT
  Result : DINT
```

추출 범위는 current source의 `preemptBankValid := TRUE;`부터 그 validation block의 마지막
`if preemptBankValid = FALSE ... end_if;`까지다. 바깥 `if restorePreempt then`은 public adapter에
남긴다. helper는 persistent write, `_memset`, `_memcpy`, client/clock call 없이 retained state를 기존
순서로 읽고 `_memcmp` 세 번을 수행한다. 성공한 경우에만 adapter-local 40-byte context를 게시한다.

| UDINT slot | 의미 |
|---:|---|
| 0 | restored Group active 0/1 |
| 1 | mask |
| 2 | token |
| 3 | generation |
| 4 | session |
| 5 | sequence |
| 6 | identity size |
| 7 | command bit pattern |
| 8 | reference bit pattern |
| 9 | admission-mode bit pattern |

current source를 대상으로 한 in-memory 계획 크기는 다음과 같다.

- public adapter raw/LF/all-CRLF `30819/29996/30820`
- private helper raw/LF/all-CRLF `21654/21072/21655`
- extracted validation block raw/LF/all-CRLF `20372/19867/20372`, SHA-256
  `9A6EFE09CBE17D062802245E06974BF80AA7268D95489DEB8C137A0E1F68A62C`
- adapter call/map block all-CRLF `994` bytes

계획 source SHA-256은
`066335AF5FF84796B0888C08F46BAA932D7E6AAAB05275DF24F1C0B86353C1AD`다. helper/declaration/local과
call/map을 제거하고 원 validation block을 reverse-inline하면 current Control source SHA-256을
byte-exact 복원한다. 실제 split 때는 adapter와 helper의 합성 read/mutation/call/result contract를 새로
승인하고 기존 monolithic hash를 split-aware ratchet으로 교체한다.

이 분할은 size debt만 제거하며 새로운 durable rollback receipt를 추가하지 않는다. current rollback은
mutation 시작 뒤 전원 차단을 재개하는 journal이 없으므로 static invalidate-before-write/magic-last ordering이
crash recovery 증거는 아니다. 이 runtime 경계도 split 뒤 그대로 남으며 별도 설계 없이는 완료로 부르지 않는다.

### 8.5 post-C78 ownership publish split plan

이 절도 **미적용 계획**이다. current `PublishAxisOwnership` source, public ABI와 mutation/control-flow
순서를 전용 semantic fence로 먼저 고정했다. LASAL source, generated declaration, Network와 Section 17
handoff는 변경하지 않았다. Section 17 external inspection, default SourceOnly와 C78 baseline이 닫히기
전에는 아래 private helper를 IDE에 선언하거나 source에 적용하지 않는다.

current public ABI는 다음 exact order다.

```text
PublishAxisOwnership
  AxisMask : UDINT
  AdmissionToken : UDINT
  OwnerGeneration : UDINT
  ReportKind : UINT
  ReportValue0 : UDINT
  ReportValue1 : UDINT
  ObservationCycle : UDINT
  Result : DINT
```

pre-split fence는 public declaration/implementation의 exact ABI와 모든 same-name `FUNCTION` header
inventory, local pointer 부재, client/clock 재표본 금지, exact call histogram `_memcmp=3`, `_memcpy=2`,
`_memset=18`, `sizeof=4`, `TO_DINT=8`, `TO_UDINT=48`, `UpdateAxisRebaseRequiredState=1`을 고정한다.
method가 사용하는 `68`개 define은 각각 단일 canonical value만 허용한다. whole control source의 모든
hash-led preprocessor line은 `#define` 또는 exact-order `#pragma usingLtd _LMCAxis`,
`_LMCRobotBase`, `LMCEcatInputLatch` 세 줄만 허용하며 line splice도 금지한다. 따라서 include,
conditional, undef/redefinition, `#error`, line marker, 임의 pragma, executable-identifier macro와
form-feed/vertical-tab 주입을 semantic hash 밖에서 우회할 수 없다. 전체 define inventory도 `167`개,
joined length `6237`, SHA-256
`455C87BB8B4BEA396585B8EFD6A5D233FD7F5BB9A3B0870FAD8D3F7C62814B7F`로 고정해 macro-expanded
same-name header 주입을 막는다. 한 번의 leftmost lexical scan으로 string/comment delimiter 순서를
보존하고, exact class block 안의 public declaration, publish가 참조하는 persistent member array 8개,
세 pragma, generated table, macro region과 qualified implementation의 상대 위치도 고정한다.

- local declaration inventory `93`개, joined length `2162`, SHA-256
  `FF2AD6EE2FADB9C3C42C74D1BA671D477BAF7409907E99BBF1F7A021D24A19E5`
- direct/bulk persistent destination mutation inventory `83`개, joined length `4848`, SHA-256
  `86AC17C8D876F87826F98F9EE160F4711E02A6BC82327E07CE1D75C064F53B99`
- retained rebase helper call을 합치면 semantic mutation event는 `84`개다. helper 내부
  `AxisRebaseRequiredState` write/read-back도 split 뒤 transitive contract에 포함한다.
- Result assignment `24`개와 `RETURN;` `23`개의 합성 inventory는 `47`개, joined length `483`,
  SHA-256 `92C5659611A0A4F3B7086490BD2623B2370230D477DEBC76F267D5F9428F6CBD`
- whitespace-free whole-method semantic length `50046`, SHA-256
  `FB6B7BE724A2AA4091004890B40B2A11E9AF35471C23CCE9738CDABA9CDDDE16`
- comment/format 변화는 허용하되 token 경계를 보존하는 lexical inventory `9672`개, joined length
  `59717`, SHA-256 `59B54CCFBA25322103DA85FDF0C92C9BC8EEAA28026A2935B35C989CABEF785A`
- focused publish fixture `69/69` reject, comment-only positive fixture accept
- ownership aggregate `271/271` reject, integrated five-waiver full
  `-SourceOnly -ExpectedSdoWriteAxis 1` PASS; six classes / `93` methods / under-limit `86` /
  unchanged baseline debt `7`
- current method raw/LF/all-CRLF `65118/63444/65119`, raw block SHA-256
  `A0B44A036D46B32D8B85E95180B6A310EABF973B18D13A837155EB4B2FBD2985`
- current Control source SHA-256
  `ACCDD97A171A5D054F1115A7CDFA0B0C83FCF165FF59ED75E5C180D448C64AD3`

이 fence는 current semantics를 보존하는 근거이지 self-contained authorization 또는 crash-atomicity
증거가 아니다.

- LMC Home receipt는 retained state를 쓰는 **same-service-instance warm continuation**이다. cold restart
  durable journal이 아니다.
- 일반 multi-axis `clearOwner`, `restoreLease`, bank destruction은 단계 journal이 없고 replay-idempotent하지
  않다. group magic-last도 invalidation marker일 뿐 transaction recovery가 아니다.
- 함수 진입 자체는 table magic, BootId/startup proof, global corruption latch를 재검증하지 않고,
  command/owner/resource/admission mapping과 허용 current phase를 완전히 재분류하지 않는다. 안전성은
  Reserve/Commit과 production caller sequencing/whitelist를 전제로 한다.
- production call site는 `21`개이며 그중 `11`개는 `Result`를 소비하지 않는다. 특히 `-2` 뒤 retained
  owner가 남을 수 있으므로 이 호출들은 publish 완료 증거로 취급하지 않는다. caller-level result
  consumption은 별도 semantic debt다.
- `ReportKind=SAFETY_PREEMPT`는 current production caller가 `0`개다. `ObservationCycle=0`은 current
  production path에서 의도적으로 사용하므로 nonzero gate를 추가하지 않는다.

한 helper만으로는 current all-CRLF `65119` bytes를 안전하게 두 method ceiling 안에 분산할 여유가
`415` bytes뿐이다. closed extraction의 ABI/call/local overhead를 넣을 수 없으므로 post-C78 minimum은
아래 **private helper 두 개**다. 둘 다 `GLOBAL` 또는 `VIRTUAL GLOBAL`로 만들지 않는다.

```text
HandleAxisOwnershipPublishHomeReceipt
  AxisMask : UDINT
  AdmissionToken : UDINT
  OwnerGeneration : UDINT
  ReportKind : UINT
  ReportValue0 : UDINT
  ReportValue1 : UDINT
  ObservationCycle : UDINT
  Result : DINT

PrepareAxisOwnershipPublishDecision
  AxisMask : UDINT
  AdmissionToken : UDINT
  OwnerGeneration : UDINT
  ReportKind : UINT
  ExpectedSession : UDINT
  ExpectedSequence : UDINT
  ExpectedCommandId : DINT
  ExpectedReference : DINT
  ExpectedAdmissionMode : DINT
  ExpectedOwnerKind : DINT
  Result : DINT
```

첫 helper는 current retained Home receipt branch 전체를 소유한다. 진입 시 `Result:=2`를 두고 `2`만
general publish continue를 뜻한다. 기존 `-4/-3/0/1` 결과, Home state read/write와
`UpdateAxisRebaseRequiredState` 호출 순서는 그대로 유지한다. adapter는 public input guard 직후 정확히
한 번 호출하고 `Result<>2`이면 그대로 반환하며 state를 재표본하지 않는다.

둘째 helper는 preempt root/bank/special replacement 검증부터 restore-lease read-only 검증까지를
소유한다. 이미 표본·검증한 expected tuple을 by value로 받고, 성공 `Result`의 bit 0/1/2에 각각
`preemptRootValid`, `forceQuarantine`, `restoreLease`를 반환한다. 음수는 기존 error다. adapter는 이
결과를 local에만 매핑한 뒤 `destroyBanks` 계산부터 계속하며 두 validator가 끝나기 전 persistent main
commit을 시작하지 않는다.

current source를 대상으로 한 in-memory 계획 크기는 다음과 같다.

- public adapter raw/LF/all-CRLF `26899/26168/26900`
- Home helper raw/LF/all-CRLF `15395/15028/15396`
- decision helper raw/LF/all-CRLF `25416/24743/25417`
- Home extraction raw/LF/all-CRLF `14214/13892/14214`, SHA-256
  `3B9C74787829FDF51B1F7E3EF2F7DB4FE1519AC17A7C146C60B56E0785507E2D`
- decision extraction raw/LF/all-CRLF `23361/22769/23361`, SHA-256
  `E2E06E5ADBF2F526C765E893512D365C008A6AE9BA1C1494500BB1133D5D58A3`

세 method 모두 `32768` 미만이다. 계획 source SHA-256은
`B262BA287EDB31F9D88C421D4C2882E8FE69B52735E6410D81742251E5C88177`이다. 두 helper/declaration과
call/map을 제거하고 원 두 block을 reverse-inline하면 current Control source SHA-256을 byte-exact
복원한다. 실제 split 뒤에는 exact private ABI/one-call dominance, adapter+helper transitive
read/write/call/Result inventory, Home `Result=2` containment, decision bit `0..2` domain, no input/live tuple
재표본, first main commit dominance, reverse-inline proof와 size debt 제거를 다시 승인한다.

### 8.6 post-C78 ownership reservation split plan

이 절은 **미적용 계획**이다. 2026-08-05 P0 교정 뒤 current
`ReserveAxisOwnership`의 public ABI와 실행 의미를 전용 semantic/structural fence로 먼저 고정했다.
LASAL generated declaration, Network와 Section 17의 hidden channel 1개 + private helper 8개 handoff는
변경하지 않았다. Section 17 external inspection, default SourceOnly와 C78 baseline이 닫히기 전에는 아래
두 helper를 IDE에 선언하거나 tracked source에 적용하지 않는다.

P0 교정은 function 안에서 선언되지 않은 `preemptRecordBase` 5개 참조를 이미 선언되어 같은 record
base 의미로 사용되는 `probeRecordBase`로 바꾼 것이다. public/class ABI, local 수, call/write/result 순서는
변하지 않았다. 교정 뒤 current 값은 다음과 같다.

- `LMCControlCommandService.st` SHA-256
  `C976CD364010EEFDFDDA8D7BC6D7655293DAD221FBEC908D50E5805CE4AFF072`
- `ReserveAxisOwnership` raw/LF/all-CRLF `79880/77732/79881` bytes
- raw block SHA-256
  `4ABD82FF0BC73FA343F6D1ACFA0FA951FA09B12BD2E2DAD1D9D76621DA0B7BFC`

current public ABI는 다음 exact order다.

```text
ReserveAxisOwnership
  CommandId : UINT
  Reference : UINT
  RequestedAxisMask : UDINT
  OwnerKind : UINT
  ResourceKind : UINT
  AdmissionMode : UINT
  CallerSessionEpoch : UDINT
  RequestSequence : UDINT
  pIdentity : ^void
  IdentitySize : UDINT
  pEffectiveAxisMask : ^UDINT
  pAdmissionToken : ^UDINT
  pOwnerGeneration : ^UDINT
  Result : DINT
```

pre-split fence는 class/implementation의 exact thirteen-input/one-output ABI, qualified Control
implementation header 26개와 모든 lexical `END_FUNCTION` token의 strict alternating order를 고정한다.
따라서 function을 macro 앞, 가짜 이웃 사이, 다른 function 내부로 옮기거나 orphan 종료 token을 남기는
relocation을 허용하지 않는다. shared publish closure를 재사용해 exact class block, generated table,
three-pragma/macro/preprocessor inventory와 comment/string masking도 함께 고정한다.

- local declaration `81`개, joined length `1817`, SHA-256
  `55AC47497D5CA174A5837D094607699207F2AE8E6DA761C9F67ED86EF48BFF1`
- exact call histogram `_memcmp=4`, `_memcpy=7`, `_memset=8`,
  `CopyAxisOwnershipPreemption=2`, `ReadAxisRebaseRequiredMask=1`, `sizeof=7`, `TO_DINT=14`,
  `TO_UDINT=45`, `ValidateAxisOwnershipIdentity=2`; client/clock call은 `0`
- ambient clock sample은 `ops.tAbsolute` read exact `2`개만 허용
- persistent/output mutation `110`개, joined length `7306`, SHA-256
  `BBBDA1315DD5D184A3DB2F9CB55BE264022726743E8B4060B6FC9629D7609361`
- Result assignment sequence `56`개, Result/RETURN 합성 token `127`개, joined length `1246`,
  SHA-256 `5F438C3D4FEE2F1F024D9AA84025C21AB3C6CB69488B2D12556855B861686154`
- whitespace-free whole-method semantic length `60886`, SHA-256
  `9E0A14511F49B47D174CECC978749BAE5C8B4D42D5E934A020BEC2158322C85E`
- lexical token `11839`개, joined length `72724`, SHA-256
  `F13EDA75E7EFF379D407E88EC5CE2C37BA3445A3FED0C7D59B3DB9C53517246F`
- corruption latch write exact `9`개, lease/preempt/live/axis/group magic-last publication과 최종 output
  singleton/order를 고정
- focused reserve fixture `62/62` reject, comment-only positive fixture accept
- ownership aggregate `271/271` reject. `HandleRequest` 자체의 균형 잡힌 body-only semantic 변경은
  Reserve ABI/body/top-level availability를 바꾸지 않으므로 이 fence의 의도적 비범위다. 이를 동결하려면
  별도 `HandleRequest` semantic/lexical fence가 필요하다.
- latest integrated five-waiver
  `-SourceOnly -ExpectedSdoWriteAxis 1` PASS; six classes / `93` methods / under-limit `86` /
  unchanged baseline debt `7`, size self-test `5/5` PASS

current production caller는 모두 `TCPMotionInterface` 안의 exact 세 곳이다.

1. diagnostics common path는 DS402에서 `Result=0`만 native 진입을 허용한다. encoder `0x7E53`은
   Reserve 실패 뒤 diagnostics handler까지 호출될 수 있지만 downstream token/identity gate가 실행을
   fail-close한다. 이때 ownership 세부 `-2/-3`은 encoder detail `9`로 축약된다.
2. LMC current-position-zero Home은 `Result=0`만 허용하고 나머지는 detail `41/42`로 종료해 service를
   호출하지 않는다.
3. ordinary Axis/Group path는 음수만 거부하고 `0/+1/+2`를 repeated-safety helper에 전달한다. `+1`은
   native ACK 없이 종료하고 `+2`는 PowerOff escalation을 한 번 수행하는 current policy다.

한 helper만으로는 current all-CRLF `79881` bytes를 두 method ceiling 아래로 나눌 수 없다. helper 한
개의 이론적 수용량보다 추출해야 할 body가 overhead 전부터 `14347` bytes 크다. post-C78 minimum은
아래 **private helper 두 개**다. 둘 다 `GLOBAL` 또는 `VIRTUAL GLOBAL`로 만들지 않는다.

```text
ValidateAxisOwnershipReserveSurface
  CommandId : UINT
  Reference : UINT
  RequestedAxisMask : UDINT
  OwnerKind : UINT
  ResourceKind : UINT
  AdmissionMode : UINT
  InitialIdentitySize : UDINT
  EffectiveAxisMask : UDINT
  SafetyPreemption : BOOL
  pReferenceAxisMask : ^UDINT
  pIdentitySize : ^UDINT
  pIdentityPackedCommand : ^UDINT
  pIdentityPackedOwner : ^UDINT
  pRepeatRootPresent : ^BOOL
  pRepeatPreemptedToken : ^UDINT
  pRepeatPreemptedGeneration : ^UDINT
  pLeaseAvailable : ^BOOL
  Result : DINT
```

```text
PrepareAxisOwnershipReserveDecision
  CommandId : UINT
  Reference : UINT
  OwnerKind : UINT
  CallerSessionEpoch : UDINT
  pIdentity : ^void
  EffectiveAxisMask : UDINT
  ReferenceAxisMask : UDINT
  InputGroupLeaseTransition : BOOL
  InputGroupStopTransition : BOOL
  InputSafetyPreemption : BOOL
  InitialForceQuarantine : BOOL
  InputRepeatRootPresent : BOOL
  RepeatFound : BOOL
  RepeatRecordBase : DINT
  InitialRepeatPreemptedToken : UDINT
  InitialRepeatPreemptedGeneration : UDINT
  pForceQuarantine : ^BOOL
  pCleanupRequiredMask : ^UDINT
  pRepeatMode : ^DINT
  pRepeatAxisMask : ^UDINT
  pRepeatAdmissionToken : ^UDINT
  pRepeatOwnerGeneration : ^UDINT
  pRepeatPreemptedToken : ^UDINT
  pRepeatPreemptedGeneration : ^UDINT
  pExistingGeneration : ^UDINT
  pLeaseCapture : ^BOOL
  pPreemptCapture : ^BOOL
  pPreemptedGroupOwner : ^BOOL
  pExistingGroupActive : ^BOOL
  Result : DINT
```

첫 helper는 input/identity shape, reference/effective mask, existing-bank shape와 repeat-root surface를
검증한다. 둘째 helper는 selected owner/repeat/preemption/lease decision을 persistent publication 전에
완성한다. adapter와 helper는 기존 local `probeRecordBase : DINT`를 그대로 사용하며 새
`preemptRecordBase` 식별자를 도입하지 않는다. helper 출력 pointer capacity/alias는 LASAL ABI가 별도로
보장하지 않으므로 adapter가 exact local address만 전달하는 one-call contract로 고정한다.

current 교정 source를 대상으로 한 in-memory 계획 크기와 reverse proof는 다음과 같다.

- surface extraction lines `2549..3244`: raw/LF/all-CRLF `25718/25022/25718`, SHA-256
  `6DFC99CA7F5DA568F3877F46740FB81B9D83C8F1DC38B6BC8F5E1AD48042DA83`
- decision extraction lines `3359..4013`: raw/LF/all-CRLF `25325/24670/25325`, SHA-256
  `69777B21E4B20EE17A7E58501739EFCEBE7F9EED25B14A7FD625447BF7EAA7CA`
- planned adapter raw/LF/all-CRLF `31060/30216/31061`, raw block SHA-256
  `A2CAC3FCC5C9AACD08FCAA848229C1478BC2E8832A067C2BC9EE2D44A9AD5924`
- planned surface helper raw/LF/all-CRLF `28151/27372/28152`, raw block SHA-256
  `A6C50F93FB743C9374F917452E32DE0F2FC9213D531E2597D5BAFAAC17C70584`
- planned decision helper raw/LF/all-CRLF `29254/28471/29255`, raw block SHA-256
  `B15C844EFDD36CE3AFB709020765CF8EE95B29A2B60964FACF09736FC0592B5F`
- surface call/map all-CRLF `801` bytes, SHA-256
  `9ED45E39343E97D099CF9CE0B59FF98DF25B29A14F300AB45FAE4ECAE5869379`
- decision call/map all-CRLF `1357` bytes, SHA-256
  `6737F75586BFD5270C62D9E03074B7F3C00EC1498788BF6A563BF26D69F77D53`
- planned whole Control source SHA-256
  `88EFF5F607AB415834F9C9A86741D77CF2DCBBE69A73CFEB9B09B6EEF40A94C6`

두 helper declaration/local/call-map을 제거하고 두 원 block을 reverse-inline하면 current Control source
SHA-256 `C976CD364010EEFDFDDA8D7BC6D7655293DAD221FBEC908D50E5805CE4AFF072`를 byte-exact 복원한다.
실제 split 뒤에는 public adapter + 두 private helper의 합성 read/write/call/result inventory,
one-call dominance, persistent first-write dominance, pointer target singleton과 reverse-inline proof를 새로
승인하고 baseline debt를 제거한다.

이 split은 size debt만 제거한다. Reserve publication은 backup/main bank를 쓰지만 단일 durable
receipt/replay journal이 없고 warm reconciler도 전체 Reserve transaction replay가 아니다. magic-last는
fail-closed 구조 증거이지 crash-atomic proof가 아니다. caller session tuple과 identity blob도 인증된
보안 주체가 아니며 token/generation은 `2^32` wrap 뒤 `1`로 진행한다. 따라서 C78 compile, cold download,
same-BootId single-axis 실행, repeat/preemption 및 power-loss 증거 없이 reservation runtime 완료로 부르지
않는다.

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
- verifier/csproj에 `Phase5TransportClean`을 구현했다. current Axis 1 SDO Write source도
  `Classes.lcb`와 동기화되어 `-AllowStaleLasalBinaryMetadata` 없이
  `-TopologyIoCheckpoint IntegratedReadOwnerDormant -ExpectedSdoWriteAxis 1` SourceOnly/full
  static을 통과한다. coherent 464-byte snapshot, `0x7E11/12/13/22` handler와 CREVIS
  coupler/input/output network wiring도 이 checkpoint에 포함된다.
- 현재 Phase 5 worktree에서 PC Debug/Release 각 1006/1006 tests와 개발 WPF Debug/Release
  actual-control smoke 각 278/278이 PASS했다.
- PC response reader는 62개 protocol command ID 각각의 정상 최대 payload를 body read 전에 검사한다.
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
유지했다. tracked `Classes.lcb`/`Networks.lcb`도 이 정적 topology와 일치하며 2026-07-30
fresh LASAL reload/Rebuild/Link와 full static까지 PASS했다. 이 결과는 PLC download/runtime
증거가 아니다.

## 12. 검증과 승인 기준

### Phase 5 자동 검증

`Phase5TransportClean / IntegratedReadOwnerDormant` checkpoint는 구현됐다. current Axis1
SDO Write policy는 `ExpectedSdoWriteAxis=1`이며 SourceOnly와 switch 없는 full static이
모두 PASS한다.
`-AllowStaleLasalBinaryMetadata`는 중간 진단 옵션으로만 남기며 current final 결과에는
사용하지 않았다.

2026-07-30 current 검증 결과는 다음과 같다.

- SourceOnly/full `Phase5TransportClean / IntegratedReadOwnerDormant`: PASS
- PC Debug/Release: 각 1006/1006 PASS
- 개발 WPF Debug/Release smoke: 각 278/278 PASS
- fresh LASAL Rebuild/Link: `0 error(s), 20 warning(s)`, Linker `Done`
- implementation smoke: 기존 executor/service 검색과 latest 변경 implementation 직접 open PASS
- 현재 IDE PID `CInvalidArgException`: 0건
- `git diff --check`: PASS
- 위 결과는 current PLC download 또는 runtime Motion/Power/SDO Write/CREVIS dynamic read
  증거가 아니다. bits 15~17은 OFF이고 `0x7E23` PLC route는 없다

```powershell
powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass `
  -File "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" `
  -RepositoryRoot "." -SourceOnly `
  -ControlServiceCheckpoint Phase5TransportClean `
  -TopologyIoCheckpoint IntegratedReadOwnerDormant `
  -ExpectedSdoWriteAxis 1

powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass `
  -File "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" `
  -RepositoryRoot "." `
  -ControlServiceCheckpoint Phase5TransportClean `
  -TopologyIoCheckpoint IntegratedReadOwnerDormant `
  -ExpectedSdoWriteAxis 1
```

- current Phase 5 SDO Write source 기준 전체 C# request/parser tests Debug/Release 각
  1006/1006 PASS. SDK Write target policy, Read/Write-aware quarantine/cleanup과 성공 Write 뒤
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
  해제하지 않는다. `0x7E50` exact Int32/4-byte Write executor/API/WPF 경로는 구현됐고
  Axis1 `UI[24] 0x2F00:24`만 SDK/PLC global+per-axis gate와 SDK allowlist가 active다.
  Axis2..4, non-exact target, missing capability bit 9와 불충분한 identity/state는 fail-closed한다.
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

병행은 작업 흐름 기준이다. `TCPIPServer1.MaxConnections=2`는 same-peer reconnect
candidate를 임시 accept하기 위한 값이고 stable PLC motion owner는 하나다. 따라서 실제
PLC 송신 시험 두 개를 동시에 실행하지 않는다. PC/static 검증과 문서·capture
분석은 병행할 수 있지만 PLC write/motion 시험은 한 세션씩 직렬화한다.

2026-07-24 이후 LASAL 변경·시험 순서는 다음으로 고정한다. 개발 source는 main 저장소의
`Lasal_PRG/Elmo_EtherCAT_Test_4Axis`에서만 수정한다. 변경 준비 후 사용자가 main project를
빌드해 오류를 확인하고, 통과한 `Elmo_EtherCAT_Test_4Axis` 폴더만
`C:\work\Elmo\Elmo_Master_test`로 복사한 뒤 그 복사본에서 장비 시험한다. 개발 작업은
test 폴더를 수정하거나 자동 동기화하지 않는다.

| 흐름 | 다음 작업 | 같이 수행할 검증 | 완료 조건 |
|---|---|---|---|
| A. Phase 5 재검증 | current Phase 5 source/static/IDE 결과 보존 완료 | `Phase5TransportClean` SourceOnly/full, PC/WPF Debug/Release, current SDO Write declaration, Compiler/Linker와 implementation smoke PASS | current PLC download와 장비 결과 보존 대기 |
| B. legacy/신규 성능 비교 | pre-route `65f8000`을 배포해 legacy baseline을 얻고 같은 PLC/build 조건에서 routed source 재측정 | 1 ms cycle jitter/overrun, dispatch P95, throughput, RAM, 10,000회 이상 soak | 12절 성능 gate 충족 및 원시 로그 보존 |
| C. packet 회귀 | read-only/identity를 먼저 확인한 뒤 저속 Group command를 안전 순서로 실행 | 정상·잘못된 size/reference/mode, Power/Enable/Stop, disconnect/reconnect, response byte 비교 | 기존 golden과 byte/status 동일, 이중 실행·stale session 0 |
| D. Phase 5 IDE 확인 | source와 tracked metadata의 `4/3/0`, 함수 8개, TCP direct 연결 0개, network external 16개 정적 계약과 fresh Rebuild/Link PASS | implementation smoke와 source hash 보존 확인 완료 | IDE가 외부 구현을 보존하고 smoke까지 최종 구조를 수용함 — PASS |
| E. 9축 network | 새 `PosController5..9`와 `_LMCAxis5..9.LMCController` 연결을 축별 점검 | generated table, simulated axis position/status, axis-order readback | 1..9 매핑과 `0x2028`/`0x202E` 값 일치 |
| F. diagnostics qualification | Group route 변경과 독립된 runner backlog 수행 | Bulk/Recorder와 read-only D5 abort/recovery code/build 완료. Axis1 exact allowlist SDO Write는 source/PC/IDE active. CREVIS 464-byte read owner와 `0x7E13/0x7E22` route는 source/static/IDE build 완료, bits 15/16 OFF. D5 quarantine, two-ticket Read recovery proof, unresolved mutation gate와 deadline-aware cleanup 포함; PLC live/pcap, CREVIS raw read, SDO Read fault matrix와 Axis1 same-value Write/readback 수행 대기 | happy path가 아닌 fault/soak, read-owner와 Write 복구 원시 결과까지 보존 |

Phase 4 source 구현을 baseline보다 먼저 진행했으므로 B는 `65f8000`과 routed revision을
각각 cold download해 같은 조건으로 비교한다. 현재 Phase 5 `Phase5TransportClean`
SourceOnly/full static, SDO Write declaration, Compiler/Linker와 implementation smoke는 PASS했고
current PLC download/runtime은 대기 상태다. A/C/F의 PLC 실행은 서로 병렬
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

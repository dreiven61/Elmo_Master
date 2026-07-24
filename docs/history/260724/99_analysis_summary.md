# Elmo Master History 260724 Analysis and Continuation

- 원본: `docs/history/Elmo_Master_history_260724.md`
- 분할 인덱스: [index.md](index.md)
- 무결성 기록: [split_manifest.json](split_manifest.json)
- 구간별 분석:
  [parts 001-065](01_chunk_digest_parts_001_065.md),
  [parts 066-130](02_chunk_digest_parts_066_130.md),
  [parts 131-194](03_chunk_digest_parts_131_194.md)
- 분석 및 현재 저장소 재검증: 2026-07-24
- 분석 범위: 194/194개 조각 전체
- 원칙: 아래의 `히스토리상 경과`와 `현재 저장소 재검증`을 구분한다.

## 1. 바로 이어서 작업할 지점

현재 초점은 `TCPMotionInterface`의 긴 command 처리부를 no-task
`LMCControlCommandService`로 원자적으로 이관하는 성능 우선 OOP 리팩터링이다.

히스토리는 **Phase 3A dormant Group-domain service body 완료**에서 끝난다. Group
11개와 Group 상태를 공유하는 Admin `0x7D20`, `0x7D22`, helper 2개는 신규 service에
구현했지만, service의 public `HandleRequest`는 계속 fail-closed이고 실제 TCP 경로는
기존 `TCPMotionInterface` handler를 사용한다. 따라서 이 상태는 production route 전환
완료가 아니며 PLC 동작도 신규 service body의 영향을 받지 않는다.

분석 도중 2026-07-24 11:28 이후 LASAL network 저장·Rebuild·Download가 추가로
진행됐다. 따라서 현재 checkout은 히스토리 종료점보다 network checkpoint 한 단계
앞서 있으며, 아래 current-state 검증을 최종 기준으로 사용한다.

| 항목 | 2026-07-24 현재 확인 결과 |
|---|---|
| service class | task 없음, `ClassSvr`, `LMCAxis1..9`, `LMCRobot` 선언 존재 |
| service public route | `HandleRequest`가 `ResponseSize := -1`; Registry/Axis도 fail-closed |
| dormant body | Group 11개 + Admin 2개와 helper 2개 존재 |
| 실제 TCP route | `ControlCommands.HandleRequest` 호출 0개; 기존 Admin/Registry/Axis/Group handler 호출 유지 |
| `Comm_Network` | task 없는 `LMCControlCommandService1` 객체 1개, 정확한 관련 연결 11개 |
| generated table | 현재 network에서 `ONE_Comm_Network_Table.st` 재생성됨 |
| SourceOnly | `Phase3GroupDormant` PASS |
| full network | `Phase3GroupDormant` PASS |
| LASAL/PLC | Rebuild·Link 성공, PLC Download Ok, project load 확인 |
| PC/WPF | Debug/Release PC 각 148/148 PASS, WPF Debug/Release build PASS |

즉시 network/IDE blocker는 해소됐다. 11:42에 `TCPMotionInterface`의
`ControlCommands`와 `LMCAxis3` implementation search가 성공했고, 전체 log의
`CInvalidArgException`은 0건이며 LASAL은 정상 종료됐다. 다음 작업 순서는 다음과 같다.

1. route 전 legacy PLC dispatch/jitter/throughput baseline을 같은 시험 조건으로 보존한다.
2. 기존 `TCPMotionInterface1`의 axis/robot 연결은 유지한 채 13개 ID를
   `ControlCommands.HandleRequest` 한 번으로 넘기는 Phase 3B 원자 전환을 수행한다.
3. Phase 3B SourceOnly/full-network/PC/WPF 회귀 뒤 LASAL Rebuild와 PLC packet 회귀를
   반복한다.

실패한 이전 Rebuild가 삭제했던 `ONE_Comm_Network_Table.st`는 과거 Git 파일을 복원하지
않고 이번 성공 Rebuild로 재생성됐다. 이 원칙은 충족됐다.

## 2. 현재 저장소 재검증

### Git과 작업 트리

- branch: `main`
- 분석 시작 시점의 `HEAD`는 `b6c3511` (`docs(api): record live results and qualification plan`),
  로컬 `origin/main` ref는 `4fd7db2`였고 로컬 비교상 3 commits 앞서 있었다.
- 종류별 정리 후 LASAL checkpoint, verifier, 설계/테스트 계획, 실험 근거, history handoff의
  5 commits를 추가했다. 따라서 이 문서가 포함된 정리 완료 시점에는 로컬 `origin/main`
  ref보다 8 commits 앞선다. 이 분석에서는 fetch하지 않았으므로 원격 서버의 최신 상태를
  확인했다는 뜻은 아니다.
- 작업 트리는 분석 시작 전부터 크게 dirty했다. LASAL source/metadata/network, 정적
  verifier와 설계 문서 변경, service/TestClass 디렉터리, packet 분석 TXT가 포함됐다.
- 분석 문서 작성 중 11:28 이후 사용자의 LASAL 작업으로 `Comm_Network.lcn`,
  `ONE_Comm_Network_Table.st`, service generated source 등이 다시 변경됐다. 이 변경은
  분할 작업이 만든 것이 아니며 현재 상태를 재검증해 아래에 반영했다.
- 11:35에는 별도 `Motion_Network.lcn` 연결 편집과 저장도 진행됐다. 이 사용자 작업은
  해석하거나 수정하지 않았고, 해당 저장 뒤 full Dormant 계약만 다시 실행해 PASS를
  확인했다. 이후 11:42 implementation search와 정상 종료까지 확인했다.
- 정리 완료 후 추적 파일과 staging area는 clean이다. `LMCControlCommandService.st`와 OOP
  설계 문서는 목적별 commit에 포함됐다.
- 미추적 상태로 남긴 항목은 OOP 설계 범위 밖의 기존 사용자 작업 `TestClass`와 0-byte
  실험 파일 `04b_Group_Absolute_DynamicTimeout_20A4.txt`뿐이다. 삭제하거나 commit에
  흡수하지 않았다.
- 분할 작업은 원본을 수정하지 않고 `docs/history/260724/`만 새로 만들었다.

### live source 경계

현재 [LMCControlCommandService.st](../../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st)는
`RealtimeTask`, `CyclicTask`, `BackgroundTask`가 모두 `false`이고 추가 queue/task를
소유하지 않는다. ASCII 위반과 `SendData`, socket, request queue 등 transport 의존성도
0개로 재확인했다.

service의 `HandleRequest`, `HandleRegistryCommands`, `HandleAxisCommands`는 fail-closed다.
`HandleGroupCommands`에는 다음 11개 ID가 있고 `HandleAdminCommands`에는 Group-domain
Admin 2개가 있다.

- Group: `0x20D2`, `0x2047`, `0x2048`, `0x2049`, `0x204A`, `0x204B`, `0x2085`,
  `0x20A4`, `0x2045`, `0x2051`, `0x20E7`
- Admin: `0x7D20`, `0x7D22`
- helper: `MoveLinearAbsEx`, `GroupReadStatus`

[TCPMotionInterface.st](../../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st)는
required `ControlCommands` client 선언은 갖지만 service `HandleRequest`를 호출하지 않는다.
Admin, Registry, Axis, Group의 기존 handler 호출은 각각 남아 있다. 이 단일 소유 상태를
Phase 3B 원자 전환 전에 변경하지 않는 것이 Phase 3A의 안전 경계다.

[Comm_Network.lcn](../../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn)을
XML로 검사했을 때 task attribute 없는 service object가 1개이고 관련 연결은 정확히
11개다. 재생성된 `ONE_Comm_Network_Table.st`에도 object metadata, TCP incoming 1개,
axis/robot outgoing 10개가 반영됐다.

### 이 turn에서 재실행한 검증

| 검증 | 결과 | 의미 |
|---|---|---|
| PowerShell verifier parser | PASS | verifier 구문 정상 |
| LASAL SourceOnly, `Phase3GroupDormant` | PASS | 현재 `.st` 선언/구현과 dormant 의미 계약 통과 |
| full-network contract | PASS | service object, 11개 연결, generated metadata/table 포함 |
| PC tests Debug | 148/148 PASS | C# request/parser/fake-RPC 계약 |
| PC tests Release | 148/148 PASS | 동일 계약의 Release 회귀 |
| `LasalApiWpfTestApp` Debug/Release build | PASS | PC 예제 앱 빌드 |
| `git diff --check` / cached | PASS | 추적된 unstaged/staged diff 기준; line-ending 경고는 남음 |
| 미추적 핵심 파일 whitespace | PASS | service source, OOP 설계 문서, 생성 history 문서를 별도 검사 |
| LASAL IDE Rebuild/Link | PASS | recent log에 compiler/linker Done, Last command succeeded; C78/C81 warning은 남음 |
| PLC link/download | PASS | `Linking at the PLC successful`, `Download Ok`, project load 확인 |
| post-Rebuild implementation smoke | PASS | `ControlCommands`/`LMCAxis3` search 성공, log exception 0, IDE 정상 종료 |
| PLC service packet/performance | 미실시 | dormant route이므로 신규 service runtime 승인 증거 없음 |

Rebuild/Download 성공도 신규 service wire/runtime 증거를 뜻하지 않는다. 실제 route는
계속 legacy이고 Phase 3B 이후 packet/performance 회귀가 필요하다.

## 3. 히스토리상 경과

아래는 194개 조각을 모두 읽어 정리한 시간순 경과다. 중간 상태는 뒤 단계에서
대체됐을 수 있으며 현재 사실은 2절을 우선한다.

| Part | 히스토리상 단계 |
|---:|---|
| 001-016 | EtherCAT Health binding, Recorder terminal Stop race, 중복 RT latch 등 P0 diagnostics 보정 |
| 017-018 | PMAS native capture와 LASAL custom wire 분리, baseline 6 commits, D5 parser fail-closed 준비 |
| 018-046 | derived `LMCSdoExecutor` 설계·IDE 생성·network 연결, first-slice와 general read 보정 |
| 046-051 | 65-row API 요구 분석, read-only Admin/Axis/Group와 `0x7D22`, live capture 판정 |
| 051-062 | Group/Bulk/Recorder Runtime Qualification runner 구현·안전 review·visual smoke, 3 commits |
| 062-074 | LASAL `Find in Implementation` 성공 오판 인정, 결과 scroll 문제와 일반 Find 우회 확인 |
| 074-104 | Static Router + No-task Domain Service 설계, 같은 TCP class 안의 family handler 1차 분리 |
| 104-119 | `LMCControlCommandService` no-task class와 `LMCAxis1..9`/`LMCRobot` client 선언 |
| 119-130 | service method/TCP client 선언 시도 실패와 verifier 보강, 새 LASAL 실행으로 재개 |
| 131-171 | service public/private ABI와 TCP `ControlCommands` client를 IDE에 저장, Phase 2 fail-closed skeleton 마감 |
| 172-193 | Group state/helper 선언, Rebuild의 미연결 `ControlCommands` root cause 확인, IDE 종료 |
| 194 | Phase 3A dormant 13-ID body, malformed-frame pointer fix, 의미·변이 verifier 강화 |

세부 part별 주제와 source line은 다음 digest가 기준이다.

- [parts 001-065 digest](01_chunk_digest_parts_001_065.md)
- [parts 066-130 digest](02_chunk_digest_parts_066_130.md)
- [parts 131-194 digest](03_chunk_digest_parts_131_194.md)

## 4. 유지할 결정과 정정

- PMAS/MMCLib native capture는 Maestro native 동작 증거이지 LASAL custom `0x7Exx`
  wire 증거가 아니다. 두 경로를 섞어 완료 판단하지 않는다.
- LASAL 선언, class/object/channel, Network 변경은 LASAL IDE에서 하고, 기존 class의
  implementation은 IDE 종료 후 외부 `.st`에서 수정한다. IDE 저장 뒤 source를 다시
  확인해 overwrite를 탐지한다.
- OOP 구조는 transport/session/FIFO와 최종 단일 `SendData`를
  `TCPMotionInterface`에 두고, no-task service가 caller buffer를 직접 처리한다.
- 새 task, queue, mailbox, 동적 할당, 추가 full-frame copy를 만들지 않는다.
- Group 11개와 상태를 공유하는 Admin 2개는 같은 checkpoint에서 이동한다. 일부 ID만
  먼저 route하면 legacy/service 이중 소유 또는 state owner 분리가 생긴다.
- `Find in Implementation`의 `Last command succeeded`는 결과가 생성됐다는 증거가
  아니다. 당시 실제 문제는 결과 목록 scroll 위치와 행 이동이었고, 일반 `Ctrl+F`의
  `Show in Find Results`가 구현 행 이동 우회로였다.
- D5 first-slice의 4축 `0x1000:0 UInt32/4` 성공은 arbitrary SDO read/write 완료 증거가
  아니었다. 이후 general 1/2/4-byte read는 구현됐지만 Write는 별도 범위다.
- Runtime Qualification runner의 build/visual smoke와 실제 PLC qualification 결과를
  구분한다. runner로 새 soak/fault packet matrix를 수행했다는 증거는 없다.

## 5. 남은 작업과 승인 경계

### 해소된 network gate

- task 없는 service object 1개와 11개 network 연결 저장
- 성공 Rebuild로 generated table 재생성
- full-network `Phase3GroupDormant` PASS
- PLC Link/Download와 project load

### 해소된 IDE 경계

- `TCPMotionInterface`의 `ControlCommands`/`LMCAxis3` implementation search 성공
- 전체 `%TEMP%\Lasal2.log`의 `CInvalidArgException` 0건
- Motion Network 저장 뒤 full Dormant 재검증, LASAL 정상 종료
- post-save service dormant body와 Git 상태 재확인

### 이후

1. Phase 3B 전 현재 legacy route의 성능 baseline을 같은 PLC/build 조건으로 저장한다.
2. Phase 3B에서 Group 11개 + Admin 2개를 service로 원자 route 전환한다.
3. Phase 4에서 Axis/Registry와 남은 Admin domain을 이동한다.
4. Phase 5에서 TCP transport를 정리하고 최종 axis/robot direct client 제거 여부를 검증한다.
5. LASAL Rebuild/Link와 implementation smoke를 반복한다.
6. PLC download 후 기존 packet golden, malformed/error, disconnect/reconnect, 안전 matrix를
   회귀한다.
7. 1 ms task jitter/overrun, latency p99, RAM, soak 결과를 측정한 뒤에만 performance와
   production 승인을 판단한다.

이전 diagnostics/qualification 흐름의 미완료 항목도 남아 있다. 특히 새 Group/Bulk/
Recorder runner의 실물 PLC 실행, SDO abort/offline/timeout/cancel/contention matrix,
Recorder reconnect/adopt, one-slave-offline, jitter/RAM 측정은 history상 미완료다.
다만 현재 OOP Phase 3B blocker를 건너뛰고 이 항목과 섞어 수정하지 않는다.

## 6. 분할 무결성

- 원본: 51,020,396 bytes, 48,495 CRLF lines, final CRLF 있음, UTF-8 no BOM
- 원본 SHA-256:
  `3a054e1717b3d4c5388abc99583d1ce8b7947d842fc05b12e83488976fcb07c8`
- 분할 전후 원본 SHA-256: 동일
- 250 source lines 기준 194 chunks
- 읽기용 chunk 크기: 4,848-84,068 bytes, 합계 2,918,239 bytes
- 100,000자 초과 image/tool-state payload 47행은 읽기용 사본에서만 hash placeholder로 치환
- non-payload 후행 space/tab 116행은 읽기용 사본에서 정규화
- 4-100 KB의 중간 image/base64 행 8개는 남아 있지만 최대 chunk가 84,068 bytes라 읽기 가능
- readable reference/rejoin SHA-256:
  `c07b771ef5c074ffac7541a624d5dc74ef88109d02bfca4fb6dc8557107ad080`
- readable rejoin과 독립 변환 기준본: 정확히 일치
- 원본 exact byte rejoin은 위 47개 치환과 116개 정규화 때문에 의도적으로 주장하지 않는다.
  모든 예외는 [split_manifest.json](split_manifest.json)에 기록했다.

원본은 수정하지 않았고, 각 part의 원본 line 범위·hash·placeholder·정규화 내역은
manifest와 index에서 추적할 수 있다.

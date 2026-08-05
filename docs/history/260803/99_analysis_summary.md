# Elmo Master 260803 history continuation summary

작성일: 2026-08-03 (KST)

## 1. 이 문서의 용도

이 문서는 다음 두 원본 히스토리를 보존·분할 판독한 뒤 현재 작업을 재개하기 위한 기준점이다.

- `docs/history/Elmo_Master_history_260803_1.md`
- `docs/history/Elmo_Master_history_260803_2.md`

히스토리에 적힌 결론은 과거 시점의 기록이다. 아래에서 `현재 확인`으로 표시한 내용만 2026-08-03 현재 작업트리에서 다시 확인했다. PLC 다운로드와 실축 상태는 이번 작업에서 확인하지 않았다.

## 2. 분할 결과와 무결성

| 원본 | 크기 | 줄 | 청크 | 원본 SHA-256 |
|---|---:|---:|---:|---|
| `Elmo_Master_history_260803_1.md` | 118,980,470 bytes | 80,500 | 322 | `1a1c0a79085e3c4a957b488f9befe718af12224005cf1ec6f24ed921ca6c1821` |
| `Elmo_Master_history_260803_2.md` | 33,631 bytes | 531 | 3 | `885d994de33de89edbe0772ef01f4508d4a064281ffadc28a5013b6ba6a29e5e` |

- 원본은 수정하지 않았다.
- split copy는 원본 250줄 단위다.
- 첫 원본의 500개 base64 포함 행은 split copy에서만 원본 행·길이·SHA-256 placeholder로 치환했다.
- split copy의 후행 space/tab 74개 행은 정리했고 모든 변경을 manifest에 기록했다.
- 325개 청크의 개별 hash, 파일 수, line range와 정제본 재결합 hash가 모두 일치한다.
- 첫 원본은 의도적으로 payload를 치환했으므로 원본과 exact-byte rejoin을 주장하지 않는다. 둘째 원본은 변환이 없어 exact-byte rejoin도 일치한다.
- 상세값은 [split_manifest.json](split_manifest.json), 전체 링크는 [index.md](index.md)를 기준으로 한다.

## 3. 히스토리 전체 흐름

### 3.1 260730 인계와 WPF 연결 직후 종료 문제

과거 260730 히스토리를 71개 청크로 분할·분석한 뒤 현재 소스와 교차 검증했다. 당시 예제 프로그램이 Connect 직후 종료된 직접 원인은 TCP 연결 실패가 아니라 오래된 Axis Power recovery journal이었다.

- journal: 이전 PLC `BootId=6`의 Power Off ACK 미확정 기록
- 당시 PLC: 재다운로드 뒤 `BootId=11`
- 결과: Connect 뒤 BootId 불일치가 발생하고 WPF가 연결 전체를 닫음

이후 WPF는 stale recovery를 삭제하지 않고 읽기 전용 quarantine으로 연결을 유지하며, 사용자가 명시적으로 archive/retire한 뒤 재시작하는 구조로 변경됐다. 로컬 입력과 상태 조회까지 과도하게 비활성화하던 UI도 분리됐다.

### 3.2 커밋된 기준점

히스토리 중 사용자의 요청으로 기능/API, WPF recovery, 테스트, LASAL, 문서·배포 변경을 목적별로 커밋했다. 현재 `main`의 HEAD는 `6537bcf`이고 `origin/main`보다 17커밋 앞이다.

중요한 과거 커밋 결과는 다음과 같다.

- diagnostics TCP takeover와 CREVIS topology/read-owner 기반
- WPF recovery/quarantine과 qualification 흐름
- API motion/diagnostics resilience 계약
- 배포 manifest와 clean distribution artifacts
- LASAL generated control-service owner 정적 검증 개선

이후의 대규모 Home/SetPosition/Group recovery 작업은 현재 미커밋 작업트리에 남아 있다.

### 3.3 SDO Write와 CREVIS

Axis 1의 정확한 `0x2F00:24 Int32/4` SDO Write 정책과 수동 activation proof가 소스에 추가됐고 Axis 2~4는 차단됐다. CREVIS topology와 read-only input owner도 구현됐다.

이것은 소스·PC·정적 계약 상태다. 현재 PLC에 해당 build가 다운로드됐거나 Axis 1 SDO Write, CREVIS live DI/DO가 실물에서 검증됐다는 뜻은 아니다.

### 3.4 복구·qualification 확장

히스토리에는 다음 PC/API/WPF 작업이 이어진다.

- Group Stop/Disable/Enable/Power/Reset accepted-once 및 상태 재개 계약
- process restart를 견디는 recovery journal과 explicit retirement
- Single Axis live qualification runner와 no-replay 정리
- `SetAxisPosition(0x7D12)`와 `ReadAxisSetPositionOutcome(0x7D14)` dormant 계약
- Axis Reference 기반의 과거 switch-search Home `0x7D13` dormant 계약
- English/Korean UI와 callback session provenance 보강
- 재현 가능한 distribution candidate/manifest 정책

각 항목의 PC 테스트 성공을 PLC/runtime 성공으로 확대 해석하면 안 된다.

### 3.5 Home 기능 분리

Home 계열은 다음 세 기능으로 분리됐다.

| 기능 | 계약 | 현재 gate |
|---|---|---|
| 과거 switch-search Home / Axis Reference | `0x7D13`, LASAL native `MoveReference` 계열 | OFF, 폐기됨 |
| DS402 Home | `0x7D15` start + `0x7D16` exact outcome | OFF |
| TEST ONLY EnDat 2.2 ID30 오류/경고 reset | TW[20], `0x3204:0x14` | OFF |

TW[20]은 multi-turn position reset이 아니다. 실제 multi-turn position 초기화는 TW[19] `0x3204:0x13`이며 현재 미구현·금지다.

DS402 Home의 SDO 순서, ControlWord bit 4, 상태 bit 12/13, CSP 복귀 전 setpoint alignment, terminal outcome 저장 구조까지 소스에 구현됐고 LASAL F9 `0 errors / 25 warnings` 기록이 있다. 그러나 capability bit 4/6/18은 계속 OFF였고 PLC 다운로드·실축 실행은 없었다.

## 4. 가장 마지막 미완료 작업

마지막 작업 목표는 DS402 Home terminal 결과가 다음 요청에 덮어써지는 문제를 막는 `0x7D17 RetireAxisDs402HomeOutcome`이었다.

확정된 계약:

- 요청 payload 48 bytes
- exact diagnostics build/BootId/map, original RequestId, 128-bit ClientIntentId, HomingMethod, nonzero RecordGeneration
- 성공 payload 92 bytes로 기존 `0x7D16` terminal snapshot을 그대로 반환
- 첫 exact retire는 terminal record를 tombstone으로 전환
- 응답 유실 뒤 같은 key/generation 재시도는 같은 성공 snapshot 반환
- 같은 축 terminal record가 retire되지 않았으면 새 `0x7D15`는 detail 32로 거부
- WPF journal은 `0x7D16 terminal -> 0x7D17 retire -> Resolve` 순서로만 해제

히스토리 마지막 GUI 상태:

- LASAL IDE에서 private method 이름 `HandleAxisDs402HomeRetire`를 생성함
- `Reference : UINT`를 추가함
- `pRequest : ^USINT`를 설정하는 중 끝남
- 나머지 입력, 출력, 저장, 구현, route, build는 끝나지 않음

## 5. 현재 작업트리에서 다시 확인한 사실

### 5.1 저장된 LASAL 상태

현재 디스크에는 `HandleAxisDs402HomeRetire` 이름이 `.st`, `Classes.lcb`, project `.lcb` 어디에도 없다. LASAL 프로세스도 실행 중이 아니다. 따라서 히스토리의 GUI 편집은 저장된 작업으로 취급하면 안 되며 메서드 선언부터 다시 해야 한다.

현재 PLC/LASAL 소스에는 다음까지만 있다.

- `LMCDiagnosticsService`: `0x7D15`, `0x7D16` route와 두 handler
- `TCPMotionInterface`: diagnostics route 목록에 `0x7D15`, `0x7D16`
- `0x7D17` route/handler: 없음
- DS402 Home gate `FALSE`
- TW20 gate `FALSE`
- 현재 nonzero BootId 기준 diagnostics capability 계산값: `0x0000633F`

### 5.2 PC SDK 상태

현재 소스에는 다음 `0x7D17` 파일과 public API가 있다.

- `LmcAdminDs402HomeOutcomeRetirementProtocol.cs`
- `LmcAdminDs402HomeOutcomeRetirementModels.cs`
- `LmcAdminDs402HomeOutcomeRetirement.cs`
- `LmcAxisDs402HomeOutcomeRetirement.cs`
- `AdminDs402HomeOutcomeRetirementContractTests.cs`

2026-08-03 현재 다시 실행한 결과:

- SDK Release `RunPcTests`: `1077/1077 PASS`
- LASAL SourceOnly static: PASS
- LASAL full/generated network static: PASS

단, 현재 static checkpoint는 `FALSE`-gated `0x7D15/0x7D16`을 검증하고 `0x7D17` 미구현을 허용하는 기존 checkpoint다. 따라서 이 PASS는 `0x7D17` PLC 구현 완료 증거가 아니다.

### 5.3 WPF 상태

`MainWindow.MaintenanceActions.cs`는 exact `0x7D16` terminal outcome을 읽은 뒤 곧바로 local recovery journal을 Resolve한다. 새 `0x7D17` API를 호출하지 않는다. 따라서 PLC handler만 추가해서는 계약이 완성되지 않는다.

### 5.4 작업트리 규모

확인 시점 상태:

- branch: `main`, HEAD `6537bcf`
- `origin/main`보다 17커밋 앞
- tracked 변경 115개
- untracked entry 83개

기존 사용자 변경이 대규모로 남아 있으므로 broad cleanup, reset, 일괄 stage를 하면 안 된다.

## 6. 별도 실기 테스트 히스토리 판정

둘째 원본은 Test3/Test4 캡처와 다음 시험 순서를 다룬다.

확인된 과거 wire evidence:

- Axis 1 기본 상대이동: 동작 PASS, 최종 PowerOff + Standstill
- Axis 2~4: Reference 복구 뒤 각각 `+10000` 상대이동 PASS, replay 0, 최종 PowerOff + Standstill
- Axis 2~4 Stop은 이동 완료 뒤 실행돼 in-motion Stop 증거가 아님
- Axis 1은 `BootId=0x0C`, Axis 2~4는 `BootId=0x0D`이므로 동일 PLC 세대 4축 qualification은 아님
- 마지막 Home Check: 4/4 Referenced
- EtherCAT: Master/4축 OP, AL=0, invalid cycle=0
- 전 축 DS402 `0x02B3`: Fault=0, Warning=1
- PLC capability `0x00000001`, BootId `0x0D`: 당시와 현재 source 기대값에 모두 불일치

따라서 마지막 실기 판정은 `통신/기본 단일축 동작 증거 있음`, `드라이브 운전 준비 및 배포 정합성 FAIL`이다. Health RPC PASS를 Warning 해제나 group-ready PASS로 보면 안 된다.

## 7. 이 쓰레드에서 이어갈 정확한 순서

### P0. `0x7D17` 저장 가능한 LASAL 구조부터 복구

1. 작업 전 `git status`와 관련 파일 hash를 다시 기록한다.
2. canonical `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`만 연다.
3. `LMCDiagnosticsService` private method `HandleAxisDs402HomeRetire`를 IDE에서 만든다.
4. 기존 Home handler와 같은 선언 구조를 사용한다.

```text
VAR_INPUT
  Reference          : UINT
  pRequest           : ^USINT
  pResponse          : ^USINT
  ResponseCapacity   : UDINT
  CallerSessionEpoch : UDINT
  RequestSize        : UDINT
VAR_OUTPUT
  ResponseSize       : DINT
```

5. 저장 후 메서드 이름·signature가 `.st`와 `Classes.lcb`에 실제 반영됐는지 확인하고 IDE를 닫는다.

### P1. 외부 source 구현

1. `LMCDiagnosticsService.st`에 exact key/generation 검증, terminal/tombstone idempotence, 92-byte serializer와 same-axis slot reuse를 구현한다.
2. `HandleRequest`에 `0x7D17` 분기를 추가한다.
3. `TCPMotionInterface.st` diagnostics route에 `0x7D17`을 추가한다.
4. LASAL verifier가 route, handler, request offsets, response size, detail domain, tombstone 규칙을 강제하도록 갱신한다.
5. `DINT_PACKET_MAP.txt`와 C# offsets를 다시 byte 단위로 대조한다.

### P2. WPF durable journal 연결

1. `ReadExactDs402HomeOutcomeAsync`에서 terminal 결과를 얻은 뒤 `RetireDs402HomeOutcomeAsync`를 호출한다.
2. exact retirement success를 받은 뒤에만 journal을 Resolve한다.
3. timeout, response loss, mismatch, malformed response에서는 journal을 유지하고 Home을 replay하지 않는다.
4. process restart와 exact retry/tombstone 회귀 테스트를 추가한다.

### P3. 공통 축 소유권 interlock

Home/TW20 실행 중 같은 축의 일반 Motion/Power가 서버에서 거부되고, 일반 Motion/Power가 진행 중이면 Home/TW20이 거부되는 공통 PLC 소유권 계약이 아직 없다. 이 계약을 구현·검증하기 전에는 DS402 Home bit 6과 TW20 bit 18을 켜면 안 된다.

### P4. 정적·IDE 검증

- SDK Debug/Release와 WPF 관련 smoke
- LASAL SourceOnly/full static
- LASAL F9/Rebuild/Link
- 변경 메서드 `Find in Implementation`
- smoke 시작 이후 `%TEMP%\Lasal2.log`의 신규 `CInvalidArgException=0`
- `git diff --check`, 관련 문서와 packet map 동기화

PLC download와 실제 축 명령은 이 단계에 포함하지 않는다.

### P5. 별도 현장 검증

개발 P0~P4가 끝난 뒤에만 canonical 프로젝트를 Rebuild/Link/cold download하고 새 BootId와 현재 source capability를 고정한다. 그 다음 축 1~4에서 Move 없이 `0x6061`, 실제 `0x6041`, `0x603F`, AxisError를 읽어 Warning 원인을 해소한다.

다음 조건 전에는 group test, Reset, TW20, SDO Write를 실행하지 않는다.

- current source와 PLC capability/BootId/map 일치
- 전 축 Fault=0
- 전 축 Warning=0
- InternalLimit=0
- `0x603F=0`
- Reference와 안전 공간 확인

그 뒤 첫 group 순서는 `Group Power On -> Set Identity -> Enable ACK -> Locked -> Disable -> Power Off`다. Buffered A/B와 deterministic in-motion Stop은 그 다음 별도 캡처로 진행한다.

## 8. 결론

지금 바로 이어갈 개발 작업은 PLC 다운로드나 group test가 아니다. 저장되지 않은 LASAL private handler 선언을 다시 만들고 `0x7D17`의 PLC route/handler와 WPF journal retirement를 완성하는 것이 첫 작업이다. 기능 gate는 계속 OFF로 유지한다.

## 9. 2026-08-03 후속 구현 체크포인트

이 절은 위 1~8절 작성 이후 같은 날짜에 진행된 후속 구현을 현재 작업트리에서 다시
확인한 결과다. 위 절의 과거 판정과 충돌하면 이 절을 우선한다.

### 9.1 완료된 source/PC 범위

- 프로젝트 공개 명칭은 `LMC_Home`과 `LMC_HomeDS402`다. 이전 별칭,
  switch-search Home, `MoveReference()` 및 DS402 method 35 설계는 폐기됐다.
- `0x7D17` PLC route, `HandleAxisDs402HomeRetire`, retained terminal/tombstone 처리와
  WPF의 `terminal query -> exact retire -> journal resolve` 순서가 현재 소스에 있다.
- DS402 Home은 method 37, Home offset 0, 모든 motion dynamics 0으로 제한되며
  switch나 물리 이동을 요구하지 않는다.
- `LMC_Home` PC 계약은 `0x7D13/0x7D18/0x7D19`, encoder maintenance PC 계약은
  전용 `0x7E53/0x7E54/0x7E55`로 구현됐다. TW19/TW20을 generic `0x7E50` 또는
  `0x20FC`로 우회하지 않는다.
- 현재 SDK Release 전체 테스트는 `1074/1074 PASS`다.
- WPF Release build는 warning/error 0이고 maintenance/recovery focused smoke는
  `23/23 PASS`다.
- LASAL ownership activation guard self-test는 `5/5`, DS402 retirement verifier
  self-test는 `19/19` negative fixture를 모두 거부했다.

이 결과는 PC/source/static 증거다. LASAL F9, PLC download 및 실축 동작 증거가 아니다.

### 9.2 첫 hard blocker 해소 및 다음 IDE 경계

`LMCEcatInputLatch`의 IDE-generated declaration은 사용자가 수정해 저장했다.
`SubmitAxisZeroHome`은 `UDINT/DINT/DINT -> DINT`, `CopyAxisZeroHomeResult`는
`UDINT/^void/UDINT -> DINT`의 exact ABI다. 중간 저장본의 잘못된 `^pVoid`도 기존
`CopySnapshot`과 같은 `^void`로 교정됐다.

추적 `.st` implementation에는 현재 다음 정적 계약이 들어갔다.

- immutable request identity 뒤 request sequence 원자 게시와 별도 RT dispatch claim
- Standstill, AxisError 0, fresh application-position CAS 선검사
- `SetPosition(Mode:=LMCAXIS_SET_ACTPOS_APPUNIT_DEST, Position:=0)` runtime 1회
- raw drive position 불변과 application/internal actual/set/destination/master 0을 서로 다른
  RT 호출에서 3회 연속 확인
- native call count 1, native state 0, evidence `0x3F`인 terminal result 뒤 applied sequence 게시
- exact retry는 terminal result를 재사용하고 native call을 재실행하지 않음

32-DINT result의 exact slot과 RT cycle/service millisecond 분리는
`docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md`에
고정했다. 이 결과는 아직 LASAL F9 또는 PLC runtime 증거가 아니다.

다음 IDE 경계는 `LMCControlCommandService`의 legacy `ReferenceState`,
`ProcessAxisReference`, `MoveReference()` 경로를 `ZeroHomeState`, 공통 axis ownership 및
`ProcessAxisZeroHome`으로 교체하고, `TCPMotionInterface`의 metadata 전달과 cyclic call을
연결해야 한다. 현재 SourceOnly verifier는 이 obsolete switch-search 경로를 발견해 의도대로
활성화를 차단한다.

### 9.3 TW19/TW20 별도 차단 조건

LASAL `LMCDiagnosticsService`에는 아직 `AxisOwnership` client,
`EncoderMaintenanceState`, `0x7E53/0x7E54/0x7E55` handlers와 cyclic processor가 없다.
Comm Network의 ownership 연결도 선행돼야 한다. 또한 각 축의 encoder family와 feedback
socket `1..4`가 확인되지 않았으므로 TW19/TW20 gate와 capability bit 18/19는 계속 OFF다.

추가 로컬 증거로 Axis4의 2026-06-11 EAS export와 `P01.a04` 화면에서 과거
Panasonic/socket 1, `CA[59]=23`, `CA[62]=0`을 확인했다. 이는 현재 live readback이 아니고
multi-turn resolution도 0이므로 TW19/TW20 활성화 근거가 아니다. Axis1..3은 family/socket
근거가 없다. 현재 활성화 가능한 축은 0개다.

현재 authoritative 구현/IDE 순서는
`docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md`를
따른다.

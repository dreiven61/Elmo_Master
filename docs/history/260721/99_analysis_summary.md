# Elmo Master History 260721 Analysis and Continuation

- 원본: `docs/history/Elmo_Master_history_260721.md`
- 분할 인덱스: [index.md](index.md)
- 무결성 기록: [split_manifest.json](split_manifest.json)
- 분석 기준일: 2026-07-21
- 현재 저장소 재검토: 2026-07-22
- 원칙: 아래의 `히스토리상 경과`와 `현재 저장소 재검증`을 구분한다.

## 1. 바로 이어서 작업할 지점

최초 분석에서 `Motion_Network.lcn`의 시각 배치 변경을 `LMCEcatInputLatch1`의 독립
`RealTime="1 ms"` task 추가로 잘못 판정했다. HEAD와 current XML을 객체 단위로 다시
비교한 결과 두 버전 모두 이 객체에 독립 RealTime/Cyclic/Background 속성이 없다.
`_LMCAxis1.LMCPreRtWorkTrigger -> LMCEcatInputLatch1.ClassSvr` 연결만 유지되며
full-network 정적 계약도 통과한다. 따라서 실행 순서 회귀는 현재 blocker가 아니다.

현재 검증 결과는 다음과 같다.

| 검증 | 현재 결과 | 의미 |
|---|---|---|
| PC Release API test | `101/101 PASS` | C# request/parser/fake RPC 계약 통과 |
| LASAL source-only contract | PASS | `.st` source 계약 통과 |
| LASAL full-network contract | PASS | trigger 연결 유지, 독립 scheduled task 없음 |
| PMAS Version2 Debug build | PASS | 실행 중 EXE lock을 피한 임시 output으로 검증 |
| 최신 LASAL IDE Rebuild | 대기 | 16:02 통합 build 뒤 17:56 Stop 멱등 패치가 추가됨 |
| `git diff --check` | PASS | 현재 추적 diff의 공백 오류 없음 |
| PLC download/runtime | 미실시 | 실제 장비 동작 증거 없음 |

다음 순서가 현재 가장 안전하다.

1. 기능 변경, PMAS Version2, packet 분석, history를 목적별 커밋으로 분리한다.
2. 최신 `LMCRecorderStore` source로 Rebuild/Link와 implementation smoke를 반복한다.
3. D1-D4 single-bank PLC matrix와 custom `0x7Exx` capture를 수행한다.
4. 장비 검증을 병행할 수 없으면 다음 source 증분인 D5 4-byte SDO Read-only를 구현하되
   capability는 compile/static gate와 live PLC 시험 전까지 열지 않는다.

Network 편집은 저장소 규칙에 따라 LASAL IDE에서 해야 한다. 기존 class의
implementation 수정은 외부 `.st`에서 하고 매 수정마다 IDE 재동기화를 반복하지 않는다.

## 2. 현재 저장소 재검증

### Git

- branch: `main`
- `HEAD`: `29b5512 feat: complete single-bank recorder trigger workflow`
- `origin/main`: `f9bc88a docs: history update`
- 로컬 `main`은 `origin/main`보다 3개 커밋 앞서 있다.
  - `f56e269 feat: add EtherCAT diagnostics API and test application`
  - `fe64280 docs: document diagnostics internals and PLC test workflow`
  - `29b5512 feat: complete single-bank recorder trigger workflow`

현재 LASAL dirty 파일에는 기능 변경과 IDE metadata churn이 함께 있다. 객체 단위 비교
결과는 다음과 같다.

- `LMCRecorderStore.st`의 기능 변경은 terminal Ready/Uploading Stop을 identity와 owner
  검증 뒤 멱등 성공으로 처리하는 부분이다.
- class `.st`의 `Objectsize`, `.lcb`, `.lcn` binary/XML diff에는 IDE가 저장한 시각 크기,
  Position과 `DrawChnConn` 변화가 포함된다.
- HEAD와 current `Motion_Network.lcn`의 `LMCEcatInputLatch1` scheduling과 trigger 연결은
  의미상 같다. 독립 1 ms task 추가는 없다.
- 기능 commit에는 source와 필요한 정적 계약을 우선 포함하고, 의미 없는 IDE 시각 churn은
  별도 검토한다. 사용자 변경을 임의로 되돌리지는 않는다.

### 현재 source 기능 경계

`HEAD`와 현재 설계 문서를 대조한 결과는 다음과 같다.

| 단계 | 현재 source 상태 | 아직 필요한 것 |
|---|---|---|
| D0 | Capability/envelope 활성 | PLC packet/runtime 검증 |
| D1 | Health, 24-entry Catalog, PI Read 활성 source | 실제 EtherCAT runtime 검증 |
| D2 | same-cycle Bulk 활성 source | BootId/reconnect/실기 snapshot 검증 |
| D3 | single-bank manual Recorder 활성 source | RAM, jitter, chunk, adopt 실기 검증 |
| D4 | single-bank Ring, pre-trigger, edge/window/mask, 강제 trigger 활성 source | **Double bank는 비활성**, 실기 검증 |
| D5 | C#/WPF public contract 존재 | PLC PI Write/SDO ticket 실행부, allowlist, interlock |
| D6 | 미구현 | Elmo식 static/handle compatibility facade |

현재 주요 고정값은 다음과 같다.

- Catalog: 물리축 1~4, 축당 활성 PDO 6개, 총 24 signal
- `MapRevision = 0x957F101E`
- RT 목표 주기: 1 ms
- Recorder storage: 1,280,000 bytes, 최대 24채널, 단일 bank
- Recorder chunk data: 최대 1,280 bytes
- 정상 D0~D4 single-bank capability 예상값: `0x0000003F`
- stateful capability에는 nonzero retained `DiagnosticsBootId`가 필요

기존 motion/group PLC E2E와 재캡처는 여전히 `0/25`이며 diagnostics PLC 시험
matrix도 미실시다. `source-active`, build PASS, static contract PASS는 PLC 완료가 아니다.

현재 [중앙 상태 문서](../../architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)와
`LMC_Library/LMC_API_Delivery/README.md`의 PC test 표기는 `101/101`로 갱신했다.

## 3. 히스토리상 경과

아래는 215개 조각을 모두 읽어 정리한 시간순 경과다. 중간 상태는 뒤 단계에서
대체됐을 수 있으며 현재 사실은 2절을 우선한다.

| Part | 히스토리상 단계 |
|---:|---|
| 001 | 260716 인계 재분석, 전체 문서 정리, `99dcc9b` 문서 커밋 |
| 001~002 | LASAL EtherCAT Health/PI/Bulk/Recorder 가능성 검토 |
| 002 | 내부 API 구현 설명서 v2.0 Markdown/HTML 작성 |
| 003~006 | Sync/Async, TCP/UDP, timeout, EventMask, Heartbeat, Elmo API 구조 정리 |
| 006~007 | PI/Bulk/Recorder 통합 설계 확정, static facade를 D6로 연기 |
| 007 | D0 `0x7E00` PC/PLC vertical slice와 53개 테스트 |
| 008~080 | LASAL class/channel 생성, 4축 channel 정정, IDE 저장 rollback 위험 발견 |
| 081~126 | D1 metadata, C# 계약, RT latch/trigger/network 구현과 86개 테스트 |
| 127~144 | D2 Bulk와 D3 single-bank Recorder 복구·구현, 첫 Rebuild 오류 수정 |
| 145~153 | IDE implementation smoke, 89개 테스트, 당시 D0-only PLC 상태 브리핑 |
| 154~182 | API/WPF 완성, D1~D3 활성화, 100개 테스트, `f56e269`/`fe64280` 커밋 |
| 182~203 | D4 single-bank Ring/Trigger와 상태기계·parser 감사 |
| 204~214 | IDE가 외부 `.st`를 덮은 사실 발견·복구, 새 멤버 방식 폐기, 편집 원칙 확정 |
| 215 | 101개 테스트와 `29b5512` 커밋, D0~D4 single-bank 인계 |

구간별 파일 힌트는 다음 digest에 있다.

- [parts 001~072 digest](01_chunk_digest_parts_001_072.md)
- [parts 073~144 digest](02_chunk_digest_parts_073_144.md)
- [parts 145~215 digest](03_chunk_digest_parts_145_215.md)

## 4. 반드시 유지할 결정과 정정

### API와 통신

- Sync와 Async는 모두 TCP 요청/TCP 응답이다.
- 현재 Async는 blocking 통신을 `Task.Run`으로 옮긴 편의 계층이며 wire pipelining이 아니다.
- PC UDP listener와 callback endpoint 등록은 구현돼 있지만 LASAL UDP event sender는
  아직 없다. 명령 결과를 UDP로 받는 구조가 아니다.
- `eventMask`는 전달·저장되지만 실제 LASAL event filtering은 sender가 생기기 전까지
  활성 기능으로 볼 수 없다.
- instance 기반 `LMCConnection`을 코어로 유지하고 Elmo .NET식 static/handle facade는
  wire와 PLC가 안정된 뒤 D6에서만 추가한다.

### EtherCAT와 Recorder

- D1 Catalog는 physical axis 1~4의 활성 PDO만 포함한다. software axis 5~9를
  EtherCAT PDO처럼 노출하지 않는다.
- 축당 활성 PDO는 Target Position, Digital Outputs, Control Word, Actual Position,
  Digital Inputs, Status Word의 6개다.
- slave별 callback에서 즉시 latch하는 방식은 축 사이 cycle 혼합 위험으로 폐기됐다.
- `LMCEcatInputLatch1`은 독립 task가 아니라 `_LMCAxis1.LMCPreRtWorkTrigger`에서 실행한다.
- RT 경로에는 동적 메모리, 문자열, TCP, 파일 I/O, SDO, blocking lock을 넣지 않는다.
- 4 MB class 내부 array는 LASAL class 크기 제약 때문에 폐기됐고 global fixed bank를 쓴다.
- D4 전체가 완료된 것이 아니다. single-bank Ring/Trigger만 활성이고 Double bank는 꺼져 있다.
- 현재 Recorder CRC 정책은 `None`이다.
- 1.28 MB bank 실제 상한은 16채널 20,000 samples 또는 24채널 13,333 samples다.

### LASAL 편집과 배포

- IDE Save/Reload/Rebuild가 오래된 내부 상태로 외부 `.st` 구현을 덮을 수 있음이 실제로
  발생했다. IDE 상태를 source truth로 간주하면 안 된다.
- IDE는 Class 생성·선언 구조 변경과 Network 편집에만 사용한다.
- 기존 class implementation은 외부 `.st`에서 수정하고 정적 계약으로 우선 검증한다.
- 고객 배포 폴더는 개발 산출물과 자동 미러링하지 않는다. 내부 PLC 시험이 끝난 뒤
  확정된 DLL과 문서만 복사한다.
- 과거 테스트 수치 53/86/89/100은 중간 상태다. 현재 PC 기준은 101개다.

## 5. 남은 범위

### 즉시 blocker

- 최신 Recorder Stop source의 LASAL IDE Rebuild/Link와 implementation smoke
- D1-D4 single-bank PLC download/runtime 및 custom `0x7Exx` 재캡처
- 현재 IDE 생성 `.lcb/.lcn/.st` diff에서 기능 변경과 시각 metadata churn 분리

### 실제 PLC 검증

- CP313 다운로드와 startup/restart
- nonzero retained `DiagnosticsBootId`와 capability `0x3F`
- EtherCAT Health/Catalog/PI Read 실제 값
- same-cycle Bulk snapshot
- D3/D4 single-bank Recorder configure/start/trigger/status/header/chunk/release
- disconnect/adopt와 `AdoptActiveRecorder`
- RT jitter, free RAM, invalid-cycle 처리
- packet recapture와 오류/fail-closed matrix
- 기존 motion/group 25 command E2E

시험 순서는 [내부 PLC 시험 가이드](../../architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md)를 따른다.

### 후속 개발

1. D5 4-byte SDO Read-only ticket/dispatcher
2. D4 Double Buffer
3. D5 PI/SDO Write policy와 safety allowlist/interlock
4. D6 static/handle compatibility facade
5. 실기 검증이 끝난 뒤 고객 배포 산출물 갱신

## 6. 분할 무결성

- 원본 크기: 42,025,939 bytes
- 원본 줄 수: 53,548, CRLF 및 final CRLF
- 원본 SHA-256:
  `1ddbdcd8d6dd6947d79f0764879c6160d90d8e5d259888b202ca5c0ce42f9d9d`
- target 250줄, 215개 chunk
- 100,000자를 넘는 39개 computer-use image/tool-state 행은 읽기용 분할본에서만
  원본 행 번호·문자 수·SHA-256이 있는 placeholder로 치환
- non-payload source 168개 행의 후행 space/tab은 읽기용 분할본에서 정규화
- 원본 SHA-256은 분할 전후 동일
- 읽기용 chunk 재결합 SHA-256:
  `32c1b041be1d63d08e5bb731e4b4683024a6b999bebb2b893357b3fcf247e468`
- 읽기용 재결합은 독립 치환 기준본과 정확히 일치

상세 source line, chunk hash, placeholder hash와 정규화 행 번호는
[split_manifest.json](split_manifest.json)에 기록했다.

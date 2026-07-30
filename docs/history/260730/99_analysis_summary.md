# Elmo Master 260730 작업 재개 요약

## 목적과 판독 범위

- 원본 `Elmo_Master_history_260730_1.md`부터 `_5.md`까지 총 16,790행을 250행 단위 71개 물리 청크로 나눴다.
- 원본은 수정하지 않았고, 71/71개 청크를 모두 읽었다.
- 이 문서는 히스토리의 당시 주장과 2026-07-30 현재 checkout에서 다시 확인한 사실을 구분한다.
- 상세 청크 탐색은 [index.md](index.md), 원본·분할 무결성은 [split_manifest.json](split_manifest.json)을 기준으로 한다.

## 한 줄 결론

다음 작업 시작점은 PLC TCP 장애가 아니다. 먼저 현재 full-static 검증기의 control-service 객체 번호 하드코딩을 고치고, 그다음 Axis Power에서 실제 드러난 identity mismatch를 포함해 다섯 recovery owner의 post-connect 정책을 함께 설계해야 한다. `read-only recovery quarantine`은 그 설계의 후보안이지 확정 요구사항이 아니다.

## 현재 checkout에서 재확인한 사실

### Git와 변경 경계

- 브랜치: `main`
- HEAD: `6ce2cb2`
- 확인 시점의 worktree: tracked status 항목 126개. 기본 `git status --porcelain`의 축약된 untracked 항목은 120개이며, `--untracked-files=all` 기준 실제 untracked 파일은 이번 산출물 78개를 포함해 249개다.
- 이 분석 작업은 기존 dirty 변경을 수정하지 않았다. 새로 만든 것은 `docs/history/260730/` 아래 분할본, manifest, digest, index, 이 요약뿐이다.
- 기존 변경 수가 크므로 앞으로도 파일별 현재 내용을 다시 읽고, 사용자 변경과 겹치지 않는 범위에서만 수정해야 한다.

### LASAL 통신·Network

확인 사실:

- `TCPMotionInterface.st`에는 `OS_TCP_USER_GETPEERIP`, same-peer 후보 판정, 기존 socket shutdown, session/takeover 결과 격리가 들어 있다.
- `TCPMotionInterface.st`는 control request를 `ControlCommands.HandleRequest(...)`로 전달한다.
- `Comm_Network.lcn`에는 editable derived `TCPIPServer1`, `MaxConnections=2`, `TCPMotionInterface1.ControlCommands -> LMCControlCommandService1.ClassSvr`, service에서 `_LMCAxis1..9`와 `_LMCRobotBase1`로 가는 10개 연결이 있다.
- SourceOnly 정적 계약은 다음 checkpoint에서 PASS한다.
  - `Phase5TransportClean`
  - `StaticTopologyOnly`
- generated table을 포함한 full 정적 계약은 아래 오류로 FAIL한다.
  - `Phase5TransportClean generated table does not retain exactly ten control-service axis/robot connections.`

원인 판정:

- `Verify-LasalContract.ps1` 4,768-4,771행은 control-service 연결의 source object를 `TO_UDINT(2)`로 고정한다.
- 현재 `ONE_Comm_Network_Table.st`에서는 top-level object 순서가 `LMCControlCommandService1` 다음 `LMCDiagnosticsService1`이므로 service는 object 1, diagnostics는 object 2다.
- 실제 축/로봇 10개 generated connection은 모두 `TO_UDINT(1)`로 존재한다.
- 따라서 현재 full-static 실패는 generated connection 누락이 아니라 verifier의 stale object ordinal 가정으로 판단된다. generated `.st`를 수동 수정하면 안 된다.

### WPF `Connect failed`

확인 사실:

- 현재 `MainWindow.xaml.cs`는 TCP 연결, RPC 초기화, topology 자동 로드 뒤 `EnsureAxisPowerOnRecoveryConnectionIdentityAsync(...)`를 호출한다.
- 이어 Axis Stop/Reset, Motion, Group Profile Lock, Group Power recovery identity도 차례로 검사한다.
- 현재 `MainWindow.AxisPowerOnRecovery.cs`는 recovery record와 새 연결의 BootId 또는 MapRevision이 다르면 예외를 던진다.
- 그 예외가 발생하면 `MainWindow.xaml.cs`는 연결을 닫고 Connect 전체를 실패시킨다.
- 로컬 journal은 현재도 존재한다.
  - 경로: `C:\Users\dreiv\AppData\Local\Elmo\LasalMotionControlApiExample\AxisPowerOnRecoveryJournal\v1\axis-power-on-recovery.bin`
  - 크기: 127 bytes
  - 수정 시각: 2026-07-30 10:45:44 KST
  - SHA-256: `D065A33282E25A255DF03E0D73CE5B91AAA9C6CD61D7847145DE6B578610E025`
  - binary record: version 2, checksum 정상, `AcceptedAwaitingProof`, expected Power On=`false`, BootId=6, MapRevision=`0x957F101E`, endpoint=`10.10.150.1:4000`, axis=`_LMCAxis1`/reference 1

판정:

- 히스토리 5에서 본 `Connect failed`의 직접 원인은 TCP accept 실패가 아니라 WPF recovery identity mismatch 정책이다.
- journal 단순 삭제는 과거에 ACK된 Power Off 명령의 미확정 결과를 지우므로 안전한 해결이 아니다.
- 새 BootId에서 관측한 현재 상태는 현재 상태의 증거일 뿐, 이전 BootId에서 접수된 명령의 완료 증거로 소급하면 안 된다.

### PLC/runtime 증거 경계

- 히스토리에 기록된 same-IP takeover 성공은 외부 시험 프로젝트 결과다.
- 로그에서 확인된 2026-07-30 후속 download는 `_test` 또는 `_backup` 경로였고, 현재 canonical master checkout을 다시 build/download해 실행했다는 증거는 확인되지 않았다.
- 따라서 현재 master의 takeover, service route, recovery state machine은 소스·정적 계약 수준이다. PLC runtime 완료로 선언할 수 없다.

## 다섯 히스토리의 통합 결론

### History 1 — API/LASAL 구현 확장

- Phase 3B부터 Phase 5 transport-only 구조까지 진행했고, control/diagnostics routing, connection generation, safety priority, durable mutation/recovery journal, Recorder/Bulk/D5 qualification, Axis/Group accepted-once state machine이 크게 확장됐다.
- 같은 IPv4의 stale socket을 새 client가 takeover하는 로직을 editable `TCPIPServer`와 `TCPMotionInterface`에 반영했다.
- 기록상 마지막 PC 결과는 SDK Debug/Release `974/974`, WPF Release `206/206` PASS였지만 과거 snapshot 수치다. 현재 build/runtime 증거로 재사용하지 않는다.
- CREVIS configured topology는 7개 항목, 그중 CREVIS 3개 표시까지 갔지만 live node health/DI/DO와 `0x7E13/0x7E22/0x7E23`은 닫히지 않았다.
- SDO Write와 Recorder Double live gate는 계속 OFF였다.

### History 2 — Encoder Multiturn fault reset

- 일반 Fault Reset은 `QuitError()`에서 DS402 controlword `0x6040` bit 7로 이어지는 경로가 확인됐다.
- ACK는 fault clearance가 아니다. `StateWord.Fault`, `AxError`, `0x603F`, 실제 위치/encoder 상태를 별도로 확인해야 한다.
- `0x3204:20`, `0x20FC:02` 같은 후보는 drive model, firmware, encoder protocol/socket이 확정되지 않았으므로 적용값이 아니다.

### History 3 — Test/Test2 capture

- 최초 Axis Absolute Move capture는 최종 Standstill 전에 종료돼 불충분했다.
- Test2에서는 Axis move `9995 -> 50000`, non-standstill 뒤 Standstill 3회, 최종 위치 50000 readback 3회가 wire에서 확인됐다.
- Topology 7 entries/CRC `0x15867EEC`, signal catalog 24 entries/CRC `0x957F101E`, capabilities `0x613F`가 기록됐다.
- 네 축 모두 Warning이 남았다. axis 1은 `0x02B3`, axis 2-4는 `0x02D0`이었다.
- 일부 formal capture는 PASS 뒤 2초 보존 조건을 충족하지 않았다.

### History 4 — 진행도와 사용자 설명서

- `docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.*`, 계획 문서, API 사용자 설명서 v1.9를 생성·검수했다.
- 당시에도 production 판정은 NO-GO였고, source churn 때문에 숫자와 테스트 PASS를 동일 fingerprint의 최종 증거로 보지 않았다.

### History 5 — 연결 실패 진단

- LASAL server는 `Port=4000`, `_STATE_ACCEPT`, `ErrorCode=0`, interface `_STATE_RUNNING`으로 관측됐다.
- WPF는 TCP/RPC/topology까지 성공한 뒤 stale Axis Power journal의 BootId mismatch로 스스로 연결을 닫았다.
- 이 동작은 현재 source와 현재 journal에서도 다시 확인됐다.

## 이어서 진행할 우선순위

### P0-1. full-static verifier를 현재 generated object 순서에 맞춘다

1. `Verify-LasalContract.ps1`의 `TO_UDINT(2)` 하드코딩을 제거한다.
2. generated object table에서 `LMCCONTROLCOMMANDSERVICE1`의 실제 ordinal을 찾거나, 이름 기반 연결 관계로 검증한다.
3. service가 first/second object인 fixture를 모두 추가해 순서 변경 회귀를 막는다.
4. SourceOnly와 full 정적 계약을 다시 실행한다.
5. `ONE_Comm_Network_Table.st` 같은 생성 파일은 직접 고치지 않는다.

### P0-2. 다섯 recovery owner의 identity mismatch 정책을 먼저 설계한다

상태와 경계:

- 현재 Connect는 Axis Power, Axis Stop/Reset, Motion, Group Profile Lock, Group Power의 recovery identity를 연속 검사한다.
- 실제 재현된 record는 Axis Power Off이지만, Axis Power 하나만 바꾸면 다른 owner mismatch에서 다시 연결이 닫힐 수 있다.
- 이전 명령을 자동 재전송하거나 과거 ACK를 현재 BootId의 완료 증거로 간주하면 안 된다.
- journal 손상, endpoint mismatch, identity 불안정 상태는 계속 fail-closed여야 한다.
- 기존 journal 파일은 정책과 시험 기준이 확정되기 전 삭제하지 않는다.

검토할 후보안이며 아직 승인·검증된 계약이 아님:

1. 다섯 owner의 journal, identity check, mutation interlock, close 동작을 한 상태 모델로 먼저 목록화한다.
2. TCP/RPC 성공 뒤 identity mismatch가 발생했을 때 connection 전체 종료와 `read-only recovery quarantine` 중 어느 정책을 적용할지 안전 기준으로 결정한다.
3. quarantine을 채택한다면 모든 recovery owner와 write/motion 우회 경로를 막고, capability/topology, lookup/state, DS402 `0x6041`, drive error `0x603F` 등 허용 read를 명시한다.
4. old identity record의 archive/tombstone 권한과 증거 조건은 별도 안전 결정으로 정의한다. 현재 상태 관측만으로 과거 명령 완료를 소급 확정하지 않는다.
5. 다섯 owner 각각의 mismatch와 복수 journal 동시 존재를 포함한 WPF 회귀 시험을 먼저 만든다.

### P1. 현재 fingerprint로 PC 검증을 다시 고정한다

1. 현재 Git fingerprint와 dirty 범위를 기록한다.
2. SDK Debug/Release 테스트를 순차 실행한다.
3. VS Build Tools 경로로 WPF Release build/smoke를 실행한다.
4. 과거 `974/974`, `206/206`과 섞지 않고 새 결과를 별도 기록한다.

### P2. canonical LASAL master를 검증한다

1. LASAL Save/Generate 후 Rebuild/Link한다.
2. 변경 class의 `Find in Implementation` smoke를 수행한다.
3. smoke 시작 이후 `%TEMP%\Lasal2.log`의 새 `CInvalidArgException`을 확인한다.
4. canonical master 경로의 download임을 로그로 고정한다.

### P3. 실기 시험은 안전한 read-only부터 진행한다

1. 네 축 Warning 원인을 `0x6041`, `0x603F`, `AxError`, drive/encoder 정보로 확인한다.
2. same-peer abnormal close → reconnect → first RPC, 다른 IP 거절, peer lookup failure, 반복 reconnect/soak를 시험한다.
3. Bulk, Recorder, D5 timeout/cancel/recovery를 각각 별도 pcap/QTEST로 수행한다.
4. PASS/cleanup 뒤 capture를 최소 2초 유지한다.
5. motion 회귀는 마지막에 수행한다.

## 아직 하지 않은 것

- verifier 또는 WPF recovery 정책 코드를 수정하지 않았다.
- journal을 삭제·변경하지 않았다.
- LASAL IDE build, canonical PLC download, 실축 명령을 실행하지 않았다.
- 기존 staged/unstaged 사용자 변경을 정리하거나 커밋하지 않았다.
- SDO Write, PI/DO Write, dynamic CREVIS I/O gate를 열지 않았다.

## 분할·문서 검증 결과

- 5개 원본의 현재 SHA-256이 manifest 기록과 모두 일치한다.
- source별 chunk를 순서대로 byte 재결합한 결과가 5/5개 모두 원본과 정확히 같다.
- physical chunk, index의 고유 chunk link, 전체 판독 수가 각각 71개로 일치한다.
- index의 내부 링크 78개는 모두 존재한다.
- index, 이 요약, 4개 digest의 trailing whitespace는 0건이다.
- `git diff --check`는 PASS했다.
- `git diff --cached --check`는 기존 staged `docs/history/Elmo_Master_history_260721.md`의 trailing whitespace 때문에 FAIL했다. 이번에 만든 파일의 문제가 아니며 기존 stage는 수정하지 않았다.

## 분석 산출물

- [index.md](index.md): 원본, 무결성, 71개 청크의 line range와 주제 색인
- [01_chunk_digest_history_1_parts_001_017.md](01_chunk_digest_history_1_parts_001_017.md)
- [02_chunk_digest_history_1_parts_018_034.md](02_chunk_digest_history_1_parts_018_034.md)
- [03_chunk_digest_history_1_parts_035_050.md](03_chunk_digest_history_1_parts_035_050.md)
- [04_chunk_digest_histories_2_5.md](04_chunk_digest_histories_2_5.md)
- [split_manifest.json](split_manifest.json): source/chunk SHA-256와 exact-byte rejoin 기록

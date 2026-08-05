# Elmo Master 260730 작업 재개 요약

## 목적과 판독 범위

- 원본 `Elmo_Master_history_260730_1.md`부터 `_5.md`까지 총 16,790행을 250행 단위 71개 물리 청크로 나눴다.
- 원본은 수정하지 않았고, 71/71개 청크를 모두 읽었다.
- 이 문서는 히스토리의 당시 주장과 2026-07-30 현재 checkout에서 다시 확인한 사실을 구분한다.
- 상세 청크 탐색은 [index.md](index.md), 원본·분할 무결성은 [split_manifest.json](split_manifest.json)을 기준으로 한다.

## 한 줄 결론

다음 작업 시작점은 PLC TCP 장애가 아니다. 먼저 현재 full-static 검증기의 control-service 객체 번호 하드코딩을 고치고, 그다음 Axis Power에서 실제 드러난 identity mismatch를 포함해 다섯 recovery owner의 post-connect 정책을 함께 설계해야 한다. `read-only recovery quarantine`은 그 설계의 후보안이지 확정 요구사항이 아니다.

## 2026-07-30 current 후속 진행 (아래 초기 분석보다 우선)

아래 “한 줄 결론”과 P0-1/P0-2는 최초 분석 시점의 작업 지시다. 이후 current
`main@6537bcf` + working tree에서 다음 단계까지 진행됐으므로 새 시작점은 PLC 적용과
qualification이다.

- full-static verifier/generated metadata 불일치는 해소됐다. `Phase5TransportClean /
  IntegratedReadOwnerDormant` SourceOnly와 full/network static이 모두 PASS한다.
- recovery identity mismatch는 read-only quarantine, 명시적 retirement/readmission과
  no-replay 경계를 포함한 current C#/WPF 구현으로 진행됐다. 최신 source 전체 회귀도 PASS했다.
- Group Enable qualification runner는 일반 raw 경로가 아니라 durable journal arm ->
  `0x2047` exactly once -> accepted-before-first-status durable publish -> `0x2045` status-only
  stable proof -> durable resolve의 accepted-once 경로를 사용한다. focused PC 시험은 PASS했지만
  current PLC Group Enable/Lock 증거는 없다.
- Axis1 exact target `0x2F00:24 Int32/4` SDO Write는 source/PC 경로가 활성화됐다. manual
  SDO Write는 exact current connection session, `DiagnosticsBuild`, `BootId`, `MapRevision`,
  approved target이 일치하고 baseline Read, 값 불변 `preWriteGuard` Read, Write, readback의
  서로 다른 four-ticket same-value proof가 끝난 뒤에만 열린다. 로그는 `preWriteGuard`, 네
  ticket과 각 terminal `resultBytes`를 보존한다. axis 2~4는 차단 상태이며 current PLC Write
  증거는 아직 없다. manual second-click은 SDK mutation gate 안의 fresh
  `DiagnosticsBuild`/`BootId`/`MapRevision` exact 비교를 통과해야 하며 drift면 `0x7E50` 0회다.
  identity mismatch/disconnect를 관측한 proof는 A -> B -> A에서도 영구 폐기된다.
- 최신 SDK Debug/Release 각각 `976/976`, WPF Debug/Release 각각 `235/235`, LASAL
  SourceOnly/full static은 모두 PASS했다. 이는 PC/static 증거이며 PLC live 증거가 아니다.
- internal topology qualifier는 V2로 보강됐다. explicit scope/mode, exact 17-frame
  zero-network dry-run, `0x7E23` 금지, executable/SDK SHA-256, declared source fingerprint,
  BootId/build/exact `MapRevision=0x957F101E`, create-new durable report와 cleanup/result 뒤
  2초 retention 계약을 검증했다. actual PLC live report는 아직 없다.
- 최신 source의 Release 예제 EXE를 forced Rebuild한 경로는
  `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Release/LasalMotionControlApiExample.exe`,
  SHA-256은 `363E613DCE768C269A74EFFC8CB3FF253C52875568E71CDC39B32D9E5956AFD5`다.
  같은 폴더의 `LasalMotionControlLib.dll` SHA-256은
  `2C1393058188B7484A45F5CC9ECC9485F6ADE13EAC9CE78A9E4577EF96925C7D`다. EXE/DLL version은
  `0.9.1.0`, DLL product version은 `0.9.1-preview`이고 V4 marker/title 회귀도 PASS했다. 이
  hash는 current 미커밋 working-tree 개발 산출물 provenance이며 distribution/production
  provenance는 아니다.
  `LMC_API_Distribution`은 Axis1 SDO Write/retirement/single-instance 보호가 없는 이전
  gate-off manifest snapshot이므로 이번 작업에서 동기화하지 않았다. 기존 package를 건드리지
  않는 staging/atomic-finalize candidate와 SDK/LASAL/WPF/DOCX/PDF semantic policy preflight는
  P0 pending이다.
- current C# protocol ID는 62개, LASAL dispatcher route는 61개다. route는 capability-advertised
  active 53개, dormant read-owner `0x7E13/0x7E22` 2개와 reserved/dormant 6개다.
  `0x7E23`은 C# contract에는 있지만 PLC route/handler가 없다.
- 요구사항 65개는 직접 구현 16, LASAL 적응 구현 24, 부분/비활성 12, 실제 미구현 9,
  흡수/비동등 보류 4다. 완전/적응 구현은 `40/65 = 61.5%`, 부분 포함은
  `52/65 = 80.0%`이며 PLC live 통과율이 아니다.
- current LASAL source는 `0x7E11/0x7E12/0x7E13/0x7E22`를 route/구현한다.
  `LMCEcatInputLatch`는 CREVIS coupler/input/output을 포함한 coherent 464-byte snapshot을
  publish하고 해당 client는 Motion Network에 연결됐다. bits 15/16은 의도적으로 OFF이며
  current PLC download와 dynamic read live proof는 없다. `0x7E23` PLC route/handler는 없고
  bit 17도 OFF다.
- fresh LASAL reload 뒤 Ctrl+F9 Rebuild/Link는 `0 error(s), 20 warning(s)`, Linker
  `Done`으로 끝났다. `LMCSdoExecutor.st`의 build 전후 SHA-256은 동일했고, 기존
  implementation 검색과 latest 변경 implementation 직접 open smoke가 성공했다. 현재 IDE PID의
  `CInvalidArgException`은 0건이다.
- 이 결과는 source/static/IDE build와 focused PC 증거다. canonical current PLC cold download,
  BootId/MapRevision 및 source/network/unit/task provenance, 실제 Motion/Power/Group Enable/SDO
  Write 전송, CREVIS dynamic read와 final state/readback은 아직 검증하지 않았다. source-active
  기능의 runtime enablement도 완료 상태가 아니다.

따라서 다음 우선순위는 current working tree를 목적별 commit/clean checkout으로 고정하고,
기존 package를 보존하는 transactional candidate/semantic preflight P0를 병행하는 것이다.
그다음 fresh build를
current PLC에 cold download해 read-only identity, safety chain, UNIT/reference, topology와 dormant
`0x7E13/0x7E22` raw read를 먼저 확인한다. 검증 결과에 따라 bits 15/16을 활성화한 뒤 승인된
작은 범위에서 Motion/Power, Group Enable과 Axis1 SDO Write를 pcap/QTEST/PLC log로
qualification한다. 아래 본문은 최초 분석의 근거와 당시 상태를 보존한 기록으로 읽는다.

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
- 당시 CREVIS configured topology는 7개 항목, 그중 CREVIS 3개 표시까지 갔지만 live node
  health/DI/DO와 `0x7E13/0x7E22/0x7E23`은 닫히지 않았다. 위 current override에서
  `0x7E13/0x7E22` source/static/IDE 구현까지는 전진했지만 live 판정은 여전히 미완료다.
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

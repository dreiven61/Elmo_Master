# Elmo Master history 260730 index

## 빠른 시작

작업을 이어갈 때는 먼저 [99_analysis_summary.md](99_analysis_summary.md)를 읽는다. 상세 근거가 필요할 때 아래 digest에서 해당 part를 찾고, 마지막으로 원문 청크를 연다.

판독 완료 범위:

- 원본 5개, 총 16,790행
- 물리 청크 71개, 71/71개 전체 판독
- 원본 파일은 수정하지 않음
- 분할 과정의 내용 변환 없음
- 원본별 chunk byte 재결합과 SHA-256 일치

## 원본과 분할 무결성

| Source | Lines | Bytes | Chunks | Encoding / EOL | SHA-256 |
|---|---:|---:|---:|---|---|
| `../Elmo_Master_history_260730_1.md` | 12,285 | 954,912 | 50 | UTF-8 no BOM / LF | `3401abaaf01c06400d77c143adf055241b114ddbf9b95af23ce61f57c917d635` |
| `../Elmo_Master_history_260730_2.md` | 288 | 17,221 | 2 | UTF-8 no BOM / CRLF | `602cace47e5b9b91b801b63c9f18406b5d5893eef39d0c2f2ccb2efa03fa768b` |
| `../Elmo_Master_history_260730_3.md` | 767 | 45,207 | 4 | UTF-8 no BOM / LF | `c43182b684f2ca6d6f6e73de02df278e20734a19c5d65440b67d554dcef506ae` |
| `../Elmo_Master_history_260730_4.md` | 1,813 | 114,808 | 8 | UTF-8 no BOM / LF | `d47ca97d37d906bb246fc5cbdf92ac0d70757fb6d36e10299a0acf448811ca00` |
| `../Elmo_Master_history_260730_5.md` | 1,637 | 76,666 | 7 | UTF-8 no BOM / LF | `66b618014cef71efd38d587d201bdeca130049ab1c2aa941d61b24ee9590c858` |

기계 판독 가능한 상세값은 [split_manifest.json](split_manifest.json)에 있다. 모든 chunk는 원본 byte stream의 연속 구간이며, 파일명에 1-based 원본 line range가 들어 있다.

## 분석 digest

| Digest | Coverage | Status |
|---|---|---|
| [01_chunk_digest_history_1_parts_001_017.md](01_chunk_digest_history_1_parts_001_017.md) | History 1, lines 1-4,250 | 17/17 read |
| [02_chunk_digest_history_1_parts_018_034.md](02_chunk_digest_history_1_parts_018_034.md) | History 1, lines 4,251-8,500 | 17/17 read |
| [03_chunk_digest_history_1_parts_035_050.md](03_chunk_digest_history_1_parts_035_050.md) | History 1, lines 8,501-12,285 | 16/16 read |
| [04_chunk_digest_histories_2_5.md](04_chunk_digest_histories_2_5.md) | Histories 2-5, 21 chunks | 21/21 read |

## History 1 chunk catalog

| Part | Source lines | Topic hint |
|---:|---:|---|
| [001](Elmo_Master_history_260730_1_part_001_lines_00001_00250.md) | 1-250 | 이전 대형 history handoff와 Phase 3B service routing 시작 |
| [002](Elmo_Master_history_260730_1_part_002_lines_00251_00500.md) | 251-500 | Phase 4/5 진행과 격리 시험 worktree |
| [003](Elmo_Master_history_260730_1_part_003_lines_00501_00750.md) | 501-750 | Computer Use 지침; 프로젝트 변경 없음 |
| [004](Elmo_Master_history_260730_1_part_004_lines_00751_01000.md) | 751-1,000 | LASAL Class 2 탐색과 실행 준비 |
| [005](Elmo_Master_history_260730_1_part_005_lines_01001_01250.md) | 1,001-1,250 | LASAL 실행과 사용자 입력 충돌 |
| [006](Elmo_Master_history_260730_1_part_006_lines_01251_01500.md) | 1,251-1,500 | 빈 LASAL 창과 project 미오픈 상태 |
| [007](Elmo_Master_history_260730_1_part_007_lines_01501_01750.md) | 1,501-1,750 | 시험 worktree의 정적 준비와 IDE 미검증 경계 |
| [008](Elmo_Master_history_260730_1_part_008_lines_01751_02000.md) | 1,751-2,000 | LASAL GUI 자동화 중단과 원본 개발 복귀 |
| [009](Elmo_Master_history_260730_1_part_009_lines_02001_02250.md) | 2,001-2,250 | Phase 5 transport clean, 커밋, 시험 worktree 제거 |
| [010](Elmo_Master_history_260730_1_part_010_lines_02251_02500.md) | 2,251-2,500 | PC/WPF 개발 지속과 사용자 LASAL build 순서 |
| [011](Elmo_Master_history_260730_1_part_011_lines_02501_02750.md) | 2,501-2,750 | Group/Bulk와 Recorder fault/cancel 검증 |
| [012](Elmo_Master_history_260730_1_part_012_lines_02751_03000.md) | 2,751-3,000 | 장비 없이 가능한 1차 업데이트 범위 마감 |
| [013](Elmo_Master_history_260730_1_part_013_lines_03001_03250.md) | 3,001-3,250 | 1차 코드 정리·커밋과 시험 폴더 전달 |
| [014](Elmo_Master_history_260730_1_part_014_lines_03251_03500.md) | 3,251-3,500 | failure stage와 ticket 보존 계약 |
| [015](Elmo_Master_history_260730_1_part_015_lines_03501_03750.md) | 3,501-3,750 | D5 recovery와 SDO Write 종단 간 요청 |
| [016](Elmo_Master_history_260730_1_part_016_lines_03751_04000.md) | 3,751-4,000 | 안전 gate가 있는 SDO Write와 CREVIS 추가 |
| [017](Elmo_Master_history_260730_1_part_017_lines_04001_04250.md) | 4,001-4,250 | topology/I/O SDK·GUI와 live I/O 착수 |
| [018](Elmo_Master_history_260730_1_part_018_lines_04251_04500.md) | 4,251-4,500 | output shadow, uncertain result, CREVIS dynamic gap |
| [019](Elmo_Master_history_260730_1_part_019_lines_04501_04750.md) | 4,501-4,750 | durable mutation journal과 통합 RT owner 설계 |
| [020](Elmo_Master_history_260730_1_part_020_lines_04751_05000.md) | 4,751-5,000 | CREVIS snapshot/mailbox와 단계별 verifier |
| [021](Elmo_Master_history_260730_1_part_021_lines_05001_05250.md) | 5,001-5,250 | verifier 강화와 불확정 output UI 정책 |
| [022](Elmo_Master_history_260730_1_part_022_lines_05251_05500.md) | 5,251-5,500 | verifier 우회 방지와 최신 GUI 식별 |
| [023](Elmo_Master_history_260730_1_part_023_lines_05501_05750.md) | 5,501-5,750 | read-only topology qualifier와 stale SDO recovery |
| [024](Elmo_Master_history_260730_1_part_024_lines_05751_06000.md) | 5,751-6,000 | auto topology, process journal, parser stress |
| [025](Elmo_Master_history_260730_1_part_025_lines_06001_06250.md) | 6,001-6,250 | session provenance와 safety-priority admission |
| [026](Elmo_Master_history_260730_1_part_026_lines_06251_06500.md) | 6,251-6,500 | connection race, 실제 WPF smoke, parser/process 검증 |
| [027](Elmo_Master_history_260730_1_part_027_lines_06501_06750.md) | 6,501-6,750 | Recorder Double core와 D5 contention |
| [028](Elmo_Master_history_260730_1_part_028_lines_06751_07000.md) | 6,751-7,000 | D5 timeout recovery와 dormant double-bank PLC core |
| [029](Elmo_Master_history_260730_1_part_029_lines_07001_07250.md) | 7,001-7,250 | Recorder recovery inventory, journal, token |
| [030](Elmo_Master_history_260730_1_part_030_lines_07251_07500.md) | 7,251-7,500 | D4 token, SDO restart recovery, D5 queued cancel |
| [031](Elmo_Master_history_260730_1_part_031_lines_07501_07750.md) | 7,501-7,750 | Double journal, interlock, qualification adapter |
| [032](Elmo_Master_history_260730_1_part_032_lines_07751_08000.md) | 7,751-8,000 | D4 reconcile, CREVIS stale UI, D1/D5 disconnect |
| [033](Elmo_Master_history_260730_1_part_033_lines_08001_08250.md) | 8,001-8,250 | CREVIS UI/SDO draft와 D5 transport-loss adapter |
| [034](Elmo_Master_history_260730_1_part_034_lines_08251_08500.md) | 8,251-8,500 | D5 two-session recovery와 gated same-value Write |
| [035](Elmo_Master_history_260730_1_part_035_lines_08501_08750.md) | 8,501-8,750 | inline SDO read, constructor gate, WPF control 검증 |
| [036](Elmo_Master_history_260730_1_part_036_lines_08751_09000.md) | 8,751-9,000 | typed lookup, reconnect reset, Group Enable stable proof |
| [037](Elmo_Master_history_260730_1_part_037_lines_09001_09250.md) | 9,001-9,250 | Group Power accepted-once와 safety race |
| [038](Elmo_Master_history_260730_1_part_038_lines_09251_09500.md) | 9,251-9,500 | Group Profile Lock journal과 identity recovery |
| [039](Elmo_Master_history_260730_1_part_039_lines_09501_09750.md) | 9,501-9,750 | release manifest, topology evidence, Group Stop |
| [040](Elmo_Master_history_260730_1_part_040_lines_09751_10000.md) | 9,751-10,000 | CREVIS evidence, priority Group Stop, late-result 격리 |
| [041](Elmo_Master_history_260730_1_part_041_lines_10001_10250.md) | 10,001-10,250 | Recorder accepted resource와 nonmodal SDO arm/submit |
| [042](Elmo_Master_history_260730_1_part_042_lines_10251_10500.md) | 10,251-10,500 | Recorder Double config-only journal과 cleanup |
| [043](Elmo_Master_history_260730_1_part_043_lines_10501_10750.md) | 10,501-10,750 | CREVIS IDE checkpoint와 durable motion recovery |
| [044](Elmo_Master_history_260730_1_part_044_lines_10751_11000.md) | 10,751-11,000 | same-peer takeover 반영과 Axis Power journal |
| [045](Elmo_Master_history_260730_1_part_045_lines_11001_11250.md) | 11,001-11,250 | Reset, DS402, PowerOff, Stop facade |
| [046](Elmo_Master_history_260730_1_part_046_lines_11251_11500.md) | 11,251-11,500 | total deadline과 Group/Axis safety continuation |
| [047](Elmo_Master_history_260730_1_part_047_lines_11501_11750.md) | 11,501-11,750 | Axis Reset split state와 Power attribution |
| [048](Elmo_Master_history_260730_1_part_048_lines_11751_12000.md) | 11,751-12,000 | Group/Axis Power와 Enable restart durability |
| [049](Elmo_Master_history_260730_1_part_049_lines_12001_12250.md) | 12,001-12,250 | Group Disable와 Axis Stop/Reset process recovery |
| [050](Elmo_Master_history_260730_1_part_050_lines_12251_12285.md) | 12,251-12,285 | 최종 PC checkpoint와 master LASAL 잔여 proof |

## History 2 chunk catalog — Encoder Multiturn reset

| Part | Source lines | Topic hint |
|---:|---:|---|
| [001](Elmo_Master_history_260730_2_part_001_lines_00001_00250.md) | 1-250 | 일반 DS402 reset 경로와 미확정 encoder-specific SDO 후보 |
| [002](Elmo_Master_history_260730_2_part_002_lines_00251_00288.md) | 251-288 | one-shot Write 절차와 Fault/AxError/position 완료 조건 |

## History 3 chunk catalog — Capture qualification

| Part | Source lines | Topic hint |
|---:|---:|---|
| [001](Elmo_Master_history_260730_3_part_001_lines_00001_00250.md) | 1-250 | 최초 Test 분석과 조기 종료된 Axis Move capture |
| [002](Elmo_Master_history_260730_3_part_002_lines_00251_00500.md) | 251-500 | WPF build blocker 수정과 Test2 재시험 순서 |
| [003](Elmo_Master_history_260730_3_part_003_lines_00501_00750.md) | 501-750 | Test2 6세트 wire PASS, axis move proof, DS402 Warning |
| [004](Elmo_Master_history_260730_3_part_004_lines_00751_00767.md) | 751-767 | Wireshark 필터, 2초 유지, 실행 금지 gate |

## History 4 chunk catalog — Progress and API manual

| Part | Source lines | Topic hint |
|---:|---:|---|
| [001](Elmo_Master_history_260730_4_part_001_lines_00001_00250.md) | 1-250 | 진행도/계획 MD·HTML 생성 시작 |
| [002](Elmo_Master_history_260730_4_part_002_lines_00251_00500.md) | 251-500 | Browser automation reference; 새 프로젝트 결론 없음 |
| [003](Elmo_Master_history_260730_4_part_003_lines_00501_00750.md) | 501-750 | Browser automation reference; 새 프로젝트 결론 없음 |
| [004](Elmo_Master_history_260730_4_part_004_lines_00751_01000.md) | 751-1,000 | Browser automation reference; 새 프로젝트 결론 없음 |
| [005](Elmo_Master_history_260730_4_part_005_lines_01001_01250.md) | 1,001-1,250 | Browser automation reference; 새 프로젝트 결론 없음 |
| [006](Elmo_Master_history_260730_4_part_006_lines_01251_01500.md) | 1,251-1,500 | desktop/mobile render와 15px overflow 발견 |
| [007](Elmo_Master_history_260730_4_part_007_lines_01501_01750.md) | 1,501-1,750 | overflow 수정, 진행도 수치, production NO-GO |
| [008](Elmo_Master_history_260730_4_part_008_lines_01751_01813.md) | 1,751-1,813 | API 사용자 설명서 v1.9, DOCX/PDF 검수 |

## History 5 chunk catalog — Connect failure

| Part | Source lines | Topic hint |
|---:|---:|---|
| [001](Elmo_Master_history_260730_5_part_001_lines_00001_00250.md) | 1-250 | 접속 장애 분리 진단과 `_test` download 경로 확인 |
| [002](Elmo_Master_history_260730_5_part_002_lines_00251_00500.md) | 251-500 | Computer Use 지침과 실행 앱 inventory |
| [003](Elmo_Master_history_260730_5_part_003_lines_00501_00750.md) | 501-750 | 실행 앱 inventory; 새 기술 판정 없음 |
| [004](Elmo_Master_history_260730_5_part_004_lines_00751_01000.md) | 751-1,000 | LASAL server `_STATE_ACCEPT`, error 0, client 0 관측 |
| [005](Elmo_Master_history_260730_5_part_005_lines_01001_01250.md) | 1,001-1,250 | WPF 실행과 pending Axis Power Off recovery 경고 |
| [006](Elmo_Master_history_260730_5_part_006_lines_01251_01500.md) | 1,251-1,500 | Connect failed 재현과 transport/policy 분리 |
| [007](Elmo_Master_history_260730_5_part_007_lines_01501_01637.md) | 1,501-1,637 | TCP/RPC/topology 성공 후 BootId mismatch로 연결 종료 |

## 해석 규칙

- 청크와 digest의 테스트 수치·커밋·PASS는 그 시점의 기록이다.
- 현재 사실은 [99_analysis_summary.md](99_analysis_summary.md)의 `현재 checkout에서 재확인한 사실` 절을 우선한다. 뒤의 우선순위 절은 구현 전 검토할 작업 제안이다.
- ACK는 명령 완료가 아니다. stable status/readback과 identity가 함께 맞아야 한다.
- configured topology는 runtime discovery나 live I/O 상태가 아니다.
- PC/static PASS, LASAL IDE build/download, PLC/runtime, 실축 proof를 서로 대체하지 않는다.

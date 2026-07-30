# Elmo Master 260730 Histories 2-5 Chunk Digest

- 범위: `Elmo_Master_history_260730_2.md` ~ `_5.md`
- 판독 범위: 21/21개 물리 청크 전체
- 주의: 이 문서는 히스토리상 경과를 요약한다. 현재 checkout 사실은
  [99_analysis_summary.md](99_analysis_summary.md)의 재검증 결과를 우선한다.

## History 2 - Encoder Multiturn reset

| Part | Source lines | History-only digest |
|---:|---:|---|
| [001](Elmo_Master_history_260730_2_part_001_lines_00001_00250.md) | 1-250 | Gold Twitter의 Encoder Multiturn Error를 SIGMATEK에서 해제할 수 있는지 검토했다. 현재 `Reset`은 `LMCAxis.QuitError()`에서 DS402 `0x6040` bit 7까지 전달되지만, 오류 원인 제거와 실제 Fault 해제는 별도 확인이 필요하다고 정정했다. GUI/API/PLC의 D5 SDO Write 경로가 종단 간 차단돼 있음을 확인했고, 임의 Write 개방 대신 encoder-reset 전용 one-shot을 제안했다. `TW[20]` 후보인 `0x3204:20` 및 EnDat 후보 `0x20FC:02`는 엔코더·펌웨어·socket별 조건이므로 현재 적용값으로 확정하면 안 된다. LASAL-only 임시 정비안으로 별도 `EtherCAT_SDOBase` 객체를 축 slave에 연결하는 절차를 제시했다. |
| [002](Elmo_Master_history_260730_2_part_002_lines_00251_00288.md) | 251-288 | 2-byte 후보 설정과 one-shot Write 완료 확인 절차를 마무리했다. callback 뒤 `READY`, 이후 DS402 Fault Reset, `StateWord.Fault=0`, `AxError=0`, 실제 절대위치 일치를 확인하도록 했다. cyclic Force, Startup SDO, 동시 executor 사용을 금지하고 상시 기능은 제한된 전용 method로 구현하라고 했다. 실제 장비 적용이나 성공 증거는 없다. |

### History 2 continuation

- 소스 추적상 일반 DS402 Fault Reset 경로와 현재 SDO Write 차단은 확인됐었다.
- Encoder Multiturn 전용 SDO index/value는 실장비 정보 없이 적용하지 않는다.
- 다음 입력은 drive 모델·firmware, encoder 제조사/프로토콜, EAS socket,
  `MF`, `EE[1]`, `0x603F`, `0x6041`이다.

## History 3 - Capture qualification and remaining live tests

| Part | Source lines | History-only digest |
|---:|---:|---|
| [001](Elmo_Master_history_260730_3_part_001_lines_00001_00250.md) | 1-250 | 첫 Test 세트 27개 command ID, 512 request/response를 분석했다. TCP/RPC happy path는 정상이나 Axis Absolute Move는 pcap이 완료 로그보다 1.766초 먼저 끝나 최종 Standstill을 증명하지 못했다고 판정했다. Group move는 InPosition까지 확인됐지만 최종 `0x2051` readback은 없었다. Health의 DS402 `0x02B3` Warning과 예상보다 큰 이동량도 지적했다. 이후 재시험 및 신규 qualification 목록 작성을 시작했다. |
| [002](Elmo_Master_history_260730_3_part_002_lines_00251_00500.md) | 251-500 | 당시 최신 WPF가 `D5SdoTimeoutQualificationOrchestrator.cs`의 csproj Compile 누락으로 빌드 실패해 실기 시작을 중단하라고 했다. 수정 후 Topology, Axis Move, Group, Bulk, Recorder, D5 read-only 시험 순서를 제시했고 SDO/DO Write 등 gated 기능을 금지했다. Wireshark capture/display filter와 opcode 필터를 정리한 뒤 Test2 6세트 분석으로 넘어갔다. |
| [003](Elmo_Master_history_260730_3_part_003_lines_00501_00750.md) | 501-750 | Test2의 Connect, Capabilities, Health, PI Catalog, Topology, Axis Absolute Move를 wire 기준 PASS로 판정했다. 축 이동은 `9995 -> 50000`, non-standstill, Standstill 3회, final position 3회가 확인됐다. Topology는 7 entries/CRC `0x15867EEC`, Catalog는 24 entries/CRC `0x957F101E`였다. 네 축 DS402 Warning, CREVIS live health/DI/DO 미증명, post-PASS 2초 미보존을 남겼다. 당시 PC Release 669/669 및 WPF Release smoke 66/66은 PC 증거일 뿐 PLC 전체 승인이 아니라고 구분했다. 후속 요청에는 실제 PLC에서 남은 P0-P4 시험을 안전 순서로 재정리했다. |
| [004](Elmo_Master_history_260730_3_part_004_lines_00751_00767.md) | 751-767 | Wireshark 필터, 시험별 별도 pcap/QTEST 저장, PASS/cleanup 후 2초 유지 조건을 마무리했다. Recorder Double Bank, PI/SDO Write, extended SDO, selected health, DI/DO/DO Write 및 `0x7E13/22/23`은 실행 금지로 남겼다. |

### History 3 continuation

- Test2로 Axis Absolute Move 완료 증거와 정적 Topology 증거는 닫혔다.
- 다음 실기 전 소스/다운로드 fingerprint를 고정해야 한다.
- 남은 순서는 read-only drive warning 확인, Bulk, Recorder, D5 fault/recovery,
  one-slave-offline, same-peer takeover, 마지막 motion 회귀다.

## History 4 - Development status and API manual

| Part | Source lines | History-only digest |
|---:|---:|---|
| [001](Elmo_Master_history_260730_4_part_001_lines_00001_00250.md) | 1-250 | API 계획/진행도를 MD와 HTML로 만들라는 요청이다. 소스·테스트·Git을 재검토해 문서 4개를 생성했고, SDK/WPF/LASAL/실기 증거를 분리하려 했다. 이 구간 후반은 브라우저 검수 도구 문서로 프로젝트 결론이 없다. |
| [002](Elmo_Master_history_260730_4_part_002_lines_00251_00500.md) | 251-500 | 브라우저 자동화 API 문서가 이어진다. 새 Elmo 프로젝트 결정이나 검증 결과는 없다. |
| [003](Elmo_Master_history_260730_4_part_003_lines_00501_00750.md) | 501-750 | 브라우저 자동화 타입/API 문서와 세션 지침이다. 새 프로젝트 결론은 없다. |
| [004](Elmo_Master_history_260730_4_part_004_lines_00751_01000.md) | 751-1,000 | 브라우저 자동화 지침이 반복된다. 새 프로젝트 결론은 없다. |
| [005](Elmo_Master_history_260730_4_part_005_lines_01001_01250.md) | 1,001-1,250 | 브라우저 자동화 API 문서가 계속된다. 새 프로젝트 결론은 없다. |
| [006](Elmo_Master_history_260730_4_part_006_lines_01251_01500.md) | 1,251-1,500 | 진행도/계획 HTML을 데스크톱과 390px 모바일에서 렌더링했다. 진행도 화면은 overflow가 없었고 계획 화면은 15px 가로 overflow가 발견돼 원인 분석으로 넘어갔다. |
| [007](Elmo_Master_history_260730_4_part_007_lines_01501_01750.md) | 1,501-1,750 | 계획 HTML의 모바일 overflow를 수정하고 링크·HTML·콘솔을 검증했다. 진행도는 40/65 완전·적응, 50/65 부분 포함, wire/LASAL dispatcher/active 62/59/53, production NO-GO로 기록했다. 소스가 계속 바뀌어 941/941·175/175를 동일 fingerprint의 확정 PASS로 보지 않았고 full static FAIL을 남겼다. 이후 최신 API 설명서 생성 요청으로 전환해 Markdown과 생성기를 수정했다. |
| [008](Elmo_Master_history_260730_4_part_008_lines_01751_01813.md) | 1,751-1,813 | API 사용자 설명서를 v1.9, 2026-07-30, A4 35쪽으로 재생성했다. Axis/Group wait·resume/takeover, drive error, Diagnostics D0-D5, PI/Bulk, Recorder, SDO Read, Topology와 차단된 Write 기능을 구분했다. DOCX 61 headings/105 tables, PDF 전체 렌더, 생성본/배포본 hash 일치가 기록됐다. PLC/실장비 검증은 하지 않았다. |

### History 4 continuation

- `docs/status`의 진행도/계획 MD·HTML과 API 사용자 설명서 v1.9가 산출물이다.
- 당시 snapshot도 source churn 때문에 production NO-GO였고 full static 실패가 있었다.
- 숫자와 테스트 결과는 현재 checkout에서 다시 계산해야 한다.

## History 5 - Connect failure diagnosis

| Part | Source lines | History-only digest |
|---:|---:|---|
| [001](Elmo_Master_history_260730_5_part_001_lines_00001_00250.md) | 1-250 | 테스트 앱 접속 불가 원인을 C#·TCP·LASAL로 분리 진단하기 시작했다. 그날 PLC 다운로드본이 canonical repo가 아니라 `C:\work\Elmo\Elmo_Master_test\Elmo_EtherCAT_Test_4Axis`였음을 확인했다. 후반은 Computer Use 지침이다. |
| [002](Elmo_Master_history_260730_5_part_002_lines_00251_00500.md) | 251-500 | Computer Use 안전 지침과 실행 앱 목록이다. LASAL Class 2와 Visual Studio가 실행 중임을 확인했지만 새 기술 판정은 없다. |
| [003](Elmo_Master_history_260730_5_part_003_lines_00501_00750.md) | 501-750 | 실행 앱 inventory가 이어진다. 새 프로젝트 결론은 없다. |
| [004](Elmo_Master_history_260730_5_part_004_lines_00751_01000.md) | 751-1,000 | 열린 LASAL 화면을 읽어 TCP server `Port=4000`, `_STATE_ACCEPT`, `ErrorCode=0`, `ConnectedClients=0`, interface `_STATE_RUNNING`을 확인했다. 이 시점에는 PLC/LASAL server가 살아 있고 client가 붙지 않은 상태였다. |
| [005](Elmo_Master_history_260730_5_part_005_lines_01001_01250.md) | 1,001-1,250 | canonical repo의 Release WPF v0.9.1.0을 실행했다. 초기 UI에 durable Axis Power Off ACK가 proof 대기 중이라는 안전 경고가 나타났고 Connect 재현을 준비했다. |
| [006](Elmo_Master_history_260730_5_part_006_lines_01251_01500.md) | 1,251-1,500 | Connect를 실행하자 UI가 `Connect failed`가 됐고 실행 로그를 열었다. TCP와 RPC 자체 실패인지 후단 safety gate인지 구분하기 위한 증거 수집 구간이다. |
| [007](Elmo_Master_history_260730_5_part_007_lines_01501_01637.md) | 1,501-1,637 | 로그상 TCP 연결, RPC 초기화, Connected 전환, Topology 7개 조회까지 모두 성공한 뒤 앱이 recovery identity mismatch로 연결을 닫았다. journal은 `_LMCAxis1` Power Off ACK/BootId 6, 현재 PLC는 BootId 8, MapRevision은 동일했다. 진단 결론은 LASAL server 정상, 직접 원인은 stale Axis Power recovery journal이었다. journal 삭제는 proof를 지우므로 먼저 read-only로 PowerOff/Standstill을 확인한 뒤 명시적으로 해제해야 한다고 했다. 코드 수정은 하지 않았다. |

### History 5 continuation

- 가장 최근 관측 장애는 transport failure가 아니라 WPF recovery-policy failure다.
- 현 구현이 여전히 mismatch에서 전체 Connect를 실패시키는지 live source로 확인해야 한다.
- 안전한 수정 방향은 연결과 read-only 진단을 유지하고, stale journal을 명시적인
  status-only reconciliation 대상으로 노출하는 것이다. 단순 파일 삭제는 하지 않는다.

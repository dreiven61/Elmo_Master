# Parts 001-072 digest

## 사용 주의

이 문서는 `Elmo_Master_history_260721_part_001`부터 `part_072`까지를 읽어 만든 히스토리 인덱스다. 아래 내용은 대화 당시의 요청, 판단, 작업 상태를 요약한 것이며 현재 저장소, Git 원격, LASAL IDE, PLC 또는 실기 상태를 증명하지 않는다. 구현을 재개하기 전에는 관련 소스와 `git status`, LASAL 프로젝트 상태를 다시 확인해야 한다. 분할본에서 placeholder로 치환된 대형 image/tool-state payload는 원본에서 다시 읽지 않았다.

## Chronological phases

| 단계 | Part 범위 | 주제 |
|---|---:|---|
| A | 001 | 이전 `260716` 히스토리 인계, 프로젝트 문서 재분석 및 문서 커밋 기록 |
| B | 001-002 | LASAL EtherCAT Health, PI, Bulk, Recorder 구현 가능성 검토 |
| C | 002 | 기존 내부 API 개발 가이드를 구현 흐름 중심의 Markdown/HTML 설명서로 확장 |
| D | 003-006 | sync/async, TCP/UDP callback, timeout, EventMask, Heartbeat, Elmo 연결 구조 질의응답 |
| E | 006-007 | PI/Bulk/Recorder 통합 설계 확정과 static/handle facade 후속 연기 |
| F | 007 | D0 `0x7E00` diagnostics capability vertical slice 구현 및 정적 검증 기록 |
| G | 007-018 | D1 wire/RT 순서 검토와 LASAL 프로젝트의 온라인·읽기 전용 상태 파악 |
| H | 019-030 | LASAL IDE에서 `LMCEcatInputLatch`, `LMCDiagnosticsService` 생성·저장 |
| I | 030-056 | `LMCEcatInputLatch.EcatMaster` client channel 구성·저장 |
| J | 057-072 | Drive client 구성 시작, 물리축 1-4 범위 결정, `Drive1 -> Elmo_1` 적용까지 진행 |

## Part별 topical hint

| Part | 한 줄 요약 |
|---:|---|
| 001 | `260716` 인계 요약, 프로젝트 문서 정리·커밋 기록, EtherCAT PI/Bulk/Recorder 가능성 검토 시작 |
| 002 | 활성 PDO와 Recorder 제약을 정리하고 내부 API 코드 설명서 Markdown/HTML을 생성 |
| 003 | sync/async가 같은 TCP 경로임을 설명하고 WPF 예제와 UDP callback 관련 오해를 정정 |
| 004 | RPC 초기화의 TCP·UDP listener 범위, 기본 timeout, EventMask와 cancellation을 설명 |
| 005 | cancellation 처리, Elmo 원본 callback/EventMask 비교 및 이벤트 비트 표 정리 |
| 006 | CAN/EtherCAT Heartbeat 의미와 Elmo .NET static handle 구조를 확인하고 facade를 후속으로 연기 |
| 007 | PI/Bulk/Recorder 통합 설계를 완성하고 D0 `0x7E00` capability slice를 구현·정적 검증 |
| 008 | D1 IDE 등록과 RT 순서 확인을 위한 Windows app-control 절차 시작 |
| 009 | 설치된 LASAL 앱과 열린 `Elmo_EtherCAT_Test_4Axis` 프로젝트 창을 탐색·캡처 |
| 010 | LASAL Hardware Network에서 CP313, Elmo 축, 온라인 채널 상태를 조사 |
| 011 | 24-entry PDO Catalog/CRC를 확정하고 slave별 callback latch 대신 RT trigger 후보를 결정 |
| 012 | computer-use 지침과 대형 screenshot/tool-state placeholder가 중심인 구간 |
| 013 | LASAL 대상 창, 메뉴, 온라인 Hardware 상태를 다시 관찰한 로그 |
| 014 | LASAL을 편집 가능한 오프라인 상태로 전환하려는 UI 조작 |
| 015 | 기존 탭의 `[RO]` 표시와 온라인 상태를 다시 확인 |
| 016 | DEBUG 메뉴와 오프라인 여부를 확인하고 읽기 전용 상태를 점검 |
| 017 | PROJECT 메뉴와 키 입력 경로를 조사하고 IDE 생성 경로만 사용한다는 경계를 확정 |
| 018 | Alt+F6 상태 전환과 읽기 전용 대화상자 확인·닫기 |
| 019 | LASAL Class View 열기 |
| 020 | Class 추가 메뉴 진입 |
| 021 | Class root를 선택하고 `SetPDOSettings()` 기준 활성 PDO를 확인 |
| 022 | 신규 클래스 `LMCEcatInputLatch` 이름 입력·생성 확정 |
| 023 | `LMCEcatInputLatch` 생성 후 LASAL Class/Hardware UI 상태 확인 |
| 024 | Class root를 다시 선택하고 `LMCDiagnosticsService` 생성 시작 |
| 025 | `LMCDiagnosticsService` 생성 대화상자와 Hardware/Class UI 진행 |
| 026 | EtherCAT master/slave 채널과 신규 클래스 생성 화면의 accessibility continuation |
| 027 | `LMCDiagnosticsService` 이름 입력·생성 확정 |
| 028 | 두 신규 클래스가 보이는 상태에서 프로젝트 변경 저장 |
| 029 | 저장 후 Class tree와 프로젝트 속성 상태 확인 |
| 030 | 두 신규 클래스 생성·저장을 확인하고 latch 클래스 구성 열기 |
| 031 | `LMCEcatInputLatch` class configuration 메뉴 확인 |
| 032 | latch에 client channel 추가 |
| 033 | client-channel 생성 대화상자와 EtherCAT master 채널 목록 확인 |
| 034 | master client channel 이름 `EcatMaster` 입력 |
| 035 | `EcatMaster` client channel 구성 선택 |
| 036 | `EcatMaster` 자료형 속성 확인 |
| 037 | 채널 자료형 목록과 Hardware/Class UI continuation |
| 038 | `EcatMaster`의 EtherCAT master 관련 속성 선택 |
| 039 | `EcatMaster` 속성 편집 종료·상태 확인 |
| 040 | `EcatMaster` 속성값 유지 여부 재확인 |
| 041 | master channel 편집 중 LASAL 화면 accessibility continuation |
| 042 | `EcatMaster` 속성 다시 선택 |
| 043 | `EcatMaster` channel kind 설정 시도 |
| 044 | channel kind 항목 선택 |
| 045 | channel-kind 편집 결과를 포함한 LASAL UI continuation |
| 046 | `EcatMaster` channel kind 목록 열기 |
| 047 | `Command Channel` 항목으로 이동한 중간 탐색 단계 |
| 048 | `Command Channel` 선택 동작 재시도 |
| 049 | `EcatMaster` 채널 속성 편집 UI continuation |
| 050 | master channel type을 다시 편집 |
| 051 | `EcatMaster` type 목록 재개방 |
| 052 | `ECAT_Master_Base` 자료형과 최종 channel kind 선택 진행 |
| 053 | `EcatMaster` 설정 후 전체 Class/Hardware UI continuation |
| 054 | 소스와 ENI의 활성 PDO를 재확인하고 실행 중 PLC 매핑은 별도 검증 대상으로 구분 |
| 055 | `EcatMaster = ECAT_Master_Base / Object Channel`이 최종 설정임을 확인 |
| 056 | `EcatMaster` 설정 저장 전 확인, 프로젝트 저장, 저장 결과 확인 |
| 057 | 저장 후 EtherCAT slave 채널과 `LMCEcatInputLatch` class tree 확인 |
| 058 | Drive client channel 추가 메뉴 열기 |
| 059 | 첫 Drive client 생성 시작 |
| 060 | client 이름 `Drive1` 입력·입력 상태 확인 |
| 061 | `Drive1` 이름 확정·속성 확인 |
| 062 | `Drive1` channel type 선택과 type 목록 열기 |
| 063 | `Drive1` type combo 확장 |
| 064 | `Drive1` channel type 옵션 확인 |
| 065 | type 목록에서 `Command Channel`로 이동한 중간 단계에서 사용자 입력 감지 |
| 066 | `Drive1` class 속성 선택 중 추가 사용자 입력 감지 |
| 067 | Class tree에 `EcatMaster`와 `Drive1`부터 `Drive8`까지 생성된 상태 확인 |
| 068 | 동시 수동 입력을 인지하고 D1은 `Drive1-4`만 `Elmo_1-4 / Object Channel`로 두기로 결정 |
| 069 | `Drive1` class 속성 선택·현재값 확인 |
| 070 | `Drive1` 설정 행 선택·값 확인 |
| 071 | `Drive1` class combo 조작 전 Hardware/Class UI continuation |
| 072 | `Drive1` class 목록에서 `Elmo_1`로 이동·적용한 직후 구간 종료 |

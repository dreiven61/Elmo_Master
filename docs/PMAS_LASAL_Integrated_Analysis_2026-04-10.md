# PMAS/LASAL 통합 분석 노트 (내부 참조)

- 작성일: 2026-04-10
- 목적: 지금까지 수행한 PDF 분석, API 매핑, WPF 구현, 패킷 캡처 분석 결과를 한 파일로 통합해 이후 작업 기준으로 사용

## 1) 기준 자료

### 문서/PDF
- `Maestro Administrative and Motion API_2022_12_v2.012.pdf`
- `MMCLibDotNET Libs V3.0.0.7` (라이브러리/매뉴얼 번들)
- `MMCLibDotNET Test App_V3.0.0.7`
- `EtherCAT Controller(Master) 요구 사양.xlsx` (시트: `ELMO Controller API`)

### 이미 정리된 텍스트 자료
- `elmo_controller_api_ko.md`
- `elmo_controller_api_translation_tables_ko.md`
- `Codex_PMAS_WPF/API_MAPPING.md`
- `Codex_LASAL_WPF/API_MAPPING.md`

### 패킷 캡처/분석 산출물
- 폴더: `packet_capture/`
- 주요 파일:
  - `Motion_Test.pcapng`, `Motion_Tes2t.pcapng`
  - `MoveAbsoluteEx.pcapng`, `ReadActualPosition.pcapng`, `ReadStatus.pcapng`
  - `motion_test_tcp4000.tsv`, `motion_tes2t_4000.tsv`
  - `Motion_Tes2t_Summary.csv`
  - `MoveAbsoluteEx_DelayTest*_StateChanges*.csv`, `*_Command_StatusTimeline*.csv`
  - `ReadStatus_BitAnalysis.csv`

## 2) 구현 결과 요약

### PMAS 앱 (`Codex_PMAS_WPF`)
- Visual Studio 2019 WPF 기반 테스트 앱
- MMCLibDotNET 기반 연결/축/그룹/PI/Bulk/Recorder 테스트 UI
- Cycle 테스트 확장:
  - CycleTest: 이동 -> In-position(위치 허용오차 기반) -> 복귀
  - CycleTest2: In-position 대기 없이 MoveAbsolute 연속 발행
  - CycleTest3: ReadStatus(StatusWord 비트) 기반 In-position 판단
  - CycleTest4: 단발 MoveAbsoluteEx 후 ReadStatus 샘플링 캡처
- 결과 저장:
  - XLSX 저장 기능 내장 (`SimpleXlsxExporter`)
  - Position/Status 샘플 저장 상한: 300,000개

### LASAL 앱 (`Codex_LASAL_WPF`)
- PMAS UI/구조를 복제한 SIGMATEK TCP/IP 이행용 더미 버전
- 핵심 파일: `Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs`
- 동작 방향:
  - Elmo MMCLib 의존을 제거하고 소켓 프레임 송수신 골격 구현
  - 연결 버튼에서 TCP 소켓 open/close
  - 프로그램 종료 시 소켓 정리

## 3) 실험 중 주요 오류/이슈 정리

### 자주 관찰된 오류
- `ConnectRPC returned -4`
  - 의미: 핸들/연결 초기화 실패 계열 (`InvalidHandle` 성격)
- `NC_NODE_NOT_FOUND`
  - 의미: 축 이름/리소스 매핑 불일치 가능성 높음
- `NC_DIRECTION_TYPE_OUT_OF_RANGE`
  - 의미: MoveAbsoluteEx 전달 방향/모드 파라미터 불일치
- `NC_UNSUITABLE_NODE_STATE`
  - 의미: 노드 상태가 명령 수용 불가 (Power/OpMode/상태전이 미충족)

### 현장 체크 순서 권장
1. ConnectRPC 성공/핸들 유효성 확인
2. Axis name/AxisRef 매핑 일치 확인
3. PowerOn + OpMode(DS402) 상태 확인
4. Move 파라미터 범위(속도/가감속/저크/방향) 점검
5. ReadStatus/StatusWord로 상태 전이 확인

## 4) 패킷 분석 핵심 (대상 IP: `192.168.1.3`)

## 4-1) MoveAbsoluteEx / ReadActualPosition 트래픽 패턴

- 관찰 포트 조합 예: `192.168.1.13:14132 -> 192.168.1.3:4000`
- MoveAbsoluteEx 요청 특징:
  - TCP payload 길이 `64 bytes`
  - 시작 시그니처(hex) `9f20...` (LE 기준 `0x209F` 계열)
- MoveAbsoluteEx 응답 특징:
  - TCP payload 길이 `16 bytes`
  - 예: `00000800...`
- ReadActualPosition 요청 특징:
  - TCP payload 길이 `9 bytes`
  - 시작 시그니처(hex) `2e20...`
- ReadActualPosition 응답 특징:
  - TCP payload 길이 `24 bytes`
  - 예: `00001000...`

## 4-2) 타이밍 관찰 요약 (`Motion_Tes2t_Summary.csv`)

- ReadActualPosition Req->Rsp 평균: 약 `0.658 ms`
- MoveAbsoluteEx Req->Rsp 평균: 약 `0.663 ms`
- Move 명령 -> 첫 위치 변화 감지 평균: 약 `1.409 ms`
  - 최소 약 `1.306 ms`, 최대 약 `1.581 ms`

해석:
- “Move 완료 시점에 별도 Done 푸시 패킷”이 항상 발생하는 구조로 보이지 않았음
- 실제 앱에서는 `ReadActualPosition` 또는 `ReadStatus` 폴링으로 상태를 판단하는 방식이 실무적으로 타당

## 4-3) ReadStatus 비트 해석 참고 (`ReadStatus_BitAnalysis.csv`)

- 샘플 값:
  - `uiState = 0x40000080`
  - `StatusWord = 0x12B7`
- StatusWord 주요 비트:
  - bit0 Ready to switch on = 1
  - bit1 Switched on = 1
  - bit2 Operation enabled = 1
  - bit3 Fault = 0
  - bit4 Voltage enabled = 1
  - bit5 Quick stop = 1
  - bit7 Warning = 1
  - bit9 Remote = 1
  - bit10 Target reached = 0 (해당 샘플 시점)
  - bit12 Operation-mode-specific = 1
  - bit14 Manufacturer specific = 0 (의미는 벤더 정의, 일반 CiA402 표준 비트로 단정 불가)

## 5) LASAL 더미 프레임 사양 (현재 코드 기준)

기준 파일: `SigmatekTcpIpDummyMMCLib.cs`

### Command ID
- PowerOn: `0x2081`
- PowerOff: `0x2082`
- Reset: `0x2083`
- Stop: `0x2084`
- GetActualPosition: `0x00E0` (코드 내부 가정)
- MoveAbsoluteEx: `0x209F`

### 고정 프레임 포맷
- Power frame: `16 bytes`
  - `[0..1] cmd`, `[2..3] axisRef`, `[4..7] payloadLen=8`, `[8..11] on/off`, `[12..15] bufferedMode`
- Reset frame: `12 bytes`
  - `[0..1] cmd`, `[2..3] axisRef`, `[4..7] payloadLen=4`, `[8..11] execute`
- Stop frame: `16 bytes`
  - `[0..1] cmd`, `[2..3] axisRef`, `[4..7] payloadLen=8`, `[8..11] bufferedMode`, `[12..15] execute`
- GetActualPosition request: `8 bytes`
  - `[0..1] cmd`, `[2..3] axisRef`, `[4..7] payloadLen=0`
- MoveAbsoluteEx request: `64 bytes`
  - `[0..1] cmd`, `[2..3] axisRef`, `[4..7] payloadLen=56`
  - `[8..47] int64(position, vel, acc, dec, jerk)`
  - `[48..63] int32(direction, bufferedMode, execute, reserved)`

### 구현상 유의점
- `ForceZeroAxisRefForCommands = true`로 설정되어 있어, 명령 송신 시 AxisRef를 0으로 강제 가능
- 파라미터 형식은 `long(Int64)` 중심으로 구성됨 (double 직접 송신 지양 요구 반영)

## 6) 실제 Elmo 바이너리와의 일치성 주의

- PMAS 실패킷에서 ReadActualPosition 요청은 `2e20` 시그니처가 관찰됨
- LASAL 더미의 `GetActualPosition=0x00E0` 가정은 “현재 코드 편의용”이며, 실장 전 PLC/드라이브 측 프로토콜과 대조 필요
- 따라서 아래를 분리해서 관리해야 함:
  - 관측 기반 사실(PCAP/TSV/CSV)
  - 구현 편의 가정(더미 프레임 파서/커맨드 ID)

## 7) 저장 위치 요약

- 코드:
  - `Codex_PMAS_WPF/`
  - `Codex_LASAL_WPF/`
- 테스트 리포트:
  - `Codex_PMAS_WPF/Reports/`
  - `Codex_LASAL_WPF/Reports/`
- 패킷 분석 산출물:
  - `packet_capture/`

## 8) 다음 작업 권장 (후속)

1. PLC(ST/C) 파서와 실제 캡처 프레임 바이트 단위 대조
2. ReadStatus 응답 필드의 오프셋 확정(uiState/statusWord/error)
3. “명령 수락 시점 vs 실제 축 이동 시작 시점” 정의를 하나로 고정
4. LASAL 더미 프레임 ID를 캡처 실측 기반으로 재정렬

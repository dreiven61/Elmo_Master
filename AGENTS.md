# AGENTS.md

이 저장소에서 작업하는 에이전트와 사람이 따라야 할 기본 규칙이다.

## 핵심 원칙

- 돌려 말하지 않는다.
- 확인한 사실과 추정을 섞지 않는다.
- Elmo PMAS/MMCLib 기준 동작과 SIGMATEK/LASAL 이식 구현을 구분한다.
- 코드 변경 시 관련 문서와 검증 기준까지 같이 맞춘다.

## System Of Record

사실 판단 우선순위는 아래 순서다.

1. 개발 대상 소스
   - `Codex_PMAS_WPF/**`
   - `LMC_Library/LMC_API_Delivery/src/**`
   - `LMC_Library/LasalApiWpfTestApp/**`
   - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/**/*.st`
   - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/**/*.st`
   - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/**/*`
2. 프로젝트 분석 문서
   - `docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`
   - `docs/PMAS_LASAL_Integrated_Analysis_2026-04-10.md`
   - `docs/architecture/SIGMATEK_LASAL_coding_rules.md`
   - `docs/architecture/SIGMATEK_LASAL_programming_method_study.md`
   - `docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md`
   - `docs/history/**`
3. Elmo/Maestro API 레퍼런스
   - `output/pdf/maestro_api_md/**`
   - 루트의 Maestro/MMCLib PDF와 압축 해제 자료
4. 실험 자료
   - `test/packet_capture/**`
   - `test/profile_capture/**`
   - `test/Reports_PMAS/**`
   - `test/Reports_Lasal/**`

주의:

- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/*.lcp`, `*.lcb`, `*.lcn`, `*.lba`, `*.lob`, `*.ldi`는 LASAL 프로젝트/생성물 성격이 강하다. 사람이 읽는 diff 기준으로만 판단하지 않는다.
- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/ProjectInternal/`은 IDE 내부 상태다. 설계 근거로 쓰지 않는다.
- 이미 Git에 추적된 파일은 `.gitignore`로 숨겨지지 않는다.

## LASAL 변경 규칙

- LASAL 작업 전 아래 문서를 먼저 확인한다.
  - `docs/architecture/SIGMATEK_LASAL_coding_rules.md`
  - `docs/architecture/SIGMATEK_LASAL_programming_method_study.md`
  - `docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md`
- 개발 대상은 Git 추적 프로젝트 `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`다. 미추적 `_Edit` 복제본은 사용자가 명시적으로 지정하지 않는 한 수정하지 않는다.
- 새로 작성하는 LASAL custom source의 선언/구현, 식별자, 주석, 문자열과 IDE에 입력하는 class/object/channel/network 이름 및 comment는 7-bit ASCII만 사용한다. 한국어 설명은 `docs/**/*.md`에 기록하고 기존 vendor source를 일괄 재인코딩하지 않는다.
- LASAL IDE 저장/빌드 후 변경 클래스의 `Find in Implementation` smoke test를 수행하고, smoke 시작 시점 이후 `%TEMP%\Lasal2.log`에 새 `CInvalidArgException`이 없는지 확인한다.
- `TCPMotionInterface.st` 또는 TCP 프레임을 바꾸면
  `LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs`,
  `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`와 바이트 offset 단위로
  대조하고 request/parser 자동 테스트를 갱신한다.
- `Elmo_1..4`, `_LMCAxis*`, `ECAT_DS402Base`, `Network/**`를 바꾸면 EtherCAT PDO, DS402 상태, 축 연결, 네트워크 연결도를 같이 확인한다.
- LASAL CodeGenerator 헤더가 있는 `.st` 파일은 생성 영역과 `//{{LSL_IMPLEMENTATION` 사용자 구현 영역을 구분한다.

## Git 기준

- 추적 대상:
  - 사람이 수정하는 `.st`, `.h`, `.cpp`, `.c`
  - 필요한 프로젝트 등록/네트워크 파일
  - `docs/**/*.md`
- 기본 무시 대상:
  - `bin/`, `obj/`, `.vs/`
  - `Reports/`, `downloads/`, `*.pcapng`
  - `ProjectInternal/`
  - LASAL 빌드/IDE 생성물 `*.lba`, `*.lob`, `*.ldi`, `*.lhd`, `*.lcc`
- 동작 변경, 문서 변경, 실험 결과 커밋은 가능한 한 목적별로 분리한다.

## 검증 기준

- C# 변경 시 가능한 환경에서 `dotnet build`, `msbuild`, 또는 Visual Studio Build Tools로 확인한다.
- LASAL 변경은 이 환경에서 IDE 빌드/PLC 다운로드를 자동 검증할 수 없다. 대신 코드, 네트워크 파일, 문서, PC 송신 프레임을 교차 확인한다.
- 커밋 전 최소한 `git diff --check`와 `git diff --cached --check`를 통과시킨다.

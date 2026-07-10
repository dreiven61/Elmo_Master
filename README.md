# Elmo Master Workspace

Elmo PMAS/MMCLib 기준 동작을 분석하고, SIGMATEK/LASAL 및 WPF 테스트 프로그램으로 재현하기 위한 작업 저장소다.

## 주요 폴더

- `Codex_PMAS_WPF/`: Elmo MMCLibDotNET 기반 WPF 테스트 앱
- `Codex_LASAL_WPF/`: SIGMATEK TCP/IP 이식용 WPF 테스트 앱
- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/`: Git 추적 LASAL 4축 EtherCAT 테스트 프로젝트
- `docs/`: 분석, 설계, 히스토리 문서
- `packet_capture/`: 패킷 캡처와 분석 산출물
- `output/pdf/maestro_api_md/`: Maestro API 문서 추출/분석 자료

## 작업 기준 문서

- `AGENTS.md`
- `docs/PMAS_LASAL_Integrated_Analysis_2026-04-10.md`
- `docs/architecture/SIGMATEK_LASAL_coding_rules.md`
- `docs/architecture/SIGMATEK_LASAL_programming_method_study.md`
- `docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md`

LASAL 코드를 수정할 때는 코딩 규칙과 오류 예방 지침서를 먼저 확인한다.

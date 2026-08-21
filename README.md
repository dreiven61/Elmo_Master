# Elmo Master Workspace

Elmo PMAS/MMCLib 기준 동작을 분석하고 SIGMATEK/LASAL 전용 DINT API와 WPF
예제로 이식·검증하는 저장소다.

현재 개발 버전은 `LasalMotionControlLib 0.9.1-preview`이며 production 승인본이 아니다.
current API 계약과 개발 상태는 각각 `docs/api/API_MANUAL.md`와
`docs/api/API_DEVELOPMENT_PROGRESS.md`에서만 관리한다.

## 현재 구성

| 경로 | 역할 | 기준 여부 |
|---|---|---|
| `Codex_PMAS_WPF/` | Elmo MMCLibDotNET 비교·생산 cycle benchmark 앱 | PMAS 기준 |
| `Codex_LASAL_WPF/` | 실제 TCP 일부와 local simulation/no-op이 섞인 초기 hybrid 앱 | legacy 참고만 |
| `LMC_Library/LMC_API_Delivery/src/` | 현재 LASAL-DINT C# API source | canonical |
| `LMC_Library/LasalApiWpfTestApp/` | API source를 직접 참조하는 개발·실기 진단 WPF | canonical 개발 앱 |
| `LMC_Library/LMC_API_Distribution/` | DLL, 독립 예제와 사용자 매뉴얼 | 외부 전달 기준 |
| `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/` | 현재 tracked LASAL PLC source/network | canonical PLC project |
| `test/packet_capture/`, `test/profile_capture/` | packet/profile 원본과 분석 자료 | 실험 근거 |
| `test/Reports_PMAS/`, `test/Reports_Lasal/` | PMAS/LASAL 비교 시험 결과 | 실험 근거 |
| `output/pdf/maestro_api_md/` | Maestro API 문서 추출 자료 | vendor reference |
| `docs/history/` | 날짜별 작업 히스토리와 이어하기 문서 | 과거 맥락 |

폴더명은 `4Axis`지만 현재 지원 범위는 다음처럼 분리한다.

- single-axis API와 software object: 축 1~9
- physical Elmo/DS402: 축 1~4
- simulated software axis: 축 5~9
- Cartesian SetKin/Lock/Move: X/Y/Z/U 축 1~4

9축 단축 지원은 9축 동시 Cartesian group motion을 뜻하지 않는다.

## 먼저 읽을 문서

1. [API 설명서](docs/api/API_MANUAL.md)
2. [API 개발 진척도](docs/api/API_DEVELOPMENT_PROGRESS.md)
3. [현재 아키텍처 및 릴리스 상태](docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)
4. [LASAL 코딩 규칙](docs/architecture/SIGMATEK_LASAL_coding_rules.md)
5. [LASAL 프로그래밍 방법](docs/architecture/SIGMATEK_LASAL_programming_method_study.md)
6. [LASAL 오류 예방 지침](docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md)

`docs/PMAS_LASAL_Integrated_Analysis_2026-04-10.md`는 PMAS와 초기 dummy 구현을
분석한 역사적 기준선이다. 현재 LASAL-DINT 구현 상태 판단에는 사용하지 않는다.

LASAL 코드를 수정할 때는 위 코딩 규칙·방법·오류 예방 지침을 먼저 확인하고,
PC frame과 LASAL parser, network와 테스트 문서를 같은 변경에서 맞춘다.

# LMC_Library 구성 안내

이 디렉터리는 LASAL Motion Control PC API의 개발, 예제와 배포 자료를 역할별로
분리한다.

| 폴더 | 용도 | 기준 |
|---|---|---|
| `LMC_API_Delivery` | API C# source, tests, 설계 기록 | 개발 source of truth |
| `LasalApiWpfTestApp` | source ProjectReference를 사용하는 개발/실기 진단 예제 | 내부 개발용 |
| `LMC_API` | packet 근거, 상세 개발 설명, source review, package build script | 내부 개발용 |
| `LMC_API_Distribution` | API, 독립 WPF 예제, 단일 API 사용설명서 | 외부 전달 기준 |

현재 버전은 `0.9.1-preview`다. PC 테스트 100/100, LASAL source/full 계약,
LASAL IDE Rebuild/Link와 개발 WPF Debug/Release 빌드는 통과했다. 기존 motion/group
PLC E2E/packet 재캡처는 0/25이고 diagnostics D1~D3 runtime 시험도 미실시다.
production 승인본으로 표기하지 않는다.

## 개발자 시작 위치

- 프로젝트 전체 현재 상태:
  [`../docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`](../docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)
- 구조/변경/릴리스 절차:
  [`LMC_API/API_DEVELOPMENT_GUIDE.md`](LMC_API/API_DEVELOPMENT_GUIDE.md)
- 최근 source review:
  [`LMC_API/API_SOURCE_REVIEW_2026-07-15.md`](LMC_API/API_SOURCE_REVIEW_2026-07-15.md)
- API 현재 구현 상태:
  [`LMC_API_Delivery/README.md`](LMC_API_Delivery/README.md)
- EtherCAT PI/Bulk/Recorder 내부 PLC 시험 순서:
  [`../docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md`](../docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md)

## 사용자 전달 위치

- 패키지 안내:
  [`LMC_API_Distribution/README.md`](LMC_API_Distribution/README.md)
- 사용자 매뉴얼:
  [`LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf`](LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf)

`LMC_API/LMC_API`는 `0.9.0-pc-api` 구버전 보관본이며 새 배포에 사용하지 않는다.

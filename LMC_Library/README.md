# LMC_Library 구성 안내

이 디렉터리는 LASAL Motion Control PC API의 개발, 예제와 배포 자료를 역할별로
분리한다.

| 폴더 | 용도 | 기준 |
|---|---|---|
| `LMC_API_Delivery` | API C# source, tests, 설계 기록 | 개발 source of truth |
| `LasalApiWpfTestApp` | source ProjectReference를 사용하는 개발/실기 진단 예제 | 내부 개발용 |
| `LMC_API` | packet 근거, 상세 개발 설명, source review, package build script | 내부 개발용 |
| `LMC_API_Distribution` | API, 독립 WPF 예제, 단일 API 사용설명서 | 외부 전달 기준 |

현재 버전은 `0.9.1-preview`다. PC 테스트 Debug/Release 각 135/135, LASAL
SourceOnly/full static 계약과 개발 WPF Debug/Release build 및 각 3초 startup smoke는
통과했다. `Classes.lcb`의 general `TryStartRead` declaration도 current source와
동기화되어 있다. BootId 5 legacy `0x13F` PLC capture에서 `0x1000:0` UInt32 4-byte SDO Read는
물리축 1~4 모두 Completed/Success를 반환했다. 현재 `0x213F` general-inline
캡처에서는 첫 오류 뒤 Submit `ResourceBusy(9)` 고착을 재현해 executor state machine을
수정했고, 이후 1/2/4-byte runtime 정상 동작과 관련 capture 분석을 확인했다. 전체 D5
fault matrix와 최신 IDE build/download/smoke log는 남아 있다. 기존 motion/group PLC
E2E/packet 재캡처는 0/25이고 diagnostics D1~D4 runtime도 미실시다. D4 Double,
PI/SDO Write와 8/12-byte/extended SDO result는 미구현이다. Phase 1의 D1/D2 기반
PI/Bulk compatibility facade는 구현됐지만, 기존 D6 계획의 static/handle wrapper와
별도 D6 wire는 구현하지 않았다. `0x7D00/10/20` Admin은 source/static만 완료됐고
LASAL IDE build/download와 PLC 검증이 남아 있다.
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
- native PMAS capture 분석과 구현 정렬:
  [`../docs/architecture/ELMO_NATIVE_API_PACKET_CAPTURE_ANALYSIS_2026-07-21.md`](../docs/architecture/ELMO_NATIVE_API_PACKET_CAPTURE_ANALYSIS_2026-07-21.md),
  [`../docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md`](../docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md)

## 사용자 전달 위치

- 패키지 안내:
  [`LMC_API_Distribution/README.md`](LMC_API_Distribution/README.md)
- 사용자 매뉴얼:
  [`LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf`](LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf)

`LMC_API/LMC_API`는 `0.9.0-pc-api` 구버전 보관본이며 새 배포에 사용하지 않는다.

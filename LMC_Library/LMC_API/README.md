# LASAL Motion Control API 내부 개발 영역

이 폴더는 `LasalMotionControlLib`의 내부 설계, 패킷 근거, 소스 리뷰와 배포
절차를 관리한다. 최종 사용자에게 전달하는 폴더가 아니다.

## 현재 기준

- API 버전: `0.9.1-preview`
- 대상: .NET Framework 4.8
- PC API 소스: `../LMC_API_Delivery/src`
- 자동 테스트: `../LMC_API_Delivery/tests`
- 개발용 WPF 예제: `../LasalApiWpfTestApp`
- 정식 배포 폴더: `../LMC_API_Distribution`
- LASAL 어댑터: `../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- PC 자동 테스트: Debug/Release 각 135/135 PASS
- 개발 WPF: Debug/Release build와 각 3초 startup smoke PASS
- LASAL 정적 계약: SourceOnly/full static PASS; `Classes.lcb`의 general
  `TryStartRead` declaration과 current source 동기화 확인
- LASAL IDE: D0-D4와 gate-off D5 source Rebuild/Link `0 error`; gate-on fixed-source
  runtime download 확인, 대응 build/smoke log 미보존
- 기존 motion/group PLC E2E/Wireshark 재캡처: `0/25`
- diagnostics source: D1~D3, D4 single-bank Ring/Trigger와 D5 general-inline SDO Read
  활성(`CapabilityBits=0x0000213F`, MaxSDO=4); D4 Double, PI/SDO Write,
  8/12-byte/extended SDO result는 미구현
- Phase 1 source: Admin `0x7D00/10/20`, typed drive read, PI/Bulk builder/reader와
  PC-local error catalog 구현; success-capable 50개/handled 52개
- Phase 1 검증 경계: C#과 LASAL static PASS; Admin LASAL IDE build/download,
  실물 parameter 값/UNIT과 packet capture는 미검증
- diagnostics PLC 시험 matrix: legacy 축 1~4와 general-inline 1/2/4-byte SDO Read는
  사용자 실기 PASS. 최종 확인 신규 pcap/log와 D5 fault matrix/D1~D4 시험은 없음

`LMC_API/LMC_API`는 2026-07-13의 `0.9.0-pc-api` 구버전 보관본이다.
현재 DLL, 문서 또는 예제로 재사용하지 않는다. `Elmo_API_Packet2`는 패킷 분석
근거이며 외부 배포 대상이 아니다.

## 내부 문서

- [API_DEVELOPMENT_GUIDE.md](API_DEVELOPMENT_GUIDE.md): 구조, wire 계약,
  변경 규칙, 테스트와 릴리스 절차
- [API_DEVELOPMENT_GUIDE.html](API_DEVELOPMENT_GUIDE.html): 위 내부 설명서의
  standalone HTML
- [API_DEVELOPMENT_GUIDE_PRINT_STYLE.html](API_DEVELOPMENT_GUIDE_PRINT_STYLE.html):
  Pandoc HTML 재생성용 print style
- [API_SOURCE_REVIEW_2026-07-15.md](API_SOURCE_REVIEW_2026-07-15.md): 이번
  배포 준비 전 소스/문서/바이너리 리뷰 결과
- [API_USER_MANUAL_KO.md](API_USER_MANUAL_KO.md): 사용자 매뉴얼 초기 초안용 Markdown
- `Generate-ApiUserManual.py`: 초기 PDF 초안 생성기
- `Generate-ApiUserManualDocx.py`: 초기 편집용 DOCX 초안 생성기
- `Elmo_API_Packet2/PACKET_ANALYSIS.md`: 원본 packet 분석
- `../LMC_API_Delivery/docs`: 기능별 설계 및 구현 기록

## 배포 원칙

1. `LMC_API_Delivery/src`에서 Release DLL을 새로 빌드한다.
2. `LMC_API_Distribution/01_API`의 DLL만 외부 배포 기준으로 사용한다.
3. 배포 예제는 DLL을 `..\..\01_API\LasalMotionControlLib.dll` 상대경로로
   참조하고 API 소스 프로젝트를 참조하지 않는다.
4. 외부 문서는 `03_API_User_Manual`의 열람용 PDF와 편집용 DOCX를 사용한다.
   사용자가 편집한 DOCX와 그 DOCX에서 내보낸 PDF가 배포 기준이다. Markdown과
   생성 스크립트는 초기 초안 제작용이며 배포 빌드에서 문서를 덮어쓰지 않는다.
5. PC 테스트, LASAL 정적 계약, 배포 예제 Debug/Release 빌드와 SHA-256을
   모두 확인한다.
6. LASAL IDE/PLC 검증이 끝나기 전에는 `production`, `validated` 또는
   `release approved`라고 표기하지 않는다.

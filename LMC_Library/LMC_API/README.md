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
- PC 자동 테스트: current SDK Debug/Release direct 각 1133/1133 PASS
- 개발 WPF: Debug/Release Rebuild PASS, full smoke 각 339/339 PASS, supplied actual
  example EXE relaunch gate 각 1/1 PASS
- LASAL 정적 계약: historical `GateDVisualLayout` checkpoint는 PASS했지만 current
  `ad4af91` PS5.1 SourceOnly/full target은 verifier compatibility 경계를 통과한 뒤
  `Classes.lcb` sanctioned Gate D identity drift에서 exit 1 STOP
- LASAL IDE: historical Rebuild/Link와 implementation smoke 증거는 보존한다. current
  artifact는 reviewed transition 전 finalizer/Rebuild/Download를 반복하지 않으며 PLC/runtime
  qualification도 완료로 보지 않는다
- 기존 motion/group PLC E2E/Wireshark 재캡처: 대표 subset은 과거 PASS, 전체 25-command matrix 미완료
- diagnostics source: D1~D3, D4 single-bank Ring/Trigger, D5 general-inline SDO Read와
  Axis 1 exact `0x2F00:24 Int32/4` SDO Write 활성(`CapabilityBits=0x0000633F`, MaxSDO=4).
  WPF 수동 Write는 현재 connection/session/build/boot/map에서 exact same-value 4-ticket
  qualification을 통과한 뒤에만 열린다. mismatch/disconnect proof는 영구 폐기하고 실제
  second-click은 SDK mutation gate의 fresh identity exact 비교 뒤에만 전송한다. D4 Double,
  PI Write, Axis 2..4/비승인 SDO Write와
  8/12-byte/extended result는 off/미구현
- Admin source: Phase 1 `0x7D00/10/20` read와 Phase 2 `0x7D22`
  GroupMoveLinearRelative, typed drive read, PI/Bulk builder/reader와 PC-local error
  catalog 구현; capability-advertised active 53개, PLC dispatcher route 61개(active 53 +
  dormant read-owner 2 + reserved/dormant 6). `0x7E23` route는 없음
- Admin 검증 경계: C#과 LASAL static 및 current IDE Rebuild/Link PASS; current PLC download,
  실물 parameter 값/UNIT, relative motion과 packet capture는 미검증
- diagnostics PLC 시험 matrix: legacy 축 1~4와 general-inline 1/2/4-byte SDO Read는
  사용자 실기 PASS. 최종 확인 신규 pcap/log와 D5 fault matrix/D1~D4 시험은 없음

`LMC_API/LMC_API`는 2026-07-13의 `0.9.0-pc-api` 구버전 보관본이다.
현재 DLL, 문서 또는 예제로 재사용하지 않는다. `Elmo_API_Packet2`는 패킷 분석
근거이며 외부 배포 대상이 아니다.

`../LMC_API_Distribution`도 current source를 재조립한 패키지가 아니다. manifest와 내부
파일은 일관되지만 Axis1 SDO Write, stale recovery retirement와 actual-EXE reconnect gate가
없는 `1.9` gate-off snapshot이다. `Build-LmcApiDistribution.ps1`은 이 폴더를 직접 덮어쓰지
않고 별도 sibling candidate만 생성한다. Current `2.3-candidate` DOCX/PDF는 검토용 입력일
뿐이며, current Gate D STOP 때문에 full Distribution build와 candidate publish는 PASS가 아니다.
current PLC live proof와 release-scope 승인 전에는 canonical을 덮어쓰거나 정식 배포로
동기화하지 않는다.

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
- `Build-LmcApiDistribution.ps1`: canonical 무변경 transactional candidate build
- `DistributionPipeline.ps1`: staging/lock/seal/drift/success-only rename 구현
- `DistributionSemanticPolicy.ps1`: SDK/LASAL/WPF/DINT/README/DOCX/PDF 의미 preflight;
  DOCX와 PDF 각각에 `2.3-candidate`, bounded reconnect/actual-EXE PC-only 경계와 preview
  release 안전 경고를 요구한다
- `Test-LmcApiDistributionPipeline.ps1`, `Test-LmcDistributionSemanticPolicy.ps1`,
  `Test-LmcReleaseManifest.ps1`: release 경로 회귀
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
7. release build는 같은 volume의 `.LMC_API_Distribution.stage.*`에서 모든 검증을 끝낸 뒤
   존재하지 않는 `LMC_API_Distribution_candidate_*`로 한 번만 rename한다. 실패 시 staging만
   제거하고 canonical과 이미 publish된 candidate는 자동 삭제하지 않는다.
8. schema 2 manifest에는 release input tree hash와 semantic policy hash/result를 포함한다.
   외부 DOCX/PDF가 current scope와 다르면 candidate finalize를 차단한다.

개발 중 dirty-tree fail-path를 확인할 때만 `-AllowDirty`와 명시적인 빈 sibling path를 쓴다.
정식 candidate는 clean tree에서 다음처럼 생성한다.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 `
  -RepositoryRoot C:\work\Elmo\Elmo_Master
```

세부 불변 조건과 2026-07-31 검증 결과는
[transactional Distribution candidate 설계](../../docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md)를
참조한다.

## 2.3-candidate 외부 매뉴얼 검토 절차

Canonical Distribution의 `1.9` DOCX/PDF는 수정하지 않는다. Current Markdown에서 편집용
DOCX를 생성하고, Microsoft Word에서 목차와 페이지 번호를 갱신해 저장한 **같은 DOCX**에서
PDF를 export한다.

```powershell
python LMC_Library\LMC_API\Generate-ApiUserManualDocx.py `
  --source LMC_Library\LMC_API\API_USER_MANUAL_KO.md `
  --output output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx
```

검토 후보 경로는 다음 둘이다.

- `output/doc/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx`:
  `93238` bytes, SHA-256
  `A23211A5F530736E6BDC8746DCA1DF4556C47E08524828A7ADB70DC8C91C3182`
- `output/pdf/LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf`:
  `1013620` bytes, SHA-256
  `9E82A467C1BEC2FC3FE20AF1EE8D1332C66D07617CAB2D512C744357C5C28E70`

Word 저장 후 DOCX Office 2016-targeted OpenXmlValidator는 `0`, PDF는 A4 `43`쪽이며 전체 렌더 검수에서
clipping/overlap/blank/tofu가 없고 embedded/subset font `8/8`이다.

두 문서는 `Test-LmcDistributionManualReleasePolicy -DocxText -PdfText`의 exact 3/3을
통과해야 한다. Clean-tree release 재개 시에만 아래처럼 두 경로를 명시적으로 전달한다.
현재 Gate D STOP에서는 이 명령을 실행해 PASS를 주장하지 않는다.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 `
  -RepositoryRoot C:\work\Elmo\Elmo_Master `
  -ManualDocxPath output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx `
  -ManualPdfPath output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf
```

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
- PC 자동 테스트: Debug/Release 각 1042/1042 PASS
- 개발 WPF: Debug/Release build와 actual-control smoke 각 297/297 PASS
- LASAL 정적 계약: `ExpectedSdoWriteAxis=1` SourceOnly/full static PASS;
  `Classes.lcb` declaration과 current source 동기화 확인
- LASAL IDE: current Axis 1 gate-on source Rebuild/Link `0 errors / 20 warnings`,
  Linker Done; executor/service implementation smoke와 신규 `CInvalidArgException=0` PASS.
  current PLC download는 미실시
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
파일은 일관되지만 Axis1 SDO Write, stale recovery retirement와 single-instance 보호가 없는
이전 gate-off snapshot이다. `Build-LmcApiDistribution.ps1`은 이 폴더를 직접 덮어쓰지 않고
별도 sibling candidate만 생성한다. 2026-07-31 실제 current build는 기존 DOCX/PDF의 stale
SDO Write 설명을 `MANUAL_SDO_WRITE_SCOPE`로 차단해 candidate를 만들지 않았고 canonical
tree hash는 전후 동일했다. current PLC live proof와 release-scope 승인 전에는 정식 배포로
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
- `DistributionSemanticPolicy.ps1`: SDK/LASAL/WPF/DINT/README/DOCX/PDF 의미 preflight
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

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
않고 별도 sibling candidate만 생성한다. Commit `88f1c57`부터 staging의
`LasalApiWpfTestApp.sln`은 exact C# project 1개, project GUID 일치와 Debug/Release
`Any CPU`의 `ActiveCfg`/`Build.0`만 허용하고, 같은 solution을 Debug와 Release로 모두
Rebuild한 뒤에만 `Run` 복사와 actual-EXE gate로 진행한다.
Commit `bf31030`은 transaction의 `InputTreeSha256` 범위를 exact project `.lcp/.lcb`,
tracked Class/Include/Source 검증 입력과 tracked+physical Network 전체로 넓혔다. 따라서
seeded ignored `.lba/.lob` 8개도 실제 존재하면 지문에 포함되고, pure-Git checkout에서 새
ignored Network 파일이 나타나거나 Control source, `Classes.lcb`, `Networks.lcb`가 검증 뒤
바뀌면 promotion 전에 fail-closed한다. PS5.1/PS7 pipeline fixture는 각각 `192/192` PASS다.

위 `192/192`은 `bf31030` 시점의 historical pipeline 수치다. Historical predecessor
commit `febb1b0`은 manual/canonical 경로 확정, `vswhere`/Python 등 tool discovery와
transaction 시작보다 먼저 mandatory dual-host release tooling preflight를 실행하도록
고정했다. Windows PowerShell 5.1(Desktop)과 PowerShell 7(Core)은 각각 Pipeline
`245`, SemanticPolicy `50` + policy check `18`, ReleaseManifest `56`, method-size `16`, UDP
callback `296`, Control `HandleRequest` `13`의 동일한 6개 suite를 통과했다. Worker는
inherited `PSModulePath`를 사용하지 않고 exact `$PSHOME\Modules`만 사용하며, suite별
expected evidence 정확히 1개, exact terminal line, stderr 없음과 exit `0`을 요구한다.
Timeout이면 해당 PID의 process tree를 `taskkill /T /F`로 종료한다. 이 predecessor의 최종
Windows PowerShell 5.1 parent 실행은 `802.8`초에 다음 terminal을 반환했다.

```text
PASS LMC.DistributionToolingHostParity 12/12 (PS5=6/6; PS7=6/6) files=92 SHA256=99D6D27101C126D7D03018763067A2D8A2C02B7FBFF41450641822488305DC62
```

Predecessor의 92개 monitored file repository-relative path/length/SHA-256 ordinal digest는
transaction input tree와 promotion drift fence에 묶였다.

Historical predecessor commit `39c3e6f`는 이 gate를 schema 3/toolchain provenance까지
확장했다. Artifact는
ordinal ordering으로 고정하고, manifest에는 절대경로 없는 exact 8-role
`role|version|SHA-256` record를 사용한다. Git은 실제 core executable, C# compiler는 선택된
Roslyn/csc 전체 inventory를 해시한다. 네 개 실제 `.csproj`에 `CscToolPath`, `CscToolExe`,
`RoslynTargetsPath`, `CSharpCoreTargetsPath`, `UseSharedCompilation`의 다섯 property를 강제하고
`UseSharedCompilation=false`를 실행 증거로 확인한다. Python은 base runtime inventory와
`python-docx` 221개, `pypdf` 117개 distribution inventory를 독립 역할로 묶는다. PS5.1/PS7
preflight host executable SHA-256 attestation과 toolchain snapshot은 prepared input에 묶이고,
promotion 직전 실제 경로를 다시 resolve/hash해 드리프트를 fail-closed한다.
이 predecessor의 Windows PowerShell 5.1 parent 실행은 `808.553`초에 다음을 반환했다.

```text
PASS LMC.DistributionToolingHostParity 12/12 (PS5=6/6; PS7=6/6) files=94 SHA256=C25A61055F83B7F171B5FFB7A4F6B821CBC5642EDB2614A9E6D95C7BFBE9F543
```

Host-parity attestation SHA-256은
`A83A038227732EE777F0CDDB1549158633DC0E438B2464200A6EC1ABE0A78215`, toolchain SHA-256은
`9EC464FA97755C202D8DF895767889228169678C16364B21507BAC7A5BDE419D`다. Mandatory aggregate는
호스트별 exact 6-suite/`12/12`로 ToolchainProvenance test를 실행하지 않고 파일만
monitored inventory에 포함한다. 별도 focused PS5.1/PS7에서 각각 provenance `44/44`,
manifest `94/94`, pipeline `284/284`를 PASS했다.

Current commit `1b9be6a`는 ToolchainProvenance를 host별 일곱 번째 mandatory child suite로
통합했다. Windows PowerShell 5.1과 PowerShell 7은 각각 Pipeline `286`, SemanticPolicy
`50` + policy check `18`, ReleaseManifest `100`, ToolchainProvenance `49`, method-size `16`,
UDP callback `296`, Control `HandleRequest` `13`을 별도 worker에서 통과해야 한다. 최종
Windows PowerShell 5.1 parent 실행은 `831331ms`에 다음을 반환했다.

```text
PASS LMC.DistributionToolingHostParity 14/14 (PS5=7/7; PS7=7/7) files=94 SHA256=F2B6DE0D9A595983D94D9E0B58B62BDE4B3FAFBE7F24EE1B6114354C3E7848D8
```

Current host-parity attestation SHA-256은
`CE3D330EE2198070A48D923B43DB33A5E9177D9B4A147B3F46D1772027B34B36`, toolchain SHA-256은
`C3219FED42CD96590BAC56A25702599763284D117DBC0A680CE92AB0F8C15A18`이다. 별도 focused
PS5.1/PS7도 각각 ToolchainProvenance `49/49`, ReleaseManifest `100/100`, Pipeline
`286/286`을 PASS했다. Current 8-role 범위는 active Python dependency closure 전체를
묶지 않는다. 다음 PC-only gap은 actual workload의 external exact 5개인 `lxml`,
`typing_extensions`, `cryptography`, Pillow, cffi의 active dependency closure를 deterministic
inventory/promotion drift fence에 묶는 것이다. cffi의 `_cffi_backend`는 실제 로드됐고
`pycparser`는 미로드이므로, 둘을 혼동하지 않고 unrelated `site-packages`도 제외한다. 이는
PC/tooling 증거일 뿐이다.
Current full Distribution은 여전히 reviewed Gate D STOP으로 actual EXE, current schema 3
manifest, publish 전에 멈춘다.
생성된 current schema 3 candidate manifest는 없고 LASAL IDE, PLC, Download/runtime도 실행하지
않았다.

Current `2.3-candidate` DOCX/PDF는 검토용 입력일 뿐이다. Clean detached
`afdf6a3`에서 두 exact manual을 명시한 full Distribution build를 실제 실행했지만 약
`214`초 뒤 첫 Debug `RunTests` 내부 LASAL 계약에서 `TerminalWakeBrokerCandidate`에
승인된 physical snapshot ratchet이 없다는 current Gate D STOP으로 중단됐다. Git tracked
status는 clean이었지만 noncanonical manual 입력 때문에 `-AllowDirty`를 명시한
`dirty-preview` policy run이었다. 후속 `d6ddf05`와 `bf31030`까지 포함한 clean detached
`bf31030` direct Windows PowerShell 재실행도 `214.415`초 뒤 같은 Debug `RunTests` STOP으로
끝났고 focused verifier는 `10.320`초에 같은 no-approved-ratchet blocker를 확인했다. Sibling
candidate, stage와 lock residue는 없고 canonical tracked state는 불변이며 actual-EXE gate,
manifest와 publish/final rename에는 도달하지 않았다. 따라서 current full Distribution
build와 candidate publish는 PASS가 아니고 Gate D STOP은 그대로다.
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
- `DistributionPipeline.ps1`: staging/lock/seal/drift/success-only rename과 staged example
  solution의 exact one-project/GUID/Debug+Release `Any CPU` 계약 구현
- `DistributionSemanticPolicy.ps1`: SDK/LASAL/WPF/DINT/README/DOCX/PDF 의미 preflight;
  DOCX와 PDF 각각에 `2.3-candidate`, bounded reconnect/actual-EXE PC-only 경계와 preview
  release 안전 경고를 요구한다
- `DistributionToolchainProvenance.ps1`: release host/Git/vswhere/MSBuild/Roslyn/Python package의
  pathless 8-role version/SHA-256 snapshot, host attestation 및 promotion re-resolution gate
- `Test-LmcDistributionToolingHostParity.ps1`: Windows PowerShell 5.1/PowerShell 7의
  mandatory seven-suite host parity, isolated module path, exact evidence와 monitored-file digest gate
- `Test-LmcApiDistributionPipeline.ps1`, `Test-LmcDistributionSemanticPolicy.ps1`,
  `Test-LmcReleaseManifest.ps1`: release 경로 회귀
- `Generate-ApiUserManual.py`: 초기 PDF 초안 생성기
- `Generate-ApiUserManualDocx.py`: 초기 편집용 DOCX 초안 생성기
- `Elmo_API_Packet2/PACKET_ANALYSIS.md`: 원본 packet 분석
- `../LMC_API_Delivery/docs`: 기능별 설계 및 구현 기록

## 배포 원칙

아래 build 단계에 들어가기 전에 dual-host release tooling preflight를 반드시 먼저 실행한다.
Manual/canonical 선택, mutable tool discovery와 transaction은 이 preflight가 exact `14/14`와
validated tooling digest를 반환한 뒤에만 시작한다.

1. `LMC_API_Delivery/src`에서 Release DLL을 새로 빌드한다.
2. `LMC_API_Distribution/01_API`의 DLL만 외부 배포 기준으로 사용한다.
3. 배포 예제는 DLL을 `..\..\01_API\LasalMotionControlLib.dll` 상대경로로
   참조하고 API 소스 프로젝트를 참조하지 않는다.
4. 외부 문서는 `03_API_User_Manual`의 열람용 PDF와 편집용 DOCX를 사용한다.
   사용자가 편집한 DOCX와 그 DOCX에서 내보낸 PDF가 배포 기준이다. Markdown과
   생성 스크립트는 초기 초안 제작용이며 배포 빌드에서 문서를 덮어쓰지 않는다.
5. PC 테스트, LASAL 정적 계약, 배포 예제 solution의 exact one-project/GUID/configuration과
   solution Debug/Release Rebuild 및 최종 EXE/DLL SHA-256을 모두 확인한다.
6. LASAL IDE/PLC 검증이 끝나기 전에는 `production`, `validated` 또는
   `release approved`라고 표기하지 않는다.
7. release build는 같은 volume의 `.LMC_API_Distribution.stage.*`에서 모든 검증을 끝낸 뒤
   존재하지 않는 `LMC_API_Distribution_candidate_*`로 한 번만 rename한다. 실패 시 staging만
   제거하고 canonical과 이미 publish된 candidate는 자동 삭제하지 않는다.
8. schema 3 manifest는 release input tree와 semantic policy hash/result, ordinal artifact records,
   exact 8-role toolchain records/SHA-256, PS5.1/PS7 executable SHA-256 attestation을 포함한다.
   외부 DOCX/PDF가 current scope와 다르거나 promotion 직전 toolchain re-resolution이 다르면
   candidate finalize를 차단한다.
9. Historical predecessor `39c3e6f`은 schema 3과 bounded 8-role provenance를 구현했지만
   mandatory aggregate에는 ToolchainProvenance를 실행하지 않았다.
10. Current `1b9be6a`는 ToolchainProvenance `49`를 host별 일곱 번째 mandatory suite로
    통합해 PS5.1/PS7 aggregate `14/14`를 PASS했다.
11. 다음 external exact 5개 `lxml`, `typing_extensions`, `cryptography`, Pillow, cffi의
    deterministic active dependency closure provenance와 promotion drift를 묶는다. 실제 로드된
    cffi `_cffi_backend`는 포함하고, 미로드 `pycparser`와 unrelated `site-packages`는 제외한다.
12. Gate D가 해제되지 않아 current candidate/manifest/publish는 생성하지 않았다.
    남은 Python provenance 범위와 reviewed Gate D를 모두 닫은 후 clean full Distribution을
    실행한다.

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
통과해야 한다. 이 noncanonical 검토본으로 tracked-clean preview를 재개할 때는 아래처럼
`-AllowDirty`와 두 경로를 명시적으로 전달한다. `afdf6a3` clean detached 재실행은 이 입력으로
시작했지만 위 Gate D physical snapshot ratchet에서 fail-closed STOP했고 full package/candidate
publish PASS를 주장하지 않는다.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 `
  -RepositoryRoot C:\work\Elmo\Elmo_Master `
  -AllowDirty `
  -ManualDocxPath output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx `
  -ManualPdfPath output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf
```

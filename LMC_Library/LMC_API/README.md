# LASAL Motion Control API 내부 개발 영역

이 폴더는 `LasalMotionControlLib`의 내부 설계, 패킷 근거, 소스 리뷰와 배포
절차를 관리한다. 최종 사용자에게 전달하는 폴더가 아니다.

Current 사람용 문서는 한 곳에서 관리한다.

- [API 설명서](../../docs/api/API_MANUAL.md)
- [API 개발 진척도](../../docs/api/API_DEVELOPMENT_PROGRESS.md)

아래의 시험 수치와 release candidate 설명은 기존 배포 baseline을 재현하기 위한 역사 기록이다.
current 구현률, 시험 결과와 artifact identity를 판단할 때 사용하지 않는다.

## Historical 2.3-candidate 기준

- API 버전: `0.9.1-preview`
- 대상: .NET Framework 4.8
- PC API 소스: `../LMC_API_Delivery/src`
- 자동 테스트: `../LMC_API_Delivery/tests`
- 개발용 WPF 예제: `../LasalApiWpfTestApp`
- 정식 배포 폴더: `../LMC_API_Distribution`
- LASAL 어댑터: `../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- PC 자동 테스트: historical SDK Debug/Release direct 각 1133/1133 PASS
- 개발 WPF: historical full smoke `339/339`과 actual example EXE relaunch gate 각 `1/1` PASS;
  current V2는 Release full smoke `347/347`, isolated Debug build와 `Wpf.CallbackV2.*`
  `17/17` PASS. Actual-EXE V2 및 PLC same-window reconnect는 미실행
- LASAL 정적 계약: historical `GateDVisualLayout` checkpoint와 pre-approval STOP evidence를
  보존한다. Current `d4204b4` clean detached tracked `24402BFA...` tuple은 PS5.1/PS7 verifier
  self-test 각 `296/296`과 SourceOnly를 PASS하고 `ProductionApproved=true`,
  `NeedsRebaseline=false`다. Main working tree 사용자 `D4C1FF46...`는 exact sanctioned identity
  drift로 계속 reject되며 post-approval full/network static target은 실행하지 않았다
- LASAL IDE: historical Rebuild/Link와 implementation smoke 증거는 보존한다. Exact tracked
  Gate D static 승인은 LASAL IDE build, PLC Download/runtime 또는 main dirty artifact 승인이 아니다
- reconnect PLC image: predecessor `d4204b4`/`e3c9365` checkpoint 당시에는 뒤이은
  `bbe8a8d`/current reconnect image 전달이 없었다. 이후 2026-08-12 15:58에 current reconnect
  source와 일치하는 image를 LASAL build/download했지만, 같은 창 Close -> Connect live PASS는
  아직 확인되지 않았다
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

Historical implementation commit `1b9be6a`와 이를 기록한 documentation commit `4867096`은
ToolchainProvenance를 host별 일곱 번째 mandatory child suite로 통합한 8-role predecessor다.
Windows PowerShell 5.1과 PowerShell 7은 각각 Pipeline `286`, SemanticPolicy `50` + policy
check `18`, ReleaseManifest `100`, ToolchainProvenance `49`, method-size `16`, UDP callback
`296`, Control `HandleRequest` `13`을 별도 worker에서 통과했다. 최종 Windows PowerShell 5.1
parent 실행은 `831331ms`에 다음을 반환했다.

```text
PASS LMC.DistributionToolingHostParity 14/14 (PS5=7/7; PS7=7/7) files=94 SHA256=F2B6DE0D9A595983D94D9E0B58B62BDE4B3FAFBE7F24EE1B6114354C3E7848D8
```

이 predecessor의 host-parity attestation SHA-256은
`CE3D330EE2198070A48D923B43DB33A5E9177D9B4A147B3F46D1772027B34B36`, toolchain SHA-256은
`C3219FED42CD96590BAC56A25702599763284D117DBC0A680CE92AB0F8C15A18`이다. 별도 focused
PS5.1/PS7도 각각 ToolchainProvenance `49/49`, ReleaseManifest `100/100`, Pipeline
`286/286`을 PASS했다.

Current commit `3c63dea`는 actual DOCX/PDF workload가 로드한 exact seven root package owner를
schema 3의 pathless 13-role provenance와 promotion re-resolution에 묶었다. Mandatory gate는
계속 host별 exact seven suite이며 PS5.1-parent 결과는 다음과 같다.

```text
PASS LMC.DistributionToolingHostParity 14/14 (PS5=7/7; PS7=7/7) files=94 SHA256=F687FDE9198C9F0CDF8AB4106FAB0C3B5059DF49B55C8E9B34DEC99859CDB4CA
```

Current host-parity attestation SHA-256은
`FBAD123C4E3DEC4E9018885559E1645A69E47E69DE0E83D1116F8581D27B787D`, toolchain SHA-256은
`91E56793F99B5D17D9325D425308179FB780161CFFD9D29613653737C2D6F7EB`이다. PS5.1/PS7
focused 결과는 각각 ToolchainProvenance `84/84`, ReleaseManifest `108/108`, Pipeline
`291/291`, SemanticPolicy `52/52` + policy check `18`이다. Exact inventory는
`CSharpCompiler=108`, `Git=1`, `MSBuild=1`, `PowerShell=1`, `PyPdf=117`, `Python=2489`,
`PythonCffi=53`, `PythonCryptography=195`, `PythonDocx=221`, `PythonLxml=208`,
`PythonPillow=219`, `PythonTypingExtensions=7`, `VsWhere=1`이다.

Active owner set은 `cffi`, `cryptography`, `lxml`, `pillow`, `typing-extensions`,
`python-docx`, `pypdf` exact seven이다. cffi metadata의 `Scripts` entrypoint는 Python
root-relative path로 normalize하고, base `Python` role에서는 `Scripts`와
`Lib/site-packages`를 제외한 뒤 exact package-owner inventory로만 다시 포함한다. Active
package의 `.pyc`는 보존한다. Ownerless module은 bounded built-in/frozen/runtime-root/synthetic
계약만 허용하며 `Scripts`, `site-packages` 또는 runtime 외부 path는 fail-closed한다.
`pycparser`와 unrelated `site-packages`는 active owner set에 없으므로 포함하지 않는다.
Toolchain probe, semantic document extraction, PDF validation, DOCX validation의 네 Python
실행 path는 모두 `-B`를 강제한다.

이는 PC/tooling 증거일 뿐이다. Last full Distribution은 reviewed Gate D의 승인되지 않은
`TerminalWakeBrokerCandidate` physical snapshot ratchet에서 STOP했고 actual EXE, current
generated schema 3 candidate manifest와 publish에 도달하지 않았다. 이 문장은
`d4204b4` 전 historical run 결과다.

Commit `bcc6a9c`는 독립 검토한 `2.3-candidate` DOCX/PDF를 tracked canonical release input으로
승격했다. DOCX는 `91,103` bytes / SHA-256
`F3DC33521A8DB623641FA07A2C1B161009BCF3F01622DC037442A9726900F8DD`, PDF는 `1,002,300`
bytes / SHA-256 `317A87FC42EF5A845202FFDB384C3AC23247C1B7A73530488C96FF0D805D2880`이다. Word와
OpenXML validation error는 `0`, PDF는 A4 `43`쪽, DOCX heading `66`개/table `109`개이며 모든
font가 embedded됐다. 43쪽 visual 검토에서 clipping/overlap/blank/tofu는 없었고 실제 추출 text의
manual release policy는 `3/3` PASS했다. PS5.1/PS7 focused Pipeline `291/291`,
SemanticPolicy `52/52` + policy check `18`, ReleaseManifest `108/108`도 PASS했다. Clean detached
`bcc6a9c`에서 default resolver가 이 canonical pair를 선택하고 worktree state가 clean이며 manual
policy `3/3`임을 다시 확인했다. 이는 production 승인이 아니라 clean candidate용 입력 baseline이다.

Commit `f304e8b`는 canonical package/example README에 preview, production NO-GO와 current SDO
안전 범위를 맞추고 해당 의미를 semantic regression에 고정했다. PS5.1/PS7은 각각
SemanticPolicy `53/53` + policy check `18`, Pipeline `291/291`, ToolchainProvenance `84/84`를
PASS했다. Production template와 build logic은 바꾸지 않았다. Canonical source snapshot을 직접
검사한 semantic run은 manual/README policy를 지난 뒤 `CANDIDATE_WPF_SOURCE_SET`에서 멈췄다.
이 snapshot은 fresh staged candidate가 아니므로 current full Distribution 또는 candidate PASS가
아니다.

Commit `978597b`는 위 active closure, initial canonical manual과 README policy를 current
release-input documentation baseline으로 기록했다. Commit `d4204b4`는 모든 기존 exact 검사를
통과한 clean tracked `Classes.lcb` 8,549,773 bytes / SHA-256
`24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861`
`TerminalWakeBrokerCandidate`만 PC/static `ProductionApproved=true`, `NeedsRebaseline=false`로
승인했다. PS5.1/PS7 self-test는 각각 `296/296`, clean detached SourceOnly는 두 host에서
PASS했다. Main working tree 사용자 `Classes.lcb` SHA-256
`D4C1FF4650499777A17854DA638269543938532520F0C5D178D61FF13BAA0C36`는 계속 reject된다.

Commit `5d5aebe`는 이 Gate D 경계를 반영한 당시 canonical manual을 게시했다. Markdown은
`94,108` bytes / SHA-256
`D7DE1AF51A548AA7361614167D546A7057C8D03260CE92CFA9335964A611C022`, DOCX는 `92,229`
bytes / SHA-256 `57D17650D1F24E9350830E784EFE94E00CB1A89CB126CD9A05865580A9708B46`, PDF는
`1,003,309` bytes / SHA-256
`83A57CC4B15D4E0BA4E0D9A54FD044C82A131168D16B36F2694F76AF098232E0`이다. `bcc6a9c`의
이전 size/hash와 검토 결과는 initial promotion historical evidence로 보존한다.

2026-08-12 reconnect V2 manual 갱신 뒤 current tracked pair는 Markdown `96,004` bytes /
SHA-256 `9A5FE9D42F08EF2E4B3507EAEFF8956013F819B3A7F2FB00E334C356CAC3179E`, DOCX
`95,511` bytes / `1BD54016B6E121CABF152164499E3DD943249C48A7ED6D8FE66271969C8A04B3`, PDF
`1,022,442` bytes / `50DDE24F8B45341500D1DCF6A647BD818D6B805D9CC0F81737664F33A5550A44`다.
PS5.1/PS7 SemanticPolicy self-test는 actual canonical pair smoke와 scoped negative를 포함해 각각
`70/70`, policy check `18`을 PASS했다. Actual extracted DOCX/PDF reconnect policy `3/3`, A4 `43`쪽, heading `66`, table `109`,
Office 2010~Microsoft 365 OpenXML error `0`, all-font embedding과 전 페이지 visual defect `0`을
확인했다. 이는 release-input 문서 증거이며 PLC reconnect runtime PASS가 아니다.

Exact tracked Gate D static 승인 뒤 full/network static target과 clean full Distribution,
current generated schema 3 candidate, full-build actual EXE gate, manifest와 publish는 실행하거나
생성하지 않았다. `d4204b4`/`5d5aebe` checkpoint 당시 LASAL IDE, PLC, Download/runtime도
실행하지 않았다. 이후 15:58 current reconnect image build/download는 별도 범위로 완료됐지만
같은 창 live PASS는 없으며, Production NO-GO와 clean full Distribution에서 fresh candidate의
WPF source set부터 다시 검증하는 조건은 그대로다.

## 내부 문서

- [API_DEVELOPMENT_GUIDE.md](API_DEVELOPMENT_GUIDE.md): 구조, wire 계약,
  변경 규칙, 테스트와 릴리스 절차
- [API_DEVELOPMENT_GUIDE.html](API_DEVELOPMENT_GUIDE.html): 위 내부 설명서의
  standalone HTML
- [API_DEVELOPMENT_GUIDE_PRINT_STYLE.html](API_DEVELOPMENT_GUIDE_PRINT_STYLE.html):
  Pandoc HTML 재생성용 print style
- [API_SOURCE_REVIEW_2026-07-15.md](API_SOURCE_REVIEW_2026-07-15.md): 이번
  배포 준비 전 소스/문서/바이너리 리뷰 결과
- [API_MANUAL.md](../../docs/api/API_MANUAL.md): current 사용자 매뉴얼 Markdown 정본
- [API_DEVELOPMENT_PROGRESS.md](../../docs/api/API_DEVELOPMENT_PROGRESS.md): current 진척도 정본
- [API_USER_MANUAL_KO.md](API_USER_MANUAL_KO.md): legacy 경로 이동 안내
- `Build-LmcApiDistribution.ps1`: canonical 무변경 transactional candidate build
- `DistributionPipeline.ps1`: staging/lock/seal/drift/success-only rename과 staged example
  solution의 exact one-project/GUID/Debug+Release `Any CPU` 계약 구현
- `DistributionSemanticPolicy.ps1`: SDK/LASAL/WPF/DINT/README/DOCX/PDF 의미 preflight;
  DOCX와 PDF 각각에 `2.3-candidate`, bounded reconnect/actual-EXE PC-only 경계와 preview
  release 안전 경고를 요구한다
- `DistributionToolchainProvenance.ps1`: release host/Git/vswhere/MSBuild/Roslyn/Python active
  package의 pathless 13-role version/SHA-256 snapshot, host attestation 및 promotion
  re-resolution gate
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
   exact 13-role toolchain records/SHA-256, PS5.1/PS7 executable SHA-256 attestation을 포함한다.
   외부 DOCX/PDF가 current scope와 다르거나 promotion 직전 toolchain re-resolution이 다르면
   candidate finalize를 차단한다.
9. Historical predecessor `39c3e6f`은 schema 3과 bounded 8-role provenance를 구현했지만
   mandatory aggregate에는 ToolchainProvenance를 실행하지 않았다.
10. Historical `1b9be6a`/`4867096`은 8-role ToolchainProvenance `49`를 host별 일곱 번째
    mandatory suite로 통합해 PS5.1/PS7 aggregate `14/14`를 PASS했다.
11. Current `3c63dea`는 exact seven active Python root package owner를 포함한 pathless 13-role
    provenance와 promotion drift fence를 구현했다. Active package `.pyc`는 유지하고 base
    `Scripts`/`site-packages`, `pycparser`, unrelated package는 제외하며 ownerless 경계를
    fail-closed한다.
12. `d4204b4`는 exact clean tracked `24402BFA...` Gate D physical snapshot ratchet을 승인했다.
    Main working tree 사용자 `D4C1FF46...`는 계속 reject되며 post-approval full/network static은
    실행하지 않았다.
13. `5d5aebe`는 Gate D 경계를 반영한 당시 canonical manual을 게시했다. 이후에도 clean full
    Distribution을 실행하지 않아 current generated schema 3 candidate manifest, full-build actual
    EXE gate와 publish는 생성하지 않았다. Canonical source snapshot의
    `CANDIDATE_WPF_SOURCE_SET` STOP은 fresh candidate가 아니다.
14. 2026-08-12 reconnect V2 policy와 current source 경계를 Markdown/DOCX/PDF에 반영하고
    canonical pair를 재검증했다. Same-window PLC reconnect는 여전히 미검증이다.

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

## 2.3-candidate canonical 외부 매뉴얼 baseline

Historical 2.3 Markdown에서 편집용 DOCX를 생성하고, Microsoft Word에서 목차와 페이지 번호를
갱신해 저장한 **같은 DOCX**에서 PDF를 export했다. Initial 검토 완료 pair는 `bcc6a9c`에서
tracked canonical release input으로 승격했고 `5d5aebe`가 Gate D 경계를 반영한 current pair를
게시했다. Current pair는 reconnect V2 경계를 추가 반영했다. 아래 명령은 current
`2.4-development` Markdown의 검토용 초안을 생성하며 **2.3-candidate를 덮어쓰지 않는다**.
2.4 DOCX/PDF 독립 검토와 semantic policy/test 승격 전에는 distribution build가 명시적으로
차단된다.

```powershell
python LMC_Library\LMC_API\Generate-ApiUserManualDocx.py `
  --source docs\api\API_MANUAL.md `
  --output output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.4-development.docx
```

Current canonical 경로와 exact bytes는 다음과 같다.

- `LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx`:
  `95,511` bytes, SHA-256
  `1BD54016B6E121CABF152164499E3DD943249C48A7ED6D8FE66271969C8A04B3`
- `LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf`:
  `1,022,442` bytes, SHA-256
  `50DDE24F8B45341500D1DCF6A647BD818D6B805D9CC0F81737664F33A5550A44`

`bcc6a9c` initial pair의 Word/OpenXML validation error `0`, A4 PDF `43`쪽, DOCX heading
`66`/table `109`, all-font embedding과 전 페이지 visual defect `0`은 historical 검토 결과다.
Current V2 pair도 A4 `43`쪽, DOCX heading `66`/table `109`, Office 2010~Microsoft 365
OpenXML error `0`, all-font embedding, 전 페이지 visual defect `0`과 extracted-text policy
`3/3`을 통과했다. 양 host SemanticPolicy self-test는 actual pair smoke를 포함해 `70/70`이다.

Initial pair는 `Test-LmcDistributionManualReleasePolicy -DocxText -PdfText` exact `3/3`을
통과했고 clean detached `bcc6a9c`에서 default resolver canonical 선택/worktree clean/manual
policy `3/3`을 확인했다. Current V2 pair도 canonical 경로에 있으므로 clean candidate는 manual
override 없이 이 tracked input을 resolve해야 한다.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 `
  -RepositoryRoot C:\work\Elmo\Elmo_Master
```

Exact tracked Gate D static 승인은 `d4204b4`에서 완료됐지만 그 뒤 full/network static과 clean
full Distribution은 실행하지 않았다. 이 baseline은 production 승인이 아니며 clean full
Distribution과 PLC/runtime Definition of Done이 남아 있다.

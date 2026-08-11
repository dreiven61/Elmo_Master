# LMC API transactional Distribution candidate 설계 및 검증

- 기준 시각: 2026-07-31 01:59 KST
- 대상: `main@6537bcf1bf0fdb338a934b63891fc9ee110aecad` + 현재 working tree
- 상태: transaction/semantic preflight와 비canonical `2.0-candidate` manual 실제 candidate finalize PASS
- production 판정: **NO-GO**

## 1. 결론

`Build-LmcApiDistribution.ps1`은 더 이상 기존
`LMC_Library/LMC_API_Distribution`을 in-place로 수정하지 않는다. 같은 volume의 sibling
staging에 current source candidate를 조립하고, 모든 build·test·semantic·manifest 검증이
끝난 경우에만 존재하지 않는 candidate path로 `Directory.Move`를 한 번 수행한다.

첫 실제 전체 실행은 SDK/LASAL/WPF와 DOCX 구조 검사를 지난 뒤 기존 canonical DOCX/PDF의
`MANUAL_SDO_WRITE_SCOPE`에서 의도적으로 차단됐다. 이후 current Markdown에서 생성하고 전 페이지
검토한 비canonical `2.0-candidate` DOCX/PDF를 명시적 입력으로 사용했다. 최종 provenance-hardened
실행은 15-check semantic preflight와 schema 2 manifest 검증을 통과하고 sibling candidate를
publish했다. canonical Distribution은 전후 동일하다.

이 결과는 PC release 경로의 fail-closed 증거다. current PLC download, Motion/Power 또는
SDO Write live 실행 증거는 아니다.

### 2026-08-12 release-host preflight historical predecessor

위 candidate PASS와 `56/56`, `28/28`/15, `86/86` 수치는 2026-07-31 historical evidence다.
Commit `febb1b0`은 `Build-LmcApiDistribution.ps1`의 가장 앞에 mandatory dual-host
tooling preflight를 추가했다. 이 gate는 manual/canonical 경로 확정, `vswhere`/Python 등
mutable tool discovery와 transaction보다 먼저 실행된다.

Windows PowerShell 5.1(Desktop)과 PowerShell 7(Core)은 각각 아래 exact 6-suite 계약을
통과해야 한다.

| suite | host별 current evidence |
|---|---:|
| Pipeline | `245` |
| SemanticPolicy | `50`, policy check `18` |
| ReleaseManifest | `56` |
| method-size | `16` |
| UDP callback | `296` |
| Control `HandleRequest` | `13` |

Worker는 poisoned inherited `PSModulePath` 대신 exact `$PSHOME\Modules`만 사용한다. 각 suite는
expected evidence 정확히 1개, exact terminal line, stderr 없음과 exit `0`을 요구하며 timeout은
해당 PID의 process tree를 `taskkill /T /F`로 종료한다. 최종 Windows PowerShell 5.1 parent
실행은 `802.8`초에 다음 terminal을 반환했다.

```text
PASS LMC.DistributionToolingHostParity 12/12 (PS5=6/6; PS7=6/6) files=92 SHA256=99D6D27101C126D7D03018763067A2D8A2C02B7FBFF41450641822488305DC62
```

이 결과는 PC/tooling 검증이다. Full Distribution은 current Gate D STOP으로 actual-EXE,
current manifest와 publish/final rename 전에 중단된 상태이며 LASAL IDE, PLC Download/runtime은
실행하지 않았다.

### 2026-08-12 schema 3/bounded toolchain provenance historical predecessor

Commit `39c3e6f`는 release provenance를 schema 3으로 올렸다. Package artifact는
repository/host culture와 무관한 ordinal ordering으로 고정하고, manifest에는 절대경로를
노출하지 않는 exact 8-role `role|version|SHA-256` record를 기록한다. 논리적 role은
`CSharpCompiler`, `Git`, `MSBuild`, `PowerShell`, `PyPdf`, `Python`, `PythonDocx`,
`VsWhere`다. Git은 wrapper가 아닌 실제 core executable을 resolve/hash하고, C# compiler는
선택된 Roslyn/csc 전체 inventory를 해시한다.

실제 build에서는 SDK test, WPF smoke, SDK library, staged example의 네 `.csproj`에
`CscToolPath`, `CscToolExe`, `RoslynTargetsPath`, `CSharpCoreTargetsPath`,
`UseSharedCompilation`의 다섯 compiler property를 강제하고 `UseSharedCompilation=false`를
실행 증거로 확인한다. Python role은 `site-packages`를 제외한 base runtime 전체,
`PythonDocx`는 `python-docx` 221개, `PyPdf`는 `pypdf` 117개 distribution file inventory를
독립적으로 묶는다. PS5.1/PS7 host executable SHA-256 attestation, monitored snapshot,
toolchain snapshot을 transaction input에 묶이고 promotion 직전 실제 executable/package를 다시
resolve/hash해 드리프트를 fail-closed한다.

위 범위는 active Python dependency closure 전체를 묶지 않는 bounded snapshot이다.
`python-docx`/`pypdf`의 실제 import closure에 있는 `lxml`, `typing_extensions`,
`cryptography`, Pillow, cffi는 8-role inventory에 포함되지 않는다. 또한 이 commit의 mandatory
aggregate는 ToolchainProvenance test를 실행하지 않고 파일만 monitored inventory에 포함했다.

최종 Windows PowerShell 5.1 parent aggregate는 `808.553`초에 다음을 반환했다.

```text
PASS LMC.DistributionToolingHostParity 12/12 (PS5=6/6; PS7=6/6) files=94 SHA256=C25A61055F83B7F171B5FFB7A4F6B821CBC5642EDB2614A9E6D95C7BFBE9F543
```

Host-parity attestation SHA-256은
`A83A038227732EE777F0CDDB1549158633DC0E438B2464200A6EC1ABE0A78215`, toolchain SHA-256은
`9EC464FA97755C202D8DF895767889228169678C16364B21507BAC7A5BDE419D`다. Mandatory aggregate는
호스트별 exact 6-suite/`12/12`로 ToolchainProvenance test를 실행하지 않고 파일만
monitored inventory에 포함한다. PS5.1/PS7 별도 focused로 provenance `44/44`, manifest
`94/94`, pipeline `284/284`가 모두 PASS했다.

### 2026-08-12 current mandatory seventh-suite provenance gate

Commit `1b9be6a`는 ToolchainProvenance를 host별 일곱 번째 mandatory child suite로 통합했다.
Windows PowerShell 5.1과 PowerShell 7은 각각 Pipeline `286`, SemanticPolicy `50` + policy
check `18`, ReleaseManifest `100`, ToolchainProvenance `49`, method-size `16`, UDP callback
`296`, Control `HandleRequest` `13`을 별도 worker에서 통과해야 한다. 최종 Windows PowerShell
5.1 parent aggregate는 `831331ms`에 다음을 반환했다.

```text
PASS LMC.DistributionToolingHostParity 14/14 (PS5=7/7; PS7=7/7) files=94 SHA256=F2B6DE0D9A595983D94D9E0B58B62BDE4B3FAFBE7F24EE1B6114354C3E7848D8
```

Current host-parity attestation SHA-256은
`CE3D330EE2198070A48D923B43DB33A5E9177D9B4A147B3F46D1772027B34B36`, toolchain SHA-256은
`C3219FED42CD96590BAC56A25702599763284D117DBC0A680CE92AB0F8C15A18`이다. 별도 focused
PS5.1/PS7도 각각 ToolchainProvenance `49/49`, ReleaseManifest `100/100`, Pipeline
`286/286`을 PASS했다. 다음 PC-only gap은 actual workload의 external exact 5개인 `lxml`,
`typing_extensions`, `cryptography`, Pillow, cffi의 active dependency closure만 deterministic
provenance와 promotion re-resolution에 묶는 것이다. cffi의 `_cffi_backend`는 실제 로드됐고
`pycparser`는 미로드이므로, 둘을 혼동하지 않고 unrelated `site-packages`도 제외한다.

이는 PC/tooling 증거이며 current full Distribution은
reviewed Gate D STOP으로 actual EXE, current schema 3 manifest, publish 전에 멈춰 있다.
따라서 실제로 생성된 current schema 3 candidate manifest는 없고 LASAL IDE, PLC,
Download/runtime도 실행하지 않았다.

## 2. 기존 결함

이전 build 순서는 기존 Distribution의 DLL, 예제 EXE/DLL/config를 차례로 덮어쓴 뒤
후반에 문서, cleanup, hash와 manifest를 검증했다. 중간 실패 시 다음 문제가 가능했다.

- 기존 package가 old/new 파일이 섞인 상태가 됨
- stale manifest가 변경된 binary를 가리킴
- 실패 지점에 따라 `bin/obj/.vs` 또는 부분 복사 파일이 남음
- current source와 외부 DOCX/PDF의 의미가 달라도 구조 검증만 통과할 수 있음

## 3. 구현 파일

| 파일 | 책임 |
|---|---|
| `LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` | 명시적 manual 입력, release input/Git provenance, 전체 build/test, candidate 조립, semantic preflight와 manifest 생성 orchestration |
| `LMC_Library/LMC_API/DistributionPipeline.ps1` | manual 경로/byte snapshot, canonical snapshot, exclusive lock, sibling staging, seal, drift 검사, success-only rename와 안전 cleanup |
| `LMC_Library/LMC_API/DistributionSemanticPolicy.ps1` | SDK/LASAL/WPF/DINT/README/DOCX/PDF 의미 교차 검증 |
| `LMC_Library/LMC_API/DistributionToolchainProvenance.ps1` | pathless 8-role toolchain snapshot, host attestation binding, physical inventory와 promotion re-resolution |
| `LMC_Library/LMC_API/ReleaseManifest.ps1` | schema 3 ordinal artifact/toolchain/preflight manifest 생성과 재검증 |
| `LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1` | PS5.1/PS7 seven-suite host parity, isolated module path, exact evidence/timeout와 monitored-file digest preflight |
| `LMC_Library/LMC_API/DistributionREADME.md` | candidate 최상위 README 원본 |
| `LMC_Library/LMC_API/DistributionExampleREADME.md` | binary-reference 예제 README 원본 |
| `LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1` | transaction 성공/실패/경쟁/drift/cleanup 회귀 |
| `LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1` | semantic false-pass와 실제 source 계약 회귀 |
| `LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1` | manifest schema/hash/atomic write 회귀 |

## 4. transaction 불변 조건

1. canonical Distribution은 read-only input으로만 취급한다.
2. candidate는 canonical의 direct sibling이며 이름은
   `LMC_API_Distribution_candidate_*` 형식이고 시작 시 존재하지 않아야 한다.
3. staging과 candidate는 canonical과 같은 volume을 사용한다.
4. sibling lock은 `FileShare.None`으로 한 writer만 허용한다.
5. 시작 시 canonical file/directory set과 모든 파일 SHA-256을 snapshot한다.
6. build 입력 파일의 경로·크기·SHA-256을 정렬해 input tree SHA-256을 만든다.
7. validation 뒤 staging을 seal하고 promotion 직전에 staging tamper, input drift와
   canonical drift를 다시 검사한다.
8. commit은 staging에서 candidate로의 `Directory.Move` 한 번뿐이다.
9. commit 전 실패 시 검증된 staging만 제거한다. canonical과 이미 commit된 candidate는
   자동 삭제하지 않는다.
10. 정상/일반 실패 후 transaction lock을 안전하게 닫고 제거한다.
11. 비canonical DOCX/PDF는 둘을 함께 지정해야 하며 repository 내부의 non-reparse regular
    file이어야 한다. 이 입력은 `-AllowDirty` 없이는 거부하고 manifest를 항상
    `dirty-preview`로 기록한다.
12. manual은 transaction lock 안에서 exact byte snapshot을 만들고, 원래 logical
    repository-relative path에 snapshot length/hash를 대응시켜 input tree hash를 계산한다.
    staging에는 원본 경로를 다시 읽지 않고 snapshot bytes만 기록한다.
13. staged manual hash는 snapshot과 같아야 하며 promotion 직전 live input hash와
    `SourceCommit`/`WorktreeState`를 다시 확인한다. manifest는 callback이 받은 transaction
    baseline과 prepared Git metadata만 기록한다.
14. manual/canonical 경로 확정, mutable tool discovery와 transaction 전에 dual-host tooling
    preflight `14/14`가 먼저 PASS해야 한다.
15. current preflight 전후 94개 monitored file의 repository-relative path/length/SHA-256을 ordinal
    정렬해 digest를 만들고 prepared input 작성과 promotion 전까지 같은 snapshot인지 확인한다.
16. validated tooling digest를 input tree의 synthetic record에 포함해 preflight evidence와
    transaction fingerprint를 연결한다.
17. actual release toolchain은 pathless 8-role version/SHA-256와 PS5.1/PS7 executable SHA-256
    attestation으로 transaction fingerprint에 묶는다.
18. promotion 직전에 Git core, PowerShell, vswhere, MSBuild, Roslyn/csc, Python base runtime,
    `python-docx`, `pypdf`를 물리 경로에서 다시 resolve/hash하고 prepared snapshot과
    하나라도 다르면 publish를 차단한다.

## 5. candidate 내용과 검증 순서

아래 candidate 조립 순서는 mandatory dual-host tooling preflight가 PASS하고 validated tooling
snapshot을 반환한 뒤에만 시작한다.

1. current SDK와 개발 WPF source를 입력으로 수집한다.
2. 예제 project의 source 항목과 bytes를 current 개발 project와 exact 비교한다.
3. 예제 project는 source `ProjectReference` 대신
   `..\..\01_API\LasalMotionControlLib.dll` binary reference를 사용한다.
4. SDK Debug/Release test, LASAL network/static contract, WPF Debug/Release smoke를 실행한다.
5. SDK Release DLL과 candidate 예제 Debug/Release를 build한다.
6. 검토한 외부 DOCX/PDF의 exact bytes를 staging하고 hash, 최소 구조와 semantic scope를 검사한다.
7. canonical/source/run DLL이 byte-identical인지 확인한다.
8. candidate의 `bin/obj/.vs`만 안전하게 제거한다.
9. schema 3 `RELEASE_MANIFEST.md`를 atomic write하고 즉시 재검증한다.
10. seal/drift 검사를 통과한 경우에만 candidate 이름으로 publish한다.

## 6. semantic policy 범위

정책은 다음 계약을 SDK, LASAL, WPF, DINT map, candidate README와 외부 DOCX/PDF에서
필요한 조합으로 교차 확인한다.

- ACK는 request acceptance이며 terminal polling이 completion 증거다.
- Close/Dispose/Cancel/timeout은 Motion Stop을 전송하지 않는다.
- motion 값의 UNIT 변환은 caller 책임이며 wire 값은 raw DINT다.
- preview이며 PLC/live 미검증 상태를 production 승인으로 표현하지 않는다.
- D4 Double과 PI Write는 비활성이다.
- topology bits 15~17은 0이고 `0x7E23` PLC route는 없다.
- 유일한 SDO Write allowlist는 Axis 1 `0x2F00:24 Int32/4`다.
- Axis 2~4와 다른 target은 차단한다.
- manual Write는 current connection/session의 `DiagnosticsBuild`, `BootId`,
  `MapRevision`, exact target identity와 서로 다른 four-ticket same-value proof를 요구한다.
- reconnect, disconnect 또는 identity/target drift 뒤 proof를 재사용하지 않는다.

## 7. manifest schema 3

candidate manifest에는 다음 provenance를 필수 기록한다.

- source commit과 dirty/clean state
- release input tree SHA-256
- semantic policy SHA-256과 `PASS` result
- ordinal-sorted package artifact path/size/SHA-256
- pathless 8-role release toolchain version/SHA-256와 aggregate toolchain SHA-256
- PS5.1/PS7 edition/major/version/executable SHA-256 host record, preflight digest/file count/run
  count와 attestation SHA-256
- assembly/file/product version
- canonical DLL과 두 replica의 byte identity
- package 전체 파일의 relative path, size와 SHA-256

manifest 자체는 temporary file에 쓴 뒤 rename하며, 생성 직후 같은 입력으로 다시 검증한다.
Current schema 3 구현의 Python 항목은 base runtime, `python-docx`, `pypdf`까지이며
`lxml`/`typing_extensions`/`cryptography`/Pillow/cffi active dependency closure는 후속
PC-only provenance gap으로 남아 있다. cffi `_cffi_backend`는 loaded, `pycparser`는
unloaded 경계다.

## 8. 검증 결과

### 단위/정책 회귀 (2026-07-31 historical)

| 검증 | 결과 |
|---|---:|
| PowerShell parser | PASS |
| release manifest | `56/56` PASS |
| semantic policy | `28/28`, policy check `15` PASS |
| transaction pipeline | `86/86` PASS |
| semantic policy SHA-256 | `13783910DC38D86B7470CBA0721012B3E262DC3A3429BE50C24D4592DCDF5352` |

transaction fixture는 manual pair/path/reparse/worktree/snapshot, prepared input/metadata,
success, callback failure, seal tamper, input drift, occupied target, canonical mutation,
nested lock 경쟁과 cleanup 거부를 포함한다. 일반 성공/실패에서 canonical hash 불변,
staging residue 0과 lock residue 0을 확인했다.

### 듀얼 호스트 release tooling preflight (2026-08-12 current `1b9be6a`)

| 검증 | PS5.1 | PS7 |
|---|---:|---:|
| Pipeline | `286/286` | `286/286` |
| SemanticPolicy | `50/50`, policy check `18` | `50/50`, policy check `18` |
| ReleaseManifest | `100/100` | `100/100` |
| ToolchainProvenance | `49/49` | `49/49` |
| method-size | `16/16` | `16/16` |
| UDP callback | `296/296` | `296/296` |
| Control `HandleRequest` | `13/13` | `13/13` |

전체 결과는 `14/14`(`PS5=7/7`, `PS7=7/7`), elapsed `831331ms`, monitored files는 `94`,
ordinal digest는 `F2B6DE0D9A595983D94D9E0B58B62BDE4B3FAFBE7F24EE1B6114354C3E7848D8`이다.
Host-parity attestation SHA-256은
`CE3D330EE2198070A48D923B43DB33A5E9177D9B4A147B3F46D1772027B34B36`, toolchain SHA-256은
`C3219FED42CD96590BAC56A25702599763284D117DBC0A680CE92AB0F8C15A18`이다. Mandatory
aggregate는 위 exact seven suite를 두 host에서 각각 실행한다. Historical predecessor
`39c3e6f`의 `12/12`, files `94`, digest
`C25A61055F83B7F171B5FFB7A4F6B821CBC5642EDB2614A9E6D95C7BFBE9F543`, 별도 focused
ToolchainProvenance `44/44`는 mandatory 통합 전 snapshot으로 보존한다. 더 이전 `febb1b0`의
`12/12`, files `92`, digest
`99D6D27101C126D7D03018763067A2D8A2C02B7FBFF41450641822488305DC62`는 94-file/schema 3
보강 전 snapshot으로 보존한다.

### 2026-07-31 historical current input 전체 실행

이번 dirty-preview 실제 실행은 검토한 비canonical manual pair와 존재하지 않는 sibling 경로를
명시했다.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 `
  -RepositoryRoot C:\work\Elmo\Elmo_Master `
  -AllowDirty `
  -CandidatePath C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution_candidate_20260731_manual_2_0_provenance `
  -ManualDocxPath C:\work\Elmo\Elmo_Master\output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.0-candidate.docx `
  -ManualPdfPath C:\work\Elmo\Elmo_Master\output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.0-candidate.pdf
```

| 항목 | 관측값 |
|---|---|
| 최종 결과 | transaction commit 및 semantic policy `15/15` PASS |
| published candidate | `LMC_Library/LMC_API_Distribution_candidate_20260731_manual_2_0_provenance` |
| source commit / state | `6537bcf1bf0fdb338a934b63891fc9ee110aecad` / `dirty-preview` |
| release input tree SHA-256 | `09BEAD2FCC757E245707A43B246E1A0752CF7D00D33DFCB0A11D72148BD2DE9F` |
| release manifest SHA-256 | `AF3F12EDF88D4CC5ADC09B6C75E14E2343EDC037A0A5BD5B21ADA96D4EDA9915` |
| candidate tree SHA-256 / records | `681B1D4B6662D3464691F38648BBF3E9A87859474819A410BAF8BF010B12C5B9` / `80` |
| DLL SHA-256 | `2C1393058188B7484A45F5CC9ECC9485F6ADE13EAC9CE78A9E4577EF96925C7D` |
| DOCX SHA-256 | `FAF4B503CDD8C278E641D45E516A196F813FC215DBDD289CEF3E904B0109522B`; 검수본과 shipped bytes 동일 |
| PDF SHA-256 / pages | `F0728BA7456BB62C98EBE333858664DBD3641639F64F5DE6A1F19B90EAE7A120` / `34`; 검수본과 shipped bytes 동일 |
| canonical before SHA-256 | `3AE733AF990E0096256E35AB56258640C525A238482D4F510207AD46CA3FC1CA` |
| canonical after SHA-256 | 동일 |
| staging residue | 0 |
| lock residue | 0 |

검증 과정에서 stale canonical manual의 `MANUAL_SDO_WRITE_SCOPE`, example README의
`PREVIEW_PRODUCTION_NO_GO`, PowerShell 5.1 inline Python quoting, pre-lock hash/manifest TOCTOU,
ignored manual의 false-clean provenance, reparse 재검증과 Git metadata race를 각각 fail-closed 또는
정적 리뷰로 발견했다. 최종 경로는 exact-byte snapshot, logical-path fingerprint, staged hash,
prepared metadata와 promotion 전 live 재검증으로 보강했다.

## 9. 남은 작업

1. 대규모 working tree를 목적별로 commit하고 clean checkout에서 같은 gate를 재현한다.
2. reviewed Gate D 후 clean source baseline에서 candidate와 schema 3 manifest를 처음
   생성하고 독립 검토한다.
3. binary-reference 예제 실행과 excluded-file/package hash를 별도 reviewer가 재확인한다.
4. 별도 승인 후에만 candidate를 정식 Distribution/배포 대상으로 승격한다.
5. production 판정은 current PLC download, 안전 승인과 Motion/Power/SDO Write live proof가
   끝날 때까지 계속 NO-GO다.

### 2026-08-12 current 남은 release gate

`39c3e6f`로 artifact ordinal cross-host ordering, schema 3, bounded 8-role toolchain과 host
attestation/promotion re-resolution을 구현했다. Current `1b9be6a`는 ToolchainProvenance를
일곱 번째 mandatory suite로 통합해 PS5.1/PS7 exact `14/14`를 PASS했다. 남은 PC-only gap은
active Python dependency closure에 있는
external exact 5개 `lxml`, `typing_extensions`, `cryptography`, Pillow, cffi의 deterministic
provenance와 promotion drift fence를 묶는 것이다. 실제 로드된 cffi `_cffi_backend`는
포함하고 미로드 `pycparser`와 unrelated `site-packages`는 제외한다.

Current full Distribution은 reviewed Gate D STOP으로 actual EXE, schema 3 manifest 생성,
candidate publish에 도달하지 않았다. 위 PC-only gap과 reviewed Gate D를 모두 닫은 후
clean full Distribution을 실행하고 그때 생성된 schema 3 manifest/candidate를 독립
검토한다. LASAL IDE, PLC, Download/runtime은 이 PC/tooling 작업에서 실행하지 않았다.

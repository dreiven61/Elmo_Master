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
| `LMC_Library/LMC_API/ReleaseManifest.ps1` | schema 2 manifest 생성과 재검증 |
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

## 5. candidate 내용과 검증 순서

1. current SDK와 개발 WPF source를 입력으로 수집한다.
2. 예제 project의 source 항목과 bytes를 current 개발 project와 exact 비교한다.
3. 예제 project는 source `ProjectReference` 대신
   `..\..\01_API\LasalMotionControlLib.dll` binary reference를 사용한다.
4. SDK Debug/Release test, LASAL network/static contract, WPF Debug/Release smoke를 실행한다.
5. SDK Release DLL과 candidate 예제 Debug/Release를 build한다.
6. 검토한 외부 DOCX/PDF의 exact bytes를 staging하고 hash, 최소 구조와 semantic scope를 검사한다.
7. canonical/source/run DLL이 byte-identical인지 확인한다.
8. candidate의 `bin/obj/.vs`만 안전하게 제거한다.
9. schema 2 `RELEASE_MANIFEST.md`를 atomic write하고 즉시 재검증한다.
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

## 7. manifest schema 2

candidate manifest에는 다음 provenance를 필수 기록한다.

- source commit과 dirty/clean state
- release input tree SHA-256
- semantic policy SHA-256과 `PASS` result
- assembly/file/product version
- canonical DLL과 두 replica의 byte identity
- package 전체 파일의 relative path, size와 SHA-256

manifest 자체는 temporary file에 쓴 뒤 rename하며, 생성 직후 같은 입력으로 다시 검증한다.

## 8. 검증 결과

### 단위/정책 회귀

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

### 실제 current input 전체 실행

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
2. clean source baseline에서 candidate와 schema 2 manifest를 다시 만들고 독립 검토한다.
3. binary-reference 예제 실행과 excluded-file/package hash를 별도 reviewer가 재확인한다.
4. 별도 승인 후에만 candidate를 정식 Distribution/배포 대상으로 승격한다.
5. production 판정은 current PLC download, 안전 승인과 Motion/Power/SDO Write live proof가
   끝날 때까지 계속 NO-GO다.

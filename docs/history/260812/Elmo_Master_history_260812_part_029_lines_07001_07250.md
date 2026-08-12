>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "Ordinal|Sort-Object|Get-LmcDistributionOrdinal|Get-LmcOrdinal|Get-LmcReleaseInputFiles|Get-LmcDistributionMonitoredFileSnapshot" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 | Select-Object -First 140`
> - Ran `$p='LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1';$l=Get-Content $p; $l[60..104]`
>
> </details>
>
> 독립 검토도 ordinal-final 후보를 CLEAN으로 판정했습니다. PS5.1/PS7 production snapshot 출력이 byte-for-byte 동일하고, 92 records+digest의 고정 출력 SHA도 일치했습니다. 이제 이 동결본으로 마지막 PS5.1-parent 12-suite aggregate를 다시 실행합니다.
>
> 최종 재실행이 시작됐으며, 이번 결과만 commit 근거로 사용합니다. 동일 4개 파일 해시가 유지되는 동안 다른 편집은 하지 않습니다.
>
> 현재 aggregate는 초반 PS5.1 suite들을 순차 실행 중이며 오류 출력은 없습니다. exact evidence line이 없거나 중복돼도 실패하도록 고정돼 있어 단순 exit 0만으로 통과하지 않습니다.
>
> PS5.1 6/6은 모두 완료됐고 PS7 pipeline 245/245 구간으로 넘어갔습니다. 남은 것은 PS7의 semantic, manifest, size, UDP, HandleRequest입니다.
>
> PS7 쪽 검증이 계속 진행 중입니다. 동시에 겹치지 않는 read-only 문서 계획을 병렬로 준비하고 있으며, 코드나 문서는 아직 수정하지 않습니다.
>
> frozen code와 스테이지 상태는 그대로입니다. PS7의 장기 UDP 단계 전후를 순차 검증 중이며, 최종 terminal 12/12과 ordinal digest가 모두 일치해야만 다음 단계로 갑니다.
>
> 현재까지 failure signal은 없습니다. 정상 경로가 길지만 suite별 900초 상한과 aggregate 외부 감시가 모두 살아 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-20) } | Select-Object Id,ProcessName,StartTime,CPU,Responding | Sort-Object StartTime | Format-Table -AutoSize`
>
> </details>
>
> 프로세스 상태를 확인했고 PS7 UDP worker가 CPU를 사용하며 `Responding=True`로 정상 실행 중입니다. parent PS5.1 aggregate도 유지되고 있으며 중복 aggregate는 없습니다.
>
> 문서 갱신 범위도 read-only로 정리했습니다. 최종 aggregate가 통과하면 과거 `129/192` 수치는 삭제하지 않고 historical로 남기고, current `245/12-suite/92-file` 증거를 별도 현재 항목으로 추가하겠습니다.
>
> PS7 장기 suite가 진행 중이며 frozen 파일 해시는 그대로입니다. 실패·timeout·stderr가 생기면 aggregate를 즉시 무효 처리하도록 유지하고 있습니다.
>
> 현재까지 새 finding은 없습니다. 마지막 aggregate 완료 후에는 독립 reviewer가 output·digest·process residue와 실제 diff를 한 번 더 대조합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-20) } | Select-Object Id,ProcessName,StartTime,CPU,Responding | Sort-Object StartTime | Format-Table -AutoSize`
>
> </details>
>
> 장기 worker 프로세스는 종료됐고 aggregate가 결과·후처리를 정리하는 단계로 보입니다. 최종 terminal과 residue를 받기 전까지는 PASS로 선언하지 않겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime,CPU,Responding | Sort-Object StartTime | Format-Table -AutoSize`
>
> </details>
>
> 최종 결과가 나왔습니다. PS5.1 부모 aggregate는 12/12 PASS, 802.8초였고 terminal은 `files=92 SHA256=99D6D...DC62`로 고정됐습니다. 모든 worker 종료, transaction lock/stage residue 0, 4개 파일 해시 불변입니다. 이제 독립 최종 검토를 받고 exact 4파일만 stage/commit합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$owned=@('LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1','LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1','LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1'); $rows=foreach($file in $owned){$item=Get-Item -LiteralPath $file;[pscustomobject]@{Path=$file;Bytes=$item.Length;SHA256=(Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash}}; $rows|Format-List; git status --short; git diff --check -- $owned; git diff --cached --check; 'CACHED_NAMES'; git diff --cached --name-only; 'RELEVANT_PROCESSES'; Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -match 'Test-LmcDistributionToolingHostParity|LMC_API_Distribution\.transaction\.lock|\.LMC_API_Distribution\.stage' } | Select-Object ProcessId,Name,CommandLine | Format-List`
>
> </details>
>
> 독립 최종 검토도 P0/P1 없음으로 끝났습니다. 지금 검증된 4개 파일만 정확히 stage하고, 사용자 `Classes.lcb`와 미추적 자료는 제외한 채 별도 목적 commit을 생성합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git add -- LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1; git diff --cached --check; git diff --cached --name-status; git diff --cached --stat`
> - Ran `git commit -m "test(distribution): require dual-host tooling preflight"`
> - Ran `git show --stat --oneline --decorate --no-renames HEAD; git diff --check HEAD^ HEAD; git status --short; git diff --cached --check; git diff --cached --name-only`
>
> </details>
>
> 코드 tranche를 `febb1b0`으로 커밋했습니다. 이제 권위 문서 5개에 현재 preflight 계약과 실측 12/12를 반영하되, 기존 full Distribution의 Gate D STOP과 no actual-EXE/manifest/publish/no PLC 경계는 그대로 유지합니다.
>
> 문서 5개 패치와 다음 P0-D toolchain provenance 설계를 서로 겹치지 않는 범위에서 병렬 진행 중입니다. 새 P0-D는 실제 PowerShell/Git/vswhere/MSBuild/compiler/Python 패키지 identity를 transaction과 schema 3 manifest에 묶는 작업이며, full Distribution은 실행하지 않습니다.
>
> P0-D 설계는 확정됐습니다. schema 3에는 toolchain hash뿐 아니라 방금 검증한 host-parity `PASS/12/tooling digest`도 명시적으로 묶고, manifest에는 논리적 역할·버전·SHA만 기록해 사용자 절대 경로가 새지 않게 합니다.
>
> 문서 편집과 P0-D 구현 모두 아직 지정 범위 안에서 진행 중입니다. 사용자 `Classes.lcb`, historical evidence, candidate/manual artifact에는 접근하거나 변경하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short | Select-Object -First 25`
>
> </details>
>
> 문서 4개는 현재 패치 중이고 계획 문서 1개가 남아 있습니다. P0-D 코드는 아직 설계에서 구현으로 넘어가는 단계이며, 두 작업은 파일 충돌 없이 진행되고 있습니다.
>
> 5개 문서 패치가 완료됐습니다. 현재 독립 reviewer가 `febb1b0`의 실측값과 Gate D STOP 경계를 문장별로 대조하고 있으며, 수정된 문서 외 파일은 검토 범위에서 제외합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --stat -- $docs; git diff --check -- $docs; rg -n "febb1b0|99D6D271|245|802\.8|ToolingHostParity|tooling host-parity|tooling preflight|92" $docs`
>
> </details>
>
> 문서 diff는 5개 파일, `+150/-4`, scoped diff-check PASS입니다. 현재 reviewer가 historical `192/129`와 current `245/12-suite`의 시간축, 그리고 `Gate D STOP` 문구를 마지막으로 대조 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short | Select-Object -First 20`
>
> </details>
>
> 문서 reviewer가 정확성 P1 하나를 찾았습니다. 다음 P0-D를 artifact 정렬·toolchain provenance로만 적고 `ReleaseManifest schema 3` 전환을 일부 문서에서 빠뜨렸습니다. 같은 5개 문서 안에서 이 DoD를 명시하고 재검토하겠습니다.
>
> <details><summary>Edited 5 files, ran a command</summary>
>
> - Ran `rg -n -C 2 "P0-D|artifact.*ordering|toolchain|schema 3|schema 2" LMC_Library/LMC_API/README.md docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md | Select-Object -Last 160`
> - Wrote `./LMC_Library/LMC_API/README.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md` (+5 -3)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+9 -7)
>
> </details>
>
> <details><summary>Edited 5 files, ran a command</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $docs; git diff --cached --check; git diff --stat -- $docs; rg -n "schema 3|schema3" $docs`
> - Wrote `./LMC_Library/LMC_API/README.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+3 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md` (+4 -4)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+5 -5)
>
> </details>
>
> 재검토에서 P1 하나가 더 좁혀졌습니다. `cross-host ordering`만으로는 현재 문제인 culture-dependent `Sort-Object`의 교정 기준이 약하므로, 5개 문서 모두 완료조건을 `ordinal cross-host artifact ordering`으로 명시하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md` (+1 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+4 -4)
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $docs; git diff --cached --check; rg -n "ordinal cross-host" $docs; git diff --stat -- $docs`
>
> </details>
>
> 문서 재검토가 CLEAN으로 끝났습니다. 5개 문서만 stage해 별도 documentation commit으로 묶고, 진행 중인 P0-D 코드 파일은 포함하지 않습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git add -- LMC_Library/LMC_API/README.md LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md; git diff --cached --check; git diff --cached --name-status; git diff --cached --stat`
> - Ran `git commit -m "docs(api): record dual-host release preflight"`
> - Ran `git show --stat --oneline --no-renames HEAD; git diff --check HEAD^ HEAD; git status --short | Select-Object -First 30; git diff --cached --check; git diff --cached --name-only`
>
> </details>
>
> 문서도 `701550c`로 별도 커밋했습니다. 바로 다음 P0-D 구현이 시작되어 현재 새 provenance 모듈과 `ReleaseManifest.ps1` schema 3/ordinal 정렬 변경이 작업 중이며, 아직 검증 전이라 commit 대상은 아닙니다.
>
> P0-D는 현재 schema 3 데이터 모델과 8-role identity resolver를 구현 중입니다. 핵심은 runtime 절대 경로를 검증에만 쓰고, manifest에는 역할명·버전·파일 SHA와 attestation hash만 남기는 것입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short | Select-Object -First 20; git diff --stat -- LMC_Library/LMC_API/ReleaseManifest.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1`
>
> </details>
>
> 현재 변경 범위는 새 provenance 구현/테스트 2개와 builder/manifest 2개입니다. 기존 preflight·문서·LASAL 파일과는 겹치지 않으며, manifest schema 변경과 promotion 재검증 wiring까지 1차 패치가 들어간 상태입니다.
>
> P0-D 1차 구현은 실제 PC에서 8개 역할을 해석하는 데 성공했습니다. 선택된 VS2019 Roslyn `csc`, bundled Python과 실제 imported `python-docx`/`pypdf`까지 포함했고, 새 provenance 테스트 25/25와 schema 3 manifest 테스트 89/89가 우선 PS7에서 통과했습니다. 남은 작업은 host-parity 수치 갱신, transaction drift fixture, PS5.1 대조와 독립 review입니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "^(function|param)|Role|ToolchainSha|HostParity|Absolute|Path|csc|python-docx|pypdf|vswhere|MSBuild|Schema" LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -First 220; rg -n "Toolchain|SchemaVersion|Sort-Object|Ordinal" LMC_Library/LMC_API/ReleaseManifest.ps1 | Select-Object -First 180; rg -n "Toolchain|Composite|Fingerprint|SchemaVersion" LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 | Select-Object -First 220`
> - Ran `Get-Content -LiteralPath LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj -TotalCount 120; rg -n "Roslyn|CscTool|LangVersion|TargetFramework" LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj Directory.Build.* -g "*"`
>
> </details>
>
> 정확성 보강도 반영했습니다. `python-docx`와 `pypdf`는 이제 `__init__.py` 한 파일만 해시하지 않고, 설치 metadata의 전체 파일 목록을 ordinal `relative path|length|SHA`로 고정하며 누락·중복·reparse·root escape를 거부합니다. Manifest에는 그 전체 distribution digest만 기록되고 실제 경로는 노출하지 않습니다.
>
> 현재 whole-package fingerprint 변경에 맞춰 tamper/escape/missing fixture를 갱신 중입니다. 이 단계가 통과한 뒤에야 builder transaction과 schema 3 manifest 테스트 수치를 다시 고정합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short | Select-Object -First 18; git diff --stat -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/ReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1`
>
> </details>
>
> 변경 범위는 현재 6개 코드/테스트 파일로 유지됩니다. host-parity suite는 schema 3 manifest의 새 expected count와 provenance 구현 파일 inventory를 반영했고, pipeline transaction fixture는 아직 작성 중입니다.
>
> whole-package 검증이 통과했습니다. 현재 bundle 기준 `python-docx` 221개 파일, `pypdf` 117개 파일을 fingerprint하며 provenance 테스트 28/28, pipeline 255/255가 PASS했습니다. post-populate tool byte 변조도 candidate 없음·canonical 불변·promotion 0회·stage/lock 0으로 fail-closed했습니다. 이제 PS5.1/PS7 전체 focused matrix와 독립 review만 남았습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$p='LMC_Library/LMC_API/DistributionToolchainProvenance.ps1';$l=Get-Content $p; foreach($r in @(@(220,350),@(350,480),@(555,665),@(660,810))){"---$($r[0])-$($r[1])---";$l[($r[0]-1)..([Math]::Min($r[1]-1,$l.Count-1))]}`
> - Ran `rg -n -C 4 "Hosts =|Label =|Sha256|PowerShellHome|Version =" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 | Select-Object -Last 100`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n "ToolingPreflight(FileCount|Result|RunCount|Digest|Host|Sha)" LMC_Library/LMC_API/ReleaseManifest.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionToolchainProvenance.ps1`
>
> </details>
>
> 추가 감사에서 dual-host attestation이 PS5/PS7의 버전만 보존하고 실행파일 SHA를 남기지 않는 공백을 찾았습니다. 이를 즉시 보강해 두 host executable을 physical/non-reparse로 검증하고, manifest에는 경로 없이 `label|edition|major|version|SHA`를 기록합니다. monitored file count도 attestation hash와 schema 3에 함께 고정합니다.
>
> 현재 attestation 5-field host record와 `ToolingFileCount` schema wiring을 테스트에 반영 중입니다. 이 변경 후 이전 28/89/255 수치는 다시 산출하며, 기존 수치를 그대로 재사용하지 않습니다.
>
> 수정은 계속 지정 7파일 범위 안에 있습니다. 현재는 schema 3 renderer/validator와 synthetic attestation fixture가 같은 canonical hash를 계산하는지 정리하는 단계입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-15) } | Select-Object Id,ProcessName,StartTime,CPU,Responding | Sort-Object StartTime | Format-Table -AutoSize`
>
> </details>
>
> PS5.1과 PS7 focused test 프로세스가 둘 다 실행 중이며 `Responding=True`입니다. 결과가 돌아올 때까지 동일 bytes를 유지하고 있습니다.
>
> 요청한 attestation 보강 후 PS5.1 focused matrix가 새 수치로 통과했습니다: provenance 28/28, manifest 94/94, pipeline 258/258. 새 구현·테스트 2개가 monitored inventory에 포함되어 현재 count는 92가 아니라 94이며, 이전 문서의 92는 `febb1b0` 당시 증거로 그대로 유효합니다. PS7 최종 대조 후 current 94-file aggregate를 다시 실행합니다.
>
> PS7 focused rerun이 진행 중입니다. 이 결과가 PS5.1과 exact count/terminal이 같아야 코드 동결과 독립 review로 넘어갑니다.
>
> 현재까지 새 blocker는 없습니다. full Distribution은 실행하지 않고, 변경된 mandatory preflight 자체만 current 94-file snapshot으로 최종 재실행할 계획입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-15) } | Select-Object Id,ProcessName,StartTime,CPU,Responding | Sort-Object StartTime | Format-Table -AutoSize`
>
> </details>
>
> 양쪽 test host는 살아 있고 오류 없이 결과 집계 중입니다. current 소스는 계속 7파일 범위로 동결되어 있습니다.
>
> current bytes의 양쪽 focused matrix가 exact 일치로 끝났습니다: provenance 28/28, manifest 94/94, pipeline 260/260, 7파일 AST 오류 0입니다. Host resolver는 suite 실행 전후 실행파일 SHA도 재검증합니다. 지금 독립 review 중이며 CLEAN이면 current 94-file 12/12 aggregate를 실행합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short | Select-Object -First 25; git diff --stat -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/ReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1; git diff --check -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/ReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1; git diff --cached --check`
>
> </details>
>
> 독립 review가 유효한 P1을 잡았습니다. manifest가 기록한 `csc`는 MSBuild 기본 toolset에서 계산했지만 실제 MSBuild 호출이 `CscToolPath/CscToolExe/RoslynTargetsPath`를 강제하지 않아 외부 override가 다른 compiler를 선택할 수 있었습니다. 실제 build마다 해당 경로를 예약 속성으로 고정하고 `UseSharedCompilation=false`를 적용하며, C# compiler identity도 `csc.exe` 한 파일이 아닌 전체 Roslyn toolset inventory digest로 올립니다. 이전 focused 결과는 무효로 하고 다시 검증합니다.
>
> 현재 compiler pinning과 Roslyn 전체 inventory fixture를 구현 중입니다. 특히 대소문자만 다른 override key도 거부하고, temporary MSBuild property probe가 실제로 attested 경로를 해석하는지 비공허하게 확인하도록 요구했습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$git=(Get-Command git.exe -CommandType Application | Select-Object -First 1).Source; $git; Get-Item $git | Select FullName,Length,VersionInfo; & $git --exec-path; & $git --version --build-options`
> - Ran `$paths=@('C:\Program Files\Git\cmd\git.exe','C:\Program Files\Git\mingw64\bin\git.exe','C:\Program Files\Git\mingw64\libexec\git-core\git.exe'); foreach($p in $paths){if(Test-Path $p){$i=Get-Item $p;[pscustomobject]@{Path=$p;Length=$i.Length;SHA=(Get-FileHash $p -Algorithm SHA256).Hash}}} | Format-List`
> - Ran `$paths=@('C:\Program Files\Git\cmd\git.exe','C:\Program Files\Git\mingw64\bin\git.exe','C:\Program Files\Git\mingw64\libexec\git-core\git.exe');$rows=foreach($p in $paths){if(Test-Path $p){$i=Get-Item $p;[pscustomobject]@{Path=$p;Length=$i.Length;SHA=(Get-FileHash $p -Algorithm SHA256).Hash}}};$rows|Format-List`
>
> </details>
>
> 추가 review에서 두 결속 공백도 함께 닫고 있습니다. 현재 release PowerShell은 attested PS5/PS7 executable SHA 중 하나와 일치해야 하고, Git은 `cmd\git.exe` launcher뿐 아니라 실제 `mingw64\bin\git.exe` core도 묶습니다. 이 PC에서는 두 Git 파일이 서로 다른 SHA이므로 launcher 하나만 기록하면 충분하지 않습니다.
>

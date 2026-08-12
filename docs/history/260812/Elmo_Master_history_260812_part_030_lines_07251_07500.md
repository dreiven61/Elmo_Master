> 현재 compiler/Git/host 결속 보강 후 fixture 재작성 중입니다. 아직 새 PASS 수치는 없으며 이전 28/94/260은 이 변경의 최종 근거로 사용하지 않습니다.
>
> 작업 범위와 안전 경계는 유지 중입니다. full Distribution·candidate·LASAL·PLC는 건드리지 않고, synthetic/temp property probe와 transaction fixture만 사용합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/ReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1; Get-Item LMC_Library/LMC_API/DistributionToolchainProvenance.ps1,LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 | Select Name,Length,LastWriteTime | Format-Table -AutoSize`
> - Ran `Get-Process powershell,pwsh,MSBuild,csc -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-15) } | Select Id,ProcessName,StartTime,CPU,Responding | Sort StartTime | Format-Table -AutoSize`
>
> </details>
>
> compiler pinning은 builder 전 호출부까지 확장돼 diff가 커졌지만, 아직 fixture가 그 변경을 따라잡는 중입니다. 새 module bytes만 먼저 커졌고 테스트는 아직 최종 재실행 전 상태라 계속 동결/검토 중으로 취급합니다.
>
> 현재까지 blocker 보고는 없고, 대규모 compiler property fixture 패치를 마무리하는 중입니다. 안전상 중간 상태에서는 어떤 PASS도 선언하지 않습니다.
>
> 독립 reviewer가 세 P1을 구체적으로 잡았고 모두 수정 중입니다.
>
> - compiler override 가능성 → 5개 Roslyn/Csc 속성 강제, shared compilation 비활성, binding probe
> - 46KB Git launcher만 해시 → 실제 4.3MB `git-core\git.exe`를 해석·사용
> - custom/ISE host 가능성 → invoking host가 preflight의 path/edition/major/version/SHA와 exact 일치하도록 제한
>
> 추가로 Python interpreter도 실행파일 한 개가 아니라 runtime inventory를 묶습니다. 아직 재검증 전입니다.
>
> 현재 builder가 세 P1의 production/test fixture를 함께 맞추고 있습니다. reviewer는 code bytes가 동결될 때까지 최종 판정을 보류 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh,MSBuild,csc -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-10) } | Select Id,ProcessName,StartTime,CPU,Responding | Sort StartTime | Format-Table -AutoSize`
>
> </details>
>
> 아직 test 프로세스는 시작되지 않았고 patch/fixture 정리 단계입니다. 중간 상태의 수치를 재사용하지 않고 새 bytes 기준으로 양 host 검증을 다시 시작합니다.
>
> 세 P1 수정이 구현 단계에서 닫혔습니다. 현재 PS7 provenance 43/43, pipeline 284/284 PASS입니다. 실제 MSBuild 진단 target이 강제된 5개 compiler 속성과 `UseSharedCompilation=false`를 확인했고 residue 0입니다. 전체 10개 MSBuild 호출이 같은 pin을 사용하며, 이제 PS5.1/PS7 manifest 포함 전체 focused matrix를 다시 돌립니다.
>
> 양 host focused matrix가 진행 중입니다. 새 최종 count와 manifest schema 3 byte determinism이 일치해야 독립 재리뷰로 넘어갑니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh,MSBuild,csc -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-15) } | Select Id,ProcessName,StartTime,CPU,Responding | Sort StartTime | Format-Table -AutoSize`
>
> </details>
>
> focused 실행 후 fixture/수치 동결을 정리 중입니다. 아직 최종 CLEAN 신호가 없으므로 stage나 commit은 하지 않습니다.
>
> 현재 양 host provenance 43/43, manifest 94/94, PS7 pipeline 284/284가 통과했습니다. compiler binding probe는 standalone이 아니라 실제 4개 C# project의 evaluated properties를 확인하도록 강화됐고 PS7 재통과했습니다. 마지막 강화본의 PS5.1 pipeline과 frozen review가 남았습니다.
>
> PS5.1 final pipeline 재실행 중입니다. 이 결과가 284/284로 맞으면 7파일을 동결해 reviewer가 production wiring과 fixture를 최종 대조합니다.
>
> 아직 final pipeline 결과 집계 중이며 timeout이나 failure signal은 없습니다. 기존 user dirty와 candidate/manual 파일은 그대로 보존되고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh,MSBuild,csc -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-15) } | Select Id,ProcessName,StartTime,CPU,Responding | Sort StartTime | Format-Table -AutoSize`
>
> </details>
>
> 최종 focused 동결 결과가 나왔습니다: PS5.1/PS7 provenance 44/44, manifest 94/94, pipeline 284/284, AST 7/7 오류 0, diff-check PASS, residue 0입니다. reviewer가 exact 7-file SHA로 최종 reread 중이며 CLEAN이면 current 94-file 12/12 aggregate를 실행합니다.
>
> 독립 reviewer가 frozen 7파일의 production 호출부와 test fixture를 줄 단위로 대조 중입니다. 아직 추가 finding은 없지만 최종 CLEAN 전에는 장시간 aggregate를 시작하지 않습니다.
>
> 독립 review가 P0/P1 CLEAN으로 끝났습니다. compiler/Git/PowerShell/Python binding, schema 3 ordinal path-free records, promotion 직전 재해석과 cleanup fixture가 모두 확인됐습니다. 이제 frozen 7파일로 PS5.1-parent current 12/12 aggregate를 시작합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/DistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/ReleaseManifest.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1','LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1');$rows=foreach($f in $files){$i=Get-Item $f;[pscustomobject]@{Path=$f;Bytes=$i.Length;Sha256=(Get-FileHash $f -Algorithm SHA256).Hash}};$rows|Format-List; git diff --check -- $files; git diff --cached --check`
>
> </details>
>
> 최종 7파일 hash를 별도로 고정했고 aggregate가 진행 중입니다. 이 실행은 current 94-file snapshot과 schema 3 test count를 사용하는 첫 12/12 증거입니다.
>
> 초반 PS5.1 suite들이 순차 실행 중이며 현재 오류 출력은 없습니다. toolchain resolver가 각 suite 전후 host executable SHA와 monitored source digest를 재검증합니다.
>
> aggregate는 정상 범위에서 진행 중입니다. 긴 UDP 296/296 단계가 포함돼 전체 예상 시간은 이전과 비슷한 약 13분입니다.
>
> frozen source와 stage 상태는 그대로이며 timeout/failure 신호는 없습니다. final terminal과 attestation digest가 나오기 전에는 PASS로 처리하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-20) } | Select Id,ProcessName,StartTime,CPU,Responding | Sort StartTime | Format-Table -AutoSize`
>
> </details>
>
> 프로세스 상태상 PS5.1 6개는 끝났고 현재 PS7의 장기 UDP worker가 CPU를 사용하며 `Responding=True`입니다. 이후 HandleRequest 하나만 남습니다.
>
> PS7 UDP는 계속 정상 실행 중이며 900초 상한 안입니다. 소스·해시 변경은 없습니다.
>
> 아직 장기 UDP fixture 단계입니다. 종료 뒤 exact 296 terminal과 outer worker terminal을 모두 확인해야 다음 suite로 넘어갑니다.
>
> 현재까지 failure/stderr/timeout 신호는 없습니다. final aggregate는 frozen 7파일 기준으로만 평가합니다.
>
> PS7 UDP가 예상 실측 범위에서 계속 진행 중입니다. checked-in timeout kill과 외부 감시가 모두 유지되고 있습니다.
>
> 변경 없음: 장기 fixture는 정상이고, 코드·문서·stage에는 새 변경이 없습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-20) } | Select Id,ProcessName,StartTime,CPU,Responding | Sort StartTime | Format-Table -AutoSize`
>
> </details>
>
> 장기 worker와 aggregate parent는 종료됐고 현재 결과·residue 정리 단계로 보입니다. terminal 검증을 받기 전까지 최종 PASS는 보류합니다.
>
> P0-D 최종 acceptance가 통과했습니다. PS5.1 부모 12/12, 808.553초, current `files=94`, digest `C25A...F543`입니다. path-free dual-host attestation SHA, 8-role toolchain SHA, full Roslyn/Python/docx/pypdf inventory도 고정됐고 residue 0입니다. 이제 root에서 7파일 hash·diff를 재확인한 뒤 exact scope만 commit합니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/DistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/ReleaseManifest.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1','LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1');$rows=foreach($f in $files){$i=Get-Item -LiteralPath $f;[pscustomobject]@{Path=$f;Bytes=$i.Length;Lines=(Get-Content -LiteralPath $f).Count;Sha256=(Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash}};$rows|Format-List; git diff --check -- $files; git diff --cached --check; git status --short | Select-Object -First 30`
> - Ran `git add -- LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/ReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1; git diff --cached --check; git diff --cached --name-status; git diff --cached --stat`
> - Ran `git commit -m "feat(distribution): bind release toolchain provenance"`
> - Ran `git show --stat --oneline --no-renames HEAD; git diff --check HEAD^ HEAD; git status --short | Select-Object -First 30; git diff --cached --check; git diff --cached --name-only`
>
> </details>
>
> P0-D 코드를 `39c3e6f`로 커밋했습니다. 이제 5개 권위 문서에서 schema 3/toolchain provenance를 “다음 gap”이 아니라 완료된 PC/tooling 증거로 전환하고, 동시에 이후의 다음 PC-only gap을 read-only로 다시 선별합니다.
>
> 문서에는 `febb1b0`의 92-file predecessor와 `39c3e6f`의 94-file schema 3 결과를 시간축으로 분리해 기록하고 있습니다. 새 code가 candidate manifest를 실제 생성한 것은 아니므로, “schema 3 구현·self-test PASS”와 “full Distribution manifest 미생성”도 명확히 나눕니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short | Select-Object -First 16`
>
> </details>
>
> 현재 worktree는 다시 사용자 기존 dirty만 남은 상태이고, 문서 agent는 아직 read/patch 준비 단계입니다. next-gap audit도 코드 변경 없이 설계 공백을 좁히고 있습니다.
>
> 문서와 next-gap audit에 중간 상태를 요청했습니다. 둘 다 아직 코드나 사용자 파일을 추가로 변경하지 않았습니다.
>
> next-gap audit가 바로 후속 P1을 찾았습니다. schema 3은 Python base runtime과 `python-docx`/`pypdf` 자체는 묶지만, 실제 import dependency인 `lxml`, `typing_extensions`, `cryptography`, `Pillow`는 제외합니다. 이 바이트가 바뀌어도 현재 Toolchain SHA가 유지될 수 있으므로, 문서에는 39c3e6f 범위를 좁게 완료로 기록하고 dependency closure를 다음 PC-only gap으로 남깁니다.
>
> 더 높은 필수 공백도 확인됐습니다. 새 provenance 44/44 테스트는 양 host에서 따로 실행했지만 mandatory preflight의 6-suite allowlist에는 들어 있지 않습니다. 즉 미래에 그 테스트가 깨져도 12/12 gate가 통과할 수 있습니다. 이를 7-suite/host, 14/14 mandatory gate로 올리고 attestation/schema 3 계약까지 맞추는 별도 보정 tranche를 바로 시작합니다.
>
> 다음 P0-D 보정 구현을 시작했습니다. 현재 `39c3e6f`의 44/44는 양 host에서 따로 통과했지만 mandatory gate 내부 suite는 아니므로, 정확히는 “구현·focused PASS”이고 “mandatory provenance gate 완료”는 아닙니다. 문서도 이 차이를 반영하고, 코드는 7-suite/host `14/14`로 올립니다. 그 다음이 Python transitive dependency closure입니다.
>
> 14/14 보정은 기존 allowlist/spec/worker/attestation/manifest/builder/test 수치를 함께 바꾸는 좁은 tranche입니다. full Distribution이나 dependency closure는 섞지 않습니다.
>
> 새 seventh suite는 독립 실행 실측이 PS5.1 11.4초, PS7 18.6초라 timeout 180초로 고정합니다. 코드 범위는 기존 7파일 안이며, 누락·대체·중복·nonzero/evidence drift와 구형 6/12·7/13 attestation 거부를 추가합니다.
>
> schema 3 문서 패치는 5개 파일 `+275/-60`으로 동결됐습니다. 현재 독립 reviewer가 12/12 mandatory와 별도 44/44 focused를 혼동하지 않았는지, 다음 14/14·dependency closure 순서가 정확한지 검토 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $docs; git diff --stat -- $docs; rg -n "39c3e6f|14/14|44/44|C25A6105|A83A0382|9EC464FA|lxml|typing_extensions|cryptography|Pillow|schema 3 manifest" $docs`
>
> </details>
>
> 문서 diff는 현재 `39c3e6f`의 정확한 한계를 명시합니다: 94-file 12/12는 provenance test를 감시만 했고, 44/44는 별도 실행입니다. reviewer가 이 시간축과 “현재 schema 3 manifest 미생성” 경계를 검토 중입니다.
>
> 14/14 code patch는 아직 fixture/계약 동기화 단계입니다. 문서 reviewer는 현재까지 blocker 없이 마지막 stale 문구 검사를 진행 중입니다.
>
> 14/14 보정의 focused matrix가 양 host에서 통과했습니다: Toolchain 49/49, Manifest 100/100, Pipeline 286/286, AST/diff-check CLEAN입니다. 아직 독립 code review와 최종 14/14 aggregate 전이므로 commit하지 않습니다. 문서는 14/14 완료 뒤 current 상태로 한 번 더 보정해 묶겠습니다.
>
> 39c3e6f 문서 diff는 CLEAN으로 검증됐습니다. 같은 reviewer가 이제 14/14 code diff를 별도 read-only로 검토 중이며, 문서 commit은 새 code 결과에 맞춰 한 번 더 current 상태로 보정한 뒤 진행합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/ReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1; rg -n "14/14|SuiteCount.?7|RunCount.?14|ToolchainProvenance|49 distribution|100/100|286" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | Select-Object -First 220`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 진행 중인 건은 배포 전 필수 검사에 `ToolchainProvenance`를 7번째 suite로 넣어 14/14로 고정하는 작업입니다. 집중 테스트는 양 PowerShell에서 통과했고, 지금 독립 코드 리뷰 결과를 받은 뒤 동결된 바이트로 최종 14/14 장시간 검사를 실행하겠습니다. 기존 사용자 LASAL 파일과 PLC/IDE는 건드리지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git diff --name-only; git log -5 --oneline`
>
> </details>
>
> 독립 리뷰는 현재 7파일 diff를 P0/P1 없음으로 판정했습니다. `ToolchainProvenance`가 실제로 PS5/PS7 각각 별도 프로세스로 실행되고, 7 suite/host·14/14 계약과 구형 6/12·7/13 거부까지 확인됐습니다. 이제 파일을 동결한 채 Windows PowerShell 5.1 부모에서 최종 14/14를 돌립니다. 약 14분 걸리는 검사입니다.
>
> 14/14 장시간 검사는 현재 계속 진행 중이며, 파일은 동결 상태입니다. 먼저 PS5의 7개 suite를 모두 끝낸 뒤 PS7의 7개 suite로 넘어가는 순차 실행이라 시간이 걸립니다. 종료 결과가 나오기 전에는 PASS로 간주하지 않겠습니다.
>
> 진행상태를 확인했습니다. PS5는 7/7을 이미 마쳤고, PS7도 Pipeline·SemanticPolicy·ReleaseManifest를 통과해 현재 새 `ToolchainProvenance` suite를 실행 중입니다. 이후 PS7의 MethodSize·UDP callback·ControlHandleRequest가 남았습니다.
>
> 새 suite와 MethodSize까지 통과했고, 지금 PS7의 가장 긴 `UdpCallback` 296/296 검사가 실행 중입니다. 이것이 끝나면 ControlHandleRequest 하나만 남습니다. 현재까지 비정상 종료나 파일 변경은 없습니다.
>
> `UdpCallback` 검사는 정상 범위에서 계속 돌고 있습니다. 이 검사는 이전 실측상 PS7에서 약 8분 걸리므로 무출력 자체는 이상 징후가 아닙니다. 900초 제한과 프로세스 종료 검사가 걸려 있어 멈춤이면 실패로 닫힙니다.
>
> 아직 PS7 UDP 정적 계약 검증 구간입니다. 결과 수치가 나오기 전에는 다음 단계로 넘어가지 않고 있으며, 테스트 중 파일 편집·스테이징·배포 후보 생성은 없습니다.
>
> PS7 UDP worker를 2초 간격으로 확인했고 CPU가 약 2초 증가했으며 `Responding=True`입니다. 멈춘 것이 아니라 실제 계산 중입니다. 메모리도 약 262MB로 안정적이고, 동결 상태를 유지합니다.
>
> 장시간 UDP 검증은 계속 진행 중입니다. 현재 단계는 PC 정적 verifier self-test이며 PLC 통신이나 네트워크 접속은 전혀 없습니다. 완료되면 정확한 296/296 근거와 마지막 Control 검사까지 한 번에 확인합니다.
>
> 아직 같은 UDP 검증 구간이며 타임아웃 한도 안입니다. 이 단계의 이전 정상 소요가 약 8분이어서 현재 경과는 예상 범위입니다. 실패·중단 신호는 없습니다.
>
> 최종 장시간 검사는 방금 성공했습니다: Windows PowerShell 5.1 부모에서 `14/14 (PS5=7/7; PS7=7/7)`, exit 0, 약 13분 51초입니다. 이제 같은 동결 바이트의 전체 digest·attestation·toolchain SHA와 잔류 프로세스/스테이지/락 0을 수집한 뒤 정확한 7파일만 커밋하겠습니다.
>
> PASS 자체는 확보됐고, 현재는 커밋 전 증거 고정 단계입니다. 테스트가 본 94개 파일의 digest와 7개 변경 파일 SHA가 실행 전후 동일한지, worker·stage·transaction lock 잔류가 없는지를 확인하고 있습니다.
>
> 최종 증거가 고정됐습니다. 14/14 외에 attestation `CE3D…4B36`, toolchain `C321…5A18`, 프로세스·stage·lock 잔류 0까지 확인됐습니다. 제가 지금 7파일 SHA와 diff를 독립 재확인한 후 그 7개만 스테이징·커밋합니다. 문서 5개와 사용자 `Classes.lcb`는 스테이지에서 제외합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$files = @(
> 'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1',
> 'LMC_Library/LMC_API/DistributionToolchainProvenance.ps1',
> 'LMC_Library/LMC_API/ReleaseManifest.ps1',
> 'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1',
> 'LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1',
> 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1',
> 'LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1'
> )
> Get-FileHash -Algorithm SHA256 $files | ForEach-Object { '{0}  {1}' -f $_.Hash, $_.Path }
> git diff --check -- $files
> git diff --cached --check -- $files
> git diff --stat -- $files`
> - Ran `git add -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/ReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1
> git diff --cached --name-only
> git diff --cached --check
> git status --short`
> - Ran `git commit -m "fix(distribution): require provenance in dual-host gate"; git show --stat --oneline --summary HEAD; git diff-tree --no-commit-id --name-only -r HEAD; git status --short`
>
> </details>
>
> 코드 커밋 `1b9be6a`를 정확히 7파일로 만들었습니다. 사용자 `Classes.lcb`와 기존 미추적 자료는 그대로 남아 있습니다. 이제 이미 작성된 5개 문서를 새 현재값인 7 suite/host·14/14로 보정하고, 이전 `39c3e6f`의 12/12는 역사적 선행 증거로 분리하겠습니다.
>
> 문서 보정과 다음 설계 공백 감사도 병렬로 시작했습니다. 문서 쪽은 새 `1b9be6a`의 14/14를 현재 증거로 바꾸고 있으며, 별도 감사에서는 `python-docx`·`pypdf`가 실제 사용하는 `lxml`·`typing_extensions`·`cryptography`·`Pillow`만 결정적으로 묶는 방법을 확인합니다. 전체 `site-packages`를 무작정 해시하지는 않습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "site-packages|PythonDocx|PyPdf|typing_extensions|cryptography|Pillow|lxml|DistributionFiles|ToolchainSha|Get-LmcPython" LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 LMC_Library/LMC_API/DistributionSemanticPolicy.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
> - Ran `Get-Content LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -Skip 930 -First 155; Get-Content LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1 | Select-Object -Skip 360 -First 335`
> - Ran `rg -n "PythonCandidatePaths|bundled.*Python|python.exe|load_workspace" LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/*.ps1`
>
> </details>
>
> 문서 보정은 진행 중이고, 다음 구현의 핵심 경계도 확인됐습니다. 현재 Python runtime 해시는 `site-packages` 전체를 의도적으로 제외하며 `python-docx`와 `pypdf` 자체 배포 파일만 묶습니다. 따라서 실제 실행 시 로드되는 하위 배포판을 별도 “활성 closure”로 증명·재검증해야 합니다.
>
> 문서 5개 중 README와 transactional design은 새 14/14 기준으로 보정됐고, 나머지 자동시험 기록·현재 아키텍처·개발계획을 수정 중입니다. 약 15분 안에 범위와 문구를 동결해 독립 검토로 넘길 예정입니다.
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `$py = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; if (-not (Test-Path -LiteralPath $py -PathType Leaf)) { throw "Bundled Python missing: $py" }; & $py -c "import importlib.metadata as m, json; names=['python-docx','pypdf','lxml','typing_extensions','cryptography','Pillow']; print(json.dumps({n:{'version':m.version(n),'requires':m.requires(n)} for n in names},indent=2))"`
> - Ran `$py = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -c "import sys, importlib.metadata as m; import docx,pypdf; pkgs=m.packages_distributions(); loaded={d for name in sys.modules for d in (pkgs.get(name.split('.')[0]) or [])}; print('\n'.join(sorted(loaded, key=str.casefold)))"`
> - Ran `rg -n "import (docx|pypdf)|from (docx|pypdf)|python-docx|pypdf" LMC_Library/LMC_API --glob '*.ps1' --glob '*.py'`
> - Ran `$py = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -c "import sys, importlib.metadata as m; from docx import Document; from pypdf import PdfReader; mp=m.packages_distributions(); rows=[]; [rows.append((n,','.join(mp.get(n.split('.')[0]) or []))) for n in sys.modules if mp.get(n.split('.')[0])]; print('\n'.join(f'{a}|{b}' for a,b in sorted(rows)))"`
> - Ran `$py = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'; & $py -c "import sys, importlib.metadata as m; baseline=set(sys.modules); from docx import Document; from pypdf import PdfReader; mp=m.packages_distributions(); roots={'python-docx','pypdf'}; out=sorted({d for name in set(sys.modules)-baseline for d in (mp.get(name.split('.')[0]) or [])}-roots,key=str.casefold); print('\n'.join(out))"`
> - Ran `Get-Content LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 | Select-Object -Skip 620 -First 185`
>
> </details>
>
> 로컬 실측으로 활성 의존성 경계를 더 좁혔습니다. 깨끗한 Python 시작 상태와 비교해 `docx`·`pypdf` import가 새로 적재한 외부 배포판은 정확히 `lxml`, `typing_extensions`, `cryptography`, `Pillow`, `cffi` 5개입니다. `setuptools`는 시작 전부터 적재돼 있어 활성 closure에서 제외하는 것이 맞습니다.
>
> <details><summary>Ran 3 commands</summary>

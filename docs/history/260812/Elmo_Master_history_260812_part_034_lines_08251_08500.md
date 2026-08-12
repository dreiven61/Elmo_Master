> 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'
> )
> $actual=@(git diff --cached --name-only)
> if($actual.Count-ne$expected.Count){throw "staged count drift: $($actual.Count)"}
> for($i=0;$i-lt$expected.Count;$i++){if($actual[$i]-cne$expected[$i]){throw "staged path drift at ${i}: $($actual[$i])"}}
> git diff --cached --check
> if($LASTEXITCODE-ne0){throw 'cached diff check failed'}
> git commit -m 'docs(api): promote reviewed 2.3 manuals'
> if($LASTEXITCODE-ne0){throw "git commit failed: $LASTEXITCODE"}
> git show --stat --oneline --summary HEAD
> git status --short -- $expected`
> - Ran `rg -n "function (Get|Resolve)-LmcDistributionManual|Get-LmcDistributionManualWorktreeState" 'LMC_Library/LMC_API/DistributionPipeline.ps1' 'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'`
> - Ran `$p='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\DistributionPipeline.ps1'; $lines=Get-Content -LiteralPath $p; for($i=0;$i -le 165;$i++){ '{0,5}: {1}' -f ($i+1),$lines[$i] }`
> - Ran `$p='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Build-LmcApiDistribution.ps1'; $lines=Get-Content -LiteralPath $p; for($i=1075;$i -le 1130;$i++){ '{0,5}: {1}' -f ($i+1),$lines[$i] }`
> - Ran `$ErrorActionPreference='Stop'
> $repo='C:\work\Elmo\Elmo_Master'
> $verify='C:\work\Elmo\Elmo_Master_manual_verify_bcc6a9c'
> if(Test-Path -LiteralPath $verify){throw "verify path already exists: $verify"}
> git -C $repo worktree add --detach $verify bcc6a9c
> if($LASTEXITCODE-ne0){throw 'git worktree add failed'}
> try{
>   $status=@(git -C $verify status --porcelain=v1 --untracked-files=all)
>   "DETACHED_STATUS_COUNT=$($status.Count)"
>   if($status.Count-ne0){$status;throw 'detached worktree is not clean'}
>   $docx=Join-Path $verify 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
>   $pdf=Join-Path $verify 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
>   foreach($p in @($docx,$pdf)){$i=Get-Item -LiteralPath $p;$h=(Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash;'{0}|{1}|{2}' -f $i.Name,$i.Length,$h}
>   $ps5=Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe'
>   $probe=@'
> $ErrorActionPreference='Stop'
> . (Join-Path $args[0] 'LMC_Library\LMC_API\DistributionPipeline.ps1')
> . (Join-Path $args[0] 'LMC_Library\LMC_API\DistributionSemanticPolicy.ps1')
> $docx=Join-Path $args[0] 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
> $pdf=Join-Path $args[0] 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
> $resolved=Resolve-LmcDistributionManualInputs -RepositoryRoot $args[0] -CanonicalPdfPath $pdf -CanonicalDocxPath $docx
> $state=Get-LmcDistributionManualWorktreeState -UsesCanonicalInputs $resolved.UsesCanonicalInputs -WorktreeState clean
> $python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
> $dt=Get-LmcDistributionPolicyDocumentText -Path $resolved.DocxPath -PythonPath $python
> $pt=Get-LmcDistributionPolicyDocumentText -Path $resolved.PdfPath -PythonPath $python
> $policy=Test-LmcDistributionManualReleasePolicy -DocxText $dt -PdfText $pt
> 'RESOLVER_CANONICAL={0}' -f $resolved.UsesCanonicalInputs
> 'MANUAL_WORKTREE_STATE={0}' -f $state
> 'MANUAL_POLICY={0}/{1}' -f $policy.Result,$policy.CheckCount
> '@
>   & $ps5 -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -Command $probe $verify
>   if($LASTEXITCODE-ne0){throw 'detached manual probe failed'}
>   "DETACHED_HEAD=$(git -C $verify rev-parse --short HEAD)"
> }
> finally{
>   $resolvedVerify=(Resolve-Path -LiteralPath $verify -ErrorAction SilentlyContinue).Path
>   if($resolvedVerify){
>     if(-not $resolvedVerify.StartsWith('C:\work\Elmo\',[StringComparison]::OrdinalIgnoreCase)){throw "unsafe worktree removal target: $resolvedVerify"}
>     git -C $repo worktree remove --force $resolvedVerify
>     if($LASTEXITCODE-ne0){throw 'git worktree remove failed'}
>   }
> }
> "VERIFY_PATH_EXISTS_AFTER=$([bool](Test-Path -LiteralPath $verify))"`
> - Ran `"VERIFY_PATH_EXISTS=$([bool](Test-Path -LiteralPath 'C:\work\Elmo\Elmo_Master_manual_verify_bcc6a9c'))"; git worktree list --porcelain | Select-String 'manual_verify_bcc6a9c' -Context 1,1`
>
> </details>
>
> 전용 커밋 `bcc6a9c`는 완료됐습니다. 첫 clean-detached 점검은 작업트리 자체와 해시는 맞았지만, 검증용 중첩 PowerShell 인자 인용이 실패했습니다. 작업트리는 즉시 제거돼 잔여 0이며, 같은 commit을 더 단순한 직접 함수 호출로 다시 검증합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $repo='C:\work\Elmo\Elmo_Master'
> $verify='C:\work\Elmo\Elmo_Master_manual_verify_bcc6a9c'
> if(Test-Path -LiteralPath $verify){throw "verify path already exists: $verify"}
> git -C $repo worktree add --detach $verify bcc6a9c
> if($LASTEXITCODE-ne0){throw 'git worktree add failed'}
> try{
>   $status=@(git -C $verify status --porcelain=v1 --untracked-files=all)
>   "DETACHED_STATUS_COUNT=$($status.Count)"
>   if($status.Count-ne0){$status;throw 'detached worktree is not clean'}
>   . (Join-Path $verify 'LMC_Library\LMC_API\DistributionPipeline.ps1')
>   . (Join-Path $verify 'LMC_Library\LMC_API\DistributionSemanticPolicy.ps1')
>   $docx=Join-Path $verify 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
>   $pdf=Join-Path $verify 'LMC_Library\LMC_API_Distribution\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
>   $resolved=Resolve-LmcDistributionManualInputs -RepositoryRoot $verify -CanonicalPdfPath $pdf -CanonicalDocxPath $docx
>   $state=Get-LmcDistributionManualWorktreeState -UsesCanonicalInputs $resolved.UsesCanonicalInputs -WorktreeState clean
>   $python='C:\Users\dreiv\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
>   $dt=Get-LmcDistributionPolicyDocumentText -Path $resolved.DocxPath -PythonPath $python
>   $pt=Get-LmcDistributionPolicyDocumentText -Path $resolved.PdfPath -PythonPath $python
>   $policy=Test-LmcDistributionManualReleasePolicy -DocxText $dt -PdfText $pt
>   "RESOLVER_CANONICAL=$($resolved.UsesCanonicalInputs)"
>   "MANUAL_WORKTREE_STATE=$state"
>   "MANUAL_POLICY=$($policy.Result)/$($policy.CheckCount)"
>   foreach($p in @($docx,$pdf)){$i=Get-Item -LiteralPath $p;$h=(Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash;'{0}|{1}|{2}' -f $i.Name,$i.Length,$h}
>   "DETACHED_HEAD=$(git -C $verify rev-parse --short HEAD)"
> }
> finally{
>   $resolvedVerify=(Resolve-Path -LiteralPath $verify -ErrorAction SilentlyContinue).Path
>   if($resolvedVerify){
>     if(-not $resolvedVerify.StartsWith('C:\work\Elmo\',[StringComparison]::OrdinalIgnoreCase)){throw "unsafe worktree removal target: $resolvedVerify"}
>     git -C $repo worktree remove --force $resolvedVerify
>     if($LASTEXITCODE-ne0){throw 'git worktree remove failed'}
>   }
> }
> "VERIFY_PATH_EXISTS_AFTER=$([bool](Test-Path -LiteralPath $verify))"`
> - Ran `$ErrorActionPreference='Stop'
> $root='C:\work\Elmo\Elmo_Master\tmp'
> $targets=@(
> 'C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_3c63dea',
> 'C:\work\Elmo\Elmo_Master\tmp\pdfs\manual_promotion_20260812',
> 'C:\work\Elmo\Elmo_Master\tmp\docs\manual_promotion_20260812'
> )
> foreach($target in $targets){
>   $resolved=(Resolve-Path -LiteralPath $target -ErrorAction Stop).Path
>   if(-not $resolved.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw "unsafe temp cleanup target: $resolved"}
>   if((Split-Path -Leaf $resolved) -notin @('manual_promotion_3c63dea','manual_promotion_20260812')){throw "unexpected cleanup leaf: $resolved"}
>   "REMOVING_TEMP=$resolved"
>   Remove-Item -LiteralPath $resolved -Recurse -Force
> }
> foreach($target in $targets){"EXISTS_AFTER=$([bool](Test-Path -LiteralPath $target))|$target"}`
>
> </details>
>
> `bcc6a9c` clean detached 검증은 canonical resolver 선택, `WorktreeState=clean`, manual policy 3/3, 두 해시 모두 PASS했습니다. 검증용 worktree는 제거해 잔여 0입니다. 렌더용 임시 폴더 3개만 삭제 정책에 막혀 그대로 두었고, 다른 Temp나 사용자 파일은 건드리지 않았습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- 'LMC_Library/LMC_API_Distribution/README.md' 'LMC_Library/LMC_API_Distribution/02_Example_Program/README.md' 'LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1'; git diff --stat -- 'LMC_Library/LMC_API_Distribution/README.md' 'LMC_Library/LMC_API_Distribution/02_Example_Program/README.md' 'LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1'`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff -- 'LMC_Library/LMC_API_Distribution/README.md' 'LMC_Library/LMC_API_Distribution/02_Example_Program/README.md' 'LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1'`
>
> </details>
>
> canonical README 변경은 현재 3파일로 동결됐고, PS5.1/PS7에서 Semantic 53/53·policy 18/18, Pipeline 291/291, Toolchain 84/84가 모두 PASS했습니다. production 빌드 템플릿과 배포 로직은 건드리지 않았으며, 마지막 독립 diff 검토만 기다립니다.
>
> canonical README 수정은 독립 리뷰까지 CLEAN입니다. 오래된 “승인 target 0개/매뉴얼 1.0”을 제거하고 Axis 1 UI[24] four-ticket 경계, production 비승인 문구를 맞췄으며 실제 production 템플릿을 사용하는 회귀 테스트도 추가했습니다. 이 3파일을 별도 커밋합니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $files=@(
> 'LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1',
> 'LMC_Library/LMC_API_Distribution/02_Example_Program/README.md',
> 'LMC_Library/LMC_API_Distribution/README.md'
> )
> $pre=@(git diff --cached --name-only)
> if($pre.Count-ne0){throw "unexpected preexisting staged files: $($pre -join ', ')"}
> git diff --check -- $files
> if($LASTEXITCODE-ne0){throw 'scoped diff check failed'}
> git add -- $files
> if($LASTEXITCODE-ne0){throw 'git add failed'}
> $actual=@(git diff --cached --name-only)
> if($actual.Count-ne$files.Count){throw "staged count drift: $($actual.Count)"}
> foreach($f in $files){if($actual -cnotcontains $f){throw "missing staged path: $f"}}
> git diff --cached --check
> if($LASTEXITCODE-ne0){throw 'cached diff check failed'}
> git commit -m 'test(distribution): bind preview README policy'
> if($LASTEXITCODE-ne0){throw "git commit failed: $LASTEXITCODE"}
> git show --stat --oneline --summary HEAD`
> - Ran `git status --short --untracked-files=no`
> - Ran `Get-Item -LiteralPath 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md','LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html' -ErrorAction SilentlyContinue | Select-Object FullName,Length,LastWriteTime`
> - Ran `rg -n "API_DEVELOPMENT_GUIDE\.html|API_DEVELOPMENT_PLAN_2026-07-30\.html|pandoc|markdown.*html|Convert.*Html" . --glob '!docs/history/**' --glob '!bin/**' --glob '!obj/**' | Select-Object -First 200`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git log --oneline -8; git status --short --untracked-files=no`
>
> </details>
>
> 매뉴얼 승격과 README 회귀 수정은 각각 `bcc6a9c`, `f304e8b`로 분리했습니다. 현재는 8개 문서/HTML 동기화와 Gate D ratchet의 정확한 승인 대상·해시를 병렬 감사 중입니다. 아직 full Distribution이나 LASAL/PLC 작업은 실행하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short --untracked-files=no; git diff --stat -- 'LMC_Library/LMC_API/README.md' 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md' 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html' 'LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md' 'docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md' 'docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html'`
>
> </details>
>
> 문서 동기화는 6개 Markdown까지 반영됐고, 현재 Pandoc HTML 재생성·stale 문구 검사를 진행 중입니다. Gate D 감사도 current verifier와 physical tuple을 대조 중이며 아직 승인하거나 파일을 바꾸지는 않았습니다.
>
> Gate D의 핵심이 좁혀졌습니다. 메인 작업트리의 사용자 `Classes.lcb` 해시 `13EA…`는 계약 비교가 `REJECTED_BOUNDARY_OR_CONTRACT_DRIFT`라 승인하면 안 됩니다. 반면 clean tracked `24402…` physical tuple은 기존 sanctioned checkpoint와 일치합니다. 따라서 사용자 바이너리는 그대로 두고, clean tracked tuple만 승인하는 verifier 경계 변경이 가능한지 마지막 fixture 검증 중입니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$p='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $ranges=@(@(230,330),@(7310,7380),@(11500,11680)); $lines=Get-Content -LiteralPath $p; foreach($range in $ranges){"=== $($range[0]+1)-$($range[1]+1) ==="; for($i=$range[0];$i-le$range[1];$i++){ '{0,6}: {1}' -f ($i+1),$lines[$i] }}`
> - Ran `rg -n "NeedsRebaseline|TerminalWakeBrokerCandidate.*approved|physical snapshot ratchet|AllowDerivedCapture" 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(7390,7445),@(11190,11250),@(13740,13810))){"=== $($range[0]+1)-$($range[1]+1) ===";for($i=$range[0];$i-le$range[1];$i++){'{0,6}: {1}'-f($i+1),$lines[$i]}}`
>
> </details>
>
> Gate D 코드 경계도 확인했습니다. 현재 모든 “derived” 상태를 일괄 미승인 처리하는 두 boolean 때문에, 이미 exact tuple이 고정된 `TerminalWakeBrokerCandidate`도 계속 CAPTURE로 남습니다. 최종 감사가 CLEAN이면 이 상태 하나만 PASS로 승격하고, 다른 derived 상태와 사용자 dirty `13EA…`는 계속 실패하도록 음성 fixture를 고정하겠습니다.
>
> Gate D 감사 결과, exact `24402…` tracked physical tuple만 승인하는 1파일 변경이 정당하다고 판정됐습니다. `13EA…` 사용자 dirty 바이너리는 90바이트·57구간·35 owner drift로 계속 거부합니다. 지금 verifier 승인 boolean만 좁게 바꾸고 PS5/PS7 296/296, clean detached PASS, main dirty FAIL을 모두 재검증합니다.
>
> <details><summary>Edited a file, ran a command</summary>
>
> - Ran `$p='C:\work\Elmo\Elmo_Master\docs\status\API_DEVELOPMENT_PLAN_2026-07-30.html'; $lines=Get-Content -LiteralPath $p; for($i=360;$i -le 375;$i++){ '{0,5}: {1}' -f ($i+1),$lines[$i] }`
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+1 -1)
>
> </details>
>
> 8개 문서/HTML 독립 재검토도 CLEAN입니다. schema 3 구현은 `39c3e6f`, mandatory suite는 `1b9be6a`, 13-role 확장은 `3c63dea`로 귀속을 바로잡았고, 매뉴얼 승격·README 정책·Gate D STOP 경계가 모두 일치합니다. 이 문서 tranche를 별도 커밋합니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $files=@(
> 'LMC_Library/LMC_API/README.md',
> 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md',
> 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html',
> 'LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md',
> 'docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md',
> 'docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md',
> 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md',
> 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html'
> )
> $pre=@(git diff --cached --name-only)
> if($pre.Count-ne0){throw "unexpected preexisting staged files: $($pre -join ', ')"}
> git diff --check -- $files
> if($LASTEXITCODE-ne0){throw 'scoped diff check failed'}
> git add -- $files
> if($LASTEXITCODE-ne0){throw 'git add failed'}
> $actual=@(git diff --cached --name-only)
> if($actual.Count-ne$files.Count){throw "staged count drift: $($actual.Count)"}
> foreach($f in $files){if($actual -cnotcontains $f){throw "missing staged path: $f"}}
> git diff --cached --check
> if($LASTEXITCODE-ne0){throw 'cached diff check failed'}
> git commit -m 'docs(api): record current release input baseline'
> if($LASTEXITCODE-ne0){throw "git commit failed: $LASTEXITCODE"}
> git show --stat --oneline --summary HEAD`
> - Ran `git status --short --untracked-files=no; git diff --stat -- 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'`
> - Ran `git diff -- 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'`
> - Ran `rg -n "DerivedCandidateApproved" 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'`
>
> </details>
>
> Gate D diff는 현재 1파일 `+11/-3`으로 정확히 동결됐습니다. exact hash/layout 검사를 모두 지난 뒤에만 `TerminalWakeBrokerCandidate`를 승인하며, `Absent/VendorImported` 외 다른 derived 단계는 계속 capture-only입니다. 지금 장시간 PS5/PS7 self-test와 clean-detached/dirty-main 양쪽 경계를 검증 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -like '*Verify-LasalUdpCallbackContract.ps1*' -or $_.CommandLine -like '*gate_d_ratchet*' } | Select-Object ProcessId,ParentProcessId,Name,CreationDate,CommandLine | Format-List`
>
> </details>
>
> Stream error
>
> Reconnecting 5/5
> stream disconnected before completion: An error occurred while processing your request. You can retry your request, or contact us through our help center at help.openai.com if the error persists. Please include the request ID 836f6c19-76db-481a-918a-0b30044a4154 in your message.
>
> Gate D self-test 병렬 래퍼가 300초 제한에 걸렸지만 제품 실패는 아닙니다. PS7 자식 PID만 정확히 확인해 종료했고 파일 변경은 없습니다. 이제 PS5.1과 PS7을 각각 더 긴 제한으로 실행해 결과를 분리 확보합니다.

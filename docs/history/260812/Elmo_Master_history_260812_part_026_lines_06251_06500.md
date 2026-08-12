> - Ran ````$path = 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'
> $text = [IO.File]::ReadAllText($path)
> $needle = "    Assert-ExactInventory ```n        -Actual @(`$expectedRecords.Name)"
> if (($text.Split($needle).Count - 1) -ne 1) { throw ('record debug anchor count=' + ($text.Split($needle).Count - 1)) }
> $insert = @"
>     Write-Output ('DBG_RECORD_COUNT=' + `$expectedRecords.Count)
>     foreach (`$dbgRecord in `$expectedRecords) {
>         Write-Output ('DBG_REC=' + `$dbgRecord.Name + '|LEN=' + `$dbgRecord.Block.Length + '|SHA=' + (Get-TextSha256 -Text `$dbgRecord.Block))
>     }
>     exit 0
> "@
> $text = $text.Replace($needle, $insert + "`n" + $needle)
> $scriptRootLiteral = "'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests'"
> $text = $text.Replace('$PSScriptRoot', $scriptRootLiteral)
> $sb = [ScriptBlock]::Create($text)
> & $sb -VerifyCurrent -RepositoryRoot 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25' -ExpectedState TerminalWakeBrokerCandidate````
> - Ran ``$path = 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'
> $text = [IO.File]::ReadAllText($path)
> $needle = '    $actualByName = @{}'
> if (($text.Split($needle).Count - 1) -ne 1) { throw ('record debug anchor count=' + ($text.Split($needle).Count - 1)) }
> $insert = @"
>     Write-Output ('DBG_RECORD_COUNT=' + `$expectedRecords.Count)
>     foreach (`$dbgRecord in `$expectedRecords) {
>         Write-Output ('DBG_REC=' + `$dbgRecord.Name + '|LEN=' + `$dbgRecord.Block.Length + '|SHA=' + (Get-TextSha256 -Text `$dbgRecord.Block))
>     }
>     exit 0
> "@
> $text = $text.Replace($needle, $insert + "`n" + $needle)
> $scriptRootLiteral = "'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests'"
> $text = $text.Replace('$PSScriptRoot', $scriptRootLiteral)
> $sb = [ScriptBlock]::Create($text)
> & $sb -VerifyCurrent -RepositoryRoot 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25' -ExpectedState TerminalWakeBrokerCandidate``
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n "END_FUNCTION\[ \\t\]\*\$|END_FUNCTION.*\$|END_CLASS.*\$|END_VAR.*\$" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1 | Select-Object -First 120; git check-attr text eol -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st; git ls-files --eol -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git log --oneline --decorate -12 -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1; git log --oneline --decorate -12 -- Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st; git log -1 --format='%H %ad %s' --date=iso-strict -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1; git log -1 --format='%H %ad %s' --date=iso-strict -- Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st`
>
> </details>
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `rg -n -C 3 "ExpectedTerminalWakeLayout|canonical Network aggregate drifted|FullNetwork" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1 | Select-Object -First 180`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; $c[240..370]; $c[5750..5960]`
> - Ran `rg -n "function Get-NetworkSnapshotEvidence|FullNetworkSha256|TrackedNetworkSha256" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1 | Select-Object -First 80; $p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; $c[7460..7730]`
> - Ran `$root='C:\work\Elmo\Elmo_Master_release_verify_5c48f25'; $rel='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network'; $tracked=@(git -C $root ls-files -- "$rel/**" | ForEach-Object { $_.Trim().Replace('\','/') } | Sort-Object -Unique); $available=@(Get-ChildItem -LiteralPath (Join-Path $root $rel.Replace('/','\')) -File -Recurse -Force | ForEach-Object { $_.FullName.Substring($root.Length+1).Replace('\','/') } | Sort-Object -Unique); Write-Output ('TRACKED=' + $tracked.Count); Write-Output ('AVAILABLE=' + $available.Count); Write-Output '---AVAILABLE_NOT_TRACKED---'; Compare-Object -ReferenceObject $tracked -DifferenceObject $available -PassThru | Where-Object { $_ -in $available }; Write-Output '---TRACKED_NOT_AVAILABLE---'; Compare-Object -ReferenceObject $available -DifferenceObject $tracked -PassThru | Where-Object { $_ -in $tracked }`
> - Ran `$root='C:\work\Elmo\Elmo_Master'; $rel='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network'; $tracked=@(git -C $root ls-files -- "$rel/**" | ForEach-Object { $_.Trim().Replace('\','/') } | Sort-Object -Unique); $available=@(Get-ChildItem -LiteralPath (Join-Path $root $rel.Replace('/','\')) -File -Recurse -Force | ForEach-Object { $_.FullName.Substring($root.Length+1).Replace('\','/') } | Sort-Object -Unique); Write-Output ('TRACKED=' + $tracked.Count); Write-Output ('AVAILABLE=' + $available.Count); Compare-Object -ReferenceObject $tracked -DifferenceObject $available -PassThru | Where-Object { $_ -in $available }`
> - Ran ``$root='C:\work\Elmo\Elmo_Master_release_verify_5c48f25'; $rel='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network'; function ShaBytes([byte[]]$b){$a=[Security.Cryptography.SHA256]::Create();try{return ([BitConverter]::ToString($a.ComputeHash($b))).Replace('-','')}finally{$a.Dispose()}}; function ShaText([string]$s){return ShaBytes ([Text.UTF8Encoding]::new($false,$true).GetBytes($s))}; $tracked=@(git -C $root ls-files -- "$rel/**" | ForEach-Object { $_.Trim().Replace('\','/') } | Where-Object {$_} | Sort-Object -Unique); $files=@(); foreach($p in $tracked){$full=Join-Path $root $p.Replace('/','\');$b=[IO.File]::ReadAllBytes($full);$files += [pscustomobject]@{Path=$p;Tracked=$true;Available=$true;Bytes=$b.Length;Sha=(ShaBytes $b)}}; $fullId=[string]::Join("`n",@($files|ForEach-Object{"$($_.Path)|1|1|$($_.Bytes)|$($_.Sha)"}));$trackedId=[string]::Join("`n",@($files|ForEach-Object{"$($_.Path)|$($_.Bytes)|$($_.Sha)"})); Write-Output ('FULL_COUNT='+$files.Count); Write-Output ('FULL_SHA='+(ShaText $fullId)); Write-Output ('TRACKED_COUNT='+$files.Count); Write-Output ('TRACKED_SHA='+(ShaText $trackedId)); foreach($f in $files){Write-Output ("$($f.Path)|$($f.Bytes)|$($f.Sha)")}``
> - Ran ``$root='C:\work\Elmo\Elmo_Master'; $rel='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network'; function ShaBytes([byte[]]$b){$a=[Security.Cryptography.SHA256]::Create();try{return ([BitConverter]::ToString($a.ComputeHash($b))).Replace('-','')}finally{$a.Dispose()}}; function ShaText([string]$s){return ShaBytes ([Text.UTF8Encoding]::new($false,$true).GetBytes($s))}; $tracked=@(git -C $root ls-files -- "$rel/**" | ForEach-Object { $_.Trim().Replace('\','/') } | Where-Object {$_} | Sort-Object -Unique); $avail=@(Get-ChildItem -LiteralPath (Join-Path $root $rel.Replace('/','\')) -File -Recurse -Force|ForEach-Object{$_.FullName.Substring($root.Length+1).Replace('\','/')}|Sort-Object -Unique);$all=@($tracked+$avail|Sort-Object -Unique);$lines=@();foreach($p in $all){$full=Join-Path $root $p.Replace('/','\');$b=[IO.File]::ReadAllBytes($full);$lines += "$p|$([int]($tracked -contains $p))|1|$($b.Length)|$(ShaBytes $b)"};Write-Output ('FULL_COUNT='+$all.Count);Write-Output ('FULL_SHA='+(ShaText ([string]::Join("`n",$lines))))``
>
> </details>
>
> 원인은 두 층으로 분리됐습니다.
>
> - 첫 실패는 실제 소스 변화가 아니라 Windows CRLF에서 `END_FUNCTION` 경계를 잘못 읽어 `CyWork` 뒤 13개 함수까지 한 함수로 합친 verifier 버그입니다.
> - 그 보정을 메모리에서 적용하면 다음 실패는 진짜 clean checkout에 없는 8개 무시 대상 `.lba/.lob`까지 “full Network 23개”로 요구하는 계약입니다. Git 추적 Network 15개 자체는 기대 hash와 정확히 일치합니다.
>
> 따라서 소스나 generated 파일을 복사해 맞추지 않고, verifier가 “추적 15개만 존재하는 진짜 clean checkout”을 exact tuple로 별도 허용하도록 좁게 보강하겠습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n "8/8|16/16|HandleRequest.*fence|whole-method|solution|LasalApiWpfTestApp\.sln|clean detached|seeded|ignored.*\.lba|Network aggregate|PureGit" docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md LMC_Library\LMC_API\README.md docs\status\API_DEVELOPMENT_PLAN_2026-07-30.md | Select-Object -First 300`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; $c[13580..13680]; $q='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1'; $d=Get-Content -LiteralPath $q; $d[43600..43680]`
> - Ran `rg -n -C 8 "AllowDerivedCapture|UdpCallbackVerifier|VerifyCurrent" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 | Select-Object -Last 120`
> - Ran `$p='docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md'; $c=Get-Content -LiteralPath $p; $c[285..325]; $c[1060..1185]; $q='LMC_Library\LMC_API\README.md'; $d=Get-Content -LiteralPath $q; $d[90..155]; $e='docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'; $f=Get-Content -LiteralPath $e; $f[380..430]; $f[970..1010]`
>
> </details>
>
> verifier 수정은 한 파일 안에서 진행 중입니다. 기존 두 23-file identity는 그대로 보존하고, 오직 다음 exact 세 번째 경우만 추가합니다: Git 추적 Network 15개가 모두 존재하고 full hash가 `239F71DC…`인 순수 checkout. 생성물 1~7개만 섞이거나, 변조·추가 파일·tracked 누락은 모두 계속 차단하도록 fixture를 넣고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1; git diff --stat -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1; git diff --check -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1`
>
> </details>
>
> 현재 patch는 CRLF 함수 경계와 순수 Git Network tuple을 한 verifier에 반영했고, 지금 PS5.1/PS7 self-test를 돌리는 단계입니다. tracked source·`Classes.lcb`·Network 파일은 전혀 수정하지 않았습니다. 이 patch가 통과해도 Distribution은 다음의 의도된 `needsRebaseline` ratchet에서 멈출 가능성이 높으며, 그 STOP은 우회하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('pwsh.exe','powershell.exe') -and $_.CommandLine -match 'Verify-LasalUdpCallbackContract|Verify-LasalContract' } | Select-Object ProcessId,Name,CreationDate,CommandLine`
>
> </details>
>
> 양쪽 AST, 교차 EOL parser, 순수 Git checkout focused 검증은 이미 통과했습니다. 남은 full self-test 두 프로세스는 CPU를 사용하며 정상 진행 중이고, hang 징후는 없습니다. 종료 결과를 받은 뒤 독립 reviewer를 한 번 더 붙입니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1`
> - Ran `rg -n "negative fixtures rejected|negativeCount -ne|expected [0-9]+|positives accepted" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1 | Select-Object -Last 30`
>
> </details>
>
> 수정본은 동결됐고 양쪽 전체 self-test가 `294/294`, 순수 Git current 검증이 모두 `exit 0 / CAPTURE`입니다. 이제 두 독립 reviewer가 “세 번째 tuple이 범용 완화가 아닌 exact 허용인지”와 negative fixture의 실효성을 최종 확인 중입니다.
>
> 독립 reviewer가 기능 false-pass는 없지만 테스트 ratchet의 결합도를 지적했습니다. 기존 negative 하나가 “pure-Git extra”와 “seeded partial”을 동시에 깨고, corrupt fixture도 Full/Tracked hash를 같이 깨서 어느 한 검사가 빠져도 다른 검사로 통과할 수 있었습니다. 지금 partial·corrupt·extra·pure-Git hash를 각각 단독으로 깨는 4개 fixture로 분리하고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1; git diff --check -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1; git diff -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 | Select-String -Pattern "pure Git Network|seeded|FullNetworkCount|FullNetworkSha256|TrackedNetworkCount|TrackedNetworkSha256" -Context 2,3`
>
> </details>
>
> fixture 분리는 완료됐습니다. 현재는 다음을 서로 독립적으로 검증합니다: tracked 누락(14), seeded partial(22), seeded hash 손상(23), extra ignored(24), pure-Git hash 손상(15). 양 호스트 full self-test를 다시 실행 중이며, 결과가 끝나면 latest diff만 재리뷰하고 커밋합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('pwsh.exe','powershell.exe') -and $_.CommandLine -match 'Verify-LasalUdpCallbackContract' } | Select-Object ProcessId,Name,CreationDate,CommandLine`
>
> </details>
>
> 재실행은 정상입니다. PS5.1은 이전 기준 약 4분, PS7은 약 9분이 걸리는 대형 self-test라 현재 무출력은 예상 범위입니다. CPU 사용과 프로세스 응답은 유지되고 있고 예외 출력은 없습니다.
>
> PS5.1 self-test는 종료했고 결과가 wrapper에 보관됐습니다. PS7만 계속 정상 실행 중입니다. 완료 전에는 커밋하지 않고 동일 frozen bytes를 유지합니다.
>
> 최종 frozen patch는 두 reviewer 모두 CLEAN입니다. 양 호스트 `296/296`, true-clean current `CAPTURE`, exact tuple/negative fixture 독립성까지 확인됐습니다. 이제 verifier 한 파일만 세 번째 커밋으로 고정하고 clean worktree를 전진시킨 뒤 전체 Distribution을 다시 실행합니다.
>
> <details><summary>Ran 9 commands</summary>
>
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $h=(Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash; Write-Output ('SHA256='+$h); git diff --check -- $p; git add -- $p; git diff --cached --name-only; git diff --cached --stat; git diff --cached --check`
> - Ran `git commit -m "fix(lasal): make callback verifier checkout-safe"`
> - Ran `git -C C:\work\Elmo\Elmo_Master_release_verify_5c48f25 checkout --detach afdf6a3; Write-Output ('HEAD=' + (git -C C:\work\Elmo\Elmo_Master_release_verify_5c48f25 rev-parse --short=12 HEAD)); $s=git -C C:\work\Elmo\Elmo_Master_release_verify_5c48f25 status --porcelain=v1 --untracked-files=all; Write-Output ('STATUS_COUNT=' + @($s).Count); foreach($p in @('C:\work\Elmo\Elmo_Master_release_verify_5c48f25\output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx','C:\work\Elmo\Elmo_Master_release_verify_5c48f25\output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf')){$i=Get-Item -LiteralPath $p; Write-Output ("$($i.Length)|$((Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash)|$p")}; Test-Path -LiteralPath C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Distribution_candidate_clean_afdf6a3`
> - Ran `$root=[IO.Path]::GetFullPath('C:\work\Elmo\Elmo_Master_release_verify_5c48f25'); Get-ChildItem -LiteralPath (Join-Path $root 'LMC_Library') -Directory -Recurse -Force | Where-Object { $_.Name -in @('bin','obj') } | ForEach-Object { $full=[IO.Path]::GetFullPath($_.FullName); if(-not $full.StartsWith($root + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw "outside root: $full"}; $full }`
> - Ran `$verifyRoot=[IO.Path]::GetFullPath('C:\work\Elmo\Elmo_Master_release_verify_5c48f25'); $targets=@(Get-ChildItem -LiteralPath (Join-Path $verifyRoot 'LMC_Library') -Directory -Recurse -Force | Where-Object { $_.Name -in @('bin','obj') }); foreach($target in $targets){$resolved=[IO.Path]::GetFullPath($target.FullName); if(-not $resolved.StartsWith($verifyRoot + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw "Refusing outside verification worktree: $resolved"}; if($target.Name -notin @('bin','obj')){throw "Unexpected target: $resolved"}; Remove-Item -LiteralPath $resolved -Recurse -Force}; Write-Output ('REMOVED_COUNT='+$targets.Count); $remaining=@(Get-ChildItem -LiteralPath (Join-Path $verifyRoot 'LMC_Library') -Directory -Recurse -Force | Where-Object { $_.Name -in @('bin','obj') }); Write-Output ('REMAINING_COUNT='+$remaining.Count); $status=git -C $verifyRoot status --porcelain=v1 --untracked-files=all; Write-Output ('STATUS_COUNT='+@($status).Count)`
> - Ran `Remove-Item -LiteralPath 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API\LMC_API\bin' -Recurse -Force
> Remove-Item -LiteralPath 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\src\bin' -Recurse -Force
> Remove-Item -LiteralPath 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\src\obj' -Recurse -Force
> Remove-Item -LiteralPath 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin' -Recurse -Force
> Remove-Item -LiteralPath 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\obj' -Recurse -Force
> Write-Output ('STATUS_COUNT=' + @(git -C 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25' status --porcelain=v1 --untracked-files=all).Count)`
> - Ran `git clean -ndX -- LMC_Library/LMC_API/LMC_API/bin LMC_Library/LMC_API_Delivery/src/bin LMC_Library/LMC_API_Delivery/src/obj LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/bin LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/obj`
> - Ran `git status --short --ignored -- LMC_Library/LMC_API/LMC_API/bin LMC_Library/LMC_API_Delivery/src/bin LMC_Library/LMC_API_Delivery/src/obj LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/bin LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/obj; git ls-files -- LMC_Library/LMC_API/LMC_API/bin`
> - Ran `git clean -fdX -- LMC_Library/LMC_API_Delivery/src/bin LMC_Library/LMC_API_Delivery/src/obj LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/bin LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/obj; git status --short --ignored -- LMC_Library/LMC_API_Delivery/src/bin LMC_Library/LMC_API_Delivery/src/obj LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/bin LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/obj; Write-Output ('STATUS_COUNT=' + @(git status --porcelain=v1 --untracked-files=all).Count)`
>
> </details>
>
> 이전 실패 실행이 남긴 detached worktree의 정확한 4개 `bin/obj` 디렉터리만 제거했습니다. 모두 빌드로 재생성되는 ignored 산출물이고, tracked `LMC_API/LMC_API/bin/LasalMotionControlLib.dll`과 매뉴얼 입력은 보존했습니다. worktree Git 상태는 다시 0입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 -RepositoryRoot C:\work\Elmo\Elmo_Master_release_verify_5c48f25 -AllowDirty -CandidatePath C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Distribution_candidate_clean_afdf6a3 -ManualDocxPath C:\work\Elmo\Elmo_Master_release_verify_5c48f25\output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx -ManualPdfPath C:\work\Elmo\Elmo_Master_release_verify_5c48f25\output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf`
>
> </details>
>
> 전체 파이프라인 재실행은 새 verifier 커밋 `afdf6a3`에서 오류 없이 진행 중입니다. 이번에는 pure-Git Network 판정을 통과한 뒤, 승인되지 않은 TerminalWake physical snapshot ratchet이 실제 첫 STOP인지 확인하는 실행입니다.
>
> 2분 경과, 계속 정상 실행 중입니다. 이전 false blocker가 발생하던 UDP callback 구간은 곧 도달할 시점이며, wrapper가 여전히 최종 오류 원문을 숨기면 동일 target만 단독 실행해 정확한 STOP을 회수하겠습니다.
>
> 재실행도 214초 뒤 같은 상위 `RunTests` 실패로 종료됐지만, 이는 wrapper 메시지입니다. 트랜잭션 rollback을 확인한 다음 UDP verifier를 default 인자로 직접 호출해, 구조 검증 이후의 의도된 ratchet STOP 문구를 정확히 캡처합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$root='C:\work\Elmo\Elmo_Master_release_verify_5c48f25'; $candidate=Join-Path $root 'LMC_Library\LMC_API_Distribution_candidate_clean_afdf6a3'; Write-Output ('CANDIDATE_EXISTS='+(Test-Path -LiteralPath $candidate)); Write-Output ('STATUS_COUNT='+@(git -C $root status --porcelain=v1 --untracked-files=all).Count); Get-ChildItem -LiteralPath (Join-Path $root 'LMC_Library') -Force | Where-Object { $_.Name -like '*candidate_clean_afdf6a3*' -or $_.Name -like '*.staging*' -or $_.Name -like '*.lock*' } | Select-Object FullName,PSIsContainer,Length; Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('MSBuild.exe','dotnet.exe','LasalMotionControlApiExample.exe') -and $_.CommandLine -match 'Elmo_Master_release_verify_5c48f25' } | Select-Object ProcessId,Name,CommandLine`
> - Ran `powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1 -VerifyCurrent -RepositoryRoot C:\work\Elmo\Elmo_Master_release_verify_5c48f25`
> - Ran `powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1 -VerifyCurrent -RepositoryRoot C:\work\Elmo\Elmo_Master_release_verify_5c48f25 -ExpectedState TerminalWakeBrokerCandidate -AllowDerivedCapture`
>
> </details>
>
> 재실행 결과가 의도한 경계까지 정리됐습니다.
>
> - false blocker 2개는 해소됨
> - focused clean checkout은 `CAPTURE`, `productionApproved=False`, `needsRebaseline=True`
> - default Distribution은 정확히 “approved physical snapshot ratchet 없음”에서 STOP
> - candidate/staging/lock은 모두 rollback됐고 worktree 상태는 0
>
> 이제 이 결과와 새 solution/HandleRequest/size 계약을 권위 문서에 반영하되, Distribution PASS나 production 승인으로 쓰지 않겠습니다.
>
> 문서 갱신과 다음 offline backlog 감사를 병렬로 진행 중입니다. 이번 문서의 핵심은 “clean checkout 구조 검증 PASS”와 “release builder PASS”를 분리하는 것입니다. 전자는 명시적 `AllowDerivedCapture`에서만 CAPTURE했고, 후자는 승인 ratchet 때문에 candidate 생성 전에 STOP했습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git diff --stat; git diff --check`
>
> </details>
>
> 문서는 5개 권위 위치에 반영됐습니다. solution gate, `HandleRequest` 13/13, size 16/16, UDP 296/296, clean detached 214초 STOP을 각각 분리해 기록했고 지금 문구·scope 검수 중입니다. 다음 offline 후보는 증거 publication trust와 양 PowerShell 호스트 aggregate gate인데, 안전성과 실제 설계 우선순위를 더 따져 선택하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git diff --stat -- LMC_Library/LMC_API/README.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md test/AUTOMATED_TESTS_2026-07-10.md; git diff --check`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 재접속 코드와 실제 EXE 회귀 게이트까지는 정적·PC 검증으로 닫았고, 지금은 clean checkout 배포 검증에서 드러난 마지막 경계를 문서에 정확히 고정하는 단계입니다. 현재 배포는 실패한 게 아니라, 승인되지 않은 Gate D 물리 스냅샷 전이에서 의도대로 STOP한 상태이며 LASAL IDE·PLC·Download는 건드리지 않았습니다. 문서 독립 검토와 다음 PC-only 우선순위 감사까지 마친 뒤 목적별 커밋으로 닫겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$docs = @('LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git status --short; git log -5 --oneline; git diff --stat -- $docs; git diff --check -- $docs; git diff --cached --check -- $docs`
> - Ran `$docs = @('LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --unified=3 -- $docs`
>
> </details>
>
> 5개 문서 패치는 동결됐고 범위 검사는 통과했습니다. 이제 별도 리뷰로 역사적 2.0 PASS와 현재 2.3 Gate D STOP, `-AllowDirty`의 의미, actual-EXE/manifest 미도달 경계가 과장 없이 일치하는지 줄 단위로 다시 확인하겠습니다.
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `$docs = @('LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); rg -n "129/129|214|TerminalWakeBrokerCandidate|88f1c57|d735446|afdf6a3|candidate|actual-EXE|manifest|dirty-preview|AllowDirty" $docs`
> - Ran `$p='docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'; $lines=Get-Content -LiteralPath $p; for($i=1038;$i -le 1055;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
> - Ran `$p='docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'; $lines=Get-Content -LiteralPath $p; for($i=1028;$i -le 1040;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
> - Ran `rg -n "function Get-LmcReleaseInputFiles|Get-LmcReleaseInputFiles|InputTreeSha256|release input|ReleaseInput" LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1`
> - Ran `$p='LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(250,430),@(730,780),@(920,970))){ for($i=$range[0];$i -le $range[1];$i++){ '{0}:{1}' -f $i,$lines[$i-1] }; '' }`
> - Ran `rg -n "GetInputFingerprint|fingerprint|InputTree|drift|ValidatePrepared" LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1`
>
> </details>
>
> 다음 PC-only 필수 항목도 확정했습니다. 배포 transaction이 promotion 전 입력 fingerprint를 재검사하지만 LASAL 쪽은 현재 4파일만 묶어, 실제 static gate가 읽는 `LMCControlCommandService.st`, `Classes.lcb`, `Networks.lcb` 등의 동시 변경을 놓칠 수 있습니다. 문서 리뷰와 병렬로 이 fingerprint 범위를 fail-closed로 완결하되, Gate D baseline이나 사용자 `Classes.lcb`는 변경하지 않겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git diff --name-only`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); rg -n -i "current.{0,80}(distribution|candidate|manifest).{0,80}PASS|full Distribution.{0,80}PASS|candidate publish.{0,80}PASS|clean tree.{0,80}PASS|SourceOnly.{0,80}PASS" $docs`
> - Ran `$p='docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'; $lines=Get-Content -LiteralPath $p; for($i=1;$i -le 65;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
>
> </details>
>
> 문서 수치와 clean-detached 실행 근거는 독립 재현까지 일치했습니다. 리뷰가 현재 확인 중인 유일한 표현 쟁점은 method-size의 `raw/LF/CRLF exact baseline`이 “현재 물리 줄끝 하나”가 아니라 “세 가지 직렬화 차원별 현재 상한”이라는 점을 문서가 충분히 명확히 말하는지입니다. 동시에 배포 입력 fingerprint 보강은 LASAL 검증 입력의 정확한 포함·제외 규칙부터 고정하고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1`
>
> </details>
>
> 독립 리뷰에서 문서 과장만이 아니라 실제 portability 틈이 확인됐습니다. size ratchet이 main의 mixed-EOL 추출값을 baseline으로 잡아 clean checkout에서는 Recorder/RtWork가 baseline보다 작고, clean-current의 1바이트 증가 일부를 허용합니다. 단순히 문구만 낮추지 않고 verifier를 checkout/EOL 불변 기준으로 고쳐 정말 `current+1`을 막을 수 있는지 별도 구현·검증하겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "BaselineDebt|Get-.*Method|RawBytes|LfBytes|CrLfBytes|Measure|Normalize" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(35,190),@(500,535))){ for($i=$range[0];$i -le $range[1];$i++){ '{0}:{1}' -f $i,$lines[$i-1] }; '' }`
>
> </details>
>
> <details><summary>Ran 7 commands</summary>

> </details>
>
> 두 번째 blocker도 production source drift가 아닙니다. Fresh checkout은 `.gitattributes`대로 LF인데, self-test가 checkout EOL을 그대로 보존하면서 CRLF로 고정된 historical Gate A fixture와 비교했습니다. ConfigObjects synthetic fixture만 checkout-independent CRLF로 직렬화하고 LF/CRLF 교차 fixture를 추가하는 방향으로 범위를 좁히고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; git diff --stat -- 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'`
>
> </details>
>
> 정적 감사 결과, checkout EOL을 직접 보존하는 결함은 ConfigObjects synthetic helper 한 곳뿐입니다. TCP/lcp/generated includes/Network 경로는 이미 canonical LF 또는 binary identity를 사용합니다. 지금 verifier 1파일에 고정 CRLF materialization과 LF/CRLF 교차 self-test를 추가하고 있습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran ``$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; rg -n "New-Synthetic|\\n|`n" $p | Select-Object -Skip 450 -First 350``
> - Ran `Select-String -LiteralPath 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1' -Pattern '\\n','New-Synthetic' | Where-Object {$_.LineNumber -ge 8100 -and $_.LineNumber -le 10600} | Select-Object LineNumber,Line | Format-Table -Wrap`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content $p; for($i=9138;$i -le 9290;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> 추가 동적 감사에서 clean checkout의 verifier 자체가 CRLF인 경우도 드러났습니다. Empty-stub synthetic helper가 LF 전용 정규식으로 source macro를 제거해 발생한 두 번째 fixture 결함이며, 시작 즉시 canonical LF로 바꾸도록 함께 수정했습니다. 지금 patched clean PS7 전체 296 self-test로 더 뒤의 blocker가 없는지 확인 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content $p; foreach($range in @(@(9760,9875),@(10170,10280),@(10320,10550))){for($i=$range[0];$i -le $range[1];$i++){'{0,5}: {1}' -f $i,$l[$i-1]};''}`
>
> </details>
>
> 동적 추적에서 세 번째 fixture 문제도 확인했습니다. `ONE_Comm_Network_Table` synthetic here-string이 clean CRLF verifier의 EOL을 보존해 LF 전용 삽입 anchor가 실패한 것입니다. 이 helper도 즉시 canonical LF로 고정했고, 세 보정을 모두 넣은 clean PS7 전체 self-test가 뒤쪽 계약까지 진행 중입니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object {($_.Name -in @('pwsh.exe','powershell.exe')) -and $_.CommandLine -match 'UdpCallback|Verify-Lasal'} | Select-Object ProcessId,ParentProcessId,Name,CreationDate,CommandLine | Format-List`
> - Ran `Get-Process -Id 8788 -ErrorAction SilentlyContinue | Select-Object Id,StartTime,CPU,Responding,WorkingSet64 | Format-List`
>
> </details>
>
> 세 보정을 넣은 clean PS7 self-test는 현재 CPU를 계속 사용하며 정상 연산 중입니다. 추가 편집은 멈춘 상태이고, 296/296 전체 종료 전에는 이 수정안을 동결하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process -Id 8788 -ErrorAction SilentlyContinue | Select-Object Id,StartTime,CPU,Responding,WorkingSet64 | Format-List`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content $p; for($i=8470;$i -le 8495;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}; for($i=9134;$i -le 9150;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content $p; foreach($range in @(@(9500,9635),@(9938,10030),@(10235,10322))){for($i=$range[0];$i -le $range[1];$i++){'{0,5}: {1}' -f $i,$l[$i-1]};''}`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object {($_.Name -eq 'pwsh.exe') -and $_.CommandLine -match 'UdpCallback|Verify-Lasal'} | Select-Object ProcessId,ParentProcessId,CreationDate,CommandLine | Format-List`
> - Ran `git diff -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1`
> - Ran `Get-Process -Id 31740 -ErrorAction SilentlyContinue | Select-Object Id,StartTime,CPU,Responding,WorkingSet64 | Format-List`
>
> </details>
>
> 최종 후보는 세 root point로 정리됐습니다: ConfigObjects historical fixture는 고정 CRLF, synthetic sender source와 ONE table은 canonical LF입니다. 개별 증상을 덧대지 않고 생성 루트에서 고쳤으며, clean CRLF verifier + LF checkout 조합의 전체 self-test가 계속 연산 중입니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-Process -Id 31740 -ErrorAction SilentlyContinue | Select-Object Id,StartTime,CPU,Responding,WorkingSet64 | Format-List`
> - Ran `Get-Process -Id 31740 -ErrorAction SilentlyContinue | Select-Object Id,StartTime,CPU,Responding,WorkingSet64 | Format-List`
>
> </details>
>
> clean PS7 self-test는 CPU를 계속 사용하고 있으며 hang 징후가 없습니다. 현재까지 세 root 보정 뒤 새 blocker 출력은 없고, 완료 전 추가 파일은 건드리지 않고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Select-String -LiteralPath 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1' -Pattern "return @'","= @'" | Select-Object LineNumber,Line | Format-Table -AutoSize`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> clean checkout에서만 드러나는 UDP self-test EOL 결함을 계속 좁히고 있습니다. 확인된 네 번째 root helper까지 수정됐고, 다음 장시간 실행 전에 같은 유형의 here-string 생성기를 전부 정적 분류해 연쇄 실패를 끊겠습니다. LASAL source와 사용자 `Classes.lcb`는 건드리지 않습니다.
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `$repo = 'C:\work\Elmo\Elmo_Master'; git status --short; git diff --numstat -- 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; git diff --check -- 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; rg -n "^function (Get-ExpectedTcpDisarmHelperExecutable|Get-ExpectedTerminalWakePublishBlock|New-SyntheticDerivedSource|Get-SyntheticGateCRpcLifecycleFunction|New-SyntheticDerivedNetwork|New-SyntheticProjectDefinition|New-SyntheticVendorTransceiverSource|New-SyntheticVendorInterfaceSource|New-SyntheticTerminalWakeDiagnosticsSource|New-SyntheticConfigObjects)" $p; git diff -- $p`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; foreach($range in @(@(3090,3225),@(3515,3605),@(9250,9575),@(9760,10030),@(10220,10310))){ $s=$range[0]; $e=$range[1]; "--- $s-$e ---"; for($i=$s;$i -le $e;$i++){ '{0}:{1}' -f $i,$c[$i-1] } }`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; rg -n "Get-ExpectedTerminalWakeTryTakeBlock|Get-ExpectedTerminalWakePublishBlock|Get-ExpectedTcpDisarmHelperExecutable|New-SyntheticVendorTransceiverSource|New-SyntheticVendorInterfaceSource|New-SyntheticTerminalWakeDiagnosticsSource|New-SyntheticProjectDefinition|\.Xml|New-SyntheticDerivedNetwork" $p`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; foreach($range in @(@(3290,3425),@(3610,3655),@(9570,9630),@(10320,10560),@(12790,13130))){$s=$range[0];$e=$range[1];"---$s-$e---";for($i=$s;$i -le $e;$i++){ '{0}:{1}' -f $i,$c[$i-1]}}`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; rg -n "TerminalWake|TryTakeD5|PublishD5|Gate D" $p | Select-Object -Last 140`
>
> </details>
>
> 정적 전수분류 결과, 최종 최소 수정점은 6개 root로 확정했습니다. 기존 4개 외에 canonical LF TCP에 삽입되는 Disarm/TerminalWake expected block 2개도 LF로 고정해야 clean/main checkout에서 synthetic TCP 바이트가 같아집니다. 이 범위로 동결한 뒤 양 PowerShell 호스트의 296개 self-test와 실제 host-parity worker를 재검증하겠습니다.
>
> 최종 6-root 패치는 한 파일 `+73/-8`로 동결됐고 AST·diff 검사는 통과했습니다. 이제 물리적으로 `CRLF verifier + LF tracked source`인 clean checkout에서 PS5.1/PS7 296개 전체 self-test를 순차 실행하고, 동시에 main 조합은 독립 PS5.1 회귀 실행으로 확인합니다. 이 단계가 끝나기 전에는 커밋하지 않습니다.
>
> main 조합은 최종 바이트로 PS5.1 `296/296 PASS`(224.9초)했습니다. 독립 리뷰도 P0/P1 없음이며, clean checkout의 실제 물리 조합(`CRLF verifier + LF ConfigObjects`)에서 이제 PS5.1 → PS7 순서로 최종 검증 중입니다.
>
> clean checkout 물리 조합의 PS5.1도 `296/296 PASS`(219.8초)했습니다. 같은 바이트/EOL 트리에서 마지막 PS7 전체 self-test를 실행 중이며, 이 결과가 끝나면 한 파일 커밋 후 새 clean detached full Distribution을 처음부터 다시 시작합니다.
>
> PS7 검증은 약 4분대까지 오류 없이 계속 계산 중입니다. 이 self-test는 과거에도 약 8–9분 걸렸고 1,200초 제한을 두었으므로, 중간에 끊지 않고 완료 결과만 판정에 사용합니다.
>
> 마지막 clean PS7도 `296/296 PASS`(505.1초)했습니다. 최종 증거는 main PS5와 clean PS5/PS7 모두 통과했고 독립 리뷰도 CLEAN입니다. 이제 정확히 verifier 한 파일만 스테이징·커밋한 뒤, 새 커밋의 새 clean detached worktree에서 full Distribution을 재실행합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; git rev-parse --short=8 HEAD; git diff --numstat -- $p; Get-FileHash -Algorithm SHA256 -LiteralPath $p | Format-List Algorithm,Hash,Path; git diff --check -- $p; git diff --cached --check`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; git add -- $p; git diff --cached --name-only; git diff --cached --numstat; git diff --cached --check; git commit -m "test(lasal): make UDP selftest checkout-portable"`
>
> </details>
>
> checkout-portability 수정은 `105daf2`로 정확히 한 파일만 커밋했습니다. 새 detached worktree와 새 candidate를 만들고, 기본 인자만 사용하는 PS5.1 full Distribution을 처음부터 재실행 중입니다. 이전 두 실패 worktree/candidate는 재사용하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git worktree list --porcelain; Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -like '*Build-LmcApiDistribution.ps1*105daf2*' -or $_.CommandLine -like '*Elmo_Master_release_verify_105daf2*' } | Select-Object ProcessId,ParentProcessId,Name,CommandLine | Format-List`
>
> </details>
>
> 새 `105daf2` detached worktree는 생성됐고 HEAD가 정확히 일치합니다. 아직 full build 프로세스는 시작 전이며, 사전 canonical/manual 해시 고정 단계가 끝나는 대로 direct PS5.1 실행 PID와 시작 시각을 보고하겠습니다.
>
> full run은 사전 5개 PS5 suite를 지나 가장 긴 `UdpCallback` 296개 구간에 진입했습니다. 아직 preflight 단계라 candidate/stage/lock은 모두 0이며, 트랜잭션 순서도 지켜지고 있습니다.
>
> PS5 `UdpCallback`은 CPU를 계속 사용하며 정상 진행 중입니다. 이 구간은 clean 단독 실행에서 약 220초였으므로 현재 상태는 예상 범위이고, candidate는 계속 생성되지 않았습니다.
>
> PS5의 7개 preflight suite는 모두 완료됐고 PS7 쪽으로 전환했습니다. 현재 PS7 `ToolchainProvenance`까지 전진했으며, candidate/stage/lock은 계속 0입니다. 다음 긴 구간은 PS7 `UdpCallback`입니다.
>
> PS7 `UdpCallback`이 정상 실행 중입니다. clean 단독 기준 약 505초가 걸린 구간이라 지금은 예상 범위이며, preflight가 끝날 때까지 배포 candidate를 만들지 않는 fail-closed 순서가 유지되고 있습니다.
>
> PS7 장시간 suite는 계속 CPU 진행 중이고 오류 출력은 없습니다. 이 단계에서는 결과가 끝에 일괄 반환되므로 중간 PASS로 과장하지 않고, 종료 코드와 exact terminal evidence만 사용합니다.
>
> PS7 `UdpCallback`은 CPU 약 277초까지 진행했고 여전히 정상입니다. candidate가 없는 상태도 유지되어, 만약 실패해도 canonical 배포물에는 영향이 없는 단계입니다.
>
> PS7 마지막 장시간 gate가 CPU 약 397초까지 전진했습니다. 과거 정상 소요(약 505초)와 일치하므로 중단하지 않고 그대로 완료를 기다립니다.
>
> 이번 run은 mandatory preflight `14/14 PASS`까지 처음 통과했지만, 그 직후 별도 toolchain provenance 재해석에서 STOP했습니다. 정확한 blocker는 세 Python 후보 모두에 대해 `A child-process argument contains a quotation mark.`입니다. 우회·재시도 없이 종료했고, 지금 canonical 불변성과 잔여물부터 감사한 뒤 코드 원인을 좁힙니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `rg -n "A child-process argument contains a quotation mark|child-process argument|compatible Python provenance candidate" LMC_Library/LMC_API -g '*.ps1'`
> - Ran `rg -n "^function Invoke-.*Process|Invoke-.*Process" LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 | Select-Object -First 120`
> - Ran `$a='LMC_Library/LMC_API/DistributionToolchainProvenance.ps1'; $b='LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; $fa=rg '^function ' $a | ForEach-Object { ($_ -split 'function ')[1] -replace ' \{.*','' }; $fb=rg '^function ' $b | ForEach-Object { ($_ -split 'function ')[1] -replace ' \{.*','' }; Compare-Object $fa $fb -IncludeEqual -ExcludeDifferent | Format-Table -AutoSize; rg -n "quotation mark|Join-.*Argument|ArgumentList|Arguments =|\.Arguments" $a $b`
> - Ran `$files=@('LMC_Library/LMC_API/DistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1','LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'); foreach($f in $files){"---$f---"; rg -n "ConvertTo-LmcDistributionProcessArgument|\. .*ToolingHostParity|\. .*ToolchainProvenance" $f}`
> - Ran `$f='LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; $c=Get-Content $f; for($i=1;$i -le 130;$i++){ '{0}:{1}' -f $i,$c[$i-1] }; foreach($f2 in @('LMC_Library/LMC_API/DistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1')){ "---$f2"; $c=Get-Content $f2; $hit=(Select-String -LiteralPath $f2 -Pattern '^function ConvertTo-LmcDistributionProcessArgument').LineNumber; for($i=$hit;$i -le $hit+45;$i++){ '{0}:{1}' -f $i,$c[$i-1] }}`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/DistributionToolchainProvenance.ps1','LMC_Library/LMC_API/ReleaseManifest.ps1','LMC_Library/LMC_API/DistributionPipeline.ps1','LMC_Library/LMC_API/DistributionSemanticPolicy.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'); $all=foreach($f in $files){ rg '^function ' $f | ForEach-Object { [pscustomobject]@{File=$f;Name=(($_ -replace '^function\s+','') -replace '\s*\{.*$','')} } }; $all | Group-Object Name | Where-Object Count -gt 1 | ForEach-Object { $_.Group | Format-Table -AutoSize | Out-String }`
>
> </details>
>
> 원인은 Python이나 PS5/PS7 차이가 아니라 dot-source 함수명 충돌입니다. provenance 쪽은 따옴표를 escape하지만, 뒤에 로드된 host-parity의 동명 helper가 이를 덮어써 Python `-c` 코드를 거부했습니다. private helper를 고유 이름으로 바꾸고, Build가 로드하는 구현 파일들 사이의 함수명 중복을 AST 계약으로 막는 회귀 테스트를 추가합니다.
>
> 실패 후 감사는 clean입니다: candidate/stage/lock/process/temp 잔여가 모두 0이고 canonical tree와 DOCX/PDF 해시는 전후 동일합니다. 수정은 동명 helper 하나를 분리하는 데 그치지 않고, Build가 dot-source하는 구현 전체에서 함수명 중복을 자동 거부하도록 테스트까지 묶고 있습니다.
>
> 수정 범위는 2파일로 줄였습니다. provenance private helper를 고유 이름으로 바꾸고, provenance 테스트가 실제 Build와 같은 로드 순서로 host-parity를 뒤에 dot-source하도록 해 기존의 따옴표 포함 Python probe 자체가 충돌을 비공허하게 재현·차단합니다. Pipeline/host-parity 수치 계약은 바꾸지 않습니다.
>
> 2파일 수정 후 양 호스트에서 provenance `84/84`, pipeline `291/291`, AST가 모두 통과했습니다. Build 로드 범위의 top-level 함수도 `89/89 unique`로 확인됐습니다. 지금 실제 Build dot-source 순서 그대로 13-role snapshot을 재해석해 원래 실패 지점을 직접 검증합니다.
>
> 충돌 수정은 정확히 2파일로 동결됐습니다. 양 호스트 provenance `84/84`, pipeline `291/291`, 실제 Build 로드 순서의 13-role snapshot이 모두 통과했고 독립 리뷰도 CLEAN입니다. 이 두 파일만 커밋한 뒤 또 다른 새 detached worktree에서 full Distribution을 다시 시작합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/DistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1'); git rev-parse --short=8 HEAD; git diff --numstat -- $files; git diff --check -- $files; git diff --cached --check; git status --short -- $files`
> - Ran `$files=@('LMC_Library/LMC_API/DistributionToolchainProvenance.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolchainProvenance.ps1'); git add -- $files; git diff --cached --name-only; git diff --cached --numstat; git diff --cached --check; git commit -m "fix(distribution): isolate toolchain process quoting"`
>
> </details>
>
> 함수 충돌 수정은 `5e53865`로 2파일만 커밋했습니다. 지금 또 다른 새 clean detached worktree/candidate로 full Distribution을 처음부터 실행합니다. 동시에 post-preflight 경로를 읽기 전용으로 선감사해 다음 load-order/EOL 계열 blocker가 있는지도 병렬 확인합니다.
>
> 새 run은 PS5의 앞 5개 suite를 지나 `UdpCallback`에 진입했습니다. candidate/stage/lock은 아직 0이고, 이번에도 preflight 전체가 끝나기 전에는 배포 트랜잭션을 시작하지 않습니다.
>
> PS5 장시간 suite는 CPU 약 120초까지 정상 진행 중이며 오류가 없습니다. 현재 run의 canonical/manual 사전 해시는 직전 clean run과 동일하게 고정됐습니다.
>
> PS5 7/7은 완료되어 PS7로 전환했습니다. 병렬 정적 감사도 prepared inputs, manual/toolchain fingerprint, solution/EOL, candidate reference 제거 경로까지 구체적 blocker 없이 CLEAN 상태입니다.
>
> PS7 `UdpCallback`에 진입했고 정상 계산 중입니다. 이 장시간 gate 뒤에는 ControlHandleRequest 하나만 남으며, 통과하면 이번에는 수정한 production toolchain snapshot과 실제 build 경로로 넘어갑니다.
>
> PS7 장시간 suite는 CPU 약 142초까지 전진했고 오류가 없습니다. post-preflight 전체 경로의 정적 선감사도 최종 CLEAN으로 끝났습니다.
>
> PS7 `UdpCallback`은 CPU 약 258초까지 정상입니다. 아직 candidate가 없으므로 이 장시간 검증이 실패하더라도 canonical 배포물은 그대로 보존되는 구조입니다.
>
> PS7 마지막 장시간 gate는 CPU 약 384초까지 진행했습니다. 예상 정상 소요 안쪽이며, 종료 코드를 기다리고 있습니다.
>
> 중요한 진전입니다. mandatory `14/14`가 끝났고 이전 Python quoting blocker도 통과해 transaction lock이 생성됐습니다. 아직 candidate는 없지만 이제 prepared-input/toolchain snapshot 이후 실제 build 경로에 진입했습니다.
>
> transaction stage가 생성됐고 현재 `LasalMotionControlLib.Tests.csproj /t:RunTests /p:Configuration=Debug`와 test EXE가 실행 중입니다. candidate는 promotion 전까지 여전히 absent이며, 이전 quoting 결함은 실제 production 경로에서도 해소됐습니다.
>
> Debug RunTests 내부에서 현재 `Verify-LasalContract.ps1 -SourceOnly`가 CPU 약 178초로 정상 실행 중입니다. stage와 lock만 활성 상태이고 candidate는 아직 없으며, 새 blocker는 없습니다.
>
> 이번에는 실제 Debug RunTests까지 갔지만 그 안의 `Verify-LasalContract.ps1 -SourceOnly`에서 STOP했습니다. 현재 표면 메시지는 MSBuild wrapper뿐이므로, 우선 stage/lock/canonical 상태를 감사한 뒤 동일한 focused child 명령을 단독 실행해 첫 내부 blocker를 정확히 뽑겠습니다.
>
> 실패는 promotion 이전이라 canonical 변경 가능성은 낮지만, 추정으로 처리하지 않고 전후 tree/manual 해시와 stage·lock 정리를 확인 중입니다. focused 재현은 full build가 아니라 동일 SourceOnly verifier 한 번만 실행해 내부 오류를 노출하는 진단입니다.
>
> focused SourceOnly 진단이 PS5.1에서 실제 csproj 인자 그대로 실행 중입니다: `Phase5TransportClean`, `IntegratedReadOwnerDormant`, `ExpectedSdoWriteAxis=1`. 900초 제한을 두었고 stdout/stderr를 분리해 첫 내부 blocker만 확인합니다.
>
> 내부 blocker는 코드 결함이 아니라 외부 상태입니다. exact 오류는 `LASAL2 must be closed ... running PID 27236`입니다. 이 run은 LASAL IDE가 열린 상태라 repository snapshot을 의도적으로 거부했습니다. 해당 PID를 종료하거나 우회하지 않고, 시작 시각·창 상태를 읽기 전용으로 확인한 뒤 canonical/잔여물 감사를 마칩니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p=Get-Process -Id 27236 -ErrorAction SilentlyContinue; if($p){ $c=Get-CimInstance Win32_Process -Filter 'ProcessId=27236'; [pscustomobject]@{Id=$p.Id;Name=$p.ProcessName;StartTime=$p.StartTime;Responding=$p.Responding;MainWindowTitle=$p.MainWindowTitle;Path=$p.Path;ParentProcessId=$c.ParentProcessId;CommandLine=$c.CommandLine} | Format-List } else {'PID27236 absent'}`
>
> </details>
>
> 현재 메인 프로젝트를 연 LASAL2(PID 27236)가 실행 중이어서 release snapshot은 정책상 진행할 수 없습니다. 저장 중인 작업이 있다면 보존한 뒤 LASAL IDE를 닫아 주세요. 저는 프로세스를 종료하지 않고, 그동안 원래 Debug 실패의 정적 원인과 남은 로그 단서를 계속 조사하겠습니다.
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `$root='C:\work\Elmo\Elmo_Master_release_verify_5e53865_20260812_a'; Get-ChildItem -LiteralPath $root -Recurse -Force -File | Where-Object { $_.LastWriteTime -ge [datetime]'2026-08-12T08:30:00' -and $_.LastWriteTime -le [datetime]'2026-08-12T08:35:30' } | Sort-Object LastWriteTime | Select-Object LastWriteTime,Length,FullName | Format-Table -AutoSize`
> - Ran `$root='C:\work\Elmo\Elmo_Master_release_verify_5e53865_20260812_a'; Get-ChildItem -LiteralPath $root -Recurse -Force -File | Where-Object { $_.LastWriteTime -ge [datetime]'2026-08-12T08:30:00' -and $_.LastWriteTime -le [datetime]'2026-08-12T08:35:30' } | Sort-Object LastWriteTime | ForEach-Object { '{0:O}|{1}|{2}' -f $_.LastWriteTime,$_.Length,$_.FullName }`
> - Ran `$log=Join-Path $env:TEMP 'Lasal2.log'; if(Test-Path -LiteralPath $log){ Get-Item -LiteralPath $log | Select-Object FullName,Length,CreationTime,LastWriteTime | Format-List; Get-Content -LiteralPath $log -Tail 40 } else {'no Lasal2.log'}`
> - Ran `$log=Join-Path $env:TEMP 'Lasal2.log'; Select-String -LiteralPath $log -Pattern '^\[08:3[0-5]:' | Select-Object -Last 80 | ForEach-Object { $_.Line }`
> - Ran `$p=Get-Process -Id 27236 -ErrorAction SilentlyContinue; if($p){[pscustomobject]@{Id=$p.Id;Start=$p.StartTime;Responding=$p.Responding;Title=$p.MainWindowTitle;CPU=$p.CPU}|Format-List}else{'PID27236 absent'}`
> - Ran `$log=Join-Path $env:TEMP 'Lasal2.log'; $lines=Get-Content -LiteralPath $log; $idx=0; for($i=0;$i -lt $lines.Count;$i++){ if($lines[$i] -match '\[08:38:.*P:27236'){ $idx=$i; break } }; $start=[Math]::Max(0,$idx-60); for($i=$start;$i -lt [Math]::Min($lines.Count,$idx+10);$i++){ $lines[$i] }`
>
> </details>
>
> 원래 08:34 실패의 내부 오류를 Windows PowerShell Operational event에서 복구했습니다: `Phase5TransportClean TCPMotionInterface generated server/client/data counts are not 4/3/0.` LASAL IDE는 후속 focused 실행만 오염시켰고 원인은 아닙니다. 실제 결함은 UDP verifier가 `Auto`에서 TerminalWake 상태를 올바르게 감지해도 상위 wrapper가 여전히 `Auto`를 사용해 구형 3-client topology를 기대하는 상태 전파 누락입니다.
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $c=Get-Content $p; foreach($range in @(@(28400,28470),@(30090,30170))){$s=$range[0];$e=$range[1];"---$s-$e---";for($i=$s;$i -le $e;$i++){ '{0}:{1}' -f $i,$c[$i-1] }}; rg -n "WrapperUdpCallbackExpectedState|UdpCallbackExpectedState|TerminalWakeBrokerCandidate|generated server/client/data counts" $p`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $c=Get-Content $p; for($i=30155;$i -le 30275;$i++){ '{0}:{1}' -f $i,$c[$i-1] }`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; rg -n "ProductionApproved|NeedsRebaseline|\[pscustomobject\]|return \[pscustomobject\]" $p | Select-Object -Last 80`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content $p; for($i=13820;$i -le 13880;$i++){ '{0}:{1}' -f $i,$c[$i-1] }`
> - Ran `rg -n "WrapperUdpCallback|UdpCallbackExpectedState|Phase5TransportClean TCPMotionInterface generated server/client" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests -g '*.ps1' -g '*.cs' -g '*.csproj'`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; rg -n "SelfTestOnly|Wrapper.*SelfTest|Invoke-Lasal.*SelfTest" $p | Select-Object -First 180`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $c=Get-Content $p; for($i=26420;$i -le 26630;$i++){ '{0}:{1}' -f $i,$c[$i-1] }`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p=Get-Process -Id 27236 -ErrorAction SilentlyContinue; if($p){'LASAL2_ACTIVE|'+$p.Id+'|'+$p.StartTime.ToString('O')+'|'+$p.Responding+'|CPU='+[math]::Round($p.CPU,1)}else{'LASAL2_CLOSED'}`
>
> </details>
>
> LASAL2가 이제 종료된 것을 확인했습니다. PID를 제가 종료한 것이 아닙니다. 외부 간섭이 없는 상태에서 retained clean worktree의 동일 SourceOnly 명령을 한 번 재실행해 `Auto` 상태 전파 결함을 확정한 뒤 수정에 들어갑니다.
>
> LASAL2가 새 PID 26868로 다시 열려 있어 focused verifier는 시작하지 않았습니다. 사용자 작업을 방해하지 않도록 종료·우회하지 않습니다. 원래 오류는 Operational event로 이미 정확히 복구됐으므로, 외부 IDE와 무관한 순수 state-parser 수정·단위 fixture부터 진행하고 SourceOnly 재검증은 IDE가 안정적으로 닫힌 뒤 수행합니다.

>
> - Ran `git status --short -- LMC_Library/LMC_API; Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,CPU,StartTime,Path | Sort-Object StartTime | Format-Table -AutoSize`
> - Ran `$p='LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; [pscustomobject]@{Exists=(Test-Path $p);Now=(Get-Date -Format o)}; git status --short -- LMC_Library/LMC_API`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API; Get-Date -Format o`
>
> </details>
>
> 구현 계약을 동결했고 이제 패치 단계로 넘어갔습니다. 범위는 새 preflight 1개와 builder/pipeline-test 2개, 총 3파일입니다. 코드 작성 후 PS5.1·PS7 단위검증, 실제 12-suite 실행, 독립 adversarial review 순으로 진행하며 전체 예상은 약 1.5~2시간입니다. 동시에 그 다음 PC-only 설계 공백도 읽기 전용으로 선별하고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API; if(Test-Path 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'){Get-Item 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1' | Select-Object Length,LastWriteTime}`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API; Get-Date -Format o`
> - Ran `git status --short -- LMC_Library/LMC_API; if(Test-Path 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'){Get-Item 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1' | Select-Object Length,LastWriteTime}`
>
> </details>
>
> 첫 구현 파일이 생성됐습니다. 다음 단계까지도 선별했습니다. preflight 완료 후에는 배포에 실제 사용된 `MSBuild/csc/Python/docx/pypdf/git/PowerShell`의 버전·해시가 현재 manifest와 승격 fingerprint에 묶이지 않는 P0-D provenance 공백을 PC-only로 닫겠습니다. 우선 지금은 preflight 3파일을 완성·검증하는 데 집중합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$f='LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; Get-Content -LiteralPath $f -TotalCount 120; rg -n "^function |WorkerSuite|PSModulePath|Timeout|Suite|Get-FileHash|Digest|Monitor|ProcessStartInfo|Kill|PASS LMC" -- $f | Select-Object -First 240`
> - Ran `$f='LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1';$c=Get-Content $f; foreach($range in @(@(104,230),@(227,400),@(400,560),@(650,950))){$range[0]..$range[1] | % { '{0,5}: {1}' -f $_,$c[$_-1] }}`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$f='LMC_Library/LMC_API/DistributionSemanticPolicy.ps1';$c=Get-Content $f; 516..610 | % { '{0}: {1}' -f $_,$c[$_-1] }; 698..830 | % { '{0}: {1}' -f $_,$c[$_-1] }`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API; git diff --stat -- LMC_Library/LMC_API; Get-Item 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1' | Select-Object Length,LastWriteTime`
> - Ran `git diff -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1','LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'); foreach($f in $files){$t=$null;$e=$null;[System.Management.Automation.Language.Parser]::ParseFile((Resolve-Path $f),[ref]$t,[ref]$e)|Out-Null; "$f errors=$($e.Count) tokens=$($t.Count)"; $e | % Message}`
> - Ran `git status --short -- LMC_Library/LMC_API; git diff --stat -- LMC_Library/LMC_API; Get-Date -Format o`
>
> </details>
>
> 초안 정적검토에서 실제 blocker를 잡았습니다. transaction은 승격 직전에 fingerprint provider를 `preparedInputs=null`로 다시 부르므로 null 분기가 없으면 모든 실제 배포가 실패하고, worker가 module path를 격리하기 전에 `Join-Path`를 부르면 오염된 사용자 모듈이 먼저 자동 로드될 수 있습니다. 둘 다 수정 요청했고, suite 순서 exact 비교와 timeout 출력 상한 fixture도 함께 보강 중입니다.
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API; git diff -- LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | Select-Object -First 220`
> - Ran `git diff -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 | Select-Object -Last 150`
> - Ran `rg -n "Get-LmcReleaseInputTreeSha256" LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API -g '*.ps1'`
> - Ran `Get-Content 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1' -TotalCount 42; rg -n "ExactEvidence|EvidenceLine|Write-Host.*DistributionTooling|12/12|MyInvocation.MyCommand.Path|PreflightScriptPath|WaitForExit|Kill|Bounded|Compare-Object" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1`
> - Ran `$f='LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1';$c=Get-Content $f; 110..285 | % { '{0}: {1}' -f $_,$c[$_-1] }; 720..765 | % { '{0}: {1}' -f $_,$c[$_-1] }; 970..1065 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; 550..610 | % { '{0}: {1}' -f $_,$c[$_-1] }; 271..335 | % { '{0}: {1}' -f $_,$c[$_-1] }; 1060..1080 | % { '{0}: {1}' -f $_,$c[$_-1] }`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; 597..728 | % { '{0}: {1}' -f $_,$c[$_-1] }`
>
> </details>
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `$c=Get-Content 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1'; 570..590 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `git diff --stat -- LMC_Library/LMC_API; Get-Item 'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1' | Select Length,LastWriteTime; Get-Date -Format o`
> - Ran `git diff -- LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | rg -n "^\+\s*#|^\+\s*function |tooling|Preflight|PSModule|timeout|host|mutat|assertions" | Select-Object -First 240`
> - Ran `$f='LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1';$c=Get-Content $f; 1940..2440 | % { if($_ -le $c.Count){'{0}: {1}' -f $_,$c[$_-1]} }`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'; 650..1160 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'; 300..650 | % { '{0}: {1}' -f $_,$c[$_-1] }`
>
> </details>
>
> 테스트 fixture도 약 465줄 추가됐고 지금 비공허성까지 검토 중입니다. 단순히 “예외 문구가 나왔다”만 확인하지 않고, 첫 fingerprint는 통과하고 두 번째 승격 직전 fingerprint에서 실제로 digest drift가 차단되는지, timeout child가 남지 않는지, candidate·canonical·stage·lock 불변이 유지되는지를 직접 고정하도록 보완하고 있습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$p=(Resolve-Path 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1').Path; & powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $p 1>$env:TEMP\lmc_preflight_noop_out.txt 2>$env:TEMP\lmc_preflight_noop_err.txt; $code=$LASTEXITCODE; [pscustomobject]@{Exit=$code;Stdout=(Get-Content $env:TEMP\lmc_preflight_noop_out.txt -Raw -ErrorAction SilentlyContinue);Stderr=(Get-Content $env:TEMP\lmc_preflight_noop_err.txt -Raw -ErrorAction SilentlyContinue)} | Format-List; Remove-Item -LiteralPath $env:TEMP\lmc_preflight_noop_out.txt,$env:TEMP\lmc_preflight_noop_err.txt -Force -ErrorAction SilentlyContinue`
> - Ran `& 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File (Resolve-Path 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1').Path; Write-Output "EXIT=$LASTEXITCODE"`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API; git status --short -- LMC_Library/LMC_API; Get-Date -Format o`
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-10)} | Select-Object Id,ProcessName,CPU,StartTime,Path | Format-Table -AutoSize`
>
> </details>
>
> 날짜가 바뀐 현재, 구현은 3파일 약 580줄 규모까지 진행됐고 PS5.1·PS7 단위시험이 시작됐습니다. 정상 실행과 별도로 host 누락/위장/중복, evidence 누락·중복, timeout tree kill, 대량 stdout/stderr, module-path poison, pre/post fingerprint drift를 모두 실패 fixture로 돌리고 있습니다.
>
> <details><summary>Ran 8 commands</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-15)} | Select-Object Id,ProcessName,CPU,StartTime | Sort StartTime | Format-Table -AutoSize; git diff --stat -- LMC_Library/LMC_API`
> - Ran `git diff -- LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1`
> - Ran `rg -n "Pipeline.*EvidenceLine|235 distribution|192 distribution|PASS: .*distribution pipeline" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1; git diff --stat -- LMC_Library/LMC_API`
> - Ran `rg -n "Elapsed|host=|PS5=|PS7=|taskkill|WaitForExit|StandardOutput.Length" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | Select-Object -First 120`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; 230..330 | % { '{0}: {1}' -f $_,$c[$_-1] }; 1045..1085 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `rg -n "Label =|Name = 'Windows|Name = 'Power|Resolve-LmcDistributionPowerShellHost" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 | Select-Object -First 80`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; 490..515 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-10)} | Select-Object Id,ProcessName,CPU,StartTime | Sort StartTime | Format-Table -AutoSize; git diff --stat -- LMC_Library/LMC_API`
>
> </details>
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; 80..112 | % { '{0}: {1}' -f $_,$c[$_-1] }; git diff --check -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1; git diff --cached --check`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; 131..205 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'; 650..750 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `rg -n "duplicate.*evidence|occurrence|twice|Duplicate" LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | Select-Object -First 50`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-20)} | Select-Object Id,ProcessName,CPU,StartTime,Path | Sort StartTime | Format-Table -AutoSize`
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-20)} | Select-Object Id,ProcessName,CPU,StartTime | Sort StartTime | Format-Table -AutoSize`
>
> </details>
>
> 실제 aggregate는 현재 PS5.1의 가장 긴 UDP 296/296 구간을 정상 수행 중입니다. 단위시험은 양쪽 host에서 235/235까지 통과했고, 실제 gate는 여기서 같은 검증을 순차적으로 다시 실행해 총 12/12를 증명합니다. 실행 중에는 파일을 동결했으며, 완료 후 duplicate-evidence fixture 1건을 추가한 뒤 짧은 재검증과 최종 감사를 하겠습니다.
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-25)} | Select-Object Id,ProcessName,CPU,StartTime | Sort StartTime | Format-Table -AutoSize; Get-Date -Format o`
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-30)} | Select-Object Id,ProcessName,CPU,StartTime | Sort StartTime | Format-Table -AutoSize`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; 535..600 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-30)} | Select-Object Id,ProcessName,CPU,StartTime | Sort StartTime | Format-Table -AutoSize; Get-Date -Format o`
> - Ran `rg -n "selectedToolingSnapshot|ValidatedToolingSnapshot|preparedInputs.*null|null.*prepared|ProviderCalls|tooling.*fingerprint|ValidatePrepared" LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | Select-Object -First 100`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'; 400..430 | % { '{0}: {1}' -f $_,$c[$_-1] }; 815..870 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `Get-Process -Id 2592 -ErrorAction SilentlyContinue | Select-Object Id,CPU,StartTime,Responding; Get-Date -Format o`
>
> </details>
>
> 첫 12-suite 실제 실행은 PS5.1 6개를 모두 끝냈고 PS7의 긴 UDP 검증만 진행 중이며 그 뒤 HandleRequest 1개가 남습니다. 이 실행 결과를 확보한 뒤 exact suite path/evidence tuple과 duplicate-evidence fixture를 추가하므로, 최종 frozen bytes에 대해서는 문서화된 진입점인 Windows PowerShell 5.1 parent에서 12/12를 다시 실행합니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API; Get-Date -Format o`
> - Ran `git diff --stat -- LMC_Library/LMC_API; rg -n "expectedSpecifications|Expected.*RelativePath|EvidenceLine.*drift|duplicate.*evidence" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | Select-Object -First 80`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; 590..680 | % { '{0}: {1}' -f $_,$c[$_-1] }; rg -n "DUPLICATE|duplicate|evidence occurrence" LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | Select-Object -First 30`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'; 460..545 | % { '{0}: {1}' -f $_,$c[$_-1] }; 675..715 | % { '{0}: {1}' -f $_,$c[$_-1] }`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-10)} | Select Id,ProcessName,CPU,StartTime | Sort StartTime | Format-Table -AutoSize; rg -n "PASS: [0-9]+ distribution|EvidenceLine = 'PASS: [0-9]+" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'; 515..590 | % { '{0}: {1}' -f $_,$c[$_-1] }`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$root=(Resolve-Path '.').Path.TrimEnd('\'); $script=(Resolve-Path 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1').Path; $home='C:\Windows\System32\WindowsPowerShell\v1.0'; $b64=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($root)); $h64=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($home)); $nonce=[guid]::NewGuid().ToString('N'); & 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -WorkerSuite MethodSize -WorkerRepositoryRootBase64 $b64 -WorkerPowerShellHomeBase64 $h64 -WorkerNonce $nonce; Write-Output "EXIT=$LASTEXITCODE"`
> - Ran `$root=(Resolve-Path '.').Path.TrimEnd('\'); $script=(Resolve-Path 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1').Path; $ps5Root='C:\Windows\System32\WindowsPowerShell\v1.0'; $b64=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($root)); $h64=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($ps5Root)); $nonce=[guid]::NewGuid().ToString('N'); & 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -WorkerSuite MethodSize -WorkerRepositoryRootBase64 $b64 -WorkerPowerShellHomeBase64 $h64 -WorkerNonce $nonce; Write-Output "EXIT=$LASTEXITCODE"`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 dual-host preflight의 마지막 계약을 재검증 중입니다. `MethodSize` worker는 PS5.1/PS7 모두 실제로 outer marker까지 반환하므로 `WorkerTerminates=false`가 맞고, 방금 제기된 반대 finding은 철회됐습니다. 이제 최종 241개 pipeline 회귀와 PS5.1 부모 기준 12-suite 전체 실행만 남았습니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `git status --short; git diff --stat -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1; git diff --check -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1; git diff --cached --check`
> - Ran `$files = @('LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1','LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1','LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1'); foreach ($file in $files) { $item = Get-Item -LiteralPath $file; $hash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash; [pscustomobject]@{Path=$file;Bytes=$item.Length;Sha256=$hash} } | Format-Table -AutoSize`
> - Ran `$files = @('LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1','LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1','LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1'); $rows = foreach ($file in $files) { $item = Get-Item -LiteralPath $file; $hash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash; [pscustomobject]@{Path=$file;Bytes=$item.Length;Sha256=$hash} }; $rows | Format-Table -AutoSize`
> - Ran `rg -n "^(function|param|if \(\$WorkerSuite|\$script:SuiteSpecifications|PASS LMC\.DistributionTooling|Get-LmcDistributionTooling|Invoke-LmcDistributionTooling|WorkerTerminates|EvidenceLine|TimeoutSeconds)" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1; rg -n "ToolingPreflight|ValidatedTooling|validated-tooling-preflight|Test-LmcDistributionToolingHostParity" LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
> - Ran `$p='LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; $lines=Get-Content -LiteralPath $p; $ranges=@(@(1,48),@(531,677),@(678,888),@(889,1100)); foreach($range in $ranges){ "--- $($range[0])-$($range[1]) ---"; $lines[($range[0]-1)..([Math]::Min($range[1]-1,$lines.Count-1))] }; $b='LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; $bl=Get-Content -LiteralPath $b; foreach($range in @(@(1,90),@(530,690),@(930,1050))){ "--- Build $($range[0])-$($range[1]) ---"; $bl[($range[0]-1)..([Math]::Min($range[1]-1,$bl.Count-1))] }`
>
> </details>
>
> 최종 aggregate는 현재 frozen bytes로 PS5.1 부모에서 순차 실행 중입니다. 앞선 동일 계약 실행은 12/12 통과했고 약 13분이 걸렸으며, 이번 재실행은 exact 241 계약이 추가된 최종 증거를 고정하기 위한 것입니다. 작업 파일 해시는 실행 시작값과 그대로 일치합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git check-ignore -v LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1; if ($LASTEXITCODE -ne 0) { 'NOT_IGNORED' }; git check-attr text eol whitespace -- LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1`
>
> </details>
>
> 현재 PS5.1 쪽 앞의 4개 suite를 통과하고, 가장 오래 걸리는 UDP 296/296 self-test를 실행 중입니다. 이후 PS5 HandleRequest와 PS7 6개가 남아 있습니다. 별도 중복 aggregate는 없고 frozen source도 유지 중입니다.
>
> UDP self-test는 아직 정상 실행 중이며 timeout이나 오류 출력은 없습니다. 이 검증은 의도적으로 양쪽 PowerShell에서 전체 296개 fixture를 실행하므로 현재 tranche에서 가장 긴 단계입니다.
>
> 소스·해시는 계속 동결 상태이고, aggregate child가 CPU를 사용하며 진행 중입니다. 900초 per-suite fail-closed timeout과 process-tree 정리 경계 안에서 기다리고 있습니다.
>
> 변경 없음: PS5 UDP 검증이 계속 진행 중이고 예외나 timeout 신호는 없습니다. 완료되면 per-suite 결과를 누락 없이 확인한 뒤 PS7 구간으로 넘어갑니다.
>
> PS5.1 6/6은 완료됐고 PS7도 앞의 4개를 완료했습니다. 지금 마지막 장시간 단계인 PS7 UDP 296/296을 실행 중이며, 그 뒤 HandleRequest 13/13 하나만 남습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n "distribution|Distribution|PowerShell|PS5|PS7|tooling|preflight|Gate D|actual EXE|manifest|publish" LMC_Library/LMC_API/README.md docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md | Select-Object -Last 140`
>
> </details>
>
> PS7 UDP self-test가 계속 정상 진행 중입니다. 다른 검증·편집은 겹치지 않게 중단해 두었고, 완료 전까지 4개 파일의 frozen hash를 유지합니다.
>
> 현재까지 오류 출력이나 자식 프로세스 이탈은 없습니다. PS7 UDP는 이전 실측 약 8.5분인 장기 fixture라, 정상 범위에서 진행 중입니다.
>
> 최종 aggregate는 여전히 PS7 UDP 구간입니다. timeout은 900초로 고정되어 있고, 실패 시 process tree를 종료하고 PASS로 오인하지 않도록 설계되어 있습니다.
>
> 변경 없음: 장기 UDP self-test가 frozen verifier를 사용해 계속 실행 중이며, 코드·문서·스테이지에는 새 변경이 없습니다.
>
> PS7 UDP 검증은 예상 소요 범위 안에서 계속 진행 중입니다. 완료 신호가 오면 마지막 HandleRequest 13/13과 aggregate terminal line을 즉시 대조하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-20) } | Select-Object Id,ProcessName,StartTime,CPU,Responding | Sort-Object StartTime | Format-Table -AutoSize`
>
> </details>
>
> 첫 최종 aggregate는 12/12 자체는 통과했지만 최종 digest 재계산에서 실제 P1을 발견했습니다. 같은 92개 파일인데 PS5.1과 PS7의 `Sort-Object` 문화권 정렬 차이로 digest가 달랐습니다. 이 결과는 최종 PASS로 사용하지 않고, ordinal 정렬로 고친 뒤 양 host digest 동일 fixture와 전체 aggregate를 다시 실행합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n "Sort-Object|Select-Object -Unique|Compare-Object" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1`
>
> </details>
>
> 현재 host-independent fingerprint 보정을 적용 중입니다. monitored 92개 경로의 dedup·정렬뿐 아니라 transaction input-tree canonical order까지 현재 culture 의존이 남지 않는지 함께 점검하고 있습니다. 표시용 정렬은 건드리지 않고 해시에 들어가는 순서만 ordinal로 고정합니다.
>
> 범위를 확정했습니다. 이번 tranche에서는 preflight snapshot과 release input-tree 해시의 canonical ordering까지 ordinal로 고정합니다. Manifest artifact ordering은 별도 schema 3/toolchain provenance 작업과 맞물리므로 다음 tranche에서 cross-host determinism으로 함께 닫겠습니다.
>
> ordinal 보정과 회귀 fixture 추가가 진행 중입니다. 첫 12/12 run은 원인 발견용 증거로만 보존하고, 새 digest가 PS5.1/PS7에서 byte-identical하지 않으면 최종 승인하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1','LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1','LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1'); $rows=foreach($file in $files){$i=Get-Item -LiteralPath $file;[pscustomobject]@{Path=$file;Bytes=$i.Length;SHA256=(Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash}}; $rows | Format-List; git diff --numstat -- $files; git diff --check -- $files; git diff --cached --check`
>
> </details>
>
> ordinal-final 후보가 동결됐습니다. 양쪽 pipeline은 이제 245/245이고, 실제 production 92-record sequence와 digest가 PS5.1/PS7에서 동일하다는 fixture도 통과했습니다. 현재 독립 reviewer가 culture-dependent 정렬 잔존 여부를 확인한 뒤 마지막 12/12 재실행을 승인할 단계입니다.

> - Ran `git status --short | Select-Object -First 20`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short | Select-Object -First 15; git diff --stat -- LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1`
>
> </details>
>
> 추가로 size ratchet의 실제 fail-open도 확인했습니다. `ReserveAxisOwnership`와 `RtWork`가 이미 줄었는데 baseline은 옛 큰 값이라 각각 최대 2,149바이트와 약 0.5KB 재증가를 허용하고 있었습니다. 현재 exact 크기로 baseline을 낮추고, 남은 세 debt의 raw/LF/CRLF 각각 `+1` 회귀를 모두 거부하는 회귀를 추가합니다.
>
> <details><summary>Ran 8 commands</summary>
>
> - Ran `Get-Content -LiteralPath LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 | Select-Object -First 100; Get-Content -LiteralPath LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 | Select-Object -Skip 365 -First 150`
> - Ran `git status --short | Select-Object -First 20; git diff --stat -- LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1`
> - Ran `Get-Content -LiteralPath LMC_Library\LMC_API\DistributionPipeline.ps1 | Select-Object -Skip 1 -First 160; rg -n "Get-LmcDistributionManualWorktreeState" -n LMC_Library\LMC_API\DistributionPipeline.ps1`
> - Ran `git diff -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1`
> - Ran `rg -n "reserveBaseline" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1`
> - Ran `rg -n "self-test.*8/8|8/8.*self-test|method-size|size budget|77731|72907" docs\architecture\ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md docs\architecture\LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md LMC_Library\LMC_API\README.md`
> - Ran `git diff --stat -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1; git diff --check -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1`
> - Ran `Get-Process pwsh,powershell -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,CPU,StartTime | Sort-Object StartTime`
>
> </details>
>
> 현재 세 축을 병렬로 닫고 있습니다.
>
> - Distribution `.sln` exact contract: 구현 완료 단계, PS5.1/PS7 회귀 실행 중
> - Method-size baseline: 수정 완료, 양쪽 `16/16` 및 `101/98/3 PASS`
> - Control `HandleRequest` whole-method fence: 850-line body inventory와 적대 fixture 구현 중
>
> 모두 LASAL source·`Classes.lcb`·PLC에는 손대지 않습니다. 새 코드가 고정되면 분리 worktree를 새 HEAD로 갱신해 full candidate를 끝까지 실행합니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `git status --short | Where-Object { $_ -match 'Distribution|Verify-Lasal|Classes' }; git diff --stat -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1`
> - Ran `git diff -- LMC_Library/LMC_API/DistributionPipeline.ps1`
> - Ran `git diff -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
> - Ran `git diff -- LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | Select-String -Pattern '^\+|^@@' | Select-Object -First 280`
> - Ran `powershell.exe -NoProfile -Command "@(Compare-Object -ReferenceObject @('a') -DifferenceObject @('a','a')).Count"; pwsh -NoProfile -Command "@(Compare-Object -ReferenceObject @('a') -DifferenceObject @('a','a')).Count"`
>
> </details>
>
> 독립 리뷰가 실제 빌드 호환 문제를 잡았습니다. 기존 wrapper는 `/p:Platform=AnyCPU`를 강제하지만 `.sln` 구성 이름은 `Any CPU`라 그대로는 `MSB4126`이 납니다. solution build 두 호출만 정확히 `Platform="Any CPU"`를 사용하도록 수정했고, canonical solution의 Debug/Release 구성 검증은 통과했습니다. 최종 적대 리뷰를 이어갑니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1; git status --short | Where-Object { $_ -match 'Verify-LasalContract|Distribution|Classes' }`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg --files test | rg -i "distribution|release|manual|candidate" | Select-Object -First 100; Get-ChildItem -LiteralPath test -Directory | Select-Object Name`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git diff -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 | rg "^\+function |^\+\s*\[switch\]|^\+\s*Assert-|^\+\s*Write-Output|^\+if \(\$Control" | Select-Object -First 240`
> - Ran `$d = git diff -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1; $d | Select-String -Pattern '^\+function ','^\+\s*\[switch\]','^\+\s*Write-Output','^\+if \(\$Control' | Select-Object -First 160`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1'; $line=(Select-String -LiteralPath $p -Pattern '^function Assert-LasalControlHandleRequestMutationFences').LineNumber; Write-Output ('LINE='+$line); Get-Content -LiteralPath $p | Select-Object -Skip ($line-10) -First 260`
>
> </details>
>
> 세 구현이 모두 동결됐습니다. 이제 제가 동일 바이트를 독립 실행합니다: solution pipeline `129/129`, size ratchet `16/16` 및 current scan, Control fence `13/13`을 PS7·PS5.1 양쪽에서 동시에 재현합니다. 그 뒤 리뷰 결과를 반영하고 clean worktree full candidate로 넘어갑니다.
>
> <details><summary>Ran 13 commands</summary>
>
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API\Test-LmcApiDistributionPipeline.ps1`
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master' -RunSelfTest`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master' -RunSelfTest`
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master'`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalCustomMethodSizeBudget.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master'`
> - Ran `& pwsh -NoLogo -NoProfile -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master' -ControlHandleRequestVerifierSelfTestOnly`
> - Ran `& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 -RepositoryRoot 'C:\work\Elmo\Elmo_Master' -ControlHandleRequestVerifierSelfTestOnly`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1'; $line=(Select-String -LiteralPath $p -Pattern '^function Assert-LasalControlHandleRequestMutationFences').LineNumber; Get-Content -LiteralPath $p | Select-Object -Skip ($line+245) -First 365`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1'; $line=(Select-String -LiteralPath $p -Pattern "Name = 'UnexpectedHelperCallAdded'").LineNumber; Get-Content -LiteralPath $p | Select-Object -Skip ($line-5) -First 215`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1'; $line=@(Select-String -LiteralPath $p -Pattern "Name = 'UnexpectedHelperCallAdded'")[0].LineNumber; Get-Content -LiteralPath $p | Select-Object -Skip ($line-5) -First 215`
> - Ran `$d=git diff -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1; $d | Select-String -Pattern 'ControlHandleRequest','Assert-LasalControlHandleRequestMutationFences' | Select-Object -Last 35`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1'; $matches=@(Select-String -LiteralPath $p -Pattern 'Assert-LasalControlHandleRequestMutationFences'); $matches | ForEach-Object { $_.LineNumber.ToString() + ':' + $_.Line.Trim() }; foreach($m in $matches | Select-Object -Skip 1){'---'; Get-Content -LiteralPath $p | Select-Object -Skip ($m.LineNumber-6) -First 18}`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 PC-only 계약 보강 3건(배포 `.sln` 검증, LASAL 메서드 크기 ratchet, `HandleRequest` 의미 fence)은 양쪽 PowerShell 호스트에서 모두 통과했습니다. 지금 독립 리뷰 2건을 회수한 뒤, 변경을 목적별 커밋하고 깨끗한 별도 worktree에서 전체 배포 파이프라인을 실제로 끝까지 돌리겠습니다. PLC/LASAL IDE/Download는 건드리지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git diff --stat; git diff --check; git diff --cached --check`
>
> </details>
>
> 독립 리뷰에서 현재까지 blocker는 없습니다. 남은 확인은 `HandleRequest` 적대 fixture가 의미 변경을 빠뜨리지 않는지에 대한 마지막 false-pass 점검뿐입니다. 그동안 사용자 `Classes.lcb`와 기존 미추적 증거는 제외한 명시적 path만 커밋할 준비를 하고 있습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git branch --show-current; git rev-parse --short=12 HEAD; git diff --check -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1; git diff --name-only -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1`
> - Ran `git add -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1; git diff --cached --name-only; git diff --cached --stat; git diff --cached --check`
> - Ran `git commit -m "test(distribution): validate packaged solution builds"`
>
> </details>
>
> 마지막 적대 리뷰도 CLEAN입니다. 이제 첫 커밋으로 배포 `.sln` 계약을 고정했고(`88f1c57`), 두 번째로 LASAL 소스를 건드리지 않는 정적 verifier 2개만 별도 커밋합니다. 이후 clean detached worktree를 새 HEAD로 전진시켜 전체 배포 빌드를 시작하겠습니다.
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `git diff --check -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1; git add -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1; git diff --cached --name-only; git diff --cached --stat; git diff --cached --check`
> - Ran `git commit -m "test(lasal): pin control request and size contracts"`
> - Ran `git status --short; git log -3 --oneline; git diff --cached --check`
> - Ran `git worktree list --porcelain; git -C C:\work\Elmo\Elmo_Master_release_verify_5c48f25 status --short --untracked-files=all; git -C C:\work\Elmo\Elmo_Master_release_verify_5c48f25 checkout --detach d735446; git -C C:\work\Elmo\Elmo_Master_release_verify_5c48f25 rev-parse --short=12 HEAD; git -C C:\work\Elmo\Elmo_Master_release_verify_5c48f25 status --short --untracked-files=all; Get-FileHash -Algorithm SHA256 -LiteralPath C:\work\Elmo\Elmo_Master_release_verify_5c48f25\output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx, C:\work\Elmo\Elmo_Master_release_verify_5c48f25\output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf | Select-Object Path,Hash; Test-Path -LiteralPath C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Distribution_candidate_clean_d735446`
> - Ran `$paths = @('C:\work\Elmo\Elmo_Master_release_verify_5c48f25\output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx','C:\work\Elmo\Elmo_Master_release_verify_5c48f25\output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf'); foreach ($p in $paths) { $i = Get-Item -LiteralPath $p; $h = (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash; Write-Output "$($i.FullName)|$($i.Length)|$h" }; $s = git -C C:\work\Elmo\Elmo_Master_release_verify_5c48f25 status --porcelain=v1 --untracked-files=all; Write-Output ('STATUS_COUNT=' + @($s).Count)`
> - Ran `Get-Content -LiteralPath LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 -TotalCount 90`
>
> </details>
>
> 두 커밋을 새 HEAD `d735446`에 고정했고, detached worktree는 Git 상태 0이며 DOCX/PDF 입력 해시도 검증값과 정확히 일치합니다. 이제 전체 Distribution 파이프라인을 시작합니다. 이 실행은 C#/PowerShell 빌드·정적 검증과 후보 디렉터리 생성만 수행하며 PLC나 LASAL IDE에는 연결하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 -RepositoryRoot C:\work\Elmo\Elmo_Master_release_verify_5c48f25 -AllowDirty -CandidatePath C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Distribution_candidate_clean_d735446 -ManualDocxPath C:\work\Elmo\Elmo_Master_release_verify_5c48f25\output\doc\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.docx -ManualPdfPath C:\work\Elmo\Elmo_Master_release_verify_5c48f25\output\pdf\LASAL_Motion_Control_API_User_Manual_KO_2.3-candidate.pdf`
>
> </details>
>
> 전체 파이프라인은 현재 정상 실행 중이며 아직 오류 출력은 없습니다. 이 단계는 SDK/WPF/LASAL 정적 계약을 순차 실행하므로 몇 분간 무출력일 수 있습니다. 시간 제한과 후보·잠금 잔여물 정리는 스크립트의 트랜잭션 경계로 묶여 있습니다.
>
> 약 1분 추가 경과했고 계속 오류 없이 실행 중입니다. 지금은 기존 full LASAL/SDK 계약 구간으로 보이며, 여기서 `Classes.lcb` Gate D STOP이 재발하면 우회하지 않고 정확한 첫 blocker와 자동 rollback 상태를 기록하겠습니다.
>
> 계속 무출력 정상 진행입니다. 이전 동일 계약은 약 3분 전후에 Gate D 판정까지 갔으므로, 현재 실행도 그 구간을 통과하는 중입니다. 사용자 작업 폴더가 아니라 별도 clean worktree에서만 빌드 산출물이 생성되고 있습니다.
>
> 전체 배포는 210초 뒤 첫 Debug `RunTests` 단계에서 중단됐습니다. 아직 오류 원문이 wrapper에 가려진 상태라 성공으로 해석하지 않겠습니다. 지금 트랜잭션 rollback·후보 잔여물·Git 상태를 먼저 확인한 뒤, 같은 MSBuild target만 단독 재실행해 실제 첫 실패 메시지를 회수하겠습니다.
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `$root='C:\work\Elmo\Elmo_Master_release_verify_5c48f25'; $candidate='C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Distribution_candidate_clean_d735446'; Write-Output ('HEAD=' + (git -C $root rev-parse --short=12 HEAD)); $status=git -C $root status --porcelain=v1 --untracked-files=all; Write-Output ('STATUS_COUNT=' + @($status).Count); Write-Output ('CANDIDATE_EXISTS=' + (Test-Path -LiteralPath $candidate)); Get-ChildItem -LiteralPath (Join-Path $root 'LMC_Library') -Force | Where-Object { $_.Name -like '*Distribution_candidate_clean_d735446*' -or $_.Name -like '*.staging*' -or $_.Name -like '*.lock*' } | Select-Object FullName,PSIsContainer,Length; Get-Process | Where-Object { $_.ProcessName -match 'MSBuild|dotnet|VBCSCompiler|LasalMotionControlApiExample' } | Select-Object ProcessName,Id,StartTime; rg -n "RunTests|Invoke-LmcMSBuild" LMC_Library\LMC_API\Build-LmcApiDistribution.ps1 LMC_Library\LMC_API\DistributionPipeline.ps1`
> - Ran `$p='LMC_Library\LMC_API\Build-LmcApiDistribution.ps1'; $c=Get-Content -LiteralPath $p; $c[560..625]; $c[806..830]; rg -n "RunTests|RunLasalContract|RunLasalNetworkContract|Exec Command" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj`
> - Ran `rg -n "\$msbuild\s*=|vswhere|MSBuild.exe" LMC_Library\LMC_API\Build-LmcApiDistribution.ps1; $c=Get-Content -LiteralPath LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj; $c[180..230]`
> - Ran `powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 -RepositoryRoot C:\work\Elmo\Elmo_Master_release_verify_5c48f25 -SourceOnly -ControlServiceCheckpoint Phase5TransportClean -TopologyIoCheckpoint FullContract -ExpectedSdoWriteAxis 1`
> - Ran `rg -n "LasalTopologyIoCheckpoint" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj; Get-Content -LiteralPath LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj -TotalCount 40`
> - Ran `powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 -RepositoryRoot C:\work\Elmo\Elmo_Master_release_verify_5c48f25 -SourceOnly -ControlServiceCheckpoint Phase5TransportClean -TopologyIoCheckpoint IntegratedReadOwnerDormant -ExpectedSdoWriteAxis 1`
>
> </details>
>
> 단독 SourceOnly 계약도 현재 오류 없이 진행 중입니다. 앞서 잘못 넣은 checkpoint 값은 즉시 검증 오류로 끝났고, 현재는 csproj의 실제 기본값 `IntegratedReadOwnerDormant`로 동일 조건을 재현하고 있습니다.
>
> SourceOnly verifier는 2분째 계속 진행 중입니다. 새 `HandleRequest` fence는 초반 focused 검증에서 이미 통과했고, 현재는 그 뒤의 기존 전체 static inventory와 Gate D identity 판정 구간입니다.
>
> 실제 첫 blocker를 회수했습니다. 새 변경이 아니라 기존 UDP callback 정적 계약에서 `TerminalWakeBrokerCandidate.CyWork` 전체 token SHA가 현재 추적 소스와 어긋납니다. clean worktree에서도 재현되므로 사용자 `Classes.lcb`와는 무관합니다. 지금 소스 변경인지 verifier 기준 stale인지 Git 이력과 정확한 token diff로 판별한 뒤, 증거 없이 baseline만 올리는 일은 하지 않겠습니다.
>
> <details><summary>Ran 20 commands</summary>
>
> - Ran `rg -n -C 5 "TerminalWakeBrokerCandidate|complete function token stream drifted|CyWork complete" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1; rg -n "TerminalWakeBrokerCandidate|FUNCTION CyWork|FUNCTION_BLOCK|CyWork" Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class -g '*.st'`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; $c[1820..2020]; rg -n "LMCUdpCallbackSender::CyWork|FUNCTION .*CyWork|CyWork" Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st`
> - Ran `rg -n "function New-SyntheticTerminalWakeDerivedSource|New-SyntheticTerminalWakeDerivedSource" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1; $s=Get-Content -LiteralPath Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st; $s[320..470]`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; $c[9000..9180]`
> - Ran `rg -n "function New-SyntheticDerivedSource|FUNCTION VIRTUAL GLOBAL LMCUdpCallbackSender::CyWork" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; $c[8645..8705]; $c[12570..12675]`
> - Ran `rg -n "function Get-CommentInsensitiveTokenStream|function Get-FunctionRecords" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1; $p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; $c[780..930]`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; $c[955..1015]; $c[1325..1405]`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; $c[969..1060]`
> - Ran ``$actualText = [IO.File]::ReadAllText('Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st'); $verifierText = [IO.File]::ReadAllText('LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'); $pat = '(?ms)^FUNCTION VIRTUAL GLOBAL LMCUdpCallbackSender::CyWork\r?$.*?^END_FUNCTION\r?$'; $a = [regex]::Match($actualText,$pat).Value; $ms = [regex]::Matches($verifierText,$pat); Write-Output ('EXPECTED_MATCHES=' + $ms.Count); $e=$ms[0].Value; function Tokens([string]$t) { $s=$t.Replace("`r`n","`n").Replace("`r","`n"); $s=[regex]::Replace($s,'(?s)\(\*.*?\*\)',' '); $s=[regex]::Replace($s,'(?m)//[^\r\n]*',' '); return @([regex]::Matches($s,'"(?:[^"]|"")*"|0[xX][0-9A-Fa-f]+|[0-9]+#[A-Za-z0-9_]+|\$[A-Za-z_@][A-Za-z0-9_@]*|#[A-Za-z_@][A-Za-z0-9_@]*|[A-Za-z_@][A-Za-z0-9_@]*|[0-9]+(?:\.[0-9]+)?|:=|\+=|-=|\*=|/=|<>|<=|>=|::|\.\.|=>|[^\s]') | ForEach-Object {$_.Value}) }; $at=@(Tokens $a); $et=@(Tokens $e); Write-Output ('ACTUAL_TOKENS=' + $at.Count); Write-Output ('EXPECTED_TOKENS=' + $et.Count); $n=[Math]::Max($at.Count,$et.Count); for($i=0;$i -lt $n;$i++){ $av=if($i -lt $at.Count){$at[$i]}else{'<END>'}; $ev=if($i -lt $et.Count){$et[$i]}else{'<END>'}; if($av -cne $ev){$lo=[Math]::Max(0,$i-12);$hi=[Math]::Min($n-1,$i+30); Write-Output ('FIRST_DIFF=' + $i + '|ACTUAL=' + $av + '|EXPECTED=' + $ev); for($j=$lo;$j -le $hi;$j++){ $aa=if($j -lt $at.Count){$at[$j]}else{'<END>'}; $ee=if($j -lt $et.Count){$et[$j]}else{'<END>'}; Write-Output (('{0:D4}|A={1}|E={2}' -f $j,$aa,$ee))}; break }}; Write-Output '---ACTUAL---'; Write-Output $a; Write-Output '---EXPECTED---'; Write-Output $e``
> - Ran `rg -n "DerivedSource\s*=|LMCUdpCallbackSender.st|Get-LasalUdpCallback.*Snapshot|New-LasalUdpCallback" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1 | Select-Object -First 100; rg -n "Verify-LasalUdpCallbackContract|UdpCallback" LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 | Select-Object -First 80`
> - Ran `$p='LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'; $c=Get-Content -LiteralPath $p; $c[0..90]; $c[7720..7930]; $c[13280..13430]`
> - Ran `powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1 -VerifyCurrent -RepositoryRoot C:\work\Elmo\Elmo_Master_release_verify_5c48f25 -ExpectedState TerminalWakeBrokerCandidate`
> - Ran `pwsh -NoProfile -File LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1 -VerifyCurrent -RepositoryRoot C:\work\Elmo\Elmo_Master_release_verify_5c48f25 -ExpectedState TerminalWakeBrokerCandidate`
> - Ran ``@'
> $path = 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'
> $text = [IO.File]::ReadAllText($path)
> $needle = '$expected = Get-CommentInsensitiveTokenStream -Text $expectedByName[$name]'
> if (($text.Split($needle).Count - 1) -ne 1) { throw 'debug injection anchor count drifted' }
> $insert = @"
> `$expected = Get-CommentInsensitiveTokenStream -Text `$expectedByName[`$name]
>         if (`$name -ceq 'CyWork') {
>             Write-Output ('DBG_ACTUAL_LEN=' + `$actual.Length)
>             Write-Output ('DBG_EXPECTED_LEN=' + `$expected.Length)
>             Write-Output ('DBG_ACTUAL_SHA=' + (Get-TextSha256 -Text `$actual))
>             Write-Output ('DBG_EXPECTED_SHA=' + (Get-TextSha256 -Text `$expected))
>             Write-Output ('DBG_ACTUAL=' + `$actual)
>             Write-Output ('DBG_EXPECTED=' + `$expected)
>         }
> "@
> $text = $text.Replace($needle, $insert.TrimEnd())
> $scriptRootLiteral = "'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests'"
> $text = $text.Replace('$PSScriptRoot', $scriptRootLiteral)
> $sb = [ScriptBlock]::Create($text)
> & $sb -VerifyCurrent -RepositoryRoot 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25' -ExpectedState TerminalWakeBrokerCandidate
> '@ | pwsh -NoProfile -Command -``
> - Ran ``$path = 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'
> $text = [IO.File]::ReadAllText($path)
> $needle = '$expected = Get-CommentInsensitiveTokenStream -Text $expectedByName[$name]'
> if (($text.Split($needle).Count - 1) -ne 1) { throw 'debug injection anchor count drifted' }
> $insert = @"
> `$expected = Get-CommentInsensitiveTokenStream -Text `$expectedByName[`$name]
>         if (`$name -ceq 'CyWork') {
>             Write-Output ('DBG_ACTUAL_LEN=' + `$actual.Length)
>             Write-Output ('DBG_EXPECTED_LEN=' + `$expected.Length)
>             Write-Output ('DBG_ACTUAL_SHA=' + (Get-TextSha256 -Text `$actual))
>             Write-Output ('DBG_EXPECTED_SHA=' + (Get-TextSha256 -Text `$expected))
>             Write-Output ('DBG_ACTUAL=' + `$actual)
>             Write-Output ('DBG_EXPECTED=' + `$expected)
>         }
> "@
> $text = $text.Replace($needle, $insert.TrimEnd())
> $scriptRootLiteral = "'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests'"
> $text = $text.Replace('$PSScriptRoot', $scriptRootLiteral)
> $sb = [ScriptBlock]::Create($text)
> & $sb -VerifyCurrent -RepositoryRoot 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25' -ExpectedState TerminalWakeBrokerCandidate``
> - Ran ``$path = 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'
> $text = [IO.File]::ReadAllText($path)
> $needle = '$expected = Get-CommentInsensitiveTokenStream -Text $expectedByName[$name]'
> $insert = @"
> `$expected = Get-CommentInsensitiveTokenStream -Text `$expectedByName[`$name]
>         if (`$name -ceq 'CyWork') {
>             Write-Output ('DBG_ACTUAL_LEN=' + `$actual.Length)
>             Write-Output ('DBG_EXPECTED_LEN=' + `$expected.Length)
>             Write-Output ('DBG_ACTUAL_SHA=' + (Get-TextSha256 -Text `$actual))
>             Write-Output ('DBG_EXPECTED_SHA=' + (Get-TextSha256 -Text `$expected))
>             Write-Output ('DBG_ACTUAL=' + `$actual)
>             Write-Output ('DBG_EXPECTED=' + `$expected)
>             exit 0
>         }
> "@
> if (($text.Split($needle).Count - 1) -ne 1) { throw 'debug injection anchor count drifted' }
> $text = $text.Replace($needle, $insert.TrimEnd())
> $scriptRootLiteral = "'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests'"
> $text = $text.Replace('$PSScriptRoot', $scriptRootLiteral)
> $sb = [ScriptBlock]::Create($text)
> & $sb -VerifyCurrent -RepositoryRoot 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25' -ExpectedState TerminalWakeBrokerCandidate``
> - Ran ````$path = 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalUdpCallbackContract.ps1'
> $text = [IO.File]::ReadAllText($path)
> $needle = "    Assert-ExactInventory ```r`n        -Actual @(`$expectedRecords.Name)"
> if (($text.Split($needle).Count - 1) -ne 0) { $newline = "`r`n" } else { $newline = "`n"; $needle = "    Assert-ExactInventory ```n        -Actual @(`$expectedRecords.Name)" }
> if (($text.Split($needle).Count - 1) -ne 1) { throw ('record debug anchor count=' + ($text.Split($needle).Count - 1)) }
> $insert = @"
>     Write-Output ('DBG_RECORD_COUNT=' + `$expectedRecords.Count)
>     foreach (`$dbgRecord in `$expectedRecords) {
>         Write-Output ('DBG_REC=' + `$dbgRecord.Name + '|LEN=' + `$dbgRecord.Block.Length + '|SHA=' + (Get-TextSha256 -Text `$dbgRecord.Block))
>     }
>     exit 0
> "@
> $text = $text.Replace($needle, $insert + $needle)
> $scriptRootLiteral = "'C:\work\Elmo\Elmo_Master_release_verify_5c48f25\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests'"
> $text = $text.Replace('$PSScriptRoot', $scriptRootLiteral)
> $sb = [ScriptBlock]::Create($text)
> & $sb -VerifyCurrent -RepositoryRoot 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25' -ExpectedState TerminalWakeBrokerCandidate````

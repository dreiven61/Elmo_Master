>
> - Ran `git status --short -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/DistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1`
> - Ran `git diff --unified=3 -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 | Select-Object -First 260`
> - Ran `$paths = git ls-files -- 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/**' 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/**' 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/**' 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcp'; $paths | ForEach-Object { [System.IO.Path]::GetExtension($_).ToLowerInvariant() } | Group-Object | Sort-Object Count -Descending | Format-Table Count,Name -AutoSize; $paths | Where-Object { [System.IO.Path]::GetExtension($_).ToLowerInvariant() -notin @('.st','.h','.c','.cpp','.lcb','.lcn','.xml','.lcp') }`
> - Ran `rg -n -i "\.vov|\.lba|\.lob|Get-ChildItem.*(Class|Network|Include)|FullNetwork|Network aggregate|lcp|lcn|lcb" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 | Select-Object -First 300`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $lines=Get-Content -LiteralPath $p; for($i=7600;$i -le 7695;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
> - Ran `git ls-files -- 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/**'; Get-ChildItem -LiteralPath 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network' -Recurse -File -Force | ForEach-Object { $_.FullName.Substring((Resolve-Path '.').Path.Length+1) }`
> - Ran `git ls-files -- 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcb'; Test-Path -LiteralPath 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcb'; if (Test-Path -LiteralPath 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcb') { Get-Item -LiteralPath 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcb' | Select-Object Length,LastWriteTime }`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $lines=Get-Content -LiteralPath $p; for($i=45;$i -le 80;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `Get-Content -LiteralPath 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1' -TotalCount 35`
> - Ran `git status --short -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1; git diff --stat -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1`
> - Ran `git diff --unified=5 -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1`
>
> </details>
>
> size ratchet의 원인은 확인됐습니다. CRLF source에서는 정규식이 `END_FUNCTION` 뒤의 `\r`만 block에 포함해 LF보다 1바이트, CRLF 투영에서는 2바이트 큰 유령 차이를 만들었습니다. 지금 source를 canonical LF로 한 번만 정규화한 뒤 LF와 보수적 all-CRLF 크기를 계산하도록 바꿔 main mixed-EOL과 clean checkout이 동일한 baseline을 내는지 양쪽 PowerShell에서 검증 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,CPU,StartTime,Responding | Sort-Object StartTime`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1; git diff --check -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1`
>
> </details>
>
> size ratchet portability 수정은 독립 리뷰까지 CLEAN입니다. main mixed-EOL과 clean detached가 PS5.1/PS7 모두 동일한 `101/98/3`과 세 exact tuple을 내고, 9개 `current+1` 차원이 모두 거부됩니다. 이 한 파일을 먼저 목적별 커밋으로 고정한 뒤 문서의 baseline 수치를 새 값으로 맞추겠습니다.
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1'; git diff --check -- $p; git add -- $p; git diff --cached --check -- $p; git diff --cached --stat -- $p; git status --short -- $p; git commit -m "fix(lasal): make size ratchet checkout-stable" -- $p`
> - Ran `git status --short; git log -4 --oneline`
> - Ran `git diff --stat -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1; git diff --check -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1`
> - Ran `git diff --unified=3 -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
> - Ran `rg -n "LasalValidation|validation input|post-populate|Network generated|ONE_Comm|LMCControlCommandService|Classes\.lcb|Networks\.lcb|171" LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | Select-Object -Last 140`
> - Ran `$p='LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'; $lines=Get-Content -LiteralPath $p; for($i=820;$i -le 1175;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
> - Ran `$p='LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'; $lines=Get-Content -LiteralPath $p; for($i=60;$i -le 115;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }; for($i=620;$i -le 690;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
>
> </details>
>
> <details><summary>Edited 4 files, ran 3 commands</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); rg -n "d735446|75829|75249|77210|72907|71437|73287|method-size" $docs`
> - Ran `$p='docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md'; $lines=Get-Content -LiteralPath $p; for($i=268;$i -le 320;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+4 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+7 -5)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+3 -2)
> - Ran `Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-20) } | Select-Object Id,ProcessName,CPU,StartTime,Responding`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $docs; git diff --stat -- $docs; rg -n "d6ddf05|75248|77208|71436|73285|75829|72907" $docs`
>
> </details>
>
> 배포 fingerprint 독립 리뷰에서는 한 가지 보안·정확성 blocker를 잡았습니다. Network 내부만 reparse를 검사하면 상위 `Lasal_PRG`/project 또는 Class·Include·root `.lcp/.lcb`가 junction일 때 저장소 밖 파일을 따라갈 수 있습니다. 전체 검증 입력의 repository-root ancestry와 각 파일을 fail-closed로 검사하고 junction fixture까지 추가한 뒤에만 커밋하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; $lines=Get-Content -LiteralPath $p; for($i=285;$i -le 520;$i++){ if($i -ge 285 -and $i -le 380 -or $i -ge 430 -and $i -le 510){ '{0}:{1}' -f $i,$lines[$i-1] } }`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'); git diff --check -- $files; git diff --cached --check -- $files; git diff --stat -- $files; Get-FileHash -Algorithm SHA256 -LiteralPath $files | Select-Object Path,Hash`
>
> </details>
>
> 배포 fingerprint 보강도 독립 리뷰 CLEAN입니다. 이제 exact project `.lcp/.lcb`, tracked Class·Include·Source, tracked+모든 물리 Network 파일(ignored 8개 포함)을 묶고, 5개 post-populate 변조 시나리오가 모두 promotion을 막습니다. 이 두 파일을 별도 커밋으로 고정하겠습니다.
>
> <details><summary>Edited 4 files, ran 3 commands</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1'); git diff --check -- $files; git add -- $files; git diff --cached --check -- $files; git diff --cached --stat -- $files; git status --short -- $files; git commit -m "test(distribution): bind complete LASAL inputs" -- $files`
> - Ran `rg -n "bf31030" LMC_Library/LMC_API/README.md LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md`
> - Ran `$items=@(@('LMC_Library/LMC_API/README.md',45,68),@('LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md',954,978),@('docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md',972,1002),@('docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md',23,38)); foreach($it in $items){ 'FILE '+$it[0]; $lines=Get-Content -LiteralPath $it[0]; for($i=$it[1];$i -le $it[2];$i++){ '{0}:{1}' -f $i,$lines[$i-1] }; '' }`
> - Wrote `./LMC_Library/LMC_API/README.md` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+7 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+3 -0)
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $docs; git diff --cached --check -- $docs; git diff --stat -- $docs; git status --short -- $docs; rg -n "bf31030|d6ddf05|192/192|75248|77208|71436|73285" $docs`
>
> </details>
>
> 두 새 커밋은 각각 독립 검증을 통과했지만, 서로 합쳐진 최신 HEAD에서 full Distribution을 아직 다시 돌리지는 않았습니다. 새 clean detached worktree를 별도로 만들어 exact manual 입력으로 재실행하겠습니다. 예상 결과는 같은 Gate D STOP이며, 그보다 앞에서 새 fingerprint나 EOL-stable size gate가 깨지면 새 결함으로 처리합니다. 기존 검증 worktree와 사용자 파일은 보존합니다.
>
> 문서 리뷰는 chronology 두 곳을 정확히 잡았습니다. 이전 214초 실행에는 `88f1c57`·`d735446`·`afdf6a3`만 포함됐고, 방금 커밋한 `d6ddf05`·`bf31030`은 포함되지 않았습니다. 지금 최신 HEAD clean detached 재실행이 진행 중이므로 그 실제 결과로 문장을 한 번에 교정하겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git worktree list --porcelain; Get-ChildItem -LiteralPath C:\work\Elmo -Directory -Filter 'Elmo_Master_release_verify_bf31030*' -ErrorAction SilentlyContinue | Select-Object FullName,LastWriteTime; Get-Process powershell,MSBuild,VBCSCompiler -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-15) } | Select-Object Id,ProcessName,CPU,StartTime,Responding`
> - Ran `Get-Process powershell,MSBuild,VBCSCompiler -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-10) } | Select-Object Id,ProcessName,CPU,StartTime,Responding | Sort-Object StartTime`
>
> </details>
>
> 첫 최신 재실행은 candidate 경로를 canonical의 직접 sibling으로 지정하지 않아 즉시 preflight 차단됐고, 두 번째는 `pwsh`가 물려준 `PSModulePath` 때문에 Windows PowerShell 5.1에서 `Get-FileHash` 모듈이 가려진 harness 오염이었습니다. 둘 다 candidate·staging residue 없이 끝났고 저장소 결함은 아닙니다. 지금 환경 상속 없는 direct `powershell.exe` 명령으로 최종 재실행 중입니다.
>
> direct 재실행은 40초를 넘어 계속 진행돼 이전 두 harness/preflight 오류 지점은 통과했습니다. 여기서 기대하는 종료는 약 3~4분 뒤 default UDP Gate D의 승인되지 않은 physical snapshot STOP이며, 더 이른 다른 실패가 나오면 최신 커밋 회귀로 분리해 바로 수정하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,MSBuild,VBCSCompiler -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -gt (Get-Date).AddMinutes(-10) } | Select-Object Id,ProcessName,CPU,StartTime,Responding | Sort-Object StartTime`
>
> </details>
>
> 최신 HEAD 검증도 기대한 경계로 끝났습니다. direct Windows PowerShell full run은 `214.415s` 뒤 Debug `RunTests`에서 exit 1, focused UDP verifier는 `10.320s`에 정확히 `TerminalWakeBrokerCandidate ... no approved physical snapshot ratchet`을 반환했습니다. Candidate·stage·lock은 0, canonical hash와 manual hash는 불변입니다. 이제 문서 chronology를 이 최신 실행으로 교정합니다.
>
> <details><summary>Edited 4 files, ran a command</summary>
>
> - Ran `$items=@(@('LMC_Library/LMC_API/README.md',48,72),@('LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md',956,982),@('docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md',975,1008),@('docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md',25,43)); foreach($it in $items){ 'FILE '+$it[0]; $lines=Get-Content -LiteralPath $it[0]; for($i=$it[1];$i -le $it[2];$i++){ '{0}:{1}' -f $i,$lines[$i-1] }; '' }`
> - Wrote `./LMC_Library/LMC_API/README.md` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+12 -10)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+12 -10)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+7 -6)
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $docs; git diff --cached --check -- $docs; git diff --stat -- $docs; rg -n "214\.415|10\.320|이 세 commit|다시 PASS시킨|재실행하지|bf31030|d6ddf05" $docs`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git log -7 --oneline; git diff --name-only; git diff --cached --name-only`
>
> </details>
>
> 최종 5문서 리뷰도 CLEAN입니다. 과거 `afdf6a3` 실행과 최신 `bf31030` 실행 chronology, size 독립 검증, fingerprint 5개 drift fixture, Gate D STOP·미도달 경계가 모두 소스/로그와 일치합니다. 이 5개만 문서 커밋으로 고정하겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$docs=@('LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $docs; git add -- $docs; git diff --cached --check -- $docs; git diff --cached --stat -- $docs; git status --short -- $docs; git commit -m "docs(api): record checkout-safe release stop" -- $docs`
> - Ran `git status --short; git log -6 --oneline; git diff --check; git diff --cached --check`
>
> </details>
>
> 첫 PC-only tranche는 목적별 3개 커밋으로 닫혔고 main에는 사용자 기존 `Classes.lcb`/미추적 증거만 남았습니다. 다음 설계상 P1은 최근 실제로 두 번 드러난 PowerShell 5.1/7 의미 차이를 배포 시작 전에 잡는 dual-host tooling preflight입니다. Staging 생성 전에 양 host의 pipeline·semantic·manifest·핵심 LASAL focused verifier가 모두 exit 0과 정상 terminal PASS를 내는지 강제하겠습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `Get-ChildItem -LiteralPath 'LMC_Library/LMC_API' -File -Filter 'Test-*.ps1' | Select-Object Name,Length | Sort-Object Name; rg -n "PASS:|TestCount|PolicyCheckCount|assertions|param\(" LMC_Library/LMC_API/Test-*.ps1`
> - Ran `$files=Get-ChildItem -LiteralPath 'LMC_Library/LMC_API' -File -Filter 'Test-*.ps1' | Select-Object -ExpandProperty FullName; rg -n "PASS:|TestCount|PolicyCheckCount|assertions|^param\(" $files`
> - Ran `$files=@('LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1','LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1','LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1'); foreach($f in $files){ 'FILE '+$f; Get-Content -LiteralPath $f -Tail 25 }`
> - Ran `$p='LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; $lines=Get-Content -LiteralPath $p; for($i=1;$i -le 120;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }; for($i=720;$i -le 790;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 다음 PC-only 필수 항목인 “배포 트랜잭션 전에 PowerShell 5.1/7 양쪽 도구 체인을 검증하는 preflight”를 진행 중입니다. 특히 방금 실측된 `PSModulePath` 오염 재발을 차단하고, 후보·스테이징을 만들기 전에 실패하도록 구현과 독립 감사를 병행하고 있습니다. LASAL IDE·PLC·Download와 사용자 `Classes.lcb`는 건드리지 않습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git status --short; git log -1 --oneline; git diff --stat; git diff --cached --stat`
> - Ran `$files=@('LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LMC_API/DistributionPipeline.ps1','LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1','LMC_Library/LMC_API/DistributionSemanticPolicy.ps1','LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1'); foreach($f in $files){Write-Output "FILE $f"; if(Test-Path $f){Get-Item $f | Select-Object Length,LastWriteTime; rg -n "Invoke-LmcMSBuild|Start-Process|powershell|pwsh|Transaction|staging|Candidate|RunTests|SemanticPolicy|ReleaseManifest" -- $f | Select-Object -First 50}}`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$f='LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; $c=Get-Content -LiteralPath $f; 1..125 | ForEach-Object { '{0,5}: {1}' -f $_,$c[$_-1] }; 780..980 | ForEach-Object { '{0,5}: {1}' -f $_,$c[$_-1] }`
> - Ran `$ErrorActionPreference='Stop'; Get-Command powershell.exe,pwsh.exe | Select-Object Name,Source,Version; git check-attr text eol -- LMC_Library/LMC_API/*.ps1; rg -n "RunSelfTest|SelfTestOnly|Test-Lmc.*Policy|PASS: .*assertions|TOTAL .*PASSED" LMC_Library/LMC_API/*.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/*.ps1 | Select-Object -First 200`
> - Ran `rg -n "RunSelfTest|SelfTestOnly|Test-Lmc.*Policy|PASS: .*assertions|TOTAL .*PASSED" LMC_Library/LMC_API LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests -g '*.ps1' | Select-Object -First 240`
>
> </details>
>
> 현재까지 정리된 핵심 조건은 세 가지입니다. 새 preflight는 후보 디렉터리·스테이징·트랜잭션 락 생성 전에 실행하고, `powershell.exe`와 `pwsh.exe`를 각각 깨끗한 환경으로 직접 실행하며, 한 호스트의 실패·누락·시간초과만 있어도 배포를 시작하지 않도록 fail-closed로 고정합니다. 기존 사용자 변경은 그대로 보존 중입니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `Get-Content -LiteralPath 'LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1' -Tail 45; Get-Content -LiteralPath 'LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1' -Tail 20; Get-Content -LiteralPath 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1' -Tail 30; Get-Content -LiteralPath 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1' -Tail 5`
> - Ran `rg -n "PASS.*self|296/296|ControlHandleRequestVerifier|PolicyCheckCount|TestCount|PolicySha256" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalCustomMethodSizeBudget.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1`
> - Ran `rg -n "PASS.*[Ss]elf|PASS.*UDP|Self-test passed|tests passed" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 | Select-Object -Last 20; $c=Get-Content 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; 28272..28300 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; 13740..13760 | % { '{0}: {1}' -f $_,$c[$_-1] }`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `@(Get-Command pwsh.exe -All -CommandType Application) | ForEach-Object { [pscustomobject]@{Name=$_.Name;Source=$_.Source;Path=$_.Path;CommandType=$_.CommandType.ToString()} } | Format-List`
> - Ran `$paths=@(Get-Command pwsh.exe -All -CommandType Application | % Source); foreach($p in $paths){$i=Get-Item -LiteralPath $p -Force; [pscustomobject]@{Path=$p;Length=$i.Length;Attributes=$i.Attributes;LinkType=$i.LinkType;Target=($i.Target -join ';');Version=$i.VersionInfo.FileVersion}} | Format-List`
> - Ran `$paths=@(Get-Command pwsh.exe -All -CommandType Application | ForEach-Object Source); $rows=@(foreach($p in $paths){$i=Get-Item -LiteralPath $p -Force; [pscustomobject]@{Path=$p;Length=$i.Length;Attributes=$i.Attributes;LinkType=$i.LinkType;Target=($i.Target -join ';');Version=$i.VersionInfo.FileVersion}}); $rows | Format-List`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1; if(Test-Path 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'){Get-Item 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1' | Select-Object Length,LastWriteTime}`
> - Ran `$f='LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; $c=Get-Content $f; 500..570 | % { '{0}: {1}' -f $_,$c[$_-1] }; rg -n "Get-LmcReleaseInputFiles|Test-LmcDistributionTooling" -- $f`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API; Get-Date -Format o`
>
> </details>
>
> 감사 과정에서 두 가지 추가 결함 가능성도 선제적으로 막고 있습니다. 정상 PC에는 `pwsh.exe`가 실제 실행 파일과 0바이트 App Execution Alias 두 개로 보이므로 alias를 제거한 뒤 실체만 검증해야 하고, preflight가 검사한 스크립트가 트랜잭션 시작 전에 바뀌는 race를 막기 위해 검증 digest를 release input fingerprint에 묶어야 합니다. UDP 검증은 실제로 PS7에서 약 520초 걸렸기 때문에 오탐 없는 900초 개별 timeout으로 고정할 예정입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$f='LMC_Library/LMC_API/DistributionPipeline.ps1';$c=Get-Content $f; 909..1035 | % { '{0}: {1}' -f $_,$c[$_-1] }`
>
> </details>
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `$f='LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; $c=Get-Content $f; 570..650 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; 648..665 | % { '{0}: {1}' -f $_,$c[$_-1] }`
> - Ran `rg -n "RepositoryRoot|Get-Content|Copy-Item|Resolve-Path|Join-Path \$repositoryRoot|actualSource|actual_current" LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1 | Select-Object -First 160`
> - Ran `$c=Get-Content 'LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1'; 400..460 | % { '{0}: {1}' -f $_,$c[$_-1] }`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API; Get-ChildItem 'LMC_Library/LMC_API' -Filter '*ToolingHostParity*' -Force | Select-Object FullName,Length,LastWriteTime`
>
> </details>
>
> 아직 파일을 건드리지 않은 채 계약을 먼저 고정하고 있습니다. 이유는 이 검사가 자체적으로 약 13분 걸리는 12개 child suite를 돌리므로, 잘못 설계하면 정상 빌드가 항상 timeout되거나 검증한 바이트와 배포에 사용한 바이트가 달라질 수 있기 때문입니다. 현재는 6개 suite×2개 host, host별 고정 timeout, worker 시작 즉시 module path 격리, 검증 digest의 초기·승격 직전 재확인까지 확정했습니다.
>
> <details><summary>Ran 2 commands</summary>

> $preDllHash = (Get-FileHash -LiteralPath $runDll -Algorithm SHA256).Hash
> $gateArgs = @($smokeProject,'/t:RunWpfExecutableRelaunchTest','/p:Configuration=Release','/p:Platform=AnyCPU',("/p:WpfExecutableRelaunchExe=" + $runExe),'/nologo','/verbosity:minimal')
> & $msbuild @gateArgs
> if ($LASTEXITCODE -ne 0) { throw 'Binary-reference Run EXE relaunch gate failed.' }
> $postExeHash = (Get-FileHash -LiteralPath $runExe -Algorithm SHA256).Hash
> $postDllHash = (Get-FileHash -LiteralPath $runDll -Algorithm SHA256).Hash
> $sourceDllHash = (Get-FileHash -LiteralPath $sourceDll -Algorithm SHA256).Hash
> if ($preExeHash -cne $postExeHash -or $preDllHash -cne $postDllHash) { throw 'Run artifacts changed during gate.' }
> if ($postDllHash -cne $sourceDllHash) { throw 'Run SDK differs from canonical Release SDK.' }
> [pscustomobject]@{
>     Result = 'PASS'
>     TempRoot = $tempRoot
>     CandidateProjectReferenceCount = $candidateXml.SelectNodes('//m:ProjectReference',$candidateNs).Count
>     RunExeBytes = (Get-Item -LiteralPath $runExe).Length
>     RunExeSha256 = $postExeHash
>     RunDllBytes = (Get-Item -LiteralPath $runDll).Length
>     RunDllSha256 = $postDllHash
>     ConfigState = if (Test-Path -LiteralPath ($runExe + '.config')) { 'PRESENT' } else { 'ABSENT_TO_ABSENT_PASS' }
> } | Format-List`
>
> </details>
>
> binary-reference 후보도 실제 Run EXE에서 PASS했습니다: ProjectReference `0`, TCP `3/28`, EXE `829AC331…F910F79`, SDK `7D179781…181A7ED8`, config absent 유지입니다. 이제 이 검증만을 위해 만든 고정 임시 디렉터리의 절대 경로와 reparse 0을 재확인한 뒤 삭제합니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `$target = 'C:\work\Elmo\Elmo_Master\.codex_wpf_binary_candidate_20260811'; $resolved = [System.IO.Path]::GetFullPath($target).TrimEnd('\'); if ($resolved -cne $target) { throw "Unexpected candidate path: $resolved" }; if (-not $resolved.StartsWith('C:\work\Elmo\Elmo_Master\', [System.StringComparison]::Ordinal)) { throw 'Candidate path escaped workspace.' }; if (-not (Test-Path -LiteralPath $target -PathType Container)) { throw 'Candidate temp root missing.' }; $reparse = @(Get-ChildItem -LiteralPath $target -Recurse -Force | Where-Object { $_.Attributes -band [System.IO.FileAttributes]::ReparsePoint }); if ((Get-Item -LiteralPath $target -Force).Attributes -band [System.IO.FileAttributes]::ReparsePoint -or $reparse.Count -ne 0) { throw 'Refusing to remove a candidate tree containing a reparse point.' }; Remove-Item -LiteralPath 'C:\work\Elmo\Elmo_Master\.codex_wpf_binary_candidate_20260811' -Recurse -Force; if (Test-Path -LiteralPath $target) { throw 'Candidate temp cleanup failed.' }; 'REMOVED verified disposable binary-candidate build tree; recoverability=not needed (generated from current source)'`
> - Ran `$target = 'C:\work\Elmo\Elmo_Master\.codex_wpf_binary_candidate_20260811'; $resolved = [System.IO.Path]::GetFullPath($target).TrimEnd('\'); $root = Get-Item -LiteralPath $target -Force; $entries = @(Get-ChildItem -LiteralPath $target -Recurse -Force); $reparse = @($entries | Where-Object { $_.Attributes -band [System.IO.FileAttributes]::ReparsePoint }); [pscustomobject]@{ ExpectedExact = ($resolved -ceq $target); InsideWorkspace = $resolved.StartsWith('C:\work\Elmo\Elmo_Master\',[System.StringComparison]::Ordinal); RootReparse = [bool]($root.Attributes -band [System.IO.FileAttributes]::ReparsePoint); DescendantReparseCount = $reparse.Count; EntryCount = $entries.Count; FileBytes = [long](($entries | Where-Object { -not $_.PSIsContainer } | Measure-Object Length -Sum).Sum) } | Format-List`
> - Ran `[System.IO.Directory]::Delete('C:\work\Elmo\Elmo_Master\.codex_wpf_binary_candidate_20260811', $true); if (Test-Path -LiteralPath 'C:\work\Elmo\Elmo_Master\.codex_wpf_binary_candidate_20260811') { throw 'Candidate temp cleanup failed.' }; 'REMOVED verified disposable binary-candidate build tree; recoverability=not needed (generated from current source)'`
> - Ran `$procs = @(Get-Process LasalMotionControlApiExample,LasalApiWpfTestApp.SmokeTests,MSBuild -ErrorAction SilentlyContinue); $probeRoots = @(Get-ChildItem -LiteralPath ([System.IO.Path]::GetTempPath()) -Directory -Force -Filter 'Elmo.WpfExecutableRelaunch.*' -ErrorAction SilentlyContinue); [pscustomobject]@{ RelatedProcessCount=$procs.Count; ProbeTempRootCount=$probeRoots.Count; CandidateTempExists=(Test-Path -LiteralPath 'C:\work\Elmo\Elmo_Master\.codex_wpf_binary_candidate_20260811') } | Format-List; git status --short -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LasalApiWpfTestApp`
>
> </details>
>
> 실제 binary-reference candidate Run EXE까지 독립 PASS했고, 검증용 임시 트리 102개 항목/19.9MB는 reparse 0을 확인한 뒤 제거했습니다. 관련 프로세스와 probe temp 잔여는 모두 0입니다. 이제 7파일 diff를 최종 고정하고 코드 커밋을 만든 뒤, 문서에는 “전체 distribution은 기존 LASAL contract blocker 전 단계에서 중지됐지만 candidate EXE gate 자체는 PASS”로 분리 기록하겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `$files = @('LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/ExecutableRelaunchProbe.cs','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfExecutableRelaunchIntegrationTests.cs','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj'); git diff --check -- $files; git diff --cached --check; foreach ($newFile in @($files | Where-Object { -not (git ls-files --error-unmatch -- $_ 2>$null) })) { $output = @(git diff --no-index --check -- NUL $newFile 2>&1); $errors = @($output | Where-Object { $_ -match '(trailing whitespace|space before tab|blank line at end of file)' }); if ($errors.Count -ne 0) { $errors; exit 1 } }; $astTokens=$null; $astErrors=$null; [void][System.Management.Automation.Language.Parser]::ParseFile((Resolve-Path 'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'),[ref]$astTokens,[ref]$astErrors); if ($astErrors.Count -ne 0) { $astErrors; exit 1 }; [xml](Get-Content -LiteralPath 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj' -Raw) | Out-Null; [xml](Get-Content -LiteralPath 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj' -Raw) | Out-Null; 'FINAL_STATIC_CHECKS=PASS'; git diff --numstat -- $files; foreach ($path in $files) { $item=Get-Item -LiteralPath $path; $hash=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash; "$path|$($item.Length)|$hash" }`
> - Ran `git add -- 'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1' 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs' 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/ExecutableRelaunchProbe.cs' 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj' 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs' 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfExecutableRelaunchIntegrationTests.cs' 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj'; git diff --cached --check; $expected=@('LMC_Library/LMC_API/Build-LmcApiDistribution.ps1','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfExecutableRelaunchIntegrationTests.cs','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/ExecutableRelaunchProbe.cs','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj') | Sort-Object; $actual=@(git diff --cached --name-only) | Sort-Object; $delta=Compare-Object $expected $actual; if ($delta) { $delta | Out-String | Write-Error; exit 1 }; 'STAGED_SCOPE=EXACT_7'; git diff --cached --stat`
> - Ran `git commit -m "test(wpf): gate executable relaunch recovery"`
>
> </details>
>
> 코드 변경은 `cbf2548`로 정확히 7파일만 커밋했습니다. 다음은 문서 정합화입니다. 기존 문서의 “same-process 새 MainWindow라 EXE relaunch/mutex 증거가 아님”을 역사적 한계로 남기고, 새 별도 `1/1` gate가 증명한 범위와 여전히 증명하지 못한 PLC cleanup/readiness를 분리해 갱신하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git show --stat --oneline --summary cbf2548; git status --short; git diff --cached --name-only`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "macro-to-custom boundary closure drifted|baseline axis ownership reserve mutation blocker|compact identity/preemption" -S . --glob '!docs/history/**' --glob '!Lasal_PRG/**/Classes.lcb'`
> - Ran `Get-Content 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1' | Select-Object -Skip 7480 -First 135; Get-Content 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1' | Select-Object -Skip 10140 -First 215`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 실제 EXE의 X 종료→프로세스 종료→동일 EXE 재실행 경로까지 자동 회귀 검증은 통과했고 코드 커밋도 완료했습니다. 지금은 권위 문서 반영과, 전체 배포 파이프라인을 새 테스트 전에 막은 기존 LASAL 정적 계약 오류를 분리 진단하는 마무리 단계입니다. PLC/LASAL/Download는 건드리지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git log -3 --oneline --decorate; git diff --stat; git diff --check; git diff --cached --check`
>
> </details>
>
> 현재 작업 트리는 예상대로 사용자 소유 `Classes.lcb`와 기존 미추적 자료만 남아 있고, reconnect 코드 커밋은 깨끗합니다. 문서 에이전트가 아직 편집 전이라 기다리는 동안 배포 차단 메시지의 생성 조건과 실제 현재값을 독립적으로 대조하겠습니다.
>
> <details><summary>Ran 11 commands</summary>
>
> - Ran `rg -n -C 8 "macro-to-custom boundary closure drifted|baseline axis ownership reserve mutation blocker|ReserveAxisOwnership|macro-to-custom" LMC_Library Lasal_PRG docs test -g "*.ps1" -g "*.md" -g "*.txt"`
> - Ran `rg -n -C 12 "macro-to-custom boundary closure drifted|baseline axis ownership reserve mutation blocker" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(7420,7595),@(10160,10340))){ $s=$range[0]; $e=$range[1]; for($i=$s;$i -le $e;$i++){ '{0}:{1}' -f $i,$lines[$i-1] } }`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $lines=Get-Content -LiteralPath $p; for($i=7320;$i -le 7425;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'; rg -n "^\s*#\s*define|^\s*FUNCTION\s+LMCControlCommandService::ReadAxisRebaseRequiredMask|LSL_IMPLEMENTATION|LSL_MACRO" $p | Select-Object -Last 35`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'; $lines=Get-Content -LiteralPath $p; for($i=675;$i -le 692;$i++){ '{0}:{1}' -f $i,$lines[$i-1] }`
> - Ran `rg -n "function Get-LasalLexicalScanText|function Get-LasalScanText|param\(" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" | Select-Object -First 20`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $l=Get-Content -LiteralPath $p; for($i=1;$i -le 185;$i++){ '{0}:{1}' -f $i,$l[$i-1] }`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'; $text=[IO.File]::ReadAllText((Resolve-Path $p)); $macro=[regex]::Matches($text,'(?im)^[^\S\r\n]*#[^\S\r\n]*define[^\S\r\n]+[A-Za-z_][A-Za-z0-9_]*(?:[^\r\n]*)\r?$'); $hdr=[regex]::Match($text,'(?im)^\s*FUNCTION\s+LMCControlCommandService::ReadAxisRebaseRequiredMask\s*$'); $start=$macro[-1].Index+$macro[-1].Length; $between=$text.Substring($start,$hdr.Index-$start); [pscustomobject]@{MacroCount=$macro.Count;LastMacro=$macro[-1].Value;HeaderSuccess=$hdr.Success;BetweenLength=$between.Length;BetweenEscaped=($between.ToCharArray()|%{ 'U+{0:X4}' -f [int]$_}) -join ',';WhitespaceOnly=($between -match '\A[ \t\r\n]*\z')} | Format-List`
> - Ran ``@'
> $p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
> $text=[IO.File]::ReadAllText((Resolve-Path $p))
> $scan=[regex]::Replace($text,'(?s)\(\*.*?\*\)|//[^\r\n]*|"(?:[^"]|"")*"',{param($m) [regex]::Replace($m.Value,'[^\r\n]',' ')})
> $macro=[regex]::Matches($scan,'(?im)^[^\S\r\n]*#[^\S\r\n]*define[^\S\r\n]+[A-Za-z_][A-Za-z0-9_]*(?:[^\r\n]*)\r?$')
> $headers=[regex]::Matches($scan,'(?im)^\s*FUNCTION(?:\s+GLOBAL)?(?:\s+TAB)?\s+LMCControlCommandService::(?<Name>@?[A-Za-z_][A-Za-z0-9_]*)\s*$')
> $struct=[regex]::Matches($scan,'(?i)(?:\bFUNCTION(?:\s+GLOBAL)?(?:\s+TAB)?\s+LMCControlCommandService::(?<Name>@?[A-Za-z_][A-Za-z0-9_]*)\b|(?<End>\bEND_FUNCTION\b))')
> $idx=-1
> for($i=0;$i -lt $headers.Count;$i++){if($headers[$i].Groups['Name'].Value -ceq 'ReadAxisRebaseRequiredMask'){$idx=$i}}
> $evt=$struct[$idx*2]
> $last=$macro[-1].Index+$macro[-1].Length
> $between=$scan.Substring($last,$evt.Index-$last)
> [pscustomobject]@{Headers=$headers.Count;Struct=$struct.Count;FirstCustomIndex=$idx;ExpectedHeaderIndex=$headers[$idx].Index;EventIndex=$evt.Index;EventValue=$evt.Value;LastMacroEnd=$last;BetweenLen=$between.Length;BetweenWhitespace=($between -match '\A[ \t\r\n]*\z');BetweenPreview=($between.Substring(0,[Math]::Min($between.Length,200)) -replace "`r",'<CR>' -replace "`n",'<LF>')}|fl
> for($i=0;$i -lt [Math]::Min(8,$headers.Count);$i++){[pscustomobject]@{i=$i;Header=$headers[$i].Groups['Name'].Value;HeaderIndex=$headers[$i].Index;StructHeader=$struct[$i*2].Value;StructIndex=$struct[$i*2].Index;StructEnd=$struct[$i*2+1].Value;StructEndIndex=$struct[$i*2+1].Index}}
> '@ | pwsh -NoProfile -Command -``
> - Ran `& pwsh -NoProfile -File "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" -RepositoryRoot (Resolve-Path '.').Path -AxisOwnershipReserveVerifierSelfTestOnly; exit $LASTEXITCODE`
>
> </details>
>
> 현재 소스에서 문제의 Reserve 정적 검증만 독립 재실행한 결과는 `62/62 PASS`입니다. 따라서 배포 시도에서 나온 오류가 현재 소스의 지속적 결함인지, 실행 중 스냅샷/호출 순서에 의한 일시적 실패인지 구분해야 합니다. 전체 배포를 섣불리 다시 돌리지는 않고, 감사 에이전트의 실행 경로 분석과 대조하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n -C 6 "RunTests|Verify-LasalContract|AllowDirty|RepositoryRoot" "LMC_Library/LMC_API/Build-LmcApiDistribution.ps1" | Select-Object -First 220`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git diff --name-only`
>
> </details>
>
> 배포 차단 원인을 확정했습니다. PLC 소스나 `Classes.lcb` 문제가 아니라 Windows PowerShell 5.1 호환성 버그입니다. 검증기가 .NET `MatchCollection[-1]`을 썼는데, PowerShell 7에서는 마지막 항목이지만 5.1에서는 `null`이 되어 정상 소스를 오류로 판정했습니다. 해당 검증기만 좁게 고치고 PS5/PS7 및 실제 MSBuild 경로를 다시 검증하겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "\[\s*-\s*[0-9]+\s*\]" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $l=Get-Content $p; foreach($range in @(@(2480,2520),@(2615,2670),@(2790,2830),@(5415,5460),@(5665,5720),@(19510,19555))){for($i=$range[0];$i -le $range[1];$i++){ '{0}:{1}' -f $i,$l[$i-1] }}`
> - Ran `rg -n -C 5 "RunLasalContract|Verify-LasalContract" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj"`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git status --short | Select-Object -First 25`
> - Ran `$p='LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; $l=Get-Content $p; for($i=805;$i -le 985;$i++){ if($i -in 805..985){'{0}:{1}' -f $i,$l[$i-1]} }`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git status --short | Select-Object -First 20; git diff -- "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
> - Ran `rg -n "\$lastMacroMatch|\$lastEndVar|\$lastResponseEndVar" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Process powershell,pwsh,MSBuild -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime,CPU | Format-Table -AutoSize`
>
> </details>
>
> 호환성 패치는 LASAL 검증기 1파일에만 들어갔고, 음수 인덱스가 남은 세 곳은 실제 배열이라 PS5에서도 안전한 것으로 분류됐습니다. 지금 PS5/PS7 집중 검증과 배포가 실제로 호출하는 `RunLasalContract`·`RunLasalNetworkContract`를 각각 확인 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --name-only | Where-Object { $_ -ne 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb' }; git diff --stat -- "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --check -- "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"; git diff --numstat -- "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
>
> </details>
>
> 집중 회귀는 PS7과 PS5.1에서 모두 통과했고, 독립 리뷰도 패치를 CLEAN으로 판정했습니다. 현재 가장 긴 `RunLasalContract` 전체 SourceOnly 검증이 진행 중이며 아직 오류는 없습니다. 이 단계가 끝나면 실제 배포 전체를 다시 실행해 새 EXE 게이트까지 도달하는지 확인합니다.
>
> PS5 호환성 오류는 제거됐고 전체 계약이 그 지점을 정상 통과했습니다. 다음 중단은 의도된 별도 안전 게이트입니다: 현재 사용자 `Classes.lcb`가 승인된 Gate D identity와 다르기 때문에 `sanctioned Gate D identity drifted`로 차단됐습니다. 이 파일은 건드리지 않습니다. reconnect 코드 문제와는 분리된 정상적인 fail-closed 동작입니다.
>
> 후속 Network 계약도 동일한 실제 Windows PowerShell 5.1 경로로 실행 중입니다. 지금까지 새 호환성 수정 지점에서는 문제가 없고, 사용자 `Classes.lcb`는 읽기만 했으며 수정·복구·스테이지하지 않았습니다.
>
> 양쪽 실제 MSBuild 계약 모두 수정 지점을 통과했고, 각각 약 3분 뒤 동일한 의도된 `Classes.lcb` Gate D identity 차단에서 멈췄습니다. 검증기 수정은 독립 리뷰까지 CLEAN이므로 이 1파일만 먼저 커밋하고, 그 커밋 해시를 문서에 고정하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$target='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; git diff --check -- $target; git diff --cached --check; git add -- $target; $staged=@(git diff --cached --name-only); if($staged.Count -ne 1 -or $staged[0] -cne $target){ throw "Unexpected staged scope: $($staged -join ', ')" }; git diff --cached --check; git diff --cached --stat; git commit -m "fix(lasal): make contract verifier PS5 compatible"`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git log -4 --oneline`
>
> </details>
>
> 검증기 수정은 `ad4af91`로 별도 커밋했습니다. 현재는 12개 권위 문서에 “실제 EXE 재실행 PASS”와 “전체 배포는 현재 미승인 `Classes.lcb` Gate D에서 의도적으로 STOP”을 섞지 않고 반영 중입니다. 문서가 끝나면 독립 모순 검토 후 정확한 범위만 커밋합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git diff --check -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"; git diff --numstat -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
> - Ran `$names='LasalMotionControlApiExample','Lasal2','MSBuild','powershell'; Get-Process -ErrorAction SilentlyContinue | Where-Object { $names -contains $_.ProcessName } | Select-Object Id,ProcessName,StartTime,CPU | Format-Table -AutoSize; Get-ChildItem -LiteralPath $env:TEMP -Directory -Filter 'Elmo.WpfExecutableRelaunch.*' -ErrorAction SilentlyContinue | Select-Object FullName`
> - Ran `Get-CimInstance Win32_Process -Filter "ProcessId=13760" | Select-Object ProcessId,ParentProcessId,CommandLine | Format-List`
>
> </details>
>
> 문서 초안은 12개 정확 범위로 동결됐습니다. 지금 두 명이 서로 다른 관점에서 독립 검토 중입니다. 저는 동시에 placeholder·stale 수치·“full distribution PASS” 같은 과장 표현과, 실제 코드 순서/해시/STOP 경계를 기계적으로 검사하겠습니다.
>
> <details><summary>Ran 8 commands</summary>
>
> - Ran `$expected=@('LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md','LMC_Library/LMC_API/API_USER_MANUAL_KO.md','LMC_Library/LMC_API_Delivery/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md','LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt','LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md','LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md','LMC_Library/LasalApiWpfTestApp/API_MAPPING.md','LMC_Library/LasalApiWpfTestApp/DESIGN.md','LMC_Library/LasalApiWpfTestApp/README.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'); $actual=@(git diff --name-only -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"); if([string]::Join('|',$actual) -cne [string]::Join('|',($expected|Sort-Object))){ 'ACTUAL:'; $actual; 'EXPECTED_SORTED:'; $expected|Sort-Object; exit 3 }; git diff --check -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"; rg -n "\[FINAL|TODO|TBD|PLACEHOLDER|full Distribution.*PASS|전체 배포.*PASS|actual EXE.*미검증|다음 수동 Connect|다음 manual Connect|335/335|334/334|af4ab63" "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
> - Ran `$expected=@('LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md','LMC_Library/LMC_API/API_USER_MANUAL_KO.md','LMC_Library/LMC_API_Delivery/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md','LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt','LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md','LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md','LMC_Library/LasalApiWpfTestApp/API_MAPPING.md','LMC_Library/LasalApiWpfTestApp/DESIGN.md','LMC_Library/LasalApiWpfTestApp/README.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'); $actual=@(git diff --name-only -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"); $delta=@(Compare-Object $expected $actual); if($delta.Count -ne 0 -or $actual.Count -ne 12){ $delta; exit 3 }; git diff --check -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"; rg -n "\[FINAL|TODO|TBD|PLACEHOLDER|full Distribution.*PASS|전체 배포.*PASS|actual EXE.*미검증|다음 수동 Connect|다음 manual Connect|335/335|334/334|af4ab63" "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
> - Ran `rg -n "cbf2548|ad4af91|3/28|13,? ?2,? ?13|177\.7|174\.9|829AC331|BB91C40C|sanctioned Gate D identity drifted|manifest.*미도달|full Distribution.*아니다|PLC.*proof|100 ms" "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
> - Ran `$files=@('LMC_Library/LasalApiWpfTestApp/README.md','LMC_Library/LMC_API_Delivery/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'); foreach($f in $files){'=== '+$f; rg -n -C 4 "binary-reference|manifest|transaction completion|tested.*final|full Distribution|RunLasalContract|RunLasalNetworkContract" $f | Select-Object -Last 60}`
> - Ran `$p='docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'; $l=Get-Content $p; for($i=985;$i -le 1017;$i++){ '{0}:{1}' -f $i,$l[$i-1] }`
> - Ran `$p='LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md'; $l=Get-Content $p; for($i=724;$i -le 790;$i++){ '{0}:{1}' -f $i,$l[$i-1] }`
> - Ran `rg -n -i "current.*PASS LASAL|RunLasalContract.*PASS|RunLasalNetworkContract.*PASS|LASAL.StaticContract.*PASS" "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
> - Ran `rg -n "62B2|BB91|7D179|E64B49|829AC" "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
>
> </details>
>
> 독립 문서 검토에서 두 종류의 정확한 오류를 잡았습니다.
>
> - 과거 LASAL PASS 문구 몇 곳이 아직 “현재 PASS”로 남아, 방금 확인한 current Gate D STOP과 충돌합니다.
> - mutex contender는 TCP 0이지만 `MUTEX_BUSY` 보고서 1개를 쓰는데, 한 문서가 temp mutation 0으로 잘못 묶었습니다.
>
> 해당 문장만 좁게 교정한 뒤 다시 두 명에게 재검토시키겠습니다.
>
> <details><summary>Edited 5 files, ran 2 commands</summary>
>
> - Ran `$specs=@(@('LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md',1038,1060),@('LMC_Library/LMC_API_Delivery/README.md',150,185),@('LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md',280,315),@('LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md',768,790),@('docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md',398,412),@('LMC_Library/LasalApiWpfTestApp/API_MAPPING.md',252,278)); foreach($s in $specs){$f=$s[0];$a=[int]$s[1];$b=[int]$s[2];'=== '+$f;$l=Get-Content -LiteralPath $f;for($i=$a;$i -le $b;$i++){'{0}:{1}'-f $i,$l[$i-1]}}`
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+15 -9)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+3 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Ran `git diff --check -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"; rg -n -i "current.*PASS LASAL|RunLasalContract.*current.*PASS|RunLasalNetworkContract.*current.*PASS|LASAL SourceOnly static contract \| .*PASS \||malformed.*contender.*mutation.*0" "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"; git diff --numstat -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
>
> </details>
>
> <details><summary>Edited 4 files, ran 4 commands</summary>
>
> - Ran `rg -n -i "SourceOnly/full.*PASS|SourceOnly.*PASS|full static contract.*PASS|full static.*PASS|LASAL full.*PASS" "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
> - Ran `$p='LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md'; $l=Get-Content $p; for($i=314;$i -le 344;$i++){'{0}:{1}'-f $i,$l[$i-1]}`
> - Ran ``rg -n -C 3 "production (example )?EXE|production `?LasalMotionControlApiExample|production executable" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md"``
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Ran `git diff --check -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"; rg -n -i "production (example )?EXE|production executable|full Distribution.*PASS|현재.*SourceOnly.*PASS|current.*SourceOnly.*PASS" "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
>
> </details>
>
> 두 차례 독립 검토에서 나온 문서 오류를 모두 교정했습니다. 특히 full Distribution이 아직 PASS하지 않았는데 “production EXE”라고 부르던 네 문장을 “제공된 actual example EXE”로 낮춰, 개발/standalone 증거와 배포 승인 상태를 분리했습니다. 최종 재검토만 남았습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$expected=@('LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md','LMC_Library/LMC_API/API_USER_MANUAL_KO.md','LMC_Library/LMC_API_Delivery/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md','LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt','LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md','LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md','LMC_Library/LasalApiWpfTestApp/API_MAPPING.md','LMC_Library/LasalApiWpfTestApp/DESIGN.md','LMC_Library/LasalApiWpfTestApp/README.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'); $actual=@(git diff --name-only -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"); $delta=@(Compare-Object $expected $actual); if($delta.Count -ne 0 -or $actual.Count -ne 12){$delta;throw 'scope'}; git diff --check -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"; git diff --cached --check; git diff --stat -- "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" "LMC_Library/LMC_API/API_USER_MANUAL_KO.md" "LMC_Library/LMC_API_Delivery/README.md" "LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md" "LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md" "LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt" "LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md" "LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md" "LMC_Library/LasalApiWpfTestApp/API_MAPPING.md" "LMC_Library/LasalApiWpfTestApp/DESIGN.md" "LMC_Library/LasalApiWpfTestApp/README.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"; git status --short | Select-Object -First 20`
>
> </details>
>
> 최종 문서 리뷰는 P0/P1/P2 없이 CLEAN입니다. 이제 사용자 `Classes.lcb`와 기존 미추적 자료를 제외하고 정확히 12개 문서만 스테이지해 커밋합니다.
>
> <details><summary>Ran 2 commands</summary>
>

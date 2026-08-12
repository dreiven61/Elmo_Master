>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -like '*Verify-LasalUdpCallbackContract.ps1*RunSelfTest*' } | Select-Object ProcessId,ParentProcessId,Name,CreationDate,CommandLine | Format-List`
>
> </details>
>
> PS5.1 Gate D self-test는 최종 바이트에서 296/296 PASS(220.9초)했습니다. PS7 동일 self-test를 단독 실행 중이며, 이 완료 뒤 main dirty 거부와 clean detached 승인 경계를 연속 확인합니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -like '*Verify-LasalUdpCallbackContract.ps1*RunSelfTest*' } | Select-Object ProcessId,ParentProcessId,Name,CreationDate,CommandLine | Format-List; Get-Process -Id ((Get-CimInstance Win32_Process | Where-Object { $_.Name -eq 'pwsh.exe' -and $_.CommandLine -like '*Verify-LasalUdpCallbackContract.ps1*RunSelfTest*' } | Select-Object -Last 1).ProcessId) -ErrorAction SilentlyContinue | Select-Object Id,CPU,Responding,WorkingSet64`
> - Ran `Get-Process -Id 20084 -ErrorAction SilentlyContinue | Select-Object Id,CPU,Responding,WorkingSet64,StartTime`
> - Ran `Get-Process -Id 20084 -ErrorAction SilentlyContinue | Select-Object Id,CPU,Responding,WorkingSet64,StartTime`
>
> </details>
>
> PS7 self-test는 약 8분 경과했고 PID 20084가 계속 CPU를 사용하며 응답 중입니다. 과거 정상 소요와 같은 구간이라 중단하지 않고 완료를 기다립니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-Process -Id 20084 -ErrorAction SilentlyContinue | Select-Object Id,CPU,Responding,WorkingSet64,StartTime; Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -like '*Verify-LasalUdpCallbackContract.ps1*' -and $_.ProcessId -ne $PID } | Select-Object ProcessId,Name,CreationDate,CommandLine | Format-Table -Wrap`
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -like '*Verify-LasalContract.ps1*' -or $_.CommandLine -like '*RunLasalContract*' } | Select-Object ProcessId,ParentProcessId,Name,CreationDate,CommandLine | Format-List`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Gate D 검증을 끝까지 고정하겠습니다. 지금은 clean tracked `24402…` 승인과 main dirty `13EA…` 거부를 분리해 확인 중이며, 검증 동결 전에는 verifier를 스테이지하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git diff --stat; git diff -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1; Get-Process powershell,pwsh,MSBuild,dotnet -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime,CPU,Responding | Format-Table -AutoSize`
>
> </details>
>
> 현재 검증은 명확합니다. PS5/PS7 self-test는 각각 296/296 PASS했고, clean tracked `24402…`는 새 승인 경계에서 PASS, 사용자 dirty `13EA…`는 동일한 Classes identity 오류로 계속 차단됩니다. PS7 SourceOnly 한 건만 마친 뒤 커밋 범위를 고정하겠습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n --glob "*.md" --glob "*.html" --glob "API_USER_MANUAL_KO.md" "Gate D|TerminalWakeBrokerCandidate|24402BFA|13EA5823|physical snapshot|물리 스냅샷|ratchet|라쳇|full Distribution|actual EXE" LMC_Library/LMC_API docs README.md | Select-Object -First 500`
> - Ran `git show --stat --oneline 978597b; git show --name-only --format= 978597b`
> - Ran `$p='LMC_Library/LMC_API/API_USER_MANUAL_KO.md'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(20,65),@(360,390))){ for($i=$range[0];$i -le $range[1];$i++){ '{0,4}: {1}' -f $i,$lines[$i-1] }; '' }`
> - Ran `rg -n "generate_api_user_manual_ko|API_USER_MANUAL_KO|pandoc|render_docx|User_Manual_KO" LMC_Library/LMC_API README.md docs | Select-Object -First 250`
>
> </details>
>
> PS7 SourceOnly은 장시간 정적 계약 전수 구간에서 정상 실행 중입니다. 추가 코드 변경은 없고, 그 동안 독립 reviewer가 승인 범위가 `TerminalWakeBrokerCandidate` 하나에만 닫혀 있는지 확인하고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LMC_API/README.md'; $l=Get-Content $p; for($i=248;$i -le 275;$i++){ '{0,4}: {1}' -f $i,$l[$i-1] }`
>
> </details>
>
> Gate D ratchet은 독립 검토까지 CLEAN입니다. 이제 verifier 1개만 스테이지해 커밋하고, 사용자 `Classes.lcb` 및 모든 미추적 자료는 그대로 제외합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'; $file='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; if($hash -ne 'F036B9B3F2D3E173D38BFB6CBBAB05EC4F877CCDF9B972E95C6ED35B7DE34E37'){throw "Verifier hash drifted: $hash"}; git diff --check -- $file; if($LASTEXITCODE -ne 0){throw 'working diff-check failed'}; git add -- $file; git diff --cached --check -- $file; if($LASTEXITCODE -ne 0){throw 'cached diff-check failed'}; $cached=(git diff --cached --name-only); if(@($cached).Count -ne 1 -or $cached[0] -cne $file){throw ('Unexpected staged scope: ' + ($cached -join ', '))}; git diff --cached --stat; git commit -m "test(lasal): approve exact Gate D physical snapshot"`
> - Ran `$ErrorActionPreference='Stop'; $file='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $cached=@(git diff --cached --name-only); if($cached.Count -ne 1 -or $cached[0] -cne $file){throw ('Unexpected staged scope: ' + ($cached -join ', '))}; git diff --cached --check -- $file; if($LASTEXITCODE -ne 0){throw 'cached diff-check failed'}; git diff --cached --stat; git commit -m "test(lasal): approve exact Gate D physical snapshot"; if($LASTEXITCODE -ne 0){throw 'commit failed'}; git rev-parse --short HEAD; git status --short`
>
> </details>
>
> `d4204b4`로 Gate D exact physical snapshot 승인을 커밋했습니다. 다음은 문서/PDF 검증 절차에 따라 사용자 매뉴얼의 STOP 문구를 “tracked `24402…` 정적 승인 완료, main dirty `13EA…` 거부 유지, full Distribution·PLC 미실행”으로 교정하고 canonical DOCX/PDF를 다시 생성·검수하는 단계입니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-Content -LiteralPath 'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1' -TotalCount 180; rg -n "Build-LmcApiDistribution.ps1|CandidatePath|ManualDocxPath|AllowDirty|dirty-preview|RepositoryRoot" LMC_Library/LMC_API/README.md docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md | Select-Object -First 180`
> - Ran `$p='LMC_Library/LMC_API/README.md'; $l=Get-Content $p; for($i=35;$i -le 60;$i++){ '{0,4}: {1}' -f $i,$l[$i-1] }`
>
> </details>
>
> Gate D 커밋은 `d4204b4`입니다. 매뉴얼 재생성은 Markdown 교정 → Word 목차/필드 갱신 → 같은 DOCX에서 PDF export → OpenXML·43쪽 전수 렌더·폰트·양 호스트 정책 검증 순서로 진행 중입니다. 이 단계에서도 full Distribution은 아직 실행하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'; git diff -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md'`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `Get-Process WINWORD,powershell,pwsh,python -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime,CPU,Responding,MainWindowTitle | Sort-Object StartTime | Format-Table -AutoSize`
> - Ran `git diff --check -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md'; git diff -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md'`
> - Ran `Get-Process WINWORD,python,powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-20)} | Select-Object Id,ProcessName,StartTime,CPU,Responding | Format-Table -AutoSize; git status --short -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'`
>
> </details>
>
> 매뉴얼 원문 교정은 완료됐고 현재 생성·검수 단계입니다. 승인 범위는 clean tracked hash 하나만 명시했으며, 사용자 dirty hash는 문서에서도 계속 명시적 reject로 유지했습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx' 'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'; Get-Process WINWORD,python,powershell,pwsh -ErrorAction SilentlyContinue | Where-Object {$_.StartTime -gt (Get-Date).AddMinutes(-30)} | Select-Object Id,ProcessName,StartTime,CPU,Responding | Format-Table -AutoSize`
>
> </details>
>
> 새 DOCX/PDF 생성은 완료됐고 Word 기준 43쪽입니다. 현재 OpenXML 오류 0, heading 66/table 109, PDF 43쪽 A4까지 통과했으며, 남은 것은 폰트·양 호스트 semantic policy·43쪽 전수 시각 검사입니다.
>
> 시각 검수는 전체 페이지를 실제 렌더해 진행 중입니다. 문서 바이트는 동결 상태로 유지하고 있으며, 페이지 잘림·겹침·빈 페이지·한글 글리프 문제를 찾으면 생성본을 그대로 승인하지 않고 다시 만들겠습니다.
>
> PDF 폰트 8종 embedded/subset와 PS5.1·PS7 manual policy 3/3까지 통과했습니다. 43쪽 contact sheet와 변경 페이지 원본 확대 검사가 끝나면 exact 3파일을 독립 재검토한 뒤 커밋합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'; $paths=@('LMC_Library/LMC_API/API_USER_MANUAL_KO.md','LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx','LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'); Get-Item -LiteralPath $paths | Select-Object FullName,Length; Get-FileHash -Algorithm SHA256 -LiteralPath $paths | Select-Object Path,Hash; git diff --check -- $paths; git diff --stat -- $paths; git status --short -- $paths`
> - Ran `$paths=@('LMC_Library/LMC_API/API_USER_MANUAL_KO.md','LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx','LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'); foreach($p in $paths){$i=Get-Item -LiteralPath $p; $h=Get-FileHash -Algorithm SHA256 -LiteralPath $p; Write-Output ($i.Length.ToString()+' '+$h.Hash+' '+$p)}`
>
> </details>
>
> 새 canonical 매뉴얼은 동결됐습니다: DOCX 92,229 bytes, PDF 1,003,309 bytes이며 43쪽·OpenXML 0·폰트 8종 embedded/subset·양 호스트 policy 3/3입니다. 지금 독립 reviewer가 문구와 실제 아티팩트를 교차검토하고 있습니다.
>
> 독립 reviewer도 clean tracked 승인과 dirty 거부를 양 호스트에서 재현했고, 현재 P0/P1은 없습니다. 남은 확인은 43쪽 전체 contact sheet와 OpenXML/폰트/정책 재검증뿐입니다.
>
> 독립 검토까지 CLEAN입니다. 매뉴얼 원본·DOCX·PDF 정확히 3파일만 커밋하고, 이어 8개 current release 문서의 역사/현재 상태를 동기화하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'; $files=@('LMC_Library/LMC_API/API_USER_MANUAL_KO.md','LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx','LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'); $expected=@{'LMC_Library/LMC_API/API_USER_MANUAL_KO.md'='D7DE1AF51A548AA7361614167D546A7057C8D03260CE92CFA9335964A611C022';'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx'='57D17650D1F24E9350830E784EFE94E00CB1A89CB126CD9A05865580A9708B46';'LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf'='83A57CC4B15D4E0BA4E0D9A54FD044C82A131168D16B36F2694F76AF098232E0'}; foreach($f in $files){$h=(Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash; if($h -ne $expected[$f]){throw "Hash drift: $f $h"}}; git diff --check -- $files; if($LASTEXITCODE -ne 0){throw 'working diff-check failed'}; git add -- $files; $cached=@(git diff --cached --name-only); if($cached.Count -ne 3){throw ('Staged count drift: '+$cached.Count)}; foreach($f in $files){if($cached -notcontains $f){throw "Missing staged file: $f"}}; git diff --cached --check -- $files; if($LASTEXITCODE -ne 0){throw 'cached diff-check failed'}; git diff --cached --stat; git commit -m "docs(api): publish Gate D approved manual"; if($LASTEXITCODE -ne 0){throw 'commit failed'}; git rev-parse --short HEAD`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git log -6 --oneline; git status --short | Select-Object -First 20`
>
> </details>
>
> Gate D 상태를 반영한 매뉴얼 3파일도 `5d5aebe`로 커밋했습니다. 현재 8개 release-status 문서는 과거 STOP 실행 기록을 보존하면서, 최신 상태만 `d4204b4` tracked static 승인 / full Distribution 미실행으로 교정 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html' 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md' 'LMC_Library/LMC_API/README.md' 'LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md' 'docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md' 'docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'; git diff --stat -- 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html' 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md' 'LMC_Library/LMC_API/README.md' 'LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md' 'docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md' 'docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'`
>
> </details>
>
> 8개 문서 중 API Guide Markdown, README, 자동시험 문서까지 교정됐습니다. 남은 architecture/transactional/plan 문서와 HTML 생성이 끝나면 stale 문구를 전수 검색하고 독립 검토를 붙입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html' 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md' 'LMC_Library/LMC_API/README.md' 'LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md' 'docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md' 'docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html' 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md' 'LMC_Library/LMC_API/README.md' 'LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md' 'docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md' 'docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'; Get-Process pandoc -ErrorAction SilentlyContinue | Select-Object Id,StartTime,CPU`
>
> </details>
>
> 8개 문서 모두 수정됐고 Guide HTML도 문서에 기록된 exact Pandoc 명령으로 재생성됐습니다. 현재는 historical STOP 문장을 잘못 지우지 않으면서 stale current 표현만 남지 않았는지 최종 대조 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html','LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md','LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --stat -- $files; git diff --check -- $files; rg -n "Gate D STOP은 유지|Gate D STOP/production NO-GO는 그대로|reviewed Gate D physical snapshot ratchet 뒤|reviewed Gate D와 clean full Distribution|ProductionApproved=false.*TerminalWakeBrokerCandidate" $files`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md','LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); rg -n "5d5aebe|57D17650|83A57CC4|d4204b4|F036B9B3|24402BFA|13EA5823" $files`
>
> </details>
>
> 8개 문서의 scoped diff-check와 stale scan은 PASS했고, Guide HTML은 exact Pandoc 재생성 두 번의 SHA가 동일했습니다. 독립 reviewer가 현재/역사 경계와 새 manual hash까지 마지막으로 대조하고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff -- 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md' 'docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html'`
>
> </details>
>
> 독립 검토에서 P2 한 건을 잡았습니다. 개발계획 요약 두 곳이 여전히 “current SourceOnly/full PASS”로 남아 있어, 실제 증거인 “current SourceOnly PASS / post-approval full·network 미실행”과 충돌합니다. 해당 두 문장만 좁혀 수정합니다.
>
> <details><summary>Edited 2 files, ran a command</summary>
>
> - Ran `$p='docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'; $l=Get-Content $p; for($i=154;$i -le 164;$i++){ '{0,4}: {1}' -f $i,$l[$i-1] }; ''; $p2='docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html'; $l2=Get-Content $p2; for($i=536;$i -le 547;$i++){ '{0,4}: {1}' -f $i,$l2[$i-1] }`
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+2 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+2 -2)
>
> </details>
>
> <details><summary>Edited a file, ran 2 commands</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html','LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md','LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $files; rg -n "current SourceOnly/full PASS|LASAL SourceOnly/full PASS|SourceOnly/full static PASS" $files`
> - Ran `$p='docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'; $l=Get-Content $p; for($i=605;$i -le 618;$i++){ '{0,4}: {1}' -f $i,$l[$i-1] }`
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+4 -1)
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$files=@('LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html','LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md','LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $files; git diff --cached --check; rg -n "current SourceOnly/full PASS|LASAL SourceOnly/full PASS|current .*SourceOnly/full static PASS|Gate D STOP은 유지|Gate D STOP/production NO-GO는 그대로|reviewed Gate D physical snapshot ratchet 뒤|reviewed Gate D와 clean full Distribution" $files; git diff --stat -- $files`
>
> </details>
>
> 계획 문서의 마지막 stale 체크박스도 잡아냈습니다. 이제 Markdown과 dashboard 모두 “current SourceOnly PASS”와 “post-approval full/network static 미실행”을 별도 항목으로 분리했고, 전체 8파일 diff-check를 다시 통과했습니다.
>
> 8개 문서도 독립 검토 CLEAN입니다. 정확히 이 8파일만 커밋한 뒤, 새 HEAD의 clean detached checkout에서 canonical manual 기본 입력으로 full Distribution을 실행하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'; $files=@('LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html','LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md','LMC_Library/LMC_API/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md','docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html','docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'); git diff --check -- $files; if($LASTEXITCODE -ne 0){throw 'working diff-check failed'}; git add -- $files; $cached=@(git diff --cached --name-only); if($cached.Count -ne 8){throw ('Staged count drift: '+$cached.Count+' '+($cached -join ','))}; foreach($f in $files){if($cached -notcontains $f){throw "Missing staged file: $f"}}; git diff --cached --check -- $files; if($LASTEXITCODE -ne 0){throw 'cached diff-check failed'}; git diff --cached --stat; git commit -m "docs(api): record approved Gate D baseline"; if($LASTEXITCODE -ne 0){throw 'commit failed'}; git rev-parse HEAD; git status --short | Select-Object -First 12`
>
> </details>
>
> 현재 HEAD는 `396044a`입니다. clean detached checkout에서 `-AllowDirty`·manual override 없이 canonical 입력 그대로 full Distribution을 시작했습니다. LASAL IDE/PLC/Download는 실행하지 않으며, 실패하면 우회하지 않고 첫 blocker와 rollback 상태를 그대로 고정합니다.
>
> 첫 clean full run은 28.3초 만에 fail-closed했습니다. 원인은 Gate D나 매뉴얼이 아니라 mandatory host-parity가 semantic suite의 최신 assertion count와 불일치한 tooling 계약입니다. 우회하지 않았고, 먼저 rollback·canonical 불변을 확인한 뒤 이 preflight 계약을 좁게 수정하겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "SemanticPolicy|53|52|TestCount" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1 LMC_Library/LMC_API/DistributionToolchainProvenance.ps1 LMC_Library/LMC_API/ReleaseManifest.ps1 LMC_Library/LMC_API/Test-LmcReleaseManifest.ps1`
> - Ran `$p='LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; $l=Get-Content $p; foreach($range in @(@(590,710),@(1048,1075))){for($i=$range[0];$i -le $range[1];$i++){'{0,4}: {1}' -f $i,$l[$i-1]};''}`
>
> </details>
>
> 원인은 확정됐습니다. SemanticPolicy는 정상 `53/18`, host-parity만 과거 `52`를 6곳에 고정하고 있었습니다. 현재 SHA/check 계약은 그대로 두고 assertion count만 `53`으로 맞춘 뒤 양 호스트 direct·worker·pipeline 회귀를 실행 중입니다.
>
> 수정은 host-parity 파일 1개, 정확히 `52→53` 여섯 곳뿐입니다. 양 호스트 direct와 격리 worker는 이미 exit 0이며 stderr 0, SHA/check 값은 불변입니다. 독립 reviewer가 pipeline 291 fixture를 마지막으로 재실행하고 있습니다.
>
> 독립 검토 CLEAN입니다. host-parity count fix 1파일만 커밋하고, 새 commit의 별도 clean detached worktree에서 full Distribution을 처음부터 다시 실행합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$ErrorActionPreference='Stop'; $file='LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'; $hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; if($hash -ne 'A5505A5E8BABB6AC83A73F04A922B619EBDD59A0965C431E590BCD479A679C26'){throw "Hash drifted: $hash"}; git diff --check -- $file; if($LASTEXITCODE -ne 0){throw 'diff-check failed'}; git add -- $file; $cached=@(git diff --cached --name-only); if($cached.Count -ne 1 -or $cached[0] -cne $file){throw ('Unexpected staged scope: '+($cached -join ','))}; git diff --cached --check -- $file; if($LASTEXITCODE -ne 0){throw 'cached diff-check failed'}; git commit -m "test(distribution): sync semantic host-parity count"; if($LASTEXITCODE -ne 0){throw 'commit failed'}; git rev-parse HEAD`
>
> </details>
>
> 새 HEAD `8d51cee`에서 두 번째 clean run을 06:51:28에 시작했습니다. 기존 실패 worktree는 건드리지 않았고, 새 detached checkout·새 candidate·canonical 기본 manual만 사용합니다. 현재 mandatory 14-suite preflight의 PS5 Pipeline 단계입니다.
>
> 두 번째 run은 91.9초에 다음 fail-closed 경계를 찾았습니다. count fix는 통과했고 PS5의 Pipeline·Semantic·Manifest·Toolchain·MethodSize까지 PASS했지만, UDP self-test가 clean checkout의 `ConfigObjects.st` generated registry identity를 거부했습니다. 현재 EOL/checkout portability 문제인지 실제 source drift인지 분리 진단 중이며 우회하지 않습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `$main='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Source/ConfigObjects.st'; $det='C:\work\Elmo\Elmo_Master_release_verify_8d51cee\Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Source\ConfigObjects.st'; foreach($p in @($main,$det)){if(Test-Path -LiteralPath $p){$i=Get-Item -LiteralPath $p;$h=Get-FileHash -Algorithm SHA256 -LiteralPath $p;$bytes=[IO.File]::ReadAllBytes((Resolve-Path $p));$crlf=0;$lf=0;for($j=0;$j -lt $bytes.Length;$j++){if($bytes[$j]-eq 10){$lf++;if($j -gt 0 -and $bytes[$j-1]-eq 13){$crlf++}}};[pscustomobject]@{Path=$p;Length=$i.Length;Hash=$h.Hash;LF=$lf;CRLF=$crlf}}}; git check-attr -a -- 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Source/ConfigObjects.st'; git show HEAD:'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Source/ConfigObjects.st' | Out-Null; rg -n "ConfigObjects.*registry|ConfigObjects|Gate A" 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1' | Select-Object -First 80`
> - Ran `$paths=@('Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/ConfigObjects.st','C:\work\Elmo\Elmo_Master_release_verify_8d51cee\Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\ConfigObjects.st'); foreach($p in $paths){$full=(Resolve-Path -LiteralPath $p).Path;$i=Get-Item -LiteralPath $full;$h=Get-FileHash -Algorithm SHA256 -LiteralPath $full;$b=[IO.File]::ReadAllBytes($full);$lf=0;$crlf=0;for($j=0;$j -lt $b.Length;$j++){if($b[$j]-eq 10){$lf++;if($j -gt 0 -and $b[$j-1]-eq 13){$crlf++}}}; Write-Output ("$($i.Length) $($h.Hash) LF=$lf CRLF=$crlf $p")}; git check-attr -a -- 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/ConfigObjects.st'; $p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content $p; for($i=375;$i -le 390;$i++){'{0,4}: {1}' -f $i,$l[$i-1]}`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content $p; for($i=870;$i -le 910;$i++){'{0,4}: {1}' -f $i,$l[$i-1]}`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content $p; foreach($range in @(@(10020,10075),@(10950,10990),@(11090,11130))){for($i=$range[0];$i -le $range[1];$i++){'{0,5}: {1}' -f $i,$l[$i-1]};''}; rg -n "RunSelfTest|Invoke-UdpCallbackVerifierSelfTest|RepositoryRoot" $p | Select-Object -Last 30`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "function New-Synthetic|ReadAllText\(\$path|PSScriptRoot.*\.\.\\\.\.\\\.\.\\\.\." LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1`
> - Ran `rg -n "ReadAll(Text|Bytes)|Get-Content|Join-Path \$root|Get-CurrentRepositorySnapshot" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 | Select-Object -First 250`
>

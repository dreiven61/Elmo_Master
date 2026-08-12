>
> 범위는 정확히 이 개발본입니다. 종료 시 `0x405D -1` 예외를 사용자 예외로 띄우지 않게 정리하고, 다음 실행의 첫 `0x8080 -1`에서는 실패 소켓을 버린 뒤 새 TCP 세션을 딱 한 번 재시도하도록 구현하겠습니다. 실행 중인 테스트 앱은 빌드 잠금 해제를 위해 필요할 때 제가 종료하고, 가짜 PLC 테스트·전체 SDK/WPF 회귀·독립 리뷰·문서·커밋까지 마치겠습니다. 실제 PLC 다운로드나 LASAL 변경은 하지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$procs=Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('LasalMotionControlApiExample.exe','MSBuild.exe','LasalMotionControlLib.Tests.exe') } | Select-Object ProcessId,Name,ExecutablePath,CommandLine; $procs | Format-List; git status --short --untracked-files=all | Select-Object -First 80; git diff --stat -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs`
>
> </details>
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st'; $l=Get-Content -LiteralPath $p; for($i=380;$i -le 455;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; $l=Get-Content -LiteralPath $p; for($i=680;$i -le 880;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
> - Ran `rg -n "RpcCallbackLastDisarmResult|Disarm.*-9|ErrorId=-1|same-socket|FreshSessionRetry|20 ms|100 ms|CallbackSender.*disconnect|fence mismatch" LMC_Library docs Lasal_PRG/Elmo_EtherCAT_Test_4Axis -g "*.md" -g "*.txt" -g "*.cs" -g "*.st" | Select-Object -First 250`
> - Ran `$p='LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md'; $l=Get-Content -LiteralPath $p; for($i=1290;$i -le 1320;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}; for($i=1645;$i -le 1680;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n "public .*IsRpcInitialized|public .*IsCallbackListenerRunning|CallbackLocalEndPoint|LastCloseException" LMC_Library/LMC_API_Delivery/src/LmcConnection.cs`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $l=Get-Content -LiteralPath $p; foreach($range in @(@(555,690),@(5300,5415),@(9610,9675))){$s=$range[0];$e=$range[1];for($i=$s;$i -le [Math]::Min($e,$l.Count);$i++){'{0,5}: {1}' -f $i,$l[$i-1]};''}`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $l=Get-Content -LiteralPath $p; for($i=9670;$i -le [Math]::Min(9725,$l.Count);$i++){'{0,5}: {1}' -f $i,$l[$i-1]}; for($i=5410;$i -le 5445;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
> - Ran `rg -n "Initial|WindowClose|CloseNack|Shutdown|PersistentMinusOne|FreshSession|LastClose|RpcClose" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs | Select-Object -First 160`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs'; $l=Get-Content -LiteralPath $p; for($i=150;$i -le 390;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> 두 화면을 코드와 대조한 결과도 일치합니다.
>
> - `X` 종료 시 SDK는 로컬 TCP/콜백 상태를 실제로 정리하지만, `0x405D`의 `-1`을 마지막에 다시 던져 디버거가 멈춥니다.
> - PLC는 실패한 callback 소유권 정보를 fail-closed로 보존하므로, 다음 실행의 첫 `0x8080`도 `-1`이 될 수 있습니다.
> - 현재 수정본은 종료 경로를 “예외를 던지지 않는 Dispose + `LastCloseException` 기록”으로 바꿨고, 다음 첫 연결에서도 정확한 `-1` 응답 두 번일 때만 새 TCP를 한 번 엽니다.
> - 지금 같은 PLC/같은 포트에서 `성공 연결 → X 종료 -1 → 새 창 첫 연결 -1/-1 → 새 TCP 성공`을 한 번의 연속 가짜 서버 시나리오로 고정하는 중입니다.
>
> 예외를 무조건 숨기거나 무한 재시도하는 방식은 쓰지 않습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $l=Get-Content -LiteralPath $p; for($i=9190;$i -le 9285;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $l=Get-Content -LiteralPath $p; for($i=9650;$i -le 9715;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('MSBuild.exe','LasalMotionControlLib.Tests.exe','LasalApiWpfTestApp.SmokeTests.exe','LasalMotionControlApiExample.exe','vstest.console.exe') } | Select-Object ProcessId,Name,CreationDate,ExecutablePath,CommandLine | Format-List`
>
> </details>
>
> 종료 경로에서 한 단계 더 안전장치를 넣고 있습니다. 단순히 예외만 숨기면, 예상 밖의 Dispose 실패 때 숨은 TCP/콜백이 남은 채 새 연결을 열 수 있습니다.
>
> 그래서 종료와 내부 재접속 모두 다음 계약으로 통일합니다.
>
> - Dispose를 최대 2회까지만 시도
> - 실제 상태가 `Disconnected`, RPC 미초기화, callback listener 중지, endpoint 없음인지 확인
> - 이 조건이 충족된 경우에만 `0x405D -1`을 경고로 기록하고 종료/재접속 진행
> - 조건이 안 맞으면 새 연결을 절대 열지 않고 기존 객체를 복원해 실패를 명시
>
> 정상적인 `0x405D -1`은 SDK 내부에서 로컬 정리가 완료되므로 사용자 화면의 예외 중단은 없어지고, 진단 정보는 로그에 그대로 남습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n "Dispose.*Bounded|LocalConnectionCleanup|HasCompleteLocal|two bounded|Try.*Dispose|BestEffort" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs | Select-Object -First 80; git diff --stat -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $l=Get-Content -LiteralPath $p; foreach($range in @(@(5350,5425),@(9235,9310),@(9680,9750))){$s=$range[0];$e=$range[1];for($i=$s;$i -le $e;$i++){'{0,5}: {1}' -f $i,$l[$i-1]};''}`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs'; $l=Get-Content -LiteralPath $p; for($i=165;$i -le 340;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('MSBuild.exe','LasalMotionControlLib.Tests.exe','LasalApiWpfTestApp.SmokeTests.exe','LasalMotionControlApiExample.exe') } | Select-Object ProcessId,Name,CreationDate,ExecutablePath,CommandLine | Format-List`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "CloseShortFailureStep|ClientDisconnectBoundaryStep|WaitForConnectFailedClean" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs; rg -n "static void NotNull|NotNull\(" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests LMC_Library/LasalApiWpfTestApp -g "*.cs" | Select-Object -First 20`
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('MSBuild.exe','LasalApiWpfTestApp.SmokeTests.exe','LasalMotionControlLib.Tests.exe') } | Select-Object ProcessId,Name,CreationDate,CommandLine | Format-List`
>
> </details>
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `Get-ChildItem -LiteralPath 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin' -Recurse -Filter 'LasalMotionControlLib.Tests.exe' | Select-Object FullName,LastWriteTime,Length | Format-Table -AutoSize; Get-ChildItem -LiteralPath 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\bin' -Recurse -Filter '*.exe' | Select-Object FullName,LastWriteTime,Length | Format-Table -AutoSize`
> - Ran `Get-Item 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Debug\LasalMotionControlLib.Tests.exe','LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Release\LasalMotionControlLib.Tests.exe' | ForEach-Object { $_.FullName + '|' + $_.Length + '|' + $_.LastWriteTime.ToString('o') }`
> - Ran `& 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Debug\LasalMotionControlLib.Tests.exe'`
> - Ran `rg -n "TopologyUiFeatureMarker|CREVIS.*V5|FeatureMarker" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs LMC_Library/LasalApiWpfTestApp -g "*.md"`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $l=Get-Content -LiteralPath $p; for($i=1;$i -le 30;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}; for($i=330;$i -le 375;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> 첫 검증 결과가 나왔습니다.
>
> - 새 종료/재접속 시나리오 5개와 기존 `ErrorId=0` 회귀를 묶은 표적 테스트: `6/6 PASS`
> - SDK 전체 Debug 테스트: `1133/1133 PASS`
> - Release 빌드: PASS
> - 현재 전체 WPF Release smoke를 실행 중입니다.
>
> 또 새 바이너리를 확실히 구분할 수 있도록 기존 topology용 `V5` 표시는 유지하고, 시작 로그에 별도 `ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V1`, SDK DLL 경로와 빌드 시각을 남기도록 보강하고 있습니다. 다음 현장 재시험에서 이전 EXE/DLL을 잘못 실행했는지 바로 확인할 수 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('MSBuild.exe','LasalApiWpfTestApp.SmokeTests.exe','LasalMotionControlLib.Tests.exe') } | Select-Object ProcessId,Name,CreationDate,CommandLine | Format-List`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff --unified=0 -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs | Select-String -Pattern '^\+\s*"Wpf\.CallbackV2\.|^-\s*"Wpf\.CallbackV2\.'`
> - Ran `rg -n "ReconnectPolicy|SdkPath|SdkBuildUtc|TopologyUiFeatureMarker" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs | Select-Object -First 60`
>
> </details>
>
> <details><summary>Ran 9 commands</summary>
>
> - Ran `$files=@('LMC_Library/LasalApiWpfTestApp/README.md','LMC_Library/LasalApiWpfTestApp/API_MAPPING.md','LMC_Library/LasalApiWpfTestApp/DESIGN.md','LMC_Library/LMC_API_Delivery/README.md','LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md','LMC_Library/LMC_API_Delivery/docs/LMC_API/API_DEVELOPMENT_GUIDE.md','LMC_Library/LMC_API_Delivery/docs/LMC_API/API_USER_MANUAL_KO.md'); foreach($p in $files){ '### '+$p; $l=Get-Content -LiteralPath $p; $ranges=switch -Wildcard ($p){ '*LasalApiWpfTestApp/README.md' {@(@(1,90))} '*API_MAPPING.md' {@(@(1,115),@(140,155))} '*DESIGN.md' {@(@(60,112))} '*/README.md' {@(@(925,975))} '*AUTOMATED_TESTS*' {@(@(620,655),@(765,810))} '*API_DEVELOPMENT_GUIDE.md' {@(@(240,265))} '*API_USER_MANUAL_KO.md' {@(@(270,310))} }; foreach($range in $ranges){for($i=$range[0];$i -le [Math]::Min($range[1],$l.Count);$i++){'{0,5}: {1}' -f $i,$l[$i-1]}}; '' }`
> - Ran `rg --files LMC_Library | rg "API_DEVELOPMENT_GUIDE\.md$|API_USER_MANUAL_KO\.md$|LasalApiWpfTestApp/README\.md$|LasalApiWpfTestApp/DESIGN\.md$|LMC_API_Delivery/README\.md$"`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/README.md';$l=Get-Content -LiteralPath $p;for($i=1;$i -le [Math]::Min(95,$l.Count);$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/DESIGN.md';$l=Get-Content -LiteralPath $p;for($i=55;$i -le [Math]::Min(115,$l.Count);$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
> - Ran `$p='LMC_Library/LMC_API_Delivery/README.md';$l=Get-Content -LiteralPath $p;for($i=925;$i -le [Math]::Min(980,$l.Count);$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
> - Ran `$p='LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md';$l=Get-Content -LiteralPath $p;for($i=240;$i -le [Math]::Min(270,$l.Count);$i++){'{0,5}: {1}' -f $i,$l[$i-1]};'';$p='LMC_Library/LMC_API/API_USER_MANUAL_KO.md';$l=Get-Content -LiteralPath $p;for($i=270;$i -le [Math]::Min(315,$l.Count);$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
> - Ran `rg -n "PersistentInitFailureCleansUpAndManualReconnect|next manual Connect|다음 수동 Connect|af4ab63|ErrorId=0.*retry|same.socket.*retry|automatic reconnect|자동 reconnect|335/335|334/334" LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API_Delivery LMC_Library/LMC_API docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md -g "*.md" -g "*.txt" | Select-Object -First 250`
> - Ran `& 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Release\LasalMotionControlLib.Tests.exe'`
> - Ran `git status --short --untracked-files=no; git log -1 --oneline`
>
> </details>
>
> 코드 쪽은 동결됐습니다.
>
> - Debug WPF `339/339 PASS`
> - Release WPF `339/339 PASS`
> - SDK Debug/Release 각각 `1133/1133 PASS`
> - 독립 callback/reconnect 재검토 `9/9 PASS`, P0/P1 없음
> - 새 Debug EXE SHA-256: `28D461E2…D31A`
>
> 이제 코드·테스트 두 파일만 먼저 커밋해 고정한 뒤, 그 커밋과 검증 수치를 문서에 반영하는 별도 문서 커밋으로 마무리하겠습니다. 기존 `Classes.lcb`와 사용자의 다른 미추적 파일은 건드리지 않습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff --check -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs; git diff --numstat -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs; git add -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs; git diff --cached --check; git diff --cached --name-status`
> - Ran `git commit -m "fix(wpf): recover bounded RPC reconnect sessions"`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short --untracked-files=no; git show --stat --oneline --summary 14ccf58`
>
> </details>
>
> 코드·테스트는 `14ccf58`로 먼저 고정했습니다. 정확히 두 파일만 들어갔고, 사용자의 `Classes.lcb` 변경은 커밋에서 제외했습니다.
>
> 현재는 12개 관련 문서에서 예전 “실패 후 다음 수동 Connect” 설명을 새 계약으로 바꾸는 중입니다. 특히 다음을 명확히 분리합니다.
>
> - SDK: 같은 소켓 `20 ms` 1회
> - WPF: 실패 소켓 폐기 후 새 TCP `100 ms` 1회
> - X 종료: 로컬 정리 후 close 오류는 기록만 보존
> - 100 ms는 PLC 준비 완료 증명이 아니며, 실제 PLC 재시험은 별도
>
> 문서까지 독립 검토한 뒤 별도 커밋하겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "ReconnectPolicyMarker|EnsureCompleteLocalConnectionCleanupAsync|IsExactPersistentSessionInitMinusOneFailure|FreshSessionRetry=Scheduled|protected override async void OnClosing" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs; rg -n "ShutdownCloseMinusOneThenInitialFreshSessionRetrySucceeds|InitialSecondPersistentMinusOneFailureStopsBounded|ReconnectPersistentMinusOneUsesOneFreshSessionRetry|ReconnectSecondPersistentMinusOneFailureStopsBounded|ReconnectErrorZeroDoesNotUseFreshSessionRetry" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs`
> - Ran `$exe='LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Debug\LasalMotionControlApiExample.exe'; $dll='LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Debug\LasalMotionControlLib.dll'; Get-Item $exe,$dll | Select-Object FullName,Length,LastWriteTimeUtc | Format-Table -AutoSize; Get-FileHash -Algorithm SHA256 $exe,$dll | Select-Object Path,Hash | Format-Table -AutoSize`
> - Ran `$files=@('LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Debug\LasalMotionControlApiExample.exe','LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Debug\LasalMotionControlLib.dll'); foreach($f in $files){$i=Get-Item $f;$h=(Get-FileHash -Algorithm SHA256 $f).Hash; '{0}|{1}|{2}|{3}' -f $i.FullName,$i.Length,$i.LastWriteTimeUtc.ToString('o'),$h}`
>
> </details>
>
> 현재 코드 커밋은 그대로 유지되고 문서 편집만 진행 중입니다. 12개 문서의 오래된 `335/335`, “다음 수동 Connect”, legacy callback 설명 위치를 전부 매핑했습니다.
>
> 문서는 역사 기록을 지우지 않고:
>
> - 당시 `af4ab63 / 335/335`는 당시 결과로 보존
> - 현재 `14ccf58 / Debug·Release 339/339`를 별도 current 결과로 추가
> - 실제 PLC에서 확인되지 않은 100 ms 타이밍은 미검증으로 명시
>
> 하는 방식으로 정리하고 있습니다.
>
> 문서 작업도 병렬화했습니다.
>
> - 앞쪽 6개: 예제 README/API mapping/design, SDK README, 개발 가이드, 사용자 매뉴얼
> - 뒤쪽 6개: 자동 테스트, callback/RPC 설계, Gate D runbook, DINT map, architecture current status
>
> 두 묶음을 서로 다른 파일로 나눠 동시에 수정하고 있습니다. 역사적 `334/335` 결과는 삭제하지 않고 당시 스냅샷으로 남기며, 현재 결과만 `14ccf58 / 339`로 올립니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short --untracked-files=no; git diff --stat`
>
> </details>
>
> 12개 문서 패치가 모두 적용됐고 현재 모순 검사를 돌리고 있습니다. 변경은 reconnect 계약 설명에만 한정했으며, LASAL/PLC source나 기존 post-STOP 증거는 수정하지 않았습니다.
>
> 현재 문서 diff는 약 `+422/-148`이고, 사용자의 `Classes.lcb`는 여전히 별도 변경으로 남아 있으며 문서 커밋에 포함하지 않을 예정입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --check -- LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md LMC_Library/LMC_API/API_USER_MANUAL_KO.md LMC_Library/LMC_API_Delivery/README.md LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md LMC_Library/LasalApiWpfTestApp/API_MAPPING.md LMC_Library/LasalApiWpfTestApp/DESIGN.md LMC_Library/LasalApiWpfTestApp/README.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`
>
> </details>
>
> 문서 12개 수정도 완료됐습니다. 지금은 마지막 정밀 검사만 남았습니다.
>
> 추가로 잡은 오래된 모순도 함께 정리했습니다.
>
> - 개발 가이드의 “자동 retry 없음” 문구
> - legacy `0x405C 12/4`만 설명하던 부분
> - `Dispose`가 항상 `0x405D`를 보낸다는 과도한 설명
> - `1117/335`를 여전히 current로 부르던 문장
> - callback 단계 실패도 outer retry할 수 있는 것처럼 읽히는 표현
>
> 최종 diff-check와 독립 문서 리뷰 후 커밋하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 코드 수정은 이미 독립 검토와 Debug/Release 전체 테스트까지 통과했습니다. 지금은 동결된 12개 문서를 코드와 한 번 더 대조한 뒤, 사용자 파일을 제외하고 문서만 별도 커밋하겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$docPaths = @(
> 'LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md',
> 'LMC_Library/LMC_API/API_USER_MANUAL_KO.md',
> 'LMC_Library/LMC_API_Delivery/README.md',
> 'LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md',
> 'LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md',
> 'LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt',
> 'LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md',
> 'LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md',
> 'LMC_Library/LasalApiWpfTestApp/API_MAPPING.md',
> 'LMC_Library/LasalApiWpfTestApp/DESIGN.md',
> 'LMC_Library/LasalApiWpfTestApp/README.md',
> 'docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'
> )
> git status --short
> git diff --stat -- $docPaths
> git diff --check -- $docPaths
> $runbook = $docPaths[6]
> $lines = Get-Content -LiteralPath $runbook
> for ($i = 850; $i -le [Math]::Min(866, $lines.Count); $i++) { '{0}: {1}' -f $i, $lines[$i-1] }`
>

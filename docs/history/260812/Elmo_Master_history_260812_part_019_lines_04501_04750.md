> $docs=@(
> 'docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md',
> 'LMC_Library/LasalApiWpfTestApp/DESIGN.md',
> 'LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md',
> 'LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md',
> 'LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md'
> )
> rg -n -i --pcre2 'TODO|FIXME|미완료|대기|필수|remaining|required|not implemented|아직|별도.*필요' -- $docs | Select-Object -First 220`
> - Ran `$path='docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'
> $lines=Get-Content -LiteralPath $path
> foreach($range in @(@(1035,1085),@(1086,1130),@(900,965),@(800,830))){
>   "RANGE $($range[0])-$($range[1])"
>   for($i=$range[0];$i -le [Math]::Min($range[1],$lines.Count);$i++){ '{0}: {1}' -f $i,$lines[$i-1] }
> }`
>
> </details>
>
> 현재 설계의 명시적 P0 중 PLC 없이 가능한 항목은 배포용 DOCX/PDF 경고·계약 최신화이고, 재접속 쪽에는 실제 EXE 재실행을 자동 검증하지 못한 한계가 남아 있습니다. 둘 중 무엇이 ‘필수 다음 단계’인지 소스·빌드 파이프라인 기준으로 우선순위를 확정하고 있습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n -i "DOCX|PDF|API_USER_MANUAL|03_API_User_Manual|Render|pandoc|manual" LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LMC_API_Delivery LMC_Library/LMC_API_Distribution -g '*.ps1' -g '*.md' -g '*.txt' -g '*.csproj' | Select-Object -First 220
> "DOC_FILES"
> Get-ChildItem -LiteralPath 'LMC_Library' -Recurse -File -Include *.docx,*.pdf | Where-Object { $_.FullName -match 'API|Manual|Distribution' } | Select-Object FullName,Length,LastWriteTimeUtc`
> - Ran `rg -n -i "API_USER_MANUAL_KO\.md|LASAL_Motion_Control_API_User_Manual_KO|2\.0-candidate|generate.*docx|docx.*pdf" . -g '*.py' -g '*.ps1' -g '*.md' | Select-Object -First 260`
> - Ran `$paths=@(
> 'LMC_Library/LMC_API/Generate-ApiUserManualDocx.py',
> 'output/doc/LASAL_Motion_Control_API_User_Manual_KO_2.0-candidate.docx',
> 'output/pdf/LASAL_Motion_Control_API_User_Manual_KO_2.0-candidate.pdf'
> )
> foreach($p in $paths){
>   if(Test-Path -LiteralPath $p){$i=Get-Item -LiteralPath $p; $h=(Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash; "$p|EXISTS|$($i.Length)|$h|tracked=$([bool](git ls-files --error-unmatch -- $p 2>$null))"} else {"$p|ABSENT"}
> }
> Get-ChildItem -LiteralPath 'LMC_Library/LMC_API' -File -Filter '*Manual*' | Select-Object Name,Length,LastWriteTimeUtc
> Get-ChildItem -LiteralPath 'output/doc' -File -ErrorAction SilentlyContinue | Where-Object Name -match 'Manual' | Select-Object Name,Length,LastWriteTimeUtc
> Get-ChildItem -LiteralPath 'output/pdf' -File -ErrorAction SilentlyContinue | Where-Object Name -match 'Manual' | Select-Object Name,Length,LastWriteTimeUtc`
> - Ran `$path='docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md'
> $lines=Get-Content -LiteralPath $path
> foreach($range in @(@(25,60),@(290,335),@(425,455))){"RANGE $($range[0])-$($range[1])";for($i=$range[0];$i -le $range[1];$i++){'{0}: {1}' -f $i,$lines[$i-1]}}`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n -i "Mutex|OnStartup|Environment\.GetCommandLineArgs|CommandLine|smoke|test hook|Named" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/*.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/*.cs | Select-Object -First 260
> "PROJECT_REFS"
> Get-Content -LiteralPath 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj' | Select-Object -First 280
> "APP_XAML"
> Get-Content -LiteralPath 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml' | Select-Object -First 120`
> - Ran `$path='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs'
> $lines=Get-Content -LiteralPath $path
> for($i=1;$i -le $lines.Count;$i++){'{0}: {1}' -f $i,$lines[$i-1]}
> "PROCESS_TEST_HITS"
> rg -n -i "ProcessStartInfo|Process\.Start|child process|ChildProcess|Environment\.GetCommandLineArgs|AppDomain|Mutex|ApplicationInstanceLease" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp -g '*.cs' | Select-Object -First 300`
> - Ran `rg -n "GetEnvironmentVariable|Environment\." LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp -g '*.cs' | Select-Object -First 240
> rg -n "TextRemote|Remote.*Port|ButtonConnect|Connect" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml | Select-Object -First 140`
>
> </details>
>
> 설계상 다음 큰 코드 항목은 LASAL `ReserveAxisOwnership` 분리지만, 새 private method 선언은 IDE 작업이 필요하고 현재 사용자의 생성물 변경과 충돌할 수 있어 지금 바로 건드리면 안 됩니다. 그래서 우선 사용자가 실제로 겪은 “프로세스 종료 후 재실행” 경계를 별도 프로세스 회귀로 닫는 방안을 검토하고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n "ReserveAxisOwnership|8\.6|Reservation" docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface.st LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 | Select-Object -First 200`
>
> </details>
>
> 우선순위는 정리됐습니다. 기능 계약만 보면 실제 EXE 재실행 시험은 선택 사항이지만, 사용자가 보고한 정확한 증상을 “고쳤다”고 닫으려면 P1 필수입니다. 현재 시험은 같은 프로세스의 새 창만 검증하므로, 실제 프로세스 teardown·mutex·실행 EXE/DLL provenance까지 포함하는 회귀를 다음 tranche로 잡겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$env:LOCALAPPDATA='C:\Temp\CodexProbeLocalAppData'
> [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)`
> - Ran `rg -n -C 4 "MainWindow\(" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs | Select-Object -First 180
> rg -n -C 3 "internal MainWindow|public MainWindow" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp -g '*.cs' | Select-Object -First 200`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$path='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs'
> $lines=Get-Content -LiteralPath $path
> for($i=1;$i -le [Math]::Min(220,$lines.Count);$i++){'{0}: {1}' -f $i,$lines[$i-1]}
> "CSPROJ_APP"
> Get-Content -LiteralPath 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj' | Select-Object -First 260`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 설계상 다음 LASAL 필수 작업은 IDE 선언 변경 없이는 안전하게 못 합니다. 그래서 지금은 사용자 증상을 정확히 겨냥한 PC 전용 보강—실제 EXE 종료·재실행, named mutex 재획득, 동일 fake endpoint에서 fresh-TCP 복구—를 구현하겠습니다. PLC/LASAL/Download와 사용자 AppData는 건드리지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short; git log -3 --oneline`
>
> </details>
>
> 구현 범위는 실제 배포 후보 EXE까지 묶었습니다. 핵심 PASS 조건은 “창 객체 재생성”이 아니라 첫 프로세스 완전 종료, 두 번째 프로세스의 동일 mutex 재획득, 동일 endpoint에서 정확히 3개 TCP 세션입니다. 지금은 구현과 별개로 실패 가능성이 큰 App startup/자동 Connect/종료 경계를 교차검토하고 있습니다.
>
> <details><summary>Ran 10 commands</summary>
>
> - Ran `rg -n "OnStartup|ApplicationInstanceLease|MainWindow\(|Connect|ButtonConnect|OnClosing|Close\(|TextServer|TextPort|journal" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs | Select-Object -First 220`
> - Ran `Get-Content LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs -TotalCount 120; rg -n "x:Name=\"(TextServer|TextPort|TextCallback|ButtonConnect|TextConnectionState|TextExecutionLog)\"" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml`
> - Ran `Select-String -Path 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.xaml' -Pattern 'TextServer|TextPort|TextCallback|ButtonConnect|TextConnectionState|TextExecutionLog'`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.xaml' | Select-Object -Skip 85 -First 70`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.xaml.cs' | Select-Object -Skip 90 -First 185`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfMainWindowIntegrationTests.cs' | Select-Object -Skip 130 -First 300`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfMainWindowIntegrationTests.cs' -TotalCount 60; Select-String -Path 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfMainWindowIntegrationTests.cs' -Pattern 'CreateConnectAndTopologySteps|FakeRpcServer' | Select-Object -Last 20`
> - Ran `Get-Content 'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1' | Select-Object -Skip 760 -First 190`
> - Ran `rg -n "function Invoke-LmcMSBuild|RunWpfSmokeTests|TargetPath" LMC_Library/LMC_API/Build-LmcApiDistribution.ps1 LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj`
> - Ran `Get-Content 'LMC_Library\LMC_API\Build-LmcApiDistribution.ps1' | Select-Object -Skip 570 -First 70; Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj' | Select-Object -Skip 75 -First 25`
>
> </details>
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `rg -n "class FakeRpcServer|Completed|Step|AcceptedClientCount|Wait" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/FakeRpcServer.cs | Select-Object -First 200`
> - Ran `rg --files LMC_Library | rg 'FakeRpcServer\.cs$|Fake.*Rpc.*Server'`
> - Ran `rg -n "class FakeRpcServer|Completed|AcceptedClientCount|Verify|Request" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/FakeRpcServer.cs | Select-Object -First 220`
> - Ran `Get-Content 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\FakeRpcServer.cs' | Select-Object -Skip 45 -First 250`
> - Ran `rg -n "FileEntries|Files|RelativePath|SHA256|Example" LMC_Library/LMC_API/ReleaseManifest.ps1 | Select-Object -First 150`
> - Ran `Get-Content 'LMC_Library\LMC_API\ReleaseManifest.ps1' | Select-Object -Skip 185 -First 155`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\Program.cs' -TotalCount 220`
>
> </details>
>
> 실제 EXE 경로에서 자동 topology read가 `0x405C` 뒤에 추가로 발생하므로, 테스트는 이를 숨기지 않고 read-only wire로 명시적으로 검증합니다. 첫 프로세스의 close NACK 이후 종료를 확인한 다음에만 두 번째 EXE를 띄우며, 테스트한 Run EXE/DLL/config는 전후 해시가 같아야 다음 manifest 단계로 넘어가게 합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1; git diff --stat -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\ApplicationInstanceLease.cs' -TotalCount 220`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "WaitForConnectCompleted|ButtonConnect.IsEnabled|TextConnectionState.Text" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs | Select-Object -First 80`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfMainWindowIntegrationTests.cs' | Select-Object -Skip 910 -First 70`
>
> </details>
>
> probe 자체도 fail-closed로 묶었습니다. 잘못된 probe 인수는 named mutex·저널·TCP 전에 종료되고, 유효한 모드에서는 원격/로컬 IP를 모두 `127.0.0.1`, callback 포트를 `0`으로 고정합니다. 실제 Connect handler가 예외를 UI 안에서 처리하므로 단순 프로세스 생존이 아니라 `Connect completed + Connected + Close enabled`까지 확인합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1; git diff --stat -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
>
> </details>
>
> 설계 고정이 끝났고 5파일 구현 패치에 들어갔습니다. child는 연결 완료 후 PID/HWND/phase를 temp-owned report로 내고 대기하며, parent가 실제 `WM_CLOSE`를 보내 종료시킵니다. 첫 프로세스 종료가 확인되기 전에는 두 번째를 시작하지 않습니다. 예상 구현·초기 빌드까지 약 45분입니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "actual EXE|실제 EXE|relaunch|재실행|new MainWindow|same test process|same-process|named mutex|named-mutex|339/339|ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V1" LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API_Delivery docs test/Reports_Lasal | Select-Object -First 300`
> - Ran `git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1; git diff --stat -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
> - Ran `Get-Process LasalMotionControlApiExample -ErrorAction SilentlyContinue | Select-Object Id,Path,StartTime; Get-Process Lasal2 -ErrorAction SilentlyContinue | Select-Object Id,Path,StartTime`
>
> </details>
>
> 현재 실행 중인 예제나 LASAL 프로세스는 없어 실제 child-process gate를 돌릴 환경 충돌도 없습니다. 패치는 중간 반쪽 상태를 만들지 않도록 인수 검증·App 격리·parent wire 오라클을 한 번에 조립 중입니다. 구현이 들어오면 먼저 invalid-arg 0-network와 동일 EXE 2회 경로만 targeted로 돌린 뒤 전수 테스트로 확장합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
>
> </details>
>
> 첫 패치가 들어왔습니다. 현재 App startup probe 분기와 smoke-runner 전용 CLI/프로젝트 등록까지 적용됐고, 다음 패치에서 child probe 구현·wire gate·distribution 호출을 붙입니다. 아직 중간 상태이므로 지금은 빌드 판정을 내리지 않고 파일 범위만 감시합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1; git diff --stat -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs | Select-Object -First 800`
>
> </details>
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `rg -n "running|completed|failed" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs | rg 'operationName|TextOperationState|OperationState' | Select-Object -First 80`
> - Ran `git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1; git diff --stat -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
> - Ran `(Get-Item 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfExecutableRelaunchIntegrationTests.cs').Length; (Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfExecutableRelaunchIntegrationTests.cs').Count; Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfExecutableRelaunchIntegrationTests.cs' -TotalCount 80`
> - Ran `rg -n "RunGate|Verify|StartProcess|contender|first|second|WM|SendMessage|Create.*Steps|Delete|Kill|Hash|config|Report|MUTEX|PASS" 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfExecutableRelaunchIntegrationTests.cs' | Select-Object -First 260`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfExecutableRelaunchIntegrationTests.cs' | Select-Object -Skip 80 -First 560`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfExecutableRelaunchIntegrationTests.cs' | Select-Object -Skip 900 -First 285`
>
> </details>
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n "CloseCompletion|PASS_BY|Window_Closed|TextConnectionState" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\App.xaml.cs' | Select-Object -Skip 245 -First 135`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.xaml.cs' | Select-Object -Skip 9560 -First 150`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.xaml.cs' | Select-Object -Skip 9680 -First 105`
>
> </details>
>
> 초기 빌드는 통과했지만 회귀 하네스에서 두 가지를 바로 차단했습니다. 예외 시 child가 살아남아 mutex를 잡는 경로와, owner READY 뒤 contender 실행 중 fake server의 3초 read timeout에 의존하는 경로입니다. 둘 다 deterministic cleanup/barrier로 고친 뒤에만 실제 PASS를 인정합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1; git diff --stat -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
> - Ran `(Get-Item 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\ExecutableRelaunchProbe.cs').Length; (Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\ExecutableRelaunchProbe.cs').Count; git diff -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs | Select-Object -First 250`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n "DisposeProcess|Terminate|Barrier|ManualResetEventSlim|ReportTimeoutMilliseconds|ProcessTimeoutMilliseconds|WaitFor.*Contender|AfterResponse" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfExecutableRelaunchIntegrationTests.cs`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfExecutableRelaunchIntegrationTests.cs' | Select-Object -Skip 115 -First 235; Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfExecutableRelaunchIntegrationTests.cs' | Select-Object -Skip 540 -First 150`
>
> </details>
>
> <details><summary>Ran 7 commands</summary>
>
> - Ran `git diff -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj`
> - Ran `git diff -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfExecutableRelaunchIntegrationTests.cs' | Select-Object -Skip 500 -First 40`
> - Ran `rg -n "fields.Count|Unknown|RequiredFields|17" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfExecutableRelaunchIntegrationTests.cs | Select-Object -First 30`
> - Ran `Get-Content 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfExecutableRelaunchIntegrationTests.cs' | Select-Object -Skip 1010 -First 90`
> - Ran `git status --short -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1; git diff --stat -- LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
> - Ran `git diff -- LMC_Library/LMC_API/Build-LmcApiDistribution.ps1`
>
> </details>

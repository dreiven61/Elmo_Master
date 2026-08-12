> - Ran `$start=(Get-Date).AddHours(-2); Get-WinEvent -FilterHashtable @{LogName='Application';StartTime=$start} -ErrorAction SilentlyContinue | Where-Object {$_.ProviderName -in @('.NET Runtime','Application Error','Windows Error Reporting') -or $_.Message -match 'LasalMotionControlApiExample|LasalApiWpfTestApp|LMC'} | Select-Object -First 20 TimeCreated,ProviderName,Id,LevelDisplayName,Message | Format-List`
> - Ran `$cut=(Get-Date).AddHours(-3); Get-ChildItem -LiteralPath 'LMC_Library/LasalApiWpfTestApp' -Recurse -File -ErrorAction SilentlyContinue | Where-Object {$_.LastWriteTime -ge $cut -and $_.Extension -in @('.log','.txt','.json','.dmp')} | Sort-Object LastWriteTime -Descending | Select-Object -First 50 FullName,Length,LastWriteTime | Format-Table -AutoSize`
> - Ran `$cut=(Get-Date).AddHours(-3); Get-ChildItem -LiteralPath ([IO.Path]::GetTempPath()) -File -ErrorAction SilentlyContinue | Where-Object {$_.LastWriteTime -ge $cut -and ($_.Name -match 'Lasal|Lmc|Wpf|Elmo|Motion|error|log')} | Sort-Object LastWriteTime -Descending | Select-Object -First 50 FullName,Length,LastWriteTime | Format-Table -AutoSize`
> - Ran `rg --files LMC_Library | rg -i "example|sample" | Select-Object -First 200`
>
> </details>
>
> 한 가지 유력한 원인이 바로 보입니다. 우리가 고친 대상은 개발용 `LasalApiWpfTestApp`인데, 사용자가 실행하는 “예제파일”은 별도 복사본인 `LMC_API_Distribution/02_Example_Program/Run/LasalMotionControlApiExample.exe`일 가능성이 큽니다. 이 배포 예제 소스·DLL·EXE가 최신 reconnect 수정과 동기화됐는지 지금 바이트와 코드로 확인하겠습니다.
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `$a='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $b='LMC_Library/LMC_API_Distribution/02_Example_Program/LasalApiWpfTestApp/MainWindow.xaml.cs'; [pscustomobject]@{CanonicalBytes=(Get-Item $a).Length;CanonicalSha=(Get-FileHash -Algorithm SHA256 $a).Hash;DistributionBytes=(Get-Item $b).Length;DistributionSha=(Get-FileHash -Algorithm SHA256 $b).Hash;Same=((Get-FileHash -Algorithm SHA256 $a).Hash -eq (Get-FileHash -Algorithm SHA256 $b).Hash)} | ConvertTo-Json -Compress; git diff --no-index --stat -- $b $a; exit 0`
> - Ran `$files=Get-ChildItem -LiteralPath 'LMC_Library' -Recurse -File -Filter 'LasalMotionControlApiExample.exe' -ErrorAction SilentlyContinue; $files | ForEach-Object { $vi=$_.VersionInfo; [pscustomobject]@{Path=$_.FullName;Bytes=$_.Length;Sha256=(Get-FileHash -Algorithm SHA256 -LiteralPath $_.FullName).Hash;LastWriteUtc=$_.LastWriteTimeUtc.ToString('o');FileVersion=$vi.FileVersion;ProductVersion=$vi.ProductVersion} } | ConvertTo-Json -Depth 4`
> - Ran `$files=Get-ChildItem -LiteralPath 'LMC_Library' -Recurse -File -Filter 'LasalMotionControlLib.dll' -ErrorAction SilentlyContinue; $files | ForEach-Object { $vi=$_.VersionInfo; [pscustomobject]@{Path=$_.FullName;Bytes=$_.Length;Sha256=(Get-FileHash -Algorithm SHA256 -LiteralPath $_.FullName).Hash;LastWriteUtc=$_.LastWriteTimeUtc.ToString('o');FileVersion=$vi.FileVersion} } | ConvertTo-Json -Depth 4`
> - Ran `git log -1 --format='%H %cI %s' -- 'LMC_Library/LMC_API_Distribution/02_Example_Program/Run/LasalMotionControlApiExample.exe' 'LMC_Library/LMC_API_Distribution/02_Example_Program/Run/LasalMotionControlLib.dll'; git status --short -- 'LMC_Library/LMC_API_Distribution/02_Example_Program'`
> - Ran `rg --files LMC_Library | rg -i "(distribution|package|publish|release).*(ps1|cs|md|json|cmd|bat)$|\.ps1$" | Select-Object -First 300`
> - Ran `Get-Content -LiteralPath 'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1' -TotalCount 260; '---PIPELINE---'; Get-Content -LiteralPath 'LMC_Library/LMC_API/DistributionPipeline.ps1' -TotalCount 220; '---README---'; Get-Content -LiteralPath 'LMC_Library/LMC_API_Distribution/README.md' -TotalCount 220`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 원인은 거의 좁혀졌습니다. 사용자가 실행하는 배포 예제 EXE/DLL은 7월 30일 산출물이고, 이후 반영한 재접속 수정은 개발용 소스·테스트에만 들어가 배포 예제에는 포함되지 않았습니다. 지금 배포 생성 경로를 확인해 실제 예제까지 동기화하고, 재접속 회귀 테스트와 패키지 산출물을 함께 검증하겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "CandidatePath|AllowDirty|Move-Item|Rename|RunPcTests|RunWpf|Copy-LmcDevelopmentExample|RELEASE_MANIFEST|Distribution" "LMC_Library/LMC_API/Build-LmcApiDistribution.ps1"; Get-Content -LiteralPath "LMC_Library/LMC_API/Build-LmcApiDistribution.ps1" | Select-Object -Last 260; git status --short -- "LMC_Library/LMC_API_Distribution" "LMC_Library/LasalApiWpfTestApp" "LMC_Library/LMC_API_Delivery"`
> - Ran `rg -n "Promote|CandidateTransaction|CanonicalRoot|candidate completed|Canonical distribution preserved|LMC_API_Distribution_candidate|Build-LmcApiDistribution" "LMC_Library/LMC_API" "LMC_Library/LMC_API_Delivery" "docs" -g "*.ps1" -g "*.md" -g "*.txt"; Get-Content -LiteralPath "LMC_Library/LMC_API/DistributionPipeline.ps1" -TotalCount 360; git log --oneline --decorate -12 -- "LMC_Library/LMC_API_Distribution" "LMC_Library/LMC_API/Build-LmcApiDistribution.ps1"`
> - Ran `Get-Content -LiteralPath "LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md" | Select-Object -Skip 1140 -First 70; Get-Content -LiteralPath "docs/architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md" | Select-Object -Skip 125 -First 70; Get-ChildItem -LiteralPath "LMC_Library" -Directory -Filter "LMC_API_Distribution_candidate_*" | Select-Object Name,LastWriteTime; git status --short --untracked-files=all | Select-Object -First 80`
>
> </details>
>
> 현재 확인된 핵심은 “수정이 틀렸다”기보다 “수정된 프로그램을 실행하지 않았다”에 가깝습니다. 저장소의 실제 배포 예제는 개발용 예제보다 약 1,400줄 뒤처져 있고, 실행 파일도 재접속 수정 전 날짜입니다. 다만 배포본만 새로 만들기 전에, 현재 재접속 경로 자체에 남은 결함이 없는지도 병렬로 확인 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$dev='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp'; $dist='LMC_Library/LMC_API_Distribution/02_Example_Program/LasalApiWpfTestApp'; git diff --no-index --stat -- $dist $dev; rg -n "RetryRpcInitialization|CallbackLocalEndPoint|RequestedCallback|BoundCallback|ErrorZero|Reconnect|CloseCurrentConnectionAsync|RpcInitConnectionAsync" $dev $dist -g "*.cs"; Get-Content -LiteralPath "$dist/LasalApiWpfTestApp.csproj" -TotalCount 240; Get-Content -LiteralPath "$dev/LasalApiWpfTestApp.csproj" -TotalCount 240`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(130,250),@(250,345),@(470,535),@(630,720))){"RANGE $($range[0])-$($range[1])"; for($i=$range[0];$i -le $range[1];$i++){ if($i -le $lines.Count){'{0,4}: {1}' -f $i,$lines[$i-1]}}}; rg -n "LmcConnection|MainWindow.xaml.cs|source set|candidate WPF|byte-identical|DevelopmentExample" "LMC_Library/LMC_API/DistributionSemanticPolicy.ps1" "LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1"`
>
> </details>
>
> 한 가지 유력한 원인이 바로 보입니다. 우리가 고친 대상은 개발용 LasalApiWpfTestApp인데, 사용자가 실행하는 “예제파일”은 별도 복사본인 LMC_API_Distribution/02_Example_Program/Run/LasalMotionControlApiExample.exe일 가능성이 큽니다. 이 배포 예제 소스·DLL·EXE가 최신 reconnect 수정과 동기화됐는지 지금 바이트와 코드로 확인하겠습니다.
>
> 아냐. LasalApiWpfTestApp 이걸로 테스트 했는데 문제생겼ㅆ어
>
> 알겠습니다. 배포본 가설은 이번 오류 원인에서 제외하겠습니다. `LasalApiWpfTestApp`의 실제 재접속 순서로 다시 보겠습니다. 지금 가장 의심되는 부분은 이전 연결을 `Dispose`한 직후 새 `0x8080`을 보내 PLC의 이전 callback owner 정리가 끝나기 전에 충돌하는 경로입니다. 정확한 오류 문구가 있으면 보내 주세요. 없어도 현재 코드와 fake-peer로 그 경로를 재현해 수정하겠습니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `$mw='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $lc='LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib/LmcConnection.cs'; $wt='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/WpfMainWindowIntegrationTests.cs'; $lines=Get-Content -LiteralPath $mw; foreach($range in @(@(410,610),@(5380,5515),@(8980,9110))){"MAIN $($range[0])-$($range[1])"; for($i=$range[0];$i -le $range[1];$i++){if($i -le $lines.Count){'{0,5}: {1}' -f $i,$lines[$i-1]}}}; $lines=Get-Content -LiteralPath $lc; foreach($range in @(@(1450,1625),@(535,720),@(2720,2860))){"LMC $($range[0])-$($range[1])"; for($i=$range[0];$i -le $range[1];$i++){if($i -le $lines.Count){'{0,5}: {1}' -f $i,$lines[$i-1]}}}; rg -n "ErrorZero|PersistentInit|Reconnect|CloseCurrentConnection|RpcInit|0x405D|Unregister|DisconnectAsync|Dispose" $wt $lc $mw`
> - Ran `rg --files "LMC_Library/LMC_API_Delivery" | rg "(LmcConnection\.cs|WpfMainWindowIntegrationTests\.cs)$"; rg -n "class LMCConnection" "LMC_Library" -g "*.cs" | Select-Object -First 20; rg -n "ErrorZeroInitFailure|PersistentInitFailure" "LMC_Library" -g "*.cs"`
> - Ran `$lc='LMC_Library/LMC_API_Delivery/src/LmcConnection.cs'; $wt='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs'; $mw='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $lines=Get-Content -LiteralPath $lc; foreach($range in @(@(1460,1630),@(2710,2865),@(3220,3385))){"LMC $($range[0])-$($range[1])"; for($i=$range[0];$i -le $range[1];$i++){if($i -le $lines.Count){'{0,5}: {1}' -f $i,$lines[$i-1]}}}; $lines=Get-Content -LiteralPath $mw; "MAIN 5250-5355"; for($i=5250;$i -le 5355;$i++){if($i -le $lines.Count){'{0,5}: {1}' -f $i,$lines[$i-1]}}; $lines=Get-Content -LiteralPath $wt; foreach($range in @(@(130,390),@(5700,5840))){"TEST $($range[0])-$($range[1])"; for($i=$range[0];$i -le $range[1];$i++){if($i -le $lines.Count){'{0,5}: {1}' -f $i,$lines[$i-1]}}}`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $l=Get-Content -LiteralPath $p; for($i=5250;$i -le 5360;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}; $p='LMC_Library/LMC_API_Delivery/src/LmcConnection.cs'; rg -n "public.*Close|CloseAsync|Disconnect|Unregister|0x405D|RpcCallback.*Unregister|Dispose\(" $p; $l=Get-Content -LiteralPath $p; foreach($range in @(@(315,500),@(800,1035),@(1290,1460),@(2980,3225),@(3380,3515))){"RANGE $($range[0])-$($range[1])"; for($i=$range[0];$i -le $range[1];$i++){if($i -le $l.Count){'{0,5}: {1}' -f $i,$l[$i-1]}}}`
> - Ran `$p='LMC_Library/LMC_API_Delivery/src/LmcConnection.cs'; $l=Get-Content -LiteralPath $p; foreach($range in @(@(280,325),@(1160,1335),@(1960,2085))){"RANGE $($range[0])-$($range[1])"; for($i=$range[0];$i -le $range[1];$i++){'{0,5}: {1}' -f $i,$l[$i-1]}}; rg -n "RpcClose\(|RpcClose|SessionClose|405D" "LMC_Library/LMC_API_Delivery/src" -g "*.cs"`
>
> </details>
>
> 정정 하나 더 필요합니다. PC에 `LasalApiWpfTestApp`이 두 벌 있습니다.
>
> - 저장소 개발본: `C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp`
> - 별도 예제본: `C:\work\Elmo\LMC_API\02_Example_Program\LasalApiWpfTestApp`
>
> 두 번째 쪽은 오늘 다시 빌드됐지만 참조 DLL이 7월 16일 구버전이라 최신 재접속 코드가 전혀 들어가지 않았습니다. 어느 쪽을 실제 실행했는지 최근 VS/실행 흔적으로 확정하는 동시에, 개발본 자체의 ErrorId=0 재접속 처리도 다시 검토하고 있습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `$paths=@('C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp','C:\work\Elmo\LMC_API\02_Example_Program\LasalApiWpfTestApp'); foreach($p in $paths){"ROOT=$p"; if(Test-Path -LiteralPath $p){Get-ChildItem -LiteralPath $p -Recurse -File -Include *.sln,*.csproj,LasalMotionControlApiExample.exe,LasalMotionControlLib.dll | Where-Object {$_.FullName -notmatch '\\obj\\'} | Select-Object FullName,Length,LastWriteTime | Sort-Object FullName}}; Get-Process devenv -ErrorAction SilentlyContinue | Select-Object Id,StartTime,MainWindowTitle,Path; Get-CimInstance Win32_Process -Filter "Name='devenv.exe'" -ErrorAction SilentlyContinue | Select-Object ProcessId,CommandLine,ExecutablePath`
> - Ran `$files=@('C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.xaml.cs','C:\work\Elmo\LMC_API\02_Example_Program\LasalApiWpfTestApp\MainWindow.xaml.cs','C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution\02_Example_Program\LasalApiWpfTestApp\MainWindow.xaml.cs','C:\work\Elmo\LMC_API\02_Example_Program\LasalApiWpfTestApp\bin\Debug\LasalMotionControlApiExample.exe','C:\work\Elmo\LMC_API\02_Example_Program\LasalApiWpfTestApp\bin\Debug\LasalMotionControlLib.dll'); foreach($f in $files){if(Test-Path -LiteralPath $f){$i=Get-Item -LiteralPath $f; $h=(Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash; "$($i.FullName)|$($i.Length)|$($i.LastWriteTime.ToString('O'))|$h"}}; rg -n "CallbackRegistrationMode|Version2WakeHint|ErrorZero|RequestedCallback|BoundCallback" "C:\work\Elmo\LMC_API\02_Example_Program\LasalApiWpfTestApp" -g "*.cs"; Get-Content -LiteralPath "C:\work\Elmo\LMC_API\02_Example_Program\LasalApiWpfTestApp\LasalApiWpfTestApp.csproj" -TotalCount 90`
> - Ran `$root='C:\work\Elmo\LMC_API'; Get-ChildItem -LiteralPath $root -Force | Select-Object Name,Mode,Length,LastWriteTime; if(Test-Path -LiteralPath "$root\RELEASE_MANIFEST.md"){Get-Content -LiteralPath "$root\RELEASE_MANIFEST.md" -TotalCount 30}; Get-ChildItem -LiteralPath "$root\02_Example_Program\.vs\LasalApiWpfTestApp\v16" -Force -ErrorAction SilentlyContinue | Select-Object FullName,Length,LastWriteTime,LastAccessTime; Get-ChildItem -LiteralPath "$root\02_Example_Program" -Force | Select-Object Name,Mode,Length,LastWriteTime; rg -n "CallbackRegistrationMode|Version2WakeHint|RpcInitConnection|CloseCurrentConnectionAsync" "$root\02_Example_Program\LasalApiWpfTestApp\MainWindow.xaml.cs" "$root\01_API" -g "*.cs" -g "*.md"`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; $l=Get-Content -LiteralPath $p; foreach($range in @(@(1640,1710),@(2590,2665))){"RANGE $($range[0])-$($range[1])"; for($i=$range[0];$i -le $range[1];$i++){'{0,5}: {1}' -f $i,$l[$i-1]}}; $p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st'; $l=Get-Content -LiteralPath $p; for($i=380;$i -le 455;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> 확인했습니다. 사용하신 이름은 맞지만, 최근 실행 흔적이 가리키는 실제 경로는 저장소 개발본이 아니라 `C:\work\Elmo\LMC_API\02_Example_Program\LasalApiWpfTestApp`입니다. 이 솔루션은 오늘 빌드됐어도 7월 16일 DLL을 계속 참조합니다. 현재 PLC는 실패를 `ErrorId=-1`로 보내는데 구 DLL은 이를 `0`처럼 보이게 하므로, 보신 현상과도 정확히 맞습니다. 최신 소스·DLL·EXE가 한 세트인 새 예제 패키지를 지금 만들고 전체 테스트를 돌리겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$candidate='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution_candidate_20260811_reconnect_current_01'; if(Test-Path -LiteralPath $candidate){throw "Candidate already exists: $candidate"}; $logRoot=Join-Path $env:TEMP ('LmcReconnectPackage-' + [guid]::NewGuid().ToString('N')); New-Item -ItemType Directory -Path $logRoot | Out-Null; $stdout=Join-Path $logRoot 'stdout.log'; $stderr=Join-Path $logRoot 'stderr.log'; $args=@('-NoProfile','-ExecutionPolicy','Bypass','-File','C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Build-LmcApiDistribution.ps1','-RepositoryRoot','C:\work\Elmo\Elmo_Master','-AllowDirty','-CandidatePath',$candidate); $proc=Start-Process -FilePath 'powershell.exe' -ArgumentList $args -WorkingDirectory 'C:\work\Elmo\Elmo_Master' -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru; [pscustomobject]@{Pid=$proc.Id;Candidate=$candidate;LogRoot=$logRoot;Stdout=$stdout;Stderr=$stderr}|ConvertTo-Json -Compress`
> - Ran `$pidToCheck=27712; $p=Get-Process -Id $pidToCheck -ErrorAction SilentlyContinue; "RUNNING=$([bool]$p)"; if($p){"CPU=$($p.CPU) START=$($p.StartTime.ToString('O'))"}; $out='C:\Users\dreiv\AppData\Local\Temp\LmcReconnectPackage-a180da096635488c8916ff08a1a5b78b\stdout.log'; $err='C:\Users\dreiv\AppData\Local\Temp\LmcReconnectPackage-a180da096635488c8916ff08a1a5b78b\stderr.log'; if(Test-Path $out){Get-Content -LiteralPath $out -Tail 30}; if(Test-Path $err){'STDERR';Get-Content -LiteralPath $err -Tail 30}`
>
> </details>
>
> 첫 패키지 실행은 코드 문제와 무관하게, 이 PC의 `powershell.exe` 세션에서 `Get-FileHash` 모듈을 찾지 못해 시작 단계에서 중단됐습니다. candidate나 기존 예제는 건드리지 않았습니다. 같은 스크립트를 현재 지원되는 PowerShell 7로 다시 실행합니다.
>
> <details><summary>Ran 8 commands</summary>
>
> - Ran `$candidate='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution_candidate_20260811_reconnect_current_01'; "CANDIDATE_EXISTS=$(Test-Path -LiteralPath $candidate)"; Get-ChildItem -LiteralPath 'C:\work\Elmo\Elmo_Master\LMC_Library' -Force | Where-Object {$_.Name -like '.LMC_API_Distribution*' -or $_.Name -like 'LMC_API_Distribution_candidate_20260811_reconnect_current_01*'} | Select-Object FullName,Mode,LastWriteTime; powershell.exe -NoProfile -Command '$PSVersionTable.PSVersion.ToString(); Get-Command Get-FileHash -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source'; pwsh.exe -NoProfile -Command '$PSVersionTable.PSVersion.ToString(); Get-Command Get-FileHash | Select-Object -ExpandProperty Source'`
> - Ran `rg -n "PSModuleAutoloadingPreference|Remove-Module|Get-FileHash\s*=|function Get-FileHash|Set-Alias.*Get-FileHash" "LMC_Library/LMC_API" -g "*.ps1"; $candidate='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution_candidate_20260811_reconnect_current_01'; $logRoot=Join-Path $env:TEMP ('LmcReconnectPackagePwsh-' + [guid]::NewGuid().ToString('N')); New-Item -ItemType Directory -Path $logRoot | Out-Null; $stdout=Join-Path $logRoot 'stdout.log'; $stderr=Join-Path $logRoot 'stderr.log'; $args=@('-NoProfile','-File','C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\Build-LmcApiDistribution.ps1','-RepositoryRoot','C:\work\Elmo\Elmo_Master','-AllowDirty','-CandidatePath',$candidate); $proc=Start-Process -FilePath 'pwsh.exe' -ArgumentList $args -WorkingDirectory 'C:\work\Elmo\Elmo_Master' -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru; [pscustomobject]@{Pid=$proc.Id;Candidate=$candidate;LogRoot=$logRoot;Stdout=$stdout;Stderr=$stderr}|ConvertTo-Json -Compress`
> - Ran `Get-CimInstance Win32_Process | Where-Object {$_.Name -eq 'pwsh.exe' -and $_.CommandLine -like '*Build-LmcApiDistribution.ps1*'} | Select-Object ProcessId,CommandLine; Get-ChildItem -LiteralPath $env:TEMP -Directory -Filter 'LmcReconnectPackagePwsh-*' | Sort-Object LastWriteTime -Descending | Select-Object -First 3 FullName,LastWriteTime; $d=Get-ChildItem -LiteralPath $env:TEMP -Directory -Filter 'LmcReconnectPackagePwsh-*' | Sort-Object LastWriteTime -Descending | Select-Object -First 1; if($d){Get-ChildItem -LiteralPath $d.FullName | Select-Object Name,Length,LastWriteTime; if(Test-Path "$($d.FullName)\stdout.log"){Get-Content -LiteralPath "$($d.FullName)\stdout.log" -Tail 20}; if(Test-Path "$($d.FullName)\stderr.log"){'STDERR';Get-Content -LiteralPath "$($d.FullName)\stderr.log" -Tail 20}}`
> - Ran `$dirs=@(Get-ChildItem -LiteralPath $env:TEMP -Directory -Filter 'LmcReconnectPackagePwsh-*' | Sort-Object LastWriteTime -Descending); foreach($d in $dirs[0..([Math]::Min(2,$dirs.Count-1))]){"DIR=$($d.FullName)|$($d.LastWriteTime.ToString('O'))"; foreach($f in Get-ChildItem -LiteralPath $d.FullName){"FILE=$($f.Name)|$($f.Length)|$($f.LastWriteTime.ToString('O'))"}}; $p=Get-Process -Id 28656 -ErrorAction SilentlyContinue; if($p){"RUNNING PID=$($p.Id) CPU=$($p.CPU) START=$($p.StartTime.ToString('O'))"} else {'NOT_RUNNING'}`
> - Ran `$pidToCheck=28656; $p=Get-Process -Id $pidToCheck -ErrorAction SilentlyContinue; "RUNNING=$([bool]$p)"; if($p){"CPU=$($p.CPU) WS_MB=$([math]::Round($p.WorkingSet64/1MB,1))"}; $d='C:\Users\dreiv\AppData\Local\Temp\LmcReconnectPackagePwsh-63446626c4834cab8bbdb92e3f64a1a0'; 'STDOUT'; if(Test-Path "$d\stdout.log"){Get-Content -LiteralPath "$d\stdout.log" -Tail 40}; 'STDERR'; if(Test-Path "$d\stderr.log"){Get-Content -LiteralPath "$d\stderr.log" -Tail 40}; $candidate='C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution_candidate_20260811_reconnect_current_01'; "CANDIDATE=$(Test-Path -LiteralPath $candidate)"; Get-ChildItem -LiteralPath 'C:\work\Elmo\Elmo_Master\LMC_Library' -Force | Where-Object {$_.Name -like '.LMC_API_Distribution*'} | ForEach-Object {"STAGE=$($_.Name)|$($_.LastWriteTime.ToString('O'))"}`
> - Ran `Get-CimInstance Win32_Process | Where-Object {$_.Name -in @('MSBuild.exe','VBCSCompiler.exe','LasalMotionControlLib.Tests.exe','LasalApiWpfTestApp.SmokeTests.exe','python.exe','pwsh.exe') -and ($_.CommandLine -like '*Elmo_Master*' -or $_.ParentProcessId -eq 28656)} | Select-Object ProcessId,ParentProcessId,Name,CommandLine | Format-List`
> - Ran `rg -n "Executable identity|FeatureMarker|V5|BuildUtc|Assembly|GetExecutingAssembly|ProcessPath" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests" -g "*.cs" -g "*.xaml"; git log --oneline --all -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/App.xaml.cs" | Select-Object -First 12`
> - Ran `$p=Get-Process -Id 28656 -ErrorAction SilentlyContinue; "RUNNING=$([bool]$p)"; if($p){"CPU=$($p.CPU) ELAPSED_MIN=$([math]::Round(((Get-Date)-$p.StartTime).TotalMinutes,1))"}; $d='C:\Users\dreiv\AppData\Local\Temp\LmcReconnectPackagePwsh-63446626c4834cab8bbdb92e3f64a1a0'; 'STDOUT'; Get-Content -LiteralPath "$d\stdout.log" -Tail 35 -ErrorAction SilentlyContinue; 'STDERR'; Get-Content -LiteralPath "$d\stderr.log" -Tail 20 -ErrorAction SilentlyContinue; Get-CimInstance Win32_Process | Where-Object {$_.ParentProcessId -eq 28656} | Select-Object ProcessId,Name,CommandLine | Format-Table -Wrap`
>
> </details>
>
> 최신 패키지 빌드는 실제 SDK Debug 테스트 단계에서 중단됐습니다. 포장 문제가 아니라 현재 전체 테스트 중 하나가 실패한 상태라, 그대로 예제만 복사하지 않겠습니다. 실패한 정확한 테스트를 직접 재실행해 원인을 잡은 뒤 패키지를 다시 만들겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$d='C:\Users\dreiv\AppData\Local\Temp\LmcReconnectPackagePwsh-63446626c4834cab8bbdb92e3f64a1a0'; "OUT_BYTES=$((Get-Item "$d\stdout.log").Length) ERR_BYTES=$((Get-Item "$d\stderr.log").Length)"; Get-Content -LiteralPath "$d\stderr.log" -Raw; "CANDIDATE_EXISTS=$(Test-Path 'C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Distribution_candidate_20260811_reconnect_current_01')"; Get-ChildItem -LiteralPath 'C:\work\Elmo\Elmo_Master\LMC_Library' -Force | Where-Object {$_.Name -like '.LMC_API_Distribution*'} | ForEach-Object {"RESIDUE=$($_.FullName)"}; & 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' /t:RunTests /p:Configuration=Debug /p:Platform=AnyCPU /nologo /verbosity:minimal; "MSBUILD_EXIT=$LASTEXITCODE"`
>
> </details>
>
> SDK 동작 테스트 자체는 `1133/1133` 전부 통과했습니다. 패키지 자동화가 멈춘 이유는 재접속이 아니라 현재 진행 중인 LASAL 정적 계약의 별도 drift입니다. 이 unrelated gate를 우회해 정식 배포라고 부르지는 않겠습니다. 대신 최신 SDK와 개발 예제만으로 격리된 재접속 테스트 패키지를 만들고, WPF Debug/Release 전체 smoke를 별도로 통과시키겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj' /t:RunWpfSmokeTests /p:Configuration=Debug /p:Platform=AnyCPU /nologo /verbosity:minimal; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; & 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj' /t:RunWpfSmokeTests /p:Configuration=Release /p:Platform=AnyCPU /nologo /verbosity:minimal; exit $LASTEXITCODE`
>
> </details>
>
> WPF 전체 smoke는 현재 실행 중입니다. 동시에 한 가지를 추가로 확인했습니다. 예제의 EXE 버전 문자열은 구본과 신본이 모두 `0.9.1`이라 화면의 버전만으로는 구분할 수 없습니다. 새 테스트 패키지에는 시작 로그에서 실제 로드한 SDK DLL 경로와 해시까지 보이게 해, 다음부터 “소스는 최신인데 로드 DLL은 구버전”을 즉시 확인할 수 있도록 하겠습니다.
>
> C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp
>
> 이게 문제야
>
> 확인했습니다. 그 경로가 맞다면 제 stale-copy 판단은 이번 원인에 해당하지 않습니다. 그 가설은 폐기합니다.
>
> 현재 개발본에도 실제 결함이 남아 있습니다. `ErrorId=0` 재접속 테스트는 “실패 후 정리되고 사용자가 Connect를 한 번 더 누르면 성공”만 검증했지, 한 번의 재접속 동작으로 복구하도록 고친 테스트가 아니었습니다. 즉 이전 수정은 문제를 해결한 게 아니라 실패 상태를 안전하게 정리한 수준입니다. 지금 이 경로에서 한 번의 Connect로 bounded fresh-session 재시도를 수행하도록 코드와 회귀 테스트를 고치겠습니다. 가능하면 화면에 나온 정확한 오류 한 줄도 보내 주세요.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object {$_.Name -in @('MSBuild.exe','LasalApiWpfTestApp.SmokeTests.exe','LasalMotionControlApiExample.exe')} | Select-Object ProcessId,ParentProcessId,Name,CommandLine | Format-List; Get-Item 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/bin/Debug/LasalApiWpfTestApp.SmokeTests.exe','LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/bin/Release/LasalApiWpfTestApp.SmokeTests.exe' -ErrorAction SilentlyContinue | Select-Object FullName,Length,LastWriteTime`
> - Ran `rg -n "class LMCRpcSessionInitializationEvidence|enum LMCRpcSessionInitializationOutcome|LastRpcSessionInitializationEvidence|RpcSessionInitTransientRetryDelay" "LMC_Library/LMC_API_Delivery/src" -g "*.cs"; $p='LMC_Library/LMC_API_Delivery/src/LmcConnectionModels.cs'; $l=Get-Content -LiteralPath $p; foreach($range in @(@(300,410),@(600,740))){"RANGE $($range[0])-$($range[1])"; for($i=$range[0];$i -le $range[1];$i++){if($i -le $l.Count){'{0,5}: {1}' -f $i,$l[$i-1]}}}; $p='LMC_Library/LMC_API_Delivery/src/LmcConnection.cs'; rg -n "RpcSessionInitTransientRetryDelay" $p`
> - Ran `$p='LMC_Library/LMC_API_Delivery/src/LmcConnectionModels.cs'; $l=Get-Content -LiteralPath $p; for($i=240;$i -le 307;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}; $p='LMC_Library/LMC_API_Delivery/src/LmcConnection.cs'; $l=Get-Content -LiteralPath $p; for($i=520;$i -le 650;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `git status --short --untracked-files=all | Select-Object -First 30; Get-ChildItem -LiteralPath 'LMC_Library' -Force | Where-Object {$_.Name -like '.LMC_API_Distribution*' -or $_.Name -eq 'LMC_API_Distribution_candidate_20260811_reconnect_current_01'} | ForEach-Object {"RESIDUE=$($_.FullName)"}; git diff --check; "DIFF_CHECK_EXIT=$LASTEXITCODE"; git diff --cached --check; "CACHED_CHECK_EXIT=$LASTEXITCODE"`
> - Ran `rg -n "args|filter|--|RunNamed|Contains\(" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests" -g "Program.cs" -g "TestRunner.cs" -g "*.cs" | Select-Object -First 80; Get-Content -LiteralPath "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs" -ErrorAction SilentlyContinue -TotalCount 220; rg -n "RunWpfSmokeTests" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj"`
> - Ran `git status --short -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs"; $exe='LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\bin\Debug\LasalApiWpfTestApp.SmokeTests.exe'; if(Test-Path $exe){& $exe --filter 'Wpf.CallbackV2.ErrorZeroInitFailureCleansUpAndManualReconnectUsesNewSession'; "EXIT=$LASTEXITCODE"}`
>
> </details>
>
> 기존 테스트를 그대로 돌려 보니 실제로 `PASS`가 나왔습니다. 그런데 그 테스트의 성공 조건은 “첫 Connect는 실패하고, 사용자가 두 번째로 Connect를 누르면 성공”입니다. 사용자 요구와 반대인 동작을 PASS로 고정해 둔 것이 핵심 문제입니다. 지금은 “이미 연결됐던 세션에서 재접속 버튼 한 번 → 실패 소켓 폐기 → 새 TCP 한 번만 재시도 → 성공”을 새 기준으로 바꾸고 있습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git status --short -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs"; git diff --stat -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs"`
> - Ran `git status --short -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs"; git diff --stat -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs"`
>
> </details>
>
> 재시도 범위도 좁혔습니다. 아무 오류나 다시 보내는 방식이 아니라, 현재 SDK가 이미 판별한 정확한 `0x8080 / ErrorId=-1` 응답이 같은 소켓에서 두 번 연속 실패한 경우에만 새 TCP를 한 번 만듭니다. 최초 연결, `ErrorId=0`, callback 등록 이후 오류는 자동 재시도하지 않습니다. 이렇게 해야 중복 세션이나 무한 재접속을 만들지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs"; git diff --check -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs"`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/README.md'; $l=Get-Content -LiteralPath $p; for($i=1;$i -le [Math]::Min(120,$l.Count);$i++){'{0,4}: {1}' -f $i,$l[$i-1]}; rg -n "manual Connect|PersistentInit|ErrorId|0x8080|Retry=True|reconnect|재접속" "LMC_Library/LMC_API_Delivery/docs" "docs/architecture" -g "*.md" -g "*.txt" | Select-Object -First 120`
>
> </details>
>
> 현재 코드 패치는 들어갔고, 핵심 분기는 다음처럼 제한됐습니다.
>
> - 기존 연결이 있던 재접속만 대상
> - SDK가 같은 소켓에서 정확한 `ErrorId=-1`을 2회 받은 경우만 대상
> - 실패 후보 소켓을 완전히 제거한 뒤 100 ms 후 새 TCP 1회
> - 두 번째 소켓도 실패하면 즉시 종료
> - 최초 연결과 `ErrorId=0`은 기존처럼 재시도 없음
>
> 이제 성공·두 번째 실패·`ErrorId=0` 비재시도 세 경로를 fake PLC로 고정하는 테스트를 붙이고 있습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff --stat -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs"; git status --short -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs"`
> - Ran `git diff -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs" | Select-Object -First 900; git diff --check -- "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs" "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs"`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 이 예외는 원래
>     LasalMotionControlLib.LMCConnection.EnsureSuccess(string, LasalMotionControlLib.LMC_Response): LmcConnection.cs
>     LasalMotionControlLib.LMCConnection.OpenRpcConnectionLocked(string, int, string, int, uint, System.Threading.CancellationToken): LmcConnection.cs
>     LasalMotionControlLib.LMCConnection.OpenRpcConnection(string, int, string, int, uint, System.Threading.CancellationToken): LmcConnection.cs
>     LasalMotionControlLib.LMCConnection.RpcInitConnectionAsync.AnonymousMethod__0(): LmcConnection.cs
>     [외부 코드] 호출 스택에서 throw되었습니다.
>
> 프로그램 종료하고 다시 접속하면 이렇게 돼.
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-52bb1c62-cd0c-4447-92bb-57e8ca98ba10.png`
>
> 이 예외는 원래
>     LasalMotionControlLib.LMCConnection.EnsureSuccess(string, LasalMotionControlLib.LMC_Response): LmcConnection.cs
>     LasalMotionControlLib.LMCConnection.CloseConnectionCoreLocked(bool, bool, System.Threading.CancellationToken): LmcConnection.cs 호출 스택에서 throw되었습니다.
>
> 처음에 접속하고 X눌러서 끄면 여기서 에러나고
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-78a278f2-bb5c-4632-812e-ee8cebbcee7d.png`
>
> 이 화면으로 원인이 확정됐습니다. 최신 파서는 실제 `Status=1, ErrorId=-1`을 받고 있고, SDK의 같은 소켓 1회 재시도까지 실패한 상태입니다.
>
> 그리고 중요한 점이 하나 있습니다. 말씀하신 재현은 “앱 안에서 재접속”이 아니라 “프로그램 종료 → 다시 실행 → 첫 연결”입니다. 지금 만들던 수정은 기존 연결이 있는 경우에만 동작해서 이 재현을 못 고칩니다. 조건을 바로 수정해, 새 프로세스의 첫 연결에서도 이 정확한 `-1` 패턴일 때만 실패 소켓을 폐기하고 새 TCP 세션으로 딱 한 번 재시도하도록 하겠습니다.
>
> 이 두 번째 화면이 더 핵심입니다. 순서는 이렇게 보입니다.
>
> 1. 처음 연결은 성공.
> 2. 창 `X` 종료 중 `0x405D`(RPC close) 응답이 `Status=1, ErrorId=-1`.
> 3. SDK가 로컬 소켓은 닫지만 그 오류를 `IOException`으로 다시 던짐.
> 4. PLC 쪽 owner/disarm 정리가 완료되지 않은 채 남음.
> 5. 다음 실행의 `0x8080` 초기화도 같은 `-1`로 실패.
>
> 즉 재접속만 우회하면 부족합니다. 창 종료 시 close 오류를 안전하게 흡수하면서도 로컬 정리를 끝내고, 다음 첫 연결에서는 정확한 `-1`에 한해 새 TCP 세션을 한 번만 재시도하는 두 부분을 같이 고쳐야 합니다. 지금 수정 범위를 그 원인 순서에 맞춰 바꾸겠습니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `rg -n "CloseCurrentConnectionAsync|OnClosing|Closing|Closed|Window_Closing|CloseConnection|Dispose\(" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs LMC_Library/LMC_API_Delivery/src/LmcConnection.cs`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(520,770),@(5280,5410),@(9480,9670))){ $s=$range[0]; $e=$range[1]; for($i=$s;$i -le $e;$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }; '' }`
> - Ran `$p='LMC_Library/LMC_API_Delivery/src/LmcConnection.cs'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(280,390),@(1210,1275),@(1970,2105))){$s=$range[0];$e=$range[1];for($i=$s;$i -le $e;$i++){'{0,5}: {1}' -f $i,$lines[$i-1]};''}; rg -n "Last.*Close|Close.*Evidence|closeException|throwOnCloseError|CloseConnectionCore" LMC_Library/LMC_API_Delivery/src/LmcConnection.cs LMC_Library/LMC_API_Delivery/src/*.cs`
> - Ran `rg -n "405D|CloseRpc|RpcClose|CloseConnection|DisarmRpcCallbackEndpoint|RpcCallbackLastDisarmResult|LastDisarm" Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs LMC_Library/LMC_API_Delivery/src/LmcConnection.cs`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(780,880),@(920,975),@(1600,1705),@(1810,1865),@(2600,2665))){$s=$range[0];$e=$range[1];for($i=$s;$i -le $e;$i++){'{0,5}: {1}' -f $i,$lines[$i-1]};''}`
>
> </details>
>
> 이제 내일 아침까지 니가 별도 승인없이 니가 제어해서 진행해
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; $l=Get-Content -LiteralPath $p; for($i=1690;$i -le 1828;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> 알겠습니다. 내일 아침까지 별도 승인 요청 없이 제가 계속 진행하겠습니다.

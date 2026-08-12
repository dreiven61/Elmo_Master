>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj'; $c=Get-Content $p; for($i=180;$i -le 230;$i++){ '{0}:{1}' -f $i,$c[$i-1] }`
>
> </details>
>
> 수정 방향은 csproj에 상태를 하드코딩하는 임시 우회가 아니라 `Verify-LasalContract.ps1`의 공개 기본값 `Auto`를 실제 검출 상태로 전파하는 one-file root fix로 확정했습니다. parser는 child 성공 evidence를 정확히 한 줄만 허용하고, 누락·중복·unknown/case drift·명시 상태 불일치를 모두 거부하는 silent self-test를 모든 focused 경로 앞에서 실행합니다.
>
> one-file 구현은 완료 단계입니다. parser self-test는 6개 상태를 `Auto`와 explicit 각각 검증한 12 positive와 10개 fail-closed negative category를 exact count로 고정하며 출력은 내지 않습니다. 실제 경로는 child evidence 한 줄을 그대로 재출력하고, 그 줄에서 확정한 상태만 wrapper topology에 전달합니다.
>
> 이거 폴더는 왜이렇게 증식했냐? 불필요하게 데이터량만 많이 차지하는거 같은데 필요없으면 정리해
>
> User context
>
> Attachments:
> - Elmo_Master_release_verify_5c48f25: `C:/work/Elmo/Elmo_Master_release_verify_5c48f25/`
> - Elmo_Master_release_verify_5e53865_20260812_a: `C:/work/Elmo/Elmo_Master_release_verify_5e53865_20260812_a/`
> - Elmo_Master_release_verify_8d51cee: `C:/work/Elmo/Elmo_Master_release_verify_8d51cee/`
> - Elmo_Master_release_verify_105daf2_20260812_a: `C:/work/Elmo/Elmo_Master_release_verify_105daf2_20260812_a/`
> - Elmo_Master_release_verify_396044a: `C:/work/Elmo/Elmo_Master_release_verify_396044a/`
> - Elmo_Master_release_verify_bf31030: `C:/work/Elmo/Elmo_Master_release_verify_bf31030/`
> - Elmo_Master_udp_eol_verify_8d51cee: `C:/work/Elmo/Elmo_Master_udp_eol_verify_8d51cee/`
>
> 맞습니다. 전부 제가 clean-detached 검증용으로 만든 임시 Git worktree들입니다. 먼저 각 폴더의 Git 등록 상태·tracked/ignored 변경·용량·실행 중 프로세스 참조를 확인하고, 보존할 사용자 변경이 없는 폐기된 검증 worktree만 Git 절차로 제거하겠습니다. 현재 main 저장소와 LASAL2가 연 메인 프로젝트는 건드리지 않습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$targets=@(
> 'C:\work\Elmo\Elmo_Master_release_verify_5c48f25',
> 'C:\work\Elmo\Elmo_Master_release_verify_5e53865_20260812_a',
> 'C:\work\Elmo\Elmo_Master_release_verify_8d51cee',
> 'C:\work\Elmo\Elmo_Master_release_verify_105daf2_20260812_a',
> 'C:\work\Elmo\Elmo_Master_release_verify_396044a',
> 'C:\work\Elmo\Elmo_Master_release_verify_bf31030',
> 'C:\work\Elmo\Elmo_Master_udp_eol_verify_8d51cee'
> );
> $registered=@{}; $lines=git worktree list --porcelain; $current=''; foreach($line in $lines){ if($line -like 'worktree *'){ $current=$line.Substring(9); $registered[[IO.Path]::GetFullPath($current)]=$true } };
> $processes=Get-CimInstance Win32_Process | Where-Object { $_.CommandLine };
> $results=foreach($target in $targets){
>   $full=[IO.Path]::GetFullPath($target); $exists=Test-Path -LiteralPath $full -PathType Container;
>   $status=@(); $ignored=@(); $head=''; $bytes=0L; $files=0; $refs=@();
>   if($exists){
>     $head=(git -C $full rev-parse --verify HEAD 2>$null);
>     $status=@(git -C $full status --short --untracked-files=all 2>$null);
>     $ignored=@(git -C $full status --short --ignored --untracked-files=all 2>$null | Where-Object { $_ -like '!!*' });
>     Get-ChildItem -LiteralPath $full -Recurse -Force -File -ErrorAction SilentlyContinue | ForEach-Object { $bytes += $_.Length; $files++ };
>     $refs=@($processes | Where-Object { $_.CommandLine.IndexOf($full,[StringComparison]::OrdinalIgnoreCase) -ge 0 } | ForEach-Object { "$($_.ProcessId):$($_.Name)" });
>   }
>   [pscustomobject]@{Path=$full;Exists=$exists;Registered=$registered.ContainsKey($full);Head=$head;TrackedOrUntrackedCount=$status.Count;Status=($status -join ' | ');IgnoredCount=$ignored.Count;Files=$files;Bytes=$bytes;ProcessRefs=($refs -join ',')}
> }; $results | ConvertTo-Json -Depth 3`
>
> </details>
>
> 그리고 아직도 새버전 API 테스트 프로그램을 접속하고 다시 닫고 연결하려고 하면 에러나 이거 아직 버그가 해결이 안됐어. 이거 해결해줘.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$targets=@('C:\work\Elmo\Elmo_Master_release_verify_5c48f25','C:\work\Elmo\Elmo_Master_release_verify_5e53865_20260812_a','C:\work\Elmo\Elmo_Master_release_verify_8d51cee','C:\work\Elmo\Elmo_Master_release_verify_105daf2_20260812_a','C:\work\Elmo\Elmo_Master_release_verify_396044a','C:\work\Elmo\Elmo_Master_release_verify_bf31030','C:\work\Elmo\Elmo_Master_udp_eol_verify_8d51cee'); $registered=@{}; $lines=git worktree list --porcelain; foreach($line in $lines){if($line -like 'worktree *'){$x=[IO.Path]::GetFullPath($line.Substring(9));$registered[$x]=$true}}; $processes=Get-CimInstance Win32_Process | Where-Object CommandLine; foreach($target in $targets){$full=[IO.Path]::GetFullPath($target);$exists=Test-Path -LiteralPath $full -PathType Container;$status=@();$ignored=@();$head='';$bytes=0L;$files=0;$refs=@();if($exists){$head=(git -C $full rev-parse --verify HEAD 2>$null);$status=@(git -C $full status --short --untracked-files=all 2>$null);$ignored=@(git -C $full status --short --ignored --untracked-files=all 2>$null|Where-Object{$_ -like '!!*'});Get-ChildItem -LiteralPath $full -Recurse -Force -File -ErrorAction SilentlyContinue|ForEach-Object{$bytes+=$_.Length;$files++};$refs=@($processes|Where-Object{$_.CommandLine.IndexOf($full,[StringComparison]::OrdinalIgnoreCase)-ge 0}|ForEach-Object{"$($_.ProcessId):$($_.Name)"})};[pscustomobject]@{Path=$full;Exists=$exists;Registered=$registered.ContainsKey($full);Head=$head;StatusCount=$status.Count;Status=($status -join ' | ');IgnoredCount=$ignored.Count;Files=$files;Bytes=$bytes;ProcessRefs=($refs -join ',')}} | Format-List`
>
> </details>
>
> 두 요청을 같이 처리하겠습니다.
>
> - 나열한 검증 worktree는 사용자 변경·프로세스 참조를 다시 정확히 확인한 뒤 불필요한 것만 제거합니다.
> - 재연결 오류는 별도 버그로 취급합니다. 최신 테스트 앱에서 `연결 → 닫기 → 재연결` 경로의 실제 객체 수명·소켓 정리·오류 로그를 코드와 테스트로 재현해 원인을 고치겠습니다. PLC 실동작 증거와 PC 테스트 결과는 구분해 보고하겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$targets=@('C:\work\Elmo\Elmo_Master_release_verify_5c48f25','C:\work\Elmo\Elmo_Master_release_verify_5e53865_20260812_a','C:\work\Elmo\Elmo_Master_release_verify_8d51cee','C:\work\Elmo\Elmo_Master_release_verify_105daf2_20260812_a','C:\work\Elmo\Elmo_Master_release_verify_396044a','C:\work\Elmo\Elmo_Master_release_verify_bf31030','C:\work\Elmo\Elmo_Master_udp_eol_verify_8d51cee'); $registered=@{}; foreach($line in (git worktree list --porcelain)){if($line -like 'worktree *'){$x=[IO.Path]::GetFullPath($line.Substring(9));$registered[$x]=$true}}; $selfPid=$PID; $processes=Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -and $_.ProcessId -ne $selfPid }; $results=@(foreach($target in $targets){$full=[IO.Path]::GetFullPath($target);$exists=Test-Path -LiteralPath $full -PathType Container;$status=@();$ignored=@();$head='';$bytes=0L;$files=0;$refs=@();if($exists){$head=(git -C $full rev-parse --verify HEAD 2>$null);$status=@(git -C $full status --short --untracked-files=all 2>$null);$ignored=@(git -C $full status --short --ignored --untracked-files=all 2>$null|Where-Object{$_ -like '!!*'});Get-ChildItem -LiteralPath $full -Recurse -Force -File -ErrorAction SilentlyContinue|ForEach-Object{$bytes+=$_.Length;$files++};$refs=@($processes|Where-Object{$_.CommandLine.IndexOf($full,[StringComparison]::OrdinalIgnoreCase)-ge 0}|ForEach-Object{"$($_.ProcessId):$($_.Name)"})};[pscustomobject]@{Path=$full;Exists=$exists;Registered=$registered.ContainsKey($full);Head=$head;StatusCount=$status.Count;Status=($status -join ' | ');IgnoredCount=$ignored.Count;Files=$files;Bytes=$bytes;ProcessRefs=($refs -join ',')}}); $results | Format-List`
> - Ran `$udp='C:\work\Elmo\Elmo_Master_udp_eol_verify_8d51cee'; $rel='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; git -C $udp diff --exit-code 105daf2 -- $rel; $same=($LASTEXITCODE -eq 0); $sizes=@(837316373,836728304,811562132,811564844,811562132,837348293,811564833); [pscustomobject]@{UdpWorktreeMatchesCommitted105daf2=$same;TotalBytes=($sizes|Measure-Object -Sum).Sum;TotalGiB=[math]::Round((($sizes|Measure-Object -Sum).Sum/1GB),3);MainWorktree=(git rev-parse --show-toplevel);MainHead=(git rev-parse HEAD)} | Format-List`
>
> </details>
>
> 확인 결과 7개 모두 Git 검증용 worktree이고 외부 프로세스 참조가 0입니다. 6개는 tracked clean, 나머지 UDP EOL worktree의 유일한 수정도 이미 `105daf2`에 커밋된 내용과 정확히 같습니다. 합계는 약 5.36 GiB라 전부 불필요하며, 지금 Git worktree 절차로 제거합니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `$targets=@('C:\work\Elmo\Elmo_Master_release_verify_5c48f25','C:\work\Elmo\Elmo_Master_release_verify_5e53865_20260812_a','C:\work\Elmo\Elmo_Master_release_verify_8d51cee','C:\work\Elmo\Elmo_Master_release_verify_105daf2_20260812_a','C:\work\Elmo\Elmo_Master_release_verify_396044a','C:\work\Elmo\Elmo_Master_release_verify_bf31030','C:\work\Elmo\Elmo_Master_udp_eol_verify_8d51cee'); $scope=[IO.Path]::GetFullPath('C:\work\Elmo')+[IO.Path]::DirectorySeparatorChar; $main=[IO.Path]::GetFullPath((git rev-parse --show-toplevel)); $registered=@{}; foreach($line in (git worktree list --porcelain)){if($line -like 'worktree *'){$registered[[IO.Path]::GetFullPath($line.Substring(9))]=$true}}; $selfPid=$PID; $processes=Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -and $_.ProcessId -ne $selfPid }; foreach($target in $targets){$full=[IO.Path]::GetFullPath((Get-Item -LiteralPath $target -Force).FullName); if(-not $full.StartsWith($scope,[StringComparison]::OrdinalIgnoreCase)){throw "Target escaped cleanup scope: $full"}; if($full -eq $main){throw "Refusing to remove main worktree: $full"}; if(-not $registered.ContainsKey($full)){throw "Target is not a registered worktree: $full"}; $refs=@($processes|Where-Object{$_.CommandLine.IndexOf($full,[StringComparison]::OrdinalIgnoreCase)-ge 0}); if($refs.Count -ne 0){throw "Target has active process references: $full => $($refs.ProcessId -join ',')"} }; foreach($target in $targets){ & git worktree remove --force -- $target; if($LASTEXITCODE -ne 0){throw "git worktree remove failed for $target"}; "REMOVED|$target" }; git worktree prune --expire now; if($LASTEXITCODE -ne 0){throw 'git worktree prune failed'}; foreach($target in $targets){"EXISTS|$([bool](Test-Path -LiteralPath $target))|$target"}; 'REGISTERED_REMAINING'; git worktree list --porcelain`
>
> </details>
>
> 정리 완료했습니다. 나열한 7개 검증 worktree를 모두 Git 등록과 함께 제거했고, 경로도 전부 없어졌습니다. 약 5.36 GiB를 회수했으며 현재 등록된 worktree는 메인 `C:\work\Elmo\Elmo_Master` 하나뿐입니다. 사용자 `Classes.lcb`, untracked 자료, 실행 중인 LASAL 메인 프로젝트는 건드리지 않았습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "Connect|Disconnect|Close|Dispose|Reconnect|OpenAsync|CloseAsync|IsConnected|TcpClient|LmcClient" LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API_Delivery/src -g '*.cs' | Select-Object -First 260`
> - Ran `rg -n "LMCConnection|ConnectAsync|Connect\(|DisconnectAsync|Disconnect\(|Close\(|Dispose\(" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp -g '*.cs' | Select-Object -First 240; rg -n "class LMCConnection|public .*Connect|public .*Disconnect|void Dispose|Close" LMC_Library/LMC_API_Delivery/src/LmcConnection*.cs LMC_Library/LMC_API_Delivery/src -g '*.cs' | Select-Object -First 240`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $c=Get-Content $p; foreach($range in @(@(520,760),@(5480,5605),@(1200,1295))){$s=$range[0];$e=$range[1];"---$s-$e---";for($i=$s;$i -le $e;$i++){ '{0}:{1}' -f $i,$c[$i-1] }}`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; $c=Get-Content $p; for($i=754;$i -le 900;$i++){ '{0}:{1}' -f $i,$c[$i-1] }; rg -n "CloseCurrentConnectionAsync" $p`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 지정한 검증 폴더 7개는 전부 정리했고 약 5.36 GiB를 회수했습니다. 지금은 중복 실행 없이 실제 프로그램 한 개로 `연결 → 명시적 닫기 → 재연결`을 재현해, 닫기 오류인지 PLC 세션 잔류인지 소켓/콜백 해제 문제인지 정확히 갈라내고 있습니다. 원인이 확인되면 해당 경로만 수정하고 재연결 회귀 테스트까지 붙이겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git status --short; rg -n "CloseCurrentConnectionAsync|RpcInitConnectionAsync|Persistent|fresh|retry|Reconnect|reconnect" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs LMC_Library/LMC_API_Delivery/src/LmcConnection.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.Tests -g "*.cs"`
> - Ran `$p='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; Get-Content -LiteralPath $p | Select-Object -Skip 5290 -First 170; rg --files LMC_Library/LasalApiWpfTestApp | rg "Tests|\.csproj$"; rg -n "CloseCurrentConnectionAsync|ReconnectFreshSessionRetry|persistent.*-1|fresh TCP|Close Connection" LMC_Library -g "*Test*.cs" -g "*.cs"`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$t='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs'; Get-Content -LiteralPath $t | Select-Object -Skip 560 -First 430; $s='LMC_Library/LMC_API_Delivery/src/LmcConnection.cs'; Get-Content -LiteralPath $s | Select-Object -Skip 1900 -First 230; Get-Content -LiteralPath $s | Select-Object -Skip 1420 -First 140`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "CloseShortFailureStep|LastCloseException|local transport was closed|CloseConnectedWindow|ButtonCloseConnection|explicit close|Close failed|Close completed|Connection cleanup warning" LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs; rg -n "RpcSessionInitTransientRetryDelayMilliseconds|ShouldRetryRpcSessionInit|CloseConnectionAsync|EnsureCompleteLocalConnectionCleanupAsync|HasCompleteLocalConnectionCleanup" LMC_Library/LMC_API_Delivery/src/LmcConnection.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs`
> - Ran `$t='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs'; Get-Content -LiteralPath $t | Select-Object -Skip 120 -First 145; Get-Content -LiteralPath $t | Select-Object -Skip 6425 -First 90; $m='LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs'; Get-Content -LiteralPath $m | Select-Object -Skip 9240 -First 120; Get-Content -LiteralPath $m | Select-Object -Skip 9680 -First 95`
>
> </details>
>
> 재현에서 중요한 사실이 확인됐습니다. 이번에는 닫기까지 가기도 전에 첫 연결부터 PLC가 `0x8080` 초기화를 4회(동일 TCP 2회 + 새 TCP 2회) 모두 `Status=1 / ErrorId=-1`로 거부했습니다. 즉 현재 PC 앱이 소켓을 못 닫아서 생긴 단순 오류가 아니라, PLC 쪽 세션이 이전 연결을 점유한 채 풀리지 않는 상태가 실제 원인 후보입니다. 이제 PLC의 `0x8080` 세션 등록과 `0x405D` 닫기 처리에서 점유 상태가 어떻게 해제되는지 추적하겠습니다.
>
> <details><summary>Ran 10 commands</summary>
>
> - Ran `rg -n "8080|405D|RpcSession|RPC session|SessionInit|CloseConnection|Close Connection|ClientDisconnect|disconnect|ErrorId.*-1|session.*owner|owner.*session" Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include LMC_Library/LMC_API_Delivery -g "*.st" -g "*.h" -g "*.cs" -g "*.ps1" -g "*.txt"`
> - Ran `$files=@('docs/architecture/SIGMATEK_LASAL_coding_rules.md','docs/architecture/SIGMATEK_LASAL_programming_method_study.md','docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md'); foreach($f in $files){ Write-Output "FILE=$f"; (Get-Content -LiteralPath $f).Count; rg -n "TCP|socket|disconnect|session|atomic|method|implementation|ASCII|CodeGenerator|external editor|검증|연결|세션|해제" $f }`
> - Ran `Get-Content docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md | Select-Object -Skip 350 -First 40; Get-Content docs/architecture/SIGMATEK_LASAL_coding_rules.md | Select-Object -Skip 115 -First 25; rg -n "function TCPMotionInterface::HandleRpcLifecycleCommands|HandleRpcLifecycleCommands|RpcSession|SessionEpoch|ConnSocket|ActiveSocket|SocketInfo" Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`
> - Ran `$f='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; Get-Content -LiteralPath $f | Select-Object -Skip 620 -First 275; Get-Content -LiteralPath $f | Select-Object -Skip 1625 -First 255`
> - Ran `$f='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; rg -n "FUNCTION TCPMotionInterface::DisarmRpcCallbackEndpoint|DisarmRpcCallbackEndpoint|LastTakeoverResult|LastOwnerDisconnectRequestRet|RpcInitialized|RpcSocket" $f; Get-Content -LiteralPath $f | Select-Object -Skip 2570 -First 125; Get-Content -LiteralPath LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt | Select-Object -Skip 1600 -First 85`
> - Ran `rg -n "FUNCTION .*DisarmEndpoint|DisarmEndpoint|ArmEndpoint|ExpectedSessionEpoch|ExpectedCookie" Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class -g "*.st"; $f='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st'; if(Test-Path $f){ Get-Content -LiteralPath $f | Select-Object -Skip 250 -First 260 }`
> - Ran `$f='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; Get-Content -LiteralPath $f | Select-Object -Skip 900 -First 275; rg -n "LastTakeoverResult|RpcCallbackLastDisarmResult|ConnectedClients|LastCandidate|LastOwnerDisconnect" LMC_Library/LasalApiWpfTestApp LMC_Library/LMC_API_Delivery Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class -g "*.cs" -g "*.st" -g "*.ps1"`
> - Ran `rg -n "RpcSessionInit\(|0x8080|RPC_INIT|RpcSessionInit" LMC_Library/LMC_API_Delivery/src -g "*.cs"; rg -n "RpcCallbackLastDisarmResult|RpcCallbackRegistered|RpcCallbackProtocolVersion|RpcCallbackSessionEpoch|ActiveEndpoint" docs test LMC_Library -g "*.md" -g "*.txt" -g "*.ps1" | Select-Object -First 120`
> - Ran `rg -n -i "rpc.*session.*init|8080" LMC_Library/LMC_API_Delivery/src -g "*.cs"; rg -n "RpcCallbackLastDisarmResult" docs test LMC_Library -g "*.md" -g "*.txt" -g "*.ps1" | Select-Object -First 80`
> - Ran `Get-Content LMC_Library/LasalApiWpfTestApp/API_MAPPING.md | Select-Object -Skip 70 -First 55; Get-Content LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md | Select-Object -Skip 690 -First 235`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "persistent callback disarm|disarm.*-8|disarm.*-9|force-clear|force clear|owner loss|owner-loss|stale.*endpoint|fence mismatch|DisarmEndpoint" docs LMC_Library/LMC_API_Delivery/docs LMC_Library/LasalApiWpfTestApp -g "*.md" -g "*.txt" | Select-Object -First 240`
> - Ran `Get-Content LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md | Select-Object -Skip 270 -First 75; Get-Content LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md | Select-Object -Skip 1360 -First 52; Get-Content LMC_Library/LasalApiWpfTestApp/README.md | Select-Object -Skip 45 -First 85`
> - Ran `$f='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st'; rg -n "ActiveEndpoint\.Armed\s*:=|_memset\(dest\s*:=\s*#ActiveEndpoint|ClearPendingFrames\(|Depth\s*:=|FUNCTION .*ServiceTransmitQueue|FUNCTION .*ErrorCallback|FUNCTION .*EnsureSocketReady" $f; Get-Content -LiteralPath $f | Select-Object -Skip 480 -First 290`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$f='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; rg -n "negative disarm|DisarmEndpoint result|disarm.*preserv|FenceMatches|DisarmEndpoint\(|RpcInitialized.*FALSE|owner loss|force-clear|force clear|stale fence|OwnerLoss" $f | Select-Object -First 240`
> - Ran `$f='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; Get-Content -LiteralPath $f | Select-Object -Skip 2170 -First 535; Get-Content -LiteralPath $f | Select-Object -Skip 8640 -First 250; Get-Content -LiteralPath $f | Select-Object -Skip 9280 -First 275`
>
> </details>
>
> PC 쪽 재현 프로그램은 정상 종료했고 소켓/포트 잔류는 0입니다. 소스상 무한 재접속 실패도 확정됐습니다: 이전 callback 해제 결과가 `-8`(세션/쿠키 불일치) 또는 `-9`(sender 연결 불가)이면 PLC가 상태를 보존하고, 이후 모든 `0x8080`이 같은 `-1`만 반복합니다. 단순 재시도 시간을 늘려서는 해결되지 않으므로, 정상 socket 소유권 상실이 확정된 경우에만 오래된 callback을 안전하게 폐기하는 PLC 경계를 설계 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n "339/339|339 WPF|339.*smoke|WPF.*339|SmokeTests.*339|TOTAL.*339|PASSED.*339" LMC_Library docs -g "*.ps1" -g "*.cs" -g "*.md" -g "*.txt" | Select-Object -First 100`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 정리 대상 7개 폴더는 모두 제거해 약 5.36 GiB를 회수했습니다. 재접속 오류는 PC 소켓 잔류가 아니라 PLC 쪽 콜백 세션의 `-8/-9` 해제 실패가 영구히 남는 경로까지 재현·추적했고, 지금은 “실제 TCP 소유권이 확실히 사라진 경우의 `-8`만 정리하고 `-9`는 계속 차단”하는 좁은 수정과 회귀 테스트를 적용 중입니다. 사용자 `Classes.lcb`와 미추적 파일은 건드리지 않겠습니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `git status --short; git diff --stat; git diff --check; git diff --cached --check`
> - Ran `git diff -- Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(760,890),@(2585,2665))){ for($i=$range[0];$i -le $range[1];$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] } }`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; $lines=Get-Content -LiteralPath $p; for($i=650;$i -le 765;$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }`
> - Ran `git status --short -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st; Get-Process powershell,pwsh -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime,CPU,Path | Sort-Object StartTime | Format-Table -AutoSize`
>
> </details>
>
> 실제 오류는 현재 프로그램에서 첫 연결부터 재현됐습니다. 새 TCP 객체를 만들어 네 번 재시도해도 PLC가 유효한 `0x8080 / ErrorId=-1` 응답을 반복했고, 그 시점에는 PC의 UDP 5000 포트조차 아직 열리지 않았습니다. 따라서 재시도 시간을 늘리거나 PC 연결 객체만 다시 만드는 식으로는 해결되지 않으며, PLC의 오래된 콜백 소유권을 안전한 두 상황에서만 폐기하도록 수정하고 있습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object { $_.ProcessId -in @(38480,18416) } | Select-Object ProcessId,ParentProcessId,CreationDate,CommandLine | Format-List`
> - Ran `Get-Item -LiteralPath 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1','Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st','Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st' | Select-Object Name,Length,LastWriteTime | Format-Table -AutoSize`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "DisarmEndpoint|DisarmRpcCallbackEndpoint|ConnSocketInfo|owner-loss|owner loss|force-clear|force clear|LastDisarm|FenceMatches" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content -LiteralPath $p; foreach($r in @(@(2220,2670),@(3480,3830),@(9260,9340),@(12770,13120))){ for($i=$r[0];$i -le $r[1];$i++){ '{0,5}: {1}' -f $i,$l[$i-1] } }`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $l=Get-Content -LiteralPath $p; for($i=43770;$i -le 43850;$i++){ '{0,6}: {1}' -f $i,$l[$i-1] }`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st; Get-Item -LiteralPath 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1' | Select-Object Length,LastWriteTime`
>
> </details>
>
> 수정은 안전 경계를 좁게 유지했습니다. 정상 요청이나 임의 클라이언트가 강제로 지우는 방식이 아니라, 기존 fence 해제가 정확히 `-8`이고 TCP 소유권 상실이 확정된 경우만 stale endpoint/FIFO를 폐기합니다. `CallbackSender` 연결 자체가 끊긴 `-9`, 다른 IP의 접속, 이미 교체된 옛 소켓의 늦은 disconnect는 계속 실패 차단됩니다. 현재 이 조건들을 깨뜨리는 변형 테스트까지 검증기에 고정하고 있습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff --stat -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st; git diff --check -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1 Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $l=Get-Content -LiteralPath $p; for($i=43812;$i -le 43828;$i++){ '{0,6}: {1}' -f $i,$l[$i-1] }; rg -n "ownerLossRetirement|owner-loss|owner loss|ExpectedSessionEpoch:=0|ExpectedSessionEpoch := 0|callback disarm call count|expected eight|expected six" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `git diff --unified=8 -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1`
> - Ran `Get-Item -LiteralPath 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1','LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1' | Select-Object Name,Length,LastWriteTime | Format-Table -AutoSize; git diff --numstat -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1`
>
> </details>
>
> PC 쪽에도 사용자가 말한 동작 그대로 회귀 테스트를 추가했습니다. 같은 창·고정 UDP 포트에서 `연결 → 명시적 닫기 → 재연결`과, PLC가 닫기에 `-1`을 반환하더라도 로컬 TCP/UDP가 정리된 뒤 같은 포트로 재연결되는 경우를 각각 검증했고, 현재 두 테스트를 5회 반복해 모두 통과했습니다. 실제 장애 원인인 PLC stale-session 수정 검증은 계속 진행 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --stat -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs; git diff --check -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs; git diff --unified=6 -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs`
>
> </details>
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `Get-Item -LiteralPath 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1','LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1' | Select Name,Length,LastWriteTime | Format-Table -AutoSize; rg -n "expected (six|eight) Candidate|callbackDisarmCallCount -ne" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1; rg -n "owner-loss|ownerLossRetirement|partial-zero|negative disarm|retirement" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 | Select-Object -Last 40`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content -LiteralPath $p; foreach($r in @(@(11310,11430),@(13030,13065),@(13350,13460))){ for($i=$r[0];$i -le $r[1];$i++){ '{0,5}: {1}' -f $i,$l[$i-1] } }`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content -LiteralPath $p; for($i=11415;$i -le 11470;$i++){ '{0,5}: {1}' -f $i,$l[$i-1] }; rg -n "Assert-OwnerLossRetirementSyntheticLifecycle" $p`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content -LiteralPath $p; for($i=210;$i -le 340;$i++){ '{0,5}: {1}' -f $i,$l[$i-1] }`
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-Item -LiteralPath 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1' | Select Length,LastWriteTime; rg -n "callbackDisarmCallCount -ne|expected (six|eight) Candidate" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1; git diff --numstat -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1`
> - Ran `Get-CimInstance Win32_Process | Where-Object { ($_.Name -in @('powershell.exe','pwsh.exe')) -and $_.CommandLine -match 'Verify-Lasal(UdpCallback)?Contract' } | Select-Object ProcessId,ParentProcessId,CreationDate,CommandLine | Format-List`
>
> </details>
>
> PLC 수정은 이제 구현과 변형 테스트까지 들어갔고, 별도 통합 검증기의 기존 “해제 호출 6회” 고정값도 새 확인 재호출 두 곳을 반영해 정확히 8회로 맞췄습니다. 장시간 양 호스트 테스트를 돌리기 전에 현재 4개 파일을 동결하고 독립 검토부터 통과시키는 중입니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg --files LMC_Library | rg "(CALLBACK_LISTENER_DESIGN|RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION|AUTOMATED_TESTS|GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK|API_MAPPING|README\.md$)"`

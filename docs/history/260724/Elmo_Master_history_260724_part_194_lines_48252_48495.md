> - Ran `rg -n -m 20 "_LMCPROF_POS\s*:\s*(STRUCT|ARRAY|DINT)|TYPE\s+_LMCPROF_POS|_LMCPROF_POS" "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_LMCRobotBase" -g '*.st'`
> - Ran `Get-Date
> Get-Item -LiteralPath 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st' | Select-Object LastWriteTime,Length
> rg -n "^\s*0x[0-9A-Fa-f]+:" 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
> "LINES=$((Get-Content -LiteralPath $p).Count)"
> rg -n "SendData|CurrentSock|RequestBuf|Sendbuf|_StdLib|Socket|SessionEpoch|Ingress|RequestQueue|_TCPIPServer|CyWork|RtWork|BackgroundWork" $p
> rg -n "ResponseSize :=" $p
> $bytes=[System.IO.File]::ReadAllBytes((Resolve-Path $p))
> $nonAscii=$bytes | Where-Object { $_ -gt 127 }
> "NON_ASCII_BYTES=$($nonAscii.Count)"`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
> $lines=Get-Content -LiteralPath $p
> foreach($range in @(@(245,460),@(455,740),@(735,920))){ $a=$range[0]-1; $b=$range[1]-1; for($i=$a;$i -le $b;$i++){ '{0,4}: {1}' -f ($i+1),$lines[$i] }; '---CHUNK---' }`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
> $lines=Get-Content -LiteralPath $p
> foreach($range in @(@(913,1195),@(1196,1410))){ $a=$range[0]-1; $b=$range[1]-1; for($i=$a;$i -le $b;$i++){ '{0,4}: {1}' -f ($i+1),$lines[$i] }; '---CHUNK---' }`
> - Ran `rg -n "_memcpy\(" "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class" -g '*.st' | Select-Object -First 80`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `& 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1' -RepositoryRoot 'C:/work/Elmo/Elmo_Master' -SourceOnly -ControlServiceCheckpoint Phase3GroupDormant`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n -C 5 "ControlServiceCheckpoint|Phase3GroupDormant|Phase3GroupRouted|serviceGroupCommandIds|serviceAdminCommandIds|ControlCommands\.HandleRequest|HandleRequest must" 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'
> rg -n -C 3 "RunLasalContract|RunLasalNetworkContract|ControlServiceCheckpoint" 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj'`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'
> $lines=Get-Content -LiteralPath $p
> foreach($range in @(@(20,180),@(530,770),@(800,900),@(1240,1310))){ $a=$range[0]-1; $b=$range[1]-1; for($i=$a;$i -le $b;$i++){ '{0,4}: {1}' -f ($i+1),$lines[$i] }; '---CHUNK---' }`
> - Ran `rg -n -C 4 "GroupMovePos|GroupKinematicReady|Variables:|pRequestFrame|pResponseFrame|_StdLib" 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1' | Select-Object -First 220`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Phase 3A 서비스 구현은 현재 소스 전용 정적 검증까지 통과했습니다. 지금은 독립 리뷰 결과를 받아, 서비스 본문이 잘못돼도 검증기가 통과할 수 있는 의미 검증 공백부터 막은 뒤 전체 회귀를 다시 돌리겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `rg -n "ControlServiceCheckpoint|Phase3GroupDormant|Phase3GroupRouted|groupHandlerBlock|adminHandlerBlock|groupMoveBlock|LMCControlCommandService" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $lines=Get-Content -LiteralPath $p; $ranges=@(@(580,760),@(1220,1330),@(1750,1925)); foreach($range in $ranges){ "--- $($range[0])-$($range[1]) ---"; $lines[($range[0]-1)..($range[1]-1)] }`
>
> </details>
>
> <details><summary>Ran 6 commands</summary>
>
> - Ran `git diff -- "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" "docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
> - Ran `rg -n "^FUNCTION (GLOBAL )?LMCControlCommandService::|^\s*0x(20D2|2047|2048|2049|204A|204B|2085|20A4|2045|2051|20E7|7D20|7D22):|RequestFrameSize|ResponseSize :=|GroupKinematicReady|_memcpy|LMCRobot\." "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st"`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(252,515),@(514,912),@(913,1191),@(1192,1415))){ "--- $($range[0])-$($range[1]) ---"; for($i=$range[0]; $i -le [Math]::Min($range[1],$lines.Count); $i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }}`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'; $lines=Get-Content -LiteralPath $p; for($i=734;$i -le 912;$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }`
> - Ran `rg -n "Phase 3|Phase 4|Phase 5|3A|3B|Registry|Axis|Admin|route|라우" "docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md"`
> - Ran `$p='docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md'; $lines=Get-Content -LiteralPath $p; for($i=240;$i -le 430;$i++){ '{0,4}: {1}' -f $i,$lines[$i-1] }`
>
> </details>
>
> 독립 검토와 별개로 확인한 결과, 현재 자동 검증은 “13개 ID가 존재한다”는 점은 잡지만 서비스 내부의 offset·응답 크기·상태 매핑까지 충분히 증명하지 못했습니다. 그래서 라우팅 전 단계에서 이 공백을 먼저 보강 중이며, 실제 TCP 경로는 여전히 기존 구현만 사용하므로 지금 PLC 동작은 이 신규 본문의 영향을 받지 않습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `git status --short; git diff --check; git diff --cached --check`
> - Ran `$p=Get-Process | Where-Object { $_.ProcessName -match 'Lasal' }; if($p){$p | Select-Object ProcessName,Id,MainWindowTitle | Format-Table -AutoSize}else{'LASAL_CLOSED'}; $log=Join-Path $env:TEMP 'Lasal2.log'; if(Test-Path $log){$hits=Select-String -LiteralPath $log -Pattern 'CInvalidArgException' -SimpleMatch; "CInvalidArgException_total=$($hits.Count)"}`
> - Ran `git diff -- "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj"`
> - Ran `$p='docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md'; $lines=Get-Content -LiteralPath $p; for($i=421;$i -le 470;$i++){ '{0,4}: {1}' -f $i,$lines[$i-1] }`
>
> </details>
>
> 독립 리뷰에서 실제 코드 결함 1건을 잡았습니다. 9바이트 명령 6곳이 LASAL의 `&` 단락 평가를 가정해, 8바이트 malformed frame에서도 offset 8을 읽을 수 있었습니다. 현재 caller의 고정 버퍼 때문에 물리적 위험은 낮지만 pointer/size ABI 위반이므로 지금 중첩 size gate로 수정합니다.
>
> <details><summary>Edited a file, ran a command</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+51 -29)
> - Ran `rg -n -U "RequestFrameSize = 9\)\s*&\s*\r?\n\s*\(\(pRequestFrame \+ 8\)\^\$USINT" "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st"; if($LASTEXITCODE -eq 1){'NO_UNGUARDED_9_BYTE_DEREF'}; rg -n "RequestFrameSize = 9|groupCommandInputValid :=|objectRegistryReady :=" "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st"`
>
> </details>
>
> <details><summary>Edited 2 files, ran 11 commands</summary>
>
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'; $raw=Get-Content -Raw -LiteralPath $p; "bytes=$([Text.Encoding]::UTF8.GetByteCount($raw)) lines=$((Get-Content -LiteralPath $p).Count) nonAscii=$(([regex]::Matches($raw,'[^\x00-\x7F]')).Count)"; foreach($bad in @('SendData','CurrentSock','RequestBuf','Sendbuf','_StdLib','Type0')){"$bad=$(([regex]::Matches($raw,[regex]::Escape($bad),[Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count)"}; $groups=[regex]::Match($raw,'(?s)FUNCTION LMCControlCommandService::HandleGroupCommands.*?END_FUNCTION').Value; $admin=[regex]::Match($raw,'(?s)FUNCTION LMCControlCommandService::HandleAdminCommands.*?END_FUNCTION').Value; "HandleGroupBytes=$([Text.Encoding]::UTF8.GetByteCount($groups)) HandleAdminBytes=$([Text.Encoding]::UTF8.GetByteCount($admin))"`
> - Ran `rg -n "pRequestFrame\s*\+|pRequestFrame\^" "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st"`
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+12 -7)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+18 -5)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -1)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+3 -1)
> - Ran `rg -n "FUNCTION TCPMotionInterface::MsgPaser|VAR$|udSize|Payload|sizeof\(Sendbuf\)|HandleGroupCommands\(\)|ControlCommands" "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st" | Select-Object -First 160`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; $lines=Get-Content -LiteralPath $p; for($i=940;$i -le 1150;$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; $lines=Get-Content -LiteralPath $p; for($i=80;$i -le 280;$i++){ '{0,4}: {1}' -f $i,$lines[$i-1] }`
> - Ran `rg -n "\bdRetcode\b|ControlResponse|ResponseSize" "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st" | Select-Object -First 80`
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'; $lines=Get-Content -LiteralPath $p; for($i=3045;$i -le 3210;$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }`
> - Ran `rg -n "Assert-ControlService|Service.*Semantic|service.*20D2|pointer|short-frame|RequestFrameSize = 9|ControlResponseSize|Phase3GroupDormant'" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" | Select-Object -First 200`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $lines=Get-Content -LiteralPath $p; for($i=2260;$i -le [Math]::Min(2500,$lines.Count);$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $lines=Get-Content -LiteralPath $p; for($i=2070;$i -le 2265;$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $lines=Get-Content -LiteralPath $p; for($i=1970;$i -le 2075;$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }`
>
> </details>
>
> 코드 쪽 범위 초과 가능성은 수정했습니다. 검증기도 이제 서비스의 13개 명령과 두 helper를 직접 대상으로 frame size/offset/outer status/native 호출을 검사하고 있으며, 향후 Phase 3B에서 legacy body를 제거해도 legacy 검사 때문에 거짓 실패하지 않도록 checkpoint별 소유권 검사를 분리하는 중입니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "Phase3GroupRouted.*argument|control-service route|ControlCommands.*HandleRequest|ControlResponseSize|ResponseCapacity:=sizeof|RequestFrameSize|exact.*argument|single SendData" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" | Select-Object -Last 100`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $lines=Get-Content -LiteralPath $p; for($i=820;$i -le 870;$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }`
> - Ran `& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' /t:RunPcTests /p:Configuration=Debug /m /v:minimal /nologo`
>
> </details>
>
> <details><summary>Edited a file, ran 6 commands</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $tokens=$null; $errors=$null; [void][System.Management.Automation.Language.Parser]::ParseFile((Resolve-Path $p),[ref]$tokens,[ref]$errors); if($errors.Count -eq 0){'POWERSHELL_PARSE_PASS'}else{$errors | ForEach-Object {$_.Message}; exit 1}`
> - Ran `powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" -RepositoryRoot "." -SourceOnly -ControlServiceCheckpoint Phase3GroupDormant`
> - Ran `powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" -RepositoryRoot "." -SourceOnly`
> - Ran `$script='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; foreach($checkpoint in @('Phase2Skeleton','Phase3GroupRouted')){ $output=& powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -RepositoryRoot '.' -SourceOnly -ControlServiceCheckpoint $checkpoint 2>&1; $code=$LASTEXITCODE; "CHECKPOINT=$checkpoint EXIT=$code"; $output | Select-Object -First 4; if($code -eq 0){exit 1} }`
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+8 -0)
> - Ran `rg -n "controlResponseSize|shared single|common fail|exact HandleRequest|RequestFrameSize:=.*Payload|pRequestFrame:=.*RequestBuf|ResponseCapacity:=.*Sendbuf" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $lines=Get-Content -LiteralPath $p; for($i=1280;$i -le 1400;$i++){ '{0,5}: {1}' -f $i,$lines[$i-1] }`
>
> </details>
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `rg -n "validateLegacy|Phase3GroupRouted.*legacy|if \(\$ControlServiceCheckpoint -ne 'Phase3GroupRouted'\)|LegacyGroup" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
> - Ran `rg -n 'validateLegacy|Phase3GroupRouted.*legacy|ControlServiceCheckpoint -ne.*Phase3GroupRouted|LegacyGroup' 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $lines=Get-Content -LiteralPath $p; foreach($range in @(@(1410,1470),@(1885,1950))){"---";for($i=$range[0];$i -le $range[1];$i++){'{0,5}: {1}' -f $i,$lines[$i-1]}}`
>
> </details>
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `Get-Item 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1' | Select-Object Length,LastWriteTime; Get-Date`
> - Ran `$ErrorActionPreference='Stop'; $script='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $tokens=$null; $errors=$null; [void][System.Management.Automation.Language.Parser]::ParseFile((Resolve-Path $script),[ref]$tokens,[ref]$errors); if($errors.Count -ne 0){$errors | ForEach-Object {$_.Message}; exit 1}; 'POWERSHELL_PARSE_PASS'; & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -RepositoryRoot '.' -SourceOnly; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; & 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' /t:RunLasalContract /p:Configuration=Debug /v:minimal /nologo; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; foreach($checkpoint in @('Phase2Skeleton','Phase3GroupRouted')){ $output=& powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -RepositoryRoot '.' -SourceOnly -ControlServiceCheckpoint $checkpoint 2>&1; $code=$LASTEXITCODE; "EXPECTED_REJECT checkpoint=$checkpoint exit=$code"; ($output | Select-Object -First 1); if($code -eq 0){exit 1} }`
> - Ran `$script='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $output=& powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -RepositoryRoot '.' -ControlServiceCheckpoint Phase3GroupDormant 2>&1; $code=$LASTEXITCODE; "FULL_STATIC_EXIT=$code"; $output | Select-Object -First 8; if($code -eq 0){exit 1}`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $lines=Get-Content -LiteralPath $p; for($i=380;$i -le 420;$i++){ '{0,4}: {1}' -f $i,$lines[$i-1] }`
>
> </details>
>
> 자동 검증은 통과했고 PC 테스트도 148/148입니다. 추가로 full-static 실패가 현재처럼 단순 “파일 없음”으로 끝나지 않도록, 재생성해야 할 LASAL network table과 조치를 명시하는 preflight 오류로 바꾸겠습니다.
>
> <details><summary>Edited a file, ran 3 commands</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -0)
> - Ran `$script='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $tokens=$null; $errors=$null; [void][System.Management.Automation.Language.Parser]::ParseFile((Resolve-Path $script),[ref]$tokens,[ref]$errors); if($errors.Count -ne 0){$errors | ForEach-Object {$_.Message}; exit 1}; 'POWERSHELL_PARSE_PASS'; & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -RepositoryRoot '.' -SourceOnly; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; $output=& powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -RepositoryRoot '.' -ControlServiceCheckpoint Phase3GroupDormant 2>&1; $code=$LASTEXITCODE; "FULL_STATIC_EXPECTED_BLOCK_EXIT=$code"; $output | Select-Object -First 2; if($code -eq 0){exit 1}`
> - Ran `& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' /t:RunLasalContract /p:Configuration=Debug /v:minimal /nologo; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; & 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.sln' /t:Build /p:Configuration=Debug /m /v:minimal /nologo`
> - Ran `git diff --check; $a=$LASTEXITCODE; git diff --cached --check; $b=$LASTEXITCODE; "DIFF_CHECK_EXIT=$a CACHED_DIFF_CHECK_EXIT=$b"; git diff --stat -- "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj" "docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"; $svc='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'; $raw=Get-Content -Raw -LiteralPath $svc; "SERVICE bytes=$([Text.Encoding]::UTF8.GetByteCount($raw)) lines=$((Get-Content $svc).Count) nonAscii=$(([regex]::Matches($raw,'[^\x00-\x7F]')).Count)"`
>
> </details>
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `$p='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn'; [xml]$x=Get-Content -Raw -LiteralPath $p; $obj=@($x.SelectNodes("//Object[@Name='LMCControlCommandService1' or @Class='LMCControlCommandService']")); $links=@($x.SelectNodes("//Connection[contains(@Source,'ControlCommands') or contains(@Source,'LMCControlCommandService1') or contains(@Destination,'LMCControlCommandService1')]")); "service_objects=$($obj.Count) service_links=$($links.Count) generated_table_exists=$(Test-Path 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/ONE_Comm_Network_Table.st')"`
> - Ran `$lasal=Get-Process | Where-Object {$_.ProcessName -match '^Lasal'}; if($lasal){$lasal | Select-Object ProcessName,Id}else{'LASAL_CLOSED'}; $log=Join-Path $env:TEMP 'Lasal2.log'; if(Test-Path $log){"CInvalidArgException_total=$(@(Select-String -LiteralPath $log -Pattern 'CInvalidArgException' -SimpleMatch).Count)"}`
> - Ran `rg -n "FUNCTION LMCControlCommandService::HandleGroupCommands|if RequestFrameSize = 9 then|FUNCTION LMCControlCommandService::HandleAdminCommands|FUNCTION LMCControlCommandService::MoveLinearAbsEx|FUNCTION LMCControlCommandService::GroupReadStatus" "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st"; rg -n "ControlServiceCheckpoint =|serviceFrameContracts|Phase3GroupRouted response bound|LASAL generated table for|Service 0x20E7 must establish" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"; rg -n "진행 상태\(2026-07-24\): 위 13개|실패한 generator|method-local|ControlServiceCheckpoint Phase3GroupDormant" "docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md"; rg -n "dormant service|ONE_Comm_Network_Table|Phase 3A body" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
> - Ran `rg -n "control-service route|invalid-only|expected exactly one shared send|complete request frame" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $l=Get-Content $p; for($i=1370;$i -le 1443;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `rg -n "controlService.*32768|service.*ByteCount|LMCControlCommandService.*bytes" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
>
> </details>
>
> 최종 재검토에서 검증기 false-pass 1건을 더 찾았습니다. 일부 ACK 명령은 총 크기와 outer status만 검사해 `+2/+4/+8/+12/+14` 필드가 틀려도 통과할 수 있었습니다. 해당 6개 명령과 `0x2045` 오류 경로의 바이트 위치·`-2/-3` 구분까지 고정하는 검사를 추가 중입니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $l=Get-Content $p; for($i=740;$i -le 825;$i++){'{0,5}: {1}' -f $i,$l[$i-1]}`
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $l=Get-Content $p; for($i=25;$i -le 120;$i++){'{0,4}: {1}' -f $i,$l[$i-1]}`
>
> </details>
>
> 검증기를 단순 “현재 코드 통과용”이 아니라 라우팅 전 안전 게이트로 만들기 위해 변이 테스트 수준으로 재검토하고 있습니다. 지금까지 handler 오배선, 호출보다 먼저 응답하는 순서 오류, Axis2/3 누락, `&`→`|` 변경, motion 인자 누락도 잡도록 보강했고, 실제 서비스 코드는 이 검토에서 새 wire 불일치는 나오지 않았습니다.
>
> 최종 독립 재검토까지 끝났고 추가 P1/P2는 없습니다. 이제 최신 파일 그대로 parser, Dormant 계약, 잘못된 checkpoint 거부, PC 148 tests, WPF build, diff/ASCII/금지 의존성 검사를 한 번에 다시 확정하겠습니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'; $script='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1'; $tokens=$null; $errors=$null; [void][System.Management.Automation.Language.Parser]::ParseFile((Resolve-Path $script),[ref]$tokens,[ref]$errors); if($errors.Count -ne 0){$errors | ForEach-Object {$_.Message}; exit 1}; 'POWERSHELL_PARSE_PASS'; & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -RepositoryRoot '.' -SourceOnly; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -RepositoryRoot '.' -SourceOnly -ControlServiceCheckpoint Phase3GroupDormant; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; & 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' /t:RunLasalContract /p:Configuration=Debug /v:minimal /nologo; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; foreach($checkpoint in @('Phase2Skeleton','Phase3GroupRouted')){ $out=& powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -RepositoryRoot '.' -SourceOnly -ControlServiceCheckpoint $checkpoint 2>&1; $code=$LASTEXITCODE; "EXPECTED_REJECT $checkpoint EXIT=$code :: $($out[0])"; if($code -eq 0){exit 1} }; $out=& powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $script -RepositoryRoot '.' -ControlServiceCheckpoint Phase3GroupDormant 2>&1; $code=$LASTEXITCODE; "EXPECTED_FULL_BLOCK EXIT=$code :: $($out[0])"; if($code -eq 0){exit 1}`
> - Ran `$msbuild='C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe'; $pcOut=& $msbuild 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' /t:RunPcTests /p:Configuration=Debug /m /v:minimal /nologo 2>&1; $pcCode=$LASTEXITCODE; if($pcCode -ne 0){$pcOut; exit $pcCode}; $pcOut | Where-Object {$_ -match 'TOTAL '}; & $msbuild 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.sln' /t:Build /p:Configuration=Debug /m /v:minimal /nologo; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; git diff --check; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; git diff --cached --check; if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}; $svc='Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'; $raw=Get-Content -Raw -LiteralPath $svc; $nonAscii=[regex]::Matches($raw,'[^\x00-\x7F]').Count; $forbidden=[regex]::Matches($raw,'(?i)(SendData|CurrentSock|RequestBuf|Sendbuf|_StdLib|_TCPIPServer|CyWork|RtWork)').Count; "SERVICE_STATIC nonAscii=$nonAscii forbiddenTransportRefs=$forbidden bytes=$([Text.Encoding]::UTF8.GetByteCount($raw))"; if($nonAscii -ne 0 -or $forbidden -ne 0){exit 1}; $lasal=@(Get-Process | Where-Object {$_.ProcessName -match '^Lasal'}); "LASAL_PROCESS_COUNT=$($lasal.Count)"; $log=Join-Path $env:TEMP 'Lasal2.log'; if(Test-Path $log){"CInvalidArgException_total=$(@(Select-String -LiteralPath $log -Pattern 'CInvalidArgException' -SimpleMatch).Count)"}`
> - Ran `rg -n "^FUNCTION LMCControlCommandService::HandleGroupCommands|if RequestFrameSize = 9 then|^FUNCTION LMCControlCommandService::HandleAdminCommands|^FUNCTION LMCControlCommandService::MoveLinearAbsEx|^FUNCTION LMCControlCommandService::GroupReadStatus" "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st"; rg -n "ControlServiceCheckpoint = 'Phase3GroupDormant'|LASAL generated table for|Phase3GroupRouted.*ownership|must establish the exact 1328|exact typed ACK|LockProfile Axis1|Admin detail" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"; rg -n "진행 상태\(2026-07-24\): 위 13개|method-local|실패한 generator|ControlServiceCheckpoint Phase3GroupDormant" "docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md"; rg -n "Group 11개와 Group-domain Admin|ONE_Comm_Network_Table|Phase 3A body" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"`
> - Ran `rg -n "HandleRequest group ownership|response scratch|invalid-response bound|expected exactly one shared send|exact four-axis connected-client|LockProfile argument" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1"`
> - Ran `git status --short; git diff --stat -- "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj" "LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1" "docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md"; git ls-files --error-unmatch "docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md" 2>$null; if($LASTEXITCODE -ne 0){'DESIGN_DOC_UNTRACKED'}; git ls-files --error-unmatch "Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st" 2>$null; if($LASTEXITCODE -ne 0){'SERVICE_SOURCE_UNTRACKED'}`
>
> </details>
>
> <details><summary>Plan</summary>
>
> Phase 3A dormant Group-domain service body, pointer-safety fix, direct semantic verifier, future routed transport gate, docs, and PC/WPF regression are complete. Production routing remains intentionally blocked until the user saves the no-task service object and 11 Object Network links and a successful LASAL rebuild regenerates the network table.
>
> - [x] 현재 worktree와 service/network/generated 상태 확인
> - [x] Phase 3 Group-domain 변수·method·frame 계약 확정
> - [x] network 전환 전 dormant service 구현과 정적 의미 검증 강화
> - [ ] 사용자 network 저장·LASAL Rebuild 후 full-static 및 13-ID 원자 route 전환
> - [ ] Phase 4 Axis/Registry/Admin 이동과 Phase 5 transport 정리·전체 회귀
> - [ ] LASAL build·PLC packet regression·성능 승인
>
> </details>
</details>

Phase 3A 구현을 완료했습니다. 현재 실제 TCP 경로는 기존 구현만 사용하므로 PLC 동작은 아직 바뀌지 않았습니다.

- Group 11개 + Admin `0x7D20`, `0x7D22`와 helper 2개 구현: [LMCControlCommandService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st:252)
- 짧은 malformed frame의 offset 8 범위 초과 가능성 6곳 수정
- 서비스 frame/offset/native 호출과 향후 Routed 경로까지 검증하도록 정적 계약 강화: [Verify-LasalContract.ps1](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1:11)
- 설계 및 현재 상태 문서 갱신: [OOP 설계](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md:359), [현재 상태](/C:/work/Elmo/Elmo_Master/docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md:16)

검증 결과:

- PC tests: `148/148 PASS`
- WPF Debug build: PASS
- Phase3 Dormant SourceOnly/MSBuild 계약: PASS
- Phase2 및 Routed checkpoint 오판 방지: 정상 거부
- ASCII/transport 의존성/diff check: PASS
- `CInvalidArgException`: 0
- 독립 변이 검토 후 추가 P1/P2 없음

다음은 사용자 LASAL 작업이 필요합니다.

1. `Comm_Network`에 task 없는 `LMCControlCommandService1` 배치
2. 다음 11개 연결
   - `TCPMotionInterface1.ControlCommands -> LMCControlCommandService1.ClassSvr`
   - `LMCControlCommandService1.LMCAxis1..9 -> _LMCAxis1..9.Control`
   - `LMCControlCommandService1.LMCRobot -> _LMCRobotBase1.Control`
3. 기존 `TCPMotionInterface1`의 axis/robot 연결은 아직 삭제하지 않음
4. 저장 후 Rebuild 성공 확인
5. LASAL 종료

실패한 이전 Rebuild가 `ONE_Comm_Network_Table.st`를 삭제했습니다. 이전 Git 파일을 복원하지 말고 성공한 Rebuild로 재생성해야 합니다. 작업 후 알려주면 full-static을 확인하고 13개 ID를 서비스로 원자 전환하는 Phase 3B를 진행하겠습니다.

커밋과 스테이징은 하지 않았습니다.

<details><summary>1 previous message</summary>

> System error
>
> You've hit your usage limit. Visit https://chatgpt.com/codex/settings/usage to purchase more credits or try again at Jul 29th, 2026 7:57 AM.
</details>

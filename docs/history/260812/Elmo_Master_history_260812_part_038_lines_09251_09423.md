>
> </details>
>
> 코드 변경은 동결됐고 현재 독립 검토가 진행 중입니다. 동시에 문서에는 “일반 RPC 요청에서 강제 해제 금지”, “확정된 소유권 상실의 `-8`만 내부적으로 정리”, “`-9`는 Network 문제로 계속 차단”이라는 경계를 반영하고 있습니다. PLC 다운로드 전이라 실제 장비 해결 완료로 과장하지 않고, 정적·자동 테스트와 실기 확인을 분리해 마무리하겠습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; $l=Get-Content -LiteralPath $p; for($i=11470;$i -le 11500;$i++){ '{0,5}: {1}' -f $i,$l[$i-1] }`
> - Ran `rg -n "296(?:/296| negative| UDP| distribution|\b)" LMC_Library/LMC_API LMC_Library/LMC_API_Delivery --glob '*.ps1' --glob '*.cs' --glob '*.md' --glob '*.txt'`
> - Ran `rg -n "expectedNegativeCount|negativeCount -ne|negative fixtures rejected" LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1`
> - Ran `rg -n "UdpCallback|EvidencePattern|EvidenceLine" LMC_Library/LMC_API/Test-LmcApiDistributionPipeline.ps1 | Select-Object -First 80`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --unified=3 -- LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1 | Select-String -Pattern '^-' -Context 1,1 | Select-Object -First 120`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `git status --short -- LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1; rg -n "UdpCallbackContract\\\.SelfTest|PASS LASAL.UdpCallbackContract.SelfTest" LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1 | Select-Object -First 8`
>
> </details>
>
> <details><summary>Ran a command</summary>
>
> - Ran `$p='LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1'; @('sender top-level bare token residue','sender top-level stray control residue','sender unexpected TYPE span','Gate C command hex literal split by whitespace','Gate C mask hex literal split by whitespace','Gate C typed lvalue suffix split by whitespace','Gate C address operator split by whitespace','Gate C malformed init disarms before validation','Gate C non-owner legacy request locks shape','owner-loss sentinel no longer requires all-zero triple','Gate C owner-loss accepts unavailable sender result','Gate C ordinary 0x8080 invokes owner-loss retirement') | ForEach-Object { $n=$_; $c=(Select-String -LiteralPath $p -SimpleMatch $n).Count; [pscustomobject]@{Name=$n;Count=$c} } | Format-Table -AutoSize; git diff --check -- $p 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'`
>
> </details>
>
> 독립 검토에서 기존 테스트를 줄여 숫자만 맞추려던 부분을 발견해 바로 되돌렸습니다. 기존 296개 회귀를 전부 보존하고 새 owner-loss 변형 9개를 추가해 최종 계약을 `305/305`로 올렸으며, 재검토는 P0/P1 없이 통과했습니다. 이제 Windows PowerShell 5.1과 PowerShell 7에서 장시간 self-test를 순차 실행합니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object { ($_.Name -in @('powershell.exe','pwsh.exe')) -and $_.CommandLine -match 'Verify-LasalUdpCallbackContract.ps1.+RunSelfTest' } | Select-Object ProcessId,ParentProcessId,CreationDate,Name,CommandLine | Format-List`
> - Ran `Get-Process -Id 27712 -ErrorAction SilentlyContinue | Select-Object Id,Responding,CPU,WorkingSet64,StartTime | Format-List`
>
> </details>
>
> PS5.1의 `305/305` self-test가 현재 정상적으로 계산 중이며 프로세스 응답과 CPU 진행을 확인했습니다. 이 검사는 약 4분, 이어지는 PS7 검사는 약 8분 걸리는 기존 장시간 구간입니다. 파일은 동결 상태로 유지하고 있습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-Process -Id 27712 -ErrorAction SilentlyContinue | Select-Object Id,Responding,CPU,WorkingSet64,StartTime | Format-List`
> - Ran `git diff --stat -- LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md LMC_Library/LasalApiWpfTestApp/API_MAPPING.md LMC_Library/LasalApiWpfTestApp/README.md; git diff --check -- LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md LMC_Library/LasalApiWpfTestApp/API_MAPPING.md LMC_Library/LasalApiWpfTestApp/README.md`
>
> </details>
>
> Windows PowerShell 5.1 검증이 방금 `305/305 PASS`로 끝났습니다(238.0초). 동결 해시도 그대로입니다. 이제 동일한 검증을 PowerShell 7에서 순차 실행하고 있습니다.
>
> <details><summary>Ran 5 commands</summary>
>
> - Ran `git diff --unified=4 -- LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md LMC_Library/LasalApiWpfTestApp/API_MAPPING.md LMC_Library/LasalApiWpfTestApp/README.md`
> - Ran `Get-CimInstance Win32_Process | Where-Object { ($_.Name -in @('powershell.exe','pwsh.exe')) -and $_.CommandLine -match 'Verify-LasalUdpCallbackContract.ps1.+RunSelfTest' } | Select-Object ProcessId,ParentProcessId,CreationDate,Name,CommandLine | Format-List; Get-Process pwsh -ErrorAction SilentlyContinue | Where-Object Id -ne $PID | Sort-Object StartTime -Descending | Select-Object -First 4 Id,CPU,Responding,StartTime | Format-Table -AutoSize`
> - Ran `$p='LMC_Library/LMC_API/README.md'; $l=Get-Content -LiteralPath $p; foreach($n in @(20,64,104,173)){ for($i=$n-3;$i -le $n+4;$i++){ '{0,4}: {1}' -f $i,$l[$i-1] }; '' }`
> - Ran `Get-Process -Id 5648 -ErrorAction SilentlyContinue | Select-Object Id,Responding,CPU,WorkingSet64,StartTime | Format-List`
> - Ran `$files=@('Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st','Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st','LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1','LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1','LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1'); Get-FileHash -Algorithm SHA256 -LiteralPath $files | Select-Object Path,Hash | Format-Table -AutoSize`
>
> </details>
>
> PS7 검증도 계속 정상적으로 CPU를 사용하며 진행 중이고 오류 출력은 없습니다. 현재 코드 해시는 검토 시점과 변함없습니다. 끝나면 짧은 Pipeline 계약 테스트까지 양 호스트에서 확인하고, 문서의 `305/305 pending`을 실제 PASS 증거로 바꾸겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-Process -Id 5648 -ErrorAction SilentlyContinue | Select-Object Id,Responding,CPU,WorkingSet64,StartTime | Format-List`
> - Ran `Get-Process -Id 5648 -ErrorAction SilentlyContinue | Select-Object Id,Responding,CPU,WorkingSet64,StartTime | Format-List`
>
> </details>
>
> PS7은 예상대로 더 오래 걸리는 구간이지만 응답 상태와 CPU 진행은 정상입니다. 이 동안 다른 파일 수정은 없고, 문서도 테스트 결과가 확정되기 전 문구를 건드리지 않고 대기 중입니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `Get-Process -Id 5648 -ErrorAction SilentlyContinue | Select-Object Id,Responding,CPU,WorkingSet64,StartTime | Format-List`
> - Ran `Get-Process -Id 5648 -ErrorAction SilentlyContinue | Select-Object Id,Responding,CPU,WorkingSet64,StartTime | Format-List`
>
> </details>
>
> PowerShell 7도 방금 동일한 `305/305 PASS`로 끝났습니다(566.2초). 따라서 새 PLC 경계는 두 PowerShell 엔진에서 동일하게 검증됐습니다. 지금은 HostParity의 `305` 증거 문자열을 소비하는 Pipeline 계약을 양쪽에서 짧게 재검증하고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-CimInstance Win32_Process | Where-Object { ($_.Name -in @('powershell.exe','pwsh.exe')) -and $_.CommandLine -match 'Test-LmcApiDistributionPipeline.ps1' } | Select ProcessId,ParentProcessId,CreationDate,Name,CommandLine | Format-List`
>
> </details>
>
> 양 호스트 Pipeline 계약도 각각 `291/291 PASS`했습니다. 문서 독립 검토에서 테스트 9개를 “stale-disarm”이라고 뭉뚱그린 표현 하나가 부정확하다는 지적이 나와, 실제 의미대로 “기존 negative fixture 9개”로 바로잡고 최종 상태 점검만 하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
>
> </details>
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$ErrorActionPreference='Stop'
> $owned=@(
> 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1',
> 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1',
> 'LMC_Library/LMC_API/Test-LmcDistributionToolingHostParity.ps1',
> 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs',
> 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st',
> 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st',
> 'LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md',
> 'LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md',
> 'LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md',
> 'LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md',
> 'LMC_Library/LasalApiWpfTestApp/API_MAPPING.md',
> 'LMC_Library/LasalApiWpfTestApp/README.md')
> '=== STATUS ==='
> git status --short
> '=== OWNED NUMSTAT ==='
> git diff --numstat -- $owned
> '=== OWNED HASHES ==='
> Get-FileHash -Algorithm SHA256 -LiteralPath $owned | ForEach-Object { '{0}  {1}' -f $_.Hash,($_.Path.Substring((Get-Location).Path.Length+1)) }
> '=== DIFF CHECK ==='
> git diff --check
> git diff --cached --check
> '=== WORKTREES ==='
> git worktree list --porcelain
> '=== REMOVED PATHS ==='
> $removed=@('C:\work\Elmo\Elmo_Master_release_verify_5c48f25','C:\work\Elmo\Elmo_Master_release_verify_5e53865_20260812_a','C:\work\Elmo\Elmo_Master_release_verify_8d51cee','C:\work\Elmo\Elmo_Master_release_verify_105daf2_20260812_a','C:\work\Elmo\Elmo_Master_release_verify_396044a','C:\work\Elmo\Elmo_Master_release_verify_bf31030','C:\work\Elmo\Elmo_Master_udp_eol_verify_8d51cee'); $removed | ForEach-Object { '{0}={1}' -f $_,(Test-Path -LiteralPath $_) }
> '=== RELATED PROCESSES ==='
> @(Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -match 'Verify-LasalUdpCallbackContract|Test-LmcApiDistributionPipeline|LasalApiWpfTestApp.SmokeTests.exe|Elmo_Master_release_verify|Elmo_Master_udp_eol_verify' } | Select-Object ProcessId,Name,CommandLine) | Format-Table -AutoSize`
> - Ran `$self=$PID; @(Get-CimInstance Win32_Process | Where-Object { $_.ProcessId -ne $self -and $_.CommandLine -match '(Verify-LasalUdpCallbackContract\.ps1.+RunSelfTest|Test-LmcApiDistributionPipeline\.ps1|LasalApiWpfTestApp\.SmokeTests\.exe|Elmo_Master_release_verify_|Elmo_Master_udp_eol_verify_)' } | Select-Object ProcessId,Name,CommandLine) | Format-Table -AutoSize`
>
> </details>
>
> <details><summary>Plan</summary>
>
> 지정 worktree 정리, 오류 재현/원인 분리, PLC owner-loss 수정, PC 회귀 테스트, dual-host 검증, 문서와 독립 검토까지 완료했습니다. 남은 것은 사용자가 LASAL IDE에서 build/download한 뒤 실제 PLC로 재시험하는 외부 런타임 단계입니다.
>
> - [x] 지정된 7개 검증 worktree의 안전성 확인 후 제거
> - [x] 최신 Debug 앱으로 오류 재현하고 PC/PLC 원인 분리
> - [x] owner-loss 상황에만 stale callback을 폐기하는 PLC 수정 및 검증기 갱신
> - [x] 양 PowerShell 호스트 회귀 테스트와 독립 diff 검토
> - [x] 관련 설계/테스트 문서 동기화 및 최종 인계
>
> </details>
</details>

정리와 소스 수정은 완료했습니다. 다만 PLC에 아직 다운로드하지 않았으므로 실제 장비에서 해결됐다고 단정할 단계는 아닙니다.

- 지정한 검증 폴더 7개를 모두 제거했습니다.
- 약 5.36 GiB를 회수했고 Git worktree는 메인 폴더 하나만 남았습니다.
- 사용자 `Classes.lcb`와 미추적 파일은 건드리지 않았습니다.

재접속 오류는 PC 포트 잔류가 원인이 아니었습니다. 새 TCP 연결 두 개에서도 PLC가 `0x8080 / ErrorId=-1`을 네 번 반복 반환했습니다. PLC의 오래된 callback fence가 `-8` 상태로 영구 보존되는 경로를 수정했습니다.

- [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st)
- [LMCUdpCallbackSender.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st)

수정은 확정된 owner 전환 또는 현재 socket disconnect에서 정확히 `-8`일 때만 stale callback을 정리합니다. `-9`, 다른 IP, 이전 socket의 늦은 disconnect, 일반 `0x8080/0x405D` 실패는 계속 차단합니다.

PC에도 같은 창·고정 UDP 포트의 `Connect → Close → Connect` 회귀 테스트 두 개를 추가했습니다.

- 정상 Close
- `0x405D ErrorId=-1`이어도 로컬 정리 후 재접속

검증 결과:

- WPF Debug 빌드 PASS
- 신규 WPF 테스트 `2/2 PASS`, 추가 5회 반복도 전부 PASS
- UDP verifier: PS5.1·PS7 각각 `305/305 PASS`
- Pipeline: PS5.1·PS7 각각 `291/291 PASS`
- 양 호스트 AST, `git diff --check`, 독립 리뷰 PASS

남은 단계는 LASAL IDE에서 변경된 두 `.st`를 빌드·다운로드한 뒤 실제 `Connect → Close → Connect` 재시험입니다. 그래도 실패하면 PLC Watch의 `RpcCallbackLastDisarmResult`를 확인해야 합니다. 값이 `-9`라면 stale session이 아니라 `CallbackSender` Network 연결 문제이며, 이번 수정은 안전상 이를 강제로 우회하지 않습니다.

변경 사항은 아직 stage/commit하지 않았습니다. 현재 `VerifyCurrent`는 기존 사용자 변경 `Classes.lcb` identity drift에서 의도대로 중단됩니다.

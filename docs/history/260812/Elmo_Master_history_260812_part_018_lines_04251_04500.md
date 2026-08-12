> </details>
>
> <details><summary>Edited a file, ran a command</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md` (+2 -2)
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
> $patterns = @(
> 'no automatic retry|automatic reconnect.*없|자동 retry.*없|자동 reconnect.*없',
> 'next manual Connect|다음 수동 Connect',
> 'process relaunch|EXE relaunch|프로세스 재시작.*검증|실제 프로세스',
> '100 ?ms.*(guarantee|보장|readiness|준비)',
> 'ErrorId.?=.?-1.*(-8|-9)',
> 'Close button.*nonzero|Close 버튼.*nonzero|항상.*close error',
> '334/334|335/335|1117/1117|1130/1130|af4ab63'
> )
> foreach ($pattern in $patterns) {
>   "PATTERN: $pattern"
>   rg -n -i --pcre2 $pattern -- $docPaths
> }
> "DIFFCHECK"
> git diff --check -- $docPaths
> "CHANGED FILES"
> git diff --name-only -- $docPaths`
>
> </details>
>
> 문서에서 발견된 한 문장을 바로잡았습니다. 명시적 Close는 정상 ACK에서도 오류를 내는 게 아니라, `0x405D` 실패 시에만 로컬 정리 후 오류를 보고한다는 조건부 계약으로 수정했습니다. 나머지 문서는 독립 검토 중입니다.
>
> 현재 문서 범위는 정확히 12개이며 `Classes.lcb`와 기존 미추적 증거 파일은 제외돼 있습니다. 정적 모순 검색과 whitespace 검사는 통과했고, 독립 검토의 마지막 결과만 기다리고 있습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `git diff --unified=1 -- LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md LMC_Library/LMC_API/API_USER_MANUAL_KO.md LMC_Library/LMC_API_Delivery/README.md LMC_Library/LasalApiWpfTestApp/API_MAPPING.md LMC_Library/LasalApiWpfTestApp/DESIGN.md LMC_Library/LasalApiWpfTestApp/README.md`
>
> </details>
>
> 독립 검토에서 재접속 계약 자체는 맞지만, 기존 callback 설명 네 곳이 새 version-2/producer 후보 상태와 충돌하는 것을 찾았습니다. 문서 커밋 전에 해당 문장도 같은 범위 안에서 정정하겠습니다.
>
> <details><summary>Edited 5 files, ran a command</summary>
>
> - Ran `$targets = @(
> @('LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md',515,532),
> @('LMC_Library/LasalApiWpfTestApp/DESIGN.md',1118,1136),
> @('docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md',488,503),
> @('LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md',194,210),
> @('LMC_Library/LasalApiWpfTestApp/API_MAPPING.md',112,124)
> )
> foreach ($target in $targets) {
>   $path = [string]$target[0]
>   "FILE: $path"
>   $lines = Get-Content -LiteralPath $path
>   for ($i = [int]$target[1]; $i -le [Math]::Min([int]$target[2], $lines.Count); $i++) { '{0}: {1}' -f $i, $lines[$i-1] }
> }`
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+6 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
>
> </details>
>
> <details><summary>Edited 8 files, ran 7 commands</summary>
>
> - Ran `git diff --unified=1 -- LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md`
> - Ran `rg -n -C 8 "public void Dispose|void Dispose\(" LMC_Library/LMC_API_Delivery/src/LmcConnection.cs
> rg -n -C 5 "CloseConnectionCoreLocked\(" LMC_Library/LMC_API_Delivery/src/LmcConnection.cs`
> - Ran `$path='LMC_Library/LMC_API_Delivery/src/LmcConnection.cs'
> $lines=Get-Content -LiteralPath $path
> for($i=2024;$i -le 2108;$i++){ '{0}: {1}' -f $i,$lines[$i-1]}`
> - Ran `$path='docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md'
> $lines=Get-Content -LiteralPath $path
> for($i=102;$i -le 126;$i++){ '{0}: {1}' -f $i,$lines[$i-1] }
> $paths=@(
> 'LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt',
> 'LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md'
> )
> foreach($p in $paths){ "FILE: $p"; rg -n -C 2 "strict Close|rethrows after cleanup" -- $p }`
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
> rg -n -i --pcre2 "Dispose.{0,60}(guarantee|보장)|(?:guarantee|보장).{0,60}Dispose" -- $docPaths
> rg -n "strict Close.*throw|strict Close.*rethrow" -- $docPaths`
> - Ran ``rg -n -C 1 "Dispose.*local cleanup|local cleanup.*Dispose|`Dispose`" LMC_Library/LMC_API/API_USER_MANUAL_KO.md LMC_Library/LMC_API_Delivery/README.md LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md``
> - Ran `rg -n -C 1 'Dispose.*local cleanup|local cleanup.*Dispose|Dispose.*보장|Dispose.*수행' LMC_Library/LMC_API/API_USER_MANUAL_KO.md LMC_Library/LMC_API_Delivery/README.md LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md`
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -7)
>
> </details>
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
> git diff --check -- $docPaths
> rg -n -i --pcre2 '2026-07-31 current override|strict Close.{0,60}(cleanup 뒤 throw|rethrows\s*$)|Dispose.{0,40}(guarantees local cleanup|local cleanup을\s*$)' -- $docPaths
> git diff --stat -- $docPaths`
>
> </details>
>
> 독립 최종 검토가 CLEAN으로 끝났습니다. 이제 지정된 12개 문서만 스테이징해 목록과 cached whitespace를 검증한 다음, 코드와 분리된 문서 커밋을 만들겠습니다.
>
> <details><summary>Ran 3 commands</summary>
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
> $preStaged = @(git diff --cached --name-only)
> if ($preStaged.Count -ne 0) { throw "Unexpected pre-existing staged files: $($preStaged -join ', ')" }
> git add -- $docPaths
> $actual = @(git diff --cached --name-only)
> $expected = @($docPaths | Sort-Object)
> $actualSorted = @($actual | Sort-Object)
> if (($actualSorted.Count -ne $expected.Count) -or (Compare-Object -ReferenceObject $expected -DifferenceObject $actualSorted)) {
>   throw "Staged scope mismatch. Actual: $($actual -join ', ')"
> }
> git diff --cached --check -- $docPaths
> "STAGED_COUNT=$($actual.Count)"
> $actual
> git diff --cached --stat -- $docPaths`
> - Ran `git commit -m "docs(wpf): document bounded RPC reconnect recovery"`
> - Ran `$exe = 'C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Debug\LasalMotionControlApiExample.exe'
> $dll = 'C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Debug\LasalMotionControlLib.dll'
> "HEAD=$(git rev-parse HEAD)"
> git log -2 --format='%H %s'
> "STAGED_COUNT=$(@(git diff --cached --name-only).Count)"
> git diff --cached --check
> git diff --check
> "CODE_DOC_DIRTY"
> git status --short -- LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md LMC_Library/LMC_API/API_USER_MANUAL_KO.md LMC_Library/LMC_API_Delivery/README.md LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md LMC_Library/LasalApiWpfTestApp/API_MAPPING.md LMC_Library/LasalApiWpfTestApp/DESIGN.md LMC_Library/LasalApiWpfTestApp/README.md docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md
> "REMAINING_STATUS"
> git status --short
> if (Test-Path -LiteralPath $exe) {
>   $item = Get-Item -LiteralPath $exe
>   $hash = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash
>   "DEBUG_EXE=$($item.Length)|$($item.LastWriteTimeUtc.ToString('o'))|$hash"
> }
> if (Test-Path -LiteralPath $dll) {
>   $item = Get-Item -LiteralPath $dll
>   $hash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash
>   "DEBUG_DLL=$($item.Length)|$($item.LastWriteTimeUtc.ToString('o'))|$hash"
> }
> $ownedProcesses = @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match 'MSBuild|SmokeTests|LasalMotionControlApiExample' })
> "OWNED_PROCESS_COUNT=$($ownedProcesses.Count)"
> $ownedProcesses | Select-Object ProcessName,Id,StartTime`
>
> </details>
>
> <details><summary>Plan</summary>
>
> 코드와 문서가 각각 별도 커밋됐고, 최종 scope/whitespace/프로세스 검증까지 완료했습니다. 실제 PLC 재시험만 현장 확인으로 남습니다.
>
> - [x] X 종료의 0x405D 실패를 로컬 정리 후 보존하고 창 종료 예외를 억제
> - [x] 정확한 persistent -1에 한해 fresh TCP 세션 1회 재시도
> - [x] Debug/Release 빌드와 SDK/WPF 회귀 테스트
> - [x] 관련 12개 문서 정합성 검토 및 별도 커밋
> - [x] 실제 PLC 재시험용 실행 경로·증거 항목 인계
>
> </details>
</details>

수정 완료했고 커밋까지 분리했습니다.

- 코드: `14ccf58` — `fix(wpf): recover bounded RPC reconnect sessions`
- 문서: `8572ae5` — `docs(wpf): document bounded RPC reconnect recovery`

변경된 동작:

- X 종료 중 `0x405D Status=1, ErrorId=-1`이 와도 로컬 TCP/callback 정리를 완료하고 진단 정보만 보존합니다. 예외가 WPF 밖으로 전파되지 않습니다.
- 재실행 후 첫 Connect에서 `0x8080 ErrorId=-1`이 같은 소켓에서 2회 지속되면, 실패 연결을 폐기하고 100ms 후 새 TCP 연결을 정확히 1회 시도합니다.
- 두 번째 TCP도 실패하면 무한 재시도 없이 완전히 정리하고 Connect 버튼을 다시 활성화합니다.
- `ErrorId=0`, malformed 응답, transport 오류, callback 등록 실패에는 fresh-TCP 재시도를 하지 않습니다.

검증 결과:

- SDK Debug/Release: 각각 `1133/1133 PASS`
- WPF Debug/Release 전체: 각각 `339/339 PASS`
- reconnect targeted: `6/6 PASS`
- 독립 callback/reconnect 검토: `9/9 PASS`, P0/P1 없음
- staged 파일 0, 관련 코드·문서 clean
- 기존 `Classes.lcb`와 미추적 증거 파일은 건드리지 않았습니다.

재시험할 Debug 실행 파일은 [LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Debug/LasalMotionControlApiExample.exe)이며 SHA-256은 `28D461E2949AB2F34C004503046371D97EE83026E445F0EB9A01E2685E26D31A`입니다. 시작 로그에서 `ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V1`도 확인할 수 있습니다.

다만 100ms는 PC측 제한 재시도 정책이지 PLC 정리 완료 신호는 아닙니다. 실제 PLC에서 계속 실패한다면 `FreshSessionRetry=Scheduled|Used` 로그와 PLC의 `RpcCallbackLastDisarmResult`를 확인해야 `-8/-9` 또는 lifecycle/ownership 거부를 구분할 수 있습니다. 이번 작업에서는 PLC Download나 LASAL 변경을 수행하지 않았습니다.

<details><summary>93 previous messages</summary>

> 재접속 수정은 끝났지만 전체 목표는 아직 완료가 아닙니다. 현재 설계 문서와 소스를 다시 대조해, PLC 조작 없이 진행 가능한 다음 필수 개발 항목을 좁힌 뒤 구현·검증까지 계속하겠습니다.
>
> <details><summary>Ran 2 commands</summary>
>
> - Ran `$memoryPath='C:\Users\dreiv\.codex\memories\MEMORY.md'
> "HEAD=$(git rev-parse HEAD)"
> git status --short
> "MEMORY_HITS"
> rg -n -i 'Elmo|reconnect|callback|Gate D|LasalApiWpfTestApp' $memoryPath | Select-Object -First 40
> "CURRENT_PENDING_DOC_HITS"

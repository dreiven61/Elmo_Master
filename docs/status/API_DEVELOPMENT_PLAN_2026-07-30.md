# Elmo Master API 개발 계획

> **2026-08-03 후속 설계 우선:** 이 문서는 2026-07-30/31 snapshot이다. 아래의
> `ReferenceAxis`, physical switch, `MoveReference()` 계획과 당시 test 수치 및
> SourceOnly PASS 판정은 현재 작업에 사용하지 않는다. 현재 Home/encoder 구현과 IDE 순서는
> [LMC Home current-position zero and encoder maintenance IDE handoff](../architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md),
> 최신 소스 대조는 [260803 continuation summary](../history/260803/99_analysis_summary.md)의
> 9절을 따른다.

- 작성 기준: 2026-07-30 계획, 2026-07-31 working-tree 검증 반영
- 기준 branch/HEAD: `main@6537bcf1bf0fdb338a934b63891fc9ee110aecad`
- 현재 릴리스 상태: `0.9.1-preview`, production **NO-GO**
- 진행 현황: [API_DEVELOPMENT_PROGRESS_2026-07-30.md](API_DEVELOPMENT_PROGRESS_2026-07-30.md)
- Group Reset stable 계약:
  [GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md](../architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md)
- Axis SetPosition dormant 계약:
  [AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md](../architecture/AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md)
- Axis Reference LASAL-native dormant 계약:
  [AXIS_REFERENCE_LASAL_NATIVE_DORMANT_CONTRACT_2026-07-31.md](../architecture/AXIS_REFERENCE_LASAL_NATIVE_DORMANT_CONTRACT_2026-07-31.md)
- HTML 계획표: [API_DEVELOPMENT_PLAN_2026-07-30.html](API_DEVELOPMENT_PLAN_2026-07-30.html)

> 이 계획은 새 기능 수를 늘리는 것보다 현재 구현을 재현 가능한 source baseline으로 고정하고,
> LASAL/PLC 증거를 닫는 것을 먼저 둔다. PC/static PASS와 PLC/runtime PASS를 합치지 않는다.

> **2026-08-11 current release override:** `88f1c57`은 staged example solution의 exact
> one-project/GUID/Debug+Release `Any CPU` 계약과 두 configuration Rebuild를 고정했고,
> `d735446`은 Control `HandleRequest` whole-method fence PS5.1/PS7 `13/13`을 고정했다.
> `d6ddf05`는 method-size parser를 checkout/EOL-stable하게 고쳐 main mixed-EOL과 clean
> detached에서 동일한 inventory `101/98/3`, self-test `16/16`을 고정했다. `afdf6a3` UDP verifier는 PS5.1/PS7
> `296/296`을 PASS하지만 default production invocation은 승인된 physical snapshot ratchet이
> 없는 `TerminalWakeBrokerCandidate`에서 계속 STOP한다. Clean detached `afdf6a3` full
> Distribution은 약 `214`초 뒤 같은 gate에서 중단됐다. 후속 `d6ddf05`/`bf31030`까지 포함한
> clean detached `bf31030` direct Windows PowerShell 재실행도 exit `1`, `214.415`초 뒤 같은
> Debug `RunTests` STOP이었고 focused verifier는 `10.320`초에 no-approved-ratchet blocker를
> 확인했다. 두 실행은 tracked clean이지만 noncanonical manual 때문에 `-AllowDirty`/
> `dirty-preview` policy였다. Candidate, stage/lock, actual-EXE/manifest/publish 증거는 생성되지
> 않았으며 canonical/manual hashes는 불변이고 LASAL IDE/PLC Download/runtime은 실행하지 않았다.
> 후속 `bf31030`은 exact LASAL validation input과 physical Network aggregate를 release
> fingerprint에 묶고 PS5.1/PS7 pipeline `192/192`을 PASS했지만 Gate D STOP을 바꾸지 않는다.

## 목표

1. 현재 active 53-command 범위를 동일 source hash에서 재현 가능하게 만든다.
2. 최신 LASAL project를 IDE에서 Rebuild/Link하고 current PLC에 내려 source/network/unit/task
   정합을 확인한다.
3. motion/group와 diagnostics의 미완료 live matrix를 pcap/QTEST/PLC log로 닫는다.
4. 이번 release에서 제외할 advanced 기능과 상위 요구 공백을 승인하고 capability와 policy를 OFF로 유지한다.
5. production Definition of Done을 통과한 뒤에만 distribution과 외부 manual을 갱신한다.

## 현재 위치

| Milestone | 상태 | 현재 근거 | 다음 gate |
|---|---|---|---|
| M0. Working-tree baseline 고정 | **부분 완료** | current SDK Debug/Release 1042/1042, WPF Debug/Release 297/297, LASAL SourceOnly/full PASS | 목적별 commit + clean checkout 재현 |
| M1. LASAL current integration | **source/static 완료 / fresh IDE 대기** | `IntegratedReadOwnerDormant`와 dormant Admin `0x7D12/0x7D13` current SourceOnly/full PASS; Rebuild/Link `0 error(s), 20 warning(s)`와 3-class smoke/log는 callback/`0x7D12`/`0x7D13` 이전 checkpoint | callback+`0x7D12`+`0x7D13` current source Save/Rebuild/Link/smoke 후 current PLC cold download + provenance |
| M2. PLC read-only/safety baseline | **부분** | 기존 일부 capture, topology static inventory와 dormant read-owner source/static | fresh build cold download + raw/physical read qualification |
| M3. Active motion/diagnostics qualification | **부분** | Single Axis runner 9/9과 whole-sequence durable journal/process-restart recovery, Group Enable durable accepted-once, Axis1 exact-session four-ticket/manual-Write gate PC PASS | current PLC Motion/Power/SDO live matrix |
| M4. Gated advanced diagnostics/I/O | **선택/후속** | D4/PI off, topology read-owner dormant, Axis1 SDO Write source-active | 기능별 live 승인 |
| M5. Product release | **historical dirty-preview PASS / current Gate D STOP** | 2026-07-31 `2.0-candidate` sibling/manifest PASS는 historical이다. Current tracked-clean detached `afdf6a3` + exact `2.3-candidate` manual build는 `-AllowDirty`/`dirty-preview` policy로 실행됐고 Debug `RunTests`의 unapproved Gate D physical snapshot ratchet에서 fail-closed했다. Candidate/manifest/publish는 없음 | reviewed Gate D physical snapshot transition + clean full candidate 재현 + M3 active scope DoD + M4/상위 공백 명시적 제외 승인 |

## 우선순위 요약

| 우선순위 | 범위 | 종료 조건 |
|---|---|---|
| **P0-A** | source freeze와 PC 회귀 안정화 | 동일 hash에서 SDK Debug/Release, WPF build/smoke 전량 PASS |
| **P0-B** | LASAL full 정합과 IDE 적용 | SourceOnly/full PASS, Rebuild/Link, implementation smoke, log clean |
| **P0-C** | current PLC download와 active 범위 qualification | safety readback, 25-command, D1~D5 승인 matrix와 증거 완결 |
| **P0-D** | preview 범위/배포 정리 | advanced/상위 공백의 OFF 범위 승인, 원본 무변경 transactional candidate, semantic preflight, external manual, manifest/hash/provenance |
| **P1** | dynamic CREVIS read/write와 선택적 advanced diagnostics | capability별 raw live + physical correlation 뒤 gate 활성 |
| **P2** | 미구현 상위 API와 제품화 | 승인된 요구별 API/wire/LASAL/live/package DoD 통과 |

## P0-A. Working-tree baseline과 PC gate

### 작업

1. API, WPF, LASAL, tests를 동시에 수정하는 작업을 잠시 멈추고 snapshot hash를 기록한다.
2. 현재 변경을 기능별로 검토한다. 사용자 변경은 보존하며 generated/IDE file과 사람이 수정하는
   source를 구분한다.
3. SDK Debug/Release를 forced Rebuild 후 실행한다.
4. WPF Release를 forced Rebuild하고 smoke를 실행한다.
5. WPF Debug는 실행 중인 VS/WPF가 기본 `bin/Debug`를 잠그면 임시 `OutDir`로 build만 확인한다.
6. 모든 실행의 시작/종료 source hash가 같은지 확인한다.
7. Group Enable/PowerOff timing 회귀를 최소 두 번 반복해 비결정성을 닫는다.
8. 시험 수치와 결과를 이 진행 문서와 기존 System of Record에 동기화한다.
9. Group Reset stable member error-clearance를 same-session split contract로 구현·시험했다.
   valid `0x20D2` observed snapshot 뒤 `0x2049`를 정확히 한 번 보내고, Resume round마다
   `0x2045` 한 번과 pinned member 전원의 `0x2028`을 보내 all-clear 3회 연속을 확인한다.
   timeout/cancel/status failure는 status-only Resume을 보존하고 accepted/outcome-uncertain
   Stop/PowerOff/safe Disable만 terminal supersede한다. valid safety NACK는 Reset continuation을
   복원한다. prepared/accepted command-before durable journal은 endpoint, PLC identity, group과
   ordered member snapshot을 고정한다. reconnect/restart는 fresh `0x20D2` exact-match 뒤 새
   status-only continuation만 attach하고 `0x2049`를 재전송하지 않는다.

### 현재 blocker

- 최신 Group Enable qualifier의 durable accepted-once 경로와 Axis1 manual SDO Write의
  exact-session four-ticket 활성 gate, identity-pinned fresh capability pre-wire와 proof 영구 폐기는
  PC 시험에서 PASS했다.
- current SDK Debug/Release build/test는 Axis Reference 신규 16개와 fake-RPC request snapshot
  회귀를 포함해 각각 1042/1042, WPF Debug/Release는 각각 297/297 PASS했다. SDK/WPF Debug
  추가 반복도 같은 1042/297 count로 PASS했다. fake-RPC worker 기록은 한 lock에서 request와
  session ordinal을 함께 추가하고 관측자에는 stable snapshot만 반환하므로, WPF recovery poll의
  concurrent `List` 열거에서 관측된 `Collection was modified` flake를 제거했다. Single Axis live
  runner focused 9/9은 pre-wire
  cancel, Build drift zero mutation, external Axis Stop/PowerOff no-duplicate 계약까지 포함한다.
- Admin `0x7D12 SetAxisPosition` request를 56-byte frame으로 고정했다. fresh
  DiagnosticsBuild/BootId/MapRevision, process/session을 넘어 유일한 4 x U32 client intent,
  expected actual-position CAS와 prepare-time one-shot을 함께 pin한다. `0x7D14`는 이 exact
  key의 terminal result만 읽는 56-byte read-only query와 92-byte success response 계약이다.
  PLC capability bit 3/5는 OFF이고 raw valid `0x7D12`도 `InvalidState/detail 10`, native
  SetPosition 0회로 닫힌다. retained two-bank store, query route, terminal retirement CAS는
  LASAL IDE 구조 작업 전이라 source-active가 아니다. 독립 journal core를 UI에 arm하지 않고,
  authoritative query·unified ownership과 함께 연결하기 전에는 WPF 실행 경로를 열지 않는다.
  `ActualPosition == Target`은 과거 성공 증거가 아니다.
- Admin `0x7D13 StartAxisReference` dormant slice는 56-byte request/32-byte response 계약이다.
  capability bit 4는 OFF, native `_LMCAxis.MoveReference()` 호출은 0회이고 WPF에는 노출하지
  않는다. 이는 DS402 homing이 아닌 LASAL-native reference다. 현재 Motion Network에는 physical
  `HWMin/HWMax/RefSwitch/ZImpulse/LatchPos` source가 없으며, 활성화 시 PLC가 독립 감시할
  `MaxTravel>0`과 `TimeoutMs>0`은 mandatory다. 신규 Reference PC 16개와 LASAL
  SourceOnly/full static은 PASS했고 fresh IDE gate는 아직 관측 전이다.
- Single Axis whole-sequence 상위 journal의 단조 checkpoint/exact CAS, 보수적 crash promotion,
  process 종료 뒤 자동 mutation 0회와 명시적 Power Off 안전 복구는 focused PC 회귀로 닫았다.
  이는 current PLC download, 실제 축 이동/정지 또는 packet 증거가 아니다.
- Group Reset stable member error-clearance의 same-session continuation/API/WPF/test를 구현했다.
  raw `GroupReset[Async]`만 ACK-only로 남고 WPF 버튼은 durable exact reconnect/restart
  status-only 경로를 포함한 stable API를 사용한다. current PLC download와 physical/runtime
  capture는 아직 남아 있다.
- 다만 working tree는 아직 대규모 미커밋 상태다. 목적별 commit과 clean checkout 재현 전에는
  M0 source baseline 고정이 완료된 것이 아니다.

### 완료 조건

- 변경 없는 하나의 source hash에서 SDK Debug/Release 전량 PASS
- 같은 hash에서 WPF Release Rebuild와 smoke 전량 PASS
- 반복 실행 결과와 test count가 동일
- working-tree snapshot 또는 commit/hash manifest 보존

### 실행 명령

```powershell
$taskMsBuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe'
$taskPcTests = '.\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj'
$taskWpfSln = '.\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.sln'
$taskWpfSmoke = '.\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj'

function Get-TaskWorkingTreeFingerprint {
  $taskTracked = (git diff --binary HEAD 2>$null | git hash-object --stdin).Trim()
  $taskUntrackedManifest = @(
    git -c core.quotepath=false ls-files --others --exclude-standard |
      Sort-Object |
      ForEach-Object {
        $taskFile = $_
        '{0}  {1}' -f (Get-FileHash -LiteralPath $taskFile -Algorithm SHA256).Hash, $taskFile
      }
  )
  $taskUntracked = (($taskUntrackedManifest -join "`n") | git hash-object --stdin).Trim()
  "$taskTracked/$taskUntracked"
}

$taskBefore = Get-TaskWorkingTreeFingerprint
& $taskMsBuild $taskPcTests '/t:Rebuild;RunPcTests' /p:Configuration=Debug /nologo
& $taskMsBuild $taskPcTests '/t:Rebuild;RunPcTests' /p:Configuration=Release /nologo
& $taskMsBuild $taskWpfSln /t:Rebuild /p:Configuration=Release /nologo
& $taskMsBuild $taskWpfSmoke /t:RunWpfSmokeTests /p:Configuration=Debug /nologo
& $taskMsBuild $taskWpfSmoke /t:RunWpfSmokeTests /p:Configuration=Release /nologo
& $taskMsBuild $taskPcTests /t:RunLasalContract /p:Configuration=Release /p:LasalTopologyIoCheckpoint=IntegratedReadOwnerDormant /nologo
& $taskMsBuild $taskPcTests /t:RunLasalNetworkContract /p:Configuration=Release /p:LasalTopologyIoCheckpoint=IntegratedReadOwnerDormant /nologo
$taskAfter = Get-TaskWorkingTreeFingerprint
if ($taskBefore -ne $taskAfter) { throw "Working tree changed during qualification: $taskBefore -> $taskAfter" }
```

## P0-B. LASAL full 정합, IDE build, PLC download

### 작업 순서

1. [완료] current external/generated source와 verifier의 network/metadata 계약을 대조하고
   `IntegratedReadOwnerDormant` SourceOnly/full static을 모두 PASS했다.
2. [완료, callback/`0x7D12`/`0x7D13` 전 checkpoint] generated `.st/.lcb/.lba`를 임의 수동 교정하지 않고 fresh LASAL reload로 current
   `LMCSdoExecutor.st` constructor까지 다시 읽었다.
3. [완료, callback/`0x7D12`/`0x7D13` 전 checkpoint] same-peer `TCPIPServer`, `TCPMotionInterface`, `LMCControlCommandService`,
   `LMCDiagnosticsService`, `LMCEcatInputLatch`, `LMCRecorderStore`, `LMCSdoExecutor`,
   topology/network를 포함해 Rebuild/Link했다: `0 error(s), 20 warning(s)`, Linker `Done`.
4. [완료, callback/`0x7D12`/`0x7D13` 전 checkpoint] `LMCSdoExecutor`는 Axis1 D5 Write 활성 경로의 current integration gate로 확인했다.
   constructor declaration, generated `@STD` binding, state/buffer 초기화, 최초 `Idle` publish를
   IDE/source/static에서 확인했고, build 전후 source SHA-256이 동일했다.
5. [완료, callback/`0x7D12`/`0x7D13` 전 checkpoint] `LMCEcatInputLatch`, `LMCDiagnosticsService`, `TCPMotionInterface` 세 변경 class의
   implementation을 IDE에서 직접 열어 smoke를 수행했다.
6. [완료, callback/`0x7D12`/`0x7D13` 전 checkpoint] current LASAL PID와 smoke 기준 이후 `%TEMP%\Lasal2.log`의 새
   `CInvalidArgException`은 0건이다.
7. [완료, `0x7D13` 전 checkpoint] SourceOnly와 full/network static을 callback+`0x7D12` source에서 다시 PASS했다.
8. [완료] callback+`0x7D12`+`0x7D13` current source의 SourceOnly와 full/network static을 다시 PASS했다.
9. [대기] callback+`0x7D12`+`0x7D13` current source를 IDE Save/Rebuild/Link하고 `TCPMotionInterface`/`LMCControlCommandService` implementation smoke/log를 다시 확인한다.
10. [대기] PLC cold download 후 project/source/network/unit/task와 BootId/MapRevision을 기록한다.

### 정적 계약 개별 진단 명령

아래는 P0-B만 다시 볼 때 쓰는 명령이다. 최종 qualification은 P0-A 일괄 block처럼 PC/WPF와
정적 계약을 하나의 fingerprint 전후 범위에서 실행한다.

```powershell
$taskMsBuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe'
$taskPcTests = '.\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj'

& $taskMsBuild $taskPcTests /t:RunLasalContract `
  /p:Configuration=Release `
  /p:LasalTopologyIoCheckpoint=IntegratedReadOwnerDormant /nologo

& $taskMsBuild $taskPcTests /t:RunLasalNetworkContract `
  /p:Configuration=Release `
  /p:LasalTopologyIoCheckpoint=IntegratedReadOwnerDormant /nologo
```

### 완료 조건

- [x] `Phase5TransportClean / IntegratedReadOwnerDormant` SourceOnly와 full 모두 PASS
- [ ] callback+`0x7D12`+`0x7D13` current source LASAL Compiler/Linker ERROR/FATAL 0 — 세 변경 전 checkpoint WARNING 20 기록
- [x] `LMCSdoExecutor` constructor/binding/init/`Idle` publish 계약 확인
- [ ] callback+`0x7D12`+`0x7D13` current source 변경 class implementation smoke PASS
- [ ] current source smoke 이후 신규 `CInvalidArgException` 0
- [ ] current PLC download 및 source/network/unit/task provenance 보존
- [ ] same-peer takeover가 master project에서 정상이고 다른-IP/fault/soak 경계가 기록됨

## P0-C. PLC qualification

### 선행 안전 gate

아래가 승인되기 전 motion/write 시험을 시작하지 않는다.

- physical E-stop과 STO/drive disable 경로
- HW/SW limit와 travel 범위
- axis 1..9 UNIT, MaxModulo, BinOffset, reference offset
- physical axis 1..4와 simulated axis 5..9의 적용 범위
- group X/Y/Z/U 연결과 작은 시험 거리
- 한 번에 하나의 motion owner/session
- Stop/PowerOff가 TCP Close/Cancel과 다른 동작이라는 운영자 확인

### 실행 순서

1. Connect, capability, BootId/MapRevision, lookup
2. read-only axis/group status, position, parameters, drive status/error
3. configured topology `0x7E11/0x7E12`와 7-entry order/CRC
4. current source용 `topology-inventory` raw qualifier를 실행하고 bit 14, nonzero BootId,
   `0x15867EEC`, exact 7-entry order/identity/CRC가 담긴 durable report를 보존
5. axis별 PowerOn -> stable status -> 작은 Move -> final position -> Stop -> stable standstill -> PowerOff.
   개발 WPF의 Single Axis live runner는 이 순서를 실제 `0x2023/0x20A0/0x2022/0x2023` accepted-once
   경로와 10단계 whole-sequence 상위 journal 및 command별 durable journal로 구현했다. 세 물리
   안전 확인 전에는 RPC 0건이며, Move 이후 취소/실패는 Move replay 없이 exact-identity
   Stop/PowerOff cleanup을 수행한다. crash 승격과 process 종료 뒤 zero-replay/명시적 Power Off
   recovery까지 PC/fake-RPC 회귀는 완료했지만 current PLC 실행/packet/물리 안전 proof는 별도다.
6. group PowerOn -> power poll -> SetKin -> durable journal arm -> Group Enable `0x2047` exactly
   once -> accepted durable publish -> `0x2045` status-only stable Lock proof -> Move -> final state
   -> Unlock -> PowerOff. qualification runner도 같은 accepted-once 경로만 사용한다.
7. Group Reset은 valid `0x20D2` observed member snapshot을 먼저 보존하고 `0x2049`를 정확히
   한 번 보낸다. 이후 round마다 `0x2045` 한 번과 pinned member 전원의 `0x2028`을 exact
   snapshot 순서대로 보내 group/member error 모두 0인 full-clear를 3회 연속 확인한다.
   generic SDK snapshot은 `1..16`개 nonzero/unique reference를 허용하며 expected topology/current
   PLC build attestation이 아니다. timeout, disconnect 또는 Stop/PowerOff/safe Disable takeover
   capture에서 automatic Reset replay가 0회인지 확인한다. PC에서는 command-before journal의
   Armed/Accepted/status-round process-kill 뒤 exact endpoint/DiagnosticsBuild/BootId/MapRevision와
   group/member snapshot을 다시 확인하고 fresh `0x20D2` 1회, Reset 0회로 status-only recovery하는
   계약을 검증한다. 이 PC 증거와 PLC/runtime capture는 구분한다.
8. motion/group 25-command success/expected-failure matrix
9. D1 Catalog/Health/PI fault·stale matrix
10. D2 exact 24-entry lifecycle, 100회, one-slave-offline partial/recovery
11. D3/D4 Single/Ring/trigger/reconnect-adopt/hash/soak
12. D5 Read offline/abort/contention/timeout/drain/queued-cancel/disconnect/orphan/late callback
13. Axis1 `0x2F00:24 Int32/4` SDO Write를 exact current connection session,
    `DiagnosticsBuild`, `BootId`, `MapRevision`, approved target identity에 고정한다. baseline Read
    -> 값 불변 `preWriteGuard` Read -> final safety -> Write 정확히 1회 -> guarded readback의
    서로 다른 four-ticket same-value proof가 모두 끝난 뒤에만 manual SDO Write를 연다.
    qualification evidence에는 `preWriteGuard`, 각 terminal `resultBytes`, 네 ticket identity를
    보존한다. identity drift와 response-loss/reconnect에서는 manual Write를 다시 닫고 자동 replay
    없이 recovery readmission을 확인한다.
14. negative/malformed wire와 reconnect/fault recovery

### 캡처 기준

- capture filter:
  `host 10.10.150.1 and (tcp port 4000 or udp port 5000)`
- display filter:
  `ip.addr == 10.10.150.1 && tcp.port == 4000 && tcp.len > 0`
- TCP 이상:
  `tcp.analysis.retransmission || tcp.analysis.lost_segment || tcp.analysis.out_of_order || tcp.flags.reset == 1`
- 버튼/command 전 캡처를 시작하고 final PASS/FAIL/ABORTED, cleanup, 최종 상태 뒤 약 2초까지 보존한다.
- scenario마다 같은 이름의 `pcapng + QTEST TXT + PLC log`를 보존한다.
- ACK 뒤 `0x2028`/`0x2045` stable samples와 endpoint position/readback을 완료 근거로 쓴다.

### 성능 회귀 gate

동일 controller, task cycle, compiler, build 조건에서 기존 baseline과 비교한다.

- control request dispatch 10,000회 이상
- task overrun 0
- dispatch P95가 baseline 대비 5% 이상 악화되지 않음
- throughput이 baseline의 98% 아래로 떨어지지 않음
- response frame과 status byte가 baseline과 byte-identical

### 완료 조건

- active command별 request/response/error/final-state evidence 존재
- Power/Enable/Stop/Move ACK와 최종 상태가 구분되고, Group Reset ACK와 group/member
  all-clear 3회 proof도 구분됨
- expected failure에서 mutation 0 또는 승인된 recovery가 입증됨
- fault/reconnect 뒤 stale descriptor/session을 재사용하지 않음
- 시험 binary/source hash와 PLC BootId/MapRevision이 report에 기록됨
- `topology-inventory` durable report와 성능 회귀 gate PASS

## P0-D. Release scope와 배포

### 먼저 결정할 범위

| 기능 | 현재 상태 | 이번 release 권장안 | 활성 조건 |
|---|---|---|---|
| D4 Double bank | source/PC contract, gate OFF | preview에서 제외 유지 | 2.56 MB RAM, RT jitter, A/B 동시, reconnect/release live PASS |
| D5 SDO Write | Axis1 exact target source-active; exact session/build/BootId/MapRevision four-ticket proof 뒤 manual Write를 여는 focused PC gate PASS | PLC/live 완료 전 production NO-GO | fresh download/bit 9/UI24 ownership, `preWriteGuard`/`resultBytes`, four-ticket/readback, pcap/physical proof |
| D5 Read orphan recovery | `ApplicationRecoveryOnly` 가능, durable orphan witness 없음 | `orphanQualified=false`, fail-closed와 수동 recovery로 제한 | disconnect/orphan/late-callback 무해성 + 수동 recovery 증거; durable witness는 P1 |
| PI Write | C# scaffold, PLC Unsupported | 제외 유지 | semantic allowlist, handler, fault/physical proof |
| Dynamic Health/DI | C#/WPF와 PLC `0x7E13/22` read-owner 구현, dormant static/IDE PASS | raw/physical proof 전 production 제외 | current PLC download, dormant raw qualifier, disconnect/recovery, 32-pattern physical correlation 뒤 bits 15/16 활성 |
| Digital Output | C#/WPF guard, PLC route 없음 | P1 read-only 이후 | `0x7E23`, bit 17, RT single owner/CAS/readback/fault proof |
| callback ownership / typed callback | `0x405C` wire 유지, exact TCP-peer/port validate-then-commit와 raw SDK session provenance/WPF stale-queue drop source 계약 구현; typed sender/parser 없음 | endpoint ownership은 유지하고 typed callback은 이번 release에서 명시적 제외 | current PLC duplicate/mismatch capture; typed 기능은 실제 payload/schema와 PLC event sender 승인 뒤 별도 |
| SetPosition | 128-bit intent+diagnostics identity `0x7D12` SDK/wire와 LASAL dormant parser, read-only `0x7D14` SDK 계약, 독립 journal core; bit 3/5 OFF, query PLC route/store 없음, native call 0, WPF 미연결 | production에서 제외 유지 | IDE-created two-bank retained store/query/terminal retirement CAS, journal+unified axis/group mutation ownership 동시 연결, task/core priority, application-approved `SetPositionMaxJump>0`, `IsReferenced` 정책과 PLC proof |
| ReferenceAxis | `0x7D13` SDK/wire/LASAL dormant fail-closed; 56-byte request/32-byte response, bit 4 OFF, native call 0, WPF 미노출; `HomeDS402` 적응 항목이지만 DS402 homing은 아님 | production에서 제외 유지 | physical reference input 배선/active level/debounce, recipe별 native mode, unified mutation ownership, mandatory PLC `MaxTravel`/`TimeoutMs` watchdog과 축별 bench proof |
| 상위 실제 미구현 | `HomeDS402Ex`, `SetOpMode` 2개 | production 전 명시적 제외 승인 필수 | 별도 승인 시 dedicated command DoD로 구현 |

### 배포 작업

1. release scope와 제외 기능을 capability/SDK/WPF/manual에서 일치시킨다.
2. external DOCX/PDF에 preview, 안전, UNIT, polling, Close/Cancel != Stop을 반영한다.
3. **[구현/실패경로 검증 완료]** 기존 `LMC_API_Distribution`을 in-place
   Rebuild/overwrite/cleanup하지 않는다.
   같은 volume의 새 staging directory에 current source candidate를 조립하고 모든 검증이 끝난
   뒤 존재하지 않는 candidate target으로 한 번만 rename한다. 실패 시 검증된 staging만
   제거하고 기존 package의 file-set/hash가 그대로인지 확인한다.
4. **[구현/정책 회귀 완료]** preview/production, Axis1-only SDO Write, dormant bits 15~17,
   `0x7E23` 부재, PI/D4 Double OFF와 manual 경고를 SDK/LASAL/WPF/DOCX/PDF에서 교차 검증하는
   semantic policy preflight를 candidate finalize 전 실행한다.
5. **[dirty-preview 실제 PASS]** current Markdown의 Axis1 exact target,
   identity-pinned four-ticket gate와 Axis2~4 차단을 반영한 `2.0-candidate` DOCX/PDF를 전 페이지
   검토했다. 검수본 exact bytes를 명시적 입력으로 사용한 sibling candidate가 semantic
   15-check를 통과했다.
6. **[구현/실제 manifest PASS]** `RELEASE_MANIFEST.md` schema 2에 source
   commit/input hash, dirty 여부, DLL version, semantic policy hash/result와 모든 파일
   SHA-256을 기록하고 즉시 재검증했다. noncanonical manual은 `-AllowDirty`와
   `dirty-preview`를 강제하고 transaction-locked byte snapshot/metadata를 manifest baseline에 묶는다.
7. **[구현/fixture 86/86 및 실제 PASS]** 임시 `bin/obj/.vs`, Reports와 captures를 candidate에서 제외한다.
8. **[clean detached 실행/STOP]** `88f1c57`, `d735446`, `afdf6a3` 뒤 clean detached
   `afdf6a3`에서 exact `2.3-candidate` DOCX/PDF로 full build를 실행했다. 약 `214`초 뒤 첫
   Debug `RunTests` 내부 `TerminalWakeBrokerCandidate`의 승인된 physical snapshot ratchet
   부재로 중단됐고 sibling candidate, actual-EXE gate, manifest와 publish/final rename에는
   도달하지 않았다. Git tracked status는 clean이었지만 noncanonical manual 입력 때문에
   `-AllowDirty`/`dirty-preview` policy로 실행했다. Gate D reviewed transition 없이 우회하거나
   production PASS로 바꾸지 않는다.

## P1. Dynamic CREVIS와 advanced diagnostics

### P1-1. read-only topology/health/DI

1. [완료] LASAL IDE에 Coupler/InputSlot/OutputSlot declaration과 network를 생성했다.
2. [완료] `LasalTopologyIoCheckpoint=IdeStructureReady` static checkpoint를 통과했다.
3. [완료] 464-byte coherent snapshot owner와 `0x7E13` Node Health, `0x7E22` Digital I/O Read를 구현했다.
4. [완료] capability bits 15~17을 OFF로 유지한 채 `IntegratedReadOwnerDormant`
   SourceOnly/full static, Rebuild/Link와 3-class implementation smoke를 통과했다.
5. [완료] qualifier V2를 fail-closed로 보강했다. explicit scope/mode, exact 8/17-frame
   dry-run, `0x7E23` 금지, create-new durable report, binary/source fingerprint,
   BootId/build/exact `MapRevision=0x957F101E`, cleanup 뒤 result와 2초 evidence retention,
   외부 pcap/PLC-log hash 연계를 PC test와 dry-run으로 확인했다.
6. [대기] `topology-io-qualify --scope integrated-read-owner-dormant --execute-live
   --confirm PLC-RAW-TOPOLOGY-IO-READ ...` raw qualifier의 durable live report를 보존한다.
7. [대기] GL/Elmo disconnect/recovery와 32-pattern DI physical correlation을 통과한다.
8. [대기] 위 증거가 끝난 뒤에만 bits 15/16을 활성화한다. `0x7E23`과 bit 17은
   P1-2가 완료될 때까지 구현/활성화하지 않는다.

### P1-2. Digital Output

read-only P1-1을 먼저 완료한다.

- `0x7E23` RT single mailbox
- whole/masked CAS와 output revision
- stale/offline/invalid에서 mutation 0
- response loss 자동 replay 0
- physical readback, cable/fault, owner contention matrix
- 모든 증거가 끝난 뒤 bit 17과 SDK allowlist 활성

### P1-3. 선택적 D4/D5 활성화

- D4 Double: RAM/jitter/A-upload-B-capture/reconnect/release 증거 후 gate 활성
- D5 Write: Axis1-only source-active 상태를 live qualification한다. 성공 시 승인 범위를 유지하고
  실패 시 gate를 다시 OFF한다.
- PI Write: semantic allowlist, PLC handler, fault/physical proof를 갖춘 별도 승인 기능으로만 활성
- D5 Read orphan: durable orphan witness를 추가하지 않으면 `ApplicationRecoveryOnly`,
  `orphanQualified=false`, disconnect fail-closed 범위를 유지한다.

## P2. 신규 API와 제품화

요구사항 상위 21개는 active 17개, dormant/partial 2개(`HomeDS402`의 LASAL-native
`ReferenceAxis` 적응, `SetPosition`), 실제 미구현 2개(`HomeDS402Ex`, `SetOpMode`)다. 아래
항목을 이번 release에서 미룬다면 production 승인 기록에 각각 제외 사유와 영향 범위를 남긴다.

1. `ReferenceAxis` / `HomeUsingLasalReference`
   - `0x7D13 StartAxisReference`의 56-byte request/32-byte response dormant 계약까지 구현했다.
     capability bit 4 OFF, native `_LMCAxis.MoveReference()` 0회, WPF 미노출이다.
   - 요구사항 감사에서는 `HomeDS402`의 LASAL-native 적응으로 분류하지만 DS402 homing은 아니다.
     `HomeDS402Ex`는 계속 실제 미구현이다.
   - 현재 physical `HWMin/HWMax/RefSwitch/ZImpulse/LatchPos` source가 없다. 활성화 전에 배선,
     active level/debounce, ownership과 recipe별 native mode를 확정하고 PLC-side mandatory
     `MaxTravel`/`TimeoutMs` watchdog을 축별 bench에서 입증한다.
2. `SetPosition`
   - `0x7D12`는 4 x U32 client intent와 fresh diagnostics identity를 포함하는 56-byte
     request로 갱신했고 LASAL dormant parser는 valid request도 `InvalidState/detail 10`,
     native `_LMCAxis.SetPosition` 0회로 닫는다.
   - `0x7D14 ReadAxisSetPositionOutcome` SDK query와 독립 durable journal core는 구현하되,
     PLC에는 아직 retained store/route/retirement가 없고 capability bit 3/5는 OFF다. journal을
     MainWindow dispatch/interlock에 연결하거나 WPF 버튼을 노출하지 않는다.
   - 활성화 전 IDE-created two-bank store, exact read-only query와 terminal retirement CAS,
     journal/no-auto-replay와 axis/group unified mutation ownership의 동시 연결, motion RT와
     task/core priority 정합, zero velocity/safe axis state, software limit, application-approved
     `SetPositionMaxJump>0`, `IsReferenced` 정책과 실제 PLC proof가 필요하다.
3. `SetOpMode`
   - LMC가 DS402 mode를 소유할지 결정하고 dedicated state machine으로 구현
4. 이후 semantic Axis/Group parameter write, Axis Override, 8-byte SDO Read, typed callback
5. generic kinematics, MoveCircle, Profile Conditioning은 실제 요구 승인 후 별도 설계

모든 신규 command의 공통 DoD:

- public C# API와 immutable typed result
- golden request/response bytes와 malformed parser test
- LASAL parser/dispatch/target validation과 deterministic error
- fake-RPC integration, WPF smoke
- 실제 PLC success/expected-failure/fault test
- Wireshark 재캡처와 final state/readback
- packet map, API manual, backlog, distribution 동시 갱신

## 의존성과 위험

| 항목 | 영향 | 대응 |
|---|---|---|
| working tree 동시 변경 | test 수치와 hash가 즉시 stale | gate 실행 동안 source freeze, 시작/종료 hash 비교 |
| LASAL generated state | text source와 binary metadata 불일치 | IDE 재생성 후 full static으로 확인, 임의 binary 판단 금지 |
| 실제 장비 접근 | PLC/runtime DoD 진행 불가 | read-only와 mutation 일정을 분리하고 캡처 창 확보 |
| 안전 승인 | motion/write 시험 금지 | E-stop/limit/UNIT/reference checklist 서명 |
| D4 RAM/jitter | PLC cycle/메모리 위험 | gate OFF 유지, 수치 측정 후 별도 승인 |
| SDO/DO/SetPosition/Reference mutation | 재전송 시 장비 상태 불명확 | single writer, no replay, command별 durable journal, physical verification; SetPosition/Reference는 unified axis/group ownership, Reference는 PLC travel/time watchdog 필요 |
| callback/multi-PC | single-owner callback endpoint는 exact TCP peer에 고정했지만 인증·암호화와 multi-PC motion owner는 미정 | typed callback과 multi-PC control은 제외, current single-owner tuple 불변 계약 유지 |
| in-place Distribution build | 후반 검증 실패 전에 기존 DLL/EXE가 덮어써질 수 있음 | 기존 package read-only, staging candidate, semantic preflight, success-only final rename |

## production Definition of Done

아래를 모두 만족하기 전에는 `0.9.1-preview`를 production으로 바꾸지 않는다.

- [ ] current source commit/hash와 배포 DLL provenance 기록
- [x] current SDK Debug/Release 1042/1042, WPF Debug/Release 297/297 PASS; Debug 추가 반복 count 동일
- [x] dormant `0x7D12/0x7D13`을 포함한 `IntegratedReadOwnerDormant` SourceOnly/full static PASS
- [ ] callback+`0x7D12`+`0x7D13` current source LASAL IDE Rebuild/Link와 implementation smoke/log PASS — 세 변경 전 checkpoint는 WARNING 20으로 PASS
- [ ] 다운로드된 PLC source/network/unit/task가 Git snapshot과 일치
- [ ] 실제 장비 안전 chain, limit, UNIT, reference 승인
- [ ] single-axis 1..9와 Cartesian X/Y/Z/U 적용 범위 승인
- [ ] active command별 PLC E2E와 packet/final-state 증거 완료
- [ ] `topology-inventory` durable report와 동일 조건 10,000회 성능 회귀 gate PASS
- [ ] same-peer takeover의 same-IP/other-IP/fault/soak 경계 완료
- [ ] D5 Read는 durable witness 또는 `ApplicationRecoveryOnly` + `orphanQualified=false` 범위 승인
- [ ] callback endpoint ownership source/PC 계약의 current PLC duplicate/mismatch capture 완료;
  typed sender/parser와 multi-PC control은 승인 schema/owner 정책 전까지 명시적 범위 제외
- [ ] M4 advanced 기능과 상위 요구 공백을 구현하거나 항목별 명시적 범위 제외
- [x] historical external `2.0-candidate` DOCX/PDF의 preview/안전/UNIT/polling 및 Axis1 SDO scope 갱신·전 페이지 검토
- [x] 원본 무변경 transaction, success-only rename, failure cleanup과 semantic policy 회귀 PASS
- [x] historical external `2.0-candidate` DOCX/PDF exact bytes를 사용한 실제 sibling Distribution candidate와 semantic policy `15/15` PASS
- [x] historical candidate cleanup, version/input hash/schema 2 manifest, canonical hash와 transaction residue 재확인
- [ ] current exact `2.3-candidate` manual의 tracked-clean full Distribution PASS — `afdf6a3` 재실행은 `-AllowDirty`/`dirty-preview` policy였고 Gate D physical snapshot ratchet에서 STOP, candidate/actual-EXE/manifest/publish 없음
- [ ] `git diff --check`와 `git diff --cached --check` PASS

## 문서 갱신 규칙

- test count는 실제 forced Rebuild 실행값만 갱신한다.
- `source-active`, `PC PASS`, `static PASS`, `IDE PASS`, `PLC PASS`, `hardware PASS`를 별도 열로 유지한다.
- live PASS에는 pcap/QTEST/PLC log와 source/binary identity를 연결한다.
- capability를 활성화할 때 PLC handler, SDK policy, WPF UI, packet map을 같은 변경에서 맞춘다.
- 실패와 미검증 항목을 삭제하지 않고 종료 근거가 생길 때만 상태를 올린다.

## 근거

- [현재 아키텍처 및 릴리스 상태](../architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)
- [요구사항 커버리지](../architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md)
- [Diagnostics 잔여 구현 계획](../architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md)
- [T2 IDE structure handoff](../architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md)
- [Topology qualification tool](../../LMC_Library/LMC_API_Delivery/docs/TOPOLOGY_IO_QUALIFICATION_TOOL_2026-07-28.md)
- [API development backlog](../../LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)
- [Automated tests](../../LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md)

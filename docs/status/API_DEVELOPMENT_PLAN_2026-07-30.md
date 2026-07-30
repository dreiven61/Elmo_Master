# Elmo Master API 개발 계획

- 작성 기준: 2026-07-30 current working tree
- 기준 branch/HEAD: `main@6ce2cb2b9e49647b22c8c99e6c43f9a38a48d00c`
- 현재 릴리스 상태: `0.9.1-preview`, production **NO-GO**
- 진행 현황: [API_DEVELOPMENT_PROGRESS_2026-07-30.md](API_DEVELOPMENT_PROGRESS_2026-07-30.md)
- HTML 계획표: [API_DEVELOPMENT_PLAN_2026-07-30.html](API_DEVELOPMENT_PLAN_2026-07-30.html)

> 이 계획은 새 기능 수를 늘리는 것보다 현재 구현을 재현 가능한 source baseline으로 고정하고,
> LASAL/PLC 증거를 닫는 것을 먼저 둔다. PC/static PASS와 PLC/runtime PASS를 합치지 않는다.

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
| M0. Working-tree baseline 고정 | **부분 완료** | 목적별 local commit, current SDK Debug/Release 975/975, WPF Release 208/208 PASS | clean checkout 재현 |
| M1. LASAL current integration | **차단** | SourceOnly PASS | full static FAIL 해소 + IDE Rebuild/Link |
| M2. PLC read-only/safety baseline | **부분** | 기존 일부 capture, topology static inventory | current cold download + safety/readback 승인 |
| M3. Active motion/diagnostics qualification | **부분** | 대표 happy path PASS | 25-command/fault/soak/recovery matrix |
| M4. Gated advanced diagnostics/I/O | **선택/후속** | C#/WPF scaffold와 dormant source | 이번 release에서 명시 제외 가능; 승인된 기능만 M3 뒤 단계별 live proof |
| M5. Product release | **대기** | `0.9.1-preview` | M3 active scope DoD + M4/상위 공백 명시적 제외 승인 + manual/manifest/provenance |

## 우선순위 요약

| 우선순위 | 범위 | 종료 조건 |
|---|---|---|
| **P0-A** | source freeze와 PC 회귀 안정화 | 동일 hash에서 SDK Debug/Release, WPF build/smoke 전량 PASS |
| **P0-B** | LASAL full 정합과 IDE 적용 | SourceOnly/full PASS, Rebuild/Link, implementation smoke, log clean |
| **P0-C** | current PLC download와 active 범위 qualification | safety readback, 25-command, D1~D5 승인 matrix와 증거 완결 |
| **P0-D** | preview 범위/배포 정리 | advanced/상위 공백의 OFF 범위 승인, external manual, manifest/hash/provenance |
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

### 현재 blocker

- 2026-07-30 current C# source에서 SDK Debug/Release `975/975`, WPF Release
  `208/208`을 전량 재실행해 PASS했다. 이전 174/175-test WPF timing 이력은 최신 208-test
  full run과 관련 targeted run으로 닫았다.
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
& $taskMsBuild $taskWpfSmoke /t:RunWpfSmokeTests /p:Configuration=Release /nologo
& $taskMsBuild $taskWpfSmoke /t:RunWpfSmokeTests /p:Configuration=Release /nologo
& $taskMsBuild $taskPcTests /t:RunLasalContract /p:Configuration=Release /p:LasalTopologyIoCheckpoint=StaticTopologyOnly /nologo
& $taskMsBuild $taskPcTests /t:RunLasalNetworkContract /p:Configuration=Release /p:LasalTopologyIoCheckpoint=StaticTopologyOnly /nologo
$taskAfter = Get-TaskWorkingTreeFingerprint
if ($taskBefore -ne $taskAfter) { throw "Working tree changed during qualification: $taskBefore -> $taskAfter" }
```

## P0-B. LASAL full 정합, IDE build, PLC download

### 작업 순서

1. full static 실패 원인을 먼저 확정한다.
   - verifier hard-coded source-object ordinal/reference: axis 1..9 + Robot 10개가 `TO_UDINT(2)`
   - current generated table의 같은 필드: 10개 모두 `TO_UDINT(1)`
   - connection 방향 필드는 별도 `C_DIR`
   - `.lcn`에는 연결 10개가 존재
   - verifier ordinal 기대와 IDE 생성 ordinal 중 어느 쪽이 실제 LASAL 계약인지 확인한다.
2. generated `.st/.lcb/.lba`를 임의 수동 교정하지 않는다. LASAL IDE의 class/network source를
   확인하고 `Reload Class -> Save/Generate`로 재생성한다.
3. same-peer `TCPIPServer`, `TCPMotionInterface`, `LMCControlCommandService`,
   `LMCDiagnosticsService`, `LMCRecorderStore`, `LMCSdoExecutor`, topology/network를 포함해
   Rebuild/Link한다.
4. `LMCSdoExecutor`는 D5 Write 활성 여부와 무관한 current integration gate로 취급한다.
   constructor declaration, generated `@STD` binding, state/buffer 초기화, 최초 `Idle` publish를
   IDE/source에서 확인한다.
5. 변경 class의 앞/중간/뒤에서 `Find in Implementation` smoke를 수행한다.
6. smoke 시작 시각 이후 `%TEMP%\Lasal2.log`에 새 `CInvalidArgException`이 0개인지 확인한다.
7. SourceOnly와 full/network static을 같은 source에서 다시 실행한다.
8. PLC cold download 후 project/source/network/unit/task와 BootId/MapRevision을 기록한다.

### 정적 계약 개별 진단 명령

아래는 P0-B만 다시 볼 때 쓰는 명령이다. 최종 qualification은 P0-A 일괄 block처럼 PC/WPF와
정적 계약을 하나의 fingerprint 전후 범위에서 실행한다.

```powershell
$taskMsBuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe'
$taskPcTests = '.\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj'

& $taskMsBuild $taskPcTests /t:RunLasalContract `
  /p:Configuration=Release `
  /p:LasalTopologyIoCheckpoint=StaticTopologyOnly /nologo

& $taskMsBuild $taskPcTests /t:RunLasalNetworkContract `
  /p:Configuration=Release `
  /p:LasalTopologyIoCheckpoint=StaticTopologyOnly /nologo
```

### 완료 조건

- `Phase5TransportClean / StaticTopologyOnly` SourceOnly와 full 모두 PASS
- LASAL Compiler/Linker ERROR/FATAL 0
- `LMCSdoExecutor` constructor/binding/init/`Idle` publish 계약 확인
- 변경 class implementation smoke PASS
- smoke 이후 신규 `CInvalidArgException` 0
- current PLC download 및 source/network/unit/task provenance 보존
- same-peer takeover가 master project에서 정상이고 다른-IP/fault/soak 경계가 기록됨

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
5. axis별 PowerOn -> stable status -> 작은 Move -> final position -> Stop -> stable standstill -> PowerOff
6. group PowerOn -> power poll -> SetKin -> Lock -> Move -> final state -> Unlock -> PowerOff
7. motion/group 25-command success/expected-failure matrix
8. D1 Catalog/Health/PI fault·stale matrix
9. D2 exact 24-entry lifecycle, 100회, one-slave-offline partial/recovery
10. D3/D4 Single/Ring/trigger/reconnect-adopt/hash/soak
11. D5 Read offline/abort/contention/timeout/drain/queued-cancel/disconnect/orphan/late callback
12. negative/malformed wire와 reconnect/fault recovery

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
- Power/Enable/Stop/Move ACK와 최종 상태가 구분됨
- expected failure에서 mutation 0 또는 승인된 recovery가 입증됨
- fault/reconnect 뒤 stale descriptor/session을 재사용하지 않음
- 시험 binary/source hash와 PLC BootId/MapRevision이 report에 기록됨
- `topology-inventory` durable report와 성능 회귀 gate PASS

## P0-D. Release scope와 배포

### 먼저 결정할 범위

| 기능 | 현재 상태 | 이번 release 권장안 | 활성 조건 |
|---|---|---|---|
| D4 Double bank | source/PC contract, gate OFF | preview에서 제외 유지 | 2.56 MB RAM, RT jitter, A/B 동시, reconnect/release live PASS |
| D5 SDO Write | exact Int32 scaffold, gate/allowlist OFF | 제외 유지 | 대상/축 승인, constructor, single-writer, four-ticket/readback/physical proof |
| D5 Read orphan recovery | `ApplicationRecoveryOnly` 가능, durable orphan witness 없음 | `orphanQualified=false`, fail-closed와 수동 recovery로 제한 | disconnect/orphan/late-callback 무해성 + 수동 recovery 증거; durable witness는 P1 |
| PI Write | C# scaffold, PLC Unsupported | 제외 유지 | semantic allowlist, handler, fault/physical proof |
| Dynamic Health/DI | C#/WPF 구현, PLC route 없음 | P1 read-only | `0x7E13/22`, bits 15/16, coherent snapshot, physical correlation |
| Digital Output | C#/WPF guard, PLC route 없음 | P1 read-only 이후 | `0x7E23`, bit 17, RT single owner/CAS/readback/fault proof |
| typed callback | raw UDP listener만 존재 | 구현 또는 명시적 제외 결정 | 실제 payload/schema와 PLC event sender |
| 상위 요구 공백 | 요구사항 명칭 `HomeDS402/HomeDS402Ex`, `SetPosition`, `SetOpMode` 미구현 | production 전 명시적 제외 승인 필수 | 별도 승인 시 `ReferenceAxis/HomeUsingLasalReference` 등 command DoD로 구현 |

### 배포 작업

1. release scope와 제외 기능을 capability/SDK/WPF/manual에서 일치시킨다.
2. external DOCX/PDF에 preview, 안전, UNIT, polling, Close/Cancel != Stop을 반영한다.
3. current source에서 Distribution을 재조립한다.
4. `RELEASE_MANIFEST.md`에 source commit/hash, dirty 여부, DLL version과 모든 파일 SHA-256을 기록한다.
5. 임시 `bin/obj`, Reports, captures를 배포 폴더에서 제외한다.
6. final package를 clean checkout에서 재검증한다.

## P1. Dynamic CREVIS와 advanced diagnostics

### P1-1. read-only topology/health/DI

1. LASAL IDE에 Coupler/InputSlot/OutputSlot declaration과 network를 생성한다.
2. `LasalTopologyIoCheckpoint=IdeStructureReady` static checkpoint를 통과한다.
3. 464-byte coherent snapshot owner와 `0x7E13` Node Health, `0x7E22` Digital I/O Read를 구현한다.
4. bits 15/16을 OFF로 유지한 채 `IntegratedReadOwnerDormant` checkpoint를 통과한다.
5. `topology-io-qualify --scope integrated-read-owner-dormant --execute-live
   --confirm PLC-RAW-TOPOLOGY-IO-READ ...` raw qualifier의 durable report를 보존한다.
6. GL/Elmo disconnect/recovery와 32-pattern DI physical correlation을 통과한다.
7. 위 증거가 끝난 뒤에만 bits 15/16을 활성화한다.

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
- D5 Write: 사용자 승인 target/axis, 안전 state, baseline/pre-write guard/final safety/Write/readback
  네 ticket과 pcap/physical evidence 후 활성
- PI Write: semantic allowlist, PLC handler, fault/physical proof를 갖춘 별도 승인 기능으로만 활성
- D5 Read orphan: durable orphan witness를 추가하지 않으면 `ApplicationRecoveryOnly`,
  `orphanQualified=false`, disconnect fail-closed 범위를 유지한다.

## P2. 신규 API와 제품화

요구사항 상위 21개 중 실제 공백 4개를 먼저 검토한다. 아래 항목을 이번 release에서 미룬다면
production 승인 기록에 각각 제외 사유와 영향 범위를 남긴다.

1. `ReferenceAxis` / `HomeUsingLasalReference`
   - 요구사항 표의 `HomeDS402`/`HomeDS402Ex`에 대응하는 LASAL-native reference adapter다.
     full DS402 homing-method 지원을 입증하기 전에는 `HomeDS402Ex`라는 구현 명칭을 쓰지 않는다.
   - RefSwitch/HWMin/HWMax/LatchPos 배선과 ownership부터 확정한다.
2. `SetPosition`
   - zero velocity, safe axis state, jump/limit, explicit execute 계약 필요
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
| SDO/DO mutation | 재전송 시 장비 상태 불명확 | single writer, no replay, durable journal, physical verification |
| callback/multi-PC | ownership/보안 불명확 | 이번 release 제외 또는 명시 protocol/owner 정책 결정 |

## production Definition of Done

아래를 모두 만족하기 전에는 `0.9.1-preview`를 production으로 바꾸지 않는다.

- [ ] current source commit/hash와 배포 DLL provenance 기록
- [ ] SDK Debug/Release, WPF build/smoke, SourceOnly/full static 전량 PASS
- [ ] LASAL IDE Rebuild/Link와 implementation smoke PASS
- [ ] 다운로드된 PLC source/network/unit/task가 Git snapshot과 일치
- [ ] 실제 장비 안전 chain, limit, UNIT, reference 승인
- [ ] single-axis 1..9와 Cartesian X/Y/Z/U 적용 범위 승인
- [ ] active command별 PLC E2E와 packet/final-state 증거 완료
- [ ] `topology-inventory` durable report와 동일 조건 10,000회 성능 회귀 gate PASS
- [ ] same-peer takeover의 same-IP/other-IP/fault/soak 경계 완료
- [ ] D5 Read는 durable witness 또는 `ApplicationRecoveryOnly` + `orphanQualified=false` 범위 승인
- [ ] M4 advanced 기능, 상위 요구 공백, callback/ownership을 구현하거나 항목별 명시적 범위 제외
- [ ] external user manual의 preview/안전/UNIT/polling 경고 갱신
- [ ] Distribution cleanup, version/hash/manifest 재확인
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

# 개발 상태 스냅샷 — 2026-08-31

- current integration / qualification source: `dev`
- manual/progress sync commit: `b08cd07` (`docs(api): sync manual and development progress`)
- source baseline analyzed for current live blocker: `dev@db954731c27c30f43f706b101276b81b022bd60a`
- remote branches: `main`, `dev` only
- open PR: none
- production release posture: **NO-GO**
- active P0 tracking: issue #46

이 문서는 2026-08-31 현재의 실행/검증 상태를 요약한다. source 구현, PC regression, LASAL C78 artifact, PLC load, 실제 wire mutation, physical effect와 production release를 서로 다른 gate로 판정한다.

---

## 1. 이번 정리에서 완료된 것

### Repository / branch

- 과거 `codex/*` 작업 branch 29개를 `dev` ancestry 기준으로 확인했다.
- 29개 모두 `dev`에 이미 포함된 ancestor임을 확인한 뒤 삭제했다.
- 현재 remote branch는 `main`, `dev` 두 개만 유지한다.
- stale qualification PR/branch를 current source truth로 사용하지 않는다.
- 앞으로 작업 branch가 필요하면 작업 -> 검증 -> `dev` merge -> 즉시 삭제를 기본으로 한다.

### SetOperationMode / Generic SDO corrective

PR #58에서 다음 corrective source가 `dev`에 통합됐다.

- SetOperationMode CSP same-target `SucceededNoWrite`와 real cross-mode 구분
- PP/PV/IP/CSP fresh drive-status preflight
- ordinary Generic SDO Write가 qualification-only PowerOff 조건을 재사용하던 문제 분리
- Generic SDO PLC safe-state를 DS402 base `0x40`, `0x21`, `0x23`으로 명시
- OperationEnabled/Fault/semantic raw-object 차단 유지
- WPF SetOperationMode Start gate를 actual live gate 값으로 표시
- Generic SDO R03/R04/R05 current 상태와 durable no-replay policy 정렬

PR #58 software evidence:

- API Debug full suite: **1200/1200 PASS**
- Generic SDO WPF focused smoke: **17/17 PASS**
- API Debug/Release build: PASS
- WPF Debug/Release build: PASS
- corrective/static verifier: PASS
- Generic SDO policy verifier: PASS

이 결과는 physical mode change 또는 SDO Write 성공 증거가 아니다.

### Documentation

2026-08-31에 다음 current 문서를 source truth에 맞게 동기화했다.

- `docs/api/API_MANUAL.md` -> **2.5-development**
- `docs/api/API_MANUAL.html` 재생성
- `docs/api/API_DEVELOPMENT_PROGRESS.md` -> **1.3-current**
- `docs/api/API_DEVELOPMENT_PROGRESS.html` 재생성
- `docs/api/design/SET_OPERATION_MODE_IMPLEMENTATION_UPDATE_20260828.md`의 17:28 live blocker 분석 유지
- 이 문서 `DEVELOPMENT_STATUS_20260831.md`를 current snapshot으로 추가

과거 manual의 `SetOperationMode CSP-only`, `bits 8/9/10 OFF`, `Axis1 UI24가 Generic SDO의 유일 target` 설명은 current source 판정에서 제거했다.

---

## 2. SetOperationMode — current source truth

### Activation / supported modes

current `dev` source:

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
Admin feature mask = 0x00000717
SetOperationModeSupportedMask = 0x018A
```

`0x018A` target:

- PP = 1
- PV = 3
- IP = 7
- CSP = 8

Homing(6)은 SetOperationMode가 소유하지 않는다.

### Safety / lifecycle

real cross-mode 후보는 Start 전에 다음 fresh observation을 읽는다.

- LASAL AxisStatus
- DS402 `0x6041`
- DS402 `0x6061`

cross-mode admission:

- Standstill=True
- DS402 Fault=False
- DS402 OperationEnabled=False

same-target는 `SucceededNoWrite`가 가능하다. 특히 CSP->CSP 성공은 실제 `0x6060` dispatch 증거가 아니다.

mutation lifecycle:

```text
preflight 0x6061
-> one-time 0x6060 requested mode Write
-> 0x6061 verify
-> terminal outcome
-> exact-generation retire
```

원 Start/`0x6060` Write는 uncertain outcome에서 자동 replay하지 않는다.

---

## 3. 2026-08-28 17:28 live finding — 현재 P0 blocker

live executable identity:

```text
LasalMotionControlApiExample.exe Version=0.9.1.0
BuildUtc=2026-08-28 08:27:44 UTC
Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5
ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2
LasalMotionControlLib.dll BuildUtc=2026-08-28 08:27:41 UTC
BootId observed in callback log = 0x00000062
```

Axis1 current CSP(8)에서:

```text
requested PV(3) -> cross-mode preflight PASS, StatusWord=0x02D0
requested PP(1) -> cross-mode preflight PASS, StatusWord=0x02D0
requested IP(7) -> cross-mode preflight PASS, StatusWord=0x02D0
requested CSP(8) -> same-target no-write candidate
```

하지만 모든 시도가 다음 예외에서 종료됐다.

```text
The supplied diagnostics capabilities are not the current observation.
```

### Root cause

현재 WPF ordering:

```text
Admin.GetCapabilitiesAsync
-> RefreshDiagnosticsCapabilitiesAsync              // observation N
-> GetPhysicalAxisAsync
-> ReadDriveStatusAsync
   -> inline D5 0x6041
      -> Diagnostics.GetCapabilities                // N+1
   -> inline D5 0x6061
      -> Diagnostics.GetCapabilities                // N+2
-> PrepareSetOperationMode(cached observation N)
-> requireCurrentObservation=true
-> stale capability reject
```

즉 fresh preflight 자체가 Diagnostics capability observation sequence를 진행시켜 직전에 캐시한 capability를 stale로 만든다.

### 이번 로그가 증명하는 것 / 증명하지 않는 것

증명:

- PP/PV/IP가 software selector/mask에서 선택 가능
- `ReadDriveStatusAsync()`가 current CSP(8), StatusWord=0x02D0을 읽음
- cross-mode safety preflight가 통과함
- host capability freshness validation이 mutation 전에 차단함

증명하지 않음:

- PLC가 PP/PV/IP를 reject했다는 것
- `0x7D23`이 전송됐다는 것
- `0x6060` Write가 전송됐다는 것
- mode change가 성공/실패했다는 것

실패 위치는 `PrepareSetOperationMode()` 이전/내부 validation이므로 이 재현의 mutation wire count는 **0**으로 판정한다.

`D5 terminal wake ignored: no exact current retained ticket`는 preflight inline D5 read activity와 시간적으로 대응하며, 현재 primary blocker는 아니다.

---

## 4. SetOperationMode corrective design — 다음 구현

freshness fence를 제거하지 않는다. 실행 순서를 다음으로 바꾼다.

```text
1. Admin capability refresh / selected mode advertise 확인
2. GetPhysicalAxis
3. ReadDriveStatusAsync fresh preflight
4. FINAL Diagnostics capability refresh
5. Ensure capability/admission ready
6. PrepareSetOperationMode
7. durable ArmBeforeDispatch
8. Start exactly once
```

유지할 계약:

- `requireCurrentObservation=true`
- stable DiagnosticsBuild/BootId/MapRevision
- Standstill/Fault/OperationEnabled safety fence
- one-shot operator confirmation
- durable pre-dispatch journal
- no-replay invariant
- raw Generic SDO `0x6060` block

필수 regression:

- preflight 이전 old observation은 stale reject돼야 함
- preflight 이후 final refresh한 observation으로 Prepare 성공해야 함
- final refresh와 Prepare 사이에 capability-producing call이 없어야 함
- Prepare 성공 전 mutation/journal Start wire는 0회여야 함

---

## 5. Generic SDO — current state

current source:

```text
LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE
LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE
```

완료 software tranche:

- SDO-R02 dual-entry executor
- SDO-R03 generic physical axis 1..4 scalar Write, canonical 1/2/4-byte width
- SDO-R04 arbitrary request editor + exact preview + reserved/semantic warning
- SDO-R05 durable exact-request identity + restart read-only recovery
- PR #58 ordinary Write safe-state correction

ordinary Generic SDO Write safety:

```text
Standstill=True
DS402 Fault=False
DS402 OperationEnabled=False
```

PLC generic safe base states:

- `0x40` Switch On Disabled
- `0x21` Ready To Switch On
- `0x23` Switched On

계속 차단:

```text
0x6040
0x6060
0x607A
0x60FF
0x6071
0x3204
0x20FC
```

Axis1 UI24 same-value four-ticket path는 특정 qualification preset으로 유지하지만 Generic SDO API 전체의 유일 허용 target은 아니다.

현재 남은 gate는 실제 safe non-semantic 1/2/4-byte Write + exact readback hardware evidence다.

---

## 6. 다른 주요 기능 상태

| 영역 | current 판정 | blocker / 다음 gate |
|---|---|---|
| HomeDS402 | source/WPF qualification 통합, activation OFF | fresh C78/generated artifact + hardware matrix |
| HomeDS402Ex | SDK/ownership/retained store/WPF recovery 존재, physical runtime OFF | approved hardware profile + artifact closure |
| SetPosition | lifecycle/WPF recovery/host receipt tooling 존재, runtime fail-closed | issue #44 vendor CRC golden fixture + IDE-generated `_FileSys` ABI |
| PI Write | dormant | capability/allowlist OFF |
| Digital Output Write | missing runtime route | handler/owner/allowlist 필요 |
| Recorder Double | dormant | capability/route gate OFF |

SetPosition 외부 blocker를 추측으로 우회하지 않는다.

---

## 7. Repository 운영 원칙

현재 remote branch는 다음 두 개뿐이다.

```text
main
dev
```

`dev`가 유일한 current integration/qualification source truth다.

source CI PASS, C78 compile, PLC load, physical wire/effect를 같은 PASS로 합치지 않는다. 실기 결과에는 가능한 한 다음 identity를 같이 남긴다.

- source commit SHA
- WPF EXE version/build time
- SDK DLL build time
- DiagnosticsBuild
- BootId
- MapRevision
- loaded PLC artifact identity

---

## 8. 현재 우선순위

1. SetOperationMode capability freshness ordering 수정
2. stale-old/current-final capability regression 추가
3. API/WPF Debug/Release validation
4. Axis1 PP/PV/IP/CSP physical matrix
5. Axis1 Generic SDO safe non-semantic 1/2/4-byte Write/readback matrix
6. timeout/disconnect/response-loss/no-replay recovery matrix
7. Axis2..4 확대
8. HomeDS402 / HomeDS402Ex / SetPosition blocker 순차 처리

production release는 위 physical qualification이 끝나기 전까지 **NO-GO**다.

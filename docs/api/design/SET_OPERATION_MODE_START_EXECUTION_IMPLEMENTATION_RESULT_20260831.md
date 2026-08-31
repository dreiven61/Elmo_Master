# SetOperationMode Start 실행 경로 구현 결과 — 2026-08-31

> 상태: **SOFTWARE IMPLEMENTED / PHYSICAL QUALIFICATION OPEN**
>
> 설계 기준: `SET_OPERATION_MODE_START_EXECUTION_REFACTOR_PLAN_20260831.md`
>
> functional implementation commit: `d4ce1b2f9c2a41f5117e0bd769533d0483c1ff91`
>
> 추적: issue #46
>
> production release posture: **NO-GO**

---

## 1. 구현 결과

2026-08-31 설계의 두 software blocker를 current `dev`에서 수정했다.

### A. Start Click handler ownership — CLOSED in software

기존 WPF는 button 생성 시 `ButtonStartAxisSetOperationMode_Click()`을 등록한 뒤 `InitializeReadOnlyApiUi()`에서 이를 detach하고 `ButtonStartAxisSetOperationModeWithRejectResolution_Click()`으로 다시 연결했다.

현재 구현은 다음 하나의 event path만 유지한다.

```text
Start Selected Mode Once button
-> ButtonStartAxisSetOperationMode_Click
-> RunOperationAsync
-> StartAxisSetOperationModeOnceAsync
```

`MainWindow.ReadOnlyApi.cs`에서는 SetOperationMode Start handler를 detach/rebind하지 않는다. `ButtonStartAxisSetOperationModeWithRejectResolution_Click()`은 제거했다.

기존 definitive Start rejection archival/active-journal clear/UI update 동작은 삭제하지 않고 canonical `ButtonStartAxisSetOperationMode_Click()`의 `LMCAxisSetOperationModeRejectedException` 처리로 이동했다.

### B. Diagnostics capability freshness ordering — CLOSED in software

기존 실패 순서:

```text
Diagnostics observation N
-> ReadDriveStatus preflight
   -> inline D5 0x6041 / 0x6061
   -> observation sequence advances
-> PrepareSetOperationMode(cached N)
-> stale current-observation reject
```

현재 구현 순서:

```text
Admin capability / selected-mode check
-> GetPhysicalAxis
-> ReadDriveStatus preflight
-> FINAL RefreshDiagnosticsCapabilitiesAsync
-> EnsureAxisSetOperationModeCapabilitiesReady
-> local finalDiagnosticCapabilities
-> PrepareSetOperationMode(finalDiagnosticCapabilities)
-> durable ArmBeforeDispatch
-> SetOperationModeAsync exactly once
-> accepted: read-only outcome recovery
```

FINAL Diagnostics refresh와 `PrepareSetOperationMode()` 사이에는 drive-status read 또는 별도 `GetCapabilities*()` 호출을 넣지 않는다. `requireCurrentObservation=true` freshness fence는 유지한다.

---

## 2. 유지된 safety contract

이번 corrective에서 다음을 완화하지 않았다.

- Diagnostics current-observation freshness validation
- Diagnostics Build / BootId / MapRevision identity
- Admin capability triad
- PLC-advertised supported mode mask
- explicit one-shot confirmation
- physical axis 1..4 validation
- cross-mode `Standstill=True`
- DS402 Fault=False
- DS402 OperationEnabled=False
- durable journal pre-dispatch arm
- Start maximum one dispatch
- accepted/uncertain outcome에서 original Start automatic replay 금지
- Generic SDO raw `0x6060` mutation 금지

PLC SetOperationMode lifecycle/safety logic은 이번 tranche에서 변경하지 않았다.

---

## 3. 추가 regression guard

### WPF smoke

`Wpf.SetOperationModeRecovery.CanonicalStartClickUsesSingleHandler`를 추가했다.

검증:

```text
handler entry count before click = 0
button Click one time
handler entry count after click = 1
```

따라서 canonical handler가 다시 dead handler가 되거나 이중 event subscription이 생기는 회귀를 검출한다.

### permanent source verifier

`tools/Verify-SetOperationModeStartExecution.ps1`을 추가했다.

검증 항목:

- canonical Start event subscription 정확히 1개
- obsolete `ButtonStartAxisSetOperationModeWithRejectResolution_Click` 없음
- `MainWindow.ReadOnlyApi.cs`의 Start detach 없음
- definitive rejection resolution 보존
- `preflight -> FINAL Diagnostics -> Prepare -> Arm -> SetOperationModeAsync` 순서
- FINAL Diagnostics와 Prepare 사이 capability-producing/read helper 없음
- Prepare가 `finalDiagnosticCapabilities`를 사용
- Start core 내부 `SetOperationModeAsync()` 정확히 1개
- phase log 존재

---

## 4. software qualification evidence

최종 promotion run에서 다음을 모두 통과했다.

```text
API Debug full tests                  1200 / 1200 PASS
WPF SetOperationModeRecovery Debug       7 / 7 PASS
WPF AxisSetOperationModeJournal Debug    7 / 7 PASS
WPF SetOperationModeSdk Debug            1 / 1 PASS
Generic SDO Wpf.Sdo Debug               17 / 17 PASS
API Release build                       PASS
WPF Release build                       PASS
WPF SetOperationModeRecovery Release     7 / 7 PASS
WPF AxisSetOperationModeJournal Release  7 / 7 PASS
WPF SetOperationModeSdk Release          1 / 1 PASS
git diff --check                        PASS
Start execution source verifier         PASS
```

초기 broad `--filter SetOperationMode` 실행에서 별도 HomeDS402Ex Korean localization baseline 1건이 같이 선택되어 실패한 이력이 있으나, 새 canonical Start test와 SetOperationMode lifecycle/recovery tests는 해당 run에서도 PASS였다. 최종 qualification은 SetOperationMode 소유 테스트 그룹을 명시적으로 분리해 수행했다.

---

## 5. 현재 디버깅 기준 경로

실기에서 `Start Selected Mode Once`를 클릭할 때 다음 breakpoint 순서로 확인한다.

```text
ButtonStartAxisSetOperationMode_Click
-> StartAxisSetOperationModeOnceAsync
-> VerifyAxisSetOperationModeTransitionPreflightAsync
-> FINAL RefreshDiagnosticsCapabilitiesAsync
-> PrepareSetOperationMode
-> LMCSingleAxis.SetOperationModeAsync
-> LMCAdmin.StartAxisSetOperationModeAsync
-> transport mutation boundary
```

정상적으로 Start 단계에 진입하면 최소 다음 phase log가 순서대로 관측돼야 한다.

```text
SetOperationMode Start UI handler entered.
SetOperationMode preflight ...
SetOperationMode final Diagnostics refreshed: ...
SetOperationMode prepared: ...
SetOperationMode journal armed before dispatch: ...
SetOperationMode 0x7D23 dispatch boundary crossed once: ...
```

`0x7D23 dispatch boundary crossed once` 로그는 accepted RPC return 이후 기록되는 host-side evidence이며 mode-change 완료 자체의 증거는 아니다. 완료 판정은 retained outcome 및 최종 `0x6061` observation으로 한다.

---

## 6. 아직 OPEN인 physical qualification

software implementation 완료를 physical PASS로 해석하지 않는다.

Axis1에서 exact updated source / PLC image / WPF executable identity를 맞춘 후 다음 matrix가 필요하다.

| current | requested | 기대 physical evidence |
|---|---|---|
| CSP | CSP | `SucceededNoWrite` 가능, `0x6060` dispatch 0회 |
| CSP | PP | `0x6060=1` 최대 1회, 최종 `0x6061=1` |
| CSP | PV | `0x6060=3` 최대 1회, 최종 `0x6061=3` |
| CSP | IP | `0x6060=7` 최대 1회, 최종 `0x6061=7` |
| PP/PV/IP | CSP | `0x6060=8` 최대 1회, 최종 `0x6061=8` |

이후 failure/recovery matrix와 Axis2..4 확대가 남아 있다.

---

## 7. repository 상태

구현은 별도 장기 작업 branch 없이 `dev`에서 수행했다. 임시 promotion workflow와 적용 helper는 qualification 및 functional commit 이후 삭제했다. 영구 보존 대상은 functional source, WPF regression 및 `Verify-SetOperationModeStartExecution.ps1`이다.

현재 다음 작업은 새로운 software scaffold 추가가 아니라 exact updated build를 사용한 Axis1 physical mode-change matrix다.

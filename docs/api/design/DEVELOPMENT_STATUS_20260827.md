# 개발 상태 스냅샷 — 2026-08-27

- current production branch: `dev`
- current production HEAD: `cd89d189a3dd574c1fc1147eba07dff88effc54a`
- P0 redesign audit branch: `codex/sdo-mode-redesign-docs-20260827@4909200ba45e9e5d4f87334e92f6190599f471e2`
- production 판정: **NO-GO**

설계/source/static, generated artifact, C78, PLC load, hardware/packet, production activation은 서로 다른 gate다. Branch-local 구현을 `dev` current completion으로 승격하지 않는다.

---

## 1. 최우선 API current 상태

| API | release-oriented 진행도 | current `dev` 상태 | 다음 핵심 gate |
|---|---:|---|---|
| HomeDS402 | 50% | H37-01/02/03/04/10 software/current-dev qualification 완료, activation OFF | fresh C78/artifact H37-05/06 -> Axis1 H37-07 -> Axis2..4 -> activation |
| SetOperationMode | 60% | lifecycle/owner/no-replay/MODE-10/MODE-13 존재, production gate FALSE, bits 8/9/10 OFF | redesign MODE-R01 SupportedModeMask -> MODE-R02 physical matrix |
| HomeDS402Ex | 40% | SDK/retained store/ownership/WPF recovery 존재, physical runtime OFF, bit 11 OFF | issue #28 profile + issue #35 artifact -> actual runtime/hardware |
| SetPosition | 25% | P1 lifecycle + WPF durable recovery current-dev 통합, runtime activation OFF | durable A/B store + RT/native exactly-once |

진행률은 checklist 단순 비율이 아니다. 특히 이번 Generic SDO/SetOperationMode branch-local 작업만으로 SetOperationMode release 진행률을 올리지 않는다.

---

## 2. 2026-08-27 사용자 요청 3건 audit

| 요청 | 판정 | 근거 |
|---|---|---|
| SetOperationMode PP/PV/IP/CSP 실제 전환 | **부분 구현 / 미완료** | qualification branch에는 multi-mode request/UI가 있으나 bench definitive reject. redesign/current dev에 PLC SupportedModeMask 없음. actual mode-change PASS 없음 |
| Generic SDO arbitrary Write | **미구현** | arbitrary request model test는 추가됐으나 `AllowedSdoWrites` / `RequireSdoWriteAllowed()`가 여전히 Axis1 `0x2F00:24` exact target만 허용 |
| `LMCSdoExecutor` manual Server Read/Write | **source 구현 완료 / bench 미완료** | dual-entry source + C78 build + PLC basic smoke 존재. manual physical Read/Write/arbitration/D5 regression은 미완료 |

따라서 issue #46의 세 요구는 아직 전체 완료가 아니다.

---

## 3. branch-local SDO-R02 현재 evidence

PR #47 branch에는 `LMCSdoExecutor` dual-entry 구현이 존재한다.

구현됨:

- `RequestSource` NONE/MANUAL_SERVER/PROGRAMMATIC
- manual `ParaReadWrite::Write` Read/Write dispatch
- `ParaType`/`ParaString` base-compatible behavior
- manual/programmatic shared arbitration
- callback source dispatch
- manual `ClassState`/`ErrorCode`/`ParaLength` publish
- tokenized programmatic path 유지
- focused dual-entry source verifier

현재 build/runtime evidence:

- C78/ARM Rebuild/Link: **0 errors, 101 warnings**
- generated `Classes.lcb` 재생성: 확인
- PLC download: 사용자 확인 PASS
- PLC basic normal run: 사용자 확인 PASS

아직 닫히지 않은 SDO-R02 gate:

- Axis1..4 executor network direct-open
- Axis1..4 manual `0x6061:0` Read
- safe object manual Write + exact readback
- manual/programmatic BUSY/no-wire contention
- opposite-entry reuse after completion
- late/source mismatch quarantine
- programmatic D5 regression
- same-image generated artifact physical identity closure

상세 evidence: `docs/api/design/evidence/SDO_R02_C78_DOWNLOAD_SMOKE_20260827.md`

---

## 4. Generic SDO Write current blocker

`LmcDiagnosticsD5Models.cs`에는 현재도 다음 exact allowlist가 살아 있다.

```text
AllowedSdoWrites
RequireSdoWriteAllowed()
CreateAllowedSdoWriteTargets()
```

현재 effective target은 Axis1 `0x2F00:24 Int32/4` 한 개다.

따라서 다음은 서로 구분한다.

```text
arbitrary request model construction -> 가능
arbitrary request submission policy   -> 아직 불가
```

다음 작업은 SDO-R03이다.

1. exact target allowlist를 generic admission에서 제거
2. capability/request validity/owner 기반 policy로 교체
3. 1/2/4-byte arbitrary Write tests
4. semantic reserved object policy
5. `0x2F00:24`를 preset으로만 유지

그 뒤 SDO-R04 WPF editor, SDO-R05 durable recovery를 진행한다.

---

## 5. SetOperationMode redesign current blocker

별도 MODE-11 qualification branch에는 PP(1)/PV(3)/IP(7)/CSP(8) request support가 있다. 그러나 그 branch는 `DO NOT MERGE`이고 이전 bench에서 Start가 definitive reject됐다.

현재 필요한 MODE-R01:

- PLC `SupportedModeMask`
- SDK typed capability model
- WPF selector = SDK-known ∩ PLC-supported
- stale/no-mask fail-closed
- rejection log에 ErrorId/DetailCode/RequestedMode/current mode/Build/BootId/MapRevision/Admin mask/SupportedModeMask 표시

MODE-R02는 새 same-image C78/PLC에서 다음 Axis1 matrix를 실행한다.

```text
CSP -> PP -> CSP
CSP -> PV -> CSP
CSP -> IP -> CSP
CSP -> CSP same-mode
```

Axis1 PASS 후 Axis2..4로 확대한다.

---

## 6. PR #47 current qualification 상태

Latest audited head `4909200...`:

기능 측 focused checks:

- SetOperationMode source invariants: **57/57 PASS**
- SetOperationMode define order: **PASS**
- SetOperationMode WPF Debug recovery smoke: **PASS**
- SetOperationMode WPF Release recovery smoke: **PASS**
- SetOperationMode C78 evidence collector self-test: **PASS**

그러나 PR 전체는 아직 merge-ready가 아니다.

확인된 latest-head failure 중 focused SetOperationMode workflows는 diff hygiene에서 실패했다.

- architecture 문서 trailing whitespace
- SDO-R02 evidence 문서 EOF extra blank line

이번 문서 sync에서 두 hygiene 문제를 제거한다. 다른 전체-repository workflow는 최신 head 재실행 결과를 별도로 확인해야 한다.

---

## 7. 다른 API production boundaries

### HomeDS402

current dev 완료:

- H37-02 five-gate static contract
- H37-03 lifecycle PC contract
- H37-04 ownership/preemption
- H37-10 WPF durable no-replay

남음:

- H37-05/06 fresh generated artifact/C78
- H37-07 Axis1 matrix
- H37-08 Axis2..4
- H37-09 activation

Admin bit 6과 다섯 activation value는 OFF.

### HomeDS402Ex

current dev software:

- HOMEEX-03/04 SDK lifecycle
- HOMEEX-05 retained outcome store
- HOMEEX-06 parser/scaffold
- HOMEEX-07 full owner identity
- HOMEEX-08 approved-plan preparation
- HOMEEX-09 source/static collector
- HOMEEX-12 WPF durable recovery

남음:

- issue #28 hardware profile approval
- issue #35 fresh artifact/C78 closure
- actual parameter/mode/controlword/homing runtime
- HOMEEX-10/11 hardware
- HOMEEX-13 activation

`LMC_DIAG_DS402_HOME_EX_ENABLED=FALSE`, Admin bit 11 OFF.

### SetPosition

current dev에는 SP-04A WPF durable no-replay recovery가 통합됐다.

남음:

- durable `_FileSys` A/B backend
- vendor CRC / IDE-generated ABI prerequisites
- RT exactly-once/native execution
- C78/PLC/hardware
- capability activation

---

## 8. P0 다음 작업 순서

현재 최우선 순서는 issue #46 기준이다.

```text
1. PR #47 hygiene/CI 정리
2. SDO-R02 manual Server bench qualification 완료
3. SDO-R03 Generic Write policy 일반화
4. SDO-R04 WPF arbitrary-target editor
5. SDO-R05 exact-request durable recovery
6. MODE-R01 SupportedModeMask + rejection diagnostics
7. MODE-R02 Axis1 multi-mode physical matrix
8. Axis2..4 확대
9. REL-R01 distribution/docs/artifact sync
10. production activation 별도 review
```

---

## 9. activation rule

어떤 API도 다음 항목이 같은 승인 세트에서 연결되지 않으면 Active로 승격하지 않는다.

1. exact source commit
2. generated artifact identity
3. fresh C78/ARM build/link
4. exact PLC load identity
5. Build/BootId/MapRevision
6. PC/source/static tests
7. hardware/packet normal + negative matrix
8. durable no-replay evidence
9. paired capability/UI/manual update

Branch-local source 구현이나 user-reported basic PLC smoke만으로 production capability를 켜지 않는다.

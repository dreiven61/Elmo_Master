# 개발 상태 스냅샷 — 2026-08-28 / 2026-08-31 update

- current integration / qualification source: `dev`
- current qualification posture: **SetOperationMode + Generic SDO gates are ON in source; hardware PASS is NOT established**
- SetOperationMode latest physical identity: `Build=1 / BootId=0x00000068 / MapRevision=0x957F101E`
- production release posture: **NO-GO**
- active P0 tracking: issue #46

이 문서는 current `dev`의 실제 실행 경로와 physical evidence를 기준으로 상태를 판정한다. source/PC test, generated LASAL artifact, PLC load, physical wire/effect, production release는 각각 별도 gate다.

---

## 1. 현재 P0 요약

| 영역 | current 구현 상태 | 현재 gate 상태 | 다음 gate |
|---|---|---|---|
| SetOperationMode | PP/PV/IP/CSP lifecycle, mode mask, final Diagnostics refresh, durable no-replay, cross-mode preflight 통합 | **PLC Start admission BLOCKED: Detail 49 at BootId 0x68** | Detail 49 내부 원인 observability 분리 설계 후 원인별 수정 |
| Generic SDO | dual-entry executor + generic scalar Write + editor/preview + durable recovery + safe-state correction 통합 | **global Write ON / hardware PASS 아님** | safe non-semantic 1/2/4-byte Write/readback 실기 |
| HomeDS402 | H37 software/source/WPF qualification 통합 | activation OFF | fresh artifact/C78 -> hardware matrix |
| HomeDS402Ex | SDK, ownership, retained store, approved-plan gate, WPF recovery 존재 | physical runtime/activation OFF | approved profile + fresh C78 -> runtime/hardware qualification |
| SetPosition | lifecycle + WPF recovery + host factory receipt/readback tooling 존재 | native runtime activation OFF | issue #44 vendor CRC + generated `_FileSys` ABI -> A/B backend -> RT exactly-once |

---

## 2. SetOperationMode current source truth

현재 source expectation:

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
Admin FeatureMask = 0x00000717
SetOperationModeSupportedMask = 0x018A
```

`0x018A` = PP(1), PV(3), IP(7), CSP(8).

구현된 host path:

- supported-mode live capability 확인;
- Axis1 current mode/status preflight;
- cross-mode Standstill/Fault/OperationEnabled fence;
- final Diagnostics Build/BootId/MapRevision refresh;
- one-shot Prepare;
- durable ArmBeforeDispatch;
- Start exactly once;
- definitive reject archive / no automatic replay.

raw Generic SDO `0x6060` mutation은 계속 금지한다.

---

## 3. SetOperationMode latest physical evidence

latest WPF/SDK identity:

```text
WPF BuildUtc=2026-08-31 02:33:22 UTC
SDK BuildUtc=2026-08-31 02:33:19 UTC
```

Axis1 CSP(8) -> PP(1):

```text
cross-mode preflight passed
StatusWord=0x02D0
final Diagnostics refreshed
Build=1
BootId=0x00000068
MapRevision=0x957F101E
Prepare PASS
journal armed before dispatch
PLC Start reject
ErrorId=-31000
Detail=SetOperationModeOutcomeStorageUnavailable(49)
```

BootId history `0x66 -> 0x67 -> 0x68`에서 동일 Detail 49가 반복됐다.

따라서 이전 host capability freshness ordering defect는 current blocker가 아니다. 현재 blocker는 **PLC Start admission before `0x6060` mutation**이다.

아직 physical PASS가 아닌 항목:

- Start acceptance;
- `0x6060` one-byte mutation;
- `0x6061` target verification;
- terminal outcome/retire;
- accepted/uncertain mutation recovery.

---

## 4. owner-channel correction 재평가

commit `c670bd6fbc816116eacbe19b94199479d1a8cacf`는:

- embedded LASAL client metadata order 정렬;
- AxisOwnership disconnected를 Detail 52로 분리;
- SDK/error catalog/static verification 동기화.

정적/source consistency 관점에서는 유지할 correction이지만, BootId `0x68` 실기에서 Detail 49가 재현됐으므로 **physical root-cause fix로는 실패**했다.

current corrected Start source 기준:

```text
zero session/sequence/admission token/owner generation -> 49
AxisOwnership disconnected -> 52
ownership identity validate/commit failure -> 42
runtime SetOperationMode feature gate OFF -> 64
```

따라서 corrected image가 실제로 실행된다는 전제에서는 Detail 49를 AxisOwnership disconnected로 계속 해석하지 않는다.

---

## 5. Detail 49 observability implementation

Detail 49 ambiguity split은 current `dev` source에 구현됐다. 이 변경은 admission, ownership, retained outcome, or no-replay behavior를 완화하지 않는 diagnostic contract change다.

```text
49 = actual SetOperationMode outcome/storage unavailable
52 = AxisOwnership channel unavailable [existing]
63 = SetOperationModeAdmissionIdentityUnavailable
64 = SetOperationModeFeatureDisabled
```

Start rejection은 다음 zero/nonzero discriminator를 별도 detail로 구분한다.

- feature enabled/disabled;
- caller session epoch zero/nonzero;
- request sequence zero/nonzero;
- admission token zero/nonzero;
- owner generation zero/nonzero;
- AxisOwnership connected/disconnected;
- ownership validate result;
- ownership commit result.

PLC download 후 Detail 63/64를 확인해 다음 functional root cause를 결정한다. admission token이나 ownership validation을 우회하지 않는다.

---

## 6. Generic SDO current source truth

activation:

```text
LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE
LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE
```

완료된 software tranche:

- manual Server / programmatic API dual-entry arbitration;
- axis 1..4 generic scalar 1/2/4-byte Write policy;
- WPF arbitrary request editor + exact preview;
- v3 durable exact-request recovery;
- ordinary Write safe-state correction.

ordinary Write requirements:

- Standstill=True
- Fault=False
- OperationEnabled=False

raw semantic/dedicated-owner blocklist 유지:

`0x6040`, `0x6060`, `0x607A`, `0x60FF`, `0x6071`, `0x3204`, `0x20FC`.

software evidence는 hardware Write/readback PASS를 대신하지 않는다.

---

## 7. repository 운영 상태

remote integration branch는 `main`, `dev`만 유지한다.

운영 규칙:

- `dev` = current integration / qualification source truth;
- source SHA + LASAL generated artifact + PLC-loaded image + WPF build를 별도 identity gate로 관리;
- CI/source PASS와 hardware PASS를 분리;
- blocker 발견 시 불필요한 branch를 늘리지 않고 current design/status 문서를 먼저 갱신.

---

## 8. 현재 작업 순서

1. **DONE** — host capability freshness ordering correction
2. **DONE** — Axis1 CSP -> PP host preflight/final refresh/Prepare/journal path 확인
3. **DONE** — BootId `0x66`, `0x67`, `0x68`에서 PLC Detail 49 반복 확인
4. **DONE** — owner-channel source consistency correction을 physical fix에서 제외하여 재분류
5. **NOW (DESIGN ONLY)** — Detail 49 내부 원인 분리 설계 문서화
6. 다음 implementation 시 feature-disabled vs admission-identity-zero를 별도 detail/evidence로 분리
7. 원인 확정 후 해당 runtime path만 수정
8. Start accepted 이후 Axis1 PP/PV/IP/CSP physical matrix
9. Generic SDO physical matrix
10. failure/recovery matrix -> Axis2..4
11. production release activation review

현재 release 판정은 계속 **NO-GO**다.

# 개발 상태 스냅샷 — 2026-09-01

- current integration / qualification source: `dev`
- current baseline: `0afbc2a79dff1b63f908b1bde3bd2502843045ff` (`dev : SetOpMode Complete`)
- SetOperationMode: **IMPLEMENTATION COMPLETE / Active**
- current P0: **Generic SDO 잔여 구현/qualification**
- production release posture: **NO-GO**

이 문서는 SetOperationMode 완료 후의 current 개발 순서와 기능 상태를 고정한다. 구현 완료, CI,
C78/generated artifact, PLC load, physical effect와 production release를 서로 다른 판정으로 기록한다.

---

## 1. SetOperationMode 완료

current supported contract:

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
Admin feature mask = 0x00000717
SetOperationModeSupportedMask = 0x018A
Modes = PP(1), PV(3), IP(7), CSP(8)
Commands = 0x7D23 / 0x7D24 / 0x7D25
```

완료된 핵심:

- exact requested-mode ACK/domain-failure echo; CSP-fixed ACK 분류 제거
- fresh cross-mode drive preflight + FINAL Diagnostics capability refresh
- same-target no-write와 cross-mode write 구분
- exact requested mode `0x6060` one-shot dispatch
- original deadline 안의 `0x6061` read-only settling
- write-dispatched 이후 automatic Start/Write replay 금지
- terminal owner publish/release bounded retry without new SDO write
- exact terminal outcome + owner released/executor reusable evidence
- exact-generation retirement
- WPF durable pre-dispatch journal
- Running exact-key polling; premature PASS 금지
- Failed/Aborted terminal archive/retire 후 failure 반환
- indeterminate/query reject durable fence 유지
- stale recovery operator retirement에서 PLC completion proof를 조작하지 않음
- Generic SDO raw `0x6060` permanent deny

과거 17:28 capability freshness, Detail46/readback, owner publish reason6 및 CSP-fixed ACK 문제는
`SET_OPERATION_MODE_READBACK_SETTLING_FIX_20260831.md`에 historical investigation으로 남긴다.

### CI / repository hygiene

feature implementation 완료와 아래 CI 상태를 구분한다.

- C78 evidence tool run `33455821803`: SUCCESS
- static qualification run `33455821841`: 93 PASS / 1 FAIL
  - remaining fail: LASAL metadata Client order vs generated declaration order
- WPF recovery run `33455821887`: hosted runner MSBuild discovery 단계 실패; tests 미실행

이 항목들은 repository qualification hygiene이며 current SetOperationMode 기능 설계를 다시 열지 않는다.

---

## 2. P0 — Generic SDO

issue #46의 SetOperationMode 범위는 완료됐고 issue는 Generic SDO 잔여 범위만 추적한다.

현재 구현:

- Manual Server + tokenized programmatic dual-entry 기반
- physical axis1..4 generic scalar 1/2/4-byte Write source policy
- arbitrary WPF editor / exact request preview
- durable exact-request journal / no automatic replay
- ordinary safe-state gate
- **SWR-01 SOURCE/PC COMPLETE**: UI24 known preset과 generic admission 분리. preset 0건이어도
  global/capability policy가 준비되면 `CanAttemptSubmission=True`; legacy `NoApprovedTarget`은 호환성만
  유지하고 generic evaluation은 더 이상 set하지 않음
- **SWR-02~04 SOFTWARE COMPLETE (working tree, 2026-09-01)**:
  - four-ticket canary 결과를 target authorization이 아닌 image/session transport proof로 저장
  - ordinary Write의 exact baseline Read -> immutable confirmation -> exact pre-write guard Read
  - journal v4에 baseline/prewrite/expected bytes를 저장하고 equality 뒤에만 one-shot dispatch
  - generic identity-pinned submit과 Manual/programmatic shared executor ownership 유지
  - PLC `0x7E50` parser가 Bool/8-bit=1, 16-bit=2, 32-bit=4 byte payload를 exact length로 수용

계속 차단하는 semantic/dedicated-owner object:

```text
0x6040 0x6060 0x607A 0x60FF 0x6071 0x3204 0x20FC
```

남은 qualification 순서:

1. C78 Rebuild/Link와 PLC download
2. Axis1 safe non-semantic 1/2/4-byte exact Write/readback
3. Manual/programmatic 동시 접근 BUSY arbitration
4. timeout/disconnect/readback mismatch durable recovery
5. Axis2..4 matrix

VS2019 Debug WPF/Smoke project와 SDK test project는 build PASS했고 SWR-01~04 source verifier도 PASS했다.
요청에 따라 test executable, C78 build/link, PLC download, 실축 Write/readback과 packet evidence는
실행하지 않았다. 따라서 **software implementation ready / physical qualification pending**이며 Generic
SDO 전체 release 상태는 계속 P0/NO-GO다. 상세 결과는
`SDO_WRITE_SOFTWARE_IMPLEMENTATION_RESULT_20260901.md`를 따른다.

---

## 3. P1 — HomeDS402

tracker: issue #32.

software/source/WPF qualification은 존재하지만 activation은 OFF다. 다음은 current exact `dev` tree에서
fresh C78/ARM Rebuild+Link, generated artifact identity review, full SourceOnly closure를 묶어 수행한다.
그 same-image로 PLC/hardware normal/fault/timeout matrix를 진행한 뒤 별도 activation review한다.

---

## 4. P1 — HomeDS402Ex

trackers: issue #28, issue #35.

먼저 axis1..4 wiring/polarity/homing method/scale/range를 evidence와 함께 승인한다. 그 profile과 같은
source tree에서 fresh C78/generated artifact/SourceOnly를 닫은 뒤 parameter SDO snapshot/program/restore와
physical homing runtime으로 진행한다. 그 전까지 capability bit11과 physical runtime은 OFF다.

---

## 5. Blocked — SetPosition

tracker: issue #44.

현재 lifecycle/WPF recovery/host factory tooling은 있으나 durable A/B runtime backend와 native
exactly-once execution은 fail-closed다. 다음 두 external prerequisite가 필요하다.

- real LASAL/vendor `CheckSum.CRC32` golden vectors
- LASAL IDE/CodeGenerator가 생성한 `_FileSys` class/client ABI

CRC 알고리즘이나 generated ABI를 추측해서 우회하지 않는다.

---

## 6. 후순위 기능

| 기능 | 상태 | next |
|---|---|---|
| PI Write | Dormant | capability/allowlist qualification |
| D4 Recorder Double | Dormant | route/capability proof |
| Dynamic node/DI | Dormant | bits15/16 qualification |
| Extended SDO result | Dormant | bit12 qualification |
| Digital Output Write `0x7E23` | Missing | LASAL handler/owner/allowlist |

---

## 7. 개발 순서

```text
SetOperationMode COMPLETE
        |
        v
Generic SDO completion (#46 remainder)
        |
        +--> HomeDS402 artifact/hardware (#32)
        |
        +--> HomeDS402Ex profile + artifact (#28, #35)
        |
        +--> SetPosition external prerequisites (#44)
        |
        v
Dormant/Missing API activation + distribution/release qualification
```

SetPosition의 외부 prerequisite가 준비되지 않아도 Generic SDO/HomeDS402/HomeDS402Ex의 독립 작업은 진행할
수 있다. 반대로 hardware/profile evidence가 필요한 값을 추측으로 채우지는 않는다.

---

## 8. production release

SetOperationMode feature implementation은 완료됐지만 전체 API는 아직 preview다. production release에는
남은 기능별 PC/source/C78/PLC/hardware gate와 distribution/manual sync가 동일 source/artifact set에서
닫혀야 한다. 따라서 project-level 판정은 계속 **NO-GO**다.

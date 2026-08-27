# SetOperationMode MODE-11 software integration evidence — 2026-08-27

## 판정

SetOperationMode의 PP(1), PV(3), IP(7), CSP(8) **software implementation**을 current `dev` 통합 후보로 승격한다.

이 evidence는 software/source 단계의 구현 완료를 의미하며 C78/PLC/hardware qualification 또는 production activation을 의미하지 않는다.

## 통합 lineage

- source qualification lineage: PR #18 `codex/setopmode-mode11-bench-activation`
- integration PR: #48 `codex/setopmode-mode11-implementation-integration`
- PR #18 head는 integration history의 merge parent로 보존한다.
- PR #18의 qualification-only activation 변경은 최종 integration tree에서 제외한다.

## software 구현 범위

- SDK allow-list: PP(1), PV(3), IP(7), CSP(8)
- Homing(6): `HomeDS402` / `HomeDS402Ex` 전용으로 SetOperationMode에서 계속 거부
- WPF requested-mode selector: PP/PV/IP/CSP
- LASAL dormant runtime:
  - preflight `0x6061`
  - same-mode이면 `0x6060` write 0회
  - mode 변경이면 exact one-byte `0x6060:0 = requestedMode`
  - verify `0x6061 == requestedMode`
  - write-dispatch 이후 original mutation 자동 replay 금지
  - recovery는 retained requested mode와 read-only verify를 사용
- SDK/WPF regression coverage를 multi-mode semantics에 맞게 갱신
- dynamic WPF localization contract를 multi-mode UI에 맞게 갱신

## activation boundary

통합 tree에서 다음을 유지한다.

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE
Admin mask = 0x00000017
Admin bits 8/9/10 = OFF
```

따라서 software target path가 구현돼 있어도 production Start는 fail-closed 상태다.

## 진행도

SetOperationMode release-oriented progress를 **60% -> 65%**로 갱신한다.

65%는 다음을 포함한다.

- lifecycle / exact request identity
- common owner / diagnostics SDO arbitration
- irreversible-dispatch no-replay recovery
- safety preemption / quarantine
- WPF durable journal/recovery
- PP/PV/IP/CSP software mutation target path

다음은 아직 포함하지 않는다.

1. PLC-advertised `SupportedModeMask`
2. current exact-source fresh C78/ARM + generated artifact identity
3. same-image PLC load/runtime proof
4. Axis1 PP/PV/IP/CSP packet/hardware matrix
5. MODE-12 timeout/disconnect/mismatch/quarantine/retire matrix
6. Axis2..4 physical expansion
7. MODE-14 paired production activation

## release rule

이 문서 또는 PC CI PASS만으로 compile gate나 Admin capability bits를 올리지 않는다. production activation은 위 downstream evidence가 같은 승인 source/image 세트로 닫힌 뒤 별도 changeset에서 수행한다.

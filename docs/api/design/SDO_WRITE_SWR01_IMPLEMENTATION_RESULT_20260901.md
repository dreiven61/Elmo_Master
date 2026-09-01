# SDO Write SWR-01 구현 결과

> 후속 상태: SWR-02~04 software 구현은 같은 날 완료됐다. current 판정은
> `SDO_WRITE_SOFTWARE_IMPLEMENTATION_RESULT_20260901.md`를 사용한다.

- 날짜: 2026-09-01
- 기준 branch: `dev`
- 범위: `SDO_WRITE_DETAILED_DESIGN_20260901.md`의 SWR-01
- 판정: **SOURCE/PC COMPLETE / PLC-HARDWARE NOT TESTED / Generic SDO P0 NO-GO**

## 1. 완료한 변경

- `LMCSdoWriteTarget`과 `ApprovedTargets`를 known preset metadata로 명시했다.
- UI24 preset collection의 내부 이름을 generic allowlist와 분리했다.
- preset 목록이 0건이어도 global/capability/identity/payload 조건이 준비되면
  `CanAttemptSubmission=True`가 되도록 고정했다.
- public `NoApprovedTarget = 1u << 0` 값은 source/binary compatibility를 위해 유지했다.
- generic evaluation은 `NoApprovedTarget`을 더 이상 set하지 않는다.
- global SDK Write gate OFF는 `WritePolicyDisabled = 1u << 10`으로 구분한다.
- WPF UI24 same-value qualification은 generic authorization이 아니라 transport-canary임을 표시한다.
- PLC bit9는 기존처럼 global generic gate에만 의존하고 UI24 per-axis flag와 독립임을 source
  comment와 verifier로 고정했다.

## 2. 유지한 안전 경계

- 실제 SDO Write를 실행하지 않았다.
- semantic/dedicated-owner blocklist는 변경하지 않았다:
  `0x6040`, `0x6060`, `0x607A`, `0x60FF`, `0x6071`, `0x3204`, `0x20FC`.
- physical axis 범위 1..4, canonical scalar 1/2/4-byte, Bool 0/1 검사는 유지했다.
- durable arm 이후 automatic replay 금지와 exact readback 계약은 변경하지 않았다.
- UI24 conservative value range와 current target-bound qualification canary는 SWR-02 전까지 유지한다.

## 3. PC 검증

- `Verify-LasalGenericSdoWrite.ps1`: PASS
- `LasalMotionControlLib.Tests` build: warning 0 / error 0
- SDK full regression: 1200/1200 PASS
- WPF solution Debug build: PASS
- WPF full smoke: 393/393 PASS

## 4. 미완료

- SWR-02 image/session transport proof 전환
- SWR-03 ordinary generic baseline/pre-write guard/journal/one-shot/readback workflow
- C78 Rebuild/Link와 generated artifact 확인
- PLC download
- Axis1 1/2/4-byte physical Write/readback
- contention/abort/timeout/cancel/disconnect no-replay matrix
- Axis2..4 확대와 packet evidence

따라서 SWR-01 완료를 Generic SDO 전체 구현 완료나 production PASS로 확대하지 않는다.

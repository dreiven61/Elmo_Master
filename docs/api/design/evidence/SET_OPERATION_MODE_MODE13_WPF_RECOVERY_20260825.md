# SetOperationMode MODE-13 WPF Recovery Evidence

- 기준일: 2026-08-25
- 기준 branch: `dev`
- merged implementation: PR #15, squash commit `01bc9ed80b77a901b57afc8ee32a7b446a1f7f85`
- qualification workflow: `SetOperationMode WPF recovery qualification`
- verified PR run: `32789073664`
- 판정 범위: **PC/WPF PASS only**
- production activation: **NO-GO / OFF 유지**

## 1. MODE-13 계약

MODE-13의 PC/WPF 경계는 다음과 같이 검증했다.

1. `0x7D23` Start 전 exact recovery identity를 durable journal에 먼저 기록한다.
2. Start ACK는 completion으로 취급하지 않는다.
3. accepted 또는 write-boundary 이후 uncertain 결과는 original `0x7D23`을 재전송하지 않는다.
4. startup/reconnect recovery는 exact identity를 다시 확인한 뒤 `0x7D24` outcome query와 read-only observation만 사용한다.
5. terminal outcome과 exact `RecordGeneration`을 durable journal에 기록한 다음 `0x7D25` retire를 수행한다.
6. retire 성공을 확인한 뒤에만 active recovery interlock을 해제한다.
7. `DiagnosticsBuild + DiagnosticsBootId + MapRevision` 중 하나라도 다르면 recovery request를 wire에 보내지 않는다.

## 2. definitive Start rejection 의미

current LASAL `HandleAxisSetOperationModeStart` source를 확인한 결과 deterministic domain rejection은
`AxisOperationModeState[recordBase]`를 `Running`으로 publish하기 전에 failure ACK를 반환한다.
따라서 SDK가 `LMCAxisSetOperationModeRejectedException`을 반환한 request에는 조회할 retained
SetOperationMode outcome이 존재하지 않는다.

WPF는 이 경우 accepted/uncertain recovery와 다르게 처리한다.

- exact durable request identity와 rejection response를 다시 대조한다.
- checksum-protected evidence 파일에 endpoint, axis, Build/BootId/MapRevision, 128-bit intent,
  requested mode, response tuple과 당시 active journal bytes/hash를 먼저 write-through 저장한다.
- evidence 저장이 완료된 뒤에만 active recovery journal을 제거하고 journal을 reopen한다.
- identity mismatch, success-shaped response 또는 evidence write 실패는 fail-closed이며 active
  interlock을 해제하지 않는다.
- 이 해제는 command replay가 아니다. 새 Start는 별도 explicit confirmation과 새 recovery identity가 필요하다.

## 3. Windows qualification 결과

GitHub Actions PR run `32789073664`에서 다음이 PASS했다.

| Gate | 결과 |
|---|---:|
| Debug WPF smoke build | PASS, 0 warnings / 0 errors |
| Debug `--filter SetOperationMode` | `12/12 PASS` |
| Release WPF smoke build | PASS, 0 warnings / 0 errors |
| Release `--filter SetOperationMode` | `12/12 PASS` |
| `git diff --check origin/dev...HEAD` | PASS |

focused smoke 12개에는 다음 recovery 성질이 포함된다.

- durable journal no-replay surface
- startup `ArmedBeforeDispatch -> RecoveryRequired`
- terminal proof + exact-generation retire
- recovery-key mismatch/stale-copy rejection
- journal integrity validation
- SDK BootId mismatch zero-wire fence
- WPF startup endpoint lock
- explicit one-shot confirmation gate
- definitive rejection durable archive 후 interlock 해제
- rejection identity mismatch 시 interlock 유지

## 4. 이 PASS가 증명하지 않는 것

이 checkpoint로 아래 항목을 PASS 처리하지 않는다.

- fresh LASAL IDE C78/ARM Rebuild/Link
- generated `Classes.lcb` artifact identity 승인
- same image PLC download/runtime
- live drive `0x6061 -> 0x6060 -> 0x6061` packet causal evidence
- 축 1~4 timeout/disconnect/mismatch/quarantine hardware matrix
- capability bits 8/9/10 활성화
- `LMC_DIAG_SET_OPERATION_MODE_ENABLED = TRUE`

따라서 SetOperationMode는 계속 `Dormant`이며 production activation은 금지한다.

## 5. 다음 gate

MODE-13 PC/WPF gate 이후 개발 순서는 다음과 같다.

1. latest `dev` source의 fresh C78/ARM Rebuild/Link
2. generated artifact identity/ABI review
3. 같은 image의 Build/BootId/MapRevision tuple 기록
4. MODE-11 same-mode no-write + exact one-write/readback packet
5. MODE-12 축 1부터 timeout/disconnect/mismatch/quarantine/retire matrix 후 축 2~4 확대
6. 위 증거가 모두 닫힌 뒤에만 MODE-14 paired activation 검토

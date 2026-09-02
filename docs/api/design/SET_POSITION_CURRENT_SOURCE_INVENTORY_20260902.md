# SetPosition SP-C0 current source inventory — 2026-09-02

- branch: `dev`
- design: `SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`
- tranche: `SP-C0 current-dev source inventory / regression freeze`
- result: **PASS**
- validating commit: `08e85d456ad118abaec8405fe9ab1c1ec3baa974`
- workflow: `SetPosition SP-C0 current source inventory`
- run: `33579066015`
- activation posture: **KEEP OFF**

이 문서는 SP-C1 이후 구현에서 existing source를 중복 작성하지 않도록 current `dev`의 SetPosition 경계를 고정한다.

## Existing

### LASAL Store / lifecycle

- `LMCSetPositionStore.st`
  - 336 UDINT / 1,344-byte canonical ledger backing 존재
  - `BeginSetPosition`
  - `CommitSetPositionTerminal`
  - `ReadSetPositionOutcome`
  - `RetireSetPositionOutcome`
  - `CheckSum` client 존재
- `LMCControlCommandService.st`
  - `AxisSetPositionAsyncState` cross-cycle scaffold 존재
  - `SetPositionStore` client wiring 존재
  - `HandleAdminSetPosition`
  - `ProcessAdminSetPositionAsync`
- `TCPMotionInterface.st`
  - pending SetPosition handler 존재
  - `0x7D12 Start`, `0x7D14 ReadOutcome`, `0x7D1A Retire` route 존재

### WPF / host tooling

- `AxisSetPositionRecoveryJournal.cs` core 존재
- `AxisSetPositionRecoveryJournalTests.cs` 존재
- MainWindow 계층에는 SetPosition recovery 관련 기존 partial integration이 있으므로 SP-C6에서 먼저 재사용 범위를 inventory하고 missing dispatch/interlock만 추가한다.
- existing host deployment tooling 유지:
  - `tools/LmcSetPositionStoreDeploymentReceipt.ps1`
  - `tools/Start-LmcSetPositionStoreDeployment.ps1`
  - `tools/Verify-LmcSetPositionStoreDeployment.ps1`

## Missing / not authorized yet

- production `_FileSys` durable A/B backend 없음
- `ProcessAdminSetPositionAsync` production path의 authorized native `.SetPosition()` call 없음
- RT claim-before-native exactly-once mutation executor 미구현
- terminal durable readback-before-release 미구현
- `tools/Generate-LmcSetPositionStoreImages.ps1` 없음

이 항목은 SP-C1 prerequisite를 우회해 구현하지 않는다.

## Fail-closed gates confirmed

```text
LMC_ADMIN_SET_POSITION_STORE_CONFIGURED = FALSE
LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED = FALSE
LMC_ADMIN_SET_POSITION_MAX_JUMP_AXIS1 = 0
LMC_ADMIN_SET_POSITION_MAX_JUMP_AXIS2 = 0
LMC_ADMIN_SET_POSITION_MAX_JUMP_AXIS3 = 0
LMC_ADMIN_SET_POSITION_MAX_JUMP_AXIS4 = 0
```

Admin capability bits 3/5/7 및 production activation은 계속 OFF로 취급한다.

## Automated evidence

`tools/Verify-SetPositionCurrentSourceInventory.ps1`:

- **37 checks PASS**
- `_FileSys` backend가 IDE ABI evidence 없이 hand-authored되지 않았음
- SetPosition Start/Outcome/Retire routes 존재
- Store/ordinary ownership/max-jump fail-closed 확인
- native SetPosition production call 부재 확인
- journal/receipt tools existing 범위 확인
- factory image generator가 CRC evidence 전에는 없는 상태 확인

WPF Debug build:

- **0 warnings / 0 errors**

`Wpf.AxisSetPositionJournal` smoke:

- **11/11 PASS**

`git diff --check`:

- PASS

## SP-C0 decision

`SP-C0 current source inventory frozen`은 완료로 판정한다.

다음 단계는 **SP-C1 prerequisite capture**다. 아래 두 항목 없이는 SP-C2 durable backend를 시작하지 않는다.

1. vendor `CheckSum.CRC32` golden fixture
2. LASAL IDE-generated `_FileSys` class/client/channel ABI

이 문서는 SetPosition을 Active로 승인하지 않는다. SetPosition은 계속 `Dormant / fail-closed`다.

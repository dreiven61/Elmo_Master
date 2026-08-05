# Native capture alignment implementation design

- Date: 2026-07-21
- Reviewed: 2026-07-23
- Evidence: `ELMO_NATIVE_API_PACKET_CAPTURE_ANALYSIS_2026-07-21.md`
- Implementation status: PMAS/custom Recorder and documentation changes applied;
  D5 general-inline 1/2/4-byte plus TypeMismatch recovery live PASS, remaining
  controller fault/Recorder gates open
- Targets:
  - `Codex_PMAS_WPF_Version2`
  - LASAL custom Recorder `0x7E43`
  - D2/D5 architecture documentation

## 1. Boundary

PMAS Version2 uses MMCLibDotNET directly. LASAL WPF and PLC use the repository-local
`0x7Exx` DINT protocol. The implementation target is functional alignment at the API
workflow level, not packet cloning.

Custom protocol invariants remain unchanged.

- nonzero RequestId and response echo
- MapRevision and DiagnosticsBootId validation
- session owner and Recorder identity validation
- fixed memory, bounded chunk, CRC and release/adopt lifecycle
- write capabilities disabled until fail-closed policy and PLC tests pass

## 2. PMAS EtherCAT Health

### Problem

`MMC_ETHERCAT_DIAGNOSTICS_INFO` contains RX error, invalid frame and lost-link counters
for ports 0..3. PMAS Version2 filtered and displayed only RX and lost-link counters.
A slave with only invalid-frame errors was hidden.

### Design

- A row is nonzero when any RX, InvalidFrames or LostLink counter is nonzero.
- The row text displays all three counter groups for all four ports.
- Do not label native `usNetworkState` as EtherCAT ESM state.
- Do not synthesize axis Online, AL code or DS402 values from communication counters.

## 3. PMAS Recorder configuration helper

### Problem

The UI exposed raw native `uiRv` and `uiRp` only. Checked PI Catalog rows were not used,
and the zero-value guard rejected the valid formula result for AxisRef 0, input PI index 0.

### Design

`Use Selected PI` converts checked, supported PI entries in displayed order.

```text
uiRv = (AxisReference << 16)
     | (PiIndex & 0x3FFF)
     | (Direction << 14)

uiRc = (1 << selectedCount) - 1
```

Rules:

- Direction must be `ePI_INPUT` or `ePI_OUTPUT`.
- Count must be 1..22.
- Existing unsupported PI types remain blocked.
- `uiRv=0` is accepted because AxisRef 0 + input + PI index 0 encodes to zero.
- Raw editing remains available for native regular signals and expert trigger setup.
- The helper does not send a controller packet. Start remains the operation that calls
  `MMC_BeginRecordingCmdEx`.

## 4. PMAS Recorder lifecycle and download gate

### Local state

The window retains the following controller-bound state.

```text
HasStatus
RemainingRecordingIndex (uiRr)
TriggerStatus (uiSr)
ValidatedHeader
```

This state is invalidated before Start, Stop and Status RPC attempts, and on connection
reset or local Release. Header cache is cleared before every Header RPC attempt. A failed
or lost RPC response therefore cannot reactivate an older ready/header state. Downloaded
PC data may remain after disconnect for export, but it is cleared by a new Start attempt,
a new Download attempt or Release. Every Status refresh requires a new Header read, so
Download always uses a header read after the latest authoritative `uiSr`.

### Status decode

```text
Phase     = uiSr & 0xFF
ReadyMask = (uiSr >> 8) & 0xFF
```

Ready-mask mapping uses native one-based wording and zero-based MMCLib buffer index.

| ReadyMask bit | Native wording | BufferIndex |
|---:|---|---:|
| 0 | Buffer 1 ready | 0 |
| 1 | Buffer 2 ready | 1 |

### Header preconditions

`MMC_UploadDataHeaderCmd` is called only when all conditions hold.

1. Connection is active.
2. Status has been refreshed.
3. At least one ready-buffer bit is set.

Native `MMC_UploadDataHeaderCmd` has no BufferIndex input; the returned configuration
header is global. The response is cached only when `Status=0`, `ErrorID=0` and `Rl>0`.
On success the UI sets `From=0`, `To=Rl-1`.

### Download preconditions

`MMC_UploadDataCmd` is called only when all conditions hold.

1. The selected buffer is still ready in the last authoritative `uiSr`.
2. A valid global Recorder header was read.
3. `From <= To`.
4. `To < Header.Rl`.

The downloaded array is published to plot/CSV state only after the synchronous MMCLib
call returns successfully. An MMCLib status/error exception therefore cannot be displayed
as successful downloaded data.

### UI workflow

```text
Load PI Catalog
  -> check rows
  -> Use Selected PI
  -> review uiRp and Configure/Start
  -> Stop or wait for completion
  -> Refresh Status until ready
  -> Read Header
  -> Download
  -> Export CSV if a file is needed
```

`Download` stores the array only in process memory. `Export CSV` is the file operation.
Changing BufferIndex or the From/To range immediately re-evaluates Download button
availability.

## 5. Custom LASAL Stop semantics

### Problem

The native example returned success when Stop was called after status `uiSr=0x0104`
(terminal-ready). Custom `0x7E43` returned DetailCode 19 unless state was Armed or
Recording. A natural completion racing with Stop could therefore surface a false failure.

### State rule

The handler keeps validation in this order.

1. Request size and caller session
2. RecordId/BufferId
3. MapRevision
4. DiagnosticsBootId
5. owner epoch
6. state action

State action is:

| State | Result |
|---|---|
| Armed, Recording | Increment `StopRequestSequence`, success |
| Ready, Uploading | No state mutation, idempotent success |
| Empty, Configured, other | DetailCode 19 InvalidState |

Ready/Uploading success does not bypass identity or ownership. A stale or foreign client
continues to fail closed.

## 6. D2 and D5 design alignment

### D2

Custom PI snapshot corresponds semantically to native:

- `MMC_ConfigureBulkReadPI` / `0x1102`
- `MMC_PerformBulkReadCmdPI` / `0x1103`

Native generic parameter bulk `0x10C9/0x10CA` is out of current custom Diagnostics scope.

### D5 SDO Read: legacy evidence and current general-inline contract

The only successful native SDO capture is `0x1000:0`, UInt32, 4 bytes. The initial custom
SDO Read-only increment therefore used:

```text
CapabilityBits after all gates = 0x0000013F
MaxSdoDataBytes                = 4
Allowed data length            = 4 only
SDOWrite / PIWrite / result chunk bits = 0
```

The legacy internal test source advertised this capability and the retained capture remains
valid evidence for that fixed vector. It is not evidence for the current general-inline scope
or production approval.

The initial design deferred 8/12-byte support because the earlier callback path did not expose
actual SDO response length. Zero-initialized trailing bytes could otherwise hide an object
size mismatch.

The current 2026-07-22 implementation parses `0x7E03`, `0x7E04` and `0x7E50`, uses four
`LMCSdoExecutor : EtherCAT_SDOBase` drive clients, callback mailboxes and a one-ticket service
state machine. It validates fixed sizes, SDO flags, the reserved field and exact read/write
payload shape. Malformed shapes return BoundsInvalid. General-inline Read accepts slave 1..4,
nonzero ObjectIndex, any U8 SubIndex, timeout 1..60000 and ValueType-matched 1/2/4-byte lengths.
It requires capability bits 8 and 13; the stable-BootId test value is `0x0000213F` with
`MaxSdoDataBytes=4`. Write, 8/12-byte and extended result remain fail-closed. The earlier
captures prove the legacy `0x13F`, `0x1000:0` UInt32 4-byte path on slaves 1 through 4.
The BootId 6 general-inline capture reproduced a submit-side `ResourceBusy(9)` lockout after an
earlier error. The executor source then changed to publish Running before the vendor request can
callback, clean up through a private Releasing state, return owned validation failures as
consumable terminal results, and reserve hard quarantine for unsolicited/duplicate callbacks or
invariant failures. `10_DriveRead_Axis1to4` subsequently proved general-inline Int8/1-byte and
BitField16/2-byte success. `12_SDO_GeneralInline_4Byte_FailureRecovery` proved UInt32/4-byte
success and same-BootId TypeMismatch failure followed by Int8/1-byte success without
ResourceBusy. Remaining qualification is abort/offline, timeout, queued cancel,
disconnect/orphan, contention and duplicate/late callback behavior.

## 7. Verification gates

Static/PC gates:

- PMAS Version2 Debug/Release build: PASS to temporary outputs because the running app
  locked the normal Debug output executable
- current LMC PC tests: Debug/Release 1006/1006 PASS
- WPF Debug/Release builds and actual-control smokes: 278/278 PASS
- LASAL SourceOnly/full static contracts: PASS
- `Classes.lcb` general `TryStartRead` declaration and generated metadata: synchronized
- `git diff --check`: PASS before commit

The gate-off D5 source passed the recorded LASAL IDE Rebuild/Link. BootId 5 and the four-axis
runtime results prove that a gate-on PLC download ran, but the matching IDE Rebuild/Link and
implementation-smoke log was not preserved.

Live gates that remain manual:

- PMAS WPF: `uiSr=0` keeps Header/Download disabled and reports no ready buffer
- PMAS WPF: `uiSr=0x0104`, header `Rl=64` permits BufferIndex 0 and range `[0..63]`
- custom LASAL: Ready/Uploading Stop succeeds for the owner and rejects wrong identity
- custom `0x7E45/0x7E46` packet sequence and recorder data/CRC
- D5 abort/offline, timeout, queued cancel, disconnect/orphan, contention and
  duplicate/late callback lifecycle after general-inline 1/2/4-byte and
  TypeMismatch-recovery PASS

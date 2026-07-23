# SIGMATEK Phase 1/2 Live Packet Capture Analysis

- Analysis date: 2026-07-23
- Target PLC: `10.10.150.1:4000`
- PC client: `10.10.150.13`
- Test app: `LMC_Library/LasalApiWpfTestApp`
- Protocol reference: `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`
- Capture root: `test/packet_capture/SIGMATEK_API_Analyze`

## 1. Conclusion

The 2026-07-23 captures close the happy-path PLC evidence for the Phase 1 read-only
Admin/drive surface, Phase 2 `0x7D22` relative motion, the four-entry D1/D2 PI/Bulk
flow, D5 general-inline 1/2/4-byte Read, and same-BootId recovery after an expected
type failure. The dynamic group-monitor timeout, PowerOff verification flow, and the
`0x2051` None/ACS static member-slot alias also passed their live regressions.

The result is not a production approval. Fault injection, stale identity, repeated
Bulk soak, and true queued Buffered chaining remain separate runtime gates. The
`09b` result proves the current static alias only; it does not prove a true ACS
coordinate transform or MCS/PCS behavior.

| Test | Verdict | Evidence boundary |
|---|---|---|
| `01_Admin_Capabilities_7D00` | PASS | Admin capability response accepted on the live PLC. |
| `02_Admin_AxisParameters_1to4_7D10` | PASS | Physical axes 1..4 and the requested semantic parameters returned valid responses. |
| `03_Admin_GroupParameters_7D20` | PASS | Fixed group `0x0100` parameter selections returned valid responses. |
| `04_Group_Absolute_Regression_20A4` | PASS with prior UI defect | Motion was accepted and later reached stable InPosition. The old fixed 15 s UI monitor timed out before the long move completed. |
| `04b_Group_Absolute_DynamicTimeout_20A4` | PASS | A 55.034 s calculated limit kept polling through a 20.152 s move and reached three stable InPosition samples. |
| `05_Group_Relative_Aborting_XYZU_7D22` | PASS | Four `Buffer=1 (Aborting)` requests and ACKs succeeded, but all four XYZU deltas changed together. |
| `05b_Group_Relative_Aborting_PerAxis_7D22` | PASS for slot mapping; mislabeled buffer | X/Y/Z/U mapping is isolated and correct. The actual eight requests use `Buffer=2 (Buffered)`, not Aborting. |
| `06_Group_Relative_Buffered_7D22` | PASS for acceptance | Buffered requests were accepted. A second request queued while the first was still moving was not demonstrated. |
| `07_Group_Relative_StopRace_7D22_2085` | partial PASS | Move-first then Stop recovery passed. The separate stop-first race branch was not captured. |
| `08_Group_Recovery_2085_2048_204B` | partial PASS | Recovery dispatches passed, but the original run did not contain the required final status read. |
| `08b_PowerOff_FinalStatus_2045` | PASS | Final `0x2045` clears only the local Power Ready bit and proves `PowerOn=False`. |
| `08c_PowerOff_UI_Verification_204B_2045` | PASS with visual boundary | Pending/final application logs and final `PowerOn=False` passed. Button label/enable state needs a screenshot for visual proof. |
| `09_Group_ReadPosition_None_ACS_2051` | INVALID / recapture | The file contains two `0x2045 ReadStatus` calls and no `0x2051`; None/ACS position equivalence remains untested. |
| `09b_Group_ReadPosition_None_ACS_2051` | PASS for static alias | Coordinate 0/1 requests returned byte-identical 68-byte typed payloads. This is not evidence of a true ACS transform or MCS/PCS support. |
| `10_DriveRead_Axis1to4` | PASS | Axis 1..4 legacy status and all 12 D5 SDO tickets completed successfully. |
| `11_PI_Bulk_Regression` | PASS | Capability, 24-entry Catalog, four PI reads, Bulk Pending to Active, same-cycle snapshot, and Release match the current wire contract. |
| `12_SDO_GeneralInline_4Byte_FailureRecovery` | PASS | UInt32/4 succeeded, intentional UInt16/2 mismatch failed terminally, and Int8/1 then succeeded on the same BootId without ResourceBusy. |

## 2. Per-axis relative motion supplement (`05b`)

### 2.1 Exact `0x7D22` requests

Eight requests were captured. Slots 5..16 are zero, coordinate is None, transition is
ExactStop, and Execute is 1 in every request.

| Frame | RequestId | X | Y | Z | U | Buffer | ACK |
|---:|---:|---:|---:|---:|---:|---|---|
| 108 | 2 | +10000 | 0 | 0 | 0 | 2 Buffered | frame 109 success |
| 166 | 4 | -10000 | 0 | 0 | 0 | 2 Buffered | frame 167 success |
| 236 | 6 | 0 | +10000 | 0 | 0 | 2 Buffered | frame 237 success |
| 297 | 8 | 0 | -10000 | 0 | 0 | 2 Buffered | frame 298 success |
| 355 | 10 | 0 | 0 | +10000 | 0 | 2 Buffered | frame 356 success |
| 414 | 12 | 0 | 0 | -10000 | 0 | 2 Buffered | frame 415 success |
| 478 | 14 | 0 | 0 | 0 | +10000 | 2 Buffered | frame 479 success |
| 540 | 16 | 0 | 0 | 0 | -10000 | 2 Buffered | frame 541 success |

All requests use velocity 10000, acceleration 100000, deceleration 100000, jerk 0,
coordinate 0, transition 0, and buffer 2. Every response has successful outer and
Admin status, zero error/detail, and the matching RequestId.

### 2.2 Position-slot evidence

The `0x2051` start-position reads before the next command show that only the requested
slot changed.

| Response frame | X | Y | Z | U | Meaning |
|---:|---:|---:|---:|---:|---|
| 105 | 801340 | 801341 | 801341 | 801343 | initial |
| 163 | 811340 | 801341 | 801341 | 801343 | after X+ |
| 233 | 801340 | 801341 | 801341 | 801343 | after X- |
| 294 | 801340 | 811341 | 801341 | 801343 | after Y+ |
| 352 | 801340 | 801341 | 801341 | 801343 | after Y- |
| 411 | 801340 | 801341 | 811341 | 801343 | after Z+ |
| 475 | 801340 | 801341 | 801341 | 801343 | after Z- |
| 537 | 801340 | 801341 | 801341 | 811343 | after U+ |

The log reports motion observed and stable Group InPosition completion for all eight
commands. There is no independent position read after U-, so the final U return value
is not separately sampled even though the command and completion monitor passed.

The filename contains `Aborting`, but wire field `Buffer=2` and the application log both
prove that this run used Buffered. The earlier `05_Group_Relative_Aborting_XYZU_7D22`
capture contains four `Buffer=1` requests at frames 1884, 2063, 2751, and 2924 with
successful ACKs. Taken together, the two captures prove Aborting acceptance and
per-axis XYZU mapping. They do not prove two commands concurrently chained in the
Buffered queue.

## 3. Power-off final status supplement (`08b`)

| Frame | Relative time | Command/result | Raw state |
|---:|---:|---|---|
| 9/10 | 1.686949 s | `0x2048` Disable/Unlock success | - |
| 23/24 | 4.562858 s | `0x2045` before PowerOff | `0x40050000` |
| 31/32 | 8.133504 s | `0x204B` PowerOff accepted | - |
| 34/35 | 15.063645 s | final `0x2045` success | `0x40010000` |

The XOR between the two status values is exactly `0x00040000`, the project-local Power
Ready bit. Disabled/Unlocked (`0x00010000`) and the unrelated high state bit remain set;
function status, error, and group error remain zero. This is wire-level proof that the
PowerOff mode change completed with `PowerOn=False` about 6.93 seconds after the ACK.

## 4. Physical drive reads (`10`)

The capture contains these client requests:

- `0x2028` legacy axis status: 4
- `0x7E50` SubmitSDO: 12
- `0x7E03` operation-status polls: 48
- `0x7E00` diagnostics capability refresh: 12

Each axis runs one standalone operation-mode read and one composite status read. The
composite order is `0x2028 -> 0x6041:0 -> 0x6061:0`, so it is deliberately non-atomic.

| Axis | Legacy axis state | `0x6041:0` | `0x6061:0` | Result |
|---:|---|---|---|---|
| 1 | `0x2290020E` | `0x02B3` | 8 (CSP) | PASS |
| 2 | `0x22D0020E` | `0x02B3` | 8 (CSP) | PASS |
| 3 | `0x22D0020E` | `0x02B3` | 8 (CSP) | PASS |
| 4 | `0x22D0020E` | `0x02B3` | 8 (CSP) | PASS |

All 12 SDO submissions use MapRevision `0x957F101E`, timeout 1000 cycles, BootId 8,
and read flags 0. All terminal tickets are `Completed/Success`, operation error 0,
detail 0, and exact typed length. There are eight Int8/1 mode results with value 8 and
four BitField16/2 statusword results with value `0x02B3`.

The raw state/status values are reported as independent sources. This capture proves
the API and wire flow, not that these raw values are correct for a separate mechanical
reference or safety interpretation.

## 5. PI and Bulk (`11`)

### 5.1 Capability and Catalog

All seven capability responses are identical:

- Build 1, capability bits `0x0000213F`
- MapRevision `0x957F101E`
- Catalog entries 24, MaxBulkSignals 24
- Catalog stride 80, value stride 16
- Base cycle 1000 us, BootId 8, MaxSDO 4

Catalog info and two chunks return 16 + 8 entries with a valid LastChunk flag. The
selected actual-position entries are `0x00100104`, `0x00100204`, `0x00100304`, and
`0x00100404`, all Int32, active physical PDO inputs, and Bulk-readable.

### 5.2 Sequential PI

| Axis | Frame | Cycle | Raw | Status |
|---:|---:|---:|---:|---|
| 1 | 36 | 6157348 | 672215245 | Valid |
| 2 | 40 | 6157365 | 1511076167 | Valid |
| 3 | 44 | 6157368 | 672215325 | Valid |
| 4 | 48 | 6157372 | 672217197 | Valid |

The different cycle counters are expected because `Read Selected PI` executes four
sequential `0x7E20` calls. It is not the same-cycle path.

### 5.3 Bulk lifecycle and same-cycle snapshot

- Configure frame 52/53: requested ID 0, four signal IDs in axis order; returned
  BulkId 1, ConfigRevision 1, State Pending, count 4.
- Status frame 55/56: the same identity becomes Active at cycle 6165973.
- Snapshot frame 58/59: cycle 6168254, phase InputMapped, sequence 12336508 (even),
  flags `SameCycle|InputMapped`, no Partial flag.
- Release frame 61/62: exact identity and common success response.

| Signal | Raw | Type/status/detail |
|---|---:|---|
| `0x00100104` | 672215245 | Int32 / Valid / 0 |
| `0x00100204` | 1511076166 | Int32 / Valid / 0 |
| `0x00100304` | 672215327 | Int32 / Valid / 0 |
| `0x00100404` | 672217200 | Int32 / Valid / 0 |

The four entries share one snapshot header, cycle, timestamp, phase, and sequence as
required. Small differences from the earlier sequential PI values are expected because
the Bulk snapshot was captured about 10.9 seconds later.

## 6. Findings that remain open

1. `0x2047 GroupEnable` repeatedly returned valid error `-6`, while a later status read
   reported Locked/Standby and motion succeeded. Source inspection confirms that
   `LockProfile` acceptance is followed by a stale same-cycle LockState read. This is a
   PLC adapter defect, not a packet parser failure. Do not convert `-6` to success in the
   PC UI; remove the same-cycle completion check, ACK native acceptance and verify final
   lock through `0x2045` polling. The observed delay is the manual status-click interval,
   not measured lock latency.
2. The invalid `09` file is superseded by `09b`: coordinate DINT 0 and 1 both returned
   the same static member-slot vector. A true ACS transform is not implemented or
   proven, and MCS/PCS rejection still has no live negative capture.
3. Buffered acceptance is proven; true queue chaining requires dispatching command B
   before command A reaches InPosition and observing both ordered completions.
4. D1/D2 fault paths, stale map/config/BootId rejection, release reuse/double-release,
   partial entries, and long repeated snapshots remain untested.
5. D3/D4 Recorder trigger, reconnect/adopt, chunk integrity and soak remain untested.
6. D5 general-inline happy path and TypeMismatch recovery are now captured. Offline,
   timeout, cancel, disconnect/orphan and duplicate/late callback qualification remain.

## 7. None/ACS position captures (`09` and `09b`)

### 7.1 Invalid original capture (`09`)

The intended capture has no `0x2051` request.

| Request/response | Command | Result |
|---|---|---|
| frames 13/14 | `0x2045 GroupReadStatus` | `State=0x40060000`, all errors zero |
| frames 22/23 | `0x2045 GroupReadStatus` | `State=0x40060000`, all errors zero |

The text log likewise records `Read Group Status` twice. This is a valid status capture
but not evidence for None/ACS position equivalence. Preserve it as an invalid test
record; it is superseded by `09b_Group_ReadPosition_None_ACS_2051`.

### 7.2 Valid static-alias recapture (`09b`)

The corrected capture uses one TCP stream from `10.10.150.13:2090` to
`10.10.150.1:4000` and contains exactly two `0x2051` exchanges.

| Frames | Request | Response |
|---|---|---|
| 1/2 | Group `0x0100`, payload 8, coordinate None(0), Execute 1 | 0.715 ms, typed payload 68 bytes |
| 4/5 | Group `0x0100`, payload 8, coordinate ACS(1), Execute 1 | 2.365 ms, typed payload 68 bytes |

Both responses have `HeaderStatus=0`, `FunctionStatus=0x4000`, and `ErrorId=0`.
The 16 raw DINT slots are byte-identical:

```text
[-999997, -999998, -999997, -999998, 0, 0, 0, 0,
 0,       0,       0,       0,       0, 0, 0, 0]
```

Slots 1..4 therefore retain the X/Y/Z/U member order and slots 5..16 are zero in this
live sample. The companion TXT records two `Read Group Position PASS` operations.

This closes the runtime gate for the deliberately defined None/ACS static member-slot
alias. It does not establish that ACS performs a coordinate transform, and it provides
no runtime evidence for MCS/PCS transformation or rejection.

## 8. Dynamic timeout regression (`04b`)

The first attempt targeted `-1990000` on X/Y/Z/U and was correctly rejected with
function status 1 and error 7. The accepted attempt used:

- start `(801343, 801343, 801343, 801346)`
- target `(-1000000, -1000000, -1000000, -1000000)`
- velocity 200000, acceleration/deceleration 100000, jerk 0
- coordinate None, transition ExactStop, buffer Aborting

The request ACK at frame 29 was successful. Frames 32 through 977 contain 316 status
responses with state `0x40040000`. Frames 980, 983 and 986 then contain three stable
`0x40060000` responses. First InPosition occurred 20.025 s after the ACK and stable
completion occurred after 20.152 s.

The current UI formula yields 55,034 ms from an XYZU L1 distance of 7,205,375. This
proves that monitoring continued beyond the old 15 s limit and completed normally.
The companion TXT is zero bytes, so the exact timeout log string itself is not evidence.

## 9. PowerOff UI-flow regression (`08c`)

| Frames | Command | Result |
|---|---|---|
| 23/24 | `0x2048 GroupDisable` | success |
| 27/28 | `0x204B GroupPowerOff` | accepted, success |
| 32/33 | `0x2045 GroupReadStatus` | `0x40010000`, PowerOn clear, all errors zero |

The final status response arrived 1.899 s after the PowerOff ACK. The application log
records accepted/start-only, then local-state cleanup and explicit
`Power Off verified: ... PowerOn=False`. The packet and log therefore pass the control
flow. They cannot prove the visual button label or individual `IsEnabled` properties;
that requires a screenshot or UI automation assertion.

## 10. General-inline 4-byte and recovery (`12`)

Capability refresh remained stable at bits `0x0000213F`, MapRevision `0x957F101E`,
MaxSDO 4 and BootId 8.

| Ticket | Request | Terminal | Cycles | Result |
|---:|---|---|---:|---|
| 13 | Slave 1, `0x1018:1`, UInt32/4 | Completed/Success | 17 | `9A 00 00 00` = 154 |
| 14 | Slave 1, `0x6061:0`, UInt16/2 | Failed/Failed | 30 | `-32001`, TypeMismatch(5), no data |
| 15 | Slave 1, `0x6061:0`, Int8/1 | Completed/Success | 36 | `08` = CSP |

All Submit envelopes succeeded and all three tickets were allocated. BootId remained 8
through the intentional failure and following success. ResourceBusy(9) did not appear.
This closes the general-inline 4-byte happy path and the exact same-BootId TypeMismatch
recovery case; it does not close timeout, cancel, offline or disconnect/orphan cases.

The ordered source, UI and recapture work is defined in
[SIGMATEK next runtime qualification and Test UI design](SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md).

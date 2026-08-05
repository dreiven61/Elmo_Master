# Drive DS402 Fault and Error Code diagnostics

Date: 2026-07-29

## Outcome

The PC API now treats the real DS402 status word and the LASAL axis status as
different observations.

- `LMCDriveStatus.HasDs402Fault` is derived only from bit 3 of the status word
  read from `0x6041:0` through the bounded D5 SDO Read path.
- `GetDriveErrorCode[Async]` reads `0x603F:0` as one `UInt16`/2-byte D5 SDO
  operation and returns a typed result with ticket and terminal-status
  provenance.
- The existing `ReadDriveStatus[Async]` sequence remains unchanged: LASAL axis
  status, `0x6041:0`, and `0x6061:0`. Adding the error-code API does not add a
  third SDO attempt to that composite.

No new TCP opcode, diagnostics capability bit, LASAL source, LASAL class, or
Network connection is required for this slice. Both object reads use the
existing `0x7E50` submit and `0x7E03` terminal-status path.

## Wire contract

`GetDriveErrorCode[Async]` fixes the SDO request to:

| Field | Value |
|---|---:|
| Slave reference | physical axis/slave `1..4` |
| Object index | `0x603F` |
| Sub-index | `0` |
| Value type | `UInt16` |
| Data length | `2` |
| Operation | Read |

The result bytes are interpreted as a little-endian unsigned 16-bit value.
`HasError` reports whether that returned value is nonzero. This is an
observation of object `0x603F`; it is not an automatic fault-reset decision.

The call keeps the existing D5 read admission contract:

- `SDORead` and `SDOReadGeneralInline` must both be advertised.
- `DiagnosticsBootId` and `MapRevision` must be nonzero and session-current.
- `MaxSdoDataBytes` must be at least 2.
- only physical slave references `1..4` are accepted.
- capability and identity rejection occurs before `0x7E50` transmission.
- accepted, rejected, outcome-uncertain, and terminal evidence use the existing
  `LMCDriveReadFailureContext` path.

## Important boundary

`LMCReadStatusResult.StatusWord` from command `0x2028` is not the DS402 status
word. The current LASAL `LMCControlCommandService` writes literal zero into that
reserved response field. It must not be used to decide whether DS402 Fault is
clear.

The three observations remain separate:

| Observation | Source | Meaning in this project |
|---|---|---|
| `AxisErrorId` / axis error flags | LASAL `_LMCAxis` status via `0x2028` | native LASAL axis error state |
| DS402 Fault | bit 3 of SDO `0x6041:0` | current drive state-word fault indication |
| Drive error code | SDO `0x603F:0` | current returned drive error-code object value |

Therefore the Axis Reset facade's stable `AxisErrorId == 0` result is still not
proof that the DS402 Fault bit and `0x603F` have cleared. Hardware qualification
must record all three observations before and after Reset.

## Verification boundary

2026-07-29 PC verification checkpoint (historical counts; see the current status document for current totals):

- SDK Debug full suite: `876/876 PASS`
- SDK Release full suite: `876/876 PASS`
- WPF Release actual-control smoke: `125/125 PASS`
- LASAL SourceOnly `Phase5TransportClean / StaticTopologyOnly`: PASS

PC tests can prove request bytes, result parsing, capability/identity zero-wire
gates, one-attempt behavior, and UI rendering. They do not prove that the live
drive returns the expected values. PLC/download and physical-axis packet capture
remain required.

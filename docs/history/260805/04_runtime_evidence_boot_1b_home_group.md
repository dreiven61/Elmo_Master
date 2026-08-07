# BootId 0x1B LMC Home and Group basic-motion runtime evidence

작성일: 2026-08-05 (KST)

## 1. Evidence identity

- WPF executable: `LasalMotionControlApiExample.exe`
- version: `0.9.1.0`
- build UTC: `2026-08-05 05:27:15`
- feature: `CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5`
- supplied log SHA-256: `5507B0938C46F49A9804F9D44C45EBFBC9AFBFDC3F2CC4583783403A335EF1A1`
- supplied log length: `15,239` bytes
- LASAL runtime identity:
  - `BootId=0x0000001B`
  - `MapRevision=0x957F101E`
  - `DiagnosticsBuild=1`
  - `DiagnosticsBits=0x000C633F`
  - `AdminFeatures=0x00000017`

This report preserves the exact conclusions extracted from the supplied WPF log and the live
`%TEMP%\Lasal2.log`. It does not turn an ACK into terminal proof and does not infer a physical
effect that the logs do not contain.

## 2. C78 rebuild and download

The canonical project was rebuilt twice in the LASAL session that started at 14:19:42.

| Time | Result | Evidence |
|---|---|---|
| 14:21:18-14:21:41 | PASS | C78/ARM, compiler errors `0`, warnings `55`, required TCP/Control/Diagnostics sources compiled |
| 14:22:14 | FAIL | Download command ended with `Timeout waiting CPU state` |
| 14:24:20 | FAIL | Second download command ended with `Timeout waiting CPU state` |
| 14:24:25-14:24:46 | PASS | C78/ARM, compiler errors `0`, warnings `55`, required TCP/Control/Diagnostics sources compiled |
| 14:26:42-14:26:49 | PASS | Canonical download and PLC link succeeded; LASAL reported `Download Ok` |

Both successful rebuilds had the same exact warning histogram:

- `W0069=35`
- `W0072=17`
- `W0073=3`

No `CInvalidArgException` appears after the 14:19:42 LASAL session start. However, there is no
`Searching implementation` record in that interval, so this session does not contain the required
three-class `Find in Implementation` smoke proof.

## 3. Four-axis LMC Home

All four axes completed on the same BootId and MapRevision. Every terminal record had
`RecordState=Succeeded`, `HomeSucceeded=True`, `OriginalStatus=0`, `OriginalErrorId=0`,
`OriginalDetail=0`, `AxisError=0`, `NativeCommandState=0`, `StopState=0`, and
`EvidenceFlags=0x0000003B`.

| Axis | Raw before | Raw after | Delta | Six application/internal coordinate fields | Record generation |
|---:|---:|---:|---:|---|---:|
| 1 | 8,028,421 | 8,028,421 | 0 | all `0` | 1 |
| 2 | 8,379,120 | 8,379,120 | 0 | all `0` | 2 |
| 3 | 8,383,547 | 8,383,548 | +1 | all `0` | 3 |
| 4 | 8,384,900 | 8,384,901 | +1 | all `0` | 4 |

The WPF prints `Read Home Status PASS` only after exact terminal materialization and exact
`0x7D19` retirement snapshot validation. The generation sequence `1 -> 4`, admission of every
next axis without detail `41`, and the subsequent `Identity Home Check PASS: 4/4 axes referenced`
close the previous stale-receipt/owner-release failure for this downloaded checkpoint.

This is runtime proof of the temporary `0x3B` SetPosition-only Home contract. Raw feedback is still
recorded but is not a success gate. It is not proof that the raw encoder count is physically fixed.

## 4. Group basic motion

The same BootId passed the following sequence:

1. Group `_LMCRobotBase1` load, reference `256`.
2. Identity Home Check `4/4`.
3. Group Power On with three consecutive `PowerOn=True` samples.
4. Set Identity Kinematics.
5. Group Enable with three consecutive powered Locked Standby samples.
6. Multiple absolute moves between raw coordinates `0` and `1,000,000` on X/Y/Z/U.
7. Non-standstill observation and stable Group InPosition completion on actual-distance moves.

All recorded Group moves completed naturally at their target. No Group Stop, Disable, or Power Off
appears before the supplied log ends. Therefore this evidence proves Group basic lifecycle and
motion, but it does not prove an actual in-motion Group Stop or the final safe shutdown sequence.

## 5. Encoder maintenance boundary

The log contains four Arm/Execute/Read Outcome PASS sequences for TEST ONLY Encoder Maintenance.
Those lines do not print the exact terminal record, SDO abort code, verification flags, DS402
`0x6041`, `0x603F`, AxisError, or a before/after physical effect. They prove that the command and
outcome-read path returned without a surfaced WPF failure; they do not qualify TW19 or TW20 physical
behavior.

## 6. Remaining gates after this checkpoint

- Run the missing three-class `Find in Implementation` smoke and confirm no new
  `CInvalidArgException` from the smoke start.
- Read the real per-axis DS402 `0x6041`, `0x603F`, `0x6061`, and AxisError on this or a newly frozen
  same-Boot checkpoint before additional mutation tests.
- Prove actual non-standstill Group Stop, then final Group Disable and Power Off stable readback.
- Prove single-axis motion and actual in-motion Stop, Axis1 first and then Axis2..4.
- Prove `AxisRebaseRequiredState` restart/power-loss retention separately.
- Keep DS402 Home and ordinary ownership gates `FALSE` until their remaining source semantic debt,
  C78 build, download, and dedicated runtime matrix are closed.
- Close the `PublishAxisOwnership` Result-unconsumed production caller debt before provider method
  splitting or ordinary ownership activation.

## 7. Test5 follow-up evidence

The later Test5 text/pcap set closes this report's open software-level Group Stop and shutdown
items for the same `BootId=0x1B` and `MapRevision=0x957F101E` checkpoint. Three genuine
non-standstill Group Stops each used one `0x2085` request and reached three consecutive
`0x40060000` standby samples. Group Disable and an initial Power Off were followed by three
Power On/Off cycles; the third cycle's Off is the final state. Every phase reached its expected
three-sample state without mutation replay.

The supplied per-axis read used only `0x2028`. It proves successful LASAL status and
`AxisErrorId=0` for Axis1..4, but leaves real DS402 `0x6041`, `0x6061`, and `0x603F` reads open.
See [Test5 runtime evidence](05_runtime_evidence_group_stop_safe_shutdown_and_axis_status.md) for
the hashes, packet counts, exact states, and evidence boundary.

# LASAL Motion Control API candidate package

Version: `LasalMotionControlLib 0.9.1-preview`
Runtime: Windows, .NET Framework 4.8

This directory is a transactionally assembled release candidate. It is not a
production-approved package and it does not replace the existing
`LMC_API_Distribution` directory.

## Safety and evidence boundary

- Motion and Power APIs transmit real PLC commands when a connection owns a
  valid current-session axis or group handle and the application is not in a
  recovery quarantine.
- An ACK proves command acceptance only. Confirm completion with typed status
  polling, stable samples, and the final position or state readback described
  in the manual.
- `Close`, `Dispose`, timeout, and cancellation do not issue Motion Stop.
- The DLL does not convert engineering units. The caller must convert physical
  values to the PLC application UNIT before passing DINT values.
- Current PC and LASAL static/build checks are not current PLC or hardware
  proof. Fresh PLC download, safety-chain approval, command captures, and final
  physical readback remain required before production use.

## Preview feature scope

- Axis 1 SDO Write is limited to Gold UI[24], exact target
  `Slave 1 / 0x2F00:24 / Int32 / 4 bytes`. The example enables manual Write
  only after an exact current-session Build/BootId/MapRevision four-ticket
  same-value qualification. Identity drift or disconnect revokes that proof,
  and an uncertain Write is never replayed automatically.
- Axis 2..4 SDO Write and every non-approved SDO target remain blocked.
- Dynamic node health and digital I/O capability bits 15..17 remain off.
  PLC command `0x7E23` is absent and digital output Write is unsupported.
- PI Write and D4 Double Recorder remain off.

## Contents

| Directory | Contents |
|---|---|
| `01_API` | `LasalMotionControlLib.dll` for client applications |
| `02_Example_Program` | Binary-reference WPF source, solution, and Run files |
| `03_API_User_Manual` | Korean DOCX source and matching PDF |

`RELEASE_MANIFEST.md` records the source commit, dirty/clean preview state,
release-input and semantic-policy hashes, DLL identity, and every shipped file
hash. A `dirty-preview` manifest is evidence for an integration candidate only;
it is not production approval.

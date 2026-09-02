# HomeDS402 Method 37 operator activation implementation — 2026-09-02

## Conclusion

The WPF screen was not usable for DS402 Home because the connected PLC image did
not advertise the Admin HomeDS402 capability and the UI did not expose the homing
method as a selectable value. This was not an operator usage error.

The current implementation supports only DS402 homing method 37:

- no axis search motion
- Home offset fixed at 0
- current actual position is defined as 0
- method selector contains exactly Method 37

Moving switch/index homing methods 1..34 are not implemented for use. Their
`HomeDS402Ex` runtime remains disabled because Axis1/2 wiring, polarity, debounce,
direction, travel limit, scale, and approved method lists are not qualified.

## Implemented changes

### WPF

- Added a qualified homing-method selector to the DS402 Home section.
- Limited the selector to Method 37.
- Added an explicit availability message for connection, capability refresh,
  missing PLC capability, confirmation, and armed states.
- Preserved the durable no-replay rule: an uncertain Start is recovered only by
  exact `0x7D16` query and `0x7D17` retire.

### LASAL source

The five HomeDS402 activation values are atomic ON:

- TCP ordinary ownership gate: ON
- Control ordinary ownership gate: ON
- Diagnostics HomeDS402 runtime gate: ON
- InputLatch startup sweep gate: ON
- Admin HomeDS402 feature bit 6: ON (`0x00000757`)

Admin `PhysicalAxisCount` is 2. DS402 Home Start also rejects Axis3/4 through the
configured physical-drive mask before it can arm a durable journal or execute.

Diagnostics operational capability remains `0x0000613F`. Its bit 6 is
RecorderDoubleBank, not HomeDS402. The activation verifier was corrected to read
the Admin capability bit instead.

### Adjacent regression integrity

Shared ordinary ownership is now ON for HomeDS402, but SetPosition remains dormant:

- durable Store configured gate OFF
- Axis1..4 maximum jump values 0
- SetPosition Admin capability bits OFF
- no authorized native SetPosition call

The Diagnostics error catalog was completed for existing detail codes 43 and 44,
and the unknown-code test boundary moved to 45.

### Stale recovery retirement bug fix

An identity-mismatched DS402 Home record entered read-only quarantine but could not
be retired because `MaintenanceActionRecoveryJournal` was absent from the common
retirement registry. The UI therefore showed `Active durable recovery records: none`
even though the Home record remained active.

The maintenance journal is now included in:

- stale-record metadata and UI listing
- exact durable source-byte evidence capture
- immutable retirement-ledger commit
- post-commit exact-source CAS resolve
- startup crash-finalization of a committed decision

Retirement still sends no Home, motion, power, SDO, replay, or cleanup command. It
only archives the original local journal bytes and marks that exact stale local
record Resolved after operator confirmation.

## Static and PC verification

- topology verifier: 154 checks PASS; HomeDS402 source activation atomic ON
- H37 activation verifier: 46 checks PASS; Admin mask `0x00000757`
- H37 ownership verifier: 21 checks PASS
- H37 method-size verifier: 10 checks PASS
- H37 WPF durable recovery verifier: 36 checks PASS
- H37 current-dev top-level verifier: 18 checks PASS
- SetPosition current-source inventory: 39 checks PASS
- API Release tests: 1200/1200 PASS
- WPF Release smoke: 398/398 PASS

These are source/static and PC test results. They are not LASAL compile, PLC
download, PLC runtime, packet, physical-axis, or production PASS.

## Operator procedure after this change

> `LMC Home (0x7D13)` is not Servo On. It is the current-position-zero command that clears the retained rebase barrier for the selected axis. A fresh/retained `AxisRebaseRequiredState` may therefore reject WPF `Power On (0x2023)` even when direct LASAL PowerOn works. The adapter reports this as `ErrorId=-15 (AxisRebaseRequired)` instead of generic ownership conflict.

Required test order when the selected physical axis still has the rebase bit set:

```text
PowerOff + Standstill
-> exact LMC Home 0x7D13
-> terminal success + exact retire
-> Power On 0x2023
-> stable PowerOn proof
-> HomeDS402 Method 37 test
```

Do not clear the retained word manually and do not bypass the barrier in PowerOn.

1. In LASAL IDE, rebuild/link the tracked project and confirm 0 errors.
2. Download that exact image to the PLC and capture its Build/BootId/MapRevision.
3. Close the previously running WPF process and start a build containing this change.
4. Connect, refresh Home capability, and confirm:
   - Admin HomeDS402 capability is available
   - PhysicalAxisCount is 2
   - target is Axis1 for the first run
5. Confirm the physical axis is PowerOff, Standstill, and position-stable.
6. Select Method 37, keep Home offset 0, enter the timeout, and check the one-shot
   confirmation box.
7. Execute DS402 Home exactly once.
8. Use Home Status Read to query the exact outcome, then retire the exact terminal
   record. Do not submit Start again when the result is unknown.

Expected semantic effect: the axis does not search or move; its current actual
position becomes 0. If switch/index search motion is required, stop here and qualify
the `HomeDS402Ex` hardware profile before implementation/activation.

## Remaining evidence

- fresh LASAL C78/ARM rebuild/link and generated-artifact identity
- exact PLC download/image identity
- Axis1 normal run and failure matrix
- Axis2 qualification
- Axis3/4 deterministic nonphysical rejection at runtime
- packet and physical/online status evidence

Until these are complete, production posture remains NO-GO.

# SetOperationMode multi-mode qualification target

This file records the software target that is intended to be merged to `dev` before a separate hardware-test activation branch is created.

## Software target

- DS402 modes exposed by the SetOperationMode software path: PP(1), PV(3), IP(7), CSP(8)
- Homing(6) remains owned by HomeDS402/HomeDS402Ex and is not a public SetOperationMode target.
- AdminCapabilities advertises `SetOperationModeSupportedMask` from the existing final UInt16 slot.
- Supported software mask: `0x018A` (bits 1, 3, 7, 8).
- WPF selector is populated only from `PLC advertised mask ∩ {PP, PV, IP, CSP}`.
- SDK Prepare fails before wire dispatch when the requested mode is not currently advertised.

## Production boundary

The merge candidate remains dormant in production source:

- `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE`
- Admin capability bits 8/9/10 = OFF (`0x00000017`)
- therefore `SetOperationModeSupportedMask = 0`

This dormant state is intentional and is not a regression. Hardware testing must use a separate qualification-only branch created from the exact merged `dev` commit, where only the paired SetOperationMode activation gates are enabled.

## Qualification branch expectation

The hardware-test branch must advertise these together:

- Admin Start/OutcomeRead/OutcomeRetire bits 8/9/10 ON (`0x00000717`)
- `LMC_DIAG_SET_OPERATION_MODE_ENABLED = TRUE`
- `SetOperationModeSupportedMask = 0x018A`

After rebuilding/linking and loading that exact image, WPF capability refresh is expected to show `AdminTriad=True` and `SupportedModeMask=0x018A`, enabling PP/PV/IP/CSP selections subject to the existing diagnostics identity, journal, owner, state, and confirmation interlocks.

No hardware PASS or production activation is claimed by this document.

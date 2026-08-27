# SetOperationMode MODE-11 Bench Precondition Modes

Status: **QUALIFICATION BRANCH ONLY / DO NOT MERGE INTO `dev`**

Branch: `codex/setopmode-mode11-bench-activation`

## Purpose

MODE-11B requires an independently prepared non-CSP starting state so the return-to-CSP path can prove one and only one one-byte `0x6060:0=08` write. The original qualification UI exposed only CSP(8), which made that precondition impossible to create from the test application when the drive was already in CSP.

This branch therefore exposes an explicit qualification-only target selector.

## Allowed targets

| UI target | DS402 mode value | Qualification use |
|---|---:|---|
| Profile Position (PP) | 1 | non-CSP bench precondition |
| Profile Velocity (PV) | 3 | non-CSP bench precondition |
| Interpolated Position (IP) | 7 | non-CSP bench precondition |
| Cyclic Synchronous Position (CSP) | 8 | normal MODE-11 target / return-to-CSP proof |

All other mode values remain rejected. In particular, **Homing(6) is not exposed by SetOperationMode**; homing remains owned by HomeDS402/HomeDS402Ex.

## Safety boundary

PP/PV/IP are **bench-precondition states only**. Keep the selected axis standstill and operation-disabled while preparing the precondition. Do not execute ordinary motion in PP/PV/IP from this qualification workflow. Return the axis to CSP(8) before motion testing.

The existing SetOperationMode safety contract remains in force:

- explicit one-shot confirmation before Start;
- durable recovery identity is armed before the write boundary;
- accepted or uncertain Start is never replayed;
- `0x6060` mutation remains exactly one byte;
- non-same-mode mutation still requires standstill, DS402 Fault clear, and Operation Enabled clear;
- recovery remains read-only with respect to `0x6060`;
- generic D5 continues to deny `0x6060` writes.

## Runtime semantics

The requested mode stored in the durable SetOperationMode record is now the execution truth for this qualification branch:

- same-mode detection compares `0x6061` against the retained requested mode rather than literal CSP(8);
- `0x6060` write data is the retained requested mode rather than literal `8`;
- warm recovery restores the retained requested-mode identity;
- terminal success still requires the observed mode to match the retained requested mode.

This preserves the original CSP behavior while allowing PP/PV/IP only for MODE-11 bench preparation.

## Production boundary

Production `dev` must remain SetOperationMode activation OFF with Admin bits 8/9/10 OFF. This document and the broader mode allow-list are qualification evidence only and do not approve MODE-14 production activation.

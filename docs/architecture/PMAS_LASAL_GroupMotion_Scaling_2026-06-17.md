# PMAS/LASAL Group Motion Scaling

Date: 2026-06-17

## Background

Codex_PMAS_WPF uses PMAS encoder-count style motion values for Group Motion tests. Codex_LASAL_WPF uses LASAL displayed units in the UI, matching the existing SingleAxis behavior, and scales those displayed units before sending them to the LASAL dummy TCP/IP backend.

Observed symptom:

- Sending Group endpoint `1000,2000,3000,0` appeared in LASAL visualization as `0.10,0.20,0.30,0.00`.
- This proves the LASAL Group frame field was being interpreted as LASAL internal units, where `10000` internal units equals `1.00` displayed LASAL unit.

## Scaling Rule

The mechanical ratio supplied for this test setup is:

- `8388608` counts, or 23-bit count range, equals `360` LASAL displayed units.
- `1` LASAL displayed unit equals `10000` LASAL internal units.

Therefore PMAS-count defaults are converted to Codex_LASAL_WPF UI values by:

```text
lasal_ui_unit = pmas_count * 360 / 8388608
```

Then Codex_LASAL_WPF converts its UI values to LASAL internal frame values by:

```text
lasal_internal = lasal_ui_unit * 10000
```

Examples:

| PMAS count input | Codex_LASAL_WPF UI value | LASAL internal frame value |
| ---: | ---: | ---: |
| 8388608 | 360 | 3600000 |
| 16777216 | 720 | 7200000 |
| 25165824 | 1080 | 10800000 |

## Implementation

The UI-to-frame conversion is applied in:

- `Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs`
- `MMCGroupAxis.BuildMoveLinearAbsoluteExFrame`

Converted fields, using `lasal_ui_unit * 10000`:

- Position vector
- Velocity
- Acceleration
- Deceleration
- Jerk

Unconverted fields:

- Transition params
- Buffered mode
- Coordinate system
- Transition mode
- Superimposed
- Execute flag

Reason: transition parameters are mode-specific command parameters, not guaranteed to be PMAS position-count values.

## UI Defaults

Codex_LASAL_WPF Group tab defaults are LASAL UI-unit equivalents of Codex_PMAS_WPF count-based defaults:

- Endpoint: `360,360,360,360`
- Velocity: `360`
- Acceleration/Deceleration: `360000`
- Jerk: `360000000`

Codex_LASAL_WPF also now includes `Cycle Test Group1`, copied from Codex_PMAS_WPF and adapted for LASAL's available Cycle Test tabs.

## Test Expectation

When the Group tab sends endpoint `360,360,360,360`, the TCP frame should contain internal values `3600000,3600000,3600000,3600000`, and LASAL visualization should show approximately:

```text
360.00, 360.00, 360.00, 360.00
```

If LASAL still shows `0.10` style values, the command path is bypassing `MMCGroupAxis.MoveLinearAbsoluteEx`, the UI is still using old raw values such as `1000`, or the target is not running the updated Codex_LASAL_WPF binary.

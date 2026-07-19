# Cycle Test Group1 Design

## Reviewed Markdown Inputs

- `Codex_PMAS_WPF/API_MAPPING.md`
  - Confirms `Codex_PMAS_WPF/PmasApiWpfTestApp.sln` is the WPF test app.
  - Confirms implemented Group Motion APIs include `MMC_GroupReadStatusCmd`, `MoveLinearAbsoluteEx`, `GroupEnable`, `GroupStop`, member status, and Cartesian transform setup.
- `test/Reports_Lasal/CycleResults/ReadActualPosition_Latency_Compare_20260415_PMASSamples.md`
  - Existing latency logs measure app-side call duration, so `GroupReadStatus` latency must be recorded with the same `Stopwatch` start/end around the managed API call.

## API Facts Used

- `MMCGroupAxis.GroupReadStatus(ref ushort usGroupErrorID)` returns `uint`.
- PMAS group status bits include:
  - `NC_GROUP_STANDBY = 0x00020000`
  - `NC_GROUP_MOVING = 0x00002000`
  - `NC_GROUP_STOPPING = 0x00001000`
  - `NC_GROUP_ERROR = 0x00004000`
- The Maestro sample code waits for group completion with:
  - `(Group.GroupReadStatus() & NC_GROUP_STANDBY_MASK) == NC_GROUP_STANDBY_MASK`
- Therefore the default Group1 in-position/complete condition is `0x00020000`, exposed as an editable mask in the UI.

## UI

- Add a new tab named `Cycle Test Group1`.
- Inputs are group-level:
  - P1, P2, P3, P4 point vectors, parsed as double arrays and padded to 16 positions.
  - Velocity, acceleration, deceleration, jerk.
  - Buffered mode, coordinate system, transition mode, transition parameters, superimposed.
  - Cycle count, warmup cycles, move timeout, poll interval, stable samples, drop threshold, group in-position mask.
  - Stop on timeout, stop on group error/exception, high-priority worker, high-precision wait, 1 ms timer resolution.
- Result area follows the existing Cycle Test tabs:
  - Progress bar, run status, summary text, save folder, auto-save, save button.

## Motion Flow

- Each measured cycle issues this point sequence:
  - `P1 -> P2 -> P3 -> P4 -> P1`
- Normal mode:
  - Issue one `MoveLinearAbsoluteEx`.
  - Poll `GroupReadStatus`.
  - Treat the point as reached when `(status & inPositionMask) == inPositionMask` for the requested stable sample count and `groupErrorId == 0`.
  - Then issue the next point.
- Warmup cycles run first and are not counted in measured success totals.

## Blending / Transition Test Plan

- Waiting for `NC_GROUP_STANDBY` after every point intentionally prevents path blending, because the next command is not queued until the previous move has completed.
- To test blending, add `Queue points for transition/blending` mode:
  - Use a non-aborting buffered mode such as `MC_BUFFERED_MODE`, `MC_BLENDING_LOW_MODE`, `MC_BLENDING_PREVIOUS_MODE`, `MC_BLENDING_NEXT_MODE`, or `MC_BLENDING_HIGH_MODE`.
  - Use a non-none transition mode such as `MC_TM_CORNER_DISTANCE_MODE`, `MC_TM_MAX_CORNER_DEVIATION_MODE`, or another controller-supported mode.
  - Queue `P1, P2, P3, P4, P1` without per-point `GroupReadStatus` waits.
  - Poll `GroupReadStatus` only after the final queued point until the final `NC_GROUP_STANDBY` mask is observed.
- This keeps the default mode aligned with in-position verification while giving a practical way to apply and compare blending.

## Logging

- Create a `GroupStatusReadSamples` sheet for the new test.
- Each `GroupReadStatus` poll records:
  - Sample index, cycle index, point/phase.
  - Group error ID.
  - Raw group status as hex.
  - In-position result.
  - Stable counter.
  - Read start/end offset from test start.
  - Read latency in ms.
- The result sheet records:
  - Group name, remote IP.
  - All motion parameters and transition settings.
  - Attempted/success cycles, timeout/error counts, drop count.
  - `GroupReadStatus` latency average/max.
  - Poll period average/max.
  - Captured/dropped sample counts.

## Files To Change

- `PmasApiWpfTestApp/MainWindow.xaml`
  - Add `Cycle Test Group1` tab and controls.
- `PmasApiWpfTestApp/MainWindow.xaml.cs`
  - Initialize Group1 combo boxes.
- `PmasApiWpfTestApp/MainWindow.CycleTestGroupOperations.cs`
  - New partial class file for Group1 options, metrics, execution, status polling, UI updates, and XLSX export.
- `PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs`
  - Prevent axis cycle tests and the group cycle test from running concurrently.
- `PmasApiWpfTestApp/PmasApiWpfTestApp.csproj`
  - Include the new code file.

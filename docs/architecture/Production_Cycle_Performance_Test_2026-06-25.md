# Production Cycle Performance Test

Date: 2026-06-25

## Purpose

This test implements the performance comparison proposed by Jonas Drager.

The result to compare is not a single TCP latency number. The result is the total elapsed time needed to complete many realistic short production cycles on both systems.

The measured cycle model is:

```text
one part cycle =
  send move command P1 -> P2
  + axis movement
  + done check by actual-position polling
  + optional forward actor delay
  + send move command P2 -> P1
  + axis movement
  + done check by actual-position polling
  + optional return actor delay
```

The actor delays model non-motion time such as vacuum drop, gripper open, or settling time.

## Implementation

The existing single-axis `Cycle Test` tab was extended in both WPF programs:

- `Codex_PMAS_WPF/PmasApiWpfTestApp`
- `Codex_LASAL_WPF/PmasApiWpfTestApp`

New UI inputs:

- `Forward Actor Delay (ms)`
- `Return Actor Delay (ms)`

New result fields:

- `TotalElapsed(ms)`
- `AveragePartTime(ms)`
- `Throughput(parts/min)`
- `CommandLatencyAvg(ms)`
- `CommandLatencyMax(ms)`
- `ResponseLatencyAvg(ms)`
- `ResponseLatencyMax(ms)`

The command latency is measured around the synchronous `MoveAbsolute`/`MoveAbsoluteEx` call. The response latency is measured around each `GetActualPosition` call used for done checking.

## Comparison Rules

Before comparing PMAS and SIGMATEK results, these inputs must be equivalent:

- Same axis and mechanical load.
- Same effective movement distance.
- Same velocity, acceleration, deceleration, and jerk.
- Same in-position tolerance.
- Same polling interval and stable sample count.
- Same actor delays.
- Same warm-up cycle count.

If a motion profile cannot be made exactly identical, the known motion-time difference must be recorded before interpreting the total elapsed result.

## Current Done Check

The implemented done check is actual-position polling:

```text
abs(target position - actual position) <= in-position tolerance
```

This is intentionally shared by PMAS and SIGMATEK WPF tests because both sides already support it through the existing single-axis cycle test path.

## Result Interpretation

Use the following result hierarchy:

1. `TotalElapsed(ms)` for the full test.
2. `AveragePartTime(ms)` for normalized comparison.
3. `Throughput(parts/min)` for production interpretation.
4. `CommandLatency*` and `ResponseLatency*` only as diagnostic support.

Do not conclude that a system is faster from command latency alone. Jonas's requested comparison is the total production-cycle time under the same motion and actor-delay assumptions.

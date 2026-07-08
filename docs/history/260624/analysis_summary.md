# Elmo_Master 2026-06-24 history analysis summary

Source: `docs/history/Elmo_Master_history_260624.md`

Split files:

- `docs/history/260624/index.md`
- `docs/history/260624/Elmo_Master_history_260624_part_01.md` through `part_10.md`

## Purpose

This summary is the working handoff for continuing this thread without rereading the full 2400-line history file.

Facts below are taken from the split history files and a current repository check performed after the split.

## Current Repository State Checked After Split

- Current repo: `C:\work\Elmo\Elmo_Master`
- Current branch at check time: `main`
- Current `HEAD`: `c97bc4e dev : Commit`
- `HEAD` and `origin/main` pointed to the same commit at check time.
- `git status --short` showed only untracked paths:
  - `data_capture/`
  - `docs/api_analysis/`
  - `docs/history/260624/`
  - `docs/history/Elmo_Master_history_260624.md`
- The API analysis documents exist locally under `docs/api_analysis/`, but the history says they had not been committed yet.

## Part Map

| Part | Source lines | Main content |
|---:|---:|---|
| 01 | 1-250 | Prior `260617` split summary, CREVIS/Beijer XML finding, first folder-based commits, start of PMAS `Cycle Test Group1` implementation |
| 02 | 251-500 | PMAS `Cycle Test Group1` completion, defaults/CRLF fixes, PMAS commits, LASAL network connection shape cleanup, first PMAS MoveLinearAbsoluteEx packet analysis |
| 03 | 501-750 | Clarification that Group tab sends extra Actual/Target reads because app code adds snapshots, true Ex vs non-Ex command distinction, LASAL coding rules work initially done in wrong SEMICS repo |
| 04 | 751-1000 | Correcting repo mistake by applying LASAL rules to `Elmo_Master`, then starting Group Motion TCP implementation from `MoveLinearAbsoluteEx` and `GroupReadStatus` captures |
| 05 | 1001-1250 | LASAL TCP implementation summary, initial scaling problem, LASAL WPF Group Test port from PMAS, Sigmatek capture comparison begins |
| 06 | 1251-1500 | Sigmatek capture conclusion, final LASAL scaling correction, three folder-based commits, start of SNET vs Maestro PDF/API analysis |
| 07 | 1501-1750 | PDF extraction attempts and switch to `pdftotext` |
| 08 | 1751-2000 | API table extraction scripts and generation plan |
| 09 | 2001-2250 | Generated analysis document content construction for SNET, Maestro, and comparison |
| 10 | 2251-2400 | Final API analysis result and note that `docs/api_analysis/` plus unrelated `data_capture/` are untracked |

## Confirmed Work Already Done

### 2026-06-17 history split

- `docs/history/260617/` was created earlier.
- It contains `index.md`, `analysis_summary.md`, and `Elmo_Master_history_260617_part_01.md` through `part_10.md`.
- Earlier conclusion from that summary:
  - Beijer V16 XML bundle is a valid ZIP and contains `GL-9086`, `GT-12FA`, and `GT-22BA`.
  - Beijer XML Vendor ID is `0x00000755`.
  - CREVIS `idx2749` and `idx3466` downloads were HTML error responses, not ZIP/XML.
  - Use Beijer XML only if the actual EtherCAT scan Vendor ID is `0x00000755`; if it is CREVIS Vendor ID such as `0x0000029D`, a CREVIS-native ESI is still needed.

### PMAS WPF Group Cycle Test

- `Codex_PMAS_WPF` received a new `Cycle Test Group1` tab and supporting code.
- Key files from history:
  - `Codex_PMAS_WPF/CycleTestGroup1_Design.md`
  - `Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml`
  - `Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestGroupOperations.cs`
  - `Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs`
- Default completion/in-position condition is `NC_GROUP_STANDBY` mask `0x00020000`.
- `GroupReadStatus` latency is logged to a `GroupStatusReadSamples` sheet.
- Normal mode waits for `GroupReadStatus` after each point, so it is not a blending test.
- Blending/transition testing is through queue mode: send P1, P2, P3, P4, P1 first, then wait for final standby.
- PMAS WPF build was verified with VS2019 MSBuild after implementation.

### PMAS Group Packet Analysis

- Actual/target group position reads seen around a move were caused by app code snapshots, not proof that the DLL splits a move into multiple commands.
- The Group tab path at that time called `ReadGroupPositionSnapshot` before and after the move.
- Those snapshots call:
  - `GroupReadActualPosition`, command ID `0x2051`
  - `GroupReadTargetPosition`, command ID `0x213B`
- The button label `MMC_MoveLinearAbsoluteCmd` was misleading in the analyzed code path:
  - Actual captured move command was `MoveLinearAbsoluteEx`, command ID `0x20A4`.
  - True non-Ex `MoveLinearAbsoluteCmd` is command ID `0x2043`.
- `MoveLinearAbsoluteEx` request length in the capture was 312 bytes.
- Non-Ex `MoveLinearAbsoluteCmd` was expected to use a smaller payload, with float-sized velocity/acc/dec/jerk and transition parameters.
- Move ACK means command accepted, not motion complete. Completion must be checked by status/position polling.

### LASAL Rules And Repo Correction

- A LASAL rules/ignore cleanup was first committed in the wrong repo, `C:\Users\dreiv\source\repos\SIGMATEK\SEMICS`.
- That mistake was acknowledged.
- The same kind of rules were then applied to the correct repo, `C:\work\Elmo\Elmo_Master`, and committed as:
  - `a461d8b docs(lasal): add coding rules and update 4-axis project`
- Relevant files added/updated in `Elmo_Master`:
  - `AGENTS.md`
  - `README.md`
  - `.gitignore`
  - `.gitattributes`
  - `docs/architecture/SIGMATEK_LASAL_coding_rules.md`
  - `docs/architecture/SIGMATEK_LASAL_programming_method_study.md`

### LASAL Group Motion TCP Implementation

- Packet captures confirmed:
  - `MoveLinearAbsoluteEx` command ID: `0x20A4`
  - `GroupReadStatus` command ID: `0x2045`
- `Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` was extended to handle these commands.
- LASAL implementation details from history:
  - Parse 16 double position slots from the TCP frame.
  - Map only the first 9 positions to `_LMCPROF_POS.Pos1..Pos9`.
  - Call `LMCRobot.MoveLinearCoord(...)`.
  - For `GroupReadStatus`, return base state `0x40000000`.
  - OR `0x00020000` only when `LMCRobot.AxInPosition(... PositionWindow:=0)` returns nonzero.
- `Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs` was changed so LASAL WPF sends real TCP frames:
  - `GroupReadStatus`: 16-byte `0x2045` request and 20-byte response parsing.
  - `MoveLinearAbsoluteEx`: 312-byte `0x20A4` request.
  - Move ACK is read immediately so the next `GroupReadStatus` does not consume a stale ACK.
- LASAL IDE build/download could not be verified from CLI.

### Final LASAL Scaling Decision

The first scaling implementation treated LASAL WPF Group inputs as PMAS counts. That was corrected.

Final confirmed rule:

```text
Codex_PMAS_WPF 8,388,608 counts == Codex_LASAL_WPF 360 displayed units
Codex_LASAL_WPF 360 displayed units -> TCP frame 3,600,000 internal units
```

Therefore:

- LASAL WPF Group UI values are LASAL displayed units.
- TCP frame internal value is `LASAL WPF input * 10000`.
- Do not apply PMAS-count-to-LASAL conversion inside the LASAL WPF send path.
- `PmasCountToLasalInternalUnitScale` was removed from the final implementation.
- `docs/architecture/PMAS_LASAL_GroupMotion_Scaling_2026-06-17.md` documents this rule.

Current file check after split confirmed:

- `SigmatekTcpIpDummyMMCLib.cs` contains `LasalInternalUnitsPerUnit = 10000.0`.
- `MainWindow.xaml` LASAL Group defaults include:
  - Group endpoint `360,360,360,360`
  - Group velocity `360`
  - Group acceleration/deceleration `360000`
  - Group jerk `360000000`
  - Cycle Group P2 `720,360,360,720`
  - Cycle Group P3 `360,720,720,360`
  - Cycle Group velocity `3600`

### Sigmatek Capture Result Before Final Follow-Up

The analyzed Sigmatek capture showed:

- Move frame structure and basic scaling path were working.
- `GroupReadStatus` request matched the expected `0x2045` frame.
- Sigmatek response was `0x40000000`, not `0x40020000`.
- With default mask `0x00020000`, `(status & 0x00020000) == 0x00020000` is false.
- Cause in current LASAL logic: `LMCRobot.AxInPosition(... PositionWindow:=0)` returned 0.
- `PositionWindow:=0` may be too strict, but this is not yet proven by a later capture.

Next technical validation from history:

1. Run/capture `MMCMoveLinearAbsoluteExCmd`, not the simple wrapper path.
2. Poll `GroupReadStatus` after motion completion.
3. Confirm whether response becomes `0x40020000`.
4. If it stays `0x40000000`, consider changing LASAL `AxInPosition` `PositionWindow:=0` to a realistic tolerance window and document the chosen value.

### Folder-Based Commits Already Made

Earlier commits in the history include:

- `5aac565 feat(pmas): add group motion and PI bulk tooling`
- `667deff docs: add Maestro API and history references`
- `95d6d00 docs(output): add extracted Maestro and EtherCAT artifacts`
- `943d3c5 test: add cycle capture and motor parameter artifacts`
- `b921107 feat(pmas): add group motion cycle test`
- `363b25a test(pmas): add group cycle result captures`
- `5d40d31 feat(ethercat): add 4-axis test project`
- `a461d8b docs(lasal): add coding rules and update 4-axis project`
- `a201192 Update LASAL WPF group motion test`
- `7ac179b Document group motion TCP protocol`
- `646a290 Update Elmo EtherCAT LASAL project`

Current `git log` after split also shows a later `HEAD` commit:

- `c97bc4e dev : Commit`

Do not assume the old "ahead of origin by 11" statement is still true. At the current check, `c97bc4e` was both `HEAD` and `origin/main`.

### SNET vs Maestro API Analysis

The user requested analysis of:

- `C:/work/자료/EMotion/SNET-ECAT-User-Manual-25.05.08-ko/SNET-ECAT User Manual 25.05.08 ko/Chapter6_Library_(250508).pdf`
- `Maestro Administrative and Motion API_2022_12_v2.012.pdf`

Generated documents:

- `docs/api_analysis/SNET_ECAT_Library_API_Analysis_2026-06-23.md`
- `docs/api_analysis/Maestro_Administrative_Motion_API_Analysis_2026-06-23.md`
- `docs/api_analysis/SNET_ECAT_vs_Maestro_API_Comparison_2026-06-23.md`

Main conclusions:

- The two PDFs do not provide same-condition quantitative latency/throughput benchmark data.
- Performance comparison in the documents is structural, based on controller-side execution, PC round-trip reduction, group/path handling, trigger/capture specificity, and PI/Bulk Read support.
- Maestro is stronger or directly supported for:
  - Group objects and `GroupReadStatus`
  - Transition/blending
  - Kinematics transforms
  - PVT, ECAM, Gear
  - Process Image, Bulk Read, Recording, Events
  - Broad EtherCAT/CANbus/DS401/EIP/FoE/admin functions
- SNET is stronger or more direct for:
  - Trigger output
  - Capture/latch
  - Dedicated SNET/RTEX/SNET-ECAT I/O, ADC/DAC areas
  - Explicit gantry sync/homing functions
- Current Group Motion P1-P4-P1, transition mode, and group in-position tests are Maestro API model work, not SNET API model work.

## Current Continuation Points

Use these before doing more work:

1. Check `git status --short` before editing. Current untracked folders include history split output and API docs.
2. If asked to commit the latest local artifacts, decide whether to include:
   - `docs/history/Elmo_Master_history_260624.md`
   - `docs/history/260624/`
   - `docs/api_analysis/`
   - `data_capture/` only if the user explicitly wants it and its contents are reviewed.
3. For Group Motion behavior, the next real technical task is not another scale change. It is a capture/validation task:
   - Use `MMCMoveLinearAbsoluteExCmd`.
   - Check full Ex parameters in the TCP frame.
   - Check whether `GroupReadStatus` reaches `0x40020000`.
   - If not, inspect/change LASAL `AxInPosition` tolerance.
4. Do not confuse these APIs:
   - `0x20A4`: `MoveLinearAbsoluteEx`
   - `0x2043`: true non-Ex `MoveLinearAbsoluteCmd`
   - `0x2045`: `GroupReadStatus`
   - `0x2051`: `GroupReadActualPosition`
   - `0x213B`: `GroupReadTargetPosition`
5. Do not reintroduce PMAS-count scaling inside `Codex_LASAL_WPF` Group send path. The final current rule is LASAL UI units times `10000`.

# Elmo Master history analysis summary - 260526

Source:
- `C:\work\Elmo\Elmo_Master\docs\history\Elmo_Master_history_260526.md`

Split output:
- `C:\work\Elmo\Elmo_Master\docs\history\260526\index.md`
- 3 analysis chunks under `docs\history\260526`
- 8 embedded base64 images extracted to `docs\history\260526\assets`

## Current continuation point

The last unresolved user request in the history is:

> `Maestro Administrative and Motion API_2022_12_v2.012.pdf` has broken bookmark links. Reconnect the PDF bookmarks.

This task did not fail because the PDF operation is impossible. It stopped after a context compaction/system error. The useful facts already found are:

- Source PDF: `C:\work\Elmo\Elmo_Master\Maestro Administrative and Motion API_2022_12_v2.012.pdf`
- PDF page count observed in the prior attempt: 2435 pages.
- Existing outline/bookmarks observed: 895 items.
- Broken destination pattern: about 870 of 895 bookmarks point to page 1.
- Intended repair approach: keep the existing bookmark titles/tree where possible, but remap destinations by matching `Chapter N` and numbered section titles like `x.y.z` to actual extracted text pages.
- Previous investigation reached `PdfWriter.add_outline_item` signature checking, but did not produce the repaired PDF.

## Project context

Main project areas:

- `Codex_PMAS_WPF`: WPF/.NET Framework 4.8 PMAS test application derived from Elmo MMCLibDotNET examples.
- `Codex_LASAL_WPF`: LASAL/Sigmatek-facing TCP dummy/test application that sends Elmo-like motion frames directly.
- `packet_capture`: Elmo/Sigmatek packet captures, CSV timelines, and analysis reports.
- `profile_capture`: motion profile traces and comparison artifacts.
- `docs`: API summaries, packet-return guides, integrated analysis notes.

Important generated API overview files currently visible as untracked files:

- `docs\Maestro_API_Function_Overview_2022_12_v2_012.*`
- `docs\Maestro_API_Function_Overview_KO_2022_12_v2_012.*`
- `docs\Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.*`
- `docs\Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.*`

The CoreAPI versions are the corrected API overview set. The earlier 709-item versions include Chapter 24 programming/wrapper material and should not be treated as the final API-only summary.

## Protocol facts to preserve

Final packet interpretation from later analysis overrides earlier intermediate notes:

- `MoveAbsoluteEx`
  - Command ID: `0x209F`
  - TCP request payload starts little-endian as `9f 20`
  - Request length: 64 bytes total in the captured payload.
  - Response length: 16 bytes.
  - Response meaning: default function-block output, not position data.
  - Important response fields from observed payload:
    - payload size around offset 2: `0x0008`
    - FB handle at offset 8
    - status at offset 12
    - error ID at offset 14
  - Completion must be checked separately using `ReadStatus` or `ReadActualPosition`.

- `ReadActualPosition`
  - Command ID: `0x202E`
  - TCP request payload starts little-endian as `2e 20`
  - Request length: 9 bytes in observed captures.
  - Response length: 24 bytes in repeated captures.
  - Response structure observed consistently:
    - 8-byte fixed/header area
    - `double` at offset 8: position
    - `double` at offset 16: auxiliary value, likely profile velocity or related setting
  - Any older note claiming the final response is 20 bytes is stale or incomplete.

- `ReadStatus`
  - Used after `MoveAbsoluteEx` to infer motion start.
  - Observed response length: 20 bytes.
  - Key field: `uiState` / status transition.
  - Motion start was inferred from first `ReadStatus` response changing to accelerating state.

- Wireshark showing `RSL` does not mean the actual transport/protocol changed. Use `tcp.port==4000` and payload hex to identify Elmo command IDs.

## Performance analysis facts

- PMAS/LASAL latency comparisons must separate app-measured `ReadLatency(ms)` from PCAP request-response RTT.
- Simple FIFO request-response matching can be wrong when the capture contains `req, req, rsp` patterns.
- Later valid comparison used adjacent request-response matching plus warmup removal.
- One recorded comparison:
  - XLSX `ReadLatency` mean under 2 ms: about `1.018326 ms`
  - PCAP RTT mean under 2 ms: about `0.961102 ms`
  - difference: about `0.057 ms`
- Polling loop behavior in the existing code is read-complete plus wait interval, not absolute read-start scheduling.
- Improving the wait loop can improve poll-period uniformity, but it does not make PLC/controller response itself faster.
- Real bottlenecks discussed: PLC task cycle, controller processing, C# socket send/recv structure, Windows scheduling, queue buildup when requests are too dense.

## Git context from history

The history records multiple pushed commits to `origin/main`, including PMAS/LASAL code, report artifacts, packet captures, profile captures, and temporary Excel lock-file ignore rules.

Current local status checked during this analysis shows untracked generated docs and `docs\history`. Do not assume the worktree is clean now.

## Recommended next action

Continue with the unresolved PDF bookmark repair:

1. Use the `pdf` skill workflow.
2. Read the existing PDF outline tree with `pypdf`.
3. Extract actual chapter/section pages from the PDF text.
4. Build a corrected copy, for example:
   - `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
5. Validate:
   - page count unchanged
   - outline count roughly preserved
   - sample bookmarks resolve to actual chapter/section pages, not page 1
   - original PDF remains untouched

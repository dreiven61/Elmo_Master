# Elmo Master History 260609 Analysis Summary

- Source: `docs/history/Elmo_Master_history_260609.md`
- Split folder: `docs/history/260609`
- Split files: `Elmo_Master_history_260609_part_01.md` through `Elmo_Master_history_260609_part_04.md`
- Source size checked: 741 lines, 42,931 bytes
- Analysis date: 2026-06-09

## Current Thread State

This history does not show an unfinished implementation task at the end. The latest completed topic is the explanation of `MMC_RpcInitConnection()` connection types:

- `MMC_RPC_CONN_TYPE`: remote connection from an external PC application to the Maestro MMC server, normally over Ethernet/TCP/IP.
- `MMC_IPC_CONN_TYPE`: local inter-process connection inside the same Maestro/device OS environment.
- For the current PC-based PMAS/WPF control program context, the correct practical choice is almost certainly `MMC_RPC_CONN_TYPE` with `cpHostIPAddr` set to the Maestro IP.

## Completed Work Captured In This History

1. `docs/history/260526` was created from the large 2026-05-26 history file.
   - The original 2026-05-26 history was preserved.
   - Base64 screenshots were extracted into `docs/history/260526/assets`.
   - `docs/history/260526/analysis_summary.md` was created.

2. The broken bookmarks in `Maestro Administrative and Motion API_2022_12_v2.012.pdf` were repaired.
   - Output PDF: `output/pdf/Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
   - Mapping CSV: `output/pdf/Maestro Administrative and Motion API_2022_12_v2.012_bookmark_map.csv`
   - Verification captured in history:
     - Page count stayed at 2435.
     - Bookmark count stayed at 895.
     - 870 bookmarks that wrongly pointed to page 1 were fixed.
     - Fallback mappings: 0.
     - Bookmarks pointing to page 1 after repair: 0.

3. CoreAPI overview table files were reformatted.
   - Files:
     - `docs/Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.md`
     - `docs/Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.xlsx`
     - `docs/Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.md`
     - `docs/Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.xlsx`
   - Changes:
     - `Section/섹션` and `Page/페이지` columns removed.
     - Tables changed to function/parameter/content structure.
     - Function-name column made narrower.
     - Content/role column made wider with wrapping.
     - XLSX range verified as `A1:C342`.

4. Public availability of newer Elmo Maestro API documentation was checked.
   - Public official site did not expose a newer `Maestro Administrative and Motion API` PDF than local `v2.012 / Dec 2022`.
   - The Resource Center showed newer firmware, libraries, and release notes, but not the API manual PDF.
   - Exact conclusion from the history: local `v2.012` is the latest publicly confirmable API manual, but a newer private Service Portal document may exist.

5. Sigmatek blending documents were analyzed.
   - Input folder: `docs/sigmatek_blending`
   - Main concepts:
     - Rounding mode / blending sphere uses `Radius`.
     - Smooth rounding uses `TransRadius`.
     - Smooth interpolation modes include `_LMCPROF_SMOOTH_CUBIC`, `_LMCPROF_SMOOTH_QUINT`, and `_LMCPROF_SMOOTH_CLOTH`.
     - `_LMCPROF_SMOOTH_CLOTH` should be treated as a clothoid-style interpolation/rounding mode, though the provided PDFs only listed it and did not deeply define it.
     - `TransMode` chooses the interpolation method; `Radius`/`TransRadius` defines the spatial tolerance/blending area.

6. Elmo blending was compared against Sigmatek blending.
   - Elmo has blending, but its model is different:
     - `BufferMode` decides transition velocity policy.
     - `TransitionMode` decides the transition curve/geometry.
     - `fTransitionParameter` is interpreted according to the selected transition mode.
   - Sigmatek is expressed more directly as:
     - `TransMode`: interpolation curve type.
     - `Radius`/`TransRadius`: tolerance/blending sphere size.
   - Elmo `MC_BLENDING_*` alone does not mean cubic/quint/clothoid-style path smoothing. For Sigmatek-like smoothing, Elmo requires the correct combination of `eBufferMode`, `eTransitionMode`, and `fTransitionParameter`.

## Important Technical Conclusions

- `ReadActualPosition` final packet-analysis basis from the earlier 2026-05-26 history is a 24-byte response, not the older intermediate 20-byte interpretation.
- Elmo API document `v2.012` remains the working reference unless a newer Service Portal document is obtained.
- In Elmo, do not equate `MC_BLENDING_NEXT_MODE` or similar `MC_BLENDING_*` values with Sigmatek smooth rounding. They are transition velocity policies.
- Elmo transition modes comparable to smooth path blending include polynomial/corner transition modes such as `POLYNOM3`, `POLYNOM5`, `POLYNOM7`, and `PLN8`.
- Sigmatek `_LMCPROF_SMOOTH_CUBIC`, `_LMCPROF_SMOOTH_QUINT`, and `_LMCPROF_SMOOTH_CLOTH` are interpolation/rounding mode selections, but they still need a blending/tolerance region through `Radius` or `TransRadius`.

## Local Repository State Observed During This Analysis

`git status --short` showed the following untracked areas/files:

- `docs/history/`
- `output/`
- `docs/Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.md`
- `docs/Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.xlsx`
- `docs/Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.md`
- `docs/Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.xlsx`
- `docs/Maestro_API_Function_Overview_KO_translation_cache.json`
- `tmp_api_names_sample.txt`

These were not staged or committed by this analysis step.

## How To Continue

Use this summary as the restart point. If the next task is technical discussion, the active context is Elmo Maestro API, Sigmatek/Elmo blending comparison, and PC-to-Maestro RPC control. If the next task is file work, first inspect whether the target file is one of the untracked generated artifacts above before editing.

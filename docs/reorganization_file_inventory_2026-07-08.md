# Reorganization File Inventory - 2026-07-08

This inventory records the rescan after the workspace was reorganized by
functional area.

## Scan Summary

- Total Git-visible changes before staging: 1,809
- Modified tracked files: 4
- Deleted tracked files from old locations: 681
- New non-ignored files in reorganized locations: 1,124

## Split Commit Plan

The reorganization is committed by functional area instead of one large commit:

1. `docs/`
   - API analysis docs, architecture notes, Korean API reference docs, split history files, and this inventory.
   - Includes only the required PMAS `MMCLibDotNET Libs V3.0.0.7/Lib` runtime files under `docs/Elmo_Lib/` so `Codex_PMAS_WPF` can build after the folder move.
2. `Codex_PMAS_WPF/`, `Codex_LASAL_WPF/`
   - Production-cycle WPF test changes and PMAS project reference path update.
3. `LMC_Library/`
   - LMC API delivery package, protocol docs, packet text exports, test app, and selected packet captures.
4. `test/`
   - Reorganized reports, packet captures, profile-capture files, parameter images, and old report/capture location deletions.
5. `Lasal_PRG/` and `Elmo_EtherCAT_Test_4Axis/`
   - Reorganized LASAL program sources, include files, network/project files, and removal of the old root LASAL project tree.

`git diff --check` is run before each commit.

## Main Groups

| Group | Meaning |
|---|---|
| `Lasal_PRG/` | Reorganized LASAL program sources, include files, network files, and selected project metadata |
| `LMC_Library/` | LMC API delivery package, packet TXT exports, protocol docs, sample app, and selected packet captures |
| `docs/` | API analysis, history split, architecture notes, and Korean API reference docs |
| `test/` | Reorganized reports, packet-capture analysis, parameter images, and profile-capture analysis |
| `Codex_PMAS_WPF/`, `Codex_LASAL_WPF/` | Existing WPF cycle-test source changes retained in place |

## Force-Included Ignored Inputs

The repository normally ignores captures and spreadsheet reports. For this
reorganization commit, the following ignored inputs are intentionally included
because they are now part of the organized evidence set:

- `LMC_Library/LMC_API/Elmo_API_Packet2/WireShark/*.pcapng`
- `LMC_Library/LMC_API/LMC_API/bin/LmcMotionApi.dll`
- `docs/Elmo_Lib/MMCLibDotNET Libs V3.0.0.7/Lib/*`
- `test/**/*.pcapng`
- `test/**/*.xlsx`

## Still Excluded

The following ignored categories remain excluded:

- Visual Studio `.vs/`, `bin/`, and `obj/` build output except selected delivery/runtime DLLs
- LASAL generated build artifacts such as new untracked `.lba`, `.lob`, `.lhd`, `.ldi`, `.lcc`
- Duplicate archive bundles such as `LMC_Library/*.zip`
- Full vendor archive/extracted bundles under `docs/Elmo_Lib/`; only the selected runtime `Lib` files are committed.
- Local temporary/cache files

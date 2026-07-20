# Elmo Master History 260716 Index

Source: `docs/history/Elmo_Master_history_260716.md`

## Split method and integrity

- Source size: 2,060,307 bytes
- Source lines: 16,760 (`System.IO.File.ReadAllLines`)
- Source line ending: CRLF, including the final line ending
- Target chunk size: 250 lines
- Boundary rule: cumulative 250-line boundaries; advance past blank lines so a
  chunk does not end with a blank Markdown line
- Chunk count: 68
- Chunk size range: 476 to 41,419 bytes
- Source SHA-256:
  `a88a537aa9f6a10b31995a81afb5cbaef6bbe25b152e6d731b90fcd7a1ade3c1`
- Sanitized split/rejoin SHA-256:
  `274ca885076e361aa4ae21fe48c66b93f5023ce2b76eeb904c7aaaa0ed606956`
- Sanitized rejoin check: passed; see [split_manifest.json](split_manifest.json)
- Original source hash after splitting: unchanged

Source line 1,913 contains a 1,048,602-character embedded computer-use JPEG and
tool-state payload. It would make one chunk larger than the entire readable
history. The split copy contains a one-line placeholder with the original
line's SHA-256; the original payload remains unchanged in the source file.
Therefore exact byte rejoin to the original is intentionally not claimed.
Six source lines with trailing tabs are also trimmed in the split copies so the
artifacts pass whitespace checks. Their source line numbers and original hashes
are recorded in the manifest. All other source content is retained.

## Chronological phases

1. Parts 001-005: 260710 handoff, API audit, caller-side UNIT policy, RPC and PC
   API completion.
2. Parts 006-018: canonical LASAL project selection, `_Edit` IDE failure study,
   first dispatcher work, and the change from RT mailbox execution to
   CyWork-only execution.
3. Parts 019-038: LASAL command activation, library synchronization, IDE build
   and implementation-search smoke, test-app hardening, and checkpoint commits.
4. Parts 039-047: new simplified WPF example, hardware lookup/AxisInfo debugging,
   case-insensitive names, and `$DINT` overlay repair.
5. Parts 048-058: group semantics, reference/limit/UNIT investigation, Jerk,
   9-axis single-axis dispatch, Home Check, and LASAL naming cleanup.
6. Parts 059-068: source review, standalone distribution, API-only manual,
   document QA, final commits, and push.

## Chunks

| Part | Source lines | File | Main content |
|---:|---:|---|---|
| 001 | 1-250 | [part 001](Elmo_Master_history_260716_part_001_lines_00001_00250.md) | 260710 handoff, 23-command audit, UNIT policy, and RPC start |
| 002 | 251-500 | [part 002](Elmo_Master_history_260716_part_002_lines_00251_00500.md) | Caller-side UNIT, single-session RPC, dispatcher, typed responses, and WPF safety |
| 003 | 501-750 | [part 003](Elmo_Master_history_260716_part_003_lines_00501_00750.md) | Thirty PC tests, LASAL contracts, queue design only, and five checkpoint commits |
| 004 | 751-1000 | [part 004](Elmo_Master_history_260716_part_004_lines_00751_01000.md) | PC 23/23 API completion, lifecycle/WPF work, package, and 42 tests |
| 005 | 1001-1250 | [part 005](Elmo_Master_history_260716_part_005_lines_01001_01250.md) | PC commits and canonical versus `_Edit` Find-in-Implementation investigation |
| 006 | 1251-1500 | [part 006](Elmo_Master_history_260716_part_006_lines_01251_01500.md) | Canonical dispatcher versus `_Edit` legacy parser comparison in open IDEs |
| 007 | 1501-1750 | [part 007](Elmo_Master_history_260716_part_007_lines_01501_01750.md) | `_Edit` class registration and readable-source checks |
| 008 | 1751-2000 | [part 008](Elmo_Master_history_260716_part_008_lines_01751_02000.md) | Find-in-Implementation semantics and reproduced `_Edit` exception; payload omitted |
| 009 | 2001-2250 | [part 009](Elmo_Master_history_260716_part_009_lines_02001_02250.md) | UTF-8 comment hypothesis, LASAL error-prevention guide, and design gates |
| 010 | 2251-2500 | [part 010](Elmo_Master_history_260716_part_010_lines_02251_02500.md) | Task/core risks, LMCAxis1 queue, and source-first position-read implementation |
| 011 | 2501-2750 | [part 011](Elmo_Master_history_260716_part_011_lines_02501_02750.md) | Position-read commit/push and ReadStatus/IDE work start |
| 012 | 2751-3000 | [part 012](Elmo_Master_history_260716_part_012_lines_02751_03000.md) | Computer-use setup for LASAL IDE work |
| 013 | 3001-3250 | [part 013](Elmo_Master_history_260716_part_013_lines_03001_03250.md) | Opening the canonical LASAL project and file-dialog targeting problems |
| 014 | 3251-3500 | [part 014](Elmo_Master_history_260716_part_014_lines_03251_03500.md) | File-dialog recovery and continued canonical project loading |
| 015 | 3501-3750 | [part 015](Elmo_Master_history_260716_part_015_lines_03501_03750.md) | Legacy client removal, IDE diagnostics, and 1 ms TCPMotionInterface settings |
| 016 | 3751-4000 | [part 016](Elmo_Master_history_260716_part_016_lines_03751_04000.md) | LMCAxis1 client creation and attempted `_LMCAxis1.Control` network connection |
| 017 | 4001-4250 | [part 017](Elmo_Master_history_260716_part_017_lines_04001_04250.md) | ReadStatus verification and missing `DriveComL2.h` diagnosis |
| 018 | 4251-4500 | [part 018](Elmo_Master_history_260716_part_018_lines_04251_04500.md) | ReadStatus completion and RT mailbox removal in favor of CyWork-only execution |
| 019 | 4501-4751 | [part 019](Elmo_Master_history_260716_part_019_lines_04501_04751.md) | Eight axis and three group commands, type fix, and C78/C81 library diagnosis |
| 020 | 4752-5000 | [part 020](Elmo_Master_history_260716_part_020_lines_04752_05000.md) | E2E test order, 18 supported/5 blocked snapshot, TestApp and Git cleanup start |
| 021 | 5001-5250 | [part 021](Elmo_Master_history_260716_part_021_lines_05001_05250.md) | Tool guidance and API context for TestApp/library cleanup |
| 022 | 5251-5500 | [part 022](Elmo_Master_history_260716_part_022_lines_05251_05500.md) | Locating the open canonical LASAL IDE session |
| 023 | 5501-5750 | [part 023](Elmo_Master_history_260716_part_023_lines_05501_05750.md) | Rebuild launch and initially failed implementation-search smoke targeting |
| 024 | 5751-6000 | [part 024](Elmo_Master_history_260716_part_024_lines_05751_06000.md) | Re-selecting class/network views to find the implementation-search entry point |
| 025 | 6001-6250 | [part 025](Elmo_Master_history_260716_part_025_lines_06001_06250.md) | API range review, claimed clean LASAL rebuild, and class-tree inspection |
| 026 | 6251-6500 | [part 026](Elmo_Master_history_260716_part_026_lines_06251_06500.md) | LASAL screen state and TCPMotionInterface tree expansion |
| 027 | 6501-6750 | [part 027](Elmo_Master_history_260716_part_027_lines_06501_06750.md) | LMCAxis1-4, LMCRobot, and `_StdLib` client inspection |
| 028 | 6751-7000 | [part 028](Elmo_Master_history_260716_part_028_lines_06751_07000.md) | LMCAxis1 state and Power/pos/velo smoke targets |
| 029 | 7001-7250 | [part 029](Elmo_Master_history_260716_part_029_lines_07001_07250.md) | Server method menus and implementation-search path investigation |
| 030 | 7251-7500 | [part 030](Elmo_Master_history_260716_part_030_lines_07251_07500.md) | Editor Find dialog control and Power search |
| 031 | 7501-7750 | [part 031](Elmo_Master_history_260716_part_031_lines_07501_07750.md) | Power/pos/velo searches and Global view navigation |
| 032 | 7751-8000 | [part 032](Elmo_Master_history_260716_part_032_lines_07751_08000.md) | Server/client variable tree and view checks |
| 033 | 8001-8250 | [part 033](Elmo_Master_history_260716_part_033_lines_08001_08250.md) | Network navigation plus WPF safety and Release-build status |
| 034 | 8251-8500 | [part 034](Elmo_Master_history_260716_part_034_lines_08251_08500.md) | Motion Network accessibility-tree inspection |
| 035 | 8501-8750 | [part 035](Elmo_Master_history_260716_part_035_lines_08501_08750.md) | Activating and moving within the Motion Network editor |
| 036 | 8751-9000 | [part 036](Elmo_Master_history_260716_part_036_lines_08751_09000.md) | Opening the Power variable context menu |
| 037 | 9001-9250 | [part 037](Elmo_Master_history_260716_part_037_lines_09001_09250.md) | Power Find-in-Implementation and transition to pos search |
| 038 | 9251-9500 | [part 038](Elmo_Master_history_260716_part_038_lines_09251_09500.md) | Successful smoke, TestApp hardening, Git policy, validation, and four commits |
| 039 | 9501-9750 | [part 039](Elmo_Master_history_260716_part_039_lines_09501_09750.md) | Simplified WPF example request and removal of dummy/unimplemented UI |
| 040 | 9751-10000 | [part 040](Elmo_Master_history_260716_part_040_lines_09751_10000.md) | Real API reference conversion, initial build, and app discovery |
| 041 | 10001-10250 | [part 041](Elmo_Master_history_260716_part_041_lines_10001_10250.md) | Windows app inventory with little new engineering content |
| 042 | 10251-10500 | [part 042](Elmo_Master_history_260716_part_042_lines_10251_10500.md) | Correcting the EXE path and successfully launching the example |
| 043 | 10501-10750 | [part 043](Elmo_Master_history_260716_part_043_lines_10501_10750.md) | Locating the running example and starting UI inspection |
| 044 | 10751-11000 | [part 044](Elmo_Master_history_260716_part_044_lines_10751_11000.md) | UI smoke and Stop/PowerOff/Standstill safety-path redesign |
| 045 | 11001-11251 | [part 045](Elmo_Master_history_260716_part_045_lines_11001_11251.md) | Revised UI, command competition fixes, and regression results |
| 046 | 11252-11500 | [part 046](Elmo_Master_history_260716_part_046_lines_11252_11500.md) | Lookup case bug, AxisInfo -4, and `$DINT` memory-overlay diagnosis |
| 047 | 11501-11750 | [part 047](Elmo_Master_history_260716_part_047_lines_11501_11750.md) | `TO_DINT` repair, Arm/MessageBox removal, Jerk input, and axes 2-4 diagnosis |
| 048 | 11751-12000 | [part 048](Elmo_Master_history_260716_part_048_lines_11751_12000.md) | Reference/encoder-retain diagnosis and missing Group API implementation |
| 049 | 12001-12251 | [part 049](Elmo_Master_history_260716_part_049_lines_12001_12251.md) | Group errors, RobotOn/LockProfile semantics, and status defects |
| 050 | 12252-12500 | [part 050](Elmo_Master_history_260716_part_050_lines_12252_12500.md) | Software-end-position error, UI sequence, and UNIT/raw-DINT controls |
| 051 | 12501-12750 | [part 051](Elmo_Master_history_260716_part_051_lines_12501_12750.md) | 128 mm MaxModulo/BinOffset investigation and UI cleanup |
| 052 | 12751-13000 | [part 052](Elmo_Master_history_260716_part_052_lines_12751_13000.md) | Log collapsing, wider Group layout, and app-search setup |
| 053 | 13001-13250 | [part 053](Elmo_Master_history_260716_part_053_lines_13001_13250.md) | Windows application inventory |
| 054 | 13251-13500 | [part 054](Elmo_Master_history_260716_part_054_lines_13251_13500.md) | Continued application inventory for visual testing |
| 055 | 13501-13750 | [part 055](Elmo_Master_history_260716_part_055_lines_13501_13750.md) | Temporary WPF build and interrupted visual inspection |
| 056 | 13751-14000 | [part 056](Elmo_Master_history_260716_part_056_lines_13751_14000.md) | UI result and 128 mm/`0x40000000` boundary conclusion |
| 057 | 14001-14251 | [part 057](Elmo_Master_history_260716_part_057_lines_14001_14251.md) | 10 mm/rev experiment and start of 9-axis dispatcher expansion |
| 058 | 14252-14500 | [part 058](Elmo_Master_history_260716_part_058_lines_14252_14500.md) | Nine-axis control, four-axis Home Check, LASAL rename, and first push |
| 059 | 14501-14750 | [part 059](Elmo_Master_history_260716_part_059_lines_14501_14750.md) | Remote verification and standalone distribution/source-review start |
| 060 | 14751-15000 | [part 060](Elmo_Master_history_260716_part_060_lines_14751_15000.md) | Three-part distribution, editable DOCX, and API-only rewrite start |
| 061 | 15001-15250 | [part 061](Elmo_Master_history_260716_part_061_lines_15001_15250.md) | API-only manual validation and request for simpler Maestro-style content |
| 062 | 15251-15500 | [part 062](Elmo_Master_history_260716_part_062_lines_15251_15500.md) | Concise API reference and conversion of user-edited DOCX |
| 063 | 15501-15750 | [part 063](Elmo_Master_history_260716_part_063_lines_15501_15750.md) | Word PDF export, 21-page QA, and footer/table overlap fixes |
| 064 | 15751-16000 | [part 064](Elmo_Master_history_260716_part_064_lines_15751_16000.md) | Vector/strip PDF repair experiments |
| 065 | 16001-16250 | [part 065](Elmo_Master_history_260716_part_065_lines_16001_16250.md) | Ghostscript/hybrid trials and switch to image-based output |
| 066 | 16251-16500 | [part 066](Elmo_Master_history_260716_part_066_lines_16251_16500.md) | 21-page image PDF assembly and full-page/contact-sheet QA |
| 067 | 16501-16750 | [part 067](Elmo_Master_history_260716_part_067_lines_16501_16750.md) | Searchable edited manual promotion, full validation, three commits, and push |
| 068 | 16751-16760 | [part 068](Elmo_Master_history_260716_part_068_lines_16751_16760.md) | Final commit list, test result, and distribution path |

## Resume entry

Read [99_analysis_summary.md](99_analysis_summary.md) first. The final history
request to commit and push is already complete. The next technical work is not
more unverified API source expansion; it is current-source LASAL IDE and PLC
validation, beginning with Rebuild/Link and Find-in-Implementation smoke, then
read-only packet/E2E tests before any motion.

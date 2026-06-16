# Elmo_Master_history_260617 analysis summary

- Source: `../Elmo_Master_history_260617.md`
- Split index: `./index.md`
- Split files: `Elmo_Master_history_260617_part_01.md` through `Elmo_Master_history_260617_part_10.md`
- Source line count used for split: 2266 lines
- Chunk size: 250 source lines

## Current Thread Context

This history continues from the 260609 summary and mainly covers PMAS WPF test app work around Elmo Maestro group motion, PI access, bulk read, device identification, and EtherCAT XML/ESI lookup.

The latest unfinished work in the history is the CREVIS XML lookup:

- Requested devices: `CREVIS GL-9086`, `GT-12FA`, `GT22BA`/`GT-22BA`.
- A Beijer V16 EtherCAT IO XML bundle was downloaded and extracted locally.
- The Beijer bundle contains `GL-9086`, `GT-12FA`, and `GT-22BA`.
- The Beijer XML vendor ID is `#x00000755` (`Beijer Electronics`).
- CREVIS download attempts for `idx2749` and `idx3466` produced PHP error HTML, not valid ZIP files.
- Practical next check: compare actual EtherCAT scan identity. If the physical device scans as Beijer vendor `0x00000755`, the local Beijer V16 XML is usable. If it scans as CREVIS vendor `0x0000029D`, the Beijer XML is only a reference and a true CREVIS ESI is still needed.

Local CREVIS/Beijer files checked:

- `output/CREVIS_GL9086_ESI_V16/GL9086_GN9386_EtherCAT_IO_XML_V16_2025-07-11.zip` is a valid ZIP.
- `output/CREVIS_GL9086_ESI_V16/extracted/Beijer_GL-9086_V16.xml`
- `output/CREVIS_GL9086_ESI_V16/extracted/Beijer EtherCAT IO Module/Beijer_GT-xxxx_V16.xml`
- `output/CREVIS_GL9086_ESI_V16/CREVIS_Network_Adapter_EtherCAT_XML_idx2749.zip` is not a ZIP. It is PHP error HTML.
- `output/CREVIS_GL9086_ESI_V16/CREVIS_Network_Adapter_EtherCAT_XML_idx3466.zip` is not a ZIP. It is PHP error HTML.

## WPF App Work Already Done

Modified working tree files from this history:

- `Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.Coverage.cs`
- `Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs`
- `Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs`
- `Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml`
- `Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs`
- `Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs`
- `.gitattributes`

The history repeatedly reports successful builds using Visual Studio 2019 MSBuild. `dotnet msbuild` was not reliable for this WPF project because it did not pick up generated XAML code in the same way.

## Group Motion Findings

Confirmed group behavior:

- `GroupEnable` is close in operating meaning to Sigmatek `ProfileLock`, but not identical.
- All group member axes must already be individually enabled/healthy before `GroupEnable`.
- `NC_ONE_GRP_MEMBER_IS_DISABLED` means at least one axis in the group is disabled.
- Group info being readable does not imply the group can be enabled.

Important final code state:

- Default group axes are `a01,a02,a03,a04`.
- `Prepare Group MCS` performs member power-on, kinematic transform, then group enable.
- Current default Cartesian node order in code is `X,Y,Z,U,V,W,N1...N9`.
- For a 4-axis group, current code maps `a01->X`, `a02->Y`, `a03->Z`, `a04->U`.
- Endpoint default is currently `8388608,8388608,8388608,8388608`.
- Earlier history contains a wrong intermediate idea that `a04` could be excluded from kinematic. That was corrected: all group members must be present in kinematic for the current controller/group.

Group tab additions:

- auto-sync group axis names from `GetGroupMembersInfo`
- member status diagnostics
- `Power On Members`
- `Power Off Members`
- `Prepare Group MCS`
- group actual/target position snapshot logging
- `MoveLinearAbsolute` and `MoveLinearRelative` routed through UI velocity/acc/dec/jerk/coord parameters
- clearer error hints for group/coordinate/kinematic failures

## PI And Bulk Read Findings

PI means Processing Image: the cyclic EtherCAT input/output image mapped from PDOs.

Important PI details:

- PI index is not a CANopen object index.
- `GetPIVarInfo` by index/direction is the reliable way to inspect what a PI slot actually is.
- `GetPIVarInfoByAlias` needs the exact alias string in the controller PI map. Guess strings like `I.6041.0` can fail with `NC_PI_VAR_NOT_FOUND`.
- Failed alias lookup previously printed invalid struct garbage; code was updated to check return values and print meaningful errors.

Important value/type correction:

- `I0x6064.0` at PI index `1` was a 32-bit signed actual position.
- Reading it with `ReadPIVarUShort` only returns the low 16 bits.
- For actual position, use `MMC_ReadPIVarInt`.

PI tab additions:

- PI variable type combo for read/write
- `MMC_ReadPIVarInt`
- type mismatch warnings when reading a PI slot with the wrong type
- `MMC_GetPIVarInfo`
- improved alias error handling
- PI bulk read UI and logic

Bulk read distinctions:

- Parameter Bulk Read is for parameter/preset-style bulk reads.
- PI Bulk Read is for PI index/direction-based cyclic data.
- `Bulk Config` and `PI Bulk Config` are separate runtime configuration slot enums.
- `MAX` and `None` are not usable config slots and were removed from user-selectable combos.
- Config is not persistent storage. Treat it as runtime read preparation; reconnect/restart/target change means configure again.

## Device And XML Findings

Elmo product:

- `G-SOLBEL9/200JEH` was identified as Gold Solo Bell EtherCAT family.
- Meaning summarized in history: 9A continuous, 200VDC bus class, EtherCAT with switches, feedback `E`, Sink I/O plus Source STO configuration.
- Of the checked Elmo XML files, `Elmo ECAT 00010420 V10.xml` is the matching candidate.
- Platinum XML files are not the right family.
- Final validation should compare EtherCAT scan identity:
  - Vendor ID `0x0000009A`
  - Product Code `0x00030924` or `0x00030925`
  - Revision `0x00010420`

CREVIS/Beijer XML:

- Valid local Beijer bundle:
  - adapter XML: `Beijer_GL-9086_V16.xml`
  - module XML: `Beijer_GT-xxxx_V16.xml`
- Verified entries:
  - `GL-9086`, ProductCode `#x474C9086`
  - `GT-12FA`, ModuleIdent `#x475412FA`
  - `GT-22BA`, ModuleIdent `#x475422BA`
- Risk: vendor ID mismatch if physical hardware is CREVIS-branded with CREVIS vendor ID instead of Beijer vendor ID.

## Known Corrections To Preserve

- Ignore the earlier `INITIAL / Synchronized warning / CmdExecErrorInfo` discussion. User explicitly said that belonged to another project.
- Initial claim that a 4-axis group could use only 3 kinematic axes was wrong for this controller. All group members must be in the kinematic definition.
- Alias format guesses were wrong until actual PI info showed aliases like `I0x6041.0`/`I0x6064.0` style.
- A `ReadPIVarUShort` result for a 32-bit signed value is only the low word, not the full value.
- Config slots are runtime preparation slots, not saved data stores.

## Recommended Next Step

If the next task continues the last active thread, finish the CREVIS XML lookup by either:

1. Asking the user for the actual EtherCAT scan Vendor ID/Product Code/Revision of the GL-9086.
2. If it is Beijer vendor `0x00000755`, use the already extracted Beijer V16 XML files.
3. If it is CREVIS vendor `0x0000029D`, continue looking for a true CREVIS ESI or convert only with explicit user approval after explaining the vendor-ID risk.

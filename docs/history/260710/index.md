# Elmo Master History 260710 Index

Source: `docs/history/Elmo_Master_history_260710.md`

Split method:

- Source lines: 3,512
- Source bytes: 218,876
- Target chunk size: 250 lines
- Boundary rule: move a split forward one line when the target line is blank
- Chunk count: 15
- SHA-256: `126b0760635737f45a34310d9e13955cd68ea291b9b77c6b3f5f00f5c2b50a55`
- Byte rejoin check: passed; see [split_manifest.json](split_manifest.json)
- Omitted or substituted payloads: none

## Chunks

| Part | Source lines | File | Main content |
|---:|---:|---|---|
| 1 | 1-250 | [part 01](Elmo_Master_history_260710_part_01_lines_0001_0250.md) | Reorganization commit split and remote branch update |
| 2 | 251-500 | [part 02](Elmo_Master_history_260710_part_02_lines_0251_0500.md) | LASAL original/Edit comparison, legacy DLL patch, source delivery start |
| 3 | 501-750 | [part 03](Elmo_Master_history_260710_part_03_lines_0501_0750.md) | Packet capture analysis and migration from fixed scaling to `unit.h` profiles |
| 4 | 751-1000 | [part 04](Elmo_Master_history_260710_part_04_lines_0751_1000.md) | API readability, delivery naming, unit constants, Buffered Mode investigation |
| 5 | 1001-1250 | [part 05](Elmo_Master_history_260710_part_05_lines_1001_1250.md) | `_LMCAxis` non-buffered behavior and `LasalMotionControlLib` rename |
| 6 | 1251-1500 | [part 06](Elmo_Master_history_260710_part_06_lines_1251_1500.md) | Reference lookup model and `LMC_API_Delivery` scope/no-conversion decision |
| 7 | 1501-1750 | [part 07](Elmo_Master_history_260710_part_07_lines_1501_1750.md) | Object/API restructuring and removal of internal conversion helpers |
| 8 | 1751-2000 | [part 08](Elmo_Master_history_260710_part_08_lines_1751_2000.md) | Unit constants restored as declarations and `MMC` symbols renamed to `LMC` |
| 9 | 2001-2250 | [part 09](Elmo_Master_history_260710_part_09_lines_2001_2250.md) | LASAL DINT contract, Axis/Group references, TCP versus RPC |
| 10 | 2251-2500 | [part 10](Elmo_Master_history_260710_part_10_lines_2251_2500.md) | Multi-PC session design and start of captured RPC handshake implementation |
| 11 | 2501-2751 | [part 11](Elmo_Master_history_260710_part_11_lines_2501_2751.md) | RPC handshake, UDP callback listener ownership, response model review |
| 12 | 2752-3000 | [part 12](Elmo_Master_history_260710_part_12_lines_2752_3000.md) | Envelope-based response parsing and LASAL-specific frame builder naming |
| 13 | 3001-3251 | [part 13](Elmo_Master_history_260710_part_13_lines_3001_3251.md) | Public wrapper/alias removal and test-app migration |
| 14 | 3252-3500 | [part 14](Elmo_Master_history_260710_part_14_lines_3252_3500.md) | Line-ending policy, test-app formatting, current DINT/capture documentation |
| 15 | 3501-3512 | [part 15](Elmo_Master_history_260710_part_15_lines_3501_3512.md) | Final conclusion: `0x2051` and `0x20E7` remain unimplemented |

## Resume Entry

Read [99_analysis_summary.md](99_analysis_summary.md) first.

The current continuation point is the PC/PLC packet-contract gap between
`LMC_Library/LMC_API_Delivery` and the live LASAL sources:

1. Confirm which LASAL project is the canonical deployment target.
2. Align the DINT header, command IDs, response offsets, and RPC handshake on
   both the C# and LASAL sides.
3. Implement `0x2051 GroupReadActualPosition` on that verified contract.
4. Reverse-engineer the full 1,320-byte `0x20E7 SetKinTransform` payload before
   creating its API or frame builder.
5. Do not overwrite or stage the pre-existing dirty delivery-package changes
   until their scope is reviewed separately.

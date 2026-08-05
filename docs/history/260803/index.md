# Elmo Master history 260803 split index

## Split method and integrity

- Original files are preserved unchanged.
- Target size: 250 source lines per readable chunk.
- Total readable chunks: 325.
- Embedded base64 runs of at least 4,096 characters are replaced only in split copies by SHA-256 placeholders.
- Trailing spaces and tabs are trimmed only in split copies and recorded in the manifest.
- Sanitized chunk rejoin checks passed for every source; exact-byte rejoin to a transformed source is intentionally not claimed.
- Machine-readable hashes and every transformation are in [split_manifest.json](split_manifest.json).
- Current-state conclusions and the actual continuation point are in [99_analysis_summary.md](99_analysis_summary.md).

## Analysis coverage

- [Parts 001-107 digest](01_chunk_digest_parts_001_107.md)
- [Parts 108-214 digest](02_chunk_digest_parts_108_214.md)
- [Parts 215-322 and history 2 digest](03_chunk_digest_parts_215_322_and_history_2.md)
- [Current continuation summary](99_analysis_summary.md)

## Elmo_Master_history_260803_1.md

- Source: `docs/history/Elmo_Master_history_260803_1.md`
- Size: 118,980,470 bytes
- Lines: 80,500
- Source SHA-256: `1a1c0a79085e3c4a957b488f9befe718af12224005cf1ec6f24ed921ca6c1821`
- Readable chunks: 322
- Base64-bearing source lines replaced: 500
- Trailing-whitespace source lines normalized: 74

| Part | Source lines | File |
|---:|---:|---|
| 001 | 1-250 | [Elmo_Master_history_260803_1_part_001_lines_00001_00250.md](Elmo_Master_history_260803_1_part_001_lines_00001_00250.md) |
| 002 | 251-500 | [Elmo_Master_history_260803_1_part_002_lines_00251_00500.md](Elmo_Master_history_260803_1_part_002_lines_00251_00500.md) |
| 003 | 501-750 | [Elmo_Master_history_260803_1_part_003_lines_00501_00750.md](Elmo_Master_history_260803_1_part_003_lines_00501_00750.md) |
| 004 | 751-1,000 | [Elmo_Master_history_260803_1_part_004_lines_00751_01000.md](Elmo_Master_history_260803_1_part_004_lines_00751_01000.md) |
| 005 | 1,001-1,250 | [Elmo_Master_history_260803_1_part_005_lines_01001_01250.md](Elmo_Master_history_260803_1_part_005_lines_01001_01250.md) |
| 006 | 1,251-1,500 | [Elmo_Master_history_260803_1_part_006_lines_01251_01500.md](Elmo_Master_history_260803_1_part_006_lines_01251_01500.md) |
| 007 | 1,501-1,750 | [Elmo_Master_history_260803_1_part_007_lines_01501_01750.md](Elmo_Master_history_260803_1_part_007_lines_01501_01750.md) |
| 008 | 1,751-2,000 | [Elmo_Master_history_260803_1_part_008_lines_01751_02000.md](Elmo_Master_history_260803_1_part_008_lines_01751_02000.md) |
| 009 | 2,001-2,250 | [Elmo_Master_history_260803_1_part_009_lines_02001_02250.md](Elmo_Master_history_260803_1_part_009_lines_02001_02250.md) |
| 010 | 2,251-2,500 | [Elmo_Master_history_260803_1_part_010_lines_02251_02500.md](Elmo_Master_history_260803_1_part_010_lines_02251_02500.md) |
| 011 | 2,501-2,750 | [Elmo_Master_history_260803_1_part_011_lines_02501_02750.md](Elmo_Master_history_260803_1_part_011_lines_02501_02750.md) |
| 012 | 2,751-3,000 | [Elmo_Master_history_260803_1_part_012_lines_02751_03000.md](Elmo_Master_history_260803_1_part_012_lines_02751_03000.md) |
| 013 | 3,001-3,250 | [Elmo_Master_history_260803_1_part_013_lines_03001_03250.md](Elmo_Master_history_260803_1_part_013_lines_03001_03250.md) |
| 014 | 3,251-3,500 | [Elmo_Master_history_260803_1_part_014_lines_03251_03500.md](Elmo_Master_history_260803_1_part_014_lines_03251_03500.md) |
| 015 | 3,501-3,750 | [Elmo_Master_history_260803_1_part_015_lines_03501_03750.md](Elmo_Master_history_260803_1_part_015_lines_03501_03750.md) |
| 016 | 3,751-4,000 | [Elmo_Master_history_260803_1_part_016_lines_03751_04000.md](Elmo_Master_history_260803_1_part_016_lines_03751_04000.md) |
| 017 | 4,001-4,250 | [Elmo_Master_history_260803_1_part_017_lines_04001_04250.md](Elmo_Master_history_260803_1_part_017_lines_04001_04250.md) |
| 018 | 4,251-4,500 | [Elmo_Master_history_260803_1_part_018_lines_04251_04500.md](Elmo_Master_history_260803_1_part_018_lines_04251_04500.md) |
| 019 | 4,501-4,750 | [Elmo_Master_history_260803_1_part_019_lines_04501_04750.md](Elmo_Master_history_260803_1_part_019_lines_04501_04750.md) |
| 020 | 4,751-5,000 | [Elmo_Master_history_260803_1_part_020_lines_04751_05000.md](Elmo_Master_history_260803_1_part_020_lines_04751_05000.md) |
| 021 | 5,001-5,250 | [Elmo_Master_history_260803_1_part_021_lines_05001_05250.md](Elmo_Master_history_260803_1_part_021_lines_05001_05250.md) |
| 022 | 5,251-5,500 | [Elmo_Master_history_260803_1_part_022_lines_05251_05500.md](Elmo_Master_history_260803_1_part_022_lines_05251_05500.md) |
| 023 | 5,501-5,750 | [Elmo_Master_history_260803_1_part_023_lines_05501_05750.md](Elmo_Master_history_260803_1_part_023_lines_05501_05750.md) |
| 024 | 5,751-6,000 | [Elmo_Master_history_260803_1_part_024_lines_05751_06000.md](Elmo_Master_history_260803_1_part_024_lines_05751_06000.md) |
| 025 | 6,001-6,250 | [Elmo_Master_history_260803_1_part_025_lines_06001_06250.md](Elmo_Master_history_260803_1_part_025_lines_06001_06250.md) |
| 026 | 6,251-6,500 | [Elmo_Master_history_260803_1_part_026_lines_06251_06500.md](Elmo_Master_history_260803_1_part_026_lines_06251_06500.md) |
| 027 | 6,501-6,750 | [Elmo_Master_history_260803_1_part_027_lines_06501_06750.md](Elmo_Master_history_260803_1_part_027_lines_06501_06750.md) |
| 028 | 6,751-7,000 | [Elmo_Master_history_260803_1_part_028_lines_06751_07000.md](Elmo_Master_history_260803_1_part_028_lines_06751_07000.md) |
| 029 | 7,001-7,250 | [Elmo_Master_history_260803_1_part_029_lines_07001_07250.md](Elmo_Master_history_260803_1_part_029_lines_07001_07250.md) |
| 030 | 7,251-7,500 | [Elmo_Master_history_260803_1_part_030_lines_07251_07500.md](Elmo_Master_history_260803_1_part_030_lines_07251_07500.md) |
| 031 | 7,501-7,750 | [Elmo_Master_history_260803_1_part_031_lines_07501_07750.md](Elmo_Master_history_260803_1_part_031_lines_07501_07750.md) |
| 032 | 7,751-8,000 | [Elmo_Master_history_260803_1_part_032_lines_07751_08000.md](Elmo_Master_history_260803_1_part_032_lines_07751_08000.md) |
| 033 | 8,001-8,250 | [Elmo_Master_history_260803_1_part_033_lines_08001_08250.md](Elmo_Master_history_260803_1_part_033_lines_08001_08250.md) |
| 034 | 8,251-8,500 | [Elmo_Master_history_260803_1_part_034_lines_08251_08500.md](Elmo_Master_history_260803_1_part_034_lines_08251_08500.md) |
| 035 | 8,501-8,750 | [Elmo_Master_history_260803_1_part_035_lines_08501_08750.md](Elmo_Master_history_260803_1_part_035_lines_08501_08750.md) |
| 036 | 8,751-9,000 | [Elmo_Master_history_260803_1_part_036_lines_08751_09000.md](Elmo_Master_history_260803_1_part_036_lines_08751_09000.md) |
| 037 | 9,001-9,250 | [Elmo_Master_history_260803_1_part_037_lines_09001_09250.md](Elmo_Master_history_260803_1_part_037_lines_09001_09250.md) |
| 038 | 9,251-9,500 | [Elmo_Master_history_260803_1_part_038_lines_09251_09500.md](Elmo_Master_history_260803_1_part_038_lines_09251_09500.md) |
| 039 | 9,501-9,750 | [Elmo_Master_history_260803_1_part_039_lines_09501_09750.md](Elmo_Master_history_260803_1_part_039_lines_09501_09750.md) |
| 040 | 9,751-10,000 | [Elmo_Master_history_260803_1_part_040_lines_09751_10000.md](Elmo_Master_history_260803_1_part_040_lines_09751_10000.md) |
| 041 | 10,001-10,250 | [Elmo_Master_history_260803_1_part_041_lines_10001_10250.md](Elmo_Master_history_260803_1_part_041_lines_10001_10250.md) |
| 042 | 10,251-10,500 | [Elmo_Master_history_260803_1_part_042_lines_10251_10500.md](Elmo_Master_history_260803_1_part_042_lines_10251_10500.md) |
| 043 | 10,501-10,750 | [Elmo_Master_history_260803_1_part_043_lines_10501_10750.md](Elmo_Master_history_260803_1_part_043_lines_10501_10750.md) |
| 044 | 10,751-11,000 | [Elmo_Master_history_260803_1_part_044_lines_10751_11000.md](Elmo_Master_history_260803_1_part_044_lines_10751_11000.md) |
| 045 | 11,001-11,250 | [Elmo_Master_history_260803_1_part_045_lines_11001_11250.md](Elmo_Master_history_260803_1_part_045_lines_11001_11250.md) |
| 046 | 11,251-11,500 | [Elmo_Master_history_260803_1_part_046_lines_11251_11500.md](Elmo_Master_history_260803_1_part_046_lines_11251_11500.md) |
| 047 | 11,501-11,750 | [Elmo_Master_history_260803_1_part_047_lines_11501_11750.md](Elmo_Master_history_260803_1_part_047_lines_11501_11750.md) |
| 048 | 11,751-12,000 | [Elmo_Master_history_260803_1_part_048_lines_11751_12000.md](Elmo_Master_history_260803_1_part_048_lines_11751_12000.md) |
| 049 | 12,001-12,250 | [Elmo_Master_history_260803_1_part_049_lines_12001_12250.md](Elmo_Master_history_260803_1_part_049_lines_12001_12250.md) |
| 050 | 12,251-12,500 | [Elmo_Master_history_260803_1_part_050_lines_12251_12500.md](Elmo_Master_history_260803_1_part_050_lines_12251_12500.md) |
| 051 | 12,501-12,750 | [Elmo_Master_history_260803_1_part_051_lines_12501_12750.md](Elmo_Master_history_260803_1_part_051_lines_12501_12750.md) |
| 052 | 12,751-13,000 | [Elmo_Master_history_260803_1_part_052_lines_12751_13000.md](Elmo_Master_history_260803_1_part_052_lines_12751_13000.md) |
| 053 | 13,001-13,250 | [Elmo_Master_history_260803_1_part_053_lines_13001_13250.md](Elmo_Master_history_260803_1_part_053_lines_13001_13250.md) |
| 054 | 13,251-13,500 | [Elmo_Master_history_260803_1_part_054_lines_13251_13500.md](Elmo_Master_history_260803_1_part_054_lines_13251_13500.md) |
| 055 | 13,501-13,750 | [Elmo_Master_history_260803_1_part_055_lines_13501_13750.md](Elmo_Master_history_260803_1_part_055_lines_13501_13750.md) |
| 056 | 13,751-14,000 | [Elmo_Master_history_260803_1_part_056_lines_13751_14000.md](Elmo_Master_history_260803_1_part_056_lines_13751_14000.md) |
| 057 | 14,001-14,250 | [Elmo_Master_history_260803_1_part_057_lines_14001_14250.md](Elmo_Master_history_260803_1_part_057_lines_14001_14250.md) |
| 058 | 14,251-14,500 | [Elmo_Master_history_260803_1_part_058_lines_14251_14500.md](Elmo_Master_history_260803_1_part_058_lines_14251_14500.md) |
| 059 | 14,501-14,750 | [Elmo_Master_history_260803_1_part_059_lines_14501_14750.md](Elmo_Master_history_260803_1_part_059_lines_14501_14750.md) |
| 060 | 14,751-15,000 | [Elmo_Master_history_260803_1_part_060_lines_14751_15000.md](Elmo_Master_history_260803_1_part_060_lines_14751_15000.md) |
| 061 | 15,001-15,250 | [Elmo_Master_history_260803_1_part_061_lines_15001_15250.md](Elmo_Master_history_260803_1_part_061_lines_15001_15250.md) |
| 062 | 15,251-15,500 | [Elmo_Master_history_260803_1_part_062_lines_15251_15500.md](Elmo_Master_history_260803_1_part_062_lines_15251_15500.md) |
| 063 | 15,501-15,750 | [Elmo_Master_history_260803_1_part_063_lines_15501_15750.md](Elmo_Master_history_260803_1_part_063_lines_15501_15750.md) |
| 064 | 15,751-16,000 | [Elmo_Master_history_260803_1_part_064_lines_15751_16000.md](Elmo_Master_history_260803_1_part_064_lines_15751_16000.md) |
| 065 | 16,001-16,250 | [Elmo_Master_history_260803_1_part_065_lines_16001_16250.md](Elmo_Master_history_260803_1_part_065_lines_16001_16250.md) |
| 066 | 16,251-16,500 | [Elmo_Master_history_260803_1_part_066_lines_16251_16500.md](Elmo_Master_history_260803_1_part_066_lines_16251_16500.md) |
| 067 | 16,501-16,750 | [Elmo_Master_history_260803_1_part_067_lines_16501_16750.md](Elmo_Master_history_260803_1_part_067_lines_16501_16750.md) |
| 068 | 16,751-17,000 | [Elmo_Master_history_260803_1_part_068_lines_16751_17000.md](Elmo_Master_history_260803_1_part_068_lines_16751_17000.md) |
| 069 | 17,001-17,250 | [Elmo_Master_history_260803_1_part_069_lines_17001_17250.md](Elmo_Master_history_260803_1_part_069_lines_17001_17250.md) |
| 070 | 17,251-17,500 | [Elmo_Master_history_260803_1_part_070_lines_17251_17500.md](Elmo_Master_history_260803_1_part_070_lines_17251_17500.md) |
| 071 | 17,501-17,750 | [Elmo_Master_history_260803_1_part_071_lines_17501_17750.md](Elmo_Master_history_260803_1_part_071_lines_17501_17750.md) |
| 072 | 17,751-18,000 | [Elmo_Master_history_260803_1_part_072_lines_17751_18000.md](Elmo_Master_history_260803_1_part_072_lines_17751_18000.md) |
| 073 | 18,001-18,250 | [Elmo_Master_history_260803_1_part_073_lines_18001_18250.md](Elmo_Master_history_260803_1_part_073_lines_18001_18250.md) |
| 074 | 18,251-18,500 | [Elmo_Master_history_260803_1_part_074_lines_18251_18500.md](Elmo_Master_history_260803_1_part_074_lines_18251_18500.md) |
| 075 | 18,501-18,750 | [Elmo_Master_history_260803_1_part_075_lines_18501_18750.md](Elmo_Master_history_260803_1_part_075_lines_18501_18750.md) |
| 076 | 18,751-19,000 | [Elmo_Master_history_260803_1_part_076_lines_18751_19000.md](Elmo_Master_history_260803_1_part_076_lines_18751_19000.md) |
| 077 | 19,001-19,250 | [Elmo_Master_history_260803_1_part_077_lines_19001_19250.md](Elmo_Master_history_260803_1_part_077_lines_19001_19250.md) |
| 078 | 19,251-19,500 | [Elmo_Master_history_260803_1_part_078_lines_19251_19500.md](Elmo_Master_history_260803_1_part_078_lines_19251_19500.md) |
| 079 | 19,501-19,750 | [Elmo_Master_history_260803_1_part_079_lines_19501_19750.md](Elmo_Master_history_260803_1_part_079_lines_19501_19750.md) |
| 080 | 19,751-20,000 | [Elmo_Master_history_260803_1_part_080_lines_19751_20000.md](Elmo_Master_history_260803_1_part_080_lines_19751_20000.md) |
| 081 | 20,001-20,250 | [Elmo_Master_history_260803_1_part_081_lines_20001_20250.md](Elmo_Master_history_260803_1_part_081_lines_20001_20250.md) |
| 082 | 20,251-20,500 | [Elmo_Master_history_260803_1_part_082_lines_20251_20500.md](Elmo_Master_history_260803_1_part_082_lines_20251_20500.md) |
| 083 | 20,501-20,750 | [Elmo_Master_history_260803_1_part_083_lines_20501_20750.md](Elmo_Master_history_260803_1_part_083_lines_20501_20750.md) |
| 084 | 20,751-21,000 | [Elmo_Master_history_260803_1_part_084_lines_20751_21000.md](Elmo_Master_history_260803_1_part_084_lines_20751_21000.md) |
| 085 | 21,001-21,250 | [Elmo_Master_history_260803_1_part_085_lines_21001_21250.md](Elmo_Master_history_260803_1_part_085_lines_21001_21250.md) |
| 086 | 21,251-21,500 | [Elmo_Master_history_260803_1_part_086_lines_21251_21500.md](Elmo_Master_history_260803_1_part_086_lines_21251_21500.md) |
| 087 | 21,501-21,750 | [Elmo_Master_history_260803_1_part_087_lines_21501_21750.md](Elmo_Master_history_260803_1_part_087_lines_21501_21750.md) |
| 088 | 21,751-22,000 | [Elmo_Master_history_260803_1_part_088_lines_21751_22000.md](Elmo_Master_history_260803_1_part_088_lines_21751_22000.md) |
| 089 | 22,001-22,250 | [Elmo_Master_history_260803_1_part_089_lines_22001_22250.md](Elmo_Master_history_260803_1_part_089_lines_22001_22250.md) |
| 090 | 22,251-22,500 | [Elmo_Master_history_260803_1_part_090_lines_22251_22500.md](Elmo_Master_history_260803_1_part_090_lines_22251_22500.md) |
| 091 | 22,501-22,750 | [Elmo_Master_history_260803_1_part_091_lines_22501_22750.md](Elmo_Master_history_260803_1_part_091_lines_22501_22750.md) |
| 092 | 22,751-23,000 | [Elmo_Master_history_260803_1_part_092_lines_22751_23000.md](Elmo_Master_history_260803_1_part_092_lines_22751_23000.md) |
| 093 | 23,001-23,250 | [Elmo_Master_history_260803_1_part_093_lines_23001_23250.md](Elmo_Master_history_260803_1_part_093_lines_23001_23250.md) |
| 094 | 23,251-23,500 | [Elmo_Master_history_260803_1_part_094_lines_23251_23500.md](Elmo_Master_history_260803_1_part_094_lines_23251_23500.md) |
| 095 | 23,501-23,750 | [Elmo_Master_history_260803_1_part_095_lines_23501_23750.md](Elmo_Master_history_260803_1_part_095_lines_23501_23750.md) |
| 096 | 23,751-24,000 | [Elmo_Master_history_260803_1_part_096_lines_23751_24000.md](Elmo_Master_history_260803_1_part_096_lines_23751_24000.md) |
| 097 | 24,001-24,250 | [Elmo_Master_history_260803_1_part_097_lines_24001_24250.md](Elmo_Master_history_260803_1_part_097_lines_24001_24250.md) |
| 098 | 24,251-24,500 | [Elmo_Master_history_260803_1_part_098_lines_24251_24500.md](Elmo_Master_history_260803_1_part_098_lines_24251_24500.md) |
| 099 | 24,501-24,750 | [Elmo_Master_history_260803_1_part_099_lines_24501_24750.md](Elmo_Master_history_260803_1_part_099_lines_24501_24750.md) |
| 100 | 24,751-25,000 | [Elmo_Master_history_260803_1_part_100_lines_24751_25000.md](Elmo_Master_history_260803_1_part_100_lines_24751_25000.md) |
| 101 | 25,001-25,250 | [Elmo_Master_history_260803_1_part_101_lines_25001_25250.md](Elmo_Master_history_260803_1_part_101_lines_25001_25250.md) |
| 102 | 25,251-25,500 | [Elmo_Master_history_260803_1_part_102_lines_25251_25500.md](Elmo_Master_history_260803_1_part_102_lines_25251_25500.md) |
| 103 | 25,501-25,750 | [Elmo_Master_history_260803_1_part_103_lines_25501_25750.md](Elmo_Master_history_260803_1_part_103_lines_25501_25750.md) |
| 104 | 25,751-26,000 | [Elmo_Master_history_260803_1_part_104_lines_25751_26000.md](Elmo_Master_history_260803_1_part_104_lines_25751_26000.md) |
| 105 | 26,001-26,250 | [Elmo_Master_history_260803_1_part_105_lines_26001_26250.md](Elmo_Master_history_260803_1_part_105_lines_26001_26250.md) |
| 106 | 26,251-26,500 | [Elmo_Master_history_260803_1_part_106_lines_26251_26500.md](Elmo_Master_history_260803_1_part_106_lines_26251_26500.md) |
| 107 | 26,501-26,750 | [Elmo_Master_history_260803_1_part_107_lines_26501_26750.md](Elmo_Master_history_260803_1_part_107_lines_26501_26750.md) |
| 108 | 26,751-27,000 | [Elmo_Master_history_260803_1_part_108_lines_26751_27000.md](Elmo_Master_history_260803_1_part_108_lines_26751_27000.md) |
| 109 | 27,001-27,250 | [Elmo_Master_history_260803_1_part_109_lines_27001_27250.md](Elmo_Master_history_260803_1_part_109_lines_27001_27250.md) |
| 110 | 27,251-27,500 | [Elmo_Master_history_260803_1_part_110_lines_27251_27500.md](Elmo_Master_history_260803_1_part_110_lines_27251_27500.md) |
| 111 | 27,501-27,750 | [Elmo_Master_history_260803_1_part_111_lines_27501_27750.md](Elmo_Master_history_260803_1_part_111_lines_27501_27750.md) |
| 112 | 27,751-28,000 | [Elmo_Master_history_260803_1_part_112_lines_27751_28000.md](Elmo_Master_history_260803_1_part_112_lines_27751_28000.md) |
| 113 | 28,001-28,250 | [Elmo_Master_history_260803_1_part_113_lines_28001_28250.md](Elmo_Master_history_260803_1_part_113_lines_28001_28250.md) |
| 114 | 28,251-28,500 | [Elmo_Master_history_260803_1_part_114_lines_28251_28500.md](Elmo_Master_history_260803_1_part_114_lines_28251_28500.md) |
| 115 | 28,501-28,750 | [Elmo_Master_history_260803_1_part_115_lines_28501_28750.md](Elmo_Master_history_260803_1_part_115_lines_28501_28750.md) |
| 116 | 28,751-29,000 | [Elmo_Master_history_260803_1_part_116_lines_28751_29000.md](Elmo_Master_history_260803_1_part_116_lines_28751_29000.md) |
| 117 | 29,001-29,250 | [Elmo_Master_history_260803_1_part_117_lines_29001_29250.md](Elmo_Master_history_260803_1_part_117_lines_29001_29250.md) |
| 118 | 29,251-29,500 | [Elmo_Master_history_260803_1_part_118_lines_29251_29500.md](Elmo_Master_history_260803_1_part_118_lines_29251_29500.md) |
| 119 | 29,501-29,750 | [Elmo_Master_history_260803_1_part_119_lines_29501_29750.md](Elmo_Master_history_260803_1_part_119_lines_29501_29750.md) |
| 120 | 29,751-30,000 | [Elmo_Master_history_260803_1_part_120_lines_29751_30000.md](Elmo_Master_history_260803_1_part_120_lines_29751_30000.md) |
| 121 | 30,001-30,250 | [Elmo_Master_history_260803_1_part_121_lines_30001_30250.md](Elmo_Master_history_260803_1_part_121_lines_30001_30250.md) |
| 122 | 30,251-30,500 | [Elmo_Master_history_260803_1_part_122_lines_30251_30500.md](Elmo_Master_history_260803_1_part_122_lines_30251_30500.md) |
| 123 | 30,501-30,750 | [Elmo_Master_history_260803_1_part_123_lines_30501_30750.md](Elmo_Master_history_260803_1_part_123_lines_30501_30750.md) |
| 124 | 30,751-31,000 | [Elmo_Master_history_260803_1_part_124_lines_30751_31000.md](Elmo_Master_history_260803_1_part_124_lines_30751_31000.md) |
| 125 | 31,001-31,250 | [Elmo_Master_history_260803_1_part_125_lines_31001_31250.md](Elmo_Master_history_260803_1_part_125_lines_31001_31250.md) |
| 126 | 31,251-31,500 | [Elmo_Master_history_260803_1_part_126_lines_31251_31500.md](Elmo_Master_history_260803_1_part_126_lines_31251_31500.md) |
| 127 | 31,501-31,750 | [Elmo_Master_history_260803_1_part_127_lines_31501_31750.md](Elmo_Master_history_260803_1_part_127_lines_31501_31750.md) |
| 128 | 31,751-32,000 | [Elmo_Master_history_260803_1_part_128_lines_31751_32000.md](Elmo_Master_history_260803_1_part_128_lines_31751_32000.md) |
| 129 | 32,001-32,250 | [Elmo_Master_history_260803_1_part_129_lines_32001_32250.md](Elmo_Master_history_260803_1_part_129_lines_32001_32250.md) |
| 130 | 32,251-32,500 | [Elmo_Master_history_260803_1_part_130_lines_32251_32500.md](Elmo_Master_history_260803_1_part_130_lines_32251_32500.md) |
| 131 | 32,501-32,750 | [Elmo_Master_history_260803_1_part_131_lines_32501_32750.md](Elmo_Master_history_260803_1_part_131_lines_32501_32750.md) |
| 132 | 32,751-33,000 | [Elmo_Master_history_260803_1_part_132_lines_32751_33000.md](Elmo_Master_history_260803_1_part_132_lines_32751_33000.md) |
| 133 | 33,001-33,250 | [Elmo_Master_history_260803_1_part_133_lines_33001_33250.md](Elmo_Master_history_260803_1_part_133_lines_33001_33250.md) |
| 134 | 33,251-33,500 | [Elmo_Master_history_260803_1_part_134_lines_33251_33500.md](Elmo_Master_history_260803_1_part_134_lines_33251_33500.md) |
| 135 | 33,501-33,750 | [Elmo_Master_history_260803_1_part_135_lines_33501_33750.md](Elmo_Master_history_260803_1_part_135_lines_33501_33750.md) |
| 136 | 33,751-34,000 | [Elmo_Master_history_260803_1_part_136_lines_33751_34000.md](Elmo_Master_history_260803_1_part_136_lines_33751_34000.md) |
| 137 | 34,001-34,250 | [Elmo_Master_history_260803_1_part_137_lines_34001_34250.md](Elmo_Master_history_260803_1_part_137_lines_34001_34250.md) |
| 138 | 34,251-34,500 | [Elmo_Master_history_260803_1_part_138_lines_34251_34500.md](Elmo_Master_history_260803_1_part_138_lines_34251_34500.md) |
| 139 | 34,501-34,750 | [Elmo_Master_history_260803_1_part_139_lines_34501_34750.md](Elmo_Master_history_260803_1_part_139_lines_34501_34750.md) |
| 140 | 34,751-35,000 | [Elmo_Master_history_260803_1_part_140_lines_34751_35000.md](Elmo_Master_history_260803_1_part_140_lines_34751_35000.md) |
| 141 | 35,001-35,250 | [Elmo_Master_history_260803_1_part_141_lines_35001_35250.md](Elmo_Master_history_260803_1_part_141_lines_35001_35250.md) |
| 142 | 35,251-35,500 | [Elmo_Master_history_260803_1_part_142_lines_35251_35500.md](Elmo_Master_history_260803_1_part_142_lines_35251_35500.md) |
| 143 | 35,501-35,750 | [Elmo_Master_history_260803_1_part_143_lines_35501_35750.md](Elmo_Master_history_260803_1_part_143_lines_35501_35750.md) |
| 144 | 35,751-36,000 | [Elmo_Master_history_260803_1_part_144_lines_35751_36000.md](Elmo_Master_history_260803_1_part_144_lines_35751_36000.md) |
| 145 | 36,001-36,250 | [Elmo_Master_history_260803_1_part_145_lines_36001_36250.md](Elmo_Master_history_260803_1_part_145_lines_36001_36250.md) |
| 146 | 36,251-36,500 | [Elmo_Master_history_260803_1_part_146_lines_36251_36500.md](Elmo_Master_history_260803_1_part_146_lines_36251_36500.md) |
| 147 | 36,501-36,750 | [Elmo_Master_history_260803_1_part_147_lines_36501_36750.md](Elmo_Master_history_260803_1_part_147_lines_36501_36750.md) |
| 148 | 36,751-37,000 | [Elmo_Master_history_260803_1_part_148_lines_36751_37000.md](Elmo_Master_history_260803_1_part_148_lines_36751_37000.md) |
| 149 | 37,001-37,250 | [Elmo_Master_history_260803_1_part_149_lines_37001_37250.md](Elmo_Master_history_260803_1_part_149_lines_37001_37250.md) |
| 150 | 37,251-37,500 | [Elmo_Master_history_260803_1_part_150_lines_37251_37500.md](Elmo_Master_history_260803_1_part_150_lines_37251_37500.md) |
| 151 | 37,501-37,750 | [Elmo_Master_history_260803_1_part_151_lines_37501_37750.md](Elmo_Master_history_260803_1_part_151_lines_37501_37750.md) |
| 152 | 37,751-38,000 | [Elmo_Master_history_260803_1_part_152_lines_37751_38000.md](Elmo_Master_history_260803_1_part_152_lines_37751_38000.md) |
| 153 | 38,001-38,250 | [Elmo_Master_history_260803_1_part_153_lines_38001_38250.md](Elmo_Master_history_260803_1_part_153_lines_38001_38250.md) |
| 154 | 38,251-38,500 | [Elmo_Master_history_260803_1_part_154_lines_38251_38500.md](Elmo_Master_history_260803_1_part_154_lines_38251_38500.md) |
| 155 | 38,501-38,750 | [Elmo_Master_history_260803_1_part_155_lines_38501_38750.md](Elmo_Master_history_260803_1_part_155_lines_38501_38750.md) |
| 156 | 38,751-39,000 | [Elmo_Master_history_260803_1_part_156_lines_38751_39000.md](Elmo_Master_history_260803_1_part_156_lines_38751_39000.md) |
| 157 | 39,001-39,250 | [Elmo_Master_history_260803_1_part_157_lines_39001_39250.md](Elmo_Master_history_260803_1_part_157_lines_39001_39250.md) |
| 158 | 39,251-39,500 | [Elmo_Master_history_260803_1_part_158_lines_39251_39500.md](Elmo_Master_history_260803_1_part_158_lines_39251_39500.md) |
| 159 | 39,501-39,750 | [Elmo_Master_history_260803_1_part_159_lines_39501_39750.md](Elmo_Master_history_260803_1_part_159_lines_39501_39750.md) |
| 160 | 39,751-40,000 | [Elmo_Master_history_260803_1_part_160_lines_39751_40000.md](Elmo_Master_history_260803_1_part_160_lines_39751_40000.md) |
| 161 | 40,001-40,250 | [Elmo_Master_history_260803_1_part_161_lines_40001_40250.md](Elmo_Master_history_260803_1_part_161_lines_40001_40250.md) |
| 162 | 40,251-40,500 | [Elmo_Master_history_260803_1_part_162_lines_40251_40500.md](Elmo_Master_history_260803_1_part_162_lines_40251_40500.md) |
| 163 | 40,501-40,750 | [Elmo_Master_history_260803_1_part_163_lines_40501_40750.md](Elmo_Master_history_260803_1_part_163_lines_40501_40750.md) |
| 164 | 40,751-41,000 | [Elmo_Master_history_260803_1_part_164_lines_40751_41000.md](Elmo_Master_history_260803_1_part_164_lines_40751_41000.md) |
| 165 | 41,001-41,250 | [Elmo_Master_history_260803_1_part_165_lines_41001_41250.md](Elmo_Master_history_260803_1_part_165_lines_41001_41250.md) |
| 166 | 41,251-41,500 | [Elmo_Master_history_260803_1_part_166_lines_41251_41500.md](Elmo_Master_history_260803_1_part_166_lines_41251_41500.md) |
| 167 | 41,501-41,750 | [Elmo_Master_history_260803_1_part_167_lines_41501_41750.md](Elmo_Master_history_260803_1_part_167_lines_41501_41750.md) |
| 168 | 41,751-42,000 | [Elmo_Master_history_260803_1_part_168_lines_41751_42000.md](Elmo_Master_history_260803_1_part_168_lines_41751_42000.md) |
| 169 | 42,001-42,250 | [Elmo_Master_history_260803_1_part_169_lines_42001_42250.md](Elmo_Master_history_260803_1_part_169_lines_42001_42250.md) |
| 170 | 42,251-42,500 | [Elmo_Master_history_260803_1_part_170_lines_42251_42500.md](Elmo_Master_history_260803_1_part_170_lines_42251_42500.md) |
| 171 | 42,501-42,750 | [Elmo_Master_history_260803_1_part_171_lines_42501_42750.md](Elmo_Master_history_260803_1_part_171_lines_42501_42750.md) |
| 172 | 42,751-43,000 | [Elmo_Master_history_260803_1_part_172_lines_42751_43000.md](Elmo_Master_history_260803_1_part_172_lines_42751_43000.md) |
| 173 | 43,001-43,250 | [Elmo_Master_history_260803_1_part_173_lines_43001_43250.md](Elmo_Master_history_260803_1_part_173_lines_43001_43250.md) |
| 174 | 43,251-43,500 | [Elmo_Master_history_260803_1_part_174_lines_43251_43500.md](Elmo_Master_history_260803_1_part_174_lines_43251_43500.md) |
| 175 | 43,501-43,750 | [Elmo_Master_history_260803_1_part_175_lines_43501_43750.md](Elmo_Master_history_260803_1_part_175_lines_43501_43750.md) |
| 176 | 43,751-44,000 | [Elmo_Master_history_260803_1_part_176_lines_43751_44000.md](Elmo_Master_history_260803_1_part_176_lines_43751_44000.md) |
| 177 | 44,001-44,250 | [Elmo_Master_history_260803_1_part_177_lines_44001_44250.md](Elmo_Master_history_260803_1_part_177_lines_44001_44250.md) |
| 178 | 44,251-44,500 | [Elmo_Master_history_260803_1_part_178_lines_44251_44500.md](Elmo_Master_history_260803_1_part_178_lines_44251_44500.md) |
| 179 | 44,501-44,750 | [Elmo_Master_history_260803_1_part_179_lines_44501_44750.md](Elmo_Master_history_260803_1_part_179_lines_44501_44750.md) |
| 180 | 44,751-45,000 | [Elmo_Master_history_260803_1_part_180_lines_44751_45000.md](Elmo_Master_history_260803_1_part_180_lines_44751_45000.md) |
| 181 | 45,001-45,250 | [Elmo_Master_history_260803_1_part_181_lines_45001_45250.md](Elmo_Master_history_260803_1_part_181_lines_45001_45250.md) |
| 182 | 45,251-45,500 | [Elmo_Master_history_260803_1_part_182_lines_45251_45500.md](Elmo_Master_history_260803_1_part_182_lines_45251_45500.md) |
| 183 | 45,501-45,750 | [Elmo_Master_history_260803_1_part_183_lines_45501_45750.md](Elmo_Master_history_260803_1_part_183_lines_45501_45750.md) |
| 184 | 45,751-46,000 | [Elmo_Master_history_260803_1_part_184_lines_45751_46000.md](Elmo_Master_history_260803_1_part_184_lines_45751_46000.md) |
| 185 | 46,001-46,250 | [Elmo_Master_history_260803_1_part_185_lines_46001_46250.md](Elmo_Master_history_260803_1_part_185_lines_46001_46250.md) |
| 186 | 46,251-46,500 | [Elmo_Master_history_260803_1_part_186_lines_46251_46500.md](Elmo_Master_history_260803_1_part_186_lines_46251_46500.md) |
| 187 | 46,501-46,750 | [Elmo_Master_history_260803_1_part_187_lines_46501_46750.md](Elmo_Master_history_260803_1_part_187_lines_46501_46750.md) |
| 188 | 46,751-47,000 | [Elmo_Master_history_260803_1_part_188_lines_46751_47000.md](Elmo_Master_history_260803_1_part_188_lines_46751_47000.md) |
| 189 | 47,001-47,250 | [Elmo_Master_history_260803_1_part_189_lines_47001_47250.md](Elmo_Master_history_260803_1_part_189_lines_47001_47250.md) |
| 190 | 47,251-47,500 | [Elmo_Master_history_260803_1_part_190_lines_47251_47500.md](Elmo_Master_history_260803_1_part_190_lines_47251_47500.md) |
| 191 | 47,501-47,750 | [Elmo_Master_history_260803_1_part_191_lines_47501_47750.md](Elmo_Master_history_260803_1_part_191_lines_47501_47750.md) |
| 192 | 47,751-48,000 | [Elmo_Master_history_260803_1_part_192_lines_47751_48000.md](Elmo_Master_history_260803_1_part_192_lines_47751_48000.md) |
| 193 | 48,001-48,250 | [Elmo_Master_history_260803_1_part_193_lines_48001_48250.md](Elmo_Master_history_260803_1_part_193_lines_48001_48250.md) |
| 194 | 48,251-48,500 | [Elmo_Master_history_260803_1_part_194_lines_48251_48500.md](Elmo_Master_history_260803_1_part_194_lines_48251_48500.md) |
| 195 | 48,501-48,750 | [Elmo_Master_history_260803_1_part_195_lines_48501_48750.md](Elmo_Master_history_260803_1_part_195_lines_48501_48750.md) |
| 196 | 48,751-49,000 | [Elmo_Master_history_260803_1_part_196_lines_48751_49000.md](Elmo_Master_history_260803_1_part_196_lines_48751_49000.md) |
| 197 | 49,001-49,250 | [Elmo_Master_history_260803_1_part_197_lines_49001_49250.md](Elmo_Master_history_260803_1_part_197_lines_49001_49250.md) |
| 198 | 49,251-49,500 | [Elmo_Master_history_260803_1_part_198_lines_49251_49500.md](Elmo_Master_history_260803_1_part_198_lines_49251_49500.md) |
| 199 | 49,501-49,750 | [Elmo_Master_history_260803_1_part_199_lines_49501_49750.md](Elmo_Master_history_260803_1_part_199_lines_49501_49750.md) |
| 200 | 49,751-50,000 | [Elmo_Master_history_260803_1_part_200_lines_49751_50000.md](Elmo_Master_history_260803_1_part_200_lines_49751_50000.md) |
| 201 | 50,001-50,250 | [Elmo_Master_history_260803_1_part_201_lines_50001_50250.md](Elmo_Master_history_260803_1_part_201_lines_50001_50250.md) |
| 202 | 50,251-50,500 | [Elmo_Master_history_260803_1_part_202_lines_50251_50500.md](Elmo_Master_history_260803_1_part_202_lines_50251_50500.md) |
| 203 | 50,501-50,750 | [Elmo_Master_history_260803_1_part_203_lines_50501_50750.md](Elmo_Master_history_260803_1_part_203_lines_50501_50750.md) |
| 204 | 50,751-51,000 | [Elmo_Master_history_260803_1_part_204_lines_50751_51000.md](Elmo_Master_history_260803_1_part_204_lines_50751_51000.md) |
| 205 | 51,001-51,250 | [Elmo_Master_history_260803_1_part_205_lines_51001_51250.md](Elmo_Master_history_260803_1_part_205_lines_51001_51250.md) |
| 206 | 51,251-51,500 | [Elmo_Master_history_260803_1_part_206_lines_51251_51500.md](Elmo_Master_history_260803_1_part_206_lines_51251_51500.md) |
| 207 | 51,501-51,750 | [Elmo_Master_history_260803_1_part_207_lines_51501_51750.md](Elmo_Master_history_260803_1_part_207_lines_51501_51750.md) |
| 208 | 51,751-52,000 | [Elmo_Master_history_260803_1_part_208_lines_51751_52000.md](Elmo_Master_history_260803_1_part_208_lines_51751_52000.md) |
| 209 | 52,001-52,250 | [Elmo_Master_history_260803_1_part_209_lines_52001_52250.md](Elmo_Master_history_260803_1_part_209_lines_52001_52250.md) |
| 210 | 52,251-52,500 | [Elmo_Master_history_260803_1_part_210_lines_52251_52500.md](Elmo_Master_history_260803_1_part_210_lines_52251_52500.md) |
| 211 | 52,501-52,750 | [Elmo_Master_history_260803_1_part_211_lines_52501_52750.md](Elmo_Master_history_260803_1_part_211_lines_52501_52750.md) |
| 212 | 52,751-53,000 | [Elmo_Master_history_260803_1_part_212_lines_52751_53000.md](Elmo_Master_history_260803_1_part_212_lines_52751_53000.md) |
| 213 | 53,001-53,250 | [Elmo_Master_history_260803_1_part_213_lines_53001_53250.md](Elmo_Master_history_260803_1_part_213_lines_53001_53250.md) |
| 214 | 53,251-53,500 | [Elmo_Master_history_260803_1_part_214_lines_53251_53500.md](Elmo_Master_history_260803_1_part_214_lines_53251_53500.md) |
| 215 | 53,501-53,750 | [Elmo_Master_history_260803_1_part_215_lines_53501_53750.md](Elmo_Master_history_260803_1_part_215_lines_53501_53750.md) |
| 216 | 53,751-54,000 | [Elmo_Master_history_260803_1_part_216_lines_53751_54000.md](Elmo_Master_history_260803_1_part_216_lines_53751_54000.md) |
| 217 | 54,001-54,250 | [Elmo_Master_history_260803_1_part_217_lines_54001_54250.md](Elmo_Master_history_260803_1_part_217_lines_54001_54250.md) |
| 218 | 54,251-54,500 | [Elmo_Master_history_260803_1_part_218_lines_54251_54500.md](Elmo_Master_history_260803_1_part_218_lines_54251_54500.md) |
| 219 | 54,501-54,750 | [Elmo_Master_history_260803_1_part_219_lines_54501_54750.md](Elmo_Master_history_260803_1_part_219_lines_54501_54750.md) |
| 220 | 54,751-55,000 | [Elmo_Master_history_260803_1_part_220_lines_54751_55000.md](Elmo_Master_history_260803_1_part_220_lines_54751_55000.md) |
| 221 | 55,001-55,250 | [Elmo_Master_history_260803_1_part_221_lines_55001_55250.md](Elmo_Master_history_260803_1_part_221_lines_55001_55250.md) |
| 222 | 55,251-55,500 | [Elmo_Master_history_260803_1_part_222_lines_55251_55500.md](Elmo_Master_history_260803_1_part_222_lines_55251_55500.md) |
| 223 | 55,501-55,750 | [Elmo_Master_history_260803_1_part_223_lines_55501_55750.md](Elmo_Master_history_260803_1_part_223_lines_55501_55750.md) |
| 224 | 55,751-56,000 | [Elmo_Master_history_260803_1_part_224_lines_55751_56000.md](Elmo_Master_history_260803_1_part_224_lines_55751_56000.md) |
| 225 | 56,001-56,250 | [Elmo_Master_history_260803_1_part_225_lines_56001_56250.md](Elmo_Master_history_260803_1_part_225_lines_56001_56250.md) |
| 226 | 56,251-56,500 | [Elmo_Master_history_260803_1_part_226_lines_56251_56500.md](Elmo_Master_history_260803_1_part_226_lines_56251_56500.md) |
| 227 | 56,501-56,750 | [Elmo_Master_history_260803_1_part_227_lines_56501_56750.md](Elmo_Master_history_260803_1_part_227_lines_56501_56750.md) |
| 228 | 56,751-57,000 | [Elmo_Master_history_260803_1_part_228_lines_56751_57000.md](Elmo_Master_history_260803_1_part_228_lines_56751_57000.md) |
| 229 | 57,001-57,250 | [Elmo_Master_history_260803_1_part_229_lines_57001_57250.md](Elmo_Master_history_260803_1_part_229_lines_57001_57250.md) |
| 230 | 57,251-57,500 | [Elmo_Master_history_260803_1_part_230_lines_57251_57500.md](Elmo_Master_history_260803_1_part_230_lines_57251_57500.md) |
| 231 | 57,501-57,750 | [Elmo_Master_history_260803_1_part_231_lines_57501_57750.md](Elmo_Master_history_260803_1_part_231_lines_57501_57750.md) |
| 232 | 57,751-58,000 | [Elmo_Master_history_260803_1_part_232_lines_57751_58000.md](Elmo_Master_history_260803_1_part_232_lines_57751_58000.md) |
| 233 | 58,001-58,250 | [Elmo_Master_history_260803_1_part_233_lines_58001_58250.md](Elmo_Master_history_260803_1_part_233_lines_58001_58250.md) |
| 234 | 58,251-58,500 | [Elmo_Master_history_260803_1_part_234_lines_58251_58500.md](Elmo_Master_history_260803_1_part_234_lines_58251_58500.md) |
| 235 | 58,501-58,750 | [Elmo_Master_history_260803_1_part_235_lines_58501_58750.md](Elmo_Master_history_260803_1_part_235_lines_58501_58750.md) |
| 236 | 58,751-59,000 | [Elmo_Master_history_260803_1_part_236_lines_58751_59000.md](Elmo_Master_history_260803_1_part_236_lines_58751_59000.md) |
| 237 | 59,001-59,250 | [Elmo_Master_history_260803_1_part_237_lines_59001_59250.md](Elmo_Master_history_260803_1_part_237_lines_59001_59250.md) |
| 238 | 59,251-59,500 | [Elmo_Master_history_260803_1_part_238_lines_59251_59500.md](Elmo_Master_history_260803_1_part_238_lines_59251_59500.md) |
| 239 | 59,501-59,750 | [Elmo_Master_history_260803_1_part_239_lines_59501_59750.md](Elmo_Master_history_260803_1_part_239_lines_59501_59750.md) |
| 240 | 59,751-60,000 | [Elmo_Master_history_260803_1_part_240_lines_59751_60000.md](Elmo_Master_history_260803_1_part_240_lines_59751_60000.md) |
| 241 | 60,001-60,250 | [Elmo_Master_history_260803_1_part_241_lines_60001_60250.md](Elmo_Master_history_260803_1_part_241_lines_60001_60250.md) |
| 242 | 60,251-60,500 | [Elmo_Master_history_260803_1_part_242_lines_60251_60500.md](Elmo_Master_history_260803_1_part_242_lines_60251_60500.md) |
| 243 | 60,501-60,750 | [Elmo_Master_history_260803_1_part_243_lines_60501_60750.md](Elmo_Master_history_260803_1_part_243_lines_60501_60750.md) |
| 244 | 60,751-61,000 | [Elmo_Master_history_260803_1_part_244_lines_60751_61000.md](Elmo_Master_history_260803_1_part_244_lines_60751_61000.md) |
| 245 | 61,001-61,250 | [Elmo_Master_history_260803_1_part_245_lines_61001_61250.md](Elmo_Master_history_260803_1_part_245_lines_61001_61250.md) |
| 246 | 61,251-61,500 | [Elmo_Master_history_260803_1_part_246_lines_61251_61500.md](Elmo_Master_history_260803_1_part_246_lines_61251_61500.md) |
| 247 | 61,501-61,750 | [Elmo_Master_history_260803_1_part_247_lines_61501_61750.md](Elmo_Master_history_260803_1_part_247_lines_61501_61750.md) |
| 248 | 61,751-62,000 | [Elmo_Master_history_260803_1_part_248_lines_61751_62000.md](Elmo_Master_history_260803_1_part_248_lines_61751_62000.md) |
| 249 | 62,001-62,250 | [Elmo_Master_history_260803_1_part_249_lines_62001_62250.md](Elmo_Master_history_260803_1_part_249_lines_62001_62250.md) |
| 250 | 62,251-62,500 | [Elmo_Master_history_260803_1_part_250_lines_62251_62500.md](Elmo_Master_history_260803_1_part_250_lines_62251_62500.md) |
| 251 | 62,501-62,750 | [Elmo_Master_history_260803_1_part_251_lines_62501_62750.md](Elmo_Master_history_260803_1_part_251_lines_62501_62750.md) |
| 252 | 62,751-63,000 | [Elmo_Master_history_260803_1_part_252_lines_62751_63000.md](Elmo_Master_history_260803_1_part_252_lines_62751_63000.md) |
| 253 | 63,001-63,250 | [Elmo_Master_history_260803_1_part_253_lines_63001_63250.md](Elmo_Master_history_260803_1_part_253_lines_63001_63250.md) |
| 254 | 63,251-63,500 | [Elmo_Master_history_260803_1_part_254_lines_63251_63500.md](Elmo_Master_history_260803_1_part_254_lines_63251_63500.md) |
| 255 | 63,501-63,750 | [Elmo_Master_history_260803_1_part_255_lines_63501_63750.md](Elmo_Master_history_260803_1_part_255_lines_63501_63750.md) |
| 256 | 63,751-64,000 | [Elmo_Master_history_260803_1_part_256_lines_63751_64000.md](Elmo_Master_history_260803_1_part_256_lines_63751_64000.md) |
| 257 | 64,001-64,250 | [Elmo_Master_history_260803_1_part_257_lines_64001_64250.md](Elmo_Master_history_260803_1_part_257_lines_64001_64250.md) |
| 258 | 64,251-64,500 | [Elmo_Master_history_260803_1_part_258_lines_64251_64500.md](Elmo_Master_history_260803_1_part_258_lines_64251_64500.md) |
| 259 | 64,501-64,750 | [Elmo_Master_history_260803_1_part_259_lines_64501_64750.md](Elmo_Master_history_260803_1_part_259_lines_64501_64750.md) |
| 260 | 64,751-65,000 | [Elmo_Master_history_260803_1_part_260_lines_64751_65000.md](Elmo_Master_history_260803_1_part_260_lines_64751_65000.md) |
| 261 | 65,001-65,250 | [Elmo_Master_history_260803_1_part_261_lines_65001_65250.md](Elmo_Master_history_260803_1_part_261_lines_65001_65250.md) |
| 262 | 65,251-65,500 | [Elmo_Master_history_260803_1_part_262_lines_65251_65500.md](Elmo_Master_history_260803_1_part_262_lines_65251_65500.md) |
| 263 | 65,501-65,750 | [Elmo_Master_history_260803_1_part_263_lines_65501_65750.md](Elmo_Master_history_260803_1_part_263_lines_65501_65750.md) |
| 264 | 65,751-66,000 | [Elmo_Master_history_260803_1_part_264_lines_65751_66000.md](Elmo_Master_history_260803_1_part_264_lines_65751_66000.md) |
| 265 | 66,001-66,250 | [Elmo_Master_history_260803_1_part_265_lines_66001_66250.md](Elmo_Master_history_260803_1_part_265_lines_66001_66250.md) |
| 266 | 66,251-66,500 | [Elmo_Master_history_260803_1_part_266_lines_66251_66500.md](Elmo_Master_history_260803_1_part_266_lines_66251_66500.md) |
| 267 | 66,501-66,750 | [Elmo_Master_history_260803_1_part_267_lines_66501_66750.md](Elmo_Master_history_260803_1_part_267_lines_66501_66750.md) |
| 268 | 66,751-67,000 | [Elmo_Master_history_260803_1_part_268_lines_66751_67000.md](Elmo_Master_history_260803_1_part_268_lines_66751_67000.md) |
| 269 | 67,001-67,250 | [Elmo_Master_history_260803_1_part_269_lines_67001_67250.md](Elmo_Master_history_260803_1_part_269_lines_67001_67250.md) |
| 270 | 67,251-67,500 | [Elmo_Master_history_260803_1_part_270_lines_67251_67500.md](Elmo_Master_history_260803_1_part_270_lines_67251_67500.md) |
| 271 | 67,501-67,750 | [Elmo_Master_history_260803_1_part_271_lines_67501_67750.md](Elmo_Master_history_260803_1_part_271_lines_67501_67750.md) |
| 272 | 67,751-68,000 | [Elmo_Master_history_260803_1_part_272_lines_67751_68000.md](Elmo_Master_history_260803_1_part_272_lines_67751_68000.md) |
| 273 | 68,001-68,250 | [Elmo_Master_history_260803_1_part_273_lines_68001_68250.md](Elmo_Master_history_260803_1_part_273_lines_68001_68250.md) |
| 274 | 68,251-68,500 | [Elmo_Master_history_260803_1_part_274_lines_68251_68500.md](Elmo_Master_history_260803_1_part_274_lines_68251_68500.md) |
| 275 | 68,501-68,750 | [Elmo_Master_history_260803_1_part_275_lines_68501_68750.md](Elmo_Master_history_260803_1_part_275_lines_68501_68750.md) |
| 276 | 68,751-69,000 | [Elmo_Master_history_260803_1_part_276_lines_68751_69000.md](Elmo_Master_history_260803_1_part_276_lines_68751_69000.md) |
| 277 | 69,001-69,250 | [Elmo_Master_history_260803_1_part_277_lines_69001_69250.md](Elmo_Master_history_260803_1_part_277_lines_69001_69250.md) |
| 278 | 69,251-69,500 | [Elmo_Master_history_260803_1_part_278_lines_69251_69500.md](Elmo_Master_history_260803_1_part_278_lines_69251_69500.md) |
| 279 | 69,501-69,750 | [Elmo_Master_history_260803_1_part_279_lines_69501_69750.md](Elmo_Master_history_260803_1_part_279_lines_69501_69750.md) |
| 280 | 69,751-70,000 | [Elmo_Master_history_260803_1_part_280_lines_69751_70000.md](Elmo_Master_history_260803_1_part_280_lines_69751_70000.md) |
| 281 | 70,001-70,250 | [Elmo_Master_history_260803_1_part_281_lines_70001_70250.md](Elmo_Master_history_260803_1_part_281_lines_70001_70250.md) |
| 282 | 70,251-70,500 | [Elmo_Master_history_260803_1_part_282_lines_70251_70500.md](Elmo_Master_history_260803_1_part_282_lines_70251_70500.md) |
| 283 | 70,501-70,750 | [Elmo_Master_history_260803_1_part_283_lines_70501_70750.md](Elmo_Master_history_260803_1_part_283_lines_70501_70750.md) |
| 284 | 70,751-71,000 | [Elmo_Master_history_260803_1_part_284_lines_70751_71000.md](Elmo_Master_history_260803_1_part_284_lines_70751_71000.md) |
| 285 | 71,001-71,250 | [Elmo_Master_history_260803_1_part_285_lines_71001_71250.md](Elmo_Master_history_260803_1_part_285_lines_71001_71250.md) |
| 286 | 71,251-71,500 | [Elmo_Master_history_260803_1_part_286_lines_71251_71500.md](Elmo_Master_history_260803_1_part_286_lines_71251_71500.md) |
| 287 | 71,501-71,750 | [Elmo_Master_history_260803_1_part_287_lines_71501_71750.md](Elmo_Master_history_260803_1_part_287_lines_71501_71750.md) |
| 288 | 71,751-72,000 | [Elmo_Master_history_260803_1_part_288_lines_71751_72000.md](Elmo_Master_history_260803_1_part_288_lines_71751_72000.md) |
| 289 | 72,001-72,250 | [Elmo_Master_history_260803_1_part_289_lines_72001_72250.md](Elmo_Master_history_260803_1_part_289_lines_72001_72250.md) |
| 290 | 72,251-72,500 | [Elmo_Master_history_260803_1_part_290_lines_72251_72500.md](Elmo_Master_history_260803_1_part_290_lines_72251_72500.md) |
| 291 | 72,501-72,750 | [Elmo_Master_history_260803_1_part_291_lines_72501_72750.md](Elmo_Master_history_260803_1_part_291_lines_72501_72750.md) |
| 292 | 72,751-73,000 | [Elmo_Master_history_260803_1_part_292_lines_72751_73000.md](Elmo_Master_history_260803_1_part_292_lines_72751_73000.md) |
| 293 | 73,001-73,250 | [Elmo_Master_history_260803_1_part_293_lines_73001_73250.md](Elmo_Master_history_260803_1_part_293_lines_73001_73250.md) |
| 294 | 73,251-73,500 | [Elmo_Master_history_260803_1_part_294_lines_73251_73500.md](Elmo_Master_history_260803_1_part_294_lines_73251_73500.md) |
| 295 | 73,501-73,750 | [Elmo_Master_history_260803_1_part_295_lines_73501_73750.md](Elmo_Master_history_260803_1_part_295_lines_73501_73750.md) |
| 296 | 73,751-74,000 | [Elmo_Master_history_260803_1_part_296_lines_73751_74000.md](Elmo_Master_history_260803_1_part_296_lines_73751_74000.md) |
| 297 | 74,001-74,250 | [Elmo_Master_history_260803_1_part_297_lines_74001_74250.md](Elmo_Master_history_260803_1_part_297_lines_74001_74250.md) |
| 298 | 74,251-74,500 | [Elmo_Master_history_260803_1_part_298_lines_74251_74500.md](Elmo_Master_history_260803_1_part_298_lines_74251_74500.md) |
| 299 | 74,501-74,750 | [Elmo_Master_history_260803_1_part_299_lines_74501_74750.md](Elmo_Master_history_260803_1_part_299_lines_74501_74750.md) |
| 300 | 74,751-75,000 | [Elmo_Master_history_260803_1_part_300_lines_74751_75000.md](Elmo_Master_history_260803_1_part_300_lines_74751_75000.md) |
| 301 | 75,001-75,250 | [Elmo_Master_history_260803_1_part_301_lines_75001_75250.md](Elmo_Master_history_260803_1_part_301_lines_75001_75250.md) |
| 302 | 75,251-75,500 | [Elmo_Master_history_260803_1_part_302_lines_75251_75500.md](Elmo_Master_history_260803_1_part_302_lines_75251_75500.md) |
| 303 | 75,501-75,750 | [Elmo_Master_history_260803_1_part_303_lines_75501_75750.md](Elmo_Master_history_260803_1_part_303_lines_75501_75750.md) |
| 304 | 75,751-76,000 | [Elmo_Master_history_260803_1_part_304_lines_75751_76000.md](Elmo_Master_history_260803_1_part_304_lines_75751_76000.md) |
| 305 | 76,001-76,250 | [Elmo_Master_history_260803_1_part_305_lines_76001_76250.md](Elmo_Master_history_260803_1_part_305_lines_76001_76250.md) |
| 306 | 76,251-76,500 | [Elmo_Master_history_260803_1_part_306_lines_76251_76500.md](Elmo_Master_history_260803_1_part_306_lines_76251_76500.md) |
| 307 | 76,501-76,750 | [Elmo_Master_history_260803_1_part_307_lines_76501_76750.md](Elmo_Master_history_260803_1_part_307_lines_76501_76750.md) |
| 308 | 76,751-77,000 | [Elmo_Master_history_260803_1_part_308_lines_76751_77000.md](Elmo_Master_history_260803_1_part_308_lines_76751_77000.md) |
| 309 | 77,001-77,250 | [Elmo_Master_history_260803_1_part_309_lines_77001_77250.md](Elmo_Master_history_260803_1_part_309_lines_77001_77250.md) |
| 310 | 77,251-77,500 | [Elmo_Master_history_260803_1_part_310_lines_77251_77500.md](Elmo_Master_history_260803_1_part_310_lines_77251_77500.md) |
| 311 | 77,501-77,750 | [Elmo_Master_history_260803_1_part_311_lines_77501_77750.md](Elmo_Master_history_260803_1_part_311_lines_77501_77750.md) |
| 312 | 77,751-78,000 | [Elmo_Master_history_260803_1_part_312_lines_77751_78000.md](Elmo_Master_history_260803_1_part_312_lines_77751_78000.md) |
| 313 | 78,001-78,250 | [Elmo_Master_history_260803_1_part_313_lines_78001_78250.md](Elmo_Master_history_260803_1_part_313_lines_78001_78250.md) |
| 314 | 78,251-78,500 | [Elmo_Master_history_260803_1_part_314_lines_78251_78500.md](Elmo_Master_history_260803_1_part_314_lines_78251_78500.md) |
| 315 | 78,501-78,750 | [Elmo_Master_history_260803_1_part_315_lines_78501_78750.md](Elmo_Master_history_260803_1_part_315_lines_78501_78750.md) |
| 316 | 78,751-79,000 | [Elmo_Master_history_260803_1_part_316_lines_78751_79000.md](Elmo_Master_history_260803_1_part_316_lines_78751_79000.md) |
| 317 | 79,001-79,250 | [Elmo_Master_history_260803_1_part_317_lines_79001_79250.md](Elmo_Master_history_260803_1_part_317_lines_79001_79250.md) |
| 318 | 79,251-79,500 | [Elmo_Master_history_260803_1_part_318_lines_79251_79500.md](Elmo_Master_history_260803_1_part_318_lines_79251_79500.md) |
| 319 | 79,501-79,750 | [Elmo_Master_history_260803_1_part_319_lines_79501_79750.md](Elmo_Master_history_260803_1_part_319_lines_79501_79750.md) |
| 320 | 79,751-80,000 | [Elmo_Master_history_260803_1_part_320_lines_79751_80000.md](Elmo_Master_history_260803_1_part_320_lines_79751_80000.md) |
| 321 | 80,001-80,250 | [Elmo_Master_history_260803_1_part_321_lines_80001_80250.md](Elmo_Master_history_260803_1_part_321_lines_80001_80250.md) |
| 322 | 80,251-80,500 | [Elmo_Master_history_260803_1_part_322_lines_80251_80500.md](Elmo_Master_history_260803_1_part_322_lines_80251_80500.md) |

## Elmo_Master_history_260803_2.md

- Source: `docs/history/Elmo_Master_history_260803_2.md`
- Size: 33,631 bytes
- Lines: 531
- Source SHA-256: `885d994de33de89edbe0772ef01f4508d4a064281ffadc28a5013b6ba6a29e5e`
- Readable chunks: 3
- Base64-bearing source lines replaced: 0
- Trailing-whitespace source lines normalized: 0

| Part | Source lines | File |
|---:|---:|---|
| 001 | 1-250 | [Elmo_Master_history_260803_2_part_001_lines_00001_00250.md](Elmo_Master_history_260803_2_part_001_lines_00001_00250.md) |
| 002 | 251-500 | [Elmo_Master_history_260803_2_part_002_lines_00251_00500.md](Elmo_Master_history_260803_2_part_002_lines_00251_00500.md) |
| 003 | 501-531 | [Elmo_Master_history_260803_2_part_003_lines_00501_00531.md](Elmo_Master_history_260803_2_part_003_lines_00501_00531.md) |

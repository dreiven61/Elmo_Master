# Elmo Master history split index - 2026-08-19

작성일: 2026-08-19 (KST)

## Source and integrity

- Source: `../Elmo_Master_history_260819.md`
- Source bytes: 185764439
- Source lines: 53925
- Source SHA-256: `127D66642A40E57C8F0908083303C845B781282A7AFE8AAE23BFC4751928DC6D`
- Split rule: 250 source lines per chunk
- Oversized-line rule: source lines longer than 4000 characters are replaced only in split copies
- Omitted oversized lines: 410
- Omission manifest: [01_omitted_payload_manifest.csv](./01_omitted_payload_manifest.csv)
- Splitter: [Split-HistoryExport.ps1](./Split-HistoryExport.ps1)
- Readable chunk total: 2515662 bytes; maximum chunk: 23120 bytes
- The original source file is unchanged and remains the lossless record.

## Topic navigation

The ranges below follow transcript order, not trustworthy per-turn timestamps.

| Source lines | Parts | Topic | Evidence boundary |
|---:|---:|---|---|
| 1-497 | [001](./part-001-lines-00001-00250.md)-[002](./part-002-lines-00251-00500.md) | 260813 handoff, reconnect V2, shared WPF journal interlock | History + PC/source diagnosis |
| 498-4564 | [002](./part-002-lines-00251-00500.md)-[019](./part-019-lines-04501-04750.md) | recovery quarantine/retirement and Close/X recovery | PC tests + limited live UI; Servo remained gated |
| 4565-9205 | [019](./part-019-lines-04501-04750.md)-[037](./part-037-lines-09001-09250.md) | collapsible recovery UI, TW19 default and terminal observation | SDO terminal observed; physical multi-turn effect open |
| 9206-13887 | [037](./part-037-lines-09001-09250.md)-[056](./part-056-lines-13751-14000.md) | SetPosition query/retire and retained-store design | PC/static; activation OFF |
| 13888-25510 | [056](./part-056-lines-13751-14000.md)-[103](./part-103-lines-25501-25750.md) | LASAL Store/CheckSum/Network/method creation | IDE/C78 only; no PLC download |
| 25511-34539 | [103](./part-103-lines-25501-25750.md)-[139](./part-139-lines-34501-34750.md) | retained Store lifecycle and Control route implementation | PC/static/C78; dormant |
| 34540-35622 | [139](./part-139-lines-34501-34750.md)-[143](./part-143-lines-35501-35750.md) | Gate D rebaseline and method-size blocker | Artifact/static only |
| 35623-46825 | [143](./part-143-lines-35501-35750.md)-[188](./part-188-lines-46751-47000.md) | handler/dispatcher split, coordinate and ownership dormant slice | Source/static/C78; no runtime |
| 46826-52551 | [188](./part-188-lines-46751-47000.md)-[211](./part-211-lines-52501-52750.md) | RT task audit and native-zero preflight declarations | Design/IDE declarations |
| 52552-53754 | [211](./part-211-lines-52501-52750.md)-[216](./part-216-lines-53751-53925.md) | P0 preflight implementation, verification, docs and commits | PC/static/C78; activation OFF |
| 53755-53925 | [216](./part-216-lines-53751-53925.md) | scoped cache/test/history cleanup | Local filesystem + commits |

## Chunks

| Part | Source lines | File | Bytes | Omitted lines | Topic hint |
|---:|---:|---|---:|---:|---|
| 1 | 1-250 | [part-001-lines-00001-00250.md](./part-001-lines-00001-00250.md) | 11522 | 0 | callback, reconnect, journal, LASAL, WPF, history |
| 2 | 251-500 | [part-002-lines-00251-00500.md](./part-002-lines-00251-00500.md) | 12429 | 0 | journal, Servo On, LASAL, WPF |
| 3 | 501-750 | [part-003-lines-00501-00750.md](./part-003-lines-00501-00750.md) | 10421 | 0 | journal, Servo On, LASAL, WPF |
| 4 | 751-1000 | [part-004-lines-00751-01000.md](./part-004-lines-00751-01000.md) | 15107 | 0 | callback, LASAL, WPF |
| 5 | 1001-1250 | [part-005-lines-01001-01250.md](./part-005-lines-01001-01250.md) | 15453 | 0 | callback, LASAL |
| 6 | 1251-1500 | [part-006-lines-01251-01500.md](./part-006-lines-01251-01500.md) | 15642 | 0 | callback, LASAL |
| 7 | 1501-1750 | [part-007-lines-01501-01750.md](./part-007-lines-01501-01750.md) | 19526 | 0 | callback, EventMask, reconnect, LASAL |
| 8 | 1751-2000 | [part-008-lines-01751-02000.md](./part-008-lines-01751-02000.md) | 16932 | 0 | callback, EventMask, reconnect, LASAL, WPF |
| 9 | 2001-2250 | [part-009-lines-02001-02250.md](./part-009-lines-02001-02250.md) | 18116 | 0 | callback, EventMask, reconnect, LASAL |
| 10 | 2251-2500 | [part-010-lines-02251-02500.md](./part-010-lines-02251-02500.md) | 15105 | 0 | callback, LASAL, WPF |
| 11 | 2501-2750 | [part-011-lines-02501-02750.md](./part-011-lines-02501-02750.md) | 13482 | 0 | callback, LASAL |
| 12 | 2751-3000 | [part-012-lines-02751-03000.md](./part-012-lines-02751-03000.md) | 16277 | 1 | callback, LASAL |
| 13 | 3001-3250 | [part-013-lines-03001-03250.md](./part-013-lines-03001-03250.md) | 14526 | 0 | callback, EventMask, LASAL, WPF |
| 14 | 3251-3500 | [part-014-lines-03251-03500.md](./part-014-lines-03251-03500.md) | 13224 | 0 | callback, reconnect, journal, Servo On, LASAL, WPF |
| 15 | 3501-3750 | [part-015-lines-03501-03750.md](./part-015-lines-03501-03750.md) | 16014 | 0 | callback, LASAL, WPF |
| 16 | 3751-4000 | [part-016-lines-03751-04000.md](./part-016-lines-03751-04000.md) | 15875 | 0 | callback, LASAL |
| 17 | 4001-4250 | [part-017-lines-04001-04250.md](./part-017-lines-04001-04250.md) | 17887 | 0 | callback, EventMask, reconnect, LASAL |
| 18 | 4251-4500 | [part-018-lines-04251-04500.md](./part-018-lines-04251-04500.md) | 18741 | 0 | callback, Servo On, LASAL, WPF |
| 19 | 4501-4750 | [part-019-lines-04501-04750.md](./part-019-lines-04501-04750.md) | 12199 | 0 | journal, Servo On, LASAL, WPF |
| 20 | 4751-5000 | [part-020-lines-04751-05000.md](./part-020-lines-04751-05000.md) | 8381 | 0 | LASAL, WPF |
| 21 | 5001-5250 | [part-021-lines-05001-05250.md](./part-021-lines-05001-05250.md) | 13795 | 0 | callback, LASAL, WPF |
| 22 | 5251-5500 | [part-022-lines-05251-05500.md](./part-022-lines-05251-05500.md) | 17160 | 0 | callback, LASAL |
| 23 | 5501-5750 | [part-023-lines-05501-05750.md](./part-023-lines-05501-05750.md) | 17462 | 0 | callback, reconnect, LASAL |
| 24 | 5751-6000 | [part-024-lines-05751-06000.md](./part-024-lines-05751-06000.md) | 16848 | 0 | callback, reconnect, LASAL |
| 25 | 6001-6250 | [part-025-lines-06001-06250.md](./part-025-lines-06001-06250.md) | 15526 | 0 | callback, reconnect, LASAL |
| 26 | 6251-6500 | [part-026-lines-06251-06500.md](./part-026-lines-06251-06500.md) | 15305 | 0 | callback, LASAL, WPF |
| 27 | 6501-6750 | [part-027-lines-06501-06750.md](./part-027-lines-06501-06750.md) | 10506 | 0 | callback, reconnect |
| 28 | 6751-7000 | [part-028-lines-06751-07000.md](./part-028-lines-06751-07000.md) | 13190 | 0 | LASAL, WPF |
| 29 | 7001-7250 | [part-029-lines-07001-07250.md](./part-029-lines-07001-07250.md) | 18099 | 0 | callback, LASAL |
| 30 | 7251-7500 | [part-030-lines-07251-07500.md](./part-030-lines-07251-07500.md) | 13462 | 0 | journal, LASAL, WPF |
| 31 | 7501-7750 | [part-031-lines-07501-07750.md](./part-031-lines-07501-07750.md) | 13208 | 0 | journal, LASAL, WPF |
| 32 | 7751-8000 | [part-032-lines-07751-08000.md](./part-032-lines-07751-08000.md) | 8108 | 0 | LASAL |
| 33 | 8001-8250 | [part-033-lines-08001-08250.md](./part-033-lines-08001-08250.md) | 15689 | 0 | callback, LASAL, WPF |
| 34 | 8251-8500 | [part-034-lines-08251-08500.md](./part-034-lines-08251-08500.md) | 23120 | 0 | callback, journal, LASAL, cleanup |
| 35 | 8501-8750 | [part-035-lines-08501-08750.md](./part-035-lines-08501-08750.md) | 22728 | 0 | callback, journal, LASAL, cleanup |
| 36 | 8751-9000 | [part-036-lines-08751-09000.md](./part-036-lines-08751-09000.md) | 22958 | 0 | callback, journal, LASAL, WPF, cleanup |
| 37 | 9001-9250 | [part-037-lines-09001-09250.md](./part-037-lines-09001-09250.md) | 19747 | 0 | SetPosition, journal, SRAMRETAIN, LASAL, WPF |
| 38 | 9251-9500 | [part-038-lines-09251-09500.md](./part-038-lines-09251-09500.md) | 18317 | 0 | SetPosition, callback, Gate D, SRAMRETAIN, SourceOnly, LASAL |
| 39 | 9501-9750 | [part-039-lines-09501-09750.md](./part-039-lines-09501-09750.md) | 10736 | 0 | LASAL |
| 40 | 9751-10000 | [part-040-lines-09751-10000.md](./part-040-lines-09751-10000.md) | 10691 | 0 | LASAL |
| 41 | 10001-10250 | [part-041-lines-10001-10250.md](./part-041-lines-10001-10250.md) | 11131 | 0 | LASAL |
| 42 | 10251-10500 | [part-042-lines-10251-10500.md](./part-042-lines-10251-10500.md) | 11127 | 0 | LASAL |
| 43 | 10501-10750 | [part-043-lines-10501-10750.md](./part-043-lines-10501-10750.md) | 10862 | 0 | LASAL, cleanup |
| 44 | 10751-11000 | [part-044-lines-10751-11000.md](./part-044-lines-10751-11000.md) | 11350 | 0 | LASAL, cleanup |
| 45 | 11001-11250 | [part-045-lines-11001-11250.md](./part-045-lines-11001-11250.md) | 9879 | 0 | LASAL, cleanup |
| 46 | 11251-11500 | [part-046-lines-11251-11500.md](./part-046-lines-11251-11500.md) | 11968 | 0 | SRAMRETAIN, LASAL |
| 47 | 11501-11750 | [part-047-lines-11501-11750.md](./part-047-lines-11501-11750.md) | 11003 | 0 | LASAL, cleanup |
| 48 | 11751-12000 | [part-048-lines-11751-12000.md](./part-048-lines-11751-12000.md) | 11549 | 0 | LMCEcatInputLatch, callback, LASAL |
| 49 | 12001-12250 | [part-049-lines-12001-12250.md](./part-049-lines-12001-12250.md) | 12153 | 1 | LMCEcatInputLatch, callback, LASAL, cleanup |
| 50 | 12251-12500 | [part-050-lines-12251-12500.md](./part-050-lines-12251-12500.md) | 7655 | 1 | LASAL, WPF |
| 51 | 12501-12750 | [part-051-lines-12501-12750.md](./part-051-lines-12501-12750.md) | 7926 | 0 | LASAL, WPF |
| 52 | 12751-13000 | [part-052-lines-12751-13000.md](./part-052-lines-12751-13000.md) | 11248 | 3 | SRAMRETAIN, LASAL, WPF |
| 53 | 13001-13250 | [part-053-lines-13001-13250.md](./part-053-lines-13001-13250.md) | 11673 | 1 | LASAL |
| 54 | 13251-13500 | [part-054-lines-13251-13500.md](./part-054-lines-13251-13500.md) | 8161 | 1 | LMCEcatInputLatch, callback, LASAL |
| 55 | 13501-13750 | [part-055-lines-13501-13750.md](./part-055-lines-13501-13750.md) | 10261 | 1 | LASAL |
| 56 | 13751-14000 | [part-056-lines-13751-14000.md](./part-056-lines-13751-14000.md) | 17255 | 0 | SetPosition, callback, Gate D, journal, SRAMRETAIN, SourceOnly |
| 57 | 14001-14250 | [part-057-lines-14001-14250.md](./part-057-lines-14001-14250.md) | 8131 | 0 | LASAL, WPF |
| 58 | 14251-14500 | [part-058-lines-14251-14500.md](./part-058-lines-14251-14500.md) | 9276 | 0 | LASAL |
| 59 | 14501-14750 | [part-059-lines-14501-14750.md](./part-059-lines-14501-14750.md) | 9807 | 0 | LASAL |
| 60 | 14751-15000 | [part-060-lines-14751-15000.md](./part-060-lines-14751-15000.md) | 10921 | 0 | SetPosition, LASAL |
| 61 | 15001-15250 | [part-061-lines-15001-15250.md](./part-061-lines-15001-15250.md) | 10233 | 1 | LASAL |
| 62 | 15251-15500 | [part-062-lines-15251-15500.md](./part-062-lines-15251-15500.md) | 12294 | 3 | LASAL, cleanup |
| 63 | 15501-15750 | [part-063-lines-15501-15750.md](./part-063-lines-15501-15750.md) | 12563 | 2 | LMCEcatInputLatch, callback, LASAL |
| 64 | 15751-16000 | [part-064-lines-15751-16000.md](./part-064-lines-15751-16000.md) | 13914 | 1 | SetPosition, LASAL, cleanup |
| 65 | 16001-16250 | [part-065-lines-16001-16250.md](./part-065-lines-16001-16250.md) | 11618 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 66 | 16251-16500 | [part-066-lines-16251-16500.md](./part-066-lines-16251-16500.md) | 11933 | 2 | LMCEcatInputLatch, callback, LASAL |
| 67 | 16501-16750 | [part-067-lines-16501-16750.md](./part-067-lines-16501-16750.md) | 13637 | 0 | LMCEcatInputLatch, callback, LASAL |
| 68 | 16751-17000 | [part-068-lines-16751-17000.md](./part-068-lines-16751-17000.md) | 10937 | 3 | SetPosition, LMCSetPositionStore, LASAL |
| 69 | 17001-17250 | [part-069-lines-17001-17250.md](./part-069-lines-17001-17250.md) | 13592 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 70 | 17251-17500 | [part-070-lines-17251-17500.md](./part-070-lines-17251-17500.md) | 11273 | 2 | SetPosition, C78, LASAL |
| 71 | 17501-17750 | [part-071-lines-17501-17750.md](./part-071-lines-17501-17750.md) | 11801 | 2 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 72 | 17751-18000 | [part-072-lines-17751-18000.md](./part-072-lines-17751-18000.md) | 11335 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 73 | 18001-18250 | [part-073-lines-18001-18250.md](./part-073-lines-18001-18250.md) | 10141 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 74 | 18251-18500 | [part-074-lines-18251-18500.md](./part-074-lines-18251-18500.md) | 11121 | 0 | SetPosition, LMCSetPositionStore, LASAL |
| 75 | 18501-18750 | [part-075-lines-18501-18750.md](./part-075-lines-18501-18750.md) | 8605 | 0 | LMCEcatInputLatch, LASAL |
| 76 | 18751-19000 | [part-076-lines-18751-19000.md](./part-076-lines-18751-19000.md) | 10624 | 0 | SetPosition, LMCSetPositionStore, LASAL |
| 77 | 19001-19250 | [part-077-lines-19001-19250.md](./part-077-lines-19001-19250.md) | 6296 | 4 | SetPosition, LASAL |
| 78 | 19251-19500 | [part-078-lines-19251-19500.md](./part-078-lines-19251-19500.md) | 6515 | 7 | LASAL |
| 79 | 19501-19750 | [part-079-lines-19501-19750.md](./part-079-lines-19501-19750.md) | 9792 | 8 | SetPosition, LMCSetPositionStore, LASAL |
| 80 | 19751-20000 | [part-080-lines-19751-20000.md](./part-080-lines-19751-20000.md) | 10524 | 9 | SetPosition, SRAMRETAIN, LASAL |
| 81 | 20001-20250 | [part-081-lines-20001-20250.md](./part-081-lines-20001-20250.md) | 10994 | 7 | LASAL |
| 82 | 20251-20500 | [part-082-lines-20251-20500.md](./part-082-lines-20251-20500.md) | 6425 | 0 | SetPosition, LASAL |
| 83 | 20501-20750 | [part-083-lines-20501-20750.md](./part-083-lines-20501-20750.md) | 13293 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 84 | 20751-21000 | [part-084-lines-20751-21000.md](./part-084-lines-20751-21000.md) | 8169 | 0 | LASAL, cleanup |
| 85 | 21001-21250 | [part-085-lines-21001-21250.md](./part-085-lines-21001-21250.md) | 4692 | 0 | SetPosition, LMCSetPositionStore, LASAL |
| 86 | 21251-21500 | [part-086-lines-21251-21500.md](./part-086-lines-21251-21500.md) | 6135 | 0 | SetPosition, LMCSetPositionStore, LASAL |
| 87 | 21501-21750 | [part-087-lines-21501-21750.md](./part-087-lines-21501-21750.md) | 6759 | 0 | SetPosition, LMCSetPositionStore, LASAL |
| 88 | 21751-22000 | [part-088-lines-21751-22000.md](./part-088-lines-21751-22000.md) | 5630 | 1 | SetPosition, LMCSetPositionStore, LASAL |
| 89 | 22001-22250 | [part-089-lines-22001-22250.md](./part-089-lines-22001-22250.md) | 5923 | 2 | SetPosition, LMCSetPositionStore, LASAL |
| 90 | 22251-22500 | [part-090-lines-22251-22500.md](./part-090-lines-22251-22500.md) | 7589 | 6 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 91 | 22501-22750 | [part-091-lines-22501-22750.md](./part-091-lines-22501-22750.md) | 6771 | 4 | SetPosition, LMCSetPositionStore, LASAL |
| 92 | 22751-23000 | [part-092-lines-22751-23000.md](./part-092-lines-22751-23000.md) | 4692 | 0 | SetPosition, LMCSetPositionStore, LASAL |
| 93 | 23001-23250 | [part-093-lines-23001-23250.md](./part-093-lines-23001-23250.md) | 4666 | 0 | SetPosition, LMCSetPositionStore, LASAL |
| 94 | 23251-23500 | [part-094-lines-23251-23500.md](./part-094-lines-23251-23500.md) | 4123 | 0 | LASAL |
| 95 | 23501-23750 | [part-095-lines-23501-23750.md](./part-095-lines-23501-23750.md) | 4116 | 0 | LASAL |
| 96 | 23751-24000 | [part-096-lines-23751-24000.md](./part-096-lines-23751-24000.md) | 4015 | 0 | SetPosition, LASAL |
| 97 | 24001-24250 | [part-097-lines-24001-24250.md](./part-097-lines-24001-24250.md) | 5823 | 2 | SetPosition, LMCSetPositionStore, LASAL |
| 98 | 24251-24500 | [part-098-lines-24251-24500.md](./part-098-lines-24251-24500.md) | 5848 | 0 | SetPosition, LMCSetPositionStore, LASAL |
| 99 | 24501-24750 | [part-099-lines-24501-24750.md](./part-099-lines-24501-24750.md) | 10143 | 2 | SetPosition, LMCSetPositionStore, SRAMRETAIN, LASAL |
| 100 | 24751-25000 | [part-100-lines-24751-25000.md](./part-100-lines-24751-25000.md) | 15897 | 0 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 101 | 25001-25250 | [part-101-lines-25001-25250.md](./part-101-lines-25001-25250.md) | 10091 | 0 | SetPosition, LMCSetPositionStore, LASAL |
| 102 | 25251-25500 | [part-102-lines-25251-25500.md](./part-102-lines-25251-25500.md) | 12608 | 0 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 103 | 25501-25750 | [part-103-lines-25501-25750.md](./part-103-lines-25501-25750.md) | 14433 | 0 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 104 | 25751-26000 | [part-104-lines-25751-26000.md](./part-104-lines-25751-26000.md) | 10214 | 0 | SetPosition, LMCSetPositionStore, C78, LASAL, WPF |
| 105 | 26001-26250 | [part-105-lines-26001-26250.md](./part-105-lines-26001-26250.md) | 8602 | 1 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 106 | 26251-26500 | [part-106-lines-26251-26500.md](./part-106-lines-26251-26500.md) | 13731 | 4 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 107 | 26501-26750 | [part-107-lines-26501-26750.md](./part-107-lines-26501-26750.md) | 17869 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 108 | 26751-27000 | [part-108-lines-26751-27000.md](./part-108-lines-26751-27000.md) | 17484 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 109 | 27001-27250 | [part-109-lines-27001-27250.md](./part-109-lines-27001-27250.md) | 16934 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 110 | 27251-27500 | [part-110-lines-27251-27500.md](./part-110-lines-27251-27500.md) | 16833 | 1 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 111 | 27501-27750 | [part-111-lines-27501-27750.md](./part-111-lines-27501-27750.md) | 9979 | 3 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 112 | 27751-28000 | [part-112-lines-27751-28000.md](./part-112-lines-27751-28000.md) | 9763 | 5 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 113 | 28001-28250 | [part-113-lines-28001-28250.md](./part-113-lines-28001-28250.md) | 6068 | 7 | SetPosition, LMCSetPositionStore, LASAL |
| 114 | 28251-28500 | [part-114-lines-28251-28500.md](./part-114-lines-28251-28500.md) | 7366 | 4 | C78, LASAL, WPF |
| 115 | 28501-28750 | [part-115-lines-28501-28750.md](./part-115-lines-28501-28750.md) | 6977 | 2 | SetPosition, LMCSetPositionStore, LASAL |
| 116 | 28751-29000 | [part-116-lines-28751-29000.md](./part-116-lines-28751-29000.md) | 8089 | 1 | SetPosition, C78, LASAL |
| 117 | 29001-29250 | [part-117-lines-29001-29250.md](./part-117-lines-29001-29250.md) | 8558 | 0 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 118 | 29251-29500 | [part-118-lines-29251-29500.md](./part-118-lines-29251-29500.md) | 6640 | 1 | SetPosition, LMCSetPositionStore, LASAL, WPF |
| 119 | 29501-29750 | [part-119-lines-29501-29750.md](./part-119-lines-29501-29750.md) | 8615 | 0 | SetPosition, LMCSetPositionStore, C78, LASAL, cleanup |
| 120 | 29751-30000 | [part-120-lines-29751-30000.md](./part-120-lines-29751-30000.md) | 8005 | 0 | SetPosition, LMCSetPositionStore, C78, LASAL, WPF, cleanup |
| 121 | 30001-30250 | [part-121-lines-30001-30250.md](./part-121-lines-30001-30250.md) | 6025 | 3 | SetPosition, LMCSetPositionStore, LASAL, WPF |
| 122 | 30251-30500 | [part-122-lines-30251-30500.md](./part-122-lines-30251-30500.md) | 6637 | 0 | C78, LASAL |
| 123 | 30501-30750 | [part-123-lines-30501-30750.md](./part-123-lines-30501-30750.md) | 5068 | 0 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 124 | 30751-31000 | [part-124-lines-30751-31000.md](./part-124-lines-30751-31000.md) | 8186 | 0 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 125 | 31001-31250 | [part-125-lines-31001-31250.md](./part-125-lines-31001-31250.md) | 8491 | 0 | SetPosition, LMCSetPositionStore, LASAL, WPF |
| 126 | 31251-31500 | [part-126-lines-31251-31500.md](./part-126-lines-31251-31500.md) | 14461 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 127 | 31501-31750 | [part-127-lines-31501-31750.md](./part-127-lines-31501-31750.md) | 14034 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 128 | 31751-32000 | [part-128-lines-31751-32000.md](./part-128-lines-31751-32000.md) | 14748 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 129 | 32001-32250 | [part-129-lines-32001-32250.md](./part-129-lines-32001-32250.md) | 14438 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL, WPF |
| 130 | 32251-32500 | [part-130-lines-32251-32500.md](./part-130-lines-32251-32500.md) | 9686 | 0 | C78, LASAL |
| 131 | 32501-32750 | [part-131-lines-32501-32750.md](./part-131-lines-32501-32750.md) | 9563 | 0 | LASAL, WPF |
| 132 | 32751-33000 | [part-132-lines-32751-33000.md](./part-132-lines-32751-33000.md) | 10224 | 0 | LASAL |
| 133 | 33001-33250 | [part-133-lines-33001-33250.md](./part-133-lines-33001-33250.md) | 14520 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, LASAL |
| 134 | 33251-33500 | [part-134-lines-33251-33500.md](./part-134-lines-33251-33500.md) | 19074 | 1 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 135 | 33501-33750 | [part-135-lines-33501-33750.md](./part-135-lines-33501-33750.md) | 15558 | 3 | SetPosition, LMCSetPositionStore, C78, LASAL |
| 136 | 33751-34000 | [part-136-lines-33751-34000.md](./part-136-lines-33751-34000.md) | 15476 | 1 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, C78, LASAL |
| 137 | 34001-34250 | [part-137-lines-34001-34250.md](./part-137-lines-34001-34250.md) | 15447 | 1 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, C78, LASAL |
| 138 | 34251-34500 | [part-138-lines-34251-34500.md](./part-138-lines-34251-34500.md) | 15250 | 1 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, C78, LASAL |
| 139 | 34501-34750 | [part-139-lines-34501-34750.md](./part-139-lines-34501-34750.md) | 18363 | 1 | SetPosition, LMCSetPositionStore, C78, SRAMRETAIN, SourceOnly, LASAL |
| 140 | 34751-35000 | [part-140-lines-34751-35000.md](./part-140-lines-34751-35000.md) | 8830 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, SourceOnly, LASAL |
| 141 | 35001-35250 | [part-141-lines-35001-35250.md](./part-141-lines-35001-35250.md) | 16985 | 0 | SetPosition, LMCSetPositionStore, callback, Gate D, C78, SRAMRETAIN |
| 142 | 35251-35500 | [part-142-lines-35251-35500.md](./part-142-lines-35251-35500.md) | 8052 | 0 | LASAL |
| 143 | 35501-35750 | [part-143-lines-35501-35750.md](./part-143-lines-35501-35750.md) | 10183 | 0 | SetPosition, Gate D, SourceOnly, LASAL |
| 144 | 35751-36000 | [part-144-lines-35751-36000.md](./part-144-lines-35751-36000.md) | 9069 | 0 | LASAL |
| 145 | 36001-36250 | [part-145-lines-36001-36250.md](./part-145-lines-36001-36250.md) | 20417 | 3 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 146 | 36251-36500 | [part-146-lines-36251-36500.md](./part-146-lines-36251-36500.md) | 15091 | 2 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 147 | 36501-36750 | [part-147-lines-36501-36750.md](./part-147-lines-36501-36750.md) | 9638 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 148 | 36751-37000 | [part-148-lines-36751-37000.md](./part-148-lines-36751-37000.md) | 8027 | 0 | LASAL, WPF |
| 149 | 37001-37250 | [part-149-lines-37001-37250.md](./part-149-lines-37001-37250.md) | 8296 | 0 | LASAL |
| 150 | 37251-37500 | [part-150-lines-37251-37500.md](./part-150-lines-37251-37500.md) | 15727 | 6 | SetPosition, LMCSetPositionStore, LASAL |
| 151 | 37501-37750 | [part-151-lines-37501-37750.md](./part-151-lines-37501-37750.md) | 9812 | 7 | SetPosition, LMCSetPositionStore, LASAL |
| 152 | 37751-38000 | [part-152-lines-37751-38000.md](./part-152-lines-37751-38000.md) | 20389 | 7 | SetPosition, LMCSetPositionStore, LASAL |
| 153 | 38001-38250 | [part-153-lines-38001-38250.md](./part-153-lines-38001-38250.md) | 17983 | 9 | SetPosition, LMCSetPositionStore, LASAL |
| 154 | 38251-38500 | [part-154-lines-38251-38500.md](./part-154-lines-38251-38500.md) | 20185 | 6 | SetPosition, LMCSetPositionStore, LASAL |
| 155 | 38501-38750 | [part-155-lines-38501-38750.md](./part-155-lines-38501-38750.md) | 20104 | 6 | SetPosition, LMCSetPositionStore, LASAL |
| 156 | 38751-39000 | [part-156-lines-38751-39000.md](./part-156-lines-38751-39000.md) | 10950 | 8 | LASAL |
| 157 | 39001-39250 | [part-157-lines-39001-39250.md](./part-157-lines-39001-39250.md) | 13739 | 11 | C78, LASAL |
| 158 | 39251-39500 | [part-158-lines-39251-39500.md](./part-158-lines-39251-39500.md) | 8971 | 9 | LASAL |
| 159 | 39501-39750 | [part-159-lines-39501-39750.md](./part-159-lines-39501-39750.md) | 13776 | 5 | SetPosition, LMCSetPositionStore, LASAL |
| 160 | 39751-40000 | [part-160-lines-39751-40000.md](./part-160-lines-39751-40000.md) | 10586 | 4 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 161 | 40001-40250 | [part-161-lines-40001-40250.md](./part-161-lines-40001-40250.md) | 14105 | 5 | SetPosition, LMCSetPositionStore, LASAL |
| 162 | 40251-40500 | [part-162-lines-40251-40500.md](./part-162-lines-40251-40500.md) | 20055 | 6 | SetPosition, LMCSetPositionStore, LASAL |
| 163 | 40501-40750 | [part-163-lines-40501-40750.md](./part-163-lines-40501-40750.md) | 11324 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 164 | 40751-41000 | [part-164-lines-40751-41000.md](./part-164-lines-40751-41000.md) | 20550 | 6 | SetPosition, LMCSetPositionStore, LASAL |
| 165 | 41001-41250 | [part-165-lines-41001-41250.md](./part-165-lines-41001-41250.md) | 14095 | 4 | SetPosition, LMCSetPositionStore, LASAL, WPF |
| 166 | 41251-41500 | [part-166-lines-41251-41500.md](./part-166-lines-41251-41500.md) | 11131 | 4 | SetPosition, Gate D, C78, LASAL |
| 167 | 41501-41750 | [part-167-lines-41501-41750.md](./part-167-lines-41501-41750.md) | 7736 | 6 | SetPosition, C78, SourceOnly, LASAL |
| 168 | 41751-42000 | [part-168-lines-41751-42000.md](./part-168-lines-41751-42000.md) | 6376 | 0 | LASAL, WPF |
| 169 | 42001-42250 | [part-169-lines-42001-42250.md](./part-169-lines-42001-42250.md) | 5330 | 1 | LASAL |
| 170 | 42251-42500 | [part-170-lines-42251-42500.md](./part-170-lines-42251-42500.md) | 5134 | 0 | LASAL |
| 171 | 42501-42750 | [part-171-lines-42501-42750.md](./part-171-lines-42501-42750.md) | 7051 | 0 | LASAL, WPF |
| 172 | 42751-43000 | [part-172-lines-42751-43000.md](./part-172-lines-42751-43000.md) | 5184 | 0 | LASAL |
| 173 | 43001-43250 | [part-173-lines-43001-43250.md](./part-173-lines-43001-43250.md) | 9669 | 0 | LASAL |
| 174 | 43251-43500 | [part-174-lines-43251-43500.md](./part-174-lines-43251-43500.md) | 8519 | 0 | LASAL |
| 175 | 43501-43750 | [part-175-lines-43501-43750.md](./part-175-lines-43501-43750.md) | 8766 | 0 | C78, LASAL, WPF |
| 176 | 43751-44000 | [part-176-lines-43751-44000.md](./part-176-lines-43751-44000.md) | 8657 | 2 | C78, LASAL |
| 177 | 44001-44250 | [part-177-lines-44001-44250.md](./part-177-lines-44001-44250.md) | 9265 | 0 | SetPosition, C78, SourceOnly, LASAL, WPF |
| 178 | 44251-44500 | [part-178-lines-44251-44500.md](./part-178-lines-44251-44500.md) | 7296 | 0 | LASAL |
| 179 | 44501-44750 | [part-179-lines-44501-44750.md](./part-179-lines-44501-44750.md) | 10517 | 7 | LASAL, WPF |
| 180 | 44751-45000 | [part-180-lines-44751-45000.md](./part-180-lines-44751-45000.md) | 7939 | 8 | C78, LASAL, WPF |
| 181 | 45001-45250 | [part-181-lines-45001-45250.md](./part-181-lines-45001-45250.md) | 9955 | 7 | C78, LASAL |
| 182 | 45251-45500 | [part-182-lines-45251-45500.md](./part-182-lines-45251-45500.md) | 8371 | 10 | SetPosition, C78, LASAL |
| 183 | 45501-45750 | [part-183-lines-45501-45750.md](./part-183-lines-45501-45750.md) | 13441 | 1 | SetPosition, C78, SourceOnly, LASAL, WPF |
| 184 | 45751-46000 | [part-184-lines-45751-46000.md](./part-184-lines-45751-46000.md) | 6802 | 6 | C78, SourceOnly, LASAL |
| 185 | 46001-46250 | [part-185-lines-46001-46250.md](./part-185-lines-46001-46250.md) | 7712 | 10 | LASAL |
| 186 | 46251-46500 | [part-186-lines-46251-46500.md](./part-186-lines-46251-46500.md) | 10239 | 8 | C78, SourceOnly, LASAL |
| 187 | 46501-46750 | [part-187-lines-46501-46750.md](./part-187-lines-46501-46750.md) | 7813 | 10 | SetPosition, LASAL, WPF |
| 188 | 46751-47000 | [part-188-lines-46751-47000.md](./part-188-lines-46751-47000.md) | 15299 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, journal, C78, SRAMRETAIN |
| 189 | 47001-47250 | [part-189-lines-47001-47250.md](./part-189-lines-47001-47250.md) | 8408 | 0 | LASAL |
| 190 | 47251-47500 | [part-190-lines-47251-47500.md](./part-190-lines-47251-47500.md) | 10158 | 1 | LASAL |
| 191 | 47501-47750 | [part-191-lines-47501-47750.md](./part-191-lines-47501-47750.md) | 14609 | 3 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 192 | 47751-48000 | [part-192-lines-47751-48000.md](./part-192-lines-47751-48000.md) | 12354 | 2 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 193 | 48001-48250 | [part-193-lines-48001-48250.md](./part-193-lines-48001-48250.md) | 11507 | 8 | SetPosition, LMCEcatInputLatch, LASAL |
| 194 | 48251-48500 | [part-194-lines-48251-48500.md](./part-194-lines-48251-48500.md) | 8823 | 3 | SetPosition, LASAL |
| 195 | 48501-48750 | [part-195-lines-48501-48750.md](./part-195-lines-48501-48750.md) | 7702 | 7 | SetPosition, LMCEcatInputLatch, LASAL |
| 196 | 48751-49000 | [part-196-lines-48751-49000.md](./part-196-lines-48751-49000.md) | 6899 | 5 | SetPosition, LMCEcatInputLatch, LASAL |
| 197 | 49001-49250 | [part-197-lines-49001-49250.md](./part-197-lines-49001-49250.md) | 8125 | 7 | LASAL |
| 198 | 49251-49500 | [part-198-lines-49251-49500.md](./part-198-lines-49251-49500.md) | 8545 | 8 | SetPosition, LASAL |
| 199 | 49501-49750 | [part-199-lines-49501-49750.md](./part-199-lines-49501-49750.md) | 9622 | 8 | LMCEcatInputLatch, LASAL |
| 200 | 49751-50000 | [part-200-lines-49751-50000.md](./part-200-lines-49751-50000.md) | 18774 | 5 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, LASAL |
| 201 | 50001-50250 | [part-201-lines-50001-50250.md](./part-201-lines-50001-50250.md) | 9180 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 202 | 50251-50500 | [part-202-lines-50251-50500.md](./part-202-lines-50251-50500.md) | 7867 | 1 | SetPosition, LMCEcatInputLatch, LASAL |
| 203 | 50501-50750 | [part-203-lines-50501-50750.md](./part-203-lines-50501-50750.md) | 17418 | 0 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, LASAL |
| 204 | 50751-51000 | [part-204-lines-50751-51000.md](./part-204-lines-50751-51000.md) | 20676 | 1 | SetPosition, LMCSetPositionStore, LMCEcatInputLatch, callback, LASAL |
| 205 | 51001-51250 | [part-205-lines-51001-51250.md](./part-205-lines-51001-51250.md) | 10325 | 0 | SetPosition, LMCSetPositionStore, callback, LASAL |
| 206 | 51251-51500 | [part-206-lines-51251-51500.md](./part-206-lines-51251-51500.md) | 13396 | 6 | SetPosition, callback, LASAL |
| 207 | 51501-51750 | [part-207-lines-51501-51750.md](./part-207-lines-51501-51750.md) | 12386 | 5 | SetPosition, LASAL |
| 208 | 51751-52000 | [part-208-lines-51751-52000.md](./part-208-lines-51751-52000.md) | 13416 | 6 | SetPosition, LASAL |
| 209 | 52001-52250 | [part-209-lines-52001-52250.md](./part-209-lines-52001-52250.md) | 13336 | 13 | SetPosition, LMCSetPositionStore, LASAL |
| 210 | 52251-52500 | [part-210-lines-52251-52500.md](./part-210-lines-52251-52500.md) | 13942 | 15 | SetPosition, LMCEcatInputLatch, LASAL |
| 211 | 52501-52750 | [part-211-lines-52501-52750.md](./part-211-lines-52501-52750.md) | 11358 | 3 | SetPosition, journal, C78, SourceOnly, LASAL, WPF |
| 212 | 52751-53000 | [part-212-lines-52751-53000.md](./part-212-lines-52751-53000.md) | 9216 | 0 | LASAL |
| 213 | 53001-53250 | [part-213-lines-53001-53250.md](./part-213-lines-53001-53250.md) | 9106 | 0 | LASAL |
| 214 | 53251-53500 | [part-214-lines-53251-53500.md](./part-214-lines-53251-53500.md) | 9103 | 0 | C78, LASAL |
| 215 | 53501-53750 | [part-215-lines-53501-53750.md](./part-215-lines-53501-53750.md) | 11631 | 2 | SetPosition, LMCEcatInputLatch, journal, C78, SRAMRETAIN, Distribution |
| 216 | 53751-53925 | [part-216-lines-53751-53925.md](./part-216-lines-53751-53925.md) | 11230 | 0 | SetPosition, LMCSetPositionStore, SRAMRETAIN, Distribution, LASAL, history |

## Resume artifact

- Read [99_analysis_summary.md](./99_analysis_summary.md) after the chunk analysis is complete.

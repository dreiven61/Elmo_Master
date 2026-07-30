# EtherCAT Test2 capture audit

- Capture date: 2026-07-28
- Audit date: 2026-07-29
- Scope: `test/packet_capture/SIGMATEK_API_Analyze/Test2`

## 결론

최신 Test2 wire에는 CREVIS 구성 정보가 이미 있다. `0x7E11/0x7E12`는
revision `0x15867EEC`의 7-entry topology를 반환하며, 그중 CREVIS 계열은 coupler
1개와 slot module 2개다.

반면 CREVIS의 동적 node health와 digital I/O는 아직 wire에 없다. capability bits
15~17은 0이고 `0x7E13`, `0x7E22`, `0x7E23` 요청도 없다. 기존
`Read EtherCAT Health` capture의 실제 command는 `0x7E10`이며 Elmo 4축만 반환한다.

따라서 아래 두 결과를 구분해야 한다.

- configured topology 표시: 현재 capture로 확인됨
- EtherCAT 상태에 따른 CREVIS health/I/O 동적 변경: 미구현이며 확인되지 않음

## Raw command sequence

| capture | TCP request sequence |
|---|---|
| `Connect.pcapng` | `0x405D -> 0x8080 -> 0x405C -> 0x7E00(id 1) -> 0x7E00(id 2) -> 0x7E11(id 3) -> 0x7E12(id 4..10, start 0..6)` |
| `Topology.pcapng` | `0x7E00(id 19) -> 0x7E00(id 20) -> 0x7E11(id 21) -> 0x7E12(id 22..28, start 0..6)` |
| `Read EtherCAT Health.pcapng` | `0x7E00(id 13) -> 0x7E10(id 14)` |

세 capture의 전체 TCP request에서 `0x7E13`, `0x7E22`, `0x7E23`은 0건이다.

## Capability evidence

`0x7E00` 응답의 핵심 값은 다음과 같다.

| field | value |
|---|---:|
| CapabilityBits | `0x0000613F` |
| MapRevision | `0x957F101E` |
| DiagnosticsBootId | `17` |
| EtherCATTopology bit 14 | 1 |
| SDOWrite bit 9 | 0 |
| EtherCATNodeHealth bit 15 | 0 |
| DigitalIORead bit 16 | 0 |
| DigitalIOWrite bit 17 | 0 |

원시 little-endian 근거는 `3f610000 1e107f95`와 응답 끝의
`04000000 11000000`이다.

## Topology evidence

`Topology.pcapng`의 `0x7E11` 응답은 다음 계약을 반환한다.

| field | value |
|---|---:|
| TopologyRevision | `0x15867EEC` |
| TotalNodeCount | 7 |
| EntryStride | 96 bytes |
| MaxEntriesPerChunk | 1 |
| ConfiguredSlaveCount | 5 |
| SlotModuleCount | 2 |
| PhysicalAxisCount | 4 |
| TopologyFlags | `0x0000000F` |
| CRC kind | 1 |

`0x7E12` entry 순서는 다음과 같다.

| index | master index | name | identity |
|---:|---:|---|---|
| 0 | 0 | `GL_9086_11` | coupler, NodeId `0xEC000001` |
| 1 | 1 | `Elmo_11` | axis/SDO reference 1 |
| 2 | 2 | `Elmo_21` | axis/SDO reference 2 |
| 3 | 3 | `Elmo_31` | axis/SDO reference 3 |
| 4 | 4 | `Elmo_41` | axis/SDO reference 4 |
| 5 | none | `GL_9086_1_Slot001` | input 4 bytes, IOReference `0x00010001` |
| 6 | none | `GL_9086_1_Slot011` | output 4 bytes, IOReference `0x00010002` |

`Connect.txt`와 `Topology.txt`도 각각 automatic/manual load에서
`Revision=0x15867EEC, Nodes=7, CREVIS=3`을 기록한다. 즉 최신 GUI build에서는
configured CREVIS row가 실제로 표시됐다.

## Legacy health boundary

`Read EtherCAT Health.pcapng`의 `0x7E10` 응답은 4개 fixed record만 갖는다.
각 record는 `SlaveIndex 0..3`, `PhysicalAxis 1..4`다. 이것은 7-node topology의
동적 health가 아니라 기존 Elmo 4축 health다.

CREVIS coupler와 두 slot의 동적 상태를 표시하려면 LASAL IDE에서 T2 client/method/
network 구조를 생성한 뒤 외부 source로 `0x7E13/0x7E22` owner를 구현해야 한다.
구조 작업 기준은 `LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md`다.

## 검증 경계

이 문서는 저장된 packet bytes와 GUI text log의 offline 분석이다. 현재 controller에
대한 재실행, LASAL IDE build, PLC download, cable fault test 또는 physical digital I/O
검증을 수행한 결과가 아니다.

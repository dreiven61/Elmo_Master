# Generic SDO Write software 구현 결과 — 2026-09-01

## 판정

**Software implementation ready / physical qualification pending**

SWR-01~04의 source 구현은 완료했다. C78 build/link, PLC download, EtherCAT mailbox/packet, 실제
object 값 변경과 failure matrix는 실행하지 않았으므로 hardware PASS 또는 production PASS가 아니다.

## 완료한 구현

- UI24 Axis1 known preset과 generic address admission을 분리했다.
- same-value canary의 네 ticket 결과를 current connection/session/build/BootId/MapRevision/capability에
  귀속된 transport proof로 저장한다. ordinary target은 canary tuple과 같을 필요가 없다.
- ordinary Write 첫 클릭은 exact baseline Read 후 immutable request와 baseline bytes만 arm한다.
- 두 번째 exact 클릭은 safe-axis/fresh capability/proof를 다시 검사하고 exact pre-write Read가
  baseline과 같을 때만 journal을 arm한다.
- journal format v4는 baseline, pre-write guard, expected Write bytes를 함께 저장하며 old record는
  새로운 Write authorization으로 승격하지 않는다.
- SDK mutation gate 안에서 identity를 다시 확인한 뒤 `0x7E50` Write를 정확히 한 번 submit한다.
  실패/불명확/재시작 경로는 Write를 자동 replay하거나 값을 자동 restore하지 않는다.
- PLC `0x7E50` parser는 canonical scalar payload를 exact length로 해석한다.
  - Bool/Int8/UInt8/BitField8: 1 byte
  - Int16/UInt16/BitField16: 2 bytes
  - Int32/UInt32/Real32/BitField32: 4 bytes
- permanent blocklist는 `0x6040`, `0x6060`, `0x607A`, `0x60FF`, `0x6071`, `0x3204`, `0x20FC`다.
- ordinary safe gate는 `Standstill=True`, `DS402 Fault=False`, `OperationEnabled=False`, DS402 base
  `0x40/0x21/0x23`다. PowerOff는 canary qualification의 보수 조건이지 generic runtime 필수조건이 아니다.
- Manual Class View와 programmatic path는 같은 `LMCSdoExecutor` owner gate를 사용한다.

## 현재 검증

- VS2019 MSBuild WPF Debug: PASS, warning 0 / error 0
- VS2019 MSBuild WPF SmokeTests Debug project build: PASS, warning 0 / error 0
- `dotnet build` SDK solution Debug: PASS, warning 0 / error 0
- `Verify-LasalGenericSdoWrite.ps1`: `PASS SWR-01..04 generic scalar SDO Write source and exact-once ordering contract`
- test executable: 사용자의 요청에 따라 미실행
- LASAL C78 build/link 및 PLC download: 미실행
- physical Write/readback/pcap: 미실행

## 사용자 qualification에서 확인할 항목

1. same-value UI24 canary 네 ticket PASS 후 ordinary target의 Write 버튼이 활성화되는지 확인한다.
2. Axis1에서 승인된 non-semantic object를 1/2/4 byte별로 baseline과 같은 값부터 시험한다.
3. 첫 클릭은 baseline/confirmation만 만들고 Write ticket이 없어야 한다.
4. 두 번째 exact 클릭은 pre-write bytes가 같을 때 Write ticket 한 개만 만들어야 한다.
5. terminal `Completed/Success` 뒤 exact readback이 expected bytes와 같아야 journal이 resolve된다.
6. baseline 변경, capability/BootId/MapRevision 변경, blocked object, contention, timeout, disconnect에서는
   hidden second Write와 automatic replay가 0회인지 확인한다.
7. Axis1 결과를 Axis2~4에 복사 판정하지 않고 각 축 evidence를 별도로 남긴다.

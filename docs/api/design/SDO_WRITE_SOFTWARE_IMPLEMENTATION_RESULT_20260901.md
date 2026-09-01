# Generic SDO Write software 구현 결과 — 2026-09-01

## 판정

**Direct-manual software implementation complete / physical qualification pending**

SWR-01~04의 source 구현은 완료했다. C78 build/link, PLC download, EtherCAT mailbox/packet, 실제
object 값 변경과 failure matrix는 실행하지 않았으므로 hardware PASS 또는 production PASS가 아니다.

## 완료한 구현

- UI24 Axis1 known preset과 generic address admission을 분리했다.
- same-value canary의 네 ticket 결과는 optional engineering diagnostic evidence로만 저장한다.
  ordinary manual Write의 admission이나 button enablement에는 사용하지 않는다.
- ordinary Write 첫 클릭은 exact baseline Read 후 immutable request와 baseline bytes만 arm한다.
- 두 번째 exact 클릭은 safe-axis/fresh capability를 다시 검사하고 exact pre-write Read가
  baseline과 같을 때만 journal을 arm한다.
- journal format v4는 baseline, pre-write guard, expected Write bytes를 함께 저장하며 old record는
  새로운 Write authorization으로 승격하지 않는다.
- SDK mutation gate 안에서 identity를 다시 확인한 뒤 `0x7E50` Write를 정확히 한 번 submit한다.
  실패/불명확/재시작 경로는 Write를 자동 replay하거나 값을 자동 restore하지 않는다.
- PLC `0x7E50` parser는 canonical scalar payload를 exact length로 해석한다.
  - Bool/Int8/UInt8/BitField8: 1 byte
  - Int16/UInt16/BitField16: 2 bytes
  - Int32/UInt32/Real32/BitField32: 4 bytes
- Generic SDO Write에는 ObjectIndex denylist가 없다. `0x0000`만 invalid이며 `0x6040`, `0x6060`,
  `0x607A`, `0x60FF`, `0x6071`, `0x3204`, `0x20FC`도 주소만으로 거부하지 않는다.
- ordinary safe gate는 `Standstill=True`, `DS402 Fault=False`, `OperationEnabled=False`, DS402 base
  `0x40/0x21/0x23`다. PowerOff는 canary qualification의 보수 조건이지 generic runtime 필수조건이 아니다.
- Manual Class View와 programmatic path는 같은 `LMCSdoExecutor` owner gate를 사용한다.

## 현재 검증

- VS2019 MSBuild WPF Debug/Release: PASS
- VS2019 MSBuild WPF SmokeTests Debug/Release project build: PASS
- `dotnet build` SDK solution Debug: PASS, warning 0 / error 0
- `Verify-LasalGenericSdoWrite.ps1`: `PASS SWR-01..04 generic scalar SDO Write source and exact-once ordering contract`
- SDK automated regression: 1200/1200 PASS
- WPF SDO focused smoke: 17/17 PASS
- WPF localization smoke: 9/9 PASS
- LASAL C78 build/link 및 PLC download: 미실행
- physical Write/readback/pcap: 미실행

## 2026-09-02 live UI blocker 수정

실행 중인 WPF의 읽기 전용 UI evidence에서 PLC capability 자체는 정상으로 확인됐다.

- `bit8/read=1`
- `bit9/write=1`
- `bit13/general=1`
- durable mutation journal: `PASS`
- mutation interlock: clear
- 실제 SDK policy blocker: `CapabilityObservationNotCurrent`

원인은 `SubmitSdoAsync`/`ReadSdoInlineAsync`가 내부 capability preflight를 수행할 때 SDK의 latest
observation sequence는 증가하지만 WPF가 표시 중인 cached observation은 이전 sequence로 남는 데 있었다.
그 결과 Read 성공 뒤 PLC가 Write를 광고하고 있어도 ordinary Write 버튼만 비활성화됐다.

수정 계약:

- cached blocker가 `CapabilityObservationNotCurrent` 하나뿐이면 ordinary manual Write 버튼은 활성화한다.
- 클릭 handler는 baseline, guard, Write 전에 기존대로 fresh capability를 다시 읽고 검증한다.
- `SDOWrite capability missing`, connection, response, identity, payload, SDK policy 및 journal/admission
  blocker는 계속 fail-closed한다.
- optional same-value qualification runner는 current cached observation을 요구하는 기존 보수 gate를 유지한다.

수정 후 검증:

- canonical WPF Release build: PASS
- distribution example source + current SDK Release reference build: PASS
- WPF SDO focused smoke: 17/17 PASS
- stale-only observation은 fresh-preflight 경로로 허용: PASS
- stale + `SdoWriteCapabilityMissing`은 계속 차단: PASS

## 사용자 qualification에서 확인할 항목

1. Connect/capability refresh 후 same-value canary 없이 ordinary Write 버튼이 활성화되는지 확인한다.
2. Axis1에서 operator가 승인한 object를 1/2/4 byte별로 baseline과 같은 값부터 시험한다.
3. 첫 클릭은 baseline/confirmation만 만들고 Write ticket이 없어야 한다.
4. 두 번째 exact 클릭은 pre-write bytes가 같을 때 Write ticket 한 개만 만들어야 한다.
5. terminal `Completed/Success` 뒤 exact readback이 expected bytes와 같아야 journal이 resolve된다.
6. baseline 변경, capability/BootId/MapRevision 변경, ObjectIndex 0, contention, timeout, disconnect에서는
   hidden second Write와 automatic replay가 0회인지 확인한다.
7. Axis1 결과를 Axis2~4에 복사 판정하지 않고 각 축 evidence를 별도로 남긴다.

## 2026-09-02 1-byte live attempt 분석 및 보정

사용자 실기 로그에서 `0x6060:0 / UInt8 / Length=1 / WriteData=01`은 첫 클릭의 baseline Read와
두 번째 클릭의 safe-axis, fresh capability, pre-write guard까지 통과했다. 그러나 실제 Write submit
직전 PC quarantine evidence 생성기가 `WriteData.Length == 4`를 강제하여
`ArgumentException`을 발생시켰다. 로그 판정은 `D5_EXTERNAL_NOT_SUBMITTED`이며 이 시도에서 PLC로
Write는 전송되지 않았다.

수정 내용:

- quarantine evidence가 Generic SDO scalar 계약과 동일하게 1/2/4-byte WriteData를 허용한다.
- evidence의 `WriteData.Length`는 `DataLength`와 정확히 같아야 한다.
- baseline Read가 PLC terminal slot을 교체하면 이전 terminal UI ticket을 즉시 폐기한다.
- 이후 표시된 `ErrorId=-32000, DetailCode=23`은 Write 실패가 아니라 이미 교체된 이전 ticket을
  재조회한 stale-ticket 오류였으며, 위 ticket 폐기로 차단한다.

수정 후 PC 검증:

- canonical WPF Release build: PASS, warning 0 / error 0
- distribution example source + current SDK Release reference build: PASS
- WPF SDO focused smoke: 18/18 PASS
- 1/2/4-byte quarantine evidence regression: PASS

실제 PLC Write terminal 결과와 exact readback은 새 실행 파일로 재시험하기 전까지 미확정이다.

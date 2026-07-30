# Elmo Master API 개발 진행 현황

- 기준 시각: 2026-07-30 12:02 KST
- 대상: `main@6ce2cb2b9e49647b22c8c99e6c43f9a38a48d00c` + 현재 working tree
- 릴리스 표기: `LasalMotionControlLib 0.9.1-preview`
- HTML 대시보드: [API_DEVELOPMENT_PROGRESS_2026-07-30.html](API_DEVELOPMENT_PROGRESS_2026-07-30.html)
- 개발 계획: [API_DEVELOPMENT_PLAN_2026-07-30.md](API_DEVELOPMENT_PLAN_2026-07-30.md)

> 이 문서는 커밋된 릴리스 상태가 아니라 2026-07-30의 대규모 미커밋 working-tree
> snapshot이다. 11시대에 C# production/test source를 고정한 뒤 SDK Debug/Release와 WPF Release
> 전량을 다시 실행했다. 아래 PC 결과는 그 current source의 완료 관측값이지만, 목적별 commit과
> clean checkout 재현 전이므로 최종 release baseline은 아니다.

## 한 줄 결론

**C# API와 WPF current 자동 회귀는 전량 PASS했지만, 최신 LASAL generated source 정합과
IDE/PLC/실축 검증이 끝나지 않아 production 배포는 불가하다.** 현재 병목은 목적별 source baseline
고정, LASAL Save/Generate/Rebuild/Link, PLC qualification이다.

## 한눈에 보는 현재 상태

| 지표 | 현재 값 | 판정 |
|---|---:|---|
| 요구사항 완전/적응 구현 | **40/65 (61.5%)** | 활성 기능 경로가 있는 항목만 계산 |
| 부분 구현 포함 | **50/65 (76.9%)** | 비활성 scaffold 포함, production 완료율 아님 |
| 상위 요구사항 기능 경로 | **17/21 (81.0%)** | `HomeDS402`, `HomeDS402Ex`, `SetOpMode`, `SetPosition` 공백 |
| C# protocol command ID | **62** | 현재 source 자동 대조 |
| LASAL dispatcher contract | **59** | 성공 가능 active 53 + reserved/dormant 6 |
| SDK 자동 시험 | **975/975 Debug, 975/975 Release PASS** | current C# source 전량 PASS |
| WPF Release | **Rebuild warning/error 0/0, smoke 208/208 PASS** | current C#/WPF source 전량 PASS |
| LASAL SourceOnly | **PASS** | `Phase5TransportClean / StaticTopologyOnly` |
| LASAL full/network static | **FAIL** | stale `Classes.lcb`가 삭제된 `_TCPIPServer_RT` 등록을 유지; IDE 재생성 필요 |
| PLC/실축 | **부분 검증** | 일부 happy path만 PASS, 전체 matrix 미완료 |
| 배포 | **차단** | `0.9.1-preview`, production DoD 미충족 |

단일 숫자로 합치지 않는다. PC test 통과율, 기능 커버리지, LASAL 통합, PLC 실기는 서로 다른
증거다. 특히 ACK는 명령 수락 증거이지 최종 완료 증거가 아니다.

## 진행 축

### 1. 요구사항 커버리지

| 분류 | 개수 | 비율 | 의미 |
|---|---:|---:|---|
| 직접 구현 | 16 | 24.6% | 공개 C# 경로와 LASAL 실행 경로 존재 |
| LASAL 적응 구현 | 24 | 36.9% | 다른 API/workflow로 목적 달성 |
| 부분 구현/비활성 | 10 | 15.4% | 제한 범위 또는 capability/policy OFF |
| 실제 미구현 | 11 | 16.9% | 공개 API 또는 PLC handler 없음 |
| 흡수/비동등 보류 | 4 | 6.2% | 다른 API에 흡수하거나 1:1 복제가 부적절 |
| 합계 | 65 | 100% | 기준 workbook 65개 |

요구사항 원본 감사 기준의 완전/적응 구현률은 `40/65 = 61.5%`다. 부분 구현을 포함하면
`50/65 = 76.9%`지만, 이 수치는 PLC live 통과율이 아니다.

### 2. PC/API와 wire 구현

- canonical C# API: `LMC_Library/LMC_API_Delivery/src`
- canonical 개발 WPF: `LMC_Library/LasalApiWpfTestApp`
- canonical PLC source: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- C# protocol ID 62개 중 LASAL TCP dispatcher contract는 59개다.
- dispatcher 59개 중 성공 응답 가능 active command는 53개다.
- C#에는 있으나 LASAL runtime route가 없는 command는 정확히 아래 3개다.
  - `0x7E13` EtherCAT Node Health
  - `0x7E22` Digital I/O Read
  - `0x7E23` Digital Output Write
- 09:07 이후 추가 변경된 `LmcAxisStopWait`/`LmcAxisResetWait`와 WPF
  `AxisCommandRecoveryJournal`은 PC wait/resume/recovery 계층이다. Stop은 기존
  `0x2022 -> 0x2028`, Reset은 기존 `0x2024 -> 0x2028`을 사용하며 새 wire opcode나
  LASAL dispatcher case를 추가하지 않았다.

### 3. PC 자동 검증

2026-07-30 11시대 current C# source에서 재실행한 결과다.

| Gate | 마지막 관측 결과 | 현재 판정 |
|---|---|---|
| SDK Debug forced Rebuild + tests | 975/975 PASS | current source 전량 PASS |
| SDK Release forced Rebuild + tests | 975/975 PASS | current source 전량 PASS |
| WPF Release Rebuild | warning 0, error 0 PASS | current source build PASS |
| WPF Release smoke | 208/208 PASS | full run 126.5초; targeted safety/process 회귀도 별도 PASS |
| LASAL SourceOnly static | PASS | external source 계약 PASS |
| LASAL full/network static | FAIL | stale generated registration; IDE Save/Generate/Rebuild 필요 |

WPF targeted 결과는 Axis Reset `7/7`, Motion `30/30`, Axis Stop/Reset integration `18/18`,
Axis command journal `9/9`, 실제 child-process recovery `4/4`, Axis Power `28/28` PASS다.
ACK 뒤 durable mark 직전 Kill, Reset -> Stop pinned abort/reconnect, completed Reset + Stop NACK의
final D0 match/mismatch와 old-session late event 격리를 포함한다. 이는 fake-RPC/PC 증거이며 PLC
runtime 또는 물리 정지·오류 해제 증거가 아니다.

### 4. LASAL IDE와 PLC/실기

| Gate | 상태 | 확인된 범위 | 남은 범위 |
|---|---|---|---|
| 최신 LASAL IDE Rebuild/Link | 미완료 | 2026-07-24 이전 snapshot build 이력 | 현재 topology/same-peer/SDO 포함 build |
| implementation smoke/log | 미완료 | 과거 일부 smoke | 변경 class 전체, 신규 `CInvalidArgException=0` |
| current PLC cold download | 미완료 | 과거/부분 download 이력 | Git/source/network/unit/task 정합 확인 |
| motion/group live | 부분 | Admin, 대표 axis/group move/stop/power 경로 | 25-command 전체 matrix, fault/race/final state |
| D1/D2 live | 부분 | Catalog/PI, 4-entry Bulk happy path | fault/stale/24-entry/100회/soak |
| D3/D4 live | 미검증 | PC code/build | Single/Ring/trigger/reconnect; Double은 gate OFF |
| D5 Read live | 부분 | 1/2/4-byte와 TypeMismatch 복구 | abort/contention/timeout/cancel/orphan/late callback |
| topology live | 부분 | `0x7E11` 1회 + `0x7E12` 7회 static inventory | dynamic Health/DI/DO와 physical correlation |

## 영역별 상세 판정

| 영역 | 현재 판정 | 구현된 핵심 | 완료로 볼 수 없는 이유 |
|---|---|---|---|
| RPC/connection/lookup | 핵심 구현 | init/register/close, session generation, lookup, same-peer takeover source | typed PLC event sender 없음; master current build/fault/soak 미완료 |
| Single Axis 1..9 | 핵심 구현·검증 부분 | Power/Reset/Stop, status/position, absolute/relative/velocity, accepted-once wait/recovery | physical 1..4 전체 matrix와 simulated 5..9 범위 승인 미완료; Homing/SetPosition/SetOpMode 없음 |
| Group X/Y/Z/U | 핵심 구현·검증 부분 | member/status/power/lock/reset/stop/position/linear abs·rel/fixed identity | true Buffered, stop-first, `0x2047` 최신 live와 full matrix 미완료 |
| Admin | 제한 범위 구현 | capability, axis/group semantic read, group relative move | 쓰기/general raw parameter 없음; invalid/stale/fault matrix 미완료 |
| D1 Catalog/Health/PI | 구현·검증 부분 | Catalog, EtherCAT Health, PI Read | fault/stale matrix와 최신 full qualification 미완료 |
| D2 Bulk | 구현·검증 부분 | Configure/Status/Snapshot/Release | exact 24-entry lifecycle, offline partial/recovery, soak 미완료 |
| D3 Recorder | source/PC 완료·live 미검증 | Single/Ring/trigger/download/reconnect tooling | PLC runtime, hash/soak/reconnect-adopt 증거 없음 |
| D4 Double | dormant/비활성 | two-bank source 계약과 WPF durable recovery | capability bit 6과 네 route gate OFF; RAM/jitter/live 미검증 |
| D5 SDO | Read 부분 완료·Write 비활성 | 1/2/4-byte Read, ticket/status/cancel, PC recovery tooling | fault matrix 미완료; Write allowlist/gate OFF; 8-byte/extended 미구현 |
| EtherCAT topology/I/O | static inventory만 완료 | 7-entry configured topology와 WPF/API | `0x7E13/22/23`, bits 15~17, dynamic/physical proof 없음 |
| WPF qualification | current PC 회귀 PASS | motion/Bulk/Recorder/D5/topology runner와 durable recovery, Release 208/208 | PLC execution과 clean-checkout 재현 미확정 |
| Distribution | 과거 preview | version `0.9.1-preview` 구조 | current source 재조립/manifest/외부 manual 갱신 전 |

## 현재 완료로 인정하는 범위

- TCP transport와 control/diagnostics service의 source 책임 분리
- C# request/parser/fake-RPC 계약의 광범위한 자동 시험
- Admin `0x7D00/10/20/22` happy path
- 대표 axis/group motion, Stop/PowerOff 경로
- D1 Catalog/axis 1..4 PI Read happy path
- D2 4-entry Bulk happy path
- D5 general-inline 1/2/4-byte Read와 동일 BootId TypeMismatch 후 복구
- configured topology `0x7E11/0x7E12`, revision `0x15867EEC`, 7-entry wire 응답
- 외부 시험 프로젝트의 동일 IPv4 stale-socket takeover happy path

각 항목의 “완료”는 적힌 증거 범위에만 적용한다. current master build/download, fault matrix,
실축 안전 성능까지 확대하지 않는다.

## 현재 blocker

1. **working tree가 아직 대규모 미커밋 상태다.** current C# source 전량 PASS는 확보했지만,
   목적별 commit과 clean checkout 재현 전에는 고정 release baseline이 아니다.
2. **LASAL full static이 FAIL한다.** current `Classes.lcb`가 삭제된 `_TCPIPServer_RT`를 계속
   가리키며 `Networks.lcb`, `.lba`, export와 root `.lcb`도 이전 생성본이다. 외부 시험 생성물을
   복사하지 말고 마스터 LASAL에서 Save/Generate/Rebuild/Link해야 한다.
3. **최신 LASAL IDE build/download가 없다.** current topology/same-peer/SDO source를 포함한
   Rebuild/Link, implementation smoke, log, cold download가 필요하다.
4. **PLC qualification matrix가 미완료다.** motion/group 25개, D1/D2 fault·soak,
   D3/D4 runtime, D5 fault/recovery가 남았다.
5. **동적 CREVIS I/O는 PLC 미구현이다.** `0x7E13/22/23`과 bits 15~17은 OFF다.
6. **D4 Double과 SDO/PI Write는 의도적으로 gate-off다.** 승인 없이 활성화하면 안 된다.
7. **실제 안전 범위가 승인되지 않았다.** E-stop, HW/SW limit, UNIT, reference/home,
   one-motion-owner 정책을 장비에서 확인해야 한다.

## production 판정

현재 production 판정은 **NO-GO**다. 최소한 아래가 모두 닫혀야 한다.

- source hash가 고정된 SDK/WPF/정적 계약 전량 PASS
- LASAL IDE Rebuild/Link와 implementation smoke PASS
- current PLC download와 source/network/unit/task provenance 일치
- 안전 chain/limit/UNIT/reference 승인
- active command별 PLC E2E, stable final state, packet 재캡처 완료
- callback/ownership 범위를 구현하거나 명시적으로 제외
- 외부 사용자 문서의 preview/안전/UNIT/polling 제약 반영
- Distribution cleanup, version/hash/manifest 재생성

## 근거

- [현재 아키텍처 및 릴리스 상태](../architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)
- [요구사항 커버리지](../architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md)
- [Diagnostics 잔여 구현 계획](../architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md)
- [EtherCAT topology/I/O 설계](../architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md)
- [Test2 topology capture audit](../architecture/LMC_ETHERCAT_TEST2_CAPTURE_AUDIT_2026-07-28.md)
- [API delivery README](../../LMC_Library/LMC_API_Delivery/README.md)
- [자동 시험 문서](../../LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md)
- [개발 backlog](../../LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)
- [packet map](../../LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt)

상세 설계 문서는 일부 과거 시험 수치와 서로 충돌한다. 이 문서의 자동 시험 수치는
2026-07-30 직접 실행 관측값을 사용했고, topology는 최신 Test2 capture audit를 우선했다.

# Elmo Master API 개발 진행 현황

> **2026-08-03 후속 설계 우선:** 이 문서는 2026-07-30/31 snapshot이다. 아래의
> `ReferenceAxis`, switch-search Home, `MoveReference()` 설명과 당시 test 수치 및
> SourceOnly PASS 판정은 현재 상태가 아니다. 현재 Home/encoder 구현과 IDE 순서는
> [LMC Home current-position zero and encoder maintenance IDE handoff](../architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md),
> 최신 소스 대조는 [260803 continuation summary](../history/260803/99_analysis_summary.md)의
> 9절을 따른다.

- 기준 시각: 2026-07-31 working-tree
- 대상: `main@6537bcf1bf0fdb338a934b63891fc9ee110aecad` + 현재 working tree
- 릴리스 표기: `LasalMotionControlLib 0.9.1-preview`
- HTML 대시보드: [API_DEVELOPMENT_PROGRESS_2026-07-30.html](API_DEVELOPMENT_PROGRESS_2026-07-30.html)
- 개발 계획: [API_DEVELOPMENT_PLAN_2026-07-30.md](API_DEVELOPMENT_PLAN_2026-07-30.md)

> 이 문서는 커밋된 릴리스 상태가 아니라 2026-07-30의 대규모 미커밋 working-tree
> snapshot이다. 최신 source에서 SDK/WPF Debug/Release forced Rebuild와
> manual SDO identity-pinned pre-wire/lifecycle 회귀가 모두 PASS했다. LASAL
> `IntegratedReadOwnerDormant` SourceOnly/full static은 current PASS다. Admin
> `0x7D12 SetAxisPosition`과 `0x7D13 StartAxisReference` dormant/fail-closed current
> source/static 및 Reference PC 계약 시험은 PASS했다. fresh IDE Rebuild/Link PASS는 callback
> ownership, `0x7D12`, `0x7D13` 편집 전 관측값이므로 callback+`0x7D12`+`0x7D13` current
> source의 Save/Rebuild/Link/smoke가 남았다.
> transactional Distribution은 `2.0-candidate` DOCX/PDF exact bytes, semantic
> policy 15-check와 schema 2 manifest를 포함한 sibling candidate까지 당시 입력으로 PASS했다.
> 이후 Group Reset source가 변경되어 이 candidate는 historical/stale input이다. 이 결과는
> `dirty-preview`이며 목적별 commit, clean checkout과 새 candidate 재현 전에는 최종 release
> baseline이 아니다.

> **2026-08-12 SetPosition override:** SDK의 read-only `0x7D14`에 이어 `0x7D1A
> RetireAxisSetPositionOutcome` nonzero-generation CAS와 Admin bit 7 계약을 추가했고,
> Debug/Release 1151/1151이 PASS했다. PLC bit 3/5/7, retained store,
> `0x7D14/0x7D1A` route/tombstone과 WPF journal 연결은 여전히 없으며, 아래 1042 수치와
> query-only 설명은 2026-07-31 snapshot이다.

## 한 줄 결론

**최신 Single Axis whole-sequence recovery, Group Enable/SDO/Group Reset과 dormant Admin
SetPosition/Axis Reference를 포함한 current PC 회귀와 LASAL source/static은 PASS했다.
SetPosition은 128-bit intent와 diagnostics identity를 포함하는 56-byte request로 갱신했고,
`0x7D14` read-only outcome query, `0x7D1A` nonzero-generation retirement CAS SDK 계약과
독립 journal core까지 추가했다.
callback+`0x7D12`+`0x7D13` current source의 fresh IDE build/smoke는 대기 중이다.
하지만 current PLC download와 Motion/Power/Axis1-only SDO Write live 증거가 없어 production
배포는 불가하다.** dirty-preview Distribution candidate는 실제 생성됐고, 현재 병목은 source
baseline 고정, clean candidate 독립 재현, PLC provenance와 safety/readback/packet qualification이다.

## 한눈에 보는 현재 상태

| 지표 | 현재 값 | 판정 |
|---|---:|---|
| 요구사항 완전/적응 구현 | **40/65 (61.5%)** | 활성 기능 경로가 있는 항목만 계산 |
| 부분 구현 포함 | **52/65 (80.0%)** | dormant/capability-off 구현 포함, production 완료율 아님 |
| 상위 요구사항 분류 | **active 17 / dormant·partial 2 / missing 2** | partial은 `HomeDS402` via LASAL `ReferenceAxis`, `SetPosition`; missing은 `HomeDS402Ex`, `SetOpMode` |
| C# unique protocol command ID | **64** | 현재 source 자동 대조 |
| LASAL handled route | **63** | success-capable active 53 + dormant read-owner 2 + reserved/dormant 8 |
| SDK 자동 시험 | **1042/1042 PASS** | current Debug/Release 각각; Admin SetPosition 18개, Axis Reference 16개와 fake-RPC snapshot 회귀 포함 |
| WPF 자동 시험 | **297/297 PASS** | current Debug/Release build/smoke 각각; Debug 추가 반복 count 동일 |
| LASAL SourceOnly | **PASS** | `Phase5TransportClean / IntegratedReadOwnerDormant` |
| LASAL full/network static | **PASS** | generated source/network/metadata 정적 계약 PASS |
| LASAL IDE Rebuild/Link | **세 변경 전 PASS / current fresh IDE 대기** | 기존 `0 error(s), 20 warning(s)`; callback+`0x7D12`+`0x7D13` current source 재빌드 필요; PLC runtime 증거는 아님 |
| PLC/실축 | **과거 부분 검증 / current 미검증** | fresh build download와 current Motion/Power/Axis1 SDO Write live 없음 |
| 배포 | **historical dirty-preview candidate PASS / current input stale / production 차단** | 당시 `2.0-candidate` manual exact bytes, semantic `15/15`, schema 2 manifest, canonical 무변경; Group Reset 변경 반영 candidate/clean baseline/승격 미완료 |

단일 숫자로 합치지 않는다. PC test 통과율, 기능 커버리지, LASAL 통합, PLC 실기는 서로 다른
증거다. 특히 ACK는 명령 수락 증거이지 최종 완료 증거가 아니다.

## 진행 축

### 1. 요구사항 커버리지

| 분류 | 개수 | 비율 | 의미 |
|---|---:|---:|---|
| 직접 구현 (D) | 16 | 24.6% | 공개 C# 경로와 LASAL 실행 경로 존재 |
| LASAL 적응 구현 (E) | 24 | 36.9% | 다른 API/workflow로 목적 달성 |
| 부분 구현/비활성 (P) | 12 | 18.5% | 제한 범위, dormant route 또는 capability/policy OFF |
| 실제 미구현 (G) | 9 | 13.8% | 공개 API 또는 PLC handler 없음 |
| 흡수/비동등 보류 (X) | 4 | 6.2% | 다른 API에 흡수하거나 1:1 복제가 부적절 |
| 합계 | 65 | 100% | 기준 workbook 65개 |

요구사항 원본 감사 기준의 완전/적응 구현률은 `40/65 = 61.5%`다. 부분 구현을 포함하면
`52/65 = 80.0%`지만, 이 수치는 PLC live 통과율이 아니다.

### 2. PC/API와 wire 구현

- canonical C# API: `LMC_Library/LMC_API_Delivery/src`
- canonical 개발 WPF: `LMC_Library/LasalApiWpfTestApp`
- canonical PLC source: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- C# unique protocol ID 64개 중 LASAL handled route는 63개다.
- handled route 63개는 success-capable active 53개, capability-off dormant read-owner
  `0x7E13/0x7E22` 2개, reserved/dormant 8개로 나뉜다. 마지막 8개에는 diagnostics 6개와
  Admin `0x7D12 SetAxisPosition`, `0x7D13 StartAxisReference`가 포함된다.
- Admin family는 active 4개와 dormant 2개다.
- C#에는 있으나 LASAL runtime route가 없는 command는 `0x7E23` Digital Output Write 1개다.
- `0x7D12`는 28-byte request/36-byte response, expected actual-position CAS와 prepare-time
  one-shot intent를 구현했다. capability bit 3은 OFF다. exact valid raw request도
  `InvalidState/detail 10`으로 끝나고 native `_LMCAxis.SetPosition` 호출은 0회다. malformed 또는
  write-boundary 이후 결과 불명확은 exact session을 fault시킨다. Detail 11 native reject는
  common `ErrorId=-6`만 허용하고 payload `P+24`의 full `_LMCAXIS_CMDERROR` U32를 typed 예외에
  보존한다. positive/other ErrorId는 malformed다. 활성화 조건은
  [bounded coordinate-correction 설계](../architecture/AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md)를 따른다.
- `0x7D13 StartAxisReference`는 56-byte request/32-byte response의 dormant fail-closed
  계약이다. capability bit 4는 OFF, native `_LMCAxis.MoveReference()` 호출은 0회이고 WPF에는
  노출하지 않는다. 요구사항 감사에서 `HomeDS402`의 LASAL-native 적응으로 분류하지만 DS402
  homing이 아니다. 현재 Motion Network에는 physical `HWMin/HWMax/RefSwitch/ZImpulse/LatchPos`
  source가 없으며, 활성화 시 PLC가 독립 감시할 `MaxTravel>0`과 `TimeoutMs>0`은 mandatory다.
  상세 활성 조건은 [Axis Reference dormant 계약](../architecture/AXIS_REFERENCE_LASAL_NATIVE_DORMANT_CONTRACT_2026-07-31.md)을 따른다.
- 09:07 이후 추가 변경된 `LmcAxisStopWait`/`LmcAxisResetWait`와 WPF
  `AxisCommandRecoveryJournal`은 PC wait/resume/recovery 계층이다. Stop은 기존
  `0x2022 -> 0x2028`, Reset은 기존 `0x2024 -> 0x2028`을 사용하며 새 wire opcode나
  LASAL dispatcher case를 추가하지 않았다.
- raw Group Reset `GroupReset[Async]`는 `0x2049` ACK-only로 남는다. WPF 버튼과 stable API는
  valid `0x20D2` observed snapshot -> `0x2049` exactly once -> Resume round별 `0x2045` + pinned
  member 전원의 `0x2028`, all-clear 3회 연속 계약을 구현한다. timeout/cancel/status failure는
  same-session status-only continuation을 보존하고, accepted/outcome-uncertain
  Stop/PowerOff/safe Disable 또는 pinned-member mutation은 terminal supersede다. valid safety
  NACK는 continuation을 보존한다. command-before durable journal은 endpoint,
  DiagnosticsBuild/BootId/MapRevision, group과 ordered member identity를 고정한다. reconnect/process
  restart는 fresh `0x20D2`를 한 번만 다시 읽어 exact-match한 경우에만 새 status-only continuation을
  attach하며 `0x2049`는 재전송하지 않는다.

### 3. PC 자동 검증

아래 수치는 latest working-tree source에서 forced Rebuild 뒤 다시 관측했다.

| Gate | 마지막 관측 결과 | 현재 판정 |
|---|---|---|
| SDK Debug build + tests | 1042/1042 PASS | current working tree; fake-RPC request/session atomic append와 stable snapshot 회귀 포함; 추가 반복 동일 count |
| SDK Release build + tests | 1042/1042 PASS | current working tree; Admin SetPosition 18개와 Axis Reference 16개 포함 |
| WPF Debug build/smoke | 297/297 PASS | current working tree; 추가 반복 동일 count |
| WPF Release build/smoke | 297/297 PASS | current working tree |
| 개발 Release binary | EXE `890EAC64...7E0B6`, DLL `F273F4DD...6BBF8` | version 0.9.1.0, 2026-07-31 forced Release Rebuild 산출물; 미커밋 개발 산출물이며 Distribution 아님 |
| Single Axis live qualification focused | 9/9 PASS | exact Power/Move/Stop/PowerOff, pre-wire cancel, Build drift zero mutation, 외부 Stop/PowerOff no-duplicate fake-RPC 계약 |
| Single Axis whole-sequence recovery | focused PC PASS | 10단계 monotonic journal/exact CAS, volatile crash promotion, process 종료 뒤 자동 mutation 0회와 명시적 Power Off 안전 복구; PLC/실축 증거 아님 |
| Group Enable qualifier focused | PASS | `0x2047` exactly once, durable accepted-before-status, status-only stable proof, durable resolve 계약 |
| Group Reset stable member error-clearance | **SDK/WPF/PC 구현 / PLC 미검증** | prepared/accepted durable journal, same-session 및 exact reconnect/restart status-only attach, observed member poll, all-clear 3회, safety takeover/NACK/no-replay PC 계약 PASS; PLC capture 필요 |
| Axis1 SDO manual activation focused | PASS | exact current session/build/BootId/MapRevision/target four-ticket same-value proof 전 zero-wire/비활성 계약 |
| Axis1 SDO identity-pinned submit/lifecycle | PASS | SDK fresh Build/BootId/MapRevision mismatch `NotAttempted`/`0x7E50` 0회, A→B→A 및 disconnect proof 영구 폐기 |
| topology/I/O qualifier V2 dry-run | 17/17 exact planned frames PASS | explicit scope/mode, zero network, `0x7E23` forbidden, executable/SDK SHA-256 기록 |
| LASAL SourceOnly static | PASS | `Phase5TransportClean / IntegratedReadOwnerDormant` external source 계약 PASS |
| LASAL full/network static | PASS | `IntegratedReadOwnerDormant` generated source/network/metadata 계약 PASS |
| LASAL IDE Rebuild/Link | callback/`0x7D12`/`0x7D13` 전 checkpoint PASS / current 대기 | 기존 fresh reload `0 error(s), 20 warning(s)`; 세 변경을 포함한 current source fresh build/link와 smoke 필요 |

현재 최신 변경에서 Group Enable qualification runner는 durable accepted-once 경로를 사용하고,
Axis1 manual SDO Write는 exact current-session four-ticket same-value proof 전 강제 handler
호출에서도 zero-wire로 닫힌다. second-click은 SDK mutation gate의 fresh identity exact 비교를
통과해야 하며 mismatch/disconnect proof는 영구 폐기된다. SDO 성공 evidence는 baseline,
`preWriteGuard`, Write, readback의 서로 다른 ticket과 각 terminal `resultBytes`를 보존한다.
이 결과는 fake-RPC/PC 전체 회귀 증거이며 PLC runtime, 물리 Lock/Write/정지 증거가 아니다.

### 4. LASAL IDE와 PLC/실기

| Gate | 상태 | 확인된 범위 | 남은 범위 |
|---|---|---|---|
| 최신 LASAL IDE Rebuild/Link | callback/`0x7D12`/`0x7D13` 전 PASS / current 대기 | fresh reload 뒤 `0 error(s), 20 warning(s)`, Linker `Done`; 세 변경 전 checkpoint | callback+`0x7D12`+`0x7D13` current source fresh Save/Rebuild/Link, warning 20 분류와 설치 library 정합 복구 |
| implementation smoke/log | callback/`0x7D12`/`0x7D13` 전 PASS / current 대기 | `LMCEcatInputLatch`, `LMCDiagnosticsService`, `TCPMotionInterface` 직접 open 성공; 당시 신규 `CInvalidArgException=0` | 세 변경 이후 current `TCPMotionInterface`/`LMCControlCommandService` smoke/log 재확인; PLC runtime 증거와 혼동 금지 |
| current PLC cold download | 미완료 | 과거/부분 download 이력 | Git/source/network/unit/task 정합 확인 |
| current motion/group live | 미검증 | 과거 대표 capture와 current source/PC 경로만 존재 | fresh download 뒤 Motion/Power/Group Enable 25-command matrix, fault/race/final state |
| D1/D2 live | 부분 | Catalog/PI, 4-entry Bulk happy path | fault/stale/24-entry/100회/soak |
| D3/D4 live | 미검증 | PC code/build | Single/Ring/trigger/reconnect; Double은 gate OFF |
| D5 Read live | 부분 | 1/2/4-byte와 TypeMismatch 복구 | abort/contention/timeout/cancel/orphan/late callback |
| D5 Write live | 미검증 | Axis1 exact target source-active; exact-session four-ticket/manual gate focused PC PASS | fresh bit 9, UI24 ownership, `preWriteGuard`/`resultBytes`, four-ticket/readback, pcap/physical proof |
| topology live | source/static 완료·runtime 미검증 | `0x7E11/12` static inventory와 dormant `0x7E13/22` read-owner | current PLC download, raw qualifier, dynamic Health/DI physical correlation; `0x7E23` 미구현 |
| callback endpoint live | source/PC 계약·runtime 미검증 | unchanged `0x405C`, valid current TCP peer + port, first-valid commit, exact-duplicate idempotence, mismatch-preserve; raw SDK/WPF session provenance | fresh IDE compile/download, peer byte order와 duplicate/mismatch ACK capture, 실제 UDP event sender/payload |

## 영역별 상세 판정

| 영역 | 현재 판정 | 구현된 핵심 | 완료로 볼 수 없는 이유 |
|---|---|---|---|
| RPC/connection/lookup | 핵심 구현 | init/register/close, same-peer takeover source, `0x405C` exact-peer validate-then-commit, raw callback `SessionGeneration`/owner/current-session provenance와 WPF stale queued event drop | P0 ownership 변경의 master IDE/PLC capture와 fault/soak 미완료; typed PLC event sender/parser 없음 |
| Single Axis 1..9 | 핵심 구현·PC 검증 부분 | Power/Reset/Stop, status/position, absolute/relative/velocity, accepted-once wait/recovery, exact-identity PowerOn -> Relative Move -> Stop -> PowerOff live runner와 cancel safe cleanup; `SetPosition`과 LASAL-native `ReferenceAxis` SDK/wire+dormant fail-closed contract | runner는 실제 전송 경로지만 current PLC/physical 1..4 실행·packet·안전 증거와 simulated 5..9 범위 승인 미완료; `HomeDS402Ex`/`SetOpMode` 없음; Reference bit 4 OFF/native call 0/physical ref input 없음, DS402 homing 아님; SetPosition bit 3 OFF/native call 0 |
| Group X/Y/Z/U | 핵심 구현·PC 검증 부분 | member/status/power/lock, raw ACK-only Reset과 durable exact-restart stable member error-clear Reset, stop/position/linear abs·rel/fixed identity, Enable qualifier durable accepted-once | true Buffered, stop-first, `0x2047`/`0x2049` PLC live와 full matrix 미완료 |
| Admin | 2026-07-31 active/dormant snapshot + SDK-only SetPosition recovery 2 | capability, axis/group semantic read, group relative move; diagnostics identity+128-bit intent `0x7D12` 56/36-byte 계약, read-only `0x7D14` 56/92-byte terminal query와 `0x7D1A` 60/92-byte nonzero-generation retirement CAS SDK 계약 | SetPosition bits 3/5/7 OFF, native call 0, WPF mutation 미노출; `0x7D14/0x7D1A` PLC store/route/tombstone, unified ownership과 PLC invalid·stale·fault matrix 미완료 |
| D1 Catalog/Health/PI | 구현·검증 부분 | Catalog, EtherCAT Health, PI Read | PLC fault/stale matrix와 live qualification 미완료 |
| D2 Bulk | 구현·검증 부분 | Configure/Status/Snapshot/Release | exact 24-entry lifecycle, offline partial/recovery, soak 미완료 |
| D3 Recorder | source/PC 완료·live 미검증 | Single/Ring/trigger/download/reconnect tooling | PLC runtime, hash/soak/reconnect-adopt 증거 없음 |
| D4 Double | dormant/비활성 | two-bank source 계약과 WPF durable recovery | capability bit 6과 네 route gate OFF; RAM/jitter/live 미검증 |
| D5 SDO | Read 부분 완료·Axis1 Write source/PC active | 1/2/4-byte Read, Axis1 `0x2F00:24 Int32/4`, exact-session four-ticket proof, identity-pinned manual gate, guarded readback/recovery | current PLC Write 미검증; axis 2~4 차단; 8-byte/extended 미구현 |
| EtherCAT topology/I/O | read-owner dormant 구현·live 미검증 | 7-entry configured topology, 464-byte coherent snapshot, `0x7E13/22` PLC route/handler, qualifier V2 exact 17-frame dry-run | bits 15~17 OFF, current PLC/raw/physical proof 없음; `0x7E23` 미구현 |
| WPF qualification | current PC Debug/Release 297/297 PASS | Single Axis 실제 PowerOn/Relative Move/Stop/PowerOff accepted-once runner와 10단계 whole-sequence crash journal/process-restart safety recovery, post-Move cancel safe cleanup, external Axis Stop/PowerOff no-duplicate 인수, Group Enable durable accepted-once, Axis1 exact-session SDO activation/pinned submit 및 기존 motion/Bulk/Recorder/D5/topology runner | current PLC/실축 실행·packet 증거와 clean-checkout 재현 미확정 |
| Distribution | historical dirty-preview candidate PASS / current input stale | 원본 무변경 sibling staging, exact-byte manual snapshot, prepared Git provenance, success-only rename, schema 2 manifest와 15-check semantic policy; 당시 manifest 56/56, policy 28/28, transaction 86/86 PASS | 이후 Group Reset source 변경 미반영; 새 candidate, clean checkout 재현, 독립 검토와 canonical 승격 미완료 |

## 현재 완료로 인정하는 범위

- TCP transport와 control/diagnostics service의 source 책임 분리
- C# request/parser/fake-RPC 계약의 광범위한 자동 시험
- Admin `0x7D00/10/20/22` happy path, diagnostics identity+128-bit intent를 고정한
  `0x7D12 SetAxisPosition` dormant/fail-closed request/parser/session 계약, SDK의 repeatable
  read-only `0x7D14` exact terminal query와 `0x7D1A` nonzero-generation retirement CAS 계약,
  `0x7D13 StartAxisReference` dormant 계약.
  `0x7D13`은 LASAL-native reference이며 DS402 homing이 아니다. `0x7D12/13` capability는
  OFF/native call 0이고 `0x7D14`는 PLC route/store가 아직 없다. 현재 test count와
  SourceOnly/full static 결과는 아래 검증 결과를 기준으로 갱신한다.
- 대표 axis/group motion, Stop/PowerOff의 source/PC 경로. current PLC Motion/Power 증거는 아님
- Single Axis live runner의 정확한 실제 opcode 순서, fresh ready status, movement/stable/final-position
  proof, 10단계 whole-sequence journal, crash/process-restart zero-replay와 명시적 Power Off recovery,
  Move zero-replay cancel cleanup PC 계약. current PLC/실축 실행 증거는 아님
- Group Enable qualifier의 command-once, durable accepted-before-status, status-only stable proof와
  resolve focused PC 계약. current PLC Lock 증거는 아님
- Group Reset의 command-once, pinned group/member status-only stable proof, same-session Resume,
  exact-identity reconnect/process-restart attach, accepted/outcome-uncertain safety supersede, valid
  NACK restore와 WPF fail-closed interlock. current PLC error-clear 증거는 아님
- D1 Catalog/axis 1..4 PI Read happy path
- D2 4-entry Bulk happy path
- D5 general-inline 1/2/4-byte Read와 동일 BootId TypeMismatch 후 복구
- Axis1 `0x2F00:24 Int32/4` SDO Write source/PC 계약, exact session/build/BootId/MapRevision
  four-ticket same-value proof 뒤 manual Write 활성, `preWriteGuard`/terminal `resultBytes` evidence와
  recovery readmission. PLC Write 증거는 아님
- configured topology `0x7E11/0x7E12`, revision `0x15867EEC`, 7-entry wire 응답
- `IntegratedReadOwnerDormant`의 464-byte snapshot, `0x7E13/0x7E22` route/handler와
  SourceOnly/full static. callback/`0x7D12`/`0x7D13` 전 IDE build/smoke만 PASS했으며 current PLC/live
  증거는 아님
- 당시 canonical package 무변경 transactional candidate, input/seal/canonical/Git metadata drift 검출,
  success-only rename과 15-check semantic policy의 PC fixture. 검토한 `2.0-candidate` DOCX/PDF
  exact bytes를 사용한 당시 input 전체 실행도 PASS했다. 이후 Group Reset source 변경으로 현재
  input candidate는 아니다. 당시 release input tree hash는
  `09BEAD2F...DE9F`, manifest hash는 `AF3F12ED...9915`, canonical tree hash는 전후
  `3AE733AF...CA1CA`, staging/lock residue는 0이다
- 외부 시험 프로젝트의 동일 IPv4 stale-socket takeover happy path

각 항목의 “완료”는 적힌 증거 범위에만 적용한다. current master build/download, fault matrix,
실축 안전 성능까지 확대하지 않는다.

## 현재 blocker

1. **current working tree의 목적별 commit과 clean checkout 재현이 남았다.** 동일 commit
   집합을 새 checkout에서 SDK/WPF/SourceOnly/full gate로 다시 확인해야 고정 baseline이다.
2. **fresh LASAL build가 current PLC에 적용됐다는 증거가 없다.** SourceOnly/full static은
   마지막 관측에서 PASS했지만 callback+`0x7D12`+`0x7D13` current source의 fresh IDE
   Rebuild/Link와 implementation smoke부터 남아 있다.
   cold download, BootId/MapRevision 및 source/network/unit/task provenance도 없다.
3. **PLC qualification matrix가 미완료다.** motion/group 25개, D1/D2 fault·soak,
   D3/D4 runtime, D5 fault/recovery가 남았다. Group Reset raw API는 ACK-only이고 stable
   SDK/WPF는 PC에서만 검증됐으므로 valid
   `0x20D2` snapshot, `0x2049` 1회, round별 `0x2045` + member별 `0x2028`, all-clear 3회와
   safety/no-replay capture가 필요하다.
4. **동적 CREVIS read-owner는 dormant source/static까지만 완료됐다.** `0x7E13/22` route와
   464-byte owner는 구현됐지만 bits 15~17은 OFF이고 current PLC/raw/physical proof가 없다.
   `0x7E23` output route/owner는 아직 구현되지 않았다.
5. **D4 Double과 PI Write는 gate-off다. Axis1 SDO Write는 source-active지만 PLC/live
   qualification이 없어 production 승인 상태가 아니다.**
6. **dirty-preview Distribution candidate는 생성됐지만 Group Reset 변경 전 입력이라 stale이며 release baseline이 아니다.**
   `LMC_API_Distribution_candidate_20260731_manual_2_0_provenance`는 semantic `15/15`, schema 2
   manifest와 exact manual hash 검증을 통과했다. 그러나 이후 source가 바뀌었고 입력도 대규모
   미커밋 working tree이므로 새 candidate 생성, 목적별 commit, clean checkout 재현, 독립 검토와
   별도 canonical 승격 승인이 남았다.
7. **실제 안전 범위가 승인되지 않았다.** E-stop, HW/SW limit, UNIT, reference/home,
   one-motion-owner 정책을 장비에서 확인해야 한다.
8. **SetPosition authoritative recovery의 PLC 저장소가 없다.** IDE-created two-bank
   retained store, `0x7D14` route, terminal retirement CAS와 crash-point/torn-write 시험을
   완료하고, journal과 unified mutation ownership을 같은 slice에서 WPF에 연결하기 전에는
   capability bit 3/5/7과 실제 mutation을 열 수 없다.

## production 판정

현재 production 판정은 **NO-GO**다. 최소한 아래가 모두 닫혀야 한다.

- source hash가 고정된 최신 SDK/WPF 전량 회귀와 정적 계약 PASS
- LASAL IDE Rebuild/Link와 implementation smoke PASS
- current PLC download와 source/network/unit/task provenance 일치
- 안전 chain/limit/UNIT/reference 승인
- active command별 PLC E2E, stable final state, packet 재캡처 완료
- callback endpoint ownership의 current PLC duplicate/mismatch capture를 완료하고, 실제 payload
  capture 전 typed sender/parser 및 별도 multi-PC motion owner는 명시적으로 제외
- 외부 사용자 문서의 preview/안전/UNIT/polling 제약 반영
- 원본 무변경 transactional Distribution candidate, semantic policy preflight와 version/input hash/manifest 재생성

## 근거

- [현재 아키텍처 및 릴리스 상태](../architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)
- [요구사항 커버리지](../architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md)
- [Diagnostics 잔여 구현 계획](../architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md)
- [EtherCAT topology/I/O 설계](../architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md)
- [Test2 topology capture audit](../architecture/LMC_ETHERCAT_TEST2_CAPTURE_AUDIT_2026-07-28.md)
- [transactional Distribution candidate 설계/검증](../architecture/LMC_API_TRANSACTIONAL_DISTRIBUTION_CANDIDATE_2026-07-31.md)
- [Group Reset stable member error-clearance 계약](../architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md)
- [Axis SetPosition bounded coordinate-correction 계약](../architecture/AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md)
- [Axis Reference LASAL-native dormant 계약](../architecture/AXIS_REFERENCE_LASAL_NATIVE_DORMANT_CONTRACT_2026-07-31.md)
- [API delivery README](../../LMC_Library/LMC_API_Delivery/README.md)
- [자동 시험 문서](../../LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md)
- [개발 backlog](../../LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)
- [packet map](../../LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt)

상세 설계 문서의 시험 수치는 자동 시험 문서의 current Debug/Release forced Rebuild 기준선으로
동기화한다. topology 판단은 최신 Test2 capture audit를 우선했다.

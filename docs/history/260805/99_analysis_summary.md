# Elmo Master 260805 history continuation summary

작성일: 2026-08-05 (KST)

## 1. 문서 목적

이 문서는 다음 네 원본 히스토리를 원본 보존 상태로 분할·전수 판독한 뒤, 현재 작업을 다시 시작하기 위한 기준점이다.

- `docs/history/Elmo_Master_history_260805_1.md`
- `docs/history/Elmo_Master_history_260805_2.md`
- `docs/history/Elmo_Master_history_260805_3.md`
- `docs/history/Elmo_Master_history_260805_4.md`

히스토리에 기록된 소스, 빌드, PLC와 실축 상태는 과거 시점의 증거다. 아래에서 `2026-08-05 현재 확인`으로 명시한 항목만 현재 작업트리에서 다시 확인했다. 이번 분석에서는 LASAL IDE 편집, Rebuild, PLC 다운로드와 실축 명령을 실행하지 않았다.

상세 탐색은 [index.md](index.md), 원본·분할 무결성은 [split_manifest.json](split_manifest.json), 전수 판독 근거는 다음 세 digest를 기준으로 한다.

- [parts 001-117](01_chunk_digest_parts_001_117.md)
- [parts 118-234](02_chunk_digest_parts_118_234.md)
- [parts 235-350 및 histories 2-4](03_chunk_digest_parts_235_350_and_histories_2_4.md)

## 2. 분할 결과와 무결성

| 원본 | bytes | lines | chunks | 원본 SHA-256 |
|---|---:|---:|---:|---|
| `Elmo_Master_history_260805_1.md` | 13,793,991 | 87,280 | 350 | `148dba5ccf6a6f3fd51c072462c305c98aaa1992363e1daf2e4b0cd686f293cd` |
| `Elmo_Master_history_260805_2.md` | 5,840 | 107 | 1 | `e4c865757b3d92836d135e6824c75074e1dfc2de91f97280bd46d6b5e57377e8` |
| `Elmo_Master_history_260805_3.md` | 2,168 | 32 | 1 | `40e017d3b4fdde7a3c37ae4605a1726a2b9b801902bcbd4098fd39cc58884ccc` |
| `Elmo_Master_history_260805_4.md` | 10,821 | 126 | 1 | `d56d0dfbbcac72537c3b79ad96f8669562a2fdba1e29a95d1317a84cb5de1587` |

- 총 `87,545`줄을 250줄 단위 `353`개 읽기용 청크로 만들었다.
- 원본 4개의 현재 bytes, lines와 SHA-256이 manifest와 모두 일치한다.
- 청크 `353/353`의 존재, bytes, SHA-256, 실제 줄 수, part 번호와 원본 줄 범위가 모두 일치한다.
- source별 줄 범위는 1행부터 마지막 행까지 연속이며 누락·중복 청크는 없다.
- History 1의 image/base64 run `555`개는 읽기용 청크에서 원본 줄 번호·문자 수·SHA-256 placeholder로 치환했다.
- 읽기용 청크의 후행 space/tab `251`개 행은 제거했다. 원본에는 적용하지 않았다.
- History 1 정제본 재결합 SHA-256은 `9921a2623dbd9da1dac49234dd1655b83f96828cf520d05be449b8b4eb7c9707`로 정제 기준 스트림과 일치한다.
- Histories 2~4는 변환이 없어 각 청크 재결합 hash가 원본 hash와 byte-exact하게 일치한다.
- 전수 판독 coverage는 `117/117 + 117/117 + 119/119 = 353/353`이다.

## 3. 전체 히스토리의 진행 흐름

### 3.1 시작점: `0x7D17` 부재

첫 시작점은 DS402 Home terminal record를 다음 요청이 덮어쓰지 않도록 하는 `0x7D17 RetireAxisDs402HomeOutcome`이었다. 당시 PC SDK 계약은 있었지만 LASAL handler/route와 WPF의 `0x7D16 terminal -> 0x7D17 retire -> journal Resolve` 연결은 없었다.

이후 히스토리에서는 다음을 source/IDE/static 수준으로 구현했다.

- LASAL `HandleAxisDs402HomeRetire` declaration, `0x7D17` route와 tombstone/idempotent retirement
- SDK public facade와 parser
- WPF exact `0x7D16`/`0x7D17` snapshot equality 확인 뒤 journal resolve
- retirement, replay, detail 32와 no-overwrite 정적 계약

2026-08-05 현재 source에도 LASAL handler/route, TCP route, SDK와 WPF exact-retirement 경로가 존재한다. 그러나 이것이 현재 PLC에 다운로드되어 실행됐다는 증거는 아니다.

### 3.2 Home과 encoder maintenance 요구 정리

사용자는 LMC Home, DS402 Home, TW19와 TW20을 모두 구현 대상으로 유지하도록 요청했다. 히스토리에서 기능은 다음처럼 분리됐다.

| 기능 | 현재 source 의미 | 현재 runtime 판정 |
|---|---|---|
| LMC Home | 축을 움직이지 않고 현재 actual position을 application zero로 만드는 `0x7D13/18/19` 계열 | source gate TRUE지만 최신 ABI/C78/download/실축 증거 없음 |
| DS402 Home | method 37과 별도 retained result/retirement/drain 경로 | source gate FALSE, runtime 금지 |
| TW19 | `0x20FC:01`, UInt16 value 1, multiturn position reset | source gate TRUE지만 최신 build/download 재검증 필요 |
| TW20 | `0x20FC:02`, UInt16 value 1, error/warning reset | source gate TRUE지만 최신 physical-effect proof 없음 |

초기 History 4의 `0x3204:13/14`, TW19 미구현, TW20 gate FALSE 판정은 더 오래된 snapshot이다. 후반 main history와 현재 source가 이를 대체한다. 현재 계약은 generic `0x3204` fallback을 허용하지 않는다.

### 3.3 공통 axis ownership과 복구 계약 확대

main history는 Home/maintenance가 축별로 서로 충돌하지 않고 restart와 safety preemption을 견디도록 공통 ownership을 크게 확장했다.

- InputLatch startup snapshot과 서로 다른 RT cycle의 full startup proof
- `OwnershipState`, startup/observer/lease/preempted/identity storage
- full identity validation과 `RequiredPhase` RESERVED/ACTIVE 검증
- Reserve/Commit/Rollback/Publish, one-level safety-preempt snapshot과 cleanup
- DS402 Home durable intent/WAL/receipt/retirement, bit-4 drain과 cleanup
- repeated Stop/no-resend, Stop 후 1회 PowerOff escalation
- TW19 뒤 successful LMC Home 전까지 motion을 막는 retained rebase barrier
- 대형 TCP/Control/Diagnostics methods 분할과 32 KiB size ratchet

여러 중간 C78 Rebuild와 search smoke가 기록돼 있지만 서로 다른 source checkpoint다. 특히 part 332~338의 C78 `0 errors / 50 warnings` 뒤에도 Control/Diagnostics/TCP 구현과 verifier가 크게 바뀌었다. 따라서 그 build를 최종 source의 build 증거로 재사용하면 안 된다.

### 3.4 과거 실기 관찰

후반 history에는 오래된 PLC checkpoint에서 다음 관찰이 있다.

- integer mask를 `|`에서 LASAL integer `OR`로 교정한 뒤 capability/topology가 회복됨
- 당시 deployment에서 `DiagnosticsBits=0x000C633F`, topology advertised, TW19/TW20 true, `BootId=0x14`
- 사용자가 TW19 동작을 보고함
- Axis1 LMC Home은 동작했으나 다음 축은 stale completed receipt/ownership과 cleanup 문제로 막힘
- Axis2 queried outcome은 `Quarantined`, `OriginalErrorId=-31000`, detail `38`, position delta `+1`

이 관찰 뒤 receipt interception, cancel/drain/cleanup, tolerance, ownership와 retained barrier source가 다시 변경됐다. 따라서 위 값과 동작은 최종 source의 실기 PASS가 아니다.

## 4. 2026-08-05 현재 확인한 작업트리 상태

### 4.1 Git과 IDE

- branch: `main`
- HEAD: `6537bcf`
- staged change: 0
- 기존 tracked/untracked 변경이 매우 큰 dirty worktree다. broad cleanup, reset, 일괄 stage를 하면 안 된다.
- `Lasal2.exe`: 실행 중 아님
- canonical 개발 대상은 `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`다.

### 4.2 최종 source hash와 static 계약

현재 source로 다섯 pre-IDE waiver를 명시한 다음 검증을 다시 실행해 PASS했다.

```powershell
& 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1' `
  -RepositoryRoot '.' -SourceOnly -ExpectedSdoWriteAxis 1 `
  -AllowPendingTcpSafetyHelperDeclaration `
  -AllowPendingControlSafetyRepeatHelperDeclaration `
  -AllowPendingTcpRpcLifecycleHelperDeclaration `
  -AllowPendingAxisRebaseRequiredStateDeclaration `
  -AllowPendingDiagnosticsMethodSplitDeclarations
```

확인 결과:

- `TCPMotionInterface`: 10 methods, SHA-256 `D1ECBA7249DE054B867829854FB11DEA2D19863F96D8559456968B8EB12B9E36`
- `LMCControlCommandService`: 26 methods, SHA-256 `C976CD364010EEFDFDDA8D7BC6D7655293DAD221FBEC908D50E5805CE4AFF072`
- `LMCDiagnosticsService`: 25 methods, SHA-256 `348E45AD486B4072D0105E7C0800B31BAF30A0B908F8AD2A5D2C26D3E46496E8`
- 전체 custom service: 6 classes, 93 methods, under-limit 86, 기존 baseline debt 7
- `SourceOnly Phase5TransportClean / IntegratedReadOwnerDormant`: PASS

이 PASS는 다섯 declaration waiver를 둔 source-only 계약이다. generated ABI, C78 build, PLC download와 runtime PASS가 아니다.

### 4.3 현재 source gate

현재 tracked `.st`에서 다시 확인한 값:

- `LMC_ADMIN_AXIS_HOME_ENABLED TRUE`
- `LMC_AXIS_REBASE_BARRIER_ENABLED TRUE`
- `LMC_DIAG_ENCODER_TW19_ENABLED TRUE`
- `LMC_DIAG_ENCODER_TW20_ENABLED TRUE`
- `LMC_DIAG_DS402_HOME_ENABLED FALSE`
- `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE`
- SDO Write: global TRUE, UI24 Axis1만 TRUE, Axis2~4 FALSE
- EtherCAT I/O Write: global/GT22BA FALSE

TRUE는 최신 source의 intended gate일 뿐이다. Section 17 ABI가 없고 최신 C78/download 증거도 없으므로 LMC Home/TW19/TW20을 현재 PLC에서 사용 가능하다고 판정하지 않는다.

## 5. 현재 정확한 재개점: Section 17

최종 blocker는 [IDE handoff Section 17](../../architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md#17-2026-08-04-one-visit-retained-barrier-and-eight-private-helper-ide-handoff)이다.

현재 `.st`에는 아래 helper implementation과 call이 존재하지만 `Classes.lcb`와 project `.lcb` generated metadata에서는 다음 아홉 이름이 모두 0건이다.

### `TCPMotionInterface`

- private `HandleControlSafetyDrainPending`
- private `HandleRpcLifecycleCommands`

### `LMCControlCommandService`

- hidden server channel `AxisRebaseRequiredState : SvrCh_UDINT`
- private `HandleAxisOwnershipSafetyRepeat`
- private `ReadAxisRebaseRequiredMask`
- private `UpdateAxisRebaseRequiredState`

### `LMCDiagnosticsService`

- private `HandleEncoderMaintenancePreemption`
- private `HandleAxisDs402HomeReceiptStages`
- private `HandleAxisDs402HomeCleanupStages`

`AxisRebaseRequiredState`는 현재 Network에서도 0건이다. 이는 의도된 상태다. 이 channel은 Comm Network에 연결하지 않고 hidden/file-retentive/non-visualized로 유지해야 한다.

## 6. 다음 작업의 exact 순서

1. Section 17 문서를 다시 읽고 canonical LASAL project만 연다.
2. 위 hidden channel 1개와 private function 8개를 exact name/type/order/property로 한 번의 IDE 방문에서 추가한다.
3. 여덟 function을 `GLOBAL` 또는 `VIRTUAL GLOBAL`로 만들지 않는다.
4. Network object와 connection을 추가·삭제하지 않는다.
5. Save All 한다.
6. **Rebuild하지 않고** LASAL IDE를 종료한다.
7. 외부에서 세 class generated declaration, `Classes.lcb`, channel property, Network non-change와 source hash를 검사한다.
8. 다섯 pre-IDE waiver를 모두 제거한 default `Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1`을 통과시킨다.
9. 그 뒤에만 별도 C78 Rebuild를 실행한다.
10. Rebuild 뒤 변경 class `Find in Implementation`과 smoke 시작 이후 `%TEMP%\Lasal2.log`의 새 `CInvalidArgException=0`을 확인한다.
11. 그 다음에만 cold download/restart와 새 nonzero BootId, capability, map/build identity를 고정한다.
12. 동일 BootId의 read-only health와 한 축 LMC Home exact terminal/owner release/다음 축 admission을 먼저 증명한다.

현재 단계에서 F9/Rebuild, PLC download, Home/TW write, Group/Reset/motion 시험을 먼저 실행하면 안 된다.

## 7. 실기 qualification 순서

Section 17, waiver-free static, C78, smoke와 cold download가 모두 통과한 뒤에도 다음 순서를 지킨다.

1. 새 동일 BootId에서 diagnostics build/capability/map/catalog 고정
2. EtherCAT master/4축 OP, AL=0, invalid cycle=0 확인
3. 축별 실제 `0x6061`, DS402 `0x6041`, `0x603F`, AxisError 확인
4. Warning/Fault/InternalLimit가 있으면 write 없이 원인 확인 후 중단
5. 축 1부터 한 축 LMC Home exact terminal success와 owner release 확인
6. 다음 축 Start가 stale detail `41` 없이 admission되는지 확인
7. 같은 BootId에서 축 1~4 단일축 기본 시험
8. 실제 non-Standstill 중 Stop, terminal polling과 3회 stable readback
9. Group 무이동 lifecycle 뒤 소거리 move와 실제 in-motion Group Stop
10. 실제 Fault가 있을 때만 Reset
11. TW19/TW20은 exact SDO completion과 physical effect를 분리해 마지막에 확인
12. CREVIS live DI/DO는 별도 qualification

ACK, capability refresh PASS와 Health RPC PASS는 완료나 physical effect가 아니다.

## 8. 계속 열려 있는 기술 경계

- latest source의 C78 build, download와 hardware evidence 없음
- `AxisRebaseRequiredState` encoded word와 restart/power-loss retention target proof 없음
- no-owner bit-4 automatic drain과 관련 gate atomic activation proof 없음
- `PublishAxisOwnership` Result를 소비하지 않는 production caller 11곳의 fail-closed 처리 미완료
- 일반 multi-axis publication/rollback의 crash-atomic 또는 cold-restart recovery 미증명
- post-C78 Reserve/Rollback/Publish/DS402 receipt 추가 size split은 설계만 존재하며 Section 17에 추가하면 안 됨
- 과거 전 축 `0x6041=0x02B3 Warning=1`, same-BootId 4축 qualification과 실제 in-motion Stop 미해결
- LMC Home Axis2 quarantine 후 수정본의 실축 연속성 미검증
- TW19 과거 동작 보고는 현재 source/BootId 재qualification이 아님
- TW20 protocol success와 실제 encoder effect의 current proof 없음

## 9. 보조 히스토리의 지속 결론

### 시험 항목

History 2의 즉시 ABI blocker는 후반 main history에서 바뀌었지만 시험 규율은 유지한다. `IDE/source -> C78 -> cold download identity -> read-only -> single-axis -> in-motion Stop -> Group -> Home/TW` 순서를 지키고 ACK를 완료로 보지 않는다.

### ActualPosition UNIT

History 3과 현재 source의 해석은 다음과 같다.

- ActualPosition은 encoder raw count가 아니라 PLC MotionLib의 application-unit DINT다.
- 현재 source 기준 `10000 DINT = 1 mm`, `1 DINT = 0.0001 mm`다.
- C#은 raw DINT를 그대로 반환하므로 caller가 `/ 10000.0`으로 mm를 해석한다.
- 실제 다운로드된 PLC의 UNIT 설정과 같다는 것은 download identity로 별도 확인해야 한다.

### 오래된 Home/TW 상태

History 4의 “Home/TW19/TW20 미구현·비활성”은 당시 PLC/source snapshot에는 맞았지만 현재 source 설명으로 쓰면 틀린다. 현재 source는 LMC Home/TW19/TW20 gate가 TRUE이고 구현 골격을 넘어선 상태지만, Section 17/C78/download/runtime gate 때문에 사용자 관점에서는 여전히 사용할 수 있다고 말할 수 없다.

## 10. 사용자 운영 결정

- implementation 중에는 architecture/IDE-handoff 문서를 코드와 함께 갱신한다.
- user/API/deployment manual, README와 HTML은 C78와 실기 동작이 안정될 때까지 반복 갱신하지 않고 기존 수정본을 보존한다.
- captured history의 최신 explicit IDE-control 시간은 평일 17:30부터 다음 날 08:00까지, 토·일요일과 대한민국 공휴일은 종일이다. 더 오래된 08:30 반복 문구보다 08:00 지시가 최신이다.
- 이 시간 허용은 LASAL IDE declaration/build/search에만 적용한다. PLC download, gate activation, 실축 motion/write 권한으로 확대하지 않는다.

## 11. 한 줄 재개 상태

현재 source-only 계약은 다섯 waiver로 PASS하지만 Section 17의 generated channel 1개와 private helper 8개가 없다. **다음 작업은 Section 17 exact declaration을 Save All한 뒤 Rebuild 없이 종료하고 외부 ABI 검사를 받는 것**이며, 그 전에는 C78, download와 실축 시험으로 넘어가지 않는다.

## 12. 2026-08-05 후속 재개 상태

이 절은 위 Section 4~6과 Section 11의 당시 재개점을 대체한다. 원래 기록은 chronology 보존을 위해
수정하지 않았다.

- Section 17 channel 1개와 private declaration 8개는 canonical project에 저장됐다.
- generated declaration, `Classes.lcb`, channel property와 실제 Network connection 0개를 확인했다.
- `Initialize=true` 때문에 Comm Network 생성물에 초기값 representation이 생기지만 connection은 아니다.
- 다섯 pre-IDE waiver 없는 `-SourceOnly -ExpectedSdoWriteAxis 1`은 PASS했다.
- 첫 C78 Rebuild는 `_memcmp` `UDINT -> DINT` 네 건으로 실패했고 비교 전용 `UDINT` local로 수정했다.
- verifier는 모든 DS402 Start `_memcmp` receiver가 `UDINT`인지 검사하며 두 회귀 fixture를 포함한다.
- 최종 정적 상태의 `LMCDiagnosticsService` SHA-256은
  `AEA62BEDE0F4121278DFC893071A5045A7B82A5C1755ABC44ABAB606829D9FA7`이다.
- 첫 로그의 55 warnings는 `W0069 35 + W0072 17 + W0073 3`이며 즉시 논리 결함 분류는 0건이다.

현재 exact 재개점은 **추가 source feature를 넣지 않고 C78 Rebuild를 다시 실행해
`0 errors / 55 warnings` 기준선을 닫는 것**이다. 그 뒤 `Find in Implementation`, 새
`CInvalidArgException=0`을 확인하기 전에는 Link/download와 PLC 시험으로 넘어가지 않는다.

## 13. 저장 후 정적 재확인과 ownership caller 감사

사용자 저장 뒤 `LMCDiagnosticsService.st` SHA-256은 Section 12의
`AEA62BEDE0F4121278DFC893071A5045A7B82A5C1755ABC44ABAB606829D9FA7`와 같았다. `_memcmp`
네 곳은 모두 비교 전용 `UDINT` local을 유지한다. 다섯 pre-IDE waiver 없는 전체 SourceOnly 계약과
AxisRebase barrier self-test `37/37`도 다시 PASS했다.

이 저장 뒤 `%TEMP%\Lasal2.log`의 최종 시각은 여전히 `2026-08-05 11:33:37`이며 새 C78 Rebuild
기록은 없다. 따라서 정적 저장 결과는 PASS지만 C78 `0 errors / 55 warnings` 기준선은 아직 미확인이다.

동시에 current `PublishAxisOwnership` production caller를 전수 감사했다. exact `21`곳 중 결과를
분기에서 소비하는 곳은 `10`, local에 대입만 하고 검사하지 않는 곳은 `11`이다. 미소비 caller는
TCP 7곳, Control 1곳, Diagnostics 3곳이며 모두 `QUARANTINE` publication이다. 이 11곳에서 유일한
완료 결과는 `0`이고, 특히 `-2`는 retained owner를 남길 수 있으므로 tuple clear나 terminal response로
진행하면 안 된다. exact matrix와 caller-level verifier 계획은
[performance/OOP design Section 8.5.1](../../architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md#851-production-caller-result-consumption-matrix)에 기록했다.

caller fix는 provider size split과 별도 semantic tranche다. Section 17 C78와 implementation smoke가
닫히기 전에는 어느 쪽도 source에 적용하지 않는다.

pre-C78에서 가능한 test-only 선행 작업으로 caller inventory ratchet을 추가했다. current source의
exact `21 total / 10 Result-consumed / 11 Result-unconsumed OPEN`과 모든 result receiver `DINT`를
고정하며, 기존 publish focused self-test는 provider `69/69`와 caller inventory `8/8` negative
fixture를 거부했다. 전체 `-SourceOnly -ExpectedSdoWriteAxis 1`도 PASS했다.

이 PASS는 11곳의 fail-closed 처리가 끝났다는 뜻이 아니다. syntactic def-use baseline만 고정한 것이며,
실제 caller fix 때는 result domain, clear/send/stage 전 check dominance와 retained tuple recovery를
별도 semantic fixture로 함께 추가해야 한다. LASAL build input hash는 이 작업에서 변하지 않았다.

## 14. 2026-08-05 C78, download와 BootId `0x1B` runtime 후속 상태

이 절은 Section 12의 C78 대기 상태와 Section 3.4의 오래된 Home 실패 관찰을 현재 배포 증거로
대체한다. 상세 근거는 [BootId 0x1B runtime evidence](04_runtime_evidence_boot_1b_home_group.md)에
보존했다.

- 14:24 C78/ARM Rebuild는 `0 errors / 55 warnings`로 끝났다. warning histogram은
  `W0069=35`, `W0072=17`, `W0073=3`이다.
- 14:22와 14:24 download는 `Timeout waiting CPU state`로 실패했고, 14:26 canonical download와
  PLC link는 성공했다.
- 새 runtime identity는 `BootId=0x1B`, `MapRevision=0x957F101E`, `DiagnosticsBuild=1`,
  `DiagnosticsBits=0x000C633F`, `AdminFeatures=0x17`이다.
- 같은 BootId에서 Axis1..4 LMC Home이 모두 exact terminal `Succeeded`, `HomeSucceeded=True`,
  `AxisError=0`, 좌표 6개 `0`, evidence `0x3B`, retirement PASS로 끝났다.
- generation `1 -> 4`, 다음 축 admission과 `Identity Home Check PASS: 4/4`가 확인되어 이전
  detail `41` stale receipt/owner-release blocker는 이 checkpoint에서 해소됐다.
- Group Power/Set Identity/Enable과 실제 non-Standstill 왕복 이동은 PASS했다.
- 모든 Group move가 자연 완료됐으므로 실제 in-motion Group Stop은 아직 미검증이다.
- 14:19 이후 `CInvalidArgException`은 0건이지만 `Searching implementation` 기록도 0건이다. 따라서
  required three-class implementation smoke는 여전히 별도 gate다.

현재 다음 source semantic tranche는 Section 13의 `PublishAxisOwnership` Result 미소비 11곳을
fail-closed하는 것이다. DS402 Home gate와 ordinary ownership gate는 계속 `FALSE`로 유지한다.

## 15. ownership caller 단일 rollback authority 정정

Section 13의 `21/10/11`은 당시 source inventory로는 정확하지만, 후속 full-nesting 감사에서 TCP
`MsgPaser`의 마지막 두 OPEN caller는 Result 소비를 추가할 정상 caller가 아니라 Control terminal
response를 다시 rollback하는 이중 finalizer로 확인됐다.

- `LMCControlCommandService.HandleRequest`는 exact failure를 반환하기 전에 이미 rollback하고 success는
  commit한다.
- safety-drain pending `Result=1`은 `ownershipSafetyPumpRejected=TRUE`라 일반 finalizer에 진입하지
  않고 tuple을 보존하며 TCP RETAIN continuation으로 이어진다.
- 따라서 Control이 request당 유일한 commit/rollback authority다.
- TCP의 exact-failure/malformed rollback과 그 실패 publication 2곳은 제거 대상이다. malformed
  response는 ownership mutation 없이 deterministic 24-byte detail `42`로만 정규화한다.
- 제거 뒤 generic production inventory는 `19`, 기존 consumed `10`, 실제 caller-fix 대상 OPEN은 `9`,
  최종 계약은 `19/19/0`이다. TCP restart-only publish-failure latch 대상도 `7`에서 `5`로 줄어든다.

LASAL IDE가 열린 동안 production `.st`는 외부 수정하지 않는다. 현재는 Section 8.5.2 문서와 synthetic
target verifier를 위 단일 authority 기준으로 먼저 교정하고, IDE 종료 후 같은 tranche에서 TCP source,
Control/Diagnostics/TCP Result 소비, default production verifier 전환을 함께 적용한다.

## 16. Test5 Group Stop, 안전 종료와 축 상태 증거

이 절의 후속 실기 증거는 Section 14의 당시 결론인 "actual in-motion Group Stop 미검증"을 대체한다.

상세 packet 근거는
[Test5 runtime evidence](05_runtime_evidence_group_stop_safe_shutdown_and_axis_status.md)에 보존했다.

- 같은 `BootId=0x1B`, `MapRevision=0x957F101E`에서 실제 non-standstill Group Stop 3회가
  각각 `0x2085` 한 번만 송신된 뒤 status-only `0x2045`로 `0x40060000` 3연속에 도달했다.
- 독립 `Stable=3_3` pcap도 정확히 세 번의 `0x2045`와 세 응답 `0x40060000`을 기록했다.
- Group Disable 뒤 상태는 `0x40050000`, Power Off 뒤 상태는 `0x40010000`으로 각각 3회
  연속 확인됐다. initial Power Off 뒤 Power On/Off 3회가 이어졌고 세 번째 Off가 최종 상태다.
  모든 mutation은 replay 없이 통과했다.
- 네 pcap 모두 retransmission, fast retransmission, lost/out-of-order segment, duplicate ACK와
  TCP RST가 0건이다.
- 축별 새 캡처는 Axis1..4의 `0x2028`이 모두 function/error/AxisError `0`으로 성공했음을
  증명한다. 그러나 `0x7E50/0x7E03`이 한 건도 없어 실제 DS402 `0x6041`, `0x6061`, `0x603F`는
  아직 읽지 않았다. 다음 실기 항목은 `Read Drive Status`와 `Get Drive Error Code`다.

이 runtime PASS는 현재 진행 중인 `PublishAxisOwnership` caller fail-closed source tranche를
대체하지 않는다. ordinary ownership과 DS402 Home gate는 계속 dormant로 유지한다.

## 17. ownership publication fail-closed source와 verifier 전환 완료

LASAL IDE가 종료된 상태에서 Section 15와 architecture Section 8.5.2의 source tranche를 canonical
project에 적용했다. 이 절은 Section 13의 historical `21/10/11 OPEN` 상태와 Section 15의 적용 전
계획을 현재 static checkpoint로 대체한다.

- `TCPMotionInterface`는 generic publication caller 5곳의 nonzero Result를 request clear와 wire
  response보다 먼저 `ActiveRequest.Reserved=2`로 arm한다. `CyWork`는 phase `2 -> 3` evidence/close
  claim을 한 번만 수행하고 background pumps는 계속 진행하며 transport dequeue는 차단한다.
- `Response`와 `ConnSocketInfo`는 phase 2/3 delayed callback, disconnect clear와 새 candidate takeover를
  차단한다. ordinary disconnect의 stale discard state는 다음 accepted connection의 existing reset에서
  정리한다.
- Control은 request당 유일한 commit/rollback authority다. TCP의 중복 rollback/publication 두 곳은
  제거했고 malformed Control response만 ownership mutation 없이 24-byte detail `42`로 정규화한다.
- Diagnostics generic publication은 성공 domain `{0}`, preemption cleanup 4곳은 exact replay를 포함한
  `{0,1}`을 성공으로 처리한다. 허용 domain 밖 Result는 Encoder `[190]/[191]` 또는 DS402
  `[119]/[118]="PBF1"` evidence를 terminal stage보다 먼저 남긴다.

현재 generic caller inventory는 TCP `5`, Control `7`, Diagnostics `7`, 합계 `19`다. production
contract는 `19 total / 19 Result-consumed / 0 OPEN`이고, preemption-cleanup caller는 4곳이다.
legacy `21/10/11`은 synthetic regression fixture에서만 유지한다.

검증 결과:

- focused publish verifier: provider negative `69/69`, legacy inventory negative `8/8`, target caller
  negative `47/47`, current production `19/19/0`, exit `0`;
- full `-SourceOnly -ExpectedSdoWriteAxis 1`: target `19/19/0`, preemption caller `4`,
  `PASS LASAL.StaticContract.SourceOnly`, exit `0`;
- PC library/test Debug build와 `RunPcTests`: `1082/1082 PASS`, exit `0`;
- custom method-size budget: `93` methods, `86` under limit, existing baseline debt `7`, PASS. 변경된
  `LMCControlCommandService.HandleRequest`는 all-CRLF `32672` bytes로 32 KiB 미만이며 margin은
  `96` bytes다;
- 전체 tracked/untracked 관련 diff check와 verifier diff check는 whitespace error 없이 PASS했다.

현재 source SHA-256:

| Source | SHA-256 |
|---|---|
| `TCPMotionInterface.st` | `98EE4A57A6E8EAE3AE6606F1DB892D30EA50B62D79C230AEA8B9FADD6348046F` |
| `LMCControlCommandService.st` | `D044E29218255E5859FACB1831B5B33E6E3EAEF34AB9758E4B1EDDA9CEF6CF5E` |
| `LMCDiagnosticsService.st` | `097462D1751E4A6ED7466827B8F6E3EB2C2914D5EE0409AED3CD43BD1404FB54` |
| `Verify-LasalContract.ps1` | `514A4B28CD94BD687EC45A246D50ACABEE64E69484347BB0092215385408293D` |

이 checkpoint는 static source 완료다. 아직 새 source의 C78 Rebuild, canonical project Save All,
세 class `Find in Implementation` smoke와 smoke 시작 뒤 `%TEMP%\Lasal2.log`의 새
`CInvalidArgException=0`, PLC download/restart는 수행하지 않았다. 따라서 이번 PC `RunPcTests`와
기존 PLC `BootId=0x1B` runtime은 이 source tranche가 배포됐다는 증거가 아니다. ordinary ownership과
DS402 Home gate도 계속 dormant다.

## 18. AxisRebase retained barrier 감사와 post-C78 순서

현재 `AxisRebaseRequiredState`는 source/generated/static 수준에서 구현 완료다. hidden
`SvrCh_UDINT`의 `Initialize=true`, `DefValue=16#5242530F`, `WriteProtected=false`,
`Retentive=File`, `Visualized=false` 속성, private codec 두 개, Network endpoint 무연결,
TW19 pre-dispatch arm, exact Home terminal-success clear와 persistence retry가 canonical project에
존재한다. focused verifier self-test는 현재 worktree에서 `37/37` negative fixture reject로 PASS했고,
custom method-size ratchet self-test도 `5/5` PASS했다.

이 정적 PASS는 target file flush 또는 restart/power-loss durability 증거가 아니다. WPF는 TW19 terminal
retirement 뒤 maintenance no-replay journal을 `Resolved`로 닫고 Home 필요 경고만 표시하며 별도 rebase
journal/interlock을 유지하지 않는다. 다른 client가 Home을 수행했을 때 동기화할 public read API도 없으므로
WPF에 별도 shadow interlock을 추가하지 않는다. 현재 설계에서는 PLC retained barrier가 유일한 authority다.

target retention qualification의 exact expected sequence는 다음과 같다.

| 단계 | effective mask | encoded word |
|---|---:|---:|
| 초기 | `0xF` | `0x5242530F` |
| Axis1 exact Home 뒤 | `0xE` | `0x5242531E` |
| Axis2 exact Home 뒤 | `0xC` | `0x5242533C` |
| Axis3 exact Home 뒤 | `0x8` | `0x52425378` |
| Axis4 exact Home 뒤 | `0x0` | `0x524253F0` |
| empty에서 Axis2 TW19 arm | `0x2` | `0x524253D2` |
| warm restart 및 실제 power-loss 뒤 | `0x2` | `0x524253D2` |
| Axis2 exact Home 뒤 | `0x0` | `0x524253F0` |

각 단계에서 다른 축 bit 보존, invalid magic/complement의 fail-closed effective `0xF`, blocked
PowerOn/motion의 native/SDO call 0, failed/quarantined TW19 또는 Home 뒤 bit 유지도 함께 증명해야 한다.
BootId `0x1B` Home/group capture는 이 retention gate를 닫지 않는다.

새 production source tranche는 현재 ownership fail-closed source의 Save All, fresh C78, 세 class
implementation smoke가 끝나기 전에 시작하지 않는다. 그 기준선 뒤 첫 별도 tranche는 설계 Section 8.2의
`PublishAxisOwnershipPreemptionCleanup` read-only validator split이다. 현재 public method debt는
raw/LF/all-CRLF `37128/36143/37129` bytes이고, 현재 whole Control source SHA-256은
`D044E29218255E5859FACB1831B5B33E6E3EAEF34AB9758E4B1EDDA9CEF6CF5E`다. Save All이 EOL 또는 hash를
바꿀 수 있으므로 helper insertion plan과 reverse-inline hash는 C78 기준선 입력을 확정한 뒤 다시 계산한다.

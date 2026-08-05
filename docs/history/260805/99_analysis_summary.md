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

# Elmo Master 260806 continuation summary

작성일: 2026-08-06 (KST)

## 1. 문서 목적과 증거 경계

이 문서는 `PublishAxisOwnershipPreemptionCleanup`과 후속
`PublishAxisOwnershipDs402Receipt` method-size split의 current continuation point를 기록한다.
아래의 source/hash/size는 2026-08-06 current worktree에서 재확인한 결과다.

이 기록은 source, generated artifact, C78 build, IDE smoke와 PC regression의 현재 상태를 증명한다.
PLC download·restart·실축 runtime은 아직 최종 증거가 없다.

## 2. 적용된 source split

`LMCControlCommandService` private function을 다음 exact ABI로 추가했다.

```text
ValidateAxisOwnershipPreemptionReplacement
  PreemptedAdmissionToken : UDINT
  PreemptedOwnerGeneration : UDINT
  OldCommand : DINT
  OldOwnerKind : DINT
  OldResourceKind : DINT
  OldIdentitySize : UDINT
  Result : BOOL
```

- declaration과 implementation에 `GLOBAL`/`VIRTUAL GLOBAL`이 없다.
- parent `PublishAxisOwnershipPreemptionCleanup`의 replacement validation 경로에 helper 호출이
  단 한 번 존재한다.
- helper는 public input과 parent가 이미 검증·표본한 old tuple만 by-value로 받는다.
- `OwnershipState`와 `OwnershipIdentityState`를 읽지만 persistent state를 쓰지 않는다.

다음 15개 local을 parent에서 helper로 이동했다.

`probeAxisIndex`, `probeAxisBit`, `replacementRecordBase`, `replacementHeaderBase`,
`singletonToken`, `singletonGeneration`, `singletonMask`, `replacementIdentitySize`,
`replacementTailSize`, `replacementTailOffset`, `replacementPackedCommand`,
`replacementPackedOwner`, `replacementFound`, `replacementValid`, `replacementStateValid`

pre-split의 `singletonToken := 0;`부터 `tupleValid := replacementValid;`까지 7,678-byte
block을 helper로 옮겼다. 이 block의 SHA-256은
`95D07F6EDEC47747606F4A2DEBDF2AF240C2F872EA638C2B176B9203335C053E`다.

source-level reverse-inline 검토에서 parent의 persistent mutation 26개와 publication 순서,
root-magic clear/commit-last, public Result 의미는 변하지 않았다. current parent Result
assignment histogram은 `-1 x1`, `-2 x5`, `-3 x8`, `0 x1`, `1 x1`다. 이 결과는
source 비교이며 split-aware semantic verifier 최종 PASS를 대체하지 않는다.

## 3. hash chain과 Network 불변

| 단계/파일 | SHA-256 |
|---|---|
| pre-IDE `LMCControlCommandService.st` | `D044E29218255E5859FACB1831B5B33E6E3EAEF34AB9758E4B1EDDA9CEF6CF5E` |
| IDE declaration 저장 후, implementation 적용 전 source | `2F690EA15DEC5F5F3C93DE8A36D10AA47DEB70942CC4F97BDEC9D0EA184B7BA2` |
| current implementation source | `3BCA660E4569E8EA6222CD81EA683BF7D9BD2A007AB2464162DF5673FDB3EEBE` |
| stray token 복구 후 current `TCPMotionInterface.st` | `9210C199A02153FEE4110556C5396CD49C9AEAC7F22B1405AA20F00FC522A129` |
| final C78 Rebuild 후 `Class/Classes.lcb` | `D82728DC9C2AC703BF7461E14709C98082A7F3436555A8DB58924D36149E1EDE` |
| current `Comm_Network.lcn` | `55284463115C04B3EDFA380C0CF3766F652C6E3D944F9582E692963C0575516B` |

`Comm_Network.lcn`은 pre-split hash와 같다. 따라서 이 helper 추가로 Network 연결이
변경되지 않았다.

## 4. method-size 실측

`Verify-LasalCustomMethodSizeBudget.ps1`의 FUNCTION block 파싱과 byte 정규화 규칙으로
current source를 실측했다.

| Method | raw | LF | all-CRLF | gate |
|---|---:|---:|---:|---|
| `PublishAxisOwnershipPreemptionCleanup` | 29277 | 28500 | 29301 | `<32768` |
| `ValidateAxisOwnershipPreemptionReplacement` | 7933 | 7933 | 8142 | `<32768` |

current six custom classes inventory는 methods `94`, under-limit `88`, baseline debt `6`이다.
기존 cleanup parent는 32 KiB debt에서 벗어났고 parent/helper 모두 일반 hard gate
대상이 됐다.

## 5. 닫힌 gate와 남은 runtime gate

2026-08-06 current source에서 다음 gate를 닫았다.

1. generated declaration/`Classes.lcb` exact private ABI와 method registration: full static PASS
2. split-aware helper negative fixture: `38/38` PASS, method-size self-test `5/5` PASS,
   methods `94`/under-limit `88`/baseline debt `6`
3. post-build SourceOnly/full static: `ExpectedSdoWriteAxis=1`로 모두 PASS
4. 첫 C78 Rebuild: `TCPMotionInterface.st` line 298의 stray `U`/`UDINT`로 `E0016` 1건을 정확히 검출
5. stray 두 줄 제거와 prefix guard 추가: ownership activation negative fixture `287/287` PASS
6. 두 번째 canonical Rebuild: 2026-08-06 17:16 KST, C78/ARM, `0 errors / 55 coded warnings`,
   Linker Done; `W0069=35`, `W0072=17`, `W0073=3`, compatibility warning 6줄
7. compile count: `TCPMotionInterface`, `LMCControlCommandService`, `LMCDiagnosticsService` 각각 1회;
   Download/Connect 0건
8. implementation smoke: 세 class의 `Open Implementation Editor` 3건,
   smoke 시작 이후 `CInvalidArgException=0`
9. PC regression: SDK Debug `1082/1082`, WPF Debug Rebuild 성공과 smoke `330/330`

남은 gate는 current PLC download, restart 후 reconstruction/retained-state recovery와 실축 runtime
확인이다. 위 PASS는 runtime 완료 증거가 아니다.

## 6. 후속 DS402 receipt Stage-87 split

같은 날 후속 단계에서 `PublishAxisOwnershipDs402Receipt`의 Stage-87 tokenless always-return branch를
다음 private helper로 분리했다.

```text
HandleAxisOwnershipDs402ReceiptStage87Recovery
  pState : ^USINT
  activeIndex : DINT
  AxisMask : UDINT
  ReportKind : UINT
  ReportValue0 : UDINT
  ReportValue1 : UDINT
  ObservationCycle : UDINT
  Result : DINT
```

IDE Save All과 external inspection에서 declaration/implementation 모두 private이고 `Classes.lcb`
flags `0x00000000`, input `7`, output `Result : DINT`를 확인했다. post-IDE/pre-split source는
`606170` bytes, SHA-256
`BAB60FF1891F424B132C52EF3FBF5D099AB010BFF1D7E812648DFA7BF619BE7A`다. implementation split 뒤
current source는 `606348` bytes, SHA-256
`DA93EB01DBF7E842C36EE22E1ACBF6277D60C0E12C58B93A24BA870976321FCF`다.

원 branch 내부 588줄은 한 tab deindent 외 byte-exact 이동했고 reverse-inline은 pre-split source를
byte-exact 복원한다. current adapter/helper 실측은 각각 `21836/21279/21837`,
`26182/25531/26183` raw/LF/all-CRLF bytes다. adapter/helper local은 `35/42`, persistent mutation은
`28/49`, transitive `77`개다.

검증 결과는 다음과 같다.

1. focused split-aware negative fixture `67/67` PASS
2. method-size self-test `6/6` PASS
3. six classes methods/under-limit/debt `95/90/5`
4. waiver 없는 `-SourceOnly -ExpectedSdoWriteAxis 1` exit `0` PASS
5. `Classes.lcb`와 Network 3개 hash 불변 확인
6. 독립 reverse-inline/diff review actionable finding 없음

증거 폴더는 `test/Reports_Lasal/C78_20260806_ds402_receipt_split/`이다. 2026-08-07 C78/ARM Rebuild는
`26318.1 ms`에 성공했고 IDE 집계는 `0 errors / 55 warnings`다. 전체 build log는 source warning
`55`개와 C78/C81 version warning `6`개, ERROR/FATAL `0`개다. 실제
`Comm_Network.LMCControlCommandService1.LMCAxis1`의 `Find in Implementation`은 `29` hits,
`1` matched file / `3` searched files로 성공했고 post-smoke `CInvalidArgException=0`이다. Save All 뒤
current `Classes.lcb` SHA-256은
`9147D2185860FE2082777013FC944248196B686402FE88F7EF52FAB9875301E0`이며 post-save SourceOnly도
exit `0`으로 다시 통과했다. IDE는 종료했고 Download는 하지 않았다. 따라서 Section 8.4 rollback
split은 current `DA93EB01...` source에서 재기준화할 수 있으며 PLC download/runtime만 별도 gate로
남는다.

## 7. 2026-08-07 RollbackAxisOwnership post-IDE implementation checkpoint

초기 DA93 read-only 계획 gate 뒤 exact private helper declaration을 LASAL IDE로 저장했다. 그 결과
post-IDE/pre-implementation Control은 `606820` bytes, SHA-256
`DAA8E134CE6E67BA47D6B30530F0FB9DBEF041A1B355466472872975897C3DF0`이 됐다. 같은 시점
`Classes.lcb`는 `8429648` bytes, SHA-256
`2AEFD0B004B9F0CE1688077FC5B842AB46B893C811A8951DF2E7F8CDF23406A5`다. helper는 source와
generated metadata에 private로 존재하고 implementation은 empty stub였다. Network는 바뀌지 않았다.

DAA8 monolithic `RollbackAxisOwnership` baseline은 line `5032..6337`, byte0 `[180762,230865)`,
raw/LF/all-CRLF `50103/48798/50104`, SHA-256
`2A88838417913B76449739447AAA8175157EAF8A370CC53F7FF916A3F25FF745`다. 안전한 extraction은 두 번째
`preemptBankValid := TRUE;`인 line `5375..5879`, byte0 `[192424,212796)`, raw/LF/all-CRLF
`20372/19867/20372`, SHA-256
`9A6EFE09CBE17D062802245E06974BF80AA7268D95489DEB8C137A0E1F68A62C`다. 바깥
`if restorePreempt then`/`end_if` line `5374`/`5880`은 adapter에 남겼다. extraction inventory는
`_memcmp=3`, `TO_UDINT=9`, persistent write/`_memset`/`_memcpy=0`이다.

DAA8에서 exact candidate를 다시 계산하고 reverse-inline으로 DAA8 byte-exact 복원을 증명한 뒤
implementation을 적용했다. current Control의 IDE CRLF checkpoint는 `608436` bytes, SHA-256
`A51E716363E8DB38E7BE6D849BC2C29D4FE7B51E801D5704BA7F95D73CCC8753`이고 Git canonical LF는
`591670` bytes, SHA-256
`7EAB9F0E71A85C1459FD01A381859D9EC5095949D536E78B056A67BE91C2D1BE`다. planned whole source는
두 projection 모두 exact다.

- adapter canonical LF/all-CRLF `29124/29922`, canonical LF SHA-256
  `8855AEEAE9B617CEAC1D10C7CC4ADB7F4D0536D108592560CE0D39ACF344AFAC`
- helper canonical LF/all-CRLF `21451/22046`, canonical LF SHA-256
  `AE6AD76007725544FBC57D8D60DF5C483CD3381149A1D14C424C96BCBEE0AF09`
- call map canonical LF/all-CRLF `758/776`, canonical LF SHA-256
  `66E328773321E978F63BF13F3080E77193D27D69E704081A7205D366EC76FF55`
- actual generated declaration canonical LF/all-CRLF `207/216`, canonical LF SHA-256
  `4BC23CE3F6FAC1F2E18CBC5D2AF7E2C27111834B8064E322AB5C6E66D0FD44E4`

helper는 NIL, exact 40-byte size와 mask 범위를 검사하고 full validation 성공 뒤에만 10개 UDINT context
slot을 게시한다. helper nonzero는 public output에 직접 흘리지 않고 adapter의 기존 위치에서
`Result := -3; RETURN;`으로 변환한다. helper persistent write는 `0`이고 adapter persistent write `79`,
public Result assignment `15`, `RETURN` `14`는 유지됐다.

현재 검증 결과는 다음과 같다.

1. current adapter/helper split verifier `20/20` expected semantic rejection PASS
2. ownership aggregate `287/287` PASS
3. method inventory methods/under-limit/debt `96/92/4` PASS
4. waiver 없는 `Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1` exit `0` PASS
5. pre-Rebuild `Classes.lcb`와 Network 세 파일 hash 불변

DAA8 one-shot planner `18/18`은 construction/reverse proof이고 current A51E의 CRLF와 fresh-checkout
LF 입력에서 모두 통과한다. A51E current `20/20`은 integrated adapter/helper composite fence다.
서로 다른 증거 층이다. 세부 planner와 manifest는
`test/Reports_Lasal/C78_20260807_rollback_split_rebaseline/`에 있다.

A51E C78/ARM Rebuild는 2026-08-07 11:39 KST canonical 단일 세션에서 성공했다. captured input 8개는
byte-exact이고 rebuild command window는 compiler error `0`, coded warning `55`개(`W0069=35`,
`W0072=17`, `W0073=3`), result 뒤 version compatibility warning `6`개, 필수 custom ST 6개 각 1회
compile, Linker `Done`, command success다. append 전체의 `CInvalidArgException`과 download/online
command는 `0`이다.

post-C78 `Classes.lcb`는 `8430171` bytes, SHA-256
`3B5D814F566F20D49D8033CC6E6F735A1503D91B7A3D5F87D3E6339FECC3421B`다. C78이 detailed ABI record
외에 compact symbol entry를 추가해 helper 이름은 두 번 나오지만, exact private three-input ABI record는
한 개이고 565-byte SHA-256
`094573D70AC34005F1072D5FE88D705CD2D63BD8F4B3A16068228D97EFB4F337`는 유지됐다. verifier의
whole-binary 이름 유일성 오탐을 method-header-qualified record 유일성으로 교정한 뒤 full static은 다시
exit `0`으로 PASS했다. current verifier SHA-256은
`D9C4AD42C27EFA8C40284623B28CDAE3C816AB9A72EFF25548C7E6102E1B3670`다.

별도 GUI Build Output transcript와 두 exact implementation search 증거는 캡처하지 못했다. 따라서 strict
dual-evidence와 changed-class smoke는 pending이고 download/restart와 PLC/실축 runtime도 수행하지
않았다. 이 split은 method-size debt만 줄였고 durable power-loss recovery journal을 추가하지 않았다.

Recorder는 2026-08-07 사용자 결정으로 추가 개발을 중단했다. 기존 기능과 시험 자산은 그대로 두며,
SIGMATEK과 네트워크/EtherCAT 데이터 경로를 협의하고 사용자가 명시적으로 재개하기 전에는 Recorder
코드, size-debt 분할, 새 qualification을 진행하지 않는다. 재개 시 설계 문서를 먼저 갱신한다.

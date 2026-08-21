# Elmo Master 2026-08-19 historical continuation summary

작성일: 2026-08-19 (KST)

## 문서 성격

이 문서는 `Elmo_Master_history_260819.md`를 현재 쓰레드에서 이어 읽기 위한 재개
요약이다. 히스토리에 기록된 당시 결론과 2026-08-19 현재 저장소에서 다시 확인한
사실을 구분한다. PC/static/IDE 증거를 PLC/runtime/hardware 증거로 승격하지 않는다.

- 원본: [`../Elmo_Master_history_260819.md`](../Elmo_Master_history_260819.md)
- 원본 크기: 185,764,439 bytes / 53,925 lines
- 원본 SHA-256:
  `127D66642A40E57C8F0908083303C845B781282A7AFE8AAE23BFC4751928DC6D`
- 분할본: 250 source lines 기준 216개, 총 2,515,662 bytes
- 판독 범위: part 001-072, 073-144, 145-216의 세 구간으로 나눠 216개를 전수 판독
- 대형 줄 축약: 4,000자를 넘는 410줄을 분할본에서만 placeholder로 교체
  - image/base64 계열 355줄, oversized tool output 55줄
  - 100,000자를 넘는 실제 대형 payload 174줄
  - 각 placeholder와
    [`01_omitted_payload_manifest.csv`](./01_omitted_payload_manifest.csv)에 원문 줄번호,
    문자 수와 decoded-text SHA-256을 보존했다.
- 원본은 수정하지 않았다. 완전한 기록은 원본이 System of Record다.

분할 범위와 각 파일 링크는 [`00_index.md`](./00_index.md)를 사용한다.

## 히스토리 진행 요약

| Source lines | Split parts | 진행 내용 | 당시 최종 판정 |
|---:|---:|---|---|
| 1-497 | 001-002 | 260813 인계, reconnect V2, Close/Power 차단 원인 확인 | 네 기능의 개별 PLC 고장이 아니라 stale `OutcomeUnverified` diagnostics journal의 공통 WPF interlock이었다. |
| 498-4564 | 002-019 | recovery quarantine/retirement, Close/X 복구, 현장 Servo 시험 준비 | BootId 불일치 stale record를 보존·격리하고 Close/X를 허용했다. Recorder는 범위에서 제외했다. Servo On은 운영자 retirement 전까지 안전상 차단했다. |
| 4565-9205 | 019-037 | 접이식 recovery UI, TW19 기본값, stale journal retirement, TW19 실기 관찰 | `0x20FC:01` terminal success와 후속 LMC Home은 확인됐지만 실제 absolute multi-turn 물리 효과는 증명하지 못했다. |
| 9206-13887 | 037-056 | SetPosition query/retire와 retained-store 설계 | wire/PC 계약과 fail-closed scaffold를 만들었고 capability/native는 OFF로 유지했다. |
| 13888-25510 | 056-103 | LASAL IDE에서 `LMCSetPositionStore`, CheckSum, Comm Network, methods 생성 | C78 build와 구조 연결을 확인했지만 SRAMRETAIN 실제 target allocation과 PLC download는 하지 않았다. |
| 25511-34539 | 103-139 | retained Store lifecycle 및 Control `0x7D12/14/1A` 구현 | Store/Control source와 정적 검증은 완료했지만 macro `FALSE`, capability bits OFF, native call 0이었다. |
| 34540-35622 | 139-143 | UDP Gate D rebaseline, SourceOnly blocker 분석 | Gate D static approval은 통과했고 `HandleAdminCommands` 32 KiB 초과가 다음 blocker였다. |
| 35623-46825 | 143-188 | private handler/dispatcher 분리, coordinate/ownership/rollback dormant slice | Full SourceOnly와 C78를 닫았지만 activation과 PLC download는 계속 금지했다. |
| 46826-52551 | 188-211 | RT task 감사와 `LMCEcatInputLatch` P0 preflight 선언 | Axis1 trigger 기반 RT 후보만 확인돼 native 실행을 넣지 않고 observation-only preflight로 범위를 축소했다. |
| 52552-53754 | 211-216 | P0 구현·검증·문서화와 5개 목적별 commit | frozen 16/32 ABI, `95/95`, C78/Link, current Gate D/Main SourceOnly를 기록했다. 기능은 activation OFF였다. |
| 53755-53925 | 216 | cache/test/history 정리와 임시 LASAL 파일 삭제 | cleanup commits를 만들고 untracked `TestClass`와 `.etf`를 삭제했다. |

## 핵심 결론

### 1. WPF recovery와 TW19

- Close/X, Axis Power On, Group Power On이 함께 막힌 직접 원인은 PLC command failure가
  아니라 동일 diagnostics recovery interlock이었다. 차단 시 PLC command는 전송되지 않았다.
- stale record는 삭제하지 않고 immutable retirement ledger로 보존한 뒤 read-only
  quarantine/retirement 흐름으로 복구했다.
- TW19 기본 operation은 `Tw19MultiturnPositionReset`으로 변경됐고, 히스토리에는
  `RequestId=36`, terminal `Succeeded`, `SdoAbort=0`, `VerificationFlags=0x3FF`가 기록돼 있다.
- 이 terminal은 exact SDO completion과 cleanup 증거다. raw position `6028554`는 그대로였고
  LMC Home이 application position만 0으로 만들었다. 따라서 실제 encoder multi-turn 물리
  초기화 효과는 아직 open evidence item이다.

### 2. SetPosition은 production 기능이 아니라 dormant P0다

현재 설계 기준은
[`AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md`](../../architecture/AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md)다.

- `LMCSetPositionStore`에는 1,344-byte retained ledger와 Begin/Commit/Read/Retire source가 있다.
- 최종 cross-boot wire 길이는 `0x7D14` 60 bytes, `0x7D1A` 64 bytes다. 초기 56/60-byte
  설계는 original/current BootId 분리 뒤 폐기됐다.
- Control에는 exact route/parser, private `HandleAdminSetPosition`, coordinate/new-Armed
  ownership 경계가 있다.
- `LMCEcatInputLatch`에는 16-DINT request mailbox, 32-DINT result, Submit/Copy/private RT
  preflight와 `RtWork` 호출이 있다.
- `READY`는 한 RT sample의 coherent preflight snapshot일 뿐 SetPosition 성공, native 접수,
  durable terminal 또는 hardware 적용 증거가 아니다.

다음 안전값은 계속 고정돼 있다.

- `LMC_ADMIN_SET_POSITION_STORE_CONFIGURED=FALSE`
- `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED=FALSE`
- axis 1..4 max-jump `0`
- Admin capability `0x00000017`; bits 3/5/7 OFF
- Admin SetPosition 경로의 claim/native count/state `0`
- Admin SetPosition 경로의 native `_LMCAxis.SetPosition()` call site `0`

internal `-12`는 미래의 native 가능성 이후 terminal durability가 불확실할 때 wire response 없이
connection을 닫기 위한 dormant sentinel이다. 현재 producer는 0회이며, future async poll용 `-13`과
혼용하면 안 된다.

`LMCEcatInputLatch`에 존재하는 기존 `.SetPosition()` 호출은 ZeroHome/Home용이다. 이를 새
Admin SetPosition executor의 native call로 세면 안 된다.

### 3. Callback/EventMask는 별도 미완료 트랙이다

- current UDP Gate D `336/336`과 `ProductionApproved=True`는 current artifact에 대한
  PC/static 승인이다. 같은 창 `Close -> Connect`, new session registration과 새 ticket callback을
  증명하는 live GD-04가 아니다.
- current producer는 `EventMaskBit=1`, `EventType=1`의 D5 terminal wake만 만든다.
- Connect registration은 mask를 저장하지만 current validation은 bit 0이 포함됐는지만 확인하므로
  producer/consumer 지원 범위보다 넓은 mask도 받을 수 있다.
- 과거 live 화면의 callback registration success에도 accepted callback count는 0이었다. 등록
  성공만으로 GD-01 또는 GD-04 callback 전달을 PASS 처리하지 않는다.
- 따라서 Elmo 비교를 반영한 SIGMATEK supported-mask table, per-bit producer/payload/UI,
  duplicate/debounce 정책과 live qualification이 승인되기 전에는 `0x1`을 전체 EventMask 구현으로
  보고하면 안 된다.

## 중간 상태와 최종 상태를 혼동하면 안 되는 항목

- LASAL IDE 입력 중 `RequestFrameSize=UINT`, 잘못된 배열 상한, input 순서와 pointer/type
  오류가 여러 번 나타났지만 모두 후속 디스크 검사에서 발견돼 최종 declaration/C78 전에
  교정됐다. 중간 화면을 current ABI로 사용하지 않는다.
- Store CheckSum의 최종 구조는 class 내부 `_CheckSum`/`Internal=true`가 아니다.
  `LMCSetPositionStore.CheckSum`을 `Internal=false` 외부 client로 두고 Comm Network의
  `LMCSetPositionCheckSum1.ClassSvr`에 연결한 형태가 C78를 통과했다.
- stale LASAL editor가 외부에서 복구한 implementation을 다시 stub으로 덮어쓴 이력이 있다.
  외부 implementation 수정 뒤에는 stale tab에서 Save All하지 말고 프로젝트 재로드, 고유 본문
  확인, Rebuild, source hash 재확인 순서를 지킨다.
- Store 추가 뒤 발생한 `usingLtd` SourceOnly failure를 처음에는 “기존 무관 blocker”로
  표현했지만, 실제로는 같은 변경에서 pragma 수 3->4를 verifier에 반영하지 않은 누락이었다.
  final verifier는 exact 4와 negative fixture로 정정됐다.
- retained store의 초기 1,008-byte Intent+A/B 설계는 retirement crash에서 tombstone을 잃을 수
  있어 폐기됐다. current layout은 축별 84-byte Intent+A/B/C 네 슬롯, 4축 합계 1,344 bytes다.
- coordinate gate 뒤 첫 C78의 `1 error / 78 warnings`는 성공으로 처리하지 않았다. verifier가
  `Classes.lcb`를 읽는 동안 IDE가 같은 파일을 쓰면서 발생한 충돌이었고, verifier를 중단한
  독점 Rebuild의 `0 errors / 78 warnings`가 그 단계의 유효 결과다.
- generated artifact가 바뀔 때마다 이전 UDP/Main approval은 폐기하고 재기준화했다. current
  tuple은 최종 `Classes.lcb=CC5B7FD8...`, UDP `336/336`, Main SourceOnly PASS만 사용한다.
- distribution host parity의 `13/13` 기대값은 stale test pin이었고 current focused `20/20`에
  맞춘 뒤 전체 host parity `14/14`가 최종 결과다.
- line 46822의 “아직 commit하지 않음”은 이후 5개 목적별 commit으로 superseded됐다.
  line 53753의 “TestClass/EasyCase 보존”도 뒤의 명시적 사용자 삭제 요청으로 superseded됐다.
- 처음의 “full native RT executor가 다음” 계획은 Axis2~4 task/core/priority 증거 부족이 확인된
  뒤 native 0 preflight로 의도적으로 축소됐다. 이 축소가 current P0 범위다.

## 2026-08-19 live repository 재확인

이번 쓰레드에서 직접 다시 확인한 값이다.

- branch/HEAD: `main`, `5c80afe chore(repo): compact test and history artifacts`,
  `origin/main`과 동일
- 히스토리에 기록된 7개 최신 commit이 현재 log에 존재한다.
  - `e03352b`, `1b3011d`, `e2a4328`, `a3e83e6`, `d254f0e`
  - `e691aad`, `5c80afe`
- tracked/staged diff는 없다. 현재 `git status`의 미추적 항목은 새 원본
  `docs/history/Elmo_Master_history_260819.md`와 이 `docs/history/260819/` 분할 세트다.
- `Class/TestClass/`와 `EasyCase/LMCSetPositionStore.etf`는 현재 존재하지 않는다.
- [`LMCControlCommandService.st`](../../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st)는
  macro `FALSE`, max-jump 0, capability `0x00000017`을 실제로 유지한다.
- [`global_LMCSetPositionStore.st`](../../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSetPositionStore/global_LMCSetPositionStore.st)는
  `ARRAY [0..335] OF UDINT`의 `VAR_GLOBAL RETAIN`을 유지한다.
- [`LMCEcatInputLatch.st`](../../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st)는
  137,891 bytes이며 SHA-256
  `F7DC9857DB528D73481831D3D1F9DA3A63420DF653A2146C6E30397337855FA1`로 문서와 일치한다.
- [`Class/Classes.lcb`](../../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb)는
  8,600,084 bytes / SHA-256
  `CC5B7FD831616551117DB8260257362069DB51880C53250DBF3CEC35458A48E4`로 문서와 일치한다.
- tracked Motion Network는 `_LMCAxis1.LMCPreRtWorkTrigger -> LMCEcatInputLatch1.ClassSvr` 한
  trigger와 `LMCEcatInputLatch1.LMCAxis1..4 -> _LMCAxis1..4.Control`을 포함한다. 이것은 실제
  target task/core/OS priority 측정값이 아니다.
- 저장소에서 target `Autoexec.lsl` 또는 전체 `SET SRAMRETAIN` allocation 증거를 찾지 못했다.

이번 쓰레드에서는 SDK 1153/1153, WPF 356/356, preflight 95/95, Gate D 336/336,
SourceOnly와 C78 Rebuild를 다시 실행하지 않았다. 위 수치는 commit된 current 문서와 히스토리에
기록된 checkpoint이며, source/artifact hash와 commit 정합성만 live 재확인했다.

## 증거 경계

| Level | 현재 인정 가능한 내용 | 아직 없는 내용 |
|---|---|---|
| PC | commit된 SDK/WPF 회귀 checkpoint와 wire/parser 계약 | 이번 쓰레드 fresh 재실행 |
| source/static | 현재 source flags, ABI, Network, hashes와 tracked diff 없음 | P1/P2 구현과 activation proof |
| LASAL IDE/build | current `Classes.lcb`가 기록된 C78 artifact hash와 일치 | 이번 쓰레드 fresh Rebuild/로그 |
| PLC/runtime | 과거 TW19 SDO terminal과 일부 역사적 test image 기록 | current P0 image download, task/core/priority, SRAM map, SetPosition runtime |
| hardware | TW19 뒤 application Home=0 관찰 | TW19 physical multi-turn effect, SetPosition axis 1..4 E2E/crash matrix |

## 이어서 할 작업

즉시 마지막 사용자 요청이었던 임시 `TestClass`/`.etf` 삭제는 완료됐다. 남은 주 개발점은
SetPosition P1 이후다.

1. **P1 Control async, native 0**
   - Store Begin precedence와 fresh-Armed-only reserve를 유지한다.
   - pending context와 TCP internal `-13` consumer를 구현한다.
   - replay/exact Armed/corrupt/storage unavailable에서는 mailbox와 native가 0회여야 한다.
2. **P2 RT claim/native**
   - versioned claim-before-native, 단일 논리 call site, stable 3-sample terminal observer와
     post-claim uncertainty quarantine을 구현한다.
   - 이 단계에서도 capability는 OFF로 둔다.
3. **P3 외부 activation proof**
   - 실제 target task chain/core/OS priority와 worst-case jitter를 캡처한다.
   - `Autoexec.lsl`, 모든 retained consumer, 전체 `SET SRAMRETAIN` size/range/overlap과 cold
     power-cycle retention을 확인한다.
4. **P4 WPF recovery**
   - MainWindow mutation journal/interlock, query/retirement E2E와 replay 0회를 연결한다.
5. **P5 activation**
   - approved max-jump, Store/ordinary macro와 capability bits 3/5/7을 한 paired release에서만
     전환하고 PLC/hardware fault matrix를 수행한다.

다음 source 작업으로는 P1 Control async를 진행할 수 있다. 단, P2 native 또는 P5 activation으로
넘어가기 전에 task/core/priority와 SRAMRETAIN 외부 증거를 확보해야 한다. 현재 macro/capability를
먼저 켜거나 current-position equality를 과거 native 실행 증거로 사용하면 안 된다.

병행 backlog는 두 가지다.

- Callback: GD-04 fixed-port `Connect -> Close -> Connect` live evidence와 final EventMask support table
- Encoder: TW19 전후 drive/encoder absolute multi-turn 값을 별도 readback으로 확인

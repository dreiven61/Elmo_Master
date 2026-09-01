# SetOperationMode 8 -> 1 Readback / WPF 완료 처리 수정

- 기준: 2026-08-31, local/GitHub `dev` HEAD `96ac8132e976e46e10fa39d49f8af01298a6576b`
- 입력 evidence: 사용자 16:43~16:46 실행 로그 및 화면
- 판정: 확인된 소스 결함 수정 및 PC focused 검증 완료. **PLC 재빌드/다운로드와 8 -> 1 실기 검증은 미완료.**
- 이전 Detail49/63 admission 문제와 구분한다. 이번 Start는 접수됐고 후속 Outcome에서 Detail46이 발생했다.

## 1. 확인한 사실 / 아직 모르는 것

| 구분 | 확인 내용 |
| --- | --- |
| 사용자 실기 관찰 | PLC에서 6060=1로 만든 뒤 WPF CSP 요청 시 8로 바뀜 |
| Request22 | 1 -> 8, terminal Succeeded, write/readback/owner-release/executor-reusable 증거 및 generation4 retire 확인 |
| Request30 | 8 -> 1, Start 접수, 후속 Query34에서 Error=-31000 / Detail46 |
| PLC identity | Build1 / Boot0x71 / Map0x957F101E |
| 기존 PLC 소스 | 정상 6061 read가 요청 mode와 한 번만 달라도 즉시 quarantine. 설정 timeout이 남아도 재확인하지 않음 |
| 기존 WPF 소스 | Running 응답이면 recovery 메서드가 정상 반환하여 공통 wrapper가 PASS 출력 |
| 추가 재현 | 활성 mode journal의 전역 버튼 차단이 CloseConnection도 비활성화함 |

현재 로그에는 Write callback의 AbortCode/OS result 및 실제 quarantine reason이 없다.
따라서 이번 Request30이 **mode 반영 지연인지, 드라이브 SDO 거부인지, 다른 writer 간섭인지 확정하지 않는다.**
Detail46만으로 `6060=1` write 실패 또는 `6061=8` 지속을 단정할 수 없다.

## 2. 구현 변경

### PLC: 기존 함수 implementation만 수정

`Class/LMCDiagnosticsService/LMCDiagnosticsService.st`:

- 6060 Write는 기존처럼 정확히 한 번만 dispatch한다.
- 정상 6061 응답이 다른 mode이면 원래 timeout 안에서 read-only 재확인한다.
- 정상 verify 및 read-only recovery read 발행 간격은 최소 50ms다.
- 정상 verify 반복은 원래 시작 시각/timeout을 갱신하지 않는다.
- recovery read의 2,000ms budget은 최초 발행 때 한 번만 시작하고 후속 read에 남은 시간을 전달한다.
- requested mode와 exact readback이 일치해야 기존 terminal commit/owner-release를 진행한다.
- 제한 시간이 지나도 불일치하거나 callback/owner가 불확정이면 기존 quarantine을 유지한다. 실패를 성공으로 바꾸거나 live owner를 강제 해제하지 않는다.
- 소비한 write callback의 validation/OS/abort/length를 기존 runtime 배열의 빈 슬롯144~147에 남긴다. 격리 중 IDE로 원인을 확인할 수 있다.

RETAIN 크기, 배열 크기(192 DINT), method/class/client 선언, Network, PDO, TCP frame은 변경하지 않았다.
새 runtime 슬롯142~147은 기존 zero-init/clear 범위128~159 안이다. terminal 완료 시에는 기존 runtime clear에 따라 초기화된다.
SDK packet/parser 및 `DINT_PACKET_MAP.txt` 변경은 필요 없다. 0x7D23/24/25 schema/offset은 그대로다.

### WPF

`MainWindow.AxisSetOperationModeRecovery.cs`:

- Start 접수 후 exact-key 0x7D24를 100ms 간격으로 조회하여 Running을 계속 확인한다.
- PC observation budget은 `min(request timeout, 60000) + 3000ms`다. RPC 자체 timeout은 별도이며 이 budget이 전송 중 RPC를 중단하지는 않는다.
- `Task.Delay`와 async RPC로 dispatcher를 점유하지 않는다. 충돌하는 mutation 제한은 유지한다.
- Succeeded -> terminal proof 저장 -> exact-generation 0x7D25 성공 후에만 해당 작업의 PASS를 출력한다.
- Failed/Aborted -> terminal proof 저장 및 정상 retire 후, 작업 실패로 출력한다. 이 경우 active journal은 해제된다.
- Running 지속/조회 거부/불확정 -> PASS를 출력하지 않고 journal을 보존한다.
- 활성 journal이 있어도 CloseConnection을 허용한다. 연결 종료는 motion stop 또는 결과 확정이 아니다.
- 상세 실패 안내가 `UpdateUiState`에서 지워지지 않도록 현재 record identity에 묶어 유지한다.

## 3. 검증 결과

| 검증 | 결과 |
| --- | --- |
| VS2019 MSBuild / .NET Framework4.8 WPF Debug | PASS |
| Wpf.SetOperationModeRecovery | 10/10 PASS (신규3 포함) |
| Wpf.AxisSetOperationModeJournal | 8/8 PASS |
| Wpf.RecoveryRetirement (ledger 포함) | 27/27 PASS |
| Wpf.SetOperationModeSdk | 1/1 PASS |
| Verify-SetOperationModeStartExecution.ps1 | PASS |
| Verify-SetOperationModeStatic.ps1 -QualificationActivation | **84 PASS / 기존 Client 순서 검사 1 FAIL** |
| git diff --check / git diff --cached --check | PASS |
| LASAL IDE C78 rebuild / download / physical 8 -> 1 | 미실시 |

신규 WPF 테스트는 실제 window/dispatcher와 loopback FakeRpcServer를 사용한다.

1. Running -> Running -> Succeeded: premature PASS 없음, 자동 terminal retire, 제한 해제.
2. Running -> Running -> Failed: terminal retire 후 FAILED 표시, premature PASS 없음.
3. Running -> Running -> Detail46: journal 보존, 0x7D25 없음, ContextIdle 후에도 Close 가능, 상세 오류 유지.

세 테스트 모두 0x7D24 3회, **0x7D23 재전송 0회**를 검사한다. 이 테스트는 PLC SDO 동작을 실행하지 않는다.
static 검사에 bounded readback/clock/no-replay/callback 보관 계약11건을 추가했다.
현재 helper method text 크기는 mutation21,371 / recovery15,016 bytes로 32KiB 아래다.

### 기존 변경에서 발견한 별도 불일치

작업 시작 당시 이미 `Classes.lcb` 및 Diagnostics의 XML Client 순서가 변경돼 있었다.

- XML Channels: AxisOwnership -> InputLatch
- 생성된 class 선언 및 channel table: InputLatch -> AxisOwnership

이 사용자 변경을 되돌리거나 `.lcb`를 편집하지 않았다. 검사를 완화하여 PASS로 만들지도 않았다.
IDE에서 두 Client 선언/연결과 생성 결과를 확인한 뒤 rebuild해야 한다. 이 불일치가 이번 Detail46의 원인이라고 확정한 것은 아니다.

## 4. 실행 파일

기존 실행 경로를 Debug build로 갱신했다.

`LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Debug/LasalMotionControlApiExample.exe`

- 파일 시각: 2026-08-31 17:24:09 KST
- EXE SHA256: `514BEEE17CDE30F08E4C16FE7B4B4065C42C677555C1E604E42B50A7EDC804B3`
- SDK SHA256: `5138F8890B17A407B002EA18A09885AB509C0927A046586E349F70777A03D44C`
- 테스트한 EXE/DLL과 Debug 실행 경로의 hash 일치 확인.
- 검증 산출물은 같은 프로젝트 `bin/ModeSettlingVerified/`에 모았다. 배포본/production 승인으로 간주하지 않는다.

## 5. 실기 재검증 순서

1. 장비/축의 안전한 정지 상태를 독립 확인한다. 기존 Boot0x71 Request30의 결과를 임의 성공/실패로 간주하지 않는다.
2. LASAL Client 순서/연결 불일치를 IDE에서 확인하고 tracked 프로젝트를 rebuild/download한다. 기존 함수 구현 외 새 함수 생성은 없다.
3. WPF 새 Debug EXE 실행. 새 PLC Boot와 기존 journal Boot가 다를 때는 기존 stale-evidence 보관/폐기 UI로 **사용자가 명시 확인**한다. 이 작업은 옛 명령의 결과 증명이 아니다. PLC Boot가 같고 live uncertain owner가 남아 있으면 파일 삭제로 강제 해제하지 않는다.
4. standstill / operation-disabled 조건과 드라이브를 확인한 뒤 8 -> 1을 한 번 실행한다.
5. Running 중 자동 조회가 계속되고 terminal Succeeded / Observed=1 / OwnerReleased / ExecutorReusable / exact retirement까지 확인되는지 본다.
6. 1 -> 8도 동일하게 확인한다. mode 설정 성공과 실제 PP/CSP motion qualification은 별개다.

### Detail46이 재발하면, 재전송 전에 수집

Axis1은 `LMCDiagnosticsService1.AxisOperationModeState`의 아래 값을 IDE에서 **읽기만** 한다. 실제 instance 이름은 해당 Network를 따른다.

| index | 의미 |
| --- | --- |
| 0 | record state (5 = quarantined) |
| 10 / 11 / 23 | requested / last observed / preflight mode |
| 17 / 18 | exact executor token / evidence flags |
| 24 | quarantine reason: 4=verify mismatch, 1=write timeout, 5=callback, 나머지는 source 상수 대조 |
| 128 / 140 | runtime stage / runtime quarantine reason |
| 144 / 145 / 146 / 147 | write callback validation / OS result / SDO AbortCode / actual length |

축2~4의 record index에는 `(axis - 1) * 32`를 더한다. runtime128~147은 단일 active operation 공용이다.
UI 로그 전체 및 가능하면 SDO6060/6061 packet evidence를 함께 보존한다. 0x6060을 다시 쓰거나 현 상태만으로 옛 intent를 완료 처리하지 않는다.

이 자료로 write 자체의 거부와 readback 불일치를 구분할 수 있다. 아직은 실제 8 -> 1 완료를 보장하지 않는다.

## 6. C78 E0166 후속 수정

사용자 IDE 빌드에서 10240행 `DINT cannot be converted to UDINT` 오류가 확인됐다.
추가한 callback 진단 저장 코드에서 `completion.OsResult`는 `DINT`인데 저장 대상에
`$UDINT` overlay를 붙인 것이 원인이다. 원래 `AxisOperationModeState` 배열도 DINT이므로
overlay를 제거하고 아래처럼 동일 타입으로 직접 저장한다. 음수 OS 오류값도 유지된다.

```st
AxisOperationModeState[LMC_DIAG_MODE_RUNTIME_WRITE_OS_RESULT] := completion.OsResult;
```

인접 ValidationCode/AbortCode/ActualLength는 모두 UDINT여서 기존 unsigned overlay가 맞다.
정적 검사에 OS result의 signed 대입 및 unsigned overlay 재발 금지 계약을 반영했다.
이번 변경은 PLC implementation 한 줄이며 WPF 재빌드는 필요 없다. C78 재빌드 확인은 남아 있다.
수정 후 static 검사 결과는 85 PASS / 기존 Client 순서 불일치 1 FAIL이며, diff whitespace 검사는 통과했다.

## 7. 17:33 실기 재시험 — 미해결

- 사용자 로그: WPF BuildUtc `2026-08-31 08:30:06 UTC`, SDK `08:30:04 UTC`.
- `%TEMP%/Lasal2.log`: 17:31:30 Diagnostics/Control/SDO executor 다운로드 성공, 17:31:32 PLC linking 성공 확인. 이후 프로젝트 reset/restart 기록이 있다.
- PLC Boot `0x73`, Build1, Map `0x957F101E`, Axis1, 요청 mode1, preflight mode8/statusword0x02D0.
- 17:33:21.366 Start 접수/복구 journal 유지, 17:33:21.542 Query6에서 Detail46. 접수 로그 뒤 약176ms다.
- 17:33:23.578 수동 Query8도 Detail46. 원래 Request3 결과는 여전히 불확정이다.
- 이번 WPF는 잘못된 PASS 대신 FAILED 및 상세 복구 안내를 표시한다. **PLC 8 -> 1 문제는 해결되지 않았다.**

현재 수정된 정상 verify-mismatch 분기는 원래 timeout까지 재확인한다. 정상 clock/5초 timeout이라면
176ms 격리를 단순한 readback 반영 지연으로 설명할 수 없다. callback, owner/preemption,
runtime clock/metadata 또는 로드된 구현의 실제 분기를 확인해야 한다. 다운로드 성공만으로
PLC 메모리/실행 상태까지 검증됐다고 보지 않는다.

현재 record의 `[24]` quarantine reason, `[11]` observed mode, `[18]` evidence flags,
`[128]/[140]` runtime stage/reason, `[144..147]` write callback 값을 재시작 전 읽도록 요청했다.
원인 값이 오기 전에는 timeout을 다시 늘리거나 live uncertain owner/journal을 강제 해제하지 않는다.

## 8. 2026-09-01 live retained evidence — mode write 성공, owner publish 실패

사용자가 PLC를 재시작하지 않은 상태에서 `LMCDiagnosticsService1.AxisOperationModeState`를
확인했다. Axis 1 record와 공용 runtime의 핵심 값은 다음과 같다.

| Slot | Value | 판정 |
|---:|---:|---|
| record `+11` | `1` | 0x6061 readback이 요청 mode ProfilePosition(1)을 관측했다. |
| record `+18` | `47` (`0x2F`) | Write requested/dispatched와 verify dispatched/completed, executor reusable은 모두 참이다. Owner released bit `0x10`만 없다. |
| record `+23` | `8` | preflight/이전 관측 mode는 CSP(8)였다. |
| record `+24` | `6` | quarantine reason은 owner publish 실패다. |
| runtime `140` | `6` | 공용 runtime에도 같은 owner-publish 실패 원인이 남아 있다. |
| runtime `144..147` | `0 / 0 / 0 / 1` | callback validation=0, OS result=0, SDO AbortCode=0, actual length=1이다. |

따라서 이번 8 -> 1 시험의 SDO write와 exact 1-byte readback은 성공했다. 전체 API가
실패 및 `RecoveryRequired`로 남은 직접 원인은 terminal success를 common ownership에
게시하고 owner를 release하는 단계가 성공하지 못했기 때문이다. 현재 증거로는
`PublishAxisOwnership` 내부의 exact owner/identity 검증 실패인지, 이미 quarantine된
owner 상태인지까지는 구분되지 않는다. live uncertain record를 성공 처리하거나 강제
해제하지 않고 owner publish 경로를 수정·계측해야 한다.

## 9. Terminal owner publish bounded retry 수정

`ProcessAxisSetOperationModeRecoveryStages`의 terminal success/failure 경로를 수정했다.
기존 코드는 `PublishAxisOwnership`이 한 번이라도 `0` 이외를 반환하면 즉시 reason 6으로
quarantine했다. 수정 후에는 다음 계약을 적용한다.

- SDO write와 0x6061 readback은 다시 실행하지 않는다.
- 이미 stage된 terminal record와 exact owner tuple을 유지한다.
- 원래 Start 요청의 timeout deadline까지 owner publish/release만 재시도한다.
- 성공하면 `OwnerReleased`를 기록하고 기존 terminal commit 순서로 완료한다.
- deadline까지 실패하면 기존처럼 reason 6, Detail46으로 fail-closed quarantine한다.
- `record +21`과 runtime `[148]`에 마지막 signed owner publish result를 저장한다.
- runtime `[149]`에 publish attempt 수를 포화 증가 방식으로 저장한다.
- runtime `[150]`에 마지막 publish 시각을 저장하고 50 ms보다 빠른 재호출을 금지한다.
- terminal payload identity 자체가 손상된 경우 `-4`를 저장하고 즉시 quarantine한다.

정적 검사에는 no-SDO-replay, original-deadline quarantine, signed result 보존, attempt count와
50 ms rate-limit 계약을 추가했다. 결과는 `90 PASS / 1 기존 FAIL`이다. 기존 FAIL은
`LMCDiagnosticsService.st` XML Client 순서와 생성 declaration 순서가 다른 항목이며 이번
implementation 수정에서 생성 파일이나 IDE 선언을 임의 변경하지 않았다. C78 build,
download 및 물리 8 -> 1 재시험은 아직 수행되지 않았다.

## 10. 2026-09-01 common ownership 캡처로 확정된 CSP 고정 ACK 판정

사용자가 추가로 캡처한 common ownership 상태는 다음을 확인했다.

- `OwnershipState[24]=0`: 공용 ownership 저장소 integrity 오류가 아니다.
- Axis 1 owner record는 `state=11(QUARANTINED)`, `owner=6(AxisOperationMode)`,
  `command=0x7D23`, axis/reference/resource/admission과 56-byte identity가 모두 일치한다.
- observer record는 강제 quarantine을 요구하지 않는다.
- `OwnershipIdentityState`의 magic/version/size/token/generation/session/sequence와
  command/reference/owner/resource/admission이 owner record와 일치한다.

따라서 identity 또는 preemption 저장소 손상은 배제됐다. 직접 원인은
`TCPMotionInterface.st`의 0x7D23 exact response 분류가 성공/도메인 실패 모두
`Sendbuf[24] = 8`만 인정하던 CSP 전용 판정이다. 요청 mode 1에 대해 PLC diagnostics가
정상적으로 mode 1을 echo해도 TCP 계층은 exact ACK도 exact failure도 아니라고 판단하고
`RollbackAxisOwnership(... Reason:=-21)`을 실행했다. 이후 비동기 0x6060 write와 0x6061
readback이 성공해도 이미 owner가 quarantine되어 terminal publish가 실패했다.

수정 내용은 다음과 같다.

- admission rejection payload의 mode를 상수 8이 아니라 exact requested mode로 echo한다.
- 성공 ACK와 well-shaped domain-failure ACK의 P16을 exact requested mode와 비교한다.
- PP(1), PV(3), IP(7), CSP(8) 모두 동일 계약을 사용한다.
- frame 크기/offset, command ID, no-replay, timeout, quarantine 및 terminal retire 규칙은
  변경하지 않는다.
- `OutcomeStoreCorrupt(47)` 또는 Detail46 fail-closed 검증을 완화하지 않는다. 잘못된
  CSP 고정 분류가 owner를 먼저 rollback하지 않게 하여 정상 terminal publish 경로가
  실행되도록 한다.

기대되는 실기 결과는 8 -> 1 요청 후 0x6061=1 확인, terminal success commit,
exact-generation retire, owner record 해제 및 WPF UI 재활성화다. 이는 C78 build/download와
물리 시험 전까지 정적 수정 상태이며 실기 PASS로 판정하지 않는다.

## 11. 2026-09-01 SetOperationMode 구현 완료

current completion baseline은 `dev@0afbc2a79dff1b63f908b1bde3bd2502843045ff`
(`dev : SetOpMode Complete`)이다. 위 1~10절은 문제를 좁혀 간 historical investigation으로 보존하고,
current 판정은 다음으로 고정한다.

- PP/PV/IP/CSP exact requested-mode ACK/response contract 적용
- CSP(8) hardcoded ACK 판정 제거
- exact requested-mode `0x6060` one-shot + `0x6061` read-only settling
- irreversible dispatch 이후 no replay
- terminal owner publish/release bounded retry
- durable exact outcome/generation retirement
- WPF Running polling / premature PASS 방지 / indeterminate fence
- supported mask `0x018A`, capability triad와 runtime gate active

따라서 SetOperationMode **feature implementation은 완료**다. 이후 이 기능에서 발견되는 CI runner,
metadata/generated artifact 정합성은 repository qualification hygiene로 분리한다. 전체 API production
release 여부는 Generic SDO, HomeDS402, HomeDS402Ex, SetPosition 등 나머지 gate와 함께 별도 판정한다.

- SDK `1074/1074`, WPF Maintenance `23/23`, UI `8/8` 통과

이제 LASAL IDE에서 [LMCEcatInputLatch.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st:136)의 선언만 아래처럼 수정해 주세요.

```text
SubmitAxisZeroHome
  OperationToken : UDINT
  AxisReference : DINT
  ExpectedActualPosition : DINT
  Result : DINT

CopyAxisZeroHomeResult
  OperationToken : UDINT
  pDest : ^void
  DestSize : UDINT
  Result : DINT
```

`Save All`만 하고 F9는 아직 실행하지 마세요. 저장 후 `저장 완료`라고 알려주시면 제가 `LMC_Home` PLC 구현을 이어서 작성하겠습니다.

<details><summary>24 previous messages</summary>

> 사용자 IDE 저장을 기다리는 동안 멈추지 않고 진행하겠습니다. 제가 IDE를 직접 조작하지 않는 범위에서 축 1~4의 엔코더 종류·feedback socket 근거를 로컬 설정/캡처에서 찾고, `LMC_Home`·DS402·TW19/TW20 정적 검증기가 최종 설계를 실제로 강제하는지 점검하겠습니다. 확인되지 않은 엔코더 값을 추정해서 활성화하지는 않겠습니다.
>
> 추가로 최신 Markdown과 함께 배포되는 HTML 인계 문서가 아직 예전 cycle 단위 선언을 담고 있는 불일치를 발견했습니다. 구현자가 HTML만 열면 잘못된 변수를 만들 수 있으므로, 이 문서는 Markdown의 현재 `TimeoutMilliseconds` 계약과 동기화하겠습니다.
>
> ```
> SubmitAxisZeroHome
>   OperationToken : UDINT
>   AxisReference : DINT
>   ExpectedActualPosition : DINT
>   Result : DINT
>
> CopyAxisZeroHomeResult
>   OperationToken : UDINT
>   pDest : ^void
>   DestSize : UDINT
>   Result : DINT
>
> 이거 저장하고 IDE 종료했어
> ```
>
> 확인하겠습니다. 이제부터는 IDE를 다시 열지 않고, 저장된 generated declaration과 프로젝트 등록 파일이 실제로 맞는지 먼저 검사한 뒤 `LMCEcatInputLatch.st`의 구현부만 외부에서 작성하겠습니다. 그 구현을 정적 검증한 다음 다음 IDE 선언 묶음을 정확히 요청드리겠습니다.
>
> 저장 내용은 정상 반영됐습니다. `OperationToken`과 `DestSize`는 `UDINT`, stale-position 입력도 추가됐고, IDE가 `^void`를 내부 표기인 `^pVoid`로 정규화한 것도 정상입니다. 이제 RT 경로에 “Standstill 확인 → fresh 실제 위치 일치 확인 → `SetPosition(..., 0)` 정확히 1회 → raw drive 불변 및 좌표 0을 3개 연속 cycle로 확인” 로직을 넣고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+46 -8)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 저장된 선언은 확인했습니다. 다만 IDE가 `^void`를 `^pVoid`로 정규화한 부분은 포인터 깊이 해석이 엇갈릴 수 있어, 기존 LASAL 선언 사례와 타입 정의를 먼저 대조한 뒤 구현을 이어가겠습니다. IDE는 다시 열지 않으셔도 됩니다.
>
> 한 가지 선언만 바로잡아 주세요. 현재 저장 결과가 `pDest : ^pVoid`인데, 같은 클래스의 기존 복사 함수들은 모두 `pDest : ^void`이고 `^pVoid`는 이 프로젝트에서 포인터를 돌려주는 이중 포인터 용도로 쓰입니다.
>
> LASAL IDE에서 `LMCEcatInputLatch > CopyAxisZeroHomeResult > pDest`의 타입을 **`void`에 포인터 1단계**, 저장된 `.st` 표기가 정확히 **`pDest : ^void;`**가 되도록 수정한 뒤 Save All하고 IDE를 종료해 주세요. F9는 아직 하지 마세요. 나머지 선언은 그대로 두면 됩니다. 그동안 저는 RT mailbox 구현 계약과 검증 스크립트를 정리하겠습니다.
>
> ^void로 바꿨다
>
> 확인하겠습니다. 선언과 구현 시그니처가 둘 다 `^void`로 저장됐고 IDE 프로세스가 종료된 것을 확인한 다음, 해당 `.st`의 RT 구현을 바로 적용하겠습니다.
>
> 교정된 ABI를 확인했습니다. 이제 `LMCEcatInputLatch`에 한 번만 실행되는 `SetPosition(...ACTPOS...DEST, 0)` 호출, 원시 드라이브 위치 불변 확인, 3개 연속 fresh sample, terminal 결과의 원자적 게시를 넣고 있습니다. 이 경로에는 `MoveReference`, Home 스위치, 리미트 스위치 호출을 넣지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+19 -9)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+362 -5)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+8 -6)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+138 -19)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+3 -2)
>
> </details>
>
> RT 내부 배열의 슬롯 의미도 이제 고정합니다. RT cycle 번호와 wire의 millisecond 필드를 섞지 않도록, Latch 결과에는 cycle만 두고 `StartMilliseconds/CompletionMilliseconds`는 이후 `LMCControlCommandService`가 `ops.tAbsolute`로 기록하도록 문서와 검증기를 맞추고 있습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+66 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+30 -8)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+1 -0)
>
> </details>
>
> 엔코더 쪽은 로컬 자료를 다시 대조한 결과, 현재 capability를 켤 수 있는 축이 없습니다. Axis4에만 과거 Panasonic/소켓 1 흔적이 있으나 멀티턴 비트가 0이고 최신 readback도 없습니다. 이 사실은 기능 코드를 막는 게 아니라, TW19/TW20 gate를 계속 OFF로 두어 오적용을 막는 근거로 반영하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+26 -0)
> - Wrote `./docs/history/260803/99_analysis_summary.md` (+22 -21)
>
> </details>
>
> 리뷰에서 재실행 방지 경계 두 곳을 찾았습니다. RT가 호출 마커를 남긴 뒤 phase 갱신 전에 중단됐다고 가정해도 다시 호출하지 않도록 `NativeCallCount != 0`인 phase 0을 corrupt terminal로 막고, 같은 OperationToken을 다른 축/위치와 재사용하는 요청도 거부하도록 보강합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+12 -5)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+6 -3)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+8 -4)
> - Wrote `./docs/history/260803/99_analysis_summary.md` (+1 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+2 -2)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+4 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+13 -6)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+2 -1)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+2 -0)
>
> </details>
>
> 현재 Latch 구현과 ABI 음성 검증은 각각 `24/24`, `10/10` 통과했습니다. 이제 전체 SourceOnly를 한 번 돌려 이 단계가 끝난 뒤 실제로 다음 IDE 미입력 지점에서 fail-closed하는지 확인하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 확인했습니다. `pDest : ^void`가 맞습니다. 현재 디스크의 선언·구현 ABI와 RT zero-home 계약을 다시 검증한 뒤, 제가 IDE를 만지지 않고 다음에 사용자가 입력할 `LMCControlCommandService` 항목만 정확히 정리하겠습니다.
>
> 현재 저장본에서 선언부와 구현부 모두 `^void`로 일치합니다. 이 수정으로 ABI 차단점은 해소됐고, 이제 자동 음성 테스트와 문서 동기화를 마지막으로 확인하고 있습니다. 다만 `LMCControlCommandService`에는 아직 스위치 탐색 기반의 구 `MoveReference` 경로가 남아 있어 지금은 F9 빌드 단계가 아닙니다.
</details>

좋아. `^void` 수정은 정상 반영됐습니다.

현재 확인 결과:

- 선언부·구현부 모두 `pDest : ^void`
- 현재 소스 ABI 검증 PASS
- 잘못된 ABI 음성 테스트 `10/10` 거부
- RT mailbox 음성 테스트 `24/24` 거부
- Home 경로에 `MoveReference` 없음
- 네 축 모두 `SetPosition(Mode:=LMCAXIS_SET_ACTPOS_APPUNIT_DEST, Position:=0)`만 사용
- 물리 이동과 Home/Limit 스위치 탐색 없음

다음 LASAL IDE 작업을 부탁합니다. `LMCControlCommandService` 한 클래스만 수정하세요. 정확한 내용은 [IDE 핸드오프 문서 3.2](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:182)에 있습니다.

핵심 작업:

- `InputLatch : CltChCmd_LMCEcatInputLatch` Client 추가
- `ReferenceState` 삭제
- `ZeroHomeState[0..63]`, `OwnershipState[0..351]` 추가
- `HandleRequest` 끝에 `CallerSessionEpoch`, `RequestSequence`, `AdmissionToken`, `OwnerGeneration` 추가
- `ProcessAxisReference` 삭제
- `ProcessAxisZeroHome` 추가
- 문서에 기재된 축 소유권 함수 8개 추가

이번에는 **Save All 후 IDE만 종료**하세요. 아직 F9 빌드는 하지 마세요. 저장이 끝나면 “저장하고 IDE 종료했다”라고 알려주세요. 이후 기존 `MoveReference` 구현을 제가 외부 소스에서 LMC zero-home 경로로 교체하겠습니다.

<details><summary>29 previous messages</summary>

> IDE 선언 작업을 기다리는 동안 멈추지 않고 진행하겠습니다. 저는 선언부는 건드리지 않은 채 `LMCControlCommandService`의 기존 `MoveReference` 구현을 새 zero-home 흐름으로 교체할 준비를 하고, 병렬로 TW19/TW20과 전체 구현 문서의 남은 차단점을 다시 감사하겠습니다. 축별 실기 근거가 없는 write gate는 활성화하지 않습니다.
>
> 현재 구현 경계도 확인했습니다. `LMC_Home`은 RT mailbox까지 정적으로 완성됐지만 서비스 쪽은 아직 구 parser이고, TW19/TW20은 전용 lifecycle 자체가 LASAL에 없습니다. 지금 두 경로를 각각 prewire 구현 중이며, encoder write gate는 계속 `FALSE`로 유지합니다. 동시에 PC 계약 전체 회귀도 다시 돌립니다.
>
> PC 전체 회귀는 최신 소스에서 `1074/1074 PASS`, 빌드 경고·오류 `0`으로 다시 확인됐습니다. 지금은 LASAL 선언 생성 영역을 건드리지 않고 두 implementation을 작성 중입니다. 완료 후에는 “정적 구현 완료”와 “IDE 선언/Network 필요”, “PLC/실축 미검증”을 분리해 보고하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -3)
>
> </details>
>
> PC 계층은 그대로 정상이고, LASAL 쪽에서는 첫 번째 source-only 진전이 들어갔습니다. `LMCDiagnosticsService`에 TW19/TW20 전용 gate·상태기계 prewire가 추가됐고 gate는 `FALSE`, 축별 profile/socket/evidence도 모두 `0`입니다. 아직 generated declaration과 TCP route가 없으므로 F9 금지는 그대로입니다. 코드가 완성되면 제가 전체 diff를 별도로 검토합니다.
>
> 현재 가장 중요한 wire 오류도 잡았습니다. Control service는 full frame을 반환하므로 `0x7D13`은 payload 24바이트/총 32바이트, `0x7D18/19`는 payload 144바이트/총 152바이트여야 합니다. 기존 정적 검증기가 총 길이와 payload offset을 혼동하고 있어 이를 함께 교정 중입니다. 잘못된 24/144 총 길이에 코드를 맞추지는 않습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> `^void` 변경은 반영된 상태로 계속 진행 중입니다. 지금은 IDE를 다시 열지 않고, 그 ABI를 기준으로 LMC Zero Home 실행 경로와 TW19/TW20 유지보수 경로의 소스·검증기를 마무리한 뒤 다음 IDE 입력 항목만 정확히 정리하겠습니다.
>
> 현재까지 확인된 핵심은 같습니다. PC 계약 테스트는 SDK `1074/1074`, WPF `329/329`로 통과했지만 이는 PLC 실기 증거가 아닙니다. TW19/TW20은 정확한 엔코더 소켓·프로파일·실축 증거가 없어 모든 활성 gate를 계속 `FALSE`로 둡니다.
>
> 지금 소스는 의도적으로 “IDE 선언 저장 전 pre-wire” 단계라서 F9를 누르면 안 됩니다. 구현부와 검증기를 먼저 일치시킨 다음, 제가 생성 선언부를 직접 건드리지 않고 필요한 IDE 입력 목록만 묶어서 요청드리겠습니다.
>
> LMC Home 쪽에서는 기존 `ProcessAxisReference`/`MoveReference` 경로를 제거하고 `ProcessAxisZeroHome`으로 교체했습니다. 또한 9축 공통 ownership 구현에서 발견한 multi-axis commit 오류를 바로 수정 대상으로 돌렸습니다. 아직 검토·테스트 전이므로 완료라고 부르지 않겠습니다.
>
> 현재 작업 분리는 이렇게 유지하고 있습니다.
>
> - LMC Home: 현재 좌표를 0으로 재정의하는 RT 경로, `MoveReference` 호출 0회.
> - DS402 Home: method 37/current-position 방식만 허용하되 아직 gate OFF.
> - TW20/TW19: 각각 정확히 `0x3204:14`/`:13`만 허용하고, 축별 증거가 0이라 gate OFF.
> - 공통 조건: ACK가 아니라 retained terminal·executor drain·ownership release까지 확인.
>
> 다음 IDE 요청 전까지 정적 검증에서 이 경계가 깨지지 않는지 확인하겠습니다.
>
> TW20/TW19: 각각 정확히 0x3204:14/:13만 허용하고, 축별 증거가 0이라 gate OFF.
>
> 이거는 16#20FC:1,2 에 1쓰면 리셋되는건데 니가 얘기하는건 어떤거냐?
>
> 맞습니다. 지금 제가 TW19/TW20 의미를 잘못 연결했을 가능성이 큽니다. `0x3204:13/14`와 `0x20FC:1/2`의 근거를 로컬 Elmo 문서·기존 캡처·현재 소스에서 바로 대조하겠습니다. 그 전까지 TW19/TW20 구현은 중단하고 어떤 write도 활성화하지 않겠습니다.
>
> <details><summary>Searched the web</summary>
>
> Searched the web for `site:elmomc.com "0x20FC" "TW[20]" ...`
>
> Searched the web for `Elmo Gold Line DS-402 Implementation Guide 20FC Absolute Sensors Functions PDF ...`
>
> </details>
>
> 맞습니다. 제가 말한 `0x3204`는 Elmo의 범용 `TW[]` alias 객체였습니다.
>
> - `0x3204:0x13` = `TW[19]`, LONG/4-byte
> - `0x3204:0x14` = `TW[20]`, LONG/4-byte
> - `0x20FC:1` = 같은 `TW[19]` 기능의 전용 Absolute Sensor 객체, UINT16/2-byte
> - `0x20FC:2` = 같은 `TW[20]` 기능의 전용 객체, UINT16/2-byte
>
> 그리고 쓰는 값 `1`은 단순 Boolean이라기보다 “feedback socket 1”을 뜻합니다. 현재 엔코더가 socket 1이면 말씀대로 `0x20FC:1` 또는 `:2`에 UINT16 값 `1`을 쓰는 것이 정확합니다.
>
> 따라서 기능 의미는 같지만, 우리가 실제 EtherCAT SDO로 구현할 계약은 사용자가 확인한 전용 경로인 `0x20FC:1/2`가 맞습니다. 제가 `0x20FC`를 fallback으로 보고 금지한다고 정리한 부분은 잘못됐습니다. 방금 TW 구현을 중단했고, 코드·SDK·WPF·DINT map·검증기를 전부 `0x20FC`, 2-byte 기준으로 수정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 맞습니다. 제가 `0x3204`의 범용 `TW[]` 별칭을 실제 EtherCAT SDO 계약과 혼동했습니다. 구현 기준을 `0x20FC:1 = TW19`, `0x20FC:2 = TW20`, `UINT16(2바이트)`, 값 `1 = feedback socket 1`로 바로잡고 있으며, 기존 `0x3204` 전제는 코드·검증기·문서에서 제거하겠습니다. 현재 병렬 작업 결과를 회수한 뒤 전체 정적 테스트까지 다시 확인하겠습니다.
>
> Elmo NetHelp와 이 프로젝트의 실제 ESI까지 대조했습니다. 네 축의 드라이브 revision 모두 ESI상 `0x20FC`를 지원하고, ESI 정의도 `SI1 = Clear Absolute Multi Pos`, `SI2 = Reset EnDAT Error`, 둘 다 `UINT/16-bit/WO`입니다. 따라서 주소·자료형 문제는 해소됐고, 남은 gate 조건은 주소가 아니라 축별 엔코더 종류와 현재 motor-off 증거입니다.
>
> 정정 범위는 단순 문서 수정이 아니라 실제 송신 계약까지 포함합니다. 현재 확인한 목표는 `0x3204`로 재시도하거나 이중 전송하지 않고, 전용 경로에서 정확히 한 번만 `0x20FC:1/2`에 2바이트 값을 쓰는 것입니다. 일반 SDO 편집기에서는 이 파괴적 명령을 계속 차단하고, 전용 확인·motor-off·소유권 경로로만 통과시키겠습니다.
>
> 한 가지 실기 정보만 확인해 주시면 gate 범위를 정확히 정할 수 있습니다. `1`이 네 축 모두의 feedback socket 1이라는 뜻인지, 그리고 Axis1~4의 엔코더 종류가 각각 Panasonic/Tamagawa/EnDat 2.2 중 무엇인지 알려주세요. 답을 기다리는 동안 주소·타입·TCP 전용 경로의 정적 검증은 계속 진행하겠습니다.
>
> PLC 송신 구현은 이제 네 축 모두 `UINT` 임시값을 사용해 `WriteLength := 2`로 고정됐고, TCP의 `0x7E53/54/55` 전용 Start/Outcome/Retire route도 연결됐습니다. 지금은 `0x3204`, 잘못된 4바이트 길이, fallback을 다시 끼워 넣는 음성 fixture들이 모두 거부되는지 회귀 검증 중입니다.
>
> PC 회귀 테스트는 현재 단독으로 다시 실행 중이며 프로세스가 응답 상태로 진행되고 있습니다. 한편 Home 쪽은 별도 결론이 나왔습니다. 좌표 0 설정 로직 자체는 정상이나, 기존 Move/Power/Group 명령이 공용 ownership을 우회하므로 gate만 켜는 것은 경쟁 조건이 생깁니다. 이 부분은 주소 수정과 별개의 활성화 차단점으로 남겨 두겠습니다.
>
> 정정 회귀는 통과했습니다: SDK `1075/1075`, WPF `329/329`, encoder 음성 fixture `34/34`입니다. 이제 제가 최종으로 남은 HTML 동기화와 정적 검사만 직접 확인하겠습니다. LASAL IDE는 제어하지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+28 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic

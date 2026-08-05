END_VAR
```

```text
LMCControlCommandService

FUNCTION GLOBAL PublishAxisOwnershipDs402Receipt
VAR_INPUT
    AxisMask : UDINT
    AdmissionToken : UDINT
    OwnerGeneration : UDINT
    ReportKind : UINT
    ReportValue0 : UDINT
    ReportValue1 : UDINT
    ObservationCycle : UDINT
    pDs402State : ^void
    Ds402StateSize : UDINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
```

새 Client나 Comm Network 연결은 추가하지 마세요. 기존 연결도 삭제하지 마세요. 완료 후 `Save All → IDE 종료`까지만 하고, 아직 Rebuild하지 않은 상태로 알려주세요. 그러면 구현부와 verifier를 이어서 맞추겠습니다.

<details><summary>86 previous messages</summary>

> IDE 선언이 아직 반영됐는지 현재 파일을 먼저 확인하겠습니다. 반영 전이라면 그 작업을 기다리는 동안 설계문서 기준의 다음 미구현 항목과 정적 계약을 계속 정리하겠습니다.
>
> IDE 선언을 기다리는 동안 멈추지 않고, 구현 문서 대비 남은 소스 작업을 영역별로 병렬 감사하겠습니다. 코드 위치 조사와 검토를 나누기 위해 `caveman:cavecrew` 지침을 적용합니다.
>
> 현재 IDE 생성물에는 아직 새 선언이 없습니다. 대신 ordinary Home의 검증된 receipt 구현을 DS402에 대입해 보니, DS402 retained state의 `124..127`을 terminal/rollback receipt overlay로 쓰는 설계가 가장 일관적입니다. 다음 Start가 이를 지우기 전까지 warm-restart 증거를 유지할 수 있도록 정확한 슬롯·단계 계약을 문서화하고 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 확인했습니다. 앱과 LASAL IDE가 모두 종료된 상태이므로 지금은 외부 편집이 허용된 추적 `.st` 소스와 검증 스크립트만 수정하겠습니다. 우선 Encoder Maintenance의 소유권 롤백·무동작 판정·타임아웃 복구 결함을 실제 코드에서 다시 확인한 뒤 보강하고, IDE 선언이 필요한 DS402 receipt/drain은 건드리지 않겠습니다.
>
> 이제 LASAL IDE에서 아래 선언만 부탁합니다. 전체 내용은 [IDE handoff Section 15 (line 1035)](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:1035)에 있습니다.
>
> 작업 완료했어
>
> 완료된 선언부터 바로 검사하겠습니다. 지금은 Rebuild하지 않고, 생성된 클래스 선언·GLOBAL 함수 시그니처와 기존 Comm Network 연결 보존 여부만 확인한 뒤 implementation 작업으로 이어가겠습니다.
>
> 선언 검사는 통과했습니다. 두 함수의 이름·타입·입력 순서가 Section 15와 일치하고, 구현부는 IDE가 만든 빈 stub 상태입니다. 기존 `InputLatch`·`AxisOwnership` 연결도 각각 1개로 유지됐고 앱/IDE 프로세스도 없습니다. 이제 이 ABI를 사용해 DS402 durable receipt와 bit4 safety-drain 구현을 진행하겠습니다.
>
> 현재 ABI를 실제 상태 머신에 맞춰 대조 중입니다. 확인된 핵심은 두 가지입니다. DS402 receipt는 기존 runtime slot `124..127`을 terminal/rollback 단계에서만 receipt ledger로 전환해야 하고, safety drain은 단순 bit 4 low가 아니라 이미 대기 중인 `SetHigh` 요청까지 tombstone 처리해야 합니다. 이 두 조건을 빠뜨리면 warm restart나 늦은 RT 요청에서 다시 bit 4가 올라갈 수 있습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+22 -8)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+101 -25)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+50 -5)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+38 -17)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+81 -6)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+10 -2)
>
> </details>
>
> InputLatch 쪽은 먼저 반영했습니다. safety drain이 기존 Home mailbox보다 우선하고, 실제 bit 4 high 쓰기 직전에 dispatch sequence를 claim하며, low readback 뒤 같은 token의 대기 명령을 `-6`으로 폐기하도록 구성했습니다. 이제 ownership receipt를 6단계 ledger로 구현하고 있으며, gate와 capability bit는 그대로 유지합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+370 -6)
>
> </details>
>
> 아직도 안고쳐진거지?
>
> User context
>
> Attachments:
> - [15:36:28.549] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\L…: `C:/Users/dreiv/.codex/attachments/081ee5b3-f795-4e11-8e21-2fabbd49ef12/pasted-text.txt`
>
> 지금 제공한 로그를 확인하겠습니다. 다만 현재까지 한 수정은 방금 생성한 IDE ABI 위에 외부 소스를 작성 중인 단계라 C78 Rebuild·PLC Download가 아직 없었습니다. 따라서 실행 중인 PLC가 이전 빌드라면 증상은 그대로인 것이 맞습니다. 로그에서 BootId와 실패 지점을 확인해 정확히 구분하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 맞습니다. 현재 PLC에 내려간 버전 기준으로는 아직 안 고쳐졌습니다.
>
> 이번 로그에서 Axis1은 Home 후 raw 위치가 `8027834 → 8027836`으로 `+2 count` 변했고, 그래서 `Detail=38`로 격리됐습니다. 그 격리가 해제되지 않아 Axis2·3은 연쇄적으로 `Detail=41` 거절입니다. 지금 작성 중인 수정본은 아직 C78 Rebuild·PLC Download 전이므로 재시험해도 같은 결과가 납니다. 우선 `+2 count` 실측을 반영하고 격리 해제 경로까지 정적 검증한 뒤, 빌드 요청 시점을 정확히 알려드리겠습니다.
>
> 이번 로그는 단순히 “이전 빌드가 남아 있다”는 증거만이 아닙니다. 준비해 둔 `±1 count` 허용치도 Axis1의 실측 `+2`를 통과하지 못합니다. Home은 좌표 6개가 모두 0이고 축 오류도 없었으므로, raw feedback 검증을 wrap-safe `±2 count`로 맞추되 `±3`부터는 계속 실패하도록 PLC의 두 판정 지점과 검증기를 함께 수정하겠습니다.
>
> <details><summary>Edited 13 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+7 -3)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+5 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+28 -18)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminHomeModels.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminLmcHomeContractTests.cs` (+64 -6)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -3)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+14 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+95 -41)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+50 -2)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+118 -5)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+44 -8)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+12 -5)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+7 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+98 -18)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+110 -5)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+16 -16)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 네. 현재 PLC 다운로드본은 아직 미수정 상태입니다. 다만 이번 로그의 핵심인 raw feedback `+2 count`까지 허용하도록 PLC·SDK·계약 테스트는 맞췄고, 지금은 DS402 실패 후 축을 붙잡는 정리 경로까지 정적 검증 중입니다. 아직 Rebuild/Download나 재테스트는 하지 마세요.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+19 -18)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+6 -2)
>
> </details>
>
> `+2 count` 수정 자체는 끝났습니다. 지금 남은 것은 실패한 Home이 Axis1 ownership을 남겨 Axis2/3를 `Detail 41`로 막는 경로입니다. 이 경로는 전원 중단 중에도 다시 이어지는 receipt와 강제 bit-4 LOW drain으로 바꾸고 있습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+161 -138)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+24 -6)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+26 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+47 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalAxisOwnershipActivationGuard.Fixture.ps1` (+13 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalAxisOwnershipActivationGuard.Fixture.ps1` (+13 -13)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+19 -22)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+3 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+50 -39)
>
> </details>
>
> 현재 정적 검토에서 한 가지를 더 확인했습니다. 기존 실패 cleanup만 고치면 부족하고, Safety 명령이 실행되기 전에 DS402 bit 4 LOW/readback가 먼저 끝나야 합니다. 지금 그 순서를 Control Service 앞단에 넣고 있으며, 이 검증이 끝날 때까지는 DS402 Home gate를 계속 `FALSE`로 유지합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+166 -8)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+5 -5)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+89 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 네. 첨부 로그 기준으로는 아직 완전히 고쳐지지 않았습니다. Axis1은 위치 0 설정까지 갔지만 `Quarantined/Detail 38`로 끝났고, 그 소유권이 남아 Axis2·3이 `Detail 41`로 막혔습니다. 지금은 이 두 경로를 소스에서 닫은 뒤 정적 검증까지 통과시키는 중이며, 아직 Rebuild/Download 단계는 아닙니다.
>
> 진행하면서 설계문서는 바로바로 업데이트해도 상관없지만 매뉴얼은 바로바로 업데이트 하지마. 바로바로 업데이트 하니까 쓸데없이 많은 일이 소모되는거 같다...
>
> 알겠습니다. 앞으로는 구현 중에는 설계문서와 handoff만 갱신하고, 사용자 매뉴얼·API 매뉴얼·배포 문서는 기능 계약이 확정되고 정적 검증까지 끝난 뒤 한 번에 맞추겠습니다. 이미 수정된 매뉴얼은 지금 추가로 손대지 않고, 이번 수정의 안정화 작업부터 끝내겠습니다.
>
> 이제 실패 응답 전에 소유권을 회수할 수 있도록, DS402 Start에 “실행 예정”과 “롤백 예정”을 구분하는 영속 단계부터 넣겠습니다. 이 구분이 없으면 전원/태스크 재시작 시 실패 요청을 다시 실행으로 오인할 수 있어, 단순히 Rollback 호출만 추가해서는 충분하지 않습니다. 사용자 매뉴얼은 건드리지 않습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+137 -95)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+5 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+3 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+48 -0)
>
> </details>
>
> 영속 롤백 단계는 소스에 반영됐고, 첫 전체 계약 검증은 예상했던 기존 정규식 한 곳에서 멈췄습니다. 구현 오류를 찾은 것이 아니라, 검증기가 새 `HIGH dispatch sequence` 보호문을 허용하지 않는 상태입니다. 이제 검증기를 새 계약에 맞추되, HIGH·LOW 경로를 느슨하게 통과시키지 않도록 별도 조건으로 나눠 갱신하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+39 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+27 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -3)
>
> </details>
>
> 독립 재검토에서 두 가지를 더 확인했습니다. Rollback이 성공했는데도 응답을 `Detail 42(격리)`로 덮던 부분과, 일시적인 채널 단절을 즉시 영구 격리로 바꾸던 부분입니다. 둘 다 실제로 다음 축을 막을 수 있으므로, 성공 시 원래 실패 원인을 보존하고 영속 receipt는 제한 시간 동안 재시도하도록 바로 보정하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+27 -15)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+4 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+33 -25)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+33 -25)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+92 -30)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+15 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -84)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+21 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+5 -3)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+14 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+21 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+22 -5)
>
> </details>
>
> 계약 검증은 이제 DS402 Home/소유권 구간을 지나 TCP callback 자체검사까지 진행됐습니다. 현재 중단점은 Home 수정과 무관한 오래된 negative fixture가 더 이상 현재 소스 문자열을 찾지 못한 문제입니다. 이 항목은 우회하지 않고 현재 구현의 실제 구문에 맞춰 고친 뒤 전체 검증을 계속하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+22 -20)
>
> </details>
>
> 전체 SourceOnly 계약 검증이 현재 구성(`ExpectedSdoWriteAxis=1`)으로 통과했습니다. 다만 여기서 끝내지 않고, 방금 추가한 stage88·receipt 재시도와 bit-4 drain을 두 명에게 최신 소스 기준으로 다시 독립 검토시키고, 그동안 저는 C# 테스트·ASCII·diff 검사를 진행하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+0 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+114 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+8 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+11 -5)
>
> </details>
>
> 설계문서는 현재 상태를 바로 반영하겠습니다. 기존 Section 14는 당시의 역사적 checkpoint로 보존하고, 그 결론을 덮어쓰지 않은 채 Section 16에 “IDE 선언 완료 후 실제 구현·검증 결과”를 새로 추가하겠습니다. 매뉴얼 파일은 수정하지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+83 -0)
>
> </details>
>
> 재검토에서 실제 소유권 누수 가능성 두 개가 확인됐습니다. Commit 결과가 불확실한 순간을 즉시 `101`로 바꾸면 common owner를 회수할 방법이 사라지고, partial receipt도 채널 단절 후 같은 문제가 생깁니다. 해결은 실패한 Commit을 rollback-only 단계로 원자적으로 되돌리고, exact RESERVED/ACTIVE 어느 쪽이든 “아직 handler dispatch 전”이라는 증거 아래 receipt로 회수하며, 일시 단절에서는 journal을 지우지 않고 재시도하는 것입니다. 이 수정부터 반영합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+12 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+8 -23)
>

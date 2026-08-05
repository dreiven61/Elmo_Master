> [13:25:12.845] Home/Encoder capabilities: DiagnosticsBits=0x000C633F; DiagnosticsBuild=1; BootId=0x00000014; MapRevision=0x957F101E; TW20=True; TW19=True; AdminFeatures=0x00000017
> [13:25:12.846] Refresh Home/Encoder Maintenance Capabilities PASS.
> [13:25:37.725] LMC Home Start (Current Position Zero) started.
> [13:25:37.789] LMC Home Start (Current Position Zero) PASS.
> [13:25:37.792] LMC Home outcome pending. Use Read Home Status for exact completion proof.
> [13:26:08.132] Read Home Status started.
> [13:26:08.158] LMC Home outcome: RecordState=Quarantined; HomeSucceeded=False; OriginalStatus=1; OriginalErrorId=-31000; OriginalDetail=38 (LmcHomeExecutionFailed); AxisStatus=0x22D0000A; AxisError=0; RawDriveBefore=8382700; RawDriveAfter=8382701; ActualApplicationAfter=0; SetApplicationAfter=0; ActualInternalAfter=0; SetInternalAfter=0; DestinationInternalAfter=0; MasterInternalAfter=0; NativeCommandState=0; EvidenceFlags=0x0000001F; StopState=0xFFFFFFF9 (-7); RuntimePhase=1; RecordGeneration=1.
> [13:26:08.202] Read Home Status PASS.
> [13:26:12.122] Load Axis started.
> [13:26:12.130] Axis loaded. Name=_LMCAxis2, Ref=2
> [13:26:12.131] Load Axis PASS.
> [13:26:16.351] Refresh Home/Encoder Maintenance Capabilities started.
> [13:26:16.363] Home/Encoder capabilities: DiagnosticsBits=0x000C633F; DiagnosticsBuild=1; BootId=0x00000014; MapRevision=0x957F101E; TW20=True; TW19=True; AdminFeatures=0x00000017
> [13:26:16.364] Refresh Home/Encoder Maintenance Capabilities PASS.
> [13:26:19.623] LMC Home Start (Current Position Zero) started.
> [13:26:19.771] LMC Home Start (Current Position Zero) FAILED: LMC_Home was not accepted. ErrorId=-31000, DetailCode=41. No terminal success is implied.
> [13:26:55.495] Read Home Status started.
> [13:26:55.503] Read Home Status PASS.
>
> 결과가 명확해졌습니다. 최신 receipt/outcome 경로는 작동했지만 Home 실행 자체가 실패했습니다.
>
> - 실패 지점: `RuntimePhase=1`
> - 직접 원인: `StopState=-7`
> - 결과: `LmcHomeExecutionFailed(38)`로 terminal quarantine
> - 두 번째 Start의 detail `41`은 그 quarantine ownership이 남아 있어 발생한 정상 차단입니다.
>
> 더 이상 Home을 누르지 마세요. 현재 boot에서는 재시도해도 안 됩니다. 앱은 종료해도 됩니다. 이제 `StopState=-7`의 정확한 source 경로와 quarantine 해제 조건을 추적해 수정하겠습니다.
>
> 앱이랑 ide는 종료했어
>
> 좋습니다. 앱과 IDE는 그대로 종료 상태로 두세요. 현재 PLC의 quarantine 상태는 재시작 전까지 건드리지 않겠습니다. 로그상 `8382700 → 8382701`의 1-count 변화 때문에 3회 안정 검증 중 exact raw-position 비교가 `-7`로 실패한 가능성이 매우 높습니다. 이 비교가 실제 API 계약보다 과도한지 vendor 자료와 현재 소스를 대조해 수정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 확인했습니다. 앱과 LASAL IDE는 그대로 닫아 두시면 됩니다. 지금은 외부 소스만 수정해서, Axis2 실패 원인인 `RawDrive` 1카운트 변화를 Home 실패로 판정하는 잘못된 조건부터 바로잡겠습니다. 완료 전에는 재실행이나 재테스트하지 마세요.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminHomeModels.cs` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminLmcHomeContractTests.cs` (+59 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+5 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+8 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+4 -4)
>
> </details>
>
> 원인은 확정됐습니다. `8382700 → 8382701`의 1카운트 변화만 실패 조건에 걸렸고, 축 정지·AxisError 0·네이티브 호출 성공·LASAL 좌표 6개 0은 모두 통과했습니다. 그래서 raw 검증을 제거하지 않고 `-1/0/+1 count`만 허용하고 `±2`부터는 계속 실패하도록 PLC와 SDK를 같은 기준으로 수정 중입니다.
>
> <details><summary>Edited 11 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminHomeModels.cs` (+3 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+3 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+29 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+28 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+5 -2)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+21 -8)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+6 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+9 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+6 -3)
>
> </details>
>
> LMC Home 수정은 정적 검증까지 끝났습니다. PLC·서비스·SDK가 모두 `-1/0/+1 count`를 같은 방식으로 판정하고, SDK Debug/Release가 각각 `1082/1082 PASS`, LASAL Home mailbox verifier가 `36/36` negative fixture를 거부했습니다. 이제 IDE 선언 변경 없이 먼저 고칠 수 있는 DS402 Home의 입장 오류 분류, timeout 경계, terminal 증거 누락을 처리하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 확인했습니다. 앱과 LASAL IDE는 닫힌 상태로 유지해 주세요. 지금은 IDE를 건드리지 않고 추적된 `.st` 소스와 검증 스크립트만 수정하겠습니다. DS402 Home gate는 계속 `FALSE`로 둔 채, 먼저 TCP admission과 종료 증거 판정을 보강하겠습니다.
>
> 현재 확인된 핵심은 두 가지입니다. 첫째, 새 LMC Home `±1 count` 수정은 소스·SDK 정적 검증까지 통과했지만 아직 PLC 빌드/다운로드 전입니다. 둘째, DS402 Home은 gate를 열기 전에 admission 거절 분류와 최종 StatusWord/ActualPosition 증거가 부족합니다. 이 두 결함을 지금 분리해서 수정 중입니다.
>
> TCP 쪽은 이제 malformed `0x7D15`를 owner 충돌로 오인하지 않고 Diagnostics로 넘기도록 수정됐습니다. 정상 형식의 Start만 `-2 → detail 41`, 그 외 admission 실패 → `detail 42`로 차단합니다. 지금 이 동작이 다시 빠지지 않도록 정적 verifier와 음성 fixture를 추가하고 있습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+83 -25)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+73 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+63 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+13 -31)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+2 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+68 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+12 -4)
>
> </details>
>
> 전체 verifier를 돌리자 새 TCP 보강 때문에 `MsgPaser`가 LASAL 안전 상한 32768바이트를 7바이트 초과한 것이 바로 잡혔습니다. 로직을 줄이지 않고 함수 안의 비필수 설명 주석만 정리해 충분한 여유를 확보하겠습니다. 이건 사용자가 전에 말한 “클래스가 너무 커서 검색이 이상해지는” 문제와 같은 경계입니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+3 -10)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+0 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+80 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+12 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+42 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+12 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+43 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+51 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+16 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+7 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -1)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+38 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+50 -0)
>
> </details>
>
> DS402 1차 외부 소스 보강은 끝났습니다. 현재 결과는 `34/34` 음성 fixture PASS, 실제 소스 targeted contract PASS, `MsgPaser` 31379/32768 bytes입니다. 전체 SourceOnly도 새 검사들을 통과했고, 별개인 기존 D5 SDO Write production gate에서만 중단됩니다. DS402 gate는 여전히 `FALSE`입니다. 이제 warm-restart reconcile과 bounded quarantine를 이어서 작업 중입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+16 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+14 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+6 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+23 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+23 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+22 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+60 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -2)
>
> </details>
>
> warm-restart 경로도 source에 반영됐습니다. 다만 prepared record가 일부만 손상된 상태에서 재개되지 않도록, 지금 record/runtime 슬롯 불변식을 더 엄격하게 교차 확인하고 verifier에 warm ordering/rollback 회귀를 추가하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+33 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+121 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+10 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+38 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+69 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+51 -2)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+9 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 확인했습니다. 앱과 LASAL IDE가 모두 종료된 상태로 유지해 주세요. 지금은 추적된 `.st` 소스와 정적 검증만 진행하고, IDE 선언 작업이 필요한 시점에 정확한 입력 항목만 다시 부탁드리겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -1)
>
> </details>
>
> 확인 결과 Elmo WPF 앱과 Elmo LASAL 프로젝트 창은 없습니다. `Lasal2` 프로세스 2개는 각각 `{OHT_PLC}`, `{EodTest_CAN}` 다른 프로젝트라서 건드리지 않고 있습니다.
>
> 현재 DS402 Home cleanup의 무한 대기 경로를 정리 중입니다. 일반 실패와 safety preemption 모두 1초 경계에서 fail-closed quarantine으로 끝내고, 미확인 SDO 토큰이나 bit-4 상태를 성공으로 지우지 않도록 검증을 추가했습니다. 아직 C78 빌드나 PLC 다운로드 단계는 아닙니다.
>
> 리뷰에서 실제 warm-interruption 결함 1개를 찾았습니다. rollback 성공 후 runtime 배열 전체를 지우고 generation을 다시 쓰는 사이에 전원이 끊기면 generation이 0으로 남아 토큰 세대가 재사용될 수 있습니다. generation 슬롯 자체를 절대 지우지 않도록 두 구간으로 나눠 초기화하는 방식으로 바로 수정하겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+10 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+6 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+21 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+9 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+55 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+12 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+9 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+0 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+75 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+43 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+28 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+4 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -2)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+11 -6)
>
> </details>
>
> DS402 cleanup 정적 계약은 이제 `54/54` negative fixture와 현재 소스 targeted contract를 통과했습니다. 다음으로 ASCII, 함수 크기, whitespace, gate 상태를 다시 확인한 뒤 IDE에서 필요한 선언만 정확히 넘기겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+1 -1)
>
> </details>
>
> 마지막 리뷰를 병렬로 돌리고 있습니다. 현재까지 gate는 계속 `FALSE`, Admin feature bit 6도 OFF이고, 전체 `SourceOnly`는 새 DS402 검사를 지난 뒤 기존 D5 승인 gate에서만 의도대로 중단했습니다. 다음 단계는 새 Network가 아니라 IDE 선언 2개와 변수 4개뿐입니다.
>
> 최종 adversarial 검토에서 verifier의 허점 3개를 찾았습니다. 실제 소스 결함은 아니지만, generation을 상수로 바꾸거나 warm-reconcile 분기 연산자를 뒤집거나 TCP 조건의 `|`/`&`를 서로 맞바꾸면 기존 검사가 놓쳤습니다. 이 세 변조를 모두 거부하도록 verifier를 강화한 뒤 IDE 작업을 부탁드리겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+27 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+32 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalDs402HomeRetirementContract.Fixture.ps1` (+19 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+20 -20)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+65 -2)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+2 -1)
>
> </details>
</details>

외부 소스 작업은 완료했습니다.

- DS402 generation slot `109` warm-interruption 결함 수정
- cleanup stage `90..99`를 1초 후 fail-closed quarantine 처리
- verifier `61/61` negative fixture PASS
- 실제 소스 adversarial 변조 `7/7` 거부 PASS
- 현재 DS402 targeted contract PASS
- LASAL 소스 4개 ASCII 및 `git diff --check` PASS
- DS402 gate는 계속 `FALSE`, C78 빌드·다운로드·실축 검증은 아직 수행하지 않음

이제 LASAL IDE에서 아래 선언만 부탁합니다. 전체 내용은 [IDE handoff Section 15](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md:1035)에 있습니다.

```text
LMCEcatInputLatch Variables
Ds402HomeDrainRequestSequence : UDINT
Ds402HomeDrainAppliedSequence : UDINT
Ds402HomeDispatchSequence : UDINT
Ds402HomeDrainMailbox : ARRAY [0..7] OF DINT

FUNCTION GLOBAL RequestDs402HomeSafetyDrain
VAR_INPUT
    OperationToken : UDINT
    AxisReference : DINT
END_VAR
VAR_OUTPUT
    Result : DINT

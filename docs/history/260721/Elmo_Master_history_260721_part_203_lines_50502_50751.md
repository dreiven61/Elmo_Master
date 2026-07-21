> 					304 custom Name
> 					305 custom Class
> 					306 custom DataType
> 					307 custom Type
> 					308 custom Required
> 					309 custom DefaultInitValue
> 					310 custom Disable RedefinitionVariableWarning
> 					311 custom Comment
> 				312 도구 모음 ID: 59392
> 					313 단추
> 					314 단추
> 			315 탭 항목 (selectable) Properties
> 			316 단추 Close
>
> The focused UI element is 3 창 ID: 59648.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window:globalThis.targetWindow,x:745,y:337,screenshotId:\"screenshot-0\"});\nawait new Promise(r=>setTimeout(r,1000));\nglobalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:false});\nnodeRepl.write(JSON.stringify({title:globalThis.state.window?.title}));\nif(globalThis.state.screenshots?.[0]?.url) await nodeRepl.emitImage(globalThis.state.screenshots[0].url);",
>   "title": "Run TCP diagnostics smoke search",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {"title":"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface"}
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> </details>
>
> LASAL IDE 재빌드는 `0 errors, 3 warnings`로 끝났고, 변경 경로에 대한 세 번의 구현 검색도 실행했습니다. 이제 검색 시점 이후 로그에서 `CInvalidArgException` 유무와 실제 검색 기록을 확인하겠습니다.
>
> 다음은 병렬로 진행합니다. 한 작업은 WPF에서 Trigger 선택 시 Ring 모드가 일관되게 선택되도록 보완하고, 다른 작업은 구현 문서·README의 capability/CRC/제한사항을 현재 코드와 맞춥니다. 동시에 D4 PLC 상태기계를 독립 검토해 숨은 오류를 잡겠습니다.
>
> 현재 D4 코드 자체를 재검토하면서 두 경계조건을 추가로 발견했습니다. Trigger 전에 사용자가 Stop하면 Ring의 가장 오래된 샘플 위치를 고정해야 하고, 이미 Trigger가 발생해 post-capture 중인 Record에는 두 번째 force-trigger를 성공 처리하면 안 됩니다. 이 두 항목을 코드와 정적 계약에 같이 보완합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st` (+17 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
>
> </details>
>
> 독립 검토에서 실제 호환성 오류가 하나 확인됐습니다. Ring이 pre-trigger 대기 중이면 오래된 샘플이 밀리면서 `StartCycle`이 정상적으로 변하는데, PC SDK가 이를 Record 전체에서 불변으로 간주해 두 번째 Status부터 예외를 낼 수 있습니다. SDK는 Ready/Uploading으로 동결된 뒤에만 시작 cycle/timestamp를 고정하도록 수정하겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st` (+10 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderModels.cs` (+41 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderProtocol.cs` (+11 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+58 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+11 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderModels.cs` (+4 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderProtocol.cs` (+5 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+3 -2)
>
> </details>
>
> 추가 검토 결과, 로컬에서 구성한 Trigger Recorder는 SDK가 `PreTriggerSamples/PostTriggerSamples`를 identity에 보존하지 않아 PLC가 잘못된 `TriggerIndex`나 샘플 수를 보내도 통과할 수 있었습니다. 구성 shape를 identity에 유지하고, 로컬 identity에 한해 `triggerIndex=pre`, TriggerComplete 시 `sampleCount=pre+1+post`를 강제하겠습니다. Adopt된 identity는 원래 설정을 모르므로 기존 완화 검증을 유지합니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderModels.cs` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderProtocol.cs` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderProtocol.cs` (+11 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderProtocol.cs` (+16 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs` (+2 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 소스 기준으로 D4 Recorder Ring/Trigger 보완분부터 다시 빌드·계약 테스트하겠습니다. 실패 항목은 바로 수정하고, LASAL IDE 재빌드와 문서 동기화까지 끝낸 뒤 목적별로 커밋하겠습니다. 고객 배포용 디렉터리는 이번 범위에서 건드리지 않습니다.
>
> 1차 점검에서 새 pre/post 형상 검증이 실제 구현과 맞지만 기존 테스트 fixture 하나가 예전 Single 완료 상태를 재사용해 1건 실패하는 것을 확인했습니다. fixture를 실제 Ring TriggerComplete 상태로 고치고, 잘못된 trigger index·sample count를 거부하는 회귀 테스트도 추가하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+37 -1)
>
> </details>
>
> 추가 리뷰에서 wire parser가 PLC가 만들 수 없는 “trigger 이후 총 샘플 수가 pre+trigger+post를 초과한 상태”를 받아들이는 경계를 찾았습니다. PLC 경로 자체의 오류는 아니지만 SDK 계약이 느슨하므로, status/header 양쪽에서 상한과 triggered Recorder의 SampleCountComplete 금지를 강제하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderProtocol.cs` (+14 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+54 -3)
>
> </details>
>
> SDK의 StartCycle 고정 시점도 한 군데 더 좁힙니다. 값이 계속 이동할 수 있는 것은 trigger 전 Ring(또는 원 구성을 모르는 adopted identity)이고, Manual/Single은 첫 샘플부터 고정되어야 하므로 그 이후 변경을 거부하도록 회귀 테스트를 넣겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderModels.cs` (+6 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderProtocol.cs` (+5 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+27 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+10 -2)
>
> </details>
>
> 현재 PC 계약 테스트는 100/100 통과했고 LASAL source-only 계약도 통과했습니다. 추가 독립 리뷰에서 나온 parser 경계 두 건까지 수정했으며, 지금은 RT ring 상태기계의 wrap·pre/post·disconnect 경계를 한 번 더 감사하고 있습니다. 그 결과를 반영한 뒤 LASAL IDE Rebuild/Link를 다시 실행합니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write({skyType: typeof sky, targetWindowType: typeof targetWindow, stateType: typeof state})",
>   "title": "LASAL IDE 상태 확인"
> }
> ```
>
> ```text
> { skyType: 'object', targetWindowType: 'object', stateType: 'object' }
> ```
>
> RT 감사에서 실제로 수정해야 할 항목이 나왔습니다. 현재 조건 trigger가 EtherCAT 입력 유효성을 보지 않아 slave offline 시 0으로 채워진 PDO가 `MaskAllClear`나 하강 edge를 가짜로 발생시킬 수 있습니다. 조건 trigger는 master/slave 상태가 유효할 때만 판정하고, invalid 구간에서는 edge 이력을 끊도록 PLC 코드를 보완하겠습니다. 명시적 `Trigger Now`는 신호 상태와 무관한 사용자 요청이므로 그대로 허용합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st` (+57 -32)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st` (+9 -7)
>
> </details>
>
> 응답 유실 orphan은 문서에 제한으로만 남기기엔 API 테스트 단계에서도 문제가 큽니다. 새 command를 늘리지 않고 기존 `AdoptRecorder(0x7E49)`의 `RecordId=0, BufferId=0`을 “현재 단일 active Recorder 검색+adopt” sentinel로 정의하겠습니다. 정상 ID 기반 Adopt는 그대로 유지하고, SDK에는 명시적인 `AdoptActiveRecorder` API를 추가해 앱 crash나 Start 응답 유실 뒤에도 BootId만으로 복구할 수 있게 하겠습니다.
>
> 복구 경로는 현재 PLC·SDK·WPF 세 계층에 반영 중입니다. `0/0` sentinel은 단일 bank에서만 허용하고, 향후 Double bank가 켜지면 모호한 자동 선택을 하지 않도록 SDK가 capability 단계에서 거부하게 했습니다. 정상 exact-ID Adopt 경로는 변경하지 않습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 소스 기준 검증은 101/101까지 통과했습니다. 남은 핵심은 최신 LASAL 변경분을 IDE에서 다시 빌드하고 `Find in Implementation` 스모크 테스트한 뒤, 변경 범위를 최종 점검해 커밋하는 것입니다. 먼저 작업 트리와 하위 검토 결과를 다시 맞춘 다음 IDE 검증으로 넘어가겠습니다.
>
> <details><summary>Called 3 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 상태 확인",
>   "code": "nodeRepl.write(await sky.get_state({window: targetWindow}))",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> sky.get_state is not a function
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 제어 API 확인",
>   "code": "nodeRepl.write(Object.keys(sky))",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   'activate_window',
>   'get_window_state',
>   'click',
>   'scroll',
>   'drag',
>   'press_key',
>   'type_text',
>   'launch_app',
>   'list_apps',
>   'list_windows',
>   'get_window',
>   'perform_secondary_action',
>   'set_value',
>   'transport'
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 화면 확인",
>   "code": "nodeRepl.write(await sky.get_window_state({window: targetWindow}))",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3084742,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,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'... 312591 more characters,
>       originX: -1928,
>       originY: -19,
>       width: 1920,
>       height: 1040
>     }
>   ],
>   accessibility: null

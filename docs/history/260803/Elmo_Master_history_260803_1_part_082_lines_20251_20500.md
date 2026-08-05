> 							17892 콘솔 트리 (selectable) Drive1
> 							17893 콘솔 트리 (selectable) Drive2
> 							17894 콘솔 트리 (selectable) Drive3
> 							17895 콘솔 트리 (selectable) Drive4
> 							17896 콘솔 트리 (selectable) RecorderStore
> 							18322 콘솔 트리 (selectable) Coupler
> 							31405 콘솔 트리 (selectable) InputSlot
> 							37416 콘솔 트리 (selectable) OutputSlot
> 						17898 콘솔 트리 (selectable) Methods
> 							71826 콘솔 트리 (selectable) Global
> 								72587 콘솔 트리 (selectable) RtWork
> 								72588 콘솔 트리 (selectable) CopySnapshot
> 									81521 콘솔 트리 (selectable) pDest
> 									81522 콘솔 트리 (selectable) DestSize
> 									81523 콘솔 트리 (selectable) Result
> 								75607 콘솔 트리 (selectable) CopyTopologyIoSnapshot
> 									79045 콘솔 트리 (selectable) pDest
> 									100473 콘솔 트리 (selectable) DestSize
> 									102799 콘솔 트리 (selectable) Result
> 								104363 콘솔 트리 (selectable) AdvanceOutputRevision
> 									105880 콘솔 트리 (selectable) Revision
> 							71827 콘솔 트리 (selectable) Private
> 						17899 콘솔 트리 (selectable) Variables
> 							44037 콘솔 트리 (selectable) PublishSequence
> 							44038 콘솔 트리 (selectable) SnapshotBytes
> 							44367 콘솔 트리 (selectable) OutputRevision
> 							61212 콘솔 트리 (selectable) OutputObserved
> 							63856 콘솔 트리 (selectable) OutputPreviousValid
> 							66427 콘솔 트리 (selectable) OutputPreviousValue
> 						17900 콘솔 트리 (selectable) Objects
> 						17901 콘솔 트리 (selectable) Dependencies
> 					162 콘솔 트리 (selectable) LMCRecorderStore
> 					163 콘솔 트리 (selectable) LMCSdoExecutor
> 					164 콘솔 트리 (selectable) TCPIPServer
> 					165 콘솔 트리 (selectable) TCPMotionInterface
> 			118604 탭 항목 (selectable) Lib
> 			118605 탭 항목 (selectable) File
> 			118606 탭 항목 (selectable) Global
> 			118607 탭 항목 (selectable) Net
> 			118608 탭 항목 (selectable) Class
> 			171 단추 Close
> 		172 Tab Properties ID: 296581344
> 			173 창 ID: 287643296
> 				174 TABLE Properties Window ID: 294249272
> 					118609 custom Name
> 					118610 custom Virtual
> 					118611 custom Global access
> 					118612 custom AWL implementation
> 					118613 custom CDecl
> 					118614 custom Comment
> 				175 도구 모음 ID: 59392
> 					176 단추
> 					177 단추
> 			118615 탭 항목 (selectable) Properties
> 			179 단추 Close
>
> The focused UI element is 143 트리 ID: 103.
> ```
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
>   "title": "Open helper member menu",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,element_index:118447,mouse_button:\"right\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect helper member menu",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.slice(0,1800));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService* Secondary Actions: Raise
> 		118991 창
> 		118992 창
> 		118993 메뉴
> 			119243 메뉴 항목 Edit Method Enter
> 			119244 메뉴 항목 Delete Method Del
> 			119245 메뉴 항목 Move Up
> 			119246 메뉴 항목 (disabled) Move Down
> 			119247 메뉴 항목 Copy
> 			119248 메뉴 항목 New Input Variable
> 			119249 메뉴 항목 New Output Variable
> 			119250 메뉴 항목 Add to Newinst
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCDiagnosticsService* Secondary Actions: Raise ID: 65282
> 				3 창 ID: 59648
> 					4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STA
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Add command id input",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:75,y:129,screenshotId:\"screenshot-2\"}); await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"CommandId\"}); await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect command id input",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.slice(-5000));"
> }
> ```
>
> ```text
> 20027 단추 Paste (Strg+V)
> 				120028 메뉴 항목 Undo (Strg+Z)
> 				120029 메뉴 항목 (disabled) Redo (Strg+Y)
> 				120030 단추 Navigate Backward (Alt+Left)
> 				120031 단추 (disabled) Navigate Forward (Alt +Right)
> 			112 메뉴 모음 Menu Bar
> 				120032 메뉴 항목 FILE
> 				120033 메뉴 항목 EDIT
> 				120034 메뉴 항목 VIEW
> 				120035 메뉴 항목 PROJECT
> 				120036 메뉴 항목 BUILD
> 				120037 메뉴 항목 DEBUG
> 				120038 메뉴 항목 ANALYZE
> 				120039 메뉴 항목 TOOLS
> 				120040 메뉴 항목 EXTRAS
> 				120041 메뉴 항목 WINDOW
> 				120042 메뉴 항목 HELP
> 		124 창 Splitter ID: 404445040
> 		125 창 Splitter ID: 404445712
> 		126 Tab Output ID: 296578152
> 			127 창 ID: 1200
> 				128 창 ID: 1200
> 					129 LIST ID: 1204
> 						130 목록 항목 (selectable)
> 						131 목록 항목 (selectable)
> 						132 목록 항목 (selectable)
> 					133 스크롤 막대 (disabled) ID: 59904
> 						134 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						135 위치 조정 위치 ID: ScrollbarThumb
> 						136 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			120043 탭 항목 (selectable) Python Script
> 			120044 탭 항목 (selectable) Debugger
> 			120045 탭 항목 (selectable) Output
> 			140 단추 Close
> 		141 창 Splitter ID: 404446216
> 		142 Tab Class View ID: 296578608
> 			143 트리 ID: 103
> 				144 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					145 단추 위쪽 스크롤 화살표 ID: UpButton
> 					17888 단추 페이지 위로 ID: UpPageButton
> 					146 위치 조정 위치 ID: ScrollbarThumb
> 					147 단추 페이지 아래로 ID: DownPageButton
> 					148 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				118523 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 					118524 단추 왼쪽 스크롤 화살표 ID: UpButton
> 					118525 위치 조정 위치 ID: ScrollbarThumb
> 					118526 단추 페이지 오른쪽으로 ID: DownPageButton
> 					118527 단추 오른쪽 스크롤 화살표 ID: DownButton
> 				118528 위치 조정 (disabled)
> 				149 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					150 콘솔 트리 (selectable) External
> 					151 콘솔 트리 (selectable) Sigmatek
> 					152 콘솔 트리 (selectable) Elmo_1
> 					153 콘솔 트리 (selectable) Elmo_2
> 					154 콘솔 트리 (selectable) Elmo_3
> 					155 콘솔 트리 (selectable) Elmo_4
> 					156 콘솔 트리 (selectable) GL_9086_1
> 					157 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					158 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					159 콘솔 트리 (selectable) LMCControlCommandService
> 					160 콘솔 트리 (selectable) LMCDiagnosticsService
> 						115922 콘솔 트리 (selectable) Servers
> 						115923 콘솔 트리 (selectable) Clients
> 						115924 콘솔 트리 (selectable) Methods
> 							116516 콘솔 트리 (selectable) Global
> 							116517 콘솔 트리 (selectable) Private
> 								118373 콘솔 트리 (selectable) LMCDiagnosticsService
> 								118374 콘솔 트리 (selectable) IsSdoReadReady
> 								118375 콘솔 트리 (selectable) GetSdoWritePolicyDetail
> 								118376 콘솔 트리 (selectable) BuildCatalogEntry
> 								118447 콘솔 트리 (selectable) HandleEtherCATTopologyIoRequest
> 									119895 콘솔 트리 (selectable) CommandId
> 						115925 콘솔 트리 (selectable) Variables
> 						115926 콘솔 트리 (selectable) Objects
> 						115927 콘솔 트리 (selectable) Dependencies
> 					161 콘솔 트리 (selectable) LMCEcatInputLatch
> 						17889 콘솔 트리 (selectable) Servers
> 						17890 콘솔 트리 (selectable) Clients
> 							17891 콘솔 트리 (selectable) EcatMaster
> 							17892 콘솔 트리 (selectable) Drive1
> 							17893 콘솔 트리 (selectable) Drive2
> 							17894 콘솔 트리 (selectable) Drive3
> 							17895 콘솔 트리 (selectable) Drive4
> 							17896 콘솔 트리 (selectable) RecorderStore
> 							18322 콘솔 트리 (selectable) Coupler
> 							31405 콘솔 트리 (selectable) InputSlot
> 							37416 콘솔 트리 (selectable) OutputSlot
> 						17898 콘솔 트리 (selectable) Methods
> 							71826 콘솔 트리 (selectable) Global
> 								72587 콘솔 트리 (selectable) RtWork
> 								72588 콘솔 트리 (selectable) CopySnapshot
> 									81521 콘솔 트리 (selectable) pDest
> 									81522 콘솔 트리 (selectable) DestSize
> 									81523 콘솔 트리 (selectable) Result
> 								75607 콘솔 트리 (selectable) CopyTopologyIoSnapshot
> 									79045 콘솔 트리 (selectable) pDest
> 									100473 콘솔 트리 (selectable) DestSize
> 									102799 콘솔 트리 (selectable) Result
> 								104363 콘솔 트리 (selectable) AdvanceOutputRevision
> 									105880 콘솔 트리 (selectable) Revision
> 							71827 콘솔 트리 (selectable) Private
> 						17899 콘솔 트리 (selectable) Variables
> 							44037 콘솔 트리 (selectable) PublishSequence
> 							44038 콘솔 트리 (selectable) SnapshotBytes

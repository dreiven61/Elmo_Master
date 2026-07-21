> node_repl.js
>
> ```json
> {
>   "title": "Drive2 클래스 목록 열기",
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:'ALT+ARROWDOWN'}); globalThis.state=null; nodeRepl.write('opened class dropdown');"
> }
> ```
>
> ```text
> unsupported key: ARROWDOWN
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 입력 상태 재확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write('refreshed');"
> }
> ```
>
> ```text
> refreshed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive2 클래스 목록 열기",
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:'Alt_L+Down'}); globalThis.state=null; nodeRepl.write('opened class dropdown');"
> }
> ```
>
> ```text
> opened class dropdown
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 클래스 목록 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; var t=String(state.accessibility?.tree||state.accessibility?.document_text||''); var i=t.lastIndexOf('custom Class'); nodeRepl.write(t.slice(Math.max(0,i),Math.min(t.length,i+5000)));"
> }
> ```
>
> ```text
> custom Class
> 					349 custom DataType
> 					350 custom Type
> 					351 custom Required
> 					352 custom DefaultInitValue
> 					353 custom Comment
> 				354 도구 모음 ID: 59392
> 					355 단추
> 					356 단추
> 			357 탭 항목 (selectable) Properties
> 			358 단추 Close
>
> The focused UI element is 1 편집.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 클래스 항목 확인",
>   "code": "var t2=String(globalThis.state.accessibility?.tree||globalThis.state.accessibility?.document_text||''); var i2=t2.lastIndexOf('ECAT_Slave_Base'); nodeRepl.write(t2.slice(Math.max(0,i2-1200),Math.min(t2.length,i2+3000)));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 편집
> 		2 LIST
> 			3 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 				4 단추 위쪽 스크롤 화살표 ID: UpButton
> 				5 단추 페이지 위로 ID: UpPageButton
> 				6 위치 조정 위치 ID: ScrollbarThumb
> 				7 단추 페이지 아래로 ID: DownPageButton
> 				8 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			9 목록 항목 (selectable) ECAT_Slave_Base
> 			10 목록 항목 (selectable) EEPROM
> 			11 목록 항목 (selectable) Elmo_1
> 			12 목록 항목 (selectable) Elmo_2
> 			13 목록 항목 (selectable) Elmo_3
> 			14 목록 항목 (selectable) Elmo_4
> 			15 목록 항목 (selectable) EtherCAT_Hub_Base
> 			16 목록 항목 (selectable) EtherCAT_PLC
> 			17 목록 항목 (selectable) GetCILInterface
> 			18 목록 항목 (selectable) Hub_Base
> 			19 목록 항목 (selectable) Hub_Base_Root
> 			20 목록 항목 (selectable) HwBase
> 			21 목록 항목 (selectable) HwBaseCDIAS
> 			22 목록 항목 (selectable) HwcLogging
> 			23 목록 항목 (selectable) HwControl
> 			24 목록 항목 (selectable) HwControlBase
> 			25 목록 항목 (selectable) HWRtPostScan
> 			26 목록 항목 (selectable) I2CCDIAS
> 			27 목록 항목 (selectable) LMCDiagnosticsService
> 			28 목록 항목 (selectable) LMCEcatInputLatch
> 			29 목록 항목 (selectable) Merker
> 			30 목록 항목 (selectable) MerkerEx
> 			31 목록 항목 (selectable) MoveSplineTable
> 			32 목록 항목 (selectable) PosController
> 			33 목록 항목 (selectable) RamFile
> 			34 목록 항목 (selectable) SafetyCDIAS_Base
> 			35 목록 항목 (selectable) SafetyManager
> 			36 목록 항목 (selectable) SafetyRoutingTables
> 			37 목록 항목 (selectable) SafetyUDP
> 			38 목록 항목 (selectable) SdiasBase
> 		39 창 작업 영역 ID: 59648
> 			40 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				41 창 ID: 59648
> 					42 창 xtpBarTop ID: 59419
> 						43 도구 모음 Hardware Editor
> 							44 단추 Hardware Editor Configuration Settings
> 							45 단추 Calculate Resources of Project
> 							46 단추 (disabled) Upload Hardware Tree from PLC
> 							47 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							48 단추 Show Detail Mode\Show Configuration Mode
> 							49 단추 Generates the ENI File of the current project
> 					50 창 ID: 59648
> 						51 트리 ID: 1
> 							52 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								53 단추 위쪽 스크롤 화살표 ID: UpButton
> 								54 단추 페이지 위로 ID: UpPageButton
> 								55 위치 조정 위치 ID: ScrollbarThumb
> 								56 단추 페이지 아래로 ID: DownPageButton
> 								57 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							58 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								59 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]->
> 								60 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								61 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								62 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								63 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								64 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								65 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								66 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								67 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								68 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								69 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								70 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								71 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								72 콘솔 트리 (selectable)
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive2를 Elmo_2로 지정",
>   "code": "await sky.click({window:globalThis.targetWindow,element_index:12}); globalThis.state=null; nodeRepl.write('selected Elmo_2');"
> }
> ```
>
> ```text
> element 12 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive2 목록 상태 갱신",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||'').slice(0,1800));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 편집
> 		2 LIST
> 			3 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 				4 단추 위쪽 스크롤 화살표 ID: UpButton
> 				5 단추 페이지 위로 ID: UpPageButton
> 				6 위치 조정 위치 ID: ScrollbarThumb
> 				7 단추 페이지 아래로 ID: DownPageButton
> 				8 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			9 목록 항목 (selectable) ECAT_Slave_Base
> 			10 목록 항목 (selectable) EEPROM
> 			11 목록 항목 (selectable) Elmo_1
> 			12 목록 항목 (selectable) Elmo_2
> 			13 목록 항목 (selectable) Elmo_3
> 			14 목록 항목 (selectable) Elmo_4
> 			15 목록 항목 (selectable) EtherCAT_Hub_Base
> 			16 목록 항목 (selectable) EtherCAT_PLC
> 			17 목록 항목 (selectable) GetCILInterface
> 			18 목록 항목 (selectable) Hub_Base
> 			19 목록 항목 (selectable) Hub_Base_Root
> 			20 목록 항목 (selectable) HwBase
> 			21 목록 항목 (selectable) HwBaseCDIAS
> 			22 목록 항목 (selectable) HwcLogging
> 			23 목록 항목 (selectable) HwControl
> 			24 목록 항목 (selectable) HwControlBase
> 			25 목록 항목 (selectable) HWRtPostScan
> 			26 목록 항목 (selectable) I2CCDIAS
> 			27 목록 항목 (selectable) LMCDiagnosticsService
> 			28 목록 항목 (selectable) LMCEcatInputLatch
> 			29 목록 항목 (selectable) Merker
> 			30 목록 항목 (selectable) MerkerEx
> 			31 목록 항목 (selectable) MoveSplineTable
> 			32 목록 항목 (selectable) PosController
> 			33 목록 항목 (selectable) RamFile
> 			34 목록 항목 (selectable) SafetyCDIAS_Base
> 			35 목록 항목 (selectable) SafetyManager
> 			36 목록 항목 (selectable) SafetyRoutingTables
> 			37 목록 항목 (selectable) SafetyUDP
> 			38 목록 항목 (selectable) SdiasBase
> 		39 창 작업 영역 ID: 59648
> 			40 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				41 창 ID: 59648
> 					42 창 xtpBarTop ID: 59419
> 						43 도구 모음 Hardware Editor
> 							44 단추 Hardware Editor Configuration Settings
> 							45 단추 Calculate Resources of P
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive2 클래스 탐색",
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:'Down'}); globalThis.state=null; nodeRepl.write('down 1');"
> }
> ```
>
> ```text
> down 1
> ```
>
> MCP tool call

> 				222 단추 Close project
> 				223 단추 Print
> 				224 단추 Cut (Strg+X)
> 				225 단추 Copy (Strg+C)
> 				226 단추 Paste (Strg+V)
> 				227 메뉴 항목 Undo (Strg+Z)
> 				228 메뉴 항목 (disabled) Redo (Strg+Y)
> 				229 단추 Navigate Backward (Alt+Left)
> 				230 단추 (disabled) Navigate Forward (Alt +Right)
> 			231 메뉴 모음 Menu Bar
> 				232 메뉴 항목 FILE
> 				233 메뉴 항목 EDIT
> 				234 메뉴 항목 VIEW
> 				235 메뉴 항목 PROJECT
> 				236 메뉴 항목 BUILD
> 				237 메뉴 항목 DEBUG
> 				238 메뉴 항목 ANALYZE
> 				239 메뉴 항목 TOOLS
> 				240 메뉴 항목 EXTRAS
> 				241 메뉴 항목 WINDOW
> 				242 메뉴 항목 HELP
> 		243 창 Splitter ID: 125724648
> 		244 창 Splitter ID: 125724480
> 		245 Tab Output ID: 295437008
> 			246 창 ID: 1200
> 				247 창 ID: 1200
> 					248 LIST ID: 1201
> 						249 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							250 단추 위쪽 스크롤 화살표 ID: UpButton
> 							251 단추 페이지 위로 ID: UpPageButton
> 							252 위치 조정 위치 ID: ScrollbarThumb
> 							253 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						254 목록 항목 (selectable)
> 						255 목록 항목 (selectable)
> 						256 목록 항목 (selectable)
> 						257 목록 항목 (selectable)
> 						258 목록 항목 (selectable)
> 					259 스크롤 막대 ID: 59904
> 						260 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						261 위치 조정 위치 ID: ScrollbarThumb
> 						262 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			263 탭 항목 (selectable) Python Script
> 			264 탭 항목 (selectable) Output
> 			265 탭 항목 (selectable) Debugger
> 			266 단추 Close
> 		267 창 Splitter ID: 125724144
> 		268 Tab Class View ID: 125483184
> 			269 트리 ID: 103
> 				270 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					271 콘솔 트리 (selectable) External
> 					272 콘솔 트리 (selectable) Sigmatek
> 					273 콘솔 트리 (selectable) _TCPIPServer_RT
> 					274 콘솔 트리 (selectable) Elmo_1
> 					275 콘솔 트리 (selectable) Elmo_2
> 					276 콘솔 트리 (selectable) Elmo_3
> 					277 콘솔 트리 (selectable) Elmo_4
> 					278 콘솔 트리 (selectable) LMCDiagnosticsService
> 					279 콘솔 트리 (selectable) LMCEcatInputLatch
> 						280 콘솔 트리 (selectable) Servers
> 						281 콘솔 트리 (selectable) Clients
> 							282 콘솔 트리 (selectable) EcatMaster
> 							283 콘솔 트리 (selectable) Drive1
> 						284 콘솔 트리 (selectable) Methods
> 						285 콘솔 트리 (selectable) Dependencies
> 					286 콘솔 트리 (selectable) TCPMotionInterface
> 			287 탭 항목 (selectable) Lib
> 			288 탭 항목 (selectable) File
> 			289 탭 항목 (selectable) Global
> 			290 탭 항목 (selectable) Net
> 			291 탭 항목 (selectable) Class
> 			292 단추 Close
> 		293 Tab Properties ID: 125485008
> 			294 창 ID: 290002192
> 				295 TABLE Properties Window ID: 293314152
> 					296 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						297 단추 위쪽 스크롤 화살표 ID: UpButton
> 						298 위치 조정 위치 ID: ScrollbarThumb
> 						299 단추 페이지 아래로 ID: DownPageButton
> 						300 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					301 custom Name
> 					302 custom DataType
> 					303 custom Type
> 					304 custom Required
> 					305 custom DefaultInitValue
> 					306 custom Comment
> 				307 도구 모음 ID: 59392
> 					308 단추
> 					309 단추
> 			310 탭 항목 (selectable) Properties
> 			311 단추 Close
>
> The focused UI element is 269 트리 ID: 103.
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
>   "title": "Drive1 채널 타입 선택",
>   "code": "await sky.click({window:globalThis.targetWindow,element_index:303}); globalThis.state=null; nodeRepl.write('type row selected');"
> }
> ```
>
> ```text
> type row selected
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 타입 행 상태 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.focused_element||''));"
> }
> ```
>
> ```text
> 295 TABLE Properties Window ID: 293314152
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
>   "title": "Drive1 타입 목록 열기",
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:'space'}); globalThis.state=null; nodeRepl.write('type list requested');"
> }
> ```
>
> ```text
> type list requested
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 타입 목록 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				3 창 ID: 59648
> 					4 창 xtpBarTop ID: 59419
> 						5 도구 모음 Hardware Editor
> 							6 단추 Hardware Editor Configuration Settings
> 							7 단추 Calculate Resources of Project
> 							8 단추 (disabled) Upload Hardware Tree from PLC
> 							9 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							10 단추 Show Detail Mode\Show Configuration Mode
> 							11 단추 Generates the ENI File of the current project
> 					12 창 ID: 59648
> 						13 트리 ID: 1
> 							14 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								15 단추 위쪽 스크롤 화살표 ID: UpButton
> 								16 단추 페이지 위로 ID: UpPageButton
> 								17 위치 조정 위치 ID: ScrollbarThumb
> 								18 단추 페이지 아래로 ID: DownPageButton
> 								19 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							20 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								21 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]->
> 								22 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								23 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								24 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								25 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								26 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								27 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								28 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								29 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								30 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								31 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								32 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								33 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								34 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								35 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								36 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								37 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								38 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								39 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								40 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 									41 콘솔 트리 (selectable) Class State (ClassState) <-[]->
> 									42 콘솔 트리 (selectable) USB:00, Empty
> 									43 콘솔 트리 (selectable) CAN:00, Empty
> 									44 콘솔 트리 (selectable) Ethernet:00:0, Empty
> 								45 콘솔 트리 (selectable) ALARM:00, Empty
> 								46 콘솔 트리 (selectable) SDIAS:00, Empty
> 								47 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 									48 콘솔 트리 (selectable) ---------------------- General -----------------------
> 									49 콘솔 트리 (selectable) Class State (ClassState) <-[]->
> 									50 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 									51 콘솔 트리 (selectable) Slave State (SlaveState) <-[]->
> 									52 콘솔 트리 (selectable) Device Address (DeviceAdress) <-[]->
> 									53 콘솔 트리 (selectable) Vendor ID (VendorID) <-[]->
> 									54 콘솔 트리 (selectable) Product Code (ProductCode) <-[]->
> 									55 콘솔 트리 (selectable) Revision No (RevisionNo) <-[]->
> 									56 콘솔 트리 (selectable) Serial No (SerialNo) <-[]->
> 									57 콘솔 트리 (selectable) Device Name (DeviceName) <-[]->
> 									58 콘솔 트리 (selectable) Product Revision (ProductRevision) <-[]->
> 									59 콘솔 트리 (selectable) AL StatusCode (AL_StatusCode) <-[]->
> 									60 콘솔 트리 (selectable) Online (Online) <-[]-> _LMCAxis1.HWReady
> 									61 콘솔 트리 (selectable) ---------------------- Axis -----------------------
> 									62 콘솔 트리 (selectable) Act Position (ActPos) <-[]-> _LMCAxis1.ActPosition
> 									63 콘솔 트리 (selectable) Set Position (SetPos) <-[]-> PosController1.oOutputSignal
> 									64 콘솔 트리 (selectable) Act Speed (ActSpeed) <-[]->
> 									65 콘솔 트리 (selectable) Set Speed (SetSpeed) <-[]->
> 									66 콘솔 트리 (selectable) Act Torque (ActTorque) <-[]->
> 									67 콘솔 트리 (selectable) Set Torque (SetTorque) <-[]->
> 									68 콘솔 트리 (selectable) Act Xw (ActXw) <-[]->
> 									69 콘솔 트리 (selectable) State Word (StateWord) <-[]->
> 									70 콘솔 트리 (selectable) Control Word (ControlWord) <-[]->
> 									71 콘솔 트리 (selectable) Axis Enable (AxEnable) <-[]-> PosController1.DriveEnabled
> 									72 콘솔 트리 (selectable) Axis Error (AxError) <-[]-> _LMCAxis1.HWError
> 									73 콘솔 트리 (selectable) Axis Error Quit (AxErrorQuit) <-[]-> PosController1.HWErrorQuit
> 									74 콘솔 트리 (selectable) Outputs_DigitalOutputs (Outputs_DigitalOutputs) <-[]->
> 									75 콘솔 트리 (selectable) Inputs_DigitalInputs (Inputs_DigitalInputs) <-[]->
> 									76 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_2 (Elmo_21)
> 								77 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							78 콘솔 트리 (selectable) Unplaced Module(s)
> 			79 창 Motion_Network Secondary Actions: Raise ID: 65284
> 				80 창 ID: 59648
> 					81 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						82 단추 위쪽 스크롤 화살표 ID: UpButton
> 						83 위치 조정 위치 ID: ScrollbarThumb
> 						84 단추 페이지 아래로 ID: DownPageButton
> 						85 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					86 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						87 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						88 위치 조정 위치 ID: ScrollbarThumb
> 						89 단추 페이지 오른쪽으로 ID: DownPageButton

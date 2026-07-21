> 					273 단추 페이지 아래로 ID: DownPageButton
> 					274 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				275 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					276 콘솔 트리 (selectable) External
> 					277 콘솔 트리 (selectable) Sigmatek
> 					278 콘솔 트리 (selectable) _TCPIPServer_RT
> 					279 콘솔 트리 (selectable) Elmo_1
> 					280 콘솔 트리 (selectable) Elmo_2
> 					281 콘솔 트리 (selectable) Elmo_3
> 					282 콘솔 트리 (selectable) Elmo_4
> 					283 콘솔 트리 (selectable) LMCDiagnosticsService
> 						284 콘솔 트리 (selectable) Servers
> 						285 콘솔 트리 (selectable) Methods
> 					286 콘솔 트리 (selectable) LMCEcatInputLatch
> 						287 콘솔 트리 (selectable) Servers
> 						288 콘솔 트리 (selectable) Clients
> 							289 콘솔 트리 (selectable) EcatMaster
> 							290 콘솔 트리 (selectable) Drive1
> 							291 콘솔 트리 (selectable) Drive2
> 							292 콘솔 트리 (selectable) Drive3
> 							293 콘솔 트리 (selectable) Drive4
> 						294 콘솔 트리 (selectable) Methods
> 						295 콘솔 트리 (selectable) Dependencies
> 					296 콘솔 트리 (selectable) TCPMotionInterface
> 			297 탭 항목 (selectable) Lib
> 			298 탭 항목 (selectable) File
> 			299 탭 항목 (selectable) Global
> 			300 탭 항목 (selectable) Net
> 			301 탭 항목 (selectable) Class
> 			302 단추 Close
> 		303 Tab Properties ID: 125485008
> 			304 창 ID: 290002192
> 				305 TABLE Properties Window ID: 293314152
> 					306 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						307 단추 위쪽 스크롤 화살표 ID: UpButton
> 						308 위치 조정 위치 ID: ScrollbarThumb
> 						309 단추 페이지 아래로 ID: DownPageButton
> 						310 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					311 custom Name
> 					312 custom Revision
> 					313 custom GUID
> 					314 custom Task Settings
> 					315 custom Sigmatek
> 					316 custom OSInterface
> 					317 custom IconPath
> 					318 custom SharedCommandTable
> 					319 custom Objectsize
> 					320 custom Singleton
> 					321 custom Hidden
> 					322 custom Deprecated
> 					323 custom GCCOptions
> 					324 custom Comment
> 					325 custom Filename
> 				326 도구 모음 ID: 59392
> 					327 단추
> 					328 단추
> 			329 탭 항목 (selectable) Properties
> 			330 단추 Close
>
> The focused UI element is 269 트리 ID: 103.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "서비스 클래스 메뉴 열기",
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:'Shift_L+F10'}); globalThis.state=null; nodeRepl.write('context menu requested');"
> }
> ```
>
> ```text
> context menu requested
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "서비스 클래스 메뉴 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||'').slice(0,3500));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 창
> 		2 창
> 		3 메뉴
> 			4 메뉴 항목 Edit Source Enter
> 			5 메뉴 항목 Edit Defines
> 			6 메뉴 항목 Delete Del
> 			7 메뉴 항목 Duplicate Class
> 			8 메뉴 항목 Derive Class
> 			9 메뉴 항목 Find Reference
> 			10 메뉴 항목 Print Class
> 			11 메뉴 항목 Export Class(es)
> 			12 메뉴 항목 Reload Class
> 			13 메뉴 항목 Re\Import MatLab Model
> 			14 메뉴 항목 Protection Setting
> 			15 메뉴 항목 New Server
> 			16 메뉴 항목 New Client
> 			17 메뉴 항목 New Method
> 			18 메뉴 항목 New Std. Methods
> 			19 메뉴 항목 New Variable
> 			20 메뉴 항목 New Type
> 			21 메뉴 항목 New Table
> 			22 메뉴 항목 Add Network
> 			23 메뉴 항목 Add File...
> 			24 메뉴 항목 Open Subfolders
> 			25 메뉴 항목 Paste
> 			26 메뉴 항목 Copy
> 		27 창 작업 영역 ID: 59648
> 			28 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				29 창 ID: 59648
> 					30 창 xtpBarTop ID: 59419
> 						31 도구 모음 Hardware Editor
> 							32 단추 Hardware Editor Configuration Settings
> 							33 단추 Calculate Resources of Project
> 							34 단추 (disabled) Upload Hardware Tree from PLC
> 							35 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							36 단추 Show Detail Mode\Show Configuration Mode
> 							37 단추 Generates the ENI File of the current project
> 					38 창 ID: 59648
> 						39 트리 ID: 1
> 							40 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								41 단추 위쪽 스크롤 화살표 ID: UpButton
> 								42 단추 페이지 위로 ID: UpPageButton
> 								43 위치 조정 위치 ID: ScrollbarThumb
> 								44 단추 페이지 아래로 ID: DownPageButton
> 								45 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							46 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								47 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]->
> 								48 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								49 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								50 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								51 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								52 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								53 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								54 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								55 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								56 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								57 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								58 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								59 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								60 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								61 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								62 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								63 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								64 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								65 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								66 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 									67 콘솔 트리 (selectable) Class State (ClassState) <-[]->
> 									68 콘솔 트리 (selectable) USB:00, Empty
> 									69 콘솔 트리 (selectable) CAN:00, Empty
> 									70 콘솔 트리 (selectable) Ethernet:00:0, Empty
> 								71 콘솔 트리 (selectable) ALARM:00, Emp
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 클라이언트 생성",
>   "code": "await sky.click({window:globalThis.targetWindow,element_index:16}); globalThis.state=null; nodeRepl.write('new client');"
> }
> ```
>
> ```text
> new client
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "새 클라이언트 속성 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; var tx6=String(state.accessibility?.tree||state.accessibility?.document_text||''); var ix6=tx6.lastIndexOf('Tab Class View'); nodeRepl.write(tx6.slice(ix6,ix6+5200));"
> }
> ```
>
> ```text
> Tab Class View ID: 125483184
> 			295 트리 ID: 103
> 				296 편집 ID: 1
> 				297 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					298 단추 위쪽 스크롤 화살표 ID: UpButton
> 					299 위치 조정 위치 ID: ScrollbarThumb
> 					300 단추 페이지 아래로 ID: DownPageButton
> 					301 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				302 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					303 콘솔 트리 (selectable) External
> 					304 콘솔 트리 (selectable) Sigmatek
> 					305 콘솔 트리 (selectable) _TCPIPServer_RT
> 					306 콘솔 트리 (selectable) Elmo_1
> 					307 콘솔 트리 (selectable) Elmo_2
> 					308 콘솔 트리 (selectable) Elmo_3
> 					309 콘솔 트리 (selectable) Elmo_4
> 					310 콘솔 트리 (selectable) LMCDiagnosticsService
> 						311 콘솔 트리 (selectable) Servers
> 						312 콘솔 트리 (selectable) Clients
> 							313 콘솔 트리 (selectable) Client
> 						314 콘솔 트리 (selectable) Methods
> 					315 콘솔 트리 (selectable) LMCEcatInputLatch
> 						316 콘솔 트리 (selectable) Servers
> 						317 콘솔 트리 (selectable) Clients
> 							318 콘솔 트리 (selectable) EcatMaster
> 							319 콘솔 트리 (selectable) Drive1
> 							320 콘솔 트리 (selectable) Drive2
> 							321 콘솔 트리 (selectable) Drive3
> 							322 콘솔 트리 (selectable) Drive4
> 						323 콘솔 트리 (selectable) Methods
> 						324 콘솔 트리 (selectable) Dependencies
> 					325 콘솔 트리 (selectable) TCPMotionInterface
> 			326 탭 항목 (selectable) Lib
> 			327 탭 항목 (selectable) File
> 			328 탭 항목 (selectable) Global
> 			329 탭 항목 (selectable) Net
> 			330 탭 항목 (selectable) Class
> 			331 단추 Close
> 		332 Tab Properties ID: 125485008
> 			333 창 ID: 290002192
> 				334 TABLE Properties Window ID: 293314152
> 					335 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						336 단추 위쪽 스크롤 화살표 ID: UpButton
> 						337 위치 조정 위치 ID: ScrollbarThumb
> 						338 단추 페이지 아래로 ID: DownPageButton
> 						339 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					340 custom Name
> 					341 custom Revision
> 					342 custom GUID
> 					343 custom Task Settings
> 					344 custom Sigmatek
> 					345 custom OSInterface
> 					346 custom IconPath
> 					347 custom SharedCommandTable
> 					348 custom Objectsize
> 					349 custom Singleton
> 					350 custom Hidden
> 					351 custom Deprecated
> 					352 custom GCCOptions

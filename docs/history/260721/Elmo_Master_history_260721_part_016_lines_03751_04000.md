> 				186 단추 (disabled) Previous bookmark
> 				187 단추 (disabled) Next bookmark
> 				188 단추 (disabled) Delete all bookmarks
> 				189 단추 (disabled) Previous bookmark in this file
> 				190 단추 (disabled) Next bookmark in this file
> 				191 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				192 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				193 단추 (disabled) Unindent (Shift+Tab)
> 				194 단추 (disabled) Indent (Tab)
> 			195 도구 모음 Macros Manager
> 				196 메뉴 항목 Macros
> 			197 도구 모음 Layout Manager
> 				198 메뉴 항목 Layouts
> 			199 도구 모음 Toolbox
> 				200 단추 DataAnalyzer
> 				201 메뉴 항목 Toolbar Options
> 			202 도구 모음 Net Edit
> 				203 단추 (disabled) Select
> 				204 메뉴 항목 Toolbar Options
> 			205 도구 모음 Debug
> 				206 단추 Go online (Alt+F6)
> 				207 단추 (disabled) Change Online Settings
> 				208 메뉴 항목 Online Connection
> 				209 단추 (disabled) Set Online Connection For Current Project
> 				210 단추 Download (F6)
> 				211 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				212 단추 Download Module on the Fly
> 				213 단추 (disabled) Save Project on PLC
> 				214 단추 (disabled) Start (F7)
> 				215 단추 Reset (F8)
> 				216 단추 (disabled) Toggle breakpoint (F4)
> 				217 단추 (disabled) Create condition breakpoint
> 				218 메뉴 항목 Toolbar Options
> 			219 도구 모음 Build
> 				220 메뉴 항목 (disabled) Target Architecture
> 				221 단추 Build changes (F9)
> 				222 단추 Rebuild project (Strg+F9)
> 				223 단추 (disabled) Cancel building (Ctrl+Break)
> 				224 단추 Link project
> 			225 도구 모음 Standard
> 				226 단추 New project (Strg+N)
> 				227 단추 Open a file (Strg+Shift+O)
> 				228 단추 Close active document (Strg+F4)
> 				229 단추 (disabled) Save file (Strg+S)
> 				230 단추 Open project (Strg+O)
> 				231 단추 (disabled) Save project changes (Strg+Shift+S)
> 				232 단추 Close project
> 				233 단추 Print
> 				234 단추 Cut (Strg+X)
> 				235 단추 Copy (Strg+C)
> 				236 단추 Paste (Strg+V)
> 				237 메뉴 항목 (disabled) Undo (Strg+Z)
> 				238 메뉴 항목 (disabled) Redo (Strg+Y)
> 				239 단추 Navigate Backward (Alt+Left)
> 				240 단추 (disabled) Navigate Forward (Alt +Right)
> 			241 메뉴 모음 Menu Bar
> 				242 메뉴 항목 FILE
> 				243 메뉴 항목 EDIT
> 				244 메뉴 항목 VIEW
> 				245 메뉴 항목 PROJECT
> 				246 메뉴 항목 BUILD
> 				247 메뉴 항목 DEBUG
> 				248 메뉴 항목 ANALYZE
> 				249 메뉴 항목 TOOLS
> 				250 메뉴 항목 EXTRAS
> 				251 메뉴 항목 WINDOW
> 				252 메뉴 항목 HELP
> 		253 창 Splitter ID: 125724648
> 		254 창 Splitter ID: 125724480
> 		255 Tab Debugger ID: 295437008
> 			256 탭 항목 (selectable) Python Script
> 			257 탭 항목 (selectable) Output
> 			258 탭 항목 (selectable) Debugger
> 			259 단추 Close
> 		260 창 Splitter ID: 125724144
> 		261 Tab Network View ID: 125483184
> 			262 트리 ID: 104
> 				263 콘솔 트리 (selectable, disabled) Elmo_EtherCAT_Test_4Axis
> 					264 콘솔 트리 (selectable, disabled) Comm_Network
> 					265 콘솔 트리 (selectable, disabled) HW_Network
> 					266 콘솔 트리 (selectable, disabled) Motion_Network
> 			267 탭 항목 (selectable) Lib
> 			268 탭 항목 (selectable) File
> 			269 탭 항목 (selectable) Global
> 			270 탭 항목 (selectable) Net
> 			271 탭 항목 (selectable) Class
> 			272 단추 Close
> 		273 Tab Properties ID: 125485008
> 			274 창 ID: 290002192
> 				275 TABLE Properties Window ID: 293314152
> 					276 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						277 단추 위쪽 스크롤 화살표 ID: UpButton
> 						278 위치 조정 위치 ID: ScrollbarThumb
> 						279 단추 페이지 아래로 ID: DownPageButton
> 						280 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					281 custom Name
> 					282 custom Revision
> 					283 custom Type
> 					284 custom Draw Connection
> 					285 custom Comment
> 					286 custom Filename
> 				287 도구 모음 ID: 59392
> 					288 단추
> 					289 단추
> 			290 탭 항목 (selectable) Properties
> 			291 단추 Close
>
> The focused UI element is 0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL DEBUG 메뉴 열기",
>   "code": "{const observation=globalThis.state;if(observation?.accessibility==null)throw new Error('No accessibility observation');globalThis.state=null;try{await sky.click({window:observation.window,element_index:247});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});}catch(error){throw new Error('Input or refresh outcome is unknown; reobserve before retrying',{cause:error});}globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));}"
> }
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
>   "title": "LASAL 오프라인 여부 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				3 창 ID: 59648
> 					4 창 xtpBarTop ID: 59419
> 						5 도구 모음 Hardware Editor
> 							6 단추 Hardware Editor Configuration Settings
> 							7 단추 Calculate Resources of Project
> 							8 단추 Upload Hardware Tree from PLC
> 							9 단추 Compare Hardware Tree of project and PLC\Reset Online Compare
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
> 							20 콘솔 트리 (selectable, disabled) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								21 콘솔 트리 (selectable, disabled) EtherCAT Class State (ClassState) <-[]-> (_ClassOk)
> 								22 콘솔 트리 (selectable, disabled) EtherCAT State (EtherCATState) <-[]-> (ECAT_STATE_OP)
> 								23 콘솔 트리 (selectable, disabled) EtherCAT Software Version (SoftwareVersion) <-[]-> ("1.8")
> 								24 콘솔 트리 (selectable, disabled) EtherCAT Synchron (Synchron) <-[]-> (1)
> 								25 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]-> (14)
> 								26 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]-> (39)
> 								27 콘솔 트리 (selectable, disabled) EtherCAT Act Realtime Time (Act_RtTime) <-[]-> (142)
> 								28 콘솔 트리 (selectable, disabled) EtherCAT Min Realtime Time (Min_RtTime) <-[]-> (8)
> 								29 콘솔 트리 (selectable, disabled) EtherCAT Max Realtime Time (Max_RtTime) <-[]-> (224)
> 								30 콘솔 트리 (selectable, disabled) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]-> (0)
> 								31 콘솔 트리 (selectable, disabled) Sdias Class State (ClassState) <-[]-> (_NoHardware)
> 								32 콘솔 트리 (selectable, disabled) Sdias Firmware Version (FirmwareVersion) <-[]-> (16#00000120)
> 								33 콘솔 트리 (selectable, disabled) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]-> (16#00000000)
> 								34 콘솔 트리 (selectable, disabled) Sdias Retry Counter (RetryCounter) <-[]-> (0)
> 								35 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]-> (0)
> 								36 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]-> (0)
> 								37 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]-> (0)
> 								38 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]-> (0)
> 								39 콘솔 트리 (selectable, disabled) Sdias Manager Option Bits (ManagerOptionBits) <-[]-> (0)
> 								40 콘솔 트리 (selectable, disabled) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 									41 콘솔 트리 (selectable, disabled) Class State (ClassState) <-[]-> (0)
> 									42 콘솔 트리 (selectable, disabled) USB:00, Empty
> 									43 콘솔 트리 (selectable, disabled) CAN:00, Empty
> 									44 콘솔 트리 (selectable, disabled) Ethernet:00:0, Empty
> 								45 콘솔 트리 (selectable, disabled) ALARM:00, Empty
> 								46 콘솔 트리 (selectable, disabled) SDIAS:00, Empty
> 								47 콘솔 트리 (selectable, disabled) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 									48 콘솔 트리 (selectable, disabled) ---------------------- General -----------------------
> 									49 콘솔 트리 (selectable, disabled) Class State (ClassState) <-[]-> (_ClassOk)
> 									50 콘솔 트리 (selectable, disabled) EtherCAT State (EtherCATState) <-[]-> (ECAT_STATE_OP)
> 									51 콘솔 트리 (selectable, disabled) Slave State (SlaveState) <-[]-> (8)
> 									52 콘솔 트리 (selectable, disabled) Device Address (DeviceAdress) <-[]-> (16#000003E9)
> 									53 콘솔 트리 (selectable, disabled) Vendor ID (VendorID) <-[]-> (154)
> 									54 콘솔 트리 (selectable, disabled) Product Code (ProductCode) <-[]-> (16#00030924)
> 									55 콘솔 트리 (selectable, disabled) Revision No (RevisionNo) <-[]-> (16#00010420)
> 									56 콘솔 트리 (selectable, disabled) Serial No (SerialNo) <-[]-> (0)
> 									57 콘솔 트리 (selectable, disabled) Device Name (DeviceName) <-[]-> ("Slave 01 (Elmo Drive )")
> 									58 콘솔 트리 (selectable, disabled) Product Revision (ProductRevision) <-[]-> ("-")
> 									59 콘솔 트리 (selectable, disabled) AL StatusCode (AL_StatusCode) <-[]-> (0)
> 									60 콘솔 트리 (selectable, disabled) Online (Online) <-[]-> _LMCAxis1.HWReady (1)
> 									61 콘솔 트리 (selectable, disabled) ---------------------- Axis -----------------------
> 									62 콘솔 트리 (selectable, disabled) Act Position (ActPos) <-[]-> _LMCAxis1.ActPosition (2597708)
> 									63 콘솔 트리 (selectable, disabled) Set Position (SetPos) <-[]-> PosController1.oOutputSignal (2597952)
> 									64 콘솔 트리 (selectable, disabled) Act Speed (ActSpeed) <-[]-> (0)
> 									65 콘솔 트리 (selectable, disabled) Set Speed (SetSpeed) <-[]-> (0)
> 									66 콘솔 트리 (selectable, disabled) Act Torque (ActTorque) <-[]-> (0)
> 									67 콘솔 트리 (selectable, disabled) Set Torque (SetTorque) <-[]-> (0)
> 									68 콘솔 트리 (selectable, disabled) Act Xw (ActXw) <-[]-> (0)
> 									69 콘솔 트리 (selectable, disabled) State Word (StateWord) <-[]-> (2#00000000000000000000001011010000)
> 									70 콘솔 트리 (selectable, disabled) Control Word (ControlWord) <-[]-> (2#00000000000000000000000000000000)
> 									71 콘솔 트리 (selectable, disabled) Axis Enable (AxEnable) <-[]-> PosController1.DriveEnabled (0)
> 									72 콘솔 트리 (selectable, disabled) Axis Error (AxError) <-[]-> _LMCAxis1.HWError (0)
> 									73 콘솔 트리 (selectable, disabled) Axis Error Quit (AxErrorQuit) <-[]-> PosController1.HWErrorQuit (0)
> 									74 콘솔 트리 (selectable, disabled) Outputs_DigitalOutputs (Outputs_DigitalOutputs) <-[]-> (0)
> 									75 콘솔 트리 (selectable, disabled) Inputs_DigitalInputs (Inputs_DigitalInputs) <-[]-> (4128776)
> 									76 콘솔 트리 (selectable, disabled) EtherCAT_ETHERNET:01, Elmo_2 (Elmo_21)
> 								77 콘솔 트리 (selectable, disabled) MULTIVARAN:00, Empty
> 							78 콘솔 트리 (selectable, disabled) Unplaced Module(s)
> 			79 창 Motion_Network [RO] Secondary Actions: Raise ID: 65284
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
> 						90 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					91 위치 조정 (disabled)
> 			92 창 HW_Network [RO] Secondary Actions: Raise ID: 65281
> 				93 창 ID: 59648
> 					94 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						95 단추 위쪽 스크롤 화살표 ID: UpButton
> 						96 위치 조정 위치 ID: ScrollbarThumb
> 						97 단추 페이지 아래로 ID: DownPageButton
> 						98 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					99 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						100 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						101 위치 조정 위치 ID: ScrollbarThumb
> 						102 단추 페이지 오른쪽으로 ID: DownPageButton
> 						103 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					104 위치 조정 (disabled)
> 			105 창 TCPMotionInterface [RO] Secondary Actions: Raise ID: 65280
> 				106 창 ID: 59648
> 					107 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if CurrentSock = dSock then CurrentSock := 0; SessionEpoch += 1; if SessionEpoch = 0 ID: 10000
> 						108 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							109 단추 위쪽 스크롤 화살표 ID: UpButton
> 							110 단추 페이지 위로 ID: UpPageButton
> 							111 위치 조정 위치 ID: ScrollbarThumb

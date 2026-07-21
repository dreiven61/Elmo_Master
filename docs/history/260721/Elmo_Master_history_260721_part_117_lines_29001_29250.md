> 			232 탭 항목 (selectable) Python Script
> 			233 탭 항목 (selectable) Output
> 			234 탭 항목 (selectable) Debugger
> 			235 단추 Close
> 		236 창 Splitter ID: 302436696
> 		237 Tab Class View ID: 297479384
> 			238 트리 ID: 103
> 				239 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					240 콘솔 트리 (selectable) External
> 					241 콘솔 트리 (selectable) Sigmatek
> 					242 콘솔 트리 (selectable) _TCPIPServer_RT
> 					243 콘솔 트리 (selectable) Elmo_1
> 					244 콘솔 트리 (selectable) Elmo_2
> 					245 콘솔 트리 (selectable) Elmo_3
> 					246 콘솔 트리 (selectable) Elmo_4
> 					247 콘솔 트리 (selectable) LMCDiagnosticsService
> 					248 콘솔 트리 (selectable) LMCEcatInputLatch
> 						249 콘솔 트리 (selectable) Servers
> 						250 콘솔 트리 (selectable) Clients
> 						251 콘솔 트리 (selectable) Methods
> 						252 콘솔 트리 (selectable) Variables
> 							253 콘솔 트리 (selectable) PublishSequence
> 						254 콘솔 트리 (selectable) Dependencies
> 					255 콘솔 트리 (selectable) TCPMotionInterface
> 			256 탭 항목 (selectable) Lib
> 			257 탭 항목 (selectable) File
> 			258 탭 항목 (selectable) Global
> 			259 탭 항목 (selectable) Net
> 			260 탭 항목 (selectable) Class
> 			261 단추 Close
> 		262 Tab Properties ID: 302395640
> 			263 창 ID: 291858232
> 				264 TABLE Properties Window ID: 297584112
> 					265 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						266 단추 위쪽 스크롤 화살표 ID: UpButton
> 						267 위치 조정 위치 ID: ScrollbarThumb
> 						268 단추 페이지 아래로 ID: DownPageButton
> 						269 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					270 custom Name
> 					271 custom Revision
> 					272 custom GUID
> 					273 custom Task Settings
> 					274 custom Sigmatek
> 					275 custom OSInterface
> 					276 custom IconPath
> 					277 custom SharedCommandTable
> 					278 custom Objectsize
> 					279 custom Singleton
> 					280 custom Hidden
> 					281 custom Deprecated
> 					282 custom GCCOptions
> 					283 custom Comment
> 					284 custom Filename
> 				285 도구 모음 ID: 59392
> 					286 단추
> 					287 단추
> 			288 탭 항목 (selectable) Properties
> 			289 단추 Close
>
> The focused UI element is 238 트리 ID: 103.
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var o17=globalThis.state;var sid17=o17.screenshots?.[0]?.id;if(!sid17)throw new Error('no screenshot');globalThis.state=null;await sky.click({window:o17.window,screenshotId:sid17,x:1604,y:658});globalThis.state=await sky.get_window_state({window:o17.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.focused_element||''));",
>   "title": "snapshot 배열 변수 생성"
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var o18=globalThis.state;globalThis.state=null;await sky.press_key({window:o18.window,key:'Escape'});globalThis.state=await sky.get_window_state({window:o18.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "title": "잘못 연 새 메서드 취소"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				3 창 ID: 59648
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if CurrentSock = dSock then CurrentSock := 0; SessionEpoch += 1; if SessionEpoch = 0 ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 단추 페이지 위로 ID: UpPageButton
> 							8 위치 조정 위치 ID: ScrollbarThumb
> 							9 단추 페이지 아래로 ID: DownPageButton
> 							10 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						11 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							12 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							13 위치 조정 위치 ID: ScrollbarThumb
> 							14 단추 페이지 오른쪽으로 ID: DownPageButton
> 							15 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						16 위치 조정 (disabled)
> 			17 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				18 창 ID: 59648
> 					19 창 xtpBarTop ID: 59419
> 						20 도구 모음 Hardware Editor
> 							21 단추 Hardware Editor Configuration Settings
> 							22 단추 Calculate Resources of Project
> 							23 단추 (disabled) Upload Hardware Tree from PLC
> 							24 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							25 단추 Show Detail Mode\Show Configuration Mode
> 							26 단추 Generates the ENI File of the current project
> 					27 창 ID: 59648
> 						28 트리 ID: 1
> 							29 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								30 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]->
> 								31 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								32 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								33 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								34 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								35 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								36 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								37 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								38 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								39 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								40 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								41 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								42 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								43 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								44 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								45 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								46 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								47 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								48 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								49 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								50 콘솔 트리 (selectable) ALARM:00, Empty
> 								51 콘솔 트리 (selectable) SDIAS:00, Empty
> 								52 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								53 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							54 콘솔 트리 (selectable) Unplaced Module(s)
> 			55 창 Elmo_4 Secondary Actions: Raise ID: 65285
> 				56 창 ID: 59648
> 					57 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						58 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							59 단추 위쪽 스크롤 화살표 ID: UpButton
> 							60 위치 조정 위치 ID: ScrollbarThumb
> 							61 단추 페이지 아래로 ID: DownPageButton
> 							62 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						63 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							64 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							65 위치 조정 위치 ID: ScrollbarThumb
> 							66 단추 페이지 오른쪽으로 ID: DownPageButton
> 							67 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						68 위치 조정 (disabled)
> 			69 창 Motion_Network Secondary Actions: Raise ID: 65284
> 				70 창 ID: 59648
> 					71 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						72 단추 위쪽 스크롤 화살표 ID: UpButton
> 						73 위치 조정 위치 ID: ScrollbarThumb
> 						74 단추 페이지 아래로 ID: DownPageButton
> 						75 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					76 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						77 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						78 위치 조정 위치 ID: ScrollbarThumb
> 						79 단추 페이지 오른쪽으로 ID: DownPageButton
> 						80 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					81 위치 조정 (disabled)
> 			82 창 Comm_Network Secondary Actions: Raise ID: 65283
> 				83 창 ID: 59648
> 					84 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						85 단추 위쪽 스크롤 화살표 ID: UpButton
> 						86 위치 조정 위치 ID: ScrollbarThumb
> 						87 단추 페이지 아래로 ID: DownPageButton
> 						88 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			89 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				90 창 ID: 59648
> 					91 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						92 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							93 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							94 위치 조정 위치 ID: ScrollbarThumb
> 							95 단추 페이지 오른쪽으로 ID: DownPageButton
> 							96 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			97 창 HW_Network Secondary Actions: Raise ID: 65281
> 				98 창 ID: 59648
> 					99 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						100 단추 위쪽 스크롤 화살표 ID: UpButton
> 						101 위치 조정 위치 ID: ScrollbarThumb
> 						102 단추 페이지 아래로 ID: DownPageButton
> 						103 단추 아래쪽 스크롤 화살표 ID: DownButton
> 		104 상태 표시줄 ID: 59393
> 			105 텍스트
> 			106 텍스트
> 			107 텍스트
> 			108 텍스트
> 			109 텍스트
> 			110 텍스트 Offline
> 			111 텍스트
> 			112 텍스트 NUM
> 			113 텍스트
> 		114 창 xtpBarTop ID: 59419
> 			115 도구 모음 Script
> 			116 도구 모음 Edit
> 				117 단추 Toggle bookmark
> 				118 단추 (disabled) Previous bookmark
> 				119 단추 (disabled) Next bookmark
> 				120 단추 (disabled) Delete all bookmarks
> 				121 단추 (disabled) Previous bookmark in this file
> 				122 단추 (disabled) Next bookmark in this file
> 				123 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				124 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				125 단추 (disabled) Unindent (Shift+Tab)
> 				126 단추 (disabled) Indent (Tab)
> 			127 도구 모음 Macros Manager
> 				128 메뉴 항목 Macros
> 			129 도구 모음 Layout Manager
> 				130 메뉴 항목 Layouts
> 			131 도구 모음 Toolbox
> 				132 단추 DataAnalyzer
> 				133 단추 Interpreter
> 				134 단추 DiasDrive
> 				135 단추 PLC Diagnosis
> 				136 단추 Hardware Editor
> 				137 단추 Graphical Hardware Editor
> 				138 단추 Connection Manager
> 				139 단추 Task Configuration
> 			140 도구 모음 Net Edit
> 				141 단추 (disabled) Select
> 				142 단추 (disabled) Move view
> 				143 단추 (disabled) Insert comment
> 				144 단추 (disabled) Zoom(+/-)
> 				145 단추 (disabled) Zoom to all
> 				146 단추 (disabled) Zoom selection
> 			147 도구 모음 Debug
> 				148 단추 Go online (Alt+F6)
> 				149 단추 Change Online Settings
> 				150 메뉴 항목 Online Connection
> 				151 단추 (disabled) Set Online Connection For Current Project
> 				152 단추 (disabled) Download (F6)
> 				153 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)

> 								77 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							78 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								79 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								80 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								81 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								82 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								83 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								84 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								85 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								86 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								87 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								88 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								89 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								90 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								91 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								92 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								93 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								94 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								95 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								96 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								97 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								98 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								99 콘솔 트리 (selectable) ALARM:00, Empty
> 								100 콘솔 트리 (selectable) SDIAS:00, Empty
> 								101 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								102 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							103 콘솔 트리 (selectable) Unplaced Module(s)
> 			104 창 Elmo_4 Secondary Actions: Raise ID: 65284
> 				105 창 ID: 59648
> 					106 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						107 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							108 단추 위쪽 스크롤 화살표 ID: UpButton
> 							109 위치 조정 위치 ID: ScrollbarThumb
> 							110 단추 페이지 아래로 ID: DownPageButton
> 							111 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						112 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							113 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							114 위치 조정 위치 ID: ScrollbarThumb
> 							115 단추 페이지 오른쪽으로 ID: DownPageButton
> 							116 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						117 위치 조정 (disabled)
> 			118 창 Motion_Network Secondary Actions: Raise ID: 65283
> 				119 창 ID: 59648
> 					120 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						121 단추 위쪽 스크롤 화살표 ID: UpButton
> 						122 위치 조정 위치 ID: ScrollbarThumb
> 						123 단추 페이지 아래로 ID: DownPageButton
> 						124 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					125 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						126 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						127 위치 조정 위치 ID: ScrollbarThumb
> 						128 단추 페이지 오른쪽으로 ID: DownPageButton
> 						129 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					130 위치 조정 (disabled)
> 			131 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				132 창 ID: 59648
> 					133 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						134 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							135 단추 위쪽 스크롤 화살표 ID: UpButton
> 							136 위치 조정 위치 ID: ScrollbarThumb
> 							137 단추 페이지 아래로 ID: DownPageButton
> 							138 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						139 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							140 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							141 위치 조정 위치 ID: ScrollbarThumb
> 							142 단추 페이지 오른쪽으로 ID: DownPageButton
> 							143 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						144 위치 조정 (disabled)
> 			145 창 HW_Network Secondary Actions: Raise ID: 65281
> 				146 창 ID: 59648
> 					147 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						148 단추 위쪽 스크롤 화살표 ID: UpButton
> 						149 위치 조정 위치 ID: ScrollbarThumb
> 						150 단추 페이지 아래로 ID: DownPageButton
> 						151 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					152 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						153 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						154 위치 조정 위치 ID: ScrollbarThumb
> 						155 단추 페이지 오른쪽으로 ID: DownPageButton
> 						156 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					157 위치 조정 (disabled)
> 		158 상태 표시줄 ID: 59393
> 			159 텍스트
> 			160 텍스트 TCPMotionInterface::MsgPaser
> 			161 텍스트
> 			162 텍스트 Ln 498 Col 31
> 			163 텍스트
> 			164 텍스트 Offline
> 			165 텍스트
> 			166 텍스트 NUM
> 			167 텍스트
> 		168 창 xtpBarTop ID: 59419
> 			169 도구 모음 Script
> 			170 도구 모음 Edit
> 				171 단추 Toggle bookmark
> 				172 단추 (disabled) Previous bookmark
> 				173 단추 (disabled) Next bookmark
> 				174 단추 (disabled) Delete all bookmarks
> 				175 단추 (disabled) Previous bookmark in this file
> 				176 단추 (disabled) Next bookmark in this file
> 				177 단추 Comment selected text (Ctrl+Shift+C)
> 				178 단추 Remove comment (Ctrl+Shift+X)
> 				179 단추 Unindent (Shift+Tab)
> 				180 단추 Indent (Tab)
> 			181 도구 모음 Macros Manager
> 				182 메뉴 항목 Macros
> 			183 도구 모음 Layout Manager
> 				184 메뉴 항목 Layouts
> 			185 도구 모음 Toolbox
> 				186 단추 DataAnalyzer
> 				187 메뉴 항목 Toolbar Options
> 			188 도구 모음 Net Edit
> 				189 단추 (disabled) Select
> 				190 메뉴 항목 Toolbar Options
> 			191 도구 모음 Debug
> 				192 단추 Go online (Alt+F6)
> 				193 단추 Change Online Settings
> 				194 메뉴 항목 Online Connection
> 				195 단추 (disabled) Set Online Connection For Current Project
> 				196 단추 (disabled) Download (F6)
> 				197 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				198 단추 (disabled) Download Module on the Fly
> 				199 단추 (disabled) Save Project on PLC
> 				200 단추 (disabled) Start (F7)
> 				201 단추 (disabled) Reset (F8)
> 				202 단추 Toggle breakpoint (F4)
> 				203 단추 Create condition breakpoint
> 				204 메뉴 항목 Toolbar Options
> 			205 도구 모음 Build
> 				206 메뉴 항목 Target Architecture
> 				207 단추 Build changes (F9)
> 				208 단추 Rebuild project (Strg+F9)
> 				209 단추 (disabled) Cancel building (Ctrl+Break)
> 				210 단추 Link project
> 			211 도구 모음 Standard
> 				212 단추 New project (Strg+N)
> 				213 단추 Open a file (Strg+Shift+O)
> 				214 단추 Close active document (Strg+F4)
> 				215 단추 (disabled) Save file (Strg+S)
> 				216 단추 Open project (Strg+O)
> 				217 단추 (disabled) Save project changes (Strg+Shift+S)
> 				218 단추 Close project
> 				219 단추 Print
> 				220 단추 Cut (Strg+X)
> 				221 단추 Copy (Strg+C)
> 				222 단추 (disabled) Paste (Strg+V)
> 				223 메뉴 항목 (disabled) Undo (Strg+Z)
> 				224 메뉴 항목 (disabled) Redo (Strg+Y)
> 				225 단추 Navigate Backward (Alt+Left)
> 				226 단추 (disabled) Navigate Forward (Alt +Right)
> 			227 메뉴 모음 Menu Bar
> 				228 메뉴 항목 FILE
> 				229 메뉴 항목 EDIT
> 				230 메뉴 항목 VIEW
> 				231 메뉴 항목 PROJECT
> 				232 메뉴 항목 BUILD
> 				233 메뉴 항목 DEBUG
> 				234 메뉴 항목 ANALYZE
> 				235 메뉴 항목 TOOLS
> 				236 메뉴 항목 EXTRAS
> 				237 메뉴 항목 WINDOW
> 				238 메뉴 항목 HELP
> 		239 창 Splitter ID: 481389416
> 		240 창 Splitter ID: 481390928
> 		241 Tab Output ID: 409867992
> 			242 창 ID: 1200
> 				243 창 ID: 1200
> 					244 LIST ID: 1204
> 						245 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							246 단추 위쪽 스크롤 화살표 ID: UpButton
> 							247 단추 페이지 위로 ID: UpPageButton
> 							248 위치 조정 위치 ID: ScrollbarThumb
> 							249 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						250 목록 항목 (selectable)
> 						251 목록 항목 (selectable)
> 						252 목록 항목 (selectable)
> 						253 목록 항목 (selectable)
> 						254 목록 항목 (selectable)
> 						255 목록 항목 (selectable)
> 						256 목록 항목 (selectable)
> 						257 목록 항목 (selectable)
> 						258 목록 항목 (selectable)
> 						259 목록 항목 (selectable)
> 					260 스크롤 막대 ID: 59904
> 						261 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						262 위치 조정 위치 ID: ScrollbarThumb
> 						263 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			264 탭 항목 (selectable) Python Script
> 			265 탭 항목 (selectable) Debugger
> 			266 탭 항목 (selectable) Output
> 			267 단추 Close
> 		268 창 Splitter ID: 608798568
> 		269 Tab Global View ID: 409868448
> 			270 트리 ID: 105
> 				271 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					272 콘솔 트리 (selectable) Types
> 					273 콘솔 트리 (selectable) Variables
> 			274 탭 항목 (selectable) Lib
> 			275 탭 항목 (selectable) File
> 			276 탭 항목 (selectable) Global
> 			277 단추 Close
> 		278 Tab Properties ID: 409871640
> 			279 창 ID: 121918456
> 				280 TABLE Properties Window ID: 127184512
> 				281 도구 모음 ID: 59392
> 					282 단추
> 					283 단추
> 			284 탭 항목 (selectable) Properties
> 			285 단추 Close
>
> The focused UI element is 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000.
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
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"Need fresh accessibility state\");\n  globalThis.state = null;\n  await sky.click({window: observation.window, element_index: 218});\n  globalThis.state = await sky.get_window_state({window: observation.window, include_screenshot: true, include_text: true});\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}",
>   "title": "LASAL 프로젝트 다시 불러오기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				3 창 ID: 59648
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
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
> 			17 창 Comm_Network Secondary Actions: Raise ID: 65288
> 				18 창 ID: 59648

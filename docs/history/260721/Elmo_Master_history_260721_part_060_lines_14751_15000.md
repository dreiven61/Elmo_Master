> 			99 창 HW_Network Secondary Actions: Raise ID: 65281
> 				100 창 ID: 59648
> 					101 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						102 단추 위쪽 스크롤 화살표 ID: UpButton
> 						103 위치 조정 위치 ID: ScrollbarThumb
> 						104 단추 페이지 아래로 ID: DownPageButton
> 						105 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					106 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						107 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						108 위치 조정 위치 ID: ScrollbarThumb
> 						109 단추 페이지 오른쪽으로 ID: DownPageButton
> 						110 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					111 위치 조정 (disabled)
> 			112 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				113 창 ID: 59648
> 					114 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if CurrentSock = dSock then CurrentSock := 0; SessionEpoch += 1; if SessionEpoch = 0 ID: 10000
> 						115 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							116 단추 위쪽 스크롤 화살표 ID: UpButton
> 							117 단추 페이지 위로 ID: UpPageButton
> 							118 위치 조정 위치 ID: ScrollbarThumb
> 							119 단추 페이지 아래로 ID: DownPageButton
> 							120 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						121 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							122 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							123 위치 조정 위치 ID: ScrollbarThumb
> 							124 단추 페이지 오른쪽으로 ID: DownPageButton
> 							125 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						126 위치 조정 (disabled)
> 			127 창 Comm_Network Secondary Actions: Raise ID: 65283
> 				128 창 ID: 59648
> 					129 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						130 단추 위쪽 스크롤 화살표 ID: UpButton
> 						131 위치 조정 위치 ID: ScrollbarThumb
> 						132 단추 페이지 아래로 ID: DownPageButton
> 						133 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					134 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						135 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						136 위치 조정 위치 ID: ScrollbarThumb
> 						137 단추 페이지 오른쪽으로 ID: DownPageButton
> 						138 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					139 위치 조정 (disabled)
> 			140 창 Elmo_4 Secondary Actions: Raise ID: 65285
> 				141 창 ID: 59648
> 					142 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						143 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							144 단추 위쪽 스크롤 화살표 ID: UpButton
> 							145 위치 조정 위치 ID: ScrollbarThumb
> 							146 단추 페이지 아래로 ID: DownPageButton
> 							147 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						148 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							149 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							150 위치 조정 위치 ID: ScrollbarThumb
> 							151 단추 페이지 오른쪽으로 ID: DownPageButton
> 							152 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						153 위치 조정 (disabled)
> 			154 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				155 창 ID: 59648
> 					156 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						157 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							158 단추 위쪽 스크롤 화살표 ID: UpButton
> 							159 위치 조정 위치 ID: ScrollbarThumb
> 							160 단추 페이지 아래로 ID: DownPageButton
> 							161 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						162 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							163 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							164 위치 조정 위치 ID: ScrollbarThumb
> 							165 단추 페이지 오른쪽으로 ID: DownPageButton
> 							166 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						167 위치 조정 (disabled)
> 		168 상태 표시줄 ID: 59393
> 			169 텍스트
> 			170 텍스트
> 			171 텍스트
> 			172 텍스트
> 			173 텍스트
> 			174 텍스트
> 			175 텍스트
> 			176 텍스트
> 			177 텍스트 NUM
> 			178 텍스트
> 		179 창 xtpBarTop ID: 59419
> 			180 도구 모음 Script
> 			181 도구 모음 Edit
> 				182 단추 (disabled) Toggle bookmark
> 				183 단추 (disabled) Previous bookmark
> 				184 단추 (disabled) Next bookmark
> 				185 단추 (disabled) Delete all bookmarks
> 				186 단추 (disabled) Previous bookmark in this file
> 				187 단추 (disabled) Next bookmark in this file
> 				188 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				189 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				190 단추 (disabled) Unindent (Shift+Tab)
> 				191 단추 (disabled) Indent (Tab)
> 			192 도구 모음 Macros Manager
> 				193 메뉴 항목 Macros
> 			194 도구 모음 Layout Manager
> 				195 메뉴 항목 Layouts
> 			196 도구 모음 Toolbox
> 				197 단추 DataAnalyzer
> 				198 메뉴 항목 Toolbar Options
> 			199 도구 모음 Net Edit
> 				200 단추 (disabled) Select
> 				201 메뉴 항목 Toolbar Options
> 			202 도구 모음 Debug
> 				203 단추 Go online (Alt+F6)
> 				204 단추 Change Online Settings
> 				205 메뉴 항목 Online Connection
> 				206 단추 (disabled) Set Online Connection For Current Project
> 				207 단추 (disabled) Download (F6)
> 				208 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				209 단추 (disabled) Download Module on the Fly
> 				210 단추 (disabled) Save Project on PLC
> 				211 단추 (disabled) Start (F7)
> 				212 단추 (disabled) Reset (F8)
> 				213 단추 (disabled) Toggle breakpoint (F4)
> 				214 단추 (disabled) Create condition breakpoint
> 				215 메뉴 항목 Toolbar Options
> 			216 도구 모음 Build
> 				217 메뉴 항목 Target Architecture
> 				218 단추 Build changes (F9)
> 				219 단추 Rebuild project (Strg+F9)
> 				220 단추 (disabled) Cancel building (Ctrl+Break)
> 				221 단추 Link project
> 			222 도구 모음 Standard
> 				223 단추 New project (Strg+N)
> 				224 단추 Open a file (Strg+Shift+O)
> 				225 단추 Close active document (Strg+F4)
> 				226 단추 (disabled) Save file (Strg+S)
> 				227 단추 Open project (Strg+O)
> 				228 단추 (disabled) Save project changes (Strg+Shift+S)
> 				229 단추 Close project
> 				230 단추 Print
> 				231 단추 Cut (Strg+X)
> 				232 단추 Copy (Strg+C)
> 				233 단추 Paste (Strg+V)
> 				234 메뉴 항목 Undo (Strg+Z)
> 				235 메뉴 항목 (disabled) Redo (Strg+Y)
> 				236 단추 Navigate Backward (Alt+Left)
> 				237 단추 (disabled) Navigate Forward (Alt +Right)
> 			238 메뉴 모음 Menu Bar
> 				239 메뉴 항목 FILE
> 				240 메뉴 항목 EDIT
> 				241 메뉴 항목 VIEW
> 				242 메뉴 항목 PROJECT
> 				243 메뉴 항목 BUILD
> 				244 메뉴 항목 DEBUG
> 				245 메뉴 항목 ANALYZE
> 				246 메뉴 항목 TOOLS
> 				247 메뉴 항목 EXTRAS
> 				248 메뉴 항목 WINDOW
> 				249 메뉴 항목 HELP
> 		250 창 Splitter ID: 125724648
> 		251 창 Splitter ID: 125724480
> 		252 Tab Output ID: 295437008
> 			253 창 ID: 1200
> 				254 창 ID: 1200
> 					255 LIST ID: 1201
> 						256 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							257 단추 위쪽 스크롤 화살표 ID: UpButton
> 							258 단추 페이지 위로 ID: UpPageButton
> 							259 위치 조정 위치 ID: ScrollbarThumb
> 							260 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						261 목록 항목 (selectable)
> 						262 목록 항목 (selectable)
> 						263 목록 항목 (selectable)
> 						264 목록 항목 (selectable)
> 						265 목록 항목 (selectable)
> 					266 스크롤 막대 ID: 59904
> 						267 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						268 위치 조정 위치 ID: ScrollbarThumb
> 						269 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			270 탭 항목 (selectable) Python Script
> 			271 탭 항목 (selectable) Output
> 			272 탭 항목 (selectable) Debugger
> 			273 단추 Close
> 		274 창 Splitter ID: 125724144
> 		275 Tab Class View ID: 125483184
> 			276 트리 ID: 103
> 				277 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					278 콘솔 트리 (selectable) External
> 					279 콘솔 트리 (selectable) Sigmatek
> 					280 콘솔 트리 (selectable) _TCPIPServer_RT
> 					281 콘솔 트리 (selectable) Elmo_1
> 					282 콘솔 트리 (selectable) Elmo_2
> 					283 콘솔 트리 (selectable) Elmo_3
> 					284 콘솔 트리 (selectable) Elmo_4
> 					285 콘솔 트리 (selectable) LMCDiagnosticsService
> 					286 콘솔 트리 (selectable) LMCEcatInputLatch
> 						287 콘솔 트리 (selectable) Servers
> 						288 콘솔 트리 (selectable) Clients
> 							289 콘솔 트리 (selectable) EcatMaster
> 						290 콘솔 트리 (selectable) Methods
> 						291 콘솔 트리 (selectable) Dependencies
> 					292 콘솔 트리 (selectable) TCPMotionInterface
> 			293 탭 항목 (selectable) Lib
> 			294 탭 항목 (selectable) File
> 			295 탭 항목 (selectable) Global
> 			296 탭 항목 (selectable) Net
> 			297 탭 항목 (selectable) Class
> 			298 단추 Close
> 		299 Tab Properties ID: 125485008
> 			300 창 ID: 290002192
> 				301 TABLE Properties Window ID: 293314152
> 				302 도구 모음 ID: 59392
> 					303 단추
> 					304 단추
> 			305 탭 항목 (selectable) Properties
> 			306 단추 Close
>
> The focused UI element is 276 트리 ID: 103.
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
>   "title": "LASAL Drive1 이름 입력",
>   "code": "await sky.type_text({window:globalThis.targetWindow,text:'Drive1'}); globalThis.state=null; nodeRepl.write('typed Drive1');"
> }
> ```
>
> ```text
> typed Drive1
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 이름 입력 상태 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.focused_element||''));"
> }
> ```
>
> ```text
> 270 편집 ID: 1
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>

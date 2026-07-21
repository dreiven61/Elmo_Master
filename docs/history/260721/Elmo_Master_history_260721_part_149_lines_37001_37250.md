> 				88 창 ID: 59648
> 					89 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						90 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							91 단추 위쪽 스크롤 화살표 ID: UpButton
> 							92 위치 조정 위치 ID: ScrollbarThumb
> 							93 단추 페이지 아래로 ID: DownPageButton
> 							94 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						95 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							96 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							97 위치 조정 위치 ID: ScrollbarThumb
> 							98 단추 페이지 오른쪽으로 ID: DownPageButton
> 							99 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						100 위치 조정 (disabled)
> 			101 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				102 창 ID: 59648
> 					103 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						104 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							105 단추 위쪽 스크롤 화살표 ID: UpButton
> 							106 위치 조정 위치 ID: ScrollbarThumb
> 							107 단추 페이지 아래로 ID: DownPageButton
> 							108 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						109 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							110 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							111 위치 조정 위치 ID: ScrollbarThumb
> 							112 단추 페이지 오른쪽으로 ID: DownPageButton
> 							113 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						114 위치 조정 (disabled)
> 			115 창 HW_Network Secondary Actions: Raise ID: 65281
> 				116 창 ID: 59648
> 					117 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						118 단추 위쪽 스크롤 화살표 ID: UpButton
> 						119 위치 조정 위치 ID: ScrollbarThumb
> 						120 단추 페이지 아래로 ID: DownPageButton
> 						121 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					122 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						123 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						124 위치 조정 위치 ID: ScrollbarThumb
> 						125 단추 페이지 오른쪽으로 ID: DownPageButton
> 						126 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					127 위치 조정 (disabled)
> 		128 상태 표시줄 ID: 59393
> 			129 텍스트
> 			130 텍스트 TCPMotionInterface::CyWork
> 			131 텍스트
> 			132 텍스트 Ln 83 Col 28
> 			133 텍스트
> 			134 텍스트 Offline
> 			135 텍스트
> 			136 텍스트 NUM
> 			137 텍스트
> 		138 창 xtpBarTop ID: 59419
> 			139 도구 모음 Script
> 			140 도구 모음 Edit
> 				141 단추 Toggle bookmark
> 				142 단추 (disabled) Previous bookmark
> 				143 단추 (disabled) Next bookmark
> 				144 단추 (disabled) Delete all bookmarks
> 				145 단추 (disabled) Previous bookmark in this file
> 				146 단추 (disabled) Next bookmark in this file
> 				147 단추 Comment selected text (Ctrl+Shift+C)
> 				148 단추 Remove comment (Ctrl+Shift+X)
> 				149 단추 Unindent (Shift+Tab)
> 				150 단추 Indent (Tab)
> 			151 도구 모음 Macros Manager
> 				152 메뉴 항목 Macros
> 			153 도구 모음 Layout Manager
> 				154 메뉴 항목 Layouts
> 			155 도구 모음 Toolbox
> 				156 단추 DataAnalyzer
> 				157 메뉴 항목 Toolbar Options
> 			158 도구 모음 Net Edit
> 				159 단추 (disabled) Select
> 				160 메뉴 항목 Toolbar Options
> 			161 도구 모음 Debug
> 				162 단추 Go online (Alt+F6)
> 				163 단추 Change Online Settings
> 				164 메뉴 항목 Online Connection
> 				165 단추 (disabled) Set Online Connection For Current Project
> 				166 단추 (disabled) Download (F6)
> 				167 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				168 단추 (disabled) Download Module on the Fly
> 				169 단추 (disabled) Save Project on PLC
> 				170 단추 (disabled) Start (F7)
> 				171 단추 (disabled) Reset (F8)
> 				172 단추 Toggle breakpoint (F4)
> 				173 단추 Create condition breakpoint
> 				174 메뉴 항목 Toolbar Options
> 			175 도구 모음 Build
> 				176 메뉴 항목 Target Architecture
> 				177 단추 Build changes (F9)
> 				178 단추 Rebuild project (Strg+F9)
> 				179 단추 (disabled) Cancel building (Ctrl+Break)
> 				180 단추 Link project
> 			181 도구 모음 Standard
> 				182 단추 New project (Strg+N)
> 				183 단추 Open a file (Strg+Shift+O)
> 				184 단추 Close active document (Strg+F4)
> 				185 단추 (disabled) Save file (Strg+S)
> 				186 단추 Open project (Strg+O)
> 				187 단추 (disabled) Save project changes (Strg+Shift+S)
> 				188 단추 Close project
> 				189 단추 Print
> 				190 단추 Cut (Strg+X)
> 				191 단추 Copy (Strg+C)
> 				192 단추 (disabled) Paste (Strg+V)
> 				193 메뉴 항목 (disabled) Undo (Strg+Z)
> 				194 메뉴 항목 (disabled) Redo (Strg+Y)
> 				195 단추 Navigate Backward (Alt+Left)
> 				196 단추 (disabled) Navigate Forward (Alt +Right)
> 			197 메뉴 모음 Menu Bar
> 				198 메뉴 항목 FILE
> 				199 메뉴 항목 EDIT
> 				200 메뉴 항목 VIEW
> 				201 메뉴 항목 PROJECT
> 				202 메뉴 항목 BUILD
> 				203 메뉴 항목 DEBUG
> 				204 메뉴 항목 ANALYZE
> 				205 메뉴 항목 TOOLS
> 				206 메뉴 항목 EXTRAS
> 				207 메뉴 항목 WINDOW
> 				208 메뉴 항목 HELP
> 		209 창 Splitter ID: 411855768
> 		210 창 Splitter ID: 411851736
> 		211 Tab Output ID: 409867992
> 			212 창 ID: 1200
> 				213 창 ID: 1200
> 					214 LIST ID: 1204
> 					215 스크롤 막대 (disabled) ID: 59904
> 						216 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						217 위치 조정 위치 ID: ScrollbarThumb
> 						218 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			219 탭 항목 (selectable) Python Script
> 			220 탭 항목 (selectable) Output
> 			221 탭 항목 (selectable) Debugger
> 			222 단추 Close
> 		223 창 Splitter ID: 411854424
> 		224 Tab Class View ID: 409868448
> 			225 트리 ID: 103
> 				226 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					227 콘솔 트리 (selectable) External
> 					228 콘솔 트리 (selectable) Sigmatek
> 					229 콘솔 트리 (selectable) _TCPIPServer_RT
> 					230 콘솔 트리 (selectable) Elmo_1
> 					231 콘솔 트리 (selectable) Elmo_2
> 					232 콘솔 트리 (selectable) Elmo_3
> 					233 콘솔 트리 (selectable) Elmo_4
> 					234 콘솔 트리 (selectable) LMCDiagnosticsService
> 					235 콘솔 트리 (selectable) LMCEcatInputLatch
> 					236 콘솔 트리 (selectable) LMCRecorderStore
> 					237 콘솔 트리 (selectable) TCPMotionInterface
> 			238 탭 항목 (selectable) Lib
> 			239 탭 항목 (selectable) File
> 			240 탭 항목 (selectable) Global
> 			241 탭 항목 (selectable) Net
> 			242 탭 항목 (selectable) Class
> 			243 단추 Close
> 		244 Tab Properties ID: 409871640
> 			245 창 ID: 121918456
> 				246 TABLE Properties Window ID: 127184512
> 					247 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						248 단추 위쪽 스크롤 화살표 ID: UpButton
> 						249 위치 조정 위치 ID: ScrollbarThumb
> 						250 단추 페이지 아래로 ID: DownPageButton
> 						251 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					252 custom Name
> 					253 custom Revision
> 					254 custom GUID
> 					255 custom BaseClass
> 					256 custom Task Settings
> 					257 custom Sigmatek
> 					258 custom OSInterface
> 					259 custom IconPath
> 					260 custom SharedCommandTable
> 					261 custom Objectsize
> 					262 custom Singleton
> 					263 custom Hidden
> 					264 custom Deprecated
> 					265 custom GCCOptions
> 					266 custom Comment
> 					267 custom Filename
> 				268 도구 모음 ID: 59392
> 					269 단추
> 					270 단추
> 			271 탭 항목 (selectable) Properties
> 			272 단추 Close
>
> The focused UI element is 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000.
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
>   "title": "Dismiss LASAL class menu",
>   "code": "{\n  const observation = globalThis.state2;\n  globalThis.state2 = null;\n  await sky.press_key({window:observation.window, key:\"Escape\"});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:false, include_text:true});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write(String(state2.accessibility?.tree || \"\"));\n}"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
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
> 			28 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				29 창 ID: 59648
> 					30 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 						31 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							32 단추 위쪽 스크롤 화살표 ID: UpButton
> 							33 단추 페이지 위로 ID: UpPageButton
> 							34 위치 조정 위치 ID: ScrollbarThumb
> 							35 단추 페이지 아래로 ID: DownPageButton
> 							36 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						37 스크롤 막대 가로 ID: NonClientHorizontalScrollBar

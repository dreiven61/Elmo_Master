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
> 						90 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					91 위치 조정 (disabled)
> 			92 창 HW_Network Secondary Actions: Raise ID: 65281
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
> 			105 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				106 창 ID: 59648
> 					107 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if CurrentSock = dSock then CurrentSock := 0; SessionEpoch += 1; if SessionEpoch = 0 ID: 10000
> 						108 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							109 단추 위쪽 스크롤 화살표 ID: UpButton
> 							110 단추 페이지 위로 ID: UpPageButton
> 							111 위치 조정 위치 ID: ScrollbarThumb
> 							112 단추 페이지 아래로 ID: DownPageButton
> 							113 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						114 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							115 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							116 위치 조정 위치 ID: ScrollbarThumb
> 							117 단추 페이지 오른쪽으로 ID: DownPageButton
> 							118 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						119 위치 조정 (disabled)
> 			120 창 Comm_Network Secondary Actions: Raise ID: 65283
> 				121 창 ID: 59648
> 					122 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						123 단추 위쪽 스크롤 화살표 ID: UpButton
> 						124 위치 조정 위치 ID: ScrollbarThumb
> 						125 단추 페이지 아래로 ID: DownPageButton
> 						126 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					127 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						128 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						129 위치 조정 위치 ID: ScrollbarThumb
> 						130 단추 페이지 오른쪽으로 ID: DownPageButton
> 						131 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					132 위치 조정 (disabled)
> 			133 창 Elmo_4 Secondary Actions: Raise ID: 65285
> 				134 창 ID: 59648
> 					135 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						136 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							137 단추 위쪽 스크롤 화살표 ID: UpButton
> 							138 위치 조정 위치 ID: ScrollbarThumb
> 							139 단추 페이지 아래로 ID: DownPageButton
> 							140 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						141 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							142 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							143 위치 조정 위치 ID: ScrollbarThumb
> 							144 단추 페이지 오른쪽으로 ID: DownPageButton
> 							145 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						146 위치 조정 (disabled)
> 			147 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				148 창 ID: 59648
> 					149 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						150 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							151 단추 위쪽 스크롤 화살표 ID: UpButton
> 							152 위치 조정 위치 ID: ScrollbarThumb
> 							153 단추 페이지 아래로 ID: DownPageButton
> 							154 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						155 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							156 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							157 위치 조정 위치 ID: ScrollbarThumb
> 							158 단추 페이지 오른쪽으로 ID: DownPageButton
> 							159 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						160 위치 조정 (disabled)
> 		161 상태 표시줄 ID: 59393
> 			162 텍스트
> 			163 텍스트
> 			164 텍스트
> 			165 텍스트
> 			166 텍스트
> 			167 텍스트
> 			168 텍스트
> 			169 텍스트
> 			170 텍스트 NUM
> 			171 텍스트
> 		172 창 xtpBarTop ID: 59419
> 			173 도구 모음 Script
> 			174 도구 모음 Edit
> 				175 단추 (disabled) Toggle bookmark
> 				176 단추 (disabled) Previous bookmark
> 				177 단추 (disabled) Next bookmark
> 				178 단추 (disabled) Delete all bookmarks
> 				179 단추 (disabled) Previous bookmark in this file
> 				180 단추 (disabled) Next bookmark in this file
> 				181 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				182 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				183 단추 (disabled) Unindent (Shift+Tab)
> 				184 단추 (disabled) Indent (Tab)
> 			185 도구 모음 Macros Manager
> 				186 메뉴 항목 Macros
> 			187 도구 모음 Layout Manager
> 				188 메뉴 항목 Layouts
> 			189 도구 모음 Toolbox
> 				190 단추 DataAnalyzer
> 				191 메뉴 항목 Toolbar Options
> 			192 도구 모음 Net Edit
> 				193 단추 (disabled) Select
> 				194 메뉴 항목 Toolbar Options
> 			195 도구 모음 Debug
> 				196 단추 Go online (Alt+F6)
> 				197 단추 Change Online Settings
> 				198 메뉴 항목 Online Connection
> 				199 단추 (disabled) Set Online Connection For Current Project
> 				200 단추 (disabled) Download (F6)
> 				201 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				202 단추 (disabled) Download Module on the Fly
> 				203 단추 (disabled) Save Project on PLC
> 				204 단추 (disabled) Start (F7)
> 				205 단추 (disabled) Reset (F8)
> 				206 단추 (disabled) Toggle breakpoint (F4)
> 				207 단추 (disabled) Create condition breakpoint
> 				208 메뉴 항목 Toolbar Options
> 			209 도구 모음 Build
> 				210 메뉴 항목 Target Architecture
> 				211 단추 Build changes (F9)
> 				212 단추 Rebuild project (Strg+F9)
> 				213 단추 (disabled) Cancel building (Ctrl+Break)
> 				214 단추 Link project
> 			215 도구 모음 Standard
> 				216 단추 New project (Strg+N)
> 				217 단추 Open a file (Strg+Shift+O)
> 				218 단추 Close active document (Strg+F4)
> 				219 단추 (disabled) Save file (Strg+S)
> 				220 단추 Open project (Strg+O)
> 				221 단추 Save project changes (Strg+Shift+S)
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
> 					280 콘솔 트리 (selectable) TCPMotionInterface
> 			281 탭 항목 (selectable) Lib
> 			282 탭 항목 (selectable) File
> 			283 탭 항목 (selectable) Global
> 			284 탭 항목 (selectable) Net
> 			285 탭 항목 (selectable) Class
> 			286 단추 Close
> 		287 Tab Properties ID: 125485008
> 			288 창 ID: 290002192
> 				289 TABLE Properties Window ID: 293314152
> 					290 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						291 단추 위쪽 스크롤 화살표 ID: UpButton
> 						292 위치 조정 위치 ID: ScrollbarThumb
> 						293 단추 페이지 아래로 ID: DownPageButton
> 						294 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					295 custom Name
> 					296 custom Revision
> 					297 custom GUID
> 					298 custom Task Settings
> 					299 custom Sigmatek
> 					300 custom OSInterface
> 					301 custom IconPath
> 					302 custom SharedCommandTable
> 					303 custom Objectsize
> 					304 custom Singleton
> 					305 custom Hidden
> 					306 custom Deprecated
> 					307 custom GCCOptions
> 					308 custom Comment
> 					309 custom Filename
> 				310 도구 모음 ID: 59392
> 					311 단추
> 					312 단추
> 			313 탭 항목 (selectable) Properties
> 			314 단추 Close
>
> The focused UI element is 269 트리 ID: 103.
> ```

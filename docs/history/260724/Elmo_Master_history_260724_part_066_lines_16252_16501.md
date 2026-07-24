>   "code": "globalThis.targetApp = globalThis.apps.find(a => a.id === '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe'); if (!globalThis.targetApp || globalThis.targetApp.windows.length !== 1) throw new Error('Expected one LASAL window'); var returnedLasalWindow = globalThis.targetApp.windows[0]; globalThis.targetWindow = await sky.get_window({id: returnedLasalWindow.id, app: returnedLasalWindow.app}); await sky.activate_window({window: globalThis.targetWindow}); globalThis.state = await sky.get_window_state({window: globalThis.targetWindow, include_screenshot:true, include_text:true}); globalThis.targetWindow = globalThis.state.window; nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || ''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				3 창 ID: 59648
> 					4 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5 단추 위쪽 스크롤 화살표 ID: UpButton
> 						6 위치 조정 위치 ID: ScrollbarThumb
> 						7 단추 페이지 아래로 ID: DownPageButton
> 						8 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					9 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						10 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						11 위치 조정 위치 ID: ScrollbarThumb
> 						12 단추 페이지 오른쪽으로 ID: DownPageButton
> 						13 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					14 위치 조정 (disabled)
> 			15 창 TCPMotionInterface Secondary Actions: Raise ID: 65281
> 				16 창 ID: 59648
> 					17 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
> 						18 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							19 단추 위쪽 스크롤 화살표 ID: UpButton
> 							20 위치 조정 위치 ID: ScrollbarThumb
> 							21 단추 페이지 아래로 ID: DownPageButton
> 							22 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						23 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							24 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							25 위치 조정 위치 ID: ScrollbarThumb
> 							26 단추 페이지 오른쪽으로 ID: DownPageButton
> 							27 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						28 위치 조정 (disabled)
> 			29 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65284
> 				30 창 ID: 59648
> 					31 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := ID: 10000
> 						32 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							33 단추 위쪽 스크롤 화살표 ID: UpButton
> 							34 단추 페이지 위로 ID: UpPageButton
> 							35 위치 조정 위치 ID: ScrollbarThumb
> 							36 단추 페이지 아래로 ID: DownPageButton
> 							37 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							39 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							40 위치 조정 위치 ID: ScrollbarThumb
> 							41 단추 페이지 오른쪽으로 ID: DownPageButton
> 							42 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						43 위치 조정 (disabled)
> 			44 창 HW_Network Secondary Actions: Raise ID: 65283
> 				45 창 ID: 59648
> 					46 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						47 단추 위쪽 스크롤 화살표 ID: UpButton
> 						48 위치 조정 위치 ID: ScrollbarThumb
> 						49 단추 페이지 아래로 ID: DownPageButton
> 						50 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					51 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						52 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						53 위치 조정 위치 ID: ScrollbarThumb
> 						54 단추 페이지 오른쪽으로 ID: DownPageButton
> 						55 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					56 위치 조정 (disabled)
> 			57 창 Motion_Network Secondary Actions: Raise ID: 65282
> 				58 창 ID: 59648
> 					59 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						60 단추 위쪽 스크롤 화살표 ID: UpButton
> 						61 위치 조정 위치 ID: ScrollbarThumb
> 						62 단추 페이지 아래로 ID: DownPageButton
> 						63 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					64 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						65 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						66 위치 조정 위치 ID: ScrollbarThumb
> 						67 단추 페이지 오른쪽으로 ID: DownPageButton
> 						68 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					69 위치 조정 (disabled)
> 		70 상태 표시줄 ID: 59393
> 			71 텍스트
> 			72 텍스트
> 			73 텍스트
> 			74 텍스트
> 			75 텍스트
> 			76 텍스트 Offline
> 			77 텍스트
> 			78 텍스트 NUM
> 			79 텍스트
> 		80 창 xtpBarTop ID: 59419
> 			81 도구 모음 Script
> 			82 도구 모음 Edit
> 				83 단추 (disabled) Toggle bookmark
> 				84 단추 (disabled) Previous bookmark
> 				85 단추 (disabled) Next bookmark
> 				86 단추 (disabled) Delete all bookmarks
> 				87 단추 (disabled) Previous bookmark in this file
> 				88 단추 (disabled) Next bookmark in this file
> 				89 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				90 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				91 단추 (disabled) Unindent (Shift+Tab)
> 				92 단추 (disabled) Indent (Tab)
> 			93 도구 모음 Macros Manager
> 				94 메뉴 항목 Macros
> 			95 도구 모음 Layout Manager
> 				96 메뉴 항목 Layouts
> 			97 도구 모음 Toolbox
> 				98 단추 DataAnalyzer
> 				99 메뉴 항목 Toolbar Options
> 			100 도구 모음 Net Edit
> 				101 단추 Select
> 				102 메뉴 항목 Toolbar Options
> 			103 도구 모음 Debug
> 				104 단추 Go online (Alt+F6)
> 				105 단추 Change Online Settings
> 				106 메뉴 항목 Online Connection
> 				107 단추 (disabled) Set Online Connection For Current Project
> 				108 단추 (disabled) Download (F6)
> 				109 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				110 단추 (disabled) Download Module on the Fly
> 				111 단추 (disabled) Save Project on PLC
> 				112 단추 (disabled) Start (F7)
> 				113 단추 (disabled) Reset (F8)
> 				114 단추 (disabled) Toggle breakpoint (F4)
> 				115 단추 (disabled) Create condition breakpoint
> 				116 메뉴 항목 Toolbar Options
> 			117 도구 모음 Build
> 				118 메뉴 항목 Target Architecture
> 				119 단추 Build changes (F9)
> 				120 단추 Rebuild project (Strg+F9)
> 				121 단추 (disabled) Cancel building (Ctrl+Break)
> 				122 단추 Link project
> 			123 도구 모음 Standard
> 				124 단추 New project (Strg+N)
> 				125 단추 Open a file (Strg+Shift+O)
> 				126 단추 Close active document (Strg+F4)
> 				127 단추 (disabled) Save file (Strg+S)
> 				128 단추 Open project (Strg+O)
> 				129 단추 (disabled) Save project changes (Strg+Shift+S)
> 				130 단추 Close project
> 				131 단추 Print
> 				132 단추 Cut (Strg+X)
> 				133 단추 Copy (Strg+C)
> 				134 단추 Paste (Strg+V)
> 				135 메뉴 항목 (disabled) Undo (Strg+Z)
> 				136 메뉴 항목 (disabled) Redo (Strg+Y)
> 				137 단추 Navigate Backward (Alt+Left)
> 				138 단추 (disabled) Navigate Forward (Alt +Right)
> 			139 메뉴 모음 Menu Bar
> 				140 메뉴 항목 FILE
> 				141 메뉴 항목 EDIT
> 				142 메뉴 항목 NETEDIT
> 				143 메뉴 항목 VIEW
> 				144 메뉴 항목 PROJECT
> 				145 메뉴 항목 BUILD
> 				146 메뉴 항목 DEBUG
> 				147 메뉴 항목 ANALYZE
> 				148 메뉴 항목 TOOLS
> 				149 메뉴 항목 EXTRAS
> 				150 메뉴 항목 WINDOW
> 				151 메뉴 항목 HELP
> 		152 창 Splitter ID: 292091552
> 		153 창 Splitter ID: 292092896
> 		154 Tab Output ID: 296619328
> 			155 창 ID: 1200
> 				156 창 ID: 1200
> 					157 LIST ID: 1204
> 						158 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							159 단추 위쪽 스크롤 화살표 ID: UpButton
> 							160 단추 페이지 위로 ID: UpPageButton
> 							161 위치 조정 위치 ID: ScrollbarThumb
> 							162 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						163 목록 항목 (selectable)
> 						164 목록 항목 (selectable)
> 						165 목록 항목 (selectable)
> 					166 스크롤 막대 (disabled) ID: 59904
> 						167 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						168 위치 조정 위치 ID: ScrollbarThumb
> 						169 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			170 탭 항목 (selectable) Python Script
> 			171 탭 항목 (selectable) Output
> 			172 탭 항목 (selectable) Debugger
> 			173 단추 Close
> 		174 창 Splitter ID: 292096088
> 		175 Tab Network View ID: 296620240
> 			176 트리 ID: 104
> 				177 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					178 콘솔 트리 (selectable) Comm_Network
> 					179 콘솔 트리 (selectable) EtherCAT_Network
> 					180 콘솔 트리 (selectable) HW_Network
> 					181 콘솔 트리 (selectable) Motion_Network
> 			182 탭 항목 (selectable) Lib
> 			183 탭 항목 (selectable) File
> 			184 탭 항목 (selectable) Global
> 			185 탭 항목 (selectable) Net
> 			186 탭 항목 (selectable) Class
> 			187 단추 Close
> 		188 Tab Properties ID: 296621152
> 			189 창 ID: 288524952
> 				190 TABLE Properties Window ID: 292730376
> 					191 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						192 단추 위쪽 스크롤 화살표 ID: UpButton
> 						193 위치 조정 위치 ID: ScrollbarThumb
> 						194 단추 페이지 아래로 ID: DownPageButton
> 						195 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					196 custom Name
> 					197 custom Class
> 					198 custom DataType
> 					199 custom Type
> 					200 custom Required
> 					201 custom DefaultInitValue
> 					202 custom Disable RedefinitionVariableWarning
> 					203 custom Comment
> 				204 도구 모음 ID: 59392
> 					205 단추
> 					206 단추
> 			207 탭 항목 (selectable) Properties
> 			208 단추 Close
>
> The focused UI element is 3 창 ID: 59648.
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
>   "title": "두 번째 Find Results 확인",
>   "code": "{ var observation = globalThis.state; var screenshotId = observation.screenshots?.[0]?.id; if (screenshotId == null) throw new Error('No screenshot id'); globalThis.state = null; await sky.click({window: observation.window, screenshotId, x: 392, y: 958}); globalThis.state = await sky.get_window_state({window: observation.window, include_screenshot:true, include_text:true}); globalThis.targetWindow = globalThis.state.window; nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || '')); }"
> }
> ```
>
> ```text
> point (392, 958) is outside window bounds { originX: 0, originY: 0, width: 1450, height: 763 }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Find Results 탭 위치 재확인",
>   "code": "globalThis.state = await sky.get_window_state({window: globalThis.targetWindow, include_screenshot:true, include_text:false}); globalThis.targetWindow = globalThis.state.window; nodeRepl.write(JSON.stringify({window:globalThis.state.window,screens:globalThis.state.screenshots?.map(s=>({id:s.id,width:s.width,height:s.height,zIndex:s.zIndex}))},null,2));"
> }
> ```
>
> ```text

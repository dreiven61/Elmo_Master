> Image output: image/jpeg
>
> Image output: image/jpeg
>
> 지금 LASAL 화면을 직접 확인했습니다. 말씀하신 그대로입니다. `Find Results 1`은 `Total found: 29`라고 집계하지만 결과 행 영역은 비어 있고, `Find Results 2`도 완전히 비어 있습니다. 즉 검색 명령 성공 여부가 아니라, LASAL이 검색 결과 목록을 표시하지 못하는 별도 UI/인덱스 증상입니다. 결과 목록 객체가 실제로 비어 있는지, 행은 있으나 렌더링만 안 되는지 한 단계 더 확인하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Find Results 1 내부 행 확인",
>   "code": "{ var observation3 = globalThis.state; var screenshotId3 = observation3.screenshots?.find(s=>s.zIndex===0)?.id || observation3.screenshots?.[0]?.id; globalThis.state = null; await sky.click({window: observation3.window, screenshotId: screenshotId3, x: 296, y: 708}); globalThis.state = await sky.get_window_state({window: observation3.window, include_screenshot:false, include_text:true}); globalThis.targetWindow = globalThis.state.window; nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || '')); }"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network Secondary Actions: Raise
> 		1 도구 설명 Find Results 2
> 		2 창 작업 영역 ID: 59648
> 			3 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				4 창 ID: 59648
> 					5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						6 단추 위쪽 스크롤 화살표 ID: UpButton
> 						7 위치 조정 위치 ID: ScrollbarThumb
> 						8 단추 페이지 아래로 ID: DownPageButton
> 						9 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						11 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						12 위치 조정 위치 ID: ScrollbarThumb
> 						13 단추 페이지 오른쪽으로 ID: DownPageButton
> 						14 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					15 위치 조정 (disabled)
> 			16 창 TCPMotionInterface Secondary Actions: Raise ID: 65281
> 				17 창 ID: 59648
> 					18 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
> 						19 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							20 단추 위쪽 스크롤 화살표 ID: UpButton
> 							21 위치 조정 위치 ID: ScrollbarThumb
> 							22 단추 페이지 아래로 ID: DownPageButton
> 							23 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						24 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							25 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							26 위치 조정 위치 ID: ScrollbarThumb
> 							27 단추 페이지 오른쪽으로 ID: DownPageButton
> 							28 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						29 위치 조정 (disabled)
> 			30 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65284
> 				31 창 ID: 59648
> 					32 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := ID: 10000
> 						33 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							34 단추 위쪽 스크롤 화살표 ID: UpButton
> 							35 단추 페이지 위로 ID: UpPageButton
> 							36 위치 조정 위치 ID: ScrollbarThumb
> 							37 단추 페이지 아래로 ID: DownPageButton
> 							38 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						39 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							40 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							41 위치 조정 위치 ID: ScrollbarThumb
> 							42 단추 페이지 오른쪽으로 ID: DownPageButton
> 							43 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						44 위치 조정 (disabled)
> 			45 창 HW_Network Secondary Actions: Raise ID: 65283
> 				46 창 ID: 59648
> 					47 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						48 단추 위쪽 스크롤 화살표 ID: UpButton
> 						49 위치 조정 위치 ID: ScrollbarThumb
> 						50 단추 페이지 아래로 ID: DownPageButton
> 						51 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					52 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						53 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						54 위치 조정 위치 ID: ScrollbarThumb
> 						55 단추 페이지 오른쪽으로 ID: DownPageButton
> 						56 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					57 위치 조정 (disabled)
> 			58 창 Motion_Network Secondary Actions: Raise ID: 65282
> 				59 창 ID: 59648
> 					60 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						61 단추 위쪽 스크롤 화살표 ID: UpButton
> 						62 위치 조정 위치 ID: ScrollbarThumb
> 						63 단추 페이지 아래로 ID: DownPageButton
> 						64 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					65 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						66 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						67 위치 조정 위치 ID: ScrollbarThumb
> 						68 단추 페이지 오른쪽으로 ID: DownPageButton
> 						69 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					70 위치 조정 (disabled)
> 		71 상태 표시줄 ID: 59393
> 			72 텍스트
> 			73 텍스트
> 			74 텍스트
> 			75 텍스트
> 			76 텍스트
> 			77 텍스트 Offline
> 			78 텍스트
> 			79 텍스트 NUM
> 			80 텍스트
> 		81 창 xtpBarTop ID: 59419
> 			82 도구 모음 Script
> 			83 도구 모음 Edit
> 				84 단추 (disabled) Toggle bookmark
> 				85 단추 (disabled) Previous bookmark
> 				86 단추 (disabled) Next bookmark
> 				87 단추 (disabled) Delete all bookmarks
> 				88 단추 (disabled) Previous bookmark in this file
> 				89 단추 (disabled) Next bookmark in this file
> 				90 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				91 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				92 단추 (disabled) Unindent (Shift+Tab)
> 				93 단추 (disabled) Indent (Tab)
> 			94 도구 모음 Macros Manager
> 				95 메뉴 항목 Macros
> 			96 도구 모음 Layout Manager
> 				97 메뉴 항목 Layouts
> 			98 도구 모음 Toolbox
> 				99 단추 DataAnalyzer
> 				100 메뉴 항목 Toolbar Options
> 			101 도구 모음 Net Edit
> 				102 단추 Select
> 				103 메뉴 항목 Toolbar Options
> 			104 도구 모음 Debug
> 				105 단추 Go online (Alt+F6)
> 				106 단추 Change Online Settings
> 				107 메뉴 항목 Online Connection
> 				108 단추 (disabled) Set Online Connection For Current Project
> 				109 단추 (disabled) Download (F6)
> 				110 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				111 단추 (disabled) Download Module on the Fly
> 				112 단추 (disabled) Save Project on PLC
> 				113 단추 (disabled) Start (F7)
> 				114 단추 (disabled) Reset (F8)
> 				115 단추 (disabled) Toggle breakpoint (F4)
> 				116 단추 (disabled) Create condition breakpoint
> 				117 메뉴 항목 Toolbar Options
> 			118 도구 모음 Build
> 				119 메뉴 항목 Target Architecture
> 				120 단추 Build changes (F9)
> 				121 단추 Rebuild project (Strg+F9)
> 				122 단추 (disabled) Cancel building (Ctrl+Break)
> 				123 단추 Link project
> 			124 도구 모음 Standard
> 				125 단추 New project (Strg+N)
> 				126 단추 Open a file (Strg+Shift+O)
> 				127 단추 Close active document (Strg+F4)
> 				128 단추 (disabled) Save file (Strg+S)
> 				129 단추 Open project (Strg+O)
> 				130 단추 (disabled) Save project changes (Strg+Shift+S)
> 				131 단추 Close project
> 				132 단추 Print
> 				133 단추 Cut (Strg+X)
> 				134 단추 Copy (Strg+C)
> 				135 단추 Paste (Strg+V)
> 				136 메뉴 항목 (disabled) Undo (Strg+Z)
> 				137 메뉴 항목 (disabled) Redo (Strg+Y)
> 				138 단추 Navigate Backward (Alt+Left)
> 				139 단추 (disabled) Navigate Forward (Alt +Right)
> 			140 메뉴 모음 Menu Bar
> 				141 메뉴 항목 FILE
> 				142 메뉴 항목 EDIT
> 				143 메뉴 항목 NETEDIT
> 				144 메뉴 항목 VIEW
> 				145 메뉴 항목 PROJECT
> 				146 메뉴 항목 BUILD
> 				147 메뉴 항목 DEBUG
> 				148 메뉴 항목 ANALYZE
> 				149 메뉴 항목 TOOLS
> 				150 메뉴 항목 EXTRAS
> 				151 메뉴 항목 WINDOW
> 				152 메뉴 항목 HELP
> 		153 창 Splitter ID: 292091552
> 		154 창 Splitter ID: 292092896
> 		155 Tab Output ID: 296619328
> 			156 창 ID: 1200
> 				157 창 ID: 1200
> 					158 LIST ID: 1204
> 						159 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							160 단추 위쪽 스크롤 화살표 ID: UpButton
> 							161 단추 페이지 위로 ID: UpPageButton
> 							162 위치 조정 위치 ID: ScrollbarThumb
> 							163 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						164 목록 항목 (selectable)
> 						165 목록 항목 (selectable)
> 						166 목록 항목 (selectable)
> 					167 스크롤 막대 (disabled) ID: 59904
> 						168 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						169 위치 조정 위치 ID: ScrollbarThumb
> 						170 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			171 탭 항목 (selectable) Python Script
> 			172 탭 항목 (selectable) Output
> 			173 탭 항목 (selectable) Debugger
> 			174 단추 Close
> 		175 창 Splitter ID: 292096088
> 		176 Tab Network View ID: 296620240
> 			177 트리 ID: 104
> 				178 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					179 콘솔 트리 (selectable) Comm_Network
> 					180 콘솔 트리 (selectable) EtherCAT_Network
> 					181 콘솔 트리 (selectable) HW_Network
> 					182 콘솔 트리 (selectable) Motion_Network
> 			183 탭 항목 (selectable) Lib
> 			184 탭 항목 (selectable) File
> 			185 탭 항목 (selectable) Global
> 			186 탭 항목 (selectable) Net
> 			187 탭 항목 (selectable) Class
> 			188 단추 Close
> 		189 Tab Properties ID: 296621152
> 			190 창 ID: 288524952
> 				191 TABLE Properties Window ID: 292730376
> 					192 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						193 단추 위쪽 스크롤 화살표 ID: UpButton
> 						194 위치 조정 위치 ID: ScrollbarThumb
> 						195 단추 페이지 아래로 ID: DownPageButton
> 						196 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					197 custom Name
> 					198 custom Class
> 					199 custom DataType
> 					200 custom Type
> 					201 custom Required
> 					202 custom DefaultInitValue
> 					203 custom Disable RedefinitionVariableWarning
> 					204 custom Comment
> 				205 도구 모음 ID: 59392
> 					206 단추
> 					207 단추
> 			208 탭 항목 (selectable) Properties
> 			209 단추 Close
> 		210 도구 설명 Find Results 1
>
> The focused UI element is 158 LIST ID: 1204.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "숨은 결과 행 선택 여부 확인",
>   "code": "{ var observation4 = globalThis.state; globalThis.state = null; await sky.click({window: observation4.window, element_index: 164}); globalThis.state = await sky.get_window_state({window: observation4.window, include_screenshot:true, include_text:true}); globalThis.targetWindow = globalThis.state.window; nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || '')); }"
> }
> ```
>
> ```text
> element 164 has no cached bounds
> ```
>
> MCP tool call

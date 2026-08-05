>   {
>     "id": "AcrobatReader",
>     "windows": [],
>     "displayName": "Adobe Acrobat",
>     "lastUsedDate": "2026-05-06",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Advanced IP Scanner\\advanced_ip_scanner.exe",
>     "windows": [],
>     "displayName": "Advanced IP Scanner",
>     "lastUsedDate": "2026-04-07",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\Qt\\Tools\\QtCreator\\bin\\qtcreator.exe",
>     "windows": [],
>     "displayName": "Qt Creator 18.0.1 (Community)",
>     "lastUsedDate": "2026-04-07",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\Users\\dreiv\\eclipse\\java-2023-09\\eclipse\\eclipse.exe",
>     "windows": [],
>     "displayName": "Eclipse IDE for Java Developers - 2023-09",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\ProgramData\\MDS6\\MDS Installation Guide.pdf",
>     "windows": [],
>     "displayName": "MDS Installation Guide",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\WSL\\wslsettings\\wslsettings.exe",
>     "windows": [],
>     "displayName": "WSL Settings",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX Cam Designer\\3.6.0\\ctrlX-CamDesigner.exe",
>     "windows": [],
>     "displayName": "ctrlX Cam Designer 3.6.1",
>     "lastUsedDate": "2026-03-13",
>     "useCount": 0,
>     "isRunning": false
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{\n  globalThis.targetApp = apps.find((app) => app.id === \"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\");\n  if (!targetApp) throw new Error(\"LASAL Class 2 app not returned\");\n  var candidates = targetApp.windows.filter(w => w.title === \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService\");\n  if (candidates.length !== 1) throw new Error(\"Expected one LASAL project window, got \" + candidates.length);\n  globalThis.targetWindow = await sky.get_window({ id: candidates[0].id, app: candidates[0].app });\n  await sky.activate_window({ window: targetWindow });\n  globalThis.state = await sky.get_window_state({ window: targetWindow, include_screenshot: true, include_text: true });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}",
>   "title": "Inspect LASAL IDE"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65282
> 				3 창 ID: 59648
> 					4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex >= 0x8000 then physicalAxis := (CatalogIndex shr 8) and 0x007F; topologyCount := CatalogIndex and 0x00FF; if (topologyCount = 0) | (physicalAxis >= 7) | ((physicalAxis + topologyCount) > 7) then RETURN; end_if; (pEntry + 16)^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 20)^$UINT := phy ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 위치 조정 위치 ID: ScrollbarThumb
> 							8 단추 페이지 아래로 ID: DownPageButton
> 							9 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							11 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							12 위치 조정 위치 ID: ScrollbarThumb
> 							13 단추 페이지 오른쪽으로 ID: DownPageButton
> 							14 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						15 위치 조정 (disabled)
> 			16 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281
> 				17 창 ID: 59648
> 					18 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
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
> 			30 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				31 창 ID: 59648
> 					32 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 						33 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							34 단추 위쪽 스크롤 화살표 ID: UpButton
> 							35 위치 조정 위치 ID: ScrollbarThumb
> 							36 단추 페이지 아래로 ID: DownPageButton
> 							37 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							39 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							40 위치 조정 위치 ID: ScrollbarThumb
> 							41 단추 페이지 오른쪽으로 ID: DownPageButton
> 							42 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						43 위치 조정 (disabled)
> 		44 상태 표시줄 ID: 59393
> 			45 텍스트
> 			46 텍스트
> 			47 텍스트
> 			48 텍스트 Ln 13 Col 50
> 			49 텍스트
> 			50 텍스트 Offline
> 			51 텍스트
> 			52 텍스트 NUM
> 			53 텍스트
> 		54 창 xtpBarTop ID: 59419
> 			55 도구 모음 Edit
> 				56 단추 Toggle bookmark
> 				57 단추 (disabled) Previous bookmark
> 				58 단추 (disabled) Next bookmark
> 				59 단추 (disabled) Delete all bookmarks
> 				60 단추 (disabled) Previous bookmark in this file
> 				61 단추 (disabled) Next bookmark in this file
> 				62 단추 Comment selected text (Ctrl+Shift+C)
> 				63 단추 Remove comment (Ctrl+Shift+X)
> 				64 단추 Unindent (Shift+Tab)
> 				65 단추 Indent (Tab)
> 			66 도구 모음 Macros Manager
> 				67 메뉴 항목 Macros
> 			68 도구 모음 Layout Manager
> 				69 메뉴 항목 Layouts
> 			70 도구 모음 Toolbox
> 				71 단추 DataAnalyzer
> 				72 메뉴 항목 Toolbar Options
> 			73 도구 모음 Net Edit
> 				74 단추 (disabled) Select
> 				75 메뉴 항목 Toolbar Options
> 			76 도구 모음 Debug
> 				77 단추 Go online (Alt+F6)
> 				78 단추 Change Online Settings
> 				79 메뉴 항목 Online Connection
> 				80 단추 (disabled) Set Online Connection For Current Project
> 				81 단추 (disabled) Download (F6)
> 				82 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				83 단추 (disabled) Download Module on the Fly
> 				84 단추 (disabled) Save Project on PLC
> 				85 단추 (disabled) Start (F7)
> 				86 단추 (disabled) Reset (F8)
> 				87 단추 Toggle breakpoint (F4)
> 				88 단추 Create condition breakpoint
> 				89 메뉴 항목 Toolbar Options
> 			90 도구 모음 Build
> 				91 메뉴 항목 Target Architecture
> 				92 단추 Build changes (F9)
> 				93 단추 Rebuild project (Strg+F9)
> 				94 단추 (disabled) Cancel building (Ctrl+Break)
> 				95 단추 Link project
> 			96 도구 모음 Standard
> 				97 단추 New project (Strg+N)
> 				98 단추 Open a file (Strg+Shift+O)
> 				99 단추 Close active document (Strg+F4)
> 				100 단추 (disabled) Save file (Strg+S)
> 				101 단추 Open project (Strg+O)
> 				102 단추 (disabled) Save project changes (Strg+Shift+S)
> 				103 단추 Close project
> 				104 단추 Print
> 				105 단추 Cut (Strg+X)
> 				106 단추 Copy (Strg+C)
> 				107 단추 Paste (Strg+V)
> 				108 메뉴 항목 (disabled) Undo (Strg+Z)
> 				109 메뉴 항목 (disabled) Redo (Strg+Y)
> 				110 단추 Navigate Backward (Alt+Left)
> 				111 단추 (disabled) Navigate Forward (Alt +Right)
> 			112 메뉴 모음 Menu Bar
> 				113 메뉴 항목 FILE
> 				114 메뉴 항목 EDIT
> 				115 메뉴 항목 VIEW
> 				116 메뉴 항목 PROJECT
> 				117 메뉴 항목 BUILD
> 				118 메뉴 항목 DEBUG
> 				119 메뉴 항목 ANALYZE
> 				120 메뉴 항목 TOOLS
> 				121 메뉴 항목 EXTRAS
> 				122 메뉴 항목 WINDOW
> 				123 메뉴 항목 HELP
> 		124 창 Splitter ID: 404445040
> 		125 창 Splitter ID: 404445712
> 		126 Tab Output ID: 296578152
> 			127 창 ID: 1200
> 				128 창 ID: 1200
> 					129 LIST ID: 1204
> 						130 목록 항목 (selectable)
> 						131 목록 항목 (selectable)
> 						132 목록 항목 (selectable)
> 					133 스크롤 막대 (disabled) ID: 59904
> 						134 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						135 위치 조정 위치 ID: ScrollbarThumb
> 						136 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			137 탭 항목 (selectable) Python Script
> 			138 탭 항목 (selectable) Debugger
> 			139 탭 항목 (selectable) Output
> 			140 단추 Close
> 		141 창 Splitter ID: 404446216
> 		142 Tab Class View ID: 296578608
> 			143 트리 ID: 103
> 				144 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					145 단추 위쪽 스크롤 화살표 ID: UpButton
> 					146 위치 조정 위치 ID: ScrollbarThumb
> 					147 단추 페이지 아래로 ID: DownPageButton
> 					148 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				149 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					150 콘솔 트리 (selectable) External
> 					151 콘솔 트리 (selectable) Sigmatek
> 					152 콘솔 트리 (selectable) Elmo_1
> 					153 콘솔 트리 (selectable) Elmo_2
> 					154 콘솔 트리 (selectable) Elmo_3
> 					155 콘솔 트리 (selectable) Elmo_4
> 					156 콘솔 트리 (selectable) GL_9086_1
> 					157 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					158 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					159 콘솔 트리 (selectable) LMCControlCommandService
> 					160 콘솔 트리 (selectable) LMCDiagnosticsService
> 					161 콘솔 트리 (selectable) LMCEcatInputLatch
> 					162 콘솔 트리 (selectable) LMCRecorderStore
> 					163 콘솔 트리 (selectable) LMCSdoExecutor
> 					164 콘솔 트리 (selectable) TCPIPServer
> 					165 콘솔 트리 (selectable) TCPMotionInterface
> 			166 탭 항목 (selectable) Lib
> 			167 탭 항목 (selectable) File
> 			168 탭 항목 (selectable) Global
> 			169 탭 항목 (selectable) Net
> 			170 탭 항목 (selectable) Class
> 			171 단추 Close
> 		172 Tab Properties ID: 296581344
> 			173 창 ID: 287643296
> 				174 TABLE Properties Window ID: 294249272
> 				175 도구 모음 ID: 59392
> 					176 단추
> 					177 단추

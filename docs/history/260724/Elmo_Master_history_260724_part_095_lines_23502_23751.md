> 						149 목록 항목 (selectable)
> 						150 목록 항목 (selectable)
> 						151 목록 항목 (selectable)
> 						152 목록 항목 (selectable)
> 						153 목록 항목 (selectable)
> 						154 목록 항목 (selectable)
> 						155 목록 항목 (selectable)
> 						156 목록 항목 (selectable)
> 						157 목록 항목 (selectable)
> 						158 목록 항목 (selectable)
> 						159 목록 항목 (selectable)
> 						160 목록 항목 (selectable)
> 						161 목록 항목 (selectable)
> 						162 목록 항목 (selectable)
> 						163 목록 항목 (selectable)
> 						164 목록 항목 (selectable)
> 						165 목록 항목 (selectable)
> 						166 목록 항목 (selectable)
> 						167 목록 항목 (selectable)
> 						168 목록 항목 (selectable)
> 						169 목록 항목 (selectable)
> 						170 목록 항목 (selectable)
> 						171 목록 항목 (selectable)
> 						172 목록 항목 (selectable)
> 						173 목록 항목 (selectable)
> 						174 목록 항목 (selectable)
> 						175 목록 항목 (selectable)
> 						176 목록 항목 (selectable)
> 						177 목록 항목 (selectable)
> 						178 목록 항목 (selectable)
> 						179 목록 항목 (selectable)
> 						180 목록 항목 (selectable)
> 						181 목록 항목 (selectable)
> 						182 목록 항목 (selectable)
> 						183 목록 항목 (selectable)
> 						184 목록 항목 (selectable)
> 						185 목록 항목 (selectable)
> 						186 목록 항목 (selectable)
> 						187 목록 항목 (selectable)
> 						188 목록 항목 (selectable)
> 						189 목록 항목 (selectable)
> 						190 목록 항목 (selectable)
> 						191 목록 항목 (selectable)
> 						192 목록 항목 (selectable)
> 						193 목록 항목 (selectable)
> 						194 목록 항목 (selectable)
> 						195 목록 항목 (selectable)
> 						196 목록 항목 (selectable)
> 						197 목록 항목 (selectable)
> 						198 목록 항목 (selectable)
> 						199 목록 항목 (selectable)
> 					200 스크롤 막대 ID: 59904
> 						201 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						202 위치 조정 위치 ID: ScrollbarThumb
> 						203 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			204 탭 항목 (selectable) Python Script
> 			205 탭 항목 (selectable) Debugger
> 			206 탭 항목 (selectable) Output
> 			207 단추 Close
> 		208 창 Splitter ID: 358285744
> 		209 Tab Class View ID: 295832136
> 			210 트리 (disabled) ID: 103
> 			211 탭 항목 (selectable) Lib
> 			212 탭 항목 (selectable) File
> 			213 탭 항목 (selectable) Global
> 			214 탭 항목 (selectable) Net
> 			215 탭 항목 (selectable) Class
> 			216 단추 Close
> 		217 Tab Properties ID: 295834416
> 			218 창 (disabled) ID: 286533584
> 				219 TABLE Properties Window ID: 293432256
> 				220 도구 모음 ID: 59392
> 					221 단추
> 					222 단추
> 			223 탭 항목 (selectable) Properties
> 			224 단추 Close
>
> The focused UI element is 1 창 작업 영역 ID: 59648.
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
>   "title": "LASAL 프로젝트 로딩 대기",
>   "code": "await new Promise(r=>setTimeout(r,5000)); globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 창 ACallTip
> 		2 창 작업 영역 ID: 59648
> 			3 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				4 창 ID: 59648
> 					5 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
> 						6 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							7 단추 위쪽 스크롤 화살표 ID: UpButton
> 							8 단추 페이지 위로 ID: UpPageButton
> 							9 위치 조정 위치 ID: ScrollbarThumb
> 							10 단추 페이지 아래로 ID: DownPageButton
> 							11 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						12 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							13 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							14 위치 조정 위치 ID: ScrollbarThumb
> 							15 단추 페이지 오른쪽으로 ID: DownPageButton
> 							16 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						17 위치 조정 (disabled)
> 			18 창 Elmo_4 Secondary Actions: Raise ID: 65286
> 				19 창 ID: 59648
> 					20 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						21 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							22 단추 위쪽 스크롤 화살표 ID: UpButton
> 							23 위치 조정 위치 ID: ScrollbarThumb
> 							24 단추 페이지 아래로 ID: DownPageButton
> 							25 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						26 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							27 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							28 위치 조정 위치 ID: ScrollbarThumb
> 							29 단추 페이지 오른쪽으로 ID: DownPageButton
> 							30 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						31 위치 조정 (disabled)
> 			32 창 EtherCAT_Network Secondary Actions: Raise ID: 65284
> 				33 창 ID: 59648
> 					34 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						35 단추 위쪽 스크롤 화살표 ID: UpButton
> 						36 위치 조정 위치 ID: ScrollbarThumb
> 						37 단추 페이지 아래로 ID: DownPageButton
> 						38 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			39 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				40 창 ID: 59648
> 					41 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := ID: 10000
> 						42 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							43 단추 위쪽 스크롤 화살표 ID: UpButton
> 							44 단추 페이지 위로 ID: UpPageButton
> 							45 위치 조정 위치 ID: ScrollbarThumb
> 							46 단추 페이지 아래로 ID: DownPageButton
> 							47 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						48 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							49 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							50 위치 조정 위치 ID: ScrollbarThumb
> 							51 단추 페이지 오른쪽으로 ID: DownPageButton
> 							52 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						53 위치 조정 (disabled)
> 			54 창 HW_Network Secondary Actions: Raise ID: 65282
> 				55 창 ID: 59648
> 			56 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				57 창 ID: 59648
> 					58 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						59 단추 위쪽 스크롤 화살표 ID: UpButton
> 						60 위치 조정 위치 ID: ScrollbarThumb
> 						61 단추 페이지 아래로 ID: DownPageButton
> 						62 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					63 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						64 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						65 위치 조정 위치 ID: ScrollbarThumb
> 						66 단추 페이지 오른쪽으로 ID: DownPageButton
> 						67 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					68 위치 조정 (disabled)
> 			69 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				70 창 ID: 59648
> 					71 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						72 단추 위쪽 스크롤 화살표 ID: UpButton
> 						73 위치 조정 위치 ID: ScrollbarThumb
> 						74 단추 페이지 아래로 ID: DownPageButton
> 						75 단추 아래쪽 스크롤 화살표 ID: DownButton
> 		76 상태 표시줄 ID: 59393
> 			77 텍스트
> 			78 텍스트 TCPMotionInterface::CyWork
> 			79 텍스트
> 			80 텍스트 Ln 88 Col 1
> 			81 텍스트
> 			82 텍스트 Offline
> 			83 텍스트
> 			84 텍스트 NUM
> 			85 텍스트
> 		86 창 xtpBarTop ID: 59419
> 			87 도구 모음 Edit
> 				88 단추 Toggle bookmark
> 				89 단추 (disabled) Previous bookmark
> 				90 단추 (disabled) Next bookmark
> 				91 단추 (disabled) Delete all bookmarks
> 				92 단추 (disabled) Previous bookmark in this file
> 				93 단추 (disabled) Next bookmark in this file
> 				94 단추 Comment selected text (Ctrl+Shift+C)
> 				95 단추 Remove comment (Ctrl+Shift+X)
> 				96 단추 Unindent (Shift+Tab)
> 				97 단추 Indent (Tab)
> 			98 도구 모음 Macros Manager
> 				99 메뉴 항목 Macros
> 			100 도구 모음 Layout Manager
> 				101 메뉴 항목 Layouts
> 			102 도구 모음 Toolbox
> 				103 단추 DataAnalyzer
> 				104 단추 Interpreter
> 				105 단추 DiasDrive
> 				106 단추 PLC Diagnosis
> 				107 단추 Hardware Editor
> 				108 단추 Graphical Hardware Editor
> 				109 단추 Connection Manager
> 				110 단추 Task Configuration
> 			111 도구 모음 Net Edit
> 				112 단추 (disabled) Select
> 				113 단추 (disabled) Move view
> 				114 단추 (disabled) Insert comment
> 				115 단추 (disabled) Zoom(+/-)
> 				116 단추 (disabled) Zoom to all
> 				117 단추 (disabled) Zoom selection
> 			118 도구 모음 Debug
> 				119 단추 Go online (Alt+F6)
> 				120 단추 Change Online Settings
> 				121 메뉴 항목 Online Connection
> 				122 단추 (disabled) Set Online Connection For Current Project
> 				123 단추 (disabled) Download (F6)
> 				124 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				125 단추 (disabled) Download Module on the Fly
> 				126 단추 (disabled) Save Project on PLC
> 				127 단추 (disabled) Start (F7)
> 				128 단추 (disabled) Reset (F8)
> 				129 단추 Toggle breakpoint (F4)
> 				130 단추 Create condition breakpoint
> 				131 단추 Remove all breakpoint(s) (Shift+F4)
> 				132 단추 AWL trigger on/off
> 				133 단추 (disabled) Fix AWL trigger to current instruction
> 				134 단추 Activate/Deactivate Live View
> 				135 단추 Activate/Deactivate Auto Watch
> 				136 단추 (disabled) Goto instruction pointer
> 				137 단추 (disabled) Step into (F5)
> 				138 단추 (disabled) Step over (Alt+F5)
> 				139 단추 (disabled) Step out (Shift+F5)
> 				140 단추 (disabled) Set instruction pointer
> 			141 도구 모음 Build
> 				142 메뉴 항목 Target Architecture
> 				143 단추 Build changes (F9)
> 				144 단추 Rebuild project (Strg+F9)
> 				145 단추 (disabled) Cancel building (Ctrl+Break)
> 				146 단추 Link project
> 			147 도구 모음 Standard
> 				148 단추 New project (Strg+N)
> 				149 단추 Open a file (Strg+Shift+O)
> 				150 단추 Close active document (Strg+F4)
> 				151 단추 (disabled) Save file (Strg+S)
> 				152 단추 Open project (Strg+O)
> 				153 단추 (disabled) Save project changes (Strg+Shift+S)
> 				154 단추 Close project

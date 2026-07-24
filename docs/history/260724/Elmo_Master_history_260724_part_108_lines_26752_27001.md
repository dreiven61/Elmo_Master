> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				3 창 ID: 59648
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
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
> 			17 창 Elmo_4 Secondary Actions: Raise ID: 65286
> 				18 창 ID: 59648
> 					19 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						20 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							21 단추 위쪽 스크롤 화살표 ID: UpButton
> 							22 위치 조정 위치 ID: ScrollbarThumb
> 							23 단추 페이지 아래로 ID: DownPageButton
> 							24 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						25 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							26 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							27 위치 조정 위치 ID: ScrollbarThumb
> 							28 단추 페이지 오른쪽으로 ID: DownPageButton
> 							29 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						30 위치 조정 (disabled)
> 			31 창 EtherCAT_Network Secondary Actions: Raise ID: 65284
> 				32 창 ID: 59648
> 					33 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						34 단추 위쪽 스크롤 화살표 ID: UpButton
> 						35 위치 조정 위치 ID: ScrollbarThumb
> 						36 단추 페이지 아래로 ID: DownPageButton
> 						37 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						39 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						40 위치 조정 위치 ID: ScrollbarThumb
> 						41 단추 페이지 오른쪽으로 ID: DownPageButton
> 						42 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					43 위치 조정 (disabled)
> 			44 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				45 창 ID: 59648
> 					46 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := ID: 10000
> 						47 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							48 단추 위쪽 스크롤 화살표 ID: UpButton
> 							49 단추 페이지 위로 ID: UpPageButton
> 							50 위치 조정 위치 ID: ScrollbarThumb
> 							51 단추 페이지 아래로 ID: DownPageButton
> 							52 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						53 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							54 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							55 위치 조정 위치 ID: ScrollbarThumb
> 							56 단추 페이지 오른쪽으로 ID: DownPageButton
> 							57 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						58 위치 조정 (disabled)
> 			59 창 HW_Network Secondary Actions: Raise ID: 65282
> 				60 창 ID: 59648
> 					61 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						62 단추 위쪽 스크롤 화살표 ID: UpButton
> 						63 위치 조정 위치 ID: ScrollbarThumb
> 						64 단추 페이지 아래로 ID: DownPageButton
> 						65 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			66 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				67 창 ID: 59648
> 					68 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						69 단추 위쪽 스크롤 화살표 ID: UpButton
> 						70 위치 조정 위치 ID: ScrollbarThumb
> 						71 단추 페이지 아래로 ID: DownPageButton
> 						72 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					73 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 페이지 오른쪽으로 ID: DownPageButton
> 						77 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					78 위치 조정 (disabled)
> 			79 창 Comm_Network Secondary Actions: Raise ID: 65280
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
> 		92 상태 표시줄 ID: 59393
> 			93 텍스트
> 			94 텍스트
> 			95 텍스트
> 			96 텍스트
> 			97 텍스트
> 			98 텍스트 Offline
> 			99 텍스트
> 			100 텍스트 NUM
> 			101 텍스트
> 		102 창 xtpBarTop ID: 59419
> 			103 도구 모음 Edit
> 				104 단추 Toggle bookmark
> 				105 단추 (disabled) Previous bookmark
> 				106 단추 (disabled) Next bookmark
> 				107 단추 (disabled) Delete all bookmarks
> 				108 단추 (disabled) Previous bookmark in this file
> 				109 단추 (disabled) Next bookmark in this file
> 				110 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				111 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				112 단추 (disabled) Unindent (Shift+Tab)
> 				113 단추 (disabled) Indent (Tab)
> 			114 도구 모음 Macros Manager
> 				115 메뉴 항목 Macros
> 			116 도구 모음 Layout Manager
> 				117 메뉴 항목 Layouts
> 			118 도구 모음 Toolbox
> 				119 단추 DataAnalyzer
> 				120 메뉴 항목 Toolbar Options
> 			121 도구 모음 Net Edit
> 				122 단추 (disabled) Select
> 				123 메뉴 항목 Toolbar Options
> 			124 도구 모음 Debug
> 				125 단추 Go online (Alt+F6)
> 				126 단추 Change Online Settings
> 				127 메뉴 항목 Online Connection
> 				128 단추 (disabled) Set Online Connection For Current Project
> 				129 단추 (disabled) Download (F6)
> 				130 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				131 단추 (disabled) Download Module on the Fly
> 				132 단추 (disabled) Save Project on PLC
> 				133 단추 (disabled) Start (F7)
> 				134 단추 (disabled) Reset (F8)
> 				135 단추 Toggle breakpoint (F4)
> 				136 단추 Create condition breakpoint
> 				137 메뉴 항목 Toolbar Options
> 			138 도구 모음 Build
> 				139 메뉴 항목 Target Architecture
> 				140 단추 Build changes (F9)
> 				141 단추 Rebuild project (Strg+F9)
> 				142 단추 (disabled) Cancel building (Ctrl+Break)
> 				143 단추 Link project
> 			144 도구 모음 Standard
> 				145 단추 New project (Strg+N)
> 				146 단추 Open a file (Strg+Shift+O)
> 				147 단추 Close active document (Strg+F4)
> 				148 단추 (disabled) Save file (Strg+S)
> 				149 단추 Open project (Strg+O)
> 				150 단추 (disabled) Save project changes (Strg+Shift+S)
> 				151 단추 Close project
> 				152 단추 Print
> 				153 단추 Cut (Strg+X)
> 				154 단추 Copy (Strg+C)
> 				155 단추 Paste (Strg+V)
> 				156 메뉴 항목 Undo (Strg+Z)
> 				157 메뉴 항목 (disabled) Redo (Strg+Y)
> 				158 단추 Navigate Backward (Alt+Left)
> 				159 단추 (disabled) Navigate Forward (Alt +Right)
> 			160 메뉴 모음 Menu Bar
> 				161 메뉴 항목 FILE
> 				162 메뉴 항목 EDIT
> 				163 메뉴 항목 VIEW
> 				164 메뉴 항목 PROJECT
> 				165 메뉴 항목 BUILD
> 				166 메뉴 항목 DEBUG
> 				167 메뉴 항목 ANALYZE
> 				168 메뉴 항목 TOOLS
> 				169 메뉴 항목 EXTRAS
> 				170 메뉴 항목 WINDOW
> 				171 메뉴 항목 HELP
> 		172 창 Splitter ID: 360603976
> 		173 창 Splitter ID: 359974384
> 		174 Tab Output ID: 358094976
> 			175 창 ID: 1200
> 				176 창 ID: 1200
> 					177 LIST ID: 1201
> 						178 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							179 단추 위쪽 스크롤 화살표 ID: UpButton
> 							180 단추 페이지 위로 ID: UpPageButton
> 							181 위치 조정 위치 ID: ScrollbarThumb
> 							182 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						183 목록 항목 (selectable)
> 						184 목록 항목 (selectable)
> 						185 목록 항목 (selectable)
> 						186 목록 항목 (selectable)
> 						187 목록 항목 (selectable)
> 						188 목록 항목 (selectable)
> 						189 목록 항목 (selectable)
> 						190 목록 항목 (selectable)
> 					191 스크롤 막대 ID: 59904
> 						192 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						193 위치 조정 위치 ID: ScrollbarThumb
> 						194 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			195 탭 항목 (selectable) Python Script
> 			196 탭 항목 (selectable) Debugger
> 			197 탭 항목 (selectable) Output
> 			198 단추 Close
> 		199 창 Splitter ID: 360599944
> 		200 Tab Class View ID: 358100904
> 			201 트리 ID: 103
> 				202 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					203 단추 위쪽 스크롤 화살표 ID: UpButton
> 					204 단추 페이지 위로 ID: UpPageButton
> 					205 위치 조정 위치 ID: ScrollbarThumb
> 					206 단추 페이지 아래로 ID: DownPageButton
> 					207 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				208 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					209 콘솔 트리 (selectable) External
> 					210 콘솔 트리 (selectable) Sigmatek
> 					211 콘솔 트리 (selectable) _TCPIPServer_RT
> 					212 콘솔 트리 (selectable) Elmo_1
> 					213 콘솔 트리 (selectable) Elmo_2
> 					214 콘솔 트리 (selectable) Elmo_3
> 					215 콘솔 트리 (selectable) Elmo_4
> 					216 콘솔 트리 (selectable) LMCControlCommandService
> 						217 콘솔 트리 (selectable) Servers
> 						218 콘솔 트리 (selectable) Clients
> 						219 콘솔 트리 (selectable) Methods
> 					220 콘솔 트리 (selectable) LMCDiagnosticsService
> 					221 콘솔 트리 (selectable) LMCEcatInputLatch
> 					222 콘솔 트리 (selectable) LMCRecorderStore
> 					223 콘솔 트리 (selectable) LMCSdoExecutor
> 					224 콘솔 트리 (selectable) TCPMotionInterface
> 			225 탭 항목 (selectable) Lib
> 			226 탭 항목 (selectable) File
> 			227 탭 항목 (selectable) Global
> 			228 탭 항목 (selectable) Net
> 			229 탭 항목 (selectable) Class
> 			230 단추 Close
> 		231 Tab Properties ID: 358094064
> 			232 창 ID: 290198624
> 				233 TABLE Properties Window ID: 293571936
> 				234 도구 모음 ID: 59392
> 					235 단추
> 					236 단추
> 			237 탭 항목 (selectable) Properties
> 			238 단추 Close
>
> The focused UI element is 201 트리 ID: 103.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg

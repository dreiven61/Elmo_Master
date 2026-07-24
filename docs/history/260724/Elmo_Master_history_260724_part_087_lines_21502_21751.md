> 				169 메뉴 항목 ANALYZE
> 				170 메뉴 항목 TOOLS
> 				171 메뉴 항목 EXTRAS
> 				172 메뉴 항목 WINDOW
> 				173 메뉴 항목 HELP
> 		174 창 Splitter ID: 355145208
> 		175 창 Splitter ID: 355146888
> 		176 Tab Output ID: 295674376
> 			177 창 ID: 1200
> 				178 창 ID: 1200
> 					179 LIST ID: 1201
> 						180 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							181 단추 위쪽 스크롤 화살표 ID: UpButton
> 							182 단추 페이지 위로 ID: UpPageButton
> 							183 위치 조정 위치 ID: ScrollbarThumb
> 							184 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						185 목록 항목 (selectable)
> 						186 목록 항목 (selectable)
> 						187 목록 항목 (selectable)
> 						188 목록 항목 (selectable)
> 						189 목록 항목 (selectable)
> 						190 목록 항목 (selectable)
> 						191 목록 항목 (selectable)
> 						192 목록 항목 (selectable)
> 					193 스크롤 막대 ID: 59904
> 						194 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						195 위치 조정 위치 ID: ScrollbarThumb
> 						196 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			197 탭 항목 (selectable) Python Script
> 			198 탭 항목 (selectable) Debugger
> 			199 탭 항목 (selectable) Output
> 			200 단추 Close
> 		201 창 Splitter ID: 355143864
> 		202 Tab Class View ID: 295678480
> 			203 트리 ID: 103
> 				204 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					205 콘솔 트리 (selectable) External
> 					206 콘솔 트리 (selectable) Sigmatek
> 					207 콘솔 트리 (selectable) _TCPIPServer_RT
> 					208 콘솔 트리 (selectable) Elmo_1
> 					209 콘솔 트리 (selectable) Elmo_2
> 					210 콘솔 트리 (selectable) Elmo_3
> 					211 콘솔 트리 (selectable) Elmo_4
> 					212 콘솔 트리 (selectable) LMCDiagnosticsService
> 					213 콘솔 트리 (selectable) LMCEcatInputLatch
> 					214 콘솔 트리 (selectable) LMCRecorderStore
> 					215 콘솔 트리 (selectable) LMCSdoExecutor
> 					216 콘솔 트리 (selectable) TCPMotionInterface
> 			217 탭 항목 (selectable) Lib
> 			218 탭 항목 (selectable) File
> 			219 탭 항목 (selectable) Global
> 			220 탭 항목 (selectable) Net
> 			221 탭 항목 (selectable) Class
> 			222 단추 Close
> 		223 Tab Properties ID: 295677568
> 			224 창 ID: 289346696
> 				225 TABLE Properties Window ID: 293355256
> 					226 custom Name
> 					227 custom Revision
> 					228 custom GUID
> 					229 custom BaseClass
> 					230 custom Task Settings
> 					231 custom Sigmatek
> 					232 custom OSInterface
> 					233 custom IconPath
> 					234 custom SharedCommandTable
> 					235 custom Objectsize
> 					236 custom Singleton
> 					237 custom Hidden
> 					238 custom Deprecated
> 					239 custom GCCOptions
> 					240 custom Comment
> 					241 custom Filename
> 				242 도구 모음 ID: 59392
> 					243 단추
> 					244 단추
> 			245 탭 항목 (selectable) Properties
> 			246 단추 Close
>
> The focused UI element is 203 트리 ID: 103.
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
>   "title": "Add private group handler declaration",
>   "code": "{\n  var obsMenu = globalThis.state;\n  var shotMenu = obsMenu.screenshots?.[0]?.id;\n  globalThis.state = null;\n  await sky.click({window:obsMenu.window, screenshotId:shotMenu, x:1781, y:648});\n  globalThis.state = await sky.get_window_state({window:obsMenu.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}"
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
> 			22 메뉴 항목 Delete Network
> 			23 메뉴 항목 Add File...
> 			24 메뉴 항목 Open Subfolders
> 			25 메뉴 항목 Paste
> 			26 메뉴 항목 Copy
> 		27 창 작업 영역 ID: 59648
> 			28 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				29 창 ID: 59648
> 					30 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
> 						31 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							32 단추 위쪽 스크롤 화살표 ID: UpButton
> 							33 단추 페이지 위로 ID: UpPageButton
> 							34 위치 조정 위치 ID: ScrollbarThumb
> 							35 단추 페이지 아래로 ID: DownPageButton
> 							36 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						37 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							38 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							39 위치 조정 위치 ID: ScrollbarThumb
> 							40 단추 페이지 오른쪽으로 ID: DownPageButton
> 							41 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						42 위치 조정 (disabled)
> 			43 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				44 창 ID: 59648
> 					45 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						46 단추 위쪽 스크롤 화살표 ID: UpButton
> 						47 위치 조정 위치 ID: ScrollbarThumb
> 						48 단추 페이지 아래로 ID: DownPageButton
> 						49 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					50 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						51 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						52 위치 조정 위치 ID: ScrollbarThumb
> 						53 단추 페이지 오른쪽으로 ID: DownPageButton
> 						54 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					55 위치 조정 (disabled)
> 			56 창 Elmo_4 Secondary Actions: Raise ID: 65286
> 				57 창 ID: 59648
> 					58 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						59 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							60 단추 위쪽 스크롤 화살표 ID: UpButton
> 							61 위치 조정 위치 ID: ScrollbarThumb
> 							62 단추 페이지 아래로 ID: DownPageButton
> 							63 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						64 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							65 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							66 위치 조정 위치 ID: ScrollbarThumb
> 							67 단추 페이지 오른쪽으로 ID: DownPageButton
> 							68 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						69 위치 조정 (disabled)
> 			70 창 EtherCAT_Network Secondary Actions: Raise ID: 65284
> 				71 창 ID: 59648
> 					72 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						73 단추 위쪽 스크롤 화살표 ID: UpButton
> 						74 위치 조정 위치 ID: ScrollbarThumb
> 						75 단추 페이지 아래로 ID: DownPageButton
> 						76 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			77 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				78 창 ID: 59648
> 					79 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := ID: 10000
> 						80 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							81 단추 위쪽 스크롤 화살표 ID: UpButton
> 							82 단추 페이지 위로 ID: UpPageButton
> 							83 위치 조정 위치 ID: ScrollbarThumb
> 							84 단추 페이지 아래로 ID: DownPageButton
> 							85 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						86 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							87 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							88 위치 조정 위치 ID: ScrollbarThumb
> 							89 단추 페이지 오른쪽으로 ID: DownPageButton
> 							90 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						91 위치 조정 (disabled)
> 			92 창 HW_Network Secondary Actions: Raise ID: 65282
> 				93 창 ID: 59648
> 			94 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				95 창 ID: 59648
> 					96 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						97 단추 위쪽 스크롤 화살표 ID: UpButton
> 						98 위치 조정 위치 ID: ScrollbarThumb
> 						99 단추 페이지 아래로 ID: DownPageButton
> 						100 단추 아래쪽 스크롤 화살표 ID: DownButton
> 		101 상태 표시줄 ID: 59393
> 			102 텍스트
> 			103 텍스트
> 			104 텍스트
> 			105 텍스트
> 			106 텍스트
> 			107 텍스트 Offline
> 			108 텍스트
> 			109 텍스트 NUM
> 			110 텍스트
> 		111 창 xtpBarTop ID: 59419
> 			112 도구 모음 Edit
> 				113 단추 Toggle bookmark
> 				114 단추 (disabled) Previous bookmark
> 				115 단추 (disabled) Next bookmark
> 				116 단추 (disabled) Delete all bookmarks
> 				117 단추 (disabled) Previous bookmark in this file
> 				118 단추 (disabled) Next bookmark in this file
> 				119 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				120 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				121 단추 (disabled) Unindent (Shift+Tab)
> 				122 단추 (disabled) Indent (Tab)
> 			123 도구 모음 Macros Manager
> 				124 메뉴 항목 Macros
> 			125 도구 모음 Layout Manager
> 				126 메뉴 항목 Layouts
> 			127 도구 모음 Toolbox
> 				128 단추 DataAnalyzer
> 				129 단추 Interpreter
> 				130 단추 DiasDrive
> 				131 단추 PLC Diagnosis
> 				132 단추 Hardware Editor
> 				133 단추 Graphical Hardware Editor
> 				134 단추 Connection Manager
> 				135 단추 Task Configuration
> 			136 도구 모음 Net Edit
> 				137 단추 (disabled) Select
> 				138 단추 (disabled) Move view
> 				139 단추 (disabled) Insert comment
> 				140 단추 (disabled) Zoom(+/-)
> 				141 단추 (disabled) Zoom to all
> 				142 단추 (disabled) Zoom selection
> 			143 도구 모음 Debug
> 				144 단추 Go online (Alt+F6)
> 				145 단추 Change Online Settings
> 				146 메뉴 항목 Online Connection

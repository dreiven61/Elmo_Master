> 				230 메뉴 항목 ANALYZE
> 				231 메뉴 항목 TOOLS
> 				232 메뉴 항목 EXTRAS
> 				233 메뉴 항목 WINDOW
> 				234 메뉴 항목 HELP
> 		235 창 Splitter ID: 411855768
> 		236 창 Splitter ID: 411851736
> 		237 Tab Output ID: 409867992
> 			238 창 ID: 1200
> 				239 창 ID: 1200
> 					240 LIST ID: 1204
> 					241 스크롤 막대 (disabled) ID: 59904
> 						242 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						243 위치 조정 위치 ID: ScrollbarThumb
> 						244 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			245 탭 항목 (selectable) Python Script
> 			246 탭 항목 (selectable) Output
> 			247 탭 항목 (selectable) Debugger
> 			248 단추 Close
> 		249 창 Splitter ID: 411854424
> 		250 Tab Class View ID: 409868448
> 			251 트리 ID: 103
> 				252 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					253 콘솔 트리 (selectable) External
> 					254 콘솔 트리 (selectable) Sigmatek
> 					255 콘솔 트리 (selectable) _TCPIPServer_RT
> 					256 콘솔 트리 (selectable) Elmo_1
> 					257 콘솔 트리 (selectable) Elmo_2
> 					258 콘솔 트리 (selectable) Elmo_3
> 					259 콘솔 트리 (selectable) Elmo_4
> 					260 콘솔 트리 (selectable) LMCDiagnosticsService
> 					261 콘솔 트리 (selectable) LMCEcatInputLatch
> 					262 콘솔 트리 (selectable) LMCRecorderStore
> 					263 콘솔 트리 (selectable) TCPMotionInterface
> 			264 탭 항목 (selectable) Lib
> 			265 탭 항목 (selectable) File
> 			266 탭 항목 (selectable) Global
> 			267 탭 항목 (selectable) Net
> 			268 탭 항목 (selectable) Class
> 			269 단추 Close
> 		270 Tab Properties ID: 409871640
> 			271 창 ID: 121918456
> 				272 TABLE Properties Window ID: 127184512
> 					273 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						274 단추 위쪽 스크롤 화살표 ID: UpButton
> 						275 위치 조정 위치 ID: ScrollbarThumb
> 						276 단추 페이지 아래로 ID: DownPageButton
> 						277 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					278 custom Name
> 					279 custom Revision
> 					280 custom GUID
> 					281 custom BaseClass
> 					282 custom Task Settings
> 					283 custom Sigmatek
> 					284 custom OSInterface
> 					285 custom IconPath
> 					286 custom SharedCommandTable
> 					287 custom Objectsize
> 					288 custom Singleton
> 					289 custom Hidden
> 					290 custom Deprecated
> 					291 custom GCCOptions
> 					292 custom Comment
> 					293 custom Filename
> 				294 도구 모음 ID: 59392
> 					295 단추
> 					296 단추
> 			297 탭 항목 (selectable) Properties
> 			298 단추 Close
> 		299 대화 상자 Find Secondary Actions: Raise
> 			300 텍스트 Find What:
> 			301 콤보 상자 Find What: ID: 1115
> 				302 편집 Find What: ID: 1001
> 				303 단추 열기 ID: DropDown
> 			304 확인란 Match Case ID: 1116
> 			305 확인란 Whole Word ID: 1117
> 			306 확인란 (disabled) Include Properties ID: 1776
> 			307 확인란 Show in Find Results ID: 1430
> 			308 확인란 Show in Find Results 2 ID: 1431
> 			309 확인란 Ignore Whitespaces ID: 1423
> 			310 확인란 Ignore Return ID: 1424
> 			311 확인란 Ignore Comments ID: 1799
> 			312 확인란 Use regular expression ID: 1774
> 			313 그룹 Direction:
> 			314 라디오 단추 (selectable) Up ID: 1118
> 			315 라디오 단추 (selectable) Down ID: 1119
> 			316 단추 Find ID: 1120
> 			317 단추 Mark All ID: 1729
> 			318 단추 Clear all marks ID: 1730
> 			319 단추 Close ID: 1
> 			320 단추 Cancel ID: 2
> 			321 제목 표시줄
> 				322 메뉴 모음 시스템 ID: MenuBar
> 					323 메뉴 항목 시스템
> 				324 단추 닫기
>
> The focused UI element is 30 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000.
> ```
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
>   "title": "Dismiss LASAL Find dialog",
>   "code": "{\n  const observation = globalThis.state2;\n  globalThis.state2 = null;\n  await sky.press_key({window:observation.window, key:\"Escape\"});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write(String(state2.accessibility?.tree || \"\"));\n}"
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
> 			17 창 Motion_Network Secondary Actions: Raise ID: 65285
> 				18 창 ID: 59648
> 					19 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						20 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21 위치 조정 위치 ID: ScrollbarThumb
> 						22 단추 페이지 아래로 ID: DownPageButton
> 						23 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					24 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						25 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						26 위치 조정 위치 ID: ScrollbarThumb
> 						27 단추 페이지 오른쪽으로 ID: DownPageButton
> 						28 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					29 위치 조정 (disabled)
> 			30 창 Comm_Network Secondary Actions: Raise ID: 65286
> 				31 창 ID: 59648
> 					32 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						33 단추 위쪽 스크롤 화살표 ID: UpButton
> 						34 위치 조정 위치 ID: ScrollbarThumb
> 						35 단추 페이지 아래로 ID: DownPageButton
> 						36 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					37 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						38 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						39 위치 조정 위치 ID: ScrollbarThumb
> 						40 단추 페이지 오른쪽으로 ID: DownPageButton
> 						41 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					42 위치 조정 (disabled)
> 			43 창 Hardware Editor Secondary Actions: Raise ID: 65284
> 				44 창 ID: 59648
> 					45 창 xtpBarTop ID: 59419
> 						46 도구 모음 Hardware Editor
> 							47 단추 Hardware Editor Configuration Settings
> 							48 단추 Calculate Resources of Project
> 							49 단추 (disabled) Upload Hardware Tree from PLC
> 							50 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							51 단추 Show Detail Mode\Show Configuration Mode
> 							52 단추 Generates the ENI File of the current project
> 					53 창 ID: 59648
> 						54 트리 ID: 1
> 							55 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								56 단추 위쪽 스크롤 화살표 ID: UpButton
> 								57 단추 페이지 위로 ID: UpPageButton
> 								58 위치 조정 위치 ID: ScrollbarThumb
> 								59 단추 페이지 아래로 ID: DownPageButton
> 								60 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							61 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								62 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								63 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								64 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								65 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								66 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								67 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								68 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								69 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								70 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								71 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								72 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								73 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								74 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								75 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								76 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								77 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								78 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								79 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								80 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								81 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								82 콘솔 트리 (selectable) ALARM:00, Empty
> 								83 콘솔 트리 (selectable) SDIAS:00, Empty
> 								84 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								85 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							86 콘솔 트리 (selectable) Unplaced Module(s)
> 			87 창 Elmo_4 Secondary Actions: Raise ID: 65283
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

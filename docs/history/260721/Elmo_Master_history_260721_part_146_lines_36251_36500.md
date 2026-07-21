> }
> ```
>
> ```text
> point (2904, 632) is outside window bounds { originX: 0, originY: 0, width: 1450, height: 763 }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reobserve LASAL Find dialog",
>   "code": "globalThis.state2 = await sky.get_window_state({window:targetWindow2, include_screenshot:true, include_text:true});\nglobalThis.targetWindow2 = state2.window;\nnodeRepl.write(String(state2.accessibility?.tree || \"\"));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 대화 상자 Find Secondary Actions: Raise
> 			2 텍스트 Find What:
> 			3 콤보 상자 Find What: ID: 1115
> 				4 편집 Find What: ID: 1001
> 				5 단추 열기 ID: DropDown
> 			6 확인란 Match Case ID: 1116
> 			7 확인란 Whole Word ID: 1117
> 			8 확인란 (disabled) Include Properties ID: 1776
> 			9 확인란 Show in Find Results ID: 1430
> 			10 확인란 Show in Find Results 2 ID: 1431
> 			11 확인란 Ignore Whitespaces ID: 1423
> 			12 확인란 Ignore Return ID: 1424
> 			13 확인란 Ignore Comments ID: 1799
> 			14 확인란 Use regular expression ID: 1774
> 			15 그룹 Direction:
> 			16 라디오 단추 (selectable) Up ID: 1118
> 			17 라디오 단추 (selectable) Down ID: 1119
> 			18 단추 Find ID: 1120
> 			19 단추 Mark All ID: 1729
> 			20 단추 Clear all marks ID: 1730
> 			21 단추 Close ID: 1
> 			22 단추 Cancel ID: 2
> 			23 제목 표시줄
> 				24 메뉴 모음 시스템 ID: MenuBar
> 					25 메뉴 항목 시스템
> 				26 단추 닫기
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
> 							38 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							39 위치 조정 위치 ID: ScrollbarThumb
> 							40 단추 페이지 오른쪽으로 ID: DownPageButton
> 							41 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						42 위치 조정 (disabled)
> 			43 창 Motion_Network Secondary Actions: Raise ID: 65285
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
> 			56 창 Comm_Network Secondary Actions: Raise ID: 65286
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
> 			69 창 Hardware Editor Secondary Actions: Raise ID: 65284
> 				70 창 ID: 59648
> 					71 창 xtpBarTop ID: 59419
> 						72 도구 모음 Hardware Editor
> 							73 단추 Hardware Editor Configuration Settings
> 							74 단추 Calculate Resources of Project
> 							75 단추 (disabled) Upload Hardware Tree from PLC
> 							76 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							77 단추 Show Detail Mode\Show Configuration Mode
> 							78 단추 Generates the ENI File of the current project
> 					79 창 ID: 59648
> 						80 트리 ID: 1
> 							81 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								82 단추 위쪽 스크롤 화살표 ID: UpButton
> 								83 단추 페이지 위로 ID: UpPageButton
> 								84 위치 조정 위치 ID: ScrollbarThumb
> 								85 단추 페이지 아래로 ID: DownPageButton
> 								86 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							87 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								88 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								89 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								90 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								91 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								92 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								93 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								94 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								95 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								96 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								97 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								98 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								99 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								100 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								101 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								102 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								103 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								104 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								105 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								106 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								107 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								108 콘솔 트리 (selectable) ALARM:00, Empty
> 								109 콘솔 트리 (selectable) SDIAS:00, Empty
> 								110 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								111 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							112 콘솔 트리 (selectable) Unplaced Module(s)
> 			113 창 Elmo_4 Secondary Actions: Raise ID: 65283
> 				114 창 ID: 59648
> 					115 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						116 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							117 단추 위쪽 스크롤 화살표 ID: UpButton
> 							118 위치 조정 위치 ID: ScrollbarThumb
> 							119 단추 페이지 아래로 ID: DownPageButton
> 							120 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						121 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							122 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							123 위치 조정 위치 ID: ScrollbarThumb
> 							124 단추 페이지 오른쪽으로 ID: DownPageButton
> 							125 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						126 위치 조정 (disabled)
> 			127 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				128 창 ID: 59648
> 					129 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						130 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							131 단추 위쪽 스크롤 화살표 ID: UpButton
> 							132 위치 조정 위치 ID: ScrollbarThumb
> 							133 단추 페이지 아래로 ID: DownPageButton
> 							134 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						135 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							136 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							137 위치 조정 위치 ID: ScrollbarThumb
> 							138 단추 페이지 오른쪽으로 ID: DownPageButton
> 							139 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						140 위치 조정 (disabled)
> 			141 창 HW_Network Secondary Actions: Raise ID: 65281
> 				142 창 ID: 59648
> 					143 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						144 단추 위쪽 스크롤 화살표 ID: UpButton
> 						145 위치 조정 위치 ID: ScrollbarThumb
> 						146 단추 페이지 아래로 ID: DownPageButton
> 						147 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					148 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						149 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						150 위치 조정 위치 ID: ScrollbarThumb
> 						151 단추 페이지 오른쪽으로 ID: DownPageButton
> 						152 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					153 위치 조정 (disabled)
> 		154 상태 표시줄 ID: 59393
> 			155 텍스트
> 			156 텍스트 TCPMotionInterface::CyWork
> 			157 텍스트
> 			158 텍스트 Ln 83 Col 28
> 			159 텍스트
> 			160 텍스트 Offline
> 			161 텍스트
> 			162 텍스트 NUM
> 			163 텍스트
> 		164 창 xtpBarTop ID: 59419
> 			165 도구 모음 Script
> 			166 도구 모음 Edit
> 				167 단추 Toggle bookmark
> 				168 단추 (disabled) Previous bookmark
> 				169 단추 (disabled) Next bookmark
> 				170 단추 (disabled) Delete all bookmarks
> 				171 단추 (disabled) Previous bookmark in this file
> 				172 단추 (disabled) Next bookmark in this file
> 				173 단추 Comment selected text (Ctrl+Shift+C)
> 				174 단추 Remove comment (Ctrl+Shift+X)
> 				175 단추 Unindent (Shift+Tab)
> 				176 단추 Indent (Tab)
> 			177 도구 모음 Macros Manager
> 				178 메뉴 항목 Macros
> 			179 도구 모음 Layout Manager
> 				180 메뉴 항목 Layouts
> 			181 도구 모음 Toolbox
> 				182 단추 DataAnalyzer
> 				183 메뉴 항목 Toolbar Options
> 			184 도구 모음 Net Edit
> 				185 단추 (disabled) Select
> 				186 메뉴 항목 Toolbar Options
> 			187 도구 모음 Debug
> 				188 단추 Go online (Alt+F6)
> 				189 단추 Change Online Settings
> 				190 메뉴 항목 Online Connection
> 				191 단추 (disabled) Set Online Connection For Current Project
> 				192 단추 (disabled) Download (F6)
> 				193 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				194 단추 (disabled) Download Module on the Fly
> 				195 단추 (disabled) Save Project on PLC
> 				196 단추 (disabled) Start (F7)
> 				197 단추 (disabled) Reset (F8)
> 				198 단추 Toggle breakpoint (F4)
> 				199 단추 Create condition breakpoint
> 				200 메뉴 항목 Toolbar Options
> 			201 도구 모음 Build
> 				202 메뉴 항목 Target Architecture
> 				203 단추 Build changes (F9)
> 				204 단추 Rebuild project (Strg+F9)
> 				205 단추 (disabled) Cancel building (Ctrl+Break)
> 				206 단추 Link project
> 			207 도구 모음 Standard
> 				208 단추 New project (Strg+N)
> 				209 단추 Open a file (Strg+Shift+O)
> 				210 단추 Close active document (Strg+F4)
> 				211 단추 (disabled) Save file (Strg+S)
> 				212 단추 Open project (Strg+O)
> 				213 단추 (disabled) Save project changes (Strg+Shift+S)
> 				214 단추 Close project
> 				215 단추 Print
> 				216 단추 Cut (Strg+X)
> 				217 단추 Copy (Strg+C)
> 				218 단추 (disabled) Paste (Strg+V)
> 				219 메뉴 항목 (disabled) Undo (Strg+Z)
> 				220 메뉴 항목 (disabled) Redo (Strg+Y)
> 				221 단추 Navigate Backward (Alt+Left)
> 				222 단추 (disabled) Navigate Forward (Alt +Right)
> 			223 메뉴 모음 Menu Bar
> 				224 메뉴 항목 FILE
> 				225 메뉴 항목 EDIT
> 				226 메뉴 항목 VIEW
> 				227 메뉴 항목 PROJECT
> 				228 메뉴 항목 BUILD
> 				229 메뉴 항목 DEBUG

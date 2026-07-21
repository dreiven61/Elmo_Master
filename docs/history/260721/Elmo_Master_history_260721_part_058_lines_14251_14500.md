> 						296 단추 위쪽 스크롤 화살표 ID: UpButton
> 						297 위치 조정 위치 ID: ScrollbarThumb
> 						298 단추 페이지 아래로 ID: DownPageButton
> 						299 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					300 custom Name
> 					301 custom Class
> 					302 custom DataType
> 					303 custom Type
> 					304 custom Required
> 					305 custom DefaultInitValue
> 					306 custom Comment
> 				307 도구 모음 ID: 59392
> 					308 단추
> 					309 단추
> 			310 탭 항목 (selectable) Properties
> 			311 단추 Close
>
> The focused UI element is 13 트리 ID: 1.
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
>   "title": "LASAL Drive 클라이언트 추가 메뉴 열기",
>   "code": "await sky.click({window:globalThis.targetWindow,element_index:281,mouse_button:'right'}); globalThis.state=null; nodeRepl.write('context menu requested');"
> }
> ```
>
> ```text
> context menu requested
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 클라이언트 메뉴 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 창
> 		2 창
> 		3 메뉴
> 			4 메뉴 항목 New Client
> 			5 메뉴 항목 Add Separator
> 			6 메뉴 항목 Paste
> 			7 메뉴 항목 Copy
> 		8 창 작업 영역 ID: 59648
> 			9 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				10 창 ID: 59648
> 					11 창 xtpBarTop ID: 59419
> 						12 도구 모음 Hardware Editor
> 							13 단추 Hardware Editor Configuration Settings
> 							14 단추 Calculate Resources of Project
> 							15 단추 (disabled) Upload Hardware Tree from PLC
> 							16 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							17 단추 Show Detail Mode\Show Configuration Mode
> 							18 단추 Generates the ENI File of the current project
> 					19 창 ID: 59648
> 						20 트리 ID: 1
> 							21 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								22 단추 위쪽 스크롤 화살표 ID: UpButton
> 								23 단추 페이지 위로 ID: UpPageButton
> 								24 위치 조정 위치 ID: ScrollbarThumb
> 								25 단추 페이지 아래로 ID: DownPageButton
> 								26 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							27 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								28 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]->
> 								29 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								30 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								31 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								32 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								33 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								34 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								35 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								36 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								37 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								38 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								39 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								40 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								41 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								42 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								43 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								44 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								45 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								46 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								47 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 									48 콘솔 트리 (selectable) Class State (ClassState) <-[]->
> 									49 콘솔 트리 (selectable) USB:00, Empty
> 									50 콘솔 트리 (selectable) CAN:00, Empty
> 									51 콘솔 트리 (selectable) Ethernet:00:0, Empty
> 								52 콘솔 트리 (selectable) ALARM:00, Empty
> 								53 콘솔 트리 (selectable) SDIAS:00, Empty
> 								54 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 									55 콘솔 트리 (selectable) ---------------------- General -----------------------
> 									56 콘솔 트리 (selectable) Class State (ClassState) <-[]->
> 									57 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 									58 콘솔 트리 (selectable) Slave State (SlaveState) <-[]->
> 									59 콘솔 트리 (selectable) Device Address (DeviceAdress) <-[]->
> 									60 콘솔 트리 (selectable) Vendor ID (VendorID) <-[]->
> 									61 콘솔 트리 (selectable) Product Code (ProductCode) <-[]->
> 									62 콘솔 트리 (selectable) Revision No (RevisionNo) <-[]->
> 									63 콘솔 트리 (selectable) Serial No (SerialNo) <-[]->
> 									64 콘솔 트리 (selectable) Device Name (DeviceName) <-[]->
> 									65 콘솔 트리 (selectable) Product Revision (ProductRevision) <-[]->
> 									66 콘솔 트리 (selectable) AL StatusCode (AL_StatusCode) <-[]->
> 									67 콘솔 트리 (selectable) Online (Online) <-[]-> _LMCAxis1.HWReady
> 									68 콘솔 트리 (selectable) ---------------------- Axis -----------------------
> 									69 콘솔 트리 (selectable) Act Position (ActPos) <-[]-> _LMCAxis1.ActPosition
> 									70 콘솔 트리 (selectable) Set Position (SetPos) <-[]-> PosController1.oOutputSignal
> 									71 콘솔 트리 (selectable) Act Speed (ActSpeed) <-[]->
> 									72 콘솔 트리 (selectable) Set Speed (SetSpeed) <-[]->
> 									73 콘솔 트리 (selectable) Act Torque (ActTorque) <-[]->
> 									74 콘솔 트리 (selectable) Set Torque (SetTorque) <-[]->
> 									75 콘솔 트리 (selectable) Act Xw (ActXw) <-[]->
> 									76 콘솔 트리 (selectable) State Word (StateWord) <-[]->
> 									77 콘솔 트리 (selectable) Control Word (ControlWord) <-[]->
> 									78 콘솔 트리 (selectable) Axis Enable (AxEnable) <-[]-> PosController1.DriveEnabled
> 									79 콘솔 트리 (selectable) Axis Error (AxError) <-[]-> _LMCAxis1.HWError
> 									80 콘솔 트리 (selectable) Axis Error Quit (AxErrorQuit) <-[]-> PosController1.HWErrorQuit
> 									81 콘솔 트리 (selectable) Outputs_DigitalOutputs (Outputs_DigitalOutputs) <-[]->
> 									82 콘솔 트리 (selectable) Inputs_DigitalInputs (Inputs_DigitalInputs) <-[]->
> 									83 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_2 (Elmo_21)
> 								84 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							85 콘솔 트리 (selectable) Unplaced Module(s)
> 			86 창 Motion_Network Secondary Actions: Raise ID: 65284
> 				87 창 ID: 59648
> 					88 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						89 단추 위쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 페이지 아래로 ID: DownPageButton
> 						92 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					93 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						94 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						95 위치 조정 위치 ID: ScrollbarThumb
> 						96 단추 페이지 오른쪽으로 ID: DownPageButton
> 						97 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					98 위치 조정 (disabled)
> 			99 창 HW_Network Secondary Actions: Raise ID: 65281
> 				100 창 ID: 59648
> 					101 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						102 단추 위쪽 스크롤 화살표 ID: UpButton
> 						103 위치 조정 위치 ID: ScrollbarThumb
> 						104 단추 페이지 아래로 ID: DownPageButton
> 						105 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					106 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						107 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						108 위치 조정 위치 ID: ScrollbarThumb
> 						109 단추 페이지 오른쪽으로 ID: DownPageButton
> 						110 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					111 위치 조정 (disabled)
> 			112 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				113 창 ID: 59648
> 					114 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if CurrentSock = dSock then CurrentSock := 0; SessionEpoch += 1; if SessionEpoch = 0 ID: 10000
> 						115 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							116 단추 위쪽 스크롤 화살표 ID: UpButton
> 							117 단추 페이지 위로 ID: UpPageButton
> 							118 위치 조정 위치 ID: ScrollbarThumb
> 							119 단추 페이지 아래로 ID: DownPageButton
> 							120 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						121 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							122 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							123 위치 조정 위치 ID: ScrollbarThumb
> 							124 단추 페이지 오른쪽으로 ID: DownPageButton
> 							125 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						126 위치 조정 (disabled)
> 			127 창 Comm_Network Secondary Actions: Raise ID: 65283
> 				128 창 ID: 59648
> 					129 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						130 단추 위쪽 스크롤 화살표 ID: UpButton
> 						131 위치 조정 위치 ID: ScrollbarThumb
> 						132 단추 페이지 아래로 ID: DownPageButton
> 						133 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					134 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						135 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						136 위치 조정 위치 ID: ScrollbarThumb
> 						137 단추 페이지 오른쪽으로 ID: DownPageButton
> 						138 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					139 위치 조정 (disabled)
> 			140 창 Elmo_4 Secondary Actions: Raise ID: 65285
> 				141 창 ID: 59648
> 					142 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						143 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							144 단추 위쪽 스크롤 화살표 ID: UpButton
> 							145 위치 조정 위치 ID: ScrollbarThumb
> 							146 단추 페이지 아래로 ID: DownPageButton
> 							147 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						148 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							149 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							150 위치 조정 위치 ID: ScrollbarThumb
> 							151 단추 페이지 오른쪽으로 ID: DownPageButton
> 							152 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						153 위치 조정 (disabled)
> 			154 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				155 창 ID: 59648
> 					156 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						157 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							158 단추 위쪽 스크롤 화살표 ID: UpButton
> 							159 위치 조정 위치 ID: ScrollbarThumb
> 							160 단추 페이지 아래로 ID: DownPageButton
> 							161 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						162 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							163 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							164 위치 조정 위치 ID: ScrollbarThumb
> 							165 단추 페이지 오른쪽으로 ID: DownPageButton
> 							166 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						167 위치 조정 (disabled)
> 		168 상태 표시줄 ID: 59393
> 			169 텍스트
> 			170 텍스트
> 			171 텍스트
> 			172 텍스트
> 			173 텍스트
> 			174 텍스트
> 			175 텍스트
> 			176 텍스트
> 			177 텍스트 NUM
> 			178 텍스트
> 		179 창 xtpBarTop ID: 59419
> 			180 도구 모음 Script
> 			181 도구 모음 Edit
> 				182 단추 (disabled) Toggle bookmark
> 				183 단추 (disabled) Previous bookmark
> 				184 단추 (disabled) Next bookmark
> 				185 단추 (disabled) Delete all bookmarks
> 				186 단추 (disabled) Previous bookmark in this file
> 				187 단추 (disabled) Next bookmark in this file
> 				188 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				189 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				190 단추 (disabled) Unindent (Shift+Tab)
> 				191 단추 (disabled) Indent (Tab)
> 			192 도구 모음 Macros Manager
> 				193 메뉴 항목 Macros
> 			194 도구 모음 Layout Manager
> 				195 메뉴 항목 Layouts
> 			196 도구 모음 Toolbox
> 				197 단추 DataAnalyzer

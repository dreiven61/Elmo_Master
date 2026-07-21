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
> 				270 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					271 단추 위쪽 스크롤 화살표 ID: UpButton
> 					272 위치 조정 위치 ID: ScrollbarThumb
> 					273 단추 페이지 아래로 ID: DownPageButton
> 					274 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				275 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					276 콘솔 트리 (selectable) External
> 					277 콘솔 트리 (selectable) Sigmatek
> 					278 콘솔 트리 (selectable) _TCPIPServer_RT
> 					279 콘솔 트리 (selectable) Elmo_1
> 					280 콘솔 트리 (selectable) Elmo_2
> 					281 콘솔 트리 (selectable) Elmo_3
> 					282 콘솔 트리 (selectable) Elmo_4
> 					283 콘솔 트리 (selectable) LMCDiagnosticsService
> 					284 콘솔 트리 (selectable) LMCEcatInputLatch
> 						285 콘솔 트리 (selectable) Servers
> 						286 콘솔 트리 (selectable) Clients
> 							287 콘솔 트리 (selectable) EcatMaster
> 							288 콘솔 트리 (selectable) Drive1
> 							289 콘솔 트리 (selectable) Drive2
> 							290 콘솔 트리 (selectable) Drive3
> 							291 콘솔 트리 (selectable) Drive4
> 						292 콘솔 트리 (selectable) Methods
> 						293 콘솔 트리 (selectable) Dependencies
> 					294 콘솔 트리 (selectable) TCPMotionInterface
> 			295 탭 항목 (selectable) Lib
> 			296 탭 항목 (selectable) File
> 			297 탭 항목 (selectable) Global
> 			298 탭 항목 (selectable) Net
> 			299 탭 항목 (selectable) Class
> 			300 단추 Close
> 		301 Tab Properties ID: 125485008
> 			302 창 ID: 290002192
> 				303 TABLE Properties Window ID: 293314152
> 					304 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						305 단추 위쪽 스크롤 화살표 ID: UpButton
> 						306 위치 조정 위치 ID: ScrollbarThumb
> 						307 단추 페이지 아래로 ID: DownPageButton
> 						308 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					309 custom Name
> 					310 custom Class
> 					311 custom DataType
> 					312 custom Type
> 					313 custom Required
> 					314 custom DefaultInitValue
> 					315 custom Comment
> 				316 도구 모음 ID: 59392
> 					317 단추
> 					318 단추
> 			319 탭 항목 (selectable) Properties
> 			320 단추 Close
>
> The focused UI element is 303 TABLE Properties Window ID: 293314152.
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
>   "title": "Drive2 설정 선택",
>   "code": "await sky.click({window:globalThis.targetWindow,element_index:289}); globalThis.state=null; nodeRepl.write('Drive2 selected');"
> }
> ```
>
> ```text
> Drive2 selected
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive2 기본 설정 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				3 창 ID: 59648
> 					4 창 xtpBarTop ID: 59419
> 						5 도구 모음 Hardware Editor
> 							6 단추 Hardware Editor Configuration Settings
> 							7 단추 Calculate Resources of Project
> 							8 단추 (disabled) Upload Hardware Tree from PLC
> 							9 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							10 단추 Show Detail Mode\Show Configuration Mode
> 							11 단추 Generates the ENI File of the current project
> 					12 창 ID: 59648
> 						13 트리 ID: 1
> 							14 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								15 단추 위쪽 스크롤 화살표 ID: UpButton
> 								16 단추 페이지 위로 ID: UpPageButton
> 								17 위치 조정 위치 ID: ScrollbarThumb
> 								18 단추 페이지 아래로 ID: DownPageButton
> 								19 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							20 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								21 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]->
> 								22 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								23 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								24 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								25 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								26 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								27 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								28 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								29 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								30 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								31 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								32 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								33 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								34 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								35 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								36 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								37 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								38 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								39 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								40 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 									41 콘솔 트리 (selectable) Class State (ClassState) <-[]->
> 									42 콘솔 트리 (selectable) USB:00, Empty
> 									43 콘솔 트리 (selectable) CAN:00, Empty
> 									44 콘솔 트리 (selectable) Ethernet:00:0, Empty
> 								45 콘솔 트리 (selectable) ALARM:00, Empty
> 								46 콘솔 트리 (selectable) SDIAS:00, Empty
> 								47 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 									48 콘솔 트리 (selectable) ---------------------- General -----------------------
> 									49 콘솔 트리 (selectable) Class State (ClassState) <-[]->
> 									50 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 									51 콘솔 트리 (selectable) Slave State (SlaveState) <-[]->
> 									52 콘솔 트리 (selectable) Device Address (DeviceAdress) <-[]->
> 									53 콘솔 트리 (selectable) Vendor ID (VendorID) <-[]->
> 									54 콘솔 트리 (selectable) Product Code (ProductCode) <-[]->
> 									55 콘솔 트리 (selectable) Revision No (RevisionNo) <-[]->
> 									56 콘솔 트리 (selectable) Serial No (SerialNo) <-[]->
> 									57 콘솔 트리 (selectable) Device Name (DeviceName) <-[]->
> 									58 콘솔 트리 (selectable) Product Revision (ProductRevision) <-[]->
> 									59 콘솔 트리 (selectable) AL StatusCode (AL_StatusCode) <-[]->
> 									60 콘솔 트리 (selectable) Online (Online) <-[]-> _LMCAxis1.HWReady
> 									61 콘솔 트리 (selectable) ---------------------- Axis -----------------------
> 									62 콘솔 트리 (selectable) Act Position (ActPos) <-[]-> _LMCAxis1.ActPosition
> 									63 콘솔 트리 (selectable) Set Position (SetPos) <-[]-> PosController1.oOutputSignal
> 									64 콘솔 트리 (selectable) Act Speed (ActSpeed) <-[]->
> 									65 콘솔 트리 (selectable) Set Speed (SetSpeed) <-[]->
> 									66 콘솔 트리 (selectable) Act Torque (ActTorque) <-[]->
> 									67 콘솔 트리 (selectable) Set Torque (SetTorque) <-[]->
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

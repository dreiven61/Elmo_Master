>     "id": 13898776,
>     "title": "MMCLibDotNETTest - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 4461624,
>     "title": "Windows-SSD (C:) - 파일 탐색기"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 19207442,
>     "title": "Elmo_manual - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe",
>     "id": 3216470,
>     "title": "LasalApiWpfTestApp - Microsoft Visual Studio"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 266798,
>     "title": "02_Example_Program - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Program Files\\Microsoft Office\\Office16\\EXCEL.EXE",
>     "id": 69598,
>     "title": "EtherCAT Controller(Master) 요구 사양.xlsx  [제한된 보기] - Excel"
>   },
>   {
>     "app": "process:C:\\Program Files\\GitExtensions\\GitExtensions.exe",
>     "id": 134470,
>     "title": "01.WaferTransferRobot (ssh_jc_manual_update) - Git Extensions"
>   },
>   {
>     "app": "process:C:\\Program Files\\Microsoft Office\\Office16\\EXCEL.EXE",
>     "id": 200664,
>     "title": "Motion_Controller_API_정리.xlsx  [제한된 보기] - Excel"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\IPMate\\ipmate.exe",
>     "id": 1902696,
>     "title": "아이피 메이트 v2.1.2.1  [선택 어댑터 : 이더넷]"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe",
>     "id": 2299272,
>     "title": "MMCLibDotNET_Test - Microsoft Visual Studio"
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
>   "title": "LASAL 온라인 상태 재확인",
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				3 창 ID: 59648
> 					4 창 xtpBarTop ID: 59419
> 						5 도구 모음 Hardware Editor
> 							6 단추 Hardware Editor Configuration Settings
> 							7 단추 Calculate Resources of Project
> 							8 단추 Upload Hardware Tree from PLC
> 							9 단추 Compare Hardware Tree of project and PLC\Reset Online Compare
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
> 							20 콘솔 트리 (selectable, disabled) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								21 콘솔 트리 (selectable, disabled) EtherCAT Class State (ClassState) <-[]-> (_ClassOk)
> 								22 콘솔 트리 (selectable, disabled) EtherCAT State (EtherCATState) <-[]-> (ECAT_STATE_OP)
> 								23 콘솔 트리 (selectable, disabled) EtherCAT Software Version (SoftwareVersion) <-[]-> ("1.8")
> 								24 콘솔 트리 (selectable, disabled) EtherCAT Synchron (Synchron) <-[]-> (1)
> 								25 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]-> (16)
> 								26 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]-> (39)
> 								27 콘솔 트리 (selectable, disabled) EtherCAT Act Realtime Time (Act_RtTime) <-[]-> (146)
> 								28 콘솔 트리 (selectable, disabled) EtherCAT Min Realtime Time (Min_RtTime) <-[]-> (8)
> 								29 콘솔 트리 (selectable, disabled) EtherCAT Max Realtime Time (Max_RtTime) <-[]-> (224)
> 								30 콘솔 트리 (selectable, disabled) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]-> (0)
> 								31 콘솔 트리 (selectable, disabled) Sdias Class State (ClassState) <-[]-> (_NoHardware)
> 								32 콘솔 트리 (selectable, disabled) Sdias Firmware Version (FirmwareVersion) <-[]-> (16#00000120)
> 								33 콘솔 트리 (selectable, disabled) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]-> (16#00000000)
> 								34 콘솔 트리 (selectable, disabled) Sdias Retry Counter (RetryCounter) <-[]-> (0)
> 								35 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]-> (0)
> 								36 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]-> (0)
> 								37 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]-> (0)
> 								38 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]-> (0)
> 								39 콘솔 트리 (selectable, disabled) Sdias Manager Option Bits (ManagerOptionBits) <-[]-> (0)
> 								40 콘솔 트리 (selectable, disabled) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 									41 콘솔 트리 (selectable, disabled) Class State (ClassState) <-[]-> (0)
> 									42 콘솔 트리 (selectable, disabled) USB:00, Empty
> 									43 콘솔 트리 (selectable, disabled) CAN:00, Empty
> 									44 콘솔 트리 (selectable, disabled) Ethernet:00:0, Empty
> 								45 콘솔 트리 (selectable, disabled) ALARM:00, Empty
> 								46 콘솔 트리 (selectable, disabled) SDIAS:00, Empty
> 								47 콘솔 트리 (selectable, disabled) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 									48 콘솔 트리 (selectable, disabled) ---------------------- General -----------------------
> 									49 콘솔 트리 (selectable, disabled) Class State (ClassState) <-[]-> (_ClassOk)
> 									50 콘솔 트리 (selectable, disabled) EtherCAT State (EtherCATState) <-[]-> (ECAT_STATE_OP)
> 									51 콘솔 트리 (selectable, disabled) Slave State (SlaveState) <-[]-> (8)
> 									52 콘솔 트리 (selectable, disabled) Device Address (DeviceAdress) <-[]-> (16#000003E9)
> 									53 콘솔 트리 (selectable, disabled) Vendor ID (VendorID) <-[]-> (154)
> 									54 콘솔 트리 (selectable, disabled) Product Code (ProductCode) <-[]-> (16#00030924)
> 									55 콘솔 트리 (selectable, disabled) Revision No (RevisionNo) <-[]-> (16#00010420)
> 									56 콘솔 트리 (selectable, disabled) Serial No (SerialNo) <-[]-> (0)
> 									57 콘솔 트리 (selectable, disabled) Device Name (DeviceName) <-[]-> ("Slave 01 (Elmo Drive )")
> 									58 콘솔 트리 (selectable, disabled) Product Revision (ProductRevision) <-[]-> ("-")
> 									59 콘솔 트리 (selectable, disabled) AL StatusCode (AL_StatusCode) <-[]-> (0)
> 									60 콘솔 트리 (selectable, disabled) Online (Online) <-[]-> _LMCAxis1.HWReady (1)
> 									61 콘솔 트리 (selectable, disabled) ---------------------- Axis -----------------------
> 									62 콘솔 트리 (selectable, disabled) Act Position (ActPos) <-[]-> _LMCAxis1.ActPosition (2597710)
> 									63 콘솔 트리 (selectable, disabled) Set Position (SetPos) <-[]-> PosController1.oOutputSignal (2597952)
> 									64 콘솔 트리 (selectable, disabled) Act Speed (ActSpeed) <-[]-> (0)
> 									65 콘솔 트리 (selectable, disabled) Set Speed (SetSpeed) <-[]-> (0)
> 									66 콘솔 트리 (selectable, disabled) Act Torque (ActTorque) <-[]-> (0)
> 									67 콘솔 트리 (selectable, disabled) Set Torque (SetTorque) <-[]-> (0)
> 									68 콘솔 트리 (selectable, disabled) Act Xw (ActXw) <-[]-> (0)
> 									69 콘솔 트리 (selectable, disabled) State Word (StateWord) <-[]-> (2#00000000000000000000001011010000)
> 									70 콘솔 트리 (selectable, disabled) Control Word (ControlWord) <-[]-> (2#00000000000000000000000000000000)
> 									71 콘솔 트리 (selectable, disabled) Axis Enable (AxEnable) <-[]-> PosController1.DriveEnabled (0)
> 									72 콘솔 트리 (selectable, disabled) Axis Error (AxError) <-[]-> _LMCAxis1.HWError (0)
> 									73 콘솔 트리 (selectable, disabled) Axis Error Quit (AxErrorQuit) <-[]-> PosController1.HWErrorQuit (0)
> 									74 콘솔 트리 (selectable, disabled) Outputs_DigitalOutputs (Outputs_DigitalOutputs) <-[]-> (0)
> 									75 콘솔 트리 (selectable, disabled) Inputs_DigitalInputs (Inputs_DigitalInputs) <-[]-> (4128776)
> 									76 콘솔 트리 (selectable, disabled) EtherCAT_ETHERNET:01, Elmo_2 (Elmo_21)
> 								77 콘솔 트리 (selectable, disabled) MULTIVARAN:00, Empty
> 							78 콘솔 트리 (selectable, disabled) Unplaced Module(s)
> 			79 창 Motion_Network [RO] Secondary Actions: Raise ID: 65284
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
> 			92 창 HW_Network [RO] Secondary Actions: Raise ID: 65281
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
> 			105 창 TCPMotionInterface [RO] Secondary Actions: Raise ID: 65280
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
> 					120 창 xtpBarTop ID: 59419
> 						121 도구 모음 Object Editor
> 							122 단추
> 							123 단추 Debugging Context:
> 							124 메뉴 항목 Context
> 			125 창 Comm_Network [RO] Secondary Actions: Raise ID: 65283
> 				126 창 ID: 59648
> 					127 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						128 단추 위쪽 스크롤 화살표 ID: UpButton
> 						129 위치 조정 위치 ID: ScrollbarThumb
> 						130 단추 페이지 아래로 ID: DownPageButton
> 						131 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					132 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						133 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						134 위치 조정 위치 ID: ScrollbarThumb
> 						135 단추 페이지 오른쪽으로 ID: DownPageButton
> 						136 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					137 위치 조정 (disabled)
> 			138 창 Elmo_4 [RO] Secondary Actions: Raise ID: 65285
> 				139 창 ID: 59648
> 					140 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						141 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							142 단추 위쪽 스크롤 화살표 ID: UpButton
> 							143 위치 조정 위치 ID: ScrollbarThumb
> 							144 단추 페이지 아래로 ID: DownPageButton
> 							145 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						146 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							147 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							148 위치 조정 위치 ID: ScrollbarThumb
> 							149 단추 페이지 오른쪽으로 ID: DownPageButton
> 							150 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						151 위치 조정 (disabled)
> 					152 창 xtpBarTop ID: 59419
> 						153 도구 모음 Object Editor
> 							154 단추
> 							155 단추 Debugging Context:
> 							156 메뉴 항목 Context
> 			157 창 _TCPIPServer_RT [RO] Secondary Actions: Raise ID: 65282
> 				158 창 ID: 59648
> 					159 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						160 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							161 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							162 위치 조정 위치 ID: ScrollbarThumb
> 							163 단추 페이지 오른쪽으로 ID: DownPageButton
> 							164 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					165 창 xtpBarTop ID: 59419
> 						166 도구 모음 Object Editor
> 							167 단추 (disabled)
> 							168 단추 (disabled) Debugging Context:
> 							169 메뉴 항목 (disabled) Context
> 		170 상태 표시줄 ID: 59393
> 			171 텍스트
> 			172 텍스트
> 			173 텍스트
> 			174 텍스트
> 			175 텍스트
> 			176 텍스트 PLC = PC
> 			177 텍스트
> 			178 텍스트
> 			179 텍스트
> 			180 텍스트 NUM
> 			181 텍스트
> 		182 창 xtpBarTop ID: 59419
> 			183 도구 모음 Script
> 			184 도구 모음 Edit
> 				185 단추 (disabled) Toggle bookmark

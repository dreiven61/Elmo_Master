>   "code": "{\n  var observationLib2 = globalThis.state;\n  if (!observationLib2?.accessibility) throw new Error(\"Reobserve before dialog\");\n  globalThis.state = null;\n  await sky.click({ window: observationLib2.window, element_index: 3 });\n  globalThis.state = await sky.get_window_state({ window: observationLib2.window, include_screenshot: true, include_text: true });\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch", App: Lasal2.exe.
> 	0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch Secondary Actions: Raise
> 		1 대화 상자 Lasal Class 2 Secondary Actions: Raise
> 			2 단추 예(Y) ID: 6
> 			3 단추 아니요(N) ID: 7
> 			4 이미지 ID: 20
> 			5 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535
> 			6 제목 표시줄
> 				7 단추 (disabled) 닫기
> 		8 창 작업 영역 ID: 59648
> 			9 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65287
> 				10 창 ID: 59648
> 					11 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 						12 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							13 단추 위쪽 스크롤 화살표 ID: UpButton
> 							14 단추 페이지 위로 ID: UpPageButton
> 							15 위치 조정 위치 ID: ScrollbarThumb
> 							16 단추 페이지 아래로 ID: DownPageButton
> 							17 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						18 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							19 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							20 위치 조정 위치 ID: ScrollbarThumb
> 							21 단추 페이지 오른쪽으로 ID: DownPageButton
> 							22 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						23 위치 조정 (disabled)
> 			24 창 Motion_Network Secondary Actions: Raise ID: 65285
> 				25 창 ID: 59648
> 					26 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						27 단추 위쪽 스크롤 화살표 ID: UpButton
> 						28 위치 조정 위치 ID: ScrollbarThumb
> 						29 단추 페이지 아래로 ID: DownPageButton
> 						30 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					31 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						32 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						33 위치 조정 위치 ID: ScrollbarThumb
> 						34 단추 페이지 오른쪽으로 ID: DownPageButton
> 						35 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					36 위치 조정 (disabled)
> 			37 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				38 창 ID: 59648
> 					39 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 						40 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							41 단추 위쪽 스크롤 화살표 ID: UpButton
> 							42 단추 페이지 위로 ID: UpPageButton
> 							43 위치 조정 위치 ID: ScrollbarThumb
> 							44 단추 페이지 아래로 ID: DownPageButton
> 							45 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						46 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							47 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							48 위치 조정 위치 ID: ScrollbarThumb
> 							49 단추 페이지 오른쪽으로 ID: DownPageButton
> 							50 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						51 위치 조정 (disabled)
> 			52 창 Comm_Network Secondary Actions: Raise ID: 65286
> 				53 창 ID: 59648
> 					54 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						55 단추 위쪽 스크롤 화살표 ID: UpButton
> 						56 위치 조정 위치 ID: ScrollbarThumb
> 						57 단추 페이지 아래로 ID: DownPageButton
> 						58 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					59 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						60 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						61 위치 조정 위치 ID: ScrollbarThumb
> 						62 단추 페이지 오른쪽으로 ID: DownPageButton
> 						63 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					64 위치 조정 (disabled)
> 			65 창 Hardware Editor Secondary Actions: Raise ID: 65284
> 				66 창 ID: 59648
> 					67 창 xtpBarTop ID: 59419
> 						68 도구 모음 Hardware Editor
> 							69 단추 Hardware Editor Configuration Settings
> 							70 단추 Calculate Resources of Project
> 							71 단추 (disabled) Upload Hardware Tree from PLC
> 							72 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							73 단추 Show Detail Mode\Show Configuration Mode
> 							74 단추 Generates the ENI File of the current project
> 					75 창 ID: 59648
> 						76 트리 ID: 1
> 							77 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								78 단추 위쪽 스크롤 화살표 ID: UpButton
> 								79 단추 페이지 위로 ID: UpPageButton
> 								80 위치 조정 위치 ID: ScrollbarThumb
> 								81 단추 페이지 아래로 ID: DownPageButton
> 								82 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							83 콘솔 트리 (selectable, disabled) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								84 콘솔 트리 (selectable, disabled) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								85 콘솔 트리 (selectable, disabled) EtherCAT State (EtherCATState) <-[]->
> 								86 콘솔 트리 (selectable, disabled) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								87 콘솔 트리 (selectable, disabled) EtherCAT Synchron (Synchron) <-[]->
> 								88 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								89 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								90 콘솔 트리 (selectable, disabled) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								91 콘솔 트리 (selectable, disabled) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								92 콘솔 트리 (selectable, disabled) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								93 콘솔 트리 (selectable, disabled) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								94 콘솔 트리 (selectable, disabled) Sdias Class State (ClassState) <-[]->
> 								95 콘솔 트리 (selectable, disabled) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								96 콘솔 트리 (selectable, disabled) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								97 콘솔 트리 (selectable, disabled) Sdias Retry Counter (RetryCounter) <-[]->
> 								98 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								99 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								100 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								101 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								102 콘솔 트리 (selectable, disabled) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								103 콘솔 트리 (selectable, disabled) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								104 콘솔 트리 (selectable, disabled) ALARM:00, Empty
> 								105 콘솔 트리 (selectable, disabled) SDIAS:00, Empty
> 								106 콘솔 트리 (selectable, disabled) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								107 콘솔 트리 (selectable, disabled) MULTIVARAN:00, Empty
> 							108 콘솔 트리 (selectable, disabled) Unplaced Module(s)
> 			109 창 Elmo_4 Secondary Actions: Raise ID: 65283
> 				110 창 ID: 59648
> 					111 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						112 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							113 단추 위쪽 스크롤 화살표 ID: UpButton
> 							114 위치 조정 위치 ID: ScrollbarThumb
> 							115 단추 페이지 아래로 ID: DownPageButton
> 							116 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						117 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							118 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							119 위치 조정 위치 ID: ScrollbarThumb
> 							120 단추 페이지 오른쪽으로 ID: DownPageButton
> 							121 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						122 위치 조정 (disabled)
> 			123 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				124 창 ID: 59648
> 					125 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						126 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							127 단추 위쪽 스크롤 화살표 ID: UpButton
> 							128 위치 조정 위치 ID: ScrollbarThumb
> 							129 단추 페이지 아래로 ID: DownPageButton
> 							130 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						131 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							132 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							133 위치 조정 위치 ID: ScrollbarThumb
> 							134 단추 페이지 오른쪽으로 ID: DownPageButton
> 							135 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						136 위치 조정 (disabled)
> 			137 창 HW_Network Secondary Actions: Raise ID: 65281
> 				138 창 ID: 59648
> 					139 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						140 단추 위쪽 스크롤 화살표 ID: UpButton
> 						141 위치 조정 위치 ID: ScrollbarThumb
> 						142 단추 페이지 아래로 ID: DownPageButton
> 						143 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					144 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						145 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						146 위치 조정 위치 ID: ScrollbarThumb
> 						147 단추 페이지 오른쪽으로 ID: DownPageButton
> 						148 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					149 위치 조정 (disabled)
> 		150 상태 표시줄 ID: 59393
> 			151 텍스트
> 			152 텍스트
> 			153 텍스트
> 			154 텍스트
> 			155 텍스트
> 			156 텍스트 Offline
> 			157 텍스트
> 			158 텍스트 NUM
> 			159 텍스트
> 		160 창 xtpBarTop ID: 59419
> 			161 도구 모음 Script
> 			162 도구 모음 Edit
> 				163 단추 Toggle bookmark
> 				164 단추 (disabled) Previous bookmark
> 				165 단추 (disabled) Next bookmark
> 				166 단추 (disabled) Delete all bookmarks
> 				167 단추 (disabled) Previous bookmark in this file
> 				168 단추 (disabled) Next bookmark in this file
> 				169 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				170 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				171 단추 (disabled) Unindent (Shift+Tab)
> 				172 단추 (disabled) Indent (Tab)
> 			173 도구 모음 Macros Manager
> 				174 메뉴 항목 Macros
> 			175 도구 모음 Layout Manager
> 				176 메뉴 항목 Layouts
> 			177 도구 모음 Toolbox
> 				178 단추 DataAnalyzer
> 				179 메뉴 항목 Toolbar Options
> 			180 도구 모음 Net Edit
> 				181 단추 (disabled) Select
> 				182 메뉴 항목 Toolbar Options
> 			183 도구 모음 Debug
> 				184 단추 Go online (Alt+F6)
> 				185 단추 Change Online Settings
> 				186 메뉴 항목 Online Connection
> 				187 단추 (disabled) Set Online Connection For Current Project
> 				188 단추 (disabled) Download (F6)
> 				189 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				190 단추 (disabled) Download Module on the Fly
> 				191 단추 (disabled) Save Project on PLC
> 				192 단추 (disabled) Start (F7)
> 				193 단추 (disabled) Reset (F8)
> 				194 단추 Toggle breakpoint (F4)
> 				195 단추 Create condition breakpoint
> 				196 메뉴 항목 Toolbar Options
> 			197 도구 모음 Build
> 				198 메뉴 항목 Target Architecture
> 				199 단추 Build changes (F9)
> 				200 단추 Rebuild project (Strg+F9)
> 				201 단추 (disabled) Cancel building (Ctrl+Break)
> 				202 단추 Link project
> 			203 도구 모음 Standard
> 				204 단추 New project (Strg+N)
> 				205 단추 Open a file (Strg+Shift+O)
> 				206 단추 Close active document (Strg+F4)
> 				207 단추 (disabled) Save file (Strg+S)
> 				208 단추 Open project (Strg+O)
> 				209 단추 (disabled) Save project changes (Strg+Shift+S)
> 				210 단추 Close project
> 				211 단추 Print
> 				212 단추 Cut (Strg+X)
> 				213 단추 Copy (Strg+C)
> 				214 단추 Paste (Strg+V)
> 				215 메뉴 항목 Undo (Strg+Z)
> 				216 메뉴 항목 (disabled) Redo (Strg+Y)
> 				217 단추 Navigate Backward (Alt+Left)
> 				218 단추 (disabled) Navigate Forward (Alt +Right)
> 			219 메뉴 모음 Menu Bar
> 				220 메뉴 항목 FILE
> 				221 메뉴 항목 EDIT
> 				222 메뉴 항목 VIEW
> 				223 메뉴 항목 PROJECT
> 				224 메뉴 항목 BUILD
> 				225 메뉴 항목 DEBUG
> 				226 메뉴 항목 ANALYZE
> 				227 메뉴 항목 TOOLS
> 				228 메뉴 항목 EXTRAS
> 				229 메뉴 항목 WINDOW
> 				230 메뉴 항목 HELP
> 		231 창 Splitter ID: 411855768
> 		232 창 Splitter ID: 411851736
> 		233 Tab Output ID: 409867992
> 			234 창 ID: 1200
> 				235 창 ID: 1200
> 					236 LIST ID: 1201
> 						237 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							238 단추 위쪽 스크롤 화살표 ID: UpButton
> 							239 단추 페이지 위로 ID: UpPageButton
> 							240 위치 조정 위치 ID: ScrollbarThumb
> 							241 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						242 목록 항목 (selectable)
> 						243 목록 항목 (selectable)

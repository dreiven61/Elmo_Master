> 						290 콘솔 트리 (selectable) Servers
> 						291 콘솔 트리 (selectable) Clients
> 							292 콘솔 트리 (selectable) InputLatch
> 							293 콘솔 트리 (selectable) RecorderStore
> 						294 콘솔 트리 (selectable) Methods
> 						295 콘솔 트리 (selectable) Variables
> 						296 콘솔 트리 (selectable) Objects
> 						297 콘솔 트리 (selectable) Dependencies
> 					298 콘솔 트리 (selectable) LMCEcatInputLatch
> 					299 콘솔 트리 (selectable) LMCRecorderStore
> 					300 콘솔 트리 (selectable) TCPMotionInterface
> 			301 탭 항목 (selectable) Lib
> 			302 탭 항목 (selectable) File
> 			303 탭 항목 (selectable) Global
> 			304 탭 항목 (selectable) Net
> 			305 탭 항목 (selectable) Class
> 			306 단추 Close
> 		307 Tab Properties ID: 409871640
> 			308 창 ID: 121918456
> 				309 TABLE Properties Window ID: 127184512
> 					310 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						311 단추 위쪽 스크롤 화살표 ID: UpButton
> 						312 위치 조정 위치 ID: ScrollbarThumb
> 						313 단추 페이지 아래로 ID: DownPageButton
> 						314 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					315 custom Name
> 					316 custom Class
> 					317 custom DataType
> 					318 custom Type
> 					319 custom Required
> 					320 custom DefaultInitValue
> 					321 custom Disable RedefinitionVariableWarning
> 					322 custom Comment
> 				323 도구 모음 ID: 59392
> 					324 단추
> 					325 단추
> 			326 탭 항목 (selectable) Properties
> 			327 단추 Close
>
> The focused UI element is 4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000.
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
>   "title": "LASAL 프로젝트 다시 불러오기",
>   "code": "{\n  var observationClose = globalThis.state;\n  if (!observationClose?.accessibility) throw new Error(\"Reobserve before close\");\n  globalThis.state = null;\n  await sky.click({ window: observationClose.window, element_index: 203 });\n  globalThis.state = await sky.get_window_state({ window: observationClose.window, include_screenshot: true, include_text: true });\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65287
> 				3 창 ID: 59648
> 					4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
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
> 			30 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				31 창 ID: 59648
> 					32 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 						33 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							34 단추 위쪽 스크롤 화살표 ID: UpButton
> 							35 단추 페이지 위로 ID: UpPageButton
> 							36 위치 조정 위치 ID: ScrollbarThumb
> 							37 단추 페이지 아래로 ID: DownPageButton
> 							38 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						39 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							40 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							41 위치 조정 위치 ID: ScrollbarThumb
> 							42 단추 페이지 오른쪽으로 ID: DownPageButton
> 							43 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						44 위치 조정 (disabled)
> 			45 창 Comm_Network Secondary Actions: Raise ID: 65286
> 				46 창 ID: 59648
> 					47 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						48 단추 위쪽 스크롤 화살표 ID: UpButton
> 						49 위치 조정 위치 ID: ScrollbarThumb
> 						50 단추 페이지 아래로 ID: DownPageButton
> 						51 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					52 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						53 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						54 위치 조정 위치 ID: ScrollbarThumb
> 						55 단추 페이지 오른쪽으로 ID: DownPageButton
> 						56 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					57 위치 조정 (disabled)
> 			58 창 Hardware Editor Secondary Actions: Raise ID: 65284
> 				59 창 ID: 59648
> 					60 창 xtpBarTop ID: 59419
> 						61 도구 모음 Hardware Editor
> 							62 단추 Hardware Editor Configuration Settings
> 							63 단추 Calculate Resources of Project
> 							64 단추 (disabled) Upload Hardware Tree from PLC
> 							65 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							66 단추 Show Detail Mode\Show Configuration Mode
> 							67 단추 Generates the ENI File of the current project
> 					68 창 ID: 59648
> 						69 트리 ID: 1
> 							70 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								71 단추 위쪽 스크롤 화살표 ID: UpButton
> 								72 단추 페이지 위로 ID: UpPageButton
> 								73 위치 조정 위치 ID: ScrollbarThumb
> 								74 단추 페이지 아래로 ID: DownPageButton
> 								75 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							76 콘솔 트리 (selectable, disabled) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								77 콘솔 트리 (selectable, disabled) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								78 콘솔 트리 (selectable, disabled) EtherCAT State (EtherCATState) <-[]->
> 								79 콘솔 트리 (selectable, disabled) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								80 콘솔 트리 (selectable, disabled) EtherCAT Synchron (Synchron) <-[]->
> 								81 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								82 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								83 콘솔 트리 (selectable, disabled) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								84 콘솔 트리 (selectable, disabled) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								85 콘솔 트리 (selectable, disabled) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								86 콘솔 트리 (selectable, disabled) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								87 콘솔 트리 (selectable, disabled) Sdias Class State (ClassState) <-[]->
> 								88 콘솔 트리 (selectable, disabled) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								89 콘솔 트리 (selectable, disabled) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								90 콘솔 트리 (selectable, disabled) Sdias Retry Counter (RetryCounter) <-[]->
> 								91 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								92 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								93 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								94 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								95 콘솔 트리 (selectable, disabled) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								96 콘솔 트리 (selectable, disabled) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								97 콘솔 트리 (selectable, disabled) ALARM:00, Empty
> 								98 콘솔 트리 (selectable, disabled) SDIAS:00, Empty
> 								99 콘솔 트리 (selectable, disabled) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								100 콘솔 트리 (selectable, disabled) MULTIVARAN:00, Empty
> 							101 콘솔 트리 (selectable, disabled) Unplaced Module(s)
> 			102 창 Elmo_4 Secondary Actions: Raise ID: 65283
> 				103 창 ID: 59648
> 					104 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						105 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							106 단추 위쪽 스크롤 화살표 ID: UpButton
> 							107 위치 조정 위치 ID: ScrollbarThumb
> 							108 단추 페이지 아래로 ID: DownPageButton
> 							109 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						110 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							111 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							112 위치 조정 위치 ID: ScrollbarThumb
> 							113 단추 페이지 오른쪽으로 ID: DownPageButton
> 							114 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						115 위치 조정 (disabled)
> 			116 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				117 창 ID: 59648
> 					118 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						119 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							120 단추 위쪽 스크롤 화살표 ID: UpButton
> 							121 위치 조정 위치 ID: ScrollbarThumb
> 							122 단추 페이지 아래로 ID: DownPageButton
> 							123 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						124 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							125 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							126 위치 조정 위치 ID: ScrollbarThumb
> 							127 단추 페이지 오른쪽으로 ID: DownPageButton
> 							128 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						129 위치 조정 (disabled)
> 			130 창 HW_Network Secondary Actions: Raise ID: 65281
> 				131 창 ID: 59648
> 					132 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						133 단추 위쪽 스크롤 화살표 ID: UpButton
> 						134 위치 조정 위치 ID: ScrollbarThumb
> 						135 단추 페이지 아래로 ID: DownPageButton
> 						136 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					137 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						138 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						139 위치 조정 위치 ID: ScrollbarThumb
> 						140 단추 페이지 오른쪽으로 ID: DownPageButton
> 						141 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					142 위치 조정 (disabled)
> 		143 상태 표시줄 ID: 59393
> 			144 텍스트
> 			145 텍스트
> 			146 텍스트
> 			147 텍스트
> 			148 텍스트
> 			149 텍스트 Offline
> 			150 텍스트
> 			151 텍스트 NUM
> 			152 텍스트
> 		153 창 xtpBarTop ID: 59419
> 			154 도구 모음 Script
> 			155 도구 모음 Edit
> 				156 단추 Toggle bookmark
> 				157 단추 (disabled) Previous bookmark
> 				158 단추 (disabled) Next bookmark
> 				159 단추 (disabled) Delete all bookmarks
> 				160 단추 (disabled) Previous bookmark in this file
> 				161 단추 (disabled) Next bookmark in this file
> 				162 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				163 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				164 단추 (disabled) Unindent (Shift+Tab)
> 				165 단추 (disabled) Indent (Tab)
> 			166 도구 모음 Macros Manager
> 				167 메뉴 항목 Macros
> 			168 도구 모음 Layout Manager
> 				169 메뉴 항목 Layouts
> 			170 도구 모음 Toolbox
> 				171 단추 DataAnalyzer
> 				172 메뉴 항목 Toolbar Options
> 			173 도구 모음 Net Edit
> 				174 단추 (disabled) Select
> 				175 메뉴 항목 Toolbar Options
> 			176 도구 모음 Debug
> 				177 단추 Go online (Alt+F6)
> 				178 단추 Change Online Settings
> 				179 메뉴 항목 Online Connection
> 				180 단추 (disabled) Set Online Connection For Current Project
> 				181 단추 (disabled) Download (F6)
> 				182 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				183 단추 (disabled) Download Module on the Fly
> 				184 단추 (disabled) Save Project on PLC
> 				185 단추 (disabled) Start (F7)
> 				186 단추 (disabled) Reset (F8)
> 				187 단추 Toggle breakpoint (F4)
> 				188 단추 Create condition breakpoint
> 				189 메뉴 항목 Toolbar Options
> 			190 도구 모음 Build

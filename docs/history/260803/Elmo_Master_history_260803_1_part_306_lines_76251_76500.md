>       '\t\t\t\t\t\t\t36 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t37 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t39 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t40 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t41 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t42 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t43 위치 조정 (disabled)\n' +
>       '\t\t\t2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285\n' +
>       '\t\t\t\t3 창 ID: 59648\n' +
>       '\t\t\t\t\t4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000\n' +
>       '\t\t\t\t\t\t5 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t6 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t7 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t8 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t9 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t11 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t12 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t13 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t14 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t15 위치 조정 (disabled)\n' +
>       '\t\t\t16 창 Comm_Network Secondary Actions: Raise ID: 65283\n' +
>       '\t\t\t\t17 창 ID: 59648\n' +
>       '\t\t\t\t\t18 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t19 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t20 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t21 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t22 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t23 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t24 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t26 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t52418 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t27 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t28 위치 조정 (disabled)\n' +
>       '\t\t\t58 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65287\n' +
>       '\t\t\t\t59 창 ID: 59648\n' +
>       '\t\t\t\t\t60 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #d'... 24181 more characters,
>     focused_element: '191690 편집 ID: 1100837040'
>   }
> }
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
>   "title": "배열 상한 확정",
>   "code": "await sky.press_key({window:targetWindow,key:\"Return\"}); globalThis.state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write(globalThis.state);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3151338,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=76320 chars=9977 sha256=c47e53b4a3a98fbaaf505e15e206b6a4744bdb43051a6cdabb334c6308b4654f]'... 195699 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=76329 chars=9977 sha256=93202d42bbcfc72ccf2dc34f90c3c888217fac12f896ed05ca69612aa1991fc0]'... 185931 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
>       '\t\t\t44 창 Motion_Network Secondary Actions: Raise ID: 65284\n' +
>       '\t\t\t\t45 창 ID: 59648\n' +
>       '\t\t\t\t\t166573 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t166574 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t166575 단추 페이지 위로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t166576 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t166577 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t166578 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t166579 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t166580 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t166581 단추 페이지 왼쪽으로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t166582 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t166583 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t166584 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t166585 위치 조정 (disabled)\n' +
>       '\t\t\t29 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65288\n' +
>       '\t\t\t\t30 창 ID: 59648\n' +
>       '\t\t\t\t\t31 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; END_VAR // The legacy recorder prefix remains 304 bytes. The coherent topology and // I/O reader uses the extended 464-byte scalar snapshot. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStat ID: 10000\n' +
>       '\t\t\t\t\t\t32 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t33 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t34 단추 페이지 위로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t\t35 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t36 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t37 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t39 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t40 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t41 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t42 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t43 위치 조정 (disabled)\n' +
>       '\t\t\t2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285\n' +
>       '\t\t\t\t3 창 ID: 59648\n' +
>       '\t\t\t\t\t4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000\n' +
>       '\t\t\t\t\t\t5 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t6 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t7 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t8 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t9 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t11 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t12 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t13 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t14 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t15 위치 조정 (disabled)\n' +
>       '\t\t\t16 창 Comm_Network Secondary Actions: Raise ID: 65283\n' +
>       '\t\t\t\t17 창 ID: 59648\n' +
>       '\t\t\t\t\t18 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t19 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t20 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t21 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t22 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t23 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t24 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t26 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t52418 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t27 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t28 위치 조정 (disabled)\n' +
>       '\t\t\t58 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65287\n' +
>       '\t\t\t\t59 창 ID: 59648\n' +
>       '\t\t\t\t\t60 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #d'... 24167 more characters,
>     focused_element: '308 TABLE Properties Window ID: 277338752'
>   }
> }
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
>   "title": "LASAL 프로젝트 저장",
>   "code": "await sky.press_key({window:targetWindow,key:\"Control_L+Shift_L+s\"}); globalThis.state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write({title:globalThis.state.window?.title,tree:globalThis.state.accessibility?.tree?.slice(-6000)});"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network',
>   tree: '뉴 항목 Online Connection\n' +
>     '\t\t\t\t197892 단추 (disabled) Set Online Connection For Current Project\n' +
>     '\t\t\t\t197893 단추 (disabled) Download (F6)\n' +
>     '\t\t\t\t197894 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)\n' +
>     '\t\t\t\t197895 단추 (disabled) Download Module on the Fly\n' +
>     '\t\t\t\t197896 단추 (disabled) Save Project on PLC\n' +
>     '\t\t\t\t197897 단추 (disabled) Start (F7)\n' +
>     '\t\t\t\t197898 단추 (disabled) Reset (F8)\n' +
>     '\t\t\t\t197899 단추 (disabled) Toggle breakpoint (F4)\n' +
>     '\t\t\t\t197900 단추 (disabled) Create condition breakpoint\n' +
>     '\t\t\t\t197901 메뉴 항목 Toolbar Options\n' +
>     '\t\t\t175 도구 모음 Build\n' +
>     '\t\t\t\t197902 메뉴 항목 Target Architecture\n' +
>     '\t\t\t\t197903 단추 Build changes (F9)\n' +
>     '\t\t\t\t197904 단추 Rebuild project (Strg+F9)\n' +
>     '\t\t\t\t197905 단추 (disabled) Cancel building (Ctrl+Break)\n' +
>     '\t\t\t\t197906 단추 Link project\n' +
>     '\t\t\t181 도구 모음 Standard\n' +
>     '\t\t\t\t197907 단추 New project (Strg+N)\n' +
>     '\t\t\t\t197908 단추 Open a file (Strg+Shift+O)\n' +
>     '\t\t\t\t197909 단추 Close active document (Strg+F4)\n' +
>     '\t\t\t\t197910 단추 (disabled) Save file (Strg+S)\n' +
>     '\t\t\t\t197911 단추 Open project (Strg+O)\n' +
>     '\t\t\t\t197912 단추 Save project changes (Strg+Shift+S)\n' +
>     '\t\t\t\t197913 단추 Close project\n' +
>     '\t\t\t\t197914 단추 Print\n' +
>     '\t\t\t\t197915 단추 Cut (Strg+X)\n' +
>     '\t\t\t\t197916 단추 Copy (Strg+C)\n' +
>     '\t\t\t\t197917 단추 Paste (Strg+V)\n' +
>     '\t\t\t\t197918 메뉴 항목 Undo (Strg+Z)\n' +
>     '\t\t\t\t197919 메뉴 항목 (disabled) Redo (Strg+Y)\n' +
>     '\t\t\t\t197920 단추 Navigate Backward (Alt+Left)\n' +
>     '\t\t\t\t197921 단추 (disabled) Navigate Forward (Alt +Right)\n' +
>     '\t\t\t197 메뉴 모음 Menu Bar\n' +
>     '\t\t\t\t197922 메뉴 항목 FILE\n' +
>     '\t\t\t\t197923 메뉴 항목 EDIT\n' +
>     '\t\t\t\t197924 메뉴 항목 NETEDIT\n' +
>     '\t\t\t\t197925 메뉴 항목 VIEW\n' +
>     '\t\t\t\t197926 메뉴 항목 PROJECT\n' +
>     '\t\t\t\t197927 메뉴 항목 BUILD\n' +
>     '\t\t\t\t197928 메뉴 항목 DEBUG\n' +
>     '\t\t\t\t197929 메뉴 항목 ANALYZE\n' +
>     '\t\t\t\t197930 메뉴 항목 TOOLS\n' +
>     '\t\t\t\t197931 메뉴 항목 EXTRAS\n' +
>     '\t\t\t\t197932 메뉴 항목 WINDOW\n' +
>     '\t\t\t\t197933 메뉴 항목 HELP\n' +
>     '\t\t69246 창 Splitter ID: 133722488\n' +
>     '\t\t233 창 Splitter ID: 617298272\n' +
>     '\t\t234 Tab Class View ID: 279804736\n' +
>     '\t\t\t235 트리 ID: 103\n' +
>     '\t\t\t\t236 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t237 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t238 단추 페이지 위로 ID: UpPageButton\n' +
>     '\t\t\t\t\t239 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t240 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t241 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t242 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis\n' +
>     '\t\t\t\t\t243 콘솔 트리 (selectable) External\n' +
>     '\t\t\t\t\t244 콘솔 트리 (selectable) Sigmatek\n' +
>     '\t\t\t\t\t245 콘솔 트리 (selectable) Elmo_1\n' +
>     '\t\t\t\t\t246 콘솔 트리 (selectable) Elmo_2\n' +
>     '\t\t\t\t\t247 콘솔 트리 (selectable) Elmo_3\n' +
>     '\t\t\t\t\t248 콘솔 트리 (selectable) Elmo_4\n' +
>     '\t\t\t\t\t249 콘솔 트리 (selectable) GL_9086_1\n' +
>     '\t\t\t\t\t250 콘솔 트리 (selectable) GL_9086_1_Slot00\n' +
>     '\t\t\t\t\t251 콘솔 트리 (selectable) GL_9086_1_Slot01\n' +
>     '\t\t\t\t\t252 콘솔 트리 (selectable) LMCControlCommandService\n' +
>     '\t\t\t\t\t\t253 콘솔 트리 (selectable) Servers\n' +
>     '\t\t\t\t\t\t254 콘솔 트리 (selectable) Clients\n' +
>     '\t\t\t\t\t\t\t40663 콘솔 트리 (selectable) LMCAxis1\n' +
>     '\t\t\t\t\t\t\t40664 콘솔 트리 (selectable) LMCAxis2\n' +
>     '\t\t\t\t\t\t\t40665 콘솔 트리 (selectable) LMCAxis3\n' +
>     '\t\t\t\t\t\t\t40666 콘솔 트리 (selectable) LMCAxis4\n' +
>     '\t\t\t\t\t\t\t40667 콘솔 트리 (selectable) LMCAxis5\n' +
>     '\t\t\t\t\t\t\t40668 콘솔 트리 (selectable) LMCAxis6\n' +
>     '\t\t\t\t\t\t\t40669 콘솔 트리 (selectable) LMCAxis7\n' +
>     '\t\t\t\t\t\t\t40670 콘솔 트리 (selectable) LMCAxis8\n' +
>     '\t\t\t\t\t\t\t40671 콘솔 트리 (selectable) LMCAxis9\n' +

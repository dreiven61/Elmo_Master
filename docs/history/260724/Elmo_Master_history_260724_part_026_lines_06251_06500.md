> 					267 콘솔 트리 (selectable) LMCEcatInputLatch
> 					268 콘솔 트리 (selectable) LMCRecorderStore
> 					269 콘솔 트리 (selectable) TCPMotionInterface
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state; globalThis.state=null; await sky.click({window:observation.window,element_index:260,click_count:2}); globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; {const t=state.accessibility?.tree||\"\"; nodeRepl.write(t.split(\"\\n\").filter(x=>/Sigmatek|EtherCAT_SDOBase|ECAT_|SDO/.test(x)).slice(0,180).join(\"\\n\"));}}",
>   "title": "SIGMATEK 클래스 목록 펼치기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
>
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const t=globalThis.state?.accessibility?.tree||\"\"; nodeRepl.write(t.split(\"\\n\").slice(-120).join(\"\\n\"));}",
>   "title": "LASAL 클래스 트리 상태 읽기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
>
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||\"\").slice(0,6000));",
>   "title": "LASAL 현재 화면 다시 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - EtherCAT_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - EtherCAT_Network Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 EtherCAT_Network Secondary Actions: Raise ID: 65288
> 				3 창 ID: 59648
> 					4 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5 단추 위쪽 스크롤 화살표 ID: UpButton
> 						6 위치 조정 위치 ID: ScrollbarThumb
> 						7 단추 페이지 아래로 ID: DownPageButton
> 						8 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			9 창 HW_Network Secondary Actions: Raise ID: 65290
> 				10 창 ID: 59648
> 			11 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65289
> 				12 창 ID: 59648
> 					13 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						14 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							15 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							16 위치 조정 위치 ID: ScrollbarThumb
> 							17 단추 페이지 오른쪽으로 ID: DownPageButton
> 							18 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			19 창 TCPMotionInterface Secondary Actions: Raise ID: 65287
> 				20 창 ID: 59648
> 					21 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 						22 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							23 단추 위쪽 스크롤 화살표 ID: UpButton
> 							24 위치 조정 위치 ID: ScrollbarThumb
> 							25 단추 페이지 아래로 ID: DownPageButton
> 							26 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						27 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							28 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							29 위치 조정 위치 ID: ScrollbarThumb
> 							30 단추 페이지 오른쪽으로 ID: DownPageButton
> 							31 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						32 위치 조정 (disabled)
> 			33 창 Comm_Network Secondary Actions: Raise ID: 65286
> 				34 창 ID: 59648
> 					35 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						36 단추 위쪽 스크롤 화살표 ID: UpButton
> 						37 위치 조정 위치 ID: ScrollbarThumb
> 						38 단추 페이지 아래로 ID: DownPageButton
> 						39 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			40 창 ECAT_DS402Base [RO] Secondary Actions: Raise ID: 65285
> 				41 창 ID: 59648
> 					42 창 FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::Init ECAT_Slave_Base::Init(); if _FirstScan then UseDefaultEnableSequence := UseDefaultEnableSequence.Read() <> 0; sigclib_atomic_setU32(pValue:=#sDriveIFSDO.udIsSDOActive, value:=ECAT_SB_SDO_FREE); end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRt // Update AxE
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const t=state.accessibility?.tree||\"\"; nodeRepl.write(t.split(\"\\n\").filter(x=>/콘솔 트리|EtherCAT|Sigmatek|External/.test(x)).slice(-220).join(\"\\n\"));}",
>   "title": "LASAL 클래스 트리 항목 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - EtherCAT_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - EtherCAT_Network Secondary Actions: Raise
> 			2 창 EtherCAT_Network Secondary Actions: Raise ID: 65288
> 					99 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 				253 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					254 콘솔 트리 (selectable) External
> 					255 콘솔 트리 (selectable) Sigmatek
> 					256 콘솔 트리 (selectable) _TCPIPServer_RT
> 					257 콘솔 트리 (selectable) Elmo_1
> 					258 콘솔 트리 (selectable) Elmo_2
> 					259 콘솔 트리 (selectable) Elmo_3
> 					260 콘솔 트리 (selectable) Elmo_4
> 					261 콘솔 트리 (selectable) LMCDiagnosticsService
> 					262 콘솔 트리 (selectable) LMCEcatInputLatch
> 					263 콘솔 트리 (selectable) LMCRecorderStore
> 					264 콘솔 트리 (selectable) TCPMotionInterface
> 					279 custom Sigmatek
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:false}); globalThis.targetWindow=state.window; nodeRepl.write(\"EtherCAT SDO object observed\");",
>   "title": "EtherCAT SDO 객체 메뉴 준비",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> EtherCAT SDO object observed
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state; const sid=observation.screenshots?.[0]?.id; globalThis.state=null; await sky.click({window:observation.window,screenshotId:sid,x:1708,y:545}); globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/EtherCAT|SDO|콘솔 트리|File/.test(x)).slice(-180).join(\"\\n\"));}",
>   "title": "LASAL 파일 트리 열기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||\"\").slice(-2500));",
>   "title": "LASAL 파일 트리 상태 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> eButton
> 							246 위치 조정 위치 ID: ScrollbarThumb
> 							247 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						248 목록 항목 (selectable)
> 						249 목록 항목 (selectable)
> 						250 목록 항목 (selectable)
> 						251 목록 항목 (selectable)
> 						252 목록 항목 (selectable)
> 						253 목록 항목 (selectable)
> 						254 목록 항목 (selectable)
> 						255 목록 항목 (selectable)
> 					256 스크롤 막대 ID: 59904
> 						257 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						258 위치 조정 위치 ID: ScrollbarThumb
> 						259 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			260 탭 항목 (selectable) Python Script
> 			261 탭 항목 (selectable) Debugger
> 			262 탭 항목 (selectable) Output
> 			263 단추 Close
> 		264 창 Splitter ID: 254458296
> 		265 Tab File Explorer ID: 251698672
> 			266 트리 ID: 102
> 				267 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					268 콘솔 트리 (selectable) Additional Files
> 					269 콘솔 트리 (selectable) Header Files
> 					270 콘솔 트리 (selectable) Source Files
> 					271 콘솔 트리 (selectable) Network Files
> 					272 콘솔 트리 (selectable) Object Files
> 					273 콘솔 트리 (selectable) Library Files
> 					274 콘솔 트리 (selectable) Dias Drive Files
> 					275 콘솔 트리 (selectable) Connection Files
> 			276 탭 항목 (selectable) Lib
> 			277 탭 항목 (selectable) File
> 			278 탭 항목 (selectable) Global
> 			279 탭 항목 (selectable) Net
> 			280 탭 항목 (selectable) Class
> 			281 단추 Close
> 		282 Tab Properties ID: 251700040
> 			283 창 ID: 244074672
> 				284 TABLE Properties Window ID: 249456424
> 					285 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						286 단추 위쪽 스크롤 화살표 ID: UpButton
> 						287 위치 조정 위치 ID: ScrollbarThumb
> 						288 단추 페이지 아래로 ID: DownPageButton
> 						289 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					290 custom Project Version
> 					291 custom Name
> 					292 custom Revision
> 					293 custom Online Connection
> 					294 custom CodeGenerator
> 					295 custom Networks
> 					296 custom Server list
> 					297 custom Target
> 					298 custom Compiler
> 					299 custom Directory
> 					300 custom OPC-UA
> 					301 custom Load all libraries
> 					302 custom Use Unit System
> 					303 custom Include Paths
> 					304 custom Library Paths
> 					305 custom Backup Includes and Loader
> 					306 custom Ignore at Cleanup
> 					307 custom Reencrypt Project On Close
> 					308 custom Enable OPC UA
> 					309 custom Enable initvalues for output parameters
> 					310 custom Use multiple CPU core
> 					311 custom Use Advanced-IO

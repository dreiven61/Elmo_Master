>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({title:state.window.title,focused:state.accessibility?.focused_element,screens:state.screenshots.map(s=>({id:s.id,z:s.zIndex,width:s.width,height:s.height})),treeTail:state.accessibility?.tree?.slice(-2600)}); for (const s of state.screenshots) if(s.zIndex<=1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface*',
>   focused: '130092 창 #define LMC_OWNER_STARTUP_PROOF_BOOT_ID 0x00000001 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; ownershipBootId : UDINT; ownershipReportCycle : UDINT; ownershipStartupResult : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. ownershipBootId := 0; ownershipStartupResult := -1; if IsClientConnected(#Diagnostics) IsClientConnected(#ControlCommands) then ownershipBootId := Diagnostics.GetDiagnosticsBootId(); if ownershipBootId <> 0 then ownershipReportCycle := ops.tAbsolute; if ownershipReportCycle = 0 then ownershipReportCycle := 1; end_if; // Only BootId is observed here. Physical, Group and executor idle proof // bits are absent, so startup remains quarantined and all reserve fails. ownershipStartupResult := ControlCommands.ReconcileAxisOwnershipStartup( DiagnosticsBootId:=ownershipBootId, ReportCycle:=ownershipReportCycle, ProofFlags:=LMC_OWNER_STARTUP_PROOF_BOOT_ID, QuarantineReason:=-31); end_if; end_if; if (PendingClosedSessionEpoch <> 0) & IsClientConnected(#Diagnostics) & IsClientConnected(#ControlCommands) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); ControlCommands.NotifyAxisOwnershipSessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // Retained owner and Home watchdogs continue across TCP session loss. if IsClientConnected(#ControlCommands) then ControlCommands.ProcessAxisZeroHome(); ControlCommands.ProcessAxisOwnership(); end_if; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FA ID: 10000',
>   screens: [
>     { id: 'screenshot-0', z: 0, width: 970, height: 512 },
>     { id: 'screenshot-1', z: 1, width: 960, height: 502 }
>   ],
>   treeTail: 'Cycle:=ownershipReportCycle, ProofFlags:=LMC_OWNER_STARTUP_PROOF_BOOT_ID, QuarantineReason:=-31); end_if; end_if; if (PendingClosedSessionEpoch <> 0) & IsClientConnected(#Diagnostics) & IsClientConnected(#ControlCommands) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); ControlCommands.NotifyAxisOwnershipSessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // Retained owner and Home watchdogs continue across TCP session loss. if IsClientConnected(#ControlCommands) then ControlCommands.ProcessAxisZeroHome(); ControlCommands.ProcessAxisOwnership(); end_if; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FA ID: 10000.'
> }
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
>   "title": "Reconcile 입력 목록 펼치기",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:750,y:136,mouse_button:\"left\"}); nodeRepl.write(\"expanded Reconcile method\");"
> }
> ```
>
> ```text
> expanded Reconcile method
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reconcile 기존 ABI 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({treeTail:state.accessibility?.tree?.slice(-3000)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   treeTail: '\n' +
>     '\t\t\t\t\t\t\t\t\t131336 콘솔 트리 (selectable) DiagnosticsBootId\n' +
>     '\t\t\t\t\t\t\t\t\t131337 콘솔 트리 (selectable) ReportCycle\n' +
>     '\t\t\t\t\t\t\t\t\t131338 콘솔 트리 (selectable) ProofFlags\n' +
>     '\t\t\t\t\t\t\t\t\t131339 콘솔 트리 (selectable) QuarantineReason\n' +
>     '\t\t\t\t\t\t\t\t\t131340 콘솔 트리 (selectable) Result\n' +
>     '\t\t\t\t\t\t\t\t130213 콘솔 트리 (selectable) NotifyAxisOwnershipSessionClosed\n' +
>     '\t\t\t\t\t\t\t\t130214 콘솔 트리 (selectable) ProcessAxisOwnership\n' +
>     '\t\t\t\t\t\t\t121381 콘솔 트리 (selectable) Private\n' +
>     '\t\t\t\t\t\t104392 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t\t106716 콘솔 트리 (selectable) GroupMovePos\n' +
>     '\t\t\t\t\t\t\t106717 콘솔 트리 (selectable) GroupKinematicReady\n' +
>     '\t\t\t\t\t\t\t106718 콘솔 트리 (selectable) ZeroHomeState\n' +
>     '\t\t\t\t\t\t\t106719 콘솔 트리 (selectable) OwnershipState\n' +
>     '\t\t\t\t\t\t\t107265 콘솔 트리 (selectable) OwnershipStartupState\n' +
>     '\t\t\t\t\t\t\t\t113185 콘솔 트리 (selectable) 0..15\n' +
>     '\t\t\t\t\t\t104393 콘솔 트리 (selectable) Objects\n' +
>     '\t\t\t\t\t\t104394 콘솔 트리 (selectable) Dependencies\n' +
>     '\t\t\t\t\t3141 콘솔 트리 (selectable) LMCDiagnosticsService\n' +
>     '\t\t\t\t\t3142 콘솔 트리 (selectable) LMCEcatInputLatch\n' +
>     '\t\t\t\t\t\t66030 콘솔 트리 (selectable) Servers\n' +
>     '\t\t\t\t\t\t66031 콘솔 트리 (selectable) Clients\n' +
>     '\t\t\t\t\t\t66032 콘솔 트리 (selectable) Methods\n' +
>     '\t\t\t\t\t\t\t67056 콘솔 트리 (selectable) Global\n' +
>     '\t\t\t\t\t\t\t\t70659 콘솔 트리 (selectable) RtWork\n' +
>     '\t\t\t\t\t\t\t\t70660 콘솔 트리 (selectable) CopySnapshot\n' +
>     '\t\t\t\t\t\t\t\t\t75839 콘솔 트리 (selectable) pDest\n' +
>     '\t\t\t\t\t\t\t\t\t75840 콘솔 트리 (selectable) DestSize\n' +
>     '\t\t\t\t\t\t\t\t\t75841 콘솔 트리 (selectable) Result\n' +
>     '\t\t\t\t\t\t\t\t70661 콘솔 트리 (selectable) CopyTopologyIoSnapshot\n' +
>     '\t\t\t\t\t\t\t\t70662 콘솔 트리 (selectable) AdvanceOutputRevision\n' +
>     '\t\t\t\t\t\t\t\t70663 콘솔 트리 (selectable) SubmitDs402HomeControl\n' +
>     '\t\t\t\t\t\t\t\t70664 콘솔 트리 (selectable) GetDs402HomeControlState\n' +
>     '\t\t\t\t\t\t\t\t70665 콘솔 트리 (selectable) SubmitDs402HomeSetpointAlignment\n' +
>     '\t\t\t\t\t\t\t\t70666 콘솔 트리 (selectable) SubmitAxisZeroHome\n' +
>     '\t\t\t\t\t\t\t\t70667 콘솔 트리 (selectable) CopyAxisZeroHomeResult\n' +
>     '\t\t\t\t\t\t\t\t70668 콘솔 트리 (selectable) CopyAxisOwnershipStartupSnapshot\n' +
>     '\t\t\t\t\t\t\t\t\t73240 콘솔 트리 (selectable) pDest\n' +
>     '\t\t\t\t\t\t\t\t\t93430 콘솔 트리 (selectable) DestSize\n' +
>     '\t\t\t\t\t\t\t\t\t96123 콘솔 트리 (selectable) Result\n' +
>     '\t\t\t\t\t\t\t67057 콘솔 트리 (selectable) Private\n' +
>     '\t\t\t\t\t\t66033 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t66034 콘솔 트리 (selectable) Objects\n' +
>     '\t\t\t\t\t\t66035 콘솔 트리 (selectable) Dependencies\n' +
>     '\t\t\t\t\t3143 콘솔 트리 (selectable) LMCRecorderStore\n' +
>     '\t\t\t\t\t3144 콘솔 트리 (selectable) LMCSdoExecutor\n' +
>     '\t\t\t\t\t3145 콘솔 트리 (selectable) TCPIPServer\n' +
>     '\t\t\t\t\t3146 콘솔 트리 (selectable) TCPMotionInterface\n' +
>     '\t\t\t131695 탭 항목 (selectable) Lib\n' +
>     '\t\t\t131696 탭 항목 (selectable) File\n' +
>     '\t\t\t131697 탭 항목 (selectable) Class\n' +
>     '\t\t\t131698 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t126526 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t126527 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t126528 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t126529 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t126530 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t131699 custom Name\n' +
>     '\t\t\t\t\t131700 custom Virtual\n' +
>     '\t\t\t\t\t131701 custom Global access\n' +
>     '\t\t\t\t\t131702 custom AWL implementation\n' +
>     '\t\t\t\t\t131703 custom CDecl\n' +
>     '\t\t\t\t\t131704 custom Comment\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t131705 탭 항목 (selectable) Properties\n' +
>     '\t\t\t96 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 83 트리 ID: 103.'
> }
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
>   "title": "ReportCycle 입력 선택",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:834,y:178,mouse_button:\"left\"}); nodeRepl.write(\"selected ReportCycle\");"
> }
> ```
>
> ```text
> selected ReportCycle
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ReportCycle 선택 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element});"
> }
> ```
>
> ```text
> { focused: '83 트리 ID: 103' }
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
>   "title": "ObservationCycle 입력 이름 편집",
>   "code": "await sky.press_key({window:targetWindow,key:\"F2\"}); nodeRepl.write(\"opened ReportCycle name editor\");"
> }
> ```
>
> ```text
> opened ReportCycle name editor
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ObservationCycle 이름 입력",
>   "code": "await sky.type_text({window:targetWindow,text:\"ObservationCycle\"}); nodeRepl.write(\"typed ObservationCycle\");"
> }
> ```
>
> ```text
> typed ObservationCycle
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ObservationCycle 이름 확정",
>   "code": "await sky.press_key({window:targetWindow,key:\"Return\"}); nodeRepl.write(\"committed ObservationCycle\");"
> }
> ```
>
> ```text
> committed ObservationCycle
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ObservationCycle 참조 갱신 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,screens:state.screenshots.map(s=>({id:s.id,z:s.zIndex,width:s.width,height:s.height}))}); for (const s of state.screenshots) if(s.zIndex>=2) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   focused: '134775 대화 상자 Automatic Source Code Update Secondary Actions: Raise',
>   screens: [
>     { id: 'screenshot-0', z: 0, width: 970, height: 512 },
>     { id: 'screenshot-1', z: 1, width: 960, height: 502 },
>     { id: 'screenshot-2', z: 2, width: 412, height: 116 }
>   ]
> }
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

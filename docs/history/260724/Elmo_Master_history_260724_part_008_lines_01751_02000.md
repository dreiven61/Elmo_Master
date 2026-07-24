>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\ntopShot = lasalState.screenshots.reduce((a,b)=>a.zIndex>b.zIndex?a:b);\nawait sky.click({window:lasalWindow, x:1417, y:646, screenshotId:topShot.id});\nnodeRepl.write(\"opened RealTime choices\")\n",
>   "title": "래치 주기 선택 목록 확인"
> }
> ```
>
> ```text
> opened RealTime choices
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
>   "code": "\nlasalState = await sky.get_window_state({window:lasalWindow, include_screenshot:true, include_text:true});\nnodeRepl.write((lasalState.accessibility?.tree || \"\").split(\"\\n\").slice(-80).join(\"\\n\"));\nfor (var shot8 of lasalState.screenshots) await nodeRepl.emitImage(shot8.url);\n",
>   "title": "래치 주기 선택값 확인"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 1,783; original character count 1,048,602; UTF-8 byte count 1,048,606; UTF-8 SHA-256 769404de4e209757f17866622b987ff6a8c0ac70bc47e53f506f03eb2d95c161; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
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
>   "code": "\ntopShot = lasalState.screenshots.reduce((a,b)=>a.zIndex>b.zIndex?a:b);\nawait sky.click({window:lasalWindow, x:1288, y:664, screenshotId:topShot.id});\nnodeRepl.write(\"selected RealTime off\")\n",
>   "title": "래치 독립 주기 해제"
> }
> ```
>
> ```text
> point (1288, 664) is outside window bounds { originX: 0, originY: 0, width: 228, height: 64 }
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
>   "code": "nodeRepl.write(lasalState.screenshots.map(s=>({id:s.id,width:s.width,height:s.height,z:s.zIndex,originX:s.originX,originY:s.originY})))",
>   "title": "LASAL 선택 목록 화면 확인"
> }
> ```
>
> ```text
> [
>   {
>     id: 'screenshot-0',
>     width: 1450,
>     height: 763,
>     z: 0,
>     originX: 56,
>     originY: 106
>   },
>   {
>     id: 'screenshot-1',
>     width: 1440,
>     height: 753,
>     z: 1,
>     originX: 61,
>     originY: 111
>   },
>   {
>     id: 'screenshot-2',
>     width: 228,
>     height: 64,
>     z: 2,
>     originX: 1237,
>     originY: 761
>   }
> ]
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
>   "code": "\nawait sky.click({window:lasalWindow, x:1288, y:664, screenshotId:\"screenshot-1\"});\nnodeRepl.write(\"selected RealTime off\")\n",
>   "title": "래치 독립 주기 해제"
> }
> ```
>
> ```text
> selected RealTime off
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
>   "code": "\nlasalState = await sky.get_window_state({window:lasalWindow, include_screenshot:true, include_text:true});\nnodeRepl.write((lasalState.accessibility?.tree || \"\").split(\"\\n\").filter(line => /Save file|Save project changes|RealTime|focused/.test(line)).join(\"\\n\"));\nfor (var shot9 of lasalState.screenshots) await nodeRepl.emitImage(shot9.url);\n",
>   "title": "래치 주기 해제 확인"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 1,876; original character count 1,048,594; UTF-8 byte count 1,048,606; UTF-8 SHA-256 3147628ef2903c83706444908102d042f7f0530d39517d8136808754e25fb1fb; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
> ```
>
> </details>
>
> Recorder 쪽은 수정이 들어갔습니다. Stop 전에 PLC 상태를 다시 읽어 이미 완료된 레코더에는 Stop을 보내지 않도록 했고, Download가 PC 메모리 복사이며 CSV Export가 실제 파일 저장이라는 안내도 추가했습니다. 지금은 LASAL IDE에서 래치 객체의 독립 `1 ms` 태스크를 `off`로 바꿔 축 Pre-RT 트리거만 남기는 중입니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nawait sky.press_key({window:lasalWindow, key:\"Control_L+Shift_L+s\"});\nnodeRepl.write(\"saved LASAL project changes\")\n",
>   "title": "LASAL 네트워크 변경 저장"
> }
> ```
>
> ```text
> saved LASAL project changes
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
>   "code": "\nlasalState = await sky.get_window_state({window:lasalWindow, include_screenshot:true, include_text:true});\nnodeRepl.write((lasalState.accessibility?.tree || \"\").split(\"\\n\").filter(line => /Save file|Save project changes|dialog|confirmation|yes|no|cancel/i.test(line)).join(\"\\n\"));\nfor (var shot10 of lasalState.screenshots) await nodeRepl.emitImage(shot10.url);\n",
>   "title": "LASAL 저장 결과 확인"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 1,912; original character count 1,048,248; UTF-8 byte count 1,048,606; UTF-8 SHA-256 f7dc81f73feca4b020139fe1db8a3b806bff1bf46399b909b56bdc498b97fdd0; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
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
>   "code": "\nawait sky.press_key({window:lasalWindow, key:\"Control_L+F9\"});\nnodeRepl.write(\"started LASAL rebuild\")\n",
>   "timeout_ms": 30000,
>   "title": "LASAL 프로젝트 재빌드"
> }
> ```
>
> ```text
> started LASAL rebuild
> ```
>
> <details><summary>Called 2 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nawait new Promise(resolve => setTimeout(resolve, 2500));\nlasalState = await sky.get_window_state({window:lasalWindow, include_screenshot:true, include_text:true});\nnodeRepl.write((lasalState.accessibility?.tree || \"\").split(\"\\n\").filter(line => /Cancel building|Compiler:|Linker:|error|warning|done/i.test(line)).slice(-50).join(\"\\n\"));\nfor (var shot11 of lasalState.screenshots) await nodeRepl.emitImage(shot11.url);\n",
>   "timeout_ms": 30000,
>   "title": "LASAL 재빌드 결과 확인"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 1,948; original character count 1,048,588; UTF-8 byte count 1,048,606; UTF-8 SHA-256 a18c46fa1d0369dc7940cea1c2e4b78311963b03b7599f6a3b6fd741a20d09dd; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nawait new Promise(resolve => setTimeout(resolve, 12000));\nlasalState = await sky.get_window_state({window:lasalWindow, include_screenshot:true, include_text:true});\nnodeRepl.write((lasalState.accessibility?.tree || \"\").split(\"\\n\").filter(line => /Cancel building|Errors?:|Warnings?:|Rebuild|done|failed|success|Compiler:|Linker:/i.test(line)).slice(-80).join(\"\\n\"));\nfor (var shot12 of lasalState.screenshots) await nodeRepl.emitImage(shot12.url);\n",
>   "timeout_ms": 30000,
>   "title": "LASAL 재빌드 완료 확인"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 1,964; original character count 1,048,592; UTF-8 byte count 1,048,606; UTF-8 SHA-256 3c4bcf48fc990e0f57979913ccdeacc7bcb0412a675240450ca9e6cff192c9af; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
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
>   "code": "\nlasalState = await sky.get_window_state({window:lasalWindow, include_screenshot:false, include_text:true});\nnodeRepl.write((lasalState.accessibility?.tree || \"\").split(\"\\n\").filter(line => /Cancel building|Rebuild project|Link project|Compiler:|Linker:|Done|failed|error/i.test(line)).slice(-40).join(\"\\n\"))\n",
>   "title": "LASAL 빌드 상태 확인"
> }
> ```
>
> ```text
> 					33 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 					47 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_RECORDER_SCHEMA_VERSION 1 #define LMC_RECORDER_MAP_REVISION 0x957F101E #define LMC_RECORDER_ERROR_ID -32000 #define LMC_RECORDER_STORAGE_BYTES 1280000 #define LMC_RECORDER_MAX_CHANNELS 24 #define LMC_RECORDER_MAX_CHUNK_BYTES 1280 #define LMC_RECORDER_EMPTY 0 #define LMC_RECORDER_CONFIGURED 1 #define LMC_RECORDER_ARMED 2 #define LMC_RECORDER_RECORDING 3 #define LMC_RECORDER_READY 4 #define LMC_RECORDER_UPLOADING 5 #define LMC_RECORDER_FAULT 6 #define LMC_RECORDER_STOP_NONE 0 #define LMC_RECORDER_STOP_COUNT_COMPLETE 1 #define LMC_RECORDER_STOP_USER 2 #define LMC_RECORDER_STOP_TRIGGER_COMPLETE 3 // The data bank is global so the generated class object stays below the // 16-bit object-size field used by the LASAL class table. Exactly one // LMCRecorderStore object is allowed in the project. VAR_GLOBAL g_LMCRecorderData : ARRAY [0..1279999] OF USINT; END_VAR FUNCTION GLOBAL LMCRecorderStore::NotifySessionClosed VAR_INPUT SessionEpoch : UDINT; END_VAR if (SessionEpoch <> 0) (SessionEpoch = OwnerSessionEpoch) then ClosedSessionEpoch := SessionEpoch; end_if; END_FUNCTION FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot VAR_INPUT pSnapshot : ^USINT; SnapshotSize : UDINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR state : UDINT; startRequest : UDINT; triggerRequest : UDINT; stopRequest : UDINT; sequence : UDINT; cycleCounter : UDINT; channelIndex : UINT; dataOffset : UDINT; snapshotOffset : UDINT; triggerRaw : UDINT; triggerHealthOffset : UDINT; triggerEvent : BOOL; triggerInputValid : BOOL; previousCondition : BOOL; currentCondition : BOOL; prehistoryReady : BOOL; previousSigned : DINT; currentSigned : DINT; thresholdSigned : DINT; lowerSigned : DINT; upperSigned : DINT; timestampStep : UDINT; timestampLowBefore : UDINT; END_VAR Result := -1; if (pSnapshot = NIL) | (SnapshotSize < 304) then RETURN; end_if; state := sigclib_atomic_getU32(pValue:=#StateValue); startRequest := sigclib_atomic_getU32(pValue:=#StartRequestSequence); triggerRequest := sigclib_atomic_getU32(pValue:=#TriggerRequestSequence); stopRequest := sigclib_atomic_getU32(pValue:=#StopRequestSequence); sequence := sigclib_atomic_getU32(pValue:=#StatusSequence) + 1; if (sequence and 1) = 0 then sequence += 1; end_if; sigclib_atomic_setU32(pValue:=#StatusSequence, value:=sequence); cycleCounter := pSnapshot^$UDINT; if (state = LMC_RECORDER_ARMED) & (startRequest <> StartAppliedSequence) then StartAppliedSequence := startRequest; SampleCount := 0; DividerCounter := 0; StopReason := LMC_RECORDER_STOP_NONE; TriggerIndex := 0xFFFFFFFF; TriggerCycle := 0; TriggerTimestampLow := 0; TriggerTimestampHigh := 0; StartCycle := 0; EndCycle := 0; StartTimestampLow := 0; StartTimestampHigh := 0; EndTimestampLow := 0; EndTimestampHigh := 0; DroppedCycles := 0; OverflowCount := 0; WriteSampleIndex := 0; FrozenFirstSampleIndex := 0; PostSamplesRemaining := 0; PreviousTriggerValue := 0; PreviousTriggerValid := FALSE; state := LMC_RECORDER_RECORDING; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_RECORDING); end_if; if ((state = LMC_RECORDER_ARMED) | (state = LMC_RECORDER_RECORDING)) & (stopRequest <> StopAppliedSequence) then StopAppliedSequence := stopRequest; if (TriggerType <> 0) & (TriggerIndex = 0xFFFFFFFF) & (SampleCapacity > 0) then FrozenFirstSampleIndex := (WriteSampleIndex + SampleCapacity - SampleCount) MOD SampleCapacity; end_if; StopReason := LMC_RECORDER_STOP_USER; if SampleCount = 0 then EndCycle := cycleCounter; EndTimestampLow := (pSnapshot + 4)^$UDINT; EndTimestampHigh := (pSnapshot + 8)^$UDINT; end_if; state := LMC_RECORDER_READY; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_READY); end_if; if state = LMC_RECORDER_RECORDING then if DividerCounter = 0 then if TriggerType = 0 then if SampleCount < SampleCapacity then if SampleCount = 0 then Start ID: 10000
> 					74 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 					88 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="status_word"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000
> 					116 창 FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::Init ECAT_Slave_Base::Init(); if _FirstScan then UseDefaultEnableSequence := UseDefaultEnableSequence.Read() <> 0; sigclib_atomic_setU32(pValue:=#sDriveIFSDO.udIsSDOActive, value:=ECAT_SB_SDO_FREE); end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRt // Update AxError if StateWord.Fault = FALSE then AxError := 0; AxErrorQuit := 0; ControlWord.ResetFault := FALSE; else // Set AxError if Fault is set in Stateword and Error is not beeing quit. if AxErrorQuit = 0 then AxError := 1; elsif (ops.tabsolute - TimeStampResetFault) >= GetQuitErrorTimeout() then AxError := 1; AxErrorQuit := 0; ControlWord.ResetFault := FALSE; end_if; end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRtPostScan if AxError = 0 then if UseDefaultEnableSequence then EnableSequence(); end_if; end_if; END_FUNCTION FUNCTION VIRTUAL ECAT_DS402Base::EnableSequence //************************************************************************************************** // ENABLE/DISABLE SEQUENCE //************************************************************************************************** case EnableSequenceSSW of //************************************************************************************************** e_EnableSequenceSSW::_Disabled: // do nothing special, we're enabled now e_EnableSequenceSSW::_Enabled: // do nothing special, we're disabled now //************************************************************************************************** e_EnableSequenceSSW::_StartEnable: ControlWord.SwitchOn := FALSE; ControlWord.EnableVoltage := TRUE; ControlWord.QuickStop := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _SwitchOn; //************************************************************************************************** e_EnableSequenceSSW::_SwitchOn: if StateWord.VoltageEnabled StateWord.QuickStop & StateWord.ReadyToSwitchOn then ControlWord.SwitchOn := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _EnableOperation; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_EnableOperation: if StateWord.VoltageEnabled & StateWord.QuickStop & StateWord.ReadyToSwitchOn & StateWord.SwitchedOn then ControlWord.EnableOperation := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _CheckOperationEnabled; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_CheckOperationEnabled: if StateWord.VoltageEnabled & StateWord.QuickStop & StateWord.ReadyToSwitchOn & StateWord.SwitchedOn & StateWord.OperationEnabled then EnableSequenceSSW := _Enabled; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_DisableOperation: if StateWord.OperationEnabled = 0 then AxEnable := 0; EnableSequenceSSW := _Disabled; end_if; //**************************************************************************** ID: 10000
> 					131 창 FUNCTION VIRTUAL GLOBAL Elmo_2::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_2::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_2_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_2_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_2::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_2_SETPOS_INDEX, ELMO_2_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_2_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_2_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_2_CONTROLWORD_INDEX, ELMO_2_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_2_ACTPOS_INDEX, ELMO_2_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_2_INPUTS_DIGITALINPUTS_INDEX, ELMO_2_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_2_STATEWORD_INDEX, ELMO_2_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 				194 단추 Rebuild project (Strg+F9)
> 				195 단추 (disabled) Cancel building (Ctrl+Break)
> 				196 단추 Link project
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nawait sky.click({window:lasalWindow, element_index:196});\nnodeRepl.write(\"started LASAL link\")\n",
>   "title": "LASAL 프로젝트 링크"
> }

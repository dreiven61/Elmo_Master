>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect Coupler client properties",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*", App: Lasal2.exe.\n' +
>     '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch* Secondary Actions: Raise\n' +
>     '\t\t1 창 작업 영역 ID: 59648\n' +
>     '\t\t\t2753 창 LMCEcatInputLatch* Secondary Actions: Raise ID: 65283\n' +
>     '\t\t\t\t2754 창 ID: 59648\n' +
>     '\t\t\t\t\t2755 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000\n' +
>     '\t\t\t\t\t\t2756 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t\t2757 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t2758 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t2759 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t2760 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t2761 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t\t2762 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t2763 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t2764 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t2765 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t2766 위치 조정 (disabled)\n' +
>     '\t\t\t2 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65282\n' +
>     '\t\t\t\t3 창 ID: 59648\n' +
>     '\t\t\t\t\t4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex >= 0x8000 then physicalAxis := (CatalogIndex shr 8) and 0x007F; topologyCount := CatalogIndex and 0x00FF; if (topologyCount = 0) | (physicalAxis >= 7) | ((physicalAxis + topologyCount) > 7) then RETURN; end_if; (pEntry + 16)^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 20)^$UINT := phy ID: 10000\n' +
>     '\t\t\t\t\t\t5 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t\t6 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t7 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t8 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t9 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t\t11 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t12 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t13 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t14 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t15 위치 조정 (disabled)\n' +
>     '\t\t\t16 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281\n' +
>     '\t\t\t\t17 창 ID: 59648\n' +
>     '\t\t\t\t\t18 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : '... 12604 more characters,
>   focused_element: '143 트리 ID: 103'
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
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Locate Coupler property controls",
>   "code": "var propLinesA = globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/DataType|Required|Coupler|Data Channel|true/.test(x)); nodeRepl.write(propLinesA.slice(-40).join(\"\\n\"));"
> }
> ```
>
> ```text
> 					32 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 							18322 콘솔 트리 (selectable) Coupler
> 					18704 custom DataType
> 					18706 custom Required
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
>   "title": "Edit Coupler client type",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,element_index:18704});"
> }
> ```
>
> ```text
> element 18704 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Focus Coupler datatype cell",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1365,y:492,screenshotId:globalThis.state.screenshots[0].id,click_count:2});"
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
>   "title": "Inspect Coupler datatype editor",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {"content":[{"type":"text","text":"{\n  tree: 'Window: \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*\", App: Lasal2.exe.\\n' +\n    '\\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch* Secondary Actions: Raise\\n' +\n    '\\t\\t20660 편집\\n' +\n    '\\t\\t20661 LIST\\n' +\n    '\\t\\t\\t20662 스크롤 막대 세로 ID: NonClientVerticalScrollBar\\n' +\n    '\\t\\t\\t\\t20663 단추 위쪽 스크롤 화살표 ID: UpButton\\n' +\n    '\\t\\t\\t\\t20664 단추 페이지 위로 ID: UpPageButton\\n' +\n    '\\t\\t\\t\\t20665 위치 조정 위치 ID: ScrollbarThumb\\n' +\n    '\\t\\t\\t\\t20666 단추 페이지 아래로 ID: DownPageButton\\n' +\n    '\\t\\t\\t\\t20667 단추 아래쪽 스크롤 화살표 ID: DownButton\\n' +\n    '\\t\\t\\t20668 목록 항목 (selectable) DINT\\n' +\n    '\\t\\t\\t20669 목록 항목 (selectable) DWORD\\n' +\n    '\\t\\t\\t20670 목록 항목 (selectable) HDINT\\n' +\n    '\\t\\t\\t20671 목록 항목 (selectable) REAL\\n' +\n    '\\t\\t\\t20672 목록 항목 (selectable) UDINT\\n' +\n    '\\t\\t\\t20673 목록 항목 (selectable) _AxisBase::_LMCAXIS_POSINFOMODE\\n' +\n    '\\t\\t\\t20674 목록 항목 (selectable) _AxisBase::_LMCAXIS_READPARAMETER_LREAL\\n' +\n    '\\t\\t\\t20675 목록 항목 (selectable) _AxisBase::t_e_LMCAXIS_GETSAFETYPARAMETER\\n' +\n    '\\t\\t\\t20676 목록 항목 (selectable) _AxisBase::t_e_LMCAXIS_SETSAFETYPARAMETER\\n' +\n    '\\t\\t\\t20677 목록 항목 (selectable) _LMCAxisBase::_CommandList\\n' +\n    '\\t\\t\\t20678 목록 항목 (selectable) _LMCAxisBase::_CoupleMonitorExtendedConfig\\n' +\n    '\\t\\t\\t20679 목록 항목 (selectable) _LMCAxisBase::t_e_THCmdClassSvr\\n' +\n    '\\t\\t\\t20680 목록 항목 (selectable) _LMCAxisBase::t_s_LMCAxis_SafetyState\\n' +\n    '\\t\\t\\t20681 목록 항목 (selectable) _LMCAxisBase::te_BLCmdClassSvr\\n' +\n    '\\t\\t\\t20682 목록 항목 (selectable) _LMCMath_SO3::_ROT_MODUS\\n' +\n    '\\t\\t\\t20683 목록 항목 (selectable) _LMCMathFunctions::t_e_LMC_MoveProf_Jerk_ProfTypes\\n' +\n    '\\t\\t\\t20684 목록 항목 (selectable) _LMCMathFunctions::t_e_LMCMath_Retcodes\\n' +\n    '\\t\\t\\t20685 목록 항목 (selectable) _LMCProfileBase::t_s_PartialDynParaLimitModes\\n' +\n    '\\t\\t\\t20686 목록 항목 (selectable) _LMCProfileBuffer::_BUFFER_PARAMETER_\\n' +\n    '\\t\\t\\t20687 목록 항목 (selectable) _LMCProfileLog::_LMCLogCmdList\\n' +\n    '\\t\\t\\t20688 목록 항목 (selectable) _LMCProfileLog::_LogParCircleDefinition\\n' +\n    '\\t\\t\\t20689 목록 항목 (selectable) _LMCProfileLog::_LogParSetRestartPointFileOffset\\n' +\n    '\\t\\t\\t20690 목록 항목 (selectable) _LMCProfileLog::_LogParSetTimeBase\\n' +\n    '\\t\\t\\t20691 목록 항목 (selectable) _LMCPublisher::_Config_\\n' +\n    '\\t\\t\\t20692 목록 항목 (selectable) _LMCPublisher::_LMCPublisher_Cmd\\n' +\n    '\\t\\t\\t20693 목록 항목 (selectable) _LMCRefBase::_LMCREF_READPARAMETER\\n' +\n    '\\t\\t\\t20694 목록 항목 (selectable) _LMCRobotBase::_PARAMETER\\n' +\n    '\\t\\t\\t20695 목록 항목 (selectable) _LMCRobotBase::_POSITION_MODE\\n' +\n    '\\t\\t\\t20696 목록 항목 (selectable) _LMCRobotBase::_ROBOT_MODE\\n' +\n    '\\t\\t\\t20697 목록 항목 (selectable) _LMCRobotBase::_ROBOT_ON_MODE\\n' +\n    '\\t\\t1 창 작업 영역 ID: 59648\\n' +\n    '\\t\\t\\t2753 창 LMCEcatInputLatch* Secondary Actions: Raise ID: 65283\\n' +\n    '\\t\\t\\t\\t2754 창 ID: 59648\\n' +\n    '\\t\\t\\t\\t\\t2755 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000\\n' +\n    '\\t\\t\\t\\t\\t\\t2756 스크롤 막대 세로 ID: NonClientVerticalScrollBar\\n' +\n    '\\t\\t\\t\\t\\t\\t\\t2757 단추 위쪽 스크롤 화살표 ID: UpButton\\n' +\n    '\\t\\t\\t\\t\\t\\t\\t2758 위치 조정 위치 ID: ScrollbarThumb\\n' +\n    '\\t\\t\\t\\t\\t\\t\\t2759 단추 페이지 아래로 ID: DownPageButton\\n' +\n    '\\t\\t\\t\\t\\t\\t\\t2760 단추 아래쪽 스크롤 화살표 ID: DownButton\\n' +\n    '\\t\\t\\t\\t\\t\\t2761 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\\n' +\n    '\\t\\t\\t\\t\\t\\t\\t2762 단추 왼쪽 스크롤 화살표 ID: UpButton\\n' +\n    '\\t\\t\\t\\t\\t\\t\\t2763 위치 조정 위치 ID: ScrollbarThumb\\n' +\n    '\\t\\t\\t\\t\\t\\t\\t2764 단추 페이지 오른쪽으로 ID: DownPageButton\\n' +\n    '\\t\\t\\t\\t\\t\\t\\t2765 단추 오른쪽 스크롤 화살표 ID: DownButton\\n' +\n    '\\t\\t\\t\\t\\t\\t2766 위치 조정 (disabled)\\n' +\n    '\\t\\t\\t2 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65282\\n' +\n    '\\t\\t\\t\\t3 창 ID: 59648\\n' +\n    '\\t\\t\\t\\t\\t4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000'... 14619 more characters,\n  focused_element: '20660 편집'\n}"},{"type":"image","data":"[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=13379 chars=247404 sha256=1b4ad627bdf4a9d83a2300b5f48b01d3ed967dbb705b721a5606f250a28be781]","mimeType":"image/jpeg","_meta":{"codex/imageDetail":"original"}},{"type":"image","data":"[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=13379 chars=227604 sha256=3d17a65edae084268c4cc11dd286411254e13c956789d6e40e31d7240a9f6cfc]","mimeType":"image/jpeg","_meta":{"codex/imageDetail":"original"}},{"type":"image","data":"[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=13379 chars=37289 sha256=e156390e23ac32580c088a63f7bf499bf151a06c9091281baba71c928584559a]…69687 chars truncated…[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=13379 chars=218384 sha256=c1ae34f6364d4102218306d7ab85f56116fdc5b70a1468e7ae03e8260e402a73]","mimeType":"image/jpeg","_meta":{"codex/imageDetail":"original"}},{"type":"image","data":"[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=13379 chars=227604 sha256=3d17a65edae084268c4cc11dd286411254e13c956789d6e40e31d7240a9f6cfc]","mimeType":"image/jpeg","_meta":{"codex/imageDetail":"original"}},{"type":"image","data":"[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=13379 chars=76292 sha256=459945d96d49257c04b97a14ada26b5cb241b443f2530807c04a91f21b4960d0]","mimeType":"image/jpeg","_meta":{"codex/imageDetail":"original"}},{"type":"image","data":"/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQAHMDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3aa+8VX+v65Bot3okVtp0kcaW93ZSu85MSSEGZZgEBLYz5bY64bpVG78a6jd6RLc6LbWsdxJZ2M0Ed0GYJLPcNEyuVYZC7eoxzk8itS68O66Nb1W50vXrSxstSdHlX+zjJcRlY1jJjlMuwHC5G6NgD2NSS+D4BhLS4MNukNjCiFC5AtpzLyxOSWzjPbrz0pJaDb/r5r9LlZfEN/rP9mw6K1rYz3lpcSubyBpzbzQyxI8bKsiZILyKcN1AOcDBb4Ivdb1PRvt+u3WmzecWESWdm8Gza7KdxaV92cA8Yxz1rRi8NLb+MpddgudkUtu8clrs4MrGPMobPGViQEY7Z9cy6FpX9jaNDYed53llzv27c7nLdMn1xWkXt/XUzl2GXZ4NYl4etbt2vWsa7j612UmclQwdTF4+m36aZL5OoPbSC2kwDsl2nacHjrx+NfP/AIO+Oevvex2OtaMmsyOdim1QxXBP0AIY/gK+i2Vo5FdeqnIrkPBvge28MeKPE2s27xNJqtxutwgO6CIncynjglj27AVVaEnNODFSlFRakjrtPlaa1tp2gmtmmiWQwTgCSIkfdbHGRW5aHpWXbxnPNa9onSqnorMmGrujWtj0rQiqjbDpV+MVwzOyBKKWkFLWRqFFFFAH/9k=","mimeType":"image/jpeg","_meta":{"codex/imageDetail":"original"}}],"isError":false,"_meta":{"codex/toolSurface":{"app":{"appId":"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe","kind":"appId"},"kind":"computerUse"}}}
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
>   "title": "Select Coupler datatype text",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"Control_L+a\"});"
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
>   "title": "Set Coupler client datatype",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"CltChCmd_GL_9086_1\"});"
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
>   "title": "Confirm Coupler datatype",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"Return\"});"
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
>   "title": "Verify Coupler declaration",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/Coupler|DataType|Required|CltChCmd_GL/.test(x)).slice(-30).join(\"\\n\")); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 					32 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 							18322 콘솔 트리 (selectable) Coupler
> 					21217 custom DataType
> 					21219 custom Required
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
>   "title": "Reopen Coupler datatype list",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1365,y:493,screenshotId:globalThis.state.screenshots[0].id,click_count:2});"
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
>   "title": "Filter Coupler datatype list",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"CltChCmd_GL_9086_1\"});"
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
>   "title": "Inspect datatype search result",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*", App: Lasal2.exe.\n' +
>     '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch* Secondary Actions: Raise\n' +
>     '\t\t22123 편집\n' +

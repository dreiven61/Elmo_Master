> 					187 콘솔 트리 (selectable, disabled) LMCSdoExecutor
> 					188 콘솔 트리 (selectable, disabled) TCPIPServer
> 					189 콘솔 트리 (selectable, disabled) TCPMotionInterface
> 			6216 탭 항목 (selectable) Lib
> 			6217 탭 항목 (selectable) File
> 			6218 탭 항목 (selectable) Global
> 			6219 탭 항목 (selectable) Net
> 			6220 탭 항목 (selectable) Class
> 			195 단추 Close
> 		196 Tab Properties ID: 424886560
> 			197 창 (disabled) ID: 309505912
> 				198 TABLE Properties Window ID: 315226536
> 					6221 custom Name
> 					6222 custom Revision
> 					6223 custom GUID
> 					6224 custom Task Settings
> 					6225 custom Sigmatek
> 					6226 custom OSInterface
> 					6227 custom IconPath
> 					6228 custom SharedCommandTable
> 					6229 custom Objectsize
> 					6230 custom Singleton
> 					6231 custom Hidden
> 					6232 custom Deprecated
> 					6233 custom GCCOptions
> 					6234 custom Comment
> 					6235 custom Filename
> 				214 도구 모음 ID: 59392
> 					215 단추
> 					216 단추
> 			6236 탭 항목 (selectable) Properties
> 			218 단추 Close
>
> The focused UI element is 4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex >= 0x8000 then physicalAxis := (CatalogIndex shr 8) and 0x007F; topologyCount := CatalogIndex and 0x00FF; if (topologyCount = 0) | (physicalAxis >= 7) | ((physicalAxis + topologyCount) > 7) then RETURN; end_if; (pEntry + 16)^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 20)^$UINT := phy ID: 10000.
> ```
>
> Image output: image/jpeg
>
> 명시적 Rebuild는 이전의 `DriveComL2.h` 오류 지점을 지나 현재 custom class까지 정상 컴파일 중입니다. 즉 프로젝트 열기 시 라이브러리 검사 오류와 실제 Rebuild 결과를 분리해야 했습니다. 완료 결과와 Link까지 계속 확인하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL Rebuild 결과 확인",
>   "timeout_ms": 20000,
>   "code": "\nawait new Promise(resolve => setTimeout(resolve, 10000));\nglobalThis.state = await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true});\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65282
> 				3 창 ID: 59648
> 					4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex >= 0x8000 then physicalAxis := (CatalogIndex shr 8) and 0x007F; topologyCount := CatalogIndex and 0x00FF; if (topologyCount = 0) | (physicalAxis >= 7) | ((physicalAxis + topologyCount) > 7) then RETURN; end_if; (pEntry + 16)^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 20)^$UINT := phy ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 위치 조정 위치 ID: ScrollbarThumb
> 							8 단추 페이지 아래로 ID: DownPageButton
> 							9 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							11 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							12 위치 조정 위치 ID: ScrollbarThumb
> 							13 단추 페이지 오른쪽으로 ID: DownPageButton
> 							14 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						15 위치 조정 (disabled)
> 			16 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281
> 				17 창 ID: 59648
> 					18 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
> 						19 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							20 단추 위쪽 스크롤 화살표 ID: UpButton
> 							21 위치 조정 위치 ID: ScrollbarThumb
> 							22 단추 페이지 아래로 ID: DownPageButton
> 							23 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						24 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							25 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							26 위치 조정 위치 ID: ScrollbarThumb
> 							27 단추 페이지 오른쪽으로 ID: DownPageButton
> 							28 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						29 위치 조정 (disabled)
> 			30 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				31 창 ID: 59648
> 					32 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 						33 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							34 단추 위쪽 스크롤 화살표 ID: UpButton
> 							35 위치 조정 위치 ID: ScrollbarThumb
> 							36 단추 페이지 아래로 ID: DownPageButton
> 							37 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							39 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							40 위치 조정 위치 ID: ScrollbarThumb
> 							41 단추 페이지 오른쪽으로 ID: DownPageButton
> 							42 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						43 위치 조정 (disabled)
> 		44 상태 표시줄 ID: 59393
> 			45 텍스트
> 			46 텍스트
> 			47 텍스트
> 			48 텍스트 Ln 1 Col 1
> 			49 텍스트
> 			50 텍스트
> 			51 텍스트
> 			52 텍스트
> 			53 텍스트 NUM
> 			54 텍스트
> 		55 창 xtpBarTop ID: 59419
> 			56 도구 모음 Script
> 			57 도구 모음 Edit
> 				7194 단추 Toggle bookmark
> 				7195 단추 (disabled) Previous bookmark
> 				7196 단추 (disabled) Next bookmark
> 				7197 단추 (disabled) Delete all bookmarks
> 				7198 단추 (disabled) Previous bookmark in this file
> 				7199 단추 (disabled) Next bookmark in this file
> 				7200 단추 Comment selected text (Ctrl+Shift+C)
> 				7201 단추 Remove comment (Ctrl+Shift+X)
> 				7202 단추 Unindent (Shift+Tab)
> 				7203 단추 Indent (Tab)
> 			68 도구 모음 Macros Manager
> 				7204 메뉴 항목 Macros
> 			70 도구 모음 Layout Manager
> 				7205 메뉴 항목 Layouts
> 			72 도구 모음 Toolbox
> 				7206 단추 DataAnalyzer
> 				7207 단추 Interpreter
> 				7208 단추 DiasDrive
> 				7209 단추 PLC Diagnosis
> 				7210 단추 Hardware Editor
> 				7211 단추 Graphical Hardware Editor
> 				7212 단추 Connection Manager
> 				7213 단추 Task Configuration
> 			81 도구 모음 Net Edit
> 				7214 단추 (disabled) Select
> 				7215 단추 (disabled) Move view
> 				7216 단추 (disabled) Insert comment
> 				7217 단추 (disabled) Zoom(+/-)
> 				7218 단추 (disabled) Zoom to all
> 				7219 단추 (disabled) Zoom selection
> 			88 도구 모음 Debug
> 				7220 단추 Go online (Alt+F6)
> 				7221 단추 Change Online Settings
> 				7222 메뉴 항목 Online Connection
> 				7223 단추 (disabled) Set Online Connection For Current Project
> 				7224 단추 (disabled) Download (F6)
> 				7225 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				7226 단추 (disabled) Download Module on the Fly
> 				7227 단추 (disabled) Save Project on PLC
> 				7228 단추 (disabled) Start (F7)
> 				7229 단추 (disabled) Reset (F8)
> 				7230 단추 Toggle breakpoint (F4)
> 				7231 단추 Create condition breakpoint
> 				7232 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				7233 단추 AWL trigger on/off
> 				7234 단추 (disabled) Fix AWL trigger to current instruction
> 				7235 단추 Activate/Deactivate Live View
> 				7236 단추 Activate/Deactivate Auto Watch
> 				7237 단추 (disabled) Goto instruction pointer
> 				7238 단추 (disabled) Step into (F5)
> 				7239 단추 (disabled) Step over (Alt+F5)
> 				7240 단추 (disabled) Step out (Shift+F5)
> 				7241 단추 (disabled) Set instruction pointer
> 			111 도구 모음 Build
> 				7242 메뉴 항목 Target Architecture
> 				7243 단추 Build changes (F9)
> 				7244 단추 Rebuild project (Strg+F9)
> 				7245 단추 (disabled) Cancel building (Ctrl+Break)
> 				7246 단추 Link project
> 			117 도구 모음 Standard
> 				7247 단추 New project (Strg+N)
> 				7248 단추 Open a file (Strg+Shift+O)
> 				7249 단추 Close active document (Strg+F4)
> 				7250 단추 (disabled) Save file (Strg+S)
> 				7251 단추 Open project (Strg+O)
> 				7252 단추 (disabled) Save project changes (Strg+Shift+S)
> 				7253 단추 Close project
> 				7254 단추 Print
> 				7255 단추 Cut (Strg+X)
> 				7256 단추 Copy (Strg+C)
> 				7257 단추 Paste (Strg+V)
> 				7258 메뉴 항목 (disabled) Undo (Strg+Z)
> 				7259 메뉴 항목 (disabled) Redo (Strg+Y)
> 				7260 단추 Navigate Backward (Alt+Left)
> 				7261 단추 (disabled) Navigate Forward (Alt +Right)
> 			133 메뉴 모음 Menu Bar
> 				7262 메뉴 항목 FILE
> 				7263 메뉴 항목 EDIT
> 				7264 메뉴 항목 VIEW
> 				7265 메뉴 항목 PROJECT
> 				7266 메뉴 항목 BUILD
> 				7267 메뉴 항목 DEBUG
> 				7268 메뉴 항목 ANALYZE
> 				7269 메뉴 항목 TOOLS
> 				7270 메뉴 항목 EXTRAS
> 				7271 메뉴 항목 WINDOW
> 				7272 메뉴 항목 HELP
> 		145 창 Splitter ID: 314043376
> 		146 창 Splitter ID: 314045392
> 		147 Tab Output ID: 424891576
> 			148 창 ID: 1200
> 				149 창 ID: 1200
> 					4627 LIST ID: 1201
> 						4842 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							4843 단추 위쪽 스크롤 화살표 ID: UpButton
> 							4844 단추 페이지 위로 ID: UpPageButton
> 							4845 위치 조정 위치 ID: ScrollbarThumb
> 							4846 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						6434 목록 항목 (selectable)
> 						6435 목록 항목 (selectable)
> 						6436 목록 항목 (selectable)
> 						6437 목록 항목 (selectable)
> 						6438 목록 항목 (selectable)
> 						6439 목록 항목 (selectable)
> 						6440 목록 항목 (selectable)
> 						6441 목록 항목 (selectable)
> 						6442 목록 항목 (selectable)
> 						6443 목록 항목 (selectable)
> 						6444 목록 항목 (selectable)
> 						6445 목록 항목 (selectable)
> 						6446 목록 항목 (selectable)
> 						6447 목록 항목 (selectable)
> 						6448 목록 항목 (selectable)
> 					162 스크롤 막대 (disabled) ID: 59904
> 						163 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						164 위치 조정 위치 ID: ScrollbarThumb
> 						165 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			7273 탭 항목 (selectable) Python Script
> 			7274 탭 항목 (selectable) Debugger
> 			7275 탭 항목 (selectable) Output
> 			169 단추 Close
> 		170 창 Splitter ID: 314042368
> 		171 Tab Class View ID: 424886104
> 			172 트리 ID: 103
> 				173 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					174 콘솔 트리 (selectable) External
> 					175 콘솔 트리 (selectable) Sigmatek
> 					176 콘솔 트리 (selectable) Elmo_1
> 					177 콘솔 트리 (selectable) Elmo_2
> 					178 콘솔 트리 (selectable) Elmo_3
> 					179 콘솔 트리 (selectable) Elmo_4
> 					180 콘솔 트리 (selectable) GL_9086_1
> 					181 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					182 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					183 콘솔 트리 (selectable) LMCControlCommandService
> 					184 콘솔 트리 (selectable) LMCDiagnosticsService
> 					185 콘솔 트리 (selectable) LMCEcatInputLatch
> 					186 콘솔 트리 (selectable) LMCRecorderStore

>   "code": "nodeRepl.write({windowKeys:Object.getOwnPropertyNames(Object.getPrototypeOf(state.window)), stateKeys:Object.keys(state)})"
> }
> ```
>
> ```text
> {
>   windowKeys: [
>     'constructor',
>     '__defineGetter__',
>     '__defineSetter__',
>     'hasOwnProperty',
>     '__lookupGetter__',
>     '__lookupSetter__',
>     'isPrototypeOf',
>     'propertyIsEnumerable',
>     'toString',
>     'valueOf',
>     '__proto__',
>     'toLocaleString'
>   ],
>   stateKeys: [ 'window', 'screenshots', 'accessibility' ]
> }
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
>   "title": "파일 열기 창 닫기",
>   "code": "await sky.press_key({window: state.window, key: 'Escape'}); await new Promise(resolve=>setTimeout(resolve,700)); state = await sky.get_window_state({window: targetWindow, include_screenshot:true, include_text:true}); nodeRepl.write({title:state.window.title, text:(state.accessibility?.tree||state.accessibility||'').toString().slice(0,4000)}); if(state.screenshots?.[0]) await nodeRepl.emitImage(state.screenshots[0])"
> }
> ```
>
> ```text
> nodeRepl.emitImage received an unsupported value
> ```
>
> <details><summary>Called 3 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 상태 재확인",
>   "code": "nodeRepl.write({title:state.window.title, accessibility:state.accessibility})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface*',
>   accessibility: null
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 화면 확인",
>   "code": "nodeRepl.write(state.screenshots?.map(s=>({keys:Object.keys(s),id:s.id,width:s.width,height:s.height,mimeType:s.mimeType,kind:typeof s.image})))"
> }
> ```
>
> ```text
> [
>   {
>     keys: [
>       'id',      'zIndex',
>       'url',     'originX',
>       'originY', 'width',
>       'height'
>     ],
>     id: 'screenshot-0',
>     width: 1920,
>     height: 1040,
>     mimeType: undefined,
>     kind: 'undefined'
>   },
>   {
>     keys: [
>       'id',      'zIndex',
>       'url',     'originX',
>       'originY', 'width',
>       'height'
>     ],
>     id: 'screenshot-1',
>     width: 161,
>     height: 25,
>     mimeType: undefined,
>     kind: 'undefined'
>   },
>   {
>     keys: [
>       'id',      'zIndex',
>       'url',     'originX',
>       'originY', 'width',
>       'height'
>     ],
>     id: 'screenshot-2',
>     width: 156,
>     height: 20,
>     mimeType: undefined,
>     kind: 'undefined'
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
>   "title": "LASAL 화면 표시",
>   "code": "await nodeRepl.emitImage(state.screenshots[0].url)"
> }
> ```
>
> Image output: image/jpeg
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "임시 Server 채널 실행 취소",
>   "code": "await sky.press_key({window: targetWindow, key: 'Control_L+z'}); await new Promise(resolve=>setTimeout(resolve,700)); state = await sky.get_window_state({window: targetWindow, include_screenshot:true, include_text:true}); nodeRepl.write({title:state.window.title,screens:state.screenshots.length}); await nodeRepl.emitImage(state.screenshots[0].url)"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface',
>   screens: 3
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
>   "title": "클래스 트리 상단 이동",
>   "code": "await sky.scroll({window:targetWindow,x:1810,y:230,scrollY:-1220,scrollX:0,screenshotId:state.screenshots[0].id}); await new Promise(resolve=>setTimeout(resolve,500)); state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); await nodeRepl.emitImage(state.screenshots[0].url)"
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
>   "title": "TCPMotionInterface 메뉴 열기",
>   "code": "await sky.click({window:targetWindow,x:1757,y:376,mouse_button:'right',screenshotId:state.screenshots[0].id}); await new Promise(resolve=>setTimeout(resolve,300)); state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility); await nodeRepl.emitImage(state.screenshots[0].url)"
> }
> ```
>
> ```text
> {
>   tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.\n' +
>     '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise\n' +
>     '\t\t1 창 작업 영역 ID: 59648\n' +
>     '\t\t\t2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285\n' +
>     '\t\t\t\t3 창 ID: 59648\n' +
>     '\t\t\t\t\t4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000\n' +
>     '\t\t\t\t\t\t5 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t\t6 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t7 단추 페이지 위로 ID: UpPageButton\n' +
>     '\t\t\t\t\t\t\t8 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t9 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t10 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t11 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t\t12 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t13 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t14 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t15 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t16 위치 조정 (disabled)\n' +
>     '\t\t\t17 창 Motion_Network Secondary Actions: Raise ID: 65281\n' +
>     '\t\t\t\t18 창 ID: 59648\n' +
>     '\t\t\t\t\t19 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t20 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t21 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t22 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t23 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t24 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t25 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t26 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t27 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t28 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t29 위치 조정 (disabled)\n' +
>     '\t\t\t30 창 Elmo_4 Secondary Actions: Raise ID: 65286\n' +
>     '\t\t\t\t31 창 ID: 59648\n' +
>     '\t\t\t\t\t32 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000\n' +
>     '\t\t\t\t\t\t33 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t\t34 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t35 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t36 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t37 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t\t39 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t40 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t41 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t42 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t43 위치 조정 (disabled)\n' +
>     '\t\t\t44 창 EtherCAT_Network Secondary Actions: Raise ID: 65284\n' +
>     '\t\t\t\t45 창 ID: 59648\n' +
>     '\t\t\t\t\t46 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t47 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t48 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t49 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t50 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t51 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283\n' +
>     '\t\t\t\t52 창 ID: 59648\n' +
>     '\t\t\t\t\t53 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; b'... 9033 more characters,
>   focused_element: '203 트리 ID: 103'
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>

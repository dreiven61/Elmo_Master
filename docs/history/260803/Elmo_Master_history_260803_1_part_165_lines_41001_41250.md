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
>   "title": "LASAL Class View 이전 위치 복원",
>   "code": "await sky.press_key({window:lasalWindow15,key:\"ALT+LEFT\"});"
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
>   "title": "LASAL Class View 이전 위치 확인",
>   "code": "var classView35=await sky.get_window_state({window:lasalWindow15,include_text:false}); nodeRepl.write(classView35.window);"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 4790364,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - EtherCAT_Network.lcn'
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
>   "title": "LASAL IDE 재시작 준비",
>   "code": "await sky.press_key({window:lasalWindow15,key:\"ALT+F4\"});"
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
>   "title": "LASAL 종료 확인",
>   "code": "await new Promise(r=>setTimeout(r,500)); var closeState36=await sky.get_window_state({window:lasalWindow15,include_text:true}); nodeRepl.write(closeState36.accessibility?.tree?.split(\"\\n\").filter(x=>/Libraries|not used|removed|예|아니요|Yes|No/.test(x)));"
> }
> ```
>
> ```text
> [
>   '\t\t\t25541 단추 예(Y) ID: 6',
>   '\t\t\t25542 단추 아니요(N) ID: 7',
>   '\t\t\t25544 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535',
>   '\t\t\t\t\t3495 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="EtherCAT_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "Elmo_11" GUID = "{47F87ED0-D0EE-486E-BC7A-A70547F6C0D9}" Class = "Elmo_1" Position = "(1500,1620)" Visualized = "true" Remotely = "true"> <Channels> <Server Name="ActPos"/> <Server Name="ActSpeed"/> <Server Name="ActTorque"/> <Server Name="ActXw"/> <Server Name="AL_StatusCode"/> <Server Name="AxEnable"/> <Server Name="AxError"/> <Server Name="AxErrorQuit"/> <Server Name="ClassState"/> <Server Name="ControlWord"/> <Server Name="DeviceAdress"/> <Server Name="DeviceName"/> <Server Name="DriveIF"/> <Server Name="EtherCATOut_1"/> <Server Name="EtherCATState"/> <Server Name="Inputs_DigitalInputs"/> <Server Name="Online"/> <Server Name="Outputs_DigitalOutputs"/> <Server Name="ProductCode"/> <Server Name="ProductRevision"/> <Server Name="RevisionNo"/> <Server Name="SerialNo"/> <Server Name="SetPos"/> <Server Name="SetSpeed"/> <Server Name="SetTorque"/> <Server Name="SlaveState"/> <Server Name="StateWord"/> <Server Name="VendorID"/> <Client Name="NoSDOBuffer" Value="0"/> <Client Name="SlaveIndex" Value="1"/> <Client Name="toMaster"/> <Client Name="UseDefaultEnableSequence" Value="1"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{7266B399-596C-4DBB-A5BF-56AC8BC68024}" Class="ECAT_DS402Base"> <Channels> <Server Name="ActPos"/> <Server Name="ActSpeed"/> <Server Name="ActTorque"/> <Server Name="ActXw"/> <Server Name="AL_StatusCode"/> <Server Name="AxEnable"/> <Server Name="AxError"/> <Server Name="AxErrorQuit"/> <Server Name="ClassState"/> <Server Name="ControlWord"/> <Server Name="DeviceAdress"/> <Server Name="DeviceName"/> <Server Name="DriveIF"/> <Server Name="EtherCATState"/> <Server Name="Online"/> <Server Name="ProductCode"/> <Server Name="ProductRevision"/> <Server Name="RevisionNo"/> <Server Name="SerialNo"/> <Server Name="SetPos"/> <Server Name="SetSpeed"/> <Server Name="SetTorque"/> <Server Name="SlaveState"/> <Server Name="StateWord"/> <Server Name="VendorID"/> <Client Name="NoSDOBuffer" Value="0"/> <Client Name="SlaveIndex" Value="1"/> <Client Name="toMaster"/> <Client Name="UseDefaultEnableSequence" Value="1"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{80EAE1CA-1B88-4D0B-86A1-68539F1C74D5}" Class="ECAT_Slave_Base"> <Channels> <Server Name="AL_StatusCode"/> <Server Name="ClassState"/> <Server Name="DeviceAdress"/> <Server Name="DeviceName"/> <Server Name="EtherCATState"/> <Server Name="Online"/> <Server Name="ProductCode"/> <Server Name="ProductRevision"/> <Server Name="RevisionNo"/> <Server Name="SerialNo"/> <Server Name="SlaveState"/> <Server Name="VendorID"/> <Client Name="NoAsyncBuffer" Value="0"/> <Client Name="NoSDOBuffer" Value="0"/> <Client Name="NoSSCBuffer" Value="0"/> <Client Name="Required" Value="1"/> <Client Name="SlaveIndex" Value="1"/> <Client Name="toMaster"/> <Client Name="toStdLib"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components ID: 10000',
>   '\t\t\t\t\t\t3496 스크롤 막대 세로 ID: NonClientVerticalScrollBar',
>   '\t\t\t\t\t\t3501 스크롤 막대 가로 ID: NonClientHorizontalScrollBar',
>   '\t\t\t\t\t\t3510 스크롤 막대 세로 ID: NonClientVerticalScrollBar',
>   '\t\t\t\t\t\t3515 스크롤 막대 가로 ID: NonClientHorizontalScrollBar',
>   '\t\t\t\t\t3469 스크롤 막대 세로 ID: NonClientVerticalScrollBar',
>   '\t\t\t\t\t3474 스크롤 막대 가로 ID: NonClientHorizontalScrollBar',
>   '\t\t\t\t\t3482 스크롤 막대 세로 ID: NonClientVerticalScrollBar',
>   '\t\t\t\t\t3487 스크롤 막대 가로 ID: NonClientHorizontalScrollBar',
>   '\t\t\t\t\t\t3524 스크롤 막대 세로 ID: NonClientVerticalScrollBar',
>   '\t\t\t\t\t\t3529 스크롤 막대 가로 ID: NonClientHorizontalScrollBar',
>   '\t\t\t\t\t\t3538 스크롤 막대 세로 ID: NonClientVerticalScrollBar',
>   '\t\t\t\t\t\t3543 스크롤 막대 가로 ID: NonClientHorizontalScrollBar',
>   '\t\t\t\t\t3551 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000',
>   '\t\t\t\t\t\t3552 스크롤 막대 세로 ID: NonClientVerticalScrollBar',
>   '\t\t\t\t\t\t3557 스크롤 막대 가로 ID: NonClientHorizontalScrollBar',
>   '\t\t\t\t\t\t5020 스크롤 막대 세로 ID: NonClientVerticalScrollBar',
>   '\t\t\t\t23396 스크롤 막대 세로 ID: NonClientVerticalScrollBar',
>   '\t\t\t\t23402 스크롤 막대 가로 ID: NonClientHorizontalScrollBar',
>   '\t\t\t\t\t\t\t\t\t23432 콘솔 트리 (selectable, disabled) _NotInitialized',
>   '\t\t\t\t\t\t\t\t\t23436 콘솔 트리 (selectable, disabled) _NoHardware',
>   '\t\t\t\t\t\t\t\t\t23443 콘솔 트리 (selectable, disabled) _HardwareNotRequiredIRQ',
>   '\t\t\t\t\t\t\t\t\t23451 콘솔 트리 (selectable, disabled) _NoCDIASInterface',
>   '\t\t\t\t\t\t\t\t\t23453 콘솔 트리 (selectable, disabled) _NoVaranInterface',
>   '\t\t\t\t\t\t\t\t\t23461 콘솔 트리 (selectable, disabled) _PortNoLink',
>   '\t\t\t\t\t\t\t\t\t23462 콘솔 트리 (selectable, disabled) _VaranDriverNotExists',
>   '\t\t\t\t\t\t\t\t\t23467 콘솔 트리 (selectable, disabled) _ManagerNotExists',
>   '\t\t\t\t\t\t\t\t\t23470 콘솔 트리 (selectable, disabled) _ComponentNotExists',
>   '\t\t\t\t\t\t\t\t\t23471 콘솔 트리 (selectable, disabled) _ClientNotExists',
>   '\t\t\t\t\t\t\t\t\t23472 콘솔 트리 (selectable, disabled) _CdiasEEPromNotExists',
>   '\t\t\t\t\t\t\t\t\t23473 콘솔 트리 (selectable, disabled) _CdiasEEPromNoGrant',
>   '\t\t\t\t\t\t\t\t\t23475 콘솔 트리 (selectable, disabled) _PortNotExists',
>   '\t\t\t\t\t\t\t\t\t23477 콘솔 트리 (selectable, disabled) _NoMutex',
>   '\t\t\t\t\t\t\t\t\t23478 콘솔 트리 (selectable, disabled) _NoTask',
>   '\t\t\t\t\t\t\t\t\t23479 콘솔 트리 (selectable, disabled) _IDNotFound',
>   '\t\t\t\t\t\t\t\t\t23480 콘솔 트리 (selectable, disabled) _IDNotInitialized',
>   '\t\t\t\t\t\t\t\t\t23482 콘솔 트리 (selectable, disabled) _CallbackNotHandled',
>   '\t\t\t\t\t\t\t\t\t23483 콘솔 트리 (selectable, disabled) _NoMem',
>   '\t\t\t\t\t\t\t\t\t23484 콘솔 트리 (selectable, disabled) _NoLegacyWd',
>   '\t\t\t\t\t\t\t\t\t23487 콘솔 트리 (selectable, disabled) _SPIFlashNoAccess',
>   '\t\t\t\t\t\t\t\t\t23488 콘솔 트리 (selectable, disabled) _ClientNotready',
>   '\t\t\t\t\t\t\t\t\t23495 콘솔 트리 (selectable, disabled) _InvalidSerialNo',
>   '\t\t\t\t\t\t\t\t\t23498 콘솔 트리 (selectable, disabled) _NoCalibDataFound',
>   '\t\t\t\t\t\t\t\t\t23510 콘솔 트리 (selectable, disabled) _DOTypeNotSupported',
>   '\t\t\t\t\t\t\t\t\t23515 콘솔 트리 (selectable, disabled) _APIUsageNotAllowed',
>   '\t\t\t\t\t\t\t\t\t23516 콘솔 트리 (selectable, disabled) _NodeTypeWrong',
>   '\t\t\t\t\t\t\t\t\t23519 콘솔 트리 (selectable, disabled) _OperationNotAllowed',
>   '\t\t\t\t\t\t\t\t\t23520 콘솔 트리 (selectable, disabled) _NodeNumberOverflow',
>   '\t\t\t\t\t\t\t\t\t23521 콘솔 트리 (selectable, disabled) _NoHandleFound',
>   '\t\t\t\t\t\t\t\t\t23522 콘솔 트리 (selectable, disabled) _TopologyNotAllowed',
>   '\t\t\t25541 단추 예(Y) ID: 6',
>   '\t\t\t25542 단추 아니요(N) ID: 7',
>   '\t\t\t25544 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535'
> ]
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
>   "title": "사용 중인 라이브러리 유지",
>   "code": "await sky.click({window:lasalWindow15,x:809,y:454});"
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
>   "title": "LASAL smoke 세션 재실행",
>   "code": "await new Promise(r=>setTimeout(r,500)); await sky.launch_app({app:lasalApp12.id}); await new Promise(r=>setTimeout(r,1500)); var windowsLasal37=await sky.list_windows({app:lasalApp12.id}); nodeRepl.write(windowsLasal37.filter(w=>w.app.includes(\"Lasal2.exe\")));"
> }
> ```
>
> ```text
> []
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL smoke 세션 대기",
>   "code": "await new Promise(r=>setTimeout(r,2500)); var windowsLasal38=await sky.list_windows({app:lasalApp12.id}); nodeRepl.write(windowsLasal38.filter(w=>w.app.includes(\"Lasal2.exe\")));"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 22020838,
>     title: 'Lasal Class 2 (02.03.001)'
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
>   "title": "LASAL 프로젝트 재열기",
>   "code": "var lasalWindow38=windowsLasal38.find(w=>w.app.includes(\"Lasal2.exe\")); await sky.press_key({window:lasalWindow38,key:\"CTRL+O\"}); await new Promise(r=>setTimeout(r,300)); await sky.type_text({window:lasalWindow38,text:\"Elmo_EtherCAT_Test_4Axis.lcp\"}); await sky.press_key({window:lasalWindow38,key:\"ENTER\"});"
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
>   "title": "LASAL smoke 프로젝트 로딩",
>   "code": "await new Promise(r=>setTimeout(r,7000)); var windowsLasal39=await sky.list_windows({app:lasalApp12.id}); nodeRepl.write(windowsLasal39.filter(w=>w.app.includes(\"Lasal2.exe\")));"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 22020838,
>     title: 'Lasal Class 2 (02.03.001)'
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
>   "title": "LASAL smoke 프로젝트 로딩 대기",
>   "code": "await new Promise(r=>setTimeout(r,5000)); var windowsLasal40=await sky.list_windows({app:lasalApp12.id}); nodeRepl.write(windowsLasal40.filter(w=>w.app.includes(\"Lasal2.exe\")));"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 22020838,
>     title: 'Lasal Class 2 (02.03.001)'
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>

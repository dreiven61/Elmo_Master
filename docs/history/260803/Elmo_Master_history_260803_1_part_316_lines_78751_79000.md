> 						27 단추 페이지 왼쪽으로 ID: UpPageButton
> 						28 위치 조정 위치 ID: ScrollbarThumb
> 						29 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					30 위치 조정 (disabled)
> 			31 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				32 창 ID: 59648
> 					33 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 						34 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							35 단추 위쪽 스크롤 화살표 ID: UpButton
> 							36 단추 페이지 위로 ID: UpPageButton
> 							37 위치 조정 위치 ID: ScrollbarThumb
> 							38 단추 페이지 아래로 ID: DownPageButton
> 							39 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						40 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							41 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							42 위치 조정 위치 ID: ScrollbarThumb
> 							43 단추 페이지 오른쪽으로 ID: DownPageButton
> 							44 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						45 위치 조정 (disabled)
> 			46 창 Comm_Network Secondary Actions: Raise ID: 65283
> 				47 창 ID: 59648
> 					48 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						49 단추 위쪽 스크롤 화살표 ID: UpButton
> 						50 단추 페이지 위로 ID: UpPageButton
> 						51 위치 조정 위치 ID: ScrollbarThumb
> 						52 단추 페이지 아래로 ID: DownPageButton
> 						53 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					54 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						55 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						56 위치 조정 위치 ID: ScrollbarThumb
> 						57 단추 페이지 오른쪽으로 ID: DownPageButton
> 						58 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					59 위치 조정 (disabled)
> 			75 창 LMCControlCommandService Secondary Actions: Raise ID: 65286
> 				76 창 ID: 59648
> 					77 창 FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; case CommandId of 0x103C, 0x1042, 0x202B: ResponseSize := HandleRegistryCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x2023, 0x2024, 0x2022, 0x2028, 0x202E, 0x209F, 0x20A0, 0x20A2: ResponseSize := HandleAxisCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x20D2, 0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085, 0x20A4, 0x2045, 0x2051, 0x20E7: ResponseSize := HandleGroupCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x7D00, 0x7D10, 0x7D12, 0x7D13, 0x7D20, 0x7D22: ResponseSize := HandleAdminCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); else ResponseSize := -1; end_case; END_FUNCTION FUNCTION LMCControlCommandService::HandleRegistryCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR VAR objectNameLength : UDINT; objectName : ARRAY [0..255] OF CHAR; resolvedReference : UINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; case CommandId of 0x103C: if ResponseCapacity < 14 then RETURN; end_if; resolvedReference := 0; if RequestFrameSize = 88 then (pRequestFrame + 87)^ := 0; if IsClientConnected(#LMCAxis1) = 1 then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis1.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 1; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis2) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis2.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 2; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis3) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis3.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 3; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis4) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis4.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricm ID: 10000
> 						78 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							79 단추 위쪽 스크롤 화살표 ID: UpButton
> 							80 단추 페이지 위로 ID: UpPageButton
> 							81 위치 조정 위치 ID: ScrollbarThumb
> 							82 단추 페이지 아래로 ID: DownPageButton
> 							83 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						84 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							85 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							86 위치 조정 위치 ID: ScrollbarThumb
> 							87 단추 페이지 오른쪽으로 ID: DownPageButton
> 							88 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						89 위치 조정 (disabled)
> 			90 창 Comm_Network.lcn Secondary Actions: Raise ID: 65282
> 				91 창 ID: 59648
> 					92 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="Comm_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "LMCControlCommandService1" GUID = "{5E164D6C-7E45-4BA4-B0F7-F9DBCCE8C71B}" Class = "LMCControlCommandService" Position = "(930,1380)" Visualized = "false"> <Channels> <Server Name="ClassSvr"/> <Client Name="LMCAxis1"/> <Client Name="LMCAxis2"/> <Client Name="LMCAxis3"/> <Client Name="LMCAxis4"/> <Client Name="LMCAxis5"/> <Client Name="LMCAxis6"/> <Client Name="LMCAxis7"/> <Client Name="LMCAxis8"/> <Client Name="LMCAxis9"/> <Client Name="LMCRobot"/> </Channels> </Object> <Object Name = "LMCDiagnosticsService1" GUID = "{F42F0DD4-D9CC-4E5B-B073-F88FACAD14A8}" Class = "LMCDiagnosticsService" Position = "(870,900)" Visualized = "false"> <Channels> <Server Name="ClassSvr"/> <Server Name="DiagnosticsBootCounter" Value="0"/> <Client Name="InputLatch"/> <Client Name="RecorderStore"/> <Client Name="SdoAxis1"/> <Client Name="SdoAxis2"/> <Client Name="SdoAxis3"/> <Client Name="SdoAxis4"/> </Channels> </Object> <Object Name = "TCPIPServer1" GUID = "{42E82217-EDCD-47A0-BF97-FCBD9C009436}" Class = "TCPIPServer" Position = "(870,180)" Visualized = "true" Remotely = "true" CyclicTime = "1 ms" BackgroundTime = "always"> <Channels> <Server Name="Control"/> <Server Name="ErrorCode"/> <Server Name="SemaName01"/> <Server Name="SemaName02"/> <Server Name="TaskName"/> <Client Name="_MultiTask"/> <Client Name="_StdLib"/> <Client Name="_TaskObjectControl"/> <Client Name="Config" Value="0"/> <Client Name="ConnectionsPerRun" Value="1"/> <Client Name="MaxConnections" Value="2"/> <Client Name="Port" Value="4000"/> <Client Name="SigCLib"/> <Client Name="SizeOfTXBuffer"/> <Client Name="Timeout"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{BEB0D8C1-05A6-452D-879B-F50A84747DCB}" Class="_TCPIPServer"> <Channels> <Server Name="Control"/> <Server Name="ErrorCode"/> <Server Name="SemaName01"/> <Server Name="SemaName02"/> <Server Name="TaskName"/> <Client Name="_MultiTask"/> <Client Name="_StdLib"/> <Client Name="_TaskObjectControl"/> <Client Name="Config"/> <Client Name="ConnectionsPerRun" Value="1"/> <Client Name="MaxConnections" Value="2"/> <Client Name="Port"/> <Client Name="SigCLib"/> <Client Name="SizeOfTXBuffer"/> <Client Name="Timeout"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="CheckSum" GUID="{924983ED-FE4B-4B5A-BC71-6E6963A07A78}" Class="_CheckSum"> <Channels> <Server Name="ClassSvr"/> </Channels> </Object> <Object Name="StrSemaName01" GUID="{299AFE23-53C0-4268-B520-661EA498CF23}" Class="String"> <Channels> <Server Name="Data"/> <Client Name="SingleRealloc" Value="1"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{B9D2D328-1416-469A-BE13-9F6CBBB1958D}" Class="StringInternal"> <Channels> <Server Name="Data"/> <Client Name="DataBuffer"/> <Client Name="SingleRealloc"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> ID: 10000
> 						93 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							94 단추 위쪽 스크롤 화살표 ID: UpButton
> 							95 위치 조정 위치 ID: ScrollbarThumb
> 							96 단추 페이지 아래로 ID: DownPageButton
> 							97 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						98 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							99 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							100 위치 조정 위치 ID: ScrollbarThumb
> 							101 단추 페이지 오른쪽으로 ID: DownPageButton
> 							102 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						103 위치 조정 (disabled)
> 			104 창 EtherCAT_Network Secondary Actions: Raise ID: 65281
> 				105 창 ID: 59648
> 					106 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						107 단추 위쪽 스크롤 화살표 ID: UpButton
> 						108 위치 조정 위치 ID: ScrollbarThumb
> 						109 단추 페이지 아래로 ID: DownPageButton
> 						110 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					111 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						112 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						113 위치 조정 위치 ID: ScrollbarThumb
> 						114 단추 페이지 오른쪽으로 ID: DownPageButton
> 						115 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					116 위치 조정 (disabled)
> 			117 창 LMCSdoExecutor Secondary Actions: Raise ID: 65280
> 				118 창 ID: 59648
> 					119 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
> 						120 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							121 단추 위쪽 스크롤 화살표 ID: UpButton
> 							122 위치 조정 위치 ID: ScrollbarThumb
> 							123 단추 페이지 아래로 ID: DownPageButton
> 							124 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						125 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							126 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							127 위치 조정 위치 ID: ScrollbarThumb
> 							128 단추 페이지 오른쪽으로 ID: DownPageButton
> 							129 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						130 위치 조정 (disabled)
> 		131 상태 표시줄 ID: 59393
> 			132 텍스트
> 			133 텍스트
> 			134 텍스트
> 			135 텍스트
> 			136 텍스트
> 			137 텍스트 Offline
> 			138 텍스트
> 			139 텍스트 NUM
> 			140 텍스트
> 		141 창 xtpBarTop ID: 59419
> 			142 도구 모음 Edit
> 				10750 단추 Toggle bookmark
> 				10751 단추 (disabled) Previous bookmark
> 				10752 단추 (disabled) Next bookmark
> 				10753 단추 (disabled) Delete all bookmarks
> 				10754 단추 (disabled) Previous bookmark in this file
> 				10755 단추 (disabled) Next bookmark in this file
> 				10756 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				10757 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				10758 단추 (disabled) Unindent (Shift+Tab)
> 				10759 단추 (disabled) Indent (Tab)
> 			153 도구 모음 Macros Manager
> 				10760 메뉴 항목 Macros
> 			155 도구 모음 Layout Manager
> 				10761 메뉴 항목 Layouts
> 			157 도구 모음 Toolbox
> 				10762 단추 DataAnalyzer
> 				10763 메뉴 항목 Toolbar Options
> 			160 도구 모음 Net Edit
> 				10764 단추 (disabled) Select
> 				10765 메뉴 항목 Toolbar Options
> 			163 도구 모음 Debug
> 				10766 단추 Go online (Alt+F6)
> 				10767 단추 Change Online Settings
> 				10768 메뉴 항목 Online Connection
> 				10769 단추 (disabled) Set Online Connection For Current Project
> 				10770 단추 (disabled) Download (F6)
> 				10771 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				10772 단추 (disabled) Download Module on the Fly
> 				10773 단추 (disabled) Save Project on PLC
> 				10774 단추 (disabled) Start (F7)
> 				10775 단추 (disabled) Reset (F8)
> 				10776 단추 Toggle breakpoint (F4)
> 				10777 단추 Create condition breakpoint
> 				10778 메뉴 항목 Toolbar Options
> 			177 도구 모음 Build
> 				10779 메뉴 항목 Target Architecture
> 				10780 단추 Build changes (F9)
> 				10781 단추 Rebuild project (Strg+F9)
> 				10782 단추 (disabled) Cancel building (Ctrl+Break)
> 				10783 단추 Link project
> 			183 도구 모음 Standard
> 				10784 단추 New project (Strg+N)
> 				10785 단추 Open a file (Strg+Shift+O)
> 				10786 단추 Close active document (Strg+F4)
> 				10787 단추 (disabled) Save file (Strg+S)
> 				10788 단추 Open project (Strg+O)
> 				10789 단추 (disabled) Save project changes (Strg+Shift+S)
> 				10790 단추 Close project
> 				10791 단추 Print
> 				10792 단추 Cut (Strg+X)
> 				10793 단추 Copy (Strg+C)
> 				10794 단추 Paste (Strg+V)
> 				10795 메뉴 항목 Undo (Strg+Z)
> 				10796 메뉴 항목 (disabled) Redo (Strg+Y)
> 				10797 단추 Navigate Backward (Alt+Left)
> 				10798 단추 (disabled) Navigate Forward (Alt +Right)
> 			199 메뉴 모음 Menu Bar
> 				10799 메뉴 항목 FILE
> 				10800 메뉴 항목 EDIT
> 				10801 메뉴 항목 VIEW
> 				10802 메뉴 항목 PROJECT
> 				10803 메뉴 항목 BUILD
> 				10804 메뉴 항목 DEBUG
> 				10805 메뉴 항목 ANALYZE
> 				10806 메뉴 항목 TOOLS
> 				10807 메뉴 항목 EXTRAS
> 				10808 메뉴 항목 WINDOW
> 				10809 메뉴 항목 HELP
> 		211 창 Splitter ID: 609954768
> 		212 창 Splitter ID: 609955104
> 		213 Tab Output ID: 279800176
> 			214 창 ID: 1200
> 				215 창 ID: 1200
> 					216 LIST ID: 1204
> 						217 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							218 단추 위쪽 스크롤 화살표 ID: UpButton
> 							219 단추 페이지 위로 ID: UpPageButton
> 							220 위치 조정 위치 ID: ScrollbarThumb
> 							221 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						222 목록 항목 (selectable)
> 						223 목록 항목 (selectable)
> 						224 목록 항목 (selectable)
> 						225 목록 항목 (selectable)
> 						226 목록 항목 (selectable)
> 						227 목록 항목 (selectable)
> 						228 목록 항목 (selectable)
> 						229 목록 항목 (selectable)
> 						230 목록 항목 (selectable)
> 						231 목록 항목 (selectable)
> 						232 목록 항목 (selectable)
> 						233 목록 항목 (selectable)
> 						234 목록 항목 (selectable)
> 						235 목록 항목 (selectable)
> 						236 목록 항목 (selectable)
> 						237 목록 항목 (selectable)
> 					238 스크롤 막대 ID: 59904
> 						239 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						240 위치 조정 위치 ID: ScrollbarThumb
> 						241 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			10810 탭 항목 (selectable) Output
> 			243 단추 Close
> 		244 창 Splitter ID: 617298272
> 		245 Tab Class View ID: 279804736
> 			246 트리 ID: 103
> 				247 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					248 단추 위쪽 스크롤 화살표 ID: UpButton
> 					249 단추 페이지 위로 ID: UpPageButton
> 					250 위치 조정 위치 ID: ScrollbarThumb
> 					251 단추 페이지 아래로 ID: DownPageButton
> 					252 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				253 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					254 콘솔 트리 (selectable) External
> 					255 콘솔 트리 (selectable) Sigmatek
> 					256 콘솔 트리 (selectable) Elmo_1
> 					257 콘솔 트리 (selectable) Elmo_2
> 					258 콘솔 트리 (selectable) Elmo_3
> 					259 콘솔 트리 (selectable) Elmo_4
> 					260 콘솔 트리 (selectable) GL_9086_1
> 					261 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					262 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					263 콘솔 트리 (selectable) LMCControlCommandService
> 						264 콘솔 트리 (selectable) Servers
> 						265 콘솔 트리 (selectable) Clients
> 							266 콘솔 트리 (selectable) LMCAxis1
> 							267 콘솔 트리 (selectable) LMCAxis2
> 							268 콘솔 트리 (selectable) LMCAxis3
> 							269 콘솔 트리 (selectable) LMCAxis4
> 							270 콘솔 트리 (selectable) LMCAxis5
> 							271 콘솔 트리 (selectable) LMCAxis6
> 							272 콘솔 트리 (selectable) LMCAxis7
> 							273 콘솔 트리 (selectable) LMCAxis8
> 							274 콘솔 트리 (selectable) LMCAxis9
> 							275 콘솔 트리 (selectable) LMCRobot
> 						276 콘솔 트리 (selectable) Methods
> 							277 콘솔 트리 (selectable) Global
> 								278 콘솔 트리 (selectable) HandleRequest
> 								279 콘솔 트리 (selectable) ProcessAxisReference
> 							280 콘솔 트리 (selectable) Private
> 						281 콘솔 트리 (selectable) Variables
> 							282 콘솔 트리 (selectable) GroupMovePos
> 							283 콘솔 트리 (selectable) GroupKinematicReady
> 							284 콘솔 트리 (selectable) ReferenceState
> 								285 콘솔 트리 (selectable) 0..18
> 						286 콘솔 트리 (selectable) Objects
> 						287 콘솔 트리 (selectable) Dependencies
> 					288 콘솔 트리 (selectable) LMCDiagnosticsService
> 						10095 콘솔 트리 (selectable) Servers
> 						10096 콘솔 트리 (selectable) Clients
> 						10097 콘솔 트리 (selectable) Methods

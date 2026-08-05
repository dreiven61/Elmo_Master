> 				116 창 ID: 59648
> 					117 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
> 						118 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							119 단추 위쪽 스크롤 화살표 ID: UpButton
> 							120 위치 조정 위치 ID: ScrollbarThumb
> 							121 단추 페이지 아래로 ID: DownPageButton
> 							122 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						123 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							124 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							125 위치 조정 위치 ID: ScrollbarThumb
> 							126 단추 페이지 오른쪽으로 ID: DownPageButton
> 							127 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						128 위치 조정 (disabled)
> 		129 상태 표시줄 ID: 59393
> 			130 텍스트
> 			131 텍스트 TCPMotionInterface::CyWork
> 			132 텍스트
> 			133 텍스트 Ln 26 Col 22
> 			134 텍스트
> 			135 텍스트 Offline
> 			136 텍스트
> 			137 텍스트 NUM
> 			138 텍스트
> 		139 창 xtpBarTop ID: 59419
> 			140 도구 모음 Edit
> 				18725 단추 Toggle bookmark
> 				18726 단추 (disabled) Previous bookmark
> 				18727 단추 (disabled) Next bookmark
> 				18728 단추 (disabled) Delete all bookmarks
> 				18729 단추 (disabled) Previous bookmark in this file
> 				18730 단추 (disabled) Next bookmark in this file
> 				18731 단추 Comment selected text (Ctrl+Shift+C)
> 				18732 단추 Remove comment (Ctrl+Shift+X)
> 				18733 단추 Unindent (Shift+Tab)
> 				18734 단추 Indent (Tab)
> 			151 도구 모음 Macros Manager
> 				18735 메뉴 항목 Macros
> 			153 도구 모음 Layout Manager
> 				18736 메뉴 항목 Layouts
> 			155 도구 모음 Toolbox
> 				18737 단추 DataAnalyzer
> 				18738 메뉴 항목 Toolbar Options
> 			158 도구 모음 Net Edit
> 				18739 단추 (disabled) Select
> 				18740 메뉴 항목 Toolbar Options
> 			161 도구 모음 Debug
> 				18741 단추 Go online (Alt+F6)
> 				18742 단추 Change Online Settings
> 				18743 메뉴 항목 Online Connection
> 				18744 단추 (disabled) Set Online Connection For Current Project
> 				18745 단추 (disabled) Download (F6)
> 				18746 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				18747 단추 (disabled) Download Module on the Fly
> 				18748 단추 (disabled) Save Project on PLC
> 				18749 단추 (disabled) Start (F7)
> 				18750 단추 (disabled) Reset (F8)
> 				18751 단추 Toggle breakpoint (F4)
> 				18752 단추 Create condition breakpoint
> 				18753 메뉴 항목 Toolbar Options
> 			175 도구 모음 Build
> 				18754 메뉴 항목 Target Architecture
> 				18755 단추 Build changes (F9)
> 				18756 단추 Rebuild project (Strg+F9)
> 				18757 단추 (disabled) Cancel building (Ctrl+Break)
> 				18758 단추 Link project
> 			181 도구 모음 Standard
> 				18759 단추 New project (Strg+N)
> 				18760 단추 Open a file (Strg+Shift+O)
> 				18761 단추 Close active document (Strg+F4)
> 				18762 단추 (disabled) Save file (Strg+S)
> 				18763 단추 Open project (Strg+O)
> 				18764 단추 (disabled) Save project changes (Strg+Shift+S)
> 				18765 단추 Close project
> 				18766 단추 Print
> 				18767 단추 Cut (Strg+X)
> 				18768 단추 Copy (Strg+C)
> 				18769 단추 (disabled) Paste (Strg+V)
> 				18770 메뉴 항목 (disabled) Undo (Strg+Z)
> 				18771 메뉴 항목 (disabled) Redo (Strg+Y)
> 				18772 단추 Navigate Backward (Alt+Left)
> 				18773 단추 (disabled) Navigate Forward (Alt +Right)
> 			197 메뉴 모음 Menu Bar
> 				18774 메뉴 항목 FILE
> 				18775 메뉴 항목 EDIT
> 				18776 메뉴 항목 VIEW
> 				18777 메뉴 항목 PROJECT
> 				18778 메뉴 항목 BUILD
> 				18779 메뉴 항목 DEBUG
> 				18780 메뉴 항목 ANALYZE
> 				18781 메뉴 항목 TOOLS
> 				18782 메뉴 항목 EXTRAS
> 				18783 메뉴 항목 WINDOW
> 				18784 메뉴 항목 HELP
> 		209 창 Splitter ID: 820665376
> 		210 창 Splitter ID: 820662184
> 		211 Tab Output ID: 279800176
> 			212 창 ID: 1200
> 				213 창 ID: 1200
> 					214 LIST ID: 1204
> 						215 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							216 단추 위쪽 스크롤 화살표 ID: UpButton
> 							217 단추 페이지 위로 ID: UpPageButton
> 							218 위치 조정 위치 ID: ScrollbarThumb
> 							219 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						220 목록 항목 (selectable)
> 						221 목록 항목 (selectable)
> 						222 목록 항목 (selectable)
> 						223 목록 항목 (selectable)
> 						224 목록 항목 (selectable)
> 					225 스크롤 막대 (disabled) ID: 59904
> 						226 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						227 위치 조정 위치 ID: ScrollbarThumb
> 						228 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			18785 탭 항목 (selectable) Python Script
> 			18786 탭 항목 (selectable) Debugger
> 			18787 탭 항목 (selectable) Output
> 			232 단추 Close
> 		233 창 Splitter ID: 617298272
> 		234 Tab Class View ID: 279804736
> 			235 트리 ID: 103
> 				236 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					237 단추 위쪽 스크롤 화살표 ID: UpButton
> 					238 단추 페이지 위로 ID: UpPageButton
> 					239 위치 조정 위치 ID: ScrollbarThumb
> 					240 단추 페이지 아래로 ID: DownPageButton
> 					241 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				242 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					243 콘솔 트리 (selectable) External
> 					244 콘솔 트리 (selectable) Sigmatek
> 					245 콘솔 트리 (selectable) Elmo_1
> 					246 콘솔 트리 (selectable) Elmo_2
> 					247 콘솔 트리 (selectable) Elmo_3
> 					248 콘솔 트리 (selectable) Elmo_4
> 					249 콘솔 트리 (selectable) GL_9086_1
> 					250 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					251 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					252 콘솔 트리 (selectable) LMCControlCommandService
> 						253 콘솔 트리 (selectable) Servers
> 						254 콘솔 트리 (selectable) Clients
> 						255 콘솔 트리 (selectable) Methods
> 							256 콘솔 트리 (selectable) Global
> 								257 콘솔 트리 (selectable) HandleRequest
> 								258 콘솔 트리 (selectable) ProcessAxisReference
> 							259 콘솔 트리 (selectable) Private
> 						260 콘솔 트리 (selectable) Variables
> 							261 콘솔 트리 (selectable) GroupMovePos
> 							262 콘솔 트리 (selectable) GroupKinematicReady
> 							263 콘솔 트리 (selectable) ReferenceState
> 								264 콘솔 트리 (selectable) 0..18
> 						265 콘솔 트리 (selectable) Objects
> 						266 콘솔 트리 (selectable) Dependencies
> 					267 콘솔 트리 (selectable) LMCDiagnosticsService
> 					268 콘솔 트리 (selectable) LMCEcatInputLatch
> 						269 콘솔 트리 (selectable) Servers
> 						270 콘솔 트리 (selectable) Clients
> 							271 콘솔 트리 (selectable) EcatMaster
> 							272 콘솔 트리 (selectable) Drive1
> 							273 콘솔 트리 (selectable) Drive2
> 							274 콘솔 트리 (selectable) Drive3
> 							275 콘솔 트리 (selectable) Drive4
> 							276 콘솔 트리 (selectable) RecorderStore
> 							277 콘솔 트리 (selectable) Coupler
> 							278 콘솔 트리 (selectable) InputSlot
> 							279 콘솔 트리 (selectable) OutputSlot
> 						280 콘솔 트리 (selectable) Methods
> 						281 콘솔 트리 (selectable) Variables
> 						282 콘솔 트리 (selectable) Objects
> 						283 콘솔 트리 (selectable) Dependencies
> 					284 콘솔 트리 (selectable) LMCRecorderStore
> 					285 콘솔 트리 (selectable) LMCSdoExecutor
> 					286 콘솔 트리 (selectable) TCPIPServer
> 					287 콘솔 트리 (selectable) TCPMotionInterface
> 						288 콘솔 트리 (selectable) Base
> 						289 콘솔 트리 (selectable) Servers
> 						290 콘솔 트리 (selectable) Clients
> 							291 콘솔 트리 (selectable) _StdLib
> 							292 콘솔 트리 (selectable) Diagnostics
> 							293 콘솔 트리 (selectable) ControlCommands
> 						294 콘솔 트리 (selectable) Methods
> 						295 콘솔 트리 (selectable) Variables
> 						296 콘솔 트리 (selectable) Types
> 						297 콘솔 트리 (selectable) Network
> 						298 콘솔 트리 (selectable) Objects
> 						299 콘솔 트리 (selectable) Dependencies
> 			18788 탭 항목 (selectable) Lib
> 			18789 탭 항목 (selectable) File
> 			18790 탭 항목 (selectable) Class
> 			18791 탭 항목 (selectable) Global
> 			18792 탭 항목 (selectable) Net
> 			305 단추 Close
> 		306 Tab Properties ID: 279798808
> 			307 창 ID: 270081768
> 				308 TABLE Properties Window ID: 277338752
> 					18793 custom Name
> 					18794 custom Class
> 					18795 custom DataType
> 					18796 custom Type
> 					18797 custom Required
> 					18798 custom DefaultInitValue
> 					18799 custom Disable RedefinitionVariableWarning
> 					18800 custom Comment
> 				317 도구 모음 ID: 59392
> 					318 단추
> 					319 단추
> 			18801 탭 항목 (selectable) Properties
> 			321 단추 Close
>
> The focused UI element is 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000.
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
>   "title": "LMCEcatInputLatch Clients 메뉴 열기",
>   "code": "{ const observation=globalThis.state; const sid=observation?.screenshots?.[0]?.id; if(!sid) throw new Error('No screenshot'); globalThis.state=null; await sky.click({window:observation.window,screenshotId:sid,x:1230,y:490,mouse_button:'right'}); globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=globalThis.state.window; nodeRepl.write(String(globalThis.state.accessibility?.tree||globalThis.state.accessibility?.document_text||'')); }"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				3 창 ID: 59648
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
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
> 			16 창 Comm_Network Secondary Actions: Raise ID: 65283
> 				17 창 ID: 59648
> 					18 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						19 단추 위쪽 스크롤 화살표 ID: UpButton
> 						20 위치 조정 위치 ID: ScrollbarThumb
> 						21 단추 페이지 아래로 ID: DownPageButton
> 						22 단추 아래쪽 스크롤 화살표 ID: DownButton

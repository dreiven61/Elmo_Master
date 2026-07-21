> 						114 위치 조정 (disabled)
> 			115 창 HW_Network Secondary Actions: Raise ID: 65281
> 				116 창 ID: 59648
> 					117 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						118 단추 위쪽 스크롤 화살표 ID: UpButton
> 						119 위치 조정 위치 ID: ScrollbarThumb
> 						120 단추 페이지 아래로 ID: DownPageButton
> 						121 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					122 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						123 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						124 위치 조정 위치 ID: ScrollbarThumb
> 						125 단추 페이지 오른쪽으로 ID: DownPageButton
> 						126 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					127 위치 조정 (disabled)
> 			128 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				129 창 ID: 59648
> 					130 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 						131 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							132 단추 위쪽 스크롤 화살표 ID: UpButton
> 							133 단추 페이지 위로 ID: UpPageButton
> 							134 위치 조정 위치 ID: ScrollbarThumb
> 							135 단추 페이지 아래로 ID: DownPageButton
> 							136 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						137 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							138 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							139 위치 조정 위치 ID: ScrollbarThumb
> 							140 단추 페이지 오른쪽으로 ID: DownPageButton
> 							141 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						142 위치 조정 (disabled)
> 		143 상태 표시줄 ID: 59393
> 			144 텍스트
> 			145 텍스트 LMCEcatInputLatch::RtWork
> 			146 텍스트
> 			147 텍스트 Ln 286 Col 1
> 			148 텍스트
> 			149 텍스트 Offline
> 			150 텍스트
> 			151 텍스트 NUM
> 			152 텍스트
> 		153 창 xtpBarTop ID: 59419
> 			154 도구 모음 Script
> 			155 도구 모음 Edit
> 				156 단추 Toggle bookmark
> 				157 단추 (disabled) Previous bookmark
> 				158 단추 (disabled) Next bookmark
> 				159 단추 (disabled) Delete all bookmarks
> 				160 단추 (disabled) Previous bookmark in this file
> 				161 단추 (disabled) Next bookmark in this file
> 				162 단추 Comment selected text (Ctrl+Shift+C)
> 				163 단추 Remove comment (Ctrl+Shift+X)
> 				164 단추 Unindent (Shift+Tab)
> 				165 단추 Indent (Tab)
> 			166 도구 모음 Macros Manager
> 				167 메뉴 항목 Macros
> 			168 도구 모음 Layout Manager
> 				169 메뉴 항목 Layouts
> 			170 도구 모음 Toolbox
> 				171 단추 DataAnalyzer
> 				172 메뉴 항목 Toolbar Options
> 			173 도구 모음 Net Edit
> 				174 단추 (disabled) Select
> 				175 메뉴 항목 Toolbar Options
> 			176 도구 모음 Debug
> 				177 단추 Go online (Alt+F6)
> 				178 단추 Change Online Settings
> 				179 메뉴 항목 Online Connection
> 				180 단추 (disabled) Set Online Connection For Current Project
> 				181 단추 (disabled) Download (F6)
> 				182 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				183 단추 (disabled) Download Module on the Fly
> 				184 단추 (disabled) Save Project on PLC
> 				185 단추 (disabled) Start (F7)
> 				186 단추 (disabled) Reset (F8)
> 				187 단추 Toggle breakpoint (F4)
> 				188 단추 Create condition breakpoint
> 				189 메뉴 항목 Toolbar Options
> 			190 도구 모음 Build
> 				191 메뉴 항목 Target Architecture
> 				192 단추 Build changes (F9)
> 				193 단추 Rebuild project (Strg+F9)
> 				194 단추 (disabled) Cancel building (Ctrl+Break)
> 				195 단추 Link project
> 			196 도구 모음 Standard
> 				197 단추 New project (Strg+N)
> 				198 단추 Open a file (Strg+Shift+O)
> 				199 단추 Close active document (Strg+F4)
> 				200 단추 (disabled) Save file (Strg+S)
> 				201 단추 Open project (Strg+O)
> 				202 단추 (disabled) Save project changes (Strg+Shift+S)
> 				203 단추 Close project
> 				204 단추 Print
> 				205 단추 Cut (Strg+X)
> 				206 단추 Copy (Strg+C)
> 				207 단추 (disabled) Paste (Strg+V)
> 				208 메뉴 항목 (disabled) Undo (Strg+Z)
> 				209 메뉴 항목 (disabled) Redo (Strg+Y)
> 				210 단추 Navigate Backward (Alt+Left)
> 				211 단추 (disabled) Navigate Forward (Alt +Right)
> 			212 메뉴 모음 Menu Bar
> 				213 메뉴 항목 FILE
> 				214 메뉴 항목 EDIT
> 				215 메뉴 항목 VIEW
> 				216 메뉴 항목 PROJECT
> 				217 메뉴 항목 BUILD
> 				218 메뉴 항목 DEBUG
> 				219 메뉴 항목 ANALYZE
> 				220 메뉴 항목 TOOLS
> 				221 메뉴 항목 EXTRAS
> 				222 메뉴 항목 WINDOW
> 				223 메뉴 항목 HELP
> 		224 창 Splitter ID: 411855768
> 		225 창 Splitter ID: 411851736
> 		226 Tab Output ID: 409867992
> 			227 창 ID: 1200
> 				228 창 ID: 1200
> 					229 LIST ID: 1201
> 						230 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							231 단추 위쪽 스크롤 화살표 ID: UpButton
> 							232 단추 페이지 위로 ID: UpPageButton
> 							233 위치 조정 위치 ID: ScrollbarThumb
> 							234 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						235 목록 항목 (selectable)
> 						236 목록 항목 (selectable)
> 						237 목록 항목 (selectable)
> 						238 목록 항목 (selectable)
> 						239 목록 항목 (selectable)
> 						240 목록 항목 (selectable)
> 						241 목록 항목 (selectable)
> 						242 목록 항목 (selectable)
> 						243 목록 항목 (selectable)
> 						244 목록 항목 (selectable)
> 						245 목록 항목 (selectable)
> 						246 목록 항목 (selectable)
> 						247 목록 항목 (selectable)
> 					248 스크롤 막대 ID: 59904
> 						249 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						250 위치 조정 위치 ID: ScrollbarThumb
> 						251 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			252 탭 항목 (selectable) Python Script
> 			253 탭 항목 (selectable) Output
> 			254 탭 항목 (selectable) Debugger
> 			255 단추 Close
> 		256 창 Splitter ID: 411854424
> 		257 Tab Class View ID: 409868448
> 			258 트리 ID: 103
> 				259 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					260 콘솔 트리 (selectable) External
> 					261 콘솔 트리 (selectable) Sigmatek
> 					262 콘솔 트리 (selectable) _TCPIPServer_RT
> 					263 콘솔 트리 (selectable) Elmo_1
> 					264 콘솔 트리 (selectable) Elmo_2
> 					265 콘솔 트리 (selectable) Elmo_3
> 					266 콘솔 트리 (selectable) Elmo_4
> 					267 콘솔 트리 (selectable) LMCDiagnosticsService
> 					268 콘솔 트리 (selectable) LMCEcatInputLatch
> 					269 콘솔 트리 (selectable) LMCRecorderStore
> 					270 콘솔 트리 (selectable) TCPMotionInterface
> 			271 탭 항목 (selectable) Lib
> 			272 탭 항목 (selectable) File
> 			273 탭 항목 (selectable) Global
> 			274 탭 항목 (selectable) Net
> 			275 탭 항목 (selectable) Class
> 			276 단추 Close
> 		277 Tab Properties ID: 409871640
> 			278 창 ID: 121918456
> 				279 TABLE Properties Window ID: 127184512
> 				280 도구 모음 ID: 59392
> 					281 단추
> 					282 단추
> 			283 탭 항목 (selectable) Properties
> 			284 단추 Close
>
> The focused UI element is 4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 진단 클래스 펼치기",
>   "code": "{\n  var observationExpand = globalThis.state;\n  globalThis.state = null;\n  await sky.click({ window: observationExpand.window, element_index: 267, click_count: 2 });\n  globalThis.state = await sky.get_window_state({ window: observationExpand.window, include_screenshot: true, include_text: true });\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ```text
> call get_window_state before issuing coordinate input
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 진단 클래스 위치 확인",
>   "code": "globalThis.state = await sky.get_window_state({ window: globalThis.targetWindow, include_screenshot: true, include_text: false });\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(JSON.stringify(globalThis.state.screenshots.map(s=>({id:s.id,width:s.width,height:s.height,originX:s.originX,originY:s.originY,z:s.zIndex}))));"
> }
> ```
>
> ```text
> [{"id":"screenshot-0","width":1450,"height":763,"originX":-1880,"originY":41,"z":0},{"id":"screenshot-1","width":1440,"height":753,"originX":-1875,"originY":46,"z":1}]
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
>   "title": "LASAL IDE 전면 활성화",
>   "code": "await sky.activate_window({ window: globalThis.targetWindow });\nglobalThis.state = await sky.get_window_state({ window: globalThis.targetWindow, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65287
> 				3 창 ID: 59648
> 					4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 단추 페이지 위로 ID: UpPageButton
> 							8 위치 조정 위치 ID: ScrollbarThumb
> 							9 단추 페이지 아래로 ID: DownPageButton
> 							10 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						11 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							12 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							13 위치 조정 위치 ID: ScrollbarThumb
> 							14 단추 페이지 오른쪽으로 ID: DownPageButton
> 							15 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						16 위치 조정 (disabled)
> 			17 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				18 창 ID: 59648
> 					19 창 xtpBarTop ID: 59419
> 						20 도구 모음 Hardware Editor
> 							21 단추 Hardware Editor Configuration Settings
> 							22 단추 Calculate Resources of Project
> 							23 단추 (disabled) Upload Hardware Tree from PLC
> 							24 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							25 단추 Show Detail Mode\Show Configuration Mode
> 							26 단추 Generates the ENI File of the current project
> 					27 창 ID: 59648

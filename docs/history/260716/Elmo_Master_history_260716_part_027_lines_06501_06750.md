> 					217 custom Backup Includes and Loader
> 					218 custom Ignore at Cleanup
> 					219 custom Reencrypt Project On Close
> 					220 custom Enable OPC UA
> 					221 custom Enable initvalues for output parameters
> 					222 custom Use multiple CPU core
> 					223 custom Use Advanced-IO
> 					224 custom AutomationML
> 					225 custom IO Connection Manager Options
> 					226 custom Comment
> 					227 custom Filename
> 				228 도구 모음 ID: 59392
> 					229 단추
> 					230 단추
> 			231 탭 항목 (selectable) Properties
> 			232 단추 Close
>
> The focused UI element is 178 트리 ID: 103.
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
>   "code": "await sky.click({window:targetWindow, x:941, y:323, screenshotId:classExpanded.screenshots[0].id}); globalThis.classExpanded2 = await sky.get_window_state({window:targetWindow, include_text:true}); nodeRepl.write((classExpanded2.accessibility?.tree||\"\").split(\"\\n\").slice(175,360).join(\"\\n\"))",
>   "title": "TCPMotionInterface 항목 펼치기"
> }
> ```
>
> ```text
> 			174 탭 항목 (selectable) Debugger
> 			175 단추 Close
> 		176 창 Splitter ID: 311498736
> 		177 Tab Class View ID: 305714776
> 			178 트리 ID: 103
> 				179 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					180 콘솔 트리 (selectable) External
> 					181 콘솔 트리 (selectable) Sigmatek
> 					182 콘솔 트리 (selectable) _TCPIPServer_RT
> 					183 콘솔 트리 (selectable) Elmo_1
> 					184 콘솔 트리 (selectable) Elmo_2
> 					185 콘솔 트리 (selectable) Elmo_3
> 					186 콘솔 트리 (selectable) Elmo_4
> 					187 콘솔 트리 (selectable) TCPMotionInterface
> 						188 콘솔 트리 (selectable) Base
> 						189 콘솔 트리 (selectable) Servers
> 						190 콘솔 트리 (selectable) Clients
> 						191 콘솔 트리 (selectable) Methods
> 						192 콘솔 트리 (selectable) Variables
> 						193 콘솔 트리 (selectable) Types
> 						194 콘솔 트리 (selectable) Network
> 						195 콘솔 트리 (selectable) Objects
> 						196 콘솔 트리 (selectable) Dependencies
> 			197 탭 항목 (selectable) Lib
> 			198 탭 항목 (selectable) File
> 			199 탭 항목 (selectable) Global
> 			200 탭 항목 (selectable) Net
> 			201 탭 항목 (selectable) Class
> 			202 단추 Close
> 		203 Tab Properties ID: 305711584
> 			204 창 ID: 302137496
> 				205 TABLE Properties Window ID: 307037824
> 					206 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						207 단추 위쪽 스크롤 화살표 ID: UpButton
> 						208 위치 조정 위치 ID: ScrollbarThumb
> 						209 단추 페이지 아래로 ID: DownPageButton
> 						210 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					211 custom Name
> 					212 custom Revision
> 					213 custom GUID
> 					214 custom BaseClass
> 					215 custom Task Settings
> 					216 custom Sigmatek
> 					217 custom OSInterface
> 					218 custom IconPath
> 					219 custom SharedCommandTable
> 					220 custom Objectsize
> 					221 custom Singleton
> 					222 custom Hidden
> 					223 custom Deprecated
> 					224 custom GCCOptions
> 					225 custom Comment
> 					226 custom Filename
> 				227 도구 모음 ID: 59392
> 					228 단추
> 					229 단추
> 			230 탭 항목 (selectable) Properties
> 			231 단추 Close
>
> The focused UI element is 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000.
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
>   "code": "await sky.click({window:targetWindow, x:959, y:384, screenshotId:classExpanded2.screenshots[0].id}); globalThis.clientsExpanded = await sky.get_window_state({window:targetWindow, include_text:true}); nodeRepl.write((clientsExpanded.accessibility?.tree||\"\").split(\"\\n\").slice(180,360).join(\"\\n\"))",
>   "title": "TCPMotionInterface 클라이언트 펼치기"
> }
> ```
>
> ```text
> 				179 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					180 콘솔 트리 (selectable) External
> 					181 콘솔 트리 (selectable) Sigmatek
> 					182 콘솔 트리 (selectable) _TCPIPServer_RT
> 					183 콘솔 트리 (selectable) Elmo_1
> 					184 콘솔 트리 (selectable) Elmo_2
> 					185 콘솔 트리 (selectable) Elmo_3
> 					186 콘솔 트리 (selectable) Elmo_4
> 					187 콘솔 트리 (selectable) TCPMotionInterface
> 						188 콘솔 트리 (selectable) Base
> 						189 콘솔 트리 (selectable) Servers
> 						190 콘솔 트리 (selectable) Clients
> 						191 콘솔 트리 (selectable) Methods
> 						192 콘솔 트리 (selectable) Variables
> 						193 콘솔 트리 (selectable) Types
> 						194 콘솔 트리 (selectable) Network
> 						195 콘솔 트리 (selectable) Objects
> 						196 콘솔 트리 (selectable) Dependencies
> 			197 탭 항목 (selectable) Lib
> 			198 탭 항목 (selectable) File
> 			199 탭 항목 (selectable) Global
> 			200 탭 항목 (selectable) Net
> 			201 탭 항목 (selectable) Class
> 			202 단추 Close
> 		203 Tab Properties ID: 305711584
> 			204 창 ID: 302137496
> 				205 TABLE Properties Window ID: 307037824
> 					206 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						207 단추 위쪽 스크롤 화살표 ID: UpButton
> 						208 위치 조정 위치 ID: ScrollbarThumb
> 						209 단추 페이지 아래로 ID: DownPageButton
> 						210 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					211 custom Name
> 					212 custom Revision
> 					213 custom GUID
> 					214 custom BaseClass
> 					215 custom Task Settings
> 					216 custom Sigmatek
> 					217 custom OSInterface
> 					218 custom IconPath
> 					219 custom SharedCommandTable
> 					220 custom Objectsize
> 					221 custom Singleton
> 					222 custom Hidden
> 					223 custom Deprecated
> 					224 custom GCCOptions
> 					225 custom Comment
> 					226 custom Filename
> 				227 도구 모음 ID: 59392
> 					228 단추
> 					229 단추
> 			230 탭 항목 (selectable) Properties
> 			231 단추 Close
>
> The focused UI element is 178 트리 ID: 103.
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
>   "code": "globalThis.clientsExpanded2 = await sky.get_window_state({window:targetWindow, include_text:true}); nodeRepl.write((clientsExpanded2.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/LMCAxis|LMCRobot|StdLib|Clients|Servers|Power|pos|velo/.test(x)).join(\"\\n\"))",
>   "title": "LASAL 클라이언트 항목 확인"
> }
> ```
>
> ```text
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000
> 						194 콘솔 트리 (selectable) Servers
> 						195 콘솔 트리 (selectable) Clients
> 							196 콘솔 트리 (selectable) LMCAxis1
> 							197 콘솔 트리 (selectable) LMCAxis2
> 							198 콘솔 트리 (selectable) LMCAxis3
> 							199 콘솔 트리 (selectable) LMCAxis4
> 							200 콘솔 트리 (selectable) LMCRobot
> 							201 콘솔 트리 (selectable) _StdLib
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
>   "code": "await sky.click({window:targetWindow, element_index:196, mouse_button:\"right\"}); globalThis.clientMenu = await sky.get_window_state({window:targetWindow, include_text:true}); nodeRepl.write((clientMenu.accessibility?.tree||\"\").split(\"\\n\").slice(-80).join(\"\\n\"))",
>   "title": "Find in Implementation 메뉴 열기"
> }
> ```
>
> ```text
> 						165 목록 항목 (selectable)
> 						166 목록 항목 (selectable)
> 						167 목록 항목 (selectable)
> 					168 스크롤 막대 ID: 59904
> 						169 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						170 위치 조정 위치 ID: ScrollbarThumb
> 						171 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			172 탭 항목 (selectable) Python Script
> 			173 탭 항목 (selectable) Output
> 			174 탭 항목 (selectable) Debugger
> 			175 단추 Close
> 		176 창 Splitter ID: 311498736
> 		177 Tab Class View ID: 305714776
> 			178 트리 ID: 103
> 				179 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					180 단추 위쪽 스크롤 화살표 ID: UpButton
> 					181 위치 조정 위치 ID: ScrollbarThumb
> 					182 단추 페이지 아래로 ID: DownPageButton
> 					183 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				184 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					185 콘솔 트리 (selectable) External
> 					186 콘솔 트리 (selectable) Sigmatek
> 					187 콘솔 트리 (selectable) _TCPIPServer_RT
> 					188 콘솔 트리 (selectable) Elmo_1
> 					189 콘솔 트리 (selectable) Elmo_2
> 					190 콘솔 트리 (selectable) Elmo_3
> 					191 콘솔 트리 (selectable) Elmo_4
> 					192 콘솔 트리 (selectable) TCPMotionInterface
> 						193 콘솔 트리 (selectable) Base
> 						194 콘솔 트리 (selectable) Servers
> 						195 콘솔 트리 (selectable) Clients
> 							196 콘솔 트리 (selectable) LMCAxis1
> 							197 콘솔 트리 (selectable) LMCAxis2
> 							198 콘솔 트리 (selectable) LMCAxis3
> 							199 콘솔 트리 (selectable) LMCAxis4
> 							200 콘솔 트리 (selectable) LMCRobot

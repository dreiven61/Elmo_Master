> 							201 콘솔 트리 (selectable) _StdLib
> 						202 콘솔 트리 (selectable) Methods
> 						203 콘솔 트리 (selectable) Variables
> 						204 콘솔 트리 (selectable) Types
> 						205 콘솔 트리 (selectable) Network
> 						206 콘솔 트리 (selectable) Objects
> 						207 콘솔 트리 (selectable) Dependencies
> 			208 탭 항목 (selectable) Lib
> 			209 탭 항목 (selectable) File
> 			210 탭 항목 (selectable) Global
> 			211 탭 항목 (selectable) Net
> 			212 탭 항목 (selectable) Class
> 			213 단추 Close
> 		214 Tab Properties ID: 305711584
> 			215 창 ID: 302137496
> 				216 TABLE Properties Window ID: 307037824
> 					217 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						218 단추 위쪽 스크롤 화살표 ID: UpButton
> 						219 위치 조정 위치 ID: ScrollbarThumb
> 						220 단추 페이지 아래로 ID: DownPageButton
> 						221 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					222 custom Name
> 					223 custom Revision
> 					224 custom GUID
> 					225 custom BaseClass
> 					226 custom Task Settings
> 					227 custom Sigmatek
> 					228 custom OSInterface
> 					229 custom IconPath
> 					230 custom SharedCommandTable
> 					231 custom Objectsize
> 					232 custom Singleton
> 					233 custom Hidden
> 					234 custom Deprecated
> 					235 custom GCCOptions
> 					236 custom Comment
> 					237 custom Filename
> 				238 도구 모음 ID: 59392
> 					239 단추
> 					240 단추
> 			241 탭 항목 (selectable) Properties
> 			242 단추 Close
>
> The focused UI element is 178 트리 ID: 103.
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); await sky.click({window:targetWindow,x:978,y:407}); globalThis.axisClientExpanded = await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((axisClientExpanded.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/LMCAxis|Power|pos|velo|Clients/.test(x)).join(\"\\n\"))",
>   "title": "LMCAxis1 멤버 펼치기"
> }
> ```
>
> ```text
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000
> 						196 콘솔 트리 (selectable) Clients
> 							197 콘솔 트리 (selectable) LMCAxis1
> 							212 콘솔 트리 (selectable) LMCAxis2
> 							213 콘솔 트리 (selectable) LMCAxis3
> 							214 콘솔 트리 (selectable) LMCAxis4
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
>   "code": "nodeRepl.write((axisClientExpanded.accessibility?.tree||\"\").split(\"\\n\").slice(185,285).join(\"\\n\"))",
>   "title": "LMCAxis1 멤버 목록 확인"
> }
> ```
>
> ```text
> 					184 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				185 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					186 콘솔 트리 (selectable) External
> 					187 콘솔 트리 (selectable) Sigmatek
> 					188 콘솔 트리 (selectable) _TCPIPServer_RT
> 					189 콘솔 트리 (selectable) Elmo_1
> 					190 콘솔 트리 (selectable) Elmo_2
> 					191 콘솔 트리 (selectable) Elmo_3
> 					192 콘솔 트리 (selectable) Elmo_4
> 					193 콘솔 트리 (selectable) TCPMotionInterface
> 						194 콘솔 트리 (selectable) Base
> 						195 콘솔 트리 (selectable) Servers
> 						196 콘솔 트리 (selectable) Clients
> 							197 콘솔 트리 (selectable) LMCAxis1
> 								198 콘솔 트리 (selectable) _LMCAXIS_OPMODE
> 									199 콘솔 트리 (selectable) PASSIVE_LMCAXIS
> 									200 콘솔 트리 (selectable) WAITIDLE_LMCAXIS
> 									201 콘솔 트리 (selectable) IDLE_LMCAXIS
> 									202 콘솔 트리 (selectable) MOVE_LMCAXIS
> 									203 콘솔 트리 (selectable) FOLLOW_LMCAXIS
> 									204 콘솔 트리 (selectable) STOPED_LMCAXIS
> 									205 콘솔 트리 (selectable) LOCKED_LMCAXIS
> 									206 콘솔 트리 (selectable) REFRUN_LMCAXIS
> 									207 콘솔 트리 (selectable) TUNEPOS_LMCAXIS
> 									208 콘솔 트리 (selectable) TUNENEG_LMCAXIS
> 									209 콘솔 트리 (selectable) ERROR_LMCAXIS
> 									210 콘솔 트리 (selectable) WAITPASSIVE_LMCAXIS
> 									211 콘솔 트리 (selectable) SBT_LMCAXIS
> 							212 콘솔 트리 (selectable) LMCAxis2
> 							213 콘솔 트리 (selectable) LMCAxis3
> 							214 콘솔 트리 (selectable) LMCAxis4
> 							215 콘솔 트리 (selectable) LMCRobot
> 							216 콘솔 트리 (selectable) _StdLib
> 						217 콘솔 트리 (selectable) Methods
> 						218 콘솔 트리 (selectable) Variables
> 						219 콘솔 트리 (selectable) Types
> 						220 콘솔 트리 (selectable) Network
> 						221 콘솔 트리 (selectable) Objects
> 						222 콘솔 트리 (selectable) Dependencies
> 			223 탭 항목 (selectable) Lib
> 			224 탭 항목 (selectable) File
> 			225 탭 항목 (selectable) Global
> 			226 탭 항목 (selectable) Net
> 			227 탭 항목 (selectable) Class
> 			228 단추 Close
> 		229 Tab Properties ID: 305711584
> 			230 창 ID: 302137496
> 				231 TABLE Properties Window ID: 307037824
> 					232 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						233 단추 위쪽 스크롤 화살표 ID: UpButton
> 						234 위치 조정 위치 ID: ScrollbarThumb
> 						235 단추 페이지 아래로 ID: DownPageButton
> 						236 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					237 custom Name
> 					238 custom Class
> 					239 custom DataType
> 					240 custom Type
> 					241 custom Required
> 					242 custom DefaultInitValue
> 					243 custom Comment
> 				244 도구 모음 ID: 59392
> 					245 단추
> 					246 단추
> 			247 탭 항목 (selectable) Properties
> 			248 단추 Close
>
> The focused UI element is 178 트리 ID: 103.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window:targetWindow,x:958,y:185}); globalThis.serversExpanded = await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((serversExpanded.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Servers|Power|pos|velo|Clients/.test(x)).join(\"\\n\"))",
>   "title": "TCPMotionInterface 서버 항목 펼치기"
> }
> ```
>
> ```text
> 						195 콘솔 트리 (selectable) Servers
> 						196 콘솔 트리 (selectable) Clients
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
>   "code": "globalThis.serversExpanded2 = await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((serversExpanded2.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Servers|Power|pos|velo/.test(x)).join(\"\\n\"))",
>   "title": "검색 대상 서버 확인"
> }
> ```
>
> ```text
> 						195 콘솔 트리 (selectable) Servers
> 							207 콘솔 트리 (selectable) Power
> 							208 콘솔 트리 (selectable) pos
> 							209 콘솔 트리 (selectable) velo
> 							218 콘솔 트리 (selectable) RobotPowerOn
> 							219 콘솔 트리 (selectable) RobotPowerOff
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
>   "code": "await sky.click({window:targetWindow,element_index:207,mouse_button:\"right\"}); globalThis.powerMenu=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((powerMenu.accessibility?.tree||\"\").split(\"\\n\").slice(-45).join(\"\\n\"))",
>   "title": "Power 구현 검색 메뉴 열기"
> }
> ```
>
> ```text
> 									237 콘솔 트리 (selectable) WAITPASSIVE_LMCAXIS
> 									238 콘솔 트리 (selectable) SBT_LMCAXIS
> 							239 콘솔 트리 (selectable) LMCAxis2
> 							240 콘솔 트리 (selectable) LMCAxis3
> 							241 콘솔 트리 (selectable) LMCAxis4
> 							242 콘솔 트리 (selectable) LMCRobot
> 							243 콘솔 트리 (selectable) _StdLib
> 						244 콘솔 트리 (selectable) Methods
> 						245 콘솔 트리 (selectable) Variables
> 						246 콘솔 트리 (selectable) Types
> 						247 콘솔 트리 (selectable) Network
> 						248 콘솔 트리 (selectable) Objects
> 						249 콘솔 트리 (selectable) Dependencies
> 			250 탭 항목 (selectable) Lib
> 			251 탭 항목 (selectable) File
> 			252 탭 항목 (selectable) Global
> 			253 탭 항목 (selectable) Net
> 			254 탭 항목 (selectable) Class
> 			255 단추 Close
> 		256 Tab Properties ID: 305711584
> 			257 창 ID: 302137496
> 				258 TABLE Properties Window ID: 307037824
> 					259 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						260 단추 위쪽 스크롤 화살표 ID: UpButton
> 						261 위치 조정 위치 ID: ScrollbarThumb
> 						262 단추 페이지 아래로 ID: DownPageButton
> 						263 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					264 custom Name
> 					265 custom GUID
> 					266 custom Visualized
> 					267 custom World
> 					268 custom Alarm
> 					269 custom DataType

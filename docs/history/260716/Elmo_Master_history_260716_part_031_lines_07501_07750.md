> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 20778988,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface'
>   },
>   {
>     app: 'process:C:\\Windows\\explorer.exe',
>     id: 657502,
>     title: 'Lasal_PRG - 파일 탐색기 [ main ↓0 ↑0 ]'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe',
>     id: 197574,
>     title: 'LasalMotionControlLibTestApp - Microsoft Visual Studio'
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
>   "code": "await sky.click({window:targetWindow,screenshotId:findBox.screenshots[2].id,x:448,y:61}); await new Promise(r=>setTimeout(r,1200)); globalThis.afterPowerSearch=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write({screens:afterPowerSearch.screenshots.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height})), focused:afterPowerSearch.accessibility?.focused_element})",
>   "title": "Power 구현 검색 실행"
> }
> ```
>
> ```text
> {
>   screens: [
>     { id: 'screenshot-0', z: 0, w: 1450, h: 763 },
>     { id: 'screenshot-1', z: 1, w: 1440, h: 753 }
>   ],
>   focused: '129 LIST ID: 1204'
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
>   "code": "await sky.click({window:targetWindow,x:400,y:300}); await sky.press_key({window:targetWindow,key:\"Control_L+f\"}); globalThis.posDialog=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((posDialog.accessibility?.tree||\"\").split(\"\\n\").slice(-28).join(\"\\n\"))",
>   "title": "pos 구현 검색 열기"
> }
> ```
>
> ```text
> 		269 대화 상자 Find Secondary Actions: Raise
> 			270 텍스트 Find What:
> 			271 콤보 상자 Find What: ID: 1115
> 				272 편집 Find What: ID: 1001
> 				273 단추 열기 ID: DropDown
> 			274 확인란 Match Case ID: 1116
> 			275 확인란 Whole Word ID: 1117
> 			276 확인란 (disabled) Include Properties ID: 1776
> 			277 확인란 Show in Find Results ID: 1430
> 			278 확인란 Show in Find Results 2 ID: 1431
> 			279 확인란 Ignore Whitespaces ID: 1423
> 			280 확인란 Ignore Return ID: 1424
> 			281 확인란 Ignore Comments ID: 1799
> 			282 확인란 Use regular expression ID: 1774
> 			283 그룹 Direction:
> 			284 라디오 단추 (selectable) Up ID: 1118
> 			285 라디오 단추 (selectable) Down ID: 1119
> 			286 단추 Find ID: 1120
> 			287 단추 Mark All ID: 1729
> 			288 단추 Clear all marks ID: 1730
> 			289 단추 Close ID: 1
> 			290 단추 Cancel ID: 2
> 			291 제목 표시줄
> 				292 메뉴 모음 시스템 ID: MenuBar
> 					293 메뉴 항목 시스템
> 				294 단추 닫기
>
> The focused UI element is 129 LIST ID: 1204.
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
>   "code": "await sky.click({window:targetWindow,screenshotId:posDialog.screenshots[2].id,x:235,y:61}); await sky.press_key({window:targetWindow,key:\"Control_L+a\"}); await sky.type_text({window:targetWindow,text:\"pos\"}); await sky.click({window:targetWindow,screenshotId:posDialog.screenshots[2].id,x:449,y:61}); await new Promise(r=>setTimeout(r,900)); globalThis.afterPosSearch=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((afterPosSearch.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Find Results|Total found|TCPMotionInterface\\.st/.test(x)).slice(-25).join(\"\\n\"))",
>   "title": "pos 구현 검색 실행"
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
>   "code": "await sky.click({window:targetWindow,x:400,y:300}); await sky.press_key({window:targetWindow,key:\"Control_L+f\"}); globalThis.veloDialog=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write(veloDialog.screenshots.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height})))",
>   "title": "velo 구현 검색 열기"
> }
> ```
>
> ```text
> [
>   { id: 'screenshot-0', z: 0, w: 1450, h: 763 },
>   { id: 'screenshot-1', z: 1, w: 1440, h: 753 },
>   { id: 'screenshot-2', z: 2, w: 504, h: 220 }
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
>   "code": "await sky.click({window:targetWindow,screenshotId:veloDialog.screenshots[2].id,x:235,y:61}); await sky.press_key({window:targetWindow,key:\"Control_L+a\"}); await sky.type_text({window:targetWindow,text:\"velo\"}); await sky.click({window:targetWindow,screenshotId:veloDialog.screenshots[2].id,x:449,y:61}); await new Promise(r=>setTimeout(r,900)); globalThis.afterVeloSearch=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((afterVeloSearch.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Total found|TCPMotionInterface\\.st/.test(x)).slice(-20).join(\"\\n\"))",
>   "title": "velo 구현 검색 실행"
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
>   "code": "await sky.click({window:targetWindow,x:410,y:562,click_count:2}); await new Promise(r=>setTimeout(r,500)); globalThis.resultOpened=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write({focused:resultOpened.accessibility?.focused_element, selected:resultOpened.accessibility?.selected_text, doc:(resultOpened.accessibility?.document_text||\"\").slice(0,300)})",
>   "title": "검색 결과 구현 위치 열기"
> }
> ```
>
> ```text
> { focused: '129 LIST ID: 1204', selected: undefined, doc: '' }
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
>   "code": "await sky.click({window:targetWindow,x:275,y:328,mouse_button:\"right\"}); globalThis.tokenMenu=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((tokenMenu.accessibility?.tree||\"\").split(\"\\n\").slice(-45).join(\"\\n\"))",
>   "title": "velo 구현 컨텍스트 메뉴 확인"
> }
> ```
>
> ```text
> 									224 콘솔 트리 (selectable) WAITPASSIVE_LMCAXIS
> 									225 콘솔 트리 (selectable) SBT_LMCAXIS
> 							226 콘솔 트리 (selectable) LMCAxis2
> 							227 콘솔 트리 (selectable) LMCAxis3
> 							228 콘솔 트리 (selectable) LMCAxis4
> 							229 콘솔 트리 (selectable) LMCRobot
> 							230 콘솔 트리 (selectable) _StdLib
> 						231 콘솔 트리 (selectable) Methods
> 						232 콘솔 트리 (selectable) Variables
> 						233 콘솔 트리 (selectable) Types
> 						234 콘솔 트리 (selectable) Network
> 						235 콘솔 트리 (selectable) Objects
> 						236 콘솔 트리 (selectable) Dependencies
> 			237 탭 항목 (selectable) Lib
> 			238 탭 항목 (selectable) File
> 			239 탭 항목 (selectable) Global
> 			240 탭 항목 (selectable) Net
> 			241 탭 항목 (selectable) Class
> 			242 단추 Close
> 		243 Tab Properties ID: 305711584
> 			244 창 ID: 302137496
> 				245 TABLE Properties Window ID: 307037824
> 					246 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						247 단추 위쪽 스크롤 화살표 ID: UpButton
> 						248 위치 조정 위치 ID: ScrollbarThumb
> 						249 단추 페이지 아래로 ID: DownPageButton
> 						250 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					251 custom Name
> 					252 custom GUID
> 					253 custom Visualized
> 					254 custom World
> 					255 custom Alarm
> 					256 custom DataType
> 					257 custom Type
> 					258 custom Initialize
> 					259 custom WriteProtected
> 					260 custom Retentive
> 					261 custom Comment
> 				262 도구 모음 ID: 59392
> 					263 단추
> 					264 단추
> 			265 탭 항목 (selectable) Properties
> 			266 단추 Close
>
> The focused UI element is 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000.
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
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); await sky.click({window:targetWindow,x:1055,y:536}); globalThis.globalTab=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((globalTab.accessibility?.tree||\"\").split(\"\\n\").slice(175,320).join(\"\\n\"))",
>   "title": "LASAL Global 뷰 확인"
> }
> ```
>

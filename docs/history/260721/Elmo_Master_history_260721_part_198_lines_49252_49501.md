>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var shotTree=globalThis.state.screenshots.sort((a,b)=>b.zIndex-a.zIndex)[0]; await sky.click({window:globalThis.targetWindow,x:1055,y:223,screenshotId:shotTree.id}); await sky.type_text({window:globalThis.targetWindow,text:\"LMCRecorderStore\"}); nodeRepl.write(\"typed class name in tree\");",
>   "title": "Locate LASAL recorder class",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> typed class name in tree
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(JSON.stringify({selected:globalThis.state.accessibility?.selected_elements||[],focused:globalThis.state.accessibility?.focused_element||null,targets:(globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/LMCRecorder|TCPMotion|LMCDiagnostics/.test(x)).slice(-80)}));",
>   "title": "Check LASAL recorder class selection",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {"selected":[],"focused":"273 트리 ID: 105","targets":["Window: \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface\", App: Lasal2.exe.","\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise","\t\t\t2 창 TCPMotionInterface Secondary Actions: Raise ID: 65280","\t\t\t\t\t4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000","\t\t\t17 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65288","\t\t\t\t\t19 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:=\"axis\"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:=\"target_position_last_tx\"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:=\"digital_outputs_last_tx\"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:=\"control_word_last_tx\"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:=\"actual_position\"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:=\"digital_inputs\"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:=\"status_word\"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000"]}
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
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:\"Control_L+Shift_L+t\"}); globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(JSON.stringify({tree:(globalThis.state.accessibility?.tree||\"\").slice(-8000),focused:globalThis.state.accessibility?.focused_element||null,shots:globalThis.state.screenshots.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height}))}));",
>   "title": "Open LASAL tree search",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {"tree":"롤 화살표 ID: UpButton\n\t\t\t\t\t\t114 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t115 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t116 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t117 위치 조정 (disabled)\n\t\t\t118 창 Comm_Network Secondary Actions: Raise ID: 65283\n\t\t\t\t119 창 ID: 59648\n\t\t\t\t\t120 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t121 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t122 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t123 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t124 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t125 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t126 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t127 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t128 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t129 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t130 위치 조정 (disabled)\n\t\t\t131 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282\n\t\t\t\t132 창 ID: 59648\n\t\t\t\t\t133 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000\n\t\t\t\t\t\t134 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t135 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t136 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t137 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t138 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t139 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t140 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t141 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t142 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t143 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t144 위치 조정 (disabled)\n\t\t\t145 창 HW_Network Secondary Actions: Raise ID: 65281\n\t\t\t\t146 창 ID: 59648\n\t\t\t\t\t147 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t148 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t149 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t150 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t151 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t152 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t153 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t154 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t155 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t156 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t157 위치 조정 (disabled)\n\t\t158 상태 표시줄 ID: 59393\n\t\t\t159 텍스트 \n\t\t\t160 텍스트\n\t\t\t161 텍스트\n\t\t\t162 텍스트\n\t\t\t163 텍스트 \n\t\t\t164 텍스트 Offline\n\t\t\t165 텍스트\n\t\t\t166 텍스트 NUM\n\t\t\t167 텍스트\n\t\t168 창 xtpBarTop ID: 59419\n\t\t\t169 도구 모음 Script\n\t\t\t170 도구 모음 Edit\n\t\t\t\t171 단추 Toggle bookmark\n\t\t\t\t172 단추 (disabled) Previous bookmark\n\t\t\t\t173 단추 (disabled) Next bookmark\n\t\t\t\t174 단추 (disabled) Delete all bookmarks\n\t\t\t\t175 단추 (disabled) Previous bookmark in this file\n\t\t\t\t176 단추 (disabled) Next bookmark in this file\n\t\t\t\t177 단추 (disabled) Comment selected text (Ctrl+Shift+C)\n\t\t\t\t178 단추 (disabled) Remove comment (Ctrl+Shift+X)\n\t\t\t\t179 단추 (disabled) Unindent (Shift+Tab)\n\t\t\t\t180 단추 (disabled) Indent (Tab)\n\t\t\t181 도구 모음 Macros Manager\n\t\t\t\t182 메뉴 항목 Macros\n\t\t\t183 도구 모음 Layout Manager\n\t\t\t\t184 메뉴 항목 Layouts\n\t\t\t185 도구 모음 Toolbox\n\t\t\t\t186 단추 DataAnalyzer\n\t\t\t\t187 메뉴 항목 Toolbar Options\n\t\t\t188 도구 모음 Net Edit\n\t\t\t\t189 단추 (disabled) Select\n\t\t\t\t190 메뉴 항목 Toolbar Options\n\t\t\t191 도구 모음 Debug\n\t\t\t\t192 단추 Go online (Alt+F6)\n\t\t\t\t193 단추 Change Online Settings\n\t\t\t\t194 메뉴 항목 Online Connection\n\t\t\t\t195 단추 (disabled) Set Online Connection For Current Project\n\t\t\t\t196 단추 (disabled) Download (F6)\n\t\t\t\t197 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)\n\t\t\t\t198 단추 (disabled) Download Module on the Fly\n\t\t\t\t199 단추 (disabled) Save Project on PLC\n\t\t\t\t200 단추 (disabled) Start (F7)\n\t\t\t\t201 단추 (disabled) Reset (F8)\n\t\t\t\t202 단추 Toggle breakpoint (F4)\n\t\t\t\t203 단추 Create condition breakpoint\n\t\t\t\t204 메뉴 항목 Toolbar Options\n\t\t\t205 도구 모음 Build\n\t\t\t\t206 메뉴 항목 Target Architecture\n\t\t\t\t207 단추 Build changes (F9)\n\t\t\t\t208 단추 Rebuild project (Strg+F9)\n\t\t\t\t209 단추 (disabled) Cancel building (Ctrl+Break)\n\t\t\t\t210 단추 Link project\n\t\t\t211 도구 모음 Standard\n\t\t\t\t212 단추 New project (Strg+N)\n\t\t\t\t213 단추 Open a file (Strg+Shift+O)\n\t\t\t\t214 단추 Close active document (Strg+F4)\n\t\t\t\t215 단추 (disabled) Save file (Strg+S)\n\t\t\t\t216 단추 Open project (Strg+O)\n\t\t\t\t217 단추 (disabled) Save project changes (Strg+Shift+S)\n\t\t\t\t218 단추 Close project\n\t\t\t\t219 단추 Print\n\t\t\t\t220 단추 Cut (Strg+X)\n\t\t\t\t221 단추 Copy (Strg+C)\n\t\t\t\t222 단추 Paste (Strg+V)\n\t\t\t\t223 메뉴 항목 Undo (Strg+Z)\n\t\t\t\t224 메뉴 항목 (disabled) Redo (Strg+Y)\n\t\t\t\t225 단추 Navigate Backward (Alt+Left)\n\t\t\t\t226 단추 (disabled) Navigate Forward (Alt +Right)\n\t\t\t227 메뉴 모음 Menu Bar\n\t\t\t\t228 메뉴 항목 FILE\n\t\t\t\t229 메뉴 항목 EDIT\n\t\t\t\t230 메뉴 항목 VIEW\n\t\t\t\t231 메뉴 항목 PROJECT\n\t\t\t\t232 메뉴 항목 BUILD\n\t\t\t\t233 메뉴 항목 DEBUG\n\t\t\t\t234 메뉴 항목 ANALYZE\n\t\t\t\t235 메뉴 항목 TOOLS\n\t\t\t\t236 메뉴 항목 EXTRAS\n\t\t\t\t237 메뉴 항목 WINDOW\n\t\t\t\t238 메뉴 항목 HELP\n\t\t239 창 Splitter ID: 481389416\n\t\t240 창 Splitter ID: 481390928\n\t\t241 Tab Output ID: 409867992\n\t\t\t242 창 ID: 1200\n\t\t\t\t243 창 ID: 1200\n\t\t\t\t\t244 LIST ID: 1201\n\t\t\t\t\t\t245 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t246 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t247 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t\t248 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t249 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t250 목록 항목 (selectable)\n\t\t\t\t\t\t251 목록 항목 (selectable)\n\t\t\t\t\t\t252 목록 항목 (selectable)\n\t\t\t\t\t\t253 목록 항목 (selectable)\n\t\t\t\t\t\t254 목록 항목 (selectable)\n\t\t\t\t\t\t255 목록 항목 (selectable)\n\t\t\t\t\t\t256 목록 항목 (selectable)\n\t\t\t\t\t\t257 목록 항목 (selectable)\n\t\t\t\t\t\t258 목록 항목 (selectable)\n\t\t\t\t\t\t259 목록 항목 (selectable)\n\t\t\t\t\t\t260 목록 항목 (selectable)\n\t\t\t\t\t\t261 목록 항목 (selectable)\n\t\t\t\t\t\t262 목록 항목 (selectable)\n\t\t\t\t\t263 스크롤 막대 ID: 59904\n\t\t\t\t\t\t264 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t265 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t266 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t267 탭 항목 (selectable) Python Script\n\t\t\t268 탭 항목 (selectable) Debugger\n\t\t\t269 탭 항목 (selectable) Output\n\t\t\t270 단추 Close\n\t\t271 창 Splitter ID: 608798568\n\t\t272 Tab Global View ID: 409868448\n\t\t\t273 트리 ID: 105\n\t\t\t\t274 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t275 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t276 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t277 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t278 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t279 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis\n\t\t\t\t\t280 콘솔 트리 (selectable) Types\n\t\t\t\t\t\t281 콘솔 트리 (selectable) External\n\t\t\t\t\t\t282 콘솔 트리 (selectable) _LMC\n\t\t\t\t\t\t\t283 콘솔 트리 (selectable) _LMCAxis\n\t\t\t\t\t\t\t284 콘솔 트리 (selectable) _LMCProfile\n\t\t\t\t\t\t\t285 콘솔 트리 (selectable) old\n\t\t\t\t\t\t\t286 콘솔 트리 (selectable) _LMC_DYN_SWLIMITS\n\t\t\t\t\t\t\t287 콘솔 트리 (selectable) _LMCCONTROLLERTYPE\n\t\t\t\t\t\t\t288 콘솔 트리 (selectable) _POSFLAGS\n\t\t\t\t\t\t\t289 콘솔 트리 (selectable) CNCInternalStruct\n\t\t\t\t\t\t\t290 콘솔 트리 (selectable) CurveTable\n\t\t\t\t\t\t\t291 콘솔 트리 (selectable) MasterStruct\n\t\t\t\t\t\t292 콘솔 트리 (selectable) DS402_Control\n\t\t\t\t\t\t293 콘솔 트리 (selectable) DS402_State\n\t\t\t\t\t\t294 콘솔 트리 (selectable) FeSetup\n\t\t\t\t\t\t295 콘솔 트리 (selectable) IO_State\n\t\t\t\t\t\t296 콘솔 트리 (selectable) pHwBase\n\t\t\t\t\t\t297 콘솔 트리 (selectable) pHwBaseCDIAS\n\t\t\t\t\t\t298 콘솔 트리 (selectable) SafetyConfigStateType\n\t\t\t\t\t\t299 콘솔 트리 (selectable) SafetyDiagState\n\t\t\t\t\t\t300 콘솔 트리 (selectable) t_e_SafetyMemState\n\t\t\t\t\t\t301 콘솔 트리 (selectable) t_e_VaranErrors\n\t\t\t\t\t\t302 콘솔 트리 (selectable) t_s_ModulInfo\n\t\t\t\t\t\t303 콘솔 트리 (selectable) IO_FLAG\n\t\t\t\t\t\t304 콘솔 트리 (selectable) SafetyDiagInfo\n\t\t\t\t\t305 콘솔 트리 (selectable) Variables\n\t\t\t306 탭 항목 (selectable) Lib\n\t\t\t307 탭 항목 (selectable) File\n\t\t\t308 탭 항목 (selectable) Global\n\t\t\t309 단추 Close\n\t\t310 Tab Properties ID: 409871640\n\t\t\t311 창 ID: 121918456\n\t\t\t\t312 TABLE Properties Window ID: 127184512\n\t\t\t\t313 도구 모음 ID: 59392\n\t\t\t\t\t314 단추\n\t\t\t\t\t315 단추\n\t\t\t316 탭 항목 (selectable) Properties\n\t\t\t317 단추 Close\n\t\t318 대화 상자 Find in Tree Secondary Actions: Raise\n\t\t\t319 텍스트 Find:\n\t\t\t320 콤보 상자 Find: ID: 1106\n\t\t\t\t321 편집 Find: ID: 1001\n\t\t\t\t322 단추 열기 ID: DropDown\n\t\t\t323 텍스트 What:\n\t\t\t324 콤보 상자 What: ID: 1112\n\t\t\t\t325 텍스트 What:\n\t\t\t\t326 단추 열기 ID: DropDown\n\t\t\t327 텍스트 Where:\n\t\t\t328 콤보 상자 Where: ID: 1102\n\t\t\t\t329 텍스트 Where:\n\t\t\t\t330 단추 열기 ID: DropDown\n\t\t\t331 확인란 Match whole word only ID: 1108\n\t\t\t332 그룹 Direction\n\t\t\t333 라디오 단추 (selectable) Up ID: 1110\n\t\t\t334 라디오 단추 (selectable) Down ID: 1111\n\t\t\t335 단추 Find Next ID: 1107\n\t\t\t336 단추 Close ID: 2\n\t\t\t337 제목 표시줄\n\t\t\t\t338 메뉴 모음 시스템 ID: MenuBar\n\t\t\t\t\t339 메뉴 항목 시스템\n\t\t\t\t340 단추 닫기\n\nThe focused UI element is 318 대화 상자 Find in Tree Secondary Actions: Raise.","focused":"318 대화 상자 Find in Tree Secondary Actions: Raise","shots":[{"id":"screenshot-0","z":0,"w":1450,"h":763},{"id":"screenshot-1","z":1,"w":1440,"h":753},{"id":"screenshot-2","z":2,"w":448,"h":225}]}
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
>   "code": "await sky.set_value({window:globalThis.targetWindow,element_index:321,value:\"LMCRecorderStore\"}); await sky.click({window:globalThis.targetWindow,element_index:334}); await sky.click({window:globalThis.targetWindow,element_index:335}); nodeRepl.write(\"searched LMCRecorderStore type\");",
>   "title": "Find LASAL recorder class",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> element 321 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var winList=await sky.list_windows(); nodeRepl.write(JSON.stringify(winList.filter(w=>/Lasal|Find in Tree/i.test(w.title||\"\"))));",
>   "title": "Locate LASAL search dialog",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [{"app":"process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe","id":3084742,"title":"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface"},{"app":"process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe","id":1837062,"title":"Lasal Class 2 (02.03.001)"},{"app":"process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe","id":68062,"title":"LasalApiWpfTestApp - Microsoft Visual Studio"}]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:false}); nodeRepl.write(JSON.stringify(globalThis.state.screenshots.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height}))));",
>   "title": "Refresh LASAL search dialog",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [{"id":"screenshot-0","z":0,"w":1450,"h":763},{"id":"screenshot-1","z":1,"w":1440,"h":753},{"id":"screenshot-2","z":2,"w":448,"h":225}]
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
>   "code": "await sky.click({window:globalThis.targetWindow,x:190,y:55,screenshotId:\"screenshot-2\"}); await sky.press_key({window:globalThis.targetWindow,key:\"Control_L+a\"}); await sky.type_text({window:globalThis.targetWindow,text:\"LMCRecorderStore\"}); await sky.click({window:globalThis.targetWindow,x:235,y:195,screenshotId:\"screenshot-2\"}); await sky.click({window:globalThis.targetWindow,x:380,y:55,screenshotId:\"screenshot-2\"}); nodeRepl.write(\"searched recorder class\");",
>   "title": "Search LASAL recorder class",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> searched recorder class
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(JSON.stringify({selected:globalThis.state.accessibility?.selected_elements||[],treeTargets:(globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/LMCRecorder|TCPMotion/.test(x)).slice(-80),focused:globalThis.state.accessibility?.focused_element||null}));",
>   "title": "Confirm LASAL recorder class search",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {"selected":[],"treeTargets":["Window: \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface\", App: Lasal2.exe.","\t0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise","\t\t\t31 창 TCPMotionInterface Secondary Actions: Raise ID: 65280","\t\t\t\t\t33 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000"],"focused":"2 대화 상자 Lasal Class 2 Secondary Actions: Raise"}
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
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:\"Return\"}); globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").split(\"\\n\").slice(-80).join(\"\\n\"));",
>   "title": "Dismiss LASAL search notice",
>   "timeout_ms": 30000
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
>   "code": "nodeRepl.write(JSON.stringify(globalThis.state.screenshots.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height}))));",
>   "title": "Inspect LASAL notice window",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [{"id":"screenshot-0","z":0,"w":1450,"h":763},{"id":"screenshot-1","z":1,"w":1440,"h":753},{"id":"screenshot-2","z":2,"w":448,"h":225},{"id":"screenshot-3","z":3,"w":178,"h":152}]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window:globalThis.targetWindow,x:126,y:129,screenshotId:\"screenshot-3\"}); nodeRepl.write(\"dismissed search notice\");",
>   "title": "Dismiss LASAL search notice",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> dismissed search notice
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(JSON.stringify({tree:(globalThis.state.accessibility?.tree||\"\").split(\"\\n\").slice(-40),shots:globalThis.state.screenshots.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height}))}));",
>   "title": "Inspect LASAL search options",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {"tree":["\t\t\t\t\t\t326 콘솔 트리 (selectable) IO_FLAG","\t\t\t\t\t\t327 콘솔 트리 (selectable) SafetyDiagInfo","\t\t\t\t\t328 콘솔 트리 (selectable) Variables","\t\t\t329 탭 항목 (selectable) Lib","\t\t\t330 탭 항목 (selectable) File","\t\t\t331 탭 항목 (selectable) Global","\t\t\t332 단추 Close","\t\t333 Tab Properties ID: 409871640","\t\t\t334 창 ID: 121918456","\t\t\t\t335 TABLE Properties Window ID: 127184512","\t\t\t\t336 도구 모음 ID: 59392","\t\t\t\t\t337 단추","\t\t\t\t\t338 단추","\t\t\t339 탭 항목 (selectable) Properties","\t\t\t340 단추 Close","\t\t341 대화 상자 Find in Tree Secondary Actions: Raise","\t\t\t342 텍스트 Find:","\t\t\t343 콤보 상자 Find: ID: 1106","\t\t\t\t344 편집 Find: ID: 1001","\t\t\t\t345 단추 열기 ID: DropDown","\t\t\t346 텍스트 What:","\t\t\t347 콤보 상자 What: ID: 1112","\t\t\t\t348 텍스트 What:","\t\t\t\t349 단추 열기 ID: DropDown","\t\t\t350 텍스트 Where:","\t\t\t351 콤보 상자 Where: ID: 1102","\t\t\t\t352 텍스트 Where:","\t\t\t\t353 단추 열기 ID: DropDown","\t\t\t354 확인란 Match whole word only ID: 1108","\t\t\t355 그룹 Direction","\t\t\t356 라디오 단추 (selectable) Up ID: 1110","\t\t\t357 라디오 단추 (selectable) Down ID: 1111","\t\t\t358 단추 Find Next ID: 1107","\t\t\t359 단추 Close ID: 2","\t\t\t360 제목 표시줄","\t\t\t\t361 메뉴 모음 시스템 ID: MenuBar","\t\t\t\t\t362 메뉴 항목 시스템","\t\t\t\t363 단추 닫기","","The focused UI element is 1 대화 상자 Find in Tree Secondary Actions: Raise."],"shots":[{"id":"screenshot-0","z":0,"w":1450,"h":763},{"id":"screenshot-1","z":1,"w":1440,"h":753},{"id":"screenshot-2","z":2,"w":448,"h":225}]}
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
>   "code": "await sky.click({window:globalThis.targetWindow,x:307,y:87,screenshotId:\"screenshot-2\"}); globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").split(\"\\n\").slice(-80).join(\"\\n\")); for (const s of globalThis.state.screenshots.filter(s=>s.zIndex===Math.max(...globalThis.state.screenshots.map(x=>x.zIndex)))) await nodeRepl.emitImage(s.url);",
>   "title": "Inspect LASAL search categories",
>   "timeout_ms": 30000
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

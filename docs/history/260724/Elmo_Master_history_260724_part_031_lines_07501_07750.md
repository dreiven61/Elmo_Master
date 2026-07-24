> 							118 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						119 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							120 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							121 위치 조정 위치 ID: ScrollbarThumb
> 							122 단추 페이지 오른쪽으로 ID: DownPageButton
> 							123 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						124 위치 조정 (disabled)
> 			125 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65281
> 				126 창 ID: 59648
> 					127 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 						128 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							129 단추 위쪽 스크롤 화살표 ID: UpButton
> 							130 위치 조정 위치 ID: ScrollbarThumb
> 							131 단추 페이지 아래로 ID: DownPageButton
> 							132 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						133 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							134 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							135 위치 조정 위치 ID: ScrollbarThumb
> 							136 단추 페이지 오른쪽으로 ID: DownPageButton
> 							137 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						138 위치 조정 (disabled)
> 			139 창 Motion_Network Secondary Actions: Raise ID: 65280
> 				140 창 ID: 59648
> 					141 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						142 단추 위쪽 스크롤 화살표 ID: UpButton
> 						143 위치 조정 위치 ID: ScrollbarThumb
> 						144 단추 페이지 아래로 ID: DownPageButton
> 						145 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					146 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						147 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						148 위치 조정 위치 ID: ScrollbarThumb
> 						149 단추 페이지 오른쪽으로 ID: DownPageButton
> 						150 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					151 위치 조정 (disabled)
> 		152 상태 표시줄 ID: 59393
> 			153 텍스트
> 			154 텍스트
> 			155 텍스트
> 			156 텍스트 Ln 1 Col 1
> 			157 텍스트
> 			158 텍스트 Offline
> 			159 텍스트
> 			160 텍스트 NUM
> 			161 텍스트
> 		162 창 xtpBarTop ID: 59419
> 			163 도구 모음 Edit
> 				164 단추 Toggle bookmark
> 				165 단추 (disabled) Previous bookmark
> 				166 단추 (disabled) Next bookmark
> 				167 단추 (disabled) Delete all bookmarks
> 				168 단추 (disabled) Previous bookmark in this file
> 				169 단추 (disabled) Next bookmark in this file
> 				170 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				171 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				172 단추 (disabled) Unindent (Shift+Tab)
> 				173 단추 (disabled) Indent (Tab)
> 			174 도구 모음 Macros Manager
> 				175 메뉴 항목 Macros
> 			176 도구 모음 Layout Manager
> 				177 메뉴 항목 Layouts
> 			178 도구 모음 Toolbox
> 				179 단추 DataAnalyzer
> 				180 단추 Interpreter
> 				181 단추 DiasDrive
> 				182 단추 PLC Diagnosis
> 				183 단추 Hardware Editor
> 				184 단추 Graphical Hardware Editor
> 				185 단추 Connection Manager
> 				186 단추 Task Configuration
> 			187 도구 모음 Net Edit
> 				188 단추 (disabled) Select
> 				189 단추 (disabled) Move view
> 				190 단추 (disabled) Insert comment
> 				191 단추 (disabled) Zoom(+/-)
> 				192 단추 (disabled) Zoom to all
> 				193 단추 (disabled) Zoom selection
> 			194 도구 모음 Debug
> 				195 단추 Go online (Alt+F6)
> 				196 단추 Change Online Settings
> 				197 메뉴 항목 Online Connection
> 				198 단추 (disabled) Set Online Connection For Current Project
> 				199 단추 (disabled) Download (F6)
> 				200 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				201 단추 (disabled) Download Module on the Fly
> 				202 단추 (disabled) Save Project on PLC
> 				203 단추 (disabled) Start (F7)
> 				204 단추 (disabled) Reset (F8)
> 				205 단추 Toggle breakpoint (F4)
> 				206 단추 Create condition breakpoint
> 				207 단추 Remove all breakpoint(s) (Shift+F4)
> 				208 단추 AWL trigger on/off
> 				209 단추 (disabled) Fix AWL trigger to current instruction
> 				210 단추 Activate/Deactivate Live View
> 				211 단추 Activate/Deactivate Auto Watch
> 				212 단추 (disabled) Goto instruction pointer
> 				213 단추 (disabled) Step into (F5)
> 				214 단추 (disabled) Step over (Alt+F5)
> 				215 단추 (disabled) Step out (Shift+F5)
> 				216 단추 (disabled) Set instruction pointer
> 			217 도구 모음 Build
> 				218 메뉴 항목 Target Architecture
> 				219 단추 Build changes (F9)
> 				220 단추 Rebuild project (Strg+F9)
> 				221 단추 (disabled) Cancel building (Ctrl+Break)
> 				222 단추 Link project
> 			223 도구 모음 Standard
> 				224 단추 New project (Strg+N)
> 				225 단추 Open a file (Strg+Shift+O)
> 				226 단추 Close active document (Strg+F4)
> 				227 단추 (disabled) Save file (Strg+S)
> 				228 단추 Open project (Strg+O)
> 				229 단추 (disabled) Save project changes (Strg+Shift+S)
> 				230 단추 Close project
> 				231 단추 Print
> 				232 단추 Cut (Strg+X)
> 				233 단추 Copy (Strg+C)
> 				234 단추 (disabled) Paste (Strg+V)
> 				235 메뉴 항목 (disabled) Undo (Strg+Z)
> 				236 메뉴 항목 (disabled) Redo (Strg+Y)
> 				237 단추 Navigate Backward (Alt+Left)
> 				238 단추 (disabled) Navigate Forward (Alt +Right)
> 			239 메뉴 모음 Menu Bar
> 				240 메뉴 항목 FILE
> 				241 메뉴 항목 EDIT
> 				242 메뉴 항목 VIEW
> 				243 메뉴 항목 PROJECT
> 				244 메뉴 항목 BUILD
> 				245 메뉴 항목 DEBUG
> 				246 메뉴 항목 ANALYZE
> 				247 메뉴 항목 TOOLS
> 				248 메뉴 항목 EXTRAS
> 				249 메뉴 항목 WINDOW
> 				250 메뉴 항목 HELP
> 		251 창 Splitter ID: 254458464
> 		252 창 Splitter ID: 254457624
> 		253 Tab Output ID: 251705512
> 			254 창 ID: 1200
> 				255 창 ID: 1200
> 					256 LIST ID: 1201
> 						257 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							258 단추 위쪽 스크롤 화살표 ID: UpButton
> 							259 단추 페이지 위로 ID: UpPageButton
> 							260 위치 조정 위치 ID: ScrollbarThumb
> 							261 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						262 목록 항목 (selectable)
> 						263 목록 항목 (selectable)
> 						264 목록 항목 (selectable)
> 						265 목록 항목 (selectable)
> 						266 목록 항목 (selectable)
> 						267 목록 항목 (selectable)
> 						268 목록 항목 (selectable)
> 						269 목록 항목 (selectable)
> 					270 스크롤 막대 ID: 59904
> 						271 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						272 위치 조정 위치 ID: ScrollbarThumb
> 						273 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			274 탭 항목 (selectable) Python Script
> 			275 탭 항목 (selectable) Debugger
> 			276 탭 항목 (selectable) Output
> 			277 단추 Close
> 		278 창 Splitter ID: 254458296
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect LASAL Class View",
>   "code": "state = await sky.get_window_state({window:targetWindow, include_screenshot:false, include_text:true}); targetWindow=state.window; var t=(state.accessibility?.tree||'').split('\\n'); nodeRepl.write(t.filter(x=>/LMCSdoExecutor|LMCDiagnosticsService|Class View|Libraries|Project|EtherCAT_Network/.test(x)).slice(-120).join('\\n'))"
> }
> ```
>
> ```text
> 			30 창 EtherCAT_Network Secondary Actions: Raise ID: 65291
> 			97 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 					99 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="status_word"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000
> 				198 단추 (disabled) Set Online Connection For Current Project
> 				202 단추 (disabled) Save Project on PLC
> 		279 Tab Class View ID: 251698672
> 					306 콘솔 트리 (selectable) LMCDiagnosticsService
> 					309 콘솔 트리 (selectable) LMCSdoExecutor
> ```
>
> <details><summary>Called 2 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect computer controls",
>   "code": "nodeRepl.write(Object.keys(sky).sort())"
> }
> ```
>
> ```text
> [
>   'activate_window',
>   'click',
>   'drag',
>   'get_window',
>   'get_window_state',
>   'launch_app',
>   'list_apps',
>   'list_windows',
>   'perform_secondary_action',
>   'press_key',
>   'scroll',
>   'set_value',
>   'transport',
>   'type_text'
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect computer action API",
>   "code": "nodeRepl.write({click:sky.click.toString(), secondary:sky.perform_secondary_action.toString(), setValue:sky.set_value.toString()})"
> }
> ```
>
> ```text
> {
>   click: '(...args) => {\r\n' +
>     '        globals.nodeRepl?.setResponseMeta({\r\n' +
>     '          [TOOL_SURFACE_META_KEY]: {\r\n' +
>     '            kind: "computerUse",\r\n' +
>     '            app: getComputerUseAppReference(args[0]),\r\n' +
>     '          },\r\n' +
>     '        });\r\n' +
>     '        return Reflect.apply(value, target, args);\r\n' +
>     '      }',
>   secondary: '(...args) => {\r\n' +
>     '        globals.nodeRepl?.setResponseMeta({\r\n' +
>     '          [TOOL_SURFACE_META_KEY]: {\r\n' +
>     '            kind: "computerUse",\r\n' +
>     '            app: getComputerUseAppReference(args[0]),\r\n' +
>     '          },\r\n' +
>     '        });\r\n' +
>     '        return Reflect.apply(value, target, args);\r\n' +
>     '      }',

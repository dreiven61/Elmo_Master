>   "code": "nodeRepl.write(await globalThis.sky.perform_secondary_action({window:globalThis.targetWindow,element:3142}))"
> }
> ```
>
> ```text
> action is required
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 제어 API 세부 확인",
>   "code": "nodeRepl.write({targetKeys:Object.keys(globalThis.sky.target||{}),targetProto:Object.getOwnPropertyNames(Object.getPrototypeOf(globalThis.sky.target||{})),clickFn:String(globalThis.sky.click),secondaryFn:String(globalThis.sky.perform_secondary_action)})"
> }
> ```
>
> ```text
> {
>   targetKeys: [
>     '0', '1', '2',
>     '3', '4', '5',
>     '6'
>   ],
>   targetProto: [
>     'length',            'constructor', 'anchor',
>     'at',                'big',         'blink',
>     'bold',              'charAt',      'charCodeAt',
>     'codePointAt',       'concat',      'endsWith',
>     'fontcolor',         'fontsize',    'fixed',
>     'includes',          'indexOf',     'isWellFormed',
>     'italics',           'lastIndexOf', 'link',
>     'localeCompare',     'match',       'matchAll',
>     'normalize',         'padEnd',      'padStart',
>     'repeat',            'replace',     'replaceAll',
>     'search',            'slice',       'small',
>     'split',             'strike',      'sub',
>     'substr',            'substring',   'sup',
>     'startsWith',        'toString',    'toWellFormed',
>     'trim',              'trimStart',   'trimLeft',
>     'trimEnd',           'trimRight',   'toLocaleLowerCase',
>     'toLocaleUpperCase', 'toLowerCase', 'toUpperCase',
>     'valueOf'
>   ],
>   clickFn: '(...args) => {\r\n' +
>     '        const app = getComputerUseAppReference(args[0]);\r\n' +
>     '        globals.nodeRepl?.setResponseMeta({\r\n' +
>     '          [TOOL_SURFACE_META_KEY]: {\r\n' +
>     '            kind: "computerUse",\r\n' +
>     '            app,\r\n' +
>     '          },\r\n' +
>     '          ...(isChromeComputerUseAppReference(app)\r\n' +
>     '            ? { [CHROME_COMPUTER_USE_META_KEY]: true }\r\n' +
>     '            : {}),\r\n' +
>     '        });\r\n' +
>     '        return Reflect.apply(value, target, args);\r\n' +
>     '      }',
>   secondaryFn: '(...args) => {\r\n' +
>     '        const app = getComputerUseAppReference(args[0]);\r\n' +
>     '        globals.nodeRepl?.setResponseMeta({\r\n' +
>     '          [TOOL_SURFACE_META_KEY]: {\r\n' +
>     '            kind: "computerUse",\r\n' +
>     '            app,\r\n' +
>     '          },\r\n' +
>     '          ...(isChromeComputerUseAppReference(app)\r\n' +
>     '            ? { [CHROME_COMPUTER_USE_META_KEY]: true }\r\n' +
>     '            : {}),\r\n' +
>     '        });\r\n' +
>     '        return Reflect.apply(value, target, args);\r\n' +
>     '      }'
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
>   "title": "InputLatch 클래스 메뉴 열기",
>   "code": "{const observation=globalThis.state;if(!observation?.accessibility)throw new Error('reobserve');globalThis.state=null;await globalThis.sky.click({window:observation.window,element_index:3142,mouse_button:'right'});globalThis.state=await globalThis.sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(String(globalThis.state.accessibility?.tree||''));}"
> }
> ```
>
> ```text
> element 3142 has no cached bounds
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 클래스 위치 확인",
>   "code": "var latestShot = globalThis.state.screenshots.reduce((a,b)=>!a||b.zIndex>a.zIndex?b:a,null); await nodeRepl.emitImage(latestShot.url); nodeRepl.write({id:latestShot.id,zIndex:latestShot.zIndex,width:latestShot.width,height:latestShot.height,originX:latestShot.originX,originY:latestShot.originY})"
> }
> ```
>
> ```text
> Cannot read properties of null (reading 'screenshots')
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
>   "title": "LASAL 클래스 위치 재확인",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;var shot2=globalThis.state.screenshots.reduce((a,b)=>!a||b.zIndex>a.zIndex?b:a,null);await nodeRepl.emitImage(shot2.url);nodeRepl.write({id:shot2.id,zIndex:shot2.zIndex,width:shot2.width,height:shot2.height,originX:shot2.originX,originY:shot2.originY})"
> }
> ```
>
> ```text
> {
>   id: 'screenshot-1',
>   zIndex: 1,
>   width: 960,
>   height: 502,
>   originX: -1890,
>   originY: 27
> }
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
>   "title": "클래스 목록 아래로 이동",
>   "code": "{const observation=globalThis.state;var s=observation.screenshots.reduce((a,b)=>!a||b.zIndex>a.zIndex?b:a,null);globalThis.state=null;await globalThis.sky.scroll({window:observation.window,screenshotId:s.id,x:840,y:240,scrollX:0,scrollY:735});globalThis.state=await globalThis.sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;var s2=globalThis.state.screenshots.reduce((a,b)=>!a||b.zIndex>a.zIndex?b:a,null);await nodeRepl.emitImage(s2.url);}"
> }
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
>   "title": "InputLatch 클래스 메뉴 열기",
>   "code": "{const observation=globalThis.state;var s=observation.screenshots.reduce((a,b)=>!a||b.zIndex>a.zIndex?b:a,null);globalThis.state=null;await globalThis.sky.click({window:observation.window,screenshotId:s.id,x:815,y:177,mouse_button:'right'});globalThis.state=await globalThis.sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(String(globalThis.state.accessibility?.tree||''));}"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			3152 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65280
> 				3153 창 ID: 59648
> 					3154 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000
> 						3155 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							3156 단추 위쪽 스크롤 화살표 ID: UpButton
> 							3157 단추 페이지 위로 ID: UpPageButton
> 							3158 위치 조정 위치 ID: ScrollbarThumb
> 							3159 단추 페이지 아래로 ID: DownPageButton
> 							3160 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						3161 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							3162 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							3163 위치 조정 위치 ID: ScrollbarThumb
> 							3164 단추 페이지 오른쪽으로 ID: DownPageButton
> 							3165 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						3166 위치 조정 (disabled)
> 			3167 창 LMCControlCommandService Secondary Actions: Raise ID: 65283
> 				3168 창 ID: 59648
> 					3169 창 #define LMC_ADMIN_AXIS_HOME_ENABLED FALSE #define LMC_AXIS_STATUS_STANDSTILL 0x02000000 #define LMC_HOME_RECORD_EMPTY 0 #define LMC_HOME_RECORD_RUNNING 1 #define LMC_HOME_RECORD_SUCCEEDED 2 #define LMC_HOME_RECORD_FAILED 3 #define LMC_HOME_RECORD_ABORTED 4 #define LMC_HOME_RECORD_QUARANTINED 5 #define LMC_HOME_ENGINE_IDLE 0 #define LMC_HOME_ENGINE_WAIT_RT 1 #define LMC_HOME_ENGINE_TERMINAL 2 #define LMC_HOME_RECORD_MAGIC 0x4C4D4348 #define LMC_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_GLOBAL_SLOTS 28 #define LMC_OWNER_AXIS_STRIDE 36 #define LMC_OWNER_AXIS_COUNT 9 #define LMC_OWNER_TABLE_MAGIC 0x4C4D434F #define LMC_OWNER_AXIS_RECORD_MAGIC 0x4F574E00 #define LMC_OWNER_STATE_IDLE 0 #define LMC_OWNER_STATE_RESERVED 1 #define LMC_OWNER_STATE_DIRECT_ACTIVE 2 #define LMC_OWNER_STATE_GROUP_LEASE 3 #define LMC_OWNER_STATE_GROUP_ACTIVE 4 #define LMC_OWNER_STATE_LMC_HOME_ACTIVE 5 #define LMC_OWNER_STATE_DS402_HOME_ACTIVE 6 #define LMC_OWNER_STATE_TW20_QUEUED 7 #define LMC_OWNER_STATE_TW20_RUNNING 8 #define LMC_OWNER_STATE_TW20_DRAINING 9 #define LMC_OWNER_STATE_SAFETY_PREEMPTING 10 #define LMC_OWNER_STATE_QUARANTINED 11 #define LMC_OWNER_KIND_DIRECT 1 #define LMC_OWNER_KIND_GROUP 2 #define LMC_OWNER_KIND_LMC_HOME 3 #define LMC_OWNER_KIND_DS402_HOME 4 #define LMC_OWNER_KIND_ENCODER 5 #define LMC_OWNER_RESOURCE_AXIS 1 #define LMC_OWNER_RESOURCE_LMC_HOME_ENGINE 2 #define LMC_OWNER_RESOURCE_DS402_HOME_ENGINE 3 #define LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE 4 #define LMC_OWNER_ADMISSION_ORDINARY 1 #define LMC_OWNER_ADMISSION_SAFETY 2 #define LMC_OWNER_ADMISSION_READ 3 #define LMC_OWNER_ADMISSION_LIFECYCLE 4 #define LMC_OWNER_REPORT_DISPATCH 1 #define LMC_OWNER_REPORT_TERMINAL_SUCCESS 2 #define LMC_OWNER_REPORT_TERMINAL_SAFE_FAILURE 3 #define LMC_OWNER_REPORT_QUARANTINE 4 #define LMC_OWNER_REPORT_SAFETY_PREEMPT 5 #define LMC_OWNER_STARTUP_PROOF_BOOT_ID 0x00000001 #define LMC_OWNER_STARTUP_PROOF_REQUIRED 0x0000000F FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; AdmissionToken : UDINT; OwnerGeneration : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; // Slots 16..21 are a synchronous call context, not retained ownership. OwnershipState[16]$UDINT := CallerSessionEpoch; OwnershipState[17]$UDINT := RequestSequence; OwnershipState[18]$UDINT := AdmissionToken; OwnershipState[19]$UDINT := OwnerGeneration; OwnershipState[20] := TO_DINT(CommandId); OwnershipState[21] := TO_DINT(Reference); case CommandId of 0x103C, 0x1042, 0x202B: ResponseSize := HandleRegistryCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x2023, 0x2024, 0x2022, 0x2028, 0x202E, 0x209F, 0x20A0, 0x20A2: ResponseSize := HandleAxisCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x20D2, 0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085, 0x20A4, 0x2045, 0x2051, 0x20E7: ResponseSize := HandleGroupCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x7D00, 0x7D10, 0x7D12, 0x7D13, 0x7D18, 0x7D19, 0x7D20, 0x7D22: ResponseSize := HandleAdminCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); else ResponseSize := -1; end_ ID: 10000
> 						3170 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							3171 단추 위쪽 스크롤 화살표 ID: UpButton
> 							3172 단추 페이지 위로 ID: UpPageButton
> 							3173 위치 조정 위치 ID: ScrollbarThumb
> 							3174 단추 페이지 아래로 ID: DownPageButton
> 							3175 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						3176 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							3177 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							3178 위치 조정 위치 ID: ScrollbarThumb
> 							3179 단추 페이지 오른쪽으로 ID: DownPageButton
> 							3180 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						3181 위치 조정 (disabled)
> 			3182 창 Motion_Network Secondary Actions: Raise ID: 65282
> 				3183 창 ID: 59648
> 					3184 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						3185 단추 위쪽 스크롤 화살표 ID: UpButton
> 						3186 위치 조정 위치 ID: ScrollbarThumb
> 						3187 단추 페이지 아래로 ID: DownPageButton
> 						3188 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					3189 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						3190 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						3191 위치 조정 위치 ID: ScrollbarThumb
> 						3192 단추 페이지 오른쪽으로 ID: DownPageButton
> 						3193 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					3194 위치 조정 (disabled)
> 			3195 창 Comm_Network Secondary Actions: Raise ID: 65281
> 				3196 창 ID: 59648
> 					3197 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						3198 단추 위쪽 스크롤 화살표 ID: UpButton
> 						3199 위치 조정 위치 ID: ScrollbarThumb
> 						3200 단추 페이지 아래로 ID: DownPageButton
> 						3201 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					3202 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						3203 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						3204 위치 조정 위치 ID: ScrollbarThumb
> 						3205 단추 페이지 오른쪽으로 ID: DownPageButton
> 						3206 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					3207 위치 조정 (disabled)
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트 LMCDiagnosticsService::HandleDiagnosticsBulkRequest
> 			5 텍스트
> 			6 텍스트 Ln 5241 Col 1
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				46834 단추 Toggle bookmark
> 				46835 단추 (disabled) Previous bookmark
> 				46836 단추 (disabled) Next bookmark
> 				46837 단추 (disabled) Delete all bookmarks
> 				46838 단추 (disabled) Previous bookmark in this file
> 				46839 단추 (disabled) Next bookmark in this file
> 				46840 단추 Comment selected text (Ctrl+Shift+C)
> 				46841 단추 Remove comment (Ctrl+Shift+X)
> 				46842 단추 Unindent (Shift+Tab)

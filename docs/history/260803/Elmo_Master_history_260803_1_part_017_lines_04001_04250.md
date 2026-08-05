> 				17225 단추 (disabled) Navigate Backward (Alt+Left)
> 				17226 단추 (disabled) Navigate Forward (Alt +Right)
> 			125 메뉴 모음 Menu Bar
> 				17227 메뉴 항목 FILE
> 				17228 메뉴 항목 EDIT
> 				17229 메뉴 항목 NETEDIT
> 				17230 메뉴 항목 VIEW
> 				17231 메뉴 항목 PROJECT
> 				17232 메뉴 항목 BUILD
> 				17233 메뉴 항목 DEBUG
> 				17234 메뉴 항목 ANALYZE
> 				17235 메뉴 항목 TOOLS
> 				17236 메뉴 항목 EXTRAS
> 				17237 메뉴 항목 WINDOW
> 				17238 메뉴 항목 HELP
> 		138 창 Splitter ID: 314043376
> 		139 창 Splitter ID: 314045392
> 		140 Tab Debugger ID: 424891576
> 			141 창 ID: 1200
> 				142 창 ID: 1200
> 					143 LIST ID: 315686088
> 						144 머리글 헤더 컨트롤 ID: Header
> 							145 머리글 항목 Name ID: HeaderItem 0
> 							146 머리글 항목 Value ID: HeaderItem 1
> 							147 머리글 항목 SetValue ID: HeaderItem 2
> 							148 머리글 항목 Format ID: HeaderItem 3
> 							149 머리글 항목 Type ID: HeaderItem 4
> 							150 머리글 항목 Address ID: HeaderItem 5
> 						151 목록 항목 (selectable)
> 							152 텍스트
> 							153 텍스트
> 							154 텍스트
> 							155 텍스트
> 							156 텍스트
> 							157 텍스트
> 						158 목록 항목 (selectable)
> 							159 텍스트
> 							160 텍스트
> 							161 텍스트
> 							162 텍스트
> 							163 텍스트
> 							164 텍스트
> 						165 목록 항목 (selectable)
> 							166 텍스트
> 							167 텍스트
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCSdoExecutor 선택 상태 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var lines = state.accessibility.tree.split('\\n').filter(x=>/LMCSdoExecutor|Class View|focused UI|창 .*\\[RO\\]/.test(x)); nodeRepl.write(lines.join('\\n'))"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor [RO]", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor [RO] Secondary Actions: Raise
> 			17264 창 LMCSdoExecutor [RO] Secondary Actions: Raise ID: 65282
> 					17266 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
> 			2 창 Motion_Network [RO] Secondary Actions: Raise ID: 65281
> 			16 창 TCPMotionInterface [RO] Secondary Actions: Raise ID: 65280
> 		195 Tab Class View ID: 424886104
> 					16290 콘솔 트리 (selectable) LMCSdoExecutor
> The focused UI element is 17266 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCSdoExecutor 편집 메뉴 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var execMenuMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 콘솔 트리 \\(selectable\\) LMCSdoExecutor\\n/); if(!execMenuMatch) throw new Error('LMCSdoExecutor not found'); await sky.click({window:state.window,element_index:Number(execMenuMatch[1]),mouse_button:'right'}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.slice(-4000))"
> }
> ```
>
> ```text
> name
> 				224 도구 모음 ID: 59392
> 					225 단추
> 					226 단추
> 			19039 탭 항목 (selectable) Properties
> 			228 단추 Close
>
> The focused UI element is 17266 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 편집 메뉴 창 확인",
>   "code": "var winList = await sky.list_windows(); nodeRepl.write(winList.filter(w=>w.app?.id===targetApp?.id || /Lasal|Menu|Popup/i.test(w.title||'')).map(w=>({id:w.id,title:w.title,app:w.app?.name||w.app?.id})))"
> }
> ```
>
> ```text
> [
>   {
>     id: 1120864,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor [RO]',
>     app: undefined
>   },
>   {
>     id: 854086,
>     title: 'LasalApiWpfTestApp - Microsoft Visual Studio',
>     app: undefined
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
>   "title": "LASAL 편집 메뉴 화면 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:false}); nodeRepl.write({window:state.window.title,screenshots:state.screenshots.map(s=>({id:s.id,width:s.width,height:s.height}))})"
> }
> ```
>
> ```text
> {
>   window: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor [RO]',
>   screenshots: [
>     { id: 'screenshot-0', width: 1920, height: 1040 },
>     { id: 'screenshot-1', width: 209, height: 528 },
>     { id: 'screenshot-2', width: 209, height: 4 },
>     { id: 'screenshot-3', width: 4, height: 524 }
>   ]
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
>   "title": "LASAL 클래스 메뉴 항목 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>/Edit Source|New Std|Constructor|New Method|Context Menu|메뉴/.test(x)).slice(-80).join('\\n'))"
> }
> ```
>
> ```text
> 		19042 메뉴
> 			22102 메뉴 항목 Edit Source Enter
> 			22103 메뉴 항목 Edit Defines
> 			22104 메뉴 항목 (disabled) Delete Del
> 			22105 메뉴 항목 (disabled) Duplicate Class
> 			22106 메뉴 항목 (disabled) Derive Class
> 			22107 메뉴 항목 Find Reference
> 			22108 메뉴 항목 Print Class
> 			22109 메뉴 항목 Export Class(es)
> 			22110 메뉴 항목 Reload Class
> 			22111 메뉴 항목 (disabled) Re\Import MatLab Model
> 			22112 메뉴 항목 Protection Setting
> 			22113 메뉴 항목 (disabled) New Server
> 			22114 메뉴 항목 (disabled) New Client
> 			22115 메뉴 항목 (disabled) New Method
> 			22116 메뉴 항목 New Std. Methods
> 			22117 메뉴 항목 (disabled) New Variable
> 			22118 메뉴 항목 (disabled) New Type
> 			22119 메뉴 항목 (disabled) New Table
> 			22120 메뉴 항목 (disabled) Delete Network
> 			22121 메뉴 항목 (disabled) Add File...
> 			22122 메뉴 항목 (disabled) Open Subfolders
> 			22123 메뉴 항목 Paste
> 			22124 메뉴 항목 Copy
> 							22127 메뉴 항목 Context
> 							22130 메뉴 항목 Context
> 				22141 메뉴 항목 Macros
> 				22142 메뉴 항목 Layouts
> 				22159 메뉴 항목 Online Connection
> 				22179 메뉴 항목 (disabled) Target Architecture
> 				22195 메뉴 항목 (disabled) Undo (Strg+Z)
> 				22196 메뉴 항목 (disabled) Redo (Strg+Y)
> 			125 메뉴 모음 Menu Bar
> 				22199 메뉴 항목 FILE
> 				22200 메뉴 항목 EDIT
> 				22201 메뉴 항목 VIEW
> 				22202 메뉴 항목 PROJECT
> 				22203 메뉴 항목 BUILD
> 				22204 메뉴 항목 DEBUG
> 				22205 메뉴 항목 ANALYZE
> 				22206 메뉴 항목 TOOLS
> 				22207 메뉴 항목 EXTRAS
> 				22208 메뉴 항목 WINDOW
> 				22209 메뉴 항목 HELP
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 표준 메서드 메뉴 열기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var stdMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 메뉴 항목 New Std\\. Methods\\n/); if(!stdMatch) throw new Error('New Std. Methods not found'); await sky.click({window:state.window,element_index:Number(stdMatch[1])}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>/Std\\.|Constructor|New |메뉴 항목/.test(x)).slice(-80).join('\\n'))"
> }
> ```
>
> ```text
> 			22900 메뉴 항목 Edit Source Enter
> 			22901 메뉴 항목 Edit Defines
> 			22902 메뉴 항목 (disabled) Delete Del
> 			22903 메뉴 항목 (disabled) Duplicate Class
> 			22904 메뉴 항목 (disabled) Derive Class
> 			22905 메뉴 항목 Find Reference
> 			22906 메뉴 항목 Print Class
> 			22907 메뉴 항목 Export Class(es)
> 			22908 메뉴 항목 Reload Class
> 			22909 메뉴 항목 (disabled) Re\Import MatLab Model
> 			22910 메뉴 항목 Protection Setting
> 			22911 메뉴 항목 (disabled) New Server
> 			22912 메뉴 항목 (disabled) New Client
> 			22913 메뉴 항목 (disabled) New Method
> 			22914 메뉴 항목 New Std. Methods
> 			22915 메뉴 항목 (disabled) New Variable
> 			22916 메뉴 항목 (disabled) New Type
> 			22917 메뉴 항목 (disabled) New Table
> 			22918 메뉴 항목 (disabled) Delete Network
> 			22919 메뉴 항목 (disabled) Add File...
> 			22920 메뉴 항목 (disabled) Open Subfolders
> 			22921 메뉴 항목 Paste
> 			22922 메뉴 항목 Copy
> 							22925 메뉴 항목 Context
> 							22928 메뉴 항목 Context
> 				22939 메뉴 항목 Macros
> 				22940 메뉴 항목 Layouts
> 				22957 메뉴 항목 Online Connection
> 				22977 메뉴 항목 (disabled) Target Architecture
> 				22982 단추 New project (Strg+N)
> 				22993 메뉴 항목 (disabled) Undo (Strg+Z)

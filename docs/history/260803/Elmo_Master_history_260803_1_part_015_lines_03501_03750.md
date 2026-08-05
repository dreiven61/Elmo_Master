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
>   "title": "LASAL Class 보기 열기",
>   "code": "await sky.click({window:targetWindow,element_index:1684}); state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.slice(-5000))"
> }
> ```
>
> ```text
> element 1684 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 창 상태 새로 읽기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.slice(-2500))"
> }
> ```
>
> ```text
> 			14693 메뉴 항목 TOOLS
> 				14694 메뉴 항목 EXTRAS
> 				14695 메뉴 항목 WINDOW
> 				14696 메뉴 항목 HELP
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
> 							168 텍스트
> 							169 텍스트
> 							170 텍스트
> 							171 텍스트
> 						172 목록 항목 (selectable)
> 							173 텍스트
> 							174 텍스트
> 							175 텍스트
> 							176 텍스트
> 							177 텍스트
> 							178 텍스트
> 						179 목록 항목 (selectable)
> 							180 텍스트
> 							181 텍스트
> 							182 텍스트
> 							183 텍스트
> 							184 텍스트
> 							185 텍스트
> 					186 스크롤 막대 (disabled) ID: 59904
> 						187 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						188 위치 조정 위치 ID: ScrollbarThumb
> 						189 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			14697 탭 항목 (selectable) Python Script
> 			14698 탭 항목 (selectable) Debugger
> 			14699 탭 항목 (selectable) Output
> 			193 단추 Close
> 		194 창 Splitter ID: 314042368
> 		195 Tab Network View ID: 424886104
> 			196 트리 ID: 104
> 				197 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					198 콘솔 트리 (selectable) Comm_Network
> 					199 콘솔 트리 (selectable) EtherCAT_Network
> 					200 콘솔 트리 (selectable) HW_Network
> 					201 콘솔 트리 (selectable) Motion_Network
> 			14700 탭 항목 (selectable) Lib
> 			14701 탭 항목 (selectable) File
> 			14702 탭 항목 (selectable) Global
> 			14703 탭 항목 (selectable) Net
> 			14704 탭 항목 (selectable) Class
> 			207 단추 Close
> 		208 Tab Properties ID: 424886560
> 			209 창 ID: 309505912
> 				210 TABLE Properties Window ID: 315226536
> 					14705 custom Name
> 					14706 custom GUID
> 					14707 custom Class
> 					14708 custom Position
> 					14709 custom Visualized
> 					14710 custom World
> 					14711 custom RealTime
> 					14712 custom BackgroundTime
> 					14713 custom RealIndex
> 					14714 custom BackgroundIndex
> 					14715 custom OPC-UA Instance
> 					14716 custom Draw Connection
> 					14717 custom Comment
> 				224 도구 모음 ID: 59392
> 					225 단추
> 					226 단추
> 			14718 탭 항목 (selectable) Properties
> 			228 단추 Close
>
> The focused UI element is 3 창 ID: 59648.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL Class 보기 열기",
>   "code": "await sky.click({window:targetWindow,element_index:14704}); state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.slice(-6000))"
> }
> ```
>
> ```text
> element 14704 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL Class 보기 열기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var classMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 탭 항목 \\(selectable\\) Class\\n/); if(!classMatch) throw new Error('Class tab not found'); await sky.click({window:state.window,element_index:Number(classMatch[1])}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.slice(-6000))"
> }
> ```
>
> ```text
> 210 단추 Connection Manager
> 				16211 단추 Task Configuration
> 			73 도구 모음 Net Edit
> 				16212 단추 Select
> 				16213 단추 Move view
> 				16214 단추 Insert comment
> 				16215 단추 Zoom(+/-)
> 				16216 단추 Zoom to all
> 				16217 단추 (disabled) Zoom selection
> 			80 도구 모음 Debug
> 				16218 단추 Go online (Alt+F6)
> 				16219 단추 (disabled) Change Online Settings
> 				16220 메뉴 항목 Online Connection
> 				16221 단추 (disabled) Set Online Connection For Current Project
> 				16222 단추 Download (F6)
> 				16223 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				16224 단추 Download Module on the Fly
> 				16225 단추 (disabled) Save Project on PLC
> 				16226 단추 (disabled) Start (F7)
> 				16227 단추 Reset (F8)
> 				16228 단추 (disabled) Toggle breakpoint (F4)
> 				16229 단추 (disabled) Create condition breakpoint
> 				16230 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				16231 단추 AWL trigger on/off
> 				16232 단추 (disabled) Fix AWL trigger to current instruction
> 				16233 단추 Activate/Deactivate Live View
> 				16234 단추 Activate/Deactivate Auto Watch
> 				16235 단추 (disabled) Goto instruction pointer
> 				16236 단추 (disabled) Step into (F5)
> 				16237 단추 (disabled) Step over (Alt+F5)
> 				16238 단추 (disabled) Step out (Shift+F5)
> 				16239 단추 (disabled) Set instruction pointer
> 			103 도구 모음 Build
> 				16240 메뉴 항목 (disabled) Target Architecture
> 				16241 단추 Build changes (F9)
> 				16242 단추 Rebuild project (Strg+F9)
> 				16243 단추 (disabled) Cancel building (Ctrl+Break)
> 				16244 단추 Link project
> 			109 도구 모음 Standard
> 				16245 단추 New project (Strg+N)
> 				16246 단추 Open a file (Strg+Shift+O)
> 				16247 단추 Close active document (Strg+F4)
> 				16248 단추 (disabled) Save file (Strg+S)
> 				16249 단추 Open project (Strg+O)
> 				16250 단추 (disabled) Save project changes (Strg+Shift+S)
> 				16251 단추 Close project
> 				16252 단추 Print
> 				16253 단추 Cut (Strg+X)
> 				16254 단추 Copy (Strg+C)
> 				16255 단추 Paste (Strg+V)
> 				16256 메뉴 항목 (disabled) Undo (Strg+Z)
> 				16257 메뉴 항목 (disabled) Redo (Strg+Y)
> 				16258 단추 (disabled) Navigate Backward (Alt+Left)
> 				16259 단추 (disabled) Navigate Forward (Alt +Right)
> 			125 메뉴 모음 Menu Bar
> 				16260 메뉴 항목 FILE
> 				16261 메뉴 항목 EDIT
> 				16262 메뉴 항목 NETEDIT
> 				16263 메뉴 항목 VIEW
> 				16264 메뉴 항목 PROJECT
> 				16265 메뉴 항목 BUILD
> 				16266 메뉴 항목 DEBUG
> 				16267 메뉴 항목 ANALYZE
> 				16268 메뉴 항목 TOOLS
> 				16269 메뉴 항목 EXTRAS
> 				16270 메뉴 항목 WINDOW
> 				16271 메뉴 항목 HELP
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

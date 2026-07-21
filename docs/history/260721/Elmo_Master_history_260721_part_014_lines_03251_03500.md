> 						141 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							142 단추 위쪽 스크롤 화살표 ID: UpButton
> 							143 위치 조정 위치 ID: ScrollbarThumb
> 							144 단추 페이지 아래로 ID: DownPageButton
> 							145 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						146 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							147 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							148 위치 조정 위치 ID: ScrollbarThumb
> 							149 단추 페이지 오른쪽으로 ID: DownPageButton
> 							150 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						151 위치 조정 (disabled)
> 					152 창 xtpBarTop ID: 59419
> 						153 도구 모음 Object Editor
> 							154 단추
> 							155 단추 Debugging Context:
> 							156 메뉴 항목 Context
> 			157 창 _TCPIPServer_RT [RO] Secondary Actions: Raise ID: 65282
> 				158 창 ID: 59648
> 					159 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						160 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							161 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							162 위치 조정 위치 ID: ScrollbarThumb
> 							163 단추 페이지 오른쪽으로 ID: DownPageButton
> 							164 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					165 창 xtpBarTop ID: 59419
> 						166 도구 모음 Object Editor
> 							167 단추 (disabled)
> 							168 단추 (disabled) Debugging Context:
> 							169 메뉴 항목 (disabled) Context
> 		170 상태 표시줄 ID: 59393
> 			171 텍스트
> 			172 텍스트
> 			173 텍스트
> 			174 텍스트
> 			175 텍스트
> 			176 텍스트 PLC = PC
> 			177 텍스트
> 			178 텍스트
> 			179 텍스트
> 			180 텍스트 NUM
> 			181 텍스트
> 		182 창 xtpBarTop ID: 59419
> 			183 도구 모음 Script
> 			184 도구 모음 Edit
> 				185 단추 (disabled) Toggle bookmark
> 				186 단추 (disabled) Previous bookmark
> 				187 단추 (disabled) Next bookmark
> 				188 단추 (disabled) Delete all bookmarks
> 				189 단추 (disabled) Previous bookmark in this file
> 				190 단추 (disabled) Next bookmark in this file
> 				191 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				192 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				193 단추 (disabled) Unindent (Shift+Tab)
> 				194 단추 (disabled) Indent (Tab)
> 			195 도구 모음 Macros Manager
> 				196 메뉴 항목 Macros
> 			197 도구 모음 Layout Manager
> 				198 메뉴 항목 Layouts
> 			199 도구 모음 Toolbox
> 				200 단추 DataAnalyzer
> 				201 메뉴 항목 Toolbar Options
> 			202 도구 모음 Net Edit
> 				203 단추 Select
> 				204 메뉴 항목 Toolbar Options
> 			205 도구 모음 Debug
> 				206 단추 Go online (Alt+F6)
> 				207 단추 (disabled) Change Online Settings
> 				208 메뉴 항목 Online Connection
> 				209 단추 (disabled) Set Online Connection For Current Project
> 				210 단추 Download (F6)
> 				211 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				212 단추 (disabled) Download Module on the Fly
> 				213 단추 (disabled) Save Project on PLC
> 				214 단추 (disabled) Start (F7)
> 				215 단추 Reset (F8)
> 				216 단추 (disabled) Toggle breakpoint (F4)
> 				217 단추 (disabled) Create condition breakpoint
> 				218 메뉴 항목 Toolbar Options
> 			219 도구 모음 Build
> 				220 메뉴 항목 (disabled) Target Architecture
> 				221 단추 Build changes (F9)
> 				222 단추 Rebuild project (Strg+F9)
> 				223 단추 (disabled) Cancel building (Ctrl+Break)
> 				224 단추 Link project
> 			225 도구 모음 Standard
> 				226 단추 New project (Strg+N)
> 				227 단추 Open a file (Strg+Shift+O)
> 				228 단추 Close active document (Strg+F4)
> 				229 단추 (disabled) Save file (Strg+S)
> 				230 단추 Open project (Strg+O)
> 				231 단추 (disabled) Save project changes (Strg+Shift+S)
> 				232 단추 Close project
> 				233 단추 Print
> 				234 단추 Cut (Strg+X)
> 				235 단추 Copy (Strg+C)
> 				236 단추 Paste (Strg+V)
> 				237 메뉴 항목 (disabled) Undo (Strg+Z)
> 				238 메뉴 항목 (disabled) Redo (Strg+Y)
> 				239 단추 Navigate Backward (Alt+Left)
> 				240 단추 (disabled) Navigate Forward (Alt +Right)
> 			241 메뉴 모음 Menu Bar
> 				242 메뉴 항목 FILE
> 				243 메뉴 항목 EDIT
> 				244 메뉴 항목 VIEW
> 				245 메뉴 항목 PROJECT
> 				246 메뉴 항목 BUILD
> 				247 메뉴 항목 DEBUG
> 				248 메뉴 항목 ANALYZE
> 				249 메뉴 항목 TOOLS
> 				250 메뉴 항목 EXTRAS
> 				251 메뉴 항목 WINDOW
> 				252 메뉴 항목 HELP
> 		253 창 Splitter ID: 125724648
> 		254 창 Splitter ID: 125724480
> 		255 Tab Debugger ID: 295437008
> 			256 탭 항목 (selectable) Python Script
> 			257 탭 항목 (selectable) Output
> 			258 탭 항목 (selectable) Debugger
> 			259 단추 Close
> 		260 창 Splitter ID: 125724144
> 		261 Tab Network View ID: 125483184
> 			262 트리 ID: 104
> 				263 콘솔 트리 (selectable, disabled) Elmo_EtherCAT_Test_4Axis
> 					264 콘솔 트리 (selectable, disabled) Comm_Network
> 					265 콘솔 트리 (selectable, disabled) HW_Network
> 					266 콘솔 트리 (selectable, disabled) Motion_Network
> 			267 탭 항목 (selectable) Lib
> 			268 탭 항목 (selectable) File
> 			269 탭 항목 (selectable) Global
> 			270 탭 항목 (selectable) Net
> 			271 탭 항목 (selectable) Class
> 			272 단추 Close
> 		273 Tab Properties ID: 125485008
> 			274 창 ID: 290002192
> 				275 TABLE Properties Window ID: 293314152
> 					276 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						277 단추 위쪽 스크롤 화살표 ID: UpButton
> 						278 위치 조정 위치 ID: ScrollbarThumb
> 						279 단추 페이지 아래로 ID: DownPageButton
> 						280 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					281 custom Name
> 					282 custom Revision
> 					283 custom Type
> 					284 custom Draw Connection
> 					285 custom Comment
> 					286 custom Filename
> 				287 도구 모음 ID: 59392
> 					288 단추
> 					289 단추
> 			290 탭 항목 (selectable) Properties
> 			291 단추 Close
>
> The focused UI element is 0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 오프라인 전환",
>   "code": "{const observation=globalThis.state;if(observation?.accessibility==null)throw new Error('No accessibility observation');const elementIndex=206;globalThis.state=null;try{await sky.click({window:observation.window,element_index:elementIndex});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});}catch(error){throw new Error('Input or refresh outcome is unknown; reobserve before retrying',{cause:error});}globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));}"
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
>   "title": "LASAL 창 확인",
>   "code": "var winD1after = await sky.list_windows(); nodeRepl.write(JSON.stringify(winD1after,null,2));"
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 395936,
>     "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor"
>   },
>   {
>     "app": "process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
>     "id": 788248,
>     "title": "ctrlX PLC 매뉴얼 스터디 노트 - Chrome"
>   },
>   {
>     "app": "process:C:\\Program Files\\WindowsApps\\OpenAI.Codex_26.715.4045.0_x64__2p2nqsd0c76g0\\app\\ChatGPT.exe",
>     "id": 721952,
>     "title": "ChatGPT"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 6886672,
>     "title": "GUDEL - 파일 탐색기 [ ssh_dev ↓0 ↑3 ]"
>   },
>   {
>     "app": "process:C:\\Program Files\\GitExtensions\\GitExtensions.exe",
>     "id": 268918,
>     "title": "GUDEL (ssh_dev) - Git Extensions"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 269030,
>     "title": "si사업부_1 - 파일 탐색기"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 4395084,
>     "title": "docs - 파일 탐색기 [ ssh_dev ↓0 ↑3 ]"
>   },
>   {
>     "app": "process:C:\\Users\\dreiv\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe",
>     "id": 2164744,
>     "title": "LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md - Visual Studio Code"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 2495932,
>     "title": "docs - 파일 탐색기 [ ssh_dev ↓0 ↑3 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 264084,
>     "title": "03_API_User_Manual - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Program Files\\Microsoft Office\\Office16\\WINWORD.EXE",
>     "id": 3085150,
>     "title": "LASAL_Motion_Control_API_User_Manual_KO.docx [읽기 전용] - Word"
>   },
>   {
>     "app": "process:C:\\Program Files\\Adobe\\Acrobat DC\\Acrobat\\Acrobat.exe",
>     "id": 70110,
>     "title": "LASAL_Motion_Control_API_User_Manual_KO.pdf - Adobe Acrobat Reader (64-bit)"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe",
>     "id": 526540,
>     "title": "PmasApiWpfTestApp - Microsoft Visual Studio"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",

> 			4424 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				4425 단추 (disabled) Toggle bookmark
> 				4426 단추 (disabled) Previous bookmark
> 				4427 단추 (disabled) Next bookmark
> 				4428 단추 (disabled) Delete all bookmarks
> 				4429 단추 (disabled) Previous bookmark in this file
> 				4430 단추 (disabled) Next bookmark in this file
> 				4431 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				4432 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				4433 단추 (disabled) Unindent (Shift+Tab)
> 				4434 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				4435 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				4436 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				4437 단추 DataAnalyzer
> 				4438 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				4439 단추 (disabled) Select
> 				4440 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				4441 단추 (disabled) Go online (Alt+F6)
> 				4442 단추 (disabled) Change Online Settings
> 				4443 메뉴 항목 (disabled) Online Connection
> 				4444 단추 (disabled) Set Online Connection For Current Project
> 				4445 단추 (disabled) Download (F6)
> 				4446 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				4447 단추 (disabled) Download Module on the Fly
> 				4448 단추 (disabled) Save Project on PLC
> 				4449 단추 (disabled) Start (F7)
> 				4450 단추 (disabled) Reset (F8)
> 				4451 단추 (disabled) Toggle breakpoint (F4)
> 				4452 단추 (disabled) Create condition breakpoint
> 				4453 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				4454 메뉴 항목 (disabled) Target Architecture
> 				4455 단추 (disabled) Build changes (F9)
> 				4456 단추 (disabled) Rebuild project (Strg+F9)
> 				4457 단추 Cancel building (Ctrl+Break)
> 				4458 단추 (disabled) Link project
> 			54 도구 모음 Standard
> 				4459 단추 (disabled) New project (Strg+N)
> 				4460 단추 Open a file (Strg+Shift+O)
> 				4461 단추 (disabled) Close active document (Strg+F4)
> 				4462 단추 (disabled) Save file (Strg+S)
> 				4463 단추 (disabled) Open project (Strg+O)
> 				4464 단추 (disabled) Save project changes (Strg+Shift+S)
> 				4465 단추 (disabled) Close project
> 				4466 단추 (disabled) Print
> 				4467 단추 Cut (Strg+X)
> 				4468 단추 Copy (Strg+C)
> 				4469 단추 Paste (Strg+V)
> 				4470 메뉴 항목 (disabled) Undo (Strg+Z)
> 				4471 메뉴 항목 (disabled) Redo (Strg+Y)
> 				4472 단추 (disabled) Navigate Backward (Alt+Left)
> 				4473 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				4474 메뉴 항목 FILE
> 				4475 메뉴 항목 EDIT
> 				4476 메뉴 항목 VIEW
> 				4477 메뉴 항목 PROJECT
> 				4478 메뉴 항목 BUILD
> 				4479 메뉴 항목 DEBUG
> 				4480 메뉴 항목 ANALYZE
> 				4481 메뉴 항목 TOOLS
> 				4482 메뉴 항목 EXTRAS
> 				4483 메뉴 항목 WINDOW
> 				4484 메뉴 항목 HELP
> 		82 창 Splitter ID: 306256112
> 		83 창 Splitter ID: 306255440
> 		84 Tab Output ID: 306064200
> 			85 창 ID: 1200
> 				86 창 ID: 1200
> 					87 LIST ID: 1201
> 						4485 목록 항목 (selectable)
> 					88 스크롤 막대 (disabled) ID: 59904
> 						89 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			4486 탭 항목 (selectable) Python Script
> 			4487 탭 항목 (selectable) Debugger
> 			4488 탭 항목 (selectable) Output
> 			95 단추 Close
> 		96 창 Splitter ID: 306256448
> 		97 Tab Class View ID: 306057360
> 			98 트리 (disabled) ID: 103
> 			4489 탭 항목 (selectable) Lib
> 			4490 탭 항목 (selectable) File
> 			4491 탭 항목 (selectable) Class
> 			102 단추 Close
> 		103 Tab Properties ID: 306057816
> 			104 창 (disabled) ID: 297102840
> 				105 TABLE Properties Window ID: 301242408
> 				106 도구 모음 ID: 59392
> 					107 단추
> 					108 단추
> 			4492 탭 항목 (selectable) Properties
> 			110 단추 Close
>
> The focused UI element is 1 창 작업 영역 ID: 59648.
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
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || ''));",
>   "title": "Check LASAL project load",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 		2 상태 표시줄 ID: 59393
> 			5105 진행률 표시줄 ID: 216680912
> 			3 텍스트
> 			4 텍스트 Compiling
> 			5 텍스트
> 			6 텍스트
> 			7 텍스트
> 			8 텍스트
> 			9 텍스트
> 			10 텍스트 Offline
> 			11 텍스트
> 			5106 텍스트 NUM
> 			5107 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				5108 단추 (disabled) Toggle bookmark
> 				5109 단추 (disabled) Previous bookmark
> 				5110 단추 (disabled) Next bookmark
> 				5111 단추 (disabled) Delete all bookmarks
> 				5112 단추 (disabled) Previous bookmark in this file
> 				5113 단추 (disabled) Next bookmark in this file
> 				5114 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				5115 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				5116 단추 (disabled) Unindent (Shift+Tab)
> 				5117 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				5118 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				5119 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				5120 단추 DataAnalyzer
> 				5121 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				5122 단추 (disabled) Select
> 				5123 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				5124 단추 (disabled) Go online (Alt+F6)
> 				5125 단추 (disabled) Change Online Settings
> 				5126 메뉴 항목 (disabled) Online Connection
> 				5127 단추 (disabled) Set Online Connection For Current Project
> 				5128 단추 (disabled) Download (F6)
> 				5129 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				5130 단추 (disabled) Download Module on the Fly
> 				5131 단추 (disabled) Save Project on PLC
> 				5132 단추 (disabled) Start (F7)
> 				5133 단추 (disabled) Reset (F8)
> 				5134 단추 (disabled) Toggle breakpoint (F4)
> 				5135 단추 (disabled) Create condition breakpoint
> 				5136 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				5137 메뉴 항목 (disabled) Target Architecture
> 				5138 단추 (disabled) Build changes (F9)
> 				5139 단추 (disabled) Rebuild project (Strg+F9)
> 				5140 단추 Cancel building (Ctrl+Break)
> 				5141 단추 (disabled) Link project
> 			54 도구 모음 Standard
> 				5142 단추 (disabled) New project (Strg+N)
> 				5143 단추 Open a file (Strg+Shift+O)
> 				5144 단추 (disabled) Close active document (Strg+F4)
> 				5145 단추 (disabled) Save file (Strg+S)
> 				5146 단추 (disabled) Open project (Strg+O)
> 				5147 단추 (disabled) Save project changes (Strg+Shift+S)
> 				5148 단추 (disabled) Close project
> 				5149 단추 (disabled) Print
> 				5150 단추 Cut (Strg+X)
> 				5151 단추 Copy (Strg+C)
> 				5152 단추 Paste (Strg+V)
> 				5153 메뉴 항목 (disabled) Undo (Strg+Z)
> 				5154 메뉴 항목 (disabled) Redo (Strg+Y)
> 				5155 단추 (disabled) Navigate Backward (Alt+Left)
> 				5156 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				5157 메뉴 항목 FILE
> 				5158 메뉴 항목 EDIT
> 				5159 메뉴 항목 VIEW
> 				5160 메뉴 항목 PROJECT
> 				5161 메뉴 항목 BUILD
> 				5162 메뉴 항목 DEBUG
> 				5163 메뉴 항목 ANALYZE
> 				5164 메뉴 항목 TOOLS
> 				5165 메뉴 항목 EXTRAS
> 				5166 메뉴 항목 WINDOW
> 				5167 메뉴 항목 HELP
> 		82 창 Splitter ID: 306256112
> 		83 창 Splitter ID: 306255440
> 		84 Tab Output ID: 306064200
> 			85 창 ID: 1200
> 				86 창 ID: 1200
> 					87 LIST ID: 1201
> 						4485 목록 항목 (selectable)
> 						4553 목록 항목 (selectable)
> 						4758 목록 항목 (selectable)
> 						4826 목록 항목 (selectable)
> 						4827 목록 항목 (selectable)
> 						5096 목록 항목 (selectable)
> 						5097 목록 항목 (selectable)
> 					88 스크롤 막대 ID: 59904
> 						89 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			5168 탭 항목 (selectable) Python Script
> 			5169 탭 항목 (selectable) Debugger
> 			5170 탭 항목 (selectable) Output
> 			95 단추 Close
> 		96 창 Splitter ID: 306256448
> 		97 Tab Class View ID: 306057360
> 			98 트리 (disabled) ID: 103
> 			5171 탭 항목 (selectable) Lib
> 			5172 탭 항목 (selectable) File
> 			5173 탭 항목 (selectable) Class
> 			102 단추 Close
> 		103 Tab Properties ID: 306057816
> 			104 창 (disabled) ID: 297102840
> 				105 TABLE Properties Window ID: 301242408
> 				106 도구 모음 ID: 59392
> 					107 단추
> 					108 단추
> 			5174 탭 항목 (selectable) Properties
> 			110 단추 Close
>
> The focused UI element is 1 창 작업 영역 ID: 59648.
> ```
>
> Image output: image/jpeg
>

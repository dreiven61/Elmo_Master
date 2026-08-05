>     "originX": -1895,
>     "originY": 22,
>     "zIndex": 0
>   },
>   {
>     "id": "screenshot-1",
>     "width": 960,
>     "height": 502,
>     "originX": -1890,
>     "originY": 27,
>     "zIndex": 1
>   },
>   {
>     "id": "screenshot-2",
>     "width": 1904,
>     "height": 1016,
>     "originX": -1912,
>     "originY": -3,
>     "zIndex": 2
>   }
> ]
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
>   "title": "Select project file",
>   "code": "{ var observation=globalThis.state; var shot=observation.screenshots.reduce((a,b)=>!a||b.zIndex>a.zIndex?b:a,null); globalThis.state=null; await sky.click({window:observation.window,screenshotId:shot.id,x:500,y:292,click_count:2}); globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||\"\")); }",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 		2 상태 표시줄 ID: 59393
> 			2055 진행률 표시줄 ID: 183938736
> 			3 텍스트
> 			4 텍스트 Load Project
> 			5 텍스트
> 			6 텍스트
> 			7 텍스트
> 			8 텍스트
> 			9 텍스트
> 			10 텍스트 Offline
> 			11 텍스트
> 			2056 텍스트 NUM
> 			2057 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				2058 단추 (disabled) Toggle bookmark
> 				2059 단추 (disabled) Previous bookmark
> 				2060 단추 (disabled) Next bookmark
> 				2061 단추 (disabled) Delete all bookmarks
> 				2062 단추 (disabled) Previous bookmark in this file
> 				2063 단추 (disabled) Next bookmark in this file
> 				2064 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				2065 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				2066 단추 (disabled) Unindent (Shift+Tab)
> 				2067 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				2068 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				2069 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				2070 단추 DataAnalyzer
> 				2071 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				2072 단추 (disabled) Select
> 				2073 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				2074 단추 (disabled) Go online (Alt+F6)
> 				2075 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				2076 메뉴 항목 (disabled) Target Architecture
> 				2077 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				2078 단추 (disabled) New project (Strg+N)
> 				2079 단추 Open a file (Strg+Shift+O)
> 				2080 단추 (disabled) Close active document (Strg+F4)
> 				2081 단추 (disabled) Save file (Strg+S)
> 				2082 단추 (disabled) Open project (Strg+O)
> 				2083 단추 (disabled) Save project changes (Strg+Shift+S)
> 				2084 단추 (disabled) Close project
> 				2085 단추 (disabled) Print
> 				2086 단추 Cut (Strg+X)
> 				2087 단추 Copy (Strg+C)
> 				2088 단추 Paste (Strg+V)
> 				2089 메뉴 항목 (disabled) Undo (Strg+Z)
> 				2090 메뉴 항목 (disabled) Redo (Strg+Y)
> 				2091 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				2092 메뉴 항목 FILE
> 				2093 메뉴 항목 EDIT
> 				2094 메뉴 항목 VIEW
> 				2095 메뉴 항목 PROJECT
> 				2096 메뉴 항목 BUILD
> 				2097 메뉴 항목 DEBUG
> 				2098 메뉴 항목 ANALYZE
> 				2099 메뉴 항목 TOOLS
> 				2100 메뉴 항목 EXTRAS
> 				2101 메뉴 항목 WINDOW
> 				2102 메뉴 항목 HELP
> 		67 창 Splitter ID: 371772512
> 		68 창 Splitter ID: 371770328
> 		69 Tab Output ID: 274603424
> 			70 창 ID: 1200
> 				71 창 ID: 1200
> 					72 LIST ID: 1201
> 						2103 목록 항목 (selectable)
> 					73 스크롤 막대 ID: 59904
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			2104 탭 항목 (selectable) Python Script
> 			2105 탭 항목 (selectable) Debugger
> 			2106 탭 항목 (selectable) Output
> 			80 단추 Close
> 		81 창 Splitter ID: 371773352
> 		82 Tab Class View ID: 274609808
> 			83 트리 (disabled) ID: 103
> 			2107 탭 항목 (selectable) Lib
> 			2108 탭 항목 (selectable) File
> 			2109 탭 항목 (selectable) Class
> 			2110 탭 항목 (selectable) Global
> 			88 단추 Close
> 		89 Tab Properties ID: 274603880
> 			90 창 (disabled) ID: 261121536
> 				91 TABLE Properties Window ID: 272349640
> 				92 도구 모음 ID: 59392
> 					93 단추
> 					94 단추
> 			2111 탭 항목 (selectable) Properties
> 			96 단추 Close
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
>   "title": "Wait for project load",
>   "code": "await new Promise(r=>setTimeout(r,3000)); globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||\"\"));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트
> 			5 텍스트
> 			6 텍스트
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				3023 단추 (disabled) Toggle bookmark
> 				3024 단추 (disabled) Previous bookmark
> 				3025 단추 (disabled) Next bookmark
> 				3026 단추 (disabled) Delete all bookmarks
> 				3027 단추 (disabled) Previous bookmark in this file
> 				3028 단추 (disabled) Next bookmark in this file
> 				3029 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				3030 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				3031 단추 (disabled) Unindent (Shift+Tab)
> 				3032 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				3033 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				3034 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				3035 단추 DataAnalyzer
> 				3036 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				3037 단추 (disabled) Select
> 				3038 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				3039 단추 (disabled) Go online (Alt+F6)
> 				3040 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				3041 메뉴 항목 (disabled) Target Architecture
> 				3042 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				3043 단추 (disabled) New project (Strg+N)
> 				3044 단추 Open a file (Strg+Shift+O)
> 				3045 단추 (disabled) Close active document (Strg+F4)
> 				3046 단추 (disabled) Save file (Strg+S)
> 				3047 단추 (disabled) Open project (Strg+O)
> 				3048 단추 (disabled) Save project changes (Strg+Shift+S)
> 				3049 단추 (disabled) Close project
> 				3050 단추 (disabled) Print
> 				3051 단추 Cut (Strg+X)
> 				3052 단추 Copy (Strg+C)
> 				3053 단추 Paste (Strg+V)
> 				3054 메뉴 항목 (disabled) Undo (Strg+Z)
> 				3055 메뉴 항목 (disabled) Redo (Strg+Y)
> 				3056 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				3057 메뉴 항목 FILE
> 				3058 메뉴 항목 EDIT
> 				3059 메뉴 항목 VIEW
> 				3060 메뉴 항목 PROJECT
> 				3061 메뉴 항목 BUILD
> 				3062 메뉴 항목 DEBUG
> 				3063 메뉴 항목 ANALYZE
> 				3064 메뉴 항목 TOOLS
> 				3065 메뉴 항목 EXTRAS
> 				3066 메뉴 항목 WINDOW
> 				3067 메뉴 항목 HELP
> 		67 창 Splitter ID: 371772512
> 		68 창 Splitter ID: 371770328
> 		69 Tab Output ID: 274603424
> 			70 창 ID: 1200
> 				71 창 ID: 1200
> 					72 LIST ID: 1201
> 						2594 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							2595 단추 위쪽 스크롤 화살표 ID: UpButton
> 							2596 단추 페이지 위로 ID: UpPageButton
> 							2597 위치 조정 위치 ID: ScrollbarThumb
> 							2598 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						2954 목록 항목 (selectable)
> 						2955 목록 항목 (selectable)
> 						2956 목록 항목 (selectable)
> 						2957 목록 항목 (selectable)
> 						2958 목록 항목 (selectable)
> 						2959 목록 항목 (selectable)
> 						3013 목록 항목 (selectable)
> 						3014 목록 항목 (selectable)
> 					73 스크롤 막대 ID: 59904
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton

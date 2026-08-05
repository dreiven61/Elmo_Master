> 			31649 메뉴 항목 Select All Ctrl+A
> 			31650 메뉴 항목 Comment
> 			31651 메뉴 항목 Dent
> 		1 창 작업 영역 ID: 59648
> 			21389 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				21390 창 ID: 59648
> 					21391 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21392 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21393 위치 조정 위치 ID: ScrollbarThumb
> 						21394 단추 페이지 아래로 ID: DownPageButton
> 						21395 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21396 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						21397 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						21398 위치 조정 위치 ID: ScrollbarThumb
> 						21399 단추 페이지 오른쪽으로 ID: DownPageButton
> 						21400 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					21401 위치 조정 (disabled)
> 			21402 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				21403 창 ID: 59648
> 					21404 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21405 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21406 위치 조정 위치 ID: ScrollbarThumb
> 						21407 단추 페이지 아래로 ID: DownPageButton
> 						21408 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21409 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						21410 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						21411 위치 조정 위치 ID: ScrollbarThumb
> 						21412 단추 페이지 오른쪽으로 ID: DownPageButton
> 						21413 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					21414 위치 조정 (disabled)
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
> 				31652 단추 (disabled) Toggle bookmark
> 				31653 단추 (disabled) Previous bookmark
> 				31654 단추 (disabled) Next bookmark
> 				31655 단추 (disabled) Delete all bookmarks
> 				31656 단추 (disabled) Previous bookmark in this file
> 				31657 단추 (disabled) Next bookmark in this file
> 				31658 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				31659 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				31660 단추 (disabled) Unindent (Shift+Tab)
> 				31661 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				31662 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				31663 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				31664 단추 DataAnalyzer
> 				31665 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				31666 단추 Select
> 				31667 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				31668 단추 Go onlin
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
>   "title": "클래스 보기로 복귀",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ESC\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 트리 검색 열기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+SHIFT+T\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 검색 대화상자 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(0,5000));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise
> 		33869 창
> 		33870 창
> 		33848 메뉴 Edit
> 			34331 메뉴 항목 (disabled) Undo Ctrl+Z
> 			34332 메뉴 항목 (disabled) Redo Ctrl+Y
> 			34333 메뉴 항목 Cut Ctrl+X
> 			34334 메뉴 항목 Copy Ctrl+C
> 			34335 메뉴 항목 Paste Ctrl+V
> 			34336 메뉴 항목 Find... Ctrl+F
> 			34337 메뉴 항목 Find in Files... Ctrl+Shift+F
> 			34338 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 			34339 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 			34340 메뉴 항목 Replace... Ctrl+R
> 			34341 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 			34342 메뉴 항목 (disabled) Goto Line... Ctrl+G
> 			34343 메뉴 항목 Reset Editor Colors
> 			34344 메뉴 항목 Mark
> 			34345 메뉴 항목 Bookmark
> 			34346 메뉴 항목 (disabled) Navigate Backward Alt+Left Arrow
> 			34347 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 			34348 메뉴 항목 Select All Ctrl+A
> 			34349 메뉴 항목 Comment
> 			34350 메뉴 항목 Dent
> 		1 창 작업 영역 ID: 59648
> 			21389 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				21390 창 ID: 59648
> 					21391 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21392 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21393 위치 조정 위치 ID: ScrollbarThumb
> 						21394 단추 페이지 아래로 ID: DownPageButton
> 						21395 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21396 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						21397 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						21398 위치 조정 위치 ID: ScrollbarThumb
> 						21399 단추 페이지 오른쪽으로 ID: DownPageButton
> 						21400 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					21401 위치 조정 (disabled)
> 			21402 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				21403 창 ID: 59648
> 					21404 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21405 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21406 위치 조정 위치 ID: ScrollbarThumb
> 						21407 단추 페이지 아래로 ID: DownPageButton
> 						21408 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21409 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						21410 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						21411 위치 조정 위치 ID: ScrollbarThumb
> 						21412 단추 페이지 오른쪽으로 ID: DownPageButton
> 						21413 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					21414 위치 조정 (disabled)
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
> 				34351 단추 (disabled) Toggle bookmark
> 				34352 단추 (disabled) Previous bookmark
> 				34353 단추 (disabled) Next bookmark
> 				34354 단추 (disabled) Delete all bookmarks
> 				34355 단추 (disabled) Previous bookmark in this file
> 				34356 단추 (disabled) Next bookmark in this file
> 				34357 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				34358 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				34359 단추 (disabled) Unindent (Shift+Tab)
> 				34360 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				34361 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				34362 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				34363 단추 DataAnalyzer
> 				34364 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				34365 단추 Select
> 				34366 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				34367 단추 Go online (Alt+F6)
> 				34368 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				34369 메뉴 항목 Target Architecture
> 				34370 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				34371 단추 New project (Strg+N)
> 				34372 단추 Open a file (Strg+Shift+O)
> 				34373 단추 Close active document (Strg+F4)
> 				34374 단추 (disabled) Save file (Strg+S)
> 				34375 단추 Open project (Strg+O)
> 				34376 단추 (disabled) Save project changes (Strg+Shift+S)
> 				34377 단추 Close project
> 				34378 단추 Print
> 				34379 단추 Cut (Strg+X)
> 				34380 단추 Copy (Strg+C)
> 				34381 단추 Paste (Strg+V)
> 				34382 메뉴 항목 (disabled) Undo (Strg+Z)
> 				34383 메뉴 항목 (disabled) Redo (Strg+Y)
> 				34384 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				34385 메뉴 항목 FILE
> 				34386 메뉴 항목 EDIT
> 					34387 메뉴 Edit
> 						34388 메뉴 항목 (disabled) Undo Ctrl+Z
> 						34389 메뉴 항목 (disabled) Redo Ctrl+Y
> 						34390 메뉴 항목 Cut Ctrl+X
> 						34391 메뉴 항목 Copy Ctrl+C
> 						34392 메뉴 항목 Paste Ctrl+V
> 						34393 메뉴 항목 Find... Ctrl+F
> 						34394 메뉴 항목 Find in Files... Ctrl+Shift+F
> 						34395 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 						34396 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 						34397 메뉴 항목 Replace... Ctrl+R
> 						34398 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 						34399 메뉴 항목 (disabled) Goto Line... Ctrl+G
> 						34400 메뉴 항목 Reset Editor Colors
> 						34401 메뉴 항목 Mark
> 						34402 메뉴 항목 Bookmark
> 						34403 메뉴 항목 (disabled) Navigate Backward Alt+Left Arrow
> 						34404 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 						34405 메뉴 항목 Select All Ctrl+A
> 						34406 메뉴 항목 Comment
> 						34407 메뉴 항목 Dent
> 				34408 메뉴 항목 NETEDIT
> 				34409 메뉴 항목 VIEW
> 				34410 메뉴 항목 PROJECT

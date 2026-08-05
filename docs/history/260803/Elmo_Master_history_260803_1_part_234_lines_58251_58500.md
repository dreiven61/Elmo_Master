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
>   "title": "LASAL 프로젝트 파일 선택",
>   "code": "nodeRepl.write((await globalThis.sky.list_windows()).filter(w=>String(w.title).includes(\"Lasal\")||String(w.title).includes(\"Open\")));"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3151338,
>     title: 'Lasal Class 2 (02.03.001)'
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
>   "title": "Elmo LASAL 프로젝트 선택",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:487,y:293,screenshotId:globalThis.state.screenshots[2].id}); await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"}); await new Promise(r=>setTimeout(r,2000)); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3151338,
>     title: 'Lasal Class 2 (02.03.001)'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=58302 chars=9977 sha256=9ab737bf4da1619ba2b6b589cca3db233c10e81965a57f8312ba7ae12be0cae5]'... 305255 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=58311 chars=9977 sha256=d7274346ab9e97fd5e4fcb66874da9ad2e55e9b58f08cf6a66c0a2cff18e567a]'... 296435 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
>       '\t\t2 상태 표시줄 ID: 59393\n' +
>       '\t\t\t2545 진행률 표시줄 ID: 192609000\n' +
>       '\t\t\t3 텍스트 \n' +
>       '\t\t\t4 텍스트 Load Project\n' +
>       '\t\t\t5 텍스트\n' +
>       '\t\t\t6 텍스트 \n' +
>       '\t\t\t7 텍스트\n' +
>       '\t\t\t8 텍스트 \n' +
>       '\t\t\t9 텍스트 \n' +
>       '\t\t\t10 텍스트 Offline\n' +
>       '\t\t\t11 텍스트\n' +
>       '\t\t\t2546 텍스트 NUM\n' +
>       '\t\t\t2547 텍스트\n' +
>       '\t\t12 창 xtpBarTop ID: 59419\n' +
>       '\t\t\t13 도구 모음 Edit\n' +
>       '\t\t\t\t2548 단추 (disabled) Toggle bookmark\n' +
>       '\t\t\t\t2549 단추 (disabled) Previous bookmark\n' +
>       '\t\t\t\t2550 단추 (disabled) Next bookmark\n' +
>       '\t\t\t\t2551 단추 (disabled) Delete all bookmarks\n' +
>       '\t\t\t\t2552 단추 (disabled) Previous bookmark in this file\n' +
>       '\t\t\t\t2553 단추 (disabled) Next bookmark in this file\n' +
>       '\t\t\t\t2554 단추 (disabled) Comment selected text (Ctrl+Shift+C)\n' +
>       '\t\t\t\t2555 단추 (disabled) Remove comment (Ctrl+Shift+X)\n' +
>       '\t\t\t\t2556 단추 (disabled) Unindent (Shift+Tab)\n' +
>       '\t\t\t\t2557 단추 (disabled) Indent (Tab)\n' +
>       '\t\t\t24 도구 모음 Macros Manager\n' +
>       '\t\t\t\t2558 메뉴 항목 Macros\n' +
>       '\t\t\t26 도구 모음 Layout Manager\n' +
>       '\t\t\t\t2559 메뉴 항목 Layouts\n' +
>       '\t\t\t28 도구 모음 Toolbox\n' +
>       '\t\t\t\t2560 단추 DataAnalyzer\n' +
>       '\t\t\t\t2561 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t31 도구 모음 Net Edit\n' +
>       '\t\t\t\t2562 단추 (disabled) Select\n' +
>       '\t\t\t\t2563 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t34 도구 모음 Debug\n' +
>       '\t\t\t\t2564 단추 (disabled) Go online (Alt+F6)\n' +
>       '\t\t\t\t2565 단추 (disabled) Change Online Settings\n' +
>       '\t\t\t\t2566 메뉴 항목 (disabled) Online Connection\n' +
>       '\t\t\t\t2567 단추 (disabled) Set Online Connection For Current Project\n' +
>       '\t\t\t\t2568 단추 (disabled) Download (F6)\n' +
>       '\t\t\t\t2569 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)\n' +
>       '\t\t\t\t2570 단추 (disabled) Download Module on the Fly\n' +
>       '\t\t\t\t2571 단추 (disabled) Save Project on PLC\n' +
>       '\t\t\t\t2572 단추 (disabled) Start (F7)\n' +
>       '\t\t\t\t2573 단추 (disabled) Reset (F8)\n' +
>       '\t\t\t\t2574 단추 (disabled) Toggle breakpoint (F4)\n' +
>       '\t\t\t\t2575 단추 (disabled) Create condition breakpoint\n' +
>       '\t\t\t\t2576 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t48 도구 모음 Build\n' +
>       '\t\t\t\t2577 메뉴 항목 (disabled) Target Architecture\n' +
>       '\t\t\t\t2578 단추 (disabled) Build changes (F9)\n' +
>       '\t\t\t\t2579 단추 (disabled) Rebuild project (Strg+F9)\n' +
>       '\t\t\t\t2580 단추 Cancel building (Ctrl+Break)\n' +
>       '\t\t\t\t2581 단추 (disabled) Link project\n' +
>       '\t\t\t54 도구 모음 Standard\n' +
>       '\t\t\t\t2582 단추 (disabled) New project (Strg+N)\n' +
>       '\t\t\t\t2583 단추 Open a file (Strg+Shift+O)\n' +
>       '\t\t\t\t2584 단추 (disabled) Close active document (Strg+F4)\n' +
>       '\t\t\t\t2585 단추 (disabled) Save file (Strg+S)\n' +
>       '\t\t\t\t2586 단추 (disabled) Open project (Strg+O)\n' +
>       '\t\t\t\t2587 단추 (disabled) Save project changes (Strg+Shift+S)\n' +
>       '\t\t\t\t2588 단추 (disabled) Close project\n' +
>       '\t\t\t\t2589 단추 (disabled) Print\n' +
>       '\t\t\t\t2590 단추 Cut (Strg+X)\n' +
>       '\t\t\t\t2591 단추 Copy (Strg+C)\n' +
>       '\t\t\t\t2592 단추 Paste (Strg+V)\n' +
>       '\t\t\t\t2593 메뉴 항목 (disabled) Undo (Strg+Z)\n' +
>       '\t\t\t\t2594 메뉴 항목 (disabled) Redo (Strg+Y)\n' +
>       '\t\t\t\t2595 단추 (disabled) Navigate Backward (Alt+Left)\n' +
>       '\t\t\t\t2596 단추 (disabled) Navigate Forward (Alt +Right)\n' +
>       '\t\t\t70 메뉴 모음 Menu Bar\n' +
>       '\t\t\t\t2597 메뉴 항목 FILE\n' +
>       '\t\t\t\t2598 메뉴 항목 EDIT\n' +
>       '\t\t\t\t2599 메뉴 항목 VIEW\n' +
>       '\t\t\t\t2600 메뉴 항목 PROJECT\n' +
>       '\t\t\t\t2601 메뉴 항목 BUILD\n' +
>       '\t\t\t\t2602 메뉴 항목 DEBUG\n' +
>       '\t\t\t\t2603 메뉴 항목 ANALYZE\n' +
>       '\t\t\t\t2604 메뉴 항목 TOOLS\n' +
>       '\t\t\t\t2605 메뉴 항목 EXTRAS\n' +
>       '\t\t\t\t2606 메뉴 항목 WINDOW\n' +
>       '\t\t\t\t2607 메뉴 항목 HELP\n' +
>       '\t\t82 창 Splitter ID: 389014280\n' +
>       '\t\t83 창 Splitter ID: 389010080\n' +
>       '\t\t84 Tab Output ID: 279800176\n' +
>       '\t\t\t85 창 ID: 1200\n' +
>       '\t\t\t\t86 창 ID: 1200\n' +
>       '\t\t\t\t\t87 LIST ID: 1201\n' +
>       '\t\t\t\t\t\t2608 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t88 스크롤 막대 (disabled) ID: 59904\n' +
>       '\t\t\t\t\t\t89 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t90 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t91 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t2609 탭 항목 (selectable) Python Script\n' +
>       '\t\t\t2610 탭 항목 (selectable) Debugger\n' +
>       '\t\t\t2611 탭 항목 (selectable) Output\n' +
>       '\t\t\t95 단추 Close\n' +
>       '\t\t96 창 Splitter ID: 389013776\n' +
>       '\t\t97 Tab Class View ID: 279804736\n' +
>       '\t\t\t98 트리 (disabled) ID: 103\n' +
>       '\t\t\t2612 탭 항목 (selectable) Lib\n' +
>       '\t\t\t2613 탭 항목 (selectable) File\n' +
>       '\t\t\t2614 탭 항목 (selectable) Class\n' +
>       '\t\t\t102 단추 Close\n' +
>       '\t\t103 Tab Properties ID: 279798808\n' +
>       '\t\t\t104 창 (disabled) ID: 270081768\n' +
>       '\t\t\t\t105 TABLE Properties Window ID: 277338752\n' +
>       '\t\t\t\t106 도구 모음 ID: 59392\n' +
>       '\t\t\t\t\t107 단추\n' +
>       '\t\t\t\t\t108 단추\n' +
>       '\t\t\t2615 탭 항목 (selectable) Properties\n' +
>       '\t\t\t110 단추 Close\n' +
>       '\n' +
>       'The focused UI element is 1 창 작업 영역 ID: 59648.',
>     focused_element: '1 창 작업 영역 ID: 59648'
>   }
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
>   "title": "LASAL 프로젝트 로딩 확인",
>   "code": "await new Promise(r=>setTimeout(r,3000)); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3151338,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=58468 chars=9977 sha256=c47e53b4a3a98fbaaf505e15e206b6a4744bdb43051a6cdabb334c6308b4654f]'... 324123 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=58477 chars=9977 sha256=93202d42bbcfc72ccf2dc34f90c3c888217fac12f896ed05ca69612aa1991fc0]'... 307711 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
>       '\t\t2 상태 표시줄 ID: 59393\n' +
>       '\t\t\t3 텍스트 \n' +
>       '\t\t\t4 텍스트 \n' +
>       '\t\t\t5 텍스트\n' +
>       '\t\t\t6 텍스트 \n' +
>       '\t\t\t7 텍스트 \n' +
>       '\t\t\t8 텍스트 Offline\n' +
>       '\t\t\t9 텍스트\n' +
>       '\t\t\t10 텍스트 NUM\n' +
>       '\t\t\t11 텍스트\n' +
>       '\t\t12 창 xtpBarTop ID: 59419\n' +
>       '\t\t\t13 도구 모음 Edit\n' +
>       '\t\t\t\t3526 단추 (disabled) Toggle bookmark\n' +

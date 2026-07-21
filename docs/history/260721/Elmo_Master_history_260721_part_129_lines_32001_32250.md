>     displayName: 'Advanced IP Scanner',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Advanced IP Scanner\\advanced_ip_scanner.exe',
>     windows: []
>   },
>   {
>     displayName: 'Qt Creator 18.0.1 (Community)',
>     id: 'C:\\Qt\\Tools\\QtCreator\\bin\\qtcreator.exe',
>     windows: []
>   },
>   {
>     displayName: 'Eclipse IDE for Java Developers - 2023-09',
>     id: 'C:\\Users\\dreiv\\eclipse\\java-2023-09\\eclipse\\eclipse.exe',
>     windows: []
>   },
>   {
>     displayName: 'MDS Installation Guide',
>     id: 'C:\\ProgramData\\MDS6\\MDS Installation Guide.pdf',
>     windows: []
>   },
>   {
>     displayName: 'WSL Settings',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\WSL\\wslsettings\\wslsettings.exe',
>     windows: []
>   },
>   {
>     displayName: 'ctrlX Cam Designer 3.6.1',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX Cam Designer\\3.6.0\\ctrlX-CamDesigner.exe',
>     windows: []
>   },
>   {
>     displayName: 'ctrlX DRIVE Engineering Serial Setup 01V30.0',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\ctrlX WORKS\\ctrlX DRIVE Engineering\\0130\\cxdd.exe',
>     windows: []
>   },
>   {
>     displayName: 'Oracle VM VirtualBox',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Oracle\\VirtualBox\\VirtualBox.exe',
>     windows: []
>   },
>   {
>     displayName: 'SlamClient',
>     id: 'C:\\work\\SIGMATEK\\Slam\\SlamLoc\\SlamClient\\SlamClient.exe',
>     windows: []
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
>   "code": "\nvar winsCU = await sky.list_windows();\nnodeRepl.write(winsCU);\n",
>   "title": "LASAL 창 검색"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
>     id: 396542,
>     title: 'ChatGPT - Chrome'
>   },
>   {
>     app: 'process:C:\\Program Files\\WindowsApps\\OpenAI.Codex_26.715.7063.0_x64__2p2nqsd0c76g0\\app\\ChatGPT.exe',
>     id: 132772,
>     title: 'ChatGPT'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe',
>     id: 68064,
>     title: 'MMCLibDotNET_Test - Microsoft Visual Studio'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe',
>     id: 68068,
>     title: 'PmasApiWpfTestApp - Microsoft Visual Studio'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe',
>     id: 68062,
>     title: 'LasalApiWpfTestApp - Microsoft Visual Studio'
>   },
>   {
>     app: 'process:C:\\Program Files\\Microsoft Office\\Office16\\EXCEL.EXE',
>     id: 67542,
>     title: 'EtherCAT Controller(Master) 요구 사양.xlsx  [제한된 보기] [사용자가 마지막으로 저장함] - Excel'
>   },
>   {
>     app: 'process:C:\\Program Files\\Microsoft Office\\Office16\\EXCEL.EXE',
>     id: 67890,
>     title: 'Motion_Controller_API_정리.xlsx  [제한된 보기] [사용자가 마지막으로 저장함] - Excel'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe',
>     id: 132936,
>     title: '카카오톡'
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
>   "code": "\nvar lasalAppCU = appsCU.find(a=>a.displayName===\"LASAL Class 2\" && (a.id||\"\").includes(\"\\\\Sigmatek\\\\Lasal\\\\Class2\"));\nawait sky.launch_app({app:lasalAppCU.id});\nawait new Promise(r=>setTimeout(r,1800));\nappsCU = await sky.list_apps();\nlasalAppCU = appsCU.find(a=>a.displayName===\"LASAL Class 2\" && (a.id||\"\").includes(\"\\\\Sigmatek\\\\Lasal\\\\Class2\"));\nnodeRepl.write(lasalAppCU);\n",
>   "title": "LASAL IDE 다시 실행"
> }
> ```
>
> ```text
> {
>   id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   windows: [],
>   displayName: 'LASAL Class 2',
>   lastUsedDate: '2026-07-20',
>   useCount: 2,
>   isRunning: false
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nvar lasalWinCU = {app:\"process:C:\\\\Program Files (x86)\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\",id:1837062,title:\"Lasal Class 2 (02.03.001)\"};\nvar stCU = await sky.get_window_state({window:lasalWinCU,include_screenshot:true,include_text:true});\nnodeRepl.write({window:stCU.window,accessibility:stCU.accessibility});\nfor (var shotCU of stCU.screenshots) await nodeRepl.emitImage(shotCU.url);\n",
>   "title": "LASAL IDE 창 복구"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1837062,
>     title: 'Lasal Class 2 (02.03.001)'
>   },
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
>       '\t\t\t13 도구 모음 Script\n' +
>       '\t\t\t14 도구 모음 Edit\n' +
>       '\t\t\t\t15 단추 (disabled) Toggle bookmark\n' +
>       '\t\t\t\t16 단추 (disabled) Previous bookmark\n' +
>       '\t\t\t\t17 단추 (disabled) Next bookmark\n' +
>       '\t\t\t\t18 단추 (disabled) Delete all bookmarks\n' +
>       '\t\t\t\t19 단추 (disabled) Previous bookmark in this file\n' +
>       '\t\t\t\t20 단추 (disabled) Next bookmark in this file\n' +
>       '\t\t\t\t21 단추 (disabled) Comment selected text (Ctrl+Shift+C)\n' +
>       '\t\t\t\t22 단추 (disabled) Remove comment (Ctrl+Shift+X)\n' +
>       '\t\t\t\t23 단추 (disabled) Unindent (Shift+Tab)\n' +
>       '\t\t\t\t24 단추 (disabled) Indent (Tab)\n' +
>       '\t\t\t25 도구 모음 Macros Manager\n' +
>       '\t\t\t\t26 메뉴 항목 Macros\n' +
>       '\t\t\t27 도구 모음 Layout Manager\n' +
>       '\t\t\t\t28 메뉴 항목 Layouts\n' +
>       '\t\t\t29 도구 모음 Toolbox\n' +
>       '\t\t\t\t30 단추 DataAnalyzer\n' +
>       '\t\t\t\t31 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t32 도구 모음 Net Edit\n' +
>       '\t\t\t\t33 단추 (disabled) Select\n' +
>       '\t\t\t\t34 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t35 도구 모음 Debug\n' +
>       '\t\t\t\t36 단추 Go online (Alt+F6)\n' +
>       '\t\t\t\t37 단추 Change Online Settings\n' +
>       '\t\t\t\t38 메뉴 항목 Online Connection\n' +
>       '\t\t\t\t39 단추 (disabled) Set Online Connection For Current Project\n' +
>       '\t\t\t\t40 단추 (disabled) Download (F6)\n' +
>       '\t\t\t\t41 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)\n' +
>       '\t\t\t\t42 단추 (disabled) Download Module on the Fly\n' +
>       '\t\t\t\t43 단추 (disabled) Save Project on PLC\n' +
>       '\t\t\t\t44 단추 (disabled) Start (F7)\n' +
>       '\t\t\t\t45 단추 (disabled) Reset (F8)\n' +
>       '\t\t\t\t46 단추 (disabled) Toggle breakpoint (F4)\n' +
>       '\t\t\t\t47 단추 (disabled) Create condition breakpoint\n' +
>       '\t\t\t\t48 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t49 도구 모음 Build\n' +
>       '\t\t\t\t50 메뉴 항목 Target Architecture\n' +
>       '\t\t\t\t51 단추 (disabled) Build changes (F9)\n' +
>       '\t\t\t\t52 단추 (disabled) Rebuild project (Strg+F9)\n' +
>       '\t\t\t\t53 단추 (disabled) Cancel building (Ctrl+Break)\n' +
>       '\t\t\t\t54 단추 (disabled) Link project\n' +
>       '\t\t\t55 도구 모음 Standard\n' +
>       '\t\t\t\t56 단추 New project (Strg+N)\n' +
>       '\t\t\t\t57 단추 Open a file (Strg+Shift+O)\n' +
>       '\t\t\t\t58 단추 (disabled) Close active document (Strg+F4)\n' +
>       '\t\t\t\t59 단추 (disabled) Save file (Strg+S)\n' +
>       '\t\t\t\t60 단추 Open project (Strg+O)\n' +
>       '\t\t\t\t61 단추 (disabled) Save project changes (Strg+Shift+S)\n' +
>       '\t\t\t\t62 단추 (disabled) Close project\n' +
>       '\t\t\t\t63 단추 (disabled) Print\n' +
>       '\t\t\t\t64 단추 Cut (Strg+X)\n' +
>       '\t\t\t\t65 단추 Copy (Strg+C)\n' +
>       '\t\t\t\t66 단추 Paste (Strg+V)\n' +
>       '\t\t\t\t67 메뉴 항목 (disabled) Undo (Strg+Z)\n' +
>       '\t\t\t\t68 메뉴 항목 (disabled) Redo (Strg+Y)\n' +
>       '\t\t\t\t69 단추 (disabled) Navigate Backward (Alt+Left)\n' +
>       '\t\t\t\t70 단추 (disabled) Navigate Forward (Alt +Right)\n' +
>       '\t\t\t71 메뉴 모음 Menu Bar\n' +
>       '\t\t\t\t72 메뉴 항목 FILE\n' +
>       '\t\t\t\t73 메뉴 항목 EDIT\n' +
>       '\t\t\t\t74 메뉴 항목 VIEW\n' +
>       '\t\t\t\t75 메뉴 항목 PROJECT\n' +
>       '\t\t\t\t76 메뉴 항목 BUILD\n' +
>       '\t\t\t\t77 메뉴 항목 DEBUG\n' +
>       '\t\t\t\t78 메뉴 항목 ANALYZE\n' +
>       '\t\t\t\t79 메뉴 항목 TOOLS\n' +
>       '\t\t\t\t80 메뉴 항목 EXTRAS\n' +
>       '\t\t\t\t81 메뉴 항목 WINDOW\n' +
>       '\t\t\t\t82 메뉴 항목 HELP\n' +
>       '\t\t83 창 Splitter ID: 404864352\n' +
>       '\t\t84 창 Splitter ID: 404864016\n' +
>       '\t\t85 Tab Output ID: 295399128\n' +
>       '\t\t\t86 창 ID: 1200\n' +
>       '\t\t\t\t87 창 ID: 1200\n' +
>       '\t\t\t\t\t88 LIST ID: 1201\n' +
>       '\t\t\t\t\t89 스크롤 막대 (disabled) ID: 59904\n' +
>       '\t\t\t\t\t\t90 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t91 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t92 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t93 탭 항목 (selectable) Python Script\n' +
>       '\t\t\t94 탭 항목 (selectable) Output\n' +
>       '\t\t\t95 탭 항목 (selectable) Debugger\n' +
>       '\t\t\t96 단추 Close\n' +
>       '\t\t97 창 Splitter ID: 404864856\n' +
>       '\t\t98 Tab Class View ID: 295396392\n' +
>       '\t\t\t99 트리 ID: 103\n' +
>       '\t\t\t100 탭 항목 (selectable) Lib\n' +
>       '\t\t\t101 탭 항목 (selectable) File\n' +
>       '\t\t\t102 탭 항목 (selectable) Global\n' +
>       '\t\t\t103 탭 항목 (selectable) Net\n' +
>       '\t\t\t104 탭 항목 (selectable) Class\n' +

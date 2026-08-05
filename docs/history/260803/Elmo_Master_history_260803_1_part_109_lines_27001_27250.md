>       '\t\t\t\t\t\t\t\t16 항목 Navigation: »No topics above this level« INTRODUCTION ID: headerContentsCell\n' +
>       '\t\t\t\t\t\t\t\t\t17 텍스트 Navigation:\n' +
>       '\t\t\t\t\t\t\t\t\t18 텍스트 »No topics above this level«\n' +
>       '\t\t\t\t\t\t\t\t\t19 텍스트 INTRODUCTION\n' +
>       '\t\t\t\t\t\t\t20 이미지 Topic functions menu Description: Topic functions menu ID: hmMenuLink\n' +
>       '\t\t\t\t\t\t\t21 텍스트 Menu\n' +
>       '\t\t\t\t\t\t\t22 이미지 This is the first topic Description: This is the first topic\n' +
>       '\t\t\t\t\t\t\t23 링크\n' +
>       '\t\t\t\t\t\t\t\t24 이미지 Go to parent chapter Description: Go to parent chapter ID: hmHomeLink\n' +
>       '\t\t\t\t\t\t\t25 링크\n' +
>       '\t\t\t\t\t\t\t\t26 이미지 Go to next topic Description: Go to next topic ID: hmNextLink\n' +
>       '\t\t\t\t\t\t\t27 텍스트 The new LASAL project software offers all the advantages of modern automation technology. The use of innovative program techniques significantly reduce the development time and makes possible simple and fast implementation of machine concepts. The addition of individual machine objects to LASAL simplifies communication between the constructor and the software designer.\n' +
>       '\t\t\t\t\t\t\t28 텍스트 See also:\n' +
>       '\t\t\t\t\t\t\t29 링크 All in One\n' +
>       '\t\t\t\t\t\t\t\t30 텍스트 All in One\n' +
>       '\t\t\t\t\t\t\t31 링크 LASAL supports several target platforms\n' +
>       '\t\t\t\t\t\t\t\t32 텍스트 LASAL supports several target platforms\n' +
>       '\t\t\t\t\t\t\t33 링크 Real time with a multilevel, multi-tasking system\n' +
>       '\t\t\t\t\t\t\t\t34 텍스트 Real time with a multilevel, multi-tasking system\n' +
>       '\t\t\t\t\t\t\t35 링크 Open data Interface\n' +
>       '\t\t\t\t\t\t\t\t36 텍스트 Open data Interface\n' +
>       '\t\t\t\t\t\t\t37 링크 Worldwide service interface\n' +
>       '\t\t\t\t\t\t\t\t38 텍스트 Worldwide service interface\n' +
>       '\t\t\t\t\t\t\t39 링크 Online help system\n' +
>       '\t\t\t\t\t\t\t\t40 텍스트 Online help system\n' +
>       '\t\t\t\t\t\t\t41 링크 Multilanguage programming tool\n' +
>       '\t\t\t\t\t\t\t\t42 텍스트 Multilanguage programming tool\n' +
>       '\t\t\t\t\t\t\t43 링크 Disclaimer\n' +
>       '\t\t\t\t\t\t\t\t44 텍스트 Disclaimer\n' +
>       '\t\t45 창\n' +
>       '\t\t46 창\n' +
>       '\t\t\t47 Tab ID: 1007\n' +
>       '\t\t\t\t48 트리\n' +
>       '\t\t\t\t\t49 콘솔 트리 (selectable) INTRODUCTION\n' +
>       '\t\t\t\t\t50 콘솔 트리 (selectable) THE DESIGN ENVIRONMENT\n' +
>       '\t\t\t\t\t51 콘솔 트리 (selectable) PROGRAMMING\n' +
>       '\t\t\t\t\t52 콘솔 트리 (selectable) ADDITIONAL VIEWS, TOOLS,...\n' +
>       '\t\t\t\t\t53 콘솔 트리 (selectable) TIPS AND HELPS FOR WORKING WITH LASAL CLASS 2\n' +
>       '\t\t\t\t\t54 콘솔 트리 (selectable) THE FIRST LASAL PROJECT\n' +
>       '\t\t\t\t\t55 콘솔 트리 (selectable) APPENDIX\n' +
>       '\t\t\t\t56 탭 항목 (selectable) 내용(C)\n' +
>       '\t\t\t\t57 탭 항목 (selectable) 색인(N)\n' +
>       '\t\t\t\t58 탭 항목 (selectable) 검색(S)\n' +
>       '\t\t\t\t59 탭 항목 (selectable) 즐겨찾기(I)\n' +
>       '\t\t60 제목 표시줄\n' +
>       '\t\t\t61 메뉴 모음 시스템 ID: MenuBar\n' +
>       '\t\t\t\t62 메뉴 항목 시스템\n' +
>       '\t\t\t63 단추 최소화\n' +
>       '\t\t\t64 단추 최대화\n' +
>       '\t\t\t65 단추 닫기\n' +
>       '\n' +
>       'The focused UI element is 11 창 INTRODUCTION.',
>     focused_element: '11 창 INTRODUCTION'
>   }
> }
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
>   "title": "LASAL 네트워크 연결 도움말 검색",
>   "code": "await globalThis.sky.click({window:globalThis.helpWindow,x:169,y:48,screenshotId:'screenshot-0'}); await new Promise(r=>setTimeout(r,400)); var helpState2 = await globalThis.sky.get_window_state({window:globalThis.helpWindow,include_screenshot:true,include_text:true}); nodeRepl.write(helpState2);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 17242032,
>     title: 'LASAL Class 2 - Online help'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=27082 chars=9977 sha256=c017d9a572b179c6f40a1be6acbb3a3d587619054e99d0756bbd502b826a2674]'... 154983 more characters,
>       originX: 109,
>       originY: 51,
>       width: 1597,
>       height: 863
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=27091 chars=9977 sha256=1ea31bfbf4ab1dfe03c75e65590d5da186a96e1980914111581928d6128d59e1]'... 8799 more characters,
>       originX: 732,
>       originY: 380,
>       width: 350,
>       height: 205
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "LASAL Class 2 - Online help", App: Lasal2.exe.\n' +
>       '\t0 창 (disabled) LASAL Class 2 - Online help Secondary Actions: Raise\n' +
>       '\t\t66 대화 상자 항목 인쇄 Secondary Actions: Raise\n' +
>       '\t\t\t67 단추 확인 ID: 1\n' +
>       '\t\t\t68 단추 취소 ID: 2\n' +
>       '\t\t\t69 텍스트 선택한 항목이나 선택한 제목의 모든 항목을 인쇄할 수 있습니다. 어떻게 하시겠습니까? ID: 65535\n' +
>       '\t\t\t70 라디오 단추 (selectable) 선택한 항목 인쇄(S) ID: 3000\n' +
>       '\t\t\t71 라디오 단추 (selectable) 선택한 제목 및 모든 하위 항목 인쇄(P) ID: 3001\n' +
>       '\t\t\t72 제목 표시줄\n' +
>       '\t\t\t\t73 단추 닫기\n' +
>       '\t\t1 도구 모음 도구 모음 ID: 1006\n' +
>       '\t\t\t2 단추 숨기기\n' +
>       '\t\t\t3 단추 (disabled) 뒤로\n' +
>       '\t\t\t4 단추 (disabled) 전달\n' +
>       '\t\t\t5 단추 인쇄\n' +
>       '\t\t\t6 단추 옵션(O)\n' +
>       '\t\t7 창\n' +
>       '\t\t\t8 창\n' +
>       '\t\t\t\t9 창\n' +
>       '\t\t\t\t\t10 창 mk:@MSITStore:C:\\program%20files%20(x86)\\sigmatek\\lasal\\class2\\bin\\LASAL_CLASS_2_EN.chm::/introduction.htm\n' +
>       '\t\t\t\t\t\t11 창 INTRODUCTION\n' +
>       '\t\t\t\t\t\t\t12 TABLE ID: topicHeaderTable\n' +
>       '\t\t\t\t\t\t\t\t13 항목 ID: headerLogoCell\n' +
>       '\t\t\t\t\t\t\t\t\t14 링크\n' +
>       '\t\t\t\t\t\t\t\t\t\t15 이미지 SIGMATEK Description: SIGMATEK\n' +
>       '\t\t\t\t\t\t\t\t16 항목 Navigation: »No topics above this level« INTRODUCTION ID: headerContentsCell\n' +
>       '\t\t\t\t\t\t\t\t\t17 텍스트 Navigation:\n' +
>       '\t\t\t\t\t\t\t\t\t18 텍스트 »No topics above this level«\n' +
>       '\t\t\t\t\t\t\t\t\t19 텍스트 INTRODUCTION\n' +
>       '\t\t\t\t\t\t\t20 이미지 Topic functions menu Description: Topic functions menu ID: hmMenuLink\n' +
>       '\t\t\t\t\t\t\t21 텍스트 Menu\n' +
>       '\t\t\t\t\t\t\t22 이미지 This is the first topic Description: This is the first topic\n' +
>       '\t\t\t\t\t\t\t23 링크\n' +
>       '\t\t\t\t\t\t\t\t24 이미지 Go to parent chapter Description: Go to parent chapter ID: hmHomeLink\n' +
>       '\t\t\t\t\t\t\t25 링크\n' +
>       '\t\t\t\t\t\t\t\t26 이미지 Go to next topic Description: Go to next topic ID: hmNextLink\n' +
>       '\t\t\t\t\t\t\t27 텍스트 The new LASAL project software offers all the advantages of modern automation technology. The use of innovative program techniques significantly reduce the development time and makes possible simple and fast implementation of machine concepts. The addition of individual machine objects to LASAL simplifies communication between the constructor and the software designer.\n' +
>       '\t\t\t\t\t\t\t28 텍스트 See also:\n' +
>       '\t\t\t\t\t\t\t29 링크 All in One\n' +
>       '\t\t\t\t\t\t\t\t30 텍스트 All in One\n' +
>       '\t\t\t\t\t\t\t31 링크 LASAL supports several target platforms\n' +
>       '\t\t\t\t\t\t\t\t32 텍스트 LASAL supports several target platforms\n' +
>       '\t\t\t\t\t\t\t33 링크 Real time with a multilevel, multi-tasking system\n' +
>       '\t\t\t\t\t\t\t\t34 텍스트 Real time with a multilevel, multi-tasking system\n' +
>       '\t\t\t\t\t\t\t35 링크 Open data Interface\n' +
>       '\t\t\t\t\t\t\t\t36 텍스트 Open data Interface\n' +
>       '\t\t\t\t\t\t\t37 링크 Worldwide service interface\n' +
>       '\t\t\t\t\t\t\t\t38 텍스트 Worldwide service interface\n' +
>       '\t\t\t\t\t\t\t39 링크 Online help system\n' +
>       '\t\t\t\t\t\t\t\t40 텍스트 Online help system\n' +
>       '\t\t\t\t\t\t\t41 링크 Multilanguage programming tool\n' +
>       '\t\t\t\t\t\t\t\t42 텍스트 Multilanguage programming tool\n' +
>       '\t\t\t\t\t\t\t43 링크 Disclaimer\n' +
>       '\t\t\t\t\t\t\t\t44 텍스트 Disclaimer\n' +
>       '\t\t45 창\n' +
>       '\t\t46 창\n' +
>       '\t\t\t47 Tab ID: 1007\n' +
>       '\t\t\t\t48 트리\n' +
>       '\t\t\t\t\t49 콘솔 트리 (selectable, disabled) INTRODUCTION\n' +
>       '\t\t\t\t\t50 콘솔 트리 (selectable, disabled) THE DESIGN ENVIRONMENT\n' +
>       '\t\t\t\t\t51 콘솔 트리 (selectable, disabled) PROGRAMMING\n' +
>       '\t\t\t\t\t52 콘솔 트리 (selectable, disabled) ADDITIONAL VIEWS, TOOLS,...\n' +
>       '\t\t\t\t\t53 콘솔 트리 (selectable, disabled) TIPS AND HELPS FOR WORKING WITH LASAL CLASS 2\n' +
>       '\t\t\t\t\t54 콘솔 트리 (selectable, disabled) THE FIRST LASAL PROJECT\n' +
>       '\t\t\t\t\t55 콘솔 트리 (selectable, disabled) APPENDIX\n' +
>       '\t\t\t\t56 탭 항목 (selectable) 내용(C)\n' +
>       '\t\t\t\t57 탭 항목 (selectable) 색인(N)\n' +
>       '\t\t\t\t58 탭 항목 (selectable) 검색(S)\n' +
>       '\t\t\t\t59 탭 항목 (selectable) 즐겨찾기(I)\n' +
>       '\t\t60 제목 표시줄\n' +
>       '\t\t\t61 메뉴 모음 시스템 ID: MenuBar\n' +
>       '\t\t\t\t62 메뉴 항목 시스템\n' +
>       '\t\t\t63 단추 최소화\n' +
>       '\t\t\t64 단추 최대화\n' +
>       '\t\t\t65 단추 닫기\n' +
>       '\t\t66 대화 상자 항목 인쇄 Secondary Actions: Raise\n' +
>       '\t\t\t67 단추 확인 ID: 1\n' +
>       '\t\t\t68 단추 취소 ID: 2\n' +
>       '\t\t\t69 텍스트 선택한 항목이나 선택한 제목의 모든 항목을 인쇄할 수 있습니다. 어떻게 하시겠습니까? ID: 65535\n' +
>       '\t\t\t70 라디오 단추 (selectable) 선택한 항목 인쇄(S) ID: 3000\n' +
>       '\t\t\t71 라디오 단추 (selectable) 선택한 제목 및 모든 하위 항목 인쇄(P) ID: 3001\n' +
>       '\t\t\t72 제목 표시줄\n' +
>       '\t\t\t\t73 단추 닫기\n' +
>       '\n' +
>       'The focused UI element is 11 창 INTRODUCTION.',
>     focused_element: '11 창 INTRODUCTION'
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
>   "title": "LASAL 도움말 검색 열기",
>   "code": "await globalThis.sky.click({window:globalThis.helpWindow,x:912,y:503,screenshotId:'screenshot-1'}); await new Promise(r=>setTimeout(r,250)); await globalThis.sky.click({window:globalThis.helpWindow,x:165,y:96}); await new Promise(r=>setTimeout(r,350)); var helpState3 = await globalThis.sky.get_window_state({window:globalThis.helpWindow,include_screenshot:true,include_text:true}); nodeRepl.write(helpState3);"
> }
> ```
>
> ```text
> point (912, 503) is outside window bounds { originX: 0, originY: 0, width: 350, height: 205 }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 도움말 검색 열기",
>   "code": "await globalThis.sky.click({window:globalThis.helpWindow,x:291,y:175}); await new Promise(r=>setTimeout(r,300)); await globalThis.sky.click({window:globalThis.helpWindow,x:165,y:96}); await new Promise(r=>setTimeout(r,350)); var helpState4 = await globalThis.sky.get_window_state({window:globalThis.helpWindow,include_screenshot:true,include_text:true}); nodeRepl.write(helpState4);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 17242032,
>     title: 'LASAL Class 2 - Online help'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=27230 chars=9977 sha256=0a344c3d0f43b43cf1f400fa34d9c03d9dbb88a5a67fedebbf05252fa0173e47]'... 154495 more characters,
>       originX: 109,
>       originY: 51,
>       width: 1597,
>       height: 863
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=27239 chars=9977 sha256=839e68b7c63d1cb4beecb15f457ddb0eae8cee449181c43209f097517f9243e7]'... 8495 more characters,
>       originX: 732,
>       originY: 380,
>       width: 350,
>       height: 205
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "LASAL Class 2 - Online help", App: Lasal2.exe.\n' +
>       '\t0 창 (disabled) LASAL Class 2 - Online help Secondary Actions: Raise\n' +
>       '\t\t66 대화 상자 항목 인쇄 Secondary Actions: Raise\n' +
>       '\t\t\t67 단추 확인 ID: 1\n' +

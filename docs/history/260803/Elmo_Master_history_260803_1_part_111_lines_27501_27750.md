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
>   "title": "네트워크 연결 도움말 조회",
>   "code": "await globalThis.sky.click({window:globalThis.helpWindow,x:165,y:151,screenshotId:'screenshot-0'}); await globalThis.sky.press_key({window:globalThis.helpWindow,key:'CTRL+A'}); await globalThis.sky.type_text({window:globalThis.helpWindow,text:'connection'}); await globalThis.sky.press_key({window:globalThis.helpWindow,key:'ENTER'}); await new Promise(r=>setTimeout(r,900)); var helpState7 = await globalThis.sky.get_window_state({window:globalThis.helpWindow,include_screenshot:true,include_text:true}); nodeRepl.write(helpState7);"
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
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=27575 chars=9977 sha256=69575cecce959588f5498a8a9d4e33d8e5b5f783fdd86ea3ef4414215d1544ce]'... 222807 more characters,
>       originX: 109,
>       originY: 51,
>       width: 1597,
>       height: 863
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "LASAL Class 2 - Online help", App: Lasal2.exe.\n' +
>       '\t0 창 LASAL Class 2 - Online help Secondary Actions: Raise\n' +
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
>       '\t\t\t\t74 텍스트 검색할 단어를 입력하십시오(W). ID: 65535\n' +
>       '\t\t\t\t75 콤보 상자 검색할 단어를 입력하십시오(W). ID: 1001\n' +
>       '\t\t\t\t\t76 편집 검색할 단어를 입력하십시오(W). ID: 1001\n' +
>       '\t\t\t\t\t77 단추 열기 ID: DropDown\n' +
>       '\t\t\t\t78 단추 접속 검색 ID: 1010\n' +
>       '\t\t\t\t79 단추 항목 나열(L) ID: 1006\n' +
>       '\t\t\t\t80 단추 (disabled) 표시(D) ID: 1009\n' +
>       '\t\t\t\t81 텍스트 항목 선택(T): ID: 3030\n' +
>       '\t\t\t\t82 LIST (disabled) 항목 선택(T): ID: 3028\n' +
>       '\t\t\t\t\t83 머리글 헤더 컨트롤 ID: Header\n' +
>       '\t\t\t\t\t\t84 머리글 항목 제목 ID: HeaderItem 0\n' +
>       '\t\t\t\t\t\t85 머리글 항목 위치 ID: HeaderItem 1\n' +
>       '\t\t\t\t\t\t86 머리글 항목 순위 ID: HeaderItem 2\n' +
>       '\t\t\t\t87 확인란 이전 결과 검색(U) ID: 3056\n' +
>       '\t\t\t\t88 확인란 비슷한 단어 검색(M) ID: 1007\n' +
>       '\t\t\t\t89 확인란 제목만 검색(R) ID: 1008\n' +
>       '\t\t\t\t90 텍스트 찾음: 0 ID: 3031\n' +
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
>   "title": "클라이언트-서버 연결 도움말 확인",
>   "code": "await globalThis.sky.click({window:globalThis.helpWindow,x:95,y:428,screenshotId:'screenshot-0'}); await globalThis.sky.press_key({window:globalThis.helpWindow,key:'ENTER'}); await new Promise(r=>setTimeout(r,700)); var helpState8 = await globalThis.sky.get_window_state({window:globalThis.helpWindow,include_screenshot:true,include_text:true}); nodeRepl.write(helpState8);"
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
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=27690 chars=9977 sha256=d201b4a25269ed4a89601b807d015fccbf4c0effe4d13c956277456a4350992d]'... 170487 more characters,
>       originX: 109,
>       originY: 51,
>       width: 1597,
>       height: 863
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "LASAL Class 2 - Online help", App: Lasal2.exe.\n' +
>       '\t0 창 LASAL Class 2 - Online help Secondary Actions: Raise\n' +
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
>       '\t\t\t\t74 텍스트 검색할 단어를 입력하십시오(W). ID: 65535\n' +
>       '\t\t\t\t75 콤보 상자 검색할 단어를 입력하십시오(W). ID: 1001\n' +
>       '\t\t\t\t\t76 편집 검색할 단어를 입력하십시오(W). ID: 1001\n' +
>       '\t\t\t\t\t77 단추 열기 ID: DropDown\n' +

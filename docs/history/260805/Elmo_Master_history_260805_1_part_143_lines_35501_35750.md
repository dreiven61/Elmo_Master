> node_repl.js
>
> ```json
> {
>   "title": "프로젝트 파일 선택",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"Elmo_EtherCAT_Test_4Axis.lcp\"});"
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
>   "title": "정식 프로젝트 로드",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
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
>   "title": "프로젝트 로드 결과 확인",
>   "code": "await new Promise(r=>setTimeout(r,2000)); globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 788822,
>     title: 'Lasal Class 2 (02.03.001)'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=35551 chars=9977 sha256=21c6e704c5631fdaa4d9779b346c7467d6c5d38a9edba75b386dc3cccbcc914d]'... 178055 more characters,
>       originX: -1895,
>       originY: 22,
>       width: 970,
>       height: 512
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=35560 chars=9977 sha256=3413df86d24af8fcaaa50a83cf5b8990769e114329b8411a14a59d2bfbd28a1c]'... 172147 more characters,
>       originX: -1890,
>       originY: 27,
>       width: 960,
>       height: 502
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
>       '\t\t2 상태 표시줄 ID: 59393\n' +
>       '\t\t\t20734 진행률 표시줄 ID: 182692360\n' +
>       '\t\t\t3 텍스트 \n' +
>       '\t\t\t4 텍스트 Compiling\n' +
>       '\t\t\t5 텍스트\n' +
>       '\t\t\t6 텍스트 \n' +
>       '\t\t\t7 텍스트\n' +
>       '\t\t\t8 텍스트 \n' +
>       '\t\t\t9 텍스트 \n' +
>       '\t\t\t10 텍스트 Offline\n' +
>       '\t\t\t11 텍스트\n' +
>       '\t\t\t20735 텍스트 NUM\n' +
>       '\t\t\t20736 텍스트\n' +
>       '\t\t12 창 xtpBarTop ID: 59419\n' +
>       '\t\t\t13 도구 모음 Edit\n' +
>       '\t\t\t\t20857 단추 (disabled) Toggle bookmark\n' +
>       '\t\t\t\t20858 단추 (disabled) Previous bookmark\n' +
>       '\t\t\t\t20859 단추 (disabled) Next bookmark\n' +
>       '\t\t\t\t20860 단추 (disabled) Delete all bookmarks\n' +
>       '\t\t\t\t20861 단추 (disabled) Previous bookmark in this file\n' +
>       '\t\t\t\t20862 단추 (disabled) Next bookmark in this file\n' +
>       '\t\t\t\t20863 단추 (disabled) Comment selected text (Ctrl+Shift+C)\n' +
>       '\t\t\t\t20864 단추 (disabled) Remove comment (Ctrl+Shift+X)\n' +
>       '\t\t\t\t20865 단추 (disabled) Unindent (Shift+Tab)\n' +
>       '\t\t\t\t20866 단추 (disabled) Indent (Tab)\n' +
>       '\t\t\t24 도구 모음 Macros Manager\n' +
>       '\t\t\t\t20867 메뉴 항목 Macros\n' +
>       '\t\t\t26 도구 모음 Layout Manager\n' +
>       '\t\t\t\t20868 메뉴 항목 Layouts\n' +
>       '\t\t\t28 도구 모음 Toolbox\n' +
>       '\t\t\t\t20869 단추 DataAnalyzer\n' +
>       '\t\t\t\t20870 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t31 도구 모음 Net Edit\n' +
>       '\t\t\t\t20871 단추 (disabled) Select\n' +
>       '\t\t\t\t20872 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t34 도구 모음 Debug\n' +
>       '\t\t\t\t20873 단추 (disabled) Go online (Alt+F6)\n' +
>       '\t\t\t\t20874 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t37 도구 모음 Build\n' +
>       '\t\t\t\t20875 메뉴 항목 (disabled) Target Architecture\n' +
>       '\t\t\t\t20876 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t40 도구 모음 Standard\n' +
>       '\t\t\t\t20877 단추 (disabled) New project (Strg+N)\n' +
>       '\t\t\t\t20878 단추 Open a file (Strg+Shift+O)\n' +
>       '\t\t\t\t20879 단추 (disabled) Close active document (Strg+F4)\n' +
>       '\t\t\t\t20880 단추 (disabled) Save file (Strg+S)\n' +
>       '\t\t\t\t20881 단추 (disabled) Open project (Strg+O)\n' +
>       '\t\t\t\t20882 단추 (disabled) Save project changes (Strg+Shift+S)\n' +
>       '\t\t\t\t20883 단추 (disabled) Close project\n' +
>       '\t\t\t\t20884 단추 (disabled) Print\n' +
>       '\t\t\t\t20885 단추 Cut (Strg+X)\n' +
>       '\t\t\t\t20886 단추 Copy (Strg+C)\n' +
>       '\t\t\t\t20887 단추 Paste (Strg+V)\n' +
>       '\t\t\t\t20888 메뉴 항목 (disabled) Undo (Strg+Z)\n' +
>       '\t\t\t\t20889 메뉴 항목 (disabled) Redo (Strg+Y)\n' +
>       '\t\t\t\t20890 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t55 메뉴 모음 Menu Bar\n' +
>       '\t\t\t\t20891 메뉴 항목 FILE\n' +
>       '\t\t\t\t20892 메뉴 항목 EDIT\n' +
>       '\t\t\t\t20893 메뉴 항목 VIEW\n' +
>       '\t\t\t\t20894 메뉴 항목 PROJECT\n' +
>       '\t\t\t\t20895 메뉴 항목 BUILD\n' +
>       '\t\t\t\t20896 메뉴 항목 DEBUG\n' +
>       '\t\t\t\t20897 메뉴 항목 ANALYZE\n' +
>       '\t\t\t\t20898 메뉴 항목 TOOLS\n' +
>       '\t\t\t\t20899 메뉴 항목 EXTRAS\n' +
>       '\t\t\t\t20900 메뉴 항목 WINDOW\n' +
>       '\t\t\t\t20901 메뉴 항목 HELP\n' +
>       '\t\t67 창 Splitter ID: 381326624\n' +
>       '\t\t68 창 Splitter ID: 381325280\n' +
>       '\t\t69 Tab Output ID: 274980984\n' +
>       '\t\t\t70 창 ID: 1200\n' +
>       '\t\t\t\t71 창 ID: 1200\n' +
>       '\t\t\t\t\t72 LIST ID: 1201\n' +
>       '\t\t\t\t\t\t20391 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t20392 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t20393 단추 페이지 위로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t\t20394 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t20395 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t20282 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20336 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20337 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20396 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20397 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20398 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20399 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20400 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20401 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20564 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20618 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20619 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20782 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20783 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20837 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20838 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20839 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20840 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20841 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20842 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20843 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20844 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20845 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20846 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20847 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20848 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20902 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20903 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20904 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20905 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20906 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20907 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20908 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20909 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20910 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20911 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20912 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20913 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20914 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20915 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20916 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20917 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20918 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20919 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20920 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20921 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20922 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20923 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20924 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20925 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20926 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20927 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20928 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20929 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20930 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20931 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20932 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20933 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20934 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20935 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20936 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20937 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20938 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20939 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20940 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20941 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20942 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20943 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20944 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20945 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20946 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20947 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20948 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20949 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20950 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t20951 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t73 스크롤 막대 ID: 59904\n' +
>       '\t\t\t\t\t\t74 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t75 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t76 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t20952 탭 항목 (selectable) Python Script\n' +
>       '\t\t\t20953 탭 항목 (selectable) Debugger\n' +
>       '\t\t\t20954 탭 항목 (selectable) Output\n' +
>       '\t\t\t80 단추 Close\n' +
>       '\t\t81 창 Splitter ID: 381325112\n' +
>       '\t\t82 Tab Class View ID: 274978704\n' +
>       '\t\t\t83 트리 (disabled) ID: 103\n' +
>       '\t\t\t20955 탭 항목 (selectable) Lib\n' +
>       '\t\t\t20956 탭 항목 (selectable) File\n' +
>       '\t\t\t20957 탭 항목 (selectable) Class\n' +
>       '\t\t\t20958 탭 항목 (selectable) Global\n' +
>       '\t\t\t88 단추 Close\n' +
>       '\t\t89 Tab Properties ID: 274980528\n' +
>       '\t\t\t90 창 (disabled) ID: 262805440\n' +
>       '\t\t\t\t91 TABLE Properties Window ID: 272651424\n' +
>       '\t\t\t\t92 도구 모음 ID: 59392\n' +
>       '\t\t\t\t\t93 단추\n' +
>       '\t\t\t\t\t94 단추\n' +
>       '\t\t\t20959 탭 항목 (selectable) Properties\n' +
>       '\t\t\t96 단추 Close\n' +
>       '\n' +

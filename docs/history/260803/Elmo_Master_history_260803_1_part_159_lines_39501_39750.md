>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\CANopen Architect Mini\\CANopenArchitectMini.exe',
>     windows: [],
>     displayName: 'CANopen Architect Mini',
>     lastUsedDate: '2026-06-04',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: 'WelconServoStudio.AppID.NoVersion',
>     windows: [],
>     displayName: 'WELSS.exe',
>     lastUsedDate: '2026-06-04',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\POWERPNT.EXE',
>     windows: [],
>     displayName: 'PowerPoint 2016',
>     lastUsedDate: '2026-05-13',
>     useCount: 4,
>     isRunning: false
>   },
>   {
>     id: 'AcrobatReader',
>     windows: [],
>     displayName: 'Adobe Acrobat',
>     lastUsedDate: '2026-05-06',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Advanced IP Scanner\\advanced_ip_scanner.exe',
>     windows: [],
>     displayName: 'Advanced IP Scanner',
>     lastUsedDate: '2026-04-07',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: 'C:\\Qt\\Tools\\QtCreator\\bin\\qtcreator.exe',
>     windows: [],
>     displayName: 'Qt Creator 18.0.1 (Community)',
>     lastUsedDate: '2026-04-07',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: 'C:\\Users\\dreiv\\eclipse\\java-2023-09\\eclipse\\eclipse.exe',
>     windows: [],
>     displayName: 'Eclipse IDE for Java Developers - 2023-09',
>     lastUsedDate: '2026-04-03',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: 'C:\\ProgramData\\MDS6\\MDS Installation Guide.pdf',
>     windows: [],
>     displayName: 'MDS Installation Guide',
>     lastUsedDate: '2026-04-03',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\WSL\\wslsettings\\wslsettings.exe',
>     windows: [],
>     displayName: 'WSL Settings',
>     lastUsedDate: '2026-04-03',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX Cam Designer\\3.6.0\\ctrlX-CamDesigner.exe',
>     windows: [],
>     displayName: 'ctrlX Cam Designer 3.6.1',
>     lastUsedDate: '2026-03-13',
>     useCount: 0,
>     isRunning: false
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
>   "title": "LASAL IDE 재실행",
>   "code": "var lasalApp12 = lasalApps12.find(a=>a.id.includes(\"Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\")); var launchLasal12 = await sky.launch_app({app:lasalApp12}); nodeRepl.write(launchLasal12);"
> }
> ```
>
> ```text
> app is required
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 재실행",
>   "code": "var launchLasal13 = await sky.launch_app({app:lasalApp12.id}); nodeRepl.write(launchLasal13);"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 프로젝트 열림 확인",
>   "code": "await new Promise(r=>setTimeout(r,2500)); var windowsLasal13 = await sky.list_windows({app:lasalApp12.id}); nodeRepl.write(windowsLasal13);"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 4790364,
>     title: 'Lasal Class 2 (02.03.001)'
>   },
>   {
>     app: 'process:C:\\Program Files\\WindowsApps\\OpenAI.Codex_26.721.11231.0_x64__2p2nqsd0c76g0\\app\\ChatGPT.exe',
>     id: 394022,
>     title: 'ChatGPT'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe',
>     id: 854086,
>     title: 'LasalApiWpfTestApp - Microsoft Visual Studio'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe',
>     id: 16322664,
>     title: '토스인컴'
>   },
>   {
>     app: 'process:C:\\Windows\\explorer.exe',
>     id: 984726,
>     title: 'work - 파일 탐색기'
>   },
>   {
>     app: 'process:C:\\Windows\\explorer.exe',
>     id: 3147398,
>     title: 'Debug - 파일 탐색기 [ main ↓0 ↑17 ]'
>   },
>   {
>     app: 'process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
>     id: 1247078,
>     title: 'Daou Office - 메일 - Chrome'
>   },
>   {
>     app: 'process:C:\\Windows\\explorer.exe',
>     id: 464146,
>     title: 'Elmo_EtherCAT_Test_4Axis_backup - 파일 탐색기'
>   },
>   {
>     app: 'process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
>     id: 10293482,
>     title: 'Repositories :: Bonobo Git Server - Chrome'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe',
>     id: 22747026,
>     title: '서보스타 전직원 단톡방'
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
>   "title": "LASAL 프로젝트 열기",
>   "code": "var lasalWindow13 = windowsLasal13.find(w=>w.title===\"Lasal Class 2 (02.03.001)\"); await sky.press_key({window:lasalWindow13,key:\"CTRL+O\"});"
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
>   "title": "LASAL 프로젝트 경로 입력",
>   "code": "await new Promise(r=>setTimeout(r,500)); var openState13 = await sky.get_window_state({window:lasalWindow13,include_text:true}); nodeRepl.write(openState13.accessibility);"
> }
> ```
>
> ```text
> null
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
>   "title": "LASAL 프로젝트 열기",
>   "code": "await sky.type_text({window:lasalWindow13,text:\"Elmo_EtherCAT_Test_4Axis.lcp\"}); await sky.press_key({window:lasalWindow13,key:\"ENTER\"});"
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
>   "title": "LASAL 프로젝트 로딩 확인",
>   "code": "await new Promise(r=>setTimeout(r,3000)); var windowsLasal14 = await sky.list_windows({app:lasalApp12.id}); nodeRepl.write(windowsLasal14.filter(w=>w.title&&w.title.includes(\"Lasal Class 2\")));"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 4790364,
>     title: 'Lasal Class 2 (02.03.001)'
>   }
> ]
> ```

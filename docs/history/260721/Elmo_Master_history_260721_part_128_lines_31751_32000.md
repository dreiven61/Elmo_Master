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
>   "code": "var winList2=await sky.list_windows(); winList2",
>   "title": "열린 창 목록"
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
>   "code": "\nsky2 = await import(\"computer-use\");\napps2 = await sky2.list_apps();\napps2\n",
>   "title": "Computer Use 앱 목록 새로고침"
> }
> ```
>
> ```text
> Module not found: computer-use
> ```
>
> MCP tool call
>
> node_repl.js_reset
>
> ```json
> {}
> ```
>
> ```text
> js kernel reset
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nvar modCU = await import(\"file:///C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.715.52143/scripts/computer-use-client.mjs\");\nawait modCU.setupComputerUseRuntime({globals:globalThis});\nvar appsCU = await sky.list_apps();\nnodeRepl.write(appsCU.map(a=>({displayName:a.displayName,id:a.id,windows:a.windows})));\n",
>   "title": "Computer Use 런타임 재초기화"
> }
> ```
>
> ```text
> [
>   {
>     displayName: 'Google Chrome',
>     id: 'Chrome',
>     windows: [ { app: 'Chrome', id: 396542, title: 'ChatGPT - Chrome' } ]
>   },
>   {
>     displayName: 'ChatGPT',
>     id: 'OpenAI.Codex_2p2nqsd0c76g0!App',
>     windows: [
>       {
>         app: 'OpenAI.Codex_2p2nqsd0c76g0!App',
>         id: 132772,
>         title: 'ChatGPT'
>       }
>     ]
>   },
>   {
>     displayName: 'Visual Studio 2019',
>     id: 'VisualStudio.89931261',
>     windows: [
>       {
>         app: 'VisualStudio.89931261',
>         id: 68064,
>         title: 'MMCLibDotNET_Test - Microsoft Visual Studio'
>       },
>       {
>         app: 'VisualStudio.89931261',
>         id: 68068,
>         title: 'PmasApiWpfTestApp - Microsoft Visual Studio'
>       },
>       {
>         app: 'VisualStudio.89931261',
>         id: 68062,
>         title: 'LasalApiWpfTestApp - Microsoft Visual Studio'
>       }
>     ]
>   },
>   {
>     displayName: 'Excel 2016',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\EXCEL.EXE',
>     windows: [
>       {
>         app: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\EXCEL.EXE',
>         id: 67542,
>         title: 'EtherCAT Controller(Master) 요구 사양.xlsx  [제한된 보기] [사용자가 마지막으로 저장함] - Excel'
>       },
>       {
>         app: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\EXCEL.EXE',
>         id: 67890,
>         title: 'Motion_Controller_API_정리.xlsx  [제한된 보기] [사용자가 마지막으로 저장함] - Excel'
>       }
>     ]
>   },
>   {
>     displayName: '카카오톡',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Kakao\\KakaoTalk\\KakaoTalk.exe',
>     windows: [
>       {
>         app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Kakao\\KakaoTalk\\KakaoTalk.exe',
>         id: 132936,
>         title: '카카오톡'
>       }
>     ]
>   },
>   {
>     displayName: 'LASAL Class 2',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     windows: []
>   },
>   {
>     displayName: 'LASAL Class 2',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe',
>     windows: []
>   },
>   {
>     displayName: 'Elmo Application Studio II 64bit',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Elmo Motion Control\\Elmo Application Studio II\\ElmoMotionControl.View.Main.exe',
>     windows: []
>   },
>   {
>     displayName: 'Word 2016',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\WINWORD.EXE',
>     windows: []
>   },
>   {
>     displayName: '꿀뷰',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Honeyview\\Honeyview.exe',
>     windows: []
>   },
>   {
>     displayName: 'IP-Mate',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\IPMate\\ipmate.exe',
>     windows: []
>   },
>   {
>     displayName: 'Notepad++',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Notepad++\\notepad++.exe',
>     windows: []
>   },
>   {
>     displayName: 'Wireshark',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Wireshark\\Wireshark.exe',
>     windows: []
>   },
>   {
>     displayName: 'picpick',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\PicPick\\picpick.exe',
>     windows: []
>   },
>   {
>     displayName: 'MDS User Guide',
>     id: 'C:\\ProgramData\\MDS6\\MDS_UM6.pdf',
>     windows: []
>   },
>   {
>     displayName: 'LASAL Safety Designer',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Safety Designer\\bin\\SafetyDesigner.exe',
>     windows: []
>   },
>   {
>     displayName: 'ctrlX DRIVE Engineering 01V30.0',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\ctrlX WORKS\\ctrlX DRIVE Engineering\\0130\\cxde.exe',
>     windows: []
>   },
>   {
>     displayName: 'ctrlX IO Engineering 3.6.3',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX IO Engineering\\3.6.3\\Studio\\Common\\ctrlX-IO-Engineering.exe',
>     windows: []
>   },
>   {
>     displayName: 'ctrlX PLC Engineering 3.6.3',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX PLC Engineering\\3.6.3\\Studio\\Common\\ctrlX-PLC-Engineering.exe',
>     windows: []
>   },
>   {
>     displayName: 'ctrlX WORKS 3.6.2',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX WORKS\\3.6.2\\ctrlx-works.exe',
>     windows: []
>   },
>   {
>     displayName: 'LASAL Screen Modeless',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Screen\\LSE_Modeless.exe',
>     windows: []
>   },
>   {
>     displayName: 'LASAL Screen Modeless',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Screen\\LSE_Modeless.exe',
>     windows: []
>   },
>   {
>     displayName: 'LASAL MachineManager',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\MachineManager\\Bin\\MachineManager.exe',
>     windows: []
>   },
>   {
>     displayName: 'LASAL MachineManager',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\MachineManager\\Bin\\MachineManager.exe',
>     windows: []
>   },
>   {
>     displayName: 'LARSConfigTool',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lars\\LARSConfigTool.exe',
>     windows: []
>   },
>   {
>     displayName: 'Freeplane',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Freeplane\\freeplane.exe',
>     windows: []
>   },
>   {
>     displayName: 'CANopen Architect Mini',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\CANopen Architect Mini\\CANopenArchitectMini.exe',
>     windows: []
>   },
>   {
>     displayName: 'WELSS.exe',
>     id: 'WelconServoStudio.AppID.NoVersion',
>     windows: []
>   },
>   {
>     displayName: 'Lars',
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lars\\Lars.exe',
>     windows: []
>   },
>   {
>     displayName: 'PowerPoint 2016',
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\POWERPNT.EXE',
>     windows: []
>   },
>   { displayName: 'Adobe Acrobat', id: 'AcrobatReader', windows: [] },
>   {

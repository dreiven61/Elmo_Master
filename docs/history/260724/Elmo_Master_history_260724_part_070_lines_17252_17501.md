> 					165 콘솔 트리 (selectable) External
> 					166 콘솔 트리 (selectable) Sigmatek
> 					167 콘솔 트리 (selectable) _TCPIPServer_RT
> 					168 콘솔 트리 (selectable) Elmo_1
> 					169 콘솔 트리 (selectable) Elmo_2
> 					170 콘솔 트리 (selectable) Elmo_3
> 					171 콘솔 트리 (selectable) Elmo_4
> 					172 콘솔 트리 (selectable) LMCDiagnosticsService
> 					173 콘솔 트리 (selectable) LMCEcatInputLatch
> 					174 콘솔 트리 (selectable) LMCRecorderStore
> 					175 콘솔 트리 (selectable) LMCSdoExecutor
> 					176 콘솔 트리 (selectable) TCPMotionInterface
> 			177 탭 항목 (selectable) Lib
> 			178 탭 항목 (selectable) File
> 			179 탭 항목 (selectable) Global
> 			180 탭 항목 (selectable) Net
> 			181 탭 항목 (selectable) Class
> 			182 단추 Close
> 		183 Tab Properties ID: 895540320
> 			184 창 ID: 288524952
> 				185 TABLE Properties Window ID: 292730376
> 					186 custom Name
> 					187 custom Class
> 					188 custom DataType
> 					189 custom Type
> 					190 custom Required
> 					191 custom DefaultInitValue
> 					192 custom Disable RedefinitionVariableWarning
> 					193 custom Comment
> 				194 도구 모음 ID: 59392
> 					195 단추
> 					196 단추
> 			197 탭 항목 (selectable) Properties
> 			198 단추 Close
>
> The focused UI element is 3 창 ID: 59648.
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
>   "title": "LASAL 결과 더블클릭 방법 확인",
>   "code": "var apiDocNow = await sky.documentation('api'); var doubleIdxNow = apiDocNow.indexOf('double_click'); nodeRepl.write(apiDocNow.slice(Math.max(0,doubleIdxNow-700),doubleIdxNow+1300));"
> }
> ```
>
> ````text
> ## API Reference
>
> # Sky Window2 API
>
> ## API Reference
>
> Use this as the supported `sky` window2 API surface.
>
> ```ts
> import { sky } from "@oai/sky";
>
> const apps = await sky.list_apps();
> const candidate_windows = apps.flatMap((app) => app.windows);
> // Choose the task-specific app and window before acting.
> // Each input action takes the specific Window for that action.
>
> interface Window2ComputerUseClient {
>   list_windows(): Promise<Array<Window>>; // List open windows that can be targeted by the window2 API.
>   get_window(input: GetWindowInput): Promise<Window>; // Rehydrate a currently open window by id; useful after losing a window binding.
>   list_apps(): Promise<Array<ListAppsApp>>; // List installed apps, including their currently open targetable windows when present.
>   launch_app(input: LaunchAppInput): Promise<void>; // Launch an app by id so its window can later be selected from `list_apps()`.
>   get_window_state(input: GetWindowStateInput): Promise<WindowState>; // Capture selected state for an open window.
>   click(input: ClickInput): Promise<void>; // Click either an indexed element from the latest window state or a coordinate in the window.
>   press_key(input: PressKeyInput): Promise<void>; // Press a `+`-separated keyboard chord in a window.
>   type_text
> ````
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "검색 결과 첫 행 선택",
>   "code": "{ var observation6 = globalThis.state; var screenshotId6 = observation6.screenshots?.find(s=>s.zIndex===0)?.id || observation6.screenshots?.[0]?.id; globalThis.state=null; await sky.click({window:observation6.window,screenshotId:screenshotId6,x:92,y:246}); globalThis.state=await sky.get_window_state({window:observation6.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=globalThis.state.window; nodeRepl.write(JSON.stringify({focused:globalThis.state.accessibility?.focused_element,selected:globalThis.state.accessibility?.selected_elements,selectedText:globalThis.state.accessibility?.selected_text},null,2)); }"
> }
> ```
>
> ```text
> {
>   "focused": "3 창 ID: 59648"
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
>   "title": "검색 결과에서 소스 라인 이동 시험",
>   "code": "{ var observation7=globalThis.state; globalThis.state=null; await sky.press_key({window:observation7.window,key:'Enter'}); globalThis.state=await sky.get_window_state({window:observation7.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=globalThis.state.window; nodeRepl.write(JSON.stringify({title:globalThis.state.window.title,focused:globalThis.state.accessibility?.focused_element,selectedText:globalThis.state.accessibility?.selected_text},null,2)); }"
> }
> ```
>
> ```text
> {
>   "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network"
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 가능해야 합니다. 방금 현재 결과를 직접 열어 보니 지금 상태는 “파일 전체가 결과 1건으로 묶이고 Enter 시 1행으로 이동”하므로, 사용자가 원하는 기존의 행 단위 탐색과 다른 상태가 맞습니다. 배치된 네트워크 채널에서 실행해서 생긴 차이인지, LASAL의 소스 인덱스가 깨진 것인지 두 갈래로 바로 확인하겠습니다. 소스 편집이나 저장은 하지 않겠습니다.
>
> 현재까지 확인된 중요한 점은 두 가지입니다.
>
> - 저장소의 정상 기준 자체가 “실제 소스 경로와 행 번호 표시 → 결과를 열면 해당 구현 행으로 이동”입니다.
> - 지금 보인 화면은 그 기준을 충족하지 않습니다. 다만 결과 묶음의 헤더를 연 것인지, 실제 일치 항목이 아래에 따로 있는지까지 구분해야 하므로 키보드 이동과 클래스 정의 쪽 검색을 각각 재현하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 행 단위 탐색 확인 준비",
>   "code": "if (!globalThis.sky) {\n  const { setupComputerUseRuntime } = await import(\"C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.715.72359/scripts/computer-use-client.mjs\");\n  await setupComputerUseRuntime({ globals: globalThis });\n}\nglobalThis.cuGuidance = await sky.documentation(\"guidance\");\nglobalThis.cuConfirmations = await sky.documentation(\"confirmations\");\nnodeRepl.write(\"Computer Use ready; guidance and confirmations loaded.\");"
> }
> ```
>
> ```text
> Computer Use ready; guidance and confirmations loaded.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "현재 LASAL 화면 확인",
>   "code": "globalThis.apps = await sky.list_apps();\nglobalThis.lasalAppsNow = apps.filter(a => /lasal.*class|lasal2/i.test(`${a.id} ${a.displayName ?? \"\"}`) && a.windows?.length);\nnodeRepl.write(JSON.stringify(lasalAppsNow, null, 2));"
> }
> ```
>
> ```text
> [
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "windows": [
>       {
>         "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>         "id": 4063414,
>         "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface.st"
>       }
>     ],
>     "displayName": "LASAL Class 2",
>     "lastUsedDate": "2026-07-23",
>     "useCount": 15,
>     "isRunning": true
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>     "windows": [
>       {
>         "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>         "id": 4063414,
>         "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface.st"
>       }
>     ],
>     "displayName": "LASAL Class 2",
>     "lastUsedDate": "2026-07-23",
>     "useCount": 15,
>     "isRunning": true
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
>   "title": "Find Results 현재 상태 읽기",
>   "code": "globalThis.targetApp = lasalAppsNow[0];\nglobalThis.targetWindow = await sky.get_window(targetApp.windows[0]);\nawait sky.activate_window({ window: targetWindow });\nglobalThis.state = await sky.get_window_state({ window: targetWindow, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = state.window;\n{\n const a = state.accessibility;\n const lines = (a?.tree || \"\").split(\"\\n\");\n const filtered = lines.filter(x => /Find Results|Total found|Matched Files|TCPMotionInterface\\.st|Ln \\d+ Col|LIST ID: 1204|LMCAxis3/i.test(x)).slice(-100);\n nodeRepl.write(JSON.stringify({focused:a?.focused_element, selected:a?.selected_text, selectedElements:a?.selected_elements, filtered}, null, 2));\n}"
> }
> ```
>
> ```text
> {
>   "focused": "4 창 //This file was generated by the LASAL2 CodeGenerator -- //Please, do not edit this file (it might be overwritten by the next generator run) //{{LSL_DECLARATION (*! <Class Name = \"TCPMotionInterface\" Revision = \"0.0\" GUID = \"{C9B663E2-7D2C-462A-B738-8FDD7B099E2F}\" RealtimeTask = \"false\" CyclicTask = \"true\" DefCyclictime = \"1 ms\" BackgroundTask = \"false\" Sigmatek = \"false\" OSInterface = \"false\" HighPriority = \"false\" Automatic = \"false\" UpdateMode = \"Prescan\" SharedCommandTable = \"true\" Objectsize = \"(536,120)\"> <Channels> <Server Name=\"acc\" GUID=\"{EF724BEA-AF9C-43E5-BDC6-0FAD76A9AD08}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"AxisRef\" GUID=\"{99145B0B-4F1C-4C52-9705-9F801FE1A3A1}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"bufMode\" GUID=\"{8B602708-A478-435C-A43F-473F29186A2C}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"CommandID\" GUID=\"{F8B2658C-8914-4808-B041-775825E501E8}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"CurrentSock\" GUID=\"{1F419127-A1E0-44AA-AC32-A2CDCE9841DF}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"dec\" GUID=\"{1A1BE933-EAD9-4A81-81E3-0CCB0EA2985F}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"dir\" GUID=\"{12F08241-03C5-4833-8272-1C429686E36C}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Exec\" GUID=\"{F862445A-42BB-4FB4-AF61-5CA82DB86CF7}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"jer\" GUID=\"{3AEE5633-5554-427D-92B5-249F061C3F20}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Payload\" GUID=\"{722817E8-BD3B-4762-B3E4-AD743AEBA249}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"pos\" GUID=\"{F431AAC4-369D-4228-812D-07B6ADE82C30}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Power\" GUID=\"{130E4A27-675A-41AC-AB89-829ABE2D8CBA}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"ReadPos\" GUID=\"{44E93FA6-636B-4EA8-A3A0-BDCB89CF8B79}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Reserved\" GUID=\"{F20FEA26-1B51-4AB0-81CB-3B30D508F31F}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"RetCode\" GUID=\"{C427A968-CED5-49C0-BFDE-AC2143C688EC}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"RobotLock\" GUID=\"{C73AB6B8-68DE-4192-ABA4-2D32F5D8D566}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"RobotPowerOff\" GUID=\"{45163924-14EF-4B60-9C3B-364662E53787}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"RobotPowerOn\" GUID=\"{DE939C5A-E281-44F8-9661-45FB4988066B}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"RobotUnLock\" GUID=\"{ADED8B18-F205-4D23-9D6E-A966DCA5605E}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"velo\" GUID=\"{62BB909C-5656-4232-B799-BE918E89FDFD}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Client Name=\"_StdLib\" Required=\"false\" Internal=\"false\"/> <Client Name=\"Diagnostics\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis1\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis2\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis3\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis4\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis5\" Required=\"true\" Internal=\" ID: 10000",
>   "filtered": [
>     "Window: \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface.st\", App: Lasal2.exe.",
>     "\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface.st Secondary Actions: Raise",
>     "\t\t\t2 창 TCPMotionInterface.st Secondary Actions: Raise ID: 65285",
>     "\t\t\t\t\t4 창 //This file was generated by the LASAL2 CodeGenerator -- //Please, do not edit this file (it might be overwritten by the next generator run) //{{LSL_DECLARATION (*! <Class Name = \"TCPMotionInterface\" Revision = \"0.0\" GUID = \"{C9B663E2-7D2C-462A-B738-8FDD7B099E2F}\" RealtimeTask = \"false\" CyclicTask = \"true\" DefCyclictime = \"1 ms\" BackgroundTask = \"false\" Sigmatek = \"false\" OSInterface = \"false\" HighPriority = \"false\" Automatic = \"false\" UpdateMode = \"Prescan\" SharedCommandTable = \"true\" Objectsize = \"(536,120)\"> <Channels> <Server Name=\"acc\" GUID=\"{EF724BEA-AF9C-43E5-BDC6-0FAD76A9AD08}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"AxisRef\" GUID=\"{99145B0B-4F1C-4C52-9705-9F801FE1A3A1}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"bufMode\" GUID=\"{8B602708-A478-435C-A43F-473F29186A2C}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"CommandID\" GUID=\"{F8B2658C-8914-4808-B041-775825E501E8}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"CurrentSock\" GUID=\"{1F419127-A1E0-44AA-AC32-A2CDCE9841DF}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"dec\" GUID=\"{1A1BE933-EAD9-4A81-81E3-0CCB0EA2985F}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"dir\" GUID=\"{12F08241-03C5-4833-8272-1C429686E36C}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Exec\" GUID=\"{F862445A-42BB-4FB4-AF61-5CA82DB86CF7}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"jer\" GUID=\"{3AEE5633-5554-427D-92B5-249F061C3F20}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Payload\" GUID=\"{722817E8-BD3B-4762-B3E4-AD743AEBA249}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"pos\" GUID=\"{F431AAC4-369D-4228-812D-07B6ADE82C30}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Power\" GUID=\"{130E4A27-675A-41AC-AB89-829ABE2D8CBA}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"ReadPos\" GUID=\"{44E93FA6-636B-4EA8-A3A0-BDCB89CF8B79}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Reserved\" GUID=\"{F20FEA26-1B51-4AB0-81CB-3B30D508F31F}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"RetCode\" GUID=\"{C427A968-CED5-49C0-BFDE-AC2143C688EC}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"RobotLock\" GUID=\"{C73AB6B8-68DE-4192-ABA4-2D32F5D8D566}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"RobotPowerOff\" GUID=\"{45163924-14EF-4B60-9C3B-364662E53787}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"RobotPowerOn\" GUID=\"{DE939C5A-E281-44F8-9661-45FB4988066B}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"RobotUnLock\" GUID=\"{ADED8B18-F205-4D23-9D6E-A966DCA5605E}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"velo\" GUID=\"{62BB909C-5656-4232-B799-BE918E89FDFD}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Client Name=\"_StdLib\" Required=\"false\" Internal=\"false\"/> <Client Name=\"Diagnostics\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis1\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis2\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis3\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis4\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis5\" Required=\"true\" Internal=\" ID: 10000",
>     "\t\t\t81 텍스트 Ln 1 Col 1",
>     "\t\t\t\t\t162 LIST ID: 1204",
>     "The focused UI element is 4 창 //This file was generated by the LASAL2 CodeGenerator -- //Please, do not edit this file (it might be overwritten by the next generator run) //{{LSL_DECLARATION (*! <Class Name = \"TCPMotionInterface\" Revision = \"0.0\" GUID = \"{C9B663E2-7D2C-462A-B738-8FDD7B099E2F}\" RealtimeTask = \"false\" CyclicTask = \"true\" DefCyclictime = \"1 ms\" BackgroundTask = \"false\" Sigmatek = \"false\" OSInterface = \"false\" HighPriority = \"false\" Automatic = \"false\" UpdateMode = \"Prescan\" SharedCommandTable = \"true\" Objectsize = \"(536,120)\"> <Channels> <Server Name=\"acc\" GUID=\"{EF724BEA-AF9C-43E5-BDC6-0FAD76A9AD08}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"AxisRef\" GUID=\"{99145B0B-4F1C-4C52-9705-9F801FE1A3A1}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"bufMode\" GUID=\"{8B602708-A478-435C-A43F-473F29186A2C}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"CommandID\" GUID=\"{F8B2658C-8914-4808-B041-775825E501E8}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"CurrentSock\" GUID=\"{1F419127-A1E0-44AA-AC32-A2CDCE9841DF}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"dec\" GUID=\"{1A1BE933-EAD9-4A81-81E3-0CCB0EA2985F}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"dir\" GUID=\"{12F08241-03C5-4833-8272-1C429686E36C}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Exec\" GUID=\"{F862445A-42BB-4FB4-AF61-5CA82DB86CF7}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"jer\" GUID=\"{3AEE5633-5554-427D-92B5-249F061C3F20}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Payload\" GUID=\"{722817E8-BD3B-4762-B3E4-AD743AEBA249}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"pos\" GUID=\"{F431AAC4-369D-4228-812D-07B6ADE82C30}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Power\" GUID=\"{130E4A27-675A-41AC-AB89-829ABE2D8CBA}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"ReadPos\" GUID=\"{44E93FA6-636B-4EA8-A3A0-BDCB89CF8B79}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"Reserved\" GUID=\"{F20FEA26-1B51-4AB0-81CB-3B30D508F31F}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"RetCode\" GUID=\"{C427A968-CED5-49C0-BFDE-AC2143C688EC}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Server Name=\"RobotLock\" GUID=\"{C73AB6B8-68DE-4192-ABA4-2D32F5D8D566}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"RobotPowerOff\" GUID=\"{45163924-14EF-4B60-9C3B-364662E53787}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"RobotPowerOn\" GUID=\"{DE939C5A-E281-44F8-9661-45FB4988066B}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"RobotUnLock\" GUID=\"{ADED8B18-F205-4D23-9D6E-A966DCA5605E}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"false\" Retentive=\"false\"/> <Server Name=\"velo\" GUID=\"{62BB909C-5656-4232-B799-BE918E89FDFD}\" Visualized=\"false\" Initialize=\"false\" WriteProtected=\"true\" Retentive=\"false\"/> <Client Name=\"_StdLib\" Required=\"false\" Internal=\"false\"/> <Client Name=\"Diagnostics\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis1\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis2\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis3\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis4\" Required=\"true\" Internal=\"false\"/> <Client Name=\"LMCAxis5\" Required=\"true\" Internal=\" ID: 10000."
>   ]
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
>   "title": "LASAL 검색 메뉴 확인",
>   "code": "await sky.click({window:targetWindow,x:88,y:40});\nglobalThis.editMenuState = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});\n{\n const lines=(editMenuState.accessibility?.tree||\"\").split(\"\\n\");\n nodeRepl.write(lines.filter(x=>/Find|검색|Go To|Goto|Line|Implementation|메뉴 항목/i.test(x)).slice(-100).join(\"\\n\"));\n}"
> }
> ```
>
> ```text
> 			83 텍스트 Offline
> 				100 메뉴 항목 Macros
> 				102 메뉴 항목 Layouts
> 				105 메뉴 항목 Toolbar Options
> 				108 메뉴 항목 Toolbar Options
> 				110 단추 Go online (Alt+F6)
> 				111 단추 Change Online Settings
> 				112 메뉴 항목 Online Connection
> 				113 단추 (disabled) Set Online Connection For Current Project
> 				115 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				122 메뉴 항목 Toolbar Options
> 				124 메뉴 항목 Target Architecture
> 				141 메뉴 항목 (disabled) Undo (Strg+Z)
> 				142 메뉴 항목 (disabled) Redo (Strg+Y)

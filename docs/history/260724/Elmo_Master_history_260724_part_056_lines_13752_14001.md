>     "id": 460048,
>     "title": "Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
>     "id": 4722902,
>     "title": "Daou Office - 홈 - Chrome"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\PicPick\\picpick.exe",
>     "id": 69298,
>     "title": "픽픽 - 이미지 007"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 5311296,
>     "title": "Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
>     "id": 15273316,
>     "title": "Daou Office - 홈 - Chrome"
>   },
>   {
>     "app": "process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
>     "id": 1249768,
>     "title": "ssh_dev용 Gantry Home 모드 재설계 - Chrome"
>   },
>   {
>     "app": "process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
>     "id": 2559370,
>     "title": "AI 이후 새로운 산업이 없긴 하네 - 특이점이 온다 마이너 갤러리 - Chrome"
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
>   "code": "globalThis.qualificationCandidates = openWindows.filter((window) => window.title === \"LASAL Motion Control API Example\" && window.app.includes(\"LasalMotionControlApiExample.exe\"));\nif (qualificationCandidates.length !== 1) {\n  nodeRepl.write(JSON.stringify(qualificationCandidates, null, 2));\n  throw new Error(`Expected exactly one qualification window; found ${qualificationCandidates.length}`);\n}\nglobalThis.qualificationReturnedWindow = qualificationCandidates[0];\nglobalThis.targetWindow = await sky.get_window({ id: qualificationReturnedWindow.id, app: qualificationReturnedWindow.app });\nawait sky.activate_window({ window: targetWindow });\nglobalThis.state = await sky.get_window_state({ window: targetWindow, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));",
>   "title": "WPF 시작 화면 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ````text
> Window: "LASAL Motion Control API Example", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL Motion Control API Example Secondary Actions: Raise
> 		1 제목 표시줄
> 			2 메뉴 모음 시스템 ID: MenuBar
> 				3 메뉴 항목 시스템
> 			4 단추 최소화
> 			5 단추 최대화
> 			6 단추 닫기
> 		7 텍스트 LASAL Motion Control API Example
> 		8 텍스트 Motion controls remain PLC-active. Diagnostics controls are enabled only after the PLC advertises the required capability. Values use the selected application UNIT or pass through as raw DINT.
> 		9 그룹 Connection / RPC callback
> 			10 텍스트 Connection / RPC callback
> 			11 텍스트 PLC IP
> 			12 편집 ID: TextRemoteIp
> 			13 텍스트 TCP port
> 			14 편집 ID: TextRemotePort
> 			15 텍스트 PC local IPv4
> 			16 편집 ID: TextLocalIp
> 			17 텍스트 Callback UDP port
> 			18 편집 ID: TextCallbackPort
> 			19 단추 Connect ID: ButtonConnect
> 				20 텍스트 Connect
> 			21 단추 (disabled) Close ID: ButtonCloseConnection
> 				22 텍스트 (disabled) Close
> 			23 텍스트 Connection state
> 			24 텍스트 Disconnected ID: TextConnectionState
> 			25 텍스트 Callback listener
> 			26 텍스트 Stopped ID: TextCallbackState
> 			27 텍스트 Connect performs RPC session initialization and callback registration automatically. Callback payloads are logged as raw diagnostic data.
> 		28 텍스트 Stop, PowerOff, and Group Stop remain available while connected. Closing the connection does not stop motion. ID: TextMotionWarning
> 		29 탭 ID: TabsMotion
> 			30 탭 항목 (selectable) Single Axis
> 				31 텍스트 Single Axis
> 				32 창 ID: ScrollSingleAxis
> 					33 그룹 Axis object
> 						34 텍스트 Axis object
> 						35 텍스트 LASAL object name
> 						36 단추 (disabled) Load Axis ID: ButtonLookupAxis
> 							37 텍스트 (disabled) Load Axis
> 						38 편집 ID: TextAxisName
> 						39 텍스트 Axis reference:
> 						40 텍스트 not loaded ID: TextAxisReference
> 					41 그룹 Read / control
> 						42 텍스트 Read / control
> 						43 단추 (disabled) Read Status ID: ButtonReadStatus
> 							44 텍스트 (disabled) Read Status
> 						45 단추 (disabled) Read Position ID: ButtonReadPosition
> 							46 텍스트 (disabled) Read Position
> 						47 단추 (disabled) Power On ID: ButtonPowerOn
> 							48 텍스트 (disabled) Power On
> 						49 단추 (disabled) Power Off ID: ButtonPowerOff
> 							50 텍스트 (disabled) Power Off
> 						51 단추 (disabled) Reset ID: ButtonReset
> 							52 텍스트 (disabled) Reset
> 						53 단추 (disabled) Stop ID: ButtonStop
> 							54 텍스트 (disabled) Stop
> 					55 그룹 Latest axis result
> 						56 텍스트 Latest axis result
> 						57 편집 ID: TextAxisResult
> 					58 그룹 Engineering values
> 						59 텍스트 Engineering values
> 						60 텍스트 PLC application UNIT
> 						61 콤보 상자 ID: ComboAxisUnit
> 						62 텍스트 Position / distance
> 						63 편집 ID: TextPosition
> 						64 텍스트 Velocity
> 						65 편집 ID: TextVelocity
> 						66 텍스트 Acceleration
> 						67 편집 ID: TextAcceleration
> 						68 텍스트 Deceleration / Stop
> 						69 편집 ID: TextDeceleration
> 						70 텍스트 Jerk (axis unit/s^3/1000)
> 						71 편집 ID: TextJerk
> 						72 텍스트 Velocity direction
> 						73 콤보 상자 ID: ComboDirection
> 						74 텍스트 PC values use mm (x10000); the saved axis transmission is 8388608 counts per 10 mm. Re-reference after downloading the new scale. None / raw DINT sends an already converted integer. Velocity/acceleration/deceleration must be positive. Enter physical jerk / 1000; the UI applies the selected UNIT.
> 					75 그룹 Motion
> 						76 텍스트 Motion
> 						77 단추 (disabled) Move Absolute ID: ButtonMoveAbsolute
> 							78 텍스트 (disabled) Move Absolute
> 						79 단추 (disabled) Move Relative ID: ButtonMoveRelative
> 							80 텍스트 (disabled) Move Relative
> 						81 단추 (disabled) Move Velocity ID: ButtonMoveVelocity
> 							82 텍스트 (disabled) Move Velocity
> 						83 텍스트 Absolute/Relative use Shortest. Relative direction comes from the distance sign. Velocity runs until Stop or PowerOff is verified.
> 			84 탭 항목 (selectable) Group Motion
> 				85 텍스트 Group Motion
> 			86 탭 항목 (selectable) EtherCAT / PI
> 				87 텍스트 EtherCAT / PI
> 			88 탭 항목 (selectable) Bulk Snapshot
> 				89 텍스트 Bulk Snapshot
> 			90 탭 항목 (selectable) Recorder
> 				91 텍스트 Recorder
> 			92 탭 항목 (selectable) SDO / Write Policy
> 				93 텍스트 SDO / Write Policy
> 			94 탭 항목 (selectable) Read-only API
> 				95 텍스트 Read-only API
> 		96 그룹 Execution log / raw callback diagnostics
> 			97 단추 Execution log / raw callback diagnostics ID: HeaderSite
> 				98 텍스트 Execution log / raw callback diagnostics
> 		99 상태 표시줄
> 			100 텍스트 Ready ID: TextOperationState
>
> Document text: ```
> 0
> ```
> ````
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var groupObservation = globalThis.state;\nif (groupObservation?.accessibility == null) throw new Error(\"No accessibility observation for Group tab click\");\nglobalThis.state = null;\nawait sky.click({ window: groupObservation.window, element_index: 84 });\nglobalThis.state = await sky.get_window_state({ window: groupObservation.window, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));",
>   "title": "Group Qualification 화면 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ````text
> Window: "LASAL Motion Control API Example", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL Motion Control API Example Secondary Actions: Raise
> 		1 제목 표시줄
> 			2 메뉴 모음 시스템 ID: MenuBar
> 				3 메뉴 항목 시스템
> 			4 단추 최소화
> 			5 단추 최대화
> 			6 단추 닫기
> 		7 텍스트 LASAL Motion Control API Example
> 		8 텍스트 Motion controls remain PLC-active. Diagnostics controls are enabled only after the PLC advertises the required capability. Values use the selected application UNIT or pass through as raw DINT.
> 		9 그룹 Connection / RPC callback
> 			10 텍스트 Connection / RPC callback
> 			11 텍스트 PLC IP
> 			12 편집 ID: TextRemoteIp
> 			13 텍스트 TCP port
> 			14 편집 ID: TextRemotePort
> 			15 텍스트 PC local IPv4
> 			16 편집 ID: TextLocalIp
> 			17 텍스트 Callback UDP port
> 			18 편집 ID: TextCallbackPort
> 			19 단추 Connect ID: ButtonConnect
> 				20 텍스트 Connect
> 			21 단추 (disabled) Close ID: ButtonCloseConnection
> 				22 텍스트 (disabled) Close
> 			23 텍스트 Connection state
> 			24 텍스트 Disconnected ID: TextConnectionState
> 			25 텍스트 Callback listener
> 			26 텍스트 Stopped ID: TextCallbackState
> 			27 텍스트 Connect performs RPC session initialization and callback registration automatically. Callback payloads are logged as raw diagnostic data.
> 		28 텍스트 Stop, PowerOff, and Group Stop remain available while connected. Closing the connection does not stop motion. ID: TextMotionWarning
> 		29 탭 ID: TabsMotion
> 			30 탭 항목 (selectable) Single Axis
> 				31 텍스트 Single Axis
> 			32 탭 항목 (selectable) Group Motion
> 				33 텍스트 Group Motion
> 				34 창 ID: ScrollGroupMotion
> 					35 그룹 Group object
> 						36 텍스트 Group object
> 						37 텍스트 LASAL object name
> 						38 단추 (disabled) Load Group ID: ButtonLookupGroup
> 							39 텍스트 (disabled) Load Group
> 						40 편집 ID: TextGroupName
> 						41 텍스트 Group reference:
> 						42 텍스트 not loaded ID: TextGroupReference
> 					43 그룹 Group commands
> 						44 텍스트 Group commands
> 						45 텍스트 Preparation: load the group first. ID: TextGroupPreparationState
> 						46 단추 (disabled) Get Members ID: ButtonGetMembers
> 							47 텍스트 (disabled) Get Members
> 						48 단추 (disabled) 1 Power On ID: ButtonGroupPowerOn
> 							49 텍스트 (disabled) 1 Power On
> 						50 단추 (disabled) 2 / 5 Read Status (Power Ready / Lock Ready) ID: ButtonGroupReadStatus
> 							51 텍스트 (disabled) 2 / 5 Read Status (Power Ready / Lock Ready)
> 						52 단추 (disabled) 4 Enable (Lock Profile) ID: ButtonGroupEnable
> 							53 텍스트 (disabled) 4 Enable (Lock Profile)
> 						54 단추 (disabled) Disable (Unlock Profile) ID: ButtonGroupDisable
> 							55 텍스트 (disabled) Disable (Unlock Profile)
> 						56 단추 (disabled) 7 Power Off ID: ButtonGroupPowerOff
> 							57 텍스트 (disabled) 7 Power Off
> 						58 단추 (disabled) Read Position ID: ButtonGroupReadPosition
> 							59 텍스트 (disabled) Read Position
> 						60 단추 (disabled) Reset ID: ButtonGroupReset
> 							61 텍스트 (disabled) Reset
> 						62 단추 (disabled) Stop ID: ButtonGroupStop
> 							63 텍스트 (disabled) Stop
> 						64 텍스트 Required order: 1 Power On -> 2 Read Status until PowerOn=True -> 3 Set Identity (automatic Home Check) -> 4 Enable (Lock Profile) -> 5 Read Status until Enabled/Locked Standby=True -> 6 Move -> Disable (Unlock Profile) -> 7 Power Off -> Verify Power Off (Read Status) until PowerOn=False. Read Position is disabled while Power Off verification is pending.
> 					65 그룹 Latest group result
> 						66 텍스트 Latest group result
> 						67 편집 ID: TextGroupResult
> 					68 그룹 Linear motion values
> 						69 텍스트 Linear motion values
> 						70 텍스트 PLC application UNIT
> 						71 콤보 상자 ID: ComboGroupUnit
> 						72 텍스트 X target / delta
> 						73 편집 ID: TextGroupPositionX
> 						74 텍스트 Y target / delta
> 						75 편집 ID: TextGroupPositionY
> 						76 텍스트 Z target / delta

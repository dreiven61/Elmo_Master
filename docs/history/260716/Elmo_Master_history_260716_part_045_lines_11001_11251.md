> <details><summary>Edited 7 files, called 4 tools</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+31 -0)
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+7 -2)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+2 -0)
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/App.xaml` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/App.xaml.cs` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj` (+1 -1)
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.launch_app({app: String.raw`C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\Codex_LASAL_WPF\\PmasApiWpfTestApp\\bin\\Debug\\LasalMotionControlApiExample.exe`}); await new Promise(r => setTimeout(r, 400)); globalThis.apps = await sky.list_apps(); globalThis.exampleApp = globalThis.apps.find(a => String(a.displayName || '').includes('LasalMotionControlApiExample')); globalThis.targetWindow = await sky.get_window(globalThis.exampleApp.windows[0]); await sky.activate_window({window: globalThis.targetWindow}); globalThis.finalUiState = await sky.get_window_state({window: globalThis.targetWindow, include_screenshot: true, include_text: true}); nodeRepl.write(globalThis.finalUiState.accessibility?.tree || '');",
>   "title": "수정된 예제 화면 확인"
> }
> ```
>
> ```text
> Cannot read properties of undefined (reading 'windows')
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.apps = await sky.list_apps(); nodeRepl.write(JSON.stringify(globalThis.apps.filter(a => String(a.id).includes('LasalMotionControlApiExample') || (a.windows || []).some(w => String(w.title).includes('LASAL Motion Control API Example'))), null, 2));",
>   "title": "예제 창 다시 찾기"
> }
> ```
>
> ```text
> [
>   {
>     "id": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\Codex_LASAL_WPF\\PmasApiWpfTestApp\\bin\\Debug\\LasalMotionControlApiExample.exe",
>     "windows": [
>       {
>         "app": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\Codex_LASAL_WPF\\PmasApiWpfTestApp\\bin\\Debug\\LasalMotionControlApiExample.exe",
>         "id": 8918514,
>         "title": "LASAL Motion Control API Example"
>       }
>     ],
>     "displayName": "LasalMotionControlApiExample",
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
>   "code": "globalThis.exampleApp = globalThis.apps.find(a => String(a.id).includes('LasalMotionControlApiExample')); globalThis.targetWindow = await sky.get_window(globalThis.exampleApp.windows[0]); await sky.activate_window({window: globalThis.targetWindow}); globalThis.finalUiState = await sky.get_window_state({window: globalThis.targetWindow, include_screenshot: true, include_text: true}); nodeRepl.write(globalThis.finalUiState.accessibility?.tree || '');",
>   "title": "수정된 예제 화면 확인"
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
> 		8 텍스트 Only PLC-active APIs are exposed. Values are converted by engineering value × PLC UNIT.
> 		9 텍스트 18 active protocol paths
> 		10 그룹 Connection / RPC callback
> 			11 텍스트 Connection / RPC callback
> 			12 텍스트 PLC IP
> 			13 편집 ID: TextRemoteIp
> 			14 텍스트 TCP port
> 			15 편집 ID: TextRemotePort
> 			16 텍스트 PC local IPv4
> 			17 편집 ID: TextLocalIp
> 			18 텍스트 Callback UDP port
> 			19 편집 ID: TextCallbackPort
> 			20 단추 Connect ID: ButtonConnect
> 				21 텍스트 Connect
> 			22 단추 (disabled) Close ID: ButtonCloseConnection
> 				23 텍스트 (disabled) Close
> 			24 텍스트 Connection state
> 			25 텍스트 Disconnected ID: TextConnectionState
> 			26 텍스트 Callback listener
> 			27 텍스트 Stopped ID: TextCallbackState
> 			28 텍스트 Connect performs RPC session initialization and callback registration automatically. Callback payloads are logged as raw diagnostic data.
> 		29 확인란 (disabled) Arm one power / motion / state command ID: CheckArmCommand
> 			30 텍스트 (disabled) Arm one power / motion / state command
> 		31 텍스트 Stop and PowerOff remain available without arming. Closing the connection does not stop motion. ID: TextMotionWarning
> 		32 탭
> 			33 탭 항목 (selectable) Single Axis
> 				34 텍스트 Single Axis
> 				35 창
> 					36 그룹 Axis object
> 						37 텍스트 Axis object
> 						38 텍스트 LASAL object name
> 						39 단추 (disabled) Load Axis ID: ButtonLookupAxis
> 							40 텍스트 (disabled) Load Axis
> 						41 편집 ID: TextAxisName
> 						42 텍스트 Axis reference:
> 						43 텍스트 not loaded ID: TextAxisReference
> 					44 그룹 Read / control
> 						45 텍스트 Read / control
> 						46 단추 (disabled) Read Status ID: ButtonReadStatus
> 							47 텍스트 (disabled) Read Status
> 						48 단추 (disabled) Read Position ID: ButtonReadPosition
> 							49 텍스트 (disabled) Read Position
> 						50 단추 (disabled) Power On ID: ButtonPowerOn
> 							51 텍스트 (disabled) Power On
> 						52 단추 (disabled) Power Off ID: ButtonPowerOff
> 							53 텍스트 (disabled) Power Off
> 						54 단추 (disabled) Reset ID: ButtonReset
> 							55 텍스트 (disabled) Reset
> 						56 단추 (disabled) Stop ID: ButtonStop
> 							57 텍스트 (disabled) Stop
> 					58 그룹 Latest axis result
> 						59 텍스트 Latest axis result
> 						60 편집 ID: TextAxisResult
> 					61 그룹 Engineering values
> 						62 텍스트 Engineering values
> 						63 텍스트 PLC UNIT multiplier
> 						64 편집 ID: TextUnitMultiplier
> 						65 텍스트 Position / distance
> 						66 편집 ID: TextPosition
> 						67 텍스트 Velocity
> 						68 편집 ID: TextVelocity
> 						69 텍스트 Acceleration
> 						70 편집 ID: TextAcceleration
> 						71 텍스트 Deceleration / Stop
> 						72 편집 ID: TextDeceleration
> 						73 텍스트 Jerk (fixed)
> 						74 편집 ID: TextJerk
> 						75 텍스트 Velocity direction
> 						76 콤보 상자 ID: ComboDirection
> 						77 텍스트 10000 is the current rotary-axis profile default, not a universal encoder constant. Velocity/acceleration/deceleration must be positive. Jerk is fixed at 0 until its PLC conversion is validated.
> 					78 그룹 Motion
> 						79 텍스트 Motion
> 						80 단추 (disabled) Move Absolute ID: ButtonMoveAbsolute
> 							81 텍스트 (disabled) Move Absolute
> 						82 단추 (disabled) Move Relative ID: ButtonMoveRelative
> 							83 텍스트 (disabled) Move Relative
> 						84 단추 (disabled) Move Velocity ID: ButtonMoveVelocity
> 							85 텍스트 (disabled) Move Velocity
> 						86 텍스트 Absolute/Relative use Shortest. Relative direction comes from the distance sign. Velocity runs until Stop or PowerOff is verified.
> 					87 스크롤 막대 ID: VerticalScrollBar
> 						88 단추 (disabled) ID: PART_LineUpButton
> 						89 단추 (disabled) ID: PageUp
> 						90 단추 (disabled) ID: PageDown
> 						91 엄지 (disabled)
> 						92 단추 (disabled) ID: PART_LineDownButton
> 			93 탭 항목 (selectable) Group State
> 				94 텍스트 Group State
> 		95 그룹 Execution log / raw callback diagnostics
> 			96 텍스트 Execution log / raw callback diagnostics
> 			97 단추 Copy Log
> 				98 텍스트 Copy Log
> 			99 단추 Clear Log
> 				100 텍스트 Clear Log
> 			101 편집 ID: TextExecutionLog
> 		102 상태 표시줄
> 			103 텍스트 Ready ID: TextOperationState
>
> Document text: ```
> [16:31:09.931] Example ready. Connect, load _LMCAxis1, and start with Read Status. No command is sent automatically.
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
>   "code": "await sky.press_key({window: globalThis.targetWindow, key: 'Alt_L+F4'}); nodeRepl.write('Example window closed.');",
>   "title": "예제 창 닫기"
> }
> ```
>
> ```text
> Example window closed.
> ```
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+43 -25)
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+31 -2)
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+14 -14)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+3 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+3 -2)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+3 -3)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+17 -2)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+4 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+8 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+1 -10)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+2 -0)
>
> </details>
>
> 안전 송신 순서까지 정리했습니다. 일반 동작 명령과 Stop/Power Off가 같은 앱 레벨 송신 게이트를 사용하고, Stop/Power Off 요청이 먼저 들어오면 아직 전송되지 않은 동작 명령은 게이트 안에서 취소됩니다. 현재 독립 재리뷰에서 P1/P2는 없으며, 마지막 Release 빌드와 42개 API 회귀 테스트를 재실행합니다.
</details>

구현 완료했습니다. 복사본만 실제 API 예제로 전환했고, 원본 `Codex_LASAL_WPF`와 기존 테스트 앱 소스는 변경하지 않았습니다.

- 실제 `LasalMotionControlLib`를 `ProjectReference`로 사용
- PLC 활성 18개 protocol 경로만 노출
- 화면은 Connection, Single Axis, Group State, Log만 유지
- 미구현·더미 API 관련 탭, 코드, 문구 제거
- UNIT은 PC에서 `engineering value × UNIT`으로 변환
- jerk는 검증 전까지 UI와 송신값 모두 `0` 고정
- 유한 이동은 실제 non-standstill 관측 후 Standstill 3회로 완료 판단
- Stop/Power Off는 별도 안전 송신 경로로 처리
- Stop/Power Off가 먼저 요청되면 아직 전송되지 않은 동작 명령은 취소
- callback은 완료 신호로 해석하지 않고 raw 진단 데이터만 표시

주요 파일:

- [README.md](./LMC_Library/Codex_LASAL_WPF/README.md)
- [DESIGN.md](./LMC_Library/Codex_LASAL_WPF/DESIGN.md)
- [API_MAPPING.md](./LMC_Library/Codex_LASAL_WPF/API_MAPPING.md)
- [MainWindow.xaml](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml)
- [MainWindow.xaml.cs](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs)

검증 결과:

- Debug/Release 빌드 통과

> 						88 엄지 (disabled)
> 						89 단추 (disabled) ID: PART_LineDownButton
> 			90 탭 항목 (selectable) Group Motion
> 				91 텍스트 Group Motion
> 			92 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				93 텍스트 EtherCAT / CREVIS / PI
> 			94 탭 항목 (selectable) Bulk Snapshot
> 				95 텍스트 Bulk Snapshot
> 			96 탭 항목 (selectable) Recorder
> 				97 텍스트 Recorder
> 			98 탭 항목 (selectable) SDO / Write Policy
> 				99 텍스트 SDO / Write Policy
> 			100 탭 항목 (selectable) Read-only API
> 				101 텍스트 Read-only API
> 		102 그룹 Execution log / raw callback diagnostics
> 			103 단추 Execution log / raw callback diagnostics ID: HeaderSite
> 				104 텍스트 Execution log / raw callback diagnostics
> 		105 상태 표시줄
> 			106 텍스트 Ready ID: TextOperationState
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
>   "code": "var obsConnect = globalThis.testState; if(!obsConnect?.accessibility) throw new Error('No test observation'); globalThis.testState=null; try { await sky.click({window:obsConnect.window, element_index:19}); globalThis.testState=await sky.get_window_state({window:obsConnect.window, include_screenshot:true, include_text:true}); } catch(error){ throw new Error('Connect outcome unknown; reobserve',{cause:error}); } globalThis.testWindow=testState.window; nodeRepl.write(String(testState.accessibility?.tree || testState.accessibility?.document_text || ''));",
>   "title": "Connect 단계 재현",
>   "timeout_ms": 30000
> }
> ```
>
> ````text
> Window: "LASAL Motion Control API Example v0.9.1.0 [CREVIS topology / editable SDO draft]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL Motion Control API Example v0.9.1.0 [CREVIS topology / editable SDO draft] Secondary Actions: Raise
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
> 			12 편집 (disabled) ID: TextRemoteIp
> 			13 텍스트 TCP port
> 			14 편집 (disabled) ID: TextRemotePort
> 			15 텍스트 PC local IPv4
> 			16 편집 (disabled) ID: TextLocalIp
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
> 		28 텍스트 SAFETY: An accepted Axis Power transition is awaiting exact-identity status-only verification; do not replay 0x2023. ID: TextMotionWarning
> 		29 탭 ID: TabsMotion
> 			30 탭 항목 (selectable) Single Axis
> 				31 텍스트 Single Axis
> 				32 창 ID: ScrollSingleAxis
> 					33 그룹 Axis object
> 						34 텍스트 Axis object
> 						35 텍스트 LASAL object name
> 						36 단추 (disabled) Load Axis ID: ButtonLookupAxis
> 							37 텍스트 (disabled) Load Axis
> 						38 편집 (disabled) ID: TextAxisName
> 						39 텍스트 Axis reference:
> 						40 텍스트 not loaded ID: TextAxisReference
> 					41 그룹 Read / control
> 						42 텍스트 Read / control
> 						43 단추 (disabled) Read Status ID: ButtonReadStatus
> 							44 텍스트 (disabled) Read Status
> 						45 단추 (disabled) Read Position ID: ButtonReadPosition
> 							46 텍스트 (disabled) Read Position
> 						47 단추 (disabled) Power On Replay Blocked - Send Power Off ID: ButtonPowerOn
> 							48 텍스트 (disabled) Power On Replay Blocked - Send Power Off
> 						49 단추 (disabled) Resume Power Off Verification (No 0x2023 Replay) ID: ButtonPowerOff
> 							50 텍스트 (disabled) Resume Power Off Verification (No 0x2023 Replay)
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
> 					84 스크롤 막대 ID: VerticalScrollBar
> 						85 단추 (disabled) ID: PART_LineUpButton
> 						86 단추 (disabled) ID: PageUp
> 						87 단추 (disabled) ID: PageDown
> 						88 엄지 (disabled)
> 						89 단추 (disabled) ID: PART_LineDownButton
> 			90 탭 항목 (selectable) Group Motion
> 				91 텍스트 Group Motion
> 			92 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				93 텍스트 EtherCAT / CREVIS / PI
> 			94 탭 항목 (selectable) Bulk Snapshot
> 				95 텍스트 Bulk Snapshot
> 			96 탭 항목 (selectable) Recorder
> 				97 텍스트 Recorder
> 			98 탭 항목 (selectable) SDO / Write Policy
> 				99 텍스트 SDO / Write Policy
> 			100 탭 항목 (selectable) Read-only API
> 				101 텍스트 Read-only API
> 		102 그룹 Execution log / raw callback diagnostics
> 			103 단추 Execution log / raw callback diagnostics ID: HeaderSite
> 				104 텍스트 Execution log / raw callback diagnostics
> 		105 상태 표시줄
> 			106 텍스트 Connect failed ID: TextOperationState
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
>   "code": "var obsLog = globalThis.testState; if(!obsLog?.accessibility) throw new Error('No test observation'); globalThis.testState=null; try { await sky.click({window:obsLog.window, element_index:103}); globalThis.testState=await sky.get_window_state({window:obsLog.window, include_screenshot:true, include_text:true}); } catch(error){ throw new Error('Log expansion outcome unknown; reobserve',{cause:error}); } globalThis.testWindow=testState.window; nodeRepl.write(String(testState.accessibility?.tree || testState.accessibility?.document_text || ''));",
>   "title": "접속 실패 로그 열기",
>   "timeout_ms": 30000
> }
> ```
>
> ````text
> Window: "LASAL Motion Control API Example v0.9.1.0 [CREVIS topology / editable SDO draft]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL Motion Control API Example v0.9.1.0 [CREVIS topology / editable SDO draft] Secondary Actions: Raise
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
> 			12 편집 (disabled) ID: TextRemoteIp
> 			13 텍스트 TCP port
> 			14 편집 (disabled) ID: TextRemotePort
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
> 		28 텍스트 SAFETY: An accepted Axis Power transition is awaiting exact-identity status-only verification; do not replay 0x2023. ID: TextMotionWarning
> 		29 탭 ID: TabsMotion
> 			30 탭 항목 (selectable) Single Axis
> 				31 텍스트 Single Axis
> 				32 창 ID: ScrollSingleAxis
> 					33 그룹 Axis object
> 						34 텍스트 Axis object
> 						35 텍스트 LASAL object name
> 						36 단추 (disabled) Load Axis ID: ButtonLookupAxis
> 							37 텍스트 (disabled) Load Axis
> 						38 편집 (disabled) ID: TextAxisName
> 						39 텍스트 Axis reference:
> 						40 텍스트 not loaded ID: TextAxisReference
> 					41 그룹 Read / control
> 						42 텍스트 Read / control
> 						43 단추 (disabled) Read Status ID: ButtonReadStatus
> 							44 텍스트 (disabled) Read Status
> 						45 단추 (disabled) Read Position ID: ButtonReadPosition
> 							46 텍스트 (disabled) Read Position
> 						47 단추 (disabled) Power On Replay Blocked - Send Power Off ID: ButtonPowerOn
> 							48 텍스트 (disabled) Power On Replay Blocked - Send Power Off
> 						49 단추 (disabled) Resume Power Off Verification (No 0x2023 Replay) ID: ButtonPowerOff
> 							50 텍스트 (disabled) Resume Power Off Verification (No 0x2023 Replay)
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

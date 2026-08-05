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
> 			106 텍스트 Power Off verification failed ID: TextOperationState
>
> Document text: ```
> 0
> ```
> ````
>
> Image output: image/jpeg
>
> 실행 화면에서 원인이 확인됐습니다. 연결은 정상(`Connected`)이고 격리 상태도 아닙니다. 현재 앱이 `Power Off verification failed` 상태와 미해결 Power Off 복구 기록 때문에 Power On·Reset·Stop·이동 명령을 잠그고 있습니다. 이제 왜 검증이 실패했고 “저널 내구성 저하”까지 표시되는지 로그와 코드 조건을 맞춰 보겠습니다.
>
> 실패 원인도 좁혀졌습니다. 실제 결과에는 마지막 상태가 `PowerOn=False, Standstill=True`인데도 80회 폴링 동안 안정 샘플 카운트가 `0/3`으로 끝났습니다. 즉 PLC가 꺼짐/정지 상태를 반환했는데 PC 쪽 검증 로직이 그 상태를 유효 샘플로 인정하지 못한 것이 핵심입니다. 이 판정 조건을 추적 중입니다.
>
> 정확한 저널 오류 문구까지 확인하려고 화면 아래의 로그 패널만 펼치겠습니다. PLC 명령이나 축 제어 버튼은 건드리지 않습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.cuState = await sky.get_window_state({window: cuTargetWindow, include_screenshot: false, include_text: true}); globalThis.cuTargetWindow = cuState.window; nodeRepl.write(String(cuState.accessibility?.tree || \"\"));",
>   "title": "로그 패널 위치 다시 확인"
> }
> ```
>
> ````text
> Window: "LASAL Motion Control API Example v0.9.1.0 [CREVIS topology / editable SDO draft]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL Motion Control API Example v0.9.1.0 [CREVIS topology / editable SDO draft] Secondary Actions: Raise
> 		1 제목 표시줄
> 			2 메뉴 모음 시스템 ID: MenuBar
> 				107 메뉴 항목 시스템
> 			4 단추 최소화
> 			5 단추 복원
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
> 			18 편집 (disabled) ID: TextCallbackPort
> 			19 단추 (disabled) Connect ID: ButtonConnect
> 				20 텍스트 (disabled) Connect
> 			21 단추 (disabled) Close ID: ButtonCloseConnection
> 				22 텍스트 (disabled) Close
> 			23 텍스트 Connection state
> 			24 텍스트 Connected ID: TextConnectionState
> 			25 텍스트 Callback listener
> 			26 텍스트 Listening 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			27 텍스트 Connect performs RPC session initialization and callback registration automatically. Callback payloads are logged as raw diagnostic data.
> 		28 텍스트 SAFETY: Axis Power journal durability is degraded. Explicit safety Power Off remains available through process-local tracking, but status proof cannot claim durable recovery resolution. ID: TextMotionWarning
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
> 						40 텍스트 1 ID: TextAxisReference
> 					41 그룹 Read / control
> 						42 텍스트 Read / control
> 						43 단추 Read Status ID: ButtonReadStatus
> 							44 텍스트 Read Status
> 						45 단추 Read Position ID: ButtonReadPosition
> 							46 텍스트 Read Position
> 						47 단추 (disabled) Power On Replay Blocked - Send Power Off ID: ButtonPowerOn
> 							48 텍스트 (disabled) Power On Replay Blocked - Send Power Off
> 						49 단추 Resume Power Off Verification (No 0x2023 Replay) ID: ButtonPowerOff
> 							50 텍스트 Resume Power Off Verification (No 0x2023 Replay)
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
> 			106 텍스트 Power Off verification failed ID: TextOperationState
>
> Document text: ```
> 0
> ```
> ````
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var cuObsLog = globalThis.cuState; globalThis.cuState = null; await sky.click({window: cuObsLog.window, element_index: 103}); globalThis.cuState = await sky.get_window_state({window: cuObsLog.window, include_screenshot: false, include_text: true}); globalThis.cuTargetWindow = cuState.window; nodeRepl.write(String(cuState.accessibility?.tree || \"\"));",
>   "title": "실행 로그 펼쳐 읽기"
> }
> ```
>
> ````text
> Window: "LASAL Motion Control API Example v0.9.1.0 [CREVIS topology / editable SDO draft]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL Motion Control API Example v0.9.1.0 [CREVIS topology / editable SDO draft] Secondary Actions: Raise
> 		1 제목 표시줄
> 			2 메뉴 모음 시스템 ID: MenuBar
> 				107 메뉴 항목 시스템
> 			4 단추 최소화
> 			5 단추 복원
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
> 			18 편집 (disabled) ID: TextCallbackPort
> 			19 단추 (disabled) Connect ID: ButtonConnect
> 				20 텍스트 (disabled) Connect
> 			21 단추 (disabled) Close ID: ButtonCloseConnection
> 				22 텍스트 (disabled) Close
> 			23 텍스트 Connection state
> 			24 텍스트 Connected ID: TextConnectionState
> 			25 텍스트 Callback listener
> 			26 텍스트 Listening 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			27 텍스트 Connect performs RPC session initialization and callback registration automatically. Callback payloads are logged as raw diagnostic data.
> 		28 텍스트 SAFETY: Axis Power journal durability is degraded. Explicit safety Power Off remains available through process-local tracking, but status proof cannot claim durable recovery resolution. ID: TextMotionWarning
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
> 						40 텍스트 1 ID: TextAxisReference
> 					41 그룹 Read / control
> 						42 텍스트 Read / control
> 						43 단추 Read Status ID: ButtonReadStatus
> 							44 텍스트 Read Status
> 						45 단추 Read Position ID: ButtonReadPosition
> 							46 텍스트 Read Position
> 						47 단추 (disabled) Power On Replay Blocked - Send Power Off ID: ButtonPowerOn
> 							48 텍스트 (disabled) Power On Replay Blocked - Send Power Off
> 						49 단추 Resume Power Off Verification (No 0x2023 Replay) ID: ButtonPowerOff
> 							50 텍스트 Resume Power Off Verification (No 0x2023 Replay)
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

> 						77 편집 ID: TextGroupPositionZ
> 						78 텍스트 U target / delta
> 						79 편집 ID: TextGroupPositionU
> 						80 텍스트 Velocity
> 						81 편집 ID: TextGroupVelocity
> 						82 텍스트 Acceleration
> 						83 편집 ID: TextGroupAcceleration
> 						84 텍스트 Deceleration / Stop
> 						85 편집 ID: TextGroupDeceleration
> 						86 텍스트 Jerk
> 						87 편집 ID: TextGroupJerk
> 						88 텍스트 Coordinate (Read: None/ACS; Motion: None)
> 						89 콤보 상자 ID: ComboGroupCoordinate
> 						90 텍스트 Transition
> 						91 콤보 상자 ID: ComboGroupTransition
> 						92 텍스트 Buffer mode
> 						93 콤보 상자 ID: ComboGroupBuffer
> 						94 단추 (disabled) 6 Move Linear Absolute ID: ButtonGroupMoveLinear
> 							95 텍스트 (disabled) 6 Move Linear Absolute
> 						96 단추 (disabled) 6 Move Linear Relative ID: ButtonGroupMoveLinearRelative
> 							97 텍스트 (disabled) 6 Move Linear Relative
> 						98 텍스트 Absolute interprets X/Y/Z/U as targets; Relative interprets them as deltas. Positions and dynamics use the selected group application UNIT. The current PLC group uses mm (x10000). Read Position accepts None/ACS, but motion remains Coordinate=None only and Move buttons are disabled while ACS is selected. For axis-mapping captures, keep three deltas at 0 and move one X/Y/Z/U axis at a time. Completion timeout is calculated from distance, velocity, acceleration, and deceleration (15 to 600 seconds).
> 					99 그룹 Cartesian 4-axis identity kinematics
> 						100 텍스트 Cartesian 4-axis identity kinematics
> 						101 텍스트 X axis object
> 						102 편집 (disabled) ID: TextKinAxisX
> 						103 텍스트 Y axis object
> 						104 편집 (disabled) ID: TextKinAxisY
> 						105 텍스트 Z axis object
> 						106 편집 (disabled) ID: TextKinAxisZ
> 						107 텍스트 U axis object
> 						108 편집 (disabled) ID: TextKinAxisU
> 						109 단추 (disabled) Home Check (X/Y/Z/U) ID: ButtonCheckKinHome
> 							110 텍스트 (disabled) Home Check (X/Y/Z/U)
> 						111 단추 (disabled) 3 Set Identity (Auto Home Check + Configure) ID: ButtonSetKinTransform
> 							112 텍스트 (disabled) 3 Set Identity (Auto Home Check + Configure)
> 						113 텍스트 Home Check reads _LMCAXIS_STATUS.IsReferenced for the four identity axes. Set Identity repeats the check automatically and is blocked when any selected axis is not referenced.
> 						114 텍스트 Identity axis Home status
> 						115 편집 ID: TextKinHomeStatus
> 					116 그룹 Qualification automation (live PLC motion)
> 						117 텍스트 Qualification automation (live PLC motion)
> 						118 텍스트 These tests send real group commands. Group Enable qualification starts powered with identity configured but unlocked/disabled. Buffered and Stop-first qualifications start powered, identity configured, and locked. Keep people and tooling clear. Group Stop and Power Off remain available. Buffered A/B returns to the captured start position only after a verified PASS; any uncertain motion is stopped and verified instead.
> 						119 텍스트 Axis
> 						120 콤보 상자 (disabled) ID: ComboQualificationGroupAxis
> 						121 텍스트 Delta A (raw DINT)
> 						122 편집 (disabled) ID: TextQualificationDeltaA
> 						123 텍스트 Delta B (raw DINT)
> 						124 편집 (disabled) ID: TextQualificationDeltaB
> 						125 텍스트 Tolerance (raw DINT)
> 						126 편집 (disabled) ID: TextQualificationTolerance
> 						127 텍스트 Velocity (raw DINT)
> 						128 편집 (disabled) ID: TextQualificationVelocity
> 						129 텍스트 Acceleration (raw DINT)
> 						130 편집 (disabled) ID: TextQualificationAcceleration
> 						131 텍스트 Deceleration / Stop (raw DINT)
> 						132 편집 (disabled) ID: TextQualificationDeceleration
> 						133 텍스트 Jerk (raw DINT)
> 						134 편집 (disabled) ID: TextQualificationJerk
> 						135 단추 (disabled) Run Enable ACK -> Locked ID: ButtonRunGroupEnableQualification
> 							136 텍스트 (disabled) Run Enable ACK -> Locked
> 						137 단추 (disabled) Run Buffered A -> B ID: ButtonRunBufferedQualification
> 							138 텍스트 (disabled) Run Buffered A -> B
> 						139 단추 (disabled) Run Deterministic Stop-First ID: ButtonRunStopFirstQualification
> 							140 텍스트 (disabled) Run Deterministic Stop-First
> 						141 단추 (disabled) Cancel Test ID: ButtonCancelQualification
> 							142 텍스트 (disabled) Cancel Test
> 						143 단추 (disabled) Save QTEST Log ID: ButtonSaveQualificationLog
> 							144 텍스트 (disabled) Save QTEST Log
> 						145 진행률 표시줄 ID: ProgressQualification
> 						146 텍스트 No qualification has run yet. ID: TextQualificationProgress
> 						147 편집 ID: TextQualificationSummary
> 					148 스크롤 막대 ID: VerticalScrollBar
> 						149 단추 (disabled) ID: PART_LineUpButton
> 						150 단추 (disabled) ID: PageUp
> 						151 단추 (disabled) ID: PageDown
> 						152 엄지 (disabled)
> 						153 단추 (disabled) ID: PART_LineDownButton
> 			154 탭 항목 (selectable) EtherCAT / PI
> 				155 텍스트 EtherCAT / PI
> 			156 탭 항목 (selectable) Bulk Snapshot
> 				157 텍스트 Bulk Snapshot
> 			158 탭 항목 (selectable) Recorder
> 				159 텍스트 Recorder
> 			160 탭 항목 (selectable) SDO / Write Policy
> 				161 텍스트 SDO / Write Policy
> 			162 탭 항목 (selectable) Read-only API
> 				163 텍스트 Read-only API
> 		164 그룹 Execution log / raw callback diagnostics
> 			165 단추 Execution log / raw callback diagnostics ID: HeaderSite
> 				166 텍스트 Execution log / raw callback diagnostics
> 		167 상태 표시줄
> 			168 텍스트 Ready ID: TextOperationState
>
> Document text: ```
> Structured QTEST results will appear here.
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
>   "code": "var groupScrollObservation = globalThis.state;\nglobalThis.state = null;\nawait sky.scroll({ window: groupScrollObservation.window, x: 1600, y: 1020, scrollX: 0, scrollY: 906 });\nglobalThis.state = await sky.get_window_state({ window: groupScrollObservation.window, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));",
>   "title": "Group Qualification controls 확인",
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
> 						77 편집 ID: TextGroupPositionZ
> 						78 텍스트 U target / delta
> 						79 편집 ID: TextGroupPositionU
> 						80 텍스트 Velocity
> 						81 편집 ID: TextGroupVelocity
> 						82 텍스트 Acceleration
> 						83 편집 ID: TextGroupAcceleration
> 						84 텍스트 Deceleration / Stop
> 						85 편집 ID: TextGroupDeceleration
> 						86 텍스트 Jerk
> 						87 편집 ID: TextGroupJerk
> 						88 텍스트 Coordinate (Read: None/ACS; Motion: None)
> 						89 콤보 상자 ID: ComboGroupCoordinate
> 						90 텍스트 Transition
> 						91 콤보 상자 ID: ComboGroupTransition
> 						92 텍스트 Buffer mode
> 						93 콤보 상자 ID: ComboGroupBuffer
> 						94 단추 (disabled) 6 Move Linear Absolute ID: ButtonGroupMoveLinear
> 							95 텍스트 (disabled) 6 Move Linear Absolute
> 						96 단추 (disabled) 6 Move Linear Relative ID: ButtonGroupMoveLinearRelative
> 							97 텍스트 (disabled) 6 Move Linear Relative
> 						98 텍스트 Absolute interprets X/Y/Z/U as targets; Relative interprets them as deltas. Positions and dynamics use the selected group application UNIT. The current PLC group uses mm (x10000). Read Position accepts None/ACS, but motion remains Coordinate=None only and Move buttons are disabled while ACS is selected. For axis-mapping captures, keep three deltas at 0 and move one X/Y/Z/U axis at a time. Completion timeout is calculated from distance, velocity, acceleration, and deceleration (15 to 600 seconds).
> 					99 그룹 Cartesian 4-axis identity kinematics
> 						100 텍스트 Cartesian 4-axis identity kinematics
> 						101 텍스트 X axis object
> 						102 편집 (disabled) ID: TextKinAxisX
> 						103 텍스트 Y axis object
> 						104 편집 (disabled) ID: TextKinAxisY
> 						105 텍스트 Z axis object
> 						106 편집 (disabled) ID: TextKinAxisZ
> 						107 텍스트 U axis object
> 						108 편집 (disabled) ID: TextKinAxisU
> 						109 단추 (disabled) Home Check (X/Y/Z/U) ID: ButtonCheckKinHome
> 							110 텍스트 (disabled) Home Check (X/Y/Z/U)
> 						111 단추 (disabled) 3 Set Identity (Auto Home Check + Configure) ID: ButtonSetKinTransform
> 							112 텍스트 (disabled) 3 Set Identity (Auto Home Check + Configure)
> 						113 텍스트 Home Check reads _LMCAXIS_STATUS.IsReferenced for the four identity axes. Set Identity repeats the check automatically and is blocked when any selected axis is not referenced.
> 						114 텍스트 Identity axis Home status
> 						115 편집 ID: TextKinHomeStatus
> 					116 그룹 Qualification automation (live PLC motion)
> 						117 텍스트 Qualification automation (live PLC motion)
> 						118 텍스트 These tests send real group commands. Group Enable qualification starts powered with identity configured but unlocked/disabled. Buffered and Stop-first qualifications start powered, identity configured, and locked. Keep people and tooling clear. Group Stop and Power Off remain available. Buffered A/B returns to the captured start position only after a verified PASS; any uncertain motion is stopped and verified instead.
> 						119 텍스트 Axis
> 						120 콤보 상자 (disabled) ID: ComboQualificationGroupAxis
> 						121 텍스트 Delta A (raw DINT)
> 						122 편집 (disabled) ID: TextQualificationDeltaA
> 						123 텍스트 Delta B (raw DINT)
> 						124 편집 (disabled) ID: TextQualificationDeltaB
> 						125 텍스트 Tolerance (raw DINT)
> 						126 편집 (disabled) ID: TextQualificationTolerance
> 						127 텍스트 Velocity (raw DINT)
> 						128 편집 (disabled) ID: TextQualificationVelocity
> 						129 텍스트 Acceleration (raw DINT)
> 						130 편집 (disabled) ID: TextQualificationAcceleration
> 						131 텍스트 Deceleration / Stop (raw DINT)
> 						132 편집 (disabled) ID: TextQualificationDeceleration
> 						133 텍스트 Jerk (raw DINT)
> 						134 편집 (disabled) ID: TextQualificationJerk
> 						135 단추 (disabled) Run Enable ACK -> Locked ID: ButtonRunGroupEnableQualification

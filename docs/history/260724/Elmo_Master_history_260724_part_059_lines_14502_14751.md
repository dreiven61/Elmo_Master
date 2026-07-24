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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var bulkTabObservation = globalThis.state;\nglobalThis.state = null;\nawait sky.click({ window: bulkTabObservation.window, element_index: 156 });\nglobalThis.state = await sky.get_window_state({ window: bulkTabObservation.window, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));",
>   "title": "Bulk Qualification 화면 확인",
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
> 			92 탭 항목 (selectable) EtherCAT / PI
> 				93 텍스트 EtherCAT / PI
> 			94 탭 항목 (selectable) Bulk Snapshot
> 				95 텍스트 Bulk Snapshot
> 				96 창 ID: ScrollBulkSnapshot
> 					97 그룹 Same-cycle Bulk configuration
> 						98 텍스트 Same-cycle Bulk configuration
> 						99 단추 (disabled) 1 Configure Selected ID: ButtonConfigureBulk
> 							100 텍스트 (disabled) 1 Configure Selected
> 						101 단추 (disabled) 2 Refresh Status ID: ButtonReadBulkStatus
> 							102 텍스트 (disabled) 2 Refresh Status
> 						103 단추 (disabled) 3 Read Snapshot ID: ButtonReadBulkSnapshot
> 							104 텍스트 (disabled) 3 Read Snapshot
> 						105 단추 (disabled) 4 Release ID: ButtonReleaseBulk
> 							106 텍스트 (disabled) 4 Release
> 						107 텍스트 Load the PI Catalog and check Bulk-readable signals first. ID: TextBulkSummary
> 					108 그룹 Latest same-cycle snapshot
> 						109 텍스트 Latest same-cycle snapshot
> 						110 datagrid ID: GridBulkSnapshot
> 							111 헤더 ID: PART_ColumnHeadersPresenter
> 								112 헤더 항목 Alias
> 									113 텍스트 Alias
> 									114 엄지 ID: PART_RightHeaderGripper
> 								115 헤더 항목 Signal ID
> 									116 텍스트 Signal ID
> 									117 엄지 ID: PART_LeftHeaderGripper
> 									118 엄지 ID: PART_RightHeaderGripper
> 								119 헤더 항목 Type
> 									120 텍스트 Type
> 									121 엄지 ID: PART_LeftHeaderGripper
> 									122 엄지 ID: PART_RightHeaderGripper
> 								123 헤더 항목 Raw Value
> 									124 텍스트 Raw Value
> 									125 엄지 ID: PART_LeftHeaderGripper
> 									126 엄지 ID: PART_RightHeaderGripper
> 								127 헤더 항목 Status
> 									128 텍스트 Status
> 									129 엄지 ID: PART_LeftHeaderGripper
> 									130 엄지 ID: PART_RightHeaderGripper
> 								131 헤더 항목 Detail
> 									132 텍스트 Detail
> 									133 엄지 ID: PART_LeftHeaderGripper
> 									134 엄지 ID: PART_RightHeaderGripper
> 					135 그룹 Bulk qualification automation
> 						136 텍스트 Bulk qualification automation
> 						137 텍스트 The runner reloads Capabilities and the PI Catalog, selects exactly all 24 BulkReadable entries in Catalog order, uses the revision-bound builder facade, and always releases the reader. It does not inject an EtherCAT slave fault.
> 						138 텍스트 Iterations
> 						139 편집 (disabled) ID: TextQualificationBulkIterations
> 						140 텍스트 Interval (ms)
> 						141 편집 (disabled) ID: TextQualificationBulkIntervalMs
> 						142 단추 (disabled) Run 24-entry Snapshot Soak ID: ButtonRunBulkSnapshotSoakQualification
> 							143 텍스트 (disabled) Run 24-entry Snapshot Soak
> 						144 단추 (disabled) Run Configure/Read/Release Soak ID: ButtonRunBulkLifecycleQualification
> 							145 텍스트 (disabled) Run Configure/Read/Release Soak
> 						146 단추 (disabled) Cancel Test ID: ButtonCancelBulkQualification
> 							147 텍스트 (disabled) Cancel Test
> 						148 단추 (disabled) Save QTEST Log ID: ButtonSaveBulkQualificationLog
> 							149 텍스트 (disabled) Save QTEST Log
> 						150 진행률 표시줄 ID: ProgressBulkQualification
> 						151 텍스트 No qualification has run yet. ID: TextBulkQualificationProgress
> 						152 편집 ID: TextBulkQualificationSummary
> 					153 스크롤 막대 ID: VerticalScrollBar
> 						154 단추 (disabled) ID: PART_LineUpButton
> 						155 단추 (disabled) ID: PageUp
> 						156 단추 (disabled) ID: PageDown
> 						157 엄지 (disabled)
> 						158 단추 (disabled) ID: PART_LineDownButton
> 			159 탭 항목 (selectable) Recorder
> 				160 텍스트 Recorder
> 			161 탭 항목 (selectable) SDO / Write Policy
> 				162 텍스트 SDO / Write Policy
> 			163 탭 항목 (selectable) Read-only API
> 				164 텍스트 Read-only API
> 		165 그룹 Execution log / raw callback diagnostics
> 			166 단추 Execution log / raw callback diagnostics ID: HeaderSite
> 				167 텍스트 Execution log / raw callback diagnostics
> 		168 상태 표시줄
> 			169 텍스트 Ready ID: TextOperationState
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
>   "code": "var recorderTabObservation = globalThis.state;\nglobalThis.state = null;\nawait sky.click({ window: recorderTabObservation.window, element_index: 159 });\nglobalThis.state = await sky.get_window_state({ window: recorderTabObservation.window, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));",
>   "title": "Recorder Qualification 화면 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> element 159 is not available in cached app state for LasalMotionControlApiExample.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state = await sky.get_window_state({ window: targetWindow, include_screenshot: false, include_text: true });\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));",
>   "title": "Recorder 탭 선택 상태 갱신",

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
> 			34 탭 항목 (selectable) EtherCAT / PI
> 				35 텍스트 EtherCAT / PI
> 			36 탭 항목 (selectable) Bulk Snapshot
> 				37 텍스트 Bulk Snapshot
> 				38 창 ID: ScrollBulkSnapshot
> 					39 그룹 Same-cycle Bulk configuration
> 						40 텍스트 Same-cycle Bulk configuration
> 						41 단추 (disabled) 1 Configure Selected ID: ButtonConfigureBulk
> 							42 텍스트 (disabled) 1 Configure Selected
> 						43 단추 (disabled) 2 Refresh Status ID: ButtonReadBulkStatus
> 							44 텍스트 (disabled) 2 Refresh Status
> 						45 단추 (disabled) 3 Read Snapshot ID: ButtonReadBulkSnapshot
> 							46 텍스트 (disabled) 3 Read Snapshot
> 						47 단추 (disabled) 4 Release ID: ButtonReleaseBulk
> 							48 텍스트 (disabled) 4 Release
> 						49 텍스트 Load the PI Catalog and check Bulk-readable signals first. ID: TextBulkSummary
> 					50 그룹 Latest same-cycle snapshot
> 						51 텍스트 Latest same-cycle snapshot
> 						52 datagrid ID: GridBulkSnapshot
> 							53 헤더 ID: PART_ColumnHeadersPresenter
> 								54 헤더 항목 Alias
> 									55 텍스트 Alias
> 									56 엄지 ID: PART_RightHeaderGripper
> 								57 헤더 항목 Signal ID
> 									58 텍스트 Signal ID
> 									59 엄지 ID: PART_LeftHeaderGripper
> 									60 엄지 ID: PART_RightHeaderGripper
> 								61 헤더 항목 Type
> 									62 텍스트 Type
> 									63 엄지 ID: PART_LeftHeaderGripper
> 									64 엄지 ID: PART_RightHeaderGripper
> 								65 헤더 항목 Raw Value
> 									66 텍스트 Raw Value
> 									67 엄지 ID: PART_LeftHeaderGripper
> 									68 엄지 ID: PART_RightHeaderGripper
> 								69 헤더 항목 Status
> 									70 텍스트 Status
> 									71 엄지 ID: PART_LeftHeaderGripper
> 									72 엄지 ID: PART_RightHeaderGripper
> 								73 헤더 항목 Detail
> 									74 텍스트 Detail
> 									75 엄지 ID: PART_LeftHeaderGripper
> 									76 엄지 ID: PART_RightHeaderGripper
> 					77 그룹 Bulk qualification automation
> 						78 텍스트 Bulk qualification automation
> 						79 텍스트 The runner reloads Capabilities and the PI Catalog, selects exactly all 24 BulkReadable entries in Catalog order, uses the revision-bound builder facade, and always releases the reader. It does not inject an EtherCAT slave fault.
> 						80 텍스트 Iterations
> 						81 편집 (disabled) ID: TextQualificationBulkIterations
> 						82 텍스트 Interval (ms)
> 						83 편집 (disabled) ID: TextQualificationBulkIntervalMs
> 						84 단추 (disabled) Run 24-entry Snapshot Soak ID: ButtonRunBulkSnapshotSoakQualification
> 							85 텍스트 (disabled) Run 24-entry Snapshot Soak
> 						86 단추 (disabled) Run Configure/Read/Release Soak ID: ButtonRunBulkLifecycleQualification
> 							87 텍스트 (disabled) Run Configure/Read/Release Soak
> 						88 단추 (disabled) Cancel Test ID: ButtonCancelBulkQualification
> 							89 텍스트 (disabled) Cancel Test
> 						90 단추 (disabled) Save QTEST Log ID: ButtonSaveBulkQualificationLog
> 							91 텍스트 (disabled) Save QTEST Log
> 						92 진행률 표시줄 ID: ProgressBulkQualification
> 						93 텍스트 No qualification has run yet. ID: TextBulkQualificationProgress
> 						94 편집 ID: TextBulkQualificationSummary
> 					95 스크롤 막대 ID: VerticalScrollBar
> 						96 단추 (disabled) ID: PART_LineUpButton
> 						97 단추 (disabled) ID: PageUp
> 						98 단추 (disabled) ID: PageDown
> 						99 엄지 (disabled)
> 						100 단추 (disabled) ID: PART_LineDownButton
> 			101 탭 항목 (selectable) Recorder
> 				102 텍스트 Recorder
> 			103 탭 항목 (selectable) SDO / Write Policy
> 				104 텍스트 SDO / Write Policy
> 			105 탭 항목 (selectable) Read-only API
> 				106 텍스트 Read-only API
> 		107 그룹 Execution log / raw callback diagnostics
> 			108 단추 Execution log / raw callback diagnostics ID: HeaderSite
> 				109 텍스트 Execution log / raw callback diagnostics
> 		110 상태 표시줄
> 			111 텍스트 Ready ID: TextOperationState
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
>   "code": "var recorderTabObservation2 = globalThis.state;\nglobalThis.state = null;\nawait sky.click({ window: recorderTabObservation2.window, element_index: 101 });\nglobalThis.state = await sky.get_window_state({ window: recorderTabObservation2.window, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));",
>   "title": "Recorder Qualification 화면 확인",
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
> 			34 탭 항목 (selectable) EtherCAT / PI
> 				35 텍스트 EtherCAT / PI
> 			36 탭 항목 (selectable) Bulk Snapshot
> 				37 텍스트 Bulk Snapshot
> 			38 탭 항목 (selectable) Recorder
> 				39 텍스트 Recorder
> 				40 창 ID: ScrollRecorder
> 					41 그룹 Recorder configuration
> 						42 텍스트 Recorder configuration
> 						43 텍스트 Sample period (cycles)
> 						44 편집 ID: TextRecorderSamplePeriod
> 						45 텍스트 Sample capacity
> 						46 편집 ID: TextRecorderSampleCapacity
> 						47 텍스트 Buffer mode
> 						48 콤보 상자 ID: ComboRecorderBufferMode
> 						49 텍스트 Trigger type
> 						50 콤보 상자 ID: ComboRecorderTriggerType
> 						51 텍스트 Trigger operator
> 						52 콤보 상자 (disabled) ID: ComboRecorderTriggerOperator
> 						53 텍스트 Trigger signal
> 						54 콤보 상자 (disabled) ID: ComboRecorderTriggerSignal
> 						55 텍스트 Pre-trigger samples
> 						56 편집 (disabled) ID: TextRecorderPreTrigger
> 						57 텍스트 Post-trigger samples
> 						58 편집 (disabled) ID: TextRecorderPostTrigger
> 						59 텍스트 Trigger value (ignored in Manual mode) ID: TextRecorderTriggerValueLabel
> 						60 편집 (disabled) ID: TextRecorderTriggerValue
> 						61 텍스트 Trigger mask (ignored in Manual mode) ID: TextRecorderTriggerMaskLabel
> 						62 편집 (disabled) ID: TextRecorderTriggerMask
> 						63 텍스트 Manual ignores all trigger fields. Edge uses TriggerValue as its threshold and forces TriggerMask to zero. Window maps lower bound to TriggerValue and upper bound to TriggerMask; signed Int16/Int32 bounds accept signed decimal input. Mask requires a BitField16/32 signal and a non-zero TriggerMask.
> 						64 텍스트 Uses checked Recordable signals from the PI Catalog. Trigger and Double modes are enabled only when advertised by the PLC.
> 						65 텍스트 Estimate: channels=0, raw=0 bytes (0.00 MiB), duration=1.000 s @ base cycle 1000 us. Capabilities not loaded. ID: TextRecorderEstimate
> 						66 단추 (disabled) Configure ID: ButtonConfigureRecorder
> 							67 텍스트 (disabled) Configure
> 						68 단추 (disabled) Start ID: ButtonStartRecorder
> 							69 텍스트 (disabled) Start
> 						70 단추 (disabled) Trigger Now ID: ButtonTriggerRecorder
> 							71 텍스트 (disabled) Trigger Now
> 						72 단추 (disabled) Stop ID: ButtonStopRecorder
> 							73 텍스트 (disabled) Stop
> 						74 단추 (disabled) Refresh Status ID: ButtonRecorderStatus
> 							75 텍스트 (disabled) Refresh Status
> 						76 단추 (disabled) Read Header ID: ButtonReadRecorderHeader
> 							77 텍스트 (disabled) Read Header
> 						78 단추 (disabled) Download ID: ButtonDownloadRecorder
> 							79 텍스트 (disabled) Download
> 						80 단추 (disabled) Cancel Download ID: ButtonCancelRecorderDownload
> 							81 텍스트 (disabled) Cancel Download
> 						82 단추 (disabled) Export CSV ID: ButtonExportRecorderCsv
> 							83 텍스트 (disabled) Export CSV
> 						84 단추 (disabled) Release ID: ButtonReleaseRecorder
> 							85 텍스트 (disabled) Release
> 						86 텍스트 Download copies a frozen PLC recording into this app's PC memory; it does not create a file. Export CSV opens a Save dialog and writes the downloaded PC data to the file you choose.
> 						87 진행률 표시줄 ID: ProgressRecorderDownload
> 						88 텍스트 Load the PI Catalog and check Recordable signals first. ID: TextRecorderSummary
> 					89 그룹 Reconnect / adopt existing Recorder
> 						90 텍스트 Reconnect / adopt existing Recorder
> 						91 텍스트 Diagnostics Boot ID
> 						92 편집 ID: TextRecorderAdoptBootId
> 						93 텍스트 Record ID
> 						94 편집 ID: TextRecorderAdoptRecordId
> 						95 텍스트 Buffer ID
> 						96 편집 ID: TextRecorderAdoptBufferId
> 						97 단추 (disabled) Adopt ID: ButtonAdoptRecorder
> 							98 텍스트 (disabled) Adopt
> 						99 텍스트 Start fills these IDs automatically and they remain visible after disconnect. Reconnect to the same PLC boot, refresh Capabilities, then Adopt. Record ID=0 and Buffer ID=0 discovers and adopts the current single-bank Recorder; nonzero Record ID keeps exact adoption. Read Status for authoritative terminal metadata or Header before Download. Trigger Now explicitly fires a locally configured non-Manual D4 recorder and is enabled only when RecorderTrigger is advertised.
> 					100 그룹 Downloaded raw sample plot
> 						101 텍스트 Downloaded raw sample plot
> 						102 텍스트 Signal
> 						103 콤보 상자 ID: ComboRecorderPlotSignal
> 						104 텍스트 No downloaded data. ID: TextRecorderPlotRange
> 					105 그룹 Recorder qualification automation
> 						106 텍스트 Recorder qualification automation
> 						107 텍스트 Single validates two identical downloads. Ring uses the advertised local forced-trigger path. Trigger Soak repeats a compact 32-sample lifecycle and releases both buffer and configuration every cycle. Reconnect/adopt and external RT evidence remain separate operator workflows.
> 						108 텍스트 Soak iterations
> 						109 편집 (disabled) ID: TextQualificationRecorderIterations
> 						110 텍스트 Refresh Capabilities and load the PI Catalog. ID: TextQualificationRecorderCapability
> 						111 단추 (disabled) Run Single Manual ID: ButtonRunRecorderSingleQualification

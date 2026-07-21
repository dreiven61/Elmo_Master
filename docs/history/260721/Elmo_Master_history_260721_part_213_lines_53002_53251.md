> 							95 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								96 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								97 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								98 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								99 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								100 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								101 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								102 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								103 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								104 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								105 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								106 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								107 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								108 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								109 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								110 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								111 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								112 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								113 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								114 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								115 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								116 콘솔 트리 (selectable) ALARM:00, Empty
> 								117 콘솔 트리 (selectable) SDIAS:00, Empty
> 								118 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								119 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							120 콘솔 트리 (selectable) Unplaced Module(s)
> 			121 창 Elmo_4 Secondary Actions: Raise ID: 65285
> 				122 창 ID: 59648
> 					123 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						124 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							125 단추 위쪽 스크롤 화살표 ID: UpButton
> 							126 위치 조정 위치 ID: ScrollbarThumb
> 							127 단추 페이지 아래로 ID: DownPageButton
> 							128 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						129 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							130 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							131 위치 조정 위치 ID: ScrollbarThumb
> 							132 단추 페이지 오른쪽으로 ID: DownPageButton
> 							133 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						134 위치 조정 (disabled)
> 			135 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				136 창 ID: 59648
> 					137 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						138 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							139 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							140 위치 조정 위치 ID: ScrollbarThumb
> 							141 단추 페이지 오른쪽으로 ID: DownPageButton
> 							142 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			143 창 HW_Network Secondary Actions: Raise ID: 65281
> 				144 창 ID: 59648
> 					145 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						146 단추 위쪽 스크롤 화살표 ID: UpButton
> 						147 위치 조정 위치 ID: ScrollbarThumb
> 						148 단추 페이지 아래로 ID: DownPageButton
> 						149 단추 아래쪽 스크롤 화살표 ID: DownButton
> 		150 상태 표시줄 ID: 59393
> 			151 텍스트
> 			152 텍스트
> 			153 텍스트
> 			154 텍스트 Ln 1 Col 1
> 			155 텍스트
> 			156 텍스트 Offline
> 			157 텍스트
> 			158 텍스트 NUM
> 			159 텍스트
> 		160 창 xtpBarTop ID: 59419
> 			161 도구 모음 Edit
> 				162 단추 Toggle bookmark
> 				163 단추 (disabled) Previous bookmark
> 				164 단추 (disabled) Next bookmark
> 				165 단추 (disabled) Delete all bookmarks
> 				166 단추 (disabled) Previous bookmark in this file
> 				167 단추 (disabled) Next bookmark in this file
> 				168 단추 Comment selected text (Ctrl+Shift+C)
> 				169 단추 Remove comment (Ctrl+Shift+X)
> 				170 단추 Unindent (Shift+Tab)
> 				171 단추 Indent (Tab)
> 			172 도구 모음 Macros Manager
> 				173 메뉴 항목 Macros
> 			174 도구 모음 Layout Manager
> 				175 메뉴 항목 Layouts
> 			176 도구 모음 Toolbox
> 				177 단추 DataAnalyzer
> 				178 단추 Interpreter
> 				179 단추 DiasDrive
> 				180 단추 PLC Diagnosis
> 				181 단추 Hardware Editor
> 				182 단추 Graphical Hardware Editor
> 				183 단추 Connection Manager
> 				184 단추 Task Configuration
> 			185 도구 모음 Net Edit
> 				186 단추 (disabled) Select
> 				187 단추 (disabled) Move view
> 				188 단추 (disabled) Insert comment
> 				189 단추 (disabled) Zoom(+/-)
> 				190 단추 (disabled) Zoom to all
> 				191 단추 (disabled) Zoom selection
> 			192 도구 모음 Debug
> 				193 단추 Go online (Alt+F6)
> 				194 단추 Change Online Settings
> 				195 메뉴 항목 Online Connection
> 				196 단추 (disabled) Set Online Connection For Current Project
> 				197 단추 (disabled) Download (F6)
> 				198 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				199 단추 (disabled) Download Module on the Fly
> 				200 단추 (disabled) Save Project on PLC
> 				201 단추 (disabled) Start (F7)
> 				202 단추 (disabled) Reset (F8)
> 				203 단추 Toggle breakpoint (F4)
> 				204 단추 Create condition breakpoint
> 				205 단추 Remove all breakpoint(s) (Shift+F4)
> 				206 단추 AWL trigger on/off
> 				207 단추 (disabled) Fix AWL trigger to current instruction
> 				208 단추 Activate/Deactivate Live View
> 				209 단추 Activate/Deactivate Auto Watch
> 				210 단추 (disabled) Goto instruction pointer
> 				211 단추 (disabled) Step into (F5)
> 				212 단추 (disabled) Step over (Alt+F5)
> 				213 단추 (disabled) Step out (Shift+F5)
> 				214 단추 (disabled) Set instruction pointer
> 			215 도구 모음 Build
> 				216 메뉴 항목 Target Architecture
> 				217 단추 Build changes (F9)
> 				218 단추 Rebuild project (Strg+F9)
> 				219 단추 (disabled) Cancel building (Ctrl+Break)
> 				220 단추 Link project
> 			221 도구 모음 Standard
> 				222 단추 New project (Strg+N)
> 				223 단추 Open a file (Strg+Shift+O)
> 				224 단추 Close active document (Strg+F4)
> 				225 단추 (disabled) Save file (Strg+S)
> 				226 단추 Open project (Strg+O)
> 				227 단추 Save project changes (Strg+Shift+S)
> 				228 단추 Close project
> 				229 단추 Print
> 				230 단추 Cut (Strg+X)
> 				231 단추 Copy (Strg+C)
> 				232 단추 (disabled) Paste (Strg+V)
> 				233 메뉴 항목 (disabled) Undo (Strg+Z)
> 				234 메뉴 항목 (disabled) Redo (Strg+Y)
> 				235 단추 Navigate Backward (Alt+Left)
> 				236 단추 (disabled) Navigate Forward (Alt +Right)
> 			237 메뉴 모음 Menu Bar
> 				238 메뉴 항목 FILE
> 				239 메뉴 항목 EDIT
> 				240 메뉴 항목 VIEW
> 				241 메뉴 항목 PROJECT
> 				242 메뉴 항목 BUILD
> 				243 메뉴 항목 DEBUG
> 				244 메뉴 항목 ANALYZE
> 				245 메뉴 항목 TOOLS
> 				246 메뉴 항목 EXTRAS
> 				247 메뉴 항목 WINDOW
> 				248 메뉴 항목 HELP
> 		249 창 Splitter ID: 608798232
> 		250 창 Splitter ID: 608802432
> 		251 Tab Output ID: 1112619832
> 			252 창 ID: 1200
> 				253 창 ID: 1200
> 					254 LIST ID: 1204
> 						255 목록 항목 (selectable)
> 						256 목록 항목 (selectable)
> 						257 목록 항목 (selectable)
> 					258 스크롤 막대 (disabled) ID: 59904
> 						259 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						260 위치 조정 위치 ID: ScrollbarThumb
> 						261 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			262 탭 항목 (selectable) Python Script
> 			263 탭 항목 (selectable) Debugger
> 			264 탭 항목 (selectable) Output
> 			265 단추 Close
> 		266 창 Splitter ID: 608798736
> 		267 Tab Class View ID: 1112624848
> 			268 트리 ID: 103
> 				269 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					270 콘솔 트리 (selectable) External
> 					271 콘솔 트리 (selectable) Sigmatek
> 					272 콘솔 트리 (selectable) _TCPIPServer_RT
> 					273 콘솔 트리 (selectable) Elmo_1
> 					274 콘솔 트리 (selectable) Elmo_2
> 					275 콘솔 트리 (selectable) Elmo_3
> 					276 콘솔 트리 (selectable) Elmo_4
> 					277 콘솔 트리 (selectable) LMCDiagnosticsService
> 					278 콘솔 트리 (selectable) LMCEcatInputLatch
> 					279 콘솔 트리 (selectable) LMCRecorderStore
> 					280 콘솔 트리 (selectable) TCPMotionInterface
> 			281 탭 항목 (selectable) Lib
> 			282 탭 항목 (selectable) File
> 			283 탭 항목 (selectable) Global
> 			284 탭 항목 (selectable) Net
> 			285 탭 항목 (selectable) Class
> 			286 단추 Close
> 		287 Tab Properties ID: 1112621200
> 			288 창 ID: 121918456
> 				289 TABLE Properties Window ID: 127184512
> 					290 custom Name
> 					291 custom Revision
> 					292 custom GUID
> 					293 custom Task Settings
> 					294 custom Sigmatek
> 					295 custom OSInterface
> 					296 custom IconPath
> 					297 custom SharedCommandTable
> 					298 custom Objectsize
> 					299 custom Singleton
> 					300 custom Hidden
> 					301 custom Deprecated
> 					302 custom GCCOptions
> 					303 custom Comment
> 					304 custom Filename
> 				305 도구 모음 ID: 59392
> 					306 단추
> 					307 단추
> 			308 탭 항목 (selectable) Properties
> 			309 단추 Close
>
> The focused UI element is 4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_RECORDER_SCHEMA_VERSION 1 #define LMC_RECORDER_MAP_REVISION 0x957F101E #define LMC_RECORDER_ERROR_ID -32000 #define LMC_RECORDER_STORAGE_BYTES 1280000 #define LMC_RECORDER_MAX_CHANNELS 24 #define LMC_RECORDER_MAX_CHUNK_BYTES 1280 #define LMC_RECORDER_EMPTY 0 #define LMC_RECORDER_CONFIGURED 1 #define LMC_RECORDER_ARMED 2 #define LMC_RECORDER_RECORDING 3 #define LMC_RECORDER_READY 4 #define LMC_RECORDER_UPLOADING 5 #define LMC_RECORDER_FAULT 6 #define LMC_RECORDER_STOP_NONE 0 #define LMC_RECORDER_STOP_COUNT_COMPLETE 1 #define LMC_RECORDER_STOP_USER 2 #define LMC_RECORDER_STOP_TRIGGER_COMPLETE 3 // The data bank is global so the generated class object stays below the // 16-bit object-size field used by the LASAL class table. Exactly one // LMCRecorderStore object is allowed in the project. VAR_GLOBAL g_LMCRecorderData : ARRAY [0..1279999] OF USINT; END_VAR FUNCTION GLOBAL LMCRecorderStore::NotifySessionClosed VAR_INPUT SessionEpoch : UDINT; END_VAR if (SessionEpoch <> 0) (SessionEpoch = OwnerSessionEpoch) then ClosedSessionEpoch := SessionEpoch; end_if; END_FUNCTION FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot VAR_INPUT pSnapshot : ^USINT; SnapshotSize : UDINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR state : UDINT; startRequest : UDINT; triggerRequest : UDINT; stopRequest : UDINT; sequence : UDINT; cycleCounter : UDINT; channelIndex : UINT; dataOffset : UDINT; snapshotOffset : UDINT; triggerRaw : UDINT; triggerEvent : BOOL; previousCondition : BOOL; currentCondition : BOOL; prehistoryReady : BOOL; previousSigned : DINT; currentSigned : DINT; thresholdSigned : DINT; lowerSigned : DINT; upperSigned : DINT; timestampStep : UDINT; timestampLowBefore : UDINT; END_VAR Result := -1; if (pSnapshot = NIL) | (SnapshotSize < 304) then RETURN; end_if; state := sigclib_atomic_getU32(pValue:=#StateValue); startRequest := sigclib_atomic_getU32(pValue:=#StartRequestSequence); triggerRequest := sigclib_atomic_getU32(pValue:=#TriggerRequestSequence); stopRequest := sigclib_atomic_getU32(pValue:=#StopRequestSequence); sequence := sigclib_atomic_getU32(pValue:=#StatusSequence) + 1; if (sequence and 1) = 0 then sequence += 1; end_if; sigclib_atomic_setU32(pValue:=#StatusSequence, value:=sequence); cycleCounter := pSnapshot^$UDINT; if (state = LMC_RECORDER_ARMED) & (startRequest <> StartAppliedSequence) then StartAppliedSequence := startRequest; SampleCount := 0; DividerCounter := 0; StopReason := LMC_RECORDER_STOP_NONE; TriggerIndex := 0xFFFFFFFF; TriggerCycle := 0; TriggerTimestampLow := 0; TriggerTimestampHigh := 0; StartCycle := 0; EndCycle := 0; StartTimestampLow := 0; StartTimestampHigh := 0; EndTimestampLow := 0; EndTimestampHigh := 0; DroppedCycles := 0; OverflowCount := 0; WriteSampleIndex := 0; FrozenFirstSampleIndex := 0; PostSamplesRemaining := 0; PreviousTriggerValue := 0; PreviousTriggerValid := FALSE; state := LMC_RECORDER_RECORDING; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_RECORDING); end_if; if ((state = LMC_RECORDER_ARMED) | (state = LMC_RECORDER_RECORDING)) & (stopRequest <> StopAppliedSequence) then StopAppliedSequence := stopRequest; StopReason := LMC_RECORDER_STOP_USER; EndCycle := cycleCounter; EndTimestampLow := (pSnapshot + 4)^$UDINT; EndTimestampHigh := (pSnapshot + 8)^$UDINT; state := LMC_RECORDER_READY; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_READY); end_if; if state = LMC_RECORDER_RECORDING then if DividerCounter = 0 then if TriggerType = 0 then if SampleCount < SampleCapacity then if SampleCount = 0 then StartCycle := cycleCounter; StartTimestampLow := (pSnapshot + 4)^$UDINT; StartTimestampHigh := (pSnapshot + 8)^$UDINT; end_if; for channelIndex := 0 to ChannelCount - 1 do dataOffset := WriteSampleIndex * TO_UDINT(SampleStrideBytes) + TO_UDINT(channelIndex) * 4; ID: 10000.
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 편집 입력 API 확인",
>   "code": "var apiText = await sky.documentation('api'); nodeRepl.write(apiText);"
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

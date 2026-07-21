> 				204 단추 Print
> 				205 단추 Cut (Strg+X)
> 				206 단추 Copy (Strg+C)
> 				207 단추 (disabled) Paste (Strg+V)
> 				208 메뉴 항목 (disabled) Undo (Strg+Z)
> 				209 메뉴 항목 (disabled) Redo (Strg+Y)
> 				210 단추 Navigate Backward (Alt+Left)
> 				211 단추 (disabled) Navigate Forward (Alt +Right)
> 			212 메뉴 모음 Menu Bar
> 				213 메뉴 항목 FILE
> 				214 메뉴 항목 EDIT
> 				215 메뉴 항목 VIEW
> 				216 메뉴 항목 PROJECT
> 				217 메뉴 항목 BUILD
> 				218 메뉴 항목 DEBUG
> 				219 메뉴 항목 ANALYZE
> 				220 메뉴 항목 TOOLS
> 				221 메뉴 항목 EXTRAS
> 				222 메뉴 항목 WINDOW
> 				223 메뉴 항목 HELP
> 		224 창 Splitter ID: 411855768
> 		225 창 Splitter ID: 411851736
> 		226 Tab Output ID: 409867992
> 			227 창 ID: 1200
> 				228 창 ID: 1200
> 					229 LIST ID: 1201
> 						230 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							231 단추 위쪽 스크롤 화살표 ID: UpButton
> 							232 단추 페이지 위로 ID: UpPageButton
> 							233 위치 조정 위치 ID: ScrollbarThumb
> 							234 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						235 목록 항목 (selectable)
> 						236 목록 항목 (selectable)
> 						237 목록 항목 (selectable)
> 						238 목록 항목 (selectable)
> 						239 목록 항목 (selectable)
> 						240 목록 항목 (selectable)
> 						241 목록 항목 (selectable)
> 						242 목록 항목 (selectable)
> 						243 목록 항목 (selectable)
> 						244 목록 항목 (selectable)
> 						245 목록 항목 (selectable)
> 						246 목록 항목 (selectable)
> 						247 목록 항목 (selectable)
> 					248 스크롤 막대 ID: 59904
> 						249 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						250 위치 조정 위치 ID: ScrollbarThumb
> 						251 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			252 탭 항목 (selectable) Python Script
> 			253 탭 항목 (selectable) Output
> 			254 탭 항목 (selectable) Debugger
> 			255 단추 Close
> 		256 창 Splitter ID: 411854424
> 		257 Tab Class View ID: 409868448
> 			258 트리 ID: 103
> 				259 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					260 콘솔 트리 (selectable) External
> 					261 콘솔 트리 (selectable) Sigmatek
> 					262 콘솔 트리 (selectable) _TCPIPServer_RT
> 					263 콘솔 트리 (selectable) Elmo_1
> 					264 콘솔 트리 (selectable) Elmo_2
> 					265 콘솔 트리 (selectable) Elmo_3
> 					266 콘솔 트리 (selectable) Elmo_4
> 					267 콘솔 트리 (selectable) LMCDiagnosticsService
> 					268 콘솔 트리 (selectable) LMCEcatInputLatch
> 					269 콘솔 트리 (selectable) LMCRecorderStore
> 					270 콘솔 트리 (selectable) TCPMotionInterface
> 			271 탭 항목 (selectable) Lib
> 			272 탭 항목 (selectable) File
> 			273 탭 항목 (selectable) Global
> 			274 탭 항목 (selectable) Net
> 			275 탭 항목 (selectable) Class
> 			276 단추 Close
> 		277 Tab Properties ID: 409871640
> 			278 창 ID: 121918456
> 				279 TABLE Properties Window ID: 127184512
> 				280 도구 모음 ID: 59392
> 					281 단추
> 					282 단추
> 			283 탭 항목 (selectable) Properties
> 			284 단추 Close
>
> The focused UI element is 4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> 프로젝트를 다시 열자 IDE가 최신 소스를 읽어 자동 재검사를 수행했고, 현재 1개 컴파일 오류가 확인됐습니다. 이 오류부터 정확히 잡은 뒤 재빌드하겠습니다. 동시에 독립 리뷰에서 발견된 write allowlist와 비동기 취소 경계 문제도 코드 수정에 들어갔습니다.
>
> <details><summary>Called 24 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 진단 클래스 메타데이터 확인",
>   "code": "{\n  var observationClass = globalThis.state;\n  globalThis.state = null;\n  await sky.perform_secondary_action({ window: observationClass.window, element_index: 267, action: \"Expand\" });\n  globalThis.state = await sky.get_window_state({ window: observationClass.window, include_screenshot: false, include_text: true });\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ```text
> element 267 has no cached secondary actions for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 진단 클래스 선택",
>   "code": "{\n  var observationClass2 = globalThis.state;\n  globalThis.state = null;\n  await sky.click({ window: observationClass2.window, element_index: 267 });\n  globalThis.state = await sky.get_window_state({ window: observationClass2.window, include_screenshot: false, include_text: true });\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ```text
> Cannot read properties of null (reading 'window')
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 진단 클래스 다시 확인",
>   "code": "globalThis.state = await sky.get_window_state({ window: globalThis.targetWindow, include_screenshot: false, include_text: true });\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65287
> 				3 창 ID: 59648
> 					4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 단추 페이지 위로 ID: UpPageButton
> 							8 위치 조정 위치 ID: ScrollbarThumb
> 							9 단추 페이지 아래로 ID: DownPageButton
> 							10 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						11 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							12 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							13 위치 조정 위치 ID: ScrollbarThumb
> 							14 단추 페이지 오른쪽으로 ID: DownPageButton
> 							15 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						16 위치 조정 (disabled)
> 			17 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				18 창 ID: 59648
> 					19 창 xtpBarTop ID: 59419
> 						20 도구 모음 Hardware Editor
> 							21 단추 Hardware Editor Configuration Settings
> 							22 단추 Calculate Resources of Project
> 							23 단추 (disabled) Upload Hardware Tree from PLC
> 							24 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							25 단추 Show Detail Mode\Show Configuration Mode
> 							26 단추 Generates the ENI File of the current project
> 					27 창 ID: 59648
> 						28 트리 ID: 1
> 							29 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								30 단추 위쪽 스크롤 화살표 ID: UpButton
> 								31 단추 페이지 위로 ID: UpPageButton
> 								32 위치 조정 위치 ID: ScrollbarThumb
> 								33 단추 페이지 아래로 ID: DownPageButton
> 								34 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							35 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								36 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								37 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								38 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								39 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								40 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								41 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								42 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								43 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								44 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								45 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								46 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								47 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								48 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								49 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								50 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								51 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								52 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								53 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								54 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								55 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								56 콘솔 트리 (selectable) ALARM:00, Empty
> 								57 콘솔 트리 (selectable) SDIAS:00, Empty
> 								58 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								59 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							60 콘솔 트리 (selectable) Unplaced Module(s)
> 			61 창 Elmo_4 Secondary Actions: Raise ID: 65285
> 				62 창 ID: 59648
> 					63 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						64 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							65 단추 위쪽 스크롤 화살표 ID: UpButton
> 							66 위치 조정 위치 ID: ScrollbarThumb
> 							67 단추 페이지 아래로 ID: DownPageButton
> 							68 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						69 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							70 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							71 위치 조정 위치 ID: ScrollbarThumb
> 							72 단추 페이지 오른쪽으로 ID: DownPageButton
> 							73 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						74 위치 조정 (disabled)
> 			75 창 Motion_Network Secondary Actions: Raise ID: 65284
> 				76 창 ID: 59648
> 					77 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						78 단추 위쪽 스크롤 화살표 ID: UpButton
> 						79 위치 조정 위치 ID: ScrollbarThumb
> 						80 단추 페이지 아래로 ID: DownPageButton
> 						81 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					82 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						83 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						84 위치 조정 위치 ID: ScrollbarThumb
> 						85 단추 페이지 오른쪽으로 ID: DownPageButton
> 						86 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					87 위치 조정 (disabled)
> 			88 창 Comm_Network Secondary Actions: Raise ID: 65283
> 				89 창 ID: 59648
> 					90 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						91 단추 위쪽 스크롤 화살표 ID: UpButton
> 						92 위치 조정 위치 ID: ScrollbarThumb
> 						93 단추 페이지 아래로 ID: DownPageButton
> 						94 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					95 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						96 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						97 위치 조정 위치 ID: ScrollbarThumb
> 						98 단추 페이지 오른쪽으로 ID: DownPageButton
> 						99 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					100 위치 조정 (disabled)
> 			101 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				102 창 ID: 59648
> 					103 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						104 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							105 단추 위쪽 스크롤 화살표 ID: UpButton
> 							106 위치 조정 위치 ID: ScrollbarThumb
> 							107 단추 페이지 아래로 ID: DownPageButton
> 							108 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						109 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							110 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							111 위치 조정 위치 ID: ScrollbarThumb
> 							112 단추 페이지 오른쪽으로 ID: DownPageButton
> 							113 단추 오른쪽 스크롤 화살표 ID: DownButton

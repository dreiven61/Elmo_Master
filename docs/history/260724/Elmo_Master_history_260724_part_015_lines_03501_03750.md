> 						118 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							119 단추 위쪽 스크롤 화살표 ID: UpButton
> 							120 단추 페이지 위로 ID: UpPageButton
> 							121 위치 조정 위치 ID: ScrollbarThumb
> 							122 단추 페이지 아래로 ID: DownPageButton
> 							123 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						124 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							125 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							126 위치 조정 위치 ID: ScrollbarThumb
> 							127 단추 페이지 오른쪽으로 ID: DownPageButton
> 							128 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						129 위치 조정 (disabled)
> 			130 창 Elmo_2 Secondary Actions: Raise ID: 65284
> 				131 창 ID: 59648
> 					132 창 FUNCTION VIRTUAL GLOBAL Elmo_2::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_2::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_2_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_2_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_2::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_2_SETPOS_INDEX, ELMO_2_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_2_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_2_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_2_CONTROLWORD_INDEX, ELMO_2_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_2_ACTPOS_INDEX, ELMO_2_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_2_INPUTS_DIGITALINPUTS_INDEX, ELMO_2_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_2_STATEWORD_INDEX, ELMO_2_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						133 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							134 단추 위쪽 스크롤 화살표 ID: UpButton
> 							135 위치 조정 위치 ID: ScrollbarThumb
> 							136 단추 페이지 아래로 ID: DownPageButton
> 							137 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						138 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							139 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							140 위치 조정 위치 ID: ScrollbarThumb
> 							141 단추 페이지 오른쪽으로 ID: DownPageButton
> 							142 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						143 위치 조정 (disabled)
> 		144 상태 표시줄 ID: 59393
> 			145 텍스트
> 			146 텍스트 LMCEcatInputLatch::RtWork
> 			147 텍스트
> 			148 텍스트 Ln 18 Col 11
> 			149 텍스트
> 			150 텍스트
> 			151 텍스트
> 			152 텍스트
> 			153 텍스트 NUM
> 			154 텍스트
> 		155 창 xtpBarTop ID: 59419
> 			156 도구 모음 Script
> 			157 도구 모음 Edit
> 				158 단추 Toggle bookmark
> 				159 단추 (disabled) Previous bookmark
> 				160 단추 (disabled) Next bookmark
> 				161 단추 (disabled) Delete all bookmarks
> 				162 단추 (disabled) Previous bookmark in this file
> 				163 단추 (disabled) Next bookmark in this file
> 				164 단추 Comment selected text (Ctrl+Shift+C)
> 				165 단추 Remove comment (Ctrl+Shift+X)
> 				166 단추 Unindent (Shift+Tab)
> 				167 단추 Indent (Tab)
> 			168 도구 모음 Macros Manager
> 				169 메뉴 항목 Macros
> 			170 도구 모음 Layout Manager
> 				171 메뉴 항목 Layouts
> 			172 도구 모음 Toolbox
> 				173 단추 DataAnalyzer
> 				174 메뉴 항목 Toolbar Options
> 			175 도구 모음 Net Edit
> 				176 단추 (disabled) Select
> 				177 메뉴 항목 Toolbar Options
> 			178 도구 모음 Debug
> 				179 단추 Go online (Alt+F6)
> 				180 단추 Change Online Settings
> 				181 메뉴 항목 Online Connection
> 				182 단추 (disabled) Set Online Connection For Current Project
> 				183 단추 (disabled) Download (F6)
> 				184 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				185 단추 (disabled) Download Module on the Fly
> 				186 단추 (disabled) Save Project on PLC
> 				187 단추 (disabled) Start (F7)
> 				188 단추 (disabled) Reset (F8)
> 				189 단추 Toggle breakpoint (F4)
> 				190 단추 Create condition breakpoint
> 				191 메뉴 항목 Toolbar Options
> 			192 도구 모음 Build
> 				193 메뉴 항목 Target Architecture
> 				194 단추 Build changes (F9)
> 				195 단추 Rebuild project (Strg+F9)
> 				196 단추 (disabled) Cancel building (Ctrl+Break)
> 				197 단추 Link project
> 			198 도구 모음 Standard
> 				199 단추 New project (Strg+N)
> 				200 단추 Open a file (Strg+Shift+O)
> 				201 단추 Close active document (Strg+F4)
> 				202 단추 (disabled) Save file (Strg+S)
> 				203 단추 Open project (Strg+O)
> 				204 단추 (disabled) Save project changes (Strg+Shift+S)
> 				205 단추 Close project
> 				206 단추 Print
> 				207 단추 Cut (Strg+X)
> 				208 단추 Copy (Strg+C)
> 				209 단추 (disabled) Paste (Strg+V)
> 				210 메뉴 항목 (disabled) Undo (Strg+Z)
> 				211 메뉴 항목 (disabled) Redo (Strg+Y)
> 				212 단추 Navigate Backward (Alt+Left)
> 				213 단추 (disabled) Navigate Forward (Alt +Right)
> 			214 메뉴 모음 Menu Bar
> 				215 메뉴 항목 FILE
> 				216 메뉴 항목 EDIT
> 				217 메뉴 항목 VIEW
> 				218 메뉴 항목 PROJECT
> 				219 메뉴 항목 BUILD
> 				220 메뉴 항목 DEBUG
> 				221 메뉴 항목 ANALYZE
> 				222 메뉴 항목 TOOLS
> 				223 메뉴 항목 EXTRAS
> 				224 메뉴 항목 WINDOW
> 				225 메뉴 항목 HELP
> 		226 창 Splitter ID: 297810792
> 		227 창 Splitter ID: 297810456
> 		228 Tab Output ID: 295820960
> 			229 창 ID: 1200
> 				230 창 ID: 1200
> 					231 LIST ID: 1201
> 						232 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							233 단추 위쪽 스크롤 화살표 ID: UpButton
> 							234 단추 페이지 위로 ID: UpPageButton
> 							235 위치 조정 위치 ID: ScrollbarThumb
> 							236 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						237 목록 항목 (selectable)
> 						238 목록 항목 (selectable)
> 						239 목록 항목 (selectable)
> 						240 목록 항목 (selectable)
> 						241 목록 항목 (selectable)
> 					242 스크롤 막대 ID: 59904
> 						243 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						244 위치 조정 위치 ID: ScrollbarThumb
> 						245 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			246 탭 항목 (selectable) Python Script
> 			247 탭 항목 (selectable) Output
> 			248 탭 항목 (selectable) Debugger
> 			249 단추 Close
> 		250 창 Splitter ID: 297812808
> 		251 Tab Class View ID: 298069024
> 			252 트리 ID: 103
> 				253 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					254 단추 위쪽 스크롤 화살표 ID: UpButton
> 					255 위치 조정 위치 ID: ScrollbarThumb
> 					256 단추 페이지 아래로 ID: DownPageButton
> 					257 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				258 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					259 콘솔 트리 (selectable) External
> 					260 콘솔 트리 (selectable) Sigmatek
> 					261 콘솔 트리 (selectable) _TCPIPServer_RT
> 					262 콘솔 트리 (selectable) Elmo_1
> 					263 콘솔 트리 (selectable) Elmo_2
> 					264 콘솔 트리 (selectable) Elmo_3
> 					265 콘솔 트리 (selectable) Elmo_4
> 					266 콘솔 트리 (selectable) LMCDiagnosticsService
> 					267 콘솔 트리 (selectable) LMCEcatInputLatch
> 						268 콘솔 트리 (selectable) Servers
> 						269 콘솔 트리 (selectable) Clients
> 							270 콘솔 트리 (selectable) EcatMaster
> 							271 콘솔 트리 (selectable) Drive1
> 							272 콘솔 트리 (selectable) Drive2
> 							273 콘솔 트리 (selectable) Drive3
> 							274 콘솔 트리 (selectable) Drive4
> 							275 콘솔 트리 (selectable) RecorderStore
> 						276 콘솔 트리 (selectable) Methods
> 							277 콘솔 트리 (selectable) Global
> 								278 콘솔 트리 (selectable) RtWork
> 								279 콘솔 트리 (selectable) CopySnapshot
> 							280 콘솔 트리 (selectable) Private
> 						281 콘솔 트리 (selectable) Variables
> 						282 콘솔 트리 (selectable) Objects
> 							283 콘솔 트리 (selectable) LMCEcatInputLatch1
> 						284 콘솔 트리 (selectable) Dependencies
> 					285 콘솔 트리 (selectable) LMCRecorderStore
> 					286 콘솔 트리 (selectable) TCPMotionInterface
> 			287 탭 항목 (selectable) Lib
> 			288 탭 항목 (selectable) File
> 			289 탭 항목 (selectable) Global
> 			290 탭 항목 (selectable) Net
> 			291 탭 항목 (selectable) Class
> 			292 단추 Close
> 		293 Tab Properties ID: 298070392
> 			294 창 ID: 289142312
> 				295 TABLE Properties Window ID: 293586600
> 					296 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						297 단추 위쪽 스크롤 화살표 ID: UpButton
> 						298 위치 조정 위치 ID: ScrollbarThumb
> 						299 단추 페이지 아래로 ID: DownPageButton
> 						300 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					301 custom Name
> 					302 custom Class
> 					303 custom DataType
> 					304 custom Type
> 					305 custom Required
> 					306 custom DefaultInitValue
> 					307 custom Comment
> 				308 도구 모음 ID: 59392
> 					309 단추
> 					310 단추
> 			311 탭 항목 (selectable) Properties
> 			312 단추 Close
> 		313 창 ACallTip
>
> The focused UI element is 5 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> LASAL IDE에서는 `LMCEcatInputLatch`, `LMCRecorderStore`, `LMCDiagnosticsService` 구현 탭이 정상적으로 열리고 Rebuild/Link가 0 error로 끝났습니다. 이번 IDE 작업 뒤 새 `CInvalidArgException`도 없고, 로그에 남은 것은 15:02의 기존 기록뿐입니다. 다음 단계는 문서 상태를 0x3F/101개 테스트 기준으로 맞춘 뒤, D5 SDO Read는 쓰기 기능 없이 단일 티켓으로 분리해서 착수하는 것입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+11 -10)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+11 -8)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:lasalWindow,key:\"Escape\"}); nodeRepl.write(\"closed tip\");",
>   "title": "LASAL 팁 닫기"
> }
> ```
>
> ```text
> closed tip
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "lasalState = await sky.get_window_state({window:lasalWindow,include_screenshot:true,include_text:true}); nodeRepl.write(lasalState.accessibility.tree.split(\"\\n\").filter(x=>x.includes(\"LMCEcatInputLatch\")||x.includes(\"LMCRecorderStore\")||x.includes(\"LMCDiagnosticsService\")).join(\"\\n\"));",
>   "title": "LASAL 클래스 위치 확인"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch Secondary Actions: Raise
> 			2 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65281
> 					4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 			45 창 LMCRecorderStore Secondary Actions: Raise ID: 65282
> 					47 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_RECORDER_SCHEMA_VERSION 1 #define LMC_RECORDER_MAP_REVISION 0x957F101E #define LMC_RECORDER_ERROR_ID -32000 #define LMC_RECORDER_STORAGE_BYTES 1280000 #define LMC_RECORDER_MAX_CHANNELS 24 #define LMC_RECORDER_MAX_CHUNK_BYTES 1280 #define LMC_RECORDER_EMPTY 0 #define LMC_RECORDER_CONFIGURED 1 #define LMC_RECORDER_ARMED 2 #define LMC_RECORDER_RECORDING 3 #define LMC_RECORDER_READY 4 #define LMC_RECORDER_UPLOADING 5 #define LMC_RECORDER_FAULT 6 #define LMC_RECORDER_STOP_NONE 0 #define LMC_RECORDER_STOP_COUNT_COMPLETE 1 #define LMC_RECORDER_STOP_USER 2 #define LMC_RECORDER_STOP_TRIGGER_COMPLETE 3 // The data bank is global so the generated class object stays below the // 16-bit object-size field used by the LASAL class table. Exactly one // LMCRecorderStore object is allowed in the project. VAR_GLOBAL g_LMCRecorderData : ARRAY [0..1279999] OF USINT; END_VAR FUNCTION GLOBAL LMCRecorderStore::NotifySessionClosed VAR_INPUT SessionEpoch : UDINT; END_VAR if (SessionEpoch <> 0) (SessionEpoch = OwnerSessionEpoch) then ClosedSessionEpoch := SessionEpoch; end_if; END_FUNCTION FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot VAR_INPUT pSnapshot : ^USINT; SnapshotSize : UDINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR state : UDINT; startRequest : UDINT; triggerRequest : UDINT; stopRequest : UDINT; sequence : UDINT; cycleCounter : UDINT; channelIndex : UINT; dataOffset : UDINT; snapshotOffset : UDINT; triggerRaw : UDINT; triggerHealthOffset : UDINT; triggerEvent : BOOL; triggerInputValid : BOOL; previousCondition : BOOL; currentCondition : BOOL; prehistoryReady : BOOL; previousSigned : DINT; currentSigned : DINT; thresholdSigned : DINT; lowerSigned : DINT; upperSigned : DINT; timestampStep : UDINT; timestampLowBefore : UDINT; END_VAR Result := -1; if (pSnapshot = NIL) | (SnapshotSize < 304) then RETURN; end_if; state := sigclib_atomic_getU32(pValue:=#StateValue); startRequest := sigclib_atomic_getU32(pValue:=#StartRequestSequence); triggerRequest := sigclib_atomic_getU32(pValue:=#TriggerRequestSequence); stopRequest := sigclib_atomic_getU32(pValue:=#StopRequestSequence); sequence := sigclib_atomic_getU32(pValue:=#StatusSequence) + 1; if (sequence and 1) = 0 then sequence += 1; end_if; sigclib_atomic_setU32(pValue:=#StatusSequence, value:=sequence); cycleCounter := pSnapshot^$UDINT; if (state = LMC_RECORDER_ARMED) & (startRequest <> StartAppliedSequence) then StartAppliedSequence := startRequest; SampleCount := 0; DividerCounter := 0; StopReason := LMC_RECORDER_STOP_NONE; TriggerIndex := 0xFFFFFFFF; TriggerCycle := 0; TriggerTimestampLow := 0; TriggerTimestampHigh := 0; StartCycle := 0; EndCycle := 0; StartTimestampLow := 0; StartTimestampHigh := 0; EndTimestampLow := 0; EndTimestampHigh := 0; DroppedCycles := 0; OverflowCount := 0; WriteSampleIndex := 0; FrozenFirstSampleIndex := 0; PostSamplesRemaining := 0; PreviousTriggerValue := 0; PreviousTriggerValid := FALSE; state := LMC_RECORDER_RECORDING; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_RECORDING); end_if; if ((state = LMC_RECORDER_ARMED) | (state = LMC_RECORDER_RECORDING)) & (stopRequest <> StopAppliedSequence) then StopAppliedSequence := stopRequest; if (TriggerType <> 0) & (TriggerIndex = 0xFFFFFFFF) & (SampleCapacity > 0) then FrozenFirstSampleIndex := (WriteSampleIndex + SampleCapacity - SampleCount) MOD SampleCapacity; end_if; StopReason := LMC_RECORDER_STOP_USER; if SampleCount = 0 then EndCycle := cycleCounter; EndTimestampLow := (pSnapshot + 4)^$UDINT; EndTimestampHigh := (pSnapshot + 8)^$UDINT; end_if; state := LMC_RECORDER_READY; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_READY); end_if; if state = LMC_RECORDER_RECORDING then if DividerCounter = 0 then if TriggerType = 0 then if SampleCount < SampleCapacity then if SampleCount = 0 then Start ID: 10000
> 			86 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283

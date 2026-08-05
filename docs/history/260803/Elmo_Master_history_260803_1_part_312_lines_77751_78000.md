> 						115 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					116 위치 조정 (disabled)
> 			117 창 LMCSdoExecutor Secondary Actions: Raise ID: 65280
> 				118 창 ID: 59648
> 					119 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
> 						120 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							121 단추 위쪽 스크롤 화살표 ID: UpButton
> 							122 위치 조정 위치 ID: ScrollbarThumb
> 							123 단추 페이지 아래로 ID: DownPageButton
> 							124 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						125 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							126 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							127 위치 조정 위치 ID: ScrollbarThumb
> 							128 단추 페이지 오른쪽으로 ID: DownPageButton
> 							129 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						130 위치 조정 (disabled)
> 		131 상태 표시줄 ID: 59393
> 			132 텍스트
> 			133 텍스트 LMCEcatInputLatch::RtWork
> 			134 텍스트
> 			135 텍스트 Ln 145 Col 27
> 			136 텍스트
> 			137 텍스트 Offline
> 			138 텍스트
> 			139 텍스트 NUM
> 			140 텍스트
> 		141 창 xtpBarTop ID: 59419
> 			142 도구 모음 Edit
> 				143 단추 Toggle bookmark
> 				144 단추 (disabled) Previous bookmark
> 				145 단추 (disabled) Next bookmark
> 				146 단추 (disabled) Delete all bookmarks
> 				147 단추 (disabled) Previous bookmark in this file
> 				148 단추 (disabled) Next bookmark in this file
> 				149 단추 Comment selected text (Ctrl+Shift+C)
> 				150 단추 Remove comment (Ctrl+Shift+X)
> 				151 단추 Unindent (Shift+Tab)
> 				152 단추 Indent (Tab)
> 			153 도구 모음 Macros Manager
> 				154 메뉴 항목 Macros
> 			155 도구 모음 Layout Manager
> 				156 메뉴 항목 Layouts
> 			157 도구 모음 Toolbox
> 				158 단추 DataAnalyzer
> 				159 메뉴 항목 Toolbar Options
> 			160 도구 모음 Net Edit
> 				161 단추 (disabled) Select
> 				162 메뉴 항목 Toolbar Options
> 			163 도구 모음 Debug
> 				164 단추 Go online (Alt+F6)
> 				165 단추 Change Online Settings
> 				166 메뉴 항목 Online Connection
> 				167 단추 (disabled) Set Online Connection For Current Project
> 				168 단추 (disabled) Download (F6)
> 				169 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				170 단추 (disabled) Download Module on the Fly
> 				171 단추 (disabled) Save Project on PLC
> 				172 단추 (disabled) Start (F7)
> 				173 단추 (disabled) Reset (F8)
> 				174 단추 Toggle breakpoint (F4)
> 				175 단추 Create condition breakpoint
> 				176 메뉴 항목 Toolbar Options
> 			177 도구 모음 Build
> 				178 메뉴 항목 Target Architecture
> 				179 단추 Build changes (F9)
> 				180 단추 Rebuild project (Strg+F9)
> 				181 단추 (disabled) Cancel building (Ctrl+Break)
> 				182 단추 Link project
> 			183 도구 모음 Standard
> 				184 단추 New project (Strg+N)
> 				185 단추 Open a file (Strg+Shift+O)
> 				186 단추 Close active document (Strg+F4)
> 				187 단추 (disabled) Save file (Strg+S)
> 				188 단추 Open project (Strg+O)
> 				189 단추 (disabled) Save project changes (Strg+Shift+S)
> 				190 단추 Close project
> 				191 단추 Print
> 				192 단추 Cut (Strg+X)
> 				193 단추 Copy (Strg+C)
> 				194 단추 Paste (Strg+V)
> 				195 메뉴 항목 (disabled) Undo (Strg+Z)
> 				196 메뉴 항목 (disabled) Redo (Strg+Y)
> 				197 단추 Navigate Backward (Alt+Left)
> 				198 단추 (disabled) Navigate Forward (Alt +Right)
> 			199 메뉴 모음 Menu Bar
> 				200 메뉴 항목 FILE
> 				201 메뉴 항목 EDIT
> 				202 메뉴 항목 VIEW
> 				203 메뉴 항목 PROJECT
> 				204 메뉴 항목 BUILD
> 				205 메뉴 항목 DEBUG
> 				206 메뉴 항목 ANALYZE
> 				207 메뉴 항목 TOOLS
> 				208 메뉴 항목 EXTRAS
> 				209 메뉴 항목 WINDOW
> 				210 메뉴 항목 HELP
> 		211 창 Splitter ID: 609954768
> 		212 창 Splitter ID: 609955104
> 		213 Tab Output ID: 279800176
> 			214 창 ID: 1200
> 				215 창 ID: 1200
> 					216 LIST ID: 1204
> 						217 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							218 단추 위쪽 스크롤 화살표 ID: UpButton
> 							219 단추 페이지 위로 ID: UpPageButton
> 							220 위치 조정 위치 ID: ScrollbarThumb
> 							221 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						222 목록 항목 (selectable)
> 						223 목록 항목 (selectable)
> 						224 목록 항목 (selectable)
> 						225 목록 항목 (selectable)
> 						226 목록 항목 (selectable)
> 						227 목록 항목 (selectable)
> 						228 목록 항목 (selectable)
> 						229 목록 항목 (selectable)
> 						230 목록 항목 (selectable)
> 						231 목록 항목 (selectable)
> 						232 목록 항목 (selectable)
> 						233 목록 항목 (selectable)
> 						234 목록 항목 (selectable)
> 						235 목록 항목 (selectable)
> 						236 목록 항목 (selectable)
> 						237 목록 항목 (selectable)
> 					238 스크롤 막대 ID: 59904
> 						239 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						240 위치 조정 위치 ID: ScrollbarThumb
> 						241 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			242 탭 항목 (selectable) Output
> 			243 단추 Close
> 		244 창 Splitter ID: 617298272
> 		245 Tab Class View ID: 279804736
> 			246 트리 ID: 103
> 				247 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					248 단추 위쪽 스크롤 화살표 ID: UpButton
> 					249 단추 페이지 위로 ID: UpPageButton
> 					250 위치 조정 위치 ID: ScrollbarThumb
> 					251 단추 페이지 아래로 ID: DownPageButton
> 					252 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				253 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					254 콘솔 트리 (selectable) External
> 					255 콘솔 트리 (selectable) Sigmatek
> 					256 콘솔 트리 (selectable) Elmo_1
> 					257 콘솔 트리 (selectable) Elmo_2
> 					258 콘솔 트리 (selectable) Elmo_3
> 					259 콘솔 트리 (selectable) Elmo_4
> 					260 콘솔 트리 (selectable) GL_9086_1
> 					261 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					262 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					263 콘솔 트리 (selectable) LMCControlCommandService
> 						264 콘솔 트리 (selectable) Servers
> 						265 콘솔 트리 (selectable) Clients
> 							266 콘솔 트리 (selectable) LMCAxis1
> 							267 콘솔 트리 (selectable) LMCAxis2
> 							268 콘솔 트리 (selectable) LMCAxis3
> 							269 콘솔 트리 (selectable) LMCAxis4
> 							270 콘솔 트리 (selectable) LMCAxis5
> 							271 콘솔 트리 (selectable) LMCAxis6
> 							272 콘솔 트리 (selectable) LMCAxis7
> 							273 콘솔 트리 (selectable) LMCAxis8
> 							274 콘솔 트리 (selectable) LMCAxis9
> 							275 콘솔 트리 (selectable) LMCRobot
> 						276 콘솔 트리 (selectable) Methods
> 							277 콘솔 트리 (selectable) Global
> 								278 콘솔 트리 (selectable) HandleRequest
> 								279 콘솔 트리 (selectable) ProcessAxisReference
> 							280 콘솔 트리 (selectable) Private
> 						281 콘솔 트리 (selectable) Variables
> 							282 콘솔 트리 (selectable) GroupMovePos
> 							283 콘솔 트리 (selectable) GroupKinematicReady
> 							284 콘솔 트리 (selectable) ReferenceState
> 								285 콘솔 트리 (selectable) 0..18
> 						286 콘솔 트리 (selectable) Objects
> 						287 콘솔 트리 (selectable) Dependencies
> 					288 콘솔 트리 (selectable) LMCDiagnosticsService
> 					289 콘솔 트리 (selectable) LMCEcatInputLatch
> 						290 콘솔 트리 (selectable) Servers
> 						291 콘솔 트리 (selectable) Clients
> 							292 콘솔 트리 (selectable) EcatMaster
> 							293 콘솔 트리 (selectable) Drive1
> 							294 콘솔 트리 (selectable) Drive2
> 							295 콘솔 트리 (selectable) Drive3
> 							296 콘솔 트리 (selectable) Drive4
> 							297 콘솔 트리 (selectable) RecorderStore
> 							298 콘솔 트리 (selectable) Coupler
> 							299 콘솔 트리 (selectable) InputSlot
> 							300 콘솔 트리 (selectable) OutputSlot
> 							301 콘솔 트리 (selectable) LMCAxis1
> 							302 콘솔 트리 (selectable) LMCAxis2
> 							303 콘솔 트리 (selectable) LMCAxis3
> 							304 콘솔 트리 (selectable) LMCAxis4
> 						305 콘솔 트리 (selectable) Methods
> 						306 콘솔 트리 (selectable) Variables
> 							307 콘솔 트리 (selectable) PublishSequence
> 							308 콘솔 트리 (selectable) SnapshotBytes
> 							309 콘솔 트리 (selectable) OutputRevision
> 							310 콘솔 트리 (selectable) OutputObserved
> 							311 콘솔 트리 (selectable) OutputPreviousValid
> 							312 콘솔 트리 (selectable) OutputPreviousValue
> 							313 콘솔 트리 (selectable) Ds402HomeRequestSequence
> 							314 콘솔 트리 (selectable) Ds402HomeAppliedSequence
> 							315 콘솔 트리 (selectable) Ds402HomeMailbox
> 							316 콘솔 트리 (selectable) Ds402HomeAlignmentState
> 								317 콘솔 트리 (selectable) 0..7
> 						318 콘솔 트리 (selectable) Objects
> 						319 콘솔 트리 (selectable) Dependencies
> 					320 콘솔 트리 (selectable) LMCRecorderStore
> 					321 콘솔 트리 (selectable) LMCSdoExecutor
> 					322 콘솔 트리 (selectable) TCPIPServer
> 					323 콘솔 트리 (selectable) TCPMotionInterface
> 						324 콘솔 트리 (selectable) Base
> 						325 콘솔 트리 (selectable) Servers
> 						326 콘솔 트리 (selectable) Clients
> 							327 콘솔 트리 (selectable) _StdLib
> 							328 콘솔 트리 (selectable) Diagnostics
> 							329 콘솔 트리 (selectable) ControlCommands
> 						330 콘솔 트리 (selectable) Methods
> 						331 콘솔 트리 (selectable) Variables
> 						332 콘솔 트리 (selectable) Types
> 						333 콘솔 트리 (selectable) Network
> 						334 콘솔 트리 (selectable) Objects
> 						335 콘솔 트리 (selectable) Dependencies
> 			336 탭 항목 (selectable) Lib
> 			337 탭 항목 (selectable) File
> 			338 탭 항목 (selectable) Class
> 			339 탭 항목 (selectable) Global
> 			340 탭 항목 (selectable) Net
> 			341 단추 Close
> 		342 Tab Properties ID: 279798808
> 			343 창 ID: 270081768
> 				344 TABLE Properties Window ID: 277338752
> 					345 custom Name
> 					346 custom Class
> 					347 custom DataType
> 					348 custom Type
> 					349 custom Required
> 					350 custom DefaultInitValue
> 					351 custom Disable RedefinitionVariableWarning
> 					352 custom Comment
> 				353 도구 모음 ID: 59392
> 					354 단추
> 					355 단추
> 			356 탭 항목 (selectable) Properties
> 			357 단추 Close
>
> The focused UI element is 4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; END_VAR // The legacy recorder prefix remains 304 bytes. The coherent topology and // I/O reader uses the extended 464-byte scalar snapshot. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStat ID: 10000.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg

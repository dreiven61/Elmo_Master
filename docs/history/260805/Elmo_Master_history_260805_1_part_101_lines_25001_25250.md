> 			26021 탭 항목 (selectable) Output
> 			80 단추 Close
> 		81 창 Splitter ID: 384981960
> 		82 Tab Class View ID: 274930128
> 			83 트리 ID: 103
> 				9220 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					9221 단추 위쪽 스크롤 화살표 ID: UpButton
> 					21459 단추 페이지 위로 ID: UpPageButton
> 					9222 위치 조정 위치 ID: ScrollbarThumb
> 					24771 단추 페이지 아래로 ID: DownPageButton
> 					9224 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				26022 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 					26023 단추 왼쪽 스크롤 화살표 ID: UpButton
> 					26024 위치 조정 위치 ID: ScrollbarThumb
> 					26025 단추 페이지 오른쪽으로 ID: DownPageButton
> 					26026 단추 오른쪽 스크롤 화살표 ID: DownButton
> 				26027 위치 조정 (disabled)
> 				9225 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					9226 콘솔 트리 (selectable) External
> 					9227 콘솔 트리 (selectable) Sigmatek
> 					9228 콘솔 트리 (selectable) Elmo_1
> 					9229 콘솔 트리 (selectable) Elmo_2
> 					9230 콘솔 트리 (selectable) Elmo_3
> 					9231 콘솔 트리 (selectable) Elmo_4
> 					9232 콘솔 트리 (selectable) GL_9086_1
> 					9233 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					9234 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					9235 콘솔 트리 (selectable) LMCControlCommandService
> 						24772 콘솔 트리 (selectable) Servers
> 						24773 콘솔 트리 (selectable) Clients
> 						24774 콘솔 트리 (selectable) Methods
> 							25330 콘솔 트리 (selectable) Global
> 							25331 콘솔 트리 (selectable) Private
> 								26028 콘솔 트리 (selectable) HandleRegistryCommands
> 								26029 콘솔 트리 (selectable) HandleAxisCommands
> 								26030 콘솔 트리 (selectable) HandleGroupCommands
> 								26031 콘솔 트리 (selectable) HandleAdminCommands
> 								26032 콘솔 트리 (selectable) MoveLinearAbsEx
> 								26033 콘솔 트리 (selectable) GroupReadStatus
> 								26034 콘솔 트리 (selectable) HandleAxisZeroHomeCommands
> 						24775 콘솔 트리 (selectable) Variables
> 						24776 콘솔 트리 (selectable) Objects
> 						24777 콘솔 트리 (selectable) Dependencies
> 					9236 콘솔 트리 (selectable) LMCDiagnosticsService
> 					9237 콘솔 트리 (selectable) LMCEcatInputLatch
> 					9238 콘솔 트리 (selectable) LMCRecorderStore
> 					9239 콘솔 트리 (selectable) LMCSdoExecutor
> 					9240 콘솔 트리 (selectable) TCPIPServer
> 					9241 콘솔 트리 (selectable) TCPMotionInterface
> 			26035 탭 항목 (selectable) Lib
> 			26036 탭 항목 (selectable) File
> 			26037 탭 항목 (selectable) Class
> 			26038 탭 항목 (selectable) Global
> 			88 단추 Close
> 		89 Tab Properties ID: 274930584
> 			90 창 ID: 266310816
> 				91 TABLE Properties Window ID: 272695160
> 					20828 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						20829 단추 위쪽 스크롤 화살표 ID: UpButton
> 						20830 위치 조정 위치 ID: ScrollbarThumb
> 						20831 단추 페이지 아래로 ID: DownPageButton
> 						20832 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					26039 custom Name
> 					26040 custom Revision
> 					26041 custom GUID
> 					26042 custom Task Settings
> 					26043 custom Sigmatek
> 					26044 custom OSInterface
> 					26045 custom IconPath
> 					26046 custom SharedCommandTable
> 					26047 custom Objectsize
> 					26048 custom Singleton
> 					26049 custom Hidden
> 					26050 custom Deprecated
> 					26051 custom GCCOptions
> 					26052 custom Comment
> 					26053 custom Filename
> 				92 도구 모음 ID: 59392
> 					93 단추
> 					94 단추
> 			26054 탭 항목 (selectable) Properties
> 			96 단추 Close
>
> The focused UI element is 83 트리 ID: 103.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Zero-home helper 표시",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.scroll({window:observation.window,x:850,y:220,scrollX:0,scrollY:360});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:false});globalThis.targetWindow=state.window;nodeRepl.write('Private methods scrolled')}"
> }
> ```
>
> ```text
> Private methods scrolled
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Zero-home helper 위치 조정",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.scroll({window:observation.window,x:850,y:200,scrollX:0,scrollY:-180});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:false});globalThis.targetWindow=state.window;nodeRepl.write('Private methods adjusted')}"
> }
> ```
>
> ```text
> Private methods adjusted
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Zero-home helper 컨텍스트 메뉴",
>   "code": "{const observation=globalThis.state;const screenshotId=observation?.screenshots?.[0]?.id;if(screenshotId==null)throw new Error('No screenshot id');globalThis.state=null;await sky.click({window:observation.window,screenshotId,x:855,y:158,mouse_button:'right'});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''))}"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			9247 창 Comm_Network Secondary Actions: Raise ID: 65281
> 				9248 창 ID: 59648
> 					9249 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						9250 단추 위쪽 스크롤 화살표 ID: UpButton
> 						9251 위치 조정 위치 ID: ScrollbarThumb
> 						9252 단추 페이지 아래로 ID: DownPageButton
> 						9253 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					9254 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						9255 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						9256 위치 조정 위치 ID: ScrollbarThumb
> 						9257 단추 페이지 오른쪽으로 ID: DownPageButton
> 						9258 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					9259 위치 조정 (disabled)
> 			9260 창 LMCControlCommandService.st Secondary Actions: Raise ID: 65283
> 				9261 창 ID: 59648
> 					9262 창 //This file was generated by the LASAL2 CodeGenerator -- //Please, do not edit this file (it might be overwritten by the next generator run) //{{LSL_DECLARATION (*! <Class Name = "LMCControlCommandService" Revision = "0.0" GUID = "{206B37D3-F5DB-4150-9018-CC5821293EA6}" RealtimeTask = "false" CyclicTask = "false" BackgroundTask = "false" Sigmatek = "false" OSInterface = "false" HighPriority = "false" Automatic = "false" UpdateMode = "Prescan" SharedCommandTable = "true" Objectsize = "(542,120)"> <Channels> <Server Name="ClassSvr" GUID="{42369435-585E-426D-AC67-541F5A8B9646}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Client Name="InputLatch" Required="true" Internal="false"/> <Client Name="LMCAxis1" Required="true" Internal="false"/> <Client Name="LMCAxis2" Required="true" Internal="false"/> <Client Name="LMCAxis3" Required="true" Internal="false"/> <Client Name="LMCAxis4" Required="true" Internal="false"/> <Client Name="LMCAxis5" Required="true" Internal="false"/> <Client Name="LMCAxis6" Required="true" Internal="false"/> <Client Name="LMCAxis7" Required="true" Internal="false"/> <Client Name="LMCAxis8" Required="true" Internal="false"/> <Client Name="LMCAxis9" Required="true" Internal="false"/> <Client Name="LMCRobot" Required="true" Internal="false"/> </Channels> </Class> *) LMCControlCommandService : CLASS //Servers: ClassSvr : SvrChCmd_DINT; //Clients: InputLatch : CltChCmd_LMCEcatInputLatch; LMCAxis1 : CltChCmd__LMCAxis; LMCAxis2 : CltChCmd__LMCAxis; LMCAxis3 : CltChCmd__LMCAxis; LMCAxis4 : CltChCmd__LMCAxis; LMCAxis5 : CltChCmd__LMCAxis; LMCAxis6 : CltChCmd__LMCAxis; LMCAxis7 : CltChCmd__LMCAxis; LMCAxis8 : CltChCmd__LMCAxis; LMCAxis9 : CltChCmd__LMCAxis; LMCRobot : CltChCmd__LMCRobotBase; //Variables: GroupMovePos : _LMCPROF_POS; GroupKinematicReady : BOOL; ZeroHomeState : ARRAY [0..63] OF DINT; OwnershipState : ARRAY [0..351] OF DINT; //Functions: FUNCTION GLOBAL HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; AdmissionToken : UDINT; OwnerGeneration : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR; FUNCTION GLOBAL ProcessAxisZeroHome; FUNCTION GLOBAL ReserveAxisOwnership VAR_INPUT CommandId : UINT; Reference : UINT; RequestedAxisMask : UDINT; OwnerKind : UINT; ResourceKind : UINT; AdmissionMode : UINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; pIdentity : ^UDINT; IdentityCount : UINT; pEffectiveAxisMask : ^UDINT; pAdmissionToken : ^UDINT; pOwnerGeneration : ^UDINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR; FUNCTION GLOBAL ValidateAxisOwnership VAR_INPUT CommandId : UINT; Reference : UINT; ExpectedAxisMask : UDINT; OwnerKind : UINT; ResourceKind : UINT; AdmissionMode : UINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; AdmissionToken : UDINT; OwnerGeneration : UDINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR; FUNCTION GLOBAL CommitAxisOwnership VAR_INPUT CommandId : UINT; Reference : UINT; ExpectedAxisMask : UDINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; AdmissionToken : UDINT; OwnerGeneration : UDINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR; FUNCTION GLOBAL RollbackAxisOwnership VAR_INPUT AdmissionToken : UDINT; OwnerGeneration : UDINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; Reason : DINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR; FUNCTION GLOBAL PublishAxisOwnership VAR_INPUT AxisMask : UDINT; AdmissionToken : UDINT; OwnerGeneration : UDINT; ReportKind : UINT; ReportValue0 : UDINT; Repor ID: 10000
> 						9263 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							9264 단추 위쪽 스크롤 화살표 ID: UpButton
> 							9265 위치 조정 위치 ID: ScrollbarThumb
> 							9266 단추 페이지 아래로 ID: DownPageButton
> 							9267 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						9268 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							9269 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							9270 위치 조정 위치 ID: ScrollbarThumb
> 							9271 단추 페이지 오른쪽으로 ID: DownPageButton
> 							9272 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						9273 위치 조정 (disabled)
> 			9274 창 Motion_Network Secondary Actions: Raise ID: 65282
> 				9275 창 ID: 59648
> 					9276 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						9277 단추 위쪽 스크롤 화살표 ID: UpButton
> 						9278 위치 조정 위치 ID: ScrollbarThumb
> 						9279 단추 페이지 아래로 ID: DownPageButton
> 						9280 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					9281 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						9282 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						9283 위치 조정 위치 ID: ScrollbarThumb
> 						9284 단추 페이지 오른쪽으로 ID: DownPageButton
> 						9285 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					9286 위치 조정 (disabled)
> 			9287 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65280
> 				9288 창 ID: 59648
> 					9289 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000
> 						9290 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							9291 단추 위쪽 스크롤 화살표 ID: UpButton
> 							9292 단추 페이지 위로 ID: UpPageButton
> 							9293 위치 조정 위치 ID: ScrollbarThumb
> 							9294 단추 페이지 아래로 ID: DownPageButton
> 							9295 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						9296 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							9297 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							9298 위치 조정 위치 ID: ScrollbarThumb
> 							9299 단추 페이지 오른쪽으로 ID: DownPageButton
> 							9300 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						9301 위치 조정 (disabled)
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트
> 			5 텍스트
> 			6 텍스트
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				28194 단추 (disabled) Toggle bookmark
> 				28195 단추 (disabled) Previous bookmark
> 				28196 단추 (disabled) Next bookmark
> 				28197 단추 (disabled) Delete all bookmarks
> 				28198 단추 (disabled) Previous bookmark in this file
> 				28199 단추 (disabled) Next bookmark in this file
> 				28200 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				28201 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				28202 단추 (disabled) Unindent (Shift+Tab)
> 				28203 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				28204 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				28205 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				28206 단추 DataAnalyzer
> 				28207 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				28208 단추 Select
> 				28209 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				28210 단추 Go online (Alt+F6)
> 				28211 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				28212 메뉴 항목 Target Architecture
> 				28213 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				28214 단추 New project (Strg+N)
> 				28215 단추 Open a file (Strg+Shift+O)
> 				28216 단추 Close active document (Strg+F4)
> 				28217 단추 (disabled) Save file (Strg+S)
> 				28218 단추 Open project (Strg+O)
> 				28219 단추 (disabled) Save project changes (Strg+Shift+S)
> 				28220 단추 Close project
> 				28221 단추 Print
> 				28222 단추 Cut (Strg+X)
> 				28223 단추 Copy (Strg+C)
> 				28224 단추 Paste (Strg+V)
> 				28225 메뉴 항목 (disabled) Undo (Strg+Z)
> 				28226 메뉴 항목 (disabled) Redo (Strg+Y)

> 			5 메뉴 항목 Edit Defines
> 			6 메뉴 항목 Delete Del
> 			7 메뉴 항목 Duplicate Class
> 			8 메뉴 항목 Derive Class
> 			9 메뉴 항목 Find Reference
> 			10 메뉴 항목 Print Class
> 			11 메뉴 항목 Export Class(es)
> 			12 메뉴 항목 Reload Class
> 			13 메뉴 항목 Re\Import MatLab Model
> 			14 메뉴 항목 Protection Setting
> 			15 메뉴 항목 New Server
> 			16 메뉴 항목 New Client
> 			17 메뉴 항목 New Method
> 			18 메뉴 항목 New Std. Methods
> 			19 메뉴 항목 New Variable
> 			20 메뉴 항목 New Type
> 			21 메뉴 항목 New Table
> 			22 메뉴 항목 Add Network
> 			23 메뉴 항목 Add File...
> 			24 메뉴 항목 Open Subfolders
> 			25 메뉴 항목 Paste
> 			26 메뉴 항목 Copy
> 		27 창 작업 영역 ID: 59648
> 			28 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				29 창 ID: 59648
> 					30 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if CurrentSock = dSock then CurrentSock := 0; SessionEpoch += 1; if SessionEpoch = 0 ID: 10000
> 						31 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							32 단추 위쪽 스크롤 화살표 ID: UpButton
> 							33 단추 페이지 위로 ID: UpPageButton
> 							34 위치 조정 위치 ID: ScrollbarThumb
> 							35 단추 페이지 아래로 ID: DownPageButton
> 							36 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						37 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							38 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							39 위치 조정 위치 ID: ScrollbarThumb
> 							40 단추 페이지 오른쪽으로 ID: DownPageButton
> 							41 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						42 위치 조정 (disabled)
> 			43 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				44 창 ID: 59648
> 					45 창 xtpBarTop ID: 59419
> 						46 도구 모음 Hardware Editor
> 							47 단추 Hardware Editor Configuration Settings
> 							48 단추 Calculate Resources of Project
> 							49 단추 (disabled) Upload Hardware Tree from PLC
> 							50 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							51 단추 Show Detail Mode\Show Configuration Mode
> 							52 단추 Generates the ENI File of the current project
> 					53 창 ID: 59648
> 						54 트리 ID: 1
> 							55 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								56 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]->
> 								57 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								58 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								59 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								60 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								61 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								62 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								63 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								64 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								65 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								66 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								67 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								68 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								69 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								70 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								71 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								72 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								73 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								74 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								75 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								76 콘솔 트리 (selectable) ALARM:00, Empty
> 								77 콘솔 트리 (selectable) SDIAS:00, Empty
> 								78 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								79 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							80 콘솔 트리 (selectable) Unplaced Module(s)
> 			81 창 Elmo_4 Secondary Actions: Raise ID: 65285
> 				82 창 ID: 59648
> 					83 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						84 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							85 단추 위쪽 스크롤 화살표 ID: UpButton
> 							86 위치 조정 위치 ID: ScrollbarThumb
> 							87 단추 페이지 아래로 ID: DownPageButton
> 							88 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						89 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							90 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							91 위치 조정 위치 ID: ScrollbarThumb
> 							92 단추 페이지 오른쪽으로 ID: DownPageButton
> 							93 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						94 위치 조정 (disabled)
> 			95 창 Motion_Network Secondary Actions: Raise ID: 65284
> 				96 창 ID: 59648
> 					97 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						98 단추 위쪽 스크롤 화살표 ID: UpButton
> 						99 위치 조정 위치 ID: ScrollbarThumb
> 						100 단추 페이지 아래로 ID: DownPageButton
> 						101 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					102 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						103 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						104 위치 조정 위치 ID: ScrollbarThumb
> 						105 단추 페이지 오른쪽으로 ID: DownPageButton
> 						106 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					107 위치 조정 (disabled)
> 			108 창 Comm_Network Secondary Actions: Raise ID: 65283
> 				109 창 ID: 59648
> 					110 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						111 단추 위쪽 스크롤 화살표 ID: UpButton
> 						112 위치 조정 위치 ID: ScrollbarThumb
> 						113 단추 페이지 아래로 ID: DownPageButton
> 						114 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			115 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				116 창 ID: 59648
> 					117 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						118 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							119 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							120 위치 조정 위치 ID: ScrollbarThumb
> 							121 단추 페이지 오른쪽으로 ID: DownPageButton
> 							122 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			123 창 HW_Network Secondary Actions: Raise ID: 65281
> 				124 창 ID: 59648
> 					125 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						126 단추 위쪽 스크롤 화살표 ID: UpButton
> 						127 위치 조정 위치 ID: ScrollbarThumb
> 						128 단추 페이지 아래로 ID: DownPageButton
> 						129 단추 아래쪽 스크롤 화살표 ID: DownButton
> 		130 상태 표시줄 ID: 59393
> 			131 텍스트
> 			132 텍스트
> 			133 텍스트
> 			134 텍스트
> 			135 텍스트
> 			136 텍스트 Offline
> 			137 텍스트
> 			138 텍스트 NUM
> 			139 텍스트
> 		140 창 xtpBarTop ID: 59419
> 			141 도구 모음 Script
> 			142 도구 모음 Edit
> 				143 단추 Toggle bookmark
> 				144 단추 (disabled) Previous bookmark
> 				145 단추 (disabled) Next bookmark
> 				146 단추 (disabled) Delete all bookmarks
> 				147 단추 (disabled) Previous bookmark in this file
> 				148 단추 (disabled) Next bookmark in this file
> 				149 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				150 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				151 단추 (disabled) Unindent (Shift+Tab)
> 				152 단추 (disabled) Indent (Tab)
> 			153 도구 모음 Macros Manager
> 				154 메뉴 항목 Macros
> 			155 도구 모음 Layout Manager
> 				156 메뉴 항목 Layouts
> 			157 도구 모음 Toolbox
> 				158 단추 DataAnalyzer
> 				159 단추 Interpreter
> 				160 단추 DiasDrive
> 				161 단추 PLC Diagnosis
> 				162 단추 Hardware Editor
> 				163 단추 Graphical Hardware Editor
> 				164 단추 Connection Manager
> 				165 단추 Task Configuration
> 			166 도구 모음 Net Edit
> 				167 단추 (disabled) Select
> 				168 단추 (disabled) Move view
> 				169 단추 (disabled) Insert comment
> 				170 단추 (disabled) Zoom(+/-)
> 				171 단추 (disabled) Zoom to all
> 				172 단추 (disabled) Zoom selection
> 			173 도구 모음 Debug
> 				174 단추 Go online (Alt+F6)
> 				175 단추 Change Online Settings
> 				176 메뉴 항목 Online Connection
> 				177 단추 (disabled) Set Online Connection For Current Project
> 				178 단추 (disabled) Download (F6)
> 				179 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				180 단추 (disabled) Download Module on the Fly
> 				181 단추 (disabled) Save Project on PLC
> 				182 단추 (disabled) Start (F7)
> 				183 단추 (disabled) Reset (F8)
> 				184 단추 Toggle breakpoint (F4)
> 				185 단추 Create condition breakpoint
> 				186 단추 Remove all breakpoint(s) (Shift+F4)
> 				187 단추 AWL trigger on/off
> 				188 단추 (disabled) Fix AWL trigger to current instruction
> 				189 단추 Activate/Deactivate Live View
> 				190 단추 Activate/Deactivate Auto Watch
> 				191 단추 (disabled) Goto instruction pointer
> 				192 단추 (disabled) Step into (F5)
> 				193 단추 (disabled) Step over (Alt+F5)
> 				194 단추 (disabled) Step out (Shift+F5)
> 				195 단추 (disabled) Set instruction pointer
> 			196 도구 모음 Build
> 				197 메뉴 항목 Target Architecture
> 				198 단추 Build changes (F9)
> 				199 단추 Rebuild project (Strg+F9)
> 				200 단추 (disabled) Cancel building (Ctrl+Break)
> 				201 단추 Link project
> 			202 도구 모음 Standard
> 				203 단추 New project (Strg+N)
> 				204 단추 Open a file (Strg+Shift+O)
> 				205 단추 Close active document (Strg+F4)
> 				206 단추 (disabled) Save file (Strg+S)
> 				207 단추 Open project (Strg+O)
> 				208 단추 (disabled) Save project changes (Strg+Shift+S)
> 				209 단추 Close project
> 				210 단추 Print
> 				211 단추 Cut (Strg+X)
> 				212 단추 Copy (Strg+C)
> 				213 단추 Paste (Strg+V)
> 				214 메뉴 항목 (disabled) Undo (Strg+Z)
> 				215 메뉴 항목 (disabled) Redo (Strg+Y)
> 				216 단추 Navigate Backward (Alt+Left)
> 				217 단추 (disabled) Navigate Forward (Alt +Right)
> 			218 메뉴 모음 Menu Bar
> 				219 메뉴 항목 FILE
> 				220 메뉴 항목 EDIT
> 				221 메뉴 항목 VIEW
> 				222 메뉴 항목 PROJECT
> 				223 메뉴 항목 BUILD
> 				224 메뉴 항목 DEBUG
> 				225 메뉴 항목 ANALYZE
> 				226 메뉴 항목 TOOLS
> 				227 메뉴 항목 EXTRAS
> 				228 메뉴 항목 WINDOW
> 				229 메뉴 항목 HELP
> 		230 창 Splitter ID: 302431488
> 		231 창 Splitter ID: 302437704
> 		232 Tab Output ID: 297478928
> 			233 창 ID: 1200
> 				234 창 ID: 1200
> 					235 LIST ID: 1201
> 						236 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							237 단추 위쪽 스크롤 화살표 ID: UpButton
> 							238 위치 조정 위치 ID: ScrollbarThumb
> 							239 단추 페이지 아래로 ID: DownPageButton
> 							240 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						241 목록 항목 (selectable)
> 						242 목록 항목 (selectable)
> 						243 목록 항목 (selectable)
> 						244 목록 항목 (selectable)
> 						245 목록 항목 (selectable)
> 						246 목록 항목 (selectable)
> 						247 목록 항목 (selectable)
> 						248 목록 항목 (selectable)
> 						249 목록 항목 (selectable)
> 						250 목록 항목 (selectable)
> 						251 목록 항목 (selectable)
> 						252 목록 항목 (selectable)
> 						253 목록 항목 (selectable)
> 					254 스크롤 막대 ID: 59904

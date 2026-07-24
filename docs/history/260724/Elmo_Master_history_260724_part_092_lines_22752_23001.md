>     '\t\t\t\t\t\t32 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t33 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t34 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t35 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t36 위치 조정 (disabled)\n' +
>     '\t\t\t37 창 Elmo_4 Secondary Actions: Raise ID: 65286\n' +
>     '\t\t\t\t38 창 ID: 59648\n' +
>     '\t\t\t\t\t39 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000\n' +
>     '\t\t\t\t\t\t40 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t\t41 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t42 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t43 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t44 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t45 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t\t46 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t47 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t48 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t49 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t50 위치 조정 (disabled)\n' +
>     '\t\t\t51 창 EtherCAT_Network Secondary Actions: Raise ID: 65284\n' +
>     '\t\t\t\t52 창 ID: 59648\n' +
>     '\t\t\t\t\t53 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t54 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t55 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t56 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t57 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t58 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283\n' +
>     '\t\t\t\t59 창 ID: 59648\n' +
>     '\t\t\t\t\t60 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHA'... 10432 more characters,
>   focused_element: '1 대화 상자 Lasal Class 2 Secondary Actions: Raise'
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "미사용 라이브러리 유지",
>   "code": "await sky.click({window:stAfterClose.window,element_index:3}); await new Promise(resolve=>setTimeout(resolve,1200)); nodeRepl.write((await sky.list_windows()).filter(w=>/Lasal Class 2/.test(w.title||'')))"
> }
> ```
>
> ```text
> stAfterClose is not defined
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "종료 확인창 상태 복구",
>   "code": "var currentWins=await sky.list_windows(); var lasalWinNow=currentWins.find(w=>/Lasal Class 2/.test(w.title||'')); var dialogStateNow=await sky.get_window_state({window:lasalWinNow,include_screenshot:false,include_text:true}); nodeRepl.write(dialogStateNow.accessibility?.tree?.split('\\n').slice(0,12).join('\\n'))"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 대화 상자 Lasal Class 2 Secondary Actions: Raise
> 			2 단추 예(Y) ID: 6
> 			3 단추 아니요(N) ID: 7
> 			4 이미지 ID: 20
> 			5 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535
> 			6 제목 표시줄
> 				7 단추 (disabled) 닫기
> 		8 창 작업 영역 ID: 59648
> 			9 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				10 창 ID: 59648
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "미사용 라이브러리 유지",
>   "code": "await sky.click({window:dialogStateNow.window,element_index:3}); await new Promise(resolve=>setTimeout(resolve,1500)); nodeRepl.write((await sky.list_windows()).filter(w=>/Lasal Class 2/.test(w.title||'')))"
> }
> ```
>
> ```text
> point (-1063, 472) is over Lasal2.exe "", not target window Lasal2.exe "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface"; activate the target or take a fresh screenshot before retrying
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 확인창 활성화",
>   "code": "await sky.activate_window({window:lasalWinNow}); await new Promise(resolve=>setTimeout(resolve,250)); dialogStateNow=await sky.get_window_state({window:lasalWinNow,include_screenshot:true,include_text:true}); nodeRepl.write({screens:dialogStateNow.screenshots.length,focused:dialogStateNow.accessibility?.focused_element})"
> }
> ```
>
> ```text
> {
>   screens: 2,
>   focused: '1 대화 상자 Lasal Class 2 Secondary Actions: Raise'
> }
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
>   "title": "미사용 라이브러리 유지",
>   "code": "await sky.click({window:dialogStateNow.window,element_index:3}); await new Promise(resolve=>setTimeout(resolve,1500)); nodeRepl.write((await sky.list_windows()).filter(w=>/Lasal Class 2/.test(w.title||'')))"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 6490100,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface'
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 완전 종료 확인",
>   "code": "await new Promise(resolve=>setTimeout(resolve,1800)); var remainLasal=(await sky.list_windows()).filter(w=>/Lasal Class 2/.test(w.title||'')); nodeRepl.write(remainLasal); if(remainLasal[0]){var remainState=await sky.get_window_state({window:remainLasal[0],include_screenshot:true,include_text:true}); nodeRepl.write(remainState.accessibility?.tree?.split('\\n').slice(0,20).join('\\n')); await nodeRepl.emitImage(remainState.screenshots[0].url)}"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 6490100,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface'
>   }
> ]Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 대화 상자 Lasal Class 2 Secondary Actions: Raise
> 			2 단추 예(Y) ID: 6
> 			3 단추 아니요(N) ID: 7
> 			4 이미지 ID: 20
> 			5 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535
> 			6 제목 표시줄
> 				7 단추 (disabled) 닫기
> 		8 창 작업 영역 ID: 59648
> 			9 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				10 창 ID: 59648
> 					11 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
> 						12 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							13 단추 위쪽 스크롤 화살표 ID: UpButton
> 							14 단추 페이지 위로 ID: UpPageButton
> 							15 위치 조정 위치 ID: ScrollbarThumb
> 							16 단추 페이지 아래로 ID: DownPageButton
> 							17 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						18 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> ```
>
> Image output: image/jpeg
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
>   "title": "아니요 선택",
>   "code": "await sky.press_key({window:lasalWinNow,key:'Tab'}); await new Promise(resolve=>setTimeout(resolve,250)); dialogStateNow=await sky.get_window_state({window:lasalWinNow,include_screenshot:true,include_text:true}); nodeRepl.write(dialogStateNow.accessibility?.focused_element); await nodeRepl.emitImage(dialogStateNow.screenshots[0].url)"
> }
> ```
>
> ```text
> 1 대화 상자 Lasal Class 2 Secondary Actions: Raise
> ```
>
> Image output: image/jpeg
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
>   "title": "LASAL 종료 완료",
>   "code": "await sky.press_key({window:lasalWinNow,key:'Return'}); await new Promise(resolve=>setTimeout(resolve,2000)); nodeRepl.write((await sky.list_windows()).filter(w=>/Lasal Class 2/.test(w.title||'')))"
> }
> ```
>
> ```text
> []
> ```
>
> LASAL 선언 단계는 끝났습니다. `HandleGroupCommands`가 Private method로 생성·저장됐고, 미사용 라이브러리 제거 질문에는 “아니요”로 종료했습니다. 이제 LASAL이 완전히 닫힌 상태에서 소스 본문과 검증 스크립트를 편집합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+662 -649)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+34 -11)
>
> </details>
>
> 1단계 소스 분리는 적용됐습니다. `MsgPaser`는 약 67KB에서 44.8KB로 줄었고, 새 `HandleGroupCommands`는 약 23.9KB입니다. 명령 본문·프레임 포맷은 그대로 옮겼으며, 검증 스크립트도 새 소유 위치와 함수 크기 상한을 검사하도록 갱신했습니다. 이제 정적 계약 실패를 먼저 잡겠습니다.
>
> <details><summary>Created a file, edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -0)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+351 -0)
> - Wrote `./AGENTS.md` (+1 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+10 -2)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+5 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+10 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+1 -0)
>
> </details>
>
> 설계 기준 문서와 관련 아키텍처/코딩 규칙 연결까지 반영했습니다. 이제 SourceOnly/full LASAL 계약과 C# Debug/Release 회귀를 병렬로 확인합니다. 여기서 PASS해도 PLC build/download가 증명되는 것은 아니므로 그 경계는 따로 표시하겠습니다.

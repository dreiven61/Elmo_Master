>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.focused_element||\"\"));",
>   "title": "파생 클래스 이름 상태 재확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 281 편집 ID: 1
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
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"Return\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/LMCSdoExecutor|dialog|대화|OK|확인|BaseClass/.test(x)).slice(-160).join(\"\\n\"));}",
>   "title": "LMCSdoExecutor 클래스 생성 확정",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> 파생 클래스 `LMCSdoExecutor : EtherCAT_SDOBase`는 LASAL IDE의 Derive Class로 정상 생성됐고, IDE 속성에서도 `BaseClass=EtherCAT_SDOBase`를 확인했습니다. 동시에 PC 쪽 first-slice 정책(축 1~4, `0x1000:0`, UInt32/4바이트, read-only)과 경계 시험은 구현되어 103/103을 통과했습니다. 이제 IDE가 생성한 클래스 파일을 확인한 뒤 adapter·서비스 state machine을 채웁니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"Control_L+Shift_L+s\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/dialog|대화|Save|저장|LMCSdoExecutor/.test(x)).slice(-160).join(\"\\n\"));}",
>   "title": "LASAL 파생 클래스 저장",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 기존 미커밋 작업은 3개 커밋으로 정리했고, 현재는 LASAL IDE에서 만든 `LMCSdoExecutor` 파생 클래스에 실행 로직과 서비스 상태기를 결합하는 단계입니다. PC 측 1차 SDO 정책 테스트는 103/103까지 통과했으며, 이제 두 LASAL 변경을 합쳐 네트워크 연결·IDE 빌드·정적 계약 검증까지 마무리하겠습니다.
>
> <details><summary>Called 2 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Check LASAL IDE session",
>   "code": "nodeRepl.write({skyType: typeof sky, targetWindowType: typeof targetWindow, stateType: typeof state})"
> }
> ```
>
> ```text
> { skyType: 'object', targetWindowType: 'object', stateType: 'object' }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect LASAL IDE",
>   "code": "nodeRepl.write(state)"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 201694,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - EtherCAT_SDOBase.st [RO]'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,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KGOLbGBsU4yq+gOBwPQVXs9F0uxx9i02yt8OJP3UCJ8wBAbgdcEjPua0KKAKVxpOnXN7FeXNhaTXcWPLnkhVnTByMMRkYNTfY7X7IbX7ND9mIx5OwbMZz93p1qeigCrcWYeKcW0jWc0xDPPAibyRgZ+ZSCcADkHiqY0K1niRdX26w8bFo5L6CFmjzjhdqKB09M1rUUAUW0jTW1Eag2n2Zvx/y8mFfN6Y+9jPTinXul6ffzQy31ja3MsJzE80KuyHr8pI46Dp6VcooAo3ekabeXcV1eafZz3MWPLmlhVnTByMMRkYPNK+lae98b17C0a8IwZzCpkIxj72M9OKu0UAZ0eh6THYyWUel2K2Ujb3t1t0EbNxyVxgngfkKG0PSWtfszaXYm32hPKNumzaCWAxjGASTj1JrRooAqR6bYxzW8sdlbJLbp5ULrEoaJOm1TjgewqGLSIEvL6eRnmS7kSV4JVRo1dFUBl+XOfkU8k8jIxWjRQBXmsbSb7R51rBJ9oUJNujB8xRnAb1HJ4PrVK70Kzl0SfSrNf7OtJlKMLONE4b7wAKkDPPOM1q0UAIo2qAOgGKWiigNgooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAK4O38G6z/AGBpGhX2s6VcaVYNafKNKdZZEt3RgpYzlQW8sAnaep4rvKKAGQxRwRLFDGkcajCogwAPYCn0UUAFUbLSNPsb+9vrOzghu71la4mRAGlIGBk/57+tXqKAKOm6Rp+mTXkun2cFtJeS+dcNGgUyPjG4/wCf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'... 542915 more characters,
>       originX: -1928,
>       originY: -19,
>       width: 1920,
>       height: 1040
>     }
>   ],
>   accessibility: null
> }
> ```
>
> </details>
>
> LASAL 쪽 공개 계약은 32바이트 고정 결과 구조로 확정했습니다. `Token/OS 결과/Abort/길이/Index/SubIndex/검증 코드/Data`의 오프셋을 두 구현에 동일하게 맞추고 있으며, 기능 비트는 실제 PLC 시험 전까지 계속 꺼진 상태(`FALSE`)로 유지합니다. 즉 이번 커밋은 전송 경로와 fail-closed 정책을 넣되, 검증되지 않은 SDO 기능을 사용자에게 활성화하지 않습니다.
>
> PC 라이브러리 검토에서 한 가지 UI 불일치도 확인했습니다. 라이브러리는 첫 단계 범위만 허용하는데 기존 WPF 탭은 임의 Index·길이·Write를 계속 선택할 수 있었습니다. 이 부분도 함께 제한하고, 허용 밖 입력은 예외로 앱을 중단시키지 않고 화면 상태 메시지로 설명하도록 수정 중입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+5 -0)
>
> </details>
>
> LASAL 구현 두 축이 합쳐졌습니다. 이제 IDE 자체 모델에 새 type·method·client를 다시 읽히고, 기존 네 개 SDOBase 객체를 파생 executor로 교체해 서비스의 `SdoAxis1..4`까지 연결하겠습니다. 이 단계에서 컴파일 오류가 나오면 소스 정적 검사와 구분해 실제 LASAL 문법/등록 문제로 바로 수정합니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Refresh LASAL IDE view",
>   "code": "state = await sky.get_window_state({window:targetWindow, include_screenshot:true, include_text:true}); targetWindow = state.window; nodeRepl.write((state.accessibility?.tree||'').split('\\n').slice(0,280).join('\\n'))"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - EtherCAT_SDOBase.st [RO]", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - EtherCAT_SDOBase.st [RO] Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 EtherCAT_SDOBase.st [RO] Secondary Actions: Raise ID: 65288
> 				3 창 ID: 59648
> 					4 창 //This file was generated by the LASAL2 CodeGenerator -- //Please, do not edit this file (it might be overwritten by the next generator run) //{{LSL_DECLARATION #include "..\..\Class\ECAT_Master_Base\ECAT_Master_Base.h" (*! <Class Name = "EtherCAT_SDOBase" Revision = "1.1" GUID = "{47421B46-76BD-4CB4-B2B2-8EEF7265063A}" RealtimeTask = "false" CyclicTask = "false" BackgroundTask = "false" Sigmatek = "true" OSInterface = "false" HighPriority = "false" Automatic = "false" UpdateMode = "Prescan" IconPath = ".\Class\EtherCAT.ico" SharedCommandTable = "true" Objectsize = "(510,120)" Comment = "Class can be used to establish a SDO Communication with an EtherCAT slave module."> <Channels> <Server Name="ClassState" GUID="{17AB54D3-7855-45B7-911A-9576DDCF8542}" Visualized="true" Initialize="false" WriteProtected="true" Retentive="false" Comment="Shows the state of the class."/> <Server Name="CompleteAccess" GUID="{169F4D23-3A87-40A6-9196-71482210E348}" Visualized="true" Initialize="true" DefValue="0" WriteProtected="false" Retentive="false" Comment="Accesstype of the SDO.#13;&#10;0 .. Single Access&#13;&#10;1 .. Complete Access"/> <Server Name="ErrorCode" GUID="{8E57551A-D82B-457E-ABF9-2947396D236F}" Visualized="true" Initialize="false" WriteProtected="true" Retentive="false" Comment="Shows the Errorcode of the SDO handling if ClassState = ERROR.&#13;&#10;&#13;&#10;0x05030000 ... toggle bit not changed&#13;&#10;0x05040000 ... SDO protocol timeout&#13;&#10;0x05040001 ... SDO command specifier invalid or unknown&#13;&#10;0x05040005 ... not enough space in the main memory&#13;&#10;0x06010000 ... access to this object is not supported&#13;&#10;0x06010001 ... attempt to read to a write_only parameter&#13;&#10;0x06010002 ... attempt to write to a read_only parameter&#13;&#10;0x06010003 ... SubIdx 0 can only be written if it is 0&#13;&#10;0x06010004 ... SDO complete access is not supported for ENUMs&#13;&#10;0x06010005 ... object length is larger than mailbox size&#13;&#10;0x06010006 ... object mapped to RxPDO, SDO download blocked&#13;&#10;0x06020000 ... the object is not found in the object directory&#13;&#10;0x06040041 ... object can&apos;t be mapped in the PDO&#13;&#10;0x06040042 ... the amount and length of objects exceeds the PDO length&#13;&#10;0x06040043 ... general parameter incompatibility reason&#13;&#10;0x06040047 ... general internal incompatibility in the device&#13;&#10;0x06060000 ... access failed due to a hardware error&#13;&#10;0x06070000 ... data type does not match, length of service parameter does not match&#13;&#10;0x06070012 ... data type does not match, length of service parameter too high&#13;&#10;0x06070013 ... data type does not match, length of service parameter too low&#13;&#10;0x06090011 ... sub index does not exist&#13;&#10;0x06090030 ... value of range of parameter exceeded (only for write access)&#13;&#10;0x06090031 ... value of range of parameter too high&#13;&#10;0x06090032 ... value of range of parameter to low&#13;&#10;0x06090036 ... maximum value is less than minimum value&#13;&#10;0x08000000 ... general error&#13;&#10;0x08000020 ... data can&apos;t be stored to the application&#13;&#10;0x08000021 ... data can&apos;t be stored to the application because of local control&#13;&#10;0x08000022 ... data can&apos;t be stored to the application because of present device state&#13;&#10;0x08000023 ... object dictionary dynamic generation failed or no object dict. is present"/> <Server Name="ParaIndex" GUID="{4979D147-33CC-40E4-BF4D-103FDF7CDD51}" Visualized="true" Initialize="true" WriteProtected="false" Retentive="false" Comment="Index from the SDO Entry"/> <Server Name="ParaLength" GUID="{81E23868-A40C-4C38-9A78-DAD5CBBBDB44}" Visualized="true" Initialize="true" WriteProtected="false" Retentive="false" Comment="ParaReadWrite = 0&#13;&#10;Shows the amount of data bytes which were read.&#13;&#10;&#13;&#10;ParaReadWrite = 1&#13;& ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 위치 조정 위치 ID: ScrollbarThumb
> 							8 단추 페이지 아래로 ID: DownPageButton
> 							9 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							11 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							12 위치 조정 위치 ID: ScrollbarThumb
> 							13 단추 페이지 오른쪽으로 ID: DownPageButton
> 							14 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						15 위치 조정 (disabled)
> 			16 창 Elmo_3 Secondary Actions: Raise ID: 65292
> 				17 창 ID: 59648
> 					18 창 FUNCTION VIRTUAL GLOBAL Elmo_3::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_3::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_3_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_3_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_3::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_3_SETPOS_INDEX, ELMO_3_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_3_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_3_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_3_CONTROLWORD_INDEX, ELMO_3_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_3_ACTPOS_INDEX, ELMO_3_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_3_INPUTS_DIGITALINPUTS_INDEX, ELMO_3_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_3_STATEWORD_INDEX, ELMO_3_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						19 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							20 단추 위쪽 스크롤 화살표 ID: UpButton
> 							21 위치 조정 위치 ID: ScrollbarThumb
> 							22 단추 페이지 아래로 ID: DownPageButton
> 							23 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						24 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							25 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							26 위치 조정 위치 ID: ScrollbarThumb
> 							27 단추 페이지 오른쪽으로 ID: DownPageButton
> 							28 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						29 위치 조정 (disabled)
> 			30 창 EtherCAT_Network Secondary Actions: Raise ID: 65291
> 				31 창 ID: 59648
> 					32 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						33 단추 위쪽 스크롤 화살표 ID: UpButton
> 						34 위치 조정 위치 ID: ScrollbarThumb
> 						35 단추 페이지 아래로 ID: DownPageButton
> 						36 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			37 창 HW_Network Secondary Actions: Raise ID: 65290
> 				38 창 ID: 59648
> 			39 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65289
> 				40 창 ID: 59648
> 					41 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						42 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							43 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							44 위치 조정 위치 ID: ScrollbarThumb
> 							45 단추 페이지 오른쪽으로 ID: DownPageButton
> 							46 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			47 창 TCPMotionInterface Secondary Actions: Raise ID: 65287
> 				48 창 ID: 59648
> 					49 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 						50 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							51 단추 위쪽 스크롤 화살표 ID: UpButton
> 							52 위치 조정 위치 ID: ScrollbarThumb
> 							53 단추 페이지 아래로 ID: DownPageButton
> 							54 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						55 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							56 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							57 위치 조정 위치 ID: ScrollbarThumb
> 							58 단추 페이지 오른쪽으로 ID: DownPageButton
> 							59 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						60 위치 조정 (disabled)
> 			61 창 Comm_Network Secondary Actions: Raise ID: 65286
> 				62 창 ID: 59648
> 					63 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						64 단추 위쪽 스크롤 화살표 ID: UpButton
> 						65 위치 조정 위치 ID: ScrollbarThumb
> 						66 단추 페이지 아래로 ID: DownPageButton
> 						67 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			68 창 ECAT_DS402Base [RO] Secondary Actions: Raise ID: 65285
> 				69 창 ID: 59648
> 					70 창 FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::Init ECAT_Slave_Base::Init(); if _FirstScan then UseDefaultEnableSequence := UseDefaultEnableSequence.Read() <> 0; sigclib_atomic_setU32(pValue:=#sDriveIFSDO.udIsSDOActive, value:=ECAT_SB_SDO_FREE); end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRt // Update AxError if StateWord.Fault = FALSE then AxError := 0; AxErrorQuit := 0; ControlWord.ResetFault := FALSE; else // Set AxError if Fault is set in Stateword and Error is not beeing quit. if AxErrorQuit = 0 then AxError := 1; elsif (ops.tabsolute - TimeStampResetFault) >= GetQuitErrorTimeout() then AxError := 1; AxErrorQuit := 0; ControlWord.ResetFault := FALSE; end_if; end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRtPostScan if AxError = 0 then if UseDefaultEnableSequence then EnableSequence(); end_if; end_if; END_FUNCTION FUNCTION VIRTUAL ECAT_DS402Base::EnableSequence //************************************************************************************************** // ENABLE/DISABLE SEQUENCE //************************************************************************************************** case EnableSequenceSSW of //************************************************************************************************** e_EnableSequenceSSW::_Disabled: // do nothing special, we're enabled now e_EnableSequenceSSW::_Enabled: // do nothing special, we're disabled now //************************************************************************************************** e_EnableSequenceSSW::_StartEnable: ControlWord.SwitchOn := FALSE; ControlWord.EnableVoltage := TRUE; ControlWord.QuickStop := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _SwitchOn; //************************************************************************************************** e_EnableSequenceSSW::_SwitchOn: if StateWord.VoltageEnabled StateWord.QuickStop & StateWord.ReadyToSwitchOn then ControlWord.SwitchOn := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _EnableOperation; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_EnableOperation: if StateWord.VoltageEnabled & StateWord.QuickStop & StateWord.ReadyToSwitchOn & StateWord.SwitchedOn then ControlWord.EnableOperation := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _CheckOperationEnabled; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_CheckOperationEnabled: if StateWord.VoltageEnabled & StateWord.QuickStop & StateWord.ReadyToSwitchOn & StateWord.SwitchedOn & StateWord.OperationEnabled then EnableSequenceSSW := _Enabled; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_DisableOperation: if StateWord.OperationEnabled = 0 then AxEnable := 0; EnableSequenceSSW := _Disabled; end_if; //**************************************************************************** ID: 10000
> 						71 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							72 단추 위쪽 스크롤 화살표 ID: UpButton
> 							73 위치 조정 위치 ID: ScrollbarThumb
> 							74 단추 페이지 아래로 ID: DownPageButton
> 							75 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						76 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							77 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							78 위치 조정 위치 ID: ScrollbarThumb
> 							79 단추 페이지 오른쪽으로 ID: DownPageButton
> 							80 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						81 위치 조정 (disabled)
> 			82 창 Elmo_2 Secondary Actions: Raise ID: 65284
> 				83 창 ID: 59648
> 					84 창 FUNCTION VIRTUAL GLOBAL Elmo_2::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_2::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_2_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_2_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_2::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_2_SETPOS_INDEX, ELMO_2_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_2_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_2_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_2_CONTROLWORD_INDEX, ELMO_2_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_2_ACTPOS_INDEX, ELMO_2_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_2_INPUTS_DIGITALINPUTS_INDEX, ELMO_2_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_2_STATEWORD_INDEX, ELMO_2_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						85 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							86 단추 위쪽 스크롤 화살표 ID: UpButton
> 							87 단추 페이지 위로 ID: UpPageButton
> 							88 위치 조정 위치 ID: ScrollbarThumb
> 							89 단추 페이지 아래로 ID: DownPageButton
> 							90 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						91 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							92 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							93 위치 조정 위치 ID: ScrollbarThumb
> 							94 단추 페이지 오른쪽으로 ID: DownPageButton
> 							95 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						96 위치 조정 (disabled)
> 			97 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				98 창 ID: 59648
> 					99 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="status_word"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000
> 						100 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							101 단추 위쪽 스크롤 화살표 ID: UpButton
> 							102 위치 조정 위치 ID: ScrollbarThumb
> 							103 단추 페이지 아래로 ID: DownPageButton
> 							104 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						105 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							106 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							107 위치 조정 위치 ID: ScrollbarThumb
> 							108 단추 페이지 오른쪽으로 ID: DownPageButton
> 							109 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						110 위치 조정 (disabled)
> 			111 창 LMCRecorderStore Secondary Actions: Raise ID: 65282
> 				112 창 ID: 59648
> 					113 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_RECORDER_SCHEMA_VERSION 1 #define LMC_RECORDER_MAP_REVISION 0x957F101E #define LMC_RECORDER_ERROR_ID -32000 #define LMC_RECORDER_STORAGE_BYTES 1280000 #define LMC_RECORDER_MAX_CHANNELS 24 #define LMC_RECORDER_MAX_CHUNK_BYTES 1280 #define LMC_RECORDER_EMPTY 0 #define LMC_RECORDER_CONFIGURED 1 #define LMC_RECORDER_ARMED 2 #define LMC_RECORDER_RECORDING 3 #define LMC_RECORDER_READY 4 #define LMC_RECORDER_UPLOADING 5 #define LMC_RECORDER_FAULT 6 #define LMC_RECORDER_STOP_NONE 0 #define LMC_RECORDER_STOP_COUNT_COMPLETE 1 #define LMC_RECORDER_STOP_USER 2 #define LMC_RECORDER_STOP_TRIGGER_COMPLETE 3 // The data bank is global so the generated class object stays below the // 16-bit object-size field used by the LASAL class table. Exactly one // LMCRecorderStore object is allowed in the project. VAR_GLOBAL g_LMCRecorderData : ARRAY [0..1279999] OF USINT; END_VAR FUNCTION GLOBAL LMCRecorderStore::NotifySessionClosed VAR_INPUT SessionEpoch : UDINT; END_VAR if (SessionEpoch <> 0) (SessionEpoch = OwnerSessionEpoch) then ClosedSessionEpoch := SessionEpoch; end_if; END_FUNCTION FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot VAR_INPUT pSnapshot : ^USINT; SnapshotSize : UDINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR state : UDINT; startRequest : UDINT; triggerRequest : UDINT; stopRequest : UDINT; sequence : UDINT; cycleCounter : UDINT; channelIndex : UINT; dataOffset : UDINT; snapshotOffset : UDINT; triggerRaw : UDINT; triggerHealthOffset : UDINT; triggerEvent : BOOL; triggerInputValid : BOOL; previousCondition : BOOL; currentCondition : BOOL; prehistoryReady : BOOL; previousSigned : DINT; currentSigned : DINT; thresholdSigned : DINT; lowerSigned : DINT; upperSigned : DINT; timestampStep : UDINT; timestampLowBefore : UDINT; END_VAR Result := -1; if (pSnapshot = NIL) | (SnapshotSize < 304) then RETURN; end_if; state := sigclib_atomic_getU32(pValue:=#StateValue); startRequest := sigclib_atomic_getU32(pValue:=#StartRequestSequence); triggerRequest := sigclib_atomic_getU32(pValue:=#TriggerRequestSequence); stopRequest := sigclib_atomic_getU32(pValue:=#StopRequestSequence); sequence := sigclib_atomic_getU32(pValue:=#StatusSequence) + 1; if (sequence and 1) = 0 then sequence += 1; end_if; sigclib_atomic_setU32(pValue:=#StatusSequence, value:=sequence); cycleCounter := pSnapshot^$UDINT; if (state = LMC_RECORDER_ARMED) & (startRequest <> StartAppliedSequence) then StartAppliedSequence := startRequest; SampleCount := 0; DividerCounter := 0; StopReason := LMC_RECORDER_STOP_NONE; TriggerIndex := 0xFFFFFFFF; TriggerCycle := 0; TriggerTimestampLow := 0; TriggerTimestampHigh := 0; StartCycle := 0; EndCycle := 0; StartTimestampLow := 0; StartTimestampHigh := 0; EndTimestampLow := 0; EndTimestampHigh := 0; DroppedCycles := 0; OverflowCount := 0; WriteSampleIndex := 0; FrozenFirstSampleIndex := 0; PostSamplesRemaining := 0; PreviousTriggerValue := 0; PreviousTriggerValid := FALSE; state := LMC_RECORDER_RECORDING; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_RECORDING); end_if; if ((state = LMC_RECORDER_ARMED) | (state = LMC_RECORDER_RECORDING)) & (stopRequest <> StopAppliedSequence) then StopAppliedSequence := stopRequest; if (TriggerType <> 0) & (TriggerIndex = 0xFFFFFFFF) & (SampleCapacity > 0) then FrozenFirstSampleIndex := (WriteSampleIndex + SampleCapacity - SampleCount) MOD SampleCapacity; end_if; StopReason := LMC_RECORDER_STOP_USER; if SampleCount = 0 then EndCycle := cycleCounter; EndTimestampLow := (pSnapshot + 4)^$UDINT; EndTimestampHigh := (pSnapshot + 8)^$UDINT; end_if; state := LMC_RECORDER_READY; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_READY); end_if; if state = LMC_RECORDER_RECORDING then if DividerCounter = 0 then if TriggerType = 0 then if SampleCount < SampleCapacity then if SampleCount = 0 then Start ID: 10000
> 						114 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							115 단추 위쪽 스크롤 화살표 ID: UpButton
> 							116 위치 조정 위치 ID: ScrollbarThumb
> 							117 단추 페이지 아래로 ID: DownPageButton

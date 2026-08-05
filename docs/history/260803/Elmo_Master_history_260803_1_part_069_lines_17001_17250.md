>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "새 pDest 선택",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,element_index:79045});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "새 pDest 속성 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 7868208,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=17043 chars=9977 sha256=3d34baa568e0957b24c1eb43e1acaa925a0e32e1aeddde2bed3a14da52fe4653]'... 211623 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=17052 chars=9977 sha256=2743db4c85442c03990bff57c7553f8e898672daf40dd4399065c5f949a343a2]'... 191947 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch* Secondary Actions: Raise\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
>       '\t\t\t2753 창 LMCEcatInputLatch* Secondary Actions: Raise ID: 65283\n' +
>       '\t\t\t\t2754 창 ID: 59648\n' +
>       '\t\t\t\t\t2755 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000\n' +
>       '\t\t\t\t\t\t2756 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t2757 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t2758 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t2759 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t2760 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t2761 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t2762 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t2763 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t2764 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t2765 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t2766 위치 조정 (disabled)\n' +
>       '\t\t\t2 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65282\n' +
>       '\t\t\t\t3 창 ID: 59648\n' +
>       '\t\t\t\t\t4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex >= 0x8000 then physicalAxis := (CatalogIndex shr 8) and 0x007F; topologyCount := CatalogIndex and 0x00FF; if (topologyCount = 0) | (physicalAxis >= 7) | ((physicalAxis + topologyCount) > 7) then RETURN; end_if; (pEntry + 16)^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 20)^$UINT := phy ID: 10000\n' +
>       '\t\t\t\t\t\t5 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t6 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t7 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t8 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t9 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t11 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t12 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t13 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t14 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t15 위치 조정 (disabled)\n' +
>       '\t\t\t16 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281\n' +
>       '\t\t\t\t17 창 ID: 59648\n' +
>       '\t\t\t\t\t18 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : '... 13359 more characters,
>     focused_element: '143 트리 ID: 103'
>   }
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
>   "title": "pDest 형식 목록 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1429,y:493,screenshotId:globalThis.state.screenshots[1].id});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "내장 형식 목록 끝으로 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"END\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "형식 목록 끝 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 7868208,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=17155 chars=9977 sha256=3d34baa568e0957b24c1eb43e1acaa925a0e32e1aeddde2bed3a14da52fe4653]'... 214763 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=17164 chars=9977 sha256=2743db4c85442c03990bff57c7553f8e898672daf40dd4399065c5f949a343a2]'... 195179 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch* Secondary Actions: Raise\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
>       '\t\t\t2753 창 LMCEcatInputLatch* Secondary Actions: Raise ID: 65283\n' +
>       '\t\t\t\t2754 창 ID: 59648\n' +
>       '\t\t\t\t\t2755 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000\n' +
>       '\t\t\t\t\t\t2756 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t2757 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t2758 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t2759 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t2760 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t2761 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t2762 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t2763 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t2764 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t2765 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t2766 위치 조정 (disabled)\n' +
>       '\t\t\t2 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65282\n' +
>       '\t\t\t\t3 창 ID: 59648\n' +
>       '\t\t\t\t\t4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex >= 0x8000 then physicalAxis := (CatalogIndex shr 8) and 0x007F; topologyCount := CatalogIndex and 0x00FF; if (topologyCount = 0) | (physicalAxis >= 7) | ((physicalAxis + topologyCount) > 7) then RETURN; end_if; (pEntry + 16)^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 20)^$UINT := phy ID: 10000\n' +
>       '\t\t\t\t\t\t5 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t6 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t7 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t8 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t9 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t11 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t12 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t13 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t14 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t15 위치 조정 (disabled)\n' +
>       '\t\t\t16 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281\n' +
>       '\t\t\t\t17 창 ID: 59648\n' +
>       '\t\t\t\t\t18 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : '... 13414 more characters,
>     focused_element: '174 TABLE Properties Window ID: 294249272'
>   }
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> <details><summary>Called 2 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "컴퓨터 제어 동작 확인",
>   "code": "nodeRepl.write(Object.keys(globalThis.sky));"
> }
> ```
>
> ```text
> [
>   'target',
>   'activate_window',
>   'get_window_state',
>   'click',
>   'scroll',
>   'drag',
>   'press_key',
>   'type_text',
>   'launch_app',
>   'list_apps',
>   'list_windows',
>   'get_window',
>   'perform_secondary_action',
>   'set_value',
>   'transport'
> ]
> ```
>
> MCP tool call
>
> node_repl.js

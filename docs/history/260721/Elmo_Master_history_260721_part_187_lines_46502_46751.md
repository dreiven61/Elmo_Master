> 					19 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						20 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21 위치 조정 위치 ID: ScrollbarThumb
> 						22 단추 페이지 아래로 ID: DownPageButton
> 						23 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					24 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						25 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						26 위치 조정 위치 ID: ScrollbarThumb
> 						27 단추 페이지 오른쪽으로 ID: DownPageButton
> 						28 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					29 위치 조정 (disabled)
> 			30 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65287
> 				31 창 ID: 59648
> 					32 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="status_word"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000
> 						33 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							34 단추 위쪽 스크롤 화살표 ID: UpButton
> 							35 단추 페이지 위로 ID: UpPageButton
> 							36 위치 조정 위치 ID: ScrollbarThumb
> 							37 단추 페이지 아래로 ID: DownPageButton
> 							38 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						39 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							40 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							41 위치 조정 위치 ID: ScrollbarThumb
> 							42 단추 페이지 오른쪽으로 ID: DownPageButton
> 							43 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						44 위치 조정 (disabled)
> 			45 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65286
> 				46 창 ID: 59648
> 					47 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 						48 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							49 단추 위쪽 스크롤 화살표 ID: UpButton
> 							50 단추 페이지 위로 ID: UpPageButton
> 							51 위치 조정 위치 ID: ScrollbarThumb
> 							52 단추 페이지 아래로 ID: DownPageButton
> 							53 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						54 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							55 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							56 위치 조정 위치 ID: ScrollbarThumb
> 							57 단추 페이지 오른쪽으로 ID: DownPageButton
> 							58 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						59 위치 조정 (disabled)
> 			60 창 Hardware Editor Secondary Actions: Raise ID: 65285
> 				61 창 ID: 59648
> 					62 창 xtpBarTop ID: 59419
> 						63 도구 모음 Hardware Editor
> 							64 단추 Hardware Editor Configuration Settings
> 							65 단추 Calculate Resources of Project
> 							66 단추 (disabled) Upload Hardware Tree from PLC
> 							67 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							68 단추 Show Detail Mode\Show Configuration Mode
> 							69 단추 Generates the ENI File of the current project
> 					70 창 ID: 59648
> 						71 트리 ID: 1
> 							72 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								73 단추 위쪽 스크롤 화살표 ID: UpButton
> 								74 단추 페이지 위로 ID: UpPageButton
> 								75 위치 조정 위치 ID: ScrollbarThumb
> 								76 단추 페이지 아래로 ID: DownPageButton
> 								77 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							78 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								79 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								80 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								81 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								82 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								83 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								84 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								85 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								86 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								87 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								88 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								89 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								90 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								91 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								92 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								93 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								94 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								95 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								96 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								97 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								98 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								99 콘솔 트리 (selectable) ALARM:00, Empty
> 								100 콘솔 트리 (selectable) SDIAS:00, Empty
> 								101 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								102 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							103 콘솔 트리 (selectable) Unplaced Module(s)
> 			104 창 Elmo_4 Secondary Actions: Raise ID: 65284
> 				105 창 ID: 59648
> 					106 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						107 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							108 단추 위쪽 스크롤 화살표 ID: UpButton
> 							109 위치 조정 위치 ID: ScrollbarThumb
> 							110 단추 페이지 아래로 ID: DownPageButton
> 							111 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						112 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							113 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							114 위치 조정 위치 ID: ScrollbarThumb
> 							115 단추 페이지 오른쪽으로 ID: DownPageButton
> 							116 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						117 위치 조정 (disabled)
> 			118 창 Motion_Network Secondary Actions: Raise ID: 65283
> 				119 창 ID: 59648
> 					120 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						121 단추 위쪽 스크롤 화살표 ID: UpButton
> 						122 위치 조정 위치 ID: ScrollbarThumb
> 						123 단추 페이지 아래로 ID: DownPageButton
> 						124 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					125 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						126 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						127 위치 조정 위치 ID: ScrollbarThumb
> 						128 단추 페이지 오른쪽으로 ID: DownPageButton
> 						129 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					130 위치 조정 (disabled)
> 			131 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				132 창 ID: 59648
> 					133 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						134 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							135 단추 위쪽 스크롤 화살표 ID: UpButton
> 							136 위치 조정 위치 ID: ScrollbarThumb
> 							137 단추 페이지 아래로 ID: DownPageButton
> 							138 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						139 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							140 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							141 위치 조정 위치 ID: ScrollbarThumb
> 							142 단추 페이지 오른쪽으로 ID: DownPageButton
> 							143 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						144 위치 조정 (disabled)
> 			145 창 HW_Network Secondary Actions: Raise ID: 65281
> 				146 창 ID: 59648
> 					147 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						148 단추 위쪽 스크롤 화살표 ID: UpButton
> 						149 위치 조정 위치 ID: ScrollbarThumb
> 						150 단추 페이지 아래로 ID: DownPageButton
> 						151 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					152 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						153 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						154 위치 조정 위치 ID: ScrollbarThumb
> 						155 단추 페이지 오른쪽으로 ID: DownPageButton
> 						156 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					157 위치 조정 (disabled)
> 		158 상태 표시줄 ID: 59393
> 			159 텍스트
> 			160 텍스트 TCPMotionInterface::MsgPaser
> 			161 텍스트
> 			162 텍스트 Ln 498 Col 31
> 			163 텍스트
> 			164 텍스트 Offline
> 			165 텍스트
> 			166 텍스트 NUM
> 			167 텍스트
> 		168 창 xtpBarTop ID: 59419
> 			169 도구 모음 Script
> 			170 도구 모음 Edit
> 				171 단추 Toggle bookmark
> 				172 단추 (disabled) Previous bookmark
> 				173 단추 (disabled) Next bookmark
> 				174 단추 (disabled) Delete all bookmarks
> 				175 단추 (disabled) Previous bookmark in this file
> 				176 단추 (disabled) Next bookmark in this file
> 				177 단추 Comment selected text (Ctrl+Shift+C)
> 				178 단추 Remove comment (Ctrl+Shift+X)
> 				179 단추 Unindent (Shift+Tab)
> 				180 단추 Indent (Tab)
> 			181 도구 모음 Macros Manager
> 				182 메뉴 항목 Macros
> 			183 도구 모음 Layout Manager
> 				184 메뉴 항목 Layouts
> 			185 도구 모음 Toolbox
> 				186 단추 DataAnalyzer
> 				187 메뉴 항목 Toolbar Options
> 			188 도구 모음 Net Edit
> 				189 단추 (disabled) Select
> 				190 메뉴 항목 Toolbar Options
> 			191 도구 모음 Debug
> 				192 단추 Go online (Alt+F6)
> 				193 단추 Change Online Settings
> 				194 메뉴 항목 Online Connection
> 				195 단추 (disabled) Set Online Connection For Current Project
> 				196 단추 (disabled) Download (F6)
> 				197 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				198 단추 (disabled) Download Module on the Fly
> 				199 단추 (disabled) Save Project on PLC
> 				200 단추 (disabled) Start (F7)
> 				201 단추 (disabled) Reset (F8)
> 				202 단추 Toggle breakpoint (F4)
> 				203 단추 Create condition breakpoint
> 				204 메뉴 항목 Toolbar Options
> 			205 도구 모음 Build
> 				206 메뉴 항목 Target Architecture
> 				207 단추 Build changes (F9)
> 				208 단추 Rebuild project (Strg+F9)
> 				209 단추 (disabled) Cancel building (Ctrl+Break)
> 				210 단추 Link project
> 			211 도구 모음 Standard
> 				212 단추 New project (Strg+N)
> 				213 단추 Open a file (Strg+Shift+O)
> 				214 단추 Close active document (Strg+F4)
> 				215 단추 (disabled) Save file (Strg+S)
> 				216 단추 Open project (Strg+O)
> 				217 단추 (disabled) Save project changes (Strg+Shift+S)
> 				218 단추 Close project
> 				219 단추 Print
> 				220 단추 Cut (Strg+X)
> 				221 단추 Copy (Strg+C)
> 				222 단추 (disabled) Paste (Strg+V)
> 				223 메뉴 항목 (disabled) Undo (Strg+Z)
> 				224 메뉴 항목 (disabled) Redo (Strg+Y)
> 				225 단추 Navigate Backward (Alt+Left)
> 				226 단추 (disabled) Navigate Forward (Alt +Right)
> 			227 메뉴 모음 Menu Bar
> 				228 메뉴 항목 FILE
> 				229 메뉴 항목 EDIT
> 				230 메뉴 항목 VIEW
> 				231 메뉴 항목 PROJECT
> 				232 메뉴 항목 BUILD
> 				233 메뉴 항목 DEBUG
> 				234 메뉴 항목 ANALYZE
> 				235 메뉴 항목 TOOLS
> 				236 메뉴 항목 EXTRAS
> 				237 메뉴 항목 WINDOW
> 				238 메뉴 항목 HELP
> 		239 창 Splitter ID: 481389416
> 		240 창 Splitter ID: 481390928
> 		241 Tab Output ID: 409867992
> 			242 창 ID: 1200
> 				243 창 ID: 1200
> 					244 LIST ID: 1204
> 						245 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							246 단추 위쪽 스크롤 화살표 ID: UpButton
> 							247 단추 페이지 위로 ID: UpPageButton
> 							248 위치 조정 위치 ID: ScrollbarThumb
> 							249 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						250 목록 항목 (selectable)
> 						251 목록 항목 (selectable)
> 						252 목록 항목 (selectable)
> 						253 목록 항목 (selectable)
> 						254 목록 항목 (selectable)
> 						255 목록 항목 (selectable)
> 						256 목록 항목 (selectable)
> 						257 목록 항목 (selectable)
> 						258 목록 항목 (selectable)
> 						259 목록 항목 (selectable)
> 					260 스크롤 막대 ID: 59904
> 						261 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						262 위치 조정 위치 ID: ScrollbarThumb
> 						263 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			264 탭 항목 (selectable) Python Script
> 			265 탭 항목 (selectable) Debugger
> 			266 탭 항목 (selectable) Output
> 			267 단추 Close
> 		268 창 Splitter ID: 608798568

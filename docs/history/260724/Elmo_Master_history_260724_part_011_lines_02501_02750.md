>       width: 1440,
>       height: 753
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
>       '\t\t\t2 창 Motion_Network Secondary Actions: Raise ID: 65280\n' +
>       '\t\t\t\t3 창 ID: 59648\n' +
>       '\t\t\t\t\t4 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t5 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t6 단추 페이지 위로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t7 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t8 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t9 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t11 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t12 단추 페이지 왼쪽으로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t13 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t14 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t15 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t16 위치 조정 (disabled)\n' +
>       '\t\t\t17 창 HW_Network Secondary Actions: Raise ID: 65288\n' +
>       '\t\t\t\t18 창 ID: 59648\n' +
>       '\t\t\t\t\t19 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t20 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t21 단추 페이지 위로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t22 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t23 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t24 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t25 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t26 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t27 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t28 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t29 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t30 위치 조정 (disabled)\n' +
>       '\t\t\t31 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65281\n' +
>       '\t\t\t\t32 창 ID: 59648\n' +
>       '\t\t\t\t\t33 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000\n' +
>       '\t\t\t\t\t\t34 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t35 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t36 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t37 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t38 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t39 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t40 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t41 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t42 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t43 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t44 위치 조정 (disabled)\n' +
>       '\t\t\t45 창 LMCRecorderStore Secondary Actions: Raise ID: 65282\n' +
>       '\t\t\t\t46 창 ID: 59648\n' +
>       '\t\t\t\t\t47 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_RECORDER_SCHEMA_VERSION 1 #define LMC_RECORDER_MAP_REVISION 0x957F101E #define LMC_RECORDER_ERROR_ID -32000 #define LMC_RECORDER_STORAGE_BYTES 1280000 #define LMC_RECORDER_MAX_CHANNELS 24 #define LMC_RECORDER_MAX_CHUNK_BYTES 1280 #define LMC_RECORDER_EMPTY 0 #define LMC_RECORDER_CONFIGURED 1 #define LMC_RECORDER_ARMED 2 #define LMC_RECORDER_RECORDING 3 #define LMC_RECORDER_READY 4 #define LMC_RECORDER_UPLOADING 5 #define LMC_RECORDER_FAULT 6 #define LMC_RECORDER_STOP_NONE 0 #define LMC_RECORDER_STOP_COUNT_COMPLETE 1 #define LMC_RECORDER_STOP_USER 2 #define LMC_RECORDER_STOP_TRIGGER_COMPLETE 3 // The data bank is global so the generated class object stays below the // 16-bit object-size field used by the LASAL class table. Exactly one // LMCRecorderStore object is allowed in the project. VAR_GLOBAL g_LMCRecorderData : ARRAY [0..1279999] OF USINT; END_VAR FUNCTION GLOBAL LMCRecorderStore::NotifySessionClosed VAR_INPUT SessionEpoch : UDINT; END_VAR if (SessionEpoch <> 0) (SessionEpoch = OwnerSessionEpoch) then ClosedSessionEpoch := SessionEpoch; end_if; END_FUNCTION FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot VAR_INPUT pSnapshot : ^USINT; SnapshotSize : UDINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR state : UDINT; startRequest : UDINT; triggerRequest : UDINT; stopRequest : UDINT; sequence : UDINT; cycleCounter : UDINT; channelIndex : UINT; dataOffset : UDINT; snapshotOffset : UDINT; triggerRaw : UDINT; triggerHealthOffset : UDINT; triggerEvent : BOOL; triggerInputValid : BOOL; previousCondition : BOOL; currentCondition : BOOL; prehistoryReady : BOOL; previousSigned : DINT; currentSigned : DINT; thresholdSigned : DINT; lowerSigned : DINT; upperSigned : DINT; timestampStep : UDINT; timestampLowBefore : UDINT; END_VAR Result := -1; if (pSnapshot = NIL) | (SnapshotSize < 304) then RETURN; end_if; state := sigclib_atomic_getU32(pValue:=#StateValue); startRequest := sigclib_atomic_getU32(pValue:=#StartRequestSequence); triggerRequest := sigclib_atomic_getU32(pValue:=#TriggerRequestSequence); stopRequest := sigclib_atomic_getU32(pValue:=#StopRequestSequence); sequence := sigclib_atomic_getU32(pValue:=#StatusSequence) + 1; if (sequence and 1) = 0 then sequence += 1; end_if; sigclib_atomic_setU32(pValue:=#StatusSequence, value:=sequence); cycleCounter := pSnapshot^$UDINT; if (state = LMC_RECORDER_ARMED) & (startRequest <> StartAppliedSequence) then StartAppliedSequence := startRequest; SampleCount := 0; DividerCounter := 0; StopReason := LMC_RECORDER_STOP_NONE; TriggerIndex := 0xFFFFFFFF; TriggerCycle := 0; TriggerTimestampLow := 0; TriggerTimestampHigh := 0; StartCycle := 0; EndCycle := 0; StartTimestampLow := 0; StartTimestampHigh := 0; EndTimestampLow := 0; EndTimestampHigh := 0; DroppedCycles := 0; OverflowCount := 0; WriteSampleIndex := 0; FrozenFirstSampleIndex := 0; PostSamplesRemaining := 0; PreviousTriggerValue := 0; PreviousTriggerValid := FALSE; state := LMC_RECORDER_RECORDING; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_RECORDING); end_if; if ((state = LMC_RECORDER_ARMED) | (state = LMC_RECORDER_RECORDING)) & (stopRequest <> StopAppliedSequence) then StopAppliedSequence := stopRequest; if (TriggerType <> 0) & (TriggerIndex = 0xFFFFFFFF) & (SampleCapacity > 0) then FrozenFirstSampleIndex := (WriteSampleIndex + SampleCapacity - SampleCount) MOD SampleCapacity; end_if; StopReason := LMC_RECORDER_STOP_USER; if SampleCount = 0 then EndCycle := cycleCounter; EndTimestampLow := (pSnapshot + 4)^$UDINT; EndTimestampHigh := (pSnapshot + 8)^$UDINT; end_if; state := LMC_RECORDER_READY; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_READY); end_if; if state = LMC_RECORDER_RECORDING then if DividerCounter = 0 then if TriggerType = 0 then if SampleCount < SampleCapacity then if SampleCount = 0 then Start ID: 10000\n' +
>       '\t\t\t\t\t\t48 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t49 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t50 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t51 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t52 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t53 스크롤 막대 가로 ID: NonClientHorizonta'... 21828 more characters,
>     focused_element: '252 트리 ID: 103'
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
>   "code": "await sky.click({window:lasalWindow,x:86,y:40}); nodeRepl.write(\"clicked Edit\");",
>   "title": "LASAL 편집 메뉴 열기"
> }
> ```
>
> ```text
> clicked Edit
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "lasalState = await sky.get_window_state({window:lasalWindow,include_screenshot:true,include_text:true}); nodeRepl.write(lasalState.accessibility.tree);",
>   "title": "LASAL 편집 메뉴 확인"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise
> 		1 창
> 		2 창
> 		3 메뉴 Edit
> 			4 메뉴 항목 Undo Ctrl+Z
> 			5 메뉴 항목 (disabled) Redo Ctrl+Y
> 			6 메뉴 항목 Cut Ctrl+X
> 			7 메뉴 항목 Copy Ctrl+C
> 			8 메뉴 항목 Paste Ctrl+V
> 			9 메뉴 항목 Find... Ctrl+F
> 			10 메뉴 항목 Find in Files... Ctrl+Shift+F
> 			11 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 			12 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 			13 메뉴 항목 Replace... Ctrl+R
> 			14 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 			15 메뉴 항목 (disabled) Goto Line... Ctrl+G
> 			16 메뉴 항목 Reset Editor Colors
> 			17 메뉴 항목 Mark
> 			18 메뉴 항목 Bookmark
> 			19 메뉴 항목 Navigate Backward Alt+Left Arrow
> 			20 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 			21 메뉴 항목 Select All Ctrl+A
> 			22 메뉴 항목 Comment
> 			23 메뉴 항목 Dent
> 		24 창 작업 영역 ID: 59648
> 			25 창 Motion_Network Secondary Actions: Raise ID: 65280
> 				26 창 ID: 59648
> 					27 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						28 단추 위쪽 스크롤 화살표 ID: UpButton
> 						29 단추 페이지 위로 ID: UpPageButton
> 						30 위치 조정 위치 ID: ScrollbarThumb
> 						31 단추 페이지 아래로 ID: DownPageButton
> 						32 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					33 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						34 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						35 단추 페이지 왼쪽으로 ID: UpPageButton
> 						36 위치 조정 위치 ID: ScrollbarThumb
> 						37 단추 페이지 오른쪽으로 ID: DownPageButton
> 						38 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					39 위치 조정 (disabled)
> 			40 창 HW_Network Secondary Actions: Raise ID: 65288
> 				41 창 ID: 59648
> 					42 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						43 단추 위쪽 스크롤 화살표 ID: UpButton
> 						44 단추 페이지 위로 ID: UpPageButton
> 						45 위치 조정 위치 ID: ScrollbarThumb
> 						46 단추 페이지 아래로 ID: DownPageButton
> 						47 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					48 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						49 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						50 위치 조정 위치 ID: ScrollbarThumb
> 						51 단추 페이지 오른쪽으로 ID: DownPageButton
> 						52 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					53 위치 조정 (disabled)
> 			54 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65281
> 				55 창 ID: 59648
> 					56 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 						57 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							58 단추 위쪽 스크롤 화살표 ID: UpButton
> 							59 위치 조정 위치 ID: ScrollbarThumb
> 							60 단추 페이지 아래로 ID: DownPageButton
> 							61 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						62 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							63 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							64 위치 조정 위치 ID: ScrollbarThumb
> 							65 단추 페이지 오른쪽으로 ID: DownPageButton
> 							66 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						67 위치 조정 (disabled)
> 			68 창 LMCRecorderStore Secondary Actions: Raise ID: 65282
> 				69 창 ID: 59648
> 					70 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_RECORDER_SCHEMA_VERSION 1 #define LMC_RECORDER_MAP_REVISION 0x957F101E #define LMC_RECORDER_ERROR_ID -32000 #define LMC_RECORDER_STORAGE_BYTES 1280000 #define LMC_RECORDER_MAX_CHANNELS 24 #define LMC_RECORDER_MAX_CHUNK_BYTES 1280 #define LMC_RECORDER_EMPTY 0 #define LMC_RECORDER_CONFIGURED 1 #define LMC_RECORDER_ARMED 2 #define LMC_RECORDER_RECORDING 3 #define LMC_RECORDER_READY 4 #define LMC_RECORDER_UPLOADING 5 #define LMC_RECORDER_FAULT 6 #define LMC_RECORDER_STOP_NONE 0 #define LMC_RECORDER_STOP_COUNT_COMPLETE 1 #define LMC_RECORDER_STOP_USER 2 #define LMC_RECORDER_STOP_TRIGGER_COMPLETE 3 // The data bank is global so the generated class object stays below the // 16-bit object-size field used by the LASAL class table. Exactly one // LMCRecorderStore object is allowed in the project. VAR_GLOBAL g_LMCRecorderData : ARRAY [0..1279999] OF USINT; END_VAR FUNCTION GLOBAL LMCRecorderStore::NotifySessionClosed VAR_INPUT SessionEpoch : UDINT; END_VAR if (SessionEpoch <> 0) (SessionEpoch = OwnerSessionEpoch) then ClosedSessionEpoch := SessionEpoch; end_if; END_FUNCTION FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot VAR_INPUT pSnapshot : ^USINT; SnapshotSize : UDINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR state : UDINT; startRequest : UDINT; triggerRequest : UDINT; stopRequest : UDINT; sequence : UDINT; cycleCounter : UDINT; channelIndex : UINT; dataOffset : UDINT; snapshotOffset : UDINT; triggerRaw : UDINT; triggerHealthOffset : UDINT; triggerEvent : BOOL; triggerInputValid : BOOL; previousCondition : BOOL; currentCondition : BOOL; prehistoryReady : BOOL; previousSigned : DINT; currentSigned : DINT; thresholdSigned : DINT; lowerSigned : DINT; upperSigned : DINT; timestampStep : UDINT; timestampLowBefore : UDINT; END_VAR Result := -1; if (pSnapshot = NIL) | (SnapshotSize < 304) then RETURN; end_if; state := sigclib_atomic_getU32(pValue:=#StateValue); startRequest := sigclib_atomic_getU32(pValue:=#StartRequestSequence); triggerRequest := sigclib_atomic_getU32(pValue:=#TriggerRequestSequence); stopRequest := sigclib_atomic_getU32(pValue:=#StopRequestSequence); sequence := sigclib_atomic_getU32(pValue:=#StatusSequence) + 1; if (sequence and 1) = 0 then sequence += 1; end_if; sigclib_atomic_setU32(pValue:=#StatusSequence, value:=sequence); cycleCounter := pSnapshot^$UDINT; if (state = LMC_RECORDER_ARMED) & (startRequest <> StartAppliedSequence) then StartAppliedSequence := startRequest; SampleCount := 0; DividerCounter := 0; StopReason := LMC_RECORDER_STOP_NONE; TriggerIndex := 0xFFFFFFFF; TriggerCycle := 0; TriggerTimestampLow := 0; TriggerTimestampHigh := 0; StartCycle := 0; EndCycle := 0; StartTimestampLow := 0; StartTimestampHigh := 0; EndTimestampLow := 0; EndTimestampHigh := 0; DroppedCycles := 0; OverflowCount := 0; WriteSampleIndex := 0; FrozenFirstSampleIndex := 0; PostSamplesRemaining := 0; PreviousTriggerValue := 0; PreviousTriggerValid := FALSE; state := LMC_RECORDER_RECORDING; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_RECORDING); end_if; if ((state = LMC_RECORDER_ARMED) | (state = LMC_RECORDER_RECORDING)) & (stopRequest <> StopAppliedSequence) then StopAppliedSequence := stopRequest; if (TriggerType <> 0) & (TriggerIndex = 0xFFFFFFFF) & (SampleCapacity > 0) then FrozenFirstSampleIndex := (WriteSampleIndex + SampleCapacity - SampleCount) MOD SampleCapacity; end_if; StopReason := LMC_RECORDER_STOP_USER; if SampleCount = 0 then EndCycle := cycleCounter; EndTimestampLow := (pSnapshot + 4)^$UDINT; EndTimestampHigh := (pSnapshot + 8)^$UDINT; end_if; state := LMC_RECORDER_READY; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_READY); end_if; if state = LMC_RECORDER_RECORDING then if DividerCounter = 0 then if TriggerType = 0 then if SampleCount < SampleCapacity then if SampleCount = 0 then Start ID: 10000
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
> 			82 창 Comm_Network Secondary Actions: Raise ID: 65286
> 				83 창 ID: 59648
> 					84 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						85 단추 위쪽 스크롤 화살표 ID: UpButton
> 						86 위치 조정 위치 ID: ScrollbarThumb
> 						87 단추 페이지 아래로 ID: DownPageButton
> 						88 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					89 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						90 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						91 위치 조정 위치 ID: ScrollbarThumb
> 						92 단추 페이지 오른쪽으로 ID: DownPageButton
> 						93 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					94 위치 조정 (disabled)
> 			95 창 TCPMotionInterface Secondary Actions: Raise ID: 65287
> 				96 창 ID: 59648
> 					97 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 						98 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							99 단추 위쪽 스크롤 화살표 ID: UpButton
> 							100 위치 조정 위치 ID: ScrollbarThumb
> 							101 단추 페이지 아래로 ID: DownPageButton
> 							102 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						103 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							104 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							105 위치 조정 위치 ID: ScrollbarThumb
> 							106 단추 페이지 오른쪽으로 ID: DownPageButton
> 							107 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						108 위치 조정 (disabled)
> 			109 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				110 창 ID: 59648
> 					111 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="status_word"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000
> 						112 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							113 단추 위쪽 스크롤 화살표 ID: UpButton
> 							114 위치 조정 위치 ID: ScrollbarThumb
> 							115 단추 페이지 아래로 ID: DownPageButton
> 							116 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						117 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							118 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							119 위치 조정 위치 ID: ScrollbarThumb
> 							120 단추 페이지 오른쪽으로 ID: DownPageButton
> 							121 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						122 위치 조정 (disabled)
> 			123 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65289
> 				124 창 ID: 59648
> 					125 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						126 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							127 단추 위쪽 스크롤 화살표 ID: UpButton
> 							128 위치 조정 위치 ID: ScrollbarThumb
> 							129 단추 페이지 아래로 ID: DownPageButton
> 							130 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						131 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							132 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							133 위치 조정 위치 ID: ScrollbarThumb
> 							134 단추 페이지 오른쪽으로 ID: DownPageButton
> 							135 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						136 위치 조정 (disabled)
> 			137 창 ECAT_DS402Base [RO] Secondary Actions: Raise ID: 65285
> 				138 창 ID: 59648
> 					139 창 FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::Init ECAT_Slave_Base::Init(); if _FirstScan then UseDefaultEnableSequence := UseDefaultEnableSequence.Read() <> 0; sigclib_atomic_setU32(pValue:=#sDriveIFSDO.udIsSDOActive, value:=ECAT_SB_SDO_FREE); end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRt // Update AxError if StateWord.Fault = FALSE then AxError := 0; AxErrorQuit := 0; ControlWord.ResetFault := FALSE; else // Set AxError if Fault is set in Stateword and Error is not beeing quit. if AxErrorQuit = 0 then AxError := 1; elsif (ops.tabsolute - TimeStampResetFault) >= GetQuitErrorTimeout() then AxError := 1; AxErrorQuit := 0; ControlWord.ResetFault := FALSE; end_if; end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRtPostScan if AxError = 0 then if UseDefaultEnableSequence then EnableSequence(); end_if; end_if; END_FUNCTION FUNCTION VIRTUAL ECAT_DS402Base::EnableSequence //************************************************************************************************** // ENABLE/DISABLE SEQUENCE //************************************************************************************************** case EnableSequenceSSW of //************************************************************************************************** e_EnableSequenceSSW::_Disabled: // do nothing special, we're enabled now e_EnableSequenceSSW::_Enabled: // do nothing special, we're disabled now //************************************************************************************************** e_EnableSequenceSSW::_StartEnable: ControlWord.SwitchOn := FALSE; ControlWord.EnableVoltage := TRUE; ControlWord.QuickStop := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _SwitchOn; //************************************************************************************************** e_EnableSequenceSSW::_SwitchOn: if StateWord.VoltageEnabled StateWord.QuickStop & StateWord.ReadyToSwitchOn then ControlWord.SwitchOn := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _EnableOperation; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_EnableOperation: if StateWord.VoltageEnabled & StateWord.QuickStop & StateWord.ReadyToSwitchOn & StateWord.SwitchedOn then ControlWord.EnableOperation := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _CheckOperationEnabled; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_CheckOperationEnabled: if StateWord.VoltageEnabled & StateWord.QuickStop & StateWord.ReadyToSwitchOn & StateWord.SwitchedOn & StateWord.OperationEnabled then EnableSequenceSSW := _Enabled; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_DisableOperation: if StateWord.OperationEnabled = 0 then AxEnable := 0; EnableSequenceSSW := _Disabled; end_if; //**************************************************************************** ID: 10000
> 						140 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							141 단추 위쪽 스크롤 화살표 ID: UpButton
> 							142 단추 페이지 위로 ID: UpPageButton
> 							143 위치 조정 위치 ID: ScrollbarThumb
> 							144 단추 페이지 아래로 ID: DownPageButton
> 							145 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						146 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							147 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							148 위치 조정 위치 ID: ScrollbarThumb
> 							149 단추 페이지 오른쪽으로 ID: DownPageButton
> 							150 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						151 위치 조정 (disabled)
> 			152 창 Elmo_2 Secondary Actions: Raise ID: 65284

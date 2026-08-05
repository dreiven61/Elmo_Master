> 					23 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						24 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						25 단추 페이지 왼쪽으로 ID: UpPageButton
> 						26 위치 조정 위치 ID: ScrollbarThumb
> 						27 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					28 위치 조정 (disabled)
> 			29 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65288
> 				30 창 ID: 59648
> 					31 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; END_VAR // The legacy recorder prefix remains 304 bytes. The coherent topology and // I/O reader uses the extended 464-byte scalar snapshot. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStat ID: 10000
> 						32 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							33 단추 위쪽 스크롤 화살표 ID: UpButton
> 							34 단추 페이지 위로 ID: UpPageButton
> 							35 위치 조정 위치 ID: ScrollbarThumb
> 							36 단추 페이지 아래로 ID: DownPageButton
> 							37 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							39 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							40 위치 조정 위치 ID: ScrollbarThumb
> 							41 단추 페이지 오른쪽으로 ID: DownPageButton
> 							42 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						43 위치 조정 (disabled)
> 			44 창 Motion_Network Secondary Actions: Raise ID: 65284
> 				45 창 ID: 59648
> 					46 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						47 단추 위쪽 스크롤 화살표 ID: UpButton
> 						48 단추 페이지 위로 ID: UpPageButton
> 						49 위치 조정 위치 ID: ScrollbarThumb
> 						50 단추 페이지 아래로 ID: DownPageButton
> 						51 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					52 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						53 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						54 단추 페이지 왼쪽으로 ID: UpPageButton
> 						55 위치 조정 위치 ID: ScrollbarThumb
> 						56 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					57 위치 조정 (disabled)
> 			58 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65287
> 				59 창 ID: 59648
> 					60 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // TW[20] is an encoder error/warning reset, not a generic multi-turn // position reset. Keep it dormant until encoder compatibility and continuous // motor-off ownership are implemented and proven on the target drive. #define LMC_DIAG_D5_TW20_TEST_RESET_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 #define LMC_DIAG_ADMIN_ERROR_ID -31000 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex ID: 10000
> 						61 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							62 단추 위쪽 스크롤 화살표 ID: UpButton
> 							63 단추 페이지 위로 ID: UpPageButton
> 							64 위치 조정 위치 ID: ScrollbarThumb
> 							65 단추 페이지 아래로 ID: DownPageButton
> 							66 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						67 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							68 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							69 위치 조정 위치 ID: ScrollbarThumb
> 							70 단추 페이지 오른쪽으로 ID: DownPageButton
> 							71 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						72 위치 조정 (disabled)
> 			73 창 LMCControlCommandService Secondary Actions: Raise ID: 65286
> 				74 창 ID: 59648
> 					75 창 FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; case CommandId of 0x103C, 0x1042, 0x202B: ResponseSize := HandleRegistryCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x2023, 0x2024, 0x2022, 0x2028, 0x202E, 0x209F, 0x20A0, 0x20A2: ResponseSize := HandleAxisCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x20D2, 0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085, 0x20A4, 0x2045, 0x2051, 0x20E7: ResponseSize := HandleGroupCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x7D00, 0x7D10, 0x7D12, 0x7D13, 0x7D20, 0x7D22: ResponseSize := HandleAdminCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); else ResponseSize := -1; end_case; END_FUNCTION FUNCTION LMCControlCommandService::HandleRegistryCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR VAR objectNameLength : UDINT; objectName : ARRAY [0..255] OF CHAR; resolvedReference : UINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; case CommandId of 0x103C: if ResponseCapacity < 14 then RETURN; end_if; resolvedReference := 0; if RequestFrameSize = 88 then (pRequestFrame + 87)^ := 0; if IsClientConnected(#LMCAxis1) = 1 then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis1.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 1; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis2) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis2.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 2; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis3) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis3.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 3; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis4) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis4.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricm ID: 10000
> 						76 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							77 단추 위쪽 스크롤 화살표 ID: UpButton
> 							78 단추 페이지 위로 ID: UpPageButton
> 							79 위치 조정 위치 ID: ScrollbarThumb
> 							80 단추 페이지 아래로 ID: DownPageButton
> 							81 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						82 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							83 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							84 위치 조정 위치 ID: ScrollbarThumb
> 							85 단추 페이지 오른쪽으로 ID: DownPageButton
> 							86 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						87 위치 조정 (disabled)
> 			88 창 Comm_Network.lcn Secondary Actions: Raise ID: 65282
> 				89 창 ID: 59648
> 					90 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="Comm_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "LMCControlCommandService1" GUID = "{5E164D6C-7E45-4BA4-B0F7-F9DBCCE8C71B}" Class = "LMCControlCommandService" Position = "(930,1380)" Visualized = "false"> <Channels> <Server Name="ClassSvr"/> <Client Name="LMCAxis1"/> <Client Name="LMCAxis2"/> <Client Name="LMCAxis3"/> <Client Name="LMCAxis4"/> <Client Name="LMCAxis5"/> <Client Name="LMCAxis6"/> <Client Name="LMCAxis7"/> <Client Name="LMCAxis8"/> <Client Name="LMCAxis9"/> <Client Name="LMCRobot"/> </Channels> </Object> <Object Name = "LMCDiagnosticsService1" GUID = "{F42F0DD4-D9CC-4E5B-B073-F88FACAD14A8}" Class = "LMCDiagnosticsService" Position = "(870,900)" Visualized = "false"> <Channels> <Server Name="ClassSvr"/> <Server Name="DiagnosticsBootCounter" Value="0"/> <Client Name="InputLatch"/> <Client Name="RecorderStore"/> <Client Name="SdoAxis1"/> <Client Name="SdoAxis2"/> <Client Name="SdoAxis3"/> <Client Name="SdoAxis4"/> </Channels> </Object> <Object Name = "TCPIPServer1" GUID = "{42E82217-EDCD-47A0-BF97-FCBD9C009436}" Class = "TCPIPServer" Position = "(870,180)" Visualized = "true" Remotely = "true" CyclicTime = "1 ms" BackgroundTime = "always"> <Channels> <Server Name="Control"/> <Server Name="ErrorCode"/> <Server Name="SemaName01"/> <Server Name="SemaName02"/> <Server Name="TaskName"/> <Client Name="_MultiTask"/> <Client Name="_StdLib"/> <Client Name="_TaskObjectControl"/> <Client Name="Config" Value="0"/> <Client Name="ConnectionsPerRun" Value="1"/> <Client Name="MaxConnections" Value="2"/> <Client Name="Port" Value="4000"/> <Client Name="SigCLib"/> <Client Name="SizeOfTXBuffer"/> <Client Name="Timeout"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{BEB0D8C1-05A6-452D-879B-F50A84747DCB}" Class="_TCPIPServer"> <Channels> <Server Name="Control"/> <Server Name="ErrorCode"/> <Server Name="SemaName01"/> <Server Name="SemaName02"/> <Server Name="TaskName"/> <Client Name="_MultiTask"/> <Client Name="_StdLib"/> <Client Name="_TaskObjectControl"/> <Client Name="Config"/> <Client Name="ConnectionsPerRun" Value="1"/> <Client Name="MaxConnections" Value="2"/> <Client Name="Port"/> <Client Name="SigCLib"/> <Client Name="SizeOfTXBuffer"/> <Client Name="Timeout"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="CheckSum" GUID="{924983ED-FE4B-4B5A-BC71-6E6963A07A78}" Class="_CheckSum"> <Channels> <Server Name="ClassSvr"/> </Channels> </Object> <Object Name="StrSemaName01" GUID="{299AFE23-53C0-4268-B520-661EA498CF23}" Class="String"> <Channels> <Server Name="Data"/> <Client Name="SingleRealloc" Value="1"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{B9D2D328-1416-469A-BE13-9F6CBBB1958D}" Class="StringInternal"> <Channels> <Server Name="Data"/> <Client Name="DataBuffer"/> <Client Name="SingleRealloc"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> ID: 10000
> 						91 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							92 단추 위쪽 스크롤 화살표 ID: UpButton
> 							93 위치 조정 위치 ID: ScrollbarThumb
> 							94 단추 페이지 아래로 ID: DownPageButton
> 							95 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						96 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							97 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							98 위치 조정 위치 ID: ScrollbarThumb
> 							99 단추 페이지 오른쪽으로 ID: DownPageButton
> 							100 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						101 위치 조정 (disabled)
> 			102 창 EtherCAT_Network Secondary Actions: Raise ID: 65281
> 				103 창 ID: 59648
> 					104 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						105 단추 위쪽 스크롤 화살표 ID: UpButton
> 						106 위치 조정 위치 ID: ScrollbarThumb
> 						107 단추 페이지 아래로 ID: DownPageButton
> 						108 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					109 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						110 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						111 위치 조정 위치 ID: ScrollbarThumb
> 						112 단추 페이지 오른쪽으로 ID: DownPageButton
> 						113 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					114 위치 조정 (disabled)
> 			115 창 LMCSdoExecutor Secondary Actions: Raise ID: 65280
> 				116 창 ID: 59648
> 					117 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
> 						118 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							119 단추 위쪽 스크롤 화살표 ID: UpButton
> 							120 위치 조정 위치 ID: ScrollbarThumb
> 							121 단추 페이지 아래로 ID: DownPageButton
> 							122 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						123 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							124 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							125 위치 조정 위치 ID: ScrollbarThumb
> 							126 단추 페이지 오른쪽으로 ID: DownPageButton
> 							127 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						128 위치 조정 (disabled)
> 		129 상태 표시줄 ID: 59393
> 			130 텍스트
> 			131 텍스트 TCPMotionInterface::CyWork
> 			132 텍스트
> 			133 텍스트 Ln 26 Col 22
> 			134 텍스트
> 			135 텍스트 Offline
> 			136 텍스트
> 			137 텍스트 NUM
> 			138 텍스트
> 		139 창 xtpBarTop ID: 59419
> 			140 도구 모음 Edit
> 				19418 단추 Toggle bookmark
> 				19419 단추 (disabled) Previous bookmark
> 				19420 단추 (disabled) Next bookmark
> 				19421 단추 (disabled) Delete all bookmarks
> 				19422 단추 (disabled) Previous bookmark in this file
> 				19423 단추 (disabled) Next bookmark in this file
> 				19424 단추 Comment selected text (Ctrl+Shift+C)
> 				19425 단추 Remove comment (Ctrl+Shift+X)
> 				19426 단추 Unindent (Shift+Tab)
> 				19427 단추 Indent (Tab)
> 			151 도구 모음 Macros Manager
> 				19428 메뉴 항목 Macros
> 			153 도구 모음 Layout Manager
> 				19429 메뉴 항목 Layouts
> 			155 도구 모음 Toolbox
> 				19430 단추 DataAnalyzer
> 				19431 메뉴 항목 Toolbar Options
> 			158 도구 모음 Net Edit
> 				19432 단추 (disabled) Select
> 				19433 메뉴 항목 Toolbar Options
> 			161 도구 모음 Debug
> 				19434 단추 Go online (Alt+F6)
> 				19435 단추 Change Online Settings
> 				19436 메뉴 항목 Online Connection
> 				19437 단추 (disabled) Set Online Connection For Current Project
> 				19438 단추 (disabled) Download (F6)
> 				19439 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				19440 단추 (disabled) Download Module on the Fly
> 				19441 단추 (disabled) Save Project on PLC
> 				19442 단추 (disabled) Start (F7)
> 				19443 단추 (disabled) Reset (F8)
> 				19444 단추 Toggle breakpoint (F4)
> 				19445 단추 Create condition breakpoint
> 				19446 메뉴 항목 Toolbar Options
> 			175 도구 모음 Build
> 				19447 메뉴 항목 Target Architecture
> 				19448 단추 Build changes (F9)
> 				19449 단추 Rebuild project (Strg+F9)
> 				19450 단추 (disabled) Cancel building (Ctrl+Break)
> 				19451 단추 Link project
> 			181 도구 모음 Standard
> 				19452 단추 New project (Strg+N)
> 				19453 단추 Open a file (Strg+Shift+O)
> 				19454 단추 Close active document (Strg+F4)
> 				19455 단추 (disabled) Save file (Strg+S)
> 				19456 단추 Open project (Strg+O)
> 				19457 단추 (disabled) Save project changes (Strg+Shift+S)
> 				19458 단추 Close project
> 				19459 단추 Print
> 				19460 단추 Cut (Strg+X)
> 				19461 단추 Copy (Strg+C)
> 				19462 단추 (disabled) Paste (Strg+V)
> 				19463 메뉴 항목 (disabled) Undo (Strg+Z)
> 				19464 메뉴 항목 (disabled) Redo (Strg+Y)
> 				19465 단추 Navigate Backward (Alt+Left)
> 				19466 단추 (disabled) Navigate Forward (Alt +Right)
> 			197 메뉴 모음 Menu Bar
> 				19467 메뉴 항목 FILE
> 				19468 메뉴 항목 EDIT
> 				19469 메뉴 항목 VIEW
> 				19470 메뉴 항목 PROJECT
> 				19471 메뉴 항목 BUILD
> 				19472 메뉴 항목 DEBUG
> 				19473 메뉴 항목 ANALYZE
> 				19474 메뉴 항목 TOOLS
> 				19475 메뉴 항목 EXTRAS
> 				19476 메뉴 항목 WINDOW
> 				19477 메뉴 항목 HELP
> 		209 창 Splitter ID: 820665376
> 		210 창 Splitter ID: 820662184
> 		211 Tab Output ID: 279800176
> 			212 창 ID: 1200
> 				213 창 ID: 1200
> 					214 LIST ID: 1204
> 						215 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							216 단추 위쪽 스크롤 화살표 ID: UpButton
> 							217 단추 페이지 위로 ID: UpPageButton
> 							218 위치 조정 위치 ID: ScrollbarThumb
> 							219 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						220 목록 항목 (selectable)
> 						221 목록 항목 (selectable)
> 						222 목록 항목 (selectable)
> 						223 목록 항목 (selectable)
> 						224 목록 항목 (selectable)
> 					225 스크롤 막대 (disabled) ID: 59904
> 						226 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						227 위치 조정 위치 ID: ScrollbarThumb
> 						228 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			19478 탭 항목 (selectable) Python Script
> 			19479 탭 항목 (selectable) Debugger
> 			19480 탭 항목 (selectable) Output
> 			232 단추 Close
> 		233 창 Splitter ID: 617298272
> 		234 Tab Class View ID: 279804736
> 			235 트리 ID: 103
> 				236 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					237 단추 위쪽 스크롤 화살표 ID: UpButton
> 					238 단추 페이지 위로 ID: UpPageButton
> 					239 위치 조정 위치 ID: ScrollbarThumb
> 					240 단추 페이지 아래로 ID: DownPageButton
> 					241 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				242 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					243 콘솔 트리 (selectable) External
> 					244 콘솔 트리 (selectable) Sigmatek
> 					245 콘솔 트리 (selectable) Elmo_1
> 					246 콘솔 트리 (selectable) Elmo_2
> 					247 콘솔 트리 (selectable) Elmo_3
> 					248 콘솔 트리 (selectable) Elmo_4
> 					249 콘솔 트리 (selectable) GL_9086_1
> 					250 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					251 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					252 콘솔 트리 (selectable) LMCControlCommandService
> 						253 콘솔 트리 (selectable) Servers
> 						254 콘솔 트리 (selectable) Clients
> 						255 콘솔 트리 (selectable) Methods
> 							256 콘솔 트리 (selectable) Global
> 								257 콘솔 트리 (selectable) HandleRequest
> 								258 콘솔 트리 (selectable) ProcessAxisReference
> 							259 콘솔 트리 (selectable) Private
> 						260 콘솔 트리 (selectable) Variables
> 							261 콘솔 트리 (selectable) GroupMovePos
> 							262 콘솔 트리 (selectable) GroupKinematicReady
> 							263 콘솔 트리 (selectable) ReferenceState
> 								264 콘솔 트리 (selectable) 0..18
> 						265 콘솔 트리 (selectable) Objects
> 						266 콘솔 트리 (selectable) Dependencies
> 					267 콘솔 트리 (selectable) LMCDiagnosticsService
> 					268 콘솔 트리 (selectable) LMCEcatInputLatch
> 						269 콘솔 트리 (selectable) Servers
> 						270 콘솔 트리 (selectable) Clients
> 							271 콘솔 트리 (selectable) EcatMaster
> 							272 콘솔 트리 (selectable) Drive1

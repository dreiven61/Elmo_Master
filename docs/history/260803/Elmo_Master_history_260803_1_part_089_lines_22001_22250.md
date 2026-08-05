> 				144882 단추 New project (Strg+N)
> 				144883 단추 Open a file (Strg+Shift+O)
> 				144884 단추 Close active document (Strg+F4)
> 				144885 단추 (disabled) Save file (Strg+S)
> 				144886 단추 Open project (Strg+O)
> 				144887 단추 (disabled) Save project changes (Strg+Shift+S)
> 				144888 단추 Close project
> 				144889 단추 Print
> 				144890 단추 Cut (Strg+X)
> 				144891 단추 Copy (Strg+C)
> 				144892 단추 Paste (Strg+V)
> 				144893 메뉴 항목 Undo (Strg+Z)
> 				144894 메뉴 항목 (disabled) Redo (Strg+Y)
> 				144895 단추 Navigate Backward (Alt+Left)
> 				144896 단추 (disabled) Navigate Forward (Alt +Right)
> 			112 메뉴 모음 Menu Bar
> 				144897 메뉴 항목 FILE
> 				144898 메뉴 항목 EDIT
> 				144899 메뉴 항목 VIEW
> 				144900 메뉴 항목 PROJECT
> 				144901 메뉴 항목 BUILD
> 				144902 메뉴 항목 DEBUG
> 				144903 메뉴 항목 ANALYZE
> 				144904 메뉴 항목 TOOLS
> 				144905 메뉴 항목 EXTRAS
> 				144906 메뉴 항목 WINDOW
> 				144907 메뉴 항목 HELP
> 		124 창 Splitter ID: 404445040
> 		125 창 Splitter ID: 404445712
> 		126 Tab Output ID: 296578152
> 			127 창 ID: 1200
> 				128 창 ID: 1200
> 					129 LIST ID: 1204
> 						130 목록 항목 (selectable)
> 						131 목록 항목 (selectable)
> 						132 목록 항목 (selectable)
> 					133 스크롤 막대 (disabled) ID: 59904
> 						134 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						135 위치 조정 위치 ID: ScrollbarThumb
> 						136 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			144908 탭 항목 (selectable) Python Script
> 			144909 탭 항목 (selectable) Debugger
> 			144910 탭 항목 (selectable) Output
> 			140 단추 Close
> 		141 창 Splitter ID: 404446216
> 		142 Tab Network View ID: 296578608
> 			144430 트리 ID: 104
> 				144431 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					144432 콘솔 트리 (selectable) Comm_Network
> 					144433 콘솔 트리 (selectable) EtherCAT_Network
> 					144434 콘솔 트리 (selectable) HW_Network
> 					144435 콘솔 트리 (selectable) Motion_Network
> 			144911 탭 항목 (selectable) Lib
> 			144912 탭 항목 (selectable) File
> 			144913 탭 항목 (selectable) Global
> 			144914 탭 항목 (selectable) Net
> 			144915 탭 항목 (selectable) Class
> 			171 단추 Close
> 		172 Tab Properties ID: 296581344
> 			173 창 ID: 287643296
> 				174 TABLE Properties Window ID: 294249272
> 					144334 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						144335 단추 위쪽 스크롤 화살표 ID: UpButton
> 						144336 위치 조정 위치 ID: ScrollbarThumb
> 						144337 단추 페이지 아래로 ID: DownPageButton
> 						144338 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					144916 custom Project Version
> 					144917 custom Name
> 					144918 custom Revision
> 					144919 custom Online Connection
> 					144920 custom CodeGenerator
> 					144921 custom Networks
> 					144922 custom Server list
> 					144923 custom Target
> 					144924 custom Compiler
> 					144925 custom Directory
> 					144926 custom OPC-UA
> 					144927 custom Load all libraries
> 					144928 custom Use Unit System
> 					144929 custom Include Paths
> 					144930 custom Library Paths
> 					144931 custom Backup Includes and Loader
> 					144932 custom Ignore at Cleanup
> 					144933 custom Reencrypt Project On Close
> 					144934 custom Enable OPC UA
> 					144935 custom Enable initvalues for output parameters
> 					144936 custom Use multiple CPU core
> 					144937 custom Use Advanced-IO
> 					144938 custom AutomationML
> 					144939 custom IO Connection Manager Options
> 					144940 custom Comment
> 					144941 custom Filename
> 				175 도구 모음 ID: 59392
> 					176 단추
> 					177 단추
> 			144942 탭 항목 (selectable) Properties
> 			179 단추 Close
> 		144366 도구 설명 Network View
>
> The focused UI element is 144430 트리 ID: 104.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> IDE 선언은 저장된 소스까지 확인됐습니다. 이제 Motion_Network에 CREVIS 3채널만 연결하고, 구조 검증을 통과시킨 뒤 구현 로직과 C# 테스트까지 이어서 완료하겠습니다. PLC 다운로드나 실제 축 동작은 실행하지 않습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Motion_Network 열기",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 7868208,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=22140 chars=9977 sha256=88caf2d8b8adbb746dd508fa777a510c14b0568869e23e5d4af68e115558f220]'... 243863 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=22149 chars=9977 sha256=75cd9ec269e88b5b7459eef83796c8512720c57840f4fac779c9b5c1fa9d3c0c]'... 223267 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,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',
>       originX: -530,
>       originY: 451,
>       width: 93,
>       height: 25
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAUAFgDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDzr4s3+veAvivq8uhateWkV6638RSU7XWTnJU8Ebgw5B4FdB4K+OeuX97DYaz4ei12aQ4D2UZjuD74GQ36VsftKeHJ9V0Hw3rFjC0tzDL/AGdIqLlmD4Mf4Agj6tXe+F/DL+C9I0rSdIt7eKYwD+0byNFErS7GPLdSMsMY6YreNOftHGL2MJVI8ilI3tT1HTNMaZpJ4dsTIjqXTdGzY+VueCMnPsDWnaXNlKkTxTW7pI2xGVlIZsZwPU4BNcfaaTffYX81LyS4ijjRVleHEm1wx2FQD/CcFyDz9a1J9Ov5LqeeC3O1Qt3DG7rnz/kynXA4QjOcfOa7paLU446vQ6gajpcBiW4vLKMysUjDyqu9gcEDJ5IPGPWrN9q1jZERo1tNdCWKNoBIodQ8iJuI5PG8Hp6etc2mmXdpDNCmn/bvtdkluz70ARxu3b9x+6S+flDHrx0zck8P3jaVqdtFuSe4vbZ1nRlDlE8kFxnuNjHB9OnNck9/67nXDodaLi1+y3M1uqXP2fcHSDazblGSnXG72JFDajZLY2V1HH50d40awCNQS+/kHnjAGSfYGovDsUtvpkdtParbNB+7whBRwP415Jwevzc5znPU0tG0m8t9XdLgAabZmRrI7gdxlOTkdtg3KPZq55abf1/w5stjUh1PTGisWmmtbeS9UGCKWWPdJnHC4JDdR90nrVlJ7GQxBJbZjKrPHhlO9RjJHqBkZPvXDW3hvU0SxhnW/WOWxtbaUW0lttiaPOQ5kUtgE5BjzznpwatXug6sJLtraMExTslm29c+VLv8xuTxt80cd/KGO1J76FHZDyZLUT25jZGTejpghhjIII6g0U4xJDZ+VGoWNI9qgdgBgUUn5AjzeO8mtGKwMApxkEA/zqP7S7OWbBYnJJ70UV7NlueTfoWIrhh2Wr0Fy/HyrRRWMzWBfhu3H8K/lVyK8kx91PyoorlmdUCzHeSf3U/I1KL2T+6n5GiisGaocL2T+6n5Gl+2yf3U/I0UVJRW1PUZodLvp0WMtDbySgEHBKqSAeenFFFFAH//2Q==',
>       originX: -530,
>       originY: 451,
>       width: 88,
>       height: 20
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise\n' +
>       '\t\t144366 도구 설명 Network View\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
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
>       '\t\t\t2753 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65283\n' +
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
>       '\t\t\t16 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281\n' +
>       '\t\t\t\t17 창 ID: 59648\n' +
>       '\t\t\t\t\t18 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaTyp'... 12473 more characters,
>     focused_element: '144430 트리 ID: 104'
>   }
> }
> ```
>
> Image output: image/jpeg
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
>   "title": "Motion_Network 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1265,y:222}); await globalThis.sky.click({window:globalThis.targetWindow,x:1265,y:222}); await new Promise(r=>setTimeout(r,1200)); globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 7868208,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=22245 chars=9977 sha256=a429bcb45353367824443e6bdd190064a8ac50f2a6bd1e771c57c677df13f7b4]'... 277719 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },

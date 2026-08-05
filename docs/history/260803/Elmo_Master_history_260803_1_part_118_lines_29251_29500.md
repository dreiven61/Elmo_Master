>     }
>   ],
>   accessibility: null
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
>   "title": "Open LASAL View menu",
>   "code": "var v1 = await globalThis.sky.click({window:globalThis.targetWindow,x:190,y:42,screenshotId:\"screenshot-1\"}); nodeRepl.write(v1);"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect LASAL View menu",
>   "code": "var ideState2 = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(ideState2);"
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
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=29298 chars=9977 sha256=762e5f2b63d713335e2a99ef9a4d0a9ddc673c955fb1239778cc34a7ef81df11]'... 206327 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=29307 chars=9977 sha256=96f99d3d9689c96456d549d9493591057f2a96d7514190ba3b34b8ef578b4fd5]'... 191583 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=29316 chars=9977 sha256=dacbea74741929e14a7f25e1aa3c8d8f3306b12cefe03e557c661c0f5e999c96]'... 8159 more characters,
>       originX: -1708,
>       originY: 95,
>       width: 196,
>       height: 299
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAEAMQDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3XXfD2nXl4ZJ4dzetZv8Awiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAddplhb29mkcSYUdBRRRQB//9k=',
>       originX: -1704,
>       originY: 394,
>       width: 196,
>       height: 4
>     },
>     {
>       id: 'screenshot-4',
>       zIndex: 4,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAEnAAQDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD2Lwzqs93pEM0v3m96K8ytvidpHh2BNOmt7q4mjALtEBhSf4eSOen50U7gdDonhTQ9Z0u3utT02C4uNoTzGByQOnSitzwYP+JBB/n0ooAXwZ/yAIP8+lFHgz/kAQf59KKAE8GH/iQQf59KKw/h1r8d94ZieNYXMbtG5hmEiZGPusByMEdhRWE8TSg+WT1+ZpGlKSukXvBWnr/wjtsJbieZxnMjlcn8gAPwAopng/wuZdCgf+3NaTP8KTIB/wCgUU5UKcndxEqkkrJnReCP+Ret/wDPpRXz7qPjTxh8Pjb6Rp18moWckQuYpLyLfKoYkbS2RnBU8+9FfVUOFsXiKaq0pRcXqtbfoYOU19h/caQ+FniPx7FFrGpX9ppRCCCG0AMpSNeRlgQCSWaivcPBH/IvW/8An0orGlxLj6MFTpSUYrRKy0++7E6V3dth4I/5F63/AM+lFHgj/kXrf/PpRXgGpk/DfxLouo+FoJrTU7V0DFTukCEEAcEHBFFfNXxbttOh8R2phKxyyWcbziNRy5Lcn32haK+4wfCVLF0IYiNVpSV7ct7fO6v9xlL2ie6+/wD4J734F+G3hZtAjkv9MTUbqRi8lzenzZHOB1J+nQUV2Hgj/kXrf/PpRXyX9oYr/n7L72N0oPeKDwR/yL1v/n0oo8Ef8i9b/wCfSiuQ0POPC/xd0TRNJjsfE9te6XqUZ+aAxmUFSBhlZeCD/Q0V5B8Qrj/hI9Zt5tDsJtSt7a1S2e4giZ0MgLMQCBg4Diiv0PB8N4DEUIVat4ykrtcy0+9X+85pzSdvaJfL/gH1D8P7aC08MW0NrDHDEM4SNQqjp2FFS+CP+Ret/wDPpRX56227s6Q8Ef8AIvW/+fSisHwdrmoR6DAqeFtalA/iSWzwfznBordYab6r/wACj/mK5veCP+Ret/8APpRR4I/5F63/AM+lFc4w8Ef8i9b/AOfSijwR/wAi9b/59KKADwR/yL1v/n0oo8Ef8i9b/wCfSigA8Ef8i9b/AOfSijwR/wAi9b/59KKADwR/yL1v/n0ooooA/9k=',
>       originX: -1512,
>       originY: 99,
>       width: 4,
>       height: 295
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise\n' +
>       '\t\t227891 창\n' +
>       '\t\t227892 창\n' +
>       '\t\t227893 메뉴 View\n' +
>       '\t\t\t227894 메뉴 항목 Output Pane\n' +
>       '\t\t\t227895 메뉴 항목 Properties Window\n' +
>       '\t\t\t227896 메뉴 항목 Debugger Pane\n' +
>       '\t\t\t227897 메뉴 항목 Python Pane\n' +
>       '\t\t\t227898 메뉴 항목 DataAnalyzer Pane\n' +
>       '\t\t\t227899 메뉴 항목 Drive Pane\n' +
>       '\t\t\t227900 메뉴 항목 Multimaster Pane\n' +
>       '\t\t\t227901 메뉴 항목 Code Analysis\n' +
>       '\t\t\t227902 메뉴 항목 Trees\n' +
>       '\t\t\t227903 메뉴 항목 Toolbars\n' +
>       '\t\t\t227904 메뉴 항목 Status Bar\n' +
>       '\t\t\t227905 메뉴 항목 Layout\n' +
>       '\t\t\t227906 메뉴 항목 Graphical User Interface\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
>       '\t\t\t155298 창 Motion_Network Secondary Actions: Raise ID: 65284\n' +
>       '\t\t\t\t155299 창 ID: 59648\n' +
>       '\t\t\t\t\t162758 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t162759 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t162760 단추 페이지 위로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t162761 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t162762 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t162763 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t162764 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t162765 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t162766 단추 페이지 왼쪽으로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t162767 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t162769 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t162770 위치 조정 (disabled)\n' +
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
>       '\t\t\t\t\t\t\t2765 단추 오른쪽 스크롤 화살표 ID: Dow'... 15024 more characters,
>     focused_element: '155299 창 ID: 59648'
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect LASAL tree panels",
>   "code": "var v2 = await globalThis.sky.click({window:globalThis.targetWindow,x:229,y:241,screenshotId:\"screenshot-1\"}); nodeRepl.write(v2);"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect LASAL tree submenu",
>   "code": "var ideState3 = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(ideState3);"
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
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=29454 chars=9977 sha256=a429bcb45353367824443e6bdd190064a8ac50f2a6bd1e771c57c677df13f7b4]'... 211279 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=29463 chars=9977 sha256=39d04f41cdd7755c82b75fe9e696f47ab403f49ec5e38e3efa3faacb26a85d60]'... 196579 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=29472 chars=9977 sha256=36102c74e8d34d748c5e0f68a7f05ffe922ace431728a1a44f6f8e332d1385a7]'... 8587 more characters,
>       originX: -1708,
>       originY: 95,
>       width: 196,
>       height: 299
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAEAMQDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3XXfD2nXl4ZJ4dzetZv8Awiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70q+GdLtyJooMSR/Op9CORRRTW4Hnfx+/5HGz/AOvBP/RklFFFfc5b/usPQ+AzP/e6nqf/2Q==',
>       originX: -1704,
>       originY: 394,
>       width: 196,
>       height: 4
>     },
>     {
>       id: 'screenshot-4',
>       zIndex: 4,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAEnAAQDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD2Lwzqs93pEM0v3m96K8ytvidpHh2BNOmt7q4mjALtEBhSf4eSOen50U7gdDonhTQ9Z0u3utT02C4uNoTzGByQOnSitzwYP+JBB/n0ooAXwZ/yAIP8+lFHgz/kAQf59KKAE8GH/iQQf59KKw/h1r8d94ZieNYXMbtG5hmEiZGPusByMEdhRWE8TSg+WT1+ZpGlKSukXvBWnr/wjtsJbieZxnMjlcn8gAPwAopng/wuZdCgf+3NaTP8KTIB/wCgUU5UKcndxEqkkrJnReCP+Ret/wDPpRXz7qPjTxh8Pjb6Rp18moWckQuYpLyLfKoYkbS2RnBU8+9FfVUOFsXiKaq0pRcXqtbfoYOU19h/caQ+FniPx7FFrGpX9ppRCCCG0AMpSNeRlgQCSWaivcPBH/IvW/8An0orGlxLj6MFTpSUYrRKy0++7E6V3dth4I/5F63/AM+lFHgj/kXrf/PpRXgGpk/DfxLouo+FoJrTU7V0DFTukCEEAcEHBFFfNXxbttOh8R2phKxyyWcbziNRy5Lcn32haK+4wfCVLF0IYiNVpSV7ct7fO6v9xlL2ie6+/wD4J734F+G3hZtAjkv9MTUbqRi8lzenzZHOB1J+nQUV2Hgj/kXrf/PpRXyX9oYr/n7L72N0oPeKDwR/yL1v/n0oo8Ef8i9b/wCfSiuQ0M3xx4vsvBWqQaZBognje3WcFLoxBcsy42hT/d6+9Fcd8fv+Rxs/+vBP/RklFfWYLLcNVoQnON213f8AmfH47NMVSxE4QnZJ9l/kHx+/5HGz/wCvBP8A0ZJRXvtFcGGzr2FKNPkvbz/4B6GKyL6xVlV9pa/l/wAEKKKK8I+gPAvj9/yONn/14J/6Mkoo+P3/ACONn/14J/6Mkor7rLf91h6HwGZ/73U9TvvH/wAO/wDhLtZhv/7U+x+XbrBs+z+ZnDM2c7h/e6e1Fd/RXydPMsTSioQlZLyX+R9fVyvC1Zuc4Xb83/mFFFFcJ6AUUUUAeQ/F/wAYa74f8S21ppF99nt3tFlZfJjfLF3GcspPQCiiivssBhaE8NCUoJu3ZHxGY4uvDEzjGbSv3Z//2Q==',
>       originX: -1512,
>       originY: 99,
>       width: 4,
>       height: 295
>     },
>     {
>       id: 'screenshot-5',
>       zIndex: 5,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=29499 chars=9976 sha256=75c1106bdb68d499aacb44649e14ef7c0f2ad368964ff9781be310cd90664ab5]',
>       originX: -1513,

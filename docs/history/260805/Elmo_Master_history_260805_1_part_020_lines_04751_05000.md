>       '\t\t\t\t\t5953 콘솔 트리 (selectable) Elmo_3\n' +
>       '\t\t\t\t\t5954 콘솔 트리 (selectable) Elmo_4\n' +
>       '\t\t\t\t\t5955 콘솔 트리 (selectable) GL_9086_1\n' +
>       '\t\t\t\t\t5956 콘솔 트리 (selectable) GL_9086_1_Slot00\n' +
>       '\t\t\t\t\t5957 콘솔 트리 (selectable) GL_9086_1_Slot01\n' +
>       '\t\t\t\t\t5958 콘솔 트리 (selectable) LMCControlCommandService\n' +
>       '\t\t\t\t\t5959 콘솔 트리 (selectable) LMCDiagnosticsService\n' +
>       '\t\t\t\t\t\t25512 콘솔 트리 (selectable) Servers\n' +
>       '\t\t\t\t\t\t25513 콘솔 트리 (selectable) Clients\n' +
>       '\t\t\t\t\t\t25514 콘솔 트리 (selectable) Methods\n' +
>       '\t\t\t\t\t\t\t26174 콘솔 트리 (selectable) Global\n' +
>       '\t\t\t\t\t\t\t26175 콘솔 트리 (selectable) Private\n' +
>       '\t\t\t\t\t\t\t\t26920 콘솔 트리 (selectable) LMCDiagnosticsService\n' +
>       '\t\t\t\t\t\t\t\t26921 콘솔 트리 (selectable) IsSdoReadReady\n' +
>       '\t\t\t\t\t\t\t\t26922 콘솔 트리 (selectable) GetSdoWritePolicyDetail\n' +
>       '\t\t\t\t\t\t\t\t26923 콘솔 트리 (selectable) BuildCatalogEntry\n' +
>       '\t\t\t\t\t\t\t\t26924 콘솔 트리 (selectable) HandleEtherCATTopologyIoRequest\n' +
>       '\t\t\t\t\t\t\t\t26925 콘솔 트리 (selectable) HandleAxisDs402HomeStart\n' +
>       '\t\t\t\t\t\t\t\t26926 콘솔 트리 (selectable) HandleAxisDs402HomeOutcome\n' +
>       '\t\t\t\t\t\t\t\t26927 콘솔 트리 (selectable) ProcessAxisDs402Home\n' +
>       '\t\t\t\t\t\t\t\t26928'... 927 more characters,
>     focused_element: '98 트리 ID: 103'
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
>   "code": "await sky.click({window: targetWindow, x: 1270, y: 178, mouse_button: 'right', screenshotId: 'screenshot-1'}); state = await sky.get_window_state({window: targetWindow, include_text: true, include_screenshot: true}); nodeRepl.write(state);",
>   "title": "Private 메서드 그룹 메뉴 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 211750980,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=4803 chars=9977 sha256=78a0a86962f4f03eb4d8abf96da423da5a5ba21e020185db411cc7d2c9c3765d]'... 287695 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=4812 chars=9977 sha256=75cd9ec269e88b5b7459eef83796c8512720c57840f4fac779c9b5c1fa9d3c0c]'... 266323 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=4821 chars=6300 sha256=82053b96408655509a6c928ff7860298b002e593017e9ac777ecaf4565fcdea7]',
>       originX: -605,
>       originY: 224,
>       width: 169,
>       height: 120
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAEAKkDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDr5dFsrqLW5ZotzrqunID7GGzz/M1w9nsm+LV54SaKMaZpd+k0TgfvX8xSxR26FQXOMAHgZJ7lFVW+P7v/AG0X/Lm/l+jM7XrDTdKm+GSf2Tp16fENtbWly17brKYQNg3RZ+63709cjheOuexufBGjx+A5PEiC8GqBvt+ftcnleZ5mceVnZtx8uMdO+eaKKct/m/zRVT4mijqGl2n9hXN35X73VdF1K6uBklQ8O1Y9g/hwrke4AzyM1Q1IW9x8TPFuhNY2qQaZoUl1FcRptmkJiQ7HbOGQGQkDA5A59Sis18C9H+ppbVev/wAiT/Aq7HxHi1DUNbtre3bTGWKGGzQrGS6nLtuLEsACByBhjweCJPgxoul/EHw9qOqazYrazQ6g8KJps8tqmQikuQj5LNuOTnnA4ooq2t/T9TCPwr1/QzPGccPgfwF4Q1fRoVkurxhZSi7ZplKyLvLDJzuBTjnABIwe2P8AGDxC3w88b2Gl6RpunXVvHp0Jja+R3aMkupI2soyQBk4zxRRSjuvX/MqW/wAv1PabDw1pjyeJCYPuTDb7f6NEf5mvNviGV8LSeDI9LijI8RxRWVyZV3GIDYBJF0w+JW67hwvHXJRSW9vT8pFR6ej/APbTqpPC+l2v9swQwYjg1zT4EGeibLTj9TWF8StH0zSfB/ijWl0+1u59L1ZWigu4xJC+6OJCHXuMSEjBByBz1BKKp/1/5ITbWP8AXSRzniC+itT8PdEt9M02Ky8T29tLcBIAptWkdcmDBwpBbILBiCAee+h8WZIvD+s+J7S0tLaWC30y11PZMpYSP9oWIxvgjMZXqOp9ccUUVMun+JiX6I3/AIbaBptzd6C7Wsca3FnfXBjiQIiFntW2qAOFG8ge3UnrWz8JdOstZ0bVY7u1iA07VLnT4igwWjjb5S2c5bnnGB7UUVT2j6fqD2+b/U6668K6SJBi37VzmkeGtMbxPr8bQfKn2faPqhooqRm3D4d0621SweKHDeeBn/tnJ/hXd/Z4/wC7RRTYH//Z',
>       originX: -601,
>       originY: 344,
>       width: 169,
>       height: 4
>     },
>     {
>       id: 'screenshot-4',
>       zIndex: 4,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAB0AAQDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD17wHqk9x4V0936+Uo6/7IorN+HaMfCVgRK4/drwAP7o9qKbAf8Ov+RQ0//rmv/oIoo+HX/Ioaf/1zX/0EUUmAfDr/AJFDT/8Armv/AKCKKPh1/wAihp//AFzX/wBBFFDAPh1/yKGn/wDXNf8A0EUUfDr/AJFDT/8Armv/AKCKKGAfDr/kUNP/AOua/wDoIoo+HX/Ioaf/ANc1/wDQRRQwD4df8ihp/wD1zX/0EUUfDr/kUNP/AOua/wDoIooYB8Ov+RQ0/wD65r/6CKKPh1/yKGn/APXNf/QRRQwD4df8ihp//XNf/QRRRRQwP//Z',
>       originX: -436,
>       originY: 228,
>       width: 4,
>       height: 116
>     }
>   ],
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
>       '\t\t\t10182 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65280\n' +
>       '\t\t\t\t10183 창 ID: 59648\n' +
>       '\t\t\t\t\t10184 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // TW[20] is an encoder error/warning reset, not a generic multi-turn // position reset. Keep it dormant until encoder compatibility and continuous // motor-off ownership are implemented and proven on the target drive. #define LMC_DIAG_D5_TW20_TEST_RESET_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 #define LMC_DIAG_ADMIN_ERROR_ID -31000 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex ID: 10000\n' +
>       '\t\t\t\t\t\t10185 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t10186 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t10187 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t10188 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t10189 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t10190 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t10191 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t10192 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t10193 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t10194 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t10195 위치 조정 (disabled)\n' +
>       '\t\t2 상태 표시줄 ID: 59393\n' +
>       '\t\t\t3 텍스트 \n' +
>       '\t\t\t4 텍스트\n' +
>       '\t\t\t5 텍스트\n' +
>       '\t\t\t6 텍스트\n' +
>       '\t\t\t7 텍스트 \n' +
>       '\t\t\t8 텍스트 Offline\n' +
>       '\t\t\t9 텍스트\n' +
>       '\t\t\t10 텍스트 NUM\n' +
>       '\t\t\t11 텍스트\n' +
>       '\t\t12 창 xtpBarTop ID: 59419\n' +
>       '\t\t\t13 도구 모음 Edit\n' +
>       '\t\t\t\t29944 단추 Toggle bookmark\n' +
>       '\t\t\t\t29945 단추 (disabled) Previous bookmark\n' +
>       '\t\t\t\t29946 단추 (disabled) Next bookmark\n' +
>       '\t\t\t\t29947 단추 (disabled) Delete all bookmarks\n' +
>       '\t\t\t\t29948 단추 (disabled) Previous bookmark in this file\n' +
>       '\t\t\t\t29949 단추 (disabled) Next bookmark in this file\n' +
>       '\t\t\t\t29950 단추 (disabled) Comment selected text (Ctrl+Shift+C)\n' +
>       '\t\t\t\t29951 단추 (disabled) Remove comment (Ctrl+Shift+X)\n' +
>       '\t\t\t\t29952 단추 (disabled) Unindent (Shift+Tab)\n' +
>       '\t\t\t\t29953 단추 (disabled) Indent (Tab)\n' +
>       '\t\t\t24 도구 모음 Macros Manager\n' +
>       '\t\t\t\t29954 메뉴 항목 Macros\n' +
>       '\t\t\t26 도구 모음 Layout Manager\n' +
>       '\t\t\t\t29955 메뉴 항목 Layouts\n' +
>       '\t\t\t28 도구 모음 Toolbox\n' +
>       '\t\t\t\t29956 단추 DataAnalyzer\n' +
>       '\t\t\t\t29957 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t31 도구 모음 Net Edit\n' +
>       '\t\t\t\t29958 단추 (disabled) Select\n' +
>       '\t\t\t\t29959 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t34 도구 모음 Debug\n' +
>       '\t\t\t\t29960 단추 Go online (Alt+F6)\n' +
>       '\t\t\t\t29961 단추 Change Online Settings\n' +
>       '\t\t\t\t29962 메뉴 항목 Online Connection\n' +
>       '\t\t\t\t29963 단추 (disabled) Set Online Connection For Current Project\n' +
>       '\t\t\t\t29964 단추 (disabled) Download (F6)\n' +
>       '\t\t\t\t29965 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)\n' +
>       '\t\t\t\t29966 단추 (disabled) Download Module on the Fly\n' +
>       '\t\t\t\t29967 단추 (disabled) Save Project on PLC\n' +
>       '\t\t\t\t29968 단추 (disabled) Start (F7)\n' +
>       '\t\t\t\t29969 단추 (disabled) Reset (F8)\n' +
>       '\t\t\t\t29970 단추 Toggle breakpoint (F4)\n' +
>       '\t\t\t\t29971 단추 Create condition breakpoint\n' +
>       '\t\t\t\t29972 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t48 도구 모음 Build\n' +
>       '\t\t\t\t29973 메뉴 항목 Target Architecture\n' +
>       '\t\t\t\t29974 단추 Build changes (F9)\n' +
>       '\t\t\t\t29975 단추 Rebuild project (Strg+F9)\n' +
>       '\t\t\t\t29976 단추 (disabled) Cancel building (Ctrl+Break)\n' +
>       '\t\t\t\t29977 단추 Link project\n' +
>       '\t\t\t54 도구 모음 Standard\n' +
>       '\t\t\t\t29978 단추 New project (Strg+N)\n' +
>       '\t\t\t\t29979 단추 Open a file (Strg+Shift+O)\n' +
>       '\t\t\t\t29980 단추 Close active document (Strg+F4)\n' +
>       '\t\t\t\t29981 단추 (disabled) Save file (Strg+S)\n' +
>       '\t\t\t\t29982 단추 Open project (Strg+O)\n' +
>       '\t\t\t\t29983 단추 (disabled) Save project changes (Strg+Shift+S)\n' +
>       '\t\t\t\t29984 단추 Close project\n' +
>       '\t\t\t\t29985 단추 Print\n' +
>       '\t\t\t\t29986 단추 Cut (Strg+X)\n' +
>       '\t\t\t\t29987 단추 Copy (Strg+C)\n' +
>       '\t\t\t\t29988 단추 Paste (Strg+V)\n' +
>       '\t\t\t\t29989 메뉴 항목 Undo (Strg+Z)\n' +
>       '\t\t\t\t29990 메뉴 항목 (disabled) Redo (Strg+Y)\n' +
>       '\t\t\t\t29991 단추 (disabled) Navigate Backward (Alt+Left)\n' +
>       '\t\t\t\t29992 단추 (disabled) Navigate Forward (Alt +Right)\n' +
>       '\t\t\t70 메뉴 모음 Menu Bar\n' +
>       '\t\t\t\t29993 메뉴 항목 FILE\n' +
>       '\t\t\t\t29994 메뉴 항목 EDIT\n' +
>       '\t\t\t\t29995 메뉴 항목 VIEW\n' +
>       '\t\t\t\t29996 메뉴 항목 PROJECT\n' +
>       '\t\t\t\t29997 메뉴 항목 BUILD\n' +
>       '\t\t\t\t29998 메뉴 항목 DEBUG\n' +
>       '\t\t\t\t29999 메뉴 항목 ANALYZE\n' +
>       '\t\t\t\t30000 메뉴 항목 TOOLS\n' +
>       '\t\t\t\t30001 메뉴 항목 EXTRAS\n' +
>       '\t\t\t\t30002 메뉴 항목 WINDOW\n' +
>       '\t\t\t\t30003 메뉴 항목 HELP\n' +
>       '\t\t82 창 Splitter ID: 306256112\n' +
>       '\t\t83 창 Splitter ID: 306255440\n' +
>       '\t\t84 Tab Output ID: 306064200\n' +
>       '\t\t\t85 창 ID: 1200\n' +
>       '\t\t\t\t86 창 ID: 1200\n' +
>       '\t\t\t\t\t87 LIST ID: 1201\n' +
>       '\t\t\t\t\t\t5235 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t5236 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t5237 단추 페이지 위로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t\t5238 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t5239 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t5871 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t5939 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t6030 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t6031 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t6032 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t6033 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t6034 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t\t6035 목록 항목 (selectable)\n' +
>       '\t\t\t\t\t88 스크롤 막대 ID: 59904\n' +
>       '\t\t\t\t\t\t89 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t90 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t91 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t30004 탭 항목 (selectable) Python Script\n' +
>       '\t\t\t30005 탭 항목 (selectable) Debugger\n' +
>       '\t\t\t30006 탭 항목 (selectable) Output\n' +
>       '\t\t\t95 단추 Close\n' +
>       '\t\t96 창 Splitter ID: 306256448\n' +
>       '\t\t97 Tab Class View ID: 306057360\n' +
>       '\t\t\t98 트리 ID: 103\n' +
>       '\t\t\t\t5943 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t5944 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t25511 단추 페이지 위로 ID: UpPageButton\n' +
>       '\t\t\t\t\t5945 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t5946 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t5947 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t26914 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t26915 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t26916 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t26917 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t26918 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t26919 위치 조정 (disabled)\n' +
>       '\t\t\t\t5948 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis\n' +
>       '\t\t\t\t\t5949 콘솔 트리 (selectable) External\n' +
>       '\t\t\t\t\t5950 콘솔 트리 (selectable) Sigmatek\n' +
>       '\t\t\t\t\t5951 콘솔 트리 (selectable) Elmo_1\n' +
>       '\t\t\t\t\t5952 콘솔 트리 (selectable) Elmo_2\n' +
>       '\t\t\t\t\t5953 콘솔 트리 (selectable) Elmo_3\n' +
>       '\t\t\t\t\t5954 콘솔 트리 (selectable) Elmo_4\n' +
>       '\t\t\t\t\t5955 콘솔 트리 (selectable) GL_9086_1\n' +
>       '\t\t\t\t\t5956 콘솔 트리 (selectable) GL_9086_1_Slot00\n' +
>       '\t\t\t\t\t5957 콘솔 트리 (selectable) GL_9086_1_Slot01\n' +
>       '\t\t\t\t\t5958 콘솔 트리 (selectable) LMCControlCommandService\n' +
>       '\t\t\t\t\t5959 콘솔 트리 (selectable) LMCDiagnosticsService\n' +
>       '\t\t\t\t\t\t25512 콘솔 트리 (selectable) Servers\n' +
>       '\t\t\t\t\t\t25513 콘솔 트리 (selectable) Clients\n' +
>       '\t\t\t\t\t\t25514 콘솔 트리 (selectable) Methods\n' +

>       '\t\t\t\t\t26836 custom Revision\n' +
>       '\t\t\t\t\t26837 custom GUID\n' +
>       '\t\t\t\t\t26838 custom Task Settings\n' +
>       '\t\t\t\t\t26839 custom Sigmatek\n' +
>       '\t\t\t\t\t26840 custom OSInterface\n' +
>       '\t\t\t\t\t26841 custom IconPath\n' +
>       '\t\t\t\t\t26842 c'... 361 more characters,
>     focused_element: '98 트리 ID: 103'
>   }
> }
> ```
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
>   "code": "nodeRepl.write(state.accessibility.tree.split('\\n').filter(x => x.includes('HandleAxisDs402Home') || x.includes('Private')).join('\\n'));",
>   "title": "Home 메서드 항목 확인"
> }
> ```
>
> ```text
> 							26175 콘솔 트리 (selectable) Private
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
>   "code": "await sky.click({window: targetWindow, x: 1320, y: 318, mouse_button: 'right', screenshotId: 'screenshot-1'}); state = await sky.get_window_state({window: targetWindow, include_text: true, include_screenshot: true}); nodeRepl.write(state);",
>   "title": "Outcome 메서드 메뉴 확인"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=4308 chars=9977 sha256=78a0a86962f4f03eb4d8abf96da423da5a5ba21e020185db411cc7d2c9c3765d]'... 301239 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=4317 chars=9977 sha256=75cd9ec269e88b5b7459eef83796c8512720c57840f4fac779c9b5c1fa9d3c0c]'... 280319 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=4326 chars=9977 sha256=78759ed026ad3dd5b5ae706975046d6cda40eac745bd3096a7c4cad2e0fb81b0]'... 935 more characters,
>       originX: -555,
>       originY: 364,
>       width: 182,
>       height: 192
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAEALYDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDivif4oWz8M+B76HQdDEepadNM9kLdltoZC4G6JA42kc4OTjJx1r6F8L+HNNvfDOkXVxDumns4ZXPqzICf1NFFUvhl6/rIl/EvT9Imn/wiek/8+9H/AAiek/8APvRRUlB/wiek/wDPvR/wiek/8+9FFAB/wiek/wDPvR/wiek/8+9FFAB/wiek/wDPvR/wiek/8+9FFAB/wiek/wDPvR/wiek/8+9FFAB/wiek/wDPvR/wiek/8+9FFAGBZ+GNLPibWEMHyr5OB9UrfHhPScD/AEeiimwD/hE9J/596P8AhE9J/wCfeiikAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAdB4d0ezsYmW2j2g0UUUAf/2Q==',
>       originX: -551,
>       originY: 556,
>       width: 182,
>       height: 4
>     },
>     {
>       id: 'screenshot-4',
>       zIndex: 4,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAC8AAQDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3Xwtq9xeaNDNKfmbrzRWb4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQAeCP+Ret/8APpRR4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQBQ8FyOPD8Hzt+f0orz3w94l1Sy0a1S4uhau6b9n9ltcqQe6vHLgrxxnn1xnFFdapSauov7mYOcU7OS+86fwl4W8Pz6JDJPoelSSMclntI2J/Eiiuo8ExofD1v8AIv5fSisOddjWz7jvBH/IvW/+fSijwR/yL1v/AJ9KKzKDwR/yL1v/AJ9KKPBH/IvW/wDn0ooAb4JkQeHrf51/P6UU7wR/yL1v/n0ooAPBH/IvW/8An0oo8Ef8i9b/AOfSigA8Ef8AIvW/+fSijwR/yL1v/n0ooAb4Jcf8I9b8N/3yfaineCP+Ret/8+lFAH//2Q==',
>       originX: -373,
>       originY: 368,
>       width: 4,
>       height: 188
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
>       '\t\t\t\t27850 단추 Toggle bookmark\n' +
>       '\t\t\t\t27851 단추 (disabled) Previous bookmark\n' +
>       '\t\t\t\t27852 단추 (disabled) Next bookmark\n' +
>       '\t\t\t\t27853 단추 (disabled) Delete all bookmarks\n' +
>       '\t\t\t\t27854 단추 (disabled) Previous bookmark in this file\n' +
>       '\t\t\t\t27855 단추 (disabled) Next bookmark in this file\n' +
>       '\t\t\t\t27856 단추 (disabled) Comment selected text (Ctrl+Shift+C)\n' +
>       '\t\t\t\t27857 단추 (disabled) Remove comment (Ctrl+Shift+X)\n' +
>       '\t\t\t\t27858 단추 (disabled) Unindent (Shift+Tab)\n' +
>       '\t\t\t\t27859 단추 (disabled) Indent (Tab)\n' +
>       '\t\t\t24 도구 모음 Macros Manager\n' +
>       '\t\t\t\t27860 메뉴 항목 Macros\n' +
>       '\t\t\t26 도구 모음 Layout Manager\n' +
>       '\t\t\t\t27861 메뉴 항목 Layouts\n' +
>       '\t\t\t28 도구 모음 Toolbox\n' +
>       '\t\t\t\t27862 단추 DataAnalyzer\n' +
>       '\t\t\t\t27863 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t31 도구 모음 Net Edit\n' +
>       '\t\t\t\t27864 단추 (disabled) Select\n' +
>       '\t\t\t\t27865 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t34 도구 모음 Debug\n' +
>       '\t\t\t\t27866 단추 Go online (Alt+F6)\n' +
>       '\t\t\t\t27867 단추 Change Online Settings\n' +
>       '\t\t\t\t27868 메뉴 항목 Online Connection\n' +
>       '\t\t\t\t27869 단추 (disabled) Set Online Connection For Current Project\n' +
>       '\t\t\t\t27870 단추 (disabled) Download (F6)\n' +
>       '\t\t\t\t27871 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)\n' +
>       '\t\t\t\t27872 단추 (disabled) Download Module on the Fly\n' +
>       '\t\t\t\t27873 단추 (disabled) Save Project on PLC\n' +
>       '\t\t\t\t27874 단추 (disabled) Start (F7)\n' +
>       '\t\t\t\t27875 단추 (disabled) Reset (F8)\n' +
>       '\t\t\t\t27876 단추 Toggle breakpoint (F4)\n' +
>       '\t\t\t\t27877 단추 Create condition breakpoint\n' +
>       '\t\t\t\t27878 메뉴 항목 Toolbar Options\n' +
>       '\t\t\t48 도구 모음 Build\n' +
>       '\t\t\t\t27879 메뉴 항목 Target Architecture\n' +
>       '\t\t\t\t27880 단추 Build changes (F9)\n' +
>       '\t\t\t\t27881 단추 Rebuild project (Strg+F9)\n' +
>       '\t\t\t\t27882 단추 (disabled) Cancel building (Ctrl+Break)\n' +
>       '\t\t\t\t27883 단추 Link project\n' +
>       '\t\t\t54 도구 모음 Standard\n' +
>       '\t\t\t\t27884 단추 New project (Strg+N)\n' +
>       '\t\t\t\t27885 단추 Open a file (Strg+Shift+O)\n' +
>       '\t\t\t\t27886 단추 Close active document (Strg+F4)\n' +
>       '\t\t\t\t27887 단추 (disabled) Save file (Strg+S)\n' +
>       '\t\t\t\t27888 단추 Open project (Strg+O)\n' +
>       '\t\t\t\t27889 단추 (disabled) Save project changes (Strg+Shift+S)\n' +
>       '\t\t\t\t27890 단추 Close project\n' +
>       '\t\t\t\t27891 단추 Print\n' +
>       '\t\t\t\t27892 단추 Cut (Strg+X)\n' +
>       '\t\t\t\t27893 단추 Copy (Strg+C)\n' +
>       '\t\t\t\t27894 단추 Paste (Strg+V)\n' +
>       '\t\t\t\t27895 메뉴 항목 (disabled) Undo (Strg+Z)\n' +
>       '\t\t\t\t27896 메뉴 항목 (disabled) Redo (Strg+Y)\n' +
>       '\t\t\t\t27897 단추 (disabled) Navigate Backward (Alt+Left)\n' +
>       '\t\t\t\t27898 단추 (disabled) Navigate Forward (Alt +Right)\n' +
>       '\t\t\t70 메뉴 모음 Menu Bar\n' +
>       '\t\t\t\t27899 메뉴 항목 FILE\n' +
>       '\t\t\t\t27900 메뉴 항목 EDIT\n' +
>       '\t\t\t\t27901 메뉴 항목 VIEW\n' +
>       '\t\t\t\t27902 메뉴 항목 PROJECT\n' +
>       '\t\t\t\t27903 메뉴 항목 BUILD\n' +
>       '\t\t\t\t27904 메뉴 항목 DEBUG\n' +
>       '\t\t\t\t27905 메뉴 항목 ANALYZE\n' +
>       '\t\t\t\t27906 메뉴 항목 TOOLS\n' +
>       '\t\t\t\t27907 메뉴 항목 EXTRAS\n' +
>       '\t\t\t\t27908 메뉴 항목 WINDOW\n' +
>       '\t\t\t\t27909 메뉴 항목 HELP\n' +
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
>       '\t\t\t27910 탭 항목 (selectable) Python Script\n' +
>       '\t\t\t27911 탭 항목 (selectable) Debugger\n' +
>       '\t\t\t27912 탭 항목 (selectable) Output\n' +
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

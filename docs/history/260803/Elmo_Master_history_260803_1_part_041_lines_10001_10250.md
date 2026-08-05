> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 메서드 목록 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state); if(state?.screenshot) await nodeRepl.emitImage(state.screenshot);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1120864,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=10025 chars=9977 sha256=75e3bc9a120457491e63cdcdb7c5c0b638830087046928e4d78fc6caaf9e384d]'... 423403 more characters,
>       originX: -8,
>       originY: -8,
>       width: 1920,
>       height: 1040
>     }
>   ],
>   accessibility: null
> }
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
>   "title": "LASAL 전역 메서드 펼치기",
>   "code": "await sky.click({window:targetWindow,screenshotId:state.screenshots[0].id,x:1507,y:322}); nodeRepl.write(\"clicked\");"
> }
> ```
>
> ```text
> clicked
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 전역 메서드 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state); if(state?.screenshot) await nodeRepl.emitImage(state.screenshot);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1120864,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=10075 chars=9977 sha256=75e3bc9a120457491e63cdcdb7c5c0b638830087046928e4d78fc6caaf9e384d]'... 423423 more characters,
>       originX: -8,
>       originY: -8,
>       width: 1920,
>       height: 1040
>     }
>   ],
>   accessibility: null
> }
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
>   "title": "LASAL 구현 찾기 메뉴 열기",
>   "code": "await sky.click({window:targetWindow,screenshotId:state.screenshots[0].id,x:1576,y:342,mouse_button:\"right\"}); nodeRepl.write(\"opened\");"
> }
> ```
>
> ```text
> opened
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 구현 찾기 메뉴 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state); if(state?.screenshot) await nodeRepl.emitImage(state.screenshot);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1120864,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=10125 chars=9977 sha256=75e3bc9a120457491e63cdcdb7c5c0b638830087046928e4d78fc6caaf9e384d]'... 406655 more characters,
>       originX: -8,
>       originY: -8,
>       width: 1920,
>       height: 1040
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=10134 chars=9977 sha256=8f33e44af3c4e24031d8ba115fbe41483e3fe436eb2e4535c7af5d942651bef3]'... 691 more characters,
>       originX: 1581,
>       originY: 336,
>       width: 182,
>       height: 192
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAEALYDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3XXfD2nXl4ZJ4dzetZv8Awiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAHQeHdHs7GJlto9oNFFFAH/9k=',
>       originX: 1585,
>       originY: 528,
>       width: 182,
>       height: 4
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAC8AAQDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3Xwtq9xeaNDNKfmbrzRWb4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQAeCP+Ret/8APpRR4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQAeCP+Ret/8APpRR4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQAeCP+Ret/8APpRR4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQAeCP+Ret/8APpRR4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQAeCP+Ret/8APpRR4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQB//2Q==',
>       originX: 1763,
>       originY: 340,
>       width: 4,
>       height: 188
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
>   "title": "LASAL 컨텍스트 메뉴 닫기",
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); nodeRepl.write(\"closed\");"
> }
> ```
>
> ```text
> closed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCSdoExecutor 소스 탭 확인",
>   "code": "await sky.click({window:targetWindow,x:192,y:138}); nodeRepl.write(\"clicked\");"
> }
> ```
>
> ```text
> clicked
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCSdoExecutor 소스 상태 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state); if(state?.screenshot) await nodeRepl.emitImage(state.screenshot);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1120864,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=10223 chars=9977 sha256=c7c49bb610cd52d98d4e37ca3e565d995ce75fde64066d65880ccfd07e397f3b]'... 385459 more characters,
>       originX: -8,
>       originY: -8,
>       width: 1920,
>       height: 1040
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=10232 chars=9977 sha256=1251de143a4b296a6bac6fce6d9b9957645599ae002ca944f4dd955a8edfa399]'... 5003 more characters,
>       originX: 185,
>       originY: 102,
>       width: 679,
>       height: 35
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

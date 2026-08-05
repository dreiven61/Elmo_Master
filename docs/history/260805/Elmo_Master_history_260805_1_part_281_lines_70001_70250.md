>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=70002 chars=9977 sha256=8eb8666b1d86063d51744d5e4a354c3e5f96fab19f65f78fb92595dbfc5d66c6]'... 362955 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=70011 chars=9977 sha256=7a6b3a64168f50670d95aced7ad3a94ac4d8ea7cc1c98baed7a950f3237b4737]'... 352695 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
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
>   "code": "await sky.press_key({window:targetWindow,key:'Shift_L+F10'}); nodeRepl.write('ok');",
>   "title": "Open cleanup method menu"
> }
> ```
>
> ```text
> ok
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "Inspect cleanup method menu"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=70063 chars=9977 sha256=8eb8666b1d86063d51744d5e4a354c3e5f96fab19f65f78fb92595dbfc5d66c6]'... 364203 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=70072 chars=9977 sha256=7a6b3a64168f50670d95aced7ad3a94ac4d8ea7cc1c98baed7a950f3237b4737]'... 353799 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=70081 chars=9977 sha256=17527daf33e95225a835745277fd795fdfdc23efdd05a02ef01fbd40e8fa13bd]'... 1179 more characters,
>       originX: 1237,
>       originY: 640,
>       width: 182,
>       height: 192
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAEALYDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3XXPD2nXl35lxDuf1rN/4RPSf+feiigSD/hE9J/596P8AhE9J/wCfeiigYf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD70f8InpP8Az70UUAH/AAiek/8APvR/wiek/wDPvRRQAf8ACJ6T/wA+9H/CJ6T/AM+9FFAB/wAInpP/AD71A/hTSft0I+z8GNz+qUUUAT/8InpP/PvR/wAInpP/AD70UUAdB4d0ezsYmW2j2g0UUUAf/9k=',
>       originX: 1241,
>       originY: 832,
>       width: 182,
>       height: 4
>     },
>     {
>       id: 'screenshot-4',
>       zIndex: 4,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAC8AAQDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3Xwtq9xeaNDNKfmbrzRXOeC7ic6BDsHygkcuB04/u0U7AN8D/APIvQ/7zfzoo8D/8i9D/ALzfzooAb4Lcr4ftwsD49tv+NFR+EpLr+wrf7NBC6cgmSVkOenTaePeigBfClla3Oh273FtBK4GA0kYY49OaKs+C/wDkX7eigA8F/wDIv29FHgv/AJF+3opAN8G6ex0C3P8AaF2vsFj/APiKKb8M3Z/CsR3u8XmOIndtxZM8HJ6jrg9xiiuOpiJRk0iki54KnkHh63xazH3BT2/2qK4Twz8RoodM8m3uvCUUcUjRgal4hFrOSp2kmMQvtBIOOckYOB0orsJOg8H+G7r+xIzYeJtZ0+3d2cW8CWrohY5ODJC7ckk4LHGeMDiitbwBHp974Xtbi2S0uIXztkjCurduCOtFAFX4SWUln4RUzG682eZ5mF0Yi4JAHPlAKOmeM9etFX/BS3X/AAj1vtmhA94ifT/aooA8+8HXviPUNGM9svixYfOljRdM/snyAEYqAPPJkzxznHOcACiul8N+GvC2paVHdan4c028u5PvzT6YsjtgADLFCTRQAvg/xLcnREXT/Desajbo7R/abd7VI3ZThsCSZG4IIztwccZHNFc/4Z+HMc+liaG18IyxySNIG1Hw6Lmf5jkhpBMm7BJx8vTGc9aKAO78FWsZ8PW5LTfhM49Peip/BH/IvW/+fSigD//Z',
>       originX: 1419,
>       originY: 644,
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window:targetWindow,x:1148,y:529}); nodeRepl.write('ok');",
>   "title": "Add cleanup axis mask input"
> }
> ```
>
> ```text
> ok
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write('observed');",
>   "title": "Refresh cleanup axis mask input"
> }
> ```
>
> ```text
> observed
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
>   "code": "await sky.type_text({window:targetWindow,text:'ExpectedAxisMask'}); nodeRepl.write('ok');",
>   "title": "Name cleanup axis mask input"
> }
> ```
>
> ```text
> ok
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:targetWindow,key:'Return'}); nodeRepl.write('ok');",
>   "title": "Commit cleanup axis mask input"
> }
> ```
>
> ```text
> ok
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write('observed');",
>   "title": "Refresh cleanup axis mask properties"
> }
> ```
>
> ```text
> observed
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
>   "code": "await sky.click({window:targetWindow,x:1283,y:512}); nodeRepl.write('ok');",
>   "title": "Open cleanup axis mask type"
> }
> ```
>
> ```text
> ok
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write('observed');",
>   "title": "Refresh cleanup axis mask type"
> }
> ```
>
> ```text
> observed
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
>   "code": "await sky.click({window:targetWindow,x:1038,y:194}); nodeRepl.write('ok');",
>   "title": "Choose cleanup axis mask UDINT"
> }
> ```

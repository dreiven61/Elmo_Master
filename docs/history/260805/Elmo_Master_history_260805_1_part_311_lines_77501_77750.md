>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=77508 chars=9977 sha256=bf9aaff17da1741b5cc2048931887aa152f7ded87a2d68135a392a74a14d6e94]'... 307399 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=77517 chars=9977 sha256=0be8d5e492377541a80f3e75171a2bfd22e925f487981c29f9f368fdb2481d75]'... 298363 more characters,
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
> 새 메서드는 이름만 입력하면 LASAL이 항목을 폐기합니다. 선언 입력/출력이 없는 임시 함수로 판단되는 동작이므로, 이번에는 이름 편집 상태에서 아이콘 메뉴로 입력과 출력을 바로 붙여 확정하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window:targetWindow,x:1067,y:165,mouse_button:'right'}); nodeRepl.write('Global context opened');",
>   "title": "Global 메서드 메뉴 열기"
> }
> ```
>
> ```text
> Global context opened
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "Global 메뉴 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=77571 chars=9977 sha256=bf9aaff17da1741b5cc2048931887aa152f7ded87a2d68135a392a74a14d6e94]'... 303479 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=77580 chars=9977 sha256=0be8d5e492377541a80f3e75171a2bfd22e925f487981c29f9f368fdb2481d75]'... 295835 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=77589 chars=6300 sha256=82053b96408655509a6c928ff7860298b002e593017e9ac777ecaf4565fcdea7]',
>       originX: 1246,
>       originY: 410,
>       width: 169,
>       height: 120
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAEAKkDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDuPhTZQeIvh3o15qqedcIrwb+5VHKrn3wBWda+GdNufAV5csk8c9neXgiaGd4vuXMiru2kbuByDkH0oorWsrVJepC+FFax8A2M3hq+1ea/v3MLahIlqVgEQO6ZOoj8w8er9QK6TQfDOlvrLK0HH9l2j/iWlz/KiisY/ov/AG4t7f13iYg8K6bF4RXXYFeK8hkndlG1o5Veclo2DA/LnHQhhjr1z4x4O8YXOufEe30a80/Tlt5ZbuF5Y43EjAqTnJYjI8tQOMY7UUVMv8/0Kj8V/T9T2uLRdPi03Q74WyGVdAnu8H7pcJARn25NWbvwdpLeDoJ5UmkuL+WzeeXzNrnfLGCFK42gA4G3GOvXJJRWs/jfr+rM6fww9P8AIz/FHgvRtFj1Q2aXb50mZyLi8ln5DqON7EjqemKf458MaVDrFvHHbKsbLZqy4GGH26FcHjsGbH+8aKKVP4qfq/zkP7Uv+3f/AG0W58M6XFcao0dsqt/wkNpHkAAkFYM/+hH86x77wNZ/8IJq+uvf30klr/aFxFassHlBleVRkiMSEY7F6KKcNl/X2Yifxv8ArrI7bxp4b0yHwbqs6W4Dras3X2rA8c+GNKh1i3jjtlWNls1ZcDDD7dCuDx2DNj/eNFFKP8SPr+jGtvu/NFi88NaYmtSItuAv9uQpgY6G1Un9ar6L4b0trjSFa2UiXW9SVvfY1xtP1G0UUUobP1/SJVTZfL/24TSvC+kzeN3tJbVXtn/tEGIgFcbrXjGO5ZifqadrHhmw09Nb06PzpbW3SymhE772j8yYqy7j8xX5ARuJIyecYAKKH/Dj/hB/q/zZFc+HrW88SiVpJI7e01xLNbWNIxG4+zh97EoX3ZbHDAYA49YNH8G2erT+HoVu7uxB0u4kZrdYWLHzY1wRJG46HsAaKKiP2fn/AOkoT3fy/wDSj0rw34Q0vSY7OCBJJDbxrEskhG5gABk4AGeOwFdr9mj/ALtFFaz+Jijsj//Z',
>       originX: 1250,
>       originY: 530,
>       width: 169,
>       height: 4
>     },
>     {
>       id: 'screenshot-4',
>       zIndex: 4,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAB0AAQDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3Xwtq9xeaNDNKfmbrzRWb4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQAeCP+Ret/8APpRR4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQAeCP+Ret/8APpRR4I/5F63/AM+lFAB4I/5F63/z6UUeCP8AkXrf/PpRQAeCP+Ret/8APpRR4I/5F63/AM+lFAB4I/5F63/z6UUUUAf/2Q==',
>       originX: 1415,
>       originY: 414,
>       width: 4,
>       height: 116
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
>   "code": "await sky.click({window:targetWindow,x:1124,y:176}); nodeRepl.write('New Method selected');",
>   "title": "취소 메서드 생성"
> }
> ```
>
> ```text
> New Method selected
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "새 메서드 입력 상태 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=77665 chars=9977 sha256=bf9aaff17da1741b5cc2048931887aa152f7ded87a2d68135a392a74a14d6e94]'... 308011 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=77674 chars=9977 sha256=0be8d5e492377541a80f3e75171a2bfd22e925f487981c29f9f368fdb2481d75]'... 299099 more characters,
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
>   "code": "await sky.press_key({window:targetWindow,key:'Control_L+a'}); await sky.type_text({window:targetWindow,text:'CancelAxisZeroHome'}); nodeRepl.write('method name typed');",
>   "title": "취소 메서드 이름 입력"
> }
> ```
>
> ```text
> method name typed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window:targetWindow,x:1059,y:389,mouse_button:'right'}); nodeRepl.write('provisional method icon context opened');",
>   "title": "취소 메서드 선언 메뉴 열기"
> }
> ```
>
> ```text
> provisional method icon context opened
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "메서드 선언 메뉴 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=77741 chars=9977 sha256=bf9aaff17da1741b5cc2048931887aa152f7ded87a2d68135a392a74a14d6e94]'... 307151 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=77750 chars=9977 sha256=0be8d5e492377541a80f3e75171a2bfd22e925f487981c29f9f368fdb2481d75]'... 298539 more characters,

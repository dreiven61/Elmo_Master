>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=65004 chars=9977 sha256=e48294ce7928fa9219068b3e308eaee6defeec82cad67104aa3226cc4d591207]'... 352631 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=65013 chars=9977 sha256=3773e74f9a6544cb2d1a667a8958a899b491cde2d5194f58ca18f5163ead92f7]'... 44167 more characters,
>       originX: 1197,
>       originY: 293,
>       width: 410,
>       height: 452
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQANEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDR8aWmiWmu3c+owajJLeX12zSQXCKsYW4deEMZ3cDONwz6iqNv4Ys4NQSDUZp3iW5uI5HgIUtHHCJFZQQcE575rovFdhe3OtXiz+ENbvTbXt00Ese9YJVeZ3BZRGSV5/hcZHQisiO38Ubi9x4Z1aSVpriZmFpIoJliEeANvAGM/pxX21KpL2atLo+q+VtdPwPiqlOPtXzR0v2fzvprr6mPLo9rpwv5b/zrmG3nhWP7PKI/OikSRlcMVbGQqHp3I+nqnwhtNOsPGWqWumRXaBLNGka4nWXdu2MMbUXGMkd88dO/nkmm+JZfDS6VL4Z1ZnSVXSf7LJkIu8hMbefmkY5z3I+novwgtdU/4S3Vr7UtJvdPSW0jjX7REygldq8EgZ+7muHOJOWFleV9Y9f70en3s7cnio4qFo20fTb3ZaX+5ef3nrtFFFfLn1AUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFNanU1qEJlabpWZdHg1pyjis26Xg1vTMpmJeN1rNU7p1U7Tngbume2fbNal2nWsmZCDkV3w1Vjhno7nkf/CWfE//AKAuk/8AgIf/AIuivaP7Sf8A55CiuX2FXudPtqXY/9k=',
>       originX: 1398,
>       originY: 747,
>       width: 209,
>       height: 16
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
>   "code": "await sky.type_text({window:targetWindow,text:'void'});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=65076 chars=9977 sha256=e9fe7c02d0475dd7e460c27bc479800466b256c15777980e8da880c206d34a73]'... 316995 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=65085 chars=9977 sha256=e48294ce7928fa9219068b3e308eaee6defeec82cad67104aa3226cc4d591207]'... 310431 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=65094 chars=9977 sha256=191c8fda03ff3ebe15cc32219749f7dfe14091cf022a3edf7d0d713c81dc9a3d]'... 1499 more characters,
>       originX: 1197,
>       originY: 293,
>       width: 410,
>       height: 452
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQANEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD2vwNrthZaNqFuJftEsfiPUbR1hx8skl3LMfvY4VHy3+62M4GZbn4gRHSL27XTdTsIxp81/ZXd1bpJFdRxruLKiyhuhVgkhjYg9sHFHwH4O8q+vtWvTPCJb7UwbKSMoGZr652T5POTFJgHHKkHPSrMvgO/vNHm0zU9diuLWLTptOsCljseJZI/L8yVvMPmuFAHyiMHLZHI2vRt9P6/q3rqOOjV/n/X5/gP8VeMp7aOW30qzv3aC7tLa41GOKM28TySRboyGbeSUkHKqyjcMsCDju64nVvBmoXlzdRW2uJb6TdXdvfS25tN8vmxNGSFk3gCNvKXK7Sck4bHFdtR0/ry/W5KvbUKKKKQwooooAKKKKACiiigAooooAKKKKACiiigAooooAKa1OprUITK03Ssy6PBrTlHFZt0vBremZTMS8brWap3Tqp2nPA3dM9s+2a1LtOtZMyEHIrvhqrHDPR3PI/+Es+J/wD0BdJ/8BD/APF0V7R/aT/88hRXL7Cr3On21Lsf/9k=',
>       originX: 1398,
>       originY: 747,
>       width: 209,
>       height: 16
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
>   "code": "await sky.press_key({window:targetWindow,key:'Down'});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=65157 chars=9977 sha256=e9fe7c02d0475dd7e460c27bc479800466b256c15777980e8da880c206d34a73]'... 317811 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=65166 chars=9977 sha256=e48294ce7928fa9219068b3e308eaee6defeec82cad67104aa3226cc4d591207]'... 310911 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=65175 chars=9977 sha256=042c44daa01f9630f2b4ec06122bca51438ea928f1ba1e8c9c8cf9c03b4fc2f8]'... 2647 more characters,
>       originX: 1197,
>       originY: 293,
>       width: 410,
>       height: 452
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQANEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD2vwNrthZaNqFuJftEsfiPUbR1hx8skl3LMfvY4VHy3+62M4GZbn4gRHSL27XTdTsIxp81/ZXd1bpJFdRxruLKiyhuhVgkhjYg9sHFHwH4O8q+vtWvTPCJb7UwbKSMoGZr652T5POTFJgHHKkHPSrMvgO/vNHm0zU9diuLWLTptOsCljseJZI/L8yVvMPmuFAHyiMHLZHI2vRt9P6/q3rqOOjV/n/X5/gP8VeMp7aOW30qzv3aC7tLa41GOKM28TySRboyGbeSUkHKqyjcMsCDju64nVvBmoXlzdRW2uJb6TdXdvfS25tN8vmxNGSFk3gCNvKXK7Sck4bHFdtR0/ry/W5KvbUKKKKQwooooAKKKKACiiigAooooAKKKKACiiigAooooAKa1OprUITK03Ssy6PBrTlHFZt0vBremZTMS8brWap3Tqp2nPA3dM9s+2a1LtOtZMyEHIrvhqrHDPR3PI/+Es+J/wD0BdJ/8BD/APF0V7R/aT/88hRXL7Cr3On21Lsf/9k=',
>       originX: 1398,
>       originY: 747,
>       width: 209,
>       height: 16
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
>   "code": "await sky.press_key({window:targetWindow,key:'Down'});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=65238 chars=9977 sha256=e9fe7c02d0475dd7e460c27bc479800466b256c15777980e8da880c206d34a73]'... 316691 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=65247 chars=9977 sha256=e48294ce7928fa9219068b3e308eaee6defeec82cad67104aa3226cc4d591207]'... 310979 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,

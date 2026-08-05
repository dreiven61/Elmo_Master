> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write('observed');",
>   "title": "Refresh destination filter"
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
>   "code": "await sky.type_text({window:targetWindow,text:'void'}); nodeRepl.write('ok');",
>   "title": "Filter void type"
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
>   "title": "Inspect void matches"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67813 chars=9977 sha256=e9fe7c02d0475dd7e460c27bc479800466b256c15777980e8da880c206d34a73]'... 317311 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67822 chars=9977 sha256=e48294ce7928fa9219068b3e308eaee6defeec82cad67104aa3226cc4d591207]'... 310603 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67831 chars=9977 sha256=9342aa0b4e7c055467ab2de0c582709947387b5aaef8742edfed7cc1a4e3c919]'... 1491 more characters,
>       originX: 1197,
>       originY: 293,
>       width: 410,
>       height: 452
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQANEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD2vwNrthZaNqFuJftEsfiPUbR1hx8skl3LMfvY4VHy3+62M4GZbn4gRHSL27XTdTsIxp81/ZXd1bpJFdRxruLKiyhuhVgkhjYg9sHFHwH4O8q+vtWvTPCJb7UwbKSMoGZr652T5POTFJgHHKkHPSrMvgO/vNHm0zU9diuLWLTptOsCljseJZI/L8yVvMPmuFAHyiMHLZHI2vRt9P6/q3rqOOjV/n/X5/gP8VeMp7aOW30qzv3aC7tLa41GOKM28TySRboyGbeSUkHKqyjcMsCDjuWri9W8GaheXN1Fba4lvpN1d299Lbm03y+bE0ZIWTeAI28pcrtJyThscV2jULb+vL9bkK9tSvKazbluDWjN0rMuulbUzOZk3chGazHcs+AQOpyTgAetXrzPNZhby5lYjIB5B7juK74LTQ4pvXUradf22pw+dpV5bX8I6vayiQD646fjWhbynPNfJ8PgTXofjLqegeG7x9PmgledbtZGQR25+ZWJXk/KyjHc8V9P6Ba3VjpNpaajqcuq3kQIkvZUCNLk8cDsOnPNZUqsqmjRpUpRhqmdPaP0rYtm6Vh2Y6Vs2vas6qNKTNKI1OKrxVYWuRnShaKKKkoKKKKACiiigAooooAKKKKACiiigAooooA//9k=',
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
>   "code": "await sky.press_key({window:targetWindow,key:'Down'}); nodeRepl.write('ok');",
>   "title": "Move to second void match"
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
>   "title": "Refresh void selection"
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
>   "code": "await sky.press_key({window:targetWindow,key:'Down'}); nodeRepl.write('ok');",
>   "title": "Move to plain void match"
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
>   "title": "Refresh plain void selection"
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
>   "code": "await sky.press_key({window:targetWindow,key:'Return'}); nodeRepl.write('ok');",
>   "title": "Commit plain void type"
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
>   "title": "Verify destination void type"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67972 chars=9977 sha256=8eb8666b1d86063d51744d5e4a354c3e5f96fab19f65f78fb92595dbfc5d66c6]'... 361679 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=67981 chars=9977 sha256=7a6b3a64168f50670d95aced7ad3a94ac4d8ea7cc1c98baed7a950f3237b4737]'... 350803 more characters,
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

> {
>   "code": "await sky.click({window:targetWindow,x:1232,y:530,clicks:2}); nodeRepl.write('ok');",
>   "title": "Enable destination pointer"
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
>   "title": "Verify destination pointer"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=68033 chars=9977 sha256=8eb8666b1d86063d51744d5e4a354c3e5f96fab19f65f78fb92595dbfc5d66c6]'... 362631 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=68042 chars=9977 sha256=7a6b3a64168f50670d95aced7ad3a94ac4d8ea7cc1c98baed7a950f3237b4737]'... 351591 more characters,
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
>   "code": "await sky.click({window:targetWindow,x:1240,y:530}); nodeRepl.write('ok');",
>   "title": "Open pointer selector"
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
>   "title": "Inspect pointer selector"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=68094 chars=9977 sha256=8eb8666b1d86063d51744d5e4a354c3e5f96fab19f65f78fb92595dbfc5d66c6]'... 362743 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=68103 chars=9977 sha256=7a6b3a64168f50670d95aced7ad3a94ac4d8ea7cc1c98baed7a950f3237b4737]'... 351647 more characters,
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
>   "code": "await sky.click({window:targetWindow,x:1431,y:530}); nodeRepl.write('ok');",
>   "title": "Open pointer choices"
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
>   "title": "Inspect pointer choices"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=68155 chars=9977 sha256=8eb8666b1d86063d51744d5e4a354c3e5f96fab19f65f78fb92595dbfc5d66c6]'... 362963 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=68164 chars=9977 sha256=7a6b3a64168f50670d95aced7ad3a94ac4d8ea7cc1c98baed7a950f3237b4737]'... 352759 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,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',
>       originX: 1398,
>       originY: 784,
>       width: 226,
>       height: 38
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window:targetWindow,x:1235,y:551}); nodeRepl.write('ok');",
>   "title": "Set destination pointer true"
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
>   "title": "Verify destination pointer true"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=68227 chars=9977 sha256=8eb8666b1d86063d51744d5e4a354c3e5f96fab19f65f78fb92595dbfc5d66c6]'... 362643 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=68236 chars=9977 sha256=7a6b3a64168f50670d95aced7ad3a94ac4d8ea7cc1c98baed7a950f3237b4737]'... 351835 more characters,
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

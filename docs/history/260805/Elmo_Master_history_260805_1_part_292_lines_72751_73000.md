> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window:targetWindow,x:1075,y:383}); nodeRepl.write('ok');",
>   "title": "Focus class tree output"
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
>   "title": "Refresh class tree output"
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
>   "code": "await sky.press_key({window:targetWindow,key:'Home'}); nodeRepl.write('ok');",
>   "title": "Move to project tree root"
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
>   "title": "Inspect project class tree"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=72820 chars=9977 sha256=61605408bf09ddf9f8ff22c837ea825515d711ed6f27f01b890cd3a331a975f4]'... 305667 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=72829 chars=9977 sha256=ad3b7d9760c8c5e46c699ee0bb29473b12ec019b05d6f3181fa4c6f47feb56f8]'... 296819 more characters,
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
>   "code": "await sky.click({window:targetWindow,x:1110,y:423}); nodeRepl.write('ok');",
>   "title": "Switch to global method view"
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
>   "title": "Inspect global method view"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=72881 chars=9977 sha256=61605408bf09ddf9f8ff22c837ea825515d711ed6f27f01b890cd3a331a975f4]'... 291823 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=72890 chars=9977 sha256=ad3b7d9760c8c5e46c699ee0bb29473b12ec019b05d6f3181fa4c6f47feb56f8]'... 283243 more characters,
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
>   "code": "await sky.click({window:targetWindow,x:1210,y:419}); nodeRepl.write('ok');",
>   "title": "Switch to class view"
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
>   "title": "Inspect class view root"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 25496404,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=72942 chars=9977 sha256=61605408bf09ddf9f8ff22c837ea825515d711ed6f27f01b890cd3a331a975f4]'... 305483 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=72951 chars=9977 sha256=ad3b7d9760c8c5e46c699ee0bb29473b12ec019b05d6f3181fa4c6f47feb56f8]'... 297035 more characters,
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
>   "code": "await sky.click({window:targetWindow,x:1150,y:363}); nodeRepl.write('ok');",
>   "title": "Select observation cycle row"
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
>   "title": "Refresh observation cycle selection"
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

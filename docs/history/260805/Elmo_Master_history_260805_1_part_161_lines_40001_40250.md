>   "title": "observer 변수 추가 메뉴 열기",
>   "code": "await sky.perform_secondary_action({window:targetWindow,x:1072,y:284}); await new Promise(r=>setTimeout(r,300)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> action is required
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "observer 변수 추가 메뉴 열기",
>   "code": "await sky.click({window:targetWindow,x:1072,y:284,button:'right'}); await new Promise(r=>setTimeout(r,300)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
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
>   "title": "LASAL 컨텍스트 메뉴 호출",
>   "code": "try{await sky.perform_secondary_action({window:targetWindow,action:'foo',x:1072,y:284});}catch(e){nodeRepl.write(String(e));}",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> TypeError: element_index must be an integer >= 0
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL UI 요소 검색",
>   "code": "var stAcc=await sky.get_window_state({window:targetWindow,include_accessibility:true}); nodeRepl.write({keys:Object.keys(stAcc),acc:stAcc.accessibility?.slice?.(0,30)});",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> { keys: [ 'window', 'screenshots', 'accessibility' ], acc: undefined }
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
>   "title": "변수 추가 메뉴 열기",
>   "code": "await sky.press_key({window:targetWindow,keys:['SHIFT','F10']}); await new Promise(r=>setTimeout(r,300)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> key is required
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "변수 추가 메뉴 열기",
>   "code": "await sky.press_key({window:targetWindow,key:'SHIFT+F10'}); await new Promise(r=>setTimeout(r,300)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "observer 변수 선언 생성",
>   "code": "await sky.click({window:targetWindow,x:1095,y:303}); await new Promise(r=>setTimeout(r,350)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
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
>   "title": "observer 변수 이름 입력",
>   "code": "await sky.type_text({window:targetWindow,text:'OwnershipObserverState'}); await sky.press_key({window:targetWindow,key:'ENTER'}); await new Promise(r=>setTimeout(r,500)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
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
>   "title": "observer 배열 타입 설정",
>   "code": "await sky.click({window:targetWindow,x:1305,y:526}); await new Promise(r=>setTimeout(r,200)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
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
>   "title": "observer 배열 타입 선택",
>   "code": "await sky.click({window:targetWindow,x:1032,y:264}); await new Promise(r=>setTimeout(r,300)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
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
>   "title": "observer 배열 범위 열기",
>   "code": "await sky.click({window:targetWindow,x:1028,y:382}); await new Promise(r=>setTimeout(r,300)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
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
>   "title": "observer 배열 범위 설정",
>   "code": "await sky.click({window:targetWindow,x:1080,y:382}); await new Promise(r=>setTimeout(r,200)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
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
>   "title": "observer 배열 상한 입력",
>   "code": "await sky.click({window:targetWindow,x:1305,y:526}); await sky.press_key({window:targetWindow,key:'CTRL+A'}); await sky.type_text({window:targetWindow,text:'107'}); await sky.press_key({window:targetWindow,key:'ENTER'}); await new Promise(r=>setTimeout(r,400)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
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
>   "title": "observer 배열 상한 수정",
>   "code": "await sky.click({window:targetWindow,x:1245,y:525}); await sky.press_key({window:targetWindow,key:'F2'}); await sky.press_key({window:targetWindow,key:'CTRL+A'}); await sky.type_text({window:targetWindow,text:'107'}); await sky.press_key({window:targetWindow,key:'ENTER'}); await new Promise(r=>setTimeout(r,350)); state=await sky.get_window_state({window:targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>

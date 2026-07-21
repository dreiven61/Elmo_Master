> {
>   "title": "Global method 그룹 선택",
>   "code": "var selGlobal=await sky.click({window:targetWindow,x:1000,y:425}); state=null;"
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
>   "title": "Global method 그룹 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); targetWindow=state.window; if(state.screenshots&&state.screenshots[0]) await nodeRepl.emitImage(state.screenshots[0].url);"
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
>   "title": "Global method 메뉴 열기",
>   "code": "var ctxGlobal=await sky.press_key({window:targetWindow,key:'Shift_L+F10'}); state=null;"
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
>   "title": "Global method 메뉴 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); targetWindow=state.window; if(state.screenshots&&state.screenshots[0]) await nodeRepl.emitImage(state.screenshots[0].url);"
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
>   "title": "Global HandleRequest 생성",
>   "code": "var newGlobalMethod=await sky.click({window:targetWindow,x:1040,y:450}); state=null;"
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
>   "title": "Global method 이름 입력 상태",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); targetWindow=state.window; if(state.screenshots&&state.screenshots[0]) await nodeRepl.emitImage(state.screenshots[0].url);"
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
>   "title": "Global method 기본 이름 선택",
>   "code": "var selGlobalName=await sky.press_key({window:targetWindow,key:'Ctrl_L+A'}); state=null;"
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
>   "title": "HandleRequest global 이름 입력",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); targetWindow=state.window; var typeGlobal=await sky.type_text({window:targetWindow,text:'HandleRequest'}); state=null;"
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
>   "title": "HandleRequest global 적용",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); targetWindow=state.window; var applyGlobal=await sky.click({window:targetWindow,x:1000,y:428}); state=null;"
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
>   "title": "HandleRequest global 생성 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); targetWindow=state.window; if(state.screenshots&&state.screenshots[0]) await nodeRepl.emitImage(state.screenshots[0].url);"
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
>   "title": "입력 API 형식 확인",
>   "code": "nodeRepl.write(String(sky.press_key));"
> }
> ```
>
> ```text
> (...args) => {
>         globals.nodeRepl?.setResponseMeta({
>           [TOOL_SURFACE_META_KEY]: {
>             kind: "computerUse",
>             app: getComputerUseAppReference(args[0]),
>           },
>         });
>         return Reflect.apply(value, target, args);
>       }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "HandleRequest 생성 확정 방식 시험",
>   "code": "var ctxGlobal2=await sky.press_key({window:targetWindow,key:'Shift_L+F10'}); state=null;"
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
>   "title": "New Global Method 선택",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); targetWindow=state.window; var newGlobal2=await sky.click({window:targetWindow,x:1040,y:450}); state=null;"
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
>   "title": "Global method 이름 선택",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); targetWindow=state.window; var selGlobalName2=await sky.press_key({window:targetWindow,key:'Ctrl_L+A'}); state=null;"
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
>   "title": "HandleRequest 이름과 확정 입력",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); targetWindow=state.window; var typeGlobalNl=await sky.type_text({window:targetWindow,text:'HandleRequest\\n'}); state=null;"
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
>   "title": "HandleRequest 생성 시험 결과",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); targetWindow=state.window; if(state.screenshots&&state.screenshots[0]) await nodeRepl.emitImage(state.screenshots[0].url);"

>   "title": "LASAL 클래스 목록 이동"
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
>   "code": "state = await sky.get_window_state({window: targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "title": "LASAL 클래스 목록 확인"
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
>   "code": "await sky.click({window: targetWindow, x: 818, y: 136});",
>   "title": "Control 클래스 선택"
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
>   "code": "state = await sky.get_window_state({window: targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "title": "Control 선택 상태 확인"
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
>   "code": "await sky.click({window: targetWindow, x: 817, y: 136, button: 'right'});",
>   "title": "Control 클래스 메뉴 열기"
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
>   "code": "state = await sky.get_window_state({window: targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "title": "Control 클래스 메뉴 확인"
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
>   "code": "await sky.press_key({window: targetWindow, key: 'SHIFT+F10'});",
>   "title": "Control 클래스 명령 열기"
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
>   "code": "state = await sky.get_window_state({window: targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "title": "Control 명령 확인"
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
>   "code": "await sky.press_key({window: targetWindow, key: 'ESC'});",
>   "title": "잘못 열린 메뉴 닫기"
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
>   "code": "await sky.click({window: targetWindow, x: 706, y: 136});",
>   "title": "Control 클래스 펼치기"
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
>   "code": "state = await sky.get_window_state({window: targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "title": "Control 클래스 구조 확인"
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
>   "code": "await sky.click({window: targetWindow, x: 718, y: 198});",
>   "title": "Control 메서드 목록 펼치기"
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
>   "code": "state = await sky.get_window_state({window: targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "title": "Control 메서드 목록 확인"
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
>   "code": "await sky.click({window: targetWindow, x: 736, y: 237});",
>   "title": "Control private 메서드 펼치기"
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
>   "code": "state = await sky.get_window_state({window: targetWindow}); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "title": "Control private 메서드 확인"
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
>   "code": "await sky.scroll({window: targetWindow, x: 932, y: 237, scrollX: 0, scrollY: 363});",
>   "title": "Control Process 메서드 찾기"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call

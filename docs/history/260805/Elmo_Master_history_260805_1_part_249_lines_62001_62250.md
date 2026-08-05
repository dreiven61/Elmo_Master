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
>   "code": "await sky.click({window:targetWindow,x:1428,y:509});"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=62043 chars=9977 sha256=e9fe7c02d0475dd7e460c27bc479800466b256c15777980e8da880c206d34a73]'... 318303 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=62052 chars=9977 sha256=e48294ce7928fa9219068b3e308eaee6defeec82cad67104aa3226cc4d591207]'... 313471 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=62061 chars=9977 sha256=d884a0d61c49aed46622ec5e4fdb0da78c90f59e289c8aad2bc1420edcb396ed]'... 30367 more characters,
>       originX: 1197,
>       originY: 293,
>       width: 410,
>       height: 452
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQANEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwB/jWwt18Q6vLcT+TcXOo3Txl87FjWRxyApJLMCo6AbeeuRkWOhi7t57n+07CG1g8oSSy+bhWkDELgIWJG0g4BH1HNdj400LU7/AFm9kbQ9YmEF7dRosNtIBLG8sjpIr7GBAZjkdxtx1JFXSvD97DoOpRXXhXXZY5JbXEQV0lLKsu51PlH5cnptOAw5zg19zTxCjRVpa6du9uvl8j4Wph3Kq/d0179r/n8/wOf1bw+1lZxeXuku0uWtLhFbepY/NGyYH3WUnHX7p9cVYn8MRtePHBqNrbxCb7JE927D7ROoXzAm1SAoYjlsDkcnmtmy/wCEpt9V1G9fwtqcn2oBo4vskoWGRP8AVOPlOdnQevPNVtL0zWYLSCLUfCOtXbW07XFuyRSRfM23If5CWXKqeCp6888V7adtZL711+fR/gJ0YX0i/ufT5X1VvmvMyP8AhHJGhtmd4rNfIknuJriUskYWVouQqZB3ADA3ZznjnFnXtEs7eJ1s2Bngsre6ZkctHKrBQzDIyDuZT9CeARWsIPEE8aw6l4U1m4ha3khn2QSRu5aczB1byyFIYgdDkZ9eF1DTdYu7e7ni8NarbyPaw2ENsLSZyqJtJkLbAOiAeuSan20uZc0lv3Vt/W//AACvYQs+WL26p329Lfrc+laKKK+GPuQooooAKKKKACiiigAooooAKKKKACiiigAprU6mtQhMrTHisy6bg1pyjis26Xg1vTMpmLeN1rNHzzKpGc9BnGT2H4mtK7XrWVKpVgR1BzXfDVHDPRnkv/C2Na/6Ef8A8m3/APiaK9k8+z/59I/++BRXN7Ov3Onno9j/2Q==',
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
>   "code": "await sky.press_key({window:targetWindow,key:'End'});"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=62124 chars=9977 sha256=e9fe7c02d0475dd7e460c27bc479800466b256c15777980e8da880c206d34a73]'... 318255 more characters,
>       originX: 182,
>       originY: 243,
>       width: 1449,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=62133 chars=9977 sha256=e48294ce7928fa9219068b3e308eaee6defeec82cad67104aa3226cc4d591207]'... 313471 more characters,
>       originX: 187,
>       originY: 248,
>       width: 1439,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=62142 chars=9977 sha256=d884a0d61c49aed46622ec5e4fdb0da78c90f59e289c8aad2bc1420edcb396ed]'... 30367 more characters,
>       originX: 1197,
>       originY: 293,
>       width: 410,
>       height: 452
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQANEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD6ObVLx9au4rS2+0WVn5cMyx7fNaZ9rcFnUKqIwY9S27jlcNW1XxS2n3drZjQtWutQulneG1g8gsyROis5ZpQig+YrDLA44IDcVLDY6haa3qhsvKSC+kiu/tEqeYqMqpHJGVDqclEUqwyAS2RwA2Tr2meIbjxTpE+n6jbW88VreqbhtPeW32PJBsR080HfgcEOMlSduMihdP66f5/1sP8Ar+v6/UtaD4sj1C+ujOUh097NNQtJXQxt5XKyrICTh0cfNwMB1HUE1Sh8czQWEUl5o2oXc/2f7fdLp8SEWVu7MYvMDyBmfYpysYZsqflGVBk1PwDbXmi6Np0d7LGLAGKaZkDPdQP/AK+NugAkIBJHcdKta/4a1G81C6n0jWItPhv7dbW9jktPPJRd2GhO9RG+HYZYOvC/Lwcj8v8Agf59vPR9wVuv9f1r5bEbeMoop7+OOG41KZb1LS0tbOFUklLW6TnDPKFICsWLMYwMYwTgtxfizxNqF78L/FT6ihFteWus2caSxBJoJI45yiNg7SNkbDI7qDlg2R18vgue2nmutE1OKzuxepd23nWpmijUWqWxjdA6lwVTOQykHHXBzzXjTwlqsXgDWtLha51N/suqag06RgNPPPFOEgSNSWJzMe38KgZJp6X+X+X/AARO/L5/8B/rY9YooopAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFNanU1qEJlaY8VmXTcGtOUcVm3S8Gt6ZlMxbxutZo+eZVIznoM4yew/E1pXa9aypVKsCOoOa74ao4Z6M8l/4WxrX/Qj/APk2/wD8TRXsnn2f/PpH/wB8Ciub2dfudPPR7H//2Q==',
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
>   "code": "await sky.press_key({window:targetWindow,key:'ArrowDown'});"
> }
> ```
>
> ```text
> unsupported key: ArrowDown
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write(await sky.documentation({topic:'api'}));"
> }
> ```
>
> ```text
> Unsupported Computer Use documentation
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write((await sky.documentation('api')).slice(0,12000));"
> }
> ```
>
> ````text
> ## API Reference
>
> # Sky Window2 API
>
> ## API Reference
>
> Use this as the supported `sky` window2 API surface.
>
> ```ts
> import { sky } from "@oai/sky";
>
> const apps = await sky.list_apps();
> const candidate_windows = apps.flatMap((app) => app.windows);
> // Choose the task-specific app and window before acting.
> // Each input action takes the specific Window for that action.
>
> interface Window2ComputerUseClient {
>   list_windows(): Promise<Array<Window>>; // List open windows that can be targeted by the window2 API.
>   get_window(input: GetWindowInput): Promise<Window>; // Rehydrate a currently open window by id; useful after losing a window binding.
>   list_apps(): Promise<Array<ListAppsApp>>; // List installed apps, including their currently open targetable windows when present.
>   launch_app(input: LaunchAppInput): Promise<void>; // Launch an app by id so its window can be selected from `list_apps()`.
>   get_window_state(input: GetWindowStateInput): Promise<WindowState>; // Capture selected state for an open window.
>   click(input: ClickInput): Promise<void>; // Click either an indexed element from the latest window state or a coordinate in the window.
>   press_key(input: PressKeyInput): Promise<void>; // Press a `+`-separated keyboard chord in a window.
>   type_text(input: TypeTextInput): Promise<void>; // Type text into the current focus in a window.
>   scroll(input: ScrollInput): Promise<void>; // Scroll by a delta from a specific coordinate in the window.
>   set_value(input: SetValueInput): Promise<void>; // Replace the value of an indexed editable element.
>   drag(input: DragInput): Promise<void>; // Drag from one window coordinate to another.
>   perform_secondary_action(input: PerformSecondaryActionInput): Promise<void>; // Invoke a secondary accessibility action on an indexed element.
>   activate_window(input: ActivateWindowInput): Promise<void>; // Optional escape hatch to bring an open window to the foreground; input methods activate their target window automatically.
>   target: "windows";
> }
>
> type Window = {
>   app: AppIdentifier; // App identifier for the app that owns this window; process-backed identifiers may include the full process path.
>   id: number; // Opaque identifier for the open window.
>   title?: string; // User-visible window title when available; may contain PII.
> };
>
> type GetWindowInput = {
>   app?: AppIdentifier; // Optional app identifier to carry forward from a previously returned `Window`.
>   id: number; // Opaque window identifier from a previously returned `Window`.

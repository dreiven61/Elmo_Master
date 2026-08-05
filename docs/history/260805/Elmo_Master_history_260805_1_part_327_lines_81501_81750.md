>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 제어 상태 확인",
>   "code": "nodeRepl.write({skyType: typeof sky, pressKey: typeof sky !== 'undefined' ? String(sky.press_key) : null, explorer: typeof projectExplorerWindow !== 'undefined' ? projectExplorerWindow : null})"
> }
> ```
>
> ```text
> {
>   skyType: 'object',
>   pressKey: '(...args) => {\r\n' +
>     '        const app = getComputerUseAppReference(args[0]);\r\n' +
>     '        globals.nodeRepl?.setResponseMeta({\r\n' +
>     '          [TOOL_SURFACE_META_KEY]: {\r\n' +
>     '            kind: "computerUse",\r\n' +
>     '            app,\r\n' +
>     '          },\r\n' +
>     '          ...(isChromeComputerUseAppReference(app)\r\n' +
>     '            ? { [CHROME_COMPUTER_USE_META_KEY]: true }\r\n' +
>     '            : {}),\r\n' +
>     '        });\r\n' +
>     '        return Reflect.apply(value, target, args);\r\n' +
>     '      }',
>   explorer: {
>     app: 'process:C:\\Windows\\explorer.exe',
>     id: 721818,
>     title: 'Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]'
>   }
> }
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Windows UI 제어 지침 확인",
>   "code": "var cuaGuidance = await sky.documentation('guidance'); var cuaApi = await sky.documentation('api'); var cuaConfirm = await sky.documentation('confirmations'); nodeRepl.write({guidance:cuaGuidance, api:cuaApi, confirmations:cuaConfirm})"
> }
> ```
>
> ````text
> {
>   guidance: '## node_repl + @oai/sky\r\n' +
>     '\r\n' +
>     '- Use `node_repl` JavaScript for all Computer Use actions.\r\n' +
>     '- The `node_repl` state persists across calls. Store cross-cell values on `globalThis`; top-level `const` and `let` names cannot be redeclared by later retries.\r\n' +
>     '- For text output, call `nodeRepl.write(...)` with a string. Use `JSON.stringify(...)` for objects.\r\n' +
>     '\r\n' +
>     '## Workflow\r\n' +
>     '\r\n' +
>     '### Initialize target selection\r\n' +
>     '\r\n' +
>     'Run the Initialize setup cell from `SKILL.md` first. Then list apps and choose the target from returned app and window objects:\r\n' +
>     '\r\n' +
>     '```js\r\n' +
>     'globalThis.apps = await sky.list_apps();\r\n' +
>     'nodeRepl.write(JSON.stringify(apps, null, 2));\r\n' +
>     '```\r\n' +
>     '\r\n' +
>     'Never reconstruct an app or window from guessed fields. Do not call `get_window`, `activate_window`, or any input method until selection has produced exactly one returned window.\r\n' +
>     '\r\n' +
>     '```js\r\n' +
>     '{\r\n' +
>     '  function escapeRegExp(value) {\r\n' +
>     '    return value.replace(/[.*+?^${}()|[\\]\\\\]/g, "\\\\$&");\r\n' +
>     '  }\r\n' +
>     '\r\n' +
>     '  function returnedWindowSummary(window) {\r\n' +
>     '    return {\r\n' +
>     '      id: window.id,\r\n' +
>     '      app: window.app,\r\n' +
>     '      title: window.title,\r\n' +
>     '    };\r\n' +
>     '  }\r\n' +
>     '\r\n' +
>     '  function requireUniqueWindow(windows, label) {\r\n' +
>     '    if (windows.length !== 1) {\r\n' +
>     '      nodeRepl.write(\r\n' +
>     '        `Returned candidate windows:\\n${JSON.stringify(windows.map(returnedWindowSummary), null, 2)}`,\r\n' +
>     '      );\r\n' +
>     '      throw new Error(`Expected exactly one target window for ${label}; found ${windows.length}`);\r\n' +
>     '    }\r\n' +
>     '    return windows[0];\r\n' +
>     '  }\r\n' +
>     '\r\n' +
>     '  globalThis.apps = await sky.list_apps();\r\n' +
>     '  globalThis.targetApp = apps.find((app) => app.id === "<app id>");\r\n' +
>     '  if (!targetApp) throw new Error("Target app was not returned by list_apps");\r\n' +
>     '  const targetAppId = targetApp.id;\r\n' +
>     '\r\n' +
>     '  if (targetApp.windows.length === 0) {\r\n' +
>     '    await sky.launch_app({ app: targetApp.id });\r\n' +
>     '    globalThis.apps = await sky.list_apps();\r\n' +
>     '    globalThis.targetApp = apps.find((app) => app.id === targetAppId);\r\n' +
>     '  }\r\n' +
>     '  if (!targetApp?.windows.length) {\r\n' +
>     '    throw new Error("Target app did not expose a window after launch");\r\n' +
>     '  }\r\n' +
>     '\r\n' +
>     '  const windowTitleHint = "<optional exact window title>";\r\n' +
>     '  const candidateWindows =\r\n' +
>     '    windowTitleHint === "<optional exact window title>"\r\n' +
>     '      ? targetApp.windows\r\n' +
>     '      : targetApp.windows.filter((window) =>\r\n' +
>     '          new RegExp(`^${escapeRegExp(windowTitleHint)}$`, "i").test(window.title ?? ""),\r\n' +
>     '        );\r\n' +
>     '  const returnedWindow = requireUniqueWindow(candidateWindows, targetApp.id);\r\n' +
>     '\r\n' +
>     '  globalThis.targetWindow = await sky.get_window({\r\n' +
>     '    id: returnedWindow.id,\r\n' +
>     '    app: returnedWindow.app,\r\n' +
>     '  });\r\n' +
>     '  await sky.activate_window({ window: targetWindow });\r\n' +
>     '  globalThis.state = await sky.get_window_state({ window: targetWindow });\r\n' +
>     '  globalThis.targetWindow = state.window;\r\n' +
>     '}\r\n' +
>     '```\r\n' +
>     '\r\n' +
>     'Use `list_windows()` when inspecting currently open windows or recovering a known running app. If the intended app is absent from `list_apps`, launch it with an explicit `.exe` path or `.exe` process identifier, refresh `list_apps()` or `list_windows()`, filter to the intended returned windows, and stop unless the filtered list has exactly one window. Escape Windows path backslashes in JavaScript strings, for example `await sky.launch_app({ app: "C:\\\\Users\\\\me\\\\build\\\\MyApp.exe" });`.\r\n' +
>     '\r\n' +
>     '### Act and refresh\r\n' +
>     '\r\n' +
>     'Use a two-cell loop for state-derived inputs: observe and stop, inspect the result, then perform exactly one action and refresh immediately. Element indexes, screenshot IDs, and coordinates are valid only for the observation that produced them. Interleaving or retry requires re-observation.\r\n' +
>     '\r\n' +
>     'Accessibility path, cell 1: observe and inspect.\r\n' +
>     '\r\n' +
>     '```js\r\n' +
>     'globalThis.state = await sky.get_window_state({\r\n' +
>     '  window: targetWindow,\r\n' +
>     '  include_screenshot: false,\r\n' +
>     '  include_text: true,\r\n' +
>     '});\r\n' +
>     'globalThis.targetWindow = state.window;\r\n' +
>     'nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || ""));\r\n' +
>     '```\r\n' +
>     '\r\n' +
>     'Stop here and inspect the emitted tree before choosing an index.\r\n' +
>     '\r\n' +
>     'Accessibility path, cell 2: one action and refresh.\r\n' +
>     '\r\n' +
>     '```js\r\n' +
>     '{\r\n' +
>     '  const observation = globalThis.state;\r\n' +
>     '  if (observation?.accessibility == null) {\r\n' +
>     '    throw new Error("No accessibility observation; reobserve before acting");\r\n' +
>     '  }\r\n' +
>     '  const elementIndex = 12; // Replace with one index from the printed accessibility tree.\r\n' +
>     '  globalThis.state = null;\r\n' +
>     '  try {\r\n' +
>     '    await sky.click({ window: observation.window, element_index: elementIndex });\r\n' +
>     '    globalThis.state = await sky.get_window_state({\r\n' +
>     '      window: observation.window,\r\n' +
>     '      include_screenshot: true,\r\n' +
>     '      include_text: true,\r\n' +
>     '    });\r\n' +
>     '  } catch (error) {\r\n' +
>     '    throw new Error("Input or refresh outcome is unknown; reobserve before retrying", {\r\n' +
>     '      cause: error,\r\n' +
>     '    });\r\n' +
>     '  }\r\n' +
>     '  globalThis.targetWindow = state.window;\r\n' +
>     '  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || ""));\r\n' +
>     '}\r\n' +
>     '```\r\n' +
>     '\r\n' +
>     'Coordinate path, cell 1: observe and inspect.\r\n' +
>     '\r\n' +
>     '```js\r\n' +
>     'globalThis.state = await sky.get_window_state({\r\n' +
>     '  window: targetWindow,\r\n' +
>     '  include_screenshot: true,\r\n' +
>     '  include_text: false,\r\n' +
>     '});\r\n' +
>     'globalThis.targetWindow = state.window;\r\n' +
>     'nodeRepl.write("Inspect the displayed screenshot, then run the coordinate action cell.");\r\n' +
>     '```\r\n' +
>     '\r\n' +
>     'Coordinate path, cell 2: one action and refresh.\r\n' +
>     '\r\n' +
>     '```js\r\n' +
>     '{\r\n' +
>     '  const observation = globalThis.state;\r\n' +
>     '  if (observation == null) {\r\n' +
>     '    throw new Error("No screenshot observation; reobserve before acting");\r\n' +
>     '  }\r\n' +
>     '  const screenshotId = observation.screenshots?.[0]?.id;\r\n' +
>     '  if (screenshotId == null) {\r\n' +
>     '    throw new Error("No screenshotId was returned by the latest screenshot observation");\r\n' +
>     '  }\r\n' +
>     '  globalThis.state = null;\r\n' +
>     '  try {\r\n' +
>     '    await sky.click({ window: observation.window, screenshotId, x: 420, y: 260 });\r\n' +
>     '    globalThis.state = await sky.get_window_state({\r\n' +
>     '      window: observation.window,\r\n' +
>     '      include_screenshot: true,\r\n' +
>     '      include_text: true,\r\n' +
>     '    });\r\n' +
>     '  } catch (error) {\r\n' +
>     '    throw new Error("Input or refresh outcome is unknown; reobserve before retrying", {\r\n' +
>     '      cause: error,\r\n' +
>     '    });\r\n' +
>     '  }\r\n' +
>     '  globalThis.targetWindow = state.window;\r\n' +
>     '  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || ""));\r\n' +
>     '}\r\n' +
>     '```\r\n' +
>     '\r\n' +
>     'For typing, observe focus first and stop. After confirming focus is correct, type in a separate cell and refresh. If typing or refresh fails, the outcome is unknown; reobserve before retrying.\r\n' +
>     '\r\n' +
>     'Focus observation cell:\r\n' +
>     '\r\n' +
>     '```js\r\n' +
>     '{\r\n' +
>     '  globalThis.state = await sky.get_window_state({\r\n' +
>     '    window: targetWindow,\r\n' +
>     '    include_screenshot: true,\r\n' +
>     '    include_text: true,\r\n' +
>     '  });\r\n' +
>     '  globalThis.targetWindow = state.window;\r\n' +
>     '  nodeRepl.write(String(state.accessibility?.focused_element || ""));\r\n' +
>     '}\r\n' +
>     '```\r\n' +
>     '\r\n' +
>     'Typing action cell:\r\n' +
>     '\r\n' +
>     '```js\r\n' +
>     '{\r\n' +
>     '  const observation = globalThis.state;\r\n' +
>     '  if (observation?.accessibility?.focused_element == null) {\r\n' +
>     '    throw new Error("No focused element observation; reobserve before typing");\r\n' +
>     '  }\r\n' +
>     '  globalThis.state = null;\r\n' +
>     '  try {\r\n' +
>     '    await sky.type_text({ window: observation.window, text: "<text>" });\r\n' +
>     '    globalThis.state = await sky.get_window_state({\r\n' +
>     '      window: observation.window,\r\n' +
>     '      include_screenshot: true,\r\n' +
>     '      include_text: true,\r\n' +
>     '    });\r\n' +
>     '  } catch (error) {\r\n' +
>     '    throw new Error("Text input or refresh outcome is unknown; reobserve before retrying", {\r\n' +
>     '      cause: error,\r\n' +
>     '    });\r\n' +

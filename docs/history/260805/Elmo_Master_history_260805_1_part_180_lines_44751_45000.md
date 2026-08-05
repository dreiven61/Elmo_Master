>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Freeplane\\freeplane.exe',
>     windows: [],
>     displayName: 'Freeplane',
>     lastUsedDate: '2026-06-09',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\CANopen Architect Mini\\CANopenArchitectMini.exe',
>     windows: [],
>     displayName: 'CANopen Architect Mini',
>     lastUsedDate: '2026-06-04',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: 'WelconServoStudio.AppID.NoVersion',
>     windows: [],
>     displayName: 'WELSS.exe',
>     lastUsedDate: '2026-06-04',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\POWERPNT.EXE',
>     windows: [],
>     displayName: 'PowerPoint 2016',
>     lastUsedDate: '2026-05-13',
>     useCount: 4,
>     isRunning: false
>   },
>   {
>     id: 'AcrobatReader',
>     windows: [],
>     displayName: 'Adobe Acrobat',
>     lastUsedDate: '2026-05-06',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Advanced IP Scanner\\advanced_ip_scanner.exe',
>     windows: [],
>     displayName: 'Advanced IP Scanner',
>     lastUsedDate: '2026-04-07',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: 'C:\\Qt\\Tools\\QtCreator\\bin\\qtcreator.exe',
>     windows: [],
>     displayName: 'Qt Creator 18.0.1 (Community)',
>     lastUsedDate: '2026-04-07',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: 'C:\\Users\\dreiv\\eclipse\\java-2023-09\\eclipse\\eclipse.exe',
>     windows: [],
>     displayName: 'Eclipse IDE for Java Developers - 2023-09',
>     lastUsedDate: '2026-04-03',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: 'C:\\ProgramData\\MDS6\\MDS Installation Guide.pdf',
>     windows: [],
>     displayName: 'MDS Installation Guide',
>     lastUsedDate: '2026-04-03',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\WSL\\wslsettings\\wslsettings.exe',
>     windows: [],
>     displayName: 'WSL Settings',
>     lastUsedDate: '2026-04-03',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX Cam Designer\\3.6.0\\ctrlX-CamDesigner.exe',
>     windows: [],
>     displayName: 'ctrlX Cam Designer 3.6.1',
>     lastUsedDate: '2026-03-13',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\ctrlX WORKS\\ctrlX DRIVE Engineering\\0130\\cxdd.exe',
>     windows: [],
>     displayName: 'ctrlX DRIVE Engineering Serial Setup 01V30.0',
>     lastUsedDate: '2026-03-03',
>     useCount: 0,
>     isRunning: false
>   }
> ]
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
>   "title": "Observe LASAL window",
>   "code": "targetWindow = {id:788822, app:'{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe', title:'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService'}; var obsLasal1 = await sky.get_window_state({window:targetWindow}); nodeRepl.write(obsLasal1);"
> }
> ```
>
> ```text
> foreground window did not report a process id
> ```
>
> <details><summary>Called 2 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect desktop control methods",
>   "code": "nodeRepl.write(Object.keys(sky).sort());"
> }
> ```
>
> ```text
> [
>   'activate_window',
>   'click',
>   'drag',
>   'get_window',
>   'get_window_state',
>   'launch_app',
>   'list_apps',
>   'list_windows',
>   'perform_secondary_action',
>   'press_key',
>   'scroll',
>   'set_value',
>   'target',
>   'transport',
>   'type_text'
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect launch operation",
>   "code": "nodeRepl.write(sky.launch_app.toString());"
> }
> ```
>
> ```text
> (...args) => {
>         const app = getComputerUseAppReference(args[0]);
>         globals.nodeRepl?.setResponseMeta({
>           [TOOL_SURFACE_META_KEY]: {
>             kind: "computerUse",
>             app,
>           },
>           ...(isChromeComputerUseAppReference(app)
>             ? { [CHROME_COMPUTER_USE_META_KEY]: true }
>             : {}),
>         });
>         return Reflect.apply(value, target, args);
>       }
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
>   "title": "Load desktop control guidance",
>   "code": "var cuGuide = await sky.documentation('guidance'); var cuApi = await sky.documentation('api'); var cuConfirm = await sky.documentation('confirmations'); nodeRepl.write(cuGuide+'\\n---API---\\n'+cuApi+'\\n---CONFIRM---\\n'+cuConfirm);"
> }
> ```
>
> ````text
> ## node_repl + @oai/sky
>
> - Use `node_repl` JavaScript for all Computer Use actions.
> - The `node_repl` state persists across calls. Store cross-cell values on `globalThis`; top-level `const` and `let` names cannot be redeclared by later retries.
> - For text output, call `nodeRepl.write(...)` with a string. Use `JSON.stringify(...)` for objects.
>
> ## Workflow
>
> ### Initialize target selection
>
> Run the Initialize setup cell from `SKILL.md` first. Then list apps and choose the target from returned app and window objects:
>
> ```js
> globalThis.apps = await sky.list_apps();
> nodeRepl.write(JSON.stringify(apps, null, 2));
> ```
>
> Never reconstruct an app or window from guessed fields. Do not call `get_window`, `activate_window`, or any input method until selection has produced exactly one returned window.
>
> ```js
> {
>   function escapeRegExp(value) {
>     return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
>   }
>
>   function returnedWindowSummary(window) {
>     return {
>       id: window.id,
>       app: window.app,
>       title: window.title,
>     };
>   }
>
>   function requireUniqueWindow(windows, label) {
>     if (windows.length !== 1) {
>       nodeRepl.write(
>         `Returned candidate windows:\n${JSON.stringify(windows.map(returnedWindowSummary), null, 2)}`,
>       );
>       throw new Error(`Expected exactly one target window for ${label}; found ${windows.length}`);
>     }
>     return windows[0];
>   }
>
>   globalThis.apps = await sky.list_apps();
>   globalThis.targetApp = apps.find((app) => app.id === "<app id>");
>   if (!targetApp) throw new Error("Target app was not returned by list_apps");
>   const targetAppId = targetApp.id;
>
>   if (targetApp.windows.length === 0) {
>     await sky.launch_app({ app: targetApp.id });
>     globalThis.apps = await sky.list_apps();
>     globalThis.targetApp = apps.find((app) => app.id === targetAppId);
>   }
>   if (!targetApp?.windows.length) {
>     throw new Error("Target app did not expose a window after launch");
>   }
>
>   const windowTitleHint = "<optional exact window title>";
>   const candidateWindows =
>     windowTitleHint === "<optional exact window title>"
>       ? targetApp.windows

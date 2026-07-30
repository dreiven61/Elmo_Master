> };
>
> type LocatorEvaluateFunction<TArg, TResult> = string | (element: Element, arg: TArg) => TResult | Promise<TResult>;
>
> type LocatorFilterOptions = {
>   has?: PlaywrightLocator;
>   hasNot?: PlaywrightLocator;
>   hasNotText?: TextMatcher;
>   hasText?: TextMatcher;
>   visible?: boolean;
> };
>
> type LocatorLocatorOptions = {
>   has?: PlaywrightLocator;
>   hasNot?: PlaywrightLocator;
>   hasNotText?: TextMatcher;
>   hasText?: TextMatcher;
> };
>
> type SelectOptionInput = string | SelectOptionDescriptor;
>
> type LocatorWaitForOptions = {
>   state: WaitForState;
>   timeoutMs?: number;
> };
>
> type FileChooserFiles = string | Array<string>;
>
> type TabClipboardItem = {
>   entries: Array<TabClipboardEntry>;
>   presentationStyle?: "unspecified" | "inline" | "attachment";
> };
>
> interface TabDevLogsOptions {
>   filter?: string; // Optional substring filter applied to the rendered log message.
>   levels?: Array<"debug" | "info" | "log" | "warn" | "error" | "warning">; // Optional levels to include.
>   limit?: number; // Maximum number of logs to return.
> }
>
> interface TabDevLogEntry {
>   level: "debug" | "info" | "log" | "warn" | "error"; // Console log level.
>   message: string; // Rendered log message text.
>   timestamp: string; // ISO 8601 timestamp for when the runtime captured the log.
>   url?: string; // Source URL reported by the browser runtime, when available.
> }
>
> interface FinalizeTabsKeep {
>   status: FinalizeTabStatus; // Where the kept tab belongs after cleanup.
>   tab: string | Tab | TabInfo; // Tab object to keep open after browser cleanup.
> }
>
> type ClipRect = {
>   height: number;
>   width: number;
>   x: number;
>   y: number;
> };
>
> type WaitUntil = LoadState | "commit";
>
> type MouseButton = "left" | "right" | "middle";
>
> type KeyboardModifier = "Alt" | "Control" | "ControlOrMeta" | "Meta" | "Shift";
>
> type SelectOptionDescriptor = {
>   index?: number;
>   label?: string;
>   value?: string;
> };
>
> type WaitForState = "attached" | "detached" | "visible" | "hidden";
>
> type TabClipboardEntry = {
>   base64?: string;
>   mimeType: string;
>   text?: string;
> };
>
> type FinalizeTabStatus = "handoff" | "deliverable";
> ```
> ````
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "반응형 화면 설정 확인",
>   "timeout_ms": 30000,
>   "code": "nodeRepl.write(await browser.capabilities.list());"
> }
> ```
>
> ```text
> [
>   {
>     id: 'visibility',
>     description: "Use to show or hide the browser to the user, and to determine the browser's current visibility. Keep browser work in the background unless the user asks to see it or live viewing is useful. When the browser should be visible, call set(true)."
>   },
>   {
>     id: 'viewport',
>     description: 'Controls an explicit browser viewport override for responsive or device-size testing. Use it when a task calls for specific dimensions or breakpoint validation; otherwise leave it unset so the browser uses its normal viewport. Reset temporary overrides before finishing unless the user asked to keep them.'
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "반응형 화면 크기 설정",
>   "timeout_ms": 30000,
>   "code": "var viewportCap = await browser.capabilities.get(\"viewport\");\nnodeRepl.write(await viewportCap.documentation());"
> }
> ```
>
> ````text
> # Browser Capability: viewport
> Browser viewport override control. Do not set the viewport during normal browser setup; most tasks should use the existing/default viewport. Use `set()` only when the user asks for specific dimensions, asks to test a responsive breakpoint or device size, or the task cannot be answered correctly without a specific viewport. Do not resize the browser just to make a screenshot larger, prettier, or fit more content. Use the default viewport, a normal screenshot, or a full-page screenshot instead. If you set a temporary viewport, call `reset()` before finishing unless the user asked to keep that viewport.
>
> ```ts
> const capability = await browser.capabilities.get("viewport");
>
> interface ViewportSize {
>   height: number;
>   width: number;
> }
>
> interface ViewportBrowserCapability {
>   reset(): Promise<void>; // Clear the explicit viewport override and return to default browser sizing.
>   set(options: ViewportSize): Promise<void>; // Apply an explicit browser viewport override.
> }
> ```
> ````
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "진행 대시보드 렌더링 확인",
>   "timeout_ms": 30000,
>   "code": "var progressTab = await browser.tabs.new();\nawait progressTab.goto(\"http://127.0.0.1:8765/API_DEVELOPMENT_PROGRESS_2026-07-30.html\");\nawait progressTab.playwright.waitForLoadState({state:\"domcontentloaded\", timeoutMs:10000});\nvar progressSummary = await progressTab.playwright.evaluate(() => ({\n  title: document.title,\n  h1: document.querySelector(\"h1\")?.innerText,\n  sectionCount: document.querySelectorAll(\"main section\").length,\n  cardCount: document.querySelectorAll(\".card\").length,\n  linkCount: document.querySelectorAll(\"a\").length,\n  viewport: {width: window.innerWidth, height: window.innerHeight},\n  overflow: {scrollWidth: document.documentElement.scrollWidth, clientWidth: document.documentElement.clientWidth},\n  firstCard: document.querySelector(\".card\")?.innerText\n}));\nnodeRepl.write(progressSummary);\nawait nodeRepl.emitImage(await progressTab.screenshot({fullPage:false}));"
> }
> ```
>
> ```text
> {
>   cardCount: 8,
>   firstCard: '완전·적응 구현\n40 / 65\n\n요구사항 목적 기준 61.5%',
>   h1: 'Elmo Master API\n개발 진행 현황',
>   linkCount: 12,
>   overflow: { clientWidth: 1265, scrollWidth: 1265 },
>   sectionCount: 6,
>   title: 'Elmo Master API 개발 진행 현황 - 2026-07-30',
>   viewport: { height: 720, width: 1280 }
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "모바일 폭 레이아웃 확인",
>   "timeout_ms": 30000,
>   "code": "await viewportCap.set({width:390,height:844});\nawait progressTab.reload();\nawait progressTab.playwright.waitForLoadState({state:\"domcontentloaded\", timeoutMs:10000});\nvar mobileProgressSummary = await progressTab.playwright.evaluate(() => ({\n  viewport: {width: window.innerWidth, height: window.innerHeight},\n  overflow: {scrollWidth: document.documentElement.scrollWidth, clientWidth: document.documentElement.clientWidth},\n  cardsColumns: getComputedStyle(document.querySelector(\".cards\")).gridTemplateColumns,\n  blockersColumns: getComputedStyle(document.querySelector(\".blockers\")).gridTemplateColumns,\n  h1Size: getComputedStyle(document.querySelector(\"h1\")).fontSize\n}));\nnodeRepl.write(mobileProgressSummary);\nawait nodeRepl.emitImage(await progressTab.screenshot({fullPage:false}));"
> }
> ```
>
> ```text
> {
>   blockersColumns: '301.031px',
>   cardsColumns: '336.625px',
>   h1Size: '32px',
>   overflow: { clientWidth: 375, scrollWidth: 375 },
>   viewport: { height: 844, width: 390 }
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "개발 계획 화면 확인",
>   "timeout_ms": 30000,
>   "code": "await viewportCap.reset();\nawait progressTab.goto(\"http://127.0.0.1:8765/API_DEVELOPMENT_PLAN_2026-07-30.html\");\nawait progressTab.playwright.waitForLoadState({state:\"domcontentloaded\", timeoutMs:10000});\nvar planSummary = await progressTab.playwright.evaluate(() => ({\n  title: document.title,\n  h1: document.querySelector(\"h1\")?.innerText,\n  milestoneCount: document.querySelectorAll(\".mile\").length,\n  runbookCount: document.querySelectorAll(\".runbook article\").length,\n  checklistCount: document.querySelectorAll(\".checklist li\").length,\n  overflow: {scrollWidth: document.documentElement.scrollWidth, clientWidth: document.documentElement.clientWidth},\n  viewport: {width: window.innerWidth, height: window.innerHeight}\n}));\nnodeRepl.write(planSummary);\nawait nodeRepl.emitImage(await progressTab.screenshot({fullPage:false}));"
> }
> ```
>
> ```text
> {
>   checklistCount: 11,
>   h1: 'Elmo Master API\n개발 계획',
>   milestoneCount: 6,
>   overflow: { clientWidth: 1265, scrollWidth: 1265 },
>   runbookCount: 9,
>   title: 'Elmo Master API 개발 계획 - 2026-07-30',
>   viewport: { height: 720, width: 1280 }
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "개발 계획 모바일 레이아웃 확인",
>   "timeout_ms": 30000,
>   "code": "await viewportCap.set({width:390,height:844});\nawait progressTab.reload();\nawait progressTab.playwright.waitForLoadState({state:\"domcontentloaded\", timeoutMs:10000});\nvar mobilePlanSummary = await progressTab.playwright.evaluate(() => ({\n  overflow: {scrollWidth: document.documentElement.scrollWidth, clientWidth: document.documentElement.clientWidth},\n  roadmapColumns: getComputedStyle(document.querySelector(\".roadmap\")).gridTemplateColumns,\n  goalColumns: getComputedStyle(document.querySelector(\".goal\")).gridTemplateColumns,\n  h1Size: getComputedStyle(document.querySelector(\"h1\")).fontSize\n}));\nnodeRepl.write(mobilePlanSummary);\nawait nodeRepl.emitImage(await progressTab.screenshot({fullPage:false}));"
> }
> ```
>
> ```text
> {
>   goalColumns: '336.625px',
>   h1Size: '32px',
>   overflow: { clientWidth: 375, scrollWidth: 390 },
>   roadmapColumns: '301.031px'
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "모바일 가로 넘침 원인 확인",
>   "timeout_ms": 30000,
>   "code": "var overflowNodes = await progressTab.playwright.evaluate(() => {\n  const limit = document.documentElement.clientWidth;\n  return Array.from(document.querySelectorAll(\"body *\"))\n    .map((el) => {\n      const r = el.getBoundingClientRect();\n      return {tag: el.tagName, cls: el.className || \"\", left: Math.round(r.left), right: Math.round(r.right), width: Math.round(r.width)};\n    })\n    .filter((x) => x.right > limit + 1 || x.left < -1)\n    .slice(0, 20);\n});\nnodeRepl.write(overflowNodes);"
> }
> ```
>

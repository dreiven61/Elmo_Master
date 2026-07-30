>
> interface DomCUAAPI {
>   click(options: DomClickOptions): Promise<void>; // Click a DOM node by its id from the visible DOM snapshot.
>   double_click(options: DomClickOptions): Promise<void>; // Double-click a DOM node by its id.
>   get_visible_dom(): Promise<unknown>; // Return a filtered DOM with node ids for interactable elements.
>   keypress(options: DomKeypressOptions): Promise<void>; // Press control characters at the currently focused element (focus it first via click/dblclick).
>   scroll(options: DomScrollOptions): Promise<void>; // Scroll either the page or a specific node (if node_id provided) by deltas.
>   type(options: DomTypeOptions): Promise<void>; // Type text into the currently focused element (focus via click first).
> }
>
> interface PlaywrightAPI {
>   domSnapshot(): Promise<string>; // Return a snapshot of the current DOM as a string, including expanded iframe body content when available.
>   evaluate<TResult, TArg>(pageFunction: PlaywrightEvaluateFunction<TArg, TResult>, arg?: TArg, options?: PlaywrightEvaluateOptions): Promise<TResult>; // Evaluate JavaScript in a read-only page scope.
>   expectNavigation<T>(action: () => Promise<T>, options: { timeoutMs?: number; url?: string; waitUntil?: LoadState }): Promise<T>; // Expect a navigation triggered by an action.
>   frameLocator(frameSelector: string): PlaywrightFrameLocator; // Create a frame-scoped locator builder.
>   getByLabel(text: TextMatcher, options: { exact?: boolean }): PlaywrightLocator; // Find elements by label text within the page.
>   getByPlaceholder(text: TextMatcher, options: { exact?: boolean }): PlaywrightLocator; // Find elements by placeholder text within the page.
>   getByRole(role: string, options: { exact?: boolean; name?: TextMatcher }): PlaywrightLocator; // Find elements by ARIA role within the page.
>   getByTestId(testId: string): PlaywrightLocator; // Find elements by test id within the page.
>   getByText(text: TextMatcher, options: { exact?: boolean }): PlaywrightLocator; // Find elements by text within the page.
>   locator(selector: string): PlaywrightLocator; // Create a locator scoped to this tab.
>   waitForEvent(event: "download", options?: WaitForEventOptions): Promise<PlaywrightDownload>; // Wait for the next event on the page.
>   waitForEvent(event: "filechooser", options?: WaitForEventOptions): Promise<PlaywrightFileChooser>;
>   waitForLoadState(options: PageWaitForLoadStateOptions): Promise<void>; // Wait for the page to reach a specific load state.
>   waitForTimeout(timeoutMs: number): Promise<void>; // Wait for a fixed duration.
>   waitForURL(url: string, options: PageWaitForURLOptions): Promise<void>; // Wait for the page URL to match the provided value.
> }
>
> interface PlaywrightFrameLocator {
>   frameLocator(frameSelector: string): PlaywrightFrameLocator; // Create a locator scoped to a nested frame.
>   getByLabel(text: TextMatcher, options: { exact?: boolean }): PlaywrightLocator; // Find elements by label within this frame.
>   getByPlaceholder(text: TextMatcher, options: { exact?: boolean }): PlaywrightLocator; // Find elements by placeholder within this frame.
>   getByRole(role: string, options: { exact?: boolean; name?: TextMatcher }): PlaywrightLocator; // Find elements by ARIA role within this frame.
>   getByTestId(testId: string): PlaywrightLocator; // Find elements by test id within this frame.
>   getByText(text: TextMatcher, options: { exact?: boolean }): PlaywrightLocator; // Find elements by text within this frame.
>   locator(selector: string): PlaywrightLocator; // Create a locator scoped to this frame.
> }
>
> interface PlaywrightLocator {
>   all(): Promise<Array<PlaywrightLocator>>; // Resolve to a list of locators for each matched element.
>   allTextContents(options: { timeoutMs?: number }): Promise<Array<string>>; // Return `textContent` for *all* elements matched by this locator.
>   and(locator: PlaywrightLocator): PlaywrightLocator; // Return a locator matching elements that satisfy both this locator and `locator`.
>   check(options: LocatorCheckOptions): Promise<void>; // Check a checkbox or switch-like control.
>   click(options: LocatorClickOptions): Promise<void>; // Click the element matched by this locator.
>   count(): Promise<number>; // Number of elements matching this locator.
>   dblclick(options: LocatorClickOptions): Promise<void>; // Double-click the element matched by this locator.
>   downloadMedia(options: LocatorDownloadMediaOptions): Promise<void>; // Trigger a download for the media or file link in the first matched element.
>   evaluate<TResult, TArg>(pageFunction: LocatorEvaluateFunction<TArg, TResult>, arg?: TArg, options?: PlaywrightEvaluateOptions): Promise<TResult>; // Evaluate JavaScript in a read-only scope; the locator must resolve unambiguously to one element.
>   fill(value: string, options: { timeoutMs?: number }): Promise<void>; // Replace the element's value with the provided text.
>   filter(options: LocatorFilterOptions): PlaywrightLocator; // Narrow this locator by additional constraints.
>   first(): PlaywrightLocator; // Return a locator pointing at the first matched element.
>   getAttribute(name: string, options: { timeoutMs?: number }): Promise<null | string>; // Return an attribute value from the first matched element.
>   getByLabel(text: TextMatcher, options: { exact?: boolean }): PlaywrightLocator; // Find elements by label text, scoped to this locator.
>   getByPlaceholder(text: TextMatcher, options: { exact?: boolean }): PlaywrightLocator; // Find elements by placeholder text, scoped to this locator.
>   getByRole(role: string, options: { exact?: boolean; name?: TextMatcher }): PlaywrightLocator; // Find elements by ARIA role, scoped to this locator.
>   getByTestId(testId: string): PlaywrightLocator; // Find elements by test id, scoped to this locator.
>   getByText(text: TextMatcher, options: { exact?: boolean }): PlaywrightLocator; // Find elements by text content, scoped to this locator.
>   innerText(options: { timeoutMs?: number }): Promise<string>; // Return the rendered (visible) text of the first matched element.
>   isEnabled(): Promise<boolean>; // Whether the first matched element is currently enabled.
>   isVisible(): Promise<boolean>; // Whether the first matched element is currently visible.
>   last(): PlaywrightLocator; // Return a locator pointing at the last matched element.
>   locator(selector: string, options: LocatorLocatorOptions): PlaywrightLocator; // Create a descendant locator scoped to this locator.
>   nth(index: number): PlaywrightLocator; // Return a locator pointing at the Nth matched element.
>   or(locator: PlaywrightLocator): PlaywrightLocator; // Return a locator matching elements that satisfy either this locator or `locator`.
>   press(value: string, options: { timeoutMs?: number }): Promise<void>; // Press a keyboard key while this locator is focused.
>   selectOption(value: SelectOptionInput | Array<SelectOptionInput>, options: { timeoutMs?: number }): Promise<void>; // Select one or more options on a native `<select>` element.
>   setChecked(checked: boolean, options: LocatorCheckOptions): Promise<void>; // Set a checkbox or switch-like control to a checked/unchecked state.
>   textContent(options: { timeoutMs?: number }): Promise<null | string>; // Return the raw textContent of the first matched element (or null if missing).
>   type(value: string, options: { timeoutMs?: number }): Promise<void>; // Type text into the element without clearing existing content.
>   uncheck(options: LocatorCheckOptions): Promise<void>; // Uncheck a checkbox or switch-like control.
>   waitFor(options: LocatorWaitForOptions): Promise<void>; // Wait for the element to reach a specific state.
> }
>
> interface PlaywrightDownload {
> }
>
> interface PlaywrightFileChooser {
>   isMultiple(): boolean; // Whether the input allows selecting multiple files.
>   setFiles(files: FileChooserFiles, options: { timeoutMs?: number }): Promise<void>; // Set the files for this chooser.
> }
>
> interface TabClipboardAPI {
>   read(): Promise<Array<TabClipboardItem>>; // Read clipboard items, including text and binary payloads.
>   readText(): Promise<string>; // Read plain text from the browser clipboard.
>   write(items: Array<TabClipboardItem>): Promise<void>; // Write clipboard items.
>   writeText(text: string): Promise<void>; // Write plain text to the browser clipboard.
> }
>
> interface TabDevAPI {
>   logs(options: TabDevLogsOptions): Promise<Array<TabDevLogEntry>>; // Read console log messages captured for this tab.
> }
>
> interface AlertDialog {
>   type: "alert";
>   dismiss(): Promise<void>;
> }
>
> interface BeforeUnloadDialog {
>   type: "beforeunload";
>   dismiss(): Promise<void>;
> }
>
> interface ConfirmDialog {
>   type: "confirm";
>   accept(): Promise<void>;
>   dismiss(): Promise<void>;
> }
>
> interface Documentation {
>   get(name: string): Promise<string>; // Read packaged documentation by its extensionless relative path.
> }
>
> interface PromptDialog {
>   type: "prompt";
>   accept(text: string): Promise<void>;
>   dismiss(): Promise<void>;
> }
>
> type BrowserCapabilityCollection = {
>   get(id: string): Promise<unknown>;
>   list(): Promise<Array<{ id: string; description: string }>>;
> };
>
> interface BrowserUserTabInfo {
>   id: string; // Opaque identifier for this browser tab.
>   lastOpened?: string; // ISO 8601 timestamp for the last time the tab was opened or focused.
>   providerTabId?: string; // Provider-owned identity for correlating an explicit reference with this fresh listing.
>   tabGroup?: string; // User-visible tab group name when the tab belongs to one.
>   title?: string; // User-visible tab title.
>   url?: string; // Current tab URL.
> }
>
> interface FinalizeTabsOptions {
>   keep?: Array<FinalizeTabsKeep>; // Explicit tab dispositions to preserve after cleanup.
> }
>
> interface TabInfo {
>   id: string; // Metadata describing an open tab.
>   title?: string;
>   url?: string;
> }
>
> type TabCapabilityCollection = {
>   get(id: string): Promise<unknown>;
>   list(): Promise<Array<{ id: string; description: string }>>;
> };
>
> type Dialog = AlertDialog | BeforeUnloadDialog | ConfirmDialog | PromptDialog;
>
> type ScreenshotOptions = {
>   clip?: ClipRect; // Crop to a specific rectangle instead of the full viewport.
>   fullPage?: boolean; // Capture the full page instead of the viewport.
> };
>
> type ClickOptions = {
>   button?: number; // Mouse button (1-left, 2-middle/wheel, 3-right, 4-back, 5-forward).
>   keypress?: Array<string>; // Modifier keys held during the click.
>   x: number;
>   y: number;
> };
>
> type DoubleClickOptions = {
>   keypress?: Array<string>; // Modifier keys held during the double click.
>   x: number;
>   y: number;
> };
>
> type DragOptions = {
>   keys?: Array<string>; // Optional modifier keys held during the drag.
>   path: Array<{ x: number; y: number }>; // Drag path as a list of points.
> };
>
> type KeypressOptions = {
>   keys: Array<string>; // Key combination to press.
> };
>
> type MoveOptions = {
>   keys?: Array<string>; // Optional modifier keys held while moving.
>   x: number;
>   y: number;
> };
>
> type ScrollOptions = {
>   keypress?: Array<string>; // Modifier keys held during scroll.
>   scrollX: number;
>   scrollY: number;
>   x: number;
>   y: number;
> };
>
> type TypeOptions = {
>   text: string;
> };
>
> type DomClickOptions = {
>   node_id: string; // Node id from `get_visible_dom()`.
> };
>
> type DomKeypressOptions = {
>   keys: Array<string>; // Key combination to press.
> };
>
> type DomScrollOptions = {
>   node_id?: string; // Optional node id to scroll within.
>   x: number; // Horizontal scroll delta.
>   y: number; // Vertical scroll delta.
> };
>
> type DomTypeOptions = {
>   text: string; // Text to type into the currently focused element.
> };
>
> type PlaywrightEvaluateFunction<TArg, TResult> = string | (arg: TArg) => TResult | Promise<TResult>;
>
> type PlaywrightEvaluateOptions = {
>   timeoutMs?: number; // Maximum time to spend setting up the read-only DOM scope and running the script.
> };
>
> type LoadState = "load" | "domcontentloaded" | "networkidle";
>
> type TextMatcher = string | RegExp;
>
> type WaitForEventOptions = {
>   timeoutMs?: number;
> };
>
> type PageWaitForLoadStateOptions = {
>   state?: LoadState;
>   timeoutMs?: number;
> };
>
> type PageWaitForURLOptions = {
>   timeoutMs?: number;
>   waitUntil?: WaitUntil;
> };
>
> type LocatorCheckOptions = {
>   force?: boolean;
>   timeoutMs?: number;
> };
>
> type LocatorClickOptions = {
>   button?: MouseButton;
>   force?: boolean;
>   modifiers?: Array<KeyboardModifier>;
>   timeoutMs?: number;
> };
>
> type LocatorDownloadMediaOptions = {
>   timeoutMs?: number;

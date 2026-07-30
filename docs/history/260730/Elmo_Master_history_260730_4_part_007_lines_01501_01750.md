> ```text
> [
>   { cls: '', left: 37, right: 390, tag: 'ARTICLE', width: 353 },
>   { cls: '', left: 37, right: 390, tag: 'ARTICLE', width: 353 },
>   { cls: '', left: 37, right: 390, tag: 'ARTICLE', width: 353 },
>   { cls: '', left: 37, right: 390, tag: 'ARTICLE', width: 353 },
>   { cls: '', left: 37, right: 390, tag: 'ARTICLE', width: 353 },
>   { cls: '', left: 37, right: 390, tag: 'ARTICLE', width: 353 },
>   { cls: '', left: 37, right: 390, tag: 'ARTICLE', width: 353 },
>   { cls: '', left: 37, right: 390, tag: 'ARTICLE', width: 353 },
>   { cls: '', left: 37, right: 390, tag: 'ARTICLE', width: 353 },
>   { cls: '', left: 70, right: 1044, tag: 'CODE', width: 974 },
>   { cls: '', left: 38, right: 858, tag: 'TABLE', width: 820 },
>   { cls: '', left: 38, right: 858, tag: 'THEAD', width: 820 },
>   { cls: '', left: 38, right: 858, tag: 'TR', width: 820 },
>   { cls: '', left: 85, right: 394, tag: 'TH', width: 309 },
>   { cls: '', left: 394, right: 858, tag: 'TH', width: 464 },
>   { cls: '', left: 38, right: 858, tag: 'TBODY', width: 820 },
>   { cls: '', left: 38, right: 858, tag: 'TR', width: 820 },
>   { cls: '', left: 85, right: 394, tag: 'TD', width: 309 },
>   { cls: '', left: 394, right: 858, tag: 'TD', width: 464 },
>   { cls: '', left: 38, right: 858, tag: 'TR', width: 820 }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "가로 스크롤 컨테이너 확인",
>   "timeout_ms": 30000,
>   "code": "var widthDiagnostics = await progressTab.playwright.evaluate(() => ({\n  html: {client: document.documentElement.clientWidth, scroll: document.documentElement.scrollWidth},\n  body: {client: document.body.clientWidth, scroll: document.body.scrollWidth},\n  main: {client: document.querySelector(\"main\").clientWidth, scroll: document.querySelector(\"main\").scrollWidth},\n  sections: Array.from(document.querySelectorAll(\"section\")).map((el, i) => ({i, cls: el.className, client: el.clientWidth, scroll: el.scrollWidth})),\n  wraps: Array.from(document.querySelectorAll(\".table-wrap, details, pre\")).map((el, i) => ({i, tag:el.tagName, cls:el.className, client:el.clientWidth, scroll:el.scrollWidth, overflow:getComputedStyle(el).overflowX}))\n}));\nnodeRepl.write(widthDiagnostics);"
> }
> ```
>
> ```text
> {
>   body: { client: 375, scroll: 390 },
>   html: { client: 375, scroll: 390 },
>   main: { client: 375, scroll: 390 },
>   sections: [
>     { client: 335, cls: '', i: 0, scroll: 335 },
>     { client: 335, cls: '', i: 1, scroll: 335 },
>     { client: 335, cls: '', i: 2, scroll: 370 },
>     { client: 335, cls: '', i: 3, scroll: 355 },
>     { client: 335, cls: '', i: 4, scroll: 335 },
>     { client: 335, cls: '', i: 5, scroll: 335 },
>     { client: 335, cls: '', i: 6, scroll: 335 },
>     { client: 335, cls: '', i: 7, scroll: 335 },
>     { client: 335, cls: '', i: 8, scroll: 335 }
>   ],
>   wraps: [
>     {
>       client: 299,
>       cls: '',
>       i: 0,
>       overflow: 'visible',
>       scroll: 299,
>       tag: 'DETAILS'
>     },
>     {
>       client: 267,
>       cls: '',
>       i: 1,
>       overflow: 'auto',
>       scroll: 1006,
>       tag: 'PRE'
>     },
>     {
>       client: 299,
>       cls: 'table-wrap',
>       i: 2,
>       overflow: 'auto',
>       scroll: 820,
>       tag: 'DIV'
>     },
>     {
>       client: 299,
>       cls: 'table-wrap',
>       i: 3,
>       overflow: 'auto',
>       scroll: 820,
>       tag: 'DIV'
>     },
>     {
>       client: 299,
>       cls: 'table-wrap',
>       i: 4,
>       overflow: 'auto',
>       scroll: 820,
>       tag: 'DIV'
>     }
>   ]
> }
> ```
>
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+9 -4)
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "모바일 가로 넘침 재검증",
>   "timeout_ms": 30000,
>   "code": "await progressTab.reload();\nawait progressTab.playwright.waitForLoadState({state:\"domcontentloaded\", timeoutMs:10000});\nvar mobilePlanRetest = await progressTab.playwright.evaluate(() => ({\n  overflow: {scrollWidth: document.documentElement.scrollWidth, clientWidth: document.documentElement.clientWidth},\n  main: {client: document.querySelector(\"main\").clientWidth, scroll: document.querySelector(\"main\").scrollWidth},\n  sectionOverflows: Array.from(document.querySelectorAll(\"section\")).map((el, i) => ({i, client:el.clientWidth, scroll:el.scrollWidth})).filter(x => x.scroll > x.client)\n}));\nnodeRepl.write(mobilePlanRetest);\nawait nodeRepl.emitImage(await progressTab.screenshot({fullPage:false}));"
> }
> ```
>
> ```text
> {
>   main: { client: 375, scroll: 375 },
>   overflow: { clientWidth: 375, scrollWidth: 375 },
>   sectionOverflows: []
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
>   "title": "브라우저 검증 마무리",
>   "timeout_ms": 30000,
>   "code": "var browserLogs = await progressTab.dev.logs({levels:[\"error\",\"warn\"], limit:50});\nawait viewportCap.reset();\nnodeRepl.write({consoleWarningsOrErrors: browserLogs.length, logs: browserLogs});\nawait browser.tabs.finalize({});"
> }
> ```
>
> ```text
> { consoleWarningsOrErrors: 0, logs: [] }
> ```
>
> </details>
>
> HTML 렌더링은 데스크톱·모바일 모두 확인했고, 계획 화면에서 발견한 모바일 가로 스크롤도 수정했습니다. 이제 네 파일의 링크, UTF-8/HTML 구조, Markdown↔HTML 수치 일치, diff whitespace를 최종 검사하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+6 -1)
>
> </details>
>
> 네 문서는 생성됐고, 링크·UTF-8·HTML 구조·데스크톱/모바일 렌더링은 통과했습니다. 현재는 별도 검토에서 숫자·build 명령·P0 순서가 소스 근거와 일치하는지만 마지막으로 확인 중입니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+1 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+4 -3)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+2 -2)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+5 -4)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 문서 4개는 초안과 시각 검수까지 끝났습니다. 지금은 계획 문서에서 “이번 릴리스 필수 범위”와 “후속 선택 범위”가 섞이지 않도록 게이트·제외 조건을 마지막으로 정합화하고 있습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+76 -26)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+48 -22)
>
> </details>
>
> 마지막 정합화에서 생산 릴리스의 핵심 모순을 바로잡았습니다. 고급 I/O·Write 기능은 이번 릴리스의 필수 선행조건이 아니라 “명시적으로 제외 가능한 후속 범위”로 분리했고, 대신 현재 active 범위의 성능·토폴로지·same-peer·orphan 복구 게이트를 생산 승인 조건에 넣었습니다.
>
> 추가 확인 결과, 문서 작성 중에도 API/WPF 테스트 소스가 09:32까지 계속 바뀌었습니다. 따라서 기존 941/941·175/175를 “현재 확정 PASS”로 올리지 않고, 이전 해시에서의 마지막 관측값과 이후 미검증 변경을 분리해 표시하겠습니다. 최신 스냅샷에 대해 빠른 회귀를 한 번 더 병렬 확인 중입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+1 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+7 -2)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+1 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+1 -1)
>
> </details>
>
> 최신 재스캔에서도 핵심 진행도 숫자는 유지됐습니다: 요구사항 40/65 완전·적응, 50/65 부분 포함, wire 62 / LASAL dispatcher 59 / active 53입니다. 새 Stop/Reset wait·recovery API는 기존 opcode를 재사용하는 PC 편의 계층이라 wire 수에는 포함되지 않습니다. 계획 문서의 실행 명령도 한글 경로를 포함해 실제 동작하는 fingerprint로 교정했습니다.
>
> 최신 회귀 배치는 SDK Release를 지나 WPF smoke와 LASAL 정적 계약까지 한 묶음으로 실행 중입니다. 시작·종료 fingerprint가 다르면 PASS 숫자가 나오더라도 확정 기준으로 쓰지 않고 “관측값”으로만 남깁니다.
>
> 최신 SDK Release 회귀가 이전 배치보다 비정상적으로 오래 걸리고 있어, 이것 자체도 최신 변경의 미확정 신호로 보고 있습니다. 일정 시간 안에 종료되지 않으면 억지로 PASS로 해석하지 않고 “최신 배치 미완료/timeout”으로 진행 문서에 기록하겠습니다.
>
> 09:43에도 WPF recovery 소스가 바뀌어 최신 배치의 시작 fingerprint가 이미 무효가 됐습니다. 그래서 진행 중이던 stale WPF 시험 프로세스만 종료했고, 문서에는 기존 완료 배치와 이번 미완료 재검증을 명확히 분리해 기록합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+5 -5)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+12 -6)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+3 -3)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+2 -2)
>
> </details>
>
> 문서 내용은 최신 09:46 스냅샷으로 고정했습니다. 이제 네 파일의 링크, HTML 파싱, MD/HTML 핵심 게이트 일치, 줄끝/공백, Git 상태를 최종 확인하고 종료하겠습니다.
</details>

완료했습니다. 생성한 파일은 4개입니다.

- [개발 진행 현황 MD](./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md)
- [개발 진행 대시보드 HTML](./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html)
- [개발 계획 MD](./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md)
- [개발 계획 로드맵 HTML](./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html)

현재 판정은 다음과 같습니다.

- 완전·적응 구현: 40/65, 61.5%
- 부분 구현 포함: 50/65, 76.9%
- C# wire / LASAL dispatcher / active: 62 / 59 / 53
- 마지막 완료 관측: SDK 941/941, WPF 175/175
- LASAL SourceOnly PASS, full static FAIL
- PLC·실축 전체 검증 미완료
- 생산 배포: NO-GO

09:45까지 소스 변경이 계속돼 자동시험 수치는 동일 해시의 최종 증거가 아니라는 점도 문서에 명시했습니다. HTML 기본 레이아웃은 데스크톱·모바일에서 확인했고, 최종 HTML 파싱과 모든 상대 링크 검사를 통과했습니다.

새 문서에는 공백 문제가 없습니다. 다만 `git diff --cached --check`는 기존 staged 파일인 `docs/history/Elmo_Master_history_260721.md`의 trailing whitespace 때문에 계속 실패합니다. 해당 사용자 파일은 수정하지 않았습니다.

> 지금 개발된 api 설명하는 설명서 추가된 api도 추가해서 만들어줘
>
> User context
>
> Attachments:
> - LASAL_Motion_Control_API_User_Manual_KO.pdf: `./LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf`
> - LASAL_Motion_Control_API_User_Manual_KO.docx: `./LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.docx`

<details><summary>10 previous messages</summary>

> 기존 DOCX를 원본으로 삼아 현재 public API와 대조한 뒤, 빠진 신규 API 설명을 같은 서식으로 추가하고 PDF도 다시 생성하겠습니다. 문서 편집에는 `doc` 스킬을, PDF 렌더링·페이지 검수에는 `pdf` 스킬을 사용하겠습니다. 먼저 두 스킬 지침과 현재 문서 구조를 확인합니다.
>
> 기존 Word/PDF는 7월 16일판이지만, 저장소의 원본 Markdown에는 7월 29일까지 추가된 Axis Power/PowerOff/Reset/Stop 및 Group Enable/Stop 안정상태 대기·재개 API가 이미 작성돼 있습니다. 이 원본을 보존해 재생성하되, 현재 메타데이터와 PDF 표지 날짜가 오래된 문제도 함께 바로잡겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManual.py` (+21 -6)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+26 -6)
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManual.py` (+44 -0)
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManualDocx.py` (+34 -0)
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManualDocx.py` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+60 -0)
>

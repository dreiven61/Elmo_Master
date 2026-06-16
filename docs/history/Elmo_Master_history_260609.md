# 히스토리 파일 분할 및 분석

> 이 파일을 분석해서 이 쓰레드에서 작업을 이어 진행하도록하자. 히스토리 파일이 너무 크니까 날짜폴더 만들어서 니가 읽어들이기 편한 사이즈로 잘라서 나눠서 저장하고 개별 파일을 읽어 들여서 내용 분석해.
>
> User context
>
> Attachments:
> - Elmo_Master_history_260526.md: `./docs/history/Elmo_Master_history_260526.md`

<details><summary>12 previous messages</summary>

> 히스토리 파일 구조와 크기부터 확인한 뒤, 원본은 건드리지 않고 `docs/history` 아래 날짜 폴더에 분할본을 만들겠습니다. 이후 분할본을 순서대로 읽어서 이어받아야 할 작업 맥락을 정리하겠습니다.
>
> 원본은 약 1.9MB, 5,618줄입니다. 한 번에 읽을 수는 있지만 누적 맥락에는 부담이 있으니 700줄 단위로 9개 조각과 인덱스를 만들겠습니다.
>
> 이제 분할 파일을 생성합니다. 원본 파일은 그대로 두고, 각 조각에는 원본 경로와 줄 범위를 헤더로 붙여 나중에 역추적하기 쉽게 만들겠습니다.
>
> 첫 분할 스크립트가 PowerShell 타입 변환에서 실패해서 헤더만 있는 조각 파일들이 생겼습니다. 이건 제가 만든 잘못된 출력물이므로 같은 폴더 안에서 바로 덮어써서 정상 분할본으로 교체하겠습니다.
>
> 700줄 기준은 줄 길이가 너무 들쭉날쭉해서 일부 조각이 500KB를 넘습니다. 읽기 편한 크기라는 요구에는 부적합하니, 줄 수가 아니라 문자량 기준으로 다시 쪼개겠습니다.
>
> 원본 4,229번째 줄 하나가 약 477K 문자입니다. 줄 단위 분할만으로는 읽기 쉬운 조각을 만들 수 없어서, 긴 줄은 별도 세그먼트로 나누는 방식으로 다시 저장하겠습니다.
>
> 긴 줄 대부분은 `data:image/png;base64,...`로 박힌 스크린샷입니다. 원본은 그대로 보존하고, 분석용 분할본에서는 base64를 PNG 파일로 추출한 뒤 링크로 대체하겠습니다. 이 방식이 실제로 읽고 분석하기 가장 좋습니다.
>
> 분할본은 분석용으로 3개 조각이 됐고, 스크린샷 8개는 `assets`로 뺐습니다. 이제 각 조각에서 핵심 요구, 결론, 이어갈 TODO, 근거 데이터를 뽑겠습니다.
>
> 핵심 흐름은 잡혔습니다. 마지막 미완료 작업이 PDF 책갈피 재연결 쪽으로 보이므로, 해당 구간은 따로 더 읽어서 현재 상태가 끝났는지 중단됐는지 확인하겠습니다.
>
> 분석상 가장 중요한 정정점은 `ReadActualPosition`입니다. 이전 중간 문서에는 20바이트 응답이라고 남은 부분이 있지만, 이후 반복 캡처 검증에서는 24바이트 응답이 일관되게 확인됐습니다. 요약 파일에도 이처럼 “최종 기준”과 “낡은 중간 결론”을 분리해서 적겠습니다.
>
> 작업 재개용 요약 문서를 날짜 폴더 안에 추가하겠습니다. 앞으로 이 쓰레드에서는 원본 히스토리 전체를 다시 읽지 않고 이 요약과 분할본만 참조하면 됩니다.
>
> <details><summary>Created 1 file</summary>
>
> - Wrote `./docs/history/260526/analysis_summary.md` (+113 -0)
>
> </details>
</details>

분석용 분할 완료했습니다. 원본은 건드리지 않았습니다.

생성 위치:
- [index.md](/C:/work/Elmo/Elmo_Master/docs/history/260526/index.md)
- [analysis_summary.md](/C:/work/Elmo/Elmo_Master/docs/history/260526/analysis_summary.md)
- 분할본 3개: `docs/history/260526/Elmo_Master_history_260526_part_*.md`
- base64 이미지 8개 추출: `docs/history/260526/assets`

핵심 분석 결과:
- 마지막으로 이어갈 미완료 작업은 `Maestro Administrative and Motion API_2022_12_v2.012.pdf`의 깨진 책갈피 링크 복구입니다.
- 이전 시도에서 PDF 책갈피 895개 중 약 870개가 1페이지로 잘못 연결된 상태로 확인됐고, 실제 Chapter/Section 텍스트 위치를 추출해서 목적지를 다시 매핑하는 방식이 맞습니다.
- 패킷 분석 쪽 최종 기준도 요약에 정리했습니다. 특히 `ReadActualPosition`은 이전 중간 결론의 20바이트가 아니라, 반복 캡처 기준 **24바이트 응답**이 최종 기준입니다.
- 현재 git 상태에는 기존 생성 문서들과 `docs/history/`가 untracked로 남아 있습니다. 이건 제가 방금 만든 분할본 포함입니다.

다음 작업은 바로 PDF 책갈피 복구로 이어가면 됩니다.

> PDF 책갈피 복구로 이어서 진행하자.

<details><summary>10 previous messages</summary>

> PDF 복구 작업으로 이어가겠습니다. 먼저 PDF 작업 지침과 현재 파일 상태를 확인하고, 원본은 그대로 둔 복사본에 책갈피를 새로 씌우는 방식으로 진행하겠습니다.
>
> 원본 PDF는 약 31MB입니다. 이제 기존 책갈피 트리의 제목/계층과 실제 본문에서 추출되는 챕터·섹션 제목을 대조해서, 자동 매핑 가능 범위를 먼저 수치로 확인하겠습니다.
>
> 매핑 기준은 명확합니다. 기존 책갈피 제목에서 `Chapter N` 또는 `x.y.z` 번호를 뽑고, 본문 29페이지 이후에서 같은 번호로 시작하는 실제 섹션 첫 위치를 찾아 목적지로 쓰겠습니다. 먼저 이 방식으로 몇 개가 해결되는지 검사합니다.
>
> 1차 매핑은 전체 895개 제목을 모두 찾았지만, 챕터 매핑에 문제가 있습니다. 본문 중 “문서 사용법” 표에 `Chapter 2`, `Chapter 3` 같은 문자열이 먼저 나오기 때문에 실제 챕터 시작 페이지로 오인했습니다. 챕터는 페이지 상단의 실제 제목만 잡도록 기준을 강화하겠습니다.
>
> 매핑 검사는 정상입니다. 895개 책갈피 전부 목적지를 찾았고, 기존에 1페이지로 몰린 870개가 실제 위치로 바뀝니다. 이제 복구 PDF와 검증용 매핑 CSV를 생성하겠습니다.
>
> <details><summary>Created 1 file</summary>
>
> - Wrote `./tmp/pdfs/fix_maestro_bookmarks.py` (+181 -0)
>
> </details>
>
> 현재 Python이 `int | None` 타입 문법을 지원하지 않는 버전이라 스크립트가 바로 중단됐습니다. 기능 문제는 아니고 문법 호환 문제라 해당 타입 표기만 제거해서 다시 실행하겠습니다.
>
> <details><summary>Edited 1 file</summary>
>
> - Wrote `./tmp/pdfs/fix_maestro_bookmarks.py` (+1 -1)
>
> </details>
>
> 복구 PDF는 생성됐고, 895개 책갈피 전부 재작성됐습니다. 다만 챕터 번호만으로 매핑하면 원본 책갈피 제목과 실제 본문 챕터 제목이 어긋나는 구간이 있을 수 있어, 챕터 제목 불일치 여부까지 검증하겠습니다.
>
> 출력 PDF 검증 결과, 페이지 수 2435개와 책갈피 895개가 유지됐고, 1페이지로 향하는 책갈피는 0개입니다. 이제 표본 페이지 렌더링 도구가 있는지 확인해서 가능하면 시각 검증까지 하겠습니다.
</details>

PDF 책갈피 복구 완료했습니다. 원본 PDF는 그대로 두고 복구본을 새로 만들었습니다.

복구 파일:
- [복구 PDF](<./output/pdf/Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf>)
- [책갈피 매핑 CSV](<./output/pdf/Maestro Administrative and Motion API_2022_12_v2.012_bookmark_map.csv>)

검증 결과:
- 페이지 수: `2435` 유지
- 책갈피 수: `895` 유지
- 잘못 1페이지로 가던 책갈피: `870개` 수정
- fallback 매핑: `0개`
- 복구 후 1페이지로 향하는 책갈피: `0개`
- 표본 페이지 렌더링 확인 완료: 표지, Chapter 1, Chapter 5, `MMC_ReadActualPosition`, Chapter 24, Chapter 26

현재 생성 파일들은 아직 git에는 올리지 않았습니다.

> 이거 완성된 결과물 보면 테이블로 내용이 좀 짤리는데 함수명 필드를 좀 줄이고 내용 필드를 늘려서 한눈에 들어올 수 있게 만들어줘
>
> User context
>
> Attachments:
> - Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.html: `./docs/Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.html`

> 그냥 여기 섹션이랑 페이지 삭제하고 함수명 필드를 줄여서 내용이 많이 보일수 있게 만들어줘
>
> User context
>
> Attachments:
> - Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.md: `./docs/Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.md`
> - Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.xlsx: `./docs/Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.xlsx`
> - Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.md: `./docs/Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.md`
> - Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.xlsx: `./docs/Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.xlsx`

<details><summary>6 previous messages</summary>

> 요청 기준을 바꿔서 진행하겠습니다. 대상은 CoreAPI 4개 파일이고, `섹션/페이지` 컬럼을 제거한 뒤 함수명 폭은 줄이고 설명/내용이 더 많이 보이도록 엑셀과 MD를 같이 맞추겠습니다.
>
> 스프레드시트 작업이 포함되어 있어서 `Spreadsheets` 스킬 지침을 먼저 확인합니다. 그 다음 파일 구조를 읽고 실제 컬럼명을 기준으로 일괄 처리하겠습니다.
>
> 엑셀 편집은 번들된 스프레드시트 런타임을 쓰겠습니다. 먼저 워크북의 시트명과 헤더를 확인해서 `섹션/페이지` 컬럼 삭제가 실제로 어느 시트에 적용돼야 하는지 고정합니다.
>
> MD는 표에서 앞 두 컬럼만 제거하면 되고, 엑셀은 같은 3컬럼 구조로 다시 저장하면서 `함수명`은 좁게, `내용/Role`은 넓게 잡겠습니다. 기존 생성물 성격상 전체 워크북을 새 레이아웃으로 재생성하는 방식이 가장 깔끔합니다.
>
> <details><summary>Created 1 file</summary>
>
> - Wrote `./tmp/spreadsheets_api_overview_edit/rebuild_core_api_layout.mjs` (+149 -0)
>
> </details>
>
> 4개 파일 수정이 완료됐습니다. 이제 렌더링 미리보기로 실제 폭과 줄바꿈이 괜찮은지 확인하고, MD 헤더도 앞 두 컬럼이 빠졌는지 검증하겠습니다.
</details>

요청대로 4개 파일 모두 수정했습니다.

변경 내용:
- `섹션/Section`, `페이지/Page` 컬럼 삭제
- MD 표를 `함수명/파라미터/내용` 구조로 변경
- XLSX도 3컬럼으로 재생성
- 엑셀에서 함수명 컬럼은 좁게, 내용/Role 컬럼은 넓게 조정
- 내용 컬럼 줄바꿈 적용, 첫 행 고정 적용
- 검증: 두 xlsx 모두 `A1:C342`, 오류 문자열 검색 0건

수정 파일:
- [Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.md](/C:/work/Elmo/Elmo_Master/docs/Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.md)
- [Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.xlsx](/C:/work/Elmo/Elmo_Master/docs/Maestro_API_Function_Overview_CoreAPI_2022_12_v2_012.xlsx)
- [Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.md](/C:/work/Elmo/Elmo_Master/docs/Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.md)
- [Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.xlsx](/C:/work/Elmo/Elmo_Master/docs/Maestro_API_Function_Overview_CoreAPI_KO_2022_12_v2_012.xlsx)

참고: 이 4개 파일은 현재 git 기준으로 아직 untracked 상태입니다.

> Maestro Administrative and Motion API_2022_12_v2.012.PDF가 최신본인지 체크해보고 이거 말고 새로 업데이트 된 내용 있으면 알려줘.

<details><summary>5 previous messages</summary>

> <details><summary>Searched web 4 times</summary>
>
> - Searched web for `Elmo Maestro Administrative and Motion API v2.012 latest PDF ...`
> - Searched web for `https://www.elmomc.com/product/gold-maestro/`
> - Searched web for `https://www.elmomc.com/products/application-studio/download-resource-center/`
> - Searched web for `"Maestro Administrative and Motion" "2.013" ...`
>
> </details>
>
> 최신 여부는 로컬 파일만으로는 판단할 수 없어서 Elmo 공식 Resource Center 기준으로 확인하겠습니다. 공개 페이지에 API 매뉴얼 자체가 올라와 있는지와, Maestro 관련 최신 펌웨어/라이브러리/릴리즈 노트가 따로 있는지 분리해서 보겠습니다.
>
> 공식 Resource Center는 공개 접근이 되고, 현재 Maestro 섹션에는 API 매뉴얼 PDF보다 펌웨어/라이브러리/릴리즈 노트가 중심으로 올라와 있습니다. 이제 실제 링크 목록과 파일명을 긁어서 `v2.012`보다 새 API 문서가 있는지 검색하겠습니다.
>
> <details><summary>Searched web 1 time</summary>
>
> <details><summary>Searched web 1 time</summary>
>
> - Searched web for `site:elmomc.com/download "Maestro Administrative" ...`
>
> </details>
>
> </details>
>
> `v2.012` API 매뉴얼 자체보다 최신인 공개 API PDF는 아직 찾지 못했습니다. 대신 2025년 기준 Maestro 펌웨어/릴리즈 노트가 공식 Resource Center에 올라와 있어서, 그 릴리즈 노트 안에 API 영향 내용이 있는지 확인하겠습니다.
</details>

확인 결과, **`Maestro Administrative and Motion API_2022_12_v2.012.pdf`보다 최신인 공개 API 매뉴얼 PDF는 찾지 못했습니다.**

단, 공개 Elmo Resource Center 기준으로는 이 API 매뉴얼 자체가 현재 Maestro 다운로드 목록에 안 보입니다. Maestro 섹션에는 최신 펌웨어/라이브러리/릴리즈 노트가 올라와 있고, API 매뉴얼 PDF 항목은 없습니다. 따라서 결론은 이렇게 봐야 합니다.

- **API 매뉴얼 기준:** 현재 보유한 `v2.012 / Dec 2022`가 공개적으로 확인 가능한 최신본으로 보임
- **단정 제한:** Elmo Service Portal/로그인 자료실에 비공개 최신 문서가 있을 수 있음
- **새 업데이트 있음:** API 매뉴얼은 아니지만 Maestro 펌웨어/툴/릴리즈 노트는 이후 버전이 있음

공식 Resource Center에서 확인된 최신 Maestro 관련 항목:
- PMAS 펌웨어: `v2.2.1.1`, 파일명 `uimage_v2.2.1.1_B15_2025_03_09_IEC_1.0.1.0.pms`
- TMAS 펌웨어: `v4.2.1.1`, 파일명 `uimage_v4.2.1.1_B15_2025_03_09.tms`
- C# 라이브러리: `Gold Maestro MMCLibDotNET Libs V3.0.0.7`
- Codesys 패키지: `Elmo_1.0.0.6`
- 문서: `Platinum Maestro 2.2.1.1 Release Note`, `Titanium Maestro Initial Release notes`, `Gold Maestro MMCLibDotNET_Test_App_V3.0.0.7`
근거: [Elmo Resource Center - Maestro Links](https://www.elmomc.com/products/application-studio/download-resource-center/)의 Maestro 항목에 위 내용이 올라와 있습니다. 공개 페이지상 Maestro 펌웨어/라이브러리/문서 목록은 보이지만 API 매뉴얼 PDF는 없습니다.

추가로 제품 페이지 기준:
- Gold Maestro 쪽은 설치 매뉴얼이 `V.1.501_April 2024`로 올라와 있음
- Platinum Maestro 쪽은 설치 가이드가 `V.2.001_April 2024`, Integrated I/O 설치 가이드가 `V.2.002_December 2024`로 올라와 있음
근거: [Gold Maestro](https://www.elmomc.com/product/gold-maestro/), [Platinum Maestro](https://www.elmomc.com/product/platinum-maestro/)

정리하면, **API 함수 정의/구조체 기준 문서로는 지금 가진 `v2.012`를 계속 기준으로 쓰면 됩니다.** 다만 PMAS/TMAS 최신 펌웨어 기준 변경사항은 별도 릴리즈 노트에 있을 가능성이 있으니, 최신 펌웨어 기능까지 반영하려면 `Platinum Maestro 2.2.1.1 Release Note`를 추가로 확보해서 비교해야 합니다. 현재 공개 링크는 locked 다운로드라 실제 PDF 내용은 로그인 없이 바로 열리지 않았습니다.

> 로그인 자료실 링크보내줘봐

<details><summary>1 previous message</summary>

> <details><summary>Searched web 1 time</summary>
>
> <details><summary>Searched web 1 time</summary>
>
> - Searched web for `site:elmomc.my.site.com Elmo service portal login ...`
>
> </details>
>
> </details>
</details>

여기입니다.

- Resource Center 로그인: [https://www.elmomc.com/login/](https://www.elmomc.com/login/)
- 한국어 로그인 페이지: [https://www.elmomc.com/kr/login/](https://www.elmomc.com/kr/login/)
- 현재 Maestro 다운로드 목록 페이지: [https://www.elmomc.com/products/application-studio/download-resource-center/](https://www.elmomc.com/products/application-studio/download-resource-center/)
- Elmo Support-Service Portal: [https://elmomc.my.site.com/login?locale=us](https://elmomc.my.site.com/login?locale=us)

참고로 Resource Center에서 locked 파일 누르면 로그인 페이지로 넘어갑니다. API 매뉴얼 최신본을 찾으려면 로그인 후 Resource Center에서 `Maestro Administrative and Motion API`, `MAN-MAESTRO-API`, `Maestro API`, `v2.012`로 검색하는 게 제일 빠릅니다.

> API 다운로드 항목이 없는데? 잘 찾아서 보내줘

<details><summary>5 previous messages</summary>

> 맞습니다. 내가 앞서 말한 Resource Center 현재 Maestro 목록에는 API 매뉴얼 항목이 안 보입니다. 그래서 지금은 “현재 목록” 말고 숨겨진/구버전 접힌 영역, 제품 페이지, 사이트 검색 인덱스까지 다시 뒤져서 실제 다운로드 가능한 위치가 있는지 확인하겠습니다.
>
> 현재 Resource Center HTML 전체를 긁어봐도 `Maestro Administrative`, `MAN-MAESTRO-API`, `v2.012`, `2022_12` 문자열은 0건입니다. 즉 네 말대로 공개 다운로드 목록에는 API 매뉴얼이 없습니다. 그래도 WordPress 검색/첨부 인덱스 쪽에 숨어 있는지 더 확인하겠습니다.
>
> 제품 페이지도 확인했습니다. Gold/Platinum/Titanium Maestro 제품 페이지에는 설치 매뉴얼/가이드만 있고 API 매뉴얼 링크는 없습니다. 이제 Download Monitor 쪽 공개 API/검색 라우트를 확인해서 다운로드 ID로 숨어 있는 항목이 있는지 보겠습니다.
>
> <details><summary>Searched web 3 times</summary>
>
> <details><summary>Searched web 3 times</summary>
>
> - Searched web for `site:elmomc.my.site.com "Maestro Administrative" ...`
> - Searched web for `Elmo Motion Control service portal documentation download API manual`
> - Searched web for `https://www.elmomc.com/capabilities/motion-control/host-programming-environment/g-mas-api-for-net/`
>
> </details>
>
> </details>
>
> 공개 검색과 공식 페이지 기준으로 확인한 결과를 정리하면, “API 매뉴얼 다운로드 링크”는 현재 공식 사이트에서 노출되지 않습니다. 대신 API 설명 페이지는 따로 있고, 실제 자료는 Service Portal/문의 경로로 돌린 상태로 보입니다.
</details>

찾아봤는데, 결론부터 말하면 **공식 사이트에서 `Maestro Administrative and Motion API` PDF 다운로드 항목은 현재 안 보입니다.** 내가 전에 Resource Center라고 한 건 부정확했습니다.

확인한 공식 링크는 이겁니다.

- G-MAS API 설명 페이지: [G-MAS API for .NET](https://www.elmomc.com/capabilities/motion-control/host-programming-environment/g-mas-api-for-net/)
- Technical Resource Center: [Download Resource Center](https://www.elmomc.com/products/application-studio/download-resource-center/)
- Service Portal 로그인: [Elmo Service Portal](https://elmomc.my.site.com/login?locale=us)
- 일반 로그인: [Elmo Login](https://www.elmomc.com/login/)

공식 Resource Center에서 실제로 보이는 Maestro 관련 항목은 API 매뉴얼이 아니라 아래 쪽입니다.

- PMAS firmware `v2.2.1.1`: [download/22423](https://www.elmomc.com/download/22423/?tmstv=1779769936)
- TMAS firmware `v4.2.1.1`: [download/22456](https://www.elmomc.com/download/22456/?tmstv=1779769936)
- `Gold Maestro MMCLibDotNET Libs V3.0.0.7`: [download/22446](https://www.elmomc.com/download/22446/?tmstv=1779769936)
- `Platinum Maestro 2.2.1.1 Release Note`: [download/22425](https://www.elmomc.com/download/22425/?tmstv=1779769936)
- `Gold Maestro MMCLibDotNET_Test_App_V3.0.0.7`: [download/22439](https://www.elmomc.com/download/22439/?tmstv=1779769936)

내가 공식 사이트에서 직접 확인한 검색어:
- `Maestro Administrative`
- `Maestro Administrative Motion API`
- `MAN-MAESTRO-API`
- `v2.012`
- `2022_12`

결과:
- Resource Center HTML 내 API 매뉴얼 문자열: **0건**
- 제품 페이지 Gold/Platinum/Titanium Maestro 내 API 매뉴얼 링크: **0건**
- WordPress 공개 검색/REST 검색: API PDF 다운로드 항목 없음
- media/download API 쪽은 인증 필요 또는 차단됨

따라서 지금 정확한 판단은 이겁니다.

**`Maestro Administrative and Motion API_2022_12_v2.012.pdf`는 공개 다운로드 목록에는 현재 없는 문서입니다.**
새 버전이 공개로 올라온 것도 못 찾았습니다. 최신본 확인은 Service Portal 로그인 후 검색하거나 Elmo/대리점에 직접 요청해야 합니다.

Service Portal에서 검색할 키워드는 이 순서로 넣으세요.

```text
Maestro Administrative and Motion API
MAN-MAESTRO-API
Maestro API
Administrative Motion API
v2.012
```

공개 사이트 기준으로는 네가 가진 `v2.012`가 우리가 확인 가능한 최신 API 문서입니다.

> 이 파일들을 분석해서 시그마텍에 블렌딩이 어떤기술들이 있는지 설명해
>
> User context
>
> Attachments:
> - Rounding Mode (blending or tolerance sphere).pdf: `./docs/sigmatek_blending/Rounding Mode (blending or tolerance sphere).pdf`
> - Smooth Rounding Mode with Cubic Spline.pdf: `./docs/sigmatek_blending/Smooth Rounding Mode with Cubic Spline.pdf`
> - Synchronization of Axis Groups.pdf: `./docs/sigmatek_blending/Synchronization of Axis Groups.pdf`

<details><summary>4 previous messages</summary>

> PDF 3개를 직접 읽어서 Sigmatek 문서 기준으로 블렌딩/라운딩/축그룹 동기화 기술을 분리해 정리하겠습니다. 우선 페이지 수와 텍스트 추출 상태부터 확인합니다.
>
> PDF 작업이므로 `pdf` 스킬 기준으로 진행하겠습니다. 이번 요청은 문서 내용 분석이 목적이라 렌더링보다 텍스트 추출과 용어/동작 구조 대조에 집중합니다.
>
> 첫 텍스트 추출 명령은 PowerShell 문법 때문에 실패했습니다. 파일에는 손대지 않았고, 같은 작업을 PowerShell 방식으로 다시 실행합니다.
>
> 세 PDF 모두 텍스트 추출이 됩니다. 이제 페이지별 본문을 뽑아 기술 요소와 조건/제약을 정리하겠습니다.
</details>

분석한 3개 문서 기준으로 보면, Sigmatek의 블렌딩은 단순히 “다음 명령을 끊지 않고 이어붙임” 수준이 아니라 **경로 전환부를 tolerance/blending sphere 안에서 재보간해서 속도·가속도·저크 불연속을 줄이는 기술**입니다.

참조 파일:
- [Rounding Mode](<./docs/sigmatek_blending/Rounding Mode (blending or tolerance sphere).pdf>)
- [Smooth Rounding Mode with Cubic Spline](<./docs/sigmatek_blending/Smooth Rounding Mode with Cubic Spline.pdf>)
- [Synchronization of Axis Groups](<./docs/sigmatek_blending/Synchronization of Axis Groups.pdf>)

**1. Rounding Mode / Blending Sphere**
`Radius`로 정의한 구 안에서 현재 path segment에서 다음 path segment 방향으로 미리 꺾어 들어가는 방식입니다.

핵심:
- `MoveLinear()` 쪽 `Radius` 파라미터로 blending sphere를 정의
- sphere 진입 후에는 다음 경로 방향으로 연속적으로 유도
- sphere 내부에서는 정확한 path accuracy가 보장되지 않음
- sphere를 빠져나오면 다시 path accuracy와 요구 속도를 회복
- `Radius`가 실제 path 길이보다 크면 실제 길이로 제한
- `Radius = 0`이면 내부적으로 smooth rounding mode가 활성화됨

제약:
- Linear-Linear 전환은 축별 속도 jump를 줄이기 위해 rounding 가능
- Linear-Circular, Circular-Linear은 tangential error 기준으로 판단
- 비접선 전환에서 tangential error가 tolerance보다 크면 rounding 대신 exact stop으로 전환
- Circular-Circular 전환에는 일반 rounding mode를 사용할 수 없음

**2. Smooth Rounding Mode**
비접선 path 전환을 tolerance sphere 안에서 spline으로 재보간하는 방식입니다.

문서에 나온 대표 모드:
- `_LMCPROF_SMOOTH_CUBIC`
- `_LMCPROF_SMOOTH_QUINT`

그리고 조건 판단에 언급된 smooth 계열:
- `_LMCPROF_SMOOTH_PARAB`
- `_LMCPROF_SMOOTH_CUBIC`
- `_LMCPROF_SMOOTH_QUINT`
- `_LMCPROF_SMOOTH_CLOTH`
- `_LMCPROF_CONT_DIRECT`

설정:
- `TransRadius` 사용
- 적용 메서드: `MoveLinear()`, `MoveRelative()`, `MoveCircle()`

Cubic vs Quint 차이:
- Cubic spline: 경로는 부드럽게 이어지지만 spline 시작/끝에서 acceleration jump가 생길 수 있음
- 5th order spline / Quint: rounding 시작/끝에서 acceleration jump가 없고, 허용 axis jerk를 유지하는 쪽

즉 품질 기준으로 보면:
- Cubic = 위치/속도 연속성 중심
- Quint = 가속도/저크까지 더 부드럽게 관리

**3. Exact Stop 자동 전환**
Sigmatek은 블렌딩이 불가능하거나 조건이 맞지 않으면 무조건 블렌딩하려고 하지 않고 exact stop으로 떨어집니다.

조건 예:
- 마지막 motion command까지 추가 sequence가 없으면 exact stop
- 이미 braking 중 exact stop 상태면 정지 완료 후 다음 motion은 standstill에서 재시작
- tolerance sphere 밖에 있을 때, 진입 전 허용 속도까지 감속 가능하면 지정 interpolation mode 실행
- 감속이 불가능하면 smooth rounding mode로 전환

이 부분이 중요합니다. Sigmatek 블렌딩은 “명령만 미리 넣으면 무조건 섞임”이 아니라 **현재 축 상태, tolerance sphere 진입 여부, 감속 가능 여부**를 보고 모드가 바뀝니다.

**4. Axis Group Synchronization**
축 그룹 동기화는 블렌딩 품질과 직접 관련 있습니다. 최대 6개 그룹을 정의할 수 있습니다.

구조:
- Group 1: Main group
- Group 2~6: Secondary group
- 그룹 index가 높을수록 path length 계산 우선순위 낮음

동작:
- Main group의 delta distance를 기준으로 path length 결정
- 보조축은 path에 coupling되어 모든 endpoint를 같은 시간에 도달하도록 맞춤
- 보조축의 speed, acceleration, deceleration 한계를 감시
- 한계를 넘으면 전체 profile의 speed/acc/dec를 줄임

주의점:
- 보조축 delta distance는 path length 계산에 포함되지 않을 수 있음
- 그래서 auxiliary axis 쪽 maximum tangential error는 직접 감시가 안 될 수 있음
- 대신 `SetAxisParameter(AxisNo, LMCPROFAX_MAX_VEL_JUMP, ...)`로 보조축 최대 속도 증가량을 제한
- 그룹마다 단위/분해능이 다르면 `SetGroupParameter()`로 group별 speed/acceleration을 따로 정의해야 함

**5. Rotary Auxiliary Axis와 Blending Sphere**
회전 보조축이 있는 경우가 특히 중요합니다.

예:
- X/Y는 선형 경로를 따라 일정 속도로 이동
- A축은 contour를 따라 회전해야 함
- Linear → Circular 접선 전환에서 A축은 새 segment 시작과 동시에 목표 속도에 올라와 있어야 함

문서에서는 단순히 maximum speed jump만 제한하면 실제 위치가 setpoint를 따라가지 못해 lagging이 생길 수 있다고 설명합니다. 이 경우 affected path segment 사이에 blending sphere를 넣어 실제 위치 지연을 줄이는 방식이 필요합니다.

**정리**
Sigmatek 블렌딩 기술은 크게 이렇게 보면 됩니다.

| 기술 | 목적 | 주요 파라미터 |
|---|---|---|
| Rounding Mode | 경로 전환부에서 속도 jump 완화 | `Radius` |
| Blending/Tolerance Sphere | 정확 경로 대신 부드러운 전환 영역 제공 | `Radius`, `TransRadius` |
| Smooth Cubic | spline으로 비접선 전환 보간 | `_LMCPROF_SMOOTH_CUBIC` |
| Smooth Quint | 가속도 jump와 jerk까지 더 부드럽게 관리 | `_LMCPROF_SMOOTH_QUINT` |
| Exact Stop Fallback | 블렌딩 불가 조건에서 안전 정지 | 내부 조건 판단 |
| Axis Group Synchronization | 주축/보조축 endpoint 동기화 | `LockProfile`, `SetGroupParameter` |
| Aux Axis Velocity Jump Limit | 보조축 속도 급증 제한 | `LMCPROFAX_MAX_VEL_JUMP` |

핵심은 하나입니다. **Sigmatek은 path 정확도를 일부 포기하는 tolerance 영역을 만들고, 그 안에서 축들을 재보간해서 속도·가속도·저크 연속성을 확보하는 방식으로 블렌딩합니다.**

> _LMCPROF_SMOOTH_CLOTH도 보간 기술 아닌가?

<details><summary>2 previous messages</summary>

> 맞습니다. `_LMCPROF_SMOOTH_CLOTH`도 이름상/문맥상 보간 방식입니다. 정확히 Sigmatek 문서에서 어떻게 정의돼 있는지 로컬 문서/도움말에 같은 키워드가 더 있는지 바로 확인하겠습니다.
>
> 로컬 텍스트 검색은 PDF 내부를 직접 못 잡아서, 방금 추출했던 PDF 본문 기준으로 말하겠습니다. 해당 문서에는 `_LMCPROF_SMOOTH_CLOTH`가 smooth rounding 계열 모드 중 하나로만 언급되고, 별도 상세 설명은 없습니다.
</details>

네, 맞습니다. `_LMCPROF_SMOOTH_CLOTH`도 **보간/라운딩 기술 계열로 보는 게 맞습니다.**

정확히 말하면:

- `_LMCPROF_SMOOTH_CUBIC` = cubic spline 기반 smooth rounding
- `_LMCPROF_SMOOTH_QUINT` = 5차 spline 기반 smooth rounding
- `_LMCPROF_SMOOTH_CLOTH` = 보통 명칭상 **clothoid / clothoid curve 기반 smooth rounding**으로 해석하는 게 맞음

`CLOTH`는 거의 확실히 `Clothoid` 약어로 봐야 합니다. Clothoid는 곡률이 선형적으로 변하는 곡선이라, 직선-곡선 또는 곡선-곡선 전환에서 급격한 곡률 변화 없이 부드럽게 이어주는 데 쓰입니다.

다만 네가 준 PDF 3개 안에서는 `_LMCPROF_SMOOTH_CLOTH`를 상세 설명하지 않고, 아래 smooth rounding 모드 목록 안에만 넣고 있습니다.

```text
_LMCPROF_SMOOTH_PARAB
_LMCPROF_SMOOTH_CUBIC
_LMCPROF_SMOOTH_QUINT
_LMCPROF_SMOOTH_CLOTH
```

그래서 정리하면:

| 모드 | 성격 |
|---|---|
| `_LMCPROF_SMOOTH_PARAB` | 포물선 기반 smooth rounding 추정 |
| `_LMCPROF_SMOOTH_CUBIC` | 3차 spline 보간 |
| `_LMCPROF_SMOOTH_QUINT` | 5차 spline 보간, 가속도/저크 연속성에 유리 |
| `_LMCPROF_SMOOTH_CLOTH` | clothoid 기반 보간/라운딩으로 봐야 함 |

내가 이전 답변에서 `_LMCPROF_SMOOTH_CLOTH`를 “대표 보간 기술”로 따로 설명하지 않은 건 누락입니다. 포함해서 봐야 합니다.

> _LMCPROF_SMOOTH_CUBIC, _LMCPROF_SMOOTH_QUINT, _LMCPROF_SMOOTH_CLOTH도 보간하기 위해서 Radius, TransRadius가 필요한거 아님?

네. **필요합니다.**
정확히 말하면 `_LMCPROF_SMOOTH_CUBIC`, `_LMCPROF_SMOOTH_QUINT`, `_LMCPROF_SMOOTH_CLOTH` 같은 smooth rounding 계열은 **보간 방식**이고, 그 보간을 적용할 **공간 범위**가 필요합니다. 그 범위가 문서에서 말하는 **tolerance sphere / blending sphere**이고, 보통 `TransRadius` 또는 `Radius`로 지정됩니다.

문서 기준으로는 이렇게 정리됩니다.

| 항목 | 의미 |
|---|---|
| `TransMode` | 전환부를 어떤 방식으로 보간할지 선택 |
| `_LMCPROF_SMOOTH_CUBIC` | cubic spline으로 전환부 보간 |
| `_LMCPROF_SMOOTH_QUINT` | 5차 spline으로 전환부 보간 |
| `_LMCPROF_SMOOTH_CLOTH` | clothoid 계열로 전환부 보간 |
| `Radius` / `TransRadius` | 보간이 적용될 tolerance/blending sphere 크기 |

문서 표현상 구분은 이렇습니다.

- `Rounding Mode` 문서:
  - `MoveLinear()`의 `Radius`로 blending sphere 정의
  - `Radius = 0`이면 smooth rounding mode가 내부적으로 활성화된다고 설명

- `Smooth Rounding Mode with Cubic Spline` 문서:
  - tolerance sphere는 `TransRadius`로 정의
  - 적용 메서드: `MoveLinear()`, `MoveRelative()`, `MoveCircle()`
  - 그 sphere 안에서 cubic 또는 5th order spline으로 보간

즉 핵심은:

```text
TransMode = 어떤 보간 곡선을 쓸지
TransRadius/Radius = 그 보간을 어느 범위에서 할지
```

그래서 `_LMCPROF_SMOOTH_CUBIC`, `_LMCPROF_SMOOTH_QUINT`, `_LMCPROF_SMOOTH_CLOTH`만 설정한다고 전환부가 완성되는 게 아니라, **전환 허용 반경/tolerance 영역이 같이 정의되어야 정상적인 blending/rounding이 됩니다.**

다만 `Radius = 0`일 때 “smooth rounding mode가 내부 활성화”된다는 문구가 있어서, Sigmatek 내부 기본값/자동 처리 경로가 있을 수는 있습니다. 그래도 제어 의도를 명확히 하려면 `TransRadius`를 명시하는 게 맞습니다.

> Elmo API에서도 Blending이 명시되어 있는데 Sigmatek이랑 어떠한 차이가 있지?
>
> User context
>
> Attachments:
> - Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf: `./output/pdf/Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`

<details><summary>6 previous messages</summary>

> Elmo 쪽은 추측하지 않고, 네가 준 책갈피 복구 PDF에서 `Blending`, `BufferMode`, `Blended Behavior` 관련 섹션을 직접 뽑아서 Sigmatek 문서 내용과 대조하겠습니다.
>
> PDF 분석 기준으로 진행합니다. 먼저 `Blending`, `BufferMode`, `Blended Behavior`가 나오는 페이지와 주변 본문을 찾겠습니다.
>
> 전체 PDF를 `pypdf`로 여러 번 훑는 방식은 느려서 중단됐습니다. 같은 내용을 PyMuPDF로 한 번만 빠르게 훑어서 관련 페이지를 잡겠습니다.
>
> 관련 키워드는 주로 Elmo 문서의 `BufferMode`, `TransitionMode`, `Blended Behavior Mechanism` 쪽에 걸립니다. 이제 정의가 들어있는 페이지를 뽑아서 실제 파라미터와 동작 의미를 확인하겠습니다.
>
> 콘솔 인코딩 때문에 특수문자에서 출력이 끊겼습니다. UTF-8로 재출력해서 해당 구간만 다시 읽겠습니다.
>
> Elmo는 단축 모션과 그룹/경로 모션에서 쓰는 블렌딩 용어가 다릅니다. 핵심 비교는 `MC_BUFFERED_MODE_ENUM`과 `NC_TRANSITION_MODE_ENUM` 정의라서 그 부분만 더 좁혀서 확인합니다.
</details>

맞습니다. Elmo API에도 Blending이 명시되어 있고, **단순 기능 유무만 보면 Elmo도 Sigmatek처럼 블렌딩/전환곡선 기능이 있습니다.**
차이는 “블렌딩을 제어하는 관점”이 다릅니다.

기준 문서:
- [Elmo API PDF](<./output/pdf/Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf>)
- Sigmatek `Rounding Mode`, `Smooth Rounding Mode`, `Synchronization of Axis Groups` 문서

**핵심 차이**
| 구분 | Elmo Maestro | Sigmatek |
|---|---|---|
| 기본 개념 | Function Block queue 기반의 `BufferMode` + `TransitionMode` | path segment 사이의 tolerance/blending sphere 기반 |
| 속도 연결 | `MC_BLENDING_LOW/PREVIOUS/NEXT/HIGH`로 전환점 속도 결정 | sphere 안에서 다음 segment 방향으로 보간 |
| 형상 보간 | Multi-axis에서 `NC_TRANSITION_MODE_ENUM`으로 전환곡선 지정 | `TransMode`로 cubic/quint/clothoid 등 보간 방식 지정 |
| 반경/허용영역 | `fTransitionParameter`가 모드에 따라 radius, corner distance, deviation 등으로 해석됨 | `Radius` / `TransRadius`가 tolerance sphere 크기 |
| 단일축 | 주로 전환점 속도 블렌딩 | 경로/segment 전환부 보간 개념이 더 직접적 |
| 다축/경로 | polynomial transition mode가 꽤 많음 | axis group sync + auxiliary axis velocity jump 관리가 명확 |
| 실패/조건 불만족 | active FB 상태면 blending arc 삽입 안 되고 buffered/none으로 바뀔 수 있음 | tangential error, sphere 진입 여부, 감속 가능 여부에 따라 exact stop/smooth fallback |

**Elmo 쪽 구조**
Elmo는 먼저 `BufferMode`가 있습니다.

```text
MC_ABORTING_MODE
MC_BUFFERED_MODE
MC_BLENDING_LOW_MODE
MC_BLENDING_PREVIOUS_MODE
MC_BLENDING_NEXT_MODE
MC_BLENDING_HIGH_MODE
```

문서상 의미는 전환점에서 속도를 어떻게 잡을지입니다.

- `Buffered`: 이전 동작 완료 후 다음 동작 시작, transition velocity = 0
- `BlendingLow`: 이전/현재 명령 중 낮은 속도로 전환
- `BlendingPrevious`: 이전 FB 속도로 전환
- `BlendingNext`: 현재 FB 속도로 전환
- `BlendingHigh`: 이전/현재 명령 중 높은 속도로 전환

즉 Elmo의 `BufferMode`는 **전환점 속도 정책**입니다.

그 다음 Multi-axis에서는 별도로 `TransitionMode`가 있습니다.

```text
MC_TM_NONE_MODE
MC_TM_DEFINED_VELOCITY_MODE
MC_TM_CORNER_DISTANCE_MODE
MC_TM_MAX_CORNER_DEVIATION_MODE
MC_TM_SWITCH_RADIUS_MODE
MC_TM_CORNER_DIST_CV_POLYNOM3
MC_TM_CORNER_DIST_CV_POLYNOM5
MC_TM_CORNER_DEVIATION_MODE_PLN6
MC_TM_CORNER_DIST_CV_POLYNOM5_NAXES
MC_TM_CORNER_DIST_CV_POLYNOM7
MC_TM_CORNER_DEVIATION_MODE_PLN8
MC_TM_DIST1_DIST2_DEVIATION_PLN6
MC_TM_DIST1_DIST2_DEVIATION_PLN8
```

여기서 Elmo도 polynomial transition curve를 만듭니다.

- `POLYNOM3`: velocity는 부드럽지만 acceleration jump 가능
- `POLYNOM5`: velocity + acceleration smooth
- `POLYNOM7`: velocity + acceleration + jerk smooth
- `PLN8`: position + velocity + acceleration + jerk smooth
- `SWITCH_RADIUS`: 원호 radius 기반 transition
- `CORNER_DISTANCE`: corner distance 기반
- `CORNER_DEVIATION`: corner deviation 기반

이 부분은 Sigmatek의 `_LMCPROF_SMOOTH_CUBIC`, `_LMCPROF_SMOOTH_QUINT`, `_LMCPROF_SMOOTH_CLOTH`와 비교할 수 있는 영역입니다.

**가장 큰 차이**
Sigmatek은 사용자가 보는 개념이 더 직관적으로:

```text
TransMode = 어떤 보간 곡선인가
TransRadius / Radius = 어느 영역에서 보간할 것인가
```

Elmo는:

```text
BufferMode = 전환점 속도를 어떻게 잡을 것인가
TransitionMode = 전환곡선 형상을 어떻게 만들 것인가
fTransitionParameter = 모드별 전환 파라미터
```

입니다.

즉 Sigmatek은 **sphere 기반 path blending** 표현이고,
Elmo는 **PLCopen FB queue + velocity blending + transition curve** 표현입니다.

**주의할 점**
Elmo에서 `Blending`이라는 단어만 보고 Sigmatek의 `TransRadius` 방식과 1:1 대응시키면 안 됩니다.

예를 들어:

```text
Elmo MC_BLENDING_NEXT_MODE
```

이건 “다음 명령 속도로 전환한다”는 의미이지, 그 자체가 cubic/quint/clothoid 보간을 의미하지 않습니다.

Elmo에서 Sigmatek의 smooth rounding에 대응되는 것은 오히려 이 조합입니다.

```text
eBufferMode = MC_BLENDING_*
eTransitionMode = MC_TM_CORNER_DIST_CV_POLYNOM5 / POLYNOM7 / PLN8 ...
fTransitionParameter = corner distance / radius / deviation
```

**결론**
Elmo도 블렌딩 기술이 있습니다. 오히려 Multi-axis transition mode만 보면 polynomial mode가 세분화되어 있습니다.

다만 차이는 이겁니다.

- Sigmatek: `Radius/TransRadius` 중심의 tolerance sphere 보간
- Elmo: `BufferMode`로 전환 속도 결정 + `TransitionMode/fTransitionParameter`로 전환곡선 생성

그래서 Elmo에서 Sigmatek과 비슷한 블렌딩을 만들려면 `BufferMode`만 설정하면 부족하고, **반드시 `TransitionMode`와 `fTransitionParameter`까지 같이 봐야 합니다.**

> MMC_RpcInitConnection
> Initiates RPC connection to Maestro server.
> MMC_LIB_API int MMC_RpcInitConnection(
> IN MMC_CONNECTION_TYPE eType,
> IN MMC_CONNECTION_PARAM_STRUCT sConnParam,
> IN MMC_CB_FUNC pCbFunc ,
> IN char* cpHostIPAddr,
> OUT MMC_CONNECT_HNDL* pHndl
> );
> Motion Mode NC – Not relevant Distributed – not relevant
> Source GMAS\includes\MMC_general_API.h
> Parameters
> eType
> [IN] Connection type (IPC(inter process communication) or RPC(remote procedure
> calls)) with the following possible MMC_CONNECTION_TYPE enumerator values:
> MMC_RPC_CONN_TYPE RPC connection to MMC server
> MMC_IPC_CONN_TYPE IPC connection to MMC server
>
> 이 함수 보면 RPC랑 IPC랑 있는데 어떻게 다른거야?

간단히 말하면 **RPC는 네트워크越 통신**, **IPC는 같은 장비/같은 OS 내부 프로세스 간 통신**입니다.

**RPC**
`Remote Procedure Call`

- PC 프로그램에서 Maestro 컨트롤러의 MMC 서버로 명령 보낼 때 사용
- 보통 Ethernet/TCP/IP 기반
- `cpHostIPAddr`에 Maestro IP를 넣는 구조
- 예: PC WPF 프로그램 → `192.168.1.3` Maestro → Motion API 실행
- 네트워크 지연, TCP 처리, 서버 응답 시간이 포함됨

즉 우리가 지금까지 패킷 캡처한 구조는 거의 이쪽입니다.

```text
PC App
  -> TCP/IP
  -> Maestro MMC Server
  -> Motion Engine
```

**IPC**
`Inter Process Communication`

- 같은 장비 내부에서 프로세스끼리 통신할 때 사용
- 네트워크를 타지 않음
- Maestro 내부에서 실행되는 프로그램이 Maestro 내부 MMC 서버와 통신할 때 쓰는 구조로 보면 됨
- RPC보다 통신 오버헤드가 작고, 네트워크 지연이 없음
- 단, 외부 PC에서 Maestro를 제어하는 용도는 아님

구조는 대략 이렇습니다.

```text
Maestro 내부 Program
  -> IPC
  -> Maestro MMC Server
  -> Motion Engine
```

**차이 핵심**
| 구분 | RPC | IPC |
|---|---|---|
| 의미 | Remote Procedure Call | Inter Process Communication |
| 위치 | 외부 PC ↔ Maestro | Maestro 내부 프로세스 ↔ Maestro 내부 서버 |
| 통신 | 네트워크/TCP 기반 | 로컬 프로세스 간 통신 |
| IP 필요 | 필요 | 보통 불필요 또는 무시 |
| 지연 | 네트워크 지연 포함 | 상대적으로 낮음 |
| 우리가 쓰는 방식 | 대부분 RPC | 아님 |

**이 함수에서의 의미**
`MMC_RpcInitConnection()`은 이름은 `RpcInitConnection`이지만 `eType`으로 RPC/IPC 둘 다 선택하게 되어 있습니다.

```c
MMC_RPC_CONN_TYPE  // RPC connection to MMC server
MMC_IPC_CONN_TYPE  // IPC connection to MMC server
```

즉 함수 이름은 RPC지만 실제로는 “MMC 서버 연결 초기화 함수”에 가깝고, `eType`으로 원격 연결인지 내부 연결인지 고르는 구조입니다.

**너희 PC 기반 PMAS/WPF 프로그램 기준**
거의 확실히 `MMC_RPC_CONN_TYPE`을 써야 합니다.

왜냐하면 PC에서 Maestro IP로 붙는 구조이기 때문입니다.

```c
eType = MMC_RPC_CONN_TYPE
cpHostIPAddr = "192.168.1.3"
```

`MMC_IPC_CONN_TYPE`은 Maestro 내부에서 돌아가는 C/C++/IEC 프로그램이 MMC 서버에 붙을 때 쓰는 쪽으로 봐야 합니다.

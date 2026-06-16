# Elmo Master History 260609 Part 01

- Source: `C:\work\Elmo\Elmo_Master\docs\history\Elmo_Master_history_260609.md`
- Lines: 1-200

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

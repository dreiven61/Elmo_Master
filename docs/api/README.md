# API 문서

이 폴더는 LASAL Motion Control API의 사용자 설명서와 개발 진척도 문서를 한곳에서 관리한다.

## 정본과 배포 형식

| 문서 | 편집 정본 | 배포·열람 형식 |
|---|---|---|
| API 설명서 | [API_MANUAL.md](API_MANUAL.md) | [API_MANUAL.html](API_MANUAL.html), [API_MANUAL.docx](API_MANUAL.docx), [API_MANUAL.pdf](API_MANUAL.pdf) |
| API 개발 진척도 | [API_DEVELOPMENT_PROGRESS.md](API_DEVELOPMENT_PROGRESS.md) | [API_DEVELOPMENT_PROGRESS.html](API_DEVELOPMENT_PROGRESS.html), [API_DEVELOPMENT_PROGRESS.xlsx](API_DEVELOPMENT_PROGRESS.xlsx), [API_DEVELOPMENT_PROGRESS_SIMPLE.xlsx](API_DEVELOPMENT_PROGRESS_SIMPLE.xlsx) |
| 최우선 API 개발 설계 | [design/README.md](design/README.md) | HomeDS402, SetOpMode, HomeDS402Ex, SetPosition 개별 설계 |

Markdown 파일이 내용의 정본이다. DOCX, PDF, XLSX와 HTML은 정본에서 생성한 파생 산출물이며
직접 편집하지 않는다. byte offset과 frame shape의 정본은
[DINT packet map](../../LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt)이다.

`API_DEVELOPMENT_PROGRESS_SIMPLE.xlsx`는 기존 65개 OPUS/OPERA 비교표의 구조를 유지한
간단 현황표다. `진행도`와 `특이사항`만 current 진척도에 맞춰 갱신하며, 구현률은 PLC/실축
시험 통과율을 의미하지 않는다.

우선순위 `상`이면서 진행도 75% 미만인 4개 API는 `design/`을 current 구현 설계와 작업
체크리스트의 정본으로 사용한다. 다른 문서는 설계 내용을 복사하지 않고 이 폴더를 링크한다.

현재 문서는 `2.4-development` 개발 문서다. 기존
`LMC_Library/LMC_API_Distribution/03_API_User_Manual`의 `2.3-candidate` 배포본은 별도 승인 전까지
그대로 유지한다.

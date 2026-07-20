- LASAL strict network contract: PASS
- `git diff --check`: 오류 없음
- 작업트리: clean
- `origin/main` 푸시 완료
- 원격 HEAD: `fdf9487979452a8f7bf56da94d287ea9f85630db`

기존 `0.9.0-pc-api` 배포 스냅샷 안의 구형 EXE는 manifest 무결성을 위해 삭제하지 않고 `legacy archive`로 명시했습니다. 소스 테스트 앱은 완전히 제거됐습니다.

> 이제 API랑 예제문서를 배포준비할꺼야
> C:\work\Elmo\Elmo_Master\LMC_Library에 배포용 폴더를 만들고
> C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API처럼 API에 대해 사용법이 정리된 문서와 dll이 포함된 배포용 폴더를 만들고
> C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API 기존 폴더에는 API 개발 설명 문서를 만들어. 이건 내부 개발용이니까 상세하게 만들고
> C:\work\Elmo\Elmo_Master\LMC_Library 배포용 폴더에 예제프로그램 우리가 만든걸 첨부하는데, 어짜피 여기에 있는 폴더는 API 개발용이 아니라 배포 및 사용하기 위함이니 실제 솔루션이 연결된건 넣지말고 아까 내가 한대로 상대경로로 배포폴더에 Lib를 이용해 링크할 수 있는 배포프로그램을 만들어..
>
> 내용을 한번 전반적으로 C:\work\Elmo\Elmo_Master\LMC_Library에 대해 소스코드 리뷰 및 개발 문서 업데이트 한담에 체계적으로 진행해보도록하자.

<details><summary>16 previous messages</summary>

> 배포 폴더는 `LMC_Library/LMC_API_Distribution`으로 구성하겠습니다. 내부 개발 영역인 `LMC_API`·`LMC_API_Delivery`와 분리하고, 배포 예제는 소스 프로젝트 참조 없이 `Lib/LasalMotionControlLib.dll`만 상대경로로 참조하도록 만들겠습니다. 먼저 공개 API·기존 문서·예제 의존성을 전수 검토한 뒤 배포 문서와 내부 개발 문서를 각각 정리하겠습니다.
>
> 초기 검토에서 핵심 정리점이 확인됐습니다. 기존 `LMC_API/LMC_API`는 42개 테스트·18/23 활성 경로·구형 테스트 앱을 적은 이전 0.9 배포 스냅샷이라 현재 배포본으로 재사용하면 안 됩니다. 이 영역은 내부 개발·과거 근거로 명확히 분류하고, 새 배포본은 현재 46개 테스트·25개 명령 범위와 9축 단축/4축 그룹 경계를 기준으로 새로 작성하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 소스 리뷰를 마무리하면서 배포물 구조를 실제로 만들겠습니다. 우선 확인된 핵심 위험은 기존 추적 DLL이 최신 API보다 오래됐다는 점입니다. 배포 폴더에는 반드시 현재 소스를 새로 Release 빌드한 DLL만 넣고, 예제의 상대경로 참조와 API 표면까지 함께 검증하겠습니다.
>
> 배포 구조는 `LMC_API_Distribution`으로 고정하겠습니다. 여기에는 현재 소스에서 새로 빌드한 preview DLL, 독립 WPF 예제, 사용자 문서만 넣고, `LMC_API`에는 패킷 근거·개발 구조·릴리스 절차·검증 한계를 담은 내부 문서를 두겠습니다. 기존 `LMC_API/LMC_API` 패키지는 구버전이므로 “보관본”으로 명시해 새 배포본과 혼동되지 않게 하겠습니다.
>
> <details><summary>Created 12 files, edited 13 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/LMC_API/README.md` (+5 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/Properties/AssemblyInfo.cs` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+7 -1)
> - Wrote `./LMC_Library/LMC_API_Distribution/LasalApiWpfTestApp/LasalApiWpfTestApp/Properties/AssemblyInfo.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/Properties/AssemblyInfo.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+29 -21)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md` (+7 -9)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+336 -0)
> - Wrote `./LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md` (+80 -0)
> - Wrote `./LMC_Library/LMC_API/README.md` (+40 -0)
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+184 -0)
> - Wrote `./LMC_Library/LMC_API_Distribution/LasalApiWpfTestApp/LasalApiWpfTestApp.sln` (+22 -0)
> - Wrote `./LMC_Library/LMC_API_Distribution/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+74 -0)
> - Wrote `./LMC_Library/LMC_API_Distribution/LasalApiWpfTestApp/README.md` (+36 -0)
> - Wrote `./LMC_Library/LMC_API_Distribution/Docs/USER_MANUAL_KO.md` (+354 -0)
> - Wrote `./LMC_Library/LMC_API_Distribution/README.md` (+40 -0)
> - Wrote `./LMC_Library/LMC_API_Distribution/Docs/API_REFERENCE.md` (+231 -0)
> - Wrote `./LMC_Library/LMC_API_Distribution/Docs/TROUBLESHOOTING.md` (+117 -0)
> - Wrote `./LMC_Library/LMC_API_Distribution/Docs/UNIT_AND_SAFETY.md` (+103 -0)
>
> </details>
>
> 이제 fresh Release 빌드와 독립성 검증을 돌리겠습니다. 이 단계에서 46개 PC 테스트, LASAL source/network 계약, 개발 예제와 배포 예제, 저장소 밖 임시 폴더의 Debug/Release 빌드까지 한 번에 확인하고 최종 DLL 해시를 확정합니다.
>
> 빌드 결과는 정상입니다. 새 DLL은 `0.9.1-preview`, 71,680 bytes, SHA-256 `14a208…f5c8b`이며 PC 테스트 46/46, LASAL source/network 정적 계약, 개발/배포 예제 및 저장소 밖 독립 Debug/Release 빌드가 모두 통과했습니다. 이제 manifest와 문서 간 현재값을 맞추고, 배포 폴더에 내부 경로나 소스 참조가 남지 않았는지 최종 감사하겠습니다.
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+8 -6)
> - Wrote `./LMC_Library/LMC_API_Distribution/RELEASE_MANIFEST.md` (+77 -0)
> - Wrote `./LMC_Library/LMC_API_Distribution/Docs/API_REFERENCE.md` (+4 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+15 -1)
>
> </details>
>
> 리뷰에서 확인된 동일 버전의 구 DLL 충돌도 정리하겠습니다. `LMC_API_Delivery/bin`의 추적 DLL은 새 배포에서 쓰지 않고, 계속 남아 있으면 다른 DLL로 잘못 참조될 위험이 있어 제거합니다. 구형 실행 패키지는 이력 보존을 위해 `LMC_API/LMC_API`에 경고문과 함께 그대로 둡니다.
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md` (+1 -2)
> - Wrote `./LMC_Library/README.md` (+36 -0)
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+5 -0)
> - Wrote `./AGENTS.md` (+6 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+29 -1)
>
> </details>
>
> 최종 리뷰에서 세 가지 개선점을 잡았습니다. 반복 빌드마다 manifest가 자동 갱신되도록 바꾸고, 배포 폴더 내부 경로 누출 검사를 실제 script gate로 넣겠습니다. 또한 `LMC_Response.Raw/Payload`가 외부에서 원본 배열을 변조할 수 있던 점을 방어적 복사로 수정해 문서와 구현을 일치시키겠습니다. 이 수정 후 버전 DLL을 다시 빌드해 최종 해시를 확정하겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+11 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+21 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+8 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj` (+3 -2)
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+120 -5)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md` (+3 -0)
>
> </details>
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+4 -2)
> - Wrote `./LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Distribution/Docs/USER_MANUAL_KO.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Distribution/README.md` (+4 -3)
>
> </details>
</details>

배포 준비 구조를 완성했습니다. 현재 공식 발행 전 단계로, 커밋 후 clean worktree에서 배포 스크립트를 한 번 더 실행하면 됩니다.

주요 결과:

- 배포 패키지: [LMC_API_Distribution](./LMC_Library/LMC_API_Distribution/README.md)
- 사용자 매뉴얼: [USER_MANUAL_KO.md](./LMC_Library/LMC_API_Distribution/Docs/USER_MANUAL_KO.md)
- API 레퍼런스: [API_REFERENCE.md](./LMC_Library/LMC_API_Distribution/Docs/API_REFERENCE.md)
- 내부 개발 설명서: [API_DEVELOPMENT_GUIDE.md](./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md)
- 소스 리뷰 결과: [API_SOURCE_REVIEW_2026-07-15.md](./LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md)
- 재현 가능한 배포 스크립트: [Build-LmcApiDistribution.ps1](./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1)
- 전체 폴더 안내: [LMC_Library/README.md](./LMC_Library/README.md)

배포 예제는 API 소스 프로젝트를 포함하지 않으며 아래 상대경로 DLL만 참조합니다.

```text
..\..\Lib\LasalMotionControlLib.dll
```

검증 결과:

- 버전: `0.9.1-preview`
- PC 테스트: 46/46 PASS
- LASAL source/network 정적 계약: PASS
- 저장소 밖 독립 Debug/Release 빌드: PASS
- 반복 Release 빌드 해시 동일: PASS
- DLL SHA-256: `4603e663a8ba34674bdd68c1dbb293c9ff676f180558eb8bcbe563b3da878fce`
- Lib DLL과 예제 runtime DLL byte-identical
- 최종 재검토: P0~P2 잔여 이슈 없음

추가 정리:

- 오래된 `LMC_API_Delivery/bin` DLL은 제거했습니다.
- 기존 `LMC_API/LMC_API`는 구버전 보관본으로 표시했습니다.
- 현재 Git 설정 기준 축 1~9의 `IntUnits=1 mm(10000)`로 단위 문서를 바로잡았습니다.
- `LMC_Response.Raw/Payload`는 외부에서 원본 배열을 변조하지 못하도록 방어 복사를 적용했습니다.
- 현재 LASAL IDE 최신 Rebuild/PLC E2E는 아직 미검증이며 실제 PLC E2E는 `0/25`입니다.

변경분은 아직 커밋하지 않았습니다. 공식 배포 manifest에도 현재 작업 트리가 dirty 상태임을 사실대로 기록했습니다.

> 배포할꺼는 3가지야
> 1. API
> 2. 예제프로그램
> 3. API 사용설명서
>    사용 설명서는 첨부파일과 같이 API 사용법만 적어서 주면돼. API 사용설명서 다시 정리해서 만들어보자..
>
> User context
>
> Attachments:
> - Maestro Administrative and Motion API_2022_12_v2.012.pdf: `./docs/Elmo_manual/Maestro Administrative and Motion API_2022_12_v2.012.pdf`

<details><summary>13 previous messages</summary>

> 첨부된 Maestro 매뉴얼의 구성과 API 설명 방식을 기준으로 다시 만들겠습니다. PDF 작업 지침을 사용해 목차·함수 설명 형식·예제 배치를 확인한 뒤, 배포물은 `API / 예제프로그램 / API 사용설명서` 세 항목으로 단순화하고 사용설명서에서는 내부 개발 내용과 검증 이력을 빼겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 첨부 매뉴얼의 함수 설명 형식, 현재 DLL의 실제 공개 API, 배포 폴더 구조를 동시에 대조하겠습니다. 사용설명서는 내부 설계·시험 이력을 제외하고, 사용자가 호출할 함수와 순서·인자·반환·예제만 남기겠습니다.
>
> 첨부 매뉴얼은 각 API를 `기능 요약 → 선언 → 인자 → 동작 조건/주의 → 반환 → 코드 예제` 순서로 설명하고 있습니다. 이 틀을 그대로 적용하되, 현재 DLL에 없는 기능과 패킷 내부 정보는 넣지 않겠습니다.
>
> 배포 문서의 기준을 정했습니다. 최종 패키지는 `01_API`, `02_Example_Program`, `03_API_User_Manual` 세 묶음으로 보이게 하고, 설명서는 한 개의 한국어 PDF로 통합하겠습니다. 기존의 개발 검증표·백로그·패킷 ID는 내부 문서에만 남깁니다.
>
> <details><summary>Created 2 files, edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+1132 -0)
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManual.py` (+601 -0)
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManual.py` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManual.py` (+3 -0)
>
> </details>
>
> 사용설명서 초안을 PDF로 생성했습니다. 이제 표지, 자동 목차, 함수별 선언/인자 표, 긴 코드 예제가 실제 페이지에서 잘리지 않는지 전체 렌더링으로 확인하고 수정하겠습니다.
>
> <details><summary>Created a file, edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+5 -1)
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManual.py` (+11 -3)
> - Wrote `./.gitignore` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Distribution/02_Example_Program/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+1 -2)
> - Wrote `./LMC_Library/LMC_API_Distribution/02_Example_Program/README.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Distribution/README.md` (+19 -0)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API/README.md` (+6 -5)
> - Wrote `./LMC_Library/README.md` (+2 -5)
>
> </details>
>
> 새 매뉴얼은 22쪽으로 정리됐고, 표지·자동 목차·긴 표와 코드의 페이지 분할까지 정상입니다. 이제 기존의 여러 사용자 문서와 샘플을 제거하고, 세 배포 항목만 남도록 빌드 절차와 상대경로를 맞추는 중입니다.
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+278 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+7 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+1 -1)
>
> </details>
>
> 첫 통합 빌드에서 시스템 기본 Python 3.8과 설치된 PDF 라이브러리 조합이 충돌했습니다. 매뉴얼 내용 문제는 아니며, 이미 정상 생성에 사용한 호환 Python을 자동 탐색하도록 빌드 절차를 고치겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+22 -3)
> - Wrote `./LMC_Library/LMC_API/Build-LmcApiDistribution.ps1` (+4 -1)
> - Wrote `./LMC_Library/LMC_API/Generate-ApiUserManual.py` (+7 -4)
> - Wrote `./LMC_Library/LMC_API/README.md` (+2 -0)
>
> </details>
>
> 세 항목 통합 빌드가 통과했습니다. API DLL, 예제 실행 DLL, 독립 패키지 빌드, 46개 PC 테스트와 LASAL 정적 계약까지 다시 확인됐습니다. 마지막으로 배포 폴더에 내부 경로·빌드 찌꺼기·구 문서가 남지 않았는지와 PDF 추적 여부를 점검하겠습니다.
</details>

배포물을 요청한 3가지로 정리했습니다.

1. [API DLL](./LMC_Library/LMC_API_Distribution/01_API/LasalMotionControlLib.dll)
2. [예제프로그램 솔루션](./LMC_Library/LMC_API_Distribution/02_Example_Program/LasalApiWpfTestApp.sln)
3. [API 사용설명서 PDF](./LMC_Library/LMC_API_Distribution/03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf)

사용설명서는 22쪽이며 첨부된 Maestro 매뉴얼처럼 함수별로 기능, 선언, 인자, 반환값, 사용 조건과 C# 예제를 정리했습니다. 내부 패킷 구조·개발 이력·시험 보고서는 제외했습니다.

검증 결과:

- PC 자동 테스트 46/46 통과
- LASAL 정적 계약 검사 통과
- 독립 배포 예제 Debug/Release 빌드 통과

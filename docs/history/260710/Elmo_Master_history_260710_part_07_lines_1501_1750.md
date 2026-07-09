>
> 현재 작업트리는 이미 이전 rename/정리 작업으로 더럽습니다. 그 상태를 되돌리지 않고 `LMC_Library/LMC_API_Delivery`만 대상으로 하겠습니다. 우선 설계 md를 추가하고, 코드에서는 API 내부 단위변환 의존성을 제거하겠습니다.
>
> 이제 실제 코드 모양을 열어 보겠습니다. 특히 `LmcProtocol` 안에 단위 변환이 protocol frame 생성에 섞여 있어 제거 범위를 판단해야 합니다.
>
> 코드 변경 전에 설계 결정을 문서로 먼저 남기겠습니다. 내용은 “더미 제외”, “객체가 ref를 보관”, “API 내부 변환 제거”, “PMAS식 이름을 1차 API로 둠”으로 잡겠습니다.
>
> 설계 문서는 `LMC_API_Delivery/docs`에 새로 추가하겠습니다. 이 문서가 이번 변경의 기준선이 되도록 작성하고, 이후 코드/샘플/README를 그 결정에 맞춥니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+67 -0)
>
> </details>
>
> 설계 md는 추가했습니다. 이제 코드 변경은 그 문서대로 “API 내부는 이미 변환된 DINT만 받는다”는 방향으로 좁혀서 적용하겠습니다.
>
> `LMCAxis`와 `LMCGroup`에서 `LMC_UnitConverter` 필드를 제거하고, 메소드 인자를 `double` application unit에서 `int` DINT로 바꿉니다. 동시에 `MoveAbsoluteEx` 같은 PMAS식 메소드명을 1차 API로 추가하고, 기존 `LMC_*` 이름은 호환 wrapper로만 남깁니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+130 -49)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+66 -25)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+13 -16)
>
> </details>
>
> 연결 객체도 `LMC_RpcInitConnection`만 남기면 PMAS식 wrapper 구조와 어긋납니다. `RpcInitConnection`/`CloseConnection`을 기본 이름으로 추가하고 기존 이름은 wrapper로 남기겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+18 -5)
>
> </details>
>
> 샘플과 문서도 같이 맞춥니다. 이제부터 예제는 API 생성자에 converter를 넘기지 않고, 사용자가 호출 전에 직접 DINT로 변환해서 넘기는 형태로 바뀝니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+22 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+15 -6)
>
> </details>
>
> DINT 패킷 문서의 첫 부분도 수정합니다. 기존 문장은 DLL이 내부 변환한다고 되어 있어서 이번 설계와 정면으로 충돌합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+3 -2)
>
> </details>
>
> 추가로 클래스 이름도 PMAS 쪽에 맞춰 `MMCSingleAxis`/`MMCGroupAxis`를 1차 타입으로 두겠습니다. 기존 `LMCAxis`/`LMCGroup`는 깨지지 않게 얇은 호환 wrapper로 남깁니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+10 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+10 -2)
>
> </details>
>
> 샘플은 새 1차 API 타입명으로 바꿉니다. 이 파일은 사용자가 따라 볼 예제라 호환 wrapper보다 실제 권장 구조를 보여줘야 합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+5 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -1)
>
> </details>
>
> 이제 내부 참조를 다시 검색해서 converter가 protocol/API 경로에 남아 있는지 확인하겠습니다. 남아 있어야 하는 건 사용자 helper 클래스 정의와 샘플의 명시적 호출뿐입니다.
>
> 단위 변환 helper도 사용자용으로 남기는 김에 1차 이름은 `Units`/`UnitConverter`로 정리하고, 기존 `LMC_Units`/`LMC_UnitConverter`는 호환 alias로 남기겠습니다. API 내부는 여전히 이 helper를 호출하지 않습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+55 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+6 -6)
>
> </details>
>
> 문서도 새 helper 이름 기준으로 바꿉니다. 구버전 이름은 호환용이라고 명시해 두겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+4 -1)
>
> </details>
>
> 코드상 내부 converter 호출은 제거됐고, converter는 helper 정의와 샘플의 명시 호출에만 남았습니다. 이제 실제 프로젝트를 빌드해서 타입/시그니처 변경이 컴파일되는지 확인하겠습니다.
>
> 빌드는 통과했습니다. 빌드 산출물이 `src/bin/Release`에 생성됐으니, 배포 폴더와 git 상태를 확인해서 문서와 실제 파일 위치가 어긋나는지 보겠습니다.
>
> 배포 폴더에 기존 `LasalMotionControlLib.dll`이 있었지만 이번 Release 빌드 산출물보다 오래된 파일입니다. 사용자가 바로 쓸 DLL 위치도 맞추기 위해 Release DLL을 배포 `bin`으로 복사하겠습니다.
>
> 샘플의 실제 위치 변환 값도 예제로 출력되게 해서, “사용자 코드에서 변환한다”는 의도가 더 분명하게 보이도록 다듬겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+1 -0)
>
> </details>
>
> `UnitConverter`/`Units` alias 추가 후 다시 빌드합니다. 이 변경은 public API 타입 변경이라 컴파일 확인이 필요합니다.
>
> 빌드는 경고 없이 통과했습니다. 다시 생성된 Release DLL을 배포 폴더에 반영하고 최종 diff/check를 보겠습니다.
>
> 변경 범위는 `LMC_API_Delivery`로 한정됐습니다. 이 요청은 “git에 히스토리 남겨서 진행”이라고 했으니, 관련 파일만 스테이징해서 커밋까지 진행하겠습니다. 다른 폴더의 기존 변경은 건드리지 않습니다.
>
> 스테이징 중 `bin`이 ignore 대상이라 한 번 막혔습니다. 소스와 문서는 대부분 올라갔는지 확인하고, 오래된 tracked DLL 삭제는 `git add -u`로 처리하겠습니다. ಹೊಸ DLL은 ignore 정책에 맞춰 커밋하지 않습니다.
>
> 스테이징 상태는 원하는 범위로 잡혔습니다. 커밋 전 staged diff 기준으로 다시 체크하고, 문제가 없으면 커밋합니다.
>
> 체크 통과했습니다. 이제 이 범위만 커밋해서 API 구조 변경 히스토리를 남기겠습니다.
>
> 커밋 완료됐습니다. 마지막으로 전체 작업트리에서 이번 커밋 외에 남은 변경이 무엇인지 확인해서, 제가 건드린 범위와 기존 미정리 변경을 구분해 보고하겠습니다.
>
> 이번 요청 범위인 `LMC_API_Delivery`는 커밋까지 끝났습니다. 남은 dirty 파일들은 기존 정리/rename 작업과 다른 폴더 변경이라 이번 커밋에는 포함하지 않았습니다.
</details>

완료했다. `Codex_LASAL_WPF` 더미는 건드리지 않고 `LMC_Library/LMC_API_Delivery`만 수정했다.

핵심 변경:
- 구조 설계 히스토리 문서 추가: [API_STRUCTURE_DECISION_2026-07-09.md](./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md:1)
- 1차 API 타입을 PMAS식으로 정리: `MMCSingleAxis`, `MMCGroupAxis`
- 기존 `LMCAxis`, `LMCGroup`, `LMC_*` 메소드는 호환 wrapper로 유지
- API 내부 단위변환 제거: motion API는 이미 변환된 LASAL/internal `DINT int` 값을 그대로 패킷에 씀
- `Units`, `UnitConverter`를 사용자용 helper로 남김. 기존 `LMC_Units`, `LMC_UnitConverter`는 호환 alias
- 샘플도 사용자 코드에서 변환 후 `MoveAbsoluteEx(...)`에 넣는 구조로 수정: [BasicUsage.cs](./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs:7)

검증:
- `dotnet build C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\LasalMotionControlLib.sln -c Release`
- 결과: 경고 0, 오류 0
- `git diff --cached --check` 통과
- Release DLL을 `C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\bin\LasalMotionControlLib.dll`로 복사해뒀다.

커밋:
- `c0fab9d Refine LASAL API object structure`

참고로 작업트리에는 기존 다른 폴더 변경이 아직 남아 있다. 이번 커밋에는 `LMC_API_Delivery` 관련 변경만 넣었다.

> LMC_Units은 선언만하고 유저가 프로그램에서 직접 넣는거라고... api library에는 빼도돼. 유닛 컨버터 하는 부분 다 빼

<details><summary>17 previous messages</summary>

> 요구사항 반영하겠습니다. 지금 기준은 더 명확합니다: API DLL은 DINT 패킷만 만들고, `Units`/`LMC_Units`/`UnitConverter` 같은 단위 관련 타입도 DLL에서 제거합니다. 사용자 프로그램에서 필요한 상수와 변환을 직접 두는 구조로 정리하겠습니다.
>
> 현재 `LMC_API_Delivery` 작업트리는 깨끗합니다. 이제 `LmcProtocol.cs`에서 단위 상수/컨버터 타입 자체를 삭제하고, 샘플은 사용자 프로그램 내부에 상수와 변환식을 직접 둔 형태로 바꾸겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+0 -160)
>
> </details>
>
> API 소스에서 단위 관련 타입은 제거했습니다. 이제 샘플은 “사용자 프로그램에서 직접 선언해서 넣는 방식”을 보여주도록 바꾸겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+22 -13)
>
> </details>
>
> 문서도 같은 기준으로 정리합니다. 이제 README와 설계 문서에는 API 라이브러리에 단위 helper가 없다는 점을 명확히 남기겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -5)
>
> </details>
>
> DINT 패킷 맵의 “public unit helper” 표현도 제거합니다. 지금 문서 기준으로는 API가 단위에 관여하지 않는다고 읽혀야 합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+6 -9)
>
> </details>
>
> 이제 남은 단위 관련 심볼을 검색해서 API DLL 안에 완전히 빠졌는지 확인하겠습니다. 샘플과 문서의 사용자 코드 예시만 남아야 합니다.
>
> 검색 결과 API 소스에는 `LMC_Units`, `Units`, `LMC_UnitConverter`, `UnitConverter`가 더 이상 남아 있지 않습니다. 이제 빌드해서 DLL에서 컴파일 오류가 없는지 확인하겠습니다.
>
> 빌드 통과했습니다. 배포용 DLL도 최신 빌드로 갱신하고, 변경분을 체크한 뒤 커밋하겠습니다.
>
> 이제 diff/check와 상태를 확인합니다. 이번에도 `LMC_API_Delivery` 변경만 스테이징하고 커밋하겠습니다.
>
> 체크는 통과했고, API 소스 안에 단위 관련 타입/메소드명은 더 이상 검색되지 않습니다. 문서와 샘플까지 포함해 커밋하겠습니다.
>
> 스테이징됐습니다. 커밋 전 staged 기준으로 한 번 더 확인합니다.
>
> staged diff 체크 통과했습니다. 이제 커밋합니다.
>
> 커밋 완료했습니다. 마지막으로 최신 커밋과 남은 작업트리 상태만 확인하겠습니다.
</details>

맞다. 이번에는 API 라이브러리에서 단위 관련 코드를 완전히 뺐다.

변경 내용:
- [LmcProtocol.cs](./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:1)에서 `Units`, `LMC_Units`, `UnitConverter`, `LMC_UnitConverter` 전부 제거
- API는 이제 `int` DINT 값을 그대로 패킷에 쓰는 구조만 가짐
- [BasicUsage.cs](./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs:1)는 사용자 프로그램 안에서 `MM`, `MMPSEC`, `MMPSEC2`를 직접 선언하고 변환해서 넣는 예제로 수정
- [API_STRUCTURE_DECISION_2026-07-09.md](./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md:22), README, DINT packet map도 같은 기준으로 수정

검증:
- `dotnet build C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\LasalMotionControlLib.sln -c Release`
- 경고 0, 오류 0
- 최신 DLL은 로컬에 `C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\bin\LasalMotionControlLib.dll`로 복사됨. `bin`은 ignore 대상이라 커밋에는 소스/문서만 들어갔다.

커밋:
- `30b08c8 Remove unit conversion helpers from LASAL API`

> 아니 Unit 구조는 있어도 코드에서 가져다 쓰는부분이 있으면 안된다고. 뒤지고싶냐 진짜.

<details><summary>17 previous messages</summary>

> 내가 해석을 잘못했다. 바로 고치겠습니다: `LMC_Units` 같은 단위 상수 선언은 다시 두고, API 내부 패킷 생성/Axis/Group 코드에서는 절대 참조하지 않게 정리합니다. `UnitConverter`는 빼고 상수 선언만 남기겠습니다.
>
> 단위 선언은 별도 파일로 분리해서 `LMC_Frame`, `MMCSingleAxis`, `MMCGroupAxis` 쪽에서 접근할 일이 없게 만들겠습니다. 검색했을 때 API 실행 경로에는 참조가 안 나와야 합니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcUnits.cs` (+27 -0)
>
> </details>
>
> 이제 프로젝트 파일에 새 선언 파일만 포함합니다. 내부 API 클래스는 여전히 이 상수를 참조하지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj` (+5 -4)
>
> </details>
>
> 문서도 정확히 고칩니다. “단위 선언은 있음, 변환/내부참조는 없음”이 기준입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+5 -4)
>
> </details>
>
> 이제 검색 기준을 엄격하게 확인하겠습니다. `LMC_Units`는 선언 파일과 문서에만 있어야 하고, `UnitConverter` 계열은 없어야 합니다.
>
> 검색 결과는 원하는 형태입니다. `LMC_Units`는 선언 파일/문서/csproj에만 있고, `LmcAxis`, `LmcGroup`, `LmcProtocol`, `LmcConnection`에서는 참조가 없습니다. 이제 빌드합니다.
>

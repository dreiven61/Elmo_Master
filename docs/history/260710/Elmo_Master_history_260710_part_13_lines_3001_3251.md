
설계 문서에도 같은 명명 규칙을 추가했습니다: [API_STRUCTURE_DECISION_2026-07-09.md](<./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md>)

검증:
- `dotnet build LMC_Library\LMC_API_Delivery\src\LasalMotionControlLib.sln -c Release` 통과, 경고 0 / 오류 0
- `git diff --check -- LMC_Library/LMC_API_Delivery` 통과
- 기존 모호한 `LMC_Frame.Power`, `Simple`, `AxisMove`, `MoveLinear` 등 호출 잔여 없음
- 배포용 DLL도 `LMC_Library\LMC_API_Delivery\bin\LasalMotionControlLib.dll`로 복사 완료

커밋:
`82e106f Rename packet builders by LASAL API target`

기존에 남아 있던 다른 dirty/untracked 파일들은 건드리지 않았습니다.

> 이거 api 구조가 너무 이상한데?
> LMC_MoveAbsoluteExCmd가 결국적으로 MoveRelativeEx을 호출하는거자나.
> 사실 LMC_MoveAbsoluteExCmd는 사용자가 만드는 프로그램에서 호출되는 메소드인거자나. 프로그램을 왜이렇게 개떡같이 해놨어..
> 불필요한 호출이 중복되자나

<details><summary>17 previous messages</summary>

> 맞습니다. 공개 wrapper가 다른 공개 메소드를 타고 들어가는 구조는 여기서는 불필요합니다. 특히 `LMC_*Cmd`가 사용자 호출용 API라면 각 함수가 자기 명령의 패킷 빌더를 직접 호출하게 바꾸는 게 맞습니다. 지금 코드 확인해서 중복 호출을 제거하겠습니다.
>
> 정확히 말하면 현재 파일 기준으로 `LMC_MoveAbsoluteExCmd`가 `MoveRelativeEx`를 호출하진 않고 `MoveAbsoluteEx`를 호출합니다. 하지만 지적한 핵심은 맞습니다. `LMC_*Cmd` 같은 사용자용 API가 다른 공개 API를 경유하는 구조 자체가 나쁩니다. 지금 `LMC_*Cmd`와 짧은 이름 API 둘 다 같은 private command helper로 바로 들어가게 바꾸겠습니다.
>
> 수정 방향은 간단합니다. 공개 메소드끼리 서로 호출하지 않게 하고, 각 명령은 `SendMoveAbsolute`, `SendMoveRelative`, `ReadStatusValue` 같은 private helper 하나로 모읍니다. 그러면 alias는 유지하면서도 공개 API 체인은 사라집니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+101 -61)
>
> </details>
>
> `LmcAxis.cs`는 공개 메소드 간 호출을 끊었습니다. `LMC_MoveAbsoluteExCmd`는 이제 `MoveAbsoluteEx`를 안 거치고, 둘 다 동일한 private `SendMoveAbsolute`로 들어갑니다. Group 쪽도 같은 구조로 맞춥니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+64 -29)
>
> </details>
>
> 문서도 바로잡습니다. 이전 문서의 “compatibility wrapper가 primary API에 delegate한다”는 문장은 지금 기준에 안 맞습니다. 사용자 호출 API는 서로를 경유하지 않는다는 정책으로 바꿉니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+4 -2)
>
> </details>
>
> 이제 구조상 공개 메소드끼리 타고 들어가는 패턴이 남아 있는지 검색하고, 빌드로 확인합니다.
>
> 검색에서 아직 세 군데가 남았습니다. `ReadStatus()` 같은 no-response 편의 메소드가 `ReadStatus(out ...)` 공개 오버로드를 경유하고 있습니다. 이것도 같은 원칙으로 private helper로 바로 보내겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+1 -1)
>
> </details>
>
> 공개 메소드 체인을 완전히 제거했습니다. 다시 검색과 빌드 확인을 돌립니다.
>
> 검색 결과 공개 메소드가 다른 공개 메소드를 `return`으로 경유하는 패턴은 더 이상 없습니다. 빌드와 diff check도 통과했습니다. DLL 복사 후 커밋 범위를 확인하겠습니다.
>
> 변경 파일은 의도한 세 개뿐입니다. 마지막으로 diff 내용을 확인하고 stage/commit합니다.
>
> diff도 문제 없습니다. `LMC_MoveAbsoluteExCmd`와 `MoveAbsoluteEx`는 이제 같은 `SendMoveAbsolute`로 들어가고, `MoveRelative`와 섞이는 구조는 없습니다. 이 수정만 별도 커밋으로 남기겠습니다.
>
> stage 완료했습니다. 커밋 직전 staged 범위를 확인합니다.
>
> stage 범위는 설계 문서와 Axis/Group API 구조 변경 세 파일뿐입니다. 커밋합니다.
</details>

맞습니다. 그 구조는 잘못 잡은 겁니다. `LMC_*Cmd`도 사용자 호출 API인데 다른 public API를 경유하게 만든 건 불필요한 체인입니다.

수정했습니다:
- [LmcAxis.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs): `LMC_MoveAbsoluteExCmd`, `MoveAbsoluteEx` 둘 다 `SendMoveAbsolute`로 직접 진입
- `MoveRelative`, `MoveVelocity`, `Power`, `Reset`, `Stop`, `ReadStatus`, `GetActualPosition`도 public-to-public 호출 제거
- [LmcGroup.cs](./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs): Group 계열도 동일하게 private command helper로 정리
- [API_STRUCTURE_DECISION_2026-07-09.md](./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md): public user-facing method끼리 호출하지 않는다는 정책으로 문서 수정

검증:
- `dotnet build ... -c Release` 통과, 경고 0 / 오류 0
- `git diff --check -- LMC_Library/LMC_API_Delivery` 통과
- public API가 다른 public API를 `return`으로 경유하는 패턴 제거 확인

커밋:
`e649614 Remove public API wrapper chaining`

기존 unrelated dirty 파일들은 그대로 뒀습니다.

> 야 니가 봤을때 Reset이랑 LMC_Reset이랑 다를게 없자나. 이렇게 기존에 중복 호출 없애라고 그리고 기왕이면 LMC_**Cmd 이걸 지워

<details><summary>17 previous messages</summary>

> 맞습니다. 지금은 호출 체인은 없앴지만 API 표면에는 같은 기능 이름이 중복으로 남아 있습니다. 이번에는 `LMC_*Cmd` 계열과 `LMC_Reset` 같은 중복 alias 자체를 제거하고, 문서/샘플 참조까지 같이 확인하겠습니다.
>
> 현재 `LMC_API_Delivery` 안에서 중복 사용자 API는 Axis 8개, Group 7개가 잡혔습니다. `LMCConnection`의 `LMC_RpcInitConnection`, `LMC_CloseConnection`은 연결 레벨이라 이번 “모션 명령 Cmd 중복 제거” 범위와 다르지만, 필요하면 다음 단계에서 같이 정리할 수 있습니다. 먼저 Axis/Group 중복 메소드부터 삭제합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+0 -57)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+0 -40)
>
> </details>
>
> Axis/Group의 중복 alias 메소드는 삭제했습니다. 이제 문서에서 “alias may remain” 같은 이전 정책을 제거하고, public API는 한 기능당 하나만 둔다는 기준으로 고칩니다.
>
> 문서와 소스 검색 결과, Axis/Group에는 이제 `LMC_*Cmd`가 없습니다. 남은 `LMC_` public 메소드는 `LMCConnection`의 연결 alias 세 개입니다. 이것도 `RpcInitConnection`/`CloseConnection`과 중복이라 같은 기준으로 삭제하는 게 맞습니다.
>
> 연결부도 같은 원칙으로 정리합니다. `LMC_RpcInitConnection`/`LMC_CloseConnection`은 삭제하고, 기본 overload도 private 구현으로 들어가게 바꿔 public끼리 경유하지 않게 만듭니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+2 -69)
>
> </details>
>
> 방금 public overload에서 실제 구현을 빼냈으니, 같은 내용을 private `OpenRpcConnection`으로 넣어 구조를 완성합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+54 -0)
>
> </details>
>
> 이제 문서를 현재 API 표면과 맞춥니다. `LMC_*` 호환 wrapper 설명은 더 이상 사실이 아니므로 삭제하고, 중복 alias 금지로 바꿉니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+7 -7)
>
> </details>
>
> 현재 `LMC_API_Delivery` 안의 C#/MD에서는 `LMC_*` 메소드 선언/호출이 더 이상 없습니다. 이제 전체 repo에서 삭제된 API를 부르는 곳이 있는지 확인하고 빌드를 돌립니다.
>
> 전체 repo 검색에서는 추적되지 않은 테스트앱과 예전 `LMC_API` 쪽에 삭제된 이름을 부르는 코드가 남아 있습니다. 이번 커밋 범위인 `LMC_API_Delivery` 소스에서는 제거됐고 빌드도 통과했습니다. 이제 DLL 복사와 public surface 확인을 하겠습니다.
>
> 빌드된 DLL 기준으로도 `LMC_*` public instance method가 더 이상 나오지 않습니다. 이제 변경 파일과 diff를 확인하고 커밋합니다.
>
> diff는 의도한 방향입니다. `LMCConnection`까지 포함해 중복 `LMC_*` public method를 제거했고, 기본 connection overload도 private 구현으로 직접 들어갑니다. 이 다섯 파일만 stage해서 커밋하겠습니다.
>
> stage 완료했습니다. 커밋 전 staged 파일만 한 번 더 확인합니다.
>
> staged 범위는 `LMC_API_Delivery`의 다섯 파일만입니다. 기존 dirty/untracked 파일은 그대로 두고 커밋합니다.
</details>

맞습니다. 중복 API 자체를 제거했습니다.

처리한 내용:
- [LmcAxis.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs)
  - `LMC_PowerCmd`, `LMC_Reset`, `LMC_StopCmd`
  - `LMC_MoveAbsoluteExCmd`, `LMC_MoveRelativeExCmd`, `LMC_MoveVelocityExCmd`
  - `LMC_ReadStatusCmd`, `LMC_ReadActualPositionCmd`
  - 전부 삭제
- [LmcGroup.cs](./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs)
  - `LMC_GetGroupMembersInfo`, `LMC_Group*Cmd`, `LMC_MoveLinearAbsoluteExCmd`
  - 전부 삭제
- [LmcConnection.cs](./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs)
  - `LMC_RpcInitConnection`, `LMC_CloseConnection` 삭제
  - public overload가 다른 public overload를 타지 않도록 private `OpenRpcConnection`으로 정리
- README/설계문서도 “한 기능당 public API 하나만 둔다”로 수정

검증:
- `dotnet build LMC_Library\LMC_API_Delivery\src\LasalMotionControlLib.sln -c Release` 통과
- `git diff --check -- LMC_Library/LMC_API_Delivery` 통과
- 빌드된 DLL reflection 기준 `LMC_*` public instance method 없음
- `LMC_API_Delivery` 소스 기준 `public LMC_*` method 없음

커밋:
`02ea0ba Remove duplicate LMC method aliases`

주의할 점: 전체 repo에는 untracked 테스트앱 `LMC_Library/LasalMotionControlLibTestApp/`와 legacy `LMC_Library/LMC_API/` 쪽에 예전 `LMC_*Cmd` 호출이 아직 남아 있습니다. 이번 커밋에서는 현재 대상인 `LMC_API_Delivery`만 정리했고, 기존 unrelated dirty 파일은 건드리지 않았습니다.

> C:\work\Elmo\Elmo_Master\LMC_Library\LasalMotionControlLibTestApp 이거 함수호출 수정해서 현재라이브러리 수정한 결과값에 매칭되도록 바꿔

<details><summary>15 previous messages</summary>

> 테스트앱은 지금 DLL에서 지운 `LMC_*Cmd` 메소드를 아직 호출하고 있어서 컴파일이 깨지는 상태가 맞습니다. 먼저 프로젝트 파일과 현재 호출부를 확인한 뒤, 새 public API 이름으로 바로 치환하겠습니다.
>
> 확인해보니 테스트앱은 `LMC_API_Delivery\src\bin\Debug\LasalMotionControlLib.dll`을 참조합니다. 호출부만 바꾸면 안 되고, 현재 라이브러리 API가 들어간 Debug DLL도 다시 빌드해서 그 참조와 맞춰야 합니다. 먼저 `MainWindow` 코드를 읽기 가능한 형태로 확인합니다.
>
> 테스트앱은 단순 이름 문제 말고 타입도 안 맞습니다. 현재 라이브러리는 motion/stop 인자를 `int` DINT로 받는데 테스트앱은 `double/float`를 넘기고 있습니다. 호출명과 단위 변환 반환 타입을 같이 맞추고, 테스트앱이 stale DLL을 보지 않도록 프로젝트 참조도 현재 라이브러리 프로젝트로 바꾸겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
>
> XAML 버튼 표시부터 현재 API 이름으로 바꿨습니다. 이제 C# 호출부를 실제 메소드명과 `int` DINT 입력에 맞게 수정합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs` (+20 -20)
>
> </details>
>
> 호출부는 새 API로 바꿨고, `U()`도 `int` DINT를 반환하도록 수정했습니다. 이제 테스트앱 csproj가 DLL 파일을 직접 물지 않고 현재 라이브러리 프로젝트를 참조하게 바꾸겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalMotionControlLibTestApp/LasalMotionControlLibTestApp.csproj` (+10 -7)
>
> </details>
>
> 이제 오래된 `LMC_*` 호출이 남았는지 검색하고, WPF 테스트앱을 실제로 빌드해 확인합니다.
>
> 호출명 검색은 통과했는데 빌드가 XAML 생성 코드를 못 만들고 있습니다. `InitializeComponent`와 x:Name들이 전부 안 보이는 건 API 문제가 아니라 WPF targets import가 안 탄 상태입니다. csproj의 WPF 빌드 설정도 같이 고쳐서 실제 빌드가 되게 맞추겠습니다.
>
> 이 PC에는 Visual Studio 2019 Professional MSBuild가 있고, WPF target도 그쪽에 있습니다. 우선 실제 MSBuild로 빌드가 되는지 확인하고, 필요하면 csproj import를 고칩니다.
>
> Visual Studio 2019 MSBuild로는 테스트앱과 라이브러리 프로젝트 참조 빌드가 경고 0 / 오류 0으로 통과했습니다. 이제 생성물은 제외하고 소스 파일만 커밋 대상으로 잡겠습니다.
>
> `git diff`가 비어 보이는 건 이 테스트앱 폴더가 아직 Git 추적 대상이 아니기 때문입니다. 생성된 `bin/obj/.vs`는 건드리지 않고, 빌드에 필요한 소스 파일만 새로 stage하겠습니다.
>
> 소스 파일만 stage했습니다. staged diff와 남은 untracked 생성물을 분리해서 확인합니다.
>
> staged 범위는 테스트앱 소스 6개 파일뿐이고 `bin/obj/.vs`는 포함되지 않았습니다. 이제 이 변경만 커밋합니다.
</details>

수정 완료했고 커밋했습니다.

변경 내용:
- [MainWindow.xaml.cs](./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml.cs)
  - `LMC_RpcInitConnection` -> `RpcInitConnection`
  - `LMC_CloseConnection` -> `CloseConnection`
  - `LMC_PowerCmd` -> `PowerOn` / `PowerOff`
  - `LMC_Reset` -> `Reset`
  - `LMC_*Cmd` 계열 전부 현재 API 이름으로 교체
  - motion 입력 변환 `U()`를 `double/float`가 아니라 현재 라이브러리 기준 `int` DINT 반환으로 수정
- [MainWindow.xaml](./LMC_Library/LasalMotionControlLibTestApp/MainWindow.xaml)
  - 버튼 표시도 `RpcInitConnection`, `CloseConnection`으로 수정
- [LasalMotionControlLibTestApp.csproj](./LMC_Library/LasalMotionControlLibTestApp/LasalMotionControlLibTestApp.csproj)
  - stale DLL 직접 참조 제거
  - `LMC_API_Delivery/src/LasalMotionControlLib.csproj` 프로젝트 참조로 변경

검증:
- 삭제된 `LMC_*Cmd` 호출 잔여 없음
- Visual Studio 2019 MSBuild로 테스트앱 빌드 통과, 경고 0 / 오류 0
- `git diff --cached --check` 통과

커밋:
`7adead4 Update LASAL test app for current API`

기존 unrelated dirty 파일들은 건드리지 않았습니다.

> 이거 프로젝트 로드할때 CR LF 안뜨게 만들어

<details><summary>12 previous messages</summary>

# Elmo_Master_history_260617 part 02

- Source: ../Elmo_Master_history_260617.md
- Original line range: 251-500

---

<details><summary>16 previous messages</summary>

> 맞습니다. 지금 Group 탭은 “명령 버튼 모음”에 가깝고, 실제 Group Axis 운용에 필요한 준비 상태 확인/키네마틱 설정/좌표계 검증 흐름이 부족합니다. 우선 현재 WPF 구조를 보고, 바로 쓸 수 있게 Group 탭에 필요한 최소 기능을 보강하겠습니다.
>
> 구조를 보니 실제로 빠진 게 명확합니다. `Group Axes CSV`를 수동 입력에 맡기고 있고, 멤버 상태 확인/일괄 Power On/키네마틱 적용 순서가 버튼 하나씩 흩어져 있어서 Group Axis 운용 중 실수가 나기 쉽습니다. `GroupMembersInfo`에서 실제 축 이름을 자동 반영하고, 멤버별 상태 확인과 일괄 Power On, 준비 시퀀스를 추가하겠습니다.
>
> 구현은 세 가지를 넣겠습니다. 1) `GetGroupMembersInfo` 결과로 `Group Axes CSV` 자동 보정, 2) 그룹 멤버별 `ReadStatus` 진단, 3) 멤버 전체 Power On 후 GroupEnable과 Cartesian transform까지 한 번에 준비하는 버튼입니다. 이러면 지금 겪은 이름 불일치/Disabled 멤버/키네마틱 누락을 UI에서 바로 잡을 수 있습니다.
>
> 이제 파일을 수정합니다. 기존 동작은 유지하고, Group 탭에 진단/준비 버튼과 내부 헬퍼만 추가해서 지금 테스트 흐름을 깨지 않게 하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+34 -14)
>
> </details>
>
> 다음은 컨텍스트에 `Group Axes CSV` 갱신 기능을 추가합니다. 이게 있어야 `GetGroupMembersInfo`로 실제 멤버명을 가져온 뒤 `SetKinTransform`과 멤버 상태 진단이 같은 축 목록을 쓰게 됩니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs` (+5 -0)
>
> </details>
>
> 이제 Group 동작 코드를 보강합니다. 특히 `SetKinTransform` 로직은 버튼과 준비 시퀀스가 같은 내부 함수를 쓰게 해서, 수동/자동 흐름이 서로 다른 값을 쓰는 문제를 없애겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs` (+304 -41)
>
> </details>
>
> 에러 메시지도 보강하겠습니다. 같은 오류가 다시 뜰 때 단순 enum만 보이지 않고, 바로 어떤 버튼/값을 확인해야 하는지 나오게 합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+21 -0)
>
> </details>
>
> 수정은 PMAS WPF 프로젝트 쪽에 적용했습니다. 이제 컴파일만 확인해서 enum/래퍼 시그니처가 맞는지 보겠습니다.
>
> `dotnet msbuild`는 기존 프로젝트 상태와 동일하게 XAML 생성 코드를 못 잡아서 실패했습니다. 새 코드만의 컴파일 오류인지 확인하려면 Visual Studio MSBuild가 필요합니다. 우선 로컬에 `MSBuild.exe`가 있는지 찾아서 가능하면 그걸로 다시 빌드하겠습니다.
>
> 기본 PATH에는 `msbuild`가 없습니다. 전체 Program Files 재귀 검색은 시간이 걸려 중단됐고, 이제 Visual Studio 설치 도구 경로만 좁혀서 확인합니다.
>
> Visual Studio 2019 MSBuild를 찾았습니다. 이 프로젝트는 그걸로 빌드해야 XAML 생성 코드가 잡힐 가능성이 높습니다. 그 방식으로 다시 검증합니다.
>
> Visual Studio 2019 MSBuild로는 정상 빌드됐습니다. 이제 빌드 산출물까지 포함해 변경된 파일 상태를 확인하고, 실제 사용 순서를 정리하겠습니다.
</details>

Group 탭 보강해놨습니다. PMAS WPF 프로젝트만 수정했습니다.

수정 내용:
- `MMC_GetGroupMembersInfo` 실행 시 실제 `AxisNames`를 읽어서 `Group Axes CSV`를 자동으로 `a01,a02,a03,a04` 같은 실제 멤버명으로 갱신
- `Read Member Status` 버튼 추가: 그룹 멤버별 `AxisRef`, `DriveID`, `AxisErrorID`, `StatusWord` 확인
- `Power On Members` 버튼 추가: `Group Axes CSV`에 있는 축들을 일괄 Power On
- `Prepare Group MCS` 버튼 추가: 멤버 정보 읽기 → CSV 자동 갱신 → 멤버 Power On → 상태 읽기 → `GroupEnable` → Cartesian `SetKinTransform`
- `Safe Test Values` 버튼 추가: 작은 테스트값으로 `MoveLinearAbsoluteEx`용 파라미터 초기화
- `NC_ONE_GRP_MEMBER_IS_DISABLED`, `NC_COORD_SYSTEM_TYPE_OUT_OF_RANGE` 에러 메시지를 원인/조치가 보이게 개선

수정 파일:
- [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml)
- [MainWindow.GroupOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs)
- [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs)
- [PmasControllerContext.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs)

빌드 확인:
```text
MSBuild VS2019: 성공
경고 0개, 오류 0개
```

실제 사용 순서는 이제 이렇게 가면 됩니다.

```text
MMC_RpcInitConnection
MMC_GetGroupByNameCmd
MMC_GetGroupMembersInfo
Prepare Group MCS
Safe Test Values
MMCMoveLinearAbsoluteExCmd
```

`Prepare Group MCS`가 지금 수동으로 하던 준비 과정을 한 번에 묶은 버튼입니다. `a04`처럼 4번째 축은 Cartesian transform에서는 일단 extra axis로 로그에만 남기고, `a01,a02,a03`을 X/Y/Z로 씁니다.

> power off members도 만들어줘 그리고 속도, 가속도, 저크 포인트 기본값은 내가 입력한 데이터를 기준으로 넣어줘

<details><summary>9 previous messages</summary>

> 요청대로 Group 탭에 멤버 전체 Power Off를 추가하고, 그룹 모션 기본 입력값을 네가 지금 테스트에 쓰던 값 기준으로 바꾸겠습니다. 기존 `Safe Test Values` 버튼도 같은 기준값을 넣도록 맞춥니다.
>
> Power Off는 그룹이 Enabled 상태면 멤버 단일축 명령이 막힐 수 있으니, 버튼에서 먼저 `GroupDisable`을 시도한 뒤 멤버별 `PowerOff`를 날리도록 넣겠습니다. 기본값은 스크린샷에 입력된 `8388608` 계열 값으로 맞춥니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+62 -57)
>
> </details>
>
> 이제 새 버튼 핸들러와 공통 PowerOff 로직을 추가합니다. `Power Off Members`는 멤버 전원을 내리기 전에 그룹을 먼저 Disable해서 그룹 락 상태 때문에 단일축 PowerOff가 막히는 경우를 피하게 하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs` (+54 -7)
>
> </details>
>
> `Safe Test Values`라는 이름은 이제 맞지 않아서 버튼 표시도 테스트 기본값 적용 의미로 바꾸겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+6 -6)
>
> </details>
>
> 표시명까지 바꿨습니다. 이제 VS2019 MSBuild로 다시 빌드해서 새 버튼 이벤트와 값 변경이 정상인지 확인합니다.
>
> 빌드 성공했습니다. 마지막으로 변경 요약만 확인하고 마무리하겠습니다.
</details>

반영했습니다.

추가/변경:
- `Power Off Members` 버튼 추가
- 실행 시 먼저 `GroupDisable`을 시도한 뒤, `Group Axes CSV`에 있는 멤버 축들을 전부 `PowerOff`
- 이후 멤버별 `ReadStatus`까지 찍어서 꺼졌는지 바로 확인 가능
- Group Motion 기본값을 네가 입력했던 값 기준으로 변경
  - Velocity: `8388608`
  - Acceleration: `8388608000`
  - Deceleration: `8388608000`
  - Jerk: `8388608000000`
  - End Point: `8388608,8388608,8388608`
  - Transition Params: `1,1,0,0`
  - Stop Deceleration: `838860800`
  - Stop Jerk: `8388608000`
- 기존 `Safe Test Values` 버튼은 `Apply Test Defaults`로 이름 바꾸고 같은 값들을 넣도록 수정

빌드 확인:
```text
VS2019 MSBuild: 성공
경고 0개, 오류 0개
```

> 일반적인 3차원 평면으로 그룹을 동작시키고 싶은데 set kinematic을 어떻게 해야되? 그리고 프로그램 업데이트할때 cr/lf 안나오게 만들어줘

<details><summary>12 previous messages</summary>

> 일반 3D Cartesian 그룹이면 `a01->X`, `a02->Y`, `a03->Z`로 1:1 매핑하는 kinematic을 넣는 게 맞습니다. 지금 코드의 의도는 그 방향인데, 계수/입력 흐름을 더 명확하게 고정하고 CR/LF 경고도 같이 정리하겠습니다.
>
> 문서 기준으로 `MC_KIN_NODE_DEF.ulTrCoef` 순서는 `back ratio`, `forward ratio`, `back shift`입니다. 일반 1:1 Cartesian이면 각 축에 `1, 1, 0`을 넣고, 첫 3축만 X/Y/Z로 잡는 구성이 맞습니다. 보조축까지 포함된 4축 그룹이면 4번째는 Cartesian XYZ transform에서 제외해야 합니다.
>
> DLL enum을 확인해보니 X/Y/Z 외에 U/V/W와 ACS A1~A6 타입도 있습니다. 하지만 일반 3D Cartesian `MoveLinear` 테스트는 먼저 `NC_PROFILER_X/Y/Z_AXIS_TYPE` 또는 MCS X/Y/Z에 첫 3축을 1:1로 매핑하는 구성이 맞고, 4번째 축은 그룹 Enable을 위해 Power On은 하되 XYZ kinematic에서는 제외하는 게 안전합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Group 3D Cartesian은 현재 코드 방향이 맞습니다. 실제로는 `a01->X, a02->Y, a03->Z`를 1:1 변환으로 잡고, `a04`는 기본 XYZ 평면/공간 이동에서는 변환에 넣지 않는 쪽이 맞아서 그 기준으로 UI 기본값과 로그를 더 명확히 만들겠습니다.
>
> 현재 `TextGroupAxes` 기본값은 이미 실제 리소스명 기준으로 바뀌어 있습니다. 다음은 SetKinematic 로그가 “어떤 축을 어떤 Cartesian 축으로 매핑했는지” 바로 보이게 하고, 저장소 라인엔딩 정책을 추가해서 Git의 CR/LF 경고를 없애겠습니다.
>
> 라인엔딩은 `core.autocrlf=true` 환경이라, WPF 코드 파일은 CRLF로 고정하는 게 가장 덜 시끄럽습니다. `.gitattributes`를 추가하고 이번에 건드린 파일들도 CRLF로 정규화하겠습니다.
>
> <details><summary>Created a file, edited a file</summary>
>
> - Wrote `./.gitattributes` (+26 -0)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs` (+22 -1)
>
> </details>
>
> 코드 쪽 변경은 작습니다. 이제 저장된 작업 파일들의 줄끝을 실제 정책과 맞춰서 고정한 뒤, Git 검사와 MSBuild를 다시 돌려 보겠습니다.
>
> `SetKinTransform` 로그는 이제 실제 매핑과 계수를 찍게 됩니다. 빌드까지 통과하면, 사용 순서와 3D Cartesian 설정값을 바로 정리해서 알려드리겠습니다.
>
> 검사 결과 `git diff --check`는 깨끗하고, Visual Studio 2019 MSBuild도 경고 0 / 오류 0으로 통과했습니다. 마지막으로 변경 상태와 줄끝 상태만 확인하고 마무리하겠습니다.
>
> 한 가지 핵심은 `v01` 그룹 멤버가 4개라는 점입니다. 일반 XYZ 직교 3축 이동이면 변환에는 3축만 넣고, 4번째 축은 그룹 Enable 조건 때문에 Power On은 하되 Cartesian XYZ 매핑에서는 제외하는 구조로 봐야 합니다.
</details>

SetKinematic 기준은 이거야:

`v01`을 일반 XYZ Cartesian 그룹으로 쓸 거면 1:1 변환으로 잡으면 된다.

- `a01 -> X`
- `a02 -> Y`
- `a03 -> Z`
- 계수는 `BackRatio=1`, `ForwardRatio=1`, `BackShift=0`
- `iMcsToAcsFuncID = NC_TR_SHIFT_FUNC`
- `MoveLinearAbsoluteEx`는 `MC_MCS_COORD`로 실행
- End Point는 `X,Y,Z` 3개 값만 입력

`a04`가 그룹 멤버면 GroupEnable 조건 때문에 Power On은 해야 한다. 다만 일반 XYZ 이동에서는 kinematic transform에 넣지 않는다. 만약 `a04`도 같이 보간 이동해야 하면 그건 단순 XYZ가 아니라 `U/V/W` 또는 별도 auxiliary 축까지 포함한 4축 kinematic으로 다시 설계해야 한다.

프로그램도 업데이트했다.

- [MainWindow.GroupOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs:359): `SetKinTransform` 로그에 `a01->X,a02->Y,a03->Z` 매핑과 계수 `1,1,0`이 찍히게 변경
- [.gitattributes](./.gitattributes:1): `*.cs`, `*.xaml`, `*.csproj`는 CRLF로 고정해서 Git CR/LF 경고 줄이도록 설정
- 이번에 수정된 WPF 코드 파일들도 실제 CRLF로 정규화 완료

검증도 끝냈다. `git diff --check` 깨끗하고, VS2019 MSBuild 기준 `PmasApiWpfTestApp` 빌드 경고 0 / 오류 0이다.

> set kinematic이 안돼
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-c8aa94de-26c7-4e51-9a18-63ed45fdb2b6.png`

<details><summary>11 previous messages</summary>

> 에러 원인이 명확합니다. `NC_ALL_AXES_SHOULD_BE_IN_KINEMATIC`라서, `v01` 그룹 멤버가 4개면 SetKinematic에도 4개 축을 전부 넣어야 합니다. 제가 앞에서 “a04는 제외”라고 본 게 틀렸고, 이 컨트롤러는 그룹 멤버 전체가 kinematic 정의 안에 있어야 합니다.
>
> 수정 방향은 SetKinematic이 `a01->X, a02->Y, a03->Z, a04->U`처럼 모든 멤버를 포함하도록 바꾸는 것입니다. 실제 순수 XYZ 3축만 쓰려면 MDS에서 3축짜리 그룹을 따로 만들어야 하고, 4축 그룹을 쓰면 4번째 축도 kinematic node로 정의해야 합니다.
>
> 다음 수정은 프로그램 로직 오류 수정입니다. `MaxCartesianTransformAxes = 3` 때문에 4축 그룹에서 3개만 넘기고 있었고, 그게 지금 오류의 직접 원인입니다.
>
> 4번째 축을 어떻게 해석할지는 두 가지입니다. 지금은 안전하게 `a04->U`로 넣겠습니다. 그래서 endpoint도 4개 좌표 `X,Y,Z,U` 형태가 되어야 합니다. 순수 XYZ만 원하면 컨트롤러의 그룹 자체를 `a01,a02,a03` 3축으로 만들어야 합니다.
>
> Move 쪽까지 생각하면, `a04->N1`일 때 MCS position 배열에서 N1은 4번째가 아니라 7번째 슬롯입니다. 문서 예제도 `X,Y,Z,0,0,0,N1...` 순서라서, 기본 endpoint도 그 순서로 바꾸겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs` (+110 -42)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+2 -2)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+20 -10)
>
> </details>
>
> 수정했습니다. 이제 `v01`이 4축이면 SetKinematic에 4개를 모두 넣고, 매핑 로그가 `a01->X[0],a02->Y[1],a03->Z[2],a04->N1[6]`처럼 찍힐 겁니다. 빌드로 확인하겠습니다.
>
> 중요한 변경점은 `a04`를 U가 아니라 `N1`로 넣은 것입니다. Elmo 문서상 Cartesian은 X/Y/Z이고 standalone/service 축은 N1-N9라서, 4번째 보조축은 N1로 보는 게 맞습니다.
>
> 빌드에서 괄호 하나가 빠진 단순 컴파일 오류가 나왔습니다. 바로 고치고 다시 빌드하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs` (+4 -4)
>
> </details>
>
> 수정 완료했습니다. 다시 빌드합니다.
</details>

원인은 내 이전 판단이 틀린 거다. 이 컨트롤러는 문서에도 `iNumAxes`가 그룹 축 수보다 작을 수 있지만 “currently not supported”라고 되어 있고, 실제 에러도 `NC_ALL_AXES_SHOULD_BE_IN_KINEMATIC`다. 즉 `v01`이 4축 그룹이면 SetKinematic에 4축 전부 들어가야 한다.

수정 완료했다.

- [MainWindow.GroupOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs:14): 기본 kinematic 매핑을 `a01->X`, `a02->Y`, `a03->Z`, `a04->N1`로 변경

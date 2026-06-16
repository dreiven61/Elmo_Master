# 히스토리 파일 분할 분석

> 이 파일을 분석해서 이 쓰레드에서 작업을 이어 진행하도록하자. 히스토리 파일이 너무 크니까 날짜폴더 만들어서 니가 읽어들이기 편한 사이즈로 잘라서 나눠서 저장하고 개별 파일을 읽어 들여서 내용 분석해.
>
> User context
>
> Attachments:
> - Elmo_Master_history_260609.md: `./docs/history/Elmo_Master_history_260609.md`

<details><summary>8 previous messages</summary>

> 히스토리 파일 구조와 크기부터 확인한 뒤, 원본은 그대로 두고 날짜별 하위 폴더에 청크 파일을 만들겠습니다. 이후 청크별 요약을 읽어서 이 쓰레드에서 이어갈 작업과 현재 상태를 뽑겠습니다.
>
> 현재 `260609` 히스토리는 741줄, 약 42KB라 원본 자체는 크지 않습니다. 그래도 요청대로 `docs/history/260609` 폴더를 만들고 200줄 단위 분할본과 인덱스를 생성하겠습니다.
>
> 이제 분할 파일을 만듭니다. 원본은 그대로 두고, 각 조각 앞에는 원본 경로와 줄 범위를 넣어서 나중에 근거 추적이 가능하게 하겠습니다.
>
> 분할은 완료됐습니다. 이제 4개 조각을 순서대로 읽고, 실제로 이어받아야 할 작업·이미 끝난 작업·주의할 정정사항을 분리해서 요약 문서로 남기겠습니다.
>
> 읽은 내용 기준으로 현재 미완료 작업은 따로 보이지 않습니다. 대신 이 날짜 히스토리는 “PDF 복구 완료 → CoreAPI 표 레이아웃 수정 완료 → 최신 API 문서 공개 여부 확인 → Sigmatek/Elmo 블렌딩 비교 → RPC/IPC 설명”까지의 작업 맥락을 담고 있습니다. 요약 파일로 고정하겠습니다.
>
> 요약 파일은 `docs/history/260609/analysis_summary.md`에 추가합니다. 원본 히스토리 전체를 다시 읽지 않아도 이 파일과 4개 분할본만 보면 이어갈 수 있게 작성합니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/history/260609/analysis_summary.md` (+96 -0)
>
> </details>
>
> 요약 문서까지 생성했습니다. 마지막으로 분할 인덱스와 요약 파일이 정상적으로 들어갔는지 확인하고 끝내겠습니다.
</details>

분할과 분석 완료했습니다. 원본 `Elmo_Master_history_260609.md`는 건드리지 않았습니다.

생성 위치:
- [index.md](/C:/work/Elmo/Elmo_Master/docs/history/260609/index.md)
- [analysis_summary.md](/C:/work/Elmo/Elmo_Master/docs/history/260609/analysis_summary.md)
- `Elmo_Master_history_260609_part_01.md` ~ `part_04.md`

확인 결과 원본은 실제로 `741줄 / 42,931 bytes`라 아주 큰 파일은 아니었지만, 요청대로 `docs/history/260609` 폴더를 만들고 200줄 단위로 4개 파일로 나눴습니다.

분석 결론:
- 현재 히스토리 끝에는 미완료 구현 작업은 없습니다.
- 마지막 작업 맥락은 `MMC_RpcInitConnection()`의 `RPC`/`IPC` 차이 설명입니다.
- PC 기반 PMAS/WPF에서 Maestro IP로 붙는 구조라면 거의 확실히 `MMC_RPC_CONN_TYPE`을 써야 합니다.
- 이전 주요 작업 상태는 PDF 책갈피 복구 완료, CoreAPI 표 레이아웃 수정 완료, Elmo/Sigmatek 블렌딩 비교 완료로 정리했습니다.
- 현재 관련 산출물들은 git 기준으로 아직 untracked 상태입니다.

> 지금 테스트 하는데 INITIAL 명령 날리면 처음에 Syncronized warning이 뜨고 Reference 완료되면 CmdExecErrorInfo 뜨면서 멈춘다. 그후에 Syncronized warning이 해제되고.

<details><summary>4 previous messages</summary>

> 현재 증상은 “초기화 중 동기화 조건이 아직 안 맞음 → Reference 완료 직후 명령 실행 에러로 정지 → 이후 동기화 warning 해제” 흐름으로 보입니다. 먼저 로컬 문서/코드에서 `INITIAL`, `Synchronized`, `CmdExecErrorInfo`가 어디에 정의돼 있는지 확인하겠습니다.
>
> 검색 결과상 문서 안에 `103 Synchronized motion is...` 계열 에러 항목이 있습니다. 이제 해당 에러 설명과 초기화/레퍼런싱 호출부 코드를 좁혀서 확인하겠습니다.
>
> 문서 기준 에러 103은 단일축이 활성화된 그룹에 속해 있을 때 단일축 모션을 걸면 발생하는 에러입니다. 지금은 코드 쪽에서 `INITIAL` 버튼/시퀀스가 실제로 단일축 명령을 보내는지, 그룹 enable 상태를 남겨두는지 확인하겠습니다.
>
> 방금 PowerShell 범위/따옴표 문법 때문에 검색 명령이 실패했습니다. 파일에는 손대지 않았고, 같은 범위를 PowerShell 문법에 맞춰 다시 확인합니다.
</details>

지금까지 확인한 근거로는 `Synchronized warning` 자체보다, Reference 완료 직후 실행되는 다음 명령이 단일축/그룹 상태 조건을 어기면서 `CmdExecErrorInfo`로 들어가는 쪽이 더 유력합니다. `CmdExecErrorInfo`가 Elmo 쪽에서 어떤 필드/오브젝트로 찍히는지도 추가로 찾겠습니다.

> 이거 내가 다른 프로젝트에서 하던 내용이야 내용 잊어먹어

알겠습니다. 방금 `INITIAL / Synchronized warning / CmdExecErrorInfo` 내용은 이 프로젝트 맥락에서 제외하고 잊겠습니다.

파일 수정은 하지 않았습니다. 다음 요청은 `C:\work\Elmo\Elmo_Master` 기준 현재 히스토리/요약 맥락에서 이어가겠습니다.

> 그룹 정보는 다 올라오는데 그룹 Enable하면 왜 동작을 안하는거지? 개별로 Enable하면 다 정상적으로 동작함.
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-fa116639-b0d7-49a3-84b5-83dabfd1ec80.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-8c8eddd4-9b57-4ba9-9e26-2c78e60205f6.png`

원인은 화면에 그대로 나와 있습니다.

`MMCError=NC_ONE_GRP_MEMBER_IS_DISABLED`

즉 **그룹 멤버 중 최소 1개 축이 Disabled 상태**라서 `MMC_GroupEnableCmd`가 거절된 겁니다. 그룹 정보가 읽히는 것과 그룹 Enable 가능 여부는 별개입니다.

현재 그룹 `v01` 멤버는 로그 기준:

```text
AxisNum = 4
a01
a02
a03
a04
```

그런데 상단 `Axis Name`은 `a04`만 잡혀 있습니다. 만약 `a04`만 개별 Enable 했다면 그룹 Enable은 당연히 실패합니다. `a01~a04` 전부 개별 축으로 Power On/Enable 되어 있고, 에러 없이 StandStill 상태여야 그룹 Enable이 됩니다.

확인 순서:

1. `Axis Name`을 `a01`로 바꿈
2. `MMCGetAxisByNameCmd`
3. 개별 `Power On / Enable`
4. `ReadStatus`로 Disabled가 아닌지 확인
5. `a02`, `a03`, `a04`도 전부 반복
6. 그 다음 `MMCGroupReadStatusCmd`
7. 필요하면 `MMCGroupResetCmd`
8. 마지막에 `MMCGroupEnableCmd`

그리고 UI 상단의 `Group Axes (CSV)`가 `X,Y,Aux`로 되어 있는데, 실제 그룹 멤버 이름은 `a01,a02,a03,a04`입니다. 이 값은 최소한 혼동을 줄이려면 실제 그룹 축 이름에 맞춰야 합니다.

```text
a01,a02,a03,a04
```

정리하면: 통신 문제도 아니고 그룹 이름 문제도 아닙니다. **그룹 안의 4축 중 하나 이상이 아직 Disabled 상태라서 그룹 Enable이 막힌 상태**입니다.

> 그러네. 개별축도 다 enable해놓고 쓰는거네. Sigmatek에서 ProfileLock이라고 생각하면되나

거의 그렇게 보면 됩니다. 다만 1:1로 완전히 같은 개념은 아닙니다.

Elmo의 `GroupEnable`은 Sigmatek의 `ProfileLock`처럼 **여러 축을 하나의 그룹 모션 대상으로 묶어서 동기 제어 가능한 상태로 전환하는 단계**에 가깝습니다. 그래서 각 개별 축이 먼저 정상 상태여야 합니다.

차이는 이렇게 보면 됩니다.

| 개념 | Elmo | Sigmatek |
|---|---|---|
| 개별 축 준비 | 각 축 `Power On / Enable` 필요 | 각 축 servo enable 필요 |
| 그룹 준비 | `MMC_GroupEnableCmd` | `ProfileLock` 계열로 그룹/프로파일 잠금 |
| 목적 | 단일축들을 그룹 모션 객체로 활성화 | 축그룹을 동기 프로파일 계산 대상으로 고정 |
| 실패 조건 | 그룹 멤버 중 Disabled/Error/Motion 상태 있으면 실패 | 축 준비 안 됨, 그룹 구성/상태 불일치면 실패 |

즉 운용 순서는 대략:

```text
각 축 Enable
→ 각 축 StandStill 확인
→ GroupEnable
→ MoveLinearAbsolute / MoveLinearRelative 같은 그룹 명령
```

그리고 중요한 점은, `GroupEnable` 이후에는 그 축들을 개별 `MoveAbsolute` 같은 단일축 명령으로 움직이면 안 됩니다. 문서상 에러 `103 Synchronized motion is prohibited`가 그 경우입니다. 그룹에 묶인 축은 그룹 Disable 전까지 그룹 명령으로 움직여야 합니다.

그래서 실무 감각으로는:

```text
Elmo GroupEnable ≈ Sigmatek ProfileLock + 그룹 모션 준비 상태
```

정도로 이해하면 맞습니다.

> MoveLinear하려는데 이렇게 에러가 난다. 해결해줘
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-3bc334f2-eb2a-4959-b9a6-5d4f69437f66.png`

<details><summary>3 previous messages</summary>

> 에러 코드가 핵심입니다. `NC_COORD_SYSTEM_TYPE_OUT_OF_RANGE`라서 축 Enable 문제가 아니라, `MoveLinearAbsolute`에 넣은 좌표계 값이 이 명령에서 허용되지 않는 상태입니다. 코드와 문서에서 `MoveLinearAbsolute`/`MoveLinearAbsoluteEx`의 좌표계 파라미터 처리를 바로 확인하겠습니다.
>
> 코드상 지금 누른 버튼은 `MoveLinearAbsolute`이고, UI의 Coord System 콤보는 `MoveLinearAbsoluteEx`에서만 명시적으로 전달됩니다. 그런데 에러가 좌표계 범위 오류로 나오는 걸 보면, 현재 그룹에 좌표계/키네마틱 설정이 안 맞거나, `SetKinTransform`이 실패한 상태에서 MCS 좌표 모션을 시도한 것으로 봐야 합니다. 문서의 해당 함수 시그니처만 더 좁혀 확인합니다.
>
> 문서상 `MoveLinearAbsolute`는 좌표계 파라미터를 직접 받지 않는 단순 오버로드이고, 실제 좌표계는 그룹에 설정된 키네마틱/좌표계 상태를 따라갑니다. 스크린샷 로그에는 직전에 `MMC_SetKinTransform`이 `NC_NODE_NOT_FOUND`로 실패한 흔적이 있어서 그 상태에서 MCS 모션을 넣으니 좌표계 오류가 나는 흐름입니다.
</details>

원인은 `MoveLinear` 값이 아니라 **좌표계/키네마틱 설정 순서 문제**입니다.

스크린샷 로그에 이미 답이 있습니다.

```text
MMCError=NC_COORD_SYSTEM_TYPE_OUT_OF_RANGE
```

그리고 직전에 이 로그가 있습니다.

```text
MMC_SetKinTransform failed:
MMCError=NC_NODE_NOT_FOUND
```

즉 현재 흐름은 이겁니다.

```text
GroupEnable 성공
→ SetKinTransform 실패
→ MCS 좌표계로 MoveLinear 시도
→ 그룹에 유효한 MCS/Cartesian transform이 없어서 좌표계 오류
```

화면 상단 `Group Axes (CSV)`가 아직:

```text
X,Y,Aux
```

로 되어 있습니다. 그런데 실제 그룹 멤버는 이전 로그 기준:

```text
a01
a02
a03
a04
```

그래서 `MMCSetKinTransform`이 `X`, `Y`, `Aux`라는 축 이름을 찾다가 `NC_NODE_NOT_FOUND`가 난 겁니다.

해결 순서:

1. `Group Axes (CSV)`를 실제 축 이름으로 바꾸세요.

```text
a01,a02,a03
```

우선 3축 Cartesian으로 테스트하세요. `a04`가 보조축이면 나중에 별도 처리해야 합니다.

2. 다시 실행 순서:

```text
MMCGetGroupByNameCmd
각 축 a01~a04 개별 Power On
MMCGroupEnableCmd
MMCSetKinTransform
MMCMoveLinearAbsoluteExCmd
```

3. `Coord System`은 `MC_MCS_COORD` 그대로 두되, 반드시 `MMCSetKinTransform`이 성공한 뒤에 해야 합니다.

4. 지금은 `MMCMoveLinearAbsoluteCmd` 말고 `MMCMoveLinearAbsoluteExCmd`로 테스트하세요. Ex 쪽은 `Coord System`, `Transition Mode`, Acc/Dec/Jerk를 명시적으로 넣기 때문에 원인 추적이 쉽습니다.

추가로 처음 테스트 값은 너무 큽니다.

```text
8388608,8388608,8388608
```

좌표계 문제가 해결되면 다음에는 limit/position range 에러가 날 수 있습니다. 첫 테스트는 작게 하세요.

```text
End Point: 1000,0,0
Velocity: 1000
Acceleration: 1000
Deceleration: 1000
Jerk: 100000
Transition Mode: MC_TM_NONE_MODE
```

정리하면: **그룹은 Enable 됐지만, MCS 좌표계를 쓰기 위한 Cartesian kinematic transform 설정이 실패한 상태입니다.** `Group Axes (CSV)`를 실제 축명 `a01,a02,a03`로 바꾸고 `MMCSetKinTransform` 성공 확인 후 MoveLinear 하세요.

> 지금 Group탭이 Group Axis를 돌리기에 아직 기능이 부족한거같다.

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
- [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:811): End Point 기본값을 `8388608,8388608,8388608,0,0,0,0`로 변경
- [MainWindow.xaml.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:162): `NC_ALL_AXES_SHOULD_BE_IN_KINEMATIC` 안내 메시지 추가

MCS position 순서는 이거다:

`X,Y,Z,U,V,W,N1,N2...`

그래서 4번째 축 `a04`를 `N1`로 쓰면 End Point에서 7번째 값이 `a04/N1`이다. 지금 기본값 `8388608,8388608,8388608,0,0,0,0`은 X/Y/Z만 이동하고 N1은 0 목표다. Absolute 이동에서 N1을 움직이면 안 되는 상황이면 마지막 `0` 대신 현재 N1 위치를 넣어야 한다. Relative 이동이면 N1 `0`이 “움직이지 않음”이다.

빌드는 통과했다.
`PmasApiWpfTestApp -> C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\bin\Debug\PmasApiWpfTestApp.exe`
경고 0 / 오류 0. 현재 실행 중인 앱은 닫고 새 빌드 exe로 다시 실행해야 반영된다.

> SetKinematic을 했는데 MoveLinear가 동작을 안해

<details><summary>13 previous messages</summary>

> 먼저 확인할 건 두 가지입니다. `MMC_MoveLinearAbsoluteCmd` 버튼은 coord system/acc/dec/jerk를 안 넘기는 단순 overload라 지금 단계에서는 쓰지 말고, `Prepare Group MCS` 이후 `MMC_MoveLinearAbsoluteExCmd`만 기준으로 봐야 합니다. 로그와 코드 흐름을 다시 확인해서 앱에서 실수하기 어렵게 보완하겠습니다.
>
> 현재 앱은 End Point를 입력한 개수만큼 `double[]`로 넘기고 있습니다. Elmo 쪽 구조체는 `dbPosition[NC_MAX_NUM_AXES_IN_NODE]` 16개 배열이라, wrapper가 짧은 배열을 어떻게 처리하는지 애매합니다. 이것도 명확하게 16개로 패딩해서 넘기도록 바꾸겠습니다.
>
> 가능성이 큰 원인은 버튼 선택입니다. `MMC_MoveLinearAbsoluteCmd` 단순 버튼은 Elmo C++ wrapper의 기본 파라미터에 의존해서 좌표계/가감속/저크가 명시되지 않습니다. 지금 앱에서는 그 버튼도 UI의 MCS/acc/dec/jerk 설정을 타도록 바꾸는 게 맞습니다.
>
> 또 하나는 순서입니다. 지금 `Prepare Group MCS`가 `GroupEnable -> SetKinematic` 순서인데, motion 준비 관점에서는 `PowerOn -> SetKinematic -> GroupEnable`이 더 맞습니다. SetKinematic 후에 그룹을 다시 Enable하도록 순서를 바꾸겠습니다.
>
> 그리고 앱 시작 기본값도 `MC_NONE_COORD`일 가능성이 있습니다. `Apply Test Defaults`를 안 누르면 좌표계가 잘못될 수 있으니, Group 탭 기본 좌표계를 처음부터 `MC_MCS_COORD`로 잡겠습니다.
>
> 추가로 진단 버튼이 필요합니다. Move 명령이 “접수됐는데 이동 안 함”인지, 아예 target/actual이 안 바뀌는지 보려면 Group Actual/Target Position을 바로 읽어야 합니다. 이 버튼도 붙입니다.
>
> 한 가지 컴파일 전 확인했는데, 현재 코드에 `SelectComboItemByName` 호출이 있는데 정의가 안 보입니다. 이전 빌드가 통과했으니 다른 partial에 있을 가능성이 있어 전체 검색해서 맞춰 넣겠습니다.
>
> Move 명령은 비동기라 `completed` 로그가 “이동 완료”가 아니라 “명령 접수”일 수도 있습니다. 그래서 Move 전후에 actual/target position을 찍어, 명령이 접수됐는지와 target이 바뀌는지를 볼 수 있게 하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs` (+157 -81)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs` (+61 -12)
>
> </details>
>
> 코드 흐름은 이제 `MoveLinearAbsoluteCmd` 버튼을 눌러도 내부적으로는 좌표계/가감속/저크가 명시되는 Ex 호출을 사용합니다. 버튼명은 테스트 API 이름을 보존하되, 실제 호출 안정성을 우선했습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+14 -9)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+42 -14)
>
> </details>
>
> 이제 사용 흐름은 단순해집니다. 새 빌드에서는 `Prepare Group MCS` 하나로 PowerOn, SetKinematic, GroupEnable까지 끝나고, Move 버튼은 명시적인 MCS 파라미터로 명령을 날립니다.
>
> 빌드 통과했습니다. 남은 건 실제 장비에서 재테스트인데, 이번 변경으로 로그가 훨씬 더 구체적으로 나올 겁니다: 요청 벡터, actual/target position, EndMotion 이벤트까지 한 번에 확인됩니다.
</details>

원인 후보가 두 개였고, 둘 다 앱에서 막히게 수정했다.

1. `Prepare Group MCS` 순서가 잘못돼 있었다.
   기존: `PowerOn -> GroupEnable -> SetKinematic`
   수정: `PowerOn -> SetKinematic -> GroupEnable`

2. `MMC_MoveLinearAbsoluteCmd` 버튼이 좌표계/가감속/저크를 명시하지 않는 단순 overload를 쓰고 있었다.
   이제 이 버튼도 내부적으로 `MoveLinearAbsoluteEx`를 호출해서 `MC_MCS_COORD`, acc/dec/jerk, transition 값을 명시해서 보낸다.

추가로 [Read Group Pos](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:752) 버튼을 만들었다. Move 전후로 Actual/Target position도 자동 로그에 찍히게 했다.

테스트 순서는 이렇게 해:

1. 실행 중인 앱 닫고 새 빌드 실행
   `C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\bin\Debug\PmasApiWpfTestApp.exe`

2. Connect

3. `Prepare Group MCS` 클릭
   로그에 이런 식으로 나와야 정상:
   `Mapping=a01->X[0],a02->Y[1],a03->Z[2],a04->N1[6]`

4. 처음 테스트는 Absolute보다 Relative가 안전함.
   End Point를 이렇게 넣고 `MMC_MoveLinearRelativeCmd` 클릭:
   `100000,0,0,0,0,0,0`

빌드 확인 완료: 경고 0 / 오류 0.
그래도 안 움직이면 이제 로그에 `MoveLinear... request`, `Before/After Actual`, `Before/After Target`, `EndMotion`이 찍힐 거라 그 부분 보면 원인이 바로 나온다.

> 명령을 이렇게 줬는데 왜 u축은 안도냐?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-7d756d8f-10a1-4b31-93d2-80123917d8f2.png`

<details><summary>5 previous messages</summary>

> 지금 안 도는 이유는 로그 그대로입니다. 현재 프로그램은 `a04`를 `U`가 아니라 `N1[6]`로 매핑해 놨고, 네가 입력한 4번째 값은 `U[3]` 슬롯입니다. 그래서 `U`에는 물려 있는 축이 없고, `a04=N1`에는 7번째 값이 0으로 들어가서 안 돈 겁니다.
>
> 이건 설정을 바꿔야 합니다. `a04->N1`로 해석했던 걸 `a04->U`로 바꾸고, 기본 endpoint 생성도 `X,Y,Z,U,V,W,N1...` 순서에서 실제 매핑된 축 수만큼 바로 값이 들어가게 고치겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs` (+20 -18)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+2 -2)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+2 -2)
>
> </details>
>
> 수정했습니다. 이제 4축 그룹 매핑은 `a04->U[3]`이고, 네가 입력한 4번째 값이 실제 a04로 갑니다. 빌드 확인하겠습니다.
>
> 빌드 후에는 기존 실행 중인 앱을 반드시 닫고 새 exe를 실행해야 합니다. 기존 프로세스에는 아직 `a04->N1` 매핑이 남아 있습니다.
</details>

안 돈 이유는 네 명령 문제가 아니라 내 매핑이 문제였다.

기존 프로그램은 4번째 축 `a04`를 `U`가 아니라 `N1[6]`로 잡고 있었다.
그런데 네가 입력한 값은:

`8388608,8388608,8388608,8388608`

이건 `X,Y,Z,U` 순서라서 4번째 값은 `U[3]`에 들어간다. 기존 매핑에서는 `U`에 연결된 축이 없으니 a04가 안 도는 게 정상이다.

수정했다.

- 기존: `a01->X[0], a02->Y[1], a03->Z[2], a04->N1[6]`
- 변경: `a01->X[0], a02->Y[1], a03->Z[2], a04->U[3]`

기본 End Point도 4축 기준으로 바꿨다:

`8388608,8388608,8388608,8388608`

빌드도 통과했다. 경고 0 / 오류 0.

테스트할 때는 실행 중인 앱 닫고 새 exe 실행한 뒤:

`C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\bin\Debug\PmasApiWpfTestApp.exe`

그 다음 `Prepare Group MCS` 로그에서 반드시 이게 보여야 한다:

`Mapping=a01->X[0],a02->Y[1],a03->Z[2],a04->U[3]`

그 상태에서 지금처럼 `MMC_MoveLinearRelativeCmd` 주면 U도 같이 돌아야 한다.

> 이 페이지에서 MMCReadPIVarUShort랑 MMCWritePIVarUShort만 동작하는데 이거 내용이랑 다른 api들은 왜 동작안되는지 분석해줘. 그리고 내용도 설명해줘
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-b6818a88-09bb-468e-a59e-d6484eb8fdfb.png`

<details><summary>6 previous messages</summary>

> PI/Bulk 탭은 실제로 동작 조건이 서로 다릅니다. 코드가 어떤 wrapper를 호출하는지와 매뉴얼의 입력 의미를 같이 확인해서, “정상 동작하는 UShort PI”와 “안 되는 나머지”를 분리해서 보겠습니다.
>
> 현재 화면의 “Bulk Read”는 PI bulk read가 아니라 일반 parameter bulk read wrapper입니다. 이 탭 이름 때문에 헷갈릴 수 있는데, PI UShort read/write와 BulkRead는 완전히 다른 API 계열입니다. 세부 enum/문서까지 확인해서 결론 내리겠습니다.
>
> UShort가 되는 건 Index 0, Direction INPUT, VAR_TYPE.USHORT 조합이 실제 PI 매핑 타입과 맞기 때문입니다. 타입이 다른 Read/Write 함수는 같은 index에 던지면 실패하는 게 정상입니다.
>
> `MMC_GetPIVarInfoByAlias`가 안 되는 직접 이유는 화면에서 Alias가 비어 있기 때문입니다. 이 API는 index를 조회하는 게 아니라 alias 문자열로 PI 항목을 역검색하는 API라, alias가 비어 있으면 앱 코드가 호출 전에 막습니다.
>
> Bulk 쪽은 현재 `Preset=None` + `Use preset` 체크 상태입니다. 이 조합은 “읽을 parameter preset이 없음”이라 실제로 설정할 항목이 없습니다. BulkRead를 쓰려면 preset 1~5를 고르거나, 체크를 끄고 Custom Values에 실제 parameter ID들을 넣어야 합니다.
>
> 코드 기준으로 결론이 나왔습니다. 이 화면에서 실제 구현된 PI Read/Write는 UShort 하나뿐이고, Bulk Read는 PI가 아니라 일반 axis parameter bulk read라서 입력 의미가 다릅니다.
</details>

결론부터 말하면, 지금 화면에서 정상 동작하는 게 `MMCReadPIVarUShort` / `MMCWritePIVarUShort`뿐인 건 이상한 게 아니다. 현재 앱 구현과 입력값 기준으로 그렇게 되는 구조다.

**PI 쪽**
[MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs:28)에서 실제 구현된 PI Read/Write는 `VAR_TYPE.USHORT` 하나뿐이다.

- `Index`: PI variable index다. CANopen index가 아니다.
- `Direction`: Read/Info에서만 의미 있다. `ePI_INPUT=0`, `ePI_OUTPUT=1`.
- `USHORT Value`: UShort PI 값을 읽거나 쓸 때 쓰는 값.
- 현재 `Index=0`, `Direction=ePI_INPUT`에서 값 `689`가 읽힌다는 건, 해당 PI 입력 index 0이 실제로 `Unsigned Short` 타입이라는 뜻이다.
- 다른 타입 API, 예를 들어 BOOL/INT/FLOAT로 같은 index를 읽으면 타입이 안 맞아서 실패하는 게 정상이다. PI 변수는 index마다 타입이 정해져 있고, API 타입과 `ucVarType`이 맞아야 한다.

`MMC_GetPIVarInfoByAlias`가 안 되는 이유는 화면상 `Alias`가 비어 있기 때문이다. 앱 코드도 빈 Alias면 바로 막는다: [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs:15). 이 API는 index로 조회하는 게 아니라 alias 문자열로 PI 정보를 찾는 API다. 매뉴얼상 alias는 PI 매핑에 등록된 이름이어야 하고, 결과로 `usPIVarOffset`, `ucDirection`, `ucVarType`, bit offset/size 등을 돌려준다.

**Bulk Read 쪽**
아래 Bulk Read는 PI Bulk Read가 아니다. 현재 앱은 `MMCBulkRead`를 쓰고 있고, 이건 “axis parameter bulk read”다: [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs:69).

그래서 입력 의미가 다르다.

- `Node Refs`: PI index가 아니라 axis/node reference다. 예: `a01` AxisRef가 0이면 `0`을 넣어야 한다.
- `Bulk Config`: bulk read 설정 버퍼 번호다.
- `Bulk Preset`: 미리 정의된 parameter 묶음이다.
- 지금 화면처럼 `Use preset` 체크 + `Bulk Preset = eNC_BULKREAD_PRESET_NONE`이면 읽을 preset이 없다.
- `Custom Values`는 preset을 안 쓸 때 읽을 parameter ID 목록이다. 현재 `0`은 의미 있는 parameter ID가 아닐 가능성이 크다.

즉 현재 페이지에서 안 되는 이유는 세 가지다.

1. `GetPIVarInfoByAlias`: Alias가 비어 있거나 실제 PI alias가 아님.
2. 다른 PI 타입 API: 앱에 UShort만 구현되어 있고, 실제 index 0 타입도 UShort라 UShort만 맞음.
3. Bulk Read: PI용 API가 아니라 parameter bulk read이며, 현재 preset/custom 입력이 유효하지 않음.

정확히 분석하려면 이 탭에 `MMC_GetPIVarInfo(index, direction)` 버튼이 필요하다. 그걸로 index 0의 `ucVarType`, alias, CANopen index/subindex를 먼저 읽으면 어떤 타입 API를 써야 하는지 바로 나온다.

> PI
> Alias
> Bulk
> 가 어떤걸 의미하는지 알려줘

**PI**
PI는 `Processing Image`다. EtherCAT cyclic frame에 매 주기 갱신되는 I/O 데이터 영역이다.

예를 들면 드라이브의 상태워드, 컨트롤워드, 실제 위치, 목표 위치 같은 PDO 데이터가 PI에 매핑될 수 있다.
`MMC_ReadPIVarUShort(Index=0, Direction=ePI_INPUT)`은 “축의 입력 PI 영역에서 0번 PI 변수를 UShort로 읽어라”는 뜻이다.

`Direction` 의미:

- `ePI_INPUT`: 드라이브/슬레이브 -> 마스터로 들어오는 PI
- `ePI_OUTPUT`: 마스터 -> 드라이브/슬레이브로 나가는 PI

**Alias**
Alias는 PI 변수에 붙은 이름이다. 사람이 보기 쉽게 붙인 식별자다.

예를 들어 어떤 PI 변수가 실제로는:

- CANopen index: `0x6041`
- subindex: `0`
- type: `USHORT`
- direction: `INPUT`

이라면 alias가 `StatusWord` 또는 문서 형식상 `I.6041.0` 같은 이름으로 등록될 수 있다.

`MMC_GetPIVarInfoByAlias`는 이 alias 문자열을 넣어서 “이 alias가 PI index 몇 번이고, 타입이 뭔지, input/output 중 어디인지”를 찾는 API다. Alias를 모르면 동작하지 않는다.

**Bulk**
Bulk는 여러 항목을 한 번에 읽는 기능이다. “묶음 읽기”다.

주의할 점은 현재 앱의 `Bulk Read`는 PI bulk가 아니라 일반 parameter bulk read다. 즉 PI index를 읽는 게 아니라 여러 축의 여러 parameter를 한 번에 읽는 기능이다.

현재 화면 기준:

- `Node Refs`: 읽을 축/노드 번호. 예: `a01`의 AxisRef가 0이면 `0`
- `Bulk Config`: bulk read 설정 슬롯
- `Bulk Preset`: 미리 정의된 parameter 묶음
- `Custom Values`: preset을 안 쓸 때 직접 넣는 parameter ID 목록

정리하면:

`PI` = EtherCAT 주기 데이터 영역
`Alias` = PI 변수 이름
`Bulk` = 여러 데이터/파라미터를 한 번에 읽는 방식

> PI는 PDO 묶음이라고 봐도 되는건가? 아님 PDO중에 하나를 찝어서 읽을수 있는건가?

거의 맞는데 표현을 정확히 하면:

`PI = PDO들이 매핑되어 만들어진 Process Image 메모리 영역`이다.

그리고 `MMC_ReadPIVar...`는 그 PI 영역 안에서 **PDO에 매핑된 변수 하나를 index로 찝어서 읽는 API**라고 보면 된다.

예시:

- EtherCAT TxPDO에 `0x6041:00 Statusword`가 매핑됨
- Maestro가 이걸 PI Input 영역의 어떤 index, 예를 들어 `PI index 0`에 배치함
- 그러면 `MMC_ReadPIVarUShort(index=0, direction=ePI_INPUT)`으로 그 Statusword 하나를 읽을 수 있음

반대로:

- EtherCAT RxPDO에 `0x6040:00 Controlword`가 매핑됨
- PI Output 영역의 어떤 index에 배치됨
- 그러면 `MMC_WritePIVarUShort(index=..., value=..., type=USHORT)`로 그 output PI 변수 하나를 쓸 수 있음

중요한 구분:

- `PDO`: EtherCAT에서 주기적으로 교환하는 통신 매핑 단위
- `PI`: 그 PDO 데이터가 마스터 내부에 펼쳐진 메모리 이미지
- `PIVar`: PI 안의 개별 변수 하나
- `PI index`: `0x6041` 같은 CANopen object index가 아니라, Maestro가 PI 변수에 붙인 순번/offset

그래서 네 질문에 답하면:

`PDO 묶음`이라기보다는, **PDO 매핑 결과 전체가 PI이고, 그중 변수 하나를 PIVar API로 찝어서 읽고 쓴다**가 정확하다.

`MMC_GetPIVarInfo(index, direction)`을 쓰면 그 PI index가 실제로 어떤 CANopen index/subindex/type/alias에 해당하는지 확인할 수 있다.

> 지금 그럼 인덱스 0에 689면 머가 써져있는거임?

지금 화면 기준으로는 **`ePI_INPUT`의 PI index 0을 UShort로 읽었더니 값이 `689`**라는 뜻이다. 네가 쓴 값이라기보다, 드라이브/슬레이브가 마스터로 보내는 입력 PI 값이다.

`689`를 16진수로 보면:

```text
689(dec) = 0x02B1
binary   = 0000 0010 1011 0001
set bits = 0, 4, 5, 7, 9
```

이 PI index 0이 만약 일반적인 CiA402 `Statusword(0x6041:00)`라면 의미는 대략 이렇다.

```text
bit 0 = Ready to switch on
bit 4 = Voltage enabled
bit 5 = Quick stop
bit 7 = Warning
bit 9 = Remote
```

즉 `0x02B1`이면 “Ready 상태이고 전압은 enabled, remote 상태, warning bit가 켜져 있음” 정도로 해석된다. 단, bit 1/2가 안 켜져 있으니 `Switched on` / `Operation enabled` 상태는 아니다.

하지만 **확정은 아직 못 한다.** PI index 0이 진짜 `0x6041 Statusword`인지 확인하려면 `MMC_GetPIVarInfo(index=0, direction=ePI_INPUT)`를 읽어야 한다. 그 결과에 `CanOpenIndex`, `SubIndex`, `VarType`, `Alias`가 나온다.

지금 앱에는 `GetPIVarInfoByAlias`만 있고, index로 조회하는 `GetPIVarInfo` 버튼이 없어서 이 확인이 바로 안 된다. 그 버튼 추가하면 `index 0 = 0x6041:00 Statusword`인지 바로 판별 가능하다.

> Alias는 머를 넘겨줘야됨?

`Alias`에는 **PI 변수에 등록된 alias 문자열**을 넘겨야 한다. 임의 이름이 아니다.

매뉴얼 기준 기본 형식은 보통:

```text
I.<CANopenIndex>.<SubIndex>
O.<CANopenIndex>.<SubIndex>
```

예:

```text
I.6041.0
O.6040.0
I.6064.0
O.607A.0
```

의미:

- `I` = input PI, 드라이브 -> 마스터
- `O` = output PI, 마스터 -> 드라이브
- `6041` = CANopen object index
- `0` = subindex

그래서 지금 네가 읽는 `ePI_INPUT index 0 = 689`가 Statusword일 가능성을 확인하려면 Alias에 먼저 이걸 넣어봐:

```text
I.6041.0
```

성공하면 결과의 `usPIVarOffset`이 PI index고, 그 값이 `0`이면 지금 읽은 index 0이 Statusword가 맞다.

다만 실제 프로젝트에서 EAS/MDS XML에 별도 alias/display name을 넣어둔 경우에는 `StatusWord`, `CW`, `SW` 같은 이름일 수도 있다. 그래서 확실한 방법은 `MMC_GetPIVarInfo(index, direction)`으로 index를 먼저 조회해서 `pAliasing`을 보는 것이다. 지금 앱에는 그 버튼이 없으니 추가하는 게 맞다.

> I.6041.0
>
> [17:13:48] value : NC_PI_INFO_BY_ALIAS
>   uiBitSize = 808857161
>   uiBitOffset = 808333620
>   usCanOpenIndex = 0
>   usPIVarOffset = 0
>   ucCanOpenSubIndex = 0
>   ucVarType = 0
>   ucDirection = 0
>   ucPadding = 0
> [17:13:48] MMC_GetPIVarInfoByAlias completed.
>
> 결과가 이렇게 나왔는데 이걸 가지고 PI 인덱스를 어떻게 보는거임?

<details><summary>8 previous messages</summary>

> 지금 출력은 신뢰하면 안 됩니다. `usPIVarOffset`가 PI index이긴 한데, `uiBitSize=808857161`, `uiBitOffset=808333620`은 물리적으로 말이 안 되는 값이라 alias 조회 실패 또는 wrapper return code를 앱이 무시한 상태로 보입니다.
>
> 앱도 고치겠습니다. 현재 `GetPIVarInfoByAlias`의 반환값을 확인하지 않아서, 실패했는데도 `completed`라고 보일 수 있습니다. index로 직접 조회하는 `MMC_GetPIVarInfo` 버튼도 추가해서 `index 0`이 실제로 뭔지 바로 보이게 하겠습니다.
>
> `usPIVarOffset`가 PI index라는 점은 맞지만, 이번 결과는 `CanOpenIndex=0`, bit size가 수억 단위라 실패값입니다. 이걸 그대로 index 0이라고 해석하면 안 됩니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs` (+95 -2)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+14 -9)
>
> </details>
>
> 앱에 `MMC_GetPIVarInfo` 버튼을 추가했고, alias 조회는 반환값과 값 유효성을 확인하도록 바꿨습니다. 이제 실패한 alias 조회가 `completed`로 보이지 않게 됩니다. 빌드 확인하겠습니다.
>
> 빌드에서 `GetPIVarInfo` wrapper가 `out`이 아니라 `ref`를 요구하는 걸 확인했습니다. 호출부만 맞춰 고치겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs` (+2 -2)
>
> </details>
>
> 수정했습니다. 다시 빌드합니다.
</details>

`usPIVarOffset`가 PI index다.
하지만 네가 받은 이 결과는 **유효한 결과가 아니다**.

```text
uiBitSize = 808857161
uiBitOffset = 808333620
usCanOpenIndex = 0
usPIVarOffset = 0
```

`uiBitSize`가 수억 bit일 수 없고, `I.6041.0`을 넣었는데 `usCanOpenIndex=0`인 것도 말이 안 된다. 즉 alias 조회가 실패했거나 wrapper가 실패값/쓰레기값을 반환했는데, 기존 앱이 return code를 안 보고 `completed`라고 찍은 상태다.

정상 결과라면 이런 식으로 나와야 한다.

```text
PIVarIndex = 0
Direction = ePI_INPUT(0)
BitSize = 16
BitOffset = ...
CanOpen = 0x6041:0
VarType = ePI_UNSIGNED_SHORT(4)
```

그때 `PIVarIndex` 또는 원 구조체의 `usPIVarOffset`이 `MMC_ReadPIVarUShort`에 넣는 PI index다.

앱도 고쳤다.

- `MMC_GetPIVarInfoByAlias`가 return/유효성 확인 없이 `completed` 찍던 문제 수정
- `MMC_GetPIVarInfo` 버튼 추가
- alias 결과를 사람이 읽기 쉽게 `PIVarIndex`, `CanOpen`, `VarType`, `Direction`으로 로그 출력

빌드 통과: 경고 0 / 오류 0.

다음 확인은 이렇게 해:

1. 새 exe 실행
2. `Index = 0`, `Direction = ePI_INPUT`
3. 새로 추가된 `MMC_GetPIVarInfo` 클릭

이 결과에서 `CanOpen=0x6041:0`이면 지금 읽은 `689`는 Statusword가 맞다. 거기에 찍히는 `Alias`가 실제로 `GetPIVarInfoByAlias`에 넣어야 하는 정확한 문자열이다.

> 요래 나오는데
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-138f5aeb-fcda-4ac1-a9c1-c261ccb93cca.png`

<details><summary>5 previous messages</summary>

> `Return=-421`이 핵심입니다. alias 조회는 실패했고, 뒤에 찍힌 `PIVarIndex=0`, `BitSize=808...` 같은 값은 실패 후 구조체에 남은 쓰레기값입니다. `-421`이 어떤 에러명인지 로컬 enum/문서에서 바로 확인하겠습니다.
>
> `-421`은 `NC_PI_VAR_NOT_FOUND`입니다. 즉 `I.6041.0`이라는 alias가 컨트롤러 PI alias table에 없습니다. 지금 메시지는 정상이고, alias 문자열 추측이 틀렸다는 뜻입니다.
>
> 앱 로그도 `Return=-421`만 찍히면 불편하니 `NC_PI_VAR_NOT_FOUND` 이름까지 같이 찍히게 조금 더 보완하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs` (+24 -17)
>
> </details>
>
> 보완했습니다. 빌드만 다시 확인하고 정리하겠습니다.
</details>

저 결과는 이렇게 보면 된다.

```text
Return = -421
```

`-421 = NC_PI_VAR_NOT_FOUND`

즉 `I.6041.0`이라는 alias를 컨트롤러 PI map에서 못 찾은 거다.
뒤에 찍힌 값들은 실패 후 구조체에 남은 쓰레기값이라 해석하면 안 된다.

```text
PIVarIndex=0
BitSize=808857161
BitOffset=808334390
CanOpen=0x0000:0
```

이건 유효한 PI 정보가 아니다. 정상이라면 Statusword는 보통 이렇게 나와야 한다.

```text
Return=0
PIVarIndex=0
Direction=ePI_INPUT
BitSize=16
CanOpen=0x6041:0
VarType=ePI_UNSIGNED_SHORT
```

중요한 점은 `I.6041.0`이 “항상 되는 표준 alias”가 아니라는 거다. 문서에는 그런 형식을 말하지만, 실제 컨트롤러 cfg/xml PI map에 그 alias가 등록돼 있어야 `GetPIVarInfoByAlias`가 성공한다. 네 장비에는 그 alias가 등록 안 된 상태다.

지금 해야 할 건 Alias 조회가 아니라:

1. `Index = 0`
2. `Direction = ePI_INPUT`
3. `MMC_GetPIVarInfo` 클릭

이걸로 PI index 0의 실제 정보를 봐야 한다. 거기서 `Alias=...`로 찍히는 값이 있으면 그 문자열이 `GetPIVarInfoByAlias`에 넣을 진짜 alias다. Alias가 빈 문자열이면 이 장비에서는 alias lookup을 쓸 수 없고, index 기반 `GetPIVarInfo` / `ReadPIVar`로 써야 한다.

앱도 추가로 고쳤다. 이제 alias 실패 로그에 `NC_PI_VAR_NOT_FOUND(-421)`처럼 에러명이 같이 찍힌다. 빌드 통과했다.

> 수정해도 똑같은데?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-ebf6a6eb-c336-4266-b138-e87afdfc8219.png`

응, 똑같이 나오는 게 맞다. 수정은 **실패를 성공처럼 보이던 문제를 고친 것**이지, `I.6041.0` alias를 새로 만들어주는 게 아니다.

현재 결과:

```text
Error = NC_PI_VAR_NOT_FOUND(-421)
Alias = I.6041.0
```

의미는 딱 하나다.

`I.6041.0`이라는 alias가 컨트롤러 PI map에 없다.

그래서 `GetPIVarInfoByAlias`는 실패가 정상이다. 이 API는 “CANopen index 6041을 찾아줘”가 아니라 “PI map에 등록된 alias 문자열을 찾아줘”다. 네 장비 PI map에 그 alias가 없으면 절대 성공 안 한다.

지금 확인해야 할 버튼은 이거다:

```text
MMC_GetPIVarInfo
```

입력:

```text
Index = 0
Direction = ePI_INPUT
```

이 버튼은 alias가 필요 없다. PI index 0이 실제로 뭔지 조회한다. 거기 로그를 봐야 한다.

정상 로그 예시는 이런 형태다.

```text
PI Info: Index=0, Direction=ePI_INPUT, BitSize=16, BitOffset=..., CanOpen=0x6041:0, VarType=..., Alias=...
```

여기서:

- `CanOpen=0x6041:0`이면 index 0은 Statusword
- `Alias=...`가 비어 있으면 이 장비는 alias가 등록 안 된 것
- alias가 찍히면 그 문자열만 `MMC_GetPIVarInfoByAlias`에 넣어야 함

정리하면:

- `MMC_ReadPIVarUShort(Index=0, Direction=ePI_INPUT)`은 됨
- `MMC_GetPIVarInfoByAlias("I.6041.0")`는 안 됨
- 이유: `I.6041.0`은 실제 alias가 아님
- 다음 액션: `MMC_GetPIVarInfo` 버튼을 눌러 index 0의 실제 `CanOpen`, `VarType`, `Alias`를 확인해야 함

> API 문서에는 MMC_LIB_API int MMC_GetPIVarInfoByAlias(
> IN MMC_CONNECT_HNDL hConn,
> IN MMC_AXIS_REF_HNDL hAxisRef,
> IN MMC_GETPIVARINFOBYALIAS_IN* pInParam,
> OUT MMC_GETPIVARINFOBYALIAS_OUT* pOutParam
> );
>
> 이렇게 인데
>
> DLL에는 public short GetPIVarInfoByAlias(string aliasing, out NC_PI_INFO_BY_ALIAS info);게 되어있냐

문서의 것은 **C API 원형**이고, DLL에서 네가 보는 것은 **C#/.NET wrapper 메서드**라서 형태가 다르다.

C API:

```c
MMC_GetPIVarInfoByAlias(
    hConn,
    hAxisRef,
    pInParam,
    pOutParam
)
```

.NET wrapper:

```csharp
public short GetPIVarInfoByAlias(string aliasing, out NC_PI_INFO_BY_ALIAS info);
```

매핑은 이렇게 보면 된다.

```text
hConn      -> MMCSingleAxis 객체 내부의 connection handle
hAxisRef   -> MMCSingleAxis 객체 내부의 AxisReference
pInParam   -> string aliasing
pOutParam  -> out NC_PI_INFO_BY_ALIAS info
return     -> short error/status code
```

즉 `MMCSingleAxis`를 만들 때 이미 축 이름과 connection handle을 넣었기 때문에 wrapper 메서드에서는 `hConn`, `hAxisRef`를 다시 안 받는다.

예를 들면 앱 코드에서:

```csharp
SingleAxis = new MMCSingleAxis(axisName, Handle);
```

이 순간 `Handle`과 `AxisRef`가 객체 안에 들어간다. 그래서 이후에는:

```csharp
Context.SingleAxis.GetPIVarInfoByAlias(alias, out info);
```

만 호출하면 wrapper가 내부적으로 C API에 필요한 `hConn`, `hAxisRef`, input/output struct를 만들어서 넘긴다.

그리고 지금 네 로그의:

```text
Return = -421
NC_PI_VAR_NOT_FOUND
```

는 wrapper가 “C API 호출 자체는 했지만 alias를 못 찾았다”는 에러를 `short`로 돌려준 것이다. `info` 구조체는 실패 시 유효하지 않다.

정리하면 문서와 DLL이 다른 게 아니라:

- 문서: low-level C API
- DLL: C API를 감싼 .NET convenience wrapper
- `string aliasing`: `MMC_GETPIVARINFOBYALIAS_IN.pAliasing`
- `out NC_PI_INFO_BY_ALIAS info`: `MMC_GETPIVARINFOBYALIAS_OUT.VarInfo`
- `hConn/hAxisRef`: `MMCSingleAxis` 객체 내부에서 처리됨

> api에는 pInParam
> Points to the MMC_GETPIVARINFOBYALIAS input data structure using the
> MMC_ReadPIVarInfoByAlias function.
>
> 이렇게 struct 타입인데. 우리는 string으로 날리자나

맞다. C API 기준으로는 `pInParam`이 struct다.
그런데 그 struct 안에 실질 입력 필드가 **alias 문자열 하나뿐**이라 .NET wrapper가 그걸 `string`으로 단순화한 거다.

문서 구조는 이거다.

```c
typedef struct mmc_getpivarinfobyalias_in
{
    char pAliasing[PI_ALIASING_LENGTH];
} MMC_GETPIVARINFOBYALIAS_IN;
```

그래서 C# wrapper:

```csharp
public short GetPIVarInfoByAlias(
    string aliasing,
    out NC_PI_INFO_BY_ALIAS info
);
```

는 내부적으로 대략 이렇게 처리한다고 보면 된다.

```csharp
MMC_GETPIVARINFOBYALIAS_IN inParam;
inParam.pAliasing = aliasing;

MMC_GETPIVARINFOBYALIAS_OUT outParam;

MMC_GetPIVarInfoByAlias(hConn, hAxisRef, &inParam, &outParam);

info = outParam.VarInfo;
return outParam.sErrorID; // 또는 wrapper가 정리한 error code
```

즉 매핑은 이렇게다.

```text
string aliasing
  -> MMC_GETPIVARINFOBYALIAS_IN.pAliasing

out NC_PI_INFO_BY_ALIAS info
  -> MMC_GETPIVARINFOBYALIAS_OUT.VarInfo
```

`hConn`, `hAxisRef`는 `MMCSingleAxis` 객체 안에 이미 들어 있어서 wrapper 메서드 인자로 안 받는다.

중요한 건 `MMC_GETPIVARINFOBYALIAS_IN`에는 direction이나 index가 없다. 오직 alias 문자열만 있다. 그래서 `I.6041.0`을 넣었을 때 `NC_PI_VAR_NOT_FOUND(-421)`가 나온 건 호출 방식 문제가 아니라, 컨트롤러 PI map에 그 alias 문자열이 없다는 뜻이다.

`I.6041.0`은 “CANopen 0x6041을 찾아라”가 아니다.
그냥 alias table에서 문자열 `"I.6041.0"`을 exact match로 찾는 거다. 없으면 실패한다.

<details><summary>8 previous messages</summary>

> Context compaction
>
> Source: automatic
> Status: completed
>
> 맞습니다. 이건 API 래퍼 문제가 아니라 컨트롤러 PI alias 문자열 포맷을 내가 잘못 짚은 겁니다. 지금 기본값/로그/가드 쪽에 `I0x6041.0` 기준으로 바로 반영할 부분이 있는지 확인하겠습니다.
>
> 현재 XAML 기본 Alias는 빈 값이라 잘못된 기본값을 박아둔 상태는 아닙니다. 다만 Alias 입력에서 숫자 정규화 함수를 태우고 있어서, `I0x6041.0` 같은 문자열을 건드리지 않는지 코드를 확인 중입니다.
>
> 확인했습니다. `NormalizeNumeric`은 콤마/유니코드 마이너스 정도만 정리해서 `I0x6041.0` 자체를 깨지는 않습니다. 그래도 PI Alias 입력에 숫자 정규화 함수를 쓰는 건 의미가 없어서 alias 전용 정리 함수로 분리하고, 기본 예시를 `I0x6041.0`로 넣겠습니다.
>
> 수정 범위는 작게 잡겠습니다. Alias는 “숫자”가 아니라 컨트롤러에 등록된 exact string이라서 PI Alias 전용 trim 함수로 바꾸고, 화면 기본 입력 예시는 `I0x6041.0`로 맞춥니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs` (+19 -14)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
>
> 코드 쪽 반영은 끝났습니다. 이제 기존에 쓰던 MSBuild로 깨지는 부분 없는지만 확인하겠습니다.
>
> 빌드는 통과했습니다. 결론은 `GetPIVarInfoByAlias(string)` 호출 형태는 문제가 아니고, 넘기는 alias 문자열이 컨트롤러 PI map에 등록된 문자열과 정확히 일치해야 하는 문제였습니다.
</details>

맞습니다. 내가 alias 포맷을 잘못 봤습니다.

`I0x6041.0` 의미는 보통 이렇게 보는 게 맞습니다.

- `I` = Input PI
- `0x6041` = CANopen object index, CiA402 Statusword
- `.0` = subindex 0

그래서 `I.6041.0`는 컨트롤러 PI map에 없는 문자열이라 `NC_PI_VAR_NOT_FOUND(-421)`가 난 겁니다. `GetPIVarInfoByAlias(string)` 래퍼 자체는 문제 없습니다. 그 `string`이 C API의 `pInParam->pAliasing`에 해당합니다.

반영한 내용:
- PI Alias 기본값을 `I0x6041.0`로 변경: [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:974)
- Alias 입력 처리를 숫자 파서가 아니라 alias 전용 trim 처리로 분리: [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs:17)
- 실패 메시지도 `I0x6041.0` 예시로 수정: [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs:28)

빌드도 통과했습니다. 경고 0개, 오류 0개.

> I0x6064.0는 PI Index 1에 있고 그 값을 읽어보면 2409가 올라오는데 실제로 Position Actual Value 값은 4720998 이정도의 값이 올라오는데 이 두개 값의 차이가 왜 벌어지는지 알 수 있을까.
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-65e038dc-04c4-464c-8f5c-880451c3645d.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-52e9279f-7415-45b3-a11f-d0620bd0456c.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-ffdccd7b-feac-4287-af62-822be716b7dd.png`

<details><summary>15 previous messages</summary>

> 원인은 거의 확정입니다. `I0x6064.0`의 PI 정보가 `BitSize=32`, `VarType=ePI_SIGNED_INT(5)`인데, 현재 앱에서는 `MMC_ReadPIVarUShort`로 16비트만 읽고 있어서 실제 32비트 위치값의 하위 word만 보고 있습니다. 코드에 `INT` 읽기를 추가해서 바로 검증 가능하게 만들겠습니다.
>
> 사진의 숫자도 그 설명과 맞습니다. 4,721,001을 16비트로 자르면 하위 16비트가 2409가 되기 때문에, 앱에서 보이는 `2409`는 위치값이 아니라 32비트 위치값의 일부입니다.
>
> 이제 래퍼의 union 필드 이름을 확인해서 `VAR_TYPE.INT`로 읽는 버튼을 붙이겠습니다. 이름이 `_int32`인지 다른 필드명인지 확인해야 컴파일이 안 깨집니다.
>
> 현재 래퍼는 실제 내부 필드(`_iint32`)와 편의 프로퍼티(`_uint16`)가 같이 있는 형태일 가능성이 큽니다. 프로퍼티까지 확인한 뒤 기존 스타일에 맞춰 추가하겠습니다.
>
> 같은 실수를 다시 줄이기 위해 `USHORT` 읽기에도 타입 불일치 경고를 넣겠습니다. `GetPIVarInfo`가 `SIGNED_INT/32bit`라고 말하는데 `USHORT`로 읽으면 로그에 바로 경고가 뜨게 하는 쪽이 낫습니다.
>
> 구현은 `MMC_ReadPIVarInt` 버튼만 추가하는 걸로 충분합니다. `I0x6064.0`는 입력 PDO라 write는 만들지 않는 게 안전합니다. USHORT 버튼은 상태워드 같은 16비트 항목용으로 남기고, 32비트 위치는 INT 버튼으로 읽게 하겠습니다.
>
> 리플렉션 출력이 비어 있어서 DLL 로딩 경로/타입 확인을 한 번 더 보겠습니다. 기존 코드가 컴파일되는 걸 보면 메서드는 있는데, 타입이 상속 계층 쪽에 있을 수 있습니다.
>
> 확인 결과 타입은 맞습니다. 메서드 이름 필터가 안 잡힌 쪽이라 전체 public 메서드명에서 다시 뽑겠습니다.
>
> `ReadPIVar`는 상속받은 `MMCNode` 쪽 메서드입니다. 이제 정확한 반환형/파라미터를 그쪽에서 확인합니다.
>
> 리플렉션 필터가 예상과 다르게 안 잡히지만, 기존 컴파일이 이미 `ReadPIVar(..., VAR_TYPE.USHORT, ref value)`를 통과하고 있으니 같은 호출 형태로 `VAR_TYPE.INT`만 추가하면 됩니다.
>
> `ReadPIVar`는 확장 메서드 형태일 수도 있습니다. 그래도 호출 구문은 기존 코드와 동일하게 유지하면 컴파일러가 찾아줍니다. 지금부터 실제 파일 패치합니다.
>
> 패치 내용은 세 가지입니다: `MMC_ReadPIVarInt` 버튼 추가, `USHORT`로 32비트 PI를 읽을 때 경고 로그 추가, 값 입력 라벨을 `USHORT Value`에서 일반 `PI Value`로 바꿉니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.Coverage.cs` (+1 -0)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs` (+67 -21)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+14 -9)
>
> </details>
>
> 수정은 들어갔고, 지금은 컴파일 확인 단계입니다. 만약 `VAR_TYPE.INT` 이름이 래퍼에서 다르게 정의돼 있으면 여기서 바로 잡겠습니다.
>
> 빌드 통과했습니다. 이제 앱에서는 `I0x6064.0 / Index=1 / ePI_INPUT` 상태에서 `MMC_ReadPIVarInt`를 누르면 Maestro의 Actual Position과 같은 계열의 32비트 값이 나와야 합니다.
</details>

원인 확정입니다. `I0x6064.0`는 로그상 `BitSize=32`, `VarType=ePI_SIGNED_INT(5)`입니다. 그런데 지금 누른 버튼은 `MMC_ReadPIVarUShort`라서 16비트만 읽습니다. 그래서 실제 위치값 전체가 아니라 32비트 Position Actual Value의 하위 16비트만 나온 겁니다.

예를 들면 실제값이 대략 `4,721,001`이면:

```text
4,721,001 = 0x00480969
하위 16비트 = 0x0969 = 2409
```

즉 앱에 나온 `2409`는 틀린 값이 아니라 `4720998~4721001` 근처 위치값의 low word입니다. 스크린샷의 `4720998`과 `2409`가 3 count 정도 안 맞는 건 EAS 화면과 테스트앱 PI read 시점이 다르기 때문입니다.

수정해둔 내용:
- `MMC_ReadPIVarInt` 버튼 추가: [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1019)
- `VAR_TYPE.INT`로 32비트 signed int 읽기 추가: [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs:68)
- `USHORT`로 32비트 PI를 읽으면 경고 로그 출력: [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs:202)

빌드도 통과했습니다. 이제 `I0x6064.0 / Index=1 / ePI_INPUT`에서는 `MMC_ReadPIVarInt`를 눌러야 Maestro의 Actual Position 값과 맞습니다.

> 수정된 코드 cr lf 안뜨게좀 만들어줘

<details><summary>4 previous messages</summary>

> 수정 파일들이 `apply_patch` 때문에 LF로 저장돼서 Git이 “나중에 CRLF로 바뀐다” 경고를 내고 있습니다. `.gitattributes` 기준을 확인한 뒤 수정된 코드 파일만 CRLF로 다시 정규화하겠습니다.
>
> `.gitattributes`는 `.cs`/`.xaml`을 CRLF로 강제하도록 맞춰져 있습니다. 지금 경고가 나는 파일은 방금 만진 `MainWindow.Coverage.cs`, `MainWindow.PiBulkOperations.cs`, `MainWindow.xaml` 세 개라서 이 셋만 CRLF로 변환합니다.
>
> CRLF 변환은 끝났습니다. 이제 Git 경고와 빌드만 다시 확인합니다.
>
> `git diff --check`에서 CRLF 경고가 사라졌습니다. 줄바꿈만 만졌지만, 최종으로 빌드도 한 번 돌려서 파일 손상 여부를 확인하겠습니다.
</details>

처리했습니다. 방금 수정된 세 파일을 `.gitattributes` 기준에 맞춰 CRLF로 정규화했습니다.

- [MainWindow.Coverage.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.Coverage.cs)
- [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs)
- [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml)

`git diff --check`에서 CRLF 경고 없어졌고, 빌드도 경고 0개 / 오류 0개로 통과했습니다.

> 이거 이렇게 할께 아니라 콤보박스로 선택해서 Read, Write 할 수 있는 기능을 만드는게 맞는거 같은데..
> 기본값은 ushort로 넣고
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-22fc19f2-5566-4595-87b5-296d48ec5a06.png`

<details><summary>10 previous messages</summary>

> 맞습니다. 타입별 버튼을 늘리는 구조는 금방 지저분해집니다. PI 값 타입 콤보박스를 추가하고, `Read`/`Write`가 선택된 타입을 따라가게 정리하겠습니다. 기본 선택은 `USHORT`로 넣습니다.
>
> 구조는 이렇게 바꾸겠습니다. `PI Type` 콤보박스에서 `USHORT/SHORT/INT/UINT/BYTE/S_BYTE/FLOAT`를 고르고, 버튼은 `MMC_ReadPIVar` / `MMC_WritePIVar` 두 개만 남깁니다. 기존 `GetPIVarInfo`는 선택한 타입과 실제 PI 타입이 맞는지 확인하는 용도로 유지합니다.
>
> `VAR_TYPE` enum은 `BYTE_ARR, S_BYTE, BYTE, SHORT, USHORT, INT, UINT, FLOAT, NOT_SET`입니다. 이번 UI에는 일반 Read/Write로 의미 있는 스칼라 타입만 넣고, `BYTE_ARR`/`NOT_SET`는 빼겠습니다.
>
> 기존 `SelectComboItemByName` 헬퍼가 있어서 기본 선택은 `USHORT`로 안정적으로 지정할 수 있습니다. 이제 PI 작업 코드를 타입 선택 기반으로 갈아끼우겠습니다.
>
> 쓰기 쪽은 `WritePIVar`가 방향 파라미터를 받지 않습니다. 그래서 UI의 `Direction`은 Read와 Info 조회용이고, Write는 선택 타입과 index/value만 넘기게 유지하겠습니다. 이 동작은 로그로 드러나게 하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.Coverage.cs` (+2 -3)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs` (+209 -66)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+44 -41)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+14 -6)
>
> </details>
>
> UI와 처리 코드는 바꿨습니다. 이제 컴파일하면서 `PI_VAR_UNION` 프로퍼티명이나 enum 매핑에 오류가 없는지 확인하겠습니다.
>
> 빌드는 통과했습니다. `apply_patch`로 수정한 파일들은 다시 LF가 섞였을 수 있어서, 요청대로 CRLF 경고가 안 뜨게 해당 코드 파일들을 정규화하고 확인하겠습니다.
>
> CRLF 정규화까지 끝났습니다. 경고가 사라졌는지와 최종 빌드 상태를 다시 확인합니다.
>
> `git diff --check`도 깨끗합니다. 마지막으로 CRLF 정규화 후 빌드 한 번 더 돌립니다.
</details>

맞는 방향이라 그렇게 바꿨습니다.

PI 탭 변경 내용:
- `PI Type` 콤보박스 추가: [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:989)
- 기본값은 `USHORT`: [MainWindow.xaml.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:57)
- 타입 선택지는 `S_BYTE`, `BYTE`, `SHORT`, `USHORT`, `INT`, `UINT`, `FLOAT`
- 버튼은 `MMC_ReadPIVar`, `MMC_WritePIVar`로 통합
- 선택한 타입 기준으로 `PI_VAR_UNION` 값을 읽고/씁니다: [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs:47)

`I0x6064.0`처럼 32비트 signed position이면 `PI Type=INT`로 읽으면 되고, `I0x6041.0` statusword는 기본값 그대로 `USHORT`로 읽으면 됩니다.

CRLF 정규화도 다시 했고, `git diff --check` 깨끗합니다. 빌드도 경고 0개 / 오류 0개로 통과했습니다.

> PDO Map이랑 PI랑 무슨 차이가 있는거지? PI는 그냥 I/O 둘다 합쳐서인건가?

차이는 이렇게 보면 됩니다.

**PDO Map**
드라이브/슬레이브와 마스터가 주기 통신으로 주고받을 **CANopen object 목록과 배치표**입니다.

예:
- `0x6041:0 Statusword`를 input PDO에 넣는다
- `0x6064:0 Position actual value`를 input PDO에 넣는다
- `0x6040:0 Controlword`를 output PDO에 넣는다
- `0x607A:0 Target position`을 output PDO에 넣는다

즉 PDO Map은 “EtherCAT cyclic packet 안에 어떤 object를 몇 bit 크기로 어떤 순서에 실을지”를 정하는 설정입니다.

**PI, Processing Image**
PDO Map대로 주기 통신해서 들어온/나갈 데이터를 마스터 내부 메모리에 펼쳐 놓은 **런타임 I/O 버퍼**입니다.

PI는 개념상 Input PI와 Output PI를 포함합니다. 그래서 “PI는 I/O 둘 다 합쳐서 부르는 말인가?”라고 하면 큰 틀에서는 맞습니다. 다만 실제 접근은 분리됩니다.

- `ePI_INPUT`: 드라이브 → 마스터
  예: `0x6041 Statusword`, `0x6064 Actual Position`
- `ePI_OUTPUT`: 마스터 → 드라이브
  예: `0x6040 Controlword`, `0x607A Target Position`

그래서 API가 `Index + Direction`을 받습니다.

**정리**
```text
PDO Map = 어떤 CANopen object를 cyclic PDO에 넣을지 정한 설정/배치표
PI      = 그 PDO Map 결과가 마스터 메모리에 올라온 실제 읽기/쓰기 버퍼
```

지금 네 예로 보면:

```text
Alias: I0x6064.0
Direction: ePI_INPUT
PI Index: 1
CanOpen: 0x6064:0
Type: SIGNED_INT, 32bit
```

이 말은 “Input PDO에 매핑된 `0x6064:0 Position actual value`가 PI input 영역의 index 1번에 있다”는 뜻입니다.

중요한 점:
- PDO에 매핑되지 않은 object는 PI로 못 읽습니다. 그건 SDO나 다른 API로 읽어야 합니다.
- PI Index는 CANopen index가 아닙니다. `PI Index 1`과 `0x6064`는 다른 체계입니다.
- Alias는 PI entry를 찾기 위한 문자열 이름입니다. `I0x6064.0` → input PI의 CANopen `0x6064:0`.
- PI read/write 타입은 PDO Map의 타입/크기와 맞춰야 합니다. `0x6064`는 32bit signed int라 `INT`로 읽어야 합니다.

> 두개를 비교할만한 사진 있으면 줘봐

<details><summary>2 previous messages</summary>

> 비교용으로는 실제 “사진”보다 구조도가 더 정확합니다. 텍스트가 틀어지면 안 되는 내용이라, 랜덤 이미지 생성 대신 정확한 SVG 다이어그램으로 만들어서 바로 보여드리겠습니다.
>
> Generated image
>
> ![Generated image](/C:/Users/dreiv/.codex/generated_images/019ea9e3-a118-71f3-9b8d-e6ea5cf449a6/ig_04256a563ee3ae6b016a2f9126805481939e80c618811f76e0.png)
</details>

위에 비교도 올렸습니다. 핵심만 보면 됩니다.

- 왼쪽 `PDO Map`: 설정값입니다. 어떤 CANopen object를 cyclic PDO에 넣을지, 순서와 bit size를 정합니다.
- 오른쪽 `PI`: 그 PDO Map 결과가 런타임에 마스터 메모리에 펼쳐진 버퍼입니다.
- `Input PI`: Drive → Master, 예: `I0x6041.0`, `I0x6064.0`
- `Output PI`: Master → Drive, 예: `O0x6040.0`, `O0x607A.0`
- `PI Index`: PI 버퍼 안의 순번입니다. `0x6064` 같은 CANopen index가 아닙니다.

단, 그림의 index/order는 예시입니다. 네 장비의 실제 값은 `MMC_GetPIVarInfoByAlias`나 `MMC_GetPIVarInfo` 결과가 기준입니다.

> 이미지 니가 그린거냐? 잘그렸네. 이거 다운받을수 있어?

<details><summary>2 previous messages</summary>

> 네, 방금 생성한 이미지입니다. 저장된 실제 파일명을 확인해서 바로 열 수 있는 경로로 알려드리겠습니다.
>
> 파일은 생성 폴더에 저장돼 있습니다. PowerShell 출력이 잘려서 전체 경로만 다시 뽑겠습니다.
</details>

네, 제가 방금 생성한 이미지입니다. 이미 로컬에 저장돼 있습니다.

![PDO Map vs PI](C:/Users/dreiv/.codex/generated_images/019ea9e3-a118-71f3-9b8d-e6ea5cf449a6/ig_04256a563ee3ae6b016a2f9126805481939e80c618811f76e0.png)

파일 경로:
`C:\Users\dreiv\.codex\generated_images\019ea9e3-a118-71f3-9b8d-e6ea5cf449a6\ig_04256a563ee3ae6b016a2f9126805481939e80c618811f76e0.png`

그 경로에서 바로 열거나 복사해서 쓰면 됩니다.

> bulk read좀 알려주고 parameter bulk read랑 pi bulk read랑 머가 다른지 설명해주고 둘다 구현 가능하면 구현해봐

<details><summary>26 previous messages</summary>

> 먼저 현재 앱의 `Bulk Read`가 실제로 어떤 API 래퍼를 쓰는지와, 문서상 PI 쪽에 “bulk/range/raw”로 부를 만한 API가 있는지 확인하겠습니다. 구현 가능 여부는 .NET 래퍼에 노출된 메서드 기준으로 판단해야 합니다.
>
> 문서상으로는 두 종류가 따로 있습니다. 현재 앱의 `MMCBulkRead`는 “parameter bulk read” 쪽이고, PI bulk read는 `MMC_ConfigureBulkReadPI` / `MMC_PerformBulkReadCmdPI` 쪽입니다. 다만 .NET DLL에 PI bulk wrapper가 실제 노출돼 있는지 확인해야 구현 방식을 정할 수 있습니다.
>
> 문서상 차이는 확인됐습니다. 이제 구현 쪽은 두 갈래입니다: `MMCBulkRead` 래퍼는 이미 있어서 parameter bulk는 현재 UI를 이름만 명확히 바꾸면 되고, PI bulk는 래퍼가 없으면 typed `ReadPIVar`를 여러 개 루프하는 “PI multi read”로 구현해야 합니다. DLL에 네이티브 PI bulk 래퍼가 있는지 먼저 확인합니다.
>
> DLL 문자열에는 `MMC_LIB_PIBULKREAD`, `GetPIVarsRangeInfo`, `ReadPIVar` 흔적은 있습니다. 그런데 public wrapper가 안 보일 수 있어서, 타입 로딩 예외까지 잡으면서 메서드/타입명을 다시 뽑겠습니다.
>
> 방금 리플렉션 스크립트 출력이 비정상적으로 비어 있어서 짧게 나눠 확인합니다. 구현은 public wrapper가 없으면 네이티브 bulk 대신 기존 `ReadPIVar`를 여러 항목 반복 호출하는 방향으로 갈 수 있습니다.
>
> 좋습니다. .NET 래퍼에 `MMCPIBulkRead`가 실제로 있습니다. 이걸 쓰면 문서의 PI bulk read를 네이티브 방식으로 구현 가능합니다. 생성자/메서드/프로퍼티 형태를 뽑아서 기존 UI에 맞게 붙이겠습니다.
>
> `MMCPIBulkRead` 타입은 있는데 생성자/메서드가 바로 안 보입니다. 상속/인터페이스 구현이 다른 네임스페이스에 걸려 있는지 멤버 전체와 인터페이스를 확인합니다.
>
> public 멤버명은 확인됐습니다: `AddEntry`, `AddEntries`, `Upload` 등이 있습니다. 이제 시그니처만 뽑으면 바로 구현 가능합니다.
>
> 리플렉션에서 `GetMethods` 출력이 이상하게 비고 있어서 `GetMembers` 기반으로 직접 파라미터를 확인합니다. PowerShell 쪽 필터 문제로 보입니다.
>
> 멤버명 출력이 한 번은 보이고 조건 필터에서는 빠져서, 전체 멤버를 따옴표 포함해서 다시 봅니다.
>
> 이제 명확히 보입니다. 다음으로 각 메서드 시그니처를 직접 출력합니다.
>
> PowerShell 리플렉션이 메서드 컬렉션 순회에서 이상하게 멈추고 있습니다. `Select-Object` 방식으로 강제로 문자열화해서 확인합니다.
>
> 시그니처 확인됐습니다. `MMCPIBulkRead(handle, config)`를 만들고 `PI_BULKREAD_ENTRY`들을 `AddEntry`로 넣은 뒤 `Upload`, `GetEntry`로 값을 꺼내는 구조입니다. 이제 entry 구조 필드를 확인합니다.
>
> 출력이 또 길어서 잘렸습니다. 필요한 `PI_BULKREAD_ENTRY`와 `MMCPIBulkReadParameter`만 짧게 뽑습니다.
>
> `GetEntry`는 `object out/ref`로 값을 돌려주는 구조라서 반환 object의 실제 타입을 처리해야 합니다. wrapper에 `MMCPIBulkReadParameter`가 있어 `Info + Value` 형태로 나올 가능성이 높습니다. 구현은 이 타입과 순수 value 둘 다 받아들이게 하겠습니다.
>
> 구현 범위는 이렇게 잡겠습니다. 기존 `Bulk Read`는 이름을 `Parameter Bulk Read`로 명확히 바꾸고, 새로 `PI Bulk Read` 영역을 추가합니다. PI bulk는 축 이름 CSV와 PI index CSV를 받아서 축×인덱스 조합을 한 번에 구성하고, `Upload/GetEntry`로 읽어오게 합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.Coverage.cs` (+5 -3)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+73 -16)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+8 -5)
>
> </details>
>
> XAML 쪽 UI는 들어갔습니다. 이제 `MMCPIBulkRead` 래퍼를 쓰는 동작 코드를 추가합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs` (+177 -25)
>
> </details>
>
> 구현 코드는 들어갔습니다. 이제 컴파일해서 namespace 충돌이나 래퍼 호출 형태를 바로 잡겠습니다.
>
> 컴파일 오류는 두 가지입니다. `GetEntry`는 `ref`가 아니라 `out`이고, `MMCPIBulkReadParameter` 타입은 DLL 내부에서는 보이지만 C# 컴파일러에는 접근 불가라 직접 캐스팅하면 안 됩니다. reflection으로 `Info/Value`를 꺼내도록 바꾸겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs` (+44 -8)
>
> </details>
>
> 접근 불가 타입 처리는 우회했습니다. 다시 빌드해서 wrapper 호출이 맞는지 확인합니다.
>
> 빌드는 통과했습니다. 이제 방금 수정한 `.cs/.xaml` 파일들을 CRLF로 다시 정규화하고 `git diff --check`로 줄바꿈 경고를 확인하겠습니다.
>
> CRLF 정규화 완료했습니다. 최종 확인으로 diff check와 빌드를 한 번 더 돌립니다.
>
> `git diff --check`는 깨끗합니다. 빌드만 마지막으로 확인합니다.
</details>

정리하면 이렇습니다.

**Parameter Bulk Read**
Maestro 내부 “파라미터/상태값”을 여러 축에서 한 번에 읽는 기능입니다. 현재 앱의 기존 Bulk Read가 이쪽입니다.

예:
- actual position
- actual velocity
- torque
- axis status
- op mode
- preset/custom parameter list

이건 PDO/PI index를 직접 읽는 게 아니라 `MMC_ConfigBulkReadCmd` / `MMC_PerformBulkReadCmd` 계열입니다.

**PI Bulk Read**
PDO Map으로 올라온 Processing Image 항목을 여러 개 한 번에 읽는 기능입니다.

예:
- `I0x6041.0` statusword
- `I0x6064.0` actual position
- `O0x6040.0` controlword
- `O0x607A.0` target position

이건 `axisRef + PI index + direction` 기준입니다. 즉 `ePI_INPUT`/`ePI_OUTPUT`을 명확히 탑니다.

구현한 내용:
- 기존 `Bulk Read` 영역을 `Parameter Bulk Read`로 이름 변경: [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1031)
- Axis Refs 기본값을 `0,1,2,3`으로 변경
- 새 `PI Bulk Read` 영역 추가: [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1095)
- `MMCPIBulkRead` wrapper로 `MMC_ConfigureBulkReadPI` / `MMC_PerformBulkReadCmdPI` 구현: [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs:158)
- Coverage에도 PI bulk 항목 추가: [MainWindow.Coverage.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.Coverage.cs:35)

사용 방식:
- `Axis Names (CSV)`: `a01,a02,a03,a04`
- `PI Indexes (CSV)`: `0,1`
- `Direction`: `ePI_INPUT`
- `PI Bulk Config`: `eBULKREAD_CONFIG_PI_1`
- `MMC_ConfigureBulkReadPI` 누르고
- `MMC_PerformBulkReadCmdPI` 누르면 됩니다.

그러면 각 축에 대해 index `0`, `1`을 한 번에 읽습니다. 네 현재 매핑 기준이면 보통 `0 = I0x6041.0`, `1 = I0x6064.0` 쪽으로 나올 겁니다. 단, 축마다 PI index 구성이 다르면 configure 단계에서 실패할 수 있습니다.

CRLF 정리했고 `git diff --check` 깨끗합니다. 빌드도 경고 0개 / 오류 0개로 통과했습니다.

> config는 config 세팅하는거고 perform은 세팅값의 결과값을 가져오는거네?

맞습니다.

`Config`는 “무엇을 읽을지” 세팅하는 단계입니다.

- 어떤 축들인지
- 어떤 parameter preset/custom list인지
- 또는 어떤 PI index/direction인지
- 어떤 bulk config slot을 쓸지

`Perform`은 그 세팅값 기준으로 실제 현재 값을 읽어오는 단계입니다.

```text
Config  = 읽을 목록/대상/버퍼 구성
Perform = 그 구성으로 현재값 snapshot 읽기
```

그래서 매번 `Config`를 누를 필요는 없습니다. 읽을 대상이 안 바뀌면:

```text
Config 1회
Perform 반복
Perform 반복
Perform 반복
```

대상이 바뀌면 다시 `Config` 해야 합니다.

예를 들어 PI Bulk에서:

```text
Axis Names = a01,a02,a03,a04
PI Indexes = 0,1
Direction = ePI_INPUT
```

으로 `Config`하면, “각 축의 input PI index 0,1을 읽겠다”가 등록됩니다. 이후 `Perform`을 누르면 그 순간의 `I0x6041.0`, `I0x6064.0` 같은 값들이 올라옵니다.

Parameter Bulk도 동일합니다. `Config`에서 preset/custom parameter 목록과 축들을 정하고, `Perform`에서 그 값들을 가져옵니다.

> config는 1,2,3,4,5,6,7,8,MAX에 저장 가능한겨? 마지막은 왜 max임?

<details><summary>5 previous messages</summary>

> 맞습니다. `MAX`는 저장 슬롯이 아니라 enum 경계값이라 UI에서 선택되면 안 됩니다. 지금 콤보박스가 enum 전체를 그대로 보여줘서 혼동을 만든 거라, `None/MAX`를 숨기고 실제 config slot만 보이게 고치겠습니다.
>
> `MAX`는 실제 저장 슬롯이 아니고, `None`도 실제 읽기 슬롯이 아닙니다. Parameter Bulk와 PI Bulk 둘 다 콤보에서 `None/MAX`를 빼고, Parameter Bulk 기본은 문서 기준으로 `eBULKREAD_CONFIG_2`, PI Bulk 기본은 `eBULKREAD_CONFIG_PI_1`로 잡겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+39 -15)
>
> </details>
>
> 수정했습니다. 이제 빌드 확인하고 CRLF도 다시 맞추겠습니다.
>
> 빌드는 통과했습니다. 방금 편집한 파일 줄바꿈을 CRLF로 정리하고 경고 여부를 확인하겠습니다.
</details>

맞습니다. `eBULKREAD_CONFIG_PI_1` ~ `eBULKREAD_CONFIG_PI_8`은 bulk read 설정을 저장하는 슬롯처럼 보면 됩니다.

```text
Config PI_1 = 이 축/이 PI index들 읽어라
Config PI_2 = 다른 축/다른 PI index들 읽어라
...
Perform PI_1 = PI_1에 저장된 구성으로 현재값 읽기
Perform PI_2 = PI_2에 저장된 구성으로 현재값 읽기
```

`MAX`는 저장 슬롯이 아닙니다. enum에서 개수/범위 체크용으로 넣는 끝값입니다.

```c
eBULKREAD_CONFIG_PI_1 = 0
...
eBULKREAD_CONFIG_PI_8 = 7
eBULKREAD_CONFIG_PI_MAX = 8  // 실제 슬롯 아님
```

`None = -1`도 실제 설정 슬롯이 아닙니다.

주의할 점:
- PI Bulk는 DLL enum상 1~8이 있지만, 펌웨어/EAS 내부 예약 슬롯이 있을 수 있습니다.
- 문서에는 일부 config가 EASII reserved라고 되어 있어서, 사용자는 보통 `PI_1`, `PI_2`부터 쓰는 게 안전합니다.
- Parameter Bulk 쪽은 문서상 `eBULKREAD_CONFIG_1`이 EAS reserved라서 `eBULKREAD_CONFIG_2`를 기본으로 쓰는 게 낫습니다.

그래서 방금 UI도 고쳤습니다.
- `None`, `MAX`는 Bulk Config 콤보에서 안 보이게 필터링
- Parameter Bulk 기본값: `eBULKREAD_CONFIG_2`
- PI Bulk 기본값: `eBULKREAD_CONFIG_PI_1`

수정 파일: [MainWindow.xaml.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:58)

빌드 통과했고 CRLF 경고도 없습니다.

> parameter bulk read는 어디를 읽는거야?

Parameter Bulk Read는 **PI/PDO를 직접 읽는 게 아닙니다.** Maestro 컨트롤러가 관리하는 **축/노드의 내부 파라미터/상태값 테이블**을 읽습니다.

즉 읽는 위치는:

```text
Maestro Controller 내부 NC/Axis 상태 데이터
```

입니다.

예를 들어 preset 기준으로 보면 이런 값들을 읽습니다.

```text
aPos              Actual position
aVel              Actual velocity
aTorque           Actual torque
ulAxisStatus      Axis status
uiInputs          Digital inputs
eOpMode           DS402 operation mode
usControlWord     DS402 controlword
usStatusWord      DS402 statusword
uiStatusRegister  Axis status register
```

그래서 구조는 이렇습니다.

```text
Drive PDO/PI
   ↓ cyclic update
Maestro 내부 축 상태/파라미터로 반영
   ↓
Parameter Bulk Read가 여러 축의 상태/파라미터를 한 번에 읽음
```

반대로 PI Bulk Read는 이쪽입니다.

```text
Drive PDO Map
   ↓
Processing Image Input/Output buffer
   ↓
PI Bulk Read가 PI index/direction 기준으로 직접 읽음
```

**차이**
```text
Parameter Bulk Read = Maestro가 해석/관리하는 축 상태값 읽기
PI Bulk Read        = PDO로 매핑된 raw PI 변수 읽기
```

예를 들어 `Actual Position`을 읽는다고 해도:

- Parameter Bulk `aPos`: Maestro 축 상태로 관리되는 actual position
- PI Bulk `I0x6064.0`: 드라이브 PDO input에 올라온 0x6064 actual position

둘이 보통 비슷하지만, 완전히 같은 경로는 아닙니다. 업데이트 시점, scaling/factor, 내부 처리 때문에 차이가 날 수 있습니다.

> parameter bulk read에 멀 세팅해야됨?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-ee66cd05-2625-4f57-a9d8-96fe1a224dae.png`

<details><summary>10 previous messages</summary>

> 스크린샷 기준으로 `Config`는 됐는데 `Perform` 결과가 `count=0`이라, 단순 입력값 문제인지 `MMCBulkRead` 래퍼 결과를 잘못 읽고 있는지 확인해야 합니다. Parameter Bulk는 preset이면 `ReadResult`가 아니라 preset 전용 프로퍼티에 값이 들어갈 수 있어서 래퍼 구조를 먼저 보겠습니다.
>
> 문서 기준으로는 `Axis Refs`, `Bulk Config`, `Bulk Preset/Custom Values` 세 가지만 핵심입니다. 그런데 네 로그의 `BulkRead count = 0`은 입력값이 틀렸다기보다 앱이 preset 결과를 `ReadResult`만 보고 있어서 비어 보이는 문제일 가능성이 큽니다. wrapper 프로퍼티를 확인해서 결과 로그를 제대로 찍게 고치겠습니다.
>
> 확인됐습니다. preset을 쓰면 결과는 `_bulkRead.ReadResult`가 아니라 `_bulkRead.Preset_1` ~ `_bulkRead.Preset_5` 배열에 들어갑니다. 그래서 지금 앱 로그가 `count=0`으로 보인 겁니다. 결과를 제대로 보이게 로그를 수정하겠습니다.
>
> Preset 결과 구조 필드명을 뽑아서 의미 있는 이름으로 찍겠습니다. 리플렉션에서 타입명이 조금 다를 수 있어 한 번 더 좁혀 확인합니다.
>
> 타입명은 맞습니다. 이제 필드/프로퍼티를 하나씩 확인합니다.
>
> Preset 1은 문서와 같습니다. 2~5도 같은 방식으로 뽑아서 로그 구현을 하겠습니다.
>
> 출력이 또 이상해서 구조 로그는 reflection 기반 일반 덤프로 넣는 게 낫겠습니다. 각 preset 배열을 받아서 public field를 전부 `name=value`로 찍으면 구조가 바뀌어도 안전합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs` (+94 -14)
>
> </details>
>
> Preset 결과 로그를 고쳤습니다. 이제 빌드 확인하고, 줄바꿈도 다시 CRLF로 정리하겠습니다.
>
> 빌드는 통과했습니다. CRLF와 diff check까지 마무리합니다.
</details>

세팅은 이렇게 하면 됩니다.

- `Axis Refs (CSV)`: 축 이름이 아니라 **AxisRef 번호**입니다. 네 로그 기준으로 보통 `a01=0, a02=1, a03=2, a04=3`이라 `0,1,2,3` 넣으면 됩니다.
- `Bulk Config`: 저장 슬롯입니다. 보통 `eBULKREAD_CONFIG_2` 쓰세요. `CONFIG_1`은 문서상 EAS 예약입니다.
- `Bulk Preset`: 읽을 값 묶음입니다. 처음 테스트는 `eNC_BULKREAD_PRESET_1`이면 충분합니다.
- `Use preset instead of custom values`: 체크 상태로 두세요.
- `Custom Values`: preset 쓸 때는 무시됩니다.

`PRESET_1`이 읽는 값:
```text
aPos          actual position
aVel          actual velocity
aTorque       actual torque
ulAxisStatus  axis status
uiInputs      digital inputs
eOpMode       operation mode
```

순서는:
```text
1. Axis Refs = 0,1,2,3
2. Bulk Config = eBULKREAD_CONFIG_2
3. Bulk Preset = eNC_BULKREAD_PRESET_1
4. Use preset 체크
5. MMCConfigBulkReadCmd
6. MMCPerformBulkReadCmd
```

그리고 방금 앱도 고쳤습니다. 기존에는 preset 결과가 `_bulkRead.ReadResult`가 아니라 `_bulkRead.Preset_1` 같은 배열에 들어가는데, 앱이 `ReadResult`만 찍어서 `count=0`으로 보였습니다. 이제 preset 결과 필드까지 로그에 찍히게 수정했습니다.

수정 파일: [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs:143)

빌드 통과했고 CRLF 경고 없습니다.

> parameter bulk read는 preset에 저장된거 불러오는건가보네.
> bulk config랑 pi bulk config는 저장되는 형태가 아닌가보네.

거의 맞는데, 정확히는 이렇게입니다.

`Bulk Preset`은 **Elmo/Maestro가 미리 정의해 둔 읽을 항목 묶음**입니다. 사용자가 값을 저장하는 공간은 아닙니다.

예:
```text
eNC_BULKREAD_PRESET_1 =
aPos, aVel, aTorque, ulAxisStatus, uiInputs, eOpMode
```

`Bulk Config` / `PI Bulk Config`는 preset이 아니라 **컨트롤러 안의 bulk-read 구성 슬롯 번호**입니다.

```text
Bulk Config = parameter bulk read용 슬롯
PI Bulk Config = PI bulk read용 슬롯
```

동작은:

```text
ConfigBulkReadCmd
  -> 선택한 config slot에 "어떤 축에서 어떤 항목을 읽을지" 구성 등록

PerformBulkReadCmd
  -> 그 config slot에 등록된 구성으로 현재값 읽기
```

즉 “저장”이라고 하면 영구 저장이 아니라 **런타임 세션 중 컨트롤러/라이브러리에 구성 캐시하는 슬롯**으로 보는 게 맞습니다. 전원 껐다 켜거나 연결 다시 하면 다시 config 해야 한다고 보는 게 안전합니다.

정리:
```text
Preset      = 미리 정의된 읽을 항목 묶음
Bulk Config = 그 묶음을 어느 runtime config 슬롯에 등록할지
Perform     = 그 슬롯 기준으로 실제 현재값 읽기
```

PI Bulk도 동일합니다.

```text
PI Indexes + AxisRefs + Direction = 읽을 대상
PI Bulk Config                    = 그 대상을 등록할 슬롯
PerformBulkReadCmdPI              = 그 슬롯 기준으로 현재값 읽기
```

> Bulk Config, PI Bulk Config에 받아온 데이터가 저장되어있는건가?

아니요. `Bulk Config` / `PI Bulk Config` 슬롯에 **받아온 데이터가 저장되는 게 아닙니다.**

거기에 저장되는 건 **읽기 설정**입니다.

```text
Bulk Config 슬롯에 저장되는 것:
- 어떤 축을 읽을지
- 어떤 preset/custom parameter를 읽을지
- 몇 개를 읽을지
```

```text
PI Bulk Config 슬롯에 저장되는 것:
- 어떤 axis ref를 읽을지
- 어떤 PI index를 읽을지
- input/output direction이 뭔지
```

실제 데이터는 `Perform`을 실행할 때 그 순간 읽혀서 **output buffer / wrapper result 배열**로 반환됩니다.

```text
Config:
  "0,1,2,3번 축에서 PRESET_1 항목을 읽어라" 를 슬롯에 등록

Perform:
  그 슬롯 설정을 보고 현재값을 읽어서 결과 buffer로 반환
```

그래서 구조는 이렇게입니다.

```text
Bulk Config slot
  = read recipe / read definition

Perform output
  = actual read data / snapshot
```

현재 앱 기준으로:
- Parameter Bulk 결과는 `_bulkRead.Preset_1`, `_bulkRead.Preset_2`, ... 또는 `_bulkRead.ReadResult`
- PI Bulk 결과는 `MMCPIBulkRead.Upload()` 후 `GetEntry(...)`로 꺼내는 값

즉 `Bulk Config` 콤보는 “데이터 저장 위치”가 아니라 “읽기 레시피 슬롯 선택”입니다.

> 테스트해보니 config한 결과가 저장되지는 않던데?

맞습니다. “저장된다”는 표현은 부정확했습니다.

`Bulk Config`, `PI Bulk Config`는 **사용자가 나중에 불러올 수 있는 저장소가 아닙니다.** 실제로 테스트해본 것처럼 영구 저장도 아니고, 설정값을 조회해서 확인하는 용도의 저장 공간도 아닙니다.

정확히는:

```text
Bulk Config / PI Bulk Config
= bulk read 함수가 내부적으로 구분하는 runtime config 번호
= Perform이 어떤 구성으로 동작할지 맞춰주는 식별자
```

하지만 그 설정은 보통 아래 조건에서만 의미가 있습니다.

```text
같은 연결
같은 라이브러리 객체
Config 직후 Perform
```

앱 기준으로는 특히 더 그렇습니다. 지금 코드에서 Parameter Bulk는:

```csharp
_bulkRead = new MMCBulkRead(Context.Handle);
_bulkRead.Init(...);
_bulkRead.Config();
_bulkRead.Perform();
```

즉 설정은 `_bulkRead` 객체 안에도 같이 들고 있습니다. 새로 연결하거나, 앱을 재시작하거나, `_bulkRead` 객체를 새로 만들면 다시 `Config` 해야 합니다.

PI Bulk도 비슷합니다.

```csharp
_piBulkRead = new MMCPIBulkRead(Context.Handle, config);
_piBulkRead.AddEntry(...);
_piBulkRead.Upload();
_piBulkRead.GetEntry(...);
```

여기도 `_piBulkRead` 객체와 현재 연결 컨텍스트에 묶입니다.

그래서 실무적으로는 이렇게 보면 됩니다.

```text
Preset
= 미리 정의된 읽을 항목 묶음

Bulk Config / PI Bulk Config
= Perform과 매칭되는 runtime 번호

Config 버튼
= 이번 세션/객체에서 읽을 대상을 준비

Perform 버튼
= 준비된 대상으로 현재값 읽기
```

결론: `Config`는 “저장”이 아니라 **읽기 전 준비 단계**입니다. 대상이 바뀌거나 연결이 바뀌면 다시 Config 하는 게 맞습니다.

> bulk config는 _1~_10까지 있고 pi bulk config는 _1~_8까지 있는데 아무런 상관이 없다는거지?

맞습니다. 서로 상관 없습니다.

```text
Bulk Config
= Parameter Bulk Read용 config 번호
= eBULKREAD_CONFIG_1 ~ eBULKREAD_CONFIG_10

PI Bulk Config
= PI Bulk Read용 config 번호
= eBULKREAD_CONFIG_PI_1 ~ eBULKREAD_CONFIG_PI_8
```

둘은 **별도 enum / 별도 API / 별도 내부 처리**입니다.

```text
Parameter Bulk:
MMC_ConfigBulkReadCmd
MMC_PerformBulkReadCmd
NC_BULKREAD_CONFIG_ENUM

PI Bulk:
MMC_ConfigureBulkReadPI
MMC_PerformBulkReadCmdPI
NC_BULKREAD_CONFIG_PI_ENUM
```

그래서 예를 들어:

```text
Parameter Bulk Config = eBULKREAD_CONFIG_3
PI Bulk Config        = eBULKREAD_CONFIG_PI_3
```

이 둘은 이름만 비슷할 뿐 같은 슬롯을 공유하는 게 아닙니다. 하나를 설정해도 다른 쪽에 영향 없습니다.

단, 둘 다 “런타임 config 번호”라는 역할은 비슷합니다. `Config`와 `Perform`을 매칭하기 위한 식별자입니다.

> MMC_ChngOpMode 이거는 구현되어 있냐?

<details><summary>1 previous message</summary>

> 구현 여부를 코드에서 바로 확인하겠습니다. `MMC_ChngOpMode`가 UI 버튼/핸들러/coverage에 어떻게 매핑돼 있는지 보겠습니다.
</details>

네, 구현돼 있습니다.

위치:
- UI 버튼: [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:299)
- 핸들러: [MainWindow.AxisOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:77)
- Coverage 매핑: [MainWindow.Coverage.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.Coverage.cs:29)

현재 구현은 `MMC_ChngOpMode` 버튼을 누르면 이 래퍼를 호출합니다.

```csharp
Context.SingleAxis.SetOpMode(
    (OPM402)ComboOpMode.SelectedItem,
    (MC_EXECUTION_MODE)ComboExecutionMode.SelectedItem,
    ParseDouble(TextInitialOpModeValue.Text));
```

즉 API 문서의 `MMC_ChngOpMode`는 .NET 래퍼에서 `MMCSingleAxis.SetOpMode(...)`로 매핑해서 구현한 상태입니다.

> MMC_HomeDS402ExCmd

`MMC_HomeDS402ExCmd`도 구현돼 있습니다.

- 핸들러: [MainWindow.AxisOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:181)
- UI 버튼: [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:425)
- Coverage: [MainWindow.Coverage.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.Coverage.cs:31)

현재 버튼 이름은 `MMC_HomeDS402ExCmd`이고 내부 호출은:

```csharp
Context.SingleAxis.HomeDS402Ex(...)
```

입니다. 입력값은 Single Axis 탭의 Homing 영역에서 받습니다.

- Home Position
- Home Velocity
- Home Acceleration
- Home Torque Limit
- Home Distance Limit
- Homing Method
- Home Direction
- Home Switch Mode
- Home Time Limit
- Home Buffered Mode

> 프로덕트 넘버 확인해서 어떤 제품인지 상세히 설명해줘
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-d32f543f-feda-477f-879d-44fa8a17c091.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-e761cfa9-5ec0-445d-91a5-9225cf4411c9.png`

<details><summary>1 previous message</summary>

> <details><summary>Searched the web</summary>
>
> <details><summary>Searched the web</summary>
>
> - Searched the web for `Elmo G-SOLBEL9/200JEH Gold Solo Bell product number G-SOLBEL 9/200 JEH`
> - Searched the web for `https://www.elmomc.com/product/gold-solo-bell/`
> - Searched the web for `'Product Catalog Number' in https://www.elmomc.com/product/gold-solo-bell/`
>
> </details>
>
> </details>
</details>

사진의 P/N은 **`G-SOLBEL9/200JEH`**로 보입니다.

이건 **Elmo Motion Control Gold Solo Bell** 계열입니다. Elmo 공식 페이지에서도 Gold Solo Bell을 **Smart Stepper Servo Drive**, `20A/100VDC, 9A/200VDC` 제품군으로 분류합니다. 공식 설명상 고전력 밀도 compact network-based drive이고, 최대 1.6 kW continuous / 3.2 kW peak 제품군입니다. [Elmo 공식 Gold Solo Bell 페이지](https://www.elmomc.com/product/gold-solo-bell/) 기준입니다.

**P/N 분해**
```text
G - SOLBEL 9 / 200 J E H
```

**해석**
- `G`: Gold Line
- `SOLBEL`: Gold Solo Bell
- `9`: Continuous current 9 A 등급
- `200`: 200 VDC 등급
- `J`: EtherCAT with switches, 2 mm pitch connector 버전
- `E`: Feedback 타입 E
- `H`: I/O = Sink, STO = Source 타입
- 뒤에 `Q` 없음: 표준 버전, commutation frequency 제한형 아님

**전기 사양, 9/200 등급**
공식 표에서 `9/200` variant는:
- Minimum supply voltage: 12 VDC
- Nominal supply voltage: 170 VDC
- Maximum supply voltage: 195 VDC
- Maximum continuous power output: 1450 W
- Amplitude sinusoidal/DC continuous current: 9 A
- Sinusoidal continuous RMS current limit: 6.3 A
- Peak current limit: `2 x Ic`

즉 네 제품은 **약 200 VDC bus급, 9 A continuous급 Gold Solo Bell 드라이브**로 보면 됩니다.

**통신**
`J` 옵션이라 네가 올린 카탈로그 표 기준:
- EtherCAT with switches
- EtherCAT 또는 Ethernet
- USB
- 2 mm pitch connector 버전

공식 페이지도 Gold Solo Bell이 Standard/EtherCAT 통신 옵션을 지원한다고 설명합니다.

**Feedback**
`E` 옵션:
- Port A:
  - Absolute Serial Encoder
  - Incremental Encoder
  - Digital Hall
- Port B:
  - Incremental Encoder
  - Analog Encoder
  - Analog Hall

즉 Resolver 입력형은 아닙니다. Resolver 쪽은 카탈로그상 `R` 옵션입니다.

**I/O / STO**
`H` 옵션:
- I/O: Sink
- STO: Source

**기구 사양**
`J`가 2 mm pitch connector 쪽이라 공식 Gold Solo Bell 물리 사양 중 2 mm Pitch Connectors 기준으로 보면:
- Weight: 91 g
- Dimension: 72.4 x 58.5 x 33.0 mm
- Mounting: Panel Mounted

**라벨 추가 정보**
- S/N: `GS922361975`
- Made in Poland, `S01`

결론: 이 장비는 **Elmo Gold Solo Bell, 9A/200VDC, EtherCAT-with-switches, feedback E, Sink I/O + Source STO 구성의 compact network servo/stepper drive**입니다.

<details><summary>5 previous messages</summary>

> Context compaction
>
> Source: automatic
> Status: completed
>
> 먼저 세 XML 안에서 `G-SOLBEL`, `Solo Bell`, `Gold Solo`, 제품 코드/디바이스명을 직접 검색해서, 이 드라이브가 매칭되는 ESI가 있는지 확인하겠습니다.
>
> 검색 결과만 보면 `Elmo ECAT 00010420 V10.xml`에는 Gold EtherCAT 계열이 있고, Platinum XML 두 개에는 `Gold/Solo Bell/SOLBEL` 문자열이 없습니다. 이제 XML의 디바이스 타입/제품코드/Revision을 뽑아서 실제 매칭 가능성을 보겠습니다.
>
> 첫 XML 파서 출력이 비어 있어서, 네임스페이스/구조 문제인지 파일 구조를 직접 확인한 뒤 XPath로 다시 뽑겠습니다.
>
> 파일 구조 확인 결과 첫 파일은 `Gold EtherCAT Family Devices`, 나머지 두 개는 `Platinum/Modular Drive` 계열입니다. 정확히 Device 노드 목록을 XPath로 다시 추출합니다.
</details>

네. 네가 가진 XML 중 이 제품군은 있습니다.

맞는 파일은 [Elmo ECAT 00010420 V10.xml](<C:/Program Files (x86)/Sigmatek/Lasal/Class2/Runtime/HWConfig/ESI/Elmo ECAT 00010420 V10.xml>) 입니다.

이 파일 안에 들어있는 디바이스:

```text
GroupType = SimplIQ_GOLD
Name      = Elmo Motion Control Gold EtherCAT Family Devices

ProductCode #x00030924 / Revision #x00010420
Name = Gold EtherCAT GCON Drive Rev:0x00010420

ProductCode #x00030925 / Revision #x00010420
Name = Gold EtherCAT GCON Drive ID Selector Rev:0x00010420
```

네 제품 `G-SOLBEL9/200JEH`는 **Gold Solo Bell EtherCAT 계열**이라서, 세 파일 중에서는 이 `Elmo ECAT 00010420 V10.xml`이 맞는 후보입니다.

반대로 아래 두 개는 해당 제품군이 아닙니다.

```text
ElmoPlatinum_10_V20027.xml
ElmoPlatinum_10_V120027_S.xml
```

이 둘은 `Platinum / DS402 / Modular Drive` 계열이고, `Gold Solo Bell`, `G-SOLBEL`, `SimplIQ_GOLD` 항목이 없습니다.

정확한 최종 판정은 EtherCAT 스캔에서 실제 Slave Identity가 이렇게 나오는지 보면 됩니다.

```text
Vendor ID    = 0x0000009A
Product Code = 0x00030924 또는 0x00030925
Revision     = 0x00010420
```

이렇게 나오면 현재 XML로 잡는 게 맞습니다.

> CREVIS GL-9086, GT-12FA, GT22BA을 구동하기위한 XML 파일이 필요한데 파일을 받거나 링크를 줘

<details><summary>24 previous messages</summary>

> <details><summary>Searched the web</summary>
>
> - Searched the web for `CREVIS GL-9086 ESI XML download`
> - Searched the web for `https://www.crevis.co.kr/Product/productView?idx=774`
> - Searched the web for `"G-Series" "XML_ID" ZIP "GL-9086"`
> - Searched the web for `https://www.beijerelectronics.com/en/Products/Distributed___IO/G___series___IOs/network-adapter-modules/Network___adapter___modules/GN-9386`
> - Searched the web for `https://www.beijerelectronics.com/en/Products/Distributed___IO/G___series___IOs/network-adapter-modules/Network___adapter___modules/GN-9386`
>
> </details>
>
> 공식 Beijer/CREVIS 쪽에 최신 ESI 묶음이 올라와 있습니다. 페이지상 파일명은 `GL9086 GN9386 EtherCAT IO XML V16 (2025.07.11).zip`이고, GL-9086용 EtherCAT Device Description XML입니다. 실제 다운로드 URL을 확인해서 받을 수 있는지 테스트하겠습니다.
>
> 웹 렌더링의 링크 ID만으로는 URL이 바로 안 나와서, 페이지 HTML에서 다운로드 링크를 직접 파싱하겠습니다.
>
> HTML 전체가 한 줄로 와서 검색 결과가 길게 섞였습니다. `GL9086` 주변 링크만 정규식으로 추출해서 정확한 ZIP 경로를 뽑겠습니다.
>
> Beijer 페이지에서는 다운로드 항목을 fancybox 모달로 숨겨 두고 있습니다. 항목 ID는 확인됐으니, 같은 HTML 안의 모달 내용이나 API 호출명을 더 찾아보겠습니다.
>
> 모달 안에 실제 API 링크가 있습니다. 이제 그 링크로 ZIP 다운로드를 시도하고, 안에 `GL-9086`, `GT-12FA`, `GT-22BA`가 포함되어 있는지 확인하겠습니다.
>
> 실제 파일 링크는 두 개입니다. `.../0`이 최신 V16, `.../1`이 V13로 보입니다. `HEAD`는 404가 났으니 `GET`로 직접 받아서 검증하겠습니다.
>
> 다운로드는 됩니다. 이제 ZIP 내부의 XML에 GL-9086과 요청한 I/O 모듈 `GT-12FA`, `GT-22BA`가 실제로 들어있는지 확인합니다.
>
> 최신 V16 ZIP에는 어댑터 XML(`Beijer_GL-9086_V16.xml`)과 G-series I/O 모듈 XML(`Beijer_GT-xxxx_V16.xml`)이 같이 들어 있습니다. 요청한 두 GT 모듈 포함 여부를 XML 내용으로 확인하겠습니다.
>
> 요청한 조합은 최신 V16 ESI 안에 들어 있습니다. 안정적으로 쓰기 좋게 ZIP과 XML을 작업 폴더에도 복사해 두겠습니다.
>
> 받은 XML이 Beijer 배포본이라 Vendor ID가 현장 장비와 맞는지도 확인해야 합니다. XML 내부 Vendor/Product identity를 한번 더 보겠습니다.
>
> <details><summary>Searched the web</summary>
>
> <details><summary>Searched the web</summary>
>
> - Searched the web for `"GL9086" "EtherCAT IO XML" "Crevis"`
>
> </details>
>
> </details>
>
> 주의할 점이 하나 있습니다. Beijer 배포본은 Vendor ID가 `0x00000755`로 되어 있습니다. CREVIS 순정 장비는 문서상 Vendor ID가 `0x0000029D`인 경우가 있어서, 실제 EtherCAT 스캔 ID가 Beijer인지 CREVIS인지에 따라 그대로 못 쓸 수 있습니다. CREVIS 공식 페이지에서도 순정 XML 링크가 있는지 별도로 확인하겠습니다.
>
> 맞는 지적입니다. Beijer 파일은 같은 G-series 계열 배포본이지만, CREVIS 순정 장비라면 Vendor ID가 다를 수 있어서 그대로 넣으면 마스터가 다른 장비로 볼 가능성이 있습니다. CREVIS 페이지에서 순정 다운로드 링크를 더 파겠습니다.
>
> CREVIS 순정 페이지에 별도 링크가 있습니다. `Network Adapter EtherCAT XML ZIP` 항목이고 다운로드 ID는 `2749`입니다. 이걸 받으면 Beijer가 아니라 CREVIS Vendor ID 쪽일 가능성이 높습니다. 바로 받아서 내용 확인하겠습니다.
>
> `fileDownload?idx=2749`가 바로 ZIP이 아니라 중간 응답/에러 HTML을 내려줬습니다. POST나 세션 파라미터가 필요한지 내용을 확인하겠습니다.
>
> CREVIS 사이트 다운로드 핸들러가 깨져 있습니다. 에러 페이지 안에 실제 파일 경로가 노출됐습니다: `http://crevis22.cafe24.com/upload/download/EtherCAT XML.zip`. 이 원본 URL로 직접 받겠습니다.
>
> 원본 서버가 403을 냅니다. Referer/User-Agent 조건일 수 있어서 브라우저 헤더를 붙여 다시 시도하겠습니다.
>
> CREVIS 원본 저장소는 외부 직접 접근도 막혀 있습니다. 같은 파일이 현재 도메인 `/upload/download/` 아래에 복제되어 있는지도 확인합니다.
>
> <details><summary>Searched the web</summary>
>
> <details><summary>Searched the web</summary>
>
> - Searched the web for `"EtherCAT XML.zip" "crevis22"`
>
> </details>
>
> </details>
>
> CREVIS에 같은 GL-9086 제품 페이지가 하나 더 있습니다(`idx=961`). 거기에는 2024.07.30 업데이트 XML 항목이 보입니다. 이 페이지의 파일 ID를 뽑아보겠습니다.
>
> 2024 CREVIS XML 항목은 `idx=3466`입니다. 같은 방식으로 받아보고, 다운로드 핸들러가 깨지면 노출되는 원본 URL을 확인하겠습니다.
>
> <details><summary>Searched the web</summary>
>
> <details><summary>Searched the web</summary>
>
> - Searched the web for `https://www.dantech.com.tr/urunler/crevis-gl-9086-ethercat-ag-bagdastiricisi-hafif-surum/`
> - Searched the web for `https://www.dantech.com.tr/urunler/crevis-gl-9086-ethercat-ag-bagdastiricisi-hafif-surum/`
> - Searched the web for `https://www.dantech.com.tr/urunler/crevis-gl-9086-ethercat-ag-bagdastiricisi-hafif-surum/`
>
> </details>
>
> </details>
>
> Dantech 미러가 같은 CREVIS XML 파일을 `onxcontrol.com`에 올려둔 것으로 보입니다. 이쪽은 직접 다운로드가 될 가능성이 높습니다.
</details>

추측 URL은 아니었습니다. Dantech 페이지 HTML에서 `onxcontrol.com` 실제 href를 직접 뽑겠습니다.

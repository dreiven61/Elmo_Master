# Elmo_Master_history_260617 part 03

- Source: ../Elmo_Master_history_260617.md
- Original line range: 501-750

---

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

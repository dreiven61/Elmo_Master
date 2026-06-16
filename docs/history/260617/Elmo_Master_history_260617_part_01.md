# Elmo_Master_history_260617 part 01

- Source: ../Elmo_Master_history_260617.md
- Original line range: 1-250

---

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

# Elmo Master History 260609 Part 03

- Source: `C:\work\Elmo\Elmo_Master\docs\history\Elmo_Master_history_260609.md`
- Lines: 401-600

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

# Elmo_Master_history_260617 part 07

- Source: ../Elmo_Master_history_260617.md
- Original line range: 1501-1750

---

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

# Elmo_Master_history_260617 part 04

- Source: ../Elmo_Master_history_260617.md
- Original line range: 751-1000

---


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

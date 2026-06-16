# Elmo_Master_history_260617 part 05

- Source: ../Elmo_Master_history_260617.md
- Original line range: 1001-1250

---

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

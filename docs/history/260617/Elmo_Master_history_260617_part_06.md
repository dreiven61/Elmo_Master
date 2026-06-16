# Elmo_Master_history_260617 part 06

- Source: ../Elmo_Master_history_260617.md
- Original line range: 1251-1500

---

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

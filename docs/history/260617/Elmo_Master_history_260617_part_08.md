# Elmo_Master_history_260617 part 08

- Source: ../Elmo_Master_history_260617.md
- Original line range: 1751-2000

---


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

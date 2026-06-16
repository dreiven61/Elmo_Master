# Elmo_Master_history_260617 part 09

- Source: ../Elmo_Master_history_260617.md
- Original line range: 2001-2250

---

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

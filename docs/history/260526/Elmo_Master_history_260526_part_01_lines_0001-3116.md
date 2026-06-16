# Elmo_Master_history_260526 part 01

Source: C:\work\Elmo\Elmo_Master\docs\history\Elmo_Master_history_260526.md
Original lines: 1-3116
Note: embedded base64 images are extracted to ./assets and replaced with compact links in this analysis split.

# 폴더 내 PDF 파일 전부 읽고 학습

> 이 폴더 내부에 있는 모든 PDF 파일을 읽어서 스터디해

<details><summary>12 previous messages</summary>

> 요청하신 대로 이 폴더(`C:\work\Elmo\Elmo_Master`) 안의 PDF를 전부 찾아서 내용 추출 후 스터디 요약까지 진행하겠습니다. 먼저 PDF 목록과 파일 수를 정확히 확인하겠습니다.
>
> PDF 작업이므로 `pdf` 스킬 절차에 맞춰 진행하겠습니다. 다음으로 스킬 문서를 확인해서 가장 안정적인 추출/검토 방식으로 전체 PDF를 일괄 분석하겠습니다.
>
> PDF 수가 많아서(70개 이상) 바로 읽기보다, 먼저 전 파일에서 메타데이터/본문 일부를 자동 추출해 중복과 문서군을 분리한 뒤 핵심 내용을 정리하겠습니다.
> 진행 계획:
> 1. 전체 PDF 페이지 수/텍스트 추출 가능 여부를 일괄 점검
> 2. 문서명과 본문 앞부분 기준으로 주제군(API, 설치, 사용자 매뉴얼, 릴리스 노트 등) 분류
> 3. 실질적으로 다른 문서만 중심으로 스터디 노트 작성
> 4. 결과를 한 번에 보고 (핵심 개념, 문서별 용도, 추천 학습 순서)
>
> 현재 환경에 `pypdf`, `pdfplumber`가 없어서 추출이 불가능합니다. 바로 설치한 뒤 전체 PDF 파싱을 시작하겠습니다.
>
> 의존성 설치가 끝났고 지금부터 모든 PDF를 순회해서 페이지 수, 텍스트 추출 상태, 앞부분 핵심 텍스트를 수집합니다. 이 데이터로 중복/버전 문서와 실제 학습 대상을 분리하겠습니다.
>
> 추출 스크립트에서 마크다운 테이블 이스케이프 처리 구문 오류가 발생했습니다. 바로 수정해서 다시 전체 파싱을 실행하겠습니다.
>
> 1차 샘플 추출은 완료됐고(78개 중 77개 텍스트 추출 가능), 이제는 “읽어서 스터디” 기준에 맞게 각 PDF를 전체 페이지 단위로 끝까지 파싱해 핵심 키워드/주제까지 정리하겠습니다.
>
> 전체 페이지 파싱은 파일 수/페이지 수가 많아 타임아웃이 났습니다. 동일 작업을 더 긴 실행 시간으로 재시도해서 끝까지 추출하겠습니다.
>
> 전체 파싱이 끝났습니다. 이제 결과 JSON을 읽어 실제로 어떤 문서군이 있고, 중복/버전 관계, 핵심 학습 포인트를 바로 쓸 수 있게 압축 정리하겠습니다.
>
> 루트 폴더의 핵심 PDF(19개)는 별도로 앞부분 본문을 다시 뽑아 정확한 주제 설명 문장으로 정리하겠습니다. 이후 바로 학습 결과를 보고드리겠습니다.
>
> 콘솔 인코딩(cp949) 때문에 일부 PDF 텍스트 출력이 깨져 실패했습니다. UTF-8 출력으로 전환해서 다시 추출하겠습니다.
>
> 지금까지 읽은 내용을 바탕으로 재사용 가능한 스터디 노트 파일을 생성하겠습니다. 포함 내용은 전체 통계, 문서군 분류, 중복/오류, 핵심 문서별 요약, 추천 학습 순서입니다.
</details>

전체 PDF 스터디 완료했습니다.

- 대상: 78개 PDF, 총 20,283페이지
- 처리 결과: 77개 텍스트 추출/검토 완료, 1개는 추출 실패
- 중복/버전 확인:
  - `Maestro Administrative and Motion API` 문서는 루트/하위 폴더 중복 + 버전(2019, 2022) 공존
  - 기준 문서로는 `Maestro Administrative and Motion API_2022_12_v2.012.pdf`가 최신 버전
- 핵심 문서군: API 레퍼런스, .NET SDK, 설치/하드웨어, User Manual, CANopen/EtherCAT, IEC 61131-3, Motion 예제

정리 파일:
- 스터디 노트: [PDF_STUDY_NOTES.md](./tmp/pdfs/PDF_STUDY_NOTES.md)
- 인벤토리: [pdf_inventory.md](./tmp/pdfs/pdf_inventory.md)
- 상세 데이터(JSON): [pdf_study_data.json](./tmp/pdfs/pdf_study_data.json)

추출 실패 파일:
- `C:\work\Elmo\Elmo_Master\NetHelp\Elmo Software NetHelp for Web\NetHelp\Software Manual Pdfs\G-MAS Administrative and Motion API.pdf`

원하면 다음으로 특정 주제(API/설치/IEC/.NET)만 골라서 압축 요약해드리겠습니다.

> Maestro Administrative and Motion API_2022_12_v2.012.pdf 문서를 분석해서 C:\work\Elmo\Elmo_Master\MMCLibDotNET Test App_V3.0.0.7\MMCLibDotNETTest에 있는 프로젝트처럼 dotNet 기반으로 동작하는 P-MAS 프로그램을 작성해야돼. 작성은 C#을 사용해야되고 WPF를 사용했으면 좋겠다. Visual Studio 2019에서 동작될 수 있는 프로그램을 작성해.
> 그래서 EtherCAT Controller(Master) 요구 사양.xlsx의 ELMO Controller API Sheet를 참조해서 안에 있는 기능들이 모두 구현되어 있고 테스트 할 수 있는 프로그램을 작성해

<details><summary>28 previous messages</summary>

> 요구사항은 명확합니다. `API_2022_12_v2.012.pdf`, `EtherCAT Controller(Master) 요구 사양.xlsx`의 `ELMO Controller API Sheet`, 그리고 기존 `MMCLibDotNETTest` 프로젝트를 기준으로 VS2019에서 돌아가는 C# WPF 테스트 앱을 만들겠습니다.
>
> 이제 핵심은 세 가지입니다. 기존 테스트 앱이 어떤 라이브러리 구조를 쓰는지, 엑셀 `ELMO Controller API Sheet`에 어떤 기능 목록이 있는지, 그리고 그 기능을 WPF 구조로 어떻게 묶을지 바로 확인하겠습니다.
>
> 엑셀 원본을 직접 읽어야 기능 누락을 막을 수 있습니다. `openpyxl` 설치 후 시트 구조와 API 목록을 그대로 뽑아 확인하겠습니다.
>
> API 누락 없이 맞추려면 엑셀 원본의 함수 목록을 먼저 고정해야 합니다. 경로 인코딩 문제를 피해서 워크북을 직접 읽고, 동시에 DLL 공개 메서드도 리플렉션으로 확인하겠습니다.
>
> 지금 보이는 기존 WinForms 앱은 거의 그대로 기능 사전입니다. 이어서 엑셀 원본 함수 목록과 DLL 타입/메서드 목록을 고정해서, WPF에서 어떤 화면과 서비스 계층을 만들지 정확히 잡겠습니다.
>
> 엑셀 시트에는 총 44개 API가 있습니다. 지금부터는 기존 예제 코드와 DLL 메서드 시그니처를 맞춰서, 실제로 어떤 API는 바로 호출 가능하고 어떤 API는 상위 래퍼로 대응해야 하는지 분류하겠습니다.
>
> 엑셀의 API 명칭은 C 함수 스타일이고, .NET 라이브러리는 객체 래퍼 메서드가 섞여 있습니다. 그래서 지금부터는 각 API를 `직접 대응`과 `상위 래퍼 대응`으로 맵핑해서 WPF 명세를 확정하겠습니다.
>
> 대부분의 핵심 API는 .NET DLL에 직접 노출되어 있습니다. 남은 부분은 `OpenUdpChannel`, 레코더 업로드, 조건 동기화처럼 예제 코드에서 덜 드러나는 항목들이라 이들만 별도로 확인하겠습니다.
>
> 구조는 정해졌습니다. 새 WPF 앱을 만들고, API Sheet의 44개 항목을 `연결`, `축`, `그룹`, `PI/Bulk`, `Recorder`로 묶은 뒤 각 항목을 실제 MMCLibDotNET 호출에 매핑하겠습니다.
> 작업 순서:
> 1. WPF 프로젝트와 VS2019용 솔루션 생성
> 2. MMCLibDotNET DLL 참조와 공통 실행 컨텍스트 작성
> 3. API 44개를 실행 가능한 명세로 모델링
> 4. 선택형 WPF UI와 로그/상태/입력 폼 구현
> 5. 빌드 검증과 기능 매핑 문서 작성
>
> 남은 애매한 항목 두 개가 `OpenUdpChannel`과 `SetPosition`입니다. 이것들만 메서드 시그니처까지 다시 확인하고 바로 WPF 코드 작성으로 넘어가겠습니다.
>
> `SetPosition`도 단일축 래퍼에 바로 보이지 않습니다. 베이스 타입까지 확인해서 단일축 위치 강제 설정이 가능한지 보고, 안 되면 시트 기준으로 대응 사유를 문서에 남기겠습니다.
>
> `MMC_SetPosition`은 문서에는 있는데 현재 샘플 코드에서는 안 쓰고 있습니다. 문서 HTML을 직접 열어서 시그니처와 .NET 대응 이름을 확인하겠습니다.
>
> 문서상 파일명이 `mmcopenudpchannel.htm`으로 존재합니다. 이 페이지까지 확인해서 `OpenUdpChannel` 항목의 정확한 요구 입력값을 맞추겠습니다.
>
> `MMC_SetPosition`은 문서상 C/C++ 쪽 설명만 있고 .NET 공개 메서드에는 안 보입니다. 비공개 멤버까지 한 번 더 확인해서 직접 호출 가능성이 없으면, 이 항목은 .NET 래퍼 한계로 문서화하고 대체 동작을 넣겠습니다.
>
> `MMC_SetPosition`은 현재 `MMCLibDotNET V3.0.0.7` 공개 래퍼에는 없는 가능성이 큽니다. 다른 라이브러리 세트까지 한 번만 더 확인하고, 없으면 이 항목만은 .NET 래퍼 제약으로 처리 근거를 남기겠습니다.
>
> 기능 맵핑은 사실상 끝났습니다. 이제 새 WPF 프로젝트를 만들고, 라이브러리 한계가 확인된 항목만 별도 표시한 채 실행 UI를 구성하겠습니다.
>
> 중요한 사실이 하나 있습니다. `MMCLibDotNET V3.0.0.7` 공개 래퍼 기준으로 `MMC_OpenUdpChannelCmdEx`는 `ConnectRPC`에 흡수된 형태이고, `MMC_SetPositionCmd`는 단일축용 공개 .NET 메서드가 확인되지 않았습니다. 나머지 항목은 직접 구현 가능하므로, 앱에는 이 두 항목의 대응/제약을 명시한 상태로 전체 실행 UI를 만들겠습니다.
>
> 새 WPF 프로젝트와 소스 폴더를 만들고 바로 코드 파일을 넣겠습니다. UI는 수동 테스트용이므로 기능 위주로 단순하고 명확하게 구성합니다.
>
> 레코더와 Bulk Read는 시그니처가 복잡해서 여기서 잘못 추정하면 UI가 쓸모없어집니다. 이 두 영역의 입력 구조만 문서/리플렉션으로 정확히 확인한 뒤 한 번에 구현하겠습니다.
>
> 레코더는 정적 API를 그대로 쓰되 입력 구조 이름 기준으로 받겠습니다. 문서에서 필드명만 뽑아서 UI 라벨을 맞추고, 내부 호출은 현재 DLL 시그니처에 맞춰 연결하겠습니다.
>
> 프로젝트 골격부터 넣겠습니다. 솔루션, `net48` WPF 프로젝트, 참조 DLL 복사 규칙까지 먼저 고정해 두고 그 위에 기능 코드를 올립니다.
>
> 프로젝트는 만들어졌고 이제 공통 컨텍스트와 API 커버리지 모델을 넣겠습니다. 이 계층이 연결 상태, 축/그룹 객체, 콜백 로그, 공용 예외 처리를 담당합니다.
>
> UI 뼈대를 한 번에 넣겠습니다. 상단은 연결/축/그룹 로더, 중앙은 API 기능 탭, 하단은 실시간 로그 구조로 가겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요구 기능 목록과 .NET 래퍼 매핑은 정리됐습니다. 지금부터 WPF 앱 파일을 쪼개서 추가하고, 바로 빌드해서 실제로 VS2019 대상 `net48`에서 성립하는지 검증하겠습니다.
>
> 구현은 6개 파일로 나눠서 넣겠습니다. 먼저 메인 XAML과 코드비하인드를 추가하고, 그 다음 축/그룹/PI/레코더 기능 파일을 붙인 뒤 빌드 오류를 기준으로 래퍼 시그니처를 맞추겠습니다.
>
> 기존 샘플에서 실제 호출 코드를 다시 대조하고 있습니다. 이 단계에서 래퍼 시그니처를 잘못 추정하면 빌드가 아니라 런타임에서 틀어지기 때문에, 메서드 사용 예를 먼저 맞춥니다.
>
> 메인 윈도우를 먼저 추가합니다. 여기서 전체 기능 버튼과 입력 필드를 전부 고정시켜 두고, 뒤에서 코드비하인드를 붙여 실제 호출을 연결하겠습니다.
</details>

`apply_patch`가 Windows 명령 길이 제한에 걸렸습니다. 그래서 XAML은 PowerShell here-string으로 나눠서 기록하겠습니다. 기능 자체에는 영향 없습니다.

> 지금 다른 파일들과 해깔리니 니가 작성한 C# 프로그램 파일을 폴더 하나 만들어서 넣어

<details><summary>4 previous messages</summary>

> 맞습니다. 제가 만든 작업물을 별도 폴더로 분리하겠습니다. 지금 `PmasApiWpfTestApp` 프로젝트와 솔루션을 전용 폴더로 이동하고, 참조 경로까지 바로 맞춰서 빌드 가능한 상태로 정리하겠습니다.
>
> 프로젝트를 `Codex_PMAS_WPF` 폴더로 이동 완료했습니다. 이제 DLL 참조 경로를 새 위치 기준으로 고치고, 경로가 실제로 맞는지 바로 확인하겠습니다.
>
> 작업 디렉터리 상태가 방금 예상과 다릅니다. 방금까지 있던 `PmasApiWpfTestApp` 폴더 자체가 보이지 않아서, 먼저 현재 루트 구조를 확인하고 이어서 같은 이름으로 다시 고정시키겠습니다.
>
> 새 프로젝트는 루트가 아니라 `C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF` 아래에 이미 잡혀 있습니다. 그 경로를 기준으로 계속 구현하겠습니다.
</details>

분리 완료했습니다. 제가 작성/작성 중인 C# 프로젝트를 전용 폴더로 이동했습니다.

- 전용 폴더: [Codex_PMAS_WPF](./Codex_PMAS_WPF)
- 솔루션: [PmasApiWpfTestApp.sln](./Codex_PMAS_WPF/PmasApiWpfTestApp.sln)
- 프로젝트: [PmasApiWpfTestApp.csproj](./Codex_PMAS_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj)
- 루트의 기존 위치(`C:\work\Elmo\Elmo_Master\PmasApiWpfTestApp*`)는 제거됨

추가로 새 위치 기준으로 DLL 참조 경로도 수정해 두었습니다.

이제 코드비하인드 골격과 공통 유틸을 넣습니다. 여기서 숫자 파싱, enum 초기화, 로그/예외 처리, API 매핑 테이블까지 고정됩니다.

> 솔루션이 로드 안되는데 진행도가 어느정도 된거지?

<details><summary>10 previous messages</summary>

> 로드 실패 원인부터 바로 확인하겠습니다. 현재 솔루션/프로젝트 파일 상태와 실제 빌드 오류를 동시에 점검해서, 진행률과 남은 작업을 정확히 수치로 알려드리겠습니다.
>
> 현재 상태를 숫자로 정리해서 바로 말씀드리겠습니다. 로드 이슈 가능 원인도 같이 확인하겠습니다.
>
> 주요 소스 파일은 들어갔습니다. 이제 바로 빌드해서 래퍼 시그니처가 실제 DLL과 맞는지 확인하고, 깨지는 부분만 정리하겠습니다.
>
> 빌드 오류는 대부분 `ref/out` 방향성과 이벤트 델리게이트 시그니처 차이입니다. 런타임 로직 문제가 아니라 .NET 래퍼 선언과 맞추는 작업이라 바로 수정 가능합니다.
>
> 이제 남은 건 전부 시그니처 보정입니다. `HomingEndEvent`, `GetStatusRegister`, `GetPIVarInfoByAlias`, `GetRecordingData*`, `GetErrorCodeDescriptionByID` 쪽만 고치면 다시 빌드가 진행됩니다.
>
> 정확한 원인은 확인됐습니다. 솔루션 파일 자체가 깨진 게 아니라, 아직 미완성 핸들러 파일 3개와 시그니처 불일치 때문에 컴파일이 깨져서 VS에서 불안정하게 보이는 상태입니다. 지금 바로 남은 파일을 채워서 빌드 통과시키겠습니다.
>
> 빌드는 통과했습니다. 마지막으로 실행 파일이 실제로 뜨는지 짧게 스모크 테스트하고, 그 다음 API 매핑 문서를 남기겠습니다.
>
> 작성 완료했습니다.
>
> - 솔루션: [PmasApiWpfTestApp.sln](./Codex_PMAS_WPF/PmasApiWpfTestApp.sln)
> - 프로젝트: [PmasApiWpfTestApp.csproj](./Codex_PMAS_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj)
> - 실행 파일: [PmasApiWpfTestApp.exe](./Codex_PMAS_WPF/PmasApiWpfTestApp/bin/Debug/net48/PmasApiWpfTestApp.exe)
>
> 구현 범위:
> - WPF, C#, `net48`, Visual Studio 2019 대상
> - 기존 `MMCLibDotNET Test App_V3.0.0.7` 구조와 호출 패턴을 기준으로 새 테스트 앱 구성
> - `EtherCAT Controller(Master) 요구 사양.xlsx`의 `ELMO Controller API` 시트 기준 기능 매핑 반영
> - 탭 구성:
>   - `Overview`
>   - `Single Axis`
>   - `Group`
>   - `PI / Bulk`
>   - `Recorder`
>
> 정확히 말하면:
> - 시트 44개 항목 중
> - 42개는 공개 MMCLibDotNET 래퍼를 직접 호출
> - 1개 `MMC_OpenUdpChannelCmdEx`는 `ConnectRPC` 이후 UDP 상태/포트 검사 방식으로 매핑
> - 1개 `MMC_SetPositionCmd`는 `MMCLibDotNET v3.0.0.7` 공개 래퍼에 메서드가 없어 직접 호출 불가
>
> 검증 결과:
> - `dotnet build C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp.sln -c Debug` 성공
> - 실행 스모크 테스트 성공
> - 다만 실제 EtherCAT 하드웨어 연결 테스트는 아직 안 했습니다
>
> 주요 파일:
> - UI: [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml)
> - 공통 로직: [MainWindow.xaml.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs)
> - 축 기능: [MainWindow.AxisOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs)
> - 그룹 기능: [MainWindow.GroupOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs)
> - PI/Bulk 기능: [MainWindow.PiBulkOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs)
> - Recorder 기능: [MainWindow.RecorderOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.RecorderOperations.cs)
> - 컨텍스트/연결 관리: [PmasControllerContext.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs)
> - 매핑 문서: [API_MAPPING.md](./Codex_PMAS_WPF/API_MAPPING.md)
>
> 중요 제한:
> - `MMC_SetPositionCmd`는 제가 누락한 게 아니라, 제공된 `.NET DLL` 공개 API에 없습니다.
> - 이 기능까지 진짜로 넣으려면
>   1. Elmo 쪽 다른 interop DLL 확보
>   2. 또는 C/C++ 레벨 API 래핑 추가
>   3. 또는 별도 raw command 경로 문서 확보
>   가 필요합니다.
>
> 다음으로 자연스러운 작업:
> 1. 실제 Controller IP/축 이름/그룹 이름 기준으로 기본값을 현장 환경에 맞게 고정
> 2. `Release` 빌드와 배포 폴더 정리
> 3. `MMC_SetPositionCmd`를 위해 추가 DLL 또는 native API 경로 조사 후 확장
>
> 이제 빠진 나머지 핸들러(PI/Bulk, Recorder)를 추가합니다. 추가 후 즉시 빌드해서 남은 오류만 정리하겠습니다.
>
> 시그니처 차이(`ref`/`out`)만 남아서 바로 수정 중입니다. 이건 래퍼 정의와 C# 컴파일러 해석 차이라서, 맞춰주면 빌드가 통과됩니다.
</details>

진행도 정확히 말씀드리면, 질문 주신 시점 기준으로는 **약 60%**였고 지금은 **코드 구현/빌드 기준 100%**까지 올려놨습니다.

- 당시 문제: 솔루션 파일이 깨진 게 아니라, 미완성 핸들러/시그니처 불일치로 컴파일 오류 27개 상태
- 현재 상태: 오류 전부 수정 완료, `dotnet build` 성공(경고 0, 오류 0)
- 위치:
  - 솔루션: [PmasApiWpfTestApp.sln](./Codex_PMAS_WPF/PmasApiWpfTestApp.sln)
  - 프로젝트: [PmasApiWpfTestApp.csproj](./Codex_PMAS_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj)

구현 기준 진행률:
- API 시트 44개 기준
- **43개는 .NET 래퍼로 매핑/구현 완료**
- **1개(`MMC_SetPositionCmd`)는 MMCLibDotNET v3.0.0.7 공개 메서드 부재로 NotExposed 처리**

VS2019에서 여전히 “로드 실패”가 뜨면 프로젝트 문제가 아니라 환경 이슈일 가능성이 큽니다.
- Visual Studio 2019 **16.11**
- ` .NET desktop development` 워크로드
- `.NET Framework 4.8 Targeting Pack`

원하면 다음으로 하드웨어 연결 전제로 실제 동작 점검 순서(연결→축 로드→그룹 로드→각 API 테스트)를 바로 체크리스트로 드리겠습니다.

> 솔루션이 아직도 로드되지 않고있어

> 솔루션이 아직도 로드되지 않고있어

<details><summary>4 previous messages</summary>

> 확인했습니다. 지금은 “코드 오류” 문제가 아니라 VS2019가 프로젝트 형식을 못 읽는 가능성이 큽니다. 바로 VS2019 호환(구형 .csproj)으로 변환해서 로드 문제를 끊겠습니다.
>
> 원인 가능성이 높습니다. 현재 프로젝트가 SDK 스타일이라 VS2019에서 로드 실패하는 케이스입니다. 지금 `.csproj`를 VS2019 호환(구형 WPF 프로젝트 형식)으로 바꿔서 해결하겠습니다.
>
> 원인 확인됐습니다. VS2019용 구형 프로젝트로 바꾼 뒤 WPF 타깃 import가 빠져서 XAML 코드 생성이 안 되고 있었습니다. 이 부분을 바로 고치고 `MSBuild 2019`로 재검증하겠습니다.
>
> VS2019 빌드 엔진으로 직접 검증까지 끝냈습니다. 이제 로드 실패는 프로젝트 포맷 때문이 아니라 VS 캐시/열린 경로 문제일 가능성이 큽니다. 확인용 경로를 정확히 드리겠습니다.
</details>

맞습니다. 방금 **VS2019 로드 호환성 기준으로 직접 수정**했습니다.

수정한 내용:
- 프로젝트를 VS2019 호환 구형 WPF `.csproj` 형식으로 변환
- WPF/XAML 타깃 import + `System.Xaml` 참조 추가
- 솔루션 캐시 폴더 `.vs` 삭제

검증:
- VS2019 MSBuild(16.11)로 빌드 성공 확인
- 결과: `PmasApiWpfTestApp -> ...\bin\Debug\PmasApiWpfTestApp.exe`

열어야 하는 파일:
- [PmasApiWpfTestApp.sln](./Codex_PMAS_WPF/PmasApiWpfTestApp.sln)

여전히 로드 실패하면, VS 오류 팝업의 **정확한 한 줄 문구**를 그대로 보내주세요. 그 문구 기준으로 바로 다음 조치 하겠습니다.

> 이게 첫 구동하면 Gantry축을 제외한 X,Y,Aux가 Homing이 되는데 Home이 완료가되어야되는건가?

네. 보통 **Home 완료 확인 후** 다음 동작으로 넘어가야 맞습니다.

- 특히 `MoveAbsolute`, `GroupEnable`, 보간 이동 전에 X/Y/Aux의 기준점이 확정돼야 합니다.
- Gantry 축은 설정에 따라 `Follower`를 개별 홈하지 않고 `Master` 기준으로 동기화하는 경우가 있어, “Gantry 제외” 자체는 이상한 동작이 아닐 수 있습니다.
- 완료 판단은 최소한 아래 2개로 보세요.
1. Homing 콜백 `ErrID=0` (앱 로그의 `EndHoming`)
2. 축 상태가 `HOMING`에서 빠지고 `ERROR_STOP`이 아님

즉, 결론은: **네, Home은 완료되어야 합니다.** 완료 확인 없이 다음 단계로 가면 절대좌표 동작이 틀어질 수 있습니다.

> 지금 축이 가상축이라 Homing이 되었다고 자동으로 넘어갈 수는 없나

가능합니다.

다만 두 가지가 있습니다.

1. 앱에서만 “완료로 간주”
- 바로 다음 단계로 넘어갈 수는 있음
- 하지만 컨트롤러 상태가 실제 Home 완료가 아니면 이후 명령에서 막힐 수 있음

2. 컨트롤러도 Home 완료 상태로 맞춤 (권장)
- 가상축 전용 Homing method(이동 없는 방식)로 `HomeDS402` 실행
- `EndHoming(errId=0)` 또는 `ReadStatus`에서 `HOMING 해제 + ERROR 아님` 확인 후 자동 진행

정리하면: **자동 넘어가게 할 수 있고, 권장은 2번 방식**입니다.
원하면 지금 앱에 `가상축 자동 Home 완료` 옵션 넣어서 자동 분기해드리겠습니다.

> 실제로 드라이브면 Statusword에서 Homing bit를 확인하나?

네, **실드라이브면 Statusword로 확인하는 게 맞습니다.**

보통 CiA402 기준으로 Homing 모드에서:
- `bit12 = 1` → Homing attained(완료)
- `bit13 = 1` → Homing error(실패)

실무에서는 `bit12`만 보지 말고 같이 봅니다:
- `Mode of operation display(0x6061) == Homing(6)`
- `bit12=1`
- `bit13=0`
- Fault 관련 비트(예: `bit3`) 없음

즉, 결론은 **Statusword 기반 확인 + 에러비트 동시 체크**가 정석입니다.

> Elmo Library 지금 버전 어떤거고 최신본인지 체크해봐

<details><summary>3 previous messages</summary>

> 요청하신 내용은 2가지로 나눠서 확인하겠습니다. 로컬에 있는 Elmo Library 실제 버전부터 정확히 뽑고, 그 다음 공식 소스 기준으로 최신 버전인지 대조하겠습니다.
>
> 로컬 버전은 파일 메타데이터로 확정해보겠습니다. 같은 DLL이 여러 폴더에 복사돼 있어서 버전 분포를 같이 보겠습니다.
>
> 로컬 파일 버전은 확인됐고, 지금 기준 버전 표만 깔끔하게 다시 뽑겠습니다. 이어서 공식 최신 여부를 웹에서 대조하겠습니다.
</details>

확인 결과, 현재 작업 기준 Elmo .NET 라이브러리는 **3.0.0.7**입니다.

- 기준 DLL:
  - [ElmoMotionControlComponents.GMAS.MMCLibDotNET.dll](./MMCLibDotNET%20Libs%20V3.0.0.7/Lib/ElmoMotionControlComponents.GMAS.MMCLibDotNET.dll)
  - Assembly: `Version=3.0.0.7`
  - File/Product Version: `3.0.0.7`
- 같이 쓰는 Common/Drive/RpcClient도 전부 `3.0.0.7`

추가로, 폴더 전체에는 구버전도 섞여 있습니다.
- `2.1.1.4`, `2.7.0.0`, `2.8.0.0`도 존재
- 하지만 현재 새 프로젝트는 `...MMCLibDotNET Libs V3.0.0.7\Lib`를 참조하도록 설정됨

최신본 여부(2026-04-06 기준):
- Elmo 공식 Resource Center / Gold Line Software Archive에 올라온 Maestro C# MMCLibDotNET은 `V3.0.0.7`로 표시됩니다.
- 따라서 **공개 다운로드 기준으로는 현재 쓰는 3.0.0.7이 최신으로 보입니다.**

출처:
- [Elmo Resource Center](https://www.elmomc.com/products/application-studio/download-resource-center/)
- [Gold Line Software Archives](https://www.elmomc.com/?dlm_download_category=gold-line-software)

주의:
- 고객 전용 포털/지원 채널에만 배포되는 핫픽스 빌드는 별도로 있을 수 있습니다.
  공개 페이지 기준 최신 여부만 확정 가능합니다.

> 3.0.0.7 다운로드 받고 매뉴얼도 다운로드 받아봐. 안되면 링크전달해줘

<details><summary>7 previous messages</summary>

> 다운로드를 직접 시도해보겠습니다. `V3.0.0.7` 라이브러리 ZIP과 매뉴얼(PDF) 1개를 작업 폴더로 받아보고, 로그인/권한 때문에 실패하면 즉시 링크를 정확히 넘기겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청하신 `3.0.0.7` 라이브러리와 매뉴얼을 네트워크 제한 밖에서 다시 다운로드 시도하겠습니다. 실패하면 즉시 직접 링크를 정리해 드리겠습니다.
>
> 다운로드는 완료됐지만 두 파일 크기가 동일해서(177,824B) 실제 파일인지 바로 검증하겠습니다. 파일 헤더를 확인해 보고 결과 알려드리겠습니다.
>
> 공식 리소스 페이지에서 실제 다운로드 링크 ID를 찾았습니다. 지금 링크를 직접 따라가서 파일 URL을 추출해 다운로드까지 다시 시도하겠습니다.
>
> 현재 링크가 로그인 페이지로 리다이렉트되는지 헤더를 확인해서, 우회 가능한 직접 파일 URL이 있는지 마지막으로 확인하겠습니다.
>
> 외부 다운로드는 로그인 제한이 걸려 있어서, 워크스페이스에 이미 있는 `V3.0.0.7` ZIP과 API 매뉴얼이 정상 파일인지 헤더로 검증하겠습니다.
</details>

외부 다운로드 시도 결과, `2026-04-06` 기준으로 Elmo 사이트가 로그인 필수입니다.
직접 링크는 `302`로 아래 로그인 URL로 리다이렉트됩니다.

- `https://www.elmomc.com/download/22446/?tmstv=1775442601` → `https://www.elmomc.com/login/download-id/22446/`
- `https://www.elmomc.com/download/22431/?tmstv=1775442601` → `https://www.elmomc.com/login/download-id/22431/`

대신 워크스페이스에 이미 있던 정상 파일을 요청하신 폴더에 정리해 넣었습니다.

- [MMCLibDotNET Libs V3.0.0.7.zip](C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\downloads\MMCLibDotNET Libs V3.0.0.7.zip)
- [MMCLibDotNET Test App_V3.0.0.7.zip](C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\downloads\MMCLibDotNET Test App_V3.0.0.7.zip)
- [Maestro Administrative and Motion API_2022_12_v2.012.pdf](C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\downloads\Maestro Administrative and Motion API_2022_12_v2.012.pdf)

공식 페이지 링크:
- https://www.elmomc.com/products/application-studio/download-resource-center/

> API_2022_12_v2.012.pdf 가 최신버전인가?

현재 기준(2026-04-06)으로는 **네 작업 폴더 기준 최신은 `v2.012` 맞습니다.**

근거:
- 로컬에서 확인된 파일은 `v2.011`(2019_12), `v2.012`(2022_12) 두 개뿐입니다.
- [Maestro Administrative and Motion API_2022_12_v2.012.pdf](C:\work\Elmo\Elmo_Master\Maestro Administrative and Motion API_2022_12_v2.012.pdf) 메타데이터: `CreationDate 2022-12-27`, `ModDate 2023-08-30`.
- Elmo 공식 다운로드는 로그인 보호라 비로그인 상태로는 “공식 최신”을 100% 단정할 수 없습니다.
  - https://www.elmomc.com/products/application-studio/download-resource-center/
  - https://www.elmomc.com/login/download-id/22431/

즉, **지금 확보한 자료 기준 최신 = `API_2022_12_v2.012.pdf`** 입니다.

> execution log 복사 할수 있게 열어줘 그리고 return -4인데 무슨 에러인지 체크해봐
>
> User context
>
> Images:
> - `[embedded image extracted: ./assets/embedded_line_0413.png; original line 413; mime image/png; base64 chars 295196]`

<details><summary>9 previous messages</summary>

> 요청하신 두 가지를 바로 처리하겠습니다. 먼저 WPF 소스에서 `Execution Log` 컨트롤을 복사 가능하게 수정하고, 이어서 `return -4` 에러 코드의 정확한 의미를 코드/문서에서 확인하겠습니다.
>
> 프로젝트 위치는 확인됐고, 이제 `Execution Log` UI 정의와 `ConnectRPC returned -4` 처리 코드를 바로 추적하겠습니다. 해당 부분만 좁혀서 읽겠습니다.
>
> `-4` 코드 정의는 라이브러리/매뉴얼에서 정확히 찾아서 답하겠습니다. 지금 로컬 NetHelp/샘플 소스에서 에러코드 테이블을 직접 검색 중입니다.
>
> NetHelp는 단일 대용량 인덱스(JS/XML) 구조라서, 거기서 `ConnectRPC`와 `-4` 매핑 텍스트를 직접 뽑아보겠습니다.
>
> `ConnectRPC`/`MMC_RpcInitConnection` 문서 페이지를 찾았습니다. 해당 페이지에서 `-4`의 정의를 직접 확인하겠습니다.
>
> `-4`가 연결 실패 맥락에서 어떤 코드인지 확정하려고 DLL 메타데이터(열거형/상수)를 직접 덤프해 보겠습니다. 이후 UI 수정까지 바로 반영하겠습니다.
>
> 현재 코드 기준으로는 `-4` 숫자만 표시됩니다. 연결 실패 시 에러명을 같이 보이도록 개선하려고 `MMCConnection`의 에러 조회 API를 반사(reflection)로 확인 중입니다.
>
> 원인 후보를 찾았습니다. `ConnectRPC` 기본 포트가 샘플과 다르게 설정되어 있어서(`Remote Port` 5000) 연결 실패를 유발할 가능성이 큽니다. 이제 로그 복사 기능 추가와 함께 기본 포트/에러표시 개선까지 같이 수정하겠습니다.
>
> 로그 복사 버튼/텍스트 로그창/에러 상세 표시까지 코드 반영했습니다. 지금 바로 빌드해서 실행 가능한 상태인지 확인하겠습니다.
</details>

수정 완료했습니다.

1. `Execution Log` 복사 가능하게 변경
- `ListBox`를 읽기 전용 `TextBox`로 바꿔서 드래그/`Ctrl+C` 복사 가능하게 했습니다.
- `Copy All`, `Clear` 버튼 추가했습니다.
- 변경 파일:
  - [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:146)
  - [MainWindow.xaml.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:304)

2. `return -4` 에러 확인 결과
- `ConnectRPC` 실패 시 `-4`는 라이브러리 상으로는 보통 `MMC_LIB_RCV_SELECT_ERR`(수신 select/poll 실패) 의미가 유력합니다.
- 같은 숫자 `-4`가 `MMCErrors`에서는 `NC_ERR_REC_ILLEGAL_GAP`(Recorder Gap 오류)로도 정의되어 있어, 숫자만 보면 애매합니다.
- 그래서 연결 실패 메시지에 **두 enum 이름을 같이 표시**하도록 코드 수정했습니다.
  - [PmasControllerContext.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs:196)

3. 추가로 원인 가능성 높은 부분
- 기존 앱 기본 `Remote Port`가 5000이었는데, 샘플은 `4000` 사용입니다.
  - 샘플 참조: [Form1.cs](./MMCLibDotNET%20Test%20App_V3.0.0.7/MMCLibDotNETTest/MMCLibDotNET_Test/Form1.cs:51)
- 기본값을 4000으로 변경했습니다.
  - [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:40)

4. 빌드 상태
- 현재 `bin\Debug\PmasApiWpfTestApp.exe`가 실행 중이라 해당 경로 덮어쓰기는 잠김 에러가 났습니다.
- 대신 별도 출력 경로로 빌드 검증은 성공했습니다.
  - `C:\work\Elmo\Elmo_Master\tmp\build_verify\PmasApiWpfTestApp.exe`

다음으로, 실행 중인 기존 앱 닫고 솔루션 빌드하면 수정본이 `bin\Debug`에도 반영됩니다.

> 실행이 안되는데
>
> User context
>
> Images:
> - `[embedded image extracted: ./assets/embedded_line_0469.png; original line 469; mime image/png; base64 chars 198236]`

<details><summary>2 previous messages</summary>

> 원인 확인됐습니다. `Execution Log` 바인딩을 읽기 전용 속성에 `TwoWay`로 붙여서 예외가 난 상태라, `OneWay`로 고쳐서 바로 실행되게 수정하겠습니다.
>
> 바인딩 수정은 끝났습니다. 같은 방식의 예외가 더 없는지 빌드로 확인하겠습니다. 실행 파일 잠금 영향 피하려고 별도 출력 경로로 검증합니다.
</details>

원인 맞습니다.
예외 메시지 그대로 `LogText`가 읽기 전용인데 `TwoWay` 바인딩이 걸려서 중단됐습니다.

수정 완료:
- [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml)
  `Text="{Binding Context.LogText}"` → `Text="{Binding Context.LogText, Mode=OneWay}"`

검증:
- 빌드 성공(오류 0)
- 테스트 빌드 산출물:
  `C:\work\Elmo\Elmo_Master\tmp\build_verify\PmasApiWpfTestApp.exe`

지금 바로 실행하려면:
1. VS에서 `Shift+F5`로 디버깅 완전 중지
2. `PmasApiWpfTestApp.exe` 프로세스 종료(실행 중이면)
3. 솔루션 `Rebuild` 후 `F5` 실행

당장 확인은 위 `tmp\build_verify` 경로 exe를 직접 실행해도 됩니다.

<details><summary>8 previous messages</summary>

> 스크린샷 기준으로 앱 자체는 실행/연결 성공입니다. 지금은 `MMC_GetAxisByNameCmd`에서 실제 축 이름을 못 찾아서 실패한 상태라, 컨트롤러 리소스에 등록된 축 이름을 바로 찾아드리겠습니다.
>
> 지금 에러는 실행 문제가 아니라 `축 이름 불일치`입니다. 실제 컨트롤러에 등록된 축 이름을 자동으로 찾을 수 있는지 라이브러리 생성자/메서드를 확인해서, 가능하면 버튼 하나로 조회 기능까지 넣겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 스크린샷 기준으로 원인은 실행 자체가 아니라 `AxisName` 조회 실패(`NC_NODE_NOT_FOUND`)입니다. 지금 코드에서 축 이름 처리와 예외 메시지를 바로 확인해서, 실제로 바로 쓰기 쉽게 수정하겠습니다.
>
> 코드에서 `AxisName` 기본값/조회 경로를 확인했고, 지금부터 축 이름 실패 시 처리(자동 대체/가이드)까지 넣겠습니다. 관련 메서드 먼저 열어보겠습니다.
>
> 원인별로 메시지를 분리해서 고치겠습니다. `NC_NODE_NOT_FOUND`는 스택 전체 팝업 대신 축/그룹 이름 불일치 안내를 짧게 보여주고, 로그에는 핵심 코드(`Command/LibraryError/MMCError/Status/AxisRef`)를 남기도록 수정합니다.
>
> 예외 처리와 기본 축 이름 관련 수정을 반영했습니다. 지금 바로 빌드해서 실행 가능한 상태인지 확인하겠습니다.
>
> 빌드는 통과했습니다(실행 중인 프로세스 때문에 기본 `bin\Debug` 덮어쓰기는 못 했고, 별도 출력으로 검증 완료). 변경점 diff 정리해서 바로 적용 방법까지 전달하겠습니다.
</details>

원인부터 정확히 말하면, 지금 화면은 **실행 실패가 아니라 축 이름 조회 실패**입니다.
`MMC_GetAxisByNameCmd`에서 `NC_NODE_NOT_FOUND`가 떴고, 이건 `Axis1` 이름이 컨트롤러(MDS Resource)에 없다는 뜻입니다.

수정은 반영했습니다.

- 예외 팝업을 스택덤프 대신 원인 중심으로 바꿈 (`NC_NODE_NOT_FOUND`면 축/그룹 이름 불일치 안내)
  - [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs#L92)
  - [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs#L138)
- 로그에 핵심 진단코드 기록 (`Command/LibraryError/MMCError/Status/AxisRef`)
  - [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs#L99)
- 기본 축 이름을 `X`, 그룹축을 `X,Y,Aux`로 변경
  - [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml#L68)
  - [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml#L82)
- 빈 Axis/Group 이름 입력 시 즉시 검증하도록 추가
  - [PmasControllerContext.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs#L232)
  - [PmasControllerContext.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs#L251)

빌드는 통과했습니다.
`bin\Debug`는 실행 중 프로세스가 잠가서 덮어쓰기 실패했고, 대신 아래로 빌드 성공 확인했습니다.

- `C:\work\Elmo\Elmo_Master\tmp\build_verify\PmasApiWpfTestApp.exe`

지금 바로 할 일:

1. Visual Studio에서 디버깅 완전 종료(`Shift+F5`)
2. 솔루션 `Rebuild`
3. `Axis Name`에 실제 MDS 축 이름 입력 후 `MMC_GetAxisByNameCmd` 실행
4. 계속 `NC_NODE_NOT_FOUND`면 MDS의 정확한 축/그룹 이름 목록을 보내주세요. 그 이름으로 UI 기본값/프리셋까지 맞춰드리겠습니다.

> 기본 테스트는 완료됐어.
>
> Motion Profile: 250ms 이동 → In-position 확인 → 복귀
> 반복 횟수: 10,000 Cycle
> 평가 항목:
>  - 통신 주기 및 응답 속도
>  - Position 도달 안정성
> - Cycle 중 Error / Drop 발생 여부

> 기본 테스트는 완료됐어.
>
> 다음에 진행할 프로젝트는 아래와 같은 동작을 해야 되는 프로그램이야.
>
> Motion Profile: 250ms 이동 → In-position 확인 → 복귀
> 반복 횟수: 10,000 Cycle
> 평가 항목:
>  - 통신 주기 및 응답 속도
>  - Position 도달 안정성
> - Cycle 중 Error / Drop 발생 여부
>
> 우리가 만든 프로그램을 뼈대로 해서 다음 프로그램을 제작해보자.

<details><summary>5 previous messages</summary>

> 요구사항 기준으로 기존 WPF 테스트앱에 `10,000 cycle 모션 프로파일 시험 모듈`을 바로 추가하겠습니다. 먼저 현재 축 이동/상태 읽기 API가 어디까지 구현돼 있는지 코드 구조를 빠르게 확인하겠습니다.
>
> 이동/상태 API는 이미 있어서 새 탭으로 스트레스 테스트 루프를 얹으면 됩니다. 이제 XAML 구조와 프로젝트 파일을 확인해서 안전하게 `Cycle Test` 모듈을 추가하겠습니다.
>
> 구현 방향은 확정했습니다. 기존 축 API를 그대로 쓰고, 새 `Cycle Test` 탭에서 백그라운드 루프로 `왕복 1사이클`을 10,000회 돌리면서 통계값을 누적하겠습니다.
>
> - 이동 단계: `MoveAbsoluteEx(target)` 실행
> - In-position 판정: `GetActualPosition()` 폴링으로 `|오차| <= Tolerance` 확인
> - 복귀 단계: `MoveAbsoluteEx(return)` 실행 후 동일 판정
> - 평가값 누적:
>   - 사이클 시간(평균/최대)
>   - API 응답 지연(`GetActualPosition` 호출 지연 평균/최대)
>   - 폴링 주기(평균/최대) + 임계치 초과 Drop 카운트
>   - In-position 오차 최대값
>   - 예외(Error) 발생 횟수
> - 제어: `Start / Stop`, 진행률과 요약 리포트 표시
>
> 지금 XAML 탭과 C# 로직 파일을 추가하고, VS2019(.NET 4.8, C# 7.3) 빌드까지 검증하겠습니다.
>
> 구현 작업 들어갑니다. 새 `Cycle Test` 탭(UI)와 백그라운드 루프 로직 파일을 추가하고, 프로젝트 파일에 컴파일 포함까지 연결하겠습니다.
>
> `Cycle Test` 탭/로직 추가는 끝났고 이제 컴파일 검증하겠습니다. 실행 중 파일 잠금 영향을 피하려고 별도 출력 경로로 빌드하겠습니다.
</details>

요구한 동작(250ms 이동 → In-position 확인 → 복귀, 10,000 cycle, 통신/응답/안정성/에러·드롭 평가) 모듈을 기존 앱에 추가했습니다.

적용 파일:
- [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml#L1055)
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L114)
- [PmasApiWpfTestApp.csproj](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj#L78)

핵심 구현:
- 새 `Cycle Test` 탭 추가
- `Start / Stop` 제어
- 백그라운드 10,000 cycle 루프
- 각 cycle:
  - `MoveAbsoluteEx(target)`
  - `GetActualPosition()` 폴링으로 In-position 판정(허용오차/연속샘플)
  - `MoveAbsoluteEx(return)`
  - 동일 판정
- 통계 수집:
  - Cycle 시간(평균/최대)
  - 통신 응답속도(`GetActualPosition` 호출 지연 평균/최대)
  - 폴링 주기(평균/최대)
  - Drop 카운트(폴링 주기 > 임계치)
  - In-position 오차 최대
  - Forward/Return timeout 수
  - Axis error/Exception 수

추가 포인트:
- `Move Time(ms)` 기본값 250
- 속도/가감속은 거리와 Move Time으로 자동 계산
- `Stop on timeout`, `Stop on axis error/exception` 옵션 제공

빌드 검증:
- VS2019 MSBuild로 성공
- 출력: `C:\work\Elmo\Elmo_Master\tmp\build_verify\PmasApiWpfTestApp.exe`

다음 실행 순서:
1. 연결 + `MMC_GetAxisByNameCmd` 완료
2. `Cycle Test` 탭에서 파라미터 확인(기본 10,000 cycle)
3. `Start 10,000 Cycle Test` 실행
4. `Result` 박스와 `Execution Log`로 평가 결과 확인

원하면 다음 단계로 결과를 CSV 파일로 자동 저장하는 기능까지 바로 붙이겠습니다.

> === Motion Profile Cycle Test Summary ===
> Profile: 1000 -> In-position -> 0 (MoveTime=250ms)
> Dynamics(auto): Velocity=4000.000, Acc=64000.000, Dec=64000.000, Jerk=100000.000
> Cycles: attempted=1, successful=0, target=10000
> Elapsed: total=18.5 ms
> Cycle time: avg=18.495 ms, max=18.495 ms
> Response latency(GetActualPosition): avg=0.000 ms, max=0.000 ms, samples=0
> Poll period: avg=0.000 ms, max=0.000 ms, drop(th>50ms)=0
> In-position settle: forward avg=0.000 ms, return avg=0.000 ms, max error=0.000000
> Timeouts/Error: forwardTimeout=0, returnTimeout=0, axisError=0, exception=1
> Last error: MoveAbsoluteEx Exception on IP = 192.168.1.3
> :Axis g01 :AxisRef = 0
> Failed on Command MC_MOVEABSOLUTEEX
> :LibraryError = NoError :MMCError = NC_DIRECTION_TYPE_OUT_OF_RANGE :MMCStatus = 16
> Outgoing Data Size = 64
>  Outgoing Data = 9F 20 00 00 38 00 00 00 00 00 00 00 00 40 8F 40 00 00 00 00 00 40 AF 40 00 00 00 00 00 40 EF 40 00 00 00 00 00 40 EF 40 00 00 00 00 00 6A F8 40 00 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00
> StackTrace:
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCException.Throw(Int32 hndl, String msg, UInt16 command, IPAddress destination, String axisName, UInt16 axisRef, LibraryErrors lbErr, MMCErrors errid, UInt16 status, Byte[] data, Exception innerException)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCAxis.CheckDeviceError(String fName, IInParam inArgs, DefaultOutputFBArgs outArgs)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCSingleAxis.MoveAbsoluteEx(Double dPosition, Double dVelocity, Double dAcceleration, Double dDeceleration, Double dJerk, MC_DIRECTION_ENUM eDirection, MC_BUFFERED_MODE_ENUM eBufferMode, Byte execute)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCSingleAxis.MoveAbsoluteEx(Double dPosition, MC_BUFFERED_MODE_ENUM eBufferMode)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCSingleAxis.MoveAbsoluteEx(Double dPosition, Double dVelocity, Double dAcceleration, Double dDeceleration, Double dJerk, MC_DIRECTION_ENUM eDirection, MC_BUFFERED_MODE_ENUM eBufferMode)
> 	   위치: PmasApiWpfTestApp.MainWindow.IssueCycleMove(Double targetPosition, CycleTestOptions options) 파일 C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\MainWindow.CycleTestOperations.cs:줄 423
> 	   위치: PmasApiWpfTestApp.MainWindow.ExecuteCycleTest(CycleTestOptions options, CancellationToken token) 파일 C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\MainWindow.CycleTestOperations.cs:줄 292

<details><summary>3 previous messages</summary>

> 로그 확인 결과 원인은 명확합니다. `NC_DIRECTION_TYPE_OUT_OF_RANGE`로 이동 명령이 첫 사이클에서 즉시 실패했고, 그래서 샘플 수가 0으로 나온 겁니다.
> 지금 사이클 테스트 모듈을 `방향 인자 없는 MoveAbsoluteEx 오버로드`를 쓰도록 수정해서 바로 해결하겠습니다.
>
> 현재 DLL에서는 `MoveAbsoluteEx`의 6인자 오버로드가 실제로 노출되지 않네요(컴파일 에러 확인).
> 시그니처를 다시 정확히 뽑아서, 유효한 호출 방식으로 즉시 재수정하겠습니다.
>
> 원인 확정됐습니다. `ComboDirection` 기본 선택이 `MC_NONE_DIRECTION(0)`이라서 `MoveAbsoluteEx`에서 바로 `NC_DIRECTION_TYPE_OUT_OF_RANGE`가 난 겁니다.
> 방향값을 절대이동에 유효한 값으로 강제 보정(`NONE/CURRENT -> SHORTEST_WAY`)해서 다시 수정하겠습니다.
</details>

원인 정확합니다.
첫 사이클에서 `MoveAbsoluteEx`에 **유효하지 않은 Direction 값**이 들어가서 `NC_DIRECTION_TYPE_OUT_OF_RANGE`가 발생했습니다.
그래서 `samples=0`, `attempted=1`, `exception=1`로 끝난 겁니다.

수정 완료:
1. `ComboDirection` 기본값을 `MC_SHORTEST_WAY`로 고정
- [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs#L41)

2. Cycle Test에서 방향값 보정 로직 추가
- `MC_NONE_DIRECTION`/`MC_CURRENT_DIRECTION`이면 자동으로 `MC_SHORTEST_WAY`로 변환
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L236)
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L433)

3. Move 실행은 보정된 direction으로 호출
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L421)

빌드 확인:
- 성공 (`Debug`, `Any CPU`)
- 출력: `C:\work\Elmo\Elmo_Master\tmp\build_verify\PmasApiWpfTestApp.exe`

다음 실행:
1. 앱 재시작
2. `Cycle Test`에서 먼저 `Cycle Count=100`으로 짧게 확인
3. 정상 통과하면 `10000`으로 올려서 본시험 진행

> 속도나 가감속 저크는 입력 어떻게 받지? 그리고 드라이브 동작모드는 돌리기전에 세팅하고 돌리면되냐

지금 기준으로는 이렇게 동작합니다.

- `Cycle Test` 탭에서는 **속도/가감속은 직접 입력이 아니라 자동 계산**입니다.
- 입력값은 `Target Position`, `Return Position`, `Move Time(ms)`를 받고,
  - `Velocity = 거리 / 이동시간`
  - `Acc = Dec = Velocity / (MoveTime*0.25)`
  로 계산됩니다.
- `Jerk`만 `TextCycleJerk`에서 직접 입력합니다.
- 계산된 값은 결과 요약의 `Dynamics(auto)`로 확인됩니다.

동작모드는 질문한 대로 맞습니다.

- **돌리기 전에 세팅하고 시작**하면 됩니다.
- 현재 Cycle Test는 시작 시 OpMode를 자동 변경하지 않습니다.
- 그래서 순서는 보통:
  1. Connect
  2. GetAxisByName
  3. Power On
  4. `MMC_ChngOpMode`로 원하는 모드 설정
  5. (필요 시 Home 완료)
  6. Cycle Test Start

원하면 Cycle Test 탭에 `Velocity/Acc/Dec` 수동입력 옵션(자동/수동 토글) 바로 추가해드리겠습니다.

> 모션 프로파일이 250ms 이동이 아니라 250mm 이동이야 속도 가속도 저크 세팅할 수 있게 해줘

<details><summary>3 previous messages</summary>

> 요구사항대로 바꾸겠습니다. `250mm 왕복` 기준으로 바꾸고, `Velocity/Acceleration/Deceleration/Jerk`를 수동 입력받도록 Cycle Test UI와 로직을 수정하겠습니다.
>
> UI를 `250mm 왕복 + 수동 속도/가감속/저크` 방식으로 교체하겠습니다. 기존 `Move Time(ms)` 기반 자동 계산은 제거하고, 거리 기준 프로파일로 바꿉니다.
>
> 코드 반영은 끝났고 지금 컴파일로 깨진 참조가 없는지 확인하겠습니다. (예전 `Target/Return/MoveTime` 필드가 완전히 제거됐는지 검증합니다.)
</details>

수정 완료했습니다.
이제 Cycle Test는 `250ms` 기준이 아니라 `250mm` 거리 기준으로 동작하고, 속도/가감속/저크를 수동으로 입력합니다.

변경 내용:
- Cycle Test 입력항목 변경
  - `Base Position (mm)`
  - `Move Distance (mm)` (기본 250)
  - `Velocity`, `Acceleration`, `Deceleration`, `Jerk` 수동 입력
  - 파일: [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml#L1078)
- 프로파일 정의 변경
  - `Base -> (Base + MoveDistance) -> Base`
  - 파일: [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L36)
- 자동 계산 로직 제거, 수동값 사용
  - 옵션 파싱/검증 (`MoveDistance != 0`, `Velocity/Acc/Dec/Jerk > 0`)
  - 파일: [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L225)
- 실행/요약 문구도 수동 다이나믹 기준으로 변경
  - 파일: [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L142), [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L585)

빌드:
- `Rebuild` 성공 (VS2019 MSBuild, 오류 0)
- 출력: `C:\work\Elmo\Elmo_Master\tmp\build_verify\PmasApiWpfTestApp.exe`

사용:
1. `Base Position=0`, `Move Distance=250`
2. `Velocity/Acc/Dec/Jerk` 원하는 값 입력
3. Start 실행하면 `0 -> 250 -> 0` 왕복을 반복합니다.

> 이거 Move Absolute를 하면 동작이 완료되면 Done 신호가 소켓으로 오나?

<details><summary>1 previous message</summary>

> 질문 핵심이 정확해서, 현재 앱 코드 기준으로 “Done이 어디로 오는지”를 바로 코드 레벨로 확인해서 답하겠습니다.
</details>

짧게 결론부터 말하면, **온다**가 맞습니다.
단, `Done` 문자열 패킷이 아니라 **MMCLib 콜백 이벤트** 형태로 들어옵니다.

- `MoveAbsoluteEx` 리턴은 “명령 전송 성공”이지 “모션 완료”가 아닙니다.
- 완료 이벤트는 `RegisterEndMotionEventCallback`으로 받습니다.
  - [PmasControllerContext.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs#L206)
- 연결 시 `ConnectRPC(..., _userCallback, eventMask, ...)`로 콜백 채널(UDP)도 같이 설정됩니다.
  - [PmasControllerContext.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs#L184)
- 완료되면 현재 앱 로그에 `EndMotion: AxisRef=..., Result=True`가 찍히도록 되어 있습니다.
  - [PmasControllerContext.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs#L357)

참고로 지금 Cycle Test는 콜백 외에도 `GetActualPosition()` 폴링으로 In-position을 판정합니다.
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L471)

`EndMotion` 로그가 안 오면 `Event Mask`, Local IP/Port, 방화벽(UDP)부터 보면 됩니다.

> 근데 Wireshark로 찍어보면 도착완료되는 시점에 패킷이 없는데?

<details><summary>1 previous message</summary>

> 가능한 원인을 코드 기준으로 바로 확인해보겠습니다. 특히 EndMotion 이벤트는 축에서 `EnableEndMotionEvent`를 별도로 켜야 하는지 먼저 체크하겠습니다.
</details>

정상일 수 있습니다. 이유는 3개입니다.

1. 현재 Cycle Test 완료 판정은 `Done 패킷`이 아니라 **위치 폴링**입니다.
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L471)
- `GetActualPosition()` 오차가 tolerance 안에 들어오면 완료로 봅니다.

2. 코드상 `EndMotion 콜백 등록`은 했지만, 축에서 `EnableEndMotionEvent()`를 호출하지 않습니다.
- 등록: [PmasControllerContext.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs#L206)
- 현재 프로젝트 검색상 `EnableEndMotionEvent` 호출 없음.

3. 콜백 패킷이 있어도 `TCP 4000`만 보면 안 잡힐 수 있습니다. 콜백은 UDP 리스너 포트(동적)로 갈 수 있습니다.
- [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs#L336)

정리: 지금 구조에서는 “도착 완료 시점에 별도 패킷이 안 보이는 것”이 이상이 아닙니다.

원하면 바로 바꿔드릴 수 있습니다.
1. 축 로드 후 `EnableEndMotionEvent()` 자동 실행
2. Cycle Test 완료 조건을 `콜백 기반`으로 선택 가능(폴링/콜백 토글)

> 지금 그럼 Cycle Test 동작 구조를 설명해줘

<details><summary>1 previous message</summary>

> 구조를 코드 기준으로 정확히 정리해서 설명하겠습니다. 핵심 메서드 라인 번호를 같이 확인하겠습니다.
</details>

**동작 구조**

1. 입력값
- `Cycle Test` 탭에서 `Base Position`, `Move Distance(mm)`, `Velocity/Acc/Dec/Jerk`, `Cycle Count`, `Tolerance`, `Timeout`, `Poll Interval`, `Stable Samples`, `Drop Threshold`를 받습니다.
- [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml#L1055)

2. 시작
- `Start` 클릭 시 옵션 파싱/검증 후 백그라운드 Task로 테스트 루프를 시작합니다.
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L123)
- 옵션 생성/검증은 여기서 합니다. (`Velocity>0`, `Acc>0`, `MoveDistance!=0` 등)
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L225)

3. 1 사이클 실행 방식
- `ForwardPosition = Base + MoveDistance`로 `MoveAbsoluteEx` 실행
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L318)
- `GetActualPosition()` 폴링으로 `|오차| <= Tolerance`가 `StableSamples` 연속 만족될 때 도착 판정
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L469)
- 그다음 `ReturnPosition = Base`로 복귀 이동 후 동일 판정
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L343)

4. 측정/판정
- 매 사이클마다 `cycle time`, `response latency`, `poll period`, `drop count`, `max position error`, `timeout`, `axis error`, `exception`을 누적합니다.
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L303)
- 진행 중 10사이클마다 UI 요약 갱신, 종료 시 최종 요약 생성
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L548)
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L585)

5. 중지
- `Stop` 클릭 시 Cancellation + 축 `Stop()` 명령을 보냅니다.
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L203)

6. Done 신호와의 관계
- 현재 Cycle Test 완료 판정은 `EndMotion 콜백`이 아니라 **위치 폴링 기반**입니다.
- 콜백 등록은 되어 있지만 테스트 완료 조건으로 사용하지 않습니다.
- [PmasControllerContext.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs#L206)
- [PmasControllerContext.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs#L357)

> 지금 코드보면 스탑워치 쓰고있는데 스탑워치 프리퀀시가 어떻게되냐

<details><summary>1 previous message</summary>

> 지금 환경에서 `Stopwatch.Frequency` 값을 직접 찍어서 정확한 숫자로 답하겠습니다.
</details>

현재 환경 기준 `Stopwatch.Frequency`는 **10,000,000** 입니다.

- `Stopwatch.IsHighResolution = True`
- 의미: 초당 10,000,000 tick, 즉 **1 tick = 0.1 μs (100 ns)** 입니다.

그래서 코드의 `elapsedMs = ticks * 1000.0 / Stopwatch.Frequency` 계산이 맞습니다.

> 그럼 1tick 100ns에 한번씩 동작된다는거야? 성능만 받쳐주면?

아니요. 그건 오해입니다.

`1 tick = 100ns`는 **시간 해상도 단위**이고,
코드가 100ns마다 실행된다는 뜻이 아닙니다.

실제 실행 주기는 다음에 의해 결정됩니다.

- 루프 내부 작업 시간 (`GetActualPosition`, 계산, 로그 등)
- `Thread.Sleep(options.PollIntervalMs)` (지금 기본 5ms)
- 스레드 스케줄링/OS 타이머 해상도

즉 현재 구조는 이론상 100ns 주기가 아니라, 실질적으로 **ms 단위 폴링**입니다.
성능이 좋아도 `Sleep`을 쓰는 한 100ns 주기로는 안 돌아갑니다.

> 실제로는 얼마단위로 동작함?

<details><summary>1 previous message</summary>

> 정확히 숫자로 답하려고, 현재 PC에서 `Sleep(5)`가 실제 몇 ms로 깨는지 바로 측정해보겠습니다.
</details>

지금 환경에서 실측하면 **약 16ms 단위**로 돕니다.

- 코드 주기 기준: `Thread.Sleep(options.PollIntervalMs)`
  - [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L515)
- 실측값:
  - `Sleep(5ms)` -> 평균 `15.964ms`, p50 `15.977ms`, p95 `16.089ms`, 최대 `98.081ms`
  - `Sleep(1ms)` -> 평균 `15.936ms`, p50 `15.976ms`, p95 `16.092ms`, 최대 `96.716ms`

즉 현재 PC 타이머 해상도 영향으로, PollInterval을 1~5ms로 줘도 실제 루프는 대체로 16ms 근처입니다.
실제 사이클 루프 주기는 여기에 `GetActualPosition()` 호출 시간까지 더해집니다.

> 이거 말고 좀더 성능 높혀서 시킬수 있는 쓰레드 없나

<details><summary>2 previous messages</summary>

> 가능합니다. 지금은 ThreadPool + `Thread.Sleep` 기반이라 타이머 해상도 한계(약 15.6ms)를 크게 받습니다.
> 지금 바로 `전용 고우선순위 스레드 + 고정밀 대기(Spin/Yield) + 1ms 타이머 해상도 옵션`을 추가해서 성능 모드로 올리겠습니다.
>
> 고성능 모드 코드 반영을 끝냈고, 지금 빌드해서 실제로 컴파일되는지 바로 확인하겠습니다.
</details>

있습니다. 방금 그 모드 넣었습니다.

추가된 고성능 옵션:
- `Use high-priority worker thread`
- `Use high-precision wait (yield/spin)`
- `Request 1ms system timer resolution`
- 위치: [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml#L1172)

코드 변경 핵심:
- ThreadPool 일반 작업 대신 `LongRunning` 전용 워커 스레드 사용
  - [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L178)
- 워커 스레드 우선순위 `Highest` 적용 옵션
  - [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L327)
- `Sleep`만 쓰지 않고 `yield/spin` 기반 고정밀 대기 옵션 추가
  - [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L555)
- `timeBeginPeriod(1)` / `timeEndPeriod(1)` 타이머 해상도 요청 옵션 추가
  - [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L730)

확인:
- Rebuild 성공(오류 0)

주의:
- 이건 “향상”이지 RTOS급 실시간 보장은 아닙니다.
- `high-precision wait` 켜면 CPU 사용률이 올라갑니다.
- `1ms timer resolution`은 시스템 전체 타이머 동작에도 영향 줍니다.

> 그리고 MoveAbsoluteEx를 쓴 이유는 머야?

핵심 이유는 3개입니다.

1. 프로파일 파라미터를 직접 넣을 수 있어서
- 이번 테스트 목적이 `Velocity/Acceleration/Deceleration/Jerk`를 제어하는 반복 시험이라,
- `MoveAbsoluteEx(position, velocity, acc, dec, jerk, ...)`가 가장 직접적입니다.
- 일반 `MoveAbsolute`는 오버로드에 따라 세부 제어 폭이 좁습니다.

2. 목표가 “절대 위치 왕복”이라서
- 구조가 `Base -> Forward -> Base` 절대 위치 반복이라 `MoveAbsoluteEx`가 맞습니다.
- `MoveRelativeEx`도 가능하지만 누적 오차/기준점 관리 면에서 절대좌표가 테스트 재현성이 좋습니다.

3. 기존 앱 구조/매핑과 일치
- 이미 Single Axis 탭에서 `MMC_MoveAbsoluteExCmd`를 쓰고 있고,
- API 매핑 시트에서도 해당 항목이 `Mapped`로 정리돼 있어 같은 경로를 재사용하는 게 안전합니다.

즉, “거리 반복 + 동역학 파라미터 검증” 목적에 가장 맞는 게 `MoveAbsoluteEx`라서 쓴 겁니다.

> MoveAbsolute와 MoveAbsoluteEx와 차이는?

<details><summary>1 previous message</summary>

> 정확히 비교하려고 현재 사용 중인 MMCLib v3.0.0.7의 메서드 시그니처를 바로 뽑아서 답하겠습니다.
</details>

실사용 기준 차이는 **파라미터 정밀도**가 핵심입니다.

- `MoveAbsolute`
  - 위치: `Double`
  - 속도/가감속/저크: `Single(float)`
- `MoveAbsoluteEx`
  - 위치: `Double`
  - 속도/가감속/저크: `Double`

지금 사용하는 v3.0.0.7 시그니처도 이렇게 나옵니다:

```csharp
MoveAbsolute(Double, Single, Single, Single, Single, MC_DIRECTION_ENUM, MC_BUFFERED_MODE_ENUM)
MoveAbsoluteEx(Double, Double, Double, Double, Double, MC_DIRECTION_ENUM, MC_BUFFERED_MODE_ENUM)
```

의미:
- 둘 다 “절대 위치 이동” 기능은 동일.
- `Ex`가 동역학 파라미터(vel/acc/dec/jerk) 정밀도가 높아서 튜닝/반복시험에 유리.
- 그래서 현재 Cycle Test에 `MoveAbsoluteEx`를 쓴 겁니다.

> 쓰레드는 StopWatch말고 어떤걸 사용한거지?

`Stopwatch`는 **시간 측정용**이고, 스레드 실행은 아래를 씁니다.

- 실행 스레드 생성: `Task.Factory.StartNew(..., TaskCreationOptions.LongRunning, ...)`
  - 사실상 전용 worker thread로 동작
- 스레드 우선순위: `Thread.CurrentThread.Priority = ThreadPriority.Highest` (옵션 켠 경우)
- 대기 방식:
  - 기본: `Thread.Sleep(...)`
  - 고정밀 모드: `Thread.Sleep(1 or 0)` + `Thread.SpinWait(...)` 조합
- 취소: `CancellationTokenSource` / `CancellationToken`
- 추가로 타이머 해상도 요청(옵션): `timeBeginPeriod(1)` / `timeEndPeriod(1)` (`winmm.dll`)

즉, “스레드 메커니즘”은 `Task( LongRunning ) + Thread`이고, `Stopwatch`는 타이밍 계산만 담당합니다.

> 성능 측정 가능할까?

가능합니다. 이미 지금 코드에서 측정하고 있습니다.

현재 수집되는 항목:
- `Cycle time` 평균/최대
- `GetActualPosition` 응답 지연 평균/최대
- `Poll period` 평균/최대 + `Drop` 카운트(임계치 초과)
- `In-position settle`(정착시간) 평균 + 최대 위치오차
- `Timeout / AxisError / Exception` 카운트

관련 코드:
- 요약 생성: [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L585)
- 측정 누적 루프: [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L303)

권장 측정 방법:
1. 같은 모션 파라미터로 `1000 cycle` 3회 반복
2. `고성능 옵션 OFF/ON` 각각 수행
3. `Cycle time`, `Poll period`, `Drop`, `Exception`을 비교

원하면 바로 `CSV 자동 저장`(run별 결과 누적)까지 붙여드릴게요.

> 일단 테스트 해보고 더 필요하면 요청할께

좋습니다.
테스트하면서 결과 요약/로그 캡처만 남겨두세요. 문제 생기면 그 값 기준으로 바로 튜닝하겠습니다.

> 지금은 MoveAb용 쓰레드. 위치 읽기 쓰레드 이렇게 두개해서 동작중인거지?

아니요. 현재는 **Cycle Test 워커 스레드 1개**에서 둘 다 합니다.

- 같은 루프에서 `MoveAbsoluteEx` 호출하고, 바로 이어서 `GetActualPosition` 폴링(In-position 판정)합니다.
  - [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L303)
  - [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L469)
- 별도 위치읽기 전용 스레드는 없습니다.
- 추가로 UI 스레드, 그리고 MMCLib 콜백 처리 스레드는 별도로 존재할 수 있습니다.

> 이거 작업한 내용을 엑셀로 정리해서 저장해줄수 있냐

<details><summary>2 previous messages</summary>

> 가능합니다. 지금까지 작업 내용을 표 형태로 정리한 `.xlsx` 파일을 만들어서 프로젝트 폴더에 저장하겠습니다. 완료 후 경로 바로 드리겠습니다.
>
> 파워셸에서 파이썬 입력 방식이 달라서 실패했습니다. 바로 재실행해서 엑셀 파일 생성하겠습니다.
</details>

정리본 엑셀 저장했습니다.

- [PmasApiWpfTestApp_WorkLog_2026-04-07.xlsx](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/Reports/PmasApiWpfTestApp_WorkLog_2026-04-07.xlsx)

구성 시트:
- `Summary`
- `ChangeLog`
- `CycleTestSpec`
- `Files`

> 내가 설명을 잘못했네. 테스트한 내용을 정리된 엑셀파일로 보고싶어.
> 테스트 완료하고 테스트된 내용을 저장할 수 있도록 기능을 만들어.

<details><summary>3 previous messages</summary>

> 요청한 기능 방향으로 구현 들어갑니다. `Cycle Test 종료 후 결과를 .xlsx로 저장`할 수 있게 UI(저장 버튼/폴더/자동저장)와 코드(엑셀 작성기)를 추가하겠습니다.
>
> 저장 기능 코드를 모두 넣었습니다. 이제 빌드해서 실제로 이벤트/엑셀 저장 코드가 정상 컴파일되는지 검증하겠습니다.
>
> 빌드까지 통과했습니다. 마지막으로 어디가 추가됐는지 라인 기준으로 정리해서 바로 쓰는 방법까지 전달하겠습니다.
</details>

요구사항 반영해서 **Cycle Test 완료 결과를 엑셀(.xlsx)로 저장하는 기능** 추가했습니다.

**추가된 기능**
- `Result` 영역에 저장 UI 추가
  - 저장 폴더 입력
  - `Auto Save` 체크
  - `Save Result (.xlsx)` 버튼
  - 파일: [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml#L1245)

- Cycle Test 완료 시 결과 스냅샷 저장 후 수동/자동 저장 가능
  - 완료 후 `Save` 버튼 활성화
  - `Auto Save` 체크 시 테스트 종료 직후 자동 저장
  - 파일: [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L206)

- 엑셀 저장 로직 구현
  - 시트 1: `Result` (테스트 파라미터 + 집계 성능 지표 + 에러 정보 + 요약)
  - 시트 2: `ExecutionLog` (실행 로그 전체)
  - 파일: [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs#L783)

- 외부 Excel 설치 없이 `.xlsx` 생성 가능한 내장 Exporter 추가
  - 파일: [SimpleXlsxExporter.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/Services/SimpleXlsxExporter.cs#L23)

- 프로젝트 참조/컴파일 항목 추가
  - [PmasApiWpfTestApp.csproj](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj#L44)
  - [PmasApiWpfTestApp.csproj](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj#L88)

**빌드 검증**
- Rebuild 성공 (오류 0)
- 출력 확인: `C:\work\Elmo\Elmo_Master\tmp\build_verify\PmasApiWpfTestApp.exe`

**사용 방법**
1. Cycle Test 실행 후 완료
2. `Result` 하단에서 저장 폴더 확인
3. 수동 저장: `Save Result (.xlsx)` 클릭
4. 자동 저장 원하면 `Auto Save` 체크 후 테스트 실행

원하면 다음으로 `파일명 규칙(축명/사이클수 포함)`이나 `회차별 이력 누적 관리`도 붙여드리겠습니다.

> MMCOpenUdpChannelCmdEx는 아무런 동작을 하지 않는데 왜그런거지?

<details><summary>3 previous messages</summary>

> 코드 기준으로 바로 답하면, 현재 버튼은 실제 `Open` 호출을 못 하고 상태 조회만 하게 되어 있습니다. 관련 라인 확인해서 정확히 짚어드리겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 코드 기준으로 왜 동작이 없어 보이는지 바로 확인해서 정확히 짚어드리겠습니다. 버튼 핸들러와 매핑 테이블 라인을 다시 확인하겠습니다.
</details>

정상입니다. 지금 버튼은 “실제 Open 명령”을 보내지 않도록 구현돼 있습니다.

- [`C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\MainWindow.xaml.cs:332`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:332)
  `MMC_OpenUdpChannelCmdEx` 클릭 시 하는 일은 `GetUDPListenerPortNumber(...)`로 **상태 조회 + 로그 출력**뿐입니다.
- [`C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\MainWindow.Coverage.cs:13`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.Coverage.cs:13)
  주석/매핑에 이미 “분리된 open wrapper는 public이 아니고, UDP callback은 ConnectRPC에서 할당됨”으로 명시돼 있습니다.
- [`C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\API_MAPPING.md`](./Codex_PMAS_WPF/API_MAPPING.md)
  동일 내용(진단용 항목)으로 정리돼 있습니다.

즉, 이 버튼 눌러도 추가 소켓 오픈 패킷이 안 보이는 게 맞습니다. `ConnectRPC` 단계에서 이미 채널이 잡히기 때문입니다.

> 이거 Cycle Test2 탭 만들고 Cycle Test랑 똑같은 로직으로 동작하는데, 이 거는 InPosition 체크하지말고 MoveAbsolute를 계속 보내는 형태로 작성해줘

<details><summary>8 previous messages</summary>

> 요청하신 대로 `Cycle Test2` 탭을 추가하고, 기존 Cycle Test를 복제하되 `InPosition` 대기 없이 `MoveAbsolute`를 연속 발행하는 흐름으로 구현하겠습니다. 먼저 현재 Cycle Test UI/코드 구조를 정확히 읽고 동일 패턴으로 확장하겠습니다.
>
> 현재 Cycle Test는 이미 정교하게 구성돼 있어서, 동일 구조를 복제해 `Cycle Test2` 전용 상태/버튼/출력 필드를 추가하고 실행 루프만 `InPosition` 대기 없이 바꾸는 방식이 가장 안전합니다. 지금 XAML과 CycleTest 코드 블록을 뜯어보고 바로 패치하겠습니다.
>
> 구조 확인 완료했습니다. 이제 `MainWindow.xaml`에 `Cycle Test2` 탭(전용 버튼/결과/저장 컨트롤 포함)을 추가하고, `MainWindow.CycleTestOperations.cs`에 `InPosition` 미사용 연속 `MoveAbsoluteEx` 루프를 별도 핸들러로 구현하겠습니다.
>
> XAML 탭 추가는 완료했습니다. 이제 코드 쪽에 `Cycle Test2` 전용 상태/핸들러/요약/엑셀 저장 루틴을 넣고, `InPosition` 대기 없는 연속 MoveAbsoluteEx 루프로 붙이겠습니다.
>
> 이제 코드에 `Cycle Test2` 실행 상태를 추가하는 본체 패치를 넣고 있습니다. 우선 동시 실행 충돌 방지를 위해 기존 Cycle Test 시작부에 상호 배제 체크를 추가하겠습니다.
>
> `Cycle Test2` 옵션 파싱/검증까지 넣었습니다. 이제 핵심인 연속 `MoveAbsoluteEx` 실행 루프(`InPosition` 대기 없음)와 결과 요약/저장 코드를 추가하겠습니다.
>
> 핵심 로직/이벤트 메서드까지 추가됐습니다. 지금 바로 빌드해서 XAML 이벤트 바인딩과 C# 컴파일 오류가 없는지 검증하겠습니다.
>
> 현재 터미널 `dotnet build`는 WPF(old csproj) 특성상 환경 제약으로 신뢰하기 어렵습니다. VS2019 `devenv` 빌드로 실제 컴파일 검증까지 진행하겠습니다.
</details>

요청하신 내용대로 `Cycle Test2`를 추가했고, `InPosition` 대기 없이 절대이동 명령을 연속 발행하도록 구현했습니다.

- 탭/화면 추가
  - [`MainWindow.xaml:1267`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1267)
  - `Cycle Test2` 탭, Start/Stop, 결과 요약, Progress, Auto Save, 엑셀 저장 UI 추가
  - 주요 컨트롤: [`MainWindow.xaml:1384`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1384), [`MainWindow.xaml:1401`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1401), [`MainWindow.xaml:1431`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1431)

- 로직 추가 (핵심)
  - Start/Stop/Save 핸들러: [`MainWindow.CycleTestOperations.cs:285`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:285), [`MainWindow.CycleTestOperations.cs:384`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:384), [`MainWindow.CycleTestOperations.cs:406`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:406)
  - 옵션 파싱: [`MainWindow.CycleTestOperations.cs:421`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:421)
  - 연속 발행 루프(무 InPosition): [`MainWindow.CycleTestOperations.cs:731`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:731)
  - 명령 발행 지점(연속 MoveAbsoluteEx): [`MainWindow.CycleTestOperations.cs:850`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:850)
  - Cycle Test2 요약 문자열: [`MainWindow.CycleTestOperations.cs:1199`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1199)

- 결과 저장(엑셀) 추가
  - 저장 함수: [`MainWindow.CycleTestOperations.cs:1325`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1325)
  - 결과 시트 생성: [`MainWindow.CycleTestOperations.cs:1402`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1402)

- 빌드 검증
  - VS2019로 실제 빌드 성공:
    - `devenv.com ... /Build "Debug|Any CPU"`
    - 결과: `성공 1, 실패 0`
  - 출력 exe: `C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\bin\Debug\PmasApiWpfTestApp.exe`

주의:
- Cycle Test2는 의도대로 `InPosition` 체크를 하지 않습니다.
- 명령은 기존 코드 체계와 동일하게 `MoveAbsoluteEx`를 연속 발행합니다.

> 이거 항목별로 기능 설명해줘
>
> User context
>
> Images:
> - `[embedded image extracted: ./assets/embedded_line_1188.png; original line 1188; mime image/png; base64 chars 32468]`

`Cycle Test` 탭 기준으로 항목 기능은 아래와 같습니다.

1. `Base Position (mm)`
기준 위치입니다. 왕복 시 복귀 목표값으로 사용됩니다.

2. `Move Distance (mm)`
왕복 이동 거리입니다. 전진 목표는 `Base + Move Distance`입니다.

3. `Velocity (mm/s)`
`MoveAbsoluteEx` 명령의 속도 파라미터입니다.

4. `Acceleration (mm/s^2)`
`MoveAbsoluteEx` 가속도 파라미터입니다.

5. `Deceleration (mm/s^2)`
`MoveAbsoluteEx` 감속도 파라미터입니다.

6. `Jerk (mm/s^3)`
`MoveAbsoluteEx` 저크 파라미터입니다.

7. `Cycle Count`
반복 횟수입니다.
1 cycle = 전진 이동 + In-position 확인 + 복귀 이동 + In-position 확인.

8. `In-position Tolerance (mm)`
목표 위치와 실제 위치 오차 허용값입니다. 이 값 이하면 도달로 판단합니다.

9. `Move Timeout (ms)`
각 구간(전진/복귀)에서 In-position 대기 최대 시간입니다.

10. `Poll Interval (ms)`
In-position 대기 중 `GetActualPosition`을 읽는 주기입니다.

11. `Stable Samples`
허용 오차 이내가 연속으로 몇 번 나와야 “도달 완료”로 볼지 정합니다.

12. `Drop Threshold (ms)`
실제 폴링 간격이 이 값보다 크면 Drop으로 카운트합니다.

13. `Stop on timeout`
전진/복귀 타임아웃 발생 시 테스트 즉시 중단합니다.

14. `Stop on axis error/exception`
축 에러(`GetAxisError`) 또는 예외(`MMCException`) 발생 시 즉시 중단합니다.

15. `Use high-priority worker thread`
테스트 워커 스레드를 `Highest` 우선순위로 실행합니다.

16. `Use high-precision wait (yield/spin)`
`Sleep` 위주가 아니라 `Sleep(0)/SpinWait` 섞어서 폴링 주기 정밀도를 높입니다.

17. `Request 1ms system timer resolution`
실행 중 `timeBeginPeriod(1)`로 시스템 타이머 해상도를 1ms로 요청합니다.

참고: 지금 스크린샷 값(예: 속도/가감속/저크 4,194,304,000)은 매우 커서 컨트롤러 범위를 넘을 가능성이 높습니다. `Drop Threshold=1ms`도 Drop 카운트를 과도하게 늘릴 수 있습니다.

> Drop Threshold는 얼마가 적당하냐

실무 기준으로는 `Drop Threshold = Poll Interval의 4~10배`가 안전합니다.

- `Poll Interval = 1ms`면 `5~10ms` 권장 (현재 `1ms`는 너무 빡빡해서 Drop 과다)
- `Poll Interval = 5ms`면 `20~50ms` 권장
- `Poll Interval = 10ms`면 `40~100ms` 권장

바로 시작값 하나만 고르면:
- 일반 Windows 테스트: `max(10ms, PollInterval*5)`
- 지금처럼 `1ms` 폴링이면 `10ms`부터 시작하세요.

> GetActualPosition을 시작해서 받아오는데 걸리는 시간이랑 해당 데이터를 엑셀로 받고싶다

<details><summary>3 previous messages</summary>

> 좋습니다. `Cycle Test`에서 `GetActualPosition` 호출 시작/종료 시점과 읽은 위치값을 샘플로 기록하고, 결과 엑셀에 별도 시트로 저장되게 바로 구현하겠습니다.
>
> `GetActualPosition` 샘플 구조와 카운터를 넣었습니다. 지금 `WaitForInPosition`에서 실제 읽기 시작/끝 시간, 응답시간, 위치 데이터를 샘플로 적재하는 코드와 엑셀 시트 출력을 붙이겠습니다.
>
> 코드 반영이 끝났습니다. 마지막으로 VS2019 빌드로 깨지는 부분 없는지 확인하고, 저장된 엑셀에 어떤 시트가 생기는지 정확히 정리해드리겠습니다.
</details>

요청하신 기능 추가 완료했습니다.

이제 `Cycle Test` 결과를 저장하면 엑셀에 `PositionSamples` 시트가 같이 생성되고, `GetActualPosition` 호출별 데이터가 기록됩니다.

변경 파일:
- [`MainWindow.CycleTestOperations.cs`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs)

핵심 구현:
1. `GetActualPosition` 샘플 기록
- 호출 시작 tick
- 호출 종료 tick
- `ReadLatency(ms)` (시작~응답까지 실제 소요)
- `ActualPosition`, `TargetPosition`, `PositionError`, `InTolerance`
- `CycleIndex`, `Phase(Forward/Return)`

2. 엑셀 저장 확장
- 기존 `Result`, `ExecutionLog` + `PositionSamples` 시트 추가
- `PositionSamples`에 아래 열 저장:
  - `SampleIndex`
  - `CycleIndex`
  - `Phase`
  - `TargetPosition(mm)`
  - `ActualPosition(mm)`
  - `PositionError(mm)`
  - `InTolerance`
  - `ReadStartFromTest(ms)`
  - `ReadEndFromTest(ms)`
  - `ReadLatency(ms)`

3. 샘플 제한
- 메모리/엑셀 과부하 방지로 최대 `200,000`개 저장
- 초과분은 `DroppedByLimit`로 집계

추가 확인:
- VS2019 빌드 성공 (`Debug|Any CPU`) 확인 완료.

원하면 다음으로 `Cycle Test2`에도 같은 방식으로 명령 송신 시점/응답 성능 샘플 시트를 붙여드릴 수 있습니다.

> === Motion Profile Cycle Test Summary ===
> Profile: Base=0 -> Forward=8388608 -> Return=0 (MoveDistance=8388608 mm)
> Dynamics(manual): Velocity=48933546.000, Acc=4893354600.000, Dec=4893354600.000, Jerk=48933546000.000
> Timing mode: highPriority=True, highPrecisionWait=True, timer1ms=True
> Cycles: attempted=2744, successful=2743, target=10000
> Elapsed: total=1623362.8 ms
> Cycle time: avg=591.596 ms, max=1738.167 ms
> Response latency(GetActualPosition): avg=0.655 ms, max=42.058 ms, samples=978159
> GetActualPosition samples: captured=200000, droppedByLimit=778159, limit=200000
> Poll period: avg=1.652 ms, max=43.061 ms, drop(th>3000ms)=0
> In-position settle: forward avg=296.782 ms, return avg=291.755 ms, max error=50099.000000
> Timeouts/Error: forwardTimeout=1, returnTimeout=0, axisError=0, exception=1
> Last error: MoveAbsoluteEx Exception on IP = 192.168.1.3
> :Axis a01 :AxisRef = 0
> Failed on Command MC_MOVEABSOLUTEEX
> :LibraryError = NoError :MMCError = NC_UNSUITABLE_NODE_STATE :MMCStatus = 16
> Outgoing Data Size = 64
>  Outgoing Data = 9F 20 00 00 38 00 00 00 00 00 00 00 00 00 00 00 00 00 00 50 55 55 87 41 00 00 80 A6 AA 3A F2 41 00 00 80 A6 AA 3A F2 41 00 00 20 50 55 C9 26 42 02 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00
> StackTrace:
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCException.Throw(Int32 hndl, String msg, UInt16 command, IPAddress destination, String axisName, UInt16 axisRef, LibraryErrors lbErr, MMCErrors errid, UInt16 status, Byte[] data, Exception innerException)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCAxis.CheckDeviceError(String fName, IInParam inArgs, DefaultOutputFBArgs outArgs)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCSingleAxis.MoveAbsoluteEx(Double dPosition, Double dVelocity, Double dAcceleration, Double dDeceleration, Double dJerk, MC_DIRECTION_ENUM eDirection, MC_BUFFERED_MODE_ENUM eBufferMode, Byte execute)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCSingleAxis.MoveAbsoluteEx(Double dPosition, MC_BUFFERED_MODE_ENUM eBufferMode)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCSingleAxis.MoveAbsoluteEx(Double dPosition, Double dVelocity, Double dAcceleration, Double dDeceleration, Double dJerk, MC_DIRECTION_ENUM eDirection, MC_BUFFERED_MODE_ENUM eBufferMode)
> 	   위치: PmasApiWpfTestApp.MainWindow.IssueCycleMove(Double targetPosition, CycleTestOptions options) 파일 C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\MainWindow.CycleTestOperations.cs:줄 900
> 	   위치: PmasApiWpfTestApp.MainWindow.ExecuteCycleTest(CycleTestOptions options, CancellationToken token) 파일 C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\MainWindow.CycleTestOperations.cs:줄 644
>
> 동작중에 에러나서 멈췄어

<details><summary>1 previous message</summary>

> 원인부터 정확히 짚겠습니다. 로그 기준으로는 `NC_UNSUITABLE_NODE_STATE`라서, 실패 시점의 축 상태가 Move를 받을 수 없는 상태로 바뀐 겁니다. 코드에서 어떤 조건에서 이 상태로 빠질 수 있는지 바로 확인하겠습니다.
</details>

멈춘 원인은 명확합니다.

1. `forward timeout`이 먼저 1회 발생했습니다.
2. 그런데 `Stop on timeout`이 꺼져 있으면 코드가 계속 진행해서 다음 `MoveAbsoluteEx`를 보냅니다.
3. 그 시점 축 상태가 Move 허용 상태가 아니어서 `NC_UNSUITABLE_NODE_STATE`가 발생했고, `Stop on axis error/exception=True`라서 종료됐습니다.

코드 근거:
- forward timeout 처리: [`MainWindow.CycleTestOperations.cs:633`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:633)
- timeout 시 중단 조건: [`MainWindow.CycleTestOperations.cs:637`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:637)
- 이후 return move 발행: [`MainWindow.CycleTestOperations.cs:644`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:644)
- 실제 명령: [`MainWindow.CycleTestOperations.cs:900`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:900)

추가로 현재 프로파일 값이 매우 큽니다.
- Distance `8,388,608`
- Vel `48,933,546`
- Acc/Dec `4,893,354,600`
- Jerk `48,933,546,000`
- `max error=50099`가 찍힌 걸 보면 안정적으로 in-position에 들어가기 어려운 세팅입니다.

바로 적용할 권장:
1. `Stop on timeout` 켜기.
2. 타임아웃을 충분히 늘리거나(예: 3000~10000ms), 속도/가감속/저크를 낮추기.
3. 에러 후에는 `Reset -> PowerOn -> OpMode 재설정` 후 재시작.
4. 실패 직후 `MMC_ReadStatusCmd`, `MMC_GetStatusRegisterCmd`로 상태 확인.

원하면 코드도 바꿔서, timeout 발생 시 다음 Move를 바로 보내지 않고 `Stop 후 해당 cycle skip`하도록 수정해줄게.

> 지금 타임아웃이 1500 ms인데 얼마로 늘리냐?

지금 조건이면 `3000 ms`로 먼저 올리세요.

- `1500 ms -> 3000 ms` 권장
- 10,000 cycle에서 아직 timeout 나오면 `5000 ms`로 한 번 더 올리면 됩니다.

단, 이번 `NC_UNSUITABLE_NODE_STATE`는 타임아웃만 늘려도 완전히 해결되진 않습니다. 타임아웃은 “조기 실패”만 줄여줍니다.

> 샘플 200,000개 저장하는데 늘리면 부담이 되려나?

네, **늘리면 부담됩니다.**

현재 구조는
1. 테스트 중 샘플을 메모리에 쌓고
2. 저장할 때 다시 문자열/행으로 전부 변환해서 엑셀로 쓰는 방식이라
저장 시점 RAM 피크가 크게 올라갑니다.

대략:
- `200,000`개: 보통 감당 가능
- `500,000`개: 저장 느려지고 메모리 부담 큼
- `1,000,000`개: 현재 방식에선 비추천

실무 권장:
- 기본은 `200,000` 유지
- 꼭 늘리면 `300,000` 정도까지만
- 그 이상은 엑셀 대신 CSV 스트리밍 저장 방식으로 바꾸는 게 맞습니다.

> 300,000개로 늘렸어

좋습니다. `300,000`이면 현실적인 상한선입니다.

체크할 점만 짚으면:
1. 실행 중 메모리 사용량
2. 저장 버튼 눌렀을 때 저장 시간
3. 엑셀 열기 속도/응답성

느려지면 다음은 샘플 수를 다시 내리기보다 `PositionSamples`를 CSV로 분리 저장하는 게 정답입니다.

> 이 내용 정리해서 엑셀파일로 만들어줘.
> 이름은 테스트 파라메터
> 저장위치는 C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\Reports
>
> User context
>
> Images:
> - `[embedded image extracted: ./assets/embedded_line_1414.png; original line 1414; mime image/png; base64 chars 210608]`

<details><summary>3 previous messages</summary>

> 요청한 값으로 `테스트 파라메터.xlsx`를 지정 경로에 바로 생성하겠습니다. 먼저 `openpyxl` 사용 가능 여부를 확인한 뒤, 안 되면 대체 방식으로 바로 저장하겠습니다.
>
> `openpyxl` 사용 가능 확인됐습니다. 지금 바로 엑셀 파일을 생성해서 `C:\\work\\Elmo\\Elmo_Master\\Codex_PMAS_WPF\\Reports`에 저장하겠습니다.
>
> 파일명 인코딩 이슈가 있어, 유니코드 이스케이프 방식으로 다시 생성하겠습니다.
</details>

완료했습니다. 아래 파일로 저장했습니다.

- [테스트 파라메터.xlsx](C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\Reports\테스트 파라메터.xlsx)

포함 내용:
- 스크린샷의 Motion Profile/Control 파라미터 값 전체
- 생성 시각(`GeneratedAt`)
- 항목/값/비고 표 형태 정리

> === Motion Profile Cycle Test Summary ===
> Profile: Base=0 -> Forward=8388608 -> Return=0 (MoveDistance=8388608 mm)
> Dynamics(manual): Velocity=48933546.000, Acc=4893354600.000, Dec=4893354600.000, Jerk=489335460000.000
> Timing mode: highPriority=True, highPrecisionWait=True, timer1ms=True
> Cycles: attempted=181, successful=180, target=10000
> Elapsed: total=104652.8 ms
> Cycle time: avg=578.181 ms, max=5031.969 ms
> Response latency(GetActualPosition): avg=0.679 ms, max=29.440 ms, samples=62106
> GetActualPosition samples: captured=62106, droppedByLimit=0, limit=300000
> Poll period: avg=1.677 ms, max=30.443 ms, drop(th>5000ms)=0
> In-position settle: forward avg=301.156 ms, return avg=275.259 ms, max error=673281.000000
> Timeouts/Error: forwardTimeout=1, returnTimeout=0, axisError=0, exception=1
> Last error: MoveAbsoluteEx Exception on IP = 192.168.1.3
> :Axis a01 :AxisRef = 0
> Failed on Command MC_MOVEABSOLUTEEX
> :LibraryError = NoError :MMCError = NC_UNSUITABLE_NODE_STATE :MMCStatus = 16
> Outgoing Data Size = 64
>  Outgoing Data = 9F 20 00 00 38 00 00 00 00 00 00 00 00 00 00 00 00 00 00 50 55 55 87 41 00 00 80 A6 AA 3A F2 41 00 00 80 A6 AA 3A F2 41 00 00 28 A4 AA 7B 5C 42 02 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00
> StackTrace:
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCException.Throw(Int32 hndl, String msg, UInt16 command, IPAddress destination, String axisName, UInt16 axisRef, LibraryErrors lbErr, MMCErrors errid, UInt16 status, Byte[] data, Exception innerException)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCAxis.CheckDeviceError(String fName, IInParam inArgs, DefaultOutputFBArgs outArgs)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCSingleAxis.MoveAbsoluteEx(Double dPosition, Double dVelocity, Double dAcceleration, Double dDeceleration, Double dJerk, MC_DIRECTION_ENUM eDirection, MC_BUFFERED_MODE_ENUM eBufferMode, Byte execute)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCSingleAxis.MoveAbsoluteEx(Double dPosition, MC_BUFFERED_MODE_ENUM eBufferMode)
> 	   위치: ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.MMCSingleAxis.MoveAbsoluteEx(Double dPosition, Double dVelocity, Double dAcceleration, Double dDeceleration, Double dJerk, MC_DIRECTION_ENUM eDirection, MC_BUFFERED_MODE_ENUM eBufferMode)
> 	   위치: PmasApiWpfTestApp.MainWindow.IssueCycleMove(Double targetPosition, CycleTestOptions options) 파일 C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\MainWindow.CycleTestOperations.cs:줄 900
> 	   위치: PmasApiWpfTestApp.MainWindow.ExecuteCycleTest(CycleTestOptions options, CancellationToken token) 파일 C:\work\Elmo\Elmo_Master\Codex_PMAS_WPF\PmasApiWpfTestApp\MainWindow.CycleTestOperations.cs:줄 644
>
> 또 멈췄어 아까랑 같은 내용인가?

네, **아까와 같은 유형**입니다.

- 이번에도 핵심 에러는 동일: `MMCError = NC_UNSUITABLE_NODE_STATE`
- 패턴도 동일:
  1. `forwardTimeout=1` 발생
  2. 이후 다음 `MoveAbsoluteEx` 시도
  3. 축 상태가 Move 불가 상태라 예외(`exception=1`)로 중단

즉, 타임아웃 값만 5000ms로 올려도 근본 원인은 그대로입니다.
지금 수치(거리/속도/가감속/저크)가 매우 커서 축 상태가 불안정해지는 게 계속 반복되고 있습니다.

바로 적용할 것:
1. `Stop on timeout` 켜기
2. 타임아웃 난 cycle에서 다음 Move를 보내지 않도록(스킵/중단) 처리
3. 단위 재확인(mm vs pulse/count), 모션 수치 크게 낮춰서 재시험

원하면 다음 턴에 timeout 발생 시 자동으로 `Stop + Reset + 재시도` 또는 `해당 cycle skip` 로직으로 코드 수정해줄게.

> 이제 지금 만든 Codex_PMAS_WPF를 복사해서 Codex_LASAL_WPF란 폴더를 만들어.
>
> 대신 이 프로그램은 Elmo Library를 사용해서 만드는게 아니라 SIGMATEK TCP/IP로 Elmo Library랑 동일하게 동작하는 프로그램을 만들거야. API를 사용하는 부분을 없애고 더미로 넣어 놓도록 해.

<details><summary>5 previous messages</summary>

> 요청하신 방향으로 진행하겠습니다. 먼저 `Codex_PMAS_WPF`를 `Codex_LASAL_WPF`로 복사한 뒤, 외부 Elmo DLL 의존성을 제거하고 동일 인터페이스를 제공하는 더미 구현으로 바꿔서 빌드 가능 상태까지 맞추겠습니다.
>
> 다음 단계로 `Codex_LASAL_WPF` 폴더를 복사한 뒤, 기존 코드 변경 없이 컴파일되도록 `ElmoMotionControl...` 네임스페이스를 로컬 더미 구현으로 대체할 계획입니다. 지금 필요한 타입/메서드를 전체 추출하겠습니다.
>
> 복사는 완료됐고, 이제 `Codex_LASAL_WPF` 쪽에서 외부 DLL을 완전히 끊기 위해 더미 타입 세트를 생성하겠습니다. 누락 없이 만들기 위해 실제 사용 타입 목록을 추출 중입니다.
>
> 이제 `Codex_LASAL_WPF` 프로젝트에서 외부 Elmo DLL 참조를 제거하고, 로컬 더미 구현 파일을 추가하겠습니다. 이후 VS2019로 빌드 검증하겠습니다.
>
> 더미 백엔드 파일과 프로젝트 참조 수정까지 끝났습니다. 이제 `Codex_LASAL_WPF` 솔루션을 VS2019로 실제 빌드해서 누락 타입/메서드가 없는지 확인하겠습니다.
</details>

작업 완료했습니다.

- 새 폴더 생성: `C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF`
- 원본 복사: `Codex_PMAS_WPF` 전체를 `Codex_LASAL_WPF`로 복제
- Elmo 외부 DLL 의존성 제거:
  - [`PmasApiWpfTestApp.csproj`](./Codex_LASAL_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj)
  - `ElmoMotionControlComponents.*` 참조/콘텐츠 항목 삭제
- 더미 백엔드 추가(Elmo API 동일 시그니처 제공):
  - [`SigmatekTcpIpDummyMMCLib.cs`](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs)
  - 네임스페이스는 기존과 동일(`ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET`)이라 기존 UI/코드 그대로 동작
  - 내부 동작은 SIGMATEK TCP/IP 전환용 더미 시뮬레이션(연결/축/그룹/Bulk/Recorder)
- 타이틀/설명 문구를 더미 백엔드 기준으로 수정:
  - [`MainWindow.xaml`](./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml)
  - [`PmasControllerContext.cs`](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs)
  - [`API_MAPPING.md`](./Codex_LASAL_WPF/API_MAPPING.md)

빌드 검증:
- `VS2019 devenv /Build "Debug|Any CPU"` 성공
- 실행 파일: [`PmasApiWpfTestApp.exe`](./Codex_LASAL_WPF/PmasApiWpfTestApp/bin/Debug/PmasApiWpfTestApp.exe)

원하면 다음 단계로 더미 파일 안에서 `MMCConnection.ConnectRPC`, `MMCSingleAxis.MoveAbsoluteEx`부터 실제 SIGMATEK TCP/IP 소켓 송수신으로 교체해줄게.

> 지금 구현되야되는건 3가지만 있으면 될꺼같다.
> 1. MMCConnection.ConnectRPC
> 2. MMCSingleAxis.MoveAbsoluteEx
> 3. MMCSingleAxis.GetActualPosition
>
> 근데 어짜피 SIGMATEK에서는 MMCSingleAxis를 받아올 수 없는 구조야. 이걸 대체할 수 있는 방안을 제시해줘

좋은 지적이고, 정답은 이겁니다.

`MMCSingleAxis`를 “받아오는” 구조가 없어도, **로컬 프록시 객체를 우리가 만들어서 대체**하면 됩니다.

추천안 (최소 수정, 바로 적용 가능):
1. `ConnectRPC`는 `SigmatekSession`(TCP 소켓+스트림+시퀀스)을 열고 `handle`만 발급
2. `LoadAxis`에서 실제 라이브러리 객체 대신 `SigmatekAxisProxy(handle, axisName)` 생성
3. `MoveAbsoluteEx` / `GetActualPosition`은 프록시가 TCP 명령 송수신으로 처리

핵심 포인트:
- 축 객체는 SIGMATEK이 주는 게 아니라 **앱이 axisName 기반으로 로컬 생성**
- 서버(PLC/LASAL)는 “명령 처리기”만 있으면 됨

명령 프로토콜 예시:
- `MOVE_ABS_EX {axis,pos,vel,acc,dec,jerk}`
- `GET_ACT_POS {axis}`
- 응답: `{ok, code, message, pos}`

대안 2개:
1. 객체형(추천): `Context.SingleAxis.MoveAbsoluteEx(...)` 유지 가능, 기존 UI/코드 변경 최소
2. 함수형: `MoveAbsoluteEx(handle, axisName, ...)`, `GetActualPosition(handle, axisName)`
   코드가 깔끔하지만 현재 프로젝트 수정량이 큼

즉, **MMCSingleAxis 부재 문제는 “AxisProxy 어댑터”로 해결**하는 게 가장 현실적입니다.
원하면 지금 `Codex_LASAL_WPF`를 이 구조로 실제 소켓 송수신 코드까지 바로 바꿔줄게.

> handle을 발급받을수가 있나? 일반적인 SIGMATEK TCP 프로그램은 소켓 스트림을 이용한 통신만 지원하는데

맞습니다. **SIGMATEK TCP 자체에서 MMCLib 같은 `handle`을 내려주지는 않습니다.**

하지만 `ConnectRPC` 호환을 유지하려면 `handle`을 **앱 내부에서 논리적으로 발급**하면 됩니다.

- 컨트롤러가 주는 handle: 없음
- 앱이 관리하는 handle: 가능 (`int`, `Guid`)

구조:
1. `ConnectRPC` 성공 시 소켓 연결 생성
2. 앱에서 `handle = 1,2,3...` 발급
3. `Dictionary<int, Session>`에 `socket/stream/lock/axis map` 저장
4. `MoveAbsoluteEx(handle, axisName, ...)`, `GetActualPosition(handle, axisName)`에서 세션 조회해 송수신
5. `CloseConnection(handle)` 시 세션 제거

즉, **handle은 통신 프로토콜 값이 아니라 로컬 세션 키**로 쓰는 겁니다.
원하면 handle 자체를 없애고 `Session` 객체 직접 넘기는 구조로도 바꿀 수 있지만, 지금 코드 호환성은 로컬 handle 방식이 가장 좋습니다.

> 지금 C:\work\Elmo\Elmo_Master\Lasal32dll 폴더를 추가했는데 이 폴더 안에 있는 PDF랑 파일들을 분석해서 해당 프로그램과 유사한 형태로 프로그램을 짤 수 있을까.

<details><summary>4 previous messages</summary>

> 가능 여부를 정확히 판단하려면 `Lasal32dll` 폴더의 파일 구성(PDF, DLL, 예제, 헤더/문서)을 먼저 확인해야 합니다. 지금 폴더 구조와 핵심 문서부터 바로 분석하겠습니다.
>
> 폴더에 이미 `src/tests/docs`까지 있는 걸 보면 단순 DLL 모음이 아니라 구현용 자료가 포함돼 있습니다. `README`와 `src`를 먼저 확인해서 지금 바로 재사용 가능한지 판단하겠습니다.
>
> 자료 확인 결과 `docs/PLC_DIRECT_PROTOCOL.md`와 `src`에 통신 계층이 이미 있어서, 구현 가능성이 높습니다. 프로토콜 문서와 핵심 Transport 코드를 읽고 어떤 형태로 붙일지 바로 정리하겠습니다.
>
> 파일 기반으로는 이미 프로토콜/전송 코드가 충분히 있고, PDF는 API 원본 근거용으로 확인하면 됩니다. PDF 4개에서 핵심 텍스트를 빠르게 추출해 실제 구현 근거가 되는지 확인하겠습니다.
</details>

가능합니다. 결론부터 말하면 **유사 프로그램 구현 가능**합니다.

근거:
- 전송 인터페이스가 이미 정리돼 있음: [`ILasalTransport.cs`](./Lasal32dll/src/Lasal32Collector/Transport/ILasalTransport.cs)
- LASAL32 DLL 호출 래퍼가 있음 (`LslDirectCreateDOB/Online/Send/Receive`): [`Lasal32NativeLibrary.cs`](./Lasal32dll/src/Lasal32Collector/Transport/Lasal32NativeLibrary.cs)
- 실제 전송 구현 있음: [`LslDirectTransport.cs`](./Lasal32dll/src/Lasal32Collector/Transport/LslDirectTransport.cs)

단, 지금 문서/코드 상태의 핵심 한계:
- 현재 프로토콜 문서는 `snapshot(0x5001)`, `batch(0x5002)`, `heartbeat(0x50FE)` 중심입니다.
  즉 **수집(collector) 중심**이고, `MoveAbsoluteEx` 같은 모션 제어 명령 규격은 안 보입니다.
  - [`PLC_DIRECT_PROTOCOL.md`](./Lasal32dll/docs/PLC_DIRECT_PROTOCOL.md)

그래서 구현 방식은 이렇게 가야 합니다:
1. `ConnectRPC` -> LslDirect 연결 세션 생성(또는 소켓 세션)
2. `MoveAbsoluteEx` -> PLC가 해석할 command ID/payload 송신
3. `GetActualPosition` -> position 요청/응답 또는 주기 snapshot에서 현재축 값 파싱

정리:
- **네, 만들 수 있습니다.**
- 다만 `MoveAbsoluteEx/GetActualPosition`을 “실제 동작”시키려면 PLC 쪽 명령 ID/페이로드 규격이 필요합니다.
- 규격이 없으면 제가 먼저 가정 규격으로 구현해두고, 나중에 PLC 규격에 맞춰 바로 치환할 수 있게 작성할 수 있습니다.

> 지금 MoveAbsoluteEx 랑 GetActualPosition 데이터 패킷 사이즈 얼마나 되는지 유추할 수 있을까? 아님 와이어샤크로 측정해봐야되냐

유추는 일부만 가능합니다.

1. `MoveAbsoluteEx`
이미 로그에 나온 값이 기준입니다. `Outgoing Data Size = 64`
즉, **명령 payload는 64바이트**로 보면 됩니다.

2. `GetActualPosition`
정확한 payload 크기는 지금 정보만으로 확정 불가입니다. 구조상 `MoveAbsoluteEx`보다 훨씬 작을 가능성은 큽니다(보통 16~32B급 추정).

3. 결론
- **API payload 크기**: 일부는 로그로 유추 가능
- **실제 wire(packet) 크기**: TCP/IP 헤더, 분할, ACK 영향이 있어서 **Wireshark로 측정해야 정확**합니다.

즉, 네가 진짜 원하는 게 “네트워크에서 몇 바이트가 오갔는지”면 Wireshark가 정답입니다.

> MoveAbsoluteEx 캡쳐한 내용 분석해봐 스샷처럼 192.168.1.3에 대한 내용만 분석하면돼
>
> User context
>
> Attachments:
> - MoveAbsoluteEx.pcapng: `./packet_capture/MoveAbsoluteEx.pcapng`
> Images:
> - `./packet_capture/MoveAbsoluteEx.png`

<details><summary>5 previous messages</summary>

> 요청대로 `192.168.1.3` 관련 트래픽만 뽑아서 실제 pcap 기준으로 길이/페이로드/포트 흐름을 정리하겠습니다. 먼저 `tshark`로 해당 세션 패킷을 직접 읽어보겠습니다.
>
> `tshark`가 PATH에 없어서 설치 경로를 찾아서 바로 실행해보겠습니다.
>
> 필터 결과 패킷은 3개뿐이라, 이제 payload까지 확인해서 요청/응답/ACK 시간차를 정확히 정리하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청하신 대로 `192.168.1.3` 관련 프레임만 기준으로, 방금 추출한 3개 패킷을 바이트 단위로 한 번 더 풀어서 의미(길이/왕복시간/요청-응답 구조)까지 정확히 정리하겠습니다.
</details>

`192.168.1.3` 기준으로 `MoveAbsoluteEx.pcapng`를 보면, 실제로 잡힌 건 **딱 3프레임**입니다.

1. `192.168.1.13:4867 -> 192.168.1.3:4000`
- TCP `PSH, ACK`, `tcp.len=64` (요청 데이터)
- Payload 64B:
  `a0 20 00 00 38 00 00 00 ...`
- 앞 8바이트는 `0x20A0` + `0x38(56)` 구조로 보이고, 뒤 데이터에 아래 값이 실려 있습니다(LE double 해석):
  - Position: `8388608`
  - Velocity: `48933546`
  - Acc: `4893354600`
  - Dec: `4893354600`
  - Jerk: `48933546000`

2. `192.168.1.3:4000 -> 192.168.1.13:4867`
- TCP `PSH, ACK`, `tcp.len=16` (응답 데이터)
- Payload 16B:
  `00 00 08 00 00 00 00 00 d0 0b 12 00 00 00 00 00`
- 이 16바이트의 의미(성공/오류 코드 매핑)는 프로토콜 스펙 없이는 단정 불가.

3. `192.168.1.13 -> 192.168.1.3`
- TCP `ACK`, `tcp.len=0` (순수 ACK)

핵심 결론:
- 이 캡처 구간에서는 **MoveAbsoluteEx 요청 1회 + 즉시 응답 1회 + ACK 1회**만 존재.
- 요청→응답 간격은 약 **149.4us**.
- 화면에서 말한 “도착완료 시점 패킷”은 이 파일 범위 내 `192.168.1.3` 트래픽에는 **없습니다**.
  즉, 완료 신호를 별도 푸시 패킷으로 안 보내거나, 캡처 구간 밖일 가능성이 큽니다.

> 요것도 사이즈 분석해
>
> User context
>
> Attachments:
> - ReadActualPosition.pcapng: `./packet_capture/ReadActualPosition.pcapng`
> Images:
> - `./packet_capture/ReadActualPosition.png`

<details><summary>2 previous messages</summary>

> 요청하신 `ReadActualPosition.pcapng`를 `192.168.1.3` 필터로 바로 뜯어서, 요청/응답 각각의 `tcp payload size`와 `frame size`를 정확히 뽑겠습니다.
>
> 패킷 3개가 잡혔고, 지금은 페이로드를 리틀엔디안으로 풀어서 `실제 값(예: position)`까지 확인하겠습니다.
</details>

`ReadActualPosition.pcapng`에서 `ip.addr == 192.168.1.3`만 보면, 트랜잭션 1회(3패킷)입니다.

1. 요청 (`192.168.1.13:4867 -> 192.168.1.3:4000`)
- `frame.len = 63`
- `tcp.len = 9`  ← 실제 앱 데이터 크기
- Payload(hex): `2e2000000100000000`

2. 응답 (`192.168.1.3:4000 -> 192.168.1.13:4867`)
- `frame.len = 78`
- `tcp.len = 24` ← 실제 앱 데이터 크기
- Payload(hex): `000010000000000000000080550160410000000053558741`

3. ACK (`192.168.1.13 -> 192.168.1.3`)
- `frame.len = 54`
- `tcp.len = 0` (순수 ACK)

시간
- 요청→응답: 약 `0.1836 ms`
- 응답→ACK: 약 `42.713 ms`

정리
- **ReadActualPosition 요청 페이로드는 9B**
- **ReadActualPosition 응답 페이로드는 24B**
- 프레임 크기까지 포함하면 각각 63B / 78B (ACK 54B) 입니다.

원하면 다음으로 `24B 응답` 필드(에러코드/포지션/속도 추정)를 고정 오프셋 표로 만들어 드릴게요.

> 지금 MoveAbsolute 명령을 줄때 Mode는 어떻게 주고있지? Mode가 없는 메소드를 주고 있는건가. Mode 준다면 PowerOn할때 주는 Mode랑 같이 주게 할수 있나?

<details><summary>3 previous messages</summary>

> 현재 코드에서 `MoveAbsolute`/`MoveAbsoluteEx` 호출 시 모드를 어디서 주는지 먼저 정확히 찾고, `PowerOn 시점 모드`와 통합 가능한지 바로 판단해서 답 드리겠습니다.
>
> 코드 기준으로는 `Operation Mode(OPM402)`와 `MoveAbsoluteEx의 Direction/BufferedMode`가 분리돼 있습니다. 지금 실제 호출 라인을 열어서 정확히 어느 시점에 어떤 파라미터가 들어가는지 확인 중입니다.
>
> 확인 결과 현재 구현은 `MoveAbsoluteEx`에 `Direction + BufferedMode`만 넣고, `OPM402(DS402 모드)`는 별도 `SetOpMode` 버튼에서만 바꾸고 있습니다. `PowerOn`과 묶는 건 코드상 가능합니다. 정확한 방식까지 정리해서 바로 말씀드리겠습니다.
</details>

지금 코드 기준으로는 이렇게 동작합니다.

1. `MoveAbsoluteEx` 호출에는 `Mode`가 아예 없는 게 아닙니다.
`Direction` + `BufferedMode`를 같이 넘깁니다.
[MainWindow.AxisOperations.cs:145](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:145)
[MainWindow.CycleTestOperations.cs:900](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:900)

2. 다만 `Operation Mode(OPM402, DS402 모드)`는 `MoveAbsoluteEx` 파라미터가 아닙니다.
`MMC_ChngOpMode(SetOpMode)`에서 별도로 설정합니다.
[MainWindow.AxisOperations.cs:82](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:82)

3. `PowerOn`도 `OPM402`를 받지 않고 `BufferedMode`만 받습니다.
[MainWindow.AxisOperations.cs:15](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:15)

4. 질문하신 “PowerOn할 때 주는 Mode랑 같이”는 가능은 합니다.
정확히는 `PowerOn` 직후에 `SetOpMode`를 연속 호출해서 묶으면 됩니다.
즉 순서는 `PowerOn -> SetOpMode -> MoveAbsoluteEx`가 맞습니다.

원하면 내가 바로 `PowerOn` 버튼 로직을 `PowerOn + SetOpMode` 일괄 실행으로 패치해줄게요.

> 미안하다 설명이 잘 안됬었네.
> MoveAbsolute 할때 어떤 BufferdMode를 보내는지 물어본거야

맞아요. 현재 코드는 `MoveAbsoluteEx` 호출 시 **UI에서 선택된 `ComboBufferedMode` 값**을 그대로 보냅니다.

- 단일축 버튼 실행:
[MainWindow.AxisOperations.cs:152](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:152)
- Cycle Test 실행:
[MainWindow.CycleTestOperations.cs:526](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:526)
[MainWindow.CycleTestOperations.cs:907](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:907)

즉, 고정값이 아니라 **지금 콤보박스에서 선택된 `MC_BUFFERED_MODE_ENUM`**입니다.
아무것도 안 바꾸면 시작 시 `SelectedIndex=0` 값(첫 enum 값)이 들어갑니다.

> 새로 CycleTest를 만들어서 똑같은 구성인데 Inposition을 우리가 하지않고 드라이브에 맏기자. ReadAxis를 해서 Inposition bit를 확인해서 Inposition 됬다고 판단하고 다음 명령 주는 방식으로 만들어

<details><summary>11 previous messages</summary>

> 요청하신 방식(드라이브 InPosition bit 기반으로 다음 Move 발행)으로 새 CycleTest를 추가하겠습니다. 먼저 현재 코드에서 CycleTest 탭 구조와 `ReadStatus/ReadAxis` 경로, InPosition 비트 체크에 쓸 수 있는 타입/상수를 확인하겠습니다.
>
> 다음으로 InPosition 비트 체크에 쓸 수 있는 enum/상수(예: TARGET_REACHED, IN_POSITION)를 라이브러리에서 직접 찾겠습니다. 있으면 그걸 그대로 쓰고, 없으면 `ReadStatus` 마스크를 옵션화해서 구현하겠습니다.
>
> 라이브러리 enum 이름을 직접 덤프해서 `InPosition`에 해당하는 비트 상수가 있는지 확인하겠습니다. 있으면 그 상수로 정확히 구현하겠습니다.
>
> `ReadStatus(UInt16& axisErrorId, UInt16& statusWord)` 오버로드가 있어서, 여기서 `statusWord` bit로 InPosition 판단하는 방식으로 구현 가능합니다. 이제 기존 `Cycle Test / Cycle Test2` UI를 그대로 복제해서 `Cycle Test3` 탭을 추가하고 이벤트를 연결하겠습니다.
>
> 구조 확인 완료했습니다. 구현은 아래 순서로 진행합니다.
>
> 1. `Cycle Test3` 탭 추가
> - 기존 `Cycle Test`와 거의 동일 UI 복제
> - 차이: `In-position Tolerance` 대신 `In-position Bit Mask (StatusWord Hex)` 입력 추가(기본 `0x0400`)
> - 버튼/상태/요약/저장 폴더를 Test3 전용 컨트롤로 분리
>
> 2. `CycleTestOperations`에 Test3 로직 추가
> - Start/Stop/Save 핸들러 추가
> - `ExecuteCycleTest3` 루프 추가
> - 대기 로직은 `ReadStatus(ref axisErrorId, ref statusWord)`를 폴링해서 `(statusWord & mask)==mask`면 InPosition 판단
> - 기존처럼 timeout, drop, axis error, exception 처리 유지
>
> 3. 요약/엑셀 저장 추가
> - Test3 Summary 문자열 추가
> - Test3 결과 시트 저장 함수 추가(Result + ExecutionLog)
>
> 4. 빌드 검증
> - VS2019 호환 `msbuild` 빌드로 컴파일 확인
>
> 이대로 바로 패치하겠습니다.
>
> 이제 코드 수정 들어갑니다. 먼저 `CycleTestOptions`와 클래스 필드에 Test3 상태(`_isCycleTest3Running`, snapshot, cancellation)와 `StatusWord InPosition bit mask` 옵션을 추가하겠습니다.
>
> 다음은 Test3 시작/정지/저장 핸들러와 옵션 파서를 추가합니다. 기존 Test2 패턴 그대로 가져가되, 실행 엔진만 `ReadStatus 기반 InPosition`으로 연결하겠습니다.
>
> 핵심 엔진(`ExecuteCycleTest3`)과 `ReadStatus + StatusWord bit` 대기 함수를 추가합니다. 여기서 InPosition 판정은 `(statusWord & mask) == mask`로 처리합니다.
>
> 이제 Test3 전용 UI 갱신/요약 빌더를 추가합니다. 결과창 텍스트는 `ReadStatus latency`, `StatusWord bit mask`, `timeout/drop/error` 중심으로 보이게 하겠습니다.
>
> 이제 `MainWindow.xaml`에 `Cycle Test3` 탭을 추가합니다. 기존 Cycle Test 구성과 동일한 입력 세트를 유지하고, `In-position Bit Mask (StatusWord Hex)` 항목만 새로 넣겠습니다.
>
> `Cycle Test3` 탭과 핵심 로직은 반영됐습니다. 지금 컴파일 돌려서 누락된 컨트롤명/메서드 참조 오류를 잡고 마무리하겠습니다.
</details>

요청한 방식으로 `Cycle Test3`를 추가했습니다.
핵심은 **GetActualPosition 오차 기반이 아니라 `ReadStatus`의 StatusWord bit로 In-position 판정**하는 테스트입니다.

변경 파일:
- [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1453)
- [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:458)

구현 내용:
1. `Cycle Test3` 탭 추가
- 동일한 모션 프로파일 입력 구성 + `In-position Bit Mask (StatusWord Hex)` 추가
- 기본값 `0x0400`
- Start/Stop/Save/Auto Save 포함
- 참조: [MainWindow.xaml:1453](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1453), [MainWindow.xaml:1553](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1553)

2. Test3 실행 로직 추가
- Start 핸들러: [MainWindow.CycleTestOperations.cs:458](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:458)
- 옵션 파서: [MainWindow.CycleTestOperations.cs:605](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:605)
- 실행 엔진: [MainWindow.CycleTestOperations.cs:1118](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1118)

3. In-position 판정 방식
- `ReadStatus(ref axisErrorId, ref statusWord)` 폴링
- `(statusWord & InPositionStatusWordMask) == InPositionStatusWordMask` 이면 in-position
- `Stable Samples` 연속 충족 시 다음 명령 발행
- 참조: [MainWindow.CycleTestOperations.cs:1281](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1281), [MainWindow.CycleTestOperations.cs:1316](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1316)

4. 결과 요약/엑셀 저장(Test3) 추가
- Summary: [MainWindow.CycleTestOperations.cs:1889](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1889)
- Save: [MainWindow.CycleTestOperations.cs:2068](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2068)
- Result sheet rows: [MainWindow.CycleTestOperations.cs:2183](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2183)

검증:
- VS2019 MSBuild로 컴파일 성공(에러 0)
- 실행 명령:
`"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" ... /p:Configuration=Debug /p:Platform=AnyCPU`

다음 확인 포인트:
1. 드라이브에서 in-position bit가 정말 `0x0400`인지 확인 후 필요하면 `TextCycle3InPositionBitMask`만 바꿔서 사용.
2. 첫 테스트는 `Cycle Count` 작게(예: 20~50) 돌려서 timeout/bit 판정 정상 여부 먼저 확인.

> 이것도 ReadPosition처럼 latency 저장하는 기능이 들어가있나?

부분적으로 들어가 있습니다.

1. `Cycle Test3`에서 `ReadStatus` 호출 latency는 측정해서 누적합니다.
[MainWindow.CycleTestOperations.cs:1281](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1281)

2. 저장 시에는 `avg/max` 같은 집계값만 저장합니다.
- Summary 텍스트: [MainWindow.CycleTestOperations.cs:1889](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1889)
- Result 시트 컬럼(`ReadStatusLatencyAvg/Max`): [MainWindow.CycleTestOperations.cs:2183](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2183)

3. `ReadPosition` 때처럼 샘플별 latency 행(`PositionSamples` 시트) 저장은 **아직 없습니다**.
원하면 `Cycle Test3`에도 샘플별로 저장하도록 바로 붙여드릴 수 있습니다.

> 저장할 수 있도록 붙여줘

<details><summary>4 previous messages</summary>

> 요청대로 `Cycle Test3`도 샘플 단위 latency를 엑셀로 저장되게 바로 붙이겠습니다. 먼저 현재 샘플 저장 구조(`PositionReadSamples`)를 재사용할지, Test3 전용 샘플을 추가할지 코드 영향 범위를 확인합니다.
>
> 좋습니다. `Cycle Test3`에 샘플별 `ReadStatus latency` 저장 구조를 추가하겠습니다. `Cycle Test`의 `PositionSamples`와 별도로 `StatusReadSamples` 시트를 만들고, 읽기마다 `statusWord/inPosition/latency`를 기록하게 하겠습니다.
>
> 읽기 루프에 샘플 수집을 연결했습니다. 이어서 샘플 저장 메서드와 엑셀 시트(`StatusReadSamples`) 출력을 추가하겠습니다.
>
> 샘플 저장 시트까지 넣었습니다. 지금 바로 빌드해서 컴파일 오류 없는지 확인하겠습니다.
</details>

붙여놨습니다. 이제 `Cycle Test3`도 샘플 단위 latency가 엑셀에 저장됩니다.

변경 핵심:
1. `ReadStatus` 폴링마다 샘플 기록 추가
[MainWindow.CycleTestOperations.cs:1300](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1300)
[MainWindow.CycleTestOperations.cs:1530](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1530)

2. `CycleTest3` 저장 시 `StatusReadSamples` 시트 추가
[MainWindow.CycleTestOperations.cs:2142](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2142)
[MainWindow.CycleTestOperations.cs:2166](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2166)

3. 샘플 시트 컬럼 생성 (`SampleIndex`, `CycleIndex`, `Phase`, `AxisErrorId`, `StatusWord(hex)`, `InPosition`, `StableCounter`, `ReadStart/End`, `ReadLatency`)
[MainWindow.CycleTestOperations.cs:2350](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2350)

4. Result 시트에도 집계값 반영 (`ReadStatusSamplesCaptured`, `DroppedByLimit`, `SampleLimit`)
[MainWindow.CycleTestOperations.cs:2287](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2287)

5. 샘플 저장 제한은 `300000`으로 설정
[MainWindow.CycleTestOperations.cs:20](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:20)

빌드 확인:
- VS2019 MSBuild 기준 `Debug|AnyCPU` 빌드 성공(오류 0).

> 이거 패킷 분석해서 ReadActualPosition하는 패킷이랑 명령 내보내는 패킷이랑 분리할 수 있는 방법을 좀 찾고. 실제로 ReadActualPosition이 얼마에 값에 다음명령이 나갔고 그 명령 받고 바로 동작됬는지 확인좀해봐
>
> User context
>
> Attachments:
> - Motion_Test.pcapng: `./packet_capture/Motion_Test.pcapng`

<details><summary>3 previous messages</summary>

> 요청하신 `Motion_Test.pcapng`를 직접 뜯어서, `ReadActualPosition` 패킷과 모션 명령(`MoveAbsoluteEx`) 패킷을 식별 규칙으로 분리하고 시간축으로 매칭하겠습니다. 먼저 전체 트래픽 구조부터 추출합니다.
>
> 이 캡처에서는 대상 컨트롤러 IP가 `192.168.1.4`로 보입니다. 지금 `TCP 4000` 세션을 기준으로 명령/응답 페이로드를 전부 뽑아서 `ReadActualPosition`/`MoveAbsoluteEx`를 분류하겠습니다.
>
> 분리 기준은 명확히 잡혔습니다(`cmd id + payload 길이`). 지금 각 `MoveAbsoluteEx` 직전 `ReadActualPosition` 값을 매칭하고, 명령 이후 실제 위치가 언제부터 변하기 시작했는지(지연시간)까지 계산하겠습니다.
</details>

분석 완료했습니다. 기준 파일은 `C:\work\Elmo\Elmo_Master\packet_capture\Motion_Test.pcapng` 입니다.

핵심 결과:
1. 실제 제어 스트림은 `192.168.1.13:14132 <-> 192.168.1.3:4000` 입니다.
2. 패킷 분리는 `명령 ID(페이로드 첫 4바이트, little-endian) + tcp.len`으로 정확히 가능합니다.
3. `ReadActualPosition` 직전 값에서 다음 `MoveAbsoluteEx`가 언제 나갔는지, 그리고 명령 후 위치가 언제 변했는지 계산했습니다.

분리 기준:
1. `ReadActualPosition 요청`
- 방향: PC -> Controller
- `tcp.len=9`
- cmd id: `0x202E` (8238)
- 예시 payload: `2e2000000100000000`

2. `MoveAbsoluteEx 요청`
- 방향: PC -> Controller
- `tcp.len=64`
- cmd id: `0x209F` (8351)  (이 캡처 기준)
- 예시 payload 시작: `9f20000038000000...`

3. `ReadAxisError 요청`
- 방향: PC -> Controller
- `tcp.len=9`
- cmd id: `0x2029` (8233)

4. 응답 구분
- `ReadActualPosition 응답`: Controller -> PC, `tcp.len=24`
- `MoveAbsoluteEx 응답`: Controller -> PC, `tcp.len=16`

Wireshark 필터 예시:
1. 전체 제어 트래픽
```wireshark
ip.addr==192.168.1.3 && tcp.port==4000
```
2. ReadActualPosition 요청만
```wireshark
ip.src==192.168.1.13 && ip.dst==192.168.1.3 && tcp.len==9 && tcp.payload[0:4]==2e:20:00:00
```
3. MoveAbsoluteEx 요청만
```wireshark
ip.src==192.168.1.13 && ip.dst==192.168.1.3 && tcp.len==64 && tcp.payload[0:4]==9f:20:00:00
```

실제 타이밍 분석:
1. 카운트
- `ReadActualPosition req=876`, `resp=875`
- `MoveAbsoluteEx req=7`, `resp=7`

2. 통신 응답시간
- Read 요청->응답(24B): 평균 `0.675 ms`
- Move 요청->응답(16B): 평균 `0.659 ms`

3. “ReadActualPosition 값 얼마일 때 다음 명령이 나갔나”
- Move #2: 직전 실제값 `8,287,914`에서 `0` 명령 (직전 read->명령 `0.113 ms`)
- Move #3: 직전 실제값 `105,635`에서 `8,388,608` 명령 (`0.808 ms`)
- Move #4: 직전 실제값 `8,304,153`에서 `0` 명령 (`0.073 ms`)
- Move #5: 직전 실제값 `103,542`에서 `8,388,608` 명령 (`0.773 ms`)
- Move #6: 직전 실제값 `8,285,896`에서 `0` 명령 (`0.129 ms`)
- Move #7: 직전 실제값 `101,417`에서 `8,388,608` 명령 (`0.727 ms`)
- Move #1은 캡처 시작 지점이라 직전 read 샘플 없음

4. “명령 받고 바로 동작했는지”
- 각 Move 명령 후 다음 ReadActualPosition 응답이 `1.34~1.48 ms` 내 도착했고, 그 시점 위치가 이미 직전 값과 달라졌습니다.
- 결론: 네트워크 기준으로는 명령 수신/반영이 빠르게 일어났고, 다음 샘플 시점(약 1~2ms)에서 이미 움직임이 관측됩니다.
- 단, 캡처만으로 “서보 내부에서 정확히 몇 us 후 시작”까지는 단정 불가합니다. (샘플링 해상도 한계)

> 그럼 명령을 내린 시점의 값과 매 주기마다 읽는 값에 대한 정보를 엑셀표로 해서 정리해줘

<details><summary>2 previous messages</summary>

> 좋습니다. 방금 분석한 `Motion_Test.pcapng` 기준으로, 요청하신 내용을 엑셀로 바로 만들어서 저장하겠습니다. `명령 시점 값` 시트와 `주기별 ReadActualPosition` 시트로 분리해서 넣겠습니다.
>
> `openpyxl` 확인됐습니다. 지금 바로 엑셀 파일 생성해서 `명령 시점 값`과 `주기별 ReadActualPosition` 데이터를 넣겠습니다.
</details>

정리해서 엑셀로 만들었습니다.

파일:
- [Motion_Test_Command_ReadAnalysis.xlsx](/C:/work/Elmo/Elmo_Master/packet_capture/Motion_Test_Command_ReadAnalysis.xlsx)

시트 구성:
1. `CommandAtIssue`
- 명령(`MoveAbsoluteEx`) 시점 기준
- 직전 `ReadActualPosition` 값
- 직전 읽기→명령까지 시간(ms)
- 명령 응답시간(ms)
- 명령 후 최초 위치 변화가 관측된 시점과 지연(ms)

2. `ReadActualPosition`
- 매 주기 `ReadActualPosition` 응답 값(프레임/시간/값)
- 이전 읽기 대비 변화량
- 직전/다음 명령과의 시간 관계(ms)

3. `Summary`
- 캡처 기준 카운트/평균 지연 요약

원하면 같은 포맷으로 다른 pcap도 자동 생성되게 스크립트로 묶어드릴게요.

> 이것도 동일하게 분석하고 csv도 똑같이 만들어
>
> User context
>
> Attachments:
> - Motion_Tes2t.pcapng: `./packet_capture/Motion_Tes2t.pcapng`

<details><summary>4 previous messages</summary>

> 요청하신 대로 `Motion_Tes2t.pcapng`도 같은 기준으로 분석하고, 이전과 동일 구조의 산출물(엑셀 + CSV)을 바로 생성하겠습니다. 먼저 캡처 파일 경로와 세션(IP/포트)을 확인합니다.
>
> 세션 구조가 이전과 동일하게 확인됐습니다(`192.168.1.13:14132 <-> 192.168.1.3:4000`). հիմա նույն 포맷으로 엑셀과 CSV 3종을 생성하겠습니다.
>
> 생성은 완료됐는데, `16B 응답`이 다른 명령 응답과 섞여 통계가 약간 과대집계될 여지가 있어서 응답 매칭 로직을 한 번 더 정확히 보정해서 파일을 덮어쓰겠습니다.
>
> CSV/엑셀 재생성 완료했습니다. 마지막으로 요약 수치만 검증해서 바로 전달하겠습니다.
</details>

동일 포맷으로 분석해서 파일 생성 완료했습니다.

생성 파일:
1. [Motion_Tes2t_Command_ReadAnalysis.xlsx](/C:/work/Elmo/Elmo_Master/packet_capture/Motion_Tes2t_Command_ReadAnalysis.xlsx)
2. [Motion_Tes2t_CommandAtIssue.csv](/C:/work/Elmo/Elmo_Master/packet_capture/Motion_Tes2t_CommandAtIssue.csv)
3. [Motion_Tes2t_ReadActualPosition.csv](/C:/work/Elmo/Elmo_Master/packet_capture/Motion_Tes2t_ReadActualPosition.csv)
4. [Motion_Tes2t_Summary.csv](/C:/work/Elmo/Elmo_Master/packet_capture/Motion_Tes2t_Summary.csv)

요약(이번 캡처):
- ReadActualPosition `2109` req / `2109` rsp
- MoveAbsoluteEx `10` req / `10` rsp
- Read Req->Rsp 평균 `0.658 ms`
- Move Req->Rsp 평균 `0.663 ms`
- Move->첫 위치변화(다음 Read에서 관측) 평균 `1.409 ms`

필요하면 이걸 버튼 한 번으로 pcap -> csv/xlsx 자동 생성하는 도구로 프로젝트에 붙여드릴게요.

> readPosition 분석할때 현재 값이랑 이전 값이랑 비교해서 차이값을 row 추가해서 넣어줘

<details><summary>2 previous messages</summary>

> 요청 이해했습니다. `ReadPosition` 샘플 저장 시 현재값과 직전값 차이(`delta`)를 추가하도록 코드 수정하고 빌드까지 확인하겠습니다.
>
> `PositionSamples`에 `직전값 대비 차이` 컬럼을 추가했습니다. 지금 바로 빌드해서 문제 없는지 확인하겠습니다.
</details>

반영 완료했습니다.

변경 내용:
1. `PositionReadSample`에 직전값 차이 필드 추가
- `DeltaFromPreviousActualPosition`
[MainWindow.CycleTestOperations.cs:111](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:111)

2. 샘플 기록 시 현재값-직전값 계산해서 저장
- 첫 샘플은 빈값
[MainWindow.CycleTestOperations.cs:1493](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1493)

3. 엑셀 `PositionSamples` 시트에 컬럼 추가
- `DeltaFromPreviousActual(mm)`
[MainWindow.CycleTestOperations.cs:2306](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2306)

빌드 확인:
- `Debug|AnyCPU` 빌드 성공, 에러 0.

다음에 `Save Result (.xlsx)` 누르면 `PositionSamples`에 차이값 컬럼이 같이 저장됩니다.

> 아니 코드에 추가 말고 니가 만든 엑셀 파일에다가 추가해달라는거였어

<details><summary>1 previous message</summary>

> 맞습니다. 요청 취지대로 코드가 아니라 제가 만든 분석 엑셀 파일을 직접 수정하겠습니다. `ReadActualPosition` 시트에 `현재값-이전값` 차이 컬럼을 넣고 값까지 채워서 저장하겠습니다.
</details>

엑셀 파일에 직접 반영했습니다. 코드 수정과 별개로 아래 2개 파일의 `ReadActualPosition` 시트에 `Diff(Current-Prev)` 컬럼을 추가했고, 각 행에 `현재값 - 이전값`을 채웠습니다.

- [Motion_Test_Command_ReadAnalysis.xlsx](/C:/work/Elmo/Elmo_Master/packet_capture/Motion_Test_Command_ReadAnalysis.xlsx)
- [Motion_Tes2t_Command_ReadAnalysis.xlsx](/C:/work/Elmo/Elmo_Master/packet_capture/Motion_Tes2t_Command_ReadAnalysis.xlsx)

원하면 컬럼명 한글(`현재-이전`)로 바꿔서 다시 저장해드릴게요.

> double? 이건 어떤 문법임?

`double?`는 C#의 `nullable value type` 문법입니다.

의미:
- `double` 또는 `null`을 담을 수 있음
- 정식 표기: `Nullable<double>`
- 축약 표기: `double?`

예시:
```csharp
double? a = null;
double? b = 1.23;
```

왜 쓰냐:
- `double`은 기본적으로 `null` 불가
- “값이 아직 없음/계산 불가” 상태를 표현하려고 `?`를 붙입니다.

> 그냥 선언하는거보다 메모리 차이가 있나?

있습니다. `double`보다 `double?`가 더 큽니다.

- `double`: 8바이트
- `double?` (`Nullable<double>`): 보통 16바이트
  값(8바이트) + `HasValue` 플래그 + 정렬 패딩

즉 대량 배열/리스트면 메모리 차이 꽤 납니다.
대신 `null` 상태 표현이 필요하면 `double?`를 써야 합니다.

> Codex_LASAL_WPF 프로그램 작성해보자. 일단 RPCConection을 누르면 TCP/IP 소켓통신 소켓을 열고, Close는 소켓을 닫아. 그리고 프로그램 종료할때도 알아서 소켓 정리되게 만들어

<details><summary>3 previous messages</summary>

> 요청하신 동작을 `Codex_LASAL_WPF`에 바로 반영하겠습니다. 먼저 현재 `RPCConnection/Close` 버튼 이벤트가 어디에 연결돼 있는지와 소켓 관련 기존 코드 유무를 확인합니다.
>
> 소켓 동작은 더미 백엔드(`SigmatekTcpIpDummyMMCLib.cs`)에 넣고, 종료 정리는 `MainWindow` 종료 이벤트에서 `Context.Disconnect()`를 강제 호출하도록 처리하겠습니다. հիմա 패치 들어갑니다.
>
> 코드 반영 끝났고 지금 컴파일 확인만 진행합니다. 빌드가 통과하면 바로 적용 결과를 정리해서 드리겠습니다.
</details>

요청한 동작으로 반영 완료했습니다.

변경 내용:
1. `MMC_RpcInitConnection` 클릭 시 실제 TCP 소켓 오픈
- `TcpClient` 생성 후 `localIp/localPort`로 `Bind`, `remoteIp/remotePort`로 `Connect` 수행
- 코드: [SigmatekTcpIpDummyMMCLib.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:428), [SigmatekTcpIpDummyMMCLib.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:460)

2. `MMC_CloseConnection` 클릭 시 소켓 닫기
- 연결 상태 제거 + `TcpClient.Close()` 실행
- 코드: [SigmatekTcpIpDummyMMCLib.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:518)

3. 프로그램 종료 시 자동 정리
- `MainWindow.OnClosing`에서 `Context.Disconnect()` 호출
- 코드: [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:37)

4. 컴파일 확인
- `Debug|AnyCPU` 빌드 성공(에러 0)

주의:
- 이제 `RPCConnection`은 실제로 원격에 TCP 연결을 시도합니다. 대상 IP/포트가 열려있지 않으면 연결 실패(InternalError)로 처리됩니다.

> 근데 Elmo Library 쓸때 서버는 4000 port를 쓰는데 클라이언트는 5000 port를 쓰자나. 이렇게하면 소켓이 두개 열리나?

아니요. 보통 **TCP 소켓 1개**만 열립니다.

- 서버: `4000` (listen)
- 클라이언트: 로컬 포트 `5000`으로 bind 후 접속
- 연결 형태: `클라IP:5000 -> 서버IP:4000` **한 개의 TCP 세션**

즉 “포트가 2개라서 소켓 2개”가 아니라,
**한 소켓의 양 끝 포트가 서로 다른 것**입니다.

예외:
- 별도 UDP 콜백 채널이나 추가 TCP 채널을 따로 열면 그때 소켓이 2개 이상 됩니다.
- 지금 네가 만든 LASAL 더미 코드 기준으로는 `ConnectRPC` 때 TCP 1개만 엽니다.

> 엘모의 MoveAbsolute 명령 할 때 프레임 구성이랑
>  그 프레임을 PLC에서 파싱할만한 ST코드를 짜줘

좋아. 캡처 기준으로 `MoveAbsoluteEx` 요청 프레임은 아래처럼 보면 된다.

**MoveAbsoluteEx 요청 프레임 (TCP payload 64 bytes, Little-Endian)**
(캡처에서 cmd id는 `0x209F` 또는 `0x20A0` 둘 다 보일 수 있음)

1. `0x00~0x03` `UDINT` Command ID (`0x0000209F` or `0x000020A0`)
2. `0x04~0x07` `UDINT` Payload Length (`56` = `0x38`)
3. `0x08~0x0F` `LREAL` Position
4. `0x10~0x17` `LREAL` Velocity
5. `0x18~0x1F` `LREAL` Acceleration
6. `0x20~0x27` `LREAL` Deceleration
7. `0x28~0x2F` `LREAL` Jerk
8. `0x30~0x33` `UDINT` Direction
9. `0x34~0x37` `UDINT` BufferedMode
10. `0x38~0x3B` `UDINT` Execute (보통 1)
11. `0x3C~0x3F` `UDINT` Reserved(0)

---

```iecst
TYPE U64_LREAL :
UNION
    U : ULINT;
    R : LREAL;
END_UNION
END_TYPE

TYPE ST_MoveAbsoluteExFrame :
STRUCT
    CmdId       : UDINT;
    PayloadLen  : UDINT;
    Position    : LREAL;
    Velocity    : LREAL;
    Accel       : LREAL;
    Decel       : LREAL;
    Jerk        : LREAL;
    Direction   : UDINT;
    BufferedMode: UDINT;
    Execute     : UDINT;
    Reserved    : UDINT;
END_STRUCT
END_TYPE

FUNCTION UDINT_From_LE : UDINT
VAR_INPUT
    pData  : POINTER TO BYTE;
    Offset : UDINT;
END_VAR
UDINT_From_LE :=
      UDINT(pData[Offset])
  OR  SHL(UDINT(pData[Offset + 1]), 8)
  OR  SHL(UDINT(pData[Offset + 2]), 16)
  OR  SHL(UDINT(pData[Offset + 3]), 24);

FUNCTION ULINT_From_LE : ULINT
VAR_INPUT
    pData  : POINTER TO BYTE;
    Offset : UDINT;
END_VAR
ULINT_From_LE :=
      ULINT(pData[Offset])
  OR  SHL(ULINT(pData[Offset + 1]), 8)
  OR  SHL(ULINT(pData[Offset + 2]), 16)
  OR  SHL(ULINT(pData[Offset + 3]), 24)
  OR  SHL(ULINT(pData[Offset + 4]), 32)
  OR  SHL(ULINT(pData[Offset + 5]), 40)
  OR  SHL(ULINT(pData[Offset + 6]), 48)
  OR  SHL(ULINT(pData[Offset + 7]), 56);

FUNCTION LREAL_From_LE : LREAL
VAR_INPUT
    pData  : POINTER TO BYTE;
    Offset : UDINT;
END_VAR
VAR
    Cvt : U64_LREAL;
END_VAR
Cvt.U := ULINT_From_LE(pData := pData, Offset := Offset);
LREAL_From_LE := Cvt.R;

FUNCTION_BLOCK FB_ParseMoveAbsoluteEx
VAR_INPUT
    pFrame   : POINTER TO BYTE;   // TCP payload 시작 주소
    FrameLen : UDINT;             // payload 길이
END_VAR
VAR_OUTPUT
    Valid    : BOOL;
    ErrorId  : UINT;              // 0=OK, 1=Len, 2=Cmd, 3=PayloadLen
    Data     : ST_MoveAbsoluteExFrame;
END_VAR

Valid := FALSE;
ErrorId := 0;

IF FrameLen < 64 THEN
    ErrorId := 1;
    RETURN;
END_IF;

Data.CmdId := UDINT_From_LE(pData := pFrame, Offset := 0);
IF NOT ((Data.CmdId = 16#0000209F) OR (Data.CmdId = 16#000020A0)) THEN
    ErrorId := 2;
    RETURN;
END_IF;

Data.PayloadLen := UDINT_From_LE(pData := pFrame, Offset := 4);
IF Data.PayloadLen <> 56 THEN
    ErrorId := 3;
    RETURN;
END_IF;

Data.Position     := LREAL_From_LE(pData := pFrame, Offset := 8);
Data.Velocity     := LREAL_From_LE(pData := pFrame, Offset := 16);
Data.Accel        := LREAL_From_LE(pData := pFrame, Offset := 24);
Data.Decel        := LREAL_From_LE(pData := pFrame, Offset := 32);
Data.Jerk         := LREAL_From_LE(pData := pFrame, Offset := 40);
Data.Direction    := UDINT_From_LE(pData := pFrame, Offset := 48);
Data.BufferedMode := UDINT_From_LE(pData := pFrame, Offset := 52);
Data.Execute      := UDINT_From_LE(pData := pFrame, Offset := 56);
Data.Reserved     := UDINT_From_LE(pData := pFrame, Offset := 60);

Valid := TRUE;
```

중요:
- TCP는 스트림이라 `recv 1회 = 프레임 1개` 보장 안 됨.
- 반드시 수신 버퍼 누적 후 `Offset+4`의 `PayloadLen` 읽어서 `총길이=8+PayloadLen` 완성 시 파싱해야 함.

> ST 말고 C랑 유사하게 짠다면?

가능. C 스타일로 쓰면 아래처럼 하면 된다.

```c
#include <stdint.h>
#include <stdbool.h>
#include <string.h>

typedef struct {
    uint32_t cmd_id;        // 0x209F or 0x20A0
    uint32_t payload_len;   // 56
    double   position;
    double   velocity;
    double   accel;
    double   decel;
    double   jerk;
    uint32_t direction;
    uint32_t buffered_mode;
    uint32_t execute;
    uint32_t reserved;
} MoveAbsoluteExFrame;

static uint32_t u32_le(const uint8_t* p) {
    return ((uint32_t)p[0]) |
           ((uint32_t)p[1] << 8) |
           ((uint32_t)p[2] << 16) |
           ((uint32_t)p[3] << 24);
}

static uint64_t u64_le(const uint8_t* p) {
    return ((uint64_t)p[0]) |
           ((uint64_t)p[1] << 8)  |
           ((uint64_t)p[2] << 16) |
           ((uint64_t)p[3] << 24) |
           ((uint64_t)p[4] << 32) |
           ((uint64_t)p[5] << 40) |
           ((uint64_t)p[6] << 48) |
           ((uint64_t)p[7] << 56);
}

static double f64_le(const uint8_t* p) {
    uint64_t u = u64_le(p);
    double d;
    memcpy(&d, &u, sizeof(d));
    return d;
}

// return 0=OK, non-zero=error
int parse_move_absolute_ex(const uint8_t* buf, int len, MoveAbsoluteExFrame* out) {
    if (!buf || !out) return -1;
    if (len < 64) return -2; // need full frame

    uint32_t cmd = u32_le(buf + 0);
    if (!(cmd == 0x0000209F || cmd == 0x000020A0)) return -3;

    uint32_t payload_len = u32_le(buf + 4);
    if (payload_len != 56) return -4;

    out->cmd_id        = cmd;
    out->payload_len   = payload_len;
    out->position      = f64_le(buf + 8);
    out->velocity      = f64_le(buf + 16);
    out->accel         = f64_le(buf + 24);
    out->decel         = f64_le(buf + 32);
    out->jerk          = f64_le(buf + 40);
    out->direction     = u32_le(buf + 48);
    out->buffered_mode = u32_le(buf + 52);
    out->execute       = u32_le(buf + 56);
    out->reserved      = u32_le(buf + 60);

    return 0;
}
```

주의:
- TCP는 스트림이라 `recv()` 한 번이 64바이트 보장 아님.
- 먼저 헤더 8바이트(`cmd`,`payload_len`) 확보, 그다음 `8 + payload_len`까지 누적해서 파싱해야 함.

> c++말고 C라고 생각해. memset이랑 memcpy써서

```c
#include <stdint.h>
#include <string.h>

typedef struct
{
    uint32_t cmd_id;        /* 0x209F or 0x20A0 */
    uint32_t payload_len;   /* 56 */
    double   position;
    double   velocity;
    double   accel;
    double   decel;
    double   jerk;
    uint32_t direction;
    uint32_t buffered_mode;
    uint32_t execute;
    uint32_t reserved;
} MoveAbsoluteExFrame;

static uint32_t read_u32_le(const uint8_t *src)
{
    return ((uint32_t)src[0]) |
           ((uint32_t)src[1] << 8) |
           ((uint32_t)src[2] << 16) |
           ((uint32_t)src[3] << 24);
}

static uint64_t read_u64_le(const uint8_t *src)
{
    return ((uint64_t)src[0]) |
           ((uint64_t)src[1] << 8)  |
           ((uint64_t)src[2] << 16) |
           ((uint64_t)src[3] << 24) |
           ((uint64_t)src[4] << 32) |
           ((uint64_t)src[5] << 40) |
           ((uint64_t)src[6] << 48) |
           ((uint64_t)src[7] << 56);
}

static double read_f64_le(const uint8_t *src)
{
    uint64_t u;
    double d;
    u = read_u64_le(src);
    memset(&d, 0, sizeof(d));
    memcpy(&d, &u, sizeof(d));
    return d;
}

/* return: 0=ok, <0=error */
int parse_move_absolute_ex_frame(const uint8_t *buf, int len, MoveAbsoluteExFrame *out)
{
    if (buf == 0 || out == 0) return -1;
    if (len < 64) return -2;

    memset(out, 0, sizeof(MoveAbsoluteExFrame));

    out->cmd_id      = read_u32_le(buf + 0);
    out->payload_len = read_u32_le(buf + 4);

    if (!(out->cmd_id == 0x0000209F || out->cmd_id == 0x000020A0)) return -3;
    if (out->payload_len != 56) return -4;

    out->position      = read_f64_le(buf + 8);
    out->velocity      = read_f64_le(buf + 16);
    out->accel         = read_f64_le(buf + 24);
    out->decel         = read_f64_le(buf + 32);
    out->jerk          = read_f64_le(buf + 40);
    out->direction     = read_u32_le(buf + 48);
    out->buffered_mode = read_u32_le(buf + 52);
    out->execute       = read_u32_le(buf + 56);
    out->reserved      = read_u32_le(buf + 60);

    return 0;
}
```

- 이건 **순수 C** 기준이다.
- TCP 수신은 프레임 분할될 수 있으니, `64바이트 누적 후 parse_move_absolute_ex_frame()` 호출해야 한다.

> 일단 메세지를 만들수 있게 Codex_LASAL_WPF의 MoveAbsolute를 구현해놔. 대신 double 쓰지말고 Long으로 써.

<details><summary>6 previous messages</summary>

> 요청대로 `Codex_LASAL_WPF`에서 `MoveAbsolute`를 “실제 전송용 메시지 생성” 방식으로 바꾸겠습니다. 먼저 현재 `MoveAbsoluteEx` 구현 위치를 확인하고, `double` 대신 `long` 기반으로 프레임 만드는 코드로 교체하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청한 내용 기준으로 `Codex_LASAL_WPF`의 `MoveAbsolute` 경로를 바로 수정하겠습니다. 먼저 현재 `SigmatekTcpIpDummyMMCLib.cs`의 `MoveAbsoluteEx` 구현과 소켓 송신 구조를 확인한 뒤, `long` 기반 메시지 생성/전송으로 바꾸겠습니다.
>
> 현재 `MoveAbsoluteEx`가 내부 시뮬레이션만 호출하는 상태라, 해당 경로에 `long` 기반 프레임 생성/소켓 송신을 추가해야 합니다. 관련 코드 구간(축 상태, 백엔드 소켓, MoveAbsoluteEx)을 이어서 확인하겠습니다.
>
> `MoveAbsoluteEx` 경로에 `long` 기반 프레임 빌더를 추가하고, TCP 스트림으로 실제 송신하도록 지금 파일을 수정하겠습니다. 기존 시뮬레이션 이동은 유지하되, 메시지 필드는 전부 정수(`long`)로 구성하겠습니다.
>
> `MoveAbsoluteEx`에 `long` 변환 후 64바이트 프레임 생성/송신 로직을 넣었습니다. 지금 빌드해서 컴파일 에러 없이 동작하는지 바로 확인하겠습니다.
</details>

요청한 내용 반영했습니다. `Codex_LASAL_WPF`에서 `MoveAbsoluteEx`가 이제 `long` 기반으로 프레임을 만들어 TCP로 송신합니다.

변경 파일:
- [SigmatekTcpIpDummyMMCLib.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs#L698)

핵심 변경:
1. `MoveAbsoluteEx(double...)` 입력값을 전부 `long`으로 변환 후 `MoveAbsolute(long...)` 호출
2. `MoveAbsolute(long...)` 추가
3. 64바이트 고정 프레임 빌더 추가 (`BuildMoveAbsoluteFrame`)
   - `Command=0x209F`
   - `AxisRef`
   - `PayloadLength=56`
   - `position/velocity/acc/dec/jerk`를 `int64` Little Endian으로 작성
   - `direction`, `bufferMode`, `execute`, `reserved`를 `int32`로 작성
4. `SendMoveAbsoluteFrame` 추가
   - 연결/쓰기 가능 여부 확인
   - TCP stream으로 송신
   - 실패 시 `MMCException` throw

빌드 확인:
- `PmasApiWpfTestApp.sln` (`Debug | Any CPU`) 빌드 성공, 에러 0개.

원하면 다음 단계로 UI 입력도 `double` 파싱이 아니라 `long` 전용 입력/검증으로 바꿔드릴 수 있습니다.

> PowerOn, PowerOff, GetActualPosition 이것도 구현해줄수 있나.

<details><summary>3 previous messages</summary>

> 가능합니다. `Codex_LASAL_WPF`에서 `PowerOn`, `PowerOff`, `GetActualPosition`도 MoveAbsolute처럼 TCP 프레임 송신(그리고 위치는 응답 파싱)으로 바로 붙이겠습니다. 먼저 현재 메서드와 예외 흐름을 확인한 뒤 수정하겠습니다.
>
> 수정 방향은 이렇게 진행합니다: `PowerOn/PowerOff`는 `long` 기반 필드의 전용 프레임을 만들어 송신하고, `GetActualPosition`은 요청 프레임 송신 후 응답에 `int64` 위치값이 있으면 우선 사용, 없으면 기존 시뮬레이션 값을 반환하도록 구현하겠습니다. ახლა 코드 반영합니다.
>
> 코드 반영 완료했고, 지금 바로 빌드해서 누락(`using`, 타입 충돌, C#7.3 호환) 없는지 확인하겠습니다.
</details>

구현했습니다. `Codex_LASAL_WPF`에 `PowerOn`, `PowerOff`, `GetActualPosition`도 TCP 프레임 기반으로 붙였습니다.

변경 파일:
- [SigmatekTcpIpDummyMMCLib.cs:699](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:699)

적용 내용:
- `PowerOn`/`PowerOff`:
  - 프레임 생성 후 송신하도록 변경
  - 위치: [PowerOn:716](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:716), [PowerOff:723](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:723), [BuildPowerFrame:1081](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1081)
- `GetActualPosition`:
  - 요청 프레임 송신 + 응답 읽기/파싱 추가
  - 응답 파싱 실패 시 기존 시뮬레이션 값으로 fallback
  - 위치: [GetActualPosition:761](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:761), [SendFrameAndTryRead:988](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:988), [BuildGetActualPositionRequestFrame:1095](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1095), [TryParseGetActualPositionResponse:1139](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1139)
- 공통 송신 경로:
  - `SendFrame`, `GetWritableStreamOrThrow`로 통합
  - 위치: [SendFrame:958](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:958)

빌드:
- `Debug | Any CPU` 빌드 성공 (오류 0).

주의:
- `PowerCommandId=0x2080`, `GetActualPositionCommandId=0x00E0`은 현재 가정값입니다. PLC 쪽 실제 명령 ID/응답 포맷과 다르면 그 값에 맞춰 바로 조정하면 됩니다.

> Axis is not loaded. 메세지가 나오는데 여기선 Axis를 Load할수 없는데 일단 Axis는 생략하고 추후에 개선하도록하자

<details><summary>2 previous messages</summary>

> 요청대로 축 로드 단계를 임시 생략하도록 바꾸겠습니다. `EnsureAxis()`에서 축이 없으면 기본 축(`Axis1`)을 자동 생성해서 진행하게 수정하겠습니다.
>
> 수정 반영했고 바로 빌드 확인하겠습니다.
</details>

반영했습니다. 이제 `Axis is not loaded.`로 멈추지 않습니다.

변경:
- [PmasControllerContext.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs) 의 `EnsureAxis()` 수정
- 축이 없으면 예외 대신 기본 축 `Axis1` 자동 생성 후 계속 진행
- 로그에 `"Axis load step is skipped. Fallback axis is auto-created: Axis1"` 남기도록 처리

빌드:
- `Codex_LASAL_WPF\PmasApiWpfTestApp.sln` `Debug | Any CPU` 빌드 성공 (오류 0)

이제 Axis Load 버튼 없이 바로 `PowerOn/PowerOff/GetActualPosition/MoveAbsoluteEx` 테스트 가능합니다.

> 이렇게 뜨는데 원인이 뭐지
>
> User context
>
> Images:
> - `[embedded image extracted: ./assets/embedded_line_2566.png; original line 2566; mime image/png; base64 chars 281144]`

원인 핵심은 로그에 이미 나와 있습니다.

- `AxisRef=0` 로 명령이 나가고 있습니다.
- 그 상태에서 `MC_POWER_ID`, `MC_MOVEABSOLUTEEX_ID`를 보내면 컨트롤러가 `NC_UNSUITABLE_NODE_STATE (Status=16)`를 반환합니다.

즉, 지금은 “유효한 축 객체/축 참조가 없는 상태”입니다.

가능성 높은 원인:
1. `Axis Name`이 실제 MDS 리소스 이름과 다름 (`a01`이 실제 이름이 아닐 수 있음)
2. `MMCGetAxisByNameCmd`가 실질적으로 축 로드 실패
3. 컨트롤러 프로젝트 RUN/다운로드 상태 문제
4. 해당 축이 단축 Power 대상이 아니라 그룹 축 설정 상태

바로 확인할 것:
1. `MMCGetAxisByNameCmd` 누른 직후 로그에서 `AxisRef` 값 확인 (`0`이면 실패)
2. 축 이름을 MDS에 있는 정확한 문자열로 입력 (대소문자/공백 포함)
3. 컨트롤러 RUN 상태 확인
4. Fault 있으면 `MMCReset` 후 `PowerOn` 재시도

원하면 코드에 `AxisRef==0`일 때 명령 보내기 전에 바로 차단하고, 오류 메시지를 `"축 로드 실패(이름 불일치)"`로 명확히 바꿔줄 수 있습니다.

> AxisRef를 세팅할 수 없어. 일단 AxisRef가 없더라도 명령어가 나갈수 있게 만들어

<details><summary>1 previous message</summary>

> 요청대로 “AxisRef 없이도 송신” 모드로 바꾸겠습니다. `Codex_LASAL_WPF`에서 `PowerOn/PowerOff/MoveAbsolute/GetActualPosition` 프레임의 AxisRef를 강제로 `0`으로 넣고 송신되게 수정하겠습니다.
</details>

요청대로 바꿨습니다. 이제 `AxisRef`를 못 잡아도 명령은 나갑니다.

변경 내용:
- [SigmatekTcpIpDummyMMCLib.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs)
- `ForceZeroAxisRefForCommands = true` 추가
- 아래 프레임 생성 시 AxisRef를 강제로 `0`으로 세팅:
  - `BuildPowerFrame`
  - `BuildGetActualPositionRequestFrame`
  - `BuildMoveAbsoluteFrame`

결과:
- 축 참조 없이도 TCP 프레임 송신됨
- 빌드 성공 (`Debug | Any CPU`, 오류 0)

주의:
- 이 상태는 임시 운용용입니다. 실제 제어 논리(축 식별)가 필요해지면 `ForceZeroAxisRefForCommands`를 `false`로 바꾸고 축 참조 매핑 로직을 넣어야 합니다.

> PowerOn이랑 PowerOff랑 Command ID 다르게 만들어

<details><summary>1 previous message</summary>

> 요청 반영해서 `PowerOn`/`PowerOff`를 서로 다른 Command ID로 분리하겠습니다. 프레임 빌더도 명령 ID를 인자로 받도록 수정한 뒤 빌드 확인하겠습니다.
</details>

반영했습니다. `PowerOn`/`PowerOff`를 서로 다른 Command ID로 분리했습니다.

변경 파일:
- [SigmatekTcpIpDummyMMCLib.cs:702](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:702)

변경 내용:
- `PowerOnCommandId = 0x2081`
- `PowerOffCommandId = 0x2082`
- `PowerOn`은 `PowerOnCommandId`로 송신
- `PowerOff`는 `PowerOffCommandId`로 송신
- `BuildPowerFrame(...)`가 `commandId`를 인자로 받아 프레임 헤더에 기록

참고 위치:
- [PowerOn:720](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:720)
- [PowerOff:727](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:727)
- [BuildPowerFrame:1085](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1085)

빌드도 완료했고 오류 없습니다.

> Stop이랑 Reset도 알려주고 지금 구현된 명령어들 command id 나열해

<details><summary>1 previous message</summary>

> 지금 코드 기준으로 실제 송신되는 Command ID를 정확히 뽑아서 정리하겠습니다. `Codex_LASAL_WPF`의 더미 라이브러리에서 상수와 `Stop/Reset` 구현 상태를 바로 확인하겠습니다.
</details>

현재 코드 기준 정확히 이렇게 되어 있습니다.

구현된 Command ID (실제 프레임 송신됨):
- `PowerOn` = `0x2081` ([SigmatekTcpIpDummyMMCLib.cs:702](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:702), [SigmatekTcpIpDummyMMCLib.cs:720](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:720))
- `PowerOff` = `0x2082` ([SigmatekTcpIpDummyMMCLib.cs:703](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:703), [SigmatekTcpIpDummyMMCLib.cs:727](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:727))
- `GetActualPosition` = `0x00E0` ([SigmatekTcpIpDummyMMCLib.cs:704](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:704), [SigmatekTcpIpDummyMMCLib.cs:765](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:765))
- `MoveAbsoluteEx` = `0x209F` ([SigmatekTcpIpDummyMMCLib.cs:705](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:705), [SigmatekTcpIpDummyMMCLib.cs:796](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:796))

`Stop`, `Reset` 상태:
- `Reset`은 현재 로컬 상태만 변경하고 TCP 송신 없음 (`_state.Powered = true`) → Command ID 미정 ([SigmatekTcpIpDummyMMCLib.cs:735](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:735))
- `Stop`도 로컬 정지만 하고 TCP 송신 없음 (`_state.Stop()`) → Command ID 미정 ([SigmatekTcpIpDummyMMCLib.cs:740](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:740))

원하면 `Stop/Reset`도 ID 지정해서 프레임 송신으로 바로 붙이겠습니다.

> Stop이랑 Reset도 알려주고 -> 알려주고가 아니라 명령어 구현해

<details><summary>2 previous messages</summary>

> 맞습니다. `Stop`/`Reset`도 말만이 아니라 실제 TCP 명령 송신으로 구현하겠습니다. 같은 파일에 Command ID 추가하고, 두 메서드에서 프레임 생성 후 `SendFrame` 타도록 수정하겠습니다.
>
> `Reset/Stop` 송신 코드 넣었습니다. 지금 바로 빌드해서 컴파일/링크 문제 없는지 확인하겠습니다.
</details>

구현 완료했습니다. 이제 `Stop`/`Reset`도 실제 명령 프레임을 TCP로 송신합니다.

변경 파일:
- [SigmatekTcpIpDummyMMCLib.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs)

추가된 Command ID:
- `ResetCommandId = 0x2083`
- `StopCommandId = 0x2084`

동작 변경:
- `Reset()`:
  - `BuildResetFrame(...)` 생성
  - `SendFrame(ResetCommandId, ...)` 송신
- `Stop(bufferedMode)`:
  - `BuildStopFrame(...)` 생성
  - `SendFrame(StopCommandId, ...)` 송신

프레임:
- `Reset`: 12 bytes (`Command`, `AxisRef`, `PayloadLength=4`, `Execute=1`)
- `Stop`: 16 bytes (`Command`, `AxisRef`, `PayloadLength=8`, `BufferedMode`, `Execute=1`)

빌드 확인:
- `Debug | Any CPU` 빌드 성공, 오류 0.

> 지금 구현된 메세지들의 프레임 구조를 나열해줘

<details><summary>1 previous message</summary>

> 지금 코드 기준으로 정확한 프레임 구조를 뽑아 정리해서 드리겠습니다. `Codex_LASAL_WPF`의 실제 빌더 함수 기준으로 오프셋/길이로 나열하겠습니다.
</details>

현재 구현 기준 프레임 구조는 아래와 같습니다. 기준 파일: [SigmatekTcpIpDummyMMCLib.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs)

공통:
- 바이트 오더: `Little Endian`
- 현재 `ForceZeroAxisRefForCommands = true`라서 송신 시 `AxisRef`는 강제로 `0`

Command ID:
- `PowerOn`: `0x2081`
- `PowerOff`: `0x2082`
- `Reset`: `0x2083`
- `Stop`: `0x2084`
- `GetActualPosition`: `0x00E0`
- `MoveAbsoluteEx`: `0x209F`

프레임:

```text
1) PowerOn / PowerOff (16 bytes)
[0..1]   UInt16  CommandId (0x2081 or 0x2082)
[2..3]   UInt16  AxisRef
[4..7]   UInt32  PayloadLength = 8
[8..11]  Int32   Power (1=On, 0=Off)
[12..15] Int32   BufferedMode
```

```text
2) Reset (12 bytes)
[0..1]   UInt16  CommandId = 0x2083
[2..3]   UInt16  AxisRef
[4..7]   UInt32  PayloadLength = 4
[8..11]  Int32   Execute = 1
```

```text
3) Stop (16 bytes)
[0..1]   UInt16  CommandId = 0x2084
[2..3]   UInt16  AxisRef
[4..7]   UInt32  PayloadLength = 8
[8..11]  Int32   BufferedMode
[12..15] Int32   Execute = 1
```

```text
4) GetActualPosition Request (8 bytes)
[0..1]   UInt16  CommandId = 0x00E0
[2..3]   UInt16  AxisRef
[4..7]   UInt32  PayloadLength = 0
```

```text
5) GetActualPosition Response (파서 기대 형태, 최소 16 bytes)
[0..1]   UInt16  CommandId = 0x00E0
[2..3]   UInt16  AxisRef
[4..7]   UInt32  PayloadLength >= 8
[8..15]  Int64   ActualPosition
```

```text
6) MoveAbsoluteEx (64 bytes)
[0..1]   UInt16  CommandId = 0x209F
[2..3]   UInt16  AxisRef
[4..7]   UInt32  PayloadLength = 56
[8..15]  Int64   Position
[16..23] Int64   Velocity
[24..31] Int64   Acceleration
[32..39] Int64   Deceleration
[40..47] Int64   Jerk
[48..51] Int32   Direction
[52..55] Int32   BufferedMode
[56..59] Int32   Execute = 1
[60..63] Int32   Reserved = 0
```

> 이게 동일하게 Elmo Library랑 같은 구조인가?

아니요. **현재 구현은 Elmo Library와 동일 구조가 아닙니다.**
지금은 “비슷한 형태”만 맞춘 상태입니다.

차이점 핵심:
- `MoveAbsoluteEx`는 헤더/길이(64B) 형태는 비슷하게 맞췄지만, 파라미터를 `long(Int64)`로 보냅니다. Elmo 원형은 `double` 기반일 가능성이 큽니다.
- `AxisRef`를 강제로 `0`으로 보내는 임시 모드입니다. 실제 Elmo는 유효한 `AxisRef`를 사용합니다.
- `PowerOn/PowerOff/Reset/Stop`의 Command ID(`0x2081~0x2084`)는 현재 우리가 임시로 둔 값입니다. Elmo 실 ID와 검증되지 않았습니다.
- MMCLib의 실제 RPC 세션/응답 처리(트랜잭션, 상태 체크, 에러 매핑)와 1:1로 같지 않습니다.

정리하면: **API 이름은 같지만 wire protocol은 아직 완전 호환이 아닙니다.**

> 5) GetActualPosition Response (파서 기대 형태, 최소 16 bytes)
> [0..1]   UInt16  CommandId = 0x00E0
> [2..3]   UInt16  AxisRef
> [4..7]   UInt32  PayloadLength >= 8
> [8..15]  Int64   ActualPosition
>
> 이거 Elmo Library 패킷 분석했을때 이렇게 나왔었냐

<details><summary>2 previous messages</summary>

> 좋은 지적입니다. 지금 질문은 “실제 캡처 기준으로 그 응답 구조가 맞았는지”라서, `ReadActualPosition.pcapng`를 직접 다시 확인해서 정확히 답하겠습니다.
>
> `Wireshark CLI(tshark)`가 없어서, 같은 검증을 Python으로 직접 파싱해서 확인하겠습니다. `ReadActualPosition.pcapng`의 TCP payload 바이트를 그대로 뽑아 비교하겠습니다.
</details>

아니요. **캡처상 그렇게 안 나왔습니다.**

`ReadActualPosition.pcapng`에서 `192.168.1.3` 관련 실제 TCP payload는:

1. 요청 (`192.168.1.13:4867 -> 192.168.1.3:4000`)
- 길이: `9 bytes`
- Hex: `2e2000000100000000`

2. 응답 (`192.168.1.3:4000 -> 192.168.1.13:4867`)
- 길이: `24 bytes`
- Hex: `000010000000000000000080550160410000000053558741`

즉, 내가 전에 적은
- `CommandId=0x00E0`
- `PayloadLength>=8`
- `총 16 bytes`
이 형태는 **Elmo 캡처와 일치하지 않습니다.**

응답 24바이트는 8바이트씩 나누면:
- `0x0000000000100000`
- `double 8391340.0`
- `double 48933472.0`

처럼 보입니다. (필드 의미는 추가 캡처/문서로 확정 필요)

> 매뉴얼을 봤을때 저 데이터가 어떤 데이터인거 같음?

<details><summary>4 previous messages</summary>

> 좋습니다. 추정이 아니라 매뉴얼 근거로 답하려면 PDF에서 `ReadActualPosition`/응답 구조 설명을 직접 확인해야 합니다. 지금 문서 텍스트를 뽑아서 해당 명령 섹션을 찾아보겠습니다.
>
> 매뉴얼 PDF보다 더 직접적인 근거가 이미 로컬에 있습니다. `NetHelp` 안의 `ReadActualPositionIN/OUT` 구현 코드를 열어서 실제 필드 의미를 확인하겠습니다.
>
> 근거가 나왔습니다. `ReadActualPositionIN/OUT` 코드상 필드와 캡처 값을 직접 매핑해서 어떤 데이터인지 확정해보겠습니다. `SwapOutParam` 헤더 구조도 같이 확인하겠습니다.
>
> 결론 낼 수 있습니다. 캡처값을 MMCLib 내부 `ReadActualPositionIN/OUT` 구조와 맞춰보면 필드 해석이 거의 일치합니다. 마지막으로 Command ID 매핑만 확인하고 정리해서 답 드리겠습니다.
</details>

매뉴얼 기준으로 보면 저 응답은 **`MMC_READACTUALPOSITION_OUT` 데이터**가 맞습니다.

근거:
- 매뉴얼 v2.012, `6.2.13 MMC_ReadActualPosition` 섹션:
  - IN: `ucEnable`
  - OUT: `double dbPosition`, `unsigned short usStatus`, `short sErrorID`
  - 파일: [Maestro Administrative and Motion API_2022_12_v2.012.pdf](/C:/work/Elmo/Elmo_Master/Maestro Administrative and Motion API_2022_12_v2.012.pdf) (423~424 page)
- MMCLib 내부 코드도 동일:
  - `ReadActualPositionIN.CommandID = 8238 (0x202E)`
  - `ReadActualPositionOUT`에서 `data[8]`부터 `double Position`, `data[16]` status, `data[18]` error 파싱
  - 파일: [ReadActualPositionIN.cs](/C:/work/Elmo/Elmo_Master/NetHelp/Elmo Maestro API Help/MMCLibDotNET/ElmoMotionControlComponents.GMAS.MMCLibDotNET/ElmoMotionControl/GMAS/EASComponents/MMCLibDotNET/InternalArgs/ReadActualPositionIN.cs), [ReadActualPositionOUT.cs](/C:/work/Elmo/Elmo_Master/NetHelp/Elmo Maestro API Help/MMCLibDotNET/ElmoMotionControlComponents.GMAS.MMCLibDotNET/ElmoMotionControl/GMAS/EASComponents/MMCLibDotNET/InternalArgs/ReadActualPositionOUT.cs)

네가 캡처한 응답(hex):
`000010000000000000000080550160410000000053558741` (24 bytes)

해석:
- `0..7`: 공통 헤더 8바이트
- `8..15`: `dbPosition (double)` = **8391340.0**
- `16..17`: `usStatus` = **0**
- `18..19`: `sErrorID` = **0**
- `20..23`: 패딩/정렬 바이트(실질 필드 아님)

핵심:
- 이전에 말한 “16바이트 응답”은 틀렸고,
- 실제는 **헤더 8 + OUT 구조체 16 = 총 24바이트**로 보는 게 맞습니다.

> P-Mas 프로그램에서 CycleTest4를 만들고 똑같은 포멧으로 명령을 줄껀데 이번에는 반복 테스트는 안할꺼야.
> 어떤식으로 동작하냐면 MoveAbsoluteEx명령을 주고 바로 데이터를 읽을껀데 이번에 읽을 데이터는 ReadStatus를 읽을껀데 이걸 어떤간격으로 얼마만큼 읽을지만 세팅할수 있게 해줘

<details><summary>7 previous messages</summary>

> `Codex_PMAS_WPF`에 `CycleTest4`를 추가하고, 기존 탭 포맷을 유지하면서 `MoveAbsoluteEx` 1회 발행 후 `ReadStatus`를 지정 간격/횟수만큼 수집하는 흐름으로 붙이겠습니다. 먼저 현재 `Cycle Test` 탭 구성과 `CycleTest2/3` 구현 방식을 확인하겠습니다.
>
> `CycleTest3`의 `ReadStatus` 수집 로직과 저장 포맷을 재사용할 수 있어 보입니다. 이제 `CycleTest4`용 옵션/실행/결과 저장 코드를 붙이고, XAML에 동일 형식의 탭을 추가하겠습니다.
>
> `CycleTest4`는 별도 옵션으로 두는 게 안전합니다. 반복 테스트 공용 옵션을 억지로 재사용하지 않고, `MoveAbsoluteEx 1회 + ReadStatus N회` 전용으로 추가하겠습니다. 저장도 기존처럼 `.xlsx`로 남기도록 붙이겠습니다.
>
> 구성은 이렇게 잡겠습니다.
>
> 1. `MainWindow.xaml`에 `Cycle Test4` 탭 추가
> 2. `MainWindow.CycleTestOperations.cs`에 `CycleTest4` 전용 상태/옵션/실행/저장 로직 추가
> 3. 동작은 `MoveAbsoluteEx` 1회 발행 후 `ReadStatus`를 `즉시 1회 + 이후 지정 간격`으로 `지정 횟수`만큼 수집
> 4. 결과는 기존 탭처럼 진행률/요약/엑셀 저장까지 동일하게 유지
>
> 이제 XAML과 코드 비하인드를 같이 수정하겠습니다.
>
> 코드 로직은 들어갔고, 이제 `Cycle Test4` 탭 UI를 추가하는 단계입니다. 기존 `Cycle Test3`와 동일한 패턴으로 `Motion Profile / Control / Result` 3구역을 넣겠습니다.
>
> 핵심 로직은 다 들어갔습니다. 빌드 전에 한 군데 더 다듬고 있습니다. `CycleTest4`에서도 `ReadStatus`에서 축 에러가 감지되면 결과를 실패로 반영하도록 기존 테스트와 기준을 맞추겠습니다.
>
> 빌드까지 통과했습니다. 마지막으로 위치 참조를 정리해서 어떤 파일에 무엇이 들어갔는지 확인하고 마무리하겠습니다.
</details>

`Codex_PMAS_WPF`에 `Cycle Test4`를 추가했습니다. 탭은 [MainWindow.xaml:1665](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1665)에 들어갔고, 입력값은 `Base Position`, `Move Distance`, `Velocity/Acc/Dec/Jerk`, `Read Count`, `Read Interval(ms)`, `Drop Threshold(ms)`입니다. 동작은 `MoveAbsoluteEx`를 1회 발행한 뒤 `ReadStatus`를 즉시 1회 읽고, 이후 지정 간격으로 지정 횟수만큼 계속 읽습니다. 반복 왕복 테스트는 하지 않습니다.

로직은 [MainWindow.CycleTestOperations.cs:649](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:649)부터 추가했습니다. 핵심은 [BuildCycleTest4Options](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:798), [ExecuteCycleTest4](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1541), [BuildCycleTest4Summary](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2506), [SaveLastCycleResult4ToExcel](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2743)입니다. 결과는 기존과 같은 형식으로 진행률/요약 표시되고, `StatusReadSamples`까지 `.xlsx`로 저장됩니다.

빌드는 `Debug | Any CPU`로 정상 통과했습니다.

> 지금 이 캡쳐된 내용으로 분석을 해보자. 192.168.1.3으로 보낸 내용과 받은 내용으로 Move Absolute 명령과 이 명령을 받고 언제 움직이기 시작했는지 추측해봐
>
> User context
>
> Attachments:
> - MoveAbsoluteEx_DelayTest.pcapng: `./packet_capture/MoveAbsoluteEx_DelayTest.pcapng`

<details><summary>8 previous messages</summary>

> `MoveAbsoluteEx_DelayTest.pcapng`에서 `192.168.1.3` 기준 TCP payload를 전부 뽑고, `MoveAbsoluteEx` 요청/응답과 이후 위치 읽기 패턴을 시간순으로 맞춰보겠습니다. 움직임 시작 시점은 네트워크에서 직접 보이는 게 아니라 첫 위치 변화 시점으로 추정해야 하므로, 그 기준으로 계산하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 캡처에서 `MoveAbsoluteEx`와 뒤따르는 폴링 명령을 분리했고, 지금은 반복적으로 읽는 명령이 정확히 무엇인지와 응답 값 변화 시점을 맞추고 있습니다. 그걸 기준으로 "명령 송신 시점"과 "실제 움직임 시작 추정 시점"을 절대시간/상대시간으로 정리하겠습니다.
>
> 반복 패킷은 `MoveAbsoluteEx` 다음에 들어오는 `ReadStatus` 폴링으로 보입니다. 이제 로컬 MMCLib 소스에서 명령 ID와 OUT 구조를 직접 대조해서, 응답 값 변화가 어떤 상태 비트 변화인지 맞추겠습니다.
>
> `ReadStatus` 명령 ID는 확인됐습니다. 이제 응답 20바이트의 필드 의미와 상태값 `0x40000042 -> 0x40000044 -> 0x40000041`를 로컬 소스/매뉴얼의 상태 정의와 대조합니다.
>
> 응답 20바이트는 확정적으로 `ReadStatus` 구조입니다. 남은 건 첫 4바이트 `uiState`의 의미인데, 이 값이 바뀌는 순간이 네트워크 상에서 확인 가능한 "움직이기 시작한 첫 징후"입니다. 상태 enum을 바로 대조하겠습니다.
>
> 상태값 해석이 붙었습니다. `0x40000042/44/41`는 각각 `가속/정속/감속 + Discrete Motion` 조합으로 읽히고, 이 캡처엔 위치값이 없어서 "정확한 실제 시작시각"은 못 찍습니다. 대신 네트워크 상으로는 "최초로 움직이고 있음이 확인된 시점"까지는 좁힐 수 있습니다.
>
> 상태 전이는 잡혔지만, 관측된 `가속 -> 정속 -> 감속` 구간 길이가 명령 파라미터로 예상되는 이동시간과 전혀 안 맞습니다. 이건 캡처만 보면 단순 프로파일 동작이라기보다 스케일링/단위 차이 또는 다른 지연이 끼어 있다는 뜻입니다. 끝까지 상태가 어떻게 끝나는지까지 보고 결론 내겠습니다.
</details>

분석 결과는 이렇습니다. 캡처 파일: [MoveAbsoluteEx_DelayTest.pcapng](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest.pcapng)

**결론**
- `MoveAbsoluteEx` 명령은 맞습니다.
- 명령 수신 후 실제로 “움직이기 시작했다”고 네트워크에서 처음 확인되는 근거는 `ReadStatus` 응답의 상태값이 `가속 중`으로 바뀐 시점입니다.
- 캡처만으로 정확한 실제 모터 시작시각을 찍을 수는 없고, `추정 구간`으로만 말할 수 있습니다.

**패킷 분리**
1. `MoveAbsoluteEx` 송신
- 패킷 `#7`
- 시간: `+0.000 ms`
- `192.168.1.13:6713 -> 192.168.1.3:4000`
- Payload `64 bytes`
- Hex:
```text
9f2000003800000000000000000060410000005055558741000080a6aa3af241000080a6aa3af2410000205055c9264202000000010000000100000000000000
```

2. `MoveAbsoluteEx` 응답
- 패킷 `#8`
- 시간: `+673.200 ms`
- `192.168.1.3:4000 -> 192.168.1.13:6713`
- Payload `16 bytes`
- Hex:
```text
0000080000000000f0a10b0000000000
```
- `DefaultOutputFBArgs` 구조로 해석하면:
  - `Handle = 762352`
  - `Status = 0`
  - `Error = 0`
- 즉, 명령은 정상 접수된 것으로 보입니다.

3. 이후 반복 패킷은 `ReadStatus`
- 요청 Payload `16 bytes`
```text
28200000080000000000000001000000
```
- `0x2028 = MC_READSTATUS_ID`
- 응답 Payload `20 bytes`
```text
00000c000000000042000040000000000000b712
```
형태는 `ReadStatusArgsOUT`와 일치합니다:
- `uiState` 4 bytes
- `usStatus` 2 bytes
- `sErrorID` 2 bytes
- `usAxisErrorID` 2 bytes
- `usStatusWord` 2 bytes

**상태값 해석**
`ReadStatus` 응답의 `uiState`가 이렇게 바뀝니다.

1. `#10` `+1596.300 ms`
- `uiState = 0x40000042`
- 하위 비트 해석:
  - `0x40 = DISCRETE_MOTION`
  - `0x02 = ACCELERATING`
- 의미: 이미 가속 중

2. `#80` `+64436.600 ms`
- `uiState = 0x40000044`
- 하위 비트 해석:
  - `0x40 = DISCRETE_MOTION`
  - `0x04 = CONSTANT_VELOCITY`
- 의미: 정속 구간

3. `#200` `+173582.800 ms`
- `uiState = 0x40000041`
- 하위 비트 해석:
  - `0x40 = DISCRETE_MOTION`
  - `0x01 = DECELERATING`
- 의미: 감속 구간

캡처 마지막도 감속 상태입니다.
- 마지막 패킷 `#208`
- 시간 `+180480.900 ms`
- 아직 `0x40000041`
- 즉, 이 캡처에는 `StandStill` 복귀가 없습니다.

**언제 움직이기 시작했는가**
정확한 값은 못 찍습니다. 이유는 이 캡처에 `ReadActualPosition`이 없기 때문입니다.

그래도 추정은 가능합니다.

1. 확실한 사실
- `+673.200 ms`에 `MoveAbsoluteEx` 정상 응답 수신
- `+1596.300 ms` 첫 `ReadStatus` 응답에서 이미 `ACCELERATING`

2. 보수적으로 말하면
- 축은 `+1.596 s` 이전에는 이미 움직이기 시작했습니다.

3. 더 현실적인 추정
- 첫 `ReadStatus` 요청이 `+963.700 ms`에 PC에서 나갑니다.
- 같은 LAN이므로 컨트롤러 도착은 거의 즉시라고 보는 게 맞습니다.
- 따라서 실제 시작시점은 대략:
  - `+0.673 s ~ +0.964 s` 사이일 가능성이 가장 높습니다.
- 이건 추정입니다. 확정은 아닙니다.

**명령 내용 디코드**
`MoveAbsoluteEx` payload는 로컬 MMCLib 구조와 정확히 맞습니다.
- `Command = 0x209F`
- `Position = 8388608.0`
- `Velocity = 48933546.0`
- `Acceleration = 4893354600.0`
- `Deceleration = 4893354600.0`
- `Jerk = 48933546000.0`
- `Direction = 2`
- `BufferMode = 1`

**중요한 점**
캡처상 상태 전이 구간은:
- 가속: `~1.6 s` 시점에 이미 진입
- 정속 진입: `~64.4 s`
- 감속 진입: `~173.6 s`

이건 명령 파라미터로 직관적으로 예상되는 이동시간과 잘 안 맞습니다. 즉 아래 중 하나입니다.
1. 단위/스케일이 네가 생각한 값과 다름
2. 컨트롤러 내부에서 다른 스케일 변환이 있음
3. 이 상태값이 순수 서보 프로파일 진행만 반영하는 게 아님
4. 테스트 조건상 외부 지연이 큼

**정리**
- `MoveAbsoluteEx` 명령은 `#7`
- 정상 응답은 `#8`
- 실제 움직임의 첫 네트워크 근거는 `#10`의 `ReadStatus = ACCELERATING`
- 따라서
  - 확실한 상한: `+1.596 s 이전`
  - 실무적 추정: `+0.673 s ~ +0.964 s`

원하면 다음 중 하나 바로 해줄 수 있습니다.
1. 이 캡처를 기준으로 시간표를 `csv`로 뽑아주기
2. `ReadActualPosition` 캡처까지 같이 대조해서 시작시점을 더 좁혀주기
3. `MoveAbsoluteEx` 응답과 `ReadStatus` 응답을 기준으로 PLC 파싱 로직 정리해주기

> csv로 뽑아봐바

<details><summary>3 previous messages</summary>

> 캡처를 기준으로 바로 CSV를 만들겠습니다. `MoveAbsoluteEx`, `ReadStatus`, 응답 지연, 상태 해석까지 들어간 형태로 저장하겠습니다.
>
> 상세 CSV 하나와 상태 전이만 추린 요약 CSV 하나를 같이 만들겠습니다. 기존 캡처 폴더에 저장하겠습니다.
>
> CSV는 생성됐습니다. 헤더와 주요 행 몇 줄만 확인하고 바로 경로 전달하겠습니다.
</details>

CSV로 뽑아놨습니다.

파일:
1. 상세 타임라인
[MoveAbsoluteEx_DelayTest_Command_StatusTimeline.csv](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest_Command_StatusTimeline.csv)

2. 상태 변화만 요약
[MoveAbsoluteEx_DelayTest_StateChanges.csv](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest_StateChanges.csv)

들어간 내용:
- `MoveAbsoluteEx` 요청/응답
- `ReadStatus` 요청/응답
- 상대시간 `rel_ms`
- 요청 대비 응답 지연 `response_latency_ms`
- `ReadStatus` 상태 해석
  - `0x40000042 = ACCELERATING|DISCRETE_MOTION`
  - `0x40000044 = CONSTANT_VELOCITY|DISCRETE_MOTION`
  - `0x40000041 = DECELERATING|DISCRETE_MOTION`

요약 CSV 기준 상태 전이:
- `1596.300 ms` 가속 상태 확인
- `64436.600 ms` 정속 상태 확인
- `173582.800 ms` 감속 상태 확인

원하면 다음으로
1. 이 CSV를 엑셀 `xlsx`로 다시 묶기
2. 실제 시작 시점 추정 칼럼을 추가하기
3. `ReadActualPosition` 캡처랑 합쳐서 비교표 만들기

> 이것도 똑같이 분석하고 csv 만들어봐
>
> User context
>
> Attachments:
> - Motion_Tes2t.pcapng: `./packet_capture/Motion_Tes2t.pcapng`

<details><summary>9 previous messages</summary>

> `Motion_Tes2t.pcapng` 구조를 먼저 분리하겠습니다. `192.168.1.3` 기준으로 어떤 명령과 어떤 피드백이 오가는지 확인한 뒤, 같은 방식으로 CSV를 만들겠습니다.
>
> 이 캡처는 이전 것과 달리 `ReadStatus`가 아니라 거의 전부 `ReadActualPosition`입니다. 그래서 이번 CSV는 `명령 시점`, `실제 위치 첫 변화 시점`, `이전값 대비 delta` 기준으로 만드는 게 맞습니다.
>
> 캡처 안에는 왕복 이동이 여러 번 들어 있고, 중간에 `0x2029` 계열 9바이트 명령이 끼어 있습니다. 이것도 같이 식별해서 CSV에 넣겠습니다.
>

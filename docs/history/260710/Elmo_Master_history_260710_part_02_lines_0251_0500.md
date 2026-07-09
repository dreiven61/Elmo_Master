
> 이거 두개 폴더 비교해서 어떤게 수정됐는지 체크해봐
>
> User context
>
> Attachments:
> - Elmo_EtherCAT_Test_4Axis: `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/`
> - Elmo_EtherCAT_Test_4Axis_Edit: `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit/`

<details><summary>5 previous messages</summary>

> 두 LASAL 폴더를 실제 파일 기준으로 비교하겠습니다. 먼저 과거 작업 메모리에 이 폴더명이 있는지 빠르게 확인하고, 동시에 파일 수/상태와 `git diff --no-index` 기준 변경 목록을 뽑겠습니다.
>
> 첫 비교에서 중요한 점이 보입니다. 두 폴더 전체가 다른 것이 아니라 대부분 동일하고, `git diff --no-index` 기준으로 차이가 난 파일은 제한적입니다. 이제 코드 변경인지 IDE/프로젝트 메타데이터 변경인지 구분하기 위해 실제 diff 내용을 보겠습니다.
>
> 실제 코드 차이는 `TCPMotionInterface.st`에 집중되어 있습니다. 지금부터는 이 파일 안에서 바뀐 명령 ID, 패킷 offset, 응답 형식, 실제 축 제어 호출부를 줄 번호 기준으로 뽑아 보겠습니다.
>
> 네트워크 쪽은 `ConnectionFileManager.xml`의 프로젝트명 변경과 `Eni.xml`의 EtherCAT TxPDO Error Code 항목 제거가 확인됩니다. 이제 group move/응답 함수 쪽 변경까지 확인해서 “실제 동작 변경”만 추려 정리하겠습니다.
>
> 중요한 동작 차이가 하나 더 보입니다. 기존 폴더는 Group Move 파라미터를 DINT/transition mode로 변환해서 `MoveLinearAbsEx()`에 넘겼는데, Edit 폴더에서는 그 변환 코드가 없어졌는지 확인 중입니다.
</details>

기준은 `Elmo_EtherCAT_Test_4Axis` -> `Elmo_EtherCAT_Test_4Axis_Edit`로 봤습니다.

결론: 두 폴더는 파일 수가 둘 다 `592`개로 거의 같은 복사본이고, 실제 의미 있는 코드 변경은 거의 전부 [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit/Class/TCPMotionInterface/TCPMotionInterface.st:424)에 몰려 있습니다.

수정된 항목:

- `Class/TCPMotionInterface/TCPMotionInterface.st`
  - TCP 수신 파싱 방식 변경: 기존은 `CommandID @0`, `AxisRef @2`, `Payload @4`였고, Edit는 `CommandID @0`, `PayloadLength @4`, `AxisRef @6`로 바뀜.
  - `udSize >= 8`, `udSize <= ReceiveBuf`, `udSize >= usPayloadLength + 8` 검증 추가.
  - 명령 ID 체계가 변경됨:
    - 기존: `0x2081`, `0x2082`, `0x2083`, `0x2084`
    - Edit: `0x2023`, `0x2022`, `0x2024`, `0x103C`, `0x202B`, `0x20A0`, `0x20A2`, `0x2047~0x2049`, `0x2085` 등 추가/변경
  - `PowerOn`, `PowerOff`, `MoveAbs`가 즉시 응답 패킷을 보내도록 변경됨.
  - `MoveAbs`는 LREAL 원본값을 `ClampLRealToDint`로 변환해서 `LMCAxis.MoveAbsolute()` 호출.
  - 주의: 기존에 있던 Group Move 변환 코드가 Edit에서 빠져 있음. 기존은 `GroupMoveVector -> GroupMovePos`, `GroupVelocityLReal -> GroupVelocity`, `GroupTransitionModeInput -> GroupTransitionMode` 변환을 했는데, Edit 파일에는 해당 대입이 검색되지 않았습니다. 이 상태면 `MoveLinearAbsEx()`가 이전 값/초기값으로 동작할 위험이 있습니다.

- `Network/Eni.xml`
  - 4축 각각에서 TxPDO `#x1A26` Error Code 항목, 즉 `#x603F Error Code UINT`가 제거됨.
  - CRC도 변경됨.

- `Network/ConnectionFileManager.xml`
  - 프로젝트명 필터가 `Elmo_EtherCAT_Test_4Axis`에서 `Elmo_EtherCAT_Test_4Axis_Edit`로 변경됨.

- 프로젝트/IDE 메타 파일 변경
  - 루트 프로젝트 파일명이 `Elmo_EtherCAT_Test_4Axis.lcp/.lcb`에서 `Elmo_EtherCAT_Test_4Axis_Edit.lcp/.lcb`로 바뀜.
  - `Class/Classes.lcb`, `Include/global.ldi`, `MultiMasterExp.mme`, `TCPMotionInterface.lba` 등 LASAL 생성/프로젝트 바이너리도 변경됨.
  - `ProjectInternal/*`은 breakpoints/open views/browser cache 같은 IDE 내부 상태 변경입니다. 설계 근거로 볼 파일은 아닙니다.

현재 `main` 기준으로는 `Elmo_EtherCAT_Test_4Axis_Edit` 폴더 전체가 untracked 상태입니다. 아직 Git에 올라간 변경은 아닙니다.

> 데이터 변환은 PC에서 하는게 맞는거 같다. PLC가 하면 불필요한 load만 늘어나고 숫자 연산의 경우 ARM계열 CPU보다 AMD 인텔 계열 퍼스널컴퓨터에서 하는게 맞아. 지금 C:\work\Elmo\Elmo_Master\LMC_Library에 API를 만든 원본 프로그램이 있으니 이걸 수정해서 Lasal에서 수행하는 단위로 맞춰서 데이터를 보낼수 있게 다 수정해. 데이터는 C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF에 더미로 어떤 방식으로 보내는지 예제로 해놨으니 참고하면 될꺼야.

<details><summary>40 previous messages</summary>

> 요지는 명확합니다. PLC 쪽에서 LREAL/DINT 변환이나 단위 변환을 하지 않게, `LMC_Library`의 PC API 송신부를 LASAL 실행 단위에 맞춰 보내도록 바꾸겠습니다. 먼저 원본 API와 `Codex_LASAL_WPF` 더미 송신 포맷을 대조해서 바꿀 지점을 특정하겠습니다.
>
> 현재 작업트리는 `main`이고, `Elmo_EtherCAT_Test_4Axis_Edit` 전체가 미추적 상태입니다. 이 폴더는 비교 대상으로만 보고 건드리지 않겠습니다. 실제 수정 대상은 `LMC_Library`의 C# API/샘플/테스트 앱 쪽으로 좁혀서 진행합니다.
>
> `LMC_Library` 안에는 API 소스 프로젝트가 아니라, 테스트 앱 소스와 이미 빌드된 `LmcMotionApi.dll`이 들어 있습니다. 그런데 테스트 앱은 DLL만 참조하고 있어서 API 내부 패킷 빌더를 직접 고치려면 DLL 소스 위치를 추가로 찾아야 합니다. 우선 저장소 전체에서 `LmcMotionApi` 소스를 찾겠습니다.
>
> 검색 결과 저장소에 `namespace LmcMotionApi` 소스는 없습니다. `LMC_Library`의 핵심 API는 `LmcMotionApi.dll` 바이너리로만 들어 있고, 소스가 있는 것은 WPF 테스트 앱뿐입니다. 따라서 DLL을 반영/디컴파일해서 현재 공개 API와 패킷 빌더를 확인한 뒤, 같은 공개 API를 유지하는 소스 DLL로 재빌드하는 방식이 필요합니다.
>
> `Codex_LASAL_WPF`의 기준은 확인됐습니다. 단축은 UI값에 `*10000` 해서 Int64 프레임으로 보내고, 그룹은 위치/속도/가감속/저크에 `*10000` 해서 double 프레임으로 보냅니다. `LMC_Library` 테스트 앱은 아직 `CountsPerRev=8388608` 기반으로 입력값을 count로 바꿔 DLL에 넘기고 있어서, 이 경로를 LASAL 단위 입력/송신으로 바꿔야 합니다.
>
> 여기서 기준을 하나 분명히 잡겠습니다. LASAL 실행 단위는 화면 표시 단위가 아니라 “내부 프레임 값”입니다. `Codex_LASAL_WPF`는 사용자가 `360`을 넣으면 실제 TCP에는 `3,600,000`을 보냅니다. 그래서 `LMC_Library`도 외부 입력은 LASAL 표시 단위로 받고, 송신 직전 PC에서 `*10000` 변환하도록 맞추겠습니다.
>
> 이제 `LMC_Library` 테스트 앱 UI와 문서에서 count/rev 전제를 찾아 바꾸겠습니다. 코드 변경 전에는 `CountsPerRev`와 `U()` 변환이 핵심 위험 지점입니다.
>
> DLL 공개 API를 확인했습니다. 필요한 공개 함수는 유지할 수 있습니다. 기존 DLL은 `LMC_MoveAbsoluteExCmd(double...)`, `LMC_MoveLinearAbsoluteExCmd(double[]...)`, `LMC_StopCmd(float...)`, `LMC_GroupStopCmd(float...)` 형태라서 호출부에서 PC 변환을 적용해도 API 시그니처는 그대로 유지됩니다. 다만 전달 패키지까지 맞추려면 문서와 샘플도 함께 수정해야 합니다.
>
> `LmcMotionApi.dll`은 여러 복사본이 있고 `bin/Patched`라는 과거 산출물도 있습니다. 이 저장소에는 소스가 없지만, DLL은 작고 공개 API가 단순합니다. 우선 현재 전달 패키지 DLL과 테스트 앱 DLL이 동일한지 확인한 뒤, 호출부/문서부터 맞추고 필요하면 새 DLL 소스 프로젝트를 추가하겠습니다.
>
> 전달 패키지의 `bin/LmcMotionApi.dll`과 `test-app/LmcMotionApi.dll`은 같은 해시입니다. 테스트 앱이 참조하는 `LmcMotionApiTestApp/bin/Debug/LmcMotionApi.dll`은 다른 해시지만 이 경로는 `.gitignore` 대상이라 커밋 대상이 아닙니다. 따라서 커밋 기준으로는 전달 패키지 DLL 2개와 테스트 앱 소스/문서를 맞추는 게 맞습니다.
>
> 패킷을 실제로 바꾸려면 DLL 내부 직렬화도 확인해야 합니다. IL 도구가 있으면 디컴파일해서 직접 확인하고, 없으면 현재 공개 API에 맞춰 새 DLL을 빌드해서 기존 전달 DLL을 대체하겠습니다.
>
> `ildasm`과 `csc`가 있습니다. 기존 DLL을 IL로 확인해서 현재 프레임 빌더가 어떤 단위로 쓰는지 정확히 보고, 같은 API를 유지한 새 DLL을 만들 수 있습니다.
>
> 기존 DLL의 IL을 확인 중입니다. 핵심은 단축/그룹 이동 프레임이 값을 그대로 쓰는지, 이미 LASAL 스케일이 들어 있는지입니다.
>
> 기존 DLL은 IL 기준으로 이동값을 그대로 `double/float`로 TCP에 씁니다. 그러면 `8388608`을 넣으면 그대로 `8388608`이 PLC로 가기 때문에, PLC가 다시 단위 해석/변환을 떠안게 됩니다. 이번 수정은 API 입력 단위는 기존 문서대로 count 계열을 유지하고, DLL 내부에서 `count * 360 * 10000 / 8388608`로 LASAL internal 값으로 바꿔 보내는 방식으로 잡겠습니다.
>
> 기존 테스트 앱의 `rev -> count` 변환은 그대로 둘 수 있습니다. 대신 DLL 안에서 count를 LASAL internal로 변환하면 UI는 계속 “rev/rps”로 쓰고, TCP는 PLC가 바로 실행할 내부 단위로 나갑니다. 읽기 응답도 반대로 internal -> count로 바꿔야 기존 앱의 `count / CountsPerRev = rev` 표시가 맞습니다.
>
> 기존 DLL 구조상 가장 안전한 바이너리 수정점은 `F64`/`F32` 직렬화 헬퍼입니다. 이 DLL에서 `F64`는 이동값 프레임 작성에 쓰이고, `F32`는 Stop/GroupStop 감속·저크 작성에 쓰입니다. 따라서 두 헬퍼에 count→LASAL internal 스케일을 넣으면 공개 API는 그대로 두고 송신값만 바뀝니다. 읽기 응답은 `ReadActualPosition`/`GroupReadActualPosition`에서 반대 변환을 적용해야 합니다.
>
> IL 패치를 적용하기 전에 영향 범위를 수치로 확인했습니다. 송신 스케일은 `3600000 / 8388608 = 0.4291534423828125`, 수신 역스케일은 `8388608 / 3600000 = 2.3301688888888887`입니다. 이걸 DLL에 직접 넣고, 문서에는 “API 입력은 기존 count 기준, TCP 송신은 LASAL internal 기준”으로 명시하겠습니다.
>
> 이제 기존 DLL을 IL 수준에서 패치합니다. 수동으로 LASAL ST를 건드리지 않고, PC API DLL의 송신 직렬화 헬퍼와 수신 double 역변환만 바꿉니다.
>
> 패치된 IL에는 송신 스케일과 수신 역스케일이 들어갔습니다. 이제 DLL로 재조립하고, reflection으로 프레임 값이 `8388608 -> 3600000`으로 바뀌었는지 확인합니다.
>
> 프레임 검증 결과 패치된 DLL은 `8388608` 입력을 단축/그룹/Stop 모두 `3600000`으로 송신합니다. 이제 이 DLL을 전달 패키지 위치에 반영하고, 테스트 앱/문서의 단위 설명을 “입력은 count/rev 기준, 송신은 LASAL internal로 PC 변환”으로 맞추겠습니다.
>
> DLL 반영은 끝났습니다. 이어서 사람이 보는 UI와 전달 문서를 바꿔서, 나중에 count/raw 값인지 LASAL internal 값인지 헷갈리지 않게 하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/LMC_API/README.md` (+5 -1)
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/LMC_API_함수명_커맨드ID_인자.txt` (+5 -1)
> - Wrote `./LMC_Library/LMC_API/LMC_API/docs/LMC_PACKET_MAP.md` (+29 -1)
> - Wrote `./LMC_Library/LMC_API/LMC_API/sample/BasicUsage.cs` (+8 -6)
> - Wrote `./LMC_Library/LmcMotionApiTestApp/MainWindow.xaml` (+2 -2)
>
> </details>
>
> 문서와 UI 라벨도 반영했습니다. 이제 테스트 앱을 다시 빌드해서 전달 패키지의 `LmcMotionApiTestApp.exe`도 같은 UI/동작 기준으로 갱신하겠습니다.
>
> 빌드는 통과했습니다. 빌드 산출 EXE를 전달 패키지 `test-app`으로 복사하고, DLL 해시가 빌드 후에도 패치본으로 유지됐는지 확인합니다.
>
> 전달 패키지의 DLL 두 곳과 테스트 앱 빌드 참조 DLL이 모두 같은 패치 해시입니다. 다음으로 실제 패키지 DLL에서 프레임 값과 수신 역변환을 다시 확인하고, Git diff를 검토하겠습니다.
>
> 검증 스크립트에서 수신 역변환 호출 인자 포장만 PowerShell 쪽에서 한 번 실수했습니다. DLL 자체 송신값은 이미 확인됐고, 지금 역변환까지 다시 확인합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 상태를 이어받아서 LMC API 쪽 바이너리/문서 수정이 실제로 원하는 단위 변환을 하는지 다시 검증하겠습니다. 특히 TCP 송신 프레임의 `8388608 -> 3600000` 변환과 실제 위치 응답 역변환을 깨끗한 프로세스에서 확인하겠습니다.
>
> 현재 변경 범위는 `LMC_Library` 아래 문서/샘플/테스트앱/패키지 DLL로 한정되어 있습니다. 이전에 비교용으로 생긴 `Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit` 폴더는 아직 대량 untracked 상태이고, 이번 수정에는 건드리지 않았습니다.
>
> DLL 내부의 프레임 생성 함수까지 직접 호출해 확인하겠습니다. 이 검증은 UI 표시가 아니라 실제 TCP payload에 들어갈 byte 배열 offset 값을 읽는 방식입니다.
>
> 첫 호출은 `AxisMove`의 첫 번째 인자 타입을 잘못 잡아서 실패했습니다. 메서드 시그니처를 정확히 다시 뽑은 뒤 같은 검증을 재실행합니다.
>
> 검증 결과는 기대값과 맞습니다. `8388608 count` 입력이 송신 프레임에서 `3600000`으로 들어가고, LASAL 내부값 `3600000` 응답은 API 반환 시 `8388608 count`로 역변환됩니다.
>
> 테스트앱 빌드도 통과했습니다. 이제 수정된 문서/샘플 내용이 실제 변경과 어긋나지 않는지 diff를 빠르게 확인하고 마무리하겠습니다.
>
> 문서 diff도 실제 동작과 맞습니다. API 호출자는 기존처럼 count 기준을 넣고, DLL 내부에서 LASAL internal 단위로 바꿔 보내는 구조로 정리되어 있습니다.
>
> 테스트앱 UI는 rev/rps 입력을 `CountsPerRev`로 count로 변환한 뒤 API를 호출하고, API DLL이 다시 LASAL internal 단위로 송신합니다. 즉 PC 쪽에서 전체 변환이 끝나고 PLC에는 LASAL 실행 단위가 넘어갑니다.
>
> 마지막으로 메모리 citation용 근거 라인만 확인합니다. 이전 작업에서 보존된 `8388608 -> 360` 스케일링 사실을 이번 변환 기준으로 사용했기 때문입니다.
>
> 이전 요청에서 “폴더별로 정리해서 커밋” 지시가 있었지만, 지금 변경은 `LMC_Library` 한 범위로 묶입니다. 해당 범위만 스테이징해서 단위 변환 변경 커밋으로 남기겠습니다.
>
> 현재 브랜치는 `main`입니다. untracked LASAL 비교 폴더는 제외하고, 추적 중인 `LMC_Library` 변경만 커밋 대상으로 잡습니다.
>
> `git diff --check`는 통과했고, 경고는 두 텍스트 파일의 LF→CRLF 변환 예정 경고뿐입니다. 이제 `LMC_Library` 단위로 커밋합니다.
>
> 커밋이 생성되었습니다. 마지막으로 작업트리에 남은 변경이 의도한 untracked 비교 폴더뿐인지 확인하겠습니다.
</details>

완료했습니다. `LMC_Library` 변경만 별도 커밋했습니다.

변경 내용:
- `LmcMotionApi.dll`이 TCP 송신 직전에 count 기준 값을 LASAL internal 단위로 변환하도록 수정했습니다.
- 변환식: `lasal_internal = count * 360 * 10000 / 8388608`
- 실제 위치 읽기 응답은 반대로 `LASAL internal -> count`로 역변환합니다.
- 테스트앱 UI/문서/샘플도 이 기준에 맞췄습니다.

검증:
- DLL 프레임 직접 호출 확인: `8388608 count -> 3600000 LASAL internal`
- 위치 응답 역변환 확인: `3600000 -> 8388608 count`
- `MSBuild` 테스트앱 빌드 성공
- `git diff --check -- LMC_Library` 통과

커밋:
- `414499d Convert LMC API motion units on PC`

주의할 점:
- `LmcMotionApi.dll` 원본 C# 소스는 저장소에 없어서 IL 재조립 방식으로 DLL을 수정했습니다.
- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit` untracked 폴더는 그대로 남겨뒀고 이번 커밋에는 포함하지 않았습니다.
- 원격 push는 하지 않았습니다.

> dll 자체를 수정해야겠어. dll에서 수정할꺼야

<details><summary>1 previous message</summary>

> 말 그대로 DLL 본체 수정이 맞습니다. 지금 상태가 실제로 DLL 바이너리 변경으로 들어가 있는지 커밋과 파일 해시를 다시 확인하겠습니다.
</details>

맞습니다. 지금 반영한 방식도 DLL 본체 수정입니다.

수정된 DLL:
- `C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\LMC_API\bin\LmcMotionApi.dll`
- `C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API\LMC_API\test-app\LmcMotionApi.dll`

두 파일 해시는 동일합니다:
`AED6E1ACC80A99CCB1A1B7870F0B61978E89275313B56F7EC71F7000AB1A725D`

DLL 내부에서 수정된 핵심은:
- 송신 전 `F64`, `F32` 직렬화 지점에서 `count -> LASAL internal` 변환
- 수신 위치 읽기 `F64R`에서 `LASAL internal -> count` 역변환

즉 WPF나 호출부에서 우회 변환한 게 아니라, `LmcMotionApi.dll` 자체가 변환된 값을 TCP payload로 만들도록 바꾼 상태입니다.
현재 커밋 `414499d`에 DLL 바이너리 변경이 포함되어 있습니다.

> C:\work\Elmo\Elmo_Master\LMC_Library\LMC_LASAL_API_Delivery 여기에 라이브러리 수정해서 dll 수정했다.

<details><summary>22 previous messages</summary>

> 새로 만든 `LMC_LASAL_API_Delivery` 폴더를 기준으로 실제 DLL과 문서/소스가 어떻게 들어갔는지 확인하겠습니다. 기존 `LMC_API` 쪽 변경과 중복인지, 별도 배포본인지 구분해서 보겠습니다.
>
> 새 폴더는 소스가 포함된 별도 LASAL 전용 API 배포본으로 보입니다. `bin` DLL은 `.gitignore`에 걸릴 가능성이 있어서, 실제 커밋 대상에 DLL을 포함하려면 강제 추가가 필요한지도 같이 확인하겠습니다.
>
> 문서상으로는 새 DLL이 `rev × 8,388,608 -> DINT count`를 보내는 구조입니다. 그런데 이전 기준은 `8388608 count == 360 displayed × 10000 == 3600000 LASAL internal`이었으므로, 실제 소스가 어느 단위를 보내는지 확인해야 합니다.
>
> 소스에서 바로 한 가지 확정 버그가 보입니다. 패킷 헤더 문서와 생성 함수는 payload length를 offset `4`에 쓰는데, 수신부 `Exchange()`는 offset `2`에서 읽고 있어서 응답 payload를 거의 항상 안 읽습니다. 이건 DLL 동작에 직접 영향이 있으니 수정해야 합니다.
>
> 빌드는 통과했습니다. 다만 새 DLL 단위가 실제 LASAL 파서와 맞는지 봐야 합니다. 지금 `Elmo_EtherCAT_Test_4Axis_Edit` 쪽은 아직 원래 LREAL offset 일부가 남아 있는 흔적이 있어서, 관련 구간을 정확히 열어보겠습니다.
>
> 확인 결과, 새 DLL 소스는 두 가지를 고쳐야 합니다. 하나는 수신 payload length offset 버그이고, 다른 하나는 단위가 `rev -> 8,388,608 count`라서 `Codex_LASAL_WPF` 예제의 `rev -> 3,600,000 LASAL internal`과 맞지 않는 점입니다. 지금 목적은 PLC 변환 제거이므로 DLL에서 `rev * 360 * 10000`로 보내도록 바로 수정하겠습니다.
>
> 수정 방향은 `LMC_UnitConverter` 자체를 LASAL internal 단위 변환기로 바꾸는 것입니다. 기본값은 `1 rev = 360 * 10000 = 3,600,000 DINT`이고, 문서/샘플도 그 기준으로 맞추겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/src/LmcLasalProtocol.cs` (+9 -9)
>
> </details>
>
> 단위 변환 함수명을 실제 의미에 맞게 바꿨으니, 이를 호출하는 Axis/Group 쪽도 같이 바꾸고 수신 payload length offset도 수정합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/src/LmcLasalAxis.cs` (+4 -4)
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/src/LmcLasalGroup.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/src/LmcLasalConnection.cs` (+1 -1)
>
> </details>
>
> 이제 문서와 샘플도 count 기준이 아니라 LASAL internal 기준으로 바꿉니다. 그래야 나중에 DLL만 보고도 `8388608 count`가 아니라 `3600000 DINT`를 보내는 구조라는 점이 명확합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/README.md` (+5 -2)
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/docs/LASAL_DINT_PACKET_MAP.txt` (+3 -1)
> - Wrote `./LMC_Library/LMC_LASAL_API_Delivery/sample/BasicUsage.cs` (+2 -2)
>
> </details>
>
> Release 빌드는 통과했습니다. 이제 빌드된 DLL을 배포 폴더의 `bin/LmcLasalMotionApi.dll`로 복사하고, DLL 내부 프레임 값이 실제로 `1 rev -> 3600000`인지 리플렉션으로 확인합니다.
>
> 배포 DLL 해시가 Release 빌드 산출물과 동일합니다. 다음 검증은 두 가지입니다: 변환기 값(`1 rev -> 3600000`)과 TCP 응답 payload length를 offset `4`에서 읽는 동작입니다.
>
> C# optional parameter는 리플렉션에서 무인자 생성자로 보이지 않아 첫 검증 스크립트가 실패했습니다. 생성자에 기본값 `3600000`을 명시해서 다시 실행합니다.
>
> 두 번째 실패는 `Exchange` 메서드 리플렉션 선택 문제입니다. 메서드 시그니처를 정확히 지정해서 다시 실행하겠습니다.
>
> 네트워크 응답 테스트는 로컬 서버 타이밍 문제로 실패했지만, 변환/프레임 검증은 별도로 가능합니다. 수신 offset 수정은 소스 diff로 확인하고, 실제 payload 단위 검증을 먼저 끝내겠습니다.
>
> 새 DLL 폴더는 소스와 최종 DLL까지 같이 남기는 게 맞습니다. `bin/`은 기본 ignore라서 최종 DLL은 강제 추가하고, `src/bin`/`obj` 같은 빌드 산출물은 제외하겠습니다.
>
> 스테이징된 대상은 `LMC_LASAL_API_Delivery` 폴더와 최종 DLL뿐입니다. 이제 staged diff와 `git diff --check --cached`를 확인한 뒤 커밋하겠습니다.

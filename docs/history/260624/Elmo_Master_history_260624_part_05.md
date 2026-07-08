<!--
Source: docs/history/Elmo_Master_history_260624.md
Original lines: 1001-1250
Part: 5 of 10
-->

# Elmo_Master_history_260624 part 05

Source lines: 1001-1250

변경 내용:
- 분석 문서 추가: [PMAS_GroupMotion_TCP_Protocol_2026-06-17.md](/C:/work/Elmo/Elmo_Master/docs/architecture/PMAS_GroupMotion_TCP_Protocol_2026-06-17.md:1)
- LASAL `TCPMotionInterface`에 추가:
  - `MoveLinearAbsoluteEx` command `0x20A4` 파싱
  - `GroupReadStatus` command `0x2045` 응답
  - `LMCRobot.MoveLinearCoord(...)` 호출
  - `LMCRobot.AxInPosition(...)` 결과로 `0x00020000` in-position bit 설정
  - 위치 double 벡터 16개 중 `_LMCPROF_POS.Pos1..Pos9`에 매핑
  - 파일: [TCPMotionInterface.st](/C:/work/Elmo/Elmo_Master/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:383)
- `Codex_LASAL_WPF` 더미 MMCLib 그룹 래퍼가 실제 TCP 프레임을 보내도록 수정:
  - `GroupReadStatus` -> 16바이트 `0x2045` 요청, 20바이트 응답 파싱
  - `MoveLinearAbsoluteEx` -> 312바이트 `0x20A4` 요청
  - Move ACK를 읽어서 다음 `GroupReadStatus`가 stale ACK를 읽지 않게 처리
  - 파일: [SigmatekTcpIpDummyMMCLib.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1416)

검증:
- `git diff --check` 통과. CRLF/LF 경고 없게 맞췄습니다.
- `dotnet build Codex_LASAL_WPF/PmasApiWpfTestApp.sln -c Debug`는 실패했습니다. 실패 원인은 기존 WPF XAML generated field/`InitializeComponent` 미생성 관련 오류이고, 이번에 수정한 `SigmatekTcpIpDummyMMCLib.cs` 오류는 출력되지 않았습니다.
- LASAL IDE 빌드는 이 CLI 환경에서 실행할 수 없어 못 했습니다.

> 이거 왜이러지?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-54bb3275-0e9f-4b5d-a770-0ce1b1d5b7eb.png`

이 오류는 보통 원격 장비 문제가 아니라, 앱이 바인딩하려는 `Local IP`가 현재 PC NIC에 없는 주소일 때 납니다. 화면의 `Local IP=10.10.150.13`이 실제 Windows 네트워크 어댑터에 잡혀 있는지 바로 확인해보겠습니다.

> 이거 명령은 들어간거 같은데 위치값 속도 가속도 비율이 안맞아서 동작이 안되는거 같아. Codex_PMAS_WPF의 SingleAxis에 들어간 비율이랑, Codex_LASAL_WPF의 SingleAxis의 비율이 같은 거거든 실제 23bit count 증가할때 lasal에서는 360unit움직이는 걸로 되어있어. 이 비율로 해서 Group 탭이랑 실제로 Codex_LASAL_WPF에도 Group Test를 진행할꺼니까 Codex_PMAS_WPF에 구현된 내용을 참고해서 Codex_LASAL_WPF에도 똑같이 구현해줘.
>
> User context
>
> Attachments:
> - README.md: `./README.md`
> - AGENTS.md: `./AGENTS.md`
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-3f42a454-0d14-418b-9a70-365ca304343a.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-ca294c1c-0440-4e5a-b2f0-521b70ff129d.png`

<details><summary>44 previous messages</summary>

> 현재 문제는 명령 자체보다 단위 변환이 맞지 않는 쪽으로 보입니다. 먼저 `Codex_PMAS_WPF`의 Group Test 구현과 `Codex_LASAL_WPF`의 SingleAxis 단위 변환 위치를 대조해서, 같은 스케일을 Group 명령에도 적용하겠습니다.
>
> 작업 트리에 LASAL IDE가 만든 변경이 많이 섞여 있습니다. 이건 건드리지 않고, 이번 요청 범위인 `Codex_LASAL_WPF` 쪽 Group 단위 변환과 Group Test 이식만 수정하겠습니다.
>
> 사실 확인: `Codex_LASAL_WPF` 단축 `MoveAbsolute`는 입력값에 `*10000`을 적용하고 있습니다. Group 탭은 그 변환이 없어서 지금 `1000,2000,3000`이 그대로 LASAL application unit으로 들어갑니다. 이 부분이 우선 원인입니다.
>
> `Codex_PMAS_WPF`에는 `Cycle Test Group1` 전용 파일과 XAML 탭이 있고, `Codex_LASAL_WPF`에는 없습니다. 그래서 이식은 파일 복사만으로 끝나지 않고, `MainWindow.xaml`, `MainWindow.xaml.cs`, `.csproj`, Group scaling까지 같이 맞춰야 합니다.
>
> 화면의 LASAL 로봇 값도 같은 결론입니다. `End Point=1000,2000,3000`을 보냈더니 LASAL Destination은 `0.10, 0.20, 0.30`으로 보입니다. 즉 현재 Group 값은 raw application unit으로 들어가고 있고, PMAS count 기준 입력을 LASAL raw로 변환하지 않았습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 원인은 꽤 명확합니다. LASAL Group 명령 쪽은 UI 입력값을 그대로 TCP 프레임에 넣고 있어서 `1000`을 보내면 LASAL 화면에서는 `0.10` unit으로 보이는 상태입니다. SingleAxis 쪽처럼 PMAS count 기준 입력을 LASAL 내부 단위로 변환하고, 그 기준으로 Group Test 탭도 LASAL WPF에 붙이겠습니다.
>
> 확인 결과 PMAS WPF의 Group 탭 기본값은 count 단위인데, LASAL WPF Group 탭 기본값은 작은 LASAL raw 값으로 남아 있었습니다. 변환은 공통 전송 계층에 넣어서 일반 Group 탭과 새 Group Test 탭이 같은 기준으로 동작하게 하겠습니다.
>
> 이제 수정 들어갑니다. 핵심은 `23bit count = 360 LASAL unit`, 그리고 기존 SingleAxis UI가 `1 unit = 10000 내부값`으로 보내는 구조라서 Group 전송 직전에 `value * 360 * 10000 / 8388608`을 적용하는 방식입니다.
>
> PMAS 쪽 Group Test 코드는 928줄짜리 독립 partial 파일입니다. LASAL 쪽에 그대로 가져오되 LASAL에는 CycleTest3/4 상태 플래그가 없어서 그 참조는 제거하고, 상태 샘플 저장 제한 상수는 LASAL 쪽 파일 안에 새로 둡니다.
>
> 파일 편집을 시작합니다. 먼저 PMAS의 `Cycle Test Group1` UI/코드를 LASAL 프로젝트로 기계적으로 복사한 뒤, LASAL enum과 실행 상태 플래그 차이에 맞춰 좁게 패치합니다.
>
> 복사까지 끝났습니다. 이제 실제 동작에 영향을 주는 부분을 패치합니다: Group TCP 프레임의 위치/속도/가속도/감속/저크를 PMAS count 기준에서 LASAL 내부 단위로 변환하고, LASAL WPF 초기값도 PMAS WPF와 맞춥니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs` (+43 -20)
>
> </details>
>
> 변환 로직은 전송 직전에 적용되도록 넣었습니다. 이제 새 탭에서 쓰는 ComboBox 초기화와 프로젝트 컴파일 포함 목록을 맞춥니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+6 -0)
>
> </details>
>
> LASAL 프로젝트 파일에는 새 partial 파일이 자동 포함되지 않는 형식이라 직접 추가합니다. 그렇지 않으면 XAML 버튼 핸들러가 존재해도 빌드 대상에 안 들어갑니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj` (+3 -2)
>
> </details>
>
> 복사된 Group Test 코드는 PMAS에 있던 CycleTest3/4 실행 상태를 확인합니다. LASAL WPF에는 해당 탭이 없으므로 그 참조를 제거하고, 상태 샘플 저장 제한은 Group Test 파일 자체에 둡니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestGroupOperations.cs` (+13 -12)
>
> </details>
>
> 일반 Group 탭 기본값도 PMAS WPF와 동일한 count 기준으로 바꿉니다. 이렇게 해야 단발 `MoveLinearAbsoluteCmd/Ex` 테스트와 `Cycle Test Group1`이 같은 숫자 체계를 씁니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+5 -5)
>
> </details>
>
> 설계/운영 기준도 문서로 남깁니다. 나중에 LASAL Group Motion을 다시 만질 때 숫자 단위가 섞이지 않도록 변환 공식과 적용 지점을 명시하겠습니다.
>
> 새 탭과 변환 코드가 들어갔고, 빠른 검색상 누락된 컨트롤/PMAS 전용 플래그 참조는 없습니다. 지금 문서 추가 후 빌드와 diff 검사를 돌려서 실제 컴파일 문제를 잡겠습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/architecture/PMAS_LASAL_GroupMotion_Scaling_2026-06-17.md` (+81 -0)
>
> </details>
>
> 라인 엔딩도 맞춥니다. C#/XAML/프로젝트 파일은 `.gitattributes` 기준 CRLF, 새 문서는 LF로 정리해서 CRLF 경고가 다시 뜨지 않게 하겠습니다.
>
> 변환/탭 추가는 끝났고, 지금부터 검증입니다. 먼저 정적 diff 체크를 돌리고 그 다음 WPF 프로젝트 빌드를 시도하겠습니다.
>
> `git diff --check`는 깨끗합니다. 변경 통계는 기존 미커밋 TCP 구현 변경까지 같이 잡히고 있고, 새 `CycleTestGroupOperations`/문서는 아직 untracked라 stat에는 빠져 있습니다. 이제 빌드로 새 탭/핸들러 누락을 확인합니다.
>
> 빌드 대상 솔루션은 `Codex_LASAL_WPF\PmasApiWpfTestApp.sln`입니다. NuGet `packages.config`는 없어서 바로 MSBuild/dotnet 빌드를 시도합니다.
>
> `dotnet build`는 이 레거시 .NET Framework WPF 프로젝트에서 XAML generated code를 만들지 못해 기존 컨트롤 전부가 없다는 식으로 실패합니다. 이건 새 탭만의 오류가 아니라 `InitializeComponent`, 기존 `TextAxisName`, 기존 `ComboBufferedMode`까지 모두 같은 패턴입니다. Visual Studio MSBuild가 있으면 그쪽으로도 한 번 확인해보겠습니다.
>
> Visual Studio 2019 MSBuild가 설치되어 있습니다. `dotnet build` 대신 이 MSBuild로 다시 빌드해서 실제 WPF XAML 컴파일 경로를 확인합니다.
>
> VS MSBuild로 실제 오류가 잡혔습니다. 복사한 Group Test 코드가 PMAS 쪽 헬퍼 `EnsureGroupLoadedFromText`, `FormatVector`를 호출하는데 LASAL WPF에는 없는 상태입니다. 이 헬퍼를 LASAL 쪽 패턴에 맞춰 추가한 뒤 다시 빌드합니다.
>
> 헬퍼 위치를 확인했습니다. `EnsureGroupLoadedFromText`는 LASAL의 `Context.LoadGroup(...)` 패턴으로 추가하고, `FormatVector`는 저장 로그용 포맷 함수라 PMAS 구현을 그대로 가져오겠습니다.
>
> LASAL 컨텍스트에는 `UpdateGroupAxisNames`가 없고, `LoadGroup`만 있습니다. 그래서 Group Test 시작 시 UI의 Group Name/Group Axes를 기준으로 현재 Group이 없거나 이름/축 목록이 다르면 `LoadGroup`을 다시 호출하는 방식으로 구현합니다.
>
> 이제 누락 헬퍼를 실제 LASAL GroupOperations 파일에 추가합니다. 저장/로그 포맷은 PMAS와 같은 `0.###` 기준으로 맞춥니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs` (+27 -0)
>
> </details>
>
> 헬퍼 추가가 끝났습니다. 변경 파일 라인 엔딩을 다시 정리하고 VS MSBuild로 재빌드합니다.
>
> VS MSBuild 기준 빌드는 성공했습니다. 마지막으로 diff 체크와 상태를 확인해서 줄 끝/불필요한 파일 변경이 없는지 봅니다.
>
> 검증 결과 `git diff --check`는 통과했고, VS MSBuild 빌드도 경고/오류 0으로 통과했습니다. 현재 작업 트리에는 LASAL IDE가 만든 기존 변경들이 많이 남아 있어서, 이번 작업 파일과 기존 변경을 구분해서 최종 정리하겠습니다.
>
> 방금 라인 엔딩 정리 과정에서 일부 기존 C# 파일 앞에 UTF-8 BOM이 붙었습니다. 내용 변경은 아니지만 불필요한 diff라서 제거하고, CRLF는 유지하겠습니다.
>
> BOM 검출은 병렬 실행이라 제거 전 상태가 섞여 보였습니다. 다시 확인하고, 실제 diff에 BOM만 남아 있는지 확인합니다.
>
> 기존 XAML/프로젝트 파일은 원래 BOM이 있는 파일이라 그대로 두고, 이번에 BOM이 불필요하게 붙었던 기존 C# 두 파일만 제거했습니다. 이제 최종 빌드/체크를 한 번 더 돌립니다.
>
> 새로 복사한 `MainWindow.CycleTestGroupOperations.cs`도 원본 PMAS 파일은 BOM이 없어서 LASAL 쪽도 BOM 없이 맞춥니다.
>
> BOM/라인엔딩 정리가 끝났습니다. 최종 빌드를 다시 돌려서 파일 인코딩 정리 후에도 문제가 없는지 확인합니다.
>
> 최종 빌드와 diff check 모두 통과했습니다. 이제 변경 파일 목록만 다시 정리해서 답변하겠습니다.
</details>

맞아. LASAL Group 쪽은 `1000`을 보내면 LASAL 화면에서 `0.10`으로 보이는 상태였고, 이건 Group 프레임에 PMAS count 값을 LASAL 내부 단위 변환 없이 그대로 넣고 있었기 때문입니다.

반영 완료했습니다.

- [SigmatekTcpIpDummyMMCLib.cs](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs): Group `MoveLinearAbsoluteEx` 전송 직전에 변환 적용
  `LASAL internal = PMAS count * 360 * 10000 / 8388608`
- 위치 벡터, Velocity, Acceleration, Deceleration, Jerk 모두 같은 비율로 변환
- Transition Params는 모드별 파라미터라 count 값으로 확정할 수 없어서 변환하지 않음
- [MainWindow.xaml](./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml): Group 탭 기본값을 Codex_PMAS_WPF count 기준값으로 변경하고 `Cycle Test Group1` 탭 추가
- [MainWindow.CycleTestGroupOperations.cs](./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestGroupOperations.cs): PMAS의 Group1 Cycle Test 로직을 LASAL WPF에 맞게 추가
- [PMAS_LASAL_GroupMotion_Scaling_2026-06-17.md](./docs/architecture/PMAS_LASAL_GroupMotion_Scaling_2026-06-17.md): 변환 기준 문서화

검증했습니다.

```powershell
MSBuild.exe Codex_LASAL_WPF/PmasApiWpfTestApp.sln /p:Configuration=Debug /p:Platform="Any CPU" /m
```

결과: 경고 0개, 오류 0개.
`git diff --check`도 통과해서 CRLF 관련 경고는 없습니다.

참고로 현재 worktree에는 LASAL IDE가 만든 `Elmo_EtherCAT_Test_4Axis` 쪽 변경이 많이 남아 있습니다. 이번 작업에서는 그쪽은 건드리지 않았고 `Codex_LASAL_WPF` 중심으로만 반영했습니다.

> 이거 구현한거 동작시켜서 캡쳐했는데 원하는대로 구현이 됐는지 분석해봐
>
> User context
>
> Attachments:
> - GroupReadStatus_Sigmatek.pcapng: `./packet_capture/GroupReadStatus_Sigmatek.pcapng`
> - MoveLinearAbsoluteEx_Sigmatek.pcapng: `./packet_capture/MoveLinearAbsoluteEx_Sigmatek.pcapng`
> - GroupReadStatus.pcapng: `./packet_capture/GroupReadStatus.pcapng`
> - MoveLinearAbsoluteEx.pcapng: `./packet_capture/MoveLinearAbsoluteEx.pcapng`

<details><summary>7 previous messages</summary>

> 캡처 4개를 기준 캡처와 새 Sigmatek 캡처로 나눠서 비교하겠습니다. 먼저 패킷 개수, TCP payload, 명령 ID, 필드 오프셋 값을 뽑아서 실제 프레임이 우리가 의도한 스케일/구조로 나갔는지 확인합니다.
>
> 이 환경에는 `tshark/capinfos`가 없습니다. 대신 pcapng를 직접 파싱해서 Ethernet/IP/TCP payload를 추출하겠습니다. 작은 캡처라서 명령 프레임 비교에는 충분합니다.
>
> 캡처 파일은 정상 pcapng 헤더로 보입니다. 이제 Python으로 pcapng Enhanced Packet Block을 직접 읽고 TCP payload만 추출합니다.
>
> `scapy`가 설치되어 있어서 직접 파싱할 수 있습니다. TCP payload별로 source/destination/길이/hex를 뽑고, 16바이트 GroupReadStatus와 312바이트 MoveLinearAbsoluteEx 프레임을 필드 단위로 디코딩하겠습니다.
>
> 요청 프레임 생성 코드와 캡처 오프셋이 일치합니다. 이제 각 프레임을 구조체로 풀어서 `명령 ID / 길이 / 위치 16개 / 속도 계열 / transition / mode`를 숫자로 비교합니다.
>
> 숫자로 풀어보니 중요한 점이 하나 나왔습니다. MoveLinearAbsoluteEx 요청 프레임의 위치 스케일링은 의도대로 들어갔지만, GroupReadStatus 응답의 In-position 판정 비트는 기존 마스크 `0x00020000`로는 새 Sigmatek 응답에서 잡히지 않습니다. 관련 코드/문서에서 이 비트 의미를 한 번 더 확인하겠습니다.
>
> LASAL 쪽 `TCPMotionInterface.st`도 확인했습니다. 현재 서버 구현은 기본 상태 `0x40000000`에 `LMCRobot.AxInPosition(...)` 결과가 1일 때만 `0x00020000`을 OR 하도록 되어 있습니다. 이번 Sigmatek 캡처는 `0x40000000`만 왔기 때문에, 프레임은 맞지만 LASAL 로직상 아직 AxInPosition이 true가 아닙니다.
</details>

분석 결과, 결론은 이렇습니다.

**결론**
MoveLinearAbsolute 쪽 스케일 변환은 원하는 대로 구현됐습니다.
하지만 이번 `MoveLinearAbsoluteEx_Sigmatek.pcapng` 캡처는 이름과 달리 “풀 옵션 Ex 명령”이 아니라 `MoveLinearAbsoluteCmd` 래퍼 경로로 나간 패턴입니다.

**MoveLinearAbsoluteEx 캡처**
`MoveLinearAbsoluteEx_Sigmatek.pcapng` 요청:

```text
cmd = 0x20A4
length = 312 bytes
pos[0..3] = 3600000, 3600000, 3600000, 3600000
vel/acc/dec/jerk = 3600000, 3600000, 3600000, 0
buffered = 0
coord = 0
transition = 0
execute = 1
response ret = 0
```

여기서 핵심은 `8388608 count -> 3600000 LASAL internal -> 360.00 unit`으로 변환됐다는 점입니다.
즉 위치 스케일링은 맞습니다.

다만 acc/dec/jerk와 transition 값은 PMAS 기준 Ex 캡처와 다릅니다.

기존 `MoveLinearAbsoluteEx.pcapng` 기준:

```text
pos[0..3] = 8388608...
vel = 8388608
acc/dec = 8388608000
jerk = 8388608000000
transitionParams = 1,1,0,0
buffered = 2
transition = 1
```

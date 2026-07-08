<!--
Source: docs/history/Elmo_Master_history_260624.md
Original lines: 251-500
Part: 2 of 10
-->

# Elmo_Master_history_260624 part 02

Source lines: 251-500

>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestGroupOperations.cs` (+2 -2)
>
> </details>
>
> 코드 연결은 끝났고, 이제 정적 diff 체크와 VS2019 MSBuild로 컴파일 확인을 합니다. 컴파일 오류가 나오면 바로 패치합니다.
>
> 정적 체크는 whitespace 오류 없이 통과했고, 줄바꿈 경고만 있습니다. 이제 실제 WPF 솔루션을 빌드합니다.
>
> MSBuild Debug 빌드는 성공했습니다. 마지막으로 변경 파일 목록과 핵심 diff만 확인해서 빠진 파일이 없는지 보겠습니다.
>
> 빌드는 통과했습니다. 작업 중 생성하지 않은 untracked `Elmo_EtherCAT_Test_4Axis/` 폴더가 보이는데, 이번 변경과 무관하므로 건드리지 않았습니다.
</details>

구현 완료했습니다.

설계 문서 먼저 작성했습니다: [CycleTestGroup1_Design.md](./Codex_PMAS_WPF/CycleTestGroup1_Design.md:1)

주요 변경:
- `Cycle Test Group1` 탭 추가: [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1951)
- Group1 실행/저장/로그 로직 추가: [MainWindow.CycleTestGroupOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestGroupOperations.cs:109)
- `GroupReadStatus` 완료 조건은 기본 `NC_GROUP_STANDBY` 마스크 `0x00020000`: [MainWindow.CycleTestGroupOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestGroupOperations.cs:18)
- `GroupReadStatus` 호출 latency를 `GroupStatusReadSamples` 시트로 저장: [MainWindow.CycleTestGroupOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestGroupOperations.cs:917)
- 기존 Cycle Test 1~4와 Group1 동시 실행 방지 추가: [MainWindow.CycleTestOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:225)

블렌딩은 일반 모드에서는 안 됩니다. 일반 모드는 각 포인트마다 `GroupReadStatus`로 `STANDBY`를 기다리기 때문에 경로가 끊깁니다. 블렌딩 테스트는 새 탭에서 `Queue points for transition/blending` 체크 후, `MC_BUFFERED_MODE` 또는 `MC_BLENDING_*` buffered mode와 `MC_TM_CORNER_DISTANCE_MODE` 같은 non-none transition mode를 선택해서 P1/P2/P3/P4/P1을 먼저 큐잉하고 마지막 P1에서만 `GroupReadStatus`를 기다리게 했습니다.

검증:
- `git diff --check` 통과
- VS2019 MSBuild Debug 빌드 성공:
  `PmasApiWpfTestApp -> ...\bin\Debug\PmasApiWpfTestApp.exe`

참고: 작업 중 보인 untracked `Elmo_EtherCAT_Test_4Axis/` 폴더는 이번 변경과 무관해 건드리지 않았습니다. 커밋은 이번 요청에 포함되지 않아서 만들지 않았습니다.

> 니 수정하고 cr lf 뜨는거 안뜨게해줘

<details><summary>3 previous messages</summary>

> CRLF 경고는 내용 문제가 아니라 수정된 파일의 working-tree 줄바꿈이 LF로 남아서 생긴 겁니다. 변경 파일들을 CRLF로 정규화하고 다시 `git diff --check`로 확인하겠습니다.
>
> 레포 설정은 명확합니다: `*.cs`, `*.xaml`, `*.csproj`는 CRLF, `*.md`는 LF입니다. 그래서 코드/XAML/프로젝트 파일만 CRLF로 바꾸고, 설계 md는 LF 유지합니다.
>
> 줄바꿈 정규화는 끝났습니다. 이제 경고가 사라졌는지 `git diff --check`와 빌드를 다시 확인합니다.
</details>

CRLF 경고 안 뜨게 정리했습니다.

- `*.cs`, `*.xaml`, `*.csproj`는 `.gitattributes` 기준대로 CRLF로 변환
- `CycleTestGroup1_Design.md`는 md 규칙대로 LF 유지
- `git diff --check` 재실행 결과 경고 없음
- VS2019 MSBuild Debug 빌드도 성공

무관한 untracked `Elmo_EtherCAT_Test_4Axis/`는 그대로 둔 상태입니다.

> 요값들을 프로그램 초기값으로 넣어줘
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-5ba5ca2d-18da-4c70-88d0-bedfd3546d7a.png`

<details><summary>5 previous messages</summary>

> 이미지에 보이는 값을 XAML 초기값으로 박겠습니다. 연결/그룹 기본값과 `Cycle Test Group1` 탭 기본값을 같이 맞추고, 줄바꿈 경고가 다시 생기지 않게 마지막에 정규화 후 빌드까지 확인합니다.
>
> 확인한 이미지 기준으로 이미 맞아있는 연결 기본값은 그대로 두고, Group1 탭 기본값만 실제로 다른 부분을 고칩니다. 특히 `Buffered Mode` 기본값은 이미지처럼 `MC_ABORTING_MODE`로 되돌립니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+10 -10)
> - Wrote `./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+1 -1)
>
> </details>
>
> 값 변경은 끝났습니다. 다시 CRLF 정규화하고 `diff --check` 및 빌드를 돌려서 경고/컴파일 문제를 확인합니다.
>
> 검증은 통과했습니다. 마지막으로 변경된 기본값이 코드에 정확히 들어갔는지 필요한 줄만 확인합니다.
</details>

이미지 값으로 `Cycle Test Group1` 초기값 맞췄습니다.

변경 위치:
- Group1 포인트/속도/jerk/cycle/timeout/poll/drop 기본값: [MainWindow.xaml](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1969)
- Buffered Mode 기본값 `MC_ABORTING_MODE`: [MainWindow.xaml.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:71)

이미 맞아있던 값은 그대로 뒀습니다: Remote/Local IP, Port, Axis `a01`, Group `v01`, Group Axes `a01,a02,a03,a04`, Acc/Dec, Transition Mode/Params, Superimposed, mask/check 옵션.

검증:
- `git diff --check` 경고 없음
- VS2019 MSBuild Debug 빌드 성공
- CRLF 경고 다시 안 뜨게 정규화 완료

> 폴더별로 다시 커밋 진행해

<details><summary>10 previous messages</summary>

> 현재 변경분을 폴더 단위로 확인한 뒤, 각 폴더별로 따로 stage/commit 하겠습니다. 무관한 파일인지 판단이 필요한 untracked 폴더는 먼저 목록만 확인합니다.
>
> 현재 변경은 세 묶음으로 나누는 게 맞습니다: `Codex_PMAS_WPF` 앱/설계 변경, `Codex_PMAS_WPF/Reports/CycleResultsGroup1` 결과 파일, `Elmo_EtherCAT_Test_4Axis` 신규 프로젝트 폴더. 이 순서로 따로 커밋하겠습니다.
>
> 먼저 PMAS WPF 앱 변경만 stage/commit 합니다. 테스트 결과 xlsx와 EtherCAT 신규 폴더는 아직 분리해서 남겨둡니다.
>
> PMAS 앱 변경만 staging 됐습니다. 커밋 전에 staged diff 범위를 확인하고 바로 커밋합니다.
>
> 첫 커밋 완료했습니다. 다음은 `Codex_PMAS_WPF/Reports/CycleResultsGroup1` 결과 파일만 별도 커밋합니다.
>
> 두 번째 커밋 완료했습니다. 마지막으로 `Elmo_EtherCAT_Test_4Axis` 전체 신규 폴더를 별도 커밋합니다. 파일 수가 508개, 약 195MB라 커밋 시간이 조금 걸릴 수 있습니다.
>
> `Elmo_EtherCAT_Test_4Axis` 안의 `.xml/.txt`는 `.gitattributes` 규칙상 LF가 맞는데 working tree가 CRLF라 경고가 났습니다. 이 폴더의 해당 파일들을 LF로 정규화한 뒤 다시 stage합니다.
>
> 줄바꿈 정규화 후 다시 stage했습니다. 이제 staged 상태와 체크를 확인하고 마지막 커밋을 진행합니다.
>
> `Elmo_EtherCAT_Test_4Axis`의 `.st` 파일들은 생성 코드라 trailing whitespace가 대량으로 있습니다. 프로젝트 원본을 훼손하지 않기 위해 whitespace 정리는 하지 않고 그대로 커밋합니다. CRLF/LF 경고는 정리했고, 이제 커밋합니다.
>
> 세 번째 커밋까지 완료했습니다. 마지막으로 워킹트리 상태와 최근 커밋 3개를 확인합니다.
</details>

폴더별로 커밋 완료했습니다. 워킹트리 깨끗합니다.

커밋 3개:
- `b921107 feat(pmas): add group motion cycle test`
  - `Codex_PMAS_WPF` 앱/설계 변경
- `363b25a test(pmas): add group cycle result captures`
  - `Codex_PMAS_WPF/Reports/CycleResultsGroup1` 결과 xlsx
- `5d40d31 feat(ethercat): add 4-axis test project`
  - `Elmo_EtherCAT_Test_4Axis` 신규 프로젝트 폴더

참고: `Elmo_EtherCAT_Test_4Axis` 안의 생성 `.st` 파일들은 trailing whitespace가 많지만, 생성 프로젝트 원본 보존을 위해 손대지 않았습니다.

> Elmo_EtherCAT_Test_4Axis의 lasal 프로그램의 network의 Motion_Network의 Elmo_11이랑 PosController1 _LMCAxis1 연결도를 참고해서 아래 추가된 3개더의 연결도도 똑같이 맞춰줘

<details><summary>10 previous messages</summary>

> `Motion_Network` 안의 연결 정의를 먼저 직접 확인하겠습니다. 기준으로 삼으라는 `Elmo_11`, `PosController1`, `_LMCAxis1` 항목이 어느 파일에 들어있는지 찾아서 같은 패턴으로 2~4번 축을 맞추겠습니다.
>
> `Motion_Network.lcn`이 XML 텍스트이고, 연결도도 그 안에 있습니다. 기준 축은 `Elmo_11`, `PosController1`, `_LMCAxis1`만 실제 연결되어 있고, 2~4축은 오브젝트는 있지만 동일 연결이 비어있는 상태인지 확인합니다.
>
> 확인 결과 2~4축 연결 자체는 이미 들어가 있지만, 1축처럼 꺾임 좌표(`Vertices`)가 정리된 연결도가 아니라 직선/대충 연결입니다. 요청은 연결 “모양”까지 1축 기준으로 맞추라는 의미로 보고, 2~4축의 `Vertices`를 1축 패턴의 Y 오프셋 복제로 수정하겠습니다.
>
> 좌표 기준을 보면 1축 연결의 꺾임 패턴은 각 축 Y 위치에 일정 오프셋을 더하면 그대로 재사용할 수 있습니다. 단 2축은 Y 오프셋이 1860, 3축은 3660, 4축은 5460이고, 3/4축 컨트롤러와 LMC 축은 X가 30px 왼쪽이라 그 부분은 현재 배치에 맞춰 유지합니다.
>
> 수정 대상은 `Motion_Network.lcn`의 최상위 `<Connections>`입니다. `ONE_Motion_Network_Table.st/.lba/.lob`는 생성 테이블이고 연결도 좌표가 아니라 컴파일 산출물 성격이라 이번에는 원본 네트워크 XML만 건드립니다.
>
> 이제 2~4축의 8개 연결씩, 총 24개 `Vertices`를 1축과 같은 꺾임 구조로 바꿉니다. 연결 Source/Destination은 그대로 두고 선 경로 좌표만 수정합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn` (+24 -24)
>
> </details>
>
> 수정은 완료했습니다. 이제 XML 파싱으로 파일이 깨지지 않았는지 확인하고, 변경된 연결 2~4축 경로만 다시 출력해서 확인합니다.
>
> 수정 후 `Motion_Network.lcn` 줄바꿈이 LF로 섞여 Git CRLF 경고가 났습니다. 파일 전체를 CRLF로 정규화하고 다시 확인하겠습니다.
>
> 검증 결과 XML 파싱은 정상이고, 최상위 connection 수는 그대로 42개입니다. 변경은 `Motion_Network.lcn` 1개 파일에서 2~4축 연결선 좌표만 바뀐 상태입니다.
</details>

수정했습니다.

변경 파일:
- [Motion_Network.lcn](./Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn:3181)

내용:
- `PosController2/3/4 -> Elmo_21/31/41`
- `_LMCAxis2/3/4 -> Elmo_21/31/41`
- 위 연결들의 `Vertices`를 `PosController1`, `_LMCAxis1`, `Elmo_11` 연결도와 같은 꺾임 패턴으로 맞췄습니다.
- Source/Destination 연결 자체는 변경하지 않았고, 연결선 경로 좌표만 정리했습니다.

확인:
- XML 파싱 정상
- 최상위 Connection 수 42개 유지
- `git diff --check` 경고 없음
- CRLF 경고 안 뜨게 `Motion_Network.lcn` CRLF로 정규화 완료

커밋은 아직 안 했습니다.

> MoveLinearAbsoluteEx 수행한 결과 값인데 이거 자세하게 패킷 분석좀 진행해봐.
>
> User context
>
> Attachments:
> - MoveLinearAbsoluteEx.pcapng: `./packet_capture/MoveLinearAbsoluteEx.pcapng`

<details><summary>14 previous messages</summary>

> 캡처 파일을 먼저 도구로 열어서 프로토콜/세션/패킷 수를 확인하겠습니다. `tshark`가 있으면 세부 필드까지 뽑고, 없으면 Python으로 pcapng를 파싱해서 TCP/UDP 흐름을 분석하겠습니다.
>
> `tshark/capinfos`는 경로에서 안 보입니다. 파일은 2.3KB로 작아서 직접 파싱해도 충분합니다. 현재 워킹트리에 LASAL 쪽 변경과 새 xlsx가 있지만 이번 분석과 무관하므로 건드리지 않겠습니다.
>
> Scapy는 설치되어 있습니다. 이제 pcapng를 Scapy로 읽어서 패킷 목록, 5-tuple 흐름, TCP payload, 원시 hex/ASCII를 뽑겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 패킷 캡처 자체는 아주 작습니다. `tshark`가 설치되어 있지 않아서 Scapy로 pcapng를 직접 파싱했고, 지금은 TCP 페이로드 안의 명령 프레임을 바이트 단위로 풀어서 `MoveLinearAbsoluteEx` 본문이 어디인지 확인하고 있습니다.
>
> 캡처 안에서 실제 `MoveLinearAbsoluteEx`는 거의 확실하게 5번 패킷입니다. 앞뒤 16바이트 요청은 상태/좌표 읽기 계열이고, 5번만 312바이트 명령 본문을 보내며 응답도 16바이트 즉시 ACK성 결과로 돌아옵니다.
>
> 명령 본문에서 눈에 띄는 값은 `8388608`, `8388608000`, `8.388608e12`, enum 값 `2`, `1`, `0x100`입니다. 즉 캡처 당시 명령 파라미터가 화면 스크린샷의 `83886080/83886080000`과 10배 또는 100배 차이가 나는 항목이 있어서, 이 부분은 “현재 캡처에 실제 들어간 값” 기준으로 분리해서 봐야 합니다.
>
> 문서 구조와도 맞았습니다. 5번 패킷의 304바이트 본문은 `position[16]`, 속도/가속/감속/저크, `transition[16]`, 좌표계/transition/buffer enum, `superimposed/execute` 순서로 해석하면 바이트가 정확히 떨어집니다.
>
> 앞뒤 4개 요청도 앱 코드 흐름과 맞습니다. `MoveLinearAbsoluteEx` 버튼 경로는 명령 전후에 `ReadGroupPositionSnapshot`을 호출하고, 그 안에서 Actual/Target 그룹 위치를 읽기 때문에 16바이트 요청 + 144바이트 응답이 두 쌍씩 붙어 있습니다.
>
> 커맨드 ID도 .NET decompile 자료에서 확인됐습니다. `0x2051`은 `GroupReadActualPosition`, `0x213B`는 `GroupReadTargetPosition`이고, 그래서 캡처는 “Actual 읽기 → Target 읽기 → MoveLinearAbsoluteEx → Actual 읽기 → Target 읽기” 순서로 보입니다.
>
> `0x20A4`도 확인됐습니다. 로컬 NetHelp의 `IDCommands.cs`에 `MC_MOVELINEARABSOLUTEEX_ID = 8356 (0x20A4)`로 정의되어 있어 5번 패킷은 이름 그대로 `MoveLinearAbsoluteEx`입니다.
>
> 응답 6번도 NetHelp 구조와 일치합니다. `MoveLinearAbsoluteExInner`는 `DefaultOutputFBArgs`를 쓰고, 이 응답은 8바이트 body로 `uiHndl/status/error`를 돌려주는 형태입니다.
>
> 주의할 점 하나 있습니다. 요청 헤더의 axis/group reference 2바이트는 `00 01`로 들어가는데, NetHelp `DataOut()`이 `AxisReference`에 swap을 적용해서 넣습니다. 그래서 바이트 그대로 little-endian 숫자 `256`으로 읽으면 안 되고, 라이브러리 관점에서는 group/axis ref `1`로 보는 게 맞습니다.
>
> 이 캡처로는 “명령 수락”과 “초기 setpoint 변화”까지만 볼 수 있고, 모션 완료 여부는 판단할 수 없습니다. 캡처가 명령 후 약 60ms 이내에 끝나며 `GroupReadStatus`나 충분한 후속 position polling이 포함되어 있지 않습니다.
</details>

[MoveLinearAbsoluteEx.pcapng](/C:/work/Elmo/Elmo_Master/packet_capture/MoveLinearAbsoluteEx.pcapng) 분석 결과입니다.

**핵심 결론**
- 실제 `MoveLinearAbsoluteEx` 명령은 **5번 패킷**입니다.
- Command ID는 `0x20A4` = `8356` = `MC_MOVELINEARABSOLUTEEX_ID`입니다.
- 응답 6번에서 `status=0`, `error=0`이라 **명령은 정상 수락**됐습니다.
- 단, 이 응답은 “모션 완료”가 아니라 **비동기 명령 접수 완료**입니다. 캡처 안에는 `GroupReadStatus`가 없어서 InPosition/완료 여부는 판단할 수 없습니다.
- 통신 품질은 정상입니다. TCP retransmission/gap 없이 sequence가 연속이고, 요청-응답 RTT 평균은 약 **0.145 ms**입니다.

**패킷 순서**
| Frame | 방향 | 의미 | RTT |
|---:|---|---|---:|
| 1 -> 2 | PC -> PMAS | `GroupReadActualPosition`, ID `0x2051` | 0.155 ms |
| 3 -> 4 | PC -> PMAS | `GroupReadTargetPosition`, ID `0x213B` | 0.136 ms |
| 5 -> 6 | PC -> PMAS | `MoveLinearAbsoluteEx`, ID `0x20A4` | 0.146 ms |
| 7 -> 8 | PC -> PMAS | `GroupReadActualPosition` after command | 0.147 ms |
| 9 -> 10 | PC -> PMAS | `GroupReadTargetPosition` after command | 0.142 ms |
| 11 | PC -> PMAS | 순수 TCP ACK | - |

**MoveLinearAbsoluteEx Payload**
5번 패킷 body 길이는 `304 bytes`입니다. NetHelp의 `MoveLinerarAbsoluteExArgsIN.DataOut()` 구조와 정확히 맞습니다.

| Offset | Field | 값 |
|---:|---|---|
| `0x000` | `position[0..3]` | `8388608, 8388608, 8388608, 8388608` |
| `0x020` | `position[4..15]` | all `0` |
| `0x080` | `velocity` | `8388608.0` |
| `0x088` | `acceleration` | `8388608000.0` |
| `0x090` | `deceleration` | `8388608000.0` |
| `0x098` | `jerk` | `8388608000000.0` |
| `0x0A0` | `transition[0..1]` | `1.0, 1.0` |
| `0x0B0` | `transition[2..15]` | all `0` |

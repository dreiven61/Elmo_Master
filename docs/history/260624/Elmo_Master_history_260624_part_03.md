<!--
Source: docs/history/Elmo_Master_history_260624.md
Original lines: 501-750
Part: 3 of 10
-->

# Elmo_Master_history_260624 part 03

Source lines: 501-750

| `0x120` | `coordSystem` | `2` = `MC_MCS_COORD` |
| `0x124` | `transitionMode` | `0` = `MC_TM_NONE_MODE` |
| `0x128` | `bufferMode` | `1` = `MC_ABORTING_MODE` |
| `0x12C` | `superimposed` | `0` |
| `0x12D` | `execute` | `1` |

중요한 점: 이 pcap에 실제 전송된 `velocity`는 `8388608`입니다. 이전 스크린샷의 `83886080`과 다릅니다. `jerk`도 캡처 기준으로는 `8388608000000`입니다.

**Position 변화**
명령 전:
- Actual: `[16777230, 16777300, 16777089, 16777220]`
- Target: `[16777216, 16777216, 16777216, 16777216]`

명령 후:
- Actual 응답은 명령 응답 후 약 **5.024 ms** 뒤 도착: `[16777161, 16777118, 16777131, 16777145]`
- Target 응답은 명령 응답 후 약 **10.015 ms** 뒤 도착: `[16733627.729, 16733627.729, 16733627.729, 16733627.729]`

즉, 컨트롤러의 target setpoint는 명령 후 이미 `16777216`에서 `16733627.729`로 약 `-43588.271` 이동했습니다. Planner가 동작을 시작한 흔적은 있습니다. 하지만 실제 축이 목표점까지 갔는지, InPosition이 됐는지는 이 캡처만으로는 증명 불가입니다.

근거 파일:
- [MoveLinerarAbsoluteExArgsIN.cs](</C:/work/Elmo/Elmo_Master/NetHelp/Elmo Maestro API Help/MMCLibDotNET/ElmoMotionControlComponents.GMAS.MMCLibDotNET/ElmoMotionControl/GMAS/EASComponents/MMCLibDotNET/InternalArgs/MoveLinerarAbsoluteExArgsIN.cs:29>)
- [DefaultOutputFBArgs.cs](</C:/work/Elmo/Elmo_Master/NetHelp/Elmo Maestro API Help/MMCLibDotNET/ElmoMotionControlComponents.GMAS.MMCLibDotNET/ElmoMotionControl/GMAS/EASComponents/MMCLibDotNET/InternalArgs/DefaultOutputFBArgs.cs:55>)
- [MainWindow.GroupOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs:277)

> 지금 이 탭에서 MoveLinearAbsoluteCmd를 호출하면 저렇게 명령이 3개가 나가는거임? 아니면 dll에서 명령이 3개가 나간다고 예상할 수있는거임?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-910e61cb-176a-4bcb-884b-a964935dd9dc.png`

<details><summary>1 previous message</summary>

> 현재 코드 기준으로 답하겠습니다. 방금 캡처와 로그 흐름은 DLL이 몰래 여러 명령을 추가 전송한 게 아니라, WPF 탭의 버튼 핸들러가 진단용 Read를 앞뒤로 붙여서 보낸 흐름입니다.
</details>

정확히 말하면 **DLL이 MoveLinearAbsoluteCmd 하나를 호출했는데 내부에서 3개를 자동으로 날린다고 보면 안 됩니다.**

현재 WPF 탭 코드가 이렇게 되어 있습니다:

1. `Before ...` 로그용 `GroupReadActualPosition`
2. `Before ...` 로그용 `GroupReadTargetPosition`
3. 실제 이송 명령 `MoveLinearAbsoluteEx`
4. `After ...` 로그용 `GroupReadActualPosition`
5. `After ...` 로그용 `GroupReadTargetPosition`

그래서 pcap에는 최소 **5개 API 요청**이 보이는 게 정상입니다.

그리고 더 중요한 점은, 지금 `MMCMoveLinearAbsoluteCmd` 버튼도 내부적으로는 단순 `MoveLinearAbsolute`가 아니라 `MoveLinearAbsoluteEx`를 호출합니다. 코드상 [MainWindow.GroupOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs:180)에서 버튼이 `RunMoveLinearAbsoluteWithUiParameters("MMC_MoveLinearAbsoluteCmd")`로 들어가고, 실제 호출은 [같은 파일 279라인](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs:279)의 `Context.GroupAxis.MoveLinearAbsoluteEx(...)`입니다.

즉 결론은:

- **이 탭에서 버튼 누르면 여러 패킷이 나가는 게 맞다.**
- 그 이유는 **우리 앱 코드가 전/후 위치 확인 Read를 붙였기 때문**이다.
- **DLL이 Move 명령 하나를 3개 명령으로 쪼개서 보내는 증거는 아니다.**
- 순수 이송 명령만 보려면 `ReadGroupPositionSnapshot("Before...")`, `ReadGroupPositionSnapshot("After...")`를 빼고 캡처해야 한다.

> 왜 코드를 그런식으로 만들었지? 굳이 명령 나가기전에 위치값을 읽어올 필요가 없는데... 원래 elmo 예제가 그렇게 구현되어 있나?

<details><summary>1 previous message</summary>

> 이건 원본 API 예제 패턴인지 확인해서 말하겠습니다. 현재 의심은 예제 기반이 아니라, 이전에 좌표계/kinematic 문제를 추적하려고 내가 앱 쪽에 넣은 진단용 Read입니다.
</details>

아닙니다. **원래 Elmo 예제가 그렇게 구현되어 있는 건 아닙니다.**

Elmo 매뉴얼 예제는 보통:

1. 구조체 파라미터 채움
2. `MMC_MoveLinearAbsoluteCmd(...)` 호출
3. 반환값 에러 체크

이 흐름입니다. 명령 전에 `GroupReadActualPosition`이나 `GroupReadTargetPosition`을 자동으로 읽는 패턴은 아닙니다.

지금 앱 코드가 그렇게 된 이유는 예제 때문이 아니라, 이전에 `MoveLinear`가 안 움직이거나 `MCS/kinematic/coord system` 문제가 있었을 때 원인 추적하려고 내가 진단 로그를 넣어둔 겁니다.

현재 코드 흐름은 여기입니다:

- [MainWindow.GroupOperations.cs:277](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs:277)
  `Before ... ReadGroupPositionSnapshot`
- [MainWindow.GroupOperations.cs:279](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs:279)
  실제 `MoveLinearAbsoluteEx`
- [MainWindow.GroupOperations.cs:293](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs:293)
  `After ... ReadGroupPositionSnapshot`

그리고 `ReadGroupPositionSnapshot` 안에서:

- `GroupReadActualPosition`
- `GroupReadTargetPosition`

두 개를 호출합니다.

결론: **테스트/패킷 분석 관점에서는 지금 구조가 불필요하게 트래픽을 오염시키고 있습니다.**
이건 순수 명령 테스트용 탭에서는 빼는 게 맞습니다. 위치 확인은 별도 `Read Group Pos` 버튼이나 옵션 체크박스로 남기고, `MoveLinearAbsoluteCmd/ExCmd` 버튼은 이송 명령 하나만 나가게 하는 게 맞습니다.

> ㅇㅋ 그럼 일단 두고 MoveLinearAbsoluteCmd만 분석하자

<details><summary>2 previous messages</summary>

> 좋습니다. 그러면 앞뒤 `ReadGroupPositionSnapshot` 패킷은 분석 대상에서 제외하고, 버튼 클릭 중 실제 이송 명령 패킷 하나만 기준으로 정리하겠습니다. 단, 현재 버튼명과 실제 호출 API가 다르기 때문에 그 구분은 먼저 확정해야 합니다.
>
> 분석 기준을 두 개로 나눠야 합니다. 현재 앱 버튼명이 `MMC_MoveLinearAbsoluteCmd`여도 실제 패킷은 `MoveLinearAbsoluteEx(0x20A4)`이고, 진짜 non-Ex `MoveLinearAbsoluteCmd`는 ID가 `0x2043`으로 별도입니다.
</details>

응. “앞뒤 Read는 무시하고 이송 명령만” 보면 현재 캡처의 핵심은 **5번 패킷 하나**입니다.

단, 정확히 구분해야 합니다.

현재 앱의 `MMC_MoveLinearAbsoluteCmd` 버튼은 이름만 그렇게 찍히고, 실제 코드는 `MoveLinearAbsoluteEx(...)`를 호출합니다. 그래서 현재 pcap의 실제 이송 명령은:

- 실제 Command ID: `0x20A4`
- 이름: `MC_MOVELINEARABSOLUTEEX_ID`
- TCP payload: `312 bytes`
- body: `304 bytes`
- 응답: 16 bytes, `uiHndl=2748944`, `status=0`, `error=0`
- 결과: **명령 정상 수락**

현재 캡처 기준 이송 명령 파라미터는:

```text
Position[0..3] = 8388608, 8388608, 8388608, 8388608
Position[4..15] = 0

Velocity     = 8388608
Acceleration = 8388608000
Deceleration = 8388608000
Jerk         = 8388608000000

TransitionParam[0..1] = 1, 1
TransitionParam[2..15] = 0

CoordSystem     = 2 = MC_MCS_COORD
TransitionMode  = 0 = MC_TM_NONE_MODE
BufferMode      = 1 = MC_ABORTING_MODE
Superimposed    = 0
Execute         = 1
```

진짜 non-Ex `MoveLinearAbsoluteCmd`라면 다릅니다.

- 진짜 Command ID: `0x2043`
- 내부 클래스: `MoveLinerarAbsoluteArgsIN`
- payload 길이 예상: `232 bytes`
- `Velocity/Acceleration/Deceleration/Jerk`는 `double`이 아니라 **float 4바이트**
- `TransitionParameter[16]`도 **float 4바이트**
- Ex 버전보다 payload가 작음

즉 지금 캡처 파일로는 **진짜 `0x2043 MoveLinearAbsoluteCmd`를 분석한 게 아니라**, 현재 앱 버튼이 보낸 **`0x20A4 MoveLinearAbsoluteEx` 명령만 분석한 것**입니다.
지금 단계에서는 그걸 기준으로 보면 됩니다.

> 이거 lasal 소스코드 수정한거 gitignore가 반영이 안되어있네.
> lasal 프로그래밍 하는 방법 스터디해서 이 폴더에 md파일 작성해서 lasal 프로그래밍 할때 마다 참조할 수 있도록 코딩규칙을 만들어
>
> User context
>
> Attachments:
> - AGENTS.md: `C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/AGENTS.md`
> - README.md: `C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/README.md`
> - PMAS_LASAL_Integrated_Analysis_2026-04-10.md: `C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/doc/PMAS_LASAL_Integrated_Analysis_2026-04-10.md`
> - .gitignore: `C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/.gitignore`
> - SIGMATEK_LASAL_programming_method_study.md: `C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/doc/architecture/SIGMATEK_LASAL_programming_method_study.md`

<details><summary>20 previous messages</summary>

> SEMICS repo 기준으로 확인하겠습니다. 먼저 `AGENTS.md`, 현재 `.gitignore`, git 상태, 기존 LASAL 스터디 문서를 같이 읽고 실제로 어떤 파일들이 추적/무시 대상인지 분리하겠습니다.
>
> 확인해보니 `doc/architecture/SIGMATEK_LASAL_programming_method_study.md`는 이미 로컬에 있지만 아직 Git 추적 전 상태입니다. 지금은 이 스터디 문서를 근거로, 실제 작업 때 바로 볼 수 있는 별도 “코딩 규칙” 문서를 만들고 `.gitignore` 쪽은 추적 파일/무시 파일을 분리해서 정리하겠습니다.
>
> `.gitignore` 자체는 `ProjectInternal/`, `*.lba`, `*.lob`, `*.ldi`, `*.bin` 같은 LASAL 내부 상태는 이미 막고 있습니다. 다만 `.lcb/.lcp/.lcn`은 이미 Git에 추적 중인 파일이라 `.gitignore`로는 수정 표시가 사라지지 않습니다. 이건 무시 규칙 문제가 아니라 “이미 tracked 된 파일” 문제입니다.
>
> 정리 방향은 이렇습니다. `.st/.h/.cpp`는 LASAL 소스라 추적 대상이고, `.lba/.lob/.ldi/.lhd/.bin/ProjectInternal`은 생성물이라 무시 대상입니다. `.lcb/.lcp/.lcn`은 이미 추적 중이라 지금 상태에서는 `.gitignore`만으로 숨길 수 없습니다.
>
> 이제 문서 파일을 새로 추가하겠습니다. 기존 스터디 문서는 길고 배경 분석 중심이라, 실제 코딩 전에 체크할 규칙만 압축한 문서를 `doc/architecture` 아래에 별도로 두겠습니다. .gitignore에는 추적 파일과 무시 파일 기준을 주석/패턴으로 명확히 보강합니다.
>
> 파일 편집 들어갑니다. `.gitignore`에는 누락된 LASAL 임시/로컬 상태 패턴을 보강하되 `.st/.h/.cpp` 같은 실제 소스와 이미 추적 중인 프로젝트 파일은 건드리지 않겠습니다. 새 문서는 매번 LASAL 작업 전에 볼 수 있는 체크리스트형 규칙으로 작성합니다.
>
> <details><summary>Created a file, edited a file</summary>
>
> - Wrote `C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/.gitignore` (+12 -0)
> - Wrote `C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/doc/architecture/SIGMATEK_LASAL_coding_rules.md` (+119 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 검증만 이어서 진행하겠습니다. 지금은 `.gitignore`가 실제로 어떤 파일을 잡는지와 새 규칙 문서/라인엔딩 상태를 확인하는 단계입니다.
>
> 방금 확인 결과 `.gitignore`는 LF 상태인데 Git 설정 때문에 “다음에 CRLF로 바뀐다” 경고가 뜹니다. 이건 파일 내용 문제가 아니라 속성/설정 문제라서, 저장소 안에서 명시적으로 라인엔딩 정책을 잡아 경고가 안 뜨게 정리하겠습니다.
>
> 현재 저장소 설정은 전역 `core.autocrlf=true`이고, `.gitattributes`가 텍스트 파일을 `eol` 없이 관리해서 LF 파일을 수정하면 경고가 납니다. `.gitignore`/문서 계열은 LF 고정으로 명시하는 쪽이 맞습니다.
>
> 이제 `.gitattributes`에 저장소 메타/문서 파일은 LF로 고정한다고 명시하겠습니다. 범위는 이번 변경과 문서 작업에 직접 관련된 파일군으로 제한합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/.gitattributes` (+4 -1)
>
> </details>
>
> 라인엔딩 정책을 반영했습니다. 이제 같은 `git diff` 계열 명령에서 경고가 사라졌는지 확인하겠습니다.
>
> `git diff --check`는 통과했고, 방금 보이던 CRLF 경고도 없어졌습니다. 하나의 확인 스크립트만 문법 오류라서 라인엔딩 카운트와 ignore 매칭을 다시 돌립니다.
>
> 현재 새 규칙 문서만 만들면 “매번 참조”가 자동으로 보장되지는 않습니다. 그래서 루트 `AGENTS.md`에 LASAL 작업 전 이 규칙 문서를 먼저 보라는 항목을 추가하는 게 맞습니다.
>
> `AGENTS.md`는 아직 수정되지 않았고, `README.md`만 기존 변경 상태입니다. 루트 작업 규칙에 새 LASAL 코딩 규칙 문서를 기준점으로 추가하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/AGENTS.md` (+3 -0)
>
> </details>
>
> `AGENTS.md`까지 연결했습니다. 마지막으로 변경 파일 전체에 대해 diff/check와 라인엔딩을 다시 확인합니다.
>
> 검증 결과 변경 파일들은 모두 LF 상태이고, `git diff --check`도 깨끗합니다. 이제 전체 status만 확인해서 내가 건드린 파일과 기존 작업물을 분리해서 보고하겠습니다.
</details>

정리 완료했습니다.

변경한 파일:
- [`.gitignore`](C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/.gitignore): LASAL 임시/생성물 패턴 추가 (`*.lcc`, `*.lst`, `*.map`, `*.err`, `*.tmp`, `*.bak`, `*.lock` 등)
- [`.gitattributes`](C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/.gitattributes): `.gitignore`, `.gitattributes`, `*.md`를 LF 고정으로 설정해서 CRLF 경고 안 뜨게 처리
- [`AGENTS.md`](C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/AGENTS.md): LASAL 작업 시 코딩 규칙 문서를 먼저 참조하도록 추가
- [`SIGMATEK_LASAL_coding_rules.md`](C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/doc/architecture/SIGMATEK_LASAL_coding_rules.md): LASAL 코딩 규칙 문서 신규 작성

확인 결과:
- `git diff --check` 통과
- 변경 파일 모두 `CRLF=0`, `LF` 상태
- `.gitignore` 테스트에서 `*.tmp`, `*.bak`, `*.lcc`, `*.lst`, `*.map`, `ProjectInternal/` 정상 ignore 확인

정확히 말하면, 현재 status에 보이는 `*.lcb`, `*.lcp`, `*.lcn` 같은 파일은 이미 Git에 tracked 상태라서 `.gitignore`로 숨겨지지 않습니다. 이건 `.gitignore` 미반영이 아니라 Git index 정책 문제입니다. 추적을 끊으려면 별도로 `git rm --cached`가 필요하지만, LASAL 프로젝트 등록/네트워크 파일이라 임의로 제거하지 않았습니다.

커밋은 하지 않았습니다.

> 내가 수정한 내용 포함해서 git에 commit하자

<details><summary>18 previous messages</summary>

> 전체 작업 트리를 확인한 뒤, ignored 파일은 제외하고 Git이 보는 변경분 전체를 스테이징해서 커밋하겠습니다. 사용자 수정분까지 포함하라는 요청이므로 이번에는 범위를 나누지 않습니다.
>
> `git diff --check`에서 사용자 수정 파일 하나에 trailing whitespace가 걸렸습니다. 기능 변경은 아니고 커밋 품질 문제라 해당 공백만 제거한 뒤 진행하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `C:/Users/dreiv/source/repos/SIGMATEK/SEMICS/PLC/SEMICS_PROGRAM/Include/C_channels.h` (+1 -1)
>

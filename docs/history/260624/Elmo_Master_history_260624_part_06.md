<!--
Source: docs/history/Elmo_Master_history_260624.md
Original lines: 1251-1500
Part: 6 of 10
-->

# Elmo_Master_history_260624 part 06

Source lines: 1251-1500

새 Sigmatek 캡처는:

```text
acc = velocity와 동일
dec = velocity와 동일
jerk = 0
transitionParams = 0...
transition = 0
```

이건 `MMCMoveLinearAbsoluteCmd`가 내부적으로 `MoveLinearAbsoluteEx` 프레임을 만든 형태와 일치합니다. 풀 파라미터 테스트를 보려면 `MMCMoveLinearAbsoluteExCmd` 버튼으로 다시 캡처해야 합니다.

**GroupReadStatus 캡처**
요청 프레임은 기준 캡처와 완전히 동일합니다.

```text
cmd = 0x2045
ref = 0
payloadLen = 8
arg0 = 0x00000100
execute = 1
```

응답은 다릅니다.

기준:

```text
status = 0x40020000
groupErrorId = 0
```

새 Sigmatek:

```text
status = 0x40000000
groupErrorId = 0
```

현재 Cycle Test Group1 기본 In-position mask는 `0x00020000`입니다.
새 캡처의 `0x40000000`에는 이 비트가 없습니다.

즉 지금 캡처 기준으로는:

```text
(status & 0x00020000) == 0x00020000  -> false
```

그래서 Cycle Test Group1이 이 상태만 계속 받으면 In-position으로 판단하지 못하고 timeout 납니다.

LASAL 코드상 원인은 명확합니다. `TCPMotionInterface.st`에서:

```text
GroupReadState := 0x40000000;
GroupReadInPosition := LMCRobot.AxInPosition(... PositionWindow:=0);

if GroupReadInPosition <> 0 then
  GroupReadState := GroupReadState or 0x00020000;
end_if;
```

이번 캡처에서는 `LMCRobot.AxInPosition(...)`이 0을 반환한 겁니다. `PositionWindow:=0`이라 너무 엄격할 가능성이 큽니다.

정리하면:

- 위치/속도 count -> LASAL internal 변환: 정상
- GroupReadStatus 요청 구현: 정상
- GroupReadStatus 응답 파싱: 정상
- 현재 캡처의 In-position 판정: 실패 상태
- Ex 풀 파라미터 검증: 아직 안 됨, 이번 캡처는 simple MoveLinearAbsolute 경로로 보임

다음 확인은 `MMCMoveLinearAbsoluteExCmd`로 다시 캡처하고, GroupReadStatus가 이동 완료 후 `0x40020000`으로 바뀌는지 봐야 합니다. 바뀌지 않으면 LASAL 쪽 `AxInPosition`의 `PositionWindow:=0`을 현실적인 허용창으로 바꿔야 합니다.

> 이거 비율이 안맞다고 Codex_PMAS_WPF 8,388,608 값이 Codex_LASAL_WPF 360에 매칭된다고 다시 단위 수정해서 코드수정해

<details><summary>12 previous messages</summary>

> 맞습니다. 내가 직전 수정에서 LASAL WPF Group 입력값을 PMAS count 기준으로 두고 내부에서 변환했는데, 지금 요구는 그게 아닙니다. `Codex_PMAS_WPF 8,388,608 == Codex_LASAL_WPF 360`이면 LASAL WPF의 입력 기본값 자체가 `360` 단위여야 하고, 전송 시에는 기존 LASAL SingleAxis처럼 `*10000`만 적용해야 합니다. 바로 수정하겠습니다.
>
> 수정 범위는 세 군데입니다. Group TCP 프레임 스케일은 `PMAS count -> LASAL internal`이 아니라 `LASAL WPF 입력 unit -> LASAL internal(*10000)`로 바꾸고, LASAL WPF Group/Cycle Group1 기본값은 PMAS count 값을 360 기준 LASAL unit 값으로 변환해 넣겠습니다.
>
> 핵심 코드를 바꿉니다. 기존 `PmasCountToLasalInternalUnitScale`은 제거하고, LASAL WPF 입력값을 LASAL 표시 단위로 보고 `*10000`만 해서 TCP 프레임에 넣도록 바꾸겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs` (+27 -30)
>
> </details>
>
> 이제 XAML 기본값을 실제 LASAL unit 기준으로 바꿉니다. `8388608 -> 360`, `16777216 -> 720`, `25165824 -> 1080` 기준입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+12 -12)
>
> </details>
>
> 문서도 기존 설명이 반대로 되어 있어서 고칩니다. LASAL WPF 입력 단위와 TCP 내부 단위를 분리해서 기록하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/PMAS_LASAL_GroupMotion_Scaling_2026-06-17.md` (+22 -17)
>
> </details>
>
> 코드와 기본값 수정은 끝났습니다. 이제 관련 문서에서 남아 있는 잘못된 스케일 설명이 있는지 확인하고, 빌드/체크를 돌리겠습니다.
>
> `apply_patch` 때문에 C#/XAML 작업본이 LF로 바뀌었습니다. 이전에 요청한 CRLF 경고가 다시 뜨지 않게 C#/XAML은 CRLF로, md는 LF로 다시 정리한 뒤 빌드합니다.
>
> 단위 수정과 줄끝 정리가 끝났습니다. 이제 VS MSBuild와 `git diff --check`로 검증합니다.
>
> 빌드와 diff check 모두 통과했습니다. 마지막으로 실제 변경 위치만 다시 확인해서 정리하겠습니다.
</details>

수정했습니다. 이전 방식은 `Codex_LASAL_WPF` 입력값을 PMAS count처럼 보고 내부에서 `8388608 -> 3600000`으로 변환했는데, 이번 기준은 그게 아니라:

```text
Codex_PMAS_WPF 8,388,608 == Codex_LASAL_WPF 360
Codex_LASAL_WPF 360 -> TCP frame 3,600,000
```

으로 맞췄습니다.

변경 내용:
- [SigmatekTcpIpDummyMMCLib.cs](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1418)
  - Group Motion 전송 스케일을 `LASAL WPF 입력값 * 10000`으로 변경
  - `PmasCountToLasalInternalUnitScale` 제거
- [MainWindow.xaml](./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:777)
  - Group 탭 기본값:
    - Position `360,360,360,360`
    - Velocity `360`
    - Acc/Dec `360000`
    - Jerk `360000000`
  - Cycle Test Group1:
    - P2 `720,360,360,720`
    - P3 `360,720,720,360`
    - P4 `720,1080,1080,720`
    - Velocity `3600`, Acc/Dec `360000`, Jerk `3600000`
- 스케일 문서도 수정했습니다.

검증:
- VS2019 MSBuild 빌드 성공
- 경고 0개, 오류 0개
- `git diff --check` 통과
- C#/XAML CRLF 유지 확인 완료

> 폴더별로 정리해서 커밋하자

<details><summary>11 previous messages</summary>

> 현재 변경이 여러 최상위 폴더에 섞여 있으니 먼저 `git status` 기준으로 폴더별 묶음을 확인하겠습니다. 그 다음 각 폴더 단위로 stage/commit을 나눠서 진행합니다.
>
> 변경 묶음은 `Codex_LASAL_WPF`, `docs`, `Elmo_EtherCAT_Test_4Axis` 세 개입니다. 커밋 전에 `Elmo_EtherCAT_Test_4Axis` 쪽 텍스트 파일들이 `.gitattributes` 기준 LF인데 작업본이 CRLF라 경고가 나고 있어서, 해당 폴더 텍스트 파일만 LF로 정리한 뒤 폴더별 커밋을 진행하겠습니다.
>
> `Elmo_EtherCAT_Test_4Axis`의 `.st/.h/.xml` 파일은 저장 시 CRLF로 바뀐 상태입니다. Git 속성은 LF라서, 이 폴더의 변경된 텍스트 파일만 LF로 맞추고 바이너리 `.lba/.lcb/.bin`은 건드리지 않겠습니다.
>
> 방금 경고는 파일 목록을 뽑는 `git diff --name-only`가 변환 전에 출력한 것입니다. 변환 후 상태를 다시 확인해서 경고가 사라졌는지 보겠습니다.
>
> LF 변환 후 CRLF 경고는 거의 사라졌고 `Eni.xml` 하나만 남았습니다. `git diff --check`의 trailing whitespace는 LASAL 코드제너레이터가 생성한 공백이라 기능 변경과 무관하고, 여기서는 제거하지 않겠습니다. `Eni.xml`만 별도로 LF 정리하고 커밋을 시작합니다.
>
> `Eni.xml`은 UTF-8/ASCII XML이고 CRLF가 남아 있습니다. 이 파일만 LF로 다시 저장합니다.
>
> 이제 폴더별 커밋 순서로 진행합니다. 먼저 `Codex_LASAL_WPF`만 stage해서 WPF 앱 변경 커밋을 만들고, 그 다음 `docs`, 마지막으로 `Elmo_EtherCAT_Test_4Axis`를 커밋하겠습니다.
>
> `Codex_LASAL_WPF` 변경부터 커밋합니다. 이 커밋에는 WPF Group Test 탭, LASAL 단위 스케일, TCP 프레임 구현 변경이 들어갑니다.
>
> 첫 번째 커밋 완료했습니다. 다음은 `docs/architecture`의 프로토콜/스케일 문서만 묶어서 커밋합니다.
>
> 문서 커밋 완료했습니다. 마지막으로 `Elmo_EtherCAT_Test_4Axis` 폴더 전체 변경을 별도 커밋으로 묶습니다. 이 폴더는 LASAL IDE 생성 파일과 TCPMotionInterface 변경이 같이 들어갑니다.
>
> 세 번째 커밋까지 완료했습니다. 최종 상태와 최근 커밋 해시를 확인해서 누락이 없는지 봅니다.
</details>

폴더별로 3개 커밋 완료했습니다.

1. `a201192` `Update LASAL WPF group motion test`
   - `Codex_LASAL_WPF` 변경분
   - Group Test 탭, LASAL 단위 스케일, TCP 프레임 구현 포함

2. `7ac179b` `Document group motion TCP protocol`
   - `docs/architecture` 문서 2개
   - Group Motion TCP 프로토콜/스케일 문서화

3. `646a290` `Update Elmo EtherCAT LASAL project`
   - `Elmo_EtherCAT_Test_4Axis` 변경분
   - LASAL IDE 생성 파일과 `TCPMotionInterface` 변경 포함

최종 `git status --short`는 clean입니다.
현재 브랜치는 `main`, `origin/main`보다 11개 커밋 ahead 상태입니다. CRLF 경고 나던 LASAL 텍스트 파일들은 `.gitattributes` 기준 LF로 정리해서 커밋했습니다.

> 두개 api 문서를 분석해서 각각의 분석 자료를 만들고, 두개를 비교한 성능분석 표를 만들어서 보여줘. 어디에는 있고 어디에는 없고, 없다면 어떤 기능이 없는지 한눈에 볼수 있게
>
> User context
>
> Attachments:
> - Chapter6_Library_(250508).pdf: `C:/work/자료/EMotion/SNET-ECAT-User-Manual-25.05.08-ko/SNET-ECAT User Manual 25.05.08 ko/Chapter6_Library_(250508).pdf`
> - Maestro Administrative and Motion API_2022_12_v2.012.pdf: `./Maestro Administrative and Motion API_2022_12_v2.012.pdf`

<details><summary>13 previous messages</summary>

> PDF 분석 작업이라 `pdf` 스킬 지침을 먼저 확인한 뒤 진행하겠습니다. 두 문서에서 API 목록/기능 범주를 뽑고, 각각 별도 분석 문서와 비교 표 문서를 만들겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Content -Path C:/Users/dreiv/.codex/skills/.system/pdf/SKILL.md`
>
> </details>
>
> 표시된 `.system/pdf` 경로는 실제 파일이 없어서, 세션에 노출된 `pdf` 스킬의 실제 경로로 다시 읽습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `Get-Content -Path C:/Users/dreiv/.codex/skills/pdf/SKILL.md`
>
> </details>
>
> 이제 PDF 구조부터 봅니다. 페이지 수, 텍스트 추출 가능 여부, 목차/API 이름 패턴을 먼저 확인해서 분석 자동화 범위를 정하겠습니다.
>
> <details><summary>Ran 3 commands</summary>
>
> - Ran `Get-Item -LiteralPath 'C:\work\자료\EMotion\SNET-ECAT-User-Manual-25.05.08-ko\SNET-ECAT User Manual 25.05.08 ko\Chapter6_Library_(250508).pdf','C:\work\Elmo\Elmo_Master\Maestro Administrative and Motion API_2022_12_v2.012.pdf' | Select-Object FullName,Length,LastWriteTime`
> - Ran `where.exe pdftoppm`
> - Ran `@'
> import importlib.util
> for name in ['pdfplumber','pypdf','fitz']:
>     print(name, importlib.util.find_spec(name) is not None)
> '@ | python -`
>
> </details>
>
> 두 PDF 모두 텍스트 추출 가능하고 `pdftoppm`도 있습니다. 목차/대표 페이지를 추출해서 API 명칭 패턴을 잡겠습니다.
>
> <details><summary>Ran a command</summary>
>
> - Ran `@'
> from pathlib import Path
> import pdfplumber
> from pypdf import PdfReader
> pdfs = {
>     'snet': Path(r'C:\work\자료\EMotion\SNET-ECAT-User-Manual-25.05.08-ko\SNET-ECAT User Manual 25.05.08 ko\Chapter6_Library_(250508).pdf'),
>     'maestro': Path(r'C:\work\Elmo\Elmo_Master\Maestro Administrative and Motion API_2022_12_v2.012.pdf'),
> }
> for name,path in pdfs.items():
>     print('\n===', name, '===')
>     reader = PdfReader(str(path))
>     print('pages', len(reader.pages))
>     print('metadata', reader.metadata)
>     outlines = []
>     try:
>         def walk(items, depth=0):

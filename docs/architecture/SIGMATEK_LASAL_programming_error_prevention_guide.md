# SIGMATEK LASAL 프로그래밍 및 IDE 오류 예방 지침서

작성일: 2026-07-10

최종 갱신: 2026-07-13

이 문서는 `Elmo_Master`에서 LASAL 소스, 네트워크, 프로젝트 파일을 수정할 때
IDE 검색 오류, 생성 영역 손상, 잘못된 프로젝트 수정, 패킷 불일치를 예방하기
위한 필수 작업 지침이다.

기본 코딩 규칙은
[`SIGMATEK_LASAL_coding_rules.md`](SIGMATEK_LASAL_coding_rules.md), 프로젝트
구조 설명은
[`SIGMATEK_LASAL_programming_method_study.md`](SIGMATEK_LASAL_programming_method_study.md)를
함께 따른다.

## 1. 적용 대상과 기준 프로젝트

개발 및 커밋 기준 프로젝트는 아래 하나다.

```text
Lasal_PRG/Elmo_EtherCAT_Test_4Axis
```

- 이 프로젝트는 Git 추적 대상이다.
- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit`는 Git 미추적 복제본이다.
- `_Edit`는 비교 및 장애 재현 자료로만 사용한다.
- 사용자가 명시적으로 기준을 변경하기 전에는 `_Edit`에서 개발하거나 커밋하지
  않는다.
- IDE 장애를 진단할 때는 두 복제 프로젝트를 동시에 열지 않는다. 모든 LASAL
  IDE를 종료한 뒤 대상 프로젝트 하나만 다시 연다.

작업 시작 시 반드시 확인한다.

```powershell
git status --short
git ls-files 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/**'
git ls-files 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit/**'
```

경로 이름이나 폴더 모양이 비슷하다는 이유로 두 프로젝트를 같은 대상으로
취급하면 안 된다.

## 2. 2026-07-10 IDE 검색 오류에서 확인한 내용

### 확인된 사실

- 기준 프로젝트의 `Find in Implementation`은 `Power`, `pos`, `velo` 검색에
  성공했다.
- `_Edit`에서는 `acc`, `dec`, `LMCAxis` 검색 명령이 실행된 직후 모두
  `CInvalidArgException`으로 중단됐다.
- `_Edit`의 Rebuild, Link, LOB 목록 구성은 성공했지만 검색 오류는 계속됐다.
- 두 프로젝트의 `.lcp` 설정은 프로젝트 이름 외에는 동일했다.
- 두 프로젝트는 같은 class name/GUID set을 가지며 `_Edit`에는 복사된
  `ProjectInternal`도 남아 있다. 장애가 기록될 당시 원본과 `_Edit`는 서로 다른
  LASAL process에서 열려 있었다.
- Class, Header, Network 등록과 파일 inventory에서는 눈에 띄는 누락이 없었다.
  다만 Find 자체가 실패했으므로 LASAL 내부 검색 index의 일관성까지 정상이라고
  확인한 것은 아니다.
- `_Edit`의 `Class/TCPMotionInterface/TCPMotionInterface.st`에는 기준
  프로젝트에 없는 다음 UTF-8 한글 주석이 있었다.

```st
// Confirmed bad example from the copied project:
// [Korean multibyte comment was present here]
```

실제 문제 줄은 `_Edit` 파일의 806행이다. 문제 문구를 이 문서에 한글로 다시
복사하지 않는 이유는 LASAL 소스에 같은 문구를 재사용하지 않도록 하기 위해서다.

### 미확정 원인 후보

해당 주석의 한글 7자는 UTF-8에서 21바이트다. 문자 수와 바이트 수가 14만큼
달라진다. `Find in Implementation`은 찾은 문자열 위치를 코드 편집기에서
색으로 표시하므로 검색기가 계산한 바이트 위치와 편집기의 문자 위치가
어긋나 `CInvalidArgException`이 발생했을 가능성이 있다.

동일 class GUID를 가진 복제 project를 동시에 연 조건, 복사된 `ProjectInternal`
및 생성 index의 identity 충돌도 분리되지 않은 후보다. 따라서 특정 후보 하나를
확정 원인으로 기록하면 안 된다.

원인 확인은 다음처럼 한 변수씩 바꾼다.

1. 모든 LASAL process를 종료한 뒤 `_Edit`만 열어 동일 검색어로 기준 시험한다.
2. 계속 실패할 때만 해당 주석을 ASCII로 바꾸고 동일 검색어로 재시험한다.
3. 그래도 실패하면 source는 유지하고 10절의 cache/새 project 절차로 넘어간다.

한글 주석을 ASCII로 바꾼 뒤 동일 검색이 성공하는 A/B 시험을 완료하기 전까지
문자 인코딩을 확정 원인으로 표현하지 않는다. 반대로 단독 실행 시험 전에는
동시 project identity 충돌을 확정 원인으로 표현하지 않는다. 예방 규칙은 두
후보 모두에 보수적으로 적용한다.

설치된 공식 도움말
`C:\Program Files (x86)\Sigmatek\Lasal\Class2\Bin\LASAL_CLASS_2_EN.chm`의
*Finding Clients / Server in the Implementation* 항목은 이 기능이 구현부에서
문자열을 찾고 코드 편집기에 색으로 표시한다고 설명한다. 같은 폴더의
`Class_Change_History_EN.chm`에도 별도 기능인
`Find / Replace (in Files)`가 invalid character 때문에 crash하던 관련 이력이
있다. 이는 이번 원인을 직접 확정하지는 않지만 문자 처리 오류를 우선 점검해야
한다는 근거다.

현재 project compiler는 C78이고, OS Interface/System/Tools libraries가 C81로
빌드됐다는 warning이 나온다. 두 비교 프로젝트의 설정이 같고 기준 프로젝트의
검색은 성공했으므로 이 warning은 이번 `CInvalidArgException`의 직접 원인으로
보지 않는다. compiler 변경은 검색 장애 복구와 섞지 말고 별도 변경으로 수행하며,
전체 Rebuild와 PLC 회귀 시험을 동반한다.

## 3. LASAL 소스 문자와 인코딩 규칙

### 필수 규칙

- 사람이 새로 입력하는 LASAL custom source의 선언과 구현, 식별자, 주석,
  문자열은 7-bit ASCII만 사용한다.
- LASAL IDE에서 새로 만드는 class/object/channel/network 이름과 comment 등
  project metadata 입력도 7-bit ASCII만 사용한다. 이 값은 generated 선언,
  XML 또는 binary project file에 들어갈 수 있다.
- 한국어 설명, 설계 이유, 패킷 해설은 `docs/**/*.md`에 기록한다.
- 한글, 이모지, 스마트 따옴표, 전각 문자, 특수 대시를 LASAL 소스에 새로
  넣지 않는다.
- 외부 편집기에서 BOM, 문자 인코딩, 줄바꿈을 자동 변환하지 않는다.
- 의미 없는 전체 파일 포맷팅 또는 줄바꿈 일괄 변환을 하지 않는다.
- 기존 SIGMATEK vendor/library 소스에 있는 확장 문자를 전체 변환하지 않는다.
  기존 파일에는 legacy 단일바이트 문자가 있으므로 프로젝트 전체를 ASCII로
  일괄 변환하면 별도 손상이 생길 수 있다.

좋은 예:

```st
// Confirmed LMC packet order
// Reject the frame when the payload length is invalid
```

금지 예:

```text
Korean comments, emoji, smart quotes, and full-width punctuation in LASAL source
```

### 새로 추가한 비ASCII 라인 검사

프로젝트 전체가 아니라 Git diff에 새로 추가된 라인만 검사한다.

```powershell
$root = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis'

git status --short -- $root

git diff --unified=0 -- $root |
  Select-String -Pattern '^\+(?!\+\+\+).*[^\x00-\x7F]'

git diff --cached --unified=0 -- $root |
  Select-String -Pattern '^\+(?!\+\+\+).*[^\x00-\x7F]'

git ls-files --others --exclude-standard -- $root |
  Where-Object { $_ -match '\.(st|h|c|cpp)$' } |
  ForEach-Object {
    Select-String -LiteralPath $_ -Pattern '[^\x00-\x7F]'
  }
```

`git status`에서 tracked, staged, untracked 범위를 먼저 확인한다. 위 정규식에서
결과가 나오면 새로 추가한 라인을 검토하고, 결과가 없으면 해당 범위에서 검출된
비ASCII 문자가 없다는 뜻이다. PowerShell `Select-String`을 사용하는 이유는
`rg`가 invalid UTF-8 또는 legacy single-byte high byte를 건너뛸 수 있기 때문이다.
첫 두 정규식 명령은 tracked diff를 검사하고 마지막 명령은 아직 stage하지 않은
신규 source 전체를 검사한다. 신규 source를 stage한 뒤에는 cached diff 검사도
다시 실행한다. 기존 vendor high byte가 포함된 줄을 수정했으면 신규 도입인지
기존 문자의 유지인지 removed line과 비교한다. binary `.lcp/.lcb/.lcn` 내부 문자는
이 검사로 검출할 수 없으므로 IDE metadata 입력 단계에서 ASCII 규칙을 지킨다.

## 4. CodeGenerator 영역 수정 규칙

`.st` 상단에 다음 문구가 있으면 CodeGenerator 관리 파일이다.

```text
Please, do not edit this file
```

파일은 다음 영역으로 나누어 판단한다.

- `//{{LSL_DECLARATION`부터 `//}}LSL_DECLARATION`: 선언 및 생성 영역
- `@CT_`, 채널 개수, 채널 테이블: 생성 영역
- `//{{LSL_IMPLEMENTATION` 이하: 사용자 구현 영역

원칙:

1. 일반 로직은 `//{{LSL_IMPLEMENTATION` 이하에서만 수정한다.
2. Client, Server, 변수, 함수 선언은 LASAL IDE에서 추가한다.
3. 생성 영역을 외부 편집기로 고쳐야 한다면 임시 프로토타입으로만 취급하고,
   IDE에서 다시 열어 생성 결과와 일치시킨다.
4. 다음 항목이 모두 맞지 않으면 채널 변경이 완료된 것이 아니다.

   - XML `<Channels>`
   - `ClassName : CLASS`의 Server/Client 멤버
   - `@CT_`의 channel count와 항목
   - `@STD`와 내부 네트워크 초기화
   - `Motion_Network.lcn`의 실제 연결
   - `ONE_Motion_Network_Table.st`의 생성 연결 테이블

생성 영역을 수동 수정한 뒤 IDE가 일부만 다시 생성하면 소스, 클래스 DB,
네트워크 테이블이 서로 다른 상태가 될 수 있다.

### 현재 프로젝트의 다음 기능 작업 전 선행 gate

현재 tracked source에는 `LMCAxis1..4`, object registry, depth-8 request queue,
`DataHandling()`/`SendData()` override가 source-first 형태로 반영됐고
승인 명령의 CyWork 실행 경로가 열려 있다. interface RT task, `RtWork()` override,
typed RT mailbox와 `sigclib_atomic_*`는 제거됐다.

2026-07-13 IDE 동기화로 `LMCAxis1..4`, client count `6`, 일반
`_TCPIPServer1` 연결을 반영했다. `TCPMotionInterface1`은 CyclicTime 1 ms만
사용하고 RealTime assignment를 두지 않는다. 다음 기능 구현 전에 아래 잔여 gate를
완료해야 한다.

1. 최종 저장된 `Motion_Network.lcn`에서
   `TCPMotionInterface1.LMCAxis1 -> _LMCAxis1.Control`과 나머지 세 축 연결을
   strict contract로 확인한다.
2. interface/server를 같은 CyWork task에 두고 `Config=0`,
   `MaxConnections=1`을 적용한다. interface CyWork는 axis RT thread와 같은
   CPU core에서 같거나 낮은 priority인지 확인한다.
3. 설치된 MotionLib가 요구하는 `_DriveMngBase/DriveComL2.h` 누락 `E0015`와
   C78/C81 library version mismatch를 해결한 뒤 Rebuild/Link 0 error를 확인한다.
4. `Find in Implementation` smoke test와 새 `CInvalidArgException` 부재를
   확인한다.

이 gate를 통과하기 전에는 수동 선언을 완성된 IDE model로 보거나 production
완료로 승인하지 않는다. 상세 기준은 7절의 object dispatcher 설계 문서를
따른다.

## 5. LASAL 파일별 취급 기준

| 파일/폴더 | 용도 | 작업 규칙 |
|---|---|---|
| `Class/**/*.st`, `*.h`, `*.c`, `*.cpp` | 사람이 검토하는 소스 | 설계 및 코드 검토의 1차 근거 |
| `*.lcp` | 프로젝트 설정과 등록 | IDE에서 변경하고 의도한 설정만 추적 |
| `*.lcn` | Network topology | IDE에서 변경하고 생성 테이블과 교차 확인 |
| `ONE_*_Table.st` | Network 생성 결과 | 연결 개수와 대상 검증에 사용 |
| `*.lcb` | class/project database | 바이너리 diff만으로 의미 판단 금지 |
| `*.lba`, `*.lob`, `*.ldi`, `*.lhd`, `*.lcc`, `*.bin` | 빌드 및 인덱스 생성물 | 직접 수정하거나 새로 stage하지 않음 |
| `ProjectInternal/`, `*.lock` | IDE 로컬 상태 | 설계 근거 및 커밋 대상이 아님 |

이미 Git에 추적된 생성 파일은 `.gitignore`로 숨겨지지 않는다. 변경 파일이
나오면 먼저 아래 명령으로 추적 여부를 확인한다.

```powershell
git ls-files -- <path>
```

## 6. 프로젝트 복제 규칙

Windows 탐색기에서 프로젝트 폴더를 통째로 복사한 뒤 `.lcp/.lcb` 이름만
바꾸는 방식으로 새 개발 프로젝트를 만들지 않는다.

필요한 경우:

1. 모든 LASAL IDE를 종료한다.
2. 원본 프로젝트의 Git 상태를 기록한다.
3. 설치된 LASAL 버전의 vendor-supported project duplication 절차를 확인해 새
   project identity를 만든다. 확인되지 않은 메뉴 이름을 임의로 가정하지 않는다.
4. 새 프로젝트의 이름, 경로, compiler version, class 등록, network를 확인한다.
5. Build/Rebuild/Link를 수행한다.
6. 최소 세 개의 Client/Server에서 `Find in Implementation`을 실행한다.
7. IDE 로그에 예외가 없는지 확인한다.

복제 프로젝트를 검증할 때 원본과 복제본을 동시에 열지 않는다.

## 7. TCP, RPC, Motion 구현 규칙

### TCP 수신

- `Response(pData, udSize, dSock)`에서 callback 포인터를 함수 밖에 보관하지
  않는다.
- frame 길이와 receive buffer 범위를 먼저 검증한다.
- TCP segment 경계와 API frame 경계를 같다고 가정하지 않는다.
- 여러 frame이 합쳐지거나 한 frame이 나뉘어 도착할 수 있어야 한다.
- 실제 motion 명령을 TCP callback 안에서 장시간 실행하지 않는다.
- tracked `TCPMotionInterface.st`의 `Response()`는 callback payload를 종료 전에
  depth-8 queue 자체 storage로 복사하고 `MsgPaser()`나 `SendData()`를 직접
  호출하지 않는다.
- `CyWork()`가 session/order와 승인된 axis/group 명령을 실행·응답한다.
  interface의 RT task, `RtWork()` override, RT mailbox와 atomic state는 사용하지
  않는다.
- 승인 명령은 `0x2023 Power`, `0x2024 Reset`, `0x2022 Stop`,
  `0x2028 ReadStatus`, `0x202E ReadPosition`, `0x209F MoveAbsolute`,
  `0x20A0 MoveRelative`, `0x20A2 MoveVelocity`, `0x2047 GroupEnable`,
  `0x2048 GroupDisable`, `0x2045 GroupReadStatus`다.
- `0x2049 GroupReset`, `0x2085 GroupStop`, `0x20A4 MoveLinear`,
  `0x2051 GroupReadActualPosition`, `0x20E7 SetKinTransform`은 지원하지 않으며 pre-case
  guard에서 deterministic error `-5`를 반환한다. 기존 helper body가 남아 있다는
  이유로 runtime 허용 상태로 판단하면 안 된다.
- source handler는 반영됐지만 LASAL IDE Rebuild, task/core 확인, PLC download,
  PLC 동작 시험과 packet 재캡처는 남아 있다. 상세 적용 상태는
  [`LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md`](../../LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md)를
  따른다.

### RPC lifecycle

현재 계약 순서는 다음과 같다.

1. `0x8080`: session init
2. `0x405C`: callback endpoint 등록
3. motion/read 명령
4. `0x405D`: close

session 소유 socket과 다른 socket의 명령은 거부한다. socket disconnect 또는
close 때 session, callback, receive accumulator 상태를 함께 정리한다.

### Object dispatcher

- 실제 `_LMCAxis1..4`와 group object 이름은 LASAL이 Network 연결을 통해
  조회한다.
- PC API는 PLC pointer를 알거나 전송하지 않는다.
- PC에는 opaque reference 또는 handle만 반환한다.
- Client 연결과 object name registry가 준비되기 전에는 motion 명령을 실행하지
  않는다.

상세 구조는
[`LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md`](../../LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md)를
기준으로 하되, IDE에서 실제 object name과 연결을 확인한 뒤 확정한다.

### UNIT 책임

- PC application이 `송신 DINT = 물리값 x PLC 설정 UNIT`을 수행한다.
- PC API DLL은 받은 DINT를 그대로 serialize한다.
- LASAL은 받은 DINT에 UNIT을 다시 곱하거나 나누지 않는다.
- 축마다 UNIT이 다르면 PC application이 축별로 변환한다.
- 더미 프로그램의 `8388608` 상수는 특정 encoder 예시일 뿐 공통 UNIT이 아니다.

상세 기준은
[`UNIT_CONVERSION_MANUAL_2026-07-10.md`](../../LMC_Library/LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md)를
따른다.

### 미지원 명령

미지원 명령에 dummy success를 반환하지 않는다. 현재 `0x2049 GroupReset`,
`0x2085 GroupStop`, `0x20A4 MoveLinear`, `0x2051 GroupReadActualPosition`,
`0x20E7 SetKinTransform`은 deterministic error `-5`로 처리한다. 실제 callback
event sender와 multi-PC ownership도 구현 및 PLC 검증 전까지 지원 대상으로
판정하지 않는다. 현재 상태는
[`API_DEVELOPMENT_BACKLOG_2026-07-10.md`](../../LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)를
기준으로 확인한다.

## 8. 반복 개발 절차

### 수정 전

1. 기준 프로젝트 경로를 확인한다.
2. 중복 LASAL IDE를 종료하고 대상 프로젝트 하나만 연다.
3. `git status --short`와 `git diff --stat`을 저장한다.
4. 대상 파일이 source, generated, cache 중 무엇인지 분류한다.
5. CodeGenerator 영역과 사용자 구현 영역을 구분한다.
6. TCP/Network 변경이면 대응 PC 코드와 패킷 문서를 먼저 연다.

### 수정 중

1. 한 번에 한 기능만 작은 diff로 수정한다.
2. 새 custom source와 IDE metadata 입력은 ASCII만 사용한다.
3. Network와 channel 변경은 IDE에서 수행한다.
4. TCP callback에 blocking motion 실행을 추가하지 않는다.
5. command ID, endian, payload length, byte offset을 숫자 단위로 대조한다.

### 수정 후

1. Save 후 Build/Rebuild/Link 결과가 0 error인지 확인한다.
2. 변경 클래스의 앞, 중간, 뒤에 위치한 Client/Server 이름으로
   `Find in Implementation`을 실행한다.
3. `Find Results`가 비어 있거나 메뉴가 조용히 닫히면 정상으로 간주하지 않는다.
4. `Lasal2.log`에서 검색 명령 뒤 예외가 없는지 확인한다.
5. 새 비ASCII diff를 검사한다.
6. C# 송신 frame과 LASAL parser를 byte offset 단위로 대조한다.
7. Network 변경이면 실제 축 연결과 생성 테이블을 확인한다.
8. `git diff --check`와 `git diff --cached --check`를 통과시킨다.

## 9. Find in Implementation smoke test

기준 프로젝트에서 실제 성공이 확인된 아래 검색어는
`TCPMotionInterface` 전용 smoke 항목이다.

- `Power`
- `pos`
- `velo`

`acc`, `dec`, `LMCAxis`는 `_Edit`에서 예외가 재현된 검색어다. A/B 진단에서는
두 프로젝트에 실제로 존재하는 동일 검색어를 양쪽에 사용해야 한다. 검색어가
다르면 프로젝트 간 성공/실패 비교 근거로 사용하지 않는다.

다른 class를 변경했으면 그 class에 실제로 존재하고 변경 지점의 앞, 중간, 뒤에서
각각 hit가 나오는 Client/Server 또는 symbol 세 개를 별도로 선택한다.

Client 또는 Server 이름을 우클릭하고 `Find in Implementation`을 실행한다.
정상 조건은 다음과 같다.

- `Find Results`에 실제 source 경로와 행 번호가 표시된다.
- 결과를 열면 해당 구현부로 이동한다.
- 로그에서 명령이 `Last command succeeded`로 끝난다.
- `CInvalidArgException`이 없다.

smoke 시작 전에 현재 로그 행 수를 저장한다.

```powershell
$log = Join-Path $env:TEMP 'Lasal2.log'
$startLine = if (Test-Path $log) {
  [IO.File]::ReadAllLines($log).Length
} else {
  0
}
```

smoke 실행 후 새로 추가된 로그만 확인한다.

```powershell
$newLog = if (Test-Path $log) {
  [IO.File]::ReadAllLines($log) | Select-Object -Skip $startLine
} else {
  @()
}

$newLog | rg 'Searching implementation|CInvalidArgException|Last command succeeded'
```

과거 로그 전체에 `_Edit` 예외가 남아 있으므로 전체 파일에서 exception 문자열이
한 번이라도 발견됐다는 이유만으로 새 smoke를 실패 처리하지 않는다.

## 10. `CInvalidArgException` 복구 절차

### 1단계: 즉시 중단 및 증거 보존

- 추가 IDE 저장과 대량 재생성을 중단한다.
- 실패한 project, class, 검색어, 시각을 기록한다.
- `git status`, `git diff`, `Lasal2.log`를 보존한다.
- 모든 LASAL IDE를 종료한다.

### 2단계: 최근 소스 변경 A/B 시험

1. 최근 추가한 `.st/.h/.c/.cpp` 라인의 비ASCII 문자를 확인한다.
2. 의심 주석을 ASCII로 바꾼다.
3. BOM과 전체 줄바꿈은 변경하지 않는다.
4. 대상 프로젝트 하나만 다시 연다.
5. Rebuild/Link 후 동일 검색어로 재시험한다.

한 번에 한 조건만 바꿔야 원인을 확정할 수 있다.

### 3단계: IDE cache 재생성

소스 A/B 시험 후에도 실패할 때만 수행한다.

1. IDE가 모두 종료됐는지 확인한다.
2. LASAL process가 남아 있지 않고 대상 project의 `*.lock`이 사라졌는지
   확인한다. process 또는 lock이 남아 있으면 cache를 이동하지 않는다.
3. 프로젝트 전체를 별도 백업한다.
4. IDE의 Clean/Rebuild 기능을 먼저 사용한다.
5. 그래도 실패하면 사용자 승인 후 `ProjectInternal`과 비추적 생성/index 파일을
   삭제하지 말고 별도 폴더로 격리한다.
6. IDE가 cache를 다시 생성하게 한다.

금지 사항:

- `git clean -fd`
- tracked `.lcp/.lcb/.lcn` 일괄 삭제
- source와 Network 파일 초기화
- 원인 확인 전 전체 프로젝트 재인코딩

### 4단계: IDE-native 새 프로젝트

계속 실패하면 IDE에서 새 identity의 프로젝트를 만들고 source와 Network만
재등록한다. 기존 `ProjectInternal`, `.lba`, `.lob`, index 파일은 복사하지
않는다.

## 11. 커밋 허용 조건

아래 조건을 모두 만족해야 LASAL 변경을 커밋한다.

- 기준 프로젝트에서만 작업했다.
- 새 custom source와 IDE metadata 입력이 ASCII다.
- CodeGenerator 선언과 구현 구조가 일치한다.
- Build/Rebuild/Link가 0 error다.
- 변경 클래스의 `Find in Implementation` smoke test가 정상이다.
- `Lasal2.log`에 새 `CInvalidArgException`이 없다.
- TCP/parser를 변경했다면 packet의 command ID, endian, length, offset이 PC와
  일치한다.
- Axis, DS402, PDO 또는 Network를 변경했다면 `_LMCAxis1..4`, DS402, PDO,
  Motion Network의 관련 연결을 확인했다.
- `git diff --check`와 `git diff --cached --check`를 통과했다.
- 신규 `.lba/.lob/.ldi/.bin/ProjectInternal`이 staged되지 않았다.
- tracked 생성 파일 변경을 소스 변경과 연결해 설명할 수 있다.
- 관련 설계 및 테스트 문서를 함께 갱신했다.

## 12. Definition of Done

11절은 source commit 허용 조건이다. LASAL 기능 자체의 production 완료는 아래
상태를 모두 충족해야 한다.

1. 기준 project/source가 명확하다.
2. IDE 생성 영역이 손상되지 않았다.
3. Build/Rebuild/Link가 정상이다.
4. `Find in Implementation`과 IDE 로그가 정상이다.
5. PC API frame과 LASAL parser가 일치한다.
6. Network, Axis, DS402 연결이 일치한다.
7. Git diff에 의도하지 않은 IDE/cache 변경이 없다.
8. 대상 PLC download가 성공했다.
9. 안전 조건을 갖춘 실제 장비 smoke test가 성공했다.
10. 실제 request/response packet을 재캡처하여 문서 계약과 일치함을 확인했다.
11. PLC 결과와 packet 근거를 테스트 문서에 기록했다.

현재 상태는 `source handler 반영 완료, LASAL IDE Rebuild 및 PLC 동작 시험
대기`로 기록한다. 이 상태는 production 완료가 아니다.

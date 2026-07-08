> 검증은 통과했습니다. 변경된 기능 위치와 문서 위치를 라인 번호 기준으로 한 번 더 뽑아서 최종 보고에 정확히 적겠습니다.
>
> 구현/검증이 끝났습니다. PLC ST는 건드리지 않았습니다. 이유는 이 테스트가 기존 단축 `MoveAbsolute`와 `ReadActualPosition` TCP 경로만 사용하고, SIGMATEK 쪽도 그 경로가 이미 구현되어 있기 때문입니다.
</details>

구현했습니다. 기존 단축 `Cycle Test`를 Jonas가 말한 생산성 비교용으로 확장했습니다.

측정 사이클은 이제 이렇게 동작합니다.

```text
Base -> Forward 위치 이동
-> actual position으로 done 확인
-> Forward Actor Delay
-> Base 복귀 이동
-> actual position으로 done 확인
-> Return Actor Delay
```

변경 파일:
- [PMAS Cycle Test 로직](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:45)
- [PMAS Cycle Test UI](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml:1262)
- [SIGMATEK/LASAL Cycle Test 로직](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:38)
- [SIGMATEK/LASAL Cycle Test UI](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:1162)
- [테스트 기준 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/Production_Cycle_Performance_Test_2026-06-25.md:1)

추가된 입력:
- `Forward Actor Delay (ms)`
- `Return Actor Delay (ms)`

추가된 결과:
- `TotalElapsed(ms)`
- `AveragePartTime(ms)`
- `Throughput(parts/min)`
- `CommandLatencyAvg/Max(ms)`
- `ResponseLatencyAvg/Max(ms)`

검증:
- `git diff --check` 통과
- PMAS WPF Debug 빌드 성공, 경고 0 / 오류 0
- LASAL WPF Debug 빌드 성공, 경고 0 / 오류 0
- 라인엔딩도 `.cs/.xaml = CRLF`, `.md = LF`로 맞춤

`Elmo_EtherCAT_Test_4Axis` PLC/LASAL ST 코드는 건드리지 않았습니다. 이번 기능은 기존 단축 `MoveAbsolute` + `ReadActualPosition` 경로만 사용하므로 PLC 쪽 새 command는 필요 없습니다.

> api 정리문서 확인해서 api리스트가 어떤식으로 차이나는지 알려줘
>
> User context
>
> Attachments:
> - EtherCAT Controller(Master) 요구 사양.xlsx: `./EtherCAT Controller(Master) 요구 사양.xlsx`
> - Motion_Controller_API_정리.xlsx: `./Motion_Controller_API_정리.xlsx`

<details><summary>12 previous messages</summary>

> 스프레드시트 비교 작업이라 `spreadsheets` 스킬 지침을 먼저 확인하고, 두 xlsx의 시트/컬럼 구조를 본 뒤 API 목록 차이를 정리하겠습니다.
>
> 표시된 스킬 경로가 현재 캐시에 없어서 실제 설치된 스킬 파일 위치를 찾아서 읽겠습니다.
>
> 실제 캐시 버전은 `26.630.12135`입니다. 이 지침을 적용해서 진행합니다.
>
> 이번 요청은 새 xlsx를 만들거나 수정하는 게 아니라 기존 두 파일의 API 목록 비교입니다. 워크북을 읽기만 할 것이고, 먼저 번들 런타임/라이브러리 경로를 확인한 뒤 시트 구조와 컬럼을 추출하겠습니다.
>
> 두 워크북의 시트명, 사용 범위, 헤더 후보를 먼저 확인합니다. 그 다음 API명으로 보이는 컬럼을 잡아서 목록 차이를 비교하겠습니다.
>
> PowerShell heredoc 문법이 bash 방식으로 들어가서 실패했습니다. 같은 내용을 PowerShell here-string으로 다시 실행합니다.
>
> 콘솔 기본 인코딩이 일부 문자를 출력하지 못했습니다. UTF-8 출력으로 다시 구조를 뽑겠습니다.
>
> 구조가 확인됐습니다. `EtherCAT Controller(Master) 요구 사양.xlsx`는 `ELMO Controller API` 시트의 1열에 짧은 요구 API 목록이 있고, `Motion_Controller_API_정리.xlsx`는 `API 목록` 시트의 OPUS(C++) 컬럼에 훨씬 큰 API 목록이 있습니다. 이제 함수명을 정규화해서 공통/누락/추가를 계산합니다.
>
> 1차 비교 결과는 의외로 두 문서의 OPUS API 함수명 집합이 완전히 같습니다. 다만 `Motion_Controller_API_정리.xlsx`는 275행짜리 문서라서 OPERA(C#) 컬럼이나 빈/분류 행이 섞여 있을 수 있습니다. 행 구조를 더 확인해서 실제 API 행이 더 있는지 검증합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 두 엑셀 파일의 API 시트 기준으로 비교는 끝났습니다. 핵심은 “OPUS/MMCLib 고유 함수명” 기준인지, “OPERA C#까지 포함한 정리 행” 기준인지에 따라 차이가 다르게 보인다는 점입니다.
>
> 추가로 행 수와 중복까지 재확인했습니다. `Motion_Controller_API_정리.xlsx`의 C# 열은 반환형이 앞에 붙어 있어서, 메서드명만 따로 뽑아 다시 정리합니다.
</details>

확인 결과, **OPUS/MMCLib API 함수명 기준으로는 두 문서의 리스트가 동일합니다.**

비교 기준:
- [EtherCAT Controller(Master) 요구 사양.xlsx](</C:/work/Elmo/Elmo_Master/EtherCAT Controller(Master) 요구 사양.xlsx>)
  - `ELMO Controller API` 시트
  - API 44개, 고유 함수명 44개
- [Motion_Controller_API_정리.xlsx](</C:/work/Elmo/Elmo_Master/Motion_Controller_API_정리.xlsx>)
  - `API 목록` 시트
  - 전체 정리 행 65개
  - OPUS/C++ 함수가 있는 행 48개
  - OPUS 고유 함수명 44개
  - OPERA/C# 전용 행 17개

즉, **요구사양 문서의 44개 ELMO API는 Motion API 정리 문서의 OPUS 열에 전부 들어있고, 빠진 것도 추가된 OPUS API도 없습니다.**

차이는 문서 구조입니다.

| 구분 | EtherCAT 요구 사양 문서 | Motion API 정리 문서 |
|---|---|---|
| 성격 | 요구 API 목록 | OPUS C++ / OPERA C# 통합 매핑표 |
| API 수 | 44개 | OPUS 고유 44개 + C# 전용 행 17개 |
| 정보량 | 함수명 + 간단 정의 | 함수명, 설명, 사용 목적, 비고, 우선순위 |
| 중복 | 없음 | overload/용도 차이 때문에 일부 OPUS 함수 중복 |

중복으로 나뉜 OPUS API는 3종입니다.

| OPUS API | Motion 문서에서 나뉜 방식 |
|---|---|
| `MMC_PowerCmd` | Power On / Power Off 용도로 분리 |
| `MMC_MoveLinearAbsoluteExCmd` | 파라미터 형태가 다른 C# overload로 분리 |
| `MMC_SendSdoCmd` | SDO download 타입별 overload로 3개 분리 |

그리고 Motion 문서에는 요구사양 문서에 없는 **OPERA/C# 전용 API 행**이 있습니다. 대표적으로:

- `GetEntry`, `AddEntry`, `Upload`, `Init`: PI/BulkRead 보조 API
- `RegisterEmergencyEventCallback`: Emergency Event callback 등록
- `ReadGroupParameters`, `WriteGroupParameters`: Group parameter 일괄 처리
- `HomeDS402`, `GetOpMode`: DS402 / Operation Mode 관련 C# API
- `EnableProfileConditioning`, `SetProfileConditioningMode`: Profile Conditioning
- `MoveLinearRelativeEx`: Group 상대 이동 C# API
- `UploadSDO`, `UploadSdoEx`: SDO upload 계열
- `GroupReadActualPosition`: Group actual position 읽기

정리하면, **기본 이식 대상 OPUS/MMCLib API 목록은 두 문서가 같고**, `Motion_Controller_API_정리.xlsx`는 거기에 **C# OPERA wrapper, overload, helper API, 우선순위/용도 설명을 붙인 확장 정리표**입니다.

> WireShark 분석된 내용을 토대로 API를 만들었어. 이걸 이용해서 lasal 에서 동작될 수 있는 소스코드를 구현해야돼. 이 내용에 대해서 구현 방안에 대해 설계해보자.
>
> User context
>
> Attachments:
> - WireShark: `./LMC_Library/LMC_API/Elmo_API_Packet2/WireShark/`

<details><summary>11 previous messages</summary>

> 먼저 실제 `WireShark` 폴더와 현재 LASAL 쪽 TCP/모션 인터페이스 구조를 같이 확인하겠습니다. 히스토리와 현재 소스를 분리해서 봐야 설계가 헛돌지 않습니다.
>
> 폴더 안에는 분석 문서가 아니라 API별 `.pcapng` 캡처가 직접 들어 있습니다. 그래서 설계는 현재 구현된 C# API/더미 프레임과 LASAL `TCPMotionInterface`를 대조해서, 캡처 기반 프로토콜 계층을 어디에 둘지 정하는 방식으로 잡겠습니다.
>
> 현재 `TCPMotionInterface.st`는 CodeGenerator 파일이고, 실제 수정 가능 영역은 아래 구현부입니다. 여기 구조가 이미 “TCP 수신 -> 파싱 -> 축/로봇 호출 -> 응답”을 한 클래스에 꽤 많이 갖고 있어서, 새 API는 무작정 붙이면 금방 유지보수 불가능해집니다.
>
> `LMC_Library/LMC_API/LMC_API/docs`에 이미 패킷 맵과 API 목록이 있습니다. 다음은 이 문서를 기준으로 “LASAL에서 바로 구현 가능한 API 계층”과 “아직 축/그룹 연결 검증이 필요한 계층”을 나누겠습니다.
>
> 확인된 전달 API는 PMAS/MMCLib을 그대로 포팅한 게 아니라, 캡처된 Maestro TCP 패킷을 `LMC_` API로 감싼 구조입니다. LASAL 구현도 같은 방향, 즉 “Elmo API 흉내”가 아니라 “패킷 command를 SIGMATEK 모션 호출로 변환하는 서버”로 설계하는 게 맞습니다.
>
> 현재 LASAL 구현은 이미 일부 command ID를 처리하지만, 새 LMC 패킷 맵과 안 맞는 부분이 있습니다. 예를 들어 `PowerCmd`는 새 맵에서 `0x2023`인데 현재 구현은 `0x2081/0x2082` 계열도 보고 있어서, 이전 더미 프로토콜과 새 캡처 기반 프로토콜을 분리해야 합니다.
>
> 중요한 설계 판단 하나: 기존 `Codex_LASAL_WPF`의 더미 MMCLib은 과거 테스트용 command id를 일부 쓰고, 새 `LMC_API` 문서는 캡처 기반 command id를 씁니다. 앞으로 기준은 `LMC_PACKET_MAP.md`가 되어야 하고, 기존 더미 프로토콜은 호환 레이어로만 남기는 게 맞습니다.
>
> 설계 관점에서 가장 위험한 부분은 Group입니다. `MoveLinearAbsoluteEx`는 패킷 구조가 비교적 명확하지만, LASAL 쪽 `LMCRobot` 연결/상태 전환/GroupDisable 후 단축 복귀가 실제 장비 상태와 맞아야 하므로 API 이름만 맞춰 성공 응답을 주면 안 됩니다.
>
> 또 하나 확인된 현재 제약입니다. `TCPMotionInterface`에는 `LMCAxis` 클라이언트가 하나뿐이라서, 새 API의 `a01~a04` 멤버 Power/상태/단축 이동을 제대로 처리하려면 4축 라우팅 구조가 필요합니다. 단일 축만 되는 상태에서 그룹 API를 “성공” 처리하면 실제 테스트에서 바로 깨질 가능성이 큽니다.
>
> 네트워크 테이블에는 `_LMCAxis1~4`와 `LMCRobot` 연결 흔적이 있습니다. 다만 `TCPMotionInterface` 클래스 선언은 단일 `LMCAxis` 클라이언트만 받도록 되어 있어서, 구현 설계에는 “축 이름/AxisRef → 실제 축 클라이언트” 매핑 추가가 포함돼야 합니다.
>
> 캡처 자체도 빠르게 훑어서 요청/응답 길이를 확인하겠습니다. 설계에서 중요한 건 ACK가 아니라 실제 TCP payload 길이와 첫 8바이트 header입니다.
</details>

확인한 기준으로 설계 방향은 명확합니다. LASAL에는 “LMC API DLL”을 이식하는 게 아니라, **LMC API가 보내는 캡처 기반 TCP 패킷을 받아서 SIGMATEK `_LMCAxis` / `LMCRobot` 호출로 변환하는 서버 계층**을 구현해야 합니다.

**확인된 사실**
- 패킷 기준 문서는 [LMC_PACKET_MAP.md](</C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API/LMC_API/docs/LMC_PACKET_MAP.md:1>)입니다.
- 실제 캡처는 [WireShark](</C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API/Elmo_API_Packet2/WireShark>)에 있고, TCP payload는 API별로 9, 16, 20, 24, 49, 64, 88, 312, 1328 bytes 등으로 나뉩니다.
- 현재 LASAL 진입점은 [TCPMotionInterface.st](</C:/work/Elmo/Elmo_Master/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:416>)입니다.
- 현재 구현은 일부 과거 더미 command ID를 사용합니다. 예: Power On/Off `0x2081/0x2082`. 새 LMC 캡처 기준은 `LMC_PowerCmd = 0x2023`입니다.

**핵심 구조**
```text
LMC_API.dll / test-app
        |
        | TCP packet
        v
TCPMotionInterface.Response
        |
        v
LMC frame parser
        |
        v
bounded command queue
        |
        v
RtWork dispatch
        |
        +--> Axis adapter: _LMCAxis1..4
        |
        +--> Group adapter: LMCRobot
        |
        v
LMC response builder -> SendData()
```

**먼저 고쳐야 할 구조 문제**
1. Header 파싱을 바꿔야 합니다.
   새 패킷 header는 사실상 `CommandId UINT`, `AxisRef UINT`, `PayloadLength UINT`, `GroupRef/Flags UINT`입니다. 현재 코드는 offset 4에서 4바이트를 `Payload`로 복사하는데, group command에서는 payload length와 group flag가 섞입니다.

2. Motion parameter 타입을 바꿔야 합니다.
   `0x209F`, `0x20A0`, `0x20A2`, `0x20A4`는 8바이트 값이 들어옵니다. 현재 `pos`, `velo`, `acc`, `dec`, `jer`가 `DINT`인데 여기에 8바이트 `MemCpy`를 하는 구조라 위험합니다. `LREAL` 또는 64-bit 별도 버퍼로 받아야 합니다.

3. `Response()`에서 모션을 직접 실행하지 않아야 합니다.
   `Response()`는 복사, 길이 검증, command queue 적재까지만 하고, 실제 `_LMCAxis` / `LMCRobot` 호출과 응답 송신은 `RtWork`에서 처리하는 구조가 맞습니다.

4. 축 라우팅이 필요합니다.
   현재 `TCPMotionInterface`는 `LMCAxis` client 하나만 받습니다. 그런데 LMC API는 `a01~a04`와 group `v01` 흐름을 씁니다. 따라서 `a01/a02/a03/a04 -> _LMCAxis1..4` 매핑이 필요합니다. 네트워크 테이블에는 `_LMCAxis1~4`와 `LMCRobot` 연결 흔적이 이미 있습니다.

5. `0x202B`도 지원해야 합니다.
   문서 API 목록에는 빠져 있지만 캡처상 `LMCAxis` 생성 과정에서 `0x103C` axis name lookup 뒤 `0x202B`가 호출됩니다. 이걸 ACK 처리하지 않으면 C# LMC API 생성자 단계에서 막힐 수 있습니다.

**구현 범위 설계**
1차 구현은 단축 API부터 가는 게 맞습니다.

| 단계 | API |
|---|---|
| 연결/lookup | `0x8080`, `0x405C`, `0x405D`, `0x103C`, `0x1042`, `0x202B` |
| 단축 기본 | `0x2023 Power`, `0x2024 Reset`, `0x2022 Stop`, `0x2028 Status`, `0x202E ActualPosition` |
| 단축 모션 | `0x209F MoveAbsolute`, `0x20A0 MoveRelative`, `0x20A2 MoveVelocity` |
| 그룹 기본 | `0x20D2`, `0x2047`, `0x2048`, `0x2049`, `0x2085`, `0x2045`, `0x2051` |
| 그룹 모션 | `0x20E7 SetKinTransform`, `0x20A4 MoveLinearAbsoluteEx` |

**추천 구현 순서**
1. `TCPMotionInterface` 안에 `LMC_` command constant, header parser, response builder를 먼저 만든다.
2. `Response()`는 `ReceiveBuf` 복사와 frame 검증만 하게 줄인다.
3. `RtWork()`에서 pending command를 하나씩 dispatch한다.
4. `0x103C`, `0x1042`, `0x202B`, `0x202E`, `0x2028`처럼 모션 위험이 낮은 read/lookup 계열부터 맞춘다.
5. 그 다음 `Power/Reset/Stop`.
6. 마지막에 `MoveAbsolute/Relative/Velocity`.
7. 단축이 안정된 뒤 group `v01` / `LMCRobot` 쪽을 붙인다.

**주의할 단위 문제**
LMC 문서는 위치/속도 값을 controller count 기준으로 설명합니다. 현재 LASAL 네트워크 초기값에는 `_LMCAxis`가 `ExUnits=8388608`, `IntUnits=360 deg`로 잡혀 있습니다. 그래서 구현 전 결정이 필요합니다.

- LMC API 입력 count를 그대로 `_LMCAxis`에 넣을지
- `count * 360 / 8388608`로 degree 변환해서 넣을지

이건 실제 `_LMCAxis.MoveAbsolute()`가 기대하는 단위를 확인한 뒤 하나로 고정해야 합니다. 여기서 틀리면 1 rev 명령이 360도인지 8,388,608도인지가 갈립니다.

**결론**
현재 설계는 기존 `TCPMotionInterface`를 완전히 버리는 게 아니라, 그 안을 **LMC protocol server**로 재정리하는 방향이 맞습니다. 다만 기존 command ID, DINT 기반 8바이트 복사, 단일 `LMCAxis` 라우팅은 그대로 두면 안 됩니다. 다음 작업은 이 설계를 문서로 고정하고, 단축 lookup/read/status부터 Phase 1 구현에 들어가는 것입니다.

> 지금 폴더를 기능별로 다 재배치했으니 폴더안에 있는 파일들을 다 재 스캔해서 종류별로 정리해서 Git에 올려

<details><summary>24 previous messages</summary>

> 먼저 현재 Git 상태, ignore 규칙, 새로 재배치된 파일 종류를 다시 스캔하겠습니다. `pcapng` 같은 캡처 파일은 이 저장소 규칙상 기본 무시 대상이라, 실제로 올릴지 여부를 상태로 확인하고 판단하겠습니다.
>
> <details><summary>Ran 4 commands</summary>
>
> - Ran `rg -n "Elmo_Master|LMC_API|pcapng|Git|ignore|capture|WireShark" C:\Users\dreiv\.codex\memories\MEMORY.md`
> - Ran `git status --short --untracked-files=all`
> - Ran `git branch --show-current`

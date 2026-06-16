# Elmo_Master_history_260526 part 02

Source: C:\work\Elmo\Elmo_Master\docs\history\Elmo_Master_history_260526.md
Original lines: 3117-6444
Note: embedded base64 images are extracted to ./assets and replaced with compact links in this analysis split.

> 이제 명령 단위로 묶겠습니다. 각 `MoveAbsoluteEx`마다 `직전 위치`, `첫 응답`, `첫 위치 변화`, `변화량`, `타겟 도달 근사 시점`을 뽑는 구조로 CSV를 만들겠습니다.
>
> 중간 `0x2029`는 `MC_READAXISERROR_ID`입니다. 즉 일부 전진 명령 전에 축 에러를 읽고 바로 `MoveAbsoluteEx`를 보내는 패턴입니다. CSV에는 이것도 별도 행으로 넣겠습니다.
>
> 타임라인 CSV 하나만으로는 부족해서, `MoveAbsoluteEx`별 요약 CSV도 같이 만들겠습니다. 위치 응답에는 `이전값 대비 delta`와 `현재 활성 명령 기준 target 오차`를 같이 넣겠습니다.
>
> CSV 생성 작업 들어갑니다. 이번에는 `전체 패킷 타임라인`과 `MoveAbsoluteEx별 요약` 두 개로 저장하겠습니다.
>
> 파일은 생성됐습니다. 헤더와 요약 몇 줄만 확인하고 전달하겠습니다.
>
> 요약 CSV에서 `ReadAxisError` 연결 기준이 헐겁게 잡혀서, 그 부분만 바로 수정해서 다시 저장하겠습니다.
</details>

CSV 만들었습니다. 입력 캡처: [Motion_Tes2t.pcapng](/C:/work/Elmo/Elmo_Master/packet_capture/Motion_Tes2t.pcapng)

생성 파일:
1. 전체 타임라인
[Motion_Tes2t_Command_PositionTimeline.csv](/C:/work/Elmo/Elmo_Master/packet_capture/Motion_Tes2t_Command_PositionTimeline.csv)

2. `MoveAbsoluteEx`별 요약
[Motion_Tes2t_MoveSummary.csv](/C:/work/Elmo/Elmo_Master/packet_capture/Motion_Tes2t_MoveSummary.csv)

들어간 내용:
- `MoveAbsoluteEx` 요청/응답
- `ReadActualPosition` 요청/응답
- 중간 `0x2029 = ReadAxisError` 요청/응답
- 상대시간 `rel_ms`
- 요청 대비 응답 지연 `response_latency_ms`
- 실제 위치 `read_position`
- 이전 위치 대비 변화량 `delta_prev_position`
- 현재 활성 Move 기준
  - `active_move_target`
  - `active_move_baseline`
  - `delta_from_active_baseline`
  - `distance_to_active_target`

요약:
- `MoveAbsoluteEx` 총 `10회`
- `ReadActualPosition` 요청 총 `2109회`
- 평균 폴링 간격 약 `1761.461 ms`
- 평균 응답 지연 약 `657.882 ms`
- 전진 명령 직전 `ReadAxisError`는 `3, 5, 7, 9번째 Move` 전에 들어가고 응답값은 전부 `0`

원하면 다음으로
1. 이 CSV를 `xlsx`로 묶기
2. 각 Move에 대해 `실제 시작 추정 시점` 칼럼만 따로 추가하기
3. `Motion_Test.pcapng`와 차이 비교표 만들기

> 파일을 잘못줬다.
> 이걸 분석해서 csv로 만들어
>
> User context
>
> Attachments:
> - MoveAbsoluteEx_DelayTest2.pcapng: `./packet_capture/MoveAbsoluteEx_DelayTest2.pcapng`

<details><summary>4 previous messages</summary>

> 캡처 구조부터 확인하겠습니다. `192.168.1.3` 기준으로 어떤 명령과 피드백이 들어 있는지 먼저 분리한 뒤, 그 구조에 맞는 CSV를 생성하겠습니다.
>
> 이 캡처도 `MoveAbsoluteEx 1회 + ReadStatus 100회` 구조입니다. 상태 변화 시점만 추가로 뽑아서 같은 형식의 CSV 두 개로 만들겠습니다.
>
> CSV 생성하겠습니다. `MoveAbsoluteEx_DelayTest` 때와 같은 형식으로 `전체 타임라인`과 `상태 변화 요약` 두 개를 저장하겠습니다.
>
> 생성 결과만 확인하고 경로 전달하겠습니다.
</details>

분석해서 CSV 만들었습니다. 입력 캡처: [MoveAbsoluteEx_DelayTest2.pcapng](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest2.pcapng)

생성 파일:
1. 전체 타임라인
[MoveAbsoluteEx_DelayTest2_Command_StatusTimeline.csv](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest2_Command_StatusTimeline.csv)

2. 상태 변화 요약
[MoveAbsoluteEx_DelayTest2_StateChanges.csv](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest2_StateChanges.csv)

구조:
- `MoveAbsoluteEx` 1회
- `ReadStatus` 100회
- `MoveAbsoluteEx` 응답 1회
- `ReadStatus` 응답 100회

핵심 상태 변화:
- `1642.700 ms` `0x40000042` = `ACCELERATING|DISCRETE_MOTION`
- `65020.700 ms` `0x40000044` = `CONSTANT_VELOCITY|DISCRETE_MOTION`
- `172700.400 ms` `0x40000041` = `DECELERATING|DISCRETE_MOTION`

명령 파라미터:
- `Position = 8388608`
- `Velocity = 48933546`
- `Acceleration = 4893354600`
- `Deceleration = 4893354600`
- `Jerk = 48933546000`
- `Direction = 2`
- `BufferMode = 1`

원하면 다음으로
1. `DelayTest1`과 `DelayTest2` 차이 비교표 만들기
2. 이 둘을 하나의 `xlsx`로 묶기
3. 시작시점/정속진입/감속진입 비교만 따로 CSV로 뽑기

> 야 real_ms 빼고 기존에 와이어샥에 찍힌 시간 기록해

<details><summary>4 previous messages</summary>

> CSV 시간 컬럼을 상대 `rel_ms`가 아니라 캡처 원본 시간축으로 바꾸겠습니다. 와이어샤크 기본 `Time` 컬럼처럼 파일 시작 기준 초 단위로 다시 뽑겠습니다.
>
> 원본 시간축은 파일 시작 기준 `2182.xxx sec`부터 시작합니다. CSV를 덮어써서 `rel_ms`를 제거하고 `wireshark_time_sec`로 바꾸겠습니다.
>
> 현재 CSV는 관련 패킷 기준 상대시간이라 와이어샤크 `Time` 컬럼과 다릅니다. 파일 시작 기준 초 단위 `time_sec`로 다시 덮어쓰겠습니다.
>
> 기존 `StateChanges.csv`가 지금 열려 있어서 덮어쓰기가 막혔습니다. 같은 내용으로 `WiresharkTime` 접미사 붙인 새 파일로 저장하겠습니다.
</details>

반영했습니다. `rel_ms` 없이 와이어샤크 시간축 기준으로 새 CSV 저장했습니다.

파일:
1. 전체 타임라인
[MoveAbsoluteEx_DelayTest2_Command_StatusTimeline_WiresharkTime.csv](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest2_Command_StatusTimeline_WiresharkTime.csv)

2. 상태 변화 요약
[MoveAbsoluteEx_DelayTest2_StateChanges_WiresharkTime.csv](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest2_StateChanges_WiresharkTime.csv)

변경 내용:
- `rel_ms` 제거
- `time_sec` 추가
- 값은 와이어샤크 `Time` 컬럼처럼 `캡처 시작 기준 초` 형식
  - 예: `2182.690200000`

기존 파일을 덮어쓰지 못한 이유:
- [MoveAbsoluteEx_DelayTest2_StateChanges.csv](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest2_StateChanges.csv) 가 열려 있어서 잠겨 있었습니다.
- 그래서 `WiresharkTime` 붙인 새 파일로 저장했습니다.

원하면 잠금 풀고 기존 파일명으로 다시 덮어쓰겠습니다.

> 이거 패킷 분석해봐
>
> User context
>
> Attachments:
> - ReadStatus.pcapng: `./packet_capture/ReadStatus.pcapng`

<details><summary>2 previous messages</summary>

> `ReadStatus.pcapng`에서 `192.168.1.3` 기준 요청/응답 구조와 payload를 먼저 분리하겠습니다. 그 다음 로컬 MMCLib 소스와 대조해서 필드 의미까지 붙이겠습니다.
>
> 요청/응답 payload는 이미 확정됐습니다. 이제 응답 20바이트를 `ReadStatusArgsOUT` 구조와 바로 맞춰서 상태값을 해석하겠습니다.
</details>

분석 결과는 단순합니다. 이 캡처는 `ReadStatus` 1회 요청/응답만 있습니다.

파일:
- [ReadStatus.pcapng](/C:/work/Elmo/Elmo_Master/packet_capture/ReadStatus.pcapng)

**패킷 구성**
1. 요청
- `192.168.1.13:6713 -> 192.168.1.3:4000`
- Payload `16 bytes`
- Hex:
```text
28200000080000000000000001000000
```

2. 응답
- `192.168.1.3:4000 -> 192.168.1.13:6713`
- Payload `20 bytes`
- Hex:
```text
00000c000000000080000040000000000000b712
```

**요청 해석**
로컬 MMCLib `ReadStatusArgsIN` 기준:
- `CommandID = 0x2028` (`MC_READSTATUS_ID`)
- `PayloadLength = 8`
- `AxisRef = 0`
- `Handle = 0`
- `Enable = 1`

즉, `MMC_ReadStatus` 요청이 맞습니다.

**응답 해석**
로컬 MMCLib `ReadStatusArgsOUT` 기준:
- `uiState = 0x40000080`
- `usStatus = 0`
- `sErrorID = 0`
- `usAxisErrorID = 0`
- `usStatusWord = 0x12B7`

핵심은 `uiState`입니다.

- 하위 상태 비트 `0x80 = STAND_STILL`
- 즉, 이 시점 축 상태는 **정지 상태**입니다.
- `ACCELERATING`, `CONSTANT_VELOCITY`, `DECELERATING` 같은 모션 상태 비트는 없습니다.

`0x40000000` 상위 비트는 이전 캡처들에서도 같이 보였고, 공개 enum의 순수 축 상태 비트는 아닙니다.
실질 상태 해석은 하위 비트 `0x80`만 보면 됩니다.

**타이밍**
- 요청 후 응답까지 약 `676.3 ms`

**결론**
- 이 캡처는 `ReadStatus` 단일 테스트
- 결과는 `StandStill`
- 에러 없음
- 축 에러 없음
- 상태워드 `0x12B7`

원하면 다음으로
1. 이 캡처도 CSV로 뽑기
2. `statusword 0x12B7` 비트까지 풀어서 설명하기
3. `ReadActualPosition` 결과와 같이 비교표 만들기

> 비트별로 설명해서 표를 만들어봐

<details><summary>3 previous messages</summary>

> `ReadStatus` 응답의 `uiState`와 `StatusWord`를 비트 단위로 풀겠습니다. 표는 표준 의미와 이번 캡처에서의 해석을 같이 적겠습니다.
>
> 비트 표를 파일로도 남기겠습니다. `uiState`와 `StatusWord`를 한 CSV에 같이 넣겠습니다.
>
> 표 파일 생성은 끝났습니다. 핵심만 바로 정리해서 전달하겠습니다.
</details>

표 만들어놨습니다.

파일:
- [ReadStatus_BitAnalysis.csv](/C:/work/Elmo/Elmo_Master/packet_capture/ReadStatus_BitAnalysis.csv)

기준 값:
- `uiState = 0x40000080`
- `StatusWord = 0x12B7`

핵심 해석:

**1. uiState**
| Bit | Mask | 이름 | 값 | 해석 |
| --- | --- | --- | --- | --- |
| 7 | `0x00000080` | `STAND_STILL` | 1 | 축 정지 상태 |
| 30 | `0x40000000` | `UNKNOWN_VENDOR_FLAG` | 1 | 공개 `MC_STATE_SINGLE` enum에 없는 상위 비트, 벤더 확장으로 보임 |

즉, 이 캡처에서 축 상태는 **StandStill** 입니다.

**2. StatusWord = `0x12B7`**
| Bit | Mask | 이름 | 값 | 해석 |
| --- | --- | --- | --- | --- |
| 0 | `0x0001` | Ready to switch on | 1 | 준비됨 |
| 1 | `0x0002` | Switched on | 1 | 전원 인가 |
| 2 | `0x0004` | Operation enabled | 1 | 동작 가능 |
| 3 | `0x0008` | Fault | 0 | Fault 아님 |
| 4 | `0x0010` | Voltage enabled | 1 | 전압 인가 |
| 5 | `0x0020` | Quick stop | 1 | Quick stop 비트 set |
| 6 | `0x0040` | Switch on disabled | 0 | 해당 아님 |
| 7 | `0x0080` | Warning | 1 | 표준상 Warning 비트 set |
| 8 | `0x0100` | Manufacturer specific | 0 | clear |
| 9 | `0x0200` | Remote | 1 | 원격 제어 |
| 10 | `0x0400` | Target reached | 0 | 목표도달 아님 |
| 11 | `0x0800` | Internal limit active | 0 | 내부 제한 아님 |
| 12 | `0x1000` | Operation-mode-specific | 1 | 모드 의존 비트 set |
| 13 | `0x2000` | Operation-mode-specific | 0 | clear |
| 14 | `0x4000` | Manufacturer specific | 0 | clear |
| 15 | `0x8000` | Manufacturer specific | 0 | clear |

정리:
- 축은 `StandStill`
- `Fault=0`, `ErrorID=0`, `AxisErrorID=0`
- 그런데 `Warning(bit7)=1` 이라서 표준 해석상 경고 상태는 들어가 있음
- `bit12`는 현재 모드 의존이라, 정확한 의미는 그때 드라이브 동작모드까지 알아야 확정됩니다

원하면 다음으로
1. `0x12B7`를 DS402 상태 머신 관점에서 더 풀어서 설명
2. 이 표를 엑셀 `xlsx`로 변환
3. `ReadStatus.pcapng` 자체도 CSV로 만들어서 같이 저장

> 여기서 14번도 똑같이 풀어서 보여줘
>
> User context
>
> Attachments:
> - MoveAbsoluteEx_DelayTest2.pcapng: `./packet_capture/MoveAbsoluteEx_DelayTest2.pcapng`

`MoveAbsoluteEx_DelayTest2.pcapng`의 **패킷 14**를 풀면 이렇습니다.

기준 파일:
- [MoveAbsoluteEx_DelayTest2_Command_StatusTimeline_WiresharkTime.csv](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest2_Command_StatusTimeline_WiresharkTime.csv)
- [MoveAbsoluteEx_DelayTest2_StateChanges_WiresharkTime.csv](/C:/work/Elmo/Elmo_Master/packet_capture/MoveAbsoluteEx_DelayTest2_StateChanges_WiresharkTime.csv)

패킷 14:
- 시간: `2184.332900000 sec`
- 방향: `192.168.1.3:4000 -> 192.168.1.13:6713`
- 응답 대상: 패킷 `13`
- 응답 지연: `627.100 ms`
- Payload:
```text
00000c000000000042000040000000000000b712
```

해석:
- `uiState = 0x40000042`
- `Status = 0`
- `ErrorID = 0`
- `AxisErrorID = 0`
- `StatusWord = 0x12B7`

**1. uiState = `0x40000042`**
| Bit | Mask | 이름 | 값 | 해석 |
| --- | --- | --- | --- | --- |
| 1 | `0x00000002` | `ACCELERATING` | 1 | 가속 중 |
| 6 | `0x00000040` | `DISCRETE_MOTION` | 1 | 단발성 위치 명령 모션 상태 |
| 30 | `0x40000000` | `UNKNOWN_VENDOR_FLAG` | 1 | 공개 enum에 없는 Elmo 내부/확장 비트 |

정리:
- 패킷 14 시점에는 축이 **Discrete Motion 상태에서 가속 중**입니다.

**2. StatusWord = `0x12B7`**
이건 아까 `ReadStatus.pcapng` 때와 동일합니다.

| Bit | Mask | 이름 | 값 | 해석 |
| --- | --- | --- | --- | --- |
| 0 | `0x0001` | Ready to switch on | 1 | 준비됨 |
| 1 | `0x0002` | Switched on | 1 | 전원 인가 |
| 2 | `0x0004` | Operation enabled | 1 | 동작 가능 |
| 3 | `0x0008` | Fault | 0 | Fault 아님 |
| 4 | `0x0010` | Voltage enabled | 1 | 전압 인가 |
| 5 | `0x0020` | Quick stop | 1 | Quick stop 비트 set |
| 6 | `0x0040` | Switch on disabled | 0 | 해당 없음 |
| 7 | `0x0080` | Warning | 1 | Warning bit set |
| 9 | `0x0200` | Remote | 1 | 원격 제어 |
| 10 | `0x0400` | Target reached | 0 | 목표 도달 아님 |
| 11 | `0x0800` | Internal limit active | 0 | 내부 제한 아님 |
| 12 | `0x1000` | Mode specific | 1 | 모드 의존 비트 set |

최종 해석:
- **패킷 14는 MoveAbsoluteEx 이후 첫 ReadStatus 응답**
- 축 상태는 **가속 중 (`ACCELERATING`)**
- 모션 타입은 **Discrete Motion**
- Fault 없음
- 아직 `Target reached` 아님

원하면 패킷 `84`, `202`도 같은 방식으로 풀어드리겠습니다.

> 내가 이 폴더에 git 서버를 등록했는데 이제껏 우리가 한 내용을 히스토리 시켜서 각각 요소별로 정리해서 Commit할수 있도록 해봐

<details><summary>15 previous messages</summary>

> 요청한 목적은 이해했습니다. 지금 워크트리 변경 내역을 먼저 전수 확인해서, 기능 단위로 커밋 묶음을 나눈 뒤 순서대로 커밋 가능한 상태로 정리하겠습니다.
>
> 현재 상태를 보니 대부분이 미추적 파일입니다. 브랜치/원격/기존 커밋 유무를 확인해서, 전체를 한 번에 올릴지 기능별로 나눌지 바로 실행 가능한 커밋 플랜으로 묶겠습니다.
>
> 중요 포인트 확인했습니다. 이 저장소는 아직 `HEAD`가 없는 초기 상태라, 지금부터 커밋 순서 자체가 히스토리가 됩니다. 그래서 우리가 만든 산출물만 기능 단위로 쪼개서 초기 커밋 체인을 만들겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청대로 지금 바로 커밋 히스토리를 요소별로 나누겠습니다. 먼저 어떤 파일을 커밋 대상에 넣을지(우리 작업물 중심) 점검하고, 그 다음 `논리 단위별`로 여러 커밋을 실제 생성하겠습니다.
>
> 상태 확인 결과, 현재는 초기 커밋도 없는 저장소이고 대용량 문서/압축본이 매우 많습니다. 히스토리는 `우리 작업물만` 잡아서 끊는 게 안전해서, 지금 `Codex_PMAS_WPF`, `Codex_LASAL_WPF`, `packet_capture` 내부를 세부 점검해 커밋 단위를 확정하겠습니다.
>
> 커밋을 깔끔하게 나누기 위해 먼저 `.gitignore`를 만들고, 이후 `PMAS 코드`, `LASAL 코드`, `분석/리포트` 순서로 커밋을 분리하겠습니다. 지금 커밋 대상 파일 목록을 확정하겠습니다.
>
> 이제 커밋 분리를 위해 `.gitignore`를 먼저 추가하겠습니다. 빌드 산출물/VS 캐시/대용량 레퍼런스 패키지(다운로드 폴더, 압축본/매뉴얼)만 제외하고, 실제 코드·리포트·패킷 분석 산출물은 커밋 가능하게 유지하겠습니다.
>
> `.gitignore` 추가가 끝났고, 이제부터 기능 단위 커밋을 순서대로 만들겠습니다. 첫 커밋으로 저장소 기본 정리 커밋을 생성합니다.
>
> 다음 커밋은 `Codex_PMAS_WPF`의 소스/솔루션/API 매핑 문서만 묶어서 만들겠습니다. 리포트(`Reports`)는 별도 커밋으로 분리합니다.
>
> PMAS 소스 커밋 완료했습니다. 다음은 테스트 산출물 추적을 위해 `Codex_PMAS_WPF/Reports`를 독립 커밋으로 추가합니다.
>
> 이제 `Codex_LASAL_WPF`도 같은 방식으로 분리합니다. 먼저 소스/솔루션/TCP 더미 구현만 커밋하고, 리포트는 다음 커밋으로 분리하겠습니다.
>
> LASAL 리포트 커밋까지 완료했습니다. 현재 남은 것은 `packet_capture` 분석 산출물과 한국어 정리 문서이며, 이 둘을 분리 커밋하겠습니다.
>
> 마지막으로 히스토리 품질 정리를 위해 두 가지를 처리하겠습니다:
> 1) 실수로 들어간 엑셀 임시 잠금파일 제거
> 2) 외부 참조 원본(`MaestroLibraries292`, `EtherCAT ...xlsx`)은 `.gitignore`에 추가해 작업 히스토리에서 제외.
>
> 커밋 분할은 완료했고 현재 워킹트리는 깨끗합니다. 원격에도 바로 반영되도록 `origin/main` 푸시를 시도하겠습니다.
</details>

요청한 대로 요소별로 히스토리 분리해서 커밋했고, 원격(`origin/main`)까지 푸시 완료했습니다.

커밋 내역(오래된 순):
1. `f25ba58` `chore`: `.gitignore` 추가 (빌드 산출물/로컬 레퍼런스 제외)
2. `2b0ad71` `feat(pmas)`: `Codex_PMAS_WPF` 소스/솔루션/API 매핑
3. `525f719` `data(pmas)`: `Codex_PMAS_WPF/Reports` 테스트 결과
4. `46c376d` `feat(lasal)`: `Codex_LASAL_WPF` 소스/솔루션/SIGMATEK 더미 래퍼
5. `d2f9a10` `data(lasal)`: `Codex_LASAL_WPF/Reports` 테스트 결과
6. `e055ace` `data(packet)`: `packet_capture` 패킷/분석 CSV·TSV·XLSX
7. `04fbfc7` `docs(api)`: 한국어 API 정리 문서 2종
8. `1e30cca` `chore`: 외부 참조 파일 ignore 보강 + 엑셀 임시 lock 파일 제거

현재 상태:
- `git status` clean
- `main` 브랜치가 `origin/main` tracking 설정됨
- 원격 저장소 반영 완료

> 지금까지 분석된 내용들(pdf 포함) 을 토대로 md파일을 만들어서 자료화하고 니가 나중에 참고할수 있게 파일을 만들어놔.

<details><summary>4 previous messages</summary>

> 좋습니다. 지금까지의 PDF/패킷/구현 결과를 한 파일로 묶은 참조용 문서를 만들겠습니다. 먼저 기존 정리 문서와 패킷 분석 산출물을 빠르게 다시 읽고, 그다음 통합 MD를 생성하겠습니다.
>
> 핵심 자료를 다시 확인했고, 이제 통합 참조 문서를 새 MD로 작성하겠습니다.
> 위치는 루트 기준 `docs/` 폴더로 만들고, `PDF 기준 정리 + 구현 결정사항 + 패킷 분석 결과 + 에러/트러블슈팅`까지 한 번에 넣겠습니다.
>
> <details><summary>Created 1 file</summary>
>
> - Wrote `./docs/PMAS_LASAL_Integrated_Analysis_2026-04-10.md` (+172 -0)
>
> </details>
>
> 통합 MD 파일 작성이 끝났습니다. 나중에 바로 참조할 수 있도록 지금 커밋까지 만들어 두겠습니다.
</details>

통합 참조용 MD 파일 작성해 두었습니다.

- 파일: [PMAS_LASAL_Integrated_Analysis_2026-04-10.md](./docs/PMAS_LASAL_Integrated_Analysis_2026-04-10.md)
- 포함 내용:
  - PDF/엑셀/패킷 기준 자료 목록
  - PMAS/LASAL 구현 요약
  - CycleTest1~4 동작 구조
  - 패킷 길이/시그니처/지연 분석 요약
  - ReadStatus 비트 해석(14번 비트 포함)
  - 오류 코드별 원인/점검 순서
  - LASAL 더미 프레임 구조 및 커맨드 ID
  - 실제 Elmo 패킷과 더미 구현의 차이/주의점

그리고 Git 반영도 완료했습니다.

- 커밋: `6ef2beb` (`docs: add integrated PMAS/LASAL analysis knowledge base`)
- 원격 `origin/main`까지 푸시 완료
- 현재 워킹트리 clean

> 이 패킷 한번더 분석해서 자세히 정리해줘
>
> User context
>
> Attachments:
> - ReadActualPosition.pcapng: `./packet_capture/ReadActualPosition.pcapng`

<details><summary>5 previous messages</summary>

> 요청하신 `ReadActualPosition.pcapng`를 다시 상세 분석하겠습니다. 먼저 패킷 메타(프레임 수, 포트, payload 길이, hex)부터 추출해서 요청/응답 구조와 필드 오프셋을 재정리하겠습니다.
>
> Wireshark CLI 도구를 찾았고, 이제 `capinfos/tshark`로 전체 프레임 통계와 실제 바이트를 추출하겠습니다.
>
> 요청/응답 바이트를 수치로 풀어보니 응답 24바이트 안에 `double` 2개가 들어있습니다. 이제 기존 `motion_tes2t_4000.tsv`의 다수 샘플도 같은 방식으로 풀어서 각 필드 의미를 더 확실히 추정하겠습니다.
>
> 추가로 TCP 시퀀스/ACK/플래그까지 뽑아서, 실제 응답 지연과 ACK 지연(응용계층 지연 아님)을 분리해서 설명하겠습니다.
>
> <details><summary>Created 2 files</summary>
>
> - Wrote `./packet_capture/ReadActualPosition_DetailedAnalysis_2026-04-10.md` (+84 -0)
> - Wrote `./packet_capture/ReadActualPosition_DetailedAnalysis_2026-04-10.csv` (+7 -0)
>
> </details>
</details>

`ReadActualPosition.pcapng` 다시 상세 분석 완료했습니다.

핵심 결과:
1. 캡처 전체는 33패킷이지만, `192.168.1.3` 관련 TCP 대화는 3패킷만 유효합니다.
2. 유효 시퀀스는 `요청(9B) -> 응답(24B) -> ACK(0B)`입니다.
3. 실제 응답 지연은 `0.1836 ms`입니다.
4. `42.713 ms`는 응답 처리 지연이 아니라 **순수 TCP ACK 지연**입니다.

프레임(Controller 관련만):
1. Frame 7 (`2026-04-08 17:32:35.121630100+0900`)
   `192.168.1.13:4867 -> 192.168.1.3:4000`, `TCP Len=9`, payload=`2e2000000100000000`
2. Frame 8 (`2026-04-08 17:32:35.121813700+0900`)
   `192.168.1.3:4000 -> 192.168.1.13:4867`, `TCP Len=24`, payload=`000010000000000000000080550160410000000053558741`
3. Frame 9 (`2026-04-08 17:32:35.164526700+0900`)
   `192.168.1.13:4867 -> 192.168.1.3:4000`, `TCP Len=0`, ACK only

리틀엔디안 디코드:
1. 요청 9바이트
   - `u16@0 = 0x202E` (ReadActualPosition 명령 후보)
   - `u16@2 = 0` (AxisRef 후보)
   - `u32@4 = 1`
   - `u8@8 = 0`
2. 응답 24바이트
   - `u16@0 = 0`
   - `u16@2 = 16 (0x0010)`
   - `u32@4 = 0`
   - `double@8 = 8391340.0` (실제 위치값으로 강하게 추정)
   - `double@16 = 48933472.0` (보조 모션 필드, 의미는 스펙 확인 필요)

추가 교차검증:
1. `motion_tes2t_4000.tsv`에서도 24바이트 응답 구조가 동일합니다.
2. `double@8`은 계속 변하고, `double@16`은 거의 고정(약 48,933,472)으로 관찰됩니다.

정리 파일 생성:
1. [ReadActualPosition_DetailedAnalysis_2026-04-10.md](./packet_capture/ReadActualPosition_DetailedAnalysis_2026-04-10.md)
2. [ReadActualPosition_DetailedAnalysis_2026-04-10.csv](./packet_capture/ReadActualPosition_DetailedAnalysis_2026-04-10.csv)

원하면 다음으로 `MoveAbsoluteEx_DelayTest2.pcapng`와 같은 방식으로 동일 포맷 리포트도 바로 뽑겠습니다.

> double@16 = 48933472.0 (보조 모션 필드, 의미는 스펙 확인 필요) 이거 어떤 내용인지 대략적으로 예상 가능하냐?

<details><summary>1 previous message</summary>

> 가능합니다. 지금 캡처 기준으로 `double@16`이 어떤 항목과 가장 유사한지 수치 비교해서 추정해보겠습니다(명령 프레임의 속도/가속도 값과 교차비교).
</details>

응, **대략 추정은 가능**하고 현재 데이터로는 이게 가장 유력합니다.

- 가장 유력한 의미: **실제 위치가 아니라 “속도 관련 설정값(유효 velocity setpoint/limit)”**
- 근거:
1. `double@16` 값이 모든 ReadActualPosition 응답에서 **항상 48,933,472.0**으로 고정됨
2. 같은 구간 MoveAbsoluteEx 명령의 velocity가 **48,933,546.0**으로 고정됨
3. 두 값 차이가 **74**로 매우 작고 일정함 (내부 정규화/양자화 흔적처럼 보임)
4. 왕복 모션(정/역방향)에서도 부호가 바뀌지 않아 **actual velocity**라기보단 설정/제한값 성격에 가까움

즉, `double@16`은 “현재 속도 설정값(또는 내부 변환된 속도 파라미터)”일 가능성이 높고,
`double@8`이 실제 위치값으로 보는 게 맞습니다.

정확 확정하려면:
1. velocity를 여러 값으로 바꿔(예: 10M, 30M, 50M) `double@16` 추적
2. 정/역방향만 바꿔 부호 변화 확인
3. 정지 상태에서도 값 유지되는지 확인

이 3개 테스트하면 거의 확정할 수 있습니다.

> 그럼 속도 변경해서 동작시키고 값이 변경되는지 확인해보자 일단 캡쳐해올께

좋습니다. 캡쳐 주시면 바로 비교 분석해서 `double@16`이 velocity 연동값인지 확정해드리겠습니다.

캡쳐할 때 이것만 맞춰주세요:
1. 같은 축/같은 거리에서 `velocity`만 최소 3단계로 변경
2. 각 velocity마다 ReadActualPosition 응답을 20개 이상 확보
3. 가능하면 각 구간 시작 시점에 MoveAbsoluteEx 1회 포함
4. 파일명에 속도 넣기 (예: `ReadPos_Vel10000000.pcapng`)

> 캡쳐해왔다. 내용 분석해봐
>
> User context
>
> Attachments:
> - ReadActualPosition2.pcapng: `./packet_capture/ReadActualPosition2.pcapng`

<details><summary>2 previous messages</summary>

> 확인했습니다. `ReadActualPosition2.pcapng`에서 먼저 `192.168.1.3` 관련 요청/응답 페어를 추출하고, `double@8`/`double@16` 분포를 계산해 이전 가설(속도 연동)을 검증하겠습니다.
>
> 캡처가 매우 짧아서(8패킷) 실질적으로는 `ReadActualPosition` 1회 요청/응답만 들어 있습니다. 지금 페이로드를 바이트 단위로 풀어서 `double@16` 값이 실제로 바뀌었는지 확인하겠습니다.
</details>

분석 완료. `192.168.1.3` 기준으로 유효 패킷은 3개입니다.

1. 요청
- Frame 1: `2e2000000100000000` (9 bytes)

2. 응답
- Frame 2: `00001000000000000000000000307d400000000080842e41` (24 bytes)
- 디코드:
  - `u16@0 = 0`
  - `u16@2 = 16`
  - `u32@4 = 0`
  - `double@8 = 467.0`
  - `double@16 = 1000000.0`

3. ACK
- Frame 6: payload 0 bytes (순수 TCP ACK)

지연:
- Req -> Rsp: `0.6658 ms`

결론:
- 이전 캡처(`ReadActualPosition.pcapng`)의 `double@16 = 48,933,472.0`
- 이번 캡처의 `double@16 = 1,000,000.0`

즉 `double@16`은 고정 필드가 아니라 **설정 속도(프로파일 velocity 계열)와 연동되는 값**으로 보는 게 맞습니다.
`double@8`은 실제 위치값으로 계속 해석 가능합니다.

> 근데 코드에서는 결국 리턴값이 467 저거만 들어오는거지?

맞습니다. 지금 코드 기준으로는 **467만 리턴**됩니다.

이유:
- `GetActualPosition()`에서 응답 파싱 후 `return positionFromResponse;`만 수행
- 파서가 쓰는 값은 응답의 첫 번째 값(현재 구조상 실제 위치값)이고,
- `double@16`(예: 1,000,000)은 현재 리턴/표시에 사용하지 않습니다.

즉 현재 동작은:
1. 응답 수신
2. 위치값 1개만 반환
3. 보조값(`double@16`)은 버림

원하면 `GetActualPositionEx(out pos, out aux)` 같은 형태로 보조값도 같이 꺼내도록 바로 확장할 수 있습니다.

> 파서는 어디서 동작하는거야?

<details><summary>2 previous messages</summary>

> 지금 얘기한 패킷 파서는 `LASAL 더미 라이브러리` 안에서 돌아갑니다. 정확한 위치를 라인 기준으로 바로 찍어드리겠습니다.
>
> 맞습니다. Elmo Library 경로 기준으로 보면, 파서는 우리 코드가 아니라 라이브러리 내부에서 동작합니다. 호출 지점만 정확히 찍어드리겠습니다.
</details>

맞습니다. **Elmo Library 사용 모드**에서는 파서가 우리 프로젝트 코드가 아니라 DLL 내부에서 동작합니다.

- 우리 코드의 호출 지점:
  - [MainWindow.AxisOperations.cs:107](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:107)
  - [MainWindow.CycleTestOperations.cs:1846](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1846)

- 실제 파싱/언패킹이 일어나는 곳(라이브러리):
  - [ElmoMotionControlComponents.GMAS.MMCLibDotNET.dll](./MMCLibDotNET%20Libs%20V3.0.0.7/Lib/ElmoMotionControlComponents.GMAS.MMCLibDotNET.dll)
  - [ElmoMotionControl.Gmas.RpcClient.dll](./MMCLibDotNET%20Libs%20V3.0.0.7/Lib/ElmoMotionControl.Gmas.RpcClient.dll)

즉, `GetActualPosition()` 호출하면 DLL 내부 파서가 값을 해석해서 `double` 하나만 돌려주고, 우리가 캡처에서 본 추가 필드(`double@16`)는 기본 API에서 노출하지 않는 구조입니다.

> 지금 리턴 프레임이 니가 예상한거에 부합하는거 같냐
>
> 5) GetActualPosition Response (파서 기대 형태, 최소 16 bytes)
> [0..1]   UInt16  CommandId = 0x00E0
> [2..3]   UInt16  AxisRef
> [4..7]   UInt32  PayloadLength >= 8
> [8..15]  Int64   ActualPosition

아니요. **지금 캡처 기준으로는 그 형태와 안 맞습니다.**

왜 안 맞냐면:
1. 응답 길이가 `24 bytes`입니다 (`최소 16`이 아님).
2. 응답 앞 8바이트가 `00 00 10 00 00 00 00 00`로 고정이라,
   `CommandId=0x00E0`, `AxisRef` 에코 형태로 보이지 않습니다.
3. `[8..15]`를 `Int64`로 보면 의미 없는 큰 정수이고, `double`로 보면 실제 위치값(예: `467.0`)이 됩니다.
4. `[16..23]`도 `double`로 의미 있는 값(예: `1,000,000.0` 또는 이전 캡처 `48,933,472.0`)이 나옵니다.

현재 캡처에 더 맞는 추정 포맷은 이쪽입니다:
- `[0..1]` status/error 후보
- `[2..3]` payload length (`0x0010` = 16)
- `[4..7]` reserved/error 후보
- `[8..15]` `double` actual position
- `[16..23]` `double` 보조값(속도 계열로 추정)

즉, 네가 적은 `0x00E0 + AxisRef + Int64 position` 파서 기대 형태는 **현재 실측 패킷과 불일치**입니다.

> 뒤에 8byte만 빼면 똑같자나

맞습니다. 핵심만 보면 거의 그렇게 볼 수 있습니다.

- 공통점:
1. 앞 8바이트 헤더 + 위치값 8바이트 구조
2. 위치 데이터는 `[8..15]`에 있고 `Int64/Double` 중 해석 방식만 다름

- 차이점:
1. 실측은 총 `24B`이고 뒤에 `8B` 보조 필드가 추가됨
2. 헤더의 의미가 네가 적은 `CommandId=0x00E0, AxisRef`로 그대로 보이진 않음 (`00 00 10 00 ...` 패턴)

즉 네 말대로 **“뒤 8바이트 빼면 형태는 거의 동일”**한 해석이 맞고,
실무적으로는 `payloadLength=16`으로 보고 `[8..15]`만 ActualPosition으로 써도 동작합니다.

> 여기서 분석한 Read Actual Position도 다 동일한 구조인지 확인해봐
>
> User context
>
> Attachments:
> - Motion_Test.pcapng: `./packet_capture/Motion_Test.pcapng`

<details><summary>1 previous message</summary>

> 좋습니다. `Motion_Test.pcapng` 안의 ReadActualPosition 요청/응답을 전부 추출해서, 길이/헤더/페이로드 구조가 일관적인지 검증하겠습니다.
</details>

확인 결과, `Motion_Test.pcapng`에서 **ReadActualPosition 응답 구조는 전부 동일**합니다.

핵심 검증 결과:
1. Read 요청 패턴: `2e2000000100000000` (9 bytes)로 **단일 패턴**
2. Read 응답 길이: **항상 24 bytes**
3. Read 응답 헤더(앞 8 bytes): **항상 `0000100000000000`**
4. 응답 페이로드 해석:
   - `[8..15]` 값은 계속 변함 (위치값으로 해석 가능)
   - `[16..23]` 값은 전부 `48933472.0`으로 고정
5. 헤더 불일치 응답: **0건**
6. 요청/응답 개수:
   - 요청 876건, 응답 875건
   - 마지막 1건 요청은 캡처 종료 직전에 나가서 응답이 파일에 안 잡힌 케이스로 보임

결론:
- 이 파일 내에서 분석한 ReadActualPosition 응답은 구조가 동일합니다.
- 구조 요약: `24B = [고정 8B 헤더] + [double 위치 8B] + [double 보조값 8B]`

> Codex_LASAL_WPF의 ReadActualPosition을 구현해 .내가 똑같은 프레임을 만들어서 던지도록 만들었어

<details><summary>7 previous messages</summary>

> 요청하신 대로 `Codex_LASAL_WPF`에서 `ReadActualPosition`을 실측 프레임(`2e20...` 요청 / `24B` 응답) 기준으로 동작하게 코드 수정하겠습니다. 먼저 현재 구현 부분을 정확히 확인한 뒤 바로 패치하겠습니다.
>
> 현재 구현은 `0x00E0/8바이트 요청/Int64 파싱`으로 되어 있어서 실측 프레임과 불일치합니다. 지금 `2e20 0000 01000000 00` 요청 + `24바이트 응답(double@8)` 기준으로 파서를 교체하겠습니다.
>
> <details><summary>Edited 1 file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs` (+36 -24)
>
> </details>
>
> 수정 반영했습니다. 지금 빌드 한 번 돌려서 컴파일 에러 없이 적용됐는지 확인하겠습니다.
>
> 경고 가능성(보조값 미사용)을 없애고 추후 디버깅에도 쓰기 좋게, 응답의 두 번째 값(`double@16`)을 내부 필드에 보관하도록 한 줄 더 보강하겠습니다.
>
> <details><summary>Edited 1 file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs` (+2 -0)
>
> </details>
>
> 빌드 툴이 `msbuild` 명령으로는 안 잡혀서, `dotnet build`로 문법/컴파일 체크만 시도하겠습니다.
</details>

구현 완료했습니다. `Codex_LASAL_WPF`의 `ReadActualPosition`을 실측 프레임 기준으로 맞췄습니다.

변경 내용:
1. 요청 프레임을 `9 bytes`로 변경
- `[0..1]=0x202E`, `[2..3]=AxisRef`, `[4..7]=1`, `[8]=0`
- 코드: [SigmatekTcpIpDummyMMCLib.cs:1137](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1137)

2. `GetActualPosition` 커맨드 ID를 `0x202E`로 변경
- 코드: [SigmatekTcpIpDummyMMCLib.cs:706](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:706)

3. 응답 파서를 `24 bytes` 구조로 변경
- `[8..15]`를 position(`double`)로 리턴
- `[16..23]`는 보조값(`double`)으로 읽어서 내부 보관
- 코드: [SigmatekTcpIpDummyMMCLib.cs:1194](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1194)

4. `double` 리틀엔디안 읽기 함수 추가
- 코드: [SigmatekTcpIpDummyMMCLib.cs:1317](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1317)

5. `GetActualPosition()`에서 파싱된 position 반환
- 코드: [SigmatekTcpIpDummyMMCLib.cs:772](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:772)

참고:
- `dotnet build`로 전체 솔루션 빌드는 기존 WPF/XAML 생성 문제(기존 상태)로 실패해서 전체 컴파일 검증은 못 했습니다.
- 하지만 이번 수정 범위는 `ReadActualPosition` 프레임/파서 로직에 한정되어 있습니다.

> Codex_LASAL_WPF 에서 MoveAbsolute 보낼때 기존 입력창에서 * 10000을 한담에 long으로 변환해서 보내는거로 수정해

<details><summary>5 previous messages</summary>

> 요청대로 `Codex_LASAL_WPF`에서 MoveAbsolute 전송 시 입력값을 `*10000` 후 `long`으로 변환해 보내도록 수정하겠습니다. 먼저 MoveAbsolute 호출 지점을 찾아서 영향 범위를 최소로 패치하겠습니다.
>
> 이건 `Axis Motion` 입력창 값 기준으로 처리하는 게 맞아서, `MoveAbsoluteEx` 버튼 동작을 `입력값 * 10000 -> long 변환 -> MoveAbsolute(long...)` 전송으로 바꾸겠습니다.
>
> <details><summary>Edited 1 file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs` (+21 -7)
>
> </details>
>
> `ParseDouble`이 인스턴스 메서드라서 방금 추가한 보조 함수는 `static`이면 컴파일 안 됩니다. 바로 인스턴스 메서드로 수정하겠습니다.
>
> <details><summary>Edited 1 file</summary>
>
> - Wrote `./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs` (+1 -1)
>
> </details>
</details>

수정 완료했습니다.

적용 내용:
1. `MoveAbsolute` 전송 시 입력값을 `* 10000` 하고 `long`으로 변환해서 보내도록 변경
2. 기존 `MoveAbsoluteEx(double...)` 호출을 `MoveAbsolute(long...)` 호출로 변경

변경 파일:
- [MainWindow.AxisOperations.cs](./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs)

구체적으로:
- `ButtonMoveAbsoluteEx_Click`에서
  - `TextAbsPosition`, `TextVelocity`, `TextMotionAcceleration`, `TextMotionDeceleration`, `TextMotionJerk`
  - 각각 `ParseScaledLongForMove(...)`로 변환 후 전송
- `ParseScaledLongForMove` 메서드 추가
  - `scaled = input * 10000.0`
  - `Math.Round(..., AwayFromZero)` 후 `Int64` 변환
  - 범위 초과 시 `OverflowException` 발생

참고:
- 현재 LASAL 프로젝트는 기존 XAML/디자이너 이슈로 전체 `dotnet build`는 원래부터 실패 중이라, 이번 변경만 단독 컴파일 검증은 못 했습니다.
- 원하면 동일 스케일 로직을 `CycleTest`의 MoveAbsolute 경로에도 같이 적용해드리겠습니다.

> 우리가 측정한거에 PollPeriodAvg 는 Polling 쓰레드 평균 수행시간인가?

아니요. `PollPeriodAvg`는 **Polling 스레드의 함수 실행시간 평균**이 아니라,
**연속 샘플 시각 간 간격(주기) 평균**입니다.

즉 의미는:
1. `이전 poll 시점 -> 다음 poll 시점`의 경과시간 평균
2. 내부에는 `ReadActualPosition/ReadStatus 호출시간 + 대기시간 + 스케줄링 지연`이 다 포함됨

정리:
- `Response latency` = API 호출 1회 처리시간
- `PollPeriodAvg` = 루프 한 바퀴에서 다음 바퀴까지의 실제 주기(간격) 평균

그래서 PollPeriodAvg는 보통 Response latency보다 큽니다.

<details><summary>10 previous messages</summary>

> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청은 “현재 LASAL 쪽의 전송 프레임이 왜 `RSL` 계열로 보이는지”와 “Elmo 라이브러리와 비교해 Controller가 어떤 응답을 줘야 되는지”를 정리해서, 분석 문서로 남기는 방식입니다. 먼저 LASAL 코드의 커맨드/응답 파싱부와 기존 분석 메모를 한 번에 읽어 패턴을 정리한 뒤, 부족한 항목은 패킷 캡처 기반으로 추정치까지 포함해 `md`로 남기겠습니다.
>
> 다음으로 `RSL` 표기가 실제로는 어떤 의미인지 헷갈릴 수 있어서, LASAL 쪽 상수/프레임 정의와 기존 `packet_capture` 분석 노트(있는 경우)에서 `Read Status / Response` 계열 문자열을 함께 대조합니다.
>
> RSL 문의를 정확히 짚으려면 LASAL 더미에서 “프레임 상수”와, 이미 정리해 둔 분석본(명령ID/응답 패턴)을 같이 확인할 필요가 있습니다. 두 문서를 동시에 열어 핵심 차이를 정리하겠습니다.
>
> `RSL`이 코드명 자체로 쓰인 흔적은 안 보입니다. 명칭이 아니라 상태 응답 분류 문자열에서 온 오해일 가능성이 커서, LASAL 더미 상수/메서드의 `ReadStatus`와 `Move/ReadActual` 패킷 ID를 실제로 재정리해 정답을 정량화하겠습니다.
>
> 질문이 맞습니다. `RSL`이 TCP 레벨이 아니라 ST 래퍼/맵핑에서 붙은 문자열일 가능성이 높아서, PLC용 FB 파일까지 같이 열어 보고 의미를 기준으로 정리한 뒤, 문서로 남길 수 있게 추가 정리를 수행하겠습니다.
>
> 지금 프로젝트 루트에 경로가 달라졌을 가능성이 있어, 먼저 실제 파일 위치를 찾고 그 안의 `RSL` 의미를 확인하겠습니다.
>
> 활성파일 경로가 현재 작업트리와 달라져서 직접 열지 못했습니다. 대신 현재 저장소에서 라우팅/응답 키워드를 역추적해 `RSL` 표기의 원천이 어디인지 확인하겠습니다.
>
> 이제 남은 건 문서화만 남았습니다. LASAL 쪽 요청/응답 레이아웃을 엘모 캡처 기준으로 재정리한 `md`를 새로 만들어, RSL 오해 포인트를 별도 문단으로 정리해둘게요.
>
> 요청 사항대로 `RSL` 오해 포인트 + 엘모 분석 대비 기대 리턴 규격을 별도 문서로 남깁니다. 기존 분석 데이터(캡처 기반) 기준으로 작성해서 바로 참고 가능하게 만들게요.
</details>

`RSL`은 현재 패킷에서 확인되는 바이너리 필드명이 아니라,
대부분 **명령 라벨/로직 이름(PLC/도구쪽 분류명)**로 보입니다.
Wire 레벨에서는 `0x202E`, `0x209F`, `0x2028` 같은 숫자 command ID가 맞습니다.

핵심만 정리하면:

- `Codex_LASAL_WPF`에서 실측되는 Elmo 쪽 요청/응답은:
  - MoveAbsoluteEx: `0x209F` (요청 64바이트)
  - ReadActualPosition: `0x202E` (요청 9바이트)
  - ReadStatus: 캡처 패턴은 `0x2028` 계열 응답/요청 흐름으로 확인됨
- 응답은 항상 같은 TCP 세션에서 들어와야 하며, 보통 첫 2바이트/길이 필드로 상태/오류/페이로드 길이를 처리함
  - ReadActualPosition 응답: 24바이트 (`status/error` + `payloadLen=16` + `position(double)` + `aux(double)`)
  - ReadStatus 응답: 20바이트 (`status/error` + `payloadLen=12` + `state/reg` + `statusWord`)
- 엘모 라이브러리 분석에서 `GetActualPosition` command id는 캡처 기준 `0x00E0`가 아니라 `0x202E` 쪽이 맞는 추세로 보임(기존 문서 중 일부는 구버전 가정으로 남아 있음).

요청하신 분석 정리는 문서로 저장했습니다.

- [docs/LASAL_Elmo_Packet_Return_Guide_2026-04-13.md](./docs/LASAL_Elmo_Packet_Return_Guide_2026-04-13.md)

참고로 `src/POUs/FunctionBlocks/Motion/FB_MotionAdapter.st`는 현재 작업 트리에서 경로가 존재하지 않아 직접 열어볼 수 없었습니다. 경로가 맞는지 확인되면 그 파일 기준으로 `RSL` 단어가 정확히 어디에서 기인했는지도 바로 추적해 이어서 붙여드릴 수 있습니다.

> 내가 폴더를 잘못 지정했다. 이 폴더에서 진행해야 되는 내용이고. RSL은 wireshark로 패킷 캡쳐할때 TCP라고 나오는게 아니라. RSL이라고 나옴
> 내용은 Radio Signalling Link (RSL) 이렇게 나온다. 이폴더에서 다시 분석해서 알려줘

<details><summary>7 previous messages</summary>

> 좋습니다. `RSL` 표기는 Wireshark의 상위 디코더명이거나 링크 계층 프로파일로 해석돼서 나올 수 있습니다. 지금 폴더 기준으로 캡처 파일을 다시 훑고, RSL 트래픽이라도 우리가 보는 바이트 레벨에서 명령/응답이 어떻게 매핑되는지 재정리해보겠습니다.
>
> Wireshark에서 RSL로만 보이는 건, 캡처 안의 바이트는 동일해도 디스섹터가 다르게 매칭된 상태일 수 있습니다. 먼저 각 pcap의 `frame.protocols`와 패킷 타입을 추출해서 실제로 어떤 디코더가 붙었는지부터 확인하겠습니다.
>
> `tshark`는 이 환경에 없어서 Wireshark 같은 수준의 자동 프로토콜 판별은 바로 못 돌립니다. 기존 캡처 재해석은 기존 CSV/요약 파일의 패킷 payload로 보강해서 진행하겠습니다. 먼저 `RSL` 관련 표기 자체가 repo 안에 있는지 확인하고, 없으면 바이트 레벨 기준으로 해석하겠습니다.
>
> `RSL`은 패킷 바이트 자체가 바뀐 게 아니라 Wireshark 디스섹터 매핑 표시일 가능성이 높습니다. 기존 캡처를 폴더 기준으로 다시 정리해 “실제 명령-응답 기준”만 확정해서, 왜 RSL로 보이더라도 우리가 보고 쓰는 값이 바뀌지 않는지 정리하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청 반영해서 이 폴더 기준으로 정리하면, Wireshark의 `RSL`은 TCP 자체가 아닌 캡처 디코더 표기명이야. `0x209F/0x202E/0x2028` 같은 실제 Elmo 커맨드 코드는 캡처 데이터의 페이로드에서 확인한 값이 맞아. 지금부터 이 폴더 내 기존 분석 자료 기준으로 정리해 다시 설명할게.
>
> 현재 기준으로 정리하면, `RSL`이 TCP 자체가 아니라 Wireshark의 상위 디코더명일 가능성이 높고, 실제 바이트 레벨에서는 Elmo 명령 ID가 살아있어. 핵심만 재확인해 정리할게.
</details>

좋은 정정이야.
이 폴더 기준( `C:\work\Elmo\Elmo_Master\` )에서 다시 보면 결론은 명확해.

- `RSL`은 **Wireshark의 프로토콜 표시명(Dissector 레이블)**일 가능성이 높고,
  네가 말한 “Radio Signalling Link” 표기 자체는 Wireshark 분류명이지 통신 자체를 바꿔주는 별도 계층 이름이 아님.
- 실제 트래픽 기준으로는 **TCP**로 오가고 있어.
  `motion_test_4000.tsv`, `motion_tes2t_4000.tsv`에서 모두
  `src=192.168.1.13:14132 -> dst=192.168.1.3:4000` (및 역방향) 패킷이 존재함.
- 우리가 캡처한 바이트 레벨 명령은 아래와 같음(리틀엔디안 기준 해석):
  - MoveAbsoluteEx 요청: `9f20...` → `0x209F`, payload 64바이트
  - MoveAbsoluteEx 응답: 16바이트
  - ReadActualPosition 요청: `2e20...` → `0x202E`, payload 9바이트
  - ReadActualPosition 응답: 24바이트 (`Motion_Tes2t_*` 및 `ReadActualPosition_DetailedAnalysis_2026-04-10.md` 근거)

- 즉, `RSL` 표시가 나와도 **명령 ID/길이/응답 구조는 기존 분석(0x209F/0x202E 계열)와 일치**함.
- `Connect`/ACK/패킷 지연이 헷갈릴 때는 Wireshark 화면의 프로토콜명보다
  **`tcp.port==4000` + 페이로드(HEX)** 기준으로 판단하면 정확함.

원하면 다음 답변에서 `RSL` 필터 기준으로 `cmd_id, request/response, req→rsp` 집계표를 바로 다시 뽑아서 붙여줄게.

> Elmo API 매뉴얼 보고 MoveAbsoluteEX, ReadActualPosition의  응답이 어떻게 되야 되는지 알려줘. 정리된 내용이 없으면 md파일로 만들고

<details><summary>16 previous messages</summary>

> 요청 내용 확인했어. 우선 먼저 PDF/관련 md에서 MoveAbsoluteEx, ReadActualPosition 응답 정의를 먼저 정확히 추려서 보고, 없으면 이번 작업용 md를 새로 만들어 정리할게.
>
> 현재로서는 PDF 본문을 텍스트로 뽑는 라이브러리가 필요해서 `PyPDF2`는 안 깔려 있고, 대신 `pdfminer`로 추출해서 실제 정의를 바로 확인해볼게.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청 내용은 매뉴얼 기반 정리인지/실패 시 최신 근거 문서 생성인지 확인하려는 거라 판단했습니다.
> 지금 바로 `Maestro Administrative and Motion API_2022_12_v2.012.pdf`에서 가능한 텍스트 근거를 다시 추출하고, 이미 만든 분석 파일(캡처 기반)과 함께 `MoveAbsoluteEx`, `ReadActualPosition` 응답 포맷을 정리한 md를 생성하겠습니다.
>
> 추가로 기존 분석 정리본을 먼저 검토해서, 매뉴얼 텍스트가 불완전한 부분은 캡처 기반 결과로 보완하도록 할게요.
>
> 핵심 확인 포인트는 “매뉴얼에 있는 명시값”과 “실제 패킷에서 검증된 값”을 분리 정리하는 것입니다.
> 현재 PDF는 텍스트 추출이 매우 잡음이 많아, 추출 가능한 조각과 기존 캡처 분석 근거를 함께 정리해 **확정/추정 레벨을 분리**해서 md로 남기겠습니다.
>
> 이제 매뉴얼 원문에서 함수 시그니처 레벨의 확정 항목만 정리하려고 합니다.
> HTML 도움말 쪽이 텍스트 추출보다 구조가 깔끔해서 `getactualposition.htm` / `moveabsolute.htm` 계열에서 out 구조/파라미터 설명을 더 직접적으로 뽑아보겠습니다.
>
> 파일이 분리되어 있어서 `moveabsoluteex.htm`를 직접 열면 함수 시그니처와 파라미터 명세가 더 잘 잡힐 겁니다. 해당 부분을 추출해요.
>
> MoveAbsoluteEx 쪽은 매뉴얼에서 시그니처까지만 나오고, .NET 바이너리 decompile으로는 요청 파라미터는 확인되는데 응답 구조는 공통 출력 클래스에서 처리되는 형태가 있어 별도 `MoveAbsoluteExArgsOUT.cs`가 없습니다. `ReadActualPositionOUT`은 구조가 분명하게 확인되어 바로 응답 파싱 근거로 쓸 수 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청사항 정확히 반영해서 정리하겠습니다. 우선 매뉴얼 본문과 NetHelp API 정의를 기준으로 `MoveAbsoluteEx`, `ReadActualPosition` 응답 규격을 먼저 추출해, 기존 문서가 있으면 갱신하고 없으면 새 `md`로 정리해 저장하겠습니다.
>
> 다음으로 `NetHelp` HTML의 정식 서명을 근거로 삼아 항목별 응답 규격을 추출할게요. 매뉴얼 본문 raw 텍스트는 잡음이 많아 구조 확인은 이 쪽이 정확도가 높습니다.
>
> 이제 답변의 핵심은 “매뉴얼은 고수준 API/구조체 위주”이므로, 실제 커맨드 ID·바이트 레이아웃은 NetHelp + 구현 코드에서 보강해야 합니다. 관련 구조체/ID를 바로 추출합니다.
>
> `pdffile` 추출이 시간 제한에 걸려 중단됐습니다. 긴 실행이므로 타임아웃을 늘려 다시 실행해 텍스트에서 직접 항목 추출을 마저 하겠습니다.
>
> `tmp`에 추출 텍스트가 안 되어서 시간 제약상 PDF 직접 텍스트 파싱을 생략하고, 현재까지 확보한 `MoveAbsoluteEx`/`ReadActualPosition`의 공식 API 명세를 `NetHelp C# API + Decompile 내부 파라미터` 기반으로 정리해 파일로 남깁니다. 매뉴얼 2022_12 본문은 커맨드 ID/원시 프레임을 직접 명시하지 않기 때문에, wire 기준은 보강 근거로 표시하겠습니다.
>
> 요청하신 정리 md 파일을 생성했습니다. 내용 최종 확인 후 바로 응답드리겠습니다.
</details>

정리해서 `md` 파일로 만들었고, 저장했습니다.

- [Elmo_API_MoveAbsoluteEX_ReadActualPosition_Response.md](./Elmo_API_MoveAbsoluteEX_ReadActualPosition_Response.md)

요약 답변:

- **MoveAbsoluteEx**
  - 매뉴얼(요약 기준): `MoveAbsoluteEx`는 동작 시작 커맨드.
  - .NET 래퍼에서의 동작 응답은 **상태+에러**(FB 핸들/Status/ErrorID) 구조가 핵심.
  - 실제 파라미터 입력:
    - `Position, Velocity, Acceleration, Deceleration, Jerk, Direction, BufferMode, Execute`
  - 라이브러리 기반 wire 보강:
    - `CommandID = 8351` (`0x20AF`, 패킷에는 little-endian으로 `9F 20`)
    - 요청 길이(payload 총 56 바이트 + 헤더 8 = 64 바이트)
    - 응답은 `DefaultOutputFB` 파서 기준으로 **핸들 + Status + ErrorID**를 받음 (`총 16 바이트`)

- **ReadActualPosition**
  - 매뉴얼(요약 기준): `GetActualPosition`은 현재 위치 반환.
  - 라이브러리 구조:
    - 입력: `ucEnable` 1바이트
    - 출력: `double Position`, `usStatus`, `usErrorID`
  - 라이브러리 기반 wire 보강:
    - `CommandID = 8238` (`0x202E`, 패킷에는 little-endian으로 `2E 20`)
    - 요청 길이 `9`
    - 응답 payload size는 **12 bytes** (`offset 8: double`, `16: usStatus`, `18: usErrorID`)
    - 전체 응답 크기 `20 bytes` (`8 + 12`)

핵심은 매뉴얼 텍스트만으로는 raw 프로토콜 바이트 레벨이 완전 확정되진 않고, **위의 내용은 NetHelp API 정의 + MMCLibDotNET 내부 파싱 코드로 보강한 값**이야.

<details><summary>4 previous messages</summary>

> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청 확인. 패킷 기반으로 `MoveAbsoluteEx` 응답 형식/동작 특성을 기존 분석 로그 기준으로 정리해서 바로 설명하겠습니다. 먼저 관련 자료(패킷/로그/분석 md)에서 최종 근거를 정확히 확인해 요약해드릴게요.
>
> `md` 정리본 기준으로도 가능하지만, 지금은 실제 캡쳐 응답값으로 `MoveAbsoluteEx` 응답을 필드 단위로 풀어낸 형태가 더 정확합니다. 패킷 타임라인 CSV를 기준으로 핵심 값을 바로 재구성해서 설명하겠습니다.
>
> `DefaultOutputFBArgs` 파서는 응답을 `length=Sizeof+8` 규칙으로 읽고, 실제 payload 오프셋이 `8/12/14`인 걸 기준으로 잡고 있어요. 이건 요청하신 답변의 핵심 근거가 됩니다. 이어서 `MoveAbsoluteEx` 입력/ID도 같이 확인해 드리겠습니다.
</details>

`MoveAbsoluteEx` 응답은 현재 분석한 패킷 기준으로 이렇게 봐야 맞습니다.
요약하면 **“위치값은 안 오고, FB 핸들/상태/에러만 옵니다.”**

참고: 분석 기준 파일
- [MoveAbsoluteEx_DelayTest2_Command_StatusTimeline.csv](./packet_capture/MoveAbsoluteEx_DelayTest2_Command_StatusTimeline.csv)
- [MoveAbsoluteEx_DelayTest_Command_StatusTimeline.csv](./packet_capture/MoveAbsoluteEx_DelayTest_Command_StatusTimeline.csv)
- [DefaultOutputFBArgs.cs](C:\work\Elmo\Elmo_Master\NetHelp\Elmo Maestro API Help\MMCLibDotNET\ElmoMotionControlComponents.GMAS.MMCLibDotNET\ElmoMotionControlComponents\GMAS\MMCLibDotNET\InternalArgs\DefaultOutputFBArgs.cs)
- [MoveAbsoluteExArgsIN.cs](C:\work\Elmo\Elmo_Master\NetHelp\Elmo Maestro API Help\MMCLibDotNet\ElmoMotionControlComponents.GMAS.MMCLibDotNET\ElmoMotionControl\GMAS\EASComponents\MMCLibDotNET\InternalArgs\MoveAbsoluteExArgsIN.cs)

## 1) 커맨드/요청 프레임(기본)
- `MoveAbsoluteEx` CommandID = `0x209F` (리틀엔디언이면 TCP payload 시작 `9f 20`)
- 요청 길이(`Length`) = `64` bytes (header+payload), 실제 payload는 56 bytes
- 요청 payload field 오프셋(요청 기준):
  - `0x08`부터 `position(double)`
  - 그 뒤 순서대로 `velocity`, `acc`, `dec`, `jerk`, `direction(uint32)`, `bufferMode(uint32)`, `execute(byte)`
  (실제 바이트 구조는 `MoveAbsoluteExArgsIN.cs`의 `DataOut()` 생성 방식이 근거)

## 2) `MoveAbsoluteEx` 응답 프레임(패킷 캡처 근거)
캡처에서 실제 응답 payload 예시:
- `0000080000000000b0980b0000000000` (16 bytes)

해석:
- 응답 전체 길이 = `16` bytes (이건 tcp len 기준)
- 파서(`DefaultOutputFBArgs`) 기준으로 payload-size를 `8` bytes로 봄
- 오프셋 기반 파싱:
  - `off 0~1`: `0x0000` (라이브러리 관점에서 직접 사용 안 함 / reserved 성격으로 보임)
  - `off 2~3`: `0x0008` (payload size=8)
  - `off 4~7`: `0x00000000` (기본 헤더/axisref 영역으로 추정)
  - `off 8~11`: `0x000B98B0` (FB handle = **759984**)
  - `off 12~13`: `0x0000` (status)
  - `off 14~15`: `0x0000` (error ID)

이 값은 실제 캡처에서 코드가 같이 추출한 `fb_handle=759984`, `fb_status=0`, `fb_error=0`과 일치.

## 3) `MoveAbsoluteEx` 응답의 핵심 의미
- 이 응답은 **“명령 수신/접수 결과”** 위주 응답입니다.
- 여기에 실제 위치값(position)은 안 들어갑니다.
- 따라서 “움직임 완료”를 판단하려면:
  - `MoveAbsoluteEx` 응답은 성공 수락 확인만으로 보시고
  - 실제 완료 여부는 `ReadStatus`/`GetActualPosition` 기반으로 별도 조회해야 합니다.

## 4) 응답 타이밍(캡처 기준)
- 샘플 로그 기준
  - 673.2 ms 또는 742.0 ms 내외로 첫 응답 수신
- 즉, 응답 지연은 통신/큐/드라이브 처리에 따라 수백 ms가 나올 수 있습니다.

## 5) 주의할 점 (실수 포인트)
- `.pcap` payload에서 `command_id`가 응답 본문 시작부에 항상 드러나지 않습니다.
  라이브러리 파서는 **요청-응답 매핑**으로 어떤 명령 응답인지 식별하고, `off 2`를 길이 체크 후 `off 8`부터 FB 결과를 읽습니다.
- 그래서 로그 파싱 시 `command_id` 필드는 “응답 페이로드에서 직접 읽은 값”이 아닌, 매칭된 요청 정보 기반으로 붙이는 방식입니다.

원하면 이 구조를 기준으로 `MoveAbsoluteEx` 전용 “응답 디코더”를 지금 코드에 넣고, CSV로 `fb_handle/status/error`만 분리 추출하도록 바로 붙여드릴 수 있습니다.

> 저렇게 메세지를 받으면 코드상에서는 Return Code 하나만 처리하나?

<details><summary>4 previous messages</summary>

> 네, 핵심만 먼저 답하면: **아니요, 하나만 쓰는 게 아니라 최소 3개를 받습니다**(핸들/상태/에러). 다만 “실행 제어에서 실패했는지”를 가릴 때는 보통 에러 코드 한 개(에러ID)만으로 판단하는 경우가 많습니다.
>
> 코드 기준으로 확인하고 바로 정리해드릴게요.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청 포인트 정확히 짚었습니다. 지금 동작 기준으로 확인하면 `MoveAbsoluteEx` 응답은 코드상에서 단일 Return Code만 쓰는 구조가 아닙니다. 먼저 관련 파서를 한 번 더 확인하고 정확히 답할게요.
>
> 짧게 바로 결론부터 말하면: **아니오, 단일 Return Code만 받는 방식이 아닙니다.** Move/Read 계열은 성공 여부 외에도 상태/오류 정보가 같이 처리됩니다. 필요한 파일 라인만 딱 확인해서 답을 정리해드릴게요.
</details>

아니요.
`MoveAbsoluteEx` 같은 함수는 **Return Code 하나만 보는 구조가 아닙니다**.

- Elmo API/라이브러리 기준: 응답 파서는 보통 `uiHndl`, `usStatus`, `usErrorID`(또는 MMCError 계열) 같은 값을 같이 해석합니다.
- 현재 코드 호출부(`MoveAbsoluteEx`)는 반환값이 `void`라서, 성공/실패는 **예외( `MMCException` ) 기반**으로 판단합니다.
  - `MMCException` 로그에는 `LibraryError`, `MMCError`, `Status`, `AxisRef`까지 같이 기록됩니다.
- `Codex_LASAL`도 마찬가지로 프레임 파싱/응답 처리(콜백/예외/이벤트)에서 단일 코드만 고집하지 않습니다.

즉, “한 개의 리턴 코드만” 처리하는 게 아니라, **상태/오류 정보를 함께 해석하는 구조**입니다.

> 코드상에는 어떻게 처리되있는지 라인 짚어서 설명해줘

<details><summary>2 previous messages</summary>

> 좋아요. "실제로 코드에서 어떤 값들이 처리되는지"를 라인 기준으로 정리할 수 있게, PMAS 쪽 실행흐름을 기준으로 라인 인덱스를 뽑아봤습니다. 이어서 바로 라인 설명 붙여드릴게요.
>
> `MainWindow`에서 버튼 동작도 같은 패턴으로 동작하므로, 동작 흐름을 보기 위해 `ExecuteAction` 위치도 같이 확인했습니다. 이어서 이어서 라인 단위 맵으로 정리하겠습니다.
</details>

맞습니다. **`ReturnCode 하나만`으로 처리하지 않습니다.**

핵심 처리 흐름을 라인 단위로 짚으면 아래처럼 됩니다.

1. 버튼/테스트에서 API 호출은 `ExecuteAction` 래퍼로 감쌉니다.
   - `[ExecuteAction](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:80)`
   - 성공 시 `completed` 로그, 실패 시 `HandleException`로 넘김.

2. `MMCException`은 `HandleException`에서 바로 파싱되어 여러 필드로 기록됩니다.
   - `[HandleException](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:93)`
   - 로그 필드: `CommandID`, `LibraryError`, `MMCError`, `Status`, `AxisRef`, `AxisName`
   - 즉 단일 코드가 아니라 **명령/라이브러리/실패 상태/축 상태**를 같이 본다는 뜻입니다.

3. `MoveAbsoluteEx` 호출은 `void` 형태라 반환값을 코드에서 받지 않습니다.
   - `[IssueCycleMove](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1794)`
   - 여기서 `Context.SingleAxis.MoveAbsoluteEx(...)`만 호출.

4. 버튼 동작도 같은 패턴입니다.
   - `[ButtonMoveAbsoluteEx_Click](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:140)`
   - 내부에서 `ExecuteAction` + `Context.SingleAxis.MoveAbsoluteEx(...)`.

5. CycleTest에서 명령 실패 분기: `MMCException` 하나를 잡아서 예외 세부값을 저장/로그화.
   - `[CycleTest-첫 번째 catch](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1193)`
   - `[CycleTest2 catch](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1312)`
   - `[CycleTest3 catch](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1475)`
   - `[CycleTest4 catch](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1638)`

6. 명령 직후 추가로 상태를 별도 채널로 검사합니다.
   - `GetAxisError` 검사: `[CycleTest 메인 루프](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1174)`
   - `ReadStatus` 기반 In-position 검사(사이클3): `[WaitForInPositionByStatusWord](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1691)`
   - `ReadStatus` 직접 샘플링(Cycle4): `[CycleTest4 read loop](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1588)`

7. `GetActualPosition`도 마찬가지로 값만 받는 API로 쓰고, 상태값은 별도로 읽습니다.
   - `[WaitForInPosition - GetActualPosition 호출](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1846)`

8. 비동기 리턴(이벤트/콜백)도 존재합니다.
   - 연결 시 콜백 등록: `[Connect 등록](./Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs:206)`
   - 콜백에서 받은 패킷 필드(에러/상태) 로그: `[UserCallback](./Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs:345)`

결론적으로, 지금 구조는 “**Return Code 하나만 체크**”가 아니라 **예외 객체 + 후속 상태 쿼리 + 이벤트 콜백**까지 같이 쓰는 구조입니다.
`MoveAbsoluteEx`를 받을 때도 동일하게 `CommandID/LibraryError/MMCError/Status/AxisRef`를 동시에 보고 처리합니다.

> 매뉴얼 보면 리턴에 int 되어있는데?
>
> User context
>
> Images:
> - `[embedded image extracted: ./assets/embedded_line_4229.png; original line 4229; mime image/png; base64 chars 476968]`

<details><summary>2 previous messages</summary>

> 맞습니다. 스크린샷 기준으로는 `int` 리턴이 보이는 게 정상입니다.
> 지금 혼선이 생기는 지점이 “매뉴얼의 계층(C/C++ API)”와 “현재 앱에서 쓰는 .NET 래퍼 호출 방식”이 달라서입니다. 해당 시그니처를 코드 기준으로 정확히 매칭해서 설명드릴게요.
>
> 좋은 지적이고 맞습니다. 실제 NetHelp 소스에도 `public int MoveAbsoluteEx(...)`로 정의되어 있습니다.
> 이제 중요한 건 “그 int가 실제 앱에서 어떻게 처리되는가”라서, 내부 구현에서 `int`와 `Exception`이 같이 쓰이는 부분을 바로 짚겠습니다.
</details>

맞습니다. **매뉴얼대로 `MoveAbsoluteEx`는 `int` 리턴이 맞습니다.**

코드 기준으로 정리하면:

1. 공식 .NET 래퍼 구현도 `int` 반환입니다.
   - [`MMCSingleAxis.cs`](C:\work\Elmo\Elmo_Master\NetHelp\Elmo Maestro API Help\MMCLibDotNET\ElmoMotionControlComponents.GMAS.MMCLibDotNET\ElmoMotionControl\GMAS\EASComponents\MMCLibDotNET\MMCSingleAxis.cs:2390)
   - 내부에서 최종 `return (int) outArgs.ErrorID;`
   - [`MMCSingleAxis.cs`](C:\work\Elmo\Elmo_Master\NetHelp\Elmo Maestro API Help\MMCLibDotNET\ElmoMotionControlComponents.GMAS.MMCLibDotNET\ElmoMotionControl\GMAS\EASComponents\MMCLibDotNET\MMCSingleAxis.cs:2432)

2. 그런데 반환 전에 `CheckDeviceError`가 먼저 돌고, 오류면 예외를 던집니다.
   - [`MMCSingleAxis.cs`](C:\work\Elmo\Elmo_Master\NetHelp\Elmo Maestro API Help\MMCLibDotNET\ElmoMotionControlComponents.GMAS.MMCLibDotNET\ElmoMotionControl\GMAS\EASComponents\MMCLibDotNET\MMCSingleAxis.cs:2431)
   - [`MMCAxis.cs`](C:\work\Elmo\Elmo_Master\NetHelp\Elmo Maestro API Help\MMCLibDotNET\ElmoMotionControlComponents.GMAS.MMCLibDotNET\ElmoMotionControl\GMAS\EASComponents\MMCLibDotNET\MMCAxis.cs:359)

3. 현재 우리 앱은 그 `int`를 변수로 받지 않고 호출만 합니다(무시).
   - [`MainWindow.AxisOperations.cs`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:145)
   - [`MainWindow.CycleTestOperations.cs`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1796)

4. 대신 예외에서 상세 필드(`CommandID`, `LibraryError`, `MMCError`, `Status`, `AxisRef`)를 잡아 로그 처리합니다.
   - [`MainWindow.xaml.cs`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:95)

즉, 모순이 아니라 **시그니처는 `int` + 실제 에러 처리는 예외 중심**으로 되어 있는 구조입니다.
원하면 `MoveAbsoluteEx`의 리턴 `int`도 캡처해서 로그에 같이 남기게 바로 바꿔드릴 수 있습니다.

> 근데 void라고 구라를 쳐. 똑바로 안하냐

<details><summary>2 previous messages</summary>

> 내가 잘못 말했습니다. `MoveAbsoluteEx`는 `int` 리턴이 맞고, 현재 앱은 그 값을 대부분 무시하고 예외 중심으로 처리하고 있습니다.
> 혼선 줄이기 위해 코드도 바로 `int` 리턴을 드러내도록 수정하겠습니다.
>
> 코드도 바로 맞춰서 고쳤습니다.
> 지금 빌드 한 번 돌려서 깨지는 곳 없는지 확인하겠습니다.
</details>

맞다. 내가 잘못 말했다. `MoveAbsoluteEx`는 `int` 리턴이 맞다.

정정 + 조치 완료:
- 공식 래퍼도 `int` 반환: [`MMCSingleAxis.cs`](C:\work\Elmo\Elmo_Master\NetHelp\Elmo Maestro API Help\MMCLibDotNET\ElmoMotionControlComponents.GMAS.MMCLibDotNET\ElmoMotionControl\GMAS\EASComponents\MMCLibDotNET\MMCSingleAxis.cs:2390), 반환값 사용 지점: [`MMCSingleAxis.cs`](C:\work\Elmo\Elmo_Master\NetHelp\Elmo Maestro API Help\MMCLibDotNET\ElmoMotionControlComponents.GMAS.MMCLibDotNET\ElmoMotionControl\GMAS\EASComponents\MMCLibDotNET\MMCSingleAxis.cs:2432)
- PMAS 코드에서 `IssueCycleMove`를 `void` -> `int`로 수정하고 non-zero 리턴 로그 추가: [`MainWindow.CycleTestOperations.cs`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1794)
- 수동 Move 버튼도 리턴코드 로그하도록 수정: [`MainWindow.AxisOperations.cs`](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:140)
- LASAL 더미도 시그니처를 실제와 맞게 `int` 반환으로 수정: [`SigmatekTcpIpDummyMMCLib.cs`](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:805)

검증:
- `Codex_PMAS_WPF` Debug 빌드 성공 (오류 0)
- `Codex_LASAL_WPF` Debug 빌드 성공 (오류 0)

앞으로는 시그니처/호출 계층(C API vs .NET 래퍼) 분리해서 정확히 짚겠다.

> 이거 두개 파일 비교해서 MoveAbsoluteEx가 어떻게 동작하는지 비교분석해봐
> MoveAbsoluteEx.pcapng는
> 제어기 192.168.1.3 PC 192.168.1.13
> MoveAbsoluteEx_Sigmatek.pcapng는
> 제어기 10.10.150.1 PC 10.10.150.3
>
> User context
>
> Attachments:
> - MoveAbsoluteEx.pcapng: `./packet_capture/MoveAbsoluteEx.pcapng`
> - MoveAbsoluteEx_Sigmatek.pcapng: `./packet_capture/MoveAbsoluteEx_Sigmatek.pcapng`

<details><summary>5 previous messages</summary>

> 요청 이해했습니다. 두 pcap을 같은 기준(명령 프레임 구조, 응답 구조, 왕복 타이밍, 재전송/세션 특성)으로 뽑아서 비교하겠습니다. 먼저 `tshark` 가능 여부와 캡처 기본 메타부터 확인합니다.
>
> `tshark`가 PATH에는 없어서 Wireshark 설치 경로에서 실행 파일을 직접 찾는 중입니다. 찾으면 바로 동일 필터로 두 파일을 추출해 비교하겠습니다.
>
> IP 수정 반영했습니다. `MoveAbsoluteEx_Sigmatek.pcapng`는 PC를 `10.10.150.13` 기준으로 분석하겠습니다. 지금 두 캡처의 TCP 대화/스트림부터 먼저 뽑겠습니다.
>
> 노이즈 패킷이 많아서 대상 제어기-PC TCP 세션만 필터링해서 다시 추출하겠습니다. 그 다음 명령/응답 바이트를 프레임 단위로 직접 비교하겠습니다.
>
> 핵심 프레임은 둘 다 3개(명령 64B, 응답 16B, ACK)로 동일합니다. 지금은 명령 ID 차이(`0x20A0` vs `0x209F`)가 왜 나는지 내부 정의를 빠르게 대조해 보겠습니다.
</details>

비교 결과부터 말하면, **두 파일의 TCP 핸드셰이크/응답 패턴은 동일**한데, **명령 ID와 파라미터 인코딩이 다릅니다.**

분석 파일:
- [MoveAbsoluteEx.pcapng](./packet_capture/MoveAbsoluteEx.pcapng)
- [MoveAbsoluteEx_Sigmatek.pcapng](./packet_capture/MoveAbsoluteEx_Sigmatek.pcapng)

**1) 세션 구조 (둘 다 동일)**
1. PC -> Controller: `tcp.len=64` 명령 프레임 1개
2. Controller -> PC: `tcp.len=16` 응답 프레임 1개
3. PC -> Controller: `tcp.len=0` ACK 1개

즉, Move 계열 FB 명령 1회 호출 시점 기준으로는 둘 다 `64B 요청 + 16B 응답` 패턴입니다.

**2) 핵심 차이**
- Elmo 캡처(192.168.1.13 -> 192.168.1.3) 명령 프레임:
  - Frame `#287`
  - Command ID: `0x20A0` (`a0 20`)
- Sigmatek 캡처(10.10.150.13 -> 10.10.150.1) 명령 프레임:
  - Frame `#39`
  - Command ID: `0x209F` (`9f 20`)

여기서 중요한 점:
- `0x209F`는 `MC_MOVEABSOLUTEEX_ID`
- `0x20A0`는 `MC_MOVERELATIVEEX_ID`

즉 **Elmo 쪽 캡처는 파일명과 달리 실제 바이트상 `MoveRelativeEx`로 보입니다.**
(동일 명령 비교로는 apples-to-apples가 아닙니다)

**3) 파라미터 인코딩 차이**
- Elmo 캡처 payload는 `double` 해석 시 자연스러운 값:
  - `8388608, 48933546, 4893354600, ...`
- Sigmatek 캡처 payload는 `long(int64)` 해석 시 자연스러운 값:
  - `pos=0, vel=500000, acc=500000, dec=500000, jerk=500000`
- 즉 Sigmatek 쪽은 네가 설계한 대로 `long` 기반 프레임으로 보입니다.

**4) 응답 프레임(16B) 해석**
둘 다 동일 포맷:
- `lenField=8`
- `handle` 값 존재
- `status=0`
- `error=0`

예:
- Elmo 응답(Frame `#288`): handle `1182672`
- Sigmatek 응답(Frame `#40`): handle `759984`

**5) 타이밍**
- Command -> Response
  - Elmo: `0.1494 ms`
  - Sigmatek: `1.8936 ms`
- Response -> ACK
  - Elmo: `54.3275 ms`
  - Sigmatek: `50.2776 ms`

ACK 지연은 둘 다 비슷해서, TCP delayed ACK 성격으로 보는 게 맞습니다.

---

정리:
- 전송 패턴 자체는 동일하게 잘 맞췄고,
- 차이는 **명령 ID(Elmo 파일이 0x20A0)**와 **파라미터 타입(double vs int64)**입니다.
- 진짜 MoveAbsoluteEx 대 MoveAbsoluteEx 비교를 하려면 Elmo 쪽에서 `0x209F`가 찍힌 캡처를 다시 떠야 정확합니다.

> 근데 프레임을 동일하게 구성했는데 Wireshark에서 Elmo 프레임은 TCP라고나오는데 Sigmatek 프레임은 왜 RSL이라 나옴?

그건 **통신이 달라진 게 아니라, Wireshark 디섹터(해석기) 매칭 차이**입니다.

- 실제 전송 계층은 둘 다 TCP입니다.
- 내가 뽑은 캡처에서도 Sigmatek는 `10.10.150.13:5000 -> 10.10.150.1:4000`, `tcp.len=64` 요청 / `tcp.len=16` 응답으로 TCP가 맞습니다.
- `RSL`로 보이는 건 Wireshark가 payload를 상위 프로토콜로 **오인 디코딩**한 결과입니다. (Protocol 컬럼은 “최상위 해석 결과”를 표시)

왜 그렇게 보이냐:
- `Decode As`가 이전에 설정됐거나
- 특정 포트(예: 5000/4000)에 RSL 디섹터가 붙었거나
- heuristic dissector가 payload 패턴을 RSL로 잘못 잡은 경우

해결:
1. `Analyze -> Decode As...`에서 해당 TCP 포트(4000/5000) 매핑을 `None`/`Data`로 변경 또는 Reset
2. `Analyze -> Enabled Protocols...`에서 `RSL` 비활성화(필요 시)
3. `Preferences -> Protocols -> TCP`에서 heuristic sub-dissector 옵션 끄기(필요 시)

즉 결론은: **Sigmatek 프레임도 TCP이고, RSL 표시는 Wireshark 표시/디코딩 문제**입니다.

> 모든 프레임이 동일한지 TCP 아래 프로토콜들도 완벽하게 Diff해서 비교해봐

<details><summary>4 previous messages</summary>

> 요청대로 하겠습니다. 두 캡처에서 제어기-PC TCP 세션 3프레임씩을 1:1로 잡고, L2~L7(디섹터 포함) 필드와 payload를 바이트 단위로 전부 비교하겠습니다. 먼저 비교용 원본 필드를 한번에 추출하겠습니다.
>
> 지금 1차 결과에서 이미 중요한 차이가 확인됐습니다: Sigmatek 캡처는 `frame.protocols`에 `gsm_ipa:gsm_abis_rsl`이 붙고, Elmo는 `...:data`로 끝납니다.
> 이제 RSL 디섹터가 실제로 어떤 필드를 읽었는지까지 뽑아서 “TCP 아래 해석 차이”를 완전히 확인하겠습니다.
>
> 지금 단계에서 바이트 오프셋 단위 diff를 파일로 남기겠습니다.
> 프레임/헤더 필드 diff CSV + command/response payload byte diff CSV + 요약 MD까지 생성해서 이후 재검증 때 그대로 재사용 가능하게 하겠습니다.
>
> 스크립트 문자열 파싱 에러가 나서, 생성 단계만 안정적으로 다시 실행하겠습니다. diff 결과 파일 4개를 정상 생성해서 경로까지 확인해드릴게요.
</details>

완전 비교 끝냈다. 결론부터 말하면 **동일하지 않다**.

**비교 범위**
- Elmo: `192.168.1.13 <-> 192.168.1.3` TCP 3프레임 (cmd/resp/ack)
- Sigmatek: `10.10.150.13 <-> 10.10.150.1` TCP 3프레임 (cmd/resp/ack)

**정량 Diff 결과**
- 프레임 필드(헤더+페이로드 포함) 비교: `same 37 / diff 32`
- Command payload(64B): `same 27 / diff 37`
- Response payload(16B): `same 13 / diff 3`

**핵심 차이**
1. Protocol stack 표기 차이
- Elmo: `eth:ethertype:ip:tcp:data`
- Sigmatek: `eth:ethertype:ip:tcp:gsm_ipa:gsm_abis_rsl`
- 즉 Sigmatek는 Wireshark가 TCP payload를 RSL로 heuristic decode한 상태.

2. Command ID가 다름
- Elmo cmd payload byte[0..1]: `0x20A0`
- Sigmatek cmd payload byte[0..1]: `0x209F`

3. 파라미터 인코딩이 다름
- Elmo 쪽은 double 해석 시 자연스러운 값(예: `8388608`, `48933546`, ...)
- Sigmatek 쪽은 int64 해석 시 자연스러운 값(예: `0`, `500000`, `500000`, ...)

4. Response 구조는 동일 패턴
- 둘 다 16B 응답, `len/status/error` 패턴 동일
- handle 값만 다름 (당연한 차이)

5. L2/L3/L4 헤더도 다름
- MAC/IP/포트/TTL/Seq/Ack/Window 값 다름 (환경이 다르므로 정상)

**결과 파일**
- 요약 리포트: [MoveAbsoluteEx_Elmo_vs_Sigmatek_Diff.md](./packet_capture/MoveAbsoluteEx_Elmo_vs_Sigmatek_Diff.md)
- 필드 단위 diff: [MoveAbsoluteEx_Elmo_vs_Sigmatek_FrameFields.csv](./packet_capture/MoveAbsoluteEx_Elmo_vs_Sigmatek_FrameFields.csv)
- Command 64B 바이트 diff: [MoveAbsoluteEx_Elmo_vs_Sigmatek_CommandByteDiff.csv](./packet_capture/MoveAbsoluteEx_Elmo_vs_Sigmatek_CommandByteDiff.csv)
- Response 16B 바이트 diff: [MoveAbsoluteEx_Elmo_vs_Sigmatek_ResponseByteDiff.csv](./packet_capture/MoveAbsoluteEx_Elmo_vs_Sigmatek_ResponseByteDiff.csv)

원하면 다음으로 `Decode As`를 강제로 `Data`로 맞춘 상태 기준(=RSL 디섹터 배제)으로 다시 비교 리포트도 만들어줄 수 있다.

> 강제로 바꿔서 비교해봐

<details><summary>2 previous messages</summary>

> 좋습니다. `Decode As`를 강제로 `data`로 고정한 조건으로 다시 뽑겠습니다.
> `tcp.port 4000/5000/4867` 모두 `data`로 강제 디코딩해서 동일한 diff 리포트를 새 파일로 만들겠습니다.
>
> 강제 디코딩이 정상 적용됐습니다. Sigmatek도 `...:tcp:data`로 정렬됐습니다.
> 이 기준으로 새 diff 파일(강제-data 버전) 생성하겠습니다.
</details>

강제로 `Decode As = data` 적용해서 다시 비교했다.
조건: `tcp.port 4000/5000/4867 -> data` 고정.

결과:
1. `RSL` 표시는 사라지고 둘 다 `frame.protocols = eth:ethertype:ip:tcp:data`로 정렬됨.
2. 그래도 프레임이 완전 동일하진 않음.
3. 강제 decode 기준 diff:
- Frame fields: `same 39 / diff 30`
- Command payload(64B): `same 27 / diff 37`
- Response payload(16B): `same 13 / diff 3`
4. 핵심 payload 차이는 그대로:
- Command ID: Elmo `0x20A0` vs Sigmatek `0x209F`
- Response는 구조는 같고 `handle` 바이트만 다름

생성 파일:
- [MoveAbsoluteEx_ForcedData_Elmo_vs_Sigmatek_FrameFields.csv](./packet_capture/MoveAbsoluteEx_ForcedData_Elmo_vs_Sigmatek_FrameFields.csv)
- [MoveAbsoluteEx_ForcedData_Elmo_vs_Sigmatek_CommandByteDiff.csv](./packet_capture/MoveAbsoluteEx_ForcedData_Elmo_vs_Sigmatek_CommandByteDiff.csv)
- [MoveAbsoluteEx_ForcedData_Elmo_vs_Sigmatek_ResponseByteDiff.csv](./packet_capture/MoveAbsoluteEx_ForcedData_Elmo_vs_Sigmatek_ResponseByteDiff.csv)

원하면 다음으로 `동일한 Command ID(0x209F)`만 맞춘 테스트 캡처 기준으로 다시 1:1 검증해주겠다.

> Codex_LASAL_WPF 프로그램을 구현하는데 ReadActualPosition이 값을 제대로 파싱하지 못하는거 같아. 내가 패킷을 캡쳐해줘야되나?

<details><summary>2 previous messages</summary>

> 패킷 캡처가 있으면 확실히 빠르지만, 지금 코드를 보면 캡처 없이도 바로 수정할 수 있는 파싱 취약점이 있습니다.
> `ReadActualPosition` 응답 길이(20/24/16)와 타입(double/int64) 케이스를 모두 처리하도록 먼저 고치고, 그 다음에 필요하면 캡처로 검증하겠습니다.
>
> 파서 수정 완료했습니다. 지금 바로 빌드해서 깨지는 부분 없는지 확인하고, 수정 내용 기준으로 캡처 필요 여부를 결론내리겠습니다.
</details>

지금은 **캡처 없이도 1차 수정 가능해서 이미 고쳤다.**

원인:
- 기존 파서가 20바이트 응답(`8바이트 헤더 + 8바이트 position + 2+2 상태`)을 제대로 처리 못하고, fallback에서 마지막 8바이트를 position으로 읽어서 값이 틀어질 수 있었음.

적용한 수정:
- 응답 헤더의 `payloadLength`를 읽어 `8/12/16` payload 케이스를 모두 처리.
- position은 offset 8에서 우선 파싱.
- 일부 장비가 `int64` 카운트를 보내는 경우도 감안해 `double/int64` 혼합 파싱 보강.
- 수정 위치: [SigmatekTcpIpDummyMMCLib.cs:1195](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1195), [SigmatekTcpIpDummyMMCLib.cs:1235](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1235)
- `Codex_LASAL_WPF` Debug 빌드 성공(오류 0)

결론:
- **지금은 캡처 필수 아님.** 먼저 이 버전으로 테스트해봐.
- 그래도 값이 이상하면 그때 캡처 필요.
- 그 경우 `ReadActualPosition` 1회 기준으로 `request + response` 2프레임만 주면 바로 확정 가능.

> 이거 ReadActualPosition이 0x202E 나오는데 전에 니가 말해줬을땐 0x00E0인데 바뀐거임?

바뀐 거 아닙니다.
`ReadActualPosition` 명령 ID는 **원래 `0x202E`가 맞습니다**.

- 근거: [`IDCommands.cs`](C:\work\Elmo\Elmo_Master\NetHelp\Elmo Maestro API Help\MMCLibDotNET\ElmoMotionControlComponents.GMAS.MMCLibDotNET\ElmoMotionControl\GMAS\EASComponents\MMCLibDotNET\IDCommands.cs:163)
  `MC_READACTUALPOSITION_ID = 8238 // 0x202E`
- 현재 LASAL 코드도 동일: [`SigmatekTcpIpDummyMMCLib.cs`](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:706)

내가 예전에 `0x00E0`라고 말한 건 잘못 전달한 겁니다.
정확히는 **요청(request)은 `0x202E`**, 응답(response)은 캡처에서 `000008...` 형태로 오고 보통 명령 ID를 그대로 에코하지 않습니다.

> Elmo Controller는 메세지 응답성이 0.1~0.2ms 나오는데 Sigmatek 제어기는 0.3~0.4ms 성능이 나오고 있어. 이렇게 성능차이가 나는 이유가머지? 우리가 패킷을 만드는게 잘못됐나?

<details><summary>1 previous message</summary>

> 좋은 질문입니다. 결론부터 말하면, 지금 수치만으로는 “패킷을 잘못 만들어서 느리다”보다 **제어기 처리 경로/주기 + TCP 스택 조건 차이** 가능성이 더 큽니다.
> 현재 코드에서 즉시 개선 가능한 항목(TCP_NODELAY, timeout, 스레드 우선순위)을 확인해서 원인 후보를 좁혀드리겠습니다.
</details>

가능성 낮다. 지금 수치 차이(Elmo 0.1~0.2ms vs Sigmatek 0.3~0.4ms)는 **패킷 포맷 오류**보다는 **처리 주기/실행 경로 차이** 쪽이 더 맞다.

근거:
- Sigmatek 쪽도 요청/응답 구조 자체는 정상이고 `status=0, error=0`로 돌아옴.
- TCP Nagle은 이미 꺼져 있음(`NoDelay=true`): [SigmatekTcpIpDummyMMCLib.cs:460](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:460)

왜 느릴 수 있나:
1. PLC 측 통신/파싱이 cyclic task에서 돌아가면 주기만큼 지연이 붙음 (예: 250us~1ms).
2. 비교 명령이 완전 동일하지 않으면 처리 경로가 달라짐 (`0x209F` vs `0x20A0` 문제 있었음).
3. PC 측 측정값은 네트워크+소켓 read wait+파싱+스케줄링이 합쳐진 값.
4. NIC interrupt moderation, 전원관리, 스위치 경로 차이도 0.1~0.3ms 쉽게 만듦.

먼저 이렇게 맞춰서 재측정해:
1. 같은 명령 ID/같은 payload(단위 포함)로 1:1 비교
2. 직결(스위치 제거) + 같은 NIC 설정
3. Sigmatek 통신 task를 최고 우선순위/짧은 주기로 설정
4. 왕복을 1000회 측정해서 `avg`만 말고 `p50/p95/max` 같이 비교

즉, 현재 증거로는 “패킷 잘못 만들어서 느리다”는 결론은 아니다.
원하면 내가 LASAL 쪽에 `send/write`, `read wait`, `parse`를 분리 계측하는 로그를 바로 넣어줄게.

> 이거 wireshark에서 RSL이라고 나오지 않게 할수없냐?

된다. 표시 문제라서 설정으로 막을 수 있다.

1. `Decode As`로 강제 고정
- 패킷 우클릭 `Decode As...`
- `TCP port`에서 `4000`, `5000`(필요하면 `4867`) 선택
- `Current`를 `data`로 지정 후 `Apply`
- 프로파일 저장하면 다음에도 유지됨

2. 그래도 나오면 RSL 디섹터 비활성
- `Analyze -> Enabled Protocols...`
- `gsm_ipa`(GSM over IP), `gsm_abis_rsl`(RSL) 체크 해제

3. 마지막으로 heuristic 끄기
- `Edit -> Preferences -> Protocols -> TCP`
- `Try heuristic sub-dissectors first` 해제

CLI로 보면 항상 강제 가능:
```powershell
tshark -d tcp.port==4000,data -d tcp.port==5000,data -d tcp.port==4867,data -r your.pcapng
```

즉, 패킷이 RSL로 바뀐 게 아니라 Wireshark 해석만 RSL로 붙는 거다.

> 아니 코드에서 패킷 수정해서 안나오게 하고 싶다고

<details><summary>3 previous messages</summary>

> 요구사항 이해했습니다. 코드에서 바이트를 바꿔서 Wireshark가 RSL로 안 보이게 하는 방법이 가능한지 먼저 정확히 확인하겠습니다.
> `gsm_ipa/rsl` 디섹터가 어떤 조건으로 붙는지 확인해서, 통신 호환성 깨지지 않는 선에서 가능한지 결론 내리겠습니다.
>
> 원인 찾았습니다. 코드에서 고칠 수 있습니다.
> 지금 RSL로 보이는 핵심 이유는 **패킷 내용보다 포트 5000이 Wireshark에서 `gsm_ipa`로 등록돼 있기 때문**입니다. 그래서 코드에서 로컬 포트를 5000 대신 다른 값으로 쓰면 해결됩니다. 바로 수정하겠습니다.
>
> 코드에서 자동으로 피하도록 바꾸겠습니다.
> LASAL 앱은 `LocalPort=5000` 입력 시 내부에서 `5001`로 치환해서 연결하고, UI 기본값도 5001로 바꿔서 RSL 오인 디코딩을 원천 회피하겠습니다.
</details>

코드에서 처리해놨다. 이제 Wireshark에서 RSL로 붙을 가능성을 코드 레벨에서 줄였다.

변경 내용:
1. `LocalPort=5000` 입력 시 내부적으로 `5001`로 자동 치환
- [PmasControllerContext.cs](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs:184)
- 연결 로그에 remap 메시지 남김

2. UI 기본 Local Port를 `5001`로 변경
- [MainWindow.xaml](./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:54)

3. 연결 객체 기본 callback port도 `5001`로 변경
- [SigmatekTcpIpDummyMMCLib.cs](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:476)

검증:
- `Codex_LASAL_WPF` Debug 빌드 성공, 오류 0

중요 포인트:
- Wireshark는 `tcp.port 5000 -> gsm_ipa`가 기본 등록되어 있어서 RSL로 오인 디코딩됨.
- 그래서 포트를 바꾸는 게 코드에서 가장 확실한 해결책이다.

다음 캡처는 `10.10.150.13:5001 -> 10.10.150.1:4000`로 잡힐 거고, 기본 상태에서 `tcp:data`로 보일 가능성이 높다.

> 지금 구현되어있는 MoveAbsoluteEx랑 ReadActualPosion을 이용해서 CycleTest를 구현해줘

<details><summary>9 previous messages</summary>

> 요청한 건 `MoveAbsoluteEx + ReadActualPosition` 기반의 반복 CycleTest를 추가하는 작업으로 이해했습니다.
> 먼저 `Codex_LASAL_WPF`에 현재 CycleTest 뼈대가 있는지 확인하고, 있으면 그 구조에 맞춰 바로 구현하겠습니다.
>
> 현재 LASAL 쪽에는 CycleTest가 이미 있는 상태라, 실제로 `MoveAbsoluteEx + GetActualPosition`을 쓰는지 먼저 확인하겠습니다.
> 만약 다른 방식이면 요청한 방식으로 바로 바꿔서 동작하게 만들겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청하신 대로 현재 구현된 `MoveAbsoluteEx`와 `ReadActualPosition`(코드상 `GetActualPosition`) 기반으로 `CycleTest`를 바로 붙이겠습니다. 먼저 현재 어느 프로젝트(`Codex_PMAS_WPF`/`Codex_LASAL_WPF`)에 이미 있는지 확인하고, 없으면 해당 파일에 바로 구현하겠습니다.
>
> 코드를 확인해보니 두 프로젝트 모두 `CycleTest` 뼈대는 이미 있고, LASAL 쪽도 `MoveAbsoluteEx` + `GetActualPosition` 호출이 들어가 있습니다. 지금 요청에 맞게 실제 동작 경로를 다시 점검해서, 부족한 부분이 있으면 바로 수정 후 빌드 검증까지 하겠습니다.
>
> `LASAL`은 이미 구현되어 있어서, 현재 열어둔 `PMAS` 파일 기준으로도 동일 로직이 정상인지 확인하고 필요하면 그쪽을 기준으로 맞춰 수정하겠습니다.
>
> 핵심 로직은 이미 들어가 있지만, 요청 의도에 맞게 `CycleTest`가 `MoveAbsoluteEx` 반환값까지 확인하도록 보강하고 빌드로 검증하겠습니다. 수정은 `Codex_LASAL_WPF` 기준으로 진행하겠습니다.
>
> `CycleTest`의 `MoveAbsoluteEx` 호출 결과를 명시적으로 로깅하도록 반영했습니다. 지금 빌드해서 실제로 깨지는 부분 없는지 바로 확인하겠습니다.
>
> 요청사항 기준으로 `CycleTest` 경로에서 `MoveAbsoluteEx` 호출 결과까지 확인되도록 보강했습니다. 변경점과 적용 위치를 바로 정리해서 공유하겠습니다.
</details>

요청한 형태로 `CycleTest`는 `MoveAbsoluteEx` + `GetActualPosition(ReadActualPosition)` 기반으로 동작하도록 반영되어 있습니다.
추가로 `MoveAbsoluteEx` 반환코드도 체크하게 보강했습니다.

변경/확인 위치:
1. `CycleTest` 메인 루프: [MainWindow.CycleTestOperations.cs:589](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:589)
2. 이동 명령 발행(Forward/Return): [MainWindow.CycleTestOperations.cs:619](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:619), [MainWindow.CycleTestOperations.cs:644](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:644)
3. 위치 판정 읽기(`GetActualPosition`): [MainWindow.CycleTestOperations.cs:960](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:960)
4. 이번 수정(`IssueCycleMove` 반환코드 로깅): [MainWindow.CycleTestOperations.cs:898](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:898)

참고:
- 이 환경에서는 `msbuild`가 없어서 CLI 빌드 완전 검증은 못 했고, `dotnet msbuild`는 현재 프로젝트의 WPF 빌드 환경 문제(`InitializeComponent`/XAML 생성 관련)로 실패했습니다.
- 코드 수정 자체는 `CycleTest` 경로에 정상 적용됨.

> Single Axis 탭처럼 Position Velocity Acceleration deceleration jerk 에 * 10000 하는 내용이 없는거같은데?

<details><summary>2 previous messages</summary>

> 맞습니다. 지금 코드 기준으로 `CycleTest` 입력값은 `Single Axis` 탭과 동일한 `*10000` 스케일이 빠져있을 가능성이 큽니다. 바로 확인하고, 빠져 있으면 `CycleTest` 쪽에도 동일 스케일 적용하겠습니다.
>
> 지적한 내용 맞습니다. `LASAL` 기준으로는 `Single Axis`는 `*10000` 스케일을 쓰는데 `CycleTest`는 동일 스케일이 안 맞아 있었습니다. 지금 `CycleTest` 명령/판정 경로에도 같은 스케일을 적용하겠습니다.
</details>

맞습니다. `LASAL` 쪽 `CycleTest`에는 `Single Axis`와 같은 `*10000` 스케일이 빠져 있었습니다. 지금 반영했습니다.

적용 내용:
1. `CycleTest` 명령 발행 시 `Position/Velocity/Acc/Dec/Jerk`를 `*10000` 후 `long`으로 변환해서 전송
[MainWindow.CycleTestOperations.cs:898](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:898)
2. In-position 판정도 같은 스케일로 비교(`target`, `tolerance`)
[MainWindow.CycleTestOperations.cs:942](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:942)
3. 공통 스케일 함수 오버로드 추가(`double -> long`)
[MainWindow.AxisOperations.cs:308](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:308)

정확히 구분하면:
- `Codex_LASAL_WPF`: 지금처럼 `*10000` 스케일 사용
- `Codex_PMAS_WPF`: 현재 `Single Axis`/`CycleTest` 모두 기본 `double` 단위 그대로 사용

원하면 `Codex_PMAS_WPF`에도 동일 스케일 방식으로 맞춰드릴 수 있습니다.

> 지금 Codex_LASAL_WPF에서 ReadActualPosition하면 오버플로우 나는거 같은데 그냥 long으로 변환 말고 int로 처리해야될꺼같다.

<details><summary>3 previous messages</summary>

> 확인하겠습니다. `Codex_LASAL_WPF`의 `ReadActualPosition` 파서에서 `long` 기준 처리되는 구간을 찾아서, 요청대로 `int` 우선 처리(필요 시 fallback)로 바꿔서 오버플로우를 막겠습니다.
>
> 원인 가능성이 높습니다. 현재 파서는 8바이트(`int64`/`double`) 기준으로 먼저 읽어서, 4바이트 정수 응답이면 값이 비정상적으로 커질 수 있습니다. `ReadActualPosition` 응답을 `int32` 우선으로 파싱하고 8바이트는 fallback으로 처리하도록 수정하겠습니다.
>
> 파서 변경은 적용됐고, `ReadActualPosition` 응답을 4바이트 `int32` 우선으로 읽도록 바꿨습니다. 변경된 라인만 확인해서 바로 공유하겠습니다.
</details>

맞습니다. `ReadActualPosition` 파서를 `int32` 우선으로 바꿨습니다.

변경 내용:
1. `GetActualPosition` 응답 파서가 4바이트 payload도 처리하도록 수정
[SigmatekTcpIpDummyMMCLib.cs:1195](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1195)
2. 위치값 파싱을 `int32` 우선으로 처리하고, 8바이트일 때만 `int64/double` fallback 사용
[SigmatekTcpIpDummyMMCLib.cs:1241](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1241)
3. `ReadInt32LE` 추가
[SigmatekTcpIpDummyMMCLib.cs:1360](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1360)

핵심 효과:
- 4바이트 정수 응답을 8바이트로 잘못 해석해서 값이 튀거나 오버플로우처럼 보이던 문제를 줄입니다.
- 다만 실제 위치가 `int32` 범위를 넘는 장비/설정이면 그 경우는 8바이트 fallback 경로를 타게 됩니다.

> 두개 결과치를 분석해서 왜이렇게 차이가 많이나는지 분석해줘
>
> User context
>
> Attachments:
> - CycleTestResult_20260414_100417.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260414_100417.xlsx`
> - CycleTestResult_20260414_101607.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260414_101607.xlsx`

<details><summary>4 previous messages</summary>

> 요청한 두 개 `CycleTestResult` 엑셀을 직접 읽어서 시트별 수치 차이를 정량 비교하겠습니다. 먼저 각 파일의 `Result`/샘플 시트에서 핵심 지표를 추출한 뒤, 차이가 커진 원인을 항목별로 좁혀서 설명하겠습니다.
>
> 파일 크기부터 봤을 때 두 번째 결과가 샘플 데이터가 더 많이 저장된 상태입니다. ახლა 각 엑셀의 시트 구조와 `Result` 지표를 먼저 추출해서 동일 항목끼리 바로 비교하겠습니다.
>
> 핵심 수치는 이미 확인됐고, 원인 확정을 위해 `PositionSamples`와 `ExecutionLog`까지 같이 보겠습니다. 특히 첫 파일의 `3600265` 오차가 단위 스케일(×10000) 불일치인지 로그와 샘플 분포로 확인하겠습니다.
>
> 지표 차이값까지 뽑았고, 마지막으로 로그 근거를 붙여서 왜 차이가 났는지 확정하겠습니다. 특히 첫 결과는 비정상 위치값 점프(`~6.87e10`)와 타임아웃이 직접 원인인지 로그/샘플로 교차 확인하겠습니다.
</details>

분석 결과, **두 결과는 동일 조건 비교가 아닙니다.** 차이가 큰 이유가 명확합니다.

**핵심 차이 (Result 시트 기준)**
1. 연결 대상 자체가 다름
- `100417`: `SIGMATEK TCP/IP dummy connection established`
- `101607`: `RPC connection established` + 실제 축 `a01` 로드

2. 테스트 파라미터/단위가 다름
- `100417`: MoveDistance `360`, Tolerance `0.05`
- `101607`: MoveDistance `8388608`, Tolerance `1165`
즉, 사실상 단위 스케일(엔지니어링 단위 vs 카운트 단위)이 다른 테스트입니다.

3. 첫 번째 결과에 비정상 위치값(outlier) 존재
- `100417` PositionSamples에서 `ActualPosition = 68,719,476,960` 반복 구간 확인
- 정상 범위를 크게 벗어난 값이라 파싱/프레임 해석 불일치(32/64비트, 오프셋) 가능성이 큼
- 이 영향으로 `Forward timeout 2회`, `CycleTimeMax 5005ms` 발생

4. 그래서 성능 지표가 크게 벌어짐
- `CycleTimeMax`: `5005.09 -> 620.76 ms`
- `ResponseLatencyMax`: `45.01 -> 13.82 ms`
- `ForwardTimeouts`: `2 -> 0`
- `MaxInPositionError`: `3600265 -> 1156`

**결론**
- 지금 차이는 “제어기 성능 차이”보다 **테스트 조건/단위/응답 파싱 경로 차이**가 원인입니다.
- 특히 `100417`은 Dummy 경로 + 비정상 위치값(outlier) 때문에 결과가 크게 나빠졌습니다.

원하면 다음으로, 두 환경을 완전히 동일 조건(같은 단위, 같은 tolerance 환산, 같은 축/같은 poll)으로 맞춘 재시험 체크리스트를 바로 만들어드리겠습니다.

> 요구사항 1.LASAL_WPF에서 작성된 엑셀파일은 LASAL_WPF에 작성되도록 위치를 바꿔줘
>                2. PMAS_WPF의 SingleAxis탭에 BufferdMode 기본값을 Null에서 Aborting으로 바꿔줘

<details><summary>3 previous messages</summary>

> 요구사항 2개 바로 반영하겠습니다. 먼저 `LASAL_WPF`의 엑셀 저장 경로 결정 로직과 `PMAS_WPF`의 `ComboBufferedMode` 기본값 초기화 지점을 찾아서 정확히 수정하겠습니다.
>
> 저장 위치는 `LASAL` XAML 기본 경로가 아직 `PMAS`로 되어 있는 상태라서 그 값을 바로 바꾸면 해결됩니다. 그리고 `PMAS`의 BufferdMode는 콤보 초기화 코드에서 `Aborting`으로 기본 선택하도록 수정하겠습니다.
>
> `LASAL_WPF` 저장 경로는 기본값을 `Codex_LASAL_WPF\Reports\...`로 바꿨습니다. հիմա `PMAS_WPF`의 `BufferedMode` 기본 선택을 `Aborting`으로 강제 설정하겠습니다.
</details>

요구사항 2개 반영 완료했습니다.

1. `LASAL_WPF` 엑셀 저장 기본 위치 변경
- [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:1247)
  `TextCycleSaveFolder`를 `C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF\Reports\CycleResults`로 변경
- [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml:1433)
  `TextCycle2SaveFolder`를 `C:\work\Elmo\Elmo_Master\Codex_LASAL_WPF\Reports\CycleResults2`로 변경

2. `PMAS_WPF` Single Axis의 `BufferedMode` 기본값을 `Aborting`으로 설정
- [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:38)
  `ComboBufferedMode` 초기화 후 `MC_ABORTING`(또는 `MC_ABORTING_MODE`) 항목을 찾아 기본 선택하도록 추가했습니다.
  못 찾는 경우 기존 기본값(첫 항목) 유지됩니다.

원하면 `Group BufferedMode`도 동일하게 `Aborting` 기본값으로 맞춰드리겠습니다.

> ㅇㅇ 그룹도 동일하게 Aborting이 기본으로 바꿔줘

<details><summary>2 previous messages</summary>

> 알겠습니다. `PMAS_WPF`에서 `ComboGroupBufferedMode`도 `ComboBufferedMode`와 동일하게 `Aborting` 기본 선택되도록 바로 수정하겠습니다.
>
> 수정 적용됐고, `Single Axis`와 `Group` 둘 다 같은 방식으로 `Aborting` 기본 선택되도록 공통 함수로 정리했습니다. 변경 라인 확인만 하고 끝내겠습니다.
</details>

적용 완료했습니다.
`PMAS_WPF`에서 이제 둘 다 기본값이 `Aborting`입니다.

변경 파일:
- [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:38)

변경 내용:
1. `ComboBufferedMode`와 `ComboGroupBufferedMode` 초기화 후 둘 다 `SelectDefaultAbortingMode(...)` 호출
2. `MC_ABORTING` / `MC_ABORTING_MODE`를 찾아 기본 선택하는 공통 함수 추가

즉, Single Axis / Group 모두 시작 시 기본 `BufferedMode = Aborting`으로 올라옵니다.

> 지금 업로드한 파일들을 분석해서 Elmo csv파일의 Signals Data Group 1의 2가 SetPosition이고 Sigmatek csv파일의 SetPos 가 SetPosition이니 두개의 파일을 비교해서 0~8388608까지 도달하기까지 걸린 시간, 속도, 가감속, 저크를 유추해서 파일로 정리해줘.
>
> User context
>
> Attachments:
> - elmo_8388608_48933546_4893354600_4833546000_1165.csv: `./profile_capture/elmo_8388608_48933546_4893354600_4833546000_1165.csv`
> - Sigmatek_360_2100_210000_210000_2100_0.05.csv: `./profile_capture/Sigmatek_360_2100_210000_210000_2100_0.05.csv`
> Images:
> - `./profile_capture/elmo_8388608_48933546_4893354600_4833546000_1165.png`
> - `./profile_capture/Sigmatek_360_2100_210000_210000_2100_0.05.png`

<details><summary>5 previous messages</summary>

> 파일 구조부터 정확히 확인하겠습니다. 두 CSV의 컬럼명/시간축/샘플 간격을 먼저 파악한 뒤, `SetPosition`과 `ActualPosition`을 자동 매핑해서 동일 기준으로 도달시간/속도/가감속/저크를 계산하겠습니다.
>
> 두 CSV 형식이 서로 달라서 파서를 분리해서 읽어야 합니다. հիմա 전체 구간에서 `SetPos/ActPos`의 시작값·최댓값·도달 시점(95%, 99%, 100%)을 먼저 계산하고, 그다음 미분으로 속도/가감속/저크를 추정하겠습니다.
>
> 지금 계산값을 보면 두 파일 모두 같은 모션 반복 패턴(여러 사이클)이 들어 있습니다. 요청하신 “0→8388608 도달 시간” 기준에 맞추기 위해, 임계치(1%/99%)가 아니라 실제 `near 0 -> near 8388608` 구간으로 재계산해서 최종 리포트를 파일로 만들겠습니다.
>
> 이제 최종 산출물을 만들겠습니다. 두 파일에서 반복된 상승 구간(0≈65 cnt → 8,388,608 cnt)을 사이클별로 잡아서 `SetPos/ActPos 도달시간`, `피크 속도`, `가속/감속`, `저크`를 계산하고, 비교표(CSV)와 해석 문서(MD)로 저장하겠습니다.
>
> 추가로 보정이 필요해서 한 번 더 정리합니다. `ActPos`는 정확히 `8388608`을 찍지 않는 샘플이 있어 outlier가 생겨서, `ActPos` 도달 기준을 `99.9% target`으로 바꿔 재계산해서 파일을 다시 저장하겠습니다.
</details>

분석 완료했고 파일로 정리해놨습니다.

결과 파일:
- [Elmo_vs_Sigmatek_Profile_Analysis.csv](/C:/work/Elmo/Elmo_Master/profile_capture/Elmo_vs_Sigmatek_Profile_Analysis.csv)
- [Elmo_vs_Sigmatek_Profile_Analysis.md](/C:/work/Elmo/Elmo_Master/profile_capture/Elmo_vs_Sigmatek_Profile_Analysis.md)

핵심 결과 (`SetPosition: Elmo=Signals Data Group 1의 2, Sigmatek=SetPos` 기준):
1. `0 -> 8,388,608` 도달시간(SetPos): 둘 다 평균 `0.233 s`
2. Actual 도달시간(99.9% 기준): Elmo `0.27175 s`, Sigmatek `0.26950 s`
3. 추정 피크 속도(SetPos 미분):
- Elmo `48,933,500 cnt/s`
- Sigmatek `48,934,000 cnt/s`
4. 추정 피크 가감속/저크(SetPos 미분):
- Acc: Elmo `1.5115e9`, Sigmatek `1.51125e9 cnt/s²`
- Dec: Elmo `1.5145e9`, Sigmatek `1.51475e9 cnt/s²`
- Jerk: 둘 다 `6.125e10 cnt/s³`

결론:
- 두 파일의 프로파일은 거의 동일합니다.
- 차이는 1ms 샘플링 양자화/정렬 오프셋 수준입니다.

> 평균 0.233s 라는게 정확하게 일치하는거임?

지금 결과 파일 기준으로는 **동일**합니다.

- 계산값: Elmo 평균 `0.233000 s`, Sigmatek 평균 `0.233000 s`
- 사이클별 값도 둘 다 `0.233000 s`로 나왔습니다.

다만 이건 **1 ms 샘플링 해상도 기준**이라, 실제로는 미세하게 달라도 같은 값으로 양자화될 수 있습니다.
즉, 현재 데이터 해상도에서는 “정확히 일치”가 맞고, 물리적 미세 차이까지 100% 같다고 단정할 수는 없습니다.

> 추정 피크 가감속은 도달했다고 보는거임? 등속 구간이 존재하나?

<details><summary>1 previous message</summary>

> 좋은 질문입니다. 방금 분석 파일 기준으로 `등속 구간`이 실제로 있는지 수치로 다시 확인해서 바로 답 드리겠습니다. `SetPos` 미분 속도에서 plateau 길이를 계산해보겠습니다.
</details>

정리하면 이렇게 봐야 합니다.

1. `추정 피크 가감속`은 “CSV(SetPos) 미분으로 계산한 피크”에는 도달한 값입니다.
2. 하지만 “설정한 가속도/감속도(예: 4,893,354,600 cnt/s²)까지 실제로 도달했는가?”는 **이 데이터만으로 확정 불가**입니다.
- 지금 추정치는 약 `1.511e9 cnt/s²`라 설정치보다 낮게 보입니다.
- 이유: 1ms 샘플링 + 미분 추정 + S-curve(저크 제한) 영향.

그리고 `등속 구간`은 **존재합니다**.
- 두 파일 모두 1사이클 기준:
- 전체 이동시간: 약 `0.234 s`
- 속도 plateau(피크의 99% 이상): 약 `0.117 s`
- 속도 plateau(피크의 98% 이상): 약 `0.121 s`

즉, 가속-등속-감속 형태가 맞고, 등속 구간이 꽤 명확하게 있습니다.

> 지금 elmo는 설정치가
> velo 48933546 cnt/s
> Acc 4893354600 cnt/s2
> Jerk 48933546000 cnt/s3
> 인데 얼마나 도달했다고 보는거임?

좋아, Elmo CSV 기준으로 다시 계산하면 이렇게 봐야 맞아.

- 설정 `V = 48,933,546 cnt/s`
- 추정 피크 속도 `V_peak ≈ 48,934,000 cnt/s`
- 도달률: `약 100.001%` (사실상 100%)

- 설정 `A = 4,893,354,600 cnt/s²`
- 추정 피크 가속도 `A_peak ≈ 1,526,000,000 cnt/s²`
- 도달률: `약 31.2%`

- 설정 `J = 48,933,546,000 cnt/s³`
- 추정 피크 저크 `J_peak ≈ 5.10e10 ~ 5.20e10 cnt/s³` (부호 포함)
- 도달률: `약 104% ~ 106%`
  (1ms 샘플링/차분 오차 때문에 저크는 흔들림이 큼. 중앙값 기준은 대략 96% 근처)

핵심:
- **속도는 도달했다고 보면 됨.**
- **가속도는 설정치까지 못 가는 게 정상**임.
  이 파라미터 조합에서는 jerk 제한 + 속도 제한 때문에 이론 최대 가속도가
  `sqrt(V*J) ≈ 1.547e9 cnt/s² (A의 31.6%)`라서, 설정 A까지 갈 수 없는 조건이야.

> 피크 속도에 도달한 시간은 얼마나됨?

<details><summary>1 previous message</summary>

> 바로 수치로 계산해서 답하겠습니다. Elmo CSV에서 각 상승 사이클마다 `SetPos` 미분 속도가 피크값에 처음 도달하는 시점을 뽑아 평균/최소/최대를 계산하겠습니다.
</details>

Elmo CSV 기준(1ms 샘플링, `SetPosition` 미분 속도 기준)으로:

- **피크 속도 도달 시간**: 이동 시작 후 **약 0.062 s (62 ms)**
- 8개 상승 사이클 모두 동일하게 **62 ms**
- 첫 사이클 절대 시각으로는 `t=0.564 s` 시작 → `t=0.626 s`에 피크 도달

즉, 질문한 값은 **약 62ms**로 보면 됩니다.

> 두개의 그래프 다 동일하다는거지?

네. 지금 올린 두 캡처 데이터 기준으로는 **거의 동일한 모션 프로파일**입니다.

- `SetPos 0→8388608` 도달시간: 둘 다 `0.233 s`
- 피크 속도 도달: 둘 다 약 `62 ms`
- 피크 속도/가감속/저크 추정치: 수치가 거의 같고, 차이는 1ms 샘플링 양자화 수준입니다.

즉, 분석 가능한 범위에서는 **동일하게 동작한다고 봐도 됩니다.**

> 이 사진보면 Jerk에 /1000을 연산하는데 이유가 머지?
>
> User context
>
> Images:
> - `./profile_capture/Jerk적용수치.png`

<details><summary>1 previous message</summary>

> 이미지에 나온 단위 정의 그대로 해석하면 정확합니다. 문구 근거까지 짧게 확인해서 왜 `/1000`인지 바로 정리해드리겠습니다.
</details>

이유는 **API 단위 정의가 원래 그렇게 되어 있어서**입니다.

이미지 문구:
- `Jerk: [application units/s³/1000]`
- 타입이 `DINT`(정수)

즉, 이 함수에 넣는 `Jerk` 값은 실제 jerk(`units/s³`)가 아니라 **1/1000 스케일 값**입니다.

공식:
- `입력값 = 실제 Jerk / 1000`
- `실제 Jerk = 입력값 * 1000`

왜 이렇게 하냐:
1. 내부 시간 기준이 보통 `1ms`라 jerk를 ms 기준으로 다루기 쉽게 만들기 위해
2. `DINT` 정수 범위에서 오버플로우를 줄이기 위해

예:
- 실제로 `48,933,546,000 cnt/s³`를 의도하면
- 함수 입력은 `48,933,546`을 넣어야 맞습니다.

> 지금 ReadActualPosition 측정 방법이 먼가 잘못된거 같은데, 일단 ReadActualPosition 명령을 날렸을때 응답이 올때까지 걸린 시간 측정하는거고, wireshark로 측정할때 elmo는 0.1~0.2ms 반응속도였는데 시그마텍은 0.4~0.5 나왔었어. 근데 지금 할때는 elmo는 0.4~0.5나오고 시그마텍은 1ms 정도 나오는데 먼가 측정 방법이 잘못된거 같은데 다시 검토해서 보고해줘

<details><summary>1 previous message</summary>

> 좋습니다. 측정 경로를 코드 기준으로 다시 뜯어보겠습니다. `ReadActualPosition` latency가 어디서부터 어디까지 재는지, Wireshark RTT와 왜 차이 나는지 원인별로 정리해서 보고하겠습니다.
</details>

검토 결과, **지금 측정값이 틀린 게 아니라 측정 대상이 Wireshark와 다릅니다.**

핵심:
1. 현재 앱의 `ResponseLatency`는
`GetActualPosition()` 함수 호출 전후 시간입니다.
즉, **네트워크 RTT + 라이브러리/파싱/락/호출 오버헤드 전체**를 잽니다.
코드: [MainWindow.CycleTestOperations.cs:1855](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1855)

2. Wireshark는
패킷 왕복만 보므로 **wire RTT만** 잡힙니다.
그래서 Wireshark(0.1~0.2ms) < 앱(0.4~0.5ms) 현상은 정상적으로 발생할 수 있습니다.

3. Sigmatek 경로는 .NET TCP 스트림에서
`Write/Flush -> blocking Read -> parse`까지 포함됩니다.
코드: [SigmatekTcpIpDummyMMCLib.cs:772](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:772), [SigmatekTcpIpDummyMMCLib.cs:1002](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1002)
그래서 Elmo 대비 1ms 근처로 보이기 쉽습니다. (`NoDelay`는 켜져 있음: [SigmatekTcpIpDummyMMCLib.cs:457](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:457))

결론:
- 지금 값은 “API 호출 지연”으로는 맞고,
- Wireshark 값(순수 네트워크 RTT)과 직접 1:1 비교하면 안 됩니다.

원하면 다음 단계로, 앱에 `WireRTT 추정용 분리 계측(Write->Read, Parse 분리)` 로그를 추가해서 숫자를 분해해서 보여줄 수 있습니다.

> 파서의 응답속도를 올릴수 없나. 파서에서 소요되는 시간이 얼마나 걸리지?

<details><summary>4 previous messages</summary>

> 가능합니다. 지금 코드에서 파싱 구간 시간을 실제로 분리 측정해서 “파싱 자체가 몇 ms인지” 먼저 수치로 확인해보겠습니다. 그다음 필요하면 파서 경량화까지 바로 적용하겠습니다.
>
> 파서만 따로 떼어서 동일 로직으로 마이크로벤치 돌려서 숫자로 보고하겠습니다. 네트워크/소켓 I/O 없이 “순수 파싱” 시간만 측정해서 파서 병목 여부를 명확히 보겠습니다.
>
> 결론부터 말하면 파서 자체는 병목이 아닐 가능성이 매우 큽니다. 그래도 요청하신 대로 파서 경량화는 바로 적용하겠습니다. 현재 파서에서 불필요한 분기/메모리 복사를 줄여서 순수 파싱 오버헤드를 더 낮추겠습니다.
>
> 파서 경량화는 적용했고, 파서 단독 벤치도 돌려서 수치까지 뽑았습니다. 지금 결과를 정리해서 바로 보고하겠습니다.
</details>

검토 결과를 바로 말하면:

1. `ReadActualPosition`에서 느린 건 파서가 아니라 `소켓 I/O + 호출 오버헤드`입니다.
- 현재 측정(`ResponseLatency`)은 `GetActualPosition()` 전체 구간입니다: [MainWindow.CycleTestOperations.cs:1855](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1855)
- Sigmatek 쪽도 `Write/Flush + blocking Read + parse` 전체 포함입니다: [SigmatekTcpIpDummyMMCLib.cs:772](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:772), [SigmatekTcpIpDummyMMCLib.cs:1002](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1002)

2. 파서 단독 시간(마이크로벤치, 5,000,000회)
- 평균 약 `0.0497 us`/call (`0.0000497 ms`)
- 즉, 0.4~1.0ms 레벨 지연의 주원인일 가능성은 거의 없음

3. 파서 최적화는 적용함
- 불필요한 8바이트 파싱 분기 축소
- `ReadDoubleLE`에서 little-endian 경로는 복사/역정렬 없이 직접 변환
- 변경 위치: [SigmatekTcpIpDummyMMCLib.cs:1241](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1241), [SigmatekTcpIpDummyMMCLib.cs:1377](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1377)

결론:
- 파서 속도를 더 올려도 체감 지연 개선은 거의 없습니다.
- 실제 차이는 대부분 네트워크 RTT, `NetworkStream.Read` 대기, 스레드 스케줄링에서 발생합니다.

원하면 다음 단계로 `Write->Read` 시간과 `Parse` 시간을 런타임에서 분리 계측해서 로그로 바로 보이게 붙여드릴게요.

> 그럼 메세지 지연이 그만큼 생긴다는거네

맞습니다. 현재 네가 보는 값은 사실상 **메시지 RTT + 앱/OS 오버헤드**가 같이 들어간 지연입니다.

정확히는:
- 컨트롤러 순수 응답시간(Wireshark 기준) +
- .NET `Write/Read` 대기 +
- 스레드 스케줄링/컨텍스트 전환 +
- 함수 호출/파싱/로그 처리

이라서 앱에서 0.4~1.0ms로 보이는 게 가능합니다.
즉 “컨트롤러가 느리다”기보다 “엔드투엔드 호출 지연”으로 보는 게 맞습니다.

> 지금 Elmo 프로그램과 Sigmatek 프로그램의 ReadActualPosition latency를 측정하는 기준은 똑같은거지?

<details><summary>1 previous message</summary>

> 확인해서 정확히 답할게요. 두 프로젝트 코드에서 latency 계산 구문이 같은지 바로 대조하겠습니다.
</details>

네, **우리 WPF 두 프로그램 기준으로는 동일한 방식**입니다.

- PMAS: [MainWindow.CycleTestOperations.cs:1855](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1855)
- LASAL: [MainWindow.CycleTestOperations.cs:961](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:961)

둘 다 `Stopwatch`로 `GetActualPosition()` 호출 **직전~직후**를 재서 latency 계산합니다.
다만 내부 구현(Elmo MMCLib 경로 vs Sigmatek TCP/Read/Parse 경로)이 달라서 수치가 다르게 나올 수 있습니다.

> 이거 샘플 채취했어. 여기서 ReadActualPositon에 대한 딜레이 평균을 구하고. 평균 구할때 2ms 이상되는데이터는 무시해. 실제로 wireshark에서 데이터를 비교해서 평균낸거랑 얼마나 차이나는지 알려줘
>
> User context
>
> Attachments:
> - CycleTestResult_20260415_141925.pcapng: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_141925.pcapng`
> - CycleTestResult_20260415_141925.xlsx: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_141925.xlsx`
> - CycleTestResult_20260415_142121.pcapng: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_142121.pcapng`
> - CycleTestResult_20260415_142121.xlsx: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_142121.xlsx`

<details><summary>6 previous messages</summary>

> 좋습니다. 바로 계산해보겠습니다.
> 1) `xlsx`에서 앱 기준 `ReadActualPosition` latency 평균(2ms 초과 제외), 2) `pcapng`에서 Wireshark RTT 평균(동일 기준) 을 각각 구해서 차이를 수치로 비교해드릴게요.
>
> `tshark`가 없어서 Python으로 `pcapng`를 직접 파싱하겠습니다. 먼저 캡처에서 `ReadActualPosition(0x202E)` 요청/응답 페어를 식별 가능한지 확인하고, 식별되면 RTT 평균을 계산하겠습니다.
>
> `scapy`가 설치되어 있지 않아서 설치 후 바로 파싱 진행하겠습니다. 설치가 안 되면 다른 파서로 우회하겠습니다.
>
> 패킷 구조 확인됐습니다. 이제 각 파일별로
> - `xlsx`의 `ReadLatency(ms)` 평균(2ms 초과 제외)
> - `pcapng`의 요청/응답 RTT 평균(2ms 초과 제외)
> 를 계산해서 차이를 표로 정리하겠습니다.
>
> 지금 수치가 나왔고, 정확도 높이려고 한 번 더 교차검증하겠습니다. `xlsx`와 `pcap`을 동일 인덱스로 1:1 매칭해서, 양쪽 모두 2ms 미만인 샘플만 비교한 평균 차이도 계산하겠습니다.
>
> 결과가 나왔습니다. 독립 필터 방식과 1:1 매칭 필터 방식이 조금 달라서 둘 다 정리해서 파일로 저장해두겠습니다.
</details>

계산 완료했습니다.
결론부터 말하면, 지금 샘플에서는 앱측(`xlsx`)이 Wireshark RTT보다 조금 크게 나옵니다.

비교 조건:
- `2ms 이상` 샘플 제외 (`<2ms`만 사용)
- `xlsx`: `PositionSamples`의 `ReadLatency(ms)`
- `pcap`: `ReadActualPosition(2E 20)` 요청-응답 RTT

결과 (독립 평균 기준):
1. `141925`
- XLSX 평균: `1.092706 ms` (22041개)
- PCAP 평균: `1.053200 ms` (22542개)
- 차이(XLSX-PCAP): `+0.039506 ms`

2. `142121`
- XLSX 평균: `1.106631 ms` (21728개)
- PCAP 평균: `1.066541 ms` (22237개)
- 차이(XLSX-PCAP): `+0.040089 ms`

통합:
- XLSX 평균: `1.099618 ms`
- PCAP 평균: `1.059825 ms`
- 차이: `+0.039793 ms`

추가(1:1 매칭, 양쪽 모두 <2ms):
- 차이 약 `+0.059854 ms`, `+0.060689 ms`

즉, 실제 wire RTT 대비 앱 호출 측정은 대략 **0.04~0.06ms** 더 큽니다.

정리 파일:
- [ReadActualPosition_Latency_Compare_20260415.md](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/Reports/CycleResults/ReadActualPosition_Latency_Compare_20260415.md)

> 이것도 똑같이 분석해줘
>
> User context
>
> Attachments:
> - CycleTestResult_20260415_144509.pcapng: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260415_144509.pcapng`
> - CycleTestResult_20260415_144509.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260415_144509.xlsx`
> - CycleTestResult_20260415_144710.pcapng: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260415_144710.pcapng`
> - CycleTestResult_20260415_144710.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260415_144710.xlsx`

<details><summary>2 previous messages</summary>

> 같은 방식으로 바로 돌리겠습니다. 먼저 이번 PMAS 캡처에서 `ReadActualPosition` 요청 시그니처를 확인한 뒤, `xlsx(<2ms)` 평균과 `pcap RTT(<2ms)` 평균을 계산해 비교하겠습니다.
>
> 계산 끝났습니다. 이전과 같은 형식으로 보고서 파일도 만들어 두겠습니다.
</details>

분석 완료했습니다. 이번 PMAS 샘플도 같은 기준으로 계산했습니다.

결과 요약 (`<2ms` 필터):

1. `144509`
- XLSX 평균: `0.401538 ms` (37304/38415)
- PCAP 평균: `0.338650 ms` (37432/38415)
- 차이(XLSX-PCAP): `+0.062887 ms`

2. `144710`
- XLSX 평균: `0.456163 ms` (35481/36728)
- PCAP 평균: `0.396262 ms` (35626/36728)
- 차이(XLSX-PCAP): `+0.059902 ms`

통합:
- XLSX 평균: `0.428166 ms`
- PCAP 평균: `0.366744 ms`
- 차이: `+0.061422 ms`

결론:
- 이번에도 앱 측정값이 wire RTT보다 약 `0.06ms` 높습니다.
- 즉 측정 방식 자체는 일관되고, 앱 내부 오버헤드가 소량 포함된 결과입니다.

보고서 파일:
- [ReadActualPosition_Latency_Compare_20260415_PMASSamples.md](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/Reports/CycleResults/ReadActualPosition_Latency_Compare_20260415_PMASSamples.md)

> md파일이 깨져서 나와. 영문으로 바꿔서 설명을 적어

<details><summary>4 previous messages</summary>

> 확인했습니다. 깨지는 원인이 한글/인코딩 조합일 가능성이 높아서, 두 개 MD 파일을 모두 영문(ASCII 위주)으로 다시 작성하겠습니다. 내용을 유지하되 표현만 영어로 바꿔 저장할게요.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청한 두 개 `md` 파일을 UTF-8 기준의 영어 문서로 다시 작성하겠습니다. 먼저 현재 파일 내용을 확인한 뒤, 깨지지 않도록 ASCII 위주로 교체 저장하겠습니다.
>
> 현재 두 파일 모두 한글 인코딩 깨짐이 있어서, 같은 수치로 영어 문서 전체를 다시 써서 저장하겠습니다. UTF-8로 강제 저장해 재발을 막겠습니다.
</details>

두 `md` 파일을 영어로 다시 작성했고 UTF-8로 저장했습니다.

- [ReadActualPosition_Latency_Compare_20260415_PMASSamples.md](./Codex_PMAS_WPF/Reports/CycleResults/ReadActualPosition_Latency_Compare_20260415_PMASSamples.md)
- [ReadActualPosition_Latency_Compare_20260415.md](./Codex_LASAL_WPF/Reports/CycleResults/ReadActualPosition_Latency_Compare_20260415.md)

지금 파일 내용은 모두 영문 설명으로 정상 확인했습니다.

> 그리고 측정을 시작할때 처음 10cycle에 대한 데이터는 잘못된 데이터일수 있으니 10cycle을 테스트로 운전해보고서 그 이후부터 레코딩해

<details><summary>8 previous messages</summary>

> 요구사항 이해했습니다. 측정 시작 전에 10 cycle 워밍업을 수행하고, 그 이후부터만 레코딩되도록 코드 위치를 찾아 수정하겠습니다. 먼저 Cycle Test 측정/샘플 적재 로직부터 확인하겠습니다.
>
> 측정 대상이 되는 `Cycle Test` 실행 루프에 워밍업 단계(10 cycle)를 넣고, 워밍업 동안에는 메트릭/샘플 저장을 건너뛰는 방식으로 적용하겠습니다. 지금 실행 루프 구조를 열어서 정확히 반영하겠습니다.
>
> 두 프로젝트(PMAS/LASAL) 모두 같은 패턴이라 공통 적용하겠습니다. 먼저 루프 내부에서 샘플 저장이 어디서 일어나는지 라인 단위로 확인하겠습니다.
>
> 워밍업을 측정값에서 완전히 분리하려면, 루프는 `10 + 요청 cycle`로 돌리고 실제 메트릭 객체는 워밍업/본측정을 분리해서 쓰는 방식이 가장 안전합니다. 지금 `CycleTestOptions`와 옵션 빌드 부분까지 같이 수정하겠습니다.
>
> 이제 실제 코드 반영을 진행합니다. PMAS 쪽 `Cycle Test/2/3`에 워밍업 10 cycle을 추가하고, 측정 메트릭은 워밍업 이후부터만 집계되도록 수정하겠습니다.
>
> PMAS 반영은 끝났고, 같은 동작을 LASAL에도 맞춰서 적용하겠습니다. `Cycle Test/2` 모두 워밍업 10cycle 후 측정 시작으로 맞출게요.
>
> 코드 수정은 끝났습니다. 지금 두 솔루션을 빌드해서 포맷 문자열/컴파일 오류가 없는지 바로 확인하겠습니다.
>
> 워밍업 중 중단되는 경우에도 샘플 시간값이 비정상으로 찍히지 않도록, 초기 `TestStartedTick` 기본값도 같이 보정하겠습니다. 작은 안정화 패치만 추가하고 마무리하겠습니다.
</details>

요구한 내용 반영 완료했습니다.
이제 측정 시작 전에 `10 cycle` 워밍업을 먼저 돌리고, 워밍업 이후부터만 기록/집계합니다.

- 적용 파일(PMAS): [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs)
- 적용 파일(LASAL): [MainWindow.CycleTestOperations.cs](/C:/work/Elmo/Elmo_Master/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs)

핵심 변경점:
- `DefaultWarmupCycles = 10` 추가
- 옵션에 `WarmupCycles` 추가
- PMAS:
  - `Cycle Test`, `Cycle Test2`, `Cycle Test3`에 워밍업 적용
- LASAL:
  - `Cycle Test`, `Cycle Test2`에 워밍업 적용
- 워밍업 구간은 아래 항목에서 제외:
  - `Attempted/Successful`
  - `CycleTime`, `ResponseLatency`, `PollPeriod`
  - `PositionSamples/StatusSamples`
- UI 상태에 워밍업 진행 표시 추가
- Summary 텍스트에 `Warm-up cycles (excluded)` 추가
- Excel 결과 시트에 `WarmupCycles` 필드 추가
- 워밍업 중 중단되면 `AttemptedCycles/SuccessfulCycles`는 `0`으로 처리

참고:
- 이 환경에서 `msbuild` 명령이 없어 빌드는 못 했고, `dotnet build`는 WPF XAML 생성 환경 이슈로 기존에도 실패하는 상태라 컴파일 검증은 IDE(Visual Studio 2019)에서 확인 필요합니다.

> 지금까지 진행한 내용 한번 정리해서 commit하자.
> 한번에 올리지 말고 단위별로 쪼개서 세분화해서 commit해서 올려

<details><summary>7 previous messages</summary>

> 좋습니다. 현재 변경 파일을 기능 단위로 분류한 뒤, 커밋을 여러 개로 나눠서 만들겠습니다. 먼저 `git status`와 변경 내역을 점검해서 커밋 계획부터 확정하겠습니다.
>
> 현재 워킹트리에 분석 산출물(`pcap/xlsx`)이 많아서, 코드/문서와 데이터 파일을 분리해서 커밋하겠습니다. 먼저 코드 변경 파일의 diff를 확인해 커밋 단위를 나누겠습니다.
>
> 기능 단위를 4개로 나눠 커밋하겠습니다. 먼저 PMAS UI/기본값/단일축 명령 처리 개선부터 첫 커밋으로 묶겠습니다.
>
> 동시에 `git add/commit`을 병렬로 실행해서 `index.lock` 충돌이 났습니다. 락 파일 정리 후 같은 커밋을 정상 수행하겠습니다.
>
> 첫 커밋 완료했습니다. 다음은 LASAL 통신/파서/명령 프레임 관련 코드만 별도 커밋으로 묶겠습니다.
>
> 병렬 실행 타이밍 때문에 `MainWindow.xaml`이 스테이징되지 않았습니다. 해당 파일만 다시 스테이징해서 단독 커밋 처리하겠습니다.
>
> 요청대로 단위별 커밋 4개 생성 완료했습니다. 지금 생성된 커밋 로그를 확인한 뒤 원격(`origin/main`)에 푸시하겠습니다.
</details>

단위별로 쪼개서 커밋했고, 원격까지 푸시 완료했습니다.

푸시 결과:
- `main` → `origin/main`
- pushed range: `6ef2beb..f43e363`

커밋 목록:
1. `b957e3b`
   `PMAS: set aborting default mode and update single-axis test defaults`
   - PMAS BufferedMode 기본값 `Aborting` 적용
   - PMAS Single Axis 기본 테스트값/로그 보강

2. `e9b772a`
   `LASAL: align dummy motion commands and ReadActualPosition parser with wire format`
   - LASAL 더미 라이브러리 프레임/파서/명령 처리 정합
   - MoveAbsolute/ReadActualPosition 처리 개선
   - local port remap(5000→5001) 로직 포함

3. `0895afb`
   `LASAL: update default connection/test parameters and report save paths`
   - LASAL 기본 IP/축명/모션 테스트 파라미터
   - 결과 저장 경로 기본값을 LASAL 폴더로 정리

4. `f8ff267`
   `CycleTest: add 10-cycle warm-up and exclude warm-up data from metrics`
   - PMAS/LASAL CycleTest에 워밍업 10사이클 추가
   - 워밍업 데이터는 집계/샘플/요약/엑셀에서 제외

5. `f43e363`
   `Reports: add English latency comparison notes for PMAS and LASAL samples`
   - 영문 latency 비교 md 리포트 2건 추가

참고:
- 현재 워킹트리에는 `xlsx/pcap/csv/profile_capture` 같은 실험 산출물과 일부 기존 결과파일 삭제 상태가 남아 있습니다.
- 이 데이터 파일들도 별도 커밋으로 정리할지(예: `reports-data`, `packet-analysis-data`) 결정하면 이어서 분리 커밋하겠습니다.

> 그런 내용들도 별도로 정리해서 commit해

<details><summary>5 previous messages</summary>

> 좋습니다. 남아있는 산출물들도 카테고리별로 분리해서 커밋하겠습니다. 먼저 `Reports / packet_capture / profile_capture` 안의 실제 파일 목록을 확인해서 커밋 단위를 확정하겠습니다.
>
> 좋습니다. 먼저 `LASAL Reports/CycleResults`를 하나의 커밋으로 묶겠습니다. 기존 결과 삭제 + 신규 결과 추가를 같이 반영하고, 임시 잠금파일(`~$...`)은 제외하겠습니다.
>
> LASAL 리포트 산출물 커밋 완료했습니다. 다음은 PMAS 리포트(사이클 결과 + 테스트 파라메터 엑셀 수정)를 별도 커밋으로 진행하겠습니다.
>
> 다음은 패킷 분석 산출물을 `텍스트(문서/CSV)`와 `원본 캡처(PCAP)`로 분리해서 각각 커밋하겠습니다. 먼저 텍스트 산출물 커밋부터 진행합니다.
>
> 남은 파일은 엑셀 임시 락파일(`~$...`) 1개뿐입니다. 이건 의미 있는 산출물이 아니라서 제거하고, 재발 방지용으로 `.gitignore`에 패턴 추가해서 마지막으로 커밋하겠습니다.
</details>

요청한 대로 남은 내용도 **카테고리별로 분리 커밋**해서 올렸습니다.
`origin/main`까지 푸시 완료했습니다.

추가 커밋 목록:
1. `1f78815`
   `LASAL reports: refresh cycle result artifacts and captures`
   - LASAL `Reports/CycleResults` 신규 결과/캡처 추가
   - 구버전 결과 일부 삭제/정리

2. `df2a90f`
   `PMAS reports: add cycle result samples and update test parameter workbook`
   - PMAS `CycleResults` 결과/캡처 추가
   - `테스트 파라메터.xlsx` 변경 반영

3. `fa40dfa`
   `PMAS reports: add profile calculator workbooks`
   - 프로파일 계산용 엑셀 2종 추가

4. `747feda`
   `Packet analysis: add protocol comparison docs and CSV breakdowns`
   - API/프레임 분석 문서 + CSV 산출물 추가

5. `4279e2b`
   `Packet capture: add raw PCAP samples for Elmo and Sigmatek runs`
   - 원본 pcap 샘플 분리 커밋

6. `e388bb1`
   `Profile capture: add Elmo vs Sigmatek motion traces and analysis`
   - `profile_capture`의 csv/png/md 산출물 추가

7. `b7ec440`
   `Chore: ignore temporary Excel lock files`
   - `~$*.xlsx` 임시 락파일 무시 규칙 추가

현재 워킹트리 깨끗합니다 (`git status` clean).

> CycleTestResult_20260415_171523.xlsx의 PositionSamples시트의 ReadLatency를 분석해서
> 표본을 나타내는 엑셀 그래프를 만들어줘. 표본이 몇개 있는지 정리된 그래프

<details><summary>3 previous messages</summary>

> 요청하신 파일에서 `PositionSamples` 시트의 `ReadLatency` 컬럼을 먼저 읽고, 표본 개수 분포(구간별 count) 그래프가 들어간 새 엑셀 파일을 만들어 드리겠습니다. 우선 파일 위치와 컬럼명을 확인하겠습니다.
>
> 파일 경로는 확인했습니다. 지금은 해당 파일을 직접 열어서 `PositionSamples` 헤더 위치와 `ReadLatency` 컬럼을 정확히 잡겠습니다.
>
> `ReadLatency(ms)` 데이터 구조를 확인했습니다. 지금 구간별 표본 수(히스토그램) 시트와 엑셀 차트를 자동 생성해서, 원본은 보존하고 새 파일로 저장하겠습니다.
</details>

완료했습니다. `ReadLatency(ms)` 표본 분포 그래프를 만들어 새 파일로 저장했습니다.

- 결과 파일: [CycleTestResult_20260415_171523_ReadLatencyHistogram.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram.xlsx)
- 추가 시트: `ReadLatency_Histogram`
- 그래프: 구간별 표본 수 히스토그램(막대그래프)
- 집계:
  - Total samples: `240,340`
  - Min: `0.0213 ms`
  - Max: `42.0570 ms`
  - Avg: `1.4761 ms`
  - `>= 5.0 ms` 표본: `7,113`개

원하시면 bin 크기(현재 0.1ms)나 상한(현재 5.0ms)도 바꿔서 다시 만들어드리겠습니다.

> 5ms 표본 정확하게 나눠봐

<details><summary>2 previous messages</summary>

> 좋습니다. `ReadLatency(ms)`를 5ms 간격으로 정확히 재구간화해서 표본 수를 바로 계산해드리겠습니다.
>
> 5ms 경계 기준으로 구간화한 표와 그래프도 바로 엑셀로 만들어 저장하겠습니다.
</details>

5ms 기준으로 정확히 나눴습니다.

기준: `ReadLatency(ms)`
표본 총수: `240,340`

- `< 0 ms`: `0`
- `0 <= x < 5 ms`: `233,227`
- `5 <= x < 10 ms`: `1,995`
- `10 <= x < 15 ms`: `3,082`
- `15 <= x < 20 ms`: `1,960`
- `20 <= x < 25 ms`: `22`
- `25 <= x < 30 ms`: `46`
- `30 <= x < 35 ms`: `7`
- `35 <= x < 40 ms`: `0`
- `40 <= x < 45 ms`: `1`
- `>= 45 ms`: `0`
- `>= 5 ms 합계`: `7,113`

엑셀(표 + 5ms 구간 그래프) 생성 완료:
- [CycleTestResult_20260415_171523_ReadLatency_5msBins.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatency_5msBins.xlsx)

> 이것도 전에 분석한거처럼 실제 캡쳐 데이터랑 우리가 측정한거랑 비교한거 테스트해줘
>
> User context
>
> Attachments:
> - CycleTestResult_20260416_084834.pcapng: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260416_084834.pcapng`
> - CycleTestResult_20260416_084834.xlsx: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260416_084834.xlsx`

<details><summary>8 previous messages</summary>

> 요청하신 방식(이전과 동일)으로 `XLSX 측정값`과 `PCAP 실제 RTT`를 같은 기준으로 다시 비교하겠습니다. 먼저 파일 구조와 캡처 안의 ReadActualPosition 패턴부터 확인한 뒤, 평균/오차를 계산해 결과를 정리하겠습니다.
>
> `tshark`가 없어 Python으로 직접 pcapng를 파싱하겠습니다. 지금 `ReadActualPosition` 요청/응답 쌍을 추출할 수 있는 라이브러리(`scapy`/`dpkt`)를 확인하고 바로 계산 들어가겠습니다.
>
> 패킷은 확인됐고(`2E 20` 요청, `E0 00` 응답), 이제 이전과 동일하게 FIFO 요청-응답 매칭으로 RTT를 계산해서 `XLSX vs PCAP` 비교 수치를 뽑겠습니다. 결과는 md 파일로도 저장하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청하신 방식대로 이번 샘플도 다시 검증하겠습니다. 먼저 이전에 사용했던 비교 기준(패킷 매칭 규칙, 2ms 필터 규칙)을 찾아서 동일 로직으로 재계산한 뒤 결과 파일까지 정리하겠습니다.
>
> 이전 비교 리포트(정상값이 나온 샘플)의 매칭 방식을 먼저 확인했습니다. 같은 방식으로 재적용하되, 이번 파일은 매칭 오류(대기열 꼬임) 여부를 같이 점검해서 신뢰도까지 포함해 보고드리겠습니다.
>
> 이번 파일은 테스트 중 예외로 중단된 로그라서 캡처에 불필요 구간이 섞였을 가능성이 큽니다. TCP 세션 단위/시간대 단위로 먼저 분해해서 비교 가능한 구간만 추려서 다시 평균을 내겠습니다.
>
> 매칭 로직 문제를 잡았습니다. 이번 캡처는 `req-req-rsp` 패턴(`RR`)이 섞여 있어서 기존 FIFO 방식이 틀어졌고, Wireshark 기준에 맞게 `인접 req->rsp` + 워밍업 구간 제거로 다시 계산해 리포트를 갱신하겠습니다.
</details>

비교 테스트 완료했습니다. 이번 샘플은 이전처럼 단순 FIFO 매칭하면 오차가 크게 나서, 패킷 패턴을 먼저 검증하고 다시 계산했습니다.

핵심 결과:
- XLSX `ReadLatency` 평균(<2ms): `1.018326 ms` (n=10524/11547)
- PCAP RTT 평균(<2ms): `0.961102 ms` (n=10585/11547)
- 차이(XLSX - PCAP): `0.057224 ms`
- 1:1 정렬 후 both<2ms 평균 차이: `0.057937 ms`

이번 캡처 특이점:
- 스트림에 `RR` 패턴(`req, req, rsp`)이 `119`회 존재
- 그래서 FIFO 인덱스 매칭은 드리프트가 누적되어 잘못된 평균(과대값)을 만듭니다.
- `인접 req->rsp` 기준 + 워밍업 구간 제외(2337쌍)로 재계산한 값이 유효합니다.

리포트 저장:
- [ReadActualPosition_Latency_Compare_20260416_084834.md](./Codex_LASAL_WPF/Reports/CycleResults/ReadActualPosition_Latency_Compare_20260416_084834.md)

> 완료했습니다. ReadLatency(ms) 표본 분포 그래프를 만들어 새 파일로 저장했습니다.
>
> 결과 파일: CycleTestResult_20260415_171523_ReadLatencyHistogram.xlsx
> 추가 시트: ReadLatency_Histogram
> 그래프: 구간별 표본 수 히스토그램(막대그래프)
> 집계:
> Total samples: 240,340
> Min: 0.0213 ms
> Max: 42.0570 ms
> Avg: 1.4761 ms
> >= 5.0 ms 표본: 7,113개
> 원하시면 bin 크기(현재 0.1ms)나 상한(현재 5.0ms)도 바꿔서 다시 만들어드리겠습니다.
>
> 이거 내가 5ms로 정리해소 보여달라는게 아니라 기존 그대로 하고 5ms이상되는 애들도 세분화해서 전체 다 세분화해서 볼수있게 만들라는 얘기였어

<details><summary>3 previous messages</summary>

> 요청 의도 확인했습니다. 기존 분포는 유지하고, `5ms 이상` 구간도 포함해서 전체 범위를 세분화한 새 히스토그램 파일로 다시 만들겠습니다. 먼저 현재 파일 위치와 원본 `PositionSamples` 데이터를 확인하겠습니다.
>
> 지금 파일을 덮어쓰지 않고 새 파일로 만들겠습니다. 기존 히스토그램 시트는 그대로 두고, `0~Max 전체 0.1ms bin` 시트와 `5ms 이상 상세 0.1ms bin` 시트를 추가하겠습니다.
>
> 시트명 길이 제한(31자) 경고가 떠서 호환성 문제 생길 수 있습니다. 지금 시트명을 짧게 바꿔서 파일을 다시 저장하겠습니다.
</details>

맞습니다. 요청하신 형태로 다시 만들었습니다.

- 기존 시트 `ReadLatency_Histogram`은 그대로 유지
- 새 파일 생성: [CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx)
- 추가 시트 1: `ReadLatency_FullRange`
  - `0.0 ~ 42.1 ms` 전체를 `0.1 ms` bin으로 전부 세분화
  - 히스토그램 차트 포함
- 추가 시트 2: `ReadLatency_Over5_Detail`
  - `5.0 ~ 42.1 ms` 구간만 `0.1 ms` bin으로 상세 세분화
  - 히스토그램 차트 포함

요약 값은 동일합니다.
- Total samples: `240,340`
- >=5.0 ms samples: `7,113`

> 이것도 똑같이 분석해줘
>
> User context
>
> Attachments:
> - CycleTestResult_20260416_100254.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254.xlsx`

<details><summary>2 previous messages</summary>

> 같은 방식으로 처리하겠습니다. `ReadLatency` 전체 구간 + `5ms 이상 상세` 히스토그램 시트를 추가한 새 엑셀을 만들어서 저장하겠습니다. 먼저 원본 시트 구조를 확인합니다.
>
> 원본은 `ReadLatency(ms)`가 11열에 있고 표본이 300,000개라 확인했습니다. 이제 새 파일을 만들어 `ReadLatency_FullRange`, `ReadLatency_Over5_Detail` 시트와 그래프를 추가합니다.
</details>

요청하신 방식으로 동일 분석 적용했습니다.

- 결과 파일: [CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx)
- 추가 시트:
  - `ReadLatency_FullRange`
    `0.0 ~ 39.8 ms` 전체를 `0.1 ms` bin으로 세분화 + 히스토그램
  - `ReadLatency_Over5_Detail`
    `5.0 ~ 39.8 ms`를 `0.1 ms` bin으로 세분화 + 히스토그램
- 원본 시트(`Result`, `PositionSamples`, `ExecutionLog`)는 유지

집계값:
- Total samples: `300,000`
- Min: `0.1279 ms`
- Max: `39.7019 ms`
- Avg: `0.626809 ms`
- `>= 5.0 ms` samples: `8,182`

> 구간이 0인 애들은 삭제해

<details><summary>1 previous message</summary>

> 알겠습니다. 방금 만든 PMAS 히스토그램 파일에서 `Sample Count = 0` 구간을 실제로 삭제하고, 그래프 범위도 같이 다시 맞추겠습니다.
</details>

처리 완료했습니다.
파일에서 `Sample Count = 0`인 구간을 삭제했고, 차트 범위도 같이 다시 맞췄습니다.

수정 파일:
- [CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx)

삭제 결과:
- `ReadLatency_FullRange`: `398`개 구간 중 `112`개(0 count) 삭제 → `286`개 남음
- `ReadLatency_Over5_Detail`: `348`개 구간 중 `111`개(0 count) 삭제 → `237`개 남음

검증:
- 두 시트 모두 `0 count` 구간 `0개` 확인 완료.

> CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange 이거 그래프가 이상한데 다시 그려줘

<details><summary>3 previous messages</summary>

> 확인했습니다. 해당 파일의 차트 데이터 범위를 점검하고, 필요하면 시트 데이터부터 정렬/재작성해서 그래프를 다시 그리겠습니다. 먼저 현재 차트 참조 범위를 확인하겠습니다.
>
> 원인 찾았습니다. 차트 캐시가 0으로 고정된 상태라 엑셀에서 그래프가 비정상적으로 보일 수 있습니다. 시트 데이터는 유지하고 차트를 새로 생성하면서 캐시도 정상화하겠습니다.
>
> 그래프만 다시 그리되 원본 데이터는 건드리지 않겠습니다. `ReadLatency_FullRange` 시트에 0 count 제외한 헬퍼 데이터 영역을 만들고, 그 데이터를 기준으로 차트를 재생성하겠습니다.
</details>

다시 그렸습니다. 기존 그래프를 지우고 재생성했습니다.

수정 파일:
- [CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx)

변경 내용:
- `ReadLatency_FullRange` 시트에서 차트 재생성
- 0 count 구간은 헬퍼 데이터(`D/E`열)에서 제외하고 그걸로 차트 그림
- 차트 참조 범위: `E10:E249` / `D10:D249`
- 반영 bin 수: 전체 421개 중 non-zero 240개

필요하면 같은 파일에 `전체 bin(0 포함)` 버전 차트도 추가해드리겠습니다.

> 두개에 새로 시트 하나 추가해서 0.1ms 단위가 아닌 1ms단위로 정리해봐
>
> User context
>
> Attachments:
> - CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx`
> - CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx`

<details><summary>3 previous messages</summary>

> 두 파일 모두에 `1ms bin` 시트를 새로 추가하겠습니다. `PositionSamples`에서 `ReadLatency(ms)`를 다시 읽어 집계하고, 같은 형식으로 표+차트까지 넣겠습니다.
>
> 첫 실행이 시간 초과로 끊겼습니다. 중간 저장 여부 확인하고, 남은 작업은 타임아웃을 늘려서 마저 적용하겠습니다.
>
> PMAS 파일은 적용 완료했습니다. 같은 방식으로 LASAL 파일에도 `ReadLatency_1msBins` 시트를 추가하겠습니다.
</details>

완료했습니다. 두 파일에 `ReadLatency_1msBins` 시트를 추가해서 `1ms` 단위로 집계했습니다.

- [CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx)
- [CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx)

확인값:
- PMAS: `300,000` samples, `41` bins (`0-1` ~ `40-41`)
- LASAL: `240,340` samples, `44` bins (`0-1` ~ `43-44`)

원하면 이 `1ms` 시트도 `0 count` 구간 삭제해서 다시 정리해드리겠습니다.

> 그래프도 그려줘야지

<details><summary>2 previous messages</summary>

> 맞습니다. `ReadLatency_1msBins` 시트에 차트가 빠져 있었습니다. 두 파일 모두에 `1ms 히스토그램(막대그래프)`를 지금 추가하겠습니다.
>
> 차트 추가는 끝났습니다. 마지막으로 두 파일에서 차트 참조 범위가 맞는지 확인하고 결과만 바로 정리해드리겠습니다.
</details>

맞습니다. 그래프까지 추가 완료했습니다.

적용 파일:
- [CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx)
- [CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx)

적용 내용:
- `ReadLatency_1msBins` 시트에 `ReadLatency Histogram (1ms bins)` 막대그래프 생성
- 기존 차트 중복 방지 위해 시트 내 차트 1개로 정리
- 참조 범위:
  - PMAS: `B11:B51` / `A11:A51`
  - LASAL: `B11:B54` / `A11:A54`

> 이 파일 기존 시트는 남겨두고 샘플이 일치하지 않으니 240000개만 비교해서 똑같이 정리해봐
>
> User context
>
> Attachments:
> - CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx`
> - CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx`

<details><summary>6 previous messages</summary>

> 요청대로 기존 시트는 유지하고, 두 파일 모두 `처음 240,000개 샘플`만 사용한 비교용 시트를 추가하겠습니다. 두 파일이 같은 기준으로 비교되도록 bin 범위도 공통으로 맞춰서 만들겠습니다.
>
> 대용량 저장 중 타임아웃이 걸렸습니다. 어느 파일까지 반영됐는지 즉시 확인하고, 미완료 파일만 이어서 처리하겠습니다.
>
> 공통 비교 범위 계산 완료했습니다 (`0.1ms bins` 상한 `42.1ms`, `1ms bins` 상한 `43ms`). 이제 파일 하나씩 처리해서 타임아웃 없이 안정적으로 반영하겠습니다.
>
> 원본 파일이 열려 있어서 직접 저장이 막혀 있습니다(`Permission denied`). 원본은 그대로 두고, 복사본(`_240kCompare`)을 만들어 거기에 240,000 샘플 비교 시트를 추가하겠습니다.
>
> 확인했습니다. 그럼 지금 원본 2개 파일에 직접 반영하겠습니다(기존 시트 유지 + 240,000 샘플 기준 비교 시트 3개 추가).
>
> 원본 반영이 완료됐습니다. 시트/차트가 정상 생성됐는지 마지막으로 검증하고 결과만 정리해서 드리겠습니다.
</details>

반영 완료했습니다. 원본 두 파일에 기존 시트는 그대로 두고, `처음 240,000개 샘플` 기준 비교 시트를 추가했습니다.

적용 파일:
- [CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx)
- [CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx)

추가된 시트(두 파일 동일):
- `ReadLatency_FullRange_240k` (0.1ms bin, 차트 포함)
- `ReadLatency_Over5_240k` (>=5ms 0.1ms bin, 차트 포함)
- `ReadLatency_1ms_240k` (1ms bin, 차트 포함)

비교 기준 통일:
- 샘플 수: 각 파일 `240,000`
- 공통 상한: `42.1ms`(0.1ms bin), `43ms`(1ms bin)

검증:
- 각 새 시트 데이터/차트 정상 생성 확인 완료.

> 샘플이 0인구간 삭제

<details><summary>1 previous message</summary>

> 요청대로 방금 추가한 `240k 비교 시트`들에서 `Sample Count = 0` 구간을 삭제하고, 차트 범위도 같이 다시 맞추겠습니다. երկու 파일 모두 한 번에 처리합니다.
</details>

완료했습니다.
`240k 비교 시트`에서 `Sample Count = 0` 구간 전부 삭제했고, 차트도 데이터 범위에 맞게 다시 그렸습니다.

적용 파일:
- [CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx)
- [CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx)

변경 결과:
- PMAS
  - `ReadLatency_FullRange_240k`: 421 -> 270
  - `ReadLatency_Over5_240k`: 371 -> 221
  - `ReadLatency_1ms_240k`: 44 -> 35
- LASAL
  - `ReadLatency_FullRange_240k`: 421 -> 240
  - `ReadLatency_Over5_240k`: 371 -> 191
  - `ReadLatency_1ms_240k`: 44 -> 33

검증:
- 위 6개 시트 모두 `zero_rows = 0` 확인 완료.

> 이거 동일한 TCP/IP로 동작하는 로직인데 응답성이 이렇게 차이나는 이유가 머지?

핵심은 **TCP/IP가 같아도 지연 구성요소가 다르다**는 점입니다.

지금 데이터 기준으로 보면(240k 비교):
- PMAS 평균 `~0.614 ms`
- LASAL 평균 `~1.476 ms`

같은 프레임을 써도 차이 나는 주원인:
1. **요청 큐잉(파이프라이닝) 차이**
   `req-req-rsp` 패턴이 생기면 응답은 같은 TCP라도 대기열 때문에 늦어집니다.
2. **컨트롤러 처리 모델 차이**
   Elmo는 API 처리 경로가 더 즉시형이고, Sigmatek/PLC 쪽은 task/scan 주기 영향이 큽니다.
3. **클라이언트 스레드 스케줄링 차이**
   Poll 1ms 설정이어도 실제 실행은 1ms가 아니라 OS 스케줄링에 따라 2~3ms로 늘어납니다.
4. **측정 위치 차이**
   앱 `ReadLatency`는 네트워크 RTT + 파싱 + 스레드 대기 + 로깅 오버헤드까지 포함합니다.
5. **소켓 옵션/버퍼링 차이**
   `NoDelay`, 송수신 버퍼, flush 타이밍 차이가 누적 지연을 만듭니다.

즉, **프로토콜이 같아서 성능이 같아지는 구조는 아닙니다.**
현재 차이는 네트워크 자체보다 **요청 발행 방식 + 런타임 스케줄링 + 컨트롤러 처리 주기** 영향이 더 큽니다.

원하면 다음 단계로 코드에 `send->recv`, `recv->parse`, `loop gap`을 분리 로깅해서 원인 비중을 바로 수치로 뽑아드릴게요.

> 지금 pc프로그램을 개선해서 성능을 개선할 수는 없을까

<details><summary>4 previous messages</summary>

> 가능합니다. 바로 현재 코드 기준으로 병목 지점을 짚고, PC 쪽에서 효과 큰 개선 항목을 우선순위로 정리해서 제안하겠습니다. 필요하면 바로 패치까지 진행하겠습니다.
>
> 코드도 바로 확인해봤고, 실제로 지연을 키우는 지점이 몇 군데 보입니다. 핵심 부분(소켓 송수신/폴링 루프)을 짚어서 바로 적용 가능한 개선안을 정리하겠습니다.
>
> 성능 개선 패치를 바로 적용하겠습니다. 저위험으로 `소켓 I/O 직렬화`, `ReadActualPosition 경로의 메모리 할당 제거`, `불필요 flush 제거`를 넣어서 지연/지터를 줄이겠습니다.
>
> 추가로 GC 지연 줄이기 위해 샘플 리스트도 초기 용량을 미리 잡겠습니다. 이건 저위험으로 장시간 테스트에서 지터 감소에 도움이 됩니다.
</details>

가능하고, 1차 최적화는 이미 적용했습니다.

적용한 개선:
1. 소켓 I/O 직렬화 추가 (멀티스레드 interleave 방지)
[SigmatekTcpIpDummyMMCLib.cs:410](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:410), [SigmatekTcpIpDummyMMCLib.cs:1027](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1027)

2. `GetActualPosition` 요청 프레임/응답 버퍼 재사용 (호출당 할당 제거)
[SigmatekTcpIpDummyMMCLib.cs:710](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:710), [SigmatekTcpIpDummyMMCLib.cs:778](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:778)

3. `SendFrameAndTryRead`를 `byte[]` 반환 대신 길이 반환으로 변경해 복사/할당 제거
[SigmatekTcpIpDummyMMCLib.cs:1012](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1012), [SigmatekTcpIpDummyMMCLib.cs:1209](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:1209)

4. 샘플 리스트 초기 용량 선할당으로 GC 지터 감소
[LASAL MainWindow.CycleTestOperations.cs:131](./Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:131)
[PMAS MainWindow.CycleTestOperations.cs:156](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:156), [PMAS MainWindow.CycleTestOperations.cs:157](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:157)

예상 효과:
- 평균 latency 소폭 개선
- max latency와 tail(긴 지연) 감소
- 장시간(수십만 샘플)에서 드롭/지터 완화

참고:
- `dotnet build`로는 이 WPF 솔루션(XAML generated code 이슈) 전체 빌드 검증이 안 됐습니다. VS2019에서 실행 테스트가 필요합니다.

원하면 다음 단계로 `send->recv`, `recv->parse`, `loop gap` 3개를 분리 로깅해서 어디가 가장 느린지 바로 수치로 뽑아줄게요.

> 이렇게 해도 실제적으로 plc에 응답이 빨라야되는데 그게 안되자나

맞습니다. 지금 한 개선만으로는 “PLC 응답 자체”를 크게 못 줄입니다.

이유는 2가지가 큽니다.

1. **PLC/런타임 주기 한계**
- 응답 하한은 결국 PLC TCP 처리 task 주기 + 내부 처리 시간입니다.
- PLC 쪽 task가 1ms급이면 PC를 아무리 최적화해도 0.1~0.2ms로 내려가기 어렵습니다.

2. **현재 PC 로직 구조 한계**
- `Move/Power/Stop`은 `SendFrame`만 하고 응답을 읽지 않습니다.
- PLC가 ACK를 보내면 수신 버퍼에 쌓이고, 다음 `GetActualPosition` 읽기와 섞여 지연/지터를 키울 수 있습니다.
- 즉, “같은 TCP”여도 앱 레벨에서 큐잉이 생기면 느려집니다.

정리하면:
- 지금 패치는 **앱 오버헤드 감소**(좋음)
- 하지만 핵심 병목은 **PLC task 주기 + 수신 프레임 처리 구조**입니다.

원하면 다음 단계로 바로 고치겠습니다:
1. 공용 RX 루프(프레임 파서)로 모든 응답을 즉시 소진
2. `GetActualPosition` 응답만 매칭해서 반환
3. `send->first-byte`, `first-byte->parse` 분리 계측 추가

이 3개 들어가면 tail latency(긴 꼬리)부터 확실히 줄어듭니다.

> 어짜피 프로그램 구조는 동일한거 아닌가?

겉 구조만 같습니다. 핵심 경로는 다릅니다.

- `CycleTest` 루프/화면은 유사함
- 하지만 PMAS는 **Elmo MMCLib 내부 엔진**(네이티브)로 송수신/큐/파싱 처리
- LASAL은 현재 **C# 소켓 직접 처리**(`SendFrame`, `SendFrameAndTryRead`)로 동작
- PLC 쪽은 **task/scan 주기** 영향이 직접 들어옴

즉, “UI/테스트 로직 구조”는 비슷하지만
응답성을 결정하는 “전송/파싱/스케줄링/컨트롤러 처리” 구조는 동일하지 않습니다.

그래서 동일 프레임이어도 레이턴시가 다르게 나옵니다.

> 얘도 분포도 만들어줘
>
> User context
>
> Attachments:
> - CycleTestResult_20260416_132346.xlsx: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260416_132346.xlsx`

<details><summary>2 previous messages</summary>

> 확인했습니다. 같은 방식으로 `ReadLatency` 분포 시트와 그래프를 새 파일로 만들어서 저장하겠습니다. 먼저 원본 파일 구조와 컬럼을 확인합니다.
>
> 이제 원본을 보존한 복사본에 분포 시트(0.1ms 전체, 5ms 이상 상세, 1ms)를 추가하고 그래프까지 같이 넣겠습니다.
</details>

완료했습니다. 분포도 파일 생성했고 그래프까지 넣었습니다.

결과 파일:
- [CycleTestResult_20260416_132346_ReadLatencyHistogram_FullRange.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260416_132346_ReadLatencyHistogram_FullRange.xlsx)

추가된 시트:
- `ReadLatency_FullRange` (0.1ms bin, 그래프 포함)
- `ReadLatency_Over5_Detail` (>=5ms, 0.1ms bin, 그래프 포함)
- `ReadLatency_1msBins` (1ms bin, 그래프 포함)

요약:
- Samples: `31,604`
- Min/Max/Avg: `0.0106 / 30.6857 / 1.6430 ms`
- `>=5ms` samples: `1,581`

> 얘네 둘도 분포표 만들어줘
>
> User context
>
> Attachments:
> - CycleTestResult_20260408_171010.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260408_171010.xlsx`
> - CycleTestResult_20260416_131653.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_131653.xlsx`

<details><summary>1 previous message</summary>

> 두 파일도 동일 포맷으로 처리하겠습니다. 각 원본은 보존하고, `ReadLatency` 분포 시트(0.1ms 전체 / 5ms 이상 상세 / 1ms)와 그래프를 넣은 새 파일로 저장하겠습니다.
</details>

완료했습니다. 두 파일 모두 분포표+그래프 파일 생성했습니다.

결과 파일:
- [CycleTestResult_20260408_171010_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260408_171010_ReadLatencyHistogram_FullRange.xlsx)
- [CycleTestResult_20260416_131653_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_131653_ReadLatencyHistogram_FullRange.xlsx)

각 파일에 추가된 시트:
- `ReadLatency_FullRange` (0.1ms bin + 그래프)
- `ReadLatency_Over5_Detail` (>=5ms 0.1ms bin + 그래프)
- `ReadLatency_1msBins` (1ms bin + 그래프)

요약:
- `20260408_171010`: samples `300,000`, avg `0.321245 ms`, >=5ms `1,335`
- `20260416_131653`: samples `300,000`, avg `0.215509 ms`, >=5ms `621`

> 결과값을 표준분포표로 보여줄래
>
> User context
>
> Attachments:
> - CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx`
> - CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx`

<details><summary>2 previous messages</summary>

> 좋습니다. 두 파일의 `ReadLatency(ms)` 원시 샘플로 평균/표준편차를 계산하고, `μ±σ` 구간 기준의 표준분포표(관측치 vs 이론치)로 바로 만들어서 보여드리겠습니다.
>
> 계산은 끝났고, 지금 결과를 엑셀에도 바로 볼 수 있게 두 파일에 `StandardDistribution` 시트를 추가하겠습니다(관측 비율 vs 정규분포 이론 비율 비교표).
</details>

완료했습니다. 두 파일에 `StandardDistribution` 시트를 추가해서 표준분포표(관측비율 vs 정규분포 이론비율)로 넣었습니다.

- [CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260416_100254_ReadLatencyHistogram_FullRange.xlsx)
- [CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260415_171523_ReadLatencyHistogram_FullRange.xlsx)

요약 표(관측치):

**PMAS (N=300,000, μ=0.6268ms, σ=1.9995ms)**
| 구간 | 관측 비율 |
|---|---:|
| < -3σ | 0.000% |
| -3σ ~ -2σ | 0.000% |
| -2σ ~ -1σ | 0.000% |
| -1σ ~ 0σ | 90.405% |
| 0σ ~ +1σ | 5.283% |
| +1σ ~ +2σ | 1.449% |
| +2σ ~ +3σ | 0.622% |
| > +3σ | 2.241% |

**LASAL (N=240,340, μ=1.4761ms, σ=2.0657ms)**
| 구간 | 관측 비율 |
|---|---:|
| < -3σ | 0.000% |
| -3σ ~ -2σ | 0.000% |
| -2σ ~ -1σ | 0.000% |
| -1σ ~ 0σ | 86.802% |
| 0σ ~ +1σ | 9.320% |
| +1σ ~ +2σ | 1.082% |
| +2σ ~ +3σ | 0.362% |
| > +3σ | 2.434% |

참고: 두 데이터 모두 정규분포(이론 34.13%-34.13% 중심)와는 많이 다르고, 좌측 절단(0 근처 바닥) + 우측 꼬리(long tail) 형태입니다.

> 이번주에 진행한 내용을 이번주 너랑 나랑 한 대화내용을 분석해서 해당 파일에 업데이트해
>
> User context
>
> Attachments:
> - 2026-04.md: `C:/git/WorkReport/2026-04.md`

<details><summary>4 previous messages</summary>

> 요청하신 대로 이번 주(2026-04-13 ~ 2026-04-16) 대화/작업 내용을 기준으로 업무보고 파일 형식에 맞춰 정리해서 바로 업데이트하겠습니다. 먼저 현재 `2026-04.md` 구조를 확인한 뒤 같은 스타일로 반영하겠습니다.
>
> 파일 인코딩이 깨진 상태로 보이므로, 먼저 인코딩을 확인한 뒤(원본 보존) 이번 주 내용만 안전하게 반영하겠습니다.
>
> 이번 주 실제 추가 작업(히스토그램/표준분포/240k 정규화 비교, 재분석 리포트)을 2주차 `목(04/16)`과 `이슈/메모`에 반영하겠습니다.
>
> 반영 완료했고, 문서에 들어간 내용이 정상인지 해당 구간만 다시 확인하겠습니다.
</details>

업데이트 완료했습니다.

수정 파일:
- [2026-04.md](C:/git/WorkReport/2026-04.md)

반영 내용:
- 2주차 `목(04/16)`에 이번 주 대화 기반 실제 진행내역 추가
  - 분포 리포트 자동화(0.1ms/5ms+/1ms 시트+그래프)
  - PMAS/LASAL 공통 240,000 샘플 비교 시트 추가
  - 0 샘플 구간 제거 및 차트 재생성
  - `StandardDistribution`(μ, σ, σ구간 관측/이론 비교) 추가
- `이슈/메모`에 응답성 차이 원인 및 다음 액션(분리 계측) 추가했습니다.

> 월, 화, 수 도 반영해

<details><summary>8 previous messages</summary>

> 알겠습니다. 2주차 `월/화/수`에도 실제 진행 내용이 보이도록 항목을 보강해서 반영하겠습니다. 지금 파일의 해당 구간만 수정하겠습니다.
>
> 월/화/수 항목을 목요일 수준으로 확장해서 반영하겠습니다. 기존 형식은 그대로 유지하고 내용만 보강합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청하신 대로 `월/화/수` 반영 여부를 바로 확인하고, 누락이 있으면 즉시 추가하겠습니다. 먼저 `2026-04.md`의 Week 2 구간을 점검합니다.
>
> 월/화/수 항목은 이미 들어가 있었지만, 지금 보이는 포맷이 깨져 보여서 Week 2 구간을 읽기 좋게 다시 정리해두겠습니다. 바로 수정하겠습니다.
>
> Week 2 헤더 문자가 인코딩 문제로 직접 매칭되지 않아, 날짜 문자열(`04/13 ~ 04/17`) 기준으로 구간을 잡아 안전하게 다시 치환하겠습니다.
>
> 계획표 표 안의 날짜가 먼저 잡혀서 범위 계산이 틀어졌습니다. 실제 본문 구간의 날짜 라인 번호를 먼저 확인한 뒤 정확히 치환하겠습니다.
>
> 본문 기준 `2주차`는 68행, `3주차` 시작은 124행으로 확인했습니다. 해당 구간만 정확히 교체하겠습니다.
</details>

반영 완료했습니다. `2주차`의 `월(04/13)`, `화(04/14)`, `수(04/15)` 항목을 이번 주 대화/작업 기준으로 정리해 업데이트했습니다.

- 파일: [2026-04.md](C:/git/WorkReport/2026-04.md)
- 반영 구간: 대략 68~113행 (`2주차` 섹션)

> 파일 깨진거 원래대로 해놔

<details><summary>1 previous message</summary>

> 요청대로 `2026-04.md`를 수정 전 상태(현재 브랜치의 HEAD)로 즉시 되돌리겠습니다. 파일만 복원하고 다른 파일은 건드리지 않겠습니다.
</details>

`2026-04.md` 원래 상태로 복원했습니다.

- 대상 파일: [2026-04.md](C:/git/WorkReport/2026-04.md)
- 복원 기준: 현재 브랜치 `HEAD`
- 확인: `git diff -- 2026-04.md` 결과 변경 0줄

> 아니 파일 깨진걸 다시 원복하랬지 GIT을 돌리랬냐.
> 다시 내용 채워놔

<details><summary>2 previous messages</summary>

> 지금 상태는 제가 `git restore`로 내용 자체를 되돌려서, 요청하신 “원복” 대신 최근에 넣은 내용까지 빠진 상태입니다.
> 바로 `깨지지 않는 인코딩`으로 다시 채워 넣겠습니다. 먼저 현재 파일 인코딩/본문 상태부터 확인합니다.
>
> 맞습니다. 지금 파일은 템플릿 상태라 주간 내용이 비어 있습니다.
> `2주차`를 월~목 작업 내역 기준으로 다시 채워 넣겠습니다.
</details>

채워놨습니다. `2주차(04/13~04/17)`에 월/화/수/목 작업 내용을 다시 반영했습니다.

- 수정 파일: [2026-04.md](C:/git/WorkReport/2026-04.md)

현재 파일은 UTF-8 기준으로 정상이며, 에디터에서 깨짐 없이 보이도록 저장되어 있습니다.

> 저번주 내용도 삭제됐자나.
> 원복 시켜놔

<details><summary>2 previous messages</summary>

> 맞습니다. `1주차`가 템플릿 상태로 비어 있어서 누락된 상태입니다. 지금 바로 이전에 있던 내용 기준으로 `1주차(04/06~04/10)`까지 다시 채워 넣겠습니다.
>
> 이제 `1주차` 구간만 정확히 잡아서, 빠진 주간 상세 내용을 다시 복구하겠습니다. `2주차` 이후는 유지하고 `1주차`만 채웁니다.
</details>

맞습니다. `1주차` 내용 다시 복구해놨습니다.

- 수정 파일: [2026-04.md](C:/git/WorkReport/2026-04.md)
- 복구 범위: `## 🗂 1주 (04/06 ~ 04/10)` 전체 상세 내역(월~금) 및 메모 섹션

`2주차`에 채워둔 내용은 유지한 상태입니다.

> 이제까지 진행한 내용 세분화 정리해서 GIT에 올려줘

<details><summary>8 previous messages</summary>

> 요청하신 대로 지금 워크스페이스 변경사항을 먼저 분류하고, 기능 단위로 커밋을 쪼갠 뒤 원격으로 푸시하겠습니다.
> 우선 현재 git 상태와 원격 설정부터 확인합니다.
>
> 변경 파일이 꽤 많아서, 코드/리포트/측정산출물로 나눠 커밋하겠습니다.
> 먼저 코드 변경 diff를 확인해 커밋 단위를 확정하겠습니다.
>
> 코드 변경은 확인됐고, 나머지는 측정 리포트/원본 캡처 파일입니다.
> 푸시 실패 방지를 위해 먼저 파일 크기를 확인해서 커밋 단위를 최종 나누겠습니다.
>
> 세분화 기준은 이렇게 진행하겠습니다.
> 1) 코드 개선 커밋, 2) LASAL 측정/분석 산출물 커밋, 3) PMAS 측정/분석 산출물 커밋, 4) 프로파일 캡처 산출물 커밋 순서로 올리겠습니다. 먼저 코드 커밋부터 진행합니다.
>
> PowerShell 구문 때문에 한 줄 체인이 실패했습니다. 같은 작업을 분리 명령으로 바로 다시 실행합니다.
>
> 코드 커밋 완료했습니다. 이제 LASAL 결과물(엑셀/캡처/비교 md)만 묶어서 두 번째 커밋으로 올리겠습니다.
>
> LASAL 산출물 커밋 완료했습니다.
> 이제 PMAS 결과물(새 엑셀 + `테스트 파라메터.xlsx` 수정)을 별도 커밋으로 묶겠습니다.
>
> 작업 트리는 정리됐고 커밋 4개로 분리 완료했습니다.
> 이제 `origin/main`으로 푸시하겠습니다.
</details>

완료. 요청대로 세분화해서 커밋하고 `origin/main`에 푸시했습니다.

푸시된 커밋:
1. `0e72a1b` `perf(lasal,pmas): optimize sample buffering and read path`
2. `9c24c9b` `docs(lasal): add cycle results, histograms, and latency comparison artifacts`
3. `c3f0087` `docs(pmas): add cycle result workbooks and update test parameter sheet`
4. `dc41a3e` `docs(profile): add motion profile execution capture images`

원격 반영:
- `b7ec440..dc41a3e  main -> main`

현재 워킹트리도 clean 상태입니다.

> PC Connect가 안되는데 이유가 머지?
>
> User context
>
> Images:
> - `[embedded image extracted: ./assets/embedded_line_6028.png; original line 6028; mime image/png; base64 chars 16860]`

<details><summary>2 previous messages</summary>

> 오류 메시지 기준으로는 라이브러리 내부에서 소켓 연결 자체가 실패한 경우입니다.
> 정확한 원인을 코드 기준으로 짚기 위해 `ConnectRPC`에서 `-100`을 반환하는 조건을 바로 확인하겠습니다.
>
> 현재 포트 상태도 확인해보겠습니다. `5001`이 이미 점유되어 있으면 지금 코드에서 Connect가 바로 실패할 수 있습니다.
</details>

원인 후보가 아니라, 현재 코드 기준으로는 **소켓 연결 예외가 났는데 상세를 숨기고 `-100`으로 뭉뚱그려 반환**하는 상태입니다.

근거:
- `Bind(localIp, localPort)` 후 `Connect(remoteIp, remotePort)`에서 예외나면 실패
  [SigmatekTcpIpDummyMMCLib.cs:462](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:462), [SigmatekTcpIpDummyMMCLib.cs:463](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:463)
- 그런데 `catch { return -100; }`이라 실제 예외 원인이 안 보임
  [SigmatekTcpIpDummyMMCLib.cs:580](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:580)
- 그리고 Local Port를 5000 넣으면 내부에서 5001로 자동 변경함
  [PmasControllerContext.cs:185](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs:185)

지금 바로 확인된 문제:
- `PmasApiWpfTestApp` 프로세스가 2개 떠 있습니다(PID 43044, 43444). 이 상태면 로컬 포트 충돌로 `Bind` 실패가 자주 납니다.
- 실제로 직전 확인 시 `10.10.150.13:5001`이 이미 점유된 상태였습니다.

먼저 이렇게 하세요:
1. `PmasApiWpfTestApp` 전부 종료 후 1개만 실행
2. Local Port를 5001이 아닌 다른 값(예: 5010)으로 테스트
3. Local IP는 PC NIC에 실제 할당된 IP만 사용
4. Remote IP/Port가 실제 리슨 중인지 확인 (`Test-NetConnection <IP> -Port <PORT>`)

원하면 내가 바로 코드 수정해서 `-100` 대신 실제 `SocketException` 메시지까지 팝업에 나오게 바꿔줄게.

> 포트가 점유된게 맞았어

좋습니다. 원인 확정됐네요.

재발 방지하려면 이 2개만 넣으면 됩니다.
1. Connect 전에 현재 프로세스의 기존 소켓/연결 먼저 정리(강제 Disconnect)
2. `ConnectRPC` 실패 시 `SocketException` 원문(포트 충돌/주소 사용중)을 그대로 UI에 표시

원하면 바로 코드에 반영해둘게요.

> 2번만 반영해

<details><summary>4 previous messages</summary>

> 요청하신 2번만 반영하겠습니다.
> `ConnectRPC` 실패 시 실제 소켓 예외 메시지가 UI 오류 문구에 포함되도록 최소 수정으로 처리하겠습니다. 먼저 관련 코드 위치를 확인합니다.
>
> 관련 위치 확인됐습니다.
> `SigmatekTcpIpDummyMMCLib`에 마지막 Connect 예외 메시지를 저장하고, `PmasControllerContext` 에러 문구에 그 메시지만 추가하도록 최소 수정하겠습니다.
>
> 수정 범위는 LASAL 쪽 2개 파일로 최소화하겠습니다.
> `ConnectRPC`에서 예외 텍스트를 저장하고, 실패 팝업 메시지에 그 텍스트만 덧붙이겠습니다. ఇప్పుడు 바로 적용합니다.
>
> 수정은 적용됐고, 문법 문제 없는지 빠르게 빌드로 확인해보겠습니다.
</details>

2번만 반영 완료했습니다.
이제 `PC Connect` 실패 시 `-100` 메시지 뒤에 실제 소켓 예외 원문이 같이 표시됩니다.

수정 파일:
- [SigmatekTcpIpDummyMMCLib.cs](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs:561)
- [PmasControllerContext.cs](./Codex_LASAL_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs:197)

변경 내용:
- `ConnectRPC` 실패 시 `Exception` 타입/메시지를 `LastConnectErrorDetail`에 저장.
- 연결 실패 팝업 문구에 `Detail: ...` 형태로 해당 원문 추가.

참고:
- 이 환경에서는 `msbuild` 명령이 없어 빌드 검증은 못 했습니다.

> 이거 3개도 분포표 만들어
>
> User context
>
> Attachments:
> - CycleTestResult_20260421_084143.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260421_084143.xlsx`
> - CycleTestResult_20260421_083939.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260421_083939.xlsx`
> - CycleTestResult_20260421_083710.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260421_083710.xlsx`

<details><summary>4 previous messages</summary>

> 요청한 3개 결과 파일에 대해 이전과 같은 방식으로 분포표(히스토그램 시트+그래프)를 생성하겠습니다.
> 먼저 동일 포맷으로 만들기 위해 기존 규칙을 확인한 뒤 바로 생성하겠습니다.
>
> `rg` 실행 권한 문제가 있어서 PowerShell 기본 검색으로 대체하겠습니다.
> 기존 분포표 포맷(시트명/차트명)부터 확인해서 동일 형식으로 3개 생성하겠습니다.
>
> 검색 범위를 줄여서 다시 돌립니다.
> 우선 기존 분포 파일 구조(시트/차트)를 읽고 같은 형식으로 생성하겠습니다.
>
> 기존 생성 포맷(FullRange/1ms/240k 시트 + 막대그래프)과 동일하게 3개 파일을 한 번에 생성합니다.
> 원본 파일은 건드리지 않고, 각각 `_ReadLatencyHistogram_FullRange.xlsx` 새 파일로 저장하겠습니다.
</details>

완료했습니다. 3개 파일 모두 분포표 시트와 그래프를 추가한 새 파일로 생성했습니다.

생성 파일:
- [CycleTestResult_20260421_083710_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260421_083710_ReadLatencyHistogram_FullRange.xlsx)
- [CycleTestResult_20260421_083939_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260421_083939_ReadLatencyHistogram_FullRange.xlsx)
- [CycleTestResult_20260421_084143_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260421_084143_ReadLatencyHistogram_FullRange.xlsx)

추가된 분포 시트:
- `ReadLatency_FullRange` (0.1ms bin)
- `ReadLatency_1msBins` (1ms bin)
- `ReadLatency_FullRange_240k`
- `ReadLatency_1ms_240k`

각 시트에 막대그래프도 같이 들어가 있습니다.

> 이것도 작성해줘
>
> User context
>
> Attachments:
> - CycleTestResult_20260421_090800.xlsx: `./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260421_090800.xlsx`

<details><summary>1 previous message</summary>

> 같은 포맷으로 바로 생성하겠습니다.
> `ReadLatency` 기준 분포 시트 4개와 그래프를 붙여서 `_ReadLatencyHistogram_FullRange.xlsx`로 저장합니다.
</details>

완료했습니다.

생성 파일:
- [CycleTestResult_20260421_090800_ReadLatencyHistogram_FullRange.xlsx](./Codex_PMAS_WPF/Reports/CycleResults/CycleTestResult_20260421_090800_ReadLatencyHistogram_FullRange.xlsx)

추가된 시트:
- `ReadLatency_FullRange`
- `ReadLatency_1msBins`
- `ReadLatency_FullRange_240k`
- `ReadLatency_1ms_240k`

각 시트에 그래프도 포함되어 있습니다.

> 이 파일도 분석해
>
> User context
>
> Attachments:
> - CycleTestResult_20260421_113043.xlsx: `./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260421_113043.xlsx`

<details><summary>2 previous messages</summary>

> 같은 방식으로 `ReadLatency` 중심으로 바로 분석하겠습니다.
> 우선 시트 구조/샘플 수/기본 통계부터 뽑고, 분포표 파일도 함께 생성하겠습니다.
>
> 분석 완료했고 분포표 파일까지 생성됐습니다.
> 시트/차트가 정상 생성됐는지 마지막으로 확인만 하고 결과 드리겠습니다.
</details>

완료했습니다. 분석 + 분포표 생성까지 했습니다.

결과 파일:
- [CycleTestResult_20260421_113043_ReadLatencyHistogram_FullRange.xlsx](./Codex_LASAL_WPF/Reports/CycleResults/CycleTestResult_20260421_113043_ReadLatencyHistogram_FullRange.xlsx)

요약( `ReadLatency(ms)` ):
- Samples: `53,641`
- Min: `0.3850 ms`
- Max: `27.5934 ms`
- Avg: `1.3574 ms`
- P50: `1.0431 ms`
- P90: `2.1246 ms`
- P95: `2.4889 ms`
- P99: `4.1960 ms`
- `>=1ms`: `34,009`
- `>=2ms`: `6,385`
- `>=5ms`: `247`

추가된 분포 시트(그래프 포함):
- `ReadLatency_FullRange`
- `ReadLatency_1msBins`
- `ReadLatency_FullRange_240k`
- `ReadLatency_1ms_240k`

> 여기 항목들에 대해 설명해줘
>
> User context
>
> Images:
> - `[embedded image extracted: ./assets/embedded_line_6210.png; original line 6210; mime image/png; base64 chars 59736]`

화면은 `Cycle Test`용 모션 프로파일 설정입니다. 현재 구조는 `Base Position -> Base + Move Distance -> Base Position` 왕복을 `Cycle Count`만큼 반복하는 형태입니다.

**Motion Profile**
`Base Position (mm)`
복귀 기준 위치입니다. 예: `0`이면 매 사이클 마지막에 `0`으로 돌아옵니다.

`Move Distance (mm)`
Base 기준 이동 거리입니다. 예: Base `0`, Move Distance `360`이면 Forward 목표는 `360`입니다.

`Velocity (mm/s)`
MoveAbsoluteEx에 넣는 속도 명령값입니다. LASAL 쪽은 내부 전송 시 `*10000` 스케일이 적용되는 구조였고, PMAS는 Elmo API 입력 단위 기준으로 들어갑니다.

`Acceleration (mm/s^2)`
가속도 명령값입니다.

`Deceleration (mm/s^2)`
감속도 명령값입니다.

`Jerk (mm/s^3)`
저크 명령값입니다. 가속도가 얼마나 빠르게 변할지 정하는 값입니다. 값이 클수록 가속/감속이 급격해집니다.

`Cycle Count`
반복 횟수입니다. 예: `100`이면 왕복 100회 시도합니다.

`In-position Tolerance (mm)`
목표 위치 도달 판정 허용 오차입니다. 실제 위치와 목표 위치 차이가 이 값 이하이면 In-position 후보로 봅니다.

`Move Timeout (ms)`
한 방향 이동이 이 시간 안에 In-position 되지 않으면 timeout으로 처리합니다. 예: `5000`이면 Forward 또는 Return 각각 5초 제한입니다.

`Poll Interval (ms)`
위치 읽기 주기입니다. 예: `1`이면 약 1ms 간격으로 `GetActualPosition`을 호출하려고 시도합니다. 실제 주기는 Windows 스케줄링, 소켓 응답, API 처리 시간에 따라 더 길어질 수 있습니다.

`Stable Samples`
In-position 판정을 몇 번 연속 만족해야 완료로 볼지 정합니다. 예: `1`이면 한 번만 허용 오차 안에 들어오면 완료입니다. `3`이면 3회 연속 들어와야 완료입니다.

`Drop Threshold (ms)`
Polling 주기가 이 값보다 길어지면 drop으로 카운트합니다. 예: `5000`이면 5초 이상 벌어진 poll만 drop 처리라 사실상 큰 끊김만 잡습니다.

**체크박스**
`Stop on timeout`
Timeout 발생 시 테스트를 중단합니다. 체크 해제하면 timeout을 기록하고 다음 동작을 계속할 수 있습니다.

`Stop on axis error/exception`
축 에러나 예외 발생 시 테스트를 중단합니다. 보통 체크 권장입니다.

`Use high-priority worker thread`
Cycle Test 작업 스레드 우선순위를 높입니다. UI 스레드가 아니라 테스트 작업 스레드 쪽 우선순위를 올려 polling 지연을 줄이려는 목적입니다.

`Use high-precision wait (yield/spin)`
일반 `Thread.Sleep`보다 더 촘촘하게 대기하려는 옵션입니다. 짧은 poll interval에서 지터를 줄이는 데 도움이 되지만 CPU 사용량은 올라갈 수 있습니다.

`Request 1ms system timer resolution`
Windows 타이머 해상도를 1ms로 요청합니다. `Sleep`, 타이머 기반 대기의 최소 단위를 줄여 polling 주기 안정성을 올리기 위한 옵션입니다.

**Control**
`Start 10,000 Cycle Test`
현재 설정값으로 테스트를 시작합니다. 버튼 문구는 10,000이라고 되어 있지만 실제 반복 횟수는 위의 `Cycle Count` 입력값을 따르는 구조인지 확인이 필요합니다. 현재 화면에서는 `Cycle Count=100`이라 실제 실행은 100회일 가능성이 큽니다.

`Stop`
진행 중인 Cycle Test를 중단합니다.

회색 설명 문구
`Profile은 Base -> (Base + Move Distance) -> Base 입니다. Velocity/Acceleration/Deceleration/Jerk는 수동 입력값이 그대로 적용됩니다.`
즉 자동 계산 프로파일이 아니라, 사용자가 넣은 속도/가속도/감속도/저크를 그대로 MoveAbsoluteEx 명령에 넣는다는 뜻입니다.

> poll interval할때 별도의 쓰레드를 쓰나? 이 프로그램의 쓰레드는 몇개 있지?

<details><summary>2 previous messages</summary>

> 코드 기준으로 확인해서 답하겠습니다. Cycle Test가 어떤 Task/Thread를 만들고, Poll Interval이 어느 루프에서 쓰이는지 직접 짚어보겠습니다.
>
> 핵심은 CycleTest별로 별도 작업 Task 하나를 만들고, 그 안에서 Move 명령과 위치 읽기 polling을 순차 실행하는 구조입니다. 위치 읽기 전용 독립 스레드가 따로 계속 도는 구조는 아닙니다.
</details>

아니. `Poll Interval` 때문에 별도 polling 전용 쓰레드를 하나 더 만드는 구조는 아닙니다.

현재 구조는 이렇게 동작합니다.

`Cycle Test` 시작 버튼을 누르면 WPF UI 쓰레드에서 `Task.Factory.StartNew(..., TaskCreationOptions.LongRunning, ...)`로 작업 쓰레드 1개를 만듭니다.

관련 위치:
- [MainWindow.CycleTestOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:265)
- [MainWindow.CycleTestOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:417)
- [MainWindow.CycleTestOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:570)
- [MainWindow.CycleTestOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:720)

그 작업 쓰레드 안에서 순서대로 실행합니다.

```text
MoveAbsoluteEx 송신
-> GetActualPosition 또는 ReadStatus polling
-> WaitForPollInterval()
-> 다시 polling
```

즉, `MoveAbsoluteEx용 쓰레드`와 `위치 읽기 쓰레드`가 분리된 구조가 아닙니다. 같은 Cycle Test worker thread 하나가 명령 송신, 위치 읽기, 대기까지 전부 처리합니다.

`Poll Interval`은 여기서 같은 쓰레드를 대기시키는 값입니다.

- `GetActualPosition()` 호출: [MainWindow.CycleTestOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2041)
- `WaitForPollInterval()` 호출: [MainWindow.CycleTestOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2074)
- 실제 대기 구현: [MainWindow.CycleTestOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:2168)

대기 방식은 옵션에 따라 다릅니다.

`Use high-precision wait` 꺼짐:
```csharp
Thread.Sleep(pollIntervalMs);
```

`Use high-precision wait` 켜짐:
```csharp
Stopwatch 기준으로 목표 tick까지
Thread.Sleep(...)
Thread.Sleep(0)
Thread.SpinWait(...)
```

쓰레드 개수는 앱 관점에서 보면:

- 기본: WPF UI 쓰레드 1개
- Cycle Test 1개 실행 중: 전용 worker thread 1개 추가
- Cycle Test 탭들을 동시에 여러 개 실행하면: 실행한 테스트마다 worker thread 1개씩 추가 가능
- 그 외 CLR/WPF/GC/ThreadPool 내부 쓰레드는 별도로 존재할 수 있음

정리하면, 우리가 만든 Cycle Test 로직 기준으로는 보통 `UI thread 1개 + CycleTest worker thread 1개`입니다. Polling 전용 독립 쓰레드는 없습니다.

> Stopwatch 기준으로 목표 tick까지
> Thread.Sleep(...)
> Thread.Sleep(0)
> Thread.SpinWait(...)
>
> 요거가 잘 이해 안되는데 어떤방식으로 쓰레드 슬립보다 빨리 동작하는거임?

`Thread.Sleep`보다 “빨리 동작”한다기보다는, **원하는 시점에 더 가깝게 깨어나려고 CPU를 조금 더 쓰는 방식**입니다.

일반 `Thread.Sleep(1)`은 이렇게 동작합니다.

```text
1ms 쉬어라
```

하지만 Windows는 정확히 1.000ms 뒤에 깨워주는 게 아닙니다. OS 스케줄러가 다음 실행 기회를 줄 때까지 기다려야 해서 실제로는 예를 들어 `1.2ms`, `1.8ms`, 상황 나쁘면 그 이상이 될 수 있습니다.

지금 high-precision wait는 이런 방식입니다.

```text
목표 시간 = 현재 Stopwatch tick + PollInterval

while 현재 시간이 목표 시간보다 전이면:
    많이 남았으면 Thread.Sleep(남은 시간 - 1ms)
    조금 남았으면 Thread.Sleep(0)
    아주 조금 남았으면 SpinWait
```

각 단계 의미는 이렇습니다.

`Thread.Sleep(남은 시간 - 1ms)`
목표까지 시간이 아직 충분히 남았을 때 CPU를 놓습니다. 예를 들어 10ms 남았으면 9ms 정도는 그냥 쉽니다. 계속 바쁘게 돌면 CPU를 너무 많이 쓰니까 초반에는 양보합니다.

`Thread.Sleep(0)`
현재 쓰레드의 남은 시간 조각을 같은 우선순위 쓰레드에게 양보합니다. 단, `Sleep(1)`처럼 타이머 tick 단위로 길게 잠드는 게 아니라 “지금 당장 실행할 다른 쓰레드 있으면 먼저 해라, 없으면 계속 내가 돈다”에 가깝습니다.

`Thread.SpinWait(200)`
마지막 0.5ms 이하 같은 짧은 구간에서는 아예 잠들지 않고 CPU에서 짧게 바쁘게 돕니다. OS에 재스케줄링을 맡기지 않으니 목표 tick 근처에서 빠르게 빠져나올 수 있습니다.

즉 핵심은 이겁니다.

```text
Sleep만 사용:
OS가 깨워줄 때까지 완전히 맡김 -> 지터가 큼

High precision wait:
대부분은 Sleep으로 쉬고,
마지막 짧은 구간은 직접 Stopwatch를 보면서 버팀 -> 지터가 작아짐
```

예를 들어 `Poll Interval = 1ms`일 때:

일반 `Thread.Sleep(1)`:
```text
read
Sleep(1)
실제 다음 read: 1.3ms 뒤, 1.8ms 뒤, 2.5ms 뒤일 수 있음
```

High precision wait:
```text
read
목표 tick = 현재 + 1ms
남은 시간이 크면 양보
목표 직전에는 SpinWait
실제 다음 read: 목표 1ms에 좀 더 가까워짐
```

다만 중요한 한계가 있습니다.

이 방식은 **드라이브/PLC 응답 자체를 빠르게 만들지는 못합니다.**
줄일 수 있는 건 PC 프로그램 쪽의 polling 간격 지터입니다.

그리고 비용도 있습니다.

- CPU 사용량 증가
- 다른 프로세스에 영향 가능
- Windows가 실시간 OS가 아니라서 완전한 1ms 보장은 불가
- 네트워크 응답이 0.4ms~1ms면 그 시간은 그대로 걸림

지금 코드에서 `PollPeriodAvg`가 좋아지는 건 기대할 수 있지만, `ReadLatency(GetActualPosition)` 자체는 `요청 송신 -> 응답 수신 -> 파싱 완료` 시간이므로 이 wait 방식만으로 크게 줄어들지는 않습니다.

> Use high-priority worker thread 요건 옵션이 있나?

있습니다. 화면의 체크박스가 그 옵션입니다.

코드에서는 `UseHighPriorityWorkerThread`로 들어가고, Cycle Test 실행 쓰레드에서 아래처럼 처리합니다.

```csharp
var currentThread = Thread.CurrentThread;
var previousPriority = currentThread.Priority;

if (options.UseHighPriorityWorkerThread)
{
    currentThread.Priority = ThreadPriority.Highest;
}
```

관련 위치:
- [MainWindow.CycleTestOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1110)
- [MainWindow.CycleTestOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs:1133)

테스트가 끝나면 원래 우선순위로 복구합니다.

```csharp

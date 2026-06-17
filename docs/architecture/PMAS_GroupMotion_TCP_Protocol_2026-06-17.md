# PMAS Group Motion TCP Protocol Notes - 2026-06-17

이 문서는 `packet_capture/MoveLinearAbsoluteEx.pcapng`와
`packet_capture/GroupReadStatus.pcapng`를 기준으로 `Elmo_EtherCAT_Test_4Axis`
LASAL 코드에 구현할 TCP 프레임 구조를 정리한 것이다.

## 기준 구현

- 기존 단축 이동: `TCPMotionInterface.Response`가 수신 버퍼를 복사하고
  `CommandID`로 분기한 뒤 `MoveAbs`에서 ACK를 송신하고
  `LMCAxis.MoveAbsolute(...)`를 호출한다.
- 기존 위치 읽기: `ReadActPos`가 `LMCAxis.ReadPosition(...)` 값을
  24바이트 응답의 payload offset 8에 DINT로 기록한다.
- 그룹 구현도 동일하게 `Response -> MsgPaser -> 기능별 함수` 경로를 사용한다.

## MoveLinearAbsoluteEx

캡쳐에서 확인된 요청 프레임:

| Offset | Size | 값/의미 |
|---:|---:|---|
| 0 | 2 | Command ID = `0x20A4` |
| 2 | 2 | Group/Axes ref = `0x0000` |
| 4 | 2 | Payload length = `0x0130` = 304 |
| 6 | 2 | Header flag/version = `0x0100` |
| 8 | 128 | 위치 벡터 16개, IEEE754 little-endian double |
| 136 | 8 | Velocity, double |
| 144 | 8 | Acceleration, double |
| 152 | 8 | Deceleration, double |
| 160 | 8 | Jerk, double |
| 168 | 128 | Transition parameter vector 16개, double |
| 296 | 4 | Buffered mode, int32 |
| 300 | 4 | Coordinate system, int32 |
| 304 | 4 | Transition mode, int32 |
| 308 | 1 | Superimposed, byte |
| 309 | 1 | Execute, byte |
| 310 | 2 | Reserved |

LASAL `_LMCPROF_POS`는 `Pos1..Pos9` DINT 구조체이므로 16개 좌표 중
앞 9개만 사용한다. PMAS 캡쳐 값은 double이지만 SIGMATEK Motion 함수 입력은
DINT application unit이다. 따라서 수신 double을 반올림 없이 LASAL `TO_DINT`로
변환해 입력한다.

응답 프레임은 기존 `MoveAbs`와 같은 ACK 계열이다.

| Offset | Size | 값/의미 |
|---:|---:|---|
| 0 | 2 | `0x0000` |
| 2 | 2 | Payload length = 8 |
| 4 | 4 | Reserved = 0 |
| 8 | 4 | FB handle. 캡쳐값은 `0x0029F210` |
| 12 | 4 | Status/Error. LASAL RetCode를 기록 |

실제 이동 호출은 `LMCRobot.MoveLinearCoord(...)`를 사용한다. 이유는 캡쳐 프레임에
Coordinate system 필드가 포함되어 있고, 기존 Cycle Test Group1 UI도 MCS/ACS를
선택하도록 되어 있기 때문이다.

## GroupReadStatus

캡쳐에서 확인된 요청 프레임:

| Offset | Size | 값/의미 |
|---:|---:|---|
| 0 | 2 | Command ID = `0x2045` |
| 2 | 2 | Group/Axes ref = `0x0000` |
| 4 | 2 | Payload length = 8 |
| 6 | 2 | Header flag/version = `0x0100` |
| 8 | 4 | Handler/selector = `0x00000100` |
| 12 | 4 | Enable/Execute = 1 |

캡쳐에서 확인된 응답 프레임:

| Offset | Size | 값/의미 |
|---:|---:|---|
| 0 | 2 | `0x0000` |
| 2 | 2 | Payload length = 12 |
| 4 | 4 | Reserved = 0 |
| 8 | 4 | `ulState`, 캡쳐값 `0x40020000` |
| 12 | 2 | Status/Error |
| 14 | 2 | Error ID |
| 16 | 2 | Group Error ID |
| 18 | 2 | Reserved |

이번 캡쳐의 in-position 판정 비트는 `0x00020000`이다. LASAL 구현에서는
`LMCRobot.AxInPosition(AxisNo:=0, PositionWindow:=0)` 결과가 1이면
`ulState`에 이 비트를 세운다. 기본 high bit `0x40000000`은 캡쳐값과 같은
상태 범주를 유지하기 위해 포함한다.

## Transition/Blending 매핑

캡쳐로 확인 가능한 것은 프레임 위치와 값뿐이다. Elmo enum과 SIGMATEK enum의
의미 매핑은 다음 보수적 규칙을 사용한다.

- Elmo `NONE(0)` -> SIGMATEK `_LMCPROF_EXACT_STOP(0)`.
- Elmo `CORNER(1)` -> SIGMATEK `_LMCPROF_CONT_DIRECT(2)`.
- tolerance-sphere 계열 테스트가 필요하면 LASAL 상수 `_LMCPROF_SMOOTH_PARAB(3)`,
  `_LMCPROF_SMOOTH_CUBIC(4)`, `_LMCPROF_SMOOTH_QUINT(5)`를 별도 UI 값으로
  노출해야 한다. 현재 WPF enum에는 이 값들이 없다.

블렌딩 테스트는 WPF에서 `MC_BUFFERED` 또는 `MC_BLENDING_PREVIOUS`로 연속 명령을
queue하고, `TransitionMode`를 `CORNER` 이상으로 보낸 뒤 `GroupReadStatus`의
in-position 도달 시점과 timeout/drop 로그를 비교하는 방식이 맞다.

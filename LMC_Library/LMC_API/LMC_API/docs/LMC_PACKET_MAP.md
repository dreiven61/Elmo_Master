# LMC Packet Map

기준 캡처: `C:\Users\user\Desktop\Elmo_API_Packet2\TXT\`

모든 다중 바이트 값은 little-endian이다. 기본 요청 헤더는 8바이트다.

| Offset | Size | 의미 |
|---:|---:|---|
| 0 | 2 | Command ID |
| 2 | 2 | Axis reference |
| 4 | 2 | Payload length |
| 6 | 2 | Group reference/flags |

| LMC API | Command ID | 요청 크기 | 비고 |
|---|---:|---:|---|
| `LMC_RpcInitConnection` session | `0x8080` | 9 | 선택적 Maestro handshake |
| `LMC_RpcInitConnection` UDP register | `0x405C` | 20 | event mask, callback port, local IPv4 |
| `LMC_CloseConnection` | `0x405D` | 9 | 연결 종료 |
| Axis name lookup | `0x103C` | 88 | ASCII name 80 bytes |
| Group name lookup | `0x1042` | 88 | ASCII name 80 bytes |
| `LMC_PowerCmd` | `0x2023` | 16 | enable + buffered/execute bytes |
| `LMC_Reset` | `0x2024` | 9 | axis reset |
| `LMC_ReadStatusCmd` | `0x2028` | 16 | axis state/status response |
| `LMC_ReadActualPositionCmd` | `0x202E` | 9 | response double at offset 12 |
| `LMC_StopCmd` | `0x2022` | 24 | float deceleration/jerk |
| `LMC_MoveAbsoluteExCmd` | `0x209F` | 64 | five doubles + direction/buffer/execute/reserved |
| `LMC_MoveRelativeExCmd` | `0x20A0` | 64 | five doubles + direction/buffer/execute/reserved |
| `LMC_MoveVelocityExCmd` | `0x20A2` | 49 | direction은 Positive(1)/Negative(3)만 허용 |
| `LMC_GetGroupMembersInfo` | `0x20D2` | 9 | group reference at header offset 6 |
| `LMC_GroupReadStatusCmd` | `0x2045` | 16 | group reference at offset 6 |
| `LMC_GroupEnableCmd` | `0x2047` | 9 | group administrative command |
| `LMC_GroupDisableCmd` | `0x2048` | 9 | group administrative command |
| `LMC_GroupResetCmd` | `0x2049` | 9 | group administrative command |
| `LMC_GroupReadActualPosition` | `0x2051` | 16 | 16-axis vector response |
| `LMC_GroupStopCmd` | `0x2085` | 24 | float deceleration/jerk |
| `LMC_SetKinTransformCartesian4Axis` | `0x20E7` | 1328 | a01..a04 → X/Y/Z/U, coefficient 1/1/0 |
| `LMC_MoveLinearAbsoluteExCmd` | `0x20A4` | 312 | 16 positions, velocity/acc/dec/jerk, transition fields |

## 단위

이 문서는 기존 Elmo/PMAS 패킷 확인용이다. LASAL 전용 단위 변환 기준으로 사용하지 않는다.

LASAL 전용 DLL은 `LMC_Library/LMC_LASAL_API_Delivery`를 기준으로 한다. 해당 DLL은 LASAL `unit.h`에서 가져온 `LMC_Units` scale profile을 사용한다.

주의:

- PMAS count와 LASAL application unit을 고정 비율로 연결하지 않는다.
- LASAL 위치/속도/가속도/저크 변환은 command별 numeric type과 `unit.h` scale을 같이 봐야 한다.

## LASAL 대응

LASAL `TCPMotionInterface.Response`는 헤더/길이만 확인하고 복사한 뒤, 실제 `_LMCAxis`/group 실행은 `RtWork` 큐에서 처리해야 한다. `0x209F`, `0x20A0`, `0x20A2`, `0x20A4`, `0x20E7`의 offset은 이 문서와 `LmcProtocol.cs`를 기준으로 바이트 단위 대조한다.

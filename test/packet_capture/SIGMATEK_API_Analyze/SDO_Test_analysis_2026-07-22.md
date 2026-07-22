# SIGMATEK D5 SDO Ticket 시험 패킷 분석

- 분석일: 2026-07-22
- 원본: `SDO_Test.pcapng`
- SHA-256: `0C5C3983ACC0270B9E890A0968F40E11E31C3DA758F2E7D26E9DD63035233496`
- TCP flow: `10.10.150.13:9743 <-> 10.10.150.1:4000`
- 후속 재시험: `SDO_Test2.pcapng`의 Slave 4 happy path는
  `SDO_Test2_analysis_2026-07-22.md`에서 PASS로 판정
- 최종 happy-path 재시험: `SDO_Test_Slave123.pcapng`까지 합쳐 Slave 1~4 모두 PASS

## 결론

캡처된 first-slice request shape에서 PC request serializer와 PLC RPC parser는 정상
동작했다. Ticket 11은 요청된 1000 cycles를 기다리지 않고 Submit과 같은 RT cycle에
`Expired/TimedOut`으로 확정됐다. 이 캡처와 수정 전 gate-on source 순서를 함께 비교하면
실제 SDO Read start 전에 끝난 경로와 일치하지만, 배포 binary identity와 executor
미진입의 직접 증거는 PLC trace가 필요하다.

원인은 `LMCDiagnosticsService.HandleRequest`의 request-local 이름이 ticket-state class
member와 대소문자만 달랐던 캡처 당시 source의 LASAL shadowing 결함이다. 로컬 자기 대입
때문에 `SdoTimeoutCycles` class member가 0으로 남아 수정 전 gate-on source 순서의
`ProcessOperations` 첫 timeout 검사에서 종료된다.

## Wire 요약

| 구분 | 수량 | 결과 |
|---|---:|---|
| `0x7E00 GetDiagnosticsCapabilities` | 2 | `CapabilityBits=0x13F`, MaxSDO=4, BootId=4 |
| `0x7E50 SubmitSDO` | 1 | Ticket 11, SDORead, Queued |
| `0x7E03 GetOperationStatus` | 5 | 모두 동일한 Expired/TimedOut terminal status |

UI에서 Refresh를 6회 눌렀다는 사용자 기록과 달리 wire에는 RequestId 55..59의 5회만
있다. TCP retransmission, sequence gap, out-of-order와 checksum 오류는 없다. 누락된 한
번은 캡처 범위 밖이거나 application request로 전송되지 않은 것이다. 결과 판정에는
영향이 없다.

## Submit 요청과 응답

| 필드 | 값 |
|---|---:|
| SlaveReference | 1 |
| ObjectIndex/SubIndex | `0x1000:0` |
| ValueType/DataLength | UInt32=5 / 4 bytes |
| TimeoutCycles | 1000 |
| MapRevision | `0x957F101E` |
| DiagnosticsBootId | 4 |
| TicketId | 11 |
| Submit state | Queued |
| SubmitCycle | 1443742 |

Submit RPC RTT는 약 3.093 ms다.

## Status 응답

5개 응답은 echoed RequestId를 제외하면 동일하다.

```text
TicketId=11
Kind=SDORead
State=Expired
Outcome=TimedOut
SubmitCycle=1443742
CompletionCycle=1443742
OperationErrorId=0
DetailCode=0x05040000
ResultLength=0
DiagnosticsBootId=4
```

첫 status request는 Submit response 뒤 1.8547803 s에 전송됐다. 이후 poll request 간격은
1.4591213, 1.3052042, 1.2093388, 1.1349134 s이고 status RTT는 0.7161..2.5175 ms다.
PLC response 뒤 약 40..52 ms의 payload 없는 ACK는 일반 TCP delayed ACK이며 operation
응답이 아니다. common RPC 응답은 성공이므로 WPF의 `Refresh Diagnostics Operation
PASS`는 조회 RPC 성공만 뜻한다. SDO operation 결과는 실패다.

## Source 원인과 수정 계약

충돌한 request local은 다음 네 개다.

```text
sdoSlaveReference <-> SdoSlaveReference
sdoObjectIndex    <-> SdoObjectIndex
sdoSubIndex       <-> SdoSubIndex
sdoTimeoutCycles  <-> SdoTimeoutCycles
```

수정본은 local을 `requestSdoSlaveReference`, `requestSdoObjectIndex`,
`requestSdoSubIndex`, `requestSdoTimeoutCycles`로 변경하고 ticket-state class member에
명시적으로 복사한다. 정적 계약은 `LMCDiagnosticsService` class member와 implementation
FUNCTION의 모든 `VAR*` 선언을 case-insensitive 비교해 충돌을 거부한다. 현재 수정본의
source-only/full 정적 계약과 PC 자동 시험 103/103은 통과했다.

## 재시험 판정

- 축 1부터 `Submit -> Refresh Ticket terminal` 한 흐름을 완료한다.
- 정상은 `Completed/Success`, ErrorId/DetailCode 0, UInt32, ResultLength 4와 inline
  4-byte data다.
- 1000-cycle 정상 요청은 Submit과 같은 cycle에 Expired가 되면 안 된다.
- 별도 유도 timeout은 unsigned `CompletionCycle - SubmitCycle >= 1000`이어야 한다.
- 이 캡처에는 EtherCAT `0x88A4` frame이 없으므로 실제 mailbox 성공 판정에는 executor
  callback/PLC trace 또는 별도 EtherCAT 관측 증거도 필요하다.

# SIGMATEK D5 SDO Slave 1-3 완료 패킷 분석

- 분석일: 2026-07-22
- 원본: `SDO_Test_Slave123.pcapng`
- SHA-256: `FF9CE905EE716DCDFC87205B99D9423EBD3E17258ECE6CD73026A2FC604FA49A`
- Diagnostics TCP flow: `10.10.150.13:9922 <-> 10.10.150.1:4000`
- 비교 대상: `SDO_Test2.pcapng`의 Slave 4 PASS

## 결론

Slave 1, 2, 3의 first-slice SDO Read happy path는 모두 정상이다. 각 축에 대해
`0x1000:0`, UInt32, 4-byte, timeout 1000 cycles 요청을 제출했고 모두 Queued ticket을
거쳐 `Completed/Success`와 동일한 결과 `92 01 02 00`을 반환했다. little-endian
UInt32로 `0x00020192`, decimal `131474`다.

앞선 `SDO_Test2.pcapng`의 Slave 4 PASS와 합치면 물리축 1~4의 현재 허용 vector에 대한
D5 first-slice SDO Read-only 구현과 happy-path runtime은 완료로 판정한다. PI/SDO Write,
8/12-byte 또는 extended result는 이 완료 범위가 아니며 계속 capability-off다. 실제
fault/cancel/orphan 동작은 production qualification 항목으로 남긴다.

## Wire 요약

| Slave | Ticket | SubmitCycle | CompletionCycle | Delta | terminal | 결과 |
|---:|---:|---:|---:|---:|---|---|
| 1 | 6 | 987464 | 987507 | 43 cycles | Completed/Success | `92 01 02 00` |
| 2 | 7 | 990944 | 990995 | 51 cycles | Completed/Success | `92 01 02 00` |
| 3 | 8 | 993897 | 993940 | 43 cycles | Completed/Success | `92 01 02 00` |

세 요청 모두 다음 조건을 만족한다.

- Operation/transport ErrorId 0, OperationDetail 0
- ResultType UInt32, ResultLength 4, inline data length 4
- DiagnosticsBootId 5
- Submit response Queued, GetOperationStatus 조회 1회에서 terminal Completed
- unsigned cycle delta가 timeout 1000 cycles보다 작음

Capability 조회 4회도 모두 `CapabilityBits=0x0000013F`,
`MapRevision=0x957F101E`, `MaxSdoDataBytes=4`, `DiagnosticsBootId=5`로 일치했다.

## 4축 합산 판정

| Slave | 증거 파일 | Delta | 결과 |
|---:|---|---:|---|
| 1 | `SDO_Test_Slave123.pcapng` | 43 cycles | PASS |
| 2 | `SDO_Test_Slave123.pcapng` | 51 cycles | PASS |
| 3 | `SDO_Test_Slave123.pcapng` | 43 cycles | PASS |
| 4 | `SDO_Test2.pcapng` | 54 cycles | PASS |

Ticket 5, 6, 7, 8과 BootId 5가 두 capture에서 연속되고 네 축 결과도 동일하다. 수정 전
`SDO_Test.pcapng`의 same-cycle Expired/TimedOut 결함은 어느 축에서도 재현되지 않았다.

## TCP와 증명 경계

Diagnostics capture는 기존 단일 TCP 연결의 일부다. retransmission, lost segment,
out-of-order, duplicate ACK, RST와 expert warning/error는 없다. SYN/FIN은 캡처 범위 밖이라
연결 수명 전체는 판단하지 않는다.

이 capture에도 EtherCAT ethertype `0x88A4` frame은 없다. 따라서 4축 각각의
PC-PLC ticket, derived executor callback 반영과 inline result happy path는 확인했지만
mailbox frame 자체를 독립 관측한 자료는 아니다.

## 남은 production qualification

다음 항목은 구현 범위를 확대하는 작업이 아니라 현재 Read-only 기능의 fault qualification이다.

1. BUSY와 bounded retry
2. offline/immediate start error와 실제 SDO abort
3. actual-length mismatch와 timeout 전후 경계
4. queued cancel, running cancel 거부
5. disconnect/orphan/late callback과 stale session/BootId/Ticket
6. allowlist 밖 object, malformed request, 8/12-byte Read와 Write의 fail-closed

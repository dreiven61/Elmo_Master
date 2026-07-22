# SIGMATEK D5 SDO 수정본 재시험 패킷 분석

- 분석일: 2026-07-22
- 원본: `SDO_Test2.pcapng`
- SHA-256: `39E99C7FBB88CE283444B0959FCCDBA922F0C0D2515D73F551BFC0F301FA970C`
- Diagnostics TCP flow: `10.10.150.13:10461 <-> 10.10.150.1:4000`
- 비교 대상: `SDO_Test.pcapng`
- 후속 재시험: `SDO_Test_Slave123.pcapng`에서 Slave 1~3도 PASS해 4축 happy path 완료

## 결론

request-local shadowing 수정본의 first-slice SDO Read는 Slave 4 실기에서 정상
동작했다. `0x1000:0`, UInt32, 4-byte 요청은 Ticket 5로 Queued 된 뒤 54 PLC cycle
후 `Completed/Success`가 됐고 결과 `92 01 02 00`을 반환했다. little-endian UInt32로
`0x00020192`, decimal `131474`다.

수정 전 캡처의 `SubmitCycle=CompletionCycle`, `Expired/TimedOut` 결함은 재현되지
않았다. 이 캡처는 Slave 4 happy path를 통과시킨 증거다. 후속
`SDO_Test_Slave123.pcapng`에서 Slave 1~3도 통과했다. abort, offline, cancel, 실제
timeout 및 orphan 경로까지 통과했다는 증거는 아니다.

## Wire 요약

| 구분 | 수량 | 결과 |
|---|---:|---|
| `0x7E00 GetDiagnosticsCapabilities` | 3 | 매번 `CapabilityBits=0x13F`, MaxSDO=4, BootId=5 |
| `0x7E10 ReadEtherCATHealth` | 1 | RPC 성공, 4축 Online/OP, AL code와 AxisError 0 |
| `0x7E50 SubmitSDO` | 1 | Ticket 5, SDORead, Queued |
| `0x7E03 GetOperationStatus` | 3 | 모두 동일한 Completed/Success terminal status |

Diagnostics TCP flow에는 retransmission, lost segment, out-of-order, duplicate ACK,
RST나 FIN 이상이 없다. 별도 TCP port 1954 flow는 LASAL IDE 통신이며 diagnostics
판정에 포함하지 않는다.

## Capability와 요청

| 필드 | 값 |
|---|---:|
| CapabilityBits | `0x0000013F` |
| MapRevision | `0x957F101E` |
| BaseCycleTimeUs | 1000 |
| MaxSdoDataBytes | 4 |
| DiagnosticsBootId | 5 |
| SlaveReference | 4 |
| ObjectIndex/SubIndex | `0x1000:0` |
| ValueType/DataLength | UInt32=5 / 4 bytes |
| TimeoutCycles | 1000 |

BootId가 이전 캡처의 4에서 5로 바뀌었고 동작도 수정 계약과 일치한다. 이는 재부팅 후
새 PLC download가 실행됐다는 강한 정황이지만 pcap 자체가 배포 binary hash나 IDE
build log를 증명하지는 않는다.

## Ticket와 결과

```text
TicketId=5
Kind=SDORead
SubmitState=Queued
SubmitCycle=92042

TerminalState=Completed
Outcome=Success
CompletionCycle=92096
OperationErrorId=0
OperationDetail=0x00000000
ResultType=UInt32
ResultLength=4
ResultData=92 01 02 00
DiagnosticsBootId=5
```

unsigned cycle delta는 `92096 - 92042 = 54`다. Base cycle 1000 us 기준 약 54 ms이며
요청 timeout 1000 cycles보다 충분히 짧다. Submit RPC RTT는 약 8.710 ms이고 status
RPC RTT는 약 0.670, 0.785, 1.580 ms다.

첫 status request는 Submit response 약 1.818 s 뒤에 전송됐다. 따라서 wire에는
Running 중간 상태가 없지만 첫 조회에서 이미 terminal Completed인 것은 정상 계약이다.
세 status 응답은 모두 동일한 결과이므로 terminal status 조회도 멱등이다.

## 수정 전후 비교

| 항목 | 수정 전 `SDO_Test` | 수정 후 `SDO_Test2` |
|---|---:|---:|
| Slave | 1 | 4 |
| BootId | 4 | 5 |
| Ticket | 11 | 5 |
| SubmitCycle | 1443742 | 92042 |
| CompletionCycle | 1443742 | 92096 |
| Cycle delta | 0 | 54 |
| terminal | Expired/TimedOut | Completed/Success |
| Error/Detail | `0 / 0x05040000` | `0 / 0x00000000` |
| 결과 | 없음 | UInt32 4 bytes, `0x00020192` |

## 증명 범위와 남은 시험

이 capture에는 EtherCAT ethertype `0x88A4` frame이 없다. 따라서 PC-PLC diagnostics
wire, PLC derived executor의 성공 callback 반영과 inline 결과 반환은 확인되지만 실제
mailbox frame을 독립적으로 관측한 자료는 아니다.

후속 `SDO_Test_Slave123.pcapng`까지 합쳐 Slave 1~4 happy path는 완료됐다. 다음 항목은
계속 별도 시험이 필요하다.

1. offline/start error와 실제 SDO abort
2. 요청 timeout이 지난 뒤의 Expired와 unsigned cycle delta
3. queued cancel, running cancel 거부와 disconnect/orphan drain
4. allowlist 밖 object, 8/12-byte Read와 Write 거부

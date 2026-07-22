# SIGMATEK D5 general-inline SDO ResourceBusy 실패 패킷 분석

- 분석일: 2026-07-22
- 원본: `SDO_Test_Error.pcapng`
- SHA-256: `79E4540D21CA353C38D031F2A7D34DCFD0E4D4C44B4179025E3ABC774D81BFA5`
- Diagnostics TCP flow: `10.10.150.13:3821 <-> 10.10.150.1:4000`
- 비교 대상: `SDO_Test2.pcapng`, `SDO_Test_Slave123.pcapng`의 legacy
  `0x1000:0` UInt32/4-byte PASS

## 결론

BootId 6 PLC는 bit 8 `SDORead`, bit 13 `SDOReadGeneralInline`과
`MaxSdoDataBytes=4`를 정상 광고했다. PC도 general-inline request를 정확한 32-byte
shape로 전송했다. 그러나 캡처된 두 `SubmitSDO`는 ticket을 만들기 전에 모두
`ErrorId=-32000`, `DetailCode=9 ResourceBusy`로 거부됐다. 따라서 이 캡처는
general-inline SDO mailbox 실행이나 typed 결과의 성공/실패를 증명하지 않는다.

첫 요청은 사용자가 의도한 `0x6061:0 Int8/1`이 아니라 wire에서
`0x6061:0 UInt16/2`로 확인됐다. 두 번째 요청은 `Int8/1`이 맞지만 이미 Busy gate에서
거부됐다. `0x6041:0`과 `0x1018:1` 요청은 이 파일에 없다.

실패 당시 source와 비교하면 owned callback validation failure가 executor를
`Quarantined`로 만들 수 있는데, 기존 `CopyCompletion`은 `ResultReady`만 `Idle`로
회수하고 `Quarantined`는 회수하지 않았다. 또한 vendor request를 공개한 뒤
`Arming -> Running`으로 바꾸는 순서에는 짧은 callback과의 경쟁 가능성이 있었다.
이 두 source 결함은 확인됐지만, 캡처에는 최초 accepted ticket과 callback이 없으므로
이번 Busy를 만든 최초 callback 조건은 확정할 수 없다.

## Wire 요약

| 구분 | 수량 | 결과 |
|---|---:|---|
| `0x7E00 GetDiagnosticsCapabilities` | 3 | 모두 `0x213F`, MaxSDO=4, BootId=6 |
| `0x7E50 SubmitSDO` | 2 | 모두 ticket 할당 전 `ResourceBusy(9)` |
| `0x7E03 GetOperationStatus` | 0 | ticket이 없어 조회되지 않음 |

Diagnostics flow에는 payload retransmission이 없다. 별도 port 1954 flow는 LASAL IDE
통신이며 D5 판정에 포함하지 않는다. 캡처에는 EtherCAT ethertype `0x88A4` frame도 없다.

## Capability

RequestId 12, 13, 15의 capability 응답은 모두 같다.

```text
DiagnosticsBuild=1
CapabilityBits=0x0000213F
MapRevision=0x957F101E
MaxSdoDataBytes=4
DiagnosticsBootId=6
CommandStatus=0
ErrorId=0
DetailCode=0
```

이 값은 general-inline test profile이 광고됐음을 증명한다. BootId가 앞선 legacy PASS
캡처의 5에서 6으로 바뀐 것은 재부팅 또는 새 download의 강한 정황이지만, pcap은 PLC
binary hash나 정확한 source revision을 증명하지 않는다.

## Submit 요청과 응답

| Request frame | Response frame | RequestId | Slave | Object | ValueType/DataLength | Timeout | RTT | 응답 |
|---:|---:|---:|---:|---|---|---:|---:|---|
| 201 | 202 | 14 | 1 | `0x6061:0` | UInt16=3 / 2 bytes | 1000 | 약 1.680 ms | `-32000 / ResourceBusy(9)` |
| 468 | 469 | 16 | 1 | `0x6061:0` | Int8=9 / 1 byte | 1000 | 약 2.250 ms | `-32000 / ResourceBusy(9)` |

두 요청 모두 다음 identity를 사용했다.

```text
SchemaVersion=1
OperationFlags=Read
MapRevision=0x957F101E
DiagnosticsBootId=6
Reserved=0
```

응답은 16-byte common domain-error payload이며 ticket identity를 포함하지 않는다.
C# SDK가 이를 `LMCDiagnosticsCommandException`으로 변환하는 것은 wire 응답과 일치한다.
두 번째 Submit은 첫 번째보다 약 10.33초 뒤였지만 같은 Busy 응답을 받았다.

## 확인된 source 결함과 캡처 추론 경계

### 확인된 source 결함

실패 당시 source에는 다음 복구 결함이 있었다.

1. callback metadata 또는 actual length validation이 실패하면 executor는 결과를
   `Quarantined` 상태로 publish했다.
2. `CopyCompletion`은 `Quarantined` 결과도 읽었지만 `ResultReady` 상태만 `Idle`로
   전환했다.
3. 따라서 service가 실패 결과를 소비해도 같은 executor의 `IsReusable()`은 계속
   false가 될 수 있었다.
4. `TryStartRead`가 `StartReadSDO` 호출 뒤에야 `Running`을 publish해, vendor cyclic
   task가 매우 빨리 callback하면 callback이 `Arming`을 관측할 가능성이 있었다.

현재 수정 source는 vendor call 전에 `Running`을 publish하고, owned validation failure를
`ResultReady`로 publish한 뒤 `Releasing` 상태를 거쳐 `Idle`로 회수한다. orphan callback도
private buffer 소유가 끝난 시점에 `Releasing -> Idle`로 회수한다. active token이 없는
unsolicited/duplicate callback과 atomic ownership 실패만 hard quarantine한다.

### pcap으로 확정할 수 없는 내용

`ResourceBusy(9)`는 service의 active/draining slot gate와 선택된 executor의
`IsReusable=false` gate에서 모두 반환될 수 있다. 이 응답에는 어느 조건인지 구분하는
필드가 없다. 또한 최초 accepted request, callback validation code와 executor state가
캡처되지 않았다. 따라서 아래 항목은 source 구조에 근거한 추정이며 wire 사실이 아니다.

- 최초 general-inline callback이 `Arming`을 관측했을 가능성
- object 실제 길이와 요청 길이 불일치가 최초 quarantine을 만들었을 가능성
- 캡처 시작 전 operation의 drain 또는 quarantine이 두 Busy 응답을 만든 정확한 경로

## 재시험 판정

수정본은 PLC 재시작 또는 download로 기존 executor state를 초기화한 뒤 시험한다.
같은 BootId와 같은 Slave 1에서 아래 순서를 재부팅 없이 연속 수행한다.

1. `0x6061:0`, Int8, 1 byte
2. `0x6041:0`, BitField16, 2 bytes
3. `0x1018:1`, UInt32, 4 bytes
4. 다시 `0x6061:0`, Int8, 1 byte

각 정상 요청은 Submit에서 Queued ticket을 반환하고 terminal
`Completed/Success`, ErrorId/DetailCode 0, 요청과 같은 ResultType/ResultLength를
반환해야 한다. 한 요청의 완료 또는 실패 뒤 다음 요청이 재부팅 없이 진행돼야 한다.

복구 회귀 시험으로 `0x6061:0`, UInt16, 2 bytes처럼 drive object의 실제 길이와 다른
shape도 별도로 실행한다. 이 요청이 abort 또는 actual-length mismatch로 terminal
Failed가 되더라도, 이어지는 올바른 Int8/1 요청은 영구 `ResourceBusy`가 되면 안 된다.
실제 active/draining operation 동안의 일시적인 Busy는 정상 계약이므로 terminal 상태,
executor callback/PLC trace와 함께 판정한다.


더 중요한 점은 현재 Git 소스에 앞에서 확인한 shadowing 결함이 아직 남아 있다는 것입니다.

[문제 대입부](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1734):

```st
SdoSlaveReference := sdoSlaveReference;
SdoObjectIndex := sdoObjectIndex;
SdoSubIndex := sdoSubIndex;
SdoTimeoutCycles := sdoTimeoutCycles;
```

따라서 이 소스를 빌드·다운로드한 PLC라면 첫 번째 `Refresh Ticket`에서 다시 `Expired/TimedOut`이 될 가능성이 확정적으로 높습니다. 각 Refresh 후 새 Submit이 가능했던 것도 이전 ticket이 terminal이 됐다는 뜻일 뿐, `Completed`인지 `Expired`인지는 알 수 없습니다.

판정은 다음과 같습니다.

- 축 1~4 Submit/티켓 발급: 정상
- 실제 EtherCAT SDO 실행 및 값 반환: 증거 없음
- 현재 Git 소스 기준 전체 동작: 아직 결함 있음

Ticket 8에서 새 Submit을 하지 말고 `Refresh Ticket`을 눌러 terminal summary를 확인해야 합니다. `Completed/Success/4 bytes`면 실제 성공이고, `Expired/TimedOut/0x05040000`이면 앞서 확인한 shadowing 결함입니다.

> 패킷 캡쳐해왔으니 분석해봐
>
> User context
>
> Attachments:
> - Submit_SDO_Read.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Submit_SDO_Read.pcapng`

<details><summary>2 previous messages</summary>

> 캡처에서 `SubmitSDO(0x7E50)` 요청, 티켓 응답, `GetOperationStatus(0x7E03)` 반복과 최종 상태를 시간순으로 복원하겠습니다. 동시에 C# serializer와 PLC byte offset을 대조해서, 앞서 의심한 shadowing timeout이 실제 wire에서도 확인되는지 보겠습니다.
>
> 초기 디코딩 결과, 캡처에는 축 4의 capability 조회와 `SubmitSDO` 한 건만 있습니다. 요청은 정확히 32-byte이고 PLC가 약 1.85 ms 뒤 `TicketId=9, State=Queued`를 반환했습니다. 다만 `GetOperationStatus(0x7E03)` 패킷이 없어서 실제 완료/timeout 결과는 이 파일만으로는 확인할 수 없습니다.
</details>

결론부터 말하면, PC→PLC의 SDO Submit 경로는 정상입니다. 하지만 실제 EtherCAT SDO Read 완료는 이 캡처에 없습니다.

[Submit_SDO_Read.pcapng](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Submit_SDO_Read.pcapng)

- SHA-256: `7E07EB6520C1BD21009891CBCD729AF35DC0131DFD02530F4975133634BA0756`
- 총 7패킷
- TCP 데이터 패킷 4개
- 흐름: `10.10.150.13:9743 ↔ 10.10.150.1:4000`
- 재전송, 순서 오류, 유실 징후 없음

패킷 순서는 다음과 같습니다.

| 순서 | 내용 | 응답 시간 |
|---|---|---:|
| 1 | `0x7E00 GetCapabilities`, RequestId 44 | |
| 2 | Capability 성공 응답 | 0.808 ms |
| 3 | `0x7E50 SubmitSDO`, RequestId 45 | |
| 4 | Ticket Queued 응답 | 1.853 ms |

Capability 응답도 정상입니다.

```text
CapabilityBits       = 0x0000013F
MapRevision          = 0x957F101E
MaxSdoDataBytes      = 4
DiagnosticsBootId    = 4
BaseCycleTimeUs      = 1000
```

Submit 요청의 wire 값도 정확합니다.

| 필드 | 캡처 값 |
|---|---:|
| SlaveReference | `4` |
| OperationFlags | `0` = Read |
| ObjectIndex | `0x1000` |
| SubIndex | `0` |
| ValueType | `5` = UInt32 |
| TimeoutCycles | `1000` |
| DataLength | `4` |
| MapRevision | `0x957F101E` |
| BootId | `4` |

PLC 응답:

```text
RequestId       = 45
DetailCode      = 0
TicketId        = 9
OperationKind   = 2 (SDORead)
OperationState  = 1 (Queued)
QueuedCycle     = 702540
BootId          = 4
```

따라서 다음은 확인됐습니다.

- WPF/C# serializer 정상
- TCP 전송 정상
- PLC의 `0x7E50` 파싱·정책 검증 정상
- Slave 4 executor 연결 확인 정상
- Ticket 생성 정상

확인되지 않은 것은 다음입니다.

- `Queued → Running`
- EtherCAT mailbox 요청 발생
- 드라이브의 `0x1000:0` 응답
- `Completed/Success` 또는 `Expired/TimedOut`
- 실제 반환된 4-byte 데이터
- 축 1~3 결과

캡처에 `0x7E03 GetOperationStatus`가 한 건도 없습니다. 마지막 TCP ACK는 PLC 응답 39.7 ms 뒤의 일반 delayed ACK이며 SDO 완료 응답이 아닙니다. EtherCAT EtherType `0x88A4` 프레임도 0개라 실제 CoE mailbox 구간은 볼 수 없습니다.

또한 이 파일은 축 1~4 전체가 아니라 축 4의 `TicketId=9` 한 건만 포함합니다. 앞선 화면의 Ticket 5~8보다 나중에 수행한 요청입니다.

앞서 확인한 [shadowing 문제](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1734)는 Ticket 발급 이후 실행 단계에 영향을 줍니다. 이번 캡처는 그 직전인 `Queued`에서 끝났기 때문에 해당 결함을 확인하거나 반박하지 못합니다.

다음 캡처에는 반드시 아래 과정이 들어가야 합니다.

```text
Submit SDO Read
→ Refresh Ticket 반복
→ State=Completed/Failed/Expired terminal 응답
```

성공 판정은 `0x7E03` 응답에서 다음 값이 보여야 합니다.

```text
State=Completed
Outcome=Success
ErrorId=0
Detail=0x00000000
ResultType=UInt32
ResultLength=4
Data=<4 bytes>
```

> 내가 어떤걸 테스트하면되냐?

<details><summary>2 previous messages</summary>

> 지금은 축을 바꿔가며 Submit만 반복할 단계가 아닙니다. 한 축씩 `Submit → Refresh Ticket을 terminal까지 반복 → 결과 저장`해야 하며, 현재 shadowing 결함이 남아 있으므로 먼저 “즉시 Expired가 재현되는지”를 확인하고 그다음 수정본으로 정상·실패·취소 시험을 나누는 게 맞습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
</details>

지금은 축을 바꿔가며 `Submit`만 반복하면 안 됩니다. **한 티켓을 terminal 상태까지 추적하는 시험**이 먼저입니다.

### 현재 빌드에서 할 시험

1. Wireshark 캡처 시작
   필터: `tcp.port == 4000 && tcp.len > 0`

2. `Refresh Capabilities`
   - `SDORead` 지원 확인
   - `MaxSDO=4 bytes`
   - `BootId`가 0이 아닌지 확인

3. Slave 1만 설정
   - Object: `0x1000`
   - Sub-index: `0`
   - Type: `UInt32`
   - Length: `4`
   - Timeout: `1000`

4. `Submit SDO Read`를 한 번만 누릅니다.

5. 같은 TicketId에 대해 `Refresh Ticket`을 100~200ms 간격으로 반복합니다.
   - `Queued → Running → Completed`
   - 또는 `Expired/Failed/Cancelled`
   중 하나가 나올 때까지 새 Submit을 하면 안 됩니다.

6. 결과 판정
   - 정상:
     - `State=Completed`
     - `Outcome=Success`
     - `ErrorId=0`
     - `DetailCode=0`
     - `ResultType=UInt32`
     - `ResultLength=4`
   - 완료되면 `Download Result` → `Save Result`
   - 현재 결함 재현:
     - `State=Expired`
     - `Outcome=TimedOut`
     - `SubmitCycle == CompletionCycle`
     - `DetailCode=0x05040000`

현재 소스의 [LMCDiagnosticsService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1734)에 대소문자만 다른 로컬 변수/멤버 변수 shadowing 결함이 남아 있습니다. 따라서 위와 같이 **동일 사이클에 즉시 Expired**가 나오면 축 2~4 시험은 중단해도 됩니다. 모든 축이 같은 원인으로 실패할 가능성이 큽니다.

### 수정본 적용 후 최소 시험

- Slave 1, 2, 3, 4 각각 `0x1000:0` 읽기
- 각 축마다 반드시 `Submit → Refresh terminal → Download Result`
- 동일 축에서 10회 반복하여 TicketId 증가와 결과 안정성 확인
- Timeout을 짧게 설정한 타임아웃 시험
- Submit 직후 `Cancel Ticket` 시험
- 작업 중 두 번째 Submit을 보내 Busy/중복 방지 확인
- 재접속 후 이전 TicketId 조회 시 stale ticket 처리 확인

지금 보내주실 자료는 다음 세 가지면 충분합니다.

- terminal 상태가 나온 화면
- `Copy Log` 내용
- Submit부터 마지막 `Refresh Ticket`까지 포함한 pcapng

테스트 기준 전체판은 [LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md)에 정리되어 있습니다.

> Refresh Capabilities 명령이 없는데?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-4cffb943-bed7-453e-afb7-027b54ff96cb.png`

맞습니다. 제가 탭 위치를 잘못 안내했습니다.

`Refresh Capabilities`는 **SDO / Write Policy 탭이 아니라 EtherCAT / PI 탭**에 있습니다. 다만 연결할 때 Capability 조회가 이미 자동 실행되고 SDO 버튼이 활성화됐으므로, 지금 시험에서는 생략해도 됩니다.

현재 화면에서는:

1. `TicketId=9`, `State=Queued` 상태이므로 새로 Submit하지 마십시오.
2. `Refresh Ticket`을 누릅니다.
3. `Completed`, `Expired`, `Failed`, `Cancelled` 중 하나가 나올 때까지 반복합니다.
4. 결과가:
   - `Completed`이면 `Download Result` 실행
   - 즉시 `Expired`이고 `SubmitCycle == CompletionCycle`이면 화면과 로그를 저장하고 시험 중단

즉, 지금 누를 버튼은 **Refresh Capabilities가 아니라 Refresh Ticket**입니다.

> Refresh Ticket 6번 실행함
>
> User context
>
> Attachments:
> - SDO_Test.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test.pcapng`

<details><summary>2 previous messages</summary>

> 새 캡처에서 6회의 `Refresh Ticket` 요청·응답을 분리해 Ticket 9가 실제로 어떤 상태 전이를 했는지 확인하겠습니다. TCP 응답 내용과 현재 PLC/C# 파서를 함께 대조하겠습니다.
>
> 캡처에는 실제로 `Refresh Ticket` 응답이 들어왔습니다. 우선 확인된 핵심은 Ticket 11이 첫 조회부터 이미 동일한 terminal 상태를 반환하며, 이후 조회에서도 값이 전혀 변하지 않는다는 점입니다. 필드 의미와 원인을 소스에 맞춰 확정하겠습니다.
</details>

결론부터 말하면 **테스트는 충분하며, PLC 코드 결함이 확정됐습니다. 더 Refresh할 필요 없습니다.**

캡처에 기록된 결과:

- `Submit SDO Read`: Slave 1, `0x1000:0`, UInt32, 4 bytes, Timeout 1000 cycles
- Ticket: `11`
- Submit 응답: `Queued`

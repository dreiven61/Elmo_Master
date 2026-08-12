> 7개 생산 파일은 manifest에 봉인된 물리 SHA와 canonical Git blob OID가 모두 정확히 일치했습니다. 현재 stage는 정확히 그 7개와 새 manifest, 총 8개뿐이며 trust-anchor나 다른 문서는 포함되지 않았습니다. cached whitespace 검사도 통과했습니다. 지금 이 8개를 하나의 원자 커밋으로 고정합니다.
>
> 생산 커밋 `5543579`가 정확히 7개 생산 파일 + manifest 1개로 생성됐습니다. 이제 커밋 트리의 7개 blob OID를 manifest와 다시 대조하고, 해당 8개 경로가 HEAD와 clean인지 확인한 뒤 마지막 focused current 검증을 실행합니다.
>
> 커밋 트리의 7개 blob이 manifest의 canonical OID와 모두 일치하고, 생산 8경로·trust-anchor는 HEAD와 clean입니다. 마지막으로 committed production tree에 focused `VerifyCurrent`를 실행합니다. 이 결과가 성공하면 **런타임 검증용 다운로드 1회**를 승인할 수 있습니다.
</details>

네, 지금 **런타임 검증용 Download 1회**를 진행해도 됩니다.

완료된 게이트:

- trust-anchor 커밋: `bb5fd93`
- 생산 7파일 + manifest 커밋: `5543579`
- trusted ValidateOnly: PASS
- sequence-4 Capture: PASS
- post-commit focused 검증: exit 0
- manifest SHA-256: `5CD3EC2C…9944A4`

다운로드 절차:

1. 전체 축·그룹·로봇 정지 및 Power Off
2. WPF 연결 해제
3. LASAL 프로젝트를 열되 수정·저장·리빌드하지 않음
4. `Download Project` 정확히 1회
5. 다운로드 과정에 포함된 load/start만 수행
6. 별도 Reset/Restart나 두 번째 Download는 하지 않음
7. 실패·중단되면 재시도하지 말고 그대로 알려주기

완료 후 다음만 알려주세요.

- `Download OK` 여부
- `Project successfully loaded` 여부
- 오류/경고 유무
- 새 `BootId`

그다음 `0x6061:0` SDO Read부터 런타임 테스트를 진행합니다. 현재는 테스트 후보이며, 런타임 증거가 확보되기 전까지 `ProductionApproved=false`입니다.

> 다운로드 진행해?

<details><summary>2 previous messages</summary>

> 다운로드는 사용자 쪽에서 진행하도록 두고, 저는 생산 7파일을 건드리지 않은 채 남아 있는 검증 도구·빌드 증거·문서 변경을 목적별로 정리하겠습니다. 먼저 현재 dirty 파일을 다시 분류하고, unrelated 사용자 파일은 모두 제외한 뒤 지원 도구와 문서만 별도 검증·커밋하겠습니다.
>
> 다운로드 진행해?
</details>

네, **지금 다운로드 진행하세요.**

- 축·그룹·로봇 정지 및 Power Off
- WPF 연결 해제
- 추가 Save/Build/Rebuild 없이 `Download Project` 1회
- 별도 Reset/Restart 금지
- 실패·중단되면 재시도하지 말고 알려주세요

완료 후 `Download OK`, `Project successfully loaded`, 오류/경고, 새 `BootId`를 알려주세요.

> 다운로드하는데 바뀐게 하나도 없다고 하는데? 근데 왜 다운로드 하지 말라고 한거임?

<details><summary>7 previous messages</summary>

> 다운로드 결과를 기다리는 동안 생산 커밋은 그대로 고정하고, 남은 지원 도구와 문서 변경의 커밋 경계를 계속 정리하겠습니다. LASAL 대상 7파일은 더 이상 수정하지 않습니다.
>
> 지원 도구는 현재 생산 소스와 분리돼 있습니다. 다운로드와 충돌하지 않는 범위에서 PowerShell 5/7 구문 검사, ASCII/EOL, 경로 의존성만 먼저 확인하고 있습니다. 빌드·LASAL·PLC에는 어떤 명령도 보내지 않습니다.
>
> 다운로드하는데 바뀐게 하나도 없다고 하는데? 근데 왜 다운로드 하지 말라고 한거임?
>
> 맞습니다. 제가 “다운로드 필요성”과 “다운로드 허용 시점”을 분리해서 설명하지 못했습니다.
>
> 지금 메시지는 **PLC에 이미 현재 실행 이미지와 같은 바이너리가 들어 있어 새로 전송할 내용이 없다는 뜻일 가능성이 큽니다.** 강제 다운로드·리빌드·Reset은 하지 마세요. 최근 LASAL 로그에서 실제 판정 문구와 이전 다운로드 이력을 바로 확인해 정확히 설명하겠습니다.
>
> 로그를 확인하니 중요한 사실이 나왔습니다. 화면의 “변경 없음” 표시와 달리 실제로는 방금 다운로드가 수행됐습니다.
>
> - 16:51:18 — `Download 282 files`
> - 16:51:19 — `Download Ok`
> - 16:51:26 — `Project successfully loaded`
>
> 따라서 **다시 다운로드하지 마세요.** 지금 다운로드 세션에 오류·중단·별도 Reset/Restart가 없었는지만 PID 34656 범위로 추가 확인하겠습니다.
>
> 추가로 로그에는 다운로드 직전 `Rebuild project`와 자동 `Save project`가 1회 기록돼 있습니다. 제가 요청한 절차와는 달랐기 때문에, 방금 다운로드된 바이너리가 봉인된 커밋과 동일한지 다시 확인해야 합니다.
>
> 지금은 다운로드 자체를 반복하지 말고 그대로 두세요. 생산 7파일의 현재 해시를 커밋 `5543579` 및 manifest와 읽기 전용으로 대조하겠습니다.
>
> 이제 테스트 해야 될 내용들에 대해 알려줘
</details>

현재는 아래 순서로 테스트하세요. **축 모션 테스트는 하지 않고 SDO Read만 사용합니다.**

### 1. 시험 준비

- LASAL은 Online 상태 유지
- 추가 Save/Rebuild/Download/Reset/Restart 금지
- 모든 축·그룹 Power Off/정지
- WPF는 아직 연결하지 않음
- Wireshark 필터:
  `ip.addr==<PLC_IP> && (tcp.port==4000 || udp.port==5000)`

LASAL Watch에서 시작값을 기록하세요.

- `SessionEpoch`
- `PendingClosedSessionEpoch`
- `RpcInitialized`
- `RpcCallbackBootId`
- `RpcCallbackSessionEpoch`
- `D5TerminalWakeAttemptCount`
- `D5TerminalWakeEnqueuedCount`
- `D5TerminalWakeRejectedCount`
- `D5TerminalWakeLastAttemptTicketId`
- `D5TerminalWakeLastAttemptTicketBootId`
- `D5TerminalWakeLastAttemptOwnerSessionEpoch`
- Sender의 `QueueDepth`, `QueuedCount`, `RingAcceptedCount`
- `QueueFullDropCount`, `AdmissionErrorDropCount`, `TransportErrorCount`

### 2. WPF 연결 및 callback 등록

WPF에서 실제 TCP source와 동일한 Local IPv4를 선택하고 Connect합니다.

확인할 내용:

- UI `Connected`
- UDP `Listening ...:5000`
- callback protocol version `2`
- mask `1`
- `BootId`, `SessionEpoch`가 0이 아님
- `RpcInitialized=true`
- sender endpoint가 Armed
- Wire에서 `0x8080 → 0x405C`
- `0x405C` 응답 Status `0`, ErrorId `0`

### 3. GD-01 정상 Wake 시험

Diagnostics SDO 패널에서:

- 반드시 `Submit SDO Read`
- Slave: 실제 시험축 `1~4`
- Index: `0x6061`
- SubIndex: `0`
- ValueType: `Int8`
- DataLength: `1`
- Timeout: `1000`
- 한 번만 클릭

기대 결과:

- nonzero `TicketId` 발급
- PLC에서 정확히 52-byte `LMC2` UDP 1개
- UDP `EventId = TicketId`
- 이후 TCP `0x7E03` 요청 정확히 1회
- UDP 수신만으로 UI 결과가 바뀌지 않고, `0x7E03` 응답 후 완료 표시
- 카운터 변화:
  - Attempt `+1`
  - Enqueued `+1`
  - Rejected `+0`
  - Sender Queued `+1`
  - RingAccepted `+1`
  - QueueDepth는 다시 `0`

### 4. Race 처리

다음 메시지가 나오면 즉시 FAIL로 판단하지 마세요.

- retained ticket 없음
- callback 처리 중 busy
- UDP는 받았지만 자동 `0x7E03` 없음

이 경우:

- 같은 SDO를 다시 Submit하지 않음
- 발급된 Ticket에 `Refresh Ticket` 1회
- 새로운 `0x7E50` 없이 `0x7E03`만 1회인지 확인
- 결과는 `INCONCLUSIVE + polling recovery`로 기록

### 5. GD-02 두 번째 Ticket

같은 `0x6061:0` Read를 새 Ticket으로 한 번 더 실행합니다.

기대:

- TicketId가 첫 번째와 다름
- 각 Ticket당 UDP 1개
- 각 Ticket당 `0x7E03` 1개
- 누적 Attempt `+2`, Enqueued `+2`, Rejected `+0`

### 6. Disarm 및 재연결

WPF를 정상 Close합니다.

확인:

- Wire에서 `0x405D`
- callback tuple 초기화
- sender Disarm
- QueueDepth `0`
- 예전 UDP가 와도 UI/TCP 상태 변화 없음

다시 연결한 뒤:

- 새 `SessionEpoch` 확인
- 이전 Ticket을 재조회하지 않음
- 새 `0x6061:0` Read로 GD-01 재시험

지금은 여기까지 먼저 수행하세요. Duplicate/reorder/EventId=0/takeover 시험은 proxy·fake peer·별도 harness가 필요하므로 이후 단계입니다.

각 시험 후 다음을 보내주세요.

- BootId / SessionEpoch / TicketId
- 위 PLC 카운터의 전후 값
- WPF 로그
- Wireshark 캡처 또는 패킷 요약
- SDO 결과값

참고로 실제 로그상 다운로드는 이미 성공했습니다: `282 files`, `Download Ok`, `Project successfully loaded`. 다운로드 직전 Rebuild로 `Classes.lcb`가 재생성된 부분은 제가 별도로 감사 중이며, 위의 read-only SDO 시험은 진행해도 됩니다.

> 이거 테스트 GUI에 니가 말한 테스트 내용이 구현 안되있는거 같은데? 내용 확인해봐.

<details><summary>7 previous messages</summary>

> 사용자가 read-only SDO 시험을 진행하는 동안 저는 방금 자동 Rebuild가 만든 `Classes.lcb` 변경이 ABI/동작 변경인지 단순 생성 메타데이터 변동인지 확인하겠습니다. LASAL과 PLC에는 추가 명령을 보내지 않고, 현재 다운로드 세션 로그와 생성 DB만 읽기 전용으로 감사합니다.
>
> 다운로드 세션 자체는 `Download Ok`이며 중단·Reset·Restart는 없었습니다. 다만 LASAL이 Download 직전에 Rebuild를 자동 수행해 `Classes.lcb`만 같은 길이로 다시 직렬화했습니다. 현재 Online 상태를 방해하지 않고, 커밋본과 현재 파일의 실제 바이트 차이 위치를 메모리에서 비교해 의미 범위를 좁히고 있습니다.
>
> 이거 테스트 GUI에 니가 말한 테스트 내용이 구현 안되있는거 같은데? 내용 확인해봐.
>
> 맞습니다. 제가 PLC Watch 항목, 패킷 캡처 항목, WPF UI에서 직접 조작할 항목을 한 목록에 섞어 설명했습니다. 실제 GUI에 어떤 버튼·필드·카운터가 구현돼 있는지 소스와 현재 실행 중인 EXE를 대조해서, **GUI에서 가능한 시험 / LASAL Watch가 필요한 시험 / 별도 도구가 필요한 시험**으로 다시 나누겠습니다.
>
> 현재 실행 중인 프로젝트 구조에서 제가 예상한 파일 경로가 달라 첫 검색이 실패했습니다. 기능 부재 판정은 아직 하지 않겠습니다. 실제 `MainWindow` 경로와 실행 EXE를 먼저 찾아서 다시 대조합니다.
>
> 현재 소스에는 `Submit SDO Read`, `Read SDO Inline`, `Refresh Ticket`, callback 자동 refresh 로직이 구현돼 있습니다. 하지만 배포 폴더의 실행 파일은 **2026-07-30 빌드**이고, 배포용 `MainWindow` 소스도 현재 개발 소스와 해시가 다릅니다. 사용자가 실행한 GUI가 이 오래된 배포 EXE라면 제가 설명한 화면이 없는 것이 맞습니다.
>
> 실행 중인 정확한 EXE 경로와 배포본에 빠진 컨트롤을 추가 대조하겠습니다.
>
> 아 그러네 미안하다
</details>

괜찮습니다. 제가 GUI 항목과 LASAL Watch/Wireshark 항목을 섞어 설명한 것도 혼동 원인이었습니다.

테스트에는 최신 실행 파일을 사용하세요.

`C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe`

GUI 위치:

- `SDO / Write Policy`
- `Asynchronous SDO Read / Write ticket`
- `Submit SDO Read`
- 결과 확인: `Execution log / raw callback diagnostics`

PLC의 Attempt/Enqueued/Rejected 카운터는 GUI가 아니라 LASAL Watch에서 봐야 합니다. 우선 GD-01 `0x6061:0`, `Int8`, 길이 `1`, timeout `1000`을 한 번 실행하면 됩니다.

> 이거 GUI가 에러났을때 다시 연결하는거 제대로 연결 안되는 버그가 있는거 같다. 내용 확인좀해봐.


# 테스트 항목 정리

> 이제 어떤 내용 테스트 하면 되는지 정리해줘

<details><summary>2 previous messages</summary>

> 현재 소스·Git 변경 상태와 기존 검증 문서를 기준으로, 자동 테스트와 LASAL IDE/PLC/실기 테스트를 구분해 정리하겠습니다. 이전 작업 기록도 짧게 대조해 이미 끝난 항목과 남은 항목을 섞지 않겠습니다.
>
> 현재 기준에서 핵심은 세 갈래입니다. 가장 급한 것은 callback owner-loss 재접속 실기 확인이고, `HomeDS402Ex`와 `SetPosition retirement`는 아직 PLC 기능이 아니라 PC 측 dormant 계약입니다. 최신 WPF 재접속 수정까지 포함해 각 항목의 PASS 조건을 확인 중입니다.
</details>

15:58 로그까지 확인했습니다. Rebuild와 282-file Download/PLC Link는 성공했고, 다운로드된 두 핵심 ST의 해시는 현재 추적 소스와 같습니다. 다만 실제 다운로드 경로는 `Elmo_Master_test` 복제본이므로, 결과 기록에는 “canonical 전체 프로젝트 provenance 확정”이 아니라 “핵심 reconnect source 일치 이미지”로 적는 것이 정확합니다.

결론부터 말하면, 지금 1순위는 **실제 PLC에서 같은 WPF 창의 `Connect → Close → Connect` 재접속 검증**입니다. PC 자동시험과 LASAL Build/Download는 이미 상당 부분 끝났습니다.

현재 확인된 상태:

- 2026-08-12 15:58 C78 Rebuild, 282-file Download, PLC Link 성공.
- 다운로드에 사용된 `TCPMotionInterface.st`, `LMCUdpCallbackSender.st` 해시는 현재 추적 소스와 일치.
- 다만 Download 경로는 `Elmo_Master_test` 복제본입니다. 따라서 결과는 “reconnect 핵심 소스 일치 시험 이미지” 증거이지 canonical 전체 프로젝트/production 승인 증거는 아닙니다.
- WPF V2 PC 회귀는 Release `347/347`, callback targeted `17/17` PASS.
- 아직 없는 증거는 **실제 PLC의 second Connect와 callback 재등록·수신 성공**입니다.

## 권장 테스트 순서

| 순서 | 시험 | 핵심 PASS 조건 |
|---:|---|---|
| 1 | GD-01 기본 callback | `0x405C` 등록 성공, 52-byte UDP 1개, 같은 Ticket의 `0x7E03` 1회, 최종 D5 성공 |
| 2 | GD-04 같은 창 재접속 | 고정 UDP port로 Close 후 second Connect 성공, 새 SessionEpoch/cookie, 새 callback 정상 수신 |
| 3 | GD-06 정상 Close | `0x405D`, disarm 결과 `0/1`, callback tuple clear, QueueDepth 0 |
| 4 | 창 X 종료→EXE 재실행 | 프로세스 종료 후 같은 EXE·port로 재접속 및 callback 성공 |
| 5 | 장애·소유권 경계 | LAN 단절/same-IP takeover와 different-IP rejection을 별도 판정 |
| 6 | 반복 soak | 재접속 반복 시 stale socket/tuple, queue 잔류, 세 번째 TCP 생성 없음 |

### 1. 시험 전 준비

- 모든 축과 그룹: Idle, Power Off, queued motion 없음.
- Recorder, D5, recovery 동작이 진행 중이지 않은지 확인.
- 이 시험에서는 SDO Write를 사용하지 않습니다.
- Wireshark에서 TCP 4000과 UDP 5000을 함께 캡처합니다.
- PLC Online Watch에는 최소한 다음을 추가합니다.

  - `SessionEpoch`
  - `RpcCallbackRegistered`
  - `RpcCallbackSessionEpoch`
  - `RpcCallbackLastDisarmResult`
  - `TakeoverCount`, `TakeoverRejectCount`, `LastTakeoverResult`
  - `QueueDepth`, `DisarmClearedCount`
  - current TCP socket
  - callback endpoint/session/cookie tuple

상세 변수와 캡처 기준은 [Gate D runtime runbook](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md:697)에 있습니다.

### 2. GD-01 기본 callback

1. WPF에서 `Connect`.
2. 화면에서 `Connected`, `Listening <endpoint>` 확인.
3. pcap에서 다음을 확인합니다.

   - `0x8080` 성공
   - 정확한 32-byte `0x405C` 요청
   - 20-byte 성공 응답
   - nonzero BootId/SessionEpoch

4. Low-level SDO에서 다음을 입력합니다.

   - Slave: 시험 축 `1..4`
   - Index: `0x6061`
   - Sub-index: `0`
   - Type: `Int8`
   - Length: `1`
   - Timeout: `1000`
   - `Submit SDO Read`를 한 번만 실행

5. `Refresh Ticket`과 `Read SDO Inline`은 누르지 않습니다.

PASS 조건:

- nonzero TicketId
- 해당 TicketId의 52-byte UDP 정확히 1개
- callback으로 시작된 `0x7E03` 정확히 1회
- UDP만으로 UI가 완료되지 않고 TCP `0x7E03` 응답 뒤 완료
- `State=Completed`, `Outcome=Success`
- QueueDepth가 다시 0
- drop/error counter 증가 없음

### 3. GD-04 같은 창 재접속 — 가장 중요한 시험

1. GD-01 성공 후 고정 callback port를 기록합니다.
2. 같은 WPF 창에서 `Close`.
3. `Disconnected`, listener `Stopped`가 될 때까지 기다립니다.
4. port를 바꾸지 않고 `Connect`.
5. 새 SessionEpoch/cookie와 새 `0x405C` 등록을 확인합니다.
6. 새 TicketId로 GD-01 SDO Read를 다시 실행합니다.

PASS는 단순히 `Connected` 표시가 뜨는 것이 아닙니다.

- second Connect 성공
- 새 TCP/session/cookie 사용
- 같은 UDP port 재사용
- 새 callback 등록 성공
- 새 52-byte callback 수신
- 새 `0x7E03`과 terminal result 성공

WPF가 자동 복구를 사용하면 다음 로그도 보존합니다.

- `ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2`
- `CandidateOrdinal`
- `FreshSessionRetryReason`
- persistent `-1`: 100 ms
- pre-response transport failure: 1000 ms
- candidate 2 실패 시 종료, candidate 3은 없어야 함

### 4. `-8` 판정 주의

`RpcCallbackLastDisarmResult=-8` 하나만 보고 PASS 처리하면 안 됩니다.

Owner-loss recovery PASS에는 모두 필요합니다.

- 허용된 current-socket disconnect/owner transition 경계
- old TCP 및 callback tuple 제거
- QueueDepth clear 또는 새 current-epoch tuple
- 새 `0x405C` 등록 성공
- 새 callback 수신 성공

반면 정상적인 clean Close의 PASS 값은 `0` 또는 `1`입니다. 일반 `0x405D`에서 `-8`이 나오면 clean Close PASS가 아니라 별도의 owner-loss recovery 사건입니다. `-9`는 fail-closed로 유지하고 private PLC state를 강제로 지우면 안 됩니다.

### 5. 이후 시험

정상 재접속을 닫은 뒤 다음 순서로 진행하면 됩니다.

- 창 X 종료 → 동일 EXE 재실행 → Connect/callback
- LAN 단절 후 same-IP recovery
- same-IP takeover와 different-IP rejection 별도 시험
- stale/duplicate/out-of-order callback
- reconnect 반복 soak
- read-only axis/group/status/topology
- 안전 승인 후 저속 Axis `PowerOn → Move → Stop → PowerOff`
- Group `PowerOn → Lock → Move → Stop → Unlock → PowerOff`
- D1~D5 fault/timeout/disconnect/orphan matrix

전체 PLC qualification 순서는 [API 개발 계획](/C:/work/Elmo/Elmo_Master/docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md:344)에 정리돼 있습니다.

## 지금 실기 시험하면 안 되는 항목

- `HomeDS402Ex`: typed 입력 모델만 존재합니다. command ID, wire, LASAL route, capability, WPF가 없습니다.
- SetPosition `0x7D12/0x7D14/0x7D1A`: PC 계약만 있고 PLC retained store/route/tombstone이 없습니다. capability bit 3/5/7도 OFF입니다.
- 위 기능들은 PLC에서 성공 여부를 시험할 단계가 아닙니다. 현재는 fail-closed와 zero-native-call 상태를 유지해야 합니다.

각 시험 결과는 같은 이름으로 `pcapng + WPF 로그 + PLC Watch 전후값 + BootId/SessionEpoch/TicketId`를 묶어 보존하면 됩니다.

> 테스트 항목 다시 정리해보자.

<details><summary>4 previous messages</summary>

> 이전 목록을 그대로 재사용하지 않고, 8월 18일 현재 Git·최근 테스트 증거·남은 PLC 검증을 다시 대조하겠습니다. 이번에는 “지금 바로 실행”, “후속 실기”, “현재 실행 금지” 세 구역으로 간결하게 재정리하겠습니다.
>
> 중요한 변경이 확인됐습니다. 8월 12일 다운로드 이미지에는 `IsClientConnected()` 정수 반환값을 `NOT`으로 처리한 결함이 남아 있었고, 현재 canonical 소스에서는 `= 0` 비교로 수정돼 있습니다. 따라서 재접속 시험은 “현재 수정본이 다시 Build/Download됐는지”를 먼저 확인하지 않으면 무효입니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 작업 트리는 정리된 릴리스 상태가 아니라 편집 중입니다. 특히 SetPosition 경로는 일부 라우팅만 보이고 처리 구현·문서가 맞지 않아, 이번 테스트 목록에서는 실행 금지 항목으로 분리하겠습니다.
</details>

결론부터 말하면, 다음 실기 핵심 순서는 **`GD-01 → GD-06 → GD-04`**입니다. 현재 작업 트리가 계속 수정 중이므로, **지금 소스를 새로 Build/Download하지 말고 시험 기준부터 고정**해야 합니다.

## 1. 시험 전 기준 고정

- [ ] 실행 EXE SHA-256 기록
- [ ] PLC `BootId / MapRevision` 기록
- [ ] 다운로드된 PLC 이미지와 `TCPMotionInterface.lba` 식별
- [ ] 전 축/그룹 안전 상태 확인
- [ ] TCP 4000, UDP 5000 패킷 캡처 시작
- [ ] PLC Watch의 callback tuple, QueueDepth, SessionEpoch 초기값 저장

주의: 8월 12일 15:58 및 8월 13일 09:00 이미지는 `NOT IsClientConnected()` 정수 처리 결함이 있었습니다. **09:16 수정 LBA가 반영된 상태만 callback 시험 후보**입니다. 현재 소스에는 이후 SetPosition 편집까지 들어갔으므로 PLC 이미지와 현재 소스가 동일하다고 보면 안 됩니다. [수정 소스](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2675), [다운로드 기록](/C:/work/Elmo/Elmo_Master/docs/history/260813/Elmo_Master_history_260813_1_part_060_lines_14751_15000.md:191)

## 2. 바로 실행할 테스트

| 순서 | 테스트 | PASS 조건 |
|---|---|---|
| 1 | 최종 Release EXE 재실행 | Debug 앱을 정상 종료한 뒤 Release 실행→종료→재실행 성공. named mutex 잔류 없음. 문서의 최종 SHA `AAC6A0...05E`를 사용하며, 재빌드했다면 새 SHA를 기록 |
| 2 | GD-01 기본 callback | `0x405C` 등록 성공 후 `0x6061:0 / Int8 / 1 byte` Read 실행. 52-byte UDP 정확히 1개, 같은 Ticket의 callback-triggered `0x7E03` 정확히 1회, terminal 성공, QueueDepth 0 |
| 3 | GD-06 정상 Close | `0x405D` 정확히 1회, disarm 결과 `0` 또는 `1`, callback tuple clear, QueueDepth 0, listener 종료 |
| 4 | GD-04 같은 창 재접속 | 앱을 종료하지 않고 같은 UDP port로 재접속. 새 SessionEpoch/cookie, 새 `0x405C`, 새 Ticket의 52-byte UDP와 `0x7E03` terminal 성공 |

`0x405D` 결과가 `-8`이면 clean Close PASS가 아닙니다. `-9`도 실패입니다. UI에서 “닫힘”만 확인해서는 GD-06 PASS로 판정할 수 없습니다. [실행 체크리스트](/C:/work/Elmo/Elmo_Master/docs/history/Elmo_Master_history_260813_2.md:28)

## 3. 그다음 후속 시험

- [ ] 같은 IP takeover와 다른 IP rejection을 별도 시험
- [ ] LAN 단절 후 재접속
- [ ] old/duplicate/out-of-order callback 및 반복 reconnect soak
- [ ] TW19 absolute multi-turn reset의 물리 효과 독립 확인
- [ ] TW20 실제 reset 시험
- [ ] 안전 승인 후 Axis `Power On → 안정 상태 3회 → Power Off`
- [ ] 이후 한 축 소거리 `Move → Stop → Power Off`
- [ ] Group `Disable → Power Off`, 이후 controlled `Power On → Enable → Disable → Power Off`
- [ ] Motion/Group 25-command matrix
- [ ] D1~D5 fault/recovery matrix
- [ ] 마지막에 Axis1 SDO Write exact 승인 대상만 별도 실행

## 4. 이미 확보된 증거

다음은 동일 이미지·바이너리라면 단순 반복할 필요가 없습니다.

- PC Debug/Release full smoke 각각 `354/354`
- RecoveryRetirement 각각 `20/20`
- quarantine 상태 실제 Close/X
- stale diagnostics journal archive/resolve
- TW19 PLC terminal/cleanup
- LMC Home application position 0
- Axis Power Off 안정 상태 `3/3`

단, TW19의 실제 multi-turn 물리 효과와 새 Axis Power On은 아직 미확인입니다. Group도 상태 조회만 했지 새 명령을 실행한 것은 아닙니다. [현재 실기 증거](/C:/work/Elmo/Elmo_Master/docs/architecture/LASAL_API_WPF_DIAGNOSTICS_MUTATION_RECOVERY_2026-08-13.md:119)

## 5. 현재 실행 금지

- **현재 dirty working tree의 PLC Build/Download**
- **SetPosition `0x7D12/0x7D14/0x7D1A` 성공 시험**
  현재 `LMC_ADMIN_SET_POSITION_STORE_CONFIGURED=FALSE`이고 retained store가 없습니다.
- **HomeDS402Ex 및 DS402 Home method 37**
- 안전구역, E-stop/STO, limit 확인 전 Servo On 및 motion
- callback 시험과 SDO Write 시험을 한 세션에서 혼합

각 결과는 `pcapng + WPF 로그 + PLC Watch 전후값 + EXE SHA + BootId/MapRevision + PLC 이미지 식별`을 한 묶음으로 남기면 됩니다.

> 1. 문서의 최종 SHA AAC6A0...05E를 사용하며, 재빌드했다면 새 SHA를 기록 -> SHA가 어떤걸 의미하는지 모르겠어
> 2. callback등록 -> 어떻게 등록하는지 모르겠음.

<details><summary>2 previous messages</summary>

> 두 항목 모두 사용자가 직접 패킷을 만들거나 값을 계산하는 작업은 아닙니다. 현재 실행 파일 위치·해시와 WPF의 자동 callback 등록 동작을 소스와 실행 절차에서 다시 확인해, 실제 버튼 기준으로 정리하겠습니다.
>
> 확인 결과, 제가 앞서 적은 `AAC6…05E`는 “현재 파일명”이 아니라 당시 Release EXE의 지문입니다. 현재 Release EXE는 이후 다시 만들어져 SHA가 이미 달라졌으므로, 그 값을 그대로 사용하라는 표현은 잘못됐습니다. 현재 파일 기준으로 바로잡겠습니다.
</details>

맞습니다. 두 표현을 작업자 기준으로 고치면 아래와 같습니다. 특히 `AAC6…05E`를 사용하라는 안내는 현재 상태와 맞지 않았습니다.

## 1. SHA가 의미하는 것

SHA-256은 **실행 파일의 지문**입니다.

- 비밀번호나 설정값이 아닙니다.
- WPF 화면에 입력하지 않습니다.
- 어떤 EXE로 시험했는지 기록하기 위한 값입니다.
- EXE가 한 번이라도 재빌드되면 값이 달라질 수 있습니다.

대상 파일은 다음입니다.

`C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe`

제가 방금 확인한 현재 SHA-256은 다음입니다.

```text
1B17A8E24D545DC28C477EEE45AF0B5F5E94924F4574DCED7AD6805D7C6FC1AA
```

문서의 `AAC6A0...05E`는 이전 빌드의 값이며, 현재 그 파일은 남아 있지 않습니다. 따라서 체크리스트는 다음처럼 고치면 됩니다.

> 시험에 사용할 EXE의 SHA-256을 시험 직전에 기록한다. 기존 `AAC6...05E`와 비교할 필요는 없다.

확인 명령:

```powershell
Get-FileHash -LiteralPath 'C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe' -Algorithm SHA256
```

## 2. Callback 등록 방법

**Callback은 사용자가 별도로 등록하지 않습니다. `Connect`를 누르면 앱이 자동으로 등록합니다.**

상단 `Connection / RPC callback` 영역에서:

1. PLC IP: `10.10.150.1`
2. TCP port: `4000`
3. PC local IPv4: 실제 PLC 연결용 PC 주소
   현재 예시는 `10.10.150.13`
4. Callback UDP port: `5000`
5. `Connect` 클릭

그러면 앱이 자동으로:

1. UDP 5000 포트를 엽니다.
2. RPC session을 초기화합니다.
3. PLC에 callback 등록 명령 `0x405C`를 보냅니다.

등록 성공 화면은 다음과 같아야 합니다.

```text
Connection state = Connected
Callback listener = Listening 10.10.150.13:5000, rejected=0
Status=0
ErrorId=0
Version=2
MaxDatagram=52
Source=10.10.150.1
EventMask=0x00000001
```

BootId, SessionEpoch, Cookie는 세션마다 바뀌므로 특정 숫자와 비교하지 말고 **0이 아닌지만 확인**하면 됩니다.

중요한 구분:

- Connect 직후 `Accepted=0`은 정상입니다.
- 위 결과는 “callback 등록 성공”만 증명합니다.
- 실제 callback 동작은 이후 아래 Read를 실행해서 확인합니다.

```text
Operation: Read
Slave: 1~4
Index: 0x6061
Sub-index: 0
Type: Int8
Length: 1
Timeout: 1000
```

`Submit SDO Read`를 한 번 누른 뒤 `Accepted` 증가, 같은 Ticket의 `0x7E03`, 최종 `Completed/Success`까지 확인해야 실제 callback E2E PASS입니다.

따라서 체크리스트 문구는 간단히 다음으로 바꾸겠습니다.

> 연결값 입력 → Connect 클릭 → 자동 callback 등록 결과 확인 → SDO Read로 실제 callback 수신 확인

> 지금 Callback Mask 구현 진행되어있는 내용은 얼마나 진행되었지?

<details><summary>2 previous messages</summary>

> Callback Mask를 PC SDK, WPF, LASAL 처리, 자동시험, 실제 PLC 증거로 나눠 현재 소스 기준으로 확인하겠습니다. 특히 “비트가 정의된 것”과 “해당 이벤트가 실제 발생·전송된 것”을 분리해서 정리하겠습니다.
>
> 여기서 말하는 “Callback Mask”는 두 의미가 섞일 수 있습니다. 현재 연결 등록의 `EventMask=0x00000001`과, Maestro의 `MMC_GetEventsMask/MMC_SetEventsMask` 같은 런타임 변경 API를 함께 확인하고 있습니다. 둘은 구현 수준이 다릅니다.
</details>

현재 상태는 **“범용 Callback Mask”가 아니라, D5 완료 알림용 `0x00000001` 한 비트만 구현된 상태**입니다.

## 진행 상태

| 항목 | 상태 |
|---|---|
| `0x405C` V2 요청에 32-bit EventMask 전송 | 완료 |
| WPF Connect 시 자동 등록 | 완료 |
| WPF가 보내는 값 | `0x00000001` 고정 |
| PLC의 Mask 검증·저장 | 완료 |
| Mask에 따른 이벤트 차단 | 완료 |
| D5 terminal 이벤트 생성 | 완료 |
| PC 수신 검증 및 자동 `0x7E03` 조회 | 완료 |
| PC fake-RPC/UDP 자동시험 | PASS 기록 있음 |
| 실제 PLC `0x405C` 등록 | PASS |
| 실제 52-byte UDP → `Accepted` → `0x7E03` | 미검증 |
| 같은 창 Close→Connect 후 Mask 재등록 | 미검증 |
| 여러 이벤트 Mask 선택 | 미구현 |
| 실행 중 Mask 변경 | 미구현 |
| `Get/Set/ClearEventsMask` API | 미구현 |

초기 1-bit 기능을 9단계로 보면 대략 **7/9까지 진행**됐습니다. 다만 남은 두 단계가 실제 PLC E2E와 재접속이므로 아직 production 완료로 보면 안 됩니다.

## 현재 Mask의 정확한 의미

```text
Mask 값     = 0x00000001
EventMaskBit = 1
EventType    = 1
EventId      = D5 TicketId
의미         = Diagnostics 작업 terminal 상태 발생
```

여기서 `0x00000001`은 최하위 비트, 즉 bit index로는 bit 0입니다. 코드에서는 값 자체를 `EventMaskBit=1`이라고 표현합니다.

WPF는 사용자가 Mask를 선택하는 구조가 아니며 Connect할 때 `1u`를 고정 전송합니다. [WPF 연결 코드](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:626)

PLC는 D5 작업이 `Completed/Failed/Cancelled/Expired`가 되면 이 비트를 확인한 후 UDP wake를 큐에 넣습니다. [PLC publisher](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2764)

PC는 UDP를 최종 결과로 믿지 않고, 같은 Ticket의 TCP `0x7E03` 상태 조회를 실행합니다. [PC 정책](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcCallbackModels.cs:87)

## 다중 Mask는 아직 동작하지 않음

현재 등록 검사는 `Mask AND 1 = 1`만 요구합니다. 따라서 아래 값도 등록 자체는 가능합니다.

```text
0x00000001  허용
0x00000003  허용
0xFFFFFFFF  허용
0x00000000  거부
0x00000002  거부
```

하지만 실제 publisher와 PC receiver는 **`EventMaskBit=1`만 허용**합니다. 추가 비트는 저장될 뿐 아무 이벤트도 생성하지 않습니다. [PLC Mask 처리](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st:451)

또한 다음 Maestro 계열 기능은 현재 없습니다.

- `MMC_GetEventsMask`
- `MMC_SetEventsMask`
- `MMC_ClearEventsMask`
- Motion Ended 이벤트
- EMCY, Heartbeat, Homing 이벤트
- WPF 이벤트별 선택 체크박스

따라서 정확한 판정은 다음입니다.

> **D5 terminal용 고정 Mask `0x1`은 소스 구현과 PLC 등록까지 완료됐습니다. 실제 UDP callback E2E와 재접속 재등록 시험은 남아 있습니다. 범용 32-bit Callback Mask 기능은 아직 구현되지 않았습니다.**

다음 확인은 `GD-01` 한 번이면 됩니다. 실제 52-byte UDP, `Accepted +1`, 동일 Ticket의 자동 `0x7E03`, QueueDepth 0을 확인하면 단일 Mask의 첫 E2E가 닫힙니다.

> Mask는 Connect에 등록하고 쓰는게 맞다고 보는데.. 그리고 elmo mask랑 비교해서 우리 sigmatek에 구현할 mask 선정해서 하기로 않했냐

<details><summary>3 previous messages</summary>

> 맞습니다. 직전 답변은 현재 코드만 설명하면서, **“Connect 시 선택 Mask 등록”과 “Elmo Event Mask를 비교해 SIGMATEK 지원 비트를 선정”하는 원래 설계 목표**를 빠뜨렸습니다. 이전 결정에서 실제로 어떤 비트를 선정했는지와, 그중 어디까지 구현됐는지를 다시 찾아 바로잡겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 기록상 구조는 분명합니다. Elmo도 `ConnectRPC(..., iEventMask, ...)`로 초기 Mask를 등록하고, 연결 후에는 `Set/Clear/GetEventsMask`로 변경할 수 있습니다. 우리 쪽은 현재 Connect 등록만 있고, 이 중 첫 비트만 구현된 상태인지 범위를 정리 중입니다.
</details>

맞습니다. 사용자 기억이 맞고, 제가 앞서 **현재 `0x1` 구현 범위만 설명해서 원래 계획을 누락했습니다.**

정확한 계획은 다음과 같습니다.

1. **Connect 시 Mask 등록**
   - `EventMask + Callback IP + UDP Port`를 `0x405C`로 PLC에 등록
   - PLC는 해당 세션 동안 등록된 Mask로 이벤트를 필터링
   - 현재 SIGMATEK 설계에서는 변경하려면 `Close → Connect`
   - Elmo 원본에는 연결 후 `Set/Clear/GetEventsMask`도 있지만, 우리 초기 범위의 필수 기능으로 확정되지는 않았습니다.

2. **Elmo EventMask와 비교해 SIGMATEK 지원 이벤트 선정**

   과거 기록에서 검토된 항목은 다음과 같습니다.

| 값 | Elmo 의미 | SIGMATEK 대응 |
|---:|---|---|
| `0x00000001` | Async Reply | 현재 D5 Terminal wake로 구현 |
| `0x00000020` | EMCY | 후보 |
| `0x00000040` | Motion Ended | 후보 |
| `0x00000080` | Heartbeat Error | EtherCAT Slave `Online TRUE→FALSE` |
| `0x00000200` | Drive Error | 후보 |
| `0x00008000` | Node Error | `AL_StatusCode`, DS402 Fault |
| `0x00080000` | Node Connected | Slave `Online FALSE→TRUE` |

Elmo 전체 비트 비교는 [과거 분석 기록](/C:/work/Elmo/Elmo_Master/docs/history/260721/Elmo_Master_history_260721_part_005_lines_01001_01250.md:182)에 있고, EtherCAT 상태 매핑은 [SIGMATEK 매핑 기록](/C:/work/Elmo/Elmo_Master/docs/history/260721/Elmo_Master_history_260721_part_006_lines_01251_01500.md:52)에 있습니다.

다만 `EMCY + Motion Ended + HBEAT + Drive Error = 0x000002E0`은 당시 **검토 예시**였고, 최종 지원 Mask로 승인된 값은 아닙니다.

현재 실제 구현은 여기서 멈춰 있습니다.

- Connect가 Mask를 받는 구조: 구현됨
- WPF: `1u`로 고정되어 Mask 선택 불가
  [MainWindow.xaml.cs:626](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:626)
- PLC/PC 송수신: `0x00000001` D5 Terminal 한 종류만 지원
- EMCY, Motion Ended, Heartbeat, Drive/Node Error, Node Connected producer: 미구현
- 실제 PLC UDP E2E 검증: 미완료
- 더 큰 문제로, 현재 등록부는 `0x1`만 포함되면 `0x3`, `0xFFFFFFFF`도 승인하지만 실제로는 `0x1` 외 이벤트를 처리하지 못합니다.

또한 현재 `0x1`은 Elmo의 완전한 `ASYNC_REPLY` 패킷 구현이 아니라, **bit 0 값을 차용한 SIGMATEK 전용 D5 wake protocol**입니다.

따라서 정확한 진행률 표현은 다음입니다.

> **Connect 기반 Mask 등록 구조와 첫 번째 `0x1` 이벤트 경로만 구현됐다. Elmo 비교 후 SIGMATEK 최종 Mask 선정, UI 적용, 각 이벤트 producer/consumer 구현은 아직 끝나지 않았다.**

그러므로 지금은 `0x1` D5 E2E만 시험 가능하고, 다중 Mask 시험에 앞서 지원 비트표를 먼저 최종 확정해야 합니다.

> 테스트해야되는 내용 다시한번 정리해보자.

<details><summary>2 previous messages</summary>

> 이번에는 **지금 바로 실기 가능한 항목**, **Callback Mask/D5 실기**, **아직 테스트하면 안 되는 미완성 항목**으로 분리해 정리하겠습니다. 현재 소스와 최근 검증 기록이 어긋나지 않는지도 다시 확인하겠습니다.
>
> 현재 저장소는 오늘 SetPosition P1 편집이 다시 진행 중이라, **현재 작업 트리를 그대로 Rebuild/Download해서 시험하는 것은 제외**해야 합니다. 장비에 이미 올라간 고정 시험 이미지로 Callback/기존 Motion을 검증하거나, 최신 소스를 시험하려면 먼저 새 PLC·WPF 한 쌍을 고정해야 합니다.
</details>

결론부터 말하면, **현재 최신 작업 트리는 PLC 다운로드/실기 NO-GO**입니다. SetPosition P1 편집 중이고 정적 기준을 아직 통과하지 못했습니다. 기존 PLC 이미지를 시험할 수는 있지만, 결과는 “현재 설치 이미지 시험”으로 기록해야 하며 최신 소스 검증으로 보면 안 됩니다. [현재 SetPosition 상태](/C:/work/Elmo/Elmo_Master/docs/architecture/AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md:5)

## 0. 시험 전 준비

개발자가 먼저 다음을 확정해야 합니다.

- 시험용 PLC 이미지
- 실제 다운로드 시각
- PLC `BootId`, `MapRevision`
- 시험용 WPF EXE 한 개
- WPF EXE SHA-256

SHA는 사용자가 입력하는 값이 아니라 **실행파일이 바뀌지 않았음을 확인하는 지문**입니다. 이전 `AAC6A0...FA05E`는 과거 WPF Release EXE의 SHA이며 PLC 이미지 SHA가 아닙니다. 현재 시험 패키지를 고정하면 새 값을 제가 체크리스트에 적어두면 됩니다.

현재 작업 트리를 새로 Rebuild/Download하면 안 됩니다. 정확히 식별된 기존 PLC 이미지를 사용하거나, SetPosition P1을 닫고 새 시험 이미지를 만든 뒤 진행해야 합니다.

안전 전제:

- 모든 축·그룹 정지
- Callback 시험 중에는 축 Power Off
- pending motion/diagnostics/recorder/recovery 없음
- Wireshark로 TCP `4000`과 Callback UDP 포트 캡처
- 가능하면 PLC Watch와 WPF 로그 동시 기록

## 1. Connect 및 Callback 자동 등록

입력 예:

- PLC IP: `10.10.150.1`
- TCP Port: `4000`
- PC Local IPv4: 실제 PLC 통신 NIC 주소
- Callback UDP Port: 고정 포트, 예: `5000`

`Connect`를 한 번 누릅니다. 별도 Callback 등록 버튼은 없습니다. Connect 내부에서 자동으로 Mask `0x00000001`, V2, 52-byte를 등록합니다. [Connect 구현](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:626)

PASS:

- `Connected`
- `Listening <PC-IP>:<UDP-Port>`
- `Status=0`, `ErrorId=0`
- `Version=2`
- `MaxDatagram=52`
- `EventMask=0x00000001`
- BootId, SessionEpoch, Cookie가 모두 0이 아님
- 패킷 순서가 `0x8080 → 0x405C`
- `0x405C` 요청 32 bytes, 응답 20 bytes

Connect 직후 `Accepted=0`은 정상입니다. 이것은 등록 성공일 뿐 실제 Callback 수신 성공은 아닙니다.

## 2. GD-01 — D5 Callback 정상 1회

축은 Power Off·Standstill 상태로 둡니다.

SDO 입력:

- Operation: `Read`
- Slave: 실제 축 `1..4`
- Index: `0x6061`
- Sub-index: `0`
- Type: `Int8`
- Length: `1`
- Timeout: `1000`

`Submit SDO Read`를 정확히 한 번 누릅니다.

시험 중에는 다음을 누르지 않습니다.

- `Refresh Ticket`
- `Read SDO Inline (wait terminal)`

PASS:

- nonzero TicketId 한 개
- PLC에서 PC로 정확히 52-byte UDP 한 개
- UDP EventId와 TicketId 일치
- WPF `Accepted +1`
- `Rejected/Duplicate/OutOfOrder` 변화 없음
- Callback이 유발한 TCP `0x7E03` 정확히 한 번
- UDP만으로 상태를 완료하지 않고, `0x7E03` 응답 후에만 완료
- 최종 `State=Completed`, `Outcome=Success`
- `ResultType=Int8`, `ResultLength=1`
- PLC `Attempt +1`, `Enqueued +1`, `Rejected +0`
- Sender `QueueDepth=0`
- drop/error counter 증가 없음

[GD-01 정식 기준](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md:775)

Callback이 너무 빨리 도착해 `skipped while busy` 또는 `no exact current retained ticket`이 나오면 FAIL이 아니라 `INCONCLUSIVE`입니다. 기존 Ticket은 `Refresh Ticket` 한 번으로 회수하고, 새 Ticket으로 다시 시험합니다.

## 3. GD-02 — 서로 다른 Ticket 2개

첫 번째 Ticket이 완전히 종료된 뒤 같은 SDO Read를 다시 한 번 실행합니다.

PASS:

- 서로 다른 TicketId 두 개
- Ticket별 UDP 한 개
- Ticket별 자동 `0x7E03` 한 개
- 첫 번째 Ticket 재전송·재적용 없음

## 4. GD-06 — 정상 Close

pending Ticket이 없는 상태에서 `Close`를 한 번 누릅니다.

PASS:

- TCP `0x405D` 확인
- WPF `Disconnected`
- Callback listener `Stopped`
- PLC `RpcCallbackLastDisarmResult=0` 또는 `1`
- Callback tuple clear
- Sender `QueueDepth=0`

`RpcCallbackLastDisarmResult=-8`은 정상 Close PASS가 아닙니다. 별도의 owner-loss recovery 증거가 필요합니다.

## 5. GD-04 — 같은 창에서 재접속

같은 WPF 창과 같은 UDP 포트를 유지합니다.

```text
Connect → GD-01 → Close → Connect → 새 GD-01
```

PASS:

- 같은 UDP 포트로 다시 Listening
- 새 `SessionEpoch`
- 새 `Cookie`
- 새 `ListenerGeneration`
- 새 `0x405C` 등록 성공
- 새 Ticket의 UDP와 `0x7E03` 정상 완료

PLC를 재부팅하지 않았다면 BootId가 그대로여도 정상입니다. SessionEpoch와 Cookie는 새 값이어야 합니다. [GD-04 기준](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md:854)

## 6. GD-05 — UDP 유실 fallback

위 항목이 모두 PASS한 뒤 선택적으로 진행합니다.

- Callback UDP 포트만 임시 차단
- TCP는 유지
- SDO Read 한 번 Submit
- PC에서 UDP Accepted와 자동 `0x7E03`이 없어야 함
- `Refresh Ticket` 한 번으로 기존 Ticket 조회
- 새 `0x7E50` Submit은 없어야 함
- 시험 후 방화벽 규칙 제거

## 7. Callback 완료 후 기존 Motion 회귀

Callback 시험과 분리해서 수행합니다.

```text
Load Axis
→ Read Status / Read Position
→ Power On
→ 작은 Relative Move
→ Standstill 확인
→ Stop
→ Power Off
→ PowerOff + Standstill 3회 확인
```

각 명령은 정확히 한 번만 송신하고, 최종 상태는 반드시 `PowerOn=False`, `Standstill=True`여야 합니다. Group 시험은 Single Axis 시험이 끝난 뒤 별도 승인 범위로 진행합니다.

## 이번 시험에서 제외

- `0x1` 이외의 EventMask
- EMCY, MotionEnded, Heartbeat, Drive/Node Error, Node Connected callback
- SetPosition `0x7D12/0x7D14/0x7D1A`
- SDO Write
- TW19/TW20 재실행
- DS402 Home 및 물리 Home
- Recorder Double
- PLC private 변수 강제 변경
- 다른 IP takeover나 강제 fault injection

즉, 첫 실기의 핵심 순서는 다음입니다.

```text
시험 이미지 확정
→ Connect/Mask 0x1 자동 등록
→ GD-01
→ GD-02
→ GD-06
→ GD-04
→ 필요 시 GD-05
→ 별도 Single Axis Motion 회귀
```

> 테스트 내용 분석해봐
>
> User context
>
> Attachments:
> - [15:46:35.610] Refresh Home/Encoder Maintenance Capabilities started. [15:46:35…: `C:/Users/dreiv/.codex/attachments/337cfa33-6699-4fb9-98a9-a472cae85150/pasted-text.txt`

<details><summary>3 previous messages</summary>

> 첨부 로그를 시간순으로 재구성하고, 각 단계가 실제 PLC 명령 성공인지 단순 UI/등록 성공인지 분리해 보겠습니다. Callback, Home/Encoder, Motion, Close/Reconnect 결과를 각각 PASS·FAIL·미확정으로 판정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 큰 틀은 확인됐습니다. 4축 단축 시험과 D5 복구 일부는 유효한 PASS지만, Group motion은 제한값 0 때문에 시작도 못 했고 Callback은 UDP wake 도착 흔적만 있을 뿐 자동 `0x7E03` 연계가 증명되지 않았습니다. 지금은 로그의 “PASS” 범위를 과장하지 않도록 최종 판정을 정리하고 있습니다.
</details>

결론부터 말하면, 이번 로그는 **부분 PASS**입니다. 단축 4축과 D5 복구 기능은 상당 부분 통과했지만, Group motion과 Callback E2E는 아직 완료되지 않았습니다.

첨부 로그 SHA-256은 `C071C984...09BE79D`, 실행 중 PLC 식별값은 전 구간 `BootId=0x42`, `MapRevision=0x957F101E`로 유지됐습니다.

| 시험 영역 | 판정 | 핵심 분석 |
|---|---|---|
| 축 1~4 Power/Move/Stop/PowerOff | **PASS** | 각 명령 1회, Move replay 0, 최종 `POWER_OFF_STANDSTILL` |
| 실제 이동 중 축 Stop | **미검증** | 네 축 모두 `inMotionStopProof=NOT_CLAIMED` |
| Group Status RTT | **PASS** | 준비 후 10,000회 측정 성공 |
| Group True Buffered | **미실행** | Group limit가 0이라 preflight에서 차단 |
| Group Stop-first | **미실행** | 동일하게 Group limit 0으로 차단 |
| D5 Abort/Contention/Timeout 복구 | **PASS** | 의도한 오류 후 정상 ticket 복구 |
| D5 Queued Cancel | **INCONCLUSIVE** | Queued→Running race로 Cancel 실패 |
| Abrupt Disconnect | **Application Recovery만 PASS** | PLC orphan cleanup은 미증명 |
| Callback | **수신 흔적만 확인** | 자동 `0x7E03` 연계는 미증명 |
| Home/TW19/TW20 | **조회·capability만 PASS** | 실제 Home/Encoder maintenance 실행 없음 |
| EtherCAT/PI/Bulk | **부분 PASS** | 조회 성공, Bulk Release 기록 없음 |

### 1. 단축 4축 시험

축 1~4 모두 유효한 PASS입니다.

- 목표 대비 최종 오차: Axis1 `+1`, 나머지 `0 raw`
- 허용오차: `100 raw`
- PowerOn, MoveRelative, Stop, PowerOff 각각 1회
- 자동 Move replay `0`
- 최종 상태 모두 `POWER_OFF_STANDSTILL`

다만 Stop은 이동 완료 후 수행됐습니다. 따라서 “Stop 명령 후 안전 상태 복귀”는 확인됐지만, **움직이는 축을 실제로 중간 정지시키는 성능은 확인되지 않았습니다.** [축 시험 결과](/C:/Users/dreiv/.codex/attachments/337cfa33-6699-4fb9-98a9-a472cae85150/pasted-text.txt:38)

### 2. Group 시험

처음 RTT 두 번의 FAIL은 통신 실패가 아니라 시험 순서 문제입니다. Group Power/Profile Lock/InPosition 준비 전에 실행했습니다.

준비 후 10,000회 RTT는 PASS했습니다.

- 평균 `2.067 ms`
- p50 `1.936 ms`
- p95 `2.842 ms`
- p99 `4.377 ms`
- 최대 `17.785 ms`
- wall throughput `465.9회/s`

단, 이것은 `PC_API_RPC_ELAPSED` 측정입니다. PLC 내부 dispatch 시간은 측정하지 않았으므로 **PLC 실시간 지터 성능 PASS로 확대하면 안 됩니다.** [RTT 결과](/C:/Users/dreiv/.codex/attachments/337cfa33-6699-4fb9-98a9-a472cae85150/pasted-text.txt:195)

실제 Group motion 두 시험은 다음 값으로 시작 전에 차단됐습니다.

```text
Velocity=10000/0
Acceleration=100000/0
Deceleration=100000/0
```

즉 PLC가 광고한 Group limit가 `0`입니다. Group motion 기능이 실패한 것이 아니라 **시험 자체가 미실행**된 상태입니다. [Group preflight 실패](/C:/Users/dreiv/.codex/attachments/337cfa33-6699-4fb9-98a9-a472cae85150/pasted-text.txt:199)

### 3. D5 진단 복구

다음은 명확한 PASS입니다.

- 잘못된 `0xFFFF` 객체 → `0x06020000` 실패 → 정상 `0x6061` 복구
- 동시 제출 → 두 번째 요청 `ResourceBusy` → 이후 정상 복구
- `TimeoutCycles=1` → `Expired/TimedOut`, `0x05040000` → 정상 복구

이 오류들은 의도한 negative vector이므로 전체 FAIL이 아닙니다. [Abort 복구](/C:/Users/dreiv/.codex/attachments/337cfa33-6699-4fb9-98a9-a472cae85150/pasted-text.txt:257)

Queued Cancel은 ticket 78이 이미 Running으로 넘어가 `InvalidState`가 발생했습니다. 로그 판정대로 **INCONCLUSIVE이며 재시험해야 합니다.** [Queued Cancel](/C:/Users/dreiv/.codex/attachments/337cfa33-6699-4fb9-98a9-a472cae85150/pasted-text.txt:331)

Abrupt Disconnect는 새 연결에서 다시 동작했다는 것만 PASS입니다. `rpcCloseSent=false`, `orphanProof=NOT_PROVEN_BY_WPF`이므로 clean Close나 PLC orphan cleanup 증거는 아닙니다. [Disconnect 결과](/C:/Users/dreiv/.codex/attachments/337cfa33-6699-4fb9-98a9-a472cae85150/pasted-text.txt:345)

### 4. Callback 판정

Callback wake는 16회 PC/WPF handler까지 도달했습니다. 하지만 전부 다음 메시지로 종료됐습니다.

```text
D5 terminal wake ignored: no exact current retained ticket
```

현재 코드에서는 이 경우 자동 TCP 상태 조회 전에 바로 반환합니다. [Callback 처리 코드](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:5840)

따라서 이번 결과는:

- UDP callback wake가 PC에 도달한 흔적: **있음**
- 정확한 retained ticket 매칭: **실패**
- callback-triggered `0x7E03`: **증거 없음**
- D5 완료 결과: qualification runner의 polling으로 확인
- GD-01 Callback E2E: **미통과**

또한 Connect 당시 `0x405C`, Mask `0x1`, SessionEpoch, Cookie, Listener 상태, Accepted 카운터, 52-byte UDP 및 pcap도 첨부에 없습니다.

### 다음 시험 순서

1. Callback GD-01 전용 시험

   - Connect에서 Mask `0x1` 등록 결과 기록
   - `0x6061:0 Int8/1` low-level Submit 1회
   - exact retained ticket 유지
   - 52-byte UDP 1회 → 자동 `0x7E03` 1회 → terminal 확인
   - Accepted/producer/sender counter와 pcap 동시 확보

2. Clean Close 및 Reconnect

   - `0x405D`, tuple/queue clear 확인
   - 같은 창·같은 UDP 포트로 재연결
   - 새 SessionEpoch/Cookie와 새 ticket callback 확인

3. Group limit 문제 해결

   - `Read Group Parameters`의 실제 반환값을 로그에 표시
   - nonzero velocity/acceleration limit 설정
   - True Buffered와 Stop-first 재시험

4. Queued Cancel 재시험

5. Bulk Snapshot `Release` 실행 및 상태 확인

마지막으로 `C04A...D8AD`는 EtherCAT topology SHA입니다. WPF EXE나 PLC 이미지 SHA가 아닙니다. 이번 로그에는 WPF EXE SHA, PLC artifact SHA/download 시각, pcap이 없으므로 **실기 기능 로그로는 유효하지만 최종 릴리스 증거로는 아직 부족합니다.**

> Clean Close 및 Reconnect는 중복 실행 안되서 테스트 불가. Group limit 문제 해결 이건 먼지 모르겠다. 다른거 자세히 설명해주고 테스트 결과 분석해봐
>
> User context
>
> Attachments:
> - Callback GD-01 전용 시험.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test6/Callback GD-01 전용 시험.pcapng`
> - [16:16:03.549] Connect started. [16:16:03.561] Connection state Disconnected ->…: `C:/Users/dreiv/.codex/attachments/0a07d080-3469-4a18-a7da-2d7cfaaebb63/pasted-text.txt`
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-bf2f6072-5cb1-4b11-9b6e-c0fa6f60b67d.png`

<details><summary>1 previous message</summary>

> 이번에는 세 증거를 함께 맞춰보겠습니다: 실행 로그, PCAP의 실제 UDP/TCP 프레임, 화면의 Callback 카운터입니다. 중복 실행 제한은 결함으로 단정하지 않고 현재 앱 구조상 시험 불가 항목으로 분리하고, Group limit가 무엇이며 화면에서 무엇을 확인해야 하는지도 실제 입력값 기준으로 설명하겠습니다.
</details>

결론부터 말하면:

- **Clean Close/Reconnect는 테스트 가능합니다.** 두 번째 프로그램을 실행하는 방식이 아닙니다.
- Callback의 핵심 경로인 `52-byte UDP → ticket 일치 → 자동 0x7E03`은 이번에 실제로 동작했습니다.
- 다만 `Read SDO Inline`을 눌러 callback 전에 polling으로 완료 상태를 읽었기 때문에, 엄격한 GD-01 전체 PASS는 아닙니다.
- Group limit 문제는 사용자가 WPF 입력값을 잘못 넣은 것이 아니라, PLC가 그룹 제한값을 `0`으로 반환해서 앱이 모션을 차단한 것입니다.
- D5 Contention은 3회 모두 현재 시험 방식으로 동시 조건을 만들지 못했습니다. PLC 결함으로 단정하면 안 됩니다.

증거 파일:

- [실행 로그](/C:/Users/dreiv/.codex/attachments/0a07d080-3469-4a18-a7da-2d7cfaaebb63/pasted-text.txt)
- [PCAP](</C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test6/Callback GD-01 전용 시험.pcapng>)
- 로그 SHA-256: `418C6539...02B194C`
- PCAP SHA-256: `D5F062CE...DBB785F`

## Clean Close/Reconnect 설명

화면의 팝업은 “재접속 불가”가 아니라 **두 번째 EXE 실행 금지**입니다.

Clean Close/Reconnect는 다음 순서입니다.

1. 팝업에서 `확인`을 눌러 두 번째 실행만 종료합니다.
2. 기존에 실행 중인 원래 창을 그대로 사용합니다.
3. 축과 그룹이 Power Off이고 미해결 recovery가 없는지 확인합니다.
4. PCAP 캡처를 시작합니다.
5. 화면 상단 `연결` 옆의 `닫기`를 누릅니다.
   - Windows 창의 `X`가 아닙니다.
6. `Connected → Closing → Disconnected`
   및 Callback listener stopped를 기다립니다.
7. 같은 창, 같은 Local IP와 UDP `5000`에서 `연결`을 누릅니다.
8. 새 SessionEpoch, 새 Cookie, 새 `0x405C` 등록을 확인합니다.
9. 새 ticket으로 Callback GD-01을 한 번 실행합니다.

이 버튼은 실제로 RPC connection만 닫고 같은 창에서 재연결할 수 있도록 구현돼 있습니다. [Close 버튼 처리](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:809)

현재 PCAP에서도 자동 재연결은 이미 발생했습니다.

| 구분 | 최초 연결 | 자동 재연결 |
|---|---:|---:|
| TCP local port | 4893 | 6266 |
| Callback UDP port | 5000 | 5000 |
| SessionEpoch | 16 | 18 |
| Cookie | `0x7CFCA0DE0D3953F8` | `0xF7A25A47564F68CA` |
| EventMask | `0x1` | `0x1` |
| Callback sequence | 1~20 | 1~7 |

화면의 `Accepted=7`은 두 번째 SessionEpoch 18에서 받은 ticket 128~134의 7개 callback과 정확히 일치합니다. 전체 PCAP에는 callback이 27개 있지만 listener 세대가 바뀌면서 화면 카운터가 새로 시작한 것입니다.

다만 PCAP 전체에 `0x405D`는 **0회**입니다. 현재 재연결은 Abrupt Disconnect 시험이 만든 것이므로:

- 고정 UDP 포트 재사용
- 새 SessionEpoch/Cookie
- 새 `0x405C`
- 재연결 후 callback 수신

은 확인됐지만, **Clean Close는 아직 미시험**입니다.

또한 로그는 `LOCAL_TCP_ZERO_LINGER_CLOSE`라고 기록했지만 PCAP에서는 PC FIN → PLC FIN → PC ACK → PLC RST 순서였습니다. 진짜 client RST 강제 종료 증거도 아닙니다.

## Callback GD-01 결과

Ticket 107에서 핵심 callback 경로는 확인됐습니다.

```text
0x7E50 Submit              frame 34
Ticket 107 반환           frame 35
사전 0x7E03 polling       5회
Completed 확인            frame 45
52-byte UDP callback      frame 46
callback 자동 0x7E03      frame 47
terminal TCP response     frame 48
```

로그도 동일합니다.

- `D5 terminal wake matched retained ticket`
- `authoritative TCP status query started`
- `Callback D5 authoritative TCP status processed`

[Ticket 107 callback 로그](/C:/Users/dreiv/.codex/attachments/0a07d080-3469-4a18-a7da-2d7cfaaebb63/pasted-text.txt:14)

UDP 수신 후:

- 자동 `0x7E03`까지 약 `17.0 ms`
- terminal 응답까지 약 `19.0 ms`
- 같은 ticket UDP 중복 없음
- TCP 재전송·loss·out-of-order 없음

따라서 다음은 PASS입니다.

- `0x405C` Mask `0x1` 등록
- 52-byte v2 UDP 전송
- EventId와 TicketId 107 일치
- WPF exact ticket correlation
- callback-triggered `0x7E03` 1회
- authoritative TCP response 처리

하지만 공식 GD-01 전체는 아직 `NOT QUALIFIED`입니다. 이유는 `Submit SDO Read`가 아니라 `Read SDO Inline`을 눌렀기 때문입니다. Callback 도착 전에 polling 5회로 이미 Completed를 확인했습니다.

### GD-01 정확한 재시험 방법

`SDO / Write 정책` 탭에서:

- Slave: `1`
- Object: `0x6061`
- SubIndex: `0`
- ValueType: `Int8`
- Length: `1`
- TimeoutCycles: `1000`

그다음:

1. Callback Accepted 현재값을 기록합니다.
2. 새 PCAP을 시작합니다.
3. **`Submit SDO Read`만 한 번 누릅니다.**
4. 다음 버튼은 누르지 않습니다.
   - `Read SDO Inline`
   - `Refresh Ticket`
   - D5 qualification runner
5. 아래 로그 두 줄이 나올 때까지 기다립니다.
   - `matched retained ticket`
   - `authoritative TCP status processed`
6. Accepted가 정확히 `+1`인지 확인합니다.
7. 캡처를 바로 종료합니다.

버튼 위치와 구분은 [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:2104)에 명시돼 있습니다.

완전한 Gate D 판정을 하려면 PLC Watch의 producer/sender counter 증가와 `QueueDepth=0`도 추가로 필요합니다.

## 새 D5 시험 결과

| 시험 | 판정 | 분석 |
|---|---|---|
| Manual Inline ticket 107 | 기능 PASS | Callback 핵심 경로 확인, 공식 GD-01 절차는 불충족 |
| Abort → Recovery | **PASS** | ticket 109 `0x06020000`, ticket 110 정상 복구 |
| Contention 1차 | **FAIL** | ticket 113이 ResourceBusy가 아니라 accepted |
| Pending Cleanup 1차 | **PASS, 제한적** | 새 ticket 114/115 성공, old 113 결과는 미확정 |
| Contention 2차 | **FAIL** | ticket 118이 accepted |
| Pending Cleanup 2차 | **PASS, 제한적** | 새 ticket 119/120 성공 |
| Timeout → Recovery | **PASS** | ticket 122 TimedOut, ticket 123 정상 복구 |
| Queued Cancel | **INCONCLUSIVE** | ticket 125가 너무 빨리 Running/Completed |
| Abrupt Disconnect | **Application Recovery PASS** | 새 ticket 128/129 성공, PLC orphan 미증명 |
| Contention 3차 | **FAIL** | ticket 132가 accepted |
| Pending Cleanup 3차 | **PASS, 제한적** | 새 ticket 133/134 성공 |

### Contention이 3번 실패한 이유

이것을 “PLC가 동시 요청을 잘못 허용했다”라고 단정하면 안 됩니다.

PCAP을 보면 첫 번째 ticket 응답과 두 번째 Submit 사이에 약 `18 ms`가 있고, 중간에 capability RPC도 들어갑니다. `0x6061` 읽기가 매우 빨라 첫 번째 동작이 이미 terminal이 된 뒤 두 번째 요청이 들어갈 수 있습니다.

즉 실제로는:

```text
첫 번째 Submit
→ 첫 번째 SDO가 빠르게 완료
→ 두 번째 Submit
→ 새 ticket 정상 허용
```

이므로 ResourceBusy 조건이 만들어지지 않은 것입니다. 이후 이전 ticket을 조회하면 현재 단일 ticket slot이 새 ticket으로 교체돼 `DetailCode=23`이 발생합니다. [첫 번째 Contention 실패](/C:/Users/dreiv/.codex/attachments/0a07d080-3469-4a18-a7da-2d7cfaaebb63/pasted-text.txt:61)

따라서 현재 판정은:

- Contention 계약 PASS: **아님**
- PLC 동시성 결함 확정: **아님**
- 현재 runner가 진짜 동시 조건을 만들지 못함: **맞음**

이 버튼은 당분간 반복하지 않는 것이 좋습니다. 확실히 느린 승인된 read vector 또는 PLC 실행을 일부러 pending 상태로 유지하는 시험 설계가 필요합니다.

화면의 초록색 `D5SdoPendingCleanup PASS`도 Contention이 통과했다는 뜻이 아닙니다. 새 ticket 두 개가 다시 정상 완료돼 **응용프로그램 사용 가능 상태가 복구됐다는 뜻**입니다. old ticket 132의 최종 결과는 여전히 모릅니다.

### Queued Cancel

Ticket 125가 Queued 구간을 지나 Running으로 넘어간 뒤 Cancel이 도착해서 `InvalidState`가 발생했습니다.

- Cancel 재전송 없음
- PLC Stop 명령 없음
- ticket은 정상 Completed
- 판정 `INCONCLUSIVE_RACE`

안전 동작은 맞지만 Queued Cancel 기능은 검증되지 않았습니다. 몇 번을 반복해도 계속 `InvalidState`라면 Contention과 마찬가지로 너무 빠른 SDO vector가 원인일 가능성이 높습니다.

## Group limit가 무엇인가

Group limit는 PLC가 그룹 이동에 허용하는 최대 속도와 가속도입니다.

```text
시험 요청
Velocity = 10000
Acceleration/Deceleration = 100000

PLC가 반환한 상한
PathVelocityLimit = 0
PathAccelerationLimit = 0

비교 결과
10000 > 0
100000 > 0

따라서 실제 Move/Stop 명령 전 차단
```

앱은 실제 그룹 motion을 보내기 전에 PLC의 limit를 읽고 요청값이 limit 이하인지 확인합니다. [WPF 비교 코드](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs:908)
PLC는 MotionLib의 실제 group parameter를 그대로 반환합니다. [PLC 반환 코드](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st:10155)

따라서 이전 Group 결과는:

- GroupTrueBuffered 기능 FAIL: 아님
- GroupDeterministicStopFirst 기능 FAIL: 아님
- **둘 다 `NOT_RUN / PRECHECK_BLOCKED`**

### 사용자가 지금 확인할 내용

1. `읽기 전용 API` 탭으로 이동합니다.
2. `Refresh Admin Capabilities`를 누릅니다.
3. `Semantic group parameters`에서 `All`을 선택합니다.
4. `Read Group Parameters`를 누릅니다.
5. 다음 세 값을 복사합니다.
   - `PathVelocityLimit`
   - `PathAccelerationLimit`
   - `JerkTime`

[Group parameter UI 코드](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs:161)

값이 양수라면 qualification의 Velocity/Acceleration/Deceleration을 그 이하로 입력해 재시험할 수 있습니다.

값이 계속 `0`이면 사용자가 WPF 값을 0으로 낮춰 해결할 문제가 아닙니다. 현재 qualification 입력은 양수만 허용합니다. 이 경우에는:

- PLC 초기화에서 안전한 group-specific limit를 설정하거나
- SIGMATEK 근거로 `0=미설정/제한 비활성`임을 확인한 뒤 runner 정책을 수정해야 합니다.

현재 소스만으로 `0`의 정확한 MotionLib 의미는 확정하지 못했습니다. 따라서 임의 값 설정은 하면 안 됩니다.

## 지금 실행할 우선순위

1. 올바른 버튼으로 Callback GD-01 재시험
2. 같은 창에서 `닫기 → 연결` Clean Close/Reconnect
3. `Read Group Parameters / All` 결과 확보
4. Bulk Snapshot 탭에서 기존 resource가 있으면 `Refresh Status → 4 Release`
5. Queued Cancel은 한두 번 재실행하되 계속 race면 중단
6. Contention은 runner 개선 전 추가 반복하지 않음

이번 로그에는 Bulk/Release, Home 실행, TW19/TW20 실제 encoder maintenance 및 이동 중 Stop 시험은 없습니다.

> [16:39:10.444] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-20 07:38:58 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-20 07:38:55 UTC.
> [16:39:10.445] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [16:45:55.685] Connect started.
> [16:45:55.773] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:45:55.785] Ignored stale connection-state event Connecting from an older transport session.
> [16:45:55.785] Connection state Connecting -> Connected
> [16:45:55.893] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:45:55.904] Connect PASS.
> [16:45:58.785] Close Connection started.
> [16:45:58.789] Connection state Connected -> Closing
> [16:45:58.805] Close Connection PASS.
> [16:46:01.250] Connect started.
> [16:46:01.252] Connection state Disconnected -> Connecting
> [16:46:01.264] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:01.265] Connection state Connecting -> Connected
> [16:46:01.339] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:01.339] Connect PASS.
> [16:46:02.611] Close Connection started.
> [16:46:02.611] Connection state Connected -> Closing
> [16:46:02.621] Close Connection PASS.
> [16:46:06.999] Connect started.
> [16:46:07.001] Connection state Disconnected -> Connecting
> [16:46:07.011] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:07.012] Connection state Connecting -> Connected
> [16:46:07.084] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:07.084] Connect PASS.
> [16:47:58.941] Submit SDO Read started.
> [16:47:58.966] QTEST|utc=2026-08-20T07:47:58.966Z|elapsedMs=0|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [16:47:58.973] QTEST|utc=2026-08-20T07:47:58.973Z|elapsedMs=8|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [16:47:58.981] QTEST|utc=2026-08-20T07:47:58.981Z|elapsedMs=16|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [16:47:59.055] QTEST|utc=2026-08-20T07:47:59.055Z|elapsedMs=91|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=135|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [16:47:59.057] QTEST|utc=2026-08-20T07:47:59.057Z|elapsedMs=93|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|ticket=135|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=135|quarantine=false
> [16:47:59.060] Submit SDO Read PASS.
> [16:47:59.096] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000087
> [16:47:59.117] QTEST|utc=2026-08-20T07:47:59.117Z|elapsedMs=151|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=135|state=Completed|outcome=Success|verdict=CLEARED
> [16:47:59.118] QTEST|utc=2026-08-20T07:47:59.118Z|elapsedMs=152|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [16:47:59.124] Callback D5 authoritative TCP status processed. TicketId=0x00000087
>
> 이렇게 했는데 정확한 결과가 안나오면 니가 PC 제어해서 테스트해봐
>
> User context
>
> Attachments:
> - GD-01 재시험.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/Test6/GD-01 재시험.pcapng`

<details><summary>4 previous messages</summary>

> 먼저 새 PCAP에서 `0x405D` 두 번과 마지막 ticket 135의 `0x7E50 → 52-byte UDP → 자동 0x7E03 → terminal` 순서를 정확히 대조하겠습니다. 증거가 부족할 때만 제가 현재 Windows 앱을 조작하겠습니다. 그 가능성 때문에 `computer-use` 지침을 먼저 확인하고, 기존 실행 상태와 안전 차단을 보존하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 새 캡처에서는 이미 핵심 순서가 확인됐습니다. 마지막 재연결 세션의 ticket 135는 수동 상태조회 없이 `Submit → 52-byte UDP 1회 → 자동 0x7E03 1회 → Completed/Success`로 이어집니다. 지금은 두 번의 Close가 실제 `0x405D` 성공 응답까지 받았는지와 실행 파일 식별값만 최종 교차검증하고 있습니다.
>
> 추가 PC 조작은 하지 않겠습니다. 이번에는 캡처가 충분히 결정적이며, 같은 시험을 다시 실행하면 새 ticket만 생기고 기존 증거를 더 강하게 만들지는 않습니다. 다만 “기능 경로 PASS”와 “PLC Watch 카운터까지 포함한 정식 Gate-D 종료”는 구분해서 판정하겠습니다.
</details>

결론부터 말하면, 이번 재시험은 **ticket 135 기준 GD-01 핵심 인과 경로 PASS**입니다. 결과가 충분히 명확하므로 제가 PC를 다시 조작하지 않았습니다.

| 항목 | 판정 |
|---|---|
| GD-01 패킷/WPF 인과 경로 | **PASS** |
| 같은 창 Close/Reconnect 동작 | **2회 PASS** |
| GD-01 전체 Gate-D 증거 묶음 | PLC Watch 증거 부족 |
| GD-04 공식 전체 절차 | 부분 완료 |
| GD-06 공식 전체 절차 | PLC Watch 증거 부족 |

### GD-01 정확한 패킷 결과

[GD-01 재시험.pcapng](</C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test6/GD-01 재시험.pcapng>)에서 확인된 순서입니다.

```text
frame 181  0x7E50 Submit SDO Read 1회
frame 182  TicketId=135 반환
frame 183  52-byte UDP callback 1회
frame 185  callback-triggered 0x7E03 1회
frame 186  Completed/Success 응답
```

최종 결과:

```text
TicketId=135
Object=0x6061:0
State=Completed
Outcome=Success
ErrorId=0
Detail=0
ResultType=Int8
ResultLength=1
Data=0x08 (8)
BootId=0x42
```

중요한 점은 다음과 같습니다.

- UDP 이전 `0x7E03` polling: **0회**
- UDP 이후 자동 `0x7E03`: **정확히 1회**
- UDP callback: **정확히 52-byte 1개**
- UDP → `0x7E03`: 약 **23.63 ms**
- UDP → terminal 응답: 약 **26.01 ms**
- TCP 재전송·손실·순서 역전: **0**
- Callback tuple도 등록값과 일치:
  - `SessionEpoch=27`
  - `Cookie=0x521F125D06FCDE46`
  - `EventMaskBit=1`
  - `EventId=135`

따라서 이전 시험처럼 Inline polling이 먼저 완료한 경우가 아닙니다. 이번에는 callback이 실제로 authoritative TCP 조회를 발생시켰습니다.

### Clean Close/Reconnect

실제로 두 번 정상 수행됐습니다.

```text
연결 1: Epoch 21 → 0x405D 성공 → 종료
연결 2: Epoch 24 → 0x405D 성공 → 종료
연결 3: Epoch 27 → ticket 135 callback 성공
```

두 `0x405D` 응답 모두 `Status=0, ErrorId=0`이며, 동일 UDP 포트 `5000`을 재사용하면서 TCP 연결·SessionEpoch·Cookie가 새로 생성됐습니다.

FIN 종료가 끝난 뒤 PLC가 추가 RST를 보낸 흔적은 있지만, 이미 `0x405D` 성공 응답과 FIN 교환이 완료된 뒤라 Clean Close 판정을 무효화하지 않습니다.

`Ignored stale connection-state event Connecting...`도 오류가 아닙니다. 이전 transport의 늦은 UI 이벤트를 버린 것으로, 현재 연결은 정상적으로 `Connected`가 됐습니다.

### 아직 남은 것

기능을 확인하기 위한 GD-01 재실행은 필요 없습니다. 다만 [Gate D runbook](</C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/GATE_D_CALLBACK_RUNTIME_TEST_RUNBOOK_2026-08-10.md:783>) 기준으로 공식 증거 묶음을 닫으려면 PLC Watch가 필요합니다.

- `D5TerminalWakeAttemptCount +1`
- `D5TerminalWakeEnqueuedCount +1`
- `D5TerminalWakeRejectedCount +0`
- `QueuedCount +1`
- `RingAcceptedCount +1`
- 최종 `QueueDepth=0`
- Close 후 `RpcCallbackLastDisarmResult=0 또는 1`
- callback tuple clear

이 값의 전후 캡처가 없다면 PC만 다시 조작해도 채워지지 않습니다. Watch 값을 먼저 기록한 뒤 1회만 실행해야 합니다.

이번 시험 식별값은 다음과 같습니다. 이전 `AAC6...` EXE SHA를 사용하면 안 됩니다.

```text
PCAP SHA-256:
697A6B7A2CBF40DB4045EE2F8A0AE9843DD05334DBE109F941C75CE8166B9C29

EXE SHA-256:
CA0B006EB21DD447512B5C9343533B5471A8718D80ACFCBD2BF7343FA02EE550

DLL SHA-256:
256E0E3F67D034EF476BCD3A89653108AC940B9664715EE25E0300D9700ADE77
```

최종 기록 문구는 **“GD-01 actual PLC callback causal chain PASS; 전체 Gate-D 종료를 위한 PLC producer/sender 및 disarm Watch 증거는 미수집”**이 정확합니다.

> 테스트를 니가좀 진행해봐 내가 참고좀 하게

<details><summary>9 previous messages</summary>

> 이번에는 제가 실제 화면에서 진행하겠습니다. `computer-use` 지침에 따라 현재 실행 중인 WPF와 LASAL 상태를 먼저 읽고, 축 동작이나 쓰기 없이 `0x6061:0 Int8/1` GD-01 한 건만 수행해 로그·패킷·가능하면 PLC Watch 전후값까지 남기겠습니다. 기존 세션과 캡처 파일은 건드리지 않고 새 증거 파일을 사용하겠습니다.
>
> `computer-use` 본문은 확인했고, 연결된 필수 문서 경로가 패키지 루트 쪽이라 바로 다시 읽겠습니다. 이 지침 때문에 화면 조작 전에 대상 창을 하나로 고정하고, 관찰과 한 번의 조작을 번갈아 수행합니다.
>
> <details><summary>Called 7 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "현재 Windows 앱 목록 확인",
>   "code": "if (!globalThis.sky) {\n  const { sky } = await import(\"@oai/sky\");\n  globalThis.sky = sky;\n}\nglobalThis.apps = await sky.list_apps();\nnodeRepl.write(JSON.stringify(apps, null, 2));"
> }
> ```
>
> ```text
> [
>   {
>     "displayName": "ChatGPT",
>     "id": "OpenAI.Codex_2p2nqsd0c76g0!App",
>     "isRunning": true,
>     "lastUsedDate": "2026-08-20",
>     "useCount": 15,
>     "windows": [
>       {
>         "app": "OpenAI.Codex_2p2nqsd0c76g0!App",
>         "id": 3933112,
>         "title": "ChatGPT"
>       }
>     ]
>   },
>   {
>     "displayName": "카카오톡",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Kakao\\KakaoTalk\\KakaoTalk.exe",
>     "isRunning": true,
>     "lastUsedDate": "2026-08-19",
>     "useCount": 11,
>     "windows": [
>       {
>         "app": "process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe",
>         "id": 131448,
>         "title": "카카오톡"
>       }
>     ]
>   },
>   {
>     "displayName": "Visual Studio 2019",
>     "id": "VisualStudio.89931261",
>     "isRunning": true,
>     "windows": [
>       {
>         "app": "VisualStudio.89931261",
>         "id": 198774,
>         "title": "LasalApiWpfTestApp (실행) - Microsoft Visual Studio"
>       }
>     ]
>   },
>   {
>     "displayName": "OLAP용 Oracle Provider for OLE DB Readme",
>     "id": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>     "isRunning": true,
>     "windows": [
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 724834,
>         "title": "Test6 - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 8390442,
>         "title": "Release - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 1117040,
>         "title": "내 PC - 파일 탐색기"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 4458804,
>         "title": "design - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 5115032,
>         "title": "api - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 4197596,
>         "title": "docs - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 1969662,
>         "title": "다운로드 - 파일 탐색기"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 4263250,
>         "title": "Elmo_Master - 파일 탐색기 [ main ↓0 ↑0 ]"
>       }
>     ]
>   },
>   {
>     "displayName": "Oracle Provider for OLE DB Readme",
>     "id": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>     "isRunning": true,
>     "windows": [
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 724834,
>         "title": "Test6 - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 8390442,
>         "title": "Release - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 1117040,
>         "title": "내 PC - 파일 탐색기"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 4458804,
>         "title": "design - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 5115032,
>         "title": "api - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 4197596,
>         "title": "docs - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 1969662,
>         "title": "다운로드 - 파일 탐색기"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 4263250,
>         "title": "Elmo_Master - 파일 탐색기 [ main ↓0 ↑0 ]"
>       }
>     ]
>   },
>   {
>     "displayName": "Windows Software Development Kit",
>     "id": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>     "isRunning": true,
>     "windows": [
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 724834,
>         "title": "Test6 - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 8390442,
>         "title": "Release - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 1117040,
>         "title": "내 PC - 파일 탐색기"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 4458804,
>         "title": "design - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 5115032,
>         "title": "api - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 4197596,
>         "title": "docs - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 1969662,
>         "title": "다운로드 - 파일 탐색기"
>       },
>       {
>         "app": "process:C:\\Windows\\explorer.exe",
>         "id": 4263250,
>         "title": "Elmo_Master - 파일 탐색기 [ main ↓0 ↑0 ]"
>       }
>     ]
>   },
>   {
>     "displayName": "Google Chrome",
>     "id": "Chrome",
>     "isRunning": true,
>     "windows": [
>       {
>         "app": "Chrome",
>         "id": 2165522,
>         "title": "CP-313-eng.pdf - Chrome"
>       },
>       {
>         "app": "Chrome",
>         "id": 2492842,
>         "title": "Daou Office - 홈 - Chrome"
>       }
>     ]
>   },
>   {
>     "displayName": "LasalMotionControlApiExample",
>     "id": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\LasalApiWpfTestApp\\LasalApiWpfTestApp\\bin\\Release\\LasalMotionControlApiExample.exe",
>     "isRunning": true,
>     "lastUsedDate": "2026-08-20",
>     "useCount": 7,
>     "windows": [
>       {
>         "app": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\LasalApiWpfTestApp\\LasalApiWpfTestApp\\bin\\Release\\LasalMotionControlApiExample.exe",
>         "id": 3476900,
>         "title": "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]"
>       }
>     ]
>   },
>   {
>     "displayName": "picpick",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\PicPick\\picpick.exe",
>     "isRunning": true,
>     "lastUsedDate": "2026-08-20",
>     "useCount": 6,
>     "windows": [
>       {
>         "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\PicPick\\picpick.exe",
>         "id": 1641218,
>         "title": "픽픽 - 이미지 002"
>       }
>     ]
>   },
>   {
>     "displayName": "Wireshark",
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Wireshark\\Wireshark.exe",
>     "isRunning": true,
>     "lastUsedDate": "2026-08-20",
>     "useCount": 2,
>     "windows": [
>       {
>         "app": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Wireshark\\Wireshark.exe",
>         "id": 1182968,
>         "title": "GD-01 재시험.pcapng"
>       }
>     ]
>   },
>   {
>     "displayName": "Excel 2016",
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\EXCEL.EXE",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-20",
>     "useCount": 52,
>     "windows": []
>   },
>   {
>     "displayName": "LASAL Class 2",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-20",
>     "useCount": 24,
>     "windows": []
>   },
>   {
>     "displayName": "LASAL Class 2",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-20",
>     "useCount": 24,
>     "windows": []
>   },
>   {
>     "displayName": "Word 2016",
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\WINWORD.EXE",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-20",
>     "useCount": 3,
>     "windows": []
>   },
>   {
>     "displayName": "꿀뷰",
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Honeyview\\Honeyview.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-20",
>     "useCount": 1,
>     "windows": []
>   },
>   {
>     "displayName": "Notepad++",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Notepad++\\notepad++.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-19",
>     "useCount": 2,
>     "windows": []
>   },
>   {
>     "displayName": "디스크 정리",
>     "id": "{1AC14E77-02E7-4E5D-B744-2EB1AE5198B7}\\cleanmgr.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-19",
>     "useCount": 2,
>     "windows": []
>   },
>   {
>     "displayName": "ctrlX DRIVE Engineering 01V30.0",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\ctrlX WORKS\\ctrlX DRIVE Engineering\\0130\\cxde.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-19",
>     "useCount": 1,
>     "windows": []
>   },
>   {
>     "displayName": "LASAL MachineManager",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\MachineManager\\Bin\\MachineManager.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-12",
>     "useCount": 2,
>     "windows": []
>   },
>   {
>     "displayName": "LASAL MachineManager",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\MachineManager\\Bin\\MachineManager.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-12",
>     "useCount": 2,
>     "windows": []
>   },
>   {
>     "displayName": "Elmo Application Studio II 64bit",
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Elmo Motion Control\\Elmo Application Studio II\\ElmoMotionControl.View.Main.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-11",
>     "useCount": 1,
>     "windows": []
>   },
>   {
>     "displayName": "LASAL Safety Designer",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Safety Designer\\bin\\SafetyDesigner.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-11",
>     "useCount": 1,
>     "windows": []
>   },
>   {
>     "displayName": "WELSS.exe",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Welconsystems\\WELSS 2.01.0073\\WelconServoStudio.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-08-06",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "Lars",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lars\\Lars.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-07-29",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "IP-Mate",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\IPMate\\ipmate.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-07-21",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "MDS User Guide",
>     "id": "C:\\ProgramData\\MDS6\\MDS_UM6.pdf",
>     "isRunning": false,
>     "lastUsedDate": "2026-07-08",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "ctrlX IO Engineering 3.6.3",
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX IO Engineering\\3.6.3\\Studio\\Common\\ctrlX-IO-Engineering.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-07-07",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "ctrlX PLC Engineering 3.6.3",
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX PLC Engineering\\3.6.3\\Studio\\Common\\ctrlX-PLC-Engineering.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-07-07",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "ctrlX WORKS 3.6.2",
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX WORKS\\3.6.2\\ctrlx-works.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-07-07",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "LASAL Screen Modeless",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Screen\\LSE_Modeless.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-07-03",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "LASAL Screen Modeless",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Screen\\LSE_Modeless.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-07-03",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "LARSConfigTool",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lars\\LARSConfigTool.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-06-11",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "Freeplane",
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Freeplane\\freeplane.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-06-09",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "CANopen Architect Mini",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\CANopen Architect Mini\\CANopenArchitectMini.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-06-04",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "PowerPoint 2016",
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\POWERPNT.EXE",
>     "isRunning": false,
>     "lastUsedDate": "2026-05-13",
>     "useCount": 4,
>     "windows": []
>   },
>   {
>     "displayName": "Adobe Acrobat",
>     "id": "AcrobatReader",
>     "isRunning": false,
>     "lastUsedDate": "2026-05-06",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "Advanced IP Scanner",
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Advanced IP Scanner\\advanced_ip_scanner.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-04-07",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "Qt Creator 18.0.1 (Community)",
>     "id": "C:\\Qt\\Tools\\QtCreator\\bin\\qtcreator.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-04-07",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "Eclipse IDE for Java Developers - 2023-09",
>     "id": "C:\\Users\\dreiv\\eclipse\\java-2023-09\\eclipse\\eclipse.exe",
>     "isRunning": false,
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "windows": []
>   },
>   {
>     "displayName": "MDS Installation Guide",
>     "id": "C:\\ProgramData\\MDS6\\MDS Installation Guide.pdf",
>     "isRunning": false,
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "windows": []
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "WPF 시험 창 상태 확인",
>   "code": "{\n  globalThis.targetApp = apps.find((app) => app.displayName === \"LasalMotionControlApiExample\");\n  if (!targetApp) throw new Error(\"Target WPF app not returned by list_apps\");\n  if (targetApp.windows.length !== 1) {\n    nodeRepl.write(JSON.stringify(targetApp.windows, null, 2));\n    throw new Error(\"Expected exactly one WPF window\");\n  }\n  globalThis.returnedWpfWindow = targetApp.windows[0];\n  globalThis.targetWindow = await sky.get_window({\n    id: returnedWpfWindow.id,\n    app: returnedWpfWindow.app,\n  });\n  await sky.activate_window({ window: targetWindow });\n  globalThis.state = await sky.get_window_state({\n    window: targetWindow,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ````text
> Window: "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write] Secondary Actions: Raise
> 		1 창 Secondary Actions: Raise
> 			2 단추 In App Menu Expander ID: InAppMenuExpander
> 		3 제목 표시줄
> 			4 메뉴 모음 시스템 ID: MenuBar
> 				5 메뉴 항목 시스템
> 			6 단추 최소화
> 			7 단추 최대화
> 			8 단추 닫기
> 		9 텍스트 언어
> 		10 콤보 상자 ID: ComboUiLanguage
> 		11 텍스트 LASAL 모션 제어 API 예제
> 		12 텍스트 모션 제어는 PLC에서 활성 상태입니다. 진단 제어는 PLC가 필요한 기능을 광고한 뒤에만 활성화됩니다. 값은 선택한 application UNIT을 사용하거나 raw DINT로 전달됩니다.
> 		13 그룹 연결 / RPC 콜백
> 			14 텍스트 연결 / RPC 콜백
> 			15 텍스트 PLC IP
> 			16 편집 (disabled) ID: TextRemoteIp
> 			17 텍스트 TCP 포트
> 			18 편집 (disabled) ID: TextRemotePort
> 			19 텍스트 PC 로컬 IPv4
> 			20 편집 (disabled) ID: TextLocalIp
> 			21 텍스트 콜백 UDP 포트
> 			22 편집 (disabled) ID: TextCallbackPort
> 			23 단추 연결 ID: ButtonConnect
> 				24 텍스트 연결
> 			25 단추 닫기 ID: ButtonCloseConnection
> 				26 텍스트 닫기
> 			27 텍스트 연결 상태
> 			28 텍스트 연결됨 ID: TextConnectionState
> 			29 텍스트 콜백 수신기
> 			30 텍스트 수신 대기 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			31 그룹 RPC 초기화 / 콜백 증거 ID: ExpanderRpcCallbackEvidence
> 				32 단추 RPC 초기화 / 콜백 증거 ID: HeaderSite
> 					33 텍스트 RPC 초기화 / 콜백 증거
> 				34 텍스트 RPC 초기화 증거
> 				35 텍스트 Attempt=3, Outcome=Connected, CandidateOrdinal=1, Remote=10.10.150.1:4000, Local=10.10.150.13, RequestedCallback=10.10.150.13:5000, Mode=Version2WakeHint, BoundCallback=10.10.150.13:5000, LocalSessionGeneration=1, 0x8080Attempts=1, Retry=False, InitOutcome=Succeeded, StartedUtc=2026-08-20T07:46:07.0018615Z, CompletedUtc=2026-08-20T07:46:07.0068642Z, LastACK={FrameValid=True, HeaderStatus=0, HeaderReserved=0, PayloadLength=24, HasCommandResult=False, CommandStatus=0, ErrorId=0}, Current=Active ID: TextRpcInitialization
> 				36 텍스트 콜백 v2 등록
> 				37 텍스트 Status=0, ErrorId=0, Version=2, MaxDatagram=52, BootId=0x00000042, SessionEpoch=27, Flags=0x00000000, Cookie=0x521F125D06FCDE46, ListenerGeneration=2, Source=10.10.150.1, EventMask=0x00000001, LocalSessionGeneration=1 ID: TextCallbackRegistration
> 				38 텍스트 PC 콜백 수신 증거
> 				39 텍스트 Accepted=1, Rejected=0, Duplicate=0, OutOfOrder=0 ID: TextCallbackCounters
> 				40 텍스트 Last decision=AcceptedWakeHint, ProtocolError=None ID: TextCallbackLastDecision
> 				41 텍스트 연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. UDP wake hint는 상태 판단 근거가 아니며, 동작 상태는 일치하는 TCP status 응답을 받은 뒤에만 변경됩니다.
> 		42 그룹 안전 / 복구 상세 정보 ID: ExpanderSafetyAndRecoveryDetails
> 			43 단추 안전 / 복구 상세 정보 ID: HeaderSite
> 				44 텍스트 안전 / 복구 상세 정보
> 		45 탭 ID: TabsMotion
> 			46 탭 항목 (selectable) 단축
> 				47 텍스트 단축
> 			48 탭 항목 (selectable) 그룹 모션
> 				49 텍스트 그룹 모션
> 			50 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				51 텍스트 EtherCAT / CREVIS / PI
> 			52 탭 항목 (selectable) Bulk 스냅샷
> 				53 텍스트 Bulk 스냅샷
> 			54 탭 항목 (selectable) 레코더
> 				55 텍스트 레코더
> 			56 탭 항목 (selectable) SDO / Write 정책
> 				57 텍스트 SDO / Write 정책
> 				58 창 ID: ScrollDiagnosticsOperations
> 					59 그룹 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19]) ID: GroupEncoderMaintenance
> 						60 텍스트 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])
> 						61 텍스트 파괴적 테스트 전용: TW[20]은 0x20FC:0x02에 UInt16 1을 씁니다. TW[19]는 0x20FC:0x01에 UInt16 1을 써 absolute multi-turn position을 reset하며 motion 전에 LMC Home이 필요합니다. 정확한 drive 축을 선택하십시오.
> 						62 텍스트 Fixed writes: TW[20]=0x20FC:0x02 <- UInt16 1; TW[19]=0x20FC:0x01 <- UInt16 1 ID: TextEncoderMaintenanceFixedTargets
> 						63 텍스트 Arm 전: 선택한 축의 Power를 끄고 안정적인 standstill과 실제 위치를 독립적으로 확인한 뒤 정확한 0x20FC 명령 지원 여부를 확인하십시오. 전용 0x7E53/0x7E54/0x7E55 경로는 일반 SDO Write와 분리됩니다.
> 						64 텍스트 작업
> 						65 콤보 상자 ID: ComboEncoderMaintenanceKind
> 						66 텍스트 Drive / 축 (1..4)
> 						67 콤보 상자 ID: ComboTestResetAxis
> 						68 텍스트 Timeout (ms, 1..60000)
> 						69 편집 ID: TextTestResetTimeout
> 						70 단추 Encoder 유지보수 Capability 새로고침 ID: ButtonRefreshTestResetCapabilities
> 							71 텍스트 Encoder 유지보수 Capability 새로고침
> 						72 단추 (disabled) Encoder 유지보수 결과 읽기 ID: ButtonReadTestResetStatus
> 							73 텍스트 (disabled) Encoder 유지보수 결과 읽기
> 						74 확인란 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다. ID: CheckTestResetPowerOffVerified
> 							75 텍스트 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다.
> 						76 확인란 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다. ID: CheckTestResetPhysicalPositionVerified
> 							77 텍스트 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다.
> 						78 확인란 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다. ID: CheckTestResetExactTargetVerified
> 							79 텍스트 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다.
> 						80 확인란 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다. ID: CheckEncoderMaintenanceCompatibilityVerified
> 							81 텍스트 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.
> 						82 텍스트 차단: 1단계의 모든 물리 확인과 encoder 호환성 확인이 필요합니다. Encoder 유지보수 작업을 arm하거나 전송하지 않았습니다. ID: TextEncoderMaintenanceArmGateStatus
> 						83 단추 안전 / 복구 상세 정보 열기 Description: 로컬 안전/복구 panel만 펼칩니다. 이 버튼은 PLC 명령을 전송하지 않으며 어떤 record도 폐기하지 않습니다. ID: ButtonOpenEncoderRecoveryDetails
> 							84 텍스트 안전 / 복구 상세 정보 열기
> 						85 단추 (disabled) 1단계 - Encoder 유지보수 Arm Description: Live mutation 승인, 현재 TW[20]/TW[19] capability, 사용 가능한 durable 유지보수 journal 및 1단계의 네 가지 확인이 필요합니다. Arm은 정확한 요청을 PC 메모리에만 준비하며 0x7E53을 전송하지 않습니다. ID: ButtonArmTestReset
> 							86 텍스트 (disabled) 1단계 - Encoder 유지보수 Arm
> 						87 확인란 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다. ID: CheckTestResetFinalConfirmed
> 							88 텍스트 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다.
> 						89 단추 (disabled) 2단계 - Encoder 유지보수 1회 실행 Description: 변경되지 않은 PC 전용 arm 요청, live mutation 승인, 현재 capability, 사용 가능한 durable 유지보수 journal 및 2단계 확인이 필요합니다. Execute는 0x7E53을 한 번 전송합니다. ID: ButtonExecuteTestReset
> 							90 텍스트 (disabled) 2단계 - Encoder 유지보수 1회 실행
> 						91 텍스트 Arm되거나 대기 중인 encoder 유지보수 작업이 없습니다. ID: TextTestResetResult
> 					92 그룹 저장된 Mutation 복구
> 						93 텍스트 저장된 Mutation 복구
> 						94 텍스트 Mutation journal 준비됨. 해결되지 않은 durable write record가 없습니다. ID: TextPersistedMutationStatus
> 						95 확인란 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다. ID: CheckPersistedMutationPhysicallyVerified
> 							96 텍스트 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.
> 						97 단추 (disabled) 복구된 Mutation 승인 ID: ButtonAcknowledgePersistedMutation
> 							98 텍스트 (disabled) 복구된 Mutation 승인
> 					99 그룹 비동기 SDO Read / Write ticket
> 						100 텍스트 비동기 SDO Read / Write ticket
> 						101 텍스트 작업
> 						102 콤보 상자 ID: ComboSdoOperation
> 						103 텍스트 슬레이브 참조 (1..4)
> 						104 편집 ID: TextSdoSlaveReference
> 						105 텍스트 객체 인덱스 (0x0001..0xFFFF)
> 						106 편집 ID: TextSdoIndex
> 						107 텍스트 서브인덱스 (0..255)
> 						108 편집 ID: TextSdoSubIndex
> 						109 텍스트 값 형식
> 						110 콤보 상자 ID: ComboSdoValueType
> 						111 텍스트 데이터 길이 (1, 2 또는 4)
> 						112 콤보 상자 ID: ComboSdoDataLength
> 						113 텍스트 Timeout (1..60000 cycle)
> 						114 편집 ID: TextSdoTimeoutCycles
> 						115 텍스트 Write 값 (10진수 또는 raw 0x hex)
> 						116 편집 (disabled) ID: TextSdoWriteData
> 						117 텍스트 SDK 승인 SDO Write target
> 						118 콤보 상자 (disabled) ID: ComboSdoWriteTarget
> 						119 단추 SDO Read 전송 Description: Write mode는 현재 session의 same-value 활성화 증명 뒤 2단계 확인을 사용합니다. Read mode는 추적되는 SDO Read 하나를 전송합니다. ID: ButtonSubmitSdo
> 							120 텍스트 SDO Read 전송
> 						121 단추 SDO Inline 읽기 (terminal 대기) Description: 일반 1/2/4-byte SDO Read 하나를 전송하고 terminal 결과를 기다립니다. SDO Write, 필수 Write readback 또는 CREVIS I/O는 실행하지 않습니다. ID: ButtonReadSdoInline
> 							122 텍스트 SDO Inline 읽기 (terminal 대기)
> 						123 단추 (disabled) Inline 대기 취소 (PC만) Description: PC 측 Inline Read 대기만 취소합니다. CancelOperation을 전송하거나 PLC ticket을 취소하거나 SDO request를 재시도하지 않습니다. ID: ButtonCancelSdoInlineWait
> 							124 텍스트 (disabled) Inline 대기 취소 (PC만)
> 						125 단추 (disabled) 필수 정확 Readback 불러오기 Description: 마지막으로 성공한 SDO Write를 확인하는 데 필요한 정확한 read request를 복원합니다. 명령은 전송하지 않습니다. ID: ButtonLoadRequiredSdoReadback
> 							126 텍스트 (disabled) 필수 정확 Readback 불러오기
> 						127 단추 Ticket 새로고침 ID: ButtonRefreshDiagnosticOperation
> 							128 텍스트 Ticket 새로고침
> 						129 단추 (disabled) 결과 다운로드 ID: ButtonDownloadSdoResult
> 							130 텍스트 (disabled) 결과 다운로드
> 						131 단추 결과 저장 ID: ButtonExportSdoResult
> 							132 텍스트 결과 저장
> 						133 단추 (disabled) Ticket 취소 ID: ButtonCancelDiagnosticOperation
> 							134 텍스트 (disabled) Ticket 취소
> 						135 텍스트 TicketId=135, Kind=SDORead, State=Completed, Outcome=Success SubmitCycle=13290199, CompletionCycle=13290221, ErrorId=0, Detail=0x00000000 ResultType=Int8, ResultLength=1, Data=08 ID: TextDiagnosticOperationSummary
> 					136 그룹 D5 SDO fault/recovery qualification (읽기 전용)
> 						137 텍스트 D5 SDO fault/recovery qualification (읽기 전용)
> 						138 텍스트 이 runner는 SDO를 쓰거나 EtherCAT fault를 주입하지 않습니다. Abort qualification은 제조사 승인된 존재하지 않는 읽기 전용 object/sub-index를 사용합니다. Contention, timeout, queued-cancel qualification은 0x6061:0 Int8/1만 사용합니다. Abrupt Disconnect는 RPC Close(0x405D)를 보내지 않고 API TCP transport만 의도적으로 끊은 뒤 재연결하여 0x6061:0을 정확히 두 번 읽습니다. 선택한 Slave 1..4는 _LMCAxis1..4에 매핑되며 모든 runner는 D5 ticket 전송 전에 PowerOn=False, Standstill=True 위치 sample 3회가 안정적으로 일치해야 합니다.
> 						139 텍스트 Abort PASS: 안정적인 BootId/MapRevision -> baseline -> 정확한 SDO abort -> 동일 값 복구. Contention PASS: 첫 0x6061 ticket 승인 -> 첫 terminal 전에 두 번째 Submit이 정확히 Rejected/ResourceBusy -> 첫 ticket Completed/Success -> 정확히 같은 첫 값으로 세 번째 별도 ticket Completed/Success. Timeout PASS: 0x6061 TimeoutCycles=1 -> 정확한 Expired/TimedOut -> 제한된 ResourceBusy drain 대기 -> 별도 동일 값 복구. Queued Cancel PASS: 0x6061 전송 -> 즉시 one-shot CancelOperation -> 정확한 Cancelled/Cancelled -> 별도 동일 값 복구. Abrupt Disconnect는 앱에서 확인 가능한 새 연결 복구만 PASS할 수 있으며 PLC MarkOrphan/late-callback 증거가 생길 때까지 orphanQualified는 false입니다. 연결 손실 전 terminal 완료 또는 Running race는 PLC orphan 증명에 대해 INCONCLUSIVE입니다. 예상 밖 승인 또는 불확실한 결과는 보존되어 자동 재시도를 차단합니다. Cancel Runner는 PLC Stop을 전송하지 않습니다.
> 						140 텍스트 대상 slave (1..4)
> 						141 편집 ID: TextD5SdoAbortSlaveReference
> 						142 텍스트 Abort 객체 인덱스
> 						143 편집 ID: TextD5SdoAbortIndex
> 						144 텍스트 Abort 서브인덱스
> 						145 편집 ID: TextD5SdoAbortSubIndex
> 						146 텍스트 Abort 값 형식
> 						147 콤보 상자 ID: ComboD5SdoAbortValueType
> 						148 텍스트 데이터 길이
> 						149 편집 ID: TextD5SdoAbortDataLength
> 						150 텍스트 일반/복구 timeout cycle
> 						151 편집 ID: TextD5SdoAbortTimeoutCycles
> 						152 단추 D5 Abort -> 복구 실행 ID: ButtonRunD5SdoAbortQualification
> 							153 텍스트 D5 Abort -> 복구 실행
> 						154 단추 D5 Contention -> 복구 실행 ID: ButtonRunD5SdoContentionQualification
> 							155 텍스트 D5 Contention -> 복구 실행
> 						156 단추 D5 Timeout -> 복구 실행 ID: ButtonRunD5SdoTimeoutQualification
> 							157 텍스트 D5 Timeout -> 복구 실행
> 						158 단추 D5 Queued Cancel -> 복구 실행 ID: ButtonRunD5SdoQueuedCancelQualification
> 							159 텍스트 D5 Queued Cancel -> 복구 실행
> 						160 단추 D5 Abrupt Disconnect -> 앱 복구 실행 ID: ButtonRunD5SdoDisconnectRecoveryQualification
> 							161 텍스트 D5 Abrupt Disconnect -> 앱 복구 실행
> 						162 단추 (disabled) Runner 취소 (PLC Stop 아님) ID: ButtonCancelD5SdoQualification
> 							163 텍스트 (disabled) Runner 취소 (PLC Stop 아님)
> 						164 단추 QTEST 로그 저장 ID: ButtonSaveD5SdoQualificationLog
> 							165 텍스트 QTEST 로그 저장
> 						166 진행률 표시줄 ID: ProgressD5SdoQualification
> 						167 텍스트 아직 qualification을 실행하지 않았습니다. ID: TextD5SdoQualificationProgress
> 						168 편집 ID: TextD5SdoQualificationSummary
> 							169 스크롤 막대 ID: HorizontalScrollBar
> 								170 단추 (disabled) ID: PART_LineLeftButton
> 								171 단추 (disabled) ID: PageLeft
> 								172 단추 (disabled) ID: PageRight
> 								173 엄지 (disabled)
> 								174 단추 (disabled) ID: PART_LineRightButton
> 					175 그룹 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						176 텍스트 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						177 텍스트 실제 WRITE: 이 runner는 승인된 target을 읽고 정확히 같은 4 byte를 쓴 뒤 정확한 readback을 요구합니다. Sentinel을 쓰지 않으며 자동 복원이나 재전송을 수행하지 않습니다.
> 						178 텍스트 PLC가 bit 8/9/13을 광고하고, 이 SDK build가 승인 target을 정확히 하나만 노출하고, durable mutation journal을 사용할 수 있고, 작업자 확인 4개가 모두 선택돼야 버튼이 fail-closed 상태에서 풀립니다. 확인 후 선택 축의 PowerOn=False, Standstill=True, 안정 위치를 다시 검사하고 target도 다시 읽어 baseline과 계속 일치해야 Write를 전송합니다.
> 						179 텍스트 SDK 승인 단일 target
> 						180 콤보 상자 ID: ComboD5SdoWriteQualificationTarget
> 						181 텍스트 Timeout (1..60000 cycle)
> 						182 편집 ID: TextD5SdoWriteQualificationTimeoutCycles
> 						183 확인란 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다. ID: CheckConfirmD5SdoWriteUi24Unused
> 							184 텍스트 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다.
> 						185 확인란 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다. ID: CheckConfirmD5SdoWriteOriginalRecorded
> 							186 텍스트 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다.
> 						187 확인란 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다. ID: CheckConfirmD5SdoWriteCaptureRunning
> 							188 텍스트 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다.
> 						189 확인란 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다. ID: CheckConfirmD5SdoWriteSingleWriter
> 							190 텍스트 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다.
> 						191 단추 (disabled) 동일 값 SDO Write Qualification 실행 Description: CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: ButtonRunD5SdoWriteSameValueQualification
> 							192 텍스트 (disabled) 동일 값 SDO Write Qualification 실행
> 						193 텍스트 준비 상태 matrix / 다음 시도 (cache, wire 전송 없음)
> 						194 텍스트 OVERALL CLOSED | EVALUATION_WIRE=NONE SDK POLICY FAIL | blockers=CapabilityObservationNotCurrent | targets=1 PLC CAPS CACHED | bit8/read=1 bit9/write=1 bit13/general=1 IDENTITY PASS | BootId=0x00000042 MapRevision=0x957F101E PAYLOAD PASS | SDO=4 req=1320 resp=2040 cycleUs=1000 QUAL TARGET PASS | selected=Reserved diagnostic UI[24] | Slave 1 | 0x2F00:24 | Int32 | -1073741823..1073741823 RUNNER PASS | connected=True idle=True motionClear=True manualTicketClear=True mutationInterlockClear=True JOURNAL PASS CONFIRMATIONS FAIL | 0/4 AXIS PROOF PENDING_RUNNER_PREFLIGHT | PowerOn=False, Standstill=True, stable position are not cached here. MANUAL WRITE CLOSED_RUN_SAME_VALUE_FIRST | proof never transfers across connection sessions or PLC identity changes. NEXT CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: TextD5SdoWriteQualificationGateStatus
> 						195 텍스트 마지막 시도 / 현재 실행
> 						196 텍스트 아직 동일 값 SDO Write qualification을 실행하지 않았습니다. ID: TextD5SdoWriteQualificationSummary
> 					197 그룹 PI Write 정책 gate
> 						198 텍스트 PI Write 정책 gate
> 						199 텍스트 EtherCAT / PI 탭에서 WritableByPolicy Catalog 항목을 정확히 하나 선택하십시오. Schema v1이 payload를 정의하고 compile-time allowlist가 승인될 때까지 호출은 fail-closed입니다. DS402 ControlWord와 target object는 영구 차단됩니다.
> 						200 텍스트 Raw 값 (10진수 또는 0x...)
> 						201 편집 (disabled) ID: TextPiWriteRawValue
> 						202 단추 (disabled) PI Write 전송 ID: ButtonSubmitPiWrite
> 							203 텍스트 (disabled) PI Write 전송
> 					204 스크롤 막대 ID: VerticalScrollBar
> 						205 단추 (disabled) ID: PART_LineUpButton
> 						206 단추 (disabled) ID: PageUp
> 						207 단추 (disabled) ID: PageDown
> 						208 엄지 (disabled)
> 						209 단추 (disabled) ID: PART_LineDownButton
> 			210 탭 항목 (selectable) 읽기 전용 API
> 				211 텍스트 읽기 전용 API
> 		212 그룹 실행 로그 / 콜백 진단 ID: ExpanderExecutionLog
> 			213 단추 실행 로그 / 콜백 진단 ID: HeaderSite
> 				214 텍스트 실행 로그 / 콜백 진단
> 			215 단추 로그 복사
> 				216 텍스트 로그 복사
> 			217 단추 로그 지우기
> 				218 텍스트 로그 지우기
> 			219 편집 ID: TextExecutionLog
> 				220 스크롤 막대 ID: VerticalScrollBar
> 					221 단추 (disabled) ID: PART_LineUpButton
> 					222 단추 (disabled) ID: PageUp
> 					223 단추 (disabled) ID: PageDown
> 					224 엄지 (disabled)
> 					225 단추 (disabled) ID: PART_LineDownButton
> 				226 스크롤 막대 ID: HorizontalScrollBar
> 					227 단추 (disabled) ID: PART_LineLeftButton
> 					228 단추 (disabled) ID: PageLeft
> 					229 단추 (disabled) ID: PageRight
> 					230 엄지 (disabled)
> 					231 단추 (disabled) ID: PART_LineRightButton
> 		232 상태 표시줄 ID: StatusOperationBar
> 			233 텍스트 Log copied ID: TextOperationState
>
> Document text: ```
> [16:39:10.444] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-20 07:38:58 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-20 07:38:55 UTC.
> [16:39:10.445] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [16:45:55.685] Connect started.
> [16:45:55.773] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:45:55.785] Ignored stale connection-state event Connecting from an older transport session.
> [16:45:55.785] Connection state Connecting -> Connected
> [16:45:55.893] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:45:55.904] Connect PASS.
> [16:45:58.785] Close Connection started.
> [16:45:58.789] Connection state Connected -> Closing
> [16:45:58.805] Close Connection PASS.
> [16:46:01.250] Connect started.
> [16:46:01.252] Connection state Disconnected -> Connecting
> [16:46:01.264] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:01.265] Connection state Connecting -> Connected
> [16:46:01.339] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:01.339] Connect PASS.
> [16:46:02.611] Close Connection started.
> [16:46:02.611] Connection state Connected -> Closing
> [16:46:02.621] Close Connection PASS.
> [16:46:06.999] Connect started.
> [16:46:07.001] Connection state Disconnected -> Connecting
> [16:46:07.011] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:07.012] Connection state Connecting -> Connected
> [16:46:07.084] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:07.084] Connect PASS.
> [16:47:58.941] Submit SDO Read started.
> [16:47:58.966] QTEST|utc=2026-08-20T07:47:58.966Z|elapsedMs=0|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [16:47:58.973] QTEST|utc=2026-08-20T07:47:58.973Z|elapsedMs=8|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [16:47:58.981] QTEST|utc=2026-08-20T07:47:58.981Z|elapsedMs=16|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [16:47:59.055] QTEST|utc=2026-08-20T07:47:59.055Z|elapsedMs=91|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=135|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [16:47:59.057] QTEST|utc=2026-08-20T07:47:59.057Z|elapsedMs=93|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|ticket=135|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=135|quarantine=false
> [16:47:59.060] Submit SDO Read PASS.
> [16:47:59.096] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000087
> [16:47:59.117] QTEST|utc=2026-08-20T07:47:59.117Z|elapsedMs=151|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=135|state=Completed|outcome=Success|verdict=CLEARED
> [16:47:59.118] QTEST|utc=2026-08-20T07:47:59.118Z|elapsedMs=152|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [16:47:59.124] Callback D5 authoritative TCP status processed. TicketId=0x00000087
> ```
> ````
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "SDO 입력값이 보이도록 시험 패널 위로 이동",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.screenshots?.[0]?.id) throw new Error(\"No current screenshot observation\");\n  const screenshotId = observation.screenshots[0].id;\n  globalThis.state = null;\n  await sky.scroll({\n    window: observation.window,\n    screenshotId,\n    x: 700,\n    y: 590,\n    scrollX: 0,\n    scrollY: -782,\n  });\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ````text
> Window: "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write] Secondary Actions: Raise
> 		1 창 Secondary Actions: Raise
> 			2 단추 In App Menu Expander ID: InAppMenuExpander
> 		3 제목 표시줄
> 			4 메뉴 모음 시스템 ID: MenuBar
> 				5 메뉴 항목 시스템
> 			6 단추 최소화
> 			7 단추 최대화
> 			8 단추 닫기
> 		9 텍스트 언어
> 		10 콤보 상자 ID: ComboUiLanguage
> 		11 텍스트 LASAL 모션 제어 API 예제
> 		12 텍스트 모션 제어는 PLC에서 활성 상태입니다. 진단 제어는 PLC가 필요한 기능을 광고한 뒤에만 활성화됩니다. 값은 선택한 application UNIT을 사용하거나 raw DINT로 전달됩니다.
> 		13 그룹 연결 / RPC 콜백
> 			14 텍스트 연결 / RPC 콜백
> 			15 텍스트 PLC IP
> 			16 편집 (disabled) ID: TextRemoteIp
> 			17 텍스트 TCP 포트
> 			18 편집 (disabled) ID: TextRemotePort
> 			19 텍스트 PC 로컬 IPv4
> 			20 편집 (disabled) ID: TextLocalIp
> 			21 텍스트 콜백 UDP 포트
> 			22 편집 (disabled) ID: TextCallbackPort
> 			23 단추 연결 ID: ButtonConnect
> 				24 텍스트 연결
> 			25 단추 닫기 ID: ButtonCloseConnection
> 				26 텍스트 닫기
> 			27 텍스트 연결 상태
> 			28 텍스트 연결됨 ID: TextConnectionState
> 			29 텍스트 콜백 수신기
> 			30 텍스트 수신 대기 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			31 그룹 RPC 초기화 / 콜백 증거 ID: ExpanderRpcCallbackEvidence
> 				32 단추 RPC 초기화 / 콜백 증거 ID: HeaderSite
> 					33 텍스트 RPC 초기화 / 콜백 증거
> 				34 텍스트 RPC 초기화 증거
> 				35 텍스트 Attempt=3, Outcome=Connected, CandidateOrdinal=1, Remote=10.10.150.1:4000, Local=10.10.150.13, RequestedCallback=10.10.150.13:5000, Mode=Version2WakeHint, BoundCallback=10.10.150.13:5000, LocalSessionGeneration=1, 0x8080Attempts=1, Retry=False, InitOutcome=Succeeded, StartedUtc=2026-08-20T07:46:07.0018615Z, CompletedUtc=2026-08-20T07:46:07.0068642Z, LastACK={FrameValid=True, HeaderStatus=0, HeaderReserved=0, PayloadLength=24, HasCommandResult=False, CommandStatus=0, ErrorId=0}, Current=Active ID: TextRpcInitialization
> 				36 텍스트 콜백 v2 등록
> 				37 텍스트 Status=0, ErrorId=0, Version=2, MaxDatagram=52, BootId=0x00000042, SessionEpoch=27, Flags=0x00000000, Cookie=0x521F125D06FCDE46, ListenerGeneration=2, Source=10.10.150.1, EventMask=0x00000001, LocalSessionGeneration=1 ID: TextCallbackRegistration
> 				38 텍스트 PC 콜백 수신 증거
> 				39 텍스트 Accepted=1, Rejected=0, Duplicate=0, OutOfOrder=0 ID: TextCallbackCounters
> 				40 텍스트 Last decision=AcceptedWakeHint, ProtocolError=None ID: TextCallbackLastDecision
> 				41 텍스트 연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. UDP wake hint는 상태 판단 근거가 아니며, 동작 상태는 일치하는 TCP status 응답을 받은 뒤에만 변경됩니다.
> 		42 그룹 안전 / 복구 상세 정보 ID: ExpanderSafetyAndRecoveryDetails
> 			43 단추 안전 / 복구 상세 정보 ID: HeaderSite
> 				44 텍스트 안전 / 복구 상세 정보
> 		45 탭 ID: TabsMotion
> 			46 탭 항목 (selectable) 단축
> 				47 텍스트 단축
> 			48 탭 항목 (selectable) 그룹 모션
> 				49 텍스트 그룹 모션
> 			50 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				51 텍스트 EtherCAT / CREVIS / PI
> 			52 탭 항목 (selectable) Bulk 스냅샷
> 				53 텍스트 Bulk 스냅샷
> 			54 탭 항목 (selectable) 레코더
> 				55 텍스트 레코더
> 			56 탭 항목 (selectable) SDO / Write 정책
> 				57 텍스트 SDO / Write 정책
> 				58 창 ID: ScrollDiagnosticsOperations
> 					59 그룹 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19]) ID: GroupEncoderMaintenance
> 						60 텍스트 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])
> 						61 텍스트 파괴적 테스트 전용: TW[20]은 0x20FC:0x02에 UInt16 1을 씁니다. TW[19]는 0x20FC:0x01에 UInt16 1을 써 absolute multi-turn position을 reset하며 motion 전에 LMC Home이 필요합니다. 정확한 drive 축을 선택하십시오.
> 						62 텍스트 Fixed writes: TW[20]=0x20FC:0x02 <- UInt16 1; TW[19]=0x20FC:0x01 <- UInt16 1 ID: TextEncoderMaintenanceFixedTargets
> 						63 텍스트 Arm 전: 선택한 축의 Power를 끄고 안정적인 standstill과 실제 위치를 독립적으로 확인한 뒤 정확한 0x20FC 명령 지원 여부를 확인하십시오. 전용 0x7E53/0x7E54/0x7E55 경로는 일반 SDO Write와 분리됩니다.
> 						64 텍스트 작업
> 						65 콤보 상자 ID: ComboEncoderMaintenanceKind
> 						66 텍스트 Drive / 축 (1..4)
> 						67 콤보 상자 ID: ComboTestResetAxis
> 						68 텍스트 Timeout (ms, 1..60000)
> 						69 편집 ID: TextTestResetTimeout
> 						70 단추 Encoder 유지보수 Capability 새로고침 ID: ButtonRefreshTestResetCapabilities
> 							71 텍스트 Encoder 유지보수 Capability 새로고침
> 						72 단추 (disabled) Encoder 유지보수 결과 읽기 ID: ButtonReadTestResetStatus
> 							73 텍스트 (disabled) Encoder 유지보수 결과 읽기
> 						74 확인란 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다. ID: CheckTestResetPowerOffVerified
> 							75 텍스트 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다.
> 						76 확인란 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다. ID: CheckTestResetPhysicalPositionVerified
> 							77 텍스트 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다.
> 						78 확인란 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다. ID: CheckTestResetExactTargetVerified
> 							79 텍스트 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다.
> 						80 확인란 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다. ID: CheckEncoderMaintenanceCompatibilityVerified
> 							81 텍스트 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.
> 						82 텍스트 차단: 1단계의 모든 물리 확인과 encoder 호환성 확인이 필요합니다. Encoder 유지보수 작업을 arm하거나 전송하지 않았습니다. ID: TextEncoderMaintenanceArmGateStatus
> 						83 단추 안전 / 복구 상세 정보 열기 Description: 로컬 안전/복구 panel만 펼칩니다. 이 버튼은 PLC 명령을 전송하지 않으며 어떤 record도 폐기하지 않습니다. ID: ButtonOpenEncoderRecoveryDetails
> 							84 텍스트 안전 / 복구 상세 정보 열기
> 						85 단추 (disabled) 1단계 - Encoder 유지보수 Arm Description: Live mutation 승인, 현재 TW[20]/TW[19] capability, 사용 가능한 durable 유지보수 journal 및 1단계의 네 가지 확인이 필요합니다. Arm은 정확한 요청을 PC 메모리에만 준비하며 0x7E53을 전송하지 않습니다. ID: ButtonArmTestReset
> 							86 텍스트 (disabled) 1단계 - Encoder 유지보수 Arm
> 						87 확인란 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다. ID: CheckTestResetFinalConfirmed
> 							88 텍스트 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다.
> 						89 단추 (disabled) 2단계 - Encoder 유지보수 1회 실행 Description: 변경되지 않은 PC 전용 arm 요청, live mutation 승인, 현재 capability, 사용 가능한 durable 유지보수 journal 및 2단계 확인이 필요합니다. Execute는 0x7E53을 한 번 전송합니다. ID: ButtonExecuteTestReset
> 							90 텍스트 (disabled) 2단계 - Encoder 유지보수 1회 실행
> 						91 텍스트 Arm되거나 대기 중인 encoder 유지보수 작업이 없습니다. ID: TextTestResetResult
> 					92 그룹 저장된 Mutation 복구
> 						93 텍스트 저장된 Mutation 복구
> 						94 텍스트 Mutation journal 준비됨. 해결되지 않은 durable write record가 없습니다. ID: TextPersistedMutationStatus
> 						95 확인란 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다. ID: CheckPersistedMutationPhysicallyVerified
> 							96 텍스트 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.
> 						97 단추 (disabled) 복구된 Mutation 승인 ID: ButtonAcknowledgePersistedMutation
> 							98 텍스트 (disabled) 복구된 Mutation 승인
> 					99 그룹 비동기 SDO Read / Write ticket
> 						100 텍스트 비동기 SDO Read / Write ticket
> 						101 텍스트 작업
> 						102 콤보 상자 ID: ComboSdoOperation
> 						103 텍스트 슬레이브 참조 (1..4)
> 						104 편집 ID: TextSdoSlaveReference
> 						105 텍스트 객체 인덱스 (0x0001..0xFFFF)
> 						106 편집 ID: TextSdoIndex
> 						107 텍스트 서브인덱스 (0..255)
> 						108 편집 ID: TextSdoSubIndex
> 						109 텍스트 값 형식
> 						110 콤보 상자 ID: ComboSdoValueType
> 						111 텍스트 데이터 길이 (1, 2 또는 4)
> 						112 콤보 상자 ID: ComboSdoDataLength
> 						113 텍스트 Timeout (1..60000 cycle)
> 						114 편집 ID: TextSdoTimeoutCycles
> 						115 텍스트 Write 값 (10진수 또는 raw 0x hex)
> 						116 편집 (disabled) ID: TextSdoWriteData
> 						117 텍스트 SDK 승인 SDO Write target
> 						118 콤보 상자 (disabled) ID: ComboSdoWriteTarget
> 						119 단추 SDO Read 전송 Description: Write mode는 현재 session의 same-value 활성화 증명 뒤 2단계 확인을 사용합니다. Read mode는 추적되는 SDO Read 하나를 전송합니다. ID: ButtonSubmitSdo
> 							120 텍스트 SDO Read 전송
> 						121 단추 SDO Inline 읽기 (terminal 대기) Description: 일반 1/2/4-byte SDO Read 하나를 전송하고 terminal 결과를 기다립니다. SDO Write, 필수 Write readback 또는 CREVIS I/O는 실행하지 않습니다. ID: ButtonReadSdoInline
> 							122 텍스트 SDO Inline 읽기 (terminal 대기)
> 						123 단추 (disabled) Inline 대기 취소 (PC만) Description: PC 측 Inline Read 대기만 취소합니다. CancelOperation을 전송하거나 PLC ticket을 취소하거나 SDO request를 재시도하지 않습니다. ID: ButtonCancelSdoInlineWait
> 							124 텍스트 (disabled) Inline 대기 취소 (PC만)
> 						125 단추 (disabled) 필수 정확 Readback 불러오기 Description: 마지막으로 성공한 SDO Write를 확인하는 데 필요한 정확한 read request를 복원합니다. 명령은 전송하지 않습니다. ID: ButtonLoadRequiredSdoReadback
> 							126 텍스트 (disabled) 필수 정확 Readback 불러오기
> 						127 단추 Ticket 새로고침 ID: ButtonRefreshDiagnosticOperation
> 							128 텍스트 Ticket 새로고침
> 						129 단추 (disabled) 결과 다운로드 ID: ButtonDownloadSdoResult
> 							130 텍스트 (disabled) 결과 다운로드
> 						131 단추 결과 저장 ID: ButtonExportSdoResult
> 							132 텍스트 결과 저장
> 						133 단추 (disabled) Ticket 취소 ID: ButtonCancelDiagnosticOperation
> 							134 텍스트 (disabled) Ticket 취소
> 						135 텍스트 TicketId=135, Kind=SDORead, State=Completed, Outcome=Success SubmitCycle=13290199, CompletionCycle=13290221, ErrorId=0, Detail=0x00000000 ResultType=Int8, ResultLength=1, Data=08 ID: TextDiagnosticOperationSummary
> 					136 그룹 D5 SDO fault/recovery qualification (읽기 전용)
> 						137 텍스트 D5 SDO fault/recovery qualification (읽기 전용)
> 						138 텍스트 이 runner는 SDO를 쓰거나 EtherCAT fault를 주입하지 않습니다. Abort qualification은 제조사 승인된 존재하지 않는 읽기 전용 object/sub-index를 사용합니다. Contention, timeout, queued-cancel qualification은 0x6061:0 Int8/1만 사용합니다. Abrupt Disconnect는 RPC Close(0x405D)를 보내지 않고 API TCP transport만 의도적으로 끊은 뒤 재연결하여 0x6061:0을 정확히 두 번 읽습니다. 선택한 Slave 1..4는 _LMCAxis1..4에 매핑되며 모든 runner는 D5 ticket 전송 전에 PowerOn=False, Standstill=True 위치 sample 3회가 안정적으로 일치해야 합니다.
> 						139 텍스트 Abort PASS: 안정적인 BootId/MapRevision -> baseline -> 정확한 SDO abort -> 동일 값 복구. Contention PASS: 첫 0x6061 ticket 승인 -> 첫 terminal 전에 두 번째 Submit이 정확히 Rejected/ResourceBusy -> 첫 ticket Completed/Success -> 정확히 같은 첫 값으로 세 번째 별도 ticket Completed/Success. Timeout PASS: 0x6061 TimeoutCycles=1 -> 정확한 Expired/TimedOut -> 제한된 ResourceBusy drain 대기 -> 별도 동일 값 복구. Queued Cancel PASS: 0x6061 전송 -> 즉시 one-shot CancelOperation -> 정확한 Cancelled/Cancelled -> 별도 동일 값 복구. Abrupt Disconnect는 앱에서 확인 가능한 새 연결 복구만 PASS할 수 있으며 PLC MarkOrphan/late-callback 증거가 생길 때까지 orphanQualified는 false입니다. 연결 손실 전 terminal 완료 또는 Running race는 PLC orphan 증명에 대해 INCONCLUSIVE입니다. 예상 밖 승인 또는 불확실한 결과는 보존되어 자동 재시도를 차단합니다. Cancel Runner는 PLC Stop을 전송하지 않습니다.
> 						140 텍스트 대상 slave (1..4)
> 						141 편집 ID: TextD5SdoAbortSlaveReference
> 						142 텍스트 Abort 객체 인덱스
> 						143 편집 ID: TextD5SdoAbortIndex
> 						144 텍스트 Abort 서브인덱스
> 						145 편집 ID: TextD5SdoAbortSubIndex
> 						146 텍스트 Abort 값 형식
> 						147 콤보 상자 ID: ComboD5SdoAbortValueType
> 						148 텍스트 데이터 길이
> 						149 편집 ID: TextD5SdoAbortDataLength
> 						150 텍스트 일반/복구 timeout cycle
> 						151 편집 ID: TextD5SdoAbortTimeoutCycles
> 						152 단추 D5 Abort -> 복구 실행 ID: ButtonRunD5SdoAbortQualification
> 							153 텍스트 D5 Abort -> 복구 실행
> 						154 단추 D5 Contention -> 복구 실행 ID: ButtonRunD5SdoContentionQualification
> 							155 텍스트 D5 Contention -> 복구 실행
> 						156 단추 D5 Timeout -> 복구 실행 ID: ButtonRunD5SdoTimeoutQualification
> 							157 텍스트 D5 Timeout -> 복구 실행
> 						158 단추 D5 Queued Cancel -> 복구 실행 ID: ButtonRunD5SdoQueuedCancelQualification
> 							159 텍스트 D5 Queued Cancel -> 복구 실행
> 						160 단추 D5 Abrupt Disconnect -> 앱 복구 실행 ID: ButtonRunD5SdoDisconnectRecoveryQualification
> 							161 텍스트 D5 Abrupt Disconnect -> 앱 복구 실행
> 						162 단추 (disabled) Runner 취소 (PLC Stop 아님) ID: ButtonCancelD5SdoQualification
> 							163 텍스트 (disabled) Runner 취소 (PLC Stop 아님)
> 						164 단추 QTEST 로그 저장 ID: ButtonSaveD5SdoQualificationLog
> 							165 텍스트 QTEST 로그 저장
> 						166 진행률 표시줄 ID: ProgressD5SdoQualification
> 						167 텍스트 아직 qualification을 실행하지 않았습니다. ID: TextD5SdoQualificationProgress
> 						168 편집 ID: TextD5SdoQualificationSummary
> 							169 스크롤 막대 ID: HorizontalScrollBar
> 								170 단추 (disabled) ID: PART_LineLeftButton
> 								171 단추 (disabled) ID: PageLeft
> 								172 단추 (disabled) ID: PageRight
> 								173 엄지 (disabled)
> 								174 단추 (disabled) ID: PART_LineRightButton
> 					175 그룹 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						176 텍스트 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						177 텍스트 실제 WRITE: 이 runner는 승인된 target을 읽고 정확히 같은 4 byte를 쓴 뒤 정확한 readback을 요구합니다. Sentinel을 쓰지 않으며 자동 복원이나 재전송을 수행하지 않습니다.
> 						178 텍스트 PLC가 bit 8/9/13을 광고하고, 이 SDK build가 승인 target을 정확히 하나만 노출하고, durable mutation journal을 사용할 수 있고, 작업자 확인 4개가 모두 선택돼야 버튼이 fail-closed 상태에서 풀립니다. 확인 후 선택 축의 PowerOn=False, Standstill=True, 안정 위치를 다시 검사하고 target도 다시 읽어 baseline과 계속 일치해야 Write를 전송합니다.
> 						179 텍스트 SDK 승인 단일 target
> 						180 콤보 상자 ID: ComboD5SdoWriteQualificationTarget
> 						181 텍스트 Timeout (1..60000 cycle)
> 						182 편집 ID: TextD5SdoWriteQualificationTimeoutCycles
> 						183 확인란 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다. ID: CheckConfirmD5SdoWriteUi24Unused
> 							184 텍스트 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다.
> 						185 확인란 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다. ID: CheckConfirmD5SdoWriteOriginalRecorded
> 							186 텍스트 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다.
> 						187 확인란 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다. ID: CheckConfirmD5SdoWriteCaptureRunning
> 							188 텍스트 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다.
> 						189 확인란 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다. ID: CheckConfirmD5SdoWriteSingleWriter
> 							190 텍스트 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다.
> 						191 단추 (disabled) 동일 값 SDO Write Qualification 실행 Description: CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: ButtonRunD5SdoWriteSameValueQualification
> 							192 텍스트 (disabled) 동일 값 SDO Write Qualification 실행
> 						193 텍스트 준비 상태 matrix / 다음 시도 (cache, wire 전송 없음)
> 						194 텍스트 OVERALL CLOSED | EVALUATION_WIRE=NONE SDK POLICY FAIL | blockers=CapabilityObservationNotCurrent | targets=1 PLC CAPS CACHED | bit8/read=1 bit9/write=1 bit13/general=1 IDENTITY PASS | BootId=0x00000042 MapRevision=0x957F101E PAYLOAD PASS | SDO=4 req=1320 resp=2040 cycleUs=1000 QUAL TARGET PASS | selected=Reserved diagnostic UI[24] | Slave 1 | 0x2F00:24 | Int32 | -1073741823..1073741823 RUNNER PASS | connected=True idle=True motionClear=True manualTicketClear=True mutationInterlockClear=True JOURNAL PASS CONFIRMATIONS FAIL | 0/4 AXIS PROOF PENDING_RUNNER_PREFLIGHT | PowerOn=False, Standstill=True, stable position are not cached here. MANUAL WRITE CLOSED_RUN_SAME_VALUE_FIRST | proof never transfers across connection sessions or PLC identity changes. NEXT CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: TextD5SdoWriteQualificationGateStatus
> 						195 텍스트 마지막 시도 / 현재 실행
> 						196 텍스트 아직 동일 값 SDO Write qualification을 실행하지 않았습니다. ID: TextD5SdoWriteQualificationSummary
> 					197 그룹 PI Write 정책 gate
> 						198 텍스트 PI Write 정책 gate
> 						199 텍스트 EtherCAT / PI 탭에서 WritableByPolicy Catalog 항목을 정확히 하나 선택하십시오. Schema v1이 payload를 정의하고 compile-time allowlist가 승인될 때까지 호출은 fail-closed입니다. DS402 ControlWord와 target object는 영구 차단됩니다.
> 						200 텍스트 Raw 값 (10진수 또는 0x...)
> 						201 편집 (disabled) ID: TextPiWriteRawValue
> 						202 단추 (disabled) PI Write 전송 ID: ButtonSubmitPiWrite
> 							203 텍스트 (disabled) PI Write 전송
> 					204 스크롤 막대 ID: VerticalScrollBar
> 						205 단추 (disabled) ID: PART_LineUpButton
> 						206 단추 (disabled) ID: PageUp
> 						207 단추 (disabled) ID: PageDown
> 						208 엄지 (disabled)
> 						209 단추 (disabled) ID: PART_LineDownButton
> 			210 탭 항목 (selectable) 읽기 전용 API
> 				211 텍스트 읽기 전용 API
> 		212 그룹 실행 로그 / 콜백 진단 ID: ExpanderExecutionLog
> 			213 단추 실행 로그 / 콜백 진단 ID: HeaderSite
> 				214 텍스트 실행 로그 / 콜백 진단
> 			215 단추 로그 복사
> 				216 텍스트 로그 복사
> 			217 단추 로그 지우기
> 				218 텍스트 로그 지우기
> 			219 편집 ID: TextExecutionLog
> 				220 스크롤 막대 ID: VerticalScrollBar
> 					221 단추 (disabled) ID: PART_LineUpButton
> 					222 단추 (disabled) ID: PageUp
> 					223 단추 (disabled) ID: PageDown
> 					224 엄지 (disabled)
> 					225 단추 (disabled) ID: PART_LineDownButton
> 				226 스크롤 막대 ID: HorizontalScrollBar
> 					227 단추 (disabled) ID: PART_LineLeftButton
> 					228 단추 (disabled) ID: PageLeft
> 					229 단추 (disabled) ID: PageRight
> 					230 엄지 (disabled)
> 					231 단추 (disabled) ID: PART_LineRightButton
> 		232 상태 표시줄 ID: StatusOperationBar
> 			233 텍스트 Log copied ID: TextOperationState
>
> Document text: ```
> [16:39:10.444] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-20 07:38:58 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-20 07:38:55 UTC.
> [16:39:10.445] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [16:45:55.685] Connect started.
> [16:45:55.773] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:45:55.785] Ignored stale connection-state event Connecting from an older transport session.
> [16:45:55.785] Connection state Connecting -> Connected
> [16:45:55.893] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:45:55.904] Connect PASS.
> [16:45:58.785] Close Connection started.
> [16:45:58.789] Connection state Connected -> Closing
> [16:45:58.805] Close Connection PASS.
> [16:46:01.250] Connect started.
> [16:46:01.252] Connection state Disconnected -> Connecting
> [16:46:01.264] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:01.265] Connection state Connecting -> Connected
> [16:46:01.339] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:01.339] Connect PASS.
> [16:46:02.611] Close Connection started.
> [16:46:02.611] Connection state Connected -> Closing
> [16:46:02.621] Close Connection PASS.
> [16:46:06.999] Connect started.
> [16:46:07.001] Connection state Disconnected -> Connecting
> [16:46:07.011] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:07.012] Connection state Connecting -> Connected
> [16:46:07.084] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:07.084] Connect PASS.
> [16:47:58.941] Submit SDO Read started.
> [16:47:58.966] QTEST|utc=2026-08-20T07:47:58.966Z|elapsedMs=0|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [16:47:58.973] QTEST|utc=2026-08-20T07:47:58.973Z|elapsedMs=8|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [16:47:58.981] QTEST|utc=2026-08-20T07:47:58.981Z|elapsedMs=16|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [16:47:59.055] QTEST|utc=2026-08-20T07:47:59.055Z|elapsedMs=91|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=135|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [16:47:59.057] QTEST|utc=2026-08-20T07:47:59.057Z|elapsedMs=93|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|ticket=135|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=135|quarantine=false
> [16:47:59.060] Submit SDO Read PASS.
> [16:47:59.096] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000087
> [16:47:59.117] QTEST|utc=2026-08-20T07:47:59.117Z|elapsedMs=151|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=135|state=Completed|outcome=Success|verdict=CLEARED
> [16:47:59.118] QTEST|utc=2026-08-20T07:47:59.118Z|elapsedMs=152|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [16:47:59.124] Callback D5 authoritative TCP status processed. TicketId=0x00000087
> ```
> ````
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "SDO 값 형식 선택 상태 확인",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"No accessibility observation\");\n  globalThis.state = null;\n  await sky.click({ window: observation.window, element_index: 110 });\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ````text
> Window: "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write] Secondary Actions: Raise
> 		1 창 Secondary Actions: Raise
> 			2 단추 In App Menu Expander ID: InAppMenuExpander
> 		3 제목 표시줄
> 			4 메뉴 모음 시스템 ID: MenuBar
> 				5 메뉴 항목 시스템
> 			6 단추 최소화
> 			7 단추 최대화
> 			8 단추 닫기
> 		9 텍스트 언어
> 		10 콤보 상자 ID: ComboUiLanguage
> 		11 텍스트 LASAL 모션 제어 API 예제
> 		12 텍스트 모션 제어는 PLC에서 활성 상태입니다. 진단 제어는 PLC가 필요한 기능을 광고한 뒤에만 활성화됩니다. 값은 선택한 application UNIT을 사용하거나 raw DINT로 전달됩니다.
> 		13 그룹 연결 / RPC 콜백
> 			14 텍스트 연결 / RPC 콜백
> 			15 텍스트 PLC IP
> 			16 편집 (disabled) ID: TextRemoteIp
> 			17 텍스트 TCP 포트
> 			18 편집 (disabled) ID: TextRemotePort
> 			19 텍스트 PC 로컬 IPv4
> 			20 편집 (disabled) ID: TextLocalIp
> 			21 텍스트 콜백 UDP 포트
> 			22 편집 (disabled) ID: TextCallbackPort
> 			23 단추 연결 ID: ButtonConnect
> 				24 텍스트 연결
> 			25 단추 닫기 ID: ButtonCloseConnection
> 				26 텍스트 닫기
> 			27 텍스트 연결 상태
> 			28 텍스트 연결됨 ID: TextConnectionState
> 			29 텍스트 콜백 수신기
> 			30 텍스트 수신 대기 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			31 그룹 RPC 초기화 / 콜백 증거 ID: ExpanderRpcCallbackEvidence
> 				32 단추 RPC 초기화 / 콜백 증거 ID: HeaderSite
> 					33 텍스트 RPC 초기화 / 콜백 증거
> 				34 텍스트 RPC 초기화 증거
> 				35 텍스트 Attempt=3, Outcome=Connected, CandidateOrdinal=1, Remote=10.10.150.1:4000, Local=10.10.150.13, RequestedCallback=10.10.150.13:5000, Mode=Version2WakeHint, BoundCallback=10.10.150.13:5000, LocalSessionGeneration=1, 0x8080Attempts=1, Retry=False, InitOutcome=Succeeded, StartedUtc=2026-08-20T07:46:07.0018615Z, CompletedUtc=2026-08-20T07:46:07.0068642Z, LastACK={FrameValid=True, HeaderStatus=0, HeaderReserved=0, PayloadLength=24, HasCommandResult=False, CommandStatus=0, ErrorId=0}, Current=Active ID: TextRpcInitialization
> 				36 텍스트 콜백 v2 등록
> 				37 텍스트 Status=0, ErrorId=0, Version=2, MaxDatagram=52, BootId=0x00000042, SessionEpoch=27, Flags=0x00000000, Cookie=0x521F125D06FCDE46, ListenerGeneration=2, Source=10.10.150.1, EventMask=0x00000001, LocalSessionGeneration=1 ID: TextCallbackRegistration
> 				38 텍스트 PC 콜백 수신 증거
> 				39 텍스트 Accepted=1, Rejected=0, Duplicate=0, OutOfOrder=0 ID: TextCallbackCounters
> 				40 텍스트 Last decision=AcceptedWakeHint, ProtocolError=None ID: TextCallbackLastDecision
> 				41 텍스트 연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. UDP wake hint는 상태 판단 근거가 아니며, 동작 상태는 일치하는 TCP status 응답을 받은 뒤에만 변경됩니다.
> 		42 그룹 안전 / 복구 상세 정보 ID: ExpanderSafetyAndRecoveryDetails
> 			43 단추 안전 / 복구 상세 정보 ID: HeaderSite
> 				44 텍스트 안전 / 복구 상세 정보
> 		45 탭 ID: TabsMotion
> 			46 탭 항목 (selectable) 단축
> 				47 텍스트 단축
> 			48 탭 항목 (selectable) 그룹 모션
> 				49 텍스트 그룹 모션
> 			50 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				51 텍스트 EtherCAT / CREVIS / PI
> 			52 탭 항목 (selectable) Bulk 스냅샷
> 				53 텍스트 Bulk 스냅샷
> 			54 탭 항목 (selectable) 레코더
> 				55 텍스트 레코더
> 			56 탭 항목 (selectable) SDO / Write 정책
> 				57 텍스트 SDO / Write 정책
> 				58 창 ID: ScrollDiagnosticsOperations
> 					59 그룹 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19]) ID: GroupEncoderMaintenance
> 						60 텍스트 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])
> 						61 텍스트 파괴적 테스트 전용: TW[20]은 0x20FC:0x02에 UInt16 1을 씁니다. TW[19]는 0x20FC:0x01에 UInt16 1을 써 absolute multi-turn position을 reset하며 motion 전에 LMC Home이 필요합니다. 정확한 drive 축을 선택하십시오.
> 						62 텍스트 Fixed writes: TW[20]=0x20FC:0x02 <- UInt16 1; TW[19]=0x20FC:0x01 <- UInt16 1 ID: TextEncoderMaintenanceFixedTargets
> 						63 텍스트 Arm 전: 선택한 축의 Power를 끄고 안정적인 standstill과 실제 위치를 독립적으로 확인한 뒤 정확한 0x20FC 명령 지원 여부를 확인하십시오. 전용 0x7E53/0x7E54/0x7E55 경로는 일반 SDO Write와 분리됩니다.
> 						64 텍스트 작업
> 						65 콤보 상자 ID: ComboEncoderMaintenanceKind
> 						66 텍스트 Drive / 축 (1..4)
> 						67 콤보 상자 ID: ComboTestResetAxis
> 						68 텍스트 Timeout (ms, 1..60000)
> 						69 편집 ID: TextTestResetTimeout
> 						70 단추 Encoder 유지보수 Capability 새로고침 ID: ButtonRefreshTestResetCapabilities
> 							71 텍스트 Encoder 유지보수 Capability 새로고침
> 						72 단추 (disabled) Encoder 유지보수 결과 읽기 ID: ButtonReadTestResetStatus
> 							73 텍스트 (disabled) Encoder 유지보수 결과 읽기
> 						74 확인란 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다. ID: CheckTestResetPowerOffVerified
> 							75 텍스트 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다.
> 						76 확인란 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다. ID: CheckTestResetPhysicalPositionVerified
> 							77 텍스트 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다.
> 						78 확인란 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다. ID: CheckTestResetExactTargetVerified
> 							79 텍스트 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다.
> 						80 확인란 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다. ID: CheckEncoderMaintenanceCompatibilityVerified
> 							81 텍스트 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.
> 						82 텍스트 차단: 1단계의 모든 물리 확인과 encoder 호환성 확인이 필요합니다. Encoder 유지보수 작업을 arm하거나 전송하지 않았습니다. ID: TextEncoderMaintenanceArmGateStatus
> 						83 단추 안전 / 복구 상세 정보 열기 Description: 로컬 안전/복구 panel만 펼칩니다. 이 버튼은 PLC 명령을 전송하지 않으며 어떤 record도 폐기하지 않습니다. ID: ButtonOpenEncoderRecoveryDetails
> 							84 텍스트 안전 / 복구 상세 정보 열기
> 						85 단추 (disabled) 1단계 - Encoder 유지보수 Arm Description: Live mutation 승인, 현재 TW[20]/TW[19] capability, 사용 가능한 durable 유지보수 journal 및 1단계의 네 가지 확인이 필요합니다. Arm은 정확한 요청을 PC 메모리에만 준비하며 0x7E53을 전송하지 않습니다. ID: ButtonArmTestReset
> 							86 텍스트 (disabled) 1단계 - Encoder 유지보수 Arm
> 						87 확인란 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다. ID: CheckTestResetFinalConfirmed
> 							88 텍스트 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다.
> 						89 단추 (disabled) 2단계 - Encoder 유지보수 1회 실행 Description: 변경되지 않은 PC 전용 arm 요청, live mutation 승인, 현재 capability, 사용 가능한 durable 유지보수 journal 및 2단계 확인이 필요합니다. Execute는 0x7E53을 한 번 전송합니다. ID: ButtonExecuteTestReset
> 							90 텍스트 (disabled) 2단계 - Encoder 유지보수 1회 실행
> 						91 텍스트 Arm되거나 대기 중인 encoder 유지보수 작업이 없습니다. ID: TextTestResetResult
> 					92 그룹 저장된 Mutation 복구
> 						93 텍스트 저장된 Mutation 복구
> 						94 텍스트 Mutation journal 준비됨. 해결되지 않은 durable write record가 없습니다. ID: TextPersistedMutationStatus
> 						95 확인란 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다. ID: CheckPersistedMutationPhysicallyVerified
> 							96 텍스트 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.
> 						97 단추 (disabled) 복구된 Mutation 승인 ID: ButtonAcknowledgePersistedMutation
> 							98 텍스트 (disabled) 복구된 Mutation 승인
> 					99 그룹 비동기 SDO Read / Write ticket
> 						100 텍스트 비동기 SDO Read / Write ticket
> 						101 텍스트 작업
> 						102 콤보 상자 ID: ComboSdoOperation
> 						103 텍스트 슬레이브 참조 (1..4)
> 						104 편집 ID: TextSdoSlaveReference
> 						105 텍스트 객체 인덱스 (0x0001..0xFFFF)
> 						106 편집 ID: TextSdoIndex
> 						107 텍스트 서브인덱스 (0..255)
> 						108 편집 ID: TextSdoSubIndex
> 						109 텍스트 값 형식
> 						110 콤보 상자 ID: ComboSdoValueType
> 						111 텍스트 데이터 길이 (1, 2 또는 4)
> 						112 콤보 상자 ID: ComboSdoDataLength
> 						113 텍스트 Timeout (1..60000 cycle)
> 						114 편집 ID: TextSdoTimeoutCycles
> 						115 텍스트 Write 값 (10진수 또는 raw 0x hex)
> 						116 편집 (disabled) ID: TextSdoWriteData
> 						117 텍스트 SDK 승인 SDO Write target
> 						118 콤보 상자 (disabled) ID: ComboSdoWriteTarget
> 						119 단추 SDO Read 전송 Description: Write mode는 현재 session의 same-value 활성화 증명 뒤 2단계 확인을 사용합니다. Read mode는 추적되는 SDO Read 하나를 전송합니다. ID: ButtonSubmitSdo
> 							120 텍스트 SDO Read 전송
> 						121 단추 SDO Inline 읽기 (terminal 대기) Description: 일반 1/2/4-byte SDO Read 하나를 전송하고 terminal 결과를 기다립니다. SDO Write, 필수 Write readback 또는 CREVIS I/O는 실행하지 않습니다. ID: ButtonReadSdoInline
> 							122 텍스트 SDO Inline 읽기 (terminal 대기)
> 						123 단추 (disabled) Inline 대기 취소 (PC만) Description: PC 측 Inline Read 대기만 취소합니다. CancelOperation을 전송하거나 PLC ticket을 취소하거나 SDO request를 재시도하지 않습니다. ID: ButtonCancelSdoInlineWait
> 							124 텍스트 (disabled) Inline 대기 취소 (PC만)
> 						125 단추 (disabled) 필수 정확 Readback 불러오기 Description: 마지막으로 성공한 SDO Write를 확인하는 데 필요한 정확한 read request를 복원합니다. 명령은 전송하지 않습니다. ID: ButtonLoadRequiredSdoReadback
> 							126 텍스트 (disabled) 필수 정확 Readback 불러오기
> 						127 단추 Ticket 새로고침 ID: ButtonRefreshDiagnosticOperation
> 							128 텍스트 Ticket 새로고침
> 						129 단추 (disabled) 결과 다운로드 ID: ButtonDownloadSdoResult
> 							130 텍스트 (disabled) 결과 다운로드
> 						131 단추 결과 저장 ID: ButtonExportSdoResult
> 							132 텍스트 결과 저장
> 						133 단추 (disabled) Ticket 취소 ID: ButtonCancelDiagnosticOperation
> 							134 텍스트 (disabled) Ticket 취소
> 						135 텍스트 TicketId=135, Kind=SDORead, State=Completed, Outcome=Success SubmitCycle=13290199, CompletionCycle=13290221, ErrorId=0, Detail=0x00000000 ResultType=Int8, ResultLength=1, Data=08 ID: TextDiagnosticOperationSummary
> 					136 그룹 D5 SDO fault/recovery qualification (읽기 전용)
> 						137 텍스트 D5 SDO fault/recovery qualification (읽기 전용)
> 						138 텍스트 이 runner는 SDO를 쓰거나 EtherCAT fault를 주입하지 않습니다. Abort qualification은 제조사 승인된 존재하지 않는 읽기 전용 object/sub-index를 사용합니다. Contention, timeout, queued-cancel qualification은 0x6061:0 Int8/1만 사용합니다. Abrupt Disconnect는 RPC Close(0x405D)를 보내지 않고 API TCP transport만 의도적으로 끊은 뒤 재연결하여 0x6061:0을 정확히 두 번 읽습니다. 선택한 Slave 1..4는 _LMCAxis1..4에 매핑되며 모든 runner는 D5 ticket 전송 전에 PowerOn=False, Standstill=True 위치 sample 3회가 안정적으로 일치해야 합니다.
> 						139 텍스트 Abort PASS: 안정적인 BootId/MapRevision -> baseline -> 정확한 SDO abort -> 동일 값 복구. Contention PASS: 첫 0x6061 ticket 승인 -> 첫 terminal 전에 두 번째 Submit이 정확히 Rejected/ResourceBusy -> 첫 ticket Completed/Success -> 정확히 같은 첫 값으로 세 번째 별도 ticket Completed/Success. Timeout PASS: 0x6061 TimeoutCycles=1 -> 정확한 Expired/TimedOut -> 제한된 ResourceBusy drain 대기 -> 별도 동일 값 복구. Queued Cancel PASS: 0x6061 전송 -> 즉시 one-shot CancelOperation -> 정확한 Cancelled/Cancelled -> 별도 동일 값 복구. Abrupt Disconnect는 앱에서 확인 가능한 새 연결 복구만 PASS할 수 있으며 PLC MarkOrphan/late-callback 증거가 생길 때까지 orphanQualified는 false입니다. 연결 손실 전 terminal 완료 또는 Running race는 PLC orphan 증명에 대해 INCONCLUSIVE입니다. 예상 밖 승인 또는 불확실한 결과는 보존되어 자동 재시도를 차단합니다. Cancel Runner는 PLC Stop을 전송하지 않습니다.
> 						140 텍스트 대상 slave (1..4)
> 						141 편집 ID: TextD5SdoAbortSlaveReference
> 						142 텍스트 Abort 객체 인덱스
> 						143 편집 ID: TextD5SdoAbortIndex
> 						144 텍스트 Abort 서브인덱스
> 						145 편집 ID: TextD5SdoAbortSubIndex
> 						146 텍스트 Abort 값 형식
> 						147 콤보 상자 ID: ComboD5SdoAbortValueType
> 						148 텍스트 데이터 길이
> 						149 편집 ID: TextD5SdoAbortDataLength
> 						150 텍스트 일반/복구 timeout cycle
> 						151 편집 ID: TextD5SdoAbortTimeoutCycles
> 						152 단추 D5 Abort -> 복구 실행 ID: ButtonRunD5SdoAbortQualification
> 							153 텍스트 D5 Abort -> 복구 실행
> 						154 단추 D5 Contention -> 복구 실행 ID: ButtonRunD5SdoContentionQualification
> 							155 텍스트 D5 Contention -> 복구 실행
> 						156 단추 D5 Timeout -> 복구 실행 ID: ButtonRunD5SdoTimeoutQualification
> 							157 텍스트 D5 Timeout -> 복구 실행
> 						158 단추 D5 Queued Cancel -> 복구 실행 ID: ButtonRunD5SdoQueuedCancelQualification
> 							159 텍스트 D5 Queued Cancel -> 복구 실행
> 						160 단추 D5 Abrupt Disconnect -> 앱 복구 실행 ID: ButtonRunD5SdoDisconnectRecoveryQualification
> 							161 텍스트 D5 Abrupt Disconnect -> 앱 복구 실행
> 						162 단추 (disabled) Runner 취소 (PLC Stop 아님) ID: ButtonCancelD5SdoQualification
> 							163 텍스트 (disabled) Runner 취소 (PLC Stop 아님)
> 						164 단추 QTEST 로그 저장 ID: ButtonSaveD5SdoQualificationLog
> 							165 텍스트 QTEST 로그 저장
> 						166 진행률 표시줄 ID: ProgressD5SdoQualification
> 						167 텍스트 아직 qualification을 실행하지 않았습니다. ID: TextD5SdoQualificationProgress
> 						168 편집 ID: TextD5SdoQualificationSummary
> 							169 스크롤 막대 ID: HorizontalScrollBar
> 								170 단추 (disabled) ID: PART_LineLeftButton
> 								171 단추 (disabled) ID: PageLeft
> 								172 단추 (disabled) ID: PageRight
> 								173 엄지 (disabled)
> 								174 단추 (disabled) ID: PART_LineRightButton
> 					175 그룹 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						176 텍스트 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						177 텍스트 실제 WRITE: 이 runner는 승인된 target을 읽고 정확히 같은 4 byte를 쓴 뒤 정확한 readback을 요구합니다. Sentinel을 쓰지 않으며 자동 복원이나 재전송을 수행하지 않습니다.
> 						178 텍스트 PLC가 bit 8/9/13을 광고하고, 이 SDK build가 승인 target을 정확히 하나만 노출하고, durable mutation journal을 사용할 수 있고, 작업자 확인 4개가 모두 선택돼야 버튼이 fail-closed 상태에서 풀립니다. 확인 후 선택 축의 PowerOn=False, Standstill=True, 안정 위치를 다시 검사하고 target도 다시 읽어 baseline과 계속 일치해야 Write를 전송합니다.
> 						179 텍스트 SDK 승인 단일 target
> 						180 콤보 상자 ID: ComboD5SdoWriteQualificationTarget
> 						181 텍스트 Timeout (1..60000 cycle)
> 						182 편집 ID: TextD5SdoWriteQualificationTimeoutCycles
> 						183 확인란 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다. ID: CheckConfirmD5SdoWriteUi24Unused
> 							184 텍스트 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다.
> 						185 확인란 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다. ID: CheckConfirmD5SdoWriteOriginalRecorded
> 							186 텍스트 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다.
> 						187 확인란 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다. ID: CheckConfirmD5SdoWriteCaptureRunning
> 							188 텍스트 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다.
> 						189 확인란 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다. ID: CheckConfirmD5SdoWriteSingleWriter
> 							190 텍스트 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다.
> 						191 단추 (disabled) 동일 값 SDO Write Qualification 실행 Description: CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: ButtonRunD5SdoWriteSameValueQualification
> 							192 텍스트 (disabled) 동일 값 SDO Write Qualification 실행
> 						193 텍스트 준비 상태 matrix / 다음 시도 (cache, wire 전송 없음)
> 						194 텍스트 OVERALL CLOSED | EVALUATION_WIRE=NONE SDK POLICY FAIL | blockers=CapabilityObservationNotCurrent | targets=1 PLC CAPS CACHED | bit8/read=1 bit9/write=1 bit13/general=1 IDENTITY PASS | BootId=0x00000042 MapRevision=0x957F101E PAYLOAD PASS | SDO=4 req=1320 resp=2040 cycleUs=1000 QUAL TARGET PASS | selected=Reserved diagnostic UI[24] | Slave 1 | 0x2F00:24 | Int32 | -1073741823..1073741823 RUNNER PASS | connected=True idle=True motionClear=True manualTicketClear=True mutationInterlockClear=True JOURNAL PASS CONFIRMATIONS FAIL | 0/4 AXIS PROOF PENDING_RUNNER_PREFLIGHT | PowerOn=False, Standstill=True, stable position are not cached here. MANUAL WRITE CLOSED_RUN_SAME_VALUE_FIRST | proof never transfers across connection sessions or PLC identity changes. NEXT CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: TextD5SdoWriteQualificationGateStatus
> 						195 텍스트 마지막 시도 / 현재 실행
> 						196 텍스트 아직 동일 값 SDO Write qualification을 실행하지 않았습니다. ID: TextD5SdoWriteQualificationSummary
> 					197 그룹 PI Write 정책 gate
> 						198 텍스트 PI Write 정책 gate
> 						199 텍스트 EtherCAT / PI 탭에서 WritableByPolicy Catalog 항목을 정확히 하나 선택하십시오. Schema v1이 payload를 정의하고 compile-time allowlist가 승인될 때까지 호출은 fail-closed입니다. DS402 ControlWord와 target object는 영구 차단됩니다.
> 						200 텍스트 Raw 값 (10진수 또는 0x...)
> 						201 편집 (disabled) ID: TextPiWriteRawValue
> 						202 단추 (disabled) PI Write 전송 ID: ButtonSubmitPiWrite
> 							203 텍스트 (disabled) PI Write 전송
> 					204 스크롤 막대 ID: VerticalScrollBar
> 						205 단추 (disabled) ID: PART_LineUpButton
> 						206 단추 (disabled) ID: PageUp
> 						207 단추 (disabled) ID: PageDown
> 						208 엄지 (disabled)
> 						209 단추 (disabled) ID: PART_LineDownButton
> 			210 탭 항목 (selectable) 읽기 전용 API
> 				211 텍스트 읽기 전용 API
> 		212 그룹 실행 로그 / 콜백 진단 ID: ExpanderExecutionLog
> 			213 단추 실행 로그 / 콜백 진단 ID: HeaderSite
> 				214 텍스트 실행 로그 / 콜백 진단
> 			215 단추 로그 복사
> 				216 텍스트 로그 복사
> 			217 단추 로그 지우기
> 				218 텍스트 로그 지우기
> 			219 편집 ID: TextExecutionLog
> 				220 스크롤 막대 ID: VerticalScrollBar
> 					221 단추 (disabled) ID: PART_LineUpButton
> 					222 단추 (disabled) ID: PageUp
> 					223 단추 (disabled) ID: PageDown
> 					224 엄지 (disabled)
> 					225 단추 (disabled) ID: PART_LineDownButton
> 				226 스크롤 막대 ID: HorizontalScrollBar
> 					227 단추 (disabled) ID: PART_LineLeftButton
> 					228 단추 (disabled) ID: PageLeft
> 					229 단추 (disabled) ID: PageRight
> 					230 엄지 (disabled)
> 					231 단추 (disabled) ID: PART_LineRightButton
> 		232 상태 표시줄 ID: StatusOperationBar
> 			233 텍스트 Log copied ID: TextOperationState
>
> Document text: ```
> [16:39:10.444] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-20 07:38:58 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-20 07:38:55 UTC.
> [16:39:10.445] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [16:45:55.685] Connect started.
> [16:45:55.773] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:45:55.785] Ignored stale connection-state event Connecting from an older transport session.
> [16:45:55.785] Connection state Connecting -> Connected
> [16:45:55.893] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:45:55.904] Connect PASS.
> [16:45:58.785] Close Connection started.
> [16:45:58.789] Connection state Connected -> Closing
> [16:45:58.805] Close Connection PASS.
> [16:46:01.250] Connect started.
> [16:46:01.252] Connection state Disconnected -> Connecting
> [16:46:01.264] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:01.265] Connection state Connecting -> Connected
> [16:46:01.339] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:01.339] Connect PASS.
> [16:46:02.611] Close Connection started.
> [16:46:02.611] Connection state Connected -> Closing
> [16:46:02.621] Close Connection PASS.
> [16:46:06.999] Connect started.
> [16:46:07.001] Connection state Disconnected -> Connecting
> [16:46:07.011] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:07.012] Connection state Connecting -> Connected
> [16:46:07.084] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:07.084] Connect PASS.
> [16:47:58.941] Submit SDO Read started.
> [16:47:58.966] QTEST|utc=2026-08-20T07:47:58.966Z|elapsedMs=0|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [16:47:58.973] QTEST|utc=2026-08-20T07:47:58.973Z|elapsedMs=8|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [16:47:58.981] QTEST|utc=2026-08-20T07:47:58.981Z|elapsedMs=16|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [16:47:59.055] QTEST|utc=2026-08-20T07:47:59.055Z|elapsedMs=91|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=135|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [16:47:59.057] QTEST|utc=2026-08-20T07:47:59.057Z|elapsedMs=93|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|ticket=135|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=135|quarantine=false
> [16:47:59.060] Submit SDO Read PASS.
> [16:47:59.096] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000087
> [16:47:59.117] QTEST|utc=2026-08-20T07:47:59.117Z|elapsedMs=151|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=135|state=Completed|outcome=Success|verdict=CLEARED
> [16:47:59.118] QTEST|utc=2026-08-20T07:47:59.118Z|elapsedMs=152|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [16:47:59.124] Callback D5 authoritative TCP status processed. TicketId=0x00000087
> ```
> ````
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "GD-01 슬레이브 입력을 1로 고정",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"No accessibility observation\");\n  globalThis.state = null;\n  await sky.set_value({ window: observation.window, element_index: 104, value: \"1\" });\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: false,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || state.accessibility?.tree || \"\"));\n}"
> }
> ```
>
> ```text
> read UIA value read-only state: 요청한 속성이 CacheRequest에 없습니다. (0x80070057)
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "실패 후 WPF 입력 상태 재확인",
>   "code": "{\n  globalThis.state = await sky.get_window_state({\n    window: targetWindow,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || state.accessibility?.tree || \"\"));\n}"
> }
> ```
>
> ```text
> 16 편집 (disabled) ID: TextRemoteIp
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "시험 입력 전체가 보이도록 WPF 창 최대화",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"No accessibility observation\");\n  globalThis.state = null;\n  await sky.click({ window: observation.window, element_index: 7 });\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || \"\"));\n}"
> }
> ```
>
> ```text
> 16 편집 (disabled) ID: TextRemoteIp
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> </details>
>
> 입력값은 화면에서 직접 확인했습니다: `Read / Slave 1 / 0x6061:0 / Int8 / 1 byte / timeout 1000`, 현재 ticket은 완료 상태라 pending 작업도 없습니다. 참고 가치가 있도록 새 캡처 안에서 `GD-01 1회 → 같은 창 Clean Close/Reconnect → GD-01 1회`를 수행해 재연결 전·후 callback을 한 번에 비교하겠습니다. 전부 읽기 전용입니다.
>
> <details><summary>Called 4 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "재연결 전 GD-01 읽기 1회 실행",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"No accessibility observation\");\n  const tree = String(observation.accessibility.tree || \"\");\n  const match = tree.match(/\\n\\s*(\\d+) 단추 SDO Read 전송/);\n  if (!match) throw new Error(\"SDO Read submit button not found in current observation\");\n  const submitIndex = Number(match[1]);\n  globalThis.state = null;\n  await sky.click({ window: observation.window, element_index: submitIndex });\n  await new Promise((resolve) => setTimeout(resolve, 800));\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ````text
> Window: "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write] Secondary Actions: Raise
> 		1 창 Secondary Actions: Raise
> 			2 단추 In App Menu Expander ID: InAppMenuExpander
> 		3 제목 표시줄
> 			4 메뉴 모음 시스템 ID: MenuBar
> 				5 메뉴 항목 시스템
> 			6 단추 최소화
> 			7 단추 복원
> 			8 단추 닫기
> 		9 텍스트 언어
> 		10 콤보 상자 ID: ComboUiLanguage
> 		11 텍스트 LASAL 모션 제어 API 예제
> 		12 텍스트 모션 제어는 PLC에서 활성 상태입니다. 진단 제어는 PLC가 필요한 기능을 광고한 뒤에만 활성화됩니다. 값은 선택한 application UNIT을 사용하거나 raw DINT로 전달됩니다.
> 		13 그룹 연결 / RPC 콜백
> 			14 텍스트 연결 / RPC 콜백
> 			15 텍스트 PLC IP
> 			16 편집 (disabled) ID: TextRemoteIp
> 			17 텍스트 TCP 포트
> 			18 편집 (disabled) ID: TextRemotePort
> 			19 텍스트 PC 로컬 IPv4
> 			20 편집 (disabled) ID: TextLocalIp
> 			21 텍스트 콜백 UDP 포트
> 			22 편집 (disabled) ID: TextCallbackPort
> 			23 단추 연결 ID: ButtonConnect
> 				24 텍스트 연결
> 			25 단추 닫기 ID: ButtonCloseConnection
> 				26 텍스트 닫기
> 			27 텍스트 연결 상태
> 			28 텍스트 연결됨 ID: TextConnectionState
> 			29 텍스트 콜백 수신기
> 			30 텍스트 수신 대기 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			31 그룹 RPC 초기화 / 콜백 증거 ID: ExpanderRpcCallbackEvidence
> 				32 단추 RPC 초기화 / 콜백 증거 ID: HeaderSite
> 					33 텍스트 RPC 초기화 / 콜백 증거
> 				34 텍스트 RPC 초기화 증거
> 				35 텍스트 Attempt=3, Outcome=Connected, CandidateOrdinal=1, Remote=10.10.150.1:4000, Local=10.10.150.13, RequestedCallback=10.10.150.13:5000, Mode=Version2WakeHint, BoundCallback=10.10.150.13:5000, LocalSessionGeneration=1, 0x8080Attempts=1, Retry=False, InitOutcome=Succeeded, StartedUtc=2026-08-20T07:46:07.0018615Z, CompletedUtc=2026-08-20T07:46:07.0068642Z, LastACK={FrameValid=True, HeaderStatus=0, HeaderReserved=0, PayloadLength=24, HasCommandResult=False, CommandStatus=0, ErrorId=0}, Current=Active ID: TextRpcInitialization
> 				36 텍스트 콜백 v2 등록
> 				37 텍스트 Status=0, ErrorId=0, Version=2, MaxDatagram=52, BootId=0x00000042, SessionEpoch=27, Flags=0x00000000, Cookie=0x521F125D06FCDE46, ListenerGeneration=2, Source=10.10.150.1, EventMask=0x00000001, LocalSessionGeneration=1 ID: TextCallbackRegistration
> 				38 텍스트 PC 콜백 수신 증거
> 				39 텍스트 Accepted=2, Rejected=0, Duplicate=0, OutOfOrder=0 ID: TextCallbackCounters
> 				40 텍스트 Last decision=AcceptedWakeHint, ProtocolError=None ID: TextCallbackLastDecision
> 				41 텍스트 연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. UDP wake hint는 상태 판단 근거가 아니며, 동작 상태는 일치하는 TCP status 응답을 받은 뒤에만 변경됩니다.
> 		42 그룹 안전 / 복구 상세 정보 ID: ExpanderSafetyAndRecoveryDetails
> 			43 단추 안전 / 복구 상세 정보 ID: HeaderSite
> 				44 텍스트 안전 / 복구 상세 정보
> 		45 탭 ID: TabsMotion
> 			46 탭 항목 (selectable) 단축
> 				47 텍스트 단축
> 			48 탭 항목 (selectable) 그룹 모션
> 				49 텍스트 그룹 모션
> 			50 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				51 텍스트 EtherCAT / CREVIS / PI
> 			52 탭 항목 (selectable) Bulk 스냅샷
> 				53 텍스트 Bulk 스냅샷
> 			54 탭 항목 (selectable) 레코더
> 				55 텍스트 레코더
> 			56 탭 항목 (selectable) SDO / Write 정책
> 				57 텍스트 SDO / Write 정책
> 				58 창 ID: ScrollDiagnosticsOperations
> 					59 그룹 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19]) ID: GroupEncoderMaintenance
> 						60 텍스트 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])
> 						61 텍스트 파괴적 테스트 전용: TW[20]은 0x20FC:0x02에 UInt16 1을 씁니다. TW[19]는 0x20FC:0x01에 UInt16 1을 써 absolute multi-turn position을 reset하며 motion 전에 LMC Home이 필요합니다. 정확한 drive 축을 선택하십시오.
> 						62 텍스트 Fixed writes: TW[20]=0x20FC:0x02 <- UInt16 1; TW[19]=0x20FC:0x01 <- UInt16 1 ID: TextEncoderMaintenanceFixedTargets
> 						63 텍스트 Arm 전: 선택한 축의 Power를 끄고 안정적인 standstill과 실제 위치를 독립적으로 확인한 뒤 정확한 0x20FC 명령 지원 여부를 확인하십시오. 전용 0x7E53/0x7E54/0x7E55 경로는 일반 SDO Write와 분리됩니다.
> 						64 텍스트 작업
> 						65 콤보 상자 ID: ComboEncoderMaintenanceKind
> 						66 텍스트 Drive / 축 (1..4)
> 						67 콤보 상자 ID: ComboTestResetAxis
> 						68 텍스트 Timeout (ms, 1..60000)
> 						69 편집 ID: TextTestResetTimeout
> 						70 단추 Encoder 유지보수 Capability 새로고침 ID: ButtonRefreshTestResetCapabilities
> 							71 텍스트 Encoder 유지보수 Capability 새로고침
> 						72 단추 (disabled) Encoder 유지보수 결과 읽기 ID: ButtonReadTestResetStatus
> 							73 텍스트 (disabled) Encoder 유지보수 결과 읽기
> 						74 확인란 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다. ID: CheckTestResetPowerOffVerified
> 							75 텍스트 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다.
> 						76 확인란 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다. ID: CheckTestResetPhysicalPositionVerified
> 							77 텍스트 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다.
> 						78 확인란 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다. ID: CheckTestResetExactTargetVerified
> 							79 텍스트 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다.
> 						80 확인란 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다. ID: CheckEncoderMaintenanceCompatibilityVerified
> 							81 텍스트 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.
> 						82 텍스트 차단: 1단계의 모든 물리 확인과 encoder 호환성 확인이 필요합니다. Encoder 유지보수 작업을 arm하거나 전송하지 않았습니다. ID: TextEncoderMaintenanceArmGateStatus
> 						83 단추 안전 / 복구 상세 정보 열기 Description: 로컬 안전/복구 panel만 펼칩니다. 이 버튼은 PLC 명령을 전송하지 않으며 어떤 record도 폐기하지 않습니다. ID: ButtonOpenEncoderRecoveryDetails
> 							84 텍스트 안전 / 복구 상세 정보 열기
> 						85 단추 (disabled) 1단계 - Encoder 유지보수 Arm Description: Live mutation 승인, 현재 TW[20]/TW[19] capability, 사용 가능한 durable 유지보수 journal 및 1단계의 네 가지 확인이 필요합니다. Arm은 정확한 요청을 PC 메모리에만 준비하며 0x7E53을 전송하지 않습니다. ID: ButtonArmTestReset
> 							86 텍스트 (disabled) 1단계 - Encoder 유지보수 Arm
> 						87 확인란 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다. ID: CheckTestResetFinalConfirmed
> 							88 텍스트 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다.
> 						89 단추 (disabled) 2단계 - Encoder 유지보수 1회 실행 Description: 변경되지 않은 PC 전용 arm 요청, live mutation 승인, 현재 capability, 사용 가능한 durable 유지보수 journal 및 2단계 확인이 필요합니다. Execute는 0x7E53을 한 번 전송합니다. ID: ButtonExecuteTestReset
> 							90 텍스트 (disabled) 2단계 - Encoder 유지보수 1회 실행
> 						91 텍스트 Arm되거나 대기 중인 encoder 유지보수 작업이 없습니다. ID: TextTestResetResult
> 					92 그룹 저장된 Mutation 복구
> 						93 텍스트 저장된 Mutation 복구
> 						94 텍스트 Mutation journal 준비됨. 해결되지 않은 durable write record가 없습니다. ID: TextPersistedMutationStatus
> 						95 확인란 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다. ID: CheckPersistedMutationPhysicallyVerified
> 							96 텍스트 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.
> 						97 단추 (disabled) 복구된 Mutation 승인 ID: ButtonAcknowledgePersistedMutation
> 							98 텍스트 (disabled) 복구된 Mutation 승인
> 					99 그룹 비동기 SDO Read / Write ticket
> 						100 텍스트 비동기 SDO Read / Write ticket
> 						101 텍스트 작업
> 						102 콤보 상자 ID: ComboSdoOperation
> 						103 텍스트 슬레이브 참조 (1..4)
> 						104 편집 ID: TextSdoSlaveReference
> 						105 텍스트 객체 인덱스 (0x0001..0xFFFF)
> 						106 편집 ID: TextSdoIndex
> 						107 텍스트 서브인덱스 (0..255)
> 						108 편집 ID: TextSdoSubIndex
> 						109 텍스트 값 형식
> 						110 콤보 상자 ID: ComboSdoValueType
> 						111 텍스트 데이터 길이 (1, 2 또는 4)
> 						112 콤보 상자 ID: ComboSdoDataLength
> 						113 텍스트 Timeout (1..60000 cycle)
> 						114 편집 ID: TextSdoTimeoutCycles
> 						115 텍스트 Write 값 (10진수 또는 raw 0x hex)
> 						116 편집 (disabled) ID: TextSdoWriteData
> 						117 텍스트 SDK 승인 SDO Write target
> 						118 콤보 상자 (disabled) ID: ComboSdoWriteTarget
> 						119 단추 SDO Read 전송 Description: Write mode는 현재 session의 same-value 활성화 증명 뒤 2단계 확인을 사용합니다. Read mode는 추적되는 SDO Read 하나를 전송합니다. ID: ButtonSubmitSdo
> 							120 텍스트 SDO Read 전송
> 						121 단추 SDO Inline 읽기 (terminal 대기) Description: 일반 1/2/4-byte SDO Read 하나를 전송하고 terminal 결과를 기다립니다. SDO Write, 필수 Write readback 또는 CREVIS I/O는 실행하지 않습니다. ID: ButtonReadSdoInline
> 							122 텍스트 SDO Inline 읽기 (terminal 대기)
> 						123 단추 (disabled) Inline 대기 취소 (PC만) Description: PC 측 Inline Read 대기만 취소합니다. CancelOperation을 전송하거나 PLC ticket을 취소하거나 SDO request를 재시도하지 않습니다. ID: ButtonCancelSdoInlineWait
> 							124 텍스트 (disabled) Inline 대기 취소 (PC만)
> 						125 단추 (disabled) 필수 정확 Readback 불러오기 Description: 마지막으로 성공한 SDO Write를 확인하는 데 필요한 정확한 read request를 복원합니다. 명령은 전송하지 않습니다. ID: ButtonLoadRequiredSdoReadback
> 							126 텍스트 (disabled) 필수 정확 Readback 불러오기
> 						127 단추 Ticket 새로고침 ID: ButtonRefreshDiagnosticOperation
> 							128 텍스트 Ticket 새로고침
> 						129 단추 (disabled) 결과 다운로드 ID: ButtonDownloadSdoResult
> 							130 텍스트 (disabled) 결과 다운로드
> 						131 단추 결과 저장 ID: ButtonExportSdoResult
> 							132 텍스트 결과 저장
> 						133 단추 (disabled) Ticket 취소 ID: ButtonCancelDiagnosticOperation
> 							134 텍스트 (disabled) Ticket 취소
> 						135 텍스트 TicketId=136, Kind=SDORead, State=Completed, Outcome=Success SubmitCycle=14279763, CompletionCycle=14279786, ErrorId=0, Detail=0x00000000 ResultType=Int8, ResultLength=1, Data=08 ID: TextDiagnosticOperationSummary
> 					136 그룹 D5 SDO fault/recovery qualification (읽기 전용)
> 						137 텍스트 D5 SDO fault/recovery qualification (읽기 전용)
> 						138 텍스트 이 runner는 SDO를 쓰거나 EtherCAT fault를 주입하지 않습니다. Abort qualification은 제조사 승인된 존재하지 않는 읽기 전용 object/sub-index를 사용합니다. Contention, timeout, queued-cancel qualification은 0x6061:0 Int8/1만 사용합니다. Abrupt Disconnect는 RPC Close(0x405D)를 보내지 않고 API TCP transport만 의도적으로 끊은 뒤 재연결하여 0x6061:0을 정확히 두 번 읽습니다. 선택한 Slave 1..4는 _LMCAxis1..4에 매핑되며 모든 runner는 D5 ticket 전송 전에 PowerOn=False, Standstill=True 위치 sample 3회가 안정적으로 일치해야 합니다.
> 						139 텍스트 Abort PASS: 안정적인 BootId/MapRevision -> baseline -> 정확한 SDO abort -> 동일 값 복구. Contention PASS: 첫 0x6061 ticket 승인 -> 첫 terminal 전에 두 번째 Submit이 정확히 Rejected/ResourceBusy -> 첫 ticket Completed/Success -> 정확히 같은 첫 값으로 세 번째 별도 ticket Completed/Success. Timeout PASS: 0x6061 TimeoutCycles=1 -> 정확한 Expired/TimedOut -> 제한된 ResourceBusy drain 대기 -> 별도 동일 값 복구. Queued Cancel PASS: 0x6061 전송 -> 즉시 one-shot CancelOperation -> 정확한 Cancelled/Cancelled -> 별도 동일 값 복구. Abrupt Disconnect는 앱에서 확인 가능한 새 연결 복구만 PASS할 수 있으며 PLC MarkOrphan/late-callback 증거가 생길 때까지 orphanQualified는 false입니다. 연결 손실 전 terminal 완료 또는 Running race는 PLC orphan 증명에 대해 INCONCLUSIVE입니다. 예상 밖 승인 또는 불확실한 결과는 보존되어 자동 재시도를 차단합니다. Cancel Runner는 PLC Stop을 전송하지 않습니다.
> 						140 텍스트 대상 slave (1..4)
> 						141 편집 ID: TextD5SdoAbortSlaveReference
> 						142 텍스트 Abort 객체 인덱스
> 						143 편집 ID: TextD5SdoAbortIndex
> 						144 텍스트 Abort 서브인덱스
> 						145 편집 ID: TextD5SdoAbortSubIndex
> 						146 텍스트 Abort 값 형식
> 						147 콤보 상자 ID: ComboD5SdoAbortValueType
> 						148 텍스트 데이터 길이
> 						149 편집 ID: TextD5SdoAbortDataLength
> 						150 텍스트 일반/복구 timeout cycle
> 						151 편집 ID: TextD5SdoAbortTimeoutCycles
> 						152 단추 D5 Abort -> 복구 실행 ID: ButtonRunD5SdoAbortQualification
> 							153 텍스트 D5 Abort -> 복구 실행
> 						154 단추 D5 Contention -> 복구 실행 ID: ButtonRunD5SdoContentionQualification
> 							155 텍스트 D5 Contention -> 복구 실행
> 						156 단추 D5 Timeout -> 복구 실행 ID: ButtonRunD5SdoTimeoutQualification
> 							157 텍스트 D5 Timeout -> 복구 실행
> 						158 단추 D5 Queued Cancel -> 복구 실행 ID: ButtonRunD5SdoQueuedCancelQualification
> 							159 텍스트 D5 Queued Cancel -> 복구 실행
> 						160 단추 D5 Abrupt Disconnect -> 앱 복구 실행 ID: ButtonRunD5SdoDisconnectRecoveryQualification
> 							161 텍스트 D5 Abrupt Disconnect -> 앱 복구 실행
> 						162 단추 (disabled) Runner 취소 (PLC Stop 아님) ID: ButtonCancelD5SdoQualification
> 							163 텍스트 (disabled) Runner 취소 (PLC Stop 아님)
> 						164 단추 QTEST 로그 저장 ID: ButtonSaveD5SdoQualificationLog
> 							165 텍스트 QTEST 로그 저장
> 						166 진행률 표시줄 ID: ProgressD5SdoQualification
> 						167 텍스트 아직 qualification을 실행하지 않았습니다. ID: TextD5SdoQualificationProgress
> 						168 편집 ID: TextD5SdoQualificationSummary
> 							169 스크롤 막대 ID: HorizontalScrollBar
> 								170 단추 (disabled) ID: PART_LineLeftButton
> 								171 단추 (disabled) ID: PageLeft
> 								172 단추 (disabled) ID: PageRight
> 								173 엄지 (disabled)
> 								174 단추 (disabled) ID: PART_LineRightButton
> 					175 그룹 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						176 텍스트 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						177 텍스트 실제 WRITE: 이 runner는 승인된 target을 읽고 정확히 같은 4 byte를 쓴 뒤 정확한 readback을 요구합니다. Sentinel을 쓰지 않으며 자동 복원이나 재전송을 수행하지 않습니다.
> 						178 텍스트 PLC가 bit 8/9/13을 광고하고, 이 SDK build가 승인 target을 정확히 하나만 노출하고, durable mutation journal을 사용할 수 있고, 작업자 확인 4개가 모두 선택돼야 버튼이 fail-closed 상태에서 풀립니다. 확인 후 선택 축의 PowerOn=False, Standstill=True, 안정 위치를 다시 검사하고 target도 다시 읽어 baseline과 계속 일치해야 Write를 전송합니다.
> 						179 텍스트 SDK 승인 단일 target
> 						180 콤보 상자 ID: ComboD5SdoWriteQualificationTarget
> 						181 텍스트 Timeout (1..60000 cycle)
> 						182 편집 ID: TextD5SdoWriteQualificationTimeoutCycles
> 						183 확인란 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다. ID: CheckConfirmD5SdoWriteUi24Unused
> 							184 텍스트 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다.
> 						185 확인란 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다. ID: CheckConfirmD5SdoWriteOriginalRecorded
> 							186 텍스트 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다.
> 						187 확인란 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다. ID: CheckConfirmD5SdoWriteCaptureRunning
> 							188 텍스트 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다.
> 						189 확인란 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다. ID: CheckConfirmD5SdoWriteSingleWriter
> 							190 텍스트 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다.
> 						191 단추 (disabled) 동일 값 SDO Write Qualification 실행 Description: CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: ButtonRunD5SdoWriteSameValueQualification
> 							192 텍스트 (disabled) 동일 값 SDO Write Qualification 실행
> 						193 텍스트 준비 상태 matrix / 다음 시도 (cache, wire 전송 없음)
> 						194 텍스트 OVERALL CLOSED | EVALUATION_WIRE=NONE SDK POLICY FAIL | blockers=CapabilityObservationNotCurrent | targets=1 PLC CAPS CACHED | bit8/read=1 bit9/write=1 bit13/general=1 IDENTITY PASS | BootId=0x00000042 MapRevision=0x957F101E PAYLOAD PASS | SDO=4 req=1320 resp=2040 cycleUs=1000 QUAL TARGET PASS | selected=Reserved diagnostic UI[24] | Slave 1 | 0x2F00:24 | Int32 | -1073741823..1073741823 RUNNER PASS | connected=True idle=True motionClear=True manualTicketClear=True mutationInterlockClear=True JOURNAL PASS CONFIRMATIONS FAIL | 0/4 AXIS PROOF PENDING_RUNNER_PREFLIGHT | PowerOn=False, Standstill=True, stable position are not cached here. MANUAL WRITE CLOSED_RUN_SAME_VALUE_FIRST | proof never transfers across connection sessions or PLC identity changes. NEXT CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: TextD5SdoWriteQualificationGateStatus
> 						195 텍스트 마지막 시도 / 현재 실행
> 						196 텍스트 아직 동일 값 SDO Write qualification을 실행하지 않았습니다. ID: TextD5SdoWriteQualificationSummary
> 					197 그룹 PI Write 정책 gate
> 						198 텍스트 PI Write 정책 gate
> 						199 텍스트 EtherCAT / PI 탭에서 WritableByPolicy Catalog 항목을 정확히 하나 선택하십시오. Schema v1이 payload를 정의하고 compile-time allowlist가 승인될 때까지 호출은 fail-closed입니다. DS402 ControlWord와 target object는 영구 차단됩니다.
> 						200 텍스트 Raw 값 (10진수 또는 0x...)
> 						201 편집 (disabled) ID: TextPiWriteRawValue
> 						202 단추 (disabled) PI Write 전송 ID: ButtonSubmitPiWrite
> 							203 텍스트 (disabled) PI Write 전송
> 					204 스크롤 막대 ID: VerticalScrollBar
> 						205 단추 (disabled) ID: PART_LineUpButton
> 						206 단추 (disabled) ID: PageUp
> 						207 단추 (disabled) ID: PageDown
> 						208 엄지 (disabled)
> 						209 단추 (disabled) ID: PART_LineDownButton
> 			210 탭 항목 (selectable) 읽기 전용 API
> 				211 텍스트 읽기 전용 API
> 		212 그룹 실행 로그 / 콜백 진단 ID: ExpanderExecutionLog
> 			213 단추 실행 로그 / 콜백 진단 ID: HeaderSite
> 				214 텍스트 실행 로그 / 콜백 진단
> 			215 단추 로그 복사
> 				216 텍스트 로그 복사
> 			217 단추 로그 지우기
> 				218 텍스트 로그 지우기
> 			219 편집 ID: TextExecutionLog
> 				220 스크롤 막대 ID: VerticalScrollBar
> 					221 단추 (disabled) ID: PART_LineUpButton
> 					222 단추 (disabled) ID: PageUp
> 					223 단추 (disabled) ID: PageDown
> 					224 엄지 (disabled)
> 					225 단추 (disabled) ID: PART_LineDownButton
> 				226 스크롤 막대 ID: HorizontalScrollBar
> 					227 단추 (disabled) ID: PART_LineLeftButton
> 					228 단추 (disabled) ID: PageLeft
> 					229 단추 (disabled) ID: PageRight
> 					230 엄지 (disabled)
> 					231 단추 (disabled) ID: PART_LineRightButton
> 		232 상태 표시줄 ID: StatusOperationBar
> 			233 텍스트 Callback D5 status refresh completed ID: TextOperationState
>
> Document text: ```
> [16:39:10.444] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-20 07:38:58 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-20 07:38:55 UTC.
> [16:39:10.445] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [16:45:55.685] Connect started.
> [16:45:55.773] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:45:55.785] Ignored stale connection-state event Connecting from an older transport session.
> [16:45:55.785] Connection state Connecting -> Connected
> [16:45:55.893] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:45:55.904] Connect PASS.
> [16:45:58.785] Close Connection started.
> [16:45:58.789] Connection state Connected -> Closing
> [16:45:58.805] Close Connection PASS.
> [16:46:01.250] Connect started.
> [16:46:01.252] Connection state Disconnected -> Connecting
> [16:46:01.264] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:01.265] Connection state Connecting -> Connected
> [16:46:01.339] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:01.339] Connect PASS.
> [16:46:02.611] Close Connection started.
> [16:46:02.611] Connection state Connected -> Closing
> [16:46:02.621] Close Connection PASS.
> [16:46:06.999] Connect started.
> [16:46:07.001] Connection state Disconnected -> Connecting
> [16:46:07.011] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:07.012] Connection state Connecting -> Connected
> [16:46:07.084] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:07.084] Connect PASS.
> [16:47:58.941] Submit SDO Read started.
> [16:47:58.966] QTEST|utc=2026-08-20T07:47:58.966Z|elapsedMs=0|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [16:47:58.973] QTEST|utc=2026-08-20T07:47:58.973Z|elapsedMs=8|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [16:47:58.981] QTEST|utc=2026-08-20T07:47:58.981Z|elapsedMs=16|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [16:47:59.055] QTEST|utc=2026-08-20T07:47:59.055Z|elapsedMs=91|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=135|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [16:47:59.057] QTEST|utc=2026-08-20T07:47:59.057Z|elapsedMs=93|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|ticket=135|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=135|quarantine=false
> [16:47:59.060] Submit SDO Read PASS.
> [16:47:59.096] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000087
> [16:47:59.117] QTEST|utc=2026-08-20T07:47:59.117Z|elapsedMs=151|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=135|state=Completed|outcome=Success|verdict=CLEARED
> [16:47:59.118] QTEST|utc=2026-08-20T07:47:59.118Z|elapsedMs=152|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [16:47:59.124] Callback D5 authoritative TCP status processed. TicketId=0x00000087
> [17:04:28.599] Submit SDO Read started.
> [17:04:28.610] QTEST|utc=2026-08-20T08:04:28.610Z|elapsedMs=0|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [17:04:28.619] QTEST|utc=2026-08-20T08:04:28.619Z|elapsedMs=9|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [17:04:28.622] QTEST|utc=2026-08-20T08:04:28.622Z|elapsedMs=11|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [17:04:28.631] QTEST|utc=2026-08-20T08:04:28.631Z|elapsedMs=20|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=136|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [17:04:28.633] QTEST|utc=2026-08-20T08:04:28.633Z|elapsedMs=22|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|ticket=136|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=136|quarantine=false
> [17:04:28.659] Submit SDO Read PASS.
> [17:04:28.677] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000088
> [17:04:28.690] QTEST|utc=2026-08-20T08:04:28.690Z|elapsedMs=79|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=136|state=Completed|outcome=Success|verdict=CLEARED
> [17:04:28.692] QTEST|utc=2026-08-20T08:04:28.692Z|elapsedMs=81|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [17:04:28.696] Callback D5 authoritative TCP status processed. TicketId=0x00000088
> ```
> ````
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "같은 창에서 Clean Close 실행",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"No accessibility observation\");\n  const tree = String(observation.accessibility.tree || \"\");\n  const match = tree.match(/\\n\\s*(\\d+) 단추 닫기 ID: ButtonCloseConnection/);\n  if (!match) throw new Error(\"Close Connection button not found\");\n  const closeIndex = Number(match[1]);\n  globalThis.state = null;\n  await sky.click({ window: observation.window, element_index: closeIndex });\n  await new Promise((resolve) => setTimeout(resolve, 700));\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ````text
> Window: "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write] Secondary Actions: Raise
> 		1 창 Secondary Actions: Raise
> 			2 단추 In App Menu Expander ID: InAppMenuExpander
> 		3 제목 표시줄
> 			4 메뉴 모음 시스템 ID: MenuBar
> 				5 메뉴 항목 시스템
> 			6 단추 최소화
> 			7 단추 복원
> 			8 단추 닫기
> 		9 텍스트 언어
> 		10 콤보 상자 ID: ComboUiLanguage
> 		11 텍스트 LASAL 모션 제어 API 예제
> 		12 텍스트 모션 제어는 PLC에서 활성 상태입니다. 진단 제어는 PLC가 필요한 기능을 광고한 뒤에만 활성화됩니다. 값은 선택한 application UNIT을 사용하거나 raw DINT로 전달됩니다.
> 		13 그룹 연결 / RPC 콜백
> 			14 텍스트 연결 / RPC 콜백
> 			15 텍스트 PLC IP
> 			16 편집 (disabled) ID: TextRemoteIp
> 			17 텍스트 TCP 포트
> 			18 편집 (disabled) ID: TextRemotePort
> 			19 텍스트 PC 로컬 IPv4
> 			20 편집 (disabled) ID: TextLocalIp
> 			21 텍스트 콜백 UDP 포트
> 			22 편집 (disabled) ID: TextCallbackPort
> 			23 단추 연결 ID: ButtonConnect
> 				24 텍스트 연결
> 			25 단추 닫기 ID: ButtonCloseConnection
> 				26 텍스트 닫기
> 			27 텍스트 연결 상태
> 			28 텍스트 연결됨 ID: TextConnectionState
> 			29 텍스트 콜백 수신기
> 			30 텍스트 수신 대기 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			31 그룹 RPC 초기화 / 콜백 증거 ID: ExpanderRpcCallbackEvidence
> 				32 단추 RPC 초기화 / 콜백 증거 ID: HeaderSite
> 					33 텍스트 RPC 초기화 / 콜백 증거
> 				34 텍스트 RPC 초기화 증거
> 				35 텍스트 Attempt=3, Outcome=Connected, CandidateOrdinal=1, Remote=10.10.150.1:4000, Local=10.10.150.13, RequestedCallback=10.10.150.13:5000, Mode=Version2WakeHint, BoundCallback=10.10.150.13:5000, LocalSessionGeneration=1, 0x8080Attempts=1, Retry=False, InitOutcome=Succeeded, StartedUtc=2026-08-20T07:46:07.0018615Z, CompletedUtc=2026-08-20T07:46:07.0068642Z, LastACK={FrameValid=True, HeaderStatus=0, HeaderReserved=0, PayloadLength=24, HasCommandResult=False, CommandStatus=0, ErrorId=0}, Current=Active ID: TextRpcInitialization
> 				36 텍스트 콜백 v2 등록
> 				37 텍스트 Status=0, ErrorId=0, Version=2, MaxDatagram=52, BootId=0x00000042, SessionEpoch=27, Flags=0x00000000, Cookie=0x521F125D06FCDE46, ListenerGeneration=2, Source=10.10.150.1, EventMask=0x00000001, LocalSessionGeneration=1 ID: TextCallbackRegistration
> 				38 텍스트 PC 콜백 수신 증거
> 				39 텍스트 Accepted=2, Rejected=0, Duplicate=0, OutOfOrder=0 ID: TextCallbackCounters
> 				40 텍스트 Last decision=AcceptedWakeHint, ProtocolError=None ID: TextCallbackLastDecision
> 				41 텍스트 연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. UDP wake hint는 상태 판단 근거가 아니며, 동작 상태는 일치하는 TCP status 응답을 받은 뒤에만 변경됩니다.
> 		42 그룹 안전 / 복구 상세 정보 ID: ExpanderSafetyAndRecoveryDetails
> 			43 단추 안전 / 복구 상세 정보 ID: HeaderSite
> 				44 텍스트 안전 / 복구 상세 정보
> 		45 탭 ID: TabsMotion
> 			46 탭 항목 (selectable) 단축
> 				47 텍스트 단축
> 			48 탭 항목 (selectable) 그룹 모션
> 				49 텍스트 그룹 모션
> 			50 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				51 텍스트 EtherCAT / CREVIS / PI
> 			52 탭 항목 (selectable) Bulk 스냅샷
> 				53 텍스트 Bulk 스냅샷
> 			54 탭 항목 (selectable) 레코더
> 				55 텍스트 레코더
> 			56 탭 항목 (selectable) SDO / Write 정책
> 				57 텍스트 SDO / Write 정책
> 				58 창 ID: ScrollDiagnosticsOperations
> 					59 그룹 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19]) ID: GroupEncoderMaintenance
> 						60 텍스트 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])
> 						61 텍스트 파괴적 테스트 전용: TW[20]은 0x20FC:0x02에 UInt16 1을 씁니다. TW[19]는 0x20FC:0x01에 UInt16 1을 써 absolute multi-turn position을 reset하며 motion 전에 LMC Home이 필요합니다. 정확한 drive 축을 선택하십시오.
> 						62 텍스트 Fixed writes: TW[20]=0x20FC:0x02 <- UInt16 1; TW[19]=0x20FC:0x01 <- UInt16 1 ID: TextEncoderMaintenanceFixedTargets
> 						63 텍스트 Arm 전: 선택한 축의 Power를 끄고 안정적인 standstill과 실제 위치를 독립적으로 확인한 뒤 정확한 0x20FC 명령 지원 여부를 확인하십시오. 전용 0x7E53/0x7E54/0x7E55 경로는 일반 SDO Write와 분리됩니다.
> 						64 텍스트 작업
> 						65 콤보 상자 ID: ComboEncoderMaintenanceKind
> 						66 텍스트 Drive / 축 (1..4)
> 						67 콤보 상자 ID: ComboTestResetAxis
> 						68 텍스트 Timeout (ms, 1..60000)
> 						69 편집 ID: TextTestResetTimeout
> 						70 단추 Encoder 유지보수 Capability 새로고침 ID: ButtonRefreshTestResetCapabilities
> 							71 텍스트 Encoder 유지보수 Capability 새로고침
> 						72 단추 (disabled) Encoder 유지보수 결과 읽기 ID: ButtonReadTestResetStatus
> 							73 텍스트 (disabled) Encoder 유지보수 결과 읽기
> 						74 확인란 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다. ID: CheckTestResetPowerOffVerified
> 							75 텍스트 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다.
> 						76 확인란 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다. ID: CheckTestResetPhysicalPositionVerified
> 							77 텍스트 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다.
> 						78 확인란 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다. ID: CheckTestResetExactTargetVerified
> 							79 텍스트 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다.
> 						80 확인란 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다. ID: CheckEncoderMaintenanceCompatibilityVerified
> 							81 텍스트 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.
> 						82 텍스트 차단: 1단계의 모든 물리 확인과 encoder 호환성 확인이 필요합니다. Encoder 유지보수 작업을 arm하거나 전송하지 않았습니다. ID: TextEncoderMaintenanceArmGateStatus
> 						83 단추 안전 / 복구 상세 정보 열기 Description: 로컬 안전/복구 panel만 펼칩니다. 이 버튼은 PLC 명령을 전송하지 않으며 어떤 record도 폐기하지 않습니다. ID: ButtonOpenEncoderRecoveryDetails
> 							84 텍스트 안전 / 복구 상세 정보 열기
> 						85 단추 (disabled) 1단계 - Encoder 유지보수 Arm Description: Live mutation 승인, 현재 TW[20]/TW[19] capability, 사용 가능한 durable 유지보수 journal 및 1단계의 네 가지 확인이 필요합니다. Arm은 정확한 요청을 PC 메모리에만 준비하며 0x7E53을 전송하지 않습니다. ID: ButtonArmTestReset
> 							86 텍스트 (disabled) 1단계 - Encoder 유지보수 Arm
> 						87 확인란 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다. ID: CheckTestResetFinalConfirmed
> 							88 텍스트 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다.
> 						89 단추 (disabled) 2단계 - Encoder 유지보수 1회 실행 Description: 변경되지 않은 PC 전용 arm 요청, live mutation 승인, 현재 capability, 사용 가능한 durable 유지보수 journal 및 2단계 확인이 필요합니다. Execute는 0x7E53을 한 번 전송합니다. ID: ButtonExecuteTestReset
> 							90 텍스트 (disabled) 2단계 - Encoder 유지보수 1회 실행
> 						91 텍스트 Arm되거나 대기 중인 encoder 유지보수 작업이 없습니다. ID: TextTestResetResult
> 					92 그룹 저장된 Mutation 복구
> 						93 텍스트 저장된 Mutation 복구
> 						94 텍스트 Mutation journal 준비됨. 해결되지 않은 durable write record가 없습니다. ID: TextPersistedMutationStatus
> 						95 확인란 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다. ID: CheckPersistedMutationPhysicallyVerified
> 							96 텍스트 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.
> 						97 단추 (disabled) 복구된 Mutation 승인 ID: ButtonAcknowledgePersistedMutation
> 							98 텍스트 (disabled) 복구된 Mutation 승인
> 					99 그룹 비동기 SDO Read / Write ticket
> 						100 텍스트 비동기 SDO Read / Write ticket
> 						101 텍스트 작업
> 						102 콤보 상자 ID: ComboSdoOperation
> 						103 텍스트 슬레이브 참조 (1..4)
> 						104 편집 ID: TextSdoSlaveReference
> 						105 텍스트 객체 인덱스 (0x0001..0xFFFF)
> 						106 편집 ID: TextSdoIndex
> 						107 텍스트 서브인덱스 (0..255)
> 						108 편집 ID: TextSdoSubIndex
> 						109 텍스트 값 형식
> 						110 콤보 상자 ID: ComboSdoValueType
> 						111 텍스트 데이터 길이 (1, 2 또는 4)
> 						112 콤보 상자 ID: ComboSdoDataLength
> 						113 텍스트 Timeout (1..60000 cycle)
> 						114 편집 ID: TextSdoTimeoutCycles
> 						115 텍스트 Write 값 (10진수 또는 raw 0x hex)
> 						116 편집 (disabled) ID: TextSdoWriteData
> 						117 텍스트 SDK 승인 SDO Write target
> 						118 콤보 상자 (disabled) ID: ComboSdoWriteTarget
> 						119 단추 SDO Read 전송 Description: Write mode는 현재 session의 same-value 활성화 증명 뒤 2단계 확인을 사용합니다. Read mode는 추적되는 SDO Read 하나를 전송합니다. ID: ButtonSubmitSdo
> 							120 텍스트 SDO Read 전송
> 						121 단추 SDO Inline 읽기 (terminal 대기) Description: 일반 1/2/4-byte SDO Read 하나를 전송하고 terminal 결과를 기다립니다. SDO Write, 필수 Write readback 또는 CREVIS I/O는 실행하지 않습니다. ID: ButtonReadSdoInline
> 							122 텍스트 SDO Inline 읽기 (terminal 대기)
> 						123 단추 (disabled) Inline 대기 취소 (PC만) Description: PC 측 Inline Read 대기만 취소합니다. CancelOperation을 전송하거나 PLC ticket을 취소하거나 SDO request를 재시도하지 않습니다. ID: ButtonCancelSdoInlineWait
> 							124 텍스트 (disabled) Inline 대기 취소 (PC만)
> 						125 단추 (disabled) 필수 정확 Readback 불러오기 Description: 마지막으로 성공한 SDO Write를 확인하는 데 필요한 정확한 read request를 복원합니다. 명령은 전송하지 않습니다. ID: ButtonLoadRequiredSdoReadback
> 							126 텍스트 (disabled) 필수 정확 Readback 불러오기
> 						127 단추 Ticket 새로고침 ID: ButtonRefreshDiagnosticOperation
> 							128 텍스트 Ticket 새로고침
> 						129 단추 (disabled) 결과 다운로드 ID: ButtonDownloadSdoResult
> 							130 텍스트 (disabled) 결과 다운로드
> 						131 단추 결과 저장 ID: ButtonExportSdoResult
> 							132 텍스트 결과 저장
> 						133 단추 (disabled) Ticket 취소 ID: ButtonCancelDiagnosticOperation
> 							134 텍스트 (disabled) Ticket 취소
> 						135 텍스트 TicketId=136, Kind=SDORead, State=Completed, Outcome=Success SubmitCycle=14279763, CompletionCycle=14279786, ErrorId=0, Detail=0x00000000 ResultType=Int8, ResultLength=1, Data=08 ID: TextDiagnosticOperationSummary
> 					136 그룹 D5 SDO fault/recovery qualification (읽기 전용)
> 						137 텍스트 D5 SDO fault/recovery qualification (읽기 전용)
> 						138 텍스트 이 runner는 SDO를 쓰거나 EtherCAT fault를 주입하지 않습니다. Abort qualification은 제조사 승인된 존재하지 않는 읽기 전용 object/sub-index를 사용합니다. Contention, timeout, queued-cancel qualification은 0x6061:0 Int8/1만 사용합니다. Abrupt Disconnect는 RPC Close(0x405D)를 보내지 않고 API TCP transport만 의도적으로 끊은 뒤 재연결하여 0x6061:0을 정확히 두 번 읽습니다. 선택한 Slave 1..4는 _LMCAxis1..4에 매핑되며 모든 runner는 D5 ticket 전송 전에 PowerOn=False, Standstill=True 위치 sample 3회가 안정적으로 일치해야 합니다.
> 						139 텍스트 Abort PASS: 안정적인 BootId/MapRevision -> baseline -> 정확한 SDO abort -> 동일 값 복구. Contention PASS: 첫 0x6061 ticket 승인 -> 첫 terminal 전에 두 번째 Submit이 정확히 Rejected/ResourceBusy -> 첫 ticket Completed/Success -> 정확히 같은 첫 값으로 세 번째 별도 ticket Completed/Success. Timeout PASS: 0x6061 TimeoutCycles=1 -> 정확한 Expired/TimedOut -> 제한된 ResourceBusy drain 대기 -> 별도 동일 값 복구. Queued Cancel PASS: 0x6061 전송 -> 즉시 one-shot CancelOperation -> 정확한 Cancelled/Cancelled -> 별도 동일 값 복구. Abrupt Disconnect는 앱에서 확인 가능한 새 연결 복구만 PASS할 수 있으며 PLC MarkOrphan/late-callback 증거가 생길 때까지 orphanQualified는 false입니다. 연결 손실 전 terminal 완료 또는 Running race는 PLC orphan 증명에 대해 INCONCLUSIVE입니다. 예상 밖 승인 또는 불확실한 결과는 보존되어 자동 재시도를 차단합니다. Cancel Runner는 PLC Stop을 전송하지 않습니다.
> 						140 텍스트 대상 slave (1..4)
> 						141 편집 ID: TextD5SdoAbortSlaveReference
> 						142 텍스트 Abort 객체 인덱스
> 						143 편집 ID: TextD5SdoAbortIndex
> 						144 텍스트 Abort 서브인덱스
> 						145 편집 ID: TextD5SdoAbortSubIndex
> 						146 텍스트 Abort 값 형식
> 						147 콤보 상자 ID: ComboD5SdoAbortValueType
> 						148 텍스트 데이터 길이
> 						149 편집 ID: TextD5SdoAbortDataLength
> 						150 텍스트 일반/복구 timeout cycle
> 						151 편집 ID: TextD5SdoAbortTimeoutCycles
> 						152 단추 D5 Abort -> 복구 실행 ID: ButtonRunD5SdoAbortQualification
> 							153 텍스트 D5 Abort -> 복구 실행
> 						154 단추 D5 Contention -> 복구 실행 ID: ButtonRunD5SdoContentionQualification
> 							155 텍스트 D5 Contention -> 복구 실행
> 						156 단추 D5 Timeout -> 복구 실행 ID: ButtonRunD5SdoTimeoutQualification
> 							157 텍스트 D5 Timeout -> 복구 실행
> 						158 단추 D5 Queued Cancel -> 복구 실행 ID: ButtonRunD5SdoQueuedCancelQualification
> 							159 텍스트 D5 Queued Cancel -> 복구 실행
> 						160 단추 D5 Abrupt Disconnect -> 앱 복구 실행 ID: ButtonRunD5SdoDisconnectRecoveryQualification
> 							161 텍스트 D5 Abrupt Disconnect -> 앱 복구 실행
> 						162 단추 (disabled) Runner 취소 (PLC Stop 아님) ID: ButtonCancelD5SdoQualification
> 							163 텍스트 (disabled) Runner 취소 (PLC Stop 아님)
> 						164 단추 QTEST 로그 저장 ID: ButtonSaveD5SdoQualificationLog
> 							165 텍스트 QTEST 로그 저장
> 						166 진행률 표시줄 ID: ProgressD5SdoQualification
> 						167 텍스트 아직 qualification을 실행하지 않았습니다. ID: TextD5SdoQualificationProgress
> 						168 편집 ID: TextD5SdoQualificationSummary
> 							169 스크롤 막대 ID: HorizontalScrollBar
> 								170 단추 (disabled) ID: PART_LineLeftButton
> 								171 단추 (disabled) ID: PageLeft
> 								172 단추 (disabled) ID: PageRight
> 								173 엄지 (disabled)
> 								174 단추 (disabled) ID: PART_LineRightButton
> 					175 그룹 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						176 텍스트 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						177 텍스트 실제 WRITE: 이 runner는 승인된 target을 읽고 정확히 같은 4 byte를 쓴 뒤 정확한 readback을 요구합니다. Sentinel을 쓰지 않으며 자동 복원이나 재전송을 수행하지 않습니다.
> 						178 텍스트 PLC가 bit 8/9/13을 광고하고, 이 SDK build가 승인 target을 정확히 하나만 노출하고, durable mutation journal을 사용할 수 있고, 작업자 확인 4개가 모두 선택돼야 버튼이 fail-closed 상태에서 풀립니다. 확인 후 선택 축의 PowerOn=False, Standstill=True, 안정 위치를 다시 검사하고 target도 다시 읽어 baseline과 계속 일치해야 Write를 전송합니다.
> 						179 텍스트 SDK 승인 단일 target
> 						180 콤보 상자 ID: ComboD5SdoWriteQualificationTarget
> 						181 텍스트 Timeout (1..60000 cycle)
> 						182 편집 ID: TextD5SdoWriteQualificationTimeoutCycles
> 						183 확인란 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다. ID: CheckConfirmD5SdoWriteUi24Unused
> 							184 텍스트 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다.
> 						185 확인란 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다. ID: CheckConfirmD5SdoWriteOriginalRecorded
> 							186 텍스트 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다.
> 						187 확인란 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다. ID: CheckConfirmD5SdoWriteCaptureRunning
> 							188 텍스트 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다.
> 						189 확인란 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다. ID: CheckConfirmD5SdoWriteSingleWriter
> 							190 텍스트 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다.
> 						191 단추 (disabled) 동일 값 SDO Write Qualification 실행 Description: CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: ButtonRunD5SdoWriteSameValueQualification
> 							192 텍스트 (disabled) 동일 값 SDO Write Qualification 실행
> 						193 텍스트 준비 상태 matrix / 다음 시도 (cache, wire 전송 없음)
> 						194 텍스트 OVERALL CLOSED | EVALUATION_WIRE=NONE SDK POLICY FAIL | blockers=CapabilityObservationNotCurrent | targets=1 PLC CAPS CACHED | bit8/read=1 bit9/write=1 bit13/general=1 IDENTITY PASS | BootId=0x00000042 MapRevision=0x957F101E PAYLOAD PASS | SDO=4 req=1320 resp=2040 cycleUs=1000 QUAL TARGET PASS | selected=Reserved diagnostic UI[24] | Slave 1 | 0x2F00:24 | Int32 | -1073741823..1073741823 RUNNER PASS | connected=True idle=True motionClear=True manualTicketClear=True mutationInterlockClear=True JOURNAL PASS CONFIRMATIONS FAIL | 0/4 AXIS PROOF PENDING_RUNNER_PREFLIGHT | PowerOn=False, Standstill=True, stable position are not cached here. MANUAL WRITE CLOSED_RUN_SAME_VALUE_FIRST | proof never transfers across connection sessions or PLC identity changes. NEXT CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: TextD5SdoWriteQualificationGateStatus
> 						195 텍스트 마지막 시도 / 현재 실행
> 						196 텍스트 아직 동일 값 SDO Write qualification을 실행하지 않았습니다. ID: TextD5SdoWriteQualificationSummary
> 					197 그룹 PI Write 정책 gate
> 						198 텍스트 PI Write 정책 gate
> 						199 텍스트 EtherCAT / PI 탭에서 WritableByPolicy Catalog 항목을 정확히 하나 선택하십시오. Schema v1이 payload를 정의하고 compile-time allowlist가 승인될 때까지 호출은 fail-closed입니다. DS402 ControlWord와 target object는 영구 차단됩니다.
> 						200 텍스트 Raw 값 (10진수 또는 0x...)
> 						201 편집 (disabled) ID: TextPiWriteRawValue
> 						202 단추 (disabled) PI Write 전송 ID: ButtonSubmitPiWrite
> 							203 텍스트 (disabled) PI Write 전송
> 					204 스크롤 막대 ID: VerticalScrollBar
> 						205 단추 (disabled) ID: PART_LineUpButton
> 						206 단추 (disabled) ID: PageUp
> 						207 단추 (disabled) ID: PageDown
> 						208 엄지 (disabled)
> 						209 단추 (disabled) ID: PART_LineDownButton
> 			210 탭 항목 (selectable) 읽기 전용 API
> 				211 텍스트 읽기 전용 API
> 		212 그룹 실행 로그 / 콜백 진단 ID: ExpanderExecutionLog
> 			213 단추 실행 로그 / 콜백 진단 ID: HeaderSite
> 				214 텍스트 실행 로그 / 콜백 진단
> 			215 단추 로그 복사
> 				216 텍스트 로그 복사
> 			217 단추 로그 지우기
> 				218 텍스트 로그 지우기
> 			219 편집 ID: TextExecutionLog
> 				220 스크롤 막대 ID: VerticalScrollBar
> 					221 단추 (disabled) ID: PART_LineUpButton
> 					222 단추 (disabled) ID: PageUp
> 					223 단추 (disabled) ID: PageDown
> 					224 엄지 (disabled)
> 					225 단추 (disabled) ID: PART_LineDownButton
> 				226 스크롤 막대 ID: HorizontalScrollBar
> 					227 단추 (disabled) ID: PART_LineLeftButton
> 					228 단추 (disabled) ID: PageLeft
> 					229 단추 (disabled) ID: PageRight
> 					230 엄지 (disabled)
> 					231 단추 (disabled) ID: PART_LineRightButton
> 		232 상태 표시줄 ID: StatusOperationBar
> 			233 텍스트 Callback D5 status refresh completed ID: TextOperationState
>
> Document text: ```
> [16:39:10.444] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-20 07:38:58 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-20 07:38:55 UTC.
> [16:39:10.445] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [16:45:55.685] Connect started.
> [16:45:55.773] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:45:55.785] Ignored stale connection-state event Connecting from an older transport session.
> [16:45:55.785] Connection state Connecting -> Connected
> [16:45:55.893] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:45:55.904] Connect PASS.
> [16:45:58.785] Close Connection started.
> [16:45:58.789] Connection state Connected -> Closing
> [16:45:58.805] Close Connection PASS.
> [16:46:01.250] Connect started.
> [16:46:01.252] Connection state Disconnected -> Connecting
> [16:46:01.264] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:01.265] Connection state Connecting -> Connected
> [16:46:01.339] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:01.339] Connect PASS.
> [16:46:02.611] Close Connection started.
> [16:46:02.611] Connection state Connected -> Closing
> [16:46:02.621] Close Connection PASS.
> [16:46:06.999] Connect started.
> [16:46:07.001] Connection state Disconnected -> Connecting
> [16:46:07.011] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:07.012] Connection state Connecting -> Connected
> [16:46:07.084] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:07.084] Connect PASS.
> [16:47:58.941] Submit SDO Read started.
> [16:47:58.966] QTEST|utc=2026-08-20T07:47:58.966Z|elapsedMs=0|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [16:47:58.973] QTEST|utc=2026-08-20T07:47:58.973Z|elapsedMs=8|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [16:47:58.981] QTEST|utc=2026-08-20T07:47:58.981Z|elapsedMs=16|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [16:47:59.055] QTEST|utc=2026-08-20T07:47:59.055Z|elapsedMs=91|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=135|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [16:47:59.057] QTEST|utc=2026-08-20T07:47:59.057Z|elapsedMs=93|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|ticket=135|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=135|quarantine=false
> [16:47:59.060] Submit SDO Read PASS.
> [16:47:59.096] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000087
> [16:47:59.117] QTEST|utc=2026-08-20T07:47:59.117Z|elapsedMs=151|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=135|state=Completed|outcome=Success|verdict=CLEARED
> [16:47:59.118] QTEST|utc=2026-08-20T07:47:59.118Z|elapsedMs=152|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [16:47:59.124] Callback D5 authoritative TCP status processed. TicketId=0x00000087
> [17:04:28.599] Submit SDO Read started.
> [17:04:28.610] QTEST|utc=2026-08-20T08:04:28.610Z|elapsedMs=0|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [17:04:28.619] QTEST|utc=2026-08-20T08:04:28.619Z|elapsedMs=9|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [17:04:28.622] QTEST|utc=2026-08-20T08:04:28.622Z|elapsedMs=11|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [17:04:28.631] QTEST|utc=2026-08-20T08:04:28.631Z|elapsedMs=20|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=136|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [17:04:28.633] QTEST|utc=2026-08-20T08:04:28.633Z|elapsedMs=22|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|ticket=136|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=136|quarantine=false
> [17:04:28.659] Submit SDO Read PASS.
> [17:04:28.677] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000088
> [17:04:28.690] QTEST|utc=2026-08-20T08:04:28.690Z|elapsedMs=79|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=136|state=Completed|outcome=Success|verdict=CLEARED
> [17:04:28.692] QTEST|utc=2026-08-20T08:04:28.692Z|elapsedMs=81|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [17:04:28.696] Callback D5 authoritative TCP status processed. TicketId=0x00000088
> [17:04:39.215] Close Connection started.
> [17:04:39.233] Connection state Connected -> Closing
> [17:04:39.253] Close Connection PASS.
> ```
> ````
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "같은 창과 UDP 5000으로 재연결",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"No accessibility observation\");\n  const tree = String(observation.accessibility.tree || \"\");\n  const match = tree.match(/\\n\\s*(\\d+) 단추 연결 ID: ButtonConnect/);\n  if (!match) throw new Error(\"Connect button not found\");\n  const connectIndex = Number(match[1]);\n  globalThis.state = null;\n  await sky.click({ window: observation.window, element_index: connectIndex });\n  await new Promise((resolve) => setTimeout(resolve, 1100));\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ````text
> Window: "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write] Secondary Actions: Raise
> 		1 창 Secondary Actions: Raise
> 			2 단추 In App Menu Expander ID: InAppMenuExpander
> 		3 제목 표시줄
> 			4 메뉴 모음 시스템 ID: MenuBar
> 				5 메뉴 항목 시스템
> 			6 단추 최소화
> 			7 단추 복원
> 			8 단추 닫기
> 		9 텍스트 언어
> 		10 콤보 상자 ID: ComboUiLanguage
> 		11 텍스트 LASAL 모션 제어 API 예제
> 		12 텍스트 모션 제어는 PLC에서 활성 상태입니다. 진단 제어는 PLC가 필요한 기능을 광고한 뒤에만 활성화됩니다. 값은 선택한 application UNIT을 사용하거나 raw DINT로 전달됩니다.
> 		13 그룹 연결 / RPC 콜백
> 			14 텍스트 연결 / RPC 콜백
> 			15 텍스트 PLC IP
> 			16 편집 (disabled) ID: TextRemoteIp
> 			17 텍스트 TCP 포트
> 			18 편집 (disabled) ID: TextRemotePort
> 			19 텍스트 PC 로컬 IPv4
> 			20 편집 (disabled) ID: TextLocalIp
> 			21 텍스트 콜백 UDP 포트
> 			22 편집 (disabled) ID: TextCallbackPort
> 			23 단추 연결 ID: ButtonConnect
> 				24 텍스트 연결
> 			25 단추 닫기 ID: ButtonCloseConnection
> 				26 텍스트 닫기
> 			27 텍스트 연결 상태
> 			28 텍스트 연결됨 ID: TextConnectionState
> 			29 텍스트 콜백 수신기
> 			30 텍스트 수신 대기 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			31 그룹 RPC 초기화 / 콜백 증거 ID: ExpanderRpcCallbackEvidence
> 				32 단추 RPC 초기화 / 콜백 증거 ID: HeaderSite
> 					33 텍스트 RPC 초기화 / 콜백 증거
> 				34 텍스트 RPC 초기화 증거
> 				35 텍스트 Attempt=4, Outcome=Connected, CandidateOrdinal=1, Remote=10.10.150.1:4000, Local=10.10.150.13, RequestedCallback=10.10.150.13:5000, Mode=Version2WakeHint, BoundCallback=10.10.150.13:5000, LocalSessionGeneration=1, 0x8080Attempts=1, Retry=False, InitOutcome=Succeeded, StartedUtc=2026-08-20T08:04:52.8760280Z, CompletedUtc=2026-08-20T08:04:52.8810342Z, LastACK={FrameValid=True, HeaderStatus=0, HeaderReserved=0, PayloadLength=24, HasCommandResult=False, CommandStatus=0, ErrorId=0}, Current=Active ID: TextRpcInitialization
> 				36 텍스트 콜백 v2 등록
> 				37 텍스트 Status=0, ErrorId=0, Version=2, MaxDatagram=52, BootId=0x00000042, SessionEpoch=30, Flags=0x00000000, Cookie=0x2A978FF0DA1E78F3, ListenerGeneration=2, Source=10.10.150.1, EventMask=0x00000001, LocalSessionGeneration=1 ID: TextCallbackRegistration
> 				38 텍스트 PC 콜백 수신 증거
> 				39 텍스트 Accepted=0, Rejected=0, Duplicate=0, OutOfOrder=0 ID: TextCallbackCounters
> 				40 텍스트 Last decision=None ID: TextCallbackLastDecision
> 				41 텍스트 연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. UDP wake hint는 상태 판단 근거가 아니며, 동작 상태는 일치하는 TCP status 응답을 받은 뒤에만 변경됩니다.
> 		42 그룹 안전 / 복구 상세 정보 ID: ExpanderSafetyAndRecoveryDetails
> 			43 단추 안전 / 복구 상세 정보 ID: HeaderSite
> 				44 텍스트 안전 / 복구 상세 정보
> 		45 탭 ID: TabsMotion
> 			46 탭 항목 (selectable) 단축
> 				47 텍스트 단축
> 			48 탭 항목 (selectable) 그룹 모션
> 				49 텍스트 그룹 모션
> 			50 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				51 텍스트 EtherCAT / CREVIS / PI
> 			52 탭 항목 (selectable) Bulk 스냅샷
> 				53 텍스트 Bulk 스냅샷
> 			54 탭 항목 (selectable) 레코더
> 				55 텍스트 레코더
> 			56 탭 항목 (selectable) SDO / Write 정책
> 				57 텍스트 SDO / Write 정책
> 				58 창 ID: ScrollDiagnosticsOperations
> 					59 그룹 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19]) ID: GroupEncoderMaintenance
> 						60 텍스트 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])
> 						61 텍스트 파괴적 테스트 전용: TW[20]은 0x20FC:0x02에 UInt16 1을 씁니다. TW[19]는 0x20FC:0x01에 UInt16 1을 써 absolute multi-turn position을 reset하며 motion 전에 LMC Home이 필요합니다. 정확한 drive 축을 선택하십시오.
> 						62 텍스트 Fixed writes: TW[20]=0x20FC:0x02 <- UInt16 1; TW[19]=0x20FC:0x01 <- UInt16 1 ID: TextEncoderMaintenanceFixedTargets
> 						63 텍스트 Arm 전: 선택한 축의 Power를 끄고 안정적인 standstill과 실제 위치를 독립적으로 확인한 뒤 정확한 0x20FC 명령 지원 여부를 확인하십시오. 전용 0x7E53/0x7E54/0x7E55 경로는 일반 SDO Write와 분리됩니다.
> 						64 텍스트 작업
> 						65 콤보 상자 ID: ComboEncoderMaintenanceKind
> 						66 텍스트 Drive / 축 (1..4)
> 						67 콤보 상자 ID: ComboTestResetAxis
> 						68 텍스트 Timeout (ms, 1..60000)
> 						69 편집 ID: TextTestResetTimeout
> 						70 단추 Encoder 유지보수 Capability 새로고침 ID: ButtonRefreshTestResetCapabilities
> 							71 텍스트 Encoder 유지보수 Capability 새로고침
> 						72 단추 (disabled) Encoder 유지보수 결과 읽기 ID: ButtonReadTestResetStatus
> 							73 텍스트 (disabled) Encoder 유지보수 결과 읽기
> 						74 확인란 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다. ID: CheckTestResetPowerOffVerified
> 							75 텍스트 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다.
> 						76 확인란 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다. ID: CheckTestResetPhysicalPositionVerified
> 							77 텍스트 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다.
> 						78 확인란 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다. ID: CheckTestResetExactTargetVerified
> 							79 텍스트 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다.
> 						80 확인란 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다. ID: CheckEncoderMaintenanceCompatibilityVerified
> 							81 텍스트 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.
> 						82 텍스트 차단: 1단계의 모든 물리 확인과 encoder 호환성 확인이 필요합니다. Encoder 유지보수 작업을 arm하거나 전송하지 않았습니다. ID: TextEncoderMaintenanceArmGateStatus
> 						83 단추 안전 / 복구 상세 정보 열기 Description: 로컬 안전/복구 panel만 펼칩니다. 이 버튼은 PLC 명령을 전송하지 않으며 어떤 record도 폐기하지 않습니다. ID: ButtonOpenEncoderRecoveryDetails
> 							84 텍스트 안전 / 복구 상세 정보 열기
> 						85 단추 (disabled) 1단계 - Encoder 유지보수 Arm Description: Live mutation 승인, 현재 TW[20]/TW[19] capability, 사용 가능한 durable 유지보수 journal 및 1단계의 네 가지 확인이 필요합니다. Arm은 정확한 요청을 PC 메모리에만 준비하며 0x7E53을 전송하지 않습니다. ID: ButtonArmTestReset
> 							86 텍스트 (disabled) 1단계 - Encoder 유지보수 Arm
> 						87 확인란 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다. ID: CheckTestResetFinalConfirmed
> 							88 텍스트 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다.
> 						89 단추 (disabled) 2단계 - Encoder 유지보수 1회 실행 Description: 변경되지 않은 PC 전용 arm 요청, live mutation 승인, 현재 capability, 사용 가능한 durable 유지보수 journal 및 2단계 확인이 필요합니다. Execute는 0x7E53을 한 번 전송합니다. ID: ButtonExecuteTestReset
> 							90 텍스트 (disabled) 2단계 - Encoder 유지보수 1회 실행
> 						91 텍스트 Arm되거나 대기 중인 encoder 유지보수 작업이 없습니다. ID: TextTestResetResult
> 					92 그룹 저장된 Mutation 복구
> 						93 텍스트 저장된 Mutation 복구
> 						94 텍스트 Mutation journal 준비됨. 해결되지 않은 durable write record가 없습니다. ID: TextPersistedMutationStatus
> 						95 확인란 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다. ID: CheckPersistedMutationPhysicallyVerified
> 							96 텍스트 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.
> 						97 단추 (disabled) 복구된 Mutation 승인 ID: ButtonAcknowledgePersistedMutation
> 							98 텍스트 (disabled) 복구된 Mutation 승인
> 					99 그룹 비동기 SDO Read / Write ticket
> 						100 텍스트 비동기 SDO Read / Write ticket
> 						101 텍스트 작업
> 						102 콤보 상자 ID: ComboSdoOperation
> 						103 텍스트 슬레이브 참조 (1..4)
> 						104 편집 ID: TextSdoSlaveReference
> 						105 텍스트 객체 인덱스 (0x0001..0xFFFF)
> 						106 편집 ID: TextSdoIndex
> 						107 텍스트 서브인덱스 (0..255)
> 						108 편집 ID: TextSdoSubIndex
> 						109 텍스트 값 형식
> 						110 콤보 상자 ID: ComboSdoValueType
> 						111 텍스트 데이터 길이 (1, 2 또는 4)
> 						112 콤보 상자 ID: ComboSdoDataLength
> 						113 텍스트 Timeout (1..60000 cycle)
> 						114 편집 ID: TextSdoTimeoutCycles
> 						115 텍스트 Write 값 (10진수 또는 raw 0x hex)
> 						116 편집 (disabled) ID: TextSdoWriteData
> 						117 텍스트 SDK 승인 SDO Write target
> 						118 콤보 상자 (disabled) ID: ComboSdoWriteTarget
> 						119 단추 SDO Read 전송 Description: Write mode는 현재 session의 same-value 활성화 증명 뒤 2단계 확인을 사용합니다. Read mode는 추적되는 SDO Read 하나를 전송합니다. ID: ButtonSubmitSdo
> 							120 텍스트 SDO Read 전송
> 						121 단추 SDO Inline 읽기 (terminal 대기) Description: 일반 1/2/4-byte SDO Read 하나를 전송하고 terminal 결과를 기다립니다. SDO Write, 필수 Write readback 또는 CREVIS I/O는 실행하지 않습니다. ID: ButtonReadSdoInline
> 							122 텍스트 SDO Inline 읽기 (terminal 대기)
> 						123 단추 (disabled) Inline 대기 취소 (PC만) Description: PC 측 Inline Read 대기만 취소합니다. CancelOperation을 전송하거나 PLC ticket을 취소하거나 SDO request를 재시도하지 않습니다. ID: ButtonCancelSdoInlineWait
> 							124 텍스트 (disabled) Inline 대기 취소 (PC만)
> 						125 단추 (disabled) 필수 정확 Readback 불러오기 Description: 마지막으로 성공한 SDO Write를 확인하는 데 필요한 정확한 read request를 복원합니다. 명령은 전송하지 않습니다. ID: ButtonLoadRequiredSdoReadback
> 							126 텍스트 (disabled) 필수 정확 Readback 불러오기
> 						127 단추 (disabled) Ticket 새로고침 ID: ButtonRefreshDiagnosticOperation
> 							128 텍스트 (disabled) Ticket 새로고침
> 						129 단추 (disabled) 결과 다운로드 ID: ButtonDownloadSdoResult
> 							130 텍스트 (disabled) 결과 다운로드
> 						131 단추 (disabled) 결과 저장 ID: ButtonExportSdoResult
> 							132 텍스트 (disabled) 결과 저장
> 						133 단추 (disabled) Ticket 취소 ID: ButtonCancelDiagnosticOperation
> 							134 텍스트 (disabled) Ticket 취소
> 						135 텍스트 SDO Read supports exact 1/2/4-byte typed values. Manual SDO Write additionally requires a four-ticket Same-Value qualification PASS bound to the exact current session and PLC identity; arbitrary object writes remain blocked. ID: TextDiagnosticOperationSummary
> 					136 그룹 D5 SDO fault/recovery qualification (읽기 전용)
> 						137 텍스트 D5 SDO fault/recovery qualification (읽기 전용)
> 						138 텍스트 이 runner는 SDO를 쓰거나 EtherCAT fault를 주입하지 않습니다. Abort qualification은 제조사 승인된 존재하지 않는 읽기 전용 object/sub-index를 사용합니다. Contention, timeout, queued-cancel qualification은 0x6061:0 Int8/1만 사용합니다. Abrupt Disconnect는 RPC Close(0x405D)를 보내지 않고 API TCP transport만 의도적으로 끊은 뒤 재연결하여 0x6061:0을 정확히 두 번 읽습니다. 선택한 Slave 1..4는 _LMCAxis1..4에 매핑되며 모든 runner는 D5 ticket 전송 전에 PowerOn=False, Standstill=True 위치 sample 3회가 안정적으로 일치해야 합니다.
> 						139 텍스트 Abort PASS: 안정적인 BootId/MapRevision -> baseline -> 정확한 SDO abort -> 동일 값 복구. Contention PASS: 첫 0x6061 ticket 승인 -> 첫 terminal 전에 두 번째 Submit이 정확히 Rejected/ResourceBusy -> 첫 ticket Completed/Success -> 정확히 같은 첫 값으로 세 번째 별도 ticket Completed/Success. Timeout PASS: 0x6061 TimeoutCycles=1 -> 정확한 Expired/TimedOut -> 제한된 ResourceBusy drain 대기 -> 별도 동일 값 복구. Queued Cancel PASS: 0x6061 전송 -> 즉시 one-shot CancelOperation -> 정확한 Cancelled/Cancelled -> 별도 동일 값 복구. Abrupt Disconnect는 앱에서 확인 가능한 새 연결 복구만 PASS할 수 있으며 PLC MarkOrphan/late-callback 증거가 생길 때까지 orphanQualified는 false입니다. 연결 손실 전 terminal 완료 또는 Running race는 PLC orphan 증명에 대해 INCONCLUSIVE입니다. 예상 밖 승인 또는 불확실한 결과는 보존되어 자동 재시도를 차단합니다. Cancel Runner는 PLC Stop을 전송하지 않습니다.
> 						140 텍스트 대상 slave (1..4)
> 						141 편집 ID: TextD5SdoAbortSlaveReference
> 						142 텍스트 Abort 객체 인덱스
> 						143 편집 ID: TextD5SdoAbortIndex
> 						144 텍스트 Abort 서브인덱스
> 						145 편집 ID: TextD5SdoAbortSubIndex
> 						146 텍스트 Abort 값 형식
> 						147 콤보 상자 ID: ComboD5SdoAbortValueType
> 						148 텍스트 데이터 길이
> 						149 편집 ID: TextD5SdoAbortDataLength
> 						150 텍스트 일반/복구 timeout cycle
> 						151 편집 ID: TextD5SdoAbortTimeoutCycles
> 						152 단추 D5 Abort -> 복구 실행 ID: ButtonRunD5SdoAbortQualification
> 							153 텍스트 D5 Abort -> 복구 실행
> 						154 단추 D5 Contention -> 복구 실행 ID: ButtonRunD5SdoContentionQualification
> 							155 텍스트 D5 Contention -> 복구 실행
> 						156 단추 D5 Timeout -> 복구 실행 ID: ButtonRunD5SdoTimeoutQualification
> 							157 텍스트 D5 Timeout -> 복구 실행
> 						158 단추 D5 Queued Cancel -> 복구 실행 ID: ButtonRunD5SdoQueuedCancelQualification
> 							159 텍스트 D5 Queued Cancel -> 복구 실행
> 						160 단추 D5 Abrupt Disconnect -> 앱 복구 실행 ID: ButtonRunD5SdoDisconnectRecoveryQualification
> 							161 텍스트 D5 Abrupt Disconnect -> 앱 복구 실행
> 						162 단추 (disabled) Runner 취소 (PLC Stop 아님) ID: ButtonCancelD5SdoQualification
> 							163 텍스트 (disabled) Runner 취소 (PLC Stop 아님)
> 						164 단추 QTEST 로그 저장 ID: ButtonSaveD5SdoQualificationLog
> 							165 텍스트 QTEST 로그 저장
> 						166 진행률 표시줄 ID: ProgressD5SdoQualification
> 						167 텍스트 아직 qualification을 실행하지 않았습니다. ID: TextD5SdoQualificationProgress
> 						168 편집 ID: TextD5SdoQualificationSummary
> 							169 스크롤 막대 ID: HorizontalScrollBar
> 								170 단추 (disabled) ID: PART_LineLeftButton
> 								171 단추 (disabled) ID: PageLeft
> 								172 단추 (disabled) ID: PageRight
> 								173 엄지 (disabled)
> 								174 단추 (disabled) ID: PART_LineRightButton
> 					175 그룹 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						176 텍스트 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						177 텍스트 실제 WRITE: 이 runner는 승인된 target을 읽고 정확히 같은 4 byte를 쓴 뒤 정확한 readback을 요구합니다. Sentinel을 쓰지 않으며 자동 복원이나 재전송을 수행하지 않습니다.
> 						178 텍스트 PLC가 bit 8/9/13을 광고하고, 이 SDK build가 승인 target을 정확히 하나만 노출하고, durable mutation journal을 사용할 수 있고, 작업자 확인 4개가 모두 선택돼야 버튼이 fail-closed 상태에서 풀립니다. 확인 후 선택 축의 PowerOn=False, Standstill=True, 안정 위치를 다시 검사하고 target도 다시 읽어 baseline과 계속 일치해야 Write를 전송합니다.
> 						179 텍스트 SDK 승인 단일 target
> 						180 콤보 상자 ID: ComboD5SdoWriteQualificationTarget
> 						181 텍스트 Timeout (1..60000 cycle)
> 						182 편집 ID: TextD5SdoWriteQualificationTimeoutCycles
> 						183 확인란 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다. ID: CheckConfirmD5SdoWriteUi24Unused
> 							184 텍스트 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다.
> 						185 확인란 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다. ID: CheckConfirmD5SdoWriteOriginalRecorded
> 							186 텍스트 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다.
> 						187 확인란 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다. ID: CheckConfirmD5SdoWriteCaptureRunning
> 							188 텍스트 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다.
> 						189 확인란 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다. ID: CheckConfirmD5SdoWriteSingleWriter
> 							190 텍스트 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다.
> 						191 단추 (disabled) 동일 값 SDO Write Qualification 실행 Description: CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: ButtonRunD5SdoWriteSameValueQualification
> 							192 텍스트 (disabled) 동일 값 SDO Write Qualification 실행
> 						193 텍스트 준비 상태 matrix / 다음 시도 (cache, wire 전송 없음)
> 						194 텍스트 OVERALL CLOSED | EVALUATION_WIRE=NONE SDK POLICY FAIL | blockers=CapabilityObservationNotCurrent | targets=1 PLC CAPS CACHED | bit8/read=1 bit9/write=1 bit13/general=1 IDENTITY PASS | BootId=0x00000042 MapRevision=0x957F101E PAYLOAD PASS | SDO=4 req=1320 resp=2040 cycleUs=1000 QUAL TARGET PASS | selected=Reserved diagnostic UI[24] | Slave 1 | 0x2F00:24 | Int32 | -1073741823..1073741823 RUNNER PASS | connected=True idle=True motionClear=True manualTicketClear=True mutationInterlockClear=True JOURNAL PASS CONFIRMATIONS FAIL | 0/4 AXIS PROOF PENDING_RUNNER_PREFLIGHT | PowerOn=False, Standstill=True, stable position are not cached here. MANUAL WRITE CLOSED_RUN_SAME_VALUE_FIRST | proof never transfers across connection sessions or PLC identity changes. NEXT CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: TextD5SdoWriteQualificationGateStatus
> 						195 텍스트 마지막 시도 / 현재 실행
> 						196 텍스트 아직 동일 값 SDO Write qualification을 실행하지 않았습니다. ID: TextD5SdoWriteQualificationSummary
> 					197 그룹 PI Write 정책 gate
> 						198 텍스트 PI Write 정책 gate
> 						199 텍스트 EtherCAT / PI 탭에서 WritableByPolicy Catalog 항목을 정확히 하나 선택하십시오. Schema v1이 payload를 정의하고 compile-time allowlist가 승인될 때까지 호출은 fail-closed입니다. DS402 ControlWord와 target object는 영구 차단됩니다.
> 						200 텍스트 Raw 값 (10진수 또는 0x...)
> 						201 편집 (disabled) ID: TextPiWriteRawValue
> 						202 단추 (disabled) PI Write 전송 ID: ButtonSubmitPiWrite
> 							203 텍스트 (disabled) PI Write 전송
> 					204 스크롤 막대 ID: VerticalScrollBar
> 						205 단추 (disabled) ID: PART_LineUpButton
> 						206 단추 (disabled) ID: PageUp
> 						207 단추 (disabled) ID: PageDown
> 						208 엄지 (disabled)
> 						209 단추 (disabled) ID: PART_LineDownButton
> 			210 탭 항목 (selectable) 읽기 전용 API
> 				211 텍스트 읽기 전용 API
> 		212 그룹 실행 로그 / 콜백 진단 ID: ExpanderExecutionLog
> 			213 단추 실행 로그 / 콜백 진단 ID: HeaderSite
> 				214 텍스트 실행 로그 / 콜백 진단
> 			215 단추 로그 복사
> 				216 텍스트 로그 복사
> 			217 단추 로그 지우기
> 				218 텍스트 로그 지우기
> 			219 편집 ID: TextExecutionLog
> 				220 스크롤 막대 ID: VerticalScrollBar
> 					221 단추 (disabled) ID: PART_LineUpButton
> 					222 단추 (disabled) ID: PageUp
> 					223 단추 (disabled) ID: PageDown
> 					224 엄지 (disabled)
> 					225 단추 (disabled) ID: PART_LineDownButton
> 				226 스크롤 막대 ID: HorizontalScrollBar
> 					227 단추 (disabled) ID: PART_LineLeftButton
> 					228 단추 (disabled) ID: PageLeft
> 					229 단추 (disabled) ID: PageRight
> 					230 엄지 (disabled)
> 					231 단추 (disabled) ID: PART_LineRightButton
> 		232 상태 표시줄 ID: StatusOperationBar
> 			233 텍스트 Connect completed ID: TextOperationState
>
> Document text: ```
> [16:39:10.444] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-20 07:38:58 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-20 07:38:55 UTC.
> [16:39:10.445] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [16:45:55.685] Connect started.
> [16:45:55.773] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:45:55.785] Ignored stale connection-state event Connecting from an older transport session.
> [16:45:55.785] Connection state Connecting -> Connected
> [16:45:55.893] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:45:55.904] Connect PASS.
> [16:45:58.785] Close Connection started.
> [16:45:58.789] Connection state Connected -> Closing
> [16:45:58.805] Close Connection PASS.
> [16:46:01.250] Connect started.
> [16:46:01.252] Connection state Disconnected -> Connecting
> [16:46:01.264] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:01.265] Connection state Connecting -> Connected
> [16:46:01.339] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:01.339] Connect PASS.
> [16:46:02.611] Close Connection started.
> [16:46:02.611] Connection state Connected -> Closing
> [16:46:02.621] Close Connection PASS.
> [16:46:06.999] Connect started.
> [16:46:07.001] Connection state Disconnected -> Connecting
> [16:46:07.011] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:07.012] Connection state Connecting -> Connected
> [16:46:07.084] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:07.084] Connect PASS.
> [16:47:58.941] Submit SDO Read started.
> [16:47:58.966] QTEST|utc=2026-08-20T07:47:58.966Z|elapsedMs=0|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [16:47:58.973] QTEST|utc=2026-08-20T07:47:58.973Z|elapsedMs=8|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [16:47:58.981] QTEST|utc=2026-08-20T07:47:58.981Z|elapsedMs=16|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [16:47:59.055] QTEST|utc=2026-08-20T07:47:59.055Z|elapsedMs=91|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=135|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [16:47:59.057] QTEST|utc=2026-08-20T07:47:59.057Z|elapsedMs=93|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|ticket=135|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=135|quarantine=false
> [16:47:59.060] Submit SDO Read PASS.
> [16:47:59.096] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000087
> [16:47:59.117] QTEST|utc=2026-08-20T07:47:59.117Z|elapsedMs=151|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=135|state=Completed|outcome=Success|verdict=CLEARED
> [16:47:59.118] QTEST|utc=2026-08-20T07:47:59.118Z|elapsedMs=152|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [16:47:59.124] Callback D5 authoritative TCP status processed. TicketId=0x00000087
> [17:04:28.599] Submit SDO Read started.
> [17:04:28.610] QTEST|utc=2026-08-20T08:04:28.610Z|elapsedMs=0|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [17:04:28.619] QTEST|utc=2026-08-20T08:04:28.619Z|elapsedMs=9|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [17:04:28.622] QTEST|utc=2026-08-20T08:04:28.622Z|elapsedMs=11|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [17:04:28.631] QTEST|utc=2026-08-20T08:04:28.631Z|elapsedMs=20|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=136|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [17:04:28.633] QTEST|utc=2026-08-20T08:04:28.633Z|elapsedMs=22|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|ticket=136|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=136|quarantine=false
> [17:04:28.659] Submit SDO Read PASS.
> [17:04:28.677] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000088
> [17:04:28.690] QTEST|utc=2026-08-20T08:04:28.690Z|elapsedMs=79|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=136|state=Completed|outcome=Success|verdict=CLEARED
> [17:04:28.692] QTEST|utc=2026-08-20T08:04:28.692Z|elapsedMs=81|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [17:04:28.696] Callback D5 authoritative TCP status processed. TicketId=0x00000088
> [17:04:39.215] Close Connection started.
> [17:04:39.233] Connection state Connected -> Closing
> [17:04:39.253] Close Connection PASS.
> [17:04:52.865] Connect started.
> [17:04:52.877] Connection state Disconnected -> Connecting
> [17:04:52.931] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [17:04:52.990] Connection state Connecting -> Connected
> [17:04:53.069] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [17:04:53.076] Connect PASS.
> ```
> ````
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "재연결 후 GD-01 읽기 1회 실행",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"No accessibility observation\");\n  const tree = String(observation.accessibility.tree || \"\");\n  const match = tree.match(/\\n\\s*(\\d+) 단추 SDO Read 전송/);\n  if (!match) throw new Error(\"SDO Read submit button not found\");\n  const submitIndex = Number(match[1]);\n  globalThis.state = null;\n  await sky.click({ window: observation.window, element_index: submitIndex });\n  await new Promise((resolve) => setTimeout(resolve, 900));\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ````text
> Window: "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write] Secondary Actions: Raise
> 		1 창 Secondary Actions: Raise
> 			2 단추 In App Menu Expander ID: InAppMenuExpander
> 		3 제목 표시줄
> 			4 메뉴 모음 시스템 ID: MenuBar
> 				5 메뉴 항목 시스템
> 			6 단추 최소화
> 			7 단추 복원
> 			8 단추 닫기
> 		9 텍스트 언어
> 		10 콤보 상자 ID: ComboUiLanguage
> 		11 텍스트 LASAL 모션 제어 API 예제
> 		12 텍스트 모션 제어는 PLC에서 활성 상태입니다. 진단 제어는 PLC가 필요한 기능을 광고한 뒤에만 활성화됩니다. 값은 선택한 application UNIT을 사용하거나 raw DINT로 전달됩니다.
> 		13 그룹 연결 / RPC 콜백
> 			14 텍스트 연결 / RPC 콜백
> 			15 텍스트 PLC IP
> 			16 편집 (disabled) ID: TextRemoteIp
> 			17 텍스트 TCP 포트
> 			18 편집 (disabled) ID: TextRemotePort
> 			19 텍스트 PC 로컬 IPv4
> 			20 편집 (disabled) ID: TextLocalIp
> 			21 텍스트 콜백 UDP 포트
> 			22 편집 (disabled) ID: TextCallbackPort
> 			23 단추 연결 ID: ButtonConnect
> 				24 텍스트 연결
> 			25 단추 닫기 ID: ButtonCloseConnection
> 				26 텍스트 닫기
> 			27 텍스트 연결 상태
> 			28 텍스트 연결됨 ID: TextConnectionState
> 			29 텍스트 콜백 수신기
> 			30 텍스트 수신 대기 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			31 그룹 RPC 초기화 / 콜백 증거 ID: ExpanderRpcCallbackEvidence
> 				32 단추 RPC 초기화 / 콜백 증거 ID: HeaderSite
> 					33 텍스트 RPC 초기화 / 콜백 증거
> 				34 텍스트 RPC 초기화 증거
> 				35 텍스트 Attempt=4, Outcome=Connected, CandidateOrdinal=1, Remote=10.10.150.1:4000, Local=10.10.150.13, RequestedCallback=10.10.150.13:5000, Mode=Version2WakeHint, BoundCallback=10.10.150.13:5000, LocalSessionGeneration=1, 0x8080Attempts=1, Retry=False, InitOutcome=Succeeded, StartedUtc=2026-08-20T08:04:52.8760280Z, CompletedUtc=2026-08-20T08:04:52.8810342Z, LastACK={FrameValid=True, HeaderStatus=0, HeaderReserved=0, PayloadLength=24, HasCommandResult=False, CommandStatus=0, ErrorId=0}, Current=Active ID: TextRpcInitialization
> 				36 텍스트 콜백 v2 등록
> 				37 텍스트 Status=0, ErrorId=0, Version=2, MaxDatagram=52, BootId=0x00000042, SessionEpoch=30, Flags=0x00000000, Cookie=0x2A978FF0DA1E78F3, ListenerGeneration=2, Source=10.10.150.1, EventMask=0x00000001, LocalSessionGeneration=1 ID: TextCallbackRegistration
> 				38 텍스트 PC 콜백 수신 증거
> 				39 텍스트 Accepted=0, Rejected=0, Duplicate=0, OutOfOrder=0 ID: TextCallbackCounters
> 				40 텍스트 Last decision=None ID: TextCallbackLastDecision
> 				41 텍스트 연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. UDP wake hint는 상태 판단 근거가 아니며, 동작 상태는 일치하는 TCP status 응답을 받은 뒤에만 변경됩니다.
> 		42 그룹 안전 / 복구 상세 정보 ID: ExpanderSafetyAndRecoveryDetails
> 			43 단추 안전 / 복구 상세 정보 ID: HeaderSite
> 				44 텍스트 안전 / 복구 상세 정보
> 		45 탭 ID: TabsMotion
> 			46 탭 항목 (selectable) 단축
> 				47 텍스트 단축
> 			48 탭 항목 (selectable) 그룹 모션
> 				49 텍스트 그룹 모션
> 			50 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				51 텍스트 EtherCAT / CREVIS / PI
> 			52 탭 항목 (selectable) Bulk 스냅샷
> 				53 텍스트 Bulk 스냅샷
> 			54 탭 항목 (selectable) 레코더
> 				55 텍스트 레코더
> 			56 탭 항목 (selectable) SDO / Write 정책
> 				57 텍스트 SDO / Write 정책
> 				58 창 ID: ScrollDiagnosticsOperations
> 					59 그룹 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19]) ID: GroupEncoderMaintenance
> 						60 텍스트 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])
> 						61 텍스트 파괴적 테스트 전용: TW[20]은 0x20FC:0x02에 UInt16 1을 씁니다. TW[19]는 0x20FC:0x01에 UInt16 1을 써 absolute multi-turn position을 reset하며 motion 전에 LMC Home이 필요합니다. 정확한 drive 축을 선택하십시오.
> 						62 텍스트 Fixed writes: TW[20]=0x20FC:0x02 <- UInt16 1; TW[19]=0x20FC:0x01 <- UInt16 1 ID: TextEncoderMaintenanceFixedTargets
> 						63 텍스트 Arm 전: 선택한 축의 Power를 끄고 안정적인 standstill과 실제 위치를 독립적으로 확인한 뒤 정확한 0x20FC 명령 지원 여부를 확인하십시오. 전용 0x7E53/0x7E54/0x7E55 경로는 일반 SDO Write와 분리됩니다.
> 						64 텍스트 작업
> 						65 콤보 상자 ID: ComboEncoderMaintenanceKind
> 						66 텍스트 Drive / 축 (1..4)
> 						67 콤보 상자 ID: ComboTestResetAxis
> 						68 텍스트 Timeout (ms, 1..60000)
> 						69 편집 ID: TextTestResetTimeout
> 						70 단추 Encoder 유지보수 Capability 새로고침 ID: ButtonRefreshTestResetCapabilities
> 							71 텍스트 Encoder 유지보수 Capability 새로고침
> 						72 단추 (disabled) Encoder 유지보수 결과 읽기 ID: ButtonReadTestResetStatus
> 							73 텍스트 (disabled) Encoder 유지보수 결과 읽기
> 						74 확인란 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다. ID: CheckTestResetPowerOffVerified
> 							75 텍스트 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다.
> 						76 확인란 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다. ID: CheckTestResetPhysicalPositionVerified
> 							77 텍스트 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다.
> 						78 확인란 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다. ID: CheckTestResetExactTargetVerified
> 							79 텍스트 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다.
> 						80 확인란 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다. ID: CheckEncoderMaintenanceCompatibilityVerified
> 							81 텍스트 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.
> 						82 텍스트 차단: 1단계의 모든 물리 확인과 encoder 호환성 확인이 필요합니다. Encoder 유지보수 작업을 arm하거나 전송하지 않았습니다. ID: TextEncoderMaintenanceArmGateStatus
> 						83 단추 안전 / 복구 상세 정보 열기 Description: 로컬 안전/복구 panel만 펼칩니다. 이 버튼은 PLC 명령을 전송하지 않으며 어떤 record도 폐기하지 않습니다. ID: ButtonOpenEncoderRecoveryDetails
> 							84 텍스트 안전 / 복구 상세 정보 열기
> 						85 단추 (disabled) 1단계 - Encoder 유지보수 Arm Description: Live mutation 승인, 현재 TW[20]/TW[19] capability, 사용 가능한 durable 유지보수 journal 및 1단계의 네 가지 확인이 필요합니다. Arm은 정확한 요청을 PC 메모리에만 준비하며 0x7E53을 전송하지 않습니다. ID: ButtonArmTestReset
> 							86 텍스트 (disabled) 1단계 - Encoder 유지보수 Arm
> 						87 확인란 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다. ID: CheckTestResetFinalConfirmed
> 							88 텍스트 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다.
> 						89 단추 (disabled) 2단계 - Encoder 유지보수 1회 실행 Description: 변경되지 않은 PC 전용 arm 요청, live mutation 승인, 현재 capability, 사용 가능한 durable 유지보수 journal 및 2단계 확인이 필요합니다. Execute는 0x7E53을 한 번 전송합니다. ID: ButtonExecuteTestReset
> 							90 텍스트 (disabled) 2단계 - Encoder 유지보수 1회 실행
> 						91 텍스트 Arm되거나 대기 중인 encoder 유지보수 작업이 없습니다. ID: TextTestResetResult
> 					92 그룹 저장된 Mutation 복구
> 						93 텍스트 저장된 Mutation 복구
> 						94 텍스트 Mutation journal 준비됨. 해결되지 않은 durable write record가 없습니다. ID: TextPersistedMutationStatus
> 						95 확인란 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다. ID: CheckPersistedMutationPhysicallyVerified
> 							96 텍스트 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.
> 						97 단추 (disabled) 복구된 Mutation 승인 ID: ButtonAcknowledgePersistedMutation
> 							98 텍스트 (disabled) 복구된 Mutation 승인
> 					99 그룹 비동기 SDO Read / Write ticket
> 						100 텍스트 비동기 SDO Read / Write ticket
> 						101 텍스트 작업
> 						102 콤보 상자 ID: ComboSdoOperation
> 						103 텍스트 슬레이브 참조 (1..4)
> 						104 편집 ID: TextSdoSlaveReference
> 						105 텍스트 객체 인덱스 (0x0001..0xFFFF)
> 						106 편집 ID: TextSdoIndex
> 						107 텍스트 서브인덱스 (0..255)
> 						108 편집 ID: TextSdoSubIndex
> 						109 텍스트 값 형식
> 						110 콤보 상자 ID: ComboSdoValueType
> 						111 텍스트 데이터 길이 (1, 2 또는 4)
> 						112 콤보 상자 ID: ComboSdoDataLength
> 						113 텍스트 Timeout (1..60000 cycle)
> 						114 편집 ID: TextSdoTimeoutCycles
> 						115 텍스트 Write 값 (10진수 또는 raw 0x hex)
> 						116 편집 (disabled) ID: TextSdoWriteData
> 						117 텍스트 SDK 승인 SDO Write target
> 						118 콤보 상자 (disabled) ID: ComboSdoWriteTarget
> 						119 단추 SDO Read 전송 Description: Write mode는 현재 session의 same-value 활성화 증명 뒤 2단계 확인을 사용합니다. Read mode는 추적되는 SDO Read 하나를 전송합니다. ID: ButtonSubmitSdo
> 							120 텍스트 SDO Read 전송
> 						121 단추 SDO Inline 읽기 (terminal 대기) Description: 일반 1/2/4-byte SDO Read 하나를 전송하고 terminal 결과를 기다립니다. SDO Write, 필수 Write readback 또는 CREVIS I/O는 실행하지 않습니다. ID: ButtonReadSdoInline
> 							122 텍스트 SDO Inline 읽기 (terminal 대기)
> 						123 단추 (disabled) Inline 대기 취소 (PC만) Description: PC 측 Inline Read 대기만 취소합니다. CancelOperation을 전송하거나 PLC ticket을 취소하거나 SDO request를 재시도하지 않습니다. ID: ButtonCancelSdoInlineWait
> 							124 텍스트 (disabled) Inline 대기 취소 (PC만)
> 						125 단추 (disabled) 필수 정확 Readback 불러오기 Description: 마지막으로 성공한 SDO Write를 확인하는 데 필요한 정확한 read request를 복원합니다. 명령은 전송하지 않습니다. ID: ButtonLoadRequiredSdoReadback
> 							126 텍스트 (disabled) 필수 정확 Readback 불러오기
> 						127 단추 Ticket 새로고침 ID: ButtonRefreshDiagnosticOperation
> 							128 텍스트 Ticket 새로고침
> 						129 단추 (disabled) 결과 다운로드 ID: ButtonDownloadSdoResult
> 							130 텍스트 (disabled) 결과 다운로드
> 						131 단추 결과 저장 ID: ButtonExportSdoResult
> 							132 텍스트 결과 저장
> 						133 단추 (disabled) Ticket 취소 ID: ButtonCancelDiagnosticOperation
> 							134 텍스트 (disabled) Ticket 취소
> 						135 텍스트 TicketId=137, Kind=SDORead, State=Completed, Outcome=Success SubmitCycle=14322828, CompletionCycle=14322849, ErrorId=0, Detail=0x00000000 ResultType=Int8, ResultLength=1, Data=08 ID: TextDiagnosticOperationSummary
> 					136 그룹 D5 SDO fault/recovery qualification (읽기 전용)
> 						137 텍스트 D5 SDO fault/recovery qualification (읽기 전용)
> 						138 텍스트 이 runner는 SDO를 쓰거나 EtherCAT fault를 주입하지 않습니다. Abort qualification은 제조사 승인된 존재하지 않는 읽기 전용 object/sub-index를 사용합니다. Contention, timeout, queued-cancel qualification은 0x6061:0 Int8/1만 사용합니다. Abrupt Disconnect는 RPC Close(0x405D)를 보내지 않고 API TCP transport만 의도적으로 끊은 뒤 재연결하여 0x6061:0을 정확히 두 번 읽습니다. 선택한 Slave 1..4는 _LMCAxis1..4에 매핑되며 모든 runner는 D5 ticket 전송 전에 PowerOn=False, Standstill=True 위치 sample 3회가 안정적으로 일치해야 합니다.
> 						139 텍스트 Abort PASS: 안정적인 BootId/MapRevision -> baseline -> 정확한 SDO abort -> 동일 값 복구. Contention PASS: 첫 0x6061 ticket 승인 -> 첫 terminal 전에 두 번째 Submit이 정확히 Rejected/ResourceBusy -> 첫 ticket Completed/Success -> 정확히 같은 첫 값으로 세 번째 별도 ticket Completed/Success. Timeout PASS: 0x6061 TimeoutCycles=1 -> 정확한 Expired/TimedOut -> 제한된 ResourceBusy drain 대기 -> 별도 동일 값 복구. Queued Cancel PASS: 0x6061 전송 -> 즉시 one-shot CancelOperation -> 정확한 Cancelled/Cancelled -> 별도 동일 값 복구. Abrupt Disconnect는 앱에서 확인 가능한 새 연결 복구만 PASS할 수 있으며 PLC MarkOrphan/late-callback 증거가 생길 때까지 orphanQualified는 false입니다. 연결 손실 전 terminal 완료 또는 Running race는 PLC orphan 증명에 대해 INCONCLUSIVE입니다. 예상 밖 승인 또는 불확실한 결과는 보존되어 자동 재시도를 차단합니다. Cancel Runner는 PLC Stop을 전송하지 않습니다.
> 						140 텍스트 대상 slave (1..4)
> 						141 편집 ID: TextD5SdoAbortSlaveReference
> 						142 텍스트 Abort 객체 인덱스
> 						143 편집 ID: TextD5SdoAbortIndex
> 						144 텍스트 Abort 서브인덱스
> 						145 편집 ID: TextD5SdoAbortSubIndex
> 						146 텍스트 Abort 값 형식
> 						147 콤보 상자 ID: ComboD5SdoAbortValueType
> 						148 텍스트 데이터 길이
> 						149 편집 ID: TextD5SdoAbortDataLength
> 						150 텍스트 일반/복구 timeout cycle
> 						151 편집 ID: TextD5SdoAbortTimeoutCycles
> 						152 단추 D5 Abort -> 복구 실행 ID: ButtonRunD5SdoAbortQualification
> 							153 텍스트 D5 Abort -> 복구 실행
> 						154 단추 D5 Contention -> 복구 실행 ID: ButtonRunD5SdoContentionQualification
> 							155 텍스트 D5 Contention -> 복구 실행
> 						156 단추 D5 Timeout -> 복구 실행 ID: ButtonRunD5SdoTimeoutQualification
> 							157 텍스트 D5 Timeout -> 복구 실행
> 						158 단추 D5 Queued Cancel -> 복구 실행 ID: ButtonRunD5SdoQueuedCancelQualification
> 							159 텍스트 D5 Queued Cancel -> 복구 실행
> 						160 단추 D5 Abrupt Disconnect -> 앱 복구 실행 ID: ButtonRunD5SdoDisconnectRecoveryQualification
> 							161 텍스트 D5 Abrupt Disconnect -> 앱 복구 실행
> 						162 단추 (disabled) Runner 취소 (PLC Stop 아님) ID: ButtonCancelD5SdoQualification
> 							163 텍스트 (disabled) Runner 취소 (PLC Stop 아님)
> 						164 단추 QTEST 로그 저장 ID: ButtonSaveD5SdoQualificationLog
> 							165 텍스트 QTEST 로그 저장
> 						166 진행률 표시줄 ID: ProgressD5SdoQualification
> 						167 텍스트 아직 qualification을 실행하지 않았습니다. ID: TextD5SdoQualificationProgress
> 						168 편집 ID: TextD5SdoQualificationSummary
> 							169 스크롤 막대 ID: HorizontalScrollBar
> 								170 단추 (disabled) ID: PART_LineLeftButton
> 								171 단추 (disabled) ID: PageLeft
> 								172 단추 (disabled) ID: PageRight
> 								173 엄지 (disabled)
> 								174 단추 (disabled) ID: PART_LineRightButton
> 					175 그룹 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						176 텍스트 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						177 텍스트 실제 WRITE: 이 runner는 승인된 target을 읽고 정확히 같은 4 byte를 쓴 뒤 정확한 readback을 요구합니다. Sentinel을 쓰지 않으며 자동 복원이나 재전송을 수행하지 않습니다.
> 						178 텍스트 PLC가 bit 8/9/13을 광고하고, 이 SDK build가 승인 target을 정확히 하나만 노출하고, durable mutation journal을 사용할 수 있고, 작업자 확인 4개가 모두 선택돼야 버튼이 fail-closed 상태에서 풀립니다. 확인 후 선택 축의 PowerOn=False, Standstill=True, 안정 위치를 다시 검사하고 target도 다시 읽어 baseline과 계속 일치해야 Write를 전송합니다.
> 						179 텍스트 SDK 승인 단일 target
> 						180 콤보 상자 ID: ComboD5SdoWriteQualificationTarget
> 						181 텍스트 Timeout (1..60000 cycle)
> 						182 편집 ID: TextD5SdoWriteQualificationTimeoutCycles
> 						183 확인란 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다. ID: CheckConfirmD5SdoWriteUi24Unused
> 							184 텍스트 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다.
> 						185 확인란 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다. ID: CheckConfirmD5SdoWriteOriginalRecorded
> 							186 텍스트 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다.
> 						187 확인란 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다. ID: CheckConfirmD5SdoWriteCaptureRunning
> 							188 텍스트 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다.
> 						189 확인란 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다. ID: CheckConfirmD5SdoWriteSingleWriter
> 							190 텍스트 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다.
> 						191 단추 (disabled) 동일 값 SDO Write Qualification 실행 Description: CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: ButtonRunD5SdoWriteSameValueQualification
> 							192 텍스트 (disabled) 동일 값 SDO Write Qualification 실행
> 						193 텍스트 준비 상태 matrix / 다음 시도 (cache, wire 전송 없음)
> 						194 텍스트 OVERALL CLOSED | EVALUATION_WIRE=NONE SDK POLICY FAIL | blockers=CapabilityObservationNotCurrent | targets=1 PLC CAPS CACHED | bit8/read=1 bit9/write=1 bit13/general=1 IDENTITY PASS | BootId=0x00000042 MapRevision=0x957F101E PAYLOAD PASS | SDO=4 req=1320 resp=2040 cycleUs=1000 QUAL TARGET PASS | selected=Reserved diagnostic UI[24] | Slave 1 | 0x2F00:24 | Int32 | -1073741823..1073741823 RUNNER PASS | connected=True idle=True motionClear=True manualTicketClear=True mutationInterlockClear=True JOURNAL PASS CONFIRMATIONS FAIL | 0/4 AXIS PROOF PENDING_RUNNER_PREFLIGHT | PowerOn=False, Standstill=True, stable position are not cached here. MANUAL WRITE CLOSED_RUN_SAME_VALUE_FIRST | proof never transfers across connection sessions or PLC identity changes. NEXT CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: TextD5SdoWriteQualificationGateStatus
> 						195 텍스트 마지막 시도 / 현재 실행
> 						196 텍스트 아직 동일 값 SDO Write qualification을 실행하지 않았습니다. ID: TextD5SdoWriteQualificationSummary
> 					197 그룹 PI Write 정책 gate
> 						198 텍스트 PI Write 정책 gate
> 						199 텍스트 EtherCAT / PI 탭에서 WritableByPolicy Catalog 항목을 정확히 하나 선택하십시오. Schema v1이 payload를 정의하고 compile-time allowlist가 승인될 때까지 호출은 fail-closed입니다. DS402 ControlWord와 target object는 영구 차단됩니다.
> 						200 텍스트 Raw 값 (10진수 또는 0x...)
> 						201 편집 (disabled) ID: TextPiWriteRawValue
> 						202 단추 (disabled) PI Write 전송 ID: ButtonSubmitPiWrite
> 							203 텍스트 (disabled) PI Write 전송
> 					204 스크롤 막대 ID: VerticalScrollBar
> 						205 단추 (disabled) ID: PART_LineUpButton
> 						206 단추 (disabled) ID: PageUp
> 						207 단추 (disabled) ID: PageDown
> 						208 엄지 (disabled)
> 						209 단추 (disabled) ID: PART_LineDownButton
> 			210 탭 항목 (selectable) 읽기 전용 API
> 				211 텍스트 읽기 전용 API
> 		212 그룹 실행 로그 / 콜백 진단 ID: ExpanderExecutionLog
> 			213 단추 실행 로그 / 콜백 진단 ID: HeaderSite
> 				214 텍스트 실행 로그 / 콜백 진단
> 			215 단추 로그 복사
> 				216 텍스트 로그 복사
> 			217 단추 로그 지우기
> 				218 텍스트 로그 지우기
> 			219 편집 ID: TextExecutionLog
> 				220 스크롤 막대 ID: VerticalScrollBar
> 					221 단추 (disabled) ID: PART_LineUpButton
> 					222 단추 (disabled) ID: PageUp
> 					223 단추 (disabled) ID: PageDown
> 					224 엄지 (disabled)
> 					225 단추 (disabled) ID: PART_LineDownButton
> 				226 스크롤 막대 ID: HorizontalScrollBar
> 					227 단추 (disabled) ID: PART_LineLeftButton
> 					228 단추 (disabled) ID: PageLeft
> 					229 단추 (disabled) ID: PageRight
> 					230 엄지 (disabled)
> 					231 단추 (disabled) ID: PART_LineRightButton
> 		232 상태 표시줄 ID: StatusOperationBar
> 			233 텍스트 Callback D5 status refresh completed ID: TextOperationState
>
> Document text: ```
> [16:39:10.444] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-20 07:38:58 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-20 07:38:55 UTC.
> [16:39:10.445] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [16:45:55.685] Connect started.
> [16:45:55.773] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:45:55.785] Ignored stale connection-state event Connecting from an older transport session.
> [16:45:55.785] Connection state Connecting -> Connected
> [16:45:55.893] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:45:55.904] Connect PASS.
> [16:45:58.785] Close Connection started.
> [16:45:58.789] Connection state Connected -> Closing
> [16:45:58.805] Close Connection PASS.
> [16:46:01.250] Connect started.
> [16:46:01.252] Connection state Disconnected -> Connecting
> [16:46:01.264] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:01.265] Connection state Connecting -> Connected
> [16:46:01.339] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:01.339] Connect PASS.
> [16:46:02.611] Close Connection started.
> [16:46:02.611] Connection state Connected -> Closing
> [16:46:02.621] Close Connection PASS.
> [16:46:06.999] Connect started.
> [16:46:07.001] Connection state Disconnected -> Connecting
> [16:46:07.011] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:07.012] Connection state Connecting -> Connected
> [16:46:07.084] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:07.084] Connect PASS.
> [16:47:58.941] Submit SDO Read started.
> [16:47:58.966] QTEST|utc=2026-08-20T07:47:58.966Z|elapsedMs=0|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [16:47:58.973] QTEST|utc=2026-08-20T07:47:58.973Z|elapsedMs=8|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [16:47:58.981] QTEST|utc=2026-08-20T07:47:58.981Z|elapsedMs=16|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [16:47:59.055] QTEST|utc=2026-08-20T07:47:59.055Z|elapsedMs=91|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=135|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [16:47:59.057] QTEST|utc=2026-08-20T07:47:59.057Z|elapsedMs=93|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|ticket=135|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=135|quarantine=false
> [16:47:59.060] Submit SDO Read PASS.
> [16:47:59.096] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000087
> [16:47:59.117] QTEST|utc=2026-08-20T07:47:59.117Z|elapsedMs=151|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=135|state=Completed|outcome=Success|verdict=CLEARED
> [16:47:59.118] QTEST|utc=2026-08-20T07:47:59.118Z|elapsedMs=152|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [16:47:59.124] Callback D5 authoritative TCP status processed. TicketId=0x00000087
> [17:04:28.599] Submit SDO Read started.
> [17:04:28.610] QTEST|utc=2026-08-20T08:04:28.610Z|elapsedMs=0|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [17:04:28.619] QTEST|utc=2026-08-20T08:04:28.619Z|elapsedMs=9|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [17:04:28.622] QTEST|utc=2026-08-20T08:04:28.622Z|elapsedMs=11|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [17:04:28.631] QTEST|utc=2026-08-20T08:04:28.631Z|elapsedMs=20|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=136|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [17:04:28.633] QTEST|utc=2026-08-20T08:04:28.633Z|elapsedMs=22|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|ticket=136|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=136|quarantine=false
> [17:04:28.659] Submit SDO Read PASS.
> [17:04:28.677] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000088
> [17:04:28.690] QTEST|utc=2026-08-20T08:04:28.690Z|elapsedMs=79|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=136|state=Completed|outcome=Success|verdict=CLEARED
> [17:04:28.692] QTEST|utc=2026-08-20T08:04:28.692Z|elapsedMs=81|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [17:04:28.696] Callback D5 authoritative TCP status processed. TicketId=0x00000088
> [17:04:39.215] Close Connection started.
> [17:04:39.233] Connection state Connected -> Closing
> [17:04:39.253] Close Connection PASS.
> [17:04:52.865] Connect started.
> [17:04:52.877] Connection state Disconnected -> Connecting
> [17:04:52.931] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [17:04:52.990] Connection state Connecting -> Connected
> [17:04:53.069] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [17:04:53.076] Connect PASS.
> [17:05:11.661] Submit SDO Read started.
> [17:05:11.673] QTEST|utc=2026-08-20T08:05:11.673Z|elapsedMs=0|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [17:05:11.681] QTEST|utc=2026-08-20T08:05:11.681Z|elapsedMs=7|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [17:05:11.683] QTEST|utc=2026-08-20T08:05:11.683Z|elapsedMs=9|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=71153dec1118442c891bd7f4d9977378|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [17:05:11.697] QTEST|utc=2026-08-20T08:05:11.697Z|elapsedMs=23|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=137|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [17:05:11.699] QTEST|utc=2026-08-20T08:05:11.699Z|elapsedMs=25|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=71153dec1118442c891bd7f4d9977378|ticket=137|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=137|quarantine=false
> [17:05:11.703] Submit SDO Read PASS.
> [17:05:11.730] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000089
> [17:05:11.745] QTEST|utc=2026-08-20T08:05:11.745Z|elapsedMs=72|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=137|state=Completed|outcome=Success|verdict=CLEARED
> [17:05:11.748] QTEST|utc=2026-08-20T08:05:11.748Z|elapsedMs=74|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [17:05:11.754] Callback D5 authoritative TCP status processed. TicketId=0x00000089
> ```
> ````
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> </details>
>
> 제가 실행한 두 읽기 모두 화면에서 `Completed/Success, Int8, Data=08`로 끝났고, 콜백 카운터는 재연결 전 `1→2`, 재연결 후 새 세션에서 `0→1`로 증가했습니다. 새 캡처는 57패킷으로 종료됐습니다. 이제 ticket 136/137 각각에 대해 UDP와 자동 `0x7E03`가 정확히 한 번인지, Close ACK와 새 Epoch/Cookie까지 패킷에서 판정합니다.
>
> 패킷 판정은 이미 성공입니다. 참고용으로 WPF의 QTEST 로그도 새 파일로 저장하겠습니다. 저장은 PLC 명령을 보내지 않으며, 방금 수행한 두 `run=` 항목을 그대로 보존합니다.
>
> <details><summary>Called 10 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "방금 실행한 QTEST 로그 저장 대화상자 열기",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"No accessibility observation\");\n  const tree = String(observation.accessibility.tree || \"\");\n  const match = tree.match(/\\n\\s*(\\d+) 단추 QTEST 로그 저장/);\n  if (!match) throw new Error(\"QTEST save button not found\");\n  const saveIndex = Number(match[1]);\n  globalThis.state = null;\n  await sky.click({ window: observation.window, element_index: saveIndex });\n  await new Promise((resolve) => setTimeout(resolve, 500));\n  globalThis.windowsAfterSave = await sky.list_windows();\n  nodeRepl.write(JSON.stringify(windowsAfterSave, null, 2));\n}"
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\LasalApiWpfTestApp\\LasalApiWpfTestApp\\bin\\Release\\LasalMotionControlApiExample.exe",
>     "id": 3476900,
>     "title": "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]"
>   },
>   {
>     "app": "OpenAI.Codex_2p2nqsd0c76g0!App",
>     "id": 3933112,
>     "title": "ChatGPT"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe",
>     "id": 131448,
>     "title": "카카오톡"
>   },
>   {
>     "app": "VisualStudio.89931261",
>     "id": 198774,
>     "title": "LasalApiWpfTestApp (실행) - Microsoft Visual Studio"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 724834,
>     "title": "Test6 - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 8390442,
>     "title": "Release - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 1117040,
>     "title": "내 PC - 파일 탐색기"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 4458804,
>     "title": "design - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 5115032,
>     "title": "api - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 4197596,
>     "title": "docs - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 1969662,
>     "title": "다운로드 - 파일 탐색기"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 4263250,
>     "title": "Elmo_Master - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "Chrome",
>     "id": 2165522,
>     "title": "CP-313-eng.pdf - Chrome"
>   },
>   {
>     "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\PicPick\\picpick.exe",
>     "id": 1641218,
>     "title": "픽픽 - 이미지 002"
>   },
>   {
>     "app": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Wireshark\\Wireshark.exe",
>     "id": 1182968,
>     "title": "GD-01 재시험.pcapng"
>   },
>   {
>     "app": "Chrome",
>     "id": 2492842,
>     "title": "Daou Office - 홈 - Chrome"
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "QTEST 저장 동작 결과 확인",
>   "code": "{\n  globalThis.state = await sky.get_window_state({\n    window: targetWindow,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ````text
> Window: "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write] Secondary Actions: Raise
> 		1 창 Secondary Actions: Raise
> 			2 단추 In App Menu Expander ID: InAppMenuExpander
> 		3 제목 표시줄
> 			4 메뉴 모음 시스템 ID: MenuBar
> 				5 메뉴 항목 시스템
> 			6 단추 최소화
> 			7 단추 복원
> 			8 단추 닫기
> 		9 텍스트 언어
> 		10 콤보 상자 ID: ComboUiLanguage
> 		11 텍스트 LASAL 모션 제어 API 예제
> 		12 텍스트 모션 제어는 PLC에서 활성 상태입니다. 진단 제어는 PLC가 필요한 기능을 광고한 뒤에만 활성화됩니다. 값은 선택한 application UNIT을 사용하거나 raw DINT로 전달됩니다.
> 		13 그룹 연결 / RPC 콜백
> 			14 텍스트 연결 / RPC 콜백
> 			15 텍스트 PLC IP
> 			16 편집 (disabled) ID: TextRemoteIp
> 			17 텍스트 TCP 포트
> 			18 편집 (disabled) ID: TextRemotePort
> 			19 텍스트 PC 로컬 IPv4
> 			20 편집 (disabled) ID: TextLocalIp
> 			21 텍스트 콜백 UDP 포트
> 			22 편집 (disabled) ID: TextCallbackPort
> 			23 단추 연결 ID: ButtonConnect
> 				24 텍스트 연결
> 			25 단추 닫기 ID: ButtonCloseConnection
> 				26 텍스트 닫기
> 			27 텍스트 연결 상태
> 			28 텍스트 연결됨 ID: TextConnectionState
> 			29 텍스트 콜백 수신기
> 			30 텍스트 수신 대기 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			31 그룹 RPC 초기화 / 콜백 증거 ID: ExpanderRpcCallbackEvidence
> 				32 단추 RPC 초기화 / 콜백 증거 ID: HeaderSite
> 					33 텍스트 RPC 초기화 / 콜백 증거
> 				34 텍스트 RPC 초기화 증거
> 				35 텍스트 Attempt=4, Outcome=Connected, CandidateOrdinal=1, Remote=10.10.150.1:4000, Local=10.10.150.13, RequestedCallback=10.10.150.13:5000, Mode=Version2WakeHint, BoundCallback=10.10.150.13:5000, LocalSessionGeneration=1, 0x8080Attempts=1, Retry=False, InitOutcome=Succeeded, StartedUtc=2026-08-20T08:04:52.8760280Z, CompletedUtc=2026-08-20T08:04:52.8810342Z, LastACK={FrameValid=True, HeaderStatus=0, HeaderReserved=0, PayloadLength=24, HasCommandResult=False, CommandStatus=0, ErrorId=0}, Current=Active ID: TextRpcInitialization
> 				36 텍스트 콜백 v2 등록
> 				37 텍스트 Status=0, ErrorId=0, Version=2, MaxDatagram=52, BootId=0x00000042, SessionEpoch=30, Flags=0x00000000, Cookie=0x2A978FF0DA1E78F3, ListenerGeneration=2, Source=10.10.150.1, EventMask=0x00000001, LocalSessionGeneration=1 ID: TextCallbackRegistration
> 				38 텍스트 PC 콜백 수신 증거
> 				39 텍스트 Accepted=1, Rejected=0, Duplicate=0, OutOfOrder=0 ID: TextCallbackCounters
> 				40 텍스트 Last decision=AcceptedWakeHint, ProtocolError=None ID: TextCallbackLastDecision
> 				41 텍스트 연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. UDP wake hint는 상태 판단 근거가 아니며, 동작 상태는 일치하는 TCP status 응답을 받은 뒤에만 변경됩니다.
> 		42 그룹 안전 / 복구 상세 정보 ID: ExpanderSafetyAndRecoveryDetails
> 			43 단추 안전 / 복구 상세 정보 ID: HeaderSite
> 				44 텍스트 안전 / 복구 상세 정보
> 		45 탭 ID: TabsMotion
> 			46 탭 항목 (selectable) 단축
> 				47 텍스트 단축
> 			48 탭 항목 (selectable) 그룹 모션
> 				49 텍스트 그룹 모션
> 			50 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				51 텍스트 EtherCAT / CREVIS / PI
> 			52 탭 항목 (selectable) Bulk 스냅샷
> 				53 텍스트 Bulk 스냅샷
> 			54 탭 항목 (selectable) 레코더
> 				55 텍스트 레코더
> 			56 탭 항목 (selectable) SDO / Write 정책
> 				57 텍스트 SDO / Write 정책
> 				58 창 ID: ScrollDiagnosticsOperations
> 					59 그룹 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19]) ID: GroupEncoderMaintenance
> 						60 텍스트 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])
> 						61 텍스트 파괴적 테스트 전용: TW[20]은 0x20FC:0x02에 UInt16 1을 씁니다. TW[19]는 0x20FC:0x01에 UInt16 1을 써 absolute multi-turn position을 reset하며 motion 전에 LMC Home이 필요합니다. 정확한 drive 축을 선택하십시오.
> 						62 텍스트 Fixed writes: TW[20]=0x20FC:0x02 <- UInt16 1; TW[19]=0x20FC:0x01 <- UInt16 1 ID: TextEncoderMaintenanceFixedTargets
> 						63 텍스트 Arm 전: 선택한 축의 Power를 끄고 안정적인 standstill과 실제 위치를 독립적으로 확인한 뒤 정확한 0x20FC 명령 지원 여부를 확인하십시오. 전용 0x7E53/0x7E54/0x7E55 경로는 일반 SDO Write와 분리됩니다.
> 						64 텍스트 작업
> 						65 콤보 상자 ID: ComboEncoderMaintenanceKind
> 						66 텍스트 Drive / 축 (1..4)
> 						67 콤보 상자 ID: ComboTestResetAxis
> 						68 텍스트 Timeout (ms, 1..60000)
> 						69 편집 ID: TextTestResetTimeout
> 						70 단추 Encoder 유지보수 Capability 새로고침 ID: ButtonRefreshTestResetCapabilities
> 							71 텍스트 Encoder 유지보수 Capability 새로고침
> 						72 단추 (disabled) Encoder 유지보수 결과 읽기 ID: ButtonReadTestResetStatus
> 							73 텍스트 (disabled) Encoder 유지보수 결과 읽기
> 						74 확인란 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다. ID: CheckTestResetPowerOffVerified
> 							75 텍스트 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다.
> 						76 확인란 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다. ID: CheckTestResetPhysicalPositionVerified
> 							77 텍스트 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다.
> 						78 확인란 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다. ID: CheckTestResetExactTargetVerified
> 							79 텍스트 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다.
> 						80 확인란 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다. ID: CheckEncoderMaintenanceCompatibilityVerified
> 							81 텍스트 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.
> 						82 텍스트 차단: 1단계의 모든 물리 확인과 encoder 호환성 확인이 필요합니다. Encoder 유지보수 작업을 arm하거나 전송하지 않았습니다. ID: TextEncoderMaintenanceArmGateStatus
> 						83 단추 안전 / 복구 상세 정보 열기 Description: 로컬 안전/복구 panel만 펼칩니다. 이 버튼은 PLC 명령을 전송하지 않으며 어떤 record도 폐기하지 않습니다. ID: ButtonOpenEncoderRecoveryDetails
> 							84 텍스트 안전 / 복구 상세 정보 열기
> 						85 단추 (disabled) 1단계 - Encoder 유지보수 Arm Description: Live mutation 승인, 현재 TW[20]/TW[19] capability, 사용 가능한 durable 유지보수 journal 및 1단계의 네 가지 확인이 필요합니다. Arm은 정확한 요청을 PC 메모리에만 준비하며 0x7E53을 전송하지 않습니다. ID: ButtonArmTestReset
> 							86 텍스트 (disabled) 1단계 - Encoder 유지보수 Arm
> 						87 확인란 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다. ID: CheckTestResetFinalConfirmed
> 							88 텍스트 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다.
> 						89 단추 (disabled) 2단계 - Encoder 유지보수 1회 실행 Description: 변경되지 않은 PC 전용 arm 요청, live mutation 승인, 현재 capability, 사용 가능한 durable 유지보수 journal 및 2단계 확인이 필요합니다. Execute는 0x7E53을 한 번 전송합니다. ID: ButtonExecuteTestReset
> 							90 텍스트 (disabled) 2단계 - Encoder 유지보수 1회 실행
> 						91 텍스트 Arm되거나 대기 중인 encoder 유지보수 작업이 없습니다. ID: TextTestResetResult
> 					92 그룹 저장된 Mutation 복구
> 						93 텍스트 저장된 Mutation 복구
> 						94 텍스트 Mutation journal 준비됨. 해결되지 않은 durable write record가 없습니다. ID: TextPersistedMutationStatus
> 						95 확인란 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다. ID: CheckPersistedMutationPhysicallyVerified
> 							96 텍스트 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.
> 						97 단추 (disabled) 복구된 Mutation 승인 ID: ButtonAcknowledgePersistedMutation
> 							98 텍스트 (disabled) 복구된 Mutation 승인
> 					99 그룹 비동기 SDO Read / Write ticket
> 						100 텍스트 비동기 SDO Read / Write ticket
> 						101 텍스트 작업
> 						102 콤보 상자 ID: ComboSdoOperation
> 						103 텍스트 슬레이브 참조 (1..4)
> 						104 편집 ID: TextSdoSlaveReference
> 						105 텍스트 객체 인덱스 (0x0001..0xFFFF)
> 						106 편집 ID: TextSdoIndex
> 						107 텍스트 서브인덱스 (0..255)
> 						108 편집 ID: TextSdoSubIndex
> 						109 텍스트 값 형식
> 						110 콤보 상자 ID: ComboSdoValueType
> 						111 텍스트 데이터 길이 (1, 2 또는 4)
> 						112 콤보 상자 ID: ComboSdoDataLength
> 						113 텍스트 Timeout (1..60000 cycle)
> 						114 편집 ID: TextSdoTimeoutCycles
> 						115 텍스트 Write 값 (10진수 또는 raw 0x hex)
> 						116 편집 (disabled) ID: TextSdoWriteData
> 						117 텍스트 SDK 승인 SDO Write target
> 						118 콤보 상자 (disabled) ID: ComboSdoWriteTarget
> 						119 단추 SDO Read 전송 Description: Write mode는 현재 session의 same-value 활성화 증명 뒤 2단계 확인을 사용합니다. Read mode는 추적되는 SDO Read 하나를 전송합니다. ID: ButtonSubmitSdo
> 							120 텍스트 SDO Read 전송
> 						121 단추 SDO Inline 읽기 (terminal 대기) Description: 일반 1/2/4-byte SDO Read 하나를 전송하고 terminal 결과를 기다립니다. SDO Write, 필수 Write readback 또는 CREVIS I/O는 실행하지 않습니다. ID: ButtonReadSdoInline
> 							122 텍스트 SDO Inline 읽기 (terminal 대기)
> 						123 단추 (disabled) Inline 대기 취소 (PC만) Description: PC 측 Inline Read 대기만 취소합니다. CancelOperation을 전송하거나 PLC ticket을 취소하거나 SDO request를 재시도하지 않습니다. ID: ButtonCancelSdoInlineWait
> 							124 텍스트 (disabled) Inline 대기 취소 (PC만)
> 						125 단추 (disabled) 필수 정확 Readback 불러오기 Description: 마지막으로 성공한 SDO Write를 확인하는 데 필요한 정확한 read request를 복원합니다. 명령은 전송하지 않습니다. ID: ButtonLoadRequiredSdoReadback
> 							126 텍스트 (disabled) 필수 정확 Readback 불러오기
> 						127 단추 Ticket 새로고침 ID: ButtonRefreshDiagnosticOperation
> 							128 텍스트 Ticket 새로고침
> 						129 단추 (disabled) 결과 다운로드 ID: ButtonDownloadSdoResult
> 							130 텍스트 (disabled) 결과 다운로드
> 						131 단추 결과 저장 ID: ButtonExportSdoResult
> 							132 텍스트 결과 저장
> 						133 단추 (disabled) Ticket 취소 ID: ButtonCancelDiagnosticOperation
> 							134 텍스트 (disabled) Ticket 취소
> 						135 텍스트 TicketId=137, Kind=SDORead, State=Completed, Outcome=Success SubmitCycle=14322828, CompletionCycle=14322849, ErrorId=0, Detail=0x00000000 ResultType=Int8, ResultLength=1, Data=08 ID: TextDiagnosticOperationSummary
> 					136 그룹 D5 SDO fault/recovery qualification (읽기 전용)
> 						137 텍스트 D5 SDO fault/recovery qualification (읽기 전용)
> 						138 텍스트 이 runner는 SDO를 쓰거나 EtherCAT fault를 주입하지 않습니다. Abort qualification은 제조사 승인된 존재하지 않는 읽기 전용 object/sub-index를 사용합니다. Contention, timeout, queued-cancel qualification은 0x6061:0 Int8/1만 사용합니다. Abrupt Disconnect는 RPC Close(0x405D)를 보내지 않고 API TCP transport만 의도적으로 끊은 뒤 재연결하여 0x6061:0을 정확히 두 번 읽습니다. 선택한 Slave 1..4는 _LMCAxis1..4에 매핑되며 모든 runner는 D5 ticket 전송 전에 PowerOn=False, Standstill=True 위치 sample 3회가 안정적으로 일치해야 합니다.
> 						139 텍스트 Abort PASS: 안정적인 BootId/MapRevision -> baseline -> 정확한 SDO abort -> 동일 값 복구. Contention PASS: 첫 0x6061 ticket 승인 -> 첫 terminal 전에 두 번째 Submit이 정확히 Rejected/ResourceBusy -> 첫 ticket Completed/Success -> 정확히 같은 첫 값으로 세 번째 별도 ticket Completed/Success. Timeout PASS: 0x6061 TimeoutCycles=1 -> 정확한 Expired/TimedOut -> 제한된 ResourceBusy drain 대기 -> 별도 동일 값 복구. Queued Cancel PASS: 0x6061 전송 -> 즉시 one-shot CancelOperation -> 정확한 Cancelled/Cancelled -> 별도 동일 값 복구. Abrupt Disconnect는 앱에서 확인 가능한 새 연결 복구만 PASS할 수 있으며 PLC MarkOrphan/late-callback 증거가 생길 때까지 orphanQualified는 false입니다. 연결 손실 전 terminal 완료 또는 Running race는 PLC orphan 증명에 대해 INCONCLUSIVE입니다. 예상 밖 승인 또는 불확실한 결과는 보존되어 자동 재시도를 차단합니다. Cancel Runner는 PLC Stop을 전송하지 않습니다.
> 						140 텍스트 대상 slave (1..4)
> 						141 편집 ID: TextD5SdoAbortSlaveReference
> 						142 텍스트 Abort 객체 인덱스
> 						143 편집 ID: TextD5SdoAbortIndex
> 						144 텍스트 Abort 서브인덱스
> 						145 편집 ID: TextD5SdoAbortSubIndex
> 						146 텍스트 Abort 값 형식
> 						147 콤보 상자 ID: ComboD5SdoAbortValueType
> 						148 텍스트 데이터 길이
> 						149 편집 ID: TextD5SdoAbortDataLength
> 						150 텍스트 일반/복구 timeout cycle
> 						151 편집 ID: TextD5SdoAbortTimeoutCycles
> 						152 단추 D5 Abort -> 복구 실행 ID: ButtonRunD5SdoAbortQualification
> 							153 텍스트 D5 Abort -> 복구 실행
> 						154 단추 D5 Contention -> 복구 실행 ID: ButtonRunD5SdoContentionQualification
> 							155 텍스트 D5 Contention -> 복구 실행
> 						156 단추 D5 Timeout -> 복구 실행 ID: ButtonRunD5SdoTimeoutQualification
> 							157 텍스트 D5 Timeout -> 복구 실행
> 						158 단추 D5 Queued Cancel -> 복구 실행 ID: ButtonRunD5SdoQueuedCancelQualification
> 							159 텍스트 D5 Queued Cancel -> 복구 실행
> 						160 단추 D5 Abrupt Disconnect -> 앱 복구 실행 ID: ButtonRunD5SdoDisconnectRecoveryQualification
> 							161 텍스트 D5 Abrupt Disconnect -> 앱 복구 실행
> 						162 단추 (disabled) Runner 취소 (PLC Stop 아님) ID: ButtonCancelD5SdoQualification
> 							163 텍스트 (disabled) Runner 취소 (PLC Stop 아님)
> 						164 단추 QTEST 로그 저장 ID: ButtonSaveD5SdoQualificationLog
> 							165 텍스트 QTEST 로그 저장
> 						166 진행률 표시줄 ID: ProgressD5SdoQualification
> 						167 텍스트 아직 qualification을 실행하지 않았습니다. ID: TextD5SdoQualificationProgress
> 						168 편집 ID: TextD5SdoQualificationSummary
> 							169 스크롤 막대 ID: HorizontalScrollBar
> 								170 단추 (disabled) ID: PART_LineLeftButton
> 								171 단추 (disabled) ID: PageLeft
> 								172 단추 (disabled) ID: PageRight
> 								173 엄지 (disabled)
> 								174 단추 (disabled) ID: PART_LineRightButton
> 					175 그룹 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						176 텍스트 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						177 텍스트 실제 WRITE: 이 runner는 승인된 target을 읽고 정확히 같은 4 byte를 쓴 뒤 정확한 readback을 요구합니다. Sentinel을 쓰지 않으며 자동 복원이나 재전송을 수행하지 않습니다.
> 						178 텍스트 PLC가 bit 8/9/13을 광고하고, 이 SDK build가 승인 target을 정확히 하나만 노출하고, durable mutation journal을 사용할 수 있고, 작업자 확인 4개가 모두 선택돼야 버튼이 fail-closed 상태에서 풀립니다. 확인 후 선택 축의 PowerOn=False, Standstill=True, 안정 위치를 다시 검사하고 target도 다시 읽어 baseline과 계속 일치해야 Write를 전송합니다.
> 						179 텍스트 SDK 승인 단일 target
> 						180 콤보 상자 ID: ComboD5SdoWriteQualificationTarget
> 						181 텍스트 Timeout (1..60000 cycle)
> 						182 편집 ID: TextD5SdoWriteQualificationTimeoutCycles
> 						183 확인란 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다. ID: CheckConfirmD5SdoWriteUi24Unused
> 							184 텍스트 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다.
> 						185 확인란 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다. ID: CheckConfirmD5SdoWriteOriginalRecorded
> 							186 텍스트 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다.
> 						187 확인란 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다. ID: CheckConfirmD5SdoWriteCaptureRunning
> 							188 텍스트 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다.
> 						189 확인란 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다. ID: CheckConfirmD5SdoWriteSingleWriter
> 							190 텍스트 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다.
> 						191 단추 (disabled) 동일 값 SDO Write Qualification 실행 Description: CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: ButtonRunD5SdoWriteSameValueQualification
> 							192 텍스트 (disabled) 동일 값 SDO Write Qualification 실행
> 						193 텍스트 준비 상태 matrix / 다음 시도 (cache, wire 전송 없음)
> 						194 텍스트 OVERALL CLOSED | EVALUATION_WIRE=NONE SDK POLICY FAIL | blockers=CapabilityObservationNotCurrent | targets=1 PLC CAPS CACHED | bit8/read=1 bit9/write=1 bit13/general=1 IDENTITY PASS | BootId=0x00000042 MapRevision=0x957F101E PAYLOAD PASS | SDO=4 req=1320 resp=2040 cycleUs=1000 QUAL TARGET PASS | selected=Reserved diagnostic UI[24] | Slave 1 | 0x2F00:24 | Int32 | -1073741823..1073741823 RUNNER PASS | connected=True idle=True motionClear=True manualTicketClear=True mutationInterlockClear=True JOURNAL PASS CONFIRMATIONS FAIL | 0/4 AXIS PROOF PENDING_RUNNER_PREFLIGHT | PowerOn=False, Standstill=True, stable position are not cached here. MANUAL WRITE CLOSED_RUN_SAME_VALUE_FIRST | proof never transfers across connection sessions or PLC identity changes. NEXT CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: TextD5SdoWriteQualificationGateStatus
> 						195 텍스트 마지막 시도 / 현재 실행
> 						196 텍스트 아직 동일 값 SDO Write qualification을 실행하지 않았습니다. ID: TextD5SdoWriteQualificationSummary
> 					197 그룹 PI Write 정책 gate
> 						198 텍스트 PI Write 정책 gate
> 						199 텍스트 EtherCAT / PI 탭에서 WritableByPolicy Catalog 항목을 정확히 하나 선택하십시오. Schema v1이 payload를 정의하고 compile-time allowlist가 승인될 때까지 호출은 fail-closed입니다. DS402 ControlWord와 target object는 영구 차단됩니다.
> 						200 텍스트 Raw 값 (10진수 또는 0x...)
> 						201 편집 (disabled) ID: TextPiWriteRawValue
> 						202 단추 (disabled) PI Write 전송 ID: ButtonSubmitPiWrite
> 							203 텍스트 (disabled) PI Write 전송
> 					204 스크롤 막대 ID: VerticalScrollBar
> 						205 단추 (disabled) ID: PART_LineUpButton
> 						206 단추 (disabled) ID: PageUp
> 						207 단추 (disabled) ID: PageDown
> 						208 엄지 (disabled)
> 						209 단추 (disabled) ID: PART_LineDownButton
> 			210 탭 항목 (selectable) 읽기 전용 API
> 				211 텍스트 읽기 전용 API
> 		212 그룹 실행 로그 / 콜백 진단 ID: ExpanderExecutionLog
> 			213 단추 실행 로그 / 콜백 진단 ID: HeaderSite
> 				214 텍스트 실행 로그 / 콜백 진단
> 			215 단추 로그 복사
> 				216 텍스트 로그 복사
> 			217 단추 로그 지우기
> 				218 텍스트 로그 지우기
> 			219 편집 ID: TextExecutionLog
> 				220 스크롤 막대 ID: VerticalScrollBar
> 					221 단추 (disabled) ID: PART_LineUpButton
> 					222 단추 (disabled) ID: PageUp
> 					223 단추 (disabled) ID: PageDown
> 					224 엄지 (disabled)
> 					225 단추 (disabled) ID: PART_LineDownButton
> 				226 스크롤 막대 ID: HorizontalScrollBar
> 					227 단추 (disabled) ID: PART_LineLeftButton
> 					228 단추 (disabled) ID: PageLeft
> 					229 단추 (disabled) ID: PageRight
> 					230 엄지 (disabled)
> 					231 단추 (disabled) ID: PART_LineRightButton
> 		232 상태 표시줄 ID: StatusOperationBar
> 			233 텍스트 Callback D5 status refresh completed ID: TextOperationState
>
> Document text: ```
> [16:39:10.444] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-20 07:38:58 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-20 07:38:55 UTC.
> [16:39:10.445] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [16:45:55.685] Connect started.
> [16:45:55.773] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:45:55.785] Ignored stale connection-state event Connecting from an older transport session.
> [16:45:55.785] Connection state Connecting -> Connected
> [16:45:55.893] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:45:55.904] Connect PASS.
> [16:45:58.785] Close Connection started.
> [16:45:58.789] Connection state Connected -> Closing
> [16:45:58.805] Close Connection PASS.
> [16:46:01.250] Connect started.
> [16:46:01.252] Connection state Disconnected -> Connecting
> [16:46:01.264] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:01.265] Connection state Connecting -> Connected
> [16:46:01.339] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:01.339] Connect PASS.
> [16:46:02.611] Close Connection started.
> [16:46:02.611] Connection state Connected -> Closing
> [16:46:02.621] Close Connection PASS.
> [16:46:06.999] Connect started.
> [16:46:07.001] Connection state Disconnected -> Connecting
> [16:46:07.011] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:07.012] Connection state Connecting -> Connected
> [16:46:07.084] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:07.084] Connect PASS.
> [16:47:58.941] Submit SDO Read started.
> [16:47:58.966] QTEST|utc=2026-08-20T07:47:58.966Z|elapsedMs=0|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [16:47:58.973] QTEST|utc=2026-08-20T07:47:58.973Z|elapsedMs=8|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [16:47:58.981] QTEST|utc=2026-08-20T07:47:58.981Z|elapsedMs=16|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [16:47:59.055] QTEST|utc=2026-08-20T07:47:59.055Z|elapsedMs=91|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=135|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [16:47:59.057] QTEST|utc=2026-08-20T07:47:59.057Z|elapsedMs=93|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|ticket=135|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=135|quarantine=false
> [16:47:59.060] Submit SDO Read PASS.
> [16:47:59.096] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000087
> [16:47:59.117] QTEST|utc=2026-08-20T07:47:59.117Z|elapsedMs=151|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=135|state=Completed|outcome=Success|verdict=CLEARED
> [16:47:59.118] QTEST|utc=2026-08-20T07:47:59.118Z|elapsedMs=152|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [16:47:59.124] Callback D5 authoritative TCP status processed. TicketId=0x00000087
> [17:04:28.599] Submit SDO Read started.
> [17:04:28.610] QTEST|utc=2026-08-20T08:04:28.610Z|elapsedMs=0|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [17:04:28.619] QTEST|utc=2026-08-20T08:04:28.619Z|elapsedMs=9|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [17:04:28.622] QTEST|utc=2026-08-20T08:04:28.622Z|elapsedMs=11|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [17:04:28.631] QTEST|utc=2026-08-20T08:04:28.631Z|elapsedMs=20|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=136|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [17:04:28.633] QTEST|utc=2026-08-20T08:04:28.633Z|elapsedMs=22|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|ticket=136|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=136|quarantine=false
> [17:04:28.659] Submit SDO Read PASS.
> [17:04:28.677] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000088
> [17:04:28.690] QTEST|utc=2026-08-20T08:04:28.690Z|elapsedMs=79|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=136|state=Completed|outcome=Success|verdict=CLEARED
> [17:04:28.692] QTEST|utc=2026-08-20T08:04:28.692Z|elapsedMs=81|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [17:04:28.696] Callback D5 authoritative TCP status processed. TicketId=0x00000088
> [17:04:39.215] Close Connection started.
> [17:04:39.233] Connection state Connected -> Closing
> [17:04:39.253] Close Connection PASS.
> [17:04:52.865] Connect started.
> [17:04:52.877] Connection state Disconnected -> Connecting
> [17:04:52.931] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [17:04:52.990] Connection state Connecting -> Connected
> [17:04:53.069] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [17:04:53.076] Connect PASS.
> [17:05:11.661] Submit SDO Read started.
> [17:05:11.673] QTEST|utc=2026-08-20T08:05:11.673Z|elapsedMs=0|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [17:05:11.681] QTEST|utc=2026-08-20T08:05:11.681Z|elapsedMs=7|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [17:05:11.683] QTEST|utc=2026-08-20T08:05:11.683Z|elapsedMs=9|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=71153dec1118442c891bd7f4d9977378|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [17:05:11.697] QTEST|utc=2026-08-20T08:05:11.697Z|elapsedMs=23|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=137|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [17:05:11.699] QTEST|utc=2026-08-20T08:05:11.699Z|elapsedMs=25|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=71153dec1118442c891bd7f4d9977378|ticket=137|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=137|quarantine=false
> [17:05:11.703] Submit SDO Read PASS.
> [17:05:11.730] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000089
> [17:05:11.745] QTEST|utc=2026-08-20T08:05:11.745Z|elapsedMs=72|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=137|state=Completed|outcome=Success|verdict=CLEARED
> [17:05:11.748] QTEST|utc=2026-08-20T08:05:11.748Z|elapsedMs=74|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [17:05:11.754] Callback D5 authoritative TCP status processed. TicketId=0x00000089
> ```
> ````
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "QTEST 저장 버튼이 보이도록 진단 패널 이동",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.screenshots?.[0]?.id) throw new Error(\"No current screenshot\");\n  const screenshotId = observation.screenshots[0].id;\n  globalThis.state = null;\n  await sky.scroll({\n    window: observation.window,\n    screenshotId,\n    x: 1200,\n    y: 760,\n    scrollX: 0,\n    scrollY: 708,\n  });\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || \"\"));\n}"
> }
> ```
>
> ```text
> 16 편집 (disabled) ID: TextRemoteIp
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "QTEST 저장 버튼까지 패널 이동",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.screenshots?.[0]?.id) throw new Error(\"No current screenshot\");\n  const screenshotId = observation.screenshots[0].id;\n  globalThis.state = null;\n  await sky.scroll({\n    window: observation.window,\n    screenshotId,\n    x: 1200,\n    y: 760,\n    scrollX: 0,\n    scrollY: 735,\n  });\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || \"\"));\n}"
> }
> ```
>
> ```text
> 16 편집 (disabled) ID: TextRemoteIp
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "QTEST 로그 저장 대화상자 열기",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"No accessibility observation\");\n  const tree = String(observation.accessibility.tree || \"\");\n  const match = tree.match(/\\n\\s*(\\d+) 단추 QTEST 로그 저장/);\n  if (!match) throw new Error(\"QTEST save button not found\");\n  const saveIndex = Number(match[1]);\n  globalThis.state = null;\n  await sky.click({ window: observation.window, element_index: saveIndex });\n  await new Promise((resolve) => setTimeout(resolve, 500));\n  globalThis.windowsAfterSave2 = await sky.list_windows();\n  nodeRepl.write(JSON.stringify(windowsAfterSave2, null, 2));\n}"
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\LasalApiWpfTestApp\\LasalApiWpfTestApp\\bin\\Release\\LasalMotionControlApiExample.exe",
>     "id": 3476900,
>     "title": "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]"
>   },
>   {
>     "app": "OpenAI.Codex_2p2nqsd0c76g0!App",
>     "id": 3933112,
>     "title": "ChatGPT"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe",
>     "id": 131448,
>     "title": "카카오톡"
>   },
>   {
>     "app": "VisualStudio.89931261",
>     "id": 198774,
>     "title": "LasalApiWpfTestApp (실행) - Microsoft Visual Studio"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 724834,
>     "title": "Test6 - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 8390442,
>     "title": "Release - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 1117040,
>     "title": "내 PC - 파일 탐색기"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 4458804,
>     "title": "design - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 5115032,
>     "title": "api - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 4197596,
>     "title": "docs - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 1969662,
>     "title": "다운로드 - 파일 탐색기"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 4263250,
>     "title": "Elmo_Master - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "Chrome",
>     "id": 2165522,
>     "title": "CP-313-eng.pdf - Chrome"
>   },
>   {
>     "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\PicPick\\picpick.exe",
>     "id": 1641218,
>     "title": "픽픽 - 이미지 002"
>   },
>   {
>     "app": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Wireshark\\Wireshark.exe",
>     "id": 1182968,
>     "title": "GD-01 재시험.pcapng"
>   },
>   {
>     "app": "Chrome",
>     "id": 2492842,
>     "title": "Daou Office - 홈 - Chrome"
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "저장 버튼 동작 여부 재확인",
>   "code": "{\n  globalThis.state = await sky.get_window_state({\n    window: targetWindow,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || \"\"));\n}"
> }
> ```
>
> ```text
> 389 편집 검색창 Description: Test4 검색 ID: SearchEditBox
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "저장 대화상자 입력 요소 확인",
>   "code": "nodeRepl.write(String(globalThis.state?.accessibility?.tree || \"\"));"
> }
> ```
>
> ````text
> Window: "LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write]", App: LasalMotionControlApiExample.exe.
> 	0 창 LASAL 모션 제어 API 예제 v0.9.1.0 [LIVE Axis qualification / qualified Axis1 UI24 SDO Write] Secondary Actions: Raise
> 		234 대화 상자 Qualification 로그 저장 Secondary Actions: Raise
> 			235 창
> 				236 도구 모음 명령 모듈 ID: FolderBandModuleInner
> 					237 단추 도움말(&H) Description: 도움말을 봅니다. ID: HelpButton
> 					238 분할 단추 자세히 Description: 보기를 변경합니다. ID: ViewControl
> 						239 단추 슬라이더 보기 Description: 기타 옵션 ID: SplitMenuButton
> 					240 단추 구성 Description: 이 폴더의 내용을 구성합니다. ID: {7DDC1264-7E4D-4F74-BBC0-D191987C8D0F}
> 					241 단추 새 폴더 Description: 새 폴더를 만듭니다. ID: {E44616AD-6DF1-4B94-85A4-E465AE8A19DB}
> 				242 창 호스트 제어 ID: ProperTreeHost
> 					243 트리 탐색 창 ID: 100
> 						244 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							245 단추 위쪽 스크롤 화살표 ID: UpButton
> 							246 단추 페이지 위로 ID: UpPageButton
> 							247 위치 조정 위치 ID: ScrollbarThumb
> 							248 단추 페이지 아래로 ID: DownPageButton
> 							249 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						250 콘솔 트리 (selectable) 바탕 화면
> 							251 콘솔 트리 (selectable) 홈
> 							252 콘솔 트리 (selectable) 갤러리
> 							253 콘솔 트리 (selectable) 즐겨찾기 시작 - 바탕 화면(고정됨)
> 							254 콘솔 트리 (selectable) 다운로드(고정됨)
> 							255 콘솔 트리 (selectable) 문서(고정됨)
> 							256 콘솔 트리 (selectable) 사진(고정됨)
> 							257 콘솔 트리 (selectable) 192.168.0.18(고정됨)
> 							258 콘솔 트리 (selectable) Slam(고정됨)
> 							259 콘솔 트리 (selectable) SlamLocCommunicator(고정됨)
> 							260 콘솔 트리 (selectable) SlamLocCommunicatorQt(고정됨)
> 							261 콘솔 트리 (selectable) work(고정됨)
> 							262 콘솔 트리 (selectable) SEMICS(고정됨)
> 							263 콘솔 트리 (selectable) repos(고정됨)
> 							264 콘솔 트리 (selectable) CtrlX(고정됨)
> 							265 콘솔 트리 (selectable) Elmo_Master(고정됨)
> 							266 콘솔 트리 (selectable) WorkReport(고정됨)
> 							267 콘솔 트리 (selectable) GUDEL(고정됨)
> 							268 콘솔 트리 (selectable) Samhyun(고정됨)
> 							269 콘솔 트리 (selectable) JC_Solution(고정됨)
> 							270 콘솔 트리 (selectable) AGV_Driving_Platform(고정됨)
> 							271 콘솔 트리 (selectable) AGV_Driving_Platform_Document(고정됨)
> 							272 콘솔 트리 (selectable) OHT_PLC(고정됨)
> 							273 콘솔 트리 (selectable) KTI_OHT(고정됨)
> 							274 콘솔 트리 (selectable) ASTO(고정됨)
> 							275 콘솔 트리 (selectable) SIGMATEK(고정됨)
> 							276 콘솔 트리 (selectable) project_file(고정됨)
> 							277 콘솔 트리 (selectable) SKI(고정됨)
> 							278 콘솔 트리 (selectable) SEComSimulator(고정됨)
> 							279 콘솔 트리 (selectable) 03.ACS,OCS(고정됨)
> 							280 콘솔 트리 (selectable) 셋업일보(고정됨)
> 							281 콘솔 트리 (selectable) DeviceMap(고정됨)
> 							282 콘솔 트리 (selectable) 현장백업파일(고정됨)
> 							283 콘솔 트리 (selectable) Debug(고정됨)
> 							284 콘솔 트리 (selectable) VirtualAgv(고정됨)
> 							285 콘솔 트리 (selectable) MELSEC MC Protocol Sample Ver 1.5(고정됨)
> 							286 콘솔 트리 (selectable) MCS(고정됨)
> 							287 콘솔 트리 (selectable) git_skba2(고정됨)
> 							288 콘솔 트리 (selectable) git_skoj2(고정됨)
> 							289 콘솔 트리 (selectable) 음악(고정됨)
> 							290 콘솔 트리 (selectable) SigmatakSlam(고정됨)
> 							291 콘솔 트리 (selectable) 동영상(고정됨)
> 							292 콘솔 트리 (selectable) git(고정됨)
> 							293 콘솔 트리 (selectable) Lasal_PRG
> 							294 콘솔 트리 (selectable) ESI_BACKUP
> 							295 콘솔 트리 (selectable) ESI
> 							296 콘솔 트리 (selectable) 즐겨찾기 종료 - Elmo_Master_test
> 							297 콘솔 트리 (selectable) 성환의 S26 Ultra
> 								298 콘솔 트리 (selectable) Saved Searches(ms-resource://MicrosoftWindows.CrossDevice/CrossDevice.Core/Resources/AppName)
> 								299 콘솔 트리 (selectable) 내부 저장소(ms-resource://MicrosoftWindows.CrossDevice/CrossDevice.Core/Resources/AppName)
> 							300 콘솔 트리 (selectable) 내 PC
> 								301 콘솔 트리 (selectable) Windows-SSD (C:)
> 							302 콘솔 트리 (selectable) 네트워크
> 				303 창 셸 폴더 보기 ID: listview
> 					304 LIST 항목 보기
> 						305 스크롤 막대 수평 ID: HorizontalScrollBar
> 							306 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							307 위치 조정 위치 ID: ScrollbarThumb
> 							308 단추 페이지 오른쪽으로 ID: DownPageButton
> 							309 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						310 머리글 머리글
> 							311 분할 단추 이름 ID: System.ItemNameDisplay
> 								312 단추 필터 드롭다운 ID: DropDown
> 							313 분할 단추 수정한 날짜 ID: System.DateModified
> 								314 단추 필터 드롭다운 ID: DropDown
> 							315 분할 단추 유형 ID: System.ItemTypeText
> 								316 단추 필터 드롭다운 ID: DropDown
> 							317 분할 단추 크기 ID: System.Size
> 								318 단추 필터 드롭다운 ID: DropDown
> 						319 목록 항목 (selectable) Axis1_20260731_141635_SingleAxisPowerMoveStopPowerOff.txt ID: 0
> 							320 편집 이름 ID: System.ItemNameDisplay
> 							321 편집 수정한 날짜 ID: System.DateModified
> 							322 편집 유형 ID: System.ItemTypeText
> 							323 편집 크기 ID: System.Size
> 						324 목록 항목 (selectable) Axis1_Test.txt ID: 1
> 							325 편집 이름 ID: System.ItemNameDisplay
> 							326 편집 수정한 날짜 ID: System.DateModified
> 							327 편집 유형 ID: System.ItemTypeText
> 							328 편집 크기 ID: System.Size
> 						329 목록 항목 (selectable) Axis2_20260731_165407_SingleAxisPowerMoveStopPowerOff.txt ID: 2
> 							330 편집 이름 ID: System.ItemNameDisplay
> 							331 편집 수정한 날짜 ID: System.DateModified
> 							332 편집 유형 ID: System.ItemTypeText
> 							333 편집 크기 ID: System.Size
> 						334 목록 항목 (selectable) Axis3_20260731_165505_SingleAxisPowerMoveStopPowerOff.txt ID: 3
> 							335 편집 이름 ID: System.ItemNameDisplay
> 							336 편집 수정한 날짜 ID: System.DateModified
> 							337 편집 유형 ID: System.ItemTypeText
> 							338 편집 크기 ID: System.Size
> 						339 목록 항목 (selectable) Axis4_20260731_165651_SingleAxisPowerMoveStopPowerOff.txt ID: 4
> 							340 편집 이름 ID: System.ItemNameDisplay
> 							341 편집 수정한 날짜 ID: System.DateModified
> 							342 편집 유형 ID: System.ItemTypeText
> 							343 편집 크기 ID: System.Size
> 						344 목록 항목 (selectable) Home Check_Read Legacy Elmo Health.txt ID: 5
> 							345 편집 이름 ID: System.ItemNameDisplay
> 							346 편집 수정한 날짜 ID: System.DateModified
> 							347 편집 유형 ID: System.ItemTypeText
> 							348 편집 크기 ID: System.Size
> 				349 텍스트 파일 이름: Description: 파일 이름: ID: SaveDialogLabel
> 				350 콤보 상자 파일 이름: ID: FileNameControlHost
> 					351 편집 파일 이름: ID: 1001
> 					352 단추 열기 ID: DropDown
> 				353 텍스트 파일 형식: Description: 파일 형식: ID: SaveDialogLabel
> 				354 콤보 상자 파일 형식: ID: FileTypeControlHost
> 					355 텍스트
> 					356 단추 열기 ID: DropDown
> 				357 트리 필드 저장 ID: SaveDialogPreviewMetadataInner
> 				358 트리 응용 프로그램 컨트롤 ID: AppControlsModuleInner
> 			359 도구 모음
> 				360 단추 폴더 숨기기
> 			361 단추 저장(S) ID: 1
> 			362 단추 취소 ID: 2
> 			363 위치 조정
> 			364 창 ID: 40965
> 				365 창
> 					366 도구 모음 탐색 단추
> 						367 단추 (disabled) 뒤로(Alt+왼쪽 화살표)
> 						368 단추 (disabled) 앞으로(Alt+오른쪽 화살표)
> 						369 단추 (disabled) 최근 위치
> 				370 창
> 					371 도구 모음 업밴드
> 						372 단추 "SIGMATEK_API_Analyze"(으)로 이동(Alt+위쪽 화살표)
> 				373 창 ID: 41477
> 					374 진행률 표시줄 로드 중
> 						375 창
> 							376 도구 모음 주소: C:\work\Elmo\Elmo_Master\test\packet_capture\SIGMATEK_API_Analyze\Test4 ID: 1001
> 								377 분할 단추 모든 위치
> 								378 분할 단추 ‎Elmo_Master
> 								379 분할 단추 ‎test
> 								380 분할 단추 ‎packet_capture
> 								381 분할 단추 ‎SIGMATEK_API_Analyze
> 								382 메뉴 항목 ‎Test4
> 						383 도구 모음 주소 대역
> 							384 단추 이전 위치
> 							385 단추 "Test4" 새로 고침(F5)
> 				386 창
> 					387 창
> 						388 창
> 							389 편집 검색창 Description: Test4 검색 ID: SearchEditBox
> 							390 단추 검색 ID: SearchBoxSearchButton
> 			391 제목 표시줄
> 				392 단추 닫기
> 		1 창 Secondary Actions: Raise
> 			2 단추 In App Menu Expander ID: InAppMenuExpander
> 		3 제목 표시줄
> 			4 메뉴 모음 시스템 ID: MenuBar
> 				5 메뉴 항목 시스템
> 			6 단추 최소화
> 			7 단추 복원
> 			8 단추 닫기
> 		9 텍스트 언어
> 		10 콤보 상자 ID: ComboUiLanguage
> 		11 텍스트 LASAL 모션 제어 API 예제
> 		12 텍스트 모션 제어는 PLC에서 활성 상태입니다. 진단 제어는 PLC가 필요한 기능을 광고한 뒤에만 활성화됩니다. 값은 선택한 application UNIT을 사용하거나 raw DINT로 전달됩니다.
> 		13 그룹 연결 / RPC 콜백
> 			14 텍스트 연결 / RPC 콜백
> 			15 텍스트 PLC IP
> 			16 편집 (disabled) ID: TextRemoteIp
> 			17 텍스트 TCP 포트
> 			18 편집 (disabled) ID: TextRemotePort
> 			19 텍스트 PC 로컬 IPv4
> 			20 편집 (disabled) ID: TextLocalIp
> 			21 텍스트 콜백 UDP 포트
> 			22 편집 (disabled) ID: TextCallbackPort
> 			23 단추 연결 ID: ButtonConnect
> 				24 텍스트 연결
> 			25 단추 닫기 ID: ButtonCloseConnection
> 				26 텍스트 닫기
> 			27 텍스트 연결 상태
> 			28 텍스트 연결됨 ID: TextConnectionState
> 			29 텍스트 콜백 수신기
> 			30 텍스트 수신 대기 10.10.150.13:5000, rejected=0 ID: TextCallbackState
> 			31 그룹 RPC 초기화 / 콜백 증거 ID: ExpanderRpcCallbackEvidence
> 				32 단추 RPC 초기화 / 콜백 증거 ID: HeaderSite
> 					33 텍스트 RPC 초기화 / 콜백 증거
> 				34 텍스트 RPC 초기화 증거
> 				35 텍스트 Attempt=4, Outcome=Connected, CandidateOrdinal=1, Remote=10.10.150.1:4000, Local=10.10.150.13, RequestedCallback=10.10.150.13:5000, Mode=Version2WakeHint, BoundCallback=10.10.150.13:5000, LocalSessionGeneration=1, 0x8080Attempts=1, Retry=False, InitOutcome=Succeeded, StartedUtc=2026-08-20T08:04:52.8760280Z, CompletedUtc=2026-08-20T08:04:52.8810342Z, LastACK={FrameValid=True, HeaderStatus=0, HeaderReserved=0, PayloadLength=24, HasCommandResult=False, CommandStatus=0, ErrorId=0}, Current=Active ID: TextRpcInitialization
> 				36 텍스트 콜백 v2 등록
> 				37 텍스트 Status=0, ErrorId=0, Version=2, MaxDatagram=52, BootId=0x00000042, SessionEpoch=30, Flags=0x00000000, Cookie=0x2A978FF0DA1E78F3, ListenerGeneration=2, Source=10.10.150.1, EventMask=0x00000001, LocalSessionGeneration=1 ID: TextCallbackRegistration
> 				38 텍스트 PC 콜백 수신 증거
> 				39 텍스트 Accepted=1, Rejected=0, Duplicate=0, OutOfOrder=0 ID: TextCallbackCounters
> 				40 텍스트 Last decision=AcceptedWakeHint, ProtocolError=None ID: TextCallbackLastDecision
> 				41 텍스트 연결 시 RPC 세션 초기화와 콜백 등록을 자동으로 수행합니다. UDP wake hint는 상태 판단 근거가 아니며, 동작 상태는 일치하는 TCP status 응답을 받은 뒤에만 변경됩니다.
> 		42 그룹 안전 / 복구 상세 정보 ID: ExpanderSafetyAndRecoveryDetails
> 			43 단추 안전 / 복구 상세 정보 ID: HeaderSite
> 				44 텍스트 안전 / 복구 상세 정보
> 		45 탭 ID: TabsMotion
> 			46 탭 항목 (selectable) 단축
> 				47 텍스트 단축
> 			48 탭 항목 (selectable) 그룹 모션
> 				49 텍스트 그룹 모션
> 			50 탭 항목 (selectable) EtherCAT / CREVIS / PI
> 				51 텍스트 EtherCAT / CREVIS / PI
> 			52 탭 항목 (selectable) Bulk 스냅샷
> 				53 텍스트 Bulk 스냅샷
> 			54 탭 항목 (selectable) 레코더
> 				55 텍스트 레코더
> 			56 탭 항목 (selectable) SDO / Write 정책
> 				57 텍스트 SDO / Write 정책
> 				58 창 ID: ScrollDiagnosticsOperations
> 					59 그룹 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19]) ID: GroupEncoderMaintenance
> 						60 텍스트 테스트 전용 - Encoder 유지보수 (TW[20] / TW[19])
> 						61 텍스트 파괴적 테스트 전용: TW[20]은 0x20FC:0x02에 UInt16 1을 씁니다. TW[19]는 0x20FC:0x01에 UInt16 1을 써 absolute multi-turn position을 reset하며 motion 전에 LMC Home이 필요합니다. 정확한 drive 축을 선택하십시오.
> 						62 텍스트 Fixed writes: TW[20]=0x20FC:0x02 <- UInt16 1; TW[19]=0x20FC:0x01 <- UInt16 1 ID: TextEncoderMaintenanceFixedTargets
> 						63 텍스트 Arm 전: 선택한 축의 Power를 끄고 안정적인 standstill과 실제 위치를 독립적으로 확인한 뒤 정확한 0x20FC 명령 지원 여부를 확인하십시오. 전용 0x7E53/0x7E54/0x7E55 경로는 일반 SDO Write와 분리됩니다.
> 						64 텍스트 작업
> 						65 콤보 상자 ID: ComboEncoderMaintenanceKind
> 						66 텍스트 Drive / 축 (1..4)
> 						67 콤보 상자 ID: ComboTestResetAxis
> 						68 텍스트 Timeout (ms, 1..60000)
> 						69 편집 ID: TextTestResetTimeout
> 						70 단추 Encoder 유지보수 Capability 새로고침 ID: ButtonRefreshTestResetCapabilities
> 							71 텍스트 Encoder 유지보수 Capability 새로고침
> 						72 단추 (disabled) Encoder 유지보수 결과 읽기 ID: ButtonReadTestResetStatus
> 							73 텍스트 (disabled) Encoder 유지보수 결과 읽기
> 						74 확인란 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다. ID: CheckTestResetPowerOffVerified
> 							75 텍스트 1단계 A: 선택한 실제 축의 PowerOn=False와 안정적인 standstill을 확인했습니다.
> 						76 확인란 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다. ID: CheckTestResetPhysicalPositionVerified
> 							77 텍스트 1단계 B: 실제 위치를 기록했으며 motion 전에 필요한 Home 절차를 실행합니다.
> 						78 확인란 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다. ID: CheckTestResetExactTargetVerified
> 							79 텍스트 1단계 C: 정확한 drive 축과 선택한 TW[20] 또는 TW[19]의 파괴적 효과를 확인했습니다.
> 						80 확인란 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다. ID: CheckEncoderMaintenanceCompatibilityVerified
> 							81 텍스트 1단계 D: 선택한 drive가 이 정확한 0x20FC 유지보수 명령을 지원함을 독립적으로 확인했습니다.
> 						82 텍스트 차단: 1단계의 모든 물리 확인과 encoder 호환성 확인이 필요합니다. Encoder 유지보수 작업을 arm하거나 전송하지 않았습니다. ID: TextEncoderMaintenanceArmGateStatus
> 						83 단추 안전 / 복구 상세 정보 열기 Description: 로컬 안전/복구 panel만 펼칩니다. 이 버튼은 PLC 명령을 전송하지 않으며 어떤 record도 폐기하지 않습니다. ID: ButtonOpenEncoderRecoveryDetails
> 							84 텍스트 안전 / 복구 상세 정보 열기
> 						85 단추 (disabled) 1단계 - Encoder 유지보수 Arm Description: Live mutation 승인, 현재 TW[20]/TW[19] capability, 사용 가능한 durable 유지보수 journal 및 1단계의 네 가지 확인이 필요합니다. Arm은 정확한 요청을 PC 메모리에만 준비하며 0x7E53을 전송하지 않습니다. ID: ButtonArmTestReset
> 							86 텍스트 (disabled) 1단계 - Encoder 유지보수 Arm
> 						87 확인란 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다. ID: CheckTestResetFinalConfirmed
> 							88 텍스트 (disabled) 2단계: 변경되지 않은 arm 요청을 한 번만 실행합니다.
> 						89 단추 (disabled) 2단계 - Encoder 유지보수 1회 실행 Description: 변경되지 않은 PC 전용 arm 요청, live mutation 승인, 현재 capability, 사용 가능한 durable 유지보수 journal 및 2단계 확인이 필요합니다. Execute는 0x7E53을 한 번 전송합니다. ID: ButtonExecuteTestReset
> 							90 텍스트 (disabled) 2단계 - Encoder 유지보수 1회 실행
> 						91 텍스트 Arm되거나 대기 중인 encoder 유지보수 작업이 없습니다. ID: TextTestResetResult
> 					92 그룹 저장된 Mutation 복구
> 						93 텍스트 저장된 Mutation 복구
> 						94 텍스트 Mutation journal 준비됨. 해결되지 않은 durable write record가 없습니다. ID: TextPersistedMutationStatus
> 						95 확인란 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다. ID: CheckPersistedMutationPhysicallyVerified
> 							96 텍스트 (disabled) 물리 target과 PLC 상태를 독립적으로 확인했습니다. 이전 명령을 재전송하지 않습니다.
> 						97 단추 (disabled) 복구된 Mutation 승인 ID: ButtonAcknowledgePersistedMutation
> 							98 텍스트 (disabled) 복구된 Mutation 승인
> 					99 그룹 비동기 SDO Read / Write ticket
> 						100 텍스트 비동기 SDO Read / Write ticket
> 						101 텍스트 작업
> 						102 콤보 상자 ID: ComboSdoOperation
> 						103 텍스트 슬레이브 참조 (1..4)
> 						104 편집 ID: TextSdoSlaveReference
> 						105 텍스트 객체 인덱스 (0x0001..0xFFFF)
> 						106 편집 ID: TextSdoIndex
> 						107 텍스트 서브인덱스 (0..255)
> 						108 편집 ID: TextSdoSubIndex
> 						109 텍스트 값 형식
> 						110 콤보 상자 ID: ComboSdoValueType
> 						111 텍스트 데이터 길이 (1, 2 또는 4)
> 						112 콤보 상자 ID: ComboSdoDataLength
> 						113 텍스트 Timeout (1..60000 cycle)
> 						114 편집 ID: TextSdoTimeoutCycles
> 						115 텍스트 Write 값 (10진수 또는 raw 0x hex)
> 						116 편집 (disabled) ID: TextSdoWriteData
> 						117 텍스트 SDK 승인 SDO Write target
> 						118 콤보 상자 (disabled) ID: ComboSdoWriteTarget
> 						119 단추 SDO Read 전송 Description: Write mode는 현재 session의 same-value 활성화 증명 뒤 2단계 확인을 사용합니다. Read mode는 추적되는 SDO Read 하나를 전송합니다. ID: ButtonSubmitSdo
> 							120 텍스트 SDO Read 전송
> 						121 단추 SDO Inline 읽기 (terminal 대기) Description: 일반 1/2/4-byte SDO Read 하나를 전송하고 terminal 결과를 기다립니다. SDO Write, 필수 Write readback 또는 CREVIS I/O는 실행하지 않습니다. ID: ButtonReadSdoInline
> 							122 텍스트 SDO Inline 읽기 (terminal 대기)
> 						123 단추 (disabled) Inline 대기 취소 (PC만) Description: PC 측 Inline Read 대기만 취소합니다. CancelOperation을 전송하거나 PLC ticket을 취소하거나 SDO request를 재시도하지 않습니다. ID: ButtonCancelSdoInlineWait
> 							124 텍스트 (disabled) Inline 대기 취소 (PC만)
> 						125 단추 (disabled) 필수 정확 Readback 불러오기 Description: 마지막으로 성공한 SDO Write를 확인하는 데 필요한 정확한 read request를 복원합니다. 명령은 전송하지 않습니다. ID: ButtonLoadRequiredSdoReadback
> 							126 텍스트 (disabled) 필수 정확 Readback 불러오기
> 						127 단추 Ticket 새로고침 ID: ButtonRefreshDiagnosticOperation
> 							128 텍스트 Ticket 새로고침
> 						129 단추 (disabled) 결과 다운로드 ID: ButtonDownloadSdoResult
> 							130 텍스트 (disabled) 결과 다운로드
> 						131 단추 결과 저장 ID: ButtonExportSdoResult
> 							132 텍스트 결과 저장
> 						133 단추 (disabled) Ticket 취소 ID: ButtonCancelDiagnosticOperation
> 							134 텍스트 (disabled) Ticket 취소
> 						135 텍스트 TicketId=137, Kind=SDORead, State=Completed, Outcome=Success SubmitCycle=14322828, CompletionCycle=14322849, ErrorId=0, Detail=0x00000000 ResultType=Int8, ResultLength=1, Data=08 ID: TextDiagnosticOperationSummary
> 					136 그룹 D5 SDO fault/recovery qualification (읽기 전용)
> 						137 텍스트 D5 SDO fault/recovery qualification (읽기 전용)
> 						138 텍스트 이 runner는 SDO를 쓰거나 EtherCAT fault를 주입하지 않습니다. Abort qualification은 제조사 승인된 존재하지 않는 읽기 전용 object/sub-index를 사용합니다. Contention, timeout, queued-cancel qualification은 0x6061:0 Int8/1만 사용합니다. Abrupt Disconnect는 RPC Close(0x405D)를 보내지 않고 API TCP transport만 의도적으로 끊은 뒤 재연결하여 0x6061:0을 정확히 두 번 읽습니다. 선택한 Slave 1..4는 _LMCAxis1..4에 매핑되며 모든 runner는 D5 ticket 전송 전에 PowerOn=False, Standstill=True 위치 sample 3회가 안정적으로 일치해야 합니다.
> 						139 텍스트 Abort PASS: 안정적인 BootId/MapRevision -> baseline -> 정확한 SDO abort -> 동일 값 복구. Contention PASS: 첫 0x6061 ticket 승인 -> 첫 terminal 전에 두 번째 Submit이 정확히 Rejected/ResourceBusy -> 첫 ticket Completed/Success -> 정확히 같은 첫 값으로 세 번째 별도 ticket Completed/Success. Timeout PASS: 0x6061 TimeoutCycles=1 -> 정확한 Expired/TimedOut -> 제한된 ResourceBusy drain 대기 -> 별도 동일 값 복구. Queued Cancel PASS: 0x6061 전송 -> 즉시 one-shot CancelOperation -> 정확한 Cancelled/Cancelled -> 별도 동일 값 복구. Abrupt Disconnect는 앱에서 확인 가능한 새 연결 복구만 PASS할 수 있으며 PLC MarkOrphan/late-callback 증거가 생길 때까지 orphanQualified는 false입니다. 연결 손실 전 terminal 완료 또는 Running race는 PLC orphan 증명에 대해 INCONCLUSIVE입니다. 예상 밖 승인 또는 불확실한 결과는 보존되어 자동 재시도를 차단합니다. Cancel Runner는 PLC Stop을 전송하지 않습니다.
> 						140 텍스트 대상 slave (1..4)
> 						141 편집 ID: TextD5SdoAbortSlaveReference
> 						142 텍스트 Abort 객체 인덱스
> 						143 편집 ID: TextD5SdoAbortIndex
> 						144 텍스트 Abort 서브인덱스
> 						145 편집 ID: TextD5SdoAbortSubIndex
> 						146 텍스트 Abort 값 형식
> 						147 콤보 상자 ID: ComboD5SdoAbortValueType
> 						148 텍스트 데이터 길이
> 						149 편집 ID: TextD5SdoAbortDataLength
> 						150 텍스트 일반/복구 timeout cycle
> 						151 편집 ID: TextD5SdoAbortTimeoutCycles
> 						152 단추 D5 Abort -> 복구 실행 ID: ButtonRunD5SdoAbortQualification
> 							153 텍스트 D5 Abort -> 복구 실행
> 						154 단추 D5 Contention -> 복구 실행 ID: ButtonRunD5SdoContentionQualification
> 							155 텍스트 D5 Contention -> 복구 실행
> 						156 단추 D5 Timeout -> 복구 실행 ID: ButtonRunD5SdoTimeoutQualification
> 							157 텍스트 D5 Timeout -> 복구 실행
> 						158 단추 D5 Queued Cancel -> 복구 실행 ID: ButtonRunD5SdoQueuedCancelQualification
> 							159 텍스트 D5 Queued Cancel -> 복구 실행
> 						160 단추 D5 Abrupt Disconnect -> 앱 복구 실행 ID: ButtonRunD5SdoDisconnectRecoveryQualification
> 							161 텍스트 D5 Abrupt Disconnect -> 앱 복구 실행
> 						162 단추 (disabled) Runner 취소 (PLC Stop 아님) ID: ButtonCancelD5SdoQualification
> 							163 텍스트 (disabled) Runner 취소 (PLC Stop 아님)
> 						164 단추 QTEST 로그 저장 ID: ButtonSaveD5SdoQualificationLog
> 							165 텍스트 QTEST 로그 저장
> 						166 진행률 표시줄 ID: ProgressD5SdoQualification
> 						167 텍스트 아직 qualification을 실행하지 않았습니다. ID: TextD5SdoQualificationProgress
> 						168 편집 ID: TextD5SdoQualificationSummary
> 							169 스크롤 막대 ID: HorizontalScrollBar
> 								170 단추 (disabled) ID: PART_LineLeftButton
> 								171 단추 (disabled) ID: PageLeft
> 								172 단추 (disabled) ID: PageRight
> 								173 엄지 (disabled)
> 								174 단추 (disabled) ID: PART_LineRightButton
> 					175 그룹 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						176 텍스트 D5 SDO Write 활성화 qualification (동일 값 전용)
> 						177 텍스트 실제 WRITE: 이 runner는 승인된 target을 읽고 정확히 같은 4 byte를 쓴 뒤 정확한 readback을 요구합니다. Sentinel을 쓰지 않으며 자동 복원이나 재전송을 수행하지 않습니다.
> 						178 텍스트 PLC가 bit 8/9/13을 광고하고, 이 SDK build가 승인 target을 정확히 하나만 노출하고, durable mutation journal을 사용할 수 있고, 작업자 확인 4개가 모두 선택돼야 버튼이 fail-closed 상태에서 풀립니다. 확인 후 선택 축의 PowerOn=False, Standstill=True, 안정 위치를 다시 검사하고 target도 다시 읽어 baseline과 계속 일치해야 Write를 전송합니다.
> 						179 텍스트 SDK 승인 단일 target
> 						180 콤보 상자 ID: ComboD5SdoWriteQualificationTarget
> 						181 텍스트 Timeout (1..60000 cycle)
> 						182 편집 ID: TextD5SdoWriteQualificationTimeoutCycles
> 						183 확인란 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다. ID: CheckConfirmD5SdoWriteUi24Unused
> 							184 텍스트 선택한 drive program이 UI[24]를 다른 용도로 사용하지 않음을 확인했습니다.
> 						185 확인란 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다. ID: CheckConfirmD5SdoWriteOriginalRecorded
> 							186 텍스트 baseline/original 값을 기록하고 실행이 중단되면 물리 target을 독립적으로 확인하겠습니다.
> 						187 확인란 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다. ID: CheckConfirmD5SdoWriteCaptureRunning
> 							188 텍스트 이 활성화 시험을 위해 PLC mailbox 증거와 packet capture가 실행 중입니다.
> 						189 확인란 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다. ID: CheckConfirmD5SdoWriteSingleWriter
> 							190 텍스트 single-writer 구간을 확보했습니다. 정확한 readback이 끝날 때까지 drive logic, PLC task, GUI, 다른 client가 이 target을 변경하지 않습니다.
> 						191 단추 (disabled) 동일 값 SDO Write Qualification 실행 Description: CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: ButtonRunD5SdoWriteSameValueQualification
> 							192 텍스트 (disabled) 동일 값 SDO Write Qualification 실행
> 						193 텍스트 준비 상태 matrix / 다음 시도 (cache, wire 전송 없음)
> 						194 텍스트 OVERALL CLOSED | EVALUATION_WIRE=NONE SDK POLICY FAIL | blockers=CapabilityObservationNotCurrent | targets=1 PLC CAPS CACHED | bit8/read=1 bit9/write=1 bit13/general=1 IDENTITY PASS | BootId=0x00000042 MapRevision=0x957F101E PAYLOAD PASS | SDO=4 req=1320 resp=2040 cycleUs=1000 QUAL TARGET PASS | selected=Reserved diagnostic UI[24] | Slave 1 | 0x2F00:24 | Int32 | -1073741823..1073741823 RUNNER PASS | connected=True idle=True motionClear=True manualTicketClear=True mutationInterlockClear=True JOURNAL PASS CONFIRMATIONS FAIL | 0/4 AXIS PROOF PENDING_RUNNER_PREFLIGHT | PowerOn=False, Standstill=True, stable position are not cached here. MANUAL WRITE CLOSED_RUN_SAME_VALUE_FIRST | proof never transfers across connection sessions or PLC identity changes. NEXT CLOSED: cached SDK/PLC policy blockers=CapabilityObservationNotCurrent. Refresh diagnostics capabilities only if the cache is missing or stale. ID: TextD5SdoWriteQualificationGateStatus
> 						195 텍스트 마지막 시도 / 현재 실행
> 						196 텍스트 아직 동일 값 SDO Write qualification을 실행하지 않았습니다. ID: TextD5SdoWriteQualificationSummary
> 					197 그룹 PI Write 정책 gate
> 						198 텍스트 PI Write 정책 gate
> 						199 텍스트 EtherCAT / PI 탭에서 WritableByPolicy Catalog 항목을 정확히 하나 선택하십시오. Schema v1이 payload를 정의하고 compile-time allowlist가 승인될 때까지 호출은 fail-closed입니다. DS402 ControlWord와 target object는 영구 차단됩니다.
> 						200 텍스트 Raw 값 (10진수 또는 0x...)
> 						201 편집 (disabled) ID: TextPiWriteRawValue
> 						202 단추 (disabled) PI Write 전송 ID: ButtonSubmitPiWrite
> 							203 텍스트 (disabled) PI Write 전송
> 					204 스크롤 막대 ID: VerticalScrollBar
> 						205 단추 (disabled) ID: PART_LineUpButton
> 						206 단추 (disabled) ID: PageUp
> 						207 단추 (disabled) ID: PageDown
> 						208 엄지 (disabled)
> 						209 단추 (disabled) ID: PART_LineDownButton
> 			210 탭 항목 (selectable) 읽기 전용 API
> 				211 텍스트 읽기 전용 API
> 		212 그룹 실행 로그 / 콜백 진단 ID: ExpanderExecutionLog
> 			213 단추 실행 로그 / 콜백 진단 ID: HeaderSite
> 				214 텍스트 실행 로그 / 콜백 진단
> 			215 단추 로그 복사
> 				216 텍스트 로그 복사
> 			217 단추 로그 지우기
> 				218 텍스트 로그 지우기
> 			219 편집 ID: TextExecutionLog
> 				220 스크롤 막대 ID: VerticalScrollBar
> 					221 단추 (disabled) ID: PART_LineUpButton
> 					222 단추 (disabled) ID: PageUp
> 					223 단추 (disabled) ID: PageDown
> 					224 엄지 (disabled)
> 					225 단추 (disabled) ID: PART_LineDownButton
> 				226 스크롤 막대 ID: HorizontalScrollBar
> 					227 단추 (disabled) ID: PART_LineLeftButton
> 					228 단추 (disabled) ID: PageLeft
> 					229 단추 (disabled) ID: PageRight
> 					230 엄지 (disabled)
> 					231 단추 (disabled) ID: PART_LineRightButton
> 		232 상태 표시줄 ID: StatusOperationBar
> 			233 텍스트 Callback D5 status refresh completed ID: TextOperationState
> 		234 대화 상자 Qualification 로그 저장 Secondary Actions: Raise
> 			235 창
> 				236 도구 모음 명령 모듈 ID: FolderBandModuleInner
> 					237 단추 도움말(&H) Description: 도움말을 봅니다. ID: HelpButton
> 					238 분할 단추 자세히 Description: 보기를 변경합니다. ID: ViewControl
> 						239 단추 슬라이더 보기 Description: 기타 옵션 ID: SplitMenuButton
> 					240 단추 구성 Description: 이 폴더의 내용을 구성합니다. ID: {7DDC1264-7E4D-4F74-BBC0-D191987C8D0F}
> 					241 단추 새 폴더 Description: 새 폴더를 만듭니다. ID: {E44616AD-6DF1-4B94-85A4-E465AE8A19DB}
> 				242 창 호스트 제어 ID: ProperTreeHost
> 					243 트리 탐색 창 ID: 100
> 						244 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							245 단추 위쪽 스크롤 화살표 ID: UpButton
> 							246 단추 페이지 위로 ID: UpPageButton
> 							247 위치 조정 위치 ID: ScrollbarThumb
> 							248 단추 페이지 아래로 ID: DownPageButton
> 							249 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						250 콘솔 트리 (selectable) 바탕 화면
> 							251 콘솔 트리 (selectable) 홈
> 							252 콘솔 트리 (selectable) 갤러리
> 							253 콘솔 트리 (selectable) 즐겨찾기 시작 - 바탕 화면(고정됨)
> 							254 콘솔 트리 (selectable) 다운로드(고정됨)
> 							255 콘솔 트리 (selectable) 문서(고정됨)
> 							256 콘솔 트리 (selectable) 사진(고정됨)
> 							257 콘솔 트리 (selectable) 192.168.0.18(고정됨)
> 							258 콘솔 트리 (selectable) Slam(고정됨)
> 							259 콘솔 트리 (selectable) SlamLocCommunicator(고정됨)
> 							260 콘솔 트리 (selectable) SlamLocCommunicatorQt(고정됨)
> 							261 콘솔 트리 (selectable) work(고정됨)
> 							262 콘솔 트리 (selectable) SEMICS(고정됨)
> 							263 콘솔 트리 (selectable) repos(고정됨)
> 							264 콘솔 트리 (selectable) CtrlX(고정됨)
> 							265 콘솔 트리 (selectable) Elmo_Master(고정됨)
> 							266 콘솔 트리 (selectable) WorkReport(고정됨)
> 							267 콘솔 트리 (selectable) GUDEL(고정됨)
> 							268 콘솔 트리 (selectable) Samhyun(고정됨)
> 							269 콘솔 트리 (selectable) JC_Solution(고정됨)
> 							270 콘솔 트리 (selectable) AGV_Driving_Platform(고정됨)
> 							271 콘솔 트리 (selectable) AGV_Driving_Platform_Document(고정됨)
> 							272 콘솔 트리 (selectable) OHT_PLC(고정됨)
> 							273 콘솔 트리 (selectable) KTI_OHT(고정됨)
> 							274 콘솔 트리 (selectable) ASTO(고정됨)
> 							275 콘솔 트리 (selectable) SIGMATEK(고정됨)
> 							276 콘솔 트리 (selectable) project_file(고정됨)
> 							277 콘솔 트리 (selectable) SKI(고정됨)
> 							278 콘솔 트리 (selectable) SEComSimulator(고정됨)
> 							279 콘솔 트리 (selectable) 03.ACS,OCS(고정됨)
> 							280 콘솔 트리 (selectable) 셋업일보(고정됨)
> 							281 콘솔 트리 (selectable) DeviceMap(고정됨)
> 							282 콘솔 트리 (selectable) 현장백업파일(고정됨)
> 							283 콘솔 트리 (selectable) Debug(고정됨)
> 							284 콘솔 트리 (selectable) VirtualAgv(고정됨)
> 							285 콘솔 트리 (selectable) MELSEC MC Protocol Sample Ver 1.5(고정됨)
> 							286 콘솔 트리 (selectable) MCS(고정됨)
> 							287 콘솔 트리 (selectable) git_skba2(고정됨)
> 							288 콘솔 트리 (selectable) git_skoj2(고정됨)
> 							289 콘솔 트리 (selectable) 음악(고정됨)
> 							290 콘솔 트리 (selectable) SigmatakSlam(고정됨)
> 							291 콘솔 트리 (selectable) 동영상(고정됨)
> 							292 콘솔 트리 (selectable) git(고정됨)
> 							293 콘솔 트리 (selectable) Lasal_PRG
> 							294 콘솔 트리 (selectable) ESI_BACKUP
> 							295 콘솔 트리 (selectable) ESI
> 							296 콘솔 트리 (selectable) 즐겨찾기 종료 - Elmo_Master_test
> 							297 콘솔 트리 (selectable) 성환의 S26 Ultra
> 								298 콘솔 트리 (selectable) Saved Searches(ms-resource://MicrosoftWindows.CrossDevice/CrossDevice.Core/Resources/AppName)
> 								299 콘솔 트리 (selectable) 내부 저장소(ms-resource://MicrosoftWindows.CrossDevice/CrossDevice.Core/Resources/AppName)
> 							300 콘솔 트리 (selectable) 내 PC
> 								301 콘솔 트리 (selectable) Windows-SSD (C:)
> 							302 콘솔 트리 (selectable) 네트워크
> 				303 창 셸 폴더 보기 ID: listview
> 					304 LIST 항목 보기
> 						305 스크롤 막대 수평 ID: HorizontalScrollBar
> 							306 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							307 위치 조정 위치 ID: ScrollbarThumb
> 							308 단추 페이지 오른쪽으로 ID: DownPageButton
> 							309 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						310 머리글 머리글
> 							311 분할 단추 이름 ID: System.ItemNameDisplay
> 								312 단추 필터 드롭다운 ID: DropDown
> 							313 분할 단추 수정한 날짜 ID: System.DateModified
> 								314 단추 필터 드롭다운 ID: DropDown
> 							315 분할 단추 유형 ID: System.ItemTypeText
> 								316 단추 필터 드롭다운 ID: DropDown
> 							317 분할 단추 크기 ID: System.Size
> 								318 단추 필터 드롭다운 ID: DropDown
> 						319 목록 항목 (selectable) Axis1_20260731_141635_SingleAxisPowerMoveStopPowerOff.txt ID: 0
> 							320 편집 이름 ID: System.ItemNameDisplay
> 							321 편집 수정한 날짜 ID: System.DateModified
> 							322 편집 유형 ID: System.ItemTypeText
> 							323 편집 크기 ID: System.Size
> 						324 목록 항목 (selectable) Axis1_Test.txt ID: 1
> 							325 편집 이름 ID: System.ItemNameDisplay
> 							326 편집 수정한 날짜 ID: System.DateModified
> 							327 편집 유형 ID: System.ItemTypeText
> 							328 편집 크기 ID: System.Size
> 						329 목록 항목 (selectable) Axis2_20260731_165407_SingleAxisPowerMoveStopPowerOff.txt ID: 2
> 							330 편집 이름 ID: System.ItemNameDisplay
> 							331 편집 수정한 날짜 ID: System.DateModified
> 							332 편집 유형 ID: System.ItemTypeText
> 							333 편집 크기 ID: System.Size
> 						334 목록 항목 (selectable) Axis3_20260731_165505_SingleAxisPowerMoveStopPowerOff.txt ID: 3
> 							335 편집 이름 ID: System.ItemNameDisplay
> 							336 편집 수정한 날짜 ID: System.DateModified
> 							337 편집 유형 ID: System.ItemTypeText
> 							338 편집 크기 ID: System.Size
> 						339 목록 항목 (selectable) Axis4_20260731_165651_SingleAxisPowerMoveStopPowerOff.txt ID: 4
> 							340 편집 이름 ID: System.ItemNameDisplay
> 							341 편집 수정한 날짜 ID: System.DateModified
> 							342 편집 유형 ID: System.ItemTypeText
> 							343 편집 크기 ID: System.Size
> 						344 목록 항목 (selectable) Home Check_Read Legacy Elmo Health.txt ID: 5
> 							345 편집 이름 ID: System.ItemNameDisplay
> 							346 편집 수정한 날짜 ID: System.DateModified
> 							347 편집 유형 ID: System.ItemTypeText
> 							348 편집 크기 ID: System.Size
> 				349 텍스트 파일 이름: Description: 파일 이름: ID: SaveDialogLabel
> 				350 콤보 상자 파일 이름: ID: FileNameControlHost
> 					351 편집 파일 이름: ID: 1001
> 					352 단추 열기 ID: DropDown
> 				353 텍스트 파일 형식: Description: 파일 형식: ID: SaveDialogLabel
> 				354 콤보 상자 파일 형식: ID: FileTypeControlHost
> 					355 텍스트
> 					356 단추 열기 ID: DropDown
> 				357 트리 필드 저장 ID: SaveDialogPreviewMetadataInner
> 				358 트리 응용 프로그램 컨트롤 ID: AppControlsModuleInner
> 			359 도구 모음
> 				360 단추 폴더 숨기기
> 			361 단추 저장(S) ID: 1
> 			362 단추 취소 ID: 2
> 			363 위치 조정
> 			364 창 ID: 40965
> 				365 창
> 					366 도구 모음 탐색 단추
> 						367 단추 (disabled) 뒤로(Alt+왼쪽 화살표)
> 						368 단추 (disabled) 앞으로(Alt+오른쪽 화살표)
> 						369 단추 (disabled) 최근 위치
> 				370 창
> 					371 도구 모음 업밴드
> 						372 단추 "SIGMATEK_API_Analyze"(으)로 이동(Alt+위쪽 화살표)
> 				373 창 ID: 41477
> 					374 진행률 표시줄 로드 중
> 						375 창
> 							376 도구 모음 주소: C:\work\Elmo\Elmo_Master\test\packet_capture\SIGMATEK_API_Analyze\Test4 ID: 1001
> 								377 분할 단추 모든 위치
> 								378 분할 단추 ‎Elmo_Master
> 								379 분할 단추 ‎test
> 								380 분할 단추 ‎packet_capture
> 								381 분할 단추 ‎SIGMATEK_API_Analyze
> 								382 메뉴 항목 ‎Test4
> 						383 도구 모음 주소 대역
> 							384 단추 이전 위치
> 							385 단추 "Test4" 새로 고침(F5)
> 				386 창
> 					387 창
> 						388 창
> 							389 편집 검색창 Description: Test4 검색 ID: SearchEditBox
> 							390 단추 검색 ID: SearchBoxSearchButton
> 			391 제목 표시줄
> 				392 단추 닫기
>
> Document text: ```
> [16:39:10.444] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-20 07:38:58 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5, ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2, SdkPath=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Release\LasalMotionControlLib.dll, SdkBuildUtc=2026-08-20 07:38:55 UTC.
> [16:39:10.445] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [16:45:55.685] Connect started.
> [16:45:55.773] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:45:55.785] Ignored stale connection-state event Connecting from an older transport session.
> [16:45:55.785] Connection state Connecting -> Connected
> [16:45:55.893] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=INITIAL, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:45:55.904] Connect PASS.
> [16:45:58.785] Close Connection started.
> [16:45:58.789] Connection state Connected -> Closing
> [16:45:58.805] Close Connection PASS.
> [16:46:01.250] Connect started.
> [16:46:01.252] Connection state Disconnected -> Connecting
> [16:46:01.264] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:01.265] Connection state Connecting -> Connected
> [16:46:01.339] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:01.339] Connect PASS.
> [16:46:02.611] Close Connection started.
> [16:46:02.611] Connection state Connected -> Closing
> [16:46:02.621] Close Connection PASS.
> [16:46:06.999] Connect started.
> [16:46:07.001] Connection state Disconnected -> Connecting
> [16:46:07.011] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [16:46:07.012] Connection state Connecting -> Connected
> [16:46:07.084] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [16:46:07.084] Connect PASS.
> [16:47:58.941] Submit SDO Read started.
> [16:47:58.966] QTEST|utc=2026-08-20T07:47:58.966Z|elapsedMs=0|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [16:47:58.973] QTEST|utc=2026-08-20T07:47:58.973Z|elapsedMs=8|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [16:47:58.981] QTEST|utc=2026-08-20T07:47:58.981Z|elapsedMs=16|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [16:47:59.055] QTEST|utc=2026-08-20T07:47:59.055Z|elapsedMs=91|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=135|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [16:47:59.057] QTEST|utc=2026-08-20T07:47:59.057Z|elapsedMs=93|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=5024ab2be40542f5b1228766d313d0d8|ticket=135|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=135|quarantine=false
> [16:47:59.060] Submit SDO Read PASS.
> [16:47:59.096] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000087
> [16:47:59.117] QTEST|utc=2026-08-20T07:47:59.117Z|elapsedMs=151|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=135|state=Completed|outcome=Success|verdict=CLEARED
> [16:47:59.118] QTEST|utc=2026-08-20T07:47:59.118Z|elapsedMs=152|run=d712c406e097429ba343af195ec621bd|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [16:47:59.124] Callback D5 authoritative TCP status processed. TicketId=0x00000087
> [17:04:28.599] Submit SDO Read started.
> [17:04:28.610] QTEST|utc=2026-08-20T08:04:28.610Z|elapsedMs=0|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [17:04:28.619] QTEST|utc=2026-08-20T08:04:28.619Z|elapsedMs=9|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [17:04:28.622] QTEST|utc=2026-08-20T08:04:28.622Z|elapsedMs=11|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [17:04:28.631] QTEST|utc=2026-08-20T08:04:28.631Z|elapsedMs=20|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=136|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [17:04:28.633] QTEST|utc=2026-08-20T08:04:28.633Z|elapsedMs=22|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=efa09c2894394ca89332a19ab132a9df|ticket=136|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=136|quarantine=false
> [17:04:28.659] Submit SDO Read PASS.
> [17:04:28.677] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000088
> [17:04:28.690] QTEST|utc=2026-08-20T08:04:28.690Z|elapsedMs=79|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=136|state=Completed|outcome=Success|verdict=CLEARED
> [17:04:28.692] QTEST|utc=2026-08-20T08:04:28.692Z|elapsedMs=81|run=12a64330ed6b4feb84f19e9230c52ec4|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [17:04:28.696] Callback D5 authoritative TCP status processed. TicketId=0x00000088
> [17:04:39.215] Close Connection started.
> [17:04:39.233] Connection state Connected -> Closing
> [17:04:39.253] Close Connection PASS.
> [17:04:52.865] Connect started.
> [17:04:52.877] Connection state Disconnected -> Connecting
> [17:04:52.931] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0x00000001
> [17:04:52.990] Connection state Connecting -> Connected
> [17:04:53.069] EtherCAT topology loaded (automatic post-connect load). Revision=0x15867EEC, Nodes=7, CREVIS=3, ConfiguredComparison=UNCHANGED, SHA256=C04AEB1F36E36753160317F95E2DF96E3E204D60EE5184EF9E240FBDE1F4D8AD.
> [17:04:53.076] Connect PASS.
> [17:05:11.661] Submit SDO Read started.
> [17:05:11.673] QTEST|utc=2026-08-20T08:05:11.673Z|elapsedMs=0|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=1|event=BEGIN|stage=manual-sdo-read-submit
> [17:05:11.681] QTEST|utc=2026-08-20T08:05:11.681Z|elapsedMs=7|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=2|event=D5_EXTERNAL_PREFLIGHT|stage=manual-sdo-read-submit|bootId=0x00000042|mapRevision=0x957F101E|requiredDataBytes=1|generalInlineRequired=true|verdict=PASS
> [17:05:11.683] QTEST|utc=2026-08-20T08:05:11.683Z|elapsedMs=9|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=3|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=71153dec1118442c891bd7f4d9977378|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|state=ARMED_BEFORE_SUBMIT
> [17:05:11.697] QTEST|utc=2026-08-20T08:05:11.697Z|elapsedMs=23|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=4|event=D5_EXTERNAL_TICKET_PRESERVED|stage=manual-sdo-read-submit|ticket=137|slave=1|bootId=0x00000042|mapRevision=0x957F101E|operationKind=SDORead|requestMetadata=AVAILABLE|object=0x6061|subIndex=0|valueType=Int8|dataLength=1|writeData=N/A|terminalWaitMs=6000
> [17:05:11.699] QTEST|utc=2026-08-20T08:05:11.699Z|elapsedMs=25|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=5|event=D5_EXTERNAL_SUBMIT_GUARD|stage=manual-sdo-read-submit|evidence=71153dec1118442c891bd7f4d9977378|ticket=137|bootId=0x00000042|mapRevision=0x957F101E|state=ACCEPTED_TICKET|detail=137|quarantine=false
> [17:05:11.703] Submit SDO Read PASS.
> [17:05:11.730] D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x00000089
> [17:05:11.745] QTEST|utc=2026-08-20T08:05:11.745Z|elapsedMs=72|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=6|event=D5_EXTERNAL_TICKET_TERMINAL|stage=callback-d5-terminal-wake|ticket=137|state=Completed|outcome=Success|verdict=CLEARED
> [17:05:11.748] QTEST|utc=2026-08-20T08:05:11.748Z|elapsedMs=74|run=cdf892cb0af54437b7e0a2d2637b88be|scenario=D5ExternalTracking:manual-sdo-read-submit|step=7|event=END|resolution=KNOWN_TICKET_TERMINAL|verdict=RESOLVED
> [17:05:11.754] Callback D5 authoritative TCP status processed. TicketId=0x00000089
> ```
> ````
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "QTEST 저장 파일명 입력칸 선택",
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"No accessibility observation\");\n  globalThis.state = null;\n  await sky.click({ window: observation.window, element_index: 351 });\n  globalThis.state = await sky.get_window_state({\n    window: observation.window,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || \"\"));\n}"
> }
> ```
>
> ```text
> element 351 is not available in cached app state for LasalMotionControlApiExample.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "저장 대화상자 화면 영역 확인",
>   "code": "nodeRepl.write(JSON.stringify((globalThis.state?.screenshots || []).map(s => ({id:s.id,width:s.width,height:s.height,originX:s.originX,originY:s.originY,zIndex:s.zIndex})), null, 2));"
> }
> ```
>
> ```text
> []
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "저장 대화상자 다시 관찰",
>   "code": "{\n  globalThis.state = await sky.get_window_state({\n    window: targetWindow,\n    include_screenshot: true,\n    include_text: true,\n  });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(JSON.stringify((state.screenshots || []).map(s => ({id:s.id,width:s.width,height:s.height,originX:s.originX,originY:s.originY,zIndex:s.zIndex})), null, 2));\n}"
> }
> ```
>
> ```text
> [
>   {
>     "id": "screenshot-0",
>     "width": 1920,
>     "height": 1032,
>     "originX": 0,
>     "originY": 0,
>     "zIndex": 0
>   },
>   {
>     "id": "screenshot-1",
>     "width": 1920,
>     "height": 1032,
>     "originX": 0,
>     "originY": 0,
>     "zIndex": 1
>   }
> ]
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
</details>

직접 시험을 진행했습니다.

결론은 다음과 같습니다.

| 시험 | 판정 | 확인 결과 |
|---|---|---|
| GD-01, ticket 136 | PASS | Submit 1회 → UDP 52바이트 1회 → 자동 `0x7E03` 1회 → Completed/Success |
| Clean Close | PASS (패킷/UI) | `0x405D` 요청·성공 응답 후 TCP 정상 종료 |
| GD-04 같은 창 Reconnect | PASS (패킷/UI) | UDP 5000 재사용, 새 TCP 연결·Epoch·Cookie 등록 |
| 재접속 후 GD-01, ticket 137 | PASS | 새 세션 callback → 자동 `0x7E03` → Completed/Success |
| 정식 Gate-D 전체 종료 | 미완료 | LASAL Online Watch의 PLC 내부 카운터 증거가 아직 없음 |

### 직접 수행한 순서

1. 기존 연결에서 `0x6061:0`, `Int8`, 길이 1, timeout 1000으로 `Submit SDO Read` 1회
2. 같은 WPF 창에서 `닫기`
3. 같은 창, 같은 UDP 5000으로 다시 `연결`
4. 동일한 SDO Read를 다시 1회 수행

Inline Read, Refresh Ticket, Cancel, SDO Write, Motion 명령은 실행하지 않았습니다.

### 핵심 패킷 결과

Ticket 136:

- Submit → UDP: 약 38.4 ms
- UDP → 자동 `0x7E03`: 약 20.3 ms
- UDP → terminal 응답: 약 23.1 ms
- 결과: `Completed / Success / Int8 / Data=08`
- callback 전 수동 `0x7E03`: 0회

Reconnect:

- `0x405D`: `Status=0, ErrorId=0`
- SessionEpoch: `27 → 30`
- Cookie: `0x521F125D06FCDE46 → 0x2A978FF0DA1E78F3`
- UDP 포트: 계속 `5000`
- 새 세션 sequence: `1`부터 다시 시작

Ticket 137:

- Submit → UDP: 약 32.9 ms
- UDP → 자동 `0x7E03`: 약 11.7 ms
- UDP → terminal 응답: 약 17.1 ms
- 결과: `Completed / Success / Int8 / Data=08`
- callback 전 수동 polling: 0회

TCP 재전송·loss·out-of-order도 0건입니다.

### 생성한 증거 파일

- [Codex_GD01_GD04_20260820.pcapng](</C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test6/Codex_GD01_GD04_20260820.pcapng>)
  SHA-256: `C9AA453B20B595F2E7251A37C52656F8BC17021F7F1C6514610A0FFD3B6F2018`

- [20260820_170925_qualification.txt](</C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/Test6/20260820_170925_qualification.txt>)
  SHA-256: `AF860356A5A5F6E997F5C41959575D3E9C97078F07458E14937F533CD3072409`

시험 실행 파일 SHA-256:

- EXE: `CA0B006EB21DD447512B5C9343533B5471A8718D80ACFCBD2BF7343FA02EE550`
- SDK DLL: `256E0E3F67D034EF476BCD3A89653108AC940B9664715EE25E0300D9700ADE77`

### 사용자가 그대로 반복할 때

- `Read`, Slave `1`
- Object `0x6061`
- SubIndex `0`
- Type `Int8`
- Length `1`
- Timeout `1000`
- `SDO Read 전송`만 1회 클릭
- Inline/Refresh/Cancel은 누르지 않음

정상이라면:

```text
State=Completed
Outcome=Success
ErrorId=0
ResultType=Int8
ResultLength=1
Data=08
```

그리고 로그에 다음 순서가 나와야 합니다.

```text
D5 terminal wake matched retained ticket
authoritative TCP status query started
D5_EXTERNAL_TICKET_TERMINAL
verdict=RESOLVED
Callback D5 authoritative TCP status processed
```

현재 WPF는 연결된 상태로 남겨뒀습니다.

남은 것은 LASAL Watch 증거입니다. 정식 Gate-D를 닫으려면 다음 증가량만 추가 확인하면 됩니다.

- `D5TerminalWakeAttemptCount +1`
- `D5TerminalWakeEnqueuedCount +1`
- `D5TerminalWakeRejectedCount +0`
- `QueuedCount +1`
- `RingAcceptedCount +1`
- 최종 `QueueDepth=0`
- drop/retry/transport error 증가 0
- Close 후 `RpcCallbackLastDisarmResult=0 또는 1`
- callback tuple clear 및 `QueueDepth=0`

즉, callback과 reconnect 동작 자체는 이번 시험에서 확인됐고, 남은 것은 PLC 내부 Watch 카운터를 캡처하는 작업입니다.

> 이 변수들은 어디서 Watch하나?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-00a24c38-4fca-41df-a233-eb572bdf6a8d.png`

<details><summary>2 previous messages</summary>

> 스크린샷의 `???`는 변수를 네트워크 전역 범위에 이름만 입력해서, 실제 선언 객체를 찾지 못한 상태입니다. 소스에서 각 변수의 소유 객체와 LASAL Watch에 넣을 정확한 경로를 확인하겠습니다.
>
> 위치는 확인됐습니다. 모두 `Motion_Network` 변수가 아니라 `Comm_Network`의 두 객체에 나뉘어 있습니다. 지금처럼 이름만 넣으면 안 되고 `객체인스턴스.변수명`으로 넣어야 합니다.
</details>

지금 `???`가 뜨는 이유는 변수명 앞에 실제 객체 인스턴스가 없기 때문입니다. 이 변수들은 `Motion_Network`가 아니라 `Comm_Network`의 두 객체에 있습니다.

현재 `Watch2`에 아래 이름을 그대로 입력하세요.

```text
TCPMotionInterface1.D5TerminalWakeAttemptCount
TCPMotionInterface1.D5TerminalWakeEnqueuedCount
TCPMotionInterface1.D5TerminalWakeRejectedCount
TCPMotionInterface1.RpcCallbackLastDisarmResult

LMCUdpCallbackSender1.QueuedCount
LMCUdpCallbackSender1.RingAcceptedCount
LMCUdpCallbackSender1.QueueDepth
```

화면에 중복으로 들어간 `QueueDepth` 한 줄은 삭제해도 됩니다.

객체 위치를 직접 확인하려면:

1. 우측 아래 `Net` 탭 클릭
2. `Comm_Network` 열기
3. 다음 두 객체 확인

   - `TCPMotionInterface1`
   - `LMCUdpCallbackSender1`

`Watch1`, `Watch2`, `Watch3` 중 어느 탭을 사용해도 상관없습니다. `Motion_Network` 화면을 열어둔 상태에서도 위처럼 객체명을 붙이면 인식되어야 합니다.

같이 추가하면 좋은 Sender 진단값은 다음과 같습니다.

```text
LMCUdpCallbackSender1.AdmissionRetryCount
LMCUdpCallbackSender1.QueueFullDropCount
LMCUdpCallbackSender1.AdmissionErrorDropCount
LMCUdpCallbackSender1.DisarmClearedCount
LMCUdpCallbackSender1.TransportErrorCount
LMCUdpCallbackSender1.LastAdmissionResult
```

판정할 때는 절대값이 아니라 시험 전후 차이를 봅니다.

- 정상 D5 Read 1회:

  - Attempt `+1`
  - Enqueued `+1`
  - Rejected `+0`
  - Queued `+1`
  - RingAccepted `+1`
  - 최종 QueueDepth `0`

- Clean Close 후:

  - `RpcCallbackLastDisarmResult=0` 또는 `1`
  - QueueDepth `0`

정확한 객체명을 넣어도 계속 `???`이면 Watch 입력 문제가 아니라, 현재 PLC에 다운로드된 이미지와 LASAL IDE 디버그 심벌이 일치하지 않을 가능성이 큽니다. 그 경우 바로 Rebuild/Download하지 말고 알려주세요. 이미지 출처부터 확인해야 합니다.

선언 근거: [TCPMotionInterface.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:144), [LMCUdpCallbackSender.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st:111)

> 정상요청인데 된건가?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-a9931032-6be9-43a4-a4cf-e42c84d1c82e.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-729e39f5-6f30-4362-9a61-700be286ce90.png`

<details><summary>1 previous message</summary>

> 두 화면의 차이는 분명합니다. PLC producer 쪽은 `134→135`, `134→135`, `Rejected=0`으로 정상 경로를 탔지만, Sender의 `QueuedCount`와 `RingAcceptedCount`가 계속 0인 점은 기대값과 맞지 않습니다. 이 값이 실제로 언제 초기화·증가하는지 소스와 함께 확인해서 판정을 분리하겠습니다.
</details>

정상 요청 자체는 처리됐습니다. 하지만 전체 GD-01 PASS로 판정하기에는 Sender Watch 증거가 맞지 않습니다.

| 변수 | 요청 전 | 요청 후 | 판정 |
|---|---:|---:|---|
| Attempt | 134 | 135 | 정상 `+1` |
| Enqueued | 134 | 135 | 정상 `+1` |
| Rejected | 0 | 0 | 정상 `+0` |
| QueuedCount | 0 | 0 | 비정상, `+1` 필요 |
| RingAcceptedCount | 0 | 0 | 비정상, `+1` 필요 |
| QueueDepth | 0 | 0 | 송신 완료 후 0이면 정상 |

즉 PLC producer는 callback 요청을 받아 `PublishEvent Result=0`으로 처리했습니다. 그러나 현재 소스에서는 Result 0을 반환하기 전에 `QueuedCount`가 반드시 증가하고, 실제 UDP ring에 전달되면 `RingAcceptedCount`도 증가해야 합니다. 둘 다 0인 것은 빠른 처리로 설명되지 않습니다. `QueueDepth=0`만 빠른 송신 완료로 설명할 수 있습니다.

`RpcCallbackLastDisarmResult=1`은 이번 정상 요청과 관계없습니다. Close 시험 때 보는 값입니다. `LastAdmissionResult=0`도 초기값이 0이므로 단독 성공 증거가 아닙니다.

현재 판정은:

- D5 producer 요청 처리: PASS
- Sender 누적 카운터: 증거 불일치
- 실제 UDP callback: WPF `Accepted +1` 또는 pcap 확인 필요
- GD-01 전체: 아직 INCONCLUSIVE

또한 LASAL 로그에는 이번 IDE 실행 후 기존 PLC에 `Going online`한 기록만 있고 새 Download 기록은 없습니다. 현재 IDE 심벌/소스와 PLC 실행 이미지가 다른 가능성이 큽니다. 지금 바로 Rebuild/Download하지 마세요.

WPF에서 다음을 확인해 주세요.

```text
Accepted: +1
Rejected/Duplicate/OutOfOrder: 변화 없음
D5 terminal wake matched retained ticket
Callback D5 authoritative TCP status processed
State=Completed, Outcome=Success
```

이 내용이 보이면 실제 callback 기능은 성공한 것이고, 남는 문제는 Sender Watch 카운터와 PLC 이미지/심벌 불일치입니다. 현재 WPF callback 화면을 캡처해 주면 이번 요청까지 최종 판정할 수 있습니다.

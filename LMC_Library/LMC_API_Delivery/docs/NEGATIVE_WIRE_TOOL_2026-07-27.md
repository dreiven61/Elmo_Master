# Internal diagnostics negative-wire tool

- 작성일: 2026-07-27
- 위치: `tests/LasalMotionControlLib.Tests/NegativeWireTool.cs`
- 실행 파일: `LasalMotionControlLib.Tests.exe`
- 상태: PC Debug/Release 각 260/260 중 전용 계약 시험 9개와 dry-run PASS;
  실제 PLC raw rejection/pcap은 미실행

## 목적과 경계

public SDK는 stale handle, stale session과 두 번째 Release를 wire 전 차단한다. 이 보호를
약화하지 않고 PLC의 raw diagnostics rejection만 확인하기 위해 기존 PC test executable에
명시적인 `negative-wire` 모드를 추가했다.

인자 없이 실행하면 기존 자동 테스트만 실행한다. `negative-wire` 모드는 배포 WPF와 public
SDK에 포함되지 않으며 임의 command ID, reference 또는 hex payload를 받지 않는다. raw
allowlist는 아래 다섯 고정 시나리오뿐이다. motion, Admin, 모든 write, SDO Submit,
Recorder 명령은 생성하거나 송신할 수 없다.

전체 260개는 직전 256개 + UI 독립 D5 quarantine ledger deterministic concurrency 4개다.
각 등록 test는 50회 반복해 candidate snapshot 뒤 clear 전 mutation, atomic clear 뒤 Arm
보존, callback 예외 뒤 waiter/ledger 재사용과 concurrent Disarm exact-once를 bounded wait로
검증하며 `Thread.Sleep`을 사용하지 않는다. 이 추가분은 PC test뿐이고
production/wire/LASAL 변경이나 PLC live/pcap 증거가 아니다. 기존 UI 독립
`D5SdoRecoveryScopePolicy`는 owner
reference+BootId+MapRevision 조합으로 네 scope를 순수 판정하고 MainWindow의 proof 시작/PASS
로그가 같은 decision을 쓴다. `mixed_evidence_sessions`도 application recovery proof는 허용하지만
same/new session 증거로 세지 않으며, 한 previous owner+identity의 동질 집합만
`NewConnectionRecovery=true`다. drive facade context는 원래 exception 객체/타입/stack을 바꾸지 않고
`LMCDriveReadFailureContext.TryGet`으로 제공한다. drive phase는
`FacadePreflight`/`AxisStatusRead`/`CapabilityPreflight`/`Submission`/`StatusPolling`/
`ResultMaterialization`, `GenericSubmissionOutcome`은 공용 `LMCSdoSubmissionOutcome`
(`NotAttempted`/`Rejected`/`OutcomeUncertain`/`Accepted`)이다. 기존
`SubmissionOutcome`/`LMCSdoReadSubmissionOutcome`은 호환용으로 같은 값을 유지한다.
각 attempt는 확보된 실제 `DiagnosticsBootId`/`MapRevision`을 보존하며, WPF는
no-submit/rejected/terminal은 guard 해제, uncertain은 quarantine, accepted nonterminal은 exact
ticket 보존, context 누락·불일치는 fail-closed로 처리한다.
raw manual `SubmitSdo[Async]`는 동일하게 예외 객체/타입/stack을 보존하고
`LMCSdoSubmissionFailureContext.TryGet`으로 `LMCSdoSubmissionPhase`
(`RequestValidation`/`SessionPreflight`/`CapabilityPreflight`/`Submission`/
`PostSubmissionValidation`), 공통 outcome, request, capability identity 확보 후의 실제
BootId/MapRevision과 accepted ticket을 제공한다. identity 확보 전 실패는 두 값을 `0`
sentinel로 둔다. WPF manual router는 no-submit/rejected를
disarm하고, uncertain은 actual BootId/MapRevision을 reconcile한 뒤 quarantine하며,
accepted는 exact ticket을 manual diagnostic state+D5 tracker에 보존한 뒤 disarm한다.
누락·불일치는 fail-closed다. 이는 code/test 계약이며 PLC live/pcap 증거가
아니다. negative-wire 모드 자체는 SDO Submit을 전혀 생성하지 않는다.

| 시나리오 | raw command | 고정 변형 | 기대 Detail | resource 처리 |
|---|---:|---|---:|---|
| `malformed-payload` | `0x7E01` | 정상 8-byte 대신 trailing zero를 붙인 9-byte payload | `BoundsInvalid(12)` | 새 connection에서 단독 실행 |
| `stale-map` | `0x7E02` | 현재 값과 다른 nonzero MapRevision, start 0, count 1 | `MapRevisionMismatch(3)` | read-only |
| `stale-boot` | `0x7E03` | TicketId 1, 현재 값과 다른 nonzero BootId | `BootIdMismatch(25)` | SDO를 submit하지 않는 read-only status |
| `stale-config` | `0x7E31` | public Configure로 만든 Bulk의 ConfigRevision만 변경 | `HandleOrGenerationStale(10)` | 결과와 무관하게 public Release 재시도 |
| `duplicate-bulk-release` | `0x7E33` | public Release 성공 뒤 같은 identity로 raw Release 1회 | `HandleOrGenerationStale(10)` | 첫 Release와 local handle 상태는 public SDK로 확정 |

오류 PASS는 Detail 하나만 보지 않는다. outer HeaderStatus/Reserved 0, payload 정확히 16
bytes, SchemaVersion 1, ResponseFlags None, CommandStatus 1, ErrorId -32000, RequestId exact
echo와 Detail exact를 모두 요구한다.

## dry-run

아래 명령은 TCP/UDP socket을 열지 않고 실행 계획만 출력한다.

```powershell
LasalMotionControlLib.Tests.exe negative-wire --dry-run --scenario all
```

개별 `--scenario` 값은 다음만 허용한다.

```text
all
malformed-payload
stale-map
stale-boot
stale-config
duplicate-bulk-release
```

## live 실행

live 실행은 exact mode token, `--execute-live`, exact confirmation, 명시적 IPv4 두 개와
기존 파일이 아닌 새 report 경로가 모두 있어야 한다. timeout은 250~10000 ms로 제한되고
callback UDP port는 0을 사용해 OS가 빈 port를 선택한다.

```powershell
LasalMotionControlLib.Tests.exe negative-wire `
  --execute-live `
  --confirm PLC-RAW-NEGATIVE `
  --host 10.10.150.1 `
  --port 4000 `
  --local 10.10.150.13 `
  --timeout-ms 3000 `
  --output C:\temp\elmo_negative_wire_20260727.txt `
  --scenario all
```

Bulk Configure가 `ResourceBusy`이면 다른 owner의 resource를 강제 해제하지 않고 실패한다.
stale-config의 소유 resource만 public Release로 cleanup한다. duplicate-release는 public
Release가 성공하여 local handle까지 Released가 된 뒤에만 raw duplicate를 보낸다.

## report와 증거 경계

TXT report는 endpoint, capability/map/BootId, scenario, command, expected/actual Detail,
request/response byte count, hex와 SHA-256, cleanup 및 connection close 결과를 기록한다.
`PCAP_EVIDENCE=NOT_CAPTURED_BY_TOOL`도 항상 기록한다.

따라서 report PASS는 PC가 보낸 raw request와 받은 PLC response 계약을 증명하지만
독립 packet capture 완료를 뜻하지 않는다. Wireshark 증거가 필요하면 별도 pcap과 이 report의
request/response hex 및 SHA-256을 대조해야 한다.

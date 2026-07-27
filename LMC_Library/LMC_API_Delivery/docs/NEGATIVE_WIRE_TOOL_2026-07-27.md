# Internal diagnostics negative-wire tool

- 작성일: 2026-07-27
- 위치: `tests/LasalMotionControlLib.Tests/NegativeWireTool.cs`
- 실행 파일: `LasalMotionControlLib.Tests.exe`
- 상태: PC Debug/Release 각 223/223 중 전용 계약 시험 9개와 dry-run PASS;
  실제 PLC raw rejection/pcap은 미실행

## 목적과 경계

public SDK는 stale handle, stale session과 두 번째 Release를 wire 전 차단한다. 이 보호를
약화하지 않고 PLC의 raw diagnostics rejection만 확인하기 위해 기존 PC test executable에
명시적인 `negative-wire` 모드를 추가했다.

인자 없이 실행하면 기존 자동 테스트만 실행한다. `negative-wire` 모드는 배포 WPF와 public
SDK에 포함되지 않으며 임의 command ID, reference 또는 hex payload를 받지 않는다. raw
allowlist는 아래 다섯 고정 시나리오뿐이다. motion, Admin, 모든 write, SDO Submit,
Recorder 명령은 생성하거나 송신할 수 없다.

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

# LMC Encoder Maintenance TW19/TW20 fixed-one activation

Date: 2026-08-04

## 결론

TW19와 TW20의 SDO write 값은 feedback socket이나 축 번호가 아니다. 선택한 drive 축에 대해
아래 값을 정확히 한 번 쓴다.

| Operation | Drive object | Data type | Write value |
|---|---:|---|---:|
| TW19 multi-turn position reset | `0x20FC:0x01` | UInt16, 2 bytes | `1` |
| TW20 error/warning reset | `0x20FC:0x02` | UInt16, 2 bytes | `1` |

`DriveReference` `1..4`가 축별 `LMCSdoExecutor`를 선택한다. `0x3204:0x13/0x14` alias,
generic `0x7E50` fallback, retry 및 dual-send는 허용하지 않는다.

## 2026-08-04 source activation

`LMCDiagnosticsService`의 아래 gate를 `TRUE`로 활성화했다.

- `LMC_DIAG_ENCODER_TW20_ENABLED`
- `LMC_DIAG_ENCODER_TW19_ENABLED`

축별 `PROFILE/SOCKET/EVIDENCE` manifest 48개는 제거했다. 이 manifest는 실제 SDO target과
write value를 선택하지 않으면서 모든 축을 영구 차단했고, `CommandValue=FeedbackSocket`이라는
잘못된 계약을 강제했다.

Wire ABI는 기존 복구 record와의 호환성을 위해 유지한다.

- Start `0x7E53`: 72-byte payload
- Outcome `0x7E54`: 72-byte request, 156-byte response
- Retire `0x7E55`: 76-byte request, 156-byte response
- 기존 profile/socket/evidence 필드: exact recovery/audit identity로만 echo한다.
- `CommandValue`: 항상 `1`

새 SDK caller는 drive와 timeout만 지정하는 2-argument request constructor를 사용할 수 있다.
기존 5-argument constructor도 source compatibility를 위해 유지하지만 socket 값은 write data가
되지 않는다.

## 유지되는 safety boundary

활성화는 무제한 raw SDO write 허용이 아니다. 전용 경로는 다음 조건을 계속 요구한다.

1. 현재 Diagnostics Build, nonzero BootId, MapRevision identity가 일치해야 한다.
2. 해당 축의 공통 axis ownership 예약과 commit이 일치해야 한다.
3. 시작 전과 write 후에 motor-off/standstill fresh sample을 확인해야 한다.
4. 선택한 축의 전용 SDO executor가 reusable이어야 한다.
5. one-shot dispatch claim 뒤에는 자동 retry/replay하지 않는다.
6. callback과 executor drain, owner cleanup이 완료된 뒤에만 terminal을 게시한다.

Terminal `Succeeded`가 증명하는 것은 exact SDO write completion, executor drain,
motor-off post observation 및 owner cleanup이다. 실제 encoder error/warning 또는 multi-turn
position이 원하는 상태로 바뀌었는지는 별도 실기 확인이 필요하다. TW19 뒤의 LMC Home
current-position-zero는 권고가 아니라 다음 PowerOn/motion을 차단하는 retained barrier 계약이다.

## TW19 retained Home barrier 설계

`LMCControlCommandService`에 Comm Network 연결이 없는 hidden server channel 하나를 둔다.

```text
AxisRebaseRequiredState : SvrCh_UDINT
  Initialize     = true
  DefValue       = 0x5242530F
  WriteProtected = false
  Retentive      = File
  Visualized     = false
```

하위 4 bit는 축 1..4의 `RebaseRequired` mask이고 상위 nibble은 그 mask의 4-bit inverse다.
상위 24 bit magic은 `0x524253`이다.

```text
encoded = 0x52425300 + ((mask xor 0xF) << 4) + mask
```

- `0x5242530F`: 유효한 초기 상태, 4축 모두 LMC Home 필요
- `0x524253F0`: 유효한 empty 상태
- magic/complement가 맞지 않는 모든 값: effective mask `0xF`로 fail closed

already-large reservation/publication method에 persistent word codec을 반복 삽입하지 않는다.
private `ReadAxisRebaseRequiredMask`가 decode/fail-closed mask를 전담하고 private
`UpdateAxisRebaseRequiredState`가 set/clear, encode, channel write와 exact readback을 전담한다.

TW19 barrier의 arm linearization point는 SDO 성공 뒤가 아니다. `CommitAxisOwnership`에서 exact
`0x7E53`, Encoder owner, Diagnostics SDO resource, Lifecycle admission, maintenance kind `2`, 단일
축 reference/mask를 모두 검증한 뒤 owner record를 ACTIVE/QUEUED로 바꾸기 전에 해당 축 bit를
set하고 readback한다. 따라서 arm 확인 전에는 SDO write가 한 번도 나갈 수 없다. arm 뒤 SDO가
실패하거나 결과가 불확실해도 bit는 유지한다. TW20 kind `1`은 bit를 set하지 않는다.

bit가 set된 축에도 다음 경로는 허용한다.

- 모든 read/status/outcome query
- Axis/Group Reset
- Axis Stop/PowerOff
- Group Disable/Stop/PowerOff
- TW19/TW20 Start, Outcome, Retire
- exact LMC Home Start, Outcome, Retire
- DS402 Home을 포함한 기존 ledger의 비동작 Outcome/Retire cleanup

다음 경로는 해당 축 또는 해당 축을 포함하는 Group에 대해 native call 전에 차단한다.

- Axis PowerOn과 MoveAbsolute/MoveRelative/MoveVelocity
- `0x7D15` DS402 Home Start
- Group Enable/PowerOn/motion/`0x7D22` mutation
- `0x20E7` SetKin은 full `kinValid` parser 성공 뒤 `GroupKinematicReady`/native marker 전에 차단

`0x20E7` malformed payload는 barrier conflict로 바꾸지 않고 기존 `-7` parser result를 유지한다.
fully valid이며 affected Group인 경우에만 rebase conflict를 반환하고 native marker/configuration
mutation은 0회다. `0x7D12` SetPosition은 현재 dormant/unavailable 상태를 그대로 유지한다. 향후
활성화하려면 정상 request shape parsing 뒤, native call 전 동일 barrier를 적용하는 것이 선행
조건이다.

이 barrier check는 unconditional이다. ordinary ownership, DS402 Home 또는 startup activation gate가
아직 dormant여도 우회하지 않는다.

Reset, DS402 Home, TW20, owner cleanup 또는 session cleanup은 bit를 clear하지 않는다. clear가
가능한 유일한 경로는 exact `0x7D13` LMC Home의 terminal-success receipt가 COMPLETE이고 기존
token/generation/session/sequence/reference/mask/evidence 검증이 모두 일치하는 경우다. 해당 bit를
clear하고 retained word readback까지 확인한 뒤에만 outcome `Result=1`을 반환한다. persistence
확인이 실패하면 COMPLETE receipt를 보존한 채 다음 exact outcome read에서 재시도하며 global
ownership quarantine으로 승격하지 않는다.

형식이 잘못된 요청은 기존 parser 오류를 유지해야 한다. barrier는 정상 형식의 금지된 mutation에만
기존 command-specific ownership-conflict/fail-closed response를 반환하고 owner/native/SDO call을
0회로 유지한다. adapter ABI가 적용되는 경로는 symbolic `-9 AxisOwnershipConflict`를 사용하고
Admin 경로는 기존 Admin envelope/error detail 형식을 유지한다. 상세 공통 ownership 계약은
[axis ownership overlay IDE handoff](./LMC_AXIS_OWNERSHIP_OVERLAY_IDENTITY_RESTORE_IDE_HANDOFF_2026-08-04.md#10-tw19-retained-current-position-zero-barrier)를 따른다.

이 설계는 TW19 gate가 `TRUE`라는 사실만으로 완료되지 않는다. hidden channel declaration,
generated metadata, external implementation, C78, download, restart/power-loss retention 및 실축
차단/해제 증거가 모두 필요하다. 그 전 TW19는 다음 motion까지 포함한 기능으로 qualified 상태가
아니다.

## Capability와 2026-08-04 runtime 결함 판정

현재 source에서 DiagnosticsBootId가 nonzero이고 현재 SDO Write axis-1 gate가 포함되면 예상
Diagnostics CapabilityBits는 `0x000C633F`다.

- bit 8: SDORead
- bit 9: SDOWrite
- bit 13: SDOReadGeneralInline
- bit 14: EtherCATTopology
- bit 18: TW20
- bit 19: TW19

09:33 C78 rebuild/download/restart 뒤 09:35 WPF 및 독립 SDK read-only 조회는 모두
`DiagnosticsBits=0x00000001`, `BootId=0x00000010`, `MapRevision=0x957F101E`를 반환했다.
최신 binary가 실행됐지만 capability overlay가 아래처럼 `|`를 사용한 것이 원인이었다.

```st
bits := bits | mask;
```

C78 runtime에서 이 식은 UDINT bitwise OR가 아니라 BOOL 결합으로 평가돼 nonzero mask 전체를
`1`로 축약했다. 그 결과 기존 topology, SDORead 및 recorder bit도 모두 소실됐다. 세 overlay를
정수 bitwise 연산인 `OR`로 수정했다.

```st
bits := bits OR mask;
```

같은 연산자 전수 감사에서 encoder maintenance의 verification flag 누적 7개 식과 retired
tombstone 1개 식에도 `|` 13개가 남아 있었다. 그대로 두면 capability가 정상화된 뒤 실제 SDO
write가 끝나도 verification mask가 `1`로 축약되어 exact `0x000003FF` 성공 판정을 통과할 수
없고, retire 상태도 손상된다. 이 8개 정수 식도 모두 `OR`로 수정했다.

정적 verifier는 capability overlay, verification flag 및 retired tombstone에 정수 `OR`만
허용하고 `|` negative fixture를 거부한다. 이 수정 binary의 PLC runtime 결과는 다음
rebuild/download/restart 전까지 미검증이다.

## 배포 및 검증 순서

1. LASAL IDE에서 current canonical project를 C78 ARM으로 clean rebuild한다.
2. build 결과를 올바른 PLC에 download하고 project restart를 확인한다.
3. WPF/API를 다시 build한다.
4. `Refresh Home/Encoder Maintenance Capabilities` 로그에서 exact
   `DiagnosticsBits=0x000C633F`, nonzero BootId, `MapRevision=0x957F101E`, `TW20=True`,
   `TW19=True`를 확인한다.
5. 실제 write 전에는 선택 축 power-off/standstill과 현재 encoder 상태를 기록한다.
6. 초기 retained word `0x5242530F`에서 Axis/Group motion은 차단되고 safety와 exact LMC Home은
   허용되는지 확인한다.
7. 축별 exact LMC Home 성공 뒤 해당 bit만 clear되는지 확인하고, 4축 완료 뒤 exact
   `0x524253F0`을 확인한다.
8. TW20은 active error/warning 전후를, TW19는 multi-turn position 전후를 독립적으로 확인한다.
9. TW19에서 SDO 전 해당 축 bit가 먼저 set되고 이후 PowerOn/motion이 차단되는지 확인한다.
10. 같은 축 exact LMC Home COMPLETE 성공 뒤에만 motion admission이 다시 열리는지 확인한다.
11. PLC restart와 target power loss 뒤 마지막 encoded word가 유지되는지 확인한다.

Source/static/build 성공은 PLC download 또는 실축 효과 증거가 아니다.

## LMCDiagnosticsService method-size split 설계와 증거

LASAL C78의 per-method `32768`-byte ceiling을 넘지 않도록 diagnostics cyclic implementation을
세 private helper로 분리했다.

- `ProcessEncoderMaintenance`의 safety-preemption 처리:
  `HandleEncoderMaintenancePreemption`
- `ProcessAxisDs402Home`의 retained receipt/recovery 처리:
  `HandleAxisDs402HomeReceiptStages`
- `ProcessAxisDs402Home`의 stage `90..101` cleanup 처리:
  `HandleAxisDs402HomeCleanupStages`

helper adapter는 기존 stage sample과 client 호출 순서를 유지한다. Encoder와 receipt helper는
`Result : DINT`를 통해 moved `RETURN`을 caller에 전달하고, cleanup helper 호출은 caller의 마지막
실행문이므로 helper return 뒤 곧바로 원 `END_FUNCTION` 경계로 끝난다. Encoder snapshot은
`^USINT` byte pointer와 기존 byte offset/type cast를 그대로 사용한다. cleanup의
`InitialCurrentCycle`은 caller가 이미 계산한 cycle을 새로 표본하지 않고 전달하기 위한 input이다.

current tracked external source의 정적 identity와 size evidence는 다음과 같다.

- `LMCDiagnosticsService.st` all-CRLF source: `266206` bytes
- source SHA-256:
  `348E45AD486B4072D0105E7C0800B31BAF30A0B908F8AD2A5D2C26D3E46496E8`
- function inventory `25`개 모두 all-CRLF `32768` bytes 미만
- 최대 method: `ProcessAxisDs402Home`, all-CRLF `30376` bytes
- 세 helper를 제거하고 body를 원 위치에 다시 inline한 canonical reverse transform:
  all-CRLF `260860` bytes, SHA-256
  `1F9CC2DB681BB16A1D347A1D0A1FB45A016DA8C92B53CE5DBB04C74F40BA74AC`

현재 세 helper implementation은 source에 있지만 LASAL generated declaration과 `Classes.lcb`
metadata는 아직 생성되지 않았다. 따라서 IDE handoff Section 17.3의 exact private ABI를 생성하고
Save All, IDE 종료, external inspection을 마치기 전에는 **C78 Rebuild를 수행하지 않는다**.
이 split은 Comm Network 또는 다른 Network connection 변경을 요구하지 않는다.

current split source의 정적 verifier 결과는 다음과 같다.

- diagnostics method-size split negative mutation `23/23` reject
- TW19 retained Home barrier negative mutation `37/37` reject
- encoder-maintenance negative mutation `56/56` reject
- ownership activation/repeated-safety negative mutation `247/247` reject
- DS402 Home retirement negative mutation `50/50` reject
- 다섯 pre-IDE declaration waiver를 사용한 full `-SourceOnly` PASS
- diagnostics split waiver만 제거하면 첫 private helper IDE declaration 누락에서 exact FAIL

method-size 정적 증거와 SourceOnly PASS는 C78 build/download 또는 PLC runtime 성공 증거가 아니다.

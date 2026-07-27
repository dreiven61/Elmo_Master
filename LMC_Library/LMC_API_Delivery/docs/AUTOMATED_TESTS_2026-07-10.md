# LASAL Motion Control API Automated Tests

작성일: 2026-07-10

최종 결과 재확인: 2026-07-27

## 구성

외부 NuGet package가 없는 .NET Framework 4.8 console runner다.

경로:
`LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests`

실패가 하나라도 있으면 process exit code `1`을 반환한다.

## 범위

- 모든 현재 request builder의 literal golden bytes
- caller DINT가 DLL에서 재스케일되지 않는지 확인
- common envelope와 malformed/truncated/trailing frame
- exact 4-byte/8-byte ACK
- name lookup reference offset
- typed Axis ReadStatus, DINT ReadActualPosition, GroupReadStatus
- captured PMAS ReadStatus/GroupReadStatus raw golden for envelope/offset
  compatibility; canonical LASAL state-bit semantics are documented separately
- exact 1350-byte `0x20D2`, count 0..16, name/array defensive copy
- `0x2051` None/ACS coordinate request, MCS/PCS/unknown enum 거부와 exact 68-byte
  LASAL-DINT `DINT[16]+status/error` typed result, slot 1..9/10..16 zero 및 배열 defensive copy
- legacy `0x2051` 136-byte LREAL response와 malformed/trailing payload 거부
- `0x20E7` exact 1320-byte Cartesian4 payload, X/Y/Z/U axis reference와
  captured application-frame SHA-256 golden
- group position 1..16 길이와 slot 5..16 nonzero 거부, group coordinate/transition/
  buffer/execute whitelist 및 velocity/acceleration/deceleration/jerk validation
- GroupStop deceleration/jerk validation과 LASAL `StopCmdNo` 비오류 계약
- RPC init, fragmented response, 실제 ephemeral UDP callback, close
- callback payload defensive copy와 controller IP가 아닌 UDP source 거부
- init status/shape, callback ACK status/shape와 truncated-response 실패 후
  socket/listener state cleanup
- options clone/timeout validation과 invalid reconnect 시 기존 session 유지
- close nonzero ACK 예외, response/error 보존과 local cleanup
- receive timeout 뒤 transport 폐기, `Faulted` 전이와 재사용 차단
- queued cancellation이 active RPC를 보존하고 in-flight cancellation은
  해당 transport만 폐기하는지 검증
- async init/close와 취소 가능한 axis/group factory 성공, reconnect 뒤 stale
  group handle 및 generation-bound exchange 거부
- axis lookup 뒤 AxisInfo success/malformed/command-error
- LASAL static contract: generated client count/entries, 9-axis network links,
  C#-ST critical offsets, 32-bit error truncation guards, legacy command block,
  `_JERK_PROFILE`/nonzero JMax와 Stop/Move Jerk 수신·전달 경로
- diagnostics D0 capability와 D1 Health/Catalog/PI Read, D2 Bulk, D3 single-bank
  Recorder request/parser 및 source contract
- 서로 다른 두 RPC connection session에서 Recorder exact/0/0 discovery Adopt,
  preserved identity, 새 OwnerSessionEpoch, Status metadata 복구, immutable download와
  adopted identity 기반 buffer/configuration Release 순서
- D4 single-bank Ring/Edge/Window/Mask/forced Trigger와 D5 general-inline SDO Read
  request/status/queued-cancel active LASAL source contract; D4 Double, D5 Write와
  extended result fail-closed contract
- Phase 1 Admin `0x7D00/0x7D10/0x7D20` golden/parser/fake-RPC, semantic key/mask,
  RequestId/session/capability와 LASAL source offset/method mapping
- Phase 2 Admin `0x7D22 GroupMoveLinearRelative` exact 104-byte golden,
  4-axis/parameter whitelist, strict ACK/native reject parser, sync/async fake-RPC,
  capability no-dispatch, stale generation과 LASAL `MoveRelativeCoord` state gate
- `GetDriveOperationMode`/`ReadDriveStatus`의 physical axis 1..4, terminal
  success/failure, `TimeoutCycles+32` bounded poll과 ticket-preserving cancellation
- PI alias와 Bulk builder/reader의 exact MapRevision, entry validation, latest
  snapshot lookup, stale session/release 및 PC-local error domain catalog
- internal `negative-wire` 모드의 live 확인 gate, 고정 5개 diagnostics raw scenario,
  arbitrary/motion/Admin/write/SDO Submit/Recorder 차단과 exact 16-byte 오류 envelope 9개 계약
- D5 SDO abort terminal과 same-Boot known-valid recovery 순수 판정 12개: 기존 8개에
  generic exact recovery의 UInt32 성공과 type/value/length 거절 4개를 추가했다. abort는
  `Failed/Failed`, `OperationErrorId=-32000`, nonzero raw abort code, result 없음이며
  recovery는 새 ticket의 `0x6061:0 Int8/1` exact baseline 값 성공
- WPF build 범위에는 Submit 전 outcome guard/unknown-ticket quarantine, same-connection
  BootId change, exact `BootIdMismatch`와 stale local session quarantine이 포함된다. 같은
  Boot/session의 exact `TicketNotFound`는 terminal-slot 교체 계약상 이전 ticket terminal만
  증명하고 outcome `UNKNOWN`으로 해제한다. multi-evidence recovery는 GeneralInline이면
  서로 다른 두 `0x6061:0 Int8/1`, legacy SDORead-only이면 서로 다른 두
  `0x1000:0 UInt32/4` ticket의 exact type/length/bytes를 검증한다. unresolved Group Disable
  포함 새 mutation gate와 15~120초 deadline-aware cleanup도 포함된다.
  기존 Bulk/Recorder/queued-ticket cleanup, Stop/PowerOff와 read-only는 계속 허용한다.
  proof scope는 `same_owner_connection_recovery`, `new_diagnostics_boot_session`,
  `new_connection_session`으로 나뉘며,
  모든 evidence owner가 바뀐 셋째 scope는 `newConnectionRecovery=true`만 뜻한다.
  WPF는 `orphanQualified=false`를 고정 기록하며 실제 orphan PASS에는 known Running old
  ticket, 실제 owner loss와 별도 PLC hook/capture가 필요하다. `D5SdoPendingCleanup`
  Resolve는 기존 qualification log에 `D5_LOG_CONTINUATION`을 이어 써 원래
  `FAIL`/`OUTCOME_UNCERTAIN`을 보존한다. 이 항목은
  analyzer 12개의 순수 상태 판정 범위를 넘어서는 UI/runtime contract이며 PLC live 증거가 아니다.
- Phase 1 facade의 예외는 pre-submit/status stage와 ticket을 항상 노출하지 않아 false
  quarantine이 가능하지만 WPF는 fail-closed한다. SDK stage+ticket exception UX는 후속
  부채다. PI Write는 SDK compile-time allowlist empty와 WPF button/handler 이중 차단을
  적용한다.

PMAS legacy `0x202E` LREAL 16-byte와 `0x2051` LREAL 136-byte response는
LASAL-DINT typed parser가 명시적으로 거부한다. DINT actual-position
golden은 PLC 재캡처 전까지 contract 기반 synthetic vector다.

## 실행

PC C# test만 실행:

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' `
  /t:RunPcTests /p:Configuration=Release /nologo
```

LASAL source static contract만 실행:

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' `
  /t:RunLasalContract /p:Configuration=Release /nologo
```

PC C# test, LASAL source static contract와 현재 WPF example build를 순서대로
실행하려면 target을 `/t:RunTests`로 바꾼다. 제거된 legacy
`LasalMotionControlLibTestApp`은 이 target에 포함하지 않는다.

현재 결과:

- `RunPcTests`: Debug/Release 각 `223/223 PASS`
  (response hard limit/AxisInfo, read-only qualification 분석·CSV 6개와
  callback exception/reentrant shutdown loopback 4개, Group Stop-first 정상/fallback/
  aggregate/UI context 4개와 Recorder two-session exact/discovery, pre-close transport-fault
  recovery, Fault no-mutation, cancel/Stop-race/release retry/quarantine 및 cleanup
  state/route/manual-recovery policy, Bulk cancel/release retry와 one-slave-partial 순수 판정,
  internal negative-wire 9개와 D5 abort/recovery analyzer 12개 포함)
- `RunLasalContract`:
  `PASS LASAL.StaticContract.SourceOnly` (Admin read와 `0x7D22`, 9축, CyWork-only, D1~D3와 D4
  single-bank Ring/Trigger 및 D5 general-inline SDO Read active source,
  D4 Double/D5 Write·extended fail-closed wire)
- `RunLasalNetworkContract`: `PASS LASAL.StaticContract`; `Classes.lcb` general
  `TryStartRead` declaration, 4축 executor network와 generated metadata 포함
- `BuildSimpleExampleApp`: D5 runner 포함 `LMC_Library/LasalApiWpfTestApp`
  Debug/Release build PASS. 각 3초 startup smoke는 기존 Group/Bulk/Recorder panel까지 PASS
- `BuildDistributionExampleApp`: binary-reference distribution example build PASS
- full distribution preview pipeline: temporary standalone example Debug/Release build,
  forbidden internal-reference scan, cleanup과 DLL hash identity PASS

target을 분리했기 때문에 PC C# 실패와 LASAL static source contract 실패를
구분할 수 있다. 자동 테스트 통과는 serializer/parser/connection lifecycle와
source contract 검증이며 LASAL IDE compile, PLC download와 실제
EtherCAT/motion 동작 검증을 대체하지 않는다.

현재 단계 구분:

| 단계 | 자동/정적 계약 상태 | 실제 PLC 상태 |
|---|---|---|
| D0 | 구현 및 test profile `CapabilityBits=0x0000213F` 계약 테스트 포함 | `11_PI_Bulk_Regression`에서 BootId 8의 동일 capability 응답 7회 PASS |
| D1~D2 | active source, cleanup orchestration과 one-slave-partial 순수 판정 테스트 포함 | `11_PI_Bulk_Regression`의 Catalog/PI/Bulk happy path PASS; 100회 soak와 operator partial live/capture 대기 |
| D3 | active source와 PC contract 테스트 포함 | 기존 Recorder happy path 캡처 존재; trigger/fault/soak는 별도 gate |
| D4 | single-bank Ring/Trigger active contract 포함 | runtime 미실시, Double 미구현 |
| D5 | general-inline Read submit/status/cancel, executor release/race와 abort/recovery analyzer 12개 포함 | `10`/`12` happy path와 TypeMismatch recovery live PASS. WPF outcome quarantine/capability별 two-ticket recovery proof/state-change gate/deadline-aware cleanup은 code/build만 완료; abort/orphan/pcap과 offline/timeout/cancel/contention live 대기 |
| Phase 1 facade | typed drive read, PI/Bulk builder/reader와 error catalog PC contract 포함 | `10_DriveRead_Axis1to4`, `11_PI_Bulk_Regression` happy path PASS |
| Admin/group | `0x7D00/10/20/22` C# golden/parser/fake-RPC와 LASAL SourceOnly mapping 포함 | `01`~`08c`, `04b`, `09b`에서 happy path/dynamic monitor/PowerOff/None-ACS static alias 검증; `0x2047` accepted-then-poll 수정본, queue/race/fault gate 잔존 |

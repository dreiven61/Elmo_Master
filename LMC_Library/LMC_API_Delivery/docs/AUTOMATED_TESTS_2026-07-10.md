# LASAL Motion Control API Automated Tests

작성일: 2026-07-10

최종 결과 재확인: 2026-07-22

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
- D4 single-bank Ring/Edge/Window/Mask/forced Trigger와 D5 general-inline SDO Read
  request/status/queued-cancel active LASAL source contract; D4 Double, D5 Write와
  extended result fail-closed contract
- Phase 1 Admin `0x7D00/0x7D10/0x7D20` golden/parser/fake-RPC, semantic key/mask,
  RequestId/session/capability와 LASAL source offset/method mapping
- `GetDriveOperationMode`/`ReadDriveStatus`의 physical axis 1..4, terminal
  success/failure, `TimeoutCycles+32` bounded poll과 ticket-preserving cancellation
- PI alias와 Bulk builder/reader의 exact MapRevision, entry validation, latest
  snapshot lookup, stale session/release 및 PC-local error domain catalog

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

- `RunPcTests`: Debug/Release 각 `135/135 PASS`
- `RunLasalContract`:
  `PASS LASAL.StaticContract.SourceOnly` (9축, CyWork-only, D1~D3와 D4
  single-bank Ring/Trigger 및 D5 general-inline SDO Read active source,
  D4 Double/D5 Write·extended fail-closed wire)
- `RunLasalNetworkContract`: `PASS LASAL.StaticContract`; `Classes.lcb` general
  `TryStartRead` declaration, 4축 executor network와 generated metadata 포함
- `BuildSimpleExampleApp`: `LMC_Library/LasalApiWpfTestApp` Debug/Release build와
  각 3초 startup smoke PASS
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
| D0 | 구현 및 test profile `CapabilityBits=0x0000213F` 계약 테스트 포함 | D1~D5와 함께 실기 검증 대기 |
| D1~D3 | active source와 PC contract 테스트 포함 | end-to-end 미실시 |
| D4 | single-bank Ring/Trigger active contract 포함 | runtime 미실시, Double 미구현 |
| D5 | general-inline Read submit/status/cancel 및 executor release/race 계약 포함 | legacy 축 1~4와 general-inline 1/2/4-byte 사용자 실기 PASS; 최종 확인 신규 pcap/log와 fault matrix 없음 |
| Phase 1 facade | typed drive read, PI/Bulk builder/reader와 error catalog PC contract 포함 | 기존 D5 SDO runtime 외 신규 Admin/facade E2E는 미실시 |
| Admin | `0x7D00/10/20` C# golden/parser/fake-RPC와 LASAL SourceOnly mapping 포함 | LASAL IDE build/download와 실물 값/UNIT/packet 미검증 |

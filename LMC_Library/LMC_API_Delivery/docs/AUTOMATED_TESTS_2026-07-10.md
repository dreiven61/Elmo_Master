# LASAL Motion Control API Automated Tests

작성일: 2026-07-10

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
- `0x2051` coordinate request와 exact 68-byte LASAL-DINT
  `DINT[16]+status/error` typed result, 배열 defensive copy
- legacy `0x2051` 136-byte LREAL response와 malformed/trailing payload 거부
- `0x20E7` exact 1320-byte Cartesian4 payload, X/Y/Z/U axis reference와
  captured application-frame SHA-256 golden
- group position 1..16 길이, enum/options validation
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

- `RunPcTests`: `46/46 PASS`
- `RunLasalContract`:
  `PASS LASAL.StaticContract (CyWork-only active command contract and ordinary TCP server network)`
- `BuildSimpleExampleApp`: `LMC_Library/LasalApiWpfTestApp` build PASS

target을 분리했기 때문에 PC C# 실패와 LASAL static source contract 실패를
구분할 수 있다. 자동 테스트 통과는 serializer/parser/connection lifecycle와
source contract 검증이며 LASAL IDE compile, PLC download와 실제
EtherCAT/motion 동작 검증을 대체하지 않는다.

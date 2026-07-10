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
- RPC init, fragmented response, 실제 ephemeral UDP callback, close
- init status/shape, callback ACK status/shape와 truncated-response 실패 후
  socket/listener state cleanup
- axis lookup 뒤 AxisInfo success/malformed/command-error
- LASAL static contract: generated client count/entries, 4-axis network links,
  C#-ST critical offsets, 32-bit error truncation guards, legacy command block

PMAS legacy `0x202E` LREAL 16-byte response는 LASAL-DINT typed parser가
명시적으로 거부한다. DINT actual-position golden은 PLC 재캡처 전까지
contract 기반 synthetic vector다.

## 실행

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' `
  /t:RunTests /p:Configuration=Release /nologo
```

30개 C# case 뒤 LASAL static contract suite도 같은 `RunTests` target에서
실행된다. 자동 테스트 통과는 source contract 검증이며 LASAL IDE compile,
PLC download와 실제 EtherCAT/motion 동작 검증을 대체하지 않는다.

# LASAL Motion Control Lib 실기 테스트 앱

이 프로그램은 Elmo DLL을 사용하지 않고 현재 저장소의
`LMC_API_Delivery/src/LasalMotionControlLib.csproj`를 직접 참조한다. 따라서
`bin/Release`를 다시 빌드하면 현재 PC API 소스가 바로 반영된다.

단, PC 자동 시험과 LASAL 정적 계약 시험을 통과했다는 사실은 PLC 실기 동작
승인을 뜻하지 않는다. 이 앱은 그 실기 확인을 안전하게 진행하기 위한 도구다.

## 현재 API 반영 상태

PC public 호출 경로와 request/response 구현은 캡처 기준 command ID 23개를
모두 포함한다.

| 구분 | command ID | 현재 상태 |
|---|---|---|
| RPC init/callback/close | `0x8080`, `0x405C`, `0x405D` | PLC source active |
| axis lookup/info | `0x103C`, `0x202B` | PLC source active |
| single axis | `0x2023`, `0x2024`, `0x2022`, `0x2028`, `0x202E`, `0x209F`, `0x20A0`, `0x20A2` | PLC source active |
| group lookup | `0x1042` | PLC source active |
| group | `0x20D2`, `0x2047`, `0x2048`, `0x2049`, `0x2085`, `0x2045`, `0x2051`, `0x20A4`, `0x20E7` | 4개 active, 5개는 아래와 같이 차단 |

현재 PLC가 명시적으로 `ErrorId=-5`를 반환하는 명령은 다음 5개다.

- `GroupReset` (`0x2049`)
- `GroupStop` (`0x2085`)
- `GroupReadActualPosition` (`0x2051`)
- `MoveLinearAbsoluteEx` (`0x20A4`)
- `SetKinTransformCartesian4Axis` (`0x20E7`)

기본 화면에서는 이 5개를 전송할 수 없다. `Allow unsupported -5 negative
test`를 켠 경우에만 전송하며, 유효한 응답의 `ErrorId`가 정확히 `-5`일 때만
PASS로 기록한다.

`ReadMemberStatus`, `PowerOnMembers`, `PowerOffMembers`는 별도 PLC group command가
아니라 member 이름별 axis lookup과 single-axis API를 조합한 PC workflow다.
`PowerOnMembers` 도중 한 축이 실패하면 PowerOn 송신을 시작했을 가능성이 있는
현재 축까지 rollback 후보에 먼저 포함하고 역순 PowerOff를 시도한다. 각 축은
PowerOff와 standstill을 모두 확인해야 rollback 성공으로 기록한다. `UNCERTAIN` 또는
`INCOMPLETE`가 기록되면 소프트웨어 상태를 믿지 말고 물리 안전 정지 후 확인한다.

## 현재 프로젝트 기본값

- axis: `_LMCAxis1`부터 `_LMCAxis4`
- group: `_LMCRobotBase1`
- Position/Velocity/Acceleration/Deceleration UNIT multiplier: `10000`
- Jerk: `0`

`10000`은 현재 4개 회전축의 degree profile에만 맞는 기본값이다. API DLL은
UNIT을 자동 변환하지 않는다. 다른 축이나 서로 다른 UNIT을 가진 group을 시험할
때는 PLC 설정을 확인하고 각 값에 맞는 UNIT을 호출자가 곱해야 한다. 이 테스트
앱의 group 입력은 UNIT multiplier 하나를 공유하므로 혼합 UNIT group motion에는
사용하지 않는다.

## 실기 시험 전 안전 조건

1. 물리 E-stop이 실제로 동작하는지 확인한다.
2. 기구부 이동 범위, 소프트웨어 limit, 작업자 접근 금지를 확인한다.
3. Position/Velocity를 화면 기본값처럼 작은 값으로 시작한다.
4. Jerk는 승인 전까지 `0`을 유지한다.
5. PowerOn, Reset, group state 변경과 motion 명령은 `Arm one
   power/motion/state command`를 켜고 경고창에서 다시 확인해야 한다. 한 명령을
   실행하려는 순간 즉시 disarm된다. API, target mode, axis/group/member 이름을
   바꿔도 즉시 disarm되므로 변경 후 다시 arm해야 한다.

`Cancel (outcome unknown)`은 PC transport/wait를 중단하며 PLC Stop을 보내지
않는다. 명령이 이미 송신됐다면 PLC 결과는 알 수 없다. 상태 변경, power, motion,
Stop/PowerOff와 rollback workflow 중에는 Cancel과 창 닫기를 막는다.
`CloseConnection`과 창 닫기도 Stop이 아니다. `MoveVelocityEx`는 송신/응답 대기
전에 해당 축을 `UNCERTAIN`으로 추적한다. 유효한 거부 응답을 받거나 `Stop` 또는
`PowerOff` 후 standstill까지 확인해야만 경고가 해제된다. 통신이 끊긴 상태라면
물리 안전 정지나 drive disable을 먼저 사용한다.

## 권장 시험 순서

1. LASAL IDE에서 canonical 프로젝트를 Rebuild하고 PLC에 올린다.
2. 앱을 Release로 Rebuild한 뒤 `bin/Release/LasalMotionControlLibTestApp.exe`를
   실행한다.
3. `RpcInitConnection` 후 TCP state와 UDP callback listening 상태를 확인한다.
4. 먼저 `ReadStatus`, `ReadPosition`, `GetMembers`, `ReadMemberStatus`,
   `GroupReadStatus`만 시험한다.
5. 한 축씩 `PowerOn`을 시험하고 상태를 확인한다.
6. 낮은 값으로 `MoveRelativeEx`를 먼저 시험하고 `Stop`, `PowerOff`를 확인한다.
7. `MoveAbsoluteEx`, 마지막에 `MoveVelocityEx`를 시험한다.
8. group enable/member power workflow는 single-axis 검증이 끝난 뒤 시험한다.
9. unsupported 5개는 동작 시험이 아니라 `-5` negative protocol 시험으로만
   실행한다.

고급 timeout, callback source 검증, event mask 변경은 `LMCConnectionOptions`를
사용하는 API consumer 기능이다. 이 화면은 기본 event mask와 기본 connection
option만 사용한다.

## 자동 검증

다음 target은 PC 42개 시험, LASAL source contract, 이 WPF 앱 빌드를 함께
실행한다.

```powershell
MSBuild.exe LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj /t:RunTests /p:Configuration=Release /p:Platform=AnyCPU
```

네트워크 파일까지 포함한 strict contract는 별도로 실행한다.

```powershell
powershell -File LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1 -RepositoryRoot .
```

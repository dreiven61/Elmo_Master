  → SendData(TRUE)
```

`CyWork()`는 비어 있고 `RtWork()`는 object registry만 만듭니다. 따라서 현재 구조로 실제 장비 모션을 승인하면 안 됩니다.

2. `TCPMotionInterface1`의 task 배치가 확인되지 않았습니다.

generated task 목록에 `TCPMotionInterface1` object가 보이지 않습니다. 따라서 현재 `RtWork()`가 실제 호출되어 `ObjectRegistryReady`가 만들어지는지도 정적으로 보장되지 않습니다.

Queue 설계뿐 아니라 다음을 IDE에서 명시해야 합니다.

- `TCPMotionInterface1.CyWork` 실행 task
- `TCPMotionInterface1.RtWork` 실행 core/timeslot
- `_LMCAxis1..4`, `_LMCRobotBase1`과 동일 core 조건
- object registry 초기화 위치

3. TCP 실행 owner가 중복될 수 있습니다.

[_TCPIPServer_RT.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_TCPIPServer_RT/_TCPIPServer_RT.st:167)는 `RtWork()`에서 `CyclicCall()`을 호출하고, `CyWork()`도 조건에 따라 호출합니다. 한쪽만 TCP transport owner가 되도록 확정해야 합니다.

4. server 설정과 단일-session 구현이 맞지 않습니다.

현재 `MaxConnections=0`은 내부적으로 16개 연결을 의미하지만, `ReceiveBuf`, `RpcSocket`, callback endpoint는 하나뿐입니다. P0는 반드시 `MaxConnections=1`로 제한해야 합니다.

또 `_TCPIPServer1`과 `_TCPIPServer_RT1`이 모두 port 4000 설정을 갖고 있습니다. 현재는 RT object만 task에 들어간 것으로 보이지만, 사용하지 않는 server의 제거·비활성 정책도 정해야 합니다.

5. Group command는 아직 열면 안 됩니다.

현재 source는 `RobotOn`, `RobotOff`, `MoveLinearCoord`를 직접 호출합니다. 그러나 group mode, kinematic, UNIT profile이 확정되지 않았습니다.

특히 현재 미커밋 변경에서 [GroupMoveRetCode](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1460)가 `_LMCPROF_NoError`로 초기화돼 있습니다. 잘못된 `0x20A4` payload나 `Execute=0`에서도 success ACK가 나갈 수 있는 회귀입니다. 이 상태는 커밋하면 안 됩니다.

6. 현재 canonical 작업 트리가 깨끗하지 않습니다.

LASAL/IDE 관련 파일들이 미커밋 상태이고 전체 `git diff --check`도 다음 trailing whitespace로 실패합니다.

- `ECAT_Master_Base.st`: 5050~5051행
- `TCPMotionInterface.st`: 286, 312~314행

새 queue 구현 전에 현재 IDE 생성 변경을 별도 checkpoint로 확정해야 합니다.

## 추가로 설계해야 할 내용

### 1. D4/D10 task/core 설계

이게 가장 먼저 해결해야 할 기술 blocker입니다.

현재 문서는 `Config=1 AP async task`를 권장하지만, AP task와 RT task 사이의 atomic/memory ordering 근거가 없습니다.

제 권장안은 단계적으로 가는 것입니다.

- 첫 `0x202E` 검증은 `Config=0/CyWork`를 TCP owner로 사용
- `_TCPIPServer_RT::RtWork()`의 `CyclicCall()`은 제거
- `Response`와 request FIFO consumer를 같은 cyclic context에 둠
- CyWork → RtWork 사이의 one-slot mailbox만 먼저 검증
- LASAL이 지원하는 RT-safe atomic/release-acquire 방법이 확인된 뒤 `Config=1`로 전환 검토

이렇게 하면 첫 단계에서 AP task까지 포함한 동시성 문제를 한꺼번에 풀지 않아도 됩니다. 단, CyWork와 RtWork 사이 mailbox의 memory visibility 근거는 여전히 확인해야 합니다.

### 2. IDE에 입력할 실제 type/member 설계

현재 [Queue/RtWork 설계안](./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md:241)은 의미만 있고 실제 IDE model 명세가 부족합니다.

다음을 필드 단위로 확정해야 합니다.

- `RequestEntry[8]`
- FIFO head/tail/count/sequence
- `RtRequestMailbox`
- `RtResultMailbox`
- `IngressFaultMailbox`
- session epoch
- active request storage
- oversize drain/discard 상태
- producer/consumer와 task 소유권
- state transition: `FREE → WRITING → READY → ACTIVE → FREE`

### 3. D1~D15 승인

현재 공식 문서에는 전부 미승인으로 남아 있습니다. 다음은 현재 권장안 그대로 승인해도 됩니다.

- depth-8 FIFO
- RX accumulator 1024
- TX buffer 4096
- P0 `MaxConnections=1`
- strict FIFO
- success ACK는 motion 완료가 아니라 accepted/started
- 첫 검증 command는 `0x202E`
- group state-changing command는 D14 전 차단
- Stop은 safety-rated emergency stop이 아님

추가 확인이 필요한 것은 D4, D10, D12, D14, D15입니다.

### 4. 기능별 후속 설계

`0x2051 GroupReadActualPosition`

- LASAL 후보 method는 `LMCRobot.GetRobotPosition()`
- LASAL `_LMCPROF_POS`는 9 DINT지만 PC 응답은 16 DINT
- ACS/MCS/PCS와 LASAL Mode/CoordSystem 매핑 필요
- IncludeTool 여부와 남은 슬롯 zero-padding 정책 필요

첫 버전은 검증 가능한 ACS만 열고 MCS/PCS는 `-7`로 거부하는 것이 안전합니다.

`0x20E7 SetKinTransformCartesian4Axis`

- 현재 전체 frame 1328바이트가 1024-byte receive buffer보다 큼
- 일반 FIFO의 96-byte payload에도 들어가지 않음
- `_LMCRobotBase`에 직접 대응하는 public `SetKinTransform` method가 없음
- CalcModel도 실제 Network에 연결되지 않음

따라서 당장은 `-5 unsupported`를 유지하고, 별도 1320-byte staging과 kinematic adapter를 설계해야 합니다.

`GroupStop`

- 후보는 `LMCRobot.StopMove(Mode:=3, Decel, Jerk)`
- profile buffer까지 비우므로 PC의 `Aborting`과 가장 유사
- same-core 및 실제 감속 동작 검증 후 활성화

`GroupReset`

- `LMCRobot.AxQuitError(AxisNo:=0)`가 후보
- axis/HW error 외 robot/profile error까지 해제하는지는 미확인
- 실제 상태 전이 검증 전에는 `-5` 유지

Callback

- PMAS 호환을 원하면 실제 UDP datagram 캡처 필요
- 독자 규격이면 `LASAL Callback v1`을 별도로 선언해야 함
- version, event type, length, session epoch, target, sequence, timestamp, status/error 정의 필요
- UDP는 best-effort, 최종 상태 판단은 polling 유지
- sender는 non-RT, RtWork는 event mailbox만 기록

Multi-PC는 P1 이후로 미루는 것이 맞습니다.

## 권장 개발 순서

1. `_Edit`을 `.gitignore` 처리하거나 저장소 밖으로 이동
2. 현재 canonical IDE/generated 변경을 검토하고 checkpoint로 고정
3. D4/D10 task/core 구조 확정
4. D0~D15 P0 정책 승인
5. IDE type/member/channel/task 배치표 작성
6. CodeGenerator 재생성, Build, Find smoke test
7. `0x202E` one-slot read-only 경로 구현
8. depth-8 FIFO와 RPC/lookup/read/admin 이관
9. `0x2051` read-only 구현
10. Power와 Axis Stop
11. 짧은 거리·저속 Axis Move
12. GroupStop/Reset 의미 검증
13. Group enable/linear motion
14. `0x20E7`
15. Callback sender
16. Multi-PC ownership
17. 모든 단계별 PLC packet 재캡처

따라서 다음 실제 작업은 모션 코드를 추가하는 것이 아니라, 현재 변경분을 checkpoint로 정리한 뒤 D4/D10과 IDE model 설계를 확정하는 것입니다. 이번 브리핑에서는 파일을 수정하지 않았습니다.

> LMCAxis를 LMCAxis1로 명명 변경하고 실제 동작 하는 내용을 구현 진행하자. 소스코드 구현부터 진행하고 네트워크 연결은 필요하게 된다면 설계만 해놓도록해.

<details><summary>14 previous messages</summary>

> `_Edit`은 제외하고 canonical `Elmo_EtherCAT_Test_4Axis`만 수정하겠습니다. 이번 단계에서는 `LMCAxis → LMCAxis1` 소스 명칭 변경과 `Response → queue → CyWork → RtWork` 실행 경로를 구현하고, `.lcn` 네트워크는 수정하지 않은 채 필요한 IDE/연결 변경표만 문서화하겠습니다. 기존 미커밋 LASAL 변경은 보존하면서 겹치는 부분을 먼저 확인하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 정식 프로젝트 `Elmo_EtherCAT_Test_4Axis`만 대상으로 진행하겠습니다. 이번 단계에서는 소스에서 `LMCAxis1` 명칭과 안전한 실행 경로를 구현하고, 실제 LASAL 네트워크 연결·태스크 배치는 문서 설계로만 남깁니다. 기존 사용자/IDE 변경분은 건드리지 않겠습니다.
>
> 구현 범위를 안전하게 고정했습니다. TCP 수신 콜백에서는 더 이상 모션 객체를 직접 호출하지 않고, 8개 요청 큐에 게시합니다. 우선 `0x202E(ReadActualPosition)`만 RT mailbox로 실제 실행하고, 아직 RT 이식하지 않은 축/그룹 제어 명령은 오동작 대신 `-5`로 명시 차단하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+97 -17)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+410 -102)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+120 -65)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+11 -11)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_TCPIPServer_RT/_TCPIPServer_RT.st` (+1 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+45 -9)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+8 -7)
>
> </details>
>
> 핵심 소스 경로는 반영됐습니다. `LMCAxis1` 클래스 테이블 해시까지 변경했고, RT 쪽은 문자열 처리와 TCP 처리를 제거한 채 축 실제 위치 읽기만 수행하도록 분리했습니다. 이제 정적 계약 검사와 LASAL 문법/동시성 위험을 집중 점검하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+5 -5)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_SOURCE_QUEUE_AND_NETWORK_APPLY_PLAN_2026-07-10.md` (+7 -2)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+11 -6)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+33 -25)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+10 -2)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+4 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+21 -15)
> - Wrote `./docs/architecture/MotionTCPDemo_vs_Elmo_EtherCAT_Test_4Axis_Analysis_2026-07-03.md` (+8 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+16 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+2 -2)
>
> </details>
>
> 검토 중 두 가지 경쟁 조건을 확인했습니다. 세션 epoch는 atomic 증가 후 재대입하지 않도록 고치고, 응답은 같은 CyWork에서 동일한 direct TX 순서를 사용해 뒤 요청이 먼저 나가는 일을 막겠습니다. 큰 프레임은 연결을 영구 차단하지 않고 길이만큼 제한적으로 폐기한 뒤 다음 프레임 경계를 회복하도록 보완합니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+3 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+4 -3)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+14 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+33 -7)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+2 -2)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+12 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+7 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_SOURCE_QUEUE_AND_NETWORK_APPLY_PLAN_2026-07-10.md` (+18 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+15 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+4 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+13 -12)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+6 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_SOURCE_QUEUE_AND_NETWORK_APPLY_PLAN_2026-07-10.md` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+21 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+67 -5)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+6 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_SOURCE_QUEUE_AND_NETWORK_APPLY_PLAN_2026-07-10.md` (+9 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+14 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+2 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+1 -3)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_SOURCE_QUEUE_AND_NETWORK_APPLY_PLAN_2026-07-10.md` (+3 -1)
>
> </details>
>
> 소스와 설계 재검토까지 끝났고 추가 수정 사항은 없습니다. 마지막으로 전체 PC 42개 테스트, source-only LASAL 계약, diff/ASCII 검사를 다시 한 번 묶어서 확인한 뒤 현재 완료/대기 경계를 정리하겠습니다.

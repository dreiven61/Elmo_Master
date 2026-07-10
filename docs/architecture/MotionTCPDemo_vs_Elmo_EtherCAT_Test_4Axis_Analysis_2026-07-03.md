# MotionTCPDemo vs Elmo_EtherCAT_Test_4Axis 구조 분석

> 2026-07-10 상태 갱신: 아래의 `Response()` 직접 parser/motion call과
> `_TCPIPServer_RT::RtWork()`의 `CyclicCall()` 설명은 source-first migration 전
> 기준선이다. canonical source는 현재 `LMCAxis1`, depth-8 request queue,
> `CyWork` coordinator와 typed `0x202E` `RtWork` mailbox로 변경됐다. 실제
> IDE/network/PLC 적용 상태는
> `LMC_Library/LMC_API_Delivery/docs/LASAL_SOURCE_QUEUE_AND_NETWORK_APPLY_PLAN_2026-07-10.md`
> 를 따른다.

작성일: 2026-07-03

## 범위

이 문서는 Jonas Draeger 메일에서 설명한 SIGMATEK 권장 구조를 기준으로 `MotionTCPDemo` 프로젝트를 분석하고, 현재 우리가 만든 `Elmo_EtherCAT_Test_4Axis` 프로젝트와 구조 차이를 비교한 결과다.

분석 대상:

- `MotionTCPDemo/Class/**/*.st`
- `MotionTCPDemo/Network/**/*.lcn`
- `MotionTCPDemo/Network/**/ONE_*_Table.st`
- `Elmo_EtherCAT_Test_4Axis/Class/**/*.st`
- `Elmo_EtherCAT_Test_4Axis/Network/**/*.lcn`
- `Elmo_EtherCAT_Test_4Axis/Network/**/ONE_*_Table.st`

제한:

- LASAL IDE build/download 검증은 하지 않았다.
- `.lcn`과 `ONE_*_Table.st`는 LASAL 생성/네트워크 파일 성격이 있으므로, 텍스트 확인 결과를 구조 판단 근거로 사용하되 최종 반영 전 IDE 재생성 여부를 확인해야 한다.
- 분석 시점 `MotionTCPDemo/`는 Git 기준 untracked 폴더였다. 여기서는 디스크에 존재하는 파일 내용 기준으로만 판단했다.

## 결론

`MotionTCPDemo`는 완성된 motion API 구현체가 아니라, SIGMATEK가 의도한 "통신 task와 real-time motion task를 분리하는 방법"을 보여주는 구조 예제다.

핵심 구조는 아래와 같다.

1. TCP 통신은 `_SigTCPDataManager`와 `TCPCommunication`이 맡는다.
2. `TCPCommunication`은 `Init()`에서 parallel communication task를 생성하고, 그 task 안에서 socket receive/send를 처리한다.
3. 수신 데이터는 `DestinationID` 기준으로 등록된 receiver callback에 전달된다.
4. 실제 motion 명령 해석부는 `TCP_MotionIF_Dummy::CallBackReceiver()`에 주석만 남아 있고 구현은 비어 있다.
5. real-time motion 쪽은 `MotionControlIF`라는 별도 `RtWork` class로 분리되어 있다.
6. motion network에는 12개 `_LMCAxis`, master axis, `_LMCProfile` 계층이 들어 있지만 대부분 simulation 축이다.
7. UDP 예제는 `_UDPTransceiver`가 `CyWork`에서 stack을 직접 읽는 구조로 들어 있다.

`Elmo_EtherCAT_Test_4Axis`는 위 예제와 방향이 다르다. 현재 프로젝트는 실제 Elmo EtherCAT 4축 drive를 붙인 통합 구현이며, `TCPMotionInterface.Response()`에서 frame을 받은 즉시 command parsing, response 송신, `LMCAxis`/`LMCRobot` motion call을 직접 수행한다. 또한 `_TCPIPServer_RT`가 `RtWork`에서 `_TCPIPServer.CyclicCall()`을 호출한다. Jonas 메일 기준으로는 TCP/Linux 통신 처리를 `RtWork`에 넣는 방식은 권장 구조가 아니다.

정확히 말하면, `MotionTCPDemo`는 "구조 예제"이고, `Elmo_EtherCAT_Test_4Axis`는 "실제 EtherCAT drive 연결 + PMAS-like TCP API 직접 이식 구현"이다.

## Jonas 메일 기준 구조

메일에서 확인된 구조 원칙은 아래다.

- TCP communication object, API command handling, motion object, axis object를 분리한다.
- motion command 실행은 `RtWork`가 도는 같은 core에서 실행되도록 한다.
- 필요하면 buffer/flag를 사용해 communication task에서 `RtWork`로 command를 넘긴다.
- command execution과 monitoring/control은 latency를 낮추기 위해 `RtWork`에서 수행한다.
- TCP library class는 자체 thread를 사용하며, class call 자체와 독립적으로 처리된다.
- TCP thread는 priority가 낮다.
- TCP data retrieval은 Linux 쪽 real-time guarantee가 없으므로 `RtWork`에 두는 것은 적합하지 않다.
- UDP는 `CyWork`에서 직접 retrieve하는 예제로 제공됐다.
- 8~12축 position/status는 client별 개별 read 또는 pointer loop 모두 가능하며 timing 차이는 없다고 설명했다.
- 축 group은 master axis follower 방식이 성능상 유리할 수 있고, profiled motion이 필요하면 `LMCProfile`을 사용한다.

## MotionTCPDemo 구조

### 네트워크 구성

`MotionTCPDemo`는 크게 3개 network로 나뉜다.

| Network | 역할 | 주요 객체 |
|---|---|---|
| `TCPComNet` | TCP communication/data manager 예제 | `_SigTCPDataManager1`, `TCP_MotionIF_Dummy1` |
| `UDPComNet` | CyWork 기반 UDP stack read 예제 | `_UDPTransceiver1`, `UDPTransmission1`, `String1` |
| `MotionNet` | simulated multi-axis/profile motion 예제 | `_LMCAxis1..12`, `_LMCAxis_Master_6_7_8`, `_LMCRobotBase1`, `MotionControlIF1` |

### TCPComNet

확인된 사실:

- `_SigTCPDataManager1`은 `CyclicTime = 10 ms`, `ComPort = 500`, send/receive buffer `100 kB`로 설정되어 있다.
- `_SigTCPDataManager` 내부 network에는 `DataManager`와 `TCPCommunication`이 들어 있다.
- `TCPCommunication`은 `coDataMng`를 통해 `DataManager`에 연결된다.
- `TCP_MotionIF_Dummy1._SigTCPDataManager`는 `_SigTCPDataManager1.ClassSvr`에 연결되어 있다.
- `TCP_MotionIF_Dummy1.SourceID`는 `1`이다.

근거:

- `MotionTCPDemo/Network/TCPComNet/TCPComNet.lcn`
- `MotionTCPDemo/Class/_SigTCPDataManager/_SigTCPDataManager.st`
- `MotionTCPDemo/Class/TCPCommunication/TCPCommunication.st`
- `MotionTCPDemo/Class/TCP_MotionIF_Dummy/TCP_MotionIF_Dummy.st`

주의할 점:

- `TCPComNet.lcn`에는 top-level receiver로 `TCP_MotionIF_Dummy1`만 확인된다.
- 그런데 `ONE_TCPComNet_Table.st`에는 `ReceiveMsg1.SourceID = 101` 항목도 남아 있다.
- 따라서 `ReceiveMsg1`은 생성 table에는 남아 있지만, 실제 `.lcn` top-level network 기준으로는 현재 연결된 예제 receiver라고 단정하지 않는 것이 맞다. IDE에서 network 재생성 상태를 확인해야 한다.

### TCPCommunication

`TCPCommunication`의 역할은 TCP socket stack 처리와 send/receive data manager 연결이다.

확인된 처리 흐름:

1. `TCPCommunication::Init()`에서 `coMultiTask.CreateThread(#CommunicationTask(), ...)`로 parallel task를 만든다.
2. `TCPCommunication::CommunicationTask()`는 endless loop로 `CyclicMethode()`를 호출한다.
3. `CyclicMethode()`는 연결 상태에서 `TransmitDataToStack()`과 `ReciveDataFromStack()`을 실행한다.
4. `ReciveDataFromStack()`은 `OS_TCP_USER_NREAD_AVAILABLE()`과 `OS_TCP_USER_RECV()`로 Linux/TCP stack 쪽 데이터를 읽는다.
5. packet이 완성되면 `NewMessage()`를 호출한다.
6. `NewMessage()`는 `PCMD_Data`에서 `RecivedData(DestinationID, SourceID, Length, pData)`를 호출한다.
7. `RecivedData()`는 `coDataMng.RecivedData(...)`로 넘긴다.

이 구조는 Jonas 메일의 "TCP library classes use their own thread" 설명과 맞다.

### DataManager

`DataManager`는 communication message routing과 send queue를 담당한다.

확인된 처리 흐름:

- `ConnectReciver(udID, pThis, pCallback)`로 receiver callback을 등록한다.
- `RecivedData(dDestinationID, dSourceID, udSize, pData)`는 등록 receiver list에서 `DestinationID`가 맞는 receiver를 찾아 callback을 호출한다.
- `SetData(...)`는 송신 job을 `DataManagerPriority`에 넣는다.
- `DataManagerPriority`는 priority `0..9`에 대해 `DataManagerFIFO` 10개를 사용한다.
- priority `0`이 가장 먼저 처리된다.

중요한 점:

- `DataManager::SetData()`에는 RT task 호출을 막는 guard가 있다.
- 하지만 `MotionTCPDemo/Class/DataManager/DataManager.h`에서 `DataManager_AcceptRtJob`가 정의되어 있다.
- 따라서 이 프로젝트 설정에서는 RT task에서 `SetData()`를 호출할 수 있도록 빌드될 가능성이 높다.

### TCP_MotionIF_Dummy

이 class가 Jonas 메일의 핵심 의도를 가장 직접적으로 보여준다.

`TCP_MotionIF_Dummy::CallBackReceiver()` 구현부에는 실제 command parser가 없다. 대신 아래 의미의 주석이 있다.

- 이 callback은 user thread의 background level에서 호출된다.
- 여기서 incoming command를 평가한다.
- 즉시 응답/처리를 하거나, RT class로 buffer를 통해 forward한다.
- `LMCAxis` command를 직접 set하려면, RT thread와 같은 core에서 실행 중인지 확인해야 한다.
- 기본적으로 user task는 core 1, RT는 core 0이라고 적혀 있다.

따라서 `MotionTCPDemo`는 "여기에 PMAS/MMCLib command parser를 구현하라"는 완제품이 아니라, "통신 callback에서 RT motion class로 넘기는 자리를 이렇게 만들라"는 skeleton이다.

### MotionNet

`MotionNet`에는 simulated multi-axis 구조가 들어 있다.

확인된 top-level 구성:

- `_LMCAxis1..12`: 각 축은 `RealTime = 1 ms`, `CyclicTime = 10 ms`, `BackgroundTime = 100 ms`
- `_LMCAxis_Master_6_7_8`: master axis 예제
- `_LMCRobotBase1`: top-level object name은 `_LMCRobotBase1`이지만 class는 `_LMCProfile`
- `MotionControlIF1`: class `MotionControlIF`, `RealTime = 1 ms`

축 설정:

- 각 `_LMCAxis`는 `SimulateMode = 1`
- `ExUnits = 65536`
- `IntUnits = 100mm`
- `VMax = 5m`

연결 구조:

- `_LMCRobotBase1.LMCAxis1` -> `_LMCAxis1.Control`
- `_LMCRobotBase1.LMCAxis2` -> `_LMCAxis2.Control`
- `_LMCRobotBase1.LMCAxis3` -> `_LMCAxis3.Control`
- `MotionControlIF1.LMCProfil` -> `_LMCRobotBase1.Control`
- `MotionControlIF1.Axis4` -> `_LMCAxis4.Control`
- `MotionControlIF1.Axis5` -> `_LMCAxis5.Control`
- `MotionControlIF1.MAxis6_7_8` -> `_LMCAxis_Master_6_7_8.Control`
- `MotionControlIF1.Axis6` -> `_LMCAxis6.Control`
- `MotionControlIF1.Axis7` -> `_LMCAxis7.Control`
- `MotionControlIF1.Axis8` -> `_LMCAxis8.Control`

판단:

- 12축 전체가 network에 존재하지만, `MotionControlIF1`에 명시적으로 연결된 축은 profile 1~3, direct axis 4~8, master axis 6_7_8이다.
- axis 9~12는 network에는 존재하지만 이 분석 범위에서 `MotionControlIF1`의 명령 대상으로 연결된 것은 확인되지 않았다.
- 이 구조는 Jonas가 말한 "8~12축 상태/position 수집"과 "master axis 또는 LMCProfile 사용"을 설명하기 위한 예제 성격이 강하다.

### UDPComNet

`UDPComNet`은 Jonas 메일의 UDP 설명과 직접 대응된다.

확인된 사실:

- `_UDPTransceiver1`은 `CyclicTime = 1 ms`
- `UDPTransmission1`은 `CyclicTime = 10 ms`
- `UDPTransmission1.Port = 8877`
- `String1.Data = "192.168.111.101"`
- network comment에 "Example with UDP. Stack is read in cyWork instead of BG"라고 적혀 있다.
- `_UDPTransceiver::CyWork()`는 `CyclicCall()`을 호출한다.
- `_UDPTransceiver::SendData()`는 `bDirect = true`일 때 즉시 `SendUDP()`를 호출하고, 아니면 ring buffer에 넣는다.

판단:

- TCP deterministic response time 문제를 `RtWork`로 해결하지 말고, 필요하면 UDP/CyWork 구조를 고려하라는 예제다.
- 단, UDPTransmission 구현도 실제 application protocol까지 완성된 것은 아니고, `Response()`에서 32-bit message를 읽는 정도의 demo다.

## Elmo_EtherCAT_Test_4Axis 구조

### 네트워크 구성

`Elmo_EtherCAT_Test_4Axis`의 핵심 network는 `Motion_Network`다.

확인된 top-level 구성:

- `_LMCAxis1..4`: 실제 4축 motion object
- `_LMCRobotBase1`: 4축 group/robot profile object
- `_TCPIPServer1`: base TCP server
- `_TCPIPServer_RT1`: `RtWork` wrapper
- `TCPMotionInterface1`: PMAS-like TCP frame parser/handler
- `Elmo_11`, `Elmo_21`, `Elmo_31`, `Elmo_41`: Elmo EtherCAT drive objects
- `PosController1..4`: `_LMCAxis`와 drive `SetPos`/status 사이의 controller adapter

축 설정:

- 각 `_LMCAxis`는 `RealTime = 1 ms`, `CyclicTime = 10 ms`, `BackgroundTime = 100 ms`
- `SimulateMode = 0`
- `ExUnits = 8388608`
- `IntUnits = 360 deg`
- `VMax = 18000 deg`

drive 연결:

- `_LMCAxis1.ActPosition` -> `Elmo_11.ActPos`
- `_LMCAxis1.LMCController` -> `PosController1.Signal_Input`
- `PosController1.oOutputSignal` -> `Elmo_11.SetPos`
- 같은 패턴이 axis 2~4에도 반복된다.
- `Elmo_11.toMaster`는 `EtherCAT_PLC1.EtherCATOut_1`에 연결되고, `Elmo_21`, `Elmo_31`, `Elmo_41`는 앞 drive의 `EtherCATOut_1`에 chain 형태로 연결된다.

group 연결:

- `_LMCRobotBase1.LMCAxis1` -> `_LMCAxis1.Control`
- `_LMCRobotBase1.LMCAxis2` -> `_LMCAxis2.Control`
- `_LMCRobotBase1.LMCAxis3` -> `_LMCAxis3.Control`
- `_LMCRobotBase1.LMCAxis4` -> `_LMCAxis4.Control`
- `TCPMotionInterface1.LMCRobot` -> `_LMCRobotBase1.Control`

### TCPMotionInterface

`TCPMotionInterface`는 `_TCPIPServerInterface`를 상속한다.

확인된 처리 흐름:

1. `_TCPIPServer.CyclicCall()`이 socket에서 `OS_TCP_USER_RECV()`로 데이터를 읽는다.
2. `_TCPIPServer`가 `pThisInterface^.Response(RECVbuffer.pData, RECVbuffer.udSize, socket)`를 호출한다.
3. `TCPMotionInterface::Response()`가 받은 data를 `ReceiveBuf`에 복사한다.
4. `CommandID`, `AxisRef`, `Payload`를 offset 기준으로 복사한다.
5. 즉시 `MsgPaser()`를 호출한다.
6. `MsgPaser()`는 command ID별로 분기한다.
7. command handler 안에서 `SendData()`와 `LMCAxis`/`LMCRobot` call을 직접 수행한다.

확인된 command:

| Command ID | 동작 |
|---|---|
| `0x2081` | `PowerOn()` |
| `0x2082` | `PowerOff()` |
| `0x2083` | `AxisReset()` |
| `0x2084` | `MoveStop()` |
| `0x202E` | `ReadActPos()` |
| `0x209F` | `MoveAbs()` |
| `0x20A4` | `MoveLinearAbsEx()` |
| `0x2045` | `GroupReadStatus()` |

중요한 구현 특성:

- `MoveAbs()`는 response를 먼저 만들고 `SendData(..., bDirect := TRUE)`로 즉시 보낸 뒤 `LMCAxis.MoveAbsolute(...)`를 호출한다.
- `ReadActPos()`는 `LMCAxis.ReadPosition(...)`을 직접 읽고 response를 즉시 보낸다.
- `MoveLinearAbsEx()`는 `LMCRobot.MoveLinearCoord(...)`를 직접 호출한 뒤 response를 즉시 보낸다.
- `GroupReadStatus()`는 `LMCRobot.ProfileInPosition(...)`을 직접 읽고 response를 즉시 보낸다.
- `RtWork()`와 `CyWork()` 양쪽에서 `LMCAxis.ReadPosition(...)`을 실행한다.

### _TCPIPServer_RT

`_TCPIPServer_RT`는 `_TCPIPServer`를 상속한 wrapper다.

확인된 사실:

- class 속성은 `RealtimeTask = true`, `DefRealtime = 1 ms`, `CyclicTask = true`
- `_TCPIPServer_RT::RtWork()`에서 `CyclicCall()`을 호출한다.
- `_TCPIPServer_RT::CyWork()`도 `bdStatus.CyclicTask = true`이면 `CyclicCall()`을 호출한다.

판단:

- 현재 구조는 TCP stack receive/send 경로를 `RtWork`에서도 실행할 수 있게 만든다.
- Jonas 메일의 "RtWork is not a viable option, since retrieving data from Linux does not provide real-time guarantees"와 맞지 않는다.
- 이 구조는 낮은 TCP latency를 의도했을 수 있지만, real-time motion task에 Linux/TCP receive 비용과 jitter를 끌고 들어오는 위험이 있다.

## 구조 차이 요약

| 항목 | MotionTCPDemo | Elmo_EtherCAT_Test_4Axis |
|---|---|---|
| 목적 | SIGMATEK 권장 architecture 예제 | Elmo EtherCAT 4축 + PMAS-like TCP API 이식 |
| 통신 계층 | `_SigTCPDataManager` + `TCPCommunication` + `DataManager` | `_TCPIPServer_RT` + `TCPMotionInterface` |
| TCP 처리 task | `TCPCommunication` parallel communication task + cyclic monitor | `_TCPIPServer_RT.RtWork()`/`CyWork()`에서 `CyclicCall()` |
| 수신 dispatch | `DestinationID` 기반 receiver callback | `TCPMotionInterface.Response()` 직접 parser 호출 |
| command parser | demo skeleton, 실제 구현 없음 | command ID별 parser/handler 구현됨 |
| motion call 위치 | `MotionControlIF.RtWork()`가 별도 자리로 존재하지만 구현은 비어 있음 | `Response()` 직후 command handler에서 직접 `LMCAxis`/`LMCRobot` 호출 |
| buffer/queue | `DataManagerPriority` + 10개 FIFO send job 구조 | PMAS command용 intermediate command queue 없음 |
| RT command handoff | 주석으로 권장, 구현은 사용자가 채워야 함 | 없음. TCP handler가 직접 motion call |
| 축 구성 | 12 simulated axes + master axis + profile | 4 real Elmo EtherCAT axes |
| drive 연결 | 실제 EtherCAT drive 없음, `SimulateMode = 1` | `Elmo_1..4` DS402/EtherCAT + `PosController1..4`, `SimulateMode = 0` |
| group/profile | `_LMCProfile` 예제와 master axis 예제 | `_LMCRobotBase1` 4축 group 연결 |
| UDP | CyWork에서 stack read하는 별도 예제 있음 | UDP 예제 없음 |

## Jonas 구조와 현재 Elmo 구조의 핵심 불일치

### 1. TCP receive가 RtWork로 들어와 있다

`Elmo_EtherCAT_Test_4Axis`의 `_TCPIPServer_RT::RtWork()`는 `CyclicCall()`을 호출한다. `_TCPIPServer.CyclicCall()` 내부는 `OS_TCP_USER_RECV()`를 호출하고, 수신 완료 시 `Response()`를 호출한다.

메일 기준으로 TCP receive는 Linux 쪽 처리가 끼므로 real-time guarantee가 없다. 따라서 TCP receive path를 `RtWork`에 넣는 현재 구조는 권장 방향과 다르다.

### 2. 통신 callback과 motion execution이 분리되어 있지 않다

현재 `TCPMotionInterface.Response()`는 다음 일을 한 번에 한다.

- raw frame copy
- command id parsing
- payload offset parsing
- response frame 생성
- `SendData(..., bDirect := TRUE)` 호출
- `LMCAxis.MoveAbsolute`, `LMCRobot.MoveLinearCoord`, `ReadPosition`, `ProfileInPosition` 호출

Jonas 구조라면 `Response()` 또는 communication callback에서는 frame 검증과 command buffer 적재까지만 하고, 실제 motion execution/monitoring은 `RtWork`에서 해야 한다.

### 3. axis selection 구조가 아직 단일 direct axis 중심이다

현재 individual motion command는 `TCPMotionInterface.LMCAxis` client 하나에 직접 연결되어 있다. network상 4축이 존재하지만 `MoveAbs()`는 `AxisRef`를 읽고도 실제로는 `LMCAxis` client 하나를 호출한다.

반면 `MotionTCPDemo`는 12축과 profile/master axis를 network에 두고, `MotionControlIF`가 여러 axis client를 받는 구조를 보여준다.

### 4. group motion은 구현되어 있지만 구조상 직접 호출이다

현재 group command는 `TCPMotionInterface`에서 `LMCRobot.MoveLinearCoord()`를 직접 호출한다. 동작 자체는 group object에 연결되어 있으나, command handoff/monitoring 분리 구조는 없다.

## 기존 Elmo 프로젝트에 적용할 구조 방향

`MotionTCPDemo`를 그대로 복사하는 것은 맞지 않다. 기존 Elmo 프로젝트는 실제 EtherCAT drive, DS402 PDO, PMAS-like frame, WPF 송신 코드와 이미 연결되어 있기 때문이다.

적용해야 할 것은 "구조 원칙"이다.

권장 변경 방향:

1. `_TCPIPServer_RT` 기반 TCP receive를 `RtWork`에서 빼는 방향을 검토한다.
2. TCP receive/callback 계층은 frame length, command id, axis/group target, payload endian 검증까지만 담당하게 한다.
3. `TCPMotionInterface.Response()`에서 직접 `LMCAxis.MoveAbsolute()` 또는 `LMCRobot.MoveLinearCoord()`를 호출하지 않게 한다.
4. communication task와 `RtWork` 사이에 command slot, ring buffer, flag + payload snapshot 중 하나를 둔다.
5. `RtWork` command executor가 pending command를 확인하고 같은 core에서 `LMCAxis`/`LMCRobot` method를 호출한다.
6. position/status는 `RtWork`에서 snapshot buffer로 갱신하고, TCP response는 그 snapshot을 읽어 보내는 방식으로 분리한다.
7. multi-axis command는 `AxisRef`를 실제 `_LMCAxis1..4` client selection에 반영하거나, pointer/client array에 준하는 구조를 추가한다.
8. group/profile command는 `_LMCRobotBase1`를 계속 사용하되, command queue와 completion/status monitoring을 분리한다.
9. TCP response time과 motion execution latency를 같은 지표로 보지 않는다. TCP response는 communication task jitter, motion completion은 `RtWork` 상태로 따로 측정한다.
10. C# WPF 송신 frame과 LASAL parser의 command id, endian, offset은 계속 byte 단위로 맞춘다.

## 판단

`MotionTCPDemo`가 보여주는 핵심은 "TCP를 빠르게 돌리는 법"이 아니라 "TCP와 motion을 분리해서 서로의 jitter를 줄이는 법"이다.

현재 `Elmo_EtherCAT_Test_4Axis`는 빠르게 기능을 붙인 직접 통합형 구조다. 실제 EtherCAT 4축 제어와 PMAS-like API 동작을 확인하기에는 유리하지만, Jonas가 지적한 8~12축 확장, TCP jitter, RT motion 안정성 관점에서는 구조 분리가 부족하다.

다음 단계에서 구조를 고친다면, 먼저 `_TCPIPServer_RT`/`TCPMotionInterface.Response()` 경로를 수정하는 것이 맞다. EtherCAT drive class나 `_LMCAxis` 자체를 먼저 바꾸는 것은 우선순위가 낮다.

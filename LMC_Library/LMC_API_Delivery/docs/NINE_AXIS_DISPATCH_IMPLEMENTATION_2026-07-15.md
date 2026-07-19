# 9축 Single-Axis Dispatcher 구현

## 적용 범위

현재 LASAL 프로젝트는 실제축 `_LMCAxis1..4`와 가상축 `_LMCAxis5..9`를
`TCPMotionInterface1`에 typed client로 연결한다. 가상축 5~9는
`SimulateMode=1`이며 hardware controller 없이 LASAL motion object로 동작한다.

`TCPMotionInterface`의 single-axis descriptor 범위를 `1..9`로 확장했다.
다음 명령은 축 1~9에 동일하게 dispatch된다.

- object-name lookup과 AxisInfo
- Read Status / Read Position
- Power On / Power Off
- Reset / Stop
- Move Absolute / Move Relative / Move Velocity

`GetGroupMembersInfo(0x20D2)`의 고정 16-slot 응답은 실제 연결된 9축의
reference, device ID, object name과 `AxisCount=9`를 반환한다. 응답 전체 길이와
PC parser 계약은 바꾸지 않았다.

축 5~9의 object name 조회는 IDE 메타데이터에 이미 등록된
`AxisObjectName1` 버퍼를 순차 scratch buffer로 재사용한다. CodeGenerator 선언
영역에 새 class variable을 직접 추가하지 않으므로 IDE 재생성 시 선언이
사라지는 문제를 피한다.

PC DLL은 원래 object name과 nonzero opaque descriptor에 축 개수 제한을 두지
않으므로 production source 변경이 필요하지 않다. WPF Single Axis 탭에서
`_LMCAxis1`부터 `_LMCAxis9`까지 이름을 입력해 한 축씩 시험할 수 있다.

## Group motion과의 경계

현재 공개된 group motion은 캡처로 확인된 X/Y/Z/U 4축 Cartesian identity
계약이다. 이번 변경은 다음 항목을 9축 group으로 확장하지 않는다.

- `LockProfile` axis mask
- `SetKinTransformCartesian4Axis`
- `MoveLinearCoord`의 X/Y/Z/U position mapping

축 5~9까지 group lock만 켜면 기존 4좌표 요청의 zero padding 때문에 가상축이
0 위치로 이동할 수 있다. 9축 동시 group motion은 정확히 9개 target을 받는
별도 direct axis-space 계약과 `LMCRobot.MoveLinear` 경로로 설계해야 한다.

별도 확인 사항: current `GroupReadActualPosition` handler는
`_LMCPROF_POS(Pos1..Pos9)` 36 bytes를 response slot 1..9에 복사한다. 이 read
동작은 4축 Move/SetKin/Lock 계약과 분리해서 PLC 재캡처하고, 4축-only read 또는
9축 readback 중 하나로 공개 계약을 확정해야 한다.

## 검증

- `RunPcTests`: 46/46 PASS
- `RunLasalContract`: PASS
- `git diff --check`: PASS

자동 검증은 LASAL IDE compile/link와 PLC download를 대신하지 않는다. PLC에서
각 축을 `_LMCAxis1`부터 `_LMCAxis9`까지 순서대로 Load하고 Read Status, Power,
작은 상대 이동, Read Position, Stop, Power Off를 확인한다.

`TCPMotionInterface`는 계속 CyWork-only이며 RT task를 추가하지 않았다. 다만
각 `_LMCAxis` motion object 자체는 1 ms RealTime task를 사용하므로 가상축 5개를
추가한 뒤 PLC CPU/real-time load를 확인해야 한다.

현재 저장된 network의 `_LMCAxis1..9.IntUnits`는 모두 `1 mm`다. 과거 문서의
실축 `10 mm/rev` 결정과 다르므로 실제 PLC download 전에 transmission 설정을
다시 확정해야 한다. dispatcher는 UNIT 변환을 수행하지 않는다.

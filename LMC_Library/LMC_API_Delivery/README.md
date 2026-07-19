# LASAL Motion Control Lib

LASAL 전용 DINT 패킷 API입니다. 기존 Elmo/Maestro용 legacy 패키지와 별도로 사용합니다.

프로젝트 전체 역할과 release gate는
[현재 아키텍처 및 릴리스 상태](../../docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)를
우선합니다.

## 개발 상태

2026-07-16 기준 C# request/typed response path와 재실행 가능한 자동 테스트가
반영됐습니다. tracked `TCPMotionInterface`에는 RPC lifecycle, 실제 LASAL
객체명 lookup, opaque descriptor, 9축 single-axis dispatcher, DINT single-axis path와
현재 공개된 group API handler를 반영했습니다.

현재 완료도를 구분하면 다음과 같습니다.

- 기존 Wireshark 캡처 기준 command: 23개
- LASAL project-local extension command: 2개
  (`0x204A GroupPowerOn`, `0x204B GroupPowerOff`; 기존 캡처 명령이 아님)
- C# request builder와 public 호출 경로: 25개 (캡처 기반 23 + local extension 2)
- LASAL source-active command: 25개 (캡처 기반 23 + local extension 2)
- CyWork axis/group control·read·motion command: 18개
  (lifecycle과 name/member metadata handler 제외)
  (`0x2023`, `0x2024`, `0x2022`, `0x2028`, `0x202E`, `0x209F`,
  `0x20A0`, `0x20A2`, `0x204A`, `0x204B`, `0x2047`, `0x2048`, `0x2045`, `0x2049`,
  `0x2085`, `0x20A4`, `0x2051`, `0x20E7`)
- 기존 캡처 기반 23-command 공개 범위의 deterministic unsupported: 0개
- C# 자동 테스트 runner: 46/46 PASS
- LASAL source-only/full-network static contract: PASS
- WPF example VS2019 MSBuild Debug: PASS
- 현재 group/9-axis source 반영 뒤 LASAL IDE Rebuild/Link: 미검증
- 현재 group/9-axis source 반영 뒤 Find in Implementation smoke: 미검증
- CyWork와 motion RT thread의 CPU core/priority 조건: 미검증
- 실제 PLC end-to-end와 Wireshark 재캡처까지 완료된 command: 0/25
  (캡처 기반 23 + local extension 2)

PC API 범위는 캡처 기반 23개 command와 LASAL local extension 2개 모두
request/public path까지 구현됐다. 새로
추가한 `0x2051`은 LASAL-DINT v1 전용 68-byte success response
(`DINT[16] + UINT16 status + INT16 error`)만 받으며 캡처의 PMAS legacy
136-byte LREAL response는 명시적으로 거부한다. 4-byte command-error
envelope는 오류 context를 보존한다. `0x20E7`은 캡처와 같은
1,320-byte payload를 만들되, 공개 호출은 캡처로 확인된 Cartesian 4축
X/Y/Z/U identity-shift와 `Buffered(2)` 조합으로 제한한다.

group source는 local extension인 `GroupPowerOn(0x204A)`/`GroupPowerOff(0x204B)`,
`GroupReset(0x2049)`, `GroupStop(0x2085)`,
`MoveLinearAbsoluteEx(0x20A4)`, `GroupReadActualPosition(0x2051)`,
`SetKinTransformCartesian4Axis(0x20E7)`까지 활성화됐다. 적용 범위는 현재
4축 static identity 구성으로 제한된다. `MoveLinearAbsoluteEx`는 좌표계
`None(0)`, transition `ExactStop(0)`/`ContinuousDirect(2)`, buffer
`Aborting(1)`/`Buffered(2)`만 허용한다. `0x20E7`은 동적 kinematic model을
생성하지 않고 exact X/Y/Z/U identity 요청을 검증해 static axis-order mapping만
설정한다. profile lock/unlock은 각각 `GroupEnable(0x2047)`의 `LockProfile`과
`GroupDisable(0x2048)`의 `UnlockProfile`이 담당한다. GroupDisable은
`ProfileInPosition(_LMCPROF_ProfileFinished)`가 확인된 상태에서만 unlock한다.

주의: current `0x2051` handler는 `_LMCPROF_POS`의 Pos1..Pos9를 DINT[16]
response slot 1..9에 복사한다. 기존 4축-only position read 문서와 충돌하므로
PLC 재캡처 뒤 4축 또는 9축 readback 계약을 확정해야 한다. 이 문제는
Move/SetKin/Lock의 4축 제한을 9축 group motion으로 확대하지 않는다.

정상 group 순서는 `GroupPowerOn -> GroupReadStatus.IsPowerOn -> identity axes
ReadStatus.IsReferenced -> SetKinTransform ->
GroupEnable/LockProfile -> motion -> GroupDisable/UnlockProfile -> GroupPowerOff ->
GroupReadStatus.IsPowerOn=false 확인`이다.
`GroupPowerOn`/`GroupPowerOff` ACK는 각각 비동기 `RobotOn`/`RobotOff` 요청 접수만
뜻하며 최종 완료가 아니다. `GroupReadStatusResult`의 `0x00040000=IsPowerOn`만
LASAL project-local Power Ready 확장이다. `0x00020000`은 Maestro 표준
`NC_GROUP_STANDBY_MASK`, `0x00010000`은 표준 `NC_GROUP_DISABLED_MASK`이며,
현재 어댑터는 각각 locked standby(`IsStandby/IsEnabled`)와 unlocked
disabled(`IsDisabled`) 조건에서 이 표준 mask를 설정한다.

다만 이것은 PC/LASAL source와 정적 계약 완료 판정이다. 현재 source를 LASAL
IDE에서 Rebuild/Link하지 않았고 PLC smoke test/재캡처도 남아 있으므로 실제 장비
API 완료가 아니다. callback은 payload 캡처가 없어 raw datagram event까지만
제공한다.
다중 PC의 읽기 공유·motion owner 정책은 LASAL session/ownership 계층에서
구현해야 한다.

실제 PLC 다운로드/packet 재캡처는 아직 수행하지 않았다. WPF example의 live
command gate, 작은 기본값, 물리 E-stop과
`../LasalApiWpfTestApp/README.md`의 순서를 지켜 단계별로 검증한다.

tracked `TCPMotionInterface.Response()`는 최대 1,328-byte frame을 2,048-byte
receive accumulator에서 조립하고 payload 최대 1,320 bytes를 depth-8 queue에
복사한다. non-RT `CyWork()` 하나가 `MsgPaser()`와 위 18개 승인 control/read/motion command를
실행합니다. interface RT task, RtWork mailbox와 atomic state는 사용하지
않습니다. 일반 `_TCPIPServer1`과 interface의 동일 cyclic task, axis RT thread와
같은 core 배치, PLC jitter를 확인하기 전까지 production-safe로 판정하지 않습니다.

상세 command matrix, 우선순위와 완료 조건:

- `docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md`

## 특징

- API 입력 단위: LASAL PLC가 받을 internal DINT
- API 내부 변환: 없음
- 단위 변환: 사용자 프로그램에서 직접 수행
- `LMC_Units`는 `unit.h` 기반 상수 선언만 제공
- 호출자는 물리값에 해당 `LMC_Units`를 곱하고 DINT 범위를 검사한 뒤 API 호출
- API 내부 코드는 `LMC_Units`를 참조하지 않음
- 현재 축의 PC application unit은 `1 mm = 10000 DINT`이며
  encoder `ExUnits=8388608`을 PC UNIT으로 사용하지 않음
- 이미 변환된 raw DINT는 추가 UNIT 변환 없이 그대로 전달할 수 있음
- LASAL PLC는 수신한 DINT를 변환 없이 `_LMCAxis` 또는 `_LMCRobot`에 전달
- `RpcInitConnection`은 TCP 연결 후 캡처 기반 RPC handshake(`0x8080`, `0x405C`)를 수행
- `RpcInitConnection`은 `0x405C` 전송 전에 callback listener를 열고 raw callback payload를 이벤트로 전달
- `CloseConnection`/`Dispose`는 캡처 기반 close frame(`0x405D`)을 송신
- 연결 timeout, state event, 초기화/transport/close 오류 분리, callback
  source-address 검증과 취소 가능한 async API를 제공
- timeout/전송 오류와 in-flight 취소는 오염된 transport를 폐기하고
  `Faulted`로 전환하며, queue 대기 중 취소는 active request를 건드리지 않음
- reconnect 후 이전 session에서 만든 axis/group object는 stale handle로 거부
- 취소 가능한 name lookup은 `LMCSingleAxis.CreateAsync`와
  `LMCGroupAxis.CreateAsync`를 사용하며 generation 검증과 request 전송을
  같은 exchange gate에서 확인
- `LMCSingleAxis`/`LMCGroupAxis` object는 name lookup으로 얻은 reference를 보관하고, 이후 motion/status API 호출 시 해당 reference를 패킷에 자동 삽입
- DLL은 `_LMCAxis1` 같은 PLC object name을 하드코딩하지 않음
- LASAL이 연결된 실제 object name을 읽어 opaque descriptor를 발급하고, 이후 descriptor로 axis client를 dispatch
- read API는 명령별 typed result를 제공하며 malformed response를 정상값 `0`과 구분
- WPF example은 네트워크/polling을 비동기로 실행하고 connection/callback 상태,
  raw callback log, 확인창 없는 즉시 명령, MoveVelocity stop 추적과 group API/options를
  제공. in-flight Cancel은 transport를 중단해 연결을 `Faulted`로 만들 수 있고
  PLC Stop을 보내지 않으므로, 안전 관련 command/rollback 중에는 Cancel을 차단
- 공개 API는 한 기능당 하나만 둡니다. `LMC_*Cmd`와 같은 중복 메소드 alias는 제공하지 않습니다.

## 폴더

- `src/bin/Release/LasalMotionControlLib.dll`: 개발 빌드 산출물. 배포 기준 DLL은
  `../LMC_API_Distribution/01_API/LasalMotionControlLib.dll`의 `0.9.1-preview`다.
- `src/`: DLL 전체 C# 소스
- `sample/BasicUsage.cs`: RPC 연결, raw callback, caller-side UNIT 변환과
  단일축 motion 호출 전 안전 확인 구조 예제
- `docs/USER_MANUAL_PREPARATION_2026-07-13.md`: 배포용 사용자 매뉴얼의
  범위, 목차, 예제와 출판 전 검증 gate
- `tests/LasalMotionControlLib.Tests/`: NuGet 없는 .NET Framework 4.8 자동 테스트 runner
- `docs/DINT_PACKET_MAP.txt`: LASAL 파서용 오프셋
- `docs/UNIT_CONVERSION_MANUAL_2026-07-10.md`: PC 호출자 UNIT 변환 배포 매뉴얼
- `docs/API_STRUCTURE_DECISION_2026-07-09.md`: 현재 API 구조 결정 기록
- `docs/RPC_CONNECTION_PACKET_DECISION_2026-07-09.md`: RPC connection 패킷 근거
- `docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md`: callback listener 소유권과 수명주기
- `docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md`: RPC/UDP callback 구현 상태와 검증 기준
- `docs/RESPONSE_MODEL_DESIGN_2026-07-09.md`: `LMC_Response` 한계와 응답 parser 재설계 방향
- `docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md`: 실제 object name lookup과 opaque descriptor 설계
- `docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md`: 일반 TCP server와 CyWork-only queue 실행 설계
- `docs/GROUP_API_IMPLEMENTATION_2026-07-14.md`: 현재 group API의 LASAL method, 제한과 검증 상태
- `docs/NINE_AXIS_DISPATCH_IMPLEMENTATION_2026-07-15.md`: 9축 single-axis dispatcher 범위와 group 분리 원칙
- `docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md`: 폐기된 RtWork 대안 검토 기록
- `docs/AUTOMATED_TESTS_2026-07-10.md`: 자동 테스트 범위와 실행법
- `docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md`: 다중 PC 세션 관리 설계

자동 테스트는 `RunPcTests`(C# 46 cases), `RunLasalContract`(tracked LASAL
source static checks), `RunTests`(두 검증과 WPF test app build) target으로
분리돼 있다.

## 주의

이 DLL은 Maestro의 LREAL 패킷과 호환되지 않습니다. LASAL 전용입니다.

`MoveCircle`은 현재 공개 C# API와 승인된 LASAL-DINT command ID/payload 계약에
없으므로 이 버전의 구현 범위가 아닙니다. 이름만 추가해 임의 wire protocol을
만들지 않습니다.

WPF example의 기본 `UNIT=10000`은 현재 저장된 `_LMCAxis1..9`의 `1 mm`
macro와 일치한다. Jerk 기본값은 `0`이지만 입력할 수 있으며
`Jerk DINT = (물리 jerk / 1000) x 축 UNIT`을 사용한다. 현재 저장된 profile은
`_JERK_PROFILE`, `JMax=75000 mm`다. 과거 `8,388,608 count/rev`는 23-bit
encoder dummy였고 DLL의 자동 변환이 아니다. 실제 배포 프로그램은 다운로드된
PLC 설정과 일치하는 UNIT/MoveType/JMax를 사용해야 한다.

Group motion의 nonzero Jerk도 robot profile 설정이 필요하다. canonical
`_LMCRobotBase1`은 `_JERK_PROFILE`, `JMax=50000 mm`로 저장돼 있다.
`GroupReset`은 `AxQuitError(AxisNo:=0)` 기반 axis/hardware error reset이며 robot
profile error 전체 초기화를 보장하지 않는다. `GroupReset`과 `GroupStop` ACK 뒤에는
`GroupReadStatusResult`로 실제 상태를 확인한다.

선형축 profile 예:

```csharp
var position = checked((int)Math.Round(1.0 * LMC_Units.MM));
axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);
```

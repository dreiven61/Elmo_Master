# LASAL Motion Control Lib

LASAL 전용 DINT 패킷 API입니다. 기존 Elmo/Maestro용 legacy 패키지와 별도로 사용합니다.

## 개발 상태

2026-07-10 기준 C# request/typed response path와 재실행 가능한 자동 테스트가
반영됐습니다. tracked `TCPMotionInterface`에는 RPC lifecycle, 실제 LASAL
객체명 lookup, opaque descriptor, 4축 dispatcher와 DINT single-axis path를
반영했습니다.

현재 완료도를 구분하면 다음과 같습니다.

- Wireshark 기준 대상 command: 23개
- C# request builder와 public 호출 경로: 23개
- 대응하는 LASAL `case` handler: 21개
- 위 21개 중 `GroupReset(0x2049)`과 `GroupStop(0x2085)`은 정상 기능이 아니라
  deterministic unsupported error `-5`를 반환
- C#에 구현됐지만 LASAL handler가 없는 command:
  `GroupReadActualPosition(0x2051)`, `SetKinTransformEx(0x20E7)`
- C# 자동 테스트 runner: 42/42 PASS
- LASAL static contract: PASS (6 clients, 4 links, offsets, error guards,
  legacy block)
- 실제 PLC end-to-end와 Wireshark 재캡처까지 완료된 command: 0개

PC API 범위는 23개 command 모두 request/public path까지 구현됐다. 새로
추가한 `0x2051`은 LASAL-DINT v1 전용 68-byte success response
(`DINT[16] + UINT16 status + INT16 error`)만 받으며 캡처의 PMAS legacy
136-byte LREAL response는 명시적으로 거부한다. 4-byte command-error
envelope는 오류 context를 보존한다. `0x20E7`은 캡처와 같은
1,320-byte payload를 만들되, 공개 호출은 캡처로 확인된 Cartesian 4축
X/Y/Z/U identity-shift와 `Buffered(2)` 조합으로 제한한다.

다만 이것은 PC source 완료 판정이다. 두 command의 LASAL handler,
command queue/RtWork 이관, IDE build, PLC smoke test와 재캡처가 남아 있으므로
실제 장비 API 완료는 아니다. callback은 payload 캡처가 없어 raw datagram
event까지만 제공한다. 다중 PC의 읽기 공유·motion owner 정책은 LASAL
session/ownership 계층에서 구현해야 한다.

LASAL IDE build와 실제 PLC 다운로드/packet 재캡처는 이 환경에서 수행하지
못했습니다. 그 검증이 끝나기 전에는 WPF test app으로 실제 motion을
수행하지 마십시오.

또한 tracked `TCPMotionInterface.Response()`가 아직 `MsgPaser()`를 직접
호출합니다. `_LMCAxis` 명령은 동일 core의 허용된 주기 context에서 호출해야
하므로 command queue를 거쳐 `RtWork`에서 실행하도록 옮기기 전까지 실제
motion 경로는 production-safe로 보지 않습니다. 현재 `_TCPIPServer_RT::RtWork()`의
TCP `CyclicCall()`도 non-RT transport task 하나로 분리해야 합니다.

상세 command matrix, 우선순위와 완료 조건:

- `docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md`

## 특징

- API 입력 단위: LASAL PLC가 받을 internal DINT
- API 내부 변환: 없음
- 단위 변환: 사용자 프로그램에서 직접 수행
- `LMC_Units`는 `unit.h` 기반 상수 선언만 제공
- 호출자는 물리값에 해당 `LMC_Units`를 곱하고 DINT 범위를 검사한 뒤 API 호출
- API 내부 코드는 `LMC_Units`를 참조하지 않음
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
- WPF test app은 네트워크/polling을 비동기로 실행하고 Cancel, connection/
  callback 상태, raw callback log와 신규 group API/options를 제공
- 공개 API는 한 기능당 하나만 둡니다. `LMC_*Cmd`와 같은 중복 메소드 alias는 제공하지 않습니다.

## 폴더

- `bin/LasalMotionControlLib.dll`: current `0.9.0-pc-api` preview DLL
- `src/`: DLL 전체 C# 소스
- `sample/BasicUsage.cs`: RPC 연결, raw callback, caller-side UNIT 변환과
  4축 group API의 안전한 구성 예제
- `tests/LasalMotionControlLib.Tests/`: NuGet 없는 .NET Framework 4.8 자동 테스트 runner
- `docs/DINT_PACKET_MAP.txt`: LASAL 파서용 오프셋
- `docs/UNIT_CONVERSION_MANUAL_2026-07-10.md`: PC 호출자 UNIT 변환 배포 매뉴얼
- `docs/API_STRUCTURE_DECISION_2026-07-09.md`: 현재 API 구조 결정 기록
- `docs/RPC_CONNECTION_PACKET_DECISION_2026-07-09.md`: RPC connection 패킷 근거
- `docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md`: callback listener 소유권과 수명주기
- `docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md`: RPC/UDP callback 구현 상태와 검증 기준
- `docs/RESPONSE_MODEL_DESIGN_2026-07-09.md`: `LMC_Response` 한계와 응답 parser 재설계 방향
- `docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md`: 실제 object name lookup과 opaque descriptor 설계
- `docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md`: TCP/CyWork/command queue/RtWork 분리 구현 설계안과 승인 항목
- `docs/AUTOMATED_TESTS_2026-07-10.md`: 자동 테스트 범위와 실행법
- `docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md`: 다중 PC 세션 관리 설계

자동 테스트는 `RunPcTests`(C# 42 cases), `RunLasalContract`(tracked LASAL
source static checks), `RunTests`(둘 다) target으로 분리돼 있다.

## 주의

이 DLL은 Maestro의 LREAL 패킷과 호환되지 않습니다. LASAL 전용입니다.

WPF test app의 `8,388,608 count/rev`는 호출자 프로그램이 선택한 23-bit
encoder 더미 profile입니다. DLL의 자동 변환이 아닙니다. 실제 배포
프로그램은 PLC 설정과 일치하는 UNIT 또는 scale로 DINT를 계산해야 합니다.

선형축 profile 예:

```csharp
var position = checked((int)Math.Round(1.0 * LMC_Units.MM));
axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);
```

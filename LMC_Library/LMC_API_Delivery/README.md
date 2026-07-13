# LASAL Motion Control Lib

LASAL 전용 DINT 패킷 API입니다. 기존 Elmo/Maestro용 legacy 패키지와 별도로 사용합니다.

## 개발 상태

2026-07-13 기준 C# request/typed response path와 재실행 가능한 자동 테스트가
반영됐습니다. tracked `TCPMotionInterface`에는 RPC lifecycle, 실제 LASAL
객체명 lookup, opaque descriptor, 4축 dispatcher와 DINT single-axis path를
반영했습니다.

현재 완료도를 구분하면 다음과 같습니다.

- Wireshark 기준 대상 command: 23개
- C# request builder와 public 호출 경로: 23개
- LASAL source-active command: 18개
- CyWork에서 실제 client call이 활성화된 command: 11개
  (`0x2023`, `0x2024`, `0x2022`, `0x2028`, `0x202E`, `0x209F`,
  `0x20A0`, `0x20A2`, `0x2047`, `0x2048`, `0x2045`)
- deterministic unsupported error `-5`: `0x2049`, `0x2085`, `0x20A4`,
  `0x2051`, `0x20E7`
- C# 자동 테스트 runner: 42/42 PASS
- LASAL static/strict contract: PASS
- LASAL IDE Rebuild/Link: PASS (`0 error`, `0 warning`)
- `Power`/`pos`/`velo` Find in Implementation smoke: PASS
- 실제 PLC end-to-end와 Wireshark 재캡처까지 완료된 command: 0개

PC API 범위는 23개 command 모두 request/public path까지 구현됐다. 새로
추가한 `0x2051`은 LASAL-DINT v1 전용 68-byte success response
(`DINT[16] + UINT16 status + INT16 error`)만 받으며 캡처의 PMAS legacy
136-byte LREAL response는 명시적으로 거부한다. 4-byte command-error
envelope는 오류 context를 보존한다. `0x20E7`은 캡처와 같은
1,320-byte payload를 만들되, 공개 호출은 캡처로 확인된 Cartesian 4축
X/Y/Z/U identity-shift와 `Buffered(2)` 조합으로 제한한다.

다만 이것은 PC source와 LASAL IDE source/build 완료 판정이다. unsupported 5개
group semantics와 PLC smoke test/재캡처가 남아 있으므로 실제 장비 API 완료는
아니다. callback은 payload 캡처가 없어 raw datagram event까지만 제공한다.
다중 PC의 읽기 공유·motion owner 정책은 LASAL session/ownership 계층에서
구현해야 한다.

실제 PLC 다운로드/packet 재캡처는 아직 수행하지 않았다. WPF test app의 live
command gate, 작은 기본값, 물리 E-stop과
`../LasalMotionControlLibTestApp/README.md`의 순서를 지켜 단계별로 검증한다.

tracked `TCPMotionInterface.Response()`는 frame을 depth-8 queue에 복사하고
non-RT `CyWork()` 하나가 `MsgPaser()`와 위 11개 승인 client call을
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
- WPF test app은 네트워크/polling을 비동기로 실행하고 connection/callback 상태,
  raw callback log, live-command arm, MoveVelocity stop 추적과 group API/options를
  제공. in-flight Cancel은 transport를 중단해 연결을 `Faulted`로 만들 수 있고
  PLC Stop을 보내지 않으므로, 안전 관련 command/rollback 중에는 Cancel을 차단
- 공개 API는 한 기능당 하나만 둡니다. `LMC_*Cmd`와 같은 중복 메소드 alias는 제공하지 않습니다.

## 폴더

- `bin/LasalMotionControlLib.dll`: current `0.9.0-pc-api` preview DLL
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
- `docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md`: 폐기된 RtWork 대안 검토 기록
- `docs/AUTOMATED_TESTS_2026-07-10.md`: 자동 테스트 범위와 실행법
- `docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md`: 다중 PC 세션 관리 설계

자동 테스트는 `RunPcTests`(C# 42 cases), `RunLasalContract`(tracked LASAL
source static checks), `RunTests`(두 검증과 WPF test app build) target으로
분리돼 있다.

## 주의

이 DLL은 Maestro의 LREAL 패킷과 호환되지 않습니다. LASAL 전용입니다.

WPF test app의 기본 `UNIT=10000`, `jerk=0`은 현재 `_LMCAxis1..4` degree
profile용이다. 과거 `8,388,608 count/rev`는 23-bit encoder dummy였고 DLL의
자동 변환이 아니다. 실제 배포 프로그램은 PLC 설정과 일치하는 UNIT 또는
scale로 DINT를 계산해야 한다.

선형축 profile 예:

```csharp
var position = checked((int)Math.Round(1.0 * LMC_Units.MM));
axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);
```

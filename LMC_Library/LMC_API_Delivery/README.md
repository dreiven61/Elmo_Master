# LASAL Motion Control Lib

LASAL 전용 DINT 패킷 API입니다. 기존 Elmo/Maestro용 legacy 패키지와 별도로 사용합니다.

## 특징

- API 입력 단위: LASAL PLC가 받을 internal DINT
- API 내부 변환: 없음
- 단위 변환: 사용자 프로그램에서 직접 수행
- `LMC_Units`는 `unit.h` 기반 상수 선언만 제공
- API 내부 코드는 `LMC_Units`를 참조하지 않음
- LASAL PLC는 수신한 DINT를 변환 없이 `_LMCAxis` 또는 `_LMCRobot`에 전달
- `RpcInitConnection`은 TCP 연결 후 캡처 기반 RPC handshake(`0x8080`, `0x405C`)를 수행
- `RpcInitConnection`은 `0x405C` 전송 전에 callback listener를 열고 raw callback payload를 이벤트로 전달
- `CloseConnection`/`Dispose`는 캡처 기반 close frame(`0x405D`)을 송신
- `LMCSingleAxis`/`LMCGroupAxis` object는 name lookup으로 얻은 reference를 보관하고, 이후 motion/status API 호출 시 해당 reference를 패킷에 자동 삽입
- `LMCAxis`/`LMCGroup` 및 `LMC_*` 메소드는 호환 wrapper입니다.

## 폴더

- `bin/LasalMotionControlLib.dll`: 최종 DLL
- `src/`: DLL 전체 C# 소스
- `sample/BasicUsage.cs`: 단축 및 그룹 사용 예제
- `docs/DINT_PACKET_MAP.txt`: LASAL 파서용 오프셋
- `docs/API_STRUCTURE_DECISION_2026-07-09.md`: 현재 API 구조 결정 기록
- `docs/RPC_CONNECTION_PACKET_DECISION_2026-07-09.md`: RPC connection 패킷 근거
- `docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md`: callback listener 소유권과 수명주기
- `docs/RESPONSE_MODEL_DESIGN_2026-07-09.md`: `LMC_Response` 한계와 응답 parser 재설계 방향
- `docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md`: 다중 PC 세션 관리 설계

## 주의

이 DLL은 Maestro의 LREAL 패킷과 호환되지 않습니다. LASAL 전용입니다.

기존 PMAS 기준 `8,388,608 count/rev` 값은 이 DLL의 송신 단위가 아닙니다. API 내부에서 자동 변환하지 않으므로 호출자가 값을 명확히 변환해야 합니다.

예:

```csharp
const int MM = 10000;
var position = checked((int)Math.Round(1.0 * MM));
axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);
```

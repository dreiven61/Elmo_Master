# LASAL Motion Control Lib

LASAL 전용 DINT 패킷 API입니다. 기존 Elmo/Maestro용 legacy 패키지와 별도로 사용합니다.

## 특징

- API 입력 단위: LASAL PLC가 받을 internal DINT
- API 내부 변환: 없음
- 단위 변환: 사용자 프로그램에서 직접 수행
- `LMC_Units`는 `unit.h` 기반 상수 선언만 제공
- API 내부 코드는 `LMC_Units`를 참조하지 않음
- LASAL PLC는 수신한 DINT를 변환 없이 `_LMCAxis` 또는 `_LMCRobot`에 전달
- TCP handshake 없이 LASAL 서버에 직접 연결
- `LMCSingleAxis`/`LMCGroupAxis` object는 name lookup으로 얻은 reference를 보관하고, 이후 motion/status API 호출 시 해당 reference를 패킷에 자동 삽입
- `LMCAxis`/`LMCGroup` 및 `LMC_*` 메소드는 호환 wrapper입니다.

## 폴더

- `bin/LasalMotionControlLib.dll`: 최종 DLL
- `src/`: DLL 전체 C# 소스
- `sample/BasicUsage.cs`: 단축 및 그룹 사용 예제
- `docs/DINT_PACKET_MAP.txt`: LASAL 파서용 오프셋
- `docs/API_STRUCTURE_DECISION_2026-07-09.md`: 현재 API 구조 결정 기록

## 주의

이 DLL은 Maestro의 LREAL 패킷과 호환되지 않습니다. LASAL 전용입니다.

기존 PMAS 기준 `8,388,608 count/rev` 값은 이 DLL의 송신 단위가 아닙니다. API 내부에서 자동 변환하지 않으므로 호출자가 값을 명확히 변환해야 합니다.

예:

```csharp
const int MM = 10000;
var position = checked((int)Math.Round(1.0 * MM));
axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);
```

# LMC Motion API

LASAL 전용 DINT 패킷 API입니다. 기존 Elmo/Maestro용 legacy 패키지와 별도로 사용합니다.

## 특징

- API 입력 단위: LASAL application unit (`double`)
- API 내부 변환: `unit.h` define과 같은 scale을 곱해서 DINT로 변환
- 기본 profile: `position=LMC_MM`, `velocity=LMC_MMPSEC`, `acceleration=LMC_MMPSEC2`, `deceleration=LMC_MMPSEC2`, `jerk=LMC_MMPSEC2`
- `LMC_Units`에 `unit.h` 기반 상수 추가: `LMC_MM=10000`, `LMC_MMPSEC=10000`, `LMC_MMPSEC2=1`, `LMC_DEG=10000`, `LMC_RPM=1000` 등
- LASAL PLC는 수신한 DINT를 변환 없이 `_LMCAxis` 또는 `_LMCRobot`에 전달
- TCP handshake 없이 LASAL 서버에 직접 연결

## 폴더

- `bin/LmcMotionApi.dll`: 최종 DLL
- `src/`: DLL 전체 C# 소스
- `sample/BasicUsage.cs`: 단축 및 그룹 사용 예제
- `docs/DINT_PACKET_MAP.txt`: LASAL 파서용 오프셋

## 주의

이 DLL은 Maestro의 LREAL 패킷과 호환되지 않습니다. LASAL 전용입니다.

기존 PMAS 기준 `8,388,608 count/rev` 값은 이 DLL의 송신 단위가 아닙니다.
예: 기본 profile에서 `1.0 mm -> 10,000 DINT`, `1.0 mm/s -> 10,000 DINT`, `1.0 mm/s2 -> 1 DINT`.

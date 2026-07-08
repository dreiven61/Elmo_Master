# LMC LASAL Motion API

LASAL 전용 DINT 패킷 API입니다. 기존 Elmo/Maestro용 `LmcMotionApi.dll`과 별도로 사용합니다.

## 특징

- API 입력 단위: rev, rps, rev/s2, rev/s3 (`double`)
- API 내부 변환: `double × 360 × 10000 → DINT`
- 기본 LASAL 내부 단위: `3,600,000 DINT/rev`
- LASAL PLC는 수신한 DINT를 변환 없이 `_LMCAxis` 또는 `_LMCRobot`에 전달
- TCP handshake 없이 LASAL 서버에 직접 연결

## 폴더

- `bin/LmcLasalMotionApi.dll`: 최종 DLL
- `src/`: DLL 전체 C# 소스
- `sample/BasicUsage.cs`: 단축 및 그룹 사용 예제
- `docs/LASAL_DINT_PACKET_MAP.txt`: LASAL 파서용 오프셋

## 주의

이 DLL은 Maestro의 LREAL 패킷과 호환되지 않습니다. LASAL 전용입니다.

기존 PMAS 기준 `8,388,608 count/rev` 값은 이 DLL의 송신 단위가 아닙니다.
예: `1 rev -> 3,600,000 LASAL internal DINT`.

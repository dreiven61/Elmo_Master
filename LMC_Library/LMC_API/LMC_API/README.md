# LMC Motion API 전달 패키지

Elmo DLL 없이 Maestro TCP 패킷으로 제어하는 C# API입니다. 함수 접두사는 `MMC_` 대신 `LMC_`를 사용합니다.

## 폴더 구성

- `bin/LmcMotionApi.dll`: 프로그램에서 참조할 API DLL
- `test-app/LmcMotionApiTestApp.exe`: 최종 패킷 테스트용 WPF 프로그램
- `docs/API_LIST.md`: 구현 API와 권장 호출 순서
- `docs/LMC_API_함수명_커맨드ID_인자.txt`: 함수명, Command ID, 인자 및 주요 응답 정리
- `docs/LMC_PACKET_MAP.md`: 명령 ID 및 패킷 메모
- `sample/BasicUsage.cs`: 연결, 단축, 그룹 제어 예제

## 사용 방법

1. C# 프로젝트 참조에 `bin/LmcMotionApi.dll`을 추가합니다.
2. `using LmcMotionApi;`를 선언합니다.
3. `LMCConnection`으로 연결한 뒤 `LMCAxis` 또는 `LMCGroup`을 생성합니다.

## 주의사항

- 기본 TCP 포트는 컨트롤러 설정에 맞춰 입력해야 합니다.
- 축 위치/속도 값은 컨트롤러 count 단위입니다. 현재 시험 기준은 1회전당 `8,388,608 count`입니다.
- 그룹 제어 전: 멤버 전체 Power On → SetKinTransform → GroupEnable 순서로 호출합니다.
- 그룹 제어 후 단축 제어로 복귀할 때: GroupStop → GroupDisable → 멤버 Power Off 순서로 그룹을 해제합니다.
- `ErrorId=2001`은 이미 Power On, `2000`은 이미 Power Off 상태에서 반환될 수 있습니다.
- `Reset`은 ErrorStop 상태에서만 사용합니다.

## 시험 완료 범위

- 4축 개별 Power On/Off 및 상태 확인
- 단축 절대/상대/속도 이동 패킷
- 4축 Cartesian X/Y/Z/U 키네마틱 설정
- GroupEnable 및 MoveLinearAbsoluteEx
- GroupStop/GroupDisable 후 단축 제어 전환

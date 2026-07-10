# LASAL Motion Control Lib 전달 패키지

Elmo DLL 없이 LASAL-DINT v1 TCP 패킷으로 제어하는 C# API 전달 폴더입니다.
기준 소스는 `../../LMC_API_Delivery`이며 public 함수 접두사는 `LMC_`가
아니라 `LMCSingleAxis`/`LMCGroupAxis` 객체 메소드입니다.
기존 `LmcMotionApi.dll`의 drop-in 교체본이 아니므로 legacy consumer는
`docs/MIGRATION_FROM_LMCMOTIONAPI.md`에 따라 수정·재컴파일해야 합니다.

2026-07-10 현재 PC source에는 캡처 대상 23개 command의 request builder와
public 호출 경로가 모두 있다. 그러나 LASAL IDE/PLC에서 end-to-end 검증된
command는 0개이고, `0x2051`과 `0x20E7`의 LASAL handler도 아직 없다.
따라서 이 폴더의 DLL/EXE는 `RELEASE_MANIFEST.md`의 SHA를 확인해야 하며,
manifest가 일치하더라도 현재 `0.9.0-pc-api`는 production 승인본이 아니다.

## 폴더 구성

- `bin/LasalMotionControlLib.dll`: 프로그램에서 참조할 API DLL
- `test-app/LasalMotionControlLibTestApp.exe`: 패킷 테스트용 WPF 프로그램
- `RELEASE_MANIFEST.md`: 버전, 파일 크기, SHA-256, 검증 결과
- `docs/API_LIST.md`: 구현 API와 권장 호출 순서
- `docs/LMC_API_함수명_커맨드ID_인자.txt`: 함수명, Command ID, 인자 및 주요 응답 정리
- `docs/LMC_PACKET_MAP.md`: 명령 ID 및 패킷 메모
- `docs/MIGRATION_FROM_LMCMOTIONAPI.md`: legacy API breaking-change 이관 절차
- `sample/BasicUsage.cs`: 연결, 단축, 그룹 제어 예제

## 사용 방법

1. C# 프로젝트 참조에 `bin/LasalMotionControlLib.dll`을 추가합니다.
2. `using LasalMotionControlLib;`를 선언합니다.
3. `LMCConnection`으로 연결한 뒤 `LMCAxis` 또는 `LMCGroup`을 생성합니다.

## 주의사항

- 기본 TCP 포트는 컨트롤러 설정에 맞춰 입력해야 합니다.
- DLL 내부에서 단위를 변환하지 않습니다. 호출자가 물리값에 PLC 설정과
  일치하는 `LMC_Units`를 곱하고 DINT 범위를 확인한 뒤 `int`로 전달합니다.
- WPF 예제의 `8,388,608`은 23-bit encoder dummy profile일 뿐 일반 UNIT이 아닙니다.
- `GroupReadActualPosition(0x2051)` 성공 응답은 LASAL-DINT v1 68-byte이고
  4-byte command-error envelope도 보존합니다. PMAS legacy 136-byte LREAL
  response와 호환되지 않습니다.
- `SetKinTransformCartesian4Axis(0x20E7)`은 캡처로 확인된 X/Y/Z/U
  identity-shift, Cartesian, `Buffered(2)` profile만 공개 지원합니다.
- callback은 UDP raw payload event만 제공합니다. typed callback payload는
  실제 datagram 캡처 전까지 정의하지 않습니다.
- 다중 PC의 읽기 공유와 motion/control owner 판정은 LASAL server 정책입니다.
- 그룹 제어 전후 순서와 safety interlock은 실제 LASAL 프로그램 승인 뒤
  적용해야 합니다.

## 구현 및 검증 범위

- PC request/public path: 23/23
- PC 자동 테스트 runner: 42/42 PASS
- LASAL static source contract: PASS
- LASAL `case` handler: 21/23 (`0x2049`, `0x2085`는 unsupported `-5`)
- LASAL handler 없음: `0x2051`, `0x20E7`
- 실제 PLC E2E 및 Wireshark 재캡처: 0/23

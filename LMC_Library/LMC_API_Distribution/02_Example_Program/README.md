# LasalApiWpfTestApp

`LasalMotionControlLib.dll`의 실제 public API를 사용하는 .NET Framework 4.8
WPF 예제다. API source project에는 연결되지 않는다.

## 빌드

Visual Studio 2019 이상에서 `LasalApiWpfTestApp.sln`을 열고 x64 Debug 또는
Release로 빌드한다. 프로젝트의 DLL 참조는 다음 상대경로로 고정돼 있다.

```text
..\..\01_API\LasalMotionControlLib.dll
```

폴더 구조를 유지하면 저장소 밖으로 복사해도 빌드된다. 빌드 후 DLL은
`bin/<Configuration>`으로 자동 복사된다.

`Run` 폴더에는 배포 시점에 빌드한 실행 파일과 동일 DLL을 함께 넣는다.
소스 수정이나 자체 빌드가 필요 없으면 `Run/LasalMotionControlApiExample.exe`를
실행할 수 있다.

## 시험 순서

1. PLC IP/TCP port, PC local IPv4와 callback UDP port를 확인한다.
2. Connect 후 object name으로 axis/group을 Load한다.
3. Single Axis에서는 Read Status/Position으로 상태를 먼저 확인한다.
4. Group Motion에서는 화면의 1~6 순서를 따른다.
5. Set Identity는 X/Y/Z/U Home Check를 자동 실행하고 하나라도
   `IsReferenced=false`이면 전송을 차단한다.
6. 작은 값으로 시작하고 motion 뒤 status/position을 다시 읽는다.
7. Stop/Disable/Power Off는 각각 다른 명령이다. 상태가 바뀔 때까지 읽어서
   확인한다.

## 복구 식별자 불일치 시 조회

저장된 복구 기록의 `BootId` 또는 `MapRevision`이 현재 PLC와 다르면 연결은
읽기 전용 격리 상태로 유지된다. 이 상태에서도 `Load Axis`는 axis reference와
AxisInfo를 표시하고, `Load Group`은 group reference와 member 목록을 함께 표시한다.
조회 결과는 앱의 제어 handle에 유지되지 않으며 Power/Reset/Stop/Motion과 복구 기록
확정은 계속 차단된다.

## SDO Read/Write 화면

Diagnostics 탭의 SDO 영역에서 `Operation=Write`를 선택하면 공용 실행 버튼이
`Arm SDO Write`로 바뀐다. 실제 Write는 PLC capability bit 9와 SDK의 exact
allowlist가 모두 열리고, 선택한 `(slave,index,subindex,type,length)`가 일치할
때만 활성화된다. 현재 기본 배포 설정은 승인 target이 없으므로 fail-closed가
정상이다. 후보 `UI[24] (0x2F00:24)`의 미사용 여부와 시험할 축을 확인하기 전에
gate를 켜지 않는다.

주의: 예제의 Close 버튼은 motion Stop을 보내지 않는다. 실제 장비의 E-stop,
software/hardware limit와 작업영역 검증을 별도로 준비한다.

현재 `Group Read Position`의 slot 5..9는 PLC source와 기존 4축 문서의 계약이
일치하지 않아 재캡처 대기 상태다. X/Y/Z/U slot 1..4 외 값은 production
판정에 사용하지 않는다. Group Move/SetKin/Lock은 계속 4축 전용이다.

API별 인자, UNIT과 반환값은
[LASAL Motion Control API 사용자 매뉴얼](../03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf)을
참조한다. 단, 포함된 PDF는 문서 버전 `1.0`이다. 최신 preview/검증/안전
제한은 먼저 [package README](../README.md)를 확인한다.

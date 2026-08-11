# LasalApiWpfTestApp

`LasalMotionControlLib.dll`의 실제 public API를 사용하는 .NET Framework 4.8
WPF 예제다. API source project에는 연결되지 않는다.

> **Preview 경고:** 이 예제와 포함 package는 preview이며 production 승인본이
> 아니다. PC/static/build 결과는 current PLC 또는 hardware 검증이 아니다.

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
`Arm SDO Write`로 바뀐다. 유일하게 source 승인된 target은 Axis 1 Gold UI[24],
exact `Slave 1 / 0x2F00:24 / Int32 / 4 bytes`다. Axis 2..4와 모든 비승인
target은 SDK와 PLC에서 차단된다.

실제 Write는 fresh PLC capability bit 9와 SDK exact allowlist가 모두 열리고,
같은 current connection/session의 `DiagnosticsBuild`, `BootId`, `MapRevision`과
exact target을 고정한 뒤에만 활성화된다. baseline, pre-write guard, Write,
guarded readback의 서로 다른 four-ticket same-value qualification이 먼저 PASS해야
하며, identity drift나 disconnect는 proof를 폐기한다. 결과가 불명확한 Write는
자동 재전송하지 않는다.

실기 전에 Axis 1 drive program에서 UI[24]가 미사용임을 확인하고 PowerOff/Standstill,
position 안정, 작업자 승인과 mutation journal을 모두 확인한다. current PLC download,
UI[24] 소유권, EtherCAT mailbox mutation과 physical readback은 아직 검증되지 않았다.

주의: 예제의 Close 버튼은 motion Stop을 보내지 않는다. 실제 장비의 E-stop,
software/hardware limit와 작업영역 검증을 별도로 준비한다.

현재 `Group Read Position`의 slot 5..9는 PLC source와 기존 4축 문서의 계약이
일치하지 않아 재캡처 대기 상태다. X/Y/Z/U slot 1..4 외 값은 production
판정에 사용하지 않는다. Group Move/SetKin/Lock은 계속 4축 전용이다.

API별 인자, UNIT과 반환값은
[LASAL Motion Control API 사용자 매뉴얼](../03_API_User_Manual/LASAL_Motion_Control_API_User_Manual_KO.pdf)을
참조한다. 포함된 DOCX/PDF는 current source 계약과 맞춘 문서 버전
`2.3-candidate`다. 이 tracked canonical release-input 승격은 full Distribution
PASS 또는 production 승인을 뜻하지 않는다. package 전체 preview/검증/안전
경계는 [package README](../README.md)도 함께 확인한다.

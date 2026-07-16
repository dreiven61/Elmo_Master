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

주의: 예제의 Close 버튼은 motion Stop을 보내지 않는다. 실제 장비의 E-stop,
software/hardware limit와 작업영역 검증을 별도로 준비한다.

API별 인자, UNIT과 반환값은
`..\03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf`를 참조한다.

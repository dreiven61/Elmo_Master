# MoveAbsoluteEx / ReadActualPosition 응답 정리

## 출처
- `NetHelp/Elmo Maestro API Help/Documents/moveabsoluteex.htm`
- `NetHelp/Elmo Maestro API Help/Documents/getactualposition.htm`
- `NetHelp/Elmo Maestro API Help/MMCLibDotNET/.../InternalArgs/MoveAbsoluteExArgsIN.cs`
- `NetHelp/Elmo Maestro API Help/MMCLibDotNET/.../InternalArgs/ReadActualPositionIN.cs`
- `NetHelp/Elmo Maestro API Help/MMCLibDotNET/.../InternalArgs/ReadActualPositionOUT.cs`
- `NetHelp/Elmo Maestro API Help/MMCLibDotNET/.../InternalArgs/DefaultOutputFBArgs.cs`

> 주의: 공식 PDF(`Maestro Administrative and Motion API_2022_12_v2.012.pdf`)는 텍스트상으로는 **명령/응답의 원시 바이트 레이아웃을 직접** 노출하지 않으며, 실제 통신 포맷 검증은 .NET 래퍼 소스 코드로 보완해야 함.

---

## 1) MoveAbsoluteEx

### API 시그니처
- C 함수: `MMC_MoveAbsoluteExCmd(hConn, hAxisRef, MMC_MOVEABSOLUTEEX_IN*, MMC_MOVEABSOLUTEEX_OUT*)`
- C# 단일축 API: `MMCSingleAxis.MoveAbsoluteEx(...)`
  - Overload 1: `(double dPosition, MC_BUFFERED_MODE_ENUM eBufferMode)`
  - Overload 2: `(double dPosition, double dVelocity, MC_BUFFERED_MODE_ENUM eBufferMode)`
  - Overload 3: `(double dPosition, double dVelocity, double dAcceleration, double dDeceleration, double dJerk, MC_DIRECTION_ENUM eDirection, MC_BUFFERED_MODE_ENUM eBufferMode)`

### 요청 입력 구조체 (공식 API 구조)
`MMC_MOVEABSOLUTEEX_IN`
- `double dbPosition`
- `double dVelocity`
- `double dAcceleration`
- `double dDeceleration`
- `double dJerk`
- `MC_DIRECTION_ENUM eDirection`
- `MC_BUFFERED_MODE_ENUM eBufferMode`
- `unsigned char ucExecute`

### 응답(동작 상태)
`MMC_MOVEABSOLUTEEX_OUT`
- `unsigned int uiHndl`
- `unsigned short usStatus`
- `short usErrorID`
- `MoveAbsoluteEx` 자체는 “command returns 상태/에러” 형태이며, .NET 래퍼는 실제로 **DefaultOutputFBArgs**로 파싱됨.
  - 즉 응답은 기능 블록 핸들 + 상태 + 에러 ID 패턴을 갖는 형태로 처리됨.

### 라이브러리 기준 와이어 레벨 (보강 값)
- `MoveAbsoluteExArgsIN.CommandID = 8351` (`0x20AF`)
- `DefaultOutputFBArgs` 크기: `12` bytes (`uiHndl 4 + usStatus 2 + sErrorID 2` + padding in parser model)
- 요청 길이 `Length = sizeof(MOVEABSOLUTEEX_IN) + 8 = 64`
  - 실제로 `DataOut()`이 만든 패킷에서 payload에 실제 값이 쓰이는 바이트는 position/velocity/acc/dec/jerk(각 8바이트), direction(4), bufferMode(4), execute(1), 나머지 패딩.
- 응답 길이(라이브러리 기본 파싱 기준): `16` bytes = `8(header)+12(payload)`

### 상태/에러 의미
- `usStatus` / `usErrorID`는 `Single Axis Common`/`Output Parameters` 규격의 공통 비트/에러 포맷을 사용.

---

## 2) ReadActualPosition

### API 시그니처
- C 함수: `MMC_ReadActualPositionCmd(hConn, hAxisRef, MMC_READACTUALPOSITION_IN*, MMC_READACTUALPOSITION_OUT*)`
- C#: `MMCSingleAxis.GetActualPosition()`

### 요청 입력 구조체
`MMC_READACTUALPOSITION_IN`
- `unsigned char ucEnable`

### 응답 구조체
`MMC_READACTUALPOSITION_OUT`
- `double dbPosition`
- `unsigned short usStatus`
- `short usErrorID`

### 라이브러리 기준 와이어 레벨 (보강 값)
- `ReadActualPositionIN.CommandID = 8238` (`0x202E`)
- 요청 길이: `9`
- 응답(파서) payload size: `12` bytes (`double 8 + usStatus 2 + usErrorID 2`)
- 데이터 파싱 오프셋(라이브러리 기준):
  - `Position`: offset **8**
  - `usStatus`: offset **16**
  - `usErrorID`: offset **18**
- 응답 전체 길이: `20` bytes (`8(header)+12(payload)`)

---

## 3) 정리

- **MoveAbsoluteEx**
  - 요청: `0x20AF`(MoveAbsoluteEx) + payload(커맨드 파라미터)
  - 응답: 상태/에러 리턴 + FB 핸들
- **ReadActualPosition**
  - 요청: `0x202E`(ReadActualPosition) + `ucEnable`
  - 응답: 현재 위치(`double`) + 상태/에러

위 값은 `MoveAbsoluteEx`/`ReadActualPosition` 에 대한 Elmo 공식 API 구조 및 MMCLibDotNET decompile을 기준으로 정리한 **동작 계측 가능한 wire-level 기대값**입니다.

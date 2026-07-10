# `LmcMotionApi` Migration Guide

`LasalMotionControlLib.dll`은 기존 `LmcMotionApi.dll`의 drop-in update가 아니다.
assembly 이름, namespace, public 함수와 숫자 단위 계약이 모두 바뀌었으므로
기존 프로그램은 DLL만 교체해서 사용할 수 없다.

## Breaking changes

| 기존 | 현재 |
|---|---|
| assembly/namespace `LmcMotionApi` | `LasalMotionControlLib` |
| `LMC_RpcInitConnection` 등 `LMC_*Cmd` 함수 | `LMCConnection`과 axis/group 객체 메소드 |
| caller가 축 reference를 각 명령에 전달 | 이름으로 `LMCSingleAxis`/`LMCGroupAxis`를 만들고 객체가 reference 보관 |
| `double`/`float` 물리값 중심 인자 | caller가 PLC UNIT을 곱한 `int` DINT |
| legacy PMAS/LREAL response 가정 | LASAL-DINT v1 exact typed response |
| callback 계약 불명확 | source IP를 검증한 raw UDP `CallbackReceived` event |

## 필수 이관 절차

1. 프로젝트 참조와 `using`을 `LasalMotionControlLib`으로 변경한다.
2. PC별로 `LMCConnection`을 생성하고 `RpcInitConnection`을 호출한다.
3. 실제 LASAL object name으로 `LMCSingleAxis`/`LMCGroupAxis`를 생성한다.
4. 각 물리값을 PLC 설정과 같은 UNIT으로 변환하고 DINT overflow를 검사한다.
5. command response의 `IsFrameValid`와 `IsSuccess`를 모두 확인한다.
6. 재연결 후에는 기존 axis/group 객체를 버리고 다시 lookup한다.
7. async 취소나 transport fault 뒤에는 해당 connection을 재연결한다.

```csharp
var rawPosition = checked((int)Math.Round(positionDeg * LMC_Units.DEG));

using (var connection = new LMCConnection())
{
    connection.RpcInitConnection(remoteIp, 4000, localIp);
    var axis = new LMCSingleAxis(connection, "_LMCAxis1");
    var response = axis.MoveAbsoluteEx(
        rawPosition,
        rawVelocity,
        rawAcceleration,
        rawDeceleration,
        0);

    if (!response.IsFrameValid || !response.IsSuccess)
    {
        throw new InvalidOperationException("Motion command failed.");
    }
}
```

기존 consumer를 즉시 이관할 수 없다면 legacy DLL/package를 별도 경로에
고정해야 한다. 같은 폴더에서 새 DLL로 덮어쓰거나 assembly binding redirect로
호환할 수 있는 계약이 아니다.

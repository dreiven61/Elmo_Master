> 빌드 통과했습니다. 배포 DLL을 최신 빌드로 복사하고, 이번 수정만 커밋하겠습니다.
>
> 스테이징 전에 상태와 체크를 확인합니다. `bin`은 ignore라 이번에도 소스/문서만 커밋합니다.
>
> 변경 범위가 정확합니다. 이제 `LmcUnits.cs` 선언 파일과 관련 문서/csproj만 커밋합니다.
>
> staged diff도 문제 없습니다. 커밋합니다.
>
> 커밋 완료했습니다. 마지막으로 검색과 상태를 다시 확인해서 내부 참조가 없는지 보고하겠습니다.
>
> 커밋 후 확인 중에 한 가지 더 처리합니다. `LmcUnits.cs`를 추가한 뒤 최종 빌드를 다시 돌려야 합니다. 바로 빌드하고 DLL도 다시 갱신하겠습니다.
>
> 빌드 통과했습니다. 배포 DLL만 최신으로 덮어쓰고 마무리 상태를 확인합니다.
>
> 최종 확인만 하겠습니다. 소스 커밋은 이미 완료됐고, `bin`은 ignore 대상이라 작업트리에는 안 잡히는 게 정상입니다.
</details>

내가 잘못 이해했다. 지금은 네가 말한 구조로 맞췄다.

수정 결과:
- `LMC_Units` 선언은 복구: [LmcUnits.cs](./LMC_Library/LMC_API_Delivery/src/LmcUnits.cs:1)
- `UnitConverter`, `LMC_UnitConverter`는 제거된 상태 유지
- `MMCSingleAxis`, `MMCGroupAxis`, `LMC_Frame`, `LMCConnection` 내부에서는 `LMC_Units` 참조 없음
- 검색 확인: 내부 API 실행 경로 파일들에서 `LMC_Units` 참조 0건
- 문서도 “상수 선언은 있으나 API 내부 코드는 참조하지 않는다”로 수정

검증:
- `dotnet build ...\LasalMotionControlLib.sln -c Release`
- 경고 0, 오류 0
- DLL도 `LMC_Library/LMC_API_Delivery/bin/LasalMotionControlLib.dll`로 다시 복사 완료

커밋:
- `c583807 Restore unit constants as declarations`

> C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery 에서 MMC라고 선언되어있는 모든걸 LMC로 바꿔

<details><summary>11 previous messages</summary>

> 요청대로 `LMC_API_Delivery` 안의 `MMC` 이름을 전부 확인해서 `LMC`로 바꾸겠습니다. 먼저 문자열 위치를 전부 보고, 코드 심볼/문서/샘플까지 같은 기준으로 수정한 뒤 빌드와 커밋까지 진행하겠습니다.
>
> 현재 `MMC`는 클래스명과 문서 표현에만 남아 있습니다. 코드 심볼은 `MMCSingleAxis -> LMCSingleAxis`, `MMCGroupAxis -> LMCGroupAxis`로 바꾸고, 문서의 `MMCLib` 표현도 이 delivery 폴더 안에서는 `LMC-style`로 바꿔 `MMC` 문자열이 남지 않게 하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+3 -3)
>
> </details>
>
> 타입명 변경에 맞춰 샘플과 문서도 수정합니다. 이 변경 뒤에는 `rg "MMC"`가 0건이어야 합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+4 -4)
>
> </details>
>
> 이제 전체 폴더에서 `MMC` 잔여 문자열을 다시 검색하고 빌드하겠습니다.
>
> `rg "MMC"`는 0건입니다. 타입명은 `LMCSingleAxis`/`LMCGroupAxis`로 정리됐습니다. 이제 빌드 확인합니다.
>
> 빌드 통과했습니다. 배포 DLL도 최신 빌드로 복사하고, diff check 후 커밋하겠습니다.
>
> 체크 통과했고 `MMC` 잔여 검색도 0건입니다. 변경 파일만 스테이징해서 커밋합니다.
>
> staged diff도 문제 없습니다. 커밋합니다.
>
> 커밋 완료했습니다. 마지막으로 최신 로그와 잔여 상태를 확인합니다.
</details>

처리했다.

변경:
- `MMCSingleAxis` -> `LMCSingleAxis`
- `MMCGroupAxis` -> `LMCGroupAxis`
- README, 샘플, 설계 문서 안의 `MMC` 표현도 제거
- `LMC_API_Delivery` 전체에서 `rg "MMC"` 결과 0건

검증:
- `dotnet build C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\src\LasalMotionControlLib.sln -c Release`
- 경고 0, 오류 0
- 최신 DLL 복사 완료: `C:\work\Elmo\Elmo_Master\LMC_Library\LMC_API_Delivery\bin\LasalMotionControlLib.dll`

커밋:
- `7207d08 Rename MMC delivery API symbols to LMC`

> ResolveAxisReference 이거 머하는 함수지?

<details><summary>1 previous message</summary>

> 현재 코드 기준으로 확인해서 답하겠습니다. `ResolveAxisReference`는 축 이름을 숫자 reference로 바꾸는 초기 lookup 함수입니다.
</details>

`ResolveAxisReference()`는 **축 이름을 축 reference 번호로 바꾸는 함수**다.

현재 흐름은 이거다.

```csharp
AxisName = axisName;
AxisReference = ResolveAxisReference(axisName);
connection.Exchange(LMC_Frame.AxisInfo(AxisReference));
```

즉 `new LMCSingleAxis(connection, "a01")` 할 때:

1. `"a01"`을 `GetAxisByName` 패킷으로 보냄
2. PLC/LASAL 쪽에서 `"a01"`에 해당하는 reference 번호를 응답함
3. 응답 offset `12`에서 `ushort`로 읽어서 `AxisReference`에 저장함
4. 이후 `MoveAbsoluteEx`, `ReadStatus`, `GetActualPosition` 같은 함수는 이 저장된 `AxisReference`를 씀

코드 위치:
- [LmcAxis.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs:212)
- name lookup frame 생성: [LmcProtocol.cs](./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:87)

이름이 조금 길긴 한데 의미는 정확하다.
`ResolveAxisReference("a01")` = `"a01" 축 이름을 패킷용 숫자 핸들/ref로 변환한다` 이 뜻이다.

> LMC_Frame은 패킷을 분석했었을때 공용으로 쓰는 프레임을 정리해놓은건가?

맞다. `LMC_Frame`은 **패킷 분석 결과를 코드로 옮겨놓은 frame builder/parser 유틸**이라고 보면 된다.

역할은 크게 3개다.

1. 공통 request header 생성

```csharp
CreateRequest(command, reference, payloadLength)
```

이 함수가 공통 헤더를 만든다.

```text
[0..1] Command ID
[2..3] Reserved
[4..5] Payload length
[6..7] Axis/Group reference
[8.. ] Payload
```

2. 명령별 패킷 payload 생성

예를 들면:

```csharp
LMC_Frame.Name(...)
LMC_Frame.AxisInfo(...)
LMC_Frame.Power(...)
LMC_Frame.AxisMove(...)
LMC_Frame.Velocity(...)
LMC_Frame.MoveLinear(...)
```

이런 함수들이 각각 `GetAxisByName`, `AxisInfo`, `MoveAbsoluteEx`, `MoveVelocityEx`, `MoveLinearAbsoluteEx` 같은 패킷을 만든다.

3. 응답에서 값 읽기

```csharp
ReadUInt16(...)
ReadUInt32(...)
ReadInt32(...)
GetResponsePayloadLength(...)
```

이런 함수들이 response header/payload를 little-endian으로 읽는다.

즉 `LMC_Frame`은 축 객체나 그룹 객체가 직접 바이트 offset을 만지지 않게 하려고 둔 계층이다.

```text
LMCSingleAxis.MoveAbsoluteEx(...)
  -> LMC_Frame.AxisMove(...)
      -> byte[] packet 생성
  -> LMCConnection.Exchange(packet)
```

정리하면, `LMC_Frame`은 **캡처에서 확인한 command id, offset, payload layout을 모아둔 패킷 조립/해석 클래스**다.

> var raw = connection.Exchange(
>                 LMC_Frame.Name(LMC_CommandId.GetAxisByName, axisName));
>
> 여기 이부분이 이해가 잘 안돼는데 어떤식임?

이 한 줄은 **“축 이름을 보낸 뒤, 그 응답 패킷을 raw byte 배열로 받는다”**는 뜻이다.

```csharp
var raw = connection.Exchange(
    LMC_Frame.Name(LMC_CommandId.GetAxisByName, axisName));
```

풀어서 쓰면 이거다.

```csharp
var request = LMC_Frame.Name(LMC_CommandId.GetAxisByName, axisName);
var raw = connection.Exchange(request);
```

각 부분 역할은 이렇다.

```csharp
LMC_CommandId.GetAxisByName
```

`0x103C` command id다. 의미는 “axis name으로 axis reference를 달라”.

```csharp
LMC_Frame.Name(..., axisName)
```

`axisName`, 예를 들면 `"a01"`을 넣어서 TCP request 패킷 `byte[]`를 만든다.

패킷은 대략 이렇게 생긴다.

```text
[0..1] command id      = 0x103C
[2..3] reserved        = 0
[4..5] payload length  = 80
[6..7] reference       = 0
[8.. ] axis name       = "a01" ASCII, 나머지 0 padding
```

그 다음:

```csharp
connection.Exchange(request)
```

이 패킷을 TCP로 보내고, 응답을 받아온다.

응답은 대략:

```text
[0..1] status/reserved
[2..3] response payload length
[4..7] reserved
[8.. ] response payload
[12..13] axis reference
```

그래서 바로 아래에서 이렇게 읽는 거다.

```csharp
return LMC_Frame.ReadUInt16(raw, LookupReferenceOffset);
```

`LookupReferenceOffset = 12`니까, 응답 raw byte 배열의 12번 offset에서 `ushort`를 읽어서 axis reference로 쓰는 구조다.

예:

```text

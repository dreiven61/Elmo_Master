
추가로 테스트 앱이 현재 라이브러리 API와 안 맞아서 빌드가 깨지던 부분도 같이 맞췄습니다. `LMC_RpcInitConnection` 인자 수와 존재하지 않는 그룹 API 호출을 정리했습니다.

검증:
- `LasalMotionControlLib.sln` 빌드 성공, 경고 0 / 오류 0
- `LasalMotionControlLibTestApp.sln` 빌드 성공, 경고 0 / 오류 0
- 전체 repo에서 `LmcMotionApi` 검색 결과 없음
- `git diff --check` 통과. 줄바꿈 CRLF 경고만 출력됨

커밋은 하지 않았습니다. 커밋할 때는 `bin` 아래 새 DLL들이 `.gitignore` 대상이라 `git add -f`로 포함해야 합니다.

> 패킷 캡쳐한거를 기반으로해서 PMAS가 Axis를 handler에 등록하고 MoveAbsolute 호출하는 방식에 대해서 설명해줄래. 패킷을 기반으로 대략적으로 어떤 구조인지 궁금해. 매번 Axis를 식별하는 이름이 넘어가는건지 아니면 미리 핸들러가 등록되면 핸들러 번호가 넘어가는건지 알고싶어

<details><summary>7 previous messages</summary>

> 패킷 기준으로 답하려면 먼저 캡쳐 파일과 기존 분석 자료를 찾아서, “축 이름으로 lookup하는 패킷”과 “MoveAbsolute 실행 패킷”의 payload를 분리해서 보겠습니다. 이전에 분석한 문서가 있으면 그걸 먼저 대조하고, 없으면 pcapng를 직접 파싱하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 패킷 자료와 현재 라이브러리 코드 양쪽을 다시 대조해서 답하겠습니다. 핵심은 “Move마다 이름이 가는지”라서, 초기 축 조회 패킷과 MoveAbsolute 패킷을 분리해서 보겠습니다.
>
> 이미 정리된 패킷 분석 문서에 답의 핵심이 있습니다. `GetAxisByName`은 이름을 80바이트 ASCII로 보내고, 이후 motion command는 offset 6의 2바이트 reference를 쓰는 구조로 정리되어 있습니다.
>
> 추가로 현재 C# 라이브러리도 같은 모델을 따릅니다. `LmcAxis` 생성 시 이름으로 reference를 한 번 얻고, 이후 `MoveAbsoluteEx`는 저장된 `AxisReference`로 `AxisMove` 프레임을 만듭니다.
>
> 캡처 원본도 있습니다. 이름 조회와 MoveAbsoluteEx가 같은 pcap 안에 있는지 실제 TCP payload를 짧게 덤프해서 확인해보겠습니다.
>
> 원본 pcap 덤프 기준으로도 구조가 명확합니다. 같은 캡처에서 `0x103C` 패킷은 `a01` 문자열을 포함하고, 그 다음 `0x209F MoveAbsoluteEx` 패킷은 이름 문자열 없이 `ref=0`과 숫자 파라미터만 들어 있습니다.
</details>

결론부터 말하면: **MoveAbsolute 호출 때마다 축 이름이 넘어가는 구조가 아닙니다.**
PMAS/MMCLib는 먼저 `GetAxisByName`으로 `a01` 같은 axis name을 보내서 **axis reference**를 받고, 이후 `MoveAbsoluteEx` 같은 motion command에는 그 reference 번호만 넣습니다.

패킷 기준 구조는 이렇습니다.

1. `0x103C GetAxisByName`
   - request payload offset `8`에 `ASCII[80]`로 axis name이 들어감.
   - 예: `a01`
   - response offset `12`에 `axis reference`가 옴.
   - 캡처 정리상 `a01=0`, `a02=1`, `a03=2`, `a04=3`입니다.
   근거: [PACKET_ANALYSIS.md](./LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md:89)

2. 그 다음 `0x202B AxisInfo`
   - 방금 받은 axis reference로 축 정보를 한 번 더 조회합니다.
   - 이건 축 단위/상태/interface 쪽 초기화 성격으로 보입니다.

3. `0x209F MoveAbsoluteEx`
   - request header offset `6`에 `UINT16 Axis reference`가 들어갑니다.
   - offset `8`부터는 position, velocity, acceleration, deceleration, jerk, direction, buffer mode, execute 같은 숫자 payload만 있습니다.
   - axis name 문자열은 없습니다.
   근거: [PACKET_ANALYSIS.md](./LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md:177)

원본 pcap도 직접 덤프해보면 같은 흐름입니다.

```text
#1 len=88 cmd=0x103C ... ascii=...a01...
#2 len=14 response ... offset 12 axis reference = 0
#3 len=20 cmd=0x202B ref=0
#5 len=64 cmd=0x209F ref=0 ... 숫자 payload only
```

즉 네가 말한 “핸들러 번호”에 가까운 건 PMAS 용어로는 **`hAxisRef` / axis reference**입니다. C API도 `MMC_MoveAbsoluteExCmd(hConn, hAxisRef, ...)` 형태라서, connection handle과 axis reference를 내부적으로 들고 있다가 motion command에 씁니다. 근거: [Elmo_API_MoveAbsoluteEX_ReadActualPosition_Response.md](./docs/Elmo_API_MoveAbsoluteEX_ReadActualPosition_Response.md:18)

주의할 점은 `hConn`은 TCP payload에 매번 필드로 들어가는 값이라기보다 라이브러리/소켓 세션 컨텍스트에 가깝고, 실제 motion request header에서 축 식별에 쓰이는 건 offset `6`의 `Axis reference`입니다. LASAL 이식 쪽도 PMAS처럼 맞추려면 `AxisRef -> 실제 LMCAxis client` 매핑 테이블을 두고, command마다 이 reference로 축을 선택해야 합니다. 단일 `LMCAxis`에 고정해서 호출하면 PMAS 구조와 다릅니다.

> AxisInfo나 MoveAbsoluteEx를 호출할때 ref 번호로 정보를 얻어오나? 파일 디스크립터랑 비슷하다고 보면되냐?

맞다. **파일 디스크립터(fd)랑 비슷한 개념으로 보면 된다.**
다만 OS fd처럼 “열린 파일 핸들”이라기보다는, PMAS/MMCLib 내부에서 **axis name을 숫자 reference로 resolve한 값**이다.

흐름은 이렇게 보면 된다.

1. `GetAxisByName("a01")`
   - 문자열 `a01`을 보냄
   - PMAS가 `axis reference = 0` 같은 번호를 반환

2. `AxisInfo(ref=0)`
   - 이 ref가 가리키는 축의 정보를 조회
   - 즉 `ref`를 key로 써서 PMAS 내부 axis table에서 축 정보를 가져오는 호출

3. `MoveAbsoluteEx(ref=0, position, velocity, ...)`
   - 정보를 “조회”하는 게 아니라, `ref=0` 축에 motion command를 넣는 호출
   - controller는 ref로 대상 축을 찾고, 그 축의 motion/function-block queue에 명령을 등록
   - 응답은 축 정보가 아니라 command 접수 결과, FB handle/status 성격

그래서 비유하면:

```text
axis name "a01"  ->  GetAxisByName  ->  axis ref 0

AxisInfo(0)      ->  0번 ref 축의 정보 조회
MoveAbsoluteEx(0)->  0번 ref 축에 이동 명령 실행
ReadPosition(0)  ->  0번 ref 축의 현재 위치 조회
```

중요한 차이는 이거다.
**ref는 매 호출마다 축 이름을 다시 보내지 않기 위한 숫자 식별자**다. 그래서 LASAL 이식에서도 PMAS처럼 하려면 `ref -> 실제 LMCAxis 인스턴스` 매핑이 있어야 한다. `AxisRef`를 읽어놓고 항상 같은 `LMCAxis`만 호출하면 PMAS 구조를 제대로 흉내낸 게 아니다.

> PC 프로그램에서는 이 핸들러를 어디에 저장해? 아까 말한 객체?

<details><summary>2 previous messages</summary>

> PC 쪽 코드에서 실제로 어디에 들고 있는지 확인해보겠습니다. PMAS WPF의 `ConnectData`, `SingleAxis` 생성부와 현재 이식 DLL의 `LMCAxis` 저장 방식을 같이 보겠습니다.
>
> PMAS WPF 기준으로는 핸들러를 앱 전역 변수에 따로 들고 있는 게 아니라 `PmasControllerContext.SingleAxis`가 가진 `MMCSingleAxis` 객체 안에 들어갑니다. 정확한 라인만 뽑아서 답하겠습니다.
</details>

맞다. PC 프로그램에서는 **`MMCSingleAxis` 객체 안에 axis reference가 저장된다**고 보면 된다.

PMAS WPF 기준 흐름은 이거다.

```csharp
SingleAxis = new MMCSingleAxis(axisName, Handle);
```

여기서 `Handle`은 연결 핸들, `axisName`은 `"a01"` 같은 축 이름이다. 이 생성자 안에서 PMAS DLL이 `GetAxisByName`을 호출해서 axis reference를 얻고, 그 값을 `MMCSingleAxis` 객체 내부에 보관한다. 앱 쪽에서는 그 값을 `SingleAxis.AxisReference`로 읽어 로그에 찍고 있다.
근거: [PmasControllerContext.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs:240)

이후 MoveAbsoluteEx 호출부는 axis name이나 ref를 다시 넘기지 않는다.

```csharp
Context.SingleAxis.MoveAbsoluteEx(...)
```

즉 `Context.SingleAxis` 객체가 이미 `hConn + hAxisRef`를 알고 있고, 메서드 호출 시 DLL 내부에서 그 reference를 패킷에 넣는다.
근거: [MainWindow.AxisOperations.cs](./Codex_PMAS_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs:145)

정리하면:

```text
PmasControllerContext
  ├─ Handle        = PMAS connection handle
  ├─ AxisName      = "a01"
  └─ SingleAxis    = MMCSingleAxis 객체
        ├─ AxisName
        ├─ AxisReference
        └─ MoveAbsoluteEx(), ReadStatus(), GetActualPosition() ...
```

그래서 네가 말한 “파일 디스크립터 같은 핸들러”는 **숫자 ref 자체**이고, PC 코드에서 그 ref를 들고 다니는 래퍼가 **`MMCSingleAxis` 객체**다. `ConnectData`나 `Context`는 그 객체를 보관하는 상위 컨테이너다.

> 그럼 클래스 객에가 이미 핸들러 axis name 이렇게 있으니 거기서 내부 메소드를 호출하는 구조네 니가 저ㅓ기 구조를 올려놨구만

맞다. 구조는 정확히 그거다.

```text
PC App
  -> Context.SingleAxis
      -> MMCSingleAxis 객체
          - connection handle
          - axis name
          - axis reference
          - DriveID 등
          - MoveAbsoluteEx(), ReadStatus(), AxisInfo() 같은 내부 메소드
```

사용자는 `Context.SingleAxis.MoveAbsoluteEx(...)`만 호출하지만, 실제로는 `MMCSingleAxis` 객체가 자기 안에 저장해둔 `connection handle + axis reference`를 써서 패킷을 만든다.

그래서 매번 `"a01"` 같은 이름을 넘기는 게 아니라:

```text
new MMCSingleAxis("a01", Handle)
  -> GetAxisByName("a01")
  -> AxisReference 저장

MoveAbsoluteEx(...)
  -> 저장된 AxisReference로 command 전송
```

이 구조 때문에 `ConnectData.SingleAxis.MoveAbsolute(...)`처럼 보이는 거고, `ConnectData`는 연결/축 객체를 들고 있는 컨텍스트, `SingleAxis`는 실제 축 핸들러 객체라고 보면 된다.

> 그래서 api 호출되면 인자를 따로 안줘도 바로 패킷에 생성되서 내보내지는거고

맞다. 정확히 그 구조다.

`MMCSingleAxis` 객체가 이미 아래 값을 들고 있으니까:

```text
connection handle
axis name
axis reference
```

사용자가 이렇게 호출하면:

```csharp
Context.SingleAxis.MoveAbsoluteEx(position, velocity, acc, dec, jerk, direction, bufferMode);
```

메소드 인자로는 motion 값만 받지만, 내부에서는 객체가 저장해둔 `axis reference`를 자동으로 붙여서 패킷을 만든다.

패킷 관점으로는 대략 이렇게 된다.

```text
MoveAbsoluteEx(...)
  -> command id = 0x209F
  -> axis reference = this.AxisReference
  -> payload = position, velocity, acc, dec, jerk, direction, bufferMode, execute
  -> TCP send
```

그래서 호출부에 `axisName`이나 `axisRef`가 안 보여도 실제 패킷에는 들어간다.
단, 들어가는 건 `"a01"` 문자열이 아니라, 처음 `GetAxisByName("a01")`에서 받아서 객체에 저장해둔 숫자 `AxisReference`다.

> 지금 우리가 구현한 api도 똑같은 구조로 되있다고 볼수 있냐?

<details><summary>1 previous message</summary>

> 현재 구현 기준으로 다시 확인해서 답하겠습니다. 같은 구조인지 보려면 `LMCAxis` 생성자와 `MoveAbsoluteEx` 프레임 생성 경로만 보면 됩니다.
</details>

큰 구조는 **같다고 보면 된다.**
현재 우리가 구현한 DLL 쪽 `LMCAxis`도 객체가 `AxisName`, `AxisReference`, connection을 들고 있고, API 메소드 호출 시 그 값을 자동으로 패킷에 넣는다.

현재 구현 흐름:

```text
new LMCAxis(connection, "a01")
  -> AxisName = "a01"
  -> GetAxisByName 패킷 전송
  -> 응답 offset 12에서 AxisReference 읽음
  -> AxisInfo(AxisReference) 호출

axis.LMC_MoveAbsoluteExCmd(...)
  -> 저장된 AxisReference 사용
  -> LMC_Frame.AxisMove(command, AxisReference, ...)
  -> 패킷 생성 후 전송
```

코드상 근거는 여기다.

- `LMCAxis`가 `AxisName`, `AxisReference`를 보관: [LmcAxis.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs:15)
- 생성자에서 `ResolveAxisReference(axisName)` 호출 후 `AxisInfo(AxisReference)` 호출: [LmcAxis.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs:26)
- `ResolveAxisReference`가 `GetAxisByName` 프레임을 보내고 응답에서 ref를 읽음: [LmcAxis.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs:131)
- `Move()`가 저장된 `AxisReference`로 `AxisMove` 프레임 생성: [LmcAxis.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs:153)

다만 **완전히 PMAS와 동일한 wire format은 아니다.**

현재 `LMC_API_Delivery`는 구조는 PMAS처럼 “객체가 ref를 들고 내부 메소드가 패킷 생성”인데, motion payload는 LASAL용 단위 변환을 거쳐 `DINT/int` 32바이트로 보내고 있다. PMAS 캡처의 `MoveAbsoluteEx`는 `LREAL/double` 기반 56바이트 payload였다.

그리고 `Codex_LASAL_WPF`에 있던 예전 더미 구현은 더 다르다. 거기는 `AxisReference`를 저장하긴 하지만 `ForceZeroAxisRefForCommands = true`라서 command 전송 시 axis ref를 강제로 `0`으로 바꾸는 임시 코드가 남아 있다. 그래서 지금 기준으로 신뢰해야 하는 쪽은 `LMC_Library/LMC_API_Delivery/src`다.

> 기존의 Codex_LASAL_WPF는 무시하자 그건 더미용으로 개발한거라 이번에 개발한게 진짜임. 일단 함수명이랑 api 구조를 동일하게 맞출 필요가 있어. 우리가 만든 단위변환은 유저가 사용하기 위함이지 api내부에서 호출되는 구조로 만들면 안돼. 내부에서 유닛변환 하는 소스코드는 걷어 내도록하자. 그리고 지금 우리가 하는 구조설계는 무조건 md파일을 만들어서 히스토리를 git에 남겨서 진행할 수 있게 작성하고 코드를 수정하도록해.

<details><summary>37 previous messages</summary>

> 요구사항은 코드 동작 변경까지 포함된 것으로 보고 진행하겠습니다. 먼저 현재 작업트리와 실제 DLL 프로젝트 구조를 확인하고, 설계 히스토리 md를 만든 뒤 그 기준으로 단위변환 내부 호출을 제거하겠습니다.

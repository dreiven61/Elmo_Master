axisName = "a01"
request = GetAxisByName("a01")
response raw[12..13] = 00 00
AxisReference = 0

axisName = "a02"
response raw[12..13] = 01 00
AxisReference = 1
```

즉 이 코드는 한 줄로 말하면:

```text
"a01"이라는 이름을 컨트롤러에 보내서, 앞으로 쓸 숫자 축 핸들러를 받아온다.
```

> 지금 api는 기능이 다 완료됐나? Elmo 기준으로 다 정상적으로 구현돼어있는지 체크해봐

<details><summary>3 previous messages</summary>

> 현재 API가 “완료”라고 볼 수 있는지, Elmo/PMAS 패킷 기준과 현재 `LMC_API_Delivery` 구현을 직접 대조해서 판단하겠습니다. 결론은 소스, 패킷 분석 문서, 빌드 상태 기준으로 나눠서 말하겠습니다.
>
> 소스와 패킷 분석을 대조해보니 방향이 분명합니다. 지금 DLL은 “Elmo 원본 호환 API”가 아니라 “LASAL DINT 패킷용 부분 구현”입니다. 빌드와 기능 커버리지까지 확인해서 완료/미완료를 구분하겠습니다.
>
> 빌드는 통과했지만, Elmo 원본 호환성 기준으로는 핵심 motion frame 타입/길이가 맞지 않습니다. 설계 문서도 현재 DLL이 LASAL DINT API라고 명시하고 있어서, “Elmo 기준 전체 완료”라고 말하면 틀립니다. 최종 판단 근거를 정리하겠습니다.
</details>

아니. **Elmo 기준으로는 다 정상 구현됐다고 보면 안 된다.**
현재 `LMC_API_Delivery`는 빌드는 되지만, Elmo/MMCLib wire 호환 API가 아니라 **LASAL internal DINT 패킷용 부분 구현**이다.

확인 결과:

- 빌드: 통과
  - `dotnet build ...\LasalMotionControlLib.sln -c Release`
  - 경고 0, 오류 0

- 객체 구조: 맞음
  - `LMCSingleAxis`가 `GetAxisByName`으로 reference 받아서 저장
  - 이후 motion/status 호출에서 저장된 reference 사용
  - 이 구조 자체는 Elmo 방식과 유사함

- 패킷 형식: Elmo 기준과 다름
  - 현재 문서/코드는 `DINT int` payload 기준임: [DINT_PACKET_MAP.txt](./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt:1)
  - Elmo 캡처 기준 `MoveAbsoluteEx`는 payload `56` bytes, `LREAL[5] + DINT[3]`
  - 현재 코드는 payload `32` bytes, `int32` 값 8개
  - 근거: [PACKET_ANALYSIS.md](./LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md:181), [LmcProtocol.cs](./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:161)

명확히 안 맞는 부분:

- `MoveAbsoluteEx`, `MoveRelativeEx`
  - Elmo: `LREAL/double` 5개 + DINT
  - 현재: `DINT/int`만 전송

- `MoveVelocityEx`
  - Elmo 캡처: payload length field `48`, 실제 TCP payload `49`, `LREAL[4] + DINT[2] + BYTE`
  - 현재: payload `24`, `DINT/int`만 전송

- `Stop`, `GroupStop`
  - Elmo: deceleration/jerk가 `REAL/float`
  - 현재: `DINT/int`

- `MoveLinearAbsoluteEx`
  - Elmo: payload `304`, 실제 request `312`, `LREAL` 중심
  - 현재: payload `96`, `DINT/int` 중심

- `ReadActualPosition`
  - Elmo 응답: position이 `LREAL/double`
  - 현재 코드: offset 8에서 `Int32`로 읽음

- 기능 커버리지
  - Elmo API 전체 기준으로는 아주 일부만 있음.
  - 현재 있는 것: 연결, axis/group name lookup, AxisInfo, Power, Reset, Stop, MoveAbsolute/Relative/Velocity, ReadStatus, ReadPosition, 일부 group 명령
  - 없는 것: parameter read/write, bool parameter, SDO upload/download, op mode, status register, homing, override, PI, recorder, callback/event, wait/condition, 많은 group/robot/kinematics 계열

결론: **LASAL DINT 전용 API로는 뼈대가 잡혀 있고 빌드도 된다. 하지만 Elmo/MMCLib 기준으로 “기능 완료”나 “정상 호환 구현”은 아니다.**
Elmo 기준으로 맞추려면 `LMC_Frame`을 캡처 분석의 LREAL/REAL layout으로 다시 맞추고, 응답 파서도 Elmo 응답 구조 기준으로 바꿔야 한다.

> Elmo랑 똑같이 구현할 필요가 없어. Lasal 프로그램에 맞춰서 구현되면 되는거야 굳이 똑같은 양식일 필요는 없고 유사한 구조를 가진 상태로 동작되면 되는거지

맞다. 기준은 **Elmo wire format 복제**가 아니라 **LASAL 프로그램이 받을 패킷 계약**이다.

현재 구조는 이 방향으로 맞춰져 있다.

- Elmo처럼 객체가 connection과 axis reference를 들고 있음
- `LMCSingleAxis("a01")` 생성 시 name lookup으로 reference 받음
- 이후 `MoveAbsoluteEx`, `ReadStatus`, `GetActualPosition`은 객체가 가진 reference로 패킷 생성
- payload는 Elmo의 `LREAL` 구조가 아니라 LASAL parser에 맞춘 `DINT/int`
- API 내부 단위변환 없음
- `LMC_Units`는 선언만 있고 내부 코드에서 사용 안 함

그래서 현재 판단은 이렇게 해야 한다.

**Elmo 호환 API인가?**
아님.

**LASAL용 LMC API 구조로는 방향이 맞나?**
맞음.

**완료라고 볼 수 있나?**
아직은 “소스 빌드 가능 + 기본 구조 구현” 상태고, 완료 판단은 LASAL 쪽과 아래 항목을 맞춰봐야 한다.

- LASAL `TCPMotionInterface`가 현재 command id, payload length, offset을 그대로 읽는지
- `AxisReference`로 실제 `_LMCAxis1..4`를 선택하는 구조가 있는지
- `MoveAbsoluteEx`, `MoveRelativeEx`, `MoveVelocityEx`, `Stop`, `ReadStatus`, `GetActualPosition` 응답 길이/offset이 PC API와 맞는지
- 필요한 API 범위가 현재 구현된 함수들로 충분한지

즉 다음 검증 기준은 Elmo가 아니라 **`DINT_PACKET_MAP.txt`와 LASAL parser의 offset 일치 여부**다. 이게 맞으면 유사 구조 API로 동작 가능하다.

> AxisGroup도 AxisByName이랑 똑같은 방식으로 handler 등록하고 동작되는건가?

응. **AxisGroup도 기본 구조는 AxisByName이랑 같다.**

단축 axis:

```text
GetAxisByName("a01")
  -> axis reference 반환
  -> LMCSingleAxis.AxisReference에 저장
  -> 이후 MoveAbsoluteEx / ReadStatus 등에서 reference 사용
```

그룹 axis:

```text
GetGroupByName("v01")
  -> group reference 반환
  -> LMCGroupAxis.GroupReference에 저장
  -> 이후 GroupEnable / GroupStop / MoveLinearAbsoluteEx 등에서 reference 사용
```

현재 코드도 그렇게 되어 있다.

```csharp
GroupName = groupName;
GroupReference = ResolveGroupReference(groupName);
```

그리고 내부에서:

```csharp
LMC_Frame.Name(LMC_CommandId.GetGroupByName, groupName)
```

으로 그룹 이름을 보내고, 응답 offset `12`에서 reference를 읽는다.

차이는 대상만 다르다.

```text
Axis:  "a01" -> axis ref, 보통 0,1,2,3
Group: "v01" -> group ref, 캡처 기준 0x0100
```

즉 `AxisGroup`도 **이름을 매번 보내는 게 아니라, 처음에 group name으로 reference를 받아서 객체에 저장하고 이후 명령에서 그 reference를 쓰는 구조**다.

> RpcInitConnection은 TCP랑 동일한건가 TCP랑 머가 다른거지?

현재 우리가 만든 `RpcInitConnection`은 **사실상 TCP connect 래퍼**다.

현재 코드 기준으로는:

```csharp
client = new TcpClient(localEndPoint);
client.Connect(remoteAddress, remotePort);
```

이게 전부다. 이름은 `RpcInitConnection`이지만, 실제로는 TCP 소켓을 열고 연결하는 함수다.

차이는 개념상 이렇게 보면 된다.

```text
TCP
  - 통신 transport 계층
  - IP/Port로 socket 연결
  - byte[]를 send/receive

RpcInitConnection
  - API 레벨 연결 초기화 함수 이름
  - 내부에서 TCP 연결을 열고
  - 이후 LMC command packet을 주고받을 준비를 함
```

Elmo/PMAS 원본에서는 `RpcInitConnection`이 단순 TCP connect보다 더 의미가 있다. 캡처상 RPC 초기화 패킷도 따로 있고, callback 주소/connection handle 같은 개념이 붙는다.

그런데 현재 LASAL용 API에서는 그걸 그대로 복제하지 않고:

```text
RpcInitConnection(...)
  -> TCP 연결 열기
  -> 이후 LMC_Frame 패킷 송수신
```

정도로 단순화해 둔 상태다.

그래서 답은:

- **네트워크 레벨에서는 TCP 연결이다.**
- **API 이름/역할상으로는 “LMC 세션 초기화” 함수다.**
- **현재 구현에서는 Elmo식 RPC handshake를 하지 않는다.**
- LASAL 서버가 그냥 TCP socket으로 command frame을 받는 구조라면 이 방식이 맞다.

> Elmo식 RPC handshake를 하면 TCP랑 무슨 차이가 있지?

Elmo식 RPC handshake를 해도 **물리 통신은 TCP**다.
차이는 TCP 위에 **Elmo/MMCLib 전용 세션 절차**가 하나 더 올라간다는 점이다.

비유하면:

```text
TCP connect
  = 전화선 연결

Elmo RPC handshake
  = 연결된 전화에서 "나 누구고, 콜백은 여기고, 이 세션 번호로 통신하자"라고 약속
```

TCP만 하면 가능한 건 이것뿐이다.

```text
IP:Port 연결
byte[] 송신
byte[] 수신
```

Elmo RPC handshake를 하면 추가로 이런 의미가 생긴다.

```text
1. 연결 세션 등록
2. host/client 식별
3. callback IP/port 전달
4. 이벤트/비동기 응답 경로 등록
5. connection handle 또는 내부 session context 생성
6. 이후 command가 어느 client/session에서 온 건지 구분
```

패킷 캡처 기준으로 Elmo에는 `RpcInitConnection` 단계가 따로 있다.

```text
RpcInitConnection step 1  -> command 0x8080, 9-byte request
RpcInitConnection callback -> command 0x405C, 20-byte request
```

즉 Elmo는 TCP socket을 연 뒤에도 “이 연결은 MMCLib RPC 세션이다”라고 컨트롤러에 등록한다.

현재 LASAL API에서는 이걸 생략했다.

```text
현재 LASAL 방식:
TCP connect
  -> 바로 GetAxisByName / MoveAbsoluteEx 같은 command frame 송신
```

# Elmo Master History 260609 Part 04

- Source: `C:\work\Elmo\Elmo_Master\docs\history\Elmo_Master_history_260609.md`
- Lines: 601-741

이 부분은 Sigmatek의 `_LMCPROF_SMOOTH_CUBIC`, `_LMCPROF_SMOOTH_QUINT`, `_LMCPROF_SMOOTH_CLOTH`와 비교할 수 있는 영역입니다.

**가장 큰 차이**
Sigmatek은 사용자가 보는 개념이 더 직관적으로:

```text
TransMode = 어떤 보간 곡선인가
TransRadius / Radius = 어느 영역에서 보간할 것인가
```

Elmo는:

```text
BufferMode = 전환점 속도를 어떻게 잡을 것인가
TransitionMode = 전환곡선 형상을 어떻게 만들 것인가
fTransitionParameter = 모드별 전환 파라미터
```

입니다.

즉 Sigmatek은 **sphere 기반 path blending** 표현이고,
Elmo는 **PLCopen FB queue + velocity blending + transition curve** 표현입니다.

**주의할 점**
Elmo에서 `Blending`이라는 단어만 보고 Sigmatek의 `TransRadius` 방식과 1:1 대응시키면 안 됩니다.

예를 들어:

```text
Elmo MC_BLENDING_NEXT_MODE
```

이건 “다음 명령 속도로 전환한다”는 의미이지, 그 자체가 cubic/quint/clothoid 보간을 의미하지 않습니다.

Elmo에서 Sigmatek의 smooth rounding에 대응되는 것은 오히려 이 조합입니다.

```text
eBufferMode = MC_BLENDING_*
eTransitionMode = MC_TM_CORNER_DIST_CV_POLYNOM5 / POLYNOM7 / PLN8 ...
fTransitionParameter = corner distance / radius / deviation
```

**결론**
Elmo도 블렌딩 기술이 있습니다. 오히려 Multi-axis transition mode만 보면 polynomial mode가 세분화되어 있습니다.

다만 차이는 이겁니다.

- Sigmatek: `Radius/TransRadius` 중심의 tolerance sphere 보간
- Elmo: `BufferMode`로 전환 속도 결정 + `TransitionMode/fTransitionParameter`로 전환곡선 생성

그래서 Elmo에서 Sigmatek과 비슷한 블렌딩을 만들려면 `BufferMode`만 설정하면 부족하고, **반드시 `TransitionMode`와 `fTransitionParameter`까지 같이 봐야 합니다.**

> MMC_RpcInitConnection
> Initiates RPC connection to Maestro server.
> MMC_LIB_API int MMC_RpcInitConnection(
> IN MMC_CONNECTION_TYPE eType,
> IN MMC_CONNECTION_PARAM_STRUCT sConnParam,
> IN MMC_CB_FUNC pCbFunc ,
> IN char* cpHostIPAddr,
> OUT MMC_CONNECT_HNDL* pHndl
> );
> Motion Mode NC – Not relevant Distributed – not relevant
> Source GMAS\includes\MMC_general_API.h
> Parameters
> eType
> [IN] Connection type (IPC(inter process communication) or RPC(remote procedure
> calls)) with the following possible MMC_CONNECTION_TYPE enumerator values:
> MMC_RPC_CONN_TYPE RPC connection to MMC server
> MMC_IPC_CONN_TYPE IPC connection to MMC server
>
> 이 함수 보면 RPC랑 IPC랑 있는데 어떻게 다른거야?

간단히 말하면 **RPC는 네트워크越 통신**, **IPC는 같은 장비/같은 OS 내부 프로세스 간 통신**입니다.

**RPC**
`Remote Procedure Call`

- PC 프로그램에서 Maestro 컨트롤러의 MMC 서버로 명령 보낼 때 사용
- 보통 Ethernet/TCP/IP 기반
- `cpHostIPAddr`에 Maestro IP를 넣는 구조
- 예: PC WPF 프로그램 → `192.168.1.3` Maestro → Motion API 실행
- 네트워크 지연, TCP 처리, 서버 응답 시간이 포함됨

즉 우리가 지금까지 패킷 캡처한 구조는 거의 이쪽입니다.

```text
PC App
  -> TCP/IP
  -> Maestro MMC Server
  -> Motion Engine
```

**IPC**
`Inter Process Communication`

- 같은 장비 내부에서 프로세스끼리 통신할 때 사용
- 네트워크를 타지 않음
- Maestro 내부에서 실행되는 프로그램이 Maestro 내부 MMC 서버와 통신할 때 쓰는 구조로 보면 됨
- RPC보다 통신 오버헤드가 작고, 네트워크 지연이 없음
- 단, 외부 PC에서 Maestro를 제어하는 용도는 아님

구조는 대략 이렇습니다.

```text
Maestro 내부 Program
  -> IPC
  -> Maestro MMC Server
  -> Motion Engine
```

**차이 핵심**
| 구분 | RPC | IPC |
|---|---|---|
| 의미 | Remote Procedure Call | Inter Process Communication |
| 위치 | 외부 PC ↔ Maestro | Maestro 내부 프로세스 ↔ Maestro 내부 서버 |
| 통신 | 네트워크/TCP 기반 | 로컬 프로세스 간 통신 |
| IP 필요 | 필요 | 보통 불필요 또는 무시 |
| 지연 | 네트워크 지연 포함 | 상대적으로 낮음 |
| 우리가 쓰는 방식 | 대부분 RPC | 아님 |

**이 함수에서의 의미**
`MMC_RpcInitConnection()`은 이름은 `RpcInitConnection`이지만 `eType`으로 RPC/IPC 둘 다 선택하게 되어 있습니다.

```c
MMC_RPC_CONN_TYPE  // RPC connection to MMC server
MMC_IPC_CONN_TYPE  // IPC connection to MMC server
```

즉 함수 이름은 RPC지만 실제로는 “MMC 서버 연결 초기화 함수”에 가깝고, `eType`으로 원격 연결인지 내부 연결인지 고르는 구조입니다.

**너희 PC 기반 PMAS/WPF 프로그램 기준**
거의 확실히 `MMC_RPC_CONN_TYPE`을 써야 합니다.

왜냐하면 PC에서 Maestro IP로 붙는 구조이기 때문입니다.

```c
eType = MMC_RPC_CONN_TYPE
cpHostIPAddr = "192.168.1.3"
```

`MMC_IPC_CONN_TYPE`은 Maestro 내부에서 돌아가는 C/C++/IEC 프로그램이 MMC 서버에 붙을 때 쓰는 쪽으로 봐야 합니다.

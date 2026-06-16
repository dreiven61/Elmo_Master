# 24.8 The MMCGroupAxis class - API 분석

- 원본 장: `Chapter 24 Programming in C++`
- 시작 PDF 페이지: 2055
- 원문 위치: [24.8 The MMCGroupAxis class](../chunks/079_p2055-p2094_24.8-The-MMCGroupAxis-class.md#pdf-page-2055)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `24.8` | 2055 | `The MMCGroupAxis class` | The MMCGroup 축 class 작업을 수행하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 24.8 The MMCGroupAxis class

- PDF 페이지: 2055
- 원문 위치: [24.8 The MMCGroupAxis class](../chunks/079_p2055-p2094_24.8-The-MMCGroupAxis-class.md#pdf-page-2055)
- 기능 설명: The MMCGroup 축 class 작업을 수행하는 API입니다.

#### 시그니처

```c
int WaitFbDone(unsigned int break_state, CMMCSingleAxis *
sng_axis);
```
```c
void initAdminSingleAxis(void);
void endAdminSingleAxis(void);
```
```c
void initAdminMultiAxis();
void endAdminMultiAxis(void);
```
```c
void SnroEnableDisableMotionEndedEvent(int);
void EnableDisableMotionEndedEvent(void);
```
```c
void SnroMoveAbsolute(int);
void MoveAbsoluteMoves(void);
```
```c
void SnroDepthName(int);
void DepthName(void);
```
```c
void SnroConnection(int);
void ConnectionTypeAndNum(void);
```
```c
void SendReciveFromEthercat(int NumAmp);
void SetGetDefDigOutput(void);
```

#### 구조체/인자

##### `MMC_MOTIONPARAMS_GROUP`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dAuxPoint[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `double dEndPoint[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucSuperimposed;` | uc Superimposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned int m_uiExecDelayMs;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

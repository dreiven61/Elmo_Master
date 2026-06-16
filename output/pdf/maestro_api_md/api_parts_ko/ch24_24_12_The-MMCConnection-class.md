# 24.12 The MMCConnection class - API 분석

- 원본 장: `Chapter 24 Programming in C++`
- 시작 PDF 페이지: 2210
- 원문 위치: [24.12 The MMCConnection class](../chunks/083_p2210-p2249_24.12-The-MMCConnection-class.md#pdf-page-2210)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `24.12` | 2210 | `The MMCConnection class` | The MMCConnection class 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 24.12 The MMCConnection class

- PDF 페이지: 2210
- 원문 위치: [24.12 The MMCConnection class](../chunks/083_p2210-p2249_24.12-The-MMCConnection-class.md#pdf-page-2210)
- 기능 설명: The MMCConnection class 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int WaitFbDone(unsigned int break_state, CMMCSingleAxis * sng_axis);
void initAdminSingleAxis(void);
```
```c
void endAdminSingleAxis(void);
void initAdminMultiAxis();
```
```c
void endAdminMultiAxis(void);
void SnroEnableDisableMotionEndedEvent(int);
```
```c
void EnableDisableMotionEndedEvent(void);
void SnroMoveAbsolute(int);
```
```c
void MoveAbsoluteMoves(void);
void SnroDepthName(int);
```
```c
void DepthName(void);
void SnroConnection(int);
```
```c
void ConnectionTypeAndNum(void);
void SendReciveFromEthercat(int NumAmp);
```
```c
void SetGetDefDigOutput(void);
int CallbackFunc(unsigned char* recvBuffer, short
recvBufferSize,void* lpsock);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

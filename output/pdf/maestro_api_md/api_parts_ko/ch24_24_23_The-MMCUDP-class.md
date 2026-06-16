# 24.23 The MMCUDP class - API 분석

- 원본 장: `Chapter 24 Programming in C++`
- 시작 PDF 페이지: 2353
- 원문 위치: [24.23 The MMCUDP class](../chunks/087_p2353-p2392_24.23-The-MMCUDP-class.md#pdf-page-2353)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `24.23` | 2353 | `The MMCUDP class` | The MMCUDP class 작업을 수행하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 24.23 The MMCUDP class

- PDF 페이지: 2353
- 원문 위치: [24.23 The MMCUDP class](../chunks/087_p2353-p2392_24.23-The-MMCUDP-class.md#pdf-page-2353)
- 기능 설명: The MMCUDP class 작업을 수행하는 API입니다.

#### 시그니처

```c
void UDPDemoClient()
{
static EDEMO_SOCK_STATE eState = eSOCKET_CONNECT_STATE;
static CMMCUDP udpClient;
static dsock_msg_t msg;
static int iCounter = 0;
int rc;
bool bWait;
switch (eState) {
case eSOCKET_CONNECT_STATE: //connect
rc = udpClient.Connect(g_szIpAddress, g_usPort, bWait);
```
```c
void UDPDemoServer()
{
static EDEMO_SOCK_STATE eState = eSOCKET_CREAT_STATE;
static CMMCUDP udpServer;
static dsock_msg_t msg;
int rc;
switch (eState) {
case eSOCKET_CREAT_STATE:
//create listener.
rc = udpServer.Create(g_usPort); //no callback, normal mode of
operation
fprintf(stderr, "%s, Create: rc = %d.\n", __func__, rc);
```
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

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

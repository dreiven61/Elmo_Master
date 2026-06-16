# 24.21 The MMCEIPDataType class - API 분석

- 원본 장: `Chapter 24 Programming in C++`
- 시작 PDF 페이지: 2338
- 원문 위치: [24.21 The MMCEIPDataType class](../chunks/086_p2323-p2352_24.18-The-CMMCBulkRead-class.md#pdf-page-2338)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `24.21` | 2338 | `The MMCEIPDataType class` | The MMCEIPData Type class 작업을 수행하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 24.21 The MMCEIPDataType class

- PDF 페이지: 2338
- 원문 위치: [24.21 The MMCEIPDataType class](../chunks/086_p2323-p2352_24.18-The-CMMCBulkRead-class.md#pdf-page-2338)
- 기능 설명: The MMCEIPData Type class 작업을 수행하는 API입니다.

#### 시그니처

```c
void terminate_application(int signum);
///////////////////////////////////////////////////////////////////////////
// External Functions //
///////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////
// Implementation //
///////////////////////////////////////////////////////////////////////////
/*
* general implementation of EtherNetIP call-back function.
*/
int fnCallback(unsigned char* ucBuffer, short sReqID, void* pSock)
{
unsigned char ucEventID = ucBuffer[2];
switch (ucEventID)
{
case NM_REQUEST_RESPONSE_RECEIVED:
//printf("NM_REQUEST_RESPONSE_RECEIVED: sReqID = %d\n", sReqID);
```
```c
int MainInit() {
long time;
struct sigaction stSigAction;
//whenever a signal is caught, call terminate_application function
stSigAction.sa_handler = terminate_application;
sigaction(SIGINT, &stSigAction, NULL);
```
```c
int OpenSessionPP() {
printf("Opening CPP EIP session\n");
```
```c
int CloseSessionPP() {
printf("Free EIP session memory\n");
```
```c
int AdpTESTPP() {
int iHBCount;
short iBuffer[7];
static short iVar=0;
g_adpPlc2Gmas.EipGetTag(iBuffer);
```
```c
int AdpInitPP() {
int rc = g_adpPlc2Gmas.EipTagInit("adp_plc2gmas");
```
```c
int DevTESTPP() {
char iBuffer[7];
char iVar;
short sReplyStatus;
//
//read device tag asynchronous
//
//single example
if(!g_devPlc2Gmas[eSINGLE_INDEX].EipIsWaiting())
{
g_devPlc2Gmas[eSINGLE_INDEX].EipGetTag(NULL);
```
```c
int DevInitPP() {
int rc = g_devPlc2Gmas[eARR_INDEX].EipTagInit("dev_plc2gmas");
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

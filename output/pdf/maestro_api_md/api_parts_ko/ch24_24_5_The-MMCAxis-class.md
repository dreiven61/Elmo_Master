# 24.5 The MMCAxis class - API 분석

- 원본 장: `Chapter 24 Programming in C++`
- 시작 PDF 페이지: 1984
- 원문 위치: [24.5 The MMCAxis class](../chunks/076_p1982-p1992_24.4.26-GetAxisError-ReadAxisError.md#pdf-page-1984)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `24.5.1` | 1985 | `MMC_GetAxisByNameCmd` | 조회 축 By Name 값/상태를 조회하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 24.5.1 DisableMotionEndedEvent

- PDF 페이지: 1985
- 원문 위치: [24.5.1 DisableMotionEndedEvent](../chunks/076_p1982-p1992_24.4.26-GetAxisError-ReadAxisError.md#pdf-page-1985)
- 기능 설명: 조회 축 By Name 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void DisableMotionEndedEvent(
) throw (CMMCException);
```
```c
void EnableMotionEndedEvent(
) throw (CMMCException);
```
```c
void EnableDisableMotionEndedEvent(void)
// ========================================
{
int loopInd;
printf("\n %s:", __func__);
```
```c
void SetDefaultManufacturerParameters(
) throw (CMMCException);
```
```c
void SetDefManufact(void)
// =========================
{
unsigned long ulong;
printf("\n %s:", __func__);
```
```c
int GetAxisByName(
const char* cName
) throw (CMMCException);
```
```c
Python Definition def MMC_GetAxisByNameCmd(hConn, pInParam, pOutParam):
return _mmcpp_lib.MMC_GetAxisByNameCmd(hConn, pInParam,
pOutParam)
```
```c
void InitAxisData(
const char* cName,
MMC_CONNECT_HNDL uHandle
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

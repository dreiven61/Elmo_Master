# 11.6 PI Functions - API 분석

- 원본 장: `Chapter 11 Process Image(PI)`
- 시작 PDF 페이지: 1099
- 원문 위치: [11.6 PI Functions](../chunks/043_p1093-p1131_11.2-Variable-Types.md#pdf-page-1099)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `11.6.1` | 1100 | `MMC_BeginRecordingEx` | Begin 기록 Ex 작업을 수행하는 API입니다. | Motion Mode NC - N/A Distributed - N/A |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 11.6.1 MMC_BeginRecordingEx

- PDF 페이지: 1100
- 원문 위치: [11.6.1 MMC_BeginRecordingEx](../chunks/043_p1093-p1131_11.2-Variable-Types.md#pdf-page-1100)
- 기능 설명: Begin 기록 Ex 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - N/A Distributed - N/A

#### 시그니처

```c
MMC_LIB_API int MMC_BeginRecordingCmdEx(
IN MMC_CONNECT_HNDL hConn,
IN MMC_BEGIN_RECORDING_IN* pInParam,
OUT MMC_BEGIN_RECORDING_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_BEGIN_RECORDING_IN`
| 필드 | 해석 |
|---|---|
| `unsigned long uiRg;` | ui Rg 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long uiRl;` | ui Rl 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long uiRc;` | ui Rc 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long uiRv[NC_MAX_REC_SIGNALS_NUM];` | 길이, 크기 또는 개수 값입니다. |
| `unsigned long uiRp[NC_MAX_REC_PARAMS_NUM];` | 파라미터 식별자 또는 파라미터 값입니다. |

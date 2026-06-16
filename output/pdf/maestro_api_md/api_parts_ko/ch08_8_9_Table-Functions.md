# 8.9 Table Functions - API 분석

- 원본 장: `Chapter 8 Position, Velocity, Time (PVT) Motion`
- 시작 PDF 페이지: 855
- 원문 위치: [8.9 Table Functions](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-855)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `8.9.1` | 856 | `MMC_TABLE_LIST_OUT` | TABLE LIST OUT 작업을 수행하는 API입니다. | - |
| `8.9.2` | 857 | `MMC_TABLE_LIST_IN` | TABLE LIST IN 작업을 수행하는 API입니다. | - |
| `8.9.3` | 858 | `MMC_TABLE_DATA_OUT` | TABLE DATA OUT 작업을 수행하는 API입니다. | - |
| `8.9.4` | 859 | `MMC_TABLE_DATA_IN` | TABLE DATA IN 작업을 수행하는 API입니다. | - |
| `8.9.5` | 860 | `MMC_GetTableList` | 조회 테이블 List 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `8.9.6` | 862 | `MMC_GetTableInfo` | 조회 테이블 정보 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 8.9.1 MMC_TABLE_LIST_OUT

- PDF 페이지: 856
- 원문 위치: [8.9.1 MMC_TABLE_LIST_OUT](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-856)
- 기능 설명: TABLE LIST OUT 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 8.9.2 MMC_TABLE_LIST_IN

- PDF 페이지: 857
- 원문 위치: [8.9.2 MMC_TABLE_LIST_IN](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-857)
- 기능 설명: TABLE LIST IN 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 8.9.3 MMC_TABLE_DATA_OUT

- PDF 페이지: 858
- 원문 위치: [8.9.3 MMC_TABLE_DATA_OUT](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-858)
- 기능 설명: TABLE DATA OUT 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 8.9.4 MMC_TABLE_DATA_IN

- PDF 페이지: 859
- 원문 위치: [8.9.4 MMC_TABLE_DATA_IN](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-859)
- 기능 설명: TABLE DATA IN 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 8.9.5 MMC_GetTableList

- PDF 페이지: 860
- 원문 위치: [8.9.5 MMC_GetTableList](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-860)
- 기능 설명: 조회 테이블 List 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetTableList(
IN MMC_CONNECT_HNDL hConn,
IN MMC_TABLE_LIST_IN* pInParam,
OUT MMC_TABLE_LIST_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_TABLE_LIST_IN`
- 구조체 필드를 추출하지 못했습니다.

##### `MMC_TABLE_LIST_OUT`
- 구조체 필드를 추출하지 못했습니다.

### 8.9.6 MMC_GetTableInfo

- PDF 페이지: 862
- 원문 위치: [8.9.6 MMC_GetTableInfo](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-862)
- 기능 설명: 조회 테이블 정보 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetTableInfo(
IN MMC_CONNECT_HNDL hConn,
IN MMC_TABLE_DATA_IN* pInParam,
OUT MMC_TABLE_DATA_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_TABLE_DATA_IN`
- 구조체 필드를 추출하지 못했습니다.

##### `MMC_TABLE_DATA_OUT`
- 구조체 필드를 추출하지 못했습니다.

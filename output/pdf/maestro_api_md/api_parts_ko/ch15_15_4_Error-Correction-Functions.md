# 15.4 Error Correction Functions - API 분석

- 원본 장: `Chapter 15 Error Correction Mechanism`
- 시작 PDF 페이지: 1311
- 원문 위치: [15.4 Error Correction Functions](../chunks/052_p1303-p1326_Chapter-15-Error-Correction-Mechanism.md#pdf-page-1311)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `15.4.1` | 1312 | `MMC_LoadErrorCorrTableCmd` | 로드 오류 Corr 테이블 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `15.4.2` | 1315 | `MMC_EnableErrorCorrTableCmd` | 활성화 오류 Corr 테이블 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `15.4.3` | 1318 | `MMC_GetErrorTableStatusCmd` | 조회 오류 테이블 상태 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `15.4.4` | 1321 | `MMC_DisableErrorCorrTableCmd` | 비활성화 오류 Corr 테이블 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `15.4.5` | 1324 | `MMC_UnloadErrorCorrTableCmd` | Unload 오류 Corr 테이블 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 15.4.1 MMC_LoadErrorCorrTable

- PDF 페이지: 1312
- 원문 위치: [15.4.1 MMC_LoadErrorCorrTable](../chunks/052_p1303-p1326_Chapter-15-Error-Correction-Mechanism.md#pdf-page-1312)
- 기능 설명: 로드 오류 Corr 테이블 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_LoadErrorCorrTableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_LOADERRORTABLE_IN* pInParam,
OUT MMC_LOADERRORTABLE_OUT* pOutParam
);
```
```c
rc = MMC_LoadErrorCorrTableCmd(hConnHndl, &stLoadErrorTableIn,
&stLoadErrorTableOut);
```

#### 구조체/인자

##### `MMC_LOADERRORTABLE_IN`
| 필드 | 해석 |
|---|---|
| `double dMaxCorrectionDelta;` | d Max Correction Delta 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_ERROR_TABLE_NUMBER eETNumber;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char pPathToETFile[NC_MAX_ET_FILE_PATH_LENGTH];` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_LOADERRORTABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 15.4.2 MMC_EnableErrorCorrTable

- PDF 페이지: 1315
- 원문 위치: [15.4.2 MMC_EnableErrorCorrTable](../chunks/052_p1303-p1326_Chapter-15-Error-Correction-Mechanism.md#pdf-page-1315)
- 기능 설명: 활성화 오류 Corr 테이블 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_EnableErrorCorrTableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_ENABLEERRORTABLE_IN* pInParam,
OUT MMC_ENABLEERRORTABLE_OUT* pOutParam
);
```
```c
rc = MMC_EnableErrorCorrTableCmd(hConnHndl, &stEnableErrorTableIn,
&stEnableErrorTableOut);
```

#### 구조체/인자

##### `MMC_ENABLEERRORTABLE_IN`
| 필드 | 해석 |
|---|---|
| `NC_ERROR_TABLE_NUMBER eTableNumber;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_ENABLEERRORTABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 15.4.3 MMC_GetErrorTableStatus

- PDF 페이지: 1318
- 원문 위치: [15.4.3 MMC_GetErrorTableStatus](../chunks/052_p1303-p1326_Chapter-15-Error-Correction-Mechanism.md#pdf-page-1318)
- 기능 설명: 조회 오류 테이블 상태 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetErrorTableStatusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETERRORTABLESTATUS_IN* pInParam,
OUT MMC_GETERRORTABLESTATUS_OUT* pOutParam
);
```
```c
rc = MMC_GetErrorTableStatusCmd(hConnHndl, &stGetErrorTableStatusIn,
&stGetErrorTableStatusOut);
```

#### 구조체/인자

##### `MMC_GETERRORTABLESTATUS_IN`
| 필드 | 해석 |
|---|---|
| `NC_ERROR_TABLE_NUMBER eTableNumber;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_GETERRORTABLESTATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucIsTableEnabled;` | 활성화/비활성화 제어 값입니다. |
| `unsigned char ucIsTableLoaded;` | uc Is 테이블 Loaded 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_NODE_HNDL_T hReferenceAxesRef[NC_ERROR_TABLE_DIMENSION_3D];` | 오류 ID입니다. |
| `NC_NODE_HNDL_T hTargetAxisRef;` | 축 식별 또는 축 관련 값입니다. |
| `char cFileName[NC_MAX_ET_FILE_PATH_LENGTH];` | 길이, 크기 또는 개수 값입니다. |
| `char sSpare[20];` | s Spare[20] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 15.4.4 MMC_DisableErrorCorrTable

- PDF 페이지: 1321
- 원문 위치: [15.4.4 MMC_DisableErrorCorrTable](../chunks/052_p1303-p1326_Chapter-15-Error-Correction-Mechanism.md#pdf-page-1321)
- 기능 설명: 비활성화 오류 Corr 테이블 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_DisableErrorCorrTableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_DISABLEERRORTABLE_IN* pInParam,
OUT MMC_DISABLEERRORTABLE_OUT* pOutParam
);
```
```c
rc = MMC_DisableErrorCorrTableCmd(hConnHndl, &stDisableErrorTableIn,
&stDisableErrorTableOut);
```

#### 구조체/인자

##### `MMC_DISABLEERRORTABLE_IN`
| 필드 | 해석 |
|---|---|
| `NC_ERROR_TABLE_NUMBER eTableNumber;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_DISABLEERRORTABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 15.4.5 MMC_UnloadErrorCorrTable

- PDF 페이지: 1324
- 원문 위치: [15.4.5 MMC_UnloadErrorCorrTable](../chunks/052_p1303-p1326_Chapter-15-Error-Correction-Mechanism.md#pdf-page-1324)
- 기능 설명: Unload 오류 Corr 테이블 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_UnloadErrorCorrTableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_UNLOADERRORTABLE_IN* pInParam,
OUT MMC_UNLOADERRORTABLE_OUT* pOutParam
);
```
```c
rc = MMC_UnloadErrorCorrTableCmd(hConnHndl, &stUnloadErrorTableIn,
&stUnloadErrorTableOut);
```

#### 구조체/인자

##### `MMC_UNLOADERRORTABLE_IN`
| 필드 | 해석 |
|---|---|
| `NC_ERROR_TABLE_NUMBER eTableNumber;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_UNLOADERRORTABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

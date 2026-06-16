# 8.10 PVT Functions - API 분석

- 원본 장: `Chapter 8 Position, Velocity, Time (PVT) Motion`
- 시작 PDF 페이지: 864
- 원문 위치: [8.10 PVT Functions](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-864)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `8.10.1` | 865 | `MMC_InitTableCmd` | 초기화 테이블 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `8.10.2` | 870 | `MMC_InitTableExCmd` | 초기화 테이블 Ex 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `8.10.3` | 875 | `MMC_LoadTableFromFileCmd` | 로드 테이블 From 파일 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `8.10.4` | 880 | `MMC_UnloadTableCmd` | Unload 테이블 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `8.10.5` | 882 | `MMC_MoveTableCmd` | 이동 테이블 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `8.10.6` | 886 | `MMC_AppendPointsToTableCmd` | 추가 Points To 테이블 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `8.10.7` | 890 | `MMC_GetTableIndexCmd` | 조회 테이블 Index 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 8.10.1 MMC_InitTable

- PDF 페이지: 865
- 원문 위치: [8.10.1 MMC_InitTable](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-865)
- 기능 설명: 초기화 테이블 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_InitTableCmd(
MMC_CONNECT_HNDL hConn,
MMC_INITTABLE_IN* pInParam,
MMC_INITTABLE_OUT* pOutParam);
```

#### 구조체/인자

##### `MMC_INITTABLE_IN`
| 필드 | 해석 |
|---|---|
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `NC_MOTION_TABLE_TYPE_ENUM eTableType;` | 데이터 또는 동작 타입 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long ulMaxNumberOfPoints;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned long ulUnderflowThreshold;` | ul Underflow Threshold 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usAxisRef;` | 축 식별 또는 축 관련 값입니다. |
| `unsigned short usDimension;` | us Dimension 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucIsDynamicMode;` | 동작 모드 값입니다. |
| `unsigned char ucIsPosAbsolute;` | uc Is Pos 절대 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucIsCyclic;` | uc Is Cyclic 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucSuperimposed;` | uc Superimposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_INITTABLE_OUT`
| 필드 | 해석 |
|---|---|
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 8.10.2 MMC_InitTableEx

- PDF 페이지: 870
- 원문 위치: [8.10.2 MMC_InitTableEx](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-870)
- 기능 설명: 초기화 테이블 Ex 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_InitTableExCmd(
MMC_CONNECT_HNDL hConn,
MMC_INITTABLEEX_IN* pInParam,
MMC_INITTABLEEX_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_INITTABLEEX_IN`
| 필드 | 해석 |
|---|---|
| `double dbConstVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `double dbConstTime;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `NC_MOTION_TABLE_TYPE_ENUM eTableType;` | 데이터 또는 동작 타입 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_ONLINE_SPLINE_MODE_ENUM eSplineMode;` | 동작 모드 값입니다. |
| `unsigned long ulMaxNumberOfPoints;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned long ulUnderflowThreshold;` | ul Underflow Threshold 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usAxisRef;` | 축 식별 또는 축 관련 값입니다. |
| `unsigned short usDimension;` | us Dimension 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucIsDynamicMode;` | 동작 모드 값입니다. |
| `unsigned char ucIsPosAbsolute;` | uc Is Pos 절대 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucIsCyclic;` | uc Is Cyclic 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucSuperimposed;` | uc Superimposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `unsigned char ucSpare[35];` | uc Spare[35] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_INITTABLEEX_OUT`
| 필드 | 해석 |
|---|---|
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 8.10.3 MMC_LoadTableFromFile

- PDF 페이지: 875
- 원문 위치: [8.10.3 MMC_LoadTableFromFile](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-875)
- 기능 설명: 로드 테이블 From 파일 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_LoadTableFromFileCmd(
MMC_CONNECT_HNDL hConn,
MMC_LOADTABLEFROMFILE_IN* pInParam,
MMC_LOADTABLE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_LOADTABLEFROMFILE_IN`
| 필드 | 해석 |
|---|---|
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `NC_MOTION_TABLE_TYPE_ENUM eTableType;` | 데이터 또는 동작 타입 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usAxisRef;` | 축 식별 또는 축 관련 값입니다. |
| `MC_PATH_DATA_REF pPathToTableFile;` | 파일명, 경로, 이름 문자열입니다. |

##### `MMC_LOADTABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |

### 8.10.4 MMC_UnloadTable

- PDF 페이지: 880
- 원문 위치: [8.10.4 MMC_UnloadTable](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-880)
- 기능 설명: Unload 테이블 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_UnloadTableCmd(
MMC_CONNECT_HNDL hConn,
MMC_UNLOADTABLE_IN* pInParam,
MMC_UNLOADTABLE_OUT* pOutParam
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 8.10.5 MMC_MoveTable

- PDF 페이지: 882
- 원문 위치: [8.10.5 MMC_MoveTable](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-882)
- 기능 설명: 이동 테이블 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveTableCmd(
MMC_CONNECT_HNDL hConn,
MMC_AXIS_REF_HNDL hAxisRef,
MMC_MOVETABLE_IN* pInParam,
MMC_MOVETABLE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVETABLE_IN`
| 필드 | 해석 |
|---|---|
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned char ucSuperImposed;` | uc Super Imposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_MOVETABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 8.10.6 MMC_AppendPointsToTable

- PDF 페이지: 886
- 원문 위치: [8.10.6 MMC_AppendPointsToTable](../chunks/033_p0886-p0889_8.10.6-MMC_AppendPointsToTable.md#pdf-page-886)
- 기능 설명: 추가 Points To 테이블 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_AppendPointsToTableCmd(
MMC_CONNECT_HNDL hConn,
MMC_APPENDPOINTSTOTABLE_IN* pInParam,
MMC_APPENDPOINTSTOTABLE_OUT* pOutParam);
```

#### 구조체/인자

##### `MMC_APPENDPOINTSTOTABLE_IN`
| 필드 | 해석 |
|---|---|
| `double dTable[NC_PVT_ECAM_MAX_ARRAY_SIZE];` | 길이, 크기 또는 개수 값입니다. |
| `NC_MOTION_TABLE_TYPE_ENUM eTableType;` | 데이터 또는 동작 타입 값입니다. |
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned long ulStartIndex;` | 인덱스 값입니다. |
| `unsigned long ulNumberOfPoints;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usAxisRef;` | 축 식별 또는 축 관련 값입니다. |
| `unsigned char ucIsTimeAbsolute;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned char ucIsAutoAppend;` | uc Is Auto 추가 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_APPENDPOINTSTOTABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 8.10.7 MMC_GetTableIndex

- PDF 페이지: 890
- 원문 위치: [8.10.7 MMC_GetTableIndex](../chunks/034_p0890-p0893_8.10.7-MMC_GetTableIndex.md#pdf-page-890)
- 기능 설명: 조회 테이블 Index 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetTableIndexCmd(
MMC_CONNECT_HNDL hConn,
MMC_GETTABLEINDEX_IN* pInParam,
MMC_GETTABLEINDEX_OUT* pOutParam
);
```
```c
rc = MMC_InitTableCmd(hConn, &stInitTableIn, &stInitTableOut);
if (NC_OK != rc)
{
HandleError();
```
```c
rc = MMC_AppendPointsToTableCmd(hConn, &stAppendPointsIn,
&stAppendPointsOut);
```
```c
rc = MMC_LoadTableFromFileCmd(hConn, &stLoadTableFromFileIn,
&stLoadTableOut);
```
```c
rc = MMC_MoveTableCmd(hConn, aRef, &stMoveTableIn, &stMoveTableOut);
if (NC_OK != rc)
{
HandleError();
```
```c
rc = MMC_GetTableIndexCmd(hConn, &stGetTableIndexIn, &stGetTableIndexOut);
if (NC_OK != rc)
{
HandleError();
```

#### 구조체/인자

##### `MMC_GETTABLEINDEX_IN`
| 필드 | 해석 |
|---|---|
| `init MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |

##### `MMC_GETTABLEINDEX_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long ulCurrentIndex;` | 인덱스 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

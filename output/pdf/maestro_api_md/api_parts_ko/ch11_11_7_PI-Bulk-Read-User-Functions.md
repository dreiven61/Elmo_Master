# 11.7 PI Bulk Read User Functions - API 분석

- 원본 장: `Chapter 11 Process Image(PI)`
- 시작 PDF 페이지: 1194
- 원문 위치: [11.7 PI Bulk Read User Functions](../chunks/045_p1172-p1205_11.6.26-MMC_WritePIVarDouble.md#pdf-page-1194)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `11.7.1` | 1195 | `MMC_ConfigureBulkReadPI` | 구성 벌크 읽기 Process Image 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `11.7.2` | 1198 | `MMC_PerformBulkReadCmdPI` | Perform 벌크 읽기 Cmd Process Image 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 11.7.1 MMC_ConfigureBulkReadPI

- PDF 페이지: 1195
- 원문 위치: [11.7.1 MMC_ConfigureBulkReadPI](../chunks/045_p1172-p1205_11.6.26-MMC_WritePIVarDouble.md#pdf-page-1195)
- 기능 설명: 구성 벌크 읽기 Process Image 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
int MMC_ConfigureBulkReadPI(
MMC_CONNECT_HNDL hConn
MMC_PICONFIGBULKREAD_IN *pInParam
MMC_PICONFIGBULKREAD_OUT *pOutParam
);
```

#### 구조체/인자

##### `MMC_PICONFIGBULKREAD_IN`
| 필드 | 해석 |
|---|---|
| `PI_BULKREAD_ENTRY pVarsAn array [NC_MAX_PI_BULK_READ_VARIABLES];` | Process Image BULKREAD ENTRY p Vars Array [NC MAX Process Image BULK READ VARIABLES] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_BULKREAD_CONFIG_PI_ENUM eConfiguration;` | e Configuration 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_PICONFIGBULKREAD_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 11.7.2 MMC_PerformBulkReadCmdPI

- PDF 페이지: 1198
- 원문 위치: [11.7.2 MMC_PerformBulkReadCmdPI](../chunks/045_p1172-p1205_11.6.26-MMC_WritePIVarDouble.md#pdf-page-1198)
- 기능 설명: Perform 벌크 읽기 Cmd Process Image 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_PerformBulkReadCmdPI(
IN MMC_CONNECT_HNDL hConn,
IN MMC_PERFORMBULKREADPI_IN* pInParam,
OUT MMC_PERFORMBULKREADPI_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_PERFORMBULKREADPI_IN`
| 필드 | 해석 |
|---|---|
| `NC_BULKREAD_CONFIG_PI_ENUM eConfiguration;` | e Configuration 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_PERFORMBULKREADPI_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long ulOutBuf[NC_MAX_BULK_READ_READABLE_PACKET_SIZE];` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

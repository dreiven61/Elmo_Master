# 13.1 Bulk Reading Functions - API 분석

- 원본 장: `Chapter 13 Bulk Parameters Reading`
- 시작 PDF 페이지: 1238
- 원문 위치: [13.1 Bulk Reading Functions](../chunks/048_p1238-p1251_Chapter-13-Bulk-Parameters-Reading.md#pdf-page-1238)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `13.1.1` | 1239 | `MMC_ConfigBulkReadCmd` | 구성 벌크 읽기 값/상태를 조회하는 API입니다. | Motion Mode NC - Immaterial Distributed - Immaterial |
| `13.1.2` | 1246 | `MMC_PerformBulkReadCmd` | Perform 벌크 읽기 값/상태를 조회하는 API입니다. | Motion Mode NC - Immaterial Distributed - Immaterial |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 13.1.1 MMC_ConfigBulkRead

- PDF 페이지: 1239
- 원문 위치: [13.1.1 MMC_ConfigBulkRead](../chunks/048_p1238-p1251_Chapter-13-Bulk-Parameters-Reading.md#pdf-page-1239)
- 기능 설명: 구성 벌크 읽기 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Immaterial Distributed - Immaterial

#### 시그니처

```c
MMC_LIB_API int MMC_ConfigBulkReadCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_CONFIGBULKREAD_IN* pInParam,
OUT MMC_CONFIGBULKREAD_OUT* pOutParam
);
```
```c
rc = MMC_ConfigBulkReadCmd(g_hConnectHndl, &stCfgIn, &stCfgOut);
etc.
```

#### 구조체/인자

##### `MMC_CONFIGBULKREAD_IN`
| 필드 | 해석 |
|---|---|
| `NC_BULKREAD_PARAMETERS_UNION uBulkReadParams;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `NC_BULKREAD_CONFIG_ENUM eConfiguration;` | e Configuration 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usAxisRefAn array[NC_MAX_AXES_PER_BULK_READ];` | 축 식별 또는 축 관련 값입니다. |
| `unsigned short usNumberOfAxes;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char ucIsPreset;` | uc Is Preset 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CONFIGBULKREAD_OUT`
| 필드 | 해석 |
|---|---|
| `float fFactorsAn array[NC_MAX_BULK_READ_READABLE_PACKET_SIZE];` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 13.1.2 MMC_PerformBulkRead

- PDF 페이지: 1246
- 원문 위치: [13.1.2 MMC_PerformBulkRead](../chunks/048_p1238-p1251_Chapter-13-Bulk-Parameters-Reading.md#pdf-page-1246)
- 기능 설명: Perform 벌크 읽기 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Immaterial Distributed - Immaterial

#### 시그니처

```c
MMC_LIB_API int MMC_PerformBulkReadCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_PERFORMBULKREAD_IN* pInParam,
OUT MMC_PERFORMBULKREAD_OUT* pOutParam
);
```
```c
rc = MMC_PerformBulkReadCmd(g_hConnectHndl, &stPerformBulkReadIn,
&stPerformBulkReadOut);
```

#### 구조체/인자

##### `MMC_PERFORMBULKREAD_IN`
| 필드 | 해석 |
|---|---|
| `NC_BULKREAD_CONFIG_ENUM eConfiguration;` | e Configuration 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_PERFORMBULKREAD_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long ulOutBuf[NC_MAX_BULK_READ_READABLE_PACKET_SIZE];` | 데이터 버퍼 또는 데이터 값입니다. |
| `NC_BULKREAD_PRESET_ENUM eChosenPreset;` | e Chosen Preset 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

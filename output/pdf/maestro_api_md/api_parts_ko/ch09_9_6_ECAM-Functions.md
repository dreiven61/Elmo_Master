# 9.6 ECAM Functions - API 분석

- 원본 장: `Chapter 9 Electronic CAM`
- 시작 PDF 페이지: 906
- 원문 위치: [9.6 ECAM Functions](../chunks/035_p0894-p0933_9.2-CAM-Table.md#pdf-page-906)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `9.6.1` | 907 | `MMC_CamTableInitCmd` | CAM 테이블 초기화 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.2` | 911 | `MMC_CamTableSelectCmd` | CAM 테이블 Select 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.3` | 915 | `MMC_UnloadTableCmd` | Unload 테이블 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.4` | 917 | `MMC_CamTableAddCmd` | CAM 테이블 Add 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.5` | 920 | `MMC_CamTableAddEx` | CAM 테이블 Add Ex 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.6` | 922 | `MMC_CamTableSetCmd` | CAM 테이블 설정 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `9.6.7` | 925 | `MMC_CamInCmd` | CAM In 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.8` | 930 | `MMC_CamOutCmd` | CAM Out 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.9` | 932 | `MMC_CamStatusCmd` | CAM 상태 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.10` | 934 | `MMC_CamSetPropertyCmd` | CAM 설정 Property 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.11` | 938 | `MMC_GearInCmd` | Gear In 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.12` | 942 | `MMC_GearInPosCmd` | Gear In Pos 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `9.6.13` | 946 | `MMC_GearOutCmd` | Gear Out 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 9.6.1 MMC_CamTableInit

- PDF 페이지: 907
- 원문 위치: [9.6.1 MMC_CamTableInit](../chunks/035_p0894-p0933_9.2-CAM-Table.md#pdf-page-907)
- 기능 설명: CAM 테이블 초기화 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_CamTableInitCmd(
MMC_CONNECT_HNDL hConn,
MMC_CAMTABLEINIT_IN* pInParam,
MMC_INITTABLE_OUT* pOutParam);
```

#### 구조체/인자

##### `MMC_CAMTABLEINIT_IN`
| 필드 | 해석 |
|---|---|
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned long ulMaxNumberOfPoints;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned long ulUnderflowThreshold;` | ul Underflow Threshold 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `CURVE_TYPE_ENUM eCurveType;` | 데이터 또는 동작 타입 값입니다. |
| `unsigned short usDimension;` | us Dimension 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucSuperimposed;` | uc Superimposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucIsFixedGap;` | uc Is Fixed Gap 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `unsigned char ucSpare[32];` | uc Spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_INITTABLE_OUT`
| 필드 | 해석 |
|---|---|
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 9.6.2 MMC_CamTableSelect

- PDF 페이지: 911
- 원문 위치: [9.6.2 MMC_CamTableSelect](../chunks/035_p0894-p0933_9.2-CAM-Table.md#pdf-page-911)
- 기능 설명: CAM 테이블 Select 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_CamTableSelectCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_CAMTABLESELECT_IN* pInParam,
OUT MMC_CAMTABLESELECT_OUT* pOutParam);
```

#### 구조체/인자

##### `MMC_CAMTABLESELECT_IN`
| 필드 | 해석 |
|---|---|
| `MC_CamRef CamTable;` | CAM 테이블 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned int uiStartMode;` | 동작 모드 값입니다. |
| `unsigned char ucIsMasterPosAbsolute;` | uc Is Master Pos 절대 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucIsSlavePosAbsolute;` | uc Is Slave Pos 절대 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucSpare[32];` | uc Spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CAMTABLESELECT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |

### 9.6.3 MMC_CamTableUnload

- PDF 페이지: 915
- 원문 위치: [9.6.3 MMC_CamTableUnload](../chunks/035_p0894-p0933_9.2-CAM-Table.md#pdf-page-915)
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

### 9.6.4 MMC_CamTableAdd

- PDF 페이지: 917
- 원문 위치: [9.6.4 MMC_CamTableAdd](../chunks/035_p0894-p0933_9.2-CAM-Table.md#pdf-page-917)
- 기능 설명: CAM 테이블 Add 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_CamTableAddCmd(
MMC_CONNECT_HNDL hConn,
MMC_CAMTABLESET_IN* pInParam,
MMC_CAMTABLESET_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CAMTABLESET_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbTable[NC_ECAM_MAX_ARRAY_SIZE];` | 길이, 크기 또는 개수 값입니다. |
| `unsigned long ulStartIndex;` | 인덱스 값입니다. |
| `unsigned long ulNumberOfPoints;` | 길이, 크기 또는 개수 값입니다. |
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned char ucSpare[32];` | uc Spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CAMTABLESET_OUT`
| 필드 | 해석 |
|---|---|
| `usigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 9.6.5 MMC_CamTableAddEx

- PDF 페이지: 920
- 원문 위치: [9.6.5 MMC_CamTableAddEx](../chunks/035_p0894-p0933_9.2-CAM-Table.md#pdf-page-920)
- 기능 설명: CAM 테이블 Add Ex 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_CamTableAddEx(
MMC_CONNECT_HNDL hConn,
MMC_CAMTABLESET_INEx* pInParam,
MMC_CAMTABLESET_OUTEx* pOutParam
);
```

#### 구조체/인자

##### `MMC_CAMTABLESET_INEx`
| 필드 | 해석 |
|---|---|
| `double dbTable[];` | double db Table[] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usColumns;` | us Columns 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long ulNumberOfPoints;` | 길이, 크기 또는 개수 값입니다. |
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |

### 9.6.6 MC_CamTableSet

- PDF 페이지: 922
- 원문 위치: [9.6.6 MC_CamTableSet](../chunks/035_p0894-p0933_9.2-CAM-Table.md#pdf-page-922)
- 기능 설명: CAM 테이블 설정 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CamTableSetCmd(
MMC_CONNECT_HNDL hConn,
MMC_CAMTABLESET_IN* pInParam,
MMC_CAMTABLESET_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CAMTABLESET_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbTable[NC_ECAM_MAX_ARRAY_SIZE];` | 길이, 크기 또는 개수 값입니다. |
| `unsigned long ulStartIndex;` | 인덱스 값입니다. |
| `unsigned long ulNumberOfPoints;` | 길이, 크기 또는 개수 값입니다. |
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |

##### `MMC_CAMTABLESET_OUT`
| 필드 | 해석 |
|---|---|
| `usigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 9.6.7 MMC_CamIn

- PDF 페이지: 925
- 원문 위치: [9.6.7 MMC_CamIn](../chunks/035_p0894-p0933_9.2-CAM-Table.md#pdf-page-925)
- 기능 설명: CAM In 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_CamInCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CAMIN_IN* pInParam,
OUT MMC_CAMIN_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CAMIN_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbMasterOffset;` | db Master Offset 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbSlaveOffset;` | db Slave Offset 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbMasterScaling;` | db Master Scaling 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbSlaveScaling;` | db Slave Scaling 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbMasterStartDistance;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbMasterSyncPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `ECAM_VALUE_SRC_ENUM eMasterValueSource;` | 전달하거나 반환받는 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned int uiStartMode;` | 동작 모드 값입니다. |
| `CURVE_TYPE_ENUM eCurveType;` | 데이터 또는 동작 타입 값입니다. |
| `ECAM_PERIODIC_ENUM ePeriodicMode;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned int uiCamTableID;` | ui CAM 테이블 ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usMaster;` | us Master 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucAutoOffset;` | uc Auto Offset 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucSpare[32];` | uc Spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CAMIN_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned int uiEndOfProfile;` | 파일명, 경로, 이름 문자열입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 9.6.8 MMC_CamOut

- PDF 페이지: 930
- 원문 위치: [9.6.8 MMC_CamOut](../chunks/035_p0894-p0933_9.2-CAM-Table.md#pdf-page-930)
- 기능 설명: CAM Out 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_CamOutCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CAMOUT_IN* pInParam,
OUT MMC_CAMOUT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CAMOUT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_CAMOUT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 9.6.9 MMC_CamStatus

- PDF 페이지: 932
- 원문 위치: [9.6.9 MMC_CamStatus](../chunks/035_p0894-p0933_9.2-CAM-Table.md#pdf-page-932)
- 기능 설명: CAM 상태 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_CamStatusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CAMSTATUS_IN* pInParam,
OUT MMC_CAMSTATUS_OUT* pOutParam);
```

#### 구조체/인자

##### `MMC_CAMSTATUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CAMSTATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long ulEndOfProfile;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned long ulCurrentIndex;` | 인덱스 값입니다. |
| `unsigned long ulCycle;` | ul Cycle 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucSpare[32];` | uc Spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 9.6.10 MMC_CamSetProperty

- PDF 페이지: 934
- 원문 위치: [9.6.10 MMC_CamSetProperty](../chunks/036_p0934-p0950_9.6.10-MMC_CamSetProperty.md#pdf-page-934)
- 기능 설명: CAM 설정 Property 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_CamSetPropertyCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CAMSETPROP_IN* pInParam,
OUT MMC_CAMSETPROP_OUT* pOutParam);
```

#### 구조체/인자

##### `MMC_CAMSETPROP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `ECAM_PROPERTIES_ENUM eProperty;` | e Property 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `ECAM_PROPERTY_VALUE value;` | 전달하거나 반환받는 값입니다. |
| `unsigned char ucSpare[32];` | uc Spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CAMSETPROP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 9.6.11 MMC_GearIn

- PDF 페이지: 938
- 원문 위치: [9.6.11 MMC_GearIn](../chunks/036_p0934-p0950_9.6.10-MMC_CamSetProperty.md#pdf-page-938)
- 기능 설명: Gear In 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_GearInCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GEARIN_IN* pInParam,
OUT MMC_GEARIN_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GEARIN_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usMaster;` | us Master 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `int iRatioNumerator;` | 길이, 크기 또는 개수 값입니다. |
| `int iRatioDenominator;` | i Ratio Denominator 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `ECAM_VALUE_SRC_ENUM eMasterValueSource;` | 전달하거나 반환받는 값입니다. |
| `unsigned int uiSyncMode;` | 동작 모드 값입니다. |
| `double dbAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `double dbDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `double dbJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucSpare[32];` | uc Spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GEARIN_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 9.6.12 MMC_GearInPos

- PDF 페이지: 942
- 원문 위치: [9.6.12 MMC_GearInPos](../chunks/036_p0934-p0950_9.6.10-MMC_CamSetProperty.md#pdf-page-942)
- 기능 설명: Gear In Pos 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_GearInPosCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GEARINPOS_IN* pInParam,
OUT MMC_GEARINPOS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GEARINPOS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usMaster;` | us Master 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbMasterStartDistance;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbMasterSyncPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbSlaveSyncPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `int iRatioNumerator;` | 길이, 크기 또는 개수 값입니다. |
| `int iRatioDenominator;` | i Ratio Denominator 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `ECAM_VALUE_SRC_ENUM eMasterValueSource;` | 전달하거나 반환받는 값입니다. |
| `unsigned int uiSyncMode;` | 동작 모드 값입니다. |
| `double dbVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `double dbAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `double dbDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `double dbJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `unsigned char ucSpare[32];` | uc Spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GEARINPOS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 9.6.13 MMC_GearOut

- PDF 페이지: 946
- 원문 위치: [9.6.13 MMC_GearOut](../chunks/036_p0934-p0950_9.6.10-MMC_CamSetProperty.md#pdf-page-946)
- 기능 설명: Gear Out 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_GearOutCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GEAROUT_IN* pInParam,
OUT MMC_GEAROUT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GEAROUT_IN`
- 구조체 필드를 추출하지 못했습니다.

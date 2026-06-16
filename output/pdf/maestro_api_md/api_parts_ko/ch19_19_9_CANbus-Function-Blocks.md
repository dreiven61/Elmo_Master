# 19.9 CANbus Function Blocks - API 분석

- 원본 장: `Chapter 19 CANbus Drive Communication`
- 시작 PDF 페이지: 1416
- 원문 위치: [19.9 CANbus Function Blocks](../chunks/057_p1410-p1448_19.1-Master-Slave-Relations.md#pdf-page-1416)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `19.9.1` | 1417 | `MMC_CancelVirtualEncoderCmd` | Cancel Virtual Encoder 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.2` | 1419 | `MMC_CancelParamEvPDO3Cmd` | Cancel 파라미터 Ev PDO3 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.3` | 1422 | `MMC_CancelParamEvPDO3Cmd` | Cancel 파라미터 Ev PDO3 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.4` | 1425 | `MMC_CfgRegParamEvPDO3Cmd` | Cfg Reg 파라미터 Ev PDO3 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.5` | 1429 | `MMC_CfgRegParamEvPDO4Cmd` | Cfg Reg 파라미터 Ev PDO4 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.6` | 1433 | `MMC_CfgUserParamEvPDO3Cmd` | Cfg 사용자 파라미터 Ev PDO3 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.7` | 1438 | `MMC_CfgUserParamEvPDO4Cmd` | Cfg 사용자 파라미터 Ev PDO4 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.8` | 1443 | `MMC_ChangeDefaultPDOConfiguration` | Change Default PDOConfiguration 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.9` | 1446 | `MMC_CfgEventModePDO3Cmd` | Cfg 이벤트 모드 PDO3 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.10` | 1449 | `MMC_CfgEventModePDO4Cmd` | Cfg 이벤트 모드 PDO4 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.11` | 1452 | `MMC_ConfigVirtualEncoderCmd` | 구성 Virtual Encoder 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.12` | 1455 | `MMC_GetAxisByCanIdCmd` | 조회 축 By Can Id 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.13` | 1458 | `MMC_GetPDOInfoCmd` | 조회 PDOInfo 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.14` | 1462 | `MMC_GetSyncTimeCmd` | 조회 동기 Time 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.15` | 1465 | `MMC_PDOGeneralReadCmd` | PDOGeneral 읽기 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.16` | 1468 | `MMC_PDOGeneralWriteCmd` | PDOGeneral 쓰기 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.17` | 1470 | `MMC_ReceiveCANRawData` | 수신 CANRaw 데이터 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.18` | 1473 | `MMC_SendCANRawData` | 전송 CANRaw 데이터 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.19` | 1476 | `MMC_SendandReceiveCANRawData` | Sendand 수신 CANRaw 데이터 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.20` | 1479 | `MMC_SendCmd` | 전송 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.21` | 1481 | `MMC_SetHeartBeatConsumerCmd` | 설정 Heart Beat Consumer 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.22` | 1484 | `MMC_SetSyncTimeCmd` | 설정 동기 Time 값/설정을 적용하는 API입니다. | Motion Mode NC - Not Supported Distributed - Supported |
| `19.9.23` | 1486 | `MMC_StartBulkUploadCmd` | 시작 벌크 업로드 작업을 수행하는 API입니다. | Motion Mode NC -Not Supported Distributed - Supported |
| `19.9.24` | 1488 | `MMC_GetBulkUploadStatusCmd` | 조회 벌크 업로드 상태 값/상태를 조회하는 API입니다. | Motion Mode NC -Supported Distributed - Supported |
| `19.9.25` | 1491 | `MMC_GetBulkUploadDataCmd` | 조회 벌크 업로드 데이터 값/상태를 조회하는 API입니다. | Motion Mode NC -Supported Distributed - Supported |
| `19.9.26` | 1493 | `MMC_ResetCommStatistics` | 리셋 Comm Statistics 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.27` | 1496 | `MMC_SendSdoCmd` | 전송 Sdo 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.28` | 1501 | `MMC_SendSdoExCmd` | 전송 Sdo Ex 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.29` | 1505 | `MMC_SendSdoAsyncCmd` | 전송 Sdo 비동기 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.30` | 1508 | `MMC_RetrieveSdoAsyncCmd` | Retrieve Sdo 비동기 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `19.9.31` | 1510 | `MMC_SendSdoAsyncExCmd` | 전송 Sdo 비동기 Ex 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 19.9.1 MMC_CancelVirtualEncoder

- PDF 페이지: 1417
- 원문 위치: [19.9.1 MMC_CancelVirtualEncoder](../chunks/057_p1410-p1448_19.1-Master-Slave-Relations.md#pdf-page-1417)
- 기능 설명: Cancel Virtual Encoder 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CancelVirtualEncoderCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELVIRTUALENCODER_IN* pInParam,
OUT MMC_CANCELVIRTUALENCODER_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CANCELVIRTUALENCODER_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CANCELVIRTUALENCODER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.2 MMC_CancelParamEvPDO3

- PDF 페이지: 1419
- 원문 위치: [19.9.2 MMC_CancelParamEvPDO3](../chunks/057_p1410-p1448_19.1-Master-Slave-Relations.md#pdf-page-1419)
- 기능 설명: Cancel 파라미터 Ev PDO3 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CancelParamEvPDO3Cmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELPARAMEVENTPDO3_IN* pInParam,
OUT MMC_CANCELPARAMEVENTPDO3_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CANCELPARAMEVENTPDO3_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CANCELPARAMEVENTPDO3_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.3 MMC_CancelParamEvPDO4

- PDF 페이지: 1422
- 원문 위치: [19.9.3 MMC_CancelParamEvPDO4](../chunks/057_p1410-p1448_19.1-Master-Slave-Relations.md#pdf-page-1422)
- 기능 설명: Cancel 파라미터 Ev PDO3 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CancelParamEvPDO3Cmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELPARAMEVENTPDO4_IN* pInParam,
OUT MMC_CANCELPARAMEVENTPDO4_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CANCELPARAMEVENTPDO4_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CANCELPARAMEVENTPDO4_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.4 MMC_CfgRegParamEvPDO3

- PDF 페이지: 1425
- 원문 위치: [19.9.4 MMC_CfgRegParamEvPDO3](../chunks/057_p1410-p1448_19.1-Master-Slave-Relations.md#pdf-page-1425)
- 기능 설명: Cfg Reg 파라미터 Ev PDO3 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CfgRegParamEvPDO3Cmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGREGULARPARAMEVENTPDO3_IN* pInParam,
OUT MMC_CONFIGREGULARPARAMEVENTPDO3_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGREGULARPARAMEVENTPDO3_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiPDOCommParamEvent;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned short usEventTimer;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned char ucEventGroup;` | 그룹 식별 또는 그룹 관련 값입니다. |
| `unsigned char ucPDOCommParam;` | 파라미터 식별자 또는 파라미터 값입니다. |

##### `MMC_CONFIGREGULARPARAMEVENTPDO3_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.5 MMC_CfgRegParamEvPDO4

- PDF 페이지: 1429
- 원문 위치: [19.9.5 MMC_CfgRegParamEvPDO4](../chunks/057_p1410-p1448_19.1-Master-Slave-Relations.md#pdf-page-1429)
- 기능 설명: Cfg Reg 파라미터 Ev PDO4 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CfgRegParamEvPDO4Cmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGREGULARPARAMEVENTPDO4_IN* pInParam,
OUT MMC_CONFIGREGULARPARAMEVENTPDO4_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGREGULARPARAMEVENTPDO4_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiPDOCommParamEvent;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned short usEventTimer;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned char ucEventGroup;` | 그룹 식별 또는 그룹 관련 값입니다. |
| `unsigned char ucPDOCommParam;` | 파라미터 식별자 또는 파라미터 값입니다. |

##### `MMC_CONFIGREGULARPARAMEVENTPDO4_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.6 MMC_CfgUserParamEvPDO3

- PDF 페이지: 1433
- 원문 위치: [19.9.6 MMC_CfgUserParamEvPDO3](../chunks/057_p1410-p1448_19.1-Master-Slave-Relations.md#pdf-page-1433)
- 기능 설명: Cfg 사용자 파라미터 Ev PDO3 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CfgUserParamEvPDO3Cmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGUSERPARAMEVENTPDO3_IN* pInParam,
OUT MMC_CONFIGUSERPARAMEVENTPDO3_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGUSERPARAMEVENTPDO3_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiPDOCommParamEvent;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned short usEventTimer;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned char ucEventGroup;` | 그룹 식별 또는 그룹 관련 값입니다. |
| `unsigned char ucSubIndex;` | 인덱스 값입니다. |
| `unsigned char ucPDOCommParam;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned char ucPDOType;` | 데이터 또는 동작 타입 값입니다. |

##### `MMC_CONFIGUSERPARAMEVENTPDO3_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.7 MMC_CfgUserParamEvPDO4

- PDF 페이지: 1438
- 원문 위치: [19.9.7 MMC_CfgUserParamEvPDO4](../chunks/057_p1410-p1448_19.1-Master-Slave-Relations.md#pdf-page-1438)
- 기능 설명: Cfg 사용자 파라미터 Ev PDO4 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CfgUserParamEvPDO4Cmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGUSERPARAMEVENTPDO4_IN* pInParam,
OUT MMC_CONFIGUSERPARAMEVENTPDO4_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGUSERPARAMEVENTPDO4_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiPDOCommParamEvent;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned short usEventTimer;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned char ucEventGroup;` | 그룹 식별 또는 그룹 관련 값입니다. |
| `unsigned char ucSubIndex;` | 인덱스 값입니다. |
| `unsigned char ucPDOCommParam;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned char ucPDOType;` | 데이터 또는 동작 타입 값입니다. |

##### `MMC_CONFIGUSERPARAMEVENTPDO4_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.8 MMC_ChangeDefaultPDOConfiguration

- PDF 페이지: 1443
- 원문 위치: [19.9.8 MMC_ChangeDefaultPDOConfiguration](../chunks/057_p1410-p1448_19.1-Master-Slave-Relations.md#pdf-page-1443)
- 기능 설명: Change Default PDOConfiguration 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ChangeDefaultPDOConfiguration(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGPDOCOMMPARAM_IN* pInParam,
OUT MMC_CONFIGPDOCOMMPARAM_OUT *pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGPDOCOMMPARAM_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucPDONum;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char ucPDODir;` | uc PDODir 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucPDOCommParam;` | 파라미터 식별자 또는 파라미터 값입니다. |

##### `MMC_CONFIGPDOCOMMPARAM_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.9 MMC_ConfigEventModePDO3

- PDF 페이지: 1446
- 원문 위치: [19.9.9 MMC_ConfigEventModePDO3](../chunks/057_p1410-p1448_19.1-Master-Slave-Relations.md#pdf-page-1446)
- 기능 설명: Cfg 이벤트 모드 PDO3 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CfgEventModePDO3Cmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGEVENTMODEPDO3_IN* pInParam,
OUT MMC_CONFIGEVENTMODEPDO3_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGEVENTMODEPDO3_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucPDOEventMode;` | 동작 모드 값입니다. |

##### `MMC_CONFIGEVENTMODEPDO3_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.10 MMC_ConfigEventModePDO4

- PDF 페이지: 1449
- 원문 위치: [19.9.10 MMC_ConfigEventModePDO4](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1449)
- 기능 설명: Cfg 이벤트 모드 PDO4 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CfgEventModePDO4Cmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGEVENTMODEPDO4_IN* pInParam,
OUT MMC_CONFIGEVENTMODEPDO4_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGEVENTMODEPDO4_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucPDOEventMode;` | 동작 모드 값입니다. |

##### `MMC_CONFIGEVENTMODEPDO4_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.11 MMC_ConfigVirtualEncoder

- PDF 페이지: 1452
- 원문 위치: [19.9.11 MMC_ConfigVirtualEncoder](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1452)
- 기능 설명: 구성 Virtual Encoder 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ConfigVirtualEncoderCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGVIRTUALENCODER_IN* pInParam,
OUT MMC_CONFIGVIRTUALENCODER_OUT* pOutParam
);
```
```c
short CongVirtualEncoder(MMC_AXIS_REF_HNDL aRef, unsigned char theGroupID)
{
MMC_CONFIGVIRTUALENCODER_IN pInParam;
MMC_CONFIGVIRTUALENCODER_OUT pOutParam;
pInParam.dbHighPos=0;
pInParam.dbLowPos=0;
pInParam.ucGroupID=theGroupID;
pInParam.ucMode=2;
int rc;
rc=MMC_ConfigVirtualEncoder(cHndl,aRef,&pInParam,&pOutParam)
if (rc != 0)
{
HandleError();
```

#### 구조체/인자

##### `MMC_CONFIGVIRTUALENCODER_IN`
| 필드 | 해석 |
|---|---|
| `double dbLowPos;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbHighPos;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fFactor;` | f Factor 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucMode;` | 동작 모드 값입니다. |
| `unsigned char ucGroupID;` | 그룹 식별 또는 그룹 관련 값입니다. |

##### `MMC_CONFIGVIRTUALENCODER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.12 MMC_GetAxisByCanId

- PDF 페이지: 1455
- 원문 위치: [19.9.12 MMC_GetAxisByCanId](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1455)
- 기능 설명: 조회 축 By Can Id 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetAxisByCanIdCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETAXISREFFROMCANID_IN* pInParam,
OUT MMC_GETAXISREFFROMCANID_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETAXISREFFROMCANID_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucNodeID;` | 노드 식별 또는 노드 관련 값입니다. |

##### `MMC_GETAXISREFFROMCANID_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usAxisRef;` | 축 식별 또는 축 관련 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.13 MMC_GetPDOInfo

- PDF 페이지: 1458
- 원문 위치: [19.9.13 MMC_GetPDOInfo](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1458)
- 기능 설명: 조회 PDOInfo 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetPDOInfoCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETPDOINFO_IN* pInParam,
OUT MMC_GETPDOINFO_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETPDOINFO_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucPDONumber;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_GETPDOINFO_OUT`
| 필드 | 해석 |
|---|---|
| `int iPDOEventMode;` | 주소 또는 IP 관련 값입니다. |
| `unsigned int uiCommParamEventPDO;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usEventTimerPDO;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned char ucRPDOCommType;` | 데이터 또는 동작 타입 값입니다. |
| `unsigned char ucTPDOCommType;` | 데이터 또는 동작 타입 값입니다. |
| `unsigned char ucTPDOCommEventGroup;` | 그룹 식별 또는 그룹 관련 값입니다. |
| `unsigned char ucRPDOCommEventGroup;` | 그룹 식별 또는 그룹 관련 값입니다. |
| `unsigned char ucSubIndexRPDO;` | 인덱스 값입니다. |
| `unsigned char ucSubIndexTPDO;` | 인덱스 값입니다. |

### 19.9.14 MMC_GetSyncTime

- PDF 페이지: 1462
- 원문 위치: [19.9.14 MMC_GetSyncTime](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1462)
- 기능 설명: 조회 동기 Time 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetSyncTimeCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETSYNCTIME_IN* pInParam,
OUT MMC_GETSYNCTIME_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETSYNCTIME_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETSYNCTIME_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usSYNCTime;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.15 MMC_PDOGeneralRead

- PDF 페이지: 1465
- 원문 위치: [19.9.15 MMC_PDOGeneralRead](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1465)
- 기능 설명: PDOGeneral 읽기 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_PDOGeneralReadCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GENERALPARAMPDOREAD_IN* pInParam,
OUT MMC_GENERALPARAMPDOREAD_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GENERALPARAMPDOREAD_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucParam;` | 파라미터 식별자 또는 파라미터 값입니다. |

##### `MMC_GENERALPARAMPDOREAD_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned __int64 ulliVal;` | 전달하거나 반환받는 값입니다. |
| `unsigned long long int ulliVal;` | 전달하거나 반환받는 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.16 MMC_PDOGeneralWrite

- PDF 페이지: 1468
- 원문 위치: [19.9.16 MMC_PDOGeneralWrite](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1468)
- 기능 설명: PDOGeneral 쓰기 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_PDOGeneralWriteCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GENERALPARAMPDOWRITE_IN* pInParam,
OUT MMC_GENERALPARAMPDOWRITE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GENERALPARAMPDOWRITE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned __int64 ulliVal;` | 전달하거나 반환받는 값입니다. |
| `unsigned long long int ulliVal;` | 전달하거나 반환받는 값입니다. |
| `unsigned char ucParam;` | 파라미터 식별자 또는 파라미터 값입니다. |

##### `MMC_GENERALPARAMPDOWRITE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.17 MMC_ReceiveCANRawData

- PDF 페이지: 1470
- 원문 위치: [19.9.17 MMC_ReceiveCANRawData](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1470)
- 기능 설명: 수신 CANRaw 데이터 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReceiveCANRawData(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN unsigned short iTimeOutms,
OUT MMC_CAN_REPLY_DATA_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CAN_REPLY_DATA_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usFunctionID;` | us 함수 ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usNumerator;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usDatasize;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned short usPadding;` | us Padding 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usCOB_ID;` | us COB ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usAxisRef;` | 축 식별 또는 축 관련 값입니다. |
| `unsigned char can_data_length;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned char data[8];` | 데이터 버퍼 또는 데이터 값입니다. |

### 19.9.18 MMC_SendCANRawData

- PDF 페이지: 1473
- 원문 위치: [19.9.18 MMC_SendCANRawData](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1473)
- 기능 설명: 전송 CANRaw 데이터 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SendCANRawData(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SENDRAWDATA_IN* pInParam,
OUT MMC_SENDRAWDATA_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SENDRAWDATA_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usCOB_ID;` | us COB ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucLength;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char pData[8];` | 데이터 버퍼 또는 데이터 값입니다. |

##### `MMC_SENDRAWDATA_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.19 MMC_SendandReceiveCANRawData

- PDF 페이지: 1476
- 원문 위치: [19.9.19 MMC_SendandReceiveCANRawData](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1476)
- 기능 설명: Sendand 수신 CANRaw 데이터 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SendandReceiveCANRawData(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SENDRAWDATA_IN *pInParam,
OUT MMC_CAN_REPLY_DATA_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SENDRAWDATA_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usCOB_ID;` | us COB ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucLength;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char pData[8];` | 데이터 버퍼 또는 데이터 값입니다. |

##### `MMC_CAN_REPLY_DATA_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usFunctionID;` | us 함수 ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usNumerator;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usDatasize;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned short usPadding;` | us Padding 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usCOB_ID;` | us COB ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usAxisRef;` | 축 식별 또는 축 관련 값입니다. |
| `unsigned char can_data_length;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned char data[8];` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned char ucAsyncEventType;` | 데이터 또는 동작 타입 값입니다. |

### 19.9.20 MMC_SendCmd

- PDF 페이지: 1479
- 원문 위치: [19.9.20 MMC_SendCmd](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1479)
- 기능 설명: 전송 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SendCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SENDCMD_IN* pInParam,
OUT MMC_SENDCMD_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SENDCMD_IN`
| 필드 | 해석 |
|---|---|
| `char pCmd[80];` | p Cmd[80] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SENDCMD_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.21 MMC_SetHeartBeatConsumer

- PDF 페이지: 1481
- 원문 위치: [19.9.21 MMC_SetHeartBeatConsumer](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1481)
- 기능 설명: 설정 Heart Beat Consumer 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetHeartBeatConsumerCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SETHEARTBEATCONSUMER_IN* pInParam,
OUT MMC_SETHEARTBEATCONSUMER_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETHEARTBEATCONSUMER_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHeartbeatTimeFactor;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

##### `MMC_SETHEARTBEATCONSUMER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.22 MMC_SetSyncTime

- PDF 페이지: 1484
- 원문 위치: [19.9.22 MMC_SetSyncTime](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1484)
- 기능 설명: 설정 동기 Time 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetSyncTimeCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SETSYNCTIME_IN* pInParam,
OUT MMC_SETSYNCTIME_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETSYNCTIME_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usSYNCTime;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

##### `MMC_SETSYNCTIME_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.23 MMC_StartBulkUpload

- PDF 페이지: 1486
- 원문 위치: [19.9.23 MMC_StartBulkUpload](../chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md#pdf-page-1486)
- 기능 설명: 시작 벌크 업로드 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC -Not Supported Distributed - Supported

#### 시그니처

```c
int MMC_StartBulkUploadCmd(IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_STARTBULKUPLOAD_IN* pInParam,
OUT MMC_STARTBULKUPLOAD_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_STARTBULKUPLOAD_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usIndex;` | 인덱스 값입니다. |
| `unsigned char ucSubIndex;` | 인덱스 값입니다. |

##### `MMC_STARTBULKUPLOAD_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.24 MMC_GetBulkUploadStatus

- PDF 페이지: 1488
- 원문 위치: [19.9.24 MMC_GetBulkUploadStatus](../chunks/059_p1488-p1509_19.9.24-MMC_GetBulkUploadStatus.md#pdf-page-1488)
- 기능 설명: 조회 벌크 업로드 상태 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC -Supported Distributed - Supported

#### 시그니처

```c
int MMC_GetBulkUploadStatusCmd(IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETBULKUPLOADSTATUS_IN* pInParam,
OUT MMC_GETBULKUPLOADSTATUS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETBULKUPLOADSTATUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETBULKUPLOADSTATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiSizeCompleted;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `short usCommError;` | 오류 ID입니다. |
| `short usUploadError;` | 오류 ID입니다. |
| `unsigned char ucUploadState;` | uc 업로드 State 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 19.9.25 MMC_GetBulkUploadData

- PDF 페이지: 1491
- 원문 위치: [19.9.25 MMC_GetBulkUploadData](../chunks/059_p1488-p1509_19.9.24-MMC_GetBulkUploadStatus.md#pdf-page-1491)
- 기능 설명: 조회 벌크 업로드 데이터 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC -Supported Distributed - Supported

#### 시그니처

```c
int MMC_GetBulkUploadDataCmd(IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETBULKUPLOADDATA_IN* pInParam,
OUT MMC_GETBULKUPLOADDATA_OUT* pOutParam)
);
```

#### 구조체/인자

##### `MMC_GETBULKUPLOADDATA_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usStartIndex;` | 인덱스 값입니다. |
| `unsigned short usEndIndex;` | 인덱스 값입니다. |

##### `MMC_GETBULKUPLOADDATA_OUT`
| 필드 | 해석 |
|---|---|
| `char cDataBuffer[NC_MAX_REC_PACKET_SIZE];` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 19.9.26 MMC_ResetCommStatistics

- PDF 페이지: 1493
- 원문 위치: [19.9.26 MMC_ResetCommStatistics](../chunks/059_p1488-p1509_19.9.24-MMC_GetBulkUploadStatus.md#pdf-page-1493)
- 기능 설명: 리셋 Comm Statistics 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ResetCommStatistics(
IN MMC_CONNECT_HNDL hConn,
IN MMC_RESETCOMMSTATISTICS_IN* pInParam,
OUT MMC_RESETCOMMSTATISTICS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_RESETCOMMSTATISTICS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_RESETCOMMSTATISTICS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19.9.27 MMC_SendSDO

- PDF 페이지: 1496
- 원문 위치: [19.9.27 MMC_SendSDO](../chunks/059_p1488-p1509_19.9.24-MMC_GetBulkUploadStatus.md#pdf-page-1496)
- 기능 설명: 전송 Sdo 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SendSdoCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SENDSDO_IN* pInParam,
OUT MMC_SENDSDO_OUT* pOutParam
);
```
```c
void DownloadSDO(
ushort objectIndex,
byte objectSubIndex,
byte[] data,
uint dataLength,
int timeout);
```
```c
void DownloadSDO(ushort objectIndex, byte objectSubIndex,
int data, int timeout);
```
```c
void DownloadSDO(ushort objectIndex, byte objectSubIndex,
uint data, int timeout);
```
```c
void DownloadSDO(ushort objectIndex, byte objectSubIndex,
short data, int timeout);
```
```c
void DownloadSDO(ushort objectIndex, byte objectSubIndex,
ushort data, int timeout);
```
```c
void DownloadSDO(ushort objectIndex, byte objectSubIndex,
byte data, int timeout);
```
```c
void DownloadSDO(ushort objectIndex, byte objectSubIndex,
float data, int timeout);
```

#### 구조체/인자

##### `MMC_SENDSDO_IN`
| 필드 | 해석 |
|---|---|
| `long lData;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned long ulDataLength;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned short usSlaveID;` | us Slave ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usIndex;` | 인덱스 값입니다. |
| `unsigned char ucSubIndex;` | 인덱스 값입니다. |
| `unsigned char ucService;` | uc Service 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SENDSDO_OUT`
| 필드 | 해석 |
|---|---|
| `long lData;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned long ulDataLength;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 19.9.28 MMC_SendSDOEx

- PDF 페이지: 1501
- 원문 위치: [19.9.28 MMC_SendSDOEx](../chunks/059_p1488-p1509_19.9.24-MMC_GetBulkUploadStatus.md#pdf-page-1501)
- 기능 설명: 전송 Sdo Ex 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SendSdoExCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SENDSDOEX_IN* pInParam,
OUT MMC_SENDSDOEX_OUT* pOutParam
);
```
```c
void DownloadSdoEx(ushort objectIndex, byte objectSubIndex,
long data, int timeout);
```
```c
void DownloadSdoEx(ushort objectIndex, byte objectSubIndex,
ulong data, int timeout);
```
```c
void DownloadSdoEx(ushort objectIndex, byte objectSubIndex,
double data, int timeout);
```
```c
void UploadSdoEx(
byte objectSubIndex,
uint dataLength,
out byte[] data,
int timeout);
```
```c
void UploadSdoEx(ushort objectIndex, byte objectSubIndex,
out byte[] data, int timeout);
```
```c
void UploadSdoEx(ushort objectIndex, byte objectSubIndex,
out long data, int timeout);
```
```c
void UploadSdoEx(ushort objectIndex, byte objectSubIndex,
out ulong data, int timeout);
```

#### 구조체/인자

##### `MMC_SENDSDOEX_IN`
| 필드 | 해석 |
|---|---|
| `SEND_SDO_DATA_EX uData;` | 데이터 버퍼 또는 데이터 값입니다. |
| `long pReserve[10];` | p Reserve[10] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usIndex;` | 인덱스 값입니다. |
| `unsigned char ucSubIndex;` | 인덱스 값입니다. |
| `unsigned char ucService;` | uc Service 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucDataLength;` | 데이터 버퍼 또는 데이터 값입니다. |

### 19.9.29 MMC_SendSdoAsync

- PDF 페이지: 1505
- 원문 위치: [19.9.29 MMC_SendSdoAsync](../chunks/059_p1488-p1509_19.9.24-MMC_GetBulkUploadStatus.md#pdf-page-1505)
- 기능 설명: 전송 Sdo 비동기 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SendSdoAsyncCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SENDSDO_IN* pInParam,
OUT MMC_SENDSDO_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SENDSDO_IN`
| 필드 | 해석 |
|---|---|
| `long lData;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned long ulDataLength;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned short usSlaveID;` | us Slave ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usIndex;` | 인덱스 값입니다. |
| `unsigned char ucSubIndex;` | 인덱스 값입니다. |
| `unsigned char ucService;` | uc Service 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SENDSDO_OUT`
| 필드 | 해석 |
|---|---|
| `long lData;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned long ulDataLength;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 19.9.30 MMC_RetrieveSDOAsync

- PDF 페이지: 1508
- 원문 위치: [19.9.30 MMC_RetrieveSDOAsync](../chunks/059_p1488-p1509_19.9.24-MMC_GetBulkUploadStatus.md#pdf-page-1508)
- 기능 설명: Retrieve Sdo 비동기 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_RetrieveSdoAsyncCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
OUT MMC_SENDSDO_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SENDSDO_OUT`
| 필드 | 해석 |
|---|---|
| `int32_t lData;` | 데이터 버퍼 또는 데이터 값입니다. |
| `uint32_t ulDataLength;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 19.9.31 MMC_SendSdoAsyncEx

- PDF 페이지: 1510
- 원문 위치: [19.9.31 MMC_SendSdoAsyncEx](../chunks/060_p1510-p1512_19.9.31-MMC_SendSdoAsyncEx.md#pdf-page-1510)
- 기능 설명: 전송 Sdo 비동기 Ex 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SendSdoAsyncExCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SENDSDOEX_IN* pInParam,
OUT MMC_SENDSDOEX_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SENDSDOEX_IN`
| 필드 | 해석 |
|---|---|
| `SEND_SDO_DATA_EX uData;` | 데이터 버퍼 또는 데이터 값입니다. |
| `long pReserve[10];` | p Reserve[10] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usIndex;` | 인덱스 값입니다. |
| `unsigned char ucSubIndex;` | 인덱스 값입니다. |
| `unsigned char ucService;` | uc Service 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucDataLength;` | 데이터 버퍼 또는 데이터 값입니다. |

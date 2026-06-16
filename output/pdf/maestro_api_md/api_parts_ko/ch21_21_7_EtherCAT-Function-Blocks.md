# 21.7 EtherCAT Function Blocks - API 분석

- 원본 장: `Chapter 21 EtherCAT Drive Communication`
- 시작 PDF 페이지: 1571
- 원문 위치: [21.7 EtherCAT Function Blocks](../chunks/062_p1552-p1589_Chapter-21-EtherCAT-Drive-Communication.md#pdf-page-1571)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `21.7.1` | 1572 | `MMC_DisableEthercatConfigMode` | 비활성화 Ethercat 구성 모드 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.2` | 1574 | `MMC_EnableEthercatConfigMode` | 활성화 Ethercat 구성 모드 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.3` | 1576 | `MMC_ECATIODisableDIChangedEvent` | ECATIODisable DIChanged 이벤트 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.4` | 1578 | `MMC_ECATIOEnableDIChangedEvent` | ECATIOEnable DIChanged 이벤트 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.5` | 1580 | `MMC_ECATIOReadDigitalInput` | ECATIORead 디지털 입력 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.6` | 1583 | `MMC_ECATIOReadAnalogInput` | ECATIORead Analog 입력 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.7` | 1586 | `MMC_ECATIOWriteAnalogOutput` | ECATIOWrite Analog 출력 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.8` | 1588 | `MMC_ECATIOWriteDigitalOutput` | ECATIOWrite 디지털 출력 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.9` | 1590 | `MMC_GetCommStatistics` | 조회 Comm Statistics 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.10` | 1594 | `MMC_GetEthercatCommStatistics` | 조회 Ethercat Comm Statistics 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.11` | 1602 | `MMC_GetCommDiagnostics` | 조회 Comm Diagnostics 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.12` | 1605 | `MMC_GetReactorStatistics` | 조회 Reactor Statistics 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed -Supported |
| `21.7.13` | 1608 | `MMC_IsEthercatConfigMode` | Is Ethercat 구성 모드 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.14` | 1610 | `MMC_ResetCommDiagnostics` | 리셋 Comm Diagnostics 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `21.7.15` | 1613 | `MMC_ResetCommStatistics` | 리셋 Comm Statistics 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 21.7.1 MMC_DisableEthercatConfigMode

- PDF 페이지: 1572
- 원문 위치: [21.7.1 MMC_DisableEthercatConfigMode](../chunks/062_p1552-p1589_Chapter-21-EtherCAT-Drive-Communication.md#pdf-page-1572)
- 기능 설명: 비활성화 Ethercat 구성 모드 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_DisableEthercatConfigMode(
IN MMC_CONNECT_HNDL hConn,
(IN MMC_DISABLE_ECATCONFIGMODE_IN* pInParam)
OUT MMC_DISABLE_ECATCONFIGMODE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_DISABLE_ECATCONFIGMODE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_DISABLE_ECATCONFIGMODE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 21.7.2 MMC_EnableEthercatConfigMode

- PDF 페이지: 1574
- 원문 위치: [21.7.2 MMC_EnableEthercatConfigMode](../chunks/062_p1552-p1589_Chapter-21-EtherCAT-Drive-Communication.md#pdf-page-1574)
- 기능 설명: 활성화 Ethercat 구성 모드 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_EnableEthercatConfigMode(
IN MMC_CONNECT_HNDL hConn,
OUT MMC_ENABLE_ECATCONFIGMODE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_ENABLE_ECATCONFIGMODE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 21.7.3 MMC_ECATIODisableDIChangedEvent

- PDF 페이지: 1576
- 원문 위치: [21.7.3 MMC_ECATIODisableDIChangedEvent](../chunks/062_p1552-p1589_Chapter-21-EtherCAT-Drive-Communication.md#pdf-page-1576)
- 기능 설명: ECATIODisable DIChanged 이벤트 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ECATIODisableDIChangedEvent (
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_DISABLEDICHANGEDEVENT_IN* pInParam,
OUT MMC_DISABLEDICHANGEDEVENT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_DISABLEDICHANGEDEVENT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_DISABLEDICHANGEDEVENT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 21.7.4 MMC_ECATIOEnableDIChangedEvent

- PDF 페이지: 1578
- 원문 위치: [21.7.4 MMC_ECATIOEnableDIChangedEvent](../chunks/062_p1552-p1589_Chapter-21-EtherCAT-Drive-Communication.md#pdf-page-1578)
- 기능 설명: ECATIOEnable DIChanged 이벤트 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_EnableDS401DIChangedEvent(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_ENABLEDICHANGEDEVENT_IN* pInParam,
OUT MMC_ENABLEDICHANGEDEVENT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_ENABLEDICHANGEDEVENT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_ENABLEDICHANGEDEVENT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 21.7.5 MMC_ECATIOReadDigitalInput

- PDF 페이지: 1580
- 원문 위치: [21.7.5 MMC_ECATIOReadDigitalInput](../chunks/062_p1552-p1589_Chapter-21-EtherCAT-Drive-Communication.md#pdf-page-1580)
- 기능 설명: ECATIORead 디지털 입력 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ECATIOReadDigitalInput (
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READDI_IN* pInParam,
OUT MMC_READDI_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READDI_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_READDI_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned __int64 ulliDI;` | ulli DI 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long long int ulliDI;` | ulli DI 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 21.7.6 MMC_ECATIOReadAnalogInput

- PDF 페이지: 1583
- 원문 위치: [21.7.6 MMC_ECATIOReadAnalogInput](../chunks/062_p1552-p1589_Chapter-21-EtherCAT-Drive-Communication.md#pdf-page-1583)
- 기능 설명: ECATIORead Analog 입력 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ECATIOReadAnalogInput(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READAI_IN* pInParam,
OUT MMC_READAI_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READAI_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucIndex;` | 인덱스 값입니다. |

##### `MMC_READAI_OUT`
| 필드 | 해석 |
|---|---|
| `short sAI;` | s AI 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 21.7.7 MMC_ECATIOWriteAnalogOutput

- PDF 페이지: 1586
- 원문 위치: [21.7.7 MMC_ECATIOWriteAnalogOutput](../chunks/062_p1552-p1589_Chapter-21-EtherCAT-Drive-Communication.md#pdf-page-1586)
- 기능 설명: ECATIOWrite Analog 출력 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ECATIOWriteAnalogOutput(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEAO_IN* pInParam,
OUT MMC_WRITEAO_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEAO_IN`
| 필드 | 해석 |
|---|---|
| `short sAO;` | s AO 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucIndex;` | 인덱스 값입니다. |

##### `MMC_WRITEAO_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 21.7.8 MMC_ECATIOWriteDigitalOutput

- PDF 페이지: 1588
- 원문 위치: [21.7.8 MMC_ECATIOWriteDigitalOutput](../chunks/062_p1552-p1589_Chapter-21-EtherCAT-Drive-Communication.md#pdf-page-1588)
- 기능 설명: ECATIOWrite 디지털 출력 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ECATIOWriteDigitalOutput(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEDO_IN* pInParam,
OUT MMC_WRITEDO_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEDO_IN`
| 필드 | 해석 |
|---|---|
| `unsigned __int64 ulliDO;` | ulli DO 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long long int ulliDO;` | ulli DO 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_WRITEDO_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 21.7.9 MMC_GetCommStatistics

- PDF 페이지: 1590
- 원문 위치: [21.7.9 MMC_GetCommStatistics](../chunks/063_p1590-p1612_21.7.9-MMC_GetCommStatistics.md#pdf-page-1590)
- 기능 설명: 조회 Comm Statistics 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetCommStatistics(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETCOMMSTATISTICS_IN* pInParam,
OUT MMC_GETCOMMSTATISTICS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETCOMMSTATISTICS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usAxesRef /*usSlaveID*/;` | unsigned short us 축 Ref /*us Slave ID*/ 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETCOMMSTATISTICS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long dwSendErrors;` | 오류 ID입니다. |
| `unsigned long dwReceiveErrors;` | 오류 ID입니다. |
| `unsigned long dwWrongWC;` | dw Wrong WC 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long dwParseErrors;` | 오류 ID입니다. |
| `unsigned short usNumOfSlaves;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucMasterState;` | uc Master State 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucSlaveState;` | uc Slave State 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 21.7.10 MMC_GetEthercatCommStatistics

- PDF 페이지: 1594
- 원문 위치: [21.7.10 MMC_GetEthercatCommStatistics](../chunks/063_p1590-p1612_21.7.9-MMC_GetCommStatistics.md#pdf-page-1594)
- 기능 설명: 조회 Ethercat Comm Statistics 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetEthercatCommStatistics(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETCOMMSTATISTICSEX_IN* pInParam,
OUT MMC_GETCOMMSTATISTICSEX_OUT* pOutParam
);
```
```c
void DownloadFoe()
{
MMC_DOWNLOADFOE_IN dlfoe ;
MMC_DOWNLOADFOE_OUT dlfoeout ;
MMC_GETFOESTATUS_OUT foestat ;
MMC_GET_GMASOP_MODE_OUT pOpmode ;
MMC_GETCOMMSTATISTICSEX_IN gcstat_In ;
MMC_GETCOMMSTATISTICSEX_OUT gcstat_Out ;
int i ;
//
//
// Before DownloadingFOE - It is good practice that drives will be
reset because
// if one of the drives is after DownloadFoE and was not reset, its
state and statitstics are unknown.
//
dlfoe.pwSlaveId[0]=0 ; // Note: Slave ID is inserted here !!
dlfoe.pwSlaveId[1]=1 ; // Note: Slave ID is inserted here !!
//
dlfoe.ucSlavesNum = 2; // Number of relevant slaves in the
pwSlaveId array.
//
// Same for slave statistics:
gcstat_In.pwSlaveId[0] = 0 ;
gcstat_In.pwSlaveId[1] = 1 ;
gcstat_In.ucSlavesNum = 2 ;
//
// Insert IP of tftp server. Usually the connection IP of the PC.
dlfoe.pucServer[0] = 10 ;
```
```c
int OnConnectGetDiagnostics()
{
int rc ;
MMC_GET_GMASOP_MODE_OUT pOpmode ;
MMC_GETCOMMSTATISTICSEX_IN gcstat_In ;
MMC_GETCOMMSTATISTICSEX_OUT gcstat_Out ;
int i ;
//
rc = MMC_GetGMASOperationMode(conn_hndl,&pOpmode) ;
if(rc < 0)
{
// Error Calling MMC_GetGMASOperationMode. Error in pOpmode.sErrorID
return ;
}
//
// Check GMAS Operational state. If == 2, then in Download FOE state.
if (pOpmode.ucResult == 2)
{
// GMAS in Download FoE state. We decided that a mesage will be
shown to user that the GMAS is in Download FoE.
}
rc = MMC_GetEthercatCommStatistics(conn_hndl,&gcstat_In,&gcstat_Out) ;
if(rc < 0)
{
// Error Calling MMC_GetEthercatCommStatistics. Error in
gcstat_Out.sErrorID
return ;
```

#### 구조체/인자

##### `MMC_GETCOMMSTATISTICSEX_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short pwSlaveId[ETHERCAT_STATISTICSEX_MAX_SLAVES];` | unsigned short pw Slave Id[ETHERCAT STATISTICSEX MAX SLAVES] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucSlavesNum;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_GETCOMMSTATISTICSEX_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long dwSendErrors;` | 오류 ID입니다. |
| `unsigned long dwReceiveErrors;` | 오류 ID입니다. |
| `unsigned long dwWrongWC;` | dw Wrong WC 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long dwParseErrors;` | 오류 ID입니다. |
| `MMC_ECAT_SII_CONTENT pstSII_Content[ETHERCAT_STATISTICSEX_MAX_SLAVES];` | ECAT SII CONTENT pst SII Content[ETHERCAT STATISTICSEX MAX SLAVES] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usNumOfSlaves;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucMasterState;` | uc Master State 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char pucAxesState[ETHERCAT_STATISTICSEX_MAX_SLAVES];` | unsigned char puc 축 State[ETHERCAT STATISTICSEX MAX SLAVES] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char pucAxesDiagnosticState[ETHERCAT_STATISTICSEX_MAX_SLAVES];` | unsigned char puc 축 Diagnostic State[ETHERCAT STATISTICSEX MAX SLAVES] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucMasterDiagnosticState;` | uc Master Diagnostic State 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 21.7.11 MMC_GetCommDiagnostics

- PDF 페이지: 1602
- 원문 위치: [21.7.11 MMC_GetCommDiagnostics](../chunks/063_p1590-p1612_21.7.9-MMC_GetCommStatistics.md#pdf-page-1602)
- 기능 설명: 조회 Comm Diagnostics 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetCommDiagnostics(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETCOMMDIAGNOSTICS_IN* pInParam,
OUT MMC_GETCOMMDIAGNOSTICS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETCOMMDIAGNOSTICS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETCOMMDIAGNOSTICS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `MMC_ETHERCAT_DIAGNOSTICS_INFO pDiagnosticsSlavesArr[ETHERCAT_ID_MAX];` | ETHERCAT DIAGNOSTICS INFO p Diagnostics Slaves Arr[ETHERCAT ID MAX] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 21.7.12 MMC_GetReactorStatistics

- PDF 페이지: 1605
- 원문 위치: [21.7.12 MMC_GetReactorStatistics](../chunks/063_p1590-p1612_21.7.9-MMC_GetCommStatistics.md#pdf-page-1605)
- 기능 설명: 조회 Reactor Statistics 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed -Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetReactorStatistics(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETREACTORSTATISTICS_IN* pInParam,
OUT MMC_GETREACTORSTATISTICS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETREACTORSTATISTICS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETREACTORSTATISTICS_OUT`
| 필드 | 해석 |
|---|---|
| `int iReactorQueueSize;` | 토크 관련 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 21.7.13 MMC_IsEthercatConfigMode

- PDF 페이지: 1608
- 원문 위치: [21.7.13 MMC_IsEthercatConfigMode](../chunks/063_p1590-p1612_21.7.9-MMC_GetCommStatistics.md#pdf-page-1608)
- 기능 설명: Is Ethercat 구성 모드 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_IsEthercatConfigMode(
IN MMC_CONNECT_HNDL hConn,
IN MMC_IS_ECATCONFIGMODE_IN* pInParam
OUT MMC_IS_ECATCONFIGMODE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_IS_ECATCONFIGMODE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_IS_ECATCONFIGMODE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucResult;` | uc Result 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 21.7.14 MMC_ResetCommDiagnostics

- PDF 페이지: 1610
- 원문 위치: [21.7.14 MMC_ResetCommDiagnostics](../chunks/063_p1590-p1612_21.7.9-MMC_GetCommStatistics.md#pdf-page-1610)
- 기능 설명: 리셋 Comm Diagnostics 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ResetCommDiagnostics(
IN MMC_CONNECT_HNDL hConn,
IN MMC_RESETCOMMDIAGNOSTICS_IN* pInParam,
OUT MMC_RESETCOMMDIAGNOSTICS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_RESETCOMMDIAGNOSTICS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_RESETCOMMDIAGNOSTICS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 21.7.15 MMC_ResetCommStatistics

- PDF 페이지: 1613
- 원문 위치: [21.7.15 MMC_ResetCommStatistics](../chunks/064_p1613-p1614_21.7.15-MMC_ResetCommStatistics.md#pdf-page-1613)
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

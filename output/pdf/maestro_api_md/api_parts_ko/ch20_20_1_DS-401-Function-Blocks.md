# 20.1 DS-401 Function Blocks - API 분석

- 원본 장: `Chapter 20 DS-401 CANbus I/O Communications`
- 시작 PDF 페이지: 1513
- 원문 위치: [20.1 DS-401 Function Blocks](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1513)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `20.1.1` | 1514 | `MMC_CancelGeneralRPDO3` | Cancel General RPDO3 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.2` | 1516 | `MMC_CancelGeneralRPDO4` | Cancel General RPDO4 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.3` | 1519 | `MMC_CancelGeneralTPDO3` | Cancel General TPDO3 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.4` | 1521 | `MMC_CancelGeneralTPDO4` | Cancel General TPDO4 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.5` | 1524 | `MMC_ConfigGeneralRPDO3` | 구성 General RPDO3 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.6` | 1526 | `MMC_ConfigGeneralRPDO4` | 구성 General RPDO4 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.7` | 1529 | `MMC_ConfigGeneralTPDO3` | 구성 General TPDO3 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.8` | 1531 | `MMC_ConfigGeneralTPDO4` | 구성 General TPDO4 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.9` | 1534 | `MMC_DisableDS401DIChangedEvent` | 비활성화 DS401 DIChanged 이벤트 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.10` | 1537 | `MMC_EnableDS401DIChangedEvent` | 활성화 DS401 DIChanged 이벤트 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.11` | 1540 | `MMC_ReadDS401DIGroup` | 읽기 DS401 DIGroup 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.12` | 1543 | `MMC_ReadDS401DInput` | 읽기 DS401 DInput 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.13` | 1546 | `MMC_WriteDS401DOGroup` | 쓰기 DS401 DOGroup 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `20.1.14` | 1549 | `MMC_WriteDS401DOutput` | 쓰기 DS401 DOutput 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 20.1.1 MMC_CancelGeneralRPDO3

- PDF 페이지: 1514
- 원문 위치: [20.1.1 MMC_CancelGeneralRPDO3](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1514)
- 기능 설명: Cancel General RPDO3 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CancelGeneralRPDO3(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELGENERALRPDO3_IN* pInParam,
OUT MMC_CANCELGENERALRPDO3_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CANCELGENERALRPDO3_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CANCELGENERALRPDO3_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20.1.2 MMC_CancelGeneralRPDO4

- PDF 페이지: 1516
- 원문 위치: [20.1.2 MMC_CancelGeneralRPDO4](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1516)
- 기능 설명: Cancel General RPDO4 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CancelGeneralRPDO4(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELGENERALRPDO4_IN* pInParam,
OUT MMC_CANCELGENERALRPDO4_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CANCELGENERALRPDO4_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CANCELGENERALRPDO4_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20.1.3 MMC_CancelGeneralTPDO3

- PDF 페이지: 1519
- 원문 위치: [20.1.3 MMC_CancelGeneralTPDO3](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1519)
- 기능 설명: Cancel General TPDO3 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CancelGeneralTPDO3(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELGENERALTPDO3_IN* pInParam,
OUT MMC_CANCELGENERALTPDO3_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CANCELGENERALTPDO3_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CANCELGENERALTPDO3_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20.1.4 MMC_CancelGeneralTPDO4

- PDF 페이지: 1521
- 원문 위치: [20.1.4 MMC_CancelGeneralTPDO4](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1521)
- 기능 설명: Cancel General TPDO4 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CancelGeneralTPDO4(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CANCELGENERALTPDO4_IN* pInParam,
OUT MMC_CANCELGENERALTPDO4_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CANCELGENERALTPDO4_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CANCELGENERALTPDO4_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20.1.5 MMC_ConfigGeneralRPDO3

- PDF 페이지: 1524
- 원문 위치: [20.1.5 MMC_ConfigGeneralRPDO3](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1524)
- 기능 설명: 구성 General RPDO3 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ConfigGeneralRPDO3(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGGENERALRPDO3_IN* pInParam,
OUT MMC_CONFIGGENERALRPDO3_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGGENERALRPDO3_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEventType;` | 데이터 또는 동작 타입 값입니다. |
| `unsigned char ucPDOCommParam;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned char ucPDOLength;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_CONFIGGENERALRPDO3_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20.1.6 MMC_ConfigGeneralRPDO4

- PDF 페이지: 1526
- 원문 위치: [20.1.6 MMC_ConfigGeneralRPDO4](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1526)
- 기능 설명: 구성 General RPDO4 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ConfigGeneralRPDO4(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGGENERALRPDO4_IN* pInParam,
OUT MMC_CONFIGGENERALRPDO4_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGGENERALRPDO4_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEventType;` | 데이터 또는 동작 타입 값입니다. |
| `unsigned char ucPDOCommParam;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned char ucPDOLength;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_CONFIGGENERALRPDO4_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20.1.7 MMC_ConfigGeneralTPDO3

- PDF 페이지: 1529
- 원문 위치: [20.1.7 MMC_ConfigGeneralTPDO3](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1529)
- 기능 설명: 구성 General TPDO3 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ConfigGeneralTPDO3(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGGENERALTPDO3_IN* pInParam,
OUT MMC_CONFIGGENERALTPDO3_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGGENERALTPDO3_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20.1.8 MMC_ConfigGeneralTPDO4

- PDF 페이지: 1531
- 원문 위치: [20.1.8 MMC_ConfigGeneralTPDO4](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1531)
- 기능 설명: 구성 General TPDO4 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ConfigGeneralTPDO4(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CONFIGGENERALTPDO4_IN* pInParam,
OUT MMC_CONFIGGENERALTPDO4_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CONFIGGENERALTPDO4_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEventType;` | 데이터 또는 동작 타입 값입니다. |

##### `MMC_CONFIGGENERALTPDO4_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20.1.9 MMC_DisableDS401DIChangedEvent

- PDF 페이지: 1534
- 원문 위치: [20.1.9 MMC_DisableDS401DIChangedEvent](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1534)
- 기능 설명: 비활성화 DS401 DIChanged 이벤트 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_DisableDS401DIChangedEvent(
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

### 20.1.10 MMC_EnableDS401DIChangedEvent

- PDF 페이지: 1537
- 원문 위치: [20.1.10 MMC_EnableDS401DIChangedEvent](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1537)
- 기능 설명: 활성화 DS401 DIChanged 이벤트 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_EnableDS401DIChangedEvent (
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

### 20.1.11 MMC_ReadDS401DIGroup

- PDF 페이지: 1540
- 원문 위치: [20.1.11 MMC_ReadDS401DIGroup](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1540)
- 기능 설명: 읽기 DS401 DIGroup 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadDS401DIGroup(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READDIGROUP_IN* pInParam,
OUT MMC_READDIGROUP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READDIGROUP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucGroupIndex;` | 인덱스 값입니다. |

##### `MMC_READDIGROUP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20.1.12 MMC_ReadDS401DInput

- PDF 페이지: 1543
- 원문 위치: [20.1.12 MMC_ReadDS401DInput](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1543)
- 기능 설명: 읽기 DS401 DInput 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadDS401DInput(
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

### 20.1.13 MMC_WriteDS401DOGroup

- PDF 페이지: 1546
- 원문 위치: [20.1.13 MMC_WriteDS401DOGroup](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1546)
- 기능 설명: 쓰기 DS401 DOGroup 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WriteDS401DOGroup(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEDOGROUP_IN* pInParam,
OUT MMC_WRITEDOGROUP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEDOGROUP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucGroupIndex;` | 인덱스 값입니다. |
| `unsigned char ucVal;` | 전달하거나 반환받는 값입니다. |

##### `MMC_WRITEDOGROUP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20.1.14 MMC_WriteDS401DOutput

- PDF 페이지: 1549
- 원문 위치: [20.1.14 MMC_WriteDS401DOutput](../chunks/061_p1513-p1551_Chapter-20-DS-401-CANbus-I-O-Communications.md#pdf-page-1549)
- 기능 설명: 쓰기 DS401 DOutput 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WriteDS401DOutput(
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

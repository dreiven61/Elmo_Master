# 18.1 Modbus Communication Function Blocks - API 분석

- 원본 장: `Chapter 18 Host Communication`
- 시작 PDF 페이지: 1387
- 원문 위치: [18.1 Modbus Communication Function Blocks](../chunks/056_p1387-p1409_Chapter-18-Host-Communication.md#pdf-page-1387)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `18.1.1` | 1388 | `MMC_MbusIsRunning` | Mbus Is Running 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `18.1.2` | 1390 | `MMC_MbusReadCoilsTable` | Mbus 읽기 Coils 테이블 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `18.1.3` | 1393 | `MMC_MbusReadHoldingRegisterTable` | Mbus 읽기 Holding 등록 테이블 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `18.1.4` | 1396 | `MMC_MbusReadInputsTable` | Mbus 읽기 입력 테이블 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `18.1.5` | 1399 | `MMC_MbusStartServer` | 값 또는 상태를 읽는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `18.1.6` | 1401 | `MMC_MbusStopServer` | 축 또는 동작을 정지 상태로 전환하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `18.1.7` | 1404 | `MMC_MbusWriteCoilsTable` | Mbus 쓰기 Coils 테이블 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `18.1.8` | 1407 | `MMC_MbusWriteHoldingRegisterTable` | Mbus 쓰기 Holding 등록 테이블 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 18.1.1 MMC_MbusIsRunning

- PDF 페이지: 1388
- 원문 위치: [18.1.1 MMC_MbusIsRunning](../chunks/056_p1387-p1409_Chapter-18-Host-Communication.md#pdf-page-1388)
- 기능 설명: Mbus Is Running 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MbusIsRunning(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSISRUNNING_IN* pInParam,
OUT MMC_MODBUSISRUNNING_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MODBUSISRUNNING_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MODBUSISRUNNING_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short isrunning;` | isrunning 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 18.1.2 MMC_MbusReadCoilsTable

- PDF 페이지: 1390
- 원문 위치: [18.1.2 MMC_MbusReadCoilsTable](../chunks/056_p1387-p1409_Chapter-18-Host-Communication.md#pdf-page-1390)
- 기능 설명: Mbus 읽기 Coils 테이블 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MbusReadCoilsTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSREADCOILS_IN* pInParam,
OUT MMC_MODBUSREADCOILS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MODBUSREADCOILS_IN`
| 필드 | 해석 |
|---|---|
| `int startRef;` | start Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `int refCnt;` | ref Cnt 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MODBUSREADCOILS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `char coilsArr[MODBUS_IPC_READ_VALUES];` | 전달하거나 반환받는 값입니다. |

### 18.1.3 MMC_MbusReadHoldingRegisterTable

- PDF 페이지: 1393
- 원문 위치: [18.1.3 MMC_MbusReadHoldingRegisterTable](../chunks/056_p1387-p1409_Chapter-18-Host-Communication.md#pdf-page-1393)
- 기능 설명: Mbus 읽기 Holding 등록 테이블 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MbusReadHoldingRegisterTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSREADHOLDINGREGISTERSTABLE_IN *pInParam,
OUT MMC_MODBUSREADHOLDINGREGISTERSTABLE_OUT *pOutParam
);
```

#### 구조체/인자

##### `MMC_MODBUSREADHOLDINGREGISTERSTABLE_IN`
| 필드 | 해석 |
|---|---|
| `int startRef;` | start Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `int refCnt;` | ref Cnt 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MODBUSREADHOLDINGREGISTERSTABLE_OUT`
| 필드 | 해석 |
|---|---|
| `short regArr[MODBUS_IPC_READ_VALUES];` | 전달하거나 반환받는 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 18.1.4 MMC_MbusReadInputsTable

- PDF 페이지: 1396
- 원문 위치: [18.1.4 MMC_MbusReadInputsTable](../chunks/056_p1387-p1409_Chapter-18-Host-Communication.md#pdf-page-1396)
- 기능 설명: Mbus 읽기 입력 테이블 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MbusReadInputsTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSREADINPUTS_IN *pInParam,
OUT MMC_MODBUSREADINPUTS_OUT *pOutParam
);
```

#### 구조체/인자

##### `MMC_MODBUSREADINPUTS_IN`
| 필드 | 해석 |
|---|---|
| `int startRef;` | start Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `int refCnt;` | ref Cnt 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MODBUSREADINPUTS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `char inputsArr[MODBUS_IPC_READ_VALUES];` | 전달하거나 반환받는 값입니다. |

### 18.1.5 MMC_MbusStartServer

- PDF 페이지: 1399
- 원문 위치: [18.1.5 MMC_MbusStartServer](../chunks/056_p1387-p1409_Chapter-18-Host-Communication.md#pdf-page-1399)
- 기능 설명: 값 또는 상태를 읽는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MbusStartServer(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSSTARTSERVER_IN *pInParam,
OUT MMC_MODBUSSTARTSERVER_OUT *pOutParam
);
```

#### 구조체/인자

##### `MMC_MODBUSSTARTSERVER_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short id;` | id 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MODBUSSTARTSERVER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 18.1.6 MMC_MbusStopServer

- PDF 페이지: 1401
- 원문 위치: [18.1.6 MMC_MbusStopServer](../chunks/056_p1387-p1409_Chapter-18-Host-Communication.md#pdf-page-1401)
- 기능 설명: 축 또는 동작을 정지 상태로 전환하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MbusStopServer(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSSTOPSERVER_IN* pInParam,
OUT MMC_MODBUSSTOPSERVER_OUT *pOutParam
);
```

#### 구조체/인자

##### `MMC_MODBUSSTOPSERVER_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MODBUSSTOPSERVER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 18.1.7 MMC_MbusWriteCoilsTable

- PDF 페이지: 1404
- 원문 위치: [18.1.7 MMC_MbusWriteCoilsTable](../chunks/056_p1387-p1409_Chapter-18-Host-Communication.md#pdf-page-1404)
- 기능 설명: Mbus 쓰기 Coils 테이블 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MbusWriteCoilsTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSWRITECOILS_IN *pInParam,
OUT MMC_MODBUSWRITECOILS_OUT *pOutParam
);
```

#### 구조체/인자

##### `MMC_MODBUSWRITECOILS_IN`
| 필드 | 해석 |
|---|---|
| `int startRef;` | start Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `int refCnt;` | ref Cnt 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char coilsArr[MODBUS_IPC_WRITE_VALUES];` | 전달하거나 반환받는 값입니다. |

##### `MMC_MODBUSWRITECOILS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 18.1.8 MMC_MbusWriteHoldingRegisterTable

- PDF 페이지: 1407
- 원문 위치: [18.1.8 MMC_MbusWriteHoldingRegisterTable](../chunks/056_p1387-p1409_Chapter-18-Host-Communication.md#pdf-page-1407)
- 기능 설명: Mbus 쓰기 Holding 등록 테이블 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MbusWriteHoldingRegisterTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_IN* pInParam,
OUT MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_IN`
| 필드 | 해석 |
|---|---|
| `int startRef;` | start Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `int refCnt;` | ref Cnt 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `short regArr[MODBUS_IPC_WRITE_VALUES];` | 전달하거나 반환받는 값입니다. |

##### `MMC_MODBUSWRITEHOLDINGREGISTERSTABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

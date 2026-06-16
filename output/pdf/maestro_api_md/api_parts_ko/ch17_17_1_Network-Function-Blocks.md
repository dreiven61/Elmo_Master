# 17.1 Network Function Blocks - API 분석

- 원본 장: `Chapter 17 Network Connectivity and Configuration`
- 시작 PDF 페이지: 1344
- 원문 위치: [17.1 Network Function Blocks](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1344)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `17.1.1` | 1345 | `MMC_CloseUdpChannelCmd` | 닫기 Udp Channel 작업을 수행하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.2` | 1348 | `MMC_GetDefGatewayCmd` | 조회 Def Gateway 값/상태를 조회하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.3` | 1351 | `MMC_GetDhcpCmd` | 조회 Dhcp 값/상태를 조회하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.4` | 1353 | `MMC_GetIpAddrCmd` | 조회 Ip Addr 값/상태를 조회하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.5` | 1356 | `MMC_GetIpMaskCmd` | 조회 Ip 마스크 값/상태를 조회하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.6` | 1359 | `MMC_GetServerIpCmd` | 조회 서버 Ip 값/상태를 조회하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.7` | 1362 | `MMC_NetworkInfoCmd` | 네트워크 정보 작업을 수행하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.8` | 1367 | `MMC_NetworkScanCmd` | 네트워크 Scan 작업을 수행하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.9` | 1370 | `MMC_OpenUdpChannelCmd` | 열기 Udp Channel 작업을 수행하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.10` | 1373 | `MMC_SetDefGatewayCmd` | 설정 Def Gateway 값/설정을 적용하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.11` | 1375 | `MMC_SetDhcpCmd` | 설정 Dhcp 값/설정을 적용하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.12` | 1378 | `MMC_SetIpAddrCmd` | 설정 Ip Addr 값/설정을 적용하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.13` | 1381 | `MMC_SetIpMaskCmd` | 설정 Ip 마스크 값/설정을 적용하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |
| `17.1.14` | 1384 | `MMC_SetServerIpCmd` | 설정 서버 Ip 값/설정을 적용하는 API입니다. | Motion Mode NC - Not Relevant Distributed - Not Relevant |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 17.1.1 MMC_CloseUdpChannel

- PDF 페이지: 1345
- 원문 위치: [17.1.1 MMC_CloseUdpChannel](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1345)
- 기능 설명: 닫기 Udp Channel 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_CloseUdpChannelCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_CLOSEUDPCHANNEL_IN* pInParam,
OUT MMC_CLOSEUDPCHANNEL_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CLOSEUDPCHANNEL_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CLOSEUDPCHANNEL_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 17.1.2 MMC_GetDefGateway

- PDF 페이지: 1348
- 원문 위치: [17.1.2 MMC_GetDefGateway](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1348)
- 기능 설명: 조회 Def Gateway 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetDefGatewayCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GET_DEFGATEWAY_IN* pInParam,
OUT MMC_GET_DEFGATEWAY_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GET_DEFGATEWAY_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GET_DEFGATEWAY_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char cFirst;` | c First 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char cSecond;` | c Second 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char cThird;` | c Third 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char cFourth;` | c Fourth 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 17.1.3 MMC_GetDhcp

- PDF 페이지: 1351
- 원문 위치: [17.1.3 MMC_GetDhcp](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1351)
- 기능 설명: 조회 Dhcp 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetDhcpCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GET_DHCP_IN* pInParam,
OUT MMC_GET_DHCP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GET_DHCP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GET_DHCP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucMode;` | 동작 모드 값입니다. |

### 17.1.4 IMMC_GetIpAddr

- PDF 페이지: 1353
- 원문 위치: [17.1.4 IMMC_GetIpAddr](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1353)
- 기능 설명: 조회 Ip Addr 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetIpAddrCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GET_IP_ADDRESS_IN* pInParam,
OUT MMC_GET_IP_ADDRESS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GET_IP_ADDRESS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GET_IP_ADDRESS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `char cFirst;` | c First 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cSecond;` | c Second 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cThird;` | c Third 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cFourth;` | c Fourth 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 17.1.5 MMC_GetIpMask

- PDF 페이지: 1356
- 원문 위치: [17.1.5 MMC_GetIpMask](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1356)
- 기능 설명: 조회 Ip 마스크 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetIpMaskCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GET_IP_MASK_IN* pInParam,
OUT MMC_GET_IP_MASK_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GET_IP_MASK_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GET_IP_MASK_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `char cFirst;` | c First 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cSecond;` | c Second 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cThird;` | c Third 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cFourth;` | c Fourth 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 17.1.6 MMC_GetServerIp

- PDF 페이지: 1359
- 원문 위치: [17.1.6 MMC_GetServerIp](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1359)
- 기능 설명: 조회 서버 Ip 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetServerIpCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GET_SERVERIP_IN* pInParam,
OUT MMC_GET_SERVERIP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GET_SERVERIP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GET_SERVERIP_OUT`
| 필드 | 해석 |
|---|---|
| `int iPort;` | 주소 또는 IP 관련 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `char cFirst;` | c First 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cSecond;` | c Second 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cThird;` | c Third 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cFourth;` | c Fourth 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 17.1.7 MMC_NetworkInfo

- PDF 페이지: 1362
- 원문 위치: [17.1.7 MMC_NetworkInfo](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1362)
- 기능 설명: 네트워크 정보 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_NetworkInfoCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_NETWORKINFO_IN* pInParam,
OUT MMC_NETWORKINFO_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_NETWORKINFO_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_NETWORKINFO_OUT`
| 필드 | 해석 |
|---|---|
| `int iBusType;` | 데이터 또는 동작 타입 값입니다. |
| `int iBusBaud;` | i Bus Baud 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `int iNumOfActiveNodes;` | 노드 식별 또는 노드 관련 값입니다. |
| `int iNumOfResFileNodes;` | 노드 식별 또는 노드 관련 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `MMC_NODESINFO stNodesInfo[CAN_ID_MAX - 1];` | 노드 식별 또는 노드 관련 값입니다. |

### 17.1.8 MMC_NetworkScan

- PDF 페이지: 1367
- 원문 위치: [17.1.8 MMC_NetworkScan](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1367)
- 기능 설명: 네트워크 Scan 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_NetworkScanCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_NETWORKSCAN_IN* pInParam,
OUT MMC_NETWORKSCAN_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_NETWORKSCAN_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_NETWORKSCAN_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 17.1.9 MMC_OpenUdpChannel

- PDF 페이지: 1370
- 원문 위치: [17.1.9 MMC_OpenUdpChannel](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1370)
- 기능 설명: 열기 Udp Channel 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_OpenUdpChannelCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_OPENUDPCHANNEL_IN* pInParam,
OUT MMC_OPENUDPCHANNEL_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_OPENUDPCHANNEL_IN`
| 필드 | 해석 |
|---|---|
| `int iEventsMask;` | 마스크 값입니다. |
| `int iPort;` | 주소 또는 IP 관련 값입니다. |
| `char cFirst;` | c First 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cSecond;` | c Second 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cThird;` | c Third 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cFourth;` | c Fourth 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_OPENUDPCHANNEL_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 17.1.10 MMC_SetDefGateway

- PDF 페이지: 1373
- 원문 위치: [17.1.10 MMC_SetDefGateway](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1373)
- 기능 설명: 설정 Def Gateway 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_SetDefGatewayCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SET_DEFGATEWAY_IN* pInParam,
OUT MMC_SET_DEFGATEWAY_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SET_DEFGATEWAY_IN`
| 필드 | 해석 |
|---|---|
| `char cFirst;` | c First 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cSecond;` | c Second 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cThird;` | c Third 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cFourth;` | c Fourth 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SET_DEFGATEWAY_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 17.1.11 MMC_SetDhcp

- PDF 페이지: 1375
- 원문 위치: [17.1.11 MMC_SetDhcp](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1375)
- 기능 설명: 설정 Dhcp 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_SetDhcpCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SET_DHCP_IN* pInParam,
OUT MMC_SET_DHCP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SET_DHCP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucMode;` | 동작 모드 값입니다. |

### 17.1.12 MMC_SetIpAddr

- PDF 페이지: 1378
- 원문 위치: [17.1.12 MMC_SetIpAddr](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1378)
- 기능 설명: 설정 Ip Addr 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_SetIpAddrCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SET_IP_ADDRESS_IN* pInParam,
OUT MMC_SET_IP_ADDRESS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SET_IP_ADDRESS_IN`
| 필드 | 해석 |
|---|---|
| `char cFirst;` | c First 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cSecond;` | c Second 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cThird;` | c Third 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cFourth;` | c Fourth 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SET_IP_ADDRESS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 17.1.13 MMC_SetIpMask

- PDF 페이지: 1381
- 원문 위치: [17.1.13 MMC_SetIpMask](../chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1381)
- 기능 설명: 설정 Ip 마스크 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_SetIpMaskCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SET_IP_MASK_IN* pInParam,
OUT MMC_SET_IP_MASK_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SET_IP_MASK_IN`
| 필드 | 해석 |
|---|---|
| `char cFirst;` | c First 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cSecond;` | c Second 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cThird;` | c Third 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cFourth;` | c Fourth 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SET_IP_MASK_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 17.1.14 MMC_SetServerIp

- PDF 페이지: 1384
- 원문 위치: [17.1.14 MMC_SetServerIp](../chunks/055_p1384-p1386_17.1.14-MMC_SetServerIp.md#pdf-page-1384)
- 기능 설명: 설정 서버 Ip 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 시그니처

```c
MMC_LIB_API int MMC_SetServerIpCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SET_SERVERIP_IN* pInParam,
OUT MMC_SET_SERVERIP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SET_SERVERIP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char cFirst;` | c First 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char cSecond;` | c Second 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char cThird;` | c Third 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char cFourth;` | c Fourth 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SET_SERVERIP_OUT`
| 필드 | 해석 |
|---|---|
| `short sErrorID;` | 오류 ID입니다. |

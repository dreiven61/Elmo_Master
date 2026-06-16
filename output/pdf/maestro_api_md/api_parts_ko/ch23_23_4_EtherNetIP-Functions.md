# 23.4 EtherNetIP Functions - API 분석

- 원본 장: `Chapter 23 EtherNetIP Communication`
- 시작 PDF 페이지: 1661
- 원문 위치: [23.4 EtherNetIP Functions](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1661)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `23.4.1` | 1662 | `EipGetAdpTagRefByName` | Eip 조회 Adp Tag Ref By Name 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.2` | 1664 | `EipWriteAdpTag` | Eip 쓰기 Adp Tag 값/설정을 적용하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.3` | 1666 | `EipReadAdpTag` | Eip 읽기 Adp Tag 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.4` | 1668 | `EipGetAssemblyRefByInstance` | Eip 조회 Assembly Ref By Instance 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.5` | 1670 | `EipGetAssemblyRefByName` | Eip 조회 Assembly Ref By Name 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.6` | 1672 | `EipSetAssembly` | Eip 설정 Assembly 값/설정을 적용하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.7` | 1674 | `EipGetAssembly` | Eip 조회 Assembly 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.8` | 1676 | `EipGetDevTagRefByName` | Eip 조회 Dev Tag Ref By Name 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.9` | 1678 | `EipSetDevTag` | Eip 설정 Dev Tag 값/설정을 적용하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.10` | 1680 | `EipGetDevTag` | Eip 조회 Dev Tag 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.11` | 1682 | `EipReadDevTagData` | Eip 읽기 Dev Tag 데이터 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.12` | 1684 | `EipSyncGetDevTag` | Eip 동기 조회 Dev Tag 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.13` | 1686 | `EipCheckDevTagReply` | Eip 확인 Dev Tag Reply 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.14` | 1688 | `EipOpenSession` | Eip 열기 Session 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.15` | 1692 | `EIPCloseSession` | EIPClose Session 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.16` | 1694 | `EipCreate` | Eip 생성 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `23.4.17` | 1696 | `EipDestroy` | Eip 삭제 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 23.4.1 EipGetAdpTagRefByName

- PDF 페이지: 1662
- 원문 위치: [23.4.1 EipGetAdpTagRefByName](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1662)
- 기능 설명: Eip 조회 Adp Tag Ref By Name 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipGetDevTagRefByName(
IN EIP_REFBYNAME_IN *pInParam,
OUT EIP_REFBYNAME_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_REFBYNAME_IN`
| 필드 | 해석 |
|---|---|
| `char cName[NAME_MAX_LENGTH];` | 길이, 크기 또는 개수 값입니다. |

##### `EIP_REFBYNAME_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 23.4.2 EipWriteAdpTag

- PDF 페이지: 1664
- 원문 위치: [23.4.2 EipWriteAdpTag](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1664)
- 기능 설명: Eip 쓰기 Adp Tag 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipWriteAdpTag(
IN EIP_WRITEADPTAG_IN *pInParam,
OUT EIP_WRITEADPTAG_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_WRITEADPTAG_IN`
| 필드 | 해석 |
|---|---|
| `long dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char buffer[MAX_REQUEST_DATA_SIZE];` | 버퍼링/블렌딩 동작 모드입니다. |

##### `EIP_WRITEADPTAG_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |
| `int iReqid;` | i Reqid 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 23.4.3 EipReadAdpTag

- PDF 페이지: 1666
- 원문 위치: [23.4.3 EipReadAdpTag](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1666)
- 기능 설명: Eip 읽기 Adp Tag 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipReadAdpTag(
IN EIP_READADPTAG_IN *pInParam,
OUT EIP_READADPTAG_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_READADPTAG_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `EIP_READADPTAG_OUT`
| 필드 | 해석 |
|---|---|
| `long dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |
| `char buffer[MAX_REQUEST_DATA_SIZE];` | 버퍼링/블렌딩 동작 모드입니다. |

### 23.4.4 EipGetAssemblyRefByInstance

- PDF 페이지: 1668
- 원문 위치: [23.4.4 EipGetAssemblyRefByInstance](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1668)
- 기능 설명: Eip 조회 Assembly Ref By Instance 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipGetAssemblyRefByInstance(
IN EIP_REFBYINSTANCE_IN *pInParam,
OUT EIP_REFBYINSTANCE_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_REFBYINSTANCE_IN`
| 필드 | 해석 |
|---|---|
| `int iInstance;` | i Instance 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `EIP_REFBYINSTANCE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 23.4.5 EipGetAssemblyRefByName

- PDF 페이지: 1670
- 원문 위치: [23.4.5 EipGetAssemblyRefByName](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1670)
- 기능 설명: Eip 조회 Assembly Ref By Name 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipGetAssemblyRefByName(
IN EIP_REFBYNAME_IN *pInParam,
OUT EIP_REFBYNAME_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_REFBYNAME_IN`
| 필드 | 해석 |
|---|---|
| `char cName[NAME_MAX_LENGTH];` | 길이, 크기 또는 개수 값입니다. |

##### `EIP_REFBYNAME_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 23.4.6 EipSetAssembly

- PDF 페이지: 1672
- 원문 위치: [23.4.6 EipSetAssembly](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1672)
- 기능 설명: Eip 설정 Assembly 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipSetAssembly(
IN EIP_SETASSEMBLY_IN *pInParam,
OUT EIP_SETASSEMBLY_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_SETASSEMBLY_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char buffer[MAX_REQUEST_DATA_SIZE];` | 버퍼링/블렌딩 동작 모드입니다. |

##### `EIP_SETASSEMBLY_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `int iReqid;` | i Reqid 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 23.4.7 EipGetAssembly

- PDF 페이지: 1674
- 원문 위치: [23.4.7 EipGetAssembly](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1674)
- 기능 설명: Eip 조회 Assembly 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipGetAssembly(
IN EIP_GETASSEMBLY_IN *pInParam,
OUT EIP_GETASSEMBLY_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_GETASSEMBLY_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usInstance;` | us Instance 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `EIP_GETASSEMBLY_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `char buffer[MAX_REQUEST_DATA_SIZE];` | 버퍼링/블렌딩 동작 모드입니다. |

### 23.4.8 EipGetDevTagRefByName

- PDF 페이지: 1676
- 원문 위치: [23.4.8 EipGetDevTagRefByName](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1676)
- 기능 설명: Eip 조회 Dev Tag Ref By Name 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipGetDevTagRefByName(
IN EIP_REFBYNAME_IN *pInParam,
OUT EIP_REFBYNAME_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_REFBYNAME_IN`
| 필드 | 해석 |
|---|---|
| `char cName[NAME_MAX_LENGTH];` | 길이, 크기 또는 개수 값입니다. |

##### `EIP_REFBYNAME_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 23.4.9 EipSetDevTag

- PDF 페이지: 1678
- 원문 위치: [23.4.9 EipSetDevTag](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1678)
- 기능 설명: Eip 설정 Dev Tag 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipSetDevTag(
IN EIP_SETDEVTAG_IN *pInParam,
OUT EIP_SETDEVTAG_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_SETDEVTAG_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char buffer[MAX_REQUEST_DATA_SIZE];` | 버퍼링/블렌딩 동작 모드입니다. |

##### `EIP_SETDEVTAG_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `int iReqid;` | i Reqid 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 23.4.10 EipGetDevTag

- PDF 페이지: 1680
- 원문 위치: [23.4.10 EipGetDevTag](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1680)
- 기능 설명: Eip 조회 Dev Tag 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipGetDevTag(
IN EIP_GETDEVTAG_IN *pInParam,
OUT EIP_GETDEVTAG_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_GETDEVTAG_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `EIP_GETDEVTAG_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `int iReqid;` | i Reqid 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 23.4.11 EipReadDevTagData

- PDF 페이지: 1682
- 원문 위치: [23.4.11 EipReadDevTagData](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1682)
- 기능 설명: Eip 읽기 Dev Tag 데이터 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipReadDevTagData(
IN EIP_READDEVTAG_IN *pInParam,
OUT EIP_READDEVTAG_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_READDEVTAG_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `EIP_READDEVTAG_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `char buffer[MAX_REQUEST_DATA_SIZE];` | 버퍼링/블렌딩 동작 모드입니다. |

### 23.4.12 EipSyncGetDevTag

- PDF 페이지: 1684
- 원문 위치: [23.4.12 EipSyncGetDevTag](../chunks/066_p1647-p1685_23.1-Terminology.md#pdf-page-1684)
- 기능 설명: Eip 동기 조회 Dev Tag 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipSyncGetDevTag(
IN EIP_GETSYNC_IN *pInParam,
OUT EIP_GETSYNC_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_GETSYNC_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usTagRef;` | us Tag Ref 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `EIP_GETSYNC_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `char buffer[MAX_REQUEST_DATA_SIZE];` | 버퍼링/블렌딩 동작 모드입니다. |

### 23.4.13 EipCheckDevTagReply

- PDF 페이지: 1686
- 원문 위치: [23.4.13 EipCheckDevTagReply](../chunks/067_p1686-p1697_23.4.13-EipCheckDevTagReply.md#pdf-page-1686)
- 기능 설명: Eip 확인 Dev Tag Reply 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
void EipCheckDevTagReply(
IN EIP_CHECKREPLY_IN *pInParam,
OUT EIP_CHECKREPLY_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_CHECKREPLY_IN`
| 필드 | 해석 |
|---|---|
| `int iReqid;` | i Reqid 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `EIP_CHECKREPLY_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `short sReplyStatus;` | 명령 또는 장치 상태 값입니다. |

### 23.4.14 EipOpenSession

- PDF 페이지: 1688
- 원문 위치: [23.4.14 EipOpenSession](../chunks/067_p1686-p1697_23.4.13-EipCheckDevTagReply.md#pdf-page-1688)
- 기능 설명: Eip 열기 Session 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipOpenSession(
IN EIP_CALLBACK_FUNC pCallBackFunc,
IN EIP_OPEN_SESSION_IN *pInParam,
OUT EIP_OPEN_SESSION_OUT *pOutParam
);
```
```c
int EIPCallback(unsigned char* ucBuffer, short sReqID, void* pSock)
{
unsigned char ucEventID = ucBuffer[2];
switch (ucEventID)
{
case NM_REQUEST_RESPONSE_RECEIVED:
//printf("NM_REQUEST_RESPONSE_RECEIVED: sReqID = %d\n", sReqID);
```

#### 구조체/인자

##### `EIP_OPEN_SESSION_IN`
| 필드 | 해석 |
|---|---|
| `char cNotifyEvant;` | c Notify Evant 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `EIP_OPEN_SESSION_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 23.4.15 EIPCloseSession

- PDF 페이지: 1692
- 원문 위치: [23.4.15 EIPCloseSession](../chunks/067_p1686-p1697_23.4.13-EipCheckDevTagReply.md#pdf-page-1692)
- 기능 설명: EIPClose Session 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EIPCloseSession(
IN EIP_CLOSE_SESSION_IN *pInParam,
OUT EIP_CLOSE_SESSION_OUT *pOutParam
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 23.4.16 EipCreate

- PDF 페이지: 1694
- 원문 위치: [23.4.16 EipCreate](../chunks/067_p1686-p1697_23.4.13-EipCheckDevTagReply.md#pdf-page-1694)
- 기능 설명: Eip 생성 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipCreate(
IN EIP_CREATE_IN *pInParam,
OUT EIP_CREATE_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_CREATE_IN`
| 필드 | 해석 |
|---|---|
| `char cPath[80];` | 파일명, 경로, 이름 문자열입니다. |

##### `EIP_CREATE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 23.4.17 EipDestroy

- PDF 페이지: 1696
- 원문 위치: [23.4.17 EipDestroy](../chunks/067_p1686-p1697_23.4.13-EipCheckDevTagReply.md#pdf-page-1696)
- 기능 설명: Eip 삭제 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int EipDestroy(
IN EIP_DESTROY_IN *pInParam,
OUT EIP_DESTROY_OUT *pOutParam
);
```

#### 구조체/인자

##### `EIP_DESTROY_IN`
| 필드 | 해석 |
|---|---|
| `char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `EIP_DESTROY_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

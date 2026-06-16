# 10.2 Main Configuration Function Blocks - API 분석

- 원본 장: `Chapter 10 API Services and Operations`
- 시작 PDF 페이지: 959
- 원문 위치: [10.2 Main Configuration Function Blocks](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-959)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `10.2.1` | 960 | `MMC_ChangeToPreOPMode` | Change To Pre OPMode 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.2` | 962 | `MMC_ChangeToOperationMode` | Change To 동작 모드 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.3` | 964 | `MMC_ClearNodeFbListCmd` | 초기화 노드 Fb List 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.4` | 966 | `MMC_CmdStatus` | 값 또는 상태를 읽는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.5` | 969 | `MMC_CloseConnection` | 닫기 연결 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.6` | 970 | `MMC_Config` | 구성 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.7` | 972 | `MMC_CreateSYNCTimer` | 생성 SYNC 타이머 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.8` | 973 | `MMC_DestroySYNCTimer` | 삭제 SYNC 타이머 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.9` | 974 | `MMC_DownloadFoE` | 다운로드 Fo E 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.10` | 980 | `MMC_Exit` | Exit 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.11` | 982 | `MMC_FreeFbStatCmd` | Free Fb Stat 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - Not relevant |
| `10.2.12` | 985 | `MMC_GetActiveVectorsNum` | 조회 Active Vectors Num 값/상태를 조회하는 API입니다. | Motion Mode NC - Not Supported Distributed - Supported |
| `10.2.13` | 987 | `MMC_GetErrorCodeDescriptionByID` | 조회 오류 Code Description By ID 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.14` | 990 | `MMC_GetFoEStatus` | 조회 Fo EStatus 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.15` | 997 | `MMC_GetEnquireFbStatusCmd` | 조회 Enquire Fb 상태 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.16` | 999 | `MMC_GetAxisByNameCmd` | 조회 축 By Name 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.17` | 1001 | `MMC_GetGroupByNameCmd` | 조회 그룹 By Name 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.18` | 1004 | `MMC_GetGMASOperationMode` | 조회 GMASOperation 모드 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - Not relevant |
| `10.2.19` | 1007 | `MMC_GetStatusRegisterCmd` | 조회 상태 등록 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.20` | 1010 | `MMC_GetResListCmd` | 조회 Res List 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.21` | 1013 | `MMC_GetResSnapshotCmd` | 조회 Res Snapshot 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.22` | 1016 | `MMC_GetVersionCmd` | 조회 버전 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.23` | 1019 | `MMC_GetVersionExCmd` | 조회 버전 Ex 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.24` | 1022 | `MMC_GetLastError` | 조회 Last 오류 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.25` | 1023 | `MMC_InitConnection` | 초기화 연결 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.26` | 1025 | `MMC_RpcInitConnection` | Rpc 초기화 연결 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.27` | 1027 | `MMC_RpcInitConnectionEx` | Rpc 초기화 연결 Ex 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.28` | 1029 | `MMC_IPCInitConnection` | IPCInit 연결 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.29` | 1031 | `MMC_LoadParamCmd` | 로드 파라미터 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - Not relevant |
| `10.2.30` | 1033 | `MMC_ResetMultiAxisControl` | 리셋 Multi 축 Control 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.31` | 1036 | `MMC_ResExportFileCmd` | Res Export 파일 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.32` | 1039 | `MMC_ResImportFileCmd` | Res Import 파일 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.33` | 1042 | `MMC_SaveParamCmd` | 저장 파라미터 작업을 수행하는 API입니다. | Motion Mode NC - Not relevant Distributed - Not relevant |
| `10.2.34` | 1045 | `MMC_SetEnquireFbStatusCmd` | 설정 Enquire Fb 상태 값/설정을 적용하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.35` | 1047 | `MMC_SetDefaultParametersCmd` | 설정 Default Parameters 값/설정을 적용하는 API입니다. | Motion Mode NC - Not relevant Distributed - Not relevant |
| `10.2.36` | 1049 | `MMC_SetDefaultParametersGlobalCmd` | 설정 Default Parameters 전역 값/설정을 적용하는 API입니다. | Motion Mode NC - Not relevant Distributed - Not relevant |
| `10.2.37` | 1051 | `MMC_SetIsToLoadGlobalParamsCmd` | 설정 Is To 로드 전역 파라미터 값/설정을 적용하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |
| `10.2.38` | 1053 | `MMC_ShowNodeStatCmd` | Show 노드 Stat 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.39` | 1056 | `MMC_GetActiveAxesNum` | 조회 Active 축 Num 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.40` | 1058 | `MMC_ToggleConsoleOutputCmd` | Toggle Console 출력 작업을 수행하는 API입니다. | Motion Mode NC - Supported? Distributed - Supported? |
| `10.2.41` | 1060 | `MMC_GetCyclesCounterCmd` | 조회 Cycles Counter 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.42` | 1062 | `MMC_WriteGroupOfParameters` | 쓰기 그룹 Of Parameters 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.43` | 1066 | `MMC_WriteGroupOfParametersEx` | 쓰기 그룹 Of Parameters Ex 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.44` | 1071 | `MMC_ReadGroupOfParameters` | 읽기 그룹 Of Parameters 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.45` | 1074 | `MMC_WaitUntilConditionFB` | 대기 Until Condition FB 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.46` | 1077 | `MMC_WaitUntilConditionFBEx` | 대기 Until Condition FBEx 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.47` | 1081 | `MMC_WriteMemoryRange` | 쓰기 메모리 범위 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.48` | 1083 | `MMC_ReadMemoryRange` | 읽기 메모리 범위 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.49` | 1085 | `MMC_SetDefaultResources` | 설정 Default Resources 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.50` | 1087 | `MMC_UserCommandControl` | 사용자 명령 Control 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.51` | 1090 | `MMC_SetAllFbExeModeImm` | 설정 All Fb Exe 모드 Imm 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `10.2.52` | 1092 | `MMC_GetVerPath` | 조회 Ver 경로 값/상태를 조회하는 API입니다. | - |
| `10.2.53` | 1092 | `MMC_DownloadVersion` | 다운로드 버전 작업을 수행하는 API입니다. | - |
| `10.2.54` | 1092 | `MMC_ReadDownloadVersionStatus` | 읽기 다운로드 버전 상태 값/상태를 조회하는 API입니다. | - |
| `10.2.55` | 1092 | `MMC_SetVerPath` | 설정 Ver 경로 값/설정을 적용하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 10.2.1 MMC_ChangeToPreOPMode

- PDF 페이지: 960
- 원문 위치: [10.2.1 MMC_ChangeToPreOPMode](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-960)
- 기능 설명: Change To Pre OPMode 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_ChangeToPreOPMode(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SET_GMAS_PREOP_IN* pInParam
OUT MMC_SET_GMAS_PREOP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SET_GMAS_PREOP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SET_GMAS_PREOP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.2 MMC_ChangeToOperationMode

- PDF 페이지: 962
- 원문 위치: [10.2.2 MMC_ChangeToOperationMode](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-962)
- 기능 설명: Change To 동작 모드 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_ChangeToOperationMode(
IN MMC_CONNECT_HNDL hConn,
OUT MMC_SET_GMAS_OP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SET_GMAS_OP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SET_GMAS_OP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 10.2.3 MMC_ClearNodeFbList

- PDF 페이지: 964
- 원문 위치: [10.2.3 MMC_ClearNodeFbList](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-964)
- 기능 설명: 초기화 노드 Fb List 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
int MMC_ClearNodeFbListCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_CLEARFBLIST_IN* pInParam,
OUT MMC_CLEARFBLIST_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CLEARFBLIST_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usAxisRef;` | 축 식별 또는 축 관련 값입니다. |

##### `MMC_CLEARFBLIST_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.4 MMC_CmdStatus

- PDF 페이지: 966
- 원문 위치: [10.2.4 MMC_CmdStatus](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-966)
- 기능 설명: 값 또는 상태를 읽는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_CmdStatus(
IN MMC_CONNECT_HNDL hConn,
IN MMC_FBSTATUS_IN* pInParam,
OUT MMC_FBSTATUS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_FBSTATUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |

##### `MMC_FBSTATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiFbStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |
| `unsigned short usFbErrorID;` | 오류 ID입니다. |

### 10.2.5 MMC_CloseConnection

- PDF 페이지: 969
- 원문 위치: [10.2.5 MMC_CloseConnection](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-969)
- 기능 설명: 닫기 연결 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_CloseConnection(
IN MMC_CONNECT_HNDL hConn
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.6 MMC_Config

- PDF 페이지: 970
- 원문 위치: [10.2.6 MMC_Config](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-970)
- 기능 설명: 구성 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

##### `MMC_CONFIG_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CONFIG_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.7 MMC_CreateSYNCTimer

- PDF 페이지: 972
- 원문 위치: [10.2.7 MMC_CreateSYNCTimer](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-972)
- 기능 설명: 생성 SYNC 타이머 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_CreateSYNCTimer(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SYNC_TIMER_CB_FUNC func,
IN unsigned short usSYNCTimerTime
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.8 MMC_DestroySYNCTimer

- PDF 페이지: 973
- 원문 위치: [10.2.8 MMC_DestroySYNCTimer](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-973)
- 기능 설명: 삭제 SYNC 타이머 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_DestroySYNCTimer(
IN MMC_CONNECT_HNDL hConn
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.9 MMC_DownloadFoE

- PDF 페이지: 974
- 원문 위치: [10.2.9 MMC_DownloadFoE](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-974)
- 기능 설명: 다운로드 Fo E 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_DownloadFoE(
IN MMC_CONNECT_HNDL hConn,
IN MMC_DOWNLOADFOE_IN* pInParam,
OUT MMC_DOWNLOADFOE_OUT* pOutParam);
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
}
```

#### 구조체/인자

##### `MMC_DOWNLOADFOE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short pwSlaveId[NC_NODES_SING_AXIS_NUM];` | 노드 식별 또는 노드 관련 값입니다. |
| `char pcFileName[256];` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char pucServer[4];` | puc Server[4] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucSlavesNum;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_DOWNLOADFOE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 10.2.10 MMC_Exit

- PDF 페이지: 980
- 원문 위치: [10.2.10 MMC_Exit](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-980)
- 기능 설명: Exit 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

##### `MMC_EXIT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_EXIT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.11 MMC_FreeFbStat

- PDF 페이지: 982
- 원문 위치: [10.2.11 MMC_FreeFbStat](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-982)
- 기능 설명: Free Fb Stat 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - Not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_FreeFbStatCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_FREEFBSTAT_IN* pInParam,
OUT MMC_FREEFBSTAT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_FREEFBSTAT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |

##### `MMC_FREEFBSTAT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiFreeLargeFb;` | ui Free Large Fb 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned int uiFreeMediumFb;` | ui Free Medium Fb 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned int uiFreeSmallFb;` | ui Free Small Fb 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 10.2.12 MMC_GetActiveVectorsNum

- PDF 페이지: 985
- 원문 위치: [10.2.12 MMC_GetActiveVectorsNum](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-985)
- 기능 설명: 조회 Active Vectors Num 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetActiveVectorsNum(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETACTIVEVECTORSNUM_IN* pInParam,
OUT MMC_GETACTIVEVECTORSNUM_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETACTIVEVECTORSNUM_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETACTIVEVECTORSNUM_OUT`
| 필드 | 해석 |
|---|---|
| `int iActiveVectorsNum;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.13 MMC_GetErrorCodeDescriptionByID

- PDF 페이지: 987
- 원문 위치: [10.2.13 MMC_GetErrorCodeDescriptionByID](../chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-987)
- 기능 설명: 조회 오류 Code Description By ID 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetErrorCodeDescriptionByID(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETERRORCODEDESCRIPTIONBYID_IN* pInParam,
OUT MMC_GETERRORCODEDESCRIPTIONBYID_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETERRORCODEDESCRIPTIONBYID_IN`
| 필드 | 해석 |
|---|---|
| `int iCode;` | i Code 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `Char cType;` | 데이터 또는 동작 타입 값입니다. |

##### `MMC_GETERRORCODEDESCRIPTIONBYID_OUT`
| 필드 | 해석 |
|---|---|
| `char pResolution[1100];` | p Resolution[1100] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char pDescription[256];` | 주소 또는 IP 관련 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.14 MMC_GetFoEStatus

- PDF 페이지: 990
- 원문 위치: [10.2.14 MMC_GetFoEStatus](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-990)
- 기능 설명: 조회 Fo EStatus 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetFoEStatus(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETFOESTATUS_IN* pInParam,
OUT MMC_GETFOESTATUS_OUT* pOutParam
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
}
```

#### 구조체/인자

##### `MMC_GETFOESTATUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETFOESTATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |
| `short sFOEStatus;` | 명령 또는 장치 상태 값입니다. |
| `FOE_SLAVE_INFO pstSlavesErrorID[NC_NODES_SING_AXIS_NUM];` | 오류 ID입니다. |
| `unsigned char ucNumOfSlaves;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char ucProgress ;` | unsigned char uc Progress 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucFOEStarted ;` | unsigned char uc FOEStarted 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 10.2.15 MMC_GetEnquireFbStatus

- PDF 페이지: 997
- 원문 위치: [10.2.15 MMC_GetEnquireFbStatus](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-997)
- 기능 설명: 조회 Enquire Fb 상태 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetEnquireFbStatusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETENQUIREFBSTATUS_IN* pInParam,
OUT MMC_GETENQUIREFBSTATUS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETENQUIREFBSTATUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETENQUIREFBSTATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned char ucCurrentStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.16 MMC_GetAxisByName

- PDF 페이지: 999
- 원문 위치: [10.2.16 MMC_GetAxisByName](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-999)
- 기능 설명: 조회 축 By Name 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetAxisByNameCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXISBYNAME_IN* pInParam,
OUT MMC_AXISBYNAME_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_AXISBYNAME_IN`
| 필드 | 해석 |
|---|---|
| `char cAxisName[NODE_NAME_MAX_LENGTH];` | 노드 식별 또는 노드 관련 값입니다. |

##### `MMC_AXISBYNAME_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |
| `unsigned short usAxisIdx;` | 인덱스 값입니다. |

### 10.2.17 MMC_GetGroupByName

- PDF 페이지: 1001
- 원문 위치: [10.2.17 MMC_GetGroupByName](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1001)
- 기능 설명: 조회 그룹 By Name 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetGroupByNameCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXISBYNAME_IN* pInParam,
OUT MMC_AXISBYNAME_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_AXISBYNAME_IN`
| 필드 | 해석 |
|---|---|
| `char cAxisName[NODE_NAME_MAX_LENGTH];` | 노드 식별 또는 노드 관련 값입니다. |

##### `MMC_AXISBYNAME_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usAxisIdx;` | 인덱스 값입니다. |

### 10.2.18 MMC_GetGMASOperationMode

- PDF 페이지: 1004
- 원문 위치: [10.2.18 MMC_GetGMASOperationMode](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1004)
- 기능 설명: 조회 GMASOperation 모드 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - Not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetGMASOperationMode(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GET_GMASOP_MODE_IN* pInParam
OUT MMC_GET_GMASOP_MODE_OUT* pOutParam
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.19 MMC_GetStatusRegister

- PDF 페이지: 1007
- 원문 위치: [10.2.19 MMC_GetStatusRegister](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1007)
- 기능 설명: 조회 상태 등록 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
int MMC_GetStatusRegisterCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETSTATUSREGISTER_IN* pInParam,
OUT MMC_GETSTATUSREGISTER_OUT* pOutParam
);
```
```c
MMC_GetStatusRegisterCmd(ui_conn_hndl,AxisA.GetRef(),&StatusIN,&StatusOUT);
if(iRetval != NC_OK)
// Error handling
}
else
{
// Status Register
StatusOUT.uiStatusRegister;
// If the axis in SW Low limit the value will be 4 in decimal and 100 in
binary
// MCS limit register relevant for group represents the status of MCS
limits
StatusOUT.uiMcsLimitRegister;
// Status
StatusOUT.usStatus;
// Error ID
StatusOUT.sErrorID;
// Future use
StatusOUT.cBuffer[32];
}
```

#### 구조체/인자

##### `MMC_GETSTATUSREGISTER_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETSTATUSREGISTER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiStatusRegister;` | 명령 또는 장치 상태 값입니다. |
| `unsigned int uiMcsLimitRegister;` | ui Mcs Limit 등록 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char cBuffer[32];` | 버퍼링/블렌딩 동작 모드입니다. |

### 10.2.20 MMC_GetResList

- PDF 페이지: 1010
- 원문 위치: [10.2.20 MMC_GetResList](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1010)
- 기능 설명: 조회 Res List 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetResListCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GET_RESLIST_IN* pInParam,
OUT MMC_GET_RESLIST_OUT* pOutParam);
```

#### 구조체/인자

##### `MMC_GET_RESLIST_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GET_RESLIST_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `char pResList[1024];` | p Res List[1024] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 10.2.21 MMC_GetResSnapshot

- PDF 페이지: 1013
- 원문 위치: [10.2.21 MMC_GetResSnapshot](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1013)
- 기능 설명: 조회 Res Snapshot 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetResSnapshotCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_RESSNAPSHOT_IN* pInParam,
OUT MMC_RESSNAPSHOT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_RESSNAPSHOT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_RESSNAPSHOT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.22 MMC_GetVersion

- PDF 페이지: 1016
- 원문 위치: [10.2.22 MMC_GetVersion](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1016)
- 기능 설명: 조회 버전 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetVersionCmd(
IN MMC_CONNECT_HNDL hConn,
OUT MMC_GET_VER_OUT* sVersion
);
```

#### 구조체/인자

##### `MMC_GET_VER_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GET_VER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiUbootVer;` | ui Uboot Ver 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `char cFirst;` | c First 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cSecond;` | c Second 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cThird;` | c Third 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cFourth;` | c Fourth 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 10.2.23 MMC_GetVersionEx

- PDF 페이지: 1019
- 원문 위치: [10.2.23 MMC_GetVersionEx](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1019)
- 기능 설명: 조회 버전 Ex 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetVersionExCmd(
IN MMC_CONNECT_HNDL hConn,
OUT MMC_GET_VEREX_OUT* sVersion
);
```
```c
rc = MMC_GetVersionExCmd(ConnHndl,&GetVerOut);
if(rc != 0)
{
printf("MMC_GetVersionExCmd failed, error %d", GetVerOut.sErrorID);
```

#### 구조체/인자

##### `MMC_GET_VEREX_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GET_VEREX_OUT`
| 필드 | 해석 |
|---|---|
| `char pcData[MAX_GETVERSION_CHARS];` | 데이터 버퍼 또는 데이터 값입니다. |

### 10.2.24 MMC_GetLastError

- PDF 페이지: 1022
- 원문 위치: [10.2.24 MMC_GetLastError](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1022)
- 기능 설명: 조회 Last 오류 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API void MMC_GetLastError (
IN MMC_CONNECT_HNDL hConn,
OUT char* chStr,
IN int iSize
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.25 MMC_InitConnection

- PDF 페이지: 1023
- 원문 위치: [10.2.25 MMC_InitConnection](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1023)
- 기능 설명: 초기화 연결 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_InitConnection(
IN MMC_CONNECTION_TYPE eType,
IN MMC_CONNECTION_PARAM_STRUCT sConnParam,
IN MMC_CB_FUNC pCbFunc,
OUT MMC_CONNECT_HNDL* pHndl
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.26 MMC_RpcInitConnection

- PDF 페이지: 1025
- 원문 위치: [10.2.26 MMC_RpcInitConnection](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1025)
- 기능 설명: Rpc 초기화 연결 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_RpcInitConnection(
IN MMC_CONNECTION_TYPE eType,
IN MMC_CONNECTION_PARAM_STRUCT sConnParam,
IN MMC_CB_FUNC pCbFunc ,
IN char* cpHostIPAddr,
OUT MMC_CONNECT_HNDL* pHndl
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.27 MMC_RpcInitConnectionEx

- PDF 페이지: 1027
- 원문 위치: [10.2.27 MMC_RpcInitConnectionEx](../chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1027)
- 기능 설명: Rpc 초기화 연결 Ex 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_RpcInitConnectionEx(
IN MMC_CONNECTION_TYPE eType,
IN MMC_CONNECTION_PARAM_STRUCT sConnParam,
IN MMC_MB_CLBK pCbFunc,
IN char* cpHostIPAddr,
OUT MMC_CONNECT_HNDL* pHndl
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.28 MMC_IPCInitConnection

- PDF 페이지: 1029
- 원문 위치: [10.2.28 MMC_IPCInitConnection](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1029)
- 기능 설명: IPCInit 연결 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_IPCInitConnection(
IN MMC_IPC_CONNECTION_PARAM_STRUCT sConnParam,
IN MMC_CB_FUNC pCbFunc ,
OUT MMC_CONNECT_HNDL* pHndl
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.29 MMC_LoadParam

- PDF 페이지: 1031
- 원문 위치: [10.2.29 MMC_LoadParam](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1031)
- 기능 설명: 로드 파라미터 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - Not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_LoadParamCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_LOADPARAM_IN* pInParam,
OUT MMC_LOADPARAM_OUT* pOutParam
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.30 MMC_ResetMultiAxisControl

- PDF 페이지: 1033
- 원문 위치: [10.2.30 MMC_ResetMultiAxisControl](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1033)
- 기능 설명: 리셋 Multi 축 Control 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_ResetMultiAxisControl(
IN MMC_CONNECT_HNDL hConn,
IN MMC_EXIT_APP_IN *pInParam,
OUT MMC_EXIT_APP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_EXIT_APP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_EXIT_APP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.31 MMC_ResExportFile

- PDF 페이지: 1036
- 원문 위치: [10.2.31 MMC_ResExportFile](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1036)
- 기능 설명: Res Export 파일 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_ResExportFileCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_RESEXPORTFILE_IN* pInParam,
OUT MMC_RESEXPORTFILE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_RESEXPORTFILE_OUT`
| 필드 | 해석 |
|---|---|
| `char pFName[33];` | 파일명, 경로, 이름 문자열입니다. |
| `char pServer[17];` | p Server[17] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char pFilePath[101];` | 파일명, 경로, 이름 문자열입니다. |
| `char ucDownloadType;` | 데이터 또는 동작 타입 값입니다. |
| `}MMC_RESEXPORTFILE_IN;` | 파일명, 경로, 이름 문자열입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

##### `MMC_RESEXPORTFILE_IN`
| 필드 | 해석 |
|---|---|
| `char pFName[33];` | 파일명, 경로, 이름 문자열입니다. |
| `char pServer[17];` | p Server[17] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char pFilePath[101];` | 파일명, 경로, 이름 문자열입니다. |
| `char ucDownloadType;` | 데이터 또는 동작 타입 값입니다. |

##### `MMC_RESEXPORTFILE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.32 MMC_ResImportFile

- PDF 페이지: 1039
- 원문 위치: [10.2.32 MMC_ResImportFile](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1039)
- 기능 설명: Res Import 파일 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MC_LIB_API int MMC_ResImportFileCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_RESIMPORTFILE_IN* pInParam,
OUT MMC_RESIMPORTFILE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_RESIMPORTFILE_IN`
| 필드 | 해석 |
|---|---|
| `char pFName[33];` | 파일명, 경로, 이름 문자열입니다. |
| `char pServer[17];` | p Server[17] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char ucDownloadType;` | 데이터 또는 동작 타입 값입니다. |

##### `MMC_RESIMPORTFILE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.33 MMC_SaveParam

- PDF 페이지: 1042
- 원문 위치: [10.2.33 MMC_SaveParam](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1042)
- 기능 설명: 저장 파라미터 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - Not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_SaveParamCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SAVEPARAM_IN* pInParam,
OUT MMC_SAVEPARAM_OUT* pOutParam
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.34 MMC_SetEnquireFbStatus

- PDF 페이지: 1045
- 원문 위치: [10.2.34 MMC_SetEnquireFbStatus](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1045)
- 기능 설명: 설정 Enquire Fb 상태 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_SetEnquireFbStatusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SETENQUIREFBSTATUS_IN* pInParam,
OUT MMC_SETENQUIREFBSTATUS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETENQUIREFBSTATUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucStatus;` | 명령 또는 장치 상태 값입니다. |

##### `MMC_SETENQUIREFBSTATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.35 MMC_SetDefaultParameters

- PDF 페이지: 1047
- 원문 위치: [10.2.35 MMC_SetDefaultParameters](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1047)
- 기능 설명: 설정 Default Parameters 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - Not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_SetDefaultParametersCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SETDEFAULTPARAMETERS_IN* pInParam,
OUT MMC_SETDEFAULTPARAMETERS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETDEFAULTPARAMETERS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SETDEFAULTPARAMETERS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.36 MMC_SetDefaultParametersGlobal

- PDF 페이지: 1049
- 원문 위치: [10.2.36 MMC_SetDefaultParametersGlobal](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1049)
- 기능 설명: 설정 Default Parameters 전역 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - Not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_SetDefaultParametersGlobalCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SETDEFAULTPARAMETERSGLOBAL_IN* pInParam,
OUT MMC_SETDEFAULTPARAMETERSGLOBAL_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETDEFAULTPARAMETERSGLOBAL_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_SETDEFAULTPARAMETERSGLOBAL_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.37 MMC_SetIsToLoadGlobalParams

- PDF 페이지: 1051
- 원문 위치: [10.2.37 MMC_SetIsToLoadGlobalParams](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1051)
- 기능 설명: 설정 Is To 로드 전역 파라미터 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_SetIsToLoadGlobalParamsCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SETISTOLOADGLOBALPARAMS_IN* pInParam,
OUT MMC_SETISTOLOADGLOBALPARAMS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETISTOLOADGLOBALPARAMS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucValue;` | 전달하거나 반환받는 값입니다. |

##### `MMC_SETISTOLOADGLOBALPARAMS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.38 MMC_ShowNodeStat

- PDF 페이지: 1053
- 원문 위치: [10.2.38 MMC_ShowNodeStat](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1053)
- 기능 설명: Show 노드 Stat 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ShowNodeStatCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SHOWNODESTAT_IN* pInParam,
OUT MMC_SHOWNODESTAT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SHOWNODESTAT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |

##### `MMC_SHOWNODESTAT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.39 MMC_GetActiveAxesNum

- PDF 페이지: 1056
- 원문 위치: [10.2.39 MMC_GetActiveAxesNum](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1056)
- 기능 설명: 조회 Active 축 Num 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetActiveAxesNum(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETACTIVEAXESNUM_IN* pInParam,
OUT MMC_GETACTIVEAXESNUM_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETACTIVEAXESNUM_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETACTIVEAXESNUM_OUT`
| 필드 | 해석 |
|---|---|
| `int iActiveAxesNum;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.40 MMC_ToggleConsoleOutput

- PDF 페이지: 1058
- 원문 위치: [10.2.40 MMC_ToggleConsoleOutput](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1058)
- 기능 설명: Toggle Console 출력 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported? Distributed - Supported?

#### 시그니처

```c
MMC_LIB_API int MMC_ToggleConsoleOutputCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_TOGGLECONSOLEOUTPUT_IN* pInParam,
OUT MMC_TOGGLECONSOLEOUTPUT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_TOGGLECONSOLEOUTPUT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_TOGGLECONSOLEOUTPUT_OUT`
| 필드 | 해석 |
|---|---|
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.41 MMC_GetCyclesCounter

- PDF 페이지: 1060
- 원문 위치: [10.2.41 MMC_GetCyclesCounter](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1060)
- 기능 설명: 조회 Cycles Counter 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetCyclesCounterCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETCYCLESCOUNTER_IN* pInParam,
OUT MMC_GETCYCLESCOUNTER_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETCYCLESCOUNTER_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETCYCLESCOUNTER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long ulCyclesCounter;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.42 MMC_WriteGroupOfParameters

- PDF 페이지: 1062
- 원문 위치: [10.2.42 MMC_WriteGroupOfParameters](../chunks/040_p1029-p1065_10.2.28-MMC_IPCInitConnection.md#pdf-page-1062)
- 기능 설명: 쓰기 그룹 Of Parameters 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WriteGroupOfParameters(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEGROUPOFPARAMETERS_IN* pInParam,
OUT MMC_WRITEGROUPOFPARAMETERS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEGROUPOFPARAMETERS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `MMC_WRITEGROUPOFPARAMETERSMEMBER sParameters[GROUP_OF_PARAMETERS_MAXIMUM_SIZE];` | 그룹 식별 또는 그룹 관련 값입니다. |
| `MC_EXECUTION_MODE eExecutionMode;` | 동작 모드 값입니다. |
| `unsigned char ucNumberOfParameters;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned char ucMode;` | 동작 모드 값입니다. |

##### `MMC_WRITEGROUPOFPARAMETERS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucProblematicEntry;` | uc Problematic Entry 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 10.2.43 MMC_WriteGroupOfParametersEx

- PDF 페이지: 1066
- 원문 위치: [10.2.43 MMC_WriteGroupOfParametersEx](../chunks/041_p1066-p1091_10.2.43-MMC_WriteGroupOfParametersEx.md#pdf-page-1066)
- 기능 설명: 쓰기 그룹 Of Parameters Ex 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WriteGroupOfParametersEX(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEGROUPOFPARAMETERSEX_IN* pInParam,
OUT MMC_WRITEGROUPOFPARAMETERSEX_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEGROUPOFPARAMETERSEX_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `MMC_WRITEGROUPOFPARAMETERSMEMBEREX sParameters[GROUP_OF_PARAMETERS_MAXIMUM_SIZE];` | 그룹 식별 또는 그룹 관련 값입니다. |
| `MC_EXECUTION_MODE eExecutionMode;` | 동작 모드 값입니다. |
| `unsigned char ucNumberOfParameters;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned char ucMode;` | 동작 모드 값입니다. |

##### `MMC_WRITEGROUPOFPARAMETERSEX_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucProblematicEntry;` | uc Problematic Entry 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 10.2.44 MMC_ReadGroupOfParameters

- PDF 페이지: 1071
- 원문 위치: [10.2.44 MMC_ReadGroupOfParameters](../chunks/041_p1066-p1091_10.2.43-MMC_WriteGroupOfParametersEx.md#pdf-page-1071)
- 기능 설명: 읽기 그룹 Of Parameters 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadGroupOfParameters(
IN MMC_CONNECT_HNDL hConn,
IN MMC_READGROUPOFPARAMETERS_IN* pInParam,
OUT MMC_READGROUPOFPARAMETERS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READGROUPOFPARAMETERS_IN`
| 필드 | 해석 |
|---|---|
| `MMC_READGROUPOFPARAMETERSMEMBER sParameters[GROUP_OF_PARAMETERS_MAXIMUM_SIZE];` | 그룹 식별 또는 그룹 관련 값입니다. |
| `unsigned char ucNumberOfParameters;` | 파라미터 식별자 또는 파라미터 값입니다. |

##### `MMC_READGROUPOFPARAMETERS_OUT`
| 필드 | 해석 |
|---|---|
| `double dbValue[GROUP_OF_PARAMETERS_MAXIMUM_SIZE];` | 그룹 식별 또는 그룹 관련 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucProblematicEntry;` | uc Problematic Entry 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 10.2.45 MMC_WaitUntilConditionFB

- PDF 페이지: 1074
- 원문 위치: [10.2.45 MMC_WaitUntilConditionFB](../chunks/041_p1066-p1091_10.2.43-MMC_WriteGroupOfParametersEx.md#pdf-page-1074)
- 기능 설명: 대기 Until Condition FB 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WaitUntilConditionFB(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WAITUNTILCONDITIONFB_IN* pInParam,
OUT MMC_WAITUNTILCONDITIONFB_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WAITUNTILCONDITIONFB_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbReferenceValue;` | 전달하거나 반환받는 값입니다. |
| `int iParameterID;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `int iParameterIndex;` | 인덱스 값입니다. |
| `MC_CONDITIONFB_OPERATION_TYPE eOperationType;` | 데이터 또는 동작 타입 값입니다. |
| `unsigned long ulSpare;` | ul Spare 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usSourceAxisReference;` | 축 식별 또는 축 관련 값입니다. |
| `unsigned char ucPadding;` | uc Padding 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucSpare[20];` | uc Spare[20] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_WAITUNTILCONDITIONFB_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 10.2.46 MMC_WaitUntilConditionFBEx

- PDF 페이지: 1077
- 원문 위치: [10.2.46 MMC_WaitUntilConditionFBEx](../chunks/041_p1066-p1091_10.2.43-MMC_WriteGroupOfParametersEx.md#pdf-page-1077)
- 기능 설명: 대기 Until Condition FBEx 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WaitUntilConditionFBEx(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WAITUNTILCONDITIONFBEx_IN* pInParam,
OUT MMC_WAITUNTILCONDITIONFBEx_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WAITUNTILCONDITIONFBEx_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 10.2.47 MMC_WriteMemoryRange

- PDF 페이지: 1081
- 원문 위치: [10.2.47 MMC_WriteMemoryRange](../chunks/041_p1066-p1091_10.2.43-MMC_WriteGroupOfParametersEx.md#pdf-page-1081)
- 기능 설명: 쓰기 메모리 범위 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WriteMemoryRange(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEMEMORYRANGE_IN* pInParam,
OUT MMC_WRITEMEMORYRANGE_OUT.* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEMEMORYRANGE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usRegAddr;` | 주소 또는 IP 관련 값입니다. |
| `unsigned char ucLength;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char pData [ETHERCAT_MEMORY_WRITE_MAX_SIZE];` | 데이터 버퍼 또는 데이터 값입니다. |

##### `MMC_WRITEMEMORYRANGE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.48 MMC_ReadMemoryRange

- PDF 페이지: 1083
- 원문 위치: [10.2.48 MMC_ReadMemoryRange](../chunks/041_p1066-p1091_10.2.43-MMC_WriteGroupOfParametersEx.md#pdf-page-1083)
- 기능 설명: 읽기 메모리 범위 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadMemoryRange(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READMEMORYRANGE_IN* pInParam,
OUT MMC_READMEMORYRANGE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READMEMORYRANGE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usRegAddr;` | 주소 또는 IP 관련 값입니다. |
| `unsigned char ucLength;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_READMEMORYRANGE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char pData[ETHERCAT_MEMORY_READ_MAX_SIZE];` | 데이터 버퍼 또는 데이터 값입니다. |

### 10.2.49 MMC_SetDefaultResources

- PDF 페이지: 1085
- 원문 위치: [10.2.49 MMC_SetDefaultResources](../chunks/041_p1066-p1091_10.2.43-MMC_WriteGroupOfParametersEx.md#pdf-page-1085)
- 기능 설명: 설정 Default Resources 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetDefaultResources(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SETDEFAULTRESOURCES_IN* pInParam,
OUT MMC_SETDEFAULTRESOURCES_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETDEFAULTRESOURCES_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucConnectionType;` | 데이터 또는 동작 타입 값입니다. |

##### `MMC_SETDEFAULTRESOURCES_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.50 MMC_UserCommandControl

- PDF 페이지: 1087
- 원문 위치: [10.2.50 MMC_UserCommandControl](../chunks/041_p1066-p1091_10.2.43-MMC_WriteGroupOfParametersEx.md#pdf-page-1087)
- 기능 설명: 사용자 명령 Control 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_UserCommandControl(
IN MMC_CONNECT_HNDL hConn,
IN MMC_USRCOMMAND_IN* pInParam,
OUT MMC_USRCOMMAND_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_USRCOMMAND_IN`
| 필드 | 해석 |
|---|---|
| `MC_COMMAND_OPERATION eUsrCommandOp;` | e Usr 명령 Op 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cUserCommand[256];` | c 사용자 Command[256] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucSpare[20];` | uc Spare[20] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_USRCOMMAND_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucIsRunning;` | uc Is Running 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucIsExist;` | uc Is Exist 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cExecutableFileName[64];` | 길이, 크기 또는 개수 값입니다. |
| `char cSpear[448];` | c Spear[448] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 10.2.51 MMC_SetAllFbExeModeImm

- PDF 페이지: 1090
- 원문 위치: [10.2.51 MMC_SetAllFbExeModeImm](../chunks/041_p1066-p1091_10.2.43-MMC_WriteGroupOfParametersEx.md#pdf-page-1090)
- 기능 설명: 설정 All Fb Exe 모드 Imm 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetAllFbExeModeImm(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SETALLFBEXEMODETOIMM_IN* pInParam,
OUT MMC_SETALLFBEXEMODETOIMM_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETALLFBEXEMODETOIMM_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usAxisRef;` | 축 식별 또는 축 관련 값입니다. |

##### `MMC_SETALLFBEXEMODETOIMM_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10.2.52 MMC_GetVerPath

- PDF 페이지: 1092
- 원문 위치: [10.2.52 MMC_GetVerPath](../chunks/042_p1092-p1092_10.2.55-MMC_SetVerPath.md#pdf-page-1092)
- 기능 설명: 조회 Ver 경로 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.53 MMC_DownloadVersion

- PDF 페이지: 1092
- 원문 위치: [10.2.53 MMC_DownloadVersion](../chunks/042_p1092-p1092_10.2.55-MMC_SetVerPath.md#pdf-page-1092)
- 기능 설명: 다운로드 버전 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.54 MMC_ReadDownloadVersionStatus

- PDF 페이지: 1092
- 원문 위치: [10.2.54 MMC_ReadDownloadVersionStatus](../chunks/042_p1092-p1092_10.2.55-MMC_SetVerPath.md#pdf-page-1092)
- 기능 설명: 읽기 다운로드 버전 상태 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 10.2.55 MMC_SetVerPath

- PDF 페이지: 1092
- 원문 위치: [10.2.55 MMC_SetVerPath](../chunks/042_p1092-p1092_10.2.55-MMC_SetVerPath.md#pdf-page-1092)
- 기능 설명: 설정 Ver 경로 값/설정을 적용하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

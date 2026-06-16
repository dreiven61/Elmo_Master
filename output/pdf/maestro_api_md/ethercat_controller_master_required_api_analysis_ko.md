# EtherCAT Controller(Master) 요구 API 분석

- 기준 엑셀: `EtherCAT Controller(Master) 요구 사양.xlsx`
- 기준 시트: `ELMO Controller API`
- 참조 자료: `api_parts_ko` 및 원문 chunk
- API 개수: 44

## 요약

이 문서는 엑셀에 적힌 ELMO/Maestro API 목록을 생성된 API 분석 문서와 대조해서 정리한 단일 보고서입니다. 함수명, 구조체명, 필드명은 원문 식별자를 유지했고 설명은 한국어로 작성했습니다.

| No | Excel Row | API | 매칭 상태 | 분석 파트 | 기능 요약 |
|---:|---:|---|---|---|---|
| 1 | 2 | `MMC_RpcInitConnection` | 분석 문서 매칭 | [10.2.26 MMC_RpcInitConnection](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md) | Rpc 초기화 연결 작업을 수행하는 API입니다. |
| 2 | 3 | `MMC_OpenUdpChannelCmdEx` | 기본형/예제 보조 매칭 | [17.1.9 MMC_OpenUdpChannel](api_parts_ko/ch17_17_1_Network-Function-Blocks.md) | UDP 채널을 여는 API입니다. 본문 정의는 `MMC_OpenUdpChannelCmd`이고, `CmdEx`는 C++ 예제에서 확인됩니다. |
| 3 | 4 | `MMC_GetAxisByNameCmd` | 분석 문서 매칭 | [10.2.16 MMC_GetAxisByName](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md) | 조회 축 By Name 값/상태를 조회하는 API입니다. |
| 4 | 5 | `MMC_GetGroupByNameCmd` | 분석 문서 매칭 | [10.2.17 MMC_GetGroupByName](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md) | 조회 그룹 By Name 값/상태를 조회하는 API입니다. |
| 5 | 6 | `MMC_GetErrorCodeDescriptionByID` | 분석 문서 매칭 | [10.2.13 MMC_GetErrorCodeDescriptionByID](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md) | 조회 오류 Code Description By ID 값/상태를 조회하는 API입니다. |
| 6 | 7 | `MMC_PowerCmd` | 분석 문서 매칭 | [6.2.11 MMC_Power](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md) | 전원 작업을 수행하는 API입니다. |
| 7 | 8 | `MMC_GroupReadStatusCmd` | 분석 문서 매칭 | [7.10.30 MMC_GroupReadStatus](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md) | 그룹 읽기 상태 값/상태를 조회하는 API입니다. |
| 8 | 9 | `MMC_GroupEnableCmd` | 분석 문서 매칭 | [7.10.26 MMC_GroupEnable](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md) | 그룹 활성화 활성화/비활성화 제어를 수행하는 API입니다. |
| 9 | 10 | `MMC_GroupDisableCmd` | 분석 문서 매칭 | [7.10.25 MMC_GroupDisable](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md) | 그룹 비활성화 활성화/비활성화 제어를 수행하는 API입니다. |
| 10 | 11 | `MMC_ConfigBulkReadCmd` | 분석 문서 매칭 | [13.1.1 MMC_ConfigBulkRead](api_parts_ko/ch13_13_1_Bulk-Reading-Functions.md) | 구성 벌크 읽기 값/상태를 조회하는 API입니다. |
| 11 | 12 | `MMC_PerformBulkReadCmd` | 분석 문서 매칭 | [13.1.2 MMC_PerformBulkRead](api_parts_ko/ch13_13_1_Bulk-Reading-Functions.md) | Perform 벌크 읽기 값/상태를 조회하는 API입니다. |
| 12 | 13 | `MMC_Reset` | 분석 문서 매칭 | [6.2.25 MMC_Reset](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md) | 리셋 작업을 수행하는 API입니다. |
| 13 | 14 | `MMC_GroupResetCmd` | 분석 문서 매칭 | [7.10.31 MMC_GroupReset](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md) | 그룹 리셋 작업을 수행하는 API입니다. |
| 14 | 15 | `MMC_SendSdoCmd` | 분석 문서 매칭 | [19.9.27 MMC_SendSDO](api_parts_ko/ch19_19_9_CANbus-Function-Blocks.md) | 전송 Sdo 작업을 수행하는 API입니다. |
| 15 | 16 | `MMC_ReadParameter` | 분석 문서 매칭 | [6.2.22 MMC_ReadParameter](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md) | 읽기 파라미터 값/상태를 조회하는 API입니다. |
| 16 | 17 | `MMC_MoveRelativeExCmd` | C++ wrapper 보조 매칭 | [24.3.16 MoveRelativeEx](api_parts_ko/ch24_24_3_The-MMCSingleAxis-class.md) | C++ `MoveRelativeEx`는 상대 거리 이동 wrapper입니다. `MMC_MoveRelativeExCmd` C API 원형은 본문에서 확인되지 않습니다. |
| 17 | 18 | `MMC_ReadBoolParameter` | 분석 문서 매칭 | [6.2.17 MMC_ReadBoolParameter](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md) | 읽기 불리언 파라미터 값/상태를 조회하는 API입니다. |
| 18 | 19 | `MMC_ChngOpMode` | 분석 문서 매칭 | [6.2.37 MMC_ChngOpMode](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md) | Chng Op 모드 작업을 수행하는 API입니다. |
| 19 | 20 | `MMC_SetPositionCmd` | 분석 문서 매칭 | [6.2.28 MMC_SetPosition](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md) | 설정 위치 값/설정을 적용하는 API입니다. |
| 20 | 21 | `MMC_HomeDS402ExCmd` | 분석 문서 매칭 | [6.1.4 MMC_HomeDS402Ex](api_parts_ko/ch06_6_1_Single-Axis-Motion-Control.md) | 확장 DS-402 홈 동작을 수행하는 API입니다. |
| 21 | 22 | `MMC_GetPIVarInfoByAlias` | 원문 chunk 매칭 | [11.6.29 MMC_GetPIVarInfoByAlias](chunks/045_p1172-p1205_11.6.26-MMC_WritePIVarDouble.md#pdf-page-1181) | alias 문자열을 키로 사용해 매핑된 Processing Image 변수의 상세 정보를 조회합니다. |
| 22 | 23 | `MMC_WritePIVarUShort` | 원문 chunk 매칭 | [11.6.18 MMC_WritePIVarUShort](chunks/044_p1132-p1171_11.6.11-MMC_ReadPIVarLongLong.md#pdf-page-1149) | 지정 index의 Processing Image Unsigned Short 변수에 값을 씁니다. |
| 23 | 24 | `MMC_ReadPIVarUShort` | 원문 chunk 매칭 | [11.6.6 MMC_ReadPIVarUShort](chunks/043_p1093-p1131_11.2-Variable-Types.md#pdf-page-1116) | 지정 index와 PI 방향(input/output)에 따라 Processing Image Unsigned Short 값을 읽습니다. |
| 24 | 25 | `MMC_WriteParameter` | 분석 문서 매칭 | [6.2.35 MMC_WriteParameter](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md) | 쓰기 파라미터 값/설정을 적용하는 API입니다. |
| 25 | 26 | `MMC_SetKinTransform` | 분석 문서 매칭 | [7.10.12 MMC_SetKinTransform](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md) | 설정 Kin 변환 값/설정을 적용하는 API입니다. |
| 26 | 27 | `MMC_GroupStopCmd` | 분석 문서 매칭 | [7.9.2 MMC_GroupStop](api_parts_ko/ch07_7_9_Multiple-Axes-Motion-Control-Functions.md) | 축 또는 동작을 정지 상태로 전환하는 API입니다. |
| 27 | 28 | `MMC_StopCmd` | 분석 문서 매칭 | [6.1.14 MMC_Stop](api_parts_ko/ch06_6_1_Single-Axis-Motion-Control.md) | 축 또는 동작을 정지 상태로 전환하는 API입니다. |
| 28 | 29 | `MMC_CloseConnection` | 분석 문서 매칭 | [10.2.5 MMC_CloseConnection](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md) | 닫기 연결 작업을 수행하는 API입니다. |
| 29 | 30 | `MMC_MoveLinearAbsoluteCmd` | 분석 문서 매칭 | [7.9.10 MMC_MoveLinearAbsolute](api_parts_ko/ch07_7_9_Multiple-Axes-Motion-Control-Functions.md) | 이동 선형 절대 작업을 수행하는 API입니다. |
| 30 | 31 | `MMC_MoveAbsoluteExCmd` | C++ wrapper 보조 매칭 | [24.3.12 MoveAbsoluteEx](api_parts_ko/ch24_24_3_The-MMCSingleAxis-class.md) | C++ `MoveAbsoluteEx`는 절대 위치 이동 wrapper입니다. `MMC_MoveAbsoluteExCmd` C API 원형은 본문에서 확인되지 않습니다. |
| 31 | 32 | `MMC_MoveLinearRelativeCmd` | 분석 문서 매칭 | [7.9.11 MMC_MoveLinearRelative](api_parts_ko/ch07_7_9_Multiple-Axes-Motion-Control-Functions.md) | 이동 선형 상대 작업을 수행하는 API입니다. |
| 32 | 33 | `MMC_MoveVelocityExCmd` | 분석 문서 매칭 | [6.1.8 MMC_MoveVelocity/MMC_MoveVelocityEx](api_parts_ko/ch06_6_1_Single-Axis-Motion-Control.md) | 지정 속도로 연속 제어 이동을 수행합니다. C API 원형이 원문에 명시되어 있습니다. |
| 33 | 34 | `MMC_SetOverrideCmd` | 분석 문서 매칭 | [6.2.27 MMC_SetOverride](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md) | 설정 오버라이드 값/설정을 적용하는 API입니다. |
| 34 | 35 | `MMC_ReadActualPositionCmd` | 분석 문서 매칭 | [6.2.13 MMC_ReadActualPosition](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md) | 읽기 실제 위치 값/상태를 조회하는 API입니다. |
| 35 | 36 | `MMC_GetStatusRegisterCmd` | 분석 문서 매칭 | [10.2.19 MMC_GetStatusRegister](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md) | 조회 상태 등록 값/상태를 조회하는 API입니다. |
| 36 | 37 | `MMC_StopRecordingCmd` | 분석 문서 매칭 | [12.5.2 MMC_StopRecording](api_parts_ko/ch12_12_5_Data-Recording-Functions.md) | 축 또는 동작을 정지 상태로 전환하는 API입니다. |
| 37 | 38 | `MMC_RecStatusCmd` | 분석 문서 매칭 | [12.5.4 MMC_RecStatus](api_parts_ko/ch12_12_5_Data-Recording-Functions.md) | Rec 상태 작업을 수행하는 API입니다. |
| 38 | 39 | `MMC_BeginRecordingCmd` | 분석 문서 매칭 | [12.5.1 MMC_BeginRecording](api_parts_ko/ch12_12_5_Data-Recording-Functions.md) | Begin 기록 작업을 수행하는 API입니다. |
| 39 | 40 | `MMC_UploadDataHeaderCmd` | 분석 문서 매칭 | [12.5.5 MMC_UploadDataHeader](api_parts_ko/ch12_12_5_Data-Recording-Functions.md) | 업로드 데이터 Header 작업을 수행하는 API입니다. |
| 40 | 41 | `MMC_UploadDataCmd` | 분석 문서 매칭 | [12.5.3 MMC_UploadData](api_parts_ko/ch12_12_5_Data-Recording-Functions.md) | 업로드 데이터 작업을 수행하는 API입니다. |
| 41 | 42 | `MMC_ReadStatusCmd` | 분석 문서 매칭 | [6.2.24 MMC_ReadStatus](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md) | 읽기 상태 값/상태를 조회하는 API입니다. |
| 42 | 43 | `MMC_MoveLinearAbsoluteExCmd` | 기본형 보조 매칭 | [7.9.10 MMC_MoveLinearAbsolute](api_parts_ko/ch07_7_9_Multiple-Axes-Motion-Control-Functions.md) | 다축 그룹의 선형 절대 이동입니다. `MMC_MoveLinearAbsoluteExCmd` C API 원형은 본문에서 확인되지 않습니다. |
| 43 | 44 | `MMC_GetGroupMembersInfo` | 분석 문서 매칭 | [7.10.39 MMC_GetGroupMembersInfo](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md) | 조회 그룹 By Name 값/상태를 조회하는 API입니다. |
| 44 | 45 | `MMC_WaitUntilConditionFB` | 분석 문서 매칭 | [10.2.45 MMC_WaitUntilConditionFB](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md) | 대기 Until Condition FB 작업을 수행하는 API입니다. |

- 엑셀 API 전체 설명 작성: 44 / 44
- 매뉴얼에서 정확한 함수명 또는 동일 계열 함수로 확인: 44 / 44
- 본문 함수 정의 절에서 `CmdEx` 원형을 확인하지 못한 항목: `MMC_OpenUdpChannelCmdEx`, `MMC_MoveRelativeExCmd`, `MMC_MoveAbsoluteExCmd`, `MMC_MoveLinearAbsoluteExCmd`
- `api_parts_ko`에 상세가 부족해 원문 chunk에서 보강한 항목: `MMC_GetPIVarInfoByAlias`, `MMC_WritePIVarUShort`, `MMC_ReadPIVarUShort`

## API별 상세

### 1. `MMC_RpcInitConnection`

- Excel row: 2
- 엑셀 Define: Initiates RPC connection to Maestro server.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [10.2 Main Configuration Function Blocks - API 분석](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md)
- 원문 위치: [10.2.26 MMC_RpcInitConnection](chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1025)
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

### 2. `MMC_OpenUdpChannelCmdEx`

- Excel row: 3
- 엑셀 Define: 비어 있음
- 매칭 상태: 기본형/예제 보조 매칭
- 분석 파트: [17.1 Network Function Blocks - API 분석](api_parts_ko/ch17_17_1_Network-Function-Blocks.md)
- 기본형 원문 위치: [17.1.9 MMC_OpenUdpChannel](chunks/054_p1344-p1383_17.1-Network-Function-Blocks.md#pdf-page-1370)
- `CmdEx` 예제 위치: [24.3.4 MMCSingleAxis code example](chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1785)
- 기능 설명: UDP 채널을 열어 네트워크/이벤트 통신을 준비하는 API입니다.
- 확인 사항: 본문 함수 정의는 `MMC_OpenUdpChannelCmd`입니다. `MMC_OpenUdpChannelCmdEx`는 C++ 예제 코드에서 호출 예로 확인되지만, 이 매뉴얼 본문에 별도 `CmdEx` 시그니처는 확인되지 않습니다.
- 지원/모드: Motion Mode NC - Not Relevant Distributed - Not Relevant

#### 기본형 시그니처

```c
MMC_LIB_API int MMC_OpenUdpChannelCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_OPENUDPCHANNEL_IN* pInParam,
OUT MMC_OPENUDPCHANNEL_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_OPENUDPCHANNEL_IN`
| 필드 | 해석 |
|---|---|
| `int iEventsMask;` | 수신/구독할 이벤트 마스크입니다. |
| `int iPort;` | UDP 포트입니다. |
| `char cFirst;` | IP 주소 첫 번째 octet입니다. |
| `char cSecond;` | IP 주소 두 번째 octet입니다. |
| `char cThird;` | IP 주소 세 번째 octet입니다. |
| `char cFourth;` | IP 주소 네 번째 octet입니다. |

##### `MMC_OPENUDPCHANNEL_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 상태입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 3. `MMC_GetAxisByNameCmd`

- Excel row: 4
- 엑셀 Define: Returns an axis index reference by its name.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [10.2 Main Configuration Function Blocks - API 분석](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md)
- 원문 위치: [10.2.16 MMC_GetAxisByName](chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-999)
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

#### 구조체/주요 인자

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

### 4. `MMC_GetGroupByNameCmd`

- Excel row: 5
- 엑셀 Define: This function returns a group index reference by its name.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [10.2 Main Configuration Function Blocks - API 분석](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md)
- 원문 위치: [10.2.17 MMC_GetGroupByName](chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1001)
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

#### 구조체/주요 인자

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

### 5. `MMC_GetErrorCodeDescriptionByID`

- Excel row: 6
- 엑셀 Define: This function receives an error\warning code and returns the description and resolution from the Personality file.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [10.2 Main Configuration Function Blocks - API 분석](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md)
- 원문 위치: [10.2.13 MMC_GetErrorCodeDescriptionByID](chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-987)
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

#### 구조체/주요 인자

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

### 6. `MMC_PowerCmd`

- Excel row: 7
- 엑셀 Define: Controls the power stage (On or Off).
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.2 Single Axis Administrative Control - API 분석](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md)
- 원문 위치: [6.2.11 MMC_Power](chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-416)
- 기능 설명: 전원 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_PowerCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_POWER_IN* pInParam,
OUT MMC_POWER_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_POWER_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |
| `unsigned char ucEnablePositive;` | 활성화/비활성화 제어 값입니다. |
| `unsigned char ucEnableNegative;` | 길이, 크기 또는 개수 값입니다. |

##### `MMC_POWER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 7. `MMC_GroupReadStatusCmd`

- Excel row: 8
- 엑셀 Define: For multiple Axes. Returns the status of an axes group according to the active Group function block. This is an administrative function block, since no movement is generated.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [7.10 Multiple Axes Administrative Control - API 분석](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md)
- 원문 위치: [7.10.30 MMC_GroupReadStatus](chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-817)
- 기능 설명: 그룹 읽기 상태 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupReadStatusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPREADSTATUS_IN* pInParam,
OUT MMC_GROUPREADSTATUS_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_GROUPREADSTATUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndlr;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_GROUPREADSTATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long ulState;` | ul State 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usGroupErrorID;` | 오류 ID입니다. |

### 8. `MMC_GroupEnableCmd`

- Excel row: 9
- 엑셀 Define: For multi-axis systems. Changes the state for a group from GroupDisabled to GroupStandby. This is an
administrative function block, since no movement is generated.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [7.10 Multiple Axes Administrative Control - API 분석](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md)
- 원문 위치: [7.10.26 MMC_GroupEnable](chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-806)
- 기능 설명: 그룹 활성화 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupEnableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPENABLE_IN* pInParam,
OUT MMC_GROUPENABLE_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_GROUPENABLE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_GROUPENABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 9. `MMC_GroupDisableCmd`

- Excel row: 10
- 엑셀 Define: For multiple Axes. Changes the state for a group to GroupDisabled, although it is an administrative function block, since no movement is generated.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [7.10 Multiple Axes Administrative Control - API 분석](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md)
- 원문 위치: [7.10.25 MMC_GroupDisable](chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-803)
- 기능 설명: 그룹 비활성화 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupDisableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPDISABLE_IN* pInParam,
OUT MMC_GROUPDISABLE_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_GROUPDISABLE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_GROUPDISABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 10. `MMC_ConfigBulkReadCmd`

- Excel row: 11
- 엑셀 Define: Configures the function to read all parameters from multiple axes.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [13.1 Bulk Reading Functions - API 분석](api_parts_ko/ch13_13_1_Bulk-Reading-Functions.md)
- 원문 위치: [13.1.1 MMC_ConfigBulkRead](chunks/048_p1238-p1251_Chapter-13-Bulk-Parameters-Reading.md#pdf-page-1239)
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

#### 구조체/주요 인자

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

### 11. `MMC_PerformBulkReadCmd`

- Excel row: 12
- 엑셀 Define: Reads those parameters which were configured by a call to ConfigBulkRead, from multiple axes.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [13.1 Bulk Reading Functions - API 분석](api_parts_ko/ch13_13_1_Bulk-Reading-Functions.md)
- 원문 위치: [13.1.2 MMC_PerformBulkRead](chunks/048_p1238-p1251_Chapter-13-Bulk-Parameters-Reading.md#pdf-page-1246)
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

#### 구조체/주요 인자

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

### 12. `MMC_Reset`

- Excel row: 13
- 엑셀 Define: Provides a method to perform transition from the state ErrorStop to StandStill or Disabled by resetting all internal axis-related errors, and returns immediately.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.2 Single Axis Administrative Control - API 분석](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md)
- 원문 위치: [6.2.25 MMC_Reset](chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-460)
- 기능 설명: 리셋 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_Reset(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_RESET_IN* pInParam,
OUT MMC_RESET_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_RESET_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_RESET_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 13. `MMC_GroupResetCmd`

- Excel row: 14
- 엑셀 Define: For multiple Axes. Makes the transition from the state GroupErrorStop to GroupDisabled by resetting all
internal group-related errors – it does not affect the output of the function block instances.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [7.10 Multiple Axes Administrative Control - API 분석](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md)
- 원문 위치: [7.10.31 MMC_GroupReset](chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-820)
- 기능 설명: 그룹 리셋 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupResetCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPRESET_IN* pInParam,
OUT MMC_GROUPRESET_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_GROUPRESET_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_GROUPRESET_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 14. `MMC_SendSdoCmd`

- Excel row: 15
- 엑셀 Define: Sends SDO message command, in units of 1, 2, or 4 bytes.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [19.9 CANbus Function Blocks - API 분석](api_parts_ko/ch19_19_9_CANbus-Function-Blocks.md)
- 원문 위치: [19.9.27 MMC_SendSDO](chunks/059_p1488-p1509_19.9.24-MMC_GetBulkUploadStatus.md#pdf-page-1496)
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

#### 구조체/주요 인자

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

### 15. `MMC_ReadParameter`

- Excel row: 16
- 엑셀 Define: Returns the value of a vendor specific parameter.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.2 Single Axis Administrative Control - API 분석](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md)
- 원문 위치: [6.2.22 MMC_ReadParameter](chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-451)
- 기능 설명: 읽기 파라미터 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READPARAMETER_IN* pInParam,
OUT MMC_READPARAMETER_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_READPARAMETER_IN`
| 필드 | 해석 |
|---|---|
| `MMC_PARAMETER_LIST_ENUM eParameterNumber;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `int iParameterArrIndex;` | 인덱스 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READPARAMETER_OUT`
| 필드 | 해석 |
|---|---|
| `double dbValue;` | 전달하거나 반환받는 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 16. `MMC_MoveRelativeExCmd`

- Excel row: 17
- 엑셀 Define: 비어 있음
- 매칭 상태: C++ wrapper 보조 매칭
- 분석 파트: [24.3 The MMCSingleAxis class - API 분석](api_parts_ko/ch24_24_3_The-MMCSingleAxis-class.md)
- 원문 위치: [24.3.16 MoveRelativeEx](chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1822)
- 기능 설명: `MoveRelativeEx`는 현재 위치 기준 상대 거리 이동을 수행하는 C++ wrapper입니다. 원문은 설명, scope, motion mode는 `MMC_MoveRelative` 절을 참조하라고 명시합니다.
- 확인 사항: 추출된 매뉴얼 본문에서 `MMC_MoveRelativeExCmd`라는 C API 원형은 확인되지 않습니다. 같은 절의 Python/C API 예시는 `MMC_MoveRelativeCmd`와 `MMC_MOVERELATIVE_IN/OUT`으로 표기됩니다.

#### C++ wrapper 시그니처

```c
int MoveRelativeEx(
double dbDistance,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveRelativeEx(
double dbDistance,
double dVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveRelativeEx(
double dbDistance,
double dVel,
double dAcceleration,
double dDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveRelativeEx(
double dbDistance,
double dVel,
double dAcceleration,
double dDeceleration,
double dJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```

#### 주요 인자

| 인자 | 해석 |
|---|---|
| `dbDistance` | 목표 상대 이동 거리입니다. 양수/음수 double 값, technical unit `[u]` 기준입니다. |
| `dVel` | 최대 속도입니다. 원문 설명은 양수 값, 단위 `[u/s]`입니다. |
| `dAcceleration` | 가속도입니다. 원문 설명은 양수 값, 단위 `[u/s2]`입니다. |
| `dDeceleration` | 감속도입니다. 원문 설명은 양수 값, 단위 `[u/s2]`입니다. |
| `dJerk` | 최대 jerk 값입니다. 원문 설명은 양수 값, 단위 `[u/s3]`입니다. |
| `eBufferMode` | 버퍼링/블렌딩 동작 모드입니다. `MMC_MOVERELATIVE_IN` 구조체 설명을 참조하라고 되어 있습니다. |

### 17. `MMC_ReadBoolParameter`

- Excel row: 18
- 엑셀 Define: Returns the value of a vendor specific with datatype unsigned long or un signed int.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.2 Single Axis Administrative Control - API 분석](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md)
- 원문 위치: [6.2.17 MMC_ReadBoolParameter](chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-435)
- 기능 설명: 읽기 불리언 파라미터 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadBoolParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READBOOLPARAMETER_IN* pInParam,
OUT MMC_READBOOLPARAMETER_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_READBOOLPARAMETER_IN`
| 필드 | 해석 |
|---|---|
| `MMC_PARAMETER_LIST_ENUM eParameterNumber;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `int iParameterArrIndex;` | 인덱스 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READBOOLPARAMETER_OUT`
| 필드 | 해석 |
|---|---|
| `long lValue;` | 전달하거나 반환받는 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 18. `MMC_ChngOpMode`

- Excel row: 19
- 엑셀 Define: Changes the motion mode between NC and Distributed. This is previous determined in the DS-402 mode.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.2 Single Axis Administrative Control - API 분석](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md)
- 원문 위치: [6.2.37 MMC_ChngOpMode](chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-491)
- 기능 설명: Chng Op 모드 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ChngOpMode(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CHANGEMOTIONMODE_IN* pInParam,
OUT MMC_CHANGEMOTIONMODE_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_CHANGEMOTIONMODE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucMotionMode;` | 동작 모드 값입니다. |

##### `MMC_CHANGEMOTIONMODE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 19. `MMC_SetPositionCmd`

- Excel row: 20
- 엑셀 Define: Sends the Set Position command to the Maestro for ac specific axis.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.2 Single Axis Administrative Control - API 분석](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md)
- 원문 위치: [6.2.28 MMC_SetPosition](chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-467)
- 기능 설명: 설정 위치 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Not Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetPositionCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SETPOSITION_IN* pInParam,
OUT MMC_SETPOSITION_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_SETPOSITION_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbModulus;` | db Modulus 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucPosMode;` | 동작 모드 값입니다. |

##### `MMC_SETPOSITION_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 20. `MMC_HomeDS402ExCmd`

- Excel row: 21
- 엑셀 Define: Commands the axis to perform the Search Home DS402 sequence for a specific Axis, and can be set by the axes parameters. This function supports Velocity Hi\Lo, DetectionTimeLimit and DetectionVelocityLimit.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.1 Single Axis Motion Control - API 분석](api_parts_ko/ch06_6_1_Single-Axis-Motion-Control.md)
- 원문 위치: [6.1.4 MMC_HomeDS402Ex](chunks/016_p0313-p0350_6.1-Single-Axis-Motion-Control.md#pdf-page-340)
- 기능 설명: 확장 DS-402 홈 동작을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_HomeDS402ExCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_HOMEDS402EX_IN* pInParam,
OUT MMC_HOME_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_HOMEDS402EX_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbDetectionVelocityLimit;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fVelocityHi;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fVelocityLo;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fDistanceLimit;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fTorqueLimit;` | 토크 관련 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `int uiHomingMethod;` | 홈 동작 방식 또는 홈 관련 파라미터입니다. |
| `unsigned int uiTimeLimit;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned int uiDetectionTimeLimit;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

##### `MMC_HOME_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 21. `MMC_GetPIVarInfoByAlias`

- Excel row: 22
- 엑셀 Define: This function returns the detailed number of mapped Processing Image variables, reading the variable alias as a key.
- 매칭 상태: 원문 chunk 매칭
- 원문 위치: [11.6.29 MMC_GetPIVarInfoByAlias](chunks/045_p1172-p1205_11.6.26-MMC_WritePIVarDouble.md#pdf-page-1181)
- 기능 설명: alias 문자열을 키로 사용해서 매핑된 Processing Image 변수의 상세 정보를 조회합니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported
- 확인 사항: 원문 typedef의 입력 구조체에는 `pAliasing`만 보입니다. 같은 절 예제에는 `GetIn.ucDirection = ePI_OUTPUT;`가 나오지만 typedef에는 해당 필드가 없어 매뉴얼 내 표기 불일치로 봐야 합니다.

#### 시그니처

```c
MMC_LIB_API int MMC_GetPIVarInfoByAlias(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETPIVARINFOBYALIAS_IN* pInParam,
OUT MMC_GETPIVARINFOBYALIAS_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_GETPIVARINFOBYALIAS_IN`
| 필드 | 해석 |
|---|---|
| `char pAliasing[PI_ALIASING_LENGTH];` | PI 변수 alias 이름입니다. 최대 길이는 `PI_ALIASING_LENGTH`입니다. 원문 remarks에 따르면 alias는 input/output을 나타내는 `I` 또는 `O`와 `objectNum.Subindex` 형식으로 구성됩니다. |

##### `MMC_GETPIVARINFOBYALIAS_OUT`
| 필드 | 해석 |
|---|---|
| `NC_PI_INFO_BY_ALIAS VarInfo;` | PI 변수 상세 정보입니다. CANopen index/sub-index와 EtherCAT cyclic frame 내 bit size/offset 정보를 포함합니다. |
| `unsigned short usStatus;` | 명령 상태입니다. |
| `short usErrorID;` | 오류 ID입니다. 원문 Parameters에는 `sErrorID`로도 표기됩니다. |

##### `NC_PI_INFO_BY_ALIAS`
| 필드 | 해석 |
|---|---|
| `unsigned int uiBitSize;` | 변수 bit 크기입니다. |
| `unsigned int uiBitOffset;` | EtherCAT frame 내 첫 bit offset입니다. |
| `unsigned short usCanOpenIndex;` | CANopen index입니다. |
| `unsigned short usPIVarOffset;` | PI 변수 offset입니다. |
| `unsigned char ucCanOpenSubIndex;` | CANopen sub-index입니다. |
| `unsigned char ucVarType;` | PI 변수 타입입니다. 예: `ePI_UNSIGNED_SHORT = 4`. |
| `unsigned char ucDirection;` | PI 방향입니다. |
| `unsigned char ucPadding;` | padding 필드입니다. |

### 22. `MMC_WritePIVarUShort`

- Excel row: 23
- 엑셀 Define: This function writes a Processing Image input\output Unsigned Short variable according to its index
- 매칭 상태: 원문 chunk 매칭
- 원문 위치: [11.6.18 MMC_WritePIVarUShort](chunks/044_p1132-p1171_11.6.11-MMC_ReadPIVarLongLong.md#pdf-page-1149)
- 기능 설명: 지정한 PI index에 Unsigned Short 값을 씁니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WritePIVarUShort(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEPIVARUSHORT_IN* pInParam,
OUT MMC_WRITEPIVARUSHORT_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_WRITEPIVARUSHORT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usData;` | PI에 기록할 unsigned short 값입니다. |
| `unsigned short usIndex;` | 기록 대상 PI index입니다. 원문 설명은 양의 정수 값입니다. |

##### `MMC_WRITEPIVARUSHORT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 상태입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 23. `MMC_ReadPIVarUShort`

- Excel row: 24
- 엑셀 Define: This function reads a Processing Image input\output Unsigned Short Variable according to its index
- 매칭 상태: 원문 chunk 매칭
- 원문 위치: [11.6.6 MMC_ReadPIVarUShort](chunks/043_p1093-p1131_11.2-Variable-Types.md#pdf-page-1116)
- 기능 설명: 지정한 PI index와 방향(input/output)에 따라 Unsigned Short 값을 읽습니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadPIVarUShort(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READPIVARUSHORT_IN* pInParam,
OUT MMC_READPIVARUSHORT_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_READPIVARUSHORT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned short usIndex;` | 읽을 PI index입니다. 원문 설명은 양의 정수 값입니다. |
| `unsigned char ucDirection;` | PI 방향입니다. `ePI_INPUT = 0`, `ePI_OUTPUT = 1`. |

##### `MMC_READPIVARUSHORT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usData;` | 읽은 unsigned short 값입니다. |
| `unsigned short usStatus;` | 명령 상태입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 24. `MMC_WriteParameter`

- Excel row: 25
- 엑셀 Define: Modifies the value of a vendor specific parameter.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.2 Single Axis Administrative Control - API 분석](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md)
- 원문 위치: [6.2.35 MMC_WriteParameter](chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-485)
- 기능 설명: 쓰기 파라미터 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WriteParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEPARAMETER_IN* pInParam,
OUT MMC_WRITEPARAMETER_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_WRITEPARAMETER_IN`
| 필드 | 해석 |
|---|---|
| `double dbValue;` | 전달하거나 반환받는 값입니다. |
| `MMC_PARAMETER_LIST_ENUM eParameterNumber;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `int iParameterArrIndex;` | 인덱스 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_WRITEPARAMETER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 25. `MMC_SetKinTransform`

- Excel row: 26
- 엑셀 Define: Sets a kinematic transformation between the ACS and MCS based on the predefined kinematic model for multi-axes. Refer to the section 7.1Coordinate System and kinematic transformation for a further detailed explanation. Refer to sections Coordinated System and Kinematic Transformation Definitions onwards for details of the structures used within this function.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [7.10 Multiple Axes Administrative Control - API 분석](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md)
- 원문 위치: [7.10.12 MMC_SetKinTransform](chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-748)
- 기능 설명: 설정 Kin 변환 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetKinTransform(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SETKINTRANSFORM_IN* pInParam,
OUT MMC_SETKINTRANSFORM_OUT* pOutParam);
```

#### 구조체/주요 인자

##### `MMC_SETKINTRANSFORM_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double ulTrCoef[NC_MAX_NUM_AXES_IN_NODE][NC_MAX_NUM_COEF];` | 노드 식별 또는 노드 관련 값입니다. |
| `int iNumAxes;` | 길이, 크기 또는 개수 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `NC_TR_FUNC_ID_ENUM iMcsToAcsFuncID[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `NC_NODE_HNDL_T hNode[NC_MAX_NUM_AXES_IN_NODE];` | 함수 블록 또는 리소스 핸들입니다. |
| `NC_AXIS_IN_GROUP_TYPE_ENUM eType[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |

##### `MMC_SETKINTRANSFORM_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 26. `MMC_GroupStopCmd`

- Excel row: 27
- 엑셀 Define: For multi-axis systems. Brings a group of axes to stop status.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [7.9 Multiple Axes Motion Control - Functions - API 분석](api_parts_ko/ch07_7_9_Multiple-Axes-Motion-Control-Functions.md)
- 원문 위치: [7.9.2 MMC_GroupStop](chunks/025_p0620-p0656_7.8.5-Online-Splines.md#pdf-page-629)
- 기능 설명: 축 또는 동작을 정지 상태로 전환하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupStopCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPSTOP_IN* pInParam,
OUT MMC_GROUPSTOP_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_GROUPSTOP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_GROUPSTOP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 27. `MMC_StopCmd`

- Excel row: 28
- 엑셀 Define: Commands a controlled motion stop and transfers the axis to the state Stopping.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.1 Single Axis Motion Control - API 분석](api_parts_ko/ch06_6_1_Single-Axis-Motion-Control.md)
- 원문 위치: [6.1.14 MMC_Stop](chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-388)
- 기능 설명: 축 또는 동작을 정지 상태로 전환하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_StopCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_STOP_IN* pInParam,
OUT MMC_STOP_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_STOP_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_STOP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 28. `MMC_CloseConnection`

- Excel row: 29
- 엑셀 Define: Closes the connection to the Maestro.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [10.2 Main Configuration Function Blocks - API 분석](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md)
- 원문 위치: [10.2.5 MMC_CloseConnection](chunks/038_p0956-p0989_Chapter-10-API-Services-and-Operations.md#pdf-page-969)
- 기능 설명: 닫기 연결 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_CloseConnection(
IN MMC_CONNECT_HNDL hConn
);
```

### 29. `MMC_MoveLinearAbsoluteCmd`

- Excel row: 30
- 엑셀 Define: For multi-axis systems. Commands an interpolated linear movement on an axes group from the actual position of the TCP to an absolute position in the specified coordinate system.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [7.9 Multiple Axes Motion Control - Functions - API 분석](api_parts_ko/ch07_7_9_Multiple-Axes-Motion-Control-Functions.md)
- 원문 위치: [7.9.10 MMC_MoveLinearAbsolute](chunks/026_p0657-p0693_7.9.7-MMC_MoveCircularAbsoluteRadius.md#pdf-page-675)
- 기능 설명: 이동 선형 절대 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveLinearAbsoluteCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVELINEARABSOLUTE_IN* pInParam,
OUT MMC_MOVELINEARABSOLUTE_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_MOVELINEARABSOLUTE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbPosition[NC_MAX_NUM_AXES_IN_NODE];` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucSuperimposed;` | uc Superimposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MOVELINEARABSOLUTE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 30. `MMC_MoveAbsoluteExCmd`

- Excel row: 31
- 엑셀 Define: 비어 있음
- 매칭 상태: C++ wrapper 보조 매칭
- 분석 파트: [24.3 The MMCSingleAxis class - API 분석](api_parts_ko/ch24_24_3_The-MMCSingleAxis-class.md)
- 원문 위치: [24.3.12 MoveAbsoluteEx](chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1810)
- 기능 설명: `MoveAbsoluteEx`는 지정한 절대 위치로 축을 이동시키는 C++ wrapper입니다. 원문은 설명, scope, motion mode는 `MMC_MoveAbsolute` 절을 참조하라고 명시합니다.
- 확인 사항: 추출된 매뉴얼 본문에서 `MMC_MoveAbsoluteExCmd`라는 C API 원형은 확인되지 않습니다. 같은 절의 Python/C API 예시는 `MMC_MoveAbsoluteCmd`와 `MMC_MOVEABSOLUTE_IN/OUT`으로 표기됩니다. 또한 원문 두 번째 오버로드는 절 제목과 달리 `MoveAbsolute`로 표기되어 있습니다.

#### C++ wrapper 시그니처

```c
int MoveAbsoluteEx(
double dPos,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAbsolute(
double dPos,
double dVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAbsoluteEx(
double dPos,
double dVel,
double dAcceleration,
double dDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAbsoluteEx(
double dPos,
double dVel,
double dAcceleration,
double dDeceleration,
double dJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```

#### 주요 인자

| 인자 | 해석 |
|---|---|
| `dPos` | 목표 절대 위치입니다. 양수/음수 double 값, technical unit `[u]` 기준입니다. |
| `dVel` | 최대 속도입니다. 원문 설명은 양수 값, 단위 `[u/s]`입니다. |
| `dAcceleration` | 가속도입니다. 원문 설명은 양수 값, 단위 `[u/s2]`입니다. |
| `dDeceleration` | 감속도입니다. 원문 설명은 양수 값, 단위 `[u/s2]`입니다. |
| `dJerk` | 최대 jerk 값입니다. 원문 설명은 양수 값, 단위 `[u/s3]`입니다. |
| `eBufferMode` | 버퍼링/블렌딩 동작 모드입니다. |

### 31. `MMC_MoveLinearRelativeCmd`

- Excel row: 32
- 엑셀 Define: For multi-axis systems. Commands an interpolated linear movement on an axes group from the actual position of the TCP to a relative distance in the specified coordinate system.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [7.9 Multiple Axes Motion Control - Functions - API 분석](api_parts_ko/ch07_7_9_Multiple-Axes-Motion-Control-Functions.md)
- 원문 위치: [7.9.11 MMC_MoveLinearRelative](chunks/026_p0657-p0693_7.9.7-MMC_MoveCircularAbsoluteRadius.md#pdf-page-682)
- 기능 설명: 이동 선형 상대 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveLinearRelativeCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVELINEARRELATIVE_IN* pInParam,
OUT MMC_MOVELINEARRELATIVE_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_MOVELINEARRELATIVE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbDistance[NC_MAX_NUM_AXES_IN_NODE];` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucSuperimposed;` | uc Superimposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MOVELINEARRELATIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 32. `MMC_MoveVelocityExCmd`

- Excel row: 33
- 엑셀 Define: 비어 있음
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.1 Single Axis Motion Control - API 분석](api_parts_ko/ch06_6_1_Single-Axis-Motion-Control.md)
- 원문 위치: [6.1.8 MMC_MoveVelocity/MMC_MoveVelocityEx](chunks/017_p0351-p0387_6.1.6-MMC_MoveAdditive.md#pdf-page-361)
- 기능 설명: 지정 속도로 연속 제어 이동을 수행합니다. 정지는 새 function block이 현재 function block을 interrupt/abort하는 방식으로 처리된다고 설명되어 있습니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported
- 확인 사항: 원문은 `MMC_MoveVelocityExCmd` 시그니처에서 `MMC_MOVEVELOCITYEX_IN/OUT` 포인터를 사용하지만, 같은 절의 상세 구조체 정의는 `MMC_MOVEVELOCITY_IN/OUT`만 제공합니다. 즉 `EX` 구조체의 필드 목록은 이 매뉴얼 본문에서 별도로 확인되지 않습니다.

#### 시그니처

```c
MMC_LIB_API int MMC_MoveVelocityExCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVEVELOCITYEX_IN* pInParam,
OUT MMC_MOVEVELOCITYEX_OUT* pOutParam
);
```

#### 공통 호출 인자

| 인자 | 해석 |
|---|---|
| `hConn` | Maestro 연결 핸들입니다. Init Connection 계열 함수에서 반환됩니다. |
| `hAxisRef` | 대상 축 참조 핸들입니다. |
| `pInParam` | 입력 구조체 포인터입니다. 원문 시그니처는 `MMC_MOVEVELOCITYEX_IN*`입니다. |
| `pOutParam` | 출력 구조체 포인터입니다. 원문 시그니처는 `MMC_MOVEVELOCITYEX_OUT*`입니다. |

#### 같은 절에 제공된 기본 구조체

##### `MMC_MOVEVELOCITY_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 상승 에지에서 명령을 시작하는 TRUE/FALSE 실행 트리거입니다. |
| `float fVelocity;` | 최대 속도입니다. 방향을 포함할 수 있으며 양수 또는 음수 값, 단위 `[u/s]`입니다. |
| `float fAcceleration;` | 가속도입니다. 양수 값, 단위 `[u/s2]`입니다. |
| `float fDeceleration;` | 감속도입니다. 양수 값, 단위 `[u/s2]`입니다. |
| `float fJerk;` | jerk 값입니다. 양수 값, 단위 `[u/s3]`입니다. |
| `MC_DIRECTION_ENUM eDirection;` | 이동 방향입니다. `MC_NONE_DIRECTION`, `MC_POSITIVE_DIRECTION`, `MC_SHORTEST_WAY`, `MC_NEGATIVE_DIRECTION`, `MC_CURRENT_DIRECTION`가 나열되어 있고, `MC_SHORTEST_WAY`는 아직 구현되지 않았다고 설명되어 있습니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. 원문은 `MMC_MoveVelocity`를 `MC_ABORTING_MODE`로 사용하는 것을 권장합니다. |

##### `MMC_MOVEVELOCITY_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 반환된 function block handle입니다. |
| `unsigned short usStatus;` | 명령 상태입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 33. `MMC_SetOverrideCmd`

- Excel row: 34
- 엑셀 Define: Sets the values of override for the whole axis, including all functions that are operating on that axis.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.2 Single Axis Administrative Control - API 분석](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md)
- 원문 위치: [6.2.27 MMC_SetOverride](chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-464)
- 기능 설명: 설정 오버라이드 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetOverrideCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SETOVERRIDE_IN* pInParam,
OUT MMC_SETOVERRIDE_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_SETOVERRIDE_IN`
| 필드 | 해석 |
|---|---|
| `float fVelFactor;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAccFactor;` | f Acc Factor 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `float fJerkFactor;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `unsigned short usUpdateVelFactorIdx;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_SETOVERRIDE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 34. `MMC_ReadActualPositionCmd`

- Excel row: 35
- 엑셀 Define: Returns the actual position of the controlled axis.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.2 Single Axis Administrative Control - API 분석](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md)
- 원문 위치: [6.2.13 MMC_ReadActualPosition](chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-423)
- 기능 설명: 읽기 실제 위치 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadActualPositionCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READACTUALPOSITION_IN* pInParam,
OUT MMC_READACTUALPOSITION_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_READACTUALPOSITION_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READACTUALPOSITION_OUT`
| 필드 | 해석 |
|---|---|
| `double dbPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 35. `MMC_GetStatusRegisterCmd`

- Excel row: 36
- 엑셀 Define: The purpose of the function is to provide usable information regarding the Maestro and axes statuses.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [10.2 Main Configuration Function Blocks - API 분석](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md)
- 원문 위치: [10.2.19 MMC_GetStatusRegister](chunks/039_p0990-p1028_10.2.14-MMC_GetFoEStatus.md#pdf-page-1007)
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

#### 구조체/주요 인자

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

### 36. `MMC_StopRecordingCmd`

- Excel row: 37
- 엑셀 Define: Halts recording of the Maestro server data.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [12.5 Data Recording Functions - API 분석](api_parts_ko/ch12_12_5_Data-Recording-Functions.md)
- 원문 위치: [12.5.2 MMC_StopRecording](chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1226)
- 기능 설명: 축 또는 동작을 정지 상태로 전환하는 API입니다.
- 지원/모드: Motion Mode NC - N/A Distributed - N/A

#### 시그니처

```c
MMC_LIB_API int MMC_StopRecordingCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_STOP_RECORDING_IN* pInParam,
OUT MMC_STOP_RECORDING_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_STOP_RECORDING_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_STOP_RECORDING_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 37. `MMC_RecStatusCmd`

- Excel row: 38
- 엑셀 Define: Requests the status of the recording.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [12.5 Data Recording Functions - API 분석](api_parts_ko/ch12_12_5_Data-Recording-Functions.md)
- 원문 위치: [12.5.4 MMC_RecStatus](chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1231)
- 기능 설명: Rec 상태 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - N/A Distributed - N/A

#### 시그니처

```c
MMC_LIB_API int MMC_RecStatusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_REC_STATUS_IN* pInParam,
OUT MMC_REC_STATUS_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_REC_STATUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_REC_STATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long uiRr;` | ui Rr 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long uiSr;` | ui Sr 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 38. `MMC_BeginRecordingCmd`

- Excel row: 39
- 엑셀 Define: Starts the recording of internal controller variables data from the Maestro server.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [12.5 Data Recording Functions - API 분석](api_parts_ko/ch12_12_5_Data-Recording-Functions.md)
- 원문 위치: [12.5.1 MMC_BeginRecording](chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1222)
- 기능 설명: Begin 기록 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - N/A Distributed - N/A

#### 시그니처

```c
MMC_LIB_API int MMC_BeginRecordingCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_BEGIN_RECORDING_IN* pInParam,
OUT MMC_BEGIN_RECORDING_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_BEGIN_RECORDING_IN`
| 필드 | 해석 |
|---|---|
| `unsigned long uiRg;` | ui Rg 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long uiRl;` | ui Rl 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long uiRc;` | ui Rc 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned long uiRv[NC_MAX_REC_SIGNALS_NUM];` | 길이, 크기 또는 개수 값입니다. |
| `unsigned long uiRp[NC_MAX_REC_PARAMS_NUM];` | 파라미터 식별자 또는 파라미터 값입니다. |

##### `MMC_BEGIN_RECORDING_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 39. `MMC_UploadDataHeaderCmd`

- Excel row: 40
- 엑셀 Define: Recorder upload data header.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [12.5 Data Recording Functions - API 분석](api_parts_ko/ch12_12_5_Data-Recording-Functions.md)
- 원문 위치: [12.5.5 MMC_UploadDataHeader](chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1234)
- 기능 설명: 업로드 데이터 Header 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - N/A Distributed - N/A

#### 시그니처

```c
MMC_LIB_API int MMC_UploadDataHeaderCmd(
IN MMC_CONNECT_HNDL hConn,
OUT NC_UPLOAD_REC_HEADER_STRUCT* pOutParam
);
```

### 40. `MMC_UploadDataCmd`

- Excel row: 41
- 엑셀 Define: Uploads recording data to the Maestro.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [12.5 Data Recording Functions - API 분석](api_parts_ko/ch12_12_5_Data-Recording-Functions.md)
- 원문 위치: [12.5.3 MMC_UploadData](chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1228)
- 기능 설명: 업로드 데이터 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - N/A Distributed - N/A

#### 시그니처

```c
MMC_LIB_API int MMC_UploadDataCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_UPLOAD_DATA_IN* pInParam,
OUT MMC_UPLOAD_DATA_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_UPLOAD_DATA_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiFrom;` | ui From 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned int uiTo;` | ui To 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned int uiBufIdx;` | 인덱스 값입니다. |

##### `MMC_UPLOAD_DATA_OUT`
| 필드 | 해석 |
|---|---|
| `long ulUpdatData[NC_MAX_LONG];` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 41. `MMC_ReadStatusCmd`

- Excel row: 42
- 엑셀 Define: Returns details of the state diagram status for the selected axis.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [6.2 Single Axis Administrative Control - API 분석](api_parts_ko/ch06_6_2_Single-Axis-Administrative-Control.md)
- 원문 위치: [6.2.24 MMC_ReadStatus](chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-457)
- 기능 설명: 읽기 상태 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadStatusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READSTATUS_IN* pInParam,
OUT MMC_READSTATUS_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_READSTATUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndlr;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READSTATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long ulState;` | ul State 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usAxisErrorID;` | 오류 ID입니다. |
| `unsigned short usStatusWord;` | 명령 또는 장치 상태 값입니다. |

### 42. `MMC_MoveLinearAbsoluteExCmd`

- Excel row: 43
- 엑셀 Define: 비어 있음
- 매칭 상태: 기본형 보조 매칭
- 분석 파트: [7.9 Multiple Axes Motion Control - Functions - API 분석](api_parts_ko/ch07_7_9_Multiple-Axes-Motion-Control-Functions.md)
- 원문 위치: [7.9.10 MMC_MoveLinearAbsolute](chunks/026_p0657-p0693_7.9.7-MMC_MoveCircularAbsoluteRadius.md#pdf-page-675)
- 기능 설명: 다축 group에서 TCP를 지정 좌표계의 절대 위치로 보간 선형 이동시키는 API입니다.
- 확인 사항: 추출된 매뉴얼 본문에서 `MMC_MoveLinearAbsoluteExCmd`라는 C API 원형은 확인되지 않습니다. 본문에는 기본형 `MMC_MoveLinearAbsoluteCmd`와 C++ `MoveLinearAbsolute` wrapper가 확인됩니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 기본형 시그니처

```c
MMC_LIB_API int MMC_MoveLinearAbsoluteCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVELINEARABSOLUTE_IN* pInParam,
OUT MMC_MOVELINEARABSOLUTE_OUT* pOutParam
);
```

#### 구조체/주요 인자

##### `MMC_MOVELINEARABSOLUTE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 상승 에지에서 명령을 시작하는 TRUE/FALSE 실행 트리거입니다. |
| `double dbPosition[NC_MAX_NUM_AXES_IN_NODE];` | 축 group/TCP의 목표 절대 위치 배열입니다. |
| `float fVelocity;` | 최대 속도입니다. 단위 `[u/s]`입니다. |
| `float fAcceleration;` | 가속도입니다. 단위 `[u/s2]`입니다. |
| `float fDeceleration;` | 감속도입니다. 단위 `[u/s2]`입니다. |
| `float fJerk;` | jerk 값입니다. 단위 `[u/s3]`입니다. |
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | transition mode에서 사용하는 전이 파라미터 배열입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | 좌표계 지정 값입니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 전이/블렌딩 방식 지정 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucSuperimposed;` | superimposed 동작 여부 관련 플래그입니다. |

##### `MMC_MOVELINEARABSOLUTE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 반환된 function block handle입니다. |
| `unsigned short usStatus;` | 명령 상태입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 43. `MMC_GetGroupMembersInfo`

- Excel row: 44
- 엑셀 Define: Returns information about a specific group and its members.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [7.10 Multiple Axes Administrative Control - API 분석](api_parts_ko/ch07_7_10_Multiple-Axes-Administrative-Control.md)
- 원문 위치: [7.10.39 MMC_GetGroupMembersInfo](chunks/031_p0843-p0846_7.10.39-MMC_GetGroupMembersInfo.md#pdf-page-843)
- 기능 설명: 조회 그룹 By Name 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetGroupMembersInfo(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETGROUPMEMBERSINFO_IN * pInParam,
OUT MMC_GETGROUPMEMBERSINFO_OUT * pOutParam
);
```
```c
MMC_GetGroupByNameCmd(hConn,&sGroupByNameInParam,&sGroupByNameOutParam);
//
// Create input and output structures
MMC_GETGROUPMEMBERSINFO_IN sMembersInfoInParam;
MMC_GETGROUPMEMBERSINFO_OUT sMembersInfoOutParam;
//
// There are no neccessary inputs in the input structure (only dummy
variable)
sMembersInfoInParam.ucDummy = 0;
//
// call GetGroupMembersInfo function (assume that there are not errors in
this function)
MMC_GetGroupMembersInfo(hConn,sGroupByNameOutParam.usAxisIdx,&sMembersInfoI
nParam,&sMembersInfoOutParam);
```

#### 구조체/주요 인자

##### `MMC_GETGROUPMEMBERSINFO_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETGROUPMEMBERSINFO_OUT`
| 필드 | 해석 |
|---|---|
| `char pAxesNames[NC_MAX_NUM_AXES_IN_NODE][NODE_NAME_MAX_LENGTH];` | 노드 식별 또는 노드 관련 값입니다. |
| `unsigned short pAxesReferences[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `unsigned short pDeviceID[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucNumOfAxes;` | 길이, 크기 또는 개수 값입니다. |

### 44. `MMC_WaitUntilConditionFB`

- Excel row: 45
- 엑셀 Define: The operation of this function block allows synchronization of numerous axes that are not part of a group, to start their motion together. In addition, it allows synchronization of numerous networked Maestro’s by starting a motion when a specific bit on a shared IO is raised.
- 매칭 상태: 분석 문서 매칭
- 분석 파트: [10.2 Main Configuration Function Blocks - API 분석](api_parts_ko/ch10_10_2_Main-Configuration-Function-Blocks.md)
- 원문 위치: [10.2.45 MMC_WaitUntilConditionFB](chunks/041_p1066-p1091_10.2.43-MMC_WriteGroupOfParametersEx.md#pdf-page-1074)
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

#### 구조체/주요 인자

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

# 14.22 Notification and Event Function Blocks in C - API 분석

- 원본 장: `Chapter 14 API Events (C & C++)`
- 시작 PDF 페이지: 1284
- 원문 위치: [14.22 Notification and Event Function Blocks in C](../chunks/049_p1252-p1290_Chapter-14-API-Events-C-C++.md#pdf-page-1284)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `14.22.1` | 1285 | `MMC_InsertNotificationFb` | Insert Notification Fb 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `14.22.2` | 1288 | `MMC_ClearEventsMaskCmd` | 초기화 Events 마스크 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `14.22.3` | 1291 | `MMC_DisableMotionEndedEventCmd` | 비활성화 Motion Ended 이벤트 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `14.22.4` | 1294 | `MMC_EnableMotionEndedEventCmd` | 활성화 Motion Ended 이벤트 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `14.22.5` | 1297 | `MMC_GetEventsMaskCmd` | 조회 Events 마스크 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `14.22.6` | 1300 | `MMC_SetEventsMaskCmd` | 설정 Events 마스크 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 14.22.1 MMC_InsertNotificationFb

- PDF 페이지: 1285
- 원문 위치: [14.22.1 MMC_InsertNotificationFb](../chunks/049_p1252-p1290_Chapter-14-API-Events-C-C++.md#pdf-page-1285)
- 기능 설명: Insert Notification Fb 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_InsertNotificationFb(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_INSNOTIFICATIONFB_IN* pInParam,
OUT MMC_INSNOTIFICATIONFB_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_INSNOTIFICATIONFB_IN`
| 필드 | 해석 |
|---|---|
| `int iEventCode;` | i 이벤트 Code 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `long lSpare[8];` | l Spare[8] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_INSNOTIFICATIONFB_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 14.22.2 MMC_ClearEventsMask

- PDF 페이지: 1288
- 원문 위치: [14.22.2 MMC_ClearEventsMask](../chunks/049_p1252-p1290_Chapter-14-API-Events-C-C++.md#pdf-page-1288)
- 기능 설명: 초기화 Events 마스크 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ClearEventsMaskCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_CLEAREVENTSMASK_IN* pInParam,
OUT MMC_CLEAREVENTSMASK_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CLEAREVENTSMASK_IN`
| 필드 | 해석 |
|---|---|
| `int iEventsMask;` | 마스크 값입니다. |

##### `MMC_CLEAREVENTSMASK_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 14.22.3 MMC_DisableMotionEndedEvent

- PDF 페이지: 1291
- 원문 위치: [14.22.3 MMC_DisableMotionEndedEvent](../chunks/050_p1291-p1299_14.22.3-MMC_DisableMotionEndedEvent.md#pdf-page-1291)
- 기능 설명: 비활성화 Motion Ended 이벤트 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_DisableMotionEndedEventCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_DISABLEMOTIONENDEDEVENT_IN* pInParam,
OUT MMC_DISABLEMOTIONENDEDEVENT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_DISABLEMOTIONENDEDEVENT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_DISABLEMOTIONENDEDEVENT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 14.22.4 MMC_EnableMotionEndedEvent

- PDF 페이지: 1294
- 원문 위치: [14.22.4 MMC_EnableMotionEndedEvent](../chunks/050_p1291-p1299_14.22.3-MMC_DisableMotionEndedEvent.md#pdf-page-1294)
- 기능 설명: 활성화 Motion Ended 이벤트 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_EnableMotionEndedEventCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_ENABLEMOTIONENDEDEVENT_IN* pInParam,
OUT MMC_ENABLEMOTIONENDEDEVENT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_ENABLEMOTIONENDEDEVENT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_ENABLEMOTIONENDEDEVENT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 14.22.5 MMC_GetEventsMask

- PDF 페이지: 1297
- 원문 위치: [14.22.5 MMC_GetEventsMask](../chunks/050_p1291-p1299_14.22.3-MMC_DisableMotionEndedEvent.md#pdf-page-1297)
- 기능 설명: 조회 Events 마스크 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetEventsMaskCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETEVENTSMASK_IN* pInParam,
OUT MMC_GETEVENTSMASK_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETEVENTSMASK_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETEVENTSMASK_OUT`
| 필드 | 해석 |
|---|---|
| `int iEventsMask;` | 마스크 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 14.22.6 MMC_SetEventsMask

- PDF 페이지: 1300
- 원문 위치: [14.22.6 MMC_SetEventsMask](../chunks/051_p1300-p1302_14.22.6-MMC_SetEventsMask.md#pdf-page-1300)
- 기능 설명: 설정 Events 마스크 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetEventsMaskCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SETEVENTSMASK_IN* pInParam,
OUT MMC_SETEVENTSMASK_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETEVENTSMASK_IN`
| 필드 | 해석 |
|---|---|
| `int iEventsMask;` | 마스크 값입니다. |

##### `MMC_SETEVENTSMASK_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

# 7.9 Multiple Axes Motion Control - Functions - API 분석

- 원본 장: `Chapter 7 Motion and Administrative - Multi-Axis`
- 시작 PDF 페이지: 627
- 원문 위치: [7.9 Multiple Axes Motion Control - Functions](../chunks/025_p0620-p0656_7.8.5-Online-Splines.md#pdf-page-627)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `7.9.1` | 628 | `Function Block Status Bit Masks` | 함수 블록 상태 Bit Masks 작업을 수행하는 API입니다. | - |
| `7.9.2` | 629 | `MMC_GroupStopCmd` | 축 또는 동작을 정지 상태로 전환하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.3` | 633 | `MMC_GroupHaltCmd` | 축을 정상 운전 조건에서 제어 정지시키는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.4` | 637 | `MMC_MoveCircularAbsoluteCmd` | 이동 원호 절대 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.5` | 645 | `MMC_MoveCircularAbsoluteCenterCmd` | 이동 원호 절대 Center 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.6` | 651 | `MMC_MoveCircularAbsoluteBorderCmd` | 이동 원호 절대 Border 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.7` | 657 | `MMC_MoveCircularAbsoluteRadiusCmd` | 이동 원호 절대 Radius 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.8` | 664 | `MMC_MoveCircularAbsoluteAngleCmd` | 이동 원호 절대 Angle 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.9` | 670 | `MMC_MoveAngle` | 이동 Angle 작업을 수행하는 API입니다. | - |
| `7.9.10` | 675 | `MMC_MoveLinearAbsoluteCmd` | 이동 선형 절대 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.11` | 682 | `MMC_MoveLinearRelativeCmd` | 이동 선형 상대 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.12` | 689 | `MMC_MoveLinearAdditiveCmd` | 이동 선형 가산 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.13` | 694 | `MMC_MoveLinearAdditiveExCmd` | 이동 선형 가산 Ex 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.14` | 699 | `MMC_MoveLinearAbsoluteRepetitiveCmd` | 이동 선형 절대 반복 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.15` | 705 | `MMC_MoveLinearRelativeRepetitive` | 이동 선형 상대 반복 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.16` | 710 | `MMC_MovePolynomAbsoluteCmd` | 이동 Polynom 절대 작업을 수행하는 API입니다. | - |
| `7.9.17` | 714 | `MMC_PathSelectCmd` | 값 또는 상태를 읽는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.18` | 717 | `MMC_MovePathCmd` | 이동 경로 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.9.19` | 724 | `MMC_PathUnselectCmd` | 경로 Unselect 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 7.9.1 Function Block Status Bit Masks

- PDF 페이지: 628
- 원문 위치: [7.9.1 Function Block Status Bit Masks](../chunks/025_p0620-p0656_7.8.5-Online-Splines.md#pdf-page-628)
- 기능 설명: 함수 블록 상태 Bit Masks 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.9.2 MMC_GroupStop

- PDF 페이지: 629
- 원문 위치: [7.9.2 MMC_GroupStop](../chunks/025_p0620-p0656_7.8.5-Online-Splines.md#pdf-page-629)
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

#### 구조체/인자

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

### 7.9.3 MMC_GroupHalt

- PDF 페이지: 633
- 원문 위치: [7.9.3 MMC_GroupHalt](../chunks/025_p0620-p0656_7.8.5-Online-Splines.md#pdf-page-633)
- 기능 설명: 축을 정상 운전 조건에서 제어 정지시키는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupHaltCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPHALT_IN* pInParam,
OUT MMC_GROUPHALT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GROUPHALT_IN`
| 필드 | 해석 |
|---|---|
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_GROUPHALT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 7.9.4 MMC_MoveCircularAbsolute

- PDF 페이지: 637
- 원문 위치: [7.9.4 MMC_MoveCircularAbsolute](../chunks/025_p0620-p0656_7.8.5-Online-Splines.md#pdf-page-637)
- 기능 설명: 이동 원호 절대 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveCircularAbsoluteCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVECIRCULARABSOLUTE_IN* pInParam,
OUT MMC_MOVECIRCULARABSOLUTE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVECIRCULARABSOLUTE_IN`
| 필드 | 해석 |
|---|---|
| `double dAuxPoint[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `double dEndPoint[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `NC_PATH_CHOICE_ENUM ePathChoice;` | 파일명, 경로, 이름 문자열입니다. |
| `NC_ARC_SHORT_LONG_ENUM eArcShortLong;` | e Arc Short Long 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_CIRC_MODE_ENUM eCircleMode;` | 동작 모드 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucSuperimposed;` | uc Superimposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_MOVECIRCULARABSOLUTE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.5 MMC_MoveCircularAbsoluteCenter

- PDF 페이지: 645
- 원문 위치: [7.9.5 MMC_MoveCircularAbsoluteCenter](../chunks/025_p0620-p0656_7.8.5-Online-Splines.md#pdf-page-645)
- 기능 설명: 이동 원호 절대 Center 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveCircularAbsoluteCenterCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVECIRCULARABSOLUTECENTER_IN* pInParam,
OUT MMC_MOVECIRCULARABSOLUTE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVECIRCULARABSOLUTECENTER_IN`
| 필드 | 해석 |
|---|---|
| `double dCenterPoint[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `double dEndPoint[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `NC_ARC_SHORT_LONG_ENUM eArcShortLong;` | e Arc Short Long 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucSuperimposed;` | uc Superimposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_MOVECIRCULARABSOLUTE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.6 MMC_MoveCircularAbsoluteBorder

- PDF 페이지: 651
- 원문 위치: [7.9.6 MMC_MoveCircularAbsoluteBorder](../chunks/025_p0620-p0656_7.8.5-Online-Splines.md#pdf-page-651)
- 기능 설명: 이동 원호 절대 Border 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveCircularAbsoluteBorderCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVECIRCULARABSOLUTEBORDER_IN* pInParam,
OUT MMC_MOVECIRCULARABSOLUTE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVECIRCULARABSOLUTEBORDER_IN`
| 필드 | 해석 |
|---|---|
| `double dBorderPoint[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `double dEndPoint[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucSuperimposed;` | uc Superimposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_MOVECIRCULARABSOLUTE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.7 MMC_MoveCircularAbsoluteRadius

- PDF 페이지: 657
- 원문 위치: [7.9.7 MMC_MoveCircularAbsoluteRadius](../chunks/026_p0657-p0693_7.9.7-MMC_MoveCircularAbsoluteRadius.md#pdf-page-657)
- 기능 설명: 이동 원호 절대 Radius 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveCircularAbsoluteRadiusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVECIRCULARABSOLUTERADIUS_IN* pInParam,
OUT MMC_MOVECIRCULARABSOLUTE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVECIRCULARABSOLUTE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.8 MMC_MoveCircularAbsoluteAngle

- PDF 페이지: 664
- 원문 위치: [7.9.8 MMC_MoveCircularAbsoluteAngle](../chunks/026_p0657-p0693_7.9.7-MMC_MoveCircularAbsoluteRadius.md#pdf-page-664)
- 기능 설명: 이동 원호 절대 Angle 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveCircularAbsoluteAngleCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVECIRCULARABSOLUTEANGLE_IN* pInParam,
OUT MMC_MOVECIRCULARABSOLUTE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVECIRCULARABSOLUTE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.9 MMC_MoveAngle

- PDF 페이지: 670
- 원문 위치: [7.9.9 MMC_MoveAngle](../chunks/026_p0657-p0693_7.9.7-MMC_MoveCircularAbsoluteRadius.md#pdf-page-670)
- 기능 설명: 이동 Angle 작업을 수행하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_MoveAngle(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVEANGLE_IN* pInParam,
OUT MMC_MOVEANGLE_OUT* pOutParam);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.9.10 MMC_MoveLinearAbsolute

- PDF 페이지: 675
- 원문 위치: [7.9.10 MMC_MoveLinearAbsolute](../chunks/026_p0657-p0693_7.9.7-MMC_MoveCircularAbsoluteRadius.md#pdf-page-675)
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

#### 구조체/인자

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

### 7.9.11 MMC_MoveLinearRelative

- PDF 페이지: 682
- 원문 위치: [7.9.11 MMC_MoveLinearRelative](../chunks/026_p0657-p0693_7.9.7-MMC_MoveCircularAbsoluteRadius.md#pdf-page-682)
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

#### 구조체/인자

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

### 7.9.12 MMC_MoveLinearAdditive

- PDF 페이지: 689
- 원문 위치: [7.9.12 MMC_MoveLinearAdditive](../chunks/026_p0657-p0693_7.9.7-MMC_MoveCircularAbsoluteRadius.md#pdf-page-689)
- 기능 설명: 이동 선형 가산 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveLinearAdditiveCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVELINEARADDITIVE_IN* pInParam,
OUT MMC_MOVELINEARADDITIVE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVELINEARADDITIVE_IN`
| 필드 | 해석 |
|---|---|
| `double dbDistance[NC_MAX_NUM_AXES_IN_NODE];` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |

##### `MMC_MOVELINEARADDITIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.13 MMC_MoveLinearAdditiveEx

- PDF 페이지: 694
- 원문 위치: [7.9.13 MMC_MoveLinearAdditiveEx](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-694)
- 기능 설명: 이동 선형 가산 Ex 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveLinearAdditiveExCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVELINEARADDITIVEEX_IN* pInParam,
OUT MMC_MOVELINEARADDITIVEEX_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVELINEARADDITIVEEX_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `double dDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |

##### `MMC_MOVELINEARADDITIVEEX_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.14 MMC_MoveLinearAbsoluteRepetitive

- PDF 페이지: 699
- 원문 위치: [7.9.14 MMC_MoveLinearAbsoluteRepetitive](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-699)
- 기능 설명: 이동 선형 절대 반복 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveLinearAbsoluteRepetitiveCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVELINEARABSOLUTEREPETITIVE_IN* pInParam,
OUT MMC_MOVELINEARABSOLUTEREPETITIVE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVELINEARABSOLUTEREPETITIVE_IN`
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
| `unsigned int uiExecDelayMs;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned char ucSuperImposed;` | uc Super Imposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MOVELINEARABSOLUTEREPETITIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.15 MMC_MoveLinearRelativeRepetitive

- PDF 페이지: 705
- 원문 위치: [7.9.15 MMC_MoveLinearRelativeRepetitive](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-705)
- 기능 설명: 이동 선형 상대 반복 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

##### `MMC_MOVELINEARRELATIVEREPETITIVE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbDistance[NC_MAX_NUM_AXES_IN_NODE];` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned int uiExecDelayMs;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `unsigned char ucSuperImposed;` | uc Super Imposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MOVELINEARRELATIVEREPETITIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.16 MMC_MovePolynomAbsolute

- PDF 페이지: 710
- 원문 위치: [7.9.16 MMC_MovePolynomAbsolute](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-710)
- 기능 설명: 이동 Polynom 절대 작업을 수행하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_MovePolynomAbsoluteCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVEPOLYNOMABSOLUTE_IN* pInParam,
OUT MMC_MOVEPOLYNOMABSOLUTE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVEPOLYNOMABSOLUTE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbAuxPoint[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `double dbEndPoint[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `double dVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `double dAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `double dDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `double dJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_MOVEPOLYNOMABSOLUTE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.17 MMC_PathSelect

- PDF 페이지: 714
- 원문 위치: [7.9.17 MMC_PathSelect](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-714)
- 기능 설명: 값 또는 상태를 읽는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_PathSelectCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_PATHSELECT_IN* pInParam,
OUT MMC_PATHSELECT_OUT* pOutParam
);
```
```c
reserved memory. In other words, this function prepares the trajectory for MMC_MovePathCmd().
Scope
All
```
```c
retval = MMC_PathSelectCmd(g_conn_hndl, g_vect_ref[vect_idx],
&stPathSelectIn, &stPathSelectOut);
```

#### 구조체/인자

##### `MMC_PATHSELECT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `MC_PATH_DATA_REF pPathToSplineFile;` | 파일명, 경로, 이름 문자열입니다. |

##### `MMC_PATHSELECT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |

### 7.9.18 MMC_MovePath

- PDF 페이지: 717
- 원문 위치: [7.9.18 MMC_MovePath](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-717)
- 기능 설명: 이동 경로 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MovePathCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVEPATH_IN* pInParam,
OUT MMC_MOVEPATH_OUT* pOutParam
);
```
```c
retval = MMC_MovePathCmd(g_conn_hndl, g_vect_ref[vect_idx], &stMovePathIn,
&stMovePathOut);
```
```c
rc = MMC_PathSelectCmd(g_hConnectHandle, g_hVectorRef, &stPathSelectIn,
&stPathSelectOut);
```
```c
rc = MMC_MovePathCmd(g_hConnectHandle, g_hVectorRef, &stMovePathIn,
&stMovePathOut);
```
```c
rc = MMC_PathUnselectCmd(g_hConnectHandle, g_hVectorRef, &stPathUnselectIn,
&stPathUnselectOut);
```

#### 구조체/인자

##### `MMC_MOVEPATH_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `float fTransitionParameter[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_TRANSITION_MODE_ENUM eTransitionMode;` | 동작 모드 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned char ucSuperImposed;` | uc Super Imposed 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MOVEPATH_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.9.19 MMC_PathUnselect

- PDF 페이지: 724
- 원문 위치: [7.9.19 MMC_PathUnselect](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-724)
- 기능 설명: 경로 Unselect 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_PathUnselectCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_PATHUNSELECT_IN* pInParam,
OUT MMC_PATHUNSELECT_OUT* pOutParam
);
```
```c
retval = MMC_PathUnselectCmd(g_conn_hndl, g_vect_ref[vect_idx],
&stPathUnselectIn, &stPathUnselectOut);
```

#### 구조체/인자

##### `MMC_PATHUNSELECT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

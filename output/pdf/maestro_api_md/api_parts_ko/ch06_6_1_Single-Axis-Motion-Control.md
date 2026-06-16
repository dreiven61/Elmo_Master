# 6.1 Single Axis Motion Control - API 분석

- 원본 장: `Chapter 6 Motion and Administrative - Single Axis`
- 시작 PDF 페이지: 313
- 원문 위치: [6.1 Single Axis Motion Control](../chunks/016_p0313-p0350_6.1-Single-Axis-Motion-Control.md#pdf-page-313)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `6.1.1` | 315 | `MMC_HaltCmd` | 축을 정상 운전 조건에서 제어 정지시키는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.1.2` | 319 | `MMC_HomeCmd` | 축 홈 동작을 수행하는 API입니다. | Motion Mode NC - Not Supported Distributed - Supported |
| `6.1.3` | 335 | `MMC_HomeDS402Cmd` | DS-402 홈 동작을 수행하는 API입니다. | Motion Mode NC - Not Supported Distributed - Supported |
| `6.1.4` | 340 | `MMC_HomeDS402ExCmd` | 확장 DS-402 홈 동작을 수행하는 API입니다. | Motion Mode NC - Not Supported Distributed - Supported |
| `6.1.5` | 345 | `MMC_MoveAbsoluteCmd` | 지정한 절대 위치로 축을 이동시키는 모션 API입니다. | Motion Mode NC - All Buffering modes are supported. Distributed - Only MC_ABORTING_MODE Buffered mode is supported. |
| `6.1.6` | 351 | `MMC_MoveAdditiveCmd` | 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.1.7` | 356 | `MMC_MoveRelativeCmd` | 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.1.8` | 361 | `MMC_MoveVelocityCmd` | 지정 속도로 연속 이동을 수행하는 모션 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.1.9` | 366 | `MMC_MoveTorqueCmd` | 지정 토크로 연속 동작을 수행하는 모션 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.1.10` | 369 | `MMC_MoveContinuousCmd` | 연속 이동 명령을 수행하는 모션 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.1.11` | 373 | `MMC_MoveAbsoluteRepetitiveCmd` | 지정한 절대 위치로 축을 이동시키는 모션 API입니다. | Motion Mode NC - All Buffering modes are supported. Distributed - Not supported. |
| `6.1.12` | 378 | `MMC_MoveRelativeRepetitiveCmd` | 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `6.1.13` | 383 | `MMC_MoveAdditiveRepetitiveCmd` | 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `6.1.14` | 388 | `MMC_StopCmd` | 축 또는 동작을 정지 상태로 전환하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 6.1.1 MMC_Halt

- PDF 페이지: 315
- 원문 위치: [6.1.1 MMC_Halt](../chunks/016_p0313-p0350_6.1-Single-Axis-Motion-Control.md#pdf-page-315)
- 기능 설명: 축을 정상 운전 조건에서 제어 정지시키는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_HaltCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_HALT_IN* pInParam,
OUT MMC_HALT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_HALT_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_HALT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 6.1.2 MMC_Home

- PDF 페이지: 319
- 원문 위치: [6.1.2 MMC_Home](../chunks/016_p0313-p0350_6.1-Single-Axis-Motion-Control.md#pdf-page-319)
- 기능 설명: 축 홈 동작을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_HomeCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_HOME_IN* pInParam,
OUT MMC_HOME_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_HOME_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fDistanceLimit;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fTorqueLimit;` | 토크 관련 값입니다. |
| `MMC_HOME_MODE_ENUM eHomingMode;` | 홈 동작 방식 또는 홈 관련 파라미터입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `MC_HOME_DIRECTION_ENUM eDirection;` | 동작 방향 지정 값입니다. |
| `MC_SWITCH_MODE_ENUM eSwitchMode;` | 동작 모드 값입니다. |
| `unsigned int uiTimeLimit;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

##### `MMC_HOME_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.1.3 MMC_HomeDS402

- PDF 페이지: 335
- 원문 위치: [6.1.3 MMC_HomeDS402](../chunks/016_p0313-p0350_6.1-Single-Axis-Motion-Control.md#pdf-page-335)
- 기능 설명: DS-402 홈 동작을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Not Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_HomeDS402Cmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_HOMEDS402_IN* pInParam,
OUT MMC_HOME_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_HOMEDS402_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fDistanceLimit;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fTorqueLimit;` | 토크 관련 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `int uiHomingMethod;` | 홈 동작 방식 또는 홈 관련 파라미터입니다. |
| `unsigned int uiTimeLimit;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

##### `MMC_HOME_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.1.4 MMC_HomeDS402Ex

- PDF 페이지: 340
- 원문 위치: [6.1.4 MMC_HomeDS402Ex](../chunks/016_p0313-p0350_6.1-Single-Axis-Motion-Control.md#pdf-page-340)
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

#### 구조체/인자

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

### 6.1.5 MMC_MoveAbsolute

- PDF 페이지: 345
- 원문 위치: [6.1.5 MMC_MoveAbsolute](../chunks/016_p0313-p0350_6.1-Single-Axis-Motion-Control.md#pdf-page-345)
- 기능 설명: 지정한 절대 위치로 축을 이동시키는 모션 API입니다.
- 지원/모드: Motion Mode NC - All Buffering modes are supported. Distributed - Only MC_ABORTING_MODE Buffered mode is supported.

#### 시그니처

```c
MMC_LIB_API int MMC_MoveAbsoluteCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVEABSOLUTE_IN* pInParam,
OUT MMC_MOVEABSOLUTE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVEABSOLUTE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_DIRECTION_ENUM eDirection;` | 동작 방향 지정 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_MOVEABSOLUTE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.1.6 MMC_MoveAdditive

- PDF 페이지: 351
- 원문 위치: [6.1.6 MMC_MoveAdditive](../chunks/017_p0351-p0387_6.1.6-MMC_MoveAdditive.md#pdf-page-351)
- 기능 설명: 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveAdditiveCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVEADDITIVE_IN* pInParam,
OUT MMC_MOVEADDITIVE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVEADDITIVE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbDistance;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_DIRECTION_ENUM eDirection;` | 동작 방향 지정 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_MOVEADDITIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.1.7 MMC_MoveRelative

- PDF 페이지: 356
- 원문 위치: [6.1.7 MMC_MoveRelative](../chunks/017_p0351-p0387_6.1.6-MMC_MoveAdditive.md#pdf-page-356)
- 기능 설명: 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
int MMC_MoveRelativeCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVERELATIVE_IN* pInParam,
OUT MMC_MOVERELATIVE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVERELATIVE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbDistance;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_DIRECTION_ENUM eDirection;` | 동작 방향 지정 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_MOVERELATIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.1.8 MMC_MoveVelocity/MMC_MoveVelocityEx

- PDF 페이지: 361
- 원문 위치: [6.1.8 MMC_MoveVelocity/MMC_MoveVelocityEx](../chunks/017_p0351-p0387_6.1.6-MMC_MoveAdditive.md#pdf-page-361)
- 기능 설명: 지정 속도로 연속 이동을 수행하는 모션 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveVelocityCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVEVELOCITY_IN* pInParam,
OUT MMC_MOVEVELOCITY_OUT* pOutParam
);
```
```c
MMC_LIB_API int MMC_MoveVelocityExCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVEVELOCITYEX_IN* pInParam,
OUT MMC_MOVEVELOCITYEX_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVEVELOCITY_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_DIRECTION_ENUM eDirection;` | 동작 방향 지정 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_MOVEVELOCITY_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.1.9 MMC_MoveTorque

- PDF 페이지: 366
- 원문 위치: [6.1.9 MMC_MoveTorque](../chunks/017_p0351-p0387_6.1.6-MMC_MoveAdditive.md#pdf-page-366)
- 기능 설명: 지정 토크로 연속 동작을 수행하는 모션 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveTorqueCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVETORQUE_IN* pInParam,
OUT MMC_MOVETORQUE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVETORQUE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbTargetTorque;` | 토크 관련 값입니다. |
| `double dbTorquetVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `double dbTorqueAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_MOVETORQUE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 6.1.10 MMC_MoveContinuous

- PDF 페이지: 369
- 원문 위치: [6.1.10 MMC_MoveContinuous](../chunks/017_p0351-p0387_6.1.6-MMC_MoveAdditive.md#pdf-page-369)
- 기능 설명: 연속 이동 명령을 수행하는 모션 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveContinuousCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVECONTINUOUS_IN* pInParam,
OUT MMC_MOVECONTINUOUS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVECONTINUOUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbDistance;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fEndVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_MOVECONTINUOUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.1.11 MMC_MoveAbsoluteRepetitive

- PDF 페이지: 373
- 원문 위치: [6.1.11 MMC_MoveAbsoluteRepetitive](../chunks/017_p0351-p0387_6.1.6-MMC_MoveAdditive.md#pdf-page-373)
- 기능 설명: 지정한 절대 위치로 축을 이동시키는 모션 API입니다.
- 지원/모드: Motion Mode NC - All Buffering modes are supported. Distributed - Not supported.

#### 시그니처

```c
MMC_LIB_API int MMC_MoveAbsoluteRepetitiveCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVEABSOLUTEREPETITIVE_IN* pInParam,
OUT MMC_MOVEABSOLUTEREPETITIVE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVEABSOLUTEREPETITIVE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_DIRECTION_ENUM eDirection;` | 동작 방향 지정 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned int uiExecDelayMs;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

##### `MMC_MOVEABSOLUTEREPETITIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.1.12 MMC_MoveRelativeRepetitive

- PDF 페이지: 378
- 원문 위치: [6.1.12 MMC_MoveRelativeRepetitive](../chunks/017_p0351-p0387_6.1.6-MMC_MoveAdditive.md#pdf-page-378)
- 기능 설명: 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveRelativeRepetitiveCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVERELATIVEREPETITIVE_IN* pInParam,
OUT MMC_MOVERELATIVEREPETITIVE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVERELATIVEREPETITIVE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbDistance;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_DIRECTION_ENUM eDirection;` | 동작 방향 지정 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned int uiExecDelayMs;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

##### `MMC_MOVERELATIVEREPETITIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.1.13 MMC_MoveAdditiveRepetitive

- PDF 페이지: 383
- 원문 위치: [6.1.13 MMC_MoveAdditiveRepetitive](../chunks/017_p0351-p0387_6.1.6-MMC_MoveAdditive.md#pdf-page-383)
- 기능 설명: 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_MoveAdditiveRepetitiveCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOVEADDITIVEREPETITIVE_IN* pInParam,
OUT MMC_MOVEADDITIVEREPETITIVE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOVEADDITIVEREPETITIVE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbDistance;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_DIRECTION_ENUM eDirection;` | 동작 방향 지정 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned int uiExecDelayMs;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

##### `MMC_MOVEADDITIVEREPETITIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.1.14 MMC_Stop

- PDF 페이지: 388
- 원문 위치: [6.1.14 MMC_Stop](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-388)
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

#### 구조체/인자

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

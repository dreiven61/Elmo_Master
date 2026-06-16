# 6.2 Single Axis Administrative Control - API 분석

- 원본 장: `Chapter 6 Motion and Administrative - Single Axis`
- 시작 PDF 페이지: 393
- 원문 위치: [6.2 Single Axis Administrative Control](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-393)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `6.2.1` | 394 | `SuperImposed Motion` | Super Imposed Motion 작업을 수행하는 API입니다. | - |
| `6.2.2` | 395 | `Special Function` | Special 함수 작업을 수행하는 API입니다. | - |
| `6.2.3` | 396 | `MMC_AxisLink` | 축 Link 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.4` | 399 | `MMC_AxisUnLink` | 축 Un Link 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.5` | 401 | `MMC_KillMotion` | Kill Motion 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.6` | 403 | `MMC_KillRepetitive` | Kill 반복 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.7` | 405 | `MMC_DwellCmd` | Dwell 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.8` | 407 | `MMC_GetFbDepthCmd` | 조회 Fb 깊이 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.9` | 410 | `MMC_MarkFbFree` | Mark Fb Free 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.10` | 413 | `MMC_GetTotalFbDepthCmd` | 조회 Total Fb 깊이 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.11` | 416 | `MMC_PowerCmd` | 전원 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.12` | 420 | `MMC_PositionProfileCmd` | 위치 프로파일 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.13` | 423 | `MMC_ReadActualPositionCmd` | 읽기 실제 위치 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.14` | 426 | `MMC_ReadActualTorqueCmd` | 읽기 실제 토크 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.15` | 429 | `MMC_ReadActualVelocityCmd` | 읽기 실제 속도 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.16` | 432 | `MMC_ReadAxisError` | 읽기 축 오류 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.17` | 435 | `MMC_ReadBoolParameter` | 읽기 불리언 파라미터 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.18` | 438 | `MMC_GlobalReadBoolParameter` | 전역 읽기 불리언 파라미터 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.19` | 441 | `MMC_ReadDigitalInputCmd` | 읽기 디지털 입력 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.20` | 445 | `MMC_ReadDigitalOutputs` | 읽기 디지털 출력 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.21` | 448 | `MMC_ReadDigitalOutputs32Bit` | 읽기 디지털 Outputs32 Bit 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.22` | 451 | `MMC_ReadParameter` | 읽기 파라미터 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.23` | 454 | `MMC_GlobalReadParameter` | 전역 읽기 파라미터 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.24` | 457 | `MMC_ReadStatusCmd` | 읽기 상태 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.25` | 460 | `MMC_Reset` | 리셋 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.26` | 462 | `MMC_ResetAsync` | 리셋 비동기 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.27` | 464 | `MMC_SetOverrideCmd` | 설정 오버라이드 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.28` | 467 | `MMC_SetPositionCmd` | 설정 위치 값/설정을 적용하는 API입니다. | Motion Mode NC - Not Supported Distributed - Supported |
| `6.2.29` | 469 | `MMC_TouchProbeEnableCmd` | 터치 프로브 활성화 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.30` | 471 | `MMC_TouchProbeDisableCmd` | 터치 프로브 비활성화 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.31` | 473 | `MMC_WriteBoolParameter` | 쓰기 불리언 파라미터 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.32` | 476 | `MMC_GlobalWriteBoolParameter` | 전역 쓰기 불리언 파라미터 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.33` | 479 | `MMC_WriteDigitalOutputs` | 쓰기 디지털 출력 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.34` | 482 | `MMC_WriteDigitalOutputs32Bit` | 쓰기 디지털 Outputs32 Bit 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.35` | 485 | `MMC_WriteParameter` | 쓰기 파라미터 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.36` | 488 | `MMC_GlobalWriteParameter` | 전역 쓰기 파라미터 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.37` | 491 | `MMC_ChngOpMode` | Chng Op 모드 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.38` | 494 | `MMC_ChangeOpModeEx` | Change Op 모드 Ex 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.39` | 497 | `MMC_SetProfileConditioning` | 설정 프로파일 컨디셔닝 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `6.2.40` | 500 | `MMC_GetProfileConditioning` | 조회 프로파일 컨디셔닝 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 6.2.1 SuperImposed Motion

- PDF 페이지: 394
- 원문 위치: [6.2.1 SuperImposed Motion](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-394)
- 기능 설명: Super Imposed Motion 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 6.2.2 Special Function

- PDF 페이지: 395
- 원문 위치: [6.2.2 Special Function](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-395)
- 기능 설명: Special 함수 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 6.2.3 MMC_AxisLink

- PDF 페이지: 396
- 원문 위치: [6.2.3 MMC_AxisLink](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-396)
- 기능 설명: 축 Link 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_AxisLink(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_AXISLINK_IN* pInParam,
OUT MMC_AXISLINK_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_AXISLINK_IN`
| 필드 | 해석 |
|---|---|
| `unsigned long ulInputParameter1;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned long ulInputParameter2;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned long ulInputParameter3;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned long ulInputParameter4;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `unsigned short usSlaveAxisReference;` | 축 식별 또는 축 관련 값입니다. |
| `unsigned char ucMode;` | 동작 모드 값입니다. |

##### `MMC_AXISLINK_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.4 MMC_AxisUnLink

- PDF 페이지: 399
- 원문 위치: [6.2.4 MMC_AxisUnLink](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-399)
- 기능 설명: 축 Un Link 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_AxisUnLink(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_AXISUNLINK_IN* pInParam,
OUT MMC_AXISUNLINK_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_AXISUNLINK_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucdummy;` | ucdummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_AXISUNLINK_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.5 MMC_KillMotion

- PDF 페이지: 401
- 원문 위치: [6.2.5 MMC_KillMotion](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-401)
- 기능 설명: Kill Motion 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_KillMotion(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_KILLMOTION_IN* i_param,
OUT MMC_KILLMOTION_OUT* o_param);
```

#### 구조체/인자

##### `MMC_KILLMOTION_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiDummy;` | ui Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_KILLMOTION_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 6.2.6 MMC_KillRepetitive

- PDF 페이지: 403
- 원문 위치: [6.2.6 MMC_KillRepetitive](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-403)
- 기능 설명: Kill 반복 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

##### `MMC_KILLREPETITIVE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiDummy;` | ui Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_KILLREPETITIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 6.2.7 MMC_Dwell

- PDF 페이지: 405
- 원문 위치: [6.2.7 MMC_Dwell](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-405)
- 기능 설명: Dwell 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_DwellCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_DWELL_IN* pInParam,
OUT MMC_DWELL_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_DWELL_IN`
| 필드 | 해석 |
|---|---|
| `unsigned long ulDwellTimeMs;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

##### `MMC_DWELL_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHandle;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.8 MMC_GetFBDepth

- PDF 페이지: 407
- 원문 위치: [6.2.8 MMC_GetFBDepth](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-407)
- 기능 설명: 조회 Fb 깊이 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetFbDepthCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETFBDEPTH_IN* pInParam,
OUT MMC_GETFBDEPTH_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETFBDEPTH_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |

##### `MMC_GETFBDEPTH_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiFbInQ;` | ui Fb In Q 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.9 MMC_MarkFbFree

- PDF 페이지: 410
- 원문 위치: [6.2.9 MMC_MarkFbFree](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-410)
- 기능 설명: Mark Fb Free 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MarkFbFree(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MARKFBFREE_IN* pInParam,
OUT MMC_MARKFBFREE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MARKFBFREE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |

##### `MMC_MARKFBFREE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 6.2.10 MMC_GetTotalFbDepth

- PDF 페이지: 413
- 원문 위치: [6.2.10 MMC_GetTotalFbDepth](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-413)
- 기능 설명: 조회 Total Fb 깊이 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetTotalFbDepthCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETFBDEPTH_IN* pInParam,
OUT MMC_GETFBDEPTH_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GETFBDEPTH_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |

##### `MMC_GETFBDEPTH_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiFbInQ;` | ui Fb In Q 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.11 MMC_Power

- PDF 페이지: 416
- 원문 위치: [6.2.11 MMC_Power](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-416)
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

#### 구조체/인자

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

### 6.2.12 MMC_PositionProfile

- PDF 페이지: 420
- 원문 위치: [6.2.12 MMC_PositionProfile](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-420)
- 기능 설명: 위치 프로파일 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_PositionProfileCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_POSITIONPROFILE_IN* pInParam,
OUT MMC_POSITIONPROFILE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_POSITIONPROFILE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `MC_PATH_REF hMemHandle;` | 함수 블록 또는 리소스 핸들입니다. |

##### `MMC_POSITIONPROFILE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 6.2.13 MMC_ReadActualPosition

- PDF 페이지: 423
- 원문 위치: [6.2.13 MMC_ReadActualPosition](../chunks/018_p0388-p0425_6.1.14-MMC_Stop.md#pdf-page-423)
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

#### 구조체/인자

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

### 6.2.14 MMC_ReadActualTorque

- PDF 페이지: 426
- 원문 위치: [6.2.14 MMC_ReadActualTorque](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-426)
- 기능 설명: 읽기 실제 토크 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadActualTorqueCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READACTUALTORQUE_IN* pInParam,
OUT MMC_READACTUALTORQUE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READACTUALTORQUE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READACTUALTORQUE_OUT`
| 필드 | 해석 |
|---|---|
| `double dActualTorque;` | 토크 관련 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucValid;` | 전달하거나 반환받는 값입니다. |

### 6.2.15 MMC_ReadActualVelocity

- PDF 페이지: 429
- 원문 위치: [6.2.15 MMC_ReadActualVelocity](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-429)
- 기능 설명: 읽기 실제 속도 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadActualVelocityCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READACTUALVELOCITY_IN* pInParam,
OUT MMC_READACTUALVELOCITY_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READACTUALVELOCITY_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READACTUALVELOCITY_OUT`
| 필드 | 해석 |
|---|---|
| `double dVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.16 MMC_ReadAxisError

- PDF 페이지: 432
- 원문 위치: [6.2.16 MMC_ReadAxisError](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-432)
- 기능 설명: 읽기 축 오류 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadAxisError(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READAXISERROR_IN* pInParam,
OUT MMC_READAXISERROR_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READAXISERROR_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READAXISERROR_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usAxisErrorID;` | 오류 ID입니다. |
| `unsigned short usLastEmergencyErrCode;` | 오류 ID입니다. |

### 6.2.17 MMC_ReadBoolParameter

- PDF 페이지: 435
- 원문 위치: [6.2.17 MMC_ReadBoolParameter](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-435)
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

#### 구조체/인자

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

### 6.2.18 MMC_GlobalReadBoolParameter

- PDF 페이지: 438
- 원문 위치: [6.2.18 MMC_GlobalReadBoolParameter](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-438)
- 기능 설명: 전역 읽기 불리언 파라미터 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GlobalReadBoolParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_READBOOLPARAMETER_IN* pInParam,
OUT MMC_READBOOLPARAMETER_OUT* pOutParam
);
```

#### 구조체/인자

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

### 6.2.19 MMC_ReadDigitalInput(s)

- PDF 페이지: 441
- 원문 위치: [6.2.19 MMC_ReadDigitalInput(s)](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-441)
- 기능 설명: 읽기 디지털 입력 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadDigitalInputCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READDIGITALINPUT_IN* pInParam,
OUT MMC_READDIGITALINPUT_OUT* pOutParam
);
```
```c
MMC_LIB_API int MMC_ReadDigitalInputsCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READDIGITALINPUTS_IN* pInParam,
OUT MMC_READDIGITALINPUTS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READDIGITALINPUT_IN`
| 필드 | 해석 |
|---|---|
| `int iInputNumber;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READDIGITALINPUT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucValue;` | 전달하거나 반환받는 값입니다. |

##### `MMC_READDIGITALINPUTS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READDIGITALINPUTS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long ulValue;` | 전달하거나 반환받는 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.20 MMC_ReadDigitalOutputs

- PDF 페이지: 445
- 원문 위치: [6.2.20 MMC_ReadDigitalOutputs](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-445)
- 기능 설명: 읽기 디지털 출력 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadDigitalOutputs(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READDIGITALOUTPUT_IN* pInParam,
OUT MMC_READDIGITALOUTPUT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READDIGITALOUTPUT_IN`
| 필드 | 해석 |
|---|---|
| `int iOutputNumber;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READDIGITALOUTPUT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucValue;` | 전달하거나 반환받는 값입니다. |

### 6.2.21 MMC_ReadDigitalOutputs32Bit

- PDF 페이지: 448
- 원문 위치: [6.2.21 MMC_ReadDigitalOutputs32Bit](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-448)
- 기능 설명: 읽기 디지털 Outputs32 Bit 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ReadDigitalOutputs32Bit(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READDIGITALOUTPUT32Bit_IN*pInParam,
OUT MMC_READDIGITALOUTPUT32Bit_OUT*pOutParam
);
```

#### 구조체/인자

##### `MMC_READDIGITALOUTPUT32Bit_IN`
| 필드 | 해석 |
|---|---|
| `int iOutputNumber;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READDIGITALOUTPUT32Bit_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |
| `unsigned long ulValue;` | 전달하거나 반환받는 값입니다. |

### 6.2.22 MMC_ReadParameter

- PDF 페이지: 451
- 원문 위치: [6.2.22 MMC_ReadParameter](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-451)
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

#### 구조체/인자

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

### 6.2.23 MMC_GlobalReadParameter

- PDF 페이지: 454
- 원문 위치: [6.2.23 MMC_GlobalReadParameter](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-454)
- 기능 설명: 전역 읽기 파라미터 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GlobalReadParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_READPARAMETER_IN* pInParam,
OUT MMC_READPARAMETER_OUT* pOutParam
);
```

#### 구조체/인자

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

### 6.2.24 MMC_ReadStatus

- PDF 페이지: 457
- 원문 위치: [6.2.24 MMC_ReadStatus](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-457)
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

#### 구조체/인자

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

### 6.2.25 MMC_Reset

- PDF 페이지: 460
- 원문 위치: [6.2.25 MMC_Reset](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-460)
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

#### 구조체/인자

##### `MMC_RESET_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_RESET_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.26 MMC_ResetAsync

- PDF 페이지: 462
- 원문 위치: [6.2.26 MMC_ResetAsync](../chunks/019_p0426-p0463_6.2.14-MMC_ReadActualTorque.md#pdf-page-462)
- 기능 설명: 리셋 비동기 작업을 수행하는 API입니다.
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

#### 구조체/인자

##### `MMC_RESET_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_RESET_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.27 MMC_SetOverride

- PDF 페이지: 464
- 원문 위치: [6.2.27 MMC_SetOverride](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-464)
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

#### 구조체/인자

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

### 6.2.28 MMC_SetPosition

- PDF 페이지: 467
- 원문 위치: [6.2.28 MMC_SetPosition](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-467)
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

#### 구조체/인자

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

### 6.2.29 MMC_TouchProbeEnable

- PDF 페이지: 469
- 원문 위치: [6.2.29 MMC_TouchProbeEnable](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-469)
- 기능 설명: 터치 프로브 활성화 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_TouchProbeEnableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_TOUCHPROBEENABLE_IN*pInParam,
OUT MMC_TOUCHPROBEENABLE_OUT*pOutParam
);
```

#### 구조체/인자

##### `MMC_TOUCHPROBEENABLE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `unsigned char ucTriggerType;` | 데이터 또는 동작 타입 값입니다. |

##### `MMC_TOUCHPROBEENABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.30 MMC_TouchProbeDisable

- PDF 페이지: 471
- 원문 위치: [6.2.30 MMC_TouchProbeDisable](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-471)
- 기능 설명: 터치 프로브 비활성화 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_TouchProbeDisableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_TOUCHPROBEDISABLE_IN* pInParam,
OUT MMC_TOUCHPROBEDISABLE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_TOUCHPROBEDISABLE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_TOUCHPROBEDISABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.31 MMC_WriteBoolParameter

- PDF 페이지: 473
- 원문 위치: [6.2.31 MMC_WriteBoolParameter](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-473)
- 기능 설명: 쓰기 불리언 파라미터 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WriteBoolParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEBOOLPARAMETER_IN* pInParam,
OUT MMC_WRITEBOOLPARAMETER_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEBOOLPARAMETER_IN`
| 필드 | 해석 |
|---|---|
| `long lValue;` | 전달하거나 반환받는 값입니다. |
| `MMC_PARAMETER_LIST_ENUM eParameterNumber;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `int iParameterArrIndex;` | 인덱스 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_WRITEBOOLPARAMETER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.32 MMC_GlobalWriteBoolParameter

- PDF 페이지: 476
- 원문 위치: [6.2.32 MMC_GlobalWriteBoolParameter](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-476)
- 기능 설명: 전역 쓰기 불리언 파라미터 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GlobalWriteBoolParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_WRITEBOOLPARAMETER_IN* pInParam,
OUT MMC_WRITEBOOLPARAMETER_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEBOOLPARAMETER_IN`
| 필드 | 해석 |
|---|---|
| `long lValue;` | 전달하거나 반환받는 값입니다. |
| `MMC_PARAMETER_LIST_ENUM eParameterNumber;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `int iParameterArrIndex;` | 인덱스 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_WRITEBOOLPARAMETER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.33 MMC_WriteDigitalOutputs

- PDF 페이지: 479
- 원문 위치: [6.2.33 MMC_WriteDigitalOutputs](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-479)
- 기능 설명: 쓰기 디지털 출력 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WriteDigitalOutputs(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEDIGITALOUTPUT_IN* pInParam,
OUT MMC_WRITEDIGITALOUTPUT_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEDIGITALOUTPUT_IN`
| 필드 | 해석 |
|---|---|
| `int iOutputNumber;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |
| `unsigned char ucValue;` | 전달하거나 반환받는 값입니다. |

##### `MMC_WRITEDIGITALOUTPUT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.34 MMC_WriteDigitalOutputs32Bit

- PDF 페이지: 482
- 원문 위치: [6.2.34 MMC_WriteDigitalOutputs32Bit](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-482)
- 기능 설명: 쓰기 디지털 Outputs32 Bit 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_WriteDigitalOutputs32Bit(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEDIGITALOUTPUT32Bit_IN* pInParam,
OUT MMC_WRITEDIGITALOUTPUT32Bit_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEDIGITALOUTPUT32Bit_IN`
| 필드 | 해석 |
|---|---|
| `int iOutputNumber;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned long ulValue;` | 전달하거나 반환받는 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_WRITEDIGITALOUTPUT32Bit_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

### 6.2.35 MMC_WriteParameter

- PDF 페이지: 485
- 원문 위치: [6.2.35 MMC_WriteParameter](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-485)
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

#### 구조체/인자

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

### 6.2.36 MMC_GlobalWriteParameter

- PDF 페이지: 488
- 원문 위치: [6.2.36 MMC_GlobalWriteParameter](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-488)
- 기능 설명: 전역 쓰기 파라미터 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GlobalWriteParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_WRITEPARAMETER_IN* pInParam,
OUT MMC_WRITEPARAMETER_OUT* pOutParam
);
```

#### 구조체/인자

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

### 6.2.37 MMC_ChngOpMode

- PDF 페이지: 491
- 원문 위치: [6.2.37 MMC_ChngOpMode](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-491)
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

#### 구조체/인자

##### `MMC_CHANGEMOTIONMODE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucMotionMode;` | 동작 모드 값입니다. |

##### `MMC_CHANGEMOTIONMODE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 6.2.38 MMC_ChangeOpModeEx

- PDF 페이지: 494
- 원문 위치: [6.2.38 MMC_ChangeOpModeEx](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-494)
- 기능 설명: Change Op 모드 Ex 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ChangeOpModeEx(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_CHANGEOPMODE_EX_IN* pInParam,
OUT MMC_CHANGEOPMODE_EX_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_CHANGEOPMODE_EX_IN`
| 필드 | 해석 |
|---|---|
| `double dbInitModeValue;` | 전달하거나 반환받는 값입니다. |
| `MC_EXECUTION_MODE eExecutionMode;` | 동작 모드 값입니다. |
| `unsigned char ucMotionMode;` | 동작 모드 값입니다. |
| `unsigned char ucSpare[19];` | uc Spare[19] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_CHANGEOPMODE_EX_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short usErrorID;` | 오류 ID입니다. |

### 6.2.39 MMC_SetProfileConditioning

- PDF 페이지: 497
- 원문 위치: [6.2.39 MMC_SetProfileConditioning](../chunks/020_p0464-p0499_6.2.27-MMC_SetOverride.md#pdf-page-497)
- 기능 설명: 설정 프로파일 컨디셔닝 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetProfileConditioning(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_PROFILECOND_IN* i_params,
OUT MMC_PROFILECOND_OUT* o_params
);
```

#### 구조체/인자

##### `MMC_PROFILECOND_IN`
| 필드 | 해석 |
|---|---|
| `Double freq[2];` | freq[2] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `Double damp[2];` | damp[2] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char freqs_num;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char impls_num[2];` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char enable;` | 활성화/비활성화 제어 값입니다. |
| `unsigned char spare[32];` | spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_PROFILECOND_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 6.2.40 MMC_GetProfileConditioning

- PDF 페이지: 500
- 원문 위치: [6.2.40 MMC_GetProfileConditioning](../chunks/021_p0500-p0503_6.2.40-MMC_GetProfileConditioning.md#pdf-page-500)
- 기능 설명: 조회 프로파일 컨디셔닝 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetProfileConditioning(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_PROFCONDINF_IN* i_params,
OUT MMC_PROFCONDINF_OUT* o_params
);
```

#### 구조체/인자

##### `MMC_PROFCONDINF_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucdummy;` | ucdummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_PROFCONDINF_OUT`
| 필드 | 해석 |
|---|---|
| `MMC_PROFILECOND_DAT data[3];` | 데이터 버퍼 또는 데이터 값입니다. |
| `int size;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |
| `unsigned char spare[32];` | spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

# 7.7 Multiple Axes Motion Control - Normalcy Modes - API 분석

- 원본 장: `Chapter 7 Motion and Administrative - Multi-Axis`
- 시작 PDF 페이지: 596
- 원문 위치: [7.7 Multiple Axes Motion Control - Normalcy Modes](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-596)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `7.7.1` | 597 | `Modes of Operation` | Modes of 동작 작업을 수행하는 API입니다. | - |
| `7.7.2` | 599 | `Normalcy Mode Functions` | Normalcy 모드 Functions 작업을 수행하는 API입니다. | - |
| `7.7.3` | 600 | `MMC_SetNormalcyMode` | 설정 Normalcy 모드 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.7.4` | 604 | `MMC_SetNormalcyOff` | 설정 Normalcy Off 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.7.5` | 606 | `MMC_GetNormalcyMode` | 조회 Normalcy 모드 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 7.7.1 Modes of Operation

- PDF 페이지: 597
- 원문 위치: [7.7.1 Modes of Operation](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-597)
- 기능 설명: Modes of 동작 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.7.2 Normalcy Mode Functions

- PDF 페이지: 599
- 원문 위치: [7.7.2 Normalcy Mode Functions](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-599)
- 기능 설명: Normalcy 모드 Functions 작업을 수행하는 API입니다.

#### 시그니처

```c
void SetNormalcyMode(NormalcyType normalcyType, NormalcyPlane normalcyPlane);
void SetNormalcyOff();
```
```c
void GetNormalcyMode(out NormalcyType normalcyType, out NormalcyPlane
normalcyPlane);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.7.3 MMC_SetNormalcyMode

- PDF 페이지: 600
- 원문 위치: [7.7.3 MMC_SetNormalcyMode](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-600)
- 기능 설명: 설정 Normalcy 모드 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetNormalcyMode(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_NORMALCY_PARAMS_IN* i_params,
OUT MMC_NORMALCY_PARAMS_OUT* o_params
);
```
```c
MMC_LIB_API int MMC_SetNormalcyMode(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_NORMALCY_PARAMS_IN* i_params,
OUT MMC_NORMALCY_PARAMS_OUT* o_params)
{
int rc = 0;
MMC_WRITEGROUPOFPARAMETERS_IN i_wgparams;
MMC_WRITEGROUPOFPARAMETERS_OUT o_wgparams;
i_wgparams.eExecutionMode = eMMC_EXECUTION_MODE_IMMEDIATE;
i_wgparams.ucNumberOfParameters = 2;
i_wgparams.ucExecute = 1;
i_wgparams.ucMode = 0; //not in use
// the setting order is from last in the list down to the first.
*/
i_wgparams.sParameters[1].dbValue = (double)i_params->ePlane;
i_wgparams.sParameters[1].eParameterNumber = MMC_NORMALCY_COORDS;
i_wgparams.sParameters[1].iParameterIndex = 0;
i_wgparams.sParameters[1].usAxisRef = hAxisRef; //the group reference.
may be another node as well.
i_wgparams.sParameters[0].dbValue = (double)i_params->eType;
i_wgparams.sParameters[0].eParameterNumber = MMC_NORMALCY_OP_MODE;
i_wgparams.sParameters[0].iParameterIndex = 0;
i_wgparams.sParameters[0].usAxisRef = hAxisRef; //the group reference.
may be another node as well.
if ((rc = MMC_WriteGroupOfParameters(hConn, hAxisRef, &i_wgparams,
&o_wgparams)) < 0)
fprintf(stderr, "%s: error %d\n", __func__, o_params->sErrorID);
```

#### 구조체/인자

##### `MMC_NORMALCY_PARAMS_IN`
| 필드 | 해석 |
|---|---|
| `MMC_NORMALCY_TYPE_ENUM eType;` | 데이터 또는 동작 타입 값입니다. |
| `MMC_NORMALCY_PLANE_ENUM ePlane;` | e Plane 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_NORMALCY_PARAMS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.7.4 MMC_SetNormalcyOff

- PDF 페이지: 604
- 원문 위치: [7.7.4 MMC_SetNormalcyOff](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-604)
- 기능 설명: 설정 Normalcy Off 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetNormalcyOff(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
OUT MMC_NORMALCY_PARAMS_OUT* o_params
);
```

#### 구조체/인자

##### `MMC_NORMALCY_PARAMS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.7.5 MMC_GetNormalcyMode

- PDF 페이지: 606
- 원문 위치: [7.7.5 MMC_GetNormalcyMode](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-606)
- 기능 설명: 조회 Normalcy 모드 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetNormalcyMode(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
OUT MMC_NORMALCY_STAUS_OUT* o_params
);
```

#### 구조체/인자

##### `MMC_NORMALCY_STATUS_OUT`
| 필드 | 해석 |
|---|---|
| `MMC_NORMALCY_TYPE_ENUM eType;` | 데이터 또는 동작 타입 값입니다. |
| `MMC_NORMALCY_PLANE_ENUM ePlane;` | e Plane 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

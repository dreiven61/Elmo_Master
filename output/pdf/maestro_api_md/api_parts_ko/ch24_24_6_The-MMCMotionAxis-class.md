# 24.6 The MMCMotionAxis class - API 분석

- 원본 장: `Chapter 24 Programming in C++`
- 시작 PDF 페이지: 1993
- 원문 위치: [24.6 The MMCMotionAxis class](../chunks/077_p1993-p2032_24.6-The-MMCMotionAxis-class.md#pdf-page-1993)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `24.6` | 1993 | `MMC_InitTableCmd` | 초기화 테이블 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 24.6 The MMCMotionAxis class

- PDF 페이지: 1993
- 원문 위치: [24.6 The MMCMotionAxis class](../chunks/077_p1993-p2032_24.6-The-MMCMotionAxis-class.md#pdf-page-1993)
- 기능 설명: 초기화 테이블 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
InitPVTTable The method is a wrapper for the MMC_InitTableCmd() C command.
InitTableCmd().
LoadPVTTable The method loads PVT table from file
AppendPointsToPVTTable This method appends points to the current PVT table (in automatic mode).
AppendPointsToPVTTable This method appends points to the current PVT table (in manual mode).
MovePVT This method inserts PVT function block.
UnloadPVTTable Unloads PVT table
Similarly, the C++ ECAM functions wrap the motion axis parameter reading functions detailed in Chapter 9
Electronic CAM and retains similar field parameter properties and values described in this document for the C
function blocks. The ECAM contains the following methods:
Function Explanation
CamGetStatus Retrieves the significant parameters of the CAM process
CamIn Executes the CAM process
CamOut Performs a Stop function procedure on the slave axis
CamTableAdd Appends points to an existing table
CamTableInit Allocates memory for the ECAM table, prepares and initializes the function
block in journal
CamTablePrint Ouputs the CAM Table data to a prepared device
CamTableSelect Selects a table by input handler
CamTableSet Used for loading a table from memory
CamTableUnload Unloads a ECAM table from the Maestro and frees a memory segment in the
Maestro shared memory according to the dimension and number of points
given in a file
24.6.1 MMCMotionAxis Source Code Examples
eTableType = eNC_TABLE_PVT_ARRAY;
ucIsCyclic = 1;
ucIsDynamicMode = 1;
ucIsPosAbsolute = 0;
ulMaxNumberOfPoints = 50;
```
```c
MMC_LIB_API int MMC_MarkFbFree(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MARKFBFREE_IN* pInParam,
OUT MMC_MARKFBFREE_OUT* pOutParam
);
```
```c
unsigned int GetFbDepth(
const unsigned int uiHndl
)throw (CMMCException);
```
```c
void EnableMotionEndedEvent(
) throw (CMMCException);
```
```c
void DisableMotionEndedEvent(
) throw (CMMCException);
```
```c
MMC_LIB_API int MMC_KillMotion(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_KILLMOTION_IN* i_param,
OUT MMC_KILLMOTION_OUT* o_param);
```
```c
void Dwell(
unsigned long ulDwellTimeMs
) throw (CMMCException);
```
```c
Python Definition def MMC_DwellCmd(hConn, hAxisRef, pInParam, pOutParam):
return _mmcpp_lib.MMC_DwellCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_DWELL_IN(object):
ulDwellTimeMs =
property(_mmcpp_lib.MMC_DWELL_IN_ulDwellTimeMs_get,
_mmcpp_lib.MMC_DWELL_IN_ulDwellTimeMs_set)
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

##### `MMC_KILLMOTION_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiDummy;` | ui Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_KILLMOTION_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

##### `MMC_KILLREPETITIVE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiDummy;` | ui Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_KILLREPETITIVE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

##### `MMC_KILLMOTION_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiDummy;` | ui Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_KILLMOTION_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short sErrorID;` | 오류 ID입니다. |

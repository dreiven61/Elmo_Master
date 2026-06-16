# 24.4 The MMCNode class - API 분석

- 원본 장: `Chapter 24 Programming in C++`
- 시작 PDF 페이지: 1925
- 원문 위치: [24.4 The MMCNode class](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1925)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `24.4.2` | 1942 | `ConfigPDOEventMode` | 구성 PDOEvent 모드 작업을 수행하는 API입니다. | - |
| `24.4.3` | 1943 | `EtherCATPIVarInfo` | Ether CATPIVar 정보 작업을 수행하는 API입니다. | - |
| `24.4.4` | 1945 | `EtherCATReadMemoryRange` | Ether CATRead 메모리 범위 값/상태를 조회하는 API입니다. | - |
| `24.4.5` | 1946 | `EtherCATWriteMemoryRange` | Ether CATWrite 메모리 범위 값/설정을 적용하는 API입니다. | - |
| `24.4.6` | 1947 | `EtherCATReadPIVar` | Ether CATRead PI 변수 값/상태를 조회하는 API입니다. | - |
| `24.4.7` | 1952 | `EtherCATWritePIVar` | Ether CATWrite PI 변수 값/설정을 적용하는 API입니다. | - |
| `24.4.8` | 1957 | `Reset` | 리셋 작업을 수행하는 API입니다. | - |
| `24.4.9` | 1958 | `MMC_ReadStatusCmd` | 읽기 상태 값/상태를 조회하는 API입니다. | - |
| `24.4.10` | 1960 | `SendSdoCmd` | 전송 Sdo 작업을 수행하는 API입니다. | - |
| `24.4.11` | 1962 | `SendSdoCmd` | 전송 Sdo 작업을 수행하는 API입니다. | - |
| `24.4.12` | 1963 | `MMC_RetrieveSdoAsyncCmd` | Retrieve Sdo 비동기 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `24.4.13` | 1966 | `SendSdoDownloadExCmd` | 전송 Sdo 다운로드 Ex 작업을 수행하는 API입니다. | - |
| `24.4.14` | 1968 | `SendSDOUpload` | 전송 SDOUpload 작업을 수행하는 API입니다. | - |
| `24.4.15` | 1969 | `SendSdoUploadExCmd` | 전송 Sdo 업로드 Ex 작업을 수행하는 API입니다. | - |
| `24.4.16` | 1971 | `SendSDOUploadAsync` | 전송 SDOUpload 비동기 작업을 수행하는 API입니다. | - |
| `24.4.17` | 1972 | `SendSdoUploadAsyncExCmd` | 전송 Sdo 업로드 비동기 Ex 작업을 수행하는 API입니다. | - |
| `24.4.18` | 1974 | `RetreiveSDOUploadAsync` | Retreive SDOUpload 비동기 작업을 수행하는 API입니다. | - |
| `24.4.19` | 1974 | `PDOGeneralRead` | PDOGeneral 읽기 값/상태를 조회하는 API입니다. | - |
| `24.4.20` | 1976 | `PDOGeneralWrite` | PDOGeneral 쓰기 값/설정을 적용하는 API입니다. | - |
| `24.4.21` | 1977 | `GetPDOInfo` | 조회 PDOInfo 값/상태를 조회하는 API입니다. | - |
| `24.4.22` | 1978 | `SetBoolParameter` | 설정 불리언 파라미터 값/설정을 적용하는 API입니다. | - |
| `24.4.23` | 1979 | `SetParameter` | 설정 파라미터 값/설정을 적용하는 API입니다. | - |
| `24.4.24` | 1980 | `GetBoolParameter` | 조회 불리언 파라미터 값/상태를 조회하는 API입니다. | - |
| `24.4.25` | 1981 | `GetParameter` | 조회 파라미터 값/상태를 조회하는 API입니다. | - |
| `24.4.26` | 1982 | `GetAxisError/ReadAxisError` | 조회 축 Error/Read 축 오류 값/상태를 조회하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 24.4.2 ConfigPDOEventMode

- PDF 페이지: 1942
- 원문 위치: [24.4.2 ConfigPDOEventMode](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1942)
- 기능 설명: 구성 PDOEvent 모드 작업을 수행하는 API입니다.

#### 시그니처

```c
void ConfigPDOEventMode(
unsigned char ucPDOEventMode
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.3 EtherCATPIVarInfo

- PDF 페이지: 1943
- 원문 위치: [24.4.3 EtherCATPIVarInfo](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1943)
- 기능 설명: Ether CATPIVar 정보 작업을 수행하는 API입니다.

#### 시그니처

```c
void EthercatPIVarInfo(
unsigned short usPIVarIndex,
unsigned char ucDirection,
NC_PI_ENTRY &VarInfo
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.4 EtherCATReadMemoryRange

- PDF 페이지: 1945
- 원문 위치: [24.4.4 EtherCATReadMemoryRange](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1945)
- 기능 설명: Ether CATRead 메모리 범위 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void EthercatReadMemoryRange(
unsigned short usRegAddr,
unsigned char ucLength,
unsigned char pData[ETHERCAT_MEMORY_READ_MAX_SIZE]
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.5 EtherCATWriteMemoryRange

- PDF 페이지: 1946
- 원문 위치: [24.4.5 EtherCATWriteMemoryRange](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1946)
- 기능 설명: Ether CATWrite 메모리 범위 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void EthercatWriteMemoryRange(
unsigned short usRegAddr,
unsigned char ucLength,
unsigned char pData[ETHERCAT_MEMORY_READ_MAX_SIZE]
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.6 EtherCATReadPIVar

- PDF 페이지: 1947
- 원문 위치: [24.4.6 EtherCATReadPIVar](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1947)
- 기능 설명: Ether CATRead PI 변수 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.7 EtherCATWritePIVar

- PDF 페이지: 1952
- 원문 위치: [24.4.7 EtherCATWritePIVar](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1952)
- 기능 설명: Ether CATWrite PI 변수 값/설정을 적용하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.8 Reset

- PDF 페이지: 1957
- 원문 위치: [24.4.8 Reset](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1957)
- 기능 설명: 리셋 작업을 수행하는 API입니다.

#### 시그니처

```c
void Reset(
) throw(CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.9 ReadStatus

- PDF 페이지: 1958
- 원문 위치: [24.4.9 ReadStatus](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1958)
- 기능 설명: 읽기 상태 값/상태를 조회하는 API입니다.

#### 시그니처

```c
Python Definition def MMC_ReadStatusCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_ReadStatusCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_READSTATUS_IN(object):
uiHndlr =
property(_mmcpp_lib.MMC_READSTATUS_IN_uiHndlr_get,
_mmcpp_lib.MMC_READSTATUS_IN_uiHndlr_set)
ucEnable =
property(_mmcpp_lib.MMC_READSTATUS_IN_ucEnable_get,
_mmcpp_lib.MMC_READSTATUS_IN_ucEnable_set)
class MMC_READSTATUS_OUT(object):
ulState =
property(_mmcpp_lib.MMC_READSTATUS_OUT_ulState_get,
_mmcpp_lib.MMC_READSTATUS_OUT_ulState_set)
usStatus =
property(_mmcpp_lib.MMC_READSTATUS_OUT_usStatus_get,
_mmcpp_lib.MMC_READSTATUS_OUT_usStatus_set)
usErrorID =
property(_mmcpp_lib.MMC_READSTATUS_OUT_usErrorID_get,
_mmcpp_lib.MMC_READSTATUS_OUT_usErrorID_set)
usAxisErrorID =
property(_mmcpp_lib.MMC_READSTATUS_OUT_usAxisErrorID_get,
_mmcpp_lib.MMC_READSTATUS_OUT_usAxisErrorID_set)
usStatusWord =
property(_mmcpp_lib.MMC_READSTATUS_OUT_usStatusWord_get,
_mmcpp_lib.MMC_READSTATUS_OUT_usStatusWord_set)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.10 SendSDO

- PDF 페이지: 1960
- 원문 위치: [24.4.10 SendSDO](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1960)
- 기능 설명: 전송 Sdo 작업을 수행하는 API입니다.

#### 시그니처

```c
virtual void SendSdoCmd(
long lData,
unsigned char ucService,
unsigned char ucSubIndex,
unsigned long ulDataLength,
unsigned short usIndex,
unsigned short usSlaveID
) throw (CMMCException);
```
```c
Python Definition def MMC_SendSdoCmd(hConn, hAxisRef, pInParam, pOutParam):
return _mmcpp_lib.MMC_SendSdoCmd(hConn, hAxisRef,
pInParam, pOutParam)
def SendSdoCmd(self, lData, ucService, ucSubIndex,
ulDataLength, usIndex, usSlaveID):
return _mmcpp_lib.CMMCNode_SendSdoCmd(self, lData,
ucService, ucSubIndex, ulDataLength, usIndex, usSlaveID)
class MMC_SENDSDO_IN(object):
lData =
property(_mmcpp_lib.MMC_SENDSDO_IN_lData_get,
_mmcpp_lib.MMC_SENDSDO_IN_lData_set)
ulDataLength =
property(_mmcpp_lib.MMC_SENDSDO_IN_ulDataLength_get,
_mmcpp_lib.MMC_SENDSDO_IN_ulDataLength_set)
usSlaveID =
property(_mmcpp_lib.MMC_SENDSDO_IN_usSlaveID_get,
_mmcpp_lib.MMC_SENDSDO_IN_usSlaveID_set)
usIndex =
property(_mmcpp_lib.MMC_SENDSDO_IN_usIndex_get,
_mmcpp_lib.MMC_SENDSDO_IN_usIndex_set)
ucSubIndex =
property(_mmcpp_lib.MMC_SENDSDO_IN_ucSubIndex_get,
_mmcpp_lib.MMC_SENDSDO_IN_ucSubIndex_set)
ucService =
property(_mmcpp_lib.MMC_SENDSDO_IN_ucService_get,
_mmcpp_lib.MMC_SENDSDO_IN_ucService_set)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.11 SendSDOEx

- PDF 페이지: 1962
- 원문 위치: [24.4.11 SendSDOEx](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1962)
- 기능 설명: 전송 Sdo 작업을 수행하는 API입니다.

#### 시그니처

```c
virtual void SendSdoCmd(
long lData,
unsigned char ucService,
unsigned char ucSubIndex,
unsigned long ulDataLength,
unsigned short usIndex,
unsigned short usSlaveID
) throw (CMMCException);
```
```c
Python Definition def MMC_SendSdoExCmd(hConn, hAxisRef, pInParam, pOutParam):
return _mmcpp_lib.MMC_SendSdoExCmd(hConn, hAxisRef,
pInParam, pOutParam)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.12 SendSdoAsyncEx

- PDF 페이지: 1963
- 원문 위치: [24.4.12 SendSdoAsyncEx](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1963)
- 기능 설명: Retrieve Sdo 비동기 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_RetrieveSdoAsyncCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
OUT MMC_SENDSDO_OUT* pOutParam
);
```
```c
virtual void SendSdoCmd(
long lData,
unsigned char ucService,
unsigned char ucSubIndex,
unsigned long ulDataLength,
unsigned short usIndex,
unsigned short usSlaveID
) throw (CMMCException);
```
```c
Python Definition def MMC_SendSdoAsyncExCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_SendSdoAsyncExCmd(hConn,
hAxisRef, pInParam, pOutParam)
```

#### 구조체/인자

##### `MMC_SENDSDO_OUT`
| 필드 | 해석 |
|---|---|
| `int32_t lData;` | 데이터 버퍼 또는 데이터 값입니다. |
| `uint32_t ulDataLength;` | 데이터 버퍼 또는 데이터 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short usErrorID;` | 오류 ID입니다. |

### 24.4.13 SendSDODownloadEx

- PDF 페이지: 1966
- 원문 위치: [24.4.13 SendSDODownloadEx](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1966)
- 기능 설명: 전송 Sdo 다운로드 Ex 작업을 수행하는 API입니다.

#### 시그니처

```c
void SendSdoDownloadExCmd(
SEND_SDO_DATA_EX *uData,
unsigned short usIndex,
unsigned char ucSubIndex,
unsigned char ucDataLength
)throw (CMMCException);
```
```c
Python Definition def SendSdoDownloadExCmd(self, uData, usIndex, ucSubIndex,
ucDataLength):
return
_mmcpp_lib.CMMCNode_SendSdoDownloadExCmd(self, uData,
usIndex, ucSubIndex, ucDataLength)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.14 SendSDOUpload

- PDF 페이지: 1968
- 원문 위치: [24.4.14 SendSDOUpload](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1968)
- 기능 설명: 전송 SDOUpload 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.15 SendSDOUploadEx

- PDF 페이지: 1969
- 원문 위치: [24.4.15 SendSDOUploadEx](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1969)
- 기능 설명: 전송 Sdo 업로드 Ex 작업을 수행하는 API입니다.

#### 시그니처

```c
void SendSdoUploadExCmd(
SEND_SDO_DATA_EX *uData,
unsigned short usIndex,
unsigned char ucSubIndex,
unsigned char ucDataLength
)throw (CMMCException);
```
```c
Python Definition def SendSdoUploadExCmd(self, uData, usIndex, ucSubIndex,
ucDataLength):
return _mmcpp_lib.CMMCNode_SendSdoUploadExCmd(self,
uData, usIndex, ucSubIndex, ucDataLength)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.16 SendSDOUploadAsync

- PDF 페이지: 1971
- 원문 위치: [24.4.16 SendSDOUploadAsync](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1971)
- 기능 설명: 전송 SDOUpload 비동기 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.17 SendSDOUploadAsyncEx

- PDF 페이지: 1972
- 원문 위치: [24.4.17 SendSDOUploadAsyncEx](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1972)
- 기능 설명: 전송 Sdo 업로드 비동기 Ex 작업을 수행하는 API입니다.

#### 시그니처

```c
void SendSdoUploadAsyncExCmd(
SEND_SDO_DATA_EX *uData,
unsigned short usIndex,
unsigned char ucSubIndex,
unsigned char ucDataLength
)throw (CMMCException);
```
```c
Python Definition def SendSdoUploadAsyncExCmd(self, uData, usIndex,
ucSubIndex, ucDataLength):
return
_mmcpp_lib.CMMCNode_SendSdoUploadAsyncExCmd(self, uData,
usIndex, ucSubIndex, ucDataLength)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.18 RetreiveSDOUploadAsync

- PDF 페이지: 1974
- 원문 위치: [24.4.18 RetreiveSDOUploadAsync](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1974)
- 기능 설명: Retreive SDOUpload 비동기 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.19 PDOGeneralRead

- PDF 페이지: 1974
- 원문 위치: [24.4.19 PDOGeneralRead](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1974)
- 기능 설명: PDOGeneral 읽기 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void RetreiveSdoUploadAsync(
long& lData
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.20 PDOGeneralWrite

- PDF 페이지: 1976
- 원문 위치: [24.4.20 PDOGeneralWrite](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1976)
- 기능 설명: PDOGeneral 쓰기 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void PDOGeneralWrite(
unsigned char ucParam,
MMCPPULL_T ulliVal
) throw(CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.21 GetPDOInfo

- PDF 페이지: 1977
- 원문 위치: [24.4.21 GetPDOInfo](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1977)
- 기능 설명: 조회 PDOInfo 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void GetPDOInfo(
unsigned char uiPDONumber,
int &iPDOEventMode,
unsigned char &ucPDOCommType,
unsigned char &ucTPDOCommEventGroup,
unsigned char &ucRPDOCommEventGroup
) throw(CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.22 SetBoolParameter

- PDF 페이지: 1978
- 원문 위치: [24.4.22 SetBoolParameter](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1978)
- 기능 설명: 설정 불리언 파라미터 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetBoolParameter(
unsigned long ulValue,
MMC_PARAMETER_LIST_ENUM eNumber,
int iIndex
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.23 SetParameter

- PDF 페이지: 1979
- 원문 위치: [24.4.23 SetParameter](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1979)
- 기능 설명: 설정 파라미터 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetParameter(
double dbValue,
MMC_PARAMETER_LIST_ENUM eNumber,
int iIndex
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.24 GetBoolParameter

- PDF 페이지: 1980
- 원문 위치: [24.4.24 GetBoolParameter](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1980)
- 기능 설명: 조회 불리언 파라미터 값/상태를 조회하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.25 GetParameter

- PDF 페이지: 1981
- 원문 위치: [24.4.25 GetParameter](../chunks/075_p1942-p1981_24.4.2-ConfigPDOEventMode.md#pdf-page-1981)
- 기능 설명: 조회 파라미터 값/상태를 조회하는 API입니다.

#### 시그니처

```c
double GetParameter(
MMC_PARAMETER_LIST_ENUM eNumber,
int iIndex
)throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.4.26 GetAxisError/ReadAxisError

- PDF 페이지: 1982
- 원문 위치: [24.4.26 GetAxisError/ReadAxisError](../chunks/076_p1982-p1992_24.4.26-GetAxisError-ReadAxisError.md#pdf-page-1982)
- 기능 설명: 조회 축 Error/Read 축 오류 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

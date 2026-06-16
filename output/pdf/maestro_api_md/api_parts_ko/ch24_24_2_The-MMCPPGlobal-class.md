# 24.2 The MMCPPGlobal class - API 분석

- 원본 장: `Chapter 24 Programming in C++`
- 시작 PDF 페이지: 1707
- 원문 위치: [24.2 The MMCPPGlobal class](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1707)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `24.2.2` | 1723 | `RegisterRTE` | 등록 RTE 작업을 수행하는 API입니다. | - |
| `24.2.3` | 1724 | `RegisterWarningClbk` | 등록 경고 Clbk 작업을 수행하는 API입니다. | - |
| `24.2.4` | 1725 | `SetThrowFlag` | 설정 Throw Flag 값/설정을 적용하는 API입니다. | - |
| `24.2.5` | 1726 | `SetThrowWarningFlag` | 설정 Throw 경고 Flag 값/설정을 적용하는 API입니다. | - |
| `24.2.6` | 1727 | `SetPrintErrorFlag` | 설정 출력 오류 Flag 값/설정을 적용하는 API입니다. | - |
| `24.2.7` | 1728 | `SetPrintWarningFlag` | 설정 출력 경고 Flag 값/설정을 적용하는 API입니다. | - |
| `24.2.8` | 1729 | `ThrowMessage` | Throw 메시지 작업을 수행하는 API입니다. | - |
| `24.2.9` | 1730 | `GetConnectionType` | 조회 연결 Type 값/상태를 조회하는 API입니다. | - |
| `24.2.10` | 1731 | `SetConnectionType` | 설정 연결 Type 값/설정을 적용하는 API입니다. | - |
| `24.2.11` | 1732 | `SetMessageFileName` | 설정 메시지 파일 Name 값/설정을 적용하는 API입니다. | - |
| `24.2.12` | 1733 | `GetSyncTime` | 조회 동기 Time 값/상태를 조회하는 API입니다. | - |
| `24.2.13` | 1734 | `SetSyncTime` | 설정 동기 Time 값/설정을 적용하는 API입니다. | - |
| `24.2.14` | 1735 | `CreateSYNCTimer` | 생성 SYNC 타이머 작업을 수행하는 API입니다. | - |
| `24.2.15` | 1736 | `DestroySYNCTimer` | 삭제 SYNC 타이머 작업을 수행하는 API입니다. | - |
| `24.2.16` | 1737 | `GetConnectionReg` | 조회 연결 Reg 값/상태를 조회하는 API입니다. | - |
| `24.2.17` | 1738 | `ConfigBulkRead` | 구성 벌크 읽기 값/상태를 조회하는 API입니다. | - |
| `24.2.18` | 1740 | `PerformBulkRead` | Perform 벌크 읽기 값/상태를 조회하는 API입니다. | - |
| `24.2.19` | 1742 | `UserCommandControl` | 사용자 명령 Control 작업을 수행하는 API입니다. | - |
| `24.2.20` | 1743 | `RegErrPolicy` | Reg Err 정책 작업을 수행하는 API입니다. | - |
| `24.2.21` | 1744 | `GetErrPolicy` | 조회 Err 정책 값/상태를 조회하는 API입니다. | - |
| `24.2.22` | 1745 | `ResetSystem` | 리셋 시스템 작업을 수행하는 API입니다. | - |
| `24.2.23` | 1746 | `RunSineGenrator` | Run Sine Genrator 작업을 수행하는 API입니다. | - |
| `24.2.24` | 1747 | `RegisterConnection` | 등록 연결 작업을 수행하는 API입니다. | - |
| `24.2.25` | 1748 | `GetConnectionReg ClearConnectionReg` | 조회 연결 Reg 초기화 연결 Reg 값/상태를 조회하는 API입니다. | - |
| `24.2.26` | 1749 | `UserCommandControl` | 사용자 명령 Control 작업을 수행하는 API입니다. | - |
| `24.2.27` | 1750 | `GetLibVersion` | 조회 라이브러리 버전 값/상태를 조회하는 API입니다. | - |
| `24.2.28` | 1751 | `GetProfileConditioning` | 조회 프로파일 컨디셔닝 값/상태를 조회하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 24.2.2 RegisterRTE

- PDF 페이지: 1723
- 원문 위치: [24.2.2 RegisterRTE](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1723)
- 기능 설명: 등록 RTE 작업을 수행하는 API입니다.

#### 시그니처

```c
void RegisterRTE(
RTE_CLBKP pRTEClbk,
bool bIsToCloseOnRTE = true
)
{m_pRTEClbk = pRTEClbk;
m_bIsToCloseOnRTE = bIsToCloseOnRTE;}
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.3 RegisterWarningClbk

- PDF 페이지: 1724
- 원문 위치: [24.2.3 RegisterWarningClbk](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1724)
- 기능 설명: 등록 경고 Clbk 작업을 수행하는 API입니다.

#### 시그니처

```c
void RegisterWarningClbk(
WARNING_CLBKP pWarningClbk
)
{m_pWarningClbk = pWarningClbk;}
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.4 SetThrowFlag

- PDF 페이지: 1725
- 원문 위치: [24.2.4 SetThrowFlag](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1725)
- 기능 설명: 설정 Throw Flag 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetThrowFlag(
bool bThrow,
bool bIsToCloseOnThrow = true
)
m_bThrowException_OnError = bThrow;
m_bIsToCloseOnThrow = bIsToCloseOnThrow;}
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.5 SetThrowWarningFlag

- PDF 페이지: 1726
- 원문 위치: [24.2.5 SetThrowWarningFlag](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1726)
- 기능 설명: 설정 Throw 경고 Flag 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetThrowWarningFlag(
bool bThrowWarining
)
{m_bThrowException_OnWarning = bThrowWarining;}
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.6 SetPrintErrorFlag

- PDF 페이지: 1727
- 원문 위치: [24.2.6 SetPrintErrorFlag](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1727)
- 기능 설명: 설정 출력 오류 Flag 값/설정을 적용하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.7 SetPrintWarningFlag

- PDF 페이지: 1728
- 원문 위치: [24.2.7 SetPrintWarningFlag](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1728)
- 기능 설명: 설정 출력 경고 Flag 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetPrintWarningFlag(
bool bPrintWarining
)
{m_bPrintWarning = bPrintWarining;}
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.8 ThrowMessage

- PDF 페이지: 1729
- 원문 위치: [24.2.8 ThrowMessage](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1729)
- 기능 설명: Throw 메시지 작업을 수행하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.9 GetConnectionType

- PDF 페이지: 1730
- 원문 위치: [24.2.9 GetConnectionType](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1730)
- 기능 설명: 조회 연결 Type 값/상태를 조회하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.10 SetConnectionType

- PDF 페이지: 1731
- 원문 위치: [24.2.10 SetConnectionType](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1731)
- 기능 설명: 설정 연결 Type 값/설정을 적용하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.11 SetMessageFileName

- PDF 페이지: 1732
- 원문 위치: [24.2.11 SetMessageFileName](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1732)
- 기능 설명: 설정 메시지 파일 Name 값/설정을 적용하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.12 GetSyncTime

- PDF 페이지: 1733
- 원문 위치: [24.2.12 GetSyncTime](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1733)
- 기능 설명: 조회 동기 Time 값/상태를 조회하는 API입니다.

#### 시그니처

```c
unsigned short GetSyncTime(
unsigned int uiConnHndl
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.13 SetSyncTime

- PDF 페이지: 1734
- 원문 위치: [24.2.13 SetSyncTime](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1734)
- 기능 설명: 설정 동기 Time 값/설정을 적용하는 API입니다.

#### 시그니처

```c
int SetSyncTime(
unsigned int uiConnHndl,
unsigned short usSync
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.14 CreateSYNCTimer

- PDF 페이지: 1735
- 원문 위치: [24.2.14 CreateSYNCTimer](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1735)
- 기능 설명: 생성 SYNC 타이머 작업을 수행하는 API입니다.

#### 시그니처

```c
void CreateSYNCTimer(
unsigned int uiConnHndl,
MMC_SYNC_TIMER_CB_FUNC pfClbk,
unsigned short usSYNCTimerTime
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.15 DestroySYNCTimer

- PDF 페이지: 1736
- 원문 위치: [24.2.15 DestroySYNCTimer](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1736)
- 기능 설명: 삭제 SYNC 타이머 작업을 수행하는 API입니다.

#### 시그니처

```c
void DestroySYNCTimer(
unsigned int uiConnHndl
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.16 GetConnectionReg

- PDF 페이지: 1737
- 원문 위치: [24.2.16 GetConnectionReg](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1737)
- 기능 설명: 조회 연결 Reg 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.17 ConfigBulkRead

- PDF 페이지: 1738
- 원문 위치: [24.2.17 ConfigBulkRead](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1738)
- 기능 설명: 구성 벌크 읽기 값/상태를 조회하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.18 PerformBulkRead

- PDF 페이지: 1740
- 원문 위치: [24.2.18 PerformBulkRead](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1740)
- 기능 설명: Perform 벌크 읽기 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void PerformBulkRead(
MMC_CONNECT_HNDL hConnHndl,
unsigned short usNumberOfAxes,
NC_BULKREAD_CONFIG_ENUM eConfiguration,
NC_BULKREAD_PRESET_ENUM& eChosenPreset,
unsigned long* ulOutputData
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.19 UserCommandControl

- PDF 페이지: 1742
- 원문 위치: [24.2.19 UserCommandControl](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1742)
- 기능 설명: 사용자 명령 Control 작업을 수행하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.20 RegErrPolicy

- PDF 페이지: 1743
- 원문 위치: [24.2.20 RegErrPolicy](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1743)
- 기능 설명: Reg Err 정책 작업을 수행하는 API입니다.

#### 시그니처

```c
void RegErrPolicy(
MMC_CONNECT_HNDL hConnHndl,
MMC_REGERRPOLICY_IN stInParams
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.21 GetErrPolicy

- PDF 페이지: 1744
- 원문 위치: [24.2.21 GetErrPolicy](../chunks/069_p1705-p1744_24.1-Introduction.md#pdf-page-1744)
- 기능 설명: 조회 Err 정책 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void GetErrPolicy(
MMC_CONNECT_HNDL hConnHndl,
MMC_GETERRPOLICY_IN stInParams,
MMC_GETERRPOLICY_OUT &stOutParams
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.22 ResetSystem

- PDF 페이지: 1745
- 원문 위치: [24.2.22 ResetSystem](../chunks/070_p1745-p1781_24.2.22-ResetSystem.md#pdf-page-1745)
- 기능 설명: 리셋 시스템 작업을 수행하는 API입니다.

#### 시그니처

```c
void ResetSystem(
MMC_CONNECT_HNDL hConnHndl
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.23 RunSineGenrator

- PDF 페이지: 1746
- 원문 위치: [24.2.23 RunSineGenrator](../chunks/070_p1745-p1781_24.2.22-ResetSystem.md#pdf-page-1746)
- 기능 설명: Run Sine Genrator 작업을 수행하는 API입니다.

#### 시그니처

```c
void RunSineGenrator(
MMC_CONNECT_HNDL hConnHndl,
MMC_SINEGENERATOR_IN &stInParams
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.24 RegisterConnection

- PDF 페이지: 1747
- 원문 위치: [24.2.24 RegisterConnection](../chunks/070_p1745-p1781_24.2.22-ResetSystem.md#pdf-page-1747)
- 기능 설명: 등록 연결 작업을 수행하는 API입니다.

#### 시그니처

```c
int RegisterConnection(
unsigned int uiConnHndl,
void * pConn
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.25 GetConnectionReg ClearConnectionReg

- PDF 페이지: 1748
- 원문 위치: [24.2.25 GetConnectionReg ClearConnectionReg](../chunks/070_p1745-p1781_24.2.22-ResetSystem.md#pdf-page-1748)
- 기능 설명: 조회 연결 Reg 초기화 연결 Reg 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void ClearConnectionReg(
unsigned int uiConnHndl
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.26 UserCommandControl

- PDF 페이지: 1749
- 원문 위치: [24.2.26 UserCommandControl](../chunks/070_p1745-p1781_24.2.22-ResetSystem.md#pdf-page-1749)
- 기능 설명: 사용자 명령 Control 작업을 수행하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.27 GetLibVersion

- PDF 페이지: 1750
- 원문 위치: [24.2.27 GetLibVersion](../chunks/070_p1745-p1781_24.2.22-ResetSystem.md#pdf-page-1750)
- 기능 설명: 조회 라이브러리 버전 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.2.28 GetProfileConditioning

- PDF 페이지: 1751
- 원문 위치: [24.2.28 GetProfileConditioning](../chunks/070_p1745-p1781_24.2.22-ResetSystem.md#pdf-page-1751)
- 기능 설명: 조회 프로파일 컨디셔닝 값/상태를 조회하는 API입니다.

#### 시그니처

```c
int GetProfileConditioning(
MMC_CONNECT_HNDL hConnHndl,
MMC_PROFCONDINF_OUT& o_params
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

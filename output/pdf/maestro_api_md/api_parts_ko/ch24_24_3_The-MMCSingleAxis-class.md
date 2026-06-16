# 24.3 The MMCSingleAxis class - API 분석

- 원본 장: `Chapter 24 Programming in C++`
- 시작 PDF 페이지: 1754
- 원문 위치: [24.3 The MMCSingleAxis class](../chunks/070_p1745-p1781_24.2.22-ResetSystem.md#pdf-page-1754)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `24.3.5` | 1795 | `SetDefaultParams` | 설정 Default 파라미터 값/설정을 적용하는 API입니다. | - |
| `24.3.6` | 1800 | `SetDefaultHomeDS402Params` | DS-402 홈 동작을 수행하는 API입니다. | - |
| `24.3.7` | 1801 | `SetDefaultHomeDS402ExParams` | 확장 DS-402 홈 동작을 수행하는 API입니다. | - |
| `24.3.8` | 1802 | `Home` | 축 홈 동작을 수행하는 API입니다. | - |
| `24.3.9` | 1803 | `MMC_HomeDS402Cmd` | DS-402 홈 동작을 수행하는 API입니다. | - |
| `24.3.10` | 1806 | `MMC_HomeDS402ExCmd` | 확장 DS-402 홈 동작을 수행하는 API입니다. | - |
| `24.3.11` | 1808 | `MMC_MoveAbsoluteCmd` | 지정한 절대 위치로 축을 이동시키는 모션 API입니다. | - |
| `24.3.12` | 1810 | `MMC_MoveAbsoluteCmd` | 지정한 절대 위치로 축을 이동시키는 모션 API입니다. | - |
| `24.3.13` | 1812 | `MMC_MoveAdditiveCmd` | 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다. | - |
| `24.3.14` | 1816 | `MMC_MoveAdditiveCmd` | 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다. | - |
| `24.3.15` | 1818 | `MMC_MoveRelativeCmd` | 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다. | - |
| `24.3.16` | 1822 | `MMC_MoveRelativeCmd` | 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다. | - |
| `24.3.17` | 1824 | `MMC_MoveVelocityCmd` | 지정 속도로 연속 이동을 수행하는 모션 API입니다. | - |
| `24.3.18` | 1826 | `MMC_MoveVelocityExCmd` | 지정 속도로 연속 이동을 수행하는 모션 API입니다. | - |
| `24.3.19` | 1828 | `MMC_MoveAbsoluteRepetitiveCmd` | 지정한 절대 위치로 축을 이동시키는 모션 API입니다. | - |
| `24.3.20` | 1832 | `MMC_MoveAbsoluteRepetitiveCmd` | 지정한 절대 위치로 축을 이동시키는 모션 API입니다. | - |
| `24.3.21` | 1835 | `MMC_MoveRelativeRepetitiveCmd` | 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다. | - |
| `24.3.22` | 1839 | `MMC_MoveRelativeRepetitiveCmd` | 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다. | - |
| `24.3.23` | 1842 | `MMC_MoveAdditiveRepetitiveExCmd` | 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다. | - |
| `24.3.24` | 1846 | `MMC_MoveAdditiveRepetitiveExCmd` | 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다. | - |
| `24.3.25` | 1849 | `MMC_MoveTorqueCmd` | 지정 토크로 연속 동작을 수행하는 모션 API입니다. | - |
| `24.3.26` | 1851 | `MMC_MoveTorqueCmd` | 지정 토크로 연속 동작을 수행하는 모션 API입니다. | - |
| `24.3.27` | 1853 | `PositionProfile` | 위치 프로파일 작업을 수행하는 API입니다. | - |
| `24.3.28` | 1854 | `GetTouchProbeData` | 조회 터치 프로브 데이터 값/상태를 조회하는 API입니다. | - |
| `24.3.29` | 1859 | `TouchProbeDisable` | 터치 프로브 비활성화 활성화/비활성화 제어를 수행하는 API입니다. | - |
| `24.3.30` | 1860 | `TouchProbeDisableEx` | 터치 프로브 비활성화 Ex 활성화/비활성화 제어를 수행하는 API입니다. | - |
| `24.3.31` | 1862 | `TouchProbeEnable` | 터치 프로브 활성화 활성화/비활성화 제어를 수행하는 API입니다. | - |
| `24.3.32` | 1863 | `TouchProbeEnableEx` | 터치 프로브 활성화 Ex 활성화/비활성화 제어를 수행하는 API입니다. | - |
| `24.3.33` | 1865 | `SetOpMode` | 설정 Op 모드 값/설정을 적용하는 API입니다. | - |
| `24.3.34` | 1866 | `SetOpModeEx` | 설정 Op 모드 Ex 값/설정을 적용하는 API입니다. | - |
| `24.3.35` | 1867 | `GetOpMode` | 조회 Op 모드 값/상태를 조회하는 API입니다. | - |
| `24.3.36` | 1870 | `PowerOn` | 전원 On 작업을 수행하는 API입니다. | - |
| `24.3.37` | 1870 | `PowerOff` | 값 또는 동작 조건을 설정하는 API입니다. | - |
| `24.3.38` | 1872 | `SendCmdViaSdoDownload` | 전송 Cmd Via Sdo 다운로드 작업을 수행하는 API입니다. | - |
| `24.3.39` | 1873 | `MMC_GetVersionExCmd` | 조회 버전 Ex 값/상태를 조회하는 API입니다. | - |
| `24.3.40` | 1877 | `GetActualPosition` | 조회 실제 위치 값/상태를 조회하는 API입니다. | - |
| `24.3.41` | 1877 | `GetActualVelocity` | 조회 실제 속도 값/상태를 조회하는 API입니다. | - |
| `24.3.42` | 1879 | `GetActualTorque` | 조회 실제 토크 값/상태를 조회하는 API입니다. | - |
| `24.3.43` | 1880 | `MMC_HaltCmd` | 축을 정상 운전 조건에서 제어 정지시키는 API입니다. | - |
| `24.3.44` | 1882 | `MMC_StopCmd` | 축 또는 동작을 정지 상태로 전환하는 API입니다. | - |
| `24.3.45` | 1883 | `GetDigInput[s]` | 조회 Dig 입력 값/상태를 조회하는 API입니다. | - |
| `24.3.46` | 1884 | `GetDigOutputs32Bit` | 조회 Dig Outputs32 Bit 값/상태를 조회하는 API입니다. | - |
| `24.3.47` | 1884 | `GetDigOutputs` | 조회 Dig 출력 값/상태를 조회하는 API입니다. | - |
| `24.3.48` | 1886 | `SetDigOutputs32Bit` | 설정 Dig Outputs32 Bit 값/설정을 적용하는 API입니다. | - |
| `24.3.49` | 1887 | `SetDigOutputs` | 설정 Dig 출력 값/설정을 적용하는 API입니다. | - |
| `24.3.50` | 1888 | `SetOverride` | 설정 오버라이드 값/설정을 적용하는 API입니다. | - |
| `24.3.51` | 1889 | `ConfigPDO` | 구성 PDO 작업을 수행하는 API입니다. | - |
| `24.3.52` | 1891 | `CancelPDO` | Cancel PDO 작업을 수행하는 API입니다. | - |
| `24.3.53` | 1892 | `ChangeDefaultPDOConfig` | Change Default PDOConfig 작업을 수행하는 API입니다. | - |
| `24.3.54` | 1893 | `ElmoSetAsyncAn array` | Elmo 설정 비동기 Array 값/설정을 적용하는 API입니다. | - |
| `24.3.55` | 1894 | `ElmoSetAsyncParam` | Elmo 설정 비동기 파라미터 값/설정을 적용하는 API입니다. | - |
| `24.3.56` | 1896 | `ElmoGetAsyncIntParam` | Elmo 조회 비동기 Int 파라미터 값/상태를 조회하는 API입니다. | - |
| `24.3.57` | 1898 | `ElmoGetAsyncFloatParam` | Elmo 조회 비동기 Float 파라미터 값/상태를 조회하는 API입니다. | - |
| `24.3.58` | 1899 | `ElmoGetAsyncIntAn array` | Elmo 조회 비동기 Int Array 값/상태를 조회하는 API입니다. | - |
| `24.3.59` | 1900 | `ElmoGetAsyncFloatAn array` | Elmo 조회 비동기 Float Array 값/상태를 조회하는 API입니다. | - |
| `24.3.60` | 1901 | `ElmoGetSyncParam` | Elmo 조회 동기 파라미터 값/상태를 조회하는 API입니다. | - |
| `24.3.61` | 1902 | `ElmoGetSyncAn array` | Elmo 조회 동기 Array 값/상태를 조회하는 API입니다. | - |
| `24.3.62` | 1903 | `ElmoCallAsync` | Elmo 호출 비동기 작업을 수행하는 API입니다. | - |
| `24.3.63` | 1904 | `ElmoExecute` | Elmo 실행 작업을 수행하는 API입니다. | - |
| `24.3.64` | 1905 | `ElmoIsReplyAwaiting` | Elmo Is Reply Awaiting 작업을 수행하는 API입니다. | - |
| `24.3.65` | 1905 | `ElmoGetReply` | Elmo 조회 Reply 값/상태를 조회하는 API입니다. | - |
| `24.3.66` | 1906 | `ConfigVirtualEncoder` | 구성 Virtual Encoder 작업을 수행하는 API입니다. | - |
| `24.3.67` | 1908 | `CancelVirtualEncoder` | Cancel Virtual Encoder 작업을 수행하는 API입니다. | - |
| `24.3.68` | 1908 | `SetPosition` | 설정 위치 값/설정을 적용하는 API입니다. | - |
| `24.3.69` | 1910 | `SetParameter` | 설정 파라미터 값/설정을 적용하는 API입니다. | - |
| `24.3.70` | 1911 | `AxisLink` | 축 Link 작업을 수행하는 API입니다. | - |
| `24.3.71` | 1912 | `AxisUnLink` | 축 Un Link 작업을 수행하는 API입니다. | - |
| `24.3.72` | 1912 | `GetBoolParameter` | 조회 불리언 파라미터 값/상태를 조회하는 API입니다. | - |
| `24.3.73` | 1914 | `SetBoolParameter` | 설정 불리언 파라미터 값/설정을 적용하는 API입니다. | - |
| `24.3.74` | 1915 | `GetParameter` | 조회 파라미터 값/상태를 조회하는 API입니다. | - |
| `24.3.75` | 1916 | `SetProfileConditioning` | 설정 프로파일 컨디셔닝 값/설정을 적용하는 API입니다. | - |
| `24.3.76` | 1919 | `MMC_GetFbDepthCmd` | 조회 Fb 깊이 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |
| `24.3.77` | 1923 | `GetStatusRegister` | 조회 상태 등록 값/상태를 조회하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 24.3.5 SetDefaultParams

- PDF 페이지: 1795
- 원문 위치: [24.3.5 SetDefaultParams](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1795)
- 기능 설명: 설정 Default 파라미터 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetDefaultParams(
const MMC_MOTIONPARAMS_SINGLE& stSingleAxisParams
);
```
```c
void initAdminSingleAxis(void)
// ==============================
{
int iEventMask;
MMC_MOTIONPARAMS_SINGLE stSingleDefault;
/* Init default Gmas Parameters */
stSingleDefault.fEndVelocity = 0;
stSingleDefault.dbDistance = 100000;
stSingleDefault.dbPosition = 0;
stSingleDefault.fVelocity = 100000;
stSingleDefault.fAcceleration = 2000000;
stSingleDefault.fDeceleration = 10000000;
stSingleDefault.fJerk = 200000000;
/* MC_POSITIVE_DIRECTION, MC_SHORTEST_WAY, */
/* MC_NEGATIVE_DIRECTION, MC_CURRENT_DIRECTION */
stSingleDefault.eDirection = MC_POSITIVE_DIRECTION;
stSingleDefault.eBufferMode = MC_BUFFERED_MODE;
stSingleDefault.ucExecute = 1;
/* CallbackFunc in ConnectIPCEx call if there */
/* is no calling to 'RegisterEventCallback' */
iEventMask = 0x7fffffff;
ComHndl = cConn.ConnectIPCEx(iEventMask, (MMC_MB_CLBK)CallbackFunc);
```
```c
void initAdminMultiAxis()
// =========================
{
AxisB.InitAxisData("a02", ComHndl);
```
```c
void endAdminSingle(void)
// =========================
{
MMC_CloseConnection(ComHndl) ;
}
void endAdminMultiAxis(void)
// ================================
{
Group.RemoveAxisFromGroup(NC_NODE_1_ID);
```
```c
void SnroMoveCombinations(int trace)
// ====================================
{
printf("%s%s -%d- ", strStrSnro, __func__, trace);
```
```c
void SnroEnableDisableMotionEndedEvent(int trace)
// =================================================
{
printf("%s%s -%d- ", strStrSnro, __func__, trace);
```
```c
void SnroSetGetParameters(int trace)
// ====================================
{
printf("%s%s -%d- ", strStrSnro, __func__, trace);
```
```c
void SnroDepthName(int trace)
// =============================
{
printf("%s%s -%d- ", strStrSnro, __func__, trace);
```

#### 구조체/인자

##### `MMC_MOTIONPARAMS_SINGLE`
| 필드 | 해석 |
|---|---|
| `double dbPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbDistance;` | 이동 거리 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `float fEndVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAcceleration;` | 가속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `float fJerk;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `MC_DIRECTION_ENUM eDirection ;` | 동작 방향 지정 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `unsigned int uiExecDelayMs;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |

### 24.3.6 SetDefaultHomeDS402Params

- PDF 페이지: 1800
- 원문 위치: [24.3.6 SetDefaultHomeDS402Params](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1800)
- 기능 설명: DS-402 홈 동작을 수행하는 API입니다.

#### 시그니처

```c
void SetDefaultHomeDS402Params(
const MMC_HOMEDS402_IN& stSingleParams
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

### 24.3.7 SetDefaultHomeDS402ExParams

- PDF 페이지: 1801
- 원문 위치: [24.3.7 SetDefaultHomeDS402ExParams](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1801)
- 기능 설명: 확장 DS-402 홈 동작을 수행하는 API입니다.

#### 시그니처

```c
void SetDefaultHomeDS402ExParams(
const MMC_HOMEDS402EX_IN& stSingleParams
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.8 Home

- PDF 페이지: 1802
- 원문 위치: [24.3.8 Home](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1802)
- 기능 설명: 축 홈 동작을 수행하는 API입니다.

#### 시그니처

```c
unsigned short Home(
[MMC_HOME_IN stHomeParams]
short* sErrorID,
unsigned int* uiHndl
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.9 HomeDS402

- PDF 페이지: 1803
- 원문 위치: [24.3.9 HomeDS402](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1803)
- 기능 설명: DS-402 홈 동작을 수행하는 API입니다.

#### 시그니처

```c
void HomeDS402(
) throw (CMMCException);
```
```c
void HomeDS402(
MMC_HOMEDS402_IN stHomeDS402Params
) throw (CMMCException);
```
```c
Python Definition def MMC_HomeDS402Cmd(hConn, hAxisRef, pInParam, pOutParam):
return _mmcpp_lib.MMC_HomeDS402Cmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_HOMEDS402_IN(object):
dbPosition =
property(_mmcpp_lib.MMC_HOMEDS402_IN_dbPosition_get,
_mmcpp_lib.MMC_HOMEDS402_IN_dbPosition_set)
fAcceleration =
property(_mmcpp_lib.MMC_HOMEDS402_IN_fAcceleration_get,
_mmcpp_lib.MMC_HOMEDS402_IN_fAcceleration_set)
fVelocity =
property(_mmcpp_lib.MMC_HOMEDS402_IN_fVelocity_get,
_mmcpp_lib.MMC_HOMEDS402_IN_fVelocity_set)
fDistanceLimit =
property(_mmcpp_lib.MMC_HOMEDS402_IN_fDistanceLimit_get,
_mmcpp_lib.MMC_HOMEDS402_IN_fDistanceLimit_set)
fTorqueLimit =
property(_mmcpp_lib.MMC_HOMEDS402_IN_fTorqueLimit_get,
_mmcpp_lib.MMC_HOMEDS402_IN_fTorqueLimit_set)
eBufferMode =
property(_mmcpp_lib.MMC_HOMEDS402_IN_eBufferMode_get,
_mmcpp_lib.MMC_HOMEDS402_IN_eBufferMode_set)
uiHomingMethod =
property(_mmcpp_lib.MMC_HOMEDS402_IN_uiHomingMethod_get,
_mmcpp_lib.MMC_HOMEDS402_IN_uiHomingMethod_set)
uiTimeLimit =
property(_mmcpp_lib.MMC_HOMEDS402_IN_uiTimeLimit_get,
_mmcpp_lib.MMC_HOMEDS402_IN_uiTimeLimit_set)
ucExecute =
```
```c
void SetDefParamsHome(void)
// ==============================
{
MMC_HOMEDS402_IN stDS402Home ;
MMC_HOME_IN stSingleParams;
OPM402 drvMode;
short sErrorID;
printf("\n %s:", __func__);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.10 HomeDS402Ex

- PDF 페이지: 1806
- 원문 위치: [24.3.10 HomeDS402Ex](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1806)
- 기능 설명: 확장 DS-402 홈 동작을 수행하는 API입니다.

#### 시그니처

```c
void HomeDS402Ex(
) throw (CMMCException);
```
```c
void HomeDS402Ex(MMC_HOMEDS402EX_IN stParams
) throw (CMMCException);
```
```c
Python Definition def MMC_HomeDS402ExCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_HomeDS402ExCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_HOMEDS402EX_IN(object):
dbPosition =
property(_mmcpp_lib.MMC_HOMEDS402EX_IN_dbPosition_get,
_mmcpp_lib.MMC_HOMEDS402EX_IN_dbPosition_set)
dbDetectionVelocityLimit =
property(_mmcpp_lib.MMC_HOMEDS402EX_IN_dbDetectionVelocityL
imit_get,
_mmcpp_lib.MMC_HOMEDS402EX_IN_dbDetectionVelocityLimit_set)
fAcceleration =
property(_mmcpp_lib.MMC_HOMEDS402EX_IN_fAcceleration_get,
_mmcpp_lib.MMC_HOMEDS402EX_IN_fAcceleration_set)
fVelocityHi =
property(_mmcpp_lib.MMC_HOMEDS402EX_IN_fVelocityHi_get,
_mmcpp_lib.MMC_HOMEDS402EX_IN_fVelocityHi_set)
fVelocityLo =
property(_mmcpp_lib.MMC_HOMEDS402EX_IN_fVelocityLo_get,
_mmcpp_lib.MMC_HOMEDS402EX_IN_fVelocityLo_set)
fDistanceLimit =
property(_mmcpp_lib.MMC_HOMEDS402EX_IN_fDistanceLimit_get,
_mmcpp_lib.MMC_HOMEDS402EX_IN_fDistanceLimit_set)
fTorqueLimit =
property(_mmcpp_lib.MMC_HOMEDS402EX_IN_fTorqueLimit_get,
_mmcpp_lib.MMC_HOMEDS402EX_IN_fTorqueLimit_set)
eBufferMode =
property(_mmcpp_lib.MMC_HOMEDS402EX_IN_eBufferMode_get,
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.11 MoveAbsolute

- PDF 페이지: 1808
- 원문 위치: [24.3.11 MoveAbsolute](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1808)
- 기능 설명: 지정한 절대 위치로 축을 이동시키는 모션 API입니다.

#### 시그니처

```c
int MoveAbsolute(
double dPos,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAbsolute(
double dPos,
float fVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAbsolute(
double dPos,
float fVel,
float fAcceleration,
float fDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAbsolute(
double dPos,
float fVel,
float fAcceleration,
float fDeceleration,
float fJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_MoveAbsoluteCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveAbsoluteCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_MOVEABSOLUTE_IN(object):
dbPosition =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_dbPosition_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_dbPosition_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fVelocity_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fAcceleration_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fDeceleration_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fJerk_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_eDirection_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_eDirection_set)
eBufferMode =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_eBufferMode_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_eBufferMode_set)
ucExecute =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_ucExecute_get,
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.12 MoveAbsoluteEx

- PDF 페이지: 1810
- 원문 위치: [24.3.12 MoveAbsoluteEx](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1810)
- 기능 설명: 지정한 절대 위치로 축을 이동시키는 모션 API입니다.

#### 시그니처

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
```c
Python Definition def MMC_MoveAbsoluteCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveAbsoluteCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_MOVEABSOLUTE_IN(object):
dbPosition =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_dbPosition_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_dbPosition_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fVelocity_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fAcceleration_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fDeceleration_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fJerk_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_eDirection_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_eDirection_set)
eBufferMode =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_eBufferMode_get,
_mmcpp_lib.MMC_MOVEABSOLUTE_IN_eBufferMode_set)
ucExecute =
property(_mmcpp_lib.MMC_MOVEABSOLUTE_IN_ucExecute_get,
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.13 MoveAdditive

- PDF 페이지: 1812
- 원문 위치: [24.3.13 MoveAdditive](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1812)
- 기능 설명: 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다.

#### 시그니처

```c
int MoveAdditive(
double dbDistance,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAdditive(
double dbDistance,
float fVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAdditive(
double dbDistance,
float fVel,
float fAcceleration,
float fDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAdditive(
double dbDistance,
float fVel,
float fAcceleration,
float fDeceleration,
float fJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_MoveAdditiveCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveAdditiveCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_MOVEADDITIVE_IN(object):
dbDistance =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_dbDistance_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_dbDistance_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_fVelocity_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_fAcceleration_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_fDeceleration_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_fJerk_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_eDirection_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_eDirection_set)
eBufferMode =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_eBufferMode_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_eBufferMode_set)
ucExecute =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_ucExecute_get,
```
```c
void MoveCombinations(void)
// ===========================
{
printf("\n Group of function: %s", __func__);
```
```c
void MoveAdditiveMoves(void)
// ============================
{
MC_BUFFERED_MODE_ENUM eBufferMode;
double dbDistance;
float fVel,
fAcceleration,
fDeceleration,
fJerk;
printf("\n %s:", __func__);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.14 MoveAdditiveEx

- PDF 페이지: 1816
- 원문 위치: [24.3.14 MoveAdditiveEx](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1816)
- 기능 설명: 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다.

#### 시그니처

```c
int MoveAdditiveEx(
double dbDistance,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAdditiveEx(
double dbDistance,
double dVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAdditiveEx(
double dbDistance,
double dVel,
double dAcceleration,
double dDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAdditiveEx(
double dbDistance,
double dVel,
double dAcceleration,
double dDeceleration,
double dJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
def MMC_MoveAdditiveCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveAdditiveCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_MOVEADDITIVE_IN(object):
dbDistance =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_dbDistance_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_dbDistance_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_fVelocity_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_fAcceleration_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_fDeceleration_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_fJerk_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_eDirection_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_eDirection_set)
eBufferMode =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_eBufferMode_get,
_mmcpp_lib.MMC_MOVEADDITIVE_IN_eBufferMode_set)
ucExecute =
property(_mmcpp_lib.MMC_MOVEADDITIVE_IN_ucExecute_get,
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.15 MoveRelative

- PDF 페이지: 1818
- 원문 위치: [24.3.15 MoveRelative](../chunks/071_p1782-p1821_24.3.4-MMCSingleAxis-Class-Functions-Code-Example-3.md#pdf-page-1818)
- 기능 설명: 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다.

#### 시그니처

```c
int MoveRelative(
double dbDistance,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveRelative(
double dbDistance,
float fVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveRelative(
double dbDistance,
float fVel,
float fAcceleration,
float fDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveRelative(
double dbDistance,
float fVel,
float fAcceleration,
float fDeceleration,
float fJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_MoveRelativeCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveRelativeCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_MOVERELATIVE_IN(object):
dbDistance =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_dbDistance_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_dbDistance_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_fVelocity_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_fAcceleration_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_fDeceleration_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_fJerk_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_eDirection_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_eDirection_set)
eBufferMode =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_eBufferMode_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_eBufferMode_set)
ucExecute =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_ucExecute_get,
```
```c
void MoveRelativeVelMoves(void)
// ===============================
{
MC_BUFFERED_MODE_ENUM eBufferMode;
OPM402 drvMode;
double dbDistance;
float fVel,
fAcceleration,
fDeceleration,
fJerk;
printf("\n %s:", __func__);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.16 MoveRelativeEx

- PDF 페이지: 1822
- 원문 위치: [24.3.16 MoveRelativeEx](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1822)
- 기능 설명: 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다.

#### 시그니처

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
```c
def MMC_MoveRelativeCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveRelativeCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_MOVERELATIVE_IN(object):
dbDistance =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_dbDistance_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_dbDistance_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_fVelocity_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_fAcceleration_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_fDeceleration_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_fJerk_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_eDirection_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_eDirection_set)
eBufferMode =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_eBufferMode_get,
_mmcpp_lib.MMC_MOVERELATIVE_IN_eBufferMode_set)
ucExecute =
property(_mmcpp_lib.MMC_MOVERELATIVE_IN_ucExecute_get,
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.17 MoveVelocity

- PDF 페이지: 1824
- 원문 위치: [24.3.17 MoveVelocity](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1824)
- 기능 설명: 지정 속도로 연속 이동을 수행하는 모션 API입니다.

#### 시그니처

```c
int MoveVelocity(
float fVelocity,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_MoveVelocityCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveVelocityCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_MOVEVELOCITY_IN(object):
fVelocity =
property(_mmcpp_lib.MMC_MOVEVELOCITY_IN_fVelocity_get,
_mmcpp_lib.MMC_MOVEVELOCITY_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVEVELOCITY_IN_fAcceleration_get,
_mmcpp_lib.MMC_MOVEVELOCITY_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVEVELOCITY_IN_fDeceleration_get,
_mmcpp_lib.MMC_MOVEVELOCITY_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVEVELOCITY_IN_fJerk_get,
_mmcpp_lib.MMC_MOVEVELOCITY_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVEVELOCITY_IN_eDirection_get,
_mmcpp_lib.MMC_MOVEVELOCITY_IN_eDirection_set)
eBufferMode =
property(_mmcpp_lib.MMC_MOVEVELOCITY_IN_eBufferMode_get,
_mmcpp_lib.MMC_MOVEVELOCITY_IN_eBufferMode_set)
ucExecute =
property(_mmcpp_lib.MMC_MOVEVELOCITY_IN_ucExecute_get,
_mmcpp_lib.MMC_MOVEVELOCITY_IN_ucExecute_set)
class MMC_MOVEVELOCITY_OUT(object):
uiHndl =
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.18 MoveVelocityEx

- PDF 페이지: 1826
- 원문 위치: [24.3.18 MoveVelocityEx](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1826)
- 기능 설명: 지정 속도로 연속 이동을 수행하는 모션 API입니다.

#### 시그니처

```c
int MoveVelocityEx(
float fVelocity,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);?????
```
```c
Python Definition def MMC_MoveVelocityExCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveVelocityExCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_MOVEVELOCITYEX_IN(object):
dVelocity =
property(_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_dVelocity_get,
_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_dVelocity_set)
dAcceleration =
property(_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_dAcceleration_get
, _mmcpp_lib.MMC_MOVEVELOCITYEX_IN_dAcceleration_set)
dDeceleration =
property(_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_dDeceleration_get
, _mmcpp_lib.MMC_MOVEVELOCITYEX_IN_dDeceleration_set)
dJerk =
property(_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_dJerk_get,
_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_dJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_eDirection_get,
_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_eDirection_set)
eBufferMode =
property(_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_eBufferMode_get,
_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_eBufferMode_set)
ucExecute =
property(_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_ucExecute_get,
_mmcpp_lib.MMC_MOVEVELOCITYEX_IN_ucExecute_set)
class MMC_MOVEVELOCITYEX_OUT(object):
uiHndl =
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.19 MoveAbsoluteRepetitive

- PDF 페이지: 1828
- 원문 위치: [24.3.19 MoveAbsoluteRepetitive](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1828)
- 기능 설명: 지정한 절대 위치로 축을 이동시키는 모션 API입니다.

#### 시그니처

```c
int MoveAbsoluteRepetitive(
double dPos,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAbsoluteRepetitive(
double dPos,
float fVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAbsoluteRepetitive(
double dPos,
float fVel,
float fAcceleration,
float fDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAbsoluteRepetitive(
double dPos,
float fVel,
float fAcceleration,
float fDeceleration,
float fJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAbsoluteRepetitive(
double dPos,
float fVel,
unsigned int uiExecDelayMs,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_MoveAbsoluteRepetitiveCmd(hConn, hAxisRef,
pInParam, pOutParam):
return
_mmcpp_lib.MMC_MoveAbsoluteRepetitiveCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_MOVEABSOLUTEREPETITIVE_IN(object):
dbPosition =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_dbPositio
n_get,
_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_dbPosition_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fVelocity
_get,
_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fAccelera
tion_get,
_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fDecelera
tion_get,
_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fJerk_get
, _mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_eDirectio
n_get,
```
```c
void MoveAbsRepetiveMoves(void)
// ===============================
{
MC_BUFFERED_MODE_ENUM eBufferMode;
double dbPos,
minMov;
float fVel,
fAcceleration,
fDeceleration,
fJerk;
unsigned int uiExecDelayMs;
int rtVal;
printf("\n %s:", __func__);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.20 MoveAbsoluteRepetitiveEx

- PDF 페이지: 1832
- 원문 위치: [24.3.20 MoveAbsoluteRepetitiveEx](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1832)
- 기능 설명: 지정한 절대 위치로 축을 이동시키는 모션 API입니다.

#### 시그니처

```c
int MoveAbsoluteRepetitiveEx(
double dPos,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAbsoluteRepetitiveEx(
double dPos,
double dVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAbsoluteRepetitiveEx(
double dPos,
double dVel,
double dAcceleration,
double dDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAbsoluteRepetitiveEx(
double dPos,
double dVel,
double dAcceleration,
double dDeceleration,
double dJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAbsoluteRepetitiveEx(
double dPos,
double dVel,
unsigned int uiExecDelayMs,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
def MMC_MoveAbsoluteRepetitiveCmd(hConn, hAxisRef,
pInParam, pOutParam):
return
_mmcpp_lib.MMC_MoveAbsoluteRepetitiveCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_MOVEABSOLUTEREPETITIVE_IN(object):
dbPosition =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_dbPositio
n_get,
_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_dbPosition_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fVelocity
_get,
_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fAccelera
tion_get,
_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fDecelera
tion_get,
_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fJerk_get
, _mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVEABSOLUTEREPETITIVE_IN_eDirectio
n_get,
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.21 MoveRelativeRepetitive

- PDF 페이지: 1835
- 원문 위치: [24.3.21 MoveRelativeRepetitive](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1835)
- 기능 설명: 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다.

#### 시그니처

```c
int MoveRelativeRepetitive(
double dPos,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveRelativeRepetitive(
double dPos,
float fVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveRelativeRepetitive(
double dPos,
float fVel,
float fAcceleration,
float fDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveRelativeRepetitive(
double dPos,
float fVel,
float fAcceleration,
float fDeceleration,
float fJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveRelativeRepetitive(
double dPos,
float fVel,
unsigned int uiExecDelayMs,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_MoveRelativeRepetitiveCmd(hConn, hAxisRef,
pInParam, pOutParam):
return _mmcpp_lib.MMC_MoveRelativeRepetitiveCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_MOVERELATIVEREPETITIVE_IN(object):
dbDistance =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_dbDistanc
e_get,
_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_dbDistance_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fVelocity
_get,
_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fAccelera
tion_get,
_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fDecelera
tion_get,
_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fJerk_get
, _mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_eDirectio
n_get,
_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_eDirection_set)
```
```c
void MoveRelativeRepetiveMoves(void)
// ====================================
{
MC_BUFFERED_MODE_ENUM eBufferMode;
double dbPos,
minMov;
float fVel,
fAcceleration,
fDeceleration,
fJerk;
unsigned int uiExecDelayMs;
int rtVal;
printf("\n %s:", __func__);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.22 MoveRelativeRepetitiveEx

- PDF 페이지: 1839
- 원문 위치: [24.3.22 MoveRelativeRepetitiveEx](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1839)
- 기능 설명: 현재 위치 또는 실행 시점 기준 상대 거리로 축을 이동시키는 모션 API입니다.

#### 시그니처

```c
int MoveRelativeRepetitiveEx(
double dPos,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveRelativeRepetitiveEx(
double dPos,
double dVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveRelativeRepetitiveEx(
double dPos,
double dVel,
double dAcceleration,
double dDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveRelativeRepetitiveEx(
double dPos,
double dVel,
double dAcceleration,
double dDeceleration,
double dJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveRelativeRepetitiveEx(
double dPos,
double dVel,
unsigned int uiExecDelayMs,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
def MMC_MoveRelativeRepetitiveCmd(hConn, hAxisRef,
pInParam, pOutParam):
return _mmcpp_lib.MMC_MoveRelativeRepetitiveCmd(hConn,
hAxisRef, pInParam, pOutParam)
class MMC_MOVERELATIVEREPETITIVE_IN(object):
dbDistance =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_dbDistanc
e_get,
_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_dbDistance_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fVelocity
_get,
_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fAccelera
tion_get,
_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fDecelera
tion_get,
_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fJerk_get
, _mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_eDirectio
n_get,
_mmcpp_lib.MMC_MOVERELATIVEREPETITIVE_IN_eDirection_set)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.23 MoveAdditiveRepetitive

- PDF 페이지: 1842
- 원문 위치: [24.3.23 MoveAdditiveRepetitive](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1842)
- 기능 설명: 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다.

#### 시그니처

```c
int MoveAdditiveRepetitive(
double dPos,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAdditiveRepetitive(
double dPos,
float fVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAdditiveRepetitive(
double dPos,
float fVel,
float fAcceleration,
float fDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAdditiveRepetitive(
double dPos,
float fVel,
float fAcceleration,
float fDeceleration,
float fJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
int MoveAdditiveRepetitive(
double dPos,
float fVel,
unsigned int uiExecDelayMs,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_MoveAdditiveRepetitiveExCmd(hConn, hAxisRef,
pInParam, pOutParam):
return
_mmcpp_lib.MMC_MoveAdditiveRepetitiveExCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_MOVEADDITIVEREPETITIVE_IN(object):
dbDistance =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_dbDistanc
e_get,
_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_dbDistance_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fVelocity
_get,
_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fAccelera
tion_get,
_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fDecelera
tion_get,
_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fJerk_get
, _mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_eDirectio
n_get,
```
```c
void MoveAdditiveRepetiveMoves(void)
// ====================================
{
MC_BUFFERED_MODE_ENUM eBufferMode;
double dbPos,
minMov;
float fVel,
fAcceleration,
fDeceleration,
fJerk;
unsigned int uiExecDelayMs;
int rtVal;
printf("\n %s:", __func__);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.24 MoveAdditiveRepetitiveEx

- PDF 페이지: 1846
- 원문 위치: [24.3.24 MoveAdditiveRepetitiveEx](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1846)
- 기능 설명: 마지막 명령 위치를 기준으로 가산 거리 이동을 수행하는 모션 API입니다.

#### 시그니처

```c
int MoveAdditiveRepetitiveEx(
double dPos,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAdditiveRepetitiveEx(
double dPos,
double dVel,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAdditiveRepetitiveEx(
double dPos,
double dVel,
double dAcceleration,
double dDeceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAdditiveRepetitiveEx(
double dPos,
double dVel,
double dAcceleration,
double dDeceleration,
double dJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
int MoveAdditiveRepetitiveEx(
double dPos,
double dVel,
unsigned int uiExecDelayMs,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE);
```
```c
def MMC_MoveAdditiveRepetitiveExCmd(hConn, hAxisRef,
pInParam, pOutParam):
return
_mmcpp_lib.MMC_MoveAdditiveRepetitiveExCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_MOVEADDITIVEREPETITIVE_IN(object):
dbDistance =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_dbDistanc
e_get,
_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_dbDistance_set)
fVelocity =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fVelocity
_get,
_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fVelocity_set)
fAcceleration =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fAccelera
tion_get,
_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fAcceleration_set)
fDeceleration =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fDecelera
tion_get,
_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fDeceleration_set)
fJerk =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fJerk_get
, _mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_fJerk_set)
eDirection =
property(_mmcpp_lib.MMC_MOVEADDITIVEREPETITIVE_IN_eDirectio
n_get,
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.25 MoveTorque

- PDF 페이지: 1849
- 원문 위치: [24.3.25 MoveTorque](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1849)
- 기능 설명: 지정 토크로 연속 동작을 수행하는 모션 API입니다.

#### 시그니처

```c
void MoveTorque(
double dbTargetTorque,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
void MoveTorque(
double dbTargetTorque,
double dbTorqeVelocity,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
void MoveTorque(
double dbTargetTorque,
double dbTorqeVelocity,
double dbTorqueAcceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_MoveTorqueCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveTorqueCmd(hConn, hAxisRef,
pInParam, pOutParam)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.26 MoveTorqueEx

- PDF 페이지: 1851
- 원문 위치: [24.3.26 MoveTorqueEx](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1851)
- 기능 설명: 지정 토크로 연속 동작을 수행하는 모션 API입니다.

#### 시그니처

```c
void MoveTorque(
double dbTargetTorque,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
void MoveTorque(
double dbTargetTorque,
double dbTorqeVelocity,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
void MoveTorque(
double dbTargetTorque,
double dbTorqeVelocity,
double dbTorqueAcceleration,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_MoveTorqueCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_MoveTorqueCmd(hConn, hAxisRef,
pInParam, pOutParam)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.27 PositionProfile

- PDF 페이지: 1853
- 원문 위치: [24.3.27 PositionProfile](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1853)
- 기능 설명: 위치 프로파일 작업을 수행하는 API입니다.

#### 시그니처

```c
int PositionProfile(
MC_PATH_REF hMemHandle,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_BUFFERED_MODE
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.28 GetTouchProbeData

- PDF 페이지: 1854
- 원문 위치: [24.3.28 GetTouchProbeData](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1854)
- 기능 설명: 조회 터치 프로브 데이터 값/상태를 조회하는 API입니다.

#### 시그니처

```c
int GetTouchProbeData(
int32_t *plBuffer,
unsigned short usTPIndex,
unsigned short usNumOfPoints
);
```
```c
int GetTouchProbeData(
double *pdBuffer,
unsigned short usTPIndex,
unsigned short usNumOfPoints
);
```
```c
int GetTouchProbeData(
int iDim,
double* pdBuffer,
unsigned short usTPIndex
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.29 TouchProbeDisable

- PDF 페이지: 1859
- 원문 위치: [24.3.29 TouchProbeDisable](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1859)
- 기능 설명: 터치 프로브 비활성화 활성화/비활성화 제어를 수행하는 API입니다.

#### 시그니처

```c
int TouchProbeDisable(
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.30 TouchProbeDisableEx

- PDF 페이지: 1860
- 원문 위치: [24.3.30 TouchProbeDisableEx](../chunks/072_p1822-p1861_24.3.16-MoveRelativeEx.md#pdf-page-1860)
- 기능 설명: 터치 프로브 비활성화 Ex 활성화/비활성화 제어를 수행하는 API입니다.

#### 시그니처

```c
int TouchProbeDisable_Ex(
short sIndex
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.31 TouchProbeEnable

- PDF 페이지: 1862
- 원문 위치: [24.3.31 TouchProbeEnable](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1862)
- 기능 설명: 터치 프로브 활성화 활성화/비활성화 제어를 수행하는 API입니다.

#### 시그니처

```c
int TouchProbeEnable(
unsigned char ucTriggerType
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.32 TouchProbeEnableEx

- PDF 페이지: 1863
- 원문 위치: [24.3.32 TouchProbeEnableEx](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1863)
- 기능 설명: 터치 프로브 활성화 Ex 활성화/비활성화 제어를 수행하는 API입니다.

#### 시그니처

```c
int TouchProbeEnable_Ex(
unsigned long ulShmArraySizeAlloc,
unsigned short usConfig,
unsigned short &usTPIndex
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.33 SetOpMode

- PDF 페이지: 1865
- 원문 위치: [24.3.33 SetOpMode](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1865)
- 기능 설명: 설정 Op 모드 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetOpMode(
OPM402 eMode,
MC_EXECUTION_MODE eExecutionMode,
double dbInitModeValue = 0,
unsigned char ucSkipValidation = 0
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.34 SetOpModeEx

- PDF 페이지: 1866
- 원문 위치: [24.3.34 SetOpModeEx](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1866)
- 기능 설명: 설정 Op 모드 Ex 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetOpMode(
OPM402 eMode,
MC_EXECUTION_MODE eExecutionMode,
double dbInitModeValue = 0,
unsigned char ucSkipValidation = 0
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.35 GetOpMode

- PDF 페이지: 1867
- 원문 위치: [24.3.35 GetOpMode](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1867)
- 기능 설명: 조회 Op 모드 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void SetDefParamsHome(void)
// ==============================
{
MMC_HOMEDS402_IN stDS402Home ;
MMC_HOME_IN stSingleParams;
OPM402 drvMode;
short sErrorID;
printf("\n %s:", __func__);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.36 PowerOn

- PDF 페이지: 1870
- 원문 위치: [24.3.36 PowerOn](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1870)
- 기능 설명: 전원 On 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.37 PowerOff

- PDF 페이지: 1870
- 원문 위치: [24.3.37 PowerOff](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1870)
- 기능 설명: 값 또는 동작 조건을 설정하는 API입니다.

#### 시그니처

```c
void PowerOn(
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
```
```c
void PowerOff(
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.38 SendCmdViaSdoDownload

- PDF 페이지: 1872
- 원문 위치: [24.3.38 SendCmdViaSdoDownload](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1872)
- 기능 설명: 전송 Cmd Via Sdo 다운로드 작업을 수행하는 API입니다.

#### 시그니처

```c
void SendCmdViaSdoDownload(
long lData | float fData,
const char* pcCmdIdx,
unsigned char ucSubIndex=1
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.39 SendCmdViaSdoUpload

- PDF 페이지: 1873
- 원문 위치: [24.3.39 SendCmdViaSdoUpload](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1873)
- 기능 설명: 조회 버전 Ex 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void SendCmdViaSdoUpload (
long& lData | float& fData,
const char* pcCmdIdx,
unsigned char ucSubIndex=1
) throw (CMMCException);
```
```c
int main_body(void)
// ============================
{
int ind;
char axName[4];
MMC_GET_VEREX_OUT t_verex_out;
gt_ConnHndl = gt_Conn.ConnectIPCEx(0x7fffffff, NULL);
```
```c
if (MMC_GetVersionExCmd(gt_ConnHndl, &t_verex_out) != 0)
{
printf("\n *** %s/%s: failed in MMC_GetVersionExCmd() \n", __FILE__,
__func__);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.40 GetActualPosition

- PDF 페이지: 1877
- 원문 위치: [24.3.40 GetActualPosition](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1877)
- 기능 설명: 조회 실제 위치 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.41 GetActualVelocity

- PDF 페이지: 1877
- 원문 위치: [24.3.41 GetActualVelocity](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1877)
- 기능 설명: 조회 실제 속도 값/상태를 조회하는 API입니다.

#### 시그니처

```c
double GetActualPosition(
) throw (CMMCException);
```
```c
double GetActualVelocity(
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.42 GetActualTorque

- PDF 페이지: 1879
- 원문 위치: [24.3.42 GetActualTorque](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1879)
- 기능 설명: 조회 실제 토크 값/상태를 조회하는 API입니다.

#### 시그니처

```c
double GetActualTorque(
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.43 Halt

- PDF 페이지: 1880
- 원문 위치: [24.3.43 Halt](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1880)
- 기능 설명: 축을 정상 운전 조건에서 제어 정지시키는 API입니다.

#### 시그니처

```c
void Halt(
float fDeceleration,
float fJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_HaltCmd(hConn, hAxisRef, pInParam, pOutParam):
return _mmcpp_lib.MMC_HaltCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_HALT_IN(object):
fDeceleration =
property(_mmcpp_lib.MMC_HALT_IN_fDeceleration_get,
_mmcpp_lib.MMC_HALT_IN_fDeceleration_set)
fJerk = property(_mmcpp_lib.MMC_HALT_IN_fJerk_get,
_mmcpp_lib.MMC_HALT_IN_fJerk_set)
eBufferMode =
property(_mmcpp_lib.MMC_HALT_IN_eBufferMode_get,
_mmcpp_lib.MMC_HALT_IN_eBufferMode_set)
ucExecute =
property(_mmcpp_lib.MMC_HALT_IN_ucExecute_get,
_mmcpp_lib.MMC_HALT_IN_ucExecute_set)
class MMC_HALT_OUT(object):
uiHndl =
property(_mmcpp_lib.MMC_HALT_OUT_uiHndl_get,
_mmcpp_lib.MMC_HALT_OUT_uiHndl_set)
usStatus =
property(_mmcpp_lib.MMC_HALT_OUT_usStatus_get,
_mmcpp_lib.MMC_HALT_OUT_usStatus_set)
usErrorID =
property(_mmcpp_lib.MMC_HALT_OUT_usErrorID_get,
_mmcpp_lib.MMC_HALT_OUT_usErrorID_set)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.44 Stop

- PDF 페이지: 1882
- 원문 위치: [24.3.44 Stop](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1882)
- 기능 설명: 축 또는 동작을 정지 상태로 전환하는 API입니다.

#### 시그니처

```c
void Stop(
float fDeceleration, | float fJerk,
MC_BUFFERED_MODE_ENUM eBufferMode = MC_ABORTING_MODE
) throw (CMMCException);
```
```c
Python Definition def MMC_StopCmd(hConn, hAxisRef, pInParam, pOutParam):
return _mmcpp_lib.MMC_StopCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_STOP_IN(object):
fDeceleration =
property(_mmcpp_lib.MMC_STOP_IN_fDeceleration_get,
_mmcpp_lib.MMC_STOP_IN_fDeceleration_set)
fJerk = property(_mmcpp_lib.MMC_STOP_IN_fJerk_get,
_mmcpp_lib.MMC_STOP_IN_fJerk_set)
eBufferMode =
property(_mmcpp_lib.MMC_STOP_IN_eBufferMode_get,
_mmcpp_lib.MMC_STOP_IN_eBufferMode_set)
ucExecute =
property(_mmcpp_lib.MMC_STOP_IN_ucExecute_get,
_mmcpp_lib.MMC_STOP_IN_ucExecute_set)
class MMC_STOP_OUT(object):
uiHndl =
property(_mmcpp_lib.MMC_STOP_OUT_uiHndl_get,
_mmcpp_lib.MMC_STOP_OUT_uiHndl_set)
usStatus =
property(_mmcpp_lib.MMC_STOP_OUT_usStatus_get,
_mmcpp_lib.MMC_STOP_OUT_usStatus_set)
usErrorID =
property(_mmcpp_lib.MMC_STOP_OUT_usErrorID_get,
_mmcpp_lib.MMC_STOP_OUT_usErrorID_set)
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.45 GetDigInput[s]

- PDF 페이지: 1883
- 원문 위치: [24.3.45 GetDigInput[s]](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1883)
- 기능 설명: 조회 Dig 입력 값/상태를 조회하는 API입니다.

#### 시그니처

```c
unsigned char GetDigInput(
int iInputNumber
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.46 GetDigOutputs32Bit

- PDF 페이지: 1884
- 원문 위치: [24.3.46 GetDigOutputs32Bit](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1884)
- 기능 설명: 조회 Dig Outputs32 Bit 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.47 GetDigOutputs

- PDF 페이지: 1884
- 원문 위치: [24.3.47 GetDigOutputs](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1884)
- 기능 설명: 조회 Dig 출력 값/상태를 조회하는 API입니다.

#### 시그니처

```c
unsigned char GetDigOutputs(
int iOutputNumber = 0
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.48 SetDigOutputs32Bit

- PDF 페이지: 1886
- 원문 위치: [24.3.48 SetDigOutputs32Bit](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1886)
- 기능 설명: 설정 Dig Outputs32 Bit 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetDigOutputs32Bit(
const int iOutputNumber,
const uint32_t ulValue
);
```
```c
void SetDigOutputs32Bit(
const uint32_t ulValue
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.49 SetDigOutputs

- PDF 페이지: 1887
- 원문 위치: [24.3.49 SetDigOutputs](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1887)
- 기능 설명: 설정 Dig 출력 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetDigOutputs(
[const int iOutputNumber,]
const unsigned char ucValue
[unsigned char ucEnable]
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.50 SetOverride

- PDF 페이지: 1888
- 원문 위치: [24.3.50 SetOverride](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1888)
- 기능 설명: 설정 오버라이드 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void SetOverride(
float fAccFactor,
float fJerkFactor,
float fVelFactor
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.51 ConfigPDO

- PDF 페이지: 1889
- 원문 위치: [24.3.51 ConfigPDO](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1889)
- 기능 설명: 구성 PDO 작업을 수행하는 API입니다.

#### 시그니처

```c
void ConfigPDO(
PDO_NUMBER_ENUM ePDONum,
PDO_PARAM_TYPE_ENUM eParamType,
unsigned int uiPDOCommParamEvent,
unsigned short usEventTimer,
unsigned char ucEventGroup,
unsigned char ucPDOCommParam,
unsigned char ucSubIndex,
unsigned char ucPDOType
)throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.52 CancelPDO

- PDF 페이지: 1891
- 원문 위치: [24.3.52 CancelPDO](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1891)
- 기능 설명: Cancel PDO 작업을 수행하는 API입니다.

#### 시그니처

```c
void CancelPDO(
PDO_NUMBER_ENUM ePDONum
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.53 ChangeDefaultPDOConfig

- PDF 페이지: 1892
- 원문 위치: [24.3.53 ChangeDefaultPDOConfig](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1892)
- 기능 설명: Change Default PDOConfig 작업을 수행하는 API입니다.

#### 시그니처

```c
void ChangeDefaultPDOConfig(
unsigned char ucPDONum,
unsigned char ucPDODir,
unsigned char ucPDOCommParam
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.54 ElmoSetAsyncAn array

- PDF 페이지: 1893
- 원문 위치: [24.3.54 ElmoSetAsyncAn array](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1893)
- 기능 설명: Elmo 설정 비동기 Array 값/설정을 적용하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.55 ElmoSetAsyncParam

- PDF 페이지: 1894
- 원문 위치: [24.3.55 ElmoSetAsyncParam](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1894)
- 기능 설명: Elmo 설정 비동기 파라미터 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void ElmoSetAsyncParam(
char cCmd[3],
int& iVal | float& fVal
) throw (CMMCException);
```
```c
void SetGetDrvParameters(void)
// ==============================
{
int iValGet,
iValSet;
float fValGet;
int loopCount,
iMax;
char* cPdrvCmd;
printf("\n %s:", __func__);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.56 ElmoGetAsyncIntParam

- PDF 페이지: 1896
- 원문 위치: [24.3.56 ElmoGetAsyncIntParam](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1896)
- 기능 설명: Elmo 조회 비동기 Int 파라미터 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void ElmoGetAsyncIntParam(
char cCmd[3]
) throw (CMMCException);
```
```c
void SetGetDrvParameters(void)
// ==============================
{
int iValGet,
iValSet;
float fValGet;
int loopCount,
iMax;
char* cPdrvCmd;
printf("\n %s:", __func__);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.57 ElmoGetAsyncFloatParam

- PDF 페이지: 1898
- 원문 위치: [24.3.57 ElmoGetAsyncFloatParam](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1898)
- 기능 설명: Elmo 조회 비동기 Float 파라미터 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void ElmoGetAsyncFloatParam(
char cCmd[3]
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.58 ElmoGetAsyncIntAn array

- PDF 페이지: 1899
- 원문 위치: [24.3.58 ElmoGetAsyncIntAn array](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1899)
- 기능 설명: Elmo 조회 비동기 Int Array 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.59 ElmoGetAsyncFloatAn array

- PDF 페이지: 1900
- 원문 위치: [24.3.59 ElmoGetAsyncFloatAn array](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1900)
- 기능 설명: Elmo 조회 비동기 Float Array 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.60 ElmoGetSyncParam

- PDF 페이지: 1901
- 원문 위치: [24.3.60 ElmoGetSyncParam](../chunks/073_p1862-p1901_24.3.31-TouchProbeEnable.md#pdf-page-1901)
- 기능 설명: Elmo 조회 동기 파라미터 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void ElmoGetSyncParam(
char cCmd[3],
float& fVal | int& iVal
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.61 ElmoGetSyncAn array

- PDF 페이지: 1902
- 원문 위치: [24.3.61 ElmoGetSyncAn array](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1902)
- 기능 설명: Elmo 조회 동기 Array 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.62 ElmoCallAsync

- PDF 페이지: 1903
- 원문 위치: [24.3.62 ElmoCallAsync](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1903)
- 기능 설명: Elmo 호출 비동기 작업을 수행하는 API입니다.

#### 시그니처

```c
void ElmoCallAsync(
char cCmd[3]
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.63 ElmoExecute

- PDF 페이지: 1904
- 원문 위치: [24.3.63 ElmoExecute](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1904)
- 기능 설명: Elmo 실행 작업을 수행하는 API입니다.

#### 시그니처

```c
void ElmoExecute(
unsigned char* pData,
unsigned char ucLength
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.64 ElmoIsReplyAwaiting

- PDF 페이지: 1905
- 원문 위치: [24.3.64 ElmoIsReplyAwaiting](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1905)
- 기능 설명: Elmo Is Reply Awaiting 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.65 ElmoGetReply

- PDF 페이지: 1905
- 원문 위치: [24.3.65 ElmoGetReply](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1905)
- 기능 설명: Elmo 조회 Reply 값/상태를 조회하는 API입니다.

#### 시그니처

```c
int ElmoIsReplyAwaiting(
) throw (CMMCException);
```
```c
void ElmoGetReply(
float& fVal | int& iVal
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.66 ConfigVirtualEncoder

- PDF 페이지: 1906
- 원문 위치: [24.3.66 ConfigVirtualEncoder](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1906)
- 기능 설명: 구성 Virtual Encoder 작업을 수행하는 API입니다.

#### 시그니처

```c
void ConfigVirtualEncoder(
double dbLowPos,
double dbHighPos,
float fFactor,
unsigned char ucMode,
unsigned char ucGroupID
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.67 CancelVirtualEncoder

- PDF 페이지: 1908
- 원문 위치: [24.3.67 CancelVirtualEncoder](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1908)
- 기능 설명: Cancel Virtual Encoder 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.68 SetPosition

- PDF 페이지: 1908
- 원문 위치: [24.3.68 SetPosition](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1908)
- 기능 설명: 설정 위치 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void CancelVirtualEncoder(
) throw (CMMCException);
```
```c
void SetPosition(
double dbPosition,
double dbModulus,
unsigned char ucPosMode
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.69 SetParameter

- PDF 페이지: 1910
- 원문 위치: [24.3.69 SetParameter](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1910)
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

### 24.3.70 AxisLink

- PDF 페이지: 1911
- 원문 위치: [24.3.70 AxisLink](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1911)
- 기능 설명: 축 Link 작업을 수행하는 API입니다.

#### 시그니처

```c
void AxisLink(
unsigned short usAxisRef,
unsigned char ucMode = 0
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.71 AxisUnLink

- PDF 페이지: 1912
- 원문 위치: [24.3.71 AxisUnLink](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1912)
- 기능 설명: 축 Un Link 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.72 GetBoolParameter

- PDF 페이지: 1912
- 원문 위치: [24.3.72 GetBoolParameter](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1912)
- 기능 설명: 조회 불리언 파라미터 값/상태를 조회하는 API입니다.

#### 시그니처

```c
void AxisUnLink(
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.73 SetBoolParameter

- PDF 페이지: 1914
- 원문 위치: [24.3.73 SetBoolParameter](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1914)
- 기능 설명: 설정 불리언 파라미터 값/설정을 적용하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.74 GetParameter

- PDF 페이지: 1915
- 원문 위치: [24.3.74 GetParameter](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1915)
- 기능 설명: 조회 파라미터 값/상태를 조회하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.75 SetProfileConditioning

- PDF 페이지: 1916
- 원문 위치: [24.3.75 SetProfileConditioning](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1916)
- 기능 설명: 설정 프로파일 컨디셔닝 값/설정을 적용하는 API입니다.

#### 시그니처

```c
int SetProfileConditioning(
MMC_PROFILECOND_IN& i_params
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 24.3.76 GetFbDepth

- PDF 페이지: 1919
- 원문 위치: [24.3.76 GetFbDepth](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1919)
- 기능 설명: 조회 Fb 깊이 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_MarkFbFree(
IN MMC_CONNECT_HNDL hConn,
IN MMC_MARKFBFREE_IN* pInParam,
OUT MMC_MARKFBFREE_OUT* pOutParam
);
```
```c
unsigned int GetFbDepth(
) throw (CMMCException);
```
```c
Python Definition def MMC_GetFbDepthCmd(hConn, hAxisRef, pInParam,
pOutParam):
return _mmcpp_lib.MMC_GetFbDepthCmd(hConn, hAxisRef,
pInParam, pOutParam)
class MMC_GETFBDEPTH_IN(object):
uiHndl =
property(_mmcpp_lib.MMC_GETFBDEPTH_IN_uiHndl_get,
_mmcpp_lib.MMC_GETFBDEPTH_IN_uiHndl_set)
class MMC_GETFBDEPTH_OUT(object):
uiFbInQ =
property(_mmcpp_lib.MMC_GETFBDEPTH_OUT_uiFbInQ_get,
_mmcpp_lib.MMC_GETFBDEPTH_OUT_uiFbInQ_set)
usStatus =
property(_mmcpp_lib.MMC_GETFBDEPTH_OUT_usStatus_get,
_mmcpp_lib.MMC_GETFBDEPTH_OUT_usStatus_set)
usErrorID =
property(_mmcpp_lib.MMC_GETFBDEPTH_OUT_usErrorID_get,
_mmcpp_lib.MMC_GETFBDEPTH_OUT_usErrorID_set)
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

### 24.3.77 GetStatusRegister

- PDF 페이지: 1923
- 원문 위치: [24.3.77 GetStatusRegister](../chunks/074_p1902-p1941_24.3.61-ElmoGetSyncAn-array.md#pdf-page-1923)
- 기능 설명: 조회 상태 등록 값/상태를 조회하는 API입니다.

#### 시그니처

```c
unsigned int GetStatusRegister(
);
```
```c
unsigned int GetStatusRegister(
MMC_GETSTATUSREGISTER_OUT& sOutput
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

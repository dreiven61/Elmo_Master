# 12.5 Data Recording Functions - API 분석

- 원본 장: `Chapter 12 Data Recording`
- 시작 PDF 페이지: 1222
- 원문 위치: [12.5 Data Recording Functions](../chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1222)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `12.5.1` | 1222 | `MMC_BeginRecordingCmd` | Begin 기록 작업을 수행하는 API입니다. | Motion Mode NC - N/A Distributed - N/A |
| `12.5.2` | 1226 | `MMC_StopRecordingCmd` | 축 또는 동작을 정지 상태로 전환하는 API입니다. | Motion Mode NC - N/A Distributed - N/A |
| `12.5.3` | 1228 | `MMC_UploadDataCmd` | 업로드 데이터 작업을 수행하는 API입니다. | Motion Mode NC - N/A Distributed - N/A |
| `12.5.4` | 1231 | `MMC_RecStatusCmd` | Rec 상태 작업을 수행하는 API입니다. | Motion Mode NC - N/A Distributed - N/A |
| `12.5.5` | 1234 | `MMC_UploadDataHeaderCmd` | 업로드 데이터 Header 작업을 수행하는 API입니다. | Motion Mode NC - N/A Distributed - N/A |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 12.5.1 MMC_BeginRecording

- PDF 페이지: 1222
- 원문 위치: [12.5.1 MMC_BeginRecording](../chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1222)
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

#### 구조체/인자

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

### 12.5.2 MMC_StopRecording

- PDF 페이지: 1226
- 원문 위치: [12.5.2 MMC_StopRecording](../chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1226)
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

#### 구조체/인자

##### `MMC_STOP_RECORDING_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char dummy;` | dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_STOP_RECORDING_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 12.5.3 MMC_UploadData

- PDF 페이지: 1228
- 원문 위치: [12.5.3 MMC_UploadData](../chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1228)
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

#### 구조체/인자

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

### 12.5.4 MMC_RecStatus

- PDF 페이지: 1231
- 원문 위치: [12.5.4 MMC_RecStatus](../chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1231)
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

#### 구조체/인자

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

### 12.5.5 MMC_UploadDataHeader

- PDF 페이지: 1234
- 원문 위치: [12.5.5 MMC_UploadDataHeader](../chunks/047_p1207-p1237_Chapter-12-Data-Recording.md#pdf-page-1234)
- 기능 설명: 업로드 데이터 Header 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - N/A Distributed - N/A

#### 시그니처

```c
MMC_LIB_API int MMC_UploadDataHeaderCmd(
IN MMC_CONNECT_HNDL hConn,
OUT NC_UPLOAD_REC_HEADER_STRUCT* pOutParam
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

# 16.2 The MMCUserParams C++ Class - API 분석

- 원본 장: `Chapter 16 Saving Maestro User Program Parameters`
- 시작 PDF 페이지: 1328
- 원문 위치: [16.2 The MMCUserParams C++ Class](../chunks/053_p1327-p1343_Chapter-16-Saving-Maestro-User-Program-Parameters.md#pdf-page-1328)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `16.2.1` | 1330 | `Open` | 열기 작업을 수행하는 API입니다. | - |
| `16.2.2` | 1331 | `Close` | 닫기 작업을 수행하는 API입니다. | - |
| `16.2.3` | 1332 | `Read` | 읽기 값/상태를 조회하는 API입니다. | - |
| `16.2.4` | 1335 | `GetXmlFileRoot` | 조회 Xml 파일 Root 값/상태를 조회하는 API입니다. | - |
| `16.2.5` | 1336 | `GetXmlFileDescrp` | 조회 Xml 파일 Descrp 값/상태를 조회하는 API입니다. | - |
| `16.2.6` | 1337 | `SetSpeakDbgLvl` | 설정 Speak Dbg Lvl 값/설정을 적용하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 16.2.1 Open

- PDF 페이지: 1330
- 원문 위치: [16.2.1 Open](../chunks/053_p1327-p1343_Chapter-16-Saving-Maestro-User-Program-Parameters.md#pdf-page-1330)
- 기능 설명: 열기 작업을 수행하는 API입니다.

#### 시그니처

```c
int Open(
char* cFileName=DEFAULT_XML_FILE_NAME,
unsigned int uiFlags=UPXML_SET_DEF_REQ_FLG,
char* cFilePath=DEFAULT_XML_FILE_PATH
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 16.2.2 Close

- PDF 페이지: 1331
- 원문 위치: [16.2.2 Close](../chunks/053_p1327-p1343_Chapter-16-Saving-Maestro-User-Program-Parameters.md#pdf-page-1331)
- 기능 설명: 닫기 작업을 수행하는 API입니다.

#### 시그니처

```c
int Close(
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 16.2.3 Read

- PDF 페이지: 1332
- 원문 위치: [16.2.3 Read](../chunks/053_p1327-p1343_Chapter-16-Saving-Maestro-User-Program-Parameters.md#pdf-page-1332)
- 기능 설명: 읽기 값/상태를 조회하는 API입니다.

#### 시그니처

```c
int Read (
char* pCtgryVal,
char*.pRsrcVal,
char* pTagName,
double &dVal, | long &lVal, | Bool &bVal, | char* pStr,
double dDefault, | long lDefault, | Bool bDefault=0
double dMin=DBL_MIN, | long lMin=LONG_MIN,
double dMax=DBL_MAX, | long lMax=LONG_MAX, |
long lLen,
) throw (CMMCException);
```
```c
int ReadArr (
char* pCtgryVal,
char*.pRsrcVal,
char* pTagName,
double dVal[], | long lVal[],
double dDefault, | long lDefault,
unsigned int& iActRdElm,
unsigned int iReqRdElm=1,
double dMin=DBL_MIN, |long lMin=LONG_MIN,
double dMax=DBL_MAX, | long lMax=LONG_MAX,
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 16.2.4 GetXmlFileRoot

- PDF 페이지: 1335
- 원문 위치: [16.2.4 GetXmlFileRoot](../chunks/053_p1327-p1343_Chapter-16-Saving-Maestro-User-Program-Parameters.md#pdf-page-1335)
- 기능 설명: 조회 Xml 파일 Root 값/상태를 조회하는 API입니다.

#### 시그니처

```c
int GetXmlFileRoot (
char* pAtt1,
char* pAtt2,
long lLen
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 16.2.5 GetXmlFileDescrp

- PDF 페이지: 1336
- 원문 위치: [16.2.5 GetXmlFileDescrp](../chunks/053_p1327-p1343_Chapter-16-Saving-Maestro-User-Program-Parameters.md#pdf-page-1336)
- 기능 설명: 조회 Xml 파일 Descrp 값/상태를 조회하는 API입니다.

#### 시그니처

```c
int GetXmlFileDescrp(
char* pAtt1,
char* pAtt2,
long lLen
) throw (CMMCException);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 16.2.6 SetSpeakDbgLvl

- PDF 페이지: 1337
- 원문 위치: [16.2.6 SetSpeakDbgLvl](../chunks/053_p1327-p1343_Chapter-16-Saving-Maestro-User-Program-Parameters.md#pdf-page-1337)
- 기능 설명: 설정 Speak Dbg Lvl 값/설정을 적용하는 API입니다.

#### 시그니처

```c
void setSpeakDbgLvl (
unsigned int uiSpeak_lvl
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

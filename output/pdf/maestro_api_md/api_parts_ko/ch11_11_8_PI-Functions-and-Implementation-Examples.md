# 11.8 PI Functions and Implementation Examples - API 분석

- 원본 장: `Chapter 11 Process Image(PI)`
- 시작 PDF 페이지: 1201
- 원문 위치: [11.8 PI Functions and Implementation Examples](../chunks/045_p1172-p1205_11.6.26-MMC_WritePIVarDouble.md#pdf-page-1201)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `11.8.3` | 1206 | `PI Full Example in C, and IEC with EtherCAT Configuration Settings` | Process Image Full Example in C, and IEC with Ether CAT Configuration Settings 값/설정을 적용하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 11.8.3 PI Full Example in C, and IEC with EtherCAT Configuration Settings

- PDF 페이지: 1206
- 원문 위치: [11.8.3 PI Full Example in C, and IEC with EtherCAT Configuration Settings](../chunks/046_p1206-p1206_11.8.3-PI-Full-Example-in-C-and-IEC-with-EtherCAT-Configuration-Settings.md#pdf-page-1206)
- 기능 설명: Process Image Full Example in C, and IEC with Ether CAT Configuration Settings 값/설정을 적용하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

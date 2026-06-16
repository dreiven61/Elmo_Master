# 22.4 MMC_ElmoGetParameter - API 분석

- 원본 장: `Chapter 22 Interpreter Command Functions`
- 시작 PDF 페이지: 1623
- 원문 위치: [22.4 MMC_ElmoGetParameter](../chunks/065_p1615-p1646_Chapter-22-Interpreter-Command-Functions.md#pdf-page-1623)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `22.4` | 1623 | `MMC_ElmoGetParameter` | Elmo 조회 파라미터 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Supported |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 22.4 MMC_ElmoGetParameter

- PDF 페이지: 1623
- 원문 위치: [22.4 MMC_ElmoGetParameter](../chunks/065_p1615-p1646_Chapter-22-Interpreter-Command-Functions.md#pdf-page-1623)
- 기능 설명: Elmo 조회 파라미터 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Supported

#### 시그니처

```c
MMC_LIB_API int MMC_ElmoGetParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN char cCmd[3],
OUT unsigned char ucValType
);
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

# 8.4 Polynomial Interpolation Functions - API 분석

- 원본 장: `Chapter 8 Position, Velocity, Time (PVT) Motion`
- 시작 PDF 페이지: 848
- 원문 위치: [8.4 Polynomial Interpolation Functions](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-848)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `8.4.1` | 848 | `Cubic polynomial - Polynomial Order 3 (eCUBIC_POLYNOM)` | Cubic polynomial - Polynomial Order 3 (e CUBIC POLYNOM) 작업을 수행하는 API입니다. | - |
| `8.4.2` | 849 | `Quintic polynomial - Polynomial Order 5 (eQUINTIC_ON_CUBIC)` | Quintic polynomial - Polynomial Order 5 (e QUINTIC ON CUBIC) 작업을 수행하는 API입니다. | - |
| `8.4.3` | 849 | `Septic polynomial - Polynomial Order 7 (eSEPTIC_ON_CUBIC)` | Septic polynomial - Polynomial Order 7 (e SEPTIC ON CUBIC) 작업을 수행하는 API입니다. | - |
| `8.4.4` | 850 | `Sinusoidal interpolation functions` | Sinusoidal interpolation functions 작업을 수행하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 8.4.1 Cubic polynomial - Polynomial Order 3 (eCUBIC_POLYNOM)

- PDF 페이지: 848
- 원문 위치: [8.4.1 Cubic polynomial - Polynomial Order 3 (eCUBIC_POLYNOM)](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-848)
- 기능 설명: Cubic polynomial - Polynomial Order 3 (e CUBIC POLYNOM) 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 8.4.2 Quintic polynomial - Polynomial Order 5 (eQUINTIC_ON_CUBIC)

- PDF 페이지: 849
- 원문 위치: [8.4.2 Quintic polynomial - Polynomial Order 5 (eQUINTIC_ON_CUBIC)](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-849)
- 기능 설명: Quintic polynomial - Polynomial Order 5 (e QUINTIC ON CUBIC) 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 8.4.3 Septic polynomial - Polynomial Order 7 (eSEPTIC_ON_CUBIC)

- PDF 페이지: 849
- 원문 위치: [8.4.3 Septic polynomial - Polynomial Order 7 (eSEPTIC_ON_CUBIC)](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-849)
- 기능 설명: Septic polynomial - Polynomial Order 7 (e SEPTIC ON CUBIC) 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 8.4.4 Sinusoidal interpolation functions

- PDF 페이지: 850
- 원문 위치: [8.4.4 Sinusoidal interpolation functions](../chunks/032_p0847-p0885_8.3-PVT-Interpolation-Mode.md#pdf-page-850)
- 기능 설명: Sinusoidal interpolation functions 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

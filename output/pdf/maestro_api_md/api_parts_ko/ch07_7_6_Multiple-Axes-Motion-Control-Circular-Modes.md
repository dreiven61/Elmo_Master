# 7.6 Multiple Axes Motion Control - Circular Modes - API 분석

- 원본 장: `Chapter 7 Motion and Administrative - Multi-Axis`
- 시작 PDF 페이지: 588
- 원문 위치: [7.6 Multiple Axes Motion Control - Circular Modes](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-588)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `7.6.1` | 589 | `Border mode` | Border mode 작업을 수행하는 API입니다. | - |
| `7.6.2` | 590 | `Center mode` | Center mode 작업을 수행하는 API입니다. | - |
| `7.6.3` | 591 | `Radius mode` | Radius mode 작업을 수행하는 API입니다. | - |
| `7.6.4` | 592 | `Angle Mode` | Angle 모드 작업을 수행하는 API입니다. | - |
| `7.6.5` | 593 | `PathChoice Data verification in MoveCircular functions` | 경로 Choice 데이터 verification in 이동 원호 functions 작업을 수행하는 API입니다. | - |
| `7.6.6` | 594 | `Move Polynomial Function Block` | 이동 Polynomial 함수 블록 작업을 수행하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 7.6.1 Border mode

- PDF 페이지: 589
- 원문 위치: [7.6.1 Border mode](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-589)
- 기능 설명: Border mode 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.6.2 Center mode

- PDF 페이지: 590
- 원문 위치: [7.6.2 Center mode](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-590)
- 기능 설명: Center mode 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.6.3 Radius mode

- PDF 페이지: 591
- 원문 위치: [7.6.3 Radius mode](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-591)
- 기능 설명: Radius mode 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.6.4 Angle Mode

- PDF 페이지: 592
- 원문 위치: [7.6.4 Angle Mode](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-592)
- 기능 설명: Angle 모드 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.6.5 PathChoice Data verification in MoveCircular functions

- PDF 페이지: 593
- 원문 위치: [7.6.5 PathChoice Data verification in MoveCircular functions](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-593)
- 기능 설명: 경로 Choice 데이터 verification in 이동 원호 functions 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.6.6 Move Polynomial Function Block

- PDF 페이지: 594
- 원문 위치: [7.6.6 Move Polynomial Function Block](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-594)
- 기능 설명: 이동 Polynomial 함수 블록 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

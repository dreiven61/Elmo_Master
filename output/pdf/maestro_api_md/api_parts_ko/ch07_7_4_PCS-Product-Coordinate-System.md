# 7.4 PCS - Product Coordinate System - API 분석

- 원본 장: `Chapter 7 Motion and Administrative - Multi-Axis`
- 시작 PDF 페이지: 511
- 원문 위치: [7.4 PCS - Product Coordinate System](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-511)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `7.4.1` | 513 | `Tracking in Dynamic Coordinate Transformations` | Tracking in Dynamic Coordinate 변환 작업을 수행하는 API입니다. | - |
| `7.4.2` | 514 | `Using the function MMC_SetKinTransformEx` | Using the function Maestro 설정 Kin 변환 Ex 값/설정을 적용하는 API입니다. | - |
| `7.4.3` | 517 | `Implementation` | Implementation 작업을 수행하는 API입니다. | - |
| `7.4.5` | 519 | `Example of Set Kinematic` | Example of 설정 키네마틱 값/설정을 적용하는 API입니다. | - |
| `7.4.6` | 520 | `Example - ACS Motion` | Example - ACS Motion 작업을 수행하는 API입니다. | - |
| `7.4.7` | 521 | `Example - MCS Motion` | Example - MCS Motion 작업을 수행하는 API입니다. | - |
| `7.5.1` | 523 | `Delta Robot kinematic` | Delta 로봇 kinematic 작업을 수행하는 API입니다. | - |
| `7.5.2` | 532 | `SCARA Robot kinematic` | SCARA 로봇 kinematic 작업을 수행하는 API입니다. | - |
| `7.5.3` | 536 | `Three Link Robot Kinematic` | Three Link 로봇 키네마틱 작업을 수행하는 API입니다. | - |
| `7.5.4` | 540 | `Dual Head Robot Kinematic` | Dual Head 로봇 키네마틱 작업을 수행하는 API입니다. | - |
| `7.5.5` | 541 | `Hexapod Platform Kinematic` | Hexapod Platform 키네마틱 작업을 수행하는 API입니다. | - |
| `7.5.6` | 544 | `Robot Tansformations Error IDs` | 값 또는 동작 조건을 설정하는 API입니다. | - |
| `7.5.7` | 547 | `Tracking System Functions` | Tracking 시스템 Functions 작업을 수행하는 API입니다. | - |
| `7.5.8` | 549 | `MC_TrackConveyorBelt Function Description` | Track Conveyor Belt 함수 Description 작업을 수행하는 API입니다. | - |
| `7.5.9` | 550 | `Tracking Workpiece Processing on a Conveyor Belt` | Tracking Workpiece Processing on a Conveyor Belt 작업을 수행하는 API입니다. | - |
| `7.5.10` | 552 | `MC_TrackRotaryTable Function Description` | Track Rotary 테이블 함수 Description 작업을 수행하는 API입니다. | - |
| `7.5.11` | 553 | `Tracking Work Part Processing on a Rotary Table` | Tracking Work Part Processing on a Rotary 테이블 작업을 수행하는 API입니다. | - |
| `7.5.12` | 554 | `Multiple Axes Motion Control - Transition and Buffer Modes` | Multiple 축 Motion Control - Transition and Buffer Modes 작업을 수행하는 API입니다. | - |
| `7.5.13` | 555 | `Single Axis Buffer Modes` | 값 또는 동작 조건을 설정하는 API입니다. | - |
| `7.5.14` | 556 | `Multi-Axes Transitions` | Multi-Axes Transitions 작업을 수행하는 API입니다. | - |
| `7.5.15` | 557 | `Matrix of Available Transition Modes` | Matrix of Available Transition Modes 작업을 수행하는 API입니다. | - |
| `7.5.16` | 583 | `Obtaining the 'S' Position of a Vertex using Transition Modes 18 & 19` | 값 또는 동작 조건을 설정하는 API입니다. | - |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 7.4.1 Tracking in Dynamic Coordinate Transformations

- PDF 페이지: 513
- 원문 위치: [7.4.1 Tracking in Dynamic Coordinate Transformations](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-513)
- 기능 설명: Tracking in Dynamic Coordinate 변환 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.4.2 Using the function MMC_SetKinTransformEx

- PDF 페이지: 514
- 원문 위치: [7.4.2 Using the function MMC_SetKinTransformEx](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-514)
- 기능 설명: Using the function Maestro 설정 Kin 변환 Ex 값/설정을 적용하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.4.3 Implementation

- PDF 페이지: 517
- 원문 위치: [7.4.3 Implementation](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-517)
- 기능 설명: Implementation 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.4.5 Example of Set Kinematic

- PDF 페이지: 519
- 원문 위치: [7.4.5 Example of Set Kinematic](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-519)
- 기능 설명: Example of 설정 키네마틱 값/설정을 적용하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.4.6 Example - ACS Motion

- PDF 페이지: 520
- 원문 위치: [7.4.6 Example - ACS Motion](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-520)
- 기능 설명: Example - ACS Motion 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.4.7 Example - MCS Motion

- PDF 페이지: 521
- 원문 위치: [7.4.7 Example - MCS Motion](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-521)
- 기능 설명: Example - MCS Motion 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.1 Delta Robot kinematic

- PDF 페이지: 523
- 원문 위치: [7.5.1 Delta Robot kinematic](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-523)
- 기능 설명: Delta 로봇 kinematic 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.2 SCARA Robot kinematic

- PDF 페이지: 532
- 원문 위치: [7.5.2 SCARA Robot kinematic](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-532)
- 기능 설명: SCARA 로봇 kinematic 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.3 Three Link Robot Kinematic

- PDF 페이지: 536
- 원문 위치: [7.5.3 Three Link Robot Kinematic](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-536)
- 기능 설명: Three Link 로봇 키네마틱 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.4 Dual Head Robot Kinematic

- PDF 페이지: 540
- 원문 위치: [7.5.4 Dual Head Robot Kinematic](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-540)
- 기능 설명: Dual Head 로봇 키네마틱 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.5 Hexapod Platform Kinematic

- PDF 페이지: 541
- 원문 위치: [7.5.5 Hexapod Platform Kinematic](../chunks/022_p0504-p0543_7.1-Coordinate-System-and-kinematic-transformation.md#pdf-page-541)
- 기능 설명: Hexapod Platform 키네마틱 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.6 Robot Tansformations Error IDs

- PDF 페이지: 544
- 원문 위치: [7.5.6 Robot Tansformations Error IDs](../chunks/023_p0544-p0582_7.5.6-Robot-Tansformations-Error-IDs.md#pdf-page-544)
- 기능 설명: 값 또는 동작 조건을 설정하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.7 Tracking System Functions

- PDF 페이지: 547
- 원문 위치: [7.5.7 Tracking System Functions](../chunks/023_p0544-p0582_7.5.6-Robot-Tansformations-Error-IDs.md#pdf-page-547)
- 기능 설명: Tracking 시스템 Functions 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.8 MC_TrackConveyorBelt Function Description

- PDF 페이지: 549
- 원문 위치: [7.5.8 MC_TrackConveyorBelt Function Description](../chunks/023_p0544-p0582_7.5.6-Robot-Tansformations-Error-IDs.md#pdf-page-549)
- 기능 설명: Track Conveyor Belt 함수 Description 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.9 Tracking Workpiece Processing on a Conveyor Belt

- PDF 페이지: 550
- 원문 위치: [7.5.9 Tracking Workpiece Processing on a Conveyor Belt](../chunks/023_p0544-p0582_7.5.6-Robot-Tansformations-Error-IDs.md#pdf-page-550)
- 기능 설명: Tracking Workpiece Processing on a Conveyor Belt 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.10 MC_TrackRotaryTable Function Description

- PDF 페이지: 552
- 원문 위치: [7.5.10 MC_TrackRotaryTable Function Description](../chunks/023_p0544-p0582_7.5.6-Robot-Tansformations-Error-IDs.md#pdf-page-552)
- 기능 설명: Track Rotary 테이블 함수 Description 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.11 Tracking Work Part Processing on a Rotary Table

- PDF 페이지: 553
- 원문 위치: [7.5.11 Tracking Work Part Processing on a Rotary Table](../chunks/023_p0544-p0582_7.5.6-Robot-Tansformations-Error-IDs.md#pdf-page-553)
- 기능 설명: Tracking Work Part Processing on a Rotary 테이블 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.12 Multiple Axes Motion Control - Transition and Buffer Modes

- PDF 페이지: 554
- 원문 위치: [7.5.12 Multiple Axes Motion Control - Transition and Buffer Modes](../chunks/023_p0544-p0582_7.5.6-Robot-Tansformations-Error-IDs.md#pdf-page-554)
- 기능 설명: Multiple 축 Motion Control - Transition and Buffer Modes 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.13 Single Axis Buffer Modes

- PDF 페이지: 555
- 원문 위치: [7.5.13 Single Axis Buffer Modes](../chunks/023_p0544-p0582_7.5.6-Robot-Tansformations-Error-IDs.md#pdf-page-555)
- 기능 설명: 값 또는 동작 조건을 설정하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.14 Multi-Axes Transitions

- PDF 페이지: 556
- 원문 위치: [7.5.14 Multi-Axes Transitions](../chunks/023_p0544-p0582_7.5.6-Robot-Tansformations-Error-IDs.md#pdf-page-556)
- 기능 설명: Multi-Axes Transitions 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.15 Matrix of Available Transition Modes

- PDF 페이지: 557
- 원문 위치: [7.5.15 Matrix of Available Transition Modes](../chunks/023_p0544-p0582_7.5.6-Robot-Tansformations-Error-IDs.md#pdf-page-557)
- 기능 설명: Matrix of Available Transition Modes 작업을 수행하는 API입니다.

#### 시그니처


#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.5.16 Obtaining the 'S' Position of a Vertex using Transition Modes 18 & 19

- PDF 페이지: 583
- 원문 위치: [7.5.16 Obtaining the 'S' Position of a Vertex using Transition Modes 18 & 19](../chunks/024_p0583-p0619_7.5.16-Obtaining-the-S-Position-of-a-Vertex-using-Transition-Modes-18-19.md#pdf-page-583)
- 기능 설명: 값 또는 동작 조건을 설정하는 API입니다.

#### 시그니처

```c
int MMC_GetMotionInfo(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOTIONINFO_IN* pInParam,
OUT MMC_MOTIONINFO_OUT* pOutParam)
The following describes the library implementation, with an explanation for each variable in the input and
output structure.
Input structure:
typedef enum
{
eMOTION_INFO_DATA,
}MOTION_INFO_ENUM;
typedef struct mmc_getmotioninfo_in
{
MOTION_INFO_ENUM eMotionInfo;
}MMC_MOTIONINFO_IN;
The user have the ability to select which type of information he would like to retrieve according to the
MOTION_INFO_ENUM , today we have only one option.
Output structure:
typedef struct mmc_getmotioninfo_out
{
INFO_DATA stInfo;
unsigned int uiFBNum;
unsigned short usStatus;
short sErrorID;
}MMC_MOTIONINFO_OUT;
typedef union
{
MOTION_INFO_DATA fbInfo[40];
```

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

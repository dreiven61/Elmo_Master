# 7.10 Multiple Axes Administrative Control - API 분석

- 원본 장: `Chapter 7 Motion and Administrative - Multi-Axis`
- 시작 PDF 페이지: 726
- 원문 위치: [7.10 Multiple Axes Administrative Control](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-726)
- 언어: 한국어 요약. 함수명, 구조체명, 필드명은 원문 API 식별자를 그대로 유지했습니다.

## API 목록

| 절 | 페이지 | API/항목 | 기능 요약 | 지원/모드 |
|---|---:|---|---|---|
| `7.10.1` | 727 | `Coordinated System and Kinematic Transformation Definitions` | Coordinated 시스템 and 키네마틱 Transformation Definitions 작업을 수행하는 API입니다. | - |
| `7.10.2` | 729 | `MC_KIN_NODE_DEF` | KIN NODE DEF 작업을 수행하는 API입니다. | - |
| `7.10.3` | 731 | `MC_KIN_REF_CARTESIAN` | KIN REF CARTESIAN 작업을 수행하는 API입니다. | - |
| `7.10.4` | 732 | `MC_KIN_REF_DELTA` | KIN REF DELTA 작업을 수행하는 API입니다. | - |
| `7.10.5` | 734 | `MC_KIN_REF_SCARA` | KIN REF SCARA 작업을 수행하는 API입니다. | - |
| `7.10.6` | 736 | `MC_KIN_REF_THREE_LINK` | KIN REF THREE LINK 작업을 수행하는 API입니다. | - |
| `7.10.7` | 738 | `MC_KIN_REF_DUAL_HEAD` | KIN REF DUAL HEAD 작업을 수행하는 API입니다. | - |
| `7.10.8` | 740 | `MC_KIN_REF_HXPD` | KIN REF HXPD 작업을 수행하는 API입니다. | - |
| `7.10.9` | 744 | `MC_KIN_REF UNION` | KIN REF UNION 작업을 수행하는 API입니다. | - |
| `7.10.10` | 745 | `NC_MCS_Info_Struct` | NC MCS 정보 Struct 작업을 수행하는 API입니다. | - |
| `7.10.11` | 747 | `NC_MCS_Kin_Ref_Struct` | NC MCS Kin Ref Struct 작업을 수행하는 API입니다. | - |
| `7.10.12` | 748 | `MMC_SetKinTransform` | 설정 Kin 변환 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `7.10.13` | 754 | `MMC_SetKinTransformEx` | 설정 Kin 변환 Ex 값/설정을 적용하는 API입니다. | - |
| `7.10.14` | 761 | `MMC_SetCartesianTransform` | 설정 Cartesian 변환 값/설정을 적용하는 API입니다. | - |
| `7.10.15` | 765 | `MMC_TrackConveyorBelt` | Track Conveyor Belt 작업을 수행하는 API입니다. | - |
| `7.10.16` | 771 | `MMC_TrackRotaryTable` | 값 또는 상태를 조회하는 API입니다. | - |
| `7.10.17` | 777 | `MMC_TrackSyncOut` | 값 또는 상태를 조회하는 API입니다. | - |
| `7.10.18` | 782 | `MMC_SetKinTransformDelta` | 설정 Kin 변환 Delta 값/설정을 적용하는 API입니다. | - |
| `7.10.19` | 785 | `MMC_SetKinTransformCartesian` | 설정 Kin 변환 Cartesian 값/설정을 적용하는 API입니다. | - |
| `7.10.20` | 788 | `MMC_SetKinTransformScara` | 설정 Kin 변환 Scara 값/설정을 적용하는 API입니다. | - |
| `7.10.21` | 791 | `MMC_SetKinTransformThreeLink` | 설정 Kin 변환 Three Link 값/설정을 적용하는 API입니다. | - |
| `7.10.22` | 794 | `MMC_SetKinTransformHxpd` | 설정 Kin 변환 Hxpd 값/설정을 적용하는 API입니다. | - |
| `7.10.23` | 797 | `MMC_GetMotionInfo` | 조회 Motion 정보 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.10.24` | 800 | `MMC_AddAxisToGroup` | Add 축 To 그룹 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.10.25` | 803 | `MMC_GroupDisableCmd` | 그룹 비활성화 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.10.26` | 806 | `MMC_GroupEnableCmd` | 그룹 활성화 활성화/비활성화 제어를 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.10.27` | 808 | `MMC_GroupReadActualPosition` | 그룹 읽기 실제 위치 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.10.28` | 811 | `MMC_GroupReadActualVelocity` | 그룹 읽기 실제 속도 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.10.29` | 814 | `MMC_GroupReadError` | 그룹 읽기 오류 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.10.30` | 817 | `MMC_GroupReadStatusCmd` | 그룹 읽기 상태 값/상태를 조회하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.10.31` | 820 | `MMC_GroupResetCmd` | 그룹 리셋 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.10.32` | 823 | `MMC_GroupSetOverrideCmd` | 그룹 설정 오버라이드 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `7.10.33` | 827 | `MMC_GroupSetPositionCmd` | 그룹 설정 위치 값/설정을 적용하는 API입니다. | Motion Mode NC - Supported Distributed - Not supported |
| `7.10.34` | 832 | `MMC_RemoveAxisFromGroup` | 제거 축 From 그룹 작업을 수행하는 API입니다. | Motion Mode NC - Supported Distributed - Not Supported |
| `7.10.35` | 835 | `MMC_GroupReadParameter` | 그룹 읽기 파라미터 값/상태를 조회하는 API입니다. | Motion Mode NC - Irrelevant Distributed - Irrelevant |
| `7.10.36` | 837 | `MMC_GroupReadBoolParameter` | 그룹 읽기 불리언 파라미터 값/상태를 조회하는 API입니다. | Motion Mode NC - Irrelevant Distributed - Irrelevant |
| `7.10.37` | 839 | `MMC_GroupWriteParameter` | 그룹 쓰기 파라미터 값/설정을 적용하는 API입니다. | Motion Mode NC - Irrelevant Distributed - Irrelevant |
| `7.10.38` | 841 | `MMC_GroupWriteBoolParameter` | 그룹 쓰기 불리언 파라미터 값/설정을 적용하는 API입니다. | Motion Mode NC - Irrelevant Distributed - Irrelevant |
| `7.10.39` | 843 | `MMC_GetGroupByNameCmd` | 조회 그룹 By Name 값/상태를 조회하는 API입니다. | Motion Mode NC - Not relevant Distributed - not relevant |

## 공통 호출 인자 해석

- `hConn`: Maestro 연결 핸들입니다. Init Connection 계열 함수에서 얻은 값을 사용합니다.
- `hAxisRef`: 대상 축 또는 그룹 참조 핸들입니다.
- `pInParam`: API별 입력 구조체 포인터입니다.
- `pOutParam`: API별 출력 구조체 포인터입니다.
- 반환값 `int`: 라이브러리 호출 결과입니다. 오류 시 매뉴얼의 Error ID/Status를 같이 확인해야 합니다.

## 상세 API

### 7.10.1 Coordinated System and Kinematic Transformation Definitions

- PDF 페이지: 727
- 원문 위치: [7.10.1 Coordinated System and Kinematic Transformation Definitions](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-727)
- 기능 설명: Coordinated 시스템 and 키네마틱 Transformation Definitions 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.10.2 MC_KIN_NODE_DEF

- PDF 페이지: 729
- 원문 위치: [7.10.2 MC_KIN_NODE_DEF](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-729)
- 기능 설명: KIN NODE DEF 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

##### `MC_KIN_NODE_DEF`
| 필드 | 해석 |
|---|---|
| `double ulTrCoef [NC_MAX_NUM_COEF];` | 길이, 크기 또는 개수 값입니다. |
| `NC_TR_FUNC_ID_ENUM iMcsToAcsFuncID;` | i Mcs To Acs Func ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_NODE_HNDL_T hNode;` | 노드 식별 또는 노드 관련 값입니다. |
| `NC_AXIS_IN_GROUP_TYPE_ENUM_EX eType;` | 데이터 또는 동작 타입 값입니다. |

### 7.10.3 MC_KIN_REF_CARTESIAN

- PDF 페이지: 731
- 원문 위치: [7.10.3 MC_KIN_REF_CARTESIAN](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-731)
- 기능 설명: KIN REF CARTESIAN 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

##### `MC_KIN_REF_CARTESIAN`
| 필드 | 해석 |
|---|---|
| `MC_KIN_NODE_DEF sNode[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `int iNumAxes;` | 길이, 크기 또는 개수 값입니다. |

### 7.10.4 MC_KIN_REF_DELTA

- PDF 페이지: 732
- 원문 위치: [7.10.4 MC_KIN_REF_DELTA](../chunks/027_p0694-p0733_7.9.13-MMC_MoveLinearAdditiveEx.md#pdf-page-732)
- 기능 설명: KIN REF DELTA 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

##### `MC_KIN_REF_DELTA`
| 필드 | 해석 |
|---|---|
| `double dbArm;` | db Arm 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbForeArm;` | db Fore Arm 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbBaseRadius;` | db Base Radius 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbEndEffectorRadius;` | db 종료 Effector Radius 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `MC_KIN_NODE_DEF sNode [NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `int iNumAxes;` | 길이, 크기 또는 개수 값입니다. |

### 7.10.5 MC_KIN_REF_SCARA

- PDF 페이지: 734
- 원문 위치: [7.10.5 MC_KIN_REF_SCARA](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-734)
- 기능 설명: KIN REF SCARA 작업을 수행하는 API입니다.

#### 시그니처


#### 구조체/인자

##### `MC_KIN_REF_SCARA`
| 필드 | 해석 |
|---|---|
| `double dInnerLinkLength;` | 길이, 크기 또는 개수 값입니다. |
| `double dOuterLinkLength;` | 길이, 크기 또는 개수 값입니다. |
| `double dShoulderOffset;` | d Shoulder Offset 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dWristOffset;` | d Wrist Offset 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dWristTheta2OffsetCoef;` | d Wrist Theta2 Offset Coef 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `MC_KIN_NODE_DEF sNode[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `int iNumAxes;` | 길이, 크기 또는 개수 값입니다. |
| `char cElbowSign;` | c Elbow Sign 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cPadding1 ;` | char c Padding1 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cPadding2 ;` | char c Padding2 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cPadding3 ;` | char c Padding3 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 7.10.6 MC_KIN_REF_THREE_LINK

- PDF 페이지: 736
- 원문 위치: [7.10.6 MC_KIN_REF_THREE_LINK](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-736)
- 기능 설명: KIN REF THREE LINK 작업을 수행하는 API입니다.

#### 시그니처


#### 구조체/인자

##### `MC_KIN_REF_THREE_LINK`
| 필드 | 해석 |
|---|---|
| `double dInnerLinkLength ;` | 길이, 크기 또는 개수 값입니다. |
| `double dMediumLinkLength;` | 길이, 크기 또는 개수 값입니다. |
| `double dOuterLinkLength;` | 길이, 크기 또는 개수 값입니다. |
| `double dShoulderOffset;` | d Shoulder Offset 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dWristOffset;` | d Wrist Offset 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dWristTheta2OffsetCoef;` | d Wrist Theta2 Offset Coef 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `MC_KIN_NODE_DEF sNode[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `int iNumAxes;` | 길이, 크기 또는 개수 값입니다. |
| `char cElbowSign ;` | char c Elbow Sign 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cPadding1 ;` | char c Padding1 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cPadding2 ;` | char c Padding2 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `char cPadding3 ;` | char c Padding3 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 7.10.7 MC_KIN_REF_DUAL_HEAD

- PDF 페이지: 738
- 원문 위치: [7.10.7 MC_KIN_REF_DUAL_HEAD](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-738)
- 기능 설명: KIN REF DUAL HEAD 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

##### `MC_KIN_REF_DUAL_HEAD`
| 필드 | 해석 |
|---|---|
| `double dOffsetX2 ;` | double d Offset X2 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dOffsetY2;` | d Offset Y2 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dOffsetZ2;` | d Offset Z2 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dOffsetW;` | d Offset W 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `MC_KIN_NODE_DEF sNode[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `int iNumAxes;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned char cAutoOffset ;` | unsigned char c Auto Offset 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char cSpare1 ;` | unsigned char c Spare1 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char cSpare2 ;` | unsigned char c Spare2 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char cSpare3 ;` | unsigned char c Spare3 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `long lSpare[100] ;` | long l Spare[100] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 7.10.8 MC_KIN_REF_HXPD

- PDF 페이지: 740
- 원문 위치: [7.10.8 MC_KIN_REF_HXPD](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-740)
- 기능 설명: KIN REF HXPD 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.10.9 MC_KIN_REF UNION

- PDF 페이지: 744
- 원문 위치: [7.10.9 MC_KIN_REF UNION](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-744)
- 기능 설명: KIN REF UNION 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.10.10 NC_MCS_Info_Struct

- PDF 페이지: 745
- 원문 위치: [7.10.10 NC_MCS_Info_Struct](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-745)
- 기능 설명: NC MCS 정보 Struct 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

##### `NC_MCS_INFO_STRUCT`
| 필드 | 해석 |
|---|---|
| `double ulTrCoef[NC_MAX_NUM_COEF];` | 길이, 크기 또는 개수 값입니다. |
| `unsigned int hNode;` | 노드 식별 또는 노드 관련 값입니다. |
| `NC_AXIS_IN_GROUP_TYPE_ENUM eType;` | 데이터 또는 동작 타입 값입니다. |
| `NC_TR_FUNC_ID_ENUM eMcsToAcsFuncID;` | e Mcs To Acs Func ID 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

### 7.10.11 NC_MCS_Kin_Ref_Struct

- PDF 페이지: 747
- 원문 위치: [7.10.11 NC_MCS_Kin_Ref_Struct](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-747)
- 기능 설명: NC MCS Kin Ref Struct 작업을 수행하는 API입니다.

#### 시그니처

- 이 절의 추출 텍스트에서 C/C++ 시그니처를 자동 확인하지 못했습니다. 원문 위치를 확인해야 합니다.

#### 구조체/인자

- 이 절에서 `*_IN` / `*_OUT` 구조체 정의를 자동 추출하지 못했습니다. 시그니처 인자와 원문 위치를 기준으로 확인해야 합니다.

### 7.10.12 MMC_SetKinTransform

- PDF 페이지: 748
- 원문 위치: [7.10.12 MMC_SetKinTransform](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-748)
- 기능 설명: 설정 Kin 변환 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_SetKinTransform(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SETKINTRANSFORM_IN* pInParam,
OUT MMC_SETKINTRANSFORM_OUT* pOutParam);
```

#### 구조체/인자

##### `MMC_SETKINTRANSFORM_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double ulTrCoef[NC_MAX_NUM_AXES_IN_NODE][NC_MAX_NUM_COEF];` | 노드 식별 또는 노드 관련 값입니다. |
| `int iNumAxes;` | 길이, 크기 또는 개수 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `NC_TR_FUNC_ID_ENUM iMcsToAcsFuncID[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `NC_NODE_HNDL_T hNode[NC_MAX_NUM_AXES_IN_NODE];` | 함수 블록 또는 리소스 핸들입니다. |
| `NC_AXIS_IN_GROUP_TYPE_ENUM eType[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |

##### `MMC_SETKINTRANSFORM_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.13 MMC_SetKinTransformEx

- PDF 페이지: 754
- 원문 위치: [7.10.13 MMC_SetKinTransformEx](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-754)
- 기능 설명: 설정 Kin 변환 Ex 값/설정을 적용하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_SetKinTransformex(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SETKINTRANSFORMEX_IN* pInParam,
OUT MMC_SETKINTRANSFORMEX_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETKINTRANSFORMEX_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `MC_KIN_REF stInput;` | st 입력 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `NC_KIN_TYPE eKinType;` | 데이터 또는 동작 타입 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

##### `MMC_SETKINTRANSFORMEX_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.14 MMC_SetCartesianTransform

- PDF 페이지: 761
- 원문 위치: [7.10.14 MMC_SetCartesianTransform](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-761)
- 기능 설명: 설정 Cartesian 변환 값/설정을 적용하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_SetCartesianTransform(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SETCARTESIANTRANSFORM_IN* pInParam,
OUT MMC_SETCARTESIANTRANSFORM_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETCARTESIANTRANSFORM_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dOffset[3];` | d Offset[3] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dRotAngle[3];` | d Rot Angle[3] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dPadding[5] ;` | double d Padding[5] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits;` | e Rot Angle Units 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `MC_EXECUTION_MODE eExecutionMode;` | 동작 모드 값입니다. |

##### `MMC_SETCARTESIANTRANSFORM_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.15 MMC_TrackConveyorBelt

- PDF 페이지: 765
- 원문 위치: [7.10.15 MMC_TrackConveyorBelt](../chunks/028_p0734-p0770_7.10.5-MC_KIN_REF_SCARA.md#pdf-page-765)
- 기능 설명: Track Conveyor Belt 작업을 수행하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_TrackConveyorBelt(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_TRACKCONVEYOR_IN* pi_params,
OUT MMC_TRACKCONVEYOR_OUT* po_params
);
```

#### 구조체/인자

##### `MMC_TRACKCONVEYOR_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbConveyorBeltOrigin[6];` | db Conveyor Belt Origin[6] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbPCSOrigin[6];` | db PCSOrigin[6] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbInitialObjectPosition[6];` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbMasterInitialPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbMasterSyncPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbMasterScaling;` | db Master Scaling 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbRampTrajectoryParams[12];` | 파라미터 식별자 또는 파라미터 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits;` | e Rot Angle Units 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `TRAJECTORY_MODE_ENUM eTrajectoryMode;` | 동작 모드 값입니다. |
| `unsigned short usConveyorBelt;` | us Conveyor Belt 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucAutoSyncPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `unsigned char ucSpare[32];` | uc Spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_TRACKCONVEYOR_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.16 MMC_TrackRotaryTable

- PDF 페이지: 771
- 원문 위치: [7.10.16 MMC_TrackRotaryTable](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-771)
- 기능 설명: 값 또는 상태를 조회하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_TrackRotaryTable(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_TRACKROTARY_IN* pInParam,
OUT MMC_TRACKROTARY_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_TRACKROTARY_IN`
| 필드 | 해석 |
|---|---|
| `double dbRotaryTableOrigin[6];` | db Rotary 테이블 Origin[6] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbPCSOrigin[6];` | db PCSOrigin[6] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbInitialObjectPosition[6];` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbMasterInitialPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbMasterSyncPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbMasterScaling;` | db Master Scaling 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbRampTrajectoryParams[12];` | 파라미터 식별자 또는 파라미터 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `PCS_ROTATION_ANGLE_UNITS_ENUM eRotAngleUnits;` | e Rot Angle Units 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `TRAJECTORY_MODE_ENUM eTrajectoryMode;` | 동작 모드 값입니다. |
| `unsigned short usRotaryTable;` | us Rotary 테이블 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucAutoSyncPosition;` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `unsigned char ucSpare[32];` | uc Spare[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_TRACKROTARY_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.17 MMC_TrackSyncOut

- PDF 페이지: 777
- 원문 위치: [7.10.17 MMC_TrackSyncOut](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-777)
- 기능 설명: 값 또는 상태를 조회하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_TrackSyncOut(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_TRACKSYNCOUT_IN* pi_params,
OUT MMC_TRACKSYNCOUT_OUT* po_params
);
```

#### 구조체/인자

##### `MMC_TRACKSYNCOUT_IN`
| 필드 | 해석 |
|---|---|
| `double dbMasterOrigin[6];` | db Master Origin[6] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbTargetPosition[6];` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `double dbRampTrajectoryParams[12];` | 파라미터 식별자 또는 파라미터 값입니다. |
| `double dbMasterScaling;` | db Master Scaling 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `double dbTime;` | 시간 제한, 주기 또는 지연 시간 값입니다. 문맥에 따라 ms 단위일 수 있습니다. |
| `double dbStopDeceleration;` | 감속도 값입니다. 보통 `[u/s2]` 단위입니다. |
| `TRAJECTORY_MODE_ENUM eTrajectoryMode;` | 동작 모드 값입니다. |
| `unsigned short usMaster;` | us Master 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucInstantly;` | uc Instantly 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `unsigned char futures[32];` | futures[32] 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_TRACKSYNCOUT_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndl;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.18 MMC_SetKinTransformDelta

- PDF 페이지: 782
- 원문 위치: [7.10.18 MMC_SetKinTransformDelta](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-782)
- 기능 설명: 설정 Kin 변환 Delta 값/설정을 적용하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_SetKinTransformDelta(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_KINTRANSFORM_DELTA_IN* pInParam,
OUT MMC_KINTRANSFORM_DELTA_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_KINTRANSFORM_DELTA_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `MC_KIN_REF_DELTA stParams;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

### 7.10.19 MMC_SetKinTransformCartesian

- PDF 페이지: 785
- 원문 위치: [7.10.19 MMC_SetKinTransformCartesian](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-785)
- 기능 설명: 설정 Kin 변환 Cartesian 값/설정을 적용하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_SetKinTransformCartesian(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_KINTRANSFORM_CARTESIAN_IN* pInParam,
OUT MMC_KINTRANSFORM_CARTESIAN_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_KINTRANSFORM_CARTESIAN_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `MC_KIN_REF_CARTESIAN stParams;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

### 7.10.20 MMC_SetKinTransformScara

- PDF 페이지: 788
- 원문 위치: [7.10.20 MMC_SetKinTransformScara](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-788)
- 기능 설명: 설정 Kin 변환 Scara 값/설정을 적용하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_SetKinTransformScara(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_KINTRANSFORM_SCARA_IN* pInParam,
OUT MMC_KINTRANSFORM_SCARA_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_KINTRANSFORM_SCARA_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `MC_KIN_REF_SCARA stParams;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |

### 7.10.21 MMC_SetKinTransformThreeLink

- PDF 페이지: 791
- 원문 위치: [7.10.21 MMC_SetKinTransformThreeLink](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-791)
- 기능 설명: 설정 Kin 변환 Three Link 값/설정을 적용하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_SetKinTransformThreeLink(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_KINTRANSFORM_THREELINK_IN* pInParam,
OUT MMC_KINTRANSFORM_THREELINK_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_KINTRANSFORM_THREELINK_IN`
| 필드 | 해석 |
|---|---|
| `MC_KIN_REF_THREE_LINK stParams;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

### 7.10.22 MMC_SetKinTransformHxpd

- PDF 페이지: 794
- 원문 위치: [7.10.22 MMC_SetKinTransformHxpd](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-794)
- 기능 설명: 설정 Kin 변환 Hxpd 값/설정을 적용하는 API입니다.

#### 시그니처

```c
MMC_LIB_API int MMC_SetKinTransformHxpd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_KINTRANSFORM_HXPD_IN* i_param,
OUT MMC_KINTRANSFORM_HXPD_OUT* o_param
);
```

#### 구조체/인자

##### `MMC_KINTRANSFORM_HXPD_IN`
| 필드 | 해석 |
|---|---|
| `MC_KIN_REF_HXPD stParams;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

### 7.10.23 MMC_GetMotionInfo

- PDF 페이지: 797
- 원문 위치: [7.10.23 MMC_GetMotionInfo](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-797)
- 기능 설명: 조회 Motion 정보 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GetMotionInfo(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_MOTIONINFO_IN* pInParam,
OUT MMC_MOTIONINFO_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_MOTIONINFO_IN`
| 필드 | 해석 |
|---|---|
| `MOTION_INFO_ENUM eMotionInfo;` | e Motion 정보 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_MOTIONINFO_OUT`
| 필드 | 해석 |
|---|---|
| `INFO_DATA stInfo;` | st 정보 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned int uiFBNum;` | 길이, 크기 또는 개수 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `unsigned short usErrorID;` | 오류 ID입니다. |

### 7.10.24 MMC_AddAxisToGroup

- PDF 페이지: 800
- 원문 위치: [7.10.24 MMC_AddAxisToGroup](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-800)
- 기능 설명: Add 축 To 그룹 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_AddAxisToGroup(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_ADDAXISTOGROUP_IN* pInParam,
OUT MMC_ADDAXISTOGROUP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_ADDAXISTOGROUP_IN`
| 필드 | 해석 |
|---|---|
| `NC_NODE_HNDL_T hNode;` | 노드 식별 또는 노드 관련 값입니다. |
| `NC_IDENT_IN_GROUP_ENUM eIdentInGroup;` | 그룹 식별 또는 그룹 관련 값입니다. |

##### `MMC_ADDAXISTOGROUP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.25 MMC_GroupDisable

- PDF 페이지: 803
- 원문 위치: [7.10.25 MMC_GroupDisable](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-803)
- 기능 설명: 그룹 비활성화 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupDisableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPDISABLE_IN* pInParam,
OUT MMC_GROUPDISABLE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GROUPDISABLE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_GROUPDISABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.26 MMC_GroupEnable

- PDF 페이지: 806
- 원문 위치: [7.10.26 MMC_GroupEnable](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-806)
- 기능 설명: 그룹 활성화 활성화/비활성화 제어를 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupEnableCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPENABLE_IN* pInParam,
OUT MMC_GROUPENABLE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GROUPENABLE_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_GROUPENABLE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.27 MMC_GroupReadActualPosition

- PDF 페이지: 808
- 원문 위치: [7.10.27 MMC_GroupReadActualPosition](../chunks/029_p0771-p0810_7.10.16-MMC_TrackRotaryTable.md#pdf-page-808)
- 기능 설명: 그룹 읽기 실제 위치 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupReadActualPosition(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPREADACTUALPOSITION_IN* pInParam,
OUT MMC_GROUPREADACTUALPOSITION_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GROUPREADACTUALPOSITION_IN`
| 필드 | 해석 |
|---|---|
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_GROUPREADACTUALPOSITION_OUT`
| 필드 | 해석 |
|---|---|
| `double dbPosition[NC_MAX_NUM_AXES_IN_NODE];` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.28 MMC_GroupReadActualVelocity

- PDF 페이지: 811
- 원문 위치: [7.10.28 MMC_GroupReadActualVelocity](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-811)
- 기능 설명: 그룹 읽기 실제 속도 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupReadActualVelocity(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPREADACTUALVELOCITY_IN* pInParam,
OUT MMC_GROUPREADACTUALVELOCITY_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GROUPREADACTUALVELOCITY_IN`
| 필드 | 해석 |
|---|---|
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_GROUPREADACTUALVELOCITY_OUT`
| 필드 | 해석 |
|---|---|
| `double dVelocity[NC_MAX_NUM_AXES_IN_NODE];` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `double dPathVelocity;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.29 MMC_GroupReadError

- PDF 페이지: 814
- 원문 위치: [7.10.29 MMC_GroupReadError](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-814)
- 기능 설명: 그룹 읽기 오류 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupReadError(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPREADERROR_IN* pInParam,
OUT MMC_GROUPREADERROR_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GROUPREADERROR_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_GROUPREADERROR_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usGroupErrorID;` | 오류 ID입니다. |

### 7.10.30 MMC_GroupReadStatus

- PDF 페이지: 817
- 원문 위치: [7.10.30 MMC_GroupReadStatus](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-817)
- 기능 설명: 그룹 읽기 상태 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupReadStatusCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPREADSTATUS_IN* pInParam,
OUT MMC_GROUPREADSTATUS_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GROUPREADSTATUS_IN`
| 필드 | 해석 |
|---|---|
| `unsigned int uiHndlr;` | 함수 블록 또는 리소스 핸들입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_GROUPREADSTATUS_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned long ulState;` | ul State 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned short usGroupErrorID;` | 오류 ID입니다. |

### 7.10.31 MMC_GroupReset

- PDF 페이지: 820
- 원문 위치: [7.10.31 MMC_GroupReset](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-820)
- 기능 설명: 그룹 리셋 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupResetCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPRESET_IN* pInParam,
OUT MMC_GROUPRESET_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GROUPRESET_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |

##### `MMC_GROUPRESET_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.32 MMC_GroupSetOverride

- PDF 페이지: 823
- 원문 위치: [7.10.32 MMC_GroupSetOverride](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-823)
- 기능 설명: 그룹 설정 오버라이드 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupSetOverrideCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_SETOVERRIDE_IN* pInParam,
OUT MMC_SETOVERRIDE_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_SETOVERRIDE_IN`
| 필드 | 해석 |
|---|---|
| `float fVelFactor;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `float fAccFactor;` | f Acc Factor 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `float fJerkFactor;` | 저크 값입니다. 보통 `[u/s3]` 단위입니다. |
| `unsigned short usUpdateVelFactorIdx;` | 속도 값입니다. 보통 `[u/s]` 단위입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_SETOVERRIDE_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.33 MMC_GroupSetPosition

- PDF 페이지: 827
- 원문 위치: [7.10.33 MMC_GroupSetPosition](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-827)
- 기능 설명: 그룹 설정 위치 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not supported

#### 시그니처

```c
MMC_LIB_API int MMC_GroupSetPositionCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GROUPSETPOSITION_IN* pInParam,
OUT MMC_GROUPSETPOSITION_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_GROUPSETPOSITION_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucExecute;` | 실행 트리거입니다. 상승 에지에서 명령을 시작하는 TRUE/FALSE 값입니다. |
| `double dbPosition[NC_MAX_NUM_AXES_IN_NODE];` | 위치 값입니다. 보통 technical unit `[u]` 단위입니다. |
| `MC_COORD_SYSTEM_ENUM eCoordSystem;` | e Coord 시스템 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |
| `MC_BUFFERED_MODE_ENUM eBufferMode;` | 버퍼링/블렌딩 동작 모드입니다. |
| `unsigned char ucMode;` | 동작 모드 값입니다. |

##### `MMC_GROUPSETPOSITION_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.34 MMC_RemoveAxisFromGroup

- PDF 페이지: 832
- 원문 위치: [7.10.34 MMC_RemoveAxisFromGroup](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-832)
- 기능 설명: 제거 축 From 그룹 작업을 수행하는 API입니다.
- 지원/모드: Motion Mode NC - Supported Distributed - Not Supported

#### 시그니처

```c
MMC_LIB_API int MMC_RemoveAxisFromGroup(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_REMOVEAXISFROMGROUP_IN* pInParam,
OUT MMC_REMOVEAXISFROMGROUP_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_REMOVEAXISFROMGROUP_IN`
| 필드 | 해석 |
|---|---|
| `NC_IDENT_IN_GROUP_ENUM eIdentInGroup;` | 그룹 식별 또는 그룹 관련 값입니다. |

##### `MMC_REMOVEAXISFROMGROUP_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.35 MMC_GroupReadParameter

- PDF 페이지: 835
- 원문 위치: [7.10.35 MMC_GroupReadParameter](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-835)
- 기능 설명: 그룹 읽기 파라미터 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Irrelevant Distributed - Irrelevant

#### 시그니처

```c
MMC_LIB_API int MMC_GroupReadParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READPARAMETER_IN* pInParam,
OUT MMC_READPARAMETER_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READPARAMETER_IN`
| 필드 | 해석 |
|---|---|
| `MMC_PARAMETER_LIST_ENUM eParameterNumber;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `int iParameterArrIndex;` | 인덱스 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READPARAMETER_OUT`
| 필드 | 해석 |
|---|---|
| `double dbValue;` | 전달하거나 반환받는 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.36 MMC_GroupReadBoolParameter

- PDF 페이지: 837
- 원문 위치: [7.10.36 MMC_GroupReadBoolParameter](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-837)
- 기능 설명: 그룹 읽기 불리언 파라미터 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Irrelevant Distributed - Irrelevant

#### 시그니처

```c
MMC_LIB_API int MMC_GroupReadBoolParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_READBOOLPARAMETER_IN* pInParam,
OUT MMC_READBOOLPARAMETER_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_READBOOLPARAMETER_IN`
| 필드 | 해석 |
|---|---|
| `MMC_PARAMETER_LIST_ENUM eParameterNumber;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `int iParameterArrIndex;` | 인덱스 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_READBOOLPARAMETER_OUT`
| 필드 | 해석 |
|---|---|
| `long lValue;` | 전달하거나 반환받는 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.37 MMC_GroupWriteParameter

- PDF 페이지: 839
- 원문 위치: [7.10.37 MMC_GroupWriteParameter](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-839)
- 기능 설명: 그룹 쓰기 파라미터 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Irrelevant Distributed - Irrelevant

#### 시그니처

```c
MMC_LIB_API int MMC_WriteParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEPARAMETER_IN* pInParam,
OUT MMC_WRITEPARAMETER_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEPARAMETER_IN`
| 필드 | 해석 |
|---|---|
| `double dbValue;` | 전달하거나 반환받는 값입니다. |
| `MMC_PARAMETER_LIST_ENUM eParameterNumber;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `int iParameterArrIndex;` | 인덱스 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_WRITEPARAMETER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.38 MMC_GroupWriteBoolParameter

- PDF 페이지: 841
- 원문 위치: [7.10.38 MMC_GroupWriteBoolParameter](../chunks/030_p0811-p0842_7.10.28-MMC_GroupReadActualVelocity.md#pdf-page-841)
- 기능 설명: 그룹 쓰기 불리언 파라미터 값/설정을 적용하는 API입니다.
- 지원/모드: Motion Mode NC - Irrelevant Distributed - Irrelevant

#### 시그니처

```c
MMC_LIB_API int MMC_GroupWriteBoolParameter(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_WRITEBOOLPARAMETER_IN* pInParam,
OUT MMC_WRITEBOOLPARAMETER_OUT* pOutParam
);
```

#### 구조체/인자

##### `MMC_WRITEBOOLPARAMETER_IN`
| 필드 | 해석 |
|---|---|
| `long lValue;` | 전달하거나 반환받는 값입니다. |
| `MMC_PARAMETER_LIST_ENUM eParameterNumber;` | 파라미터 식별자 또는 파라미터 값입니다. |
| `int iParameterArrIndex;` | 인덱스 값입니다. |
| `unsigned char ucEnable;` | 활성화/비활성화 제어 값입니다. |

##### `MMC_WRITEBOOLPARAMETER_OUT`
| 필드 | 해석 |
|---|---|
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |

### 7.10.39 MMC_GetGroupMembersInfo

- PDF 페이지: 843
- 원문 위치: [7.10.39 MMC_GetGroupMembersInfo](../chunks/031_p0843-p0846_7.10.39-MMC_GetGroupMembersInfo.md#pdf-page-843)
- 기능 설명: 조회 그룹 By Name 값/상태를 조회하는 API입니다.
- 지원/모드: Motion Mode NC - Not relevant Distributed - not relevant

#### 시그니처

```c
MMC_LIB_API int MMC_GetGroupMembersInfo(
IN MMC_CONNECT_HNDL hConn,
IN MMC_AXIS_REF_HNDL hAxisRef,
IN MMC_GETGROUPMEMBERSINFO_IN * pInParam,
OUT MMC_GETGROUPMEMBERSINFO_OUT * pOutParam
);
```
```c
MMC_GetGroupByNameCmd(hConn,&sGroupByNameInParam,&sGroupByNameOutParam);
//
// Create input and output structures
MMC_GETGROUPMEMBERSINFO_IN sMembersInfoInParam;
MMC_GETGROUPMEMBERSINFO_OUT sMembersInfoOutParam;
//
// There are no neccessary inputs in the input structure (only dummy
variable)
sMembersInfoInParam.ucDummy = 0;
//
// call GetGroupMembersInfo function (assume that there are not errors in
this function)
MMC_GetGroupMembersInfo(hConn,sGroupByNameOutParam.usAxisIdx,&sMembersInfoI
nParam,&sMembersInfoOutParam);
```

#### 구조체/인자

##### `MMC_GETGROUPMEMBERSINFO_IN`
| 필드 | 해석 |
|---|---|
| `unsigned char ucDummy;` | uc Dummy 관련 인자입니다. 원문 구조체 필드명 기준으로 확인했습니다. |

##### `MMC_GETGROUPMEMBERSINFO_OUT`
| 필드 | 해석 |
|---|---|
| `char pAxesNames[NC_MAX_NUM_AXES_IN_NODE][NODE_NAME_MAX_LENGTH];` | 노드 식별 또는 노드 관련 값입니다. |
| `unsigned short pAxesReferences[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `unsigned short pDeviceID[NC_MAX_NUM_AXES_IN_NODE];` | 노드 식별 또는 노드 관련 값입니다. |
| `unsigned short usStatus;` | 명령 또는 장치 상태 값입니다. |
| `short sErrorID;` | 오류 ID입니다. |
| `unsigned char ucNumOfAxes;` | 길이, 크기 또는 개수 값입니다. |

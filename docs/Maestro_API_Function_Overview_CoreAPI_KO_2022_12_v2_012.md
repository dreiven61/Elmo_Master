# Maestro Core API 함수 개요 (한글)

원본: Maestro Administrative and Motion API_2022_12_v2.012.pdf

범위: Chapter 4~23 API 함수 중심. Chapter 24 이후 C++/Python wrapper 및 programming 내용은 제외.

총 함수 수: 341

| 함수명 | 파라미터 | 내용 |
|---|---|---|
| MMC_RegErrPolicy | hConn, pInParam, pOutParam | 이 함수는 오류 정책을 등록하고 정의합니다. |
| MMC_GetErrPolicy | hConn, pOutParam | 이 함수는 현재 오류 정책 상태를 반환합니다. |
| MMC_ResetSystem | hConn, pInParam, pOutParam | 이 기능은 PHY의 오류 카운터, 순환 및 누락된 프레임 오류를 포함한 전체 시스템 오류를 재설정합니다. 또한 모든 노드를 INIT로 변경한 다음 OPERATIONAL 상태로 변경하여 모든 드라이브의 모터가 꺼집니다. 또한 이 함수는 Maestro 치명적인 오류를 재설정하고 완전한 작동 상태로 되돌릴 수 있습니다. |
| MMC_SetProfileConditioning | hConn, hAxisRef, i_params, o_params | 진동 감소를 위한 프로파일 조절 구성을 설정하거나 가져오는 데 사용되는 프로파일 조절 C 기능입니다. |
| MMC_GetProfileConditioning | hConn, hAxisRef, i_params, o_params | 진동 감소를 위한 프로파일 조절 구성을 설정하거나 가져오는 데 사용되는 프로파일 조절 C 기능입니다. |
| GetProfileConditioning | hConnHndl, o_params | 진동 감소를 위한 프로파일 조절 구성을 설정하거나 가져오는 데 사용되는 프로파일 조절 C 기능입니다. |
| SetProfileConditioning | i_params | 진동 감소를 위한 프로파일 조절 구성을 설정하거나 가져오는 데 사용되는 프로파일 조절 C 기능입니다. |
| MMC_Halt | hConn, hAxisRef, pInParam, pOutParam | 특정 축에 대해 제어된 동작 중지를 명령하려면 이 함수를 호출합니다. |
| MMC_Home | hConn, hAxisRef, pInParam, pOutParam | 홈 검색 시퀀스를 수행하도록 축에 명령합니다. |
| MMC_HomeDS402 | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 특정 축에 대해 홈 검색 시퀀스를 수행하기 위한 명령을 보내며 축 매개변수로 설정할 수 있습니다. |
| MMC_HomeDS402Ex | hConn, hAxisRef, pInParam, pOutParam | 특정 축에 대해 홈 검색 DS402 시퀀스를 수행하도록 축에 명령하며 축 매개변수로 설정할 수 있습니다. 이 기능은 Velocity Hi\Lo,DetectionTimeLimit 및DetectionVelocityLimit를 지원합니다. |
| MMC_MoveAbsolute | hConn, hAxisRef, pInParam, pOutParam | 지정된 절대 위치에 대한 단일 축의 개별 제어 모션을 명령합니다. |
| MMC_MoveAdditive | hConn, hAxisRef, pInParam, pOutParam | 불연속 동작 상태에서 가장 최근에 명령된 위치에 추가로 지정된 상대 거리의 제어된 동작을 명령합니다. |
| MMC_MoveRelative | hConn, hAxisRef, pInParam, pOutParam | 실행 시 설정된 위치를 기준으로 지정된 거리의 개별 제어 모션을 명령합니다. |
| MMC_MoveTorque | hConn, hAxisRef, pInParam, pOutParam | 지정된 토크로 연속 제어 동작을 명령합니다. |
| MMC_MoveContinuous | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 특정 축에 대해 MMC 서버에 연속 이동 명령을 보냅니다. |
| MMC_MoveAbsoluteRepetitive | hConn, hAxisRef, pInParam, pOutParam | 절대 목표 위치로 이동하라는 명령을 입력 인자로 받는 함수입니다. 축은 Aborted 모드에서 허용되는 함수 블록에 의해 중단될 때까지 현재 위치와 대상 위치 사이를 이동합니다. |
| MMC_MoveRelativeRepetitive | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 입력 인수 중 하나로 현재 위치를 기준으로 한 거리만큼 이동하라는 명령을 받습니다. 축은 Aborting 모드에서 허용되는 함수 블록에 의해 중단될 때까지 현재 위치와 대상 위치 사이를 이동합니다. |
| MMC_MoveAdditiveRepetitive | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 마지막 명령의 최종 위치를 기준으로 일정 거리만큼 이동하라는 명령을 입력 인자로 받습니다. 축은 Aborting 모드에서 허용되는 함수 블록에 의해 중단될 때까지 현재 위치와 대상 위치 사이를 이동합니다. |
| MMC_Stop | hConn, hAxisRef, pInParam, pOutParam | 제어된 모션 정지를 명령하고 축을 Stopping 상태로 전환합니다. |
| MMC_AxisLink | hConn, hAxisRef, pInParam, pOutParam | 물리적 축과 가상축을 연결하는 기능입니다. |
| MMC_AxisUnLink | hConn, hAxisRef, pInParam, pOutParam | 마스터(Primary)와 슬레이브(Minor)로 정의된 두 축 사이의 링크를 끊는 기능입니다. |
| MMC_KillMotion | hConn, hAxisRef, i_param, o_param | 현재 펑션 블록 이후의 반복적인 동작을 정지시키는 기능입니다. |
| MMC_KillRepetitive | hConn, hAxisRef, pInParam, pOutParam | 현재 펑션 블록 이후의 반복적인 동작을 정지시키는 기능입니다. |
| MMC_Dwell | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 Maestro에 임시 정지 상태 명령을 보냅니다. |
| MMC_GetFBDepth | hConn, hAxisRef, pInParam, pOutParam | 활성화 대기 중이거나 현재 활성화된 노드 큐의 함수 블록 수를 가져오는 명령을 보냅니다. 카운트에 포함된 함수 블록은 Done 또는 Abort 상태를 갖지 않습니다. |
| MMC_MarkFbFree | hConn, pInParam, pOutParam | 함수 블록을 사용 가능으로 표시합니다. |
| MMC_GetTotalFbDepth | hConn, hAxisRef, pInParam, pOutParam | 노드 큐에 있는 함수 블록의 총 개수(활성화 대기 중, 현재 활성화됨, 이전에 활성화되었지만 함수 블록 풀로 해제되지 않음)를 수신하라는 명령을 보냅니다. |
| MMC_Power | hConn, hAxisRef, pInParam, pOutParam | 전력 단계를 제어합니다(켜기 또는 끄기). |
| MMC_PositionProfile | hConn, hAxisRef, pInParam, pOutParam | 축의 위치 프로필을 설명합니다. |
| MMC_ReadActualPosition | hConn, hAxisRef, pInParam, pOutParam | 제어되는 축의 실제 위치를 반환합니다. |
| MMC_ReadActualTorque | hConn, hAxisRef, pInParam, pOutParam | Enable이 설정된 경우 제어 축에 대한 실제 토크 값 또는 힘을 반환합니다. |
| MMC_ReadActualVelocity | hConn, hAxisRef, pInParam, pOutParam | Enable이 설정된 경우 실제 속도 값을 반환합니다. |
| MMC_ReadAxisError | hConn, hAxisRef, pInParam, pOutParam | 함수 블록과 관련되지 않은 일반 축 오류를 표시합니다. 축 오류, 드라이브 오류, 통신 오류. |
| MMC_ReadBoolParameter | hConn, hAxisRef, pInParam, pOutParam | 데이터 유형이 unsigned long 또는 un signed int인 특정 공급업체의 값을 반환합니다. |
| MMC_GlobalReadBoolParameter | hConn, pInParam, pOutParam | 데이터 유형이 unsigned long 또는 un signed int인 공급업체 전역 부울 매개변수의 값을 반환합니다. |
| MMC_ReadDigitalOutputs | hConn, hAxisRef, pInParam, pOutParam | 특정 노드에 대한 실제 디지털 출력을 읽습니다. |
| MMC_ReadDigitalOutputs32Bit | hConn, hAxisRef, pInParam, pOutParam | 특정 노드에 대한 실제 32비트 디지털 출력 가져오기를 읽습니다. |
| MMC_ReadParameter | hConn, hAxisRef, pInParam, pOutParam | 공급업체별 매개변수의 값을 반환합니다. |
| MMC_GlobalReadParameter | hConn, pInParam, pOutParam | 공급업체 전역 매개변수의 값을 반환합니다. |
| MMC_ReadStatus | hConn, hAxisRef, pInParam, pOutParam | 선택한 축의 상태 다이어그램 상태에 대한 세부 정보를 반환합니다. |
| MMC_Reset | hConn, hAxisRef, pInParam, pOutParam | 모든 내부 축 관련 오류를 재설정하여 ErrorStop 상태에서 StandStill 또는 Disabled로의 전환을 수행하고 즉시 반환하는 방법을 제공합니다. |
| MMC_ResetAsync | hConn, hAxisRef, pInParam, pOutParam | 모든 내부 축 관련 오류를 재설정하여 ErrorStop 상태에서 StandStill 또는 Disabled로의 전환을 생성합니다. 이 함수는 절차가 완전히 완료될 때까지 기다립니다. 그런 다음 이벤트가 전송됩니다. |
| MMC_SetOverride | hConn, hAxisRef, pInParam, pOutParam | 해당 축에서 작동하는 모든 기능을 포함하여 전체 축에 대한 재정의 값을 설정합니다. |
| MMC_SetPosition | hConn, hAxisRef, pInParam, pOutParam | ac 특정 축에 대해 Set Position 명령을 Maestro로 보냅니다. |
| MMC_TouchProbeEnable | hConn, hAxisRef, pInParam, pOutParam | Enables는 트리거 이벤트에서 축 위치를 기록하는 터치 프로브입니다. |
| MMC_TouchProbeDisable | hConn, hAxisRef, pInParam, pOutParam | 트리거 이벤트에서 축 위치를 기록하기 위해 터치 프로브를 비활성화합니다. |
| MMC_WriteBoolParameter | hConn, hAxisRef, pInParam, pOutParam | BOOL 유형의 공급업체별 매개변수 값을 수정합니다. |
| MMC_GlobalWriteBoolParameter | hConn, pInParam, pOutParam | BOOL 유형의 공급업체 전역 매개변수 값을 수정합니다. |
| MMC_WriteDigitalOutputs | hConn, hAxisRef, pInParam, pOutParam | Write Digital Outputs 관련 API 기능을 수행합니다. |
| MMC_WriteDigitalOutputs32Bit | hConn, hAxisRef, pInParam, pOutParam | 단일 인수 Output(Execute의 상승 에지 사용)에서 참조하는 32비트 디지털 출력에 값을 씁니다. |
| MMC_WriteParameter | hConn, hAxisRef, pInParam, pOutParam | 공급업체별 매개변수 값을 수정합니다. |
| MMC_GlobalWriteParameter | hConn, pInParam, pOutParam | 공급업체 전역 매개변수의 값을 수정합니다. |
| MMC_ChngOpMode | hConn, hAxisRef, pInParam, pOutParam | NC과 Distributed 사이에서 모션 모드를 변경합니다. 이는 DS-402 모드에서 이전에 결정되었습니다. |
| MMC_ChangeOpModeEx | hConn, hAxisRef, pInParam, pOutParam | NC과 Distributed 사이에서 모션 모드를 변경합니다. 이는 이전에 PLC DS -402 모드에서 결정되었습니다. |
| MMC_SetProfileConditioning | hConn, hAxisRef, i_params, o_params | 이 방법은 프로필 컨디셔닝 작동 모드를 설정합니다. 모드를 ON(1)/OFF(0)하고 기타 입력 매개변수를 설정합니다. |
| MMC_GetProfileConditioning | hConn, hAxisRef, i_params, o_params | 이 방법은 이 작동 모드가 활성화된 축에 대한 작동 데이터의 프로파일 컨디셔닝 모드를 가져옵니다. |
| MMC_SetNormalcyMode | hConn, hAxisRef, i_params, o_params | 다축 시스템용. 선택한 특정 평면(xy/xz/yz)에서 정상 작동 모드를 설정합니다. |
| MMC_SetNormalcyOff | hConn, hAxisRef, o_params | 다축 시스템용. 정상 설정 꺼짐은 정상 모드를 비활성화합니다. |
| MMC_GetNormalcyMode | hConn, hAxisRef, o_params | 다축 시스템용. 정상 설정 꺼짐은 정상 모드를 비활성화합니다. |
| MMC_GroupStop | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. 축 그룹을 정지 상태로 만듭니다. |
| MMC_GroupHalt | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. 축 그룹을 정지 상태로 전환합니다. |
| MMC_MoveCircularAbsolute | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. TCP의 실제 위치에서 축 그룹에 대한 보간 원형 이동을 명령합니다. |
| MMC_MoveCircularAbsoluteCenter | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. TCP의 실제 위치에서 xes 그룹의 보간된 원형 중심 이동을 명령합니다. |
| MMC_MoveCircularAbsoluteBorder | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. TCP의 실제 위치에서 축 그룹의 보간된 원형 테두리 이동을 명령합니다. |
| MMC_MoveCircularAbsoluteRadius | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. TCP의 실제 위치에서 축 그룹의 보간된 원형 이동 반경을 명령합니다. |
| MMC_MoveCircularAbsoluteAngle | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. TCP의 실제 위치에서 축 그룹의 보간된 원형 각도 이동을 명령합니다. 움직임은 제한 없이 양의 방향이나 음의 방향으로 움직일 수 있습니다. |
| MMC_MoveAngle | hConn, hAxisRef, pInParam, pOutParam | 사용자가 호 모션에 대해 특정 평면을 지정할 수 있습니다. 여기서 호 모션은 공간에서 서로 수직인 평면(XY, XZ 또는 YZ) 중 하나에서만 수행됩니다. |
| MMC_MoveLinearAbsolute | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. TCP의 실제 위치에서 지정된 좌표계의 절대 위치까지 축 그룹에서 보간된 선형 이동을 명령합니다. |
| MMC_MoveLinearRelative | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. TCP의 실제 위치에서 지정된 좌표계의 상대 거리까지 축 그룹에서 보간된 선형 이동을 명령합니다. |
| MMC_MoveLinearAdditive | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. TCP의 실제 위치에서 지정된 좌표계의 추가 위치까지 축 그룹에서 보간된 선형 이동을 명령합니다. |
| MMC_MoveLinearAdditiveEx | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. TCP의 실제 위치에서 지정된 좌표계의 정확한 추가 위치까지 축 그룹에서 확장된 보간 선형 이동을 명령합니다. |
| MMC_MoveLinearAbsoluteRepetitive | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. 지정된 좌표계에서 입력으로 제공된 절대점까지 축 그룹 벡터에서 보간된 반복 선형 이동을 명령합니다. |
| MMC_MoveLinearRelativeRepetitive | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. 지정된 좌표계에서 입력으로 주어진 실제 위치로부터 상대 거리까지 축 그룹 벡터에서 보간된 반복 선형 이동을 명령합니다. |
| MMC_MovePolynomAbsolute | hConn, hAxisRef, pInParam, pOutParam | 다항식 표현이 관련된 다축 시스템 및 복잡한 모션 시퀀스의 경우. 이 함수는 특정 Vect 또는에 대해 Move Polynom Absolute 명령을 MMC 서버로 보냅니다. 다음을 참조하세요. |
| MMC_PathSelect | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. 파일에서 스플라인 데이터를 읽고 최적의 경로를 계산합니다. |
| MMC_MovePath | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. 이전에 정의한 스플라인 경로를 따라 드라이브 그룹을 이동합니다. |
| MMC_PathUnselect | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. 이 함수는 Maestro에서 스플라인 데이터 테이블을 언로드합니다. |
| MMC_SetKinTransform | hConn, hAxisRef, pInParam, pOutParam | 다중 축에 대해 사전 정의된 모션학 모델을 기반으로 ACS과 MCS 사이의 모션학적 변환을 설정합니다. 자세한 설명은 섹션 7.1좌표계 및 모션학적 변환을 참조하십시오. 자세한 내용은 이후의 좌표계 및 모션학적 변환 정의 섹션을 참조하세요. |
| MMC_SetKinTransformEx | hConn, hAxisRef, pInParam, pOutParam | 그룹 다중 축에 대해 사전 정의된 모션학 모델을 기반으로 ACS과 MCS 사이의 모션학 변환을 설정합니다. 자세한 설명은 좌표계 및 모션학적 변환 섹션을 참조하세요. 좌표계 및 모션학적 변환 정의 섹션을 참조하세요. |
| MMC_SetCartesianTransform | hConn, hAxisRef, pInParam, pOutParam | 그룹 다중 축에 대해 사전 정의된 모션학 모델을 기반으로 MCS 및 PCS 매개변수 사이의 그룹의 데카르트 변환을 설정합니다. 자세한 설명은 PCS - 제품 좌표계 섹션을 참조하세요. 자세한 내용은 이후의 좌표계 및 모션학적 변환 정의 섹션을 참조하십시오. |
| MMC_TrackConveyorBelt | hConn, hAxisRef, pi_params, po_params | MMC_TrackConveyorBelt 기능은 모든 로봇 위치(MCS)에서 움직이는 컨베이어 벨트(PCS)에 있는 부품까지 부드러운 RAMP-IN 모션을 실행합니다. 로봇이 컨베이어 벨트의 목표 지점에 도달하자마자 컨베이어 벨트와 동시에 이동합니다(PCS). 즉, PCS에서 동작을 수행하는 동안 컨베이어 벨트를 추적합니다. |
| MMC_TrackRotaryTable | hConn, hAxisRef, pInParam, pOutParam | MMC_TrackRotaryTable 함수는 움직이는 회전 테이블(PCS)에 있는 부품에 대해 모든 로봇 위치(MCS)에서 부드러운 RAMP-IN 모션을 실행합니다. 로봇이 회전 테이블의 목표 지점에 도달하자마자 회전 테이블과 동기적으로 이동합니다(PCS). 즉, PCS에서 동작을 수행하는 동안 회전 테이블을 추적합니다. |
| MMC_TrackSyncOut | hConn, hAxisRef, pi_params, po_params | MMC_TrackSyncOut 기능은 정지될 때까지 동기화된 PCS 모션에서 MCS 대상 위치까지 부드러운 RAMP Out 모션을 실행합니다. |
| MMC_SetKinTransformDelta | hConn, hAxisRef, pInParam, pOutParam | Delta 로봇을 사용하여 그룹 다중 축에 대해 사전 정의된 모션학 모델을 기반으로 ACS과 MCS 사이의 모션학 변환을 설정합니다. 자세한 설명은 PCS - 제품 좌표계 섹션을 참조하세요. 좌표계 및 모션학적 변환 정의 섹션을 참조하세요. |
| MMC_SetKinTransformCartesian | hConn, hAxisRef, pInParam, pOutParam | 그룹 다중 축에 대해 사전 정의된 모션학 모델을 기반으로 Cartesian 시스템에 대한 매개변수 모션학 변환(MSC에서 ACS로)을 설정합니다. 자세한 설명은 PCS - 제품 좌표계 섹션을 참조하세요. 좌표계 및 모션학적 변환 정의 섹션을 참조하세요. |
| MMC_SetKinTransformScara | hConn, hAxisRef, pInParam, pOutParam | 그룹 다중 축에 대해 사전 정의된 모션학 모델을 기반으로 SCARA 로봇에 대한 매개변수 모션학 변환(MSC에서 ACS)을 설정합니다. 자세한 설명은 PCS - 제품 좌표계 섹션을 참조하세요. 좌표계 및 모션학적 변환 정의 섹션을 참조하세요. |
| MMC_SetKinTransformThreeLink | hConn, hAxisRef, pInParam, pOutParam | 그룹 다중 축에 대해 사전 정의된 모션학 모델을 기반으로 THREELINK 로봇에 대한 매개변수 모션학 변환(MSC에서 ACS)을 설정합니다. 자세한 설명은 PCS - 제품 좌표계 섹션을 참조하세요. 좌표계 및 모션학적 변환 정의 섹션을 참조하세요. |
| MMC_SetKinTransformHxpd | hConn, hAxisRef, i_param, o_param | 그룹 다중 축에 대해 사전 정의된 모션학 모델을 기반으로 THREELINK 로봇에 대한 매개변수 모션학 변환(MSC에서 ACS)을 설정합니다. 자세한 설명은 PCS - 제품 좌표계 섹션을 참조하세요. 좌표계 및 모션학적 변환 정의 섹션을 참조하세요. |
| MMC_GetMotionInfo | hConn, hAxisRef, pInParam, pOutParam | 여러 축의 경우. 구조 배열에 대한 정보를 제공합니다. 각 구조는 다음 정보를 반환합니다. • 사용자가 제공한 FB 인덱스. 이는 새로운 pFbCommon ->dbUserData 매개변수에 의해 내부적으로 반환됩니다. |
| MMC_AddAxisToGroup | hConn, hAxisRef, pInParam, pOutParam | 여러 축의 경우. AxesGroup 구조의 그룹에 하나의 축을 추가합니다. |
| MMC_GroupDisable | hConn, hAxisRef, pInParam, pOutParam | 여러 축의 경우. 그룹의 상태를 GroupDisabled로 변경합니다. 이는 관리 함수 블록이지만 이동이 생성되지 않기 때문입니다. |
| MMC_GroupEnable | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. 그룹의 상태를 GroupDisabled에서 GroupStandby로 변경합니다. 이것은 |
| MMC_GroupReadActualPosition | hConn, hAxisRef, pInParam, pOutParam | 다축 시스템용. 축 그룹의 선택된 좌표계에서 실제 위치를 반환합니다. 이것 |
| MMC_GroupReadActualVelocity | hConn, hAxisRef, pInParam, pOutParam | 축 그룹의 선택된 좌표계에서 실제 속도를 반환합니다. 이동이 생성되지 않으므로 이는 관리 함수 블록입니다. |
| MMC_GroupReadError | hConn, hAxisRef, pInParam, pOutParam | 함수 블록과 관련되지 않은 일반적인 축 그룹 오류에 대해 설명합니다. 움직임이 발생하지 않으므로 이는 관리 함수 블록입니다. 이 기능은 현재 사용되지 않습니다. |
| MMC_GroupReadStatus | hConn, hAxisRef, pInParam, pOutParam | 여러 축의 경우. 활성 그룹 함수 블록에 따라 축 그룹의 상태를 반환합니다. 이것은 |
| MMC_GroupReset | hConn, hAxisRef, pInParam, pOutParam | 여러 축의 경우. 모든 내부 그룹 관련 오류를 재설정하여 상태 GroupErrorStop에서 GroupDisabled로 전환합니다. 이는 함수 블록 인스턴스의 출력에 영향을 주지 않습니다. |
| MMC_GroupSetOverride | hConn, hAxisRef, pInParam, pOutParam | 다축용. 여러 축의 좌표 동작에 대한 재정의 값과 해당 축 그룹에서 작동하는 모든 기능을 설정합니다. |
| MMC_GroupSetPosition | hConn, hAxisRef, pInParam, pOutParam | 여러 축의 경우. 축을 이동하지 않고 그룹 내 모든 축의 위치를 ​​설정합니다. |
| MMC_RemoveAxisFromGroup | hConn, hAxisRef, pInParam, pOutParam | 여러 축의 경우. AxesGroup 그룹에서 하나의 축을 제거합니다. 이는 관리 함수 블록이며, |
| MMC_GroupReadParameter | hConn, hAxisRef, pInParam, pOutParam | 특정 축 매개변수 그룹을 읽습니다. |
| MMC_GroupReadBoolParameter | hConn, hAxisRef, pInParam, pOutParam | 특정 그룹 축 부울 매개변수를 읽습니다. |
| MMC_GroupWriteParameter | hConn, hAxisRef, pInParam, pOutParam | 특정 그룹 축 매개변수의 값을 수정합니다. |
| MMC_GroupWriteBoolParameter | hConn, hAxisRef, pInParam, pOutParam | 특정 그룹 축 부울 매개변수의 값을 수정합니다. |
| MMC_GetGroupMembersInfo | hConn, hAxisRef, pInParam, pOutParam | 특정 그룹 및 해당 구성원에 대한 정보를 반환합니다. |
| MMC_GetTableList | hConn, pInParam, pOutParam | 이 함수는 지정된 테이블 유형에 대한 테이블 목록을 제공합니다. |
| MMC_GetTableInfo | hConn, pInParam, pOutParam | 이 함수는 주어진 테이블 핸들러에 대한 테이블 정보(현재 이름만)를 제공합니다. |
| MMC_InitTable | hConn, pInParam, pOutParam | 이 함수는 포인트의 차원과 개수에 따라 공유 메모리에 메모리 세그먼트를 할당합니다. |
| MMC_InitTableEx | hConn, pInParam, pOutParam | 이 기능은 차원과 포인트 수에 따라 공유 메모리에 무제한 메모리 세그먼트를 할당합니다. |
| MMC_LoadTableFromFile | hConn, pInParam, pOutParam | 이 함수는 파일에 지정된 포인트의 크기와 개수에 따라 Maestro 공유 메모리에 메모리 세그먼트를 할당합니다. |
| MMC_UnloadTable | hConn, pInParam, pOutParam | 이 함수는 Maestro에서 테이블을 언로드하고 파일에 지정된 포인트 수와 차원에 따라 Maestro 공유 메모리의 메모리 세그먼트를 해제합니다. |
| MMC_MoveTable | hConn, hAxisRef, pInParam, pOutParam | 이 기능은 선택한 경로를 따라 테이블을 이동합니다. |
| MMC_AppendPointsToTable | hConn, pInParam, pOutParam | 이 함수는 기존 테이블에 포인트를 추가합니다. |
| MMC_GetTableIndex | hConn, pInParam, pOutParam | 이 함수는 PVT 인덱스를 얻습니다. |
| MMC_CamTableInit | hConn, pInParam, pOutParam | 이 함수는 ECAM 테이블에 메모리를 할당하고 저널에 함수 블록을 준비하고 초기화합니다. 일반적으로 동적 추가 옵션이 없는 MC_TableInit와 유사합니다. |
| MMC_CamTableSelect | hConn, pInParam, pOutParam | 이 함수는 입력 처리기로 테이블을 선택합니다. |
| MMC_CamTableUnload | hConn, pInParam, pOutParam | 이 함수는 Maestro에서 ECAM 테이블을 언로드하고 파일에 지정된 포인트 수와 차원에 따라 Maestro 공유 메모리의 메모리 세그먼트를 해제합니다. |
| MMC_CamTableAdd | hConn, pInParam, pOutParam | 이 함수는 기존 테이블에 포인트를 추가합니다. |
| MMC_CamTableAddEx | hConn, pInParam, pOutParam | 이 기능을 사용하면 메모리에 있는 기존 테이블에 행을 무제한으로 추가할 수 있습니다. |
| MC_CamTableSet | hConn, pInParam, pOutParam | 이 방법을 사용할 때 MMC_CamTableAdd는 메모리에서 테이블을 로드하는 데 사용됩니다. |
| MMC_CamIn | hConn, hAxisRef, pInParam, pOutParam | MC_CamIn은 CAM 프로세스를 실행합니다. |
| MMC_CamOut | hConn, hAxisRef, pInParam, pOutParam | CAM 프로세스를 해제하기 위해 슬레이브 축에서 MC_Stop을 수행합니다. |
| MMC_CamStatus | hConn, hAxisRef, pInParam, pOutParam | MC_CamStatus는 CAM 프로세스의 중요한 매개변수를 검색합니다. |
| MMC_CamSetProperty | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 CAM 함수의 특정 속성을 설정합니다. ECAM 주기 모션이 비주기 모션을 사용하여 정지되는 특정 상황을 위해 만들어졌습니다. |
| MMC_GearIn | hConn, hAxisRef, pInParam, pOutParam | 슬레이브 축과 마스터 축의 속도 사이의 비율을 정의하는 명령을 제공합니다. 이 기능은 현재 지원되지 않습니다. |
| MMC_GearInPos | hConn, hAxisRef, pInParam, pOutParam | 동기화 지점부터 슬레이브 축과 마스터 축의 위치 간 기어비를 정의하는 명령을 제공합니다. 이 기능은 현재 지원되지 않습니다. |
| MMC_GearOut | hConn, hAxisRef, pInParam, pOutParam | 슬레이브 축과 마스터 축 사이의 기어를 분리하는 명령을 제공합니다. 실제로는 이 단계에서 MC_Stop입니다. 이 기능은 현재 지원되지 않습니다. |
| MMC_ChangeToPreOPMode | hConn, pOutParam | Maestro을 사전 작동 모드로 변경합니다. |
| MMC_ChangeToOperationMode | hConn, pOutParam | Maestro을 작동 모드로 변경합니다. |
| MMC_ClearNodeFbList | hConn, pInParam, pOutParam | 이는 특정 노드(예: 축 또는 그룹)의 함수 블록 목록을 지우는 기능을 추가합니다. 이는 노드가 이동 상태가 아닌 경우에만 수행할 수 있습니다. |
| MMC_CmdStatus | hConn, pInParam, pOutParam | 특정 축/그룹에 대한 Maestro 서버에 함수 블록 상태 읽기 명령을 보내고 상태를 다시 수신합니다. |
| MMC_CloseConnection | hConn | Maestro에 대한 연결을 닫습니다. |
| MMC_Config | hConn, pInParam, pOutParam | Maestro을 구성 모드로 설정하고 모든 구성 매개변수에 대한 변경을 허용합니다. |
| MMC_CreateSYNCTimer | hConn, func, usSYNCTimerTime | 연결 핸들 연산자를 사용하여 서보 드라이브, Maestro 이동을 동기화하는 SYNC 타이머를 생성합니다. |
| MMC_DestroySYNCTimer | hConn | 연결 핸들 연산자를 사용하여 서보 드라이브, Maestro 이동을 동기화하기 위해 SYNC 타이머를 제거합니다. |
| MMC_DownloadFoE | hConn, pInParam, pOutParam | EtherCAT에서 Maestro로의 파일 다운로드를 관리합니다. 중요: 이 기능을 사용하려면 Elmo에 지원을 요청하세요. |
| MMC_Exit | hConn, pInParam, pOutParam | Maestro을 구성 모드에서 일반 모드로 다시 변경합니다. |
| MMC_FreeFbStat | hConn, pInParam, pOutParam | 시스템의 사용 가능한 함수 블록 수를 포함하는 디버그 정보를 반환합니다. |
| MMC_GetActiveVectorsNum | hConn, pInParam, pOutParam | Maestro에 의해 연결되고 관리되는 활성 벡터(그룹) 수를 표시합니다. |
| MMC_GetErrorCodeDescriptionByID | hConn, pInParam, pOutParam | 이 함수는 오류\경고 코드를 수신하고 Personality 파일에서 설명과 해결 방법을 반환합니다. |
| MMC_GetFoEStatus | hConn, pInParam, pOutParam | MMC_DownloadFoE를 사용하여 호스트에서 Maestro로 파일을 다운로드한 후 EtherCAT에 대한 파일 상태를 얻습니다. 중요: 이 기능을 사용하려면 Elmo에 지원을 요청하세요. |
| MMC_GetEnquireFbStatus | hConn, pInParam, pOutParam | 현재 상태 전역 매개변수를 가져옵니다. EAS에서 FB 상태를 수신합니다. |
| MMC_GetAxisByName | hConn, pInParam, pOutParam | 이름으로 축 인덱스 참조를 반환합니다. |
| MMC_GetGroupByName | hConn, pInParam, pOutParam | 이 함수는 이름으로 그룹 인덱스 참조를 반환합니다. |
| MMC_GetGMASOperationMode | hConn, pOutParam | 현재 GMAS 작업 모드를 반환합니다. |
| MMC_GetStatusRegister | hConn, hAxisRef, pInParam, pOutParam | 이 함수의 목적은 Maestro 및 축 상태에 관한 유용한 정보를 제공하는 것입니다. |
| MMC_GetResList | hConn, pInParam, pOutParam | 모든 리소스 파일의 목록을 반환합니다. |
| MMC_GetResSnapshot | hConn, pInParam, pOutParam | 리소스 구성을 임시 스냅샷 파일에 저장합니다. |
| MMC_GetVersion | hConn, sVersion | 출력 매개변수에서 Maestro 버전을 얻습니다. |
| MMC_GetVersionEx | hConn, sVersion | 출력 매개변수에서 Maestro 확장 버전을 얻습니다. |
| MMC_GetLastError | hConn, chStr, iSize | 지정된 연결에서 발생한 마지막 오류를 반환합니다. |
| MMC_InitConnection | eType, sConnParam, pCbFunc, pHndl | Maestro 서버에 대한 연결을 시작합니다. |
| MMC_RpcInitConnection | eType, sConnParam, pCbFunc, cpHostIPAddr, pHndl | Maestro 서버에 대한 RPC 연결을 시작합니다. |
| MMC_RpcInitConnectionEx | eType, sConnParam, pCbFunc, cpHostIPAddr, pHndl | Maestro 서버에 대한 RPC 연결을 시작합니다. |
| MMC_IPCInitConnection | sConnParam, pCbFunc, pHndl | Maestro 서버에 대한 IPC 연결을 시작합니다. |
| MMC_LoadParam | hConn, pInParam, pOutParam | 다음 위치의 xml 파일에서 축, 그룹 및 전역 매개변수를 로드합니다. |
| MMC_ResetMultiAxisControl | hConn, pInParam, pOutParam | Maestro 다축 제어의 내부 재설정. Maestro의 CPU를 재설정할 수 있습니다. |
| MMC_ResExportFile | hConn, pInParam, pOutParam | Maestro에서 TFTP을 통해 호스트로 요청된 파일을 복사합니다. |
| MMC_ResImportFile | hConn, pInParam, pOutParam | TFTP을 통해 호스트에서 Maestro로 요청된 파일을 복사합니다. MC_LIB_API int MMC_ResImportFileCmd( |
| MMC_SaveParam | hConn, pInParam, pOutParam | Maestro의 축, 그룹 및 전역 매개변수를 다음 파일에 저장 및/또는 업데이트합니다. |
| MMC_SetEnquireFbStatus | hConn, pInParam, pOutParam | EASII에서 상태 전역 매개변수 수신 FB 상태를 설정합니다. |
| MMC_SetDefaultParameters | hConn, hAxisRef, pInParam, pOutParam | Maestro 기본 제조업체 매개변수를 Maestro의 특정 축 또는 그룹으로 설정합니다. |
| MMC_SetDefaultParametersGlobal | hConn, pInParam, pOutParam | Maestro에서 Maestro 기본 제조업체 전역 매개변수를 설정합니다. |
| MMC_SetIsToLoadGlobalParams | hConn, pInParam, pOutParam | 파일에서 Maestro로 전역 매개변수를 업데이트할 때 전역 매개변수를 로드할지 여부에 대한 플래그를 정의합니다. |
| MMC_ShowNodeStat | hConn, hAxisRef, pInParam, pOutParam | 축/그룹에 대한 디버그 정보를 표시합니다. |
| MMC_GetActiveAxesNum | hConn, pInParam, pOutParam | Maestro에 의해 연결되고 관리되는 활성 축 수를 표시합니다. |
| MMC_ToggleConsoleOutput | hConn, pInParam, pOutParam | 콘솔 출력을 토글합니다. 지금은 이 기능을 사용할 수 없습니다. |
| MMC_GetCyclesCounter | hConn, pInParam, pOutParam | Maestro 사이클 카운터 값을 얻습니다. |
| MMC_WriteGroupOfParameters | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 배열 매개변수 그룹을 Maestro에 씁니다. |
| MMC_WriteGroupOfParametersEx | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 일반 및 PI 함수 매개변수 그룹을 Maestro에 씁니다. |
| MMC_ReadGroupOfParameters | hConn, pInParam, pOutParam | 이 함수는 사용자에게 매개변수 그룹을 검색합니다. |
| MMC_WaitUntilConditionFB | hConn, hAxisRef, pInParam, pOutParam | 이 함수 블록을 작동하면 그룹에 속하지 않은 여러 축을 동기화하여 함께 동작을 시작할 수 있습니다. 또한 공유 IO의 특정 비트가 상승할 때 동작을 시작하여 네트워크로 연결된 수많은 Maestro의 동기화를 허용합니다. |
| MMC_WaitUntilConditionFBEx | hConn, hAxisRef, pInParam, pOutParam | 이 함수 블록의 작동은 정적 기능과 PI 기능 모두에 적용되며 그룹에 속하지 않은 수많은 축을 동기화하여 함께 모션을 시작할 수 있습니다. 또한 공유 IO의 특정 비트가 상승할 때 동작을 시작하여 네트워크로 연결된 수많은 Maestro의 동기화를 허용합니다. |
| MMC_WriteMemoryRange | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 EtherCAT 슬레이브에 대한 메모리 범위를 씁니다. |
| MMC_ReadMemoryRange | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 EtherCAT 슬레이브에서 메모리 범위를 읽습니다. |
| MMC_SetDefaultResources | hConn, pInParam, pOutParam | 이 기능은 원하는 통신 유형에 따라 Maestro 리소스 파일을 공장 기본값으로 복원합니다. eCOMM_TYPE_ETHERCAT 또는 eCOMM_TYPE_CAN |
| MMC_UserCommandControl | hConn, pInParam, pOutParam | 이 함수는 사용자 명령(사용자 프로그램 또는 LINUX 명령 실행)을 실행합니다. |
| MMC_SetAllFbExeModeImm | hConn, pInParam, pOutParam | 이 기능은 모든 함수 블록을 즉시 실행 모드로 설정합니다. |
| MMC_BeginRecordingEx | hConn, pInParam, pOutParam | Maestro 서버에서 내부 컨트롤러 변수 및 PI 변수 데이터 기록을 시작합니다. |
| MMC_ReadPIVarBOOL | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부울 변수를 읽습니다. |
| MMC_ReadPIVarChar | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 문자 변수를 읽습니다. |
| MMC_ReadPIVarUChar | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부호 없는 문자 변수를 읽습니다. |
| MMC_ReadPIVarShort | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 단축 변수를 읽습니다. |
| MMC_ReadPIVarUShort | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부호 없는 짧은 변수를 읽습니다. |
| MMC_ReadPIVarInt | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 정수 변수를 읽습니다. |
| MMC_ReadPIVarUInt | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부호 없는 정수 변수를 읽습니다. |
| MMC_ReadPIVarFloat | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부동 변수를 읽습니다. |
| MMC_ReadPIVarRaw | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 RAW 변수를 읽습니다. 여기서 변수는 32비트 이하입니다. |
| MMC_ReadPIVarLongLong | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 Long Long 변수를 읽습니다. |
| MMC_ReadPIVarULongLong | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부호 없는 Long Long 변수를 읽습니다. |
| MMC_ReadPIVarDouble | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 이중 변수를 읽습니다. |
| MMC_ReadLargePIVarRaw | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 큰 Processing Image 입력\출력 RAW 변수를 읽습니다. 여기서 변수는 32비트보다 큽니다. |
| MMC_WritePIVarBool | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부울 변수를 작성합니다. |
| MMC_WritePIVarChar | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 문자 변수를 씁니다. |
| MMC_WritePIVarUChar | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부호 없는 문자 변수를 씁니다. |
| MMC_WritePIVarUShort | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image input\output Unsigned Short 변수를 작성합니다. |
| MMC_WritePIVarShort | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image input\output Short 변수를 작성합니다. |
| MMC_WritePIVarUInt | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부호 없는 정수 변수를 작성합니다. |
| MMC_WritePIVarInt | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 정수 변수를 작성합니다. |
| MMC_WritePIVarFloat | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부동 변수를 작성합니다. |
| MMC_WritePIVarRaw | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 RAW 변수를 작성합니다. |
| MMC_WritePIVarULongLong | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 부호 없는 Long Long 변수를 작성합니다. |
| MMC_WritePIVarLongLong | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 Long Long 변수를 작성합니다. |
| MMC_WritePIVarDouble | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 Processing Image 입력\출력 이중 변수를 작성합니다. |
| MMC_WriteLargePIVarRaw | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 큰 Processing Image 입력\출력 RAW 변수를 작성합니다. |
| MMC_GetPIVarInfo | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 인덱스에 따라 변수를 읽어 필수 Processing Image 변수에 대한 자세한 정보를 반환합니다. |
| MMC_GetPIVarInfoByAlias | hConn, hAxisRef, pInParam, pOutParam | 이 함수는 변수 별칭을 키로 읽어 매핑된 Processing Image 변수의 세부 개수를 반환합니다. |
| MMC_GetPIVarsRangeInfo | hConn, hAxisRef, pInParam, pOutParam | 이 기능을 사용하면 사용자는 PI 변수 범위의 정보를 업로드할 수 있습니다. |
| MMC_GePIMemOffset | hConn, pInParam, pOutParam | 이 함수는 Maestro에 대한 PI 메모리 오프셋을 제공합니다. |
| MMC_PerformBulkReadCmdPI | hConn, pInParam, pOutParam | 이 기능을 사용하면 사용자는 매개변수의 PI 대량 읽기를 수행할 수 있습니다. |
| MMC_BeginRecording | hConn, pInParam, pOutParam | Maestro 서버에서 내부 컨트롤러 변수 데이터 기록을 시작합니다. |
| MMC_StopRecording | hConn, pInParam, pOutParam | Maestro 서버 데이터 기록을 중지합니다. |
| MMC_UploadData | hConn, pInParam, pOutParam | Maestro에 녹음 데이터를 업로드합니다. |
| MMC_RecStatus | hConn, pInParam, pOutParam | 녹화상태를 요청합니다. |
| MMC_UploadDataHeader | hConn, pOutParam | 레코더 업로드 데이터 헤더. |
| MMC_ConfigBulkRead | hConn, pInParam, pOutParam | 여러 축의 모든 매개변수를 읽는 기능을 구성합니다. |
| MMC_PerformBulkRead | hConn, pInParam, pOutParam | 여러 축에서 ConfigBulkRead 호출로 구성된 매개변수를 읽습니다. |
| MMC_InsertNotificationFb | hConn, hAxisRef, pInParam, pOutParam | 이벤트를 트리거하기 위해 대기열 내에 알림 함수 블록을 삽입합니다. 자세한 내용은 GlobalAsyncReply_Received(C++) 섹션을 참조하세요. |
| MMC_ClearEventsMask | hConn, pInParam, pOutParam | 입력 마스크에 따라 특정 연결에 대한 이벤트 마스크를 지웁니다. |
| MMC_DisableMotionEndedEvent | hConn, hAxisRef, pInParam, pOutParam | 특정 노드에 대한 모션 종료 이벤트 메커니즘을 비활성화하고 모션 진행과 관련하여 Maestro에서 피드백이 전송되지 않습니다. |
| MMC_EnableMotionEndedEvent | hConn, hAxisRef, pInParam, pOutParam | Enables는 특정 노드에 대한 모션 종료 이벤트 메커니즘입니다. |
| MMC_GetEventsMask | hConn, pInParam, pOutParam | 특정 연결에 대한 32비트 이벤트 마스크를 반환합니다. |
| MMC_SetEventsMask | hConn, pInParam, pOutParam | 입력 마스크 매개변수 iEventsMask 로 정의된 특정 연결에 대한 32비트 이벤트 마스크를 설정합니다. |
| MMC_LoadErrorCorrTable | hConn, pInParam, pOutParam | 오류 수정 테이블을 메모리에 로드합니다. 그런 다음 이 표에 따라 오류 수정이 수행됩니다. |
| MMC_EnableErrorCorrTable | hConn, pInParam, pOutParam | Enable은 오류 수정 테이블의 사용법입니다. |
| MMC_GetErrorTableStatus | hConn, pInParam, pOutParam | 함수는 테이블 번호를 입력으로 받고 테이블이 로드되거나 활성화되었는지 여부에 대한 답변을 반환합니다. |
| MMC_DisableErrorCorrTable | hConn, pInParam, pOutParam | 오류 수정 테이블의 사용을 비활성화합니다. |
| MMC_UnloadErrorCorrTable | hConn, pInParam, pOutParam | 메모리에서 오류 수정 테이블을 언로드합니다. |
| Open | cFileName, uiFlags, cFilePath | 특정 매개변수를 사용하여 XML 파일을 엽니다. |
| Close |  | XML 파일을 가리키는 파일을 닫고 파일 구문 분석에 사용되는 리소스를 해제합니다. |
| Read | pCtgryVal, pTagName, dVal, lVal, bVal, pStr, dDefault, lDefault, bDefault, lMin, dMax, lMax, lLen, iActRdElm, iReqRdElm, dMin | 주어진 변수에 대한 데이터를 검색하는 오버로드된 함수 목록입니다. 값은 매개변수의 수, 유형 및 순서에 따라 double(단일 또는 배열), long(단일 또는 배열), 부울 또는 문자열일 수 있습니다. 단일 값 매개변수 읽기 • Double, Double 유형의 매개변수 하나 검색 • Long, Long 유형의 매개변수 하나 검색 • Boolean, Boolean 유형의 매개변수 하나 검색, 공백 무시, True/False 예상 |
| GetXmlFileRoot | pAtt1, pAtt2, lLen | XML 파일 루트(XSI ID 값) pAtt1 및 XSI 위치를 반환합니다. |
| GetXmlFileDescrp | pAtt1, pAtt2, lLen | pAtt1로 표시되는 XML "파일 설명 이름" 및 XML 파일 버전을 pAtt2로 반환합니다. 반환 값의 버퍼 크기는 크기가 최소 1Len입니다. |
| MMC_CloseUdpChannel | hConn, pInParam, pOutParam | RPC/IPC 연결당 UDP 채널을 닫습니다. |
| MMC_GetDefGateway | hConn, pInParam, pOutParam | 기본 게이트웨이 IP 주소를 읽습니다. |
| MMC_GetDhcp | hConn, pInParam, pOutParam | DHCP 모드를 읽습니다. |
| IMMC_GetIpAddr | hConn, pInParam, pOutParam | DHCP 모드를 읽습니다. |
| MMC_GetIpMask | hConn, pInParam, pOutParam | IP 마스크를 읽습니다. |
| MMC_GetServerIp | hConn, pInParam, pOutParam | 서버 IP 주소를 얻습니다. |
| MMC_NetworkInfo | hConn, pInParam, pOutParam | Maestro FLASH에 있는 리소스 파일에 연결 및/또는 정의된 시스템을 자세히 설명하는 네트워크 정보를 반환합니다. |
| MMC_NetworkScan | hConn, pInParam, pOutParam | 네트워크를 스캔하여 네트워크에서 노드를 찾습니다. |
| MMC_OpenUdpChannel | hConn, pInParam, pOutParam | RPC/IPC 연결당 UDP 채널을 엽니다. |
| MMC_SetDefGateway | hConn, pInParam, pOutParam | 기본 게이트웨이 IP 주소를 설정합니다. |
| MMC_SetDhcp | hConn, pInParam, pOutParam | Maestro에 대한 DHCP 모드를 설정합니다. |
| MMC_SetIpAddr | hConn, pInParam, pOutParam | Maestro IP 주소를 설정합니다. |
| MMC_SetIpMask | hConn, pInParam, pOutParam | Maestro의 IP 넷마스크를 설정합니다. |
| MMC_SetServerIp | hConn, pInParam, pOutParam | 호스트의 서버 IP 주소를 설정합니다. |
| MMC_MbusIsRunning | hConn, pInParam, pOutParam | Modbus 연결이 작동 중임을 나타냅니다. |
| MMC_MbusReadCoilsTable | hConn, pInParam, pOutParam | Modbus 코일 테이블의 일부를 읽습니다. |
| MMC_MbusReadHoldingRegisterTable | hConn, pInParam, pOutParam | Modbus 보유 레지스터 테이블 또는 보유 레지스터의 일부를 읽습니다. |
| MMC_MbusReadInputsTable | hConn, pInParam, pOutParam | Modbus 입력 테이블에 대한 입력을 읽습니다. |
| MMC_MbusStartServer | hConn, pInParam, pOutParam | ID 값을 매개변수로 사용하여 Modbus 서버 수신 스레드를 시작합니다. |
| MMC_MbusStopServer | hConn, pInParam, pOutParam | Modbus 서버 수신 스레드를 중지합니다. |
| MMC_MbusWriteCoilsTable | hConn, pInParam, pOutParam | 모든 매개변수 >0이 부울 값 1과 유사한 Modbus 내부의 Modbus 코일 테이블 부분에 씁니다. |
| MMC_MbusWriteHoldingRegisterTable | hConn, pInParam, pOutParam | Modbus 내부의 Modbus 레지스터 테이블의 일부에 씁니다. |
| MMC_CancelVirtualEncoder | hConn, hAxisRef, pInParam, pOutParam | 이 기능은 가상 CAN 인코더로 정의된 서보 드라이브를 취소합니다. |
| MMC_CancelParamEvPDO3 | hConn, hAxisRef, pInParam, pOutParam | TPDO3 및 RXPDO3 이벤트 처리를 취소합니다. |
| MMC_CancelParamEvPDO4 | hConn, hAxisRef, pInParam, pOutParam | TPDO4 및 RXPDO4 이벤트 처리를 취소합니다. |
| MMC_CfgRegParamEvPDO3 | hConn, hAxisRef, pInParam, pOutParam | 그룹 유형에 따라 일반 매개변수 이벤트 PDO3을 구성합니다. |
| MMC_CfgRegParamEvPDO4 | hConn, hAxisRef, pInParam, pOutParam | 그룹 유형에 따라 일반 매개변수 이벤트 PDO4를 구성합니다. |
| MMC_CfgUserParamEvPDO3 | hConn, hAxisRef, pInParam, pOutParam | 그룹 유형에 따라 사용자 매개변수 이벤트 PDO3을 구성합니다. |
| MMC_CfgUserParamEvPDO4 | hConn, hAxisRef, pInParam, pOutParam | 그룹 유형에 따라 사용자 매개변수 이벤트 PDO4를 구성합니다. |
| MMC_ChangeDefaultPDOConfiguration | hConn, hAxisRef, pInParam, pOutParam | 기본 PDO 통신 매개변수를 변경합니다. |
| MMC_ConfigEventModePDO3 | hConn, hAxisRef, pInParam, pOutParam | 그룹 유형에 따라 PDO3에 대한 이벤트 모드를 구성합니다. |
| MMC_ConfigEventModePDO4 | hConn, hAxisRef, pInParam, pOutParam | 그룹 유형에 따라 PDO4에 대한 이벤트 모드를 구성합니다. |
| MMC_ConfigVirtualEncoder | hConn, hAxisRef, pInParam, pOutParam | 이 기능은 서보 드라이브를 가상 CAN 인코더로 정의합니다. |
| MMC_GetAxisByCanId | hConn, pInParam, pOutParam | CANbus ID에 따라 축 핸들을 얻습니다. |
| MMC_GetPDOInfo | hConn, hAxisRef, pInParam, pOutParam | PDO 3 및 4의 PDO 정보를 얻습니다. |
| MMC_GetSyncTime | hConn, pInParam, pOutParam | CANbus 통신이 관련된 경우 SYNC 시간을 반환합니다. |
| MMC_PDOGeneralRead | hConn, hAxisRef, pInParam, pOutParam | 특정 PDO 메시지 명령을 읽습니다. |
| MMC_PDOGeneralWrite | hConn, hAxisRef, pInParam, pOutParam | 특정 PDO 메시지 명령을 작성합니다. |
| MMC_ReceiveCANRawData | hConn, hAxisRef, iTimeOutms, pOutParam | 준비된 CANopen RAW 데이터(DS-301 또는 DS-402)를 수신합니다. |
| MMC_SendCANRawData | hConn, hAxisRef, pInParam, pOutParam | 준비된 CANopen RAW 데이터(DS-301 또는 DS-402)를 보냅니다. |
| MMC_SendandReceiveCANRawData | hConn, hAxisRef, pInParam, pOutParam | 준비된 CANopen RAW 데이터(DS-301 또는 DS-402)를 보내고 받습니다. |
| MMC_SendCmd | hConn, hAxisRef, pInParam, pOutParam | 작동하지 않음 명령 문자열을 드라이브로 보냅니다. |
| MMC_SetHeartBeatConsumer | hConn, pInParam, pOutParam | 소비자 하트비트를 사용자에 대한 이벤트로 설정합니다. |
| MMC_SetSyncTime | hConn, pInParam, pOutParam | CANbus 통신이 해당되는 경우 통신 모듈에서 동기화 시간을 설정하고 모션 모드에 IP 주소가 있는 해당 노드를 업데이트합니다. 또한 동기화 시간으로 커널을 업데이트합니다. |
| MMC_StartBulkUpload | hConn, hAxisRef, pInParam, pOutParam | Maestro은 호스트의 요청 시 대량 업로드 프로세스를 관리합니다. 즉, 호스트는 이 기능 명령을 Maestro에 보내고 Maestro은 녹화 버퍼를 업로드합니다. |
| MMC_GetBulkUploadStatus | hConn, hAxisRef, pInParam, pOutParam | 일괄 업로드의 전체 프로세스 중에 이 함수 명령을 사용하여 상태를 검색합니다. |
| MMC_GetBulkUploadData | hConn, hAxisRef, pInParam | Maestro은 호스트의 요청 시 업로드 프로세스를 관리합니다. 즉, 호스트는 "업로드 시작" 명령을 보내고 Maestro은 녹화 버퍼를 업로드합니다. 그 후 호스트는 이 함수를 보냅니다. |
| MMC_ResetCommStatistics | hConn, pInParam, pOutParam | 모든 통신 통계를 재설정합니다. 통신 오류 카운터를 재설정합니다. |
| MMC_SendSDO | hConn, hAxisRef, pInParam, pOutParam, objectIndex, objectSubIndex, data, dataLength, timeout | SDO 메시지 명령을 1, 2, 4바이트 단위로 보냅니다. |
| MMC_SendSDOEx | hConn, hAxisRef, pInParam, pOutParam, objectIndex, objectSubIndex, data, dataLength, timeout | SDO 메시지 명령을 1, 2, 4바이트 단위로 보냅니다. |
| MMC_SendSdoAsync | hConn, hAxisRef, pInParam, pOutParam | SDO 비동기 메시지 명령을 1, 2 또는 4바이트 단위로 보냅니다. |
| MMC_RetrieveSDOAsync | hConn, hAxisRef, pOutParam | SDO 비동기 메시지 명령을 1, 2 또는 4바이트 단위로 보냅니다. |
| MMC_SendSdoAsyncEx | hConn, hAxisRef, pInParam, pOutParam | SDO 비동기 메시지 명령을 1, 2 또는 4바이트 단위로 보냅니다. |
| MMC_CancelGeneralRPDO3 | hConn, hAxisRef, pInParam, pOutParam | PDO3에서 RX에 대한 DS-401 노드 또는 Maestro의 일반 구성을 취소합니다. |
| MMC_CancelGeneralRPDO4 | hConn, hAxisRef, pInParam, pOutParam | PDO4에서 RX에 대한 DS-401 노드 또는 Maestro의 일반 구성을 취소합니다. |
| MMC_CancelGeneralTPDO3 | hConn, hAxisRef, pInParam, pOutParam | PDO3에서 TX에 대한 DS-401 노드 또는 Maestro의 일반 구성을 취소합니다. |
| MMC_CancelGeneralTPDO4 | hConn, hAxisRef, pInParam, pOutParam | PDO4에서 TX에 대한 DS-401 노드 또는 Maestro의 일반 구성을 취소합니다. |
| MMC_ConfigGeneralRPDO3 | hConn, hAxisRef, pInParam, pOutParam | 일반적으로 PDO3에서 RX용 DS-401 노드 또는 Maestro을 구성합니다. |
| MMC_ConfigGeneralRPDO4 | hConn, hAxisRef, pInParam, pOutParam | 일반적으로 PDO4에서 RX용 DS-401 노드 또는 Maestro을 구성합니다. |
| MMC_ConfigGeneralTPDO3 | hConn, hAxisRef, pInParam, pOutParam | 일반적으로 PDO3의 TX에 대해 DS-401 노드 또는 Maestro을 구성합니다. |
| MMC_ConfigGeneralTPDO4 | hConn, hAxisRef, pInParam, pOutParam | 일반적으로 PDO4의 TX에 대해 DS-401 노드 또는 Maestro을 구성합니다. |
| MMC_DisableDS401DIChangedEvent | hConn, hAxisRef, pInParam, pOutParam | I/O 모듈에 대한 DS401 디지털 입력 이벤트 변경을 비활성화합니다. |
| MMC_EnableDS401DIChangedEvent | hConn, hAxisRef, pInParam, pOutParam | Enables는 DS401 디지털 입력 이벤트 변경입니다. |
| MMC_ReadDS401DIGroup | hConn, hAxisRef, pInParam, pOutParam | 8개 디지털 I/O 그룹의 DS-401 디지털 입력을 읽습니다. |
| MMC_ReadDS401DInput | hConn, hAxisRef, pInParam, pOutParam | 한 번의 작업으로 모든 64비트 I/O의 DS-401 디지털 입력을 읽어 8개의 I/O로 구성된 8개의 그룹을 읽는 것에 비해 통신 속도를 비례적으로 높입니다. |
| MMC_WriteDS401DOGroup | hConn, hAxisRef, pInParam, pOutParam | 8개 I/O 그룹의 DS-401 디지털 출력을 Maestro에 씁니다. |
| MMC_WriteDS401DOutput | hConn, hAxisRef, pInParam, pOutParam | TPDO1에 할당된 모든 DS-401 디지털 출력에 한 번에 쓰기(한 번의 작업으로 최대 64비트 I/O). 8개의 I/O로 구성된 8개의 그룹에 쓰는 것에 비해 통신 속도가 비례적으로 증가합니다. |
| GetSlaveScanAlias | hConn, pInParam, pOutParam | 새로운 API이 추가되었습니다: MC_GetSlaveScanAlias ​​C: |
| MMC_DisableEthercatConfigMode | hConn, pOutParam | EtherCAT 구성 모드를 비활성화합니다. Enables는 게이트웨이를 통한 Maestro의 직접 프로그래밍을 비활성화하는 Maestro 작업 관리자입니다. |
| MMC_EnableEthercatConfigMode | hConn, pOutParam | 게이트웨이를 통해 Maestro을 직접 프로그래밍할 수 있도록 Maestro 작업 관리자를 비활성화합니다. |
| MMC_ECATIODisableDIChangedEvent | hConn, hAxisRef, pInParam, pOutParam | I/O 모듈에 대한 EtherCAT I/O 입력 이벤트 변경을 비활성화합니다. |
| MMC_ECATIOEnableDIChangedEvent | hConn, hAxisRef, pInParam, pOutParam | Enable은 EtherCAT I/O 입력 이벤트 변경입니다. |
| MMC_ECATIOReadDigitalInput | hConn, hAxisRef, pInParam, pOutParam | 한 번의 작업으로 모든 64비트 I/O의 EtherCAT I/O 입력을 읽습니다. 이는 8개의 I/O로 구성된 8개의 그룹을 읽는 것에 비해 통신 속도를 비례적으로 증가시킵니다. |
| MMC_ECATIOReadAnalogInput | hConn, hAxisRef, pInParam, pOutParam | EtherCAT I/O 아날로그 입력을 읽습니다. |
| MMC_ECATIOWriteAnalogOutput | hConn, hAxisRef, pInParam, pOutParam | EtherCAT I/O 아날로그 출력에 씁니다. |
| MMC_ECATIOWriteDigitalOutput | hConn, hAxisRef, pInParam, pOutParam | 한 번의 작업으로 모든 64비트 I/O의 EtherCAT I/O 출력에 쓰므로 8개 I/O의 8 x 그룹에 쓰는 것에 비해 통신 속도가 비례적으로 증가합니다. |
| MMC_GetCommStatistics | hConn, hAxisRef, pInParam, pOutParam | 특정 축에 대한 통신 통계를 수신합니다. 이 함수보다는 MMC_GetEthercatCommStatistics 함수를 사용하는 것이 좋습니다. 그만큼 |
| MMC_GetEthercatCommStatistics | hConn, pInParam, pOutParam | EAS 애플리케이션에서 FoE 다운로드 메커니즘의 일부로 사용되는 EtherCAT 통신 통계를 가져옵니다. |
| MMC_GetCommDiagnostics | hConn, pInParam, pOutParam | 특정 축에 대한 통신 진단을 수신합니다. |
| MMC_GetReactorStatistics | hConn, hAxisRef, pInParam, pOutParam | Maestro 서버 기반 프로세서에서 통계를 얻습니다. |
| MMC_IsEthercatConfigMode | hConn, pOutParam | EtherCAT 구성 모드가 작동하는지 여부를 정의합니다. |
| MMC_ResetCommDiagnostics | hConn, pInParam, pOutParam | 버스에 있는 모든 슬레이브의 CRC 카운터 레지스터를 0으로 재설정합니다. CRC 카운터 레지스터는 GetCommDiagnostics 기능을 통해 검색할 수 있습니다. |
| MMC_ResetCommStatistics | hConn, pInParam, pOutParam | 모든 통신 통계를 재설정합니다. 통신 오류 카운터를 재설정합니다. |
| MMC_ElmoExecuteLabel | hConn, hAxisRef, pInParam, pOutParam | Executes는 EAS 응용 프로그램을 통해 다운로드된 사용자 프로그램입니다. |
| MMC_ElmoSetParameter | hConn, hAxisRef, ucValType, pVal | 서보 드라이브에 특정 이름으로 Elmo 드라이브 매개변수를 설정합니다. |
| MMC_ElmoGetParameter | hConn, hAxisRef, ucValType | 서보 드라이브로부터 Elmo 매개변수 수신을 요청합니다. |
| MMC_ElmoGetParameterAndRetrieveData | hConn, hAxisRef, ucValType, pVal, uiErrorID | 서보 드라이브의 매개변수를 동기적으로 요청하고 이를 검색합니다. |
| MMC_ElmoQueryOperationFIFOIndex | hConn, hAxisRef, iReceivedMsgIdx | FIFO 인덱스를 반환합니다. |
| MMC_ElmoQueryOperationFIFORetrieveData | hConn, hAxisRef, pVal, uiErrorID | 데이터를 검색하려면 FIFO 인덱스를 요청하세요. |
| MMC_ElmoQueryOperationFIFOIndexReset | hConn, hAxisRef | FIFO 메시지를 0으로 지웁니다. |
| MMC_ElmoCall | hConn, hAxisRef | ElmoCall은 사용자 프로그램인 서브루틴을 호출하는 데 사용됩니다. 여기서 cCmd[3]은 프로그램 이름입니다. |
| EipWriteAdpTag | pInParam, pOutParam | 태그 유형에 따라 어댑터 태그 데이터를 씁니다. |
| EipReadAdpTag | pInParam, pOutParam | 태그 유형에 따라 어댑터 태그 데이터를 읽습니다. 어댑터 태그 데이터를 메모리에서 입력 버퍼로 복사합니다. |
| EipGetAssemblyRefByInstance | pInParam, pOutParam | 인스턴스 참조에 따라 어셈블리 정보를 읽습니다. asm_instance를 찾고 이 인스턴스에 참조를 적용합니다. |
| EipGetAssemblyRefByName | pInParam, pOutParam | reference 이름에 따라 어셈블리 정보를 읽습니다. 이 함수는 해당 이름에 따라 어셈블리 참조 인덱스를 반환합니다. |
| EipSetAssembly | pInParam, pOutParam | out_buff 데이터로 어셈블리 데이터를 채워서 EthernetIP을 통해 보냅니다. |
| EipGetAssembly | pInParam, pOutParam | 인스턴스로 식별된 어셈블리 데이터를 in_buff에 복사합니다. |
| EipGetDevTagRefByName | pInParam, pOutParam | 이 함수는 이름에 따라 장치 태그 참조 인덱스를 반환합니다. |
| EipSetDevTag | pInParam, pOutParam | 태그 종류에 따라 디바이스 태그 데이터를 씁니다. 장치 태그 데이터를 업데이트하고 이를 EIP 장치로 보냅니다. |
| EipGetDevTag | pInParam, pOutParam | 태그 종류에 따라 디바이스 태그 데이터를 읽습니다. 특정 장치 태그를 읽으라는 요청을 EIP 장치에 보냅니다. |
| EipReadDevTagData | pInParam, pOutParam | 사용자 요청에 대한 응답으로 EIP 장치에서 수신된 장치 태그 데이터를 읽고 저장합니다. |
| EipSyncGetDevTag | pInParam, pOutParam | 디바이스 태그 데이터 읽기 요청을 보내고 응답을 기다립니다. |
| EipCheckDevTagReply | pInParam, pOutParam | 특정 장치 태그 요청에 대한 응답이 수신되었는지 확인하세요. |
| EipOpenSession | pCallBackFunc, pInParam, pOutParam | EthernetIP을 사용하려면 EIP 세션을 초기화하고 시작하세요. |
| EIPCloseSession | pInParam, pOutParam | 프로그램을 종료하기 전에 EtherNETIP 세션을 닫고 할당된 메모리를 해제하세요. |
| EipCreate | pInParam, pOutParam | EtherNetIP 세션을 만듭니다. |
| EipDestroy | pInParam, pOutParam | EtherNETIP 세션을 종료합니다. |

# 구현 API 목록

## 연결

- `LMC_RpcInitConnection`
- `LMC_CloseConnection`

## 단축

- `LMC_PowerCmd`
- `LMC_Reset`
- `LMC_StopCmd`
- `LMC_ReadStatusCmd`
- `LMC_ReadActualPositionCmd`
- `LMC_MoveAbsoluteExCmd`
- `LMC_MoveRelativeExCmd`
- `LMC_MoveVelocityExCmd`

## 그룹

- `LMC_GetGroupMembersInfo`
- `LMC_PowerMembers`
- `LMC_SetKinTransformCartesian4Axis`
- `LMC_GroupEnableCmd`
- `LMC_GroupDisableCmd`
- `LMC_GroupResetCmd`
- `LMC_GroupStopCmd`
- `LMC_GroupReadStatusCmd`
- `LMC_GroupReadActualPosition`
- `LMC_MoveLinearAbsoluteExCmd`

## 권장 순서

그룹 시작:

`Connect → GetMembers → PowerMembers(true) → SetKinTransform → GroupEnable → MoveLinear`

단축으로 복귀:

`GroupStop → GroupDisable → PowerMembers(false) → Axis PowerOn → Axis Move`

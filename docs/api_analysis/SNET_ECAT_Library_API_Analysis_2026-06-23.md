# SNET-ECAT Chapter6 Library API 분석 (2026-06-23)

## 분석 대상

- 원본: `C:/work/자료/EMotion/SNET-ECAT-User-Manual-25.05.08-ko/SNET-ECAT User Manual 25.05.08 ko/Chapter6_Library_(250508).pdf`
- PDF 정보: 435 pages, title `SNET-P`, 생성/수정일 2025-05-09
- 추출 기준: PDF 목차/섹션명에서 `eSnet*` API를 추출했다. 자동 추출 기준 목차 API 섹션은 300개, 원문 전체의 unique `eSnet*` 심볼은 371개다. 차이는 예제/enum/중복 참조가 포함되기 때문이다.

## 결론

SNET API는 `net + axis` 중심의 장치 제어 라이브러리다. 단축 이송, 보간 이송, 연속 보간, 위치/속도 override, gantry, IO, trigger, capture, EtherCAT CoE/ESC 접근을 폭넓게 제공한다. 특히 위치 동기 trigger/capture, RTEX/ECAT fieldbus capture, ADC/DAC 같은 장비 I/O 계측 기능이 강하다.

반대로 Maestro의 Group Motion처럼 group object를 만들고 `GroupReadStatus`, transition mode, kinematic transform으로 다축을 관리하는 구조는 문서상 직접 대응되지 않는다. SNET은 다축 보간과 연속 보간이 중심이고, 그룹 상태/블렌딩 의미는 별도 설계가 필요하다.

## API 영역별 정리

| 영역 | API 수 | 대표 API | 판단 |
|---|---:|---|---|
| 4. 제어기 연결하기 | 5 | `eSnetConnect`, `eSnetDisconnect`, `eSnetConnectEx`, `eSnetConnectToTcp`, `eSnetConnectToTcpEx` | 기본 관리/상태/파라미터 기능. |
| 5. Dll,Os,Fpga 프로그램 버전 정보 확인 | 3 | `eSnetGetApiVersion`, `eSnetGetOsVersion`, `eSnetGetFpgaVersion` | 기본 관리/상태/파라미터 기능. |
| 6. 로그 정보 남기기 | 2 | `eSnetSetUserLogSection`, `eSnetGetUserLogSection` | 기본 관리/상태/파라미터 기능. |
| 7. 이더넷 통신 환경 설정하기 | 5 | `eSnetSetCommunicationConfig`, `eSnetGetCommunicationConfig`, `eSnetSetCommunicationAutoPortChange`, `eSnetGetCommunicationAutoPortChange`, `eSnetCheckCommunicationTime` | 기본 관리/상태/파라미터 기능. |
| 8. 축 파라미터 설정 | 61 | `eSnetSetParam002ControlSignal`, `eSnetGetParam002ControlSignal`, `eSnetSetParam003PulseFormat`, `eSnetGetParam003PulseFormat`, `eSnetSetParam010CommandRotation`, `eSnetSetParam011DistanceRotation`, `eSnetGetParam011DistanceRotation`, `eSnetSetParam012FeedbackCommand`, ... 외 53개 | 기본 관리/상태/파라미터 기능. |
| 9. 축 알람 상태 해제 및 이송 정지 | 11 | `eSnetReset`, `eSnetResetAll`, `eSnetEmergencyStop`, `eSnetEmergencyStopAll`, `eSnetSlowStop`, `eSnetSlowStopAll`, `eSnetSetIoStop`, `eSnetGetIoStop`, ... 외 3개 | 기본 관리/상태/파라미터 기능. |
| 10. 축 상태 정보 확인 | 5 | `eSnetGetAxisStatus`, `eSnetGetAxisSignalStatus`, `eSnetGetMotionDone`, `eSnetGetErrorCode`, `eSnetGetPositionStatus` | 기본 관리/상태/파라미터 기능. |
| 11. 서보 ON/OFF | 2 | `eSnetSetServoOn`, `eSnetGetServoOn` | 기본 축 제어 기능. |
| 12. 원점 복귀 (STEP) | 8 | `eSnetAddHomingStep`, `eSnetGetHomingStep`, `eSnetSetHomingShift`, `eSnetStartHoming`, `eSnetStopHoming`, `eSnetGetHomingRate`, `eSnetGetHomingResult`, `eSnetGetIsHomingDone` | 기본 축 제어 기능. |
| 13. 원점 복귀 (Method) | 5 | `eSnetSetHomingMethod`, `eSnetGetHomingMethod`, `eSnetSetHomingMethodSpeed`, `eSnetGetHomingMethodSpeed`, `eSnetStartHomingMethod` | 기본 축 제어 기능. |
| 14. 축 지령 좌표(절대 좌표-Command position) 확인 및 변경 | 3 | `eSnetSetCommandPosition`, `eSnetGetCommandPosition`, `eSnetGetTargetPosition` | 기본 관리/상태/파라미터 기능. |
| 15. 축 현재 좌표(기계 좌표-Actual position) 확인 및 변경 | 2 | `eSnetSetActualPosition`, `eSnetGetActualPosition` | 기본 관리/상태/파라미터 기능. |
| 16. 축 지령 좌표(Command position) & 현재 좌표(Actual position) 변경 | 1 | `eSnetSetHomePosition` | 기본 관리/상태/파라미터 기능. |
| 17. 축 지령 좌표(Command position) & 현재 좌표(Actual position) 확인 | 1 | `eSnetGetPosition` | 기본 관리/상태/파라미터 기능. |
| 18. 축 상대/절대 좌표 모드 변경 | 2 | `eSnetSetAbsRelMode`, `eSnetGetAbsRelMode` | 기본 관리/상태/파라미터 기능. |
| 19. 단축 이송 | 9 | `eSnetMove`, `eSnetMoveSingle`, `eSnetMoveSingleEx`, `eSnetMoveSingleExJog`, `eSnetMoveSingleExIo`, `eSnetMoveMultiAxis`, `eSnetMoveVelocityAd`, `eSnetMovePositionAd`, ... 외 1개 | 기본 축 제어 기능. |
| 20. 보간 이송 | 7 | `eSnetMoveLine`, `eSnetMoveLineMultiAxis`, `eSnetMoveArcRadius`, `eSnetMoveArcAngle`, `eSnetMoveArcPoint`, `eSnetMoveHelical`, `eSnetMoveSpline` | 컨트롤러 보간 실행. Group object는 아니지만 다축 경로 구현 가능. |
| 21. 연속 보간 구동 | 12 | `eSnetSetContiCh`, `eSnetGetContiCh`, `eSnetBeginContiMakeJob`, `eSnetEndContiMakeJob`, `eSnetGetContiJobIndexCount`, `eSnetStartConti`, `eSnetGetContiInfoResult`, `eSnetSetContiOutputConfig`, ... 외 4개 | 컨트롤러 보간 실행. Group object는 아니지만 다축 경로 구현 가능. |
| 22. 위치/속도 Override | 6 | `eSnetOverrideVelocity`, `eSnetOverrideVelocityEx`, `eSnetOverrideInterpolationVelocity`, `eSnetOverrideVelocityAtMultiPosition`, `eSnetOverrideAccelVelocityDecelAtPosition`, `eSnetOverridePosition` | 기본 관리/상태/파라미터 기능. |
| 23. Rollover (좌표 재설정) | 2 | `eSnetSetRollover`, `eSnetGetRollover` | 기본 관리/상태/파라미터 기능. |
| 24. 겐트리 동기 구동 | 2 | `eSnetEnableGantrySync`, `eSnetIsGantrySync` | SNET 전용 명시 기능. |
| 25. 겐트리 원점 검색 | 2 | `eSnetIsSetGantrySyncHoming`, `eSnetGetGantrySyncHoming` | 기본 축 제어 기능. |
| 26. Interrupt Event | 9 | `eSnetGetInterruptEventTable`, `eSnetEraseInterruptEventTable`, `eSnetClearInterruptEventTable`, `eSnetSetInterruptEventFunction`, `eSnetEnableInterruptEvent`, `eSnetIsInterruptEvent`, `eSnetWaitInterruptEvent`, `eSnetReleaseWaitingInterruptEvent`, ... 외 1개 | 기본 관리/상태/파라미터 기능. |
| 27. 입/출력 제어 (공통) | 1 | `eSnetGetMcbUserInput` | 장비 I/O 제어 기능이 세분화되어 있음. |
| 28. 입/출력 제어 (SNET-P) | 9 | `eSnetPulseGetUserInput`, `eSnetPulseGetUserInputPortAll`, `eSnetPulseGetUserInputPort`, `eSnetPulseGetUserInputPoint`, `eSnetPulseGetUserOutput`, `eSnetPulseGetUserOutputPortAll`, `eSnetPulseGetUserOutputPort`, `eSnetPulseGetUserOutputPoint`, ... 외 1개 | 장비 I/O 제어 기능이 세분화되어 있음. |
| 29. 입/출력 제어 (SNET-P-AD) | 8 | `eSnetPulseExGetIoInput`, `eSnetPulseExGetIoInputPortAll`, `eSnetPulseExGetIoInputPoint`, `eSnetPulseExGetIoOutput`, `eSnetPulseExGetIoOutputPortAll`, `eSnetPulseExGetIoOutputPort`, `eSnetPulseExGetIoOutputPoint`, `eSnetPulseExSetIoOutputPoint` | 장비 I/O 제어 기능이 세분화되어 있음. |
| 30. 입/출력 제어 (SNET-RTEX- Option) | 10 | `eSnetRtexGetIoInput`, `eSnetRtexGetIoInputPortAll`, `eSnetRtexGetIoInputPort`, `eSnetRtexGetIoInputPoint`, `eSnetRtexGetIoOutput`, `eSnetRtexGetIoOutputPortAll`, `eSnetRtexGetIoOutputPort`, `eSnetRtexGetIoOutputPoint`, ... 외 2개 | 장비 I/O 제어 기능이 세분화되어 있음. |
| 31. 입/출력 제어 (SNET-RTEX-IO Slave) | 5 | `eSnetRtexGetIoSlaveInputPort`, `eSnetRtexGetIoSlaveInputPoint`, `eSnetRtexSetIoSlaveOutputPort`, `eSnetRtexSetIoSlaveOutputPoint`, `eSnetRtexGetIoSlaveOutputPoint` | 장비 I/O 제어 기능이 세분화되어 있음. |
| 32. 입/출력 제어 (Remote IO) | 7 | `eSnetSetRemoteIoConfig`, `eSnetGetRemoteIoConfig`, `eSnetGetRemoteIoPortInOut`, `eSnetGetRemoteIoInputPoint`, `eSnetGetRemoteIoOutputPoint`, `eSnetSetRemoteIoOutputPort`, `eSnetSetRemoteIoOutputPoint` | 장비 I/O 제어 기능이 세분화되어 있음. |
| 33. 입/출력 제어 (SNET-ECAT-IO Node) | 5 | `eSnetEcatGetIoNodeInputPortEx`, `eSnetEcatGetIoNodeInputPointEx`, `eSnetEcatSetIoNodeOutputPointEx`, `eSnetEcatGetIoNodeOutputPortEx`, `eSnetEcatGetIoNodeOutputPointEx` | SNET-ECAT 노드/CoE/ESC 제어 영역. |
| 34. Trigger 출력 (SNET-P / EMD) ☞ 일정 거리 간격으로 트리거 신호 출력 | 6 | `eSnetSetTriggerPort`, `eSnetGetTriggerPort`, `eSnetSetTriggerParameter`, `eSnetGetTriggerParameter`, `eSnetGetTriggerStatus`, `eSnetStartTrigger` | 위치/거리 기반 출력 기능. SNET의 강점 영역. |
| 35. Trigger 출력 (SNET-RTEX) ☞ 일정 거리 간격으로 트리거 신호 출력 -1 | 7 | `eSnetSetTriggerPort`, `eSnetGetTriggerPort`, `eSnetRtexSetTriggerSource`, `eSnetSetTriggerParameter`, `eSnetGetTriggerParameter`, `eSnetGetTriggerStatus`, `eSnetStartTrigger` | 위치/거리 기반 출력 기능. SNET의 강점 영역. |
| 36. Trigger 출력 (SNET-RTEX) ☞ 일정 거리 간격으로 트리거 신호 출력 -2 | 6 | `eSnetRtexSetTriggerPort`, `eSnetRtexGetTriggerPort`, `eSnetRtexSetTriggerParameter`, `eSnetRtexGetTriggerParameter`, `eSnetRtexGetTriggerStatus`, `eSnetRtexStartTrigger` | 위치/거리 기반 출력 기능. SNET의 강점 영역. |
| 37. Trigger 출력 (SNET-ECAT) ☞ 일정 거리 간격으로 트리거 신호 출력 | 8 | `eSnetSetCyclicTriggerPort`, `eSnetGetCyclicTriggerPort`, `eSnetSetCyclicTriggerPortEx`, `eSnetGetCyclicTriggerPortEx`, `eSnetSetCyclicTriggerParameter`, `eSnetGetCyclicTriggerParameter`, `eSnetGetCyclicTriggerStatus`, `eSnetStartCyclicTrigger` | 위치/거리 기반 출력 기능. SNET의 강점 영역. |
| 38. Trigger 출력 (SNET-P / RTEX / EMD) ☞ 특정 위치에서 트리거 신호 출력 | 4 | `eSnetSetTriggerTimeLevel`, `eSnetGetTriggerTimeLevel`, `eSnetSetTriggerOnlyAbs`, `eSnetResetTrigger` | 위치/거리 기반 출력 기능. SNET의 강점 영역. |
| 39. Trigger 출력 (SNET-ECAT) - 특정 위치 트리거 | 6 | `eSnetSetTriggerTimeLevelEx`, `eSnetGetTriggerTimeLevelEx`, `eSnetSetTriggerTimeLevelNode`, `eSnetGetTriggerTimeLevelNode`, `eSnetSetTriggerOnlyAbs`, `eSnetResetTrigger` | 위치/거리 기반 출력 기능. SNET의 강점 영역. |
| 40. Trigger 출력 (SNET-RTEX-2LAN) ☞ 특정 위치에서 트리거 신호 출력 | 4 | `eSnetSetTriggerTimeLevelEx`, `eSnetGetTriggerTimeLevelEx`, `eSnetSetTriggerOnlyAbsEx`, `eSnetGetTriggerOnlyAbsEx` | 위치/거리 기반 출력 기능. SNET의 강점 영역. |
| 41. Trigger 출력 ☞ 보간 이송 중 트리거 신호 출력 | 4 | `eSnetSetInterpolTriggerConfig`, `eSnetGetInterpolTriggerConfig`, `eSnetStartInterpolTrigger`, `eSnetStopInterpolTrigger` | 위치/거리 기반 출력 기능. SNET의 강점 영역. |
| 42. 설정 시간 동안 접점 출력 | 3 | `eSnetSetOutputForTime`, `eSnetGetOutputForTimeRun`, `eSnetResetOutputForTime` | 기본 관리/상태/파라미터 기능. |
| 43. Latch 입력 | 4 | `eSnetSetLatchInput`, `eSnetGetLatchInConfig`, `eSnetGetLatchInput`, `eSnetResetLatchInput` | 외부 신호 기반 위치 계측 기능. SNET의 강점 영역. |
| 44. Position Capture 1 ( SNET-P / RTEX ) | 5 | `eSnetSetCaptureConfig`, `eSnetGetCaptureConfig`, `eSnetStartCapture`, `eSnetGetCaptureStatus`, `eSnetGetCapturePosition` | 외부 신호 기반 위치 계측 기능. SNET의 강점 영역. |
| 45. Position Capture 2 ( SNET-RTEX ) | 3 | `eSnetRtexStartCapture`, `eSnetRtexGetCaptureStatus`, `eSnetRtexGetCapturePosition` | 외부 신호 기반 위치 계측 기능. SNET의 강점 영역. |
| 46. Position Capture 3 ( SNET-ECAT ) | 5 | `eSnetSetFieldbusCapture`, `eSnetGetFieldbusCapture`, `eSnetStartFieldbusCapture`, `eSnetGetFieldbusCaptureStatus`, `eSnetGetFieldbusCapturePosition` | 외부 신호 기반 위치 계측 기능. SNET의 강점 영역. |
| 47. MPG 이송 | 2 | `eSnetRtexSetMpgConfig`, `eSnetRtexGetMpgConfig` | 기본 관리/상태/파라미터 기능. |
| 48. 기구 피치 오차 보정 테이블 | 5 | `eSnetGetPositionCompensation`, `eSnetGetPositionCompensationEnable`, `eSnetGetPositionCompensationResult`, `eSnetGetCommandPositionCompensation`, `eSnetGetActualPositionCompensation` | 기본 관리/상태/파라미터 기능. |
| 49. 사용자 지정 기계좌표에서 접점 출력 | 3 | `eSnetSetOutAtActualPosition`, `eSnetGetOutAtActualPosition`, `eSnetResetOutAtActualPosition` | 기본 관리/상태/파라미터 기능. |
| 50. ADC(Analog-digital converter) / DAC(Digital-analog converter) | 6 | `eSnetSetAdcConfig`, `eSnetGetAdcConfig`, `eSnetGetAdcData`, `eSnetSetDacConfig`, `eSnetGetDacConfig`, `eSnetSetDacDigit` | 장비 I/O 제어 기능이 세분화되어 있음. |
| 51. 이더켓 (SNET-ECAT) | 9 | `eSnetEcatGetCoePdo`, `eSnetEcatSetCoePdo`, `eSnetEcatGetCoeSdo`, `eSnetEcatSetCoeSdo`, `eSnetEcatGetEscRegister`, `eSnetEcatSetEscRegister`, `eSnetEcatGetStateMachine`, `eSnetEcatGetBrake`, ... 외 1개 | SNET-ECAT 노드/CoE/ESC 제어 영역. |

## 성능 관점

- 정량 latency/throughput benchmark는 이 PDF에 공통 조건으로 제시되어 있지 않다.
- `eSnetCheckCommunicationTime`이 제공되므로 PC-제어기 응답 시간 자체는 API 레벨에서 측정할 수 있다.
- 고속 위치 동기 동작은 PC polling보다 controller/fieldbus trigger/capture API를 쓰는 구조가 성능상 유리하다.
- 다축 경로는 `eSnetMoveLine*`, `eSnetMoveArc*`, `eSnetMoveSpline`, `eSnetBeginContiMakeJob`/`eSnetStartConti` 같은 컨트롤러 실행형 API를 우선 고려해야 한다.

## Maestro 대비 SNET에 강하게 보이는 기능

- 거리/위치/보간 중 Trigger 출력: Chapter 34-41
- Latch 및 Position Capture: Chapter 43-46
- SNET-P/RTEX/Remote/SNET-ECAT IO, ADC/DAC: Chapter 27-33, 50
- 명시적 Gantry sync/homing: Chapter 24-25
- SNET-ECAT CoE/ESC/state/brake 함수: Chapter 51

## Maestro 대비 부족하거나 직접 대응이 없는 기능

- `MMC_GroupReadStatus` 같은 group object status API
- `MMC_SetKinTransform*` 같은 기구학/좌표계 transform API
- Group Motion의 `BufferMode`, `TransitionMode`, transition parameter 중심 블렌딩
- PVT table motion, Electronic CAM/Gear 기능
- Process Image, PI Bulk Read, Bulk Parameter Read 계층
- FoE download/resource import/export 등 Maestro administrative 기능

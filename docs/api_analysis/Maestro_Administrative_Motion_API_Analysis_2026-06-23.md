# Maestro Administrative and Motion API 분석 (2026-06-23)

## 분석 대상

- 원본: `C:/work/Elmo/Elmo_Master/Maestro Administrative and Motion API_2022_12_v2.012.pdf`
- PDF 정보: 2435 pages, title `Maestro Administrative and Motion`, API version 2.012, 문서 release 2022-12
- 추출 기준: PDF 목차/섹션명에서 `MMC_*`, `MC_*`, `Eip*` 계열 기능을 추출했다. 원문 전체의 unique `MMC_*` 심볼은 2501개지만 구조체/상수/래퍼/예제 참조가 섞여 있으므로 기능 비교는 목차 섹션 단위로 판단했다.

## 결론

Maestro API는 단순 축 제어 라이브러리가 아니라 motion controller 전체를 관리하는 API다. Single Axis, Multi-Axis/Group Motion, kinematic transform, PVT, ECAM/Gear, Process Image, Data Recording, Bulk Read, Event/Callback, EtherCAT/CANbus/DS401/EtherNetIP/Modbus, firmware/resource 관리까지 포함한다.

현재 `Codex_PMAS_WPF`와 `Codex_LASAL_WPF`에서 다루는 `MMC_MoveLinearAbsoluteCmd`, `MMC_GroupReadStatusCmd`, transition mode, group InPosition 판정은 이 문서의 Chapter 7 Multi-Axis/Group Motion 영역에 직접 해당한다. SNET 문서의 보간 이송과 유사한 축 이동은 가능하지만, group status/transition/blending의 원래 모델은 Maestro 쪽이 기준이다.

## API 영역별 정리

| 장 | API/섹션 수 | 대표 기능 | 판단 |
|---|---:|---|---|
| 4. Error Handling | 3 | `MMC_RegErrPolicy`, `MMC_GetErrPolicy`, `MMC_ResetSystem` | 관리/진단 보조 기능. |
| 6. Motion/Admin - Single Axis | 52 | `MMC_Halt`, `MMC_Home`, `MMC_HomeDS402`, `MMC_HomeDS402Ex`, `MMC_MoveAbsolute`, `MMC_MoveAdditive`, `MMC_MoveRelative`, `MMC_MoveVelocity/MMC_MoveVelocityEx`, `MMC_MoveTorque`, `MMC_MoveContinuous`, ... 외 42개 | 단축 모션/상태/파라미터. PLCopen/DS402 축 제어가 강함. |
| 7. Motion/Admin - Multi-Axis | 49 | `MMC_SetNormalcyMode`, `MMC_SetNormalcyOff`, `MMC_GetNormalcyMode`, `MMC_GroupStop`, `MMC_GroupHalt`, `MMC_MoveCircularAbsolute`, `MMC_MoveCircularAbsoluteCenter`, `MMC_MoveCircularAbsoluteBorder`, `MMC_MoveCircularAbsoluteRadius`, `MMC_MoveCircularAbsoluteAngle`, ... 외 39개 | Group Motion, path, kinematics. 다축 테스트 핵심 영역. |
| 8. Position, Velocity, Time (PVT) Motion | 13 | `MMC_TABLE_LIST_OUT`, `MMC_TABLE_LIST_IN`, `MMC_TABLE_DATA_OUT`, `MMC_TABLE_DATA_IN`, `MMC_GetTableList`, `MMC_GetTableInfo`, `MMC_InitTable`, `MMC_InitTableEx`, `MMC_LoadTableFromFile`, `MMC_UnloadTable`, ... 외 3개 | PVT table motion. 시간 기반 고밀도 궤적용. |
| 9. Electronic CAM | 12 | `MMC_CamTableInit`, `MMC_CamTableSelect`, `MMC_CamTableUnload`, `MMC_CamTableAdd`, `MMC_CamTableAddEx`, `MMC_CamIn`, `MMC_CamOut`, `MMC_CamStatus`, `MMC_CamSetProperty`, `MMC_GearIn`, ... 외 2개 | ECAM/Gear. master-slave 동기 응용용. |
| 10. API Services and Operations | 55 | `MMC_ChangeToPreOPMode`, `MMC_ChangeToOperationMode`, `MMC_ClearNodeFbList`, `MMC_CmdStatus`, `MMC_CloseConnection`, `MMC_Config`, `MMC_CreateSYNCTimer`, `MMC_DestroySYNCTimer`, `MMC_DownloadFoE`, `MMC_Exit`, ... 외 45개 | 연결, 리소스, FoE, 조건 대기, 메모리/파라미터 관리. |
| 11. Process Image (PI) | 33 | `MMC_BeginRecordingEx`, `MMC_ReadPIVarBOOL`, `MMC_ReadPIVarChar`, `MMC_ReadPIVarUChar`, `MMC_ReadPIVarShort`, `MMC_ReadPIVarUShort`, `MMC_ReadPIVarInt`, `MMC_ReadPIVarUInt`, `MMC_ReadPIVarFloat`, `MMC_ReadPIVarRaw`, ... 외 23개 | Process Image read/write와 PI bulk. 대량 상태 수집에 유리. |
| 12. Data Recording | 5 | `MMC_BeginRecording`, `MMC_StopRecording`, `MMC_UploadData`, `MMC_RecStatus`, `MMC_UploadDataHeader` | Recorder. 장시간 진단/파형 수집용. |
| 13. Bulk Parameters Reading | 2 | `MMC_ConfigBulkRead`, `MMC_PerformBulkRead` | Bulk parameter read. 통신 왕복 최소화에 유리. |
| 14. API Events (C/C++) | 6 | `MMC_InsertNotificationFb`, `MMC_ClearEventsMask`, `MMC_DisableMotionEndedEvent`, `MMC_EnableMotionEndedEvent`, `MMC_GetEventsMask`, `MMC_SetEventsMask` | 이벤트/callback. 비동기 진단과 motion ended 처리. |
| 15. Error Correction Mechanism | 5 | `MMC_LoadErrorCorrTable`, `MMC_EnableErrorCorrTable`, `MMC_GetErrorTableStatus`, `MMC_DisableErrorCorrTable`, `MMC_UnloadErrorCorrTable` | 관리/진단 보조 기능. |
| 17. Network Connectivity and Configuration | 13 | `MMC_CloseUdpChannel`, `MMC_GetDefGateway`, `MMC_GetDhcp`, `MMC_GetIpMask`, `MMC_GetServerIp`, `MMC_NetworkInfo`, `MMC_NetworkScan`, `MMC_OpenUdpChannel`, `MMC_SetDefGateway`, `MMC_SetDhcp`, ... 외 3개 | 관리/진단 보조 기능. |
| 18. Host Communication / Modbus | 8 | `MMC_MbusIsRunning`, `MMC_MbusReadCoilsTable`, `MMC_MbusReadHoldingRegisterTable`, `MMC_MbusReadInputsTable`, `MMC_MbusStartServer`, `MMC_MbusStopServer`, `MMC_MbusWriteCoilsTable`, `MMC_MbusWriteHoldingRegisterTable` | 관리/진단 보조 기능. |
| 19. CANbus Drive Communication | 31 | `MMC_CancelVirtualEncoder`, `MMC_CancelParamEvPDO3`, `MMC_CancelParamEvPDO4`, `MMC_CfgRegParamEvPDO3`, `MMC_CfgRegParamEvPDO4`, `MMC_CfgUserParamEvPDO3`, `MMC_CfgUserParamEvPDO4`, `MMC_ChangeDefaultPDOConfiguration`, `MMC_ConfigEventModePDO3`, `MMC_ConfigEventModePDO4`, ... 외 21개 | 드라이브/필드버스 통신과 진단 영역. |
| 20. DS-401 CANbus I/O Communication | 14 | `MMC_CancelGeneralRPDO3`, `MMC_CancelGeneralRPDO4`, `MMC_CancelGeneralTPDO3`, `MMC_CancelGeneralTPDO4`, `MMC_ConfigGeneralRPDO3`, `MMC_ConfigGeneralRPDO4`, `MMC_ConfigGeneralTPDO3`, `MMC_ConfigGeneralTPDO4`, `MMC_DisableDS401DIChangedEvent`, `MMC_EnableDS401DIChangedEvent`, ... 외 4개 | 드라이브/필드버스 통신과 진단 영역. |
| 21. EtherCAT Drive Communication | 15 | `MMC_DisableEthercatConfigMode`, `MMC_EnableEthercatConfigMode`, `MMC_ECATIODisableDIChangedEvent`, `MMC_ECATIOEnableDIChangedEvent`, `MMC_ECATIOReadDigitalInput`, `MMC_ECATIOReadAnalogInput`, `MMC_ECATIOWriteAnalogOutput`, `MMC_ECATIOWriteDigitalOutput`, `MMC_GetCommStatistics`, `MMC_GetEthercatCommStatistics`, ... 외 5개 | 드라이브/필드버스 통신과 진단 영역. |
| 22. Interpreter Command Functions | 8 | `MMC_ElmoExecuteLabel`, `MMC_ElmoSetParameter`, `MMC_ElmoGetParameter`, `MMC_ElmoGetParameterAndRetrieveData`, `MMC_ElmoQueryOperationFIFOIndex`, `MMC_ElmoQueryOperationFIFORetrieveData`, `MMC_ElmoQueryOperationFIFOIndexReset`, `MMC_ElmoCall` | 드라이브/필드버스 통신과 진단 영역. |
| 23. EtherNet/IP Communication | 15 | `EipGetAdpTagRefByName`, `EipWriteAdpTag`, `EipReadAdpTag`, `EipGetAssemblyRefByInstance`, `EipGetAssemblyRefByName`, `EipSetAssembly`, `EipGetDevTagRefByName`, `EipSetDevTag`, `EipGetDevTag`, `EipReadDevTagData`, ... 외 5개 | 드라이브/필드버스 통신과 진단 영역. |
| 24. Programming in C++ wrapper | 4 | `MMC_KillRepetitive`, `MMC_MOTIONPARAMS_GROUP()`, `MMC_RpcInitConnectionEx`, `MMC_RpcInitConnectionEx` | 언어/IEC 래퍼 또는 특수 함수. 기능 중복 포함. |
| 25. IEC 61131-3 Special Functions | 1 | `MMC_SetImmediateExec` | 언어/IEC 래퍼 또는 특수 함수. 기능 중복 포함. |

## 성능 관점

- 정량 latency/throughput benchmark는 이 PDF만으로 비교할 수 없다. 문서가 제공하는 것은 기능 구조와 호출 모델이다.
- Group Motion은 PC가 각 축을 순차 polling/명령하는 구조보다 controller 내부의 group/path/transition 계산을 쓰는 구조다. 다축 동기, kinematics, blending 테스트에는 이 구조가 유리하다.
- `Process Image`, `PI Bulk Read`, `Bulk Parameters Reading`은 여러 변수를 한 번에 수집하는 구조라 반복 개별 호출보다 통신 왕복을 줄일 수 있다.
- `Data Recording`과 `API Events`는 PC polling 대신 controller/event 기반 진단을 구성할 수 있게 한다.
- EtherCAT/CANbus/DS401/Interpreter/EtherNetIP 관련 API가 넓어 장비 통신 진단과 드라이브 직접 명령 자동화에 유리하다.

## SNET 대비 Maestro에 강하게 보이는 기능

- Group object, group status, group parameter, group position/velocity/error read
- `MMC_SetKinTransform*`, Cartesian/SCARA/ThreeLink/Hxpd, conveyor/rotary tracking
- Buffer/transition/blending을 포함한 Group Motion과 path motion
- PVT table motion, Electronic CAM, Gear
- Process Image, PI Bulk Read, Bulk Parameter Read
- Data Recording, API Events/callback, error policy
- FoE download, resource import/export, version path, system reset 등 administrative 기능
- EtherCAT/CANbus/DS401/EtherNetIP/Modbus 등 통신 기능 폭

## SNET 대비 부족하거나 직접 대응이 약한 기능

- SNET-P/RTEX/SNET-ECAT 별 거리/위치 trigger 전용 API 묶음
- RTEX/ECAT fieldbus position capture, latch, 사용자 기계좌표 output처럼 문서상 SNET에 특화된 계측 API
- SNET-P-AD/RTEX Option/Remote IO 같은 SNET 보드/노드 단위 전용 IO API
- SNET 문서의 명시적 gantry sync/homing 함수명과 동일한 직접 대응 API

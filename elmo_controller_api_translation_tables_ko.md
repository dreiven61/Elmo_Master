# ELMO Controller API 번역 표

- 원본 파일: `C:\work\SIGMATEK\Lasal32dll\P-MAS\EtherCAT Controller(Master) 요구 사양.xlsx`
- 시트: `ELMO Controller API`
- 기준일: `2026-04-02`

| No | Elmo API Function | 원문 Define | 한국어 번역 |
|---|---|---|---|
| 1 | MMC_RpcInitConnection | Initiates RPC connection to Maestro server. | Maestro 서버와의 RPC 연결을 초기화한다. |
| 2 | MMC_OpenUdpChannelCmdEx | - | 설명 없음(원문 빈칸). |
| 3 | MMC_GetAxisByNameCmd | Returns an axis index reference by its name. | 축 이름으로 축 인덱스 참조를 반환한다. |
| 4 | MMC_GetGroupByNameCmd | This function returns a group index reference by its name. | 그룹 이름으로 그룹 인덱스 참조를 반환한다. |
| 5 | MMC_GetErrorCodeDescriptionByID | This function receives an error\warning code and returns the description and resolution from the Personality file. | 오류/경고 코드를 입력받아 Personality 파일에서 설명과 해결 방법을 반환한다. |
| 6 | MMC_PowerCmd | Controls the power stage (On or Off). | 전력 스테이지를 제어한다(켜기/끄기). |
| 7 | MMC_GroupReadStatusCmd | For multiple Axes. Returns the status of an axes group according to the active Group function block. This is an administrative function block, since no movement is generated. | 다축 시스템에서 활성 Group 함수 블록 기준으로 축 그룹의 상태를 반환한다. 이동을 발생시키지 않는 관리용 함수 블록이다. |
| 8 | MMC_GroupEnableCmd | For multi-axis systems. Changes the state for a group from GroupDisabled to GroupStandby. This is an administrative function block, since no movement is generated. | 다축 시스템에서 그룹 상태를 GroupDisabled에서 GroupStandby로 변경한다. 이동을 발생시키지 않는 관리용 함수 블록이다. |
| 9 | MMC_GroupDisableCmd | For multiple Axes. Changes the state for a group to GroupDisabled, although it is an administrative function block, since no movement is generated. | 다축 시스템에서 그룹 상태를 GroupDisabled로 변경한다. 이동을 발생시키지 않는 관리용 함수 블록이다. |
| 10 | MMC_ConfigBulkReadCmd | Configures the function to read all parameters from multiple axes. | 다수 축의 모든 파라미터를 읽기 위한 Bulk Read 기능을 설정한다. |
| 11 | MMC_PerformBulkReadCmd | Reads those parameters which were configured by a call to ConfigBulkRead, from multiple axes. | ConfigBulkRead에서 설정한 파라미터들을 다수 축에서 읽어온다. |
| 12 | MMC_Reset | Provides a method to perform transition from the state ErrorStop to StandStill or Disabled by resetting all internal axis-related errors, and returns immediately. | 내부 축 관련 오류를 모두 리셋해 상태를 ErrorStop에서 StandStill 또는 Disabled로 전이시키며, 즉시 반환한다. |
| 13 | MMC_GroupResetCmd | For multiple Axes. Makes the transition from the state GroupErrorStop to GroupDisabled by resetting all internal group-related errors - it does not affect the output of the function block instances. | 다축 시스템에서 내부 그룹 관련 오류를 리셋해 상태를 GroupErrorStop에서 GroupDisabled로 전이시킨다. 함수 블록 인스턴스의 출력에는 영향을 주지 않는다. |
| 14 | MMC_SendSdoCmd | Sends SDO message command, in units of 1, 2, or 4 bytes. | 1/2/4바이트 단위의 SDO 메시지 명령을 전송한다. |
| 15 | MMC_ReadParameter | Returns the value of a vendor specific parameter. | 벤더 고유 파라미터 값을 반환한다. |
| 16 | MMC_MoveRelativeExCmd | - | 설명 없음(원문 빈칸). |
| 17 | MMC_ReadBoolParameter | Returns the value of a vendor specific with datatype unsigned long or un signed int. | 데이터 타입이 unsigned long 또는 unsigned int인 벤더 고유 파라미터 값을 반환한다. |
| 18 | MMC_ChngOpMode | Changes the motion mode between NC and Distributed. This is previous determined in the DS-402 mode. | 모션 모드를 NC와 Distributed 사이에서 변경한다. 이는 DS-402 모드에서 사전 결정된다. |
| 19 | MMC_SetPositionCmd | Sends the Set Position command to the Maestro for ac specific axis. | 특정 축에 대해 Maestro로 Set Position 명령을 전송한다. |
| 20 | MMC_HomeDS402ExCmd | Commands the axis to perform the Search Home DS402 sequence for a specific Axis, and can be set by the axes parameters. This function supports Velocity Hi\Lo, DetectionTimeLimit and DetectionVelocityLimit. | 특정 축에 대해 Search Home DS402 시퀀스를 수행하도록 명령하며, 축 파라미터로 설정할 수 있다. Velocity Hi/Lo, DetectionTimeLimit, DetectionVelocityLimit을 지원한다. |
| 21 | MMC_GetPIVarInfoByAlias | This function returns the detailed number of mapped Processing Image variables, reading the variable alias as a key. | 변수 alias를 키로 사용해 매핑된 Processing Image 변수의 상세 개수를 반환한다. |
| 22 | MMC_WritePIVarUShort | This function writes a Processing Image input\output Unsigned Short variable according to its index | 인덱스에 따라 Processing Image 입출력 Unsigned Short 변수를 쓴다. |
| 23 | MMC_ReadPIVarUShort | This function reads a Processing Image input\output Unsigned Short Variable according to its index | 인덱스에 따라 Processing Image 입출력 Unsigned Short 변수를 읽는다. |
| 24 | MMC_WriteParameter | Modifies the value of a vendor specific parameter. | 벤더 고유 파라미터 값을 변경한다. |
| 25 | MMC_SetKinTransform | Sets a kinematic transformation between the ACS and MCS based on the predefined kinematic model for multi-axes. Refer to the section 7.1Coordinate System and kinematic transformation for a further detailed explanation. Refer to sections Coordinated System and Kinematic Transformation Definitions onwards for details of the structures used within this function. | 다축용 사전 정의된 기구학 모델을 기반으로 ACS와 MCS 사이의 기구학 변환을 설정한다. 자세한 내용은 7.1 Coordinate System 및 Kinematic Transformation 섹션, 그리고 Coordinated System/Kinematic Transformation Definitions 섹션을 참조한다. |
| 26 | MMC_GroupStopCmd | For multi-axis systems. Brings a group of axes to stop status. | 다축 시스템에서 축 그룹을 정지 상태로 전환한다. |
| 27 | MMC_StopCmd | Commands a controlled motion stop and transfers the axis to the state Stopping. | 제어된 모션 정지를 명령하고 축 상태를 Stopping으로 전환한다. |
| 28 | MMC_CloseConnection | Closes the connection to the Maestro. | Maestro와의 연결을 종료한다. |
| 29 | MMC_MoveLinearAbsoluteCmd | For multi-axis systems. Commands an interpolated linear movement on an axes group from the actual position of the TCP to an absolute position in the specified coordinate system. | 다축 시스템에서 축 그룹의 TCP 현재 위치에서 지정된 좌표계의 절대 위치까지 보간 선형 이동을 명령한다. |
| 30 | MMC_MoveAbsoluteExCmd | - | 설명 없음(원문 빈칸). |
| 31 | MMC_MoveLinearRelativeCmd | For multi-axis systems. Commands an interpolated linear movement on an axes group from the actual position of the TCP to a relative distance in the specified coordinate system. | 다축 시스템에서 축 그룹의 TCP 현재 위치에서 지정 좌표계의 상대 거리만큼 보간 선형 이동을 명령한다. |
| 32 | MMC_MoveVelocityExCmd | - | 설명 없음(원문 빈칸). |
| 33 | MMC_SetOverrideCmd | Sets the values of override for the whole axis, including all functions that are operating on that axis. | 해당 축에서 동작 중인 모든 기능을 포함해 축 전체에 대한 오버라이드 값을 설정한다. |
| 34 | MMC_ReadActualPositionCmd | Returns the actual position of the controlled axis. | 제어 중인 축의 실제 위치를 반환한다. |
| 35 | MMC_GetStatusRegisterCmd | The purpose of the function is to provide usable information regarding the Maestro and axes statuses. | Maestro 및 각 축 상태에 대한 유의미한 정보를 제공한다. |
| 36 | MMC_StopRecordingCmd | Halts recording of the Maestro server data. | Maestro 서버 데이터 기록을 중지한다. |
| 37 | MMC_RecStatusCmd | Requests the status of the recording. | 기록 상태를 요청한다. |
| 38 | MMC_BeginRecordingCmd | Starts the recording of internal controller variables data from the Maestro server. | Maestro 서버의 내부 컨트롤러 변수 데이터 기록을 시작한다. |
| 39 | MMC_UploadDataHeaderCmd | Recorder upload data header. | 레코더 업로드 데이터 헤더를 처리한다. |
| 40 | MMC_UploadDataCmd | Uploads recording data to the Maestro. | 기록 데이터를 Maestro로 업로드한다. |
| 41 | MMC_ReadStatusCmd | Returns details of the state diagram status for the selected axis. | 선택된 축의 상태 다이어그램 상태 상세를 반환한다. |
| 42 | MMC_MoveLinearAbsoluteExCmd | - | 설명 없음(원문 빈칸). |
| 43 | MMC_GetGroupMembersInfo | Returns information about a specific group and its members. | 특정 그룹과 해당 멤버들에 대한 정보를 반환한다. |
| 44 | MMC_WaitUntilConditionFB | The operation of this function block allows synchronization of numerous axes that are not part of a group, to start their motion together. In addition, it allows synchronization of numerous networked Maestro's by starting a motion when a specific bit on a shared IO is raised. | 이 함수 블록은 그룹에 속하지 않은 여러 축의 동작 시작 시점을 맞춰 동시에 모션을 시작하게 한다. 또한 공유 IO의 특정 비트가 올라갈 때 모션을 시작하도록 하여, 네트워크로 연결된 여러 Maestro 간 동기화도 가능하게 한다. |

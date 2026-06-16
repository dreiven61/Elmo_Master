# Maestro Core API Function Overview

Source: Maestro Administrative and Motion API_2022_12_v2.012.pdf

Scope: Chapter 4-23 API functions only. Chapter 24+ C++/Python wrapper and programming content is excluded.

Total functions: 341

| FunctionName | Parameters | Role |
|---|---|---|
| MMC_RegErrPolicy | hConn, pInParam, pOutParam | This function registers and defines an error policy. |
| MMC_GetErrPolicy | hConn, pOutParam | This function return the current error policies statues. |
| MMC_ResetSystem | hConn, pInParam, pOutParam | This function resets the entire system errors, including error counters of PHY, cyclic and missed frames errors. In addition it changes all nodes to INIT and then to OPERATIONAL state, resulting in motor off of all drives. In addition this function might reset Maestro fatal errors and return it to fully operational status. |
| MMC_SetProfileConditioning | hConn, hAxisRef, i_params, o_params | Profile conditioning C functions used to set or get profile conditioning configuration for vibration reduction. |
| MMC_GetProfileConditioning | hConn, hAxisRef, i_params, o_params | Profile conditioning C functions used to set or get profile conditioning configuration for vibration reduction. |
| GetProfileConditioning | hConnHndl, o_params | Profile conditioning C functions used to set or get profile conditioning configuration for vibration reduction. |
| SetProfileConditioning | i_params | Profile conditioning C functions used to set or get profile conditioning configuration for vibration reduction. |
| MMC_Halt | hConn, hAxisRef, pInParam, pOutParam | Call this function to command a controlled motion stop for a specific axis. |
| MMC_Home | hConn, hAxisRef, pInParam, pOutParam | Commands the axis to perform the Search Home sequence. |
| MMC_HomeDS402 | hConn, hAxisRef, pInParam, pOutParam | This function sends a command to perform the Search Home sequence for a specific Axis, and can be set by the axes parameters. |
| MMC_HomeDS402Ex | hConn, hAxisRef, pInParam, pOutParam | Commands the axis to perform the Search Home DS402 sequence for a spec ific Axis, and can be set by the axes parameters. This function supports Velocity Hi\Lo, DetectionTimeLimit and DetectionVelocityLimit. |
| MMC_MoveAbsolute | hConn, hAxisRef, pInParam, pOutParam | Commands a discreet controlled motion for a single axis to a specified absolute position. |
| MMC_MoveAdditive | hConn, hAxisRef, pInParam, pOutParam | Commands a controlled motion of a specified relative distance additional to the most recent commanded position in the discrete motion state. |
| MMC_MoveRelative | hConn, hAxisRef, pInParam, pOutParam | Commands a discreet controlled motion of a specified distance relative to the set position at the time of the execution. |
| MMC_MoveTorque | hConn, hAxisRef, pInParam, pOutParam | Commands a continuous controlled motion at a specified torque. |
| MMC_MoveContinuous | hConn, hAxisRef, pInParam, pOutParam | This function sends a Move Continuous command to MMC server for a specific Axis. |
| MMC_MoveAbsoluteRepetitive | hConn, hAxisRef, pInParam, pOutParam | This function receives as one of the input arguments, the command to move to the absolute target position. The axis moves between the current and target position until interrupted by any allowed function block in Aborted mode. |
| MMC_MoveRelativeRepetitive | hConn, hAxisRef, pInParam, pOutParam | This function receives as one of the input arguments, the command to mo ve to a distance relative to the current position. The axis moves between the current and target position until interrupted by any allowed function block in Aborting mode. |
| MMC_MoveAdditiveRepetitive | hConn, hAxisRef, pInParam, pOutParam | This function receives as one of the input arguments, the command to move to a distance relative to the final position of the last command. The axis moves between the current and target position until interrupted by any allowed function block in Aborting mode. |
| MMC_Stop | hConn, hAxisRef, pInParam, pOutParam | Commands a controlled motion stop and transfers the axis to the state Stopping. |
| MMC_AxisLink | hConn, hAxisRef, pInParam, pOutParam | This function links between physically axis to virtual axis. |
| MMC_AxisUnLink | hConn, hAxisRef, pInParam, pOutParam | This function breaks the link between two axes defined as master (Primary) and slave (Minor). |
| MMC_KillMotion | hConn, hAxisRef, i_param, o_param | This function stops the repetitive motion after the current function block. |
| MMC_KillRepetitive | hConn, hAxisRef, pInParam, pOutParam | This function stops the repetitive motion after the current function block. |
| MMC_Dwell | hConn, hAxisRef, pInParam, pOutParam | This function sends a temporary halt status command to the Maestro. |
| MMC_GetFBDepth | hConn, hAxisRef, pInParam, pOutParam | Sends a command to get the number of function blocks in the Node Queue waiting to be activated, or presently activated. The function blocks included in the count will not have a status of Done or Abort. |
| MMC_MarkFbFree | hConn, pInParam, pOutParam | Marks a function block as free. |
| MMC_GetTotalFbDepth | hConn, hAxisRef, pInParam, pOutParam | Sends a command to receive the total number of function blocks in the Node Queue, wai ting to be activated, presently activated, and previously activated but not released to the function block pool. |
| MMC_Power | hConn, hAxisRef, pInParam, pOutParam | Controls the power stage (On or Off). |
| MMC_PositionProfile | hConn, hAxisRef, pInParam, pOutParam | Describes the position profile of an axis. |
| MMC_ReadActualPosition | hConn, hAxisRef, pInParam, pOutParam | Returns the actual position of the controlled axis. |
| MMC_ReadActualTorque | hConn, hAxisRef, pInParam, pOutParam | Returns the actual torque value or force for a controlled axis, as long as Enable is set. |
| MMC_ReadActualVelocity | hConn, hAxisRef, pInParam, pOutParam | Returns the actual velocity value, as long as Enable is set. |
| MMC_ReadAxisError | hConn, hAxisRef, pInParam, pOutParam | Displays general axis errors not relating to the function blocks e.g. axis errors, drive errors, communication errors. |
| MMC_ReadBoolParameter | hConn, hAxisRef, pInParam, pOutParam | Returns the value of a vendor specific with datatype unsigned long or un signed int. |
| MMC_GlobalReadBoolParameter | hConn, pInParam, pOutParam | Returns the value of a vendor global Boolean parameter with datatype unsigned long or un signed int. |
| MMC_ReadDigitalOutputs | hConn, hAxisRef, pInParam, pOutParam | Reads the actual digital outputs for the specific node. |
| MMC_ReadDigitalOutputs32Bit | hConn, hAxisRef, pInParam, pOutParam | Reads the actual 32 bit digital outputsGet for the specific node |
| MMC_ReadParameter | hConn, hAxisRef, pInParam, pOutParam | Returns the value of a vendor specific parameter. |
| MMC_GlobalReadParameter | hConn, pInParam, pOutParam | Returns the value of a vendor global parameter. |
| MMC_ReadStatus | hConn, hAxisRef, pInParam, pOutParam | Returns details of the state diagram status for the selected axis. |
| MMC_Reset | hConn, hAxisRef, pInParam, pOutParam | Provides a method to perform transition from the state ErrorStop to StandStill or Disabled by resetting all internal axis-related errors, and returns immediately. |
| MMC_ResetAsync | hConn, hAxisRef, pInParam, pOutParam | Produces the transition from the state ErrorStop to StandStill or Disabled by resetting all internal axis - related errors. This function waits until the procedure is fully completed. An event is then sent once the |
| MMC_SetOverride | hConn, hAxisRef, pInParam, pOutParam | Sets the values of override for the whole axis, including all functions that are operating on that axis. |
| MMC_SetPosition | hConn, hAxisRef, pInParam, pOutParam | Sends the Set Position command to the Maestro for ac specific axis. |
| MMC_TouchProbeEnable | hConn, hAxisRef, pInParam, pOutParam | Enables the touch probe to record an axis position at a trigger event. |
| MMC_TouchProbeDisable | hConn, hAxisRef, pInParam, pOutParam | Disables the touch probe to record an axis position at a trigger event. |
| MMC_WriteBoolParameter | hConn, hAxisRef, pInParam, pOutParam | Modifies the value of a vendor specific parameter of type BOOL. |
| MMC_GlobalWriteBoolParameter | hConn, pInParam, pOutParam | Modifies the value of a vendor global parameter of type BOOL. |
| MMC_WriteDigitalOutputs | hConn, hAxisRef, pInParam, pOutParam |  |
| MMC_WriteDigitalOutputs32Bit | hConn, hAxisRef, pInParam, pOutParam | Writes a value to the 32-bit digital output referenced by the single argument Output (with rising edge of Execute). |
| MMC_WriteParameter | hConn, hAxisRef, pInParam, pOutParam | Modifies the value of a vendor specific parameter. |
| MMC_GlobalWriteParameter | hConn, pInParam, pOutParam | Modifies the value of a vendor global parameter. |
| MMC_ChngOpMode | hConn, hAxisRef, pInParam, pOutParam | Changes the motion mode between NC and Distributed. This is previous determined in the DS -402 mode. |
| MMC_ChangeOpModeEx | hConn, hAxisRef, pInParam, pOutParam | Changes the motion mode between NC and Distributed. This is previous determined in the PLC DS -402 mode. |
| MMC_SetProfileConditioning | hConn, hAxisRef, i_params, o_params | This method sets Profile Conditioning mode of operation. It switches on(1)/off(0) the mode and sets other input parameters. |
| MMC_GetProfileConditioning | hConn, hAxisRef, i_params, o_params | This method obtains the Profile Conditioning mode of operation data for axes on which this mode of operation is enabled. |
| MMC_SetNormalcyMode | hConn, hAxisRef, i_params, o_params | For multi-axis systems. Set normalcy mode of operation in specific selected plane (xy/xz/yz) . |
| MMC_SetNormalcyOff | hConn, hAxisRef, o_params | For multi-axis systems. Set normalcy Off disables normalcy mode. |
| MMC_GetNormalcyMode | hConn, hAxisRef, o_params | For multi-axis systems. Set normalcy Off disables normalcy mode. |
| MMC_GroupStop | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Brings a group of axes to stop status. |
| MMC_GroupHalt | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Brings a group of axes to Halt status. |
| MMC_MoveCircularAbsolute | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an interpolated circular movement on an axes group from the actual position of the TCP. |
| MMC_MoveCircularAbsoluteCenter | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an interpolated circular center movement on an a xes group from the actual position of the TCP. |
| MMC_MoveCircularAbsoluteBorder | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an interpolated circular border movement on an axes group from the actual position of the TCP. |
| MMC_MoveCircularAbsoluteRadius | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an interpolated circular movement radius on an axes group from the actual position of the TCP. |
| MMC_MoveCircularAbsoluteAngle | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an interpolated circular angle movement on an axes group from the actual position of the TCP. The movement may be in either positive or negative direction without restriction. |
| MMC_MoveAngle | hConn, hAxisRef, pInParam, pOutParam | Allows a user to specify a certain plane for the arc motion, where the arc motion will only be performed in one of the planes perpendicular to each other in space (XY, XZ, or YZ). |
| MMC_MoveLinearAbsolute | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an interpolated linear movement on an axes group from the actual position of the TCP to an absolute position in the specified coordinate system. |
| MMC_MoveLinearRelative | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an interpolated linear movement on an axes group from the actual position of the TCP to a relative distance in the specified coordinate system. |
| MMC_MoveLinearAdditive | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an interpolated linear movement on an axes group from the actual position of the TCP to an additive position in the specified coordinate system. |
| MMC_MoveLinearAdditiveEx | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an extended interpolated linear movement on an axes group from the actual position of the TCP to an additive accurate position in the specified coordinate system. |
| MMC_MoveLinearAbsoluteRepetitive | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an interpolated repetitive linear movement on an axes group vector to an absolute point given as an input in the specified coordinate system. |
| MMC_MoveLinearRelativeRepetitive | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Commands an interpolated repetitive linear movement on an axes group vector to a relative distance from actual position given as an input in the specified coordinate system. |
| MMC_MovePolynomAbsolute | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems and complex motion sequences where the Polynomial expression is relevant. This function sends a Move Polynom Absolute command to the MMC server for specific Vect or. Refer to the |
| MMC_PathSelect | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Reads splines data from a file and calculates the optimal path. |
| MMC_MovePath | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Moves a group of drives along a previously defined spline path. |
| MMC_PathUnselect | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. This function unloads the spline data table from the Maestro. |
| MMC_SetKinTransform | hConn, hAxisRef, pInParam, pOutParam | Sets a kinematic transformation between the ACS and MCS based on the predefined kinemati c model for multi-axes. Refer to the section 7.1Coordinate System and kinematic transformation for a further detailed explanation. Refer to sections Coordinated System and Kinematic Transformation Definitions onwards for |
| MMC_SetKinTransformEx | hConn, hAxisRef, pInParam, pOutParam | Sets a kinematic transformation between the ACS and MCS based on the predefined kinematic model for group multi-axes. Refer to the section Coordinate System and kinematic transformation for a further detailed explanation. Refer to sections Coordinated System and Kinematic Transformation Definitions |
| MMC_SetCartesianTransform | hConn, hAxisRef, pInParam, pOutParam | Sets a group's cartesian transformation between MCS and PCS parameters based on the predefined kinematic model for group multi-axes. Refer to the section PCS - Product Coordinate System for a further detailed explanation. Refer to sections Coordinated System and Kinematic Transforma tion Definitions onwards for |
| MMC_TrackConveyorBelt | hConn, hAxisRef, pi_params, po_params | The MMC_TrackConveyorBelt function executes a smooth RAMP-IN motion from any robot position (in MCS), on to the Part located on a moving conveyor belt (in PCS). As soon as the robot reaches the target point on the conveyor belt it moves synchronously with the conveyor belt (in PCS), that is to say, it tracks the conveyor belt while performing a motion in PCS. |
| MMC_TrackRotaryTable | hConn, hAxisRef, pInParam, pOutParam | The MMC_TrackRotaryTable function executes a smooth RAMP-IN motion from any robot position (in MCS), on to the Part located on a moving Rotatory Table (in PCS). As soon as the robot reache s the target point on the Rotatory Table it moves synchronously with the Rotatory Table (in PCS), that is to say, it tracks the Rotatory Table while performing motion in PCS. |
| MMC_TrackSyncOut | hConn, hAxisRef, pi_params, po_params | The MMC_TrackSyncOut function executes a smooth RAMP Out motion from a synchronized PCS motion to an MCS target position until halted. |
| MMC_SetKinTransformDelta | hConn, hAxisRef, pInParam, pOutParam | Sets a kinematic transformation between the ACS and MCS based on the predefined kinematic model for group multi-axes with the Delta robot. Refer to the section PCS - Product Coordinate System for a further detailed explanation. Refer to sections Coordinated System and Kinematic Transformation Definitions |
| MMC_SetKinTransformCartesian | hConn, hAxisRef, pInParam, pOutParam | Sets the parameters kinematic transformation (MSC to ACS) for Cartesian system based on the predefined kinematic model for group multi-axes. Refer to the section PCS - Product Coordinate System for a further detailed explanation. Refer to sections Coordinated System and Kinematic Transformation Definitions |
| MMC_SetKinTransformScara | hConn, hAxisRef, pInParam, pOutParam | Sets the parameters kinematic transformation (MSC to ACS) for SCARA robot based on the predefined kinematic model for group multi-axes. Refer to the section PCS - Product Coordinate System for a further detailed explanation. Refer to sections Coordinated System and Kinematic Transformation Definitions |
| MMC_SetKinTransformThreeLink | hConn, hAxisRef, pInParam, pOutParam | Sets the parameters kinematic transformation (MSC to ACS) for THREELINK robot based on the predefined kinematic model for group multi-axes. Refer to the section PCS - Product Coordinate System for a further detailed explanation. Refer to sectionsCoordinated System and Kinematic Transformation Definitions |
| MMC_SetKinTransformHxpd | hConn, hAxisRef, i_param, o_param | Sets the parameters kinematic transformation (MSC to ACS) for THREELINK robot based on the predefined kinematic model for group multi-axes. Refer to the section PCS - Product Coordinate System for a further detailed explanation. Refer to sections Coordinated System and Kinematic Transformation Definitions |
| MMC_GetMotionInfo | hConn, hAxisRef, pInParam, pOutParam | For multiple Axes. Provides information on an array of structures – where each structure returns the following information: • FB index given by user. This is returned internally by new pFbCommon ->dbUserData parameter |
| MMC_AddAxisToGroup | hConn, hAxisRef, pInParam, pOutParam | For multiple Axes. Adds one axis to a group in a structure AxesGroup. |
| MMC_GroupDisable | hConn, hAxisRef, pInParam, pOutParam | For multiple Axes. Changes the state for a group to GroupDisabled, although it is an administrative function block, since no movement is generated. |
| MMC_GroupEnable | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Changes the state for a group from GroupDisabled to GroupStandby. This is an |
| MMC_GroupReadActualPosition | hConn, hAxisRef, pInParam, pOutParam | For multi-axis systems. Returns the actual position in the selected coordinate system of an axes group. This |
| MMC_GroupReadActualVelocity | hConn, hAxisRef, pInParam, pOutParam | Returns the actual velocity in the selected coordinate system of an axes group. This is an administrative function block, since no movement is generated. |
| MMC_GroupReadError | hConn, hAxisRef, pInParam, pOutParam | Describes general axes group errors not relating to the function blocks. This is an admi nistrative function block, since no movement is generated. This function is not in use at this moment. |
| MMC_GroupReadStatus | hConn, hAxisRef, pInParam, pOutParam | For multiple Axes. Returns the status of an axes group according to the active Group function block. This is |
| MMC_GroupReset | hConn, hAxisRef, pInParam, pOutParam | For multiple Axes. Makes the transition from the state GroupErrorStop to GroupDisabled by resetting all internal group-related errors – it does not affect the output of the function block instances. |
| MMC_GroupSetOverride | hConn, hAxisRef, pInParam, pOutParam | For multi-axes. Sets the values of override for the coordinated motion of several axes, and all functions operating on that axes group. |
| MMC_GroupSetPosition | hConn, hAxisRef, pInParam, pOutParam | For multiple Axes. Sets the Position of all axes in a group without moving the axes. |
| MMC_RemoveAxisFromGroup | hConn, hAxisRef, pInParam, pOutParam | For multiple Axes. Removes one axis from the group AxesGroup. This is an administrative function block, |
| MMC_GroupReadParameter | hConn, hAxisRef, pInParam, pOutParam | Reads a specific group of axes parameters. |
| MMC_GroupReadBoolParameter | hConn, hAxisRef, pInParam, pOutParam | Reads a specific group axes Boolean parameter. |
| MMC_GroupWriteParameter | hConn, hAxisRef, pInParam, pOutParam | Modifies the value of a specific group axes parameter. |
| MMC_GroupWriteBoolParameter | hConn, hAxisRef, pInParam, pOutParam | Modifies the value of a specific group axes Boolean parameter. |
| MMC_GetGroupMembersInfo | hConn, hAxisRef, pInParam, pOutParam | Returns information about a specific group and its members. |
| MMC_GetTableList | hConn, pInParam, pOutParam | This function provides a list of tables for a given table type. |
| MMC_GetTableInfo | hConn, pInParam, pOutParam | This function provides the table info (currently name only) for a given table handler. |
| MMC_InitTable | hConn, pInParam, pOutParam | This function allocates a memory segment in the shared memory according to the dimension and number of points. |
| MMC_InitTableEx | hConn, pInParam, pOutParam | This function allocates unlimted memory segments in the shared memory according to the dimension and number of points. |
| MMC_LoadTableFromFile | hConn, pInParam, pOutParam | The function allocates a memory segment in the Maestro shared memory according to the dimension and number of points given in a file. |
| MMC_UnloadTable | hConn, pInParam, pOutParam | The function unloads a table from the Maestro and frees a memory segment in the Maestro shared memory according to the dimension and number of points given in a file. |
| MMC_MoveTable | hConn, hAxisRef, pInParam, pOutParam | This function moves the Table along a selected path. |
| MMC_AppendPointsToTable | hConn, pInParam, pOutParam | This function appends points to an existing table. |
| MMC_GetTableIndex | hConn, pInParam, pOutParam | This function obtains the PVT index. |
| MMC_CamTableInit | hConn, pInParam, pOutParam | This function allocates memory for the ECAM table, prepares and initializes the function block in journal. In general, it is similar to MC_TableInit without an option for dynamic append. |
| MMC_CamTableSelect | hConn, pInParam, pOutParam | This function selects a table by input handler. |
| MMC_CamTableUnload | hConn, pInParam, pOutParam | The function unloads an ECAM table from the Maestro and frees a memory segment in the Maestro shared memory according to the dimension and number of points given in a file. |
| MMC_CamTableAdd | hConn, pInParam, pOutParam | This function appends points to an existing table. |
| MMC_CamTableAddEx | hConn, pInParam, pOutParam | This function allows appending an unlimited number of rows to an existing table present in memory. |
| MC_CamTableSet | hConn, pInParam, pOutParam | When using this method, MMC_CamTableAdd is used for loading a table from memory. |
| MMC_CamIn | hConn, hAxisRef, pInParam, pOutParam | MC_CamIn executes the CAM process. |
| MMC_CamOut | hConn, hAxisRef, pInParam, pOutParam | Performs a MC_Stop on the slave axis to disengage the CAM process. |
| MMC_CamStatus | hConn, hAxisRef, pInParam, pOutParam | MC_CamStatus retrieves the significant parameters of the CAM process. |
| MMC_CamSetProperty | hConn, hAxisRef, pInParam, pOutParam | This function sets specific properties of the CAM function. It was created for a specific situation whereby the ECAM periodic motion is to be stopped using a non-periodic motion. |
| MMC_GearIn | hConn, hAxisRef, pInParam, pOutParam | Provides a command a to define the ratio between the Velocity of the slave and master axes. This function is not supported at this moment. |
| MMC_GearInPos | hConn, hAxisRef, pInParam, pOutParam | Provides a command to define the gear ratio between the position of the slave and master axes from synchronization point onwards. This function is not supported at this moment. |
| MMC_GearOut | hConn, hAxisRef, pInParam, pOutParam | Provides a command to disengage a gear between slave and master axes, actually MC_Stop at this stage. This function is not supported at this moment. |
| MMC_ChangeToPreOPMode | hConn, pOutParam | Changes the Maestro to preoperational mode. |
| MMC_ChangeToOperationMode | hConn, pOutParam | Changes the Maestro to operational mode. |
| MMC_ClearNodeFbList | hConn, pInParam, pOutParam | This adds the ability to clear the function block list of a specific node, i.e. either Axis or Group.This can o nly be performed if the node is not in a moving state. |
| MMC_CmdStatus | hConn, pInParam, pOutParam | Sends a Read Function Block Status command to the Maestro server for specific Axis/Group and receive status back. |
| MMC_CloseConnection | hConn | Closes the connection to the Maestro. |
| MMC_Config | hConn, pInParam, pOutParam | Set the Maestro to configuration mode and allow changes to any configuration parameters. |
| MMC_CreateSYNCTimer | hConn, func, usSYNCTimerTime | Creates a SYNC timer to synchronize servo-drive, Maestro movements using the connection handle operator. |
| MMC_DestroySYNCTimer | hConn | Removes the SYNC timer to synchronize servo-drive, Maestro movements using the connection handle operator. |
| MMC_DownloadFoE | hConn, pInParam, pOutParam | Manages downloads of a file or files over EtherCAT to the Maestro. Important: To use this function refer to Elmo for support. |
| MMC_Exit | hConn, pInParam, pOutParam | Changes the Maestro from configuration mode back to regular mode. |
| MMC_FreeFbStat | hConn, pInParam, pOutParam | Returns debug information that contains the number of free function blocks in the system. |
| MMC_GetActiveVectorsNum | hConn, pInParam, pOutParam | Displays the number of active vectors (groups) attached and managed by the Maestro. |
| MMC_GetErrorCodeDescriptionByID | hConn, pInParam, pOutParam | This function receives an error\warning code and returns the description and resolution from the Personality file. |
| MMC_GetFoEStatus | hConn, pInParam, pOutParam | Obtains the File over EtherCAT status after a file download using MMC_DownloadFoE, from a host to the Maestro. Important: To use this function refer to Elmo for support. |
| MMC_GetEnquireFbStatus | hConn, pInParam, pOutParam | Obtains the current state global parameter Receive FB status in EAS. |
| MMC_GetAxisByName | hConn, pInParam, pOutParam | Returns an axis index reference by its name. |
| MMC_GetGroupByName | hConn, pInParam, pOutParam | This function returns a group index reference by its name. |
| MMC_GetGMASOperationMode | hConn, pOutParam | Returns the current GMAS operation mode. |
| MMC_GetStatusRegister | hConn, hAxisRef, pInParam, pOutParam | The purpose of the function is to provide usable information regarding the Maestro and axes statuses. |
| MMC_GetResList | hConn, pInParam, pOutParam | Returns the list of all resource files. |
| MMC_GetResSnapshot | hConn, pInParam, pOutParam | Save the resource configuration to temporary snapshot file. |
| MMC_GetVersion | hConn, sVersion | Obtains the Maestro version in the output parameter. |
| MMC_GetVersionEx | hConn, sVersion | Obtains the Maestro extended version in the output parameter. |
| MMC_GetLastError | hConn, chStr, iSize | Returns the last error that occurred in the designated connection. |
| MMC_InitConnection | eType, sConnParam, pCbFunc, pHndl | Initiates connection to the Maestro server. |
| MMC_RpcInitConnection | eType, sConnParam, pCbFunc, cpHostIPAddr, pHndl | Initiates RPC connection to Maestro server. |
| MMC_RpcInitConnectionEx | eType, sConnParam, pCbFunc, cpHostIPAddr, pHndl | Initiates RPC connection to Maestro server. |
| MMC_IPCInitConnection | sConnParam, pCbFunc, pHndl | Initiates IPC connection to Maestro server. |
| MMC_LoadParam | hConn, pInParam, pOutParam | Loads the axis, group, and global parameters from the xml file at the location: |
| MMC_ResetMultiAxisControl | hConn, pInParam, pOutParam | Internal reset of the Maestro multi-axis control. Allows the Maestro’s CPU to reset |
| MMC_ResExportFile | hConn, pInParam, pOutParam | Copies a requested file from the Maestro to the host via TFTP. |
| MMC_ResImportFile | hConn, pInParam, pOutParam | Copies a requested file from host to the Maestro via TFTP. MC_LIB_API int MMC_ResImportFileCmd( |
| MMC_SaveParam | hConn, pInParam, pOutParam | Save and/or update axes, group, and global parameters from the Maestro to a file at: |
| MMC_SetEnquireFbStatus | hConn, pInParam, pOutParam | Sets the state global parameter Receive FB status in EASII. |
| MMC_SetDefaultParameters | hConn, hAxisRef, pInParam, pOutParam | Sets the Maestro default manufacturer parameters to a specific Axis, or Group in the Maestro. |
| MMC_SetDefaultParametersGlobal | hConn, pInParam, pOutParam | Sets the Maestro default manufacturer global parameters in the Maestro. |
| MMC_SetIsToLoadGlobalParams | hConn, pInParam, pOutParam | Defines a flag whether to load or not, the global parameters, when updating the global parameters from a file to the Maestro. |
| MMC_ShowNodeStat | hConn, hAxisRef, pInParam, pOutParam | Displays the debug information for an Axis/Group. |
| MMC_GetActiveAxesNum | hConn, pInParam, pOutParam | Displays the number of active axes attached and managed by the Maestro. |
| MMC_ToggleConsoleOutput | hConn, pInParam, pOutParam | Toggles the Console output. This function is not available at this moment. |
| MMC_GetCyclesCounter | hConn, pInParam, pOutParam | Obtains the Maestro cycles counter value |
| MMC_WriteGroupOfParameters | hConn, hAxisRef, pInParam, pOutParam | This function writes a group of array parameters to the Maestro. |
| MMC_WriteGroupOfParametersEx | hConn, hAxisRef, pInParam, pOutParam | This function writes a group of regular and PI function parameters to the Maestro. |
| MMC_ReadGroupOfParameters | hConn, pInParam, pOutParam | This function retreive a group of parameters to the user. |
| MMC_WaitUntilConditionFB | hConn, hAxisRef, pInParam, pOutParam | The operation of this function block allows synchronization of numerous axes that are not part of a group, to start their motion together. In addition, it allows synchronization of numerous networ ked Maestro’s by starting a motion when a specific bit on a shared IO is raised. |
| MMC_WaitUntilConditionFBEx | hConn, hAxisRef, pInParam, pOutParam | The operation of this function block applies to both static and PI functions, and allows synchronization of numerous axes that are not part of a group, to start their motion together. In addition, it allows synchronization of numerous networked Maestro’s by starting a motion when a specific bit on a shared IO is raised. |
| MMC_WriteMemoryRange | hConn, hAxisRef, pInParam, pOutParam | This function writes a memory range for an EtherCAT slave. |
| MMC_ReadMemoryRange | hConn, hAxisRef, pInParam, pOutParam | This function reads a memory range from an EtherCAT slave. |
| MMC_SetDefaultResources | hConn, pInParam, pOutParam | This function restores the Maestro resource file to its factory efault according to the desired communication type; eCOMM_TYPE_ETHERCAT or eCOMM_TYPE_CAN |
| MMC_UserCommandControl | hConn, pInParam, pOutParam | This function executes a user command (user program or execute LINUX command). |
| MMC_SetAllFbExeModeImm | hConn, pInParam, pOutParam | This function sets all function blocks to immediate execution mode |
| MMC_BeginRecordingEx | hConn, pInParam, pOutParam | Starts the recording of internal controller variables and PI variables data from the Maestro server. |
| MMC_ReadPIVarBOOL | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Boolean variable according to its index |
| MMC_ReadPIVarChar | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Character variable according to its index |
| MMC_ReadPIVarUChar | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Unsigned Character variable according to its index |
| MMC_ReadPIVarShort | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Short Variable according to its index |
| MMC_ReadPIVarUShort | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Unsigned Short Variable according to its index |
| MMC_ReadPIVarInt | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Integer Variable according to its index |
| MMC_ReadPIVarUInt | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Unsigned Integer Variable according to its index |
| MMC_ReadPIVarFloat | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Float Variable according to its index |
| MMC_ReadPIVarRaw | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output RAW Variable according to its index, where the variable ≤ 32 bit |
| MMC_ReadPIVarLongLong | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Long Long Variable according to its index |
| MMC_ReadPIVarULongLong | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Unsigned Long Long Variable according to its index |
| MMC_ReadPIVarDouble | hConn, hAxisRef, pInParam, pOutParam | This function reads a Processing Image input\output Double Variable according to its index |
| MMC_ReadLargePIVarRaw | hConn, hAxisRef, pInParam, pOutParam | This function reads a large Processing Image input\output RAW Variable according to its index, where the variable > 32 bit. |
| MMC_WritePIVarBool | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Boolean variable according to its index |
| MMC_WritePIVarChar | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Character variable according to its index |
| MMC_WritePIVarUChar | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Unsigned Character variable according to its index |
| MMC_WritePIVarUShort | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Unsigned Short variable according to its index |
| MMC_WritePIVarShort | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Short variable according to its index |
| MMC_WritePIVarUInt | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Unsigned Integer Variable according to its index |
| MMC_WritePIVarInt | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Integer Variable according to its index |
| MMC_WritePIVarFloat | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Float Variable according to its index |
| MMC_WritePIVarRaw | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output RAW Variable according to its index |
| MMC_WritePIVarULongLong | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Unsigned Long Long Variable according to its index |
| MMC_WritePIVarLongLong | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Long Long Variable according to its index |
| MMC_WritePIVarDouble | hConn, hAxisRef, pInParam, pOutParam | This function writes a Processing Image input\output Double Variable according to its index |
| MMC_WriteLargePIVarRaw | hConn, hAxisRef, pInParam, pOutParam | This function writes a large Processing Image input\output RAW Variable according to its index |
| MMC_GetPIVarInfo | hConn, hAxisRef, pInParam, pOutParam | This function returns the detailed information about a required Processing Image variable, reading the variable according to its index |
| MMC_GetPIVarInfoByAlias | hConn, hAxisRef, pInParam, pOutParam | This function returns the detailed number of mapped Processing Image variables, reading the variable alias as a key. |
| MMC_GetPIVarsRangeInfo | hConn, hAxisRef, pInParam, pOutParam | This function allows the user to upload information in a range of PI variables. |
| MMC_GePIMemOffset | hConn, pInParam, pOutParam | This function provides the PI memory offset for the maestro |
| MMC_PerformBulkReadCmdPI | hConn, pInParam, pOutParam | This function allows the user to perform PI bulk read of parameters. |
| MMC_BeginRecording | hConn, pInParam, pOutParam | Starts the recording of internal controller variables data from the Maestro server. |
| MMC_StopRecording | hConn, pInParam, pOutParam | Halts recording of the Maestro server data. |
| MMC_UploadData | hConn, pInParam, pOutParam | Uploads recording data to the Maestro. |
| MMC_RecStatus | hConn, pInParam, pOutParam | Requests the status of the recording. |
| MMC_UploadDataHeader | hConn, pOutParam | Recorder upload data header. |
| MMC_ConfigBulkRead | hConn, pInParam, pOutParam | Configures the function to read all parameters from multiple axes. |
| MMC_PerformBulkRead | hConn, pInParam, pOutParam | Reads those parameters which were configured by a call to ConfigBulkRead, from multiple axes. |
| MMC_InsertNotificationFb | hConn, hAxisRef, pInParam, pOutParam | Inserts a notification function block within a queue to trigger an event. For details refer to section GlobalAsyncReply_Received (C++) |
| MMC_ClearEventsMask | hConn, pInParam, pOutParam | Clears the events mask for a specific connection depending to the input mask. |
| MMC_DisableMotionEndedEvent | hConn, hAxisRef, pInParam, pOutParam | Disables the motion ended event mechanism for a specific node, a nd no feedback is sent from the Maestro regarding the progress of the motion. |
| MMC_EnableMotionEndedEvent | hConn, hAxisRef, pInParam, pOutParam | Enables the motion ended event mechanism for a specific node. |
| MMC_GetEventsMask | hConn, pInParam, pOutParam | Returns the 32 bit events mask for a specific connection. |
| MMC_SetEventsMask | hConn, pInParam, pOutParam | Sets the 32-bit events mask for a specific connection defined by the input mask parameter iEventsMask . |
| MMC_LoadErrorCorrTable | hConn, pInParam, pOutParam | Loads an error correction table to memory. Error correction is then performed according o this table. |
| MMC_EnableErrorCorrTable | hConn, pInParam, pOutParam | Enables the usage of an error correction table. |
| MMC_GetErrorTableStatus | hConn, pInParam, pOutParam | Function recieves the table number as input and returns an answer whether the table is loaded and/or enabled. |
| MMC_DisableErrorCorrTable | hConn, pInParam, pOutParam | Disables the usage of an error correction table. |
| MMC_UnloadErrorCorrTable | hConn, pInParam, pOutParam | Unloads an error correction table from memory. |
| Open | cFileName, uiFlags, cFilePath | Opens the XML file, with specific parameters |
| Close |  | Closes the file pointed to the XML file and release resource used for parsing the file |
| Read | pCtgryVal, pTagName, dVal, lVal, bVal, pStr, dDefault, lDefault, bDefault, lMin, dMax, lMax, lLen, iActRdElm, iReqRdElm, dMin | List of overloaded function that retrieves data to given variable. The Values may be of a double (single or array), long (single or array), Boolean, or String according to the number, type, and order of the parameters: Read single value parameters • Double, retrieve one parameter of type Double • Long, retrieve one parameter of type Long • Boolean, retrieve one parameter of type Boolean, ignores white space, bu expects True / False |
| GetXmlFileRoot | pAtt1, pAtt2, lLen | Returns the XML file root (XSI ID values) pAtt1 and XSI Location |
| GetXmlFileDescrp | pAtt1, pAtt2, lLen | Returns the XML "file description name" represented by pAtt1, and XML file version as pAtt2, the buffer size for return values which are at least lLen in size. |
| MMC_CloseUdpChannel | hConn, pInParam, pOutParam | Closes a UDP channel per RPC/IPC connection. |
| MMC_GetDefGateway | hConn, pInParam, pOutParam | Reads the default gateway IP address. |
| MMC_GetDhcp | hConn, pInParam, pOutParam | Reads the DHCP mode. |
| IMMC_GetIpAddr | hConn, pInParam, pOutParam | Reads the DHCP mode. |
| MMC_GetIpMask | hConn, pInParam, pOutParam | Reads the IP mask. |
| MMC_GetServerIp | hConn, pInParam, pOutParam | Obtain the Server IP address. |
| MMC_NetworkInfo | hConn, pInParam, pOutParam | Returns the network information, detailing the systems connected and/or defined in the resources file located in the Maestro FLASH. |
| MMC_NetworkScan | hConn, pInParam, pOutParam | Scans the network to locate nodes on the network. |
| MMC_OpenUdpChannel | hConn, pInParam, pOutParam | Opens a UDP channel per RPC/IPC connection. |
| MMC_SetDefGateway | hConn, pInParam, pOutParam | Set default gateway IP address. |
| MMC_SetDhcp | hConn, pInParam, pOutParam | Sets the DHCP Mode for the Maestro. |
| MMC_SetIpAddr | hConn, pInParam, pOutParam | Sets the Maestro IP address. |
| MMC_SetIpMask | hConn, pInParam, pOutParam | Set the IP netmask of the Maestro. |
| MMC_SetServerIp | hConn, pInParam, pOutParam | Set the Server IP address of the host. |
| MMC_MbusIsRunning | hConn, pInParam, pOutParam | Signals that the Modbus connection is operational. |
| MMC_MbusReadCoilsTable | hConn, pInParam, pOutParam | Reads part of Modbus coils table. |
| MMC_MbusReadHoldingRegisterTable | hConn, pInParam, pOutParam | Reads part of Modbus holding register table or the holding registers. |
| MMC_MbusReadInputsTable | hConn, pInParam, pOutParam | Reads inputs to the Modbus Inputs Table. |
| MMC_MbusStartServer | hConn, pInParam, pOutParam | Starts the Modbus server listening thread with an ID value as a parameter. |
| MMC_MbusStopServer | hConn, pInParam, pOutParam | Stops the Modbus server listening thread. |
| MMC_MbusWriteCoilsTable | hConn, pInParam, pOutParam | Writes to part of Modbus coils table inside the Modbus where every parameter >0, is similar to Boolean value 1. |
| MMC_MbusWriteHoldingRegisterTable | hConn, pInParam, pOutParam | Writes to part of the Modbus register table inside the Modbus. |
| MMC_CancelVirtualEncoder | hConn, hAxisRef, pInParam, pOutParam | This function cancels a defined servo-drive as the virtual CAN encoder. |
| MMC_CancelParamEvPDO3 | hConn, hAxisRef, pInParam, pOutParam | Cancels the TPDO3 and RXPDO3 event processing. |
| MMC_CancelParamEvPDO4 | hConn, hAxisRef, pInParam, pOutParam | Cancels the TPDO4 and RXPDO4 event processing. |
| MMC_CfgRegParamEvPDO3 | hConn, hAxisRef, pInParam, pOutParam | Configures regular parameter event PDO3 according group type. |
| MMC_CfgRegParamEvPDO4 | hConn, hAxisRef, pInParam, pOutParam | Configures regular parameter event PDO4 according group type. |
| MMC_CfgUserParamEvPDO3 | hConn, hAxisRef, pInParam, pOutParam | Configures user parameter event PDO3 according to group type. |
| MMC_CfgUserParamEvPDO4 | hConn, hAxisRef, pInParam, pOutParam | Configures user parameter event PDO4 according to group type. |
| MMC_ChangeDefaultPDOConfiguration | hConn, hAxisRef, pInParam, pOutParam | Changes the default PDO communication parameter. |
| MMC_ConfigEventModePDO3 | hConn, hAxisRef, pInParam, pOutParam | Configures event mode for the PDO3 according group type. |
| MMC_ConfigEventModePDO4 | hConn, hAxisRef, pInParam, pOutParam | Configures event mode for the PDO4 according group type. |
| MMC_ConfigVirtualEncoder | hConn, hAxisRef, pInParam, pOutParam | This function defines a servo-drive as the virtual CAN encoder. |
| MMC_GetAxisByCanId | hConn, pInParam, pOutParam | Obtains axis handle according to the CANbus identity. |
| MMC_GetPDOInfo | hConn, hAxisRef, pInParam, pOutParam | Obtains the PDO information of PDO 3 and 4. |
| MMC_GetSyncTime | hConn, pInParam, pOutParam | Where CANbus communication is relevant, returns the SYNC time. |
| MMC_PDOGeneralRead | hConn, hAxisRef, pInParam, pOutParam | Reads a specific PDO message command. |
| MMC_PDOGeneralWrite | hConn, hAxisRef, pInParam, pOutParam | Writes a specific PDO message command. |
| MMC_ReceiveCANRawData | hConn, hAxisRef, iTimeOutms, pOutParam | Receives prepared CANopen RAW data (DS-301 or DS-402). |
| MMC_SendCANRawData | hConn, hAxisRef, pInParam, pOutParam | Sends prepared CANopen RAW data (DS-301 or DS-402). |
| MMC_SendandReceiveCANRawData | hConn, hAxisRef, pInParam, pOutParam | Sends and receives prepared CANopen RAW data (DS-301 or DS-402). |
| MMC_SendCmd | hConn, hAxisRef, pInParam, pOutParam | Not in operation Sends a command string to the drive. |
| MMC_SetHeartBeatConsumer | hConn, pInParam, pOutParam | Sets the consumer heartbeat as an event to the user. |
| MMC_SetSyncTime | hConn, pInParam, pOutParam | Where CANbus communication is relevant, sets the Sync time in the communication module, an updates the relevant nodes whose motion mode has an IP address. It also updates the kernel with the Sync time. |
| MMC_StartBulkUpload | hConn, hAxisRef, pInParam, pOutParam | The Maestro manages the bulk upload process upon request from a host, i.e. the host sends this fu nction command to the Maestro, and the Maestro uploads the recording buffer. |
| MMC_GetBulkUploadStatus | hConn, hAxisRef, pInParam, pOutParam | During the whole process of a bulk upload, the status is retrieved using this function command. |
| MMC_GetBulkUploadData | hConn, hAxisRef, pInParam | The Maestro manages the upload process upon request from a host, i.e. the host sends a "begin upload" command, and the Maestro uploads the recording buffer. Afterwards, the host sends this function |
| MMC_ResetCommStatistics | hConn, pInParam, pOutParam | Reset all communication statistics. Resets the communication error counters. |
| MMC_SendSDO | hConn, hAxisRef, pInParam, pOutParam, objectIndex, objectSubIndex, data, dataLength, timeout | Sends SDO message command, in units of 1, 2, or 4 bytes. |
| MMC_SendSDOEx | hConn, hAxisRef, pInParam, pOutParam, objectIndex, objectSubIndex, data, dataLength, timeout | Sends SDO message command, in units of 1, 2, or 4 bytes. |
| MMC_SendSdoAsync | hConn, hAxisRef, pInParam, pOutParam | Sends SDO asynchronized message command, in units of 1, 2, or 4 bytes. |
| MMC_RetrieveSDOAsync | hConn, hAxisRef, pOutParam | Sends SDO asynchronized message command, in units of 1, 2, or 4 bytes. |
| MMC_SendSdoAsyncEx | hConn, hAxisRef, pInParam, pOutParam | Sends SDO asynchronized message command, in units of 1, 2, or 4 bytes. |
| MMC_CancelGeneralRPDO3 | hConn, hAxisRef, pInParam, pOutParam | Cancels the general configuration of the DS-401 node or Maestro for RX at PDO3. |
| MMC_CancelGeneralRPDO4 | hConn, hAxisRef, pInParam, pOutParam | Cancels the general configuration of the DS-401 node or Maestro for RX at PDO4. |
| MMC_CancelGeneralTPDO3 | hConn, hAxisRef, pInParam, pOutParam | Cancels the general configuration of the DS-401 node or Maestro for TX at PDO3. |
| MMC_CancelGeneralTPDO4 | hConn, hAxisRef, pInParam, pOutParam | Cancels the general configuration of the DS-401 node or Maestro for TX at PDO4. |
| MMC_ConfigGeneralRPDO3 | hConn, hAxisRef, pInParam, pOutParam | Generally configures the DS-401 node or Maestro for RX at PDO3. |
| MMC_ConfigGeneralRPDO4 | hConn, hAxisRef, pInParam, pOutParam | Generally configures the DS-401 node or Maestro for RX at PDO4. |
| MMC_ConfigGeneralTPDO3 | hConn, hAxisRef, pInParam, pOutParam | Generally configures the DS-401 node or Maestro for TX at PDO3. |
| MMC_ConfigGeneralTPDO4 | hConn, hAxisRef, pInParam, pOutParam | Generally configures the DS-401 node or Maestro for TX at PDO4. |
| MMC_DisableDS401DIChangedEvent | hConn, hAxisRef, pInParam, pOutParam | Disables a DS401 digital input event change against an I/O module. |
| MMC_EnableDS401DIChangedEvent | hConn, hAxisRef, pInParam, pOutParam | Enables an DS401 digital input event change. |
| MMC_ReadDS401DIGroup | hConn, hAxisRef, pInParam, pOutParam | Reads the DS-401 digital inputs of a group of 8 digital I/Os. |
| MMC_ReadDS401DInput | hConn, hAxisRef, pInParam, pOutParam | Reads the DS-401 digital input of all 64 bit I/O’s in one action, increasing the communication speed proportionately versus reading 8 x groups of 8 I/O’s. |
| MMC_WriteDS401DOGroup | hConn, hAxisRef, pInParam, pOutParam | Writes the DS-401 digital outputs of a group of 8 I/O’s to the Maestro. |
| MMC_WriteDS401DOutput | hConn, hAxisRef, pInParam, pOutParam | Writes to all of the DS-401 digital outputs assigned to TPDO1 at once, up to 64 bit I/O’s in one action, increasing the communication speed proportionately versus writing to 8 x groups of 8 I/O’s. |
| GetSlaveScanAlias | hConn, pInParam, pOutParam | A new API has been added: MC_GetSlaveScanAlias C: |
| MMC_DisableEthercatConfigMode | hConn, pOutParam | Disables the EtherCAT configuration mode. Enables the Maestro task manager to disable direct programming of the Maestro via the Gateway. |
| MMC_EnableEthercatConfigMode | hConn, pOutParam | Disables the Maestro task manager to enable direct programming of the Maestro via the Gateway. |
| MMC_ECATIODisableDIChangedEvent | hConn, hAxisRef, pInParam, pOutParam | Disables an EtherCAT I/O input event change against an I/O module. |
| MMC_ECATIOEnableDIChangedEvent | hConn, hAxisRef, pInParam, pOutParam | Enables an EtherCAT I/O input event change. |
| MMC_ECATIOReadDigitalInput | hConn, hAxisRef, pInParam, pOutParam | Reads the EtherCAT I/O input of all 64 bit I/O’s in one action, increasing the communication speed proportionately versus reading 8 x groups of 8 I/O’s. |
| MMC_ECATIOReadAnalogInput | hConn, hAxisRef, pInParam, pOutParam | Reads the EtherCAT I/O analog input. |
| MMC_ECATIOWriteAnalogOutput | hConn, hAxisRef, pInParam, pOutParam | Writes to the EtherCAT I/O analog outputs. |
| MMC_ECATIOWriteDigitalOutput | hConn, hAxisRef, pInParam, pOutParam | Writes to the EtherCAT I/O outputs of all 64 bit I/O’s in one action, increasing the communication speed proportionately versus writing to 8 x groups of 8 I/O’s. |
| MMC_GetCommStatistics | hConn, hAxisRef, pInParam, pOutParam | Receives communication statistics for a specific axis. It is recommended to use the function MMC_GetEthercatCommStatistics rather than this function. The |
| MMC_GetEthercatCommStatistics | hConn, pInParam, pOutParam | Obtains the EtherCAT communication statistics used as part of the FoE download mechanism in the EAS application. |
| MMC_GetCommDiagnostics | hConn, pInParam, pOutParam | Receives communication diagnostics for specific axis. |
| MMC_GetReactorStatistics | hConn, hAxisRef, pInParam, pOutParam | Obtains the statistics from the Maestro server base processor. |
| MMC_IsEthercatConfigMode | hConn, pOutParam | Defines whether the EtherCAT configuration mode is operational or not. |
| MMC_ResetCommDiagnostics | hConn, pInParam, pOutParam | Resets the CRC counters registers of all slaves on the bus to 0. The CRC counters registers can be retrieved via the GetCommDiagnostics function. |
| MMC_ResetCommStatistics | hConn, pInParam, pOutParam | Reset all communication statistics. Resets the communication error counters. |
| MMC_ElmoExecuteLabel | hConn, hAxisRef, pInParam, pOutParam | Executes the user program that was downloaded via the EAS application. |
| MMC_ElmoSetParameter | hConn, hAxisRef, ucValType, pVal | Sets the Elmo drive parameter with a specific name in the servo drive. |
| MMC_ElmoGetParameter | hConn, hAxisRef, ucValType | Request to receive the Elmo parameters from the servo drive. |
| MMC_ElmoGetParameterAndRetrieveData | hConn, hAxisRef, ucValType, pVal, uiErrorID | Synchronously requests a parameter in the servo drive and retrieves it. |
| MMC_ElmoQueryOperationFIFOIndex | hConn, hAxisRef, iReceivedMsgIdx | Returns the FIFO index. |
| MMC_ElmoQueryOperationFIFORetrieveData | hConn, hAxisRef, pVal, uiErrorID | Request the FIFO index to retrieve data. |
| MMC_ElmoQueryOperationFIFOIndexReset | hConn, hAxisRef | Erases the message FIFO to 0. |
| MMC_ElmoCall | hConn, hAxisRef | ElmoCall is used to call a subroutine, a user program, where cCmd[3] is the name of the program |
| EipWriteAdpTag | pInParam, pOutParam | Writes adapter tag data according to the tag type. |
| EipReadAdpTag | pInParam, pOutParam | Reads the adapter tag data according to the tag type. Copies adapter tag data from memory into input buffer. |
| EipGetAssemblyRefByInstance | pInParam, pOutParam | Reads the assembly information according to the instance reference .Locates the asm_instance and applies a reference to this instance. |
| EipGetAssemblyRefByName | pInParam, pOutParam | Reads the assembly information according to the name reference .This function returns the assembly reference index according to its name. |
| EipSetAssembly | pInParam, pOutParam | Fills the assembly data with out_buff data and sends it through EthernetIP. |
| EipGetAssembly | pInParam, pOutParam | Copies an assembly data identified by instance to in_buff. |
| EipGetDevTagRefByName | pInParam, pOutParam | This function returns device tag reference index according to its name. |
| EipSetDevTag | pInParam, pOutParam | Writes the device tag data according to the tag type. Updates device tag data and sends it to the EIP device. |
| EipGetDevTag | pInParam, pOutParam | Reads the device tag data according to the tag type. Sends request to the EIP device to read specific device tag. |
| EipReadDevTagData | pInParam, pOutParam | Reads and stores device tag data received from an EIP device, as a response to user request. |
| EipSyncGetDevTag | pInParam, pOutParam | Sends a request to read device tag data, and waits for a response to be received. |
| EipCheckDevTagReply | pInParam, pOutParam | Check that a reply has been received for a specific device tag request. |
| EipOpenSession | pCallBackFunc, pInParam, pOutParam | Initialize and start an EIP session in order to be able to use EthernetIP. |
| EIPCloseSession | pInParam, pOutParam | Close an EtherNETIP session and free allocated memory before terminating program. |
| EipCreate | pInParam, pOutParam | Create an EtherNetIP session. |
| EipDestroy | pInParam, pOutParam | Kills the EtherNETIP session. |

# Chapter 10 API Services and Operations

- Source PDF: `Maestro Administrative and Motion API_2022_12_v2.012_bookmarks_fixed.pdf`
- PDF physical pages: 956-989
- Chunk: `038_p0956-p0989_Chapter-10-API-Services-and-Operations.md`

## Active Outline At Chunk Start
- p. 956 - Chapter 10 API Services and Operations

## Contained Bookmark Outline
- p. 956 - Chapter 10 API Services and Operations
  - p. 958 - 10.1 Wait Condition Function Block
  - p. 959 - 10.2 Main Configuration Function Blocks
    - p. 960 - 10.2.1 MMC_ChangeToPreOPMode
    - p. 962 - 10.2.2 MMC_ChangeToOperationMode
    - p. 964 - 10.2.3 MMC_ClearNodeFbList
    - p. 966 - 10.2.4 MMC_CmdStatus
    - p. 969 - 10.2.5 MMC_CloseConnection
    - p. 970 - 10.2.6 MMC_Config
    - p. 972 - 10.2.7 MMC_CreateSYNCTimer
    - p. 973 - 10.2.8 MMC_DestroySYNCTimer
    - p. 974 - 10.2.9 MMC_DownloadFoE
    - p. 980 - 10.2.10 MMC_Exit
    - p. 982 - 10.2.11 MMC_FreeFbStat
    - p. 985 - 10.2.12 MMC_GetActiveVectorsNum
    - p. 987 - 10.2.13 MMC_GetErrorCodeDescriptionByID

## Extracted Text

### PDF page 956
<a id="pdf-page-956"></a>
#### Chapter 10 API Services and Operations
Chapter 10 API Services and Operations
This chapter describes the API services and operations for the Maestro, and involves the following:
- Main configuration variables
- Maestro Preoperational Mode. Refer to Chapter 2 Maestro Overview section for further details
- EtherCAT Configuration Mode. Refer to Chapter 2 Maestro Overview, section for further details
- Data Recording. Refer to section PI Functions and Implementation Examples for further details
- Resource file uploading and downloading
- Download new firmware version
The following main configuration function blocks are described, where MMC_Connection_Param_Struct is an
administrative function only:
Function Block Services and Operation
MMC_InitConnection Main configuration variables
MMC_IPCInitConnection
MMC_RpcInitConnection
MMC_CloseConnection
MMC_CmdStatus
MMC_Config
MMC_Exit
MMC_FreeFbStat
MMC_GetAxisByName
MMC_GetGroupByName
MMC_GetVersion
MMC_ResetMultiAxisControl
MMC_SaveParam
MMC_ShowNodeStat
MMC_ClearNodeFbList
MMC_CloseConnection
MMC_CreateSYNCTimer
MMC_DestroySYNCTimer
MMC_DownloadFoE
MMC_Dwell
MMC_GetActiveVectorsNum

### PDF page 957
<a id="pdf-page-957"></a>
MMC_GetErrorCodeDescriptionByID
MMC_GetFoEStatus
MMC_GetEthercatCommStatistics
MMC_GetEnquireFbStatus
MMC_GetGroupMembersInfo
MMC_GetStatusRegister
MMC_GetVersionEx
MMC_LoadParam
MMC_RpcInitConnectionEx
MMC_SetEnquireFbStatus
MMC_SetDefaultParameters
MMC_SetDefaultParametersGlobal
MMC_SetIsToLoadGlobalParams
MMC_GetActiveAxesNum
MMC_ToggleConsoleOutput
MMC_GetCyclesCounter
MMC_WriteGroupOfParameters
MMC_ReadGroupOfParameters
MMC_WaitUntilConditionFB
MMC_ChangeToPreOPMode Maestro Preoperational Mode
MMC_ChangeToOperationMode
GetGMASOperationMode
MMC_GetResList Resource file variables involving list, snapshot, export,
and import. MMC_GetResSnapshot
MMC_ResExportFile
MMC_ResImportFile
MMC_GetVerPath
MMC_DownloadVersion
MMC_ReadDownloadVersionStatus
MMC_SetVerPath

### PDF page 958
<a id="pdf-page-958"></a>
##### 10.1 Wait Condition Function Block
10.1 Wait Condition Function Block
The current Wait Condition mechanism inserts a function block to the queue. This function block receives the
state DONE only when a condition is true, and restrains the function block queue, not allowing other functi on
blocks to execute until it recieves the state DONE.
[PDF field-code object omitted]
The current implementation using the function MMC_WaitUntilConditionFB () only allows performing a
condition on the available parameters which are static and are not dependent on the configuration. However,
to include enabling a condition for a PI variable value, the user should use the more flexible version
MMC_WaitUntilConditionFBEx. The user can set a condition not only on a parameter from the parameters list,
but also on a PI input or output variable.

### PDF page 959
<a id="pdf-page-959"></a>
##### 10.2 Main Configuration Function Blocks
10.2 Main Configuration Function Blocks
The following main configuration function blocks are described:
Main Configuration
MMC_ChangeToPreOPMode
MMC_ChangeToOperationMode
MMC_ClearNodeFbList
MMC_CmdStatus
MMC_CloseConnection
MMC_Config
MMC_CreateSYNCTimer
MMC_DestroySYNCTimer
MMC_DownloadFoE
MMC_Exit
MMC_FreeFbStat
MMC_GetActiveVectorsNum
MMC_GetErrorCodeDescriptionByID
MMC_GetFoEStatus
MMC_GetEnquireFbStatus
MMC_GetAxisByName
MMC_GetGroupByName
MMC_GetGroupMembersInfo
MMC_GetGMASOperationMode
MMC_GetStatusRegister
MMC_GetResList
MMC_GetResSnapshot
MMC_GetVersion
MMC_GetVersionEx
MMC_InitConnection
MMC_IPCInitConnection
MMC_LoadParam
MMC_RpcInitConnection
MMC_RpcInitConnectionEx
MMC_ResetMultiAxisControl
MMC_ResExportFile
MMC_ResImportFile
MMC_SaveParam
MMC_SetEnquireFbStatus
MMC_SetDefaultParameters
MMC_SetDefaultParametersGlobal
MMC_SetIsToLoadGlobalParams
MMC_ShowNodeStat
MMC_GetActiveAxesNum
MMC_ToggleConsoleOutput
MMC_GetCyclesCounter
MMC_WriteGroupOfParameters
MMC_WriteGroupOfParametersEx
MMC_ReadGroupOfParameters
MMC_WaitUntilConditionFB
MMC_WaitUntilConditionFBEx
MMC_WriteMemoryRange
MMC_ReadMemoryRange
MMC_SetDefaultResources
MMC_KillRepetitive
MMC_UserCommandControl
MMC_GetVerPath
MMC_DownloadVersion
MMC_ReadDownloadVersionStatus
MMC_SetVerPath

### PDF page 960
<a id="pdf-page-960"></a>
###### 10.2.1 MMC_ChangeToPreOPMode
10.2.1 MMC_ChangeToPreOPMode
Changes the Maestro to preoperational mode.
MMC_LIB_API int MMC_ChangeToPreOPMode(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SET_GMAS_PREOP_IN* pInParam
OUT MMC_SET_GMAS_PREOP_OUT* pOutParam
);
Motion Mode NC - Not relevant Distributed - not relevant
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_SET_GMAS_PREOP input data structure using the
MMC_ChangeToPreOPMode function.
pOutParam
Points to the MMC_SET_GMAS_PREOP_OUT output structure receiving information, as
a result of calling the MMC_ChangeToPreOPMode function.
Remarks
None
Scope
All
MMC_SET_GMAS_PREOP_IN Structure
typedef struct{
unsigned char ucDummy;
}MMC_SET_GMAS_PREOP_IN;
Parameters
dummy
Dummy input. Any positive character value.
MMC_SET_GMAS_PREOP_OUT Structure

### PDF page 961
<a id="pdf-page-961"></a>
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_SET_GMAS_PREOP_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and NC Profiler Error
IDs.
Figure 304 describes the function block for MMC_ChangeToPreOPMode.
[PDF field-code object omitted]
Figure 304: MMC_ChangeToPreOPMode function block
10.2.1.1 Function Block Code Example
int rc;
MMC_SET_GMAS_PREOP_IN stSetGMASPreOp_in;
MMC_SET_GMAS_PREOP_OUT stSetGMASPreOp_out;
//
// Inserting the structure parameters:
stSetGMASPreOp_in.ucDummy = 1; // Dummy input
//
rc = MMC_ChangeToPreOPMode (hConn, &stSetGMASPreOp_out);
if (rc != 0)
{
HandleError();
}

### PDF page 962
<a id="pdf-page-962"></a>
###### 10.2.2 MMC_ChangeToOperationMode
10.2.2 MMC_ChangeToOperationMode
Changes the Maestro to operational mode.
MMC_LIB_API int MMC_ChangeToOperationMode(
IN MMC_CONNECT_HNDL hConn,
OUT MMC_SET_GMAS_OP_OUT* pOutParam
);
Motion Mode NC - Not relevant Distributed - not relevant
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_SET_GMAS_OP input data structure using the
MMC_ChangeToOperationMode function.
pOutParam
Points to the MMC_SET_GMAS_OP_OUT output structure receiving information, as a
result of calling the MMC_ChangeToOperationMode function.
Remarks
None
Scope
All
MMC_SET_GMAS_OP_IN Structure
typedef struct
{
unsigned char ucDummy;
}MMC_SET_GMAS_OP_IN;
Parameters
dummy
Dummy input. Any positive character value.

### PDF page 963
<a id="pdf-page-963"></a>
MMC_SET_GMAS_OP_OUT Structure
typedef struct
{
unsigned short usStatus;
short usErrorID;
}MMC_SET_GMAS_OP_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and NC Profiler Error
IDs.
Figure 305 describes the function block for MMC_ChangeToOperationMode.
[PDF field-code object omitted]
Figure 305: MMC_ChangeToOperationMode function block
10.2.2.1 Function Block Code Example
int rc;
MMC_SET_GMAS_OP_IN stSetGMASOp_in;
MMC_SET_GMAS_OP_OUT stSetGMASOp_out;
//
// Inserting the structure parameters:
stSetGMASOp_in.ucDummy = 1; // Dummy input
//
rc = MMC_ChangeToOperationMode (hConn, &stSetGMASOp_out);
if (rc != 0)
{
HandleError();
}

### PDF page 964
<a id="pdf-page-964"></a>
###### 10.2.3 MMC_ClearNodeFbList
10.2.3 MMC_ClearNodeFbList
This adds the ability to clear the function block list of a specific node, i.e. either Axis or Group.This can o nly
be performed if the node is not in a moving state.
int MMC_ClearNodeFbListCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_CLEARFBLIST_IN* pInParam,
OUT MMC_CLEARFBLIST_OUT* pOutParam
);
Motion Mode NC - Not relevant Distributed - not relevant
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle i s returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_CLEARFBLIST input data structure using the MMC_ClearNodeFbList
function.
pOutParam
Points to the MMC_CLEARFBLIST_OUT output structure receiving information, as a
result of calling the MMC_ClearNodeFbList function.
Remarks
Refer to the use of the function in section 10.2.12MMC_GetActiveVectorsNum.
Scope
All
MMC_CLEARFBLIST_IN Structure
typedef struct mmc_clearfblist_in{
unsigned short usAxisRef;
}MMC_CLEARFBLIST_IN;
Parameters
usAxisRef

### PDF page 965
<a id="pdf-page-965"></a>
The axis reference. Any positive bitwise integer.
MMC_CLEARFBLIST_OUT Structure
typedef struct mmc_clearfblist_out{
unsigned short usStatus;
short sErrorID;
}MMC_CLEARFBLIST_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and NC Profiler Error
IDs.
Figure 306 describes the function block for MMC_ClearNodeFbList
[PDF field-code object omitted]
Figure 306: MMC_ClearNodeFbList function block

### PDF page 966
<a id="pdf-page-966"></a>
###### 10.2.4 MMC_CmdStatus
10.2.4 MMC_CmdStatus
Sends a Read Function Block Status command to the Maestro server for specific Axis/Group and receive
status back.
MMC_LIB_API int MMC_CmdStatus(
IN MMC_CONNECT_HNDL hConn,
IN MMC_FBSTATUS_IN* pInParam,
OUT MMC_FBSTATUS_OUT* pOutParam
);
Motion Mode NC - Not relevant Distributed - not relevant
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_FBSTATUS input data structure using the MMC_CmdStatus
function.
pOutParam
Points to the MMC_FBSTATUS_OUT output structure receiving information, as a result
of calling the MMC_CmdStatus function.
Remarks
None
Scope
All
MMC_FBSTATUS_IN Structure
typedef struct
{
unsigned int uiHndl;
} MMC_FBSTATUS_IN;
Parameters
uiHndl

### PDF page 967
<a id="pdf-page-967"></a>
Function block handle. Any positive integer value
MMC_FBSTATUS_OUT Structure
typedef struct
{
unsigned int uiFbStatus;
unsigned short usStatus;
short usErrorID;
unsigned short usFbErrorID;
} MMC_FBSTATUS_OUT;
Parameters
uiFbStatus
Returns the function block status. Any positive integer bitwise value.
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
usFbErrorID
Returned function block error ID. Signals where a function block error occurs. Refer to
the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 307 describes the function block for MMC_CmdStatus
[PDF field-code object omitted]
Figure 307: MMC_CmdStatus function block
10.2.4.1 Function Block Code Example
int rc;
MMC_FBSTATUS_IN stFBStatus_in;
MMC_FBSTATUS_OUT stFBStatus_out;
//
// Inserting the structure parameters:
stFBStatus_in.uiHndl = 1; //Function block handle
//
rc = MMC_CmdStatus (hConn, &stFBStatus_in, &stFBStatus_out);
if (rc != 0)

### PDF page 968
<a id="pdf-page-968"></a>
{
HandleError();
}

### PDF page 969
<a id="pdf-page-969"></a>
###### 10.2.5 MMC_CloseConnection
10.2.5 MMC_CloseConnection
Closes the connection to the Maestro.
MMC_LIB_API int MMC_CloseConnection(
IN MMC_CONNECT_HNDL hConn
);
Motion Mode NC - Not relevant Distributed - not relevant
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.

Remarks
None
Scope
The input parameter hConn is dependent on the connection handle value created when performing the
InitConnection function. This value should therefore be retained with other connection handle thread values
for use when a connection is to be closed.
Figure 308 describes the function block for MMC_CloseConnection
[PDF field-code object omitted]
Figure 308: MMC_CloseConnection function block
10.2.5.1 Function Block Code Example
int rc;
//
// Inserting the structure parameters:
hConn = 1 ; // Connection Handle Type. Number from the connection
handle
//
rc = MMC_CloseConnection (hConn);
printf("Connection State[%ld]\n", (long int)(MMC_CONNECT_HNDL) hConn);
if (rc != 0)
printf("ERROR:%s: MMC_CloseConnection fail\n", __func__);
{
HandleError();
}

### PDF page 970
<a id="pdf-page-970"></a>
###### 10.2.6 MMC_Config
10.2.6 MMC_Config
Set the Maestro to configuration mode and allow changes to any configuration parameters.
MMC_LIB_API int MMC_ConfigCmd
(IN MMC_CONNECT_HNDL hConn,
IN MMC_CONFIG_IN* pInParam,
OUT MMC_CONFIG_OUT* pOutParam
);
Motion Mode NC - Not relevant Distributed - not relevant
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_CONFIG input data structure using the MMC_Config function.
pOutParam
Points to the MMC_CONFIG_OUT output structure receiving information, as a result of
calling the MMC_Config function.
Remarks
There are two Maestro operational modes:
- Normal
- Configuration
When any communication configuration parameters (network IP etc.) are changed using the Set command,
this function is invoked to exit the configuration mode to the normal operational mode of the Maestro.
Scope
All
MMC_CONFIG_IN Structure
typedef struct mmc_config_in{
unsigned char dummy;
}MMC_CONFIG_IN;
Parameters

### PDF page 971
<a id="pdf-page-971"></a>
dummy
Dummy character. Any positive value accepted.
MMC_CONFIG_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_CONFIG_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 309 describes the function block for MMC_Config.
[PDF field-code object omitted]
Figure 309: MMC_Config function block
10.2.6.1 Function Block Code Example
int rc;
MMC_CONFIG_IN stConfig_in;
MMC_CONFIG_OUT stConfig_out;
//
// Inserting the structure parameters:
stConfig_in.dummy = 1; //dummy value
//
rc = MMC_ConfigCmd (hConn, &stConfig_out);
if (rc != 0)
{
HandleError();
}

### PDF page 972
<a id="pdf-page-972"></a>
###### 10.2.7 MMC_CreateSYNCTimer
10.2.7 MMC_CreateSYNCTimer
Creates a SYNC timer to synchronize servo-drive, Maestro movements using the connection handle
operator.
MMC_LIB_API int MMC_CreateSYNCTimer(
IN MMC_CONNECT_HNDL hConn,
IN MMC_SYNC_TIMER_CB_FUNC func,
IN unsigned short usSYNCTimerTime
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
func
[IN] Points to the callback function MMC_SYNC_TIMER_CB_FUNC using
MMC_CreateSYNCTimer.
usSYNCTimerTime
[IN] Defines the time between which a synchronization message is sent as an event.
Remarks
None
Scope
All
Figure 310 describes the function for MMC_CreateSYNCTimer.
[PDF field-code object omitted]
Figure 310: MMC_CreateSYNCTimer function

### PDF page 973
<a id="pdf-page-973"></a>
###### 10.2.8 MMC_DestroySYNCTimer
10.2.8 MMC_DestroySYNCTimer
Removes the SYNC timer to synchronize servo-drive, Maestro movements using the connection handle
operator.
MMC_LIB_API int MMC_DestroySYNCTimer(
IN MMC_CONNECT_HNDL hConn
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL i s the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
Remarks
None
Scope
All
Figure 311 describes the function for MMC_DestroySYNCTimer.
[PDF field-code object omitted]
Figure 311: MMC_DestroySYNCTimer function

### PDF page 974
<a id="pdf-page-974"></a>
###### 10.2.9 MMC_DownloadFoE
10.2.9 MMC_DownloadFoE
Manages downloads of a file or files over EtherCAT to the Maestro.
Important: To use this function refer to Elmo for support.
MMC_LIB_API int MMC_DownloadFoE(
IN MMC_CONNECT_HNDL hConn,
IN MMC_DOWNLOADFOE_IN* pInParam,
OUT MMC_DOWNLOADFOE_OUT* pOutParam);
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is co mmon
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_DOWNLOADFOE input data structure using the
MMC_DownloadFoE function.
pOutParam
Points to the MMC_DOWNLOADFOE_OUT output structure receiving information, as a
result of calling the MMC_DownloadFoE function.
Remarks
The FoE download procedure in the Maestro, is performed for N slaves on the bus i n parallel. The master
moves the slaves to Bootstrap mode and sends periodically at each Mailbox cycle, a full Mailbox message to
each slave containing relevant sections of the file, until the complete file is downloaded to all N slaves.
Scope
EtherCAT
MMC_DOWNLOADFOE_IN Structure
t typedef struct mmc_downloadfoe_in{
unsigned short pwSlaveId[NC_NODES_SING_AXIS_NUM];
char pcFileName[256];
unsigned char pucServer[4];
unsigned char ucSlavesNum;
}MMC_DOWNLOADFOE_IN;

### PDF page 975
<a id="pdf-page-975"></a>
Parameters
pwSlaveId[NC_NODES_SING_AXIS_NUM]
Slave ID to which the download is sent, with a limit of 3 characters with any positive
value, dependant on the array [NC_NODES_SING_AXIS_NUM], the number of single
axis nodes.
pcFileName[256]
Full location and filename to be downloaded to the Maestro from the host, with a limit
of 256 characters
pucServer[4]
Serverhost IP with a limit of four characters (32 bit)
ucSlavesNum
Number of slaves to download the files with a maximum of 76 slaves.
MMC_DOWNLOADFOE_OUT Structure
typedef struct mmc_downloadfoe_out{
unsigned short usStatus;
unsigned short sErrorID;
}MMC_DOWNLOADFOE_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 312 describes the function for MMC_DownloadFoE.
[PDF field-code object omitted]
Figure 312: MMC_DownloadFoE function
10.2.9.1 Function Code and Implementation Example
void DownloadFoe()
{

### PDF page 976
<a id="pdf-page-976"></a>
MMC_DOWNLOADFOE_IN dlfoe ;
MMC_DOWNLOADFOE_OUT dlfoeout ;
MMC_GETFOESTATUS_OUT foestat ;
MMC_GET_GMASOP_MODE_OUT pOpmode ;

MMC_GETCOMMSTATISTICSEX_IN gcstat_In ;
MMC_GETCOMMSTATISTICSEX_OUT gcstat_Out ;
int i ;
//
//
// Before DownloadingFOE - It is good practice that drives will be
reset because
// if one of the drives is after DownloadFoE and was not reset, its
state and statitstics are unknown.
//
dlfoe.pwSlaveId[0]=0 ; // Note: Slave ID is inserted here !!
dlfoe.pwSlaveId[1]=1 ; // Note: Slave ID is inserted here !!
//
dlfoe.ucSlavesNum = 2; // Number of relevant slaves in the
pwSlaveId array.
//
// Same for slave statistics:
gcstat_In.pwSlaveId[0] = 0 ;
gcstat_In.pwSlaveId[1] = 1 ;
gcstat_In.ucSlavesNum = 2 ;
//
// Insert IP of tftp server. Usually the connection IP of the PC.
dlfoe.pucServer[0] = 10 ;
dlfoe.pucServer[1] = 10 ;
dlfoe.pucServer[2] = 20 ;
dlfoe.pucServer[3] = 55 ;
//
// Copy file path name to the structure. Should be relative to the tftp
folder
strcpy(dlfoe.pcFileName,"FoEFW 01.01.04.68 27Oct2011P01G.abs") ;

// Start tftp server on host. Only then call the MMC_DownloadFoE.
//
int rc = MMC_DownloadFoE(conn_hndl,&dlfoe,&dlfoeout) ;
if(rc < 0)
{
// Error Calling MMC_DownloadFoE. Error in dlfoeout.sErrorID
return ;
}
//
// If we reached this line, the tftp was succesful. Poll the GMAS for
results:
while(TRUE)
{
Sleep(100) ;
//
// Check the FoE progress:
MMC_GetFoEStatus(conn_hndl,&foestat);
if(rc < 0)
{
// Error Calling MMC_GetFoEStatus. Error in foestat.sErrorID

### PDF page 977
<a id="pdf-page-977"></a>
return ;
}
//
// Check that the FoE started.
if(foestat.ucFOEStarted)
{
rc = MMC_GetGMASOperationMode(conn_hndl,&pOpmode) ;
if(rc < 0)
{
// Error Calling MMC_GetGMASOperationMode. Error in
pOpmode.sErrorID
return ;
}
// Print Remaining time - foestat.ucProgress
//
// Check Foe Download progress is over and GMAS back in
operational mode.
if ((foestat.ucProgress == 0) && (pOpmode.ucResult == 0))
{
//
// Download over. Check to see if any drives failed.
for(i = 0 ; i < dlfoe.ucSlavesNum ; i++)
{
if(foestat.pstSlavesErrorID[i].sErrorID != 0)
{
// Error on one of the slaves. Print error:
// SlaveID: foestat.pstSlavesErrorID[i].usSlaveID has
error - foestat.pstSlavesErrorID[i]
}
}
// Notify user to switch drives Off / On and then check the
download status. wait 5 sec's.
//
// please note - MAX 76 slaves can be read.
rc =
MMC_GetEthercatCommStatistics(conn_hndl,&gcstat_In,&gcstat_Out) ;
if(rc < 0)
{
// Error Calling MMC_GetEthercatCommStatistics. Error in
gcstat_Out.sErrorID
return ;
}
//
// gcstat_Out.ucMasterState - Should be EcatStateO .
operational.
// Good idea to read number of slaves on bus -
gcstat_Out.usNumOfSlaves
// gcstat_Out.ucMasterDiagnosticState - All bits should be 0,
except for: EcatMasterDiagnosticStateUpdated,
EcatMasterDiagnosticStateDefaultDataWasSet bits.
//
for(i = 0 ; i < gcstat_In.ucSlavesNum ; i++)
{
// gcstat_Out.pucAxesState[i] - Should be EcatStateO.
// gcstat_Out.pucAxesDiagnosticState[i] - Should be 0.
if((gcstat_Out.pstSII_Content[i].ulVendorId == 0x9A) &&
(gcstat_Out.pstSII_Content[i].ulRevisionNo <= 0xFF))

### PDF page 978
<a id="pdf-page-978"></a>
{
//
// Drive stuck in no firmware state. Notify User. In
This case the InitCmdFail in diagnostics is not relevant.
}
}
return ;
}
}
}
}

int OnConnectGetDiagnostics()
{
int rc ;
MMC_GET_GMASOP_MODE_OUT pOpmode ;

MMC_GETCOMMSTATISTICSEX_IN gcstat_In ;
MMC_GETCOMMSTATISTICSEX_OUT gcstat_Out ;
int i ;
//
rc = MMC_GetGMASOperationMode(conn_hndl,&pOpmode) ;
if(rc < 0)
{
// Error Calling MMC_GetGMASOperationMode. Error in pOpmode.sErrorID
return ;
}
//
// Check GMAS Operational state. If == 2, then in Download FOE state.
if (pOpmode.ucResult == 2)
{
// GMAS in Download FoE state. We decided that a mesage will be
shown to user that the GMAS is in Download FoE.
}

rc = MMC_GetEthercatCommStatistics(conn_hndl,&gcstat_In,&gcstat_Out) ;
if(rc < 0)
{
// Error Calling MMC_GetEthercatCommStatistics. Error in
gcstat_Out.sErrorID
return ;
}
//
// gcstat_Out.ucMasterState - Should be EcatStateO. operational.
// Good idea to read number of slaves on bus -
gcstat_Out.usNumOfSlaves. Should be identical to number of drives
configured.
// gcstat_Out.ucMasterDiagnosticState - All bits should be 0, except
for: EcatMasterDiagnosticStateUpdated,
EcatMasterDiagnosticStateDefaultDataWasSet bits.
//
for(i = 0 ; i < gcstat_In.ucSlavesNum ; i++)
{
// gcstat_Out.pucAxesState[i] - Should be EcatStateO.
//

### PDF page 979
<a id="pdf-page-979"></a>
if((gcstat_Out.pstSII_Content[i].ulVendorId == 0x9A) &&
(gcstat_Out.pstSII_Content[i].ulRevisionNo <= 0xFF))
{
//
// One of the Drives is stuck in no firmware state. Notify User
to go to diagnostics tab - NOT TO CONFIGURATOR.
// In this case - the gcstat_Out.pucAxesDiagnosticState[i]
InitCmd bit may be set..
}
else
{
// pucAxesDiagnosticState[i] should be 0 !
}

}
}

### PDF page 980
<a id="pdf-page-980"></a>
###### 10.2.10 MMC_Exit
10.2.10 MMC_Exit
Changes the Maestro from configuration mode back to regular mode.
MMC_LIB_API int MMC_ExitCmd
(IN MMC_CONNECT_HNDL hConn,
IN MMC_EXIT_IN* pInParam,
OUT MMC_EXIT_OUT* pOutParam
);
Motion Mode NC - Not relevant Distributed - not relevant
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_EXIT input data structure using the MMC_Exit function.
pOutParam
Points to the MMC_EXIT_OUT output structure receiving information, as a result of
calling the MMC_Exit function.
Remarks
There are two Maestro operational modes:
- Normal
- Configuration
When any communication configuration parameters (network IP etc.) are changed using the Set command,
this function is invoked to exit the configuration mode to the normal operational mode of the Maestro.
Scope
All
MMC_EXIT_IN Structure
typedef struct{
unsigned char dummy;
}MMC_EXIT_IN;
Parameters

### PDF page 981
<a id="pdf-page-981"></a>
dummy
Dummy character. Any positive value accepted.
MMC_EXIT_OUT Structure
typedef struct{
unsigned short usStatus;
short sErrorID;
}MMC_EXIT_OUT;
Parameters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 313 describes the function block for MMC_Exit
[PDF field-code object omitted]
Figure 313: MMC_Exit function block
10.2.10.1 Function Block Code Example
int rc;
MMC_EXIT_IN stExit_in;
MMC_EXIT_OUT stExit_out;
//
// Inserting the structure parameters:
stExit_in.dummy = 1; //Function block handle
//
rc = MMC_ExitCmd (hConn, &stExit_in, &stExit_out);
if (rc != 0)
{
HandleError();
}

### PDF page 982
<a id="pdf-page-982"></a>
###### 10.2.11 MMC_FreeFbStat
10.2.11 MMC_FreeFbStat
Returns debug information that contains the number of free function blocks in the system.
MMC_LIB_API int MMC_FreeFbStatCmd(
IN MMC_CONNECT_HNDL hConn,
IN MMC_FREEFBSTAT_IN* pInParam,
OUT MMC_FREEFBSTAT_OUT* pOutParam
);
Motion Mode NC - Not relevant Distributed - Not relevant
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_FREEFBSTAT input data structure using the MMC_FreeFbStat
function.
pOutParam
Points to the MMC_FREEFBSTAT_OUT output structure receiving information, as a
result of calling the MMC_FreeFbStat function.
Remarks
None
Scope
All
MMC_FREEFBSTAT_IN Structure
typedef struct
{
unsigned int uiHndl;
}MMC_FREEFBSTAT_IN;
Parameters
uiHndl

### PDF page 983
<a id="pdf-page-983"></a>
Returned function block handle. Integer with any positive value
MMC_FREEFBSTAT_OUT Structure
typedef struct
{
unsigned int uiFreeLargeFb;
unsigned int uiFreeMediumFb;
unsigned int uiFreeSmallFb;
unsigned short usStatus;
short usErrorID;
}MMC_FREEFBSTAT_OUT;
Parameters
uiFreeLargeFb
Number of free large size function blocks. Any positive integer value.
uiFreeMediumFb
Number of free medium size function blocks. Any positive integer value.
uiFreeSmallFb
Number of free small size function blocks. Any positive integer value
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 314 describes the function block for MMC_FreeFbStat
[PDF field-code object omitted]
Figure 314: MMC_FreeFbStat function block
10.2.11.1 Function Block Code Example
int rc;
MMC_FREEFBSTAT_IN stFreeFBStat_in;
MMC_FREEFBSTAT_OUT stFreeFBStat_out;
//
// Inserting the structure parameters:

### PDF page 984
<a id="pdf-page-984"></a>
stFreeFBStat_in.uiHndl = 10; // Requested function block handle
//
rc = MMC_FreeFbStatCmd (hConn, &stFreeFBStat_in, &stFreeFBStat_out);
if (rc != 0)
{
HandleError();
}

### PDF page 985
<a id="pdf-page-985"></a>
###### 10.2.12 MMC_GetActiveVectorsNum
10.2.12 MMC_GetActiveVectorsNum
Displays the number of active vectors (groups) attached and managed by the Maestro.
MMC_LIB_API int MMC_GetActiveVectorsNum(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETACTIVEVECTORSNUM_IN* pInParam,
OUT MMC_GETACTIVEVECTORSNUM_OUT* pOutParam
);
Motion Mode NC - Not Supported Distributed - Supported
Source GMAS\includes\MMC_general_API.h
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
hAxisRef
Axis/group reference handle type returned by the GetAxisRef command
pInParam
Points to the MMC_GETACTIVEVECTORSNUM input data structure using the
MMC_GetActiveVectorsNum function.
pOutParam
Points to the MMC_GETACTIVEVECTORSNUM_OUT output structure receiving
information as a result of calling the MMC_GetActiveVectorsNum function.
Remarks
This function will provide this basic information without opening the Maestro Personality file.
Scope
All
MMC_GETACTIVEVECTORSNUM_IN Structure
typedef struct {
unsigned char dummy;
}MMC_GETACTIVEVECTORSNUM_IN;
Parameters

### PDF page 986
<a id="pdf-page-986"></a>
dummy
Any dummy values
MMC_GETACTIVEVECTORSNUM_OUT Structure
typedef struct {
int iActiveVectorsNum;
unsigned short usStatus;
short sErrorID;
}MMC_GETACTIVEVECTORSNUM_OUT;
Parameters
iActiveVectorsNu
m
Provides the actives vectors in a group. positive integer value.

usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections 4.3 Maestro Error IDs, and NC Profiler
Error IDs. Displays an error code as negative or positive integers.
[missing source cross-reference] describes the function block for MMC_GetActiveVectorsNum
[PDF field-code object omitted]
Figure 315: MMC_GetActiveVectorsNum function block

### PDF page 987
<a id="pdf-page-987"></a>
###### 10.2.13 MMC_GetErrorCodeDescriptionByID
10.2.13 MMC_GetErrorCodeDescriptionByID
This function receives an error\warning code and returns the description and resolution from the
Personality file.
MMC_LIB_API int MMC_GetErrorCodeDescriptionByID(
IN MMC_CONNECT_HNDL hConn,
IN MMC_GETERRORCODEDESCRIPTIONBYID_IN* pInParam,
OUT MMC_GETERRORCODEDESCRIPTIONBYID_OUT* pOutParam
);
Motion Mode NC - Supported Distributed - Supported
Source GMAS\includes\MMC_general_API.h
GMAS Programming(IEC 61331 Program)\ElmoGlobal
Parameters
hConn
[IN] Connection handle input using hConn, where MMC_CONNECT_HNDL is the
Connection Handle Type. It should be noted that this connection handle is common
throughout all Maestro functions. This connection handle is returned by the Init
Connection command. If an error occurs, the function returns -1 and a MMC_LIB_API
error with more details.
pInParam
Points to the MMC_GETERRORCODEDESCRIPTIONBYID input data structure using the
MMC_GetErrorCodeDescriptionByID function. The input [IN] receives the error \warning
number.
pOutParam
Points to the MMC_GETERRORCODEDESCRIPTIONBYID_OUT output structure receiving
information, as a result of calling the MMC_GetErrorCodeDescriptionByID function. The
output [OUT] returns two strings, status and error id.
Remarks
None
Scope
Errors from the Maestro Personality file.
MMC_GETERRORCODEDESCRIPTIONBYID_IN Structure
typedef struct mmc_getcodedescriptionbyid_in{
int iCode;
Char cType;
} MMC_GETERRORCODEDESCRIPTIONBYID_IN;
Parameters

### PDF page 988
<a id="pdf-page-988"></a>
iCode
Error and or warning code value. Any integer value which may be positive or negative
cType
The code type, which may be one of the following:
1 - GMAS code
2 - Drive emergency code
3 - Drive Abortion Code
MMC_GETERRORCODEDESCRIPTIONBYID_OUT Structure
typedef struct mmc_getcodedescriptionbyid_out{
char pResolution[1100];
char pDescription[256];
unsigned short usStatus;
short sErrorID;
} MMC_GETERRORCODEDESCRIPTIONBYID_OUT;
Parameters
pResolution[1100]
Character value of the resolution with a maximum of 1100 characters
pDescription[256]
Character value of the description with a maximum value of 256 characters
usStatus
Bitwise returned command status with the following values:
Aborted
Done
CommandError
sErrorID
Returned command error ID. Signals where an error has occurred within the function
block. Refer to the errors listed in sections Maestro Error IDs, and NC Profiler Error IDs.
Figure 316 describes the function for MMC_GetErrorCodeDescriptionByID as applied within the IEC 61131
programming for MC_GetAxisRef.

### PDF page 989
<a id="pdf-page-989"></a>
[PDF field-code object omitted]
Figure 316: MMC_GetErrorCodeDescriptionByID function
